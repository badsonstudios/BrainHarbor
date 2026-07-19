using System.Net;
using System.Net.Http.Json;
using BrainHarbor.Web.Api;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-202: the sync API is the only write surface into the site
/// (architecture.md §4). These tests cover the security boundary, the
/// idempotency guarantee, and the medical-safety rules that must hold no
/// matter what the pipeline uploads.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public class SyncApiTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestKey = "test-sync-key-0123456789abcdef";
    private const string TestSource = "test_sync";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;

    public SyncApiTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
    {
        _database = database;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:BrainHarbor", database.ConnectionString);
            builder.UseSetting("SYNC_API_KEY", TestKey);
        });
    }

    public Task InitializeAsync() => CleanupAsync();

    public Task DisposeAsync() => CleanupAsync();

    private async Task CleanupAsync()
    {
        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.ExecuteAsync(
            "DELETE FROM aggregated_items WHERE source = @TestSource; " +
            "DELETE FROM source_sync_state WHERE source = @TestSource",
            new { TestSource });
    }

    private HttpClient AuthedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(SyncApiKeyFilter.HeaderName, TestKey);
        return client;
    }

    private static SyncItem NewItem(
        string externalId,
        string title = "A study of something",
        string sourceKind = "research",
        string? relevance = "patient_relevant",
        IReadOnlyList<string>? tags = null) => new()
        {
            Source = TestSource,
            SourceKind = sourceKind,
            ExternalId = externalId,
            Title = title,
            Url = "https://example.org/study",
            RawSummary = "Abstract text.",
            PublishedAt = new DateOnly(2026, 6, 12),
            TumorTags = tags ?? ["glioblastoma"],
            ResearchStage = "human_trial",
            Relevance = relevance,
            ClassifyModel = "test-model",
        };

    // ---------- security boundary ----------

    [Theory]
    [InlineData("/api/sync/state")]
    public async Task GetEndpointsReturn401WithoutTheKey(string url)
    {
        var response = await _factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostEndpointsReturn401WithoutTheKey()
    {
        var client = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/sync/check", new CheckRequest([]))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/sync/items",
                new UploadRequest([NewItem("x")], null))).StatusCode);
    }

    [Fact]
    public async Task WrongKeyReturns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(SyncApiKeyFilter.HeaderName, "not-the-key");

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/sync/state")).StatusCode);
    }

    [Fact]
    public async Task UnconfiguredKeyFailsClosedRatherThanOpen()
    {
        // An unset SYNC_API_KEY must never mean "anyone may write".
        var unconfigured = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:BrainHarbor", _database.ConnectionString);
            builder.UseSetting("SYNC_API_KEY", "");
        });

        var response = await unconfigured.CreateClient().GetAsync("/api/sync/state");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    // ---------- state ----------

    [Fact]
    public async Task StateReturnsPerSourceCursors()
    {
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("state-1")], "2026-06-12"));

        var state = await client.GetFromJsonAsync<SyncStateResponse>("/api/sync/state");

        var source = Assert.Single(state!.Sources.Where(s => s.Source == TestSource));
        Assert.Equal("2026-06-12", source.Cursor);
        Assert.NotNull(source.LastSuccessAt);
        Assert.Null(source.LastError);
    }

    // ---------- check ----------

    [Fact]
    public async Task CheckReturnsOnlyTheKeysNotAlreadyStored()
    {
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("known-1")], null));

        var response = await client.PostAsJsonAsync("/api/sync/check", new CheckRequest(
        [
            new ItemKey(TestSource, "known-1"),
            new ItemKey(TestSource, "brand-new-1"),
        ]));

        var body = await response.Content.ReadFromJsonAsync<CheckResponse>();
        var newKey = Assert.Single(body!.New);
        Assert.Equal("brand-new-1", newKey.ExternalId);
    }

    [Fact]
    public async Task CheckRejectsOversizedBatches()
    {
        var keys = Enumerable.Range(0, SyncEndpoints.MaxBatchSize + 1)
            .Select(i => new ItemKey(TestSource, $"k{i}")).ToList();

        var response = await AuthedClient().PostAsJsonAsync("/api/sync/check", new CheckRequest(keys));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- upload + idempotency (the acceptance criterion) ----------

    [Fact]
    public async Task UploadingTheSameBatchTwiceIsIdempotent()
    {
        var client = AuthedClient();
        var batch = new UploadRequest([NewItem("dupe-1"), NewItem("dupe-2")], null);

        var first = await (await client.PostAsJsonAsync("/api/sync/items", batch))
            .Content.ReadFromJsonAsync<UploadResponse>();
        var second = await (await client.PostAsJsonAsync("/api/sync/items", batch))
            .Content.ReadFromJsonAsync<UploadResponse>();

        Assert.Equal(2, first!.Inserted);
        Assert.Equal(0, second!.Inserted);
        Assert.Equal(2, second.Updated);

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM aggregated_items WHERE source = @TestSource", new { TestSource });
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ItemsAlwaysLandAsPendingNeverPublished()
    {
        await AuthedClient().PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("pending-1")], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var status = await connection.ExecuteScalarAsync<string>(
            "SELECT status FROM aggregated_items WHERE source = @TestSource AND external_id = 'pending-1'",
            new { TestSource });

        Assert.Equal("pending", status);
    }

    [Fact]
    public async Task ReuploadDoesNotResurrectARejectedItemOrUnpublishALiveOne()
    {
        // The human review decision must win over any later pipeline rerun.
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("reviewed-1"), NewItem("reviewed-2")], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.ExecuteAsync(
            """
            UPDATE aggregated_items SET status = 'rejected'
             WHERE source = @TestSource AND external_id = 'reviewed-1';
            UPDATE aggregated_items SET status = 'published'
             WHERE source = @TestSource AND external_id = 'reviewed-2'
            """,
            new { TestSource });

        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("reviewed-1"), NewItem("reviewed-2")], null));

        var statuses = await connection.QueryAsync<(string ExternalId, string Status)>(
            "SELECT external_id, status FROM aggregated_items WHERE source = @TestSource ORDER BY external_id",
            new { TestSource });

        Assert.Equal([("reviewed-1", "rejected"), ("reviewed-2", "published")], statuses);
    }

    [Fact]
    public async Task ReuploadCannotNullTheSummaryOfAPublishedItem()
    {
        // The blocker this test exists for: a classify-only rerun (no summary
        // in the payload) must not leave a LIVE patient page contentless.
        var client = AuthedClient();
        var withSummary = NewItem("live-1") with
        {
            PlainTitle = "A pill slowed tumor growth",
            PlainSummary = "In a large trial, people went longer before their tumor grew.",
            SummaryModel = "test-model",
            PromptVersion = "v1",
        };
        await client.PostAsJsonAsync("/api/sync/items", new UploadRequest([withSummary], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.ExecuteAsync(
            "UPDATE aggregated_items SET status = 'published' WHERE source = @TestSource AND external_id = 'live-1'",
            new { TestSource });

        // Rerun without any summary fields.
        var response = await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("live-1")], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();
        Assert.Equal(1, body!.Frozen);

        var row = await connection.QuerySingleAsync<(string Status, string? PlainSummary)>(
            "SELECT status, plain_summary FROM aggregated_items WHERE source = @TestSource AND external_id = 'live-1'",
            new { TestSource });

        Assert.Equal("published", row.Status);
        Assert.Equal("In a large trial, people went longer before their tumor grew.", row.PlainSummary);
    }

    [Fact]
    public async Task PendingItemKeepsAnEarlierSummaryWhenALaterRunOmitsIt()
    {
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("keep-1") with { PlainSummary = "Original plain summary." }], null));
        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("keep-1")], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var summary = await connection.ExecuteScalarAsync<string>(
            "SELECT plain_summary FROM aggregated_items WHERE source = @TestSource AND external_id = 'keep-1'",
            new { TestSource });

        Assert.Equal("Original plain summary.", summary);
    }

    [Fact]
    public async Task ReaderReportedFlagSurvivesAReupload()
    {
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/items", new UploadRequest([NewItem("flag-1")], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.ExecuteAsync(
            "UPDATE aggregated_items SET summary_flagged = true WHERE source = @TestSource AND external_id = 'flag-1'",
            new { TestSource });

        await client.PostAsJsonAsync("/api/sync/items", new UploadRequest([NewItem("flag-1")], null));

        var flagged = await connection.ExecuteScalarAsync<bool>(
            "SELECT summary_flagged FROM aggregated_items WHERE source = @TestSource AND external_id = 'flag-1'",
            new { TestSource });

        Assert.True(flagged);
    }

    [Fact]
    public async Task CursorDoesNotAdvanceWhenEveryItemWasRejected()
    {
        // Otherwise the pipeline skips that window forever — silent data loss.
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("cursor-good")], "window-1"));

        await client.PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("cursor-bad", sourceKind: "podcast")], "window-2"));

        var state = await client.GetFromJsonAsync<SyncStateResponse>("/api/sync/state");
        var source = Assert.Single(state!.Sources.Where(s => s.Source == TestSource));

        Assert.Equal("window-1", source.Cursor);
    }

    [Fact]
    public async Task ABatchCarryingACursorMustComeFromOneSource()
    {
        var mixed = new UploadRequest(
            [NewItem("multi-1"), NewItem("multi-2") with { Source = "pubmed" }], "window-x");

        var body = await (await AuthedClient().PostAsJsonAsync("/api/sync/items", mixed))
            .Content.ReadFromJsonAsync<UploadResponse>();

        Assert.Equal(0, body!.Inserted);
        Assert.Contains(body.Errors, e => e.Contains("single source"));
    }

    [Fact]
    public async Task DuplicateKeysWithinOneBatchCollapseToOneRow()
    {
        var body = await (await AuthedClient().PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("dup-key", title: "First"), NewItem("dup-key", title: "Second")], null)))
            .Content.ReadFromJsonAsync<UploadResponse>();

        Assert.Equal(1, body!.Inserted);
        Assert.Equal(0, body.Updated);

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var title = await connection.ExecuteScalarAsync<string>(
            "SELECT title FROM aggregated_items WHERE source = @TestSource AND external_id = 'dup-key'",
            new { TestSource });
        Assert.Equal("Second", title);
    }

    [Fact]
    public async Task ReuploadRefreshesTheContentFields()
    {
        var client = AuthedClient();
        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("update-1", title: "Old title")], null));
        await client.PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("update-1", title: "Corrected title")], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var title = await connection.ExecuteScalarAsync<string>(
            "SELECT title FROM aggregated_items WHERE source = @TestSource AND external_id = 'update-1'",
            new { TestSource });

        Assert.Equal("Corrected title", title);
    }

    // ---------- medical-safety rules ----------

    [Fact]
    public async Task PreprintMarkedPatientRelevantIsRejectedWithAClearMessage()
    {
        var response = await AuthedClient().PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("preprint-1", sourceKind: "preprint", relevance: "patient_relevant")], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();

        Assert.Equal(1, body!.Rejected);
        Assert.Equal(0, body.Inserted);
        Assert.Contains(body.Errors, e => e.Contains("preprint", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExcludedItemsAreRejectedRatherThanStored()
    {
        var response = await AuthedClient().PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("excluded-1", relevance: "excluded")], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();
        Assert.Equal(1, body!.Rejected);
    }

    [Fact]
    public async Task InventedTumorTagsAreDroppedAndReported()
    {
        var response = await AuthedClient().PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("tags-1", tags: ["glioblastoma", "dragonoma"])], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();
        Assert.Contains("dragonoma", body!.RejectedTumorTags);

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var tags = await connection.ExecuteScalarAsync<string[]>(
            "SELECT tumor_tags FROM aggregated_items WHERE source = @TestSource AND external_id = 'tags-1'",
            new { TestSource });

        Assert.Equal(["glioblastoma"], tags);
    }

    [Fact]
    public async Task AliasedTumorTagsAreNormalizedToCanonicalSlugs()
    {
        await AuthedClient().PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("tags-2", tags: ["GBM"])], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var tags = await connection.ExecuteScalarAsync<string[]>(
            "SELECT tumor_tags FROM aggregated_items WHERE source = @TestSource AND external_id = 'tags-2'",
            new { TestSource });

        Assert.Equal(["glioblastoma"], tags);
    }

    [Theory]
    [InlineData("sourceKind", "podcast")]
    [InlineData("relevance", "super_relevant")]
    [InlineData("researchStage", "vibes")]
    public async Task UndocumentedEnumValuesAreRejectedPerItem(string field, string value)
    {
        var item = field switch
        {
            "sourceKind" => NewItem("bad-1", sourceKind: value),
            "relevance" => NewItem("bad-1", relevance: value),
            _ => NewItem("bad-1") with { ResearchStage = value },
        };

        var response = await AuthedClient().PostAsJsonAsync(
            "/api/sync/items", new UploadRequest([item], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();
        Assert.Equal(1, body!.Rejected);
    }

    [Fact]
    public async Task OneBadItemDoesNotBlockTheGoodOnesInTheSameBatch()
    {
        var response = await AuthedClient().PostAsJsonAsync("/api/sync/items", new UploadRequest(
            [NewItem("mixed-good"), NewItem("mixed-bad", sourceKind: "podcast")], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();

        Assert.Equal(1, body!.Inserted);
        Assert.Equal(1, body.Rejected);
    }

    [Fact]
    public async Task NonHttpUrlsAreRejected()
    {
        var item = NewItem("url-1") with { Url = "javascript:alert(1)" };

        var response = await AuthedClient().PostAsJsonAsync(
            "/api/sync/items", new UploadRequest([item], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();
        Assert.Equal(1, body!.Rejected);
    }

    [Fact]
    public async Task ApiErrorsAreNotHtmlErrorPages()
    {
        // The status-code-pages middleware used to re-execute API 401s into
        // the HTML page — a machine client got markup, and a POST re-execute
        // had no handler at all and surfaced as a bogus 400.
        var response = await _factory.CreateClient().GetAsync("/api/sync/state");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task MalformedBodiesAreCleanBadRequestsNotServerErrors()
    {
        var client = AuthedClient();

        foreach (var (url, json) in new[]
                 {
                     ("/api/sync/check", "{}"),
                     ("/api/sync/items", "{}"),
                     ("/api/sync/items", """{"items":[null]}"""),
                 })
        {
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task NullTumorTagsDoNotCrashTheUpload()
    {
        var content = new StringContent(
            """
            {"items":[{"source":"test_sync","sourceKind":"research","externalId":"nulltags-1",
             "title":"T","url":"https://example.org","tumorTags":null}],"cursor":null}
            """,
            System.Text.Encoding.UTF8, "application/json");

        var response = await AuthedClient().PostAsync("/api/sync/items", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("undefined")]
    [InlineData("pubmed_typo")]
    public async Task UndocumentedSourcesAreRejected(string source)
    {
        var response = await AuthedClient().PostAsJsonAsync("/api/sync/items",
            new UploadRequest([NewItem("src-1") with { Source = source }], null));

        var body = await response.Content.ReadFromJsonAsync<UploadResponse>();
        Assert.Equal(1, body!.Rejected);
    }

    [Fact]
    public async Task OversizedFieldsAndImpossibleDatesAreRejected()
    {
        var client = AuthedClient();

        var longTitle = NewItem("big-title") with { Title = new string('x', 1001) };
        var futureDate = NewItem("future-1") with { PublishedAt = new DateOnly(3000, 1, 1) };

        foreach (var item in new[] { longTitle, futureDate })
        {
            var body = await (await client.PostAsJsonAsync(
                    "/api/sync/items", new UploadRequest([item], null)))
                .Content.ReadFromJsonAsync<UploadResponse>();

            Assert.Equal(1, body!.Rejected);
        }
    }

    [Fact]
    public async Task KeyComparisonRejectsPrefixesAndPaddedVariants()
    {
        foreach (var wrong in new[]
                 { TestKey[..^1], TestKey + "x", " " + TestKey, TestKey.ToUpperInvariant() })
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(SyncApiKeyFilter.HeaderName, wrong);

            Assert.Equal(HttpStatusCode.Unauthorized,
                (await client.GetAsync("/api/sync/state")).StatusCode);
        }
    }

    [Fact]
    public async Task EmptyAndOversizedUploadsAreRejected()
    {
        var client = AuthedClient();

        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/sync/items", new UploadRequest([], null))).StatusCode);

        var tooMany = Enumerable.Range(0, SyncEndpoints.MaxBatchSize + 1)
            .Select(i => NewItem($"big-{i}")).ToList();
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/sync/items", new UploadRequest(tooMany, null))).StatusCode);
    }
}
