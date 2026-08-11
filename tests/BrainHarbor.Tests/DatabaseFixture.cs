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
///
/// <para><b>Dirty-database rule (the WI-402 lesson, one home for it):</b>
/// tests may not assume the tables hold only their own rows. The default
/// <c>brainharbor_test</c> database is usually clean, but a crashed run
/// leaves seeds behind (cleanup runs per-class), and BRAINHARBOR_TEST_DB may
/// deliberately point somewhere dirty. So a test that needs its rows on page
/// one of a DESC-sorted surface seeds far-future dates (2999-01-01), and a
/// test that cares about ORDER pages until it finds its own rows rather than
/// reading page 0 — see FeedTests.MyRowsInFeedOrderAsync.</para>
/// </summary>
public sealed class DatabaseFixture
{
    public string ConnectionString { get; } = TestDatabase.ConnectionString;

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
