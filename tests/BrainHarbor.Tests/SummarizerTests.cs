using BrainHarbor.Pipeline.Claude;
using BrainHarbor.Pipeline.Sources;
using BrainHarbor.Pipeline.Summarize;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-304: the summarize step. The load-bearing behaviour: a summary with all
/// six blocks that passes the automated checks comes back un-flagged; a
/// summary with a hallucinated number (or hype, or too-hard prose) comes back
/// FLAGGED (held for a human in Auto mode); and a failed call yields no
/// summary at all — never a guess. Scripted fake CLI, no process/network.
/// </summary>
public class SummarizerTests
{
    private sealed class ScriptedRunner(params ProcessResult[] results) : IProcessRunner
    {
        private int _call;
        public Task<ProcessResult> RunAsync(string prompt, CancellationToken ct) =>
            Task.FromResult(results[Math.Min(_call++, results.Length - 1)]);
    }

    private static ProcessResult Envelope(string modelJson) =>
        new(0, $$"""{"type":"result","is_error":false,"model":"claude-opus-5","result":{{System.Text.Json.JsonSerializer.Serialize(modelJson)}}}""", "", false);

    private static Summarizer Build(ScriptedRunner runner) =>
        new(new ClaudeCli(runner, NullLogger<ClaudeCli>.Instance),
            new PromptLibrary(Options.Create(new ClaudeOptions
            {
                PromptsDirectory = Path.Combine(RepoRoot(), "src", "BrainHarbor.Pipeline", "Prompts"),
            })),
            NullLogger<Summarizer>.Instance);

    private static FetchedItem Item() => new()
    {
        Source = "pubmed",
        SourceKind = "research",
        ExternalId = "s1",
        Title = "A trial of a pill for glioma",
        Url = "https://example.org",
        RawSummary = "In a trial of 331 people with glioma, survival was 27 months versus 11 months.",
    };

    private static string FullSummary(string whatFound, int readiness = 6) =>
        $$"""
        {"plain_title":"A pill slowed glioma growth","hook":"A daily pill helped people go longer before their tumor grew.",
         "what_studied":"Researchers gave a daily pill to 331 people with glioma.",
         "what_found":{{System.Text.Json.JsonSerializer.Serialize(whatFound)}},
         "means":"For some people, the pill may add time before stronger care is needed.",
         "doesnt_mean":"This is not a promise for everyone, and it does not get rid of the tumor.",
         "readiness_score":{{readiness}},
         "readiness_reason":"Being tested in people in trials, but not yet approved."}
        """;

    [Fact]
    public async Task ACleanSummaryComesBackUnflagged()
    {
        var runner = new ScriptedRunner(Envelope(FullSummary("People went 27 months before the tumor grew, versus 11 months.")));

        var result = await Build(runner).SummarizeAsync(Item(), CancellationToken.None);

        Assert.NotNull(result.Output);
        Assert.False(result.Flagged);
        Assert.Empty(result.FlagReasons);
        Assert.Equal("claude-opus-5", result.Model);
        // Readiness comes through as the model's raw proposal; the pipeline
        // clamps it against research stage at upload (PipelineRunner).
        Assert.Equal(6, result.Output.ReadinessScore);
        Assert.False(string.IsNullOrWhiteSpace(result.Output.ReadinessReason));
    }

    [Fact]
    public async Task AMissingReadinessScoreIsInvalidSoTheCallFailsToNoSummary()
    {
        // readiness_score omitted (deserializes to 0, out of the 1-10 range) →
        // AllBlocksPresent is false → retry → no summary rather than a "0/10".
        var noReadiness = Envelope(
            """
            {"plain_title":"x","hook":"y","what_studied":"a","what_found":"b",
             "means":"c","doesnt_mean":"d","readiness_reason":"e"}
            """);
        var runner = new ScriptedRunner(noReadiness, noReadiness);

        var result = await Build(runner).SummarizeAsync(Item(), CancellationToken.None);

        Assert.Null(result.Output);
    }

    [Fact]
    public async Task AHallucinatedNumberFlagsTheSummaryButKeepsIt()
    {
        // 88% is nowhere in the source. Flag it (held for a human) — but the
        // summary is still returned so a reviewer can see + fix it.
        var runner = new ScriptedRunner(Envelope(FullSummary("The pill worked for 88% of people.")));

        var result = await Build(runner).SummarizeAsync(Item(), CancellationToken.None);

        Assert.NotNull(result.Output);
        Assert.True(result.Flagged);
        Assert.Contains(result.FlagReasons, r => r.Message.Contains("88"));
        // WI-417: the run tally counts by kind, so the kind has to survive the
        // trip out of the summarizer.
        Assert.Contains(BrainHarbor.Safety.Guardrails.FlagKind.InventedNumbers,
            result.FlagReasons.Select(r => r.Kind));
    }

