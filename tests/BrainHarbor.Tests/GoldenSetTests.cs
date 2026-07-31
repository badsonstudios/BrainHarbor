using System.Text.Json;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-301: the golden set is the quality yardstick for M3. These tests don't
/// judge summary *quality* (that's Dan's ratification + the WI-304 checks) —
/// they guarantee the fixture stays internally valid: real taxonomy slugs,
/// documented vocab, and complete ideal summaries. A malformed yardstick is
/// worse than none.
/// </summary>
public class GoldenSetTests
{
    private static readonly string[] ValidRelevance = ["patient_relevant", "early_stage", "excluded"];
    private static readonly string[] ValidStages =
        ["human_trial", "observational", "review_guideline", "preclinical_animal", "preclinical_cell", "news_other"];

    public sealed record GoldenSet(GoldenItem[] Items);
    public sealed record GoldenItem(GoldenInput Input, GoldenExpected Expected, string? Note, GoldenSummary? IdealSummary);
    public sealed record GoldenInput(string Source, string SourceKind, string ExternalId, string Title, string RawSummary, string? PublishedAt);
    public sealed record GoldenExpected(string[] TumorTags, string Relevance, string ResearchStage);
    public sealed record GoldenSummary(string PlainTitle, string WhatStudied, string WhatFound, string Means, string DoesntMean, string StageLabel, int Readiness);

    private static readonly GoldenSet Golden = Load();
    private static readonly TaxonomyStore Taxonomy = new(File.ReadAllText(
        Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml")));

    private static GoldenSet Load()
    {
        var path = Path.Combine(RepoRoot(), "tests", "BrainHarbor.Tests", "GoldenSet", "golden-set.json");
        return JsonSerializer.Deserialize<GoldenSet>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower })!;
    }

    [Fact]
    public void TheGoldenSetLoadsAndHasABalancedSpread()
    {
        Assert.True(Golden.Items.Length >= 15, "want a meaningful yardstick, not a token one");

        // Every relevance tier represented — the pipeline must be tested on
        // the items it should hide and exclude, not just the easy front-page ones.
        foreach (var tier in ValidRelevance)
        {
            Assert.True(Golden.Items.Any(i => i.Expected.Relevance == tier),
                $"golden set has no '{tier}' case");
        }
    }

    [Fact]
    public void EveryExpectedClassificationUsesTheDocumentedVocabulary()
    {
        foreach (var item in Golden.Items)
        {
            Assert.Contains(item.Expected.Relevance, ValidRelevance);
            Assert.Contains(item.Expected.ResearchStage, ValidStages);
        }
    }

    [Fact]
    public void EveryTumorTagIsARealTaxonomySlug()
    {
        // The closed-taxonomy promise: if the yardstick itself named an
        // invented tumor type, it would bless the classifier doing the same.
        foreach (var item in Golden.Items)
        {
            foreach (var tag in item.Expected.TumorTags)
            {
                Assert.True(Taxonomy.IsKnownSlug(tag),
                    $"{item.Input.ExternalId}: '{tag}' is not a taxonomy slug");
            }
        }
    }

    [Fact]
    public void ExcludedItemsCarryNoTumorTags()
    {
        foreach (var item in Golden.Items.Where(i => i.Expected.Relevance == "excluded"))
        {
            Assert.Empty(item.Expected.TumorTags);
        }
    }

    [Fact]
    public void PatientRelevantAndEarlyStageItemsAreTaggedOrExplained()
    {
        foreach (var item in Golden.Items.Where(i => i.Expected.Relevance != "excluded"))
        {
            Assert.True(item.Expected.TumorTags.Length > 0,
                $"{item.Input.ExternalId}: a shown item needs at least one tumor tag");
        }
    }

    [Fact]
    public void BorderlineCasesAreDocumented()
    {
        // A golden set earns its keep on the hard cases; each must say why.
        foreach (var item in Golden.Items)
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Note),
                $"{item.Input.ExternalId}: every case needs a one-line rationale");
        }
    }

    [Fact]
    public void EnoughIdealSummariesExistAndAllSixBlocksArePresent()
    {
        var withSummaries = Golden.Items.Where(i => i.IdealSummary is not null).ToList();
        Assert.True(withSummaries.Count >= 10, "acceptance: ideal summaries for ~10 items");

        foreach (var item in withSummaries)
        {
            var s = item.IdealSummary!;
            foreach (var (block, text) in new[]
                     {
                         ("plain_title", s.PlainTitle), ("what_studied", s.WhatStudied),
                         ("what_found", s.WhatFound), ("means", s.Means),
                         ("doesnt_mean", s.DoesntMean), ("stage_label", s.StageLabel),
                     })
            {
                Assert.False(string.IsNullOrWhiteSpace(text),
                    $"{item.Input.ExternalId}: ideal summary block '{block}' is empty");
            }
        }
    }

    [Fact]
    public void OnlyItemsShownToReadersHaveIdealSummaries()
    {
        // An excluded item should never carry a model summary — it never renders.
        foreach (var item in Golden.Items.Where(i => i.IdealSummary is not null))
        {
            Assert.NotEqual("excluded", item.Expected.Relevance);
        }
    }

    [Fact]
    public void EveryIdealSummaryPassesTheWI304Guardrails()
    {
        // The 301↔304 tie: an "ideal" summary must itself pass the automated
        // safety checks (numbers trace to source, no hype, reading level).
        // If the yardstick fails its own guardrails, the guardrails or the
        // golden set is wrong.
        foreach (var item in Golden.Items.Where(i => i.IdealSummary is not null))
        {
            var s = item.IdealSummary!;
            var prose = string.Join("\n", s.PlainTitle, s.WhatStudied, s.WhatFound, s.Means, s.DoesntMean);
            var source = $"{item.Input.Title}\n{item.Input.RawSummary}";

            var result = BrainHarbor.Pipeline.Summarize.Guardrails.Check(prose, source);

            Assert.True(result.Passed,
                $"{item.Input.ExternalId} ideal summary tripped a guardrail: {string.Join("; ", result.Reasons)}");
        }
    }

    [Fact]
    public void EveryIdealReadinessScoreRespectsItsStageCeiling()
    {
        // The readiness↔stage tie: the yardstick's own scores must obey the
        // same stage cap the pipeline enforces (Readiness.Clamp). If a golden
        // "ideal" score sat above its stage ceiling, it would bless the model
        // over-promising — a mouse study reading as near-clinic.
        foreach (var item in Golden.Items.Where(i => i.IdealSummary is not null))
        {
            var score = item.IdealSummary!.Readiness;
            var ceiling = BrainHarbor.Pipeline.Summarize.Readiness.CeilingFor(item.Expected.ResearchStage);

            Assert.InRange(score, BrainHarbor.Pipeline.Summarize.Readiness.Min,
                BrainHarbor.Pipeline.Summarize.Readiness.Max);
            Assert.True(score <= ceiling,
                $"{item.Input.ExternalId}: readiness {score} exceeds the ceiling {ceiling} " +
                $"for stage '{item.Expected.ResearchStage}'");
        }
    }

    [Fact]
    public void InputsAreRealPubmedItemsWithAbstracts()
    {
        foreach (var item in Golden.Items)
        {
            Assert.Equal("pubmed", item.Input.Source);
            Assert.False(string.IsNullOrWhiteSpace(item.Input.RawSummary),
                $"{item.Input.ExternalId}: golden inputs must carry the real abstract");
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
