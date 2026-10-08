using BrainHarbor.ContentCheck;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-582, §12.30: a glossary definition is reader-facing prose, and it is now GRADED
/// AND GATED at the 6.0 a page is held to.
///
/// <para><b>THE SET THIS FILE IS BUILT OVER</b> (§12.19 asks this first, §12.24 asks it
/// of entries and §12.25 of the complement): the 105 shipped <c>glossary/</c> entries,
/// of which <b>101 are GRADED and 4 are not</b> — <c>memantine</c> at 12 words,
/// <c>procarbazine</c> at 18, <c>pcv</c> at 19 and <c>stereotactic-radiosurgery</c> at
/// 19, all under <see cref="ContentChecker.MinimumDefinitionWordsToGrade"/>. They are
/// <b>NAMED here rather than subtracted</b>, and the ungraded set is pinned as a SET
/// rather than as the number 4, because the number is satisfied by any four entries.</para>
///
/// <para><b>WHY THE SET IS THE SUBJECT OF A TEST AT ALL: it is the escape hatch.</b> A
/// definition trimmed to 19 words leaves the graded set, which takes the over-the-limit
/// count DOWN and reads as progress. §12.23 records that exact hole shipping once
/// already, at the identical 25-word description floor, with <c>GradeDescription</c>'s
/// own comment having warned about it since WI-575 — <b>writing a hazard down does not
/// guard it</b>. The only instrument that caught a shortening during WI-578 slice 1 was
/// a <c>len(new) &gt;= len(old)</c> assertion in that item's <b>git-ignored scratch
/// script</b>. It is in the suite this time, twice: as the set equality in
/// <see cref="TheUngradedSetIsExactlyTheFourEntriesUnderTheWordFloor"/> and as a
/// per-entry word FLOOR in <see cref="TheRewrittenDefinitionsKeepTheirWords"/>.</para>
///
/// <para><b>WHAT WI-581 ALREADY FLOORS, AND WHY THIS IS NOT A SUPERSESSION</b> (§12.28:
/// the thing to diff is the assertion list). <c>GlossaryOnlyEntriesTests</c>'
/// <c>EveryGlossaryOnlyEntryIsStillReachableWithADefinition</c> floors the <b>26
/// glossary-only</b> definitions' LENGTH IN CHARACTERS at the term's length + 20, to
/// rule out a row that is only the term restated. This file floors <b>35 definitions'
/// WORDS</b> at the count each one holds today, to rule out a readability fix that is a
/// truncation. <b>Two instruments, two properties, two overlapping sets</b> — 10 entries
/// carry both — and neither subsumes the other: the character floor reaches 16 entries
/// this file says nothing about, and on the 10 it is slack by two orders of magnitude,
/// which <see cref="TheTwoFloorsOverlapOnTenEntriesAndNeitherSubsumesTheOther"/>
/// measures rather than claims.</para>
///
/// <para><b>AND THE `(not gated)` STRING IS DELETED</b>, the way WI-578 deleted
/// <c>DescriptionsOverTheLimit</c>, <c>WorstDescriptionGrade</c> and the
/// <c>(not gated — WI-578)</c> marker together with its ratchet. There is no
/// reported-but-ungated definition grade left for it to be true of.</para>
/// </summary>
public sealed class GlossaryDefinitionGradeTests
{
    /// <summary>
    /// The 4 entries <see cref="ContentChecker.CheckGlossaryTerm"/> cannot grade, by
    /// name, in ordinal order.
    ///
    /// <para><b>They are not an exemption list and this file would be wrong to call them
    /// one.</b> Nothing decided that these four may read above sixth grade; what decided
    /// it is their LENGTH, measured by a rule that predates this item by four items and
    /// carries its reason in the code — Flesch-Kincaid on a sample this short says more
    /// about the sample than about the writing. The ruling WI-582's acceptance criteria
    /// asked for (<i>"either the standard excludes them with a reason, or the word floor
    /// does, or they are exempted individually"</i>) is settled by the MIDDLE option, and
    /// the criteria's own worry is half answered by it:
    /// <b><c>pcv</c> and <c>procarbazine</c> are two of these four</b>, so they are not
    /// graded at all and were never candidates for being rewritten to game a syllable
    /// count. The other two it names — <c>lomustine</c> and <c>carmustine-wafer</c> —
    /// ARE in the graded set, and both were written under 6.0 with their drug names
    /// intact (9.3 → 3.4 and 8.4 → 3.3). <b>No entry is exempted individually and there
    /// is no exemption list.</b></para>
    /// </summary>
    private static readonly string[] UnderTheWordFloor =
        ["memantine", "pcv", "procarbazine", "stereotactic-radiosurgery"];

