using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Stores.DeleteStore;

namespace Stores.Application.Tests.Stores;

public class DeleteStoreHandlerTests
{
    private readonly IStoreRepository _stores = Substitute.For<IStoreRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Removes_store_and_saves()
    {
        var companyId = Guid.NewGuid();
        var store = TestData.Store(companyId);
        _stores.GetAsync(companyId, store.Id, Arg.Any<CancellationToken>()).Returns(store);

        var result = await new DeleteStoreHandler(_stores, _unitOfWork)
            .HandleAsync(new DeleteStoreCommand(companyId, store.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _stores.Received(1).Remove(store);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_store()
    {
        var result = await new DeleteStoreHandler(_stores, _unitOfWork)
            .HandleAsync(new DeleteStoreCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
