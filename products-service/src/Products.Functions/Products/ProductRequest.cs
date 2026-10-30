namespace Products.Functions.Products;

public sealed record ProductRequest(
    Guid StoreId,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive = true);
