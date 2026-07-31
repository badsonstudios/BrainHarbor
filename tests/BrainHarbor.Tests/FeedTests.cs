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
        _feed = new FeedRepository(
            new TestConnectionFactory(_database.ConnectionString),
            new TaxonomyStore(File.ReadAllText(taxonomyPath)));
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
        await _connection.DisposeAsync();
    }

    private Task CleanupAsync() => _connection.ExecuteAsync(
        "DELETE FROM aggregated_items WHERE source = @TestSource", new { TestSource });

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
        DateOnly? publishedAt = null) =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance,
                 tumor_tags, slug, published_at, research_stage, plain_title)
            VALUES
                (@TestSource, @sourceKind, @externalId, @title, 'https://example.org',
                 @status, @relevance, @tags, @slug, @publishedAt, 'human_trial', @title)
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
            });

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

        var page = await _feed.GetAsync(
            new FeedQuery(IncludeEarlyStage: true), CancellationToken.None);

        Assert.Contains(page.Items, i => i.Slug == "study-f-mouse2");
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

        // Scoped to this test's own rows: the dev database may hold real
        // fetched items, and asserting on global position 0 would make the
        // test depend on whatever else is in the table.
        var page = await _feed.GetAsync(new FeedQuery(), CancellationToken.None);
        var mine = page.Items
            .Select((item, index) => (item, index))
            .Where(x => x.item.Slug is "study-f-dated" or "study-f-undated")
            .ToList();

        Assert.Equal(2, mine.Count);
        Assert.Equal("study-f-dated", mine[0].item.Slug);
        Assert.Equal("study-f-undated", mine[1].item.Slug);
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
        Assert.Contains("badge badge--", html);
    }

    [Fact]
    public async Task AnItemPermalinkRendersWithItsBadgeAndProvenance()
    {
        await InsertAsync("f-perma", slug: "study-f-perma");

        var html = await _factory.CreateClient().GetStringAsync("/research/study-f-perma");

        Assert.Contains("Study f-perma", html);
        Assert.Contains("How early is this?", html);
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

    private Task InsertFullSummaryAsync(string externalId, string slug, int? readiness = 7) =>
        _connection.ExecuteAsync(
            """
            INSERT INTO aggregated_items
                (source, source_kind, external_id, title, url, status, relevance, slug,
                 research_stage, plain_title, plain_summary, plain_what_studied,
                 plain_what_found, plain_means, plain_doesnt_mean,
                 readiness_score, readiness_reason, reviewed_by)
            VALUES
                (@TestSource, 'research', @externalId, 'A jargon-heavy title', 'https://example.org',
                 'published', 'patient_relevant', @slug, 'human_trial',
                 'A pill slowed a glioma', 'A daily pill helped people.',
                 'Researchers studied people with a glioma.',
                 'The pill slowed the tumor.', 'It may add time before stronger care.',
                 'This is not a cure and does not fit every tumor.',
                 @readiness, 'Being tested in people in trials, but not yet approved.',
                 'dan@example.org')
            """,
            new { TestSource, externalId, slug, readiness });

    [Fact]
    public async Task TheItemPageRendersAllSixBlocksAndTheReadinessScore()
    {
        await InsertFullSummaryAsync("f-full", "study-f-full");

        var html = Collapse(await _factory.CreateClient().GetStringAsync("/research/study-f-full"));

        Assert.Contains("The short version", html);
        Assert.Contains("What was studied", html);
        Assert.Contains("What they found", html);
        Assert.Contains("What this means", html);          // the means-block heading
        Assert.Contains("does not fit every tumor", html); // the doesn't-mean block
        Assert.Contains("Readiness 7/10", html);
        Assert.Contains("In late human trials", html);     // the readiness band label
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
        Assert.DoesNotContain("<img", html);
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
