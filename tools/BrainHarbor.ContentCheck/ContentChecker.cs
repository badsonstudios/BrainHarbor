using System.Text;
using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BrainHarbor.ContentCheck;

public enum FindingLevel { Fail, Warn, Info }

/// <param name="Grade">
/// The reading grade this finding reports, when it reports one.
///
/// <para>WI-575: <b>the grade used to be recovered by parsing it back out of
/// <paramref name="Message"/>, and that silently disabled a build gate on every
/// comma-decimal locale.</b> The message is formatted with CurrentCulture, the parser
/// used InvariantCulture, so "reading grade 8,4" matched nothing, every description
/// scored -1, and the corpus-count ratchet reported zero and passed. A value
/// round-tripped through a human-readable string has two representations and one of
/// them depends on the machine. It rides here now; there is nothing to parse.</para>
/// </param>
public sealed record Finding(
    FindingLevel Level, string File, string Message, double? Grade = null);

/// <summary>
/// WI-106: walks curated pages + glossary terms and reports (content-pipeline
/// §5): reading grade (fail &gt; 6.0, warn ≥ 5.5), invalid front matter
/// (fail), missing sources (warn), overdue review_due (warn).
/// </summary>
public static partial class ContentChecker
{
    // WI-414 (2026-08-13, Dan): 6th grade, everywhere a reader looks. The
    // curated pages already sat at 2.5-4.9, and only two Razor pages needed
    // simplifying, so this is a floor the site already meets rather than an
    // aspiration. Summaries are NOT held to this yet - see content-pipeline
    // §5: three quarters of them would be flagged, which would empty the
    // feed rather than improve it.
    public const double FailGrade = 6.0;
    public const double WarnGrade = 5.5;

    /// <summary>
    /// Below this many words, one long word swings Flesch-Kincaid by several levels and
    /// the number says more about the sample than about the writing.
    ///
    /// <para>One constant, because it was declared twice with a comment saying "the same
    /// floor CheckRazorPage uses, and for the same reason" — which is a claim about
    /// another declaration rather than a shared one (/review round 1).</para>
    /// </summary>
    private const int MinimumWordsToGrade = 25;

    // WI-503: custom containers ONLY. Without this the ':::outlook' fence
    // parses as a paragraph and is graded as a sentence, so a reader-choice
    // gate would move the page's grade without changing a word of its prose.
    // Deliberately not UseAdvancedExtensions(): that would also restructure
    // tables and definition lists, silently re-grading every shipped page.
    private static readonly MarkdownPipeline TextPipeline =
        new MarkdownPipelineBuilder().UseCustomContainers().Build();

    /// <summary>Flags _Disclaimers.cshtml knows how to render.</summary>
    public static readonly string[] KnownDisclaimers = ["medical", "benefits", "legal"];

    /// <summary>
    /// Razor pages whose words a patient or caregiver reads. Admin and the dev
    /// styleguide are staff tools — holding a review queue to a patient reading
    /// level would only teach people to ignore the gate. Partials are included:
    /// a feed card's words are as public as a page's.
    /// </summary>
    private static bool IsReaderFacing(string relativePath) =>
        !relativePath.StartsWith("Admin/", StringComparison.OrdinalIgnoreCase)
        && !relativePath.StartsWith("Dev/", StringComparison.OrdinalIgnoreCase)
        && !Path.GetFileName(relativePath).StartsWith("_View", StringComparison.OrdinalIgnoreCase);

    public static List<Finding> CheckAll(
        string pagesRoot, string? glossaryRoot, DateOnly today, string? razorRoot = null,
        string? blocksRoot = null)
    {
        var findings = new List<Finding>();

        // WI-575: COUNTED, NOT INFERRED. DescriptionCorpusReport needs to know whether
        // any curated page was walked at all, and the first version inferred it from
        // the findings — asking for findings that carry a Grade and are not
        // descriptions. `GradeFinding` does not populate `Grade`, so that set was
        // always empty, `pagesWalked` was always 0, and the whole ratchet was dead
        // (/review round 2). Counting here cannot drift from what was walked.
        var curatedPagesWalked = 0;

        // WI-501: the blocks have to load before any page is graded — a page
        // is graded COMPOSED, so an unloadable block is a page-level failure.
        blocksRoot ??= ContentBlockStore.DefaultBlocksRootFor(pagesRoot);
        var blocks = ContentBlockStore.Load(blocksRoot);

        // At runtime a broken block only breaks the pages that include it.
        // Here it fails the build outright: shipping one is never intended.
        foreach (var (name, error) in blocks.Errors.OrderBy(e => e.Key, StringComparer.Ordinal))
        {
            findings.Add(new(FindingLevel.Fail, $"blocks/{name}.md", error));
        }

        var usedBlocks = new HashSet<string>(StringComparer.Ordinal);

        // A missing/empty root must be LOUD: a silently skipped directory is
        // how a safety gate dies of a rename. (Warn, not fail — the pages
        // root legitimately doesn't exist until WI-107 writes the pages.)
        if (Directory.Exists(pagesRoot))
        {
            var files = Directory.EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories)
                .OrderBy(f => f, StringComparer.Ordinal).ToList();
            if (files.Count == 0)
            {
                findings.Add(new(FindingLevel.Warn, pagesRoot, "pages root exists but has no .md files — nothing checked"));
            }
            curatedPagesWalked = files.Count;

            foreach (var file in files)
            {
                var relative = Path.GetRelativePath(pagesRoot, file).Replace('\\', '/');
                var raw = File.ReadAllText(file);
                findings.AddRange(CheckPage(raw, relative, today, blocks));
                findings.AddRange(CheckForMistypedDirectives(raw, relative, blocks));
                RecordUsedBlocks(raw, blocks, usedBlocks);
            }
        }
        else
        {
            findings.Add(new(FindingLevel.Warn, pagesRoot, "pages root MISSING — no pages were checked"));
        }

        // A block no page reaches is graded by nothing — the composed-page
        // rule means blocks have no reading level of their own.
        foreach (var orphan in blocks.Blocks.Keys.Where(name => !usedBlocks.Contains(name))
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            findings.Add(new(FindingLevel.Warn, $"blocks/{orphan}.md",
                "no page includes this block — nothing grades its reading level"));
        }

        // WI-582: COUNTED, NOT INFERRED — see GlossaryCorpusReport's precondition for
        // what inferring the description equivalent cost (a dead ratchet, seven green
        // tests).
        var glossaryTermsWalked = 0;

        if (glossaryRoot is not null && Directory.Exists(glossaryRoot))
        {
            var glossaryFiles = Directory.EnumerateFiles(glossaryRoot, "*.md")
                .OrderBy(f => f, StringComparer.Ordinal).ToList();

            // AN EXISTING ROOT WITH NOTHING IN IT WAS SILENT UNTIL WI-582 (/review).
            // The pages branch above has had this warning since WI-106 and the glossary
            // branch did not, so a root that exists and matches nothing — the glossary
            // moved under a subdirectory, the extension changed — walked zero entries,
            // produced zero findings, and `GlossaryCorpusReport` then took its
            // not-our-business early return. ContentCheck printed "passed, 0 failures"
            // with the definition gate switched off wholesale, which is the exact shape
            // GlossaryWhenMeasured's own docstring claims to guard against. Warn and not
            // Fail, matching the pages branch; the SET equality in
            // GlossaryDefinitionGradeTests is what gates it, against the files on disk.
            if (glossaryFiles.Count == 0)
            {
                findings.Add(new(FindingLevel.Warn, glossaryRoot,
                    "glossary root exists but has no .md files — nothing checked, and "
                    + "the definition grade gate was reached zero times"));
            }

            foreach (var file in glossaryFiles)
            {
                glossaryTermsWalked++;
                findings.AddRange(CheckGlossaryTerm(
                    File.ReadAllText(file), Path.GetFileNameWithoutExtension(file)));
            }
        }
        else if (glossaryRoot is not null)
        {
            findings.Add(new(FindingLevel.Warn, glossaryRoot, "glossary root MISSING — no terms were checked"));
        }

        // WI-414: the pages people actually land on. Their copy lives in
        // .cshtml, so until now the most-read text on the site was the only
        // text no tool checked.
        if (razorRoot is not null && Directory.Exists(razorRoot))
        {
            var razorFiles = Directory.EnumerateFiles(razorRoot, "*.cshtml", SearchOption.AllDirectories)
                .Select(f => (Full: f, Relative: Path.GetRelativePath(razorRoot, f).Replace('\\', '/')))
                .Where(f => IsReaderFacing(f.Relative))
                .OrderBy(f => f.Relative, StringComparer.Ordinal)
                .ToList();

            if (razorFiles.Count == 0)
            {
                findings.Add(new(FindingLevel.Warn, razorRoot,
                    "razor root exists but has no reader-facing .cshtml files — nothing checked"));
            }

            foreach (var (full, relative) in razorFiles)
            {
                findings.AddRange(CheckRazorPage(File.ReadAllText(full), relative));
            }
        }
        else if (razorRoot is not null)
        {
            findings.Add(new(FindingLevel.Warn, razorRoot, "razor root MISSING — no pages were checked"));
        }

        // WI-575, LAST: a corpus-level property, so it reads the findings every
        // per-page check produced rather than re-walking the corpus.
        //
        // WI-578 SLICE 3 EMPTIED MOST OF WHAT THIS USED TO DO, and this comment said so
        // wrongly for a slice before /review caught it: it read "the description's
        // reading grade cannot be gated across the whole corpus yet (29 of 49 are still
        // above the limit, down from 41)", which was stale at slice 2 and flatly false
        // at slice 3. The grade IS gated across the whole corpus now, per page, in
        // GradeDescription — so what is left here is the three things no per-page check
        // can do: say the sweep saw the whole corpus, say the instrument is alive, and
        // print the totals.
        // MATERIALISED FIRST. `AddRange` mutates `findings` while the iterator holds
        // a reference to it; this is safe today only because the single enumeration
        // completes before the first `yield`, and one more `findings.Where(...)`
        // after a yield would throw at runtime (/review round 4).
        findings.AddRange(DescriptionCorpusReport(findings, curatedPagesWalked).ToList());

