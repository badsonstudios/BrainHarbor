using System.Text.RegularExpressions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-579, asserting <b>§12.27</b>: the 2021 rule that a gene result can set a
/// tumor's grade on its own has ONE owner, and the eight places that state it are
/// not eight copies of one fact.
///
/// <para><b>THE ITEM WAS COMMISSIONED ON A MEASUREMENT AND THE MEASUREMENT WAS A
/// FLOOR.</b> WI-576 found the duplication with the shingle <c>the grade on its
/// own</c> and reported FOUR files. That phrase actually finds five — the four it
/// named plus <c>/tumors/low-grade-glioma</c> — and a fifth of its own,
/// <c>glossary/cdkn2a-b-deletion</c>, whose copy of the sentence is the one this
/// item deleted. <b>Four more state the rule and carry none of that phrase, and
/// they state it best:</b> <c>/tumors/glioma</c> ("Gene results can now set the
/// grade"), <c>/tumors/astrocytoma</c> ("graded 4 on the gene result alone, even
/// when the cells did not look that way"), <c>/tumors/glioblastoma</c> ("on its
/// gene results alone") and <c>/tumors/meningioma</c>'s grade-3 instance. So:
/// <b>eight pages, not four</b> — WI-569's §12.18 lesson ("a lexicon is a floor,
/// not a fence") arriving as a work item's own scope, which is why the floor scan
/// below ships WITH a control proving it is only a floor rather than pretending
/// otherwise.</para>
///
/// <para><b>THE RULING, and it is why seven statements are kept rather than
/// deleted: a rule and its instances are different facts.</b> "Since 2021 a gene
/// result can set the grade on its own" is one fact about grading and it has one
/// home. "An IDH-mutant astrocytoma is grade 4 if both copies of CDKN2A/B are
/// missing" is a fact about astrocytoma, and it belongs to the page whose reader is
/// holding that report — §12.10's own split ("the block carries what is universal,
/// the page carries its own slice, repeated only where it differs"), applied to a
/// fact that has no block. Three pages do restate the general rule, and each one
/// does it under a heading written in the reader's own words about a grade that
/// surprised them; <b>an answer to the question a section is headed with cannot be
/// a link.</b></para>
///
/// <para><b>WHY <c>/tests/molecular-markers</c> IS NOT THE OWNER, which is this
/// item's one reversal of its own brief.</b> The backlog called the marker page
/// "the obvious owner". It is not. The rule is a fact about GRADING, not about any
/// marker: the marker page states it four separate times (CDKN2A/B, TERT, EGFR,
/// chromosome 7/10) as four markers' behaviour, which is correct there and is a
/// different fact each time — and the page <b>itself routes to
/// <c>/tests/pathology-report#what-the-grade-means</c></b> for the grading rule.
/// Making the marker page the owner would put the rule on a page a reader reaches
/// only once they have gene results, when the reader who needs it is holding a
/// grade.</para>
///
/// <para><b>AND §12.10's ROUTE HALF WAS NEVER THE HOLE.</b> Measured before
/// anything was edited: all eight already link to <c>/tests/pathology-report</c> in
/// their own prose. What was missing was a written owner and a gate on the premise
/// — <b>nothing in the repo asserted that the owner still states the rule</b>, so
/// deleting the owner's two sentences would have left seven routes pointing at a
/// page that no longer answers, silently.</para>
///
/// <para><b>WHICH SETS THESE GATES ARE BUILT OVER (§12.19), said out loud:</b>
/// <see cref="EveryRecordedStatementIsStillWhereItsReasonSaysItIs"/> and
/// <see cref="EveryPageThatStatesTheRuleRoutesToItsOwner"/> are built over
/// <see cref="Recorded"/> — eight files, named. The two scans are built over
/// <c>CuratedPage.AllPages()</c> AND <c>CuratedPage.SharedSources()</c>, so
/// <c>blocks/</c> and <c>glossary/</c> are in scope: a glossary definition fires as
/// a tooltip on every page that says the term and a block composes onto nineteen
/// hubs, so a rule that reads only <c>pages/</c> is blind to the two directories
/// with the widest blast radius (§12.26).</para>
/// </summary>
public sealed class GradingRuleSweepTests
{
    /// <summary>The page that owns the rule, and the section of it that does.</summary>
    private const string Owner = "pages/tests/pathology-report";

    private const string OwnerSection = "What the grade means";

    private const string OwnerRoute = "/tests/pathology-report";

    /// <summary>
    /// Why a file is allowed to say this, counted POSITIVELY — §12.24, because a
    /// set defined as "everything except the owner" is redefined by every new
    /// kind of page and the partition assertion below is what notices.
    /// </summary>
    private enum Why
    {
        /// <summary>The general rule, in its one home. Exactly one of these.</summary>
        TheRule,

        /// <summary>
        /// The rule applied to ONE tumor or ONE marker, which is that page's own
        /// fact and not a copy of the rule.
        /// </summary>
        AnInstance,

        /// <summary>
        /// The general rule restated because the section is headed with the
        /// reader's own question or objection about it, and an answer to a
        /// question cannot be a link.
        /// </summary>
        AnAnswer,
    }

