namespace Stores.Application.Stores;

public sealed record AddressInput(string Street, string City, string? State, string? PostalCode, string CountryCode);
