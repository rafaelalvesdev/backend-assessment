using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Products.Functions.Common;
using Products.Functions.Functions;
using Products.Functions.Products;

namespace Products.UnitTests;

public class ProductFunctionsTests
{
    private readonly IProductService _service = Substitute.For<IProductService>();
    private readonly ProductFunctions _functions;

    public ProductFunctionsTests()
    {
        _functions = new ProductFunctions(_service);
    }

    private static HttpRequest Request(string? body = null, string query = "")
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("products.test");
        context.Request.Path = $"/api/companies/{TestData.CompanyId}/products";
        context.Request.QueryString = new QueryString(query);
        if (body is not null)
        {
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        }
        return context.Request;
    }

    [Fact]
    public async Task Create_returns_201_with_location()
    {
        var product = TestData.Product();
        _service.CreateAsync(TestData.CompanyId, Arg.Any<ProductRequest>(), Arg.Any<CancellationToken>()).Returns(product);
        var body = $$"""{"storeId":"{{TestData.StoreId}}","sku":"CAFE-250","name":"Café","price":10.5,"currency":"BRL","stockQuantity":1}""";

        var result = await _functions.Create(Request(body), TestData.CompanyId, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"https://products.test/api/companies/{TestData.CompanyId}/products/{product.Id}", created.Location);
        Assert.Same(product, created.Value);
        await _service.Received().CreateAsync(
            TestData.CompanyId,
            Arg.Is<ProductRequest>(r => r.Sku == "CAFE-250" && r.Price == 10.5m && r.IsActive),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_without_json_content_type_returns_415()
    {
        var request = Request("{}");
        request.ContentType = "text/plain";

        var result = await _functions.Create(request, TestData.CompanyId, CancellationToken.None);

        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }

    [Fact]
    public async Task Create_with_malformed_json_returns_400()
    {
        var result = await _functions.Create(Request("{ nope"), TestData.CompanyId, CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task Validation_errors_use_camel_case_field_names()
    {
        var errors = new Dictionary<string, string[]> { ["StockQuantity"] = ["must be positive"] };
        _service.CreateAsync(TestData.CompanyId, Arg.Any<ProductRequest>(), Arg.Any<CancellationToken>())
            .Returns(Error.Validation(errors));

        var result = await _functions.Create(Request("{}"), TestData.CompanyId, CancellationToken.None);

        var problem = Assert.IsType<ValidationProblemDetails>(Assert.IsType<ObjectResult>(result).Value);
        Assert.True(problem.Errors.ContainsKey("stockQuantity"));
    }

    [Fact]
    public async Task Get_maps_not_found_to_404()
    {
        var id = Guid.NewGuid();
        _service.GetAsync(TestData.CompanyId, id, Arg.Any<CancellationToken>()).Returns(Error.NotFound("missing"));

        var result = await _functions.Get(Request(), TestData.CompanyId, id, CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task Update_maps_conflict_to_409()
    {
        var id = Guid.NewGuid();
        _service.UpdateAsync(TestData.CompanyId, id, Arg.Any<ProductRequest>(), Arg.Any<CancellationToken>())
            .Returns(Error.Conflict("dup"));

        var result = await _functions.Update(Request("{}"), TestData.CompanyId, id, CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task Delete_returns_204()
    {
        var id = Guid.NewGuid();
        _service.DeleteAsync(TestData.CompanyId, id, Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await _functions.Delete(Request(), TestData.CompanyId, id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task List_passes_query_string_to_service()
    {
        var page = new PagedResult<Product>([], 2, 5, 0);
        _service.ListAsync(TestData.CompanyId, new ProductQuery(TestData.StoreId, 2, 5), Arg.Any<CancellationToken>()).Returns(page);

        var result = await _functions.List(Request(query: $"?storeId={TestData.StoreId}&page=2&pageSize=5"), TestData.CompanyId, CancellationToken.None);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result).Value);
    }

    [Theory]
    [InlineData("?page=abc")]
    [InlineData("?storeId=not-a-guid")]
    public async Task List_rejects_unparseable_query_values(string query)
    {
        var result = await _functions.List(Request(query: query), TestData.CompanyId, CancellationToken.None);

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task Catalog_returns_raw_json_from_database()
    {
        const string json = """{"companyId":"x","totalCount":0,"products":[]}""";
        _service.GetCatalogJsonAsync(TestData.CompanyId, null, Arg.Any<CancellationToken>()).Returns(json);

        var result = await _functions.Catalog(Request(), TestData.CompanyId, CancellationToken.None);

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", content.ContentType);
        Assert.Equal(json, content.Content);
    }
}
