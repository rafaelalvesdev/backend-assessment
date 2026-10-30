using Microsoft.Data.SqlClient;

namespace Products.Functions.Data;

public interface ISqlConnectionFactory
{
    SqlConnection Create();
}

public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public SqlConnection Create() => new(connectionString);
}
