using Stores.Domain.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Tests.ValueObjects;

public class CurrencyCodeTests
{
    [Theory]
    [InlineData("USD", "USD")]
    [InlineData("brl", "BRL")]
    [InlineData("EUR", "EUR")]
    [InlineData("JPY", "JPY")]
    public void From_accepts_iso_4217_codes(string input, string expected)
    {
        Assert.Equal(expected, CurrencyCode.From(input).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("ABC")]
    [InlineData("DOLLAR")]
    public void From_rejects_unknown_codes(string input)
    {
        Assert.False(CurrencyCode.IsValid(input));
        Assert.Throws<DomainException>(() => CurrencyCode.From(input));
    }
}
