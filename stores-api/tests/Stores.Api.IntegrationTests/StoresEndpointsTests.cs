using System.Net;
using System.Net.Http.Json;

namespace Stores.Api.IntegrationTests;

public class StoresEndpointsTests(StoresApiFactory factory) : IClassFixture<StoresApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_creates_store_and_returns_location()
    {
        var companyId = await _client.CreateCompanyAsync();

        var response = await _client.PostAsJsonAsync($"/api/v1/companies/{companyId}/stores", ApiClient.StoreBody("sp-001"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.ReadJsonAsync();
        var id = body["id"]!.GetValue<Guid>();
        Assert.Equal($"/api/v1/companies/{companyId}/stores/{id}", response.Headers.Location!.AbsolutePath);
        Assert.Equal("SP-001", body["code"]!.GetValue<string>());
        Assert.Equal("BR", body["address"]!["countryCode"]!.GetValue<string>());
        Assert.Equal("America/Sao_Paulo", body["timeZone"]!.GetValue<string>());
    }

    [Fact]
    public async Task Get_returns_store()
    {
        var companyId = await _client.CreateCompanyAsync();
        var created = await _client.CreateStoreAsync(companyId);

        var response = await _client.GetAsync($"/api/v1/companies/{companyId}/stores/{created["id"]}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.Equal(created["id"]!.GetValue<Guid>(), body["id"]!.GetValue<Guid>());
        Assert.Equal("São Paulo", body["address"]!["city"]!.GetValue<string>());
    }

    [Fact]
    public async Task Put_replaces_store()
    {
        var companyId = await _client.CreateCompanyAsync();
        var created = await _client.CreateStoreAsync(companyId);
        var update = new
        {
            code = "LX-001",
            name = "Baixa",
            address = new { street = "Rua Augusta, 10", city = "Lisboa", countryCode = "PT" },
            currency = "EUR",
            timeZone = "Europe/Lisbon",
            culture = "pt-PT"
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/companies/{companyId}/stores/{created["id"]}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.Equal("LX-001", body["code"]!.GetValue<string>());
        Assert.Equal("EUR", body["currency"]!.GetValue<string>());
        Assert.Null(body["address"]!["state"]);
    }

    [Fact]
    public async Task Delete_removes_store()
    {
        var companyId = await _client.CreateCompanyAsync();
        var created = await _client.CreateStoreAsync(companyId);
        var url = $"/api/v1/companies/{companyId}/stores/{created["id"]}";

        var response = await _client.DeleteAsync(url);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync(url)).StatusCode);
    }

    [Fact]
    public async Task List_is_paged_and_filterable_by_country()
    {
        var companyId = await _client.CreateCompanyAsync();
        await _client.CreateStoreAsync(companyId, ApiClient.StoreBody("BR-1"));
        await _client.CreateStoreAsync(companyId, ApiClient.StoreBody("BR-2"));
        await _client.CreateStoreAsync(companyId, ApiClient.StoreBody("US-1", "US", "USD"));

        var page = await (await _client.GetAsync($"/api/v1/companies/{companyId}/stores?page=1&pageSize=2")).ReadJsonAsync();
        var brazil = await (await _client.GetAsync($"/api/v1/companies/{companyId}/stores?country=br")).ReadJsonAsync();

        Assert.Equal(2, page["items"]!.AsArray().Count);
        Assert.Equal(3, page["totalCount"]!.GetValue<int>());
        Assert.Equal(2, page["totalPages"]!.GetValue<int>());
        Assert.Equal(["BR-1", "BR-2"], brazil["items"]!.AsArray().Select(s => s!["code"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Stores_are_isolated_per_company()
    {
        var acme = await _client.CreateCompanyAsync("Acme");
        var globex = await _client.CreateCompanyAsync("Globex");
        var store = await _client.CreateStoreAsync(acme);

        var response = await _client.GetAsync($"/api/v1/companies/{globex}/stores/{store["id"]}");
        var list = await (await _client.GetAsync($"/api/v1/companies/{globex}/stores")).ReadJsonAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(list["items"]!.AsArray());
    }

    [Fact]
    public async Task Same_code_is_allowed_in_different_companies()
    {
        var acme = await _client.CreateCompanyAsync("Acme");
        var globex = await _client.CreateCompanyAsync("Globex");

        await _client.CreateStoreAsync(acme, ApiClient.StoreBody("HQ"));
        await _client.CreateStoreAsync(globex, ApiClient.StoreBody("HQ"));
    }

    [Fact]
    public async Task Duplicate_code_in_same_company_returns_conflict()
    {
        var companyId = await _client.CreateCompanyAsync();
        await _client.CreateStoreAsync(companyId, ApiClient.StoreBody("HQ"));

        var response = await _client.PostAsJsonAsync($"/api/v1/companies/{companyId}/stores", ApiClient.StoreBody("hq"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Invalid_store_returns_validation_problem()
    {
        var companyId = await _client.CreateCompanyAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/companies/{companyId}/stores",
            ApiClient.StoreBody(country: "XX", currency: "BRLX"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.ReadJsonAsync())["errors"]!.AsObject();
        Assert.Contains("currency", errors.Select(e => e.Key));
        Assert.Contains("address.countryCode", errors.Select(e => e.Key));
    }

    [Fact]
    public async Task Malformed_json_returns_bad_request()
    {
        var companyId = await _client.CreateCompanyAsync();
        var content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"/api/v1/companies/{companyId}/stores", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_company_returns_not_found()
    {
        var response = await _client.PostAsJsonAsync($"/api/v1/companies/{Guid.NewGuid()}/stores", ApiClient.StoreBody());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Page_size_above_limit_is_rejected()
    {
        var companyId = await _client.CreateCompanyAsync();

        var response = await _client.GetAsync($"/api/v1/companies/{companyId}/stores?pageSize=500");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
