using BrainHarbor.Web.Content;
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

    public async Task OnGetAsync(
        string? tumorType, string? phase, bool includeClosed = false, int page = 0,
        string? zip = null, double? lat = null, double? lon = null,
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

        var query = new TrialQuery(TumorType, Phase, IncludeClosed, page);

        // Browse always runs, so a failed or empty near-me search still leaves
        // the reader with something to read rather than an empty page.
        Results = await trials.BrowseAsync(query, cancellationToken);

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
    public string FilterUrl(string? tumorType = null, string? phase = null, bool? includeClosed = null)
    {
        var parts = new List<string>();

        var tumor = tumorType ?? TumorType;
        if (!string.IsNullOrWhiteSpace(tumor)) parts.Add($"tumorType={Uri.EscapeDataString(tumor)}");

        var chosenPhase = phase ?? Phase;
        if (!string.IsNullOrWhiteSpace(chosenPhase)) parts.Add($"phase={Uri.EscapeDataString(chosenPhase)}");

        if (includeClosed ?? IncludeClosed) parts.Add("includeClosed=true");
        if (Zip is not null) parts.Add($"zip={Zip}");

        return parts.Count == 0 ? "/trials" : $"/trials?{string.Join("&", parts)}";
    }

    public string PageUrl(int page)
    {
        var url = FilterUrl();
        return page == 0 ? url : $"{url}{(url.Contains('?') ? "&" : "?")}page={page}";
    }
}
