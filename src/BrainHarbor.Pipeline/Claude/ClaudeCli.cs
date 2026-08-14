using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Claude;

/// <summary>
/// WI-413: WHY a Claude call failed — the distinction the callers need and the
/// CLI already knows.
///
/// The pipeline used to infer "the CLI is down" from a STREAK of three failures,
/// because a failure carried no cause. That is the wrong signal in both
/// directions: an outage beginning inside the last item or two of a small
/// window never reaches the threshold, and those items upload as permanently
/// unclassified — the state that had to be undone by deleting 532 rows from
/// production by hand. Treating an all-failed window as an outage instead would
/// stall a source forever on one item that can never be classified.
/// </summary>
public enum ClaudeFailure
{
    None,

    /// <summary>
    /// The CLI never answered: it could not be started, it timed out, it exited
    /// non-zero, or it reported an error envelope. Infrastructure, not this
    /// item — every other item would fail the same way, so the caller must stop
    /// rather than mark the window unclassifiable.
    /// </summary>
    Unavailable,

    /// <summary>
    /// It answered, and the answer was unusable — garbled JSON, the wrong
    /// shape, or output that failed validation. That is about THIS item, so
    /// the item goes to a person and the run carries on.
    /// </summary>
    UnusableOutput,
}

/// <summary>
/// Outcome of a structured Claude call. On any failure — process error,
/// timeout, malformed output, failed validation — Success is false and Value
/// is default. There is no partial or guessed value: a bad summary must never
/// look like a good one (content-pipeline.md §9, "never publish a guess").
/// </summary>
public sealed record ClaudeResult<T>
{
    // Private so a failure can never be built without saying WHY it failed.
    // With a public positional constructor, `new(false, default, "...")` would
    // leave Failure at None, Unavailable would read false, and the caller would
    // treat a dead CLI as an odd item — uploading a window that can never be
    // classified again. Too much rides on this flag to leave that expressible.
    private ClaudeResult(bool success, T? value, string? failureReason, string? model, ClaudeFailure failure)
    {
        Success = success;
        Value = value;
        FailureReason = failureReason;
        Model = model;
        Failure = failure;
    }

    public bool Success { get; }
    public T? Value { get; }
    public string? FailureReason { get; }
    public string? Model { get; }
    public ClaudeFailure Failure { get; }

    public static ClaudeResult<T> Ok(T value, string? model) =>
        new(true, value, null, model, ClaudeFailure.None);

    public static ClaudeResult<T> Fail(string reason, ClaudeFailure failure) =>
        new(false, default, reason, null, failure);

    /// <summary>True when the CLI itself is the problem, not this item.</summary>
    public bool Unavailable => Failure == ClaudeFailure.Unavailable;
}

/// <summary>
/// Asks the CLI directly whether it is alive (WI-413), instead of guessing
/// from one item's failure.
///
/// It exists because the two causes are not always separable at the point of
/// failure. A timeout is the awkward case: a single abstract slow enough to
/// blow the timeout looks exactly like a dead CLI, and treating it as one
/// would stop the source and hold the cursor — so the same item leads the
/// window tomorrow and stalls it again, forever. One trivial prompt settles it.
/// </summary>
public interface IClaudeHealthProbe
{
    Task<bool> IsAliveAsync(CancellationToken cancellationToken);
}

/// <summary>
/// WI-302: reliable programmatic access to the local `claude` CLI. Invokes it,
/// unwraps the JSON envelope, parses the model's JSON output into the expected
/// shape, validates it, and retries ONCE on any failure. If it still fails,
/// the caller flags the item unsummarized rather than shipping a guess.
/// </summary>
public sealed class ClaudeCli(IProcessRunner runner, ILogger<ClaudeCli> logger) : IClaudeHealthProbe
{
    /// <summary>The cheapest possible question, with an answer that proves both
    /// that the CLI ran and that a model replied.</summary>
    private const string HealthPrompt =
        "Reply with exactly this JSON and nothing else: {\"ok\": true}";

    private sealed record HealthAnswer(bool Ok);

    /// <summary>
    /// True if the CLI answers a trivial prompt. Deliberately strict: anything
    /// short of a usable answer counts as dead, because the caller uses this to
    /// decide whether to keep spending model calls on a window.
    /// </summary>
    public async Task<bool> IsAliveAsync(CancellationToken cancellationToken)
    {
        var result = await RunJsonAsync<HealthAnswer>(
            HealthPrompt, answer => answer.Ok, cancellationToken);

        logger.LogInformation("Claude health check: {Verdict}.",
            result.Success ? "alive" : $"not answering ({result.FailureReason})");

        return result.Success;
    }

