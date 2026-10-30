using NSubstitute;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Application.Companies.DeleteCompany;

namespace Stores.Application.Tests.Companies;

public class DeleteCompanyHandlerTests
{
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();
    private readonly IStoreRepository _stores = Substitute.For<IStoreRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteCompanyHandler _handler;

    public DeleteCompanyHandlerTests()
    {
        _handler = new DeleteCompanyHandler(_companies, _stores, _unitOfWork);
    }

    [Fact]
    public async Task Removes_company_without_stores()
    {
        var company = TestData.Company();
        _companies.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

        var result = await _handler.HandleAsync(new DeleteCompanyCommand(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _companies.Received(1).Remove(company);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_company()
    {
        var result = await _handler.HandleAsync(new DeleteCompanyCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Returns_conflict_while_company_has_stores()
    {
        var company = TestData.Company();
        _companies.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);
        _stores.AnyForCompanyAsync(company.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.HandleAsync(new DeleteCompanyCommand(company.Id), CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _companies.DidNotReceiveWithAnyArgs().Remove(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