    /// <summary>
    /// One statement of the rule: where it lives, the HEADING whose reason it
    /// belongs to, and the sentence as a reader meets it.
    ///
    /// <para><b>The heading is in the record on purpose.</b> Every reason below is
    /// about WHERE the sentence sits — the answer to a headed question, the
    /// marker's own "what your team does with it", one tumor's grade list. A
    /// record naming only the page would stay green after the sentence drifted
    /// into a footer, which is the whole-page <c>Contains()</c> defect §12.10
    /// records three separate items finding (WI-506, WI-508, WI-511).</para>
    ///
    /// <para><paramref name="Needle"/> is taken MECHANICALLY from the flattened,
    /// unemphasised page rather than typed from the source, and every one was
    /// measured to appear on no other page. WI-576's post-deploy smoke returned a
    /// failure because a needle typed from a file spanned the source's hard wrap;
    /// that was the third costume of WI-571's defect and this is the cheap way not
    /// to wear it a fourth time.</para>
    /// </summary>
    /// <param name="Hits">
    /// How many times <see cref="TheRulesVocabulary"/> matches this file, measured.
    ///
    /// <para><b>/review found why a summed floor was not enough.</b> The first
    /// version asserted <c>hits >= 16</c> over the whole corpus and skipped every
    /// match on a recorded slug — so a NINTH statement arriving on an
    /// already-recorded page was invisible, and a pattern going dead was invisible
    /// too as long as the other seven still totalled 16. One of them WAS dead (see
    /// <see cref="TheRulesVocabulary"/>). Eight exact numbers catch an addition, a
    /// removal, and a pattern that stopped matching — and they say which file.</para>
    /// </param>
    private sealed record Statement(
        string Slug, string Heading, Why Why, string Needle, int Hits, string Reason);

    private static readonly Statement[] Recorded =
    [
        new(Owner, OwnerSection, Why.TheRule,
            "Now a gene result can set the grade on its own, even when the cells under "
            + "the microscope look lower grade.",
            2,
            "THE OWNER. Grading is this page's subject: it carries the 2021 Roman-to-Arabic "
            + "change, the within-type rule and the grade's definition, and all seven others "
            + "link here. It is also the only one that states the rule AS A CHANGE, with the "
            + "before-half ('A grade used to come only from what the cells looked like'), "
            + "which is what makes it the rule rather than a fact."),

        new("pages/tests/molecular-markers", "CDKN2A/B", Why.AnInstance,
            "In some gliomas this finding sets the grade on its own, even when the cells "
            + "look like a lower grade.",
            1,
            "ONE MARKER'S BEHAVIOUR, under the §12.8 template's 'What your team does with "
            + "it'. The page says the same SHAPE of thing about four markers and means a "
            + "different fact each time, and it routes to the owner's grading section in the "
            + "next sentence. This is why the marker page is not the owner: a page that "
            + "states a rule once per marker is stating the markers, not the rule."),

        new("pages/tumors/glioma", "What the grade actually describes", Why.AnInstance,
            "Gene results can now set the grade.",
            2,
            "THE FAMILY'S OWN GRADE, and the one instance that is word-for-word the general "
            + "rule — because the glioma family spans grades 1 to 4, so 'what this applies to "
            + "here' IS 'in general' (§12.11, what a grouping page owes its umbrella). "
            + "Recorded rather than deleted for that reason, and it routes on to the marker "
            + "page for the individual tests."),

        new("pages/tumors/astrocytoma", "What the grade actually describes", Why.AnInstance,
            "A tumor can be graded 4 on the gene result alone, even when the cells did not "
            + "look that way.",
            2,
            "ONE TUMOR'S GRADE: the fourth bullet of this page's own three-grade list for "
            + "IDH-mutant astrocytoma, with the sentence that stops the list reading as a "
            + "contradiction. Says nothing about any other tumor."),

        new("pages/tumors/low-grade-glioma",
            "Why does my report say grade 4 when the scan looked low grade?", Why.AnAnswer,
            "For some tumors, a gene result now sets the grade on its own.",
            4,
            "THE ANSWER TO THE HEADING. The section is the reader's own question and this "
            + "sentence is the answer to it; the page already routes to the owner's grading "
            + "section five lines above. Replacing the answer with the link it already "
            + "carries would leave a headed question unanswered."),

        new("pages/tumors/high-grade-glioma",
            "Why does my report say grade 4 when the scan looked milder?", Why.AnAnswer,
            "Because a gene result can now set the grade on its own, even when the cells "
            + "look lower grade.",
            3,
            "THE ANSWER TO THE HEADING, and it is the whole first paragraph of the section — "
            + "the word 'Because' is doing the answering. This is the pair WI-576 recorded as "
            + "deliberately shared with glossary/cdkn2a-b-deletion and handed here; the "
            + "record is gone because the ENTRY's copy went, not this one."),

        new("pages/tumors/glioblastoma", "But my tumor did not look grade 4", Why.AnAnswer,
            "Since 2021, a tumor can be called a glioblastoma on its gene results alone.",
            1,
            "THE ANSWER TO THE READER'S OBJECTION, which the heading is written as. The "
            + "glioblastoma form of the rule is also the page's own naming rule — no IDH "
            + "change plus one of three findings — so the general and the specific are the "
            + "same sentence here."),

        new("pages/tumors/meningioma", "Is it cancer? What does its grade mean?", Why.AnInstance,
            "Either of those two changes makes it grade 3, whatever the cells look like.",
            2,
            "ONE TUMOR'S GRADE, and the only non-glioma one: a TERT promoter change or loss "
            + "of CDKN2A/B makes a meningioma grade 3. WI-579 rewrote the sentence above it, "
            + "which called the CDKN2A/B pair 'a gene', and fixed this sentence's pronoun "
            + "while there — 'Either one' sat next to two gene names and two 'things' and "
            + "could attach to either pair."),
    ];

