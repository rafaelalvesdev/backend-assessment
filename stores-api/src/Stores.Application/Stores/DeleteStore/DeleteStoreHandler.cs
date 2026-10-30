using Stores.Application.Abstractions;
using Stores.Application.Common;

namespace Stores.Application.Stores.DeleteStore;

public sealed class DeleteStoreHandler(IStoreRepository stores, IUnitOfWork unitOfWork)
    : IHandler<DeleteStoreCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteStoreCommand command, CancellationToken cancellationToken)
    {
        var store = await stores.GetAsync(command.CompanyId, command.StoreId, cancellationToken);
        if (store is null)
            return StoreErrors.NotFound(command.CompanyId, command.StoreId);

        stores.Remove(store);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
