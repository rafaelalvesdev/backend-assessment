using Stores.Application.Abstractions;
using Stores.Application.Common;

namespace Stores.Application.Companies.DeleteCompany;

public sealed class DeleteCompanyHandler(
    ICompanyRepository companies,
    IStoreRepository stores,
    IUnitOfWork unitOfWork) : IHandler<DeleteCompanyCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await companies.GetByIdAsync(command.CompanyId, cancellationToken);
        if (company is null)
            return CompanyErrors.NotFound(command.CompanyId);

        if (await stores.AnyForCompanyAsync(company.Id, cancellationToken))
            return CompanyErrors.HasStores(company.Id);

        companies.Remove(company);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
