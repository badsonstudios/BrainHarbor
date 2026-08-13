using BrainHarbor.Pipeline.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-417: the daily run leaves evidence behind.
///
/// Task Scheduler captures no console output, so everything the pipeline
/// printed — which item was excluded and why, which summaries were flagged and
/// for what — went nowhere. These tests cover the three things that make a log
/// file safe to leave running unattended: it gets written, it never carries a
/// key, and it cannot fill the disk.
/// </summary>
public class PipelineLoggingTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(), "brainharbor-logtests", Guid.NewGuid().ToString("n"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }
        catch (IOException)
        {
            // A temp directory that outlives the test run is not a test failure.
        }
        GC.SuppressFinalize(this);
    }

    private FileLogOptions Options() => new() { Directory = _directory };

    private static FileLogSink Sink(FileLogOptions options, params string?[] secrets) =>
        FileLogSink.Create(options, new LogRedactor(secrets), DateTimeOffset.Now)
            ?? throw new InvalidOperationException("the sink should open in a temp directory");

    private static ILoggerFactory FactoryFor(FileLogSink sink) =>
        LoggerFactory.Create(builder => builder.AddProvider(new FileLoggerProvider(sink)));

    /// <summary>
    /// Reads a log the way a tail window does — sharing the writer's handle.
    /// <c>File.ReadAllText</c> cannot: it asks for FileShare.Read, which does
    /// not permit the run's own write handle, so it fails while a run is live.
    /// </summary>
    private static string ReadLog(string path)
    {
        using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // ---------- it gets written ----------

    [Fact]
    public void ARunWritesItsOwnFileAndTheLinesLandInIt()
    {
        var sink = Sink(Options());
        using (var factory = FactoryFor(sink))
        {
            factory.CreateLogger("Pipeline").LogInformation(
                "[pubmed] excluded 12345 as off-topic: A study of knees");
        }

        Assert.StartsWith(
            "pipeline-", Path.GetFileName(sink.Path), StringComparison.Ordinal);
        Assert.EndsWith(".log", sink.Path, StringComparison.Ordinal);

        var text = ReadLog(sink.Path);
        Assert.Contains("excluded 12345 as off-topic: A study of knees", text, StringComparison.Ordinal);
        Assert.Contains("info", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFileCanBeReadWhileTheRunIsStillGoing()
    {
        // A two-hour backfill you cannot tail is only half an improvement, and
        // an exclusive lock would also mean nothing survives an End Task.
        using var sink = Sink(Options());
        using var factory = FactoryFor(sink);
        factory.CreateLogger("Pipeline").LogInformation("halfway through");

        // Read it with the run still holding the file open — this is the
        // Get-Content / tail case, and it is also why an End Task at the
        // two-hour limit still leaves everything written so far on disk.
        Assert.Contains("halfway through", ReadLog(sink.Path), StringComparison.Ordinal);
    }

    [Fact]
    public void TwoRunsInTheSameDayDoNotOverwriteEachOther()
    {
        var options = Options();
        var first = FileLogSink.Create(options, new LogRedactor([]), new DateTimeOffset(2026, 8, 14, 6, 0, 0, TimeSpan.Zero).ToLocalTime())!;
        var second = FileLogSink.Create(options, new LogRedactor([]), new DateTimeOffset(2026, 8, 14, 10, 30, 0, TimeSpan.Zero).ToLocalTime())!;

        first.Write("the scheduled 6am run");
        second.Write("a manual run later that morning");
        first.Dispose();
        second.Dispose();

        Assert.NotEqual(first.Path, second.Path);
        Assert.Contains("the scheduled 6am run", File.ReadAllText(first.Path), StringComparison.Ordinal);
        Assert.Contains("a manual run later", File.ReadAllText(second.Path), StringComparison.Ordinal);
    }

    [Fact]
    public void AnExceptionIsWrittenOutNotSwallowed()
    {
        var sink = Sink(Options());
        using (var factory = FactoryFor(sink))
        {
            factory.CreateLogger("Pipeline").LogError(
                new HttpRequestException("the feed is down"), "[nci_rss] failed.");
        }

        var text = ReadLog(sink.Path);
        Assert.Contains("[nci_rss] failed.", text, StringComparison.Ordinal);
        Assert.Contains("HttpRequestException", text, StringComparison.Ordinal);
        Assert.Contains("the feed is down", text, StringComparison.Ordinal);
    }

    // ---------- it never carries a key ----------

    [Fact]
    public void AConfiguredKeyIsScrubbedHoweverItReachesALogLine()
    {
        const string ncbiKey = "0123456789abcdef0123456789abcdef";
        var sink = Sink(Options(), ncbiKey);

        using (var factory = FactoryFor(sink))
        {
            factory.CreateLogger("Pipeline").LogWarning(
                "GET https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=pubmed&api_key={Key}",
                ncbiKey);
        }

        var text = ReadLog(sink.Path);
        Assert.DoesNotContain(ncbiKey, text, StringComparison.Ordinal);
        Assert.Contains(LogRedactor.Placeholder, text, StringComparison.Ordinal);
        // Still diagnosable: the request is recognisable without the secret.
        Assert.Contains("esearch.fcgi", text, StringComparison.Ordinal);
    }

    [Theory]
    // The shapes a key travels in, for keys THIS process was never handed.
    [InlineData("GET /esearch.fcgi?db=pubmed&api_key=abcd1234efgh5678&term=glioma", "abcd1234efgh5678")]
    [InlineData("header X-BrainHarbor-Key: s3cret-value-here", "s3cret-value-here")]
    [InlineData("SYNC_API_KEY=another-secret-value", "another-secret-value")]
    public void AKeyShapedValueIsScrubbedEvenWhenItWasNeverConfigured(string line, string secret)
    {
        var redactor = new LogRedactor([]);

        var scrubbed = redactor.Scrub(line);

        Assert.DoesNotContain(secret, scrubbed, StringComparison.Ordinal);
        Assert.Contains(LogRedactor.Placeholder, scrubbed, StringComparison.Ordinal);
    }

    [Fact]
    public void AQueryStringKeepsTheParametersAfterAScrubbedKey()
    {
        // Truncating at the key would throw away the term and the date window,
        // which is the half of the URL worth having.
        var scrubbed = new LogRedactor([]).Scrub(
            "GET esearch.fcgi?db=pubmed&api_key=abcd1234efgh5678&reldate=7&term=glioma");

        Assert.DoesNotContain("abcd1234efgh5678", scrubbed, StringComparison.Ordinal);
        Assert.Contains("reldate=7", scrubbed, StringComparison.Ordinal);
        Assert.Contains("term=glioma", scrubbed, StringComparison.Ordinal);
    }

    [Fact]
    public void AShortOrEmptySecretDoesNotShredTheWholeLog()
    {
        // An unset key binds to "" and a dev stub is often a single letter.
        // Blind-replacing those would redact every line while looking correct.
        var redactor = new LogRedactor([null, "", "  ", "x"]);

        Assert.Equal(
            "[pubmed] excluded 12345 as off-topic: an x-ray study",
            redactor.Scrub("[pubmed] excluded 12345 as off-topic: an x-ray study"));
    }

    [Fact]
    public void TheHttpClientFilterThatHidesTheNcbiKeyIsStillInPlace()
    {
        // The NCBI key travels in the query string because E-utilities requires
        // it there, and IHttpClientFactory logs full request URIs at
        // Information. Nothing in the suite would have noticed this line being
        // deleted — and now the URIs would go to a FILE, not just a console.
        using var host = BuildHostWithRealLogging();
        var factory = host.Services.GetRequiredService<ILoggerFactory>();

        var http = factory.CreateLogger("System.Net.Http.HttpClient.PubMedFetcher.LogicalHandler");
        Assert.False(http.IsEnabled(LogLevel.Information));
        Assert.True(http.IsEnabled(LogLevel.Warning));

        // ...and the filter is targeted, not a blanket "log less".
        Assert.True(factory.CreateLogger("Pipeline").IsEnabled(LogLevel.Information));
    }

    // ---------- it cannot fill the disk ----------

    [Fact]
    public void LogsOlderThanTheRetentionWindowAreDeleted()
    {
        var stale = WriteExistingLog("pipeline-20260101-060000.log", DateTime.UtcNow.AddDays(-45));
        var recent = WriteExistingLog("pipeline-20260810-060000.log", DateTime.UtcNow.AddDays(-3));

        using var sink = Sink(Options());   // creating a sink prunes

        Assert.False(File.Exists(stale), "a 45-day-old run log should have been pruned");
        Assert.True(File.Exists(recent), "a 3-day-old run log is still within the 30-day window");
        Assert.True(File.Exists(sink.Path), "the active file must never be pruned");
    }

    [Fact]
    public void TooManyRecentLogsAreCappedByCount()
    {
        // The age limit alone cannot bound a task re-triggered in a loop: a
        // hundred files in an hour are all younger than 30 days. (Hours old,
        // not minutes: a file written minutes ago may be another run's LIVE
        // log, and pruning deliberately leaves those alone.)
        for (var i = 0; i < 12; i++)
        {
            WriteExistingLog($"pipeline-log{i:00}.log", DateTime.UtcNow.AddHours(-i - 1));
        }

        var options = Options();
        options.MaxFiles = 5;
        using var sink = Sink(options);

        var remaining = Directory.GetFiles(_directory, "pipeline-*.log");
        Assert.Equal(5, remaining.Length);
        Assert.Contains(sink.Path, remaining);
    }

    [Fact]
    public void TheDirectoryIsCappedInBytesNotJustInFiles()
    {
        // 100 files x 32 MB is 3.2 GB, which is not what "logs are pruned for
        // you" should mean on someone's PC.
        for (var i = 0; i < 6; i++)
        {
            var path = WriteExistingLog($"pipeline-big{i:00}.log", DateTime.UtcNow.AddHours(-i - 1));
            File.WriteAllBytes(path, new byte[400 * 1024]);
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddHours(-i - 1));
        }

        var options = Options();
        options.MaxDirectoryMegabytes = 1;      // ~2.5 of those files fit
        using var sink = Sink(options);

        var total = new DirectoryInfo(_directory).GetFiles("pipeline-*.log").Sum(f => f.Length);
        Assert.True(total <= 1024 * 1024, $"directory holds {total} bytes past a 1 MB budget");
        Assert.True(File.Exists(sink.Path));
    }

    [Fact]
    public void OneRunawayRunCannotGrowItsFilePastTheCap()
    {
        var options = Options();
        options.MaxFileMegabytes = 1;
        using var sink = Sink(options);

        var line = new string('x', 4096);
        for (var i = 0; i < 500; i++)   // ~2 MB if nothing stopped it
        {
            sink.Write(line);
        }

        var written = new FileInfo(sink.Path).Length;
        Assert.True(written <= 1024 * 1024 + 4096, $"log grew to {written} bytes past a 1 MB cap");
        Assert.Contains("log truncated", ReadLog(sink.Path), StringComparison.Ordinal);
    }

    [Fact]
    public void ALiveRunsLogIsNeverPrunedByAnotherRunStartingUp()
    {
        // Prune deletes by age and by count. A second run starting while a long
        // backfill is going must not delete the file that backfill is writing
        // into — on Windows that delete can succeed against an open handle, and
        // the running process would go on writing into nothing.
        var live = Sink(Options());
        live.Write("a backfill that is still going");

        var options = Options();
        options.MaxFiles = 1;                       // as aggressive as it gets
        using var second = Sink(options);

        Assert.True(File.Exists(live.Path), "the other run's live log was deleted underneath it");
        live.Write("still going");
        Assert.Contains("still going", ReadLog(live.Path), StringComparison.Ordinal);
        live.Dispose();
    }

    [Fact]
    public void TwoRunsStartingInTheSameSecondBothGetALog()
    {
        // Named to the second, so a manual run colliding with the scheduled one
        // would otherwise find the name taken and get no log at all — or, on a
        // platform where FileShare is advisory, silently interleave into one
        // file. Both runs must end up with their own.
        var startedAt = DateTimeOffset.Now;
        using var first = FileLogSink.Create(Options(), new LogRedactor([]), startedAt)!;
        using var second = FileLogSink.Create(Options(), new LogRedactor([]), startedAt)!;

        Assert.NotEqual(first.Path, second.Path);

        first.Write("run one");
        second.Write("run two");
        Assert.Contains("run one", ReadLog(first.Path), StringComparison.Ordinal);
        Assert.Contains("run two", ReadLog(second.Path), StringComparison.Ordinal);
    }

    [Fact]
    public void MultiByteCharactersCountAgainstTheSizeCapAsBytes()
    {
        // Log lines here are full of em dashes (3 bytes each in UTF-8).
        // Measuring the cap in UTF-16 chars would let the file run well past
        // the size it advertises.
        var options = Options();
        options.MaxFileMegabytes = 1;
        using var sink = Sink(options);

        var line = new string('—', 2048);          // 6 KB on disk, 2 K chars
        for (var i = 0; i < 400; i++)
        {
            sink.Write(line);
        }

        Assert.True(new FileInfo(sink.Path).Length <= 1024 * 1024 + 8192,
            $"log grew to {new FileInfo(sink.Path).Length} bytes past a 1 MB cap");
    }

    [Fact]
    public void WritingToAClosedSinkIsIgnoredRatherThanThrowing()
    {
        // ILogger.Log wraps a provider's exception in an AggregateException at
        // the CALL site. One of those call sites is Main's finally, where it
        // would replace a meaningful exit code with a crash.
        var sink = Sink(Options());
        sink.Dispose();

        var exception = Record.Exception(() => sink.Write("after the end"));

        Assert.Null(exception);
    }

    [Fact]
    public void FileLoggingCanBeTurnedOffEntirely()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{FileLogOptions.SectionName}:Enabled"] = "false",
            [$"{FileLogOptions.SectionName}:Directory"] = _directory,
        });

        var sink = BrainHarbor.Pipeline.PipelineHost.ConfigureLogging(builder);

        Assert.Null(sink);
        Assert.False(Directory.Exists(_directory), "nothing should have been created");
    }

    [Fact]
    public void AnUnwritableDirectoryCostsTheLogNotTheRun()
    {
        // A pipeline that cannot write its log still has a job to do.
        var options = new FileLogOptions
        {
            // A path under a FILE, which can never be a directory.
            Directory = Path.Combine(WriteExistingLog("pipeline-blocker.log", DateTime.UtcNow), "logs"),
        };

        var sink = FileLogSink.Create(options, new LogRedactor([]), DateTimeOffset.Now);

        Assert.Null(sink);
    }

    private string WriteExistingLog(string name, DateTime lastWriteUtc)
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, "an earlier run\n");
        File.SetLastWriteTimeUtc(path, lastWriteUtc);
        return path;
    }

    /// <summary>
    /// The real logging configuration, built the way Main builds it — with the
    /// file provider pointed at a temp directory so the suite never writes to
    /// %LOCALAPPDATA%.
    /// </summary>
    private IHost BuildHostWithRealLogging()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{FileLogOptions.SectionName}:Directory"] = _directory,
        });

        BrainHarbor.Pipeline.PipelineHost.ConfigureLogging(builder);
        return builder.Build();
    }
}
