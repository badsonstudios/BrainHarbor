using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-580, §12.28: the restatement rule with a SHARED SOURCE as the subject.
///
/// <para><b>THE SET THIS FILE IS BUILT OVER IS <see cref="CuratedPage.SharedSources"/>
/// — `blocks/` + `glossary/` — AND NOT <see cref="CuratedPage.AllPages"/></b> (§12.19
/// asks this first, and §12.26 answered it the other way for the other direction).
/// `AllPages()` is `pages/` + `blocks/`, read by nine call sites in seven files, and
/// the glossary is in none of them. The subject here is the file whose words reach
/// more than one page, compared against every other such file: entry × entry, entry ×
/// block, and block × block. Pages are NOT the subject here; the page direction is
/// `ShippedGlossaryTests.NoPageRestatesAGlossaryDefinitionItsReaderAlsoMeetsAsATooltip`.</para>
///
/// <para><b>And the ruling is in two halves because a shared source has two reader
/// surfaces.</b> `/glossary` renders `GetTerms()` unconditionally (`Glossary.cshtml`),
/// so for entry × entry a reader ALWAYS meets both definitions in full on one page and
/// no computed property can excuse the overlap — it needs a content ruling, which
/// §12.28 gives: <b>a parallel pair MAY share its frame, and the overlap SIZE is
/// pinned so a reword reds.</b> A tooltip, by contrast, fires per page, so entry ×
/// block is decidable — and §12.28's position rule decides it.</para>
/// </summary>
public sealed class SharedSourceRestatementTests
{
    // ---------------------------------------------------------------- §12.28, half 1
    //
    // THE PARALLEL-PAIR RECORD. §12.8's WI-509 pattern — a reason beside every
    // exemption — plus the one thing an allowlist cannot do.
    //
    // `AllowedShingles` sets elsewhere in this suite FORGIVE an overlap, and
    // forgiving it is the same as not seeing it. These records pin the WINDOWS
    // THEMSELVES, so rewording either side of a licensed pair changes the set and
    // reds. That is the whole instrument: the hazard for a deliberately parallel
    // pair is not repetition, it is DRIFT, and §12.26's own fix is what proved it
    // (three entries rewritten to break a shingle, and the pons pair had already
    // diverged on two points before anyone looked).

    /// <summary>
    /// Two shared sources that overlap ON PURPOSE, with the reason and with the
    /// overlapping windows pinned — not a count of them. A count only notices an
    /// overlap that grows or shrinks; swap one window for a different one and a
    /// count stays put beside a reason describing text that no longer exists
    /// (`ShippedGlossaryTests`, /review).
    /// </summary>
    private sealed record ParallelPair(string Left, string Right, string[] Shared, string Why);

    private static readonly ParallelPair[] DeliberatelyParallel =
    [
        new("glossary/adult-type", "glossary/pediatric-type",
            [
                "a group of gliomas sorted by their biology",
                "age of the person who has them adults",
                "biology not by the age of the person",
                "gliomas sorted by their biology not by the",
                "group of gliomas sorted by their biology not",
                "of gliomas sorted by their biology not by",
                "sorted by their biology not by the age",
            ],
            "A PARALLEL PAIR SHARING ITS FRAME (§12.28). These two labels exist to be "
            + "told apart, and on /glossary every reader holds both. The shared first "
            + "sentence — 'sorted by their biology, not by the age of the person who has "
            + "them' — is the control variable: paraphrase one side and a reader cannot "
            + "tell whether the difference in wording is a difference in meaning. Each "
            + "carries its own second half (adult-type: children sometimes have one and "
            + "that is not a filing mistake; pediatric-type: adults are sometimes "
            + "diagnosed with one, and it describes the tumor rather than the person)."),

        new("glossary/astrocyte", "glossary/oligodendrocyte",
            [
                "cell in the brain and spinal cord it",
                "helper cell in the brain and spinal cord",
            ],
            "A PARALLEL PAIR SHARING ITS FRAME (§12.28). Two glial cell types, and the "
            + "shared frame is what they have in common as a matter of biology: a helper "
            + "cell in the brain and spinal cord. Each then says what only it does — "
            + "astrocyte is star-shaped and sits around nerve cells; oligodendrocyte "
            + "wraps nerve fibers in myelin — and each names the tumor that starts in it."),

        new("glossary/h3-g34", "glossary/h3-k27-altered",
            [
                "a change in a histone a histone is",
                "a histone a histone is a protein dna",
                "a histone is a protein dna wraps around",
                "a protein dna wraps around a diffuse glioma",
                "change in a histone a histone is a",
                "dna wraps around a diffuse glioma can have",
                "histone a histone is a protein dna wraps",
                "histone is a protein dna wraps around a",
                "in a histone a histone is a protein",
                "is a protein dna wraps around a diffuse",
                "protein dna wraps around a diffuse glioma can",
                "wraps around a diffuse glioma can have it",
            ],
            "A PARALLEL PAIR SHARING ITS FRAME, and the pair most in need of it "
            + "(§12.28): these two are ADJACENT on /glossary, both under 'H'. The frame "
            + "is what a histone change IS, which is identical for both findings; the "
            + "distinguishing half is the tumor each one names (diffuse hemispheric "
            + "glioma vs diffuse midline glioma, the latter conditioned on sitting in "
            + "the middle of the brain or spine). h3-g34 also cross-references its "
            + "partner out loud — 'H3 K27 is a different finding, but on the same kind "
            + "of gene' — which is the pair doing its job rather than colliding by "
            + "accident.\n\n"
            + "RE-PINNED AT WI-582 (§12.30), AND THIS RECORD IS WHY THAT ITEM DID NOT "
            + "SHIP A DIVERGED PAIR. Both sides read above 6.0 and both were rewritten, "
            + "and the first draft gave them DIFFERENT frames — 'A histone is one of the "
            + "proteins that DNA wraps around' against 'A histone is a protein DNA wraps "
            + "around' — which is the drift this record exists to catch, arriving by the "
            + "exact route §12.28 predicted: a fix for something else. The 40-WORD "
            + "CEILING (content-pipeline §6) is what forced the choice, because h3-g34 "
            + "sits at exactly 40 words and the longer frame does not fit beside its "
            + "cross-reference. 10 windows -> 12. "
            + "AND adult-type <-> pediatric-type WAS REWRITTEN TOO AND DID NOT MOVE: "
            + "its shared sentence was split at a comma, and ShinglesOfReaderText "
            + "strips punctuation, so 'biology, not by the age' and 'biology. Not by "
            + "the age' are the same windows. A sentence split is invisible to this "
            + "gate; a reworded frame is not, which is the right way round."),

        // `blocks/` sorts before `glossary/`, which the order assertion below
        // caught in this very record on its first run — the scan walks unordered
        // pairs Left < Right, so a record written the other way round is found by
        // nothing and then ALSO reported stale.
        new("blocks/escalation", "glossary/status-epilepticus",
            [
                "a seizure that lasts more than five minutes",
                "after another with no waking up in between",
            ],
            "AN ENTRY AND A BLOCK WITH NO SHARED READER, recorded rather than edited "
            + "(§12.28). This is the collision with the blast radius — the block composes "
            + "onto 25 pages — and checking it first is what found that 'status "
            + "epilepticus' is said on NONE of them. The one page that fires the tooltip, "
            + "/seizures/living-with, does not include the block, so no reader meets both. "
            + "Both copies need the five-minute rule whole: an entry defining the term "
            + "without it defines nothing, and an ambulance tier without it is not a "
            + "tier. The hazard here is DISAGREEMENT, not repetition, and the pinned "
            + "windows are the instrument for it — change the block's threshold without "
            + "changing the entry's and this reds. 25 suppression markers protecting no "
            + "reader was the alternative, and it is refused."),
    ];

