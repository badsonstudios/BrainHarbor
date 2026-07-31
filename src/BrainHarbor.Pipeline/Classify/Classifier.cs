using BrainHarbor.Pipeline.Claude;
using BrainHarbor.Pipeline.Publishing;
using BrainHarbor.Pipeline.Sources;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Classify;

/// <summary>What the classifier model returns (content-pipeline.md §9).</summary>
public sealed record ClassifyOutput
{
    public IReadOnlyList<string> TumorTags { get; init; } = [];
    public string Relevance { get; init; } = "";
    public string ResearchStage { get; init; } = "";
}

/// <summary>
/// The outcome of classifying one item: a classification, "exclude this item"
/// (don't upload), or "couldn't classify" (upload as pending so a human sorts
/// it — never dropped silently).
/// </summary>
public enum ClassifyDecision { Classified, Exclude, Unclassified }

public sealed record Classification(
    ClassifyDecision Decision,
    IReadOnlyList<string> TumorTags,
    string? Relevance,
    string? ResearchStage,
    string PromptVersion,
    string? Model = null);

public interface IItemClassifier
{
    Task<Classification> ClassifyAsync(FetchedItem item, CancellationToken cancellationToken);
}

/// <summary>
/// WI-303: sorts a fetched item for a patient audience with the local Claude
/// CLI. The closed taxonomy is fetched from the site (one source of truth,
/// loaded once per run). The model's output is validated against it; anything
/// off — invented slug, bad enum, a preprint marked patient_relevant — is
/// rejected, and a hard failure leaves the item Unclassified (uploaded
/// pending) rather than guessed.
/// </summary>
public sealed class Classifier(
    ISyncApiClient sync,
    ClaudeCli claude,
    PromptLibrary prompts,
    ILogger<Classifier> logger) : IItemClassifier
{
    private static readonly string[] ValidRelevance = ["patient_relevant", "early_stage", "excluded"];
    private static readonly string[] ValidStages =
        ["human_trial", "observational", "review_guideline", "preclinical_animal", "preclinical_cell", "news_other"];

    private readonly SemaphoreSlim _taxonomyGate = new(1, 1);
    private IReadOnlyList<TaxonomyTypeDto>? _taxonomy;
    private bool _taxonomyFailed;
    private HashSet<string> _slugs = [];

    public async Task<Classification> ClassifyAsync(FetchedItem item, CancellationToken cancellationToken)
    {
        var taxonomy = await EnsureTaxonomyAsync(cancellationToken);
        if (taxonomy is null)
        {
            // Can't classify without the closed taxonomy — leave every item
            // for a human rather than guess or drop.
            return Unclassified(item);
        }

        var template = prompts.Get("classify");
        var prompt = template.Render(new Dictionary<string, string>
        {
            ["taxonomy"] = string.Join("\n", taxonomy.Select(t =>
                $"- {t.Slug}: {t.Label}" + (t.Aliases.Count > 0 ? $" (also: {string.Join(", ", t.Aliases)})" : ""))),
            ["source_kind"] = item.SourceKind,
            ["title"] = item.Title,
            ["abstract"] = item.RawSummary ?? "(no abstract available)",
        });

        var result = await claude.RunJsonAsync<ClassifyOutput>(
            prompt, output => IsValid(output, item.SourceKind), cancellationToken);

        if (!result.Success)
        {
            logger.LogWarning("[{Source}/{Id}] classification failed ({Reason}) — leaving it for a human.",
                item.Source, item.ExternalId, result.FailureReason);
            return Unclassified(item);
        }

        var output = result.Value!;
        if (output.Relevance == "excluded")
        {
            return new Classification(ClassifyDecision.Exclude, [], "excluded", output.ResearchStage,
                template.Version, result.Model);
        }

        // Cap, don't discard: a preprint can never be patient_relevant
        // (content-pipeline.md §9, "early_stage at best"). Downgrade rather
        // than throw away an otherwise-good classification.
        var relevance = output.Relevance;
        if (item.SourceKind == "preprint" && relevance == "patient_relevant")
        {
            logger.LogInformation("[{Source}/{Id}] preprint capped from patient_relevant to early_stage.",
                item.Source, item.ExternalId);
            relevance = "early_stage";
        }

        var tags = output.TumorTags.Where(_slugs.Contains).Distinct().ToList();
        return new Classification(
            ClassifyDecision.Classified, tags, relevance, output.ResearchStage, template.Version, result.Model);
    }

    private Classification Unclassified(FetchedItem item) =>
        new(ClassifyDecision.Unclassified, [], null, null, "classify-unavailable");

    private async Task<IReadOnlyList<TaxonomyTypeDto>?> EnsureTaxonomyAsync(CancellationToken cancellationToken)
    {
        if (_taxonomy is not null)
        {
            return _taxonomy;
        }

        await _taxonomyGate.WaitAsync(cancellationToken);
        try
        {
            // Fail fast: if the first fetch failed, don't re-hit a down site
            // once per item for the rest of the run.
            if (_taxonomyFailed)
            {
                return null;
            }

            if (_taxonomy is null)
            {
                var fetched = await sync.GetTaxonomyAsync(cancellationToken);
                _slugs = fetched.Select(t => t.Slug).ToHashSet(StringComparer.Ordinal);
                _taxonomy = fetched;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _taxonomyFailed = true;
            logger.LogError(exception, "Could not fetch the taxonomy — items this run stay unclassified.");
            return null;
        }
        finally
        {
            _taxonomyGate.Release();
        }

        return _taxonomy;
    }

    internal bool IsValid(ClassifyOutput output, string sourceKind)
    {
        if (!ValidRelevance.Contains(output.Relevance) || !ValidStages.Contains(output.ResearchStage))
        {
            return false;
        }

        // Note: a preprint marked patient_relevant is NOT rejected here — it's
        // capped to early_stage after validation (see ClassifyAsync), so a good
        // classification isn't thrown away. The server + DB still hard-enforce
        // the rule as a backstop.

        if (output.TumorTags.Any(t => !_slugs.Contains(t)))
        {
            return false;
        }

        return output.Relevance == "excluded"
            ? output.TumorTags.Count == 0
            : output.TumorTags.Count > 0;
    }
}
