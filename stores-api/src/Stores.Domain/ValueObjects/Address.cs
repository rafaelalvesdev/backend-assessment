using Stores.Domain.Common;

namespace Stores.Domain.ValueObjects;

public sealed record Address
{
    public const int StreetMaxLength = 200;
    public const int CityMaxLength = 100;
    public const int StateMaxLength = 100;
    public const int PostalCodeMaxLength = 20;

    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string? PostalCode { get; }
    public CountryCode Country { get; }

    public Address(string street, string city, string? state, string? postalCode, CountryCode country)
    {
        Street = Guard.Required(street, "Street", StreetMaxLength);
        City = Guard.Required(city, "City", CityMaxLength);
        State = Guard.Optional(state, "State", StateMaxLength);
        PostalCode = Guard.Optional(postalCode, "Postal code", PostalCodeMaxLength);
        Country = country ?? throw new DomainException("Country is required.");
    }
}
