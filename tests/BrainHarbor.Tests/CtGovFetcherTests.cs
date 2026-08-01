using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-402: the ClinicalTrials.gov v2 fetcher, driven by responses RECORDED
/// from the live registry (Fixtures/ctgov) rather than hand-written JSON — the
/// WI-205 lesson was that a fetcher can pass invented fixtures and still be
/// wrong about the real API.
///
/// The rules worth protecting here:
///   * every trial carries its facts, so trials_cache stays truthful even for
///     a trial that will never be a feed item;
///   * a trial nobody can join is NOT fed to the reader as news;
///   * a 429 costs a pause, not the day's trials;
///   * a truncated run never hands back a cursor that walks backwards.
/// </summary>
public class CtGovFetcherTests
{
    private static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "ctgov", name));

    private static CtGovResponse Parse(string name) =>
        JsonSerializer.Deserialize<CtGovResponse>(Fixture(name))!;

    private static IReadOnlyList<FetchedItem> Map(string name) =>
        [.. Parse(name).Studies.Select(CtGovFetcher.ToFetchedItem).Where(i => i is not null)!];

    // ---------- mapping a recorded study ----------

    [Fact]
    public void MapsARecordedStudyToAFeedItemAndItsFacts()
    {
        var item = Map("studies-page1.json")[0];

        Assert.Equal("ctgov", item.Source);
        Assert.Equal("trial_update", item.SourceKind);
        Assert.StartsWith("NCT", item.ExternalId, StringComparison.Ordinal);
        Assert.Equal($"https://clinicaltrials.gov/study/{item.ExternalId}", item.Url);
        Assert.False(string.IsNullOrWhiteSpace(item.Title));
        Assert.NotNull(item.Trial);
        Assert.NotEmpty(item.Trial!.Conditions);
    }

    [Fact]
    public void TheFeedDateIsTheUpdateNotTheRegistrationDate()
    {
        // What makes a trial "new" to a reader is the update we are reporting.
        // Using the original registration date would file a 2016 trial at the
        // bottom of the feed forever, where nobody would ever see the update.
        var item = Map("studies-page1.json")[0];

        Assert.Equal(item.Trial!.LastUpdatePosted, item.PublishedAt);
        Assert.NotNull(item.PublishedAt);
    }

    [Fact]
    public void EveryTrialCarriesItsFactsEvenWhenItWillNeverBeAFeedItem()
    {
        // The facts are uploaded separately and unconditionally. That is what
        // lets a closed trial correct trials_cache without also earning a slot
        // in the feed.
        foreach (var fixture in new[] { "studies-page1.json", "studies-closed.json" })
        {
            Assert.All(Map(fixture), item =>
            {
                Assert.NotNull(item.Trial);
                Assert.Equal(item.ExternalId, item.Trial!.NctId);
            });
        }
    }

    [Fact]
    public void AKnownTrialIsNotReUploadedAsAnItem()
    {
        // A changed trial does NOT need a new summary: the summary says who the
        // trial is for, and the live status is read from trials_cache at render
        // time. Re-uploading would re-run the classifier and summarizer on the
        // same trials every single night for nothing.
        Assert.All(Map("studies-page1.json"), item => Assert.False(item.AlwaysUpload));
    }

    [Fact]
    public void SiteCoordinatesSurviveTheMappingIntact()
    {
        // "Near me" is the whole point of the trial finder; a dropped geoPoint
        // turns a nearby trial into an invisible one.
        var withSites = Map("studies-page1.json")
            .First(i => i.Trial!.Locations.Count > 1);

        var located = withSites.Trial!.Locations.Where(l => l.Latitude is not null).ToList();
        Assert.NotEmpty(located);
        Assert.All(located, l =>
        {
            Assert.InRange(l.Latitude!.Value, -90, 90);
            Assert.InRange(l.Longitude!.Value, -180, 180);
            Assert.False(string.IsNullOrWhiteSpace(l.Facility));
        });
    }

    [Fact]
    public void AStudyWithoutAnIdOrTitleIsSkippedRatherThanStoredHalfEmpty()
    {
        Assert.Null(CtGovFetcher.ToFetchedItem(new CtGovStudy()));
        Assert.Null(CtGovFetcher.ToFetchedItem(new CtGovStudy
        {
            ProtocolSection = new CtGovProtocolSection
            {
                IdentificationModule = new CtGovIdentification { NctId = "NCT1", BriefTitle = "  " },
            },
        }));
    }

    // ---------- what reaches a reader ----------

    [Fact]
    public void OnlyTrialsSomeoneCanStillJoinAreTreatedAsNews()
    {
        Assert.All(Map("studies-page1.json"), item => Assert.True(item.FeedWorthy));

        // Closed trials are still fetched — trials_cache must learn that they
        // closed — but they are not worth a summary or a slot in the feed.
        var closed = Map("studies-closed.json");
        Assert.NotEmpty(closed);
        Assert.All(closed, item =>
        {
            Assert.False(item.FeedWorthy);
            Assert.NotNull(item.Trial);
        });
    }

    // ---------- plain words ----------

    [Theory]
    [InlineData("RECRUITING", "Recruiting")]
    [InlineData("ACTIVE_NOT_RECRUITING", "Active, not recruiting")]
    [InlineData("TERMINATED", "Stopped early")]
    [InlineData("ENROLLING_BY_INVITATION", "Enrolling by invitation")]
    public void StatusIsStoredInWordsAPatientReadsWithoutTranslating(string raw, string expected)
    {
        Assert.Equal(expected, CtGovFetcher.PlainStatus(raw));
    }

    [Fact]
    public void AnUnknownStatusIsMadeReadableRatherThanDropped()
    {
        // Showing something true beats showing nothing at all.
        Assert.Equal("Some new state", CtGovFetcher.PlainStatus("SOME_NEW_STATE"));
        Assert.Null(CtGovFetcher.PlainStatus(null));
        Assert.Null(CtGovFetcher.PlainStatus("   "));
    }

    [Fact]
    public void PhasesReadAsPhasesAndNotApplicableIsNotTreatedAsMissing()
    {
        Assert.Equal("Phase 1", CtGovFetcher.PlainPhase(["PHASE1"]));
        Assert.Equal("Phase 2/Phase 3", CtGovFetcher.PlainPhase(["PHASE2", "PHASE3"]));
        Assert.Equal("Early phase 1", CtGovFetcher.PlainPhase(["EARLY_PHASE1"]));

        // A device or behavioural trial legitimately has no phase — that is a
        // fact about the trial, not a gap in the data.
        Assert.Equal("Not applicable", CtGovFetcher.PlainPhase(["NA"]));
        Assert.Null(CtGovFetcher.PlainPhase([]));
        Assert.Null(CtGovFetcher.PlainPhase(null));
    }

    // ---------- the window ----------

    [Fact]
    public void FirstRunLooksBackAFixedWindowRatherThanAtAllOfHistory()
    {
        Assert.Equal(new DateOnly(2026, 7, 1),
            CtGovFetcher.StartDateFor(null, new DateOnly(2026, 7, 31)));
    }

    [Fact]
    public void ACursorGivesAOneDayOverlap()
    {
        Assert.Equal(new DateOnly(2026, 7, 29),
            CtGovFetcher.StartDateFor("2026-07-30", new DateOnly(2026, 7, 31)));
    }

    [Fact]
    public void AVeryOldCursorIsCappedRatherThanRefetchingYears()
    {
        Assert.Equal(new DateOnly(2026, 2, 1),
            CtGovFetcher.StartDateFor("2019-01-01", new DateOnly(2026, 7, 31)));
    }

    [Fact]
    public void AGarbageCursorFallsBackToTheFirstRunWindow()
    {
        Assert.Equal(new DateOnly(2026, 7, 1),
            CtGovFetcher.StartDateFor("nonsense", new DateOnly(2026, 7, 31)));
    }

    [Fact]
    public async Task ATruncatedRunThatMadeNoProgressFailsInsteadOfWalkingTheCursorBackwards()
    {
        // The window starts a day BEHIND the cursor (the overlap). A run that
        // fills the page cap on those old records would otherwise return an
        // EARLIER cursor than it was given — widening the window every night,
        // re-reading the same records, and never reaching the newer ones.
        // Failing is right: it shows up on the admin health page instead of
        // looking like a quiet night.
        var pages = Enumerable.Range(0, CtGovFetcher.MaxPages)
            .Select(_ => Ok(Fixture("studies-page1.json")))
            .ToArray();
        var handler = new StubHandler(pages);

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://clinicaltrials.gov/api/v2/"),
        };
        var fetcher = new CtGovFetcher(client, NullLogger<CtGovFetcher>.Instance);

        // The fixture's newest record is from 2021; the cursor is far later.
        var result = await fetcher.FetchAsync("2026-07-30", CancellationToken.None);

        Assert.Null(result.Cursor);                                   // window held
        Assert.NotNull(result.StalledReason);
        Assert.Contains("without advancing past", result.StalledReason!, StringComparison.Ordinal);

        // The records it DID read still come back, so their facts can be
        // stored. The next run stalls identically, so dropping them here would
        // drop them forever.
        Assert.NotEmpty(result.Items);
    }

    // ---------- paging and rate limiting, against a stub transport ----------

    [Fact]
    public async Task PagesUntilTheRegistryRunsOut()
    {
        var handler = new StubHandler(
            Ok(Fixture("studies-page1.json")),
            Ok(Fixture("studies-page2.json")),
            Ok(Fixture("studies-empty.json")));

        var result = await FetchAsync(handler);

        Assert.Equal(4, result.Items.Count);
        Assert.Equal(3, handler.Requests.Count);

        // Page 1 counts the window; later pages carry the server's own token
        // rather than a computed offset.
        Assert.Contains("countTotal=true", handler.Requests[0]);
        Assert.Contains("pageToken=", handler.Requests[1]);
    }

    [Fact]
    public async Task TheWindowIsAskedForOldestFirstSoATruncatedRunCanResume()
    {
        var handler = new StubHandler(Ok(Fixture("studies-empty.json")));

        await FetchAsync(handler);

        Assert.Contains("LastUpdatePostDate%3Aasc", handler.Requests[0]);
        Assert.Contains("AREA%5BLastUpdatePostDate%5DRANGE", handler.Requests[0]);
    }

    [Fact]
    public async Task ARateLimitCostsAPauseNotTheDaysTrials()
    {
        // The registry's limit is undocumented (PLAN.md §5), so a 429 has to be
        // an ordinary condition rather than a failed source.
        var handler = new StubHandler(
            RateLimited(retryAfterSeconds: 3),
            RateLimited(retryAfterSeconds: null),
            Ok(Fixture("studies-page1.json")),
            Ok(Fixture("studies-empty.json")));

        var time = new FakeTimeProvider(new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero));
        var fetch = FetchAsync(handler, time);

        // The waits are real waits; drive the clock rather than sleeping.
        await AdvanceUntilCompleteAsync(fetch, time);

        var result = await fetch;
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(4, handler.Requests.Count);
    }

    [Fact]
    public async Task AnEndlesslyRateLimitedRunFailsWithoutMovingTheCursor()
    {
        // Failing here (rather than returning a cursor) is what makes the whole
        // window get retried next run instead of silently skipped.
        var handler = new StubHandler([.. Enumerable.Range(0, 10).Select(_ => RateLimited(1))]);

        var time = new FakeTimeProvider(new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero));
        var fetch = FetchAsync(handler, time);
        await AdvanceUntilCompleteAsync(fetch, time);

        await Assert.ThrowsAsync<CtGovRequestException>(() => fetch);
        Assert.Equal(CtGovFetcher.MaxRateLimitRetries + 1, handler.Requests.Count);
    }

    [Fact]
    public async Task AServerErrorOrDroppedConnectionIsRetriedToo()
    {
        // A once-daily unattended job must survive a transient blip rather than
        // lose the day's trials. Only 429 used to be retried, so a single 503
        // on page 17 discarded everything.
        var handler = new StubHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.RequestTimeout),
            Ok(Fixture("studies-page1.json")),
            Ok(Fixture("studies-empty.json")));

        var time = new FakeTimeProvider(new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero));
        var fetch = FetchAsync(handler, time);
        await AdvanceUntilCompleteAsync(fetch, time);

        var result = await fetch;
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(4, handler.Requests.Count);
    }

    [Fact]
    public async Task AnErrorThatWaitingCannotFixFailsImmediately()
    {
        // A 400 from a reshaped API does not get better on retry, and burning
        // four backoffs before reporting it just delays the alarm.
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));

        await Assert.ThrowsAsync<CtGovRequestException>(() => FetchAsync(handler));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public void RetryAfterFromTheServerWinsOverOurOwnBackoff()
    {
        using var response = RateLimited(retryAfterSeconds: 7);
        Assert.Equal(TimeSpan.FromSeconds(7), CtGovFetcher.RetryDelayFor(response, attempt: 0));

        using var bare = RateLimited(retryAfterSeconds: null);
        Assert.Equal(TimeSpan.FromSeconds(2), CtGovFetcher.RetryDelayFor(bare, attempt: 0));
        Assert.Equal(TimeSpan.FromSeconds(8), CtGovFetcher.RetryDelayFor(bare, attempt: 2));

        // A misconfigured header must not park an unattended nightly run.
        using var absurd = RateLimited(retryAfterSeconds: 86_400);
        Assert.Equal(TimeSpan.FromMinutes(2), CtGovFetcher.RetryDelayFor(absurd, attempt: 0));
    }

    // ---------- helpers ----------

    private static Task<FetchResult> FetchAsync(StubHandler handler, TimeProvider? time = null)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://clinicaltrials.gov/api/v2/"),
        };

        var fetcher = new CtGovFetcher(client, NullLogger<CtGovFetcher>.Instance, time);
        return fetcher.FetchAsync(null, CancellationToken.None);
    }

    /// <summary>
    /// Nudges the fake clock forward until the fetch finishes, so a test of
    /// backoff never actually waits out a real one. Gives up loudly rather
    /// than hanging the suite.
    /// </summary>
    private static async Task AdvanceUntilCompleteAsync(Task task, FakeTimeProvider time)
    {
        for (var i = 0; i < 200; i++)
        {
            if (task.IsCompleted)
            {
                return;
            }

            // A real (tiny) delay, so the fetch's continuations actually get a
            // thread before the clock jumps again.
            await Task.Delay(10);
            time.Advance(TimeSpan.FromMinutes(3));
        }

        Assert.Fail("The fetch never completed, even with the clock driven well past every backoff.");
    }

    private static HttpResponseMessage Ok(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
    };

    private static HttpResponseMessage RateLimited(int? retryAfterSeconds)
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (retryAfterSeconds is { } seconds)
        {
            response.Headers.RetryAfter =
                new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds));
        }

        return response;
    }

    private sealed class StubHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private int _next;

        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.ToString());

            if (_next >= responses.Length)
            {
                throw new InvalidOperationException(
                    $"The fetcher made {Requests.Count} requests but only " +
                    $"{responses.Length} were stubbed.");
            }

            return Task.FromResult(responses[_next++]);
        }
    }
}
