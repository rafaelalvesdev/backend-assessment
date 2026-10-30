using System.Net;
using System.Net.Http.Json;

namespace Stores.Api.IntegrationTests;

public class CompaniesEndpointsTests(StoresApiFactory factory) : IClassFixture<StoresApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Created_company_can_be_fetched()
    {
        var id = await _client.CreateCompanyAsync("Umbrella");

        var response = await _client.GetAsync($"/api/v1/companies/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Umbrella", (await response.ReadJsonAsync())["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task List_returns_companies()
    {
        await _client.CreateCompanyAsync("Initech");

        var body = await (await _client.GetAsync("/api/v1/companies?pageSize=100")).ReadJsonAsync();

        Assert.Contains(body["items"]!.AsArray(), c => c!["name"]!.GetValue<string>() == "Initech");
    }

    [Fact]
    public async Task Invalid_company_returns_validation_problem()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/companies", new { name = "", countryCode = "Brazil" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.ReadJsonAsync())["errors"]!.AsObject();
        Assert.True(errors.ContainsKey("name"));
        Assert.True(errors.ContainsKey("countryCode"));
    }

    [Fact]
    public async Task Unknown_company_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/v1/companies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_company()
    {
        var id = await _client.CreateCompanyAsync("Temporary");

        var response = await _client.DeleteAsync($"/api/v1/companies/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/v1/companies/{id}")).StatusCode);
    }

    [Fact]
    public async Task Delete_is_rejected_while_company_has_stores()
    {
        var id = await _client.CreateCompanyAsync("Busy");
        var store = await _client.CreateStoreAsync(id);

        var blocked = await _client.DeleteAsync($"/api/v1/companies/{id}");
        await _client.DeleteAsync($"/api/v1/companies/{id}/stores/{store["id"]}");
        var allowed = await _client.DeleteAsync($"/api/v1/companies/{id}");

        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.Equal("application/problem+json", blocked.Content.Headers.ContentType!.MediaType);
        Assert.Equal(HttpStatusCode.NoContent, allowed.StatusCode);
    }

    [Fact]
    public async Task Delete_unknown_company_returns_not_found()
    {
        var response = await _client.DeleteAsync($"/api/v1/companies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
