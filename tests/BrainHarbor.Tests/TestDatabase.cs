using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// The one place the test DB connection string is built.
///
/// The default is <c>brainharbor_test</c> — a database of its own in the same
/// local/CI Postgres server (WI-411), so test seeds never share tables with
/// the rows the real pipeline puts in the dev <c>brainharbor</c> database.
/// Nothing creates it by hand: <see cref="DatabaseFixture"/> runs the app's
/// MigrationRunner, whose EnsureDatabase creates it on first use.
/// <c>BRAINHARBOR_TEST_DB</c> still overrides the whole string (the fixture
/// refuses anything that is not local or obviously a test database).
///
/// Every app host the suite spins up gets its own Npgsql pool, and derived
/// WebApplicationFactory instances accumulate across the run; uncapped, their
/// idle connections pile up until Postgres hits its 100-client limit
/// ("53300: too many clients already"), which surfaced as a flaky E2E
/// failure. Capping each pool small and pruning idle connections quickly keeps
/// the total well under the limit no matter how many hosts are live.
/// </summary>
public static class TestDatabase
{
    public static string ConnectionString { get; } = Build();

    private static string Build()
    {
        var baseConnectionString =
            Environment.GetEnvironmentVariable("BRAINHARBOR_TEST_DB")
            ?? "Host=localhost;Port=5433;Database=brainharbor_test;Username=brainharbor;" +
               $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "brainharbor_dev"}";

        return new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            MaxPoolSize = 5,
            MinPoolSize = 0,
            ConnectionIdleLifetime = 5,      // seconds — shed idle connections fast
            ConnectionPruningInterval = 1,
        }.ConnectionString;
    }
}
