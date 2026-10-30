namespace Products.Functions.Products;

public sealed record Product(
    Guid Id,
    Guid CompanyId,
    Guid StoreId,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
