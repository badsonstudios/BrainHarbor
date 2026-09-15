using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-536: <c>/tumors/ependymoma</c>, the stub rewritten as a full §12.3 hub.
///
/// **The safety decisions this page makes.** (1) It includes [MECHANISM] and
/// [ESCALATION] together, and [SPINAL-CORD] straight after, because an ependymoma
/// can sit in the cord or spread to the spine (the shared block's positions are
/// SpinalCordBlockTests'). (2) Every same-day line the page writes itself says what
/// changes for a reader with a shunt (WI-535 round-2 blocker). (3) New or worsening
/// swallowing, walking or balance trouble has a tier, before and after surgery.
/// (4) Tiredness is never explained away.
///
/// **The honesty decisions.** Curability is stated in the grade section at the
/// strength its sources use ("sometimes", "when all of it can be taken out"), never
/// behind the gate, and the glioma hub's "grade 2 and above ... not curable" is
/// scoped at source so the two pages do not contradict each other. Names and grades
/// are CNS5's; the retired ones appear only as retired.
/// </summary>
public sealed class EpendymomaPageContentTests
{
    private const string Slug = "tumors/ependymoma";

    private static string Page => CuratedPage.Read("tumors", "ependymoma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string RawLf => Page.Replace("\r\n", "\n");

    private const string WhatHeading = "What is an ependymoma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string LocationHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string FindHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string LifeHeading = "Everyday life: school, work, seizures and tiredness";
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

    /// <summary>
    /// Reader-text paragraphs (blank-line bounded, heading lines removed, list items
    /// their own paragraphs), flattened.
    /// </summary>
    private static List<string> PageParagraphs() =>
        [.. Regex.Split(
                Regex.Replace(CuratedPage.ReaderText(Page).Replace("\r\n", "\n"), @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline),
                @"\n[ \t]*\n|\n(?=[ \t]*- )")
            .Select(p => CuratedPage.Flatten(p).Trim())
            .Where(p => p.Length > 0)];

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

    /// <summary>The raw text of one `###` subsection, up to the next heading of any level.</summary>
    private static string SubsectionRawLf(string heading)
    {
        var raw = RawLf;
        var start = raw.IndexOf($"\n### {heading}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"the page has no '### {heading}' subsection");
        var end = raw.IndexOf("\n#", start + 5, StringComparison.Ordinal);
        return end < 0 ? raw[start..] : raw[start..end];
    }

    [Fact]
    public void TheSectionsRunInTheOrderTheStandardSets()
    {
        var order = new[]
        {
            "The short version", WhatHeading, GradeHeading, LocationHeading, SymptomHeading,
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
        // The stub carried "can block the flow of fluid" and neither [MECHANISM] nor
        // [ESCALATION]. The two come together (EscalationBlockTests: a hub routed to
        // /treatments/shunts through [MECHANISM] owes the shunt rule).
        Assert.Contains("[MECHANISM]", Section(LocationHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[SPINAL-CORD]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CaregiverHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheLocationAnswerComesBeforeTheSharedMechanismBlock()
    {
        var section = CuratedPage.Flatten(Section(LocationHeading));
        var block = section.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(block > 0, "the section has lost [MECHANISM]");

        foreach (var place in new[] { "Low at the back of the brain", "Higher up in the brain", "In the spinal cord" })
        {
            var at = section.IndexOf(place, StringComparison.Ordinal);
            Assert.True(at >= 0 && at < block, $"'{place}' is not named before [MECHANISM]");
        }
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void TheSpinalSymptomLinePointsAtItsRule()
    {
        // A symptom list that names cord signs without a tier is the under-triage
        // shape; the tier is the [SPINAL-CORD] block, which renders at the END of the
        // when-to-call section (/review round 1: "just after the list" was wrong).
        var line = ParagraphIn(SymptomHeading, "In the spinal cord:");

        foreach (var sign in new[] { "back or neck pain", "numbness or weakness", "bladder or bowel" })
        {
            Assert.Contains(sign, line, StringComparison.Ordinal);
        }

        Assert.Contains("or one that has spread to the spine, those signs have their own rule, at the end of the section on when to call for help, below",
            line, StringComparison.Ordinal);
    }

    [Fact]
    public void EverySpinalSignThePageNamesIsInTheBlocksRule()
    {
        // /review round 1 blocker: the page sent "back or neck pain" to a rule that said
        // only "back pain". Read from the BLOCK, so the two cannot drift apart again.
        var block = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "spinal-cord.md"))));
        var line = ParagraphIn(SymptomHeading, "In the spinal cord:");

        // /review round 2: matching against the WHOLE block passed if a word appeared
        // anywhere in it, including in a tier the block had DEMOTED. Scoped to the two
        // sentences that carry the right-away tier.
        var tier = Regex.Match(block, @"\*\*If the tumor is in the spinal cord.*?not a same-day one\.", RegexOptions.Singleline);
        var soAre = Regex.Match(block, @"So are new trouble walking[^.]*\.", RegexOptions.Singleline);
        Assert.True(tier.Success && soAre.Success, "the block's right-away sentences cannot be found");
        var rule = tier.Value + " " + soAre.Value;

        (string OnPage, string InRule)[] signs =
        [
            ("neck", @"\bneck\b"), ("back", @"\bback\b"), ("numbness", @"\bnumbness\b"),
            ("weakness", @"\bweak\b"), ("bladder", @"\bbladder\b"), ("bowel", @"\bbowel\b"),
        ];

        foreach (var (onPage, inRule) in signs)
        {
            Assert.Contains(onPage, line, StringComparison.Ordinal);
            Assert.Matches(new Regex(inRule), rule);
        }
    }

    [Fact]
    public void NewOrWorseningBackOfBrainSignsHaveATier()
    {
        // /review round 1: balance and walking trouble were listed with no tier, and the
        // 2025 review says "Ataxia may emerge or worsen postoperatively".
        var line = ParagraphIn(SymptomHeading, "New or worse trouble with walking or balance");

        Assert.Contains("is a same-day call, or a right-away call if there is a shunt", line, StringComparison.Ordinal);

        // /review round 2: swallowing is in the cited sources ONLY as a post-operative
        // problem (St. Jude's surgery complications and the posterior fossa syndrome
        // page). The one source listing it as a presenting symptom is the Brain Tumour
        // Charity, which this page bars. It belongs after surgery, not in this list.
        var symptoms = CuratedPage.Flatten(Section(SymptomHeading));
        Assert.DoesNotContain("- Trouble swallowing", symptoms, StringComparison.Ordinal);
    }

    [Fact]
    public void EverySameDayLineOnThePageHonoursTheShuntRule()
    {
        // WI-535 round 2: the [ESCALATION] block on this page makes the whole same-day
        // tier right-away for a reader with a shunt, so a same-day line the page
        // writes itself has to say so too. Raw page: the block carries its own rule.
        var sameDay = PageSentences()
            .Where(s => Regex.IsMatch(s, @"\bsame[- ]day\b", RegexOptions.IgnoreCase))
            .ToList();

        Assert.True(sameDay.Count >= 5, $"only {sameDay.Count} same-day sentences, so this test checks almost nothing");

        foreach (var sentence in sameDay)
        {
            Assert.Matches(
                new Regex(@"right[- ]away[^.]{0,30}\bshunt|\bshunt[^.]{0,40}right[- ]away", RegexOptions.IgnoreCase),
                sentence);
        }
    }

    [Fact]
    public void TirednessIsNeverExplainedAwayAndPointsAtItsTier()
    {
        var life = CuratedPage.Flatten(Section(LifeHeading));

        Assert.Contains("it can also come from the tumor or from fluid that is not draining", life, StringComparison.Ordinal);
        Assert.Contains("Sleeping much more than being awake is a same-day call, or a right-away call if there is a shunt",
            life, StringComparison.Ordinal);
    }

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnThePage()
    {
        // PARAGRAPH-scoped (/review round 1): the old sentence window never saw the
        // posterior fossa syndrome paragraph, whose "most children slowly get better"
        // sat three sentences from its symptoms. A paragraph that names a warning sign
        // AND reassures must carry a tier or the other cause, in that paragraph or the
        // next. Reassurance is not an answer (harness run 1).
        var paragraphs = PageParagraphs();
        var normaliser = new Regex(
            @"\b(normal|expected|nothing to worry|common|usual|side effects?|part of (the )?treatment|(can )?comes? from (the )?(treatment|surgery)|usually (comes?|is) from|go on for a while|get better)\b",
            RegexOptions.IgnoreCase);
        var symptom = new Regex(
            // /review round 3: `behavio\w*` was missing, so the guard could not fire on a
            // behavior change — the one posterior fossa sign the tier had stranded.
            @"\b(headaches?|vomit\w*|throwing up|sleep\w*|tired\w*|drows\w*|weak\w*|numb\w*|swallow\w*|talk\w*|speak\w*|speech|walk\w*|balance|mood|behavio\w*|irritab\w*|dizz\w*)\b",
            RegexOptions.IgnoreCase);
        var answered = new Regex(
            @"also come from the tumor|same-day call|call your team the same day|right away|right-away|ambulance",
            RegexOptions.IgnoreCase);

        var checkedParagraphs = new List<string>();
        for (var i = 0; i < paragraphs.Count; i++)
        {
            if (!symptom.IsMatch(paragraphs[i]) || !normaliser.IsMatch(paragraphs[i]))
            {
                continue;
            }

            checkedParagraphs.Add(paragraphs[i]);
            var context = paragraphs[i] + " " + (i + 1 < paragraphs.Count ? paragraphs[i + 1] : "");
            Assert.True(answered.IsMatch(context),
                $"a paragraph names a warning sign and reassures, with no tier: \"{paragraphs[i]}\"");
        }

        // Positive: the three reassuring places this page has were all checked.
        foreach (var expected in new[] { "Most children slowly get better", "can go on for a while after spinal surgery", "Tiredness" })
        {
            Assert.Contains(checkedParagraphs, p => p.Contains(expected, StringComparison.Ordinal));
        }

        var spine = CuratedPage.Flatten(SubsectionRawLf("After surgery on the spine"));
        Assert.Contains("New or worse pain in the back or neck, and weakness or numbness that is new or worse, is a right-away call",
            spine, StringComparison.Ordinal);
        Assert.Contains("which of those to expect, and which mean calling them", spine, StringComparison.Ordinal);
    }

    [Fact]
    public void NewSwallowingTroubleAfterSurgeryHasATier()
    {
        // "Worth telling the team" is not a tier (WI-535 B2); /review round 1: "new"
        // alone missed a child who came home swallowing badly and got worse.
        var paragraph = ParagraphIn(CaregiverHeading, "**After surgery at the back of the brain, watch talking and swallowing.**");

        Assert.Contains("New or worse trouble swallowing after you are home is a same-day call, or a right-away call if there is a shunt",
            paragraph, StringComparison.Ordinal);
        Assert.Contains("Choking, or trouble breathing, is an ambulance call", paragraph, StringComparison.Ordinal);
        Assert.Contains("ask to be shown", paragraph, StringComparison.Ordinal);
        Assert.DoesNotContain("worth telling the team", CuratedPage.ReaderText(Page), StringComparison.Ordinal);
    }

    [Fact]
    public void ABabysPressureSignsHaveATier()
    {
        // Rendered read 1: the head growing and the soft spot bulging were listed with
        // no timing, and the shared block's tiers do not name a baby's signs.
        var baby = ParagraphIn(SymptomHeading, "In a baby,");

        Assert.Contains("the soft spot on top of the head can bulge", baby, StringComparison.Ordinal);
        // /review round 3: St. Jude lists "Irritability or confusion", and it was the one
        // presenting sign in the cited sources the page dropped.
        Assert.Contains("the baby can be more irritable than usual", baby, StringComparison.Ordinal);
        Assert.Contains("call your team the same day, or right away if there is a shunt", baby, StringComparison.Ordinal);
    }

    [Fact]
    public void PosteriorFossaSyndromeIsTimedAndHonestAndCarriesNoFigure()
    {
        var sub = CuratedPage.Flatten(SubsectionRawLf("After surgery at the back of the brain"));

        foreach (var required in new[]
                 {
                     "some children stop talking", "It always starts within the first week",
                     "Doctors know it as a complication of the operation itself",
                     "Most children slowly get better", "for some it takes months", "even years",
                     "Speech therapy can help",
                     // /review round 2 BLOCKER: the shared block files sudden loss of speech
                     // as an AMBULANCE call, and this section was giving the same sign a
                     // same-day tier. The carve-out keeps the stronger rule, and the
                     // at-home tier names only the signs it covers.
                     "**Suddenly not being able to speak is on the ambulance list above, and that rule still stands.**",
                     // /review round 3: the tier named three of the four signs the section
                     // lists, stranding behavior, which [ESCALATION] does not cover either.
                     "If the swallowing, walking, mood or behavior changes start or get worse after you are home, call your team the same day, or right away if there is a shunt",
                     "St. Jude says the risk of it is lower with an experienced surgeon",
                 })
        {
            Assert.Contains(required, sub, StringComparison.Ordinal);
        }

        // /review round 1: "it is not the tumor coming back" is in no source.
        Assert.DoesNotContain("tumor coming back", sub, StringComparison.Ordinal);

        // The sources' frequency figures run from 8% to 40% and none is specific to
        // ependymoma, so no frequency is published in any form.
        // /review round 3 nit: the old alternation would have matched "in 3 weeks" if this
        // guard were ever widened past the subsection. A ratio needs both numbers.
        var frequency = new Regex(@"\d\s*%|\b\d+\s+(in|out of)\s+\d+\b|\bin (four|three|five)\b|percent", RegexOptions.IgnoreCase);
        Assert.Matches(frequency, "it happens in 1 in 4 children"); // canary: the guard can fire
        Assert.DoesNotMatch(frequency, sub);
        Assert.Contains("{#posterior-fossa-syndrome}", SubsectionRawLf("After surgery at the back of the brain"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCancerQuestionIsAnsweredInTheFirstSentence()
    {
        // /review round 1: "Some are and some are not" went beyond every cited source for
        // grade 2, and clashed with [SPINAL-CORD]'s "When cancer presses". The answer is
        // the grade, with grade 2's recurrence beside it.
        var section = CuratedPage.Flatten(Section(GradeHeading));
        var first = CuratedPage.SentencesOf(section).First();

        Assert.Matches(new Regex(@"^\W*Grade 3 is malignant, which means cancer\."), first);

        // /review round 2 BLOCKER: the answer covered grade 3 only, while the page's own
        // next sentence says most are grade 2 or 3. A grade 2 reader was left to infer
        // "not cancer", which is the corpus's documented core confusion (§12.3 §2).
        Assert.Contains("Grade 2 is called low grade, which is about how fast it grows, not about how serious it is.",
            section, StringComparison.Ordinal);
        Assert.Contains("A grade 2 ependymoma is still treated with surgery, and it is still followed with scans for years",
            section, StringComparison.Ordinal);

        // /review round 3: a grade 1 report, and a molecular name with no grade beside
        // it, both reached the end of this section with no answer. No cited source calls
        // grade 1 benign, so the answer is scope, not a verdict.
        Assert.Contains("**Grade 1 is the slowest-growing kind.**", section, StringComparison.Ordinal);
        Assert.Contains("or carries a name with no grade beside it, that is the whole answer, not a gap in it",
            section, StringComparison.Ordinal);
        Assert.Contains("the grade does not always match how it behaves", section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"some are not|not cancer|benign", RegexOptions.IgnoreCase), CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void CurabilityIsStatedInTheGradeSectionAtItsSourcesStrength()
    {
        // ACS: "They can sometimes be cured by surgery if the entire tumor can be
        // removed, but this is not always possible." St. Jude: "hard to cure without
        // successful surgery". Stated in the grade section, never behind the gate
        // (WI-535), and never at a strength above the source.
        var cure = CuratedPage.Flatten(SubsectionRawLf("Can it be cured?"));

        Assert.Contains("**Some ependymomas can be cured by surgery, when all of it can be taken out.**", cure, StringComparison.Ordinal);
        Assert.Contains("That is not always possible", cure, StringComparison.Ordinal);
        Assert.Contains("When some has to be left behind, it is much harder to cure.", cure, StringComparison.Ordinal);
        Assert.Contains("sometimes many years later", cure, StringComparison.Ordinal);

        // §12.6 landing, scoped to the subsection's own paragraphs (WI-517).
        var hard = cure.IndexOf("much harder to cure", StringComparison.Ordinal);
        var landing = cure.IndexOf("It is the reason your team puts so much into the first operation", StringComparison.Ordinal);
        Assert.True(landing > hard, "the hard curability sentence has lost the landing that follows it");

        // The short version carries the condition too (/review round 1 nit).
        Assert.Contains("Some ependymomas can be cured that way, when all of it can be taken out.",
            CuratedPage.Flatten(Section("The short version")), StringComparison.Ordinal);

        Assert.DoesNotMatch(
            new Regex(@"usually (be )?cured|often (be )?cured|most (ependymomas )?can be cured|is curable", RegexOptions.IgnoreCase),
            CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheGliomaHubsNotCurableSentenceIsScopedAndThisPageAnswersIt()
    {
        // §12.10, read from the sibling. /tumors/glioma said "Grade 2 and above are
        // different. These grow out into the brain around them ... not curable", on the
        // page that lists ependymoma in its family.
        var glioma = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "glioma.md")));

        Assert.Contains("**Grade 2 and above are different for most gliomas.**", glioma, StringComparison.Ordinal);

        // /review round 1: "one exception to this paragraph" excepted ependymoma from
        // "They are malignant" too, and grade 3 ependymoma is malignant. The pointer now
        // makes no claim of its own.
        const string pointer = "[Ependymoma](/tumors/ependymoma) is different from the gliomas this paragraph describes, and its page explains how.";
        Assert.Contains(pointer, glioma, StringComparison.Ordinal);
        Assert.DoesNotContain("exception to this paragraph", glioma, StringComparison.Ordinal);

        // Rendered read 1: placed mid-paragraph, the pointer made "They are malignant ...
        // not curable" read as ependymoma. It comes AFTER the hard sentence.
        var hard = glioma.IndexOf("with today's treatments they are not curable", StringComparison.Ordinal);
        var at = glioma.IndexOf(pointer, StringComparison.Ordinal);
        Assert.True(hard > 0 && at > hard, "the ependymoma pointer must follow the not-curable sentence");

        var cure = CuratedPage.Flatten(SubsectionRawLf("Can it be cured?"));
        Assert.Contains("That is about the gliomas that grow out into the brain around them.", cure, StringComparison.Ordinal);
        Assert.Contains("so that sentence was not written about this tumor", cure, StringComparison.Ordinal);
    }

    [Fact]
    public void TheNamesAreCns5AndTheRetiredOnesAppearOnlyAsRetired()
    {
        var report = CuratedPage.Flatten(Section(ReportHeading));

        foreach (var name in new[]
                 {
                     "**Posterior fossa ependymoma, group PFA**", "**group PFB.**",
                     "**Supratentorial ependymoma, ZFTA fusion-positive.**",
                     "**Supratentorial ependymoma, YAP1 fusion-positive.**",
                     "**spinal ependymoma, MYCN-amplified**",
                     "**Myxopapillary ependymoma.** Low in the spine. Grade 2.",
                     "**Subependymoma.** Grade 1.",
                     // /review round 1: site-only names (CNS5: when molecular analysis
                     // "fails or is unavailable").
                     "with no group or gene after it. The name was given by place alone.",
                     // /review round 2: the research subgroup strings a methylation report
                     // carries (StatPearls, EANO).
                     "**PF-EPN-A**, **PF-EPN-B** or **ST-EPN-RELA.**",
                     "PF-EPN-A is group PFA, PF-EPN-B is group PFB, and ST-EPN-RELA is the ZFTA kind.",
                 })
        {
            Assert.Contains(name, report, StringComparison.Ordinal);
        }

        // cIMPACT-7: no grade is assigned to the molecular types.
        Assert.Contains("For the names built on a gene result, a grade is often left off", report, StringComparison.Ordinal);

        // Negation-aware (§12.9): every retired name is a bullet in the older-paperwork
        // slice, AND appears nowhere else on the page.
        var retired = CuratedPage.Flatten(SubsectionRawLf("If your older paperwork says something different"));
        (string Term, string Bullet)[] slice =
        [
            ("anaplastic", "**Anaplastic ependymoma.** An older name for a faster-growing ependymoma. It is no longer used."),
            ("classic", "**Classic ependymoma.** Reports used to split classic from anaplastic. That split is no longer used."),
            ("RELA", "**RELA fusion-positive**, or **C11orf95-RELA.** Older names for the ZFTA kind above."),
            ("C11orf95", "**C11orf95-RELA.**"),
            ("tanycytic", "**Papillary, clear cell** or **tanycytic ependymoma.** These now describe how the cells look."),
        ];

        var outsideSlice = CuratedPage.Flatten(CuratedPage.ReaderText(Page)).Replace(retired, "", StringComparison.Ordinal);
        foreach (var (term, bullet) in slice)
        {
            Assert.Contains(bullet, retired, StringComparison.Ordinal);
            Assert.DoesNotMatch(new Regex($@"\b{term}\b", RegexOptions.IgnoreCase), outsideSlice);
        }

        Assert.Contains("**Myxopapillary ependymoma, grade 1.** It is grade 2 now.", retired, StringComparison.Ordinal);

        var roman = new Regex(@"\bgrade\s+(I|II|III|IV)\b", RegexOptions.IgnoreCase);
        Assert.Matches(roman, "an older report said grade III"); // canary: the guard can fire
        Assert.DoesNotMatch(roman, CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void StagingChecksTheWholeSpineAndPrintsTheSpinalTapDisagreement()
    {
        var find = CuratedPage.Flatten(Section(FindHeading));

        Assert.Contains("of the brain **and the whole spine**", find, StringComparison.Ordinal);
        Assert.Contains("Checking does not mean it has.", find, StringComparison.Ordinal);
        Assert.Contains("fluid taken too soon can be muddied by the operation", find, StringComparison.Ordinal);
        // EANO "not earlier than 2-3 wk" against StatPearls' 10 to 14 days: the spread is
        // printed, and only EANO is a guideline (/review round 1).
        Assert.Contains("from about 10 days to about 3 weeks", find, StringComparison.Ordinal);
        Assert.DoesNotContain("guidelines", find, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheRadiationAgeLineIsPrintedAsTheDisagreementItIs()
    {
        // /review round 1: "other sources say under 3" read as the opposite, and the
        // 2025 review agrees with St. Jude; only ACS's general page says 3.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("St. Jude and a 2025 review say radiation is used from about age 1", treatment, StringComparison.Ordinal);
        Assert.Contains("some children aged 1 to 3 have chemotherapy first, to put radiation off", treatment, StringComparison.Ordinal);
        Assert.Contains("The American Cancer Society's general page says radiation is usually not given before age 3", treatment, StringComparison.Ordinal);
        Assert.DoesNotContain("other sources say", treatment, StringComparison.Ordinal);
        Assert.DoesNotContain("St. Jude gives", treatment, StringComparison.Ordinal);

        Assert.Contains("Doctors are not sure it helps after a grade 2 tumor is fully taken out", treatment, StringComparison.Ordinal);
        Assert.Contains("radiation is usually given only if some was left behind", treatment, StringComparison.Ordinal);
    }

    [Fact]
    public void TheChemotherapyNoteIsHonestAndCarriesItsTrialsLimits()
    {
        // ACNS0831: ages 1-21, randomized only after GTR/NTR or complete response, and
        // "Further follow-up is important to assess its effect on late relapses".
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("A trial in children and young adults, whose tumor was all or nearly all gone before radiation, found that adding chemotherapy after radiation did not help, at least so far",
            treatment, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"(large|larger|big|major) trial", RegexOptions.IgnoreCase), CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheReirradiationDisagreementIsPrintedAndAttributed()
    {
        var section = CuratedPage.Flatten(Section(ComesBackHeading));

        Assert.Contains("Radiation a second time is something sources describe differently", section, StringComparison.Ordinal);
        Assert.Contains("Recent reviews say it can be an option for adults and for children", section, StringComparison.Ordinal);
        Assert.Contains("The CERN Foundation says someone who has had radiation before may not be able to have more", section, StringComparison.Ordinal);

        // StatPearls, split by site (/review round 1 nit).
        Assert.Contains("After a tumor at the back of the brain, it tends to come back in the same place", section, StringComparison.Ordinal);

        // The radiation page's second-course section describes a gap of "around a year
        // or more", which is not what these sources say (WI-535).
        Assert.DoesNotContain("#why-am-i-having-radiation-again", CuratedPage.ReaderText(Page), StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"low chance|dismal|poor (outlook|prognosis)", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheScansAreLongTermAndTheirLengthIsHedgedAsTheSourcesHedgeIt()
    {
        var scans = CuratedPage.Flatten(Section(ScansHeading));

        Assert.Contains("**The scans go on for years after this tumor.**", scans, StringComparison.Ordinal);
        Assert.Contains("One recent review suggests scans for about 7 to 10 years", scans, StringComparison.Ordinal);
        Assert.Contains("Another says nobody is yet sure how often or for how long", scans, StringComparison.Ordinal);
    }

    [Fact]
    public void Nf2IsNamedByItsCurrentName()
    {
        var cause = CuratedPage.Flatten(Section(CauseHeading));

        Assert.Contains("**NF2-related schwannomatosis**", cause, StringComparison.Ordinal);
        // GeneReviews: the name "was proposed by Plotkin et al [2022]".
        Assert.Contains("Older letters call it neurofibromatosis type 2, and the new name was suggested in 2022", cause, StringComparison.Ordinal);

        // The old name only as the old name.
        Assert.Equal(1, Regex.Matches(CuratedPage.ReaderText(Page), "neurofibromatosis", RegexOptions.IgnoreCase).Count);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // The page carries orienting durations on purpose (R1): the spinal tap wait and
        // the scan years. They are redacted by exact sentence, so a survival figure
        // written anywhere else is still caught.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        foreach (var allowed in new[]
                 {
                     "They give different waits, from about 10 days to about 3 weeks.",
                     "One recent review suggests scans for about 7 to 10 years.",
                     "It is most common in children under 5.",
                 })
        {
            Assert.Contains(allowed, reader, StringComparison.Ordinal);
            reader = reader.Replace(allowed, "", StringComparison.Ordinal);
        }

        Assert.DoesNotMatch(new Regex(@"\b(median|survival|survive)\b[^.]{0,40}\d", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\d[^.]{0,40}\b(median|survival|survive)\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"survival rate|cure rate", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*%", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"per cent|percent", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"(one|two|three|five|ten)[- ]year", RegexOptions.IgnoreCase), reader);
        // /review round 1 nit: "5-year" written without "survival".
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
        Assert.Contains("A median is the middle of a group.", inside, StringComparison.Ordinal);
        Assert.Contains("the median is the person standing in the middle", inside, StringComparison.Ordinal);
        Assert.Contains("not a prediction about any one person", inside, StringComparison.Ordinal);

        // This tumor's own caveats: the pre-2021 sorting, and the kind mattering.
        Assert.Contains("Before 2021, these tumors were sorted mostly by how the cells looked", inside, StringComparison.Ordinal);
        Assert.Contains("an older figure may mix kinds that behave very differently", inside, StringComparison.Ordinal);
        Assert.Contains("when the tumor could not all be taken out, when the child is younger, or with certain gene results", inside, StringComparison.Ordinal);
        Assert.Contains("a median cannot show you the people who do far better than the middle", inside, StringComparison.Ordinal);

        // Curability lives in the grade section (WI-535).
        Assert.DoesNotMatch(CureFamily, inside);
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
                     @"do better", @"harder once it has come back", @"use the time",
                 })
        {
            Assert.DoesNotMatch(new Regex(gated, RegexOptions.IgnoreCase), outside);
        }

        // The cure family outside the gate: only these, each exactly once.
        var allowed = new[]
        {
            // The subsection heading is reader text too, and it IS the question.
            "### Can it be cured?",
            "Some ependymomas can be cured that way, when all of it can be taken out.",
            "**Some ependymomas can be cured by surgery, when all of it can be taken out.**",
            "When some has to be left behind, it is much harder to cure.",
            "You may have read that a glioma of grade 2 or above cannot be cured.",
        };
        foreach (var sentence in allowed)
        {
            Assert.Equal(1, Regex.Matches(outside, Regex.Escape(sentence)).Count);
            outside = outside.Replace(sentence, "", StringComparison.Ordinal);
        }

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
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // Case-insensitive, word-bounded (harness run 1): an Ordinal "being sick" let
        // "- Being sick, often in the morning." through, because a list item starts
        // with a capital.
        foreach (var idiom in new[] { "out of hours", "A&E", "GP", "straight away", "straight after", "feeling sick", "being sick", "tumour", "behaviour" })
        {
            Assert.DoesNotMatch(new Regex($@"(?<![\w&]){Regex.Escape(idiom)}(?![\w&])", RegexOptions.IgnoreCase), reader);
        }
    }

    [Fact]
    public void TheFrontMatterCarriesTheContractAndNoBarredSource()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"^reviewed: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline), front);
        Assert.Matches(new Regex(@"^review_due: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline), front);

        var urls = Regex.Matches(front, @"^\s+- url: (\S+)", RegexOptions.Multiline);
        Assert.True(urls.Count >= 15, $"only {urls.Count} sources");
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline).Count);

        // §12.1: the stub cited NCI's patient PDQ for everything.
        Assert.DoesNotContain("cancer.gov/types/brain", front, StringComparison.Ordinal);
        Assert.Contains("PMC8328013", front, StringComparison.Ordinal);
        Assert.Contains("PMC8018155", front, StringComparison.Ordinal);
        // /review round 1: the higher-in-the-brain speech and vision symptoms are NBTS's.
        Assert.Contains("braintumor.org/brain-tumors/about-brain-tumors/brain-tumor-types/ependymoma", front, StringComparison.Ordinal);

        // The naming-trap sources are not cited (NOTES.md), and neither is the wrong
        // StatPearls ID the scouting brief carried (a hernia chapter).
        foreach (var barred in new[] { "thebraintumourcharity.org", "dana-farber.org", "abta.org", "NBK538290", "healthline.com", "verywell" })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }
    }
}

[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class EpendymomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/ependymoma";

    private readonly WebApplicationFactory<Program> _factory;

    public EpendymomaPageRenderTests(WebApplicationFactory<Program> factory) =>
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
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/seizures/what-to-do", "/seizures/living-with");

    [Fact]
    public async Task EveryFragmentLinkLandsOnAnAnchorThatExists() =>
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task EveryBlockDirectiveComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var directive in new[]
                 { "[MECHANISM]", "[CROSSWALK]", "[CAUSES]", "[CAREGIVER]", "[TUMOR-BOARD]", "[ESCALATION]", "[SPINAL-CORD]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        foreach (var canary in new[]
                 {
                     "closed box", "Call your team the same day", "Roman numeral grades",
                     "nobody knows the cause", "tumor board", "caring for someone",
                     "If you have a shunt", "weaker than it was",
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
    public async Task ThePagesNewVocabularyFiresAndItsInlineDefinitionDoesNotEcho()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // Named in the treatment section before the subsection that describes it.
        Assert.Contains("def-posterior-fossa-syndrome", html, StringComparison.Ordinal);

        // "posterior fossa" is defined inline where it first appears; its tooltip would
        // read the sentence twice (WI-535), so it is suppressed here.
        Assert.DoesNotContain("def-posterior-fossa\"", html, StringComparison.Ordinal);

        // Rendered read 1: the glioma tooltip landed inside the curability sentence (the
        // word is already a link above it), and supratentorial's echoed "Higher up in
        // the brain" in the same bullet.
        Assert.DoesNotContain("def-glioma\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("def-supratentorial\"", html, StringComparison.Ordinal);
    }
}
