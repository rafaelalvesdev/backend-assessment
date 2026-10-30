using Stores.Application.Abstractions;
using Stores.Application.Common;

namespace Stores.Application.Companies.GetCompany;

public sealed class GetCompanyHandler(ICompanyRepository companies) : IHandler<GetCompanyQuery, Result<CompanyResponse>>
{
    public async Task<Result<CompanyResponse>> HandleAsync(GetCompanyQuery query, CancellationToken cancellationToken)
    {
        var company = await companies.GetByIdAsync(query.CompanyId, cancellationToken);

        return company is null ? CompanyErrors.NotFound(query.CompanyId) : CompanyResponse.From(company);
    }
}
