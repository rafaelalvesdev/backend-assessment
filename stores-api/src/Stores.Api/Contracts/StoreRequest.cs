using Stores.Application.Stores;
using Stores.Application.Stores.CreateStore;
using Stores.Application.Stores.UpdateStore;

namespace Stores.Api.Contracts;

public sealed record StoreRequest(
    string Code,
    string Name,
    AddressInput Address,
    string Currency,
    string TimeZone,
    string Culture)
{
    public CreateStoreCommand ToCreateCommand(Guid companyId) =>
        new(companyId, Code, Name, Address, Currency, TimeZone, Culture);

    public UpdateStoreCommand ToUpdateCommand(Guid companyId, Guid storeId) =>
        new(companyId, storeId, Code, Name, Address, Currency, TimeZone, Culture);
}
