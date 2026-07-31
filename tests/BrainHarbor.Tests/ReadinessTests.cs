using BrainHarbor.Pipeline.Summarize;

namespace BrainHarbor.Tests;

/// <summary>
/// The readiness score's safety property: the model proposes, but the research
/// stage caps. The load-bearing case is the dangerous one — lab and animal work
/// can never read as near-the-clinic, no matter what the model returns. Erring
/// LOW is safe for this audience, so the clamp only ever lowers a score.
/// </summary>
public class ReadinessTests
{
    [Theory]
    [InlineData("preclinical_cell", 2)]
    [InlineData("preclinical_animal", 2)]
    [InlineData("observational", 5)]
    [InlineData("review_guideline", 6)]
    [InlineData("human_trial", 8)]
    [InlineData("news_other", 10)]
    public void EachStageHasItsDocumentedCeiling(string stage, int ceiling)
    {
        Assert.Equal(ceiling, Readiness.CeilingFor(stage));
    }

    [Fact]
    public void AnUnknownOrMissingStageGetsTheConservativeDefaultCeiling()
    {
        // "we're not sure how far along this is" must never read as "nearly here".
        Assert.Equal(5, Readiness.CeilingFor(null));
        Assert.Equal(5, Readiness.CeilingFor("something_new_we_dont_map"));
    }

    [Fact]
    public void AMouseStudyCannotReadAsNearTheClinic()
    {
        // The whole point: even if the model is over-optimistic and says 9,
        // an animal study is capped to 2.
        var score = Readiness.Clamp(9, "preclinical_animal", out var capped);

        Assert.Equal(2, score);
        Assert.True(capped);
    }

    [Fact]
    public void AScoreWithinTheStageCeilingIsLeftAlone()
    {
        var score = Readiness.Clamp(6, "human_trial", out var capped);

        Assert.Equal(6, score);
        Assert.False(capped);
    }

    [Fact]
    public void TheClampOnlyEverLowersNeverRaises()
    {
        // A conservative model that under-scores its stage is honored — erring
        // low is the safe direction, so the ceiling never pulls a score up.
        var score = Readiness.Clamp(3, "human_trial", out var capped);

        Assert.Equal(3, score);
        Assert.False(capped);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void AZeroOrNegativeScoreFloorsToOne(int proposed)
    {
        Assert.Equal(1, Readiness.Clamp(proposed, "human_trial"));
    }

    [Fact]
    public void AnAbsurdlyHighScoreCannotEscapeTheTopOfTheScale()
    {
        // news_other allows 10; a model returning 99 still can't exceed 10.
        Assert.Equal(10, Readiness.Clamp(99, "news_other"));
    }
}
