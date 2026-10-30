namespace Stores.Application.Stores.UpdateStore;

public sealed record UpdateStoreCommand(
    Guid CompanyId,
    Guid StoreId,
    string Code,
    string Name,
    AddressInput Address,
    string Currency,
    string TimeZone,
    string Culture) : IStoreDetails;
