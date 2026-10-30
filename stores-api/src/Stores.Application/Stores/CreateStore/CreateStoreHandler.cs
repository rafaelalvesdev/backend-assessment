using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Companies;
using Stores.Domain.Stores;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Stores.CreateStore;

public sealed class CreateStoreHandler(
    ICompanyRepository companies,
    IStoreRepository stores,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IHandler<CreateStoreCommand, Result<StoreResponse>>
{
    public async Task<Result<StoreResponse>> HandleAsync(CreateStoreCommand command, CancellationToken cancellationToken)
    {
        if (!await companies.ExistsAsync(command.CompanyId, cancellationToken))
            return CompanyErrors.NotFound(command.CompanyId);

        var code = command.NormalizedCode();
        if (await stores.CodeExistsAsync(command.CompanyId, code, null, cancellationToken))
            return StoreErrors.CodeInUse(code);

        var store = Store.Create(
            command.CompanyId,
            code,
            command.Name,
            command.Address.ToAddress(),
            CurrencyCode.From(command.Currency),
            TimeZoneId.From(command.TimeZone),
            CultureCode.From(command.Culture),
            clock.GetUtcNow());

        stores.Add(store);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return store.ToResponse();
    }
}