    /// <summary>
    /// What each of the four WOULD grade if it crossed the floor, pinned so the
    /// exemption is checkable rather than asserted — and because it is the one fact that
    /// makes the floor's shape legible: <b>it is not a hiding place for entries that
    /// could not pass.</b>
    ///
    /// <para>Three of the four would FAIL on sight (<c>pcv</c> at 16.1 — it is three
    /// drug names; <c>memantine</c> 10.4; <c>procarbazine</c> 7.6) and
    /// <c>stereotactic-radiosurgery</c> would pass at 4.9. So growing any of the first
    /// three into the graded set breaks the build until it is rewritten, which is the
    /// opposite of an escape, and <b>the naive sweep of all 105 that reported 38 over
    /// the limit and a worst of 16.1 was reading exactly these three</b> — WI-416 is
    /// about that, and the second grader's cost was a wrong number that nearly reached
    /// a doc.</para>
    /// </summary>
    private static readonly (string Slug, double WouldGrade)[] IfTheyCrossedTheFloor =
    [
        ("memantine", 10.4),
        ("pcv", 16.1),
        ("procarbazine", 7.6),
        ("stereotactic-radiosurgery", 4.9),
    ];

    /// <summary>
    /// The 35 definitions this item rewrote, with <b>the word count each held BEFORE,
    /// the grade it held BEFORE, and the word count it holds NOW as a FLOOR.</b>
    ///
    /// <para><b>THE FLOOR IS THE POINT AND THE GRADE GATE CANNOT REPLACE IT.</b> Since
    /// this item the grade is a hard Fail above 6.0 for every graded entry, so nothing
    /// can drift over the limit unseen. What no grade gate can see is a definition
    /// getting SHORTER: trim one of these from 38 words to 24 and it stays graded, stays
    /// under 6.0, and quietly stops saying something. §12.23 calls that a shortening
    /// wearing a split's clothes, and it is invisible by inspection — the prose reads
    /// better and the grade improves.</para>
    ///
    /// <para><b>EVERY BEFORE-GRADE IN THIS TABLE IS ABOVE 6.0, asserted below</b>, so a
    /// row cannot be added for an entry that was never over the limit — which is how a
    /// table like this becomes a place to park a floor nobody argued for.</para>
    ///
    /// <para><b>AND THE THESIS IS IN THE TABLE'S OWN NUMBERS: 1,099 words before,
    /// 1,187 after, NOT ONE REMOVED FROM ANY OF THEM.</b> 22 of the 35 grew,
    /// 13 held exactly level, none fell. §12.22 diagnosed sentence length rather than
    /// vocabulary and §12.23/§12.24/§12.25 proved it for three directories of page
    /// descriptions; this is the fourth proof and the first on a surface with a 40-WORD
    /// CEILING over it (content-pipeline §6). Two rows of this table sit at exactly 40 —
    /// <c>h3-g34</c> and <c>tumor-treating-fields</c> — and <c>h3-g34</c> was already
    /// at 40 BEFORE, which made it the one entry whose fourth sentence boundary had to
    /// cost nothing at all. (Six entries sit at 40 corpus-wide; the other four are not
    /// this item's and are not asserted here.) See §12.30.</para>
    /// </summary>
    private static readonly (string Slug, int WordsBefore, double GradeBefore, int WordFloor)[]
        Rewritten =
    [
        new("1p-19q-co-deletion", 26, 8.5, 33),
        new("adult-type", 39, 6.4, 39),
        new("amended-report", 34, 6.1, 35),
        new("anesthesiologist", 34, 6.3, 34),
        new("atrx", 25, 7.2, 32),
        new("bevacizumab", 31, 6.4, 39),
        new("blood-thinner", 37, 6.2, 37),
        new("brain-mapping", 36, 7.5, 39),
        new("carmustine-wafer", 28, 8.4, 35),
        new("chemoradiation", 39, 7.8, 39),
        new("chromosome-7-and-10", 37, 7.4, 38),
        new("circumscribed", 36, 7.8, 36),
        new("egfr-amplification", 28, 6.7, 32),
        new("glioblastoma", 29, 6.5, 36),
        new("glioma", 34, 7.2, 38),
        new("h3-g34", 40, 6.1, 40),
        new("h3-k27-altered", 36, 7.2, 38),
        new("integrated-diagnosis", 28, 6.3, 30),
        new("lomustine", 28, 9.3, 33),
        new("methylation-profiling", 29, 6.7, 32),
        new("nadir", 32, 7.2, 32),
        new("neuro-oncologist", 31, 6.1, 31),
        new("neutropenia", 30, 7.6, 30),
        new("palliative-care", 26, 7.2, 26),
        new("pediatric-type", 35, 7.5, 37),
        new("pseudoprogression", 26, 6.3, 28),
        new("simulation", 28, 6.2, 28),
        new("somnolence-syndrome", 29, 7.6, 32),
        new("stereotactic-biopsy", 35, 7.4, 36),
        new("subtotal-resection", 26, 6.3, 26),
        new("transformation", 29, 6.4, 29),
        new("tumor-treating-fields", 33, 8.4, 40),
        new("vasogenic-edema", 23, 6.3, 27),
        new("vorasidenib", 27, 6.3, 33),
        new("watch-and-wait", 35, 8.1, 37),
    ];

