using BrainHarbor.Web.Api;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrainHarbor.Web.Pages.Admin;

public class HealthModel(SyncRepository sync) : PageModel
{
    /// <summary>Sources expected to report; anything missing is called out.</summary>
    public static readonly string[] ExpectedSources =
        ["pubmed", "nci_rss", "sciencedaily", "medrxiv", "biorxiv"];

    /// <summary>A daily pipeline that hasn't succeeded in this long is stale.</summary>
    public static readonly TimeSpan StaleAfter = TimeSpan.FromDays(2);

    public IReadOnlyList<SourceHealth> Sources { get; private set; } = [];
    public IReadOnlyList<string> MissingSources { get; private set; } = [];

    public sealed record SourceHealth(
        string Source,
        DateTimeOffset? LastSuccessAt,
        string? LastError,
        string? Cursor)
    {
        public bool IsStale =>
            LastSuccessAt is null || DateTimeOffset.UtcNow - LastSuccessAt > StaleAfter;

        public string LastSuccessText
        {
            get
            {
                if (LastSuccessAt is null)
                {
                    return "never";
                }

                var days = (int)(DateTimeOffset.UtcNow - LastSuccessAt.Value).TotalDays;
                return days switch
                {
                    <= 0 => "today",
                    1 => "yesterday",
                    _ => $"{days} days ago",
                };
            }
        }
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var state = await sync.GetStateAsync(cancellationToken);

        Sources =
        [
            .. state
                .Select(s => new SourceHealth(s.Source, s.LastSuccessAt, s.LastError, s.Cursor))
                .OrderByDescending(s => s.LastError is not null)
                .ThenByDescending(s => s.IsStale)
                .ThenBy(s => s.Source),
        ];

        MissingSources =
        [
            .. ExpectedSources.Where(expected =>
                !state.Any(s => string.Equals(s.Source, expected, StringComparison.Ordinal))),
        ];
    }
}
