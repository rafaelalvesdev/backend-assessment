using Stores.Domain.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Tests.ValueObjects;

public class TimeZoneIdTests
{
    [Theory]
    [InlineData("America/Sao_Paulo")]
    [InlineData("Europe/Lisbon")]
    [InlineData("Asia/Tokyo")]
    [InlineData("UTC")]
    public void From_accepts_iana_ids(string input)
    {
        Assert.Equal(input, TimeZoneId.From(input).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Mars/Olympus_Mons")]
    [InlineData("GMT+25")]
    public void From_rejects_unknown_ids(string input)
    {
        Assert.False(TimeZoneId.IsValid(input));
        Assert.Throws<DomainException>(() => TimeZoneId.From(input));
    }
}
