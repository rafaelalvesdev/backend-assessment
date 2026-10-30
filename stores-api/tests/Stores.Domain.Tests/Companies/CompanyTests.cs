using Stores.Domain.Common;
using Stores.Domain.Companies;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Tests.Companies;

public class CompanyTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_sets_properties()
    {
        var company = Company.Create("  Acme Retail  ", CountryCode.From("BR"), Now);

        Assert.NotEqual(Guid.Empty, company.Id);
        Assert.Equal("Acme Retail", company.Name);
        Assert.Equal("BR", company.Country.Value);
        Assert.Equal(Now, company.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_requires_a_name(string name)
    {
        Assert.Throws<DomainException>(() => Company.Create(name, CountryCode.From("BR"), Now));
    }

    [Fact]
    public void Create_rejects_names_longer_than_the_limit()
    {
        var name = new string('a', Company.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => Company.Create(name, CountryCode.From("BR"), Now));
    }
}
