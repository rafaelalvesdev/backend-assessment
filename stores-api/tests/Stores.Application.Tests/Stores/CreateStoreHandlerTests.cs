using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Stores.CreateStore;
using Stores.Domain.Stores;

namespace Stores.Application.Tests.Stores;

public class CreateStoreHandlerTests
{
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();
    private readonly IStoreRepository _stores = Substitute.For<IStoreRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _clock = new(TestData.Now);
    private readonly CreateStoreHandler _handler;

    public CreateStoreHandlerTests()
    {
        _handler = new CreateStoreHandler(_companies, _stores, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Creates_store_for_existing_company()
    {
        var companyId = Guid.NewGuid();
        _companies.ExistsAsync(companyId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.HandleAsync(TestData.CreateCommand(companyId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(companyId, result.Value.CompanyId);
        Assert.Equal("LX-001", result.Value.Code);
        Assert.Equal("PT", result.Value.Address.CountryCode);
        Assert.Equal("EUR", result.Value.Currency);
        Assert.Equal(TestData.Now, result.Value.CreatedAt);
        _stores.Received(1).Add(Arg.Is<Store>(s => s.CompanyId == companyId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_not_found_when_company_does_not_exist()
    {
        var result = await _handler.HandleAsync(TestData.CreateCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _stores.DidNotReceiveWithAnyArgs().Add(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Returns_conflict_when_code_is_already_used_in_the_company()
    {
        var companyId = Guid.NewGuid();
        _companies.ExistsAsync(companyId, Arg.Any<CancellationToken>()).Returns(true);
        _stores.CodeExistsAsync(companyId, "LX-001", null, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.HandleAsync(TestData.CreateCommand(companyId, "lx-001"), CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
