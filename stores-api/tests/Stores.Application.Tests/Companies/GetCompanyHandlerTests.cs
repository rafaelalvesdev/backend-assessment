using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Companies.GetCompany;

namespace Stores.Application.Tests.Companies;

public class GetCompanyHandlerTests
{
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();

    [Fact]
    public async Task Returns_company()
    {
        var company = TestData.Company();
        _companies.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

        var result = await new GetCompanyHandler(_companies).HandleAsync(new GetCompanyQuery(company.Id), CancellationToken.None);

        Assert.Equal(company.Id, result.Value.Id);
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_company()
    {
        var result = await new GetCompanyHandler(_companies).HandleAsync(new GetCompanyQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