    /// <summary>
    /// The reader-facing prose of each shared source: a glossary entry's PARSED
    /// definition (not its file — the front matter runs to source notes, and
    /// §12.27 recorded a route check that `Raw()` satisfied while the reader had
    /// no link at all), and a block's body with the authoring markers stripped.
    /// </summary>
    private static List<(string Slug, string Text)> SharedProse()
    {
        var all = new List<(string, string)>();

        foreach (var term in CuratedPage.GlossaryTerms)
        {
            all.Add(($"glossary/{term.Slug}", term.Definition));
        }

        foreach (var (slug, text) in CuratedPage.SharedSources()
            .Where(s => s.Slug.StartsWith("blocks/", StringComparison.Ordinal)))
        {
            all.Add((slug, CuratedPage.ReaderText(text)));
        }

        return [.. all.OrderBy(s => s.Item1, StringComparer.Ordinal)];
    }

    /// <summary>
    /// No shared source restates another one without a recorded reason — and every
    /// recorded reason still describes a real overlap, at exactly its recorded size.
    ///
    /// <para>Built over <see cref="CuratedPage.SharedSources"/>, so it covers all
    /// three directions a shared source can collide in. Measured when written:
    /// <b>3 entry × entry, 1 entry × block, 0 block × block.</b></para>
    /// </summary>
    [Fact]
    public void NoSharedSourceRestatesAnotherWithoutARecordedReason()
    {
        // ONE RECORD PER PAIR, asserted before anything reads the list — the
        // natural mistake is to APPEND a re-measurement rather than edit it, and
        // the lookup below would then throw something that says nothing about what
        // to do (ShippedGlossaryTests, /review).
        var pairs = DeliberatelyParallel.Select(p => (p.Left, p.Right)).ToList();
        Assert.Equal(pairs.Count, pairs.Distinct().Count());

        // AND IN ONE ORDER ONLY, because the scan below walks unordered pairs
        // Left < Right by slug. A record written the other way round would never be
        // found, the collision would be reported as unrecorded, and the record
        // would then ALSO be reported stale — two confusing failures for one typo.
        foreach (var pair in DeliberatelyParallel)
        {
            Assert.True(StringComparer.Ordinal.Compare(pair.Left, pair.Right) < 0,
                $"the record {pair.Left} <-> {pair.Right} is written in the wrong order; "
                + "Left must sort before Right");
        }

        var prose = SharedProse();
        var shingles = prose.ToDictionary(p => p.Slug, p => CuratedPage.ShinglesOfReaderText(p.Text));

        var offenders = new List<string>();
        var recordsUsed = new List<ParallelPair>();
        var collisions = 0;
        int entryVsEntry = 0, entryVsBlock = 0, blockVsBlock = 0;

        for (var i = 0; i < prose.Count; i++)
        {
            for (var j = i + 1; j < prose.Count; j++)
            {
                var left = prose[i].Slug;
                var right = prose[j].Slug;

                var shared = shingles[left]
                    .Intersect(shingles[right], StringComparer.Ordinal)
                    .OrderBy(s => s, StringComparer.Ordinal)
                    .ToList();

                if (shared.Count == 0)
                {
                    continue;
                }

                collisions++;

                var blockSides =
                    (left.StartsWith("blocks/", StringComparison.Ordinal) ? 1 : 0)
                    + (right.StartsWith("blocks/", StringComparison.Ordinal) ? 1 : 0);
                if (blockSides == 0)
                {
                    entryVsEntry++;
                }
                else if (blockSides == 1)
                {
                    entryVsBlock++;
                }
                else
                {
                    blockVsBlock++;
                }

                var recorded = DeliberatelyParallel.SingleOrDefault(
                    p => p.Left == left && p.Right == right);

                if (recorded is null)
                {
                    offenders.Add($"{left} <-> {right} ({shared.Count} windows): "
                        + string.Join(" | ", shared.Take(3)));
                    continue;
                }

                // USED means the RECORDED WINDOWS are the ones found, not merely
                // that this pair collides somehow.
                recordsUsed.Add(recorded);

                if (!recorded.Shared.OrderBy(s => s, StringComparer.Ordinal)
                    .SequenceEqual(shared, StringComparer.Ordinal))
                {
                    offenders.Add($"{left} <-> {right}: the overlap is not the one "
                        + "recorded, so one side has been reworded and the reason beside "
                        + "it is about text that has moved. "
                        + $"found {shared.Count} [{string.Join(" | ", shared)}], "
                        + $"recorded {recorded.Shared.Length} "
                        + $"[{string.Join(" | ", recorded.Shared)}]");
                    continue;
                }

                // THE PREMISE THE LICENCE RESTS ON (§12.27: assert the premise, not
                // facts about it). A parallel pair is licensed because it HAS a
                // distinguishing half. Trim both sides down to the shared frame and
                // the overlap set is unchanged — the check above cannot see that, and
                // what is left is one entry with two names.
                var onlyLeft = shingles[left].Except(shingles[right], StringComparer.Ordinal).Count();
                var onlyRight = shingles[right].Except(shingles[left], StringComparer.Ordinal).Count();
                if (onlyLeft == 0 || onlyRight == 0)
                {
                    offenders.Add($"{left} <-> {right}: a licensed parallel pair must each "
                        + "say something the other does not, and one of them no longer does "
                        + $"({left} has {onlyLeft} windows of its own, {right} has {onlyRight}). "
                        + "A pair with no distinguishing half is one definition with two names.");
                }
            }
        }

        // THE POSITIVE HALF, AND THE FLOORS ARE COMPUTED RATHER THAN ROUND. §12.18:
        // a property guard that has never been seen to fail has not been shown to
        // work — and `SharedProse()` reaching into two directories through two
        // different parsers is three ways for this scan to quietly run over nothing.
        var entriesOnDisk = Directory.GetFiles(CuratedPage.GlossaryRoot, "*.md").Length;
        var blocksOnDisk = Directory
            .GetFiles(CuratedPage.BlocksRoot, "*.md", SearchOption.AllDirectories).Length;
        Assert.Equal(entriesOnDisk + blocksOnDisk, prose.Count);
        Assert.Equal(entriesOnDisk, prose.Count(p => p.Slug.StartsWith("glossary/", StringComparison.Ordinal)));
        Assert.Equal(blocksOnDisk, prose.Count(p => p.Slug.StartsWith("blocks/", StringComparison.Ordinal)));

        // AND EVERY SUBJECT HAS SHINGLES TO COMPARE. A definition that yields an
        // empty window set cannot collide with anything, and a parser returning ""
        // for all of them looks identical to a clean corpus — the vacuous-canary
        // shape §12.24 records.
        var empty = prose.Where(p => shingles[p.Slug].Count == 0).Select(p => p.Slug).ToList();
        Assert.True(empty.Count == 0,
            "a shared source yielded NO eight-word windows, so it cannot collide with "
            + "anything and this scan is blind to it: " + string.Join(", ", empty));

        // OFFENDERS BEFORE STALENESS: a pair whose overlap has MOVED is both, and
        // reporting "stale, delete it" first sends the author away from the finding
        // (ShippedGlossaryTests, /review).
        Assert.True(offenders.Count == 0,
            "a shared source restates another shared source. Both reach more than one "
            + "page, so neither is a place a fact can sit quietly, and on /glossary a "
            + "reader meets every entry (§12.28). Either one of them owns the sentence, "
            + "or the pair is deliberately parallel and gets a record with its reason and "
            + "its windows pinned:\n  " + string.Join("\n  ", offenders));

        var stale = DeliberatelyParallel.Except(recordsUsed).ToList();
        Assert.True(stale.Count == 0,
            "a DeliberatelyParallel record no longer matches any collision, so it is "
            + "stale and should be deleted rather than left to exempt something later:\n  "
            + string.Join("\n  ", stale.Select(s => $"{s.Left} <-> {s.Right}")));

        // The collision count, exactly, per direction — eight per-file counts rather
        // than a sum is §12.27's own correction, and it applies to a count of pairs
        // for the same reason: a total cannot tell a new collision from a vanished one.
        Assert.Equal(DeliberatelyParallel.Length, collisions);

        // PER DIRECTION, tallied from the pairs the scan MEASURED rather than from
        // the record array. The first version counted `DeliberatelyParallel` itself,
        // which is a fact about an array literal 200 lines up and cannot move except
        // when somebody edits that literal — while the comment claimed it was the
        // collision count (/review). These three come off the corpus.
        Assert.Equal(3, entryVsEntry);
        Assert.Equal(1, entryVsBlock);
        Assert.Equal(0, blockVsBlock);
    }

