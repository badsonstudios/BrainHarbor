using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BrainHarbor.Web.Trials;

public sealed record NearbyTrial(
    string NctId,
    string Title,
    string? Status,
    string? Phase,
    string? NearestSite);

/// <param name="TotalCount">
/// How many the registry says there are, which is NOT Trials.Count — we ask for
/// at most <see cref="NearbyTrialsClient.MaxResults"/>. Printing the page size
/// as if it were the answer would put a false number on a medical page.
/// </param>
public sealed record NearbyResult(
    IReadOnlyList<NearbyTrial> Trials, bool RegistryUnavailable, int TotalCount = 0)
{
    public static NearbyResult Unavailable => new([], true);

    public bool Truncated => TotalCount > Trials.Count;
}

/// <summary>
/// WI-403: "trials near me", asked of ClinicalTrials.gov LIVE at request time
/// (architecture.md §7) rather than served from the cache.
///
/// It is live because distance is the one thing the cache cannot answer well:
/// the registry's own `filter.geo` knows every site of every trial, including
/// trials our brain-tumor window never fetched. Keyless and public domain, so
/// there is no credential in the request path.
///
/// Being in the request path is also why it fails SOFT: a slow or broken
/// registry must degrade to "we could not reach ClinicalTrials.gov just now,
/// here is the browse list" rather than an error page. A reader looking for a
/// trial while frightened should never meet a stack trace.
/// </summary>
public sealed class NearbyTrialsClient(HttpClient httpClient, ILogger<NearbyTrialsClient> logger)
{
    /// <summary>The radius "near me" means. Wide enough to be useful for a
    /// treatment worth travelling for, narrow enough to stay meaningful.</summary>
    public const int RadiusMiles = 50;

    public const int MaxResults = 25;

