using System.Globalization;
using Stores.Domain.Common;

namespace Stores.Domain.ValueObjects;

public sealed record CountryCode
{
    private static readonly HashSet<string> Known = CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(c => new RegionInfo(c.Name).TwoLetterISORegionName)
        .Where(code => code.Length == 2 && code.All(char.IsAsciiLetterUpper))
        .ToHashSet();

    public string Value { get; }

    private CountryCode(string value) => Value = value;

    public static bool IsValid(string? value) =>
        value is not null && Known.Contains(value.Trim().ToUpperInvariant());

    public static CountryCode From(string? value)
    {
        if (!IsValid(value))
            throw new DomainException($"'{value}' is not a valid ISO 3166-1 alpha-2 country code.");

        return new CountryCode(value!.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}
