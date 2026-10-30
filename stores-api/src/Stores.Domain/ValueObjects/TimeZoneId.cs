using Stores.Domain.Common;

namespace Stores.Domain.ValueObjects;

public sealed record TimeZoneId
{
    public string Value { get; }

    private TimeZoneId(string value) => Value = value;

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && TimeZoneInfo.TryFindSystemTimeZoneById(value.Trim(), out _);

    public static TimeZoneId From(string? value)
    {
        if (!IsValid(value))
            throw new DomainException($"'{value}' is not a known IANA time zone.");

        return new TimeZoneId(value!.Trim());
    }

    public override string ToString() => Value;
}
