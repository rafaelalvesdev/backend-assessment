using Microsoft.EntityFrameworkCore;
using Stores.Application.Abstractions;
using Stores.Application.Common;
using Stores.Domain.Companies;

namespace Stores.Infrastructure.Persistence.Repositories;

internal sealed class CompanyRepository(StoresDbContext db) : ICompanyRepository
{
    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Companies.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<PagedResult<Company>> ListAsync(int page, int pageSize, CancellationToken cancellationToken) =>
        db.Companies.AsNoTracking().OrderBy(c => c.Name).ThenBy(c => c.Id).ToPagedAsync(page, pageSize, cancellationToken);

    public void Add(Company company) => db.Companies.Add(company);

    public void Remove(Company company) => db.Companies.Remove(company);
}
