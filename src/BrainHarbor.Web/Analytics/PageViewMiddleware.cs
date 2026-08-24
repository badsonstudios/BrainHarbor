using System.Text.RegularExpressions;

namespace BrainHarbor.Web.Analytics;

/// <summary>
/// Decides what counts as "a page was opened" (WI-441).
///
/// The bar is deliberately high, because a number nobody trusts is worse than
/// no number. Only a successful GET that returned an HTML page is counted —
/// which excludes CSS, images, htmx fragments, the sync API, the admin area,
/// redirects and every error.
/// </summary>
public sealed partial class PageViewMiddleware(RequestDelegate next, PageViewCounter counter)
{
    /// <summary>
    /// Automated callers, filtered so the tally reflects readers rather than
    /// our own infrastructure. The deploy smoke check alone polls five routes
    /// in a loop on every release, which would swamp a small site's real
    /// numbers — the raw Azure Requests metric is unusable for exactly this
    /// reason.
    ///
    /// **The user agent is read to make this decision and then discarded.** It
    /// is never stored, never counted against, and never leaves this method.
    /// </summary>
    [GeneratedRegex(
        @"bot|crawl|spider|slurp|curl|wget|httpclient|python-requests|okhttp|"
        + @"headless|playwright|puppeteer|lighthouse|monitor|uptime|preview|scanner",
        RegexOptions.IgnoreCase)]
    private static partial Regex Automated();

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (ShouldCount(context))
        {
            // The path only, lowercased, query string dropped. A query can
            // carry a reader's ZIP or coordinates on /trials, and storing that
            // would undo the promise this whole design exists to keep.
            var path = context.Request.Path.Value ?? "/";
            if (path.Length > 200)
            {
                path = path[..200];
            }

            counter.Record(DateOnly.FromDateTime(DateTime.UtcNow), path.ToLowerInvariant());
        }
    }

    private static bool ShouldCount(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) || context.Response.StatusCode != 200)
        {
            return false;
        }

        // HTML only. This is what keeps one page view from counting as twenty:
        // a single home page request pulls stylesheets, htmx, the logo and a
        // photo per card, and every one of those is a request.
        var contentType = context.Response.ContentType;
        if (contentType is null || !contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // htmx swaps fetch a fragment of a page the reader is already on.
        // Counting them would inflate /research every time someone changed a
        // filter, which reads as traffic and is really one person browsing.
        if (context.Request.Headers.ContainsKey("HX-Request"))
        {
            return false;
        }

        var path = context.Request.Path;
        if (path.StartsWithSegments("/admin") || path.StartsWithSegments("/api")
            || path.StartsWithSegments("/dev"))
        {
            return false;
        }

        var agent = context.Request.Headers.UserAgent.ToString();

        // No user agent at all is a script, not a browser.
        return agent.Length > 0 && !Automated().IsMatch(agent);
    }
}
