using Stores.Application.Common;

namespace Stores.Application.Companies.ListCompanies;

public sealed record ListCompaniesQuery(int Page = 1, int PageSize = Paging.DefaultPageSize);
