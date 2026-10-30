using Products.Functions.Common;

namespace Products.Functions.Products;

public interface IProductRepository
{
    Task<Product?> GetAsync(Guid companyId, Guid productId, CancellationToken cancellationToken);
    Task<PagedResult<Product>> ListAsync(Guid companyId, Guid? storeId, int page, int pageSize, CancellationToken cancellationToken);
    Task<string> GetCatalogJsonAsync(Guid companyId, Guid? storeId, CancellationToken cancellationToken);
    Task<Product> InsertAsync(Guid companyId, ProductRequest request, CancellationToken cancellationToken);
    Task<Product?> UpdateAsync(Guid companyId, Guid productId, ProductRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid companyId, Guid productId, CancellationToken cancellationToken);
}
