using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-401: the timeouts the pipeline's HTTP clients ACTUALLY run with.
///
/// Adding a standard resilience handler replaces HttpClient.Timeout with an
/// infinite one and applies its own caps instead — 30 seconds total, 10 per
/// attempt by default. That default silently killed the first production
/// backfill twice: PubMed's catch-up page, and then an upload carrying three
/// hours of finished summarization work. Nothing in the suite touched the DI
/// graph, so nothing caught it. This does.
///
/// The options are named "{client}-standard"; for a typed client that is the
/// type name.
/// </summary>
public class PipelineHttpTimeoutTests
{
    private static HttpStandardResilienceOptions OptionsFor(string clientName)
    {
        // Build the real host the way the app does, then ask it what the
        // handler was actually configured with.
        var host = BuildPipelineHost();
        return host.Services
            .GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>()
            .Get($"{clientName}-standard");
    }

    private static IHost BuildPipelineHost()
    {
        var builder = Host.CreateApplicationBuilder();
        BrainHarbor.Pipeline.PipelineHost.ConfigureServices(builder);
        return builder.Build();
    }

    [Theory]
    [InlineData("ISyncApiClient")] // uploads a whole window of summaries
    [InlineData("PubMedFetcher")]  // catch-up efetch pages
    [InlineData("rss")]
    [InlineData("preprint")]
    public void EveryResilientClientAllowsAMinutePerAttempt(string clientName)
    {
        var options = OptionsFor(clientName);

        Assert.True(
            options.AttemptTimeout.Timeout >= TimeSpan.FromSeconds(60),
            $"{clientName} attempt timeout is {options.AttemptTimeout.Timeout} — the handler's " +
            "default is back, and a slow page or a big upload will be cut off mid-flight.");
        Assert.True(
            options.TotalRequestTimeout.Timeout >= options.AttemptTimeout.Timeout,
            $"{clientName} total timeout must not be shorter than one attempt.");
    }
}
