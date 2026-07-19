namespace BrainHarbor.Web.Models;

/// <summary>
/// One feed item as the _FeedCard partial renders it — the fixed anatomy
/// from sitemap.md: badge → plain-language title → one-sentence hook →
/// tumor-type tags → date + source. M2's /research feed maps aggregated
/// items into this.
/// </summary>
public sealed record FeedCard(
    ResearchStage Stage,
    string Title,
    string Url,
    string Hook,
    IReadOnlyList<string> Tags,
    string DateText,
    string Source);
