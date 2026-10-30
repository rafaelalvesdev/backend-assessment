using Stores.Application.Common;

namespace Stores.Application.Stores;

public static class StoreErrors
{
    public static Error NotFound(Guid companyId, Guid storeId) =>
        Error.NotFound("store_not_found", $"Store '{storeId}' was not found for company '{companyId}'.");

    public static Error CodeInUse(string code) =>
        Error.Conflict("store_code_in_use", $"Another store of this company already uses the code '{code}'.");
}
