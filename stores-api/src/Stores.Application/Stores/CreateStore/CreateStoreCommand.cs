namespace Stores.Application.Stores.CreateStore;

public sealed record CreateStoreCommand(
    Guid CompanyId,
    string Code,
    string Name,
    AddressInput Address,
    string Currency,
    string TimeZone,
    string Culture) : IStoreDetails;
