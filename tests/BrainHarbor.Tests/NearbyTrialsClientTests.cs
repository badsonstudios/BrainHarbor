using System.Net;
using BrainHarbor.Web.Trials;
using Microsoft.Extensions.Logging.Abstractions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-403: the LIVE ClinicalTrials.gov call that runs inside a page request.
///
/// The whole contract is that it FAILS SOFT. A reader looking for a trial while
/// frightened must never meet an error page because a registry was slow. These
/// tests exist because the first cut got that wrong in the commonest case:
/// HttpClient's own timeout throws TaskCanceledException, which IS an
/// OperationCanceledException, so an exception filter written to let real
/// cancellation through was letting the timeout through too.
/// </summary>
public class NearbyTrialsClientTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
        : HttpMessageHandler
    {
        public string? LastUrl { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUrl = request.RequestUri!.AbsoluteUri;
            return respond(request);
        }
    }

    private static NearbyTrialsClient Build(StubHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://clinicaltrials.gov/api/v2/") },
            NullLogger<NearbyTrialsClient>.Instance);

    private static StubHandler Responding(HttpStatusCode status, string body = "{}") =>
        new(_ => Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        }));

    [Fact]
    public async Task ATimeoutDegradesToUnavailableInsteadOfAnErrorPage()
    {
        // The blocker this file exists for.
        var handler = new StubHandler(_ =>
            Task.FromException<HttpResponseMessage>(new TaskCanceledException("timed out")));

        var result = await Build(handler).FindAsync(39.9, -82.9, null, CancellationToken.None);

        Assert.True(result.RegistryUnavailable);
        Assert.Empty(result.Trials);
    }

    [Fact]
    public async Task ARealCancellationStillPropagates()
    {
        // A reader navigating away is not a registry failure, and swallowing it
        // would keep work running for a request nobody is waiting on.
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var handler = new StubHandler(_ =>
            Task.FromException<HttpResponseMessage>(new TaskCanceledException("cancelled")));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Build(handler).FindAsync(39.9, -82.9, null, cancellation.Token));
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task AnyBadResponseDegradesRatherThanThrowing(HttpStatusCode status)
    {
        var result = await Build(Responding(status)).FindAsync(39.9, -82.9, null, CancellationToken.None);

        Assert.True(result.RegistryUnavailable);
    }

    [Fact]
    public async Task GarbageOrEmptyJsonIsSurvivable()
    {
        foreach (var body in new[] { "not json at all", "{}", "{\"studies\":null}" })
        {
            var result = await Build(Responding(HttpStatusCode.OK, body))
                .FindAsync(39.9, -82.9, null, CancellationToken.None);

            Assert.Empty(result.Trials);
        }
    }

    [Fact]
    public async Task ADroppedConnectionDegradesToo()
    {
        var handler = new StubHandler(_ =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("connection reset")));

        var result = await Build(handler).FindAsync(39.9, -82.9, null, CancellationToken.None);

        Assert.True(result.RegistryUnavailable);
    }

    [Fact]
    public async Task TheNearestSiteIsTheOneNearestTheReaderNotTheFirstListed()
    {
        // The registry returns EVERY site of a matching trial. Naming one on the
        // other side of the country would be worse than naming none.
        const string body = """
            {"totalCount":1,"studies":[{"protocolSection":{
              "identificationModule":{"nctId":"NCT00000001","briefTitle":"A trial"},
              "statusModule":{"overallStatus":"RECRUITING"},
              "designModule":{"phases":["PHASE2"]},
              "contactsLocationsModule":{"locations":[
                {"facility":"Far","city":"Los Angeles","state":"California","geoPoint":{"lat":34.05,"lon":-118.24}},
                {"facility":"Near","city":"Columbus","state":"Ohio","geoPoint":{"lat":39.96,"lon":-82.99}}
              ]}}}]}
            """;

        var result = await Build(Responding(HttpStatusCode.OK, body))
            .FindAsync(39.9612, -82.9988, null, CancellationToken.None);

        var trial = Assert.Single(result.Trials);
        Assert.Equal("Columbus, Ohio", trial.NearestSite);
        Assert.Equal("Recruiting", trial.Status);   // plain words, not the enum
        Assert.Equal("Phase 2", trial.Phase);
    }

    [Fact]
    public async Task ATrialWithNoUsableSiteStillRendersRatherThanBeingDropped()
    {
        const string body = """
            {"totalCount":1,"studies":[{"protocolSection":{
              "identificationModule":{"nctId":"NCT00000002","briefTitle":"A trial"},
              "contactsLocationsModule":{"locations":[]}}}]}
            """;

        var result = await Build(Responding(HttpStatusCode.OK, body))
            .FindAsync(39.9, -82.9, null, CancellationToken.None);

        var trial = Assert.Single(result.Trials);
        Assert.Null(trial.NearestSite);
    }

    [Fact]
    public async Task TheHeadlineCountComesFromTheRegistryNotOurPageSize()
    {
        // Printing the page cap as "N trials near you" would be a false number
        // on a medical page.
        const string body = """
            {"totalCount":137,"studies":[{"protocolSection":{
              "identificationModule":{"nctId":"NCT00000003","briefTitle":"A trial"}}}]}
            """;

        var result = await Build(Responding(HttpStatusCode.OK, body))
            .FindAsync(39.9, -82.9, null, CancellationToken.None);

        Assert.Equal(137, result.TotalCount);
        Assert.True(result.Truncated);
    }

    [Fact]
    public async Task TheOutgoingQueryCarriesTheDistanceFilterAndOnlyOpenTrials()
    {
        var handler = Responding(HttpStatusCode.OK, "{\"studies\":[]}");

        await Build(handler).FindAsync(39.9612, -82.9988, ["Glioblastoma"], CancellationToken.None);

        Assert.Contains("distance(39.9612,-82.9988,50mi)", Uri.UnescapeDataString(handler.LastUrl!));
        Assert.Contains("RECRUITING", Uri.UnescapeDataString(handler.LastUrl!));
        Assert.Contains("\"Glioblastoma\"", Uri.UnescapeDataString(handler.LastUrl!));
    }

    [Fact]
    public void ATumorLabelCannotReshapeTheRegistryQuery()
    {
        // Essie treats parentheses and quotes as syntax. A taxonomy label like
        // "DIPG (pontine)" must go over as a term, not as grouping operators.
        Assert.Equal("(\"DIPG (pontine)\")",
            NearbyTrialsClient.ConditionExpression(["DIPG (pontine)"]));

        // The quote is neutralised; the rest travels as ordinary text.
        var neutralised = NearbyTrialsClient.ConditionExpression(["glioma\" OR anything"]);
        Assert.StartsWith("(\"", neutralised, StringComparison.Ordinal);
        Assert.EndsWith("\")", neutralised, StringComparison.Ordinal);
        Assert.Equal(2, neutralised.Count(c => c == '"'));

        // No terms at all falls back to the full brain-tumor query rather than
        // asking the registry for everything on earth.
        Assert.Equal(Registry.BrainTumorConditionQuery,
            NearbyTrialsClient.ConditionExpression([]));
    }

    [Fact]
    public void TheWebSideRegistryWordingMatchesThePipelinesExactly()
    {
        // These plain words are STORED by the pipeline and COMPARED by the site
        // (TrialsRepository.OpenStatuses). A change on one side that is not
        // mirrored would quietly stop matching.
        foreach (var raw in new[]
                 {
                     "RECRUITING", "NOT_YET_RECRUITING", "ENROLLING_BY_INVITATION", "AVAILABLE",
                     "ACTIVE_NOT_RECRUITING", "COMPLETED", "TERMINATED", "SUSPENDED",
                     "WITHDRAWN", "UNKNOWN", "WITHHELD", "SOMETHING_NEW",
                 })
        {
            Assert.Equal(
                BrainHarbor.Pipeline.Sources.CtGovFetcher.PlainStatus(raw),
                Registry.PlainStatus(raw));
        }

        foreach (var phases in new[]
                 {
                     new[] { "PHASE1" }, ["PHASE2", "PHASE3"], ["NA"], ["EARLY_PHASE1"], [],
                 })
        {
            Assert.Equal(
                BrainHarbor.Pipeline.Sources.CtGovFetcher.PlainPhase(phases),
                Registry.PlainPhase(phases));
        }

        Assert.Equal(
            BrainHarbor.Pipeline.Sources.CtGovFetcher.ConditionQuery,
            Registry.BrainTumorConditionQuery);
    }
}
