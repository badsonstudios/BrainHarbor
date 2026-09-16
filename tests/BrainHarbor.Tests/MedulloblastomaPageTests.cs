using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-537: <c>/tumors/medulloblastoma</c>, the stub rewritten as a full §12.3 hub.
///
/// **The safety decisions this page makes.** (1) It includes [MECHANISM] and
/// [ESCALATION] together, and [SPINAL-CORD] straight after, because this tumor
/// spreads through the fluid to the spine — which is the reader the block's "or has
/// spread to the spine" clause names. (2) Every same-day line the page writes itself
/// says what changes for a reader with a shunt (WI-535's blocker). (3) Posterior
/// fossa syndrome is the shared block, so a sudden loss of speech keeps the
/// [ESCALATION] block's AMBULANCE tier rather than this page's weaker one (WI-536's
/// round-2 blocker); the block's own guards are
/// <see cref="PosteriorFossaSyndromeBlockTests"/>.
///
/// **The honesty decisions.** The grade section leads with "yes, it is cancer" and
/// then prints WHO's own warning that calling this tumor grade 4 "potentially risks
/// giving a false sense of prognosis" — the one place in the corpus where the
/// grading authority argues against its own number. Histology and molecular group
/// are TWO AXES on one report, not one list. The CSF timing rule is printed with the
/// disagreement inside a single national standard. No prognosis figures, and no
/// Roman numerals: NCI-CONNECT writes "grade IV" and is not followed for it.
/// </summary>
public sealed class MedulloblastomaPageContentTests
{
    private const string Slug = "tumors/medulloblastoma";

