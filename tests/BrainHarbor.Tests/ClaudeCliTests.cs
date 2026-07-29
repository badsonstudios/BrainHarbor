using BrainHarbor.Pipeline.Claude;
using Microsoft.Extensions.Logging.Abstractions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-302: the Claude CLI wrapper. The load-bearing guarantee is that a bad
/// call never yields a value — process failure, timeout, malformed output, or
/// failed validation all return Success=false so the caller flags the item
/// unsummarized rather than shipping a guess (content-pipeline.md §9). Uses a
/// scripted fake CLI, so no process is spawned and no network is touched.
/// </summary>
public class ClaudeCliTests
{
    // Minimal shape the model is asked to return.
    public sealed record Extraction(string Tumor, int Count);

    /// <summary>A fake CLI that replays scripted results, one per call.</summary>
    private sealed class ScriptedRunner(params ProcessResult[] results) : IProcessRunner
    {
        private int _call;
        public int Calls => _call;

        public Task<ProcessResult> RunAsync(string prompt, CancellationToken cancellationToken)
        {
            var result = results[Math.Min(_call, results.Length - 1)];
            _call++;
            return Task.FromResult(result);
        }
    }

    private static ProcessResult Envelope(string modelResultJson) =>
        // What `claude -p --output-format json` prints: an envelope whose
        // `result` string holds the model's own output.
        new(0, $$"""{"type":"result","is_error":false,"result":{{System.Text.Json.JsonSerializer.Serialize(modelResultJson)}}}""", "", false);

    private static ClaudeCli Cli(IProcessRunner runner) => new(runner, NullLogger<ClaudeCli>.Instance);

    [Fact]
    public async Task ParsesAValidModelResponse()
    {
        var runner = new ScriptedRunner(Envelope("""{"tumor":"glioblastoma","count":331}"""));

        var result = await Cli(runner).RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("glioblastoma", result.Value!.Tumor);
        Assert.Equal(331, result.Value.Count);
        Assert.Equal(1, runner.Calls);
    }

    [Fact]
    public async Task StripsACodeFenceTheModelWrappedAroundItsJson()
    {
        var runner = new ScriptedRunner(Envelope("```json\n{\"tumor\":\"glioma\",\"count\":22}\n```"));

        var result = await Cli(runner).RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("glioma", result.Value!.Tumor);
    }

    [Fact]
    public async Task RetriesOnceOnMalformedOutputThenSucceeds()
    {
        var runner = new ScriptedRunner(
            Envelope("this is not json"),
            Envelope("""{"tumor":"meningioma","count":10}"""));

        var result = await Cli(runner).RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("meningioma", result.Value!.Tumor);
        Assert.Equal(2, runner.Calls);
    }

    [Fact]
    public async Task GivesUpAfterTheRetryAndNeverReturnsAValue()
    {
        var runner = new ScriptedRunner(
            Envelope("not json"), Envelope("still not json"));

        var result = await Cli(runner).RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Null(result.Value);
        Assert.NotNull(result.FailureReason);
        Assert.Equal(2, runner.Calls);   // one try + one retry, no more
    }

    [Fact]
    public async Task ATimeoutIsAFailureNotAValue()
    {
        var runner = new ScriptedRunner(new ProcessResult(-1, "", "", TimedOut: true));

        var result = await Cli(runner).RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("timed out", result.FailureReason);
    }

    [Fact]
    public async Task ANonZeroExitIsAFailure()
    {
        var runner = new ScriptedRunner(new ProcessResult(1, "", "claude: not logged in", false));

        var result = await Cli(runner).RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("exited 1", result.FailureReason);
    }

    [Fact]
    public async Task AnIsErrorEnvelopeIsAFailure()
    {
        var runner = new ScriptedRunner(
            new ProcessResult(0, """{"type":"result","is_error":true,"result":"model refused"}""", "", false));

        var result = await Cli(runner).RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("is_error", result.FailureReason);
    }

    [Fact]
    public async Task OutputThatParsesButFailsValidationIsRejected()
    {
        // The classifier will validate e.g. "tag is a real taxonomy slug".
        // A structurally-valid but semantically-wrong response must not pass.
        var runner = new ScriptedRunner(
            Envelope("""{"tumor":"dragonoma","count":1}"""),
            Envelope("""{"tumor":"dragonoma","count":1}"""));

        var result = await Cli(runner).RunJsonAsync<Extraction>(
            "prompt", e => e.Tumor == "glioblastoma", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("validation", result.FailureReason);
    }

    // ---------- envelope-independent helpers ----------

    [Theory]
    [InlineData("{\"a\":1}", "{\"a\":1}")]
    [InlineData("```json\n{\"a\":1}\n```", "{\"a\":1}")]
    [InlineData("```\n{\"a\":1}\n```", "{\"a\":1}")]
    public void StripCodeFencesHandlesTheCommonShapes(string input, string expected)
    {
        Assert.Equal(expected, ClaudeCli.StripCodeFences(input));
    }

    [Fact]
    public async Task TheRealRunnerFailsSafeWhenTheCliIsNotInstalled()
    {
        // The most likely real misconfiguration: `claude` not on PATH. The
        // wrapper's whole contract is "never throw" — this must come back as a
        // failure the caller can flag, not an exception that crashes the run.
        var options = Microsoft.Extensions.Options.Options.Create(new ClaudeOptions
        {
            Executable = "definitely-not-a-real-binary-bh42",
            TimeoutSeconds = 10,
        });
        var cli = new ClaudeCli(new ClaudeProcessRunner(options), NullLogger<ClaudeCli>.Instance);

        var result = await cli.RunJsonAsync<Extraction>("prompt", null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.FailureReason);
    }
}
