using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Stores.UpdateStore;

public sealed class UpdateStoreHandler(
    IStoreRepository stores,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IHandler<UpdateStoreCommand, Result<StoreResponse>>
{
    public async Task<Result<StoreResponse>> HandleAsync(UpdateStoreCommand command, CancellationToken cancellationToken)
    {
        var store = await stores.GetAsync(command.CompanyId, command.StoreId, cancellationToken);
        if (store is null)
            return StoreErrors.NotFound(command.CompanyId, command.StoreId);

        var code = command.NormalizedCode();
        if (await stores.CodeExistsAsync(command.CompanyId, code, store.Id, cancellationToken))
            return StoreErrors.CodeInUse(code);

        store.Update(
            code,
            command.Name,
            command.Address.ToAddress(),
            CurrencyCode.From(command.Currency),
            TimeZoneId.From(command.TimeZone),
            CultureCode.From(command.Culture),
            clock.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return store.ToResponse();
    }
}
