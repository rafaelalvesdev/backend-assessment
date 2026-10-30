using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Stores.Api.Errors;
using Stores.Application;
using Stores.Infrastructure;
using Stores.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<StoresDbContext>();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Stores API";
    document.Info.Description = "Manage companies and their stores across countries, currencies and time zones.";
    return Task.CompletedTask;
}));

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<StoresDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("Stores API"));
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program;
