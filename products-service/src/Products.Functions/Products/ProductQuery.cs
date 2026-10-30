namespace Products.Functions.Products;

public sealed record ProductQuery(Guid? StoreId, int Page = 1, int PageSize = ProductQuery.DefaultPageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
