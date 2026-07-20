using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline.Sources;

/// <summary>
/// How much of a feed's text we are allowed to keep (PLAN.md §5). This is a
/// licensing rule, not a preference, so it lives in the type system rather
/// than in a comment: each fetcher must state its policy and the parser
/// enforces it.
/// </summary>
public enum FeedTextPolicy
{
    /// <summary>
    /// Never a valid policy — the default value must not be the permissive
    /// one. A feed whose licence hasn't been checked fails closed.
    /// </summary>
    Unspecified = 0,

    /// <summary>Public-domain source (NCI): the full description may be kept.</summary>
    FullTextAllowed,

    /// <summary>
    /// Headline + short summary + link ONLY (ScienceDaily). The description is
    /// truncated to a teaser; we summarize and link rather than republish.
    /// </summary>
    HeadlineAndTeaserOnly,
}

/// <summary>
/// WI-205: shared RSS/Atom fetcher. One instance per feed, configured with
/// its own source name, URL, and licensing policy.
///
/// The cursor is the newest publication date seen, so a missed run simply
/// re-reads a slightly older window; dedupe on the server makes the overlap
/// free.
/// </summary>
public sealed partial class RssFetcher(
    HttpClient httpClient,
    ILogger<RssFetcher> logger,
    RssFeedDefinition definition) : ISourceFetcher
{
    /// <summary>Teaser cap for licence-restricted feeds.</summary>
    public const int TeaserLength = 300;

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTag();

    public string Source => definition.Source;

    public async Task<FetchResult> FetchAsync(string? cursor, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(definition.FeedUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var xml = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var parsed = ParseFeed(xml, definition);

        // A feed that yields nothing is more likely reshaped than quiet —
        // cancer.gov has retired syndication URLs before, and a silent zero
        // looks identical to a healthy slow day.
        if (parsed.Count == 0)
        {
            logger.LogWarning(
                "[{Source}] the feed document contained no items — it may have moved or changed shape ({Url}).",
                definition.Source, definition.FeedUrl);
        }

        var items = parsed;

        // Only keep what's newer than the cursor; the server dedupes anyway,
        // but this keeps the batch small on a normal day.
        if (DateOnly.TryParse(cursor, CultureInfo.InvariantCulture, out var since))
        {
            items = [.. items.Where(i => i.PublishedAt is null || i.PublishedAt >= since)];
        }

        // Relevance is judged on the FULL description, then the licence
        // decides what we keep. Filtering on the truncated teaser would drop
        // e.g. a breast-cancer item whose brain-metastases mention falls past
        // the cut — the exact false-negative the pre-filter exists to avoid.
        var kept = items
            .Where(i => !BrainTumorPreFilter.ShouldExclude(i.Title, i.FullDescriptionForFiltering))
            .Select(i => i.Item with { RawSummary = ApplyTextPolicy(i.Item.RawSummary, definition.TextPolicy) })
            .ToList();

        if (kept.Count < items.Count)
        {
            logger.LogInformation("[{Source}] pre-filter dropped {Dropped} of {Total} item(s).",
                definition.Source, items.Count - kept.Count, items.Count);
        }

        var newest = kept.Select(i => i.PublishedAt).Where(d => d is not null).Max();
        return new FetchResult(kept,
            newest?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// A parsed entry, carrying the FULL description separately from the item.
    /// Relevance filtering reads the full text; only what gets persisted is
    /// subject to the licence.
    /// </summary>
    internal sealed record ParsedFeedItem(FetchedItem Item, string? FullDescriptionForFiltering)
    {
        public string Title => Item.Title;
        public DateOnly? PublishedAt => Item.PublishedAt;
    }

    /// <summary>Parses RSS 2.0 or Atom. Internal so tests can use recorded feeds.</summary>
    internal static IReadOnlyList<ParsedFeedItem> ParseFeed(XDocument xml, RssFeedDefinition definition)
    {
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var items = new List<ParsedFeedItem>();

        var entries = xml.Descendants("item")
            .Concat(xml.Descendants(atom + "entry"))
            .ToList();

        foreach (var entry in entries)
        {
            var title = Clean(Value(entry, "title", atom));

            // RSS puts the URL in the element text; Atom puts it in a link
            // element's href and leaves the text empty — so an empty value
            // must fall through, not win.
            var link = NullIfBlank(Value(entry, "link", atom))
                       ?? entry.Elements(atom + "link")
                           .Concat(entry.Elements("link"))
                           .Select(e => e.Attribute("href")?.Value)
                           .FirstOrDefault(href => !string.IsNullOrWhiteSpace(href));

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            {
                continue;
            }

            // guid/id is the stable dedupe key; the link is the fallback.
            var externalId = NullIfBlank(Value(entry, "guid", atom))
                             ?? NullIfBlank(entry.Element(atom + "id")?.Value)
                             ?? link;

            var description = Clean(
                Value(entry, "description", atom)
                ?? entry.Element(atom + "summary")?.Value
                ?? entry.Element(atom + "content")?.Value);

            items.Add(new ParsedFeedItem(
                new FetchedItem
                {
                    Source = definition.Source,
                    SourceKind = definition.SourceKind,
                    ExternalId = externalId.Trim(),
                    Title = title,
                    Url = link.Trim(),
                    // Full text here; FetchAsync applies the licence policy
                    // after relevance has been judged.
                    RawSummary = description,
                    PublishedAt = ParseDate(
                        Value(entry, "pubDate", atom)
                        ?? entry.Element(atom + "published")?.Value
                        ?? entry.Element(atom + "updated")?.Value
                        ?? Value(entry, "date", atom)),
                },
                description));
        }

        return items;
    }

    /// <summary>
    /// Enforces the per-source licensing rule in code (PLAN.md §5). For a
    /// headline-only feed we keep a short teaser as summarizer input and
    /// nothing more — the site links rather than republishing.
    /// </summary>
    internal static string? ApplyTextPolicy(string? description, FeedTextPolicy policy)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        if (policy == FeedTextPolicy.Unspecified)
        {
            throw new InvalidOperationException(
                "A feed must declare its text policy — an unchecked licence fails closed.");
        }

        if (policy == FeedTextPolicy.FullTextAllowed)
        {
            return description;
        }

        if (description.Length <= TeaserLength)
        {
            return description;
        }

        // Cut on a word boundary so the teaser reads as a sentence fragment,
        // not a truncated word.
        var cut = description.LastIndexOf(' ', TeaserLength);
        return string.Concat(description.AsSpan(0, cut > 0 ? cut : TeaserLength), "…");
    }

    private static string? Value(XElement entry, string name, XNamespace atom) =>
        NullIfBlank(entry.Element(name)?.Value) ?? NullIfBlank(entry.Element(atom + name)?.Value);

    private static string? NullIfBlank(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text;

    private static string? Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // Feed descriptions are usually escaped HTML.
        var decoded = System.Net.WebUtility.HtmlDecode(text);
        var stripped = HtmlTag().Replace(decoded, " ");
        return string.Join(' ', stripped.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// RFC 822 permits named US timezones ("EDT", "PST") and ScienceDaily
    /// uses them, but .NET only parses numeric offsets plus GMT/UT — so every
    /// one of their items came back undated, which pinned them to the bottom
    /// of the feed and stopped the cursor advancing (found in the WI-211
    /// shakedown). Substituting the offset before parsing fixes it.
    /// </summary>
    private static readonly Dictionary<string, string> NamedZoneOffsets = new(StringComparer.Ordinal)
    {
        ["EST"] = "-0500", ["EDT"] = "-0400",
        ["CST"] = "-0600", ["CDT"] = "-0500",
        ["MST"] = "-0700", ["MDT"] = "-0600",
        ["PST"] = "-0800", ["PDT"] = "-0700",
    };

    internal static DateOnly? ParseDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Trim();
        foreach (var (zone, offset) in NamedZoneOffsets)
        {
            if (normalized.EndsWith(zone, StringComparison.Ordinal))
            {
                normalized = string.Concat(normalized.AsSpan(0, normalized.Length - zone.Length), offset);
                break;
            }
        }

        // RFC 822 (RSS) and ISO 8601 (Atom) both appear in the wild.
        if (DateTimeOffset.TryParse(normalized, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            return DateOnly.FromDateTime(parsed.UtcDateTime);
        }

        return null;
    }
}

/// <summary>One configured feed.</summary>
public sealed record RssFeedDefinition(
    string Source,
    string FeedUrl,
    string SourceKind,
    FeedTextPolicy TextPolicy)
{
    /// <summary>
    /// NCI news releases — public domain, full text may be kept (PLAN.md §5).
    /// NOTE: cancer.gov has retired syndication URLs before; if this 404s the
    /// source fails every run, so the health page (WI-210) must surface it.
    /// </summary>
    public static RssFeedDefinition Nci { get; } = new(
        "nci_rss",
        "https://www.cancer.gov/publishedcontent/rss/syndication/rss/ncinewsreleases.rss",
        "news",
        FeedTextPolicy.FullTextAllowed);

    /// <summary>
    /// ScienceDaily brain-tumor feed — headline + summary + link ONLY. Not a
    /// style choice: republishing their text is not licensed.
    /// </summary>
    public static RssFeedDefinition ScienceDaily { get; } = new(
        "sciencedaily",
        "https://www.sciencedaily.com/rss/health_medicine/brain_tumor.xml",
        "news",
        FeedTextPolicy.HeadlineAndTeaserOnly);
}
