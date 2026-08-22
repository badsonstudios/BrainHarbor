namespace BrainHarbor.Web.Models;

/// <summary>
/// One entry in the pager: either a page to go to, or a gap.
/// </summary>
public sealed record PageLink(int Number, bool IsCurrent, bool IsGap)
{
    public static PageLink To(int number, bool isCurrent) => new(number, isCurrent, false);
    public static readonly PageLink Gap = new(0, false, true);
}

/// <summary>
/// The windowed page list a reader sees (WI-438) — the shape a product listing
/// uses: first, last, the current page and a couple either side, with a gap
/// marker standing in for the rest.
///
/// Page numbers here are ONE-BASED, because these are the numbers printed on
/// screen and put in URLs. <see cref="Feed.FeedQuery"/> and the repositories
/// stay zero-based internally; the translation happens once, in the page model,
/// and is covered by tests. Mixing the two conventions in one place is the
/// obvious way to get an off-by-one that only shows on the last page.
/// </summary>
public sealed record Pagination(int CurrentPage, int TotalPages)
{
    /// <summary>
    /// How many pages to show either side of the current one. Two keeps the
    /// control narrow enough for a phone while still letting a reader jump
    /// ahead rather than only stepping.
    /// </summary>
    public const int Radius = 2;

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public int PreviousPage => Math.Max(1, CurrentPage - 1);
    public int NextPage => Math.Min(TotalPages, CurrentPage + 1);

    /// <summary>A single page needs no pager at all.</summary>
    public bool IsNeeded => TotalPages > 1;

    public static Pagination For(int totalCount, int pageSize, int currentPageOneBased)
    {
        // A feed with nothing in it still has one (empty) page — reporting zero
        // would make "Page 1 of 0" possible.
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var current = Math.Clamp(currentPageOneBased, 1, totalPages);
        return new Pagination(current, totalPages);
    }

    /// <summary>
    /// First, last, and the window around the current page, with
    /// <see cref="PageLink.Gap"/> where pages were skipped.
    ///
    /// A gap is only emitted when it actually hides something. With a
    /// one-page hole the gap marker costs the same width as the page number it
    /// replaces, so it renders the number instead — "1 … 3 4 5" would be a
    /// worse thing to look at than "1 2 3 4 5".
    /// </summary>
    public IReadOnlyList<PageLink> Links()
    {
        var wanted = new SortedSet<int> { 1, TotalPages };
        for (var p = CurrentPage - Radius; p <= CurrentPage + Radius; p++)
        {
            if (p >= 1 && p <= TotalPages)
            {
                wanted.Add(p);
            }
        }

        var links = new List<PageLink>();
        var previous = 0;
        foreach (var p in wanted)
        {
            if (previous != 0 && p - previous > 1)
            {
                if (p - previous == 2)
                {
                    links.Add(PageLink.To(previous + 1, previous + 1 == CurrentPage));
                }
                else
                {
                    links.Add(PageLink.Gap);
                }
            }

            links.Add(PageLink.To(p, p == CurrentPage));
            previous = p;
        }

        return links;
    }
}
