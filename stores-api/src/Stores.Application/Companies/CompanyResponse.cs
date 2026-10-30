using Stores.Domain.Companies;

namespace Stores.Application.Companies;

public sealed record CompanyResponse(Guid Id, string Name, string CountryCode, DateTimeOffset CreatedAt)
{
    public static CompanyResponse From(Company company) =>
        new(company.Id, company.Name, company.Country.Value, company.CreatedAt);
}
