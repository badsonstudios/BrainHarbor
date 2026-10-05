using System.Text.RegularExpressions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-575, acceptance criterion 3: <b>the corpus swept ONCE with the widened reader
/// text</b>, because a rule that has never read the descriptions has never been tested
/// against them.
///
/// <para><b>THE HOLE.</b> <c>ContentPage.cshtml</c> renders the front-matter
/// <c>title</c> as the heading and <c>description</c> as the first paragraph under it,
/// and <c>ContentStore.SearchPages</c> puts the description in front of a reader a
/// second time as the blurb under every hit. But <c>CuratedPage.ReaderText</c> strips
/// the front matter by design, so every prose guard built on it is body-only. Four
/// items fell through: WI-524, WI-528, WI-567 — which paid a <c>/review</c> blocker
/// that survived three rounds inside that one line — and <b>WI-573 twice in one
/// item</b>.
/// </para>
///
/// <para><b>AND THE RESULT IS THE ITEM'S CENTRAL FINDING: THE TEXT IS CLEAN.</b> Swept
/// for every rule the corpus already enforces on its body, all 55 headlines carry no
/// British form, no em or en dash, no smart quote, no non-breaking space, no percentage
/// and no Roman-numeral grade. So the pain those four items felt was <b>guards not
/// reading the description</b>, not the description being wrong — which is worth knowing,
/// because it means the fix was structural (one helper, this sweep) rather than a content
/// job.</para>
///
/// <para><b>THE ONE RULE THAT WAS NEVER APPLIED IS THE ONE THAT FINDS SOMETHING.</b>
/// Nothing graded the description until WI-575, and <b>41 of 49 gradeable descriptions
/// were above the 6.0 reading limit</b> (median 8.4, max 19.7). The cause was not
/// vocabulary — <b>45 of the 55</b> were a contents list in one or two comma-spliced
/// sentences, and the longest sentence ran to a median of 27 words across the corpus
/// (28 across the gradeable 49) and a maximum of 51. That is <b>WI-578</b>, raised with
/// the measurement attached; <c>ContentChecker</c> reports the grade ungated and
/// ratchets both the count and the worst grade, so the corpus can neither grow the
/// backlog nor make one worse.</para>
///
/// <para><b>WI-578's first slice rewrote <c>treatments/</c> and the sentence-length
/// diagnosis held exactly.</b> All thirteen descriptions there now pass — the
/// directory's worst fell 19.7 → 5.3, median longest sentence 33 → 17 words — <b>with
/// not one word removed</b>: every word count stayed equal or rose, and the too-short
/// tally beside the ratchet's gain never moved off 6. The edits were a comma or colon
/// becoming a full stop, <i>plus the connective word a new sentence needs</i> — and
/// that second half is not a quibble: two of the thirteen traded a word rather than
/// only gaining one (<c>targeted-therapy</c> dropped an <i>and</i> mid-clause and
/// regained it opening the next sentence), so the claim that holds is the AGGREGATE
/// one about word counts, not a claim that nothing but punctuation moved.
/// <b>29</b> remain, 8 in <c>tests/</c> and 21 in <c>tumors/</c>, and this sweep's
/// clean-text finding above is unaffected: splitting a sentence introduces no
/// typography.</para>
///
/// <para><b>EVERY RULE HERE HAS A CANARY.</b> A sweep that is green over new text and has
/// never been seen to fire has not been shown to work (§12.18), and a sweep whose subject
/// is clean is exactly where that goes unnoticed — §12.19: <i>a guard measured on a page
/// it cannot fire on has been measured on nothing.</i> Each check below runs over the
/// corpus AND over a planted headline that it must catch.</para>
/// </summary>
public sealed class HeadlineSweepTests
{
    /// <summary>
    /// Every page's headline, flattened, keyed by slug — through
    /// <see cref="CuratedPage.Headline"/>, which is the promoted helper this item
    /// replaced every hand-written copy with. Enumerated from the diff: <b>23 sites</b>
    /// parsed the front matter by hand — 20 reading both fields (19 of them a private
    /// `Headline` property) and 3 reading the description alone. Zero remain.
    /// </summary>
    private static List<(string Slug, string Headline)> Headlines() =>
        [.. HeadlinesRaw().Select(h => (h.Slug, CuratedPage.Flatten(h.Headline)))];

