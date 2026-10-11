using BrainHarbor.ContentCheck;
using BrainHarbor.Safety;

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
        var grade = ReadingGrade.Of(
            "The cat sat on the mat. The dog ran to the park. We like to play.", ReadingGradeOptions.CuratedPages);

        Assert.True(grade < 4, $"expected < 4, got {grade}");
    }

    [Fact]
    public void PlainLanguageMedicalTextPassesTheGate()
    {
        var grade = ReadingGrade.Of(
            "A glioma is a tumor that starts in the brain. Doctors grade it " +
            "from 1 to 4. The grade tells you how fast it tends to grow. " +
            "Your care team will explain what your grade means.", ReadingGradeOptions.CuratedPages);

        Assert.True(grade <= 8.5, $"expected <= 8.5, got {grade}");
    }

    [Fact]
    public void AcademicTextScoresHigh()
    {
        var grade = ReadingGrade.Of(
            "Notwithstanding contemporary advancements in neuro-oncological " +
            "therapeutics, the prognostic implications of isocitrate dehydrogenase " +
            "mutations necessitate comprehensive multidisciplinary evaluation " +
            "incorporating histopathological and molecular characterization.", ReadingGradeOptions.CuratedPages);

        Assert.True(grade > 12, $"expected > 12, got {grade}");
    }

    [Fact]
    public void HarderTextScoresHigherThanSimplerText()
    {
        var simple = ReadingGrade.Of(
            "We read the news each day. Then we write it in plain words.", ReadingGradeOptions.CuratedPages);
        var harder = ReadingGrade.Of(
            "Subsequently, the organization disseminates carefully synthesized " +
            "summaries incorporating contemporaneous oncological developments.", ReadingGradeOptions.CuratedPages);

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
        Assert.Equal(expected, ReadingGrade.Syllables(word, ReadingGradeOptions.CuratedPages));
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
        var grade = ReadingGrade.Of(text, ReadingGradeOptions.CuratedPages);
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

        var structuredGrade = ReadingGrade.Of(
            ContentChecker.ExtractSentences(structured), ReadingGradeOptions.CuratedPages);
        var flatGrade = ReadingGrade.Of(flat, ReadingGradeOptions.CuratedPages);

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
            null, Today, CorpusFloor.None).Findings;

        Assert.Contains(findings, f => f.Level == FindingLevel.Warn && f.Message.Contains("MISSING"));
    }

    /// <summary>
    /// WI-435: and "loudly" was never loud enough. <b>The same call, the same missing
    /// root, under the floor the shipped corpus is checked against — and now it is a
    /// FAIL.</b>
    ///
    /// <para>The pair is the point. The Warn above is still emitted and still names the
    /// path, because that is the line a person reads; what changed is that it is no
    /// longer the ONLY thing emitted. The test above shipped on 2026-08-19 and was green
    /// the whole time this gate could grade zero medical pages and exit 0 — because a
    /// test asserting a WARN exists cannot notice that a WARN is all there is.</para>
    /// </summary>
    [Fact]
    public void MissingPagesRootUnderTheShippedFloorIsAFailureAndNotAWarning()
    {
        var findings = ContentChecker.CheckAll(
            Path.Combine(Path.GetTempPath(), "bh-does-not-exist-" + Guid.NewGuid().ToString("N")),
            null, Today, CorpusFloor.Shipped).Findings;

        var floor = Assert.Single(findings.Where(f => f.File == CorpusFloor.FileFor("pages")));
        Assert.Equal(FindingLevel.Fail, floor.Level);
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

        // WI-435: the pages root here is deliberately absent — this test is about the
        // glossary's own gate — so the floor says exactly that. One root held to the
        // shipped size, the rest not claimed. A `CorpusFloor.Shipped` would red on the
        // missing pages root and a `None` would let a renamed glossary through, and the
        // reason the floor is per root is that both of those are wrong answers.
        var findings = ContentChecker.CheckAll(
            Path.Combine(FindRepoRoot(), "no-pages"), glossaryRoot, Today,
            CorpusFloor.None with { GlossaryTerms = ContentChecker.GlossaryWhenMeasured })
            .Findings;

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

            // FAIL, NOT WARN, SINCE WI-578 SLICE 3 — and this line is the builder
            // lesson this very block was written to record, coming due. The over-limit
            // branch of `GradeDescription` emitted a Warn with "(not gated — WI-578)"
            // while there was a backlog to be gentle with; it emits a Fail everywhere
            // now. A builder left at Warn would still have produced a corpus the report
            // reads happily — and would have made
            // `AnOverLimitDescriptionThatWasNotFailedIsACorpusLevelFailure` below pass
            // against EVERY corpus this builder makes, which is the opposite of what it
            // asserts. **A test builder that does not match its producer tests the
            // builder**, and the producer moved.
            findings.Add(new(FindingLevel.Fail, $"p{i}.md [description]",
                $"reading grade {grade:0.0}, above the 6.0 limit", grade));
        }

        for (var i = 0; i < under; i++)
        {
            findings.Add(new(FindingLevel.Info, $"p{overTheLimit + i}.md [description]",
                "reading grade 4.0", 4.0));
        }

        for (var i = 0; i < tooShort; i++)
        {
            findings.Add(new(FindingLevel.Info,
                $"p{overTheLimit + under + i}.md [description]", "8 word(s) — too little to grade"));
        }

        return findings;
    }

    // ---------- WI-578 slice 3: what replaced the two ratchet constants ----------
    //
    // THREE TESTS WERE DELETED HERE AND THE REASON IS NOT "THEY BROKE".
    //
    //   AtParityTheRatchetReportsThatBothNumbersAreUnchanged — parity with WHAT? The
    //     two numbers it compared against are gone. The claim worth keeping out of it
    //     is round 3's, that a gate silent when it passes cannot be proved plugged in,
    //     and TheCorpusReportPrintsItsTotalsOnACleanRun below is that claim with no
    //     constant in it.
    //
    //   OneMoreDescriptionOverTheLimitFailsTheBuild — still true and no longer this
    //     method's doing. One more over the limit is a per-page Fail that names the
    //     page, which ADescriptionOverTheLimitFailsEverywhereNow asserts over every
    //     kind of path. A corpus counter asserting the same thing one line later and
    //     one page less precisely is not a second guard.
    //
    //   ADescriptionGettingWorseFailsEvenWhenTheCountIsUnchanged — the ceiling existed
    //     because a COUNT cannot see one description getting worse. A per-page gate
    //     can see nothing else. Every value the ceiling could have caught is above
    //     FailGrade and fails on sight now.
    //
    // WHAT IS NOT DELETED is the thing none of them covered: whether the per-page gate
    // is still WIRED. That is the one corpus-level claim the promotion created a need
    // for, and it has a test of its own below.

    /// <summary>
    /// IT SAYS SO WHEN IT PASSES, and that is /review round 3's blocker surviving the
    /// deletion of the constants it was first written about.
    ///
    /// <para>The report used to contribute the empty set when nothing had changed — so
    /// the call site in <c>CheckAll</c> was unobservable, and the end-to-end test written
    /// to prove the wiring was satisfied by <c>GradeDescription</c> alone. Deleting the
    /// call left the whole suite green and the tool's output byte-identical. <b>A gate
    /// that is silent when it passes cannot be proved to be plugged in.</b></para>
    ///
    /// <para>A clean corpus is <c>0</c> over the limit now rather than a recorded count,
    /// which is what WI-578 finishing means — so this asserts the line is there and says
    /// zero, not that it matches a constant.</para>
    /// </summary>
    [Fact]
    public void TheCorpusReportPrintsItsTotalsOnACleanRun()
    {
        var findings = ContentChecker.DescriptionCorpusReport(
            Corpus(0, 8, 5.0), ContentChecker.CorpusWhenMeasured).ToList();

        var info = Assert.Single(findings);
        Assert.Equal(FindingLevel.Info, info.Level);
        Assert.Equal("(corpus)", info.File);
        Assert.Contains("0 above the 6.0 limit", info.Message);

        // AND NO VERDICT SURVIVED THE COLLAPSE. The three findings this one replaced
        // carried "both unchanged" and "down from N — lower the constant to lock it
        // in", which were instructions to edit constants that no longer exist.
        Assert.DoesNotContain("unchanged", info.Message);
        Assert.DoesNotContain("down from", info.Message);
    }

    /// <summary>
    /// THE TOO-SHORT COUNT IS NAMED ON EVERY RUN, because it is the whole instrument for
    /// telling a rewrite from a truncation.
    ///
    /// <para>It used to ride on the down-count Info, which fired only when the over-limit
    /// count had fallen. There is no count left to fall and
    /// <b>§12.24's ruling makes that the normal case rather than the odd one</b>: inside
    /// a gated directory every description is already under the limit, so shortening one
    /// cannot move the limit count in either direction. The floor count is all there
    /// is.</para>
    /// </summary>
    [Fact]
    public void TheTotalsAlwaysNameTheTooShortCount()
    {
        var findings = ContentChecker.DescriptionCorpusReport(
            Corpus(0, 8, 5.0, tooShort: 3),
            ContentChecker.CorpusWhenMeasured).ToList();

        var info = Assert.Single(findings);
        Assert.Contains("3 under the 25-word floor", info.Message);
        Assert.Contains("floor count RISING", info.Message);
    }

    /// <summary>
    /// <b>IS THE PROMOTED GATE STILL WIRED?</b> The one corpus-level assertion WI-578
    /// slice 3 created a need for, and the only one of the old ratchet's Fails with a
    /// successor.
    ///
    /// <para>The grade is gated corpus-wide now, so "descriptions measured above the
    /// limit" and "descriptions FAILED for being above the limit" are the same number by
    /// construction. A gap means <see cref="ContentChecker.GradeDescription"/> is still
    /// reporting the grade and has stopped gating it — which is exactly how this gate
    /// died three times across WI-575 and WI-578, every time with the suite green: a
    /// culture round-trip, a precondition over an empty set, and a matcher that reached
    /// one entry of a two-entry list.</para>
    ///
    /// <para>The input is a corpus of over-limit descriptions left at <c>Warn</c> — the
    /// shape the producer emitted one slice ago and cannot emit now. It is built by hand
    /// rather than by <see cref="Corpus"/> for that exact reason: the builder was moved
    /// to Fail to match the producer, so asking it for the broken shape would be asking
    /// it to lie.</para>
    /// </summary>
    [Fact]
    public void AnOverLimitDescriptionThatWasNotFailedIsACorpusLevelFailure()
    {
        var findings = Corpus(0, 8, 5.0);
        findings.Add(new(FindingLevel.Warn, "p99.md" + ContentChecker.DescriptionMarker,
            "reading grade 9.0 — reported, and nobody gated it", 9.0));

        var result = ContentChecker.DescriptionCorpusReport(
            findings, ContentChecker.CorpusWhenMeasured).ToList();

        Assert.Contains(result, f => f.Level == FindingLevel.Fail
                                     && f.Message.Contains("stopped gating it"));
    }

    /// <summary>
    /// AND THE CONTROL: the same over-limit description, FAILED, is not a wiring defect.
    /// Without this the test above would pass over a check that fires on every corpus
    /// containing an over-limit description at all — §12.19, and the branch it would
    /// have been hiding is the one that runs on every real build.
    /// </summary>
    [Fact]
    public void AnOverLimitDescriptionThatWasFailedIsNotAWiringDefect()
    {
        var result = ContentChecker.DescriptionCorpusReport(
            Corpus(1, 8, 9.0), ContentChecker.CorpusWhenMeasured).ToList();

        Assert.DoesNotContain(result, f => f.Message.Contains("stopped gating it"));

        // AND THE TOTALS STILL PRINT, carrying the failure's count without a verdict.
        var corpus = Assert.Single(result);
        Assert.Equal(FindingLevel.Warn, corpus.Level);
        Assert.Contains("1 above the 6.0 limit", corpus.Message);
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

    /// <summary>
    /// A directory in NO entry of <see cref="ContentChecker.DescriptionsCleanDirectories"/>
    /// — the out-of-slice control path for the two branches that still key on the list.
    ///
    /// <para><b>It was <c>tumors/</c> for two slices, and slice 3 took it.</b> The three
    /// controls below pinned that by name — <c>Assert.DoesNotContain("tumors/", …)</c> —
    /// precisely so the day the last slice landed they would say what had happened
    /// instead of failing with a puzzling message. They did. <see cref="OutsideEverySlice"/>
    /// is the generalisation: assert the probe path is under no listed entry at all,
    /// rather than naming the one directory that happens to be unlisted this month.</para>
    ///
    /// <para><b>And the corpus still has somewhere for it to be.</b> With all three
    /// slices listed the list covers 46 of the 55 descriptions; the other nine are the
    /// seven root-level pages and the two under <c>seizures/</c>, which is why this is a
    /// real sibling directory and not an invention.</para>
    ///
    /// <para><b>A real directory rather than a bare path, and /review corrected the
    /// stated reason (slice 3).</b> Slice 2's version said a matcher written with
    /// <c>Contains</c>, or one comparing case-insensitively, would pass a directory-less
    /// probe and fail this one — which is false of this subject: neither loosening makes
    /// <c>seizures/probe.md</c> match any of the three entries. The real reason is
    /// narrower and still worth having: a directory-less probe cannot distinguish the
    /// matcher under test from one that effectively asks "does this path have a
    /// directory at all", and the bare path is carried as its own row of
    /// <see cref="EverySliceAndTheUnslicedRemainder"/> so both shapes are covered rather
    /// than conflated.</para>
    /// </summary>
    private const string UnslicedDirectory = "seizures/";

    /// <summary>Refuses a control path that has quietly become an in-slice path. The
    /// successor to three copies of <c>Assert.DoesNotContain("tumors/", …)</c>, and it
    /// fails with the sentence somebody needs rather than with a level mismatch twenty
    /// lines later.</summary>
    private static void OutsideEverySlice(string path)
    {
        var listed = ContentChecker.DescriptionsCleanDirectories
            .Where(dir => path.StartsWith(dir, StringComparison.Ordinal))
            .ToList();

        Assert.True(listed.Count == 0,
            $"'{path}' is the OUT-OF-SLICE control path, but it is inside "
            + $"{string.Join(", ", listed)} — a finished slice. A slice landed and took "
            + "this control's subject with it; pick a directory that is in no entry of "
            + $"{nameof(ContentChecker.DescriptionsCleanDirectories)} and say which one "
            + "in the test, because a control inside the thing it controls for proves "
            + "nothing");
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
    /// WI-578 slice 3: <b>A DESCRIPTION OVER THE LIMIT FAILS THE BUILD WHEREVER IT IS.</b>
    /// This is the Warn-to-Fail promotion §12.22 promised, §12.23 deferred and the last
    /// slice paid for, and the out-of-slice row is the half that is new.
    ///
    /// <para><b>THIS TEST USED TO HAVE AN OUT-OF-SLICE CONTROL AND CANNOT HAVE ONE ANY
    /// MORE, which is worth saying rather than leaving as an absence.</b> Its sibling
    /// <c>TheSameDescriptionOutsideAFinishedSliceIsOnlyTheKnownBacklog</c> asserted the
    /// same page under <c>tumors/</c> only warned, and that was the §12.19 pairing that
    /// made this test about the DIRECTORY rather than merely about an over-limit
    /// description. The level no longer depends on the directory, so there is nothing
    /// for a control to discriminate — the out-of-slice case is simply one more row of
    /// this theory. What moved out from under it is covered instead by
    /// <c>EveryFinishedSliceIsActuallyGatedOnTheShippedCorpus</c>, which is the ONLY
    /// remaining observable difference between a listed and an unlisted directory, and
    /// by the two branches below that still key on the list.</para>
    ///
    /// <para>Driven through the REAL <c>CheckPage</c> rather than a synthetic findings
    /// list, which is WI-575's round-2 lesson obeyed: seven tests stayed green over a
    /// dead ratchet because each built an input shape the producer never emits. The only
    /// thing this test hand-builds is a page.</para>
    ///
    /// <para>Run for EVERY entry in <see cref="ContentChecker.DescriptionsCleanDirectories"/>
    /// — see <see cref="FinishedSlices"/> for why the first one is not enough — plus a
    /// path in no entry at all, and a path with no directory at all.</para>
    /// </summary>
    [Theory]
    [MemberData(nameof(EverySliceAndTheUnslicedRemainder))]
    public void ADescriptionOverTheLimitFailsEverywhereNow(string prefix)
    {
        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     OverTheLimitDescription),
            prefix + "probe.md", Today);

        var fail = Assert.Single(findings, f =>
            f.Level == FindingLevel.Fail
            && f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));
        Assert.Contains("REGRESSION", fail.Message, StringComparison.Ordinal);

        // AND IT NEVER CLAIMS THE OPPOSITE OF ITS OWN LEVEL. An earlier version left
        // "(not gated — WI-578)" on the message while raising the level to Fail, so the
        // log carried both stories about the same page (/review, WI-578). The constant
        // is deleted now and this asserts the words never came back by hand.
        Assert.DoesNotContain("not gated", fail.Message, StringComparison.Ordinal);

        // AND IT DOES NOT EXPLAIN ITSELF WITH THE DIRECTORY LIST. The list did not
        // produce this level, and on the unsliced rows of this theory it is not even
        // true of the page — a failure explained by a fact that did not cause it sends
        // the reader to the wrong file.
        Assert.DoesNotContain("DescriptionsCleanDirectories", fail.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Every listed slice, plus a real directory in NO entry, plus the bare path. The
    /// promotion's whole claim is that the level is the same across all three kinds, so
    /// all three kinds are rows rather than one being a separate control.
    /// </summary>
    public static TheoryData<string> EverySliceAndTheUnslicedRemainder
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var dir in ContentChecker.DescriptionsCleanDirectories)
            {
                data.Add(dir);
            }

            data.Add(UnslicedDirectory);
            data.Add(string.Empty);
            return data;
        }
    }

    /// <summary>
    /// <b>AND THE UNSLICED ROW IS ACTUALLY UNSLICED</b> — /review, slice 3. The theory
    /// above adds <see cref="UnslicedDirectory"/> as a row and its docstring calls it "a
    /// real directory in NO entry", but nothing checked that: if a later slice listed
    /// <c>seizures/</c> the theory would quietly run a FOURTH in-slice row while still
    /// claiming to cover the unsliced case. That is the exact failure mode slice 3 had
    /// to clean up in three other tests, which pinned <c>tumors/</c> by name for two
    /// slices and then had their subject taken.
    ///
    /// <para>A <c>[Fact]</c> rather than a line inside the theory, because the claim is
    /// about the DATA SET and not about any one row — asserted inside the theory it
    /// would have to be skipped on four rows out of five.</para>
    /// </summary>
    [Fact]
    public void TheUnslicedRowOfThatTheoryIsReallyOutsideEverySlice()
    {
        // The theory's rows are every listed slice plus these two, so a row count that
        // stops tracking the list means the unsliced row has been dropped or doubled —
        // asserted by COUNT because xUnit's TheoryData rows are not plain strings to
        // enumerate, and the count is the part that can drift.
        Assert.Equal(ContentChecker.DescriptionsCleanDirectories.Count + 2,
            EverySliceAndTheUnslicedRemainder.Count);

        OutsideEverySlice(UnslicedDirectory);
    }

    /// <summary>
    /// WI-578 slice 3: <b>THE APPROACH BAND, which the promotion created the need for.</b>
    ///
    /// <para>/review: with the grade a hard corpus-wide Fail there was a CLIFF — a
    /// description at 5.9 printed <c>ok</c> (Info renders as <c>  ok</c>) and a one-word
    /// edit took it to a broken build with nothing in between, while the BODY gate has
    /// had a three-level shape over the same <see cref="ContentChecker.WarnGrade"/>
    /// since WI-414. The two now agree about what "close to the limit" means.</para>
    ///
    /// <para><b>And it fires on the shipped corpus</b>, which is what keeps it on the
    /// right side of §12.19: <c>/tumors/glioblastoma</c> reads 5.6 after slice 3's own
    /// /review round moved it from 5.4 to name a pronoun. A guard with no live subject
    /// on the day it lands has been measured on nothing.</para>
    ///
    /// <para>The assertion is on the BAND and not on a number this test typed, so a
    /// probe that drifts out of the band says so instead of passing for the wrong
    /// reason — which is what the first version of it did, at 10.4.</para>
    ///
    /// <para><b>And the probe and its control are the SAME 29 WORDS with ONE COMMA
    /// turned into a full stop</b> — which is not a convenience, it is this item's own
    /// thesis used as a fixture. 5.9 here, 4.0 in
    /// <see cref="ADescriptionWellUnderTheBandIsStillOnlyInfo"/>, not one word added or
    /// removed. So neither vocabulary nor length can be what moved the level, and the
    /// pair is a demonstration of the claim all three slices were built on rather than
    /// two unrelated sentences that happen to grade differently.</para>
    /// </summary>
    [Fact]
    public void ADescriptionInTheApproachBandWarnsRatherThanPrintingOk()
    {
        // MEASURED, NOT EYEBALLED, and the first two attempts are why that sentence is
        // here: one landed at 10.4 and one at 6.3 against a target of 5.5 to 6.0, which
        // is a window 0.5 wide. The second was worse than the first — it came from
        // pasting a grade measured for a DIFFERENT string, which is §12.23's rule about
        // a probe whose grade lives in a comment rather than in a measurement.
        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     "description: \"What a scan can show about a tumor in the brain. "
                     + "And why a radiologist writes the report the careful way they "
                     + "do, and what to ask of them.\""),
            UnslicedDirectory + "probe.md", Today);

        var description = Assert.Single(findings, f =>
            f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));

        Assert.NotNull(description.Grade);
        Assert.InRange(description.Grade!.Value,
            ContentChecker.WarnGrade, ContentChecker.FailGrade);
        Assert.Equal(FindingLevel.Warn, description.Level);
        Assert.Contains("close to the", description.Message, StringComparison.Ordinal);

        // AND IT IS NOT A FAILURE. The band exists to be the step between `ok` and a
        // broken build; a Warn that failed the build would just move the cliff.
        Assert.NotEqual(FindingLevel.Fail, description.Level);
    }

    /// <summary>And a description comfortably under the band still prints as Info, so the
    /// test above pins the BAND rather than "any grade warns". Without it, moving
    /// <see cref="ContentChecker.WarnGrade"/> to 0 would leave the suite green.
    ///
    /// <para><b>THE SAME 29 WORDS AS ITS SIBLING, WITH ONE COMMA MADE A FULL STOP</b> —
    /// 5.9 becomes 4.0 and not one word changes. The control differs from the probe by
    /// the single variable under test, and the pair happens to be a demonstration of
    /// WI-578's whole thesis: Flesch-Kincaid's words-per-sentence term was doing all of
    /// it.</para>
    ///
    /// <para>(The first version was 24 words and fell under the 25-word grading floor,
    /// so it had no grade at all and controlled for nothing — the hazard this whole item
    /// is about, arriving in its own test.)</para>
    /// </summary>
    [Fact]
    public void ADescriptionWellUnderTheBandIsStillOnlyInfo()
    {
        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     "description: \"What a scan can show about a tumor in the brain. "
                     + "And why a radiologist writes the report the careful way they "
                     + "do. And what to ask of them.\""),
            UnslicedDirectory + "probe.md", Today);

        var description = Assert.Single(findings, f =>
            f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal));

        Assert.NotNull(description.Grade);
        Assert.True(description.Grade < ContentChecker.WarnGrade,
            $"the control probe grades {description.Grade:0.0}, which is inside the "
            + "approach band — it cannot control for it");
        Assert.Equal(FindingLevel.Info, description.Level);
        Assert.DoesNotContain("close to the", description.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// WI-578, and <b>THE HOLE /review FOUND IN THE FIRST VERSION OF THE SLICE GATE.</b>
    ///
    /// <para>The gate lived in <see cref="ContentChecker.DescriptionCorpusReport"/>, which
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
        // THE PRECONDITION FIRED, WHICH IS WHAT IT WAS FOR. /review added it in slice 2
        // saying: when the last slice lands and `tumors/` joins the list, the assertions
        // below start failing with a puzzling message instead of this one. Slice 3
        // landed, it said exactly that, and the control moved to a directory in no entry
        // at all rather than to the next directory that happens to be unlisted.
        OutsideEverySlice(UnslicedDirectory);

        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more.",
                     "description: \"Why you were put on one.\""),
            UnslicedDirectory + "probe.md", Today);

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
        OutsideEverySlice(UnslicedDirectory);

        var findings = ContentChecker.CheckPage(
            PageWith("A short, simple body. It says a little and no more."),
            UnslicedDirectory + "probe.md", Today);

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
        var findings = Corpus(0, 8, 5.0, tooShort: 3);
        findings.Add(new(FindingLevel.Fail,
            FinishedSlice + "regressed.md" + ContentChecker.DescriptionMarker,
            "12 word(s) — too little to grade, and 25 are needed."));

        var result = ContentChecker.DescriptionCorpusReport(
            findings, ContentChecker.CorpusWhenMeasured).ToList();

        var corpus = Assert.Single(result);
        Assert.Equal(FindingLevel.Warn, corpus.Level);
        Assert.Equal("(corpus)", corpus.File);

        // THE VERDICTS ARE GONE...
        Assert.DoesNotContain("unchanged", corpus.Message);
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
        var findings = ContentChecker.DescriptionCorpusReport(
            [.. Enumerable.Range(0, ContentChecker.CorpusWhenMeasured)
                .Select(i => new Finding(FindingLevel.Info, $"p{i}.md", "reading grade 4.0"))],
            ContentChecker.CorpusWhenMeasured).ToList();

        Assert.Contains(findings, f => f.Level == FindingLevel.Fail
                                       && f.Message.Contains("NOT ONE description"));
    }

    /// <summary>
    /// A CORPUS TOO SMALL TO COMPARE IS LOUD, NOT SILENT — /review round 4's blocker.
    ///
    /// <para>The scope test used <c>DescriptionsOverTheLimit</c> as a page-count floor.
    /// That number was a count of bad descriptions, and WI-578 was instructed to lower
    /// it as the work landed: at 3 the description-less fixtures would start failing,
    /// and at 0 the precondition would never fire and a glossary-only run would fall
    /// through to the dead-instrument Fail. Both earlier blockers, re-armed by following
    /// this item's own instructions. <b>That count no longer exists to be confused with
    /// a corpus size</b>, and <see cref="ContentChecker.CorpusWhenMeasured"/> outlived
    /// it for a reason it states.</para>
    ///
    /// <para>And declining is reported, because round 3's ruling — a gate that is silent
    /// when it passes cannot be proved to be plugged in — is just as true of a gate
    /// silent when it declines.</para>
    /// </summary>
    [Fact]
    public void ACorpusSmallerThanTheMeasurementIsReportedRatherThanSkippedSilently()
    {
        var findings = ContentChecker.DescriptionCorpusReport(
            Corpus(1, 1, 7.0), ContentChecker.CorpusWhenMeasured - 1).ToList();

        var warn = Assert.Single(findings);
        Assert.Equal(FindingLevel.Warn, warn.Level);
        Assert.Equal("(corpus)", warn.File);
        Assert.Contains("DID NOT RUN", warn.Message);

        // AND IT NO LONGER TELLS ANYBODY TO LOWER TWO CONSTANTS THAT ARE GONE. The
        // message said "re-measure and lower DescriptionsOverTheLimit and
        // WorstDescriptionGrade together with CorpusWhenMeasured", which since slice 3
        // is an instruction to edit nothing — in the one finding whose whole job is to
        // tell somebody what to do about a corpus that shrank.
        Assert.DoesNotContain("DescriptionsOverTheLimit", warn.Message);
        Assert.DoesNotContain("WorstDescriptionGrade", warn.Message);
        Assert.Contains(nameof(ContentChecker.CorpusWhenMeasured), warn.Message);
    }

    /// <summary>
    /// AND NO CURATED PAGE AT ALL IS STILL SILENT, which is the other half: a caller
    /// scoping <c>CheckAll</c> to the glossary is not in the ratchet's business.
    /// </summary>
    [Fact]
    public void NoCuratedPageAtAllStaysSilent()
    {
        Assert.Empty(ContentChecker.DescriptionCorpusReport(Corpus(1, 1, 7.0), 0));
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
        var findings = ContentChecker.DescriptionCorpusReport(
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
        var findings = ContentChecker.DescriptionCorpusReport(
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
    /// and passed. Forever. This asserts the report reads the FIELD, by handing it a
    /// message no parser could read.</para>
    ///
    /// <para><b>The assertion had to move with the constants (WI-578 slice 3).</b> It
    /// used to look for the ceiling's Fail — <c>"now grades"</c> — which is gone. The
    /// claim is the same one and the surviving witness is the totals line: it prints
    /// <c>worst 16.1</c> only if the grade was read off the record, because the message
    /// says <c>8,4</c> and no parser here would get either number out of it.</para>
    /// </summary>
    [Fact]
    public void TheReportReadsTheGradeFieldAndNotTheMessage()
    {
        // A CORPUS AS LARGE AS THE MEASUREMENT: the report declines to speak about
        // anything smaller, because a one-page fixture is not the shipped corpus
        // (/review round 3).
        List<Finding> findings =
        [
            .. Enumerable.Range(0, ContentChecker.CorpusWhenMeasured)
                .Select(i => new Finding(FindingLevel.Info, $"p{i}.md", "reading grade 4.0")),

            // THE ONE DESCRIPTION, with a message no parser could read and the grade on
            // the FIELD. If the report ever goes back to parsing the message, this is
            // where the locale bug shows. At Fail, because the producer fails an
            // over-limit description now and a Warn here would trip the wiring check
            // for a reason that has nothing to do with locales.
            new(FindingLevel.Fail, "p0.md" + ContentChecker.DescriptionMarker,
                "reading grade 8,4 — in a locale no parser here understands", 16.1),
        ];

        var report = ContentChecker.DescriptionCorpusReport(
            findings, ContentChecker.CorpusWhenMeasured).ToList();

        var corpus = Assert.Single(report);
        Assert.Contains("worst 16.1", corpus.Message);
        Assert.Contains("1 above the 6.0 limit", corpus.Message);
    }

    /// <summary>
    /// THE REPORT RUNS ON THE REAL CORPUS, and this is the one assertion that would
    /// have caught /review round 2's blocker.
    ///
    /// <para>The tests above exercise every branch of
    /// <see cref="ContentChecker.DescriptionCorpusReport"/> with synthetic findings — and
    /// seven of them stayed green for a whole round while the ratchet was dead on the
    /// real corpus, because their body rows carried a <c>Grade</c> that the real producer
    /// never sets. <b>Branch coverage with a hand-built input proves the branches, not
    /// the wiring.</b></para>
    ///
    /// <para>So this one goes through <c>CheckAll</c> over the shipped corpus and asserts
    /// the report was REACHED: every page reports a description finding, and <b>no
    /// description anywhere in the corpus is above the limit</b> — which since WI-578
    /// slice 3 is a fact about the corpus and not a comparison against a recorded count.
    /// If the report ever stops being called, or stops seeing the descriptions, this
    /// reds.</para>
    /// </summary>
    [Fact]
    public void TheDescriptionReportActuallyRunsOverTheShippedCorpus()
    {
        var root = FindRepoRoot();
        var pagesRoot = Path.Combine(root, "src", "BrainHarbor.Web", "Content", "pages");
        var findings = ContentChecker.CheckAll(
            pagesRoot,
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "glossary"),
            Today, CorpusFloor.Shipped).Findings;

        var onDisk = Directory
            .EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories).Count();
        var descriptions = findings
            .Where(f => f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal))
            .ToList();

        Assert.Equal(onDisk, descriptions.Count);

        // THE GRADE IS ON THE RECORD. If it is ever only in the message again, the
        // locale bug is back and this is where it shows.
        Assert.Contains(descriptions, f => f.Grade is not null);

        // ZERO, AND NOT A CONSTANT. For three slices this compared `over` against a
        // recorded count in both directions, because the count was a ratchet and either
        // drift had to move it. WI-578 is finished, so the end state is a property of
        // the corpus rather than a number somebody maintains: NO page description reads
        // above sixth grade. Named, so a failure says which pages rather than "expected
        // 0, got 2".
        var over = descriptions
            .Where(f => f.Grade > ContentChecker.FailGrade)
            .Select(f => $"{f.File} grades {f.Grade:0.0}")
            .ToList();

        Assert.True(over.Count == 0,
            "every page description in the corpus must read at or under "
            + $"{ContentChecker.FailGrade:0.0} since WI-578 finished, and the grade is "
            + $"gated per page at Fail: {string.Join("; ", over)}");

        // THE WORST IS PRINTED, NOT COMPARED. The ceiling constant is gone — every
        // value it could have gated is above FailGrade and fails on sight — so the
        // number is still worth having in the failure message above and is no longer
        // its own assertion.
        var worst = descriptions.Where(f => f.Grade is not null).Max(f => f.Grade!.Value);
        Assert.True(worst <= ContentChecker.FailGrade,
            $"the worst description grades {worst:0.0}");

        // AND THE REPORT WAS ACTUALLY REACHED. This is the assertion /review round 3
        // asked for and the one the first version of this test could not make: at
        // parity the ratchet used to emit nothing, so every other line here was
        // satisfied by GradeDescription alone and the call site in CheckAll could have
        // been deleted without reddening a single test.
        var corpusLine = Assert.Single(findings, f => f.File == "(corpus)");
        Assert.Equal(FindingLevel.Info, corpusLine.Level);
        Assert.Contains("0 above the 6.0 limit", corpusLine.Message);

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
            Today, CorpusFloor.Shipped).Findings;

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
    /// WI-578 slice 3, and <b>THE ONE WAY LEFT TO UN-GATE A SHIPPED SLICE WITH THE WHOLE
    /// SUITE GREEN</b> — /review found it, and it is the fifth instance in this item's
    /// history of a gate that can be switched off silently.
    ///
    /// <para>Every other test of
    /// <see cref="ContentChecker.DescriptionsCleanDirectories"/> derives its expectation
    /// FROM the list: the theories iterate it, the two shipped-corpus tests
    /// <c>foreach</c> over it, <c>Assert.NotEmpty</c> passes at two entries, and the
    /// marker's negative control treats whatever is unlisted as legitimately unmarked.
    /// <b>So the list is both the subject and the oracle.</b> Delete <c>"tumors/"</c>
    /// and the 23 pages slice 3 paid for lose their protection against truncation and
    /// blanking, every one of those assertions still passes, and ContentCheck exits
    /// 0.</para>
    ///
    /// <para>It matters more since slice 3 than it did before. While the grade was gated
    /// per directory, dropping an entry at least changed an over-limit page's LEVEL; the
    /// grade is corpus-wide now, so this list's only remaining job is the two ways a
    /// description leaves the graded set — and that job is invisible on a corpus where
    /// nothing has left it.</para>
    ///
    /// <para><b>So this asserts the CONVERSE, which is self-maintaining.</b> Not "the
    /// listed directories are clean" — its sibling does that — but <i>a directory that
    /// is ALREADY clean must be listed</i>. It reds on deleting an entry, and it also
    /// reds on finishing a directory and forgetting to add it, which is the same
    /// omission from the other end. Nothing here needs updating when a slice lands: the
    /// expectation is computed from the corpus, and the only thing a new slice changes
    /// is which side of the test a directory falls on.</para>
    ///
    /// <para>Exactly satisfied today, which is why it can be an equality rather than a
    /// subset: <c>treatments/</c> (13), <c>tests/</c> (10) and <c>tumors/</c> (23)
    /// qualify; <c>seizures/</c> does not, because <c>what-to-do.md</c> is 20 words and
    /// therefore not graded. Root-level pages are not a directory and are not
    /// considered — six of the nine descriptions outside every entry are under the word
    /// floor, which is the reason the list does not cover the corpus.</para>
    /// </summary>
    [Fact]
    public void EveryDirectoryWhoseDescriptionsAlreadyPassIsListedAsAFinishedSlice()
    {
        var root = FindRepoRoot();
        var pagesRoot = Path.Combine(root, "src", "BrainHarbor.Web", "Content", "pages");
        var findings = ContentChecker.CheckAll(
            pagesRoot,
            Path.Combine(root, "src", "BrainHarbor.Web", "Content", "glossary"),
            Today, CorpusFloor.Shipped).Findings;

        var descriptions = findings
            .Where(f => f.File.EndsWith(ContentChecker.DescriptionMarker,
                                        StringComparison.Ordinal))
            .ToList();

        var directories = Directory
            .EnumerateDirectories(pagesRoot)
            .Select(d => Path.GetFileName(d) + "/")
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();

        // NOT VACUOUS. A pages root that stopped having subdirectories, or an
        // enumeration that returned none, would leave the comparison below between two
        // empty lists and passing (§12.19).
        Assert.True(directories.Count > 0,
            $"no subdirectories under {pagesRoot}, so this test compares two empty "
            + "lists and asserts nothing");

        // ALREADY CLEAN means every description in it is GRADED and passing. Both
        // halves, because an ungraded one is precisely how a page escapes the gate
        // this list drives — a directory with a blank or a 20-word description has not
        // been finished, and seizures/ is that case on the shipped corpus.
        var alreadyClean = directories
            .Where(dir =>
            {
                var inDirectory = descriptions
                    .Where(f => f.File.StartsWith(dir, StringComparison.Ordinal))
                    .ToList();

                return inDirectory.Count > 0
                       && inDirectory.All(f => f.Grade is not null
                                               && f.Grade <= ContentChecker.FailGrade);
            })
            .ToList();

        Assert.True(alreadyClean.Count > 0,
            "no directory on the shipped corpus has all of its descriptions graded and "
            + "passing, so this test asserts nothing — which cannot be true while "
            + "WI-578 is finished");

        var listed = ContentChecker.DescriptionsCleanDirectories
            .OrderBy(d => d, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(alreadyClean, listed);
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
            Today, CorpusFloor.Shipped).Findings;

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

        }

        // AND THE MARKER DISCRIMINATES, which is the assertion that replaced a check on
        // two markers not overlapping and is strictly stronger than it was.
        //
        // Slice 2 pinned "the marker is the only observable difference" by asserting
        // GatedMarker did not contain NotGatedMarker — because the loop above is
        // expressed as Contains(GatedMarker), so shortening it to something the
        // backlog's marker also contained would have made the narrowing mutation
        // invisible again. NotGatedMarker is deleted now, so that comparison has no
        // second operand.
        //
        // THE REAL CONTROL IS ON THE CORPUS, AND IT EXISTS BECAUSE THE LIST NEVER
        // SWALLOWED IT. Nine of the 55 descriptions are in no listed directory — the
        // seven root-level pages and the two under seizures/ — and three of those nine
        // are graded and passing. A marker that is printed on EVERY passing description
        // would satisfy the loop above on every entry while being evidence of nothing,
        // and nothing in slice 2's version could tell the difference. These three pages
        // can.
        var outside = graded
            .Where(f => f.Grade <= ContentChecker.FailGrade
                        && !ContentChecker.DescriptionsCleanDirectories
                            .Any(dir => f.File.StartsWith(dir, StringComparison.Ordinal)))
            .ToList();

        Assert.True(outside.Count > 0,
            "there is no passing description outside every entry of "
            + $"{nameof(ContentChecker.DescriptionsCleanDirectories)}, so the marker has "
            + "no negative control on the shipped corpus. If a slice has genuinely "
            + "claimed the whole corpus then this test's claim is empty and the marker, "
            + "the list and the two ungraded-escape branches should all go together");

        var wronglyMarked = outside
            .Where(f => f.Message.Contains(ContentChecker.GatedMarker,
                                           StringComparison.Ordinal))
            .Select(f => $"{f.File}: {f.Message}")
            .ToList();

        Assert.True(wronglyMarked.Count == 0,
            $"{wronglyMarked.Count} passing description(s) in NO listed directory carry "
            + $"'{ContentChecker.GatedMarker.Trim()}', so the marker is not evidence that "
            + "the gate reached anything — it is being printed either way: "
            + string.Join("; ", wronglyMarked));
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

            var findings = ContentChecker.CheckAll(root, null, Today, CorpusFloor.None, root).Findings;

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
