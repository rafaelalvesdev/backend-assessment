using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Companies.CreateCompany;
using Stores.Domain.Companies;

namespace Stores.Application.Tests.Companies;

public class CreateCompanyHandlerTests
{
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Creates_company()
    {
        var handler = new CreateCompanyHandler(_companies, _unitOfWork, new FakeTimeProvider(TestData.Now));

        var result = await handler.HandleAsync(new CreateCompanyCommand("Acme", "pt"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Acme", result.Value.Name);
        Assert.Equal("PT", result.Value.CountryCode);
        _companies.Received(1).Add(Arg.Is<Company>(c => c.Name == "Acme"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validator_rejects_invalid_country()
    {
        var result = new CreateCompanyValidator().Validate(new CreateCompanyCommand("Acme", "Portugal"));

        Assert.Contains(result.Errors, e => e.PropertyName == "CountryCode");
    }
}
