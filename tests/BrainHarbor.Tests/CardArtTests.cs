using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;

namespace BrainHarbor.Tests;

/// <summary>
/// The card hero: a content-matched photo backdrop from a vetted pool. The
/// safety-critical parts: the theme is derived from the item's own words and
/// stage, selection is deterministic, and the URL always comes from the
/// installed pool (never invented). The readiness dial that used to sit on top
/// was retired by the journey handoff, 2026-08-19; the journey path took its
/// place, and is rendered by _FeedCard.cshtml rather than from here.
/// </summary>
public class CardArtTests
{

    private static FeedCard Card(string title = "A study", string hook = "", string url = "/research/x",
        ResearchStage stage = ResearchStage.TestedInPeople, int? readiness = 6) =>
        new(stage, title, url, hook, [], "date", "PubMed", readiness);

    // ---- theme derivation (the content match) ----

    [Theory]
    [InlineData("A pill for IDH-mutant glioma", "", ResearchStage.TestedInPeople, "genetics")]
    [InlineData("A gene change in low-grade glioma", "", ResearchStage.TestedInPeople, "genetics")]
    [InlineData("A virus tested in mice", "", ResearchStage.EarlyResearchAnimals, "lab")]
    [InlineData("Immunotherapy in a dish", "cell line work", ResearchStage.EarlyResearchLabCells, "lab")]
    [InlineData("Meningioma deaths in a national database", "", ResearchStage.TestedInPeople, "data")]
    [InlineData("A new radiosurgery approach", "surgery outcomes", ResearchStage.TestedInPeople, "brain")]
    [InlineData("A press release", "", ResearchStage.News, "abstract")]
    public void ThemeIsDerivedFromContentAndStage(string title, string hook, ResearchStage stage, string expected)
    {
        Assert.Equal(expected, CardImages.ThemeFor(Card(title, hook, stage: stage)));
    }

    // ---- selection: deterministic + always from the vetted pool ----

    private static CardImages Pool() =>
        new(Path.Combine(FindRepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "img", "cards"));

    [Fact]
    public void EveryCardGetsAVettedImageAndTheSameItemAlwaysGetsTheSameOne()
    {
        var images = Pool();
        var card = Card(url: "/research/a-pill-for-glioma");

        var url = images.UrlFor(card);

        Assert.NotNull(url);
        Assert.StartsWith("/img/cards/", url);       // only ever from the pool
        Assert.EndsWith(".jpg", url);
        Assert.Equal(url, images.UrlFor(card));       // stable
    }

    [Fact]
    public void AGeneticsPostGetsAGeneticsImage()
    {
        var url = Pool().UrlFor(Card("IDH-mutant glioma", url: "/research/idh"));
        Assert.Contains("/img/cards/genetics-", url);
    }

    // ---- the hero background ----

    [Fact]
    public void TheHeroUsesThePhotoAsItsBackground()
    {
        Assert.Equal(
            "background-image:url('/img/cards/brain-01.jpg')",
            CardArt.HeroStyle("/img/cards/brain-01.jpg"));
    }

    /// <summary>
    /// A card with no matching photo still needs a surface: the journey path is
    /// laid over the hero, so an empty background would leave the indicator
    /// plate floating on nothing.
    /// </summary>
    [Fact]
    public void AnItemWithNoPhotoFallsBackToAGradientRatherThanAnEmptyBox()
    {
        var style = CardArt.HeroStyle(null);

        Assert.DoesNotContain("background-image", style);
        Assert.Contains("linear-gradient", style);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
