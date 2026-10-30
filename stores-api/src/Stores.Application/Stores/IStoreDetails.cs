namespace Stores.Application.Stores;

public interface IStoreDetails
{
    string Code { get; }
    string Name { get; }
    AddressInput Address { get; }
    string Currency { get; }
    string TimeZone { get; }
    string Culture { get; }
}
