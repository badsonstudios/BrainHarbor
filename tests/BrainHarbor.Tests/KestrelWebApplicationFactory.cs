using System.Diagnostics;
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
    /// <summary>
    /// How long the second host gets to reach ApplicationStarted. The first host
    /// has already run DbUp and the admin seeder by then, so this covers a port
    /// bind and a second, no-op pass of each — generous on purpose. A start that
    /// FAILS does not wait this long: see <see cref="WaitForOwnStart"/>.
    /// </summary>
    private static readonly TimeSpan KestrelStartTimeout = TimeSpan.FromSeconds(90);

    private IHost? _kestrelHost;

    // Pool-capped shared string — the dual-host factory opens two pools, so
    // this is the one most likely to exhaust connections without the cap.
    private static string ConnectionString => TestDatabase.ConnectionString;

    public string ServerAddress =>
        _kestrelHost is null
            ? throw new InvalidOperationException("Call CreateClient() (or EnsureServer) first.")
            : _kestrelHost.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.First();

    /// <summary>
    /// WI-439. Holds the TestServer host's start back by this long. Test-only
    /// lever for <see cref="KestrelWebApplicationFactoryStartTests"/>: it makes
    /// the race that host used to lose happen every time instead of one run in
    /// five.
    /// </summary>
    internal TimeSpan TestServerStartDelay { get; init; }

    /// <summary>WI-439. The same lever for the Kestrel host.</summary>
    internal TimeSpan KestrelStartDelay { get; init; }

    /// <summary>WI-439. Makes the Kestrel host's start throw this, after it is built.</summary>
    internal Exception? KestrelStartFailure { get; init; }

    /// <summary>
    /// WI-439. The TestServer host's ApplicationStopping token, recorded when a
    /// lever above is set, so a test can see a half-started factory clean up.
    /// </summary>
    internal CancellationToken TestServerStopping { get; private set; }

    private readonly Lock _startGate = new();

    /// <summary>
    /// Starts the dual host once, whoever asks first.
    ///
    /// Serialized because <see cref="WebApplicationFactory{T}.CreateClient"/> is
    /// not thread-safe: two test classes sharing this fixture could both enter
    /// host creation (WI-403). The intermittent "The server has not been
    /// started" this used to throw was NOT that — see <see cref="CreateHost"/>.
    /// </summary>
    public void EnsureServer()
    {
        lock (_startGate)
        {
            try
            {
                _ = CreateClient();
            }
            catch (Exception exception)
            {
                // Name what happened. The old message guessed at two causes
                // ("the database... or a port") and named neither.
                var cause = exception.GetBaseException();
                throw new InvalidOperationException(
                    $"The Kestrel test host did not start: {cause.GetType().Name}: {cause.Message}",
                    exception);
            }
        }
    }

    /// <summary>Key the Pipeline's client uses in WI-203 integration tests.</summary>
    public const string SyncApiKey = "kestrel-test-sync-key-0123456789";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:BrainHarbor", ConnectionString);
        builder.UseSetting("SYNC_API_KEY", SyncApiKey);

        if (TestServerStartDelay > TimeSpan.Zero || KestrelStartDelay > TimeSpan.Zero
            || KestrelStartFailure is not null)
        {
            builder.ConfigureServices(services => services.AddSingleton<IHostedService>(provider =>
            {
                if (provider.GetRequiredService<IServer>() is TestServer)
                {
                    TestServerStopping = provider.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
                    return new StartLever(TestServerStartDelay, null);
                }

                return new StartLever(KestrelStartDelay, KestrelStartFailure);
            }));
        }
    }

    /// <summary>
    /// WI-439. Both hosts come from ONE deferred host builder, and
    /// <c>DeferredHost.StartAsync</c> does not start anything: the app's own
    /// <c>app.Run()</c> does that on the entry-point thread, and <c>StartAsync</c>
    /// only awaits a TaskCompletionSource that EVERY host built from that builder
    /// shares. The first waited-on host to reach ApplicationStarted — or any entry
    /// point exiting — releases every host's <c>Start()</c>.
    ///
    /// This used to build both hosts, then start Kestrel, then the TestServer.
    /// Each entry point runs DbUp (behind an advisory lock) and the admin seeder
    /// before <c>app.Run()</c>, on its own thread, at the same time. Whenever the
    /// TestServer host lost that race, Kestrel's start released the shared
    /// signal, <c>testHost.Start()</c> returned instantly, and the first
    /// <c>CreateClient()</c> met a TestServer that had not started. That landed
    /// on the first test xUnit ran in the class, and the rest passed because the
    /// server had finished starting by the time they asked. On `main` it skipped
    /// the deploy.
    ///
    /// Now: build and fully start the TestServer host — its <c>Start()</c> is
    /// genuine because nothing has released the signal yet, and it rethrows that
    /// host's entry-point exception — and only then build the Kestrel host and
    /// wait on ITS OWN lifetime, because its <c>Start()</c> would return at once.
    /// The cost is that the Kestrel host's entry-point exception has nowhere to
    /// go; <see cref="WaitForOwnStart"/> says so rather than guessing.
    ///
    /// Order still matters: the TestServer host is built BEFORE the Kestrel
    /// configuration is added to the builder.
    /// </summary>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();
        try
        {
            testHost.Start();

            builder.ConfigureWebHost(web => web.UseKestrel().UseUrls("http://127.0.0.1:0"));
            _kestrelHost = builder.Build();
            WaitForOwnStart(_kestrelHost, KestrelStartTimeout);

            return testHost;
        }
        catch
        {
            // The base factory never sees a host we fail to return, so nothing
            // else would stop these — and each one holds a connection pool.
            StopAndDispose(_kestrelHost);
            _kestrelHost = null;
            StopAndDispose(testHost);
            throw;
        }
    }

    /// <summary>
    /// Waits for the host's own ApplicationStarted. A start that fails does not
    /// sit out the timeout: <c>app.Run()</c> disposes its host when the start
    /// throws, so a disposed host is the signal that the entry point is gone.
    /// </summary>
    private static void WaitForOwnStart(IHost host, TimeSpan timeout)
    {
        var clock = Stopwatch.StartNew();
        while (clock.Elapsed < timeout)
        {
            try
            {
                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                if (lifetime.ApplicationStarted.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(200)))
                {
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                throw KestrelDidNotStart("was disposed by its own entry point before it started");
            }
        }

        throw KestrelDidNotStart($"did not reach ApplicationStarted within {timeout.TotalSeconds}s");
    }

    private static InvalidOperationException KestrelDidNotStart(string what) => new(
        $"The Kestrel host {what}. Its own exception cannot be shown: both hosts share one "
        + "start signal, the TestServer host had already released it, so the exception from "
        + "the Kestrel entry point was dropped. The TestServer host started from the same "
        + "Program moments earlier, so look for what a SECOND start can hit: a port bind, "
        + "connection-pool exhaustion (53300), or a hosted service that throws or hangs. The "
        + "app's console log may carry the original.");

    private static void StopAndDispose(IHost? host)
    {
        if (host is null)
        {
            return;
        }

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            host.StopAsync(timeout.Token).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            // Already stopped, never started, or disposed: nothing left to stop.
        }

        try
        {
            host.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    protected override void Dispose(bool disposing)
    {
        // Stop, not just dispose: a disposed-but-running host leaves its entry
        // point blocked in app.Run() for the rest of the test run.
        StopAndDispose(_kestrelHost);
        base.Dispose(disposing);
    }

    /// <summary>
    /// Delays or fails a host's start from <c>StartingAsync</c>, which runs for
    /// every hosted service before any <c>StartAsync</c> — the server's included —
    /// so ApplicationStarted moves by exactly this much.
    /// </summary>
    private sealed class StartLever(TimeSpan delay, Exception? failure) : IHostedLifecycleService
    {
        public async Task StartingAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            if (failure is not null)
            {
                throw failure;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
