using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-555: <c>/treatments/palliative-care</c>. A §12.8 library page whose title is its
/// content — palliative care is not hospice — and whose risk is a small set of claims the
/// dossier and the sources state more strongly than the evidence does.
///
/// Pinned is where this page could leave a reader worse off than no page:
///
///   * THE CONFLATION ITSELF. The short version and "Is it the same as hospice?" say it
///     is not, that it goes alongside treatment aimed at the tumor, and that asking is
///     not giving up.
///   * A PROGNOSIS OUTSIDE THE READER'S CHOICE. Hospice cannot be explained without
///     talking about the last stage of an illness, so that part sits inside the
///     <c>:::outlook</c> gate (§12.5), and the gate's vocabulary is pinned out of
///     everything else. Inside it, NO life-expectancy figure: Medicare's six-month rule
///     is described in words (§12.5: consent to read, not a licence to publish).
///   * A SURVIVAL CLAIM. The evidence is one lung cancer trial (Temel 2010); the one
///     large glioblastoma study points nowhere useful. The page answers the question in
///     both directions and nowhere else.
///   * OVERSTATING A SOURCE, four times: ASCO's dropped "within 8 weeks" and its
///     "advanced cancer" scope; the backlog's "help sooner" (Schubart says HOSPICE, of
///     the six bereaved families); the unsourced "costs people months"; and Medicare's
///     coverage of palliative care outside hospice, which only NCI states, as "some".
///   * "CURE" TELLING A READER THE HELP IS NOT FOR THEM. For most malignant brain
///     tumors no treatment is curative, so the page says "treatment aimed at the tumor"
///     and uses Medicare's word only where it reports Medicare's rule.
///
/// Paragraphs are SLICED and asserted on, not the whole page, and every pattern is
/// written for <c>\r?\n</c> (the repo is CRLF locally and LF in CI).
/// </summary>
public sealed class PalliativeCarePageContentTests
{
    private const string Slug = "treatments/palliative-care";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is palliative care?";
    private const string HospiceVsHeading = "Is it the same as hospice?";
    private const string EarlyHeading = "Why ask for it early?";
    private const string HelpsHeading = "What can it help with?";
    private const string TeamHeading = "Who is on the team?";
    private const string StepsHeading = "What happens, step by step";
    private const string WhereHeading = "Where does it happen?";
    private const string GivingUpHeading = "Does it mean giving up?";
    private const string LongerHeading = "Does it help people live longer?";
    private const string CostHeading = "What does it cost?";
    private const string ChildrenHeading = "Children and teens";
    private const string HardHeading = "What is hard about it, and what can be done";
    private const string HospiceHeading = "Hospice, if and when it comes up";
    private const string CaregiverHeading = "If you are caring for someone";
    private const string WhoHeading = "Who does it, and who do I call?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    private static string Page => CuratedPage.Read("treatments", "palliative-care.md");

