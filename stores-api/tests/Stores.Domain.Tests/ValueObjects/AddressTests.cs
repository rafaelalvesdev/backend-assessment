using Stores.Domain.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Blank_optional_fields_become_null()
    {
        var address = new Address(" 1 Main St ", " Springfield ", " ", "", CountryCode.From("US"));

        Assert.Equal("1 Main St", address.Street);
        Assert.Equal("Springfield", address.City);
        Assert.Null(address.State);
        Assert.Null(address.PostalCode);
    }

    [Theory]
    [InlineData("", "Springfield")]
    [InlineData("1 Main St", "")]
    public void Street_and_city_are_required(string street, string city)
    {
        Assert.Throws<DomainException>(() => new Address(street, city, null, null, CountryCode.From("US")));
    }
}
