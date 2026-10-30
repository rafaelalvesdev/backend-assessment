using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Stores.UpdateStore;

namespace Stores.Application.Tests.Stores;

public class UpdateStoreHandlerTests
{
    private readonly IStoreRepository _stores = Substitute.For<IStoreRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _clock = new(TestData.Now);
    private readonly UpdateStoreHandler _handler;

    public UpdateStoreHandlerTests()
    {
        _handler = new UpdateStoreHandler(_stores, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Updates_store_and_saves()
    {
        var companyId = Guid.NewGuid();
        var store = TestData.Store(companyId);
        _stores.GetAsync(companyId, store.Id, Arg.Any<CancellationToken>()).Returns(store);
        _clock.Advance(TimeSpan.FromHours(1));

        var result = await _handler.HandleAsync(TestData.UpdateCommand(companyId, store.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("LX-001", result.Value.Code);
        Assert.Equal("Europe/Lisbon", result.Value.TimeZone);
        Assert.Equal(TestData.Now, result.Value.CreatedAt);
        Assert.Equal(TestData.Now.AddHours(1), result.Value.UpdatedAt);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_store()
    {
        var result = await _handler.HandleAsync(TestData.UpdateCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Returns_conflict_when_another_store_uses_the_code()
    {
        var companyId = Guid.NewGuid();
        var store = TestData.Store(companyId);
        _stores.GetAsync(companyId, store.Id, Arg.Any<CancellationToken>()).Returns(store);
        _stores.CodeExistsAsync(companyId, "LX-001", store.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.HandleAsync(TestData.UpdateCommand(companyId, store.Id), CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("SP-001", store.Code);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