    private static readonly Regex Gate = new(
        @"^:::outlook[ \t]*\r?$(.*?)^:::[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.Singleline);

    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?[ \t]*\r?$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    private static string Clean(string raw) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderTextOfBody(raw)), @"[*_]", "").Trim();

    private static List<string> ParagraphsOf(string raw) =>
        [.. Regex.Split(raw, @"\r?\n[ \t]*\r?\n|\r?\n(?=- |\d+\. )")
            // /review: dropping any chunk that starts with ":::" dropped the gate's FIRST
            // paragraph too, because it follows the fence with no blank line. Strip the
            // fence lines and keep the words.
            .Select(p => Regex.Replace(p, @"(?m)^:::\w*[ \t]*\r?$", ""))
            .Select(p => Regex.Replace(p.TrimStart(), @"^(?:-|\d+\.)\s+", ""))
            .Select(Clean).Where(p => p.Length > 0)];

    private static string PlainOf(string heading) => Clean(RawSection(heading));

    /// <summary>Everything a reader meets: headline, composed body, gate included.</summary>
    private static string Plain =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.EverythingAReaderMeets(Page)), @"[*_]", "");

    /// <summary>The gate's own words, cleaned.</summary>
    private static string Gated
    {
        get
        {
            var gates = Gate.Matches(CuratedPage.Body(Page));
            Assert.Single(gates);
            return Clean(gates[0].Groups[1].Value);
        }
    }

    /// <summary>
    /// What a reader meets WITHOUT opening the gate: the headline plus the body with the
    /// gate cut out. Built from the same body the gate is cut from, so a second gate, or
    /// a gated paragraph moved outside the fence, changes this text.
    /// </summary>
    private static string Ungated =>
        Regex.Replace(CuratedPage.Flatten(
            CuratedPage.Headline(Page, null) + " "
            + CuratedPage.ReaderTextOfBody(Gate.Replace(CuratedPage.Body(Page), " "))), @"[*_]", "");

    private static string Paragraph(string heading, string containing)
    {
        var hits = ParagraphsOf(RawSection(heading))
            .Where(p => p.Contains(containing, StringComparison.Ordinal)).ToList();
        Assert.True(hits.Count == 1,
            $"expected exactly one paragraph in '{heading}' containing '{containing}', found {hits.Count}");
        return hits[0];
    }

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentInOrder()
    {
        string[] required =
        [
            ShortHeading, WhatHeading, HospiceVsHeading, EarlyHeading, HelpsHeading, TeamHeading,
            StepsHeading, WhereHeading, GivingUpHeading, LongerHeading, CostHeading, ChildrenHeading,
            HardHeading, HospiceHeading, CaregiverHeading, WhoHeading, QuestionsHeading, NextHeading,
        ];

        Assert.Equal(required, Headings());
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchorAndThePublishedOnesAreStable()
    {
        var lines = Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*$").Select(m => m.Groups[1].Value.Trim()).ToList();
        Assert.Equal(18, lines.Count);
        Assert.All(lines, h => Assert.Matches(@"\{#[\w-]+\}$", h));

        // Deep-link targets for the hubs that route here.
        foreach (var anchor in new[] { "not-hospice", "why-early", "giving-up", "children", "hospice", "caregiver" })
        {
            Assert.Matches($@"(?m)^## .+ \{{#{anchor}\}}\s*$", Page);
        }
    }

    // ------------------------------------------------------- the one message

    [Fact]
    public void TheShortVersionSaysAnyStageAlongsideTreatmentAndNotHospice()
    {
        var shortVersion = PlainOf(ShortHeading);
        Assert.Contains("You can have it at any stage, at the same time as treatment for the tumor.", shortVersion, StringComparison.Ordinal);
        Assert.Contains("It is not the same as hospice, and asking for it is not giving up.", shortVersion, StringComparison.Ordinal);
        Assert.EndsWith("If your team has not offered it, you can ask for it.", shortVersion, StringComparison.Ordinal);
    }

    [Fact]
    public void TheHospiceQuestionIsAnsweredNoInItsFirstWordAndKeepsTheTreatment()
    {
        var answer = PlainOf(HospiceVsHeading);
        Assert.StartsWith("No. Hospice is one kind of palliative care.", answer, StringComparison.Ordinal);
        Assert.Contains("You can keep having treatment aimed at the tumor the whole time you have it.", answer, StringComparison.Ordinal);

        // Medicare's booklet calls palliative care "the part of hospice care": a reader who
        // goes there meets the conflation in an official voice, so the page names it and
        // scopes it in the same paragraph.
        var medicare = Paragraph(HospiceVsHeading, "Medicare's booklet");
        Assert.Contains("That booklet is only about hospice.", medicare, StringComparison.Ordinal);
    }

    [Fact]
    public void GivingUpIsDeniedAndTheHopeLineIsTheBrainTumorSocietys()
    {
        var giving = PlainOf(GivingUpHeading);
        Assert.StartsWith("No. You can keep having treatment aimed at the tumor.", giving, StringComparison.Ordinal);
        Assert.Contains("The National Brain Tumor Society says help from a palliative care team does not mean there is no more hope.",
            giving, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePalliativeTeamWorksAlongsideTheCancerTeamAndIsNotIt()
    {
        // NCI: the team "works together with the oncology care team". The dossier put
        // oncologists ON the palliative team; that is not NCI's list.
        Assert.Contains("They work with your cancer team, not instead of it.", PlainOf(TeamHeading), StringComparison.Ordinal);
        Assert.DoesNotMatch(@"(?i)\boncologists?\b", PlainOf(TeamHeading));
        Assert.DoesNotMatch(@"(?i)\b(?:replaces?|takes? over from|instead of) your (?:cancer )?(?:team|doctors?)\b",
            Ungated.Replace("not instead of it", "", StringComparison.Ordinal));
        Assert.Contains("They work alongside your cancer team. They do not replace them.",
            PlainOf(WhatHeading), StringComparison.Ordinal);
    }

    // ------------------------------------------------------- the reader-choice gate

    [Fact]
    public void HospiceDetailIsBehindTheGateAndTheGateIsInTheHospiceSection()
    {
        Assert.Single(Gate.Matches(CuratedPage.Body(Page)));
        Assert.Matches(Gate, RawSection(HospiceHeading));

        // §12.6, warn before you disclose: the heading is outside the gate, and the gate
        // is the first thing under it.
        Assert.Matches(@"^\s*:::outlook", RawSection(HospiceHeading));
    }

    [Fact]
    public void TheGatedVocabularyNeverAppearsOutsideTheGate()
    {
        // The hub pages' gate vocabulary (PediatricBrainTumorPageTests), plus the shapes a
        // hospice explanation arrives in. "hospice" itself is NOT here: the page's title is
        // the difference between the two, and it cannot be drawn without the word.
        var gatedVocabulary = new Regex(
            @"(?i)\bmedian\b|\blife expectancy\b|\bprognos\w*|\bend[- ]of[- ]life\b|\bterminal(?:ly)?\b"
            + @"|\bdie\b|\bdies\b|\bdied\b|\bdying\b|\bdeath\b|\blast stage\b|\blost someone\b"
            + @"|\bhow long\b[^.?]{0,30}\b(?:have|live|left)\b|\bmonths? to live\b|\bfinal (?:weeks|months|days)\b"
            // /review: the other shapes end-of-life wording arrives in.
            + @"|\bpass(?:ed|es)? away\b|\bincurable\b|\blast (?:few )?(?:weeks|months|days)\b|\bbereave\w*|\bgrie(?:f|ve|ving)\b"
            + @"|\blos(?:e|es|ing|t) (?:your|a|an|their) (?:child|loved one|relative|adult relative)\b");
        Assert.Matches(gatedVocabulary, "Families who lost a loved one were bereaved and grieving.");
        Assert.Matches(gatedVocabulary, "When the tumor is incurable, in the last few weeks.");
        Assert.Matches(gatedVocabulary, "Hospice is for people with six months or less to live, the final weeks of life.");
        Assert.DoesNotMatch(gatedVocabulary, Ungated);

        // And the gate does carry the hard part, or the fence is around nothing.
        Assert.Matches(gatedVocabulary, Gated);
    }

    [Fact]
    public void NoLifeExpectancyFigureIsPrintedEvenInsideTheGate()
    {
        // Medicare: "life expectancy of 6 months or less". §12.5's gate is consent to read,
        // not a licence to publish a figure, and a digit ban is not a prognosis ban, so the
        // words are banned too.
        var figure = new Regex(
            @"\d|(?i)\b(?:one|two|three|four|five|six|seven|eight|nine|ten|twelve)\s+(?:weeks?|months?|years?)\b"
            + @"|(?i)\bmonths?\b|(?i)\bweeks?\b|(?i)\bdays\b|(?i)\ba year\b|(?i)\bhalf a year\b");
        Assert.Matches(figure, "a doctor certifies six months or less");
        Assert.Matches(figure, "Hospice is for the final weeks of life.");
        Assert.Matches(figure, "a life expectancy of 6 months");
        Assert.DoesNotMatch(figure, Gated);

        Assert.Contains("your doctors have to certify that the illness is in its last stage", Gated, StringComparison.Ordinal);

        // /review: Medicare's election is "instead of other Medicare-covered treatments for
        // your terminal illness", not only cure — and most brain tumor treatment is not
        // curative, so the narrower form tells a reader their tumor treatment carries on.
        Assert.Contains("You sign a form choosing hospice instead of the other treatment Medicare would pay for to treat that illness.",
            Gated, StringComparison.Ordinal);
        Assert.Contains("apart from small charges for some medicines and for short stays that give the family a rest",
            Gated, StringComparison.Ordinal);
        Assert.Contains("medicare.gov/coverage/hospice-care", CitedUrls(), StringComparison.Ordinal);
    }

    [Fact]
    public void HospiceCanBeStoppedAndTheGateEndsOnSomethingToDo()
    {
        Assert.Contains("You can change your mind. Medicare says you always have the right to stop hospice at any time and go back to treatment.",
            Gated, StringComparison.Ordinal);
        // §12.6: never end a section on a frightening sentence.
        Assert.EndsWith("Ask them when it would be time to think about it.", Gated, StringComparison.Ordinal);
    }

    // ------------------------------------------------------- the overstatements

    [Fact]
    public void TheRegretFindingIsHospiceNotHelpAndCarriesNoCount()
    {
        // Schubart 2008: "They regretted not accepting hospice care sooner", said of "the
        // six family caregivers whose family members had died". The backlog said HELP, and
        // "6 of 25" reads as a quarter of all families when it is all six bereaved ones.
        var regret = new Regex(
            @"(?i)\b(?:wish|wished|regret|regretted)\b[^.]{0,30}\b(?:accept|accepted|start|started|got|chose|chosen)\b[^.]{0,20}\bhospice\b[^.]{0,10}\b(?:sooner|earlier)\b");
        Assert.Matches(regret, "Families wished they had started hospice earlier.");
        Assert.Matches(regret, Gated);
        // Scoped (/review): caregivers of ADULT patients, and the page's children section
        // sits just above it.
        Assert.Contains("In one small US study, all of the families who had lost an adult relative to a brain tumor",
            Gated, StringComparison.Ordinal);
        Assert.DoesNotMatch(regret, Ungated);

        Assert.DoesNotMatch(@"(?i)\b(?:help|support|palliative care) (?:sooner|earlier)\b", Plain);
        Assert.DoesNotMatch(@"(?i)\b(?:6|six) (?:of|out of|in) (?:25|twenty-five)\b|(?i)\bsix famil", Plain);
    }

    [Fact]
    public void TheConflationIsNcisMechanismAndNeverATimeCost()
    {
        // HINTS Brief 41 says the mix-up "may prevent patients and their caregivers from
        // requesting palliative care services or accepting palliative care when offered".
        // No source puts a time on it; the backlog's "costs people months" is unsourced.
        var mixup = Paragraph(HospiceVsHeading, "Many people mix the two up.");
        // HINTS: 49% of those who had heard of it. "Many", not "most".
        Assert.Contains("many adults who had heard of palliative care thought it was the same as hospice",
            mixup, StringComparison.Ordinal);
        Assert.Contains("The institute says this mix-up may stop people from asking for it, or from saying yes when it is offered.",
            mixup, StringComparison.Ordinal);

        Assert.DoesNotMatch(@"(?i)\bmonths?\b", Ungated);
        Assert.DoesNotMatch(@"(?i)\bcosts? (?:people|you|families|patients)\b", Plain);
    }

    [Fact]
    public void AscoIsStatedAtItsCurrentStrengthAndScope()
    {
        // Sanders 2024 dropped 2017's "within 8 weeks" ("changing the wording ... to early
        // in the treatment process"), and scopes the recommendation to ADVANCED cancer.
        var asco = Paragraph(EarlyHeading, "American Society of Clinical Oncology");
        Assert.Contains("people with advanced cancer should be sent to a palliative care team early", asco, StringComparison.Ordinal);
        Assert.Contains("early means not waiting until treatment aimed at the cancer stops", asco, StringComparison.Ordinal);
        Assert.Contains("Ask your team whether that includes you.", asco, StringComparison.Ordinal);

        var weeks = new Regex(
            @"(?i)\b(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s*(?:-|to)?\s*(?:\d+\s*)?weeks?\b"
            + @"|(?i)\bwithin (?:a few|a couple of) (?:weeks|months)\b");
        Assert.Matches(weeks, "within six weeks");
        Assert.Matches(weeks, "within 8 weeks of a diagnosis");
        Assert.Matches(weeks, "within eight weeks");
        Assert.DoesNotMatch(weeks, Plain);
    }

    [Fact]
    public void NoSentenceSaysPalliativeCareHelpsSomeoneWithABrainTumorLiveLonger()
    {
        var longer = PlainOf(LongerHeading);
        Assert.StartsWith("That is not known for brain tumors.", longer, StringComparison.Ordinal);
        Assert.Contains("It has not been shown for brain tumors.", longer, StringComparison.Ordinal);
        Assert.Contains("one study of people with lung cancer", longer, StringComparison.Ordinal);

        // No survival language anywhere but the heading's question — which is the shape
        // CAPC's "lived longer" and NCI's "may even prolong survival" would arrive in.
        // INCLUDING the answering section: the harness put "It may even prolong survival"
        // there, and the first version, which exempted the whole section, stayed green.
        // The answer never needs the vocabulary ("That is not known ... that it does").
        var survival = new Regex(@"(?i)\blive[sd]? longer\b|\bliving longer\b|\balive longer\b|\blonger li(?:fe|ves)\b|\bprolong\w*|\bsurviv\w*|\bextends? (?:your |their )?li(?:fe|ves)\b|\bmore time alive\b");
        Assert.Matches(survival, "Patients who received palliative care lived longer.");
        Assert.Matches(survival, "It may even prolong survival.");
        Assert.Matches(survival, "People who get it early end up living longer.");
        Assert.DoesNotMatch(survival, Plain.Replace(LongerHeading, " ", StringComparison.Ordinal));

        // "more time" is the gate's own words about hospice; outside it, it is a survival claim.
        Assert.DoesNotMatch(@"(?i)\bmore time\b", Ungated);

        // And the one benefit claim keeps NCI's "meant to" (/review: that is NCI's
        // definition, and ASCO 2024 finds recent outpatient trials mixed).
        Assert.EndsWith("The National Cancer Institute says that is what it is meant to improve.",
            longer, StringComparison.Ordinal);
    }

    [Fact]
    public void MedicareCoverageOfPalliativeCareKeepsNcisSome()
    {
        // Only NCI says Medicare pays for palliative care outside hospice, and it says
        // "some kinds". No medicare.gov page says it at all.
        var cost = Paragraph(CostHeading, "Medicare and Medicaid");
        Assert.Contains("The National Cancer Institute says", cost, StringComparison.Ordinal);
        Assert.Contains("Medicare and Medicaid pay for some kinds of it", cost, StringComparison.Ordinal);

        Assert.StartsWith("Most insurance pays for some palliative care, but what is covered differs.",
            PlainOf(CostHeading), StringComparison.Ordinal);
        var strong = new Regex(
            // The lookbehind lets the questions list ask "Does my insurance pay for it?".
            @"(?i)(?<!\b(?:does|do|will|would|can) (?:my |your )?)\b(?:Medicare|Medicaid|insurance)(?: and (?:Medicare|Medicaid))? (?:covers?|pays? for|will pay for) (?:all |your )?(?:palliative care|it)\b"
            + @"|(?i)\bfully covered\b|(?i)\bfree of charge\b");
        Assert.Matches(strong, "Medicare covers palliative care.");
        Assert.Matches(strong, "Medicare and Medicaid cover it.");
        Assert.Matches(strong, "Insurance pays for palliative care.");
        Assert.DoesNotMatch(strong, "Does my insurance pay for it?");
        Assert.DoesNotMatch(strong, Plain);
    }

    [Fact]
    public void CureIsMedicaresWordAndOnlyWhereTheRuleIsMedicares()
    {
        // For most malignant brain tumors no treatment is curative (Hertler), so "palliative
        // care goes alongside treatment to cure the tumor" would tell many readers it is
        // not for them. The word belongs to Medicare's rule, quoted inside the gate.
        Assert.DoesNotMatch(@"(?i)\bcur(?:e|es|ed|ing|ative)\b", Ungated);
        var withCure = ParagraphsOf(RawSection(HospiceHeading))
            .Where(p => Regex.IsMatch(p, @"(?i)\bcur(?:e|es|ed|ing|ative)\b")).ToList();
        Assert.NotEmpty(withCure);
        Assert.All(withCure, p => Assert.Contains("Medicare", p, StringComparison.Ordinal));
    }

    // ------------------------------------------------------- children

    [Fact]
    public void ConcurrentCareKeepsItsScope()
    {
        // CMS SMD #10-018: section 2302 is a Medicaid/CHIP rule for children under 21.
        // It does not reach private insurance or adults on Medicare.
        var rule = Paragraph(ChildrenHeading, "one rule that is different for children");
        // /review: stand-alone CHIP programs only have the OPTION, and the child still has
        // to qualify for hospice.
        Assert.Contains("a child under 21 who has Medicaid, and who qualifies for hospice, can have hospice and keep having treatment aimed at the tumor at the same time",
            rule, StringComparison.Ordinal);
        Assert.Contains("Some CHIP plans work the same way.", rule, StringComparison.Ordinal);
        Assert.Contains("If your child has CHIP or other insurance, ask how its rules work.", rule, StringComparison.Ordinal);

        // The only digits on the whole page are that age (the step list's markers aside).
        var words = Regex.Replace(Page, @"(?m)^\d+\.[ \t]", "");
        Assert.Equal(["21"], Regex.Matches(
                Regex.Replace(CuratedPage.Flatten(CuratedPage.EverythingAReaderMeets(words)), @"[*_]", ""), @"\d+")
            .Select(m => m.Value).Distinct().ToArray());
    }

    // ------------------------------------------------------- doors in

    [Theory]
    [InlineData("start.md")]
    [InlineData("tumors/dipg.md")]
    [InlineData("tumors/diffuse-midline-glioma.md")]
    [InlineData("tumors/pediatric-brain-tumor.md")]
    public void ThePagesThatRaisePalliativeCareRouteHere(string page)
    {
        Assert.Contains("](/treatments/palliative-care)",
            CuratedPage.Read(page.Split('/')), StringComparison.Ordinal);
    }

    // ------------------------------------------------------- source discipline

    [Fact]
    public void TheSourcesThatAreGoneWrongOrOutOfScopeAreNotCited()
    {
        var urls = CitedUrls();
        Assert.Equal(16, urls.Split('\n').Length);

        // Cancer.Net is gone; ABTA's hospice facts contradict Medicare; CaringInfo no
        // longer carries the line the dossier cited it for.
        foreach (var banned in new[] { "cancer.net", "abta.org", "caringinfo.org", "nhpco.org", "ascopubs.org" })
        {
            Assert.DoesNotContain(banned, urls, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("capc.org/documents/download/475", urls, StringComparison.Ordinal); // ASCO 2024
        Assert.Contains("PMC2600839", urls, StringComparison.Ordinal);                     // Schubart
        Assert.Contains("SMD10018", urls, StringComparison.Ordinal);                       // concurrent care
        Assert.Contains("HINTS_Brief_41", urls, StringComparison.Ordinal);                 // the mix-up
    }

    [Fact]
    public void EveryUrlInTheFrontMatterCarriesATitleAndAnAccessedDate()
    {
        var front = CuratedPage.FrontMatter(Page);
        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;
        Assert.Equal(urls, Regex.Matches(front, @"(?m)^    title: "".+""\s*$").Count);
        Assert.Equal(urls, Regex.Matches(front, @"(?m)^    accessed: \d{4}-\d{2}-\d{2}\s*$").Count);
        Assert.Matches(@"(?m)^disclaimers: \[medical\]\s*$", front);
    }

    // ------------------------------------------------------- corpus housekeeping

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn() =>
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    [Fact]
    public void ThePageUsesNoBritishSpellingsOrIdiom()
    {
        var offenders = CuratedPage.BritishForms
            .Where(form => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(form)}"))
            .Where(form => !CuratedPage.BritishFormExemptions.Any(
                ex => Regex.IsMatch(Plain, $@"(?i)\b{Regex.Escape(ex)}")
                      && ex.Contains(form, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Assert.True(offenders.Count == 0, "British forms on a page written for a US reader: " + string.Join(", ", offenders));
        Assert.Empty(CuratedPage.BritishIdiomsIn(Plain));
    }

    [Fact]
    public void ThePageNeverCharacterisesAnOutcomeOrMinimisesTheTreatment()
    {
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertNeverMinimises(Plain, Slug);
    }
}

/// <summary>
/// WI-555: the same page, against the running site.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class PalliativeCarePageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/palliative-care";

    private readonly WebApplicationFactory<Program> factory;

    public PalliativeCarePageRenderTests(WebApplicationFactory<Program> raw) =>
        factory = raw.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedWithItsAnchors()
    {
        var response = await factory.CreateClient().GetAsync(Url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.Contains("id=\"not-hospice\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"hospice\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheHospiceSectionRendersClosedBehindTheReaderChoice()
    {
        var html = await factory.CreateClient().GetStringAsync(Url);
        var at = html.IndexOf("id=\"hospice\"", StringComparison.Ordinal);
        Assert.True(at > 0);

        // The gate is a <details> with no `open`, and the hospice detail is inside it.
        var details = html.IndexOf("<details", at, StringComparison.Ordinal);
        var detailsEnd = html.IndexOf("</details>", at, StringComparison.Ordinal);
        Assert.True(details > at && detailsEnd > details, "the hospice section did not render a reader-choice gate");
        Assert.DoesNotMatch(@"<details[^>]*\bopen\b", html[details..html.IndexOf('>', details)]);
        Assert.Contains("You can change your mind.", html[details..detailsEnd], StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        await CuratedPage.AssertLinksResolve(factory.CreateClient(), Url,
            "/treatments/steroids",
            "/treatments/anti-seizure-medicines",
            "/tumors/pediatric-brain-tumor",
            "/get-help-now");
    }

    [Fact]
    public async Task TheStartPageIsADoorForTheNewlyDiagnosedReader()
    {
        var html = await factory.CreateClient().GetStringAsync("/start");
        Assert.Contains("href=\"/treatments/palliative-care\"", html, StringComparison.Ordinal);
    }
}
