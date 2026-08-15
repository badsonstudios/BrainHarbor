using BrainHarbor.Web.Models;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-109: the stage → badge mapping is the site's core trust device
/// (content-pipeline.md). The evidence ranks and aria-labels here are spec,
/// not style — docs/design/homepage-handoff/README.md §Stage badge.
///
/// Four rungs since 2026-08-15, not five, and with no gap in the ladder. The
/// old scale ran 5, 4, 2, 1 with nothing at 3, so the step between "review of
/// existing research" and "animal study" was twice the step anywhere else for
/// no reason a reader could infer.
/// </summary>
public class StageBadgeTests
{
    [Theory]
    [InlineData(ResearchStage.TestedInPeople, BadgeKind.Result, "Tested in people", 4)]
    [InlineData(ResearchStage.ReviewOfExistingResearch, BadgeKind.Result, "Review of existing research", 3)]
    [InlineData(ResearchStage.EarlyResearchAnimals, BadgeKind.Result, "Early research (animals)", 2)]
    [InlineData(ResearchStage.EarlyResearchLabCells, BadgeKind.Result, "Early research (lab cells)", 1)]
    [InlineData(ResearchStage.NewOrUpdatedTrial, BadgeKind.Progress, "New or updated trial", 0)]
    [InlineData(ResearchStage.News, BadgeKind.Info, "News", 0)]
    [InlineData(ResearchStage.Preprint, BadgeKind.Unverified, "Preprint — not yet checked by other scientists", 0)]
    public void EveryStageMapsToItsSpecifiedBadge(
        ResearchStage stage, BadgeKind kind, string label, int strength)
    {
        var badge = StageBadge.For(stage);

        Assert.Equal(kind, badge.Kind);
        Assert.Equal(label, badge.Label);
        Assert.Equal(strength, badge.EvidenceStrength);
    }

    [Fact]
    public void ResultBadgesAnnounceEvidenceStrength()
    {
        Assert.Equal("Tested in people. Evidence strength 4 of 4.",
            StageBadge.For(ResearchStage.TestedInPeople).AriaLabel);
        Assert.Equal("Early research (lab cells). Evidence strength 1 of 4.",
            StageBadge.For(ResearchStage.EarlyResearchLabCells).AriaLabel);
    }

    /// <summary>
    /// The ladder has a rung at every step. A gap is not a cosmetic detail: the
    /// meter is the site's trust device, and a reader who counts four marks on
    /// one card and two on the next is entitled to assume there is a three.
    /// </summary>
    [Fact]
    public void TheEvidenceLadderHasNoGaps()
    {
        var ranks = Enum.GetValues<ResearchStage>()
            .Select(StageBadge.For)
            .Where(b => b.Kind == BadgeKind.Result)
            .Select(b => b.EvidenceStrength)
            .OrderBy(r => r)
            .ToList();

        Assert.Equal([1, 2, 3, 4], ranks);
        Assert.All(ranks, r => Assert.InRange(r, 1, StageBadge.MeterSteps));
    }

    [Fact]
    public void NonResultBadgesAnnounceLabelOnly()
    {
        Assert.Equal("New or updated trial.",
            StageBadge.For(ResearchStage.NewOrUpdatedTrial).AriaLabel);
        Assert.Equal("News.", StageBadge.For(ResearchStage.News).AriaLabel);
        // Unverified has a (dashed) meter but must NOT announce a strength.
        Assert.Equal("Preprint — not yet checked by other scientists.",
            StageBadge.For(ResearchStage.Preprint).AriaLabel);
    }

    [Fact]
    public void EveryStageHasAMapping()
    {
        foreach (var stage in Enum.GetValues<ResearchStage>())
        {
            var badge = StageBadge.For(stage);
            Assert.False(string.IsNullOrWhiteSpace(badge.Label));
            Assert.StartsWith("badge badge--", badge.CssClass);
        }
    }

    [Fact]
    public void CssClassMatchesKind()
    {
        Assert.Equal("badge badge--result", StageBadge.For(ResearchStage.TestedInPeople).CssClass);
        Assert.Equal("badge badge--progress", StageBadge.For(ResearchStage.NewOrUpdatedTrial).CssClass);
        Assert.Equal("badge badge--info", StageBadge.For(ResearchStage.News).CssClass);
        Assert.Equal("badge badge--unverified", StageBadge.For(ResearchStage.Preprint).CssClass);
    }
}
