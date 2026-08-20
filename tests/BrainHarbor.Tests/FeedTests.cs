using System.Net;
using BrainHarbor.Web.Content;
using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-209: the public feed. Two rules here are safety rules, not features:
/// only human-approved items are ever visible, and early-stage animal/cell
/// work is hidden unless the reader asks for it (a mouse-study headline reads
/// as false hope — PLAN.md §3).
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class FeedTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestSource = "test_sync";

    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _database;
    private NpgsqlConnection _connection = null!;
    private FeedRepository _feed = null!;
    private TaxonomyStore _taxonomy = null!;

    public FeedTests(WebApplicationFactory<Program> factory, DatabaseFixture database)
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

        var taxonomyPath = Path.Combine(
            FindRepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml");
        _taxonomy = new TaxonomyStore(File.ReadAllText(taxonomyPath));
        _feed = new FeedRepository(
            new TestConnectionFactory(_database.ConnectionString), _taxonomy);
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
        await _connection.DisposeAsync();
    }

    private Task CleanupAsync() => _connection.ExecuteAsync(
        "DELETE FROM aggregated_items WHERE source = @TestSource OR external_id LIKE 'NCT8888%'; " +
        "DELETE FROM trials_cache WHERE nct_id LIKE 'NCT8888%'",
        new { TestSource });

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

    private Task InsertAsync(
        string externalId,
        string status = "published",
        string relevance = "patient_relevant",
        string sourceKind = "research",
        string[]? tags = null,
        string? slug = null,
        DateOnly? publishedAt = null,
        int? readiness = null,
        string researchStage = "human_trial") =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance,
                 tumor_tags, slug, published_at, research_stage, plain_title,
                 readiness_score)
            VALUES
                (@TestSource, @sourceKind, @externalId, @title, 'https://example.org',
                 @status, @relevance, @tags, @slug, @publishedAt, @researchStage, @title,
                 @readiness)
            """,
            new
            {
                TestSource,
                sourceKind,
                externalId,
                title = $"Study {externalId}",
                status,
                relevance,
                tags = tags ?? ["glioblastoma"],
                slug = slug ?? $"study-{externalId}",
                publishedAt = publishedAt ?? new DateOnly(2026, 6, 12),
                readiness,
                researchStage,
            });

    /// <summary>
    /// The feed's own rows, in feed order, found by walking the pages.
    ///
    /// Reading page 0 and expecting to see our own rows is really asserting
    /// "nothing else is in the table" — the dirty-database rule on
    /// DatabaseFixture. Paging until the rows turn up tests the ORDER, which
    /// is the part that matters, against any database.
    /// </summary>
    private async Task<IReadOnlyList<FeedRow>> MyRowsInFeedOrderAsync(
        FeedQuery query, params string[] slugs)
    {
        var wanted = slugs.ToHashSet(StringComparer.Ordinal);
        var found = new List<FeedRow>();

        for (var page = 0; page < 200; page++)
        {
            var result = await _feed.GetAsync(query with { Page = page }, CancellationToken.None);
            found.AddRange(result.Items.Where(i => i.Slug is not null && wanted.Contains(i.Slug)));

            if (found.Count == wanted.Count || !result.HasMore)
            {
                break;
            }
        }

        return found;
    }

    // ---------- the human gate ----------

    [Fact]
    public async Task OnlyPublishedItemsAreVisible()
    {
        await InsertAsync("f-published", status: "published");
        await InsertAsync("f-pending", status: "pending", slug: "study-f-pending");
        await InsertAsync("f-rejected", status: "rejected", slug: "study-f-rejected");
        await InsertAsync("f-pulled", status: "pulled", slug: "study-f-pulled");

        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);
        var ids = page.Items.Select(i => i.Slug).ToList();

        Assert.Contains("study-f-published", ids);
        Assert.DoesNotContain("study-f-pending", ids);
        Assert.DoesNotContain("study-f-rejected", ids);
        Assert.DoesNotContain("study-f-pulled", ids);
    }

    [Fact]
    public async Task APulledItemsPermalinkIsGoneNotJustHidden()
    {
        await InsertAsync("f-gone", status: "pulled", slug: "study-f-gone");

        var found = await _feed.GetPublishedBySlugAsync("study-f-gone", CancellationToken.None);

        Assert.Null(found);
    }

    // ---------- early-stage gating ----------

    [Fact]
    public async Task EarlyStageResearchIsHiddenByDefault()
    {
        // The anti-hype rule: animal and lab work must not reach a scared
        // reader unasked.
        await InsertAsync("f-human", relevance: "patient_relevant");
        await InsertAsync("f-mouse", relevance: "early_stage", slug: "study-f-mouse");

        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);

        Assert.DoesNotContain(page.Items, i => i.Slug == "study-f-mouse");
        Assert.Contains(page.Items, i => i.Slug == "study-f-human");
    }

    [Fact]
    public async Task UnclassifiedItemsAreVisibleOnceApproved()
    {
        // Until the M3 classifier lands every item is relevance='pending'.
        // Hiding those would mean a reviewer approves something in M2 and
        // nothing visibly happens — caught in the WI-211 shakedown.
        await InsertAsync("f-unclassified", relevance: "pending", slug: "study-f-unclassified");

        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-unclassified");
    }

    [Fact]
    public async Task EarlyStageAppearsOnlyWhenTheReaderAsksForIt()
    {
        await InsertAsync("f-mouse2", relevance: "early_stage", slug: "study-f-mouse2");

        var mine = await MyRowsInFeedOrderAsync(
            new FeedQuery(IncludeEarlyStage: true), "study-f-mouse2");

        Assert.Contains(mine, i => i.Slug == "study-f-mouse2");
    }

    // ---------- filters ----------

    [Fact]
    public async Task FilteringByAParentTumorTypeIncludesItsDescendants()
    {
        // Browsing "glioma" must surface glioblastoma research.
        await InsertAsync("f-gbm", tags: ["glioblastoma"], slug: "study-f-gbm");
        await InsertAsync("f-mening", tags: ["meningioma"], slug: "study-f-mening");

        var page = await _feed.GetAsync(new FeedQuery(TumorType: "glioma"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-gbm");
        Assert.DoesNotContain(page.Items, i => i.Slug == "study-f-mening");
    }

    [Fact]
    public async Task CatchAllItemsMatchEveryTumorFilter()
    {
        await InsertAsync("f-all", tags: ["all-brain-tumors"], slug: "study-f-all");

        var page = await _feed.GetAsync(new FeedQuery(TumorType: "meningioma"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-all");
    }

    [Fact]
    public async Task AnUnknownTumorFilterIsIgnoredRatherThanReturningNothing()
    {
        await InsertAsync("f-any", slug: "study-f-any");

        var page = await _feed.GetAsync(new FeedQuery(TumorType: "dragonoma"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-any");
    }

    [Fact]
    public async Task FilteringByKindWorks()
    {
        await InsertAsync("f-research", sourceKind: "research", slug: "study-f-research");
        await InsertAsync("f-news", sourceKind: "news", slug: "study-f-news");

        var page = await _feed.GetAsync(new FeedQuery(Kind: "news"), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-news");
        Assert.DoesNotContain(page.Items, i => i.Slug == "study-f-research");
    }

    [Theory]
    [InlineData("research", "research")]
    [InlineData("news", "news")]
    [InlineData("'; DROP TABLE aggregated_items; --", null)]
    [InlineData("nonsense", null)]
    public void OnlyDocumentedKindsSurviveNormalization(string input, string? expected)
    {
        // Querystring values never reach SQL as text; anything unrecognized
        // means "no filter".
        Assert.Equal(expected, FeedRepository.NormalizeKind(input));
    }

    [Fact]
    public async Task UndatedItemsSortLastNotFirst()
    {
        // Postgres DESC defaults to NULLS FIRST, which would float undated
        // items to the top of the feed.
        await InsertAsync("f-dated", publishedAt: new DateOnly(2026, 6, 1), slug: "study-f-dated");
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug, published_at)
            VALUES (@TestSource, 'research', 'f-undated', 'Undated study', 'https://example.org',
                    'published', 'patient_relevant', 'study-f-undated', NULL)
            """,
            new { TestSource });

        // Scoped to this test's own rows: the dev database holds real fetched
        // items, so asserting on global position 0 would make the test depend
        // on whatever else happens to be in the table.
        var mine = await MyRowsInFeedOrderAsync(
            new FeedQuery(), "study-f-dated", "study-f-undated");

        Assert.Equal(2, mine.Count);
        Assert.Equal("study-f-dated", mine[0].Slug);
        Assert.Equal("study-f-undated", mine[1].Slug);
    }

    // ---------- the pages ----------

    [Fact]
    public async Task TheEarlyStageChoicePersistsAcrossVisits()
    {
        // WI-307: a reader who opts into early-stage research shouldn't have to
        // re-tick the box on every visit; a plain visit remembers the choice.
        var client = _factory.CreateClient();

        // Fresh visit: early-stage hidden (patient-first default).
        var fresh = await client.GetStringAsync("/research");
        Assert.Contains("Early-stage animal and lab research is hidden", fresh);

        // Opt in via the filter form (applied=true carries the choice).
        await client.GetAsync("/research?applied=true&early=true");

        // A later plain visit (no query) now remembers it.
        var remembered = await client.GetStringAsync("/research");
        Assert.Contains("including early-stage research", remembered);

        // And opting back out sticks too.
        await client.GetAsync("/research?applied=true");
        var optedOut = await client.GetStringAsync("/research");
        Assert.Contains("Early-stage animal and lab research is hidden", optedOut);
    }

    [Fact]
    public async Task TheFeedPageRendersCardsAndTheEarlyStageToggle()
    {
        await InsertAsync("f-page", slug: "study-f-page");

        var html = await _factory.CreateClient().GetStringAsync("/research");

        Assert.Contains("Research updates", html);
        Assert.Contains("Show early-stage research", html);
        Assert.Contains("class=\"feed-grid\"", html);

        // The path is laid OVER the card photo, in the position the readiness
        // dial used to hold (Dan, 2026-08-19). The indicator plate is what
        // makes it legible on an arbitrary photo, so assert the nesting rather
        // than just the path's presence — a path that rendered below the hero
        // again would still pass a bare "is there a .journey" check.
        Assert.Contains("card-hero__indicator", html);
        Assert.Contains("journey journey--over", html);
        Assert.Matches(
            @"card-hero__indicator""\s*>\s*<ol class=""journey journey--over""",
            html);
    }

    [Fact]
    public async Task AnItemPermalinkRendersWithItsJourneyPathAndProvenance()
    {
        await InsertAsync("f-perma", slug: "study-f-perma");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-perma");

        Assert.Contains("Study f-perma", html);
        Assert.Contains("How far along is this research?", html);
        Assert.Contains("journey--lg", html);
        Assert.Contains("Read the original", html);
        Assert.Contains("class=\"ai-note\"", html);
    }

    [Fact]
    public async Task ItemPagesDoNotInventASummaryWhenThereIsNone()
    {
        await InsertAsync("f-nosum", slug: "study-f-nosum");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-nosum");

        Assert.Contains("have not written a plain-language summary", html);
    }

    [Fact]
    public async Task AnItemPageSaysItWasWrittenByAiAndPublishedAutomatically()
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 plain_title, plain_summary, reviewed_by)
            VALUES (@TestSource, 'research', 'f-auto', 'A study', 'https://example.org',
                    'published', 'patient_relevant', 'study-f-auto',
                    'A pill slowed tumor growth', 'A clear summary.', 'auto')
            """,
            new { TestSource });

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/study-f-auto"));

        Assert.Contains("written by AI and published", html);
        Assert.Contains("automatic safety checks", html);
    }

    // Rendered HTML wraps prose across lines; compare on collapsed whitespace.
    private static string Collapse(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, @"\s+", " ");

    [Fact]
    public async Task TheItemPageNeverClaimsAPersonReviewedIt()
    {
        // The site publishes automatically and must not claim human review —
        // not even for a row whose reviewed_by happens to be a person's email.
        await InsertAsync("f-no-human", slug: "study-f-no-human");
        await _connection.ExecuteAsync(
            "UPDATE aggregated_items SET reviewed_by = 'dan@example.org' WHERE source = @TestSource AND external_id = 'f-no-human'",
            new { TestSource });

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/study-f-no-human"));

        Assert.DoesNotContain("reviewed by a person", html);
        Assert.DoesNotContain("review it before publishing", html);
    }

    [Fact]
    public async Task AnUnknownSlugIsA404()
    {
        var response = await _factory.CreateClient().GetAsync("/research/no-such-item");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RawSourceTextIsNeverRenderedPublicly()
    {
        // Licensing: raw_summary is pipeline input only (data-model.md).
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, raw_summary, status, relevance, slug)
            VALUES (@TestSource, 'research', 'f-raw', 'A study', 'https://example.org',
                    'SECRET-ABSTRACT-TEXT', 'published', 'patient_relevant', 'study-f-raw')
            """,
            new { TestSource });

        var client = _factory.CreateClient();

        Assert.DoesNotContain("SECRET-ABSTRACT-TEXT", await client.GetStringAsync("/research"));
        Assert.DoesNotContain("SECRET-ABSTRACT-TEXT",
            await client.GetStringAsync("/research/study-f-raw"));
    }

    // ---------- WI-306: the full six-block item page ----------

    private Task InsertFullSummaryAsync(
        string externalId, string slug, int? readiness = 7, string sourceKind = "research") =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 research_stage, plain_title, plain_summary, plain_what_studied,
                 plain_what_found, plain_means, plain_doesnt_mean,
                 readiness_score, readiness_reason, reviewed_by)
            VALUES
                (@TestSource, @sourceKind, @externalId, 'A jargon-heavy title', 'https://example.org',
                 'published', 'patient_relevant', @slug, 'human_trial',
                 'A pill slowed a glioma', 'A daily pill helped people.',
                 'Researchers studied people with a glioma.',
                 'The pill slowed the tumor.', 'It may add time before stronger care.',
                 'This is not a cure and does not fit every tumor.',
                 @readiness, 'Being tested in people in trials, but not yet approved.',
                 'dan@example.org')
            """,
            new { TestSource, externalId, slug, readiness, sourceKind });

    /// <summary>
    /// A published trial page plus its row in the trial cache. Source is the
    /// real 'ctgov' because the join keys on it.
    /// </summary>
    private async Task InsertPublishedTrialAsync(string nctId, string slug, string status)
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 research_stage, plain_title, plain_summary, plain_what_studied,
                 plain_what_found, plain_means, plain_doesnt_mean, reviewed_by)
            VALUES
                ('ctgov', 'trial_update', @nctId, 'A trial of a pill', 'https://example.org',
                 'published', 'patient_relevant', @slug, 'human_trial',
                 'A trial is testing a pill for glioma', 'A trial is testing a new pill.',
                 'It is for adults with a glioma that has come back.',
                 'The trial is still going. It has not reported results.',
                 'If you fit, you could ask your care team about it.',
                 'This is a test, not a proven treatment, and it is not a cure.',
                 'auto')
            """,
            new { nctId, slug });

        await _connection.ExecuteAsync(
            """
            INSERT INTO trials_cache
                (nct_id, title, conditions, phase, overall_status, last_update_posted)
            VALUES (@nctId, 'A trial of a pill', ARRAY['Glioma'], 'Phase 2', @status, DATE '2026-07-20')
            """,
            new { nctId, status });
    }

    [Fact]
    public async Task APublishedTrialPageShowsTheStatusTheTrialHasNowNotTheOneItHadWhenWritten()
    {
        // The page is written once and then frozen; trials close. Reading the
        // status live from the trial cache is what stops a live, indexed page
        // sending someone to a door that no longer opens.
        await InsertPublishedTrialAsync("NCT88880001", "trial-closed", "Completed");

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/trial-closed"));

        Assert.Contains("This trial is not taking new patients", html);
        Assert.Contains("It has finished", html);
        Assert.DoesNotContain("Status: Recruiting", html);

        // The badge explanation is separate hand-written copy further down the
        // same page. It said "This is a trial that is enrolling" for every
        // trial regardless of status, so the page contradicted itself.
        Assert.DoesNotContain("is enrolling", html);
    }

    [Fact]
    public async Task AClosedTrialDropsOutOfTheFeedAndTheRssButKeepsItsPage()
    {
        // The card carries the hook written while the trial was open, and that
        // text is never rewritten. Left in the feed it is a standing invitation
        // to something that no longer exists. The permalink stays live and says
        // so plainly — someone looking that trial up still deserves an answer.
        await InsertPublishedTrialAsync("NCT88880004", "trial-gone", "Completed");
        await InsertPublishedTrialAsync("NCT88880005", "trial-here", "Recruiting");

        var feed = await MyRowsInFeedOrderAsync(new FeedQuery(), "trial-gone", "trial-here");
        Assert.Equal("trial-here", Assert.Single(feed).Slug);

        var syndicated = await _feed.GetAllPublishedAsync(500, CancellationToken.None);
        Assert.DoesNotContain(syndicated, i => i.Slug == "trial-gone");
        Assert.Contains(syndicated, i => i.Slug == "trial-here");

        // ...but the page itself is still there.
        var response = await _factory.CreateClient().GetAsync("/research/trial-gone");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchStillFindsAClosedTrialButDoesNotRepeatItsStaleHook()
    {
        await InsertPublishedTrialAsync("NCT88880006", "trial-searchable", "Completed");

        var hits = await _feed.SearchAsync("glioma pill", 20, CancellationToken.None);
        var hit = hits.FirstOrDefault(h => h.Slug == "trial-searchable");

        Assert.NotNull(hit);
        Assert.True(hit!.TrialHasClosed);

        // The card the search page renders must not repeat the frozen
        // "a trial is testing" hook as though it were live.
        Assert.Equal("This trial is not taking new patients.", _feed.ToCard(hit).Hook);
    }

    [Fact]
    public async Task ATrialWithNoCachedStatusIsNotTreatedAsClosed()
    {
        // Unknown is not closed. Hiding a trial we simply have no facts for
        // would silently shrink the feed on a bad sync.
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 plain_summary, reviewed_by)
            VALUES ('ctgov', 'trial_update', 'NCT88880007', 'A trial', 'https://example.org',
                    'published', 'patient_relevant', 'trial-no-facts', 'A trial hook.', 'auto')
            """);

        var feed = await MyRowsInFeedOrderAsync(new FeedQuery(), "trial-no-facts");

        Assert.Single(feed);
    }

    [Fact]
    public async Task AnOpenTrialPageSaysSoWithoutTheClosedWarning()
    {
        await InsertPublishedTrialAsync("NCT88880002", "trial-open", "Recruiting");

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/trial-open"));

        Assert.Contains("Status: Recruiting", html);
        Assert.Contains("looking for patients now", html);
        Assert.DoesNotContain("not taking new patients", html);
    }

    [Fact]
    public async Task ATrialWeHaveNoFactsForRendersWithoutInventingAStatus()
    {
        // An unknown status is not the same as "closed" — claiming either way
        // would be making something up.
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug, reviewed_by)
            VALUES ('ctgov', 'trial_update', 'NCT88880003', 'A trial', 'https://example.org',
                    'published', 'patient_relevant', 'trial-unknown', 'auto')
            """);

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/trial-unknown"));

        Assert.DoesNotContain("not taking new patients", html);
        Assert.DoesNotContain("Status:", html);
    }

    [Fact]
    public async Task ATrialPageNeverClaimsItFoundSomething()
    {
        // WI-402: the blocks hold who a trial is for and where it stands. An
        // open trial has no results, so labelling them "what they found" would
        // present a recruiting trial as if it had reported an outcome.
        await InsertFullSummaryAsync("f-trial", "study-f-trial", sourceKind: "trial_update");

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/study-f-trial"));

        Assert.Contains("Who this trial is for", html);
        Assert.Contains("Where it stands", html);
        Assert.DoesNotContain("What they found", html);
    }

    [Fact]
    public async Task TheItemPageRendersAllSixBlocksAndTheJourneyPath()
    {
        await InsertFullSummaryAsync("f-full", "study-f-full");

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/study-f-full"));

        Assert.Contains("The short version", html);
        Assert.Contains("What was studied", html);
        Assert.Contains("What they found", html);
        Assert.Contains("What this means", html);          // the means-block heading
        Assert.Contains("does not fit every tumor", html); // the doesn't-mean block
        Assert.Contains("How far along is this research?", html);
        Assert.Contains("Tested in people. Stage 4 of 4", html);  // the server-built label
    }

    /// <summary>
    /// The 1-to-10 readiness score is gone from every reader-facing surface
    /// (journey handoff, 2026-08-19) while staying in the database, the sync
    /// contract and the admin queue. This is the guard: the number is the exact
    /// thing the handoff argued against, because no single value on a 10-point
    /// scale has a meaning a patient can state, and "7 of 10 ready" reads as a
    /// promise about a schedule that nobody made.
    /// </summary>
    [Fact]
    public async Task NoReaderFacingPageShowsTheOneToTenReadinessScore()
    {
        await InsertFullSummaryAsync("f-noscore", "study-f-noscore");
        var client = _factory.CreateClient();

        foreach (var url in new[] { "/", "/research", "/research/study-f-noscore" })
        {
            var html = Collapse(await client.GetStringAsync(url));

            Assert.DoesNotContain("Readiness 7/10", html);
            Assert.DoesNotContain("of 10 ready", html);
            Assert.DoesNotContain("readiness__bar", html);
            Assert.DoesNotContain("How close is this to helping patients?", html);
        }
    }

    [Fact]
    public async Task GlossaryTermsInSummaryBlocksGetTooltips()
    {
        // "glioma" is a glossary term — the item page marks it in the summary so
        // a reader can tap for a plain definition (content-pipeline.md §6).
        await InsertFullSummaryAsync("f-tip", "study-f-tip");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-tip");

        Assert.Contains("class=\"term\"", html);
        Assert.Contains("popover", html);
    }

    [Fact]
    public async Task TheItemPageOffersAOneTapReportAProblem()
    {
        await InsertFullSummaryAsync("f-report-ui", "study-f-report-ui");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-report-ui");

        Assert.Contains("Report a problem", html);
    }

    [Fact]
    public async Task ReportingAProblemFlagsThePublishedItemAndRecordsItWithoutUnpublishing()
    {
        await InsertFullSummaryAsync("f-reported", "study-f-reported");

        var ok = await _feed.ReportProblemAsync(
            "study-f-reported", "The number looks wrong.", CancellationToken.None);
        Assert.True(ok);

        var row = await _connection.QuerySingleAsync<(bool Flagged, string Status)>(
            "SELECT summary_flagged, status FROM aggregated_items WHERE slug = 'study-f-reported'");
        Assert.True(row.Flagged);
        Assert.Equal("published", row.Status); // a reader can't take a page down

        var (action, actor, note) = await _connection.QuerySingleAsync<(string, string, string?)>(
            """
            SELECT re.action, re.actor, re.note FROM review_events re
            JOIN aggregated_items a ON a.id = re.item_id
            WHERE a.slug = 'study-f-reported'
            """);
        Assert.Equal("reported", action);
        Assert.Equal("reader", actor);
        Assert.Equal("The number looks wrong.", note);
    }

    [Fact]
    public async Task MarkdownLinksAndImagesInSummaryBlocksAreNeutralized()
    {
        // Summaries come from untrusted abstracts via an LLM. A prompt-injected
        // markdown link/image must not become a live <a href="javascript:"> or
        // an <img> (stored click-XSS / tracking pixel).
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 research_stage, plain_title, plain_summary, plain_what_studied,
                 plain_what_found, plain_means, plain_doesnt_mean, reviewed_by)
            VALUES
                (@TestSource, 'research', 'f-xss', 'A study', 'https://example.org',
                 'published', 'patient_relevant', 'study-f-xss', 'human_trial',
                 'A title', 'A hook.',
                 '[click](javascript:alert(1)) and ![x](http://evil/pixel.png)',
                 'Found.', 'Means.', 'Does not mean.', 'dan@example.org')
            """,
            new { TestSource });

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-xss");

        Assert.DoesNotContain("href=\"javascript:", html);

        // Scoped to <main>, not the whole document: the shell legitimately
        // carries the brand lockup in its header. What must never happen is an
        // <img> or a live link appearing in the summary CONTENT, which is where
        // the injected markdown would land.
        var main = html[html.IndexOf("<main", StringComparison.Ordinal)..
                        html.IndexOf("</main>", StringComparison.Ordinal)];
        Assert.DoesNotContain("<img", main);

        // The markdown must still be VISIBLE as inert text, not silently
        // dropped — a reviewer needs to see what the model actually produced.
        // So the payload's URL does appear on the page; what matters is that it
        // is text between the tags, never the src of an element.
        Assert.Contains("evil/pixel.png", main, StringComparison.Ordinal);
        Assert.DoesNotContain("src=\"http://evil", main, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ReportingTwiceDoesNotFloodTheAuditTrail()
    {
        await InsertFullSummaryAsync("f-dedup", "study-f-dedup");

        await _feed.ReportProblemAsync("study-f-dedup", "first", CancellationToken.None);
        await _feed.ReportProblemAsync("study-f-dedup", "second", CancellationToken.None);

        var events = await _connection.ExecuteScalarAsync<int>(
            """
            SELECT count(*) FROM review_events re
            JOIN aggregated_items a ON a.id = re.item_id
            WHERE a.slug = 'study-f-dedup' AND re.action = 'reported'
            """);
        Assert.Equal(1, events); // the second report is a no-op while unresolved
    }

    [Fact]
    public async Task ReportingOnAnUnknownOrUnpublishedSlugDoesNothing()
    {
        await InsertAsync("f-pending-report", status: "pending", slug: "study-f-pending-report");

        Assert.False(await _feed.ReportProblemAsync("no-such-slug", null, CancellationToken.None));
        Assert.False(await _feed.ReportProblemAsync(
            "study-f-pending-report", null, CancellationToken.None));
    }

    // ---------- WI-410: sorting ----------

    [Fact]
    public async Task FurthestAlongSortRanksByTheStageLadderTheCardsShow()
    {
        // "What is furthest along?" — ranked by the SAME four stages the
        // journey path draws, not by the 1-to-10 readiness score it replaced
        // (journey handoff, 2026-08-19). Sorting by a number the reader cannot
        // see on the card is the thing WI-429 said not to leave standing.
        await InsertAsync("s-cells", researchStage: "preclinical_cell", slug: "study-s-cells");
        await InsertAsync("s-people", researchStage: "human_trial", slug: "study-s-people");
        await InsertAsync("s-animals", researchStage: "preclinical_animal", slug: "study-s-animals");
        await InsertAsync("s-review", researchStage: "review_guideline", slug: "study-s-review");

        var rows = await MyRowsInFeedOrderAsync(
            new FeedQuery(Sort: "readiness"),
            "study-s-cells", "study-s-people", "study-s-animals", "study-s-review");

        Assert.Equal(
            ["study-s-people", "study-s-review", "study-s-animals", "study-s-cells"],
            rows.Select(r => r.Slug).ToArray());
    }

    /// <summary>
    /// Trials, news and preprints are not findings, so they must not out-rank a
    /// real one in a sort that means "furthest along" — the same reason they get
    /// a .stage-note strip instead of a journey path.
    ///
    /// The preprint here carries research_stage 'human_trial' deliberately: the
    /// mapper decides preprint from source_kind BEFORE it looks at the stage, so
    /// a SQL CASE that tested research_stage first would rank this preprint as
    /// "tested in people" and float it to the top of the feed.
    /// </summary>
    [Fact]
    public async Task ItemsThatAreNotFindingsSortBelowEveryRealFinding()
    {
        // relevance 'early_stage', not the default: a DB check constraint
        // forbids a patient_relevant preprint outright (content-pipeline §9),
        // which is a rule worth tripping over rather than working around.
        await InsertAsync("s-preprint", sourceKind: "preprint", relevance: "early_stage",
            researchStage: "human_trial", slug: "study-s-preprint");
        await InsertAsync("s-trial", sourceKind: "trial_update",
            researchStage: "human_trial", slug: "study-s-trial");
        await InsertAsync("s-labcells", researchStage: "preclinical_cell", slug: "study-s-labcells");

        var rows = await MyRowsInFeedOrderAsync(
            new FeedQuery(Sort: "readiness", IncludeEarlyStage: true),
            "study-s-preprint", "study-s-trial", "study-s-labcells");

        // The weakest real finding still beats both non-findings.
        Assert.Equal("study-s-labcells", rows[0].Slug);
    }

    [Fact]
    public async Task TypeSortGroupsKindsAndStaysNewestFirstWithinAGroup()
    {
        // Type is a grouping, not a ranking: research → news → preprint in the
        // menu's order, and inside a group the order is still newest first —
        // decided explicitly, not left to whatever the index returns.
        await InsertAsync("s-res-old", publishedAt: new DateOnly(2026, 6, 1), slug: "study-s-res-old");
        await InsertAsync("s-res-new", publishedAt: new DateOnly(2026, 6, 12), slug: "study-s-res-new");
        await InsertAsync("s-news", sourceKind: "news", slug: "study-s-news");
        // A preprint can never be patient_relevant (DB rule); pending is shown.
        await InsertAsync("s-pre", sourceKind: "preprint", relevance: "pending", slug: "study-s-pre");

        var rows = await MyRowsInFeedOrderAsync(
            new FeedQuery(Sort: "type"),
            "study-s-res-old", "study-s-res-new", "study-s-news", "study-s-pre");

        Assert.Equal(
            ["study-s-res-new", "study-s-res-old", "study-s-news", "study-s-pre"],
            rows.Select(r => r.Slug).ToArray());
    }

    [Theory]
    [InlineData("readiness", "readiness")]
    [InlineData("type", "type")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("date", null)]        // date is the default, kept canonical as null
    [InlineData("DROP TABLE", null)]  // garbage never reaches the ORDER BY switch
    public void OnlyDocumentedSortsAreAccepted(string? input, string? expected) =>
        Assert.Equal(expected, FeedRepository.NormalizeSort(input));

    [Fact]
    public async Task ShowMoreKeepsTheChosenSortAndFilters()
    {
        // A shared or bookmarked sorted view must survive paging — the sort
        // and every filter ride the Show more URL together.
        var model = new BrainHarbor.Web.Pages.Research.IndexModel(_feed, _taxonomy)
        {
            PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            },
        };

        await model.OnGetAsync(
            tumor: "glioma", early: true, page: 1, applied: true, sort: "readiness");

        Assert.Contains("page=2", model.NextPageUrl);
        Assert.Contains("tumor=glioma", model.NextPageUrl);
        Assert.Contains("early=true", model.NextPageUrl);
        Assert.Contains("sort=readiness", model.NextPageUrl);
    }

    [Fact]
    public async Task TheSortControlRendersAndTheChosenSortSticks()
    {
        var html = await _factory.CreateClient()
            .GetStringAsync("/research?sort=readiness");

        Assert.Contains("for=\"sort\"", html);
        Assert.Contains("id=\"sort\"", html);
        Assert.Contains("<option value=\"readiness\" selected", html);
    }

    // ---------- WI-309: search ----------

    [Fact]
    public async Task SearchFindsPublishedItemsByTheirPlainLanguageText()
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 research_stage, plain_title, plain_summary, plain_what_found)
            VALUES
                (@TestSource, 'research', 'srch-1', 'Original', 'https://example.org',
                 'published', 'patient_relevant', 'study-srch-1', 'human_trial',
                 'A pill for glioblastoma', 'A daily pill helped.',
                 'The vorasidenib pill slowed the tumor.')
            """,
            new { TestSource });

        var hits = await _feed.SearchAsync("vorasidenib", 10, CancellationToken.None);

        Assert.Contains(hits, h => h.Slug == "study-srch-1");
    }

    [Fact]
    public async Task SearchOnlyReturnsPublishedItems()
    {
        await InsertAsync("srch-pending", status: "pending", slug: "study-srch-pending");
        // The plain title carries a distinctive word to match on.
        await _connection.ExecuteAsync(
            "UPDATE aggregated_items SET plain_title = 'zzquux therapy' WHERE source = @TestSource AND external_id = 'srch-pending'",
            new { TestSource });

        var hits = await _feed.SearchAsync("zzquux", 10, CancellationToken.None);

        Assert.DoesNotContain(hits, h => h.Slug == "study-srch-pending");
    }

    [Fact]
    public async Task SearchIsForgivingOfMessyInputAndNeverThrows()
    {
        // websearch_to_tsquery must swallow stray quotes/operators rather than
        // 500 on a scared reader's messy query.
        var hits = await _feed.SearchAsync("\"glioma symptoms -- OR)(", 10, CancellationToken.None);

        Assert.NotNull(hits); // no exception is the assertion
    }

    [Fact]
    public async Task TheSearchPageRunsAQueryOverItemsAndPages()
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 research_stage, plain_title, plain_summary)
            VALUES
                (@TestSource, 'research', 'srch-page', 'Original', 'https://example.org',
                 'published', 'patient_relevant', 'study-srch-page', 'human_trial',
                 'A glioma study', 'A plain hook about glioma.')
            """,
            new { TestSource });

        var html = await _factory.CreateClient().GetStringAsync("/search?q=glioma");

        Assert.Contains("result(s) for", html);
        Assert.Contains("study-srch-page", html);          // an item hit
        Assert.Contains("<h2>Research</h2>", html);
    }

    [Fact]
    public async Task SearchAlsoCoversStaticPages()
    {
        // The "Pages" section only renders on a real static-page hit, so its
        // presence proves the curated pages are searched too.
        var html = await _factory.CreateClient().GetStringAsync("/search?q=privacy");

        Assert.Contains("<h2>Pages</h2>", html);
        Assert.Contains("href=\"/privacy\"", html);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