    /// <summary>
    /// The scan above, shown failing and shown discriminating — on text built here,
    /// so it keeps proving this after the corpus is clean.
    ///
    /// Three probes, ONE EDIT APART each, because a probe that changes two things
    /// cannot attribute what it measures to either.
    /// </summary>
    [Fact]
    public void TheSharedSourceScanFiresAndCanTellAParallelPairFromAParaphrase()
    {
        static List<string> Overlap(string a, string b) =>
            [.. CuratedPage.ShinglesOfReaderText(a)
                .Intersect(CuratedPage.ShinglesOfReaderText(b), StringComparer.Ordinal)
                .OrderBy(s => s, StringComparer.Ordinal)];

        const string Frame =
            "A group of gliomas sorted by their biology, not by the age of the person "
            + "who has them.";

        // 1. The frame shared verbatim, each side with its own second half — MUST collide.
        var left = Frame + " Adults usually have one of these.";
        var right = Frame + " Children sometimes have one too.";
        Assert.NotEmpty(Overlap(left, right));

        // 2. One side PARAPHRASED and nothing else — MUST NOT collide. Without this,
        //    a zero anywhere above would mean nothing (§12.26: a shingle gate cannot
        //    see a paraphrase, which is the whole reason this file also carries the
        //    position rule below).
        var paraphrased =
            "A bunch of tumors grouped by how they behave rather than by who gets them. "
            + "Children sometimes have one too.";
        Assert.Empty(Overlap(left, paraphrased));

        // 3. ONE SIDE WITH NO DISTINGUISHING HALF — the pair that is one definition
        //    with two names, which is what the premise check in the scan above
        //    (`onlyLeft == 0 || onlyRight == 0`, lines ~236) exists to catch.
        //
        //    THE FIRST VERSION OF THIS CONTROL WAS `Frame.Except(Frame).Count() == 0`,
        //    which is 0 for EVERY input including the empty set — a positive control
        //    that cannot fail, so the premise check had still never been seen to fire.
        //    /review found it, and it is the third instrument in two items to catch
        //    this one defect class (§12.27: the break harness, reading the code, and
        //    /review). Stub `ShinglesOfReaderText` to return `[]` and the old version
        //    passed unchanged.
        //
        //    So the probe is now ASYMMETRIC, which is the only shape that
        //    discriminates: `Frame` is a strict subset of `subsumed`, so one side has
        //    nothing of its own and the other does.
        var subsumed = Frame + " Adults usually have one of these.";
        static int OwnWindows(string mine, string other) =>
            CuratedPage.ShinglesOfReaderText(mine)
                .Except(CuratedPage.ShinglesOfReaderText(other), StringComparer.Ordinal)
                .Count();

        Assert.Equal(0, OwnWindows(Frame, subsumed));
        Assert.True(OwnWindows(subsumed, Frame) > 0,
            "the asymmetric probe has no own-windows on EITHER side, so it cannot tell "
            + "a subsumed pair from a licensed one and neither assertion above means "
            + "anything");

        // And the licensed shape has own-windows on BOTH sides, so the premise check
        // is seen to pass for a reason as well as to fail for one (§12.18: a guard
        // seen to fail for the WRONG reason has not been shown to work either).
        Assert.True(OwnWindows(left, right) > 0);
        Assert.True(OwnWindows(right, left) > 0);
    }

    // ---------------------------------------------------------------- §12.28, half 2
    //
    // THE POSITION RULE, which is a COMPOSITION rule and never a shingle rule — and
    // that is the point of it. `glossary/tumor-board` and `blocks/tumor-board` state
    // the same three facts in two voices and collide on ZERO windows, so no shingle
    // check reports them and §12.26's remedy ("rewrite the entry") had already been
    // spent on exactly that.
    //
    // §12.26 refused a suppression on /tumors/ependymoma and wrote the reason in a
    // comment: the page "NAMES the term before the subsection that describes it, so
    // the tooltip is a gloss at first mention rather than an echo". That is the rule.
    // GlossaryMarker fires on the FIRST occurrence only, so it is decidable — and it
    // is decided HERE WITHOUT OFFSETS, by rendering each page twice:
    //
    //   fires WITH the block and ALSO WITHOUT it -> the page names the term itself,
    //                                               above the include. A GLOSS. Keep.
    //   fires WITH the block and NOT without it  -> the reader's first meeting with
    //                                               the word is the block's own
    //                                               sentence. An ECHO. Suppress.
    //
    // Two renders rather than two offsets, deliberately: the alternative is to find
    // the tooltip's position in the rendered HTML, and a needle into rendered prose
    // cannot span a glossary term — GlossaryMarker injects `<button ...>term</button>`
    // MID-SENTENCE, which is the defect WI-579 shipped and WI-571 shipped before it.
    // Rendering twice asks the real marker the question instead of re-deriving it.

    private sealed record BlockOwnedDefinition(
        string Entry, string Block, string Include, int Gloss, int Suppressed, int NeverSaid, string Why);

    /// <summary>
    /// The blocks that OWN a fact a glossary entry also states, with the partition of
    /// their including pages pinned per block and counted positively.
    ///
    /// §12.24: a set defined by subtraction is redefined by every new kind of X — and
    /// §12.26's own "twelve including pages never got it" was a subtraction (17 − 5)
    /// that counted three deliberate glosses as omissions. So all three outcomes are
    /// counted, and they must add up to the includers.
    /// </summary>
    private static readonly BlockOwnedDefinition[] BlockOwned =
    [
        new("tumor-board", "blocks/tumor-board", "TUMOR-BOARD",
            Gloss: 3, Suppressed: 14, NeverSaid: 0,
            "The block states all three of the entry's facts in its own voice — what a "
            + "tumor board is, that you are not in the room, and that what it decides is "
            + "advice. The entry has nothing to add where the block is present. Nine "
            + "suppressions were added at WI-580 on top of the five §12.26 found written "
            + "by hand; the three glosses name the term above the include "
            + "(/tests/mri, which asserts that tooltip fires as contract item 9; "
            + "/tests/waiting-for-results, thousands of characters earlier in another "
            + "section; /treatments/craniotomy, in the sentence directly above)."),

        new("posterior-fossa-syndrome", "blocks/posterior-fossa-syndrome",
            "POSTERIOR-FOSSA-SYNDROME",
            Gloss: 3, Suppressed: 0, NeverSaid: 1,
            "NO SUPPRESSIONS, and the position rule is why rather than an exemption: all "
            + "three includers name the syndrome ABOVE the block that describes it, so "
            + "every tooltip is a gloss at first mention. EpendymomaPageTests and "
            + "MedulloblastomaPageTests each assert their tooltip fires, in writing and "
            + "with the reason — and §12.26 records that WI-576's first attempt reddened "
            + "seven such files and was the guard being wrong. This rule reds none of them."),

        new("status-epilepticus", "blocks/escalation", "ESCALATION",
            Gloss: 0, Suppressed: 0, NeverSaid: 25,
            "THE ONE WITH THE BLAST RADIUS AND NO READER. 25 pages include this block "
            + "and not one of them says 'status epilepticus', so there is no first "
            + "occurrence to place and nothing to suppress. The entry's two-window "
            + "overlap with the block is a file collision only, and it is recorded in "
            + "DeliberatelyParallel above with its reason. A block's reach is a reason "
            + "to look, not a finding (§12.28)."),
    ];