    /// <summary>
    /// The rule's vocabulary as the corpus actually writes it, derived by sweeping
    /// rather than invented: every shape below was found in a shipped file, and the
    /// per-file counts in <see cref="Recorded"/> are what hold that claim — the
    /// first version of this list said the same sentence while one pattern matched
    /// NOTHING, because it was typed as a mid-sentence paraphrase and the corpus
    /// writes it sentence-initial. <b>Derived from the corpus has to mean matched
    /// the way the corpus writes it</b>, which is why the scan ignores case.
    ///
    /// <para><b>THIS IS A FLOOR, NOT A FENCE</b>, and
    /// <see cref="EveryPatternIsShownToCatchSomethingAndTheScanIsShownToBeOnlyAFloor"/>
    /// proves both halves of that sentence. §12.18/WI-569: one hundred planted sentences over
    /// seven rounds found that two of them ("the surgeon can usually only get part
    /// of it") carried no banned word at all, and no word list reaches that by
    /// induction. <see cref="Recorded"/> is the fence. This catches the arrivals
    /// that are written in the vocabulary the corpus already uses, which is most of
    /// them, and it is the half that can run over 168 files.</para>
    /// </summary>
    private static readonly string[] TheRulesVocabulary =
    [
        @"sets? the grade on its own",
        @"[Gg]ene results? (?:can )?(?:now )?sets? the grade",
        @"graded \d+ on the gene results? alone",
        @"grade is no longer only",
        @"the grade does not come only",
        @"(?:whatever|however) the cells look",
        @"on its gene results alone",
        @"can change a grade without anything looking different",
    ];

    /// <summary>
    /// Calling the CDKN2A/B pair a SINGLE gene. CDKN2A and CDKN2B are two
    /// different genes on chromosome 9, and the entry has said "two genes" since
    /// it shipped.
    ///
    /// <para><b>The backlog named one page and the scan found three</b>, because it
    /// scanned for the SHAPE and not for the sentence: <c>/tumors/astrocytoma</c>
    /// and <c>/tumors/high-grade-glioma</c> both said "a gene called CDKN2A/B", and
    /// <c>/tumors/meningioma</c> said "a gene called CDKN2A or CDKN2B" — a singular
    /// article in front of two gene names, which is the same error wearing the
    /// plural's clothes and is the one a sentence-shaped search misses.</para>
    /// </summary>
    private static readonly string[] ThePairAsOneGene =
    [
        @"a gene called CDKN2A",
        @"a gene, CDKN2A",
        @"the gene CDKN2A",
        @"a CDKN2A/B gene\b",
    ];

    /// <summary>
    /// The corpus must say somewhere, near the term, that there are TWO — otherwise
    /// banning the singular is vacuous and the reader is simply never told.
    /// §12.24's second canary, as the converse of a ban.
    /// </summary>
    private static readonly string[] ThePairAsTwoGenes =
    [
        @"two (?:(?:neighboring|different) )?genes",
        @"CDKN2A and CDKN2B",
    ];

    private static string Raw(string slug)
    {
        var parts = slug.Split('/');
        parts[^1] += ".md";

        // `pages/` is where CuratedPage.Read() starts, so a slug carrying it would
        // resolve to pages/pages/... and fail loudly. The others are read whole.
        return parts[0] == "pages"
            ? CuratedPage.Read(parts[1..])
            : File.ReadAllText(Path.Combine(
                CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content",
                Path.Combine(parts)));
    }

    /// <summary>
    /// A file as a reader meets it: COMPOSED when it is a page (a block's prose is
    /// prose on nineteen hubs), markers stripped, flattened because the source is
    /// hard-wrapped, and emphasis stripped because a bolded word plus a line wrap
    /// defeats a phrase guard structurally.
    ///
    /// <para>Blocks and glossary entries are NOT composed: they have no includes,
    /// and <c>Compose</c> on a file that is itself a block is a question nobody
    /// asked.</para>
    /// </summary>
    private static string Plain(string slug, string text) =>
        Regex.Replace(
            CuratedPage.Flatten(CuratedPage.ReaderText(
                slug.StartsWith("pages/", StringComparison.Ordinal)
                    ? CuratedPage.Composed(text, slug)
                    : text)),
            @"[*_]", "");

    /// <summary>
    /// Every file whose words reach a reader: <c>pages/</c> and <c>blocks/</c> from
    /// <c>AllPages()</c>, <c>glossary/</c> from <c>SharedSources()</c>, blocks
    /// de-duplicated because both enumerate them.
    /// </summary>
    private static readonly Lazy<List<(string Slug, string Text)>> AllFiles =
        new(() =>
            [.. CuratedPage.AllPages()
                .Concat(CuratedPage.SharedSources())
                .DistinctBy(f => f.Slug)
                .OrderBy(f => f.Slug, StringComparer.Ordinal)]);

    /// <inheritdoc cref="AllFiles"/>
    private static List<(string Slug, string Text)> EveryFile() => AllFiles.Value;

