using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stores.Domain.ValueObjects;

namespace Stores.Infrastructure.Persistence.Configurations;

internal static class ValueConverters
{
    public static readonly ValueConverter<CountryCode, string> Country = new(v => v.Value, v => CountryCode.From(v));
    public static readonly ValueConverter<CurrencyCode, string> Currency = new(v => v.Value, v => CurrencyCode.From(v));
    public static readonly ValueConverter<TimeZoneId, string> TimeZone = new(v => v.Value, v => TimeZoneId.From(v));
    public static readonly ValueConverter<CultureCode, string> Culture = new(v => v.Value, v => CultureCode.From(v));
}