    /// <summary>
    /// Every page that includes a block owning a definition either names the term
    /// ITSELF above the include — so the tooltip is a gloss at first mention and
    /// fires — or suppresses the term with <c>!%term%</c>.
    ///
    /// <para>No page is allowed to be an ECHO: the tooltip firing on prose the
    /// reader has just finished reading, in words that can drift apart from it.</para>
    ///
    /// <para>The marker is hand-written per page and never in the block. Three test
    /// files assert <c>DoesNotContain("!%")</c> over <see cref="CuratedPage.SharedSources"/>
    /// because a marker in a block suppresses that term on every including page at
    /// once, silently (WI-510). §12.26 kept that rule and said hand-copying was now
    /// safe "because this item ships the gate that reds on the omission" — and for
    /// `tumor-board` it did not, because the entry had been paraphrased out of the
    /// shingle gate's view. <b>This is the gate that reds on the omission.</b></para>
    /// </summary>
    [Fact]
    public void EveryPageIncludingABlockThatOwnsADefinitionGlossesTheTermOrSuppressesIt()
    {
        var offenders = new List<string>();

        foreach (var owned in BlockOwned)
        {
            var term = CuratedPage.GlossaryTerms.Single(t => t.Slug == owned.Entry);
            var names = new[] { term.Term }.Concat(term.Aliases).ToList();

            // The block's first line of PROSE, read off the file rather than typed, so
            // "this page composed the block" is derived. Markdown, not HTML, so no
            // tooltip markup can have been injected into it.
            var blockBody = CuratedPage.ReaderText(
                CuratedPage.SharedSources().Single(s => s.Slug == owned.Block).Text);
            var firstProseLine = blockBody.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split('\n').First(l => l.Trim().Length > 0).Trim();
            Assert.True(firstProseLine.Length > 30,
                $"{owned.Block}: the derived include marker is too short to identify it");

            int gloss = 0, suppressed = 0, neverSaid = 0, echo = 0;

            foreach (var (slug, text) in CuratedPage.AllPages()
                .Where(p => p.Slug.StartsWith("pages/", StringComparison.Ordinal))
                .Select(p => (Slug: p.Slug["pages/".Length..], p.Text))
                .OrderBy(p => p.Slug, StringComparer.Ordinal))
            {
                var withBlock = CuratedPage.Rendered(slug, text);
                if (!withBlock.Markdown.Contains(firstProseLine, StringComparison.Ordinal))
                {
                    continue;
                }

                // THE PAGE WITHOUT THE BLOCK: the include directive removed and nothing
                // else, so the only difference between the two renders is whose words
                // the first occurrence of the term is in.
                // `[ \t\r]*` AND NOT `[ \t]*`: these files are CRLF, and a multiline
                // `$` will not match before `\r`. ContentBlocks' own summary records
                // that exact trap ("the directive rendered as literal bracket text
                // with no error at all"), and a Replace that matches nothing fails
                // silently — which is why the removal is asserted immediately below
                // rather than assumed (§12.26).
                var withoutBlock = CuratedPage.Rendered(
                    slug, Regex.Replace(text, $@"(?m)^[ \t]*\[{owned.Include}\][ \t\r]*$", ""));
                Assert.DoesNotContain(firstProseLine, withoutBlock.Markdown, StringComparison.Ordinal);

                var firesWith = CuratedPage.GlossaryTooltipsFiringOn(withBlock).Contains(owned.Entry);
                var firesWithout = CuratedPage.GlossaryTooltipsFiringOn(withoutBlock).Contains(owned.Entry);
                // `Body(text)`, matching the suppression gate below rather than
                // reading the raw file. Benign today — a marker in front matter
                // cannot change `firesWith` — but two gates in one file disagreeing
                // about what a marker IS is how the next hole gets in (/review).
                var marked = names.Any(n => CuratedPage.Body(text)
                    .Contains($"!%{n}%", StringComparison.OrdinalIgnoreCase));

                if (firesWith && firesWithout)
                {
                    gloss++;
                }
                else if (firesWith)
                {
                    echo++;
                    offenders.Add($"{slug} includes [{owned.Include}] and the "
                        + $"{owned.Entry} tooltip fires there, but it stops firing when the "
                        + "include is removed — so the reader's FIRST meeting with the word "
                        + "is the block's own sentence and the tooltip echoes prose they "
                        + $"just read (§12.28). Add !%{term.Term}% to the page, one line "
                        + "above the include, or name the term in the page's own prose first.");
                }
                else if (marked)
                {
                    suppressed++;
                }
                else
                {
                    neverSaid++;
                }
            }

            // PER BLOCK AND PER OUTCOME, exactly. §12.27: a summed floor cannot see a
            // pattern die, and it cannot see one added to a recorded page either. The
            // three numbers must also ACCOUNT FOR every includer, which is what stops
            // this being three independent floors with a gap between them.
            Assert.Equal(owned.Gloss, gloss);
            Assert.Equal(owned.Suppressed, suppressed);
            Assert.Equal(owned.NeverSaid, neverSaid);
            Assert.Equal(0, echo);

            // AND THE SET REALLY IS EVERY INCLUDER, derived a SECOND and independent
            // way. The four counts above are a partition of whatever the loop visited;
            // they say nothing about whether it visited the right pages. The loop finds
            // includers by looking for the block's first prose line in the COMPOSED
            // markdown (which is what handles a block included by another block);
            // `ContentBlocks.DirectBlockNames` reads the `[BLOCK]` directive itself.
            // Two derivations of one number, so a drift in either is visible.
            //
            // The first version of this line was
            // `Assert.Equal(Gloss + Suppressed + NeverSaid, gloss + suppressed + ...)`,
            // which is TAUTOLOGICAL given the four assertions above it and read like a
            // partition check while proving nothing — §12.18's own subject, found by
            // reading the code rather than by the harness.
            var byDirective = CuratedPage.AllPages()
                .Where(p => p.Slug.StartsWith("pages/", StringComparison.Ordinal))
                .Count(p => ContentBlocks.DirectBlockNames(p.Text)
                    .Contains(ContentBlocks.ToBlockName(owned.Include)));
            Assert.Equal(byDirective, gloss + suppressed + neverSaid + echo);
            Assert.Equal(owned.Gloss + owned.Suppressed + owned.NeverSaid, byDirective);
        }

        Assert.True(offenders.Count == 0, string.Join("\n  ", offenders));
    }

    /// <summary>
    /// The position rule, shown telling a gloss from an echo — on probe pages built
    /// here, one edit apart, because the corpus now contains no echo at all and a
    /// rule that has never been seen to fire has not been shown to work (§12.18).
    /// </summary>
    [Fact]
    public void ThePositionRuleTellsAGlossFromAnEcho()
    {
        var blocks = ContentBlockStore.Load(CuratedPage.BlocksRoot);

        static string Page(string prose) =>
            "---\ntitle: Probe\ndescription: A probe page.\n---\n\n## A heading\n\n"
            + prose + "\n";

        static bool Fires(string page) =>
            CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered("probe", page))
                .Contains("tumor-board");

