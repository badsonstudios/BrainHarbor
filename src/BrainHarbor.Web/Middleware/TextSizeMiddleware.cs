namespace BrainHarbor.Web.Middleware;

/// <summary>
/// Large-text mode (WI-102). The toggle is a plain GET link that appends
/// ?textsize=large|standard, so it works with JavaScript disabled — the
/// audience constraint every UI choice answers to. The middleware turns that
/// querystring into a persistent cookie and redirects back to the clean URL;
/// _Layout reads the cookie and adds the text-large class on &lt;html&gt;.
/// The cookie holds a display preference only — no PII (data-model.md rules).
/// </summary>
public static class TextSize
{
    public const string CookieName = "bh_textsize";
    public const string QueryParam = "textsize";
    public const string Large = "large";
    public const string Standard = "standard";

    public static bool IsLarge(HttpContext context) =>
        context.Request.Cookies[CookieName] == Large;

    /// <summary>Href for the header toggle: current URL with the mode flipped.</summary>
    public static string ToggleHref(HttpContext context)
    {
        var target = IsLarge(context) ? Standard : Large;

        // During a status-code re-execute the request path is /status/{code};
        // the toggle must point at the URL the user actually typed, or
        // enlarging text on a dead link strands them on the phantom page.
        string path;
        QueryString query;
        if (context.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>() is { } reExecute)
        {
            path = GuardScheme(new PathString(reExecute.OriginalPathBase)
                .Add(new PathString(reExecute.OriginalPath)).ToUriComponent());
            query = QueryWithoutParam(new QueryString(reExecute.OriginalQueryString));
        }
        else
        {
            path = SafeLocalPath(context);
            query = QueryWithoutParam(context.Request.Query);
        }

        return path + query.Add(QueryParam, target).ToUriComponent();
    }

    /// <summary>
    /// Current path, safe to echo into a redirect or href. A path starting
    /// with "//" would be treated as scheme-relative by browsers — an open
    /// redirect (e.g. /​/evil.com/?textsize=large) — so it collapses to "/".
    /// </summary>
    internal static string SafeLocalPath(HttpContext context) =>
        GuardScheme(context.Request.PathBase.Add(context.Request.Path).ToUriComponent());

    internal static string GuardScheme(string path) =>
        path.StartsWith("//") ? "/" : path;

    internal static QueryString QueryWithoutParam(IQueryCollection query)
    {
        var result = QueryString.Empty;
        foreach (var (key, values) in query)
        {
            if (key == QueryParam) continue;
            foreach (var value in values)
            {
                result = result.Add(key, value ?? string.Empty);
            }
        }
        return result;
    }

    internal static QueryString QueryWithoutParam(QueryString query)
    {
        var result = QueryString.Empty;
        foreach (var (key, values) in
                 Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query.Value ?? string.Empty))
        {
            if (key == QueryParam) continue;
            foreach (var value in values)
            {
                result = result.Add(key, value ?? string.Empty);
            }
        }
        return result;
    }
}

public sealed class TextSizeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if ((HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)) &&
            context.Request.Query.TryGetValue(TextSize.QueryParam, out var requested))
        {
            if (requested == TextSize.Large)
            {
                context.Response.Cookies.Append(TextSize.CookieName, TextSize.Large,
                    new CookieOptions
                    {
                        MaxAge = TimeSpan.FromDays(365),
                        HttpOnly = true,
                        Secure = context.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        IsEssential = true,
                        Path = "/",
                    });
            }
            else if (requested == TextSize.Standard)
            {
                context.Response.Cookies.Delete(TextSize.CookieName);
            }
            // Unknown values change nothing; the param is stripped either way,
            // so a redirect loop on garbage input is impossible.

            // Redirect to the same URL minus the textsize param so the
            // preference never lingers in the address bar or gets shared.
            var remaining = TextSize.QueryWithoutParam(context.Request.Query);
            context.Response.Redirect(TextSize.SafeLocalPath(context) + remaining.ToUriComponent());
            return;
        }

        await next(context);
    }
}
