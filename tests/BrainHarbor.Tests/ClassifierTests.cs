using BrainHarbor.Pipeline.Claude;
using BrainHarbor.Pipeline.Classify;
using BrainHarbor.Pipeline.Publishing;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-303: the classify step. The guarantees: the model can only emit real
/// taxonomy slugs, a preprint can never be classified patient_relevant, and a
/// classify failure leaves the item Unclassified (uploaded pending) rather
/// than dropped or guessed. Exercised with a scripted fake CLI + a stub
/// taxonomy — no process, no network.
/// </summary>
public class ClassifierTests
{
    private static readonly TaxonomyTypeDto[] Taxonomy =
    [
        new("glioblastoma", "Glioblastoma", ["GBM"]),
        new("meningioma", "Meningioma", []),
        new("all-brain-tumors", "All brain tumors", []),
    ];

    private sealed class ScriptedRunner(params ProcessResult[] results) : IProcessRunner
    {
        private int _call;
        public Task<ProcessResult> RunAsync(string prompt, CancellationToken ct) =>
            Task.FromResult(results[Math.Min(_call++, results.Length - 1)]);
    }

    private sealed class StubSync(IReadOnlyList<TaxonomyTypeDto>? taxonomy, bool throws = false) : ISyncApiClient
    {
        public Task<IReadOnlyList<TaxonomyTypeDto>> GetTaxonomyAsync(CancellationToken ct) =>
            throws ? throw new HttpRequestException("site down") : Task.FromResult(taxonomy!);
        public Task<IReadOnlyDictionary<string, SourceState>> GetStateAsync(CancellationToken ct) => throw new NotImplementedException();
        public Task<IReadOnlyList<ItemKey>> FindNewAsync(IReadOnlyList<ItemKey> keys, CancellationToken ct) => throw new NotImplementedException();
        public Task<UploadResponse> UploadAsync(IReadOnlyList<SyncItem> items, string? cursor, CancellationToken ct) => throw new NotImplementedException();
        public Task<TrialsResponse> UploadTrialsAsync(IReadOnlyList<TrialFacts> trials, CancellationToken ct) => throw new NotImplementedException();
        public Task AdvanceCursorAsync(string source, string cursor, CancellationToken ct) => throw new NotImplementedException();
        public Task ReportFailureAsync(string source, string error, CancellationToken ct) => throw new NotImplementedException();
    }

    private static ProcessResult Envelope(string modelJson) =>
        new(0, $$"""{"type":"result","is_error":false,"result":{{System.Text.Json.JsonSerializer.Serialize(modelJson)}}}""", "", false);

    private static Classifier Build(ScriptedRunner runner, IReadOnlyList<TaxonomyTypeDto>? taxonomy, bool taxonomyThrows = false)
    {
        var options = Options.Create(new ClaudeOptions
        {
            PromptsDirectory = Path.Combine(RepoRoot(), "src", "BrainHarbor.Pipeline", "Prompts"),
        });
        return new Classifier(
            new StubSync(taxonomy, taxonomyThrows),
            new ClaudeCli(runner, NullLogger<ClaudeCli>.Instance),
            new PromptLibrary(options),
            NullLogger<Classifier>.Instance);
    }

    private static FetchedItem Item(string sourceKind = "research", string? abstractText = "We studied 331 people with glioblastoma.") => new()
    {
        Source = sourceKind == "preprint" ? "medrxiv" : "pubmed",
        SourceKind = sourceKind,
        ExternalId = "x1",
        Title = "A glioblastoma study",
        Url = "https://example.org",
        RawSummary = abstractText,
    };

    [Fact]
    public async Task ClassifiesAValidResponse()
    {
        var runner = new ScriptedRunner(Envelope(
            """{"tumor_tags":["glioblastoma"],"relevance":"patient_relevant","research_stage":"human_trial"}"""));

        var c = await Build(runner, Taxonomy).ClassifyAsync(Item(), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Classified, c.Decision);
        Assert.Equal(["glioblastoma"], c.TumorTags);
        Assert.Equal("patient_relevant", c.Relevance);
        Assert.Equal("classify-v1", c.PromptVersion);
    }

    [Fact]
    public async Task ExcludedResponseMeansDoNotUpload()
    {
        var runner = new ScriptedRunner(Envelope(
            """{"tumor_tags":[],"relevance":"excluded","research_stage":"news_other"}"""));

        var c = await Build(runner, Taxonomy).ClassifyAsync(Item(), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Exclude, c.Decision);
    }

