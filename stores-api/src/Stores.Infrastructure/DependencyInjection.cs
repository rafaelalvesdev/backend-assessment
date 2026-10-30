using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stores.Application.Abstractions;
using Stores.Infrastructure.Persistence;
using Stores.Infrastructure.Persistence.Repositories;

namespace Stores.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StoresDb")
            ?? throw new InvalidOperationException("Connection string 'StoresDb' is not configured.");

        services.AddDbContext<StoresDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StoresDbContext>());
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();

        return services;
    }
}
