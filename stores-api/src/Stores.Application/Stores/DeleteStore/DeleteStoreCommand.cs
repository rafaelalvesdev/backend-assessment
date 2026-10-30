namespace Stores.Application.Stores.DeleteStore;

public sealed record DeleteStoreCommand(Guid CompanyId, Guid StoreId);
