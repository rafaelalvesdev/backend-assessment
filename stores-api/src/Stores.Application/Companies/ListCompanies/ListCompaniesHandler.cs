using Stores.Application.Abstractions;
using Stores.Application.Common;

namespace Stores.Application.Companies.ListCompanies;

public sealed class ListCompaniesHandler(ICompanyRepository companies)
    : IHandler<ListCompaniesQuery, Result<PagedResult<CompanyResponse>>>
{
    public async Task<Result<PagedResult<CompanyResponse>>> HandleAsync(ListCompaniesQuery query, CancellationToken cancellationToken)
    {
        var page = await companies.ListAsync(query.Page, query.PageSize, cancellationToken);

        return page.Map(CompanyResponse.From);
    }
}
