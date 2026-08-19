using System.Globalization;
using System.Text;
using System.Xml.Linq;
using BrainHarbor.Web.Feed;

namespace BrainHarbor.Web.Seo;

/// <summary>
/// WI-308: the machine-readable surface — robots.txt, sitemap.xml, and an RSS
/// feed of published items. These make the site discoverable and let a shared
/// item link unfurl and be indexed as health content. Only status='published'
/// items appear (the human gate holds everywhere), and every value is written
/// through XDocument/normal string building so titles can't break the markup.
/// </summary>
public static class SyndicationEndpoints
{
    // Live static pages worth indexing (dead/placeholder routes excluded).
    // /tumors is the index; crawlers reach each individual type page by
    // following its links, which is why the index has to be listed here — a
    // page nothing links to and nothing lists is invisible to a search engine
    // as well as to a reader (WI-412 shipped orphaned; found 2026-08-16).
    // /start was reachable from the home page but absent here, so search
    // engines never saw the page a newly diagnosed person is most likely to be
    // searching for. Same orphan as /tumors, pointed the other way.
    private static readonly string[] StaticPaths =
        ["/", "/start", "/research", "/tumors", "/trials", "/search", "/get-help-now", "/about", "/how-we-write", "/glossary", "/privacy", "/terms"];

    private const int SitemapLimit = 5000;
    private const int FeedLimit = 50;

    public static void MapSyndication(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/robots.txt", (HttpRequest request) =>
        {
            var baseUrl = BaseUrl(request);
            var body = string.Join("\n",
                "User-agent: *",
                "Allow: /",
                // The admin area is auth-gated, but keep crawlers out of it anyway.
                "Disallow: /admin",
                $"Sitemap: {baseUrl}/sitemap.xml",
                "");
            return Results.Text(body, "text/plain");
        });

        endpoints.MapGet("/sitemap.xml", async (
            HttpRequest request, FeedRepository feed, CancellationToken cancellationToken) =>
        {
            var baseUrl = BaseUrl(request);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlset = new XElement(ns + "urlset");

            foreach (var path in StaticPaths)
            {
                urlset.Add(new XElement(ns + "url",
                    new XElement(ns + "loc", baseUrl + path)));
            }

            var items = await feed.GetAllPublishedAsync(SitemapLimit, cancellationToken);
            foreach (var item in items)
            {
                var url = new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}/research/{item.Slug}"));
                if (item.PublishedAt is { } date)
                {
                    url.Add(new XElement(ns + "lastmod", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
                }
                urlset.Add(url);
            }

            return XmlResult(new XDocument(urlset));
        });

        endpoints.MapGet("/feed.xml", async (
            HttpRequest request, FeedRepository feed, CancellationToken cancellationToken) =>
        {
            var baseUrl = BaseUrl(request);
            var items = await feed.GetAllPublishedAsync(FeedLimit, cancellationToken);

            var channel = new XElement("channel",
                new XElement("title", "BrainHarbor — brain tumor research in plain language"),
                new XElement("link", $"{baseUrl}/research"),
                new XElement("description",
                    "Plain-language summaries of new brain tumor research and news, for patients and caregivers."),
                new XElement("language", "en"));

            foreach (var item in items)
            {
                var link = $"{baseUrl}/research/{item.Slug}";
                var entry = new XElement("item",
                    new XElement("title", item.PlainTitle ?? item.Title),
                    new XElement("link", link),
                    new XElement("guid", new XAttribute("isPermaLink", "true"), link),
                    new XElement("description", item.PlainSummary ?? item.PlainTitle ?? item.Title));
                if (item.PublishedAt is { } date)
                {
                    entry.Add(new XElement("pubDate",
                        date.ToDateTime(TimeOnly.MinValue).ToString("r", CultureInfo.InvariantCulture)));
                }
                channel.Add(entry);
            }

            var rss = new XElement("rss", new XAttribute("version", "2.0"), channel);
            return XmlResult(new XDocument(rss));
        });
    }

    private static string BaseUrl(HttpRequest request) => $"{request.Scheme}://{request.Host}";

    private static IResult XmlResult(XDocument document)
    {
        var xml = new StringBuilder();
        xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
        xml.Append(document.ToString(SaveOptions.DisableFormatting));
        return Results.Text(xml.ToString(), "application/xml");
    }
}
