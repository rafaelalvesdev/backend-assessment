using Microsoft.EntityFrameworkCore;
using Stores.Application.Abstractions;
using Stores.Domain.Companies;
using Stores.Domain.Stores;

namespace Stores.Infrastructure.Persistence;

public sealed class StoresDbContext(DbContextOptions<StoresDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoresDbContext).Assembly);

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}