    /// <summary>
    /// The 10 entries that are BOTH in <see cref="Rewritten"/> and in WI-581's 26
    /// glossary-only set — pinned by name and cross-checked against the live
    /// computation, which is what makes it a diff rather than a sentence.
    /// </summary>
    private static readonly string[] AlsoGlossaryOnly =
    [
        "carmustine-wafer", "chemoradiation", "h3-g34", "nadir", "neutropenia",
        "pediatric-type", "simulation", "somnolence-syndrome", "subtotal-resection",
        "transformation",
    ];

    // ---------------------------------------------------------------- the measurement

    /// <summary>
    /// Every <c>[definition]</c> finding the real tool produces for the real glossary
    /// directory, keyed by slug. Through <see cref="ContentChecker.CheckAll"/> and not
    /// through a loop over <c>CheckGlossaryTerm</c>, because the per-entry gate's REACH
    /// is part of what is being tested: §12.23's finding from the other end is that a
    /// glob which stops matching switches a per-page gate off wholesale.
    /// </summary>
    private static Dictionary<string, Finding> Definitions()
    {
        var findings = ContentChecker.CheckAll(
            Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages"),
            CuratedPage.GlossaryRoot,
            DateOnly.FromDateTime(DateTime.UtcNow));

        return findings
            .Where(f => f.File.EndsWith(ContentChecker.DefinitionMarker, StringComparison.Ordinal))
            .ToDictionary(
                f => f.File["glossary/".Length..^(".md".Length + ContentChecker.DefinitionMarker.Length)],
                f => f,
                StringComparer.Ordinal);
    }

    private static int Words(string definition) =>
        definition.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>
    /// A definition's length in characters, <b>with line endings normalised first.</b>
    ///
    /// <para>A shipped definition is hard-wrapped, so its character count differs by one
    /// per line between a CRLF checkout and an LF one — and CI is Linux. The first
    /// version of the margin assertion below read <c>Definition.Length</c> straight,
    /// passed here at 108 and failed at 107 on <c>endings.py</c>'s LF run. Word counts
    /// are immune (whitespace is whitespace); character counts are not.</para>
    /// </summary>
    private static int Chars(string definition) =>
        definition.ReplaceLineEndings("\n").Trim().Length;

    // ------------------------------------------------------------------- §12.30, the set

