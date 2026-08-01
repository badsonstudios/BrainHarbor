using System.Net.Http.Json;
using System.Text.Json;
using BrainHarbor.Web.Api;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-402: trial FACTS crossing the sync boundary into trials_cache.
///
/// Facts have their own endpoint because they obey the opposite rule to a
/// summary. A summary is editorial — gated by the automated checks, editable
/// and rejectable by a person, frozen once reviewed. A trial's status is a
/// fact: it must refresh on every run whatever anyone decided about the
/// summary, because a trial shown as "Recruiting" after it closed sends a
/// patient to a door that no longer opens.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public class TrialsSyncTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestKey = "test-sync-key-0123456789abcdef";
    private const string TestSource = "test_sync";
    private const string Nct = "NCT99999001";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;

    public TrialsSyncTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
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
            "DELETE FROM source_sync_state WHERE source = @TestSource; " +
            "DELETE FROM trials_cache WHERE nct_id LIKE 'NCT9999%'",
            new { TestSource });
    }

    private HttpClient AuthedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(SyncApiKeyFilter.HeaderName, TestKey);
        return client;
    }

    private static TrialFacts Facts(string status = "Recruiting", string nctId = Nct) => new()
    {
        NctId = nctId,
        Title = "A study of a new pill for glioblastoma",
        Summary = "This trial tests a new pill.",
        Conditions = ["Glioblastoma", "Brain Neoplasms"],
        Phase = "Phase 2",
        OverallStatus = status,
        LastUpdatePosted = new DateOnly(2026, 7, 20),
        Locations =
        [
            new TrialLocation("Ohio State Wexner Medical Center", "Columbus", "Ohio",
                "United States", 39.9612, -82.9988),
        ],
    };

    private Task<HttpResponseMessage> PostAsync(params TrialFacts[] trials) =>
        AuthedClient().PostAsJsonAsync("/api/sync/trials", new TrialsRequest(trials));

    private async Task<(string? Status, string? Phase, string[] Conditions, string? Summary,
        string Locations, DateOnly? LastUpdate)> ReadCacheAsync(string nctId = Nct)
    {
        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        return await connection.QuerySingleAsync<(string?, string?, string[], string?, string, DateOnly?)>(
            """
            SELECT overall_status, phase, conditions, summary,
                   locations::text, last_update_posted
            FROM trials_cache WHERE nct_id = @nctId
            """,
            new { nctId });
    }

    [Fact]
    public async Task TrialFactsAreStored()
    {
        (await PostAsync(Facts())).EnsureSuccessStatusCode();

        var row = await ReadCacheAsync();

        Assert.Equal("Recruiting", row.Status);
        Assert.Equal("Phase 2", row.Phase);
        Assert.Equal(["Glioblastoma", "Brain Neoplasms"], row.Conditions);
        Assert.Equal("This trial tests a new pill.", row.Summary);
        Assert.Equal(new DateOnly(2026, 7, 20), row.LastUpdate);
    }

    [Fact]
    public async Task SiteCoordinatesRoundTripThroughTheCache()
    {
        // The stored shape is a contract — the trial pages read these names.
        await PostAsync(Facts());

        var row = await ReadCacheAsync();
        using var locations = JsonDocument.Parse(row.Locations);
        var site = locations.RootElement[0];

        Assert.Equal("Columbus", site.GetProperty("city").GetString());
        Assert.Equal("United States", site.GetProperty("country").GetString());
        Assert.Equal(39.9612, site.GetProperty("lat").GetDouble(), 4);
        Assert.Equal(-82.9988, site.GetProperty("lon").GetDouble(), 4);
    }

    [Fact]
    public async Task AClosedTrialStopsBeingAdvertisedAsOpenEvenAfterAHumanFrozeTheItem()
    {
        // The reason facts have their own door. A person approving the summary
        // must not also freeze "is this still open?".
        var client = AuthedClient();
        await PostAsync(Facts());
        await client.PostAsJsonAsync("/api/sync/items", new UploadRequest(
        [
            new SyncItem
            {
                Source = TestSource,
                SourceKind = "trial_update",
                ExternalId = Nct,
                Title = "A study of a new pill for glioblastoma",
                Url = $"https://clinicaltrials.gov/study/{Nct}",
                Relevance = "patient_relevant",
                ResearchStage = "human_trial",
                PlainSummary = "A trial is testing a new pill.",
            },
        ], null));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        await connection.ExecuteAsync(
            """
            UPDATE aggregated_items
               SET status = 'published', reviewed_by = 'dan', reviewed_at = now()
             WHERE source = @TestSource AND external_id = @Nct
            """,
            new { TestSource, Nct });

        // The trial closes.
        (await PostAsync(Facts(status: "Completed"))).EnsureSuccessStatusCode();

        Assert.Equal("Completed", (await ReadCacheAsync()).Status);

        // ...and the reviewed summary is untouched.
        var summary = await connection.ExecuteScalarAsync<string>(
            "SELECT plain_summary FROM aggregated_items WHERE source = @TestSource AND external_id = @Nct",
            new { TestSource, Nct });
        Assert.Equal("A trial is testing a new pill.", summary);
    }

    [Fact]
    public async Task TheCacheHoldsNoPlainLanguageText()
    {
        // Editorial text lives on aggregated_items, where the review queue, the
        // safety checks and the reader's problem report can all reach it. A
        // second copy here would be an ungated door to the reader for exactly
        // the prose the safety system held back.
        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var columns = await connection.QueryAsync<string>(
            "SELECT column_name FROM information_schema.columns WHERE table_name = 'trials_cache'");

        Assert.DoesNotContain("plain_summary", columns);
    }

    [Fact]
    public async Task ARepeatedRefreshUpdatesInPlaceRatherThanDuplicating()
    {
        await PostAsync(Facts());
        await PostAsync(Facts(status: "Active, not recruiting"));

        await using var connection = new NpgsqlConnection(_database.ConnectionString);
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM trials_cache WHERE nct_id = @Nct", new { Nct });

        Assert.Equal(1, count);
        Assert.Equal("Active, not recruiting", (await ReadCacheAsync()).Status);
    }

    // ---------- validation at the trust boundary ----------

    [Theory]
    [InlineData(91.0, -82.9)]
    [InlineData(39.9, -181.0)]
    public async Task AnImpossibleCoordinateIsRejectedRatherThanPuttingASiteInTheSea(double lat, double lon)
    {
        var bad = Facts() with
        {
            Locations = [new TrialLocation("Somewhere", "Nowhere", null, "United States", lat, lon)],
        };

        var body = await (await PostAsync(bad)).Content.ReadFromJsonAsync<TrialsResponse>();

        Assert.Equal(1, body!.Rejected);
        Assert.Equal(0, body.Stored);
        Assert.Contains(body.Errors, e => e.Contains("out of range", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AFarFutureUpdateDateIsRejectedSoItCannotPinTheBrowseList()
    {
        var bad = Facts() with { LastUpdatePosted = new DateOnly(2099, 1, 1) };

        var body = await (await PostAsync(bad)).Content.ReadFromJsonAsync<TrialsResponse>();

        Assert.Equal(1, body!.Rejected);
    }

    [Fact]
    public void NullCollectionsAreAPerItemErrorNotACrash()
    {
        // Minimal APIs don't enforce non-nullable reference types when binding
        // JSON, so `{"conditions":null}` arrives as a real null. Left
        // unguarded it would be a 500 that rolls back the whole batch.
        var withNulls = new TrialFacts
        {
            NctId = Nct,
            Title = "A trial",
            Conditions = null!,
            Locations = null!,
        };

        Assert.Null(SyncRepository.ValidateTrial(withNulls));   // tolerated, not a crash

        Assert.NotNull(SyncRepository.ValidateTrial(
            withNulls with { Locations = [null!] }));
    }

    [Fact]
    public async Task ABadRecordInABatchDoesNotStopTheGoodOnes()
    {
        var body = await (await PostAsync(
                Facts(nctId: "NCT99999010"),
                Facts(nctId: "  "),
                Facts(nctId: "NCT99999011")))
            .Content.ReadFromJsonAsync<TrialsResponse>();

        Assert.Equal(2, body!.Stored);
        Assert.Equal(1, body.Rejected);
    }

    [Fact]
    public async Task AnEmptyOrMalformedBodyIsACleanBadRequest()
    {
        var client = AuthedClient();

        Assert.Equal(System.Net.HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/sync/trials", new { })).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync("/api/sync/trials", new TrialsRequest([]))).StatusCode);
    }
}
