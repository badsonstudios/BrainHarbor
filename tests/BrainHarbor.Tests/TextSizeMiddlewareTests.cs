using BrainHarbor.Web.Middleware;
using Microsoft.AspNetCore.Http;

namespace BrainHarbor.Tests;

/// <summary>
/// Pure unit tests for the WI-102 middleware — no server, no DB. The
/// integration-level behavior lives in TextSizeToggleTests.
/// </summary>
public class TextSizeMiddlewareTests
{
    private static DefaultHttpContext Get(string path, string queryString)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(queryString);
        return context;
    }

    private static Task Invoke(HttpContext context) =>
        new TextSizeMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

    [Fact]
    public async Task ProtocolRelativePathCannotCauseOpenRedirect()
    {
        // "//evil.com/" as a Location header is scheme-relative — a browser
        // would leave the site. The middleware must collapse it to "/".
        var context = Get("//evil.com/", "?textsize=large");

        await Invoke(context);

        Assert.Equal("/", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task UnknownValueStripsParamRedirectsAndSetsNoCookie()
    {
        var context = Get("/research", "?textsize=banana");

        await Invoke(context);

        Assert.Equal("/research", context.Response.Headers.Location.ToString());
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task PostRequestsPassThroughUntouched()
    {
        var context = Get("/", "?textsize=large");
        context.Request.Method = HttpMethods.Post;
        var reachedNext = false;

        await new TextSizeMiddleware(_ => { reachedNext = true; return Task.CompletedTask; })
            .InvokeAsync(context);

        Assert.True(reachedNext);
        Assert.False(context.Response.Headers.ContainsKey("Location"));
    }

    [Fact]
    public async Task HeadRequestsGetTheSameRedirectAsGet()
    {
        var context = Get("/", "?textsize=large");
        context.Request.Method = HttpMethods.Head;

        await Invoke(context);

        Assert.Equal("/", context.Response.Headers.Location.ToString());
    }
}
