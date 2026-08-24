namespace BrainHarbor.Web.Analytics;

/// <summary>
/// Writes the in-memory tally to Postgres periodically (WI-441).
///
/// Batching is the point: without it every page view would cost a database
/// round-trip on the request path, on a shared B1 whose connection pool the
/// feed and the sync API also need. A view counter must never be able to slow
/// down — or take down — the pages it is counting.
/// </summary>
public sealed class PageViewFlusher(
    PageViewCounter counter,
    IServiceScopeFactory scopes,
    ILogger<PageViewFlusher> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FlushAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutting down — fall through to the final flush below.
        }

        // One last write on the way out, so a deploy does not silently discard
        // the last minute of counts. CancellationToken.None deliberately: the
        // stopping token is already cancelled by this point, and passing it
        // would cancel the very write this exists to perform.
        await FlushAsync(CancellationToken.None);
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        var counts = counter.Drain();
        if (counts.Count == 0)
        {
            return;
        }

        try
        {
            using var scope = scopes.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<PageViewRepository>();
            await repository.AddAsync(counts, cancellationToken);
        }
        catch (Exception exception)
        {
            // A counter must never take the site down, and must never take
            // itself down either: the loop has to survive to try again. The
            // drained counts are lost, which is the correct thing to lose.
            logger.LogWarning(exception,
                "Could not write {Count} page-view tallies; they are dropped.", counts.Count);
        }
    }
}
