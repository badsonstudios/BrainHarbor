using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-513: low-grade glioma, deepened — the TEMPLATE PROOF. One tumor hub taken
/// all the way before the pattern is copied twenty-three more times.
///
/// This page follows **§12.3's seventeen-section hub order**, NOT §12.8's
/// twelve-slot library template. Seven pages in a row have used the library one
/// and the two are easy to confuse; the difference is that a hub answers "what
/// is wrong with me" and a library page answers "what is about to happen to me".
///
/// Two firsts, both pinned below:
/// <list type="bullet">
/// <item>The first page in the corpus to use the <c>:::outlook</c> reader-choice
/// gate. WI-503 built it and nothing had ever used it, so its five documented
/// fail-open modes had never been exercised on a real page.</item>
/// <item>The first tumor hub to link into the tests and treatment libraries.
/// Every library page's test says "/start is currently this page's only door";
/// this is what closes that loop.</item>
/// </list>
/// </summary>
public sealed class LowGradeGliomaPageContentTests
{
    private static string Page => CuratedPage.Read("tumors", "low-grade-glioma.md");

    private static string Reader => CuratedPage.ReaderText(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private const string GradeHeading = "Is it cancer? What does the grade mean?";
    private const string OverrideHeading = "Why does my report say grade 4 when the scan looked low grade?";
    private const string WatchHeading = "How is it usually treated?";
    private const string CaregiverHeading = "For the person caring for someone with this";

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo()
    {
        // WI-563. This page carried a COMPRESSED one-paragraph version of the
        // escalation material rather than the twelve lines its three siblings
        // shared, and the compression is what hid the gap: it mentioned
        // chemotherapy four times and contained the word "fever" only inside a
        // link label, so a grade-2 reader on temozolomide met no fever rule
        // anywhere on the page. That is §12.11's defect, live.
        //
        // Switching it to the block was a deliberate call, not a tidy-up.
        // §12.10 warns that moving a page onto a shared block can be a
        // regression dressed as factoring, so the test was: is any of the
        // twelve lines wrong for a grade-2 patient? None is — the tiers sort
        // SYMPTOMS, not prognosis, and a grade-2 glioma's hallmark is exactly
        // the seizure the ambulance tier is built around. A shorter emergency
        // list for the less acutely ill reader is the over-reassuring
        // direction, which §12.12 records as the more dangerous one.
        CuratedPage.AssertEscalationTiers(Page, "tumors/low-grade-glioma", "What symptoms does it cause?");
    }

    [Fact]
    public void TheGradeSectionSaysGradeOneAndGradeTwoAreNotTheSameThing()
    {
        // The reason this page exists. The old 202-word version said "Doctors
        // grade brain tumors from 1 to 4. Low grade means grade 1 or grade 2",
        // which is the conflation the item was written to dismantle: grade 1
        // gliomas are circumscribed and often cured by surgery, grade 2 diffuse
        // gliomas are malignant and not curable. Being told "low grade" without
        // being told which one leaves a reader with the wrong picture entirely.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(new Regex(@"[Gg]rade 1 gliomas are a different situation", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"circumscribed", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Gg]rade 2 diffuse gliomas are not that", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"malignant, and with today's treatments they are not curable",
            RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheGradeSectionDoesNotLeaveTheReaderOnTheHardestSentence()
    {
        // §12.6: never end a section on a frightening sentence, and this is the
        // bluntest thing the site says anywhere. "Not curable" has to be
        // followed by what it does and does not mean, or the page has told
        // somebody their situation is hopeless when it is not.
        var sentences = CuratedPage.SentencesOf(Section(GradeHeading));
        var tail = string.Join(" ", sentences[^3..]);

        Assert.DoesNotMatch(
            new Regex(@"not curable\.?$|malignant\.?$", RegexOptions.IgnoreCase), sentences[^1]);
        Assert.Matches(
            new Regex(@"[Nn]ot curable is not the same\s*as not treatable|long relationship|goes\s*through the grading",
                RegexOptions.IgnoreCase),
            CuratedPage.Flatten(tail));
    }

    [Fact]
    public void TheGradeSectionDoesNotBlameTheReadersOwnDoctor()
    {
        // The page repeats phrases patients were told before referral — "this
        // is the good kind", "we got it all". Printing those without the next
        // paragraph would turn a page about a tumor into a page about somebody
        // the reader has to keep seeing. The source is a neuro-oncologist
        // describing a communication failure, not an accusation.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(new Regex(@"this is the good kind", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"they were not lying to you", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageRoutesTheReaderToTheirActualDiagnosis()
    {
        // The research is explicit that this page has two jobs: explain the
        // term because everyone uses it, and immediately push the reader toward
        // the diagnosis that actually applies to them. WHO recommends against
        // the label itself, so a page that only explains it has done half a job.
        var section = CuratedPage.Flatten(Section("What is a low-grade glioma?"));

        Assert.Matches(new Regex(@"[Ii]t is not the name\s*of a tumor", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"advised against using the term", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"first thing to do is find your real diagnosis", RegexOptions.IgnoreCase), section);
        Assert.Contains("/tests/pathology-report", section, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePreCns5SourceIsNeverUsedForNamingOrGrading()
    {
        // §12.1's per-claim rule, and this page is where it bites hardest.
        // CancerNetwork carries the best material on the site for what patients
        // get told and how it lands — but it cites the 2007 WHO edition, uses
        // Roman numerals throughout, contains "oligoastrocytoma", and mentions
        // IDH exactly zero times. It is cited here for the framing and never
        // for the architecture, so the page must carry no trace of its
        // vocabulary.
        // NEGATION-AWARE, and that distinction is the whole point. There are two
        // completely different things a retired name can be doing on a page:
        // used as a live diagnosis (forbidden, contract item 3), or NAMED as
        // retired so a reader holding old paperwork can find themselves
        // (required, by the same contract item and by research §0.3, which
        // calls the crosswalk "the single highest-value block on the site").
        //
        // The first version banned the strings outright. It would have passed
        // this page and then forbidden the crosswalk on all 23 hubs that copy
        // it — foreclosing the highest-value block on the site with a test
        // whose comment said it was enforcing CNS5. Same exemption shape as
        // the Roman-numeral guard, which allows the one sentence that teaches
        // the old notation.
        foreach (var retired in new[] { "oligoastrocytoma", "anaplastic", "mixed glioma" })
        {
            foreach (Match match in Regex.Matches(Reader, Regex.Escape(retired), RegexOptions.IgnoreCase))
            {
                var window = Reader[Math.Max(0, match.Index - 320)
                                    ..Math.Min(Reader.Length, match.Index + 320)];
                Assert.True(
                    Regex.IsMatch(window, @"retired|no longer|old rules|older paperwork|before that|"
                                        + @"changed in 2021|used to be", RegexOptions.IgnoreCase),
                    $"'{retired}' is used as a live diagnosis rather than named as a retired term");
            }
        }

        // Arabic grades only. A single Roman numeral here would mean the
        // pre-CNS5 source had leaked into the grading.
        // Same negation-aware treatment as the retired names above, and for
        // the same reason: the crosswalk has to be able to PRINT "grade II" in
        // order to tell a reader it is the old way of writing grade 2.
        // IgnoreCase added by WI-516: written case-sensitive, this walked past
        // the sentence-initial "Grade IV", which is the commonest form.
        foreach (Match match in Regex.Matches(
                     Reader, @"\bgrade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase))
        {
            var window = Reader[Math.Max(0, match.Index - 320)
                                ..Math.Min(Reader.Length, match.Index + 320)];
            Assert.True(
                Regex.IsMatch(window, @"retired|no longer|old rules|older paperwork|now written",
                    RegexOptions.IgnoreCase),
                "a Roman-numeral grade is used as a live grade rather than shown as the old style");
        }
    }

    [Fact]
    public void TheMolecularOverrideSectionExplainsBothWaysAGradeFourArrives()
    {
        // The research calls this "a specific and terrifying experience that
        // this page must address head-on": the scan looked mild, everyone was
        // cautiously reassuring, and the report says grade 4. There are two
        // separate routes and a reader needs whichever is theirs.
        var section = CuratedPage.Flatten(Section(OverrideHeading));

        // The CLAIMS, not the tokens. The first version asserted the section
        // merely contained "CDKN2A/B" — and it does, further down, in the
        // sentence pointing at /tests/molecular-markers. Deleting the whole
        // grade-4 explanation left the test green. Same shape as the
        // whole-page Contains() defects WI-506, WI-508 and WI-511 each found
        // in their own suites.
        Assert.Matches(
            new Regex(@"IDH-wildtype and carries\s*certain gene changes", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"IDH-mutant astrocytoma is .{0,60}both copies.{0,20}gone",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"[Ii]t is not a mistake, and\s*nobody was hiding anything", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheWatchAndWaitSectionSaysItIsAPlanRatherThanADelay()
    {
        // The commonest doubt on this page, and the one that makes people ask
        // for treatment they do not need. The source says observation is "a
        // reasonable option for low-risk patients"; what makes that liveable is
        // knowing what it consists of and why it is chosen.
        var section = CuratedPage.Flatten(Section(WatchHeading));

        Assert.Matches(new Regex(@"[Ww]atching is not doing nothing", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"scans at set times", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Aa]sk what specifically would change the plan", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheAgeFortyLineIsPresentedAsTrialCriteriaAndNotAsARule()
    {
        // The item names this specifically, and the source says it in as many
        // words: RTOG 9802's criteria were "designed to enroll patients who
        // reasonably could receive postoperative treatment, and not to identify
        // patients who require" it, and "there is no clinical justification for
        // a strict age cutoff".
        //
        // Polarity matters. Printing "over 40 is high risk" and stopping is
        // worse than not raising it, because the reader then applies it to
        // themselves as a rule. Every mention has to carry the correction.
        var section = CuratedPage.Flatten(Section(WatchHeading));

        Assert.Matches(new Regex(@"age line around 40", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"came from a clinical trial", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Nn]ot written to decide who needs treatment", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"no clinical reason for a\s*strict age cutoff", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheVorasidenibSectionCarriesTheApprovalAndTellsTheReaderWhatToAsk()
    {
        // The currency proof for the whole library: FDA approval on 6 August
        // 2024, absent from most patient material written before then. It turns
        // entirely on the reader's IDH result, which is exactly the sort of
        // detail that sits in a report unremarked, so the page hands over the
        // question rather than just the fact.
        var section = CuratedPage.Flatten(Section(WatchHeading));

        Assert.Matches(new Regex(@"vorasidenib", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"August 2024", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"aged 12 and\s*over", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Aa]m I someone vorasidenib\s*could apply to", RegexOptions.IgnoreCase), section);

        // And the condition, because the approval is narrower than "low-grade
        // glioma": grade 2, IDH-mutant, after surgery.
        Assert.Matches(new Regex(@"grade 2 astrocytoma or oligodendroglioma", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheOutlookSectionSitsBehindTheReaderChoiceGate()
    {
        // §12.5 and WI-503. This is the FIRST page in the corpus to use the
        // gate, so the shape is pinned here rather than assumed: the heading
        // stays OUTSIDE the fence, so the page outline is complete and the
        // reader meets heading, then warning, then choice, in that order.
        Assert.Matches(
            new Regex(@"^## What might happen over time\s*$\r?\n\r?\n:::outlook\r?$",
                RegexOptions.Multiline),
            Page);

        // Exactly one gate, and it closes. WI-503 documents five ways a
        // mistyped fence publishes the content wide open with a green build.
        Assert.Equal(1, Regex.Matches(Page, @"^:::outlook\s*$", RegexOptions.Multiline).Count);
        Assert.Equal(2, Regex.Matches(Page, @"^:::", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheOutlookSectionCarriesNoNumbersAndSaysWhyTheOldOnesDoNotFit()
    {
        // §12.5: explain the concepts, publish no figures. The honest reason is
        // specific to this page and worth giving: survival figures for
        // low-grade glioma describe groups diagnosed before the gene tests that
        // now define the tumor, and before vorasidenib. A number from one of
        // those groups is describing a different illness.
        var outlook = Regex.Match(Page, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(outlook.Success, "the outlook gate is missing or unclosed");

        var inside = CuratedPage.Flatten(outlook.Groups[1].Value);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*%", inside);
        Assert.DoesNotMatch(
            new Regex(@"\d+[- ]year survival|\bfive[- ]year\b", RegexOptions.IgnoreCase), inside);

        // "median" is TAUGHT here, not banned. The first version forbade the
        // word — which would have cemented, across 23 copies, a gate that
        // publishes no figures and explains none of the vocabulary §12.5 asks
        // for. The Kirkebøen framing needs all four moves: establish it
        // describes a GROUP, present the distribution rather than the midpoint,
        // name the right skew, and say plainly it is not a prediction about one
        // person.
        Assert.Matches(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"middle of a group", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"not a prediction about you", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"long tail of people who do much better", RegexOptions.IgnoreCase), inside);

        // And the right tail belongs INSIDE. It was in section 2 on the first
        // draft, where a reader who declined outlook met the most
        // hope-preserving sentence on the page anyway — the exact thing the
        // gate exists to put behind a choice.
        Assert.Matches(
            new Regex(@"live many years with a grade 2 glioma", RegexOptions.IgnoreCase), inside);
        Assert.Matches(
            new Regex(@"before the gene tests\s*that now define your tumor", RegexOptions.IgnoreCase), inside);

        // And it hands the decision back rather than closing it off.
        Assert.Matches(
            new Regex(@"rather not have it today|ask your own team", RegexOptions.IgnoreCase), inside);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFigureAnywhereAtAll()
    {
        // Shared contract item 5, which reaches inside the gate too (§12.5).
        // The research offers several tempting figures — the proportion found
        // incidentally, the proportion of pilocytic astrocytomas that are
        // MAPK-driven — and none of them belong on a patient page.
        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*%", Reader);
        Assert.DoesNotMatch(
            new Regex(@"\b\d+(\.\d+)?\s*(Gy|Gray|mg|ml)\b", RegexOptions.IgnoreCase), Reader);
        // "median" is allowed on this page now, and only inside the gate, where
        // it is explained rather than used — the gate's own test pins that.
        // What stays banned is a FIGURE, in digits or in words.
        //
        // The proportion clause is wider than WI-511's and WI-512's, which both
        // required a verb: "most people are looking at years rather than
        // months" slipped straight through those, inside the gate, on the first
        // draft of this page. A proportion attached to people is a number
        // whatever verb follows it.
        Assert.DoesNotMatch(
            new Regex(@"\bper ?cent\b|\bpercent\b"
                    + @"|\b(most|nearly all|almost all|the majority of)\s+(people|patients)\b"
                    + @"|\b(a|one|two|three)\s+(third|quarter|half|fifth)s?\s+of\s+(people|patients)\b",
                RegexOptions.IgnoreCase),
            Reader);
    }

    [Fact]
    public void ThePageNeverCharacterisesTheOutcome()
    {
        // The shared §12.8 ban list. Harder here than anywhere: this page
        // prints "this is the good kind" as a phrase people are WRONGLY told,
        // so the list has to survive the page that quotes it.
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageFollowsTheSeventeenSectionHubOrderAndNotTheLibraryTemplate()
    {
        // §12.3, and the confusion this item exists to prevent. Seven library
        // pages in a row have used §12.8's twelve slots, and copying that here
        // would produce a page that answers "what is about to happen to me"
        // for a reader asking "what is wrong with me".
        var headings = Headings();

        Assert.Equal("The short version", headings[0]);

        // ORDER, not set membership. The first version used Assert.Contains on
        // each heading, so moving "How is it usually treated?" above "What
        // symptoms does it cause?" left it green — on the one page whose
        // entire purpose is to fix the shape before it is copied 23 times.
        var last = -1;
        foreach (var required in new[]
                 {
                     "What is a low-grade glioma?",
                     GradeHeading,
                     "Where does it grow, and why does it cause these symptoms?",
                     "What symptoms does it cause?",
                     "How do doctors find out it is this?",
                     "What do the words on my report mean?",
                     "How is it usually treated?",
                     "What is treatment actually like, and what is normal afterwards?",
                     "Everyday life: work, driving, seizures and tiredness",
                     "Follow-up scans, and what to do while you wait",
                     "If it comes back, or changes",
                     "What might happen over time",
                     CaregiverHeading,
                     "What to ask your team",
                     "Where to get support",
                 })
        {
            var at = headings.IndexOf(required);
            Assert.True(at > last, $"'{required}' is out of §12.3 order");
            last = at;
        }

        // The library template's tells. None of these belong on a hub.
        foreach (var librarySlot in new[]
                 {
                     "What happens, step by step", "How long does it take?",
                     "What you need first, and what to bring",
                     "Who reads it, and how do I get the result?",
                     "Where to go next",
                 })
        {
            Assert.DoesNotContain(librarySlot, headings);
        }
    }

    [Fact]
    public void OutlookSitsAfterEverythingActionableAndBeforeTheCaregiverSection()
    {
        // §12.3's whole design principle: "action at the end of every
        // frightening block", and outlook at position 12 so the reader has
        // everything they can act on before meeting anything frightening. The
        // ordering is invisible in review — the section reads fine anywhere —
        // which is why it is pinned by index.
        var headings = Headings();

        var recurrence = headings.IndexOf("If it comes back, or changes");
        var causes = headings.IndexOf("Did I cause this?");
        var outlook = headings.IndexOf("What might happen over time");
        var caregiver = headings.IndexOf(CaregiverHeading);
        var questions = headings.IndexOf("What to ask your team");

        Assert.True(recurrence >= 0 && causes >= 0 && outlook >= 0 && caregiver >= 0 && questions >= 0,
            "one of the §12.3 sections around the outlook gate has been renamed");

        // §12.3 has no numbered slot for self-blame — it says the block "still
        // appears; it just is not near the top". Demoted to just before the
        // gate: late enough not to greet a frightened reader with "nobody
        // knows", early enough that they meet it before outlook.
        Assert.Equal(recurrence + 1, causes);
        Assert.Equal(causes + 1, outlook);
        Assert.Equal(outlook + 1, caregiver);
        Assert.True(questions > caregiver, "the printable questions no longer come last but one");
    }

    [Fact]
    public void TheCaregiverSectionIsAboutYearsRatherThanWeeks()
    {
        // §12.7 and contract item 11. What makes this caregiver section
        // different from the treatment pages' is the timescale: this is not a
        // few hard weeks and then recovery. For this tumor it is living
        // alongside something, often for years, with seizures in the middle
        // of it.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        Assert.Contains("[CAREGIVER]", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"years of living alongside", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Ss]can weeks are hard", RegexOptions.IgnoreCase), section);

        // Watching is hard to live with for the person alongside too, and
        // saying so is the thing no comparator does.
        //
        // WI-522 CORRECTED THIS FROM A COMPARATIVE. It used to read "Watching is
        // HARDER to live with THAN TREATING", which no source on this page
        // supports — and the sources /treatments/watch-and-wait checked point
        // the other way: the one study behind the "watching costs you" claim
        // concludes distress is high "independent of management strategy",
        // QUALMS found "similar HRQoL" watched and operated, and the same
        // cohort re-surveyed found LESS distress in the watched group. Two
        // pages stating one claim at two strengths is §12.10's defect, so the
        // comparative is banned here rather than merely no longer pinned.
        Assert.Matches(
            new Regex(@"[Ww]atching is hard to live with", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(
            new Regex(@"harder to live with than|harder than treat|in a way that treat\w*|"
                + @"(?:harder|worse|tougher)\s+than\s+(?:treating|treatment|having treatment|an operation)",
                RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(Page)));
    }

    [Fact]
    public void TheCaregiverSectionLinksTheSeizurePageForTheEmergencyRatherThanRestatingIt()
    {
        // /seizures/what-to-do owns the emergency, and it is the page that says
        // which seizures need an ambulance and which do not. A hub restating
        // that list is a second copy to keep in step (§12.8), and getting it
        // wrong here would be the WI-511 under-triage blocker on a page whose
        // readers live with seizures for years.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        Assert.Contains("/seizures/what-to-do", section, StringComparison.Ordinal);

        // No competing list.
        Assert.DoesNotMatch(
            new Regex(@"[Cc]all an ambulance if|When to call an ambulance|Call 9-?1-?1", RegexOptions.IgnoreCase),
            section);

        // But it must say the distinction exists, or a caregiver treats every
        // seizure as an emergency.
        Assert.Matches(
            new Regex(@"which seizures need an ambulance and which do not", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageLinksIntoBothLibrariesRatherThanReExplainingThem()
    {
        // The other half of what this item proves. No tumor hub linked into the
        // libraries before this one, and the whole point of building seven
        // library pages first was so a hub could route instead of repeating.
        // Scoped to the sections that carry the claim, not to the page. The
        // first version checked the whole page, and every treatment link also
        // appears in "Where to get support" — so deleting the chemotherapy
        // door from the treatment list left the test green. A hub that names a
        // treatment and does not open the door beside it has done the thing
        // this test is named for.
        var diagnosis = Section("How do doctors find out it is this?");
        foreach (var door in new[]
                 {
                     "/tests/mri", "/tests/waiting-for-results", "/tests/pathology-report",
                     "/tests/molecular-markers",
                 })
        {
            Assert.Contains(door, diagnosis, StringComparison.Ordinal);
        }

        var treatment = Section("How is it usually treated?");
        foreach (var door in new[]
                 {
                     "/treatments/craniotomy", "/treatments/radiation-therapy",
                     "/treatments/chemotherapy",
                 })
        {
            Assert.Contains(door, treatment, StringComparison.Ordinal);
        }

        // And it must not re-explain what those pages own.
        foreach (var owned in new[]
                 {
                     "warm washcloth", "bone flap", "linear accelerator", "somnolence",
                     "100.4", "nadir", "gross total resection",
                 })
        {
            Assert.DoesNotContain(owned, Reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheDrivingRulesAreNeverStated()
    {
        // §12.3 section 9 and the line WI-451, WI-525 and WI-560 all hold:
        // driving law after a seizure is jurisdictional, so a page that names
        // a duration is wrong for most of its readers. Route to a person.
        var section = CuratedPage.Flatten(
            Section("Everyday life: work, driving, seizures and tiredness"));

        Assert.Matches(
            new Regex(@"depend entirely on where\s*you live", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(
            new Regex(@"\b(six|three|twelve|6|3|12)\s+months?\b.{0,40}driv|driv.{0,40}\b(six|three|twelve|6|3|12)\s+months?\b",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void ThePageCarriesTheSelfBlameBlockAndKeepsItLow()
    {
        // §12.3's "Deliberate omissions" paragraph: "The self-blame block still
        // appears; it just is not near the top." It was missing from the first
        // draft and from the entire corpus — research §0.5 gives it a dedicated
        // section, and docs/backlog.md says in as many words that self-blame
        // belongs to §12.3's block and to this item.
        //
        // Position is asserted because the whole rule is about position: a
        // reader's first three screens should not be "we do not know what
        // caused this".
        var headings = Headings();
        var causes = headings.IndexOf("Did I cause this?");

        Assert.True(causes >= 0, "the self-blame section is missing");
        Assert.True(causes > headings.Count / 2,
            "the self-blame section is in the top half of the page (§12.3 demotes it)");

        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageNamesTheRetiredTermsAReaderMayBeHolding()
    {
        // Contract item 3: "Where an older name was retired, say so: readers
        // arrive holding old paperwork." Research §0.3 calls the crosswalk the
        // single highest-value block on the site, and this page's readers are
        // exactly the ones holding a pre-2021 report.
        //
        // WI-514 moved the CNS5-wide renames into the shared [CROSSWALK] block
        // (SYNTHESIS §4.3: canonical home is the glioma umbrella, "with the
        // per-tumor slice repeated only where it differs"). So this asserts
        // against the COMPOSED page. Asserting the raw source here would be
        // asserting against the literal characters "[CROSSWALK]".
        var raw = CuratedPage.Flatten(Section("What is a low-grade glioma?"));
        var section = CuratedPage.Flatten(
            CuratedPage.ComposedSection(Page, "What is a low-grade glioma?"));

        // The page's own slice: the two names THIS group's readers hold, called
        // out before the shared block rather than left for them to find.
        Assert.Matches(new Regex(@"diffuse astrocytoma", RegexOptions.IgnoreCase), raw);
        Assert.Matches(new Regex(@"[Oo]ligoastrocytoma", RegexOptions.IgnoreCase), raw);

        // And the shared block is actually pulled in, carrying the renames and
        // the Roman-numeral line this page used to spell out itself.
        Assert.Contains("[CROSSWALK]", Section("What is a low-grade glioma?"), StringComparison.Ordinal);
        Assert.Matches(new Regex(@"grade II", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"names for these tumors changed|rules .{0,30}changed in 2021",
            RegexOptions.IgnoreCase), section);

        // And it must not read as an accusation against whoever wrote the old
        // report — the same move the grade section makes about "the good kind".
        Assert.Matches(
            new Regex(@"does not mean it was wrong", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheNciPageTheOldVersionCitedForGradingIsGone()
    {
        // The page this replaces cited cancer.gov's "Brain and Spinal Cord
        // Tumors" for its grading claim. §12.1 forbids NCI patient PDQ for
        // naming and grading outright — it is pre-CNS5, still treats retired
        // names as live diagnoses, and uses Roman numerals. That citation went
        // with the rewrite and must not come back.
        var front = CuratedPage.FrontMatter(Page);

        Assert.DoesNotContain("cancer.gov/types/brain", front, StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())];
}

/// <summary>The page as served, the gate as it actually behaves, and the doors.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class LowGradeGliomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/low-grade-glioma";

    private readonly WebApplicationFactory<Program> _factory;

    public LowGradeGliomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        // Contract item 10: existing /tumors/* URLs are preserved. This page
        // was rewritten from 202 words to the full hub, and the URL is the one
        // thing about it that must not change.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // Asserted against the ARTICLE, not the whole document. "not one
        // diagnosis" is also in the page's meta description, so a whole-page
        // Contains could not fail even with the opening rewritten away.
        var start = html.IndexOf("<article", StringComparison.Ordinal);
        var end = html.IndexOf("</article>", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "the page did not render an article");
        var article = html[start..end];

        Assert.Contains("Low-grade glioma", article, StringComparison.Ordinal);
        Assert.Contains("is not one diagnosis", article, StringComparison.Ordinal);
        Assert.Contains("a label covering", article, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheOutlookGateRendersClosedAndDoesNotLeakItsContents()
    {
        // The first real exercise of WI-503's gate. Two things matter and only
        // one of them is about markup: the section has to render as a closed
        // disclosure, AND the words inside must not be sitting in the page as
        // ordinary prose. Every one of WI-503's five documented fail-open modes
        // produces a green build and a visible outlook section, so "the build
        // passed" proves nothing here.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("reader-gate__disclosure", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", html, StringComparison.Ordinal);

        // The component emits the warning and the label, not the page.
        Assert.Contains("The next part is about outlook", html, StringComparison.Ordinal);

        // And the fence itself must not have survived into the output, which is
        // what happens when Markdig meets a container name it does not render.
        Assert.DoesNotContain(":::", html, StringComparison.Ordinal);
        Assert.DoesNotContain("outlook\n", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheHeadingStaysOutsideTheGateSoTheOutlineIsComplete()
    {
        // §12.5: the reader meets heading, then warning, then choice. If the
        // heading were inside the fence it would disappear from the page
        // outline, and a reader skimming headings would never learn the section
        // existed to decline.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // Scoped to the gate's own markup. The first version compared against
        // the FIRST "<details" in the document, and the layout has disclosures
        // of its own, so it failed on a page where the gate was rendering
        // perfectly. An assertion about position has to be anchored to the
        // thing whose position is in question.
        var heading = html.IndexOf("id=\"what-might-happen-over-time\"", StringComparison.Ordinal);
        var gate = html.IndexOf("reader-gate__disclosure", StringComparison.Ordinal);

        Assert.True(heading > 0, "the outlook heading is not on the rendered page");
        Assert.True(gate > 0, "the reader-choice gate did not render");
        Assert.True(gate > heading, "the outlook heading has been swallowed into the disclosure");
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors this hub owes its reader. It is the first hub to have any.
        // "where-to-get-support" rather than the default: a §12.3 hub ends with
        // section 15, not with §12.8's "Where to go next". The helper had that
        // id hard-coded until this page called it.
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/seizures/living-with", "/seizures/what-to-do",
            "/treatments/craniotomy", "/treatments/radiation-therapy",
            "/treatments/chemotherapy");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // New check, and it caught a real defect on its first run: the draft
        // linked /tests/pathology-report#grade, and that page had no #grade
        // anchor — AssertLinksResolve strips the fragment, so the link came
        // back a healthy 200 while landing the reader at the top of a long
        // page. WI-508's rule says an anchor becomes a published interface once
        // another page deep-links it; this is the first page that ever did, so
        // the anchor is now explicit on the target and this check pins it.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheSharedCaregiverBlockComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.DoesNotContain("[CAREGIVER]", html, StringComparison.Ordinal);
        Assert.DoesNotContain("[ESCALATION]", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAlreadyDefinedVocabularyFiresAsTooltips()
    {
        // Contract item 9. This page names the markers constantly and defines
        // none of them — /tests/molecular-markers owns them as of WI-509 — so
        // the tooltip is how a reader gets the meaning without leaving.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "idh-gene-change", "1p-19q-co-deletion", "cdkn2a-b-deletion" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheWordsThisPageDefinesItselfDoNotAlsoFireATooltip()
    {
        var page = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[]
                 {
                     "transformation", "watch-and-wait", "vorasidenib",
                     "circumscribed", "supratentorial",
                 })
        {
            Assert.DoesNotContain($"def-{slug}\"", page, StringComparison.Ordinal);
        }

        // A suppression only means something if the word is there to suppress —
        // WI-512 shipped two that suppressed nothing.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "low-grade-glioma.md")));

        // The EXACT terms, all five. The first version checked "watch" rather
        // than "watch and wait" and omitted "supratentorial" altogether — so
        // the guard written to catch WI-512's dead suppressions was itself
        // tuned around two more of them. "watch" passes trivially on
        // "watching".
        foreach (var term in new[]
                 {
                     "transformation", "watch and wait", "vorasidenib",
                     "circumscribed", "supratentorial",
                 })
        {
            Assert.True(reader.Contains(term, StringComparison.OrdinalIgnoreCase),
                $"'{term}' is suppressed here but never appears in the prose, "
                + "so the suppression is a no-op and the glossary entry is unreachable");
        }
    }

    [Fact]
    public async Task TheNewVocabularyIsStillReachableOnTheGlossary()
    {
        var html = await _factory.CreateClient().GetStringAsync("/glossary");

        foreach (var slug in new[]
                 {
                     "transformation", "watch-and-wait", "vorasidenib",
                     "circumscribed", "supratentorial",
                 })
        {
            Assert.Contains($"id=\"{slug}\"", html, StringComparison.Ordinal);
        }
    }
}
