using BrainHarbor.Web.Models;

namespace BrainHarbor.Web.Feed;

/// <summary>
/// Picks a card-backdrop photo that fits a post's content, from a small,
/// human-vetted pool under wwwroot/img/cards (grouped by theme: brain,
/// genetics, lab, data, abstract). The theme is derived from what we already
/// know about the item — its title/hook keywords and research stage — so the
/// image relates to the finding without any AI image generation. Selection is
/// deterministic per item (stable across renders), and everything falls back to
/// the brain pool, so a card always gets a safe, vetted image.
/// </summary>
public sealed class CardImages
{
    private static readonly string[] Themes = ["brain", "genetics", "lab", "data", "abstract"];
    private readonly IReadOnlyDictionary<string, string[]> _pools;

    public CardImages(IWebHostEnvironment environment)
        : this(Path.Combine(environment.WebRootPath, "img", "cards"))
    {
    }

    /// <summary>Directory-based ctor (also the test seam).</summary>
    public CardImages(string cardsDirectory)
    {
        var dir = cardsDirectory;
        _pools = Themes.ToDictionary(
            theme => theme,
            theme => Directory.Exists(dir)
                ? Directory.GetFiles(dir, $"{theme}-*.jpg")
                    .Select(f => "/img/cards/" + Path.GetFileName(f))
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray()
                : []);
    }

    /// <summary>The backdrop URL for a card, or null if no images are installed.</summary>
    public string? UrlFor(FeedCard card)
    {
        var pool = PoolFor(ThemeFor(card));
        return pool.Length == 0 ? null : pool[(int)(Fnv(card.Url) % (uint)pool.Length)];
    }

    private string[] PoolFor(string theme)
    {
        if (_pools.TryGetValue(theme, out var p) && p.Length > 0) return p;
        if (_pools.TryGetValue("brain", out var b) && b.Length > 0) return b;
        return _pools.TryGetValue("abstract", out var a) ? a : [];
    }

    /// <summary>
    /// Maps a post to an image theme from its own words + research stage.
    /// Precedence: a molecular/genetic angle wins, then lab/preclinical, then
    /// population-data studies, else a plain brain image (every item is a brain
    /// tumour item, so brain is the safe default; news gets an abstract field).
    /// </summary>
    public static string ThemeFor(FeedCard card)
    {
        var text = (card.Title + " " + card.Hook).ToLowerInvariant();

        if (HasAny(text, "idh", "mutation", "mutant", "gene", "genetic", "genomic",
                "molecular", "dna", "methylation", "egfr", "braf", "1p/19q"))
            return "genetics";

        if (card.Stage is ResearchStage.EarlyResearchAnimals or ResearchStage.EarlyResearchLabCells
            || HasAny(text, "mouse", "mice", "in vitro", "preclinical", "organoid",
                "immunotherap", "car-t", "car t", "vaccine", "antibody", "xenograft", "cell line"))
            return "lab";

        if (HasAny(text, "database", "registry", "records", "cohort", "seer", "population",
                "nationwide", "retrospective", "epidemiolog", "claims"))
            return "data";

        return card.Stage == ResearchStage.News ? "abstract" : "brain";
    }

    private static bool HasAny(string text, params string[] needles)
    {
        foreach (var n in needles)
        {
            if (text.Contains(n, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static uint Fnv(string s)
    {
        uint h = 2166136261;
        foreach (var c in s)
        {
            h ^= c;
            h *= 16777619;
        }
        return h;
    }
}
