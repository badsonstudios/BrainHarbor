using System.Diagnostics;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-439. The dual-host factory must hand back BOTH hosts started, whichever
/// one is slower. Before the fix, a TestServer host that started after its
/// Kestrel twin failed the first <c>CreateClient()</c> with "The server has not
/// been started" — about one full-suite run in five locally, three CI runs on
/// 2026-09-15, and a skipped production deploy when it landed on `main`.
///
/// The start levers make each ordering happen every time rather than by luck,
/// so the first test fails against the old <c>CreateHost</c> and the second
/// against one that trusts Kestrel's own <c>Start()</c>. The third holds the
/// failure path to its promises: fast, honest about what it cannot show, and
/// not leaving a started host behind.
/// </summary>
[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class KestrelWebApplicationFactoryStartTests
{
    private static readonly TimeSpan Slow = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task BothHostsServeWhenTheTestServerHostStartsLast()
    {
        using var factory = new KestrelWebApplicationFactory { TestServerStartDelay = Slow };

        await AssertBothHostsServe(factory);
    }

    [Fact]
    public async Task BothHostsServeWhenTheKestrelHostStartsLast()
    {
        using var factory = new KestrelWebApplicationFactory { KestrelStartDelay = Slow };

        await AssertBothHostsServe(factory);
    }

    [Fact]
    public void AKestrelHostThatFailsToStartFailsFastSaysWhyAndStopsItsTwin()
    {
        using var factory = new KestrelWebApplicationFactory
        {
            KestrelStartFailure = new InvalidOperationException("planted by WI-439"),
        };

        var clock = Stopwatch.StartNew();
        var failure = Assert.Throws<InvalidOperationException>(factory.EnsureServer);
        clock.Stop();

        // Not the 90-second timeout: a failed start disposes its host, and the
        // wait notices.
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(30),
            $"the failed start took {clock.Elapsed.TotalSeconds:0}s to report, so it sat out the timeout");

        // Honest about the one thing it cannot show, rather than guessing.
        Assert.Contains("share one start signal", failure.Message, StringComparison.Ordinal);

        // The TestServer host had started. Nobody else will stop it.
        Assert.True(factory.TestServerStopping.IsCancellationRequested,
            "the TestServer host was left running after its Kestrel twin failed to start");
    }

    private static async Task AssertBothHostsServe(KestrelWebApplicationFactory factory)
    {
        factory.EnsureServer();

        // TestServer, in memory.
        using var inMemory = await factory.CreateClient().GetAsync("/get-help-now");
        Assert.True(inMemory.IsSuccessStatusCode,
            $"the TestServer host answered {(int)inMemory.StatusCode}");

        // Kestrel, over a real socket.
        using var http = new HttpClient();
        using var overTheWire = await http.GetAsync(new Uri(new Uri(factory.ServerAddress), "/get-help-now"));
        Assert.True(overTheWire.IsSuccessStatusCode,
            $"the Kestrel host at {factory.ServerAddress} answered {(int)overTheWire.StatusCode}");
    }
}