    /// <summary>
    /// The chunk of a file that sits under one heading of any level, flattened and
    /// unemphasised.
    ///
    /// <para>Chunked rather than line-searched, because a needle that crosses a
    /// hard wrap matches zero lines — the trap that made WI-576's locator tooling
    /// necessary — and chunked rather than <c>CuratedPage.Section</c>, because
    /// three of the eight headings here are <c>###</c> and <c>Section</c> only cuts
    /// on <c>## </c>. Composing first and cutting after is also why this takes the
    /// composed text: §12.10's structural-cut trap runs the other way (a
    /// <c>Section</c> on a composed page stops at the block's own heading), and a
    /// chunk walk has no next-heading to trip over.</para>
    /// </summary>
    private static string? ChunkUnder(string plainUnflattened, string heading)
    {
        // ANY heading level, and `#{2,4}` was the first version: a level outside the
        // range is neither findable NOR a chunk boundary, so a chunk silently spills
        // past it and a needle "under" one heading can really be under another
        // (/review). No page in scope uses h1 or h5 today — which is exactly why
        // the narrow range would have gone unnoticed.
        var headings = Regex.Matches(plainUnflattened, @"^#{1,6} (.+)$", RegexOptions.Multiline);
        for (var i = 0; i < headings.Count; i++)
        {
            var title = Regex.Replace(headings[i].Groups[1].Value, @"\s*\{\#[^}]+\}\s*$", "").Trim();
            if (title != heading)
            {
                continue;
            }

            var start = headings[i].Index + headings[i].Length;
            var end = i + 1 < headings.Count ? headings[i + 1].Index : plainUnflattened.Length;
            return Regex.Replace(CuratedPage.Flatten(plainUnflattened[start..end]), @"[*_]", "");
        }

        return null;
    }

    private static string Unflattened(string slug, string text) =>
        CuratedPage.ReaderText(
            slug.StartsWith("pages/", StringComparison.Ordinal)
                ? CuratedPage.Composed(text, slug)
                : text);

    /// <summary>
    /// The premise every one of the seven routes rests on, and <b>nothing in the
    /// repo asserted it before WI-579</b>.
    ///
    /// <para>This is WI-576's finding in a second costume. There, "the page defines
    /// the term inline" lived in a message string, so deleting the defining
    /// sentence while leaving <c>!%term%</c> would have gone quiet in every guard in
    /// the repo. Here the unasserted premise is bigger: delete these two sentences
    /// and seven pages still link to this section, the item's whole ruling is
    /// false, and <b>every test in this file still passes</b> unless this one
    /// exists.</para>
    ///
    /// <para>Both halves are asserted, because <b>the rule is a CHANGE and a change
    /// needs its before.</b> "A gene result can set the grade" alone is not the
    /// 2021 rule — it is a fact about grading that leaves the reader's actual
    /// question ("so why does my report disagree with what I was told about the
    /// cells?") unanswered. The before-half is what makes it the rule, and it is
    /// what the other seven are allowed to omit.</para>
    /// </summary>
    [Fact]
    public void TheOwnerStillStatesTheRuleAndTheChangeItIs()
    {
        var owner = Recorded.Single(s => s.Why == Why.TheRule);
        Assert.Equal(Owner, owner.Slug);

        var section = ChunkUnder(Unflattened(Owner, Raw(Owner)), OwnerSection);
        Assert.NotNull(section);

        Assert.Contains("A grade used to come only from what the cells looked like", section,
            StringComparison.Ordinal);
        Assert.Contains(owner.Needle, section, StringComparison.Ordinal);
    }

    /// <summary>
    /// Every recorded statement is still on its page AND still under the heading
    /// its reason is about — the converse of the record, computed from the corpus.
    ///
    /// <para>The shape WI-578 left standing for <c>DescriptionsCleanDirectories</c>
    /// and WI-576 for <c>DeliberatelyShared</c>: a list only ever read as "these
    /// are allowed" cannot tell you that one of them stopped being true. Move a
    /// sentence out of its section and the reason beside it is about text that has
    /// moved; delete it and the ruling has seven statements, not eight, and nobody
    /// decided that.</para>
    /// </summary>
    [Fact]
    public void EveryRecordedStatementIsStillWhereItsReasonSaysItIs()
    {
        var missing = new List<string>();

        foreach (var s in Recorded)
        {
            var body = Unflattened(s.Slug, Raw(s.Slug));
            var chunk = ChunkUnder(body, s.Heading);

            if (chunk is null)
            {
                missing.Add($"{s.Slug}: no heading '{s.Heading}' — the record's reason is "
                    + "about a section that has been renamed or removed");
                continue;
            }

            if (!chunk.Contains(s.Needle, StringComparison.Ordinal))
            {
                var elsewhere = Plain(s.Slug, Raw(s.Slug))
                    .Contains(s.Needle, StringComparison.Ordinal);
                missing.Add($"{s.Slug}: the sentence is {(elsewhere ? "still on the page but "
                    + "no longer under" : "gone from")} '{s.Heading}' — [{s.Needle}]");
            }
        }

