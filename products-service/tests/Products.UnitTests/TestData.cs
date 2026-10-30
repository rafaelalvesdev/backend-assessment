using Products.Functions.Products;

namespace Products.UnitTests;

internal static class TestData
{
    public static readonly Guid CompanyId = Guid.Parse("8e0a5c1e-2b1f-4c8e-9e57-0d6a0e2c1a01");
    public static readonly Guid StoreId = Guid.Parse("5b7c2d3e-1111-4a2b-8c9d-000000000001");

    public static ProductRequest Request(string sku = "CAFE-250", decimal price = 32.90m, string currency = "BRL") =>
        new(StoreId, sku, "Café torrado 250g", "Grãos 100% arábica", price, currency, 120, true);

    public static Product Product(Guid? id = null) => new(
        id ?? Guid.NewGuid(),
        CompanyId,
        StoreId,
        "CAFE-250",
        "Café torrado 250g",
        null,
        32.90m,
        "BRL",
        120,
        true,
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow);
}
