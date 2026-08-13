using System.Net.Http.Headers;
using BrainHarbor.Pipeline.Logging;
using BrainHarbor.Pipeline.Publishing;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
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

        var logFile = ConfigureLogging(builder);

        ConfigureServices(builder);

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
        finally
        {
            // Last line of every run, including the failed ones — those are the
            // runs whose log someone actually goes looking for. In the finally
            // so the path is printed whichever exit code we leave on; before the
            // host (and with it the file provider) is disposed.
            if (logFile is not null)
            {
                logger.LogInformation("Log written to {Path}", logFile.Path);

                // Closed explicitly: a provider added as an INSTANCE is not
                // disposed by the container or by LoggerFactory, so nothing else
                // would ever release the handle. AutoFlush means no lines are
                // lost today either way — but that would stop being true the
                // moment anyone buffers writes for speed.
                logFile.Dispose();
            }
        }
    }

    /// <summary>
    /// Console + per-run file logging (WI-417). Extracted from Main so a test
    /// can build the SAME logging configuration — the HttpClient filter below
    /// is a secrets-hygiene rule, and until now nothing would have noticed if
    /// somebody deleted the line.
    ///
    /// Returns the run's log file, or null when file logging is off or the
    /// directory cannot be written.
    /// </summary>
    internal static FileLogSink? ConfigureLogging(HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });

        // HttpClient logs full request URIs at Information, and the NCBI key
        // travels as a query parameter (E-utilities requires it there). This
        // now keeps the key out of a FILE as well as off the console, which is
        // the more durable exposure — pinned by PipelineLoggingTests.
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

        var options = builder.Configuration
            .GetSection(FileLogOptions.SectionName)
            .Get<FileLogOptions>() ?? new FileLogOptions();

        if (!options.Enabled)
        {
            return null;
        }

        // The redactor is given the key values this process actually holds, so
        // it can scrub them however they reach a log line. Read from
        // configuration directly: the options graph is not built yet, and both
        // the sectioned and flat names are in play (see ConfigureServices).
        var redactor = new LogRedactor(
        [
            builder.Configuration[$"{PipelineOptions.SectionName}:{nameof(PipelineOptions.SyncApiKey)}"],
            builder.Configuration[$"{PipelineOptions.SectionName}:{nameof(PipelineOptions.NcbiApiKey)}"],
            .. PipelineOptions.FlatKeyNames.Select(name => builder.Configuration[name]),
        ]);

        var sink = FileLogSink.Create(options, redactor, DateTimeOffset.Now);
        if (sink is not null)
        {
            builder.Logging.AddProvider(new FileLoggerProvider(sink));
        }

        return sink;
    }

    /// <summary>
    /// The service graph, extracted so a test can build the SAME one. The
    /// resilience timeouts below are DI configuration, which no test could
    /// reach while this lived inline — and a wrong default here cost a
    /// production backfill (see AllowLongRequests).
    /// </summary>
    internal static void ConfigureServices(HostApplicationBuilder builder)
    {
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
                    options.SyncApiKey = builder.Configuration[PipelineOptions.SyncApiKeyFlatName] ?? "";
                }

                if (string.IsNullOrWhiteSpace(options.NcbiApiKey))
                {
                    options.NcbiApiKey = builder.Configuration[PipelineOptions.NcbiApiKeyFlatName];
                }
            })
            .ValidateDataAnnotations();

        // Read straight from configuration because the handler is built at
        // registration time, before the options are bound — and the package
        // version here has no Configure((options, provider)) overload to defer
        // it with. TryParse rather than GetValue<int?>: a junk value must fall
        // back and let ValidateDataAnnotations report it as a config error
        // (exit code 3), not crash the host with an unhandled exception.
        var syncRequestTimeout = int.TryParse(
            builder.Configuration[
                $"{PipelineOptions.SectionName}:{nameof(PipelineOptions.RequestTimeoutSeconds)}"],
            out var configuredTimeout)
            ? configuredTimeout
            : new PipelineOptions().RequestTimeoutSeconds;

        builder.Services.AddHttpClient<ISyncApiClient, SyncApiClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<PipelineOptions>>().Value;
            client.BaseAddress = new Uri(options.SyncApiBaseUrl);
            // NB: no client.Timeout here — the resilience handler below
            // replaces it with an infinite one. AllowLongRequests is the timeout.
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

            // Uploads carry a whole window of summarized items, so this one
            // must honour the configured timeout rather than the handler's
            // 30-second default (see AllowLongRequests).
            AllowLongRequests(options, syncRequestTimeout);
        });

        // Fetchers. The runner iterates whatever is registered, so adding a
        // source is one line here plus the fetcher itself.
        builder.Services.AddHttpClient<PubMedFetcher>(client =>
        {
            client.BaseAddress = new Uri("https://eutils.ncbi.nlm.nih.gov/entrez/eutils/");
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        })
        .AddStandardResilienceHandler(options => AllowLongRequests(options, 60));
        builder.Services.AddTransient<ISourceFetcher>(sp => sp.GetRequiredService<PubMedFetcher>());

        // News feeds (WI-205). Each carries its own licensing policy; see
        // RssFeedDefinition — NCI is public domain, ScienceDaily is
        // headline + teaser + link only.
        builder.Services.AddHttpClient("rss", client =>
        {
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        })
        .AddStandardResilienceHandler(options => AllowLongRequests(options, 60));

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
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        })
        .AddStandardResilienceHandler(options => AllowLongRequests(options, 60));

        foreach (var preprintServer in new[] { "medrxiv", "biorxiv" })
        {
            var name = preprintServer;
            builder.Services.AddTransient<ISourceFetcher>(sp => new PreprintFetcher(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("preprint"),
                sp.GetRequiredService<ILogger<PreprintFetcher>>(),
                name));
        }

        // ClinicalTrials.gov (WI-402) — public domain, no key. Deliberately
        // WITHOUT the standard resilience handler: the fetcher does its own
        // 429 handling (Retry-After aware), and two retry layers would multiply
        // each other's waits on a rate-limited registry.
        builder.Services.AddHttpClient<CtGovFetcher>(client =>
        {
            client.BaseAddress = new Uri("https://clinicaltrials.gov/api/v2/");
            client.Timeout = TimeSpan.FromSeconds(90);
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BrainHarborPipeline", "1.0"));
        });
        builder.Services.AddTransient<ISourceFetcher>(sp => sp.GetRequiredService<CtGovFetcher>());

        // Claude Code CLI wrapper (WI-302) — the LLM work runs through the
        // local `claude` CLI, no Anthropic API key.
        builder.Services
            .AddOptions<Claude.ClaudeOptions>()
            .Bind(builder.Configuration.GetSection(Claude.ClaudeOptions.SectionName))
            .ValidateDataAnnotations();
        builder.Services.AddSingleton<Claude.IProcessRunner, Claude.ClaudeProcessRunner>();
        builder.Services.AddSingleton<Claude.ClaudeCli>();
        builder.Services.AddSingleton<Claude.PromptLibrary>();

        // Classify + summarize steps (WI-303/304).
        builder.Services.AddSingleton<Classify.IItemClassifier, Classify.Classifier>();
        builder.Services.AddSingleton<Summarize.ISummarizer, Summarize.Summarizer>();

        builder.Services.AddTransient<PipelineRunner>();
        builder.Services.AddSingleton<DesktopNotifier>();

    }

    /// <summary>
    /// Sets the REAL timeouts for a client that has a resilience handler.
    ///
    /// Adding the standard handler replaces <c>HttpClient.Timeout</c> with an
    /// infinite one and caps requests at its own defaults instead: 30 seconds
    /// total, 10 per attempt. Both of this project's real windows are bigger
    /// than that — a catch-up efetch page from PubMed, and an upload carrying a
    /// whole window of summarized items. The first production backfill lost
    /// PubMed to the fetch cap and then threw away three hours of finished LLM
    /// work to the upload cap, because a <c>client.Timeout = 60s</c> line sat
    /// right there looking like it was in force. It was not. This is.
    ///
    /// Sampling duration only has to clear the validator's "at least twice the
    /// attempt timeout" rule; the breaker itself cannot trip at this request
    /// volume anyway (MinimumThroughput stays at its default 100).
    /// </summary>
    private static void AllowLongRequests(HttpStandardResilienceOptions options, int attemptSeconds)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(attemptSeconds);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(attemptSeconds * 3);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(attemptSeconds * 2.5);
    }
}

/// <summary>Anchor type for user-secrets and tests.</summary>
public sealed class PipelineMarker;
