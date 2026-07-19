using System.Xml.Linq;
using BrainHarbor.Pipeline.Sources;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-205: RSS parsing and — the part that matters legally — the per-source
/// text policy. ScienceDaily's licence allows headline + summary + link only;
/// that rule is enforced in code, not left to a comment.
/// </summary>
public class RssFetcherTests
{
    private const string RssFeed = """
        <?xml version="1.0"?>
        <rss version="2.0">
          <channel>
            <title>Brain Tumor News</title>
            <item>
              <title>New imaging dye helps surgeons see glioma edges</title>
              <link>https://example.org/news/1</link>
              <guid>https://example.org/news/1</guid>
              <description>&lt;p&gt;The dye makes &lt;b&gt;tumor&lt;/b&gt; tissue glow.&lt;/p&gt;</description>
              <pubDate>Tue, 09 Jun 2026 14:30:00 GMT</pubDate>
            </item>
            <item>
              <title>Meningioma surgery outcomes improve</title>
              <link>https://example.org/news/2</link>
              <guid>guid-2</guid>
              <description>Short note.</description>
              <pubDate>Mon, 08 Jun 2026 09:00:00 GMT</pubDate>
            </item>
          </channel>
        </rss>
        """;

    private const string AtomFeed = """
        <?xml version="1.0"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
          <entry>
            <title>Glioblastoma vaccine trial opens</title>
            <link href="https://example.org/atom/1"/>
            <id>tag:example.org,2026:1</id>
            <summary>A new trial is enrolling.</summary>
            <published>2026-06-10T12:00:00Z</published>
          </entry>
        </feed>
        """;

    // ParseFeed keeps the FULL description; the licence policy is applied
    // afterwards in FetchAsync, so relevance filtering never reads a truncated
    // teaser. These helpers mirror that split.
    private static IReadOnlyList<FetchedItem> Parse(string xml, RssFeedDefinition definition) =>
        [.. RssFetcher.ParseFeed(XDocument.Parse(xml), definition)
            .Select(p => p.Item with
            {
                RawSummary = RssFetcher.ApplyTextPolicy(p.Item.RawSummary, definition.TextPolicy),
            })];

    [Fact]
    public void ParsesRssItems()
    {
        var items = Parse(RssFeed, RssFeedDefinition.Nci);

        Assert.Equal(2, items.Count);
        var first = items[0];
        Assert.Equal("New imaging dye helps surgeons see glioma edges", first.Title);
        Assert.Equal("https://example.org/news/1", first.Url);
        Assert.Equal("https://example.org/news/1", first.ExternalId);
        Assert.Equal(new DateOnly(2026, 6, 9), first.PublishedAt);
        Assert.Equal("nci_rss", first.Source);
        Assert.Equal("news", first.SourceKind);
    }

    [Fact]
    public void ParsesAtomEntries()
    {
        var items = Parse(AtomFeed, RssFeedDefinition.Nci);

        var entry = Assert.Single(items);
        Assert.Equal("Glioblastoma vaccine trial opens", entry.Title);
        Assert.Equal("https://example.org/atom/1", entry.Url);
        Assert.Equal("tag:example.org,2026:1", entry.ExternalId);
        Assert.Equal(new DateOnly(2026, 6, 10), entry.PublishedAt);
    }

    [Fact]
    public void StripsHtmlAndDecodesEntitiesInDescriptions()
    {
        var summary = Parse(RssFeed, RssFeedDefinition.Nci)[0].RawSummary;

        Assert.Equal("The dye makes tumor tissue glow.", summary);
    }

    [Fact]
    public void PrefersGuidOverLinkAsTheDedupeKey()
    {
        Assert.Equal("guid-2", Parse(RssFeed, RssFeedDefinition.Nci)[1].ExternalId);
    }

    [Fact]
    public void SkipsEntriesWithoutATitleOrLink()
    {
        var items = Parse("""
            <rss version="2.0"><channel>
              <item><link>https://example.org/x</link></item>
              <item><title>No link here</title></item>
            </channel></rss>
            """, RssFeedDefinition.Nci);

        Assert.Empty(items);
    }

    [Fact]
    public void UnparseableDatesBecomeNullRatherThanFailing()
    {
        var items = Parse("""
            <rss version="2.0"><channel><item>
              <title>Glioma news</title><link>https://example.org/x</link>
              <pubDate>whenever</pubDate>
            </item></channel></rss>
            """, RssFeedDefinition.Nci);

        Assert.Null(Assert.Single(items).PublishedAt);
    }

    // ---------- licensing ----------

