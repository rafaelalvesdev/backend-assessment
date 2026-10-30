using Stores.Application.Companies.CreateCompany;

namespace Stores.Api.Contracts;

public sealed record CompanyRequest(string Name, string CountryCode)
{
    public CreateCompanyCommand ToCommand() => new(Name, CountryCode);
}
