using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Products.Functions.Products;

namespace Products.Functions.Functions;

public sealed class ProductFunctions(IProductService products)
{
    private const string Collection = "companies/{companyId:guid}/products";
    private const string Item = Collection + "/{productId:guid}";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function("ListProducts")]
    public async Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = Collection)] HttpRequest request,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var storeId = QueryGuid(request, "storeId", errors);
        var page = QueryInt(request, "page", 1, errors);
        var pageSize = QueryInt(request, "pageSize", ProductQuery.DefaultPageSize, errors);
        if (errors.Count > 0)
            return ProblemResults.Validation(errors);

        var result = await products.ListAsync(companyId, new ProductQuery(storeId, page, pageSize), cancellationToken);

        return result.IsSuccess ? new OkObjectResult(result.Value) : ProblemResults.From(result.Error);
    }

    [Function("GetProductCatalog")]
    public async Task<IActionResult> Catalog(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = Collection + "/catalog")] HttpRequest request,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var storeId = QueryGuid(request, "storeId", errors);
        if (errors.Count > 0)
            return ProblemResults.Validation(errors);

        var json = await products.GetCatalogJsonAsync(companyId, storeId, cancellationToken);

        return new ContentResult { Content = json, ContentType = "application/json", StatusCode = StatusCodes.Status200OK };
    }

    [Function("GetProduct")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = Item)] HttpRequest request,
        Guid companyId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await products.GetAsync(companyId, productId, cancellationToken);

        return result.IsSuccess ? new OkObjectResult(result.Value) : ProblemResults.From(result.Error);
    }

    [Function("CreateProduct")]
    public async Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = Collection)] HttpRequest request,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var (body, error) = await ReadBodyAsync(request, cancellationToken);
        if (error is not null)
            return error;

        var result = await products.CreateAsync(companyId, body!, cancellationToken);
        if (!result.IsSuccess)
            return ProblemResults.From(result.Error);

        var location = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path.Value!.TrimEnd('/')}/{result.Value.Id}";
        return new CreatedResult(location, result.Value);
    }

    [Function("UpdateProduct")]
    public async Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = Item)] HttpRequest request,
        Guid companyId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var (body, error) = await ReadBodyAsync(request, cancellationToken);
        if (error is not null)
            return error;

        var result = await products.UpdateAsync(companyId, productId, body!, cancellationToken);

        return result.IsSuccess ? new OkObjectResult(result.Value) : ProblemResults.From(result.Error);
    }

    [Function("DeleteProduct")]
    public async Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Item)] HttpRequest request,
        Guid companyId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await products.DeleteAsync(companyId, productId, cancellationToken);

        return result.IsSuccess ? new NoContentResult() : ProblemResults.From(result.Error);
    }

    private static async Task<(ProductRequest? Body, IActionResult? Error)> ReadBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (!request.HasJsonContentType())
            return (null, new UnsupportedMediaTypeResult());

        try
        {
            var body = await request.ReadFromJsonAsync<ProductRequest>(JsonOptions, cancellationToken);
            return body is null ? (null, ProblemResults.BadRequest("Request body is required.")) : (body, null);
        }
        catch (JsonException)
        {
            return (null, ProblemResults.BadRequest("Request body is not valid JSON."));
        }
    }

    private static Guid? QueryGuid(HttpRequest request, string name, Dictionary<string, string[]> errors)
    {
        string? value = request.Query[name];
        if (string.IsNullOrEmpty(value))
            return null;
        if (Guid.TryParse(value, out var id))
            return id;

        errors[name] = ["Must be a GUID."];
        return null;
    }

    private static int QueryInt(HttpRequest request, string name, int fallback, Dictionary<string, string[]> errors)
    {
        string? value = request.Query[name];
        if (string.IsNullOrEmpty(value))
            return fallback;
        if (int.TryParse(value, out var number))
            return number;

        errors[name] = ["Must be an integer."];
        return fallback;
    }
}
