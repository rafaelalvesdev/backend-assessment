using System.Globalization;

namespace Products.Functions.Products;

internal static class CurrencyCodes
{
    private static readonly HashSet<string> Known = CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(c => new RegionInfo(c.Name).ISOCurrencySymbol)
        .Where(code => code.Length == 3)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsValid(string? code) => code is not null && Known.Contains(code);
}