        // WI-582, and MATERIALISED FIRST for the same reason the line above is. It also
        // runs AFTER that one, which costs nothing: the description report's findings
        // carry DescriptionMarker and this one selects on DefinitionMarker, so neither
        // can see the other's output. Two corpora, two markers, no cross-talk — the
        // property ContentCheckTests pins rather than assumes.
        findings.AddRange(GlossaryCorpusReport(findings, glossaryTermsWalked).ToList());

        return findings;
    }

    /// <summary>
    /// Reading level for a Razor page. Only the grade: front matter, sources
    /// and review dates are a curated-content idea, and a page with no prose
    /// at all (a partial that is pure markup) is reported as Info rather than
    /// pretended to be grade 0.
    /// </summary>
    public static List<Finding> CheckRazorPage(string raw, string relativePath)
    {
        var text = RazorTextExtractor.ExtractSentences(raw);
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        if (words < MinimumWordsToGrade)
        {
            return [new(FindingLevel.Info, relativePath, $"{words} word(s) of prose — too little to grade")];
        }

        return [GradeFinding(ReadabilityAnalyzer.FleschKincaidGrade(text), relativePath)];
    }

    /// <summary>
    /// WI-575: the reading grade of the front-matter <c>description</c>, which
    /// until this item NOTHING GRADED.
    ///
    /// <para><c>CheckPage</c> grades <c>page.Markdown</c> — the composed BODY — and
    /// <c>ContentPage.cshtml</c> renders <c>description</c> as the FIRST PARAGRAPH a
    /// reader meets, above that body. <c>ContentStore.SearchPages</c> puts it in front
    /// of a reader a second time, as the blurb under every search hit. So the one line
    /// most readers actually read was the one line held to no reading level at
    /// all.</para>
    ///
    /// <para><b>Four items tripped over the hole before it was closed</b> — WI-524,
    /// WI-528, WI-567, and WI-573 twice in one item. §12.17's cost was concrete: a
    /// <c>/review</c> blocker survived three rounds inside that one line, because the
    /// page had been rescoped everywhere a test could look.</para>
    ///
    /// <para>Graded through <see cref="ExtractSentences"/> like the body, not by a
    /// second reader: the description is prose with sentences in it, and feeding it
    /// raw would merge its sentences the way §12.20 records for headings.</para>
    /// </summary>
    private static List<Finding> GradeDescription(ContentPage page, string relativePath)
    {
        var description = page.FrontMatter.Description;
        var where = relativePath + DescriptionMarker;

        // IS THIS PAGE INSIDE A SLICE WI-578 HAS ALREADY FINISHED? The decision lives
        // HERE, per page, and not in DescriptionCorpusReport — which is where WI-578's first
        // round put it, and /review found three defects in that placement at once:
        //
        //   1. THE RATCHET ONLY SEES GRADED DESCRIPTIONS. A page could leave the graded
        //      set by dropping under MinimumWordsToGrade, which takes `over` DOWN (it
        //      reads as progress), leaves the ceiling untouched, and made the whole
        //      slice unwindable by TRUNCATION with every gate green and exit 0. The
        //      floor is the escape hatch this method's own comment warns about, and the
        //      gate was on the wrong side of it.
        //   2. IT PRINTED BOTH STORIES. The grade finding said "(not gated — WI-578)"
        //      and the ratchet's Fail for the same page said it was gated, two lines
        //      apart — the defect §12.22 round 4 removed between a different pair.
        //   3. IT SAT BEHIND TWO CORPUS-SIZE EARLY RETURNS it does not depend on.
        //      "Is this page over the limit?" needs no calibration against
        //      CorpusWhenMeasured, but deleting one page (55 → 54) made the ratchet
        //      `yield break` before the check ran — so the slice gate could be switched
        //      off by shrinking the corpus.
        //
        // Per page, all three go away: there is one finding per description, it carries
        // its own verdict, and no corpus-level condition can suppress it.
        var finishedSlice = DescriptionsCleanDirectories
            .Any(dir => relativePath.StartsWith(dir, StringComparison.Ordinal));

        // A BLANK DESCRIPTION IS REPORTED, not skipped. `FrontMatter.Description`
        // defaults to "" and nothing required it, so a page with no first paragraph was
        // legal by the tool's own rules — while ShellPagesTests asserts every page on
        // disk produces a description finding, and the ratchet's dead-instrument Fail
        // could fire on a legal description-less corpus. Warning here closes the cause
        // rather than the symptom: with pages walked, no descriptions at all is now
        // impossible, and the Fail below reduces to the genuine case. (/review round 4.)
        if (string.IsNullOrWhiteSpace(description))
        {
            return [new(finishedSlice ? FindingLevel.Fail : FindingLevel.Warn, where,
                NoDescriptionPrefix
                + " — ContentPage.cshtml renders it as the first paragraph a "
                + "reader meets and SearchPages renders it again as the blurb under every "
                + "hit, so a page without one is a page whose opening is unwritten"
                + (finishedSlice ? FinishedSliceSuffix : ""))];
        }

        var text = ExtractSentences(description);
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        // Reported rather than skipped, so a description too short to grade is
        // visible instead of silently ungraded — which is the whole defect this
        // method exists to close. The grade is deliberately left null here, and the
        // ratchet counts these separately: a description that falls UNDER the word
        // floor has not been improved, and scoring it as progress would let a page
        // buy its way out of grading by getting shorter (/review round 1).
        //
        // AND INSIDE A FINISHED SLICE THAT IS A FAILURE, not a note. In a directory
        // that has been rewritten, going ungraded is the one move that undoes the
        // rewrite without tripping a single corpus number.
        //
        // OUTSIDE ONE IT IS STILL ONLY A NOTE, AND THE REASON CHANGED WITH SLICE 3
        // (/review). It used to be "the backlog is still outstanding and a short
        // description is merely ungraded", which stopped being true the moment the
        // backlog reached 0. The reason now is that SIX DESCRIPTIONS ARE LEGITIMATELY
        // UNDER THIS FLOOR and every one of them is outside every listed directory —
        // terms.md at 7 words, about.md at 8, digest.md 12, privacy.md 12,
        // how-we-write.md 20, seizures/what-to-do.md 20. The corpus-wide too-short
        // tally has read 6 through all three slices and is the number that proves each
        // gain was a rewrite. WI-578 is an item about descriptions that read ABOVE
        // sixth grade; promoting this branch corpus-wide would fail the build on six
        // pages it was never about.
        if (words < MinimumWordsToGrade)
        {
            return [new(finishedSlice ? FindingLevel.Fail : FindingLevel.Info, where,
                $"{words} word(s) — {TooLittleToGradePhrase}"
                + (finishedSlice
                    ? $", and {MinimumWordsToGrade} are needed. This description is NO "
                      + "LONGER GRADED, which is not an improvement: it takes the page "
                      + "out of the count instead of under the limit"
                      + FinishedSliceSuffix + SplitItSuffix
                    : ""))];
        }

        // GATED CORPUS-WIDE SINCE WI-578 SLICE 3 — the Warn-to-Fail promotion §12.22
        // promised, §12.23 deferred and the last slice paid for. WI-575 found 41 of 49
        // gradeable descriptions above the 6.0 limit (median 8.4, max 19.7) and gating
        // that would have made this the gate people route around; slice 1 rewrote
        // treatments/ (41 → 29), slice 2 rewrote tests/ (29 → 21) and slice 3 rewrote
        // the 21 in tumors/, so the backlog this branch was waiting on is ZERO and
        // there is nothing left for a Warn to be kind to.
        //
        // THE CAUSE IS SENTENCE LENGTH, NOT VOCABULARY, and three slices have now
        // proved it rather than hypothesised it: forty-two descriptions rewritten, NOT
        // ONE WORD REMOVED from any of them (every word count equal or higher), worst
        // 19.7 → 5.3 in treatments/, 13.0 → 5.2 in tests/ and 11.1 → 5.4 in tumors/.
        // FK's words-per-sentence term was doing all of it.
        //
        // AND THE PROMOTION IS PER BRANCH, NOT WHOLESALE, which is slice 3's planning
        // finding and the one place this comment used to be wrong. It said that when
        // tumors/ landed, DescriptionsCleanDirectories would have "swallowed the whole
        // corpus" and every branch here would be a Fail. IT NEVER SWALLOWS THE CORPUS:
        // with all three slices listed it covers 46 of the 55 descriptions, and the
        // other NINE — the seven root-level pages and the two under seizures/ — are in
        // no listed directory at all. SIX OF THOSE NINE ARE UNDER THE WORD FLOOR TODAY
        // (terms.md at 7 words, about.md at 8, digest.md 12, privacy.md 12,
        // how-we-write.md 20, seizures/what-to-do.md 20), and they always have been:
        // the corpus-wide too-short tally has read 6 through all three slices and is
        // the number that proves each gain was a rewrite. So promoting the FLOOR branch
        // corpus-wide would fail the build on six pages WI-578 was never about — it is
        // an item about descriptions that read ABOVE sixth grade. Only the GRADE is
        // gated everywhere; the two ways a description leaves the GRADED SET stay gated
        // per directory, which is why `finishedSlice` above is still load-bearing.
        var grade = ReadabilityAnalyzer.FleschKincaidGrade(text);

        // FAIL above the limit ANYWHERE, WARN in the approach band, Info below it.
        // Info renders as "  ok" (Program.cs), so reporting a grade-19.7 description as
        // Info discharged "says out loud why it does not gate" by printing `ok` next to
        // the worst line in the corpus (/review round 1).
        //
        // THE WARN BAND IS NEW IN SLICE 3 AND THE PROMOTION IS WHAT CREATED THE NEED
        // (/review). While the grade was reported-not-gated the levels below the limit
        // did not matter much; now that it is a hard Fail there was a CLIFF — a
        // description at 5.9 printed `  ok` and a one-word edit took it to a broken
        // build, with nothing in between. GradeFinding has had exactly this three-level
        // shape for the body since WI-414, using the same WarnGrade, and a description
        // is prose with sentences in it. The two gates now agree about what "close to
        // the limit" means instead of disagreeing by a whole level.
        //
        // IT FIRES ON THE SHIPPED CORPUS, which is what makes it a guard rather than a
        // gesture: /tumors/glioblastoma reads 5.6 — §12.19's rule is that a guard
        // measured where it cannot fire has been measured on nothing, and this one has
        // a live subject on the day it lands.
        //
        // AND THE MESSAGE NEVER CLAIMS THE OPPOSITE OF ITS OWN LEVEL. "(not gated —
        // WI-578)" was true for the backlog and was printed over a Fail for a finished
        // slice once; it is deleted now rather than reworded, because there is no
        // ungated description left for it to be true of.
        //
        // THE FAILURE DOES NOT MENTION THE DIRECTORY LIST EITHER, and that is the same
        // rule. This level is not the list's decision any more, so a suffix naming
        // DescriptionsCleanDirectories would be explaining a verdict with a fact that
        // did not produce it — on the nine of 55 descriptions where it is not even true.
        var over = grade > FailGrade;

        // THE MARKER RIDES ON "NOT OVER", NOT ON "Info". Appending it on the Info
        // branch alone would have dropped it from every description in the new Warn
        // band — and EveryFinishedSliceIsActuallyGatedOnTheShippedCorpus asserts it on
        // every PASSING description under a listed directory, so the first page to
        // reach 5.6 inside a finished slice would have failed that test for a reason
        // that has nothing to do with the gate it guards.
        var level = over ? FindingLevel.Fail
            : grade >= WarnGrade ? FindingLevel.Warn
            : FindingLevel.Info;

        return
        [
            new(level, where,
                $"reading grade {grade:0.0}"
                + (over
                    ? $", above the {FailGrade:0.0} limit{CorpusWideSuffix}{SplitItSuffix}"
                    : (level == FindingLevel.Warn
                        ? $" is close to the {FailGrade:0.0} limit"
                        : "") + (finishedSlice ? GatedMarker : "")),
                grade),
        ];
    }

    /// <summary>
    /// What a PASSING description inside a finished slice says, and <b>the only thing on
    /// the real corpus that distinguishes a listed directory from an unlisted one.</b>
    ///
    /// <para><b>A directory that passes looks identical either way</b> — the grades are
    /// the grades. So narrowing the matcher (to the first entry of
    /// <see cref="DescriptionsCleanDirectories"/>, say) would switch the gate off for a
    /// whole slice while every grade assertion over the shipped corpus stayed green. This
    /// marker is what <c>EveryFinishedSliceIsActuallyGatedOnTheShippedCorpus</c> asserts
    /// instead — the <c>…IsActuallyClean…</c> test beside it is entirely about GRADES and
    /// never reads a message, which is why they are two tests rather than one.</para>
    ///
    /// <para><b>AND SLICE 3 MADE IT THE ONLY ONE, which raises what it is worth rather
    /// than lowering it.</b> While the grade was gated per directory there were two
    /// observable differences in principle — the level of an over-limit description and
    /// this marker — and only this one was visible on a corpus with nothing wrong with
    /// it. Now that <see cref="GradeDescription"/> fails an over-limit description
    /// everywhere, the list decides only the two ways a description can leave the GRADED
    /// set, and neither of those is observable on a corpus where every description is
    /// graded and passes. <b>This marker is the whole of the evidence that the matcher
    /// still matches</b>, which is why its test also pins that a passing description
    /// OUTSIDE every listed directory does not carry it.</para>
    ///
    /// <para><b>And the wording had to change with the promotion (slice 3).</b> It read
    /// " (gated — a finished slice)" while the grade was what the list gated. It no
    /// longer is, so the bare word would now name the wrong gate on a line reporting a
    /// grade — §12.22's rule that a message must not contradict its own facts, applied
    /// to a message whose facts moved out from under it.</para>
    ///
    /// <para>(/review, slice 2: this sentence named the <c>Clean</c> test, in the comment
    /// whose whole job is to name the right one. <c>&lt;c&gt;</c> rather than
    /// <c>&lt;see cref&gt;</c> because the test assembly references this project and not
    /// the other way round, so there is nothing here for the compiler to check.)</para>
    ///
    /// <para>A constant rather than a literal for the reason
    /// <see cref="DescriptionMarker"/> is: a test that types the message tests its own
    /// typing (WI-578 slice 2).</para>
    /// </summary>
    public const string GatedMarker = " (gated against going ungraded — a finished slice)";

    /// <summary>
    /// The tail an over-the-limit description carries, <b>wherever it is</b> — WI-578
    /// slice 3 promoted that branch corpus-wide, so there is no backlog half left to
    /// distinguish and no directory to credit.
    ///
    /// <para><b>It replaced <c>NotGatedMarker</c> rather than joining it.</b> That
    /// constant read " (not gated — WI-578)" and WI-578's acceptance criteria said to
    /// delete it when the last slice landed; as a named constant that deletion was a
    /// compile error at every site rather than a search for a parenthetical, which is
    /// exactly what it was kept for.</para>
    ///
    /// <para>It does NOT name <see cref="DescriptionsCleanDirectories"/>, and
    /// <see cref="FinishedSliceSuffix"/> is the one that still does. A failure should be
    /// explained by the thing that produced it: this level is produced by the grade
    /// alone now, and nine of the 55 descriptions are in no listed directory at all, so
    /// a suffix pointing at the list would be wrong on those nine and beside the point
    /// on the other 46.</para>
    /// </summary>
    /// <remarks>
    /// <b>PAST TENSE, AND /review PAID FOR THE DIFFERENCE.</b> This read "every
    /// description in the corpus HAS READ at or under this limit since WI-578" — a
    /// present-perfect claim printed on a finding that proves one does not, so the
    /// sentence was false at the moment it appeared, and false a second way about the
    /// six descriptions that are not graded at all. The fact that is true on the run
    /// this prints on is the one about WHEN WI-578 FINISHED.
    /// </remarks>
    private const string CorpusWideSuffix =
        ". No page description read above this limit when WI-578 finished, so this is a "
        + "REGRESSION and not something a backlog item is going to come back for";

    /// <summary>
    /// The tail the two <b>finished-slice-only</b> failures of
    /// <see cref="GradeDescription"/> carry — blank, and shortened under the word floor
    /// — so they say the same thing about why rather than two literals that drift apart.
    ///
    /// <para><b>Two branches now, not three (WI-578 slice 3).</b> The over-the-limit
    /// branch stopped keying on the directory list when the grade gate went corpus-wide,
    /// so it carries <see cref="CorpusWideSuffix"/> instead. These two still key on it,
    /// because six descriptions in the corpus are legitimately under the word floor and
    /// every one of them is outside every listed directory.</para>
    ///
    /// <para><b>The advice is NOT part of it, and /review found out why (slice 2).</b>
    /// This read "…the cause is almost always sentence length: SPLIT the sentence rather
    /// than shortening it" — appended verbatim to the BLANK-description failure, where
    /// there is no sentence to split and nothing was shortened. The shared part is the
    /// part that is true of both branches; <see cref="SplitItSuffix"/> is added by the
    /// one where a sentence actually exists.</para>
    /// </summary>
    /// <remarks>
    /// <b>IT NO LONGER CONTRASTS ITSELF WITH A BACKLOG THAT IS EMPTY (/review, slice
    /// 3).</b> It ended "…a REGRESSION rather than part of the remaining backlog",
    /// which was the right thing to say while 41, then 29, then 21 descriptions were
    /// outstanding. WI-578 reached 0, so the contrast names nothing and the sentence
    /// invites a reader to go looking for a list that is empty. The word REGRESSION
    /// stays — it is the verdict, and the three tests that read this suffix read it
    /// for that word.
    ///
    /// <para>And the replacement contrast is true of BOTH branches that carry this.
    /// "…has left the graded set rather than come under the limit" was the first
    /// attempt and is odd of a BLANK description, which was never under any limit —
    /// the same mistake slice 2 made by appending the split-it advice here.</para>
    /// </remarks>
    private const string FinishedSliceSuffix =
        ". This directory is listed in DescriptionsCleanDirectories, so WI-578 rewrote "
        + "every description in it and this is a REGRESSION rather than a page nobody "
        + "has got to yet";

    /// <summary>The fix, for the two failing branches that have a sentence in them —
    /// over the limit, and shortened under the floor. See
    /// <see cref="FinishedSliceSuffix"/> for why it is not shared with the blank
    /// branch, which has no sentence to split.</summary>
    private const string SplitItSuffix =
        ". The cause is almost always sentence length: SPLIT the sentence rather than "
        + "shortening it";

    // THE TWO RATCHET CONSTANTS ARE GONE, AND THAT IS WHAT WI-578 FINISHING LOOKS LIKE.
    //
    //   DescriptionsOverTheLimit — a count of descriptions above FailGrade, 41 at
    //     WI-575, 29 after slice 1, 21 after slice 2, and 0 once slice 3 rewrote
    //     tumors/. It existed because the grade could not be gated without failing 41
    //     pages, so what was gated instead was DRIFT: the corpus may not grow the
    //     backlog. With the backlog at zero and GradeDescription failing an over-limit
    //     description on sight, "more than 0 of them" is a slower, blinder restatement
    //     of a per-page Fail that has already fired and named the page.
    //
    //   WorstDescriptionGrade — a ceiling on the worst grade, 19.7 → 13.0 → 11.1. It
    //     was the half of the ratchet that a count cannot see: push one of the 41 from
    //     8.4 to 40.0 and the count does not move. Every value it could gate is above
    //     FailGrade, so the per-page Fail now catches all of them and a ceiling above
    //     6.0 is pure slack. §12.19's rule applies to the ratchet itself in the end — a
    //     guard that cannot fire has been measured on nothing.
    //
    // WHAT DID NOT GO WITH THEM: the two ways a description leaves the graded set, which
    // no grade gate can see and which DescriptionsCleanDirectories still gates per
    // directory; the corpus-size precondition below; and the dead-instrument Fail. The
    // method they lived in is DescriptionCorpusReport now, because what is left of it
    // does not ratchet.

    /// <summary>The marker <see cref="GradeDescription"/> appends to a description
    /// finding's file path, so the four places that recognise one agree by construction
    /// rather than by four literals (/review round 2).</summary>
    public const string DescriptionMarker = " [description]";

    /// <summary>
    /// The number of curated pages the corpus-level description report was measured on
    /// (WI-575). A smaller corpus is not the shipped one and the report's totals say
    /// nothing about it.
    ///
    /// <para><b>NOT a count of bad descriptions, which is what the first version used
    /// — and the conflation switched the gate off.</b> It was
    /// <c>DescriptionsOverTheLimit</c>, and WI-578's acceptance criteria — printed in
    /// this file's own messages — said to lower that as the work landed. At 3 the
    /// description-less test fixtures would start failing; at 0 the precondition would
    /// never fire at all and a glossary-only run would fall through to the
    /// dead-instrument Fail. <b>Two independent facts were being tested with one
    /// number</b> (/review round 4), which re-armed the blockers of rounds 2 and 3 with
    /// no code change at all — just by following the instructions. <b>This number is a
    /// corpus SIZE and is never lowered to match a falling count</b>; the count it used
    /// to be confused with no longer exists to be confused with.</para>
    ///
    /// <para><b>It outlived the two constants it was calibrating (WI-578 slice 3), and
    /// on purpose.</b> There is no ratchet left to calibrate, but the report's remaining
    /// job is to say the sweep saw the whole corpus — and the per-page gate is reached
    /// once per page walked, so a glob that silently stops matching switches the gate
    /// off wholesale rather than one page at a time. That is §12.23's "deleting one page
    /// switched the slice gate off" from the other end, and it is why this check is a
    /// standalone finding and not an early return in front of
    /// <see cref="GradeDescription"/>.</para>
    /// </summary>
    public const int CorpusWhenMeasured = 55;

    /// <summary>
    /// The directories whose descriptions ALL pass <see cref="FailGrade"/> — WI-578's
    /// finished slices. <see cref="GradeDescription"/> gates <b>the two ways a
    /// description can leave the GRADED SET</b> under one of these at
    /// <see cref="FindingLevel.Fail"/>: blank, and shortened under
    /// <c>MinimumWordsToGrade</c>.
    ///
    /// <para><b>THE GRADE ITSELF IS NO LONGER THIS LIST'S BUSINESS (WI-578 slice 3).</b>
    /// It was, for two slices: the list was how the promised Warn-to-Fail promotion got
    /// applied one slice at a time instead of all at once at the end. With all three
    /// slices landed there is no backlog to be gentle with, so the grade fails
    /// everywhere and this list keeps only the part no grade gate can see.</para>
    ///
    /// <para><b>AND IT NEVER SWALLOWED THE CORPUS, which the comment it replaces said it
    /// would.</b> All three slices listed cover <b>46 of the 55</b> descriptions. The
    /// other nine are the seven root-level pages and the two under <c>seizures/</c>, and
    /// <b>six of those nine are under the word floor today</b> — legitimately, through
    /// all three slices, which is why the corpus-wide too-short tally has read 6 the
    /// whole time and is the number that proves each gain was a rewrite rather than a
    /// truncation. Deleting the out-of-slice level on the floor branch, as the promotion
    /// was written down, would fail the build on six pages this item was never about.
    /// <b>Ask which pages are in NO entry</b> — §12.19 asked which pages a guard can
    /// fire on and §12.24 asked which entries, and this is the same question asked of
    /// the complement.</para>
    ///
    /// <para><b>WHY A PER-DIRECTORY GATE AND NOT A COUNT: the other numbers could not
    /// see a swap inside a fixed total.</b> While the ratchet existed, fixing a
    /// <c>tumors/</c> description in the same commit that let a <c>treatments/</c> one
    /// regress to 9.0 held the count at 29 and the ceiling at 13.0 — both halves green,
    /// and the shipped slice silently unwound. A count locks in a TOTAL; this locks in a
    /// SLICE. The counts are gone and this is not, because the escape it closes is not a
    /// grade: truncating a description under the floor took it out of the graded set
    /// entirely, dropped the count by one (reading as PROGRESS), left the ceiling
    /// untouched, and unwound a slice with every gate green and exit 0.</para>
    ///
    /// <para>Grows by one entry per slice, and <b>nothing may be added to it that does
    /// not already pass</b>: a clean-directory list is otherwise a claim about the corpus
    /// that lives only in a constant. That is what
    /// <c>EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus</c> exists to refuse — and
    /// it counts the <c>.md</c> files on disk, so a directory cannot half-qualify
    /// either.</para>
    ///
    /// <para><b>A SLICE IS A DIRECTORY, NOT THE FILES THE SLICE EDITED.</b> Slice 2
    /// rewrote 8 descriptions in <c>tests/</c> but the directory holds 10, and the other
    /// two (<c>mri</c> at 5.0, <c>planning-scans</c> at 5.2) already passed with margin
    /// and were left alone. Adding <c>tests/</c> gates all ten, so those two are now
    /// gated by a slice that did not touch them — which is the point: the claim being
    /// locked in is <i>this directory reads at sixth grade</i>, and a claim about eight
    /// of ten files is not one anybody can act on. Slice 1 reached the same place from
    /// the other side by splitting <c>stereotactic-radiosurgery</c>, which graded exactly
    /// 6.0 with no margin at all. Slice 3 is the same again: <c>tumors/</c> holds 23
    /// descriptions, 21 were over the limit, and <c>high-grade-glioma</c> (4.6) and
    /// <c>spinal-cord-tumor</c> (5.4) were left alone because they pass with about a
    /// grade of margin.</para>
    ///
    /// <para><see cref="IReadOnlyList{T}"/> rather than an array: this list's entire job
    /// is that a gate cannot be switched off quietly, and a <c>string[]</c> field is
    /// writable through by any caller that can see it.</para>
    /// </summary>
    public static readonly IReadOnlyList<string> DescriptionsCleanDirectories
        = ["treatments/", "tests/", "tumors/"];

    /// <summary>
    /// How a blank-description finding starts, so the tool and the test that pins it
    /// agree by construction rather than by two literals.
    ///
    /// <para>/review round 5: <c>ShellPagesTests</c> matched this by typing the prefix.
    /// Reword the message and the set goes empty, <c>Assert.True(0 == 0)</c> passes, and
    /// the assertion added one round earlier is vacuous — the exact defect
    /// <see cref="DescriptionMarker"/> was introduced to remove, two blocks away in the
    /// same test.</para>
    /// </summary>
    public const string NoDescriptionPrefix = "no description";

    /// <summary>
    /// The phrase the under-the-floor branch of <see cref="GradeDescription"/> always
    /// carries, so <see cref="DescriptionCorpusReport"/> can count those findings
    /// POSITIVELY rather than as "everything left over".
    ///
    /// <para>/review, slice 3: the floor tally was
    /// <c>descriptions.Count - graded.Count - blank</c>, three lines above the comment
    /// that states §12.22's rule against sets defined by exclusion — and the number it
    /// produced is the one this file calls <i>the entire instrument</i> for telling a
    /// truncation from a rewrite (§12.24). A fourth ungraded branch would have inflated
    /// it silently. It is matched, and the three counts are asserted to partition.</para>
    ///
    /// <para>A constant for the reason <see cref="NoDescriptionPrefix"/> is: the count
    /// and the message would otherwise agree by two literals. Not a PREFIX, unlike its
    /// sibling, because the message opens with the word count.</para>
    /// </summary>
    public const string TooLittleToGradePhrase = "too little to grade";

    // ------------------------------------------------------------------ WI-582, §12.30
    // THE GLOSSARY DEFINITION, GRADED AND NOW GATED. Three constants, for the three
    // reasons the description surface needed its own: a marker so the report selects
    // grade findings POSITIVELY rather than by sniffing a message, a NAMED word floor
    // so the number stops being a bare literal beside a comment about a different
    // number, and a corpus size so a shrunken glossary cannot pass for a clean one.

    /// <summary>
    /// The marker <see cref="CheckGlossaryTerm"/> appends to a definition's GRADE
    /// finding, so <see cref="GlossaryCorpusReport"/> and the tests that pin it agree
    /// by construction rather than by three literals — <see cref="DescriptionMarker"/>'s
    /// reason, unchanged (/review round 2 of WI-575).
    ///
    /// <para>It rides only on the grade finding, not on the word-count or source
    /// findings: those are about the file and the grade is about the prose, and the
    /// report counts exactly one kind.</para>
    /// </summary>
    public const string DefinitionMarker = " [definition]";

    /// <summary>
    /// Below this many words a definition is NOT graded — the literal that used to sit
    /// inline in <see cref="CheckGlossaryTerm"/>, named here because the gate it now
    /// feeds makes the SET it defines the first question anybody asks of it (§12.19).
    ///
    /// <para><b>IT IS 20 AND <see cref="MinimumWordsToGrade"/> IS 25, and the comment
    /// beside the old literal said "the same reason <c>CheckRazorPage</c> refuses to
    /// grade under 25 words" — true of the REASON and not of the NUMBER.</b> A reader of
    /// that sentence would reasonably have read 25. The reason is shared and still
    /// stands: Flesch-Kincaid on a 25-word sample is too noisy to fail a build on. The
    /// numbers differ because the artifacts do, and measurably: a definition is capped
    /// at <b>40 words</b> by content-pipeline §6 while a page body and a description
    /// have no ceiling at all, so a definition lives in a 20-to-40-word window. Raising
    /// this to 25 would take <b>8 of the 105 shipped entries</b> out of the graded set —
    /// <c>cdkn2a-b-deletion</c>, <c>radiation-oncologist</c>, <c>posterior-fossa</c>,
    /// <c>temozolomide</c>, <c>extra-axial</c>, <c>linear-accelerator</c>,
    /// <c>craniectomy</c> and <c>microvascular-proliferation</c>, grading 2.6 to 5.7 —
    /// which is eight entries dropped out of a gate in exchange for tidiness.</para>
    ///
    /// <para><b>AND THIS IS THE ESCAPE HATCH, which is the whole hazard this item was
    /// planned against.</b> A definition trimmed under this floor LEAVES the graded set,
    /// and §12.23 records the identical hole shipping once already at the 25-word
    /// description floor. The property is gated in the suite, not here: the ungraded set
    /// is pinned as a SET of named slugs, so an entry joining it reds. A count would not
    /// — four out, four in, and nothing moves.</para>
    /// </summary>
    public const int MinimumDefinitionWordsToGrade = 20;

    /// <summary>
    /// The number of glossary entries <see cref="GlossaryCorpusReport"/>'s totals were
    /// measured on (WI-582). <see cref="CorpusWhenMeasured"/>'s reason, for the other
    /// corpus: a smaller glossary is not the shipped one, and the per-entry gate is
    /// reached once per file walked, so a glob that stops matching switches the gate off
    /// wholesale rather than one entry at a time.
    ///
    /// <para><b>A corpus SIZE, never a count of anything that can be fixed</b> — which
    /// is the conflation that re-armed two closed blockers in WI-578 (/review round 4)
    /// just by following the instructions printed in the tool's own messages.</para>
    /// </summary>
    public const int GlossaryWhenMeasured = 105;

    /// <summary>
    /// The corpus-level description report, run once over the whole corpus rather than
    /// per page — every property below is a corpus property and a per-page check cannot
    /// see any of them.
    ///
    /// <para><b>IT WAS <c>DescriptionRatchet</c> UNTIL WI-578 SLICE 3, and the rename is
    /// the point.</b> It held two upper bounds — a count of descriptions over
    /// <see cref="FailGrade"/> and a ceiling on the worst grade — because the grade could
    /// not be gated while 41 pages failed it. With the last slice landed
    /// <see cref="GradeDescription"/> fails an over-limit description on sight, both
    /// bounds became restatements of a per-page Fail that has already named the page,
    /// and they are deleted. <b>Nothing here ratchets any more</b>, and a method whose
    /// name says it does is a message contradicting its own facts — §12.22's rule
    /// applied to an identifier.</para>
    ///
    /// <para>What is left is three things no per-page check can do: say the sweep saw the
    /// whole corpus, say the instrument is alive, and print the totals. The totals matter
    /// most on a failing run: inside a listed directory the over-the-limit count cannot
    /// move, so <b>the word-floor count is the entire instrument</b> for telling a
    /// truncation from a rewrite (§12.24).</para>
    ///
    /// <para><paramref name="curatedPagesWalked"/> is passed rather than inferred — see
    /// the precondition below for what inferring it cost.</para>
    ///
    /// <para>(This docstring was stacked on the deleted <c>WorstDescriptionGrade</c> for
    /// a round, which is the defect /review round 1 fixed elsewhere in this item
    /// recreated by its own fix. A duplicate <c>&lt;summary&gt;</c> raises no compiler
    /// warning and only one survives into IntelliSense.)</para>
    /// </summary>
    public static IEnumerable<Finding> DescriptionCorpusReport(
        List<Finding> findings, int curatedPagesWalked)
    {
        var descriptions = findings
            .Where(f => f.File.EndsWith(DescriptionMarker, StringComparison.Ordinal))
            .ToList();

        // WAS A CURATED PAGE WALKED AT ALL? "No descriptions" has two causes and only
        // one is a defect. A caller can legitimately scope CheckAll to the glossary or
        // to razor pages alone — ContentCheckTests does both — and this report has no
        // business failing that. The defect is pages walked and NO description graded,
        // which is the silent death: a missing pages root is only a Warn, so it used
        // to look like progress.
        //
        // PASSED IN, NOT INFERRED, and that is /review round 2's blocker. The first
        // version derived this from the findings — those carrying a Grade and not
        // being descriptions — and `GradeFinding` never populates `Grade`, so the set
        // was always empty and the ENTIRE ratchet was dead on the real corpus. Every
        // one of the seven tests written in the same round stayed green, because each
        // built a synthetic body finding with a Grade that production cannot emit.
        // **A test builder that does not match its producer tests the builder.**
        // NOT OUR BUSINESS: no curated page in scope at all. A caller may legitimately
        // check the glossary or the razor pages alone, and ContentCheckTests does both.
        if (curatedPagesWalked == 0)
        {
            yield break;
        }

        // IN SCOPE BUT TOO SMALL TO COMPARE, and LOUD about it. A ratchet whose
        // constants were measured on CorpusWhenMeasured pages has nothing to say about
        // a three-page fixture — but saying nothing is the property round 3 rejected
        // for the passing case, and it is no better here: at 1-40 pages the earlier
        // version contributed the empty set, so ContentCheck printed nothing and exited
        // 0 and the only alarm was a test whose message said "the collection contained
        // 0 matching elements".
        //
        // Warn rather than Info because Info renders as "  ok" (Program.cs), which is
        // the mistake this item already made once with the description grade itself.
        // AND THE MESSAGE NO LONGER TELLS ANYBODY TO LOWER TWO CONSTANTS THAT ARE GONE.
        // It said "re-measure and lower DescriptionsOverTheLimit and WorstDescriptionGrade
        // together with CorpusWhenMeasured", which was the right instruction while this
        // method held a ratchet and is an instruction to edit nothing now. What a shrunken
        // corpus actually threatens is the per-page gate's REACH: GradeDescription runs
        // once per page walked, so a glob that stops matching switches it off wholesale
        // and silently (§12.23, from the other end).
        if (curatedPagesWalked < CorpusWhenMeasured)
        {
            yield return new(FindingLevel.Warn, "(corpus)",
                $"{curatedPagesWalked} curated page(s) were walked against the "
                + $"{CorpusWhenMeasured} this corpus was last measured at, so the "
                + "corpus-level description totals below DID NOT RUN. Every page that was "
                + "walked is still graded and gated one page at a time; what is not "
                + "checked is whether the sweep reached the whole corpus. If the corpus "
                + $"legitimately shrank, re-measure and lower {nameof(CorpusWhenMeasured)} "
                + "— it is a corpus SIZE and never a count of anything that can be "
                + "fixed.");
            yield break;
        }

        var graded = descriptions.Where(f => f.Grade is not null).ToList();

        // AND THE FAILURE IS ON UNGRADED, NOT ON ABSENT, which is the way the
        // instrument actually dies. Truncate every description under the 25-word floor
        // and `descriptions.Count` is still 55 while `graded.Count` is 0 — so `over`
        // becomes 0 and the old check reported a TOTAL WIN. A dead instrument printed
        // as progress is the precise failure the absent-check existed to prevent.
        if (graded.Count == 0)
        {
            yield return new(FindingLevel.Fail, "(corpus)",
                $"{curatedPagesWalked} curated page(s) were walked and NOT ONE description "
                + $"was GRADED ({descriptions.Count} description finding(s), all of them "
                + "ungraded). Either GradeDescription has stopped producing grades, or "
                + "every description has fallen under the word floor, or every page has "
                + "lost its description — all three look like '0 above the limit' to a "
                + "counter, which is why this is a failure and not a clean bill.");
            yield break;
        }
        // BLANKS COUNTED SEPARATELY. `descriptions.Count - graded.Count` swept them in
        // with the too-short ones after round 4 made a blank description a finding, so
        // both Info messages would have reported "under the 25-word floor" about a page
        // that has no description at all — the wrong cause, in the message a reader of
        // the log acts on (/review round 5). Unreachable today at zero blanks, and that
        // is exactly when an attribution error gets written down and believed.
        var blank = descriptions.Count(f =>
            f.Message.StartsWith(NoDescriptionPrefix, StringComparison.Ordinal));

        // AND THE FLOOR COUNT IS POSITIVE TOO NOW, which /review found three lines
        // above the comment forbidding exactly this. It was
        // `descriptions.Count - graded.Count - blank` — a set defined by what it is
        // NOT, which §12.22's last lesson says is redefined by every new kind of X,
        // and which the paragraph below states as a rule while this line broke it.
        // The number it produced is the one this file calls the ENTIRE instrument for
        // telling a truncation from a rewrite (§12.24), so a fourth ungraded reason
        // would have silently inflated the one count nobody can afford to doubt.
        var tooShort = descriptions.Count(f =>
            f.Message.Contains(TooLittleToGradePhrase, StringComparison.Ordinal));

        // AND THE THREE SETS MUST PARTITION THE DESCRIPTIONS, asserted rather than
        // assumed — because making each count positive moves the failure mode from
        // "one number is quietly wrong" to "the numbers do not add up", and only a
        // check says which. A fourth ungraded branch added later lands here instead of
        // vanishing.
        if (graded.Count + blank + tooShort != descriptions.Count)
        {
            yield return new(FindingLevel.Fail, "(corpus)",
                $"{descriptions.Count} description finding(s) do not partition: "
                + $"{graded.Count} graded + {blank} blank + {tooShort} under the "
                + $"{MinimumWordsToGrade}-word floor. {nameof(GradeDescription)} has a "
                + "branch these three counts cannot see, so the floor count — which is "
                + "the whole instrument for telling a truncation from a rewrite — is "
                + "measuring the wrong set.");
        }

        var over = graded.Count(f => f.Grade > FailGrade);
        var worst = graded.Max(f => f.Grade!.Value);

        // HAS ANY DESCRIPTION FAILED? GradeDescription owns that verdict — per page,
        // where it can also see the two ungraded escape routes (blank, and under the
        // word floor) that this corpus-level code is structurally blind to. Read off
        // the LEVEL it set rather than re-deriving anything, so the two cannot
        // disagree.
        //
        // NOT "HAS A FINISHED SLICE FAILED?", which is what this heading said until
        // /review (slice 3). Since the grade went corpus-wide this counts failures on
        // pages in NO listed directory too — nine of the 55 descriptions are in none —
        // and the totals line it feeds says "FAILED above" rather than naming a slice,
        // which was already right. The heading was the stale part.
        //
        // POSITIVE, not "every finding that looks like X". A description finding at
        // Fail is, by construction, a description failure — all of
        // GradeDescription's failing branches, and any further one added later.
        var regressions = descriptions.Count(f => f.Level == FindingLevel.Fail);

        // IS THE PER-PAGE GRADE GATE ACTUALLY WIRED? This is the one corpus-level
        // assertion the promotion created a need for, and it is not a restatement of the
        // per-page Fail: it compares the grades this run MEASURED against the grades it
        // FAILED. Every description above the limit must have been failed by the branch
        // that measured it, so a mismatch means GradeDescription is still reporting the
        // grade and has stopped gating it — which is precisely how this item's gate died
        // the first three times, green and silent. §12.22's lesson is that a gate can be
        // switched off by a fix for something else; this is the cheapest check that says
        // so out loud, and it is one subtraction over findings that already exist.
        //
        // NOT `over > regressions`: that counts the blank and under-floor Fails too, so a
        // corpus with one truncation in a finished slice would mask an ungated over-limit
        // page. The set has to be the same set on both sides of the comparison.
        var overFailing = graded.Count(f =>
            f.Grade > FailGrade && f.Level == FindingLevel.Fail);

        if (over != overFailing)
        {
            yield return new(FindingLevel.Fail, "(corpus)",
                $"{over} page description(s) grade above the {FailGrade:0.0} limit but "
                + $"only {overFailing} of them were FAILED. Since WI-578 the grade is "
                + "gated corpus-wide, so these two numbers are the same number — a gap "
                + $"means {nameof(GradeDescription)} still reports the grade and has "
                + "stopped gating it, which is how this gate died three times while every "
                + "test stayed green.");
        }

        // THE TOTALS, ONCE, VERDICT-FREE, AND ON EVERY RUN. Three separate findings used
        // to live here — a parity Info, a down-count Info, and a Warn for the failing
        // case — and all three were shaped by the two constants that are now gone.
        // Collapsing them into one line is not tidying; each of their conditions was a
        // comparison against a constant that no longer exists, so there is exactly one
        // story left to tell and the only thing that varies is whether somebody is
        // reading it beside a failure.
        //
        // IT SAYS SO WHEN IT PASSES, and that is /review round 3. At parity the old
        // ratchet took none of its branches and contributed the EMPTY SET, so the call
        // site was unobservable — delete it and the suite, the sibling tests and the
        // tool's stdout were all unchanged. **A gate that is silent when it passes cannot
        // be proved to be plugged in**, and the end-to-end test written to prove exactly
        // that was satisfied by GradeDescription alone. Printing unconditionally is what
        // buys the call site an assertion.
        //
        // AND IT CARRIES NO VERDICT, which is round 4's finding and slice 2's. Gating the
        // totals on the failure threw away `tooShort` on exactly the run where somebody
        // is diagnosing a truncation, and PRINTING a verdict beside a Fail — "both
        // unchanged", "lower the constant to lock it in" — is two findings telling
        // opposite stories. Facts with no verdict attached cannot contradict a Fail, so
        // the level rises to Warn beside one and the words do not change.
        //
        // AND THE GUIDANCE SENTENCE USED TO BE FALSE WHEREVER IT PRINTED (/review, slice
        // 2). It read "a truncation shows up here as the floor count RISING while the
        // limit count FALLS", which is the story §12.23's table told — and it printed
        // only when a FINISHED SLICE had failed, where every description is already under
        // the limit, so shortening one CANNOT lower `over`. The falling count belonged to
        // the un-sliced backlog, where a shortening was still read as progress and still
        // not gated. **SLICE 3 REMOVED THE LAST PLACE IT WAS AVAILABLE**: there is no
        // un-sliced backlog left anywhere in the corpus, `over` is 0 and gated on sight,
        // so the floor count is the whole instrument for every page rather than for the
        // 46 in a listed directory. Measured during slice 2 by truncating one page with
        // nothing else changed: 21 → 21 over the limit, 6 → 7 under the floor.
        // AND THE TRUNCATION SENTENCE IS CONDITIONAL NOW, which is §12.24's own finding
        // landing on §12.24's own fix (/review, slice 3). It read, unconditionally,
        // "the limit count will NOT move, BECAUSE a page has to be under the limit
        // before it can be shortened out of grading" — and the clause after `because`
        // is simply false: any page can be shortened out of grading whatever its grade.
        // §12.24's version was safe only because it printed ONLY when a finished slice
        // had failed, where every description really was under the limit. This line
        // prints on every run, including the one where `over` is not 0 — and there,
        // truncating the failing page WOULD take the limit count down by one, which is
        // §12.23's "reads as progress" half, available again, inside the message
        // written to deny it. So the claim is made where it is true and the advice is
        // the honest one where it is not.
        var howATruncationShows =
            $"A truncation shows up here as the {MinimumWordsToGrade}-word floor count "
            + "RISING"
            + (over == 0
                ? ", and the limit count will NOT move: every description in this "
                  + "corpus is already under the limit, so there is nothing for a "
                  + "shortening to take out of the count."
                : $", and with {over} description(s) over the limit it can ALSO take the "
                  + "limit count down by one — a shortening of a failing page reads as "
                  + "progress. Compare both numbers, not one.");

        yield return new(
            regressions > 0 ? FindingLevel.Warn : FindingLevel.Info, "(corpus)",
            (regressions > 0
                ? $"{regressions} description(s) FAILED above, so these totals are "
                  + "reported without a verdict: "
                // THE SUBJECT IS COUNTED AND THE TWO POPULATIONS ARE SEPARATED
                // (/review). This opened "page descriptions, all gated at or under 6.0
                // since WI-578" — no subject count, and "all" was contradicted two
                // clauses later by the six that are not graded at all.
                : $"{descriptions.Count} page description(s), {graded.Count} of them "
                  + $"graded and every one gated at {FailGrade:0.0}: ")
            + $"{over} above the {FailGrade:0.0} limit, worst {worst:0.0}, {tooShort} "
            + $"under the {MinimumWordsToGrade}-word floor and not graded"
            + $"{(blank == 0 ? "" : $", {blank} with no description at all")}. "
            + howATruncationShows);
    }

    /// <summary>
    /// The corpus-level report for glossary DEFINITIONS (WI-582, §12.30) — the three
    /// things no per-entry check can do: say the sweep saw the whole glossary, say the
    /// instrument is alive, and print the totals with the ungraded entries NAMED.
    ///
    /// <para><b>The counts here are reported and never gated</b> (§12.25, and WI-575
    /// round 4's conflation of a corpus size with a defect count). The verdict on a
    /// definition belongs to <see cref="CheckGlossaryTerm"/>, per entry, where it can
    /// also see the word floor. What this method adds is the two failures that are
    /// invisible one entry at a time: a dead instrument, and a gate that measures
    /// without gating.</para>
    ///
    /// <para><b>It does NOT gate the ungraded SET, and that is deliberate.</b> The set
    /// is the escape hatch (<see cref="MinimumDefinitionWordsToGrade"/>), so it wants
    /// pinning by NAME — and a list of four slugs in the tool would be a claim about the
    /// corpus living in a constant, the shape §12.23 made
    /// <c>EveryFinishedSliceIsActuallyCleanOnTheShippedCorpus</c> refuse. It is pinned
    /// in <c>GlossaryDefinitionGradeTests</c> against the real corpus instead; this
    /// method's job is to print the names so a human running the tool can see them.</para>
    ///
    /// <para><paramref name="glossaryTermsWalked"/> is passed rather than inferred, and
    /// WI-575's /review round 2 is why: the description report inferred its walked count
    /// from the findings, the predicate it used could never match, the count was always
    /// zero and the ENTIRE ratchet was dead while seven tests stayed green.</para>
    /// </summary>
    public static IEnumerable<Finding> GlossaryCorpusReport(
        List<Finding> findings, int glossaryTermsWalked)
    {
        // NOT OUR BUSINESS: no glossary in scope at all. A caller may legitimately
        // check the pages or the razor pages alone, and ContentCheckTests does both.
        if (glossaryTermsWalked == 0)
        {
            yield break;
        }

        var definitions = findings
            .Where(f => f.File.EndsWith(DefinitionMarker, StringComparison.Ordinal))
            .ToList();

        // IN SCOPE BUT TOO SMALL TO COMPARE, AND LOUD ABOUT IT. Warn and not Info,
        // because Program.cs renders Info as "  ok" — the mistake WI-578 made once with
        // the description grade itself. What a shrunken glossary threatens is the
        // per-entry gate's REACH: CheckGlossaryTerm runs once per file walked.
        if (glossaryTermsWalked < GlossaryWhenMeasured)
        {
            yield return new(FindingLevel.Warn, "(glossary)",
                $"{glossaryTermsWalked} glossary entr(ies) were walked against the "
                + $"{GlossaryWhenMeasured} this glossary was last measured at, so the "
                + "corpus-level definition totals below DID NOT RUN. Every entry that "
                + "was walked is still graded and gated one entry at a time; what is "
                + "not checked is whether the sweep reached the whole glossary. If the "
                + "glossary legitimately shrank, re-measure and lower "
                + $"{nameof(GlossaryWhenMeasured)} — it is a corpus SIZE and never a "
                + "count of anything that can be fixed.");
            yield break;
        }

        var graded = definitions.Where(f => f.Grade is not null).ToList();

        // THE DEAD INSTRUMENT, and it fails on UNGRADED rather than on ABSENT because
        // that is how this instrument actually dies. Trim every definition under the
        // word floor and `definitions.Count` is still 105 while `graded.Count` is 0 —
        // so "0 above the limit" reads as a total win (§12.23, row 2).
        if (graded.Count == 0)
        {
            yield return new(FindingLevel.Fail, "(glossary)",
                $"{glossaryTermsWalked} glossary entr(ies) were walked and NOT ONE "
                + $"definition was GRADED ({definitions.Count} definition finding(s), "
                + "all of them ungraded). Either CheckGlossaryTerm has stopped "
                + "producing grades, or every definition has fallen under the "
                + $"{MinimumDefinitionWordsToGrade}-word floor — both look like '0 above "
                + "the limit' to a counter, which is why this is a failure and not a "
                + "clean bill.");
            yield break;
        }

        // MATCHED POSITIVELY, never as "everything left over" (§12.22's last lesson,
        // and the line /review found three lines under the comment forbidding it).
        var tooShort = definitions
            .Where(f => f.Message.Contains(TooLittleToGradePhrase, StringComparison.Ordinal))
            .ToList();

        // AND THE TWO SETS MUST PARTITION THE WALK — not `definitions.Count`, which is
        // /review's finding and makes this strictly stronger. CheckGlossaryTerm has a
        // THIRD way out: a FormatException on bad front matter returns one Fail and no
        // marker at all, so that entry drops out of `definitions` entirely. Compared
        // against `definitions.Count` the two sets still partitioned perfectly and the
        // totals line printed "104 glossary definition(s)" over a glossary of 105.
        // Compared against the WALK it cannot: this check now sees both an unparseable
        // entry and a fourth branch added later, and the printed totals are
        // self-validating instead of depending on an unrelated Fail elsewhere in the run.
        if (graded.Count + tooShort.Count != glossaryTermsWalked)
        {
            yield return new(FindingLevel.Fail, "(glossary)",
                $"{glossaryTermsWalked} glossary entr(ies) were walked but the "
                + $"definition findings do not partition them: {graded.Count} graded + "
                + $"{tooShort.Count} under the {MinimumDefinitionWordsToGrade}-word "
                + $"floor = {graded.Count + tooShort.Count}. Either an entry produced no "
                + "definition finding at all (unparseable front matter, which fails "
                + "separately above) or CheckGlossaryTerm has a branch these counts "
                + "cannot see — and the totals printed below are then about fewer "
                + "entries than were checked.");
        }

        var over = graded.Count(f => f.Grade > FailGrade);
        var worst = graded.Max(f => f.Grade!.Value);
        var band = graded.Count(f => f.Grade >= WarnGrade && f.Grade <= FailGrade);

        // IS THE PER-ENTRY GRADE GATE ACTUALLY WIRED? Not a restatement of the
        // per-entry Fail: it compares the grades this run MEASURED against the grades
        // it FAILED. A gap means CheckGlossaryTerm still reports the grade and has
        // stopped gating it — which is precisely how WI-578's gate died three times,
        // green and silent, and how this one was always going to die.
        //
        // AND IT IS A CODE TRIPWIRE RATHER THAN A DATA DETECTOR, which the comment
        // above read as until /review labelled it. Both numbers are derived from the
        // same `Grade > FailGrade` comparison plus a level CheckGlossaryTerm sets from
        // that same comparison, so NO CORPUS can red this — only an edit to the gate
        // can. That is the identical standing the partition check above has and is
        // stated there; the two should read the same way.
        var overFailing = graded.Count(f => f.Grade > FailGrade && f.Level == FindingLevel.Fail);

        if (over != overFailing)
        {
            yield return new(FindingLevel.Fail, "(glossary)",
                $"{over} definition(s) grade above the {FailGrade:0.0} limit but only "
                + $"{overFailing} of them were FAILED. Since WI-582 the definition grade "
                + "is gated, so these two numbers are the same number — a gap means "
                + "CheckGlossaryTerm still reports the grade and has stopped gating it.");
        }

        // THE TOTALS, ONCE, VERDICT-FREE, AND ON EVERY RUN — including the passing one,
        // because a gate that is silent when it passes cannot be proved to be plugged
        // in (§12.23, /review round 3). Facts with no "unchanged" or "lower the
        // constant" framing cannot contradict a Fail printed beside them, so the level
        // rises to Warn next to one and the words do not change.
        //
        // AND THE UNGRADED ENTRIES ARE NAMED RATHER THAN COUNTED (§12.19/§12.24/§12.25).
        // "4 under the floor" is a number nobody can act on and is satisfied by any four
        // entries; the names are what let a reader of the log notice that one of them is
        // not the one that was there last week.
        yield return new(
            over > 0 ? FindingLevel.Warn : FindingLevel.Info, "(glossary)",
            (over > 0
                ? $"{over} definition(s) FAILED above, so these totals are reported "
                  + "without a verdict: "
                : $"{definitions.Count} glossary definition(s), {graded.Count} of them "
                  + $"graded and every one gated at {FailGrade:0.0}: ")
            + $"{over} above the {FailGrade:0.0} limit, worst {worst:0.0}, {band} in the "
            + $"{WarnGrade:0.0}-{FailGrade:0.0} approach band, {tooShort.Count} under the "
            + $"{MinimumDefinitionWordsToGrade}-word floor and not graded"
            + (tooShort.Count == 0
                ? ""
                : " — " + string.Join(", ", tooShort
                    .Select(f => f.File[..^DefinitionMarker.Length])
                    .OrderBy(f => f, StringComparer.Ordinal)))
            + ". A definition shortened under the word floor LEAVES the graded set, "
            + "which takes the limit count down and reads as progress; the set of "
            + "ungraded entries is pinned by NAME in the suite for exactly that reason.");
    }

    private static Finding GradeFinding(double grade, string relativePath) => grade switch
    {
        > FailGrade => new(FindingLevel.Fail, relativePath,
            $"reading grade {grade:0.0} is above the {FailGrade} limit — simplify the language"),
        >= WarnGrade => new(FindingLevel.Warn, relativePath,
            $"reading grade {grade:0.0} is close to the {FailGrade} limit"),
        _ => new(FindingLevel.Info, relativePath, $"reading grade {grade:0.0}"),
    };

    /// <summary>
    /// A whole line reading <c>[Crosswalk]</c> or <c>[crosswalk]</c> is not a
    /// directive — the pattern is uppercase — so it renders as literal
    /// bracket text on a patient's page with nothing else to signal it. When
    /// the token names a block that actually exists, it is a typo rather than
    /// prose, and the build should say so.
    /// </summary>
    private static List<Finding> CheckForMistypedDirectives(
        string raw, string relativePath, ContentBlockSet blocks)
    {
        var findings = new List<Finding>();
        foreach (Match match in MistypedDirectivePattern().Matches(raw))
        {
            var token = match.Groups[1].Value;
            if (token == token.ToUpperInvariant())
            {
                continue; // a real directive, already resolved
            }

            var name = token.ToLowerInvariant();
            if (blocks.Blocks.ContainsKey(name) || blocks.Errors.ContainsKey(name))
            {
                findings.Add(new(FindingLevel.Fail, relativePath,
                    $"'[{token}]' on a line of its own looks like an include of block '{name}' but is not "
                    + $"uppercase, so it renders as literal text — write [{name.ToUpperInvariant()}]"));
            }
        }
        return findings;
    }

    [GeneratedRegex(@"^[ \t]*\[([A-Za-z0-9][A-Za-z0-9-]*)\][ \t]*\r?$", RegexOptions.Multiline)]
    private static partial Regex MistypedDirectivePattern();

    /// <summary>
    /// Marks every block a page reaches, following blocks that include other
    /// blocks — a block used only by another block is still used. Bounded by
    /// the visited set, so an include cycle (already reported as a page
    /// failure) cannot spin here.
    /// </summary>
    private static void RecordUsedBlocks(
        string raw, ContentBlockSet blocks, HashSet<string> used)
    {
        var pending = new Queue<string>(ContentBlocks.DirectBlockNames(raw));
        while (pending.Count > 0)
        {
            var name = pending.Dequeue();
            if (!used.Add(name) || !blocks.Blocks.TryGetValue(name, out var block))
            {
                continue;
            }
            foreach (var nested in ContentBlocks.DirectBlockNames(block.Markdown))
            {
                pending.Enqueue(nested);
            }
        }
    }

    public static List<Finding> CheckPage(string raw, string relativePath, DateOnly today) =>
        CheckPage(raw, relativePath, today, null);

    public static List<Finding> CheckPage(
        string raw, string relativePath, DateOnly today, ContentBlockSet? blocks)
    {
        var findings = new List<Finding>();

        ContentPage page;
        try
        {
            var urlPath = relativePath.EndsWith(".md") ? relativePath[..^3] : relativePath;
            page = ContentStore.Parse(raw, urlPath, [], blocks);
        }
        catch (FormatException exception)
        {
            findings.Add(new(FindingLevel.Fail, relativePath, exception.Message));
            return findings;
        }

        var plainText = ExtractSentences(page.Markdown);
        findings.Add(GradeFinding(ReadabilityAnalyzer.FleschKincaidGrade(plainText), relativePath));

        findings.AddRange(GradeDescription(page, relativePath));

        if (page.FrontMatter.Sources.Count == 0)
        {
            findings.Add(new(FindingLevel.Warn, relativePath,
                "no sources in front matter — every claim must trace (content-pipeline §2)"));
        }

        if (page.FrontMatter.ReviewDue is { } due && due < today)
        {
            findings.Add(new(FindingLevel.Warn, relativePath,
                $"review overdue since {due:yyyy-MM-dd}"));
        }

        // A typo'd flag renders no disclaimer at all — on a medical page that
        // is a safety failure, so it fails the build rather than warning.
        foreach (var flag in page.FrontMatter.Disclaimers.Where(d => !KnownDisclaimers.Contains(d)))
        {
            findings.Add(new(FindingLevel.Fail, relativePath,
                $"unknown disclaimer flag '{flag}' — nothing would render; expected one of {string.Join(", ", KnownDisclaimers)}"));
        }

        return findings;
    }

    /// <summary>
    /// Block-aware plain text for the readability pass: each heading, bullet,
    /// and paragraph becomes its own sentence (terminator appended when
    /// missing) and soft line wraps join with spaces. Feeding raw plaintext
    /// to FK merges heading words into the next sentence and inflates the
    /// grade by 1–2 levels — punishing exactly the structure that helps
    /// impaired readers.
    /// </summary>
    /// <summary>
    /// One sentence, ended once. The block-level walk below has always added a
    /// terminator only when there is not one already; the caption arm did it
    /// unconditionally and produced "…for you.. " (<c>/review</c>). Harmless
    /// for Flesch-Kincaid, which splits on a run of <c>[.!?]</c>, and wrong in
    /// a function whose whole job is to say where sentences end.
    /// </summary>
    private static string Terminated(string text) =>
        text.Length > 0 && text[^1] is not ('.' or '!' or '?') ? text + ". " : text + " ";

    public static string ExtractSentences(string markdown)
    {
        var document = Markdig.Markdown.Parse(markdown, TextPipeline);
        var result = new StringBuilder();

        foreach (var block in document.Descendants().OfType<LeafBlock>())
        {
            if (block.Inline is null)
            {
                continue;
            }

            var text = string.Concat(block.Inline.Descendants().Select(inline => inline switch
            {
                // WI-561: A FIGURE'S CAPTION IS GRADED LIKE ANY OTHER SENTENCE,
                // and so is its alt text.
                //
                // An image is `![alt](src "caption")`, so Markdig keeps the alt
                // text as child literals — which this walk already picked up —
                // and the caption as the link's Title, which is a property and
                // has no inline of its own. So without this line the caption was
                // the one piece of reader-facing prose on a figure that nothing
                // graded, which is exactly WI-575's `description` hole re-dug one
                // surface over. The terminator makes it its own sentence instead
                // of a clause fused to the alt text (§12.20's heading lesson).
                //
                // The alt text is graded too, deliberately: it is prose a screen
                // reader reads aloud to the same audience, at the same reading
                // level, and it was already being graded here by accident. What
                // changes is that both are now true on purpose.
                LinkInline { IsImage: true } image when !string.IsNullOrWhiteSpace(image.Title)
                    => Terminated(image.Title.Trim()),
                LiteralInline literal => literal.Content.ToString(),
                CodeInline code => code.Content,
                // Same gap the alt-text flattener had: an entity is not a
                // literal, and dropped silently it takes a word's characters
                // out of the sample the grade is computed from.
                HtmlEntityInline entity => entity.Transcoded.ToString(),
                LineBreakInline => " ",
                _ => string.Empty,
            })).Trim();

            if (text.Length == 0)
            {
                continue;
            }

            result.Append(text);
            if (text[^1] is not ('.' or '!' or '?'))
            {
                result.Append('.');
            }
            result.Append(' ');
        }

        return result.ToString();
    }

    public static List<Finding> CheckGlossaryTerm(string raw, string slug)
    {
        try
        {
            var term = GlossaryStore.ParseTerm(raw, slug);
            var words = term.Definition.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

            var findings = new List<Finding>
            {
                words > 40
                    ? new(FindingLevel.Fail, $"glossary/{slug}.md",
                        $"definition is {words} words — the editorial limit is 40 (content-pipeline §6)")
                    : new(FindingLevel.Info, $"glossary/{slug}.md", $"ok ({words} words)"),
            };

            // WI-505: same rule and same severity as a curated page — a
            // definition a reader is handed as fact should say where it came
            // from. Warn rather than fail, matching CheckPage.
            if (term.Sources.Count == 0)
            {
                findings.Add(new(FindingLevel.Warn, $"glossary/{slug}.md",
                    "no sources in front matter — every claim must trace (content-pipeline §1)"));
            }

            // A source with a URL and no title renders as a link with no
            // accessible name (WCAG 2.4.4). The page falls back to the host so
            // nothing ships nameless, but that is a patch, not the fix.
            foreach (var source in term.Sources.Where(s => string.IsNullOrWhiteSpace(s.Title)))
            {
                findings.Add(new(FindingLevel.Warn, $"glossary/{slug}.md",
                    $"source '{source.Url}' has no title — the link would have no readable text"));
            }

            // GATED SINCE WI-582 (§12.30), AT THE SAME 6.0 A PAGE IS HELD TO — and the
            // `(not gated)` string is DELETED rather than reworded, the way WI-578
            // deleted the `(not gated — WI-578)` marker together with its ratchet.
            // There is no ungraded-but-reported definition left for it to be true of.
            //
            // WHY A DEFINITION IS HELD TO A PAGE'S STANDARD, decided before a word was
            // edited and then proved by editing them: a definition fires as a tooltip
            // on every page that says the term (§12.26 established it has a BLOCK's
            // blast radius), and for 26 of the 105 entries /glossary is the reader's
            // whole encounter with the word (§12.29). It is reader-facing prose on two
            // surfaces, so it is prose, so it is graded. 35 of the 101 graded entries
            // read above 6.0 when this item started, worst 9.3, median 5.5.
            //
            // AND THE 35 WERE WRITTEN UNDER 6.0 WITH NOT ONE WORD REMOVED, which is
            // §12.22's sentence-length diagnosis holding for a third surface after
            // §12.23/§12.24/§12.25 held it for three directories of descriptions.
            // No medical term was removed, no entry is exempted, and there is no
            // exemption list — see §12.30 for the two entries where the 40-word
            // ceiling and the 6.0 limit bind against each other.
            var where = $"glossary/{slug}.md{DefinitionMarker}";

            if (words < MinimumDefinitionWordsToGrade)
            {
                // REPORTED RATHER THAN SKIPPED, so an ungraded definition is visible
                // instead of absent — the same reason GradeDescription reports its
                // floor branch. Info and not Warn because FOUR entries sit here
                // legitimately today and always have (memantine at 12 words,
                // procarbazine 18, pcv 19, stereotactic-radiosurgery 19), so a Warn
                // would print four expected lines on every run forever.
                //
                // WHAT MAKES THE FLOOR AN EXEMPTION RATHER THAN A LOOPHOLE IS NOT
                // HERE. A per-entry check cannot see an entry ARRIVE in this branch,
                // because arriving looks exactly like belonging. The ungraded set is
                // pinned as a set of named slugs in the suite; this message is how the
                // tool says which entries are in it.
                findings.Add(new(FindingLevel.Info, where,
                    $"{words} word(s) — {TooLittleToGradePhrase}, and "
                    + $"{MinimumDefinitionWordsToGrade} are needed"));
                return findings;
            }

            var grade = ReadabilityAnalyzer.FleschKincaidGrade(term.Definition);

            // FAIL above the limit, WARN in the approach band, Info below — the shape
            // GradeFinding has had for page bodies since WI-414 and GradeDescription
            // took on at WI-578 slice 3, on the same WarnGrade. A third surface
            // disagreeing by a whole level about what "close to the limit" means is
            // three definitions of one idea.
            //
            // THE BAND HAS 19 LIVE SUBJECTS ON THE DAY IT LANDS, and that is reported
            // rather than tidied away. The counter-argument was weighed and rejected:
            // Program.cs reasons that a gate printing dozens of expected warnings
            // trains people to skip it, and 19 of 101 is a fifth of the glossary. But
            // a definition at 5.7 is not a defect and not a backlog — it is correct
            // writing near a line — and the 17 page-body warnings already in this
            // tool's output are the same thing, treated the same way since WI-414.
            // What WI-578 refused to print as `ok` was a grade-19.7 FAILURE.
            var over = grade > FailGrade;
            var level = over ? FindingLevel.Fail
                : grade >= WarnGrade ? FindingLevel.Warn
                : FindingLevel.Info;

            findings.Add(new(level, where,
                $"reading grade {grade:0.0}"
                + (over
                    ? $", above the {FailGrade:0.0} limit — split a sentence before "
                      + "simplifying a word: the cause is words-per-sentence and not "
                      + "vocabulary, measured three times (§12.22, §12.23, §12.30)"
                    : level == FindingLevel.Warn
                        ? $" is close to the {FailGrade:0.0} limit"
                        : ""),
                grade));

            return findings;
        }
        catch (FormatException exception)
        {
            return [new(FindingLevel.Fail, $"glossary/{slug}.md", exception.Message)];
        }
    }
}
