using BrainHarbor.Safety;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-304: the automated safety checks that gate auto-publish. These are what
/// make Auto mode safe, so the tests pin the failure cases hardest — a
/// hallucinated number, a hype word, or too-hard prose must all flag.
/// </summary>
public class GuardrailsTests
{
    private const string Source =
        "In a trial of 331 people with grade 2 glioma and an IDH change, " +
        "progression-free survival was 27.7 months versus 11.1 months. 45% responded.";

    // ---------- numeral post-check ----------

    [Fact]
    public void EveryNumberInTheSummaryTracesToTheSource()
    {
        var summary = "The trial had 331 people. Survival was 27.7 months versus 11.1 months.";

        Assert.Empty(Guardrails.UntraceableNumbers(summary, Source));
    }

    [Fact]
    public void AHallucinatedNumberIsCaught()
    {
        // 62% appears nowhere in the source — the classic invented statistic.
        var summary = "The pill helped 62% of patients live longer.";

        var untraceable = Guardrails.UntraceableNumbers(summary, Source);

        Assert.Contains("62", untraceable);
    }

    [Fact]
    public void ThousandsSeparatorsAndSentenceEndDotsDoNotCauseFalsePositives()
    {
        var source = "A cohort of 1,383 patients; median age 63.4 years.";
        var summary = "The study followed 1383 patients. The median age was 63.4.";

        Assert.Empty(Guardrails.UntraceableNumbers(summary, source));
    }

    // ---------- banned phrases ----------

    [Theory]
    [InlineData("This breakthrough changes everything.", "breakthrough")]
    [InlineData("A miracle drug for glioma.", "miracle")]
    [InlineData("Finally, a cure for brain tumors.", "cure")]
    [InlineData("This game-changer is here.", "game-changer")]
    public void HypeWordsAreCaught(string text, string expected)
    {
        Assert.Contains(expected, Guardrails.BannedWordsIn(text));
    }

    [Theory]
    [InlineData("This is not a cure.")]
    [InlineData("The pill does not cure the tumor.")]
    [InlineData("It is not a cure and does not work for everyone.")]
    // The natural anti-hype phrasings that a fixed 4-word window wrongly flagged
    // (found running the pipeline on real abstracts — most items were held):
    [InlineData("This does not mean it is a cure.")]
    [InlineData("It is not a promise of a cure.")]
    [InlineData("This is early research and does not mean doctors have found a cure.")]
    public void ANegatedCureIsAllowed(string text)
    {
        // The anti-hype block is SUPPOSED to say "not a cure" — that must pass,
        // or every guideline-following summary gets held (turning Auto into Review).
        Assert.DoesNotContain("cure", Guardrails.BannedWordsIn(text));
    }

    [Fact]
    public void ANegationInAPriorSentenceDoesNotExcuseAFreshCureClaim()
    {
        // Sentence-scoped: "not" belongs to the first sentence; the second makes
        // a real affirmative cure claim and must still flag.
        Assert.Contains("cure", Guardrails.BannedWordsIn("This is not easy. It is a cure for glioma."));
    }

    [Fact]
    public void ABareCureClaimIsStillFlagged()
    {
        Assert.Contains("cure", Guardrails.BannedWordsIn("Doctors found a cure for glioma."));
    }

    // ---------- spelled-out numbers ----------

    [Fact]
    public void ASpelledNumberInTheSourceMatchesADigitInTheSummary()
    {
        // Source spells "Ten studies"; summary uses "10". Not a hallucination.
        Assert.Empty(Guardrails.UntraceableNumbers("The review pooled 10 studies.", "Ten studies were included."));
    }

    [Fact]
    public void CommonSpelledWordsLikeOneAreNotTreatedAsHallucinatedNumbers()
    {
        // "one" is far more often an article than a count — flagging it is
        // noise. Only digit hallucinations are caught on the summary side.
        Assert.Empty(Guardrails.UntraceableNumbers(
            "The scan found tumors in one group of people.", "The study enrolled patients with glioma."));
    }

    // ---------- medical vocabulary allowance ----------

    [Fact]
    public void RequiredMedicalTermsDoNotPushPlainProseOverTheCeiling()
    {
        // Short sentences that must name the drug and tumor should still pass.
        var plain = "Bevacizumab may help. It slows blood vessel growth in glioblastoma. Some people had side effects.";

        Assert.True(Guardrails.GradeLevel(plain) <= Guardrails.MaxGradeLevel,
            $"grade was {Guardrails.GradeLevel(plain):0.0}");
    }

    [Fact]
    public void PlainCalmProseHasNoBannedWords()
    {
        Assert.Empty(Guardrails.BannedWordsIn(
            "The pill slowed tumor growth in a large study. It is not for everyone."));
    }

    // ---------- reading level ----------

    [Fact]
    public void SimpleShortSentencesReadBelowTheCeiling()
    {
        var plain = "The pill slowed the tumor. People took it each day. Side effects were mild.";

        Assert.True(Guardrails.GradeLevel(plain) <= Guardrails.MaxGradeLevel,
            $"grade was {Guardrails.GradeLevel(plain):0.0}");
    }