    [Fact]
    public void TheUngradedSetIsExactlyTheFourEntriesUnderTheWordFloor()
    {
        var definitions = Definitions();

        // THE SWEEP SAW THE WHOLE GLOSSARY. Compared with the files on disk, because
        // `CheckAll` has a `Directory.Exists`, an `EnumerateFiles` and an `OrderBy`
        // between the directory and this dictionary — three places for it to quietly
        // walk less than the corpus. The literal is here too because §12.30 and this
        // file's prose both say 105, and a number stated in prose should not be the one
        // number that floats.
        Assert.Equal(
            Directory.GetFiles(CuratedPage.GlossaryRoot, "*.md").Length, definitions.Count);
        Assert.Equal(105, definitions.Count);

        // AND THE CONSTANT THE TOOL CALIBRATES ITS OWN SWEEP AGAINST IS THAT SAME
        // NUMBER — a claim about the CONSTANT, which is a different claim from the one
        // above and not a third copy of it. Lower GlossaryWhenMeasured and the tool
        // starts reporting a short glossary as the measured one; the line above cannot
        // see that, because it never reads the constant.
        Assert.Equal(definitions.Count, ContentChecker.GlossaryWhenMeasured);

        var ungraded = definitions
            .Where(kv => kv.Value.Grade is null)
            .Select(kv => kv.Key)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        // THE SET, NOT THE COUNT (§12.25, and WI-575 round 4). A count of 4 is satisfied
        // by any four slugs: trim a 38-word definition to 19 while `pcv` is grown to 20
        // and the count never moves. THIS is the assertion that closes §12.23's hole.
        Assert.Equal(UnderTheWordFloor, ungraded);

        // AND THE REASON EACH ONE IS IN THAT SET IS ITS LENGTH, re-derived from the
        // shipped file rather than taken from the finding that produced the set.
        foreach (var slug in UnderTheWordFloor)
        {
            var term = CuratedPage.GlossaryTerms.Single(t => t.Slug == slug);
            Assert.True(Words(term.Definition) < ContentChecker.MinimumDefinitionWordsToGrade,
                $"glossary/{slug} is in the ungraded set but holds "
                + $"{Words(term.Definition)} words, which is at or above the "
                + $"{ContentChecker.MinimumDefinitionWordsToGrade}-word floor — so it is "
                + "ungraded for some OTHER reason, and this file's whole account of the "
                + "set is wrong");
        }

        // THE COMPLEMENT IS NOT ASSERTED HERE, and that is deliberate: with the total
        // pinned at 105 and the ungraded set pinned at four named slugs, "101 graded" is
        // true for every possible input and no mutation could red it — §12.28's shape of
        // an assertion implied by another one a few lines above it. It is asserted in
        // EveryGradedDefinitionIsAtOrUnderTheLimitAndNoneOfThemIsMerelyReported, which
        // re-derives the findings and pins neither of the other two numbers.
    }

    // ------------------------------------------------------------------ §12.30, the gate

    [Fact]
    public void EveryGradedDefinitionIsAtOrUnderTheLimitAndNoneOfThemIsMerelyReported()
    {
        var graded = Definitions()
            .Where(kv => kv.Value.Grade is not null)
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(101, graded.Count);

        var over = graded
            .Where(kv => kv.Value.Grade > ContentChecker.FailGrade)
            .Select(kv => $"{kv.Key} {kv.Value.Grade:0.0}")
            .ToList();

        Assert.True(over.Count == 0,
            "a glossary definition reads above the 6.0 limit. Split a sentence before "
            + "simplifying a word: 35 definitions went from 6.1-9.3 to 2.6-5.9 with NOT "
            + "ONE WORD REMOVED (§12.30), and the 40-word ceiling in content-pipeline §6 "
            + "is the only thing that limits how many sentences are available:\n  "
            + string.Join("\n  ", over));

        // AND THE MESSAGE NEVER CONTRADICTS ITS OWN LEVEL, which is the defect §12.22
        // round 4 found between two findings and §12.23 found inside one. There is no
        // ungated definition grade any more, so the string that used to say so is gone.
        Assert.DoesNotContain(graded, kv =>
            kv.Value.Message.Contains("not gated", StringComparison.OrdinalIgnoreCase));

        // THE APPROACH BAND HAS LIVE SUBJECTS, which §12.19's rule requires of any
        // branch: a guard measured where it cannot fire has been measured on nothing.
        // 18 of the 101 sit between 5.5 and 6.0 on the day this lands, and
        // `rescue-medicine` reads exactly 6.0 — the one entry that passes with no margin
        // at all. A FLOOR and not an equality: these 18 are not this item's subjects and
        // every one of them moves if anybody edits it.
        var band = graded.Count(kv =>
            kv.Value.Grade >= ContentChecker.WarnGrade
            && kv.Value.Grade <= ContentChecker.FailGrade);
        Assert.True(band >= 1,
            "no shipped definition is in the 5.5-6.0 approach band, so the Warn branch "
            + "of the gate has no live subject and has been measured on nothing (§12.19)");
        Assert.Equal(band, graded.Count(kv => kv.Value.Level == FindingLevel.Warn));
    }

