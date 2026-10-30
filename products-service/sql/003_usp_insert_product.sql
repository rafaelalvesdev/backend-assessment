-- Error numbers: 50400 = invalid input, 50409 = SKU already exists in the store.
CREATE OR ALTER PROCEDURE dbo.usp_InsertProduct
    @CompanyId     UNIQUEIDENTIFIER,
    @StoreId       UNIQUEIDENTIFIER,
    @Sku           VARCHAR(50),
    @Name          NVARCHAR(200),
    @Description   NVARCHAR(2000) = NULL,
    @Price         DECIMAL(19, 4),
    @CurrencyCode  CHAR(3),
    @StockQuantity INT = 0,
    @IsActive      BIT = 1,
    @ProductId     UNIQUEIDENTIFIER = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Sku = UPPER(LTRIM(RTRIM(@Sku)));
    SET @Name = LTRIM(RTRIM(@Name));
    SET @CurrencyCode = UPPER(@CurrencyCode);

    IF @CompanyId IS NULL OR @StoreId IS NULL
        THROW 50400, N'CompanyId and StoreId are required.', 1;
    IF NULLIF(@Sku, '') IS NULL
        THROW 50400, N'Sku is required.', 1;
    IF NULLIF(@Name, N'') IS NULL
        THROW 50400, N'Name is required.', 1;
    IF @Price IS NULL OR @Price < 0
        THROW 50400, N'Price must be zero or greater.', 1;
    IF @CurrencyCode IS NULL OR @CurrencyCode NOT LIKE '[A-Z][A-Z][A-Z]'
        THROW 50400, N'CurrencyCode must be a three-letter ISO 4217 code.', 1;
    IF @StockQuantity IS NULL OR @StockQuantity < 0
        THROW 50400, N'StockQuantity must be zero or greater.', 1;

    BEGIN TRANSACTION;

    IF EXISTS (
        SELECT 1
        FROM dbo.Products WITH (UPDLOCK, HOLDLOCK)
        WHERE CompanyId = @CompanyId AND StoreId = @StoreId AND Sku = @Sku)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50409, N'A product with this SKU already exists in the store.', 1;
    END

    DECLARE @inserted TABLE (ProductId UNIQUEIDENTIFIER);

    INSERT INTO dbo.Products (CompanyId, StoreId, Sku, Name, Description, Price, CurrencyCode, StockQuantity, IsActive)
    OUTPUT inserted.ProductId INTO @inserted
    VALUES (@CompanyId, @StoreId, @Sku, @Name, NULLIF(LTRIM(RTRIM(@Description)), N''), @Price, @CurrencyCode, ISNULL(@StockQuantity, 0), ISNULL(@IsActive, 1));

    SELECT @ProductId = ProductId FROM @inserted;

    COMMIT TRANSACTION;

    SELECT ProductId, CompanyId, StoreId, Sku, Name, Description, Price, CurrencyCode, StockQuantity, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Products
    WHERE ProductId = @ProductId;
END
GO
