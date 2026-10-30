using Stores.Domain.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Tests.ValueObjects;

public class CultureCodeTests
{
    [Theory]
    [InlineData("pt-BR", "pt-BR")]
    [InlineData("en-us", "en-US")]
    [InlineData("ja-JP", "ja-JP")]
    public void From_returns_canonical_name(string input, string expected)
    {
        Assert.Equal(expected, CultureCode.From(input).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("klingon")]
    [InlineData("xx-YY")]
    public void From_rejects_unknown_cultures(string input)
    {
        Assert.False(CultureCode.IsValid(input));
        Assert.Throws<DomainException>(() => CultureCode.From(input));
    }
}
