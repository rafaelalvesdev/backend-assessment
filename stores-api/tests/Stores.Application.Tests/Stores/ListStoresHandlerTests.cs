using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Stores.ListStores;
using Stores.Domain.Stores;

namespace Stores.Application.Tests.Stores;

public class ListStoresHandlerTests
{
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();
    private readonly IStoreRepository _stores = Substitute.For<IStoreRepository>();

    [Fact]
    public async Task Returns_a_page_of_stores()
    {
        var companyId = Guid.NewGuid();
        _companies.ExistsAsync(companyId, Arg.Any<CancellationToken>()).Returns(true);
        var page = new PagedResult<Store>([TestData.Store(companyId, "A"), TestData.Store(companyId, "B")], 2, 2, 5);
        _stores.ListAsync(companyId, "BR", 2, 2, Arg.Any<CancellationToken>()).Returns(page);

        var result = await new ListStoresHandler(_companies, _stores)
            .HandleAsync(new ListStoresQuery(companyId, "br", 2, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["A", "B"], result.Value.Items.Select(s => s.Code));
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(3, result.Value.TotalPages);
    }

    [Fact]
    public async Task Returns_not_found_when_company_does_not_exist()
    {
        var result = await new ListStoresHandler(_companies, _stores)
            .HandleAsync(new ListStoresQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
