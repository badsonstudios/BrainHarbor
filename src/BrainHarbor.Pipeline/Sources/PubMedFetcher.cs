using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;
using BrainHarbor.Pipeline.Publishing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrainHarbor.Pipeline.Sources;

/// <summary>
/// WI-204: PubMed via NCBI E-utilities — the research backbone (PLAN.md §5).
///
/// esearch (JSON) finds PMIDs in a date window, efetch (XML — PubMed has no
/// JSON for it) pulls titles and abstracts. The cursor is the last date
/// window fetched, so a missed run self-heals: the next run simply asks for a
/// wider window rather than losing days.
///
/// Licensing: abstracts can carry publisher rights, so raw text is stored
/// only as pipeline input for summarization — the site links and summarizes
/// rather than republishing (PLAN.md §5).
/// </summary>
public sealed class PubMedFetcher(
    HttpClient httpClient,
    IOptions<PipelineOptions> options,
    ILogger<PubMedFetcher> logger,
    TimeProvider? timeProvider = null) : ISourceFetcher
{
    public const string SourceName = "pubmed";

    /// <summary>First run looks back this far rather than all of history.</summary>
    public const int FirstRunLookbackDays = 30;

    /// <summary>Cap on a catch-up window — a year-old cursor shouldn't pull everything.</summary>
    public const int MaxLookbackDays = 120;

    /// <summary>Results per esearch page.</summary>
    public const int PageSize = 200;

    /// <summary>
    /// Hard ceiling on one run. If a window genuinely holds more than this,
    /// the cursor is NOT advanced past the unfetched remainder — the next run
    /// picks them up rather than losing them forever.
    /// </summary>
    public const int MaxResultsPerRun = 2000;

    /// <summary>
    /// The brain-tumor query. MeSH terms catch indexed articles; the title/
    /// abstract terms catch recent ones MeSH hasn't reached yet. English only:
    /// we cannot summarize what we cannot read.
    /// </summary>
    public const string Query =
        "((\"Brain Neoplasms\"[MeSH] OR \"Central Nervous System Neoplasms\"[MeSH] OR " +
        "\"Glioma\"[MeSH] OR \"Glioblastoma\"[MeSH] OR \"Meningioma\"[MeSH] OR " +
        "\"Medulloblastoma\"[MeSH] OR \"Ependymoma\"[MeSH] OR " +
        "\"Pituitary Neoplasms\"[MeSH] OR \"Neuroma, Acoustic\"[MeSH] OR " +
        "\"Craniopharyngioma\"[MeSH] OR \"Spinal Cord Neoplasms\"[MeSH] OR " +
        "\"Meningeal Carcinomatosis\"[MeSH] OR " +
        "glioma[Title/Abstract] OR glioblastoma[Title/Abstract] OR " +
        "\"brain tumor\"[Title/Abstract] OR \"brain tumour\"[Title/Abstract] OR " +
        "\"brain cancer\"[Title/Abstract] OR " +
        "meningioma[Title/Abstract] OR medulloblastoma[Title/Abstract] OR " +
        "ependymoma[Title/Abstract] OR craniopharyngioma[Title/Abstract] OR " +
        "\"diffuse midline glioma\"[Title/Abstract] OR DIPG[Title/Abstract] OR " +
        "\"primary CNS lymphoma\"[Title/Abstract] OR " +
        "\"vestibular schwannoma\"[Title/Abstract] OR " +
        "\"brain metastases\"[Title/Abstract] OR \"brain metastasis\"[Title/Abstract] OR " +
        "leptomeningeal[Title/Abstract]) AND english[lang])";

    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    private DateTimeOffset _lastRequest = DateTimeOffset.MinValue;

    public string Source => SourceName;

    public async Task<FetchResult> FetchAsync(string? cursor, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.NcbiApiKey))
        {
            // Visible, not silent: without a key NCBI throttles to 3 rps.
            logger.LogWarning("[pubmed] no NCBI API key configured — running at the lower rate limit.");
        }

        var today = DateOnly.FromDateTime(_time.GetUtcNow().UtcDateTime);
        var lookbackDays = LookbackDaysFor(cursor, today);

        logger.LogInformation("[pubmed] searching the last {Days} day(s).", lookbackDays);

        var (pmids, totalAvailable) = await SearchAsync(lookbackDays, cancellationToken);
        if (pmids.Count == 0)
        {
            return new FetchResult([], today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        var items = await FetchDetailsAsync(pmids, cancellationToken);
        var kept = items.Where(i => !PubMedPreFilter.ShouldExclude(i.Title, i.RawSummary)).ToList();

        if (kept.Count < items.Count)
        {
            logger.LogInformation("[pubmed] pre-filter dropped {Dropped} of {Total} item(s).",
                items.Count - kept.Count, items.Count);
        }

        // If the window held more than we could take, do NOT advance the
        // cursor — the remainder would otherwise fall outside every future
        // window and be invisible forever.
        if (totalAvailable > pmids.Count)
        {
            logger.LogWarning(
                "[pubmed] window holds {Total} results, fetched {Fetched}. Holding the cursor so " +
                "the rest are picked up next run.", totalAvailable, pmids.Count);
            return new FetchResult(kept, null);
        }

        return new FetchResult(kept, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Days to look back. A cursor gives self-healing catch-up (missed three
    /// days → ask for four), with a +1 day overlap because PubMed indexes
    /// continuously and same-day items can appear after a run. Dedupe on the
    /// server makes the overlap free.
    /// </summary>
    internal static int LookbackDaysFor(string? cursor, DateOnly today)
    {
        if (!DateOnly.TryParse(cursor, CultureInfo.InvariantCulture, out var last))
        {
            return FirstRunLookbackDays;
        }

        var gap = today.DayNumber - last.DayNumber;
        return Math.Clamp(gap + 1, 1, MaxLookbackDays);
    }

    /// <summary>
    /// Pages through esearch. Returns the PMIDs fetched and the total the
    /// window actually holds, so the caller can tell whether it saw
    /// everything.
    /// </summary>
    private async Task<(IReadOnlyList<string> Pmids, int TotalAvailable)> SearchAsync(
        int lookbackDays, CancellationToken cancellationToken)
    {
        var pmids = new List<string>();
        var total = 0;

        for (var start = 0; start < MaxResultsPerRun; start += PageSize)
        {
            var url = BuildUrl("esearch.fcgi", new Dictionary<string, string?>
            {
                ["db"] = "pubmed",
                ["term"] = Query,
                ["reldate"] = lookbackDays.ToString(CultureInfo.InvariantCulture),
                ["datetype"] = "edat",     // entrez date — when PubMed got it
                ["retstart"] = start.ToString(CultureInfo.InvariantCulture),
                ["retmax"] = PageSize.ToString(CultureInfo.InvariantCulture),
                ["retmode"] = "json",
                ["sort"] = "pub_date",
            });

            await ThrottleAsync(cancellationToken);
            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var document = await response.Content
                .ReadFromJsonAsync<JsonDocument>(cancellationToken);

            if (document is null ||
                !document.RootElement.TryGetProperty("esearchresult", out var result))
            {
                // E-utilities answers 200 with an error payload. Throwing keeps
                // the cursor put; per-source isolation handles the rest.
                throw new InvalidOperationException("PubMed esearch returned no esearchresult.");
            }

            if (result.TryGetProperty("ERROR", out var error))
            {
                throw new InvalidOperationException($"PubMed esearch error: {error}");
            }

            if (start == 0 &&
                result.TryGetProperty("count", out var countElement) &&
                int.TryParse(countElement.GetString(), out var parsedCount))
            {
                total = parsedCount;
            }

            if (!result.TryGetProperty("idlist", out var idList))
            {
                throw new InvalidOperationException("PubMed esearch returned no idlist.");
            }

            var page = idList.EnumerateArray()
                .Select(e => e.GetString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            pmids.AddRange(page!);

            if (page.Count < PageSize || pmids.Count >= total)
            {
                break;
            }
        }

        return (pmids, Math.Max(total, pmids.Count));
    }

    private async Task<IReadOnlyList<FetchedItem>> FetchDetailsAsync(
        IReadOnlyList<string> pmids, CancellationToken cancellationToken)
    {
        var items = new List<FetchedItem>();

        // NCBI asks for ≤200 ids per efetch request.
        foreach (var chunk in pmids.Chunk(200))
        {
            var url = BuildUrl("efetch.fcgi", new Dictionary<string, string?>
            {
                ["db"] = "pubmed",
                ["id"] = string.Join(",", chunk),
                ["retmode"] = "xml",
            });

            await ThrottleAsync(cancellationToken);
            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var xml = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
            items.AddRange(ParseArticles(xml));
        }

        return items;
    }

    /// <summary>
    /// NCBI politeness (PLAN.md §5): 10 requests/second with an API key, 3
    /// without. Paging makes bursts real, so space requests conservatively.
    /// </summary>
    private async Task ThrottleAsync(CancellationToken cancellationToken)
    {
        var minimumSpacing = string.IsNullOrWhiteSpace(options.Value.NcbiApiKey)
            ? TimeSpan.FromMilliseconds(350)   // ~3 rps
            : TimeSpan.FromMilliseconds(110);  // ~9 rps

        var sinceLast = _time.GetUtcNow() - _lastRequest;
        if (sinceLast < minimumSpacing)
        {
            await Task.Delay(minimumSpacing - sinceLast, _time, cancellationToken);
        }

        _lastRequest = _time.GetUtcNow();
    }

    /// <summary>Parses an efetch PubmedArticleSet. Internal for tests against recorded XML.</summary>
    internal static IReadOnlyList<FetchedItem> ParseArticles(XDocument xml)
    {
        var items = new List<FetchedItem>();

        foreach (var article in xml.Descendants("PubmedArticle"))
        {
            // Scope to the citation: Descendants would also pick up the PMIDs
            // of every cited reference. Real XML carries stray whitespace.
            var citation = article.Element("MedlineCitation");
            var articleElement = citation?.Element("Article");
            var pmid = citation?.Element("PMID")?.Value.Trim();
            var title = articleElement?.Element("ArticleTitle")?.Value;

            if (string.IsNullOrWhiteSpace(pmid) || string.IsNullOrWhiteSpace(title))
            {
                continue;   // Without an id or a title there's nothing to show.
            }

            // Only the real Abstract: PubMed also attaches OtherAbstract
            // blocks (publisher copies, foreign-language translations) whose
            // AbstractText children would otherwise be concatenated in.
            // Sectioned abstracts keep their labels — they help the summarizer
            // tell what was studied from what was found.
            var abstractParts = (articleElement?.Element("Abstract")?.Elements("AbstractText") ?? [])
                .Select(node =>
                {
                    var label = node.Attribute("Label")?.Value;
                    var text = CollapseWhitespace(node.Value);
                    return string.IsNullOrWhiteSpace(label) ? text : $"{label}: {text}";
                })
                .Where(t => t.Length > 0);
            var summary = string.Join("\n\n", abstractParts);

            items.Add(new FetchedItem
            {
                Source = SourceName,
                SourceKind = "research",
                ExternalId = pmid,
                Title = CollapseWhitespace(title),
                Url = $"https://pubmed.ncbi.nlm.nih.gov/{pmid}/",
                RawSummary = summary.Length == 0 ? null : summary,
                PublishedAt = ParsePublishedDate(article),
            });
        }

        return items;
    }

    private static DateOnly? ParsePublishedDate(XElement article)
    {
        // ArticleDate (electronic publication) first: for ahead-of-print
        // records PubDate is often year-only or absent, and feed ordering
        // depends on getting the real date.
        var articleDate = article.Descendants("ArticleDate").FirstOrDefault();
        if (articleDate is not null &&
            int.TryParse(articleDate.Element("Year")?.Value, out var electronicYear) &&
            int.TryParse(articleDate.Element("Month")?.Value, out var electronicMonth) &&
            int.TryParse(articleDate.Element("Day")?.Value, out var electronicDay))
        {
            try
            {
                return new DateOnly(electronicYear, electronicMonth, electronicDay);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Fall through to PubDate.
            }
        }

        // PubDate can be a full date, year+month, year only, or a MedlineDate
        // string like "2026 Jun-Jul" — take what we can get.
        var pubDate = article.Descendants("PubDate").FirstOrDefault();
        if (pubDate is null)
        {
            return null;
        }

        var yearText = pubDate.Element("Year")?.Value
                       ?? pubDate.Element("MedlineDate")?.Value?.Split(' ').FirstOrDefault();
        if (!int.TryParse(yearText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) ||
            year is < 1900 or > 2200)
        {
            return null;
        }

        var month = ParseMonth(pubDate.Element("Month")?.Value);
        var day = int.TryParse(pubDate.Element("Day")?.Value, out var parsedDay) ? parsedDay : 1;

        try
        {
            return new DateOnly(year, month, Math.Clamp(day, 1, DateTime.DaysInMonth(year, month)));
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static int ParseMonth(string? month)
    {
        if (string.IsNullOrWhiteSpace(month))
        {
            return 1;
        }

        if (int.TryParse(month, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric)
            && numeric is >= 1 and <= 12)
        {
            return numeric;
        }

        return DateTime.TryParseExact(month[..Math.Min(3, month.Length)], "MMM",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.Month
            : 1;
    }

    private static string CollapseWhitespace(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private string BuildUrl(string endpoint, Dictionary<string, string?> parameters)
    {
        // Politeness (PLAN.md §5): identify the tool and include the API key,
        // which raises the rate limit from 3 to 10 requests/second. The key is
        // a query parameter because that is what E-utilities requires — which
        // is why HttpClient request-URI logging is turned down in Program.cs.
        parameters["tool"] = "BrainHarbor";
        parameters["email"] = options.Value.ContactEmail;

        var apiKey = options.Value.NcbiApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            parameters["api_key"] = apiKey;
        }

        var query = string.Join("&", parameters
            .Where(p => p.Value is not null)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));

        return $"{endpoint}?{query}";
    }
}