    /// <summary>
    /// The same headlines, <b>NOT flattened</b> — for the checks whose subject is a
    /// CHARACTER rather than a word.
    ///
    /// <para><b>This exists because the break harness found the typography check unable
    /// to see a non-breaking space.</b> <see cref="CuratedPage.Flatten"/> is
    /// <c>Regex.Replace(page, @"\s+", " ")</c> and <b>.NET's <c>\s</c> matches
    /// U+00A0</b> — so flattening replaced the character the check hunts for, with a
    /// plain space, before the check ran. It was green because its subject had been
    /// erased upstream.</para>
    ///
    /// <para><b>A normaliser upstream of a character check destroys its subject.</b></para>
    ///
    /// <para><b>And the reason for keeping a flattened variant at all is NOT the one the
    /// first version of this comment gave.</b> It said the corpus is hard-wrapped so a
    /// two-word British form has a newline in the middle of it — true of BODY text
    /// (§12.8, WI-511) and structurally impossible here, because <c>Title</c> and
    /// <c>Description</c> both capture <c>(.+)</c> from a single line with no
    /// <c>Singleline</c>: <b>a headline cannot contain a newline.</b> Measured,
    /// <c>Flatten</c> is a no-op on all 55 headlines today. It is kept as defence
    /// against a run of spaces — and the fact that the only thing it COULD change is a
    /// space or an NBSP is exactly why the character checks must not use it.</para>
    /// </summary>
    private static List<(string Slug, string Headline)> HeadlinesRaw()
    {
        var root = CuratedPage.PagesDirectory;
        var headlines = new List<(string Slug, string Headline)>();

        foreach (var file in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var slug = Path.GetRelativePath(root, file).Replace('\\', '/')[..^3];

            // RAW BYTES, NOT NORMALISED, and this is the whole reason the sweep is
            // coverage for the `\s*$` anchor rather than a claim about it.
            //
            // The first version did `.Replace("\r\n", "\n")` first — so the only two
            // whole-corpus callers of `Headline` were immune to the exact CRLF trap the
            // promotion exists for, while a comment below claimed the sweep was that
            // coverage (/review round 2). This checkout is `text=auto` and every content
            // file on disk is CRLF, so reading the bytes means all 55 pages exercise the
            // anchor on every run.
            //
            // **A sweep that normalises its subject cannot exercise a normalisation
            // defect** — the same shape as round 1's NBSP finding, where `Flatten`
            // erased the character a check was hunting for.
            headlines.Add((slug, CuratedPage.Headline(File.ReadAllText(file), slug)));
        }

        // THE FLOOR LIVES HERE, not in one test. Three of the five checks below had
        // no floor of their own and relied on a sibling test asserting the count —
        // which `dotnet test --filter` defeats, and which means each of them passed
        // just as happily over an empty enumeration (/review round 1). §12.17: an
        // iterate-and-check guard says out loud how much it looked at, and the
        // cheapest way to give every caller that is to put it in the enumerator.
        Assert.True(headlines.Count >= 50,
            $"only {headlines.Count} headline(s) built from {root} — every check in "
            + "this file would pass vacuously, so this is a failure of the sweep and "
            + "not of the corpus");

        return headlines;
    }

