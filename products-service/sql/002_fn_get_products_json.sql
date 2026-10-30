-- Returns a single JSON object: { companyId, storeId, totalCount, products: [...] }.
-- Pass DEFAULT (or NULL) for @StoreId to list every store of the company.
CREATE OR ALTER FUNCTION dbo.fn_GetProductsJson
(
    @CompanyId UNIQUEIDENTIFIER,
    @StoreId   UNIQUEIDENTIFIER = NULL
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @products NVARCHAR(MAX) =
    (
        SELECT
            p.ProductId     AS id,
            p.StoreId       AS storeId,
            p.Sku           AS sku,
            p.Name          AS name,
            p.Description   AS description,
            p.Price         AS price,
            p.CurrencyCode  AS currency,
            p.StockQuantity AS stockQuantity,
            CAST(p.IsActive AS BIT) AS isActive,
            p.CreatedAt     AS createdAt,
            p.UpdatedAt     AS updatedAt
        FROM dbo.Products AS p
        WHERE p.CompanyId = @CompanyId
          AND (@StoreId IS NULL OR p.StoreId = @StoreId)
        ORDER BY p.StoreId, p.Sku
        FOR JSON PATH, INCLUDE_NULL_VALUES
    );

    RETURN
    (
        SELECT
            @CompanyId AS companyId,
            @StoreId   AS storeId,
            (
                SELECT COUNT(*)
                FROM dbo.Products
                WHERE CompanyId = @CompanyId
                  AND (@StoreId IS NULL OR StoreId = @StoreId)
            ) AS totalCount,
            JSON_QUERY(COALESCE(@products, N'[]')) AS products
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
    );
END
GO
