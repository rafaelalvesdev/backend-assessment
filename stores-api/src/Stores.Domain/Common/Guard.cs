namespace Stores.Domain.Common;

internal static class Guard
{
    public static string Required(string? value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} is required.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException($"{name} must be at most {maxLength} characters.");

        return trimmed;
    }

    public static string? Optional(string? value, string name, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, name, maxLength);
}
