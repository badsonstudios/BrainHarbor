using BrainHarbor.Web.Services;
using Dapper;

namespace BrainHarbor.Web.Analytics;

/// <summary>One day's tally for one path.</summary>
public sealed class PageViewRow
{
    public DateOnly ViewedOn { get; set; }
    public string Path { get; set; } = "";
    public long Views { get; set; }
}

/// <summary>Reads and writes the page-view tally (WI-441).</summary>
public sealed class PageViewRepository(IDbConnectionFactory connections)
{
    /// <summary>
    /// Adds a batch of counts. Upsert rather than insert-or-read: several
    /// instances (or a restart mid-day) must add to the same row rather than
    /// collide on the primary key or overwrite each other's totals.
    /// </summary>
    public async Task AddAsync(
        IReadOnlyCollection<KeyValuePair<(DateOnly Day, string Path), long>> counts,
        CancellationToken cancellationToken)
    {
        if (counts.Count == 0)
        {
            return;
        }

        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO page_views (viewed_on, path, views)
            VALUES (@Day, @Path, @Views)
            ON CONFLICT (viewed_on, path)
            DO UPDATE SET views = page_views.views + EXCLUDED.views
            """,
            counts.Select(c => new { c.Key.Day, c.Key.Path, Views = c.Value }),
            cancellationToken: cancellationToken));
    }

    /// <summary>Daily totals across the whole site, newest day first.</summary>
    public async Task<IReadOnlyList<PageViewRow>> DailyTotalsAsync(
        int days, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return [.. await connection.QueryAsync<PageViewRow>(new CommandDefinition(
            """
            SELECT viewed_on AS "ViewedOn", '' AS "Path", sum(views) AS "Views"
            FROM page_views
            WHERE viewed_on > (CURRENT_DATE - @days::int)
            GROUP BY viewed_on
            ORDER BY viewed_on DESC
            """,
            new { days },
            cancellationToken: cancellationToken))];
    }

    /// <summary>The most-opened pages over the window, busiest first.</summary>
    public async Task<IReadOnlyList<PageViewRow>> TopPathsAsync(
        int days, int limit, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return [.. await connection.QueryAsync<PageViewRow>(new CommandDefinition(
            """
            SELECT MAX(viewed_on) AS "ViewedOn", path AS "Path", sum(views) AS "Views"
            FROM page_views
            WHERE viewed_on > (CURRENT_DATE - @days::int)
            GROUP BY path
            ORDER BY sum(views) DESC, path
            LIMIT @limit
            """,
            new { days, limit },
            cancellationToken: cancellationToken))];
    }

    public async Task<long> TotalAsync(int days, CancellationToken cancellationToken)
    {
        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<long?>(new CommandDefinition(
            """
            SELECT sum(views) FROM page_views
            WHERE viewed_on > (CURRENT_DATE - @days::int)
            """,
            new { days },
            cancellationToken: cancellationToken)) ?? 0;
    }
}
