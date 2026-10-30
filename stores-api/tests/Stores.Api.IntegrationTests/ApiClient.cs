using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Stores.Api.IntegrationTests;

internal static class ApiClient
{
    public static object StoreBody(string code = "SP-001", string country = "BR", string currency = "BRL") => new
    {
        code,
        name = "Paulista",
        address = new { street = "Av. Paulista, 1000", city = "São Paulo", state = "SP", postalCode = "01310-100", countryCode = country },
        currency,
        timeZone = "America/Sao_Paulo",
        culture = "pt-BR"
    };

    public static async Task<Guid> CreateCompanyAsync(this HttpClient client, string name = "Acme Retail")
    {
        var response = await client.PostAsJsonAsync("/api/v1/companies", new { name, countryCode = "BR" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.ReadJsonAsync())["id"]!.GetValue<Guid>();
    }

    public static async Task<JsonNode> CreateStoreAsync(this HttpClient client, Guid companyId, object? body = null)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/companies/{companyId}/stores", body ?? StoreBody());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.ReadJsonAsync();
    }

    public static async Task<JsonNode> ReadJsonAsync(this HttpResponseMessage response) =>
        JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
}