        // AN ECHO: the page says nothing about a tumor board; the composed block is
        // where the word first appears.
        var echo = Page("Your team will talk about the plan.\n\n[TUMOR-BOARD]");
        Assert.True(Fires(echo), "the probe's tooltip does not fire at all, so neither "
            + "half of this test is evidence of anything");
        Assert.False(Fires(echo.Replace("[TUMOR-BOARD]", "", StringComparison.Ordinal)),
            "the echo probe still fires with the block removed, so it is not an echo and "
            + "this test cannot tell the two apart");

        // A GLOSS, one edit away: the same page naming the term in its own sentence
        // above the include.
        var gloss = Page("Your team will talk about the plan at a tumor board.\n\n[TUMOR-BOARD]");
        Assert.True(Fires(gloss));
        Assert.True(Fires(gloss.Replace("[TUMOR-BOARD]", "", StringComparison.Ordinal)),
            "the gloss probe stops firing with the block removed, so the page is not "
            + "naming the term itself and the rule above is reading the block");

        // AND THE SUPPRESSION, which must silence both renders — otherwise a page
        // carrying the marker could still be counted as a gloss.
        var marked = Page(
            "!%tumor board%\n\nYour team will talk about the plan at a tumor board.\n\n[TUMOR-BOARD]");
        Assert.False(Fires(marked));
        Assert.False(Fires(marked.Replace("[TUMOR-BOARD]", "", StringComparison.Ordinal)));

