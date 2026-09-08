using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-515: <c>/tumors/high-grade-glioma</c> — the counterpart to
/// <c>/tumors/low-grade-glioma</c>. Same job (a GROUPING, not a diagnosis),
/// opposite emotional weight, so §12.6's "never end a section on a frightening
/// sentence" carries more here than on any hub written so far.
///
/// Seventeen sections per §12.3 (NOT §12.8's twelve slots — §12.9's first
/// warning). Written FROM §12.9 and §12.10 rather than proving them.
///
/// The four claims the backlog names, and each has a test below:
///   1. grades 3 and 4 are a grouping that never appears on a report alone;
///   2. two tumors can both be grade 4 and be very different;
///   3. there is no grade 4 oligodendroglioma;
///   4. a grade 4 can be there from the start or arrive by transformation.
/// </summary>
public sealed class HighGradeGliomaPageContentTests
{
    private static string Page => CuratedPage.Read("tumors", "high-grade-glioma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, "tumors/high-grade-glioma");

    private const string WhatHeading = "What is a high-grade glioma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string OutlookHeading = "What might happen over time";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string RecurrenceHeading = "If it comes back, or changes";

    [Fact]
    public void TheOpeningSaysHighGradeGliomaIsAGroupRatherThanADiagnosis()
    {
        // Backlog claim 1, and the organising fact of the page. Research §3.1:
        // "high-grade glioma" is a grouping and "does not appear on a WHO CNS5
        // report as a standalone label". A reader who thinks it is their
        // diagnosis will go looking for research about the wrong thing.
        var shortVersion = CuratedPage.Flatten(Section("The short version"));

        Assert.Matches(new Regex(@"a group, not a diagnosis", RegexOptions.IgnoreCase), shortVersion);
        Assert.Matches(new Regex(@"grade 3 or grade 4", RegexOptions.IgnoreCase), shortVersion);

        // And it must hand the reader the place the real name lives, rather
        // than leaving them to guess.
        Assert.Matches(new Regex(@"pathology report", RegexOptions.IgnoreCase), shortVersion);
    }

    [Fact]
    public void ThePageSaysTheLabelWillNotAppearOnAReportByItself()
    {
        // The practical half of claim 1. Saying "it is a group" is abstract;
        // saying "you will not find these words on your report" is the thing
        // that sends somebody to look at the document.
        var section = CuratedPage.Flatten(Section(WhatHeading));

        Assert.Matches(
            new Regex(@"not the name of a tumor|will not appear on your report", RegexOptions.IgnoreCase),
            section);

        // And the question that gets them the answer.
        Assert.Matches(
            new Regex(@"full name of my tumor, not just its grade", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheGroupingRoutesToEveryDiagnosisItNamesUnderTheLabel()
    {
        // Same router duty the umbrella has, for the subset that is high grade.
        // Naming a diagnosis without a door is this page's most likely defect.
        var section = Section(WhatHeading);

        foreach (var slug in new[]
                 {
                     "/tumors/astrocytoma",
                     "/tumors/oligodendroglioma",
                     "/tumors/glioblastoma",
                     "/tumors/diffuse-midline-glioma",
                     "/tumors/glioma",
                 })
        {
            Assert.Contains(slug, section, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ThePageWarnsThatTheChildPagesItRoutesToAreStillStubs()
    {
        // WI-514's blocker, generalised and turned into a standing check. That
        // page routed the cancer question to five Wave-0 stubs while insisting
        // the forward reference "is not a dodge". A forward reference is only
        // an answer if the destination answers.
        //
        // This test does not require the stubs to be filled in — WI-516, WI-517
        // and WI-518 own that. It requires the page to be HONEST about it while
        // they are thin, and it reads the destinations rather than trusting a
        // literal, so the warning must come DOWN when they are written.
        var section = CuratedPage.Flatten(Section(WhatHeading));

        // The destinations are DERIVED from the section's own /tumors/ links,
        // not hard-coded. A hard-coded list of three missed
        // /tumors/diffuse-midline-glioma, which is also a stub — so when
        // WI-516, WI-517 and WI-518 land, the else-branch below would have
        // demanded the honesty note come down while it was still true of the
        // fourth destination.
        var destinations = Regex.Matches(Section(WhatHeading), @"\(/tumors/([a-z0-9-]+)\)")
            .Select(m => m.Groups[1].Value)
            .Where(slug => slug != "glioma")
            .Distinct()
            .ToList();

        Assert.True(destinations.Count >= 4,
            "this section routes to fewer than four diagnosis pages, so it is no longer the "
            + "router this test was written for");

        var thin = destinations.Count(slug =>
            CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", slug + ".md")))
                .Length < 4000);

        // BOTH halves, not an alternation. The break harness deleted the claim
        // ("those child pages are short at the moment") and this stayed green
        // off the redirect half alone — an alternation where either branch
        // passes is not one assertion, it is two weaker ones.
        var claim = new Regex(@"child pages are short at the moment", RegexOptions.IgnoreCase);
        var redirect = new Regex(@"page you land on is thin", RegexOptions.IgnoreCase);

        if (thin > 0)
        {
            Assert.Matches(claim, section);
            Assert.Matches(redirect, section);
        }
        else
        {
            Assert.DoesNotMatch(claim, section);
            Assert.DoesNotMatch(redirect, section);
        }
    }

    [Fact]
    public void TheCancerQuestionIsAnsweredInTheFirstSentenceAndNotDeferred()
    {
        // §12.6: answer in the first sentence under the heading. On this page
        // the answer is unambiguous — every high-grade glioma is malignant — so
        // there is no excuse for the "it depends which one" opening the
        // umbrella needed and got wrong first time.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        // The FIRST sentence, not "somewhere in the first two". The break
        // harness rewrote the opening as "A grade describes how a tumor
        // behaves. The word your team will use for this one is malignant, and
        // yes, it is cancer." — which defines a grade first and buries the
        // answer in a subordinate clause, and the two-sentence version passed
        // it. Presence was never the property; POSITION was (WI-512).
        var first = CuratedPage.SentencesOf(section).First();

        Assert.Matches(new Regex(@"^\s*Yes\b", RegexOptions.IgnoreCase), first);
        Assert.Matches(new Regex(@"\bmalignant\b", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void CurabilityIsStatedAtTheSameStrengthAsTheUmbrellaAndTheSiblingPage()
    {
        // The WI-514 defect that mattered most: the same fact rendered at two
        // strengths on two pages, with the softer version on the page more
        // people land on first. This page is the one a grade 3 or 4 reader
        // reaches, so it may not be the soft one either.
        //
        // Compared against BOTH siblings by READING THEM (§12.10), so the three
        // cannot drift apart without something going red.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        foreach (var sibling in new[] { "glioma.md", "low-grade-glioma.md" })
        {
            var text = CuratedPage.Flatten(
                CuratedPage.ReaderText(CuratedPage.Read("tumors", sibling)));
            Assert.True(
                Regex.IsMatch(text, @"they are not curable", RegexOptions.IgnoreCase),
                $"/tumors/{sibling} no longer says 'they are not curable', so the strength this "
                + "page was matched to has moved and all three need rechecking");
        }

        // The CLAIM, not the phrase (§12.10's second test trap): asserting that
        // "not curable" appears somewhere passes on a page whose only use of
        // the words is the softening line beneath it. Proven by deleting the
        // hard sentence and watching this fail.
        Assert.Matches(new Regex(@"they are not curable", RegexOptions.IgnoreCase), section);

        // AND IT MUST BE SCOPED. This is §12.10's own defect, and the first
        // draft re-committed it on the page least able to afford it: it said
        // "these tumors grow into the brain around them ... they are not
        // curable" of EVERY high-grade glioma. CNS5 defines a whole
        // supercategory of circumscribed astrocytic gliomas with "more
        // well-delineated borders separating them from the surrounding brain
        // parenchyma", and two of them are grade 3 — pleomorphic
        // xanthoastrocytoma and high-grade astrocytoma with piloid features.
        // Both siblings scope it (low-grade-glioma to "grade 2 diffuse
        // gliomas", glioma.md with an explicit carve-out); the page whose
        // thesis is "two tumors here can be very different" may not be the one
        // that does not.
        Assert.Matches(
            new Regex(@"The three named above grow into the brain", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"smaller number of high-grade gliomas are not like that",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"\bcircumscribed\b", RegexOptions.IgnoreCase), section);

        // Both halves. Without the second, the first reads as a timeline.
        Assert.Matches(new Regex(@"not the same as untreatable", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not a timeline", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TwoTumorsCanBothBeGradeFourAndBeDifferentDiseases()
    {
        // Backlog claim 2, and research §3.1 calls it "the critical framing
        // point for this page". CNS5's own words (PMC9723092): "even when
        // enough high-grade features are present to warrant a grade 4
        // designation, IDH-mutant astrocytomas are no longer referred to as
        // glioblastomas, since even high-grade IDH-mutant astrocytomas are less
        // aggressive than their IDH-wildtype counterparts".
        //
        // A reader who does not know this reads every "grade 4" story on the
        // internet as their own.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        // Grading happens WITHIN a tumor type, so the same number means
        // different things across diagnoses.
        Assert.Matches(
            new Regex(@"inside each tumor type|within each tumor type", RegexOptions.IgnoreCase),
            section);

        // And the specific, load-bearing example rather than the abstraction
        // alone: an IDH-mutant grade 4 is still not a glioblastoma.
        Assert.Matches(
            new Regex(@"IDH gene change can be grade 4 and is still not called a glioblastoma",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"[Ss]ame number, different disease", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageSaysThereIsNoGradeFourOligodendroglioma()
    {
        // Backlog claim 3, verbatim in the CCO clinical practice review: "There
        // is no oligodendroglioma tumor corresponding to CNS WHO grade 4."
        //
        // It is also the one genuinely reassuring fact on this page, and the
        // reason it is worth stating is the inference readers make without it:
        // that grade 3 is one rung below something worse. Asserted WITH that
        // reasoning attached, because the bare sentence reads as trivia.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(
            new Regex(@"no grade 4 oligodendroglioma", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"grade 3 is the top of its scale", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"no worse version", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageExplainsAGradeFourSetByGeneResultsRatherThanAppearance()
    {
        // The other half of CNS5's grading change, and the one that arrives
        // with no warning: a tumor that looked milder is called grade 4 on
        // molecular grounds. Without it, a reader reads their own report as a
        // contradiction. CDKN2A/B is the named mechanism (CCO).
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(new Regex(@"CDKN2A/B", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"set the grade on its own|whatever the cells look like", RegexOptions.IgnoreCase),
            section);

        // §12.6: never end a frightening block without a landing. The landing
        // here is that nobody was hiding anything and where to go next.
        Assert.Matches(
            new Regex(@"not a mistake and nobody was hiding anything", RegexOptions.IgnoreCase),
            section);
        Assert.Contains("/tests/molecular-markers", Section(GradeHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRetiredNameSliceIsAboutTheWordThisPagesReadersAreHolding()
    {
        // §12.10: the shared block carries what is universal, the page carries
        // its own slice. The umbrella's slice is the whole glioma family. THIS
        // page's slice is the one word its readers are actually holding —
        // "anaplastic", which before 2021 was how a report said grade 3 (CCO:
        // "eliminating the term 'anaplastic' to indicate a WHO grade 3
        // astrocytoma or oligodendroglioma"; CNS5: "the term 'anaplasia' is no
        // longer employed, instead only 'WHO grade 3' is used").
        //
        // Asserted on the COMPOSED page so a slice that silently moved into the
        // shared block would still be found — but the ANGLE is asserted too,
        // because a slice that is just a copy of the umbrella's list has not
        // done this page's job.
        var section = CuratedPage.Flatten(CuratedPage.ComposedSection(Page, ReportHeading));

        Assert.Matches(
            new Regex(@"anaplastic[^.]{0,80}grade 3|grade 3[^.]{0,80}anaplastic", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"grade number does that job now", RegexOptions.IgnoreCase), section);

        // The MAPPING, not the mention. Deleting the crosswalk row for
        // anaplastic oligodendroglioma left this green, because the
        // explanatory paragraph above still uses the phrase — so a reader
        // holding that word would have been told it is retired and never told
        // what it is called now, which is the whole job of a crosswalk.
        foreach (var (retired, replacement) in new[]
                 {
                     ("Anaplastic astrocytoma", "astrocytoma, IDH-mutant, grade 3"),
                     ("Anaplastic oligodendroglioma", "oligodendroglioma, IDH-mutant and"),
                     ("Glioblastoma multiforme", "now just glioblastoma"),
                     ("Secondary glioblastoma", "astrocytoma, IDH-mutant, grade 4"),
                 })
        {
            // Emphasis stripped rather than required — matching a literal "**"
            // tied a content assertion to markdown styling. But stripping it
            // means the first hit is the explanatory paragraph ("so an
            // anaplastic astrocytoma was a grade 3 astrocytoma"), not the
            // crosswalk row, so this checks EVERY occurrence and requires one
            // of them to carry the new name. Robust to both the styling and
            // the order the two appear in.
            // Zero-width LOOKAHEAD for the window, not a consuming group.
            // Regex.Matches returns non-overlapping matches, so a consuming
            // ".{0,200}" on the first occurrence (the explanatory paragraph)
            // swallowed the second (the crosswalk row) and the row was never
            // examined at all — a loop over "every occurrence" that silently
            // only ever saw one.
            var plain = Regex.Replace(section, @"[*_]", "");
            var occurrences = Regex.Matches(plain, Regex.Escape(retired) + @"(?=(.{0,200}))",
                RegexOptions.IgnoreCase);

            Assert.True(occurrences.Count > 0,
                $"'{retired}' is not named anywhere on this page, so a reader holding that word "
                + "is told nothing");
            Assert.True(
                occurrences.Any(o => o.Groups[1].Value.Contains(
                    replacement, StringComparison.OrdinalIgnoreCase)),
                $"'{retired}' is named but never paired with '{replacement}', so a reader holding "
                + "that word is told it is retired and not told what it is called now");
        }
    }

    [Fact]
    public void EveryRetiredNameIsNamedAsRetiredRatherThanUsedAsALiveDiagnosis()
    {
        // §12.9's warning. A ban list on these words would forbid the crosswalk
        // — the highest-value block on the site — with a comment claiming it
        // enforced CNS5. There are two different things a retired name can be
        // doing: used as a live diagnosis (forbidden) or NAMED as retired
        // (required, and this page owes the "anaplastic" one).
        //
        // Chunk-scoped, not window-scoped: WI-514 proved a +/-220 character
        // window reads "no longer" out of the ADJACENT crosswalk block, so a
        // retired name used live beside it passes.
        foreach (var retired in new[] { "anaplastic", "multiforme", "oligoastrocytoma", "mixed glioma" })
        {
            foreach (var chunk in ChunksContaining(Composed, retired))
            {
                Assert.True(
                    Regex.IsMatch(chunk,
                        @"retired|no longer|old rules|older report|older paperwork|used to be|"
                        + @"changed in 2021|before 2021|before that|still hear|still say|dropped|"
                        + @"is now|are now|was written",
                        RegexOptions.IgnoreCase),
                    $"'{retired}' is used as a live diagnosis rather than named as a retired term, "
                    + $"in: {chunk}");
            }
        }
    }

    /// <summary>
    /// The single bullet that NAMES <paramref name="diagnosis"/>, flattened.
    ///
    /// Fails loudly if no bullet names it, rather than returning empty — an
    /// empty haystack turns every <c>DoesNotMatch</c> below into a free pass,
    /// which is the WI-506 link-canary failure mode.
    /// </summary>
    private static string BulletNaming(string diagnosis)
    {
        // RawSection, not CuratedPage.Section: that helper flattens before it
        // returns, so a splitter fed its output finds no newlines, yields ONE
        // chunk, and every per-bullet assertion below silently becomes a
        // whole-section assertion that passes with all three regimens
        // collapsed onto one tumor. Same shape as WI-507's Split("\n\n") on
        // CRLF — a splitter handed text with nothing to split on does not
        // fail, it just stops being a splitter.
        var bullet = ChunksContaining(RawSection(TreatmentHeading), diagnosis)
            .FirstOrDefault(c => c.TrimStart().StartsWith("-", StringComparison.Ordinal));

        Assert.True(bullet is not null,
            $"no bullet in the treatment section names '{diagnosis}', so the three regimens "
            + "cannot be told apart and this test would prove nothing");

        return bullet!;
    }

    /// <summary>
    /// One "## " section with its line breaks intact, for the callers that need
    /// to split it into paragraphs or list items.
    /// </summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(CuratedPage.Body(Page),
            $@"^## {Regex.Escape(heading)}\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);

        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    /// <summary>
    /// One "### " subsection, line breaks intact. Needed where a rule is right
    /// for one subsection and wrong for the section around it.
    /// </summary>
    private static string RawSubsection(string heading)
    {
        var match = Regex.Match(CuratedPage.Body(Page),
            $@"^### {Regex.Escape(heading)}\s*$(.*?)(?=^#{{2,3}} |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);

        Assert.True(match.Success, $"the page has no '### {heading}' subsection");
        Assert.False(string.IsNullOrWhiteSpace(match.Groups[1].Value),
            $"'### {heading}' is empty, so anything asserted about it proves nothing");
        return match.Groups[1].Value;
    }

    /// <summary>
    /// The list items and paragraphs of a page that mention <paramref name="term"/>.
    /// The unit a claim is actually made in — see the umbrella's copy for why a
    /// character window is the wrong unit.
    /// </summary>
    private static IEnumerable<string> ChunksContaining(string markdown, string term) =>
        Regex.Split(markdown.Replace("\r\n", "\n", StringComparison.Ordinal), @"\n\s*\n|\n(?=\s*[-*] )")
            .Select(CuratedPage.Flatten)
            .Where(chunk => chunk.Contains(term, StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void RomanNumeralGradesNeverAppearAsLiveGrades()
    {
        // The site-wide guard, asserted here because this page's own source for
        // the Stupp claim (StatPearls NBK441874) writes "WHO grade III and IV"
        // — so the notation is one careless paraphrase away at all times.
        //
        // Unlike the umbrella, this page is NOT expected to print a Roman
        // numeral: its slice is about the word "anaplastic", not the notation,
        // and the shared block already teaches the numerals. So this asserts
        // absence outright rather than negation-awareness, and the canary below
        // proves the block still carries the notation somewhere.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Body(Page)));

        // IgnoreCase added by WI-516, which planted "**Grade III.**" and
        // watched this stay green. §12.9's IgnoreCase trap, fourth occurrence.
        Assert.DoesNotMatch(
            new Regex(@"grade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase), body);

        // The shared block IS supposed to print one, for the reader holding it.
        // If it stops, the site has quietly lost the translation and this
        // page's silence stops being a deliberate division of labour.
        var crosswalk = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md"));
        Assert.Matches(
            new Regex(@"grade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase), crosswalk);
    }

    [Fact]
    public void TheSliceDoesNotRepeatWhatTheSharedCrosswalkBlockAlreadySays()
    {
        // §12.10's division of labour, enforced. The block carries the
        // universal renames (the 2021 rewrite, Roman to Arabic, NOS/NEC); the
        // page carries only its own slice. WI-514 had to reconcile
        // /tumors/low-grade-glioma for exactly this reason — its Roman-numeral
        // line was about to be said twice on the same screen.
        //
        // Asserted against the BLOCK's own words rather than a literal list, so
        // that moving a sentence into the block makes this fail rather than
        // silently creating a duplicate.
        var block = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md")));
        var ownProse = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Body(Page)));

        foreach (var universal in new[]
                 {
                     "ordinary numbers instead",
                     "Gene results are now part of the name",
                     "NOS means",
                     "NEC means",
                 })
        {
            Assert.True(
                block.Contains(universal, StringComparison.OrdinalIgnoreCase),
                $"the crosswalk block no longer says '{universal}', so this page's silence about "
                + "it is no longer a division of labour and needs rechecking");
            Assert.False(
                ownProse.Contains(universal, StringComparison.OrdinalIgnoreCase),
                $"'{universal}' is said both in the shared block and in this page's own prose, so "
                + "the reader meets it twice on one screen");
        }
    }

    [Fact]
    public void TheTreatmentSectionExplainsWhySameGradeCanMeanDifferentChemotherapy()
    {
        // Research §3.7 question 4, and the practical proof of claim 2. All
        // three regimens from EANO (PMC7904519), with CATNON (PMC13186461)
        // behind the middle one: for grade 3 IDH-mutant astrocytoma, adjuvant
        // temozolomide benefits and concurrent adds nothing — "OS favored the
        // omission of concurrent temozolomide".
        //
        // The three have to be DISTINGUISHABLE, not merely listed. A section
        // that names three diagnoses and gives them one description has not
        // answered the question it asks in its own heading.
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(
            new Regex(@"[Ww]hy is my friend on different chemotherapy", RegexOptions.IgnoreCase),
            section);

        // Scoped to the BULLET each diagnosis owns, not to the section. A
        // section-wide match would pass with all three descriptions collapsed
        // onto one tumor, which is the failure this test exists to catch —
        // and a character window cannot span the bullet anyway, because the
        // diagnosis and its regimen are separate sentences.
        var glioblastoma = BulletNaming("Glioblastoma, IDH-wildtype, grade 4");
        var astrocytoma = BulletNaming("Astrocytoma, IDH-mutant, grade 3");
        var oligodendroglioma = BulletNaming("Oligodendroglioma, grade 3");

        // Glioblastoma: the two given together.
        Assert.Matches(new Regex(@"at the same time", RegexOptions.IgnoreCase), glioblastoma);

        // Grade 3 IDH-mutant astrocytoma: radiation first, and the negative
        // CATNON result stated rather than quietly skipped. Without the second
        // sentence a reader assumes their team simply chose differently.
        Assert.Matches(new Regex(@"Radiation \*\*first\*\*", RegexOptions.IgnoreCase), astrocytoma);
        // "usually left out", not "not done". CATNON says concurrent
        // temozolomide "is of no benefit" and EANO says its role "remains
        // uncertain" — neither states a practice fact, and a reader whose team
        // IS giving it would have read the stronger wording as being told
        // their treatment is wrong.
        Assert.Matches(
            new Regex(@"did not help, so it is usually left out", RegexOptions.IgnoreCase),
            astrocytoma);
        Assert.DoesNotMatch(new Regex(@"at the same time", RegexOptions.IgnoreCase), astrocytoma);

        // Oligodendroglioma: a different drug combination entirely.
        Assert.Matches(new Regex(@"\bPCV\b"), oligodendroglioma);

        // And the reassurance that follows from it, which is the point of the
        // section: a different plan is not a worse plan.
        Assert.Matches(
            new Regex(@"neither of them is being short-changed", RegexOptions.IgnoreCase), section);

        // No doses anywhere (R1: all Gy and mg figures stay out).
        Assert.DoesNotMatch(new Regex(@"\b\d+\s*(Gy|mg)\b", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void PseudoprogressionIsExplainedWithItsTimingAndItsSymptoms()
    {
        // Research §3.5 calls this "one of the most distressing and
        // least-explained experiences in the whole disease", and it is the
        // thing this page offers that a reader cannot get elsewhere.
        //
        // PMC10412732's own words: pseudoprogression is "changes concerning for
        // tumor progression that are, in fact, transient and related to
        // treatment response"; the findings are evident within the first three
        // months; the criteria "restrict TP to new contrast enhancement outside
        // of the radiation field within 12 weeks of completing radiotherapy".
        var section = CuratedPage.Flatten(Section(ScansHeading));

        Assert.Matches(new Regex(@"pseudoprogression", RegexOptions.IgnoreCase), section);

        // The claim, in the direction that matters: worse-looking scan, working
        // treatment. Not merely the word being present.
        Assert.Matches(
            new Regex(@"does not\s+always mean the tumor has grown", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"treatment working, not by the tumor winning", RegexOptions.IgnoreCase), section);

        // Timing, both figures, because a reader without them cannot tell
        // whether their own scan is inside the window.
        Assert.Matches(new Regex(@"first three months", RegexOptions.IgnoreCase), section);

        // The RULE, not the number. The page says "twelve weeks" twice — once
        // for the rule and once for the wait — so deleting the rule sentence
        // left a bare "twelve weeks" assertion green while the reader lost the
        // only sentence that explains why nobody is acting yet.
        Assert.Matches(
            new Regex(@"For the first twelve weeks after radiation", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"outside where the radiation was aimed", RegexOptions.IgnoreCase), section);

        // The part people are least prepared for, and the reason a purely
        // radiological explanation is not enough: it can be symptomatic.
        Assert.Matches(
            new Regex(@"feel worse too, not just look worse", RegexOptions.IgnoreCase), section);

        // §12.6: the frightening block lands on something actionable.
        Assert.Matches(
            new Regex(@"Ask what your team thinks they are looking at", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void RadiationNecrosisIsSeparatedFromPseudoprogressionByItsTiming()
    {
        // Two different look-alikes with two different timelines, and a reader
        // told about one and not the other will apply the wrong explanation to
        // the wrong scan. ACS at patient level: necrosis "may happen months or
        // even years after treatment", swelling "often controlled with
        // corticosteroid drugs, but sometimes surgery is needed".
        var section = CuratedPage.Flatten(Section(ScansHeading));

        Assert.Matches(new Regex(@"radiation necrosis", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Mm]onths, or even years", RegexOptions.IgnoreCase), section);

        // Position, not just presence: the late one must come AFTER the early
        // one, or the page teaches the timeline backwards.
        var early = section.IndexOf("pseudoprogression", StringComparison.OrdinalIgnoreCase);
        var late = section.IndexOf("radiation necrosis", StringComparison.OrdinalIgnoreCase);
        Assert.True(early > 0 && late > early,
            "radiation necrosis is introduced before pseudoprogression, which teaches the two "
            + "look-alikes in the wrong time order");

        // And it is not left as a bare fright: it is rare and treatable.
        // "Rarely" matches ACS ("Rarely after radiation therapy...") and
        // /treatments/radiation-therapy, which was already saying "rarely"
        // while this page said "uncommon" — the same fact at two strengths on
        // two pages, which is this project's worst repeat defect.
        Assert.Matches(new Regex(@"happens rarely", RegexOptions.IgnoreCase), section);

        var radiation = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("treatments", "radiation-therapy.md")));
        Assert.True(
            Regex.IsMatch(radiation, @"rarely", RegexOptions.IgnoreCase),
            "/treatments/radiation-therapy no longer calls radiation necrosis rare, so the "
            + "strength this page was matched to has moved");
    }

    [Fact]
    public void TransformationIsNamedAndPointsBackAtTheSiblingPage()
    {
        // Backlog claim 4's second route. A reader who arrived here from a
        // grade 2 diagnosis is a different reader with a different history, and
        // PMC10475770 is explicit that progression can go to grade 3 "or even
        // directly to grade 4".
        //
        // The sibling is READ, not assumed (§12.10): if the low-grade page
        // stops covering transformation, this link stops being a promise the
        // site can keep.
        var section = CuratedPage.Flatten(Section(RecurrenceHeading));

        Assert.Matches(new Regex(@"\btransformation\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"grade 3 or straight to grade 4", RegexOptions.IgnoreCase), section);
        Assert.Contains("/tumors/low-grade-glioma", Section(RecurrenceHeading), StringComparison.Ordinal);

        var sibling = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "low-grade-glioma.md")));
        Assert.True(
            Regex.IsMatch(sibling, @"transformation", RegexOptions.IgnoreCase),
            "/tumors/low-grade-glioma no longer covers transformation, so this page sends a "
            + "reader there for an explanation it no longer gives");
    }

    [Fact]
    public void RecurrenceIsFramedAsExpectedWithoutPublishingTheFigure()
    {
        // R3's exact ruling: no percentage for the glioblastoma relapse rate,
        // because "recurrence is expected and is planned for" carries the
        // useful part. Research §3.5 offers 90% and asks whether it belongs on
        // a patient page at all. It does not.
        //
        // The "recurrence is usually local" detail is deliberately absent too:
        // its only source (PMC3643853) is a 21-patient single-centre
        // retrospective, which is the WI-510 conference-abstract call again.
        var section = CuratedPage.Flatten(Section(RecurrenceHeading));

        Assert.Matches(
            new Regex(@"plan assumes the tumor will need attention again", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"not pessimism|not a secret being kept from you", RegexOptions.IgnoreCase),
            section);

        // The frightening section lands on options rather than on the fear.
        Assert.Matches(
            new Regex(@"not the same as being told there is nothing left to try", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void ThePageSaysWhichOfTheTwoRoutesToGradeFourIsWhich()
    {
        // Backlog claim 4, and the half the first draft missed: it covered
        // transformation (the second route) and never contrasted it with the
        // first, so the reader asking "did I have this for years and nobody
        // caught it?" got no answer.
        //
        // Published at the strength the open sources support. CCO: astrocytoma,
        // IDH-mutant "includes CNS WHO grade 2, 3 and 4 tumors and eliminates
        // the prior terminology of ... glioblastoma, IDH-mutant" — so the
        // transformed tumor KEEPS the astrocytoma name. EANO makes "without a
        // pre-existing lower grade glioma" part of the glioblastoma criteria.
        // NO frequency claim: nothing in the fetched set says how many
        // glioblastomas arise de novo, and the dossier's sources for it are a
        // modelling paper and a genomics paper that do not say it either.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(
            new Regex(@"began at grade 2 and changed over time keeps the name\s+astrocytoma",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"does not become a glioblastoma", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"arrived there by\s+completely different routes", RegexOptions.IgnoreCase),
            section);

        // The glioblastoma side, stated as a diagnostic check rather than a
        // frequency.
        Assert.Matches(
            new Regex(@"whether there was a\s+lower-grade glioma there beforehand",
                RegexOptions.IgnoreCase),
            section);
        // NO frequency claim of any wording. Banning the two phrasings I
        // happened to think of ("de novo", "most glioblastomas") let a
        // planted "it is what most grade 4 tumors are" straight through, so
        // the ban is on the QUANTIFIERS and it is scoped to this subsection —
        // the grade section as a whole legitimately says "most people brace
        // for a grade 4", and a rule that fails correct prose is worse than
        // no rule (§12.8).
        var subsection = CuratedPage.Flatten(
            RawSubsection("Did this start as something smaller?"));

        Assert.DoesNotMatch(
            new Regex(@"\b(most|usually|commonly|majority|rarely|typically|de novo)\b",
                RegexOptions.IgnoreCase),
            subsection);

        // §12.6: the section lands on something the reader can do.
        Assert.Matches(
            new Regex(@"fair\s+question and not a rude one", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheSlugMatchesThePathThePageIsActuallyServedFrom()
    {
        // Contract item 10, asserted where it can actually fail. Routing is by
        // FILE PATH — ContentStore maps /tumors/high-grade-glioma to
        // {root}/tumors/high-grade-glioma.md — so `slug:` routes nothing, and
        // ThePageIsServedAtItsExistingUrl cannot be broken by any in-file
        // mutation. It is proven by the file being where it is.
        //
        // What `slug:` DOES feed is the page's own metadata, and a slug that
        // disagrees with the path is a silent defect no request can surface.
        // This is the assertion the break harness can reach.
        // Named Slug, not Path: `const string Path` shadows System.IO.Path
        // inside the method and forces every use below to be fully qualified.
        const string Slug = "tumors/high-grade-glioma";

        // WHOLE LINE, not Contains. "slug: tumors/high-grade-glioma" is a
        // SUBSTRING of "slug: tumors/high-grade-gliomas", so a Contains check
        // passed on a slug with a stray plural — the same shape as WI-511's
        // British-forms list shipping `specialis`, which matches `specialist`.
        // Proven by mutating exactly that.
        var slugLine = CuratedPage.FrontMatter(Page)
            .Split('\n')
            .Select(l => l.TrimEnd('\r', ' ', '\t'))
            .FirstOrDefault(l => l.StartsWith("slug:", StringComparison.Ordinal));

        Assert.Equal($"slug: {Slug}", slugLine);
        Assert.True(
            File.Exists(Path.Combine(
                CuratedPage.BlocksRoot.Replace("blocks", "pages", StringComparison.Ordinal),
                Slug.Replace('/', Path.DirectorySeparatorChar) + ".md")),
            $"no file at pages/{Slug}.md, so the slug names a URL that does not resolve");
    }

    [Fact]
    public void TheSharedBlocksEveryHubOwesAreIncluded()
    {
        // Contract items 3 and 11, plus §12.3's deliberate-omissions paragraph
        // for [CAUSES] — the one WI-513 read as an aside and shipped without.
        Assert.Contains(
            "[MECHANISM]",
            Section("Where does it grow, and why does it cause these symptoms?"),
            StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);

        // WI-563. Pinned to the SECTION: an [ESCALATION] that drifts to the
        // bottom of the page is a reader who meets the ambulance tier four
        // screens after the symptoms that trigger it.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
        Assert.Contains(
            "[CAREGIVER]",
            Section("For the person caring for someone with this"),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageAnswersWhereItGrowsBeforeExplainingWhy()
    {
        // §12.6: a heading must not ask a question the section does not answer
        // — the same defect class as WI-506's "When will I know the result?"
        // heading over a section that never said when. This heading asks TWO
        // questions; [MECHANISM] answers only the second, and the first draft
        // left the first one hanging.
        //
        // CCO, already cited: "Tumors may arise anywhere in the CNS, but are
        // most often supratentorial and involve the subcortical white matter
        // and deep gray matter."
        var section = CuratedPage.Flatten(
            Section("Where does it grow, and why does it cause these symptoms?"));

        Assert.Matches(new Regex(@"\bsupratentorial\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"can start anywhere in the brain or spinal cord", RegexOptions.IgnoreCase),
            section);

        // And it answers BEFORE the block, not after it — the reader who stops
        // reading at the first paragraph is the one the rule is for.
        var raw = RawSection("Where does it grow, and why does it cause these symptoms?");
        var answer = raw.IndexOf("supratentorial", StringComparison.OrdinalIgnoreCase);
        var block = raw.IndexOf("[MECHANISM]", StringComparison.Ordinal);

        Assert.True(answer > 0 && block > 0 && answer < block,
            "the 'where' answer sits after [MECHANISM], so the heading's first question is "
            + "unanswered for anyone who does not read to the end of the block");
    }

    [Fact]
    public void ThePageAddsItsOwnFastGrowingSliceRatherThanRestatingTheBlock()
    {
        // §12.10 in the other direction. [MECHANISM] carries what a closed
        // skull does to anything inside it; what is NOT universal is that a
        // fast tumor gives the brain no time to adapt. That contrast is this
        // page's own, and it is the honest answer to "how did I not notice?".
        //
        // Springer (already in the block's sources, so it merges): peritumoral
        // edema in high-grade gliomas "is referred to as infiltrative edema
        // because it represents vasogenic edema in a zone of infiltrating tumor
        // cells".
        var section = CuratedPage.Flatten(
            Section("Where does it grow, and why does it cause these symptoms?"));

        Assert.Matches(
            new Regex(@"slow tumor gives the brain time to adjust", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"weeks, not years", RegexOptions.IgnoreCase), section);

        // The regression guard for the bad comparative this item shipped and
        // then removed. Springer describes the NATURE of the swelling —
        // "infiltrative edema ... in a zone of infiltrating tumor cells" — and
        // says nothing about how much of it there is. The draft rendered that
        // as "the swelling is heavier", which is a quantity claim no source
        // makes, on the most frightening hub on the site. §12.11 says this
        // class is "caught by reading the page, never by a gate"; this is the
        // cheap gate.
        Assert.Matches(
            new Regex(@"the swollen brain around the tumor is brain the cells have spread into",
                RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"swelling[^.]{0,40}\b(heavier|worse|greater|more)\b|"
                + @"\b(heavier|more|worse)\b[^.]{0,20}swelling", RegexOptions.IgnoreCase),
            section);

        // The self-blame landing, which is the reason the paragraph exists.
        Assert.Matches(
            new Regex(@"that is not something you missed", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Nn]one of these are things you can change by having noticed sooner",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo()
    {
        // WI-563: the tiers moved into Content/blocks/escalation.md and this
        // assertion moved with them. It ran here as three byte-identical copies
        // that had ALREADY drifted apart — this one lacked checks the others had
        // — which is the same failure as the prose it guards, one layer up.
        CuratedPage.AssertEscalationTiers(Page, "tumors/high-grade-glioma", SymptomHeading);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // Contract item 5, and this is the page where a figure would do most
        // harm. Checked on the COMPOSED reader text so a number arriving
        // through a shared block counts — and this page's own sources are full
        // of median-survival figures, which is exactly why.
        var reader = CuratedPage.ReaderText(Composed);

        Assert.DoesNotMatch(new Regex(@"\d+\s*(%|percent)", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\b(five|ten|two|three)[- ]year (survival|relative survival)",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(
            new Regex(@"median (survival|overall survival)", RegexOptions.IgnoreCase), reader);

        // A number written as a word is still a number (WI-511). The page's
        // sources report survival in months, so the word is the leak path.
        //
        // NO "with" exemption here. That lookahead was copied from the
        // low-grade page's guard, where it protects "live many years WITH a
        // grade 2 glioma" — which is exactly the phrasing most likely to leak
        // a prognosis claim onto THIS page. Carrying the exemption over would
        // have imported a hole along with the rule.
        Assert.DoesNotMatch(
            new Regex(@"(survive|survival)[^.]{0,40}\b(months|years)\b", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(reader));
    }

    [Fact]
    public void TheOutlookGateIsClosedAndTeachesMedianRatherThanBanningTheWord()
    {
        // §12.9: a gate that publishes no figures still has to teach the
        // vocabulary. §12.5 wants four moves for "median" — that it describes a
        // GROUP, the distribution rather than the midpoint, the right skew, and
        // plainly that it is not a prediction about one person.
        var section = Section(OutlookHeading);

        Assert.Contains(":::outlook", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"middle of a group", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not a prediction about any one person", RegexOptions.IgnoreCase), section);

        // BOTH, not either. §12.5 asks for the distribution AND the right skew
        // as separate moves; written as an alternation, deleting the long tail
        // stayed green off "not spread evenly" — which states there is a shape
        // without ever saying which way it leans.
        Assert.Matches(new Regex(@"not spread evenly", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"long tail of people who do far", RegexOptions.IgnoreCase), section);

        // The reason specific to THIS page: the 2021 rules split groups that
        // used to be counted together, so an older figure may describe a
        // different disease from the reader's.
        Assert.Matches(
            new Regex(@"counted together are now known to be different diseases",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void NothingSitsOutsideTheOutlookGateInThatSection()
    {
        // WI-513's right-tail defect and WI-514's correction of its own fix.
        // §12.5: the warning and the labels are emitted by the component, "not
        // typed per page". A post-gate paragraph duplicating
        // ReaderGate.WarningText is a spec violation that builds green.
        var section = Section(OutlookHeading);

        var gate = Regex.Match(section, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook section has no closed ::: gate");

        var inside = CuratedPage.Flatten(gate.Groups[1].Value);
        var outside = CuratedPage.Flatten(section.Replace(gate.Value, " ", StringComparison.Ordinal));

        Assert.Matches(new Regex(@"long tail", RegexOptions.IgnoreCase), inside);
        Assert.DoesNotMatch(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), outside);
        Assert.DoesNotMatch(new Regex(@"long tail", RegexOptions.IgnoreCase), outside);

        Assert.True(string.IsNullOrWhiteSpace(outside.Replace("#", "").Trim()),
            $"prose sits outside the outlook gate, where the component already speaks: {outside}");
    }

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3, and §12.9's first warning: a hub follows the seventeen
        // sections, NOT §12.8's twelve slots. Order, not set membership — the
        // design principle is "action at the end of every frightening block",
        // which is a statement about sequence.
        var headings = Regex.Matches(CuratedPage.Body(Page), @"^## (.+?)\s*$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        var expected = new[]
        {
            "The short version",
            WhatHeading,
            GradeHeading,
            "Where does it grow, and why does it cause these symptoms?",
            SymptomHeading,
            "How do doctors find out it is this?",
            ReportHeading,
            TreatmentHeading,
            "What is treatment actually like, and what is normal afterwards?",
            "Everyday life: work, driving, seizures and tiredness",
            ScansHeading,
            RecurrenceHeading,
            "Did I cause this?",
            OutlookHeading,
            "For the person caring for someone with this",
            "What to ask your team",
            "Where to get support",
        };

        Assert.Equal(expected, headings);
    }

    [Fact]
    public void OutlookSitsAfterEverythingActionableAndTheSelfBlameBlockIsDemoted()
    {
        // §12.3's ordering rationale as a rule, pinned separately from the full
        // order so the REASON survives a reshuffle. And §12.3's deliberate
        // omissions: the self-blame block "still appears; it just is not near
        // the top".
        var body = CuratedPage.Body(Page);

        var treatment = body.IndexOf($"## {TreatmentHeading}", StringComparison.Ordinal);
        var recurrence = body.IndexOf($"## {RecurrenceHeading}", StringComparison.Ordinal);
        var causes = body.IndexOf("## Did I cause this?", StringComparison.Ordinal);
        var outlook = body.IndexOf($"## {OutlookHeading}", StringComparison.Ordinal);
        var caregiver = body.IndexOf(
            "## For the person caring for someone with this", StringComparison.Ordinal);

        Assert.True(treatment < outlook, "outlook must come after treatment");
        Assert.True(recurrence < outlook, "outlook must come after 'if it comes back'");
        Assert.True(causes > 0, "the page has no self-blame section");
        Assert.True(causes < outlook, "the self-blame block belongs before the outlook gate");
        Assert.True(causes > body.Length / 2,
            "the self-blame block is near the top, which §12.3 explicitly rules out");
        Assert.True(outlook < caregiver, "the caregiver section must follow outlook");
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);
        Assert.Contains("slug: tumors/high-grade-glioma", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"^\s+- url:", RegexOptions.Multiline).Count;
        var accessed = Regex.Matches(front, @"^\s+accessed:", RegexOptions.Multiline).Count;
        Assert.True(urls > 0, "the page cites no sources");
        Assert.Equal(urls, accessed);
    }

    [Fact]
    public void TheSourcesThatCannotBeFetchedOrAreForbiddenNeverAppear()
    {
        // Eight of the dossier's §3 citations failed verification for WI-515.
        // Each of these URLs either cannot be opened — a citation nobody can
        // open is one nobody can verify (WI-509) — or is forbidden by §12.1, or
        // does not say what it is cited for.
        //
        //  - academic.oup.com: RE-CONFIRMED 403 "Just a moment..." on
        //    2026-09-07, for BOTH the pseudoprogression paper and CATNON. Open
        //    replacements found via Europe PMC: PMC6655498 and PMC13186461.
        //  - moffitt.org: Cloudflare-gated (WI-514). The dossier hangs §3.1's
        //    whole "grouping" claim on it.
        //  - mdpi.com: 1-byte JavaScript shell (WI-514).
        //  - ascopubs.org / ascopost.com: JavaScript shells (WI-511).
        //  - PMC12406498: an audit of steroid COMPLICATIONS (WI-514). Not
        //    needed — [MECHANISM] already carries the swelling answer.
        //  - NBK559184: the dossier cites it twice in §3 for general high-grade
        //    glioma treatment. It is the StatPearls OLIGODENDROGLIOMA chapter,
        //    which §12.1 names as internally inconsistent and bans. Radiation
        //    necrosis is re-sourced to ACS at patient level.
        //  - PMC3643853: a 21-patient single-centre retrospective. The claim it
        //    carries is dropped rather than published at that strength.
        //  - cancer.gov patient PDQ: §12.1, for naming and grading.
        //
        // Checked on the page front matter AND on every shared block, because
        // block `sources` merge into the including page and RENDER in the
        // reader's source list (§12.10) — so a bad URL added to a block ships
        // onto 24 hubs while a front-matter-only check stays green.
        var surfaces = new List<(string Where, string Text)>
        {
            ("tumors/high-grade-glioma front matter", CuratedPage.FrontMatter(Page)),
        };

        foreach (var file in Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"))
        {
            surfaces.Add(($"blocks/{Path.GetFileName(file)}", File.ReadAllText(file)));
        }

        foreach (var bad in new[]
                 {
                     "academic.oup.com",
                     "moffitt.org",
                     "mdpi.com",
                     "ascopubs.org",
                     "ascopost.com",
                     "PMC12406498",
                     "NBK559184",
                     "PMC3643853",
                     "cancer.gov/types/brain/patient",
                     "cancer.gov/types/brain/hp",
                 })
        {
            foreach (var (where, text) in surfaces)
            {
                Assert.False(
                    text.Contains(bad, StringComparison.OrdinalIgnoreCase),
                    $"'{bad}' is cited in {where}, and it either cannot be fetched, is forbidden "
                    + "by §12.1, or does not support the claim it was cited for");
            }
        }
    }

    [Fact]
    public void TheStatPearlsGliomasChapterIsCitedForTreatmentAndNeverForNaming()
    {
        // §12.1's per-claim rule, and this page is where it bites. NBK441874
        // carries the one sentence this page needs about the standard of care
        // ("the Stupp protocol is the standard of care for high-grade
        // gliomas") — and in the same article writes "High-grade gliomas (WHO
        // grade III and IV)". It is pre-CNS5 notation sitting beside a claim
        // worth having, which is exactly the WI-513 shape.
        //
        // The comment in the front matter is the record of that decision, and
        // this test fails if somebody deletes it while leaving the URL.
        //
        // The comment markers are STRIPPED and the result FLATTENED before
        // matching, in that order, and both steps were learned the hard way on
        // this test. The comment wraps at 79 characters with the break between
        // "NEVER" and "for naming", so an unflattened match fails on a comment
        // that says exactly the right thing (WI-511's "a lift"). Flattening
        // alone is not enough either: it leaves the next line's "#" sitting
        // inside the sentence, as "scan. NEVER # for naming or grading".
        var front = CuratedPage.Flatten(
            Regex.Replace(CuratedPage.FrontMatter(Page), @"(?m)^[ \t]*#[ \t]?", ""));

        Assert.Contains("NBK441874", front, StringComparison.Ordinal);

        // The scope note names what the chapter IS cited for and what it is
        // not. Review found that the page's own thesis sentence — "high-grade
        // glioma means grade 3 or grade 4" — has no other fetchable source:
        // CCO and CNS5 use the term without ever defining its range, and
        // moffitt.org (the dossier's source for it) is Cloudflare-gated. So
        // the range is taken from here DELIBERATELY, with the Roman numerals
        // translated, and the note says so. The naming ban stands.
        Assert.Matches(new Regex(@"GRADE RANGE", RegexOptions.IgnoreCase), front);
        Assert.Matches(
            new Regex(@"NEVER cited\s+for a tumor NAME", RegexOptions.IgnoreCase), front);
    }

    [Fact]
    public void ThePageNeverMinimisesOrCharacterisesTheDiagnosis()
    {
        // The shared guards, run on the composed reader text so a phrase
        // arriving through a block counts. On this page the risk runs one way:
        // an invented reassurance on the most frightening hub on the site.
        var reader = CuratedPage.ReaderText(Composed);

        CuratedPage.AssertNeverMinimises(reader, "tumors/high-grade-glioma");

        var flat = CuratedPage.Flatten(reader);
        foreach (var phrase in new[]
                 {
                     "the good kind", "nothing to worry about", "the best one to have",
                     "at least it is", "could be worse",
                 })
        {
            Assert.DoesNotContain(phrase, flat, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // WI-511's gate, which had to flatten before matching because a hard
        // wrap falls between the two words of "a lift".
        var flat = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        foreach (var form in CuratedPage.BritishForms)
        {
            if (CuratedPage.BritishFormExemptions.Any(
                    e => flat.Contains(e, StringComparison.OrdinalIgnoreCase)
                         && e.Contains(form, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            Assert.DoesNotContain(form, flat, StringComparison.OrdinalIgnoreCase);
        }
    }
}

/// <summary>
/// WI-515 against the running site. §12.9: the gate must be VERIFIED rather
/// than assumed, "because every fail-open mode builds green".
/// </summary>
/// <summary>
/// WI-517: <c>[Collection(DatabaseCollection.Name)]</c> is what serializes a
/// render class against the one dev database. This class shipped without it,
/// and so did two siblings — they had been racing every other DB-touching
/// class since they were written. It only surfaced when a FOURTH render class
/// arrived: CI came back with 397 failures, every one of them
/// "23505: duplicate key value violates unique constraint
/// pg_database_datname_index" — two fixtures running CREATE DATABASE at once.
/// Locally it looked like flake, because a different test failed each run.
/// A suite that fails somewhere else each time it runs is not flaky, it is
/// contended.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class HighGradeGliomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/high-grade-glioma";

    private readonly WebApplicationFactory<Program> _factory;

    public HighGradeGliomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        // Contract item 10: existing /tumors/* URLs are preserved. This page
        // existed as a 194-word stub and readers may have it bookmarked.
        var response = await _factory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/seizures/living-with", "/seizures/what-to-do");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // WI-513's check: AssertLinksResolve's regex stops at the '#', so a
        // link to a missing anchor comes back a healthy 200 while landing the
        // reader at the top of a long page. This page deep-links
        // /seizures/living-with#driving.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        var heading = html.IndexOf("id=\"what-might-happen-over-time\"", StringComparison.Ordinal);
        var gate = html.IndexOf("reader-gate__disclosure", StringComparison.Ordinal);

        Assert.True(heading > 0, "the outlook heading is not on the rendered page");
        Assert.True(gate > 0, "the reader-choice gate did not render");
        Assert.True(gate > heading, "the outlook heading has been swallowed into the disclosure");

        Assert.DoesNotContain(":::", html, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"<details[^>]*\bopen\b"), html);
    }

    [Fact]
    public async Task TheGatedWordsAppearNowhereOutsideTheGate()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        var gate = Regex.Match(html, @"<details class=""reader-gate__disclosure"".*?</details>",
            RegexOptions.Singleline);
        Assert.True(gate.Success, "the reader-choice gate did not render as a closed disclosure");

        var outside = Regex.Replace(html.Replace(gate.Value, " ", StringComparison.Ordinal),
            "<[^>]+>", " ");

        foreach (var gated in new[] { "median", "long tail" })
        {
            Assert.DoesNotContain(gated, outside, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task EveryBlockDirectiveComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var directive in new[]
                 { "[MECHANISM]", "[CROSSWALK]", "[CAUSES]", "[CAREGIVER]", "[ESCALATION]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // A word from each block, so a directive that expanded to nothing is
        // not mistaken for one that expanded correctly.
        Assert.Contains("closed box", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("changed in 2021", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ThePagesNewVocabularyFiresAsTooltips()
    {
        // Contract item 9. PCV is named on the page and never spelled out
        // there, so the tooltip is the only place a reader learns it is three
        // drugs — which is why it is NOT in the suppression line.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "pcv", "radiation-necrosis" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void EverySuppressedTermIsARealGlossaryEntryTheProseActuallyUses()
    {
        // WI-512 shipped two suppressions that suppressed nothing and WI-514
        // shipped four. Both halves are silent no-ops, so both are checked.
        // This page's own first draft suppressed "adult-type", which appears
        // only in a citation TITLE — caught by exactly this check.
        var page = CuratedPage.Read("tumors", "high-grade-glioma.md");
        var suppressed = Regex.Matches(page, @"!%(.+?)%")
            .Select(m => m.Groups[1].Value)
            .ToList();

        Assert.NotEmpty(suppressed);

        var terms = Directory.EnumerateFiles(
                CuratedPage.BlocksRoot.Replace("blocks", "glossary", StringComparison.Ordinal), "*.md")
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"^(?:term|also): *(.+)$",
                    RegexOptions.Multiline)
                .Select(m => m.Groups[1].Value.Trim().Trim('[', ']')))
            .SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // The BODY only. A term that appears solely in a citation title is not
        // prose, and suppressing it suppresses nothing.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Body(page)));

        foreach (var term in suppressed)
        {
            Assert.True(terms.Contains(term),
                $"'{term}' is suppressed but is not a glossary term, so the suppression is a no-op");
            Assert.True(reader.Contains(term, StringComparison.OrdinalIgnoreCase),
                $"'{term}' is suppressed but the page's prose never uses the word, so there was "
                + "nothing to suppress");
        }
    }
}
