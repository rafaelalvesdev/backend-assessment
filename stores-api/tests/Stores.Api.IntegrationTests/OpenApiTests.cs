using System.Net;

namespace Stores.Api.IntegrationTests;

public class OpenApiTests(StoresApiFactory factory) : IClassFixture<StoresApiFactory>
{
    [Fact]
    public async Task Document_describes_store_endpoints()
    {
        var response = await factory.CreateClient().GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paths = (await response.ReadJsonAsync())["paths"]!.AsObject();
        Assert.True(paths.ContainsKey("/api/v1/companies/{companyId}/stores/{storeId}"));
    }
}
