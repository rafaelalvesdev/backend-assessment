using FluentValidation;
using Products.Functions.Common;

namespace Products.Functions.Products;

public sealed class ProductService(
    IProductRepository repository,
    IValidator<ProductRequest> requestValidator,
    IValidator<ProductQuery> queryValidator) : IProductService
{
    public async Task<Result<Product>> GetAsync(Guid companyId, Guid productId, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(companyId, productId, cancellationToken);

        return product is null ? NotFound(productId) : product;
    }

    public async Task<Result<PagedResult<Product>>> ListAsync(Guid companyId, ProductQuery query, CancellationToken cancellationToken)
    {
        var validation = await queryValidator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return validation.ToError();

        return await repository.ListAsync(companyId, query.StoreId, query.Page, query.PageSize, cancellationToken);
    }

    public Task<string> GetCatalogJsonAsync(Guid companyId, Guid? storeId, CancellationToken cancellationToken) =>
        repository.GetCatalogJsonAsync(companyId, storeId, cancellationToken);

    public async Task<Result<Product>> CreateAsync(Guid companyId, ProductRequest request, CancellationToken cancellationToken)
    {
        var validation = await requestValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return validation.ToError();

        try
        {
            return await repository.InsertAsync(companyId, request, cancellationToken);
        }
        catch (DuplicateSkuException ex)
        {
            return Error.Conflict(ex.Message);
        }
    }

    public async Task<Result<Product>> UpdateAsync(Guid companyId, Guid productId, ProductRequest request, CancellationToken cancellationToken)
    {
        var validation = await requestValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return validation.ToError();

        try
        {
            var product = await repository.UpdateAsync(companyId, productId, request, cancellationToken);
            return product is null ? NotFound(productId) : product;
        }
        catch (DuplicateSkuException ex)
        {
            return Error.Conflict(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid companyId, Guid productId, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(companyId, productId, cancellationToken);

        return deleted ? Result.Success() : NotFound(productId);
    }

    private static Error NotFound(Guid productId) => Error.NotFound($"Product '{productId}' was not found.");
}
