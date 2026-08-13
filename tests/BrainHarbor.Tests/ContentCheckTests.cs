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
