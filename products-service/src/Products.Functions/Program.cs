using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Products.Functions.Data;
using Products.Functions.Products;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var connectionString = builder.Configuration.GetConnectionString("ProductsDb")
    ?? throw new InvalidOperationException("Connection string 'ProductsDb' is not configured.");

builder.Services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductRequestValidator>(ServiceLifetime.Singleton);

builder.Build().Run();