    [Fact]
    public async Task AnInventedSlugIsRejectedAndFallsToUnclassified()
    {
        // Same invented slug both tries → validation fails twice → unclassified.
        var bad = Envelope("""{"tumor_tags":["dragonoma"],"relevance":"patient_relevant","research_stage":"human_trial"}""");
        var runner = new ScriptedRunner(bad, bad);

        var c = await Build(runner, Taxonomy).ClassifyAsync(Item(), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Unclassified, c.Decision);
    }

    [Fact]
    public async Task APreprintMarkedPatientRelevantIsCappedToEarlyStageNotDiscarded()
    {
        // content-pipeline.md §9: preprints are "early_stage at best". Cap the
        // relevance rather than throw away an otherwise-good classification.
        var runner = new ScriptedRunner(Envelope(
            """{"tumor_tags":["glioblastoma"],"relevance":"patient_relevant","research_stage":"observational"}"""));

        var c = await Build(runner, Taxonomy).ClassifyAsync(Item(sourceKind: "preprint"), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Classified, c.Decision);
        Assert.Equal("early_stage", c.Relevance);
    }

    [Fact]
    public async Task TheRealModelIdIsCapturedFromTheEnvelope()
    {
        // classify_model auditability — a silent model switch must be traceable.
        var runner = new ScriptedRunner(new ProcessResult(0,
            """{"type":"result","is_error":false,"model":"claude-opus-4-8","result":"{\"tumor_tags\":[\"glioblastoma\"],\"relevance\":\"patient_relevant\",\"research_stage\":\"human_trial\"}"}""",
            "", false));

        var c = await Build(runner, Taxonomy).ClassifyAsync(Item(), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Classified, c.Decision);
        Assert.Equal("claude-opus-4-8", c.Model);
    }

    [Fact]
    public async Task APreprintCanBeEarlyStage()
    {
        var runner = new ScriptedRunner(Envelope(
            """{"tumor_tags":["glioblastoma"],"relevance":"early_stage","research_stage":"preclinical_animal"}"""));

        var c = await Build(runner, Taxonomy).ClassifyAsync(Item(sourceKind: "preprint"), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Classified, c.Decision);
        Assert.Equal("early_stage", c.Relevance);
    }

    [Fact]
    public async Task ARecoverableGarbleRetriesThenSucceeds()
    {
        var runner = new ScriptedRunner(
            Envelope("not json at all"),
            Envelope("""{"tumor_tags":["meningioma"],"relevance":"patient_relevant","research_stage":"observational"}"""));

        var c = await Build(runner, Taxonomy).ClassifyAsync(Item(), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Classified, c.Decision);
        Assert.Equal(["meningioma"], c.TumorTags);
    }

    /// <summary>
    /// WI-413: the taxonomy comes from the SITE, so failing to fetch it is the
    /// site being unreachable — infrastructure, identical for every item. It
    /// used to leave each item merely Unclassified, which meant a whole run's
    /// worth of perfectly good items uploaded as unclassifiable and could never
    /// be classified again.
    /// </summary>
    [Fact]
    public async Task IfTheTaxonomyCannotBeFetchedTheClassifierIsUnavailableNotJustStuck()
    {
        var runner = new ScriptedRunner(Envelope("""{"tumor_tags":["glioblastoma"],"relevance":"patient_relevant","research_stage":"human_trial"}"""));

        var c = await Build(runner, taxonomy: null, taxonomyThrows: true).ClassifyAsync(Item(), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Unavailable, c.Decision);
    }

    /// <summary>
    /// The other side of the WI-413 split: the CLI answered, this item's output
    /// was unusable. That is about the item, so it goes to a person and the run
    /// carries on.
    /// </summary>
    [Fact]
    public async Task ADeadCliIsUnavailableWhileAnOddItemIsMerelyUnclassified()
    {
        var dead = new ScriptedRunner(new ProcessResult(1, "", "claude: usage limit reached", false));
        var odd = Envelope("""{"tumor_tags":["dragonoma"],"relevance":"patient_relevant","research_stage":"human_trial"}""");

        var outage = await Build(dead, Taxonomy).ClassifyAsync(Item(), CancellationToken.None);
        var strange = await Build(new ScriptedRunner(odd, odd), Taxonomy)
            .ClassifyAsync(Item(), CancellationToken.None);

        Assert.Equal(ClassifyDecision.Unavailable, outage.Decision);
        Assert.Equal(ClassifyDecision.Unclassified, strange.Decision);
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
