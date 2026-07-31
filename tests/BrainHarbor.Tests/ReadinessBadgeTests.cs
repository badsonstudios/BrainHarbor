using BrainHarbor.Web.Models;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-305/306: the readiness score's reader-facing presentation. The band label
/// and description must match the number, and the accessible label must carry
/// the meaning in words (never color alone).
/// </summary>
public class ReadinessBadgeTests
{
    [Theory]
    [InlineData(10, "Available now")]
    [InlineData(9, "Available now")]
    [InlineData(8, "In late human trials")]
    [InlineData(7, "In late human trials")]
    [InlineData(6, "In early human trials")]
    [InlineData(5, "In early human trials")]
    [InlineData(4, "Watched in people")]
    [InlineData(3, "Expert review")]
    [InlineData(2, "Animal studies")]
    [InlineData(1, "Lab or idea stage")]
    public void EachScoreMapsToItsBandLabel(int score, string expectedLabel)
    {
        Assert.Equal(expectedLabel, ReadinessBadge.For(score).Label);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(99, 10)]
    public void OutOfRangeScoresAreClampedToTheScale(int score, int expected)
    {
        Assert.Equal(expected, ReadinessBadge.For(score).Score);
    }

    [Fact]
    public void TheAriaLabelCarriesTheNumberBandAndReasonInWords()
    {
        var badge = ReadinessBadge.For(2);

        Assert.Contains("2 out of 10", badge.AriaLabel);
        Assert.Contains("Animal studies", badge.AriaLabel);
        // The explanation is spoken too, so meaning never rests on color.
        Assert.Contains("animals only", badge.AriaLabel);
    }

    [Fact]
    public void TheCssModifierVariesByBandSoStylingNeverCarriesMeaningAlone()
    {
        Assert.EndsWith("available", ReadinessBadge.For(10).CssClass);
        Assert.EndsWith("animal", ReadinessBadge.For(2).CssClass);
        Assert.NotEqual(ReadinessBadge.For(10).CssClass, ReadinessBadge.For(2).CssClass);
    }
}
