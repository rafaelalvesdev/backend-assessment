using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Stores.GetStore;

namespace Stores.Application.Tests.Stores;

public class GetStoreHandlerTests
{
    private readonly IStoreRepository _stores = Substitute.For<IStoreRepository>();

    [Fact]
    public async Task Returns_store_of_the_company()
    {
        var companyId = Guid.NewGuid();
        var store = TestData.Store(companyId);
        _stores.GetAsync(companyId, store.Id, Arg.Any<CancellationToken>()).Returns(store);

        var result = await new GetStoreHandler(_stores).HandleAsync(new GetStoreQuery(companyId, store.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(store.Id, result.Value.Id);
        Assert.Equal("São Paulo", result.Value.Address.City);
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_store()
    {
        var result = await new GetStoreHandler(_stores).HandleAsync(new GetStoreQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
