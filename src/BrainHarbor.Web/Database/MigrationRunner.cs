using System.Reflection;
using DbUp;
using Npgsql;

namespace BrainHarbor.Web.Database;

public static class MigrationRunner
{
    // Arbitrary app-wide key for the advisory lock. Any instance running
    // migrations uses the same one.
    private const long AdvisoryLockKey = 727201;

    /// <summary>
    /// Creates the database if needed and applies pending SQL scripts embedded
    /// under Database/Scripts. Throws on failure — the app must not start
    /// against a half-migrated schema.
    ///
    /// Serialized by a Postgres advisory lock: DbUp's WithTransactionPerScript
    /// takes no cross-process lock, so two instances starting together (an
    /// Azure scale-out or slot swap from M4, or parallel test hosts) would
    /// race the schemaversions journal and the loser would fail to boot.
    /// </summary>
    public static void Run(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        using var lockConnection = new NpgsqlConnection(connectionString);
        lockConnection.Open();

        using (var acquire = lockConnection.CreateCommand())
        {
            acquire.CommandText = $"SELECT pg_advisory_lock({AdvisoryLockKey})";
            acquire.ExecuteNonQuery();
        }

        try
        {
            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransactionPerScript()
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException(
                    $"Database migration failed at script '{result.ErrorScript?.Name}'.", result.Error);
            }
        }
        finally
        {
            using var release = lockConnection.CreateCommand();
            release.CommandText = $"SELECT pg_advisory_unlock({AdvisoryLockKey})";
            release.ExecuteNonQuery();
        }
    }
}
