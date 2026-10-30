using Products.Functions.Common;

namespace Products.Functions.Products;

public interface IProductService
{
    Task<Result<Product>> GetAsync(Guid companyId, Guid productId, CancellationToken cancellationToken);
    Task<Result<PagedResult<Product>>> ListAsync(Guid companyId, ProductQuery query, CancellationToken cancellationToken);
    Task<string> GetCatalogJsonAsync(Guid companyId, Guid? storeId, CancellationToken cancellationToken);
    Task<Result<Product>> CreateAsync(Guid companyId, ProductRequest request, CancellationToken cancellationToken);
    Task<Result<Product>> UpdateAsync(Guid companyId, Guid productId, ProductRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid companyId, Guid productId, CancellationToken cancellationToken);
}
