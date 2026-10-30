using Microsoft.EntityFrameworkCore;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Domain.Stores;
using Stores.Domain.ValueObjects;

namespace Stores.Infrastructure.Persistence.Repositories;

internal sealed class StoreRepository(StoresDbContext db) : IStoreRepository
{
    public Task<Store?> GetAsync(Guid companyId, Guid storeId, CancellationToken cancellationToken) =>
        db.Stores.FirstOrDefaultAsync(s => s.CompanyId == companyId && s.Id == storeId, cancellationToken);

    public Task<PagedResult<Store>> ListAsync(
        Guid companyId,
        string? countryCode,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = db.Stores.AsNoTracking().Where(s => s.CompanyId == companyId);

        if (countryCode is not null)
        {
            var country = CountryCode.From(countryCode);
            query = query.Where(s => s.Address.Country == country);
        }

        return query.OrderBy(s => s.Code).ToPagedAsync(page, pageSize, cancellationToken);
    }

    public Task<bool> AnyForCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
        db.Stores.AnyAsync(s => s.CompanyId == companyId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, string code, Guid? exceptStoreId, CancellationToken cancellationToken) =>
        db.Stores.AnyAsync(
            s => s.CompanyId == companyId && s.Code == code && (exceptStoreId == null || s.Id != exceptStoreId),
            cancellationToken);

    public void Add(Store store) => db.Stores.Add(store);

    public void Remove(Store store) => db.Stores.Remove(store);
}
