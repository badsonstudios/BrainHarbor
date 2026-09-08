using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-517: <c>/tumors/oligodendroglioma</c> — the hub whose LABEL and whose
/// TREATMENT come out of one biological fact, and the first Wave 2 page written
/// against the `[ESCALATION]` block rather than hand-copying it.
///
/// Seventeen sections per §12.3, written from §12.9 to §12.12.
///
/// **This is the page where §12.1's source-precedence rule bites hardest.** The
/// single richest source on this tumor is the StatPearls oligodendroglioma
/// chapter (NBK559184) — and §12.1 names that exact chapter as one never to use
/// for naming or grading, because it gives a correct molecular definition and
/// then lapses into Roman numerals and a retired term in the same article. It is
/// cited here for symptoms, imaging and treatment mechanics only. Naming and
/// grading rest on the CNS5-aligned sources, and the tests below check for the
/// vocabulary that would leak in if that line ever slipped.
///
/// Note this page is the ONE page in the corpus allowed to cite NBK559184.
/// `/tumors/astrocytoma` and `/tumors/high-grade-glioma` ban it in their own
/// front matter, correctly — there it was the wrong tumor's chapter cited for
/// general high-grade treatment. Right chapter, right claims, is a different
/// thing from right chapter, wrong claims.
/// </summary>
public sealed class OligodendrogliomaPageContentTests
{
    private static string Page => CuratedPage.Read("tumors", "oligodendroglioma.md");

    private static string Reader => CuratedPage.ReaderText(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, "tumors/oligodendroglioma");

    private const string WhatHeading = "What is an oligodendroglioma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string LocationHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string DiagnosisHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string OutlookHeading = "What might happen over time";
    private const string CaregiverHeading = "For the person caring for someone with this";

    [Fact]
    public void TheOpeningSaysBothHalvesOfTheNameAreGeneResults()
    {
        // The organising fact. A reader who does not understand that the name
        // is two lab results cannot understand why the wait is what it is, and
        // every other section on the page rests on it.
        var shortVersion = CuratedPage.Flatten(Section("The short version"));

        Assert.Matches(
            new Regex(@"[Bb]oth halves of this tumor's name are gene results", RegexOptions.IgnoreCase),
            shortVersion);

        // And the item's headline: label and treatment out of ONE fact. This is
        // the sentence the whole page exists to deliver.
        Assert.Matches(
            new Regex(@"name and the treatment come out of the same biological fact",
                RegexOptions.IgnoreCase),
            shortVersion);
    }

