using System.Text;

namespace BrainHarbor.Pipeline.Logging;

/// <summary>
/// WI-417: the run log file itself — one file per run, scrubbed, size-capped,
/// and pruned so the directory cannot grow without bound.
///
/// Nothing here may fail a run. A pipeline that cannot write its log still has
/// a job to do, so every filesystem operation degrades to "no log" (or "no
/// pruning") and says so on stderr rather than throwing.
/// </summary>
public sealed class FileLogSink : IDisposable
{
    private readonly object _gate = new();
    private readonly LogRedactor _redactor;
    private readonly long _maxBytes;

    private StreamWriter? _writer;
    private FileStream? _stream;
    private bool _stopped;

    private FileLogSink(
        StreamWriter writer, FileStream stream, string path, LogRedactor redactor, long maxBytes)
    {
        _writer = writer;
        _stream = stream;
        _redactor = redactor;
        _maxBytes = maxBytes;
        Path = path;
    }

    /// <summary>Full path of this run's log file.</summary>
    public string Path { get; }

    /// <summary>
    /// Opens this run's file and prunes old ones. Returns null when logging to
    /// disk is impossible (no permission, read-only path) — the caller carries
    /// on with console logging only.
    /// </summary>
    public static FileLogSink? Create(
        FileLogOptions options, LogRedactor redactor, DateTimeOffset startedAt)
    {
        var directory = options.ResolvedDirectory;

        try
        {
            Directory.CreateDirectory(directory);

            var (stream, path) = OpenRunFile(directory, startedAt);

            // AutoFlush because Task Scheduler kills a run that hits its
            // ExecutionTimeLimit, and that is precisely the run whose log
            // matters most. Buffered writes would die with the process.
            var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true,
            };

            var sink = new FileLogSink(
                writer, stream, path, redactor, Math.Max(1, options.MaxFileMegabytes) * 1024L * 1024L);

            Prune(directory, path, options);
            return sink;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or NotSupportedException
                or ArgumentException)
        {
            Console.Error.WriteLine(
                $"Could not open a log file in {directory} ({exception.Message}); " +
                "continuing with console logging only.");
            return null;
        }
    }

    /// <summary>
    /// Opens this run's file. Named to the second, not just the date, so a
    /// manual run cannot overwrite the 06:00 scheduled one — and when two runs
    /// start inside the SAME second, the loser of the race takes a suffix
    /// rather than silently going without a log.
    /// </summary>
    private static (FileStream Stream, string Path) OpenRunFile(
        string directory, DateTimeOffset startedAt)
    {
        var stamp = startedAt.LocalDateTime.ToString("yyyyMMdd-HHmmss");

        for (var attempt = 0; ; attempt++)
        {
            var name = attempt == 0 ? $"pipeline-{stamp}.log" : $"pipeline-{stamp}-{attempt}.log";
            var path = System.IO.Path.Combine(directory, name);

            try
            {
                // CreateNew, not Append: it fails atomically if the name is
                // already taken, on every platform. Relying on the sharing
                // violation instead would work on Windows and quietly do the
                // wrong thing on Linux, where FileShare is advisory — the two
                // runs would interleave into one file.
                //
                // FileShare.Read so the file can be tailed WHILE the run is
                // going — a two-hour backfill you cannot watch is only half an
                // improvement. (A reader must in turn share writes, since this
                // handle holds write access: Get-Content and tail do,
                // File.ReadAllText does not.) Deliberately NOT FileShare.Delete:
                // that would let another run's pruning delete this file out from
                // under a live run, which on Windows succeeds silently and loses
                // the whole log.
                return (
                    new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read),
                    path);
            }
            catch (IOException) when (attempt < 8)
            {
                // That name is taken — by a run in the same second, or by one
                // that already finished. Try the next suffix.
            }
        }
    }

    public void Write(string line)
    {
        lock (_gate)
        {
            if (_writer is null || _stream is null || _stopped)
            {
                return;
            }

            try
            {
                var scrubbed = _redactor.Scrub(line);

                // The stream's own position, not the string's Length: log lines
                // are full of em dashes and other multi-byte characters, so
                // counting UTF-16 chars against a byte ceiling would let the
                // file grow well past the advertised cap.
                if (_stream.Position + Encoding.UTF8.GetByteCount(scrubbed) > _maxBytes)
                {
                    _stopped = true;
                    _writer.WriteLine(
                        $"... log truncated at {_maxBytes / (1024 * 1024)} MB. Something in this run " +
                        "is repeating itself; the exit code and the admin health page still stand.");
                    return;
                }

                _writer.WriteLine(scrubbed);
            }
            catch (Exception exception)
            {
                // Disk full, the file vanished, anything at all: this sink's
                // whole contract is that it cannot take a run down. An escaping
                // exception here would surface as an AggregateException from
                // ILogger.Log at the CALL site — including the one in Main's
                // finally, where it would replace a meaningful exit code with a
                // crash. Say it once on stderr and stop trying.
                _stopped = true;
                Console.Error.WriteLine($"Log writing stopped: {exception.Message}");
            }
        }
    }

    /// <summary>
    /// How recently a file must have been written to be treated as another
    /// run's LIVE log rather than an old one. A run writes continuously, so
    /// anything touched this recently may still be in use — and deleting it
    /// would take that run's whole log with it.
    /// </summary>
    private static readonly TimeSpan AssumeStillRunning = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Retention (Dan's ask: the log directory must not quietly eat the disk).
    /// Three limits, because each one has a hole the others cover: age misses a
    /// task re-triggered in a loop (a hundred files in an hour, all of them
    /// young), count misses one enormous file, and neither bounds the directory
    /// in bytes. The active file is never a candidate, and nor is anything that
    /// looks like another run still in progress.
    /// </summary>
    private static void Prune(string directory, string activePath, FileLogOptions options)
    {
        try
        {
            var now = DateTime.UtcNow;
            var candidates = new DirectoryInfo(directory)
                .GetFiles("pipeline-*.log")
                .Where(f => !string.Equals(f.FullName, activePath, StringComparison.OrdinalIgnoreCase))
                .Where(f => now - f.LastWriteTimeUtc > AssumeStillRunning)
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .ToList();

            var cutoff = now.AddDays(-Math.Max(1, options.RetentionDays));
            var keep = Math.Max(0, options.MaxFiles - 1);   // -1: this run's file counts too
            var budget = Math.Max(1, options.MaxDirectoryMegabytes) * 1024L * 1024L;
            var used = 0L;

            for (var i = 0; i < candidates.Count; i++)
            {
                var file = candidates[i];
                used += file.Length;

                if (i >= keep || file.LastWriteTimeUtc < cutoff || used > budget)
                {
                    TryDelete(file);
                }
            }
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Could not prune old logs in {directory}: {exception.Message}");
        }
    }

    private static void TryDelete(FileInfo file)
    {
        try
        {
            file.Delete();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // A log someone has open in an editor is not worth a failed run.
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _writer?.Dispose();   // disposes the stream with it
            _writer = null;
            _stream = null;
        }
    }
}
