using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Products.Functions.Common;
using Products.Functions.Products;

namespace Products.UnitTests;

public class ProductServiceTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_repository, new ProductRequestValidator(), new ProductQueryValidator());
    }

    [Fact]
    public async Task Create_inserts_valid_product()
    {
        var product = TestData.Product();
        _repository.InsertAsync(TestData.CompanyId, Arg.Any<ProductRequest>(), Arg.Any<CancellationToken>()).Returns(product);

        var result = await _service.CreateAsync(TestData.CompanyId, TestData.Request(), CancellationToken.None);

        Assert.Same(product, result.Value);
    }

    [Fact]
    public async Task Create_does_not_hit_the_database_when_invalid()
    {
        var result = await _service.CreateAsync(TestData.CompanyId, TestData.Request(price: -1), CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        await _repository.DidNotReceiveWithAnyArgs().InsertAsync(default, default!, default);
    }

    [Fact]
    public async Task Create_returns_conflict_on_duplicate_sku()
    {
        _repository.InsertAsync(TestData.CompanyId, Arg.Any<ProductRequest>(), Arg.Any<CancellationToken>())
            .Throws(new DuplicateSkuException("CAFE-250"));

        var result = await _service.CreateAsync(TestData.CompanyId, TestData.Request(), CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Get_returns_not_found_for_missing_product()
    {
        var result = await _service.GetAsync(TestData.CompanyId, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Update_returns_not_found_for_missing_product()
    {
        var result = await _service.UpdateAsync(TestData.CompanyId, Guid.NewGuid(), TestData.Request(), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Update_returns_conflict_on_duplicate_sku()
    {
        var id = Guid.NewGuid();
        _repository.UpdateAsync(TestData.CompanyId, id, Arg.Any<ProductRequest>(), Arg.Any<CancellationToken>())
            .Throws(new DuplicateSkuException("CAFE-250"));

        var result = await _service.UpdateAsync(TestData.CompanyId, id, TestData.Request(), CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Delete_returns_not_found_when_nothing_was_deleted()
    {
        var result = await _service.DeleteAsync(TestData.CompanyId, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task List_rejects_page_size_over_limit()
    {
        var result = await _service.ListAsync(TestData.CompanyId, new ProductQuery(null, 1, 500), CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task List_returns_page_from_repository()
    {
        var page = new PagedResult<Product>([TestData.Product()], 1, 20, 1);
        _repository.ListAsync(TestData.CompanyId, TestData.StoreId, 1, 20, Arg.Any<CancellationToken>()).Returns(page);

        var result = await _service.ListAsync(TestData.CompanyId, new ProductQuery(TestData.StoreId, 1, 20), CancellationToken.None);

        Assert.Same(page, result.Value);
    }
}
