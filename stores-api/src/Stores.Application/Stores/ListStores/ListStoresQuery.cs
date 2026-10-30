using Stores.Application.Common;

namespace Stores.Application.Stores.ListStores;

public sealed record ListStoresQuery(
    Guid CompanyId,
    string? CountryCode = null,
    int Page = 1,
    int PageSize = Paging.DefaultPageSize);
