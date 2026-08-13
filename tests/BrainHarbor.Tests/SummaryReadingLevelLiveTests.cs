using System.Text.Json;
using BrainHarbor.Pipeline.Claude;
using BrainHarbor.Pipeline.Sources;
using BrainHarbor.Pipeline.Summarize;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-415: does the summarize prompt actually produce 6th-grade prose?
///
/// A prompt change is a claim about model behaviour, and the only honest way
/// to check it is to run the real CLI over the golden set and measure what
/// comes back. That costs a slice of the Claude subscription, so this is an
/// opt-in category rather than part of the normal suite or CI:
///
///   BRAINHARBOR_LIVE_TESTS=1 dotnet test --filter "Category=Live" ///     -l "console;verbosity=detailed"
///
/// Two guards, because a [Trait] excludes nothing on its own: CI filters
/// Category!=Live, and the body no-ops unless BRAINHARBOR_LIVE_TESTS is set,
/// so an unfiltered local `dotnet test` stays free. Detailed verbosity is
/// required — xUnit hides output for passing tests otherwise.
///
/// It reports the distribution rather than asserting a hard threshold — the
/// number that matters (where to set Guardrails.MaxGradeLevel) is a product
/// decision about publish rate, not something a test should quietly pin.
/// </summary>
[Trait("Category", "Live")]
public class SummaryReadingLevelLiveTests(ITestOutputHelper output)
{
    /// <summary>Small on purpose: enough to see the distribution move, cheap
    /// enough to re-run after every prompt edit.</summary>
    private const int SampleSize = 8;

    [Theory]
    [InlineData("research")]
    [InlineData("trial_update")]   // summarize-trial has its own prompt + version
    public async Task GoldenSetSummariesReportTheirReadingGrade(string kind)
    {
        if (Environment.GetEnvironmentVariable("BRAINHARBOR_LIVE_TESTS") is not ("1" or "true"))
        {
            output.WriteLine("skipped — set BRAINHARBOR_LIVE_TESTS=1 to spend real model calls");
            return;
        }

        var golden = LoadGolden();
        var cases = golden
            .Where(i => (kind == "trial_update"
                            ? i.Input.SourceKind == "trial_update"
                            : i.Input.SourceKind != "trial_update")
                        && !string.IsNullOrWhiteSpace(i.Input.RawSummary))
            .Take(SampleSize)
            .ToList();

        Assert.NotEmpty(cases);

        var options = Options.Create(new ClaudeOptions());
        var prompts = new PromptLibrary(options);
        var claude = new ClaudeCli(new ClaudeProcessRunner(options), NullLogger<ClaudeCli>.Instance);
        var summarizer = new Summarizer(claude, prompts, NullLogger<Summarizer>.Instance);

        var grades = new List<double>();
        foreach (var item in cases)
        {
            var fetched = new FetchedItem
            {
                Source = item.Input.Source,
                SourceKind = item.Input.SourceKind,
                ExternalId = item.Input.ExternalId,
                Title = item.Input.Title,
                Url = $"https://example.org/{item.Input.ExternalId}",
                RawSummary = item.Input.RawSummary,
                // The trial prompt renders {{phase}}/{{status}} from these, so
                // a trial case without them would throw rather than measure.
                Trial = item.Input.SourceKind == "trial_update"
                    ? new BrainHarbor.Pipeline.Publishing.TrialFacts
                    {
                        NctId = item.Input.ExternalId,
                        Title = item.Input.Title,
                        Phase = item.Input.TrialPhase,
                        OverallStatus = item.Input.TrialStatus,
                    }
                    : null,
            };

            var result = await summarizer.SummarizeAsync(fetched, CancellationToken.None);
            if (result.Output is not { } summary)
            {
                output.WriteLine($"{item.Input.ExternalId}: no summary (model call failed)");
                continue;
            }

            var grade = Guardrails.GradeLevel(summary.AllProse);
            grades.Add(grade);
            output.WriteLine(
                $"{item.Input.ExternalId}: grade {grade:0.0}" +
                (result.Flagged ? $"  FLAGGED: {string.Join("; ", result.FlagReasons)}" : ""));
        }

        Assert.NotEmpty(grades);
        grades.Sort();
        output.WriteLine("");
        output.WriteLine($"kind           : {kind}");
        output.WriteLine($"prompt version : {prompts.Get(kind == "trial_update" ? "summarize-trial" : "summarize").Version}");
        output.WriteLine($"n              : {grades.Count}");
        output.WriteLine($"median grade   : {grades[grades.Count / 2]:0.0}");
        output.WriteLine($"mean grade     : {grades.Average():0.0}");
        output.WriteLine($"max grade      : {grades[^1]:0.0}");
        output.WriteLine($"at or under 6.0: {grades.Count(g => g <= 6.0)} of {grades.Count}");
    }

    private static List<GoldenSetTests.GoldenItem> LoadGolden()
    {
        var path = Path.Combine(RepoRoot(), "tests", "BrainHarbor.Tests", "GoldenSet", "golden-set.json");
        var set = JsonSerializer.Deserialize<GoldenSetTests.GoldenSet>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower })!;
        return [.. set.Items];
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