    [Fact]
    public async Task AHypeWordFlagsTheSummary()
    {
        var runner = new ScriptedRunner(Envelope(FullSummary("This breakthrough helped people live 27 months.")));

        var result = await Build(runner).SummarizeAsync(Item(), CancellationToken.None);

        Assert.True(result.Flagged);
        Assert.Contains(result.FlagReasons, r => r.Message.Contains("breakthrough"));
        Assert.Contains(BrainHarbor.Safety.Guardrails.FlagKind.BannedHype,
            result.FlagReasons.Select(r => r.Kind));
    }

    [Fact]
    public async Task AMissingBlockIsInvalidSoTheCallFailsToNoSummary()
    {
        // Missing "doesnt_mean" (the mandatory anti-hype block) → invalid →
        // retry → same → no summary rather than a partial one.
        var partial = Envelope("""{"plain_title":"x","hook":"y","what_studied":"a","what_found":"b","means":"c","doesnt_mean":""}""");
        var runner = new ScriptedRunner(partial, partial);

        var result = await Build(runner).SummarizeAsync(Item(), CancellationToken.None);

        Assert.Null(result.Output);
        Assert.False(result.Flagged);
    }

    [Fact]
    public async Task ACliFailureYieldsNoSummary()
    {
        var runner = new ScriptedRunner(new ProcessResult(1, "", "not logged in", false));

        var result = await Build(runner).SummarizeAsync(Item(), CancellationToken.None);

        Assert.Null(result.Output);
    }

    /// <summary>
    /// WI-413: a dead CLI is not the same as a summary that came back wrong.
    /// The runner stops the source on the first, and uploads the item for a
    /// person on the second — an item uploaded classified-but-unsummarized is
    /// never summarized again, so a half-processed item must not go up.
    /// </summary>
    [Fact]
    public async Task ADeadCliIsUnavailableWhileAnUnusableSummaryIsNot()
    {
        var dead = new ScriptedRunner(new ProcessResult(1, "", "claude: usage limit reached", false));
        var garbled = Envelope("not json at all");

        var outage = await Build(dead).SummarizeAsync(Item(), CancellationToken.None);
        var unusable = await Build(new ScriptedRunner(garbled, garbled))
            .SummarizeAsync(Item(), CancellationToken.None);

        Assert.Null(outage.Output);
        Assert.True(outage.Unavailable);

        Assert.Null(unusable.Output);
        Assert.False(unusable.Unavailable);
    }

    [Fact]
    public async Task EmDashesAreStrippedFromEveryBlockBeforeUpload()
    {
        // Dan's rule: published prose must not read as machine-written. Even if
        // the model slips an em dash in, the reader never sees one.
        var runner = new ScriptedRunner(Envelope(FullSummary(
            "People went 27 months — versus 11 months — before the tumor grew.")));

        var result = await Build(runner).SummarizeAsync(Item(), CancellationToken.None);

        Assert.NotNull(result.Output);
        Assert.DoesNotContain('—', result.Output!.AllProse);
        Assert.DoesNotContain('–', result.Output.AllProse);
    }

    [Theory]
    [InlineData("It helped — a lot.", "It helped, a lot.")]
    [InlineData("Survival was 10–20 months.", "Survival was 10 to 20 months.")]
    [InlineData("The pill — taken daily — worked.", "The pill, taken daily, worked.")]
    [InlineData("No dashes here.", "No dashes here.")]
    public void NormalizeRemovesDashesWithoutManglingText(string input, string expected)
    {
        Assert.Equal(expected, ProseStyle.Normalize(input));
    }

    [Fact]
    public void ATrialGetsTheTrialPromptAndEverythingElseGetsTheResearchOne()
    {
        // WI-402. The research template asks "what did they find", and an open
        // trial has found nothing yet — asking that of a trial description
        // invites the model to invent an outcome, which is exactly the failure
        // the guardrails exist to prevent.
        var trial = Item() with { Source = "ctgov", SourceKind = "trial_update" };

        Assert.Equal("summarize-trial", Summarizer.PromptNameFor(trial));
        Assert.Equal("summarize", Summarizer.PromptNameFor(Item()));
        Assert.Equal("summarize", Summarizer.PromptNameFor(Item() with { SourceKind = "preprint" }));
    }

    [Fact]
    public void TheTrialPromptExistsAndForbidsInventingAnOutcome()
    {
        var path = Path.Combine(RepoRoot(), "src", "BrainHarbor.Pipeline", "Prompts", "summarize-trial.md");
        var text = File.ReadAllText(path);

        Assert.StartsWith("version: summarize-trial-v", text, StringComparison.Ordinal);
        Assert.Contains("has NOT produced results yet", text, StringComparison.Ordinal);
        Assert.Contains("Never promise access", text, StringComparison.Ordinal);

        // The same anti-hype floor as the research prompt.
        foreach (var banned in new[] { "breakthrough", "miracle", "game-changer", "wonder drug" })
        {
            Assert.Contains(banned, text, StringComparison.Ordinal);
        }
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