    /// <summary>
    /// The branch the shipped corpus cannot reach: a definition ABOVE the limit is
    /// FAILED, not reported.
    ///
    /// <para><b>Synthetic on purpose, and §12.23 is why it is named as such.</b> The
    /// break harness reported WI-578's truncation mutation as a survivor because the
    /// mutation edited a shipped page while the test drove a synthetic one — the target
    /// was wrong for the input, so a live gate read as dead. <b>No content mutation can
    /// reach this test</b>; the guard that sees a real over-limit definition on disk is
    /// <see cref="EveryGradedDefinitionIsAtOrUnderTheLimitAndNoneOfThemIsMerelyReported"/>,
    /// and the two are a pair. This one exists because a clean corpus cannot show that
    /// the Fail branch is wired at all, and
    /// <c>ShippedGlossaryTermsPassTheirOwnGate</c>'s <c>DoesNotContain(Fail)</c> is
    /// satisfied just as happily by a gate that never fails anything.</para>
    /// </summary>
    [Fact]
    public void AnOverLimitDefinitionIsFailedRatherThanReported()
    {
        // 29 words, over the word floor, ONE sentence — and deliberately written in
        // ORDINARY WORDS. A fixture full of Latinate medical vocabulary would fail for
        // a reason this item explicitly measured and rejected: `lomustine` read 9.3 on
        // the strength of *occasionally*, and masking every medical term in the glossary
        // took 35 over-limit entries to 29 and no further. This fixture has no medical
        // term in it at all and reads 10.4, which is the thesis stated as a probe.
        const string oneSentence =
            "A scan of the head that is taken before the operation starts, so the team "
            + "can see where the tumor sits and plan the safest way to reach it.";

        // THE CONTROL IS THE SAME TWENTY-NINE WORDS with two boundaries moved — `so the`
        // becomes `. The` and `and plan` becomes `. And they plan`, which is minus one
        // word and plus one. 10.4 → 2.8. Neither vocabulary nor length can be what moved
        // the verdict, which is the only way a probe is evidence about the gate rather
        // than about the fixture (§12.25 shipped this shape for the description band).
        const string split =
            "A scan of the head that is taken before the operation starts. The team can "
            + "see where the tumor sits. And they plan the safest way to reach it.";

        var over = Grade(oneSentence);
        var control = Grade(split);

        Assert.Equal(FindingLevel.Fail, over.Level);
        Assert.True(over.Grade > ContentChecker.FailGrade, $"{over.Grade:0.0}");
        Assert.EndsWith(ContentChecker.DefinitionMarker, over.File, StringComparison.Ordinal);

        // THE CONTROL IS NOT "SOMETHING ELSE THAT PASSES" — the word counts are equal,
        // asserted, and both are over the grading floor so neither side is ungraded for
        // a reason that has nothing to do with the grade.
        Assert.Equal(29, Words(oneSentence));
        Assert.Equal(29, Words(split));
        Assert.True(Words(split) >= ContentChecker.MinimumDefinitionWordsToGrade);

        // Info and not merely "not Fail": 2.8 is below the approach band too, so this
        // also says the Warn branch is not swallowing a passing definition. The band's
        // own live subjects are the 18 in the shipped corpus, above.
        Assert.Equal(FindingLevel.Info, control.Level);
        Assert.True(control.Grade <= ContentChecker.WarnGrade, $"{control.Grade:0.0}");

        static Finding Grade(string definition) =>
            ContentChecker.CheckGlossaryTerm($"---\nterm: x\n---\n{definition}", "x")
                .Single(f => f.File.EndsWith(
                    ContentChecker.DefinitionMarker, StringComparison.Ordinal));
    }

    // ----------------------------------------------------------------- §12.30, the words

