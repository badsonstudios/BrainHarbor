using BrainHarbor.Web.Content;
using BrainHarbor.Web.Models;
using BrainHarbor.Web.Trials;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Trials;

/// <summary>
/// WI-403: the trial finder.
///
/// Two ways in, on one page. **Browse** reads the local cache and is always
/// available. **Near me** asks ClinicalTrials.gov live, from either the
/// browser's location (offered, never required) or a ZIP the reader types.
///
/// The ZIP form is the primary path, not the fallback: geolocation needs
/// JavaScript and a permission prompt, and this audience should not have to
/// grant either to find a trial. The geolocation button is progressive
/// enhancement on top.
/// </summary>
public class IndexModel(
    TrialsRepository trials,
    NearbyTrialsClient nearby,
    ZctaCentroids centroids,
    TaxonomyStore taxonomy) : PageModel
{
    public TrialPage Results { get; private set; } = null!;
    public IReadOnlyList<string> Phases { get; private set; } = [];

    public string? TumorType { get; private set; }
    public string? Phase { get; private set; }
    public bool IncludeClosed { get; private set; }

    /// <summary>
    /// The chosen countries, empty for every country (WI-455, multi in WI-457).
    /// Held as a set because the view asks "is this one ticked?" once per
    /// country, and there are around forty of them.
    /// </summary>
    public IReadOnlySet<string> SelectedCountries { get; private set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Countries that actually have trial sites, with counts, built from the
    /// cache rather than a fixed world list — so the menu can never offer a
    /// choice that returns nothing.
    /// </summary>
    public IReadOnlyList<TrialCountry> Countries { get; private set; } = [];

    // Near-me state
    public string? Zip { get; private set; }
    public NearbyResult? Nearby { get; private set; }
    public bool ZipNotFound { get; private set; }

    /// <summary>The types worth offering as a filter — the cross-cutting slugs
    /// would return nothing (see TrialsRepository.NonHistologySlugs).</summary>
    public IReadOnlyList<TumorType> TumorTypes =>
        [.. taxonomy.TumorTypes.Where(t => !TrialsRepository.NonHistologySlugs.Contains(t.Slug))];

    public string LabelFor(string slug) => taxonomy.LabelFor(slug);

    public int RadiusMiles => NearbyTrialsClient.RadiusMiles;

    /// <summary>The 1-based page the reader is on, for the pager (WI-438).</summary>
    public Pagination Pages { get; private set; } = new(1, 1);

    /// <param name="pageNumber">
    /// Bound explicitly from the query and deliberately NOT named `page` —
    /// `page` is a reserved route-value key in Razor Pages, so a parameter of
    /// that name silently binds the route value instead and always yields the
    /// default. This page paged as badly as /research did until WI-438; see the
    /// long note on Research/IndexModel.OnGetAsync.
    /// </param>
    public async Task OnGetAsync(
        string? tumorType, string? phase, bool includeClosed = false,
        [FromQuery(Name = "page")] int pageNumber = 1,
        string? zip = null, double? lat = null, double? lon = null,
        // Repeated in the query string — ?country=China&country=Japan. Kept as
        // the same key a single choice used, so links shared before WI-457
        // still work.
        [FromQuery(Name = "country")] string[]? country = null,
        CancellationToken cancellationToken = default)
    {
        TumorType = taxonomy.Resolve(tumorType ?? "");

        // "All brain tumors" is a taxonomy entry, not something the registry
        // writes in a condition field — as a filter it would match nothing.
        if (TumorType is not null && TrialsRepository.NonHistologySlugs.Contains(TumorType))
        {
            TumorType = null;
        }

        Phases = await trials.AvailablePhasesAsync(cancellationToken);
        Phase = TrialsRepository.NormalizePhase(phase, Phases);
        IncludeClosed = includeClosed;

        // Validated against the countries the cache actually holds, the same way
        // the phase is (WI-455). A hand-typed ?country=Wakanda is dropped rather
        // than emptying the list, and the canonical spelling from the cache is
        // kept so the checkbox it belongs to renders ticked whatever casing the
        // URL used.
        // Counts reflect the tumor type and stage as well as includeClosed
        // (WI-462), so the number beside a country is what picking it will
        // actually return. Built from the same filters as the list.
        var countingQuery = new TrialQuery(TumorType, Phase, includeClosed);
        Countries = await trials.AvailableCountriesAsync(countingQuery, cancellationToken);
        SelectedCountries = new HashSet<string>(
            Countries
                .Where(known => country is not null && country.Any(asked =>
                    string.Equals(known.Name, asked?.Trim(), StringComparison.OrdinalIgnoreCase)))
                .Select(known => known.Name),
            StringComparer.OrdinalIgnoreCase);

        var query = new TrialQuery(
            TumorType, Phase, IncludeClosed, Math.Max(0, pageNumber - 1),
            [.. SelectedCountries]);

        // Browse always runs, so a failed or empty near-me search still leaves
        // the reader with something to read rather than an empty page.
        Results = await trials.BrowseAsync(query, cancellationToken);
        Pages = Pagination.For(Results.TotalCount, TrialQuery.PageSize, pageNumber);

        // A stale ?page=99 lands on the last real page rather than an empty list.
        if (Pages.CurrentPage - 1 != query.Page)
        {
            Results = await trials.BrowseAsync(
                query with { Page = Pages.CurrentPage - 1 }, cancellationToken);
        }

        if (zip is not null || lat is not null || lon is not null)
        {
            // This response is about where one reader is. It must never sit in a
            // shared cache, and the URL carrying their ZIP or coordinates must
            // not travel out in a Referer header to anywhere they click.
            Response.Headers.CacheControl = "private, no-store";
            Response.Headers["Referrer-Policy"] = "no-referrer";
        }

        await SearchNearbyAsync(zip, lat, lon, cancellationToken);
    }

    /// <summary>Set when a lat/lon was supplied but is not a real point —
    /// silently doing nothing would leave the reader staring at no result and
    /// no explanation.</summary>
    public bool LocationUnusable { get; private set; }

    private async Task SearchNearbyAsync(
        string? zip, double? lat, double? lon, CancellationToken cancellationToken)
    {
        // The same subtree and aliases the browse filter uses, so picking a
        // tumor type cannot quietly mean one thing in the list and another in
        // the near-me results.
        var terms = TumorType is null ? null : trials.ConditionTermsFor(TumorType);

        if (lat is not null || lon is not null)
        {
            // Coordinates from the browser win — more precise than a ZIP
            // centroid, and deliberately granted by the reader.
            if (lat is { } latitude && lon is { } longitude &&
                !double.IsNaN(latitude) && !double.IsNaN(longitude) &&
                latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180)
            {
                Nearby = await nearby.FindAsync(latitude, longitude, terms, cancellationToken);
            }
            else
            {
                LocationUnusable = true;
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(zip))
        {
            return;
        }

        // Echo back only the normalized digits, never the raw input — a ZIP box
        // is still a text box on a public page.
        Zip = ZctaCentroids.Normalize(zip);

        var point = centroids.Find(zip);
        if (point is null)
        {
            ZipNotFound = true;
            return;
        }

        Nearby = await nearby.FindAsync(point.Value.Lat, point.Value.Lon, terms, cancellationToken);
    }

    /// <summary>Rebuilds the querystring for a filter link, keeping whatever
    /// else the reader had chosen.</summary>
    public string FilterUrl(
        string? tumorType = null, string? phase = null, bool? includeClosed = null)
    {
        var parts = new List<string>();

        var tumor = tumorType ?? TumorType;
        if (!string.IsNullOrWhiteSpace(tumor)) parts.Add($"tumorType={Uri.EscapeDataString(tumor)}");

        var chosenPhase = phase ?? Phase;
        if (!string.IsNullOrWhiteSpace(chosenPhase)) parts.Add($"phase={Uri.EscapeDataString(chosenPhase)}");

        // One key repeated per country, which is what a checkbox group posts
        // and what the handler binds (WI-457). Ordered so the same selection
        // always produces the same URL — otherwise a shared link and a
        // bookmarked one for the same filter would differ by set ordering.
        foreach (var country in SelectedCountries.OrderBy(c => c, StringComparer.Ordinal))
        {
            parts.Add($"country={Uri.EscapeDataString(country)}");
        }

        if (includeClosed ?? IncludeClosed) parts.Add("includeClosed=true");
        if (Zip is not null) parts.Add($"zip={Zip}");

        return parts.Count == 0 ? "/trials" : $"/trials?{string.Join("&", parts)}";
    }

    /// <summary>
    /// The current filters with every country dropped — the "clear countries"
    /// link (WI-457). Unticking forty boxes by hand is not a reasonable ask,
    /// and a plain link keeps it working with JavaScript off.
    /// </summary>
    public string FilterUrlWithoutCountries()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(TumorType)) parts.Add($"tumorType={Uri.EscapeDataString(TumorType)}");
        if (!string.IsNullOrWhiteSpace(Phase)) parts.Add($"phase={Uri.EscapeDataString(Phase)}");
        if (IncludeClosed) parts.Add("includeClosed=true");
        if (Zip is not null) parts.Add($"zip={Zip}");

        return parts.Count == 0 ? "/trials" : $"/trials?{string.Join("&", parts)}";
    }

    /// <summary>
    /// The URL for a 1-based page, keeping the current filters. Page 1 is left
    /// bare so the canonical /trials URL has no redundant ?page=1 on it.
    ///
    /// **This DOES carry the ZIP**, because it is built from
    /// <see cref="FilterUrl"/>, which appends one when the reader has searched
    /// near them. An earlier version of this comment (WI-438) claimed the
    /// opposite; it was wrong, and a false comment about a privacy property is
    /// worse than none. The ZIP has to ride along or paging the browse list
    /// throws away the near-me panel above it — and the handler already marks
    /// those responses `private, no-store` with `Referrer-Policy: no-referrer`
    /// precisely because the URL carries a location.
    /// What that does NOT protect against is a reader sharing the URL itself.
    /// Worth deciding deliberately rather than by accident; not changed here.
    /// </summary>
    public string PageUrl(int pageNumber)
    {
        var url = FilterUrl();
        return pageNumber <= 1
            ? url
            : $"{url}{(url.Contains('?') ? "&" : "?")}page={pageNumber}";
    }
}
