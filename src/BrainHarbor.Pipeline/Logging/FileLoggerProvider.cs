using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Logging;

/// <summary>
/// WI-417: writes the same log the console gets to a per-run file, because
/// Task Scheduler captures no console output — so every nightly run's detail
/// (which item was excluded and why, which summaries were flagged and for what)
/// went nowhere. The last three production incidents were all diagnosed from
/// exactly that detail.
///
/// Registered alongside the console provider and subject to the same filters,
/// so the two can never disagree about what was worth recording.
/// </summary>
[ProviderAlias("File")]
public sealed class FileLoggerProvider(FileLogSink sink) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, sink);

    public void Dispose() => sink.Dispose();

    private sealed class FileLogger(string category, FileLogSink sink) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        // The LoggerFactory applies the configured filters before calling Log,
        // so the provider itself has nothing further to decide.
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception is null)
            {
                return;
            }

            // Full date, unlike the console's HH:mm:ss — a file read three weeks
            // later has to say which day it is talking about.
            var line =
                $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {Short(logLevel)} [{Trim(category)}] {message}";

            if (exception is not null)
            {
                line += Environment.NewLine + exception;
            }

            sink.Write(line);
        }

        private static string Short(LogLevel level) => level switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => "none",
        };

        /// <summary>Namespaces make every line start with the same 30 characters
        /// and push the message off the screen; the type name is the useful part.</summary>
        private static string Trim(string category)
        {
            var lastDot = category.LastIndexOf('.');
            return lastDot < 0 ? category : category[(lastDot + 1)..];
        }
    }
}
