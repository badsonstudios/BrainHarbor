using BrainHarbor.Pipeline.Publishing;
using Dapper;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-203 acceptance: the Pipeline's typed client against the REAL Web app
/// over real HTTP (Kestrel), not a stub. This is what pins the two
/// independently-declared contract copies together — a field renamed on
/// either side fails here.
/// </summary>
[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class SyncApiClientIntegrationTests : IClassFixture<KestrelWebApplicationFactory>, IAsyncLifetime
{
    private const string TestSource = "test_sync";

    private readonly KestrelWebApplicationFactory _factory;
    private readonly DatabaseFixture _database;
    private SyncApiClient _client = null!;

    public SyncApiClientIntegrationTests(KestrelWebApplicationFactory factory, DatabaseFixture database)
    {
        _factory = factory;
        _database = database;
    }

    public async Task InitializeAsync()
    {
        _factory.EnsureServer();

        var http = new HttpClient { BaseAddress = new Uri(_factory.ServerAddress) };
        http.DefaultRequestHeaders.Add("X-BrainHarbor-Key", KestrelWebApplicationFactory.SyncApiKey);
        _client = new SyncApiClient(http, NullLogger<SyncApiClient>.Instance);

        await CleanupAsync();
    }

    public Task DisposeAsync() => CleanupAsync();

    private async Task CleanupAsync()
    {
        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.ExecuteAsync(
            "DELETE FROM aggregated_items WHERE source = @TestSource; " +
            "DELETE FROM source_sync_state WHERE source = @TestSource; " +
            "DELETE FROM trials_cache WHERE nct_id LIKE 'NCT9999%'",
            new { TestSource });
    }

    private static SyncItem NewItem(string externalId) => new()
    {
        Source = TestSource,
        SourceKind = "research",
        ExternalId = externalId,
        Title = "A study of something",
        Url = "https://example.org/study",
        RawSummary = "Abstract.",
        PublishedAt = new DateOnly(2026, 6, 12),
        TumorTags = ["GBM"],
        Relevance = "patient_relevant",
        ResearchStage = "human_trial",
    };

    [Fact]
    public async Task FullRoundTripStateCheckUploadState()
    {
        // 1. state: nothing recorded for this source yet
        var before = await _client.GetStateAsync(CancellationToken.None);
        Assert.False(before.ContainsKey(TestSource));

        // 2. check: both keys are new
        var keys = new List<ItemKey> { new(TestSource, "rt-1"), new(TestSource, "rt-2") };
        var unseen = await _client.FindNewAsync(keys, CancellationToken.None);
        Assert.Equal(2, unseen.Count);

        // 3. upload
        var upload = await _client.UploadAsync(
            [NewItem("rt-1"), NewItem("rt-2")], "2026-06-12", CancellationToken.None);
        Assert.Equal(2, upload.Inserted);
        Assert.Equal(0, upload.Rejected);

        // 4. check again: nothing new now — this is the token-saving path
        var afterUpload = await _client.FindNewAsync(keys, CancellationToken.None);
        Assert.Empty(afterUpload);

        // 5. state reflects the cursor the client sent
        var after = await _client.GetStateAsync(CancellationToken.None);
        Assert.Equal("2026-06-12", after[TestSource].Cursor);
        Assert.NotNull(after[TestSource].LastSuccessAt);
    }

    [Fact]
    public async Task CursorAdvancesWithNothingToUpload()
    {
        // The bug this test exists for: the client's chunking loop makes NO
        // request for an empty item list, so cursor-only progress silently
        // did nothing and a source's window could never move forward.
        await _client.UploadAsync([NewItem("cur-1")], "window-1", CancellationToken.None);

        await _client.AdvanceCursorAsync(TestSource, "window-2", CancellationToken.None);

        var state = await _client.GetStateAsync(CancellationToken.None);
        Assert.Equal("window-2", state[TestSource].Cursor);
    }

    [Fact]
    public async Task AdvancingACursorForAnUnknownSourceIsAnError()
    {
        var exception = await Assert.ThrowsAsync<SyncApiException>(
            () => _client.AdvanceCursorAsync("not_a_source", "x", CancellationToken.None));

        Assert.Contains("400", exception.Message);
    }

    [Fact]
    public async Task EmptyUploadIsARejectedProgrammingErrorNotASilentNoOp()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.UploadAsync([], "window-x", CancellationToken.None));
    }

    [Fact]
    public async Task EveryContractFieldSurvivesTheRoundTrip()
    {
        // Pins the two independently-declared contract copies together: a
        // rename on either side fails here rather than in production.
        var full = new SyncItem
        {
            Source = TestSource,
            SourceKind = "research",
            ExternalId = "full-1",
            Title = "Original title",
            Url = "https://example.org/full",
            RawSummary = "Abstract text.",
            PublishedAt = new DateOnly(2026, 6, 12),
            TumorTags = ["glioblastoma"],
            ResearchStage = "human_trial",
            Relevance = "patient_relevant",
            ClassifyModel = "classify-model-v1",
            PlainTitle = "A plain title",
            PlainSummary = "A plain summary.",
            SummaryModel = "summary-model-v1",
            PromptVersion = "prompt-v3",
            SummaryFlagged = true,
        };

        await _client.UploadAsync([full], null, CancellationToken.None);

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var row = await connection.QuerySingleAsync(
            """
            SELECT source_kind, title, raw_summary, url, published_at, tumor_tags,
                   research_stage, relevance, classify_model, plain_title, plain_summary,
                   summary_model, prompt_version, summary_flagged, summary_generated_at, status
            FROM aggregated_items WHERE source = @TestSource AND external_id = 'full-1'
            """,
            new { TestSource });

        Assert.Equal("research", (string)row.source_kind);
        Assert.Equal("Original title", (string)row.title);
        Assert.Equal("Abstract text.", (string)row.raw_summary);
        Assert.Equal("https://example.org/full", (string)row.url);
        Assert.Equal(new DateOnly(2026, 6, 12), (DateOnly)row.published_at);
        Assert.Equal(["glioblastoma"], (string[])row.tumor_tags);
        Assert.Equal("human_trial", (string)row.research_stage);
        Assert.Equal("patient_relevant", (string)row.relevance);
        Assert.Equal("classify-model-v1", (string)row.classify_model);
        Assert.Equal("A plain title", (string)row.plain_title);
        Assert.Equal("A plain summary.", (string)row.plain_summary);
        Assert.Equal("summary-model-v1", (string)row.summary_model);
        Assert.Equal("prompt-v3", (string)row.prompt_version);
        Assert.True((bool)row.summary_flagged);
        Assert.NotNull(row.summary_generated_at);

        // And still gated behind human review.
        Assert.Equal("pending", (string)row.status);
    }

    [Fact]
    public async Task TheTrialFactsContractSurvivesTheRoundTripToo()
    {
        // WI-402: TrialFacts is declared twice, once per side. A rename or a
        // dropped field here would silently empty the trial finder, so it gets
        // the same pinning treatment as the item contract above.
        var facts = new TrialFacts
        {
            NctId = "NCT99990042",
            Title = "A study of a new pill for glioblastoma",
            Summary = "This trial tests a new pill.",
            Conditions = ["Glioblastoma"],
            Phase = "Phase 2",
            OverallStatus = "Recruiting",
            LastUpdatePosted = new DateOnly(2026, 7, 20),
            Locations =
            [
                new TrialLocation("A cancer center", "Columbus", "Ohio",
                    "United States", 39.9612, -82.9988),
            ],
        };

        var result = await _client.UploadTrialsAsync([facts], CancellationToken.None);
        Assert.Equal(1, result.Stored);

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var row = await connection.QuerySingleAsync(
            """
            SELECT overall_status, phase, conditions, last_update_posted,
                   locations->0->>'city' AS city,
                   (locations->0->>'lat')::float8 AS lat
            FROM trials_cache WHERE nct_id = 'NCT99990042'
            """);

        Assert.Equal("Recruiting", (string)row.overall_status);
        Assert.Equal("Phase 2", (string)row.phase);
        Assert.Equal(["Glioblastoma"], (string[])row.conditions);
        Assert.Equal(new DateOnly(2026, 7, 20), (DateOnly)row.last_update_posted);
        Assert.Equal("Columbus", (string)row.city);
        Assert.Equal(39.9612, (double)row.lat, 4);
    }

    [Fact]
    public async Task TumorTagAliasesAreNormalizedServerSide()
    {
        await _client.UploadAsync([NewItem("alias-1")], null, CancellationToken.None);

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var tags = await connection.ExecuteScalarAsync<string[]>(
            "SELECT tumor_tags FROM aggregated_items WHERE source = @TestSource AND external_id = 'alias-1'",
            new { TestSource });

        Assert.Equal(["glioblastoma"], tags);
    }

    [Fact]
    public async Task UploadedItemsAreNotVisibleToReadersUntilApproved()
    {
        await _client.UploadAsync([NewItem("gate-1")], null, CancellationToken.None);

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var status = await connection.ExecuteScalarAsync<string>(
            "SELECT status FROM aggregated_items WHERE source = @TestSource AND external_id = 'gate-1'",
            new { TestSource });

        Assert.Equal("pending", status);
    }

    [Fact]
    public async Task ABadKeyGivesAnActionableErrorNotAGenericFailure()
    {
        var http = new HttpClient { BaseAddress = new Uri(_factory.ServerAddress) };
        http.DefaultRequestHeaders.Add("X-BrainHarbor-Key", "wrong-key");
        var client = new SyncApiClient(http, NullLogger<SyncApiClient>.Instance);

        var exception = await Assert.ThrowsAsync<SyncApiException>(
            () => client.GetStateAsync(CancellationToken.None));

        Assert.Contains("user-secrets", exception.Message);
    }

    [Fact]
    public async Task ServerRejectionsSurfaceAsErrorsNotSilentDrops()
    {
        var badItem = NewItem("bad-1") with { SourceKind = "podcast" };

        var result = await _client.UploadAsync([badItem], null, CancellationToken.None);

        Assert.Equal(1, result.Rejected);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task BatchesLargerThanTheServerCapAreChunked()
    {
        // The client must split, not fail: the server rejects >500 per request.
        var items = Enumerable.Range(0, SyncApiClient.MaxBatchSize + 10)
            .Select(i => NewItem($"chunk-{i}"))
            .ToList();

        var result = await _client.UploadAsync(items, "chunked-cursor", CancellationToken.None);

        Assert.Equal(items.Count, result.Inserted);

        // The cursor rides only on the final chunk, so it isn't advanced past
        // items that haven't uploaded yet.
        var state = await _client.GetStateAsync(CancellationToken.None);
        Assert.Equal("chunked-cursor", state[TestSource].Cursor);
    }

    [Fact]
    public async Task CheckIsAlsoChunked()
    {
        var keys = Enumerable.Range(0, SyncApiClient.MaxBatchSize + 10)
            .Select(i => new ItemKey(TestSource, $"ck-{i}"))
            .ToList();

        var unseen = await _client.FindNewAsync(keys, CancellationToken.None);

        Assert.Equal(keys.Count, unseen.Count);
    }
}