    /// <summary>
    /// Builds the registry condition expression for a set of terms. Each term
    /// is QUOTED, because a taxonomy label like "DIPG (pontine)" carries
    /// parentheses that the registry's Essie syntax reads as live grouping
    /// operators, and a stray quote would reshape the whole query.
    /// </summary>
    internal static string ConditionExpression(IReadOnlyList<string> terms)
    {
        var quoted = terms
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => $"\"{t.Replace("\"", " ").Trim()}\"")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return quoted.Count == 0
            ? Registry.BrainTumorConditionQuery
            : $"({string.Join(" OR ", quoted)})";
    }

    public async Task<NearbyResult> FindAsync(
        double latitude, double longitude, IReadOnlyList<string>? conditionTerms,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string?>
        {
            // Same subtree and aliases the browse filter uses, so picking
            // "Glioma" cannot mean one thing in the list and another here.
            ["query.cond"] = conditionTerms is { Count: > 0 }
                ? ConditionExpression(conditionTerms)
                : Registry.BrainTumorConditionQuery,
            ["filter.geo"] =
                $"distance({latitude.ToString("0.####", CultureInfo.InvariantCulture)}," +
                $"{longitude.ToString("0.####", CultureInfo.InvariantCulture)},{RadiusMiles}mi)",
            ["filter.overallStatus"] = "NOT_YET_RECRUITING|RECRUITING|ENROLLING_BY_INVITATION|AVAILABLE",
            ["sort"] = "LastUpdatePostDate:desc",
            ["countTotal"] = "true",
            ["pageSize"] = MaxResults.ToString(CultureInfo.InvariantCulture),
            ["fields"] =
                "protocolSection.identificationModule.nctId," +
                "protocolSection.identificationModule.briefTitle," +
                "protocolSection.statusModule.overallStatus," +
                "protocolSection.designModule.phases," +
                "protocolSection.contactsLocationsModule.locations",
        };

        var query = string.Join("&", parameters
            .Where(p => p.Value is not null)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));

        try
        {
            using var response = await httpClient.GetAsync($"studies?{query}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ClinicalTrials.gov near-me query returned {Status}.",
                    (int)response.StatusCode);
                return NearbyResult.Unavailable;
            }

            var body = await response.Content.ReadFromJsonAsync<GeoResponse>(cancellationToken);
            var trials = (body?.Studies ?? [])
                .Select(s => ToNearbyTrial(s, latitude, longitude))
                .Where(t => t is not null)
                .ToList();

            return new NearbyResult(trials!, false, body?.TotalCount ?? trials.Count);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout throws TaskCanceledException, which IS an
            // OperationCanceledException — so this is the SLOW-REGISTRY case,
            // the exact one this client exists to absorb. Only a cancellation
            // the caller actually asked for is allowed to propagate.
            logger.LogWarning("ClinicalTrials.gov did not answer within the timeout.");
            return NearbyResult.Unavailable;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Soft failure by design — see the class comment.
            logger.LogWarning(exception, "Could not reach ClinicalTrials.gov for a near-me query.");
            return NearbyResult.Unavailable;
        }
    }

    private static NearbyTrial? ToNearbyTrial(GeoStudy study, double lat, double lon)
    {
        var section = study.ProtocolSection;
        var nctId = section?.IdentificationModule?.NctId?.Trim();
        var title = section?.IdentificationModule?.BriefTitle;

        if (string.IsNullOrWhiteSpace(nctId) || string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        // The site actually closest to the reader — the registry returns every
        // site of a matching trial, including ones on the other side of the
        // country, and naming a far one would be worse than naming none.
        var nearest = (section?.ContactsLocationsModule?.Locations ?? [])
            .Where(l => l.GeoPoint?.Lat is not null && l.GeoPoint.Lon is not null)
            .OrderBy(l => DistanceMiles(lat, lon, l.GeoPoint!.Lat!.Value, l.GeoPoint.Lon!.Value))
            .FirstOrDefault();

        var where = nearest is null
            ? null
            : string.Join(", ", new[] { nearest.City, nearest.State }
                .Where(p => !string.IsNullOrWhiteSpace(p)));

        return new NearbyTrial(
            nctId,
            title,
            Registry.PlainStatus(section?.StatusModule?.OverallStatus),
            Registry.PlainPhase(section?.DesignModule?.Phases),
            string.IsNullOrWhiteSpace(where) ? nearest?.Facility : where);
    }

    /// <summary>Great-circle distance, only ever used to rank a trial's own
    /// sites against each other.</summary>
    internal static double DistanceMiles(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMiles = 3958.8;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusMiles * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private sealed class GeoResponse
    {
        [JsonPropertyName("totalCount")]
        public int? TotalCount { get; set; }

        [JsonPropertyName("studies")]
        public List<GeoStudy> Studies { get; set; } = [];
    }

    private sealed class GeoStudy
    {
        [JsonPropertyName("protocolSection")]
        public GeoProtocolSection? ProtocolSection { get; set; }
    }

    private sealed class GeoProtocolSection
    {
        [JsonPropertyName("identificationModule")]
        public GeoIdentification? IdentificationModule { get; set; }

        [JsonPropertyName("statusModule")]
        public GeoStatus? StatusModule { get; set; }

        [JsonPropertyName("designModule")]
        public GeoDesign? DesignModule { get; set; }

        [JsonPropertyName("contactsLocationsModule")]
        public GeoContactsLocations? ContactsLocationsModule { get; set; }
    }

    private sealed class GeoIdentification
    {
        [JsonPropertyName("nctId")] public string? NctId { get; set; }
        [JsonPropertyName("briefTitle")] public string? BriefTitle { get; set; }
    }

    private sealed class GeoStatus
    {
        [JsonPropertyName("overallStatus")] public string? OverallStatus { get; set; }
    }

    private sealed class GeoDesign
    {
        [JsonPropertyName("phases")] public List<string> Phases { get; set; } = [];
    }

    private sealed class GeoContactsLocations
    {
        [JsonPropertyName("locations")] public List<GeoLocation> Locations { get; set; } = [];
    }

    private sealed class GeoLocation
    {
        [JsonPropertyName("facility")] public string? Facility { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("geoPoint")] public GeoPoint? GeoPoint { get; set; }
    }

    private sealed class GeoPoint
    {
        [JsonPropertyName("lat")] public double? Lat { get; set; }
        [JsonPropertyName("lon")] public double? Lon { get; set; }
    }
}