        // The block really is being composed into these probes. Without this, every
        // assertion above holds just as well on three pages where [TUMOR-BOARD]
        // resolved to nothing (§12.10's "a test named for a block that asserts the
        // page").
        Assert.NotEmpty(blocks.Blocks);
        var composed = Regex.Replace(CuratedPage.Rendered("probe", echo).Markdown, @"\s+", " ");
        Assert.Contains("A tumor board is a meeting", composed, StringComparison.Ordinal);
    }

    /// <summary>
    /// Every <c>!%term%</c> in the corpus names a real glossary term, and that
    /// term's tooltip WOULD fire on that page if the marker were removed.
    ///
    /// <para><b>This supersedes three page-local copies</b> of
    /// <c>EverySuppressedTermIsARealGlossaryEntryTheProseActuallyUses</c> (in
    /// `AstrocytomaPageTests`, `GliomaPageTests`, `HighGradeGliomaPageTests`) —
    /// absorbed rather than left beside a wider gate saying the same thing about
    /// three of 55 pages. They are deleted, and they carried three defects
    /// between them, every one of them §12.10's "when a guard is copied, its
    /// holes are copied too":</para>
    ///
    /// <list type="number">
    /// <item><b>They read the UNCOMPOSED page</b>, so a term that reaches the
    ///   reader through a block was reported as a word "the page never uses".
    ///   §12.10 states the rule outright — a rule about a block's prose has to run
    ///   on the composed page — and this is what reddened on WI-580's nine
    ///   `!%tumor board%` markers. It was not a finding about those nine:
    ///   <b>four of the five suppressions §12.26 found ALREADY WRITTEN BY HAND
    ///   are in the identical state</b> (`chordoma`, `cns-germ-cell-tumor`,
    ///   `hemangioblastoma`, `spinal-cord-tumor` never say the words in their own
    ///   prose), and they passed only because the guard existed on three pages and
    ///   none of them was one.</item>
    /// <item><b>Two of the three called <c>ReaderText(Body(page))</c></b>, and
    ///   <c>ReaderText</c> already applies <c>Body</c>. The second call searched for
    ///   `"\n---"`, found nothing, added 4 to −1 and returned <b>the body with three
    ///   characters chopped off</b> — the exact trap <see cref="CuratedPage.ReaderTextOfBody"/>
    ///   was written for. Harmless by luck for the terms they checked.
    ///   <b>WI-566 made that shape throw</b>, so the luck is no longer load-bearing:
    ///   <c>Body</c> refuses anything that is not a whole page.</item>
    /// <item><b>A substring scan of reader text is not the firing rule.</b> It
    ///   cannot see that a heading and a link cannot fire a tooltip (§12.11, both
    ///   directions), so "the word is on the page" and "there was something to
    ///   suppress" are different claims.</item>
    /// </list>
    ///
    /// <para>So the property is asked of the REAL marker instead of re-derived:
    /// remove that one marker, render, and the tooltip must appear. That covers
    /// every hole above at once — a term in a citation title, a heading or a link
    /// does not fire, and a term arriving from a block does.</para>
    /// </summary>
    [Fact]
    public void EverySuppressionInTheCorpusNamesATermThatWouldOtherwiseFire()
    {
        var byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var term in CuratedPage.GlossaryTerms)
        {
            foreach (var name in new[] { term.Term }.Concat(term.Aliases))
            {
                byName[name] = term.Slug;
            }
        }

        var offenders = new List<string>();
        var markersSeen = 0;
        var pagesWithMarkers = 0;

        foreach (var (slug, text) in CuratedPage.AllPages()
            .Where(p => p.Slug.StartsWith("pages/", StringComparison.Ordinal))
            .Select(p => (Slug: p.Slug["pages/".Length..], p.Text))
            .OrderBy(p => p.Slug, StringComparer.Ordinal))
        {
            // THE BODY, NOT THE FILE. `GlossaryMarker.Mark` runs on the parsed
            // body — `ContentStore.Parse` splits the front matter off first — so a
            // marker in a YAML comment is invisible to the real renderer. The three
            // page-local copies scanned the whole file and got away with it; this
            // one did not, because `/tumors/medulloblastoma`'s front matter explains
            // the suppression rule and writes `!%...%` while doing so.
            var markers = Regex.Matches(CuratedPage.Body(text), @"!%(.+?)%")
                .Select(m => m.Groups[1].Value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (markers.Count == 0)
            {
                continue;
            }

            pagesWithMarkers++;

            foreach (var name in markers)
            {
                markersSeen++;

                if (!byName.TryGetValue(name, out var entry))
                {
                    offenders.Add($"{slug}: !%{name}% is not a glossary term or alias, "
                        + "so the suppression is a no-op");
                    continue;
                }

                // THAT ONE MARKER REMOVED AND NOTHING ELSE, so each suppression on a
                // page with several is attributable. Every other marker stays, which
                // is correct: they suppress other terms.
                var without = text.Replace($"!%{name}%", "", StringComparison.OrdinalIgnoreCase);
                Assert.NotEqual(text, without);

                if (CuratedPage.GlossaryTooltipsFiringOn(
                        CuratedPage.Rendered(slug, without)).Contains(entry))
                {
                    continue;
                }

                offenders.Add($"{slug}: !%{name}% suppresses nothing — with the marker "
                    + $"removed, glossary/{entry} still does not fire on this page. The "
                    + "word is absent from the composed prose, or it appears only in a "
                    + "heading or a link, where no tooltip can fire (§12.11).");
            }
        }

        // THE COUNT IS PINNED EXACTLY, AND IT HAS TO BE A LITERAL.
        //
        // The first version asserted `markersSeen <= markersOnDisk`, where
        // `markersOnDisk` was derived by the same kind of regex — which /review
        // measured as 109 against 111 and is therefore a CEILING, not a floor. It
        // cannot fail. Degrade the scan to one hit on one page (a change to `Body`
        // is the obvious way, and it is the three-chars-chopped trap this file's own
        // doc comment cites) and `1 > 0` and `1 <= 111` both pass while the gate
        // covers 1 of 109 markers.
        //
        // A DERIVED number cannot replace it either: a degraded regex degrades both
        // sides and the equality stays true. So the number is a literal, measured:
        // 109 markers across 35 pages, and distinct-per-page equals raw-in-body
        // exactly (no page repeats a marker). The 111-in-file figure is two YAML
        // COMMENTS that document the suppression rule, which `Body()` correctly
        // excludes because `GlossaryMarker` never sees front matter.
        //
        // AND THIS IS WHAT RESTORES THE COVERAGE THE THREE DELETED GUARDS HAD.
        // Each carried `Assert.NotEmpty(suppressed)`; this gate is conditional on a
        // marker EXISTING, so without an exact count it cannot see a DELETION.
        // /review measured five markers whose entry overlaps their page by ZERO
        // shingles (`!%transformation%`, `!%chemoradiation%`, `!%pseudoprogression%`
        // on high-grade-glioma, `!%astrocyte%` on astrocytoma, `!%diffuse glioma%`
        // on glioma) — so `ShippedGlossaryTests` is blind to their deletion too,
        // which is §12.28's own "a shingle cannot see a paraphrase" landing on this
        // item's own deletion. Delete any one of the 109 and this line reds.
        Assert.Equal(109, markersSeen);
        Assert.Equal(35, pagesWithMarkers);

        // NO EXEMPTION LIST, AND THAT IS WI-581's DOING. This gate shipped with a
        // `KnownNoOps` table of six markers that suppressed nothing, each recorded
        // with its measured cause and handed to WI-581 rather than fixed here. That
        // item resolved all six — five sections of `/tests/molecular-markers` and one
        // paragraph of `/treatments/radiation-therapy` now say the term their marker
        // names — so the converse check this list carried has fired its last time and
        // the list is deleted rather than kept at zero entries (§12.26, WI-578: a list
        // only ever read as "skip these" cannot tell you one of them stopped being
        // true, and an empty one cannot tell you anything at all).
        //
        // The gate is now UNCONDITIONAL over all 109 markers, which is what makes it
        // the thing that reds when a page stops saying a word it suppresses — the
        // hazard §12.26 named and §12.29's rule for cause (a) depends on.
        Assert.True(offenders.Count == 0,
            "a suppression marker suppresses nothing. WI-512, WI-514 and WI-515 each "
            + "shipped one, and a no-op marker is worse than no marker: it reads as a "
            + "decision somebody made:\n  " + string.Join("\n  ", offenders));
    }

    /// <summary>
    /// No suppression marker leaves an EMPTY PARAGRAPH in the rendered page.
    ///
    /// <para>A marker is stripped from the rendered output, so a marker that is the
    /// only thing in its paragraph renders as <c>&lt;p&gt;&lt;/p&gt;</c> — a visible
    /// gap, and an empty block element on a site where WCAG AA is a hard
    /// requirement. The corpus's own convention is to append the marker to the END
    /// of the preceding paragraph (<c>/tumors/chordoma</c>: <c>"...the standard drug
    /// options are thin.!%tumor board%"</c>), and WI-580 added eleven markers and put
    /// the first nine of them on their own line before noticing.</para>
    ///
    /// <para>Measured over production before this was written: <b>zero</b>
    /// <c>&lt;p&gt;&lt;/p&gt;</c> on the live site, so this is a floor the corpus
    /// already meets rather than a standard being invented. It is asserted over the
    /// whole corpus rather than over the eleven, because the rule is about the
    /// marker and not about this item.</para>
    /// </summary>
    [Fact]
    public void NoSuppressionMarkerLeavesAnEmptyParagraphInTheRenderedPage()
    {
        var offenders = new List<string>();
        var scanned = 0;
        var withMarkers = 0;

        foreach (var (slug, text) in CuratedPage.AllPages()
            .Where(p => p.Slug.StartsWith("pages/", StringComparison.Ordinal))
            .Select(p => (Slug: p.Slug["pages/".Length..], p.Text))
            .OrderBy(p => p.Slug, StringComparer.Ordinal))
        {
            scanned++;
            if (CuratedPage.Body(text).Contains("!%", StringComparison.Ordinal))
            {
                withMarkers++;
            }

            var html = CuratedPage.Rendered(slug, text).Html;

            // Whitespace-tolerant: Markdig may emit a newline inside the element.
            var empties = Regex.Matches(html, @"<p>\s*</p>").Count;
            if (empties > 0)
            {
                offenders.Add($"{slug}: {empties} empty paragraph(s) in the rendered page. "
                    + "A marker alone in its own paragraph renders as one once the marker "
                    + "is stripped — append it to the end of the preceding paragraph "
                    + "instead, which is what the rest of the corpus does.");
            }
        }

        // THE FLOORS, computed. A scan that renders nothing, or a corpus with no
        // markers in it, would pass this silently (§12.24).
        var pagesOnDisk = Directory
            .GetFiles(Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web",
                "Content", "pages"), "*.md", SearchOption.AllDirectories).Length;
        Assert.Equal(pagesOnDisk, scanned);
        Assert.True(withMarkers > 10,
            $"only {withMarkers} pages carry a suppression marker, so this scan cannot "
            + "be evidence about markers");

        Assert.True(offenders.Count == 0, string.Join("\n  ", offenders));
    }

    /// <summary>
    /// The empty-paragraph gate, shown firing — because the corpus is clean, so the
    /// rule has never been seen to fail (§12.18).
    /// </summary>
    [Fact]
    public void TheEmptyParagraphGateSeesAMarkerAloneInItsOwnParagraph()
    {
        static string Rendered(string prose) =>
            CuratedPage.Rendered("probe",
                "---\ntitle: Probe\ndescription: A probe page.\n---\n\n" + prose + "\n").Html;

        // A marker ALONE in its paragraph — the shape this gate exists to catch.
        Assert.Matches(@"<p>\s*</p>",
            Rendered("Your case is discussed.\n\n!%tumor board%\n\nA tumor board meets."));

        // The SAME suppression, appended to the preceding paragraph — the corpus's
        // own convention, and it must be clean. One edit apart, so the difference is
        // attributable to the placement and nothing else.
        Assert.DoesNotMatch(@"<p>\s*</p>",
            Rendered("Your case is discussed.!%tumor board%\n\nA tumor board meets."));

        // And both really do suppress, so the comparison above is between two WORKING
        // suppressions rather than between a working one and a typo.
        foreach (var page in new[]
        {
            "Your case is discussed.\n\n!%tumor board%\n\nA tumor board meets.",
            "Your case is discussed.!%tumor board%\n\nA tumor board meets.",
        })
        {
            Assert.DoesNotContain("def-tumor-board", Rendered(page), StringComparison.Ordinal);
        }

        // ...and that the term WOULD fire without the marker, or neither absence above
        // says anything.
        Assert.Contains("def-tumor-board",
            Rendered("Your case is discussed.\n\nA tumor board meets."),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The suppression gate above, shown failing on each of the three shapes it
    /// supersedes a weaker check for — on probe pages, one edit apart.
    /// </summary>
    [Fact]
    public void TheSuppressionGateSeesAMarkerForAWordNoTooltipCouldFireOn()
    {
        static bool WouldFire(string page, string name, string entry)
        {
            var without = page.Replace($"!%{name}%", "", StringComparison.Ordinal);
            Assert.NotEqual(page, without);
            return CuratedPage.GlossaryTooltipsFiringOn(
                CuratedPage.Rendered("probe", without)).Contains(entry);
        }

        static string Page(string prose) =>
            "---\ntitle: Probe\ndescription: A probe page.\n---\n\n" + prose + "\n";

        // 1. THE SANCTIONED SHAPE: the word is in the prose, so the marker suppresses
        //    something real.
        Assert.True(WouldFire(
            Page("!%tumor board%\n\nYour case goes to a tumor board first."),
            "tumor board", "tumor-board"));

        // 2. THE WORD ARRIVES FROM A BLOCK and the page never says it — the state
        //    thirteen pages in this corpus are in, and the one the three page-local
        //    copies called a no-op.
        Assert.True(WouldFire(
            Page("!%tumor board%\n\nYour team will talk about the plan.\n\n[TUMOR-BOARD]"),
            "tumor board", "tumor-board"));

        // 3. ONLY IN A HEADING — no tooltip can fire there (§12.11), so the marker
        //    suppresses nothing. A substring scan of reader text passes this.
        Assert.False(WouldFire(
            Page("!%tumor board%\n\n## What a tumor board is\n\nYour case is discussed."),
            "tumor board", "tumor-board"));

        // 4. ONLY IN LINK TEXT — likewise.
        Assert.False(WouldFire(
            Page("!%tumor board%\n\nRead about the [tumor board](/glossary) first."),
            "tumor board", "tumor-board"));

        // 5. NOT ON THE PAGE AT ALL.
        Assert.False(WouldFire(
            Page("!%tumor board%\n\nThis page is about something else."),
            "tumor board", "tumor-board"));
    }

    // ---------------------------------------------------------------- §12.28, the pons
    //
    // THE RESIDUAL DUPLICATION §12.26 HANDED OVER, AND THE LIVE MEDICAL ERROR IN IT.
    //
    // /tumors/dipg and /tumors/diffuse-midline-glioma both shipped "The pons carries
    // the nerves for VISION, hearing, speech, swallowing and MOVEMENT" while
    // glossary/pons — corrected at WI-576's /review — said "eye movement ... and
    // BALANCE". The visual pathway does not pass through the pons; and each page's
    // OWN symptom list, three sections below, already said "Double vision, or eyes
    // that do not move together" and "Trouble with balance or walking". The pages
    // refuted themselves.
    //
    // Both pages now own the anatomy (each defines "pons" inline at its first mention
    // and answers it under a heading that asks for it — §12.27: an answer to the
    // question a section is headed with cannot be a link, and it cannot be a tooltip
    // either), so `!%pons%` is suppressed on both. §12.26 named the hazard that
    // creates: delete the sentence and leave the marker and every guard goes quiet.
    //
    // SO THE GATE IS THE SET OF FUNCTIONS EACH COPY NAMES, not a shingle. A shingle
    // cannot see "vision" replaced by "eye movement" once the words around it differ,
    // and a banned-word list is a floor rather than a fence (§12.18). Set equality is
    // the one form that sees a paraphrase of a LIST.

    /// <summary>
    /// The functions this corpus says the pons carries, canonicalised. Order matters:
    /// <c>eye movement</c> is matched and consumed BEFORE <c>movement</c>, or every
    /// correct sentence also reports the word the correction removed.
    /// </summary>
    private static readonly (string Pattern, string Name)[] PonsFunctions =
    [
        ("eye movement", "EyeMovement"),
        ("vision", "Vision"),
        ("hearing", "Hearing"),
        ("speaking", "Speech"),
        ("speech", "Speech"),
        ("swallowing", "Swallowing"),
        ("balance", "Balance"),
        ("movement", "Movement"),
    ];

    /// <summary>
    /// The functions a sentence names, as a set.
    ///
    /// <para>WHOLE-WORD, and ALL occurrences consumed. Both halves were wrong in
    /// the first version and /review measured both:</para>
    /// <list type="bullet">
    /// <item><c>IndexOf</c> is a SUBSTRING match, so <c>"under supervision"</c>
    ///   reported <c>Vision</c> and <c>"causing imbalance"</c> reported
    ///   <c>Balance</c>. This file argues that set equality is the one form that
    ///   can see a paraphrase of a list, and a substring match is not set
    ///   membership. The lookarounds are `GlossaryMarker.BuildMatchers`' own.</item>
    /// <item>Consuming only the FIRST occurrence made
    ///   <c>"Eye movement and eye movement"</c> report <c>{EyeMovement, Movement}</c>
    ///   — a spurious function on correct writing, which would red the gate on a
    ///   sentence that is right. §12.8: a rule that fails a correct page is worse
    ///   than no rule.</item>
    /// </list>
    /// <para>Verified against all three shipped sentences before and after: the sets
    /// are unchanged, so this closes two holes without moving the measurement.</para>
    /// </summary>
    private static SortedSet<string> FunctionsNamedIn(string sentence)
    {
        var text = Regex.Replace(sentence, @"\s+", " ");
        var found = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var (pattern, name) in PonsFunctions)
        {
            // EVERY occurrence, longest pattern first, so "eye movement" is fully
            // consumed before "movement" is looked for at all.
            var probe = new Regex($@"(?<![\w-]){Regex.Escape(pattern)}(?![\w-])",
                RegexOptions.IgnoreCase);
            for (var m = probe.Match(text); m.Success; m = probe.Match(text))
            {
                found.Add(name);
                text = text.Remove(m.Index, m.Length);
            }
        }

        return found;
    }

    /// <summary>
    /// The sentence containing <paramref name="needle"/>, from flattened reader text.
    /// Flattened on both sides: these files are hard-wrapped, and §12.26's own deploy
    /// failed on a needle that crossed a wrap.
    /// </summary>
    private static string SentenceContaining(string readerText, string needle)
    {
        var flat = Regex.Replace(readerText, @"\s+", " ");
        var sentences = Regex.Split(flat, @"(?<=[.!?]) ");
        var hits = sentences.Where(s => s.Contains(needle, StringComparison.Ordinal)).ToList();

        // NAMED, because a bare Assert.Single here reports "the collection contained
        // 2 matching items" and leaves the reader of a red build with no idea which
        // needle in which of three files, nor whether the real fault is that a second
        // sentence appeared or that the first one was reworded away.
        Assert.True(hits.Count == 1,
            $"expected exactly one sentence containing \"{needle}\", found {hits.Count}"
            + (hits.Count == 0
                ? " — the sentence has been reworded or deleted"
                : ": " + string.Join(" || ", hits)));
        return hits[0];
    }

    /// <summary>
    /// The whole anatomy claim, which is TWO sentences on all three copies since
    /// /review restored <c>movement</c>: what the nerve paths carry, and the limb
    /// movement that the pages' own symptom lists name.
    ///
    /// <para>Both halves are required to be present, so dropping the second one —
    /// which is exactly the edit /review caught this item making — reds rather than
    /// quietly shrinking the set this gate compares.</para>
    /// </summary>
    private static string AnatomyPassage(string readerText) =>
        SentenceContaining(readerText, "pass through it")
        + " " + SentenceContaining(readerText, "movement in your arms and legs");

    /// <summary>
    /// The two pages that own the pons anatomy still carry it, and all three copies
    /// name the SAME SET of functions.
    ///
    /// <para>Three things asserted together, because each one alone can be satisfied
    /// by the others going wrong: the pages still carry the sentence (the premise the
    /// suppression rests on), the three copies agree (the drift gate), and the entry
    /// still reaches a reader somewhere (the cost of the suppression, bounded).</para>
    /// </summary>
    [Fact]
    public void BothPonsPagesStillCarryTheAnatomyAndAllThreeCopiesNameTheSameFunctions()
    {
        var entry = CuratedPage.GlossaryTerms.Single(t => t.Slug == "pons");
        var expected = FunctionsNamedIn(AnatomyPassage(entry.Definition));

        // THE SET IS WRITTEN OUT, not just compared. Three copies that agree on the
        // WRONG set would pass a pure equality check, which is the whole defect this
        // item found: the two pages agreed with each other perfectly.
        //
        // `Movement` IS IN THIS SET, and /review is why. WI-580's first fix replaced
        // `vision` with `eye movement` (right) and also dropped `movement` (wrong) —
        // leaving a cause sentence that cannot explain a symptom the same page still
        // lists, "Weakness in an arm and a leg". That is this item's own
        // self-refutation argument running in reverse, and unlike `vision` the
        // source's `movement` is not loose labelling: the corticospinal tract runs
        // through the basis pontis. Restoring it is undoing this item's deletion,
        // not adding a claim.
        Assert.Equal(
            new SortedSet<string>(StringComparer.Ordinal)
                { "Balance", "EyeMovement", "Hearing", "Movement", "Speech", "Swallowing" },
            expected);

        // AND `Vision` IS NOT, said out loud rather than left to the set above —
        // because that is the word this item exists to have removed, and an equality
        // check reports "the sets differ" without naming which member is the defect.
        Assert.DoesNotContain("Vision", expected);

        foreach (var (dir, file) in new[] { ("tumors", "dipg"), ("tumors", "diffuse-midline-glioma") })
        {
            var raw = CuratedPage.Read(dir, file + ".md");
            var reader = CuratedPage.ReaderText(raw);

            // THE PREMISE: the page still carries the sentence that makes it the owner.
            // §12.26's /review found this is the one edit in this area that really hurts
            // a reader — delete the defining sentence, leave `!%pons%` in place, and the
            // reader of the page that says the word has no definition anywhere on it.
            Assert.Equal(expected, FunctionsNamedIn(AnatomyPassage(reader)));

            // THE INLINE GLOSS AT **FIRST** MENTION, which is the other half of what
            // makes the suppression sanctioned rather than a deletion (§12.26's second
            // shape): the reader who meets the word with no tooltip on it is told what
            // it means in the same sentence.
            //
            // "FIRST" is the whole assertion and the first version of this check did
            // not contain it. `Contains("part of the brain stem", reader)` is satisfied
            // by ANY occurrence, and both pages say "brain stem" three or four times —
            // so deleting the gloss from the sentence that needs it left the check
            // green. §12.10's recorded shape: a test asserting a PHRASE where the claim
            // was its POSITION.
            // AND THE FIRST VERSION OF THE "FIRST" FIX WAS STILL TOO WEAK, which
            // /review measured. Two defects, both in the splitting:
            //
            //   * headings were not removed, so `firstMention` began "## The short
            //     version ..." — a heading is not a sentence and cannot gloss anything;
            //   * `(?<=[.!?]) ` does not split after a BOLD-terminated sentence,
            //     because the `.` is followed by `*` and not by a space. Both pages
            //     end their lede sentence in bold, so `firstMention` was the heading
            //     PLUS TWO SENTENCES.
            //
            // The attack that passed: `**DIPG is a fast-growing tumor in the pons.**
            // It is a part of the brain stem, and ...`. The reader who meets the word
            // with no tooltip is no longer told what it means in that sentence, and
            // the old assertion returned true anyway — §12.10's phrase-versus-position
            // shape, surviving the fix written to remove it. Measured both ways before
            // and after.
            //
            // Headings are stripped BEFORE flattening: afterwards there are no line
            // starts left for `(?m)^#` to anchor to, and a flattened strip would eat
            // the rest of the page.
            var flat = Regex.Replace(
                Regex.Replace(reader, @"(?m)^[ \t]*#{1,6}[ \t].*$", ""), @"\s+", " ");
            var firstMention = Regex.Split(flat, @"(?<=[.!?])[*_)""']*\s")
                .First(s => Regex.IsMatch(s, @"(?<![\w-])pons(?![\w-])", RegexOptions.IgnoreCase));
            Assert.DoesNotContain("##", firstMention, StringComparison.Ordinal);
            Assert.Contains("brain stem", firstMention, StringComparison.OrdinalIgnoreCase);

            // The suppression itself, so the decision is pinned and not inferred.
            Assert.Contains("!%pons%", raw, StringComparison.Ordinal);
        }

        // THE COST OF THE SUPPRESSION, BOUNDED. /where-your-tumor-is names the pons as
        // one of three parts of the brain stem and explains none of them, so there the
        // entry is the only explanation that reader gets — which is why trimming the
        // entry instead was refused. If this ever reaches zero, the entry has stopped
        // reaching any reader through a tooltip and belongs to WI-581's question.
        var firing = CuratedPage.AllPages()
            .Where(p => p.Slug.StartsWith("pages/", StringComparison.Ordinal))
            .Where(p => CuratedPage.GlossaryTooltipsFiringOn(
                CuratedPage.Rendered(p.Slug["pages/".Length..], p.Text)).Contains("pons"))
            .Select(p => p.Slug)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(["pages/where-your-tumor-is"], firing);
    }

    /// <summary>
    /// The function-set extractor, shown seeing the defect it exists to prevent.
    ///
    /// Two probes one edit apart, plus the ordering trap: <c>eye movement</c> must
    /// not also report <c>Movement</c>, or every corrected sentence names a function
    /// it deliberately dropped and the sets never agree.
    /// </summary>
    [Fact]
    public void TheFunctionSetExtractorSeesTheWordingThisItemCorrected()
    {
        var corrected = FunctionsNamedIn(
            "The nerve paths for eye movement, hearing, speech, swallowing and balance "
            + "pass through it.");
        var asShipped = FunctionsNamedIn(
            "The pons carries the nerves for vision, hearing, speech, swallowing and "
            + "movement.");

        Assert.NotEqual(corrected, asShipped);
        Assert.Contains("EyeMovement", corrected);
        Assert.DoesNotContain("Vision", corrected);
        Assert.Contains("Balance", corrected);

        // THE ORDERING TRAP, asserted rather than trusted to the list's order.
        Assert.DoesNotContain("Movement", corrected);
        Assert.Contains("Movement", asShipped);
        Assert.Contains("Vision", asShipped);
        Assert.DoesNotContain("Balance", asShipped);

        // And "speaking" and "speech" are the same function, which is what lets the
        // entry and the pages word it differently and still be gated.
        Assert.Equal(
            FunctionsNamedIn("paths for speaking pass through it"),
            FunctionsNamedIn("paths for speech pass through it"));

        // A HARD WRAP MUST NOT HIDE A FUNCTION. §12.26's own post-deploy failure was a
        // needle that crossed one, and these files are hard-wrapped at 80 columns.
        Assert.Equal(corrected, FunctionsNamedIn(
            "The nerve paths for eye\nmovement, hearing, speech, swallowing and\nbalance "
            + "pass through it."));

        // WHOLE-WORD, NOT SUBSTRING. /review measured both of these reporting a
        // function that is not named: a set comparison is only evidence if the
        // extractor does set MEMBERSHIP rather than substring search.
        Assert.DoesNotContain("Vision", FunctionsNamedIn("Paths under supervision pass through it."));
        Assert.DoesNotContain("Balance", FunctionsNamedIn("Paths causing imbalance pass through it."));

        // ALL OCCURRENCES CONSUMED, not just the first. A sentence that legitimately
        // names eye movement twice must not gain a spurious `Movement` — that reds
        // the gate on correct writing, which §12.8 calls worse than no rule.
        Assert.Equal(
            new SortedSet<string>(StringComparer.Ordinal) { "EyeMovement" },
            FunctionsNamedIn("Eye movement and eye movement pass through it."));

        // AND THE RESTORED LIMB CLAUSE IS SEEN, which is the half /review added. It
        // has to coexist with "eye movement" in the same passage without either
        // swallowing the other.
        Assert.Equal(
            new SortedSet<string>(StringComparer.Ordinal) { "EyeMovement", "Movement" },
            FunctionsNamedIn("The paths for eye movement pass through it. So do the "
                + "paths for movement in your arms and legs."));
    }
}