    /// <summary>
    /// WI-415: the summary arrives as a plain title plus template blocks joined
    /// by newlines, and a title has no full stop. Grading that as one run made
    /// the title and the hook a single long sentence and inflated every score —
    /// measured at 0.7 of a grade across the 1,038 published summaries. The
    /// blocks must be graded as the separate sentences they are.
    /// </summary>
    [Fact]
    public void ATitleIsNotRunIntoTheHookBelowIt()
    {
        const string Title = "A pill slowed tumor growth";
        const string Hook = "People with this gene change went longer before their tumor grew.";

        var blocks = Guardrails.GradeLevel($"{Title}\n{Hook}");

        // Supplying the missing stop must not change the verdict: the grader
        // is expected to do it itself.
        Assert.Equal(Guardrails.GradeLevel($"{Title}.\n{Hook}"), blocks, 3);

        // And it must genuinely treat them as two sentences — merely stripping
        // terminators would also satisfy the assertion above.
        Assert.True(
            blocks < Guardrails.GradeLevel($"{Title} {Hook}"),
            "two blocks must grade easier than the same words as one run-on sentence");
    }

    [Fact]
    public void WindowsLineEndingsSplitBlocksToo()
    {
        // The repo checks out CRLF; a stray \r would ride along on the last
        // word of every block.
        Assert.Equal(
            Guardrails.GradeLevel("The pill slowed the tumor.\nSide effects were mild."),
            Guardrails.GradeLevel("The pill slowed the tumor.\r\nSide effects were mild."),
            3);
    }

    [Theory]
    [InlineData("The pill slowed the tumor. People took it each day.", false)]
    [InlineData(
        "Investigators subsequently determined that stratification necessitated " +
        "additional multivariable adjustment procedures across heterogeneous cohorts.",
        true)]
    public void TheCeilingFlagsDenseProseAndPassesPlainProse(string text, bool shouldFlag)
    {
        Assert.Equal(shouldFlag, Guardrails.GradeLevel(text) > Guardrails.MaxGradeLevel);
    }

    /// <summary>
    /// The same block-boundary rule as the grader (WI-415): a negation in the
    /// plain title must not excuse a hype claim in the block below it.
    /// </summary>
    [Fact]
    public void ANegationInTheTitleDoesNotExcuseCureInTheHook()
    {
        var banned = Guardrails.BannedWordsIn("This drug is not a cure\nDoctors are calling it a cure.");

        Assert.Contains("cure", banned);
    }

    [Fact]
    public void BlankLinesBetweenBlocksAreNotCountedAsSentences()
    {
        var withBlanks = Guardrails.GradeLevel("The pill slowed the tumor.\n\n\nSide effects were mild.");
        var without = Guardrails.GradeLevel("The pill slowed the tumor.\nSide effects were mild.");

        Assert.Equal(without, withBlanks, 3);
    }

    [Fact]
    public void DenseAcademicProseReadsAboveTheCeiling()
    {
        var dense =
            "Notwithstanding the heterogeneous methodological considerations, the multifactorial " +
            "immunomodulatory microenvironmental reconfiguration demonstrated statistically significant " +
            "amelioration of progression-free survival among the intervention cohort participants.";

        Assert.True(Guardrails.GradeLevel(dense) > Guardrails.MaxGradeLevel,
            $"grade was {Guardrails.GradeLevel(dense):0.0}");
    }

    // ---------- the combined check ----------

    [Fact]
    public void ACleanSummaryPasses()
    {
        var summary = "A pill slowed tumor growth. In the study, 331 people took it. It is not a promise.";
        var result = Guardrails.Check(summary, Source + " It is not a promise.");

        // "promise" fine; numbers trace; short sentences.
        Assert.True(result.Passed, string.Join("; ", result.Reasons));
    }

    [Fact]
    public void AFlaggedSummaryReportsEveryReason()
    {
        var summary = "This breakthrough cured 99% of patients.";
        var result = Guardrails.Check(summary, Source);

        Assert.False(result.Passed);
        Assert.Contains(result.Reasons, r => r.Message.Contains("99"));            // untraceable number
        Assert.Contains(result.Reasons, r => r.Message.Contains("breakthrough")); // hype
    }

    /// <summary>
    /// WI-417: each reason carries WHICH check tripped, so a run can be counted
    /// by cause. The database stores only a summary_flagged boolean, so "4.8%
    /// were flagged" was answerable and "flagged for what" was not.
    /// </summary>
    [Fact]
    public void EveryReasonSaysWhichCheckTrippedNotJustThatOneDid()
    {
        var result = Guardrails.Check("This breakthrough cured 99% of patients.", Source);

        Assert.Contains(Guardrails.FlagKind.InventedNumbers, result.Reasons.Select(r => r.Kind));
        Assert.Contains(Guardrails.FlagKind.BannedHype, result.Reasons.Select(r => r.Kind));
        Assert.DoesNotContain(Guardrails.FlagKind.ReadingLevel, result.Reasons.Select(r => r.Kind));

        // The kind is for counting; the message still reads for a human.
        Assert.All(result.Reasons, r => Assert.False(string.IsNullOrWhiteSpace(r.Message)));
    }

    [Fact]
    public void ATooHardSummaryIsFlaggedAsAReadingLevelProblemSpecifically()
    {
        var dense =
            "Multimodal immunomodulatory microenvironmental reconfiguration demonstrated " +
            "statistically significant amelioration of progression-free survival among the " +
            "intervention cohort participants receiving concomitant chemoradiotherapy.";

        var result = Guardrails.Check(dense, dense);

        Assert.False(result.Passed);
        var reason = Assert.Single(result.Reasons);
        Assert.Equal(Guardrails.FlagKind.ReadingLevel, reason.Kind);
    }
}
