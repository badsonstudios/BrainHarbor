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

        // WI-575: COUNTED, NOT INFERRED. The description ratchet needs to know whether
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

        if (glossaryRoot is not null && Directory.Exists(glossaryRoot))
        {
            foreach (var file in Directory.EnumerateFiles(glossaryRoot, "*.md")
                         .OrderBy(f => f, StringComparer.Ordinal))
            {
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
        // per-page check produced rather than re-walking the corpus. The
        // description's reading grade cannot be gated today (41 of 49 are above the
        // limit, and the rewrite is WI-578), so what is gated is drift.
        // MATERIALISED FIRST. `AddRange` mutates `findings` while the iterator holds
        // a reference to it; this is safe today only because the single enumeration
        // completes before the first `yield`, and one more `findings.Where(...)`
        // after a yield would throw at runtime (/review round 4).
        findings.AddRange(DescriptionRatchet(findings, curatedPagesWalked).ToList());

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

        // A BLANK DESCRIPTION IS REPORTED, not skipped. `FrontMatter.Description`
        // defaults to "" and nothing required it, so a page with no first paragraph was
        // legal by the tool's own rules — while ShellPagesTests asserts every page on
        // disk produces a description finding, and the ratchet's dead-instrument Fail
        // could fire on a legal description-less corpus. Warning here closes the cause
        // rather than the symptom: with pages walked, no descriptions at all is now
        // impossible, and the Fail below reduces to the genuine case. (/review round 4.)
        if (string.IsNullOrWhiteSpace(description))
        {
            return [new(FindingLevel.Warn, where,
                NoDescriptionPrefix
                + " — ContentPage.cshtml renders it as the first paragraph a "
                + "reader meets and SearchPages renders it again as the blurb under every "
                + "hit, so a page without one is a page whose opening is unwritten")];
        }

        var text = ExtractSentences(description);
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        // Reported rather than skipped, so a description too short to grade is
        // visible instead of silently ungraded — which is the whole defect this
        // method exists to close. The grade is deliberately left null here, and the
        // ratchet counts these separately: a description that falls UNDER the word
        // floor has not been improved, and scoring it as progress would let a page
        // buy its way out of grading by getting shorter (/review round 1).
        if (words < MinimumWordsToGrade)
        {
            return [new(FindingLevel.Info, where, $"{words} word(s) — too little to grade")];
        }

        // REPORTED, NOT GATED, and the reason is a measurement rather than a
        // preference. 41 of 49 gradeable descriptions are above the 6.0 limit
        // (median 8.4, max 19.7), and the cause is not vocabulary: 45 of the 55 are
        // a contents list in one or two comma-spliced sentences, and the longest
        // sentence runs to a median of 27 words across the corpus and a maximum of
        // 51. The WORDS are simple; FK's words-per-sentence term is doing all of it. Failing the build on 41
        // pages would make this gate the thing people route around, which is the
        // failure mode this file's own comment about admin pages warns of.
        //
        // The same call this file already makes for glossary definitions. The
        // rewrite is WI-578, raised WITH the measurement. What IS gated is drift:
        // see <see cref="DescriptionRatchet"/>.
        var grade = ReadabilityAnalyzer.FleschKincaidGrade(text);

        // WARN above the limit, Info below it. Info renders as "  ok" (Program.cs), so
        // reporting a grade-19.7 description as Info discharged "says out loud why it
        // does not gate" by printing `ok` next to the worst line in the corpus
        // (/review round 1). Warn does not fail the build, so the 41 are visible
        // without the gate becoming the thing people route around.
        return
        [
            new(grade > FailGrade ? FindingLevel.Warn : FindingLevel.Info, where,
                $"reading grade {grade:0.0} (not gated — WI-578)", grade),
        ];
    }

    /// <summary>
    /// WI-575: how many page descriptions sat above <see cref="FailGrade"/> when the
    /// grade was first measured. A forty-second fails the build — which was NOT true
    /// for one review round, because the precondition below was inferred from a field
    /// nothing populated (/review round 2).
    ///
    /// <para><b>A non-gate is not nothing.</b> The grade itself cannot be gated today
    /// without failing 41 pages, so what is gated is DRIFT: the corpus may not acquire
    /// another unreadable description while the rewrite (WI-578) is outstanding. One
    /// number the code enumerates, not a per-page table that rots — and it ratchets
    /// down on its own as WI-578 lands, because the assertion is an upper bound.</para>
    ///
    /// <para>Deliberately NOT a ratchet on the grade values themselves. The worst is
    /// 19.7, so a ceiling at today's maximum would gate nothing, and §12.19's rule is
    /// that a guard measured on a page it cannot fire on has been measured on
    /// nothing.</para>
    /// </summary>
    public const int DescriptionsOverTheLimit = 41;

    /// <summary>
    /// The worst description grade when WI-575 measured it. <b>No description may exceed
    /// it</b>, which closes the hole a pure count leaves.
    ///
    /// <para>/review round 1: a count permits unbounded worsening — push any of the 41
    /// from 8.4 to 40.0 and the count does not move. A ceiling at today's maximum gates
    /// that, and unlike a per-page table it is one number. It is NOT a substitute for the
    /// count: fixing one description and adding a bad one leaves both unchanged, so the
    /// two numbers catch different regressions and both are asserted.</para>
    /// </summary>
    public const double WorstDescriptionGrade = 19.7;

    /// <summary>The marker <see cref="GradeDescription"/> appends to a description
    /// finding's file path, so the four places that recognise one agree by construction
    /// rather than by four literals (/review round 2).</summary>
    public const string DescriptionMarker = " [description]";

    /// <summary>
    /// The number of curated pages the ratchet's two constants were measured on
    /// (WI-575). A smaller corpus is not the shipped one and those constants say
    /// nothing about it.
    ///
    /// <para><b>NOT <see cref="DescriptionsOverTheLimit"/>, which is what the first
    /// version used.</b> That number is a count of bad descriptions, not a corpus size,
    /// and WI-578's acceptance criteria — printed in this file's own messages — say to
    /// lower it as the work lands. At 3 the description-less test fixtures would start
    /// failing; at 0 the precondition would never fire at all and a glossary-only run
    /// would fall through to the dead-instrument Fail. <b>Two independent facts were
    /// being tested with one number</b> (/review round 4), which re-armed the blockers
    /// of rounds 2 and 3 with no code change at all — just by following the
    /// instructions.</para>
    /// </summary>
    public const int CorpusWhenMeasured = 55;

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
    /// The ratchet, run once over the whole corpus rather than per page — the property
    /// is a corpus count and a per-page check cannot see it.
    ///
    /// <para>Two numbers, because they catch different regressions: the COUNT cannot see
    /// a description getting worse, and the CEILING cannot see a good one being swapped
    /// for a bad one. <paramref name="curatedPagesWalked"/> is passed rather than
    /// inferred — see the precondition below for what inferring it cost.</para>
    ///
    /// <para>(This docstring was stacked on <see cref="WorstDescriptionGrade"/> for a
    /// round, which is the defect /review round 1 fixed elsewhere in this item
    /// recreated by its own fix. A duplicate <c>&lt;summary&gt;</c> raises no compiler
    /// warning and only one survives into IntelliSense.)</para>
    /// </summary>
    public static IEnumerable<Finding> DescriptionRatchet(
        List<Finding> findings, int curatedPagesWalked)
    {
        var descriptions = findings
            .Where(f => f.File.EndsWith(DescriptionMarker, StringComparison.Ordinal))
            .ToList();

        // WAS A CURATED PAGE WALKED AT ALL? "No descriptions" has two causes and only
        // one is a defect. A caller can legitimately scope CheckAll to the glossary or
        // to razor pages alone — ContentCheckTests does both — and the ratchet has no
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
        if (curatedPagesWalked < CorpusWhenMeasured)
        {
            yield return new(FindingLevel.Warn, "(corpus)",
                $"{curatedPagesWalked} curated page(s) were walked against the "
                + $"{CorpusWhenMeasured} this ratchet's constants were measured on, so "
                + "THE RATCHET DID NOT RUN. If the corpus legitimately shrank, re-measure "
                + $"and lower {nameof(DescriptionsOverTheLimit)} and "
                + $"{nameof(WorstDescriptionGrade)} together with "
                + $"{nameof(CorpusWhenMeasured)}.");
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
        var tooShort = descriptions.Count - graded.Count - blank;
        var over = graded.Count(f => f.Grade > FailGrade);
        var worst = graded.Max(f => f.Grade!.Value);

        if (worst > WorstDescriptionGrade)
        {
            yield return new(FindingLevel.Fail, "(corpus)",
                $"a page description now grades {worst:0.0}, and the worst when WI-575 "
                + $"measured the corpus was {WorstDescriptionGrade:0.0}. A count of "
                + "descriptions over the limit cannot see one getting worse, so this is "
                + "the other half of the ratchet. The cause is always sentence length: "
                + "split it.");
        }

        if (over > DescriptionsOverTheLimit)
        {
            yield return new(FindingLevel.Fail, "(corpus)",
                $"{over} page descriptions are above the {FailGrade:0.0} reading limit and "
                + $"{DescriptionsOverTheLimit} were when WI-575 measured them. The "
                + "description is the first paragraph a reader meets and nothing graded it "
                + "until WI-575; the existing backlog is WI-578's, but the corpus may not "
                + "grow it. Shorten the sentence you just added — the cause is always "
                + "sentence length, not vocabulary.");
        }

        // AND THE OTHER DIRECTION IS REPORTED, so WI-578's progress is visible — with
        // the too-short count beside it, because a description that drops under the
        // word floor stops being graded rather than getting better and would
        // otherwise read as a gain (/review round 1).
        // AND IT SAYS SO WHEN IT PASSES. /review round 3: at parity the ratchet took
        // none of its branches and contributed the EMPTY SET, so the call site was
        // unobservable — delete it and the suite, the sibling tests and the tool's
        // stdout were all unchanged. **A gate that is silent when it passes cannot be
        // proved to be plugged in**, and the end-to-end test written to prove exactly
        // that was satisfied by GradeDescription alone. One Info in a 338-line run buys
        // the call site an assertion and the log a line saying the gate is alive.
        if (over == DescriptionsOverTheLimit && worst <= WorstDescriptionGrade)
        {
            yield return new(FindingLevel.Info, "(corpus)",
                $"{over} page descriptions are above the {FailGrade:0.0} reading limit and "
                + $"the worst grades {worst:0.0} — both unchanged since WI-575 measured "
                + $"them ({tooShort} more are under the {MinimumWordsToGrade}-word floor "
                + $"and are not graded{(blank == 0 ? "" : $"; {blank} page(s) have NO "
                    + "description at all")}). WI-578 owns the rewrite.");
        }

        // GATED ON THE CEILING TOO, so a failing run does not also print "down from 41
        // — lower the constant to lock it in". When the ceiling has been breached the
        // Fail is the news, and an Info inviting somebody to ratchet down beside it is
        // two findings telling opposite stories (/review round 4).
        if (over < DescriptionsOverTheLimit && worst <= WorstDescriptionGrade)
        {
            yield return new(FindingLevel.Info, "(corpus)",
                $"{over} page descriptions are above the {FailGrade:0.0} limit, down from "
                + $"{DescriptionsOverTheLimit} ({tooShort} are under the "
                + $"{MinimumWordsToGrade}-word floor and are not graded at all — check the "
                + $"gain is a rewrite and not a truncation"
                + $"{(blank == 0 ? "" : $", and {blank} page(s) have NO description")}). "
                + $"Lower {nameof(DescriptionsOverTheLimit)} to lock it in (WI-578).");
        }
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
                LiteralInline literal => literal.Content.ToString(),
                CodeInline code => code.Content,
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

            // Reported, not gated. Flesch-Kincaid on a 25-word definition is
            // too noisy to fail a build on — the same reason CheckRazorPage
            // refuses to grade under 25 words — but a definition drifting to
            // grade 9 should still be visible to whoever runs this.
            if (words >= 20)
            {
                findings.Add(new(FindingLevel.Info, $"glossary/{slug}.md",
                    $"reading grade {ReadabilityAnalyzer.FleschKincaidGrade(term.Definition):0.0} (not gated)"));
            }

            return findings;
        }
        catch (FormatException exception)
        {
            return [new(FindingLevel.Fail, $"glossary/{slug}.md", exception.Message)];
        }
    }
}
