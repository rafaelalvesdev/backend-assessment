using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Domain.Companies;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Companies.CreateCompany;

public sealed class CreateCompanyHandler(
    ICompanyRepository companies,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IHandler<CreateCompanyCommand, Result<CompanyResponse>>
{
    public async Task<Result<CompanyResponse>> HandleAsync(CreateCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = Company.Create(command.Name, CountryCode.From(command.CountryCode), clock.GetUtcNow());

        companies.Add(company);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CompanyResponse.From(company);
    }
}
