using BrainHarbor.Web.Models;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-438: the windowed pager. This is pure arithmetic with a lot of edges
/// (first page, last page, too few pages to need a gap, one-page holes), which
/// is exactly the kind of thing that looks right in a browser on the page you
/// happened to load and is wrong two pages over.
/// </summary>
public class PaginationTests
{
    private static string Render(Pagination p) =>
        string.Join(" ", p.Links().Select(l =>
            l.IsGap ? "…" : l.IsCurrent ? $"[{l.Number}]" : l.Number.ToString()));

    [Theory]
    // Few enough pages to show them all, whatever the current one is.
    [InlineData(1, 5, "[1] 2 3 4 5")]
    [InlineData(3, 5, "1 2 [3] 4 5")]
    [InlineData(5, 5, "1 2 3 4 [5]")]
    // Wide enough to need a gap on one side...
    [InlineData(1, 20, "[1] 2 3 … 20")]
    [InlineData(20, 20, "1 … 18 19 [20]")]
    // ...or both.
    [InlineData(10, 20, "1 … 8 9 [10] 11 12 … 20")]
    // A ONE-page hole renders the page rather than a gap: the marker would take
    // the same room as the number it hides, so hiding it buys nothing.
    [InlineData(4, 20, "1 2 3 [4] 5 6 … 20")]
    [InlineData(17, 20, "1 … 15 16 [17] 18 19 20")]
    public void TheWindowShowsFirstLastAndTheNeighbourhood(int current, int total, string expected)
    {
        Assert.Equal(expected, Render(new Pagination(current, total)));
    }

    [Fact]
    public void ASinglePageNeedsNoPager()
    {
        var p = Pagination.For(totalCount: 7, pageSize: 20, currentPageOneBased: 1);

        Assert.Equal(1, p.TotalPages);
        Assert.False(p.IsNeeded);
        Assert.False(p.HasPrevious);
        Assert.False(p.HasNext);
    }

    /// <summary>
    /// An empty feed still has one page. Zero would allow "Page 1 of 0", and a
    /// pager that offers a page it cannot show is worse than no pager.
    /// </summary>
    [Fact]
    public void AnEmptyFeedStillHasOnePage()
    {
        var p = Pagination.For(totalCount: 0, pageSize: 20, currentPageOneBased: 1);

        Assert.Equal(1, p.TotalPages);
        Assert.Equal(1, p.CurrentPage);
    }

    /// <summary>
    /// A hand-typed or stale URL must land somewhere real rather than 500 or
    /// show an empty page with no way back.
    /// </summary>
    [Theory]
    [InlineData(-5, 1)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(99, 3)]
    public void AnOutOfRangePageIsClampedIntoRange(int asked, int expected)
    {
        var p = Pagination.For(totalCount: 50, pageSize: 20, currentPageOneBased: asked);

        Assert.Equal(expected, p.CurrentPage);
        Assert.Equal(3, p.TotalPages);
    }

    [Fact]
    public void APartialLastPageStillCounts()
    {
        // 41 items at 20 per page is three pages, the last holding one item.
        Assert.Equal(3, Pagination.For(41, 20, 1).TotalPages);
        Assert.Equal(2, Pagination.For(40, 20, 1).TotalPages);
    }

    [Fact]
    public void PreviousAndNextNeverPointOutsideTheRange()
    {
        var first = new Pagination(1, 4);
        Assert.False(first.HasPrevious);
        Assert.Equal(1, first.PreviousPage);

        var last = new Pagination(4, 4);
        Assert.False(last.HasNext);
        Assert.Equal(4, last.NextPage);
    }

    /// <summary>
    /// The current page appears exactly once and is always marked. A pager that
    /// marks nothing leaves a reader with no idea where they are.
    /// </summary>
    [Theory]
    [InlineData(1, 20)]
    [InlineData(4, 20)]
    [InlineData(10, 20)]
    [InlineData(20, 20)]
    public void ExactlyOneLinkIsMarkedCurrent(int current, int total)
    {
        var links = new Pagination(current, total).Links();

        var marked = links.Where(l => l.IsCurrent).ToList();
        Assert.Single(marked);
        Assert.Equal(current, marked[0].Number);
        Assert.DoesNotContain(links, l => !l.IsGap && (l.Number < 1 || l.Number > total));
    }
}
