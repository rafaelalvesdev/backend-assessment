using Stores.Application.Common;
using Stores.Domain.Stores;

namespace Stores.Application.Abstractions;

public interface IStoreRepository
{
    Task<Store?> GetAsync(Guid companyId, Guid storeId, CancellationToken cancellationToken);
    Task<PagedResult<Store>> ListAsync(Guid companyId, string? countryCode, int page, int pageSize, CancellationToken cancellationToken);
    Task<bool> AnyForCompanyAsync(Guid companyId, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(Guid companyId, string code, Guid? exceptStoreId, CancellationToken cancellationToken);
    void Add(Store store);
    void Remove(Store store);
}
