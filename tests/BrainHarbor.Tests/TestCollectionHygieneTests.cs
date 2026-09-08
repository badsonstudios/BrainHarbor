using System.Reflection;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-517. Every test class that stands up a <see cref="WebApplicationFactory{T}"/>
/// touches the one dev database, because the host runs DbUp migrations on
/// start. xUnit runs each class as its own collection unless told otherwise, so
/// a class without <c>[Collection(DatabaseCollection.Name)]</c> runs in
/// PARALLEL with every other one — and two fixtures reaching
/// <c>CREATE DATABASE</c> at the same moment is
/// <c>23505: duplicate key value violates unique constraint
/// pg_database_datname_index</c>.
///
/// **This is the test that would have caught it.** Three render classes shipped
/// without the attribute across WI-514, WI-515 and WI-516 and nothing noticed,
/// because the race needs enough parallel classes to become likely. A fourth
/// arrived at WI-517 and CI came back with **397 failures**, none of them about
/// content. Locally it read as flake — a different test failed on each full run
/// and every one of them passed in isolation. **A suite that fails somewhere
/// else each time it runs is not flaky, it is contended.**
///
/// Reflection rather than a text scan, deliberately: the attribute can be
/// inherited or applied through a base class, and a regex over source files
/// would report a false failure for either.
/// </summary>
public class TestCollectionHygieneTests
{
    [Fact]
    public void EveryClassThatStartsAWebHostIsInTheDatabaseCollection()
    {
        var offenders = typeof(TestCollectionHygieneTests).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(UsesAWebHost)
            // xUnit's CollectionAttribute exposes the name only through its
            // constructor argument, not as a property.
            .Where(t => !t.GetCustomAttributesData()
                .Where(a => a.AttributeType == typeof(CollectionAttribute))
                .SelectMany(a => a.ConstructorArguments)
                .Any(arg => (arg.Value as string) == DatabaseCollection.Name))
            .Select(t => t.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        Assert.True(offenders.Count == 0,
            "these test classes stand up a WebApplicationFactory without "
            + $"[Collection({nameof(DatabaseCollection)}.Name)], so they run in parallel with "
            + "every other database-touching class and race DbUp's CREATE DATABASE:\n  "
            + string.Join("\n  ", offenders));
    }

    /// <summary>
    /// True when the class takes a web-host fixture, however it is spelled —
    /// <c>IClassFixture&lt;WebApplicationFactory&lt;Program&gt;&gt;</c>,
    /// <c>KestrelWebApplicationFactory</c>, or a subclass of either.
    /// </summary>
    private static bool UsesAWebHost(Type type) =>
        type.GetInterfaces()
            .Where(i => i.IsGenericType)
            .SelectMany(i => i.GetGenericArguments())
            .Any(IsAWebHostFixture)
        || type.GetConstructors()
            .SelectMany(c => c.GetParameters())
            .Any(p => IsAWebHostFixture(p.ParameterType));

    private static bool IsAWebHostFixture(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType
                && current.GetGenericTypeDefinition() == typeof(WebApplicationFactory<>))
            {
                return true;
            }
        }

        return false;
    }
}