    [Fact]
    public void TheRewrittenDefinitionsKeepTheirWords()
    {
        // THE TABLE'S OWN INTEGRITY FIRST. A slug typed twice, or a row out of order
        // hidden in the middle of a long literal, passes every assertion below and makes
        // the table a lie on paper (the shape WI-580's order assertion caught on its
        // first run).
        var slugs = Rewritten.Select(r => r.Slug).ToArray();
        Assert.Equal(35, slugs.Length);
        Assert.Equal(slugs.Length, slugs.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(slugs.OrderBy(s => s, StringComparer.Ordinal), slugs);

        var shipped = CuratedPage.GlossaryTerms.ToDictionary(
            t => t.Slug, t => t.Definition, StringComparer.Ordinal);

        foreach (var (slug, wordsBefore, gradeBefore, floor) in Rewritten)
        {
            Assert.True(shipped.ContainsKey(slug),
                $"{slug} is in this table but is not a shipped glossary entry");

            // THE TABLE IS THE OVER-THE-LIMIT SET, asserted. A row whose before-grade is
            // under 6.0 is a floor nobody argued for, parked in the one table in the
            // repo where a floor looks like it belongs.
            Assert.True(gradeBefore > ContentChecker.FailGrade,
                $"{slug}'s recorded before-grade is {gradeBefore:0.0}, at or under the "
                + $"{ContentChecker.FailGrade:0.0} limit — this table is the set this "
                + "item rewrote BECAUSE it read above the limit");

            // AND THE FLOOR RECORDS THAT NO WORD WAS REMOVED. A claim about two
            // literals, and it fails the moment somebody lowers a floor to make a
            // truncation pass — which is the one edit that would make this whole table
            // decorative.
            Assert.True(floor >= wordsBefore,
                $"{slug}'s floor is {floor} words against {wordsBefore} before this "
                + "item, so the recorded floor itself concedes a truncation");

            var words = Words(shipped[slug]);

            // AND THE FLOOR IS UNDER THE EDITORIAL CEILING, with the sentence the next
            // person needs. The two limits are TANGENT on two of these rows: an entry at
            // 40 words with a floor of 40 is frozen at exactly its length in both
            // directions, and a floor raised to 41 makes this suite UNSATISFIABLE —
            // `words >= floor` demands 41 and `ShippedGlossaryTermsPassTheirOwnGate`
            // fails at 41 — with nothing in either message saying why.
            Assert.True(floor <= 40,
                $"{slug}'s word floor is {floor}, above the 40-word editorial ceiling "
                + "(content-pipeline §6). No definition can satisfy both this floor and "
                + "that ceiling, so the suite is now unsatisfiable rather than failing: "
                + "the definition has to be shortened and the floor lowered together, "
                + "with the reason written down.");

            Assert.True(words >= floor,
                $"glossary/{slug}'s definition is {words} words against a floor of "
                + $"{floor}. A readability fix that REMOVES words is a truncation "
                + "wearing a split's clothes (§12.23): the prose reads better, the grade "
                + "improves, and the definition is that much closer to the "
                + $"{ContentChecker.MinimumDefinitionWordsToGrade}-word floor where "
                + "grading stops. If the words genuinely should go, lower the floor in "
                + "the same commit and say why.");
        }

        // THE TWO COLUMN CHECKSUMS, and they are the reason a single floor cannot be
        // quietly lowered. The per-entry assertions above are each local: drop one row's
        // floor from 38 to 24 to let a truncation through and every one of them still
        // passes. These two do not, and they are the number §12.23 puts beside a gain so
        // that "0 over the limit, down from 35" and "0 over the limit, because they got
        // shorter" cannot read the same.
        //
        // AND `Sum(words) >= Sum(WordsBefore)` IS NOT HERE: it is implied by
        // `words >= floor` and `floor >= wordsBefore`, both asserted per row above, so
        // it is true for every input that reaches it (§12.28).
        Assert.Equal(1099, Rewritten.Sum(r => r.WordsBefore));
        Assert.Equal(1187, Rewritten.Sum(r => r.WordFloor));
    }

    // ------------------------------------------------------- §12.30, the floor's shape

    [Fact]
    public void ThreeOfTheFourUnderTheFloorWouldFailIfTheyGrewIntoTheGradedSet()
    {
        Assert.Equal(UnderTheWordFloor, IfTheyCrossedTheFloor.Select(e => e.Slug).ToArray());

        var wouldFail = new List<string>();
        foreach (var (slug, wouldGrade) in IfTheyCrossedTheFloor)
        {
            var term = CuratedPage.GlossaryTerms.Single(t => t.Slug == slug);

            // THE TOOL'S OWN GRADER, not a second one (WI-416). The naive sweep that
            // reported 38 over the limit and a worst of 16.1 differed from the tool by
            // exactly these entries, and the second grader's cost was a wrong number
            // that nearly reached a doc.
            var measured = ReadabilityAnalyzer.FleschKincaidGrade(term.Definition);
            Assert.Equal(wouldGrade, measured, 1);

            // THE BRANCH READS THE MEASURED VALUE, NOT THE TABLE (/review). Reading
            // `wouldGrade` made the set assertion below an identity over two array
            // literals and a constant, saved only by the equality on the line above it.
            // Off the measurement it is load-bearing on its own.
            if (measured > ContentChecker.FailGrade)
            {
                wouldFail.Add(slug);
            }
        }

        // PINNED AS A SET AND NOT AS "three of four", because which three is the fact:
        // these are the entries for which crossing the word floor is a BUILD FAILURE
        // rather than an escape, and that is what makes the floor an instrument instead
        // of a hiding place.
        Assert.Equal(
            new[] { "memantine", "pcv", "procarbazine" },
            wouldFail.OrderBy(s => s, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void TheTwoFloorsOverlapOnTenEntriesAndNeitherSubsumesTheOther()
    {
        var firing = CuratedPage.GlossarySlugsFiringAnywhere();
        var glossaryOnly = CuratedPage.GlossaryTerms
            .Where(t => !firing.Contains(t.Slug))
            .Select(t => t.Slug)
            .ToHashSet(StringComparer.Ordinal);

        // WI-581's set, re-measured here rather than quoted — §12.26's closing lesson is
        // that a measurement is only true of the tree it was taken on, and this item
        // edited 10 of those 26 definitions.
        Assert.Equal(26, glossaryOnly.Count);

        var overlap = Rewritten
            .Select(r => r.Slug)
            .Where(glossaryOnly.Contains)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(AlsoGlossaryOnly, overlap);

        // NEITHER DIRECTION IS EMPTY, and the claim is a PROPERTY rather than
        // arithmetic. `26 - 10 = 16` and `35 - 10 = 25` are both true of the array
        // literals alone and no input could red either one (§12.28). What is not
        // implied is what the two sets DO:
        //
        //   * the 16 glossary-only entries this item did not touch must still clear
        //     WI-581's character floor — this item did not break the other instrument
        //     on the entries it has no floor for;
        //   * the 25 rewritten entries outside WI-581's set must actually FIRE a
        //     tooltip somewhere, which is what makes them a different POPULATION
        //     rather than a different list.
        var untouchedGlossaryOnly = glossaryOnly
            .Except(Rewritten.Select(r => r.Slug), StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        Assert.NotEmpty(untouchedGlossaryOnly);
        foreach (var slug in untouchedGlossaryOnly)
        {
            var untouched = CuratedPage.GlossaryTerms.Single(t => t.Slug == slug);
            Assert.True(untouched.Definition.Trim().Length > untouched.Term.Length + 20,
                $"glossary/{slug} is glossary-only, carries no word floor from WI-582, "
                + "and no longer clears WI-581's character floor either");
        }

        // DERIVED FROM THE PINNED LITERAL, NOT FROM THE LIVE COMPLEMENT, and /review
        // caught the first version doing the latter. `glossaryOnly` IS the complement of
        // `firing`, so `Where(s => !glossaryOnly.Contains(s))` then
        // `Assert.Contains(slug, firing)` is De Morgan over one set: an entry that STOPS
        // firing joins `glossaryOnly`, is filtered out of the list, and the assertion
        // goes vacuous instead of red. Nothing anybody could edit — a definition, a
        // page, a block, the tooltip renderer — could have failed it. Excepting
        // `AlsoGlossaryOnly` instead fixes the population to the 25 that fired when this
        // was measured, so a rewrite that kills a tooltip reds here.
        var rewrittenAndFiring = Rewritten
            .Select(r => r.Slug)
            .Except(AlsoGlossaryOnly, StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        Assert.Equal(25, rewrittenAndFiring.Count);
        Assert.All(rewrittenAndFiring, slug => Assert.Contains(slug, firing));

        // AND ON THE OVERLAP THE CHARACTER FLOOR IS SLACK BY TWO ORDERS OF MAGNITUDE,
        // MEASURED. WI-581 floors a glossary-only definition at the term's length + 20
        // CHARACTERS, to rule out "The drug procarbazine."; the tightest margin among
        // these 10 is `subtotal-resection` at 107 characters — 145 against a floor of 38.
        //
        // AND THE MEASUREMENT NORMALISES LINE ENDINGS, which is not housekeeping: the
        // first version read `Definition.Length` straight and asserted 108, because a
        // two-line definition in this CRLF checkout carries one more character than the
        // same file does on LF. It PASSED here and FAILED on the LF run of endings.py —
        // i.e. it would have failed in CI and passed on the machine that wrote it. A
        // character count over a wrapped definition is an ending-dependent number.
        // So on these 10 the character floor could not catch a truncation the word
        // floor misses, which is why the word floor was worth writing; and the 16 above
        // are why the character floor is still worth keeping. Neither subsumes the other
        // and neither is deleted (§12.28: a supersession is a merge whose diff is the
        // assertion list, and this is not one).
        foreach (var slug in overlap)
        {
            var both = CuratedPage.GlossaryTerms.Single(t => t.Slug == slug);
            var floor = Rewritten.Single(r => r.Slug == slug).WordFloor;

            Assert.True(both.Definition.Trim().Length > both.Term.Length + 20,
                $"glossary/{slug} no longer clears WI-581's character floor");
            Assert.True(floor >= 26,
                $"glossary/{slug}'s word floor is {floor}, low enough that WI-581's "
                + "character floor could become the binding one — at which point these "
                + "are one instrument and this test is the place that should say so");
        }

        Assert.Equal(107, overlap
            .Select(slug => CuratedPage.GlossaryTerms.Single(t => t.Slug == slug))
            .Min(term => Chars(term.Definition) - (term.Term.Length + 20)));
    }

    // ------------------------------------------------- §12.30, the report's call site

    /// <summary>
    /// <b>The corpus report runs, and it NAMES the four entries it cannot grade.</b>
    ///
    /// <para>§12.23's /review round 3: <b>a gate that is silent when it passes cannot be
    /// proved to be plugged in.</b> Every other test in this file reads the per-entry
    /// findings, which <see cref="ContentChecker.CheckGlossaryTerm"/> produces whether
    /// or not <see cref="ContentChecker.GlossaryCorpusReport"/> is called at all —
    /// delete the call site and all six stay green. This is the assertion the call site
    /// buys.</para>
    ///
    /// <para>And it reads the NAMES rather than the count, because that is the half the
    /// report exists for (§12.19/§12.24/§12.25): <i>"4 under the floor"</i> is satisfied
    /// by any four entries, and a reader of the log is the one person positioned to
    /// notice that one of them is not the one that was there last week.</para>
    /// </summary>
    [Fact]
    public void TheGlossaryCorpusReportRunsAndNamesTheEntriesItCannotGrade()
    {
        var findings = ContentChecker.CheckAll(
            Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages"),
            CuratedPage.GlossaryRoot,
            DateOnly.FromDateTime(DateTime.UtcNow));

        var report = Assert.Single(findings.Where(f => f.File == "(glossary)"));

        // ONE FINDING AND AT Info: the report's other four branches are a short-corpus
        // Warn, a dead-instrument Fail, a partition Fail and a wiring Fail, and any of
        // them firing means something this file claims is not true. Asserting the LEVEL
        // refuses all four at once, without this test having to know which.
        Assert.Equal(FindingLevel.Info, report.Level);

        foreach (var slug in UnderTheWordFloor)
        {
            Assert.Contains($"glossary/{slug}.md", report.Message, StringComparison.Ordinal);
        }

        // The totals, built from the constants rather than transcribed, so rewording the
        // message does not red this and renumbering it does.
        Assert.Contains($"0 above the {ContentChecker.FailGrade:0.0} limit",
            report.Message, StringComparison.Ordinal);
        Assert.Contains(
            $"{UnderTheWordFloor.Length} under the "
            + $"{ContentChecker.MinimumDefinitionWordsToGrade}-word floor",
            report.Message, StringComparison.Ordinal);
    }
}
