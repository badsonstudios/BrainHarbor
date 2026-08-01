using System.Globalization;

namespace BrainHarbor.Web.Trials;

/// <summary>
/// WI-403: turns a ZIP code a reader types into a point on the map, so "trials
/// near me" works without asking the browser for location permission.
///
/// Data is the U.S. Census Bureau's ZCTA gazetteer (public domain), shipped as
/// a file: it is read-only reference data that changes once a year, so a
/// database table would be ceremony without benefit.
///
/// **Privacy (PLAN.md §9, /privacy):** a ZIP is only ever turned into a
/// latitude and longitude for the outgoing ClinicalTrials.gov query. It is
/// never stored, never logged, and never leaves the request.
///
/// A ZIP centroid is a rough point — a large rural ZCTA can be miles across.
/// That is fine for "trials within 50 miles" and is the reason the page says
/// "near" rather than showing a precise distance.
/// </summary>
public sealed class ZctaCentroids
{
    private readonly Dictionary<string, (double Lat, double Lon)> _byZip;

    public ZctaCentroids(string csv)
    {
        _byZip = new Dictionary<string, (double, double)>(35_000, StringComparer.Ordinal);

        foreach (var line in csv.Split('\n'))
        {
            var row = line.AsSpan().Trim();
            if (row.IsEmpty || row[0] == '#')
            {
                continue;
            }

            var parts = row.ToString().Split(',');
            if (parts.Length != 3)
            {
                continue;
            }

            if (double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
            {
                _byZip[parts[0]] = (lat, lon);
            }
        }
    }

    public int Count => _byZip.Count;

    /// <summary>
    /// Looks up a ZIP as a reader would type it: extra spaces, and the ZIP+4
    /// form ("43210-1234") both work, because rejecting those would be a
    /// pointless dead end for someone copying an address.
    /// </summary>
    public (double Lat, double Lon)? Find(string? zip)
    {
        var normalized = Normalize(zip);
        return normalized is not null && _byZip.TryGetValue(normalized, out var point)
            ? point
            : null;
    }

    internal static string? Normalize(string? zip)
    {
        if (string.IsNullOrWhiteSpace(zip))
        {
            return null;
        }

        var trimmed = zip.Trim();

        // ZIP+4 — take the five-digit part.
        var dash = trimmed.IndexOf('-');
        if (dash > 0)
        {
            trimmed = trimmed[..dash];
        }

        trimmed = trimmed.Trim();
        return trimmed.Length == 5 && trimmed.All(char.IsAsciiDigit) ? trimmed : null;
    }

    public static ZctaCentroids Load(string contentRoot)
    {
        var path = Path.Combine(contentRoot, "zcta-centroids.csv");
        return new ZctaCentroids(File.Exists(path) ? File.ReadAllText(path) : "");
    }
}
