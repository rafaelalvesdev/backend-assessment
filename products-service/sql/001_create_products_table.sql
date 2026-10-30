IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductId     UNIQUEIDENTIFIER   NOT NULL CONSTRAINT DF_Products_ProductId DEFAULT NEWSEQUENTIALID(),
        CompanyId     UNIQUEIDENTIFIER   NOT NULL,
        StoreId       UNIQUEIDENTIFIER   NOT NULL,
        Sku           VARCHAR(50)        NOT NULL,
        Name          NVARCHAR(200)      NOT NULL,
        Description   NVARCHAR(2000)     NULL,
        Price         DECIMAL(19, 4)     NOT NULL,
        CurrencyCode  CHAR(3)            NOT NULL,
        StockQuantity INT                NOT NULL CONSTRAINT DF_Products_StockQuantity DEFAULT 0,
        IsActive      BIT                NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT 1,
        CreatedAt     DATETIMEOFFSET(3)  NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt     DATETIMEOFFSET(3)  NOT NULL CONSTRAINT DF_Products_UpdatedAt DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (ProductId),
        CONSTRAINT UQ_Products_Company_Store_Sku UNIQUE (CompanyId, StoreId, Sku),
        CONSTRAINT CK_Products_Price CHECK (Price >= 0),
        CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0),
        CONSTRAINT CK_Products_CurrencyCode CHECK (CurrencyCode LIKE '[A-Z][A-Z][A-Z]')
    );

    CREATE NONCLUSTERED INDEX IX_Products_Company_Store
        ON dbo.Products (CompanyId, StoreId)
        INCLUDE (Sku, Name, Price, CurrencyCode, StockQuantity, IsActive);
END
GO