    /// <summary>
    /// <c>Headline</c> SUCCEEDS ON THE BYTES AS THEY SIT ON DISK — which is what makes
    /// this sweep coverage for the <c>\s*$</c> anchor, and it is narrower coverage than
    /// the first version of this comment claimed.
    ///
    /// <para>/review round 2 emptied the first version of this test out: with the count
    /// floor moved into <see cref="HeadlinesRaw"/> and <c>Headline</c>'s non-empty
    /// asserts firing inside it, every line asserted something the enumerator already
    /// did. The replacement then asserted that a headline built from CRLF bytes would
    /// CONTAIN a carriage return — which it never can, because <c>Headline</c> returns
    /// the <c>(.+)</c> capture and that stops before the <c>\r</c>. The assertion could
    /// only ever fail, and it did.</para>
    ///
    /// <para><b>THE HONEST SCOPE.</b> This repo is <c>text=auto</c>: a Windows working
    /// tree holds the corpus as CRLF and the Linux CI runner checks it out as LF. So
    /// reading the raw bytes exercises the anchor <b>on a CRLF checkout only</b> — real
    /// coverage, not universal coverage. With <c>\s*$</c> replaced by <c>"$</c>, a
    /// fully-LF corpus passes the whole suite and a fully-CRLF one fails in the
    /// hundreds; the ending suite is what covers the case this cannot.</para>
    ///
    /// <para>What is asserted is therefore the one thing that distinguishes a working
    /// anchor from a broken one without depending on which checkout this is:
    /// <c>Headline</c> reads a non-empty title and description from the file's own
    /// bytes, un-normalised, for every page.</para>
    /// </summary>
    [Fact]
    public void HeadlineReadsATitleAndDescriptionOnBothLineEndings()
    {
        // THE CANARIES DO THE WORK, AND THEY ARE CHECKOUT-INDEPENDENT. /review round 3
        // emptied the previous version out twice: asserting a non-empty headline is
        // implied by `Headline`'s own two asserts, and comparing two enumerations of
        // the same directory cannot detect whether one of them normalised its input.
        //
        // These two CAN fail, on any machine, and they kill the `"$` mutation
        // outright: the CRLF fixture is exactly what the anchor exists for, and the LF
        // one proves the fix did not simply invert the bug.
        const string Crlf = "---\r\ntitle: \"T\"\r\ndescription: \"D\"\r\n---\r\n\r\nBody.\r\n";

        Assert.Equal("T D", CuratedPage.Headline(Crlf, "(crlf canary)"));
        Assert.Equal("T D", CuratedPage.Headline(Crlf.Replace("\r\n", "\n"), "(lf canary)"));

        // AND THE CORPUS GOES THROUGH THE SAME HELPER, un-normalised, so on a CRLF
        // working tree every page exercises the anchor — but through the ENUMERATOR,
        // which already does exactly this walk and carries the count floor. The first
        // version repeated the walk inline, byte for byte, as a fourth identical
        // traversal in this file (/review round 4).
        Assert.True(HeadlinesRaw().Count >= 50);
    }

    /// <summary>
    /// NO BRITISH FORMS in the title or the description.
    ///
    /// <para>The list is <see cref="CuratedPage.BritishForms"/> — the corpus's own, which
    /// has grown six times — rather than a copy, because a retyped list is a different
    /// list. <see cref="CuratedPage.BritishFormExemptions"/> is stripped first: "The Brain
    /// Tumour Charity" is an organization's actual name and Americanising a citation
    /// would be a worse defect than the one this prevents.</para>
    /// </summary>
    [Fact]
    public void NoHeadlineUsesABritishForm()
    {
        // A FLOOR ONE UNDER THE LIST, not nine under it. The list has grown six times
        // and only ever grows, so a floor with slack is a floor that lets nine entries
        // be deleted silently (/review round 3). §12.18: measure the headroom.
        Assert.True(CuratedPage.BritishForms.Length >= 58,
            $"the British-forms list has {CuratedPage.BritishForms.Length} entries and had "
            + "59 when this sweep was written. The list only grows; a drop means entries "
            + "were deleted, and this sweep would go quiet about whatever they caught.");

        var offenders = new List<string>();
        foreach (var (slug, headline) in Headlines())
        {
            offenders.AddRange(BritishFormsIn(headline).Select(f => $"{slug}: \"{f}\""));
        }


        Assert.True(offenders.Count == 0,
            "a title or description uses a British form. This is the first paragraph a "
            + "reader meets and nothing read it until WI-575:\n  "
            + string.Join("\n  ", offenders));

        // THE CANARY. The corpus is clean here, so without this the test is green over
        // text it might not be reading at all.
        Assert.Contains("tumour",
            BritishFormsIn("Brain tumour grades What a tumour grade means for you"));
        Assert.Empty(BritishFormsIn("Brain tumor grades What a tumor grade means for you"));

        // AND THE EXEMPTION STILL WORKS, so the canary above is not passing because the
        // stripper is broken — in BOTH casings, which is the bug the first version had.
        Assert.Empty(BritishFormsIn("A page The Brain Tumour Charity says so"));
        Assert.Empty(BritishFormsIn("A page THE BRAIN TUMOUR CHARITY says so"));
        Assert.Empty(BritishFormsIn("A page the brain tumour charity says so"));
    }

    private static List<string> BritishFormsIn(string headline)
    {
        // ORDINAL-IGNORE-CASE, like the form match below it and like the ~38 existing
        // per-page scans. The first version stripped exemptions case-sensitively while
        // matching forms case-insensitively, so a citation written "The Brain tumour
        // Charity" — or any title-cased heading — would have failed a correct page
        // (/review round 1). §12.8: a rule that fails a correct page is worse than no
        // rule, and an exemption list is where that happens quietly.
        var scanned = headline;
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            scanned = scanned.Replace(exemption, " ", StringComparison.OrdinalIgnoreCase);
        }

