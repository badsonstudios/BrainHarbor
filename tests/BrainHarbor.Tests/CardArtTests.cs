using System.Text.Encodings.Web;
using BrainHarbor.Web.Feed;
using BrainHarbor.Web.Models;
using Microsoft.AspNetCore.Html;

namespace BrainHarbor.Tests;

/// <summary>
/// The card hero: a content-matched photo backdrop (from a vetted pool) with
/// the item's readiness dial on top. The safety-critical parts: the theme is
/// derived from the item's own words/stage, selection is deterministic, and the
/// URL always comes from the installed pool (never invented).
/// </summary>
public class CardArtTests
{
    private static string Render(IHtmlContent c)
    {
        using var w = new StringWriter();
        c.WriteTo(w, HtmlEncoder.Default);
        return w.ToString();
    }

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

    // ---- the hero markup ----

    [Fact]
    public void TheHeroRendersThePhotoAndTheDialAndIsDecorative()
    {
        var html = Render(CardArt.Hero(Card(readiness: 8), "/img/cards/brain-01.jpg"));

        Assert.Contains("background-image:url('/img/cards/brain-01.jpg')", html);
        Assert.Contains(">8<", html);                 // the readiness number
        Assert.Contains("aria-hidden=\"true\"", html);
    }

    [Fact]
    public void AnUnscoredItemStillGetsAPhotoButNoDial()
    {
        var html = Render(CardArt.Hero(Card(readiness: null), "/img/cards/brain-01.jpg"));

        Assert.Contains("background-image", html);
        Assert.DoesNotContain("card-hero__dial", html);
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