        Assert.True(missing.Count == 0,
            "a recorded statement of the 2021 grading rule has moved or gone, so §12.27's "
            + "record is about text that no longer exists. Re-read the eight together before "
            + "editing this list — the point of the record is that there are eight and that "
            + "each one has a reason.\n"
            + "  AND IF THE HEADING WAS RENAMED, UPDATING THE STRING HERE IS NOT THE FIX. "
            + "Three of the eight are allowed to restate the general rule ONLY because the "
            + "section is headed with the reader's own question or objection; that is a "
            + "judgement no assertion in this file holds, and §12.27 refused to fake it with "
            + "a test for a question mark (four of the eight headings are statements, and "
            + "/tumors/glioblastoma's is 'But my tumor did not look grade 4'). So a renamed "
            + "heading means the REASON has to be re-decided and rewritten, and the Reason "
            + "field is where the next reader will look for it:\n  "
            + string.Join("\n  ", missing));
    }

    /// <summary>
    /// §12.10's route half: every page that states the rule points its reader at
    /// the page that owns it.
    ///
    /// <para><b>Asserted on the page's OWN text, uncomposed, and that is the whole
    /// care in this test.</b> WI-570's finding was that the route to
    /// <c>/where-your-tumor-is</c> lived in <c>blocks/mechanism.md</c>, so eighteen
    /// hubs had it and the five that do not compose that block had none — and a
    /// composed assertion would have called all twenty-three green. A route that a
    /// page inherits is a route the page loses the day it stops including the
    /// block, silently, because no single page is individually wrong.</para>
    /// </summary>
    [Fact]
    public void EveryPageThatStatesTheRuleRoutesToItsOwner()
    {
        // THE BODY, NOT THE WHOLE FILE, and /review caught the gap: Raw() includes
        // the YAML front matter, which on these pages runs to hundreds of lines of
        // source notes. A `# cross-ref: /tests/pathology-report` comment up there
        // would satisfy a whole-file Contains() while the reader has no route at
        // all — the doc above claims the route is in the page's own PROSE, so that
        // is what gets read. Still UNCOMPOSED, which is the other half.
        var unrouted = Recorded
            .Where(s => s.Slug != Owner)
            .Where(s => !CuratedPage.ReaderText(Raw(s.Slug))
                .Contains(OwnerRoute, StringComparison.Ordinal))
            .Select(s => s.Slug)
            .ToList();

        Assert.True(unrouted.Count == 0,
            $"a page states the 2021 grading rule and does not link to {OwnerRoute}, which "
            + "owns it (§12.27, §12.10). The route has to be in the page's own prose — one "
            + "inherited from a block disappears the day the page stops including it:\n  "
            + string.Join("\n  ", unrouted));

        // The premise of the assertion above: it would also pass if the record
        // were empty, and an emptied record is the natural end state of deleting
        // entries one at a time.
        Assert.Equal(7, Recorded.Count(s => s.Slug != Owner));
    }

    /// <summary>
    /// The partition, counted POSITIVELY (§12.24): one owner, four instances, three
    /// answers, eight in total and nothing outside those three reasons.
    ///
    /// <para>A set defined by subtraction — "everything that is not the owner" — is
    /// redefined by every new kind of page, and the count on the end is what
    /// notices a ninth arriving as a fourth KIND. WI-575 round 4 switched a gate
    /// off by conflating a corpus size with a defect count; the numbers here are
    /// the <b>reasons</b>, and the reason there are exactly three of them is the
    /// ruling.</para>
    /// </summary>
    [Fact]
    public void TheEightStatementsPartitionIntoThreeReasonsAndOneOwner()
    {
        Assert.Equal(8, Recorded.Length);
        Assert.Equal(8, Recorded.Select(s => s.Slug).Distinct().Count());

        Assert.Equal(1, Recorded.Count(s => s.Why == Why.TheRule));
        Assert.Equal(4, Recorded.Count(s => s.Why == Why.AnInstance));
        Assert.Equal(3, Recorded.Count(s => s.Why == Why.AnAnswer));

        Assert.Equal(
            Recorded.Length,
            Recorded.Count(s => s.Why is Why.TheRule or Why.AnInstance or Why.AnAnswer));

        Assert.All(Recorded, s => Assert.False(string.IsNullOrWhiteSpace(s.Reason)));
    }

    /// <summary>
    /// No file outside <see cref="Recorded"/> states the rule in the vocabulary the
    /// corpus uses — the floor, over all 168 files including <c>blocks/</c> and
    /// <c>glossary/</c>.
    ///
    /// <para><b>PER FILE AND EXACT, because a summed floor hid a dead pattern and
    /// would hide a ninth statement</b> (/review). <c>hits >= 16</c> over the whole
    /// corpus is satisfied by any seven of the eight patterns, and it skipped every
    /// match on a recorded slug — so an added statement on an already-recorded page
    /// was invisible in one direction and a pattern that matched nothing was
    /// invisible in the other. Eight exact numbers catch an addition, a removal and
    /// a dead pattern, and name the file.</para>
    ///
    /// <para><b>IGNORE CASE, and that is the fix for the pattern that was dead.</b>
    /// <c>grade is no longer only</c> matched <b>zero of 168 files</b>: the corpus's
    /// only occurrence is sentence-initial and bolded — <c>**Grade is no longer only
    /// about how the cells look.**</c> — and the pattern was written lowercase from a
    /// mid-sentence paraphrase rather than lifted from the page. A vocabulary derived
    /// from the corpus has to be matched the way the corpus writes it, which includes
    /// the capital a sentence starts with. The same hole was open for
    /// <c>(?:whatever|however) the cells look</c> and <c>the grade does not come
    /// only</c>, both of which have natural sentence-initial forms.</para>
    ///
    /// <para>Both canaries beside the scan (§12.24): the scan is shown to fire on
    /// something and shown not to fire on nothing. Without them a regex that stopped
    /// matching — an escape eaten, a composed page returning empty — reads exactly
    /// like a clean corpus.</para>
    /// </summary>
    [Fact]
    public void NoUnrecordedFileStatesTheRuleInTheVocabularyTheCorpusUses()
    {
        var expected = Recorded.ToDictionary(s => s.Slug, s => s.Hits, StringComparer.Ordinal);
        var offenders = new List<string>();
        var counted = new Dictionary<string, int>(StringComparer.Ordinal);
        var scanned = 0;

        foreach (var (slug, text) in EveryFile())
        {
            scanned++;
            var plain = Plain(slug, text);

            foreach (var pattern in TheRulesVocabulary)
            {
                foreach (var m in Regex.Matches(plain, pattern, RegexOptions.IgnoreCase)
                    .Cast<Match>())
                {
                    counted[slug] = counted.GetValueOrDefault(slug) + 1;
                    if (expected.ContainsKey(slug))
                    {
                        continue;
                    }

                    var from = Math.Max(0, m.Index - 70);
                    offenders.Add($"{slug} [{m.Value}]: ..."
                        + plain[from..Math.Min(plain.Length, m.Index + m.Length + 70)] + "...");
                }
            }
        }

        // CANARY ONE, positive and per file. A clean corpus and a broken scan are
        // the same result without this, and a SUM cannot say which pattern died.
        var wrong = expected
            .Where(e => counted.GetValueOrDefault(e.Key) != e.Value)
            .Select(e => $"{e.Key}: the vocabulary matches "
                + $"{counted.GetValueOrDefault(e.Key)} times, recorded {e.Value}")
            .ToList();

        Assert.True(wrong.Count == 0,
            "the rule's vocabulary no longer matches a recorded file the measured number of "
            + "times. FEWER means a pattern stopped matching or the sentence changed; MORE "
            + "means a statement was ADDED to a page that already had one, which §12.27's "
            + "record does not describe. Re-measure and move the number WITH its reason — do "
            + "not move it to match (WI-575 round 4 switched a gate off that way):\n  "
            + string.Join("\n  ", wrong));

        Assert.Equal(expected.Count, counted.Count);
        Assert.True(scanned > 160,
            $"only {scanned} files were scanned, so this is not the corpus it thinks it is");

        // CANARY TWO, and the first version of it was VACUOUS (/review): asserting
        // that no file contains a nonsense literal passes just as happily if
        // Plain() returns "" for all 168 files, which is the exact failure the
        // canaries exist to catch. Round-tripped through the real helper instead,
        // so it proves Plain() PRESERVES text rather than proving nothing.
        var (probeSlug, probeText) = EveryFile()[0];
        Assert.DoesNotContain("zzqqxnotinthecorpusxqqzz", Plain(probeSlug, probeText),
            StringComparison.Ordinal);
        Assert.Contains("zzqqxnotinthecorpusxqqzz",
            Plain(probeSlug, probeText + "\n\nzzqqxnotinthecorpusxqqzz\n"),
            StringComparison.Ordinal);

        Assert.True(offenders.Count == 0,
            "a file states the 2021 grading rule and is not one of §12.27's eight. Read the "
            + "eight together and decide which reason this one has — THE RULE (there is "
            + "already an owner), AN INSTANCE for one tumor or marker, or AN ANSWER to a "
            + "heading in the reader's own words — then add it with that reason. Do not "
            + "delete it to make this pass:\n  " + string.Join("\n  ", offenders));
    }

    /// <summary>
    /// One control per pattern, each matched by ITS OWN pattern and by no other —
    /// so deleting, blanking or breaking any single pattern reds this. And one
    /// control that nothing matches, which is the honest half: the scan is a floor.
    ///
    /// <para><b>THE BREAK HARNESS FOUND THE DEFECT THIS REPLACES, and it is the
    /// cheapest lesson in the file.</b> The first version had ONE caught control —
    /// <i>"In a few tumor types a gene result sets the grade on its own, whatever
    /// the cells look like"</i> — asserted with <c>Assert.Contains(vocabulary, p =>
    /// IsMatch(control, p))</c>, which passes as long as ANY pattern matches.
    /// Blanking <c>sets? the grade on its own</c> left the control caught by
    /// <c>(?:whatever|however) the cells look</c> and <b>the mutation survived on
    /// both endings</b>. A positive control that can be satisfied by a different
    /// pattern than the one under test pins no pattern at all — §12.18's "a guard
    /// never seen to fail has not been shown to work" with one more turn on it: a
    /// guard seen to fail for the wrong reason has not been shown to work either.
    /// Each control below was measured to match exactly one pattern rather than
    /// eyeballed, because eyeballing is what produced the survivor.</para>
    ///
    /// <para>WI-567 kept a documented known survivor rather than deleting it,
    /// "because a mutation aimed at nothing is a permanent red that teaches nothing
    /// — and deleting it would hide the finding". Same for
    /// <c>NothingInTheVocabularyCatchesThis</c>: it is plain English any author
    /// could write, carries not one word of the vocabulary, and is exactly why
    /// <see cref="Recorded"/> and not this scan is the fence. If you strengthen the
    /// scan until it catches that sentence, move it up into the table with its own
    /// pattern rather than deleting it.</para>
    /// </summary>
    [Fact]
    public void EveryPatternIsShownToCatchSomethingAndTheScanIsShownToBeOnlyAFloor()
    {
        // In vocabulary order, and the order is asserted below — a control list
        // that has drifted out of step still passes a per-row check if each row
        // happens to match somebody's pattern.
        (string Pattern, string Control)[] controls =
        [
            (@"sets? the grade on its own",
                "This finding sets the grade on its own."),
            (@"[Gg]ene results? (?:can )?(?:now )?sets? the grade",
                "Gene results can now set the grade."),
            (@"graded \d+ on the gene results? alone",
                "A tumor can be graded 4 on the gene result alone."),
            (@"grade is no longer only",
                "The grade is no longer only about the cells."),
            (@"the grade does not come only",
                "Since 2021 the grade does not come only from the cells."),
            (@"(?:whatever|however) the cells look",
                "It is called grade 3, whatever the cells look like."),
            (@"on its gene results alone",
                "A tumor can be named on its gene results alone."),
            (@"can change a grade without anything looking different",
                "Two things can change a grade without anything looking different."),
        ];

        Assert.Equal(TheRulesVocabulary, controls.Select(c => c.Pattern));

        foreach (var (pattern, control) in controls)
        {
            // IgnoreCase, matching the scan. A control checked case-sensitively
            // while the scan ignores case is a control for a different guard.
            var matched = TheRulesVocabulary
                .Where(p => Regex.IsMatch(control, p, RegexOptions.IgnoreCase))
                .ToList();

            Assert.True(matched.Count == 1 && matched[0] == pattern,
                $"the control for /{pattern}/ is matched by {matched.Count} patterns "
                + $"[{string.Join(" | ", matched)}], so it cannot show that THIS pattern "
                + "works — which is exactly how WI-579's first version let a blanked "
                + $"pattern survive the break harness. Control: {control}");
        }

        const string nothingInTheVocabularyCatchesThis =
            "A gene finding alone is now enough to move the number up, no matter how "
            + "ordinary the cells appeared.";

        Assert.DoesNotContain(TheRulesVocabulary,
            p => Regex.IsMatch(nothingInTheVocabularyCatchesThis, p, RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// The same treatment for <see cref="ThePairAsOneGene"/>, and /review found that
    /// it needed it: the ban shipped with <b>two positive controls that both matched
    /// only the FIRST of its four patterns</b>, so <c>a gene, CDKN2A</c>,
    /// <c>the gene CDKN2A</c> and <c>a CDKN2A/B gene</c> were pinned by nothing at
    /// all. Deleting two of them left the whole suite green, after which a page
    /// writing <i>"both copies of the gene CDKN2A/B are missing"</i> would ship the
    /// error this item exists to stop.
    ///
    /// <para>This was the <b>third</b> appearance of one defect inside one item —
    /// a positive control that does not discriminate between the patterns it is
    /// supposed to be evidence for. The first cost a break-harness survivor, the
    /// second was caught by reading, this one by /review. <b>Writing a control per
    /// pattern is cheap; discovering which pattern a control actually pinned is
    /// not.</b></para>
    ///
    /// <para>None of these four has an occurrence in the corpus, which is the
    /// point — the ban is preventive, so the corpus cannot be its control and the
    /// controls have to be written. That also makes this list a FLOOR exactly like
    /// the vocabulary: it catches the shapes an author is likely to write, and the
    /// sentence below is one it does not catch.</para>
    /// </summary>
    [Fact]
    public void EveryPairAsOneGenePatternIsShownToCatchSomething()
    {
        (string Pattern, string Control)[] controls =
        [
            (@"a gene called CDKN2A",
                "both copies of a gene called CDKN2A/B are missing"),
            (@"a gene, CDKN2A",
                "both copies of a gene, CDKN2A/B, are missing"),
            (@"the gene CDKN2A",
                "both copies of the gene CDKN2A/B are missing"),
            (@"a CDKN2A/B gene\b",
                "a CDKN2A/B gene is missing from both copies"),
        ];

        Assert.Equal(ThePairAsOneGene, controls.Select(c => c.Pattern));

        foreach (var (pattern, control) in controls)
        {
            var matched = ThePairAsOneGene
                .Where(p => Regex.IsMatch(control, p, RegexOptions.IgnoreCase))
                .ToList();

            Assert.True(matched.Count == 1 && matched[0] == pattern,
                $"the control for /{pattern}/ is matched by {matched.Count} patterns "
                + $"[{string.Join(" | ", matched)}], so it cannot show that THIS pattern "
                + $"works. Control: {control}");
        }

        // The honest half, same as the vocabulary's: a singular the shape list does
        // not reach. "CDKN2A/B is a gene" carries no article the patterns look for.
        Assert.DoesNotContain(ThePairAsOneGene,
            p => Regex.IsMatch("CDKN2A/B is a gene that puts the brakes on cell division",
                p, RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// No file calls CDKN2A and CDKN2B a single gene, and the corpus does say
    /// somewhere that they are two.
    ///
    /// <para>The second half is not decoration. A ban with nothing on the other
    /// side of it is satisfied by a corpus that never mentions the pair at all, and
    /// "the reader is never told" passes a ban as happily as "the reader is told
    /// correctly".</para>
    /// </summary>
    [Fact]
    public void NoFileCallsTheCdkn2aBPairASingleGene()
    {
        var offenders = new List<string>();
        var mentionsThePair = 0;
        var saysTwo = 0;

        foreach (var (slug, text) in EveryFile())
        {
            var plain = Plain(slug, text);
            if (!plain.Contains("CDKN2A", StringComparison.Ordinal))
            {
                continue;
            }

            mentionsThePair++;

            foreach (var pattern in ThePairAsOneGene)
            {
                foreach (var m in Regex.Matches(plain, pattern, RegexOptions.IgnoreCase)
                    .Cast<Match>())
                {
                    var from = Math.Max(0, m.Index - 90);
                    offenders.Add($"{slug} [{m.Value}]: ..."
                        + plain[from..Math.Min(plain.Length, m.Index + m.Length + 60)] + "...");
                }
            }

            // "Two" has to be NEAR the term, not merely somewhere on a page that
            // also happens to mention it — "two copies", "two weeks" and "two
            // genes" all read the same to a whole-file Contains().
            //
            // COUNTED PER FILE, and the first version counted per (file, pattern):
            // the `break` left the pattern loop running, so the glossary entry —
            // which matches "two genes" AND "CDKN2A and CDKN2B" — scored 2 and the
            // failure message said "files". A canary whose message misnames what it
            // counted sends whoever reads the red in the wrong direction, which is
            // the one job a canary has.
            // The proximity window is load-bearing for "two genes" and a no-op for
            // "CDKN2A and CDKN2B", whose own match contains the term — said out
            // loud so the next reader does not assume both are proximity-checked.
            if (ThePairAsTwoGenes.Any(pattern => Regex
                .Matches(plain, pattern, RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Any(m => plain[Math.Max(0, m.Index - 120)
                        ..Math.Min(plain.Length, m.Index + m.Length + 120)]
                    .Contains("CDKN2A", StringComparison.Ordinal))))
            {
                saysTwo++;
            }
        }

        Assert.True(mentionsThePair >= 7,
            $"only {mentionsThePair} files mention CDKN2A in their BODY, so this scan has "
            + "little to look at. SEVEN when WI-579 measured it. If you removed a mention on "
            + "purpose this number moves with the reason; note that the glossary entry counts "
            + "only because its body names the genes — the term itself is front matter, which "
            + "Body() strips, so trimming that sentence lands here rather than where the "
            + "trimming happened");

        Assert.True(offenders.Count == 0,
            "a file calls the CDKN2A/B pair a single gene. They are two different genes, "
            + "which is what glossary/cdkn2a-b-deletion has said since it shipped, and a "
            + "singular article in front of two gene names is the same error (§12.27):\n  "
            + string.Join("\n  ", offenders));

        Assert.True(saysTwo >= 3,
            $"only {saysTwo} files say near the term that CDKN2A/B is TWO genes, so the ban "
            + "above is close to vacuous — a reader can meet the pair repeatedly and never "
            + "be told. THREE when WI-579 measured it, and they are the three that OWN the "
            + "fact: glossary/cdkn2a-b-deletion, /tests/molecular-markers and "
            + "/tumors/meningioma. The two glioma hubs deliberately make no count claim — "
            + "they carry the label the reader's report carries and the count has an owner "
            + "(§12.27). If you removed an occurrence on purpose, move this number WITH the "
            + "reason; do not move it to make a red go away (WI-575 round 4)");

        // THE POSITIVE CONTROLS LIVE IN
        // EveryPairAsOneGenePatternIsShownToCatchSomething, one per pattern. They
        // were here, and both of them matched only the FIRST of the four patterns,
        // so three were pinned by nothing (/review). Two controls in the same
        // Assert.Contains shape that a sibling test in this very file documents as
        // broken is the defect arriving a third time inside one item.
    }

    /// <summary>
    /// The premise behind refusing the obvious fix: the entry carries what no page
    /// says, so suppressing its tooltip would lose the definition rather than
    /// de-duplicate it.
    ///
    /// <para>WI-576 recorded this pair as deliberately shared because it could not
    /// resolve it: the entry's second sentence WAS the rule, so rewording the hub
    /// was wrong (it answers its own headed question) and suppressing the tooltip
    /// was wrong (the entry's first sentence is the only place in the corpus that
    /// says what the genes DO). WI-579's answer is that the entry's second sentence
    /// was the copy — the rule has an owner and a tooltip is not it — so the
    /// sentence went and the first one stayed. <b>This test is what makes that
    /// answer checkable:</b> if the unique half ever moves onto a page, the entry
    /// becomes a restatement with no unique content and the decision should be
    /// re-made rather than inherited.</para>
    ///
    /// <para>And the entry is asserted to state the rule NOWHERE, through the same
    /// vocabulary as the corpus scan — which is the thing that lets WI-576's
    /// <c>DeliberatelyShared</c> record be deleted rather than edited.</para>
    /// </summary>
    [Fact]
    public void TheEntryCarriesOnlyWhatNoPageSays()
    {
        const string theEntry = "glossary/cdkn2a-b-deletion";
        const string unique = "put the brakes on cell division";

        var carriers = EveryFile()
            .Where(f => Plain(f.Slug, f.Text).Contains(unique, StringComparison.Ordinal))
            .Select(f => f.Slug)
            .ToList();

        Assert.Equal([theEntry], carriers);

        var entry = Plain(theEntry, Raw(theEntry));
        Assert.All(TheRulesVocabulary, p => Assert.False(Regex.IsMatch(entry, p),
            $"glossary/cdkn2a-b-deletion states the 2021 grading rule again (/{p}/). The rule's "
            + "owner is /tests/pathology-report#what-the-grade-means and a tooltip firing on "
            + "nineteen pages is not it (§12.27) — this is the overlap WI-579 removed."));

        Assert.Contains("CDKN2A and CDKN2B", entry, StringComparison.Ordinal);
    }
}
