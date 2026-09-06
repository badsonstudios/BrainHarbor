using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-511: T-X5 radiation therapy, the SECOND page of the treatment library
/// and the hub for every form of radiation. Full twelve slots (§12.8), like
/// WI-510: it is a treatment with a day and a procedure, so slots 2, 5, 7 and
/// 8 are all present.
///
/// The shared reading helpers live on <see cref="CuratedPage"/> in
/// TestsLibraryPagesTests.cs. Two rules are promoted onto it by this item
/// rather than written page-locally, which is what §12.8's "factor at the
/// second use" asks for at the second treatment page:
/// <see cref="CuratedPage.AssertNeverMinimises"/> and
/// <see cref="CuratedPage.BritishForms"/>.
///
/// As everywhere in this library: the prose is reviewed, not tested. What is
/// pinned here is the handful of places this page could give a reader actively
/// wrong information, and the things later treatment pages inherit.
/// </summary>
public sealed class RadiationTherapyPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "radiation-therapy.md");

    // Reader text, not raw source: this page suppresses eight tooltips with
    // the WI-105 `!%term%` marker, and those are authoring instructions rather
    // than prose (§12.8, WI-509).
    private static string Reader => CuratedPage.ReaderText(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string[] SentencesOf(string section) => CuratedPage.SentencesOf(section);

    private const string MaskHeading = "If the mask frightens you";
    private const string CaregiverHeading = "For the person caring for someone through radiation";
    private const string AcuteHeading = "Side effects during treatment, and in the weeks after";

    [Fact]
    public void TheMaskStepLeadsWithHowItFeelsBeforeItExplainsWhatItIsFor()
    {
        // The ticket's central instruction, and §12.8 slot 3: sensory first,
        // then the mechanism. The research calls mask-making a distinct and
        // under-acknowledged fear point, and a reader who meets "it holds your
        // head still so the beam is accurate" BEFORE "it feels like a warm
        // washcloth" has been handed the engineering rationale for the thing
        // frightening them. The defect is positional, so it is a test rather
        // than a note.
        var section = Section("What happens, step by step");

        var feel = section.IndexOf("warm washcloth", StringComparison.OrdinalIgnoreCase);
        var why = section.IndexOf("Why the mask exists", StringComparison.OrdinalIgnoreCase);

        Assert.True(feel > 0, "the mask step no longer says what the mesh feels like");
        Assert.True(why > feel, "the mask's purpose is explained before the reader is told how it feels");
    }

    [Fact]
    public void TheMaskStepSaysYouCanSeeAndBreatheThroughIt()
    {
        // The specific fear is suffocation, and both patient-level sources
        // answer it head on (Roswell: "You can see and breathe through it at
        // all times, and it will not hurt you"; MSK: "You will not have any
        // trouble breathing, hearing, or seeing"). Leaving it out is how a
        // page about the mask manages not to address the thing about the mask.
        var section = Section("What happens, step by step");

        Assert.Matches(new Regex(@"see through it and breathe through it", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"does not hurt", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePlanningVisitSaysNoRadiationIsGivenAtIt()
    {
        // The most useful single fact about that appointment, and the one the
        // research says is routinely not conveyed. A reader who thinks the
        // planning visit is treatment counts their course wrong and braces for
        // the wrong day.
        var section = Section("What happens, step by step");

        Assert.Matches(new Regex(@"No radiation is given", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheMaskFearSectionOffersRealOptionsAndEndsOnTheReassurance()
    {
        // The WI-506 claustrophobia section, one page over: never a list of
        // problems with no answers (§12.8, slot 6a). Pinned on the LAST
        // sentence rather than a window, per the WI-510 review finding — a
        // three-sentence window only proves the reassurance is nearby, and
        // WI-510's resection section passed one while closing on a fear.
        //
        // The section closes on the ACTION, not on a comfort. §12.6 allows
        // either ("follow it with a concrete action or something solid"), and
        // the first draft's closer — "with help, almost everybody gets through
        // the course" — was a reassurance no source supports. None of ACS,
        // MSK, Roswell or NBTS says how many people find the mask hard or how
        // many finish. An invented comfort on the most frightening section of
        // the page is the kind that stops being true for the reader it fails,
        // so it is gone and what is left is what the sources do support: the
        // therapists are in that room daily and can change how this goes.
        var section = Section(MaskHeading);
        var sentences = SentencesOf(section);

        Assert.Matches(new Regex(@"[Mm]edicine for anxiety", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"run-through without any radiation", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Mm]usic or an audiobook", RegexOptions.IgnoreCase), section);

        Assert.Matches(
            new Regex(@"can change how this goes for you", RegexOptions.IgnoreCase),
            sentences[^1]);
    }

    [Fact]
    public void TheSomnolenceSectionNamesItGivesTheTimingAndSaysItIsExpectedToPass()
    {
        // The ticket's other named requirement, and the same shape as WI-510's
        // SMA syndrome: naming a frightening thing without saying it is
        // expected to pass is worse than not raising it at all. Both halves
        // are pinned together, in the same section.
        var section = Section(AcuteHeading);

        Assert.Matches(new Regex(@"somnolence syndrome", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"gets better on its own over a few weeks", RegexOptions.IgnoreCase), section);

        // The timing figure, ATTRIBUTED in the sentence that prints it (§12.8,
        // WI-507). Review caught this as a blocker: the draft stated "around
        // four to six weeks" flat, and the only source in the whole fetched
        // set that says four to six weeks is the charity jargon-buster page —
        // which the draft had DROPPED, because the dossier miscites it for a
        // different claim. Dropping the citation left the number it was also
        // carrying with nothing behind it, which is §12.8's "deleting a bad
        // citation does not delete the claim" exactly. The page now cites that
        // source for what it does say, and says whose number it is.
        Assert.Matches(
            new Regex(@"charity in the United Kingdom puts it at four\s+to six weeks",
                RegexOptions.IgnoreCase),
            CuratedPage.Flatten(section));

        // And the page's own primary source disagrees with it, so the page
        // says that too rather than picking the tidier number.
        Assert.Matches(
            new Regex(@"the window is wider than one number suggests", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheSomnolenceSectionDoesNotPromise()
    {
        // "Do not promise" is the item's own word, and it is the difference
        // between a page a reader can trust twice and a page that was kind to
        // them once. The source says it USUALLY gets better on its own; a page
        // that upgrades that to "will" has invented the reassurance.
        //
        // Scoped to the somnolence subsection rather than the page: "always"
        // and "will" are perfectly good words elsewhere on a page about a
        // daily schedule.
        var section = Section(AcuteHeading);
        var start = section.IndexOf("Somnolence syndrome", StringComparison.OrdinalIgnoreCase);
        Assert.True(start > 0, "the somnolence subsection has been renamed or removed");

        var subsection = section[start..];

        Assert.DoesNotMatch(
            new Regex(@"will go away|always gets better|guaranteed|is certain to|you will recover",
                RegexOptions.IgnoreCase),
            subsection);

        // And the honest other half. The two cited sources disagree sharply on
        // how often this happens — the charity calls it rare, Faithfull and
        // Brada found it in sixteen of nineteen — and the draft split the
        // difference with an unattributed "uncommon", which is a third answer
        // belonging to nobody. The page now prints the disagreement.
        Assert.Matches(
            new Regex(@"does not happen to everybody", RegexOptions.IgnoreCase), subsection);
        Assert.Matches(
            new Regex(@"not settled, and the sources disagree", RegexOptions.IgnoreCase), subsection);
    }

    [Fact]
    public void TheSomnolenceSectionExplainsWhyBeingToldInAdvanceMatters()
    {
        // Straight out of Faithfull and Brada: people put the symptoms down to
        // flu or another passing illness, the unexplained and overwhelming
        // nature of them was itself a cause of anxiety, and forewarning
        // reduces that anxiety. That finding IS the argument for the
        // subsection existing, so if it goes, the section has lost its reason.
        var section = Section(AcuteHeading);

        Assert.Matches(new Regex(@"\bflu\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Tt]elling people in advance", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageCarriesTheAcuteAndLatePairInThatOrder()
    {
        // §12.8 slot 6b on a treatment page: "side effects, soon / side
        // effects, later", two headings. The ORDER is the content — a reader
        // who meets "another tumor, years later" before "you will be tired"
        // has been handed the rare thing before the near-certain one.
        var headings = Headings();

        var soon = headings.IndexOf(AcuteHeading);
        var later = headings.IndexOf("Side effects that can come later");

        Assert.True(soon >= 0, "the acute side-effect section has been renamed");
        Assert.True(later > soon, "the late-effect section is missing, or sits before the acute one");
    }

    [Fact]
    public void TheLateEffectsSectionEndsOnWhatToDoRatherThanOnTheFear()
    {
        // §12.6: never end a section on a frightening sentence, and this is
        // the most frightening section on the page — its list closes on a
        // second tumor years later. ACS supplies the honest counterweight
        // ("this small risk does not outweigh the benefits of radiation for
        // those who need it"), and the section ends on the question worth
        // asking rather than on the list.
        var sentences = SentencesOf(Section("Side effects that can come later"));

        Assert.DoesNotMatch(
            new Regex(@"second tumor|years afterward|dead tissue", RegexOptions.IgnoreCase),
            sentences[^1]);
        Assert.Matches(
            new Regex(@"useful thing to do with this list", RegexOptions.IgnoreCase),
            sentences[^1]);
    }

    [Fact]
    public void TheLateEffectsSectionPushesTheHormoneCheckAsAnAction()
    {
        // The one genuinely under-done thing on the list. The meta-analysis
        // reports endocrine insufficiency in over half of irradiated patients
        // and this page publishes no figure for it (§12.2 item 5). What it
        // publishes instead is the sentence that gets a reader a blood test.
        var section = Section("Side effects that can come later");

        Assert.Matches(
            new Regex(@"[Aa]sk whether your hormone levels will be checked", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheWholeBrainSectionGivesDirectionOnlyAndSaysWhyThereIsNoNumber()
    {
        // R3, and the reason R3 exists. The CC001 cognitive figures were
        // flagged independently as the single most likely place for this
        // content to mislead, because MOST PEOPLE IN BOTH ARMS DECLINED — a
        // percentage from one arm without that fact beside it misleads in the
        // reader's favour, which is still misleading. The page states the
        // direction and says the omission is a decision, so that a reader who
        // came for a number does not read the silence as evasion.
        var section = CuratedPage.Flatten(Section("Whole-brain radiation"));

        Assert.Matches(new Regex(@"less likely to lose thinking skills", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"no numbers on this page for that comparison, and that is deliberate",
                RegexOptions.IgnoreCase),
            section);
        // Review's blocker: the draft said "most people in both groups lost
        // some thinking skills", which imports R3's reasoning about a
        // DIFFERENT comparison (SRS alone versus SRS plus whole-brain, where
        // most in both arms did decline) and asserts it of CC001, where the
        // per-test deterioration rates run 23.3% v 40.4% and 11.5% v 24.7%.
        // "Most" is false of the arm the page is recommending. A number
        // written as a word is still a number (§12.2 item 2).
        Assert.Matches(
            new Regex(@"common in both groups", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"reduced that; it did not prevent it", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(
            new Regex(@"most people in both", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheWholeBrainSectionHandsTheReaderTheQuestionByName()
    {
        // The research's own publishable recommendation, and the most
        // actionable sentence on the page: hippocampal avoidance plus
        // memantine is a standard of care to OFFER, which means a reader who
        // does not ask may not be offered it.
        var section = Section("Whole-brain radiation");

        Assert.Matches(new Regex(@"hippocampal avoidance", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"memantine", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"avoid the memory part of my brain", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheWholeBrainMaterialIsASubsectionAndNotItsOwnPage()
    {
        // SYNTHESIS §2.2 is explicit: whole-brain radiation is a section of
        // X5, not a page. A test rather than a note, because the natural thing
        // to do with a section this size is to promote it — and that would
        // orphan it, since nothing links to /treatments/whole-brain-radiation
        // and nothing is scheduled to.
        Assert.Contains(Headings(), h => h.StartsWith("Whole-brain radiation", StringComparison.Ordinal));
        Assert.DoesNotContain("/treatments/whole-brain", Reader, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheRadioactiveAnswerIsTheFirstWordOfItsSection()
    {
        // §12.8 slot 6c, and §12.6's "answer in the first sentence". This is
        // the gadolinium question from the MRI page in a new coat: a
        // genuinely-asked worry with a flat answer, where any hedging at all
        // reads as a yes. MSK: "You will not be radioactive during or after
        // your radiation treatments. It is safe for you to be around other
        // people and pets."
        var section = Section("Am I radioactive?");

        Assert.StartsWith("No.", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"safe to be around other people", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheFractionationSectionSaysTheLengthOfTheCourseIsNotAVerdict()
    {
        // The inference a reader makes unprompted, and it is wrong: thirty
        // treatments does not mean thirty times worse off than one treatment.
        // No source says it, which is exactly why the page has to.
        var section = Section("Why every day, and why so many?");

        Assert.Matches(
            new Regex(@"not a measure of how bad your tumor is", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Hh]ealthy cells are good at repairing", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheIsItTheDrugSectionNamesTheSteroidAndTellsNobodyToStopIt()
    {
        // §12.6's recurring frame, carried over from WI-510 because the drug
        // is the same one and the failure mode is the same one. Naming the
        // effects without "never stop it on your own" is a page that gets
        // somebody's steroid stopped abruptly, which is its own emergency.
        var section = Section(AcuteHeading);

        Assert.Matches(new Regex(@"[Dd]examethasone", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"muscle", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"Never stop a steroid suddenly or on your own", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"tapering", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageCarriesNoPercentageNoGyAndNoDoseFigure()
    {
        // §12.4. "No Gy, ever" is the item's own instruction, and the dossier
        // hands over several of them. R2 keeps risk language qualitative; R3
        // keeps numbers off the whole-brain comparison. The ">80% report
        // fatigue" figure and the "over half" hormone figure are both real,
        // both sourced, and both deliberately absent.
        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*%", Reader);
        Assert.DoesNotMatch(
            new Regex(@"\b\d+(\.\d+)?\s*(Gy|Gray|mg|ml|mmol|Tesla)\b", RegexOptions.IgnoreCase), Reader);

        // Spelled-out proportions too, which the digit check cannot see. This
        // page's house style writes every number as a word ("thirty
        // treatments", "four to six weeks", "one to ten working days"), so the
        // format a future edit would naturally reach for is exactly the one a
        // `%` regex is blind to — and a draft of this page did smuggle one
        // through in words ("most people in both groups"), which review caught
        // and this assertion would not have. R3's whole point is that a
        // proportion from one arm of that trial misleads, and it misleads just
        // as much spelled out.
        // Scoped to proportions OF PEOPLE, not to the words themselves. The
        // first version banned "a quarter" outright and immediately failed on
        // this page's own slot 4 — "a quarter of an hour" is a duration R1
        // explicitly permits, not a statistic. That is the ban-list trap one
        // more time (§12.8: a rule that fails a correct page is worse than no
        // rule), and this time it was caught by running it rather than by
        // reasoning about it.
        Assert.DoesNotMatch(
            new Regex(@"\bper ?cent\b|\bpercent\b"
                    + @"|\b(a|one|two|three)\s+(third|quarter|half|fifth)s?\s+of\s+"
                    + @"(people|patients|them|those|everyone|men|women)\b",
                RegexOptions.IgnoreCase),
            Reader);
    }

    [Fact]
    public void ThePageNeverCharacterisesTheOutcome()
    {
        // The shared §12.8 ban list. WI-511 ran the whole list over the whole
        // corpus for the first time and demoted "good news"/"bad news" — see
        // CuratedPage.RejectedCharacterisations for the reason.
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageNeverMinimisesTheTreatment() =>
        // The promotion §12.8 asked for at the second treatment page. WI-510
        // wrote this page-locally on purpose ("do not factor at the FIRST use
        // either"); this is the second use, so it lives on CuratedPage now, in
        // the treatment vocabulary rather than the surgical one.
        CuratedPage.AssertNeverMinimises(Reader, "treatments/radiation-therapy");

    [Fact]
    public void ThePageLinksToSurgeryRatherThanExplainingTheOperationAgain()
    {
        // /treatments/craniotomy owns the operation as of WI-510. Two pages
        // describing a craniotomy is how they drift apart, and the surgery
        // page is the one that was reviewed hard.
        //
        // Scoped to slot 2, not to the page. The first version asserted the
        // link appeared ANYWHERE, and "Where to go next" links there too — so
        // deleting the mid-body link left the test green, which is the exact
        // whole-page-Contains failure WI-506 and WI-508 both found in their
        // own tests. The load-bearing link is the one beside the sentence
        // about tumor cells surgery could not take out; a reader who meets
        // that claim needs the door in the same breath, not eight screens
        // later in an index.
        Assert.Contains("/treatments/craniotomy", Section("Why am I having it?"), StringComparison.Ordinal);
        Assert.Contains("/treatments/craniotomy", Reader, StringComparison.Ordinal);

        foreach (var owned in new[]
                 { "bone flap", "craniectomy", "titanium", "5-ALA", "folds the skin back" })
        {
            Assert.DoesNotContain(owned, Reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheSharedBlockRatherThanRestatingIt()
    {
        // §12.7, using WI-510's shingle check rather than its first attempt:
        // the heading-based version shipped four genuine duplications that
        // were none of the headings. Eight-word shingles catch a restatement
        // in any shape. "Where to go next" is excluded — it is an index of
        // onward doors by design, so a link the block also makes belongs there.
        var section = Section(CaregiverHeading);

        Assert.Contains("[CAREGIVER]", section, StringComparison.Ordinal);

        var body = Reader.Replace("[CAREGIVER]", " ", StringComparison.Ordinal);
        var next = body.IndexOf("## Where to go next", StringComparison.Ordinal);
        var page = next > 0 ? body[..next] : body;

        var pageShingles = Shingles(page, 8).ToHashSet();
        var blockPath = Path.Combine(CuratedPage.BlocksRoot, "caregiver.md");
        var overlaps = Shingles(CuratedPage.Body(File.ReadAllText(blockPath)), 8)
            .Where(pageShingles.Contains)
            .Distinct()
            .ToList();

        Assert.True(overlaps.Count == 0,
            "this page restates the shared [CAREGIVER] block, so the reader meets it twice:\n  "
            + string.Join("\n  ", overlaps));
    }

    [Fact]
    public void TheCaregiverSectionDoesNotSendTheReaderToASeizurePageTheBlockAlreadySendsThemTo()
    {
        // A duplicated LINK is not a duplicated sentence, so the shingle check
        // cannot see it. The block already carries both seizure links, and a
        // treatment page writing about aftercare reaches for them naturally.
        var block = CuratedPage.Body(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")));

        foreach (var link in new[] { "/seizures/what-to-do", "/seizures/living-with" })
        {
            if (!block.Contains(link, StringComparison.Ordinal))
            {
                continue;
            }

            var section = Section(CaregiverHeading);
            Assert.DoesNotContain(link, section.Replace("[CAREGIVER]", " ", StringComparison.Ordinal),
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheCaregiverSectionNamesTheDailyTripAsTheJob()
    {
        // What is actually different about radiation, and the thing every
        // comparator leaves out. Surgery is one enormous day; radiation is
        // thirty ordinary ones, and the load that lands on the person at home
        // is transport. One patient in the NBTS piece names finding drivers as
        // the hardest part of the whole treatment.
        var section = Section(CaregiverHeading);

        Assert.Matches(new Regex(@"[Ee]very weekday, for weeks", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Dd]o not try to be the only driver", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCaregiverSectionWarnsThatTheTirednessOutlastsTheLastTreatment()
    {
        // The specific misreading this section exists to prevent. Somebody
        // flat six weeks after the course finished looks, to a frightened
        // caregiver, like somebody relapsing — and MSK says plainly that
        // fatigue continues for weeks to months after treatment ends.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        Assert.Matches(
            new Regex(@"does not stop on the last treatment", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"still recovering", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCaregiverSectionHasACallTodayListAndDeliberatelyNoAmbulanceList()
    {
        // The WI-510 review rule: "whenever a page carries a 'call an
        // ambulance' list, diff it against every other such list on the site".
        // This page carries no ambulance list, on purpose. The shared
        // [CAREGIVER] block already teaches the escalation — get two numbers,
        // ask what counts as "call us today" and what counts as "call an
        // ambulance", and if you are not sure, call — and it links the seizure
        // page that owns the emergency. A third copy of an ambulance list is a
        // third copy to keep in step with the other two, and the WI-510
        // blocker was a list that had already drifted out of step with one.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        Assert.Matches(
            new Regex(@"When to call the team the same day", RegexOptions.IgnoreCase), section);
        // Any second escalation list, not just one headed "call an ambulance".
        // A list headed "Call 911 if" is the same duplication wearing a
        // different hat, and the first version could not see it.
        Assert.DoesNotMatch(
            new Regex(@"When to call an ambulance|Call 9-?1-?1 if", RegexOptions.IgnoreCase),
            section);

        // The half the first version could not see, and it was a blocker.
        // Asserting only that no heading says "call an ambulance" proves the
        // page has no SECOND list; it says nothing about whether the one list
        // it does have under-triages. The draft's bullet read, flatly, "There
        // is a seizure" — six lines from a page whose whole job is to say that
        // most seizures are not an emergency and a FIRST one is. WI-510's
        // blocker was over-escalation; this is the same rule failing in the
        // more dangerous direction, and the fix is the carve-out inline rather
        // than a link, because this section must not duplicate the block's.
        var seizure = section.IndexOf("There is a seizure", StringComparison.OrdinalIgnoreCase);
        Assert.True(seizure > 0, "the call-today list no longer mentions a seizure");

        Assert.Matches(
            new Regex(@"first ever.{0,140}ambulance|ambulance.{0,140}first ever",
                RegexOptions.IgnoreCase),
            section[seizure..]);
        Assert.Matches(
            new Regex(@"more than five minutes", RegexOptions.IgnoreCase), section[seizure..]);
    }

    [Fact]
    public void TheBrainSwellingWarningEscalatesASeizureTheSameWayTheSeizurePageDoes()
    {
        // The same blocker's other half. The body's swelling paragraph lists
        // seizures among the things to "call your team right away" about, and
        // that list is read by the patient rather than the caregiver — so it
        // needs the same carve-out, or the page contradicts
        // /seizures/what-to-do in two places instead of one.
        var section = CuratedPage.Flatten(Section(AcuteHeading));
        var swelling = section.IndexOf("Swelling in the brain", StringComparison.OrdinalIgnoreCase);

        Assert.True(swelling > 0, "the brain-swelling paragraph has been renamed or removed");

        var paragraph = section[swelling..Math.Min(section.Length, swelling + 900)];
        Assert.Matches(
            new Regex(@"first-ever seizure.{0,160}ambulance", RegexOptions.IgnoreCase), paragraph);
    }

    [Fact]
    public void TheRepeatSectionSaysDoctorsDisagreeRatherThanInventingAnAnswer()
    {
        // §12.8 slot 8. No patient-level source in the set says anything at
        // all about a second course of radiation — ACS, MSK and NBTS are
        // silent, checked — so the claim rests on the EANO guideline and is
        // published at the strength that guideline actually supports:
        // reirradiation is an option after a long gap, the indications remain
        // controversial, and no trial settles it. Saying so is the honest
        // version; a confident answer here would have been invented.
        var section = Section("Why am I having radiation again?");

        Assert.Matches(new Regex(@"[Dd]octors do not agree", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"a year or more", RegexOptions.IgnoreCase), section);

        // And it must not leave the reader thinking the door is shut.
        Assert.Matches(
            new Regex(@"not a reason to assume it is off the table", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheResultSectionExplainsWhyTheFirstScanCanLookWorse()
    {
        // A radiation course has no result at the end of it, so slot 9's
        // honest answer is the follow-up scan — and the reader has to be told
        // BEFORE they see it that treatment itself can make a scan look worse.
        // T11 owns pseudoprogression (SYNTHESIS §4.3) and does not exist yet,
        // so this page names it and leaves the depth to the page that will.
        var section = Section("Who reads it, and how do I get the result?");

        Assert.Matches(new Regex(@"pseudoprogression", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not them stalling", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageUsesTheFullTwelveSlotTemplate()
    {
        // §12.8's first WI-510 rule, applied to the page after it: read the
        // standard for the slot list, not the previous page. A treatment with
        // a day and a procedure keeps slots 2, 5, 7 and 8, and three bent
        // pages in a row is how a template quietly shrinks.
        var headings = Headings();

        Assert.Contains("Why am I having it?", headings);                        // slot 2
        // Slot 4 is named here because review found it was the one slot no
        // test in this file read: every other slot is incidentally pinned by
        // some other test's Section() call, so "How long does it take?" could
        // have been deleted whole and all 37 tests stayed green — on the page
        // whose test is named for using the full twelve. It is also where the
        // R1 durations live.
        Assert.Contains("How long does it take?", headings);                     // slot 4
        Assert.Contains("What does it feel like?", headings);                    // slot 5
        Assert.Contains("What you need first, and what to bring", headings);     // slot 7
        Assert.Contains("Why am I having radiation again?", headings);           // slot 8
        Assert.Contains("Who reads it, and how do I get the result?", headings); // slot 9
    }

    [Fact]
    public void TheCaregiverSectionIsPlacedAfterSlotSevenAndBeforeTheRepeatSection()
    {
        // §12.7 fixes the position, and it is invisible in review because the
        // section reads fine anywhere. The ordering encodes "you have been
        // given everything to act on before we ask you to take on a job".
        var headings = Headings();

        var preparation = headings.IndexOf("What you need first, and what to bring");
        var caregiver = headings.IndexOf(CaregiverHeading);
        var repeat = headings.IndexOf("Why am I having radiation again?");

        Assert.True(preparation >= 0 && caregiver >= 0 && repeat >= 0,
            "one of the three §12.8 slots around the caregiver section has been renamed");
        Assert.Equal(preparation + 1, caregiver);
        Assert.Equal(caregiver + 1, repeat);
    }

    [Fact]
    public void TheAnchorsOtherPagesWillDeepLinkAreWrittenExplicitly()
    {
        // §12.8, WI-509's mechanism. Markdig derives an id from heading TEXT,
        // so rewording a heading silently breaks every inbound link and lands
        // the reader at the top of a long page with no sign anything is wrong.
        // These three are what the 23 tumor hubs will point at.
        Assert.Matches(new Regex(@"^## .*\{#the-mask\}", RegexOptions.Multiline), Page);
        Assert.Matches(new Regex(@"^### .*\{#somnolence-syndrome\}", RegexOptions.Multiline), Page);
        Assert.Matches(new Regex(@"^## .*\{#whole-brain-radiation\}", RegexOptions.Multiline), Page);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone, and §12.8 records that
        // collapsing anything else was considered and rejected.
        Assert.DoesNotContain(":::", Page, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        // Shared contract item 7, and the last two slots of §12.8. The .Trim()
        // inside Headings() is load-bearing on a CRLF checkout, not cosmetic:
        // `.` matches `\r`.
        var headings = Headings();

        Assert.Equal("The short version", headings[0]);
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8, checked here rather than trusting ContentCheck: a
        // missing `accessed` date is only a warning there, and this phase's
        // whole source discipline rests on it.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // Named, not counted: ACS carries the modality list and the late
        // effects, MSK the simulation and the daily visit, Roswell the mask,
        // NBTS the patient experience, the Brain Tumour Charity the somnolence
        // resolution, and the PMC papers the whole-brain trial, the hormone
        // review and the reirradiation guidance.
        foreach (var domain in new[]
                 {
                     "cancer.org", "mskcc.org", "roswellpark.org", "braintumor.org",
                     "thebraintumourcharity.org", "ncbi.nlm.nih.gov", "cancer.gov",
                 })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor: .NET's multiline `$` does not match before `\r`, and
        // this repo is core.autocrlf=true (WI-501, WI-506).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheDossierSourcesThatDoNotSayWhatWasClaimedNeverComeBack()
    {
        // Every P5 item so far has found the research dossier mis-citing
        // something, and a wrong citation that RESOLVES is the worst failure
        // mode on a medical site. Three sources are barred from this page:
        //
        // - PMC7017115, cited by the dossier for the cognitive late effect, is
        //   a mechanistic review largely about the MOUSE brain — the same
        //   class of error as WI-507's rodent-tissue fixation figure. ACS says
        //   it at patient level, so ACS carries it.
        // - ascopubs.org returns a 1,830-byte JavaScript shell to any fetch. A
        //   citation nobody can open is a citation nobody can verify (§12.8,
        //   the academic.oup.com rule), and the whole-brain claim rests on
        //   PMC7106984 directly.
        // - NBK66023 is NCI's patient PDQ, which §12.1 forbids for
        //   brain-metastasis radiation and which is pre-CNS5 throughout.
        //
        // The charity's JARGON-BUSTER page is deliberately NOT on this list,
        // and getting that wrong was WI-511's own blocker. The dossier miscites
        // it — it attributes "usually resolves on its own over a few weeks
        // without treatment" to it, and fetched, that page is one sentence long
        // and says no such thing. The first draft therefore dropped the URL
        // outright. But the same page IS the only source anywhere in the set
        // for the four-to-six-week timing, which the draft kept. Dropping the
        // citation left the number it was carrying with nothing behind it,
        // which is §12.8's "deleting a bad citation does not delete the claim
        // it was carrying" in a form nobody had seen yet: one URL carrying two
        // claims, wrong about one and right about the other. Both BTC pages
        // are cited now, each for what it actually says.
        var front = CuratedPage.FrontMatter(Page);

        foreach (var url in new[] { "PMC7017115", "ascopubs.org", "NBK66023" })
        {
            Assert.DoesNotContain(url, front, StringComparison.OrdinalIgnoreCase);
        }

        // And the resolution claim must stay attached to the page that carries
        // it, not to the jargon buster.
        Assert.Contains("side-effects-radiotherapy-adults", front, StringComparison.Ordinal);
    }

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())];

    private static IEnumerable<string> Shingles(string text, int n)
    {
        var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+")
            .Select(m => m.Value).ToList();
        for (var i = 0; i + n <= words.Count; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(n));
        }
    }
}

/// <summary>
/// Site-wide rules WI-511 promoted out of a page. Both were page-local
/// observations on WI-510, and both turned out to be properties of the corpus
/// rather than of one page — which is what §12.8's "run it against the pages
/// already shipped" is for.
/// </summary>
public sealed class CuratedProseHousekeepingTests
{
    /// <summary>
    /// Every file whose prose reaches a reader: curated pages, the shared
    /// blocks composed into them, and the glossary entries whose definitions
    /// fire as tooltips site-wide.
    ///
    /// One helper because review caught the three rules below scanning three
    /// different corpora. The minimisation rule read <c>AllPages()</c> only,
    /// so a glossary entry saying "a simple procedure" would have fired as a
    /// tooltip on every page in the corpus with nothing to catch it. And
    /// <c>AllPages()</c> and <c>SharedSources()</c> both include
    /// <c>blocks/</c>, so a naive concat double-reported every block offender.
    /// </summary>
    private static IEnumerable<(string Slug, string Text)> ReaderFacingFiles() =>
        CuratedPage.AllPages()
            .Concat(CuratedPage.SharedSources())
            .GroupBy(f => f.Slug)
            .Select(g => g.First());

    [Fact]
    public void NoCuratedPageBlockOrGlossaryEntryUsesABritishForm()
    {
        // WI-510 shipped seven British spellings in its first draft and every
        // gate passed clean: reading grade, ContentCheck and 1,039 tests are
        // all blind to a page written correctly for a different country.
        // §12.8 recorded "grep the page against the corpus before shipping".
        // This is that grep, made into a gate.
        //
        // BODY text only. Source titles and URLs legitimately carry British
        // forms — "The Brain Tumour Charity" is an organization's actual name
        // and "Radiotherapy Side-Effects (Adults)" is a real page's actual
        // title — and Americanising a citation would be a worse defect than
        // the one this test prevents.
        var offenders = new List<string>();

        foreach (var (slug, text) in ReaderFacingFiles())
        {
            // Flattened BEFORE matching, which is the load-bearing part: the
            // corpus is hard-wrapped, and the first version of this test read
            // raw body text and walked straight past "a\nlift" in the shared
            // caregiver block — a British idiom on eighteen tumor hubs, missed
            // by the gate written to catch exactly that. Same trap as WI-509's
            // fix test, one item later.
            var body = CuratedPage.Flatten(CuratedPage.Body(text));

            foreach (var exemption in CuratedPage.BritishFormExemptions)
            {
                body = body.Replace(exemption, " ", StringComparison.OrdinalIgnoreCase);
            }

            foreach (var form in CuratedPage.BritishForms)
            {
                foreach (Match match in Regex.Matches(
                    body, @"\b" + Regex.Escape(form), RegexOptions.IgnoreCase))
                {
                    var from = Math.Max(0, match.Index - 40);
                    var to = Math.Min(body.Length, match.Index + 40);
                    offenders.Add($"{slug}: {form} — ...{body[from..to]}...");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "British spellings in a corpus that is US throughout:\n  "
            + string.Join("\n  ", offenders.Distinct()));
    }

    [Fact]
    public void NoCuratedPageOrBlockMinimisesATreatment()
    {
        // The other promotion. Negation-aware, so "this is not a routine
        // treatment" — the correct and natural sentence a treatment page wants
        // — passes, which is the distinction that got "good sign"/"bad sign"
        // rejected at WI-509 and cost this list six candidates at WI-510.
        //
        // Glossary entries are in scope here. The first version scanned pages
        // and blocks only, so a definition reading "a simple procedure" would
        // have fired as a tooltip on every page in the corpus and no test
        // would have seen it.
        foreach (var (slug, text) in ReaderFacingFiles())
        {
            CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(text), slug);
        }
    }

    [Fact]
    public void EveryPhraseOnTheCharacterisationBanListIsAbsentFromTheWholeCorpus()
    {
        // §12.8 asks for this before ADDING a phrase. Nobody had ever asked it
        // of the phrases already on the list, and running it for the first
        // time is how WI-511 found "bad news" sitting over a correct sentence
        // on /seizures/what-to-do — "a seizure is not automatically bad news
        // about the tumor" — eight items after that page shipped. Only two
        // pages assert the list and neither uses the phrase, so nothing ever
        // went red. Now the list defends itself instead of waiting for a page
        // that happens to check it.
        var offenders = new List<string>();

        foreach (var (slug, text) in ReaderFacingFiles())
        {
            var body = CuratedPage.Flatten(CuratedPage.ReaderText(text));
            foreach (var phrase in CuratedPage.Characterisations)
            {
                if (body.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    offenders.Add($"{slug}: {phrase}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "a banned characterisation is on a shipped page, so either the page or the list is wrong:\n  "
            + string.Join("\n  ", offenders));
    }
}

/// <summary>The page as served, the door that leads to it, and the tooltips.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class RadiationTherapyPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/radiation-therapy";

    private readonly WebApplicationFactory<Program> _factory;

    public RadiationTherapyPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // The page's own title, not the words "radiation therapy". The first
        // version asserted the latter and would have passed on the body prose
        // of a completely different page — the URL is under /treatments/ and
        // the phrase appears in the first line of the short version, so the
        // assertion could never have distinguished "this page is served" from
        // "something about radiation is served".
        Assert.Contains("the mask, the daily visits", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageIsReachableFromWhereANewlyDiagnosedReaderStarts()
    {
        // The WI-412 orphan lesson. The tumor hubs do not link into the
        // libraries until WI-513, so /start is this page's only door.
        var html = await _factory.CreateClient().GetStringAsync("/start");

        Assert.Contains(Url, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors this page owes its reader: surgery, because radiation
        // usually follows it and this page must not re-explain it; the scan
        // that plans the treatment; the pathology pages, because gene results
        // decide whether radiation happens at all; and a person to talk to.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/treatments/craniotomy", "/tests/mri", "/tests/waiting-for-results",
            "/tests/pathology-report", "/tests/molecular-markers", "/get-help-now");

    [Fact]
    public async Task TheSharedCaregiverBlockComposesIntoTheRenderedPage()
    {
        // [CAREGIVER] is a directive in the source. If the block silently
        // failed to resolve, the page would still render and the caregiver
        // section would simply be missing its shared half.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.DoesNotContain("[CAREGIVER]", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAlreadyDefinedVocabularyStillFiresAsTooltipsHere()
    {
        // Contract item 9 from the other direction: terms earlier items
        // defined, which this page MENTIONS without setting out a competing
        // definition, so the tooltip is what carries the meaning.
        //
        // The comment here used to claim the page "deliberately does NOT
        // redefine" four terms including stereotactic radiosurgery, and review
        // pointed out the page's own bullet was near-verbatim with the
        // glossary entry — "in one visit or a few. There is no cutting,
        // despite the name" in both. A popover repeating the sentence it is
        // anchored inside is the noise §12.8's WI-509 rule exists to stop, so
        // SRS is now suppressed here and moved to the list below. The three
        // that remain genuinely add something the page does not say:
        // dexamethasone's "it is not chemotherapy", radiation necrosis's
        // "sometimes a sample", pseudoprogression's plainer one-liner.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[]
                 {
                     "radiation-necrosis", "pseudoprogression", "dexamethasone",
                 })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheWordsThisPageDefinesItselfDoNotAlsoFireATooltip()
    {
        // §12.8, WI-509: a popover repeating the paragraph directly beneath it
        // is noise. `fractionation` is in this list with the seven new terms
        // because "Why every day, and why so many?" IS its definition, at
        // length.
        var page = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[]
                 {
                     "radiation-oncologist", "simulation", "radiation-mask",
                     "linear-accelerator", "somnolence-syndrome",
                     "whole-brain-radiation", "memantine", "fractionation",
                     "stereotactic-radiosurgery",
                 })
        {
            Assert.DoesNotContain($"def-{slug}\"", page, StringComparison.Ordinal);
        }

        // The other direction, done the way WI-510's review settled it: assert
        // the leak that is actually possible. A `!%term%` marker written into
        // a glossary entry or a shared block suppresses that term on every
        // including page at once, silently, and nothing else would catch it.
        foreach (var (_, text) in CuratedPage.SharedSources())
        {
            Assert.DoesNotContain("!%", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheNewVocabularyIsStillReachableEvenThoughItIsSuppressedHere()
    {
        // The suppression above must not be the reason a term exists nowhere.
        // Every new term has to render on /glossary, or WI-511 has added seven
        // files no reader can ever reach — which is the failure mode WI-505
        // measured and WI-506's tooltip test was written against.
        var html = await _factory.CreateClient().GetStringAsync("/glossary");

        foreach (var slug in new[]
                 {
                     "radiation-oncologist", "simulation", "radiation-mask",
                     "linear-accelerator", "somnolence-syndrome",
                     "whole-brain-radiation", "memantine",
                 })
        {
            Assert.Contains($"id=\"{slug}\"", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheDeepLinkAnchorsRenderWithTheIdsOtherPagesWillUse()
    {
        // The published-interface half of the explicit-anchor rule: the
        // content test proves the author WROTE the anchors, this proves
        // Markdig EMITTED the ids that other pages will link to.
        //
        // Worth being honest about what this half can and cannot see, per
        // WI-510's "a both-directions assertion is only worth writing if both
        // directions are observable". Markdig derives an id from heading TEXT,
        // and for two of these three the derived id happens to be identical to
        // the explicit one — "Somnolence syndrome" slugifies to
        // `somnolence-syndrome` on its own. So deleting `{#somnolence-syndrome}`
        // alone does not move this test, and it should not: the id is still
        // correct. What the explicit anchor buys is that a REWORD cannot move
        // it, and the break that proves this test is a reword with the anchor
        // removed. `{#the-mask}` is the one that differs today ("If the mask
        // frightens you" would derive `if-the-mask-frightens-you`), so it is
        // the one carrying real weight here.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("id=\"the-mask\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"somnolence-syndrome\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"whole-brain-radiation\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"where-to-go-next\"", html, StringComparison.Ordinal);
    }
}