    private static string Page => CuratedPage.Read("tumors", "medulloblastoma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string RawLf => Page.Replace("\r\n", "\n");

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a medulloblastoma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string LocationHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string FindHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string LifeHeading = "Everyday life: school, hearing, growth and tiredness";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string ComesBackHeading = "If it comes back, or changes";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CaregiverHeading = "For the person caring for someone with this";

    private static readonly Regex CureFamily =
        new(@"\b(cure[sd]?|curable|incurable|curative)\b", RegexOptions.IgnoreCase);

    /// <summary>Reader text with markdown heading lines removed, so a heading never merges into a sentence (WI-535).</summary>
    private static string[] PageSentences() =>
        CuratedPage.SentencesOf(CuratedPage.Flatten(Regex.Replace(
            CuratedPage.ReaderText(Page), @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline)));

    /// <summary>The raw (LF) text of one `##` section, heading included.</summary>
    private static string SectionRawLf(string heading)
    {
        var raw = RawLf;
        var start = raw.IndexOf($"\n## {heading}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"the page has no '## {heading}' section");
        var end = raw.IndexOf("\n## ", start + 4, StringComparison.Ordinal);
        return end < 0 ? raw[start..] : raw[start..end];
    }

    /// <summary>The blank-line-bounded paragraph inside <paramref name="heading"/> that starts with <paramref name="opening"/>.</summary>
    private static string ParagraphIn(string heading, string opening)
    {
        var raw = SectionRawLf(heading);
        var at = raw.IndexOf(opening, StringComparison.Ordinal);
        Assert.True(at >= 0, $"no paragraph in '{heading}' starts with '{opening}'");
        var end = raw.IndexOf("\n\n", at, StringComparison.Ordinal);
        return CuratedPage.Flatten(end < 0 ? raw[at..] : raw[at..end]);
    }

    /// <summary>The COMPOSED text of one `###` subsection. Shared (WI-537, /review round 1).</summary>
    private static string ComposedSubsectionLf(string heading) =>
        CuratedPage.ComposedSubsection(Page, heading);

    [Fact]
    public void TheSectionsRunInTheOrderTheStandardSets()
    {
        var order = new[]
        {
            ShortHeading, WhatHeading, GradeHeading, LocationHeading, SymptomHeading,
            FindHeading, ReportHeading, TreatmentHeading, AfterHeading, LifeHeading,
            ScansHeading, ComesBackHeading, CauseHeading, OutlookHeading, CaregiverHeading,
            "What to ask your team", "Where to get support",
        };

        var positions = order
            .Select(h => (Heading: h, At: Page.IndexOf($"\n## {h}", StringComparison.Ordinal)))
            .ToArray();

        foreach (var (heading, at) in positions)
        {
            Assert.True(at > 0, $"the page has no '## {heading}' section");
        }

        for (var i = 1; i < positions.Length; i++)
        {
            Assert.True(positions[i].At > positions[i - 1].At,
                $"'{positions[i].Heading}' must come after '{positions[i - 1].Heading}'");
        }
    }

    [Fact]
    public void TheSharedBlocksEveryHubOwesAreIncluded()
    {
        // The stub carried [CAREGIVER] alone and cited NCI's patient page for naming.
        // [MECHANISM] and [ESCALATION] come together (EscalationBlockTests: a hub routed
        // to /treatments/shunts through [MECHANISM] owes the shunt rule).
        Assert.Contains("[MECHANISM]", Section(LocationHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[SPINAL-CORD]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CaregiverHeading), StringComparison.Ordinal);
        Assert.Contains("[POSTERIOR-FOSSA-SYNDROME]", Section(AfterHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheLocationAnswerAndTheFluidRouteComeBeforeTheSharedMechanismBlock()
    {
        var section = CuratedPage.Flatten(Section(LocationHeading));
        var block = section.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(block > 0, "the section has lost [MECHANISM]");

        // Why this tumor's symptoms are the ones they are: the fluid backing up, and the
        // fluid as the route it spreads along. Both are this page's own answer and both
        // belong before the shared block that generalises (§12.9).
        foreach (var owed in new[]
                 {
                     "most of the early symptoms come from that fluid backing up",
                     "**It can also travel in that fluid.**",
                     "That is why the whole spine is scanned and the fluid is tested",
                 })
        {
            var at = section.IndexOf(owed, StringComparison.Ordinal);
            Assert.True(at >= 0 && at < block, $"'{owed}' is not said before [MECHANISM]");
        }
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void TheCancerQuestionIsAnsweredBeforeAnythingAboutTheGrade()
    {
        // §12.3's documented core confusion, and this tumor is the corpus's clearest
        // case: it is grade 4 for every type, so a reader who meets the number first
        // meets the most frightening fact on the page with no answer beside it.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.StartsWith("**Yes. A medulloblastoma is cancer.**", section, StringComparison.Ordinal);
        Assert.Contains("The word your team will use is **malignant**", section, StringComparison.Ordinal);

        var answer = section.IndexOf("is cancer", StringComparison.Ordinal);
        var grade = section.IndexOf("grade 4", StringComparison.Ordinal);
        Assert.True(grade > answer, "the grade must not be named before the cancer question is answered");
    }

    [Fact]
    public void TheGradeSectionCarriesWhosOwnWarningAgainstReadingItsOwnNumber()
    {
        // The anchor for the whole section, and the reason it is not a generic
        // "grades are not comparable" line: WHO CNS5 says designating WNT-activated
        // medulloblastoma grade 4, "therefore equivalent to many untreatable pediatric
        // brain tumors with a dismal outcome, potentially risks giving a false sense of
        // prognosis when therapeutic options are discussed in the clinic."
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Contains("**It is grade 4, and every medulloblastoma is.**", section, StringComparison.Ordinal);
        Assert.Contains("**Grade 4 does not mean the same thing for every tumor.**", section, StringComparison.Ordinal);
        Assert.Contains("The World Health Organization says so about this tumor in particular",
            section, StringComparison.Ordinal);
        Assert.Contains("risks giving families a false picture when treatment is discussed",
            section, StringComparison.Ordinal);

        // /review round 2: WHO's reason, given as an argument about the GRADE rather than
        // about how a group does. Round 1 replaced "Most children with that kind do well
        // with today's treatment" with "because that kind responds to today's treatment",
        // which was the same ungated group-level outcome claim with the adverb removed,
        // and which slipped past the "responds well"/"responds best" bans in the very
        // list round 1 had just extended (§12.14: a fix can recreate the defect). The
        // reason now rests on a fact the page already states and CNS5 carries: every
        // medulloblastoma is grade 4, so the number cannot be about one child.
        Assert.Contains("Every medulloblastoma is grade 4 whatever its kind, so the number cannot tell you how one child will do",
            section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"responds? (well|best|better|to today)", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(Page)));

        // Grade 4 here sorts the TYPE, not one child's tumor from another's, which is what
        // makes "every medulloblastoma is grade 4" survivable to read.
        //
        // /review round 3: this said "describes the tumor's KIND", and "kind" means
        // molecular group everywhere else on this page ("the kind changes the treatment
        // plan", "which kind the tumor is decides who gets offered testing", "the kind
        // matters a great deal"). Read that way it claimed the grade tells you the group,
        // which the section denies six lines later, and both sentences were pinned, so the
        // collision was enforced. This is the section a frightened parent reads first.
        Assert.Contains("describes this type of tumor as a whole rather than sorting one child's tumor from another's",
            section, StringComparison.Ordinal);

        // And the section lands on what actually does the work (§12.6): the action is
        // reading the report, not absorbing the number.
        Assert.Contains("**The kind, and whether it has spread, matter more.**", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGliomaHubsCrossTumorGradeClaimIsTheSameClaimAtTheSameStrength()
    {
        // §12.10, read from the sibling rather than hard-coded. /tumors/glioma already
        // teaches that a grade 4 of one tumor type does not mean a grade 4 of another,
        // and WHO's own worked example for that rule IS medulloblastoma
        // (GliomaPageTests cites it). Two pages, one claim: they must not drift to two
        // strengths, and this page must not contradict the hub that teaches the rule.
        var glioma = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "glioma.md")));

        Assert.Contains("**Grade is not the same across different tumors.**", glioma, StringComparison.Ordinal);
        Assert.Contains("A grade 4 of one tumor type does not mean the same thing as a grade 4 of another",
            glioma, StringComparison.Ordinal);

        // This page states it of itself, which is the specific case of the general rule.
        Assert.Contains("**Grade 4 does not mean the same thing for every tumor.**",
            CuratedPage.Flatten(Section(GradeHeading)), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRetiredNameAppearsOnlyAsRetiredAndTheTrialNamesAreNotDiagnoses()
    {
        // CRUK: "Doctors used to talk about medulloblastoma as a primitive neuro
        // ectodermal tumour (PNET). This term is no longer used." The naming trap is
        // that the term is still LIVE in the SEOM adult guideline, an adult review and
        // GeneReviews — none of which is followed here (NOTES.md trap 2).
        var retired = CuratedPage.Flatten(SectionRawLf(WhatHeading));

        Assert.Contains("Doctors used to call this tumor a **primitive neuroectodermal tumor**, or **PNET**",
            retired, StringComparison.Ordinal);
        Assert.Contains("That name is no longer used", retired, StringComparison.Ordinal);

        // The other half of the trap, and the one a reader actually hits: PNET5/4/3 are
        // TRIAL names. A parent holding a trial leaflet reads them as the old diagnosis.
        Assert.Contains("**PNET5**, **PNET4** or **PNET3**", retired, StringComparison.Ordinal);
        Assert.Contains("Those are names of research studies, not names of the tumor",
            retired, StringComparison.Ordinal);

        // Negation-aware (§12.9, WI-513): the retired name is named as retired here and
        // appears nowhere else on the page as a live diagnosis.
        var outside = CuratedPage.Flatten(CuratedPage.ReaderText(Page))
            .Replace(retired, "", StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\bPNET\b(?!\d)"), outside);
        Assert.DoesNotMatch(new Regex(@"primitive neuroectodermal", RegexOptions.IgnoreCase), outside);

        // NCI-CONNECT writes "grade 4 (also written as grade IV)". WHO CNS5 says Roman
        // numerals belong to "past classifications", so the page uses none.
        var roman = new Regex(@"\bgrade\s+(I|II|III|IV)\b", RegexOptions.IgnoreCase);
        Assert.Matches(roman, "an older report said grade IV"); // canary: the guard can fire
        Assert.DoesNotMatch(roman, CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheReportSectionKeepsHistologyAndMolecularGroupAsTwoAxes()
    {
        // NOTES.md traps 3 and 4: CRUK calls the molecular groups "subtypes" and
        // misnumbers them, and lists the histology patterns as "4 types" beside them as
        // if they were one axis. CNS5 combines them into ONE layered diagnosis, and a
        // reader holding a report with a word from each list needs to know both belong.
        var report = CuratedPage.Flatten(Section(ReportHeading));

        Assert.Contains("**This report has two parts, and both matter.**", report, StringComparison.Ordinal);
        Assert.Contains("One says how the cells look", report, StringComparison.Ordinal);
        Assert.Contains("which molecular group the tumor is in", report, StringComparison.Ordinal);
        Assert.Contains("Your team puts them together into one diagnosis", report, StringComparison.Ordinal);

        // The four histology patterns, and the four molecular groups, each complete.
        foreach (var pattern in new[] { "**Classic.**", "**Desmoplastic or nodular.**", "**With extensive nodularity**", "**Large cell, or anaplastic.**" })
        {
            Assert.Contains(pattern, report, StringComparison.Ordinal);
        }

        foreach (var group in new[] { "**WNT-activated.**", "**SHH-activated, TP53-wildtype.**", "**SHH-activated, TP53-mutant.**", "**Non-WNT, non-SHH.**" })
        {
            Assert.Contains(group, report, StringComparison.Ordinal);
        }

        // /review round 1 BLOCKER: the draft called group 3 and group 4 RETIRED names.
        // They are current CNS5 names, which this page itself said 35 lines earlier in
        // the present tense ("the test that tells groups 3 and 4 apart"). WHO CNS5 lists
        // "Medulloblastomas, non-WNT/non-SHH (Groups 3 and 4)" as a type, and St. Jude
        // writes "Group 3 tumors are the second most common". A parent holding a report
        // that says Group 4 was being told, in the section for reading the report, that
        // the label was obsolete.
        Assert.Contains("Inside this one, your report may say **group 3** or **group 4**",
            report, StringComparison.Ordinal);
        Assert.Contains("Both names are still used", report, StringComparison.Ordinal);
        Assert.DoesNotContain("used to be called group 3", report, StringComparison.Ordinal);

        // What the group is FOR, which the section never said (/review round 1). St. Jude:
        // molecular testing classifies the tumor into subgroups "and to plan treatments".
        Assert.Contains("**Which group it is in helps decide how much treatment is given, and which trials fit.**",
            report, StringComparison.Ordinal);

        // How the two axes line up, at the strength CNS5 states it ("Nearly all WNT
        // tumors have classic morphology"; D/N and MBEN "align with the SHH molecular
        // group") — a correlation, not a rule.
        Assert.Contains("The two parts line up more often than not", report, StringComparison.Ordinal);
        Assert.Contains("Nearly all WNT tumors look classic", report, StringComparison.Ordinal);

        // The 14 subgroups, and that they are mostly research (NOTES.md trap 5: ABTA's
        // "12 subtypes" is the stale count).
        Assert.Contains("finer groupings, fourteen of them, used mostly in research",
            report, StringComparison.Ordinal);

        // The page must not call the molecular groups "subtypes" (CRUK's word), because
        // in CNS5 "subgroup" means the 14 methylation classes below them.
        Assert.DoesNotMatch(new Regex(@"\bsubtype", RegexOptions.IgnoreCase), report);
    }

    [Fact]
    public void TheMethylationTestIsNamedAsTheThingThatTellsGroupsThreeAndFourApart()
    {
        // CNS5's recommended method, and the reason a report can take weeks: IHC cannot
        // separate the non-WNT/non-SHH groups, and methylation profiling is the answer.
        var find = CuratedPage.Flatten(Section(FindHeading));

        Assert.Contains("One test reads a chemical pattern across the tumor's DNA", find, StringComparison.Ordinal);
        Assert.Contains("it is the test that tells groups 3 and 4 apart", find, StringComparison.Ordinal);
        Assert.Contains("the kind changes the treatment plan", find, StringComparison.Ordinal);
    }

    [Fact]
    public void StagingChecksTheWholeSpineAndPrintsTheCsfTimingRuleWithItsDisagreement()
    {
        // The item's headline finding. Three independent sources say 14 days or more;
        // the Canadian standard says "at least 14 days" in its workup list and "10-14
        // days" in its risk definition — one document, two numbers. Printing the spread
        // is the §12.8 rule for a disagreement nobody can resolve from the outside.
        var find = CuratedPage.Flatten(Section(FindHeading));

        Assert.Contains("**[An MRI scan](/tests/mri) of the brain, and one of the whole spine.**",
            find, StringComparison.Ordinal);

        // Why the spine scan wants to happen FIRST: post-operative artefacts mimic
        // metastases, and a false positive escalates treatment.
        Assert.Contains("The spine scan is best done before the operation", find, StringComparison.Ordinal);
        Assert.Contains("the pictures can show marks that look like spread and are not",
            find, StringComparison.Ordinal);

        // The wait, and BOTH reasons for it. The pre-operative half is a safety fact
        // (herniation risk), not a scheduling one.
        Assert.Contains("**The fluid test waits, and that wait is deliberate.**", find, StringComparison.Ordinal);
        Assert.Contains("It is not done before surgery, because the pressure in the head makes it unsafe",
            find, StringComparison.Ordinal);
        Assert.Contains("blood in the fluid makes the result hard to read", find, StringComparison.Ordinal);
        Assert.Contains("about two weeks later", find, StringComparison.Ordinal);

        // The disagreement, attributed to the one document that contains both numbers.
        Assert.Contains("one national standard says at least 14 days in one place and 10 to 14 days in another",
            find, StringComparison.Ordinal);
        Assert.Contains("Ask your team when yours is planned", find, StringComparison.Ordinal);

        // What the answer CHANGES, which is the part that makes the wait bearable.
        Assert.Contains("put the tumor in the higher risk group", find, StringComparison.Ordinal);
        Assert.Contains("Your team may call this staging, or use an M and a number",
            find, StringComparison.Ordinal);
    }

    [Fact]
    public void CraniospinalRadiationIsExplainedByWhyRatherThanNamedAndLeftThere()
    {
        // The backlog asked for craniospinal radiation by name. The reader's question is
        // not what it is called but why the whole brain and spine are treated when the
        // tumor is in one place — and the answer is this tumor's own biology.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("**[Radiation](/treatments/radiation-therapy) to the whole brain and spine**",
            treatment, StringComparison.Ordinal);
        Assert.Contains("with an extra dose aimed at the place the tumor was", treatment, StringComparison.Ordinal);
        Assert.Contains("this tumor travels in the fluid, so the treatment follows the fluid",
            treatment, StringComparison.Ordinal);
        Assert.Contains("Your team may call it craniospinal radiation", treatment, StringComparison.Ordinal);

        // §12.4 R1: no Gy figures, on a page whose sources are full of them.
        Assert.DoesNotMatch(new Regex(@"\bGy\b|\bgray\b", RegexOptions.IgnoreCase),
            CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheRadiationAgeFloorIsAttributedAndSaidToBeMoving()
    {
        // Two sources, two framings, and a threshold that is actually in motion:
        // St. Jude says radiation "is not used in children under age 3"; CRUK puts every
        // child under 3 in the high-risk group and says whole brain and spine radiation
        // is usually avoided; the AOSNP consensus says the 3-year threshold "is evolving
        // ... such as 4 or 5 years". A page that picks one number is wrong twice.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("St. Jude does not use radiation under age 3", treatment, StringComparison.Ordinal);
        Assert.Contains("Cancer Research UK says every child under 3 is in the higher risk group",
            treatment, StringComparison.Ordinal);
        Assert.Contains("Chemotherapy is used instead, to hold the tumor while the child grows",
            treatment, StringComparison.Ordinal);
        Assert.Contains("**Where the line sits is changing**", treatment, StringComparison.Ordinal);
        Assert.Contains("newer studies use age 4 or 5 in some plans", treatment, StringComparison.Ordinal);
        Assert.Contains("Ask your team where they draw it", treatment, StringComparison.Ordinal);
    }

    [Fact]
    public void TheChemotherapyDrugsCarryTheReasonTheirDosesAreCapped()
    {
        // The three drugs, and the finding that makes the caps make sense rather than
        // read as rationing: vincristine is limited to reduce neuropathy, cisplatin to
        // reduce hearing loss, cyclophosphamide to reduce infertility risk. The page
        // gives the three protected things without the drug-by-drug mapping, which is
        // more than the reader needs and less than the source claims.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("cisplatin, vincristine and cyclophosphamide", treatment, StringComparison.Ordinal);
        Assert.Contains("Doses of each are held down on purpose", treatment, StringComparison.Ordinal);
        Assert.Contains("to protect the nerves, the hearing and the chance of having children later",
            treatment, StringComparison.Ordinal);

        // §12.4 R1: no mg figures either.
        Assert.DoesNotMatch(new Regex(@"\b\d+\s*mg\b", RegexOptions.IgnoreCase), CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheDeEscalationTrialsAreNamedAsUnpublishedAndPromiseNothing()
    {
        // SJMB12, ACNS1422 and PNET5 "explored a moderate reduction of CSI ... While
        // results are not yet published". The temptation on a page whose reader would
        // give anything for less radiation is to let "testing whether" read as "will
        // show", so the page says what nobody can tell them yet.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("Several studies are testing whether children with WNT tumors can safely have less radiation",
            treatment, StringComparison.Ordinal);
        Assert.Contains("Those results are not published yet, so nobody can tell you today what they will show",
            treatment, StringComparison.Ordinal);

        // The trial names are not printed as things to ask for by name: they are trial
        // identifiers, and PNET5 is also the retired-diagnosis trap above.
        Assert.DoesNotMatch(new Regex(@"\bSJMB12\b|\bACNS1422\b"), CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheRiskGroupsAreExplainedByWhatTheyChange()
    {
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("**Risk groups decide how much treatment is given.**", treatment, StringComparison.Ordinal);

        // The SIOPE high-risk list, in reader words: spread, residual disease, LC/A
        // histology, certain gene results, and age under 3 (CRUK's own list).
        foreach (var factor in new[]
                 {
                     "if it has spread",
                     "if the cells are the large cell or anaplastic kind",
                     "if certain gene results are there",
                     "if the child is under 3",
                 })
        {
            Assert.Contains(factor, treatment, StringComparison.Ordinal);
        }

        // /review round 1: the draft wrote the residual-tumor factor as "a piece bigger
        // than a small coin". The sourced threshold is an AREA on the post-operative scan
        // (CRUK: "more than 1.5cm2 in size"; the three-trial analysis: "residual tumor of
        // > 1.5 cm 2"), and 1.5 cm2 is SMALLER than the face of a dime. So the analogy
        // moved the bar about seventy percent the REASSURING way, and turned a
        // two-dimensional measurement into a three-dimensional "piece". No source offers
        // a coin comparison. The page now routes the reader to the measurement instead,
        // which also splits a 46-word sentence carrying five conditions at once.
        Assert.Contains("your team measures what is left on the scan after surgery",
            treatment, StringComparison.Ordinal);
        Assert.Contains("so ask which side of the line yours is on", treatment, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\bcoin\b|\bcm\b", RegexOptions.IgnoreCase), treatment);

        Assert.Contains("Everyone else is in the average risk group, which gets less",
            treatment, StringComparison.Ordinal);
    }

    [Fact]
    public void NewOrWorseBalanceAndWalkingTroubleHasATier()
    {
        // The cerebellum is where this tumor sits, so balance and walking are its
        // signature signs — and a symptom list that names them with no tier is the
        // under-triage shape (WI-536 `/review` round 1).
        var line = ParagraphIn(SymptomHeading, "New or worse trouble with balance or walking");

        // /review round 2: the spread clause. [SPINAL-CORD] composes further down this
        // page and files new trouble walking as RIGHT AWAY for a tumor that has spread to
        // the spine, which is a large share of this hub's readers, while this line filed
        // the same sign as same-day. Added to /tumors/ependymoma in the same change.
        Assert.Contains(
            "is a same-day call, or a right-away call if there is a shunt or if the tumor has spread to the spine",
            line, StringComparison.Ordinal);
    }

    [Fact]
    public void ABabysPressureSignsHaveATier()
    {
        // The shared block's tiers do not name a baby's signs, and CRUK lists these as
        // the ones picked up at a routine development check.
        var baby = ParagraphIn(SymptomHeading, "In a baby,");

        Assert.Contains("the head can grow faster than it should", baby, StringComparison.Ordinal);
        Assert.Contains("the soft spot on top can swell", baby, StringComparison.Ordinal);
        Assert.Contains("call your team the same day, or right away if there is a shunt",
            baby, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSpinalSpreadLinePointsAtItsRuleAndNamesEverySignTheBlockCovers()
    {
        // This hub is the third includer of [SPINAL-CORD], and it is the one the block's
        // "or has spread to the spine" clause was widened for (WI-536 `/review` round 1
        // blocker). The page names the signs; the BLOCK owns the tier, and it renders at
        // the END of the when-to-call section, so the pointer says so.
        var line = ParagraphIn(SymptomHeading, "If the tumor has spread down the spine");

        Assert.Contains("those signs have their own rule, at the end of the section on when to call for help, below",
            line, StringComparison.Ordinal);

        // Read from the BLOCK, so the page and the rule it points at cannot drift apart.
        var block = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "spinal-cord.md"))));
        var tier = Regex.Match(block, @"\*\*If the tumor is in the spinal cord.*?not a same-day one\.", RegexOptions.Singleline);
        var soAre = Regex.Match(block, @"So are new trouble walking[^.]*\.", RegexOptions.Singleline);
        Assert.True(tier.Success && soAre.Success, "the block's right-away sentences cannot be found");
        var rule = tier.Value + " " + soAre.Value;

        // The clause that makes the block true of THIS tumor, which spreads rather than
        // starts in the cord.
        Assert.Contains("or has spread to the spine", tier.Value, StringComparison.Ordinal);

        (string OnPage, string InRule)[] signs =
        [
            ("back pain", @"\bback\b"), ("trouble walking", @"\bwalking\b"),
            ("bladder", @"\bbladder\b"), ("bowel", @"\bbowel\b"),
        ];

        foreach (var (onPage, inRule) in signs)
        {
            Assert.Contains(onPage, line, StringComparison.Ordinal);
            Assert.Matches(new Regex(inRule), rule);
        }
    }

    [Fact]
    public void EverySameDayLineOnThePageHonoursTheShuntRule()
    {
        // WI-535's blocker: the [ESCALATION] block on this page makes the whole same-day
        // tier right-away for a reader with a shunt, so a same-day line the page writes
        // itself has to say so too. RAW page: the blocks carry their own rules, and the
        // escalation block's "Same day means today" lines are not instructions to a
        // shunted reader (which is why composing here would break the guard, not
        // strengthen it).
        var sameDay = PageSentences()
            .Where(s => Regex.IsMatch(s, @"\bsame[- ]day\b", RegexOptions.IgnoreCase))
            .ToList();

        Assert.True(sameDay.Count >= 4, $"only {sameDay.Count} same-day sentences, so this test checks almost nothing");

        foreach (var sentence in sameDay)
        {
            Assert.Matches(
                new Regex(@"right[- ]away[^.]{0,30}\bshunt|\bshunt[^.]{0,40}right[- ]away", RegexOptions.IgnoreCase),
                sentence);
        }
    }

    [Fact]
    public void PosteriorFossaSyndromeIsThisTumorsOwnAndCarriesNoFigure()
    {
        // This is medulloblastoma's classic complication — St. Jude publishes "About 25%"
        // for exactly this operation. The fact is carried, the figure is not (§12.4 R2):
        // the sources' numbers for it run from about 8% to 40%.
        var sub = CuratedPage.Flatten(ComposedSubsectionLf("After surgery at the back of the brain"));

        // This page's own lead-in, which the block deliberately does not carry: how
        // common it is for THIS tumor is the including hub's sentence to write.
        Assert.Contains("**This is common after surgery for a medulloblastoma.**", sub, StringComparison.Ordinal);

        // /review round 1 BLOCKER, and the first defect in this item that a test PINNED
        // rather than caught: the draft said "more so than after most other operations
        // there", a comparative NO cited source makes. St. Jude's own posterior fossa page
        // says the opposite in kind, that the syndrome "also can happen after surgery to
        // remove other kinds of brain tumors in the posterior fossa. Those include
        // astrocytoma and ependymoma", and ACS writes "such as medulloblastoma or
        // ependymoma" with no comparison at all. The fact stays, the ranking goes, and the
        // replacement is asserted so the ranking cannot come back.
        //
        // (Rendered read 1 had already shortened this sentence for a different reason:
        // "at the back of the brain" was appearing three times in four lines once the
        // block composed in.)
        Assert.Contains("It can follow surgery for other tumors at the back of the brain too",
            sub, StringComparison.Ordinal);
        Assert.DoesNotContain("more so than", sub, StringComparison.Ordinal);

        // The block's paragraphs arrive by composition (PosteriorFossaSyndromeBlockTests
        // owns their wording); what this asserts is that this page's reader meets them.
        Assert.Contains("some children stop talking", sub, StringComparison.Ordinal);
        Assert.Contains("**Suddenly not being able to speak is on the ambulance list above, and that rule still stands.**",
            sub, StringComparison.Ordinal);

        // The long-term finding that belongs to this tumor's own cohort study, and the
        // reason it is here rather than in the block: it is medulloblastoma survivors.
        Assert.Contains("**Some of it can last.**", sub, StringComparison.Ordinal);
        Assert.Contains("more likely to have lasting trouble with attention and with how fast thinking works",
            sub, StringComparison.Ordinal);
        Assert.Contains("even after the early symptoms settled", sub, StringComparison.Ordinal);

        // The two risk factors St. Jude names, and the question they make reasonable.
        Assert.Contains("**The risk is lower with a surgeon who does many of these operations**",
            sub, StringComparison.Ordinal);
        Assert.Contains("it is higher the younger the child is", sub, StringComparison.Ordinal);
        Assert.Contains("it is a question teams expect", sub, StringComparison.Ordinal);

        var frequency = new Regex(@"\d\s*%|\b\d+\s+(in|out of)\s+\d+\b|\bin (four|three|five)\b|percent",
            RegexOptions.IgnoreCase);
        Assert.Matches(frequency, "it happens in 1 in 4 children"); // canary: the guard can fire
        Assert.DoesNotMatch(frequency, sub);

        // The anchor the glossary entry and the sibling hubs deep-link.
        Assert.Contains("{#posterior-fossa-syndrome}",
            SectionRawLf(AfterHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnTheComposedPage()
    {
        // PARAGRAPH-scoped and COMPOSED (WI-536 `/review` round 1): a paragraph that
        // names a warning sign AND reassures must carry a tier or the other cause, in
        // that paragraph or the next. Reassurance is not an answer. Composed, because
        // the posterior fossa reassurance ("Most children slowly get better") now
        // arrives from the block — read raw, this guard loses its subject.
        // The shared guard (WI-537 promoted it at this, its second use). The two
        // paragraphs named are the reassuring places this page has, and the guard must
        // have examined both: "Most children slowly get better" arrives from the
        // posterior fossa block, so read raw this check would lose its subject.
        CuratedPage.AssertNoWarningSignIsNormalised(Composed, Slug,
            "Most children slowly get better",
            "Tiredness");
    }

    [Fact]
    public void TirednessIsNeverExplainedAwayAndPointsAtItsTier()
    {
        var life = CuratedPage.Flatten(Section(LifeHeading));

        Assert.Contains("it can also come from the tumor or from fluid that is not draining",
            life, StringComparison.Ordinal);
        Assert.Contains("Sleeping much more than being awake is a same-day call, or a right-away call if there is a shunt",
            life, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLateEffectsAreNamedWithWhoWatchesThemAndWhatHelps()
    {
        // Craniospinal radiation's late effects are the reason this page exists in the
        // form it does: the treatment usually works, and the follow-up runs for years.
        // Each one gets an action, not a warning (§12.6).
        var life = CuratedPage.Flatten(Section(LifeHeading));

        Assert.Contains("can affect memory, attention and how fast school work goes", life, StringComparison.Ordinal);
        Assert.Contains("Ask early who at the hospital helps with school", life, StringComparison.Ordinal);
        Assert.Contains("Cisplatin and radiation to the head can both affect hearing", life, StringComparison.Ordinal);
        Assert.Contains("Hearing tests find it early", life, StringComparison.Ordinal);
        Assert.Contains("can change the hormones that run growth and other body systems", life, StringComparison.Ordinal);
        // WI-538 removed "and it is treatable" here. The endocrine source this page
        // cites carries the DAMAGE ("Some cancer treatments, particularly radiation to
        // the brain, may damage these glands") and a definition of growth hormone
        // deficiency that ends at "Learn more." -- the treatment detail lives on a child
        // page that was never fetched. The sibling /tumors/pediatric-brain-tumor made the
        // same claim from the same file and lost it for the same reason.
        Assert.Contains("hormone levels are part of the regular blood tests", life, StringComparison.Ordinal);

        // Follow-up is not only scans, and the scoliosis line is the one nobody expects.
        var scans = CuratedPage.Flatten(Section(ScansHeading));
        Assert.Contains("it includes hearing tests, eye tests, blood tests for hormones, and a check of balance and strength",
            scans, StringComparison.Ordinal);
        Assert.Contains("A curve in the spine can develop after treatment", scans, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRecurrenceSectionSaysThereIsNoAgreedPlanBecauseTheSourcesDo()
    {
        // "there is no consensus on the optimal management of recurrent
        // medulloblastoma" — a rare case where the honest answer is that the experts do
        // not have one, and saying so beats implying a protocol exists.
        var section = CuratedPage.Flatten(Section(ComesBackHeading));

        Assert.Contains("**There is no single agreed plan for a medulloblastoma that comes back**",
            section, StringComparison.Ordinal);
        Assert.Contains("What is offered depends on what was given the first time", section, StringComparison.Ordinal);

        // Repeat molecular profiling, because the tumor's biology can change at relapse.
        Assert.Contains("**The tumor can change**", section, StringComparison.Ordinal);
        Assert.Contains("your team may test a new sample rather than rely on the first result",
            section, StringComparison.Ordinal);

        // §12.6: the section lands on an action.
        Assert.Contains("**Ask what each option is for**", section, StringComparison.Ordinal);

        // No relapse figures, and none of the sources' outcome language.
        Assert.DoesNotMatch(new Regex(@"almost universally fatal|rarely curable|poor", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheInheritedConditionsAreNamedWithTheirCurrentNamesAndWhatTheyChange()
    {
        // Up to about 5% of cases sit on a predisposition syndrome, and the page's job is
        // the two that change something: who is offered counselling, and that Gorlin
        // syndrome changes the treatment itself.
        var cause = CuratedPage.Flatten(Section(CauseHeading));

        Assert.Contains("**Gorlin syndrome**", cause, StringComparison.Ordinal);
        Assert.Contains("**Li-Fraumeni syndrome**", cause, StringComparison.Ordinal);
        Assert.Contains("**familial adenomatous polyposis**", cause, StringComparison.Ordinal);

        // GeneReviews: "Terms such as Gardner syndrome and Turcot syndrome are of
        // historical interest and should not be used". So it appears only as the old name.
        Assert.Contains("which older letters may call Turcot syndrome", cause, StringComparison.Ordinal);
        Assert.Contains("That name is not used now", cause, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Matches(CuratedPage.ReaderText(Page), "Turcot", RegexOptions.IgnoreCase).Count);

        // The Canadian standard: "All SHH MB need genetic counseling, irrespective of
        // family history". That is a fact a family can act on.
        Assert.Contains("**Which kind the tumor is decides who gets offered testing.**", cause, StringComparison.Ordinal);
        Assert.Contains("families of children with SHH tumors whatever the family history",
            cause, StringComparison.Ordinal);

        // And the one that changes treatment: Gorlin syndrome means radiation-sparing.
        Assert.Contains("children with Gorlin syndrome are unusually sensitive to radiation",
            cause, StringComparison.Ordinal);
        Assert.Contains("say so early", cause, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // Nineteen of this page's sources publish survival figures and none reaches the
        // page (§12.5). The orienting durations it does carry are days and weeks, not
        // outcomes: the fluid-test wait, and "for years" without a number.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        Assert.DoesNotMatch(new Regex(@"\b(median|survival|survive)\b[^.]{0,40}\d", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\d[^.]{0,40}\b(median|survival|survive)\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"survival rate|cure rate", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*%", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"per cent|percent", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"(one|two|three|five|ten)[- ]year", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+[- ]year\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\b(\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|fifteen|twenty)\s+(to\s+\w+\s+)?(months?|years?)\b",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(
            new Regex(@"\b(live|lives|lived|living|survive\w*|die|dies|died|dying)\b[^.]{0,60}\b(a|a few|several|many|some|about a|less than a|more than a)\s+(months?|years?)\b",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+\s+(in|out of)\s+\d+\b", RegexOptions.IgnoreCase), reader);
    }

    [Fact]
    public void TheOutlookGateTeachesTheVocabularyAndNamesThisPagesCaveats()
    {
        var section = CuratedPage.Flatten(Section(OutlookHeading));
        var gate = Regex.Match(section, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");

        var inside = gate.Groups[1].Value;

        // §12.5 / §12.9: a gate that publishes no figures still has to teach the word.
        Assert.Contains("A median is the middle of a group.", inside, StringComparison.Ordinal);
        Assert.Contains("the median is the person standing in the middle", inside, StringComparison.Ordinal);
        Assert.Contains("not a prediction about any one person", inside, StringComparison.Ordinal);

        // This tumor's own caveat, and it is the strongest in the corpus: the molecular
        // groups were not separated in the cohorts the older figures come from, so an
        // old number mixes kinds that are now known to behave differently.
        Assert.Contains("The numbers describe children treated years ago", inside, StringComparison.Ordinal);
        Assert.Contains("The groups were also sorted differently then", inside, StringComparison.Ordinal);
        Assert.Contains("an older figure may mix kinds that behave very differently now",
            inside, StringComparison.Ordinal);

        // The honest shape of what is known, without ranking the groups as good or bad.
        Assert.Contains("the kind matters a great deal", inside, StringComparison.Ordinal);
        Assert.Contains("The outlook is harder when the tumor has spread at diagnosis",
            inside, StringComparison.Ordinal);

        // Curability is not discussed behind the gate (WI-535).
        Assert.DoesNotMatch(CureFamily, inside);

        // And it lands on the team, not on the page.
        Assert.Contains("Your own team knows your situation", inside, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGatedWordsAppearNowhereOutsideTheGate()
    {
        // COMPOSED minus the gate, case-insensitive, with the euphemisms and kindnesses
        // outlook arrives as (WI-535).
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        var gate = Regex.Match(whole, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");
        var outside = whole.Replace(gate.Value, "", StringComparison.Ordinal);

        foreach (var gated in new[]
                 {
                     @"\bmedian\b", @"far better", @"well beyond", @"end[- ]of[- ]life", @"\bhospice\b", @"\bwish",
                     @"\b(die|dies|died|death|dying)\b", @"\bterminal\b", @"life expectancy", @"\boutlook\b",
                     @"\bprognos\w*", @"long[- ]term survi\w*", @"how long\b[^.?]{0,30}\b(have|live|left)\b",
                     @"do better", @"use the time",
                     // /review round 3: the grade section's outcome-claim guard bans
                     // VOCABULARY ("responds well|best|better|to today"), and the gate
                     // itself carries the same claim in words that regex cannot see:
                     // "Treatment works well enough for the WNT kind". That is correct
                     // where it sits, but an edit moving it OUT of the gate would have gone
                     // green. §12.14: banning the phrasing is not fixing the claim, so the
                     // property being guarded here is POSITION.
                     @"works well enough",
                 })
        {
            Assert.DoesNotMatch(new Regex(gated, RegexOptions.IgnoreCase), outside);
        }

        // The cure family does not appear outside the gate at all on this page: unlike
        // ependymoma, no cited source states curability for medulloblastoma in terms a
        // grade section could carry, so the page makes no claim either way.
        Assert.DoesNotMatch(CureFamily, outside);
    }

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Composed), Slug);

        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, reader, StringComparison.OrdinalIgnoreCase);
        }

        // The specific temptation this page has to resist: the molecular groups differ
        // enormously in outcome, and every source ranks them out loud. Naming a group as
        // the good one is the characterisation the gate exists to prevent, and it would
        // sit in the identity section where a reader cannot decline it.
        var report = CuratedPage.Flatten(Section(ReportHeading));
        Assert.DoesNotMatch(new Regex(@"best|worst|good kind|bad kind", RegexOptions.IgnoreCase), report);
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        // Two of its sources are British (CRUK and The Brain Tumour Charity), and the
        // idiom half is what spelling gates miss: CRUK's symptom list is written with
        // "being sick", "fractious" and "GP" (NOTES.md trap 7).
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // Case-insensitive and word-bounded: WI-536's harness beat an Ordinal check with
        // a list item that began with a capital.
        foreach (var idiom in new[]
                 {
                     "out of hours", "A&E", "GP", "straight away", "straight after",
                     "feeling sick", "being sick", "tumour", "behaviour", "fractious",
                     "papilloedema", "co-ordination", "radiotherapy", "999",
                 })
        {
            Assert.DoesNotMatch(new Regex($@"(?<![\w&]){Regex.Escape(idiom)}(?![\w&])", RegexOptions.IgnoreCase), reader);
        }
    }

    [Fact]
    public void TheProtonExplanationIsRoutedToRatherThanRestated()
    {
        // §12.10, and the corpus enforces this one from the other side:
        // /treatments/proton-therapy OWNS the explanation of why a proton beam spares
        // tissue, and its own test asserts that no other page restates it. The first
        // draft of this page restated it in that page's own words and turned
        // ProtonTherapyPageContentTests red. What belongs here is what is specific to
        // THIS tumor's radiation field, which the proton page does not cover: when the
        // spine is in the field, the dose kept away from the chest and belly.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("**[Proton therapy](/treatments/proton-therapy)** is used for some children",
            treatment, StringComparison.Ordinal);
        Assert.Contains("When the spine is treated, it can keep more of the dose away from the chest and belly",
            treatment, StringComparison.Ordinal);
        Assert.Contains("It is not available everywhere", treatment, StringComparison.Ordinal);

        // Read from the sibling, so this cannot drift into a restatement again.
        const string Owned = "stops at the tumor instead of carrying on through";
        var proton = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "proton-therapy.md")));

        Assert.Contains(Owned, proton, StringComparison.Ordinal);
        Assert.DoesNotContain(Owned, CuratedPage.Flatten(CuratedPage.ReaderText(Page)), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSiblingPagesThatNowRouteHereCarryTheirOwnReason()
    {
        // The two doors this item opened, asserted from the sibling side so a later edit
        // cannot quietly close them. Neither sibling had a test that could see the change:
        // ChemotherapyPageTests does not pin its tumor-by-tumor list, and the pediatric
        // hub has no test file at all (WI-538 writes it).
        var chemo = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "chemotherapy.md")));

        Assert.Contains("**[Medulloblastoma](/tumors/medulloblastoma).**", chemo, StringComparison.Ordinal);

        // /review round 3: the bullet said "MOST OFTEN cisplatin, vincristine and
        // cyclophosphamide", a frequency ranking of regimens that no cited source makes
        // (they say "Common regimens include", "typically", "include"), and this test
        // PINNED it — the third pinned-claim defect in this item, after an unsourced
        // sentence (round 1) and an unattributed one (round 2). Both pages now say
        // "usually includes", at one strength, and both name lomustine, which is in the
        // North American average-risk backbone (StatPearls lists it; the Canadian
        // standards give "vincristine, CCNU, cisplatin and cyclophosphamide") and which
        // neither page mentioned while this page has a whole section on it.
        Assert.Contains("usually includes cisplatin, vincristine and cyclophosphamide", chemo, StringComparison.Ordinal);
        Assert.Contains("some plans add", chemo, StringComparison.Ordinal);
        Assert.DoesNotContain("most often cisplatin", chemo, StringComparison.Ordinal);
        Assert.Contains("For a child under 3 it may be used instead of radiation for a while",
            chemo, StringComparison.Ordinal);

        // /review round 2 BLOCKER: this test pinned the bullet's PROSE and asserted
        // nothing about where it traces, and chemotherapy.md's ten sources were all adult
        // or drug-specific, so the one claim WI-537 added to that page was the one a
        // reader could not follow (§12.2 item 2). Same defect this item had just fixed for
        // the shared block, missed on the sibling page it edited. A guard that pins an
        // unattributed claim is round 1's "protecting a falsehood" shape, one notch milder.
        // Asserted as `- url:` ENTRIES, not as bare substrings of the front matter
        // (/review round 3). A bare Contains passes on a URL that survives only in a
        // COMMENT, which is not a live citation and does not reach the reader's source
        // list — and this corpus really does mention URLs in front-matter comments, so
        // that is a live way for the check to go green while the citation is gone.
        var chemoFront = CuratedPage.FrontMatter(CuratedPage.Read("treatments", "chemotherapy.md"));
        Assert.Equal(1, Regex.Matches(chemoFront,
            Regex.Escape("- url: https://together.stjude.org/en-us/conditions/cancers/medulloblastoma.html")).Count);
        Assert.Equal(1, Regex.Matches(chemoFront,
            Regex.Escape("- url: https://pmc.ncbi.nlm.nih.gov/articles/PMC12003599/")).Count);

        // The pediatric hub named this tumor with a glossary escape (%%...%%) because the
        // hub did not exist to link. It is a link now, and the escape is gone.
        var pedsRaw = CuratedPage.Read("tumors", "pediatric-brain-tumor.md");
        Assert.Contains("[Medulloblastoma](/tumors/medulloblastoma)", pedsRaw, StringComparison.Ordinal);
        Assert.DoesNotContain("%%Medulloblastoma%%", pedsRaw, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFrontMatterCarriesTheContractAndNoBarredSource()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"^reviewed: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline), front);
        Assert.Matches(new Regex(@"^review_due: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline), front);

        var urls = Regex.Matches(front, @"^\s+- url: (\S+)", RegexOptions.Multiline);
        Assert.True(urls.Count >= 20, $"only {urls.Count} sources");
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline).Count);

        // §12.1: the stub cited cancer.gov/types/brain, which is barred for naming.
        Assert.DoesNotContain("cancer.gov/types/brain", front, StringComparison.Ordinal);

        // The naming authority, and the consensus paper the two-axes section rests on.
        Assert.Contains("PMC8328013", front, StringComparison.Ordinal);
        Assert.Contains("PMC13525349", front, StringComparison.Ordinal);

        // The CSF timing rule's three sources, including the one that disagrees with
        // itself and is cited FOR that disagreement.
        Assert.Contains("PMC12003599", front, StringComparison.Ordinal);
        Assert.Contains("PMC13257316", front, StringComparison.Ordinal);
        Assert.Contains("PMC7783450", front, StringComparison.Ordinal);

        // The naming-trap sources are not cited for anything. NCI-CONNECT writes "grade
        // IV"; the 2011 consensus paper is pre-CNS5 and its group numbering is the one
        // CRUK copied wrong; the adult SEOM guideline still says "PNET".
        foreach (var barred in new[]
                 {
                     "rare-brain-spine-tumor/tumors/medulloblastoma", "PMC3306779",
                     "PMC8057961", "healthline.com", "verywell", "mayoclinic.org",
                 })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }
    }
}

[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class MedulloblastomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/medulloblastoma";

    private readonly WebApplicationFactory<Program> _factory;

    public MedulloblastomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var response = await _factory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await response.Content.ReadAsStringAsync()).Length > 20_000,
            "a 200 with a near-empty body is not a page (WI-533)");
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support", "/get-help-now");

    [Fact]
    public async Task EveryFragmentLinkLandsOnAnAnchorThatExists() =>
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task EveryBlockDirectiveComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var directive in new[]
                 {
                     "[MECHANISM]", "[CROSSWALK]", "[CAUSES]", "[CAREGIVER]", "[TUMOR-BOARD]",
                     "[ESCALATION]", "[SPINAL-CORD]", "[POSTERIOR-FOSSA-SYNDROME]",
                 })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // One canary per block, so a directive that resolves to nothing is caught as
        // well as one that fails to resolve (WI-536).
        foreach (var canary in new[]
                 {
                     "closed box", "Call your team the same day", "Roman numeral grades",
                     "nobody knows the cause", "tumor board", "caring for someone",
                     "If you have a shunt", "weaker than it was",
                     "Most children slowly get better",
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
    }

    [Fact]
    public async Task ThePagesVocabularyFiresOnceAndTheShorterTermDoesNotDoubleUp()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // The syndrome tooltip fires where the page names it, just above the block that
        // describes it.
        Assert.Contains("def-posterior-fossa-syndrome", html, StringComparison.Ordinal);

        // And the shorter glossary term does NOT also fire. Longest-name-wins is NOT what
        // does the work here, which is what the first draft of this test assumed and what
        // the rendered page disproved. The page names the syndrome TWICE: here, and in the
        // support list where St. Jude's page for families is signposted. The syndrome
        // tooltip is first-occurrence-only, so by that second mention it is spent, and the
        // bare "posterior fossa" term then matched INSIDE the phrase there, putting a
        // second tooltip inside one name. The page suppresses the shorter term with
        // !%posterior fossa%, which is the same decision /tumors/ependymoma made.
        Assert.DoesNotContain("def-posterior-fossa\"", html, StringComparison.Ordinal);

        // "glioma" appears on this page only inside a link to the glioma hub, and a
        // tooltip inside a link is what GlossaryMarker skips. If the word ever escapes
        // the link, the suppression decision has to be made deliberately.
        Assert.DoesNotContain("def-glioma\"", html, StringComparison.Ordinal);
    }
}
