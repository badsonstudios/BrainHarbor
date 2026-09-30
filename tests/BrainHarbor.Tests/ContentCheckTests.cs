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
            var grade = i == 0 ? worst : ContentChecker.FailGrade + 1.0;
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