    [Fact]
    public void OneOfTheTwoResultsIsNotEnoughAndThePageSaysWhatTheOtherAnswerMeans()
    {
        // The commonest real confusion for this reader: they have an IDH result
        // and are waiting on 1p/19q, and nobody has told them the second result
        // decides WHICH diagnosis they have. Saying "both are required" without
        // saying what the other branch is leaves them no better off.
        var section = CuratedPage.Flatten(Section(WhatHeading));

        Assert.Matches(new Regex(@"[Oo]ne of the two is not enough", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"IDH change but no 1p/19q\s*co-deletion is an astrocytoma", RegexOptions.IgnoreCase),
            section);
        Assert.Contains("/tumors/astrocytoma", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGeneResultsOverruleTheMicroscopeAndThePageSaysSoInThatDirection()
    {
        // §5.4 of the dossier calls this "a genuine override of the
        // microscope", and it is the explanation for a real patient experience:
        // a team that sounded certain and then went careful about the name.
        //
        // The direction matters. "The gene results confirm what the microscope
        // saw" is the comfortable version and it is wrong: a textbook-looking
        // tumor with intact 1p/19q is NOT this diagnosis.
        var section = CuratedPage.Flatten(Section(WhatHeading));

        Assert.Matches(
            new Regex(@"looks like a textbook oligodendroglioma\s*under the microscope is \*\*not\*\* one if 1p/19q is intact",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"gene results\s*decide, and the appearance does not", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheCancerQuestionIsAnsweredInTheFirstSentenceOfItsSection()
    {
        // §12.6: answer first. The heading asks two questions and the cancer
        // one is the frightening one, so a section that opens by defining a
        // grade has answered the easy half.
        var sentences = CuratedPage.SentencesOf(Section(GradeHeading));

        Assert.Contains(
            sentences.Take(2),
            s => Regex.IsMatch(s, @"is cancer", RegexOptions.IgnoreCase));

        // "malignant" in the OPENING, not at a fixed index. The first version
        // asserted sentences[1], which only held because the "**" after
        // "cancer." suppressed the sentence split -- unbolding the line would
        // have broken the test for a reason unrelated to what it asserts.
        Assert.Contains(
            sentences.Take(3),
            s => Regex.IsMatch(s, @"\bmalignant\b", RegexOptions.IgnoreCase));
    }

    [Fact]
    public void TheHardCurabilitySentenceIsScopedToTheGradeItIsTrueOf()
    {
        // §12.11's first lesson and §12.12's direction rule, together. "Not
        // curable" is true of a grade 2 IDH-mutant tumor and is sourced to the
        // FDA vorasidenib review. Saying it of "this tumor" unscoped asserts it
        // of grade 3 as well without a source for that; saying nothing is the
        // over-reassuring direction, which §12.12 records as the worse one.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(
            new Regex(@"a grade 2 tumor of this kind is not\s*curable", RegexOptions.IgnoreCase),
            section);

        // Never the unscoped form.
        Assert.DoesNotMatch(
            new Regex(@"(it|this tumor|these tumors|an oligodendroglioma|oligodendrogliomas) (is|are) not curable",
                RegexOptions.IgnoreCase),
            section);

        // And grade 3 must not be left hanging. Scoping the hard sentence to
        // grade 2 and saying nothing about grade 3 leaves that reader the
        // implicature that theirs might be curable -- §12.12's more dangerous
        // direction, by omission.
        Assert.Matches(
            new Regex(@"true at grade 3 as well", RegexOptions.IgnoreCase), section);

        // §12.6: the CURABILITY PARAGRAPH must not end on the hard sentence.
        // Scoped to the text before the first subheading, not to the whole
        // section: the section has three "###" subsections under it, so
        // `sentences[^1]` came from "What the grade describes" and this check
        // could never fire. The break harness caught it -- deleting the
        // softening sentence outright left the test green.
        var lead = CuratedPage.Flatten(
            Section(GradeHeading).Split("###", StringSplitOptions.None)[0]);
        var leadSentences = CuratedPage.SentencesOf(lead);

        Assert.DoesNotMatch(
            new Regex(@"not curable\.?$", RegexOptions.IgnoreCase), leadSentences[^1]);
        Assert.Matches(
            new Regex(@"[Nn]ot curable is not the\s*same as not treatable", RegexOptions.IgnoreCase),
            lead);
        Assert.Matches(
            new Regex(@"treatments do real work", RegexOptions.IgnoreCase), lead);
    }

    [Fact]
    public void ThePageSaysThereIsNoGradeOneAndNoGradeFour()
    {
        // Verbatim in the CNS5-aligned source: "There is no oligodendroglioma
        // tumor corresponding to CNS WHO grade 4." Both halves matter — a
        // reader who has been told "grade 2" and has read the astrocytoma page
        // is bracing for a grade 4 that cannot happen here.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(
            new Regex(@"no such thing as a grade 1 oligodendroglioma", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"no oligodendroglioma that corresponds to grade 4", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"only comes in grade 2 and\s*grade 3", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCdkn2aResultIsScopedAgainstTheAstrocytomaPagesVersionOfIt()
    {
        // The cross-page trap. On /tumors/astrocytoma, CDKN2A/B homozygous
        // deletion sets grade 4 ON ITS OWN. Here the same result "may indicate
        // aggressive grade 3 behavior" and CANNOT set a grade 4, because there
        // is not one. A reader who has read both pages and is holding a
        // CDKN2A/B result will otherwise draw the astrocytoma conclusion.
        //
        // The sibling is READ (§12.10), so if the astrocytoma page ever stops
        // making the grade-4 claim, this page's contrast goes red rather than
        // quietly stale.
        var section = CuratedPage.Flatten(Section(GradeHeading));
        var astrocytoma = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "astrocytoma.md")));

        Assert.True(
            Regex.IsMatch(astrocytoma, @"CDKN2A/B", RegexOptions.IgnoreCase),
            "/tumors/astrocytoma no longer discusses CDKN2A/B, so the contrast this page "
            + "draws against it cannot be checked");

        Assert.Matches(new Regex(@"CDKN2A/B", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Ii]t does not create a grade 4", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void SeizuresAreTheHallmarkAndThePageNormalisesTheStoryTheReaderArrivedWith()
    {
        // The clinical signature. Most people with this diagnosis got here
        // because of a seizure, and being told that is the ordinary pattern is
        // worth more than the frequency figure that cannot be published.
        var section = CuratedPage.Flatten(Section(SymptomHeading));

        Assert.Matches(new Regex(@"[Ss]eizures are the hallmark", RegexOptions.IgnoreCase), section);

        // Direction, not an invented quantity. The source says 35% to 91%, and
        // 35% is not a large majority — §12.4 R2 gives a spread that wide
        // direction only. The first draft wrote "a large majority of cases" and
        // nothing looked for it.
        Assert.Matches(new Regex(@"commonest first sign", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(
            new Regex(@"(large |vast )?majority|most cases|nearly all|almost all",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"ordinary pattern for this\s*diagnosis rather than an unusual one",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo() =>
        // WI-563. The first hub written AGAINST the block rather than
        // hand-copying it, which is why WI-563 was slotted before this item.
        CuratedPage.AssertEscalationTiers(Page, "tumors/oligodendroglioma", SymptomHeading);

    [Fact]
    public void CalcificationIsExplainedRatherThanLeftOnTheReport()
    {
        // The dossier's note on this is right: patients see "calcification" on
        // a report and assume it is something they did. It is a distinctive
        // finding for this tumor, so it WILL be on their paperwork.
        var section = CuratedPage.Flatten(Section(DiagnosisHeading));

        Assert.Matches(new Regex(@"[Cc]alcification", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not something you did", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePcvAnswerIsTheOneBiologicalFactRatherThanATreatmentList()
    {
        // The item's whole reason for existing. "PCV is used for
        // oligodendroglioma" is the answer every comparator gives and it
        // explains nothing. The answer is that the co-deletion that NAMES the
        // tumor is the same thing that PREDICTS the drug response — which is
        // also the honest answer to "why are we waiting for one more test".
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(
            new Regex(@"not only the thing that names this tumor", RegexOptions.IgnoreCase), section);

        // The SOURCE's claim, not a stronger one. PMC9208578 says "prediction of
        // the best drug RESPONSE" -- how well the tumor responds. The first
        // draft of this page rendered that as "predicts which drugs it will
        // respond to best", which is drug SELECTION, and this test pinned the
        // overstatement word for word. The PCV-specific predictive claim belongs
        // to trials whose only source this page dropped as unreachable, so that
        // was WI-510's rule live on the page: the citation went, the claim
        // stayed.
        Assert.Matches(
            new Regex(@"unusually likely to respond to chemotherapy", RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"predicts which drugs?", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"label and the treatment are two consequences of the\s*same biological fact",
                RegexOptions.IgnoreCase),
            section);

        // And it must close the loop back to the wait, or the insight stays
        // abstract.
        Assert.Matches(
            new Regex(@"real reason everyone waits for that\s*test", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void PcvVersusTemozolomideIsPresentedAsOpenAndNotAsASettledAnswer()
    {
        // WI-511's somnolence shape and WI-512's carmustine shape, a third
        // time: where the evidence is genuinely unsettled, print the
        // disagreement rather than picking the comfortable side. The trial
        // designed to settle it has not reported.
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(
            new Regex(@"[Bb]oth are real options and the choice is not settled", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"temozolomide", RegexOptions.IgnoreCase), section);

        // Neither drug may be characterised as the better one outright.
        Assert.DoesNotMatch(
            new Regex(@"(PCV|[Tt]emozolomide) is (the )?(better|best|superior|preferred)",
                RegexOptions.IgnoreCase),
            section);

        // The hedge on the PCV evidence has to stay a hedge. "PCV does better"
        // is not what the source says.
        Assert.Matches(
            new Regex(@"suggests it may\s*do better", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheMissingCodeletionResultGetsAnActionAndNotJustAReassurance()
    {
        // §12.6: action at the end of a frightening block. Telling somebody
        // their report may be missing the result that names their diagnosis,
        // and stopping there, is the frightening half on its own. The action is
        // specific and it is usually available: stored tissue can often be
        // tested now.
        var section = CuratedPage.Flatten(Section(ReportHeading));

        Assert.Matches(
            new Regex(@"ask whether\s*the tissue already taken can be tested now",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"can change both the name of your diagnosis and the\s*treatment",
                RegexOptions.IgnoreCase),
            section);

        // And it must not blame the reader's own team for the gap.
        Assert.Matches(new Regex(@"not always a mistake", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void SeizureControlIsFramedAsATreatmentGoalRatherThanASideIssue()
    {
        // Dossier §5.6: seizure control is a treatment endpoint in its own
        // right for this tumor. The practical consequence for a reader is that
        // poorly controlled seizures are a reason to call, not something to
        // endure until the next appointment.
        var section = CuratedPage.Flatten(
            Section("What is treatment actually like, and what is normal afterwards?"));

        Assert.Matches(
            new Regex(@"[Ss]eizure control is part of the treatment, not a side issue",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"reason to go\s*back to your team", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheRadiationTimingTradeOffIsPublishedRatherThanHidden()
    {
        // The source is explicit that early versus delayed radiotherapy shows
        // no overall survival difference but does differ on time to progression
        // and seizure control. That is a real patient-facing choice, and a page
        // that presents the timing as fixed takes it away from them.
        var section = CuratedPage.Flatten(
            Section("What is treatment actually like, and what is normal afterwards?"));

        // SCOPED. Deferred radiotherapy is a select grade-2 decision; for grade
        // 3 the standard plan is treatment after surgery. The first draft
        // offered the whole "ask about waiting" conversation to both grades,
        // which is §12.12's over-reassuring direction with an action attached.
        Assert.Matches(
            new Regex(@"[Ii]f your tumor is grade 2, the timing of radiation is a real choice",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"[Ff]or a grade 3 tumor this is not the same conversation",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"has not been shown to change how long people\s*live", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"longer stretch before the tumor grows again", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"better seizure control after surgery", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheDrivingRuleCarriesNoDurationBecauseItIsJurisdictional()
    {
        // §12.3 section 9 is explicit: driving rules are jurisdictional, so a
        // per-tumor page must never carry a duration. This page is the one most
        // exposed to it — seizures are the hallmark and driving is the first
        // thing people ask about.
        var section = CuratedPage.Flatten(
            Section("Everyday life: work, driving, seizures and tiredness"));

        Assert.Matches(
            new Regex(@"[Dd]riving rules depend on where you live", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(
            new Regex(@"\b(six|three|twelve|6|3|12)\s*(months?|weeks?|years?)\b", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // §12.2 item 5 and §12.5. The vorasidenib FDA review in this page's
        // front matter publishes median survival figures; it is cited for the
        // curability claim and the drug's eligibility rules only.
        var reader = CuratedPage.ReaderText(Composed);

        // Proximity in BOTH directions. The first version required the number
        // BEFORE the word, so "the median survival is 14 years" -- the exact
        // shape the vorasidenib paper publishes, and the one the front-matter
        // comment worries about -- matched none of its patterns.
        Assert.DoesNotMatch(
            new Regex(@"\b(median|survival)\b[^.]{0,40}\d", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\d[^.]{0,40}\b(median|survival)\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"survival rate", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*%", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"five[- ]year", RegexOptions.IgnoreCase), reader);
    }

    [Fact]
    public void NoRomanNumeralGradeAndNoRetiredNameUsedAsALiveDiagnosis()
    {
        // Exactly what citing the StatPearls chapter risks importing: it uses
        // Roman numerals and a retired term in the same article that carries
        // the correct molecular definition. §12.1 is why naming and grading
        // come from elsewhere; this is the check that the line held.
        // The page's OWN text, not the composed page. [CROSSWALK] teaches the
        // old notation on purpose ("Roman numeral grades, such as grade II or
        // grade IV on an older report") and is covered by the site-wide Roman
        // guard's allowance. Running this check on the composed page asserts
        // that the block's teaching sentence is a defect, which it is not — the
        // first version did exactly that and failed on correct shared prose.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        // IgnoreCase. WI-516 found five copies of this guard that were all
        // case-sensitive and all stayed green on a planted "Grade III"; §12.12
        // wrote it down as the IgnoreCase trap's fourth occurrence. This copy
        // shipped without it anyway, one item later.
        Assert.DoesNotMatch(
            new Regex(@"[Gg]rade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase), reader);

        // The canary: prove the regex can fire at all. The crosswalk block
        // teaches the old notation on purpose, so it is the one place the
        // pattern MUST match -- without this, a regex that matches nothing
        // anywhere would look identical to a clean page.
        Assert.Matches(
            new Regex(@"[Gg]rade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(File.ReadAllText(
                Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md"))));

        // NEGATION-AWARE (§12.9). A retired name may be NAMED as retired — the
        // crosswalk requires it — and may never be used as a live diagnosis.
        foreach (var retired in new[] { "anaplastic oligodendroglioma", "oligoastrocytoma" })
        {
            foreach (Match hit in Regex.Matches(reader, Regex.Escape(retired), RegexOptions.IgnoreCase))
            {
                var window = reader[Math.Max(0, hit.Index - 200)..
                    Math.Min(reader.Length, hit.Index + retired.Length + 200)];
                Assert.True(
                    Regex.IsMatch(window,
                        @"retired|no longer|used to|older|eliminated|was called|2021|changed",
                        RegexOptions.IgnoreCase),
                    $"'{retired}' appears without anything marking it as a retired name: {window}");
            }
        }
    }

    [Fact]
    public void TheFrontMatterCitesTheCns5AlignedSourcesAndNotTheUnreachableOnes()
    {
        // The naming and grading claims have to trace to a CNS5-aligned source,
        // and the ones this page's dossier reaches for are dead or gated:
        //  - ascopubs.org, cited for the PCV predictive claim, returns 403 with
        //    a 5,620-byte shell (WI-511 found it, WI-515 re-confirmed, this is
        //    the third item). PMC9208578 carries the claim properly.
        //  - academic.oup.com 403s (WI-509, WI-515).
        //  - PMC10475770 is cited by the dossier for "transformation is not
        //    associated with worse outcomes in oligodendrogliomas". Fetched, the
        //    paper is about transformation PATTERNS and says no such thing. The
        //    claim is not on this page.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("cco.amegroups.org", front, StringComparison.Ordinal);
        Assert.Contains("PMC9723092", front, StringComparison.Ordinal);
        Assert.Contains("PMC9208578", front, StringComparison.Ordinal);

        // §12.11's closing rule: fetch every URL in EVERYTHING the item ships.
        // WI-515's miss was a glossary entry, and this item shipped two of them.
        // Front matter alone is not the citation surface -- glossary entries and
        // shared blocks render in the reader's source list too.
        var surfaces = new List<(string Where, string Text)>
        {
            ("tumors/oligodendroglioma front matter", front),
        };
        foreach (var (slug, text) in CuratedPage.SharedSources())
        {
            surfaces.Add((slug, text));
        }

        foreach (var bad in new[]
                 {
                     "ascopubs.org", "academic.oup.com", "PMC10475770",
                     "moffitt.org", "mdpi.com", "drugs.com",
                     "cancer.gov/types/brain/patient", "cancer.gov/types/brain/hp",
                 })
        {
            foreach (var (where, text) in surfaces)
            {
                Assert.False(
                    text.Contains(bad, StringComparison.OrdinalIgnoreCase),
                    $"{bad} is cited in {where} — it is unreachable, banned by §12.1, "
                    + "or does not carry the claim it was cited for");
            }
        }

        // And the claim the dossier hung on PMC10475770 must not have survived
        // its citation being dropped (WI-510's rule: deleting a bad citation
        // does not delete the claim).
        Assert.DoesNotMatch(
            new Regex(@"transformation is not associated with|unlike astrocytomas, (a )?transformation",
                RegexOptions.IgnoreCase),
            CuratedPage.Flatten(Reader));
    }

    [Fact]
    public void ThePageCarriesItsOwnRetiredNameSliceAndNotJustTheSharedBlock()
    {
        // §12.2 item 3 and §12.9: every hub owes a slice naming the retired
        // terms ITS OWN readers are holding. The shared [CROSSWALK] block
        // carries only the universal three (Roman numerals, gene results in the
        // name, NOS/NEC) and names no glioma at all.
        //
        // This hub owes it more than any other. Oligoastrocytoma was not
        // renamed, it was ELIMINATED — and the thing that eliminated it is the
        // 1p/19q test this page spends its length explaining. The first draft
        // said neither word, while printing "Oligoastrocytoma" in a visible
        // source title under Sources, and while /tumors/high-grade-glioma
        // already explained "anaplastic oligodendroglioma" and routed here.
        //
        // These POSITIVE assertions are the other half of
        // NoRomanNumeralGradeAndNoRetiredNameUsedAsALiveDiagnosis, whose
        // negation loop iterates over matches — on a page with no matches it
        // ran zero times and passed. A test that cannot tell "correctly marked
        // as retired" from "absent" is not a test (§12.10).
        var section = CuratedPage.Flatten(Section(ReportHeading));

        // Each name must be the SUBJECT OF ITS OWN BULLET, not merely present
        // somewhere in the section. The break harness deleted the
        // oligoastrocytoma bullet and this passed anyway, because the word
        // survives in the closing paragraph -- presence was never the property
        // (WI-512).
        Assert.Matches(
            new Regex(@"- \*\*Anaplastic oligodendroglioma\.\*\*", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"- \*\*Oligoastrocytoma\*\*", RegexOptions.IgnoreCase), section);

        // The angle, not a copy (§12.11): the two names are retired for
        // DIFFERENT reasons, and the second one is this page's own thesis.
        Assert.Matches(
            new Regex(@"[Tt]his one did not\s*get renamed", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"it was \*\*eliminated\*\*", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"1p/19q test settles it", RegexOptions.IgnoreCase), section);

        // And it must not duplicate what the shared block already says.
        var crosswalk = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md")));
        foreach (var universal in new[] { "Roman numeral grades", "NOS means", "NEC means" })
        {
            Assert.Contains(universal, crosswalk, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(universal, CuratedPage.Flatten(Page), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheLocationAnswerComesBeforeTheSharedMechanismBlock()
    {
        // §12.11: a heading with two questions in it owes two answers, and the
        // "where" has to be on the page BEFORE the block, for the reader who
        // stops after the first paragraph. The first draft put [MECHANISM] —
        // some seven hundred words about closed skulls and fluid — ahead of the
        // word "frontal", so the answer to the heading's first question came
        // after it. The sibling has this test; it was not copied across.
        var section = Section(LocationHeading);

        var mechanism = section.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        var where = section.IndexOf("frontal", StringComparison.OrdinalIgnoreCase);

        Assert.True(where > 0, "the location section never names where this tumor grows");
        Assert.True(
            where < mechanism,
            "the shared [MECHANISM] block comes before this page's own answer to "
            + "'where does it grow' — §12.11");
    }

    [Fact]
    public void TheSharedBlocksEveryHubOwesAreIncluded()
    {
        Assert.Contains("[MECHANISM]", Section(LocationHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CaregiverHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);

        // WI-563, and pinned to the SECTION: an [ESCALATION] that drifts to the
        // bottom of the page is a reader who meets the ambulance tier four
        // screens after the symptoms that trigger it.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheOutlookGateHoldsTheRightTailAndTeachesTheVocabulary()
    {
        // §12.5, and WI-513's lesson: the most hope-preserving sentence belongs
        // INSIDE the gate, and a gate that publishes no figures still has to
        // teach the words. The comparative claim about this tumor being the
        // slower-moving and most chemosensitive of the diffuse gliomas is
        // exactly the sentence a reader who declined outlook must not meet
        // anyway.
        var section = CuratedPage.Flatten(Section(OutlookHeading));

        Assert.Contains(":::outlook", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not a prediction about any one person", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"long tail", RegexOptions.IgnoreCase), section);

        var gate = Regex.Match(section, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");

        // The comparative sits inside the fence, not outside it -- and it is a
        // COMPARATIVE, not a superlative. The source says "relative
        // chemosensitivity and indolent clinical course among diffuse gliomas";
        // the first draft turned that into "the slower-moving of them" and "the
        // one that responds best", ranking this tumor first on two axes that no
        // source ranks. This test pinned that wording.
        Assert.Matches(
            new Regex(@"described as\s*slower-growing, and as more likely to respond to chemotherapy",
                RegexOptions.IgnoreCase),
            gate.Groups[1].Value);
        Assert.DoesNotMatch(
            new Regex(@"the one that responds best|slower-moving of them", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(Composed)));

        // And it is framed as a comparison between diagnoses, not a promise.
        Assert.Matches(
            new Regex(@"comparison\s*between diagnoses, not a promise about a person",
                RegexOptions.IgnoreCase),
            gate.Groups[1].Value);
    }

    [Fact]
    public void TheOutlookMaterialIsNotAlsoSittingOutsideTheGate()
    {
        // WI-503 documents five ways a mistyped fence publishes gated content
        // wide open, and every one of them builds green. The words must appear
        // inside the fence and nowhere else on the page.
        // The WHOLE page minus the gate, not the outlook section minus the
        // gate. WI-513's actual defect was the right-tail sentence sitting in
        // section 2 -- four screens ABOVE the gate -- where a section-scoped
        // check cannot see it.
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        var gate = Regex.Match(whole, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");
        var outside = whole.Replace(gate.Value, "", StringComparison.Ordinal);

        Assert.DoesNotMatch(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), outside);
        Assert.DoesNotMatch(new Regex(@"long tail", RegexOptions.IgnoreCase), outside);
        Assert.DoesNotMatch(
            new Regex(@"more likely to respond to chemotherapy", RegexOptions.IgnoreCase), outside);
        Assert.DoesNotMatch(new Regex(@"slower-growing", RegexOptions.IgnoreCase), outside);
    }

    [Fact]
    public void TheCaregiverSectionNamesTheSeizureBurdenAndTheFrontalLobeChanges()
    {
        // The two things people caring for someone with THIS diagnosis report,
        // and neither is in the shared block: the seizures are harder to live
        // with than the tumor, and the personality changes come from the tumor
        // rather than from the person.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        Assert.Contains("[CAREGIVER]", section, StringComparison.Ordinal);

        // Second person, no attribution. The first draft wrote "the thing
        // people caring for someone with this diagnosis MOST OFTEN SAY", which
        // is a quantified claim about a population that no source in this
        // page's front matter, the caregiver block's six sources, or either
        // dossier makes. §12.7 lets the caregiver material break second person
        // exactly where it reports a study finding — so borrowing that voice
        // with no study behind it is a fabricated citation with the citation
        // left off.
        Assert.Matches(
            new Regex(@"the seizures turn out to be the harder\s*part of daily life",
                RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"most often say|most people say|caregivers report", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"not the person choosing to be different", RegexOptions.IgnoreCase), section);
        Assert.Contains("/seizures/what-to-do", section, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        CuratedPage.AssertNeverMinimises(
            CuratedPage.ReaderText(Composed), "tumors/oligodendroglioma");

        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // WI-563's strip-then-scan form, not the weaker skip.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // BritishForms holds SPELLINGS, and the thing that actually misleads a
        // US reader is IDIOM. §12.10 names these; the escalation block composed
        // onto this page already uses the US forms, so a page carrying them
        // contradicts its own block. The corpus-wide sweep is WI-564; until
        // then each page defends itself. The break harness found this — putting
        // "out of hours" back passed every existing guard.
        foreach (var idiom in new[]
                 {
                     "out of hours", "advice line", "A&E", "being sick", "come round",
                 })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheSectionsRunInTheOrderTheStandardSets()
    {
        // §12.3. Order, not membership — WI-513's ordering test used set
        // membership on the one page whose whole purpose was fixing the shape.
        var order = new[]
        {
            "The short version", WhatHeading, GradeHeading, LocationHeading, SymptomHeading,
            DiagnosisHeading, ReportHeading, TreatmentHeading,
            "What is treatment actually like, and what is normal afterwards?",
            "Everyday life: work, driving, seizures and tiredness",
            "Follow-up scans, and what to do while you wait",
            "If it comes back, or changes", "Did I cause this?", OutlookHeading,
            CaregiverHeading, "What to ask your team", "Where to get support",
        };

        // "\n## ", not "## ": IndexOf("## X") matches inside "### X" at offset
        // one, so demoting any section to H3 left this green while the page
        // outline broke -- and the outline is load-bearing for §12.5.
        var positions = order
            .Select(h => (Heading: h, At: Page.IndexOf($"\n## {h}", StringComparison.Ordinal)))
            .ToArray();

        foreach (var (heading, at) in positions)
        {
            Assert.True(at > 0, $"the page has no '## {heading}' section");
        }

        for (var i = 1; i < positions.Length; i++)
        {
            Assert.True(
                positions[i].At > positions[i - 1].At,
                $"'{positions[i].Heading}' must come after '{positions[i - 1].Heading}'");
        }

        // The outlook gate comes after everything actionable and before the
        // caregiver section (§12.3).
        Assert.True(
            Page.IndexOf($"\n## {OutlookHeading}", StringComparison.Ordinal)
            < Page.IndexOf($"\n## {CaregiverHeading}", StringComparison.Ordinal));
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);
        Assert.Contains("slug: tumors/oligodendroglioma", front, StringComparison.Ordinal);
    }
}

/// <summary>
/// The render half. <c>[Collection(DatabaseCollection.Name)]</c> is NOT
/// optional: every render class in the suite carries it, and it is what
/// serializes them against the one dev database. The first version of this file
/// omitted it, and the full suite then failed a DIFFERENT render test on each
/// run — DbUp migrating under a second host, or Kestrel not starting — while
/// every one of them passed in isolation. A suite that fails somewhere else
/// each time it runs is not flaky, it is contended.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class OligodendrogliomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/oligodendroglioma";

    private readonly WebApplicationFactory<Program> _factory;

    public OligodendrogliomaPageRenderTests(WebApplicationFactory<Program> factory) =>
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
            // The required list is what must appear in the ONWARD section, not
            // what must appear on the page — the helper scopes to the section
            // id. The body links (glioma, astrocytoma, the two libraries) are
            // proved by the content tests that assert them section by section.
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/seizures/what-to-do", "/seizures/living-with");

    [Fact]
    public async Task EveryFragmentLinkLandsOnAnAnchorThatExists() =>
        // WI-513: AssertLinksResolve's regex stops at the '#', so a deep link
        // to an anchor that does not exist resolves as a healthy 200 and drops
        // the reader at the top of a long page.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task EveryBlockDirectiveComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var directive in new[]
                 { "[MECHANISM]", "[CROSSWALK]", "[CAUSES]", "[CAREGIVER]", "[TUMOR-BOARD]", "[ESCALATION]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // One canary word PER BLOCK. The first version checked two of six, so
        // emptying caregiver.md, crosswalk.md, causes.md or tumor-board.md left
        // every test in this file green -- the directive disappears from the
        // HTML either way.
        foreach (var canary in new[]
                 {
                     "closed box",                    // mechanism
                     "Call your team the same day",   // escalation
                     "Roman numeral grades",          // crosswalk
                     "nobody knows the cause",        // causes
                     "tumor board",                   // tumor-board
                     "caring for someone",            // caregiver
                 })
        {
            Assert.Contains(canary, html, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task TheOutlookGateRendersClosedAndLeaksNoFence()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.DoesNotContain(":::", html, StringComparison.Ordinal);
        Assert.Contains("<details", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"<details[^>]*\bopen\b"), html);

        // The heading is outside the gate so a reader can decline without
        // opening it.
        Assert.Contains("What might happen over time", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePagesNewVocabularyFiresAsTooltips()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "pcv", "temozolomide", "vorasidenib", "1p-19q-co-deletion" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }

        // The two words this page DEFINES itself are suppressed here and fire
        // everywhere else (WI-509).
        Assert.DoesNotContain("def-oligodendrocyte", html, StringComparison.Ordinal);
        Assert.DoesNotContain("def-calcification", html, StringComparison.Ordinal);
    }
}
