using BrainHarbor.ContentCheck;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-106 + WI-414: the readability gate. Grade thresholds are the promise
/// from content-pipeline.md §5 — fail &gt; 6.0, warn ≥ 5.5 — and since WI-414
/// they cover the Razor pages people actually land on, not just the curated
/// Markdown.
/// </summary>
public class ContentCheckTests
{
    private static readonly DateOnly Today = new(2026, 7, 19);

    // ---------- Flesch-Kincaid against known-grade samples ----------

    [Fact]
    public void SimpleTextScoresLow()
    {
        // Short words, short sentences — early-grade text.
        var grade = ReadabilityAnalyzer.FleschKincaidGrade(
            "The cat sat on the mat. The dog ran to the park. We like to play.");

        Assert.True(grade < 4, $"expected < 4, got {grade}");
    }

    [Fact]
    public void PlainLanguageMedicalTextPassesTheGate()
    {
        var grade = ReadabilityAnalyzer.FleschKincaidGrade(
            "A glioma is a tumor that starts in the brain. Doctors grade it " +
            "from 1 to 4. The grade tells you how fast it tends to grow. " +
            "Your care team will explain what your grade means.");

        Assert.True(grade <= 8.5, $"expected <= 8.5, got {grade}");
    }

    [Fact]
    public void AcademicTextScoresHigh()
    {
        var grade = ReadabilityAnalyzer.FleschKincaidGrade(
            "Notwithstanding contemporary advancements in neuro-oncological " +
            "therapeutics, the prognostic implications of isocitrate dehydrogenase " +
            "mutations necessitate comprehensive multidisciplinary evaluation " +
            "incorporating histopathological and molecular characterization.");

        Assert.True(grade > 12, $"expected > 12, got {grade}");
    }

    [Fact]
    public void HarderTextScoresHigherThanSimplerText()
    {
        var simple = ReadabilityAnalyzer.FleschKincaidGrade(
            "We read the news each day. Then we write it in plain words.");
        var harder = ReadabilityAnalyzer.FleschKincaidGrade(
            "Subsequently, the organization disseminates carefully synthesized " +
            "summaries incorporating contemporaneous oncological developments.");

        Assert.True(harder > simple);
    }

    [Theory]
    [InlineData("cat", 1)]
    [InlineData("tumor", 2)]
    [InlineData("glioma", 3)]
    [InlineData("change", 1)]   // silent e
    [InlineData("little", 2)]   // -le keeps its syllable
    [InlineData("radiation", 4)]
    public void SyllableHeuristicHandlesCommonShapes(string word, int expected)
    {
        Assert.Equal(expected, ReadabilityAnalyzer.CountSyllables(word));
    }

    // ---------- page checks ----------

    private static string PageWith(string body, string extraFrontMatter = "") => $"""
        ---
        title: Test page
        sources:
          - url: https://example.org
            title: Example
        {extraFrontMatter}
        ---

        {body}
        """;

    [Fact]
    public void SimplePagePassesWithInfoFinding()
    {
        var findings = ContentChecker.CheckPage(
            PageWith("We read the news. Then we explain it in plain words."), "about.md", Today);

        Assert.DoesNotContain(findings, f => f.Level == FindingLevel.Fail);
        Assert.Contains(findings, f => f.Level == FindingLevel.Info);
    }

