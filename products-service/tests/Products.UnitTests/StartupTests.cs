using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Products.UnitTests;

public class StartupTests
{
    [Fact]
    public async Task Application_insights_packages_are_compatible()
    {
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

        await using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<TelemetryClient>());
    }
}
