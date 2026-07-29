using BrainHarbor.Web.Services;
using Dapper;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// Requires the local Postgres container (docker compose up -d) or the CI
/// service container.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class ConnectionFactoryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    [Fact]
    [Trait("Category", "Database")]
    public async Task FactoryOpensConnectionUsableByDapper()
    {
        await using var dataSource = NpgsqlDataSource.Create(ConnectionString);
        var factory = new NpgsqlConnectionFactory(dataSource);

        await using var connection = await factory.OpenConnectionAsync();
        var result = await connection.ExecuteScalarAsync<int>("select 1");

        Assert.Equal(1, result);
    }
}
