using BrainHarbor.Web.Models;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-109: the stage → badge mapping is the site's core trust device
/// (content-pipeline.md). The evidence ranks and aria-labels here are spec,
/// not style — docs/design/entry-hub-handoff/README.md §Stage badge.
/// </summary>
public class StageBadgeTests
{
    [Theory]
    [InlineData(ResearchStage.TestedInPeople, BadgeKind.Result, "Tested in people", 5)]
    [InlineData(ResearchStage.ReviewOfExistingResearch, BadgeKind.Result, "Review of existing research", 4)]
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
        Assert.Equal("Tested in people. Evidence strength 5 of 5.",
            StageBadge.For(ResearchStage.TestedInPeople).AriaLabel);
        Assert.Equal("Early research (lab cells). Evidence strength 1 of 5.",
            StageBadge.For(ResearchStage.EarlyResearchLabCells).AriaLabel);
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
