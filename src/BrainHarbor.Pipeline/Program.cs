using System.Net.Http.Headers;
using BrainHarbor.Pipeline;
using BrainHarbor.Pipeline.Publishing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// BrainHarbor.Pipeline (architecture.md §3): a stateless console app on Dan's
// PC, run daily by Task Scheduler. Fetches sources, (from M3) classifies and
// summarizes via the Claude Code CLI, and uploads results as PENDING through
// the sync API. It holds only the sync API key — never database credentials.
//
//   --once   run one pass and exit (the only mode today; the scheduler is the
//            loop, so the app never daemonizes)

// Only --once is understood. Anything else is almost certainly a typo, and a
// silently-ignored flag on an unattended nightly job hides real mistakes.
var unknownArgs = args.Where(a => !string.Equals(a, "--once", StringComparison.Ordinal)).ToList();
if (unknownArgs.Count > 0)
{
    Console.Error.WriteLine($"Unknown argument(s): {string.Join(" ", unknownArgs)}");
    Console.Error.WriteLine("Usage: BrainHarbor.Pipeline [--once]   (one pass is the only mode)");
    return 3;
}

// Deliberately NOT passing args to the config builder: that would make
// `--Pipeline:SyncApiKey <secret>` bind from the command line, putting the key
// in the process list. Secrets come from user-secrets or the environment.
var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddUserSecrets<PipelineMarker>(optional: true);
builder.Configuration.AddEnvironmentVariables("BRAINHARBOR_");

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services
    .AddOptions<PipelineOptions>()
    .Bind(builder.Configuration.GetSection(PipelineOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<ISyncApiClient, SyncApiClient>((provider, client) =>
{
    var options = provider.GetRequiredService<IOptions<PipelineOptions>>().Value;
    client.BaseAddress = new Uri(options.SyncApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
    client.DefaultRequestHeaders.Add("X-BrainHarbor-Key", options.SyncApiKey);
    client.DefaultRequestHeaders.UserAgent.Add(
        new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
})
// A once-daily unattended job must survive a transient blip rather than lose
// a source for 24 hours. Retries are safe here: /check is a read, and /items
// is idempotent on (source, external_id).
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromSeconds(2);
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    options.Retry.UseJitter = true;
});

// Fetchers register here — WI-204 (PubMed), WI-205 (RSS), WI-206 (preprints).
// The runner iterates whatever is registered, so adding one is a single line.
builder.Services.AddTransient<PipelineRunner>();

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Pipeline");

// Ctrl+C / Task Scheduler stop should end the run cleanly, not mid-upload.
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    logger.LogWarning("Cancellation requested — finishing the current step.");
    cancellation.Cancel();
};

try
{
    var runner = host.Services.GetRequiredService<PipelineRunner>();
    var result = await runner.RunAsync(cancellation.Token);

    // Exit codes are what Task Scheduler surfaces, so they're distinct:
    //   0 all sources ok · 1 some sources failed · 2 cancelled
    //   3 bad config/arguments · 4 the run itself blew up
    return result.Failures.Count == 0 ? 0 : 1;
}
catch (OperationCanceledException)
{
    logger.LogWarning("Run cancelled.");
    return 2;
}
catch (OptionsValidationException exception)
{
    logger.LogError("Configuration is invalid: {Errors}", string.Join("; ", exception.Failures));
    logger.LogError("Set it with: dotnet user-secrets set \"Pipeline:SyncApiKey\" \"...\" " +
                    "--project src/BrainHarbor.Pipeline");
    return 3;
}
catch (Exception exception)
{
    logger.LogError(exception, "Run failed.");
    return 4;
}

/// <summary>Anchor type for user-secrets and tests.</summary>
public sealed class PipelineMarker;