    // The model is prompted to return snake_case JSON (tumor_tags, etc.).
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

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
            return (ClaudeResult<T>.Fail(
                $"claude runner threw: {exception.Message}", ClaudeFailure.Unavailable), false);
        }

        if (proc.TimedOut)
        {
            return (ClaudeResult<T>.Fail("claude timed out", ClaudeFailure.Unavailable), false);
        }

        if (proc.ExitCode != 0)
        {
            // Covers more than it looks: ClaudeProcessRunner reports a spawn
            // failure ("claude" not installed, the Windows .cmd shim) and a
            // mid-call IO error as exit -1 rather than throwing. All of it is
            // "we never got an answer".
            var detail = proc.Stderr.Trim();
            return (ClaudeResult<T>.Fail(
                $"claude exited {proc.ExitCode}{(detail.Length > 0 ? $": {Truncate(detail, 200)}" : "")}",
                ClaudeFailure.Unavailable), false);
        }

        // 1) Unwrap the CLI's JSON envelope and pull out the model's text.
        //    Garbled envelope/output is non-deterministic → retryable.
        string? resultText;
        string? model = null;
        try
        {
            using var envelope = JsonDocument.Parse(proc.Stdout);
            var root = envelope.RootElement;

            if (root.TryGetProperty("is_error", out var isError) &&
                isError.ValueKind == JsonValueKind.True)
            {
                // Unavailable, though it is still retried once first (a one-off
                // refusal deserves a second chance; a persistent one does not).
                // A dead usage limit can present exactly like this, and the
                // costs are asymmetric: stopping wrongly loses a source one
                // day's freshness with the cursor held, while carrying on
                // wrongly writes rows that have to be deleted from production
                // by hand. When the envelope says "error", stop.
                return (ClaudeResult<T>.Fail(
                    "claude reported is_error=true", ClaudeFailure.Unavailable), true);
            }

            if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.String)
            {
                // Envelope-level, so Unavailable: the ENVELOPE is the CLI's own
                // output, not the model's. A garbled model answer still arrives
                // inside a well-formed envelope (as the `result` string), so an
                // envelope that is missing or misshapen means the CLI itself is
                // not behaving — a half-installed shim, or a banner printed
                // ahead of the JSON — and it will do that for every item.
                return (ClaudeResult<T>.Fail(
                    "claude envelope had no string 'result'", ClaudeFailure.Unavailable), true);
            }

            resultText = result.GetString();

            // Capture the real model id for auditability (classify_model /
            // summary_model) — a silent model switch should be traceable.
            if (root.TryGetProperty("model", out var modelElement) &&
                modelElement.ValueKind == JsonValueKind.String)
            {
                model = modelElement.GetString();
            }
        }
        catch (JsonException)
        {
            // Also the empty-stdout case (JsonDocument.Parse("") throws): the
            // CLI exited 0 and said nothing at all, which is not an answer.
            return (ClaudeResult<T>.Fail(
                "claude stdout was not the expected JSON envelope", ClaudeFailure.Unavailable), true);
        }

        if (string.IsNullOrWhiteSpace(resultText))
        {
            // A well-formed envelope carrying an empty result: the CLI IS
            // behaving — it produced its envelope — and the model returned
            // nothing for this prompt. That is about the item.
            return (ClaudeResult<T>.Fail(
                "claude returned an empty result", ClaudeFailure.UnusableOutput), true);
        }

        // 2) Parse the model's own JSON output into the expected shape.
        T? value;
        try
        {
            value = JsonSerializer.Deserialize<T>(StripCodeFences(resultText), Json);
        }
        catch (JsonException)
        {
            return (ClaudeResult<T>.Fail(
                "model output was not valid JSON for the expected shape", ClaudeFailure.UnusableOutput), true);
        }

        if (value is null)
        {
            return (ClaudeResult<T>.Fail(
                "model output deserialized to null", ClaudeFailure.UnusableOutput), true);
        }

        if (validate is not null && !validate(value))
        {
            // Deterministic: the same output would fail validation again. This
            // is the "odd item" case — the CLI is alive and answering, this
            // particular item just cannot be handled, so it goes to a person.
            return (ClaudeResult<T>.Fail(
                "model output failed validation", ClaudeFailure.UnusableOutput), false);
        }

        return (ClaudeResult<T>.Ok(value, model), false);
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
