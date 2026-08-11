using System.Net;
using BrainHarbor.Web.Content;
using BrainHarbor.Web.Services;
using BrainHarbor.Web.Trials;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-403: the trial finder. The rules that matter to a reader:
///   * browse shows trials you could still JOIN unless you ask otherwise;
///   * a trial page shows the status the trial has NOW;
///   * plain-language text only ever comes from a PUBLISHED item, never a
///     pending or rejected one;
///   * the registry's own words are labelled as the registry's, not ours.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class TrialsPageTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;
    private NpgsqlConnection _connection = null!;
    private TrialsRepository _trials = null!;

    public TrialsPageTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
    {
        _database = database;
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", database.ConnectionString));
    }

    public async Task InitializeAsync()
    {
        _connection = new NpgsqlConnection(_database.ConnectionString);
        await _connection.OpenAsync();
        await CleanupAsync();

        var taxonomyPath = Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml");
        _trials = new TrialsRepository(
            new TestConnectionFactory(_database.ConnectionString),
            new TaxonomyStore(File.ReadAllText(taxonomyPath)));
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
        await _connection.DisposeAsync();
    }

    private Task CleanupAsync() => _connection.ExecuteAsync(
        "DELETE FROM trials_cache WHERE nct_id LIKE 'NCT7777%'; " +
        "DELETE FROM aggregated_items WHERE external_id LIKE 'NCT7777%'");

    private sealed class TestConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public async Task<System.Data.Common.DbConnection> OpenConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }

    private Task InsertTrialAsync(
        string nctId,
        string status = "Recruiting",
        string phase = "Phase 2",
        string[]? conditions = null,
        string title = "A study of a pill",
        string locations = "[]") =>
        _connection.ExecuteAsync(
            """
            -- Far-future date: browse sorts by last_update_posted DESC, so a
            -- past-dated seed falls off page 1 of a dirty database — the
            -- dirty-database rule on DatabaseFixture.
            INSERT INTO trials_cache
                (nct_id, title, conditions, phase, overall_status, summary,
                 last_update_posted, locations)
            VALUES (@nctId, @title, @conditions, @phase, @status,
                    'The trial team describes the study here.',
                    DATE '2999-01-01', @locations::jsonb)
            """,
            new
            {
                nctId, title, phase, status, locations,
                conditions = conditions ?? ["Glioblastoma"],
            });

    private Task InsertItemAsync(string nctId, string slug, string status, string summary) =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 plain_title, plain_summary, reviewed_by)
            VALUES ('ctgov', 'trial_update', @nctId, 'A study of a pill',
                    'https://clinicaltrials.gov/study/' || @nctId, @status,
                    'patient_relevant', @slug, 'A plain-language trial title', @summary, 'auto')
            """,
            new { nctId, slug, status, summary });

    // ---------- browse ----------

    [Fact]
    public async Task BrowseShowsOnlyTrialsYouCouldStillJoin()
    {
        await InsertTrialAsync("NCT77770001", status: "Recruiting");
        await InsertTrialAsync("NCT77770002", status: "Completed");

        var page = await _trials.BrowseAsync(new TrialQuery(), CancellationToken.None);
        var mine = page.Items.Where(t => t.NctId.StartsWith("NCT7777", StringComparison.Ordinal)).ToList();

        Assert.Equal("NCT77770001", Assert.Single(mine).NctId);
    }

    [Fact]
    public async Task AskingForClosedTrialsIncludesThem()
    {
        await InsertTrialAsync("NCT77770003", status: "Completed");

        var page = await _trials.BrowseAsync(
            new TrialQuery(IncludeClosed: true), CancellationToken.None);

        Assert.Contains(page.Items, t => t.NctId == "NCT77770003");
    }

    [Fact]
    public async Task FilteringByAParentTumorTypeFindsItsDescendants()
    {
        // Browsing "glioma" must surface a glioblastoma trial — the registry
        // writes its own condition strings, so this matches the taxonomy's
        // labels and aliases against them.
        await InsertTrialAsync("NCT77770004", conditions: ["Recurrent Glioblastoma Multiforme"]);
        await InsertTrialAsync("NCT77770005", conditions: ["Meningioma"]);

        var page = await _trials.BrowseAsync(new TrialQuery("glioma"), CancellationToken.None);
        var mine = page.Items.Where(t => t.NctId.StartsWith("NCT7777", StringComparison.Ordinal)).ToList();

        Assert.Equal("NCT77770004", Assert.Single(mine).NctId);
    }

    [Fact]
    public async Task AConditionFilterCannotSmuggleAWildcard()
    {
        // The patterns are built from the closed taxonomy, but the escaping is
        // what stops a term containing % from matching everything.
        await InsertTrialAsync("NCT77770006", conditions: ["Meningioma"]);

        var patterns = _trials.ConditionPatternsFor("glioblastoma");

        Assert.All(patterns, p => Assert.DoesNotContain("%%", p, StringComparison.Ordinal));
        Assert.NotEmpty(patterns);
    }

    [Fact]
    public async Task AnUnknownTumorTypeOrPhaseInTheUrlIsIgnoredRatherThanEmptyingThePage()
    {
        // A hand-edited or stale querystring must not leave a reader staring at
        // "no trials match what you picked".
        await InsertTrialAsync("NCT77770007");

        var html = await _factory.CreateClient()
            .GetStringAsync("/trials?tumorType=not-a-tumor&phase=Phase%2099");

        Assert.DoesNotContain("No trials match what you picked", html);
    }

    [Fact]
    public async Task PhaseFilterMatchesTheStoredWording()
    {
        await InsertTrialAsync("NCT77770008", phase: "Phase 1");
        await InsertTrialAsync("NCT77770009", phase: "Phase 3");

        var page = await _trials.BrowseAsync(new TrialQuery(Phase: "phase 1"), CancellationToken.None);
        var mine = page.Items.Where(t => t.NctId.StartsWith("NCT7777", StringComparison.Ordinal)).ToList();

        Assert.Equal("NCT77770008", Assert.Single(mine).NctId);
    }

    // ---------- the plain-language gate ----------

    [Fact]
    public async Task OnlyAPublishedItemLendsItsPlainLanguageToATrial()
    {
        // The join must not become a side door around the review gate: a
        // pending summary is exactly the text a safety check or a person held
        // back.
        await InsertTrialAsync("NCT77770010");
        await InsertItemAsync("NCT77770010", "trial-pending", "pending", "Held-back text.");

        var trial = await _trials.FindAsync("NCT77770010", CancellationToken.None);

        Assert.NotNull(trial);
        Assert.Null(trial!.PlainSummary);
        Assert.Equal("A study of a pill", trial.Heading);   // falls back to the registry title
    }

    [Fact]
    public async Task APublishedItemsPlainLanguageIsUsed()
    {
        await InsertTrialAsync("NCT77770011");
        await InsertItemAsync("NCT77770011", "trial-live", "published", "A trial is testing a pill.");

        var trial = await _trials.FindAsync("NCT77770011", CancellationToken.None);

        Assert.Equal("A trial is testing a pill.", trial!.PlainSummary);
        Assert.Equal("A plain-language trial title", trial.Heading);
    }

    // ---------- the pages ----------

    [Fact]
    public async Task TheTrialPageShowsTheStatusTheTrialHasNow()
    {
        await InsertTrialAsync("NCT77770012", status: "Completed");

        var html = await _factory.CreateClient().GetStringAsync("/trials/NCT77770012");

        Assert.Contains("This trial is not taking new patients", html);
        Assert.Contains("It has finished", html);
    }

    [Fact]
    public async Task TheTrialPageLabelsTheRegistrysWordsAsTheRegistrysAndLinksBack()
    {
        // Attribution is a licence requirement (PLAN.md §5) and an honesty one:
        // the registry's text must never read as our plain-language writing.
        await InsertTrialAsync("NCT77770013");

        var html = await _factory.CreateClient().GetStringAsync("/trials/NCT77770013");

        Assert.Contains("What the trial team says", html);
        Assert.Contains("own listing on ClinicalTrials.gov", html);
        Assert.Contains("https://clinicaltrials.gov/study/NCT77770013", html);
        Assert.Contains("U.S. National Library of Medicine", html);
    }

    [Fact]
    public async Task ATrialWeCannotPlaceIsNeverDescribedAsClosed()
    {
        // "We do not know" rendered as "this trial is closed" would be a made-up
        // claim sitting right above a sentence admitting we cannot tell. Same
        // rule the research item page follows.
        await _connection.ExecuteAsync(
            """
            INSERT INTO trials_cache (nct_id, title, conditions, overall_status)
            VALUES ('NCT77770020', 'A trial', ARRAY['Glioblastoma'], NULL),
                   ('NCT77770021', 'A trial', ARRAY['Glioblastoma'], 'Status unknown')
            """);

        var client = _factory.CreateClient();

        foreach (var id in new[] { "NCT77770020", "NCT77770021" })
        {
            var html = await client.GetStringAsync($"/trials/{id}");
            Assert.DoesNotContain("This trial is not taking new patients", html);
        }
    }

    [Fact]
    public async Task ATrialWeCannotPlaceStillAppearsInBrowse()
    {
        // SQL `NULL = ANY(...)` is NULL, so an unknown status was silently
        // being treated as closed and dropping out of the list entirely.
        await _connection.ExecuteAsync(
            """
            INSERT INTO trials_cache
                (nct_id, title, conditions, overall_status, last_update_posted)
            VALUES ('NCT77770022', 'A trial', ARRAY['Glioblastoma'], NULL,
                    DATE '2999-01-01') -- dated to stay on page 1 of a dirty DB
            """);

        var page = await _trials.BrowseAsync(new TrialQuery(), CancellationToken.None);

        Assert.Contains(page.Items, t => t.NctId == "NCT77770022");
    }

    [Fact]
    public async Task ADeepPageNumberIsClampedRatherThanOverflowingIntoA500()
    {
        // page * 20 in unchecked int arithmetic goes negative, and Postgres
        // rejects a negative OFFSET.
        var response = await _factory.CreateClient().GetAsync("/trials?page=200000000");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TheBrowseListSaysItIsNotEveryTrialOnTheRegistry()
    {
        // The cache holds recently-updated brain tumor trials; the search box
        // above it covers the whole registry. A reader would otherwise read
        // both as the same universe.
        await InsertTrialAsync("NCT77770023");

        var html = await _factory.CreateClient().GetStringAsync("/trials");

        Assert.Contains("not every trial on", html);
    }

    [Fact]
    public async Task AClosedTrialInBrowseDoesNotShowItsFrozenHookAsIfItWereLive()
    {
        await InsertTrialAsync("NCT77770024", status: "Completed");
        await InsertItemAsync("NCT77770024", "closed-hook", "published", "Join this trial today.");

        var html = await _factory.CreateClient().GetStringAsync("/trials?includeClosed=true");

        Assert.DoesNotContain("Join this trial today.", html);
        Assert.Contains("not taking new patients", html);
    }

    [Fact]
    public async Task AnUnknownOrMalformedTrialIdIs404()
    {
        var client = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/trials/NCT77779999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/trials/not-an-id")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/trials/NCT1")).StatusCode);
    }

    [Fact]
    public async Task TheBrowsePageWorksWithNoJavaScriptAndTellsYouToAskYourTeam()
    {
        await InsertTrialAsync("NCT77770014");

        var html = await _factory.CreateClient().GetStringAsync("/trials");

        Assert.Contains("Talk to your care team", html);
        Assert.Contains("<form method=\"get\" action=\"/trials\"", html);
        Assert.Contains("name=\"zip\"", html);
        Assert.Contains("ClinicalTrials.gov", html);
    }

    [Fact]
    public async Task AZipWeCannotPlaceSaysSoInsteadOfSearchingNowhere()
    {
        var html = await _factory.CreateClient().GetStringAsync("/trials?zip=00000");

        Assert.Contains("could not find that ZIP code", html);
    }

    [Fact]
    public async Task TheSiteListGroupsPlacesRatherThanDumpingHospitalNames()
    {
        await InsertTrialAsync("NCT77770015", locations: """
            [{"facility":"A Cancer Center","city":"Columbus","state":"Ohio","country":"United States","lat":39.96,"lon":-82.99},
             {"facility":"Another Center","city":"Cleveland","state":"Ohio","country":"United States","lat":41.5,"lon":-81.7}]
            """);

        var html = await _factory.CreateClient().GetStringAsync("/trials/NCT77770015");

        Assert.Contains("Ohio", html);
        Assert.Contains("A Cancer Center", html);
        Assert.Contains("lists 2 sites", html);
    }

    [Fact]
    public void TheTumorMenuOnlyOffersTypesTheRegistryActuallyWrites()
    {
        // "All brain tumors" is a taxonomy entry, not a condition string, so
        // offering it hands the reader "no trials match" for the broadest
        // choice on the page.
        Assert.Contains("all-brain-tumors", TrialsRepository.NonHistologySlugs);
        Assert.NotEmpty(_trials.ConditionTermsFor("glioblastoma"));
    }

    [Fact]
    public void NearMeAndBrowseAgreeOnWhatATumorTypeMeans()
    {
        // Same subtree and aliases, so "Glioma" cannot mean one thing in the
        // list and another in the near-me results.
        var terms = _trials.ConditionTermsFor("glioma");

        Assert.Contains(terms, t => t.Contains("glioblastoma", StringComparison.OrdinalIgnoreCase));

        // ...and a label carrying Essie grouping characters is quoted, not
        // pasted into the outbound query as live syntax.
        var expression = NearbyTrialsClient.ConditionExpression(["DIPG (pontine)"]);
        Assert.Equal("(\"DIPG (pontine)\")", expression);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}

/// <summary>The ZIP lookup that backs "near me" — no database, no network.</summary>
public sealed class ZctaCentroidsTests
{
    private static ZctaCentroids Load() => ZctaCentroids.Load(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "src", "BrainHarbor.Web", "Content"));

    [Fact]
    public void TheWholeCountryIsCovered()
    {
        // ~33k ZCTAs. A truncated or missing file would silently make every
        // ZIP search fail with "we could not find that ZIP".
        Assert.True(Load().Count > 30_000);
    }

    [Fact]
    public void KnownZipsResolveToRoughlyTheRightPlace()
    {
        var centroids = Load();

        var columbus = centroids.Find("43210");
        Assert.NotNull(columbus);
        Assert.InRange(columbus!.Value.Lat, 39.5, 40.5);
        Assert.InRange(columbus.Value.Lon, -83.5, -82.5);
    }

    [Theory]
    [InlineData("43210-1234")]   // ZIP+4, as copied from an address
    [InlineData("  43210 ")]
    public void ReadersDoNotHaveToTypeItPerfectly(string typed)
    {
        Assert.NotNull(Load().Find(typed));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abcde")]
    [InlineData("1234")]
    [InlineData("432100")]
    [InlineData(null)]
    public void NonsenseIsRejectedRatherThanGuessed(string? typed)
    {
        Assert.Null(ZctaCentroids.Normalize(typed));
    }

    [Fact]
    public void DistanceRanksASitesOwnLocationsSensibly()
    {
        // Columbus -> Cleveland is ~125 miles; Columbus -> Los Angeles ~1,990.
        var near = NearbyTrialsClient.DistanceMiles(39.96, -82.99, 41.50, -81.69);
        var far = NearbyTrialsClient.DistanceMiles(39.96, -82.99, 34.05, -118.24);

        Assert.InRange(near, 100, 150);
        Assert.InRange(far, 1_900, 2_100);
    }
}
