using System.Collections.Concurrent;

namespace BrainHarbor.Web.Analytics;

/// <summary>
/// A tally of how many times each page was opened, per day (WI-441).
///
/// **What this is not.** It is not analytics in the usual sense and must never
/// become it. Nothing here identifies a reader: no IP, no hashed IP, no user
/// agent, no session or device id, no referrer, no timestamp finer than the
/// day. Two counts cannot be linked to each other, and no count can be linked
/// to a person — which is what lets /privacy keep saying we do not build a
/// profile of you, rather than saying it on a technicality.
///
/// Counting people would require persisting an identifier across requests.
/// Dan's decision (2026-08-23) was to give that up rather than weaken the
/// promise: page openings, and nothing attached to them.
///
/// Counts accumulate in memory and are flushed by
/// <see cref="PageViewFlusher"/>, so serving a page never waits on a database
/// write. Up to one flush interval of counts is lost if the process dies. That
/// is the right trade for a view counter and the wrong one for anything that
/// matters — do not reuse this for something that does.
/// </summary>
public sealed class PageViewCounter
{
    private ConcurrentDictionary<(DateOnly Day, string Path), long> _counts = new();

    public void Record(DateOnly day, string path) =>
        _counts.AddOrUpdate((day, path), 1, static (_, existing) => existing + 1);

    /// <summary>
    /// Takes everything counted so far and resets. Swapping the dictionary
    /// rather than draining key-by-key means a view arriving mid-flush lands in
    /// the new one and is counted next time, instead of being dropped or
    /// double-counted.
    /// </summary>
    public IReadOnlyCollection<KeyValuePair<(DateOnly Day, string Path), long>> Drain()
    {
        var taken = Interlocked.Exchange(ref _counts, new ConcurrentDictionary<(DateOnly, string), long>());
        return [.. taken];
    }
}
