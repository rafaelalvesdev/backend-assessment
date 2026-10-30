using Stores.Domain.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Tests.ValueObjects;

public class CountryCodeTests
{
    [Theory]
    [InlineData("BR", "BR")]
    [InlineData("us", "US")]
    [InlineData(" pt ", "PT")]
    public void From_normalizes_valid_codes(string input, string expected)
    {
        var code = CountryCode.From(input);

        Assert.Equal(expected, code.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("B")]
    [InlineData("BRA")]
    [InlineData("12")]
    [InlineData("XX")]
    public void From_rejects_invalid_codes(string input)
    {
        Assert.False(CountryCode.IsValid(input));
        Assert.Throws<DomainException>(() => CountryCode.From(input));
    }

    [Fact]
    public void Codes_with_same_value_are_equal()
    {
        Assert.Equal(CountryCode.From("br"), CountryCode.From("BR"));
    }
}
