namespace Products.Functions.Products;

public sealed class DuplicateSkuException(string sku)
    : Exception($"A product with SKU '{sku.ToUpperInvariant()}' already exists in this store.");
