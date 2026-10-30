using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Products.Functions.Common;
using Products.Functions.Products;

namespace Products.Functions.Data;

public sealed class SqlProductRepository(ISqlConnectionFactory connections) : IProductRepository
{
    private const string Columns =
        "ProductId AS Id, CompanyId, StoreId, Sku, Name, Description, Price, CurrencyCode AS Currency, StockQuantity, IsActive, CreatedAt, UpdatedAt";

    private const string InsertedColumns =
        "inserted.ProductId AS Id, inserted.CompanyId, inserted.StoreId, inserted.Sku, inserted.Name, inserted.Description, " +
        "inserted.Price, inserted.CurrencyCode AS Currency, inserted.StockQuantity, inserted.IsActive, inserted.CreatedAt, inserted.UpdatedAt";

    private static readonly int[] DuplicateKeyErrors = [50409, 2627, 2601];

    public async Task<Product?> GetAsync(Guid companyId, Guid productId, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();

        return await connection.QuerySingleOrDefaultAsync<Product>(new CommandDefinition(
            $"SELECT {Columns} FROM dbo.Products WHERE CompanyId = @companyId AND ProductId = @productId",
            new { companyId, productId },
            cancellationToken: cancellationToken));
    }

    public async Task<PagedResult<Product>> ListAsync(
        Guid companyId,
        Guid? storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        const string filter = "WHERE CompanyId = @companyId AND (@storeId IS NULL OR StoreId = @storeId)";

        await using var connection = connections.Create();
        using var results = await connection.QueryMultipleAsync(new CommandDefinition(
            $"""
            SELECT COUNT(*) FROM dbo.Products {filter};

            SELECT {Columns}
            FROM dbo.Products {filter}
            ORDER BY Sku, StoreId
            OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
            """,
            new { companyId, storeId, skip = (page - 1) * pageSize, take = pageSize },
            cancellationToken: cancellationToken));

        var total = await results.ReadSingleAsync<int>();
        var items = (await results.ReadAsync<Product>()).ToList();

        return new PagedResult<Product>(items, page, pageSize, total);
    }

    public async Task<string> GetCatalogJsonAsync(Guid companyId, Guid? storeId, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();

        return await connection.ExecuteScalarAsync<string>(new CommandDefinition(
            "SELECT dbo.fn_GetProductsJson(@companyId, @storeId)",
            new { companyId, storeId },
            cancellationToken: cancellationToken)) ?? "{}";
    }

    public async Task<Product> InsertAsync(Guid companyId, ProductRequest request, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters(new
        {
            CompanyId = companyId,
            request.StoreId,
            Sku = Ansi(request.Sku, 50),
            request.Name,
            request.Description,
            request.Price,
            CurrencyCode = Ansi(request.Currency, 3),
            request.StockQuantity,
            request.IsActive
        });
        parameters.Add("ProductId", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await using var connection = connections.Create();
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_InsertProduct",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        }
        catch (SqlException ex) when (DuplicateKeyErrors.Contains(ex.Number))
        {
            throw new DuplicateSkuException(request.Sku);
        }

        return (await GetAsync(companyId, parameters.Get<Guid>("ProductId"), cancellationToken))!;
    }

    public async Task<Product?> UpdateAsync(Guid companyId, Guid productId, ProductRequest request, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();
        try
        {
            return await connection.QuerySingleOrDefaultAsync<Product>(new CommandDefinition(
                $"""
                UPDATE dbo.Products
                SET StoreId = @StoreId,
                    Sku = UPPER(TRIM(@Sku)),
                    Name = TRIM(@Name),
                    Description = NULLIF(TRIM(@Description), N''),
                    Price = @Price,
                    CurrencyCode = UPPER(@Currency),
                    StockQuantity = @StockQuantity,
                    IsActive = @IsActive,
                    UpdatedAt = SYSUTCDATETIME()
                OUTPUT {InsertedColumns}
                WHERE CompanyId = @CompanyId AND ProductId = @ProductId;
                """,
                new
                {
                    CompanyId = companyId,
                    ProductId = productId,
                    request.StoreId,
                    Sku = Ansi(request.Sku, 50),
                    request.Name,
                    request.Description,
                    request.Price,
                    Currency = Ansi(request.Currency, 3),
                    request.StockQuantity,
                    request.IsActive
                },
                cancellationToken: cancellationToken));
        }
        catch (SqlException ex) when (DuplicateKeyErrors.Contains(ex.Number))
        {
            throw new DuplicateSkuException(request.Sku);
        }
    }

    public async Task<bool> DeleteAsync(Guid companyId, Guid productId, CancellationToken cancellationToken)
    {
        await using var connection = connections.Create();

        var rows = await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM dbo.Products WHERE CompanyId = @companyId AND ProductId = @productId",
            new { companyId, productId },
            cancellationToken: cancellationToken));

        return rows > 0;
    }

    private static DbString Ansi(string value, int length) => new() { Value = value, IsAnsi = true, Length = length };
}
