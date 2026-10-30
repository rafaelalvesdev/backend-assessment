using Microsoft.Data.SqlClient;

namespace Products.IntegrationTests;

[Collection(SqlServerCollection.Name)]
public class InsertProductProcedureTests(SqlServerFixture sql)
{
    [Theory]
    [InlineData("SKU-1", "Name", -1, "BRL", 0)]
    [InlineData("", "Name", 1, "BRL", 0)]
    [InlineData("SKU-1", " ", 1, "BRL", 0)]
    [InlineData("SKU-1", "Name", 1, "R$", 0)]
    [InlineData("SKU-1", "Name", 1, "BRL", -3)]
    public async Task Rejects_invalid_input_with_error_50400(string sku, string name, decimal price, string currency, int stock)
    {
        await using var connection = new SqlConnection(sql.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand("dbo.usp_InsertProduct", connection) { CommandType = System.Data.CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@CompanyId", Guid.NewGuid());
        command.Parameters.AddWithValue("@StoreId", Guid.NewGuid());
        command.Parameters.AddWithValue("@Sku", sku);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Price", price);
        command.Parameters.AddWithValue("@CurrencyCode", currency);
        command.Parameters.AddWithValue("@StockQuantity", stock);

        var error = await Assert.ThrowsAsync<SqlException>(() => command.ExecuteNonQueryAsync());

        Assert.Equal(50400, error.Number);
    }
}