    [Fact]
    public void NciKeepsFullTextBecauseItIsPublicDomain()
    {
        var longText = new string('a', 1000);

        var result = RssFetcher.ApplyTextPolicy(longText, FeedTextPolicy.FullTextAllowed);

        Assert.Equal(longText, result);
    }

    [Fact]
    public void ScienceDailyIsTruncatedToATeaser()
    {
        // PLAN.md §5: headline + summary + link ONLY. Republishing their
        // article text is not licensed, so this is a legal boundary.
        var longText = string.Join(' ', Enumerable.Repeat("word", 400));

        var result = RssFetcher.ApplyTextPolicy(longText, FeedTextPolicy.HeadlineAndTeaserOnly);

        Assert.NotNull(result);
        Assert.True(result!.Length <= RssFetcher.TeaserLength + 1,
            $"teaser was {result.Length} chars");
        Assert.EndsWith("…", result);
    }

    [Fact]
    public void ShortDescriptionsAreNotTruncatedOrEllipsized()
    {
        var result = RssFetcher.ApplyTextPolicy("A short note.", FeedTextPolicy.HeadlineAndTeaserOnly);

        Assert.Equal("A short note.", result);
    }

    [Fact]
    public void TeaserCutsOnAWordBoundary()
    {
        var text = string.Join(' ', Enumerable.Repeat("alpha", 200));

        var result = RssFetcher.ApplyTextPolicy(text, FeedTextPolicy.HeadlineAndTeaserOnly);

        Assert.DoesNotContain("alph…", result);
    }

    [Fact]
    public void TheTwoShippedFeedsCarryTheCorrectLicensingPolicy()
    {
        Assert.Equal(FeedTextPolicy.FullTextAllowed, RssFeedDefinition.Nci.TextPolicy);
        Assert.Equal(FeedTextPolicy.HeadlineAndTeaserOnly, RssFeedDefinition.ScienceDaily.TextPolicy);
        Assert.Equal("nci_rss", RssFeedDefinition.Nci.Source);
        Assert.Equal("sciencedaily", RssFeedDefinition.ScienceDaily.Source);
    }

    [Fact]
    public void AnUndeclaredTextPolicyFailsClosed()
    {
        // The default enum value must not be the permissive one: a feed whose
        // licence nobody checked cannot silently keep full text.
        Assert.Equal(FeedTextPolicy.Unspecified, default);
        Assert.Throws<InvalidOperationException>(
            () => RssFetcher.ApplyTextPolicy("some text", default));
    }

    [Fact]
    public void RelevanceIsJudgedOnTheFullDescriptionNotTheTeaser()
    {
        // Regression: filtering the truncated teaser dropped items whose
        // brain-tumor mention fell past the cut — e.g. a breast-cancer study
        // that turns out to be about brain metastases.
        var padding = string.Join(' ', Enumerable.Repeat("padding", 100));
        var feed = $"""
            <rss version="2.0"><channel><item>
              <title>Breast cancer therapy shows early promise</title>
              <link>https://example.org/sd/2</link>
              <description>{padding} The study measured brain metastases outcomes.</description>
            </item></channel></rss>
            """;

        var parsed = RssFetcher.ParseFeed(XDocument.Parse(feed), RssFeedDefinition.ScienceDaily);
        var entry = Assert.Single(parsed);

        Assert.False(
            BrainTumorPreFilter.ShouldExclude(entry.Title, entry.FullDescriptionForFiltering),
            "the full description mentions brain metastases, so this must be kept");

        // ...and the stored text is still teaser-limited by the licence.
        var stored = RssFetcher.ApplyTextPolicy(
            entry.Item.RawSummary, RssFeedDefinition.ScienceDaily.TextPolicy);
        Assert.True(stored!.Length <= RssFetcher.TeaserLength + 1);
    }

    [Fact]
    public void TheNciFeedUrlPointsAtALivePublishedContentPath()
    {
        // The previous /syndication/rss-feed?feedName= URL 404s; cancer.gov
        // retires syndication paths, and a dead feed fails silently forever.
        Assert.Contains("publishedcontent/rss", RssFeedDefinition.Nci.FeedUrl);
    }

    [Fact]
    public void ScienceDailyItemsGoThroughTheTeaserPolicyEndToEnd()
    {
        var longDescription = string.Join(' ', Enumerable.Repeat("detail", 200));
        var feed = $"""
            <rss version="2.0"><channel><item>
              <title>Glioma study reported</title>
              <link>https://example.org/sd/1</link>
              <description>{longDescription}</description>
            </item></channel></rss>
            """;

        var item = Assert.Single(Parse(feed, RssFeedDefinition.ScienceDaily));

        Assert.True(item.RawSummary!.Length <= RssFetcher.TeaserLength + 1);
    }
}
