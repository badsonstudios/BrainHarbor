using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BrainHarbor.Tests;

/// <summary>
/// A WebApplicationFactory that also runs the app on real Kestrel (random
/// loopback port) so browser-based tests (Playwright, WI-102) have an actual
/// URL — TestServer is in-memory only. Dual-host pattern: the TestServer host
/// keeps WebApplicationFactory's plumbing working; the Kestrel host serves
/// <see cref="ServerAddress"/>.
/// </summary>
public sealed class KestrelWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _kestrelHost;

    // Pool-capped shared string — the dual-host factory opens two pools, so
    // this is the one most likely to exhaust connections without the cap.
    private static string ConnectionString => TestDatabase.ConnectionString;

    public string ServerAddress =>
        _kestrelHost is null
            ? throw new InvalidOperationException("Call CreateClient() (or EnsureServer) first.")
            : _kestrelHost.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.First();

    public void EnsureServer() => _ = CreateClient();

    /// <summary>Key the Pipeline's client uses in WI-203 integration tests.</summary>
    public const string SyncApiKey = "kestrel-test-sync-key-0123456789";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString);
        builder.UseSetting("SYNC_API_KEY", SyncApiKey);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Order matters (deferred host builder): build the TestServer host
        // BEFORE adding Kestrel config, then build + start the Kestrel twin,
        // then start the test host.
        var testHost = builder.Build();

        builder.ConfigureWebHost(web => web.UseKestrel().UseUrls("http://127.0.0.1:0"));
        _kestrelHost = builder.Build();
        _kestrelHost.Start();

        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _kestrelHost?.Dispose();
        base.Dispose(disposing);
    }
}
