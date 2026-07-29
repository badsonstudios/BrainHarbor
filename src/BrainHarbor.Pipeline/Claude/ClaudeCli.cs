using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Claude;

/// <summary>
/// Outcome of a structured Claude call. On any failure — process error,
/// timeout, malformed output, failed validation — Success is false and Value
/// is default. There is no partial or guessed value: a bad summary must never
/// look like a good one (content-pipeline.md §9, "never publish a guess").
/// </summary>
public sealed record ClaudeResult<T>(bool Success, T? Value, string? FailureReason)
{
    public static ClaudeResult<T> Ok(T value) => new(true, value, null);
    public static ClaudeResult<T> Fail(string reason) => new(false, default, reason);
}

/// <summary>
/// WI-302: reliable programmatic access to the local `claude` CLI. Invokes it,
/// unwraps the JSON envelope, parses the model's JSON output into the expected
/// shape, validates it, and retries ONCE on any failure. If it still fails,
/// the caller flags the item unsummarized rather than shipping a guess.
/// </summary>
public sealed class ClaudeCli(IProcessRunner runner, ILogger<ClaudeCli> logger)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<ClaudeResult<T>> RunJsonAsync<T>(
        string prompt, Func<T, bool>? validate, CancellationToken cancellationToken)
    {
        var (first, retryable) = await TryOnceAsync(prompt, validate, cancellationToken);
        if (first.Success || !retryable)
        {
            // Fail fast on deterministic failures — a timeout, an auth-style
            // non-zero exit, or a validation failure will fail identically on
            // a retry, so a second call just burns time and subscription cost.
            return first;
        }

        logger.LogWarning("Claude call failed ({Reason}) — retrying once.", first.FailureReason);
        var (second, _) = await TryOnceAsync(prompt, validate, cancellationToken);
        if (!second.Success)
        {
            logger.LogWarning("Claude call failed again ({Reason}) — giving up (item stays unsummarized).",
                second.FailureReason);
        }
        return second;
    }

    /// <summary>Runs one call. `retryable` is true only for non-deterministic
    /// failures (garbled output) — not timeouts, exit codes, or validation.</summary>
    private async Task<(ClaudeResult<T> Result, bool Retryable)> TryOnceAsync<T>(
        string prompt, Func<T, bool>? validate, CancellationToken cancellationToken)
    {
        ProcessResult proc;
        try
        {
            proc = await runner.RunAsync(prompt, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The runner shouldn't throw, but the contract is "never throw" —
            // belt and suspenders so a bug there can't crash the run.
            return (ClaudeResult<T>.Fail($"claude runner threw: {exception.Message}"), false);
        }

        if (proc.TimedOut)
        {
            return (ClaudeResult<T>.Fail("claude timed out"), false);
        }

        if (proc.ExitCode != 0)
        {
            var detail = proc.Stderr.Trim();
            return (ClaudeResult<T>.Fail(
                $"claude exited {proc.ExitCode}{(detail.Length > 0 ? $": {Truncate(detail, 200)}" : "")}"), false);
        }

        // 1) Unwrap the CLI's JSON envelope and pull out the model's text.
        //    Garbled envelope/output is non-deterministic → retryable.
        string? resultText;
        try
        {
            using var envelope = JsonDocument.Parse(proc.Stdout);
            var root = envelope.RootElement;

            if (root.TryGetProperty("is_error", out var isError) &&
                isError.ValueKind == JsonValueKind.True)
            {
                return (ClaudeResult<T>.Fail("claude reported is_error=true"), true);
            }

            if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.String)
            {
                return (ClaudeResult<T>.Fail("claude envelope had no string 'result'"), true);
            }

            resultText = result.GetString();
        }
        catch (JsonException)
        {
            return (ClaudeResult<T>.Fail("claude stdout was not the expected JSON envelope"), true);
        }

        if (string.IsNullOrWhiteSpace(resultText))
        {
            return (ClaudeResult<T>.Fail("claude returned an empty result"), true);
        }

        // 2) Parse the model's own JSON output into the expected shape.
        T? value;
        try
        {
            value = JsonSerializer.Deserialize<T>(StripCodeFences(resultText), Json);
        }
        catch (JsonException)
        {
            return (ClaudeResult<T>.Fail("model output was not valid JSON for the expected shape"), true);
        }

        if (value is null)
        {
            return (ClaudeResult<T>.Fail("model output deserialized to null"), true);
        }

        if (validate is not null && !validate(value))
        {
            // Deterministic: the same output would fail validation again.
            return (ClaudeResult<T>.Fail("model output failed validation"), false);
        }

        return (ClaudeResult<T>.Ok(value), false);
    }

    /// <summary>
    /// Models often wrap JSON in a ```json … ``` fence despite instructions.
    /// Strip it rather than fail an otherwise-good response.
    /// </summary>
    internal static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0)
        {
            return trimmed;
        }

        var body = trimmed[(firstNewline + 1)..];
        var lastFence = body.LastIndexOf("```", StringComparison.Ordinal);
        return (lastFence >= 0 ? body[..lastFence] : body).Trim();
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max];
}
