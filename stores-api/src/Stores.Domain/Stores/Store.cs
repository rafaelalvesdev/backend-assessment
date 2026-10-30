using System.Text.RegularExpressions;
using Stores.Domain.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Stores;

public sealed partial class Store
{
    public const int CodeMaxLength = 20;
    public const int NameMaxLength = 150;

    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public CurrencyCode Currency { get; private set; } = null!;
    public TimeZoneId TimeZone { get; private set; } = null!;
    public CultureCode Culture { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Store() { }

    public static Store Create(
        Guid companyId,
        string code,
        string name,
        Address address,
        CurrencyCode currency,
        TimeZoneId timeZone,
        CultureCode culture,
        DateTimeOffset now)
    {
        if (companyId == Guid.Empty)
            throw new DomainException("A store must belong to a company.");

        var store = new Store { Id = Guid.NewGuid(), CompanyId = companyId, CreatedAt = now };
        store.Update(code, name, address, currency, timeZone, culture, now);
        return store;
    }

    public void Update(
        string code,
        string name,
        Address address,
        CurrencyCode currency,
        TimeZoneId timeZone,
        CultureCode culture,
        DateTimeOffset now)
    {
        Code = NormalizeCode(code);
        Name = Guard.Required(name, "Name", NameMaxLength);
        Address = address ?? throw new DomainException("Address is required.");
        Currency = currency ?? throw new DomainException("Currency is required.");
        TimeZone = timeZone ?? throw new DomainException("Time zone is required.");
        Culture = culture ?? throw new DomainException("Culture is required.");
        UpdatedAt = now;
    }

    public static bool IsValidCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && code.Trim().Length <= CodeMaxLength && CodePattern().IsMatch(code.Trim());

    private static string NormalizeCode(string code)
    {
        if (!IsValidCode(code))
            throw new DomainException("Code must be 1-20 characters of letters, digits, '-' or '_'.");

        return code.Trim().ToUpperInvariant();
    }

    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex CodePattern();
}
