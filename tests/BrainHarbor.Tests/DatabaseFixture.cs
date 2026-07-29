using BrainHarbor.Web.Database;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// Migrates the test database exactly once per test run.
///
/// Every DB-touching test class boots the app (or connects directly), and app
/// startup runs DbUp in Development. xUnit runs classes in parallel, so
/// several DbUp upgrades could race the `schemaversions` journal —
/// `WithTransactionPerScript` takes no cross-process lock, so the losers fail
/// with "relation already exists". Serializing them behind one collection
/// removes the race for good.
/// </summary>
public sealed class DatabaseFixture
{
    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
           $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

    public DatabaseFixture()
    {
        // MigrationRunner CREATES the database if absent, and these tests
        // delete rows. Refuse to point at anything that isn't obviously a
        // local/test database — a stray BRAINHARBOR_TEST_DB must not migrate
        // and mutate a real environment.
        var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
        var isLocal = builder.Host is "localhost" or "127.0.0.1" or "::1" or "postgres";
        var looksLikeTestDb = builder.Database?.Contains("test", StringComparison.OrdinalIgnoreCase) == true;

        if (!isLocal && !looksLikeTestDb)
        {
            throw new InvalidOperationException(
                $"Refusing to run migrations against host '{builder.Host}' database " +
                $"'{builder.Database}'. Tests only run against a local or *test* database.");
        }

        // Tests that open their own connections need the same Dapper type
        // handlers the app registers at startup.
        BrainHarbor.Web.Services.DapperTypeHandlers.Register();

        MigrationRunner.Run(ConnectionString);
    }
}

/// <summary>
/// Test classes that touch Postgres join this collection: one migration, and
/// no two DB classes running at the same time.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "database";
}
