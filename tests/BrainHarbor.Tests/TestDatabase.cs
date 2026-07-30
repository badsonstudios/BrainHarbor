using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// The one place the test DB connection string is built. Every app host the
/// suite spins up gets its own Npgsql pool, and derived WebApplicationFactory
/// instances accumulate across a ~400-test run; uncapped, their idle
/// connections pile up until Postgres hits its 100-client limit
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
            ?? "Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;" +
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
