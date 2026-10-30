using System.Globalization;
using Stores.Domain.Common;

namespace Stores.Domain.ValueObjects;

public sealed record CultureCode
{
    public string Value { get; }

    private CultureCode(string value) => Value = value;

    public static bool IsValid(string? value) => TryResolve(value, out _);

    public static CultureCode From(string? value)
    {
        if (!TryResolve(value, out var culture))
            throw new DomainException($"'{value}' is not a known culture.");

        return new CultureCode(culture.Name);
    }

    private static bool TryResolve(string? value, out CultureInfo culture)
    {
        culture = CultureInfo.InvariantCulture;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            culture = CultureInfo.GetCultureInfo(value.Trim(), predefinedOnly: true);
            return !culture.IsNeutralCulture;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
