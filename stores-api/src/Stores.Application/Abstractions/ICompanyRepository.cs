using Stores.Application.Common;
using Stores.Domain.Companies;

namespace Stores.Application.Abstractions;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<Company>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    void Add(Company company);
    void Remove(Company company);
}
