using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Companies;

namespace Stores.Application.Stores.ListStores;

public sealed class ListStoresHandler(ICompanyRepository companies, IStoreRepository stores)
    : IHandler<ListStoresQuery, Result<PagedResult<StoreResponse>>>
{
    public async Task<Result<PagedResult<StoreResponse>>> HandleAsync(ListStoresQuery query, CancellationToken cancellationToken)
    {
        if (!await companies.ExistsAsync(query.CompanyId, cancellationToken))
            return CompanyErrors.NotFound(query.CompanyId);

        var country = query.CountryCode?.Trim().ToUpperInvariant();
        var page = await stores.ListAsync(query.CompanyId, country, query.Page, query.PageSize, cancellationToken);

        return page.Map(s => s.ToResponse());
    }
}
