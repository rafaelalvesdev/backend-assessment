using Stores.Application.Abstractions;
using Stores.Application.Common;

namespace Stores.Application.Stores.GetStore;

public sealed class GetStoreHandler(IStoreRepository stores) : IHandler<GetStoreQuery, Result<StoreResponse>>
{
    public async Task<Result<StoreResponse>> HandleAsync(GetStoreQuery query, CancellationToken cancellationToken)
    {
        var store = await stores.GetAsync(query.CompanyId, query.StoreId, cancellationToken);

        return store is null ? StoreErrors.NotFound(query.CompanyId, query.StoreId) : store.ToResponse();
    }
}
