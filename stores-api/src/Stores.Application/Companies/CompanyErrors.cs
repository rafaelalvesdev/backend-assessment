using Stores.Application.Common;

namespace Stores.Application.Companies;

public static class CompanyErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("company_not_found", $"Company '{id}' was not found.");

    public static Error HasStores(Guid id) =>
        Error.Conflict("company_has_stores", $"Company '{id}' still has stores. Delete them first.");
}
