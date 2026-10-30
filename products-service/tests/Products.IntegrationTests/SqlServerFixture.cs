using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace Products.IntegrationTests;

public sealed partial class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using (var master = new SqlConnection(_container.GetConnectionString()))
        {
            await master.OpenAsync();
            await new SqlCommand("CREATE DATABASE ProductsDb", master).ExecuteNonQueryAsync();
        }

        ConnectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "ProductsDb"
        }.ConnectionString;

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        var scripts = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "sql"), "0*.sql").Order();
        foreach (var script in scripts)
        {
            foreach (var batch in GoSeparator().Split(await File.ReadAllTextAsync(script)).Where(b => !string.IsNullOrWhiteSpace(b)))
                await new SqlCommand(batch, connection).ExecuteNonQueryAsync();
        }
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoSeparator();
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "sql";
}
