using System.Globalization;
using Stores.Domain.Common;

namespace Stores.Domain.ValueObjects;

public sealed record CurrencyCode
{
    private static readonly HashSet<string> Known = CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(c => new RegionInfo(c.Name).ISOCurrencySymbol)
        .Where(code => code.Length == 3)
        .ToHashSet();

    public string Value { get; }

    private CurrencyCode(string value) => Value = value;

    public static bool IsValid(string? value) =>
        value is not null && Known.Contains(value.Trim().ToUpperInvariant());

    public static CurrencyCode From(string? value)
    {
        if (!IsValid(value))
            throw new DomainException($"'{value}' is not a valid ISO 4217 currency code.");

        return new CurrencyCode(value!.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}
