namespace Stores.Application.Stores;

public sealed record StoreResponse(
    Guid Id,
    Guid CompanyId,
    string Code,
    string Name,
    AddressResponse Address,
    string Currency,
    string TimeZone,
    string Culture,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AddressResponse(string Street, string City, string? State, string? PostalCode, string CountryCode);
