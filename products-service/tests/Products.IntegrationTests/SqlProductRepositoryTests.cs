using System.Text.Json;
using Products.Functions.Data;
using Products.Functions.Products;

namespace Products.IntegrationTests;

[Collection(SqlServerCollection.Name)]
public class SqlProductRepositoryTests(SqlServerFixture sql)
{
    private readonly SqlProductRepository _repository = new(new SqlConnectionFactory(sql.ConnectionString));
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _storeId = Guid.NewGuid();

    private ProductRequest Request(string sku = "cafe-250", Guid? storeId = null) =>
        new(storeId ?? _storeId, sku, "Café torrado 250g", "Grãos 100% arábica", 32.9m, "brl", 120, true);

    [Fact]
    public async Task Insert_uses_procedure_and_returns_normalized_row()
    {
        var product = await _repository.InsertAsync(_companyId, Request(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(_companyId, product.CompanyId);
        Assert.Equal("CAFE-250", product.Sku);
        Assert.Equal("BRL", product.Currency);
        Assert.Equal("Café torrado 250g", product.Name);
        Assert.Equal(32.9m, product.Price);
    }

    [Fact]
    public async Task Insert_throws_for_duplicate_sku_in_same_store()
    {
        await _repository.InsertAsync(_companyId, Request(), CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateSkuException>(() =>
            _repository.InsertAsync(_companyId, Request("CAFE-250"), CancellationToken.None));
    }

    [Fact]
    public async Task Same_sku_is_allowed_in_another_store()
    {
        await _repository.InsertAsync(_companyId, Request(), CancellationToken.None);

        await _repository.InsertAsync(_companyId, Request(storeId: Guid.NewGuid()), CancellationToken.None);
    }

    [Fact]
    public async Task Get_is_scoped_to_company()
    {
        var product = await _repository.InsertAsync(_companyId, Request(), CancellationToken.None);

        Assert.NotNull(await _repository.GetAsync(_companyId, product.Id, CancellationToken.None));
        Assert.Null(await _repository.GetAsync(Guid.NewGuid(), product.Id, CancellationToken.None));
    }

    [Fact]
    public async Task List_pages_and_filters_by_store()
    {
        var otherStore = Guid.NewGuid();
        await _repository.InsertAsync(_companyId, Request("A"), CancellationToken.None);
        await _repository.InsertAsync(_companyId, Request("B"), CancellationToken.None);
        await _repository.InsertAsync(_companyId, Request("C"), CancellationToken.None);
        await _repository.InsertAsync(_companyId, Request("D", otherStore), CancellationToken.None);

        var all = await _repository.ListAsync(_companyId, null, 1, 10, CancellationToken.None);
        var secondPage = await _repository.ListAsync(_companyId, _storeId, 2, 2, CancellationToken.None);

        Assert.Equal(4, all.TotalCount);
        Assert.Equal(3, secondPage.TotalCount);
        Assert.Equal(["C"], secondPage.Items.Select(p => p.Sku));
    }

    [Fact]
    public async Task Update_changes_row_and_bumps_updated_at()
    {
        var product = await _repository.InsertAsync(_companyId, Request(), CancellationToken.None);
        var change = Request("cafe-500") with { Name = "Café 500g", Price = 59.9m, Description = null };

        var updated = await _repository.UpdateAsync(_companyId, product.Id, change, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("CAFE-500", updated.Sku);
        Assert.Equal(59.9m, updated.Price);
        Assert.Null(updated.Description);
        Assert.True(updated.UpdatedAt >= product.UpdatedAt);
        Assert.Equal(product.CreatedAt, updated.CreatedAt);
    }

    [Fact]
    public async Task Update_returns_null_for_other_company()
    {
        var product = await _repository.InsertAsync(_companyId, Request(), CancellationToken.None);

        Assert.Null(await _repository.UpdateAsync(Guid.NewGuid(), product.Id, Request(), CancellationToken.None));
    }

    [Fact]
    public async Task Update_throws_for_duplicate_sku()
    {
        await _repository.InsertAsync(_companyId, Request("A"), CancellationToken.None);
        var b = await _repository.InsertAsync(_companyId, Request("B"), CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateSkuException>(() =>
            _repository.UpdateAsync(_companyId, b.Id, Request("a"), CancellationToken.None));
    }

    [Fact]
    public async Task Delete_removes_only_matching_company_row()
    {
        var product = await _repository.InsertAsync(_companyId, Request(), CancellationToken.None);

        Assert.False(await _repository.DeleteAsync(Guid.NewGuid(), product.Id, CancellationToken.None));
        Assert.True(await _repository.DeleteAsync(_companyId, product.Id, CancellationToken.None));
        Assert.Null(await _repository.GetAsync(_companyId, product.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Catalog_json_comes_from_scalar_function()
    {
        await _repository.InsertAsync(_companyId, Request("A"), CancellationToken.None);
        await _repository.InsertAsync(_companyId, Request("B", Guid.NewGuid()), CancellationToken.None);

        using var all = JsonDocument.Parse(await _repository.GetCatalogJsonAsync(_companyId, null, CancellationToken.None));
        using var store = JsonDocument.Parse(await _repository.GetCatalogJsonAsync(_companyId, _storeId, CancellationToken.None));

        Assert.Equal(2, all.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, all.RootElement.GetProperty("products").GetArrayLength());
        Assert.Equal(1, store.RootElement.GetProperty("products").GetArrayLength());
        var product = store.RootElement.GetProperty("products")[0];
        Assert.Equal("A", product.GetProperty("sku").GetString());
        Assert.Equal("Café torrado 250g", product.GetProperty("name").GetString());
        Assert.Equal(32.9m, product.GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task Catalog_for_company_without_products_has_empty_array()
    {
        using var json = JsonDocument.Parse(await _repository.GetCatalogJsonAsync(Guid.NewGuid(), null, CancellationToken.None));

        Assert.Equal(0, json.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(JsonValueKind.Array, json.RootElement.GetProperty("products").ValueKind);
    }
}
