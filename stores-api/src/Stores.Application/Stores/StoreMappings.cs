using Stores.Domain.Stores;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Stores;

internal static class StoreMappings
{
    public static StoreResponse ToResponse(this Store store) => new(
        store.Id,
        store.CompanyId,
        store.Code,
        store.Name,
        new AddressResponse(
            store.Address.Street,
            store.Address.City,
            store.Address.State,
            store.Address.PostalCode,
            store.Address.Country.Value),
        store.Currency.Value,
        store.TimeZone.Value,
        store.Culture.Value,
        store.CreatedAt,
        store.UpdatedAt);

    public static Address ToAddress(this AddressInput input) =>
        new(input.Street, input.City, input.State, input.PostalCode, CountryCode.From(input.CountryCode));

    public static string NormalizedCode(this IStoreDetails details) => details.Code.Trim().ToUpperInvariant();
}