        return [.. CuratedPage.BritishForms
            .Where(f => scanned.Contains(f, StringComparison.OrdinalIgnoreCase))];
    }

    /// <summary>
    /// NO TYPOGRAPHIC CHARACTERS in the title or the description.
    ///
    /// <para>Em and en dashes, curly quotes and non-breaking spaces are banned in all
    /// Content markdown, front matter included — <c>blocks/hemangioblastoma</c>'s own note
    /// says so and WI-573 met the en dash and the non-breaking space in a source
    /// quotation. They matter more here than in the body: the description is
    /// re-rendered as a search-result blurb, and a character that survives one template
    /// and not the other is invisible to whoever wrote it.</para>
    /// </summary>
    [Fact]
    public void NoHeadlineCarriesATypographicCharacter()
    {
        // UNFLATTENED — see HeadlinesRaw. Flatten() would replace the
        // non-breaking space this check hunts for with a plain one.
        var offenders = new List<string>();
        foreach (var (slug, headline) in HeadlinesRaw())
        {
            offenders.AddRange(TypographyIn(headline).Select(c => $"{slug}: {c}"));
        }

        Assert.True(offenders.Count == 0,
            "a title or description carries a character the corpus bans everywhere:\n  "
            + string.Join("\n  ", offenders));

        // BOTH CANARIES, one per direction, and the characters are written as escapes
        // rather than pasted — §12.20's rule, because pasted they are indistinguishable
        // from a hyphen and a straight quote in a diff.
        Assert.NotEmpty(TypographyIn("A page One thing \u2014 and another"));
        Assert.NotEmpty(TypographyIn("A page It\u2019s here"));
        Assert.NotEmpty(TypographyIn("A page 6\u201345 years"));
        Assert.Empty(TypographyIn("A page One thing - and another. It's here. 6-45 years."));

        // EVERY ROW OF THE TABLE FIRES. The class docstring says every rule here has a
        // canary, and three rows — the two left quotes and the right double quote — had
        // none until /review round 4 counted them. Driven off the table itself, so a row
        // added later cannot arrive uncanaried.
        foreach (var (ch, name) in TypographyTable())
        {
            Assert.NotEmpty(TypographyIn($"A page x{ch}y"));
            Assert.Empty(TypographyIn("A page xy"));
            Assert.Contains("U+", name);
        }

        // AND THE CANARY THE BREAK HARNESS ASKED FOR. The non-breaking space is
        // written as a C# escape here ON PURPOSE -- pasted, it is indistinguishable
        // from a plain space in a diff, which is how it reached a description
        // unnoticed in the first place.
        //
        // THE SECOND LINE IS THE BUG THIS CHECK WAS WRITTEN WITH: the same text
        // FLATTENED loses the character, because Flatten is `\s+` and .NET's `\s`
        // matches U+00A0. Asserting it is what stops a later tidy-up reintroducing
        // Flatten here and quietly erasing this check's subject.
        Assert.NotEmpty(TypographyIn("A page Thirty\u00A0years."));
        Assert.Empty(TypographyIn(CuratedPage.Flatten("A page Thirty\u00A0years.")));
    }

    /// <summary>
    /// The characters and their names, extracted so the canary loop above can drive off
    /// the same table the check uses — a row cannot be added without being canaried
    /// (/review round 4).
    /// </summary>
    private static (char Ch, string Name)[] TypographyTable() =>
        [
                ('\u2014', "EM DASH (U+2014)"),
                ('\u2013', "EN DASH (U+2013)"),
                ('\u2018', "LEFT SINGLE QUOTE (U+2018)"),
                ('\u2019', "RIGHT SINGLE QUOTE (U+2019)"),
                ('\u201C', "LEFT DOUBLE QUOTE (U+201C)"),
                ('\u201D', "RIGHT DOUBLE QUOTE (U+201D)"),
                ('\u00A0', "NON-BREAKING SPACE (U+00A0)"),
        ];

    private static List<string> TypographyIn(string headline) =>
        [.. TypographyTable()
            .Where(p => headline.Contains(p.Ch))
            .Select(p => p.Name)];

    /// <summary>
    /// THE CORPUS'S OWN SAFETY RULE, RUN OVER THE WIDENED TEXT FOR EVERY PAGE — and
    /// this is what <see cref="CuratedPage.EverythingAReaderMeets"/> exists for.
    ///
    /// <para><c>CuratedPage.AssertNeverMinimises</c> forbids calling something a "simple
    /// procedure" or a "routine treatment" without negating it in the same clause. It is
    /// called from most page test classes and <b>the large majority of those calls pass
    /// that page's BODY</b>, so a minimisation in a description had never been checked —
    /// on the line a reader reads first, and on the blurb under every search hit.</para>
    ///
    /// <para><b>The first version of this paragraph said "called 46 times and every call
    /// passes that page's BODY".</b> Measured on <c>develop</c>: 39 call sites across 38
    /// files, and SIX already pass headline-inclusive text (<c>Plain</c>, which is
    /// <c>Headline + " " + Body</c> in those files). §12.20 — never write a count in
    /// prose about something the code can enumerate — and this one was the sweep's own
    /// argument. The argument survives the correction: those calls cover the pages that
    /// have a test class, and this covers every page in the corpus.</para>
    ///
    /// <para><b>Swept here rather than by editing the call sites</b>, because the blast
    /// radius of re-scoping every one of them is far larger than the hole being closed
    /// (§12.21's argument for leaving <c>ReaderText</c> alone). It is acceptance
    /// criterion 3 exactly — <i>the corpus swept once with the widened reader
    /// text</i>.</para>
    ///
    /// <para><b>The cost was measured before it was taken:</b> zero. No headline in the
    /// corpus carries a minimisation, negated or otherwise, so this widening fails no
    /// correct page — which §12.8 says is the thing that matters more than what it
    /// catches.</para>
    /// </summary>
    [Fact]
    public void NoPageMinimisesAnythingInTheTextAReaderMeetsFirst()
    {
        var swept = 0;
        var root = CuratedPage.PagesDirectory;

        foreach (var file in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var slug = Path.GetRelativePath(root, file).Replace('\\', '/')[..^3];
            // NORMALISED HERE, and deliberately: this check's subject is PHRASING, and
            // `AssertNeverMinimises` flattens internally anyway. The rule that a sweep
            // must not normalise applies to a check whose subject is a character or a
            // line ending — see HeadlinesRaw. Said out loud so the next reader neither
            // "fixes" it nor cites it as anchor coverage (/review round 4).
            var raw = File.ReadAllText(file).Replace("\r\n", "\n");

            // THE WIDENED TEXT, through the helper that exists for it. /review round 1
            // found this hand-rolling headline + composed body while three docstrings
            // said EverythingAReaderMeets was what it was for — and found that helper
            // reading the UNCOMPOSED body, so a rule taking it at its word would have
            // gone blind to every shared block. Both fixed; this is now its consumer,
            // which is the difference between a promoted helper and dead code.
            CuratedPage.AssertNeverMinimises(CuratedPage.EverythingAReaderMeets(raw, slug), slug);
            swept++;
        }

        Assert.True(swept >= 50,
            $"only {swept} page(s) swept, so this is reading the wrong directory and "
            + "every page above passed vacuously");

        // THE CANARY, because the corpus is clean and a rule that has never been seen to
        // fire has not been shown to work (§12.18). Both directions: the minimisation
        // without its negation must throw, and WITH the negation must not — the second
        // is the load-bearing one, since §12.8's rule is that a guard which fails a
        // correct page is worse than no guard.
        // Throws<TrueException>, not ThrowsAny<Exception>: the latter also passes when
        // the helper explodes with a NullReferenceException, which would report a
        // crashed guard as a working one (/review round 1).
        Assert.Throws<Xunit.Sdk.TrueException>(() => CuratedPage.AssertNeverMinimises(
            "A page A biopsy is a simple procedure and nothing to worry about.", "(canary)"));

        CuratedPage.AssertNeverMinimises(
            "A page There is no such thing as a simple procedure here.", "(canary)");

        // And the contracted negative, which §12.8 and §12.20 record as the form a
        // group-leading word boundary silently broke — WI-571's /review round 18.
        // (Cited as §12.21 in the first version; that section is WI-573's and says
        // nothing about it. /review round 2.)
        CuratedPage.AssertNeverMinimises(
            "A page It isn't a simple procedure and nobody should say so.", "(canary)");
    }

    /// <summary>
    /// NO PERCENTAGE AND NO ROMAN-NUMERAL GRADE in the title or the description.
    ///
    /// <para>Both are corpus-wide bans on reader text — a percentage because §12.4's
    /// anti-hype rules refuse a figure a reader measures themselves against, and Roman
    /// grades because the corpus teaches WHO grades in Arabic numerals throughout.</para>
    ///
    /// <para><b>PLAIN NUMERALS ARE ALLOWED, and the live ones are why this is not a
    /// digit ban.</b> Every numeral in the corpus's headlines is legitimate: WHO grades
    /// in Arabic ("grade 3 or grade 4"), "about 1 in 4" for the inherited condition on
    /// <c>/tumors/hemangioblastoma</c>, and <c>H3 K27</c> — a mutation name, which is the
    /// case a digit ban gets wrong for a reason that has nothing to do with grades.
    /// Banning digits outright would fail correct pages, which §12.8 says is worse than no
    /// rule. (This said "the nine live ones" and named "the four lobes", which is not a
    /// digit and is not there. /review round 1, and round 4 for the fact that the
    /// correction then stated a count in the clause explaining that the count is left
    /// out. The sweep enumerates it.)</para>
    /// </summary>
    [Fact]
    public void NoHeadlineCarriesAPercentageOrARomanGrade()
    {
        var offenders = new List<string>();
        foreach (var (slug, headline) in Headlines())
        {
            offenders.AddRange(FiguresIn(headline).Select(f => $"{slug}: {f}"));
        }

        Assert.True(offenders.Count == 0,
            "a title or description carries a percentage or a Roman-numeral grade:\n  "
            + string.Join("\n  ", offenders));

        Assert.NotEmpty(FiguresIn("A page About 40% of people"));
        Assert.NotEmpty(FiguresIn("A page What grade III means"));
        Assert.NotEmpty(FiguresIn("A page Stage IV disease"));
        Assert.NotEmpty(FiguresIn("A page WHO II tumors"));
        Assert.NotEmpty(FiguresIn("A page What type I means"));

        // AND THE INTRAVENOUS CASE MUST NOT FIRE. This is live on
        // /treatments/chemotherapy and is the reason the pattern is anchored on a
        // classifying word rather than matching a bare Roman numeral.
        Assert.Empty(FiguresIn("A page Some chemo is given by IV and some by mouth"));

        // THE NEGATIVE CANARY IS THE LOAD-BEARING ONE HERE, because it is the shape the
        // corpus's own headlines use. A digit ban would fail every one of them. (The
        // first version said "nine" here, twice, in a test whose docstring says the
        // count is left out because the sweep enumerates it. /review round 2.)
        Assert.Empty(FiguresIn("A page High-grade glioma means grade 3 or grade 4"));
        Assert.Empty(FiguresIn("A page An inherited condition linked to about 1 in 4"));
    }

    private static List<string> FiguresIn(string headline)
    {
        var found = new List<string>();

        foreach (Match m in Regex.Matches(headline, @"\d+(?:\.\d+)?\s?%"))
        {
            found.Add($"percentage {m.Value.Trim()}");
        }

        // ANCHORED ON A CLASSIFYING WORD, and widened past `grade` — the first
        // version fired only after that one word, so `stage IV`, `WHO II` and `type
        // II` walked through a test called NoHeadlineCarriesAPercentageOrARomanGrade
        // (/review round 1). Widened at a measured cost of ZERO across all 55
        // headlines, and it catches all four shapes.
        //
        // NOT A BARE ROMAN NUMERAL, and the corpus says why: `/treatments/chemotherapy`
        // carries `IV` in its headline, meaning INTRAVENOUS. A bare pattern would fail
        // that correct page, which §12.8 says is worse than no rule — and it is the
        // one live hit a bare pattern finds.
        //
        // Word-bounded on both sides, so `grade Iron` cannot fire.
        // CULTUREINVARIANT, and `WHO` case-SENSITIVE inside an otherwise
        // case-insensitive pattern. `(?i)` alone made this match the English word
        // "who" followed by a bare Roman numeral, and `(?i)` over `I` without
        // CultureInvariant is the Turkish dotless-I hazard — neither has a live hit,
        // and both are the §12.8 false-fail waiting for a correct page. /review round 2.
        foreach (Match m in Regex.Matches(
                     headline,
                     @"(?i)\b(?:grades?|stages?|types?|(?-i:WHO)|class)\s+(I{1,3}|IV|V)\b",
                     RegexOptions.CultureInvariant))
        {
            found.Add($"Roman classification {m.Value.Trim()}");
        }

        return found;
    }
}
