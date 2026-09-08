using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-516: <c>/tumors/astrocytoma</c> — the hub the CNS5 renames land on more
/// than any other, and the first Wave 2 page where THREE unrelated groups share
/// one word.
///
/// Seventeen sections per §12.3. Written from §12.9, §12.10 and §12.11 rather
/// than proving them; §12.11 is WI-515's and its first lesson is the one this
/// page is most exposed to — **a claim about "these tumors" on a page covering
/// several must name WHICH ones**. Here that is not a nicety: one of the three
/// groups is curable by surgery and another is not.
///
/// The three things the backlog names, each with a test below:
///   1. three families share the name;
///   2. personality and behaviour change, which can precede diagnosis by months
///      to years, handled as more than a symptom bullet;
///   3. the crosswalk slice that matters most — BOTH directions of the
///      "secondary glioblastoma" rename.
/// </summary>
public sealed class AstrocytomaPageContentTests
{
    private static string Page => CuratedPage.Read("tumors", "astrocytoma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, "tumors/astrocytoma");

    private const string WhatHeading = "What is an astrocytoma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string LocationHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string RecurrenceHeading = "If it comes back, or changes";
    private const string OutlookHeading = "What might happen over time";
    private const string CaregiverHeading = "For the person caring for someone with this";

    /// <summary>
    /// One "## " section with line breaks intact. §12.11: CuratedPage.Section
    /// FLATTENS before it returns, so anything that splits needs the raw text
    /// or it silently stops being a splitter.
    /// </summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(CuratedPage.Body(Page),
            $@"^## {Regex.Escape(heading)}\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);

        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

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

    private static IEnumerable<string> ChunksContaining(string markdown, string term) =>
        Regex.Split(markdown.Replace("\r\n", "\n", StringComparison.Ordinal), @"\n\s*\n|\n(?=\s*[-*] )")
            .Select(CuratedPage.Flatten)
            .Where(chunk => chunk.Contains(term, StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void TheOpeningSaysThreeDifferentGroupsShareTheName()
    {
        // Backlog claim 1 and the organising fact. A reader told "astrocytoma"
        // and nothing else has been told something that spans a tumor surgery
        // can cure and one it cannot.
        var shortVersion = CuratedPage.Flatten(Section("The short version"));

        Assert.Matches(
            new Regex(@"[Tt]hree different groups of tumors share the name", RegexOptions.IgnoreCase),
            shortVersion);
        Assert.Matches(
            new Regex(@"not variations of one thing", RegexOptions.IgnoreCase), shortVersion);

        // And the opening must say where the real answer lives.
        Assert.Matches(new Regex(@"pathology report", RegexOptions.IgnoreCase), shortVersion);

        // The rename is signposted from the top, because a reader holding old
        // paperwork arrives here not knowing this is their page.
        //
        // Deliberately NOT the words "secondary glioblastoma": the first draft
        // promised in the short version that anyone holding that phrase was in
        // the right place, and no source supports that unconditionally — see
        // BothDirectionsOfTheGlioblastomaRenameAreOnThePage. The signpost now
        // names the change, and the report section routes by IDH.
        Assert.Matches(
            new Regex(@"moved a whole group of\s+tumors out from under the name \*\*glioblastoma\*\*",
                RegexOptions.IgnoreCase),
            shortVersion);
        Assert.DoesNotMatch(
            new Regex(@"secondary\s+glioblastoma", RegexOptions.IgnoreCase), shortVersion);
    }

    [Fact]
    public void TheThreeGroupsAreNamedAndToldApartByWhatChangesForTheReader()
    {
        // Naming three groups is not enough — the reason a reader needs them is
        // that the answer to "can this be cured" differs between them. So the
        // section must carry the DIFFERENCE, not just the taxonomy.
        var section = CuratedPage.Flatten(Section(WhatHeading));

        Assert.Matches(new Regex(@"[Aa]strocytoma, IDH-mutant", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"\bcircumscribed\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Pp]ilocytic astrocytoma", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"usually seen in children", RegexOptions.IgnoreCase), section);

        // CNS5's own table gives PILOCYTIC astrocytoma grade 1 and "Good with
        // complete resection" — and in the same table gives pleomorphic
        // xanthoastrocytoma and chordoid glioma grade 2-3, high-grade
        // astrocytoma with piloid features grade 3, and astroblastoma no
        // established grade. So the curable promise is BOUND to pilocytic here.
        //
        // The first draft said "these are usually grade 1, and an operation
        // that takes all of it out can be the end of it" of the whole
        // circumscribed family, and this assertion — unbound — blessed it.
        // That is §12.11's scoping defect with the sign flipped: WI-515
        // over-frightened, this over-reassured, which is worse on a medical
        // page because it is the kind of reassurance that stops being true.
        Assert.Matches(
            new Regex(@"[Pp]ilocytic astrocytoma[^#]{0,220}can be the end of it",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"this group is mixed", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"it is the grade on your report, not\s+the word circumscribed",
                RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"[Tt]hese are usually grade 1", RegexOptions.IgnoreCase), section);

        // And the question that gets the reader further.
        Assert.Matches(
            new Regex(@"is mine IDH-mutant, what grade is it, and does it have a clear edge",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void ThePageAnswersWhetherAstrocytomaIsTheSameAsGlioblastoma()
    {
        // Research §4.7 question 1, and the page cannot dodge it: ACS says
        // "glioblastoma is the most common type of astrocytoma" while CNS5
        // makes them separate diagnoses that IDH separates absolutely. Both
        // halves are true and a reader meets both on the internet.
        var section = CuratedPage.Flatten(RawSubsection("Is an astrocytoma the same thing as a glioblastoma?"));

        Assert.Matches(
            new Regex(@"most common type of astrocytoma", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"separate diagnoses", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"never called glioblastoma now", RegexOptions.IgnoreCase), section);

        // The consequence a reader can act on: research about glioblastoma is
        // not research about them.
        Assert.Matches(
            new Regex(@"not written about you", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void CurabilityIsScopedToTheGroupItIsTrueOfAndMatchesTheSiblings()
    {
        // §12.11's first lesson, and this page is the one that most needs it:
        // WI-515 shipped a draft asserting incurability of every high-grade
        // glioma, and here the same sentence would be wrong for an entire
        // third of the page's subject.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        // §12.6: answer in the FIRST sentence. The heading asks "is it cancer"
        // and the first draft opened with "it depends which of the three
        // groups you are in", then defined both malignant and benign and only
        // ever applied one of them — so a pilocytic reader was never told
        // which word was theirs, and a defined-but-unused term is the "parses
        // but means nothing" failure §12.6 rules out.
        var first = CuratedPage.SentencesOf(section).First();
        Assert.Matches(
            new Regex(@"^\s*\*\*Some are and some are not", RegexOptions.IgnoreCase), first);
        Assert.Matches(
            new Regex(@"[Gg]rade 1\s+astrocytomas with a clear edge are usually called benign",
                RegexOptions.IgnoreCase),
            section);

        // The curable group, said plainly.
        Assert.Matches(
            new Regex(@"[Gg]rade 1 astrocytomas usually sit in a lump with an edge",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"can genuinely be the end of it", RegexOptions.IgnoreCase), section);

        // The group that is not, SCOPED — and asserted as ONE regex spanning
        // the scope and the claim. As two independent matches on a flattened
        // section, a draft could keep the scoped topic sentence and then write
        // "All astrocytomas are malignant, and they are not curable" beneath
        // it, and both assertions would still find something. §12.10's trap:
        // a test asserting a phrase where the claim is the property.
        Assert.Matches(
            new Regex(@"[Gg]rade 2 and above IDH-mutant astrocytomas are different"
                + @"[^#]{0,260}they are not curable", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"not the same as untreatable", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not a timeline", RegexOptions.IgnoreCase), section);

        // Matched to the siblings by READING them (§12.10), so the four glioma
        // pages cannot drift apart on the site's bluntest claim.
        foreach (var sibling in new[] { "glioma.md", "low-grade-glioma.md", "high-grade-glioma.md" })
        {
            var text = CuratedPage.Flatten(
                CuratedPage.ReaderText(CuratedPage.Read("tumors", sibling)));
            Assert.True(
                Regex.IsMatch(text, @"they are not curable", RegexOptions.IgnoreCase),
                $"/tumors/{sibling} no longer says 'they are not curable', so the strength this "
                + "page was matched to has moved and all four need rechecking");
        }
    }

    [Fact]
    public void TheGradeSectionExplainsAGradeSetByGeneResultsAlone()
    {
        // CCO: homozygous CDKN2A/B deletion is "diagnostic of a CNS WHO grade 4
        // tumor" on its own. Without this, a reader whose grade jumped when the
        // gene results came back reads their report as a contradiction.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(new Regex(@"CDKN2A/B", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"graded 4 on the gene result\s+alone", RegexOptions.IgnoreCase), section);

        // §12.6: the frightening block lands on something solid.
        Assert.Matches(
            new Regex(@"not a mistake and nobody\s+was hiding anything", RegexOptions.IgnoreCase),
            section);
        Assert.Contains("/tests/molecular-markers", Section(GradeHeading), StringComparison.Ordinal);

        // Grading happens within tumor type — the claim that stops a reader
        // comparing their grade 4 with somebody else's.
        Assert.Matches(
            new Regex(@"inside each tumor type", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void PersonalityChangeIsGivenItsOwnSectionAndNotASymptomBullet()
    {
        // Backlog claim 2. NBTS verbatim: "with tumors in the frontal lobe,
        // changes in behavior and personality can be present months to years
        // before diagnosis". Research §4.3 says families reinterpret years of
        // "he became difficult" after diagnosis and that the reinterpretation
        // "deserves compassionate handling".
        //
        // A symptom bullet cannot do that, so this asserts the SECTION exists
        // and carries the three things that make it more than a fact.
        var section = CuratedPage.Flatten(RawSubsection("The change that came before the diagnosis"));

        Assert.Matches(
            new Regex(@"months to years\s+before diagnosis", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"frontal lobe", RegexOptions.IgnoreCase), section);

        // ATTRIBUTED to the organization that says it, not to "the research".
        // NBTS is a patient-advocacy page, and the page already names ACS by
        // name two sections earlier — calling one "the research" and the other
        // by name is the WI-512 attribution defect in miniature.
        Assert.Matches(
            new Regex(@"National Brain Tumor Society puts it plainly", RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"The research says this plainly", RegexOptions.IgnoreCase), section);

        // The three moves that make it compassionate rather than clinical.
        Assert.Matches(
            new Regex(@"was not choosing it", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Nn]obody missed something obvious", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"worth telling your team what you noticed", RegexOptions.IgnoreCase), section);

        // §12.6: it must not end on the grief. The landing is that some of it
        // is treatable and some of it is the medicines.
        Assert.Matches(
            new Regex(@"steroids or the seizure\s+medicine", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheLocationAnswerComesBeforeTheSharedMechanismBlock()
    {
        // §12.6 and §12.11: a heading that asks TWO questions owes two answers,
        // and [MECHANISM] only answers the "why". The "where" is unusually
        // load-bearing on this page because the frontal-lobe bias is what sets
        // up the personality section.
        var raw = RawSection(LocationHeading);
        var answer = raw.IndexOf("frontal lobes", StringComparison.OrdinalIgnoreCase);
        var block = raw.IndexOf("[MECHANISM]", StringComparison.Ordinal);

        Assert.True(answer > 0, "the section never says where this tumor usually grows");
        Assert.True(block > 0, "the section does not include [MECHANISM]");
        Assert.True(answer < block,
            "the 'where' answer sits after [MECHANISM], so the heading's first question is "
            + "unanswered for anyone who does not read to the end of the block");
    }

    [Fact]
    public void BothDirectionsOfTheGlioblastomaRenameAreOnThePage()
    {
        // Backlog claim 3, and the reason this page exists in the shape it
        // does. Research §4.4: the reassuring direction is that "secondary
        // glioblastoma" is now astrocytoma, IDH-mutant, grade 4. The other is
        // that an IDH-WILDTYPE tumor once called "anaplastic astrocytoma" may
        // now be glioblastoma — CNS5 verbatim: "all tumors lacking IDH
        // mutations that have concomitant gain of chromosome 7 and loss of
        // chromosome 10, EGFR amplification, or TERT promoter mutations are
        // called glioblastoma and are given a WHO grade of 4".
        //
        // The dossier says say it plainly: "it is worse to let them discover it
        // by accident." Shipping only the comfortable half would be the site
        // picking the half that is nicer to write.
        var section = CuratedPage.Flatten(CuratedPage.ComposedSection(Page, ReportHeading));

        // Direction one: reassuring, and CONDITIONAL on the line the reader can
        // actually look up.
        //
        // The first draft said "if your old report said secondary
        // glioblastoma... you are in the right place" unconditionally. That is
        // unsourced: the phrase "secondary glioblastoma" appears in NO fetched
        // source across three items except three bibliography entries, and what
        // CNS5 actually says is about IDH status ("IDH-mutant astrocytomas are
        // no longer referred to as glioblastomas"), not about how the tumor
        // arose. "Secondary" described the route, and not every secondary
        // glioblastoma is IDH-mutant — so an IDH-wildtype reader holding that
        // word was told, in bold and first, that this was their page.
        Assert.Matches(
            new Regex(@"glioblastoma with an IDH change, you are in the right\s+place",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"astrocytoma, IDH-mutant, grade 4", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Yy]ou have not been given a different disease", RegexOptions.IgnoreCase),
            section);

        // And the older phrase is handled on its own terms, routed by IDH
        // rather than claimed.
        Assert.Matches(
            new Regex(@"older phrase secondary glioblastoma", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"IDH-mutant means this page. IDH-wildtype means", RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"secondary glioblastoma[^.]{0,60}you are in the right place",
                RegexOptions.IgnoreCase),
            section);

        // Direction two: not. Asserted as ONE claim rather than as three
        // separately-satisfiable fragments — deleting the sentence that
        // introduces it left every fragment matchable from surviving text
        // elsewhere on the page ("IDH-wildtype" is in the questions list,
        // "now told they have a glioblastoma" is in the paragraph below), so
        // the test passed on a page that no longer stated the rule.
        Assert.Matches(
            new Regex(@"the 2021 rules may call\s+it a \*\*glioblastoma\*\*, grade 4, "
                + @"on gene results\s+alone", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"even when the cells did not look\s+grade 4", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"nobody warns people about", RegexOptions.IgnoreCase), section);

        // Named mechanisms, so the reader can find them on their own report.
        foreach (var marker in new[] { "chromosome 7", "chromosome 10", "EGFR", "TERT" })
        {
            Assert.Contains(marker, section, StringComparison.OrdinalIgnoreCase);
        }

        // And it must not be softened into nothing: the page has to say this is
        // a real change and that being shaken by it is fair.
        Assert.Matches(
            new Regex(@"real change in what you are called", RegexOptions.IgnoreCase), section);
        Assert.Contains("/tumors/glioblastoma", CuratedPage.ComposedSection(Page, ReportHeading),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheHardDirectionOfTheRenameIsNotBuriedBelowTheReassuringOne()
    {
        // POSITION, not presence (WI-512, and §12.10's second trap). Both
        // directions being "on the page" is satisfied by a footnote after four
        // screens of reassurance. The subsection heading has to promise both,
        // so a reader skimming headings cannot miss the one that affects them.
        var heading = Regex.Match(CuratedPage.Body(Page),
            @"^### (The renames.*)$", RegexOptions.Multiline);

        Assert.True(heading.Success, "the rename subsection has no heading of its own");
        Assert.Matches(
            new Regex(@"both ways|both directions", RegexOptions.IgnoreCase),
            heading.Groups[1].Value);

        // And the section says up front that one direction is not reassuring,
        // rather than letting the reader find out at the bottom.
        var opening = CuratedPage.SentencesOf(
            CuratedPage.Flatten(RawSubsection(heading.Groups[1].Value))).Take(3);

        Assert.Contains(opening, s =>
            Regex.IsMatch(s, @"one direction is reassuring and the other is not",
                RegexOptions.IgnoreCase));
    }

    [Fact]
    public void EveryRetiredNameIsNamedAsRetiredRatherThanUsedAsALiveDiagnosis()
    {
        // §12.9. Chunk-scoped, because WI-514 proved a character window reads
        // "no longer" out of the adjacent crosswalk block.
        foreach (var retired in new[]
                 {
                     "anaplastic", "secondary glioblastoma", "diffuse astrocytoma", "multiforme",
                 })
        {
            foreach (var chunk in ChunksContaining(Composed, retired))
            {
                Assert.True(
                    // "is now" / "are now" are DELIBERATELY absent from this
                    // list. They are close to vacuous as evidence a name is
                    // being named rather than used — "Anaplastic astrocytoma
                    // is now treated with radiation first" would pass while
                    // using the retired name as a live diagnosis. The specific
                    // markers below carry the check on their own.
                    Regex.IsMatch(chunk,
                        @"retired|no longer|old rules|older report|older paperwork|old report|"
                        + @"older phrase|once said|used to be|changed in 2021|before 2021|"
                        + @"still hear|still say|dropped|was renamed|is now called|meant grade|"
                        + @"moved out from under",
                        RegexOptions.IgnoreCase),
                    $"'{retired}' is used as a live diagnosis rather than named as a retired term, "
                    + $"in: {chunk}");
            }
        }
    }

    [Fact]
    public void RomanNumeralGradesNeverAppearAsLiveGrades()
    {
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Body(Page)));

        // IgnoreCase, and it is not decoration. §12.9 records this trap biting
        // three times (WI-505, WI-506, WI-508): a guard written case-sensitive
        // walks straight past the sentence-initial form, which for a grade is
        // the commonest one — "Grade IV" opens a list item. FIVE copies of this
        // guard across the suite were case-sensitive until WI-516's break
        // harness planted "**Grade III.**" and every one of them stayed green.
        Assert.DoesNotMatch(
            new Regex(@"grade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase), body);

        // The canary: the shared block is supposed to keep printing the old
        // notation for the reader holding it.
        var crosswalk = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md"));
        Assert.Matches(
            new Regex(@"grade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase), crosswalk);
    }

    [Fact]
    public void TheSliceDoesNotRepeatWhatTheSharedCrosswalkBlockAlreadySays()
    {
        // §12.10/§12.11's division of labour, asserted against the BLOCK's own
        // words so that moving a sentence into the block later turns the
        // duplicate red instead of silently creating one.
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
    public void TheReportSectionExplainsTheFourResultsThatDecideTheDiagnosis()
    {
        // CCO: IDH is defining; 1p/19q separates astrocytoma from
        // oligodendroglioma; ATRX loss and TP53 are the supporting pair
        // ("strong expression of p53 and loss of ATRX staining"); CDKN2A/B sets
        // grade 4. A reader holding the report meets all four words.
        var section = CuratedPage.Flatten(Section(ReportHeading));

        foreach (var marker in new[] { "IDH", "1p/19q", "ATRX", "TP53", "CDKN2A/B" })
        {
            Assert.Contains(marker, section, StringComparison.OrdinalIgnoreCase);
        }

        // 1p/19q must be given in the direction that tells the reader which
        // page is theirs, not as a bare fact.
        Assert.Contains("/tumors/oligodendroglioma", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Matches(
            new Regex(@"not\*\* co-deleted|not co-deleted", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheTreatmentSectionTellsTheThreeGradesApart()
    {
        // Research §4.6 and CATNON. Grade 3 gets radiation THEN temozolomide,
        // and that is a different regimen from a glioblastoma's — which is the
        // whole reason a reader compares plans and worries.
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(new Regex(@"[Ww]hy the grades are treated differently", RegexOptions.IgnoreCase),
            section);

        // SCOPED to IDH-mutant. CATNON is an IDH-mutant grade 3 astrocytoma
        // trial, and this page also covers grade 3 circumscribed tumors — so an
        // unscoped "Grade 3" bullet hands a grade 3 pleomorphic
        // xanthoastrocytoma reader a regimen that was never tested on them.
        // The grading subsection above scopes itself explicitly; the treatment
        // bullets did not until review caught it.
        Assert.Matches(
            new Regex(@"These three are for the IDH-mutant group", RegexOptions.IgnoreCase),
            section);
        foreach (var grade in new[] { 2, 3, 4 })
        {
            Assert.Matches(
                new Regex($@"\*\*Grade {grade}, IDH-mutant\.\*\*", RegexOptions.IgnoreCase),
                section);
        }

        Assert.Matches(
            new Regex(@"Radiation \*\*first\*\*, then chemotherapy afterwards", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"did not help, so it is\s+usually left out", RegexOptions.IgnoreCase), section);

        // Grade 4 is treated LIKE a high-grade glioma but is not a
        // glioblastoma — the page's whole thesis, restated where it is acted on.
        Assert.Matches(
            new Regex(@"as its own diagnosis\s+rather than as a glioblastoma", RegexOptions.IgnoreCase),
            section);

        // No doses (R1).
        Assert.DoesNotMatch(new Regex(@"\b\d+\s*(Gy|mg)\b", RegexOptions.IgnoreCase), section);

        // Vorasidenib is named, and routed to the page that explains it, rather
        // than described twice.
        Assert.Matches(new Regex(@"[Vv]orasidenib", RegexOptions.IgnoreCase), section);
        Assert.Contains("/tumors/low-grade-glioma", Section(TreatmentHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TransformationIsNamedWithBothRoutesAndIsNotAFailureStory()
    {
        var section = CuratedPage.Flatten(Section(RecurrenceHeading));

        Assert.Matches(new Regex(@"\btransformation\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"grade 3 or straight to grade 4", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"rather than a sign something went wrong", RegexOptions.IgnoreCase), section);

        // §12.6: lands on options, not on the fear.
        Assert.Matches(
            new Regex(@"not the same as being\s+told there is nothing left to try",
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
        CuratedPage.AssertEscalationTiers(Page, "tumors/astrocytoma", SymptomHeading);
    }

    [Fact]
    public void TheCaregiverSectionCarriesThePersonalityChangeMaterial()
    {
        // §12.7: the block holds what is true whatever the tumor is, and the
        // page adds its own half AFTER the directive. On this page that half is
        // the reason the section is not interchangeable with any other hub's —
        // the caregiver has often been living with the personality change for
        // longer than anybody has had a diagnosis.
        var raw = RawSection(CaregiverHeading);
        var block = raw.IndexOf("[CAREGIVER]", StringComparison.Ordinal);
        Assert.True(block > 0, "the caregiver section does not include the shared block");

        var own = CuratedPage.Flatten(raw[(block + "[CAREGIVER]".Length)..]);

        Assert.Matches(
            new Regex(@"may go back months or years", RegexOptions.IgnoreCase), own);
        Assert.Matches(
            new Regex(@"thinking the relationship was in trouble", RegexOptions.IgnoreCase), own);

        // The absolution, written to the caregiver in the second person (§12.7).
        Assert.Matches(new Regex(@"\*\*You were not\.\*\*", RegexOptions.IgnoreCase), own);

        // And it must not stop at feelings: §12.7 wants what the person at home
        // actually has to do.
        Assert.Matches(
            new Regex(@"Write down what you noticed and when", RegexOptions.IgnoreCase), own);
        Assert.Matches(
            new Regex(@"which changes are the tumor, which are the medicines", RegexOptions.IgnoreCase),
            own);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // Contract item 5, checked on the COMPOSED reader text so a figure
        // arriving through a shared block counts. This page's own sources are
        // full of median-survival figures and one of them is cited for a
        // different claim entirely, which is exactly why.
        var reader = CuratedPage.ReaderText(Composed);

        Assert.DoesNotMatch(new Regex(@"\d+\s*(%|percent)", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\b(five|ten|two|three)[- ]year (survival|relative survival)",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(
            new Regex(@"median (survival|overall survival)", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"(survive|survival)[^.]{0,40}\b(months|years)\b", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(reader));

        // No comparative prognosis either. The sources say IDH-mutant tumors
        // predict "a more favourable prognosis" than IDH-wildtype ones; that is
        // a prognosis claim about two groups and it stays off, even though it
        // is the comforting direction. The page says they were found to BEHAVE
        // differently, which is what justifies the rename.
        Assert.DoesNotMatch(
            new Regex(@"(better|worse|more favou?rable|less aggressive)\s+(prognosis|outlook)",
                RegexOptions.IgnoreCase),
            CuratedPage.Flatten(reader));
    }

    [Fact]
    public void TheOutlookGateIsClosedAndTeachesMedianRatherThanBanningTheWord()
    {
        var section = Section(OutlookHeading);

        Assert.Contains(":::outlook", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"middle of a group", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not a prediction about any one person", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not spread evenly", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"long tail of people who do far", RegexOptions.IgnoreCase), section);

        // The reason specific to THIS page, and it is a real one: the 2021
        // rules moved a whole population between diagnoses, so an older figure
        // may describe a group the reader is not in — in either direction.
        Assert.Matches(
            new Regex(@"moved a whole set of tumors out of the\s+glioblastoma group",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void NothingSitsOutsideTheOutlookGateInThatSection()
    {
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
        var headings = Regex.Matches(CuratedPage.Body(Page), @"^## (.+?)\s*$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        var expected = new[]
        {
            "The short version",
            WhatHeading,
            GradeHeading,
            LocationHeading,
            SymptomHeading,
            "How do doctors find out it is this?",
            ReportHeading,
            TreatmentHeading,
            "What is treatment actually like, and what is normal afterwards?",
            "Everyday life: work, driving, seizures and tiredness",
            "Follow-up scans, and what to do while you wait",
            RecurrenceHeading,
            "Did I cause this?",
            OutlookHeading,
            CaregiverHeading,
            "What to ask your team",
            "Where to get support",
        };

        Assert.Equal(expected, headings);
    }

    [Fact]
    public void OutlookSitsAfterEverythingActionableAndTheSelfBlameBlockIsDemoted()
    {
        var body = CuratedPage.Body(Page);

        var treatment = body.IndexOf($"## {TreatmentHeading}", StringComparison.Ordinal);
        var recurrence = body.IndexOf($"## {RecurrenceHeading}", StringComparison.Ordinal);
        var causes = body.IndexOf("## Did I cause this?", StringComparison.Ordinal);
        var outlook = body.IndexOf($"## {OutlookHeading}", StringComparison.Ordinal);
        var caregiver = body.IndexOf($"## {CaregiverHeading}", StringComparison.Ordinal);

        Assert.True(treatment < outlook, "outlook must come after treatment");
        Assert.True(recurrence < outlook, "outlook must come after 'if it comes back'");
        Assert.True(causes > 0, "the page has no self-blame section");
        Assert.True(causes < outlook, "the self-blame block belongs before the outlook gate");
        Assert.True(causes > body.Length / 2,
            "the self-blame block is near the top, which §12.3 explicitly rules out");
        Assert.True(outlook < caregiver, "the caregiver section must follow outlook");
    }

    [Fact]
    public void TheSharedBlocksEveryHubOwesAreIncluded()
    {
        Assert.Contains("[MECHANISM]", Section(LocationHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CaregiverHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);

        // WI-563. Pinned to the SECTION, not the page: the block is the
        // escalation list, and an [ESCALATION] that has drifted into the
        // support section at the bottom is a reader who meets the ambulance
        // tier four screens after the symptoms that trigger it.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);

        var slugLine = front.Split('\n').Select(l => l.TrimEnd('\r', ' ', '\t'))
            .FirstOrDefault(l => l.StartsWith("slug:", StringComparison.Ordinal));
        Assert.Equal("slug: tumors/astrocytoma", slugLine);

        var urls = Regex.Matches(front, @"^\s+- url:", RegexOptions.Multiline).Count;
        var accessed = Regex.Matches(front, @"^\s+accessed:", RegexOptions.Multiline).Count;
        Assert.True(urls > 0, "the page cites no sources");
        Assert.Equal(urls, accessed);
    }

    [Fact]
    public void TheSourcesThatCannotBeFetchedOrAreForbiddenNeverAppear()
    {
        // WI-516's own bad citation, plus the standing set. NEW THIS ITEM:
        //  - pubmed.ncbi.nlm.nih.gov/35562132 is the dossier's source for the
        //    ENTIRE circumscribed-astrocytic-glioma group (§4.1 item 2, §4.3,
        //    §4.5). It returns a 133-byte bot-block, and Europe PMC confirms it
        //    is not open access anywhere. CNS5 carries the material properly:
        //    "gliomas with more well-delineated borders separating them from
        //    the surrounding brain parenchyma", and grades pilocytic
        //    astrocytoma 1 with "good with complete resection".
        //  - ascopubs.org is cited by §4.5 for transformation and §4.6 for PCV;
        //    WI-511 proved it returns a JavaScript shell.
        // The rest are inherited and each is checked on the shared blocks too,
        // because block `sources` render in the reader's source list (§12.10).
        var surfaces = new List<(string Where, string Text)>
        {
            ("tumors/astrocytoma front matter", CuratedPage.FrontMatter(Page)),
        };

        foreach (var file in Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"))
        {
            surfaces.Add(($"blocks/{Path.GetFileName(file)}", File.ReadAllText(file)));
        }

        foreach (var bad in new[]
                 {
                     "pubmed.ncbi.nlm.nih.gov/35562132",
                     "ascopubs.org",
                     "ascopost.com",
                     "academic.oup.com",
                     "moffitt.org",
                     "mdpi.com",
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
    public void ThePageNeverMinimisesOrCharacterisesTheDiagnosis()
    {
        var reader = CuratedPage.ReaderText(Composed);

        CuratedPage.AssertNeverMinimises(reader, "tumors/astrocytoma");

        var flat = CuratedPage.Flatten(reader);
        foreach (var phrase in new[]
                 {
                     "the good kind", "nothing to worry about", "the best one to have",
                     "at least it is", "could be worse", "good news",
                 })
        {
            Assert.DoesNotContain(phrase, flat, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
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
/// WI-516 against the running site. §12.9: the gate is VERIFIED, not assumed,
/// "because every fail-open mode builds green".
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
public sealed class AstrocytomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/astrocytoma";

    private readonly WebApplicationFactory<Program> _factory;

    public AstrocytomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
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
                 { "[MECHANISM]", "[CROSSWALK]", "[CAUSES]", "[CAREGIVER]", "[TUMOR-BOARD]", "[ESCALATION]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        Assert.Contains("closed box", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("changed in 2021", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ThePagesNewVocabularyFiresAsTooltips()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "atrx", "tp53", "cdkn2a-b-deletion", "vorasidenib" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void EverySuppressedTermIsARealGlossaryEntryTheProseActuallyUses()
    {
        // Both halves, and asserted on the BODY only — a term appearing solely
        // in a citation title is not prose, and suppressing it suppresses
        // nothing (WI-515 shipped exactly that and this check caught it).
        var page = CuratedPage.Read("tumors", "astrocytoma.md");
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

    [Fact]
    public async Task TheHighGradeHubStillWarnsThatItsOtherDestinationsAreThin()
    {
        // The handover WI-515 built, checked from this side. That page carries
        // a SELF-REMOVING honesty note about its child pages, enforced by a
        // test that reads the destinations. Filling in astrocytoma does not
        // retire the note, because oligodendroglioma, glioblastoma and diffuse
        // midline glioma are still stubs — WI-517 and WI-518 own those.
        //
        // Asserted here so that whoever finishes the last of them sees BOTH
        // sides go red together, rather than discovering the note has quietly
        // become a lie.
        var stillThin = new[] { "oligodendroglioma", "glioblastoma", "diffuse-midline-glioma" }
            .Count(slug =>
                CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", slug + ".md")))
                    .Length < 4000);

        var html = await _factory.CreateClient().GetStringAsync("/tumors/high-grade-glioma");

        if (stillThin > 0)
        {
            Assert.Contains("short at the moment", html, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.DoesNotContain("short at the moment", html, StringComparison.OrdinalIgnoreCase);
        }

        // And this page is no longer one of the thin ones.
        Assert.True(
            CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "astrocytoma.md")))
                .Length >= 4000,
            "/tumors/astrocytoma is still short enough to count as a stub, so WI-516 has not "
            + "actually changed the handover it was written to change");
    }
}
