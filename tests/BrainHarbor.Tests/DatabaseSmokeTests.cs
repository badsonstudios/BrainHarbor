using BrainHarbor.Web.Database;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// Requires the local Postgres container (docker compose up -d) or the CI
/// service container. Connection string overridable via BRAINHARBOR_TEST_DB.
/// </summary>
public class DatabaseSmokeTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    [Fact]
    [Trait("Category", "Database")]
    public async Task MigrationsApplyAndDatabaseIsReachable()
    {
        // Idempotent: DbUp journals applied scripts, so re-running is a no-op.
        MigrationRunner.Run(ConnectionString);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "select count(*) from schemaversions where scriptname like '%0001_baseline%'", connection);
        var applied = (long)(await command.ExecuteScalarAsync() ?? 0L);

        Assert.True(applied >= 1, "0001_baseline.sql should be journaled in schemaversions.");
    }
}
