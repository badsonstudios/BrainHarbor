using Dapper;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-201: the aggregated_items schema, and the safety rules the database
/// enforces so no future caller can bypass them (content-pipeline.md §9).
/// Runs against the migrated dev database.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public class SchemaTests(DatabaseFixture database) : IAsyncLifetime
{
    private NpgsqlConnection _connection = null!;

    public async Task InitializeAsync()
    {
        // Migrations already ran once in DatabaseFixture.
        _connection = new NpgsqlConnection(database.ConnectionString);
        await _connection.OpenAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.ExecuteAsync("DELETE FROM aggregated_items WHERE source = 'test_source'");
        await _connection.DisposeAsync();
    }

    private static string NewExternalId() => "test-" + Guid.NewGuid().ToString("N");

    private Task<int> InsertAsync(
        string sourceKind = "research",
        string relevance = "pending",
        string status = "pending",
        string? researchStage = null) =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, relevance, status, research_stage)
            VALUES
                ('test_source', @sourceKind, @externalId, 'Title', 'https://example.org',
                 @relevance, @status, @researchStage)
            """,
            new { sourceKind, externalId = NewExternalId(), relevance, status, researchStage });

    [Fact]
    public async Task TablesExistWithTheDocumentedColumns()
    {
        var columns = (await _connection.QueryAsync<string>(
            "SELECT column_name FROM information_schema.columns WHERE table_name = 'aggregated_items'"))
            .ToHashSet();

        foreach (var expected in new[]
                 {
                     "id", "source", "source_kind", "external_id", "title", "raw_summary", "url",
                     "published_at", "fetched_at", "tumor_tags", "research_stage", "relevance",
                     "classify_model", "plain_title", "plain_summary", "summary_model",
                     "prompt_version", "summary_generated_at", "summary_flagged", "status",
                     "reviewed_at", "review_note", "slug",
                 })
        {
            Assert.Contains(expected, columns);
        }

        var syncColumns = (await _connection.QueryAsync<string>(
            "SELECT column_name FROM information_schema.columns WHERE table_name = 'source_sync_state'"))
            .ToHashSet();

        Assert.Equal(
            new HashSet<string> { "source", "last_success_at", "last_error", "cursor" },
            syncColumns);
    }

    [Fact]
    public async Task BothDocumentedIndexesExist()
    {
        var indexes = (await _connection.QueryAsync<string>(
            "SELECT indexname FROM pg_indexes WHERE tablename = 'aggregated_items'"))
            .ToHashSet();

        Assert.Contains("aggregated_items_status_published_idx", indexes);
        Assert.Contains("aggregated_items_tumor_tags_idx", indexes);
    }

    [Fact]
    public async Task UndatedItemsSortLastNotFirst()
    {
        // published_at is nullable and Postgres DESC defaults to NULLS FIRST,
        // which would float undated items to the top of the feed.
        var a = NewExternalId();
        var b = NewExternalId();
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items (source, source_kind, external_id, title, url, published_at, status)
            VALUES ('test_source', 'research', @a, 'Dated', 'https://example.org', DATE '2026-01-01', 'published'),
                   ('test_source', 'research', @b, 'Undated', 'https://example.org', NULL, 'published')
            """,
            new { a, b });

        var first = await _connection.QuerySingleAsync<string>(
            """
            SELECT title FROM aggregated_items
            WHERE source = 'test_source' AND status = 'published'
            ORDER BY published_at DESC NULLS LAST LIMIT 1
            """);

        Assert.Equal("Dated", first);
    }

    [Fact]
    public async Task ItemsDefaultToPendingAndUnpublished()
    {
        await InsertAsync();

        var row = await _connection.QuerySingleAsync<(string Status, string Relevance, bool Flagged)>(
            """
            SELECT status, relevance, summary_flagged
            FROM aggregated_items WHERE source = 'test_source' ORDER BY id DESC LIMIT 1
            """);

        Assert.Equal("pending", row.Status);
        Assert.Equal("pending", row.Relevance);
        Assert.False(row.Flagged);
    }

    [Fact]
    public async Task PreprintCanNeverBePatientRelevant()
    {
        // The rule from content-pipeline.md §9, enforced in the database so no
        // future code path can publish a preprint as patient-facing evidence.
        var exception = await Assert.ThrowsAsync<PostgresException>(
            () => InsertAsync(sourceKind: "preprint", relevance: "patient_relevant"));

        Assert.Contains("preprint_never_patient_relevant", exception.Message);
    }

    [Fact]
    public async Task PreprintMayBeEarlyStage()
    {
        await InsertAsync(sourceKind: "preprint", relevance: "early_stage");
    }

    [Theory]
    [InlineData("source_kind", "podcast")]
    [InlineData("relevance", "super_relevant")]
    [InlineData("status", "live")]
    [InlineData("research_stage", "vibes")]
    public async Task UndocumentedEnumValuesAreRejected(string column, string value)
    {
        var task = column switch
        {
            "source_kind" => InsertAsync(sourceKind: value),
            "relevance" => InsertAsync(relevance: value),
            "status" => InsertAsync(status: value),
            _ => InsertAsync(researchStage: value),
        };

        await Assert.ThrowsAsync<PostgresException>(() => task);
    }

    [Fact]
    public async Task SourceAndExternalIdAreUniqueTogether()
    {
        var externalId = NewExternalId();
        const string sql =
            """
            INSERT INTO aggregated_items (source, source_kind, external_id, title, url)
            VALUES ('test_source', 'research', @externalId, 'Title', 'https://example.org')
            """;

        await _connection.ExecuteAsync(sql, new { externalId });

        await Assert.ThrowsAsync<PostgresException>(
            () => _connection.ExecuteAsync(sql, new { externalId }));
    }

    [Fact]
    public async Task TumorTagsRoundTripAsAnArray()
    {
        var externalId = NewExternalId();
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items (source, source_kind, external_id, title, url, tumor_tags)
            VALUES ('test_source', 'research', @externalId, 'Title', 'https://example.org', @tags)
            """,
            new { externalId, tags = new[] { "glioma", "glioblastoma" } });

        var tags = await _connection.QuerySingleAsync<string[]>(
            "SELECT tumor_tags FROM aggregated_items WHERE source = 'test_source' AND external_id = @externalId",
            new { externalId });

        Assert.Equal(["glioma", "glioblastoma"], tags);
    }
}
