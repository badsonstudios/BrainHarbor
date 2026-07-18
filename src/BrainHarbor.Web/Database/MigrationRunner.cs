using System.Reflection;
using DbUp;

namespace BrainHarbor.Web.Database;

public static class MigrationRunner
{
    /// <summary>
    /// Creates the database if needed and applies pending SQL scripts embedded
    /// under Database/Scripts. Throws on failure — the app must not start
    /// against a half-migrated schema.
    /// </summary>
    public static void Run(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

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
}
