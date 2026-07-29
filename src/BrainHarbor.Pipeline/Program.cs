using System.Net.Http.Headers;
using BrainHarbor.Pipeline.Publishing;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrainHarbor.Pipeline;

/// <summary>
/// BrainHarbor.Pipeline (architecture.md §3): a stateless console app on Dan's
/// PC, run daily by Task Scheduler. Fetches sources, (from M3) classifies and
/// summarizes via the Claude Code CLI, and uploads results as PENDING through
/// the sync API. It holds only the sync API key — never database credentials.
///
/// Usage: BrainHarbor.Pipeline [--once]   (one pass is the only mode; the
/// scheduler is the loop, so the app never daemonizes)
///
/// Exit codes, which is what Task Scheduler history shows:
///   0 all sources ok · 1 some sources failed · 2 cancelled
///   3 bad configuration or arguments · 4 the run itself blew up
///
/// (A named entry point rather than top-level statements: the generated
/// Program class would collide with the Web app's inside the shared test
/// project, which references both.)
/// </summary>
public static class PipelineHost
{
    public static async Task<int> Main(string[] args)
    {
        // Only --once is understood. A silently-ignored typo'd flag on an
        // unattended nightly job hides real mistakes.
        var unknownArgs = args.Where(a => !string.Equals(a, "--once", StringComparison.Ordinal)).ToList();
        if (unknownArgs.Count > 0)
        {
            Console.Error.WriteLine($"Unknown argument(s): {string.Join(" ", unknownArgs)}");
            Console.Error.WriteLine("Usage: BrainHarbor.Pipeline [--once]   (one pass is the only mode)");
            return 3;
        }

        // Deliberately NOT passing args to the config builder: that would make
        // `--Pipeline:SyncApiKey <secret>` bind from the command line, putting
        // the key in the process list. Secrets come from user-secrets or env.
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddUserSecrets<PipelineMarker>(optional: true);
        builder.Configuration.AddEnvironmentVariables("BRAINHARBOR_");

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });

        // HttpClient logs full request URIs at Information, and the NCBI key
        // travels as a query parameter (E-utilities requires it there). Task
        // Scheduler captures this console output, so turn it down.
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

        builder.Services
            .AddOptions<PipelineOptions>()
            .Bind(builder.Configuration.GetSection(PipelineOptions.SectionName))
            .PostConfigure(options =>
            {
                // api-keys-config.md documents these as flat names, and that
                // is how they are already stored in user-secrets and .env.
                // The Pipeline: section wins when both are present.
                if (string.IsNullOrWhiteSpace(options.SyncApiKey))
                {
                    options.SyncApiKey = builder.Configuration["SYNC_API_KEY"] ?? "";
                }

                if (string.IsNullOrWhiteSpace(options.NcbiApiKey))
                {
                    options.NcbiApiKey = builder.Configuration["NCBI_API_KEY"];
                }
            })
            .ValidateDataAnnotations();

        builder.Services.AddHttpClient<ISyncApiClient, SyncApiClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<PipelineOptions>>().Value;
            client.BaseAddress = new Uri(options.SyncApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            client.DefaultRequestHeaders.Add("X-BrainHarbor-Key", options.SyncApiKey);
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        })
        // A once-daily unattended job must survive a transient blip rather than
        // lose a source for 24 hours. Retries are safe: /check is a read and
        // /items is idempotent on (source, external_id).
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(2);
            options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
        });

        // Fetchers. The runner iterates whatever is registered, so adding a
        // source is one line here plus the fetcher itself.
        builder.Services.AddHttpClient<PubMedFetcher>(client =>
        {
            client.BaseAddress = new Uri("https://eutils.ncbi.nlm.nih.gov/entrez/eutils/");
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        })
        .AddStandardResilienceHandler();
        builder.Services.AddTransient<ISourceFetcher>(sp => sp.GetRequiredService<PubMedFetcher>());

        // News feeds (WI-205). Each carries its own licensing policy; see
        // RssFeedDefinition — NCI is public domain, ScienceDaily is
        // headline + teaser + link only.
        builder.Services.AddHttpClient("rss", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        })
        .AddStandardResilienceHandler();

        foreach (var feed in new[] { RssFeedDefinition.Nci, RssFeedDefinition.ScienceDaily })
        {
            var definition = feed;
            builder.Services.AddTransient<ISourceFetcher>(sp => new RssFetcher(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("rss"),
                sp.GetRequiredService<ILogger<RssFetcher>>(),
                definition));
        }

        // Preprints (WI-206) — metadata only, permanently badged.
        builder.Services.AddHttpClient("preprint", client =>
        {
            client.BaseAddress = new Uri("https://api.biorxiv.org/");
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        })
        .AddStandardResilienceHandler();

        foreach (var preprintServer in new[] { "medrxiv", "biorxiv" })
        {
            var name = preprintServer;
            builder.Services.AddTransient<ISourceFetcher>(sp => new PreprintFetcher(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("preprint"),
                sp.GetRequiredService<ILogger<PreprintFetcher>>(),
                name));
        }

        builder.Services.AddTransient<PipelineRunner>();
        builder.Services.AddSingleton<DesktopNotifier>();

        using var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Pipeline");

        // Ctrl+C should end the run cleanly rather than mid-upload. (Task
        // Scheduler's End Task terminates the process outright; nothing can
        // catch that.)
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            logger.LogWarning("Cancellation requested — finishing the current step.");
            cancellation.Cancel();
        };

        try
        {
            // Force config validation up front so a bad setting is exit code 3
            // ("fix your config") rather than surfacing later as a source
            // failure, which reads as "the feed is down".
            _ = host.Services.GetRequiredService<IOptions<PipelineOptions>>().Value;

            var runner = host.Services.GetRequiredService<PipelineRunner>();
            var result = await runner.RunAsync(cancellation.Token);

            // A scheduled task that finishes silently is one nobody notices
            // has stopped working.
            host.Services.GetRequiredService<DesktopNotifier>().Notify(result);

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
    }
}

/// <summary>Anchor type for user-secrets and tests.</summary>
public sealed class PipelineMarker;
