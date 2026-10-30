-- Optional sample data. Run after the other scripts.
DECLARE @Acme UNIQUEIDENTIFIER = '8E0A5C1E-2B1F-4C8E-9E57-0D6A0E2C1A01';
DECLARE @SaoPaulo UNIQUEIDENTIFIER = '5B7C2D3E-1111-4A2B-8C9D-000000000001';
DECLARE @Lisbon UNIQUEIDENTIFIER = '5B7C2D3E-2222-4A2B-8C9D-000000000002';

IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE CompanyId = @Acme)
BEGIN
    EXEC dbo.usp_InsertProduct @Acme, @SaoPaulo, 'CAFE-250', N'Café torrado 250g', N'Grãos 100% arábica', 32.90, 'BRL', 120;
    EXEC dbo.usp_InsertProduct @Acme, @SaoPaulo, 'ACAI-1L', N'Açaí 1L', NULL, 24.50, 'BRL', 40;
    EXEC dbo.usp_InsertProduct @Acme, @Lisbon, 'PASTEL-6', N'Pastéis de nata (6)', N'Caixa com seis unidades', 7.20, 'EUR', 15;
END
GO