    [Fact]
    public void ComplexPageFailsTheGate()
    {
        var findings = ContentChecker.CheckPage(
            PageWith("Notwithstanding contemporary neuro-oncological advancements, " +
                     "comprehensive multidisciplinary prognostication necessitates " +
                     "extraordinarily sophisticated histopathological characterization " +
                     "incorporating unprecedented methodological considerations."),
            "bad.md", Today);

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail && f.Message.Contains("reading grade"));
    }

    [Fact]
    public void MalformedFrontMatterFails()
    {
        var findings = ContentChecker.CheckPage("No front matter here.", "broken.md", Today);

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail);
    }

    [Fact]
    public void MissingSourcesWarns()
    {
        var findings = ContentChecker.CheckPage(
            "---\ntitle: No sources\n---\nShort and plain words here.", "nosrc.md", Today);

        Assert.Contains(findings, f => f.Level == FindingLevel.Warn && f.Message.Contains("sources"));
    }

    [Fact]
    public void OverdueReviewWarns()
    {
        var findings = ContentChecker.CheckPage(
            PageWith("Plain words here.", "review_due: 2026-01-01"), "stale.md", Today);

        Assert.Contains(findings, f => f.Level == FindingLevel.Warn && f.Message.Contains("overdue"));
    }

    [Fact]
    public void FutureReviewDueDoesNotWarn()
    {
        var findings = ContentChecker.CheckPage(
            PageWith("Plain words here.", "review_due: 2027-01-01"), "fresh.md", Today);

        Assert.DoesNotContain(findings, f => f.Message.Contains("overdue"));
    }

    // ---------- glossary checks ----------

    [Fact]
    public void MidBandGradeWarnsWithoutFailing()
    {
        // A sample inside the 5.5–6.0 warn band (WI-414 lowered the gate to
        // 6th grade; this sample was retuned from the old 7.5–8.5 band).
        var text = "The nurse called the family about the visit. " +
                   "She answered their questions about the plan. " +
                   "They felt better after they talked.";
        var grade = ReadabilityAnalyzer.FleschKincaidGrade(text);
        Assert.True(grade >= ContentChecker.WarnGrade && grade <= ContentChecker.FailGrade,
            $"sample must sit in the warn band, got {grade}");

        var findings = ContentChecker.CheckPage(PageWith(text), "warn.md", Today);

        Assert.Contains(findings, f => f.Level == FindingLevel.Warn && f.Message.Contains("close to"));
        Assert.DoesNotContain(findings, f => f.Level == FindingLevel.Fail);
    }

    [Fact]
    public void HeadingsAndBulletsDoNotInflateTheGrade()
    {
        // Structure helps impaired readers — the gate must not punish it.
        var structured = "## Treatment options\n\n- Surgery is one option.\n- " +
                         "Radiation is another option.\n\nYour care team will explain each one.";
        var flat = "Treatment options. Surgery is one option. Radiation is " +
                   "another option. Your care team will explain each one.";

        var structuredGrade = ReadabilityAnalyzer.FleschKincaidGrade(
            ContentChecker.ExtractSentences(structured));
        var flatGrade = ReadabilityAnalyzer.FleschKincaidGrade(flat);

        Assert.True(Math.Abs(structuredGrade - flatGrade) < 0.5,
            $"structured {structuredGrade} vs flat {flatGrade} — structure must not change the grade");
    }

    [Fact]
    public void MalformedGlossaryFrontMatterFails()
    {
        var findings = ContentChecker.CheckGlossaryTerm("No front matter.", "broken");

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail);
    }

    [Fact]
    public void MissingPagesRootWarnsLoudly()
    {
        var findings = ContentChecker.CheckAll(
            Path.Combine(Path.GetTempPath(), "bh-does-not-exist-" + Guid.NewGuid().ToString("N")),
            null, Today);

        Assert.Contains(findings, f => f.Level == FindingLevel.Warn && f.Message.Contains("MISSING"));
    }

    [Fact]
    public void GlossaryDefinitionOver40WordsFails()
    {
        var longDefinition = string.Join(' ', Enumerable.Repeat("word", 45));
        var findings = ContentChecker.CheckGlossaryTerm(
            $"---\nterm: x\n---\n{longDefinition}", "x");

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail && f.Message.Contains("40"));
    }

    [Fact]
    public void ShippedGlossaryTermsPassTheirOwnGate()
    {
        var glossaryRoot = Path.Combine(FindRepoRoot(), "src", "BrainHarbor.Web", "Content", "glossary");
        var findings = ContentChecker.CheckAll(
            Path.Combine(FindRepoRoot(), "no-pages"), glossaryRoot, Today);

        Assert.NotEmpty(findings);
        Assert.DoesNotContain(findings, f => f.Level == FindingLevel.Fail);
    }

    // ---------- WI-575: the description ratchet ----------

    /// <summary>
    /// A synthetic findings list, shaped the way <c>CheckAll</c> produces one.
    ///
    /// <para>Synthetic on purpose: the branches that matter cannot be reached from the
    /// real corpus without editing it, and a test that edits the corpus to check a
    /// counter is a test that can leave the corpus edited.</para>
    /// </summary>
    private static List<Finding> Corpus(int overTheLimit, int under, double worst,
                                        int tooShort = 0)
    {
        var findings = new List<Finding>();
        var total = overTheLimit + under + tooShort;

        for (var i = 0; i < total; i++)
        {
            // The BODY grade, built the way `GradeFinding` really builds it: the
            // three-argument constructor, so `Grade` is NULL. The first version of
            // this builder set it to 4.0 — a shape production cannot emit — and that
            // is why all seven of these tests stayed green while the ratchet was
            // entirely dead on the real corpus (/review round 2). **A test builder
            // that does not match its producer tests the builder.**
            findings.Add(new(FindingLevel.Info, $"p{i}.md", "reading grade 4.0"));
        }

        for (var i = 0; i < overTheLimit; i++)
        {
            // DERIVED FROM `worst`, NOT A LITERAL. This was `FailGrade + 1.0` — a hard
            // 7.0 — which works only while the recorded ceiling is above 7.0. WI-578's
            // declared end state is every description at or under 6.0, so the ceiling
            // ratchets down THROUGH 7.0 and every parity test in this block would then
            // fail because the FILLER breached the ceiling, for a reason unrelated to
            // what any of them tests. Halfway between the limit and the worst is over
            // the limit and under the ceiling for any legal pair (/review, WI-578).
            var grade = i == 0
                ? worst
                : ContentChecker.FailGrade + ((worst - ContentChecker.FailGrade) / 2);
            findings.Add(new(FindingLevel.Warn, $"p{i}.md [description]",
                $"reading grade {grade:0.0} (not gated — WI-578)", grade));
        }

        for (var i = 0; i < under; i++)
        {
            findings.Add(new(FindingLevel.Info, $"p{overTheLimit + i}.md [description]",
                "reading grade 4.0 (not gated — WI-578)", 4.0));
        }

        for (var i = 0; i < tooShort; i++)
        {
            findings.Add(new(FindingLevel.Info,
                $"p{overTheLimit + under + i}.md [description]", "8 word(s) — too little to grade"));
        }

        return findings;
    }

    /// <summary>
    /// AT PARITY THE RATCHET SAYS SO, and that is /review round 3's blocker.
    ///
    /// <para>It used to contribute the empty set when nothing had changed — so the call
    /// site in <c>CheckAll</c> was unobservable, and the end-to-end test written to prove
    /// the wiring was satisfied by <c>GradeDescription</c> alone. Deleting the call left
    /// the whole suite green and the tool's output byte-identical. <b>A gate that is
    /// silent when it passes cannot be proved to be plugged in.</b></para>
    /// </summary>
    [Fact]
    public void AtParityTheRatchetReportsThatBothNumbersAreUnchanged()
    {
        var findings = ContentChecker.DescriptionRatchet(
            Corpus(ContentChecker.DescriptionsOverTheLimit, 8,
                   ContentChecker.WorstDescriptionGrade),
            ContentChecker.CorpusWhenMeasured).ToList();

        var info = Assert.Single(findings);
        Assert.Equal(FindingLevel.Info, info.Level);
        Assert.Equal("(corpus)", info.File);
        Assert.Contains("both unchanged", info.Message);
    }

    [Fact]
    public void OneMoreDescriptionOverTheLimitFailsTheBuild()
    {
        var findings = ContentChecker.DescriptionRatchet(
            Corpus(ContentChecker.DescriptionsOverTheLimit + 1, 8,
                   ContentChecker.WorstDescriptionGrade),
            ContentChecker.CorpusWhenMeasured).ToList();

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail
                                       && f.Message.Contains("above the"));
    }

    /// <summary>
    /// A description getting WORSE fails, which a count cannot see. /review round 1:
    /// push any of the 41 from 8.4 to 40.0 and the count does not move.
    /// </summary>
    [Fact]
    public void ADescriptionGettingWorseFailsEvenWhenTheCountIsUnchanged()
    {
        var findings = ContentChecker.DescriptionRatchet(
            Corpus(ContentChecker.DescriptionsOverTheLimit, 8,
                   ContentChecker.WorstDescriptionGrade + 0.1),
            ContentChecker.CorpusWhenMeasured).ToList();

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail
                                       && f.Message.Contains("now grades"));
    }

    /// <summary>
    /// A drop is reported WITH the too-short count beside it, because a description
    /// that falls under the word floor stops being graded rather than getting better
    /// and would otherwise read as a gain.
    /// </summary>
    [Fact]
    public void ADropIsReportedAndNamesTheTooShortCount()
    {
        var findings = ContentChecker.DescriptionRatchet(
            Corpus(ContentChecker.DescriptionsOverTheLimit - 1, 8,
                   ContentChecker.WorstDescriptionGrade, tooShort: 3),
            ContentChecker.CorpusWhenMeasured).ToList();

        var info = Assert.Single(findings);
        Assert.Equal(FindingLevel.Info, info.Level);
        Assert.Contains("down from", info.Message);
        Assert.Contains("3 are under", info.Message);
    }

    /// <summary>The first finished slice, used to build probe paths where one directory
    /// is enough (the out-of-slice controls pin <c>tumors/</c> by name). Index rather than
    /// <c>Assert.Single</c>: the list GROWS by one entry per slice, and a single-element
    /// assertion would have failed the moment <c>tests/</c> landed — reading like the gate
    /// broke rather than like a test helper was too clever (/review, WI-578). It has
    /// two entries now, which is why the Fail probes below iterate
    /// <see cref="ContentChecker.DescriptionsCleanDirectories"/> instead of using this.
    /// The non-empty claim is asserted where it belongs, in
    /// <see cref="EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus"/>.</summary>
    private static string FinishedSlice => ContentChecker.DescriptionsCleanDirectories[0];

    /// <summary>
    /// WI-578 slice 2: <b>EVERY LISTED SLICE, NOT THE FIRST ONE.</b> The probes below ran
    /// over <see cref="FinishedSlice"/> when the list had exactly one entry, and the
    /// moment it had two, "the gate fails an over-limit description inside a finished
    /// slice" was being proved for <c>treatments/</c> and assumed for <c>tests/</c>.
    ///
    /// <para><b>And the shipped corpus cannot cover that gap, because a directory that
    /// PASSES looks identical gated or not.</b> Narrow the matcher to
    /// <c>DescriptionsCleanDirectories[0]</c> and every description under <c>tests/</c>
    /// still grades 5.2 or better, so
    /// <see cref="EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus"/>'s grade
    /// assertions stay green with the gate switched off for half the list. §12.19 asked
    /// which pages a guard can fire on; this asks which ENTRIES it can fire on.</para>
    /// </summary>
    public static TheoryData<string> FinishedSlices
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var dir in ContentChecker.DescriptionsCleanDirectories)
            {
                data.Add(dir);
            }

            return data;
        }
    }

    /// <summary>A description that grades well above 6.0 for the one reason WI-578 is
    /// about — a single comma-spliced sentence — built once so the in-slice and
    /// out-of-slice probes differ by their PATH and nothing else.</summary>
    private const string OverTheLimitDescription =
        "description: \"Why you were put on one, why somebody who has never had a "
        + "seizure is usually not given one, what the drug can do to mood and temper and "
        + "how to tell that from the tumor, why the dose is never yours to change, and "
        + "what to ask about driving.\"";

    /// <summary>
    /// WI-578: <b>A DESCRIPTION OVER THE LIMIT INSIDE A FINISHED SLICE FAILS THE BUILD.</b>
    /// Everywhere else it is the known backlog and warns; in a directory WI-578 has
    /// rewritten, it is a regression.
    ///
    /// <para>Driven through the REAL <c>CheckPage</c> rather than a synthetic findings
    /// list, which is WI-575's round-2 lesson obeyed: seven tests stayed green over a
    /// dead ratchet because each built an input shape the producer never emits. The only
    /// thing this test hand-builds is a page.</para>
    ///
    /// <para>Run for EVERY entry in <see cref="ContentChecker.DescriptionsCleanDirectories"/>
    /// — see <see cref="FinishedSlices"/> for why the first one is not enough and why the
    /// corpus test cannot make up the difference.</para>
    /// </summary>
    [Theory]
    [MemberData(nameof(FinishedSlices))]
    public void ADescriptionOverTheLimitInsideAFinishedSliceFails(string slice)
    {
        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     OverTheLimitDescription),
            slice + "probe.md", Today);

        var fail = Assert.Single(findings, f =>
            f.Level == FindingLevel.Fail
            && f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));
        Assert.Contains("REGRESSION", fail.Message, StringComparison.Ordinal);

        // AND IT NEVER CLAIMS THE OPPOSITE OF ITS OWN LEVEL. The first version left
        // "(not gated — WI-578)" on the message while raising the level to Fail, so the
        // log carried both stories about the same page (/review, WI-578).
        Assert.DoesNotContain("not gated", fail.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// And the SAME page under <c>tumors/</c> only warns — so the test above pins the
    /// DIRECTORY and not merely the presence of an over-the-limit description. §12.19: a
    /// guard proved only by the case that fires has not been shown to discriminate.
    ///
    /// <para>A real sibling directory, not a path with no directory at all: that is what
    /// makes this a control. A matcher written with <c>Contains</c>, or one comparing
    /// case-insensitively, would pass a directory-less probe and fail this one.</para>
    /// </summary>
    [Fact]
    public void TheSameDescriptionOutsideAFinishedSliceIsOnlyTheKnownBacklog()
    {
        Assert.DoesNotContain("tumors/", ContentChecker.DescriptionsCleanDirectories);

        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     OverTheLimitDescription),
            "tumors/probe.md", Today);

        var description = Assert.Single(findings, f =>
            f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));
        Assert.Equal(FindingLevel.Warn, description.Level);
        Assert.Contains(ContentChecker.NotGatedMarker, description.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// WI-578, and <b>THE HOLE /review FOUND IN THE FIRST VERSION OF THE SLICE GATE.</b>
    ///
    /// <para>The gate lived in <see cref="ContentChecker.DescriptionRatchet"/>, which
    /// only ever sees GRADED descriptions. Truncating one under the 25-word floor takes
    /// it out of the graded set entirely: the slice gate cannot see it,
    /// <c>DescriptionsOverTheLimit</c> goes DOWN by one and reads as progress, the
    /// ceiling is untouched, and the too-short tally rises inside an Info that
    /// <c>Program.cs</c> renders as <c>ok</c>. <b>The whole slice was unwindable by
    /// truncation with every gate green and the tool exiting 0.</b></para>
    ///
    /// <para>Which is the escape hatch <see cref="ContentChecker.GradeDescription"/>'s
    /// own comment had warned about since WI-575 — <i>scoring it as progress would let a
    /// page buy its way out of grading by getting shorter</i> — while the gate sat on the
    /// wrong side of it. SPLIT, DO NOT SHORTEN is a rule the tool enforces now.</para>
    ///
    /// <para><b>Slice 2 is where this stops being hypothetical.</b> Five of its eight
    /// descriptions were a single sentence of 28 to 35 words against a 25-word floor, so
    /// a "fix" that dropped three words would have taken the page out of grading
    /// altogether. The split script refuses a shortening and this is the tool-side half
    /// of the same rule.</para>
    /// </summary>
    [Theory]
    [MemberData(nameof(FinishedSlices))]
    public void TruncatingADescriptionInsideAFinishedSliceFailsInsteadOfGoingUngraded(
        string slice)
    {
        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     "description: \"Why you were put on one.\""),
            slice + "probe.md", Today);

        var fail = Assert.Single(findings, f =>
            f.Level == FindingLevel.Fail
            && f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));
        Assert.Contains("too little to grade", fail.Message, StringComparison.Ordinal);
        Assert.Contains("NO LONGER GRADED", fail.Message, StringComparison.Ordinal);

        // IT IS STILL UNGRADED, which is the point: the Fail does not come from a grade.
        Assert.Null(fail.Grade);
    }

    /// <summary>The same truncation OUTSIDE a finished slice stays an Info, because
    /// nothing has claimed that directory yet and the remaining backlog is not gated.
    /// Without this the test above would pass over a gate that failed every short
    /// description in the corpus.</summary>
    [Fact]
    public void TheSameTruncationOutsideAFinishedSliceIsStillOnlyUngraded()
    {
        // The precondition its over-the-limit sibling has, and /review found this one
        // without it: when the last slice lands and `tumors/` joins the list, the
        // assertions below start failing with a puzzling message instead of this one.
        Assert.DoesNotContain("tumors/", ContentChecker.DescriptionsCleanDirectories);

        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     "description: \"Why you were put on one.\""),
            "tumors/probe.md", Today);

        var description = Assert.Single(findings, f =>
            f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));
        Assert.Equal(FindingLevel.Info, description.Level);
        Assert.DoesNotContain("NO LONGER GRADED", description.Message, StringComparison.Ordinal);
    }

    /// <summary>And the third ungraded escape route: deleting the description outright
    /// inside a finished slice. A blank one warns elsewhere (WI-575 /review round 4) and
    /// must fail here, for the same reason a truncated one does. Over every listed slice,
    /// per <see cref="FinishedSlices"/>.</summary>
    [Theory]
    [MemberData(nameof(FinishedSlices))]
    public void ABlankDescriptionInsideAFinishedSliceFails(string slice)
    {
        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more."),
            slice + "probe.md", Today);

        var fail = Assert.Single(findings, f =>
            f.Level == FindingLevel.Fail
            && f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));
        Assert.StartsWith(ContentChecker.NoDescriptionPrefix, fail.Message,
            StringComparison.Ordinal);
        Assert.Contains("REGRESSION", fail.Message, StringComparison.Ordinal);

        // AND IT DOES NOT TELL THE PAGE TO SPLIT A SENTENCE IT DOES NOT HAVE. /review,
        // slice 2: the fix advice was part of the shared suffix, so this branch — a page
        // with NO description — was being told the cause is sentence length and to split
        // rather than shorten. A message a reader of the log acts on is the worst place
        // for advice about a sentence that is not there.
        Assert.DoesNotContain("SPLIT the sentence", fail.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>And the out-of-slice control the blank branch did not have.</b> /review, slice
    /// 2: the over-the-limit branch is paired with
    /// <see cref="TheSameDescriptionOutsideAFinishedSliceIsOnlyTheKnownBacklog"/> and the
    /// truncation branch with
    /// <see cref="TheSameTruncationOutsideAFinishedSliceIsStillOnlyUngraded"/>, but the
    /// blank one was proved only by the case that FIRES — so changing the Warn in
    /// <c>GradeDescription</c> to a Fail left the whole suite green. That is §12.19's own
    /// rule, and it was the one branch of three not holding to it.
    ///
    /// <para>The out-of-slice level is a <b>Warn</b> rather than an Info on purpose
    /// (WI-575 /review round 4): a page with no description has no opening paragraph and
    /// no search blurb, which is a defect everywhere, not merely ungated backlog.</para>
    /// </summary>
    [Fact]
    public void TheSameBlankDescriptionOutsideAFinishedSliceOnlyWarns()
    {
        Assert.DoesNotContain("tumors/", ContentChecker.DescriptionsCleanDirectories);

        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more."),
            "tumors/probe.md", Today);

        var description = Assert.Single(findings, f =>
            f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));
        Assert.Equal(FindingLevel.Warn, description.Level);
        Assert.StartsWith(ContentChecker.NoDescriptionPrefix, description.Message,
            StringComparison.Ordinal);
        Assert.DoesNotContain("REGRESSION", description.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// WI-578: when a finished slice has failed, the ratchet reports the corpus totals
    /// <b>WITHOUT A VERDICT</b> rather than suppressing them.
    ///
    /// <para>Two things are being pinned at once. The parity Info must NOT print beside a
    /// regression — a swap holds the count at parity, so "both unchanged" would sit next
    /// to a Fail naming the page that just broke, which is §12.22 round 4's
    /// two-opposite-stories defect. But suppressing it outright threw away
    /// <c>tooShort</c> on exactly the run where somebody is diagnosing a truncation
    /// (/review, WI-578), and <c>tooShort</c> is the instrument for that. Facts with no
    /// verdict attached cannot contradict the Fail.</para>
    /// </summary>
    [Fact]
    public void WhenAFinishedSliceHasFailedTheCorpusTotalsAreReportedWithoutAVerdict()
    {
        // A FAILING description finding, shaped exactly as GradeDescription now emits
        // one for a page inside a finished slice: Fail level, marker suffix, no grade
        // (the truncation case). The ratchet reads the LEVEL, so this is the shape it
        // actually keys on rather than one invented for the test.
        var findings = Corpus(ContentChecker.DescriptionsOverTheLimit, 8,
                              ContentChecker.WorstDescriptionGrade, tooShort: 3);
        findings.Add(new(FindingLevel.Fail,
            FinishedSlice + "regressed.md" + ContentChecker.DescriptionMarker,
            "12 word(s) — too little to grade, and 25 are needed."));

        var result = ContentChecker.DescriptionRatchet(
            findings, ContentChecker.CorpusWhenMeasured).ToList();

        var corpus = Assert.Single(result);
        Assert.Equal(FindingLevel.Warn, corpus.Level);
        Assert.Equal("(corpus)", corpus.File);

        // THE VERDICTS ARE GONE...
        Assert.DoesNotContain("both unchanged", corpus.Message);
        Assert.DoesNotContain("down from", corpus.Message);

        // ...AND THE FACTS ARE NOT, including the one that diagnoses a truncation.
        //
        // FOUR, NOT THREE, and getting that wrong first time is the reason it is worth
        // asserting exactly: the regressed page added above IS a too-short description
        // (ungraded, not blank), so it is counted among the facts as well as named in
        // the failure. 3 from the builder + itself. A tally that silently omitted the
        // page that just broke would be the wrong number in the one message somebody
        // reads while diagnosing it.
        Assert.Contains("4 under the", corpus.Message);
        Assert.Contains("floor count RISING", corpus.Message);
    }

    /// <summary>
    /// PAGES WALKED AND NO DESCRIPTION GRADED IS LOUD — the silent-death path. Every
    /// way this gate could stop measuring used to land in the "improved" branch and
    /// report a clean bill.
    /// </summary>
    [Fact]
    public void PagesWalkedWithNoDescriptionGradedIsAFailure()
    {
        // A CORPUS AS LARGE AS THE MEASUREMENT, because the ratchet now declines to
        // speak about anything smaller — a 3-page fixture is not the shipped corpus
        // (/review round 3).
        var findings = ContentChecker.DescriptionRatchet(
            [.. Enumerable.Range(0, ContentChecker.CorpusWhenMeasured)
                .Select(i => new Finding(FindingLevel.Info, $"p{i}.md", "reading grade 4.0"))],
            ContentChecker.CorpusWhenMeasured).ToList();

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail
                                       && f.Message.Contains("NOT ONE description"));
    }

    /// <summary>
    /// A CORPUS TOO SMALL TO COMPARE IS LOUD, NOT SILENT — /review round 4's blocker.
    ///
    /// <para>The scope test used <see cref="ContentChecker.DescriptionsOverTheLimit"/>
    /// as a page-count floor. That number is a count of bad descriptions, and WI-578 is
    /// instructed to lower it as the work lands: at 3 the description-less fixtures
    /// would start failing, and at 0 the precondition would never fire and a
    /// glossary-only run would fall through to the dead-instrument Fail. Both earlier
    /// blockers, re-armed by following this item's own instructions.</para>
    ///
    /// <para>And declining is reported, because round 3's ruling — a gate that is silent
    /// when it passes cannot be proved to be plugged in — is just as true of a gate
    /// silent when it declines.</para>
    /// </summary>
    [Fact]
    public void ACorpusSmallerThanTheMeasurementIsReportedRatherThanSkippedSilently()
    {
        var findings = ContentChecker.DescriptionRatchet(
            Corpus(1, 1, 7.0), ContentChecker.CorpusWhenMeasured - 1).ToList();

        var warn = Assert.Single(findings);
        Assert.Equal(FindingLevel.Warn, warn.Level);
        Assert.Equal("(corpus)", warn.File);
        Assert.Contains("THE RATCHET DID NOT RUN", warn.Message);
    }

    /// <summary>
    /// AND NO CURATED PAGE AT ALL IS STILL SILENT, which is the other half: a caller
    /// scoping <c>CheckAll</c> to the glossary is not in the ratchet's business.
    /// </summary>
    [Fact]
    public void NoCuratedPageAtAllStaysSilent()
    {
        Assert.Empty(ContentChecker.DescriptionRatchet(Corpus(1, 1, 7.0), 0));
    }

    /// <summary>
    /// EVERY DESCRIPTION TRUNCATED UNDER THE WORD FLOOR IS A FAILURE, NOT A TOTAL WIN.
    ///
    /// <para>/review round 3: with all 55 descriptions under the 25-word floor,
    /// <c>descriptions.Count</c> is still 55 while <c>graded.Count</c> is 0 — so
    /// <c>over</c> becomes 0 and the ratchet reported <i>"0 above the limit, down from
    /// 41"</i> and passed. A dead instrument printed as progress, which is the precise
    /// failure the absent-description check was written to prevent. The check is on
    /// UNGRADED now, not on ABSENT.</para>
    /// </summary>
    [Fact]
    public void EveryDescriptionFallingUnderTheWordFloorIsAFailure()
    {
        var findings = ContentChecker.DescriptionRatchet(
            Corpus(0, 0, 0, tooShort: ContentChecker.CorpusWhenMeasured),
            ContentChecker.CorpusWhenMeasured).ToList();

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail
                                       && f.Message.Contains("NOT ONE description"));
    }

    /// <summary>
    /// AND NO CURATED PAGE IN SCOPE IS SILENT, because a caller may legitimately check
    /// the glossary or the razor pages alone — <see cref="ShippedGlossaryTermsPassTheirOwnGate"/>
    /// does the first, and the first version of the precondition failed it. §12.8: a
    /// guard that fails a correct case is worse than no guard.
    ///
    /// <para>The finding here deliberately CARRIES a grade, so the test turns on the
    /// page count alone. /review round 2 pointed out that the first version's finding
    /// had a null grade, which satisfied the old predicate for a second reason and left
    /// the test passing whatever the path said.</para>
    /// </summary>
    [Fact]
    public void NoCuratedPageInScopeIsNotTheRatchetsBusiness()
    {
        var findings = ContentChecker.DescriptionRatchet(
        [
            new(FindingLevel.Info, "glossary/x.md", "reading grade 4.0 (not gated)", 4.0),
        ], curatedPagesWalked: 0).ToList();

        Assert.Empty(findings);
    }

    /// <summary>
    /// THE GRADE RIDES ON THE RECORD, NOT IN THE MESSAGE — and this test is the reason
    /// the field exists.
    ///
    /// <para>The first version recovered the grade by parsing it back out of the
    /// message, which is formatted with CurrentCulture while the parser used
    /// InvariantCulture. On any comma-decimal locale "reading grade 8,4" matched
    /// nothing, every description scored -1, and the gate reported zero above the limit
    /// and passed. Forever. This asserts the ratchet reads the FIELD, by handing it a
    /// message no parser could read.</para>
    /// </summary>
    [Fact]
    public void TheRatchetReadsTheGradeFieldAndNotTheMessage()
    {
        // A CORPUS AS LARGE AS THE MEASUREMENT: the ratchet declines to speak about
        // anything smaller, because its constants were measured on 55 pages and a
        // one-page fixture is not the shipped corpus (/review round 3).
        List<Finding> findings =
        [
            .. Enumerable.Range(0, ContentChecker.CorpusWhenMeasured)
                .Select(i => new Finding(FindingLevel.Info, $"p{i}.md", "reading grade 4.0")),

            // THE ONE DESCRIPTION, with a message no parser could read and the grade on
            // the FIELD. If the ratchet ever goes back to parsing the message, this is
            // where the locale bug shows.
            new(FindingLevel.Warn, "p0.md" + ContentChecker.DescriptionMarker,
                "reading grade 8,4 — in a locale no parser here understands",
                ContentChecker.WorstDescriptionGrade + 5),
        ];

        var ratchet = ContentChecker.DescriptionRatchet(
            findings, ContentChecker.CorpusWhenMeasured).ToList();

        Assert.Contains(ratchet, f => f.Level == FindingLevel.Fail
                                      && f.Message.Contains("now grades"));
    }

    /// <summary>
    /// THE RATCHET RUNS ON THE REAL CORPUS, and this is the one assertion that would
    /// have caught /review round 2's blocker.
    ///
    /// <para>The seven tests above exercise every branch of
    /// <see cref="ContentChecker.DescriptionRatchet"/> with synthetic findings — and all
    /// seven stayed green for a whole round while the ratchet was dead on the real
    /// corpus, because their body rows carried a <c>Grade</c> that the real producer
    /// never sets. <b>Branch coverage with a hand-built input proves the branches, not
    /// the wiring.</b></para>
    ///
    /// <para>So this one goes through <c>CheckAll</c> over the shipped corpus and asserts
    /// the ratchet was REACHED: every page reports a description finding, and the count
    /// of descriptions over the limit is exactly the recorded constant. If the ratchet
    /// ever stops being called, or stops seeing the descriptions, this reds.</para>
    /// </summary>
    [Fact]
    public void TheDescriptionRatchetActuallyRunsOverTheShippedCorpus()
    {
        var root = FindRepoRoot();
        var pagesRoot = Path.Combine(root, "src", "BrainHarbor.Web", "Content", "pages");
        var findings = ContentChecker.CheckAll(
            pagesRoot,
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "glossary"),
            Today);

        var onDisk = Directory
            .EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories).Count();
        var descriptions = findings
            .Where(f => f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal))
            .ToList();

        Assert.Equal(onDisk, descriptions.Count);

        // THE GRADE IS ON THE RECORD. If it is ever only in the message again, the
        // locale bug is back and this is where it shows.
        Assert.Contains(descriptions, f => f.Grade is not null);

        // AND THE RECORDED CONSTANT IS STILL THE TRUTH. Not a floor: if it drifts in
        // either direction the constant has to move with it, which is the point of a
        // ratchet. Over means the corpus grew the backlog; under means WI-578 made
        // progress and the constant should be lowered to lock it in.
        var over = descriptions.Count(f => f.Grade > ContentChecker.FailGrade);
        Assert.Equal(ContentChecker.DescriptionsOverTheLimit, over);

        // THE CONTRACT, NOT A ROUNDED EQUALITY. The ratchet fails on `>`, so that is
        // what is asserted; `Assert.Equal(double, double, int)` is decimal-place
        // precision with banker's rounding and would pass 19.749 (/review round 3).
        var worst = descriptions.Where(f => f.Grade is not null).Max(f => f.Grade!.Value);
        Assert.True(worst <= ContentChecker.WorstDescriptionGrade,
            $"the worst description grades {worst:0.0} against a recorded ceiling of "
            + $"{ContentChecker.WorstDescriptionGrade:0.0}");

        // AND THE RATCHET WAS ACTUALLY REACHED. This is the assertion /review round 3
        // asked for and the one the first version of this test could not make: at
        // parity the ratchet used to emit nothing, so every other line here was
        // satisfied by GradeDescription alone and the call site in CheckAll could have
        // been deleted without reddening a single test.
        var corpusLine = Assert.Single(findings, f => f.File == "(corpus)");
        Assert.Equal(FindingLevel.Info, corpusLine.Level);
        Assert.Contains("both unchanged", corpusLine.Message);

        // AND NOTHING THE RATCHET PRODUCES IS A FAILURE TODAY, which is what makes the
        // assertions above meaningful rather than a restatement of the corpus.
        Assert.DoesNotContain(findings, f => f.Level == FindingLevel.Fail);
    }

    /// <summary>
    /// WI-578: <b>A FINISHED SLICE IS A CLAIM ABOUT THE CORPUS, so the corpus is what
    /// checks it.</b> <see cref="ContentChecker.DescriptionsCleanDirectories"/> is a
    /// constant, and a constant can name a directory that does not pass, or one that
    /// does not exist — in which case the gate it drives fires on nothing and §12.19
    /// applies: a guard measured on a page it cannot fire on has been measured on
    /// nothing.
    ///
    /// <para>This is the assertion a synthetic findings list cannot make, and it is the
    /// round-2 lesson from WI-575 obeyed rather than re-narrated: the two tests above
    /// build their own inputs, so they prove the branch and say nothing about whether
    /// <c>treatments/</c> actually reads at sixth grade. Only the shipped corpus can say
    /// that.</para>
    /// </summary>
    [Fact]
    public void EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus()
    {
        var root = FindRepoRoot();
        var pagesRoot = Path.Combine(root, "src", "BrainHarbor.Web", "Content", "pages");
        var findings = ContentChecker.CheckAll(
            pagesRoot,
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "glossary"),
            Today);

        var graded = findings
            .Where(f => f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal)
                        && f.Grade is not null)
            .ToList();

        Assert.NotEmpty(ContentChecker.DescriptionsCleanDirectories);

        foreach (var dir in ContentChecker.DescriptionsCleanDirectories)
        {
            var inDirectory = graded
                .Where(f => f.File.StartsWith(dir, StringComparison.Ordinal))
                .ToList();

            // NOT VACUOUS, AND NOT PARTIAL. A typo in the constant ("treatment/") would
            // leave every assertion below iterating an empty list and passing; a count
            // of ">0" would also accept a directory where only SOME descriptions are
            // graded, and an ungraded one is precisely how a page escapes this gate.
            // Counted off disk so the number cannot rot (/review, WI-578).
            var onDisk = Directory
                .EnumerateFiles(Path.Combine(pagesRoot, dir.TrimEnd('/')),
                                "*.md", SearchOption.AllDirectories)
                .Count();

            Assert.True(onDisk > 0,
                $"'{dir}' is listed as a finished slice but there is no such directory "
                + $"under {pagesRoot} — check the spelling against the paths "
                + $"{nameof(ContentChecker)} emits, because a misspelled entry gates "
                + "nothing while looking like it gates a directory");

            Assert.True(onDisk == inDirectory.Count,
                $"'{dir}' holds {onDisk} page(s) on disk but only {inDirectory.Count} "
                + "produced a GRADED description. A page in a finished slice that is not "
                + "graded has left the gate's reach — blank, or under the "
                + $"{25}-word floor — which is the escape hatch this slice gate exists "
                + "to close");

            var bad = inDirectory
                .Where(f => f.Grade > ContentChecker.FailGrade)
                .Select(f => $"{f.File} grades {f.Grade:0.0}")
                .ToList();

            Assert.True(bad.Count == 0,
                $"'{dir}' is listed as a finished slice, so every description under it "
                + $"must grade at or under {ContentChecker.FailGrade:0.0}: "
                + string.Join("; ", bad));
        }
    }

    /// <summary>
    /// WI-578 slice 2: <b>AND THE GATE ACTUALLY REACHES EVERY ENTRY IN THE LIST.</b> The
    /// test above is entirely about GRADES — and a directory that PASSES grades the same
    /// whether it is gated or not. Narrow the matcher in
    /// <see cref="ContentChecker.GradeDescription"/> to
    /// <c>DescriptionsCleanDirectories[0]</c> and every description under <c>tests/</c>
    /// still grades 5.2 or better, so every assertion in it stays green while half the
    /// list is quietly ungated.
    ///
    /// <para><b>The marker is the only observable difference on a clean corpus</b>, which
    /// is what makes this an assertion rather than a nicety. A separate test because it is
    /// a separate claim: <i>these directories read at sixth grade</i> and <i>these
    /// directories are gated</i> fail for different reasons and should not share a
    /// name.</para>
    ///
    /// <para>Asserted as "says gated" rather than "does not say not-gated", so a third
    /// message shape — a reworded parenthetical, or none at all — fails here too instead
    /// of passing a double negative.</para>
    /// </summary>
    [Fact]
    public void EveryFinishedSliceIsActuallyGatedOnTheShippedCorpus()
    {
        var root = FindRepoRoot();
        var findings = ContentChecker.CheckAll(
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "pages"),
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "glossary"),
            Today);

        var graded = findings
            .Where(f => f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal)
                        && f.Grade is not null)
            .ToList();

        Assert.NotEmpty(ContentChecker.DescriptionsCleanDirectories);

        foreach (var dir in ContentChecker.DescriptionsCleanDirectories)
        {
            var inDirectory = graded
                .Where(f => f.File.StartsWith(dir, StringComparison.Ordinal))
                .ToList();

            // PASSING ONES ONLY, and /review found out why. `inDirectory` includes any
            // description OVER the limit, and those carry the regression suffix rather
            // than the marker — so on the regression this test is most likely to see
            // beside a real failure, the message below would have said "N PASSING
            // description(s) … even though its grades pass" about a page whose grade does
            // not pass. The over-limit case belongs to the sibling Clean test, which is
            // the separation this test's own docstring claims.
            var passing = inDirectory
                .Where(f => f.Grade <= ContentChecker.FailGrade)
                .ToList();

            // NOT VACUOUS PER ENTRY. The clean test counts the directory off disk; this
            // one only needs to know it is looking at something, and says which entry is
            // empty if it is not — a misspelled entry would otherwise make this loop
            // iteration pass by having nothing to check (§12.19).
            Assert.True(passing.Count > 0,
                $"'{dir}' is listed as a finished slice but produced no passing graded "
                + "description at all, so this iteration asserts nothing");

            var ungated = passing
                .Where(f => !f.Message.Contains(ContentChecker.GatedMarker,
                                                StringComparison.Ordinal))
                .Select(f => $"{f.File}: {f.Message}")
                .ToList();

            Assert.True(ungated.Count == 0,
                $"'{dir}' is listed as a finished slice, but {ungated.Count} passing "
                + $"description(s) under it do not carry '{ContentChecker.GatedMarker.Trim()}' "
                + "— the gate is not reaching this entry even though its grades pass: "
                + string.Join("; ", ungated));

            // AND THE TWO MARKERS ARE ACTUALLY DIFFERENT. /review: the whole claim is
            // "the marker is the only observable difference", and it is expressed as
            // Contains(GatedMarker) — so shortening GatedMarker to something the
            // backlog's marker also contains would make the narrowing mutation invisible
            // again. Asserted here rather than in a marker-shaped unit test, because
            // here it is beside the assertion that depends on it.
            Assert.DoesNotContain(ContentChecker.NotGatedMarker,
                ContentChecker.GatedMarker, StringComparison.Ordinal);
            Assert.All(passing, f => Assert.DoesNotContain(
                ContentChecker.NotGatedMarker, f.Message, StringComparison.Ordinal));
        }
    }

    // ---------- WI-414: reader-facing Razor prose ----------

    [Fact]
    public void RazorMarkupAndCodeAreNotGradedAsProse()
    {
        // The words a reader sees are the only words that count. Everything
        // else here (an attribute, an expression, a comment, a script) would
        // drag the grade around while saying nothing about the writing.
        var razor = """
            @model IndexModel
            @* An internal note about implementation subtleties. *@
            <section class="hub" aria-label="Overwhelmingly complicated description">
                <h1>We help you</h1>
                @if (Model.Items.Count > 0)
                {
                    <p>Scientists do the research. AI puts it into plain words.</p>
                }
                <script>var complicatedInitialization = configureEverything();</script>
            </section>
            """;

        var text = RazorTextExtractor.ExtractSentences(razor);

        Assert.Contains("We help you.", text);
        Assert.Contains("Scientists do the research.", text);
        Assert.DoesNotContain("Overwhelmingly", text);      // attribute value
        Assert.DoesNotContain("Model", text);               // razor expression
        Assert.DoesNotContain("implementation", text);      // razor comment
        Assert.DoesNotContain("configureEverything", text); // script body
    }

    /// <summary>
    /// The regression that made this tool trustworthy: `@if (x)` with a space
    /// before the bracket left the whole condition behind as "prose", grading
    /// a partial with no reader-facing words at all at grade 18. Nonsense
    /// findings are how a gate gets ignored.
    /// </summary>
    [Fact]
    public void AConditionIsNeverMistakenForProse()
    {
        var razor = """
            <span>
                @if (Model.Kind is BadgeKind.Result or BadgeKind.Unverified)
                {
                    <span class="meter"></span>
                }
                else if (Model.Kind == BadgeKind.Progress)
                {
                    <span>@Model.Label</span>
                }
            </span>
            """;

        var text = RazorTextExtractor.ExtractSentences(razor);

        Assert.DoesNotContain("BadgeKind", text);
        Assert.DoesNotContain("Unverified", text);
        Assert.True(
            text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length < 3,
            $"expected almost no prose, got: {text}");
    }

    [Fact]
    public void OrdinaryWordsThatLookLikeKeywordsSurvive()
    {
        // "if" and "for" are also English. Only a keyword followed by a
        // bracket is control flow.
        var text = RazorTextExtractor.ExtractSentences(
            "<p>Call us if you need help, or ask for a ride.</p>");

        Assert.Equal("Call us if you need help, or ask for a ride. ", text);
    }

    [Fact]
    public void HeadingsDoNotRunIntoTheParagraphBelowThem()
    {
        // The WI-106 lesson, carried over: merging a heading into the next
        // sentence inflates the grade and punishes exactly the structure that
        // helps an impaired reader.
        Assert.Equal(
            "Where to start. Ask your care team first. ",
            RazorTextExtractor.ExtractSentences(
                "<h2>Where to start</h2><p>Ask your care team first.</p>"));
    }

    [Fact]
    public void APageWithTooLittleProseIsNotGraded()
    {
        // A badge partial's words come from the model at runtime; grading six
        // stray words would produce a number nobody should act on.
        var findings = ContentChecker.CheckRazorPage(
            "<span class=\"badge\">@Model.Label</span>", "Shared/_StageBadge.cshtml");

        Assert.Equal(FindingLevel.Info, Assert.Single(findings).Level);
        Assert.Contains("too little to grade", findings[0].Message);
    }

    [Fact]
    public void HardRazorProseFailsTheGate()
    {
        var razor = """
            <p>
                Notwithstanding the aforementioned considerations regarding
                methodological heterogeneity, the investigators subsequently
                determined that stratification of participants necessitated
                additional multivariable adjustment procedures.
            </p>
            <p>
                Consequently, generalizability remains substantially constrained
                by unmeasured confounding variables inherent to observational
                epidemiological investigations of this particular nature.
            </p>
            """;

        var finding = Assert.Single(ContentChecker.CheckRazorPage(razor, "Hard.cshtml"));

        Assert.Equal(FindingLevel.Fail, finding.Level);
        Assert.Contains("simplify the language", finding.Message);
    }

    [Fact]
    public void AdminAndDevPagesAreNotHeldToThePatientReadingLevel()
    {
        // Staff tools legitimately use words like "classification" and
        // "authorization". Failing the build over them would only teach
        // people to ignore the gate.
        var root = Path.Combine(Path.GetTempPath(), $"bh-razor-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(root, "Admin"));
        Directory.CreateDirectory(Path.Combine(root, "Dev"));
        try
        {
            const string Hard =
                "<p>Reviewers reconcile classification discrepancies before publication " +
                "authorization, documenting justification within the audit infrastructure.</p>";
            File.WriteAllText(Path.Combine(root, "Admin", "Queue.cshtml"), Hard);
            File.WriteAllText(Path.Combine(root, "Dev", "StyleGuide.cshtml"), Hard);

            var findings = ContentChecker.CheckAll(root, null, Today, root);

            Assert.DoesNotContain(findings, f => f.File.Contains("Admin", StringComparison.Ordinal));
            Assert.DoesNotContain(findings, f => f.File.Contains("Dev", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
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
