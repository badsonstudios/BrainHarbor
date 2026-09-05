using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-512: T-X8 chemotherapy, the third treatment page and the last item of P5
/// Wave 1.
///
/// A drugs page bends §12.8 the way WI-509's reference list did: slot 3 becomes
/// the list of drugs.
///
/// Slot 5 was DROPPED in the first draft, on the argument that four drugs given
/// four different ways share no answer to "what does it feel like?". Review
/// put it back and was right: three of the four are capsules taken at home, and
/// a CYCLE has one shared shape — unremarkable on the day, sick that evening,
/// flat by the end of the week, and the nadir arriving exactly when you feel
/// finished. That material was already on the page, scattered between the
/// side-effects list and the durations section, which is precisely the WI-506
/// trap §12.8 records: "a reader looking for this should not have to read the
/// procedure to find it." It is now slot 5, headed as a role rather than
/// copied ("What does a cycle feel like?"), per WI-507's precedent.
///
/// It also mattered that this would have been the FIRST treatment page to drop
/// slot 5, with both siblings carrying it — and §12.8's own warning is that
/// three bent pages in a row is how a template quietly shrinks.
///
/// The shared helpers live on <see cref="CuratedPage"/>. `BritishForms` and
/// `AssertNeverMinimises` are site-wide as of WI-511 and are NOT rewritten here
/// — `CuratedProseHousekeepingTests` covers this page automatically.
/// </summary>
public sealed class ChemotherapyPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "chemotherapy.md");

    private static string Reader => CuratedPage.ReaderText(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string[] SentencesOf(string section) => CuratedPage.SentencesOf(section);

    private const string FeverHeading = "Your blood counts, and the fever rule";
    private const string DrugsHeading = "The drugs, one at a time";
    private const string CaregiverHeading = "For the person caring for someone on chemotherapy";

    [Fact]
    public void TheFeverRuleGivesTheThresholdAndSaysToCallAtAnyHour()
    {
        // The life-safety content on this page, and the reason the item exists
        // in the shape it does. ACS: "A temperature over 100.4 F" → call your
        // cancer care team or get medical help.
        //
        // This number is published deliberately, against the grain of every
        // other number rule on the site. It is not an R1 duration and not an
        // R2 risk percentage — it is an actionable threshold, the same class as
        // "a seizure lasting more than five minutes", which
        // /seizures/what-to-do already publishes. A reader at 2am needs a
        // number, not a category.
        var section = CuratedPage.Flatten(Section(FeverHeading));

        Assert.Matches(new Regex(@"100\.4", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"calling your team straight away, at any hour", RegexOptions.IgnoreCase),
            section);

        // The Celsius form, which was unpinned: 100.4 \u00b0F is 38.0 \u00b0C, and
        // changing the parenthetical to 37 or 39 left every test green. It is
        // the life-safety number in its second form, sitting beside the one the
        // page publishes on purpose.
        Assert.Contains("(38 \u00b0C)", section, StringComparison.Ordinal);

        // And exactly ONE threshold. Nothing stopped a second, different number
        // being added anywhere on the page, which is how a reader ends up with
        // two rules and no way to choose between them.
        // NOTE the string is NOT verbatim. `@"...\u00b0..."` does not process
        // the escape \u2014 a verbatim string keeps it as six literal characters \u2014
        // so the first version of this regex hunted for a backslash and never
        // matched a degree sign. It passed on a page with two thresholds.
        var thresholds = Regex.Matches(CuratedPage.Flatten(Reader), "\\b\\d{2,3}(\\.\\d)?\\s*\u00b0?\\s*F\\b")
            .Select(m => Regex.Replace(m.Value, @"\s+", " ").Trim())
            .Distinct()
            .ToList();
        Assert.True(thresholds.Count <= 1,
            "the page states more than one temperature threshold: " + string.Join(", ", thresholds));
    }

    [Fact]
    public void TheFeverRuleNeverTellsAnyoneToWaitAndSee()
    {
        // The under-triage direction, which is the one that kills. WI-511's
        // blocker was a call-today list that said "there is a seizure" flat,
        // six lines from a page saying a first seizure is a 911 call — the
        // same rule failing quietly. Here the risk is a hedge creeping in:
        // "if it persists", "in the morning", "see how you feel". An infection
        // with low white cells is the thing that punishes waiting.
        var section = CuratedPage.Flatten(Section(FeverHeading));

        // Written to catch ADVICE to delay, not the words themselves. The
        // first version banned the bare string "in the morning" and promptly
        // failed on this page's own "Not in the morning." — a ban list blind
        // to its own negation, which is the §12.8 lesson that has now cost
        // WI-509, WI-510, WI-511 and this item one candidate each. The forms
        // below are the ones that would actually tell somebody to wait.
        Assert.DoesNotMatch(
            new Regex(@"wait and see|wait until (the )?morning|call (us |your team )?in the morning|"
                    + @"see how (you|they) feel (first|before)|if it persists, call|sleep on it",
                RegexOptions.IgnoreCase),
            section);

        // And it says the opposite out loud, so the point is made rather than
        // merely not-contradicted.
        Assert.Matches(
            new Regex(@"Not in the morning\. Not after seeing how you feel\.", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheFeverRuleSendsTheReaderToGetTheirOwnTeamsNumber()
    {
        // §12.8's "ask what your centre has" rule, applied to a threshold
        // rather than a machine. Teams differ, and a page that publishes one
        // number without saying so has told a reader to ignore their own team.
        // The out-of-hours phone number is the other half: a threshold with
        // nobody to call is not an instruction.
        var section = CuratedPage.Flatten(Section(FeverHeading));

        Assert.Matches(
            new Regex(@"Their temperature rule.{0,200}Use theirs", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"number to call out of hours", RegexOptions.IgnoreCase), section);

        // "If unsure, call" has to sit after the list, the way the craniotomy
        // caregiver section does it. It is the line that makes an incomplete
        // list safe.
        var unsure = section.IndexOf("If you are not sure whether to call",
            StringComparison.OrdinalIgnoreCase);
        var list = section.IndexOf("Chills, or shaking", StringComparison.OrdinalIgnoreCase);
        Assert.True(list > 0, "the call-straight-away list has been reworded away");
        Assert.True(unsure > list, "the 'if you are not sure, call' line is missing or misplaced");
    }

    [Fact]
    public void TheBloodCountSectionExplainsAllThreeCellLinesAndWhatEachMeans()
    {
        // ACS gives the three lines and this is the page that has to make them
        // mean something: a reader looking at a printout with three numbers on
        // it needs to know which one explains the bruise and which one explains
        // being out of breath.
        var section = Section(FeverHeading);

        Assert.Matches(new Regex(@"White cells.{0,120}infection", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Platelets.{0,120}(bruis|bleed)", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Red cells.{0,120}(tired|breathless)", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheNadirIsNamedWithItsTimingBecauseTheTimingIsTheWholePoint()
    {
        // ACS: the nadir "usually occurs about 7 to 10 days after getting
        // chemo. This is when a person is most at risk". Naming the word
        // without the timing would be vocabulary; the timing is what tells a
        // reader which week to be careful in.
        var section = CuratedPage.Flatten(Section(FeverHeading));

        Assert.Matches(new Regex(@"\bnadir\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"seven to ten days after treatment", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheLomustineSectionCarriesR1sWordingWithTheReasonNotTheInterval()
    {
        // R1's ruling, quoted in the item as mandatory: lomustine is "taken
        // only occasionally, not every day, because it lowers blood counts for
        // weeks after you take it". The INTERVAL is replaced by the REASON —
        // an interval is a cycle schedule and stays out, but the reason is the
        // thing that stops a reader thinking their treatment is being rationed.
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        Assert.Matches(
            new Regex(@"only occasionally, not every\s*day, because it lowers your blood counts for weeks",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheLomustineSectionSaysTheDipComesLateAndSaysWhyThatMatters()
    {
        // The distinctive and genuinely dangerous fact about this drug, and the
        // reason it gets its own section rather than a line. EANO: nitrosoureas
        // cause delayed (4-6 weeks) rather than early (2-3 weeks) and more
        // often cumulative leukopenia and thrombocytopenia. A reader who
        // assumes the temozolomide pattern relaxes at exactly the wrong moment.
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        Assert.Matches(new Regex(@"four to six weeks after a dose", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"two to three weeks that is usual with temozolomide", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"least expect a problem", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePcvSectionSplitsWhatIsSwallowedFromWhatGoesIntoAVein()
    {
        // CRUK says it plainly and it is the single most practical thing about
        // PCV: "You have vincristine into your bloodstream (intravenously). You
        // take lomustine and procarbazine as capsules." A reader told only
        // "you will have PCV" does not know whether they are going to the
        // hospital.
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        Assert.Matches(
            new Regex(@"Procarbazine and lomustine are capsules you swallow", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"Vincristine is different: it goes into a vein", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void ThePcvSectionNamesTheNerveDamageAndSaysToReportItEarly()
    {
        // The effect that actually changes management, and the one people put
        // up with silently because numb fingers do not feel like an emergency.
        // Naming it without "tell them early" is a page that watches somebody
        // lose something they do not get back.
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        Assert.Matches(
            new Regex(@"nerve damage in the hands and feet", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Tell your team early", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePcvToleranceFindingNamesTheCountryItCameFrom()
    {
        // §12.8's WI-507 rule: a finding from one country's centre is
        // attributed in the sentence that prints it. WI-507 pinned its version
        // with a test; this page shipped without one, and got the country
        // WRONG — "the United Kingdom", when PMC7869199's methods say "a
        // national neuro-oncology centre in Ireland" and every author is at
        // Beaumont Hospital, Dublin. Nothing could have caught it but reading
        // the source, so the test pins the pairing rather than the word.
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        var finding = section.IndexOf("poorly tolerated", StringComparison.OrdinalIgnoreCase);
        Assert.True(finding > 0, "the PCV tolerance finding is no longer on the page");

        var sentence = section[Math.Max(0, finding - 320)..finding];
        Assert.Matches(new Regex(@"\bin Ireland\b", RegexOptions.IgnoreCase), sentence);
        Assert.DoesNotMatch(new Regex(@"United Kingdom|\bUK\b", RegexOptions.IgnoreCase), sentence);
    }

    [Fact]
    public void TheWaferSectionPrintsTheDisagreementRatherThanPickingASide()
    {
        // The item briefed this the other way round, and reading the source
        // changed it. The 2022 review IS titled "Is It Still an Option?" — but
        // it ANSWERS ITS OWN TITLE YES: "CWs implantation plays a significant
        // role in improving the OS... careful patient selection... should be
        // desirable." Taking the title as evidence of decline would have been
        // judging a paper by its headline.
        //
        // EANO is genuinely more cautious. Two good sources that disagree, so
        // the page prints the disagreement — the same shape as WI-511's
        // somnolence frequency. No survival figures either way (§12.2 item 5).
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        Assert.Matches(
            new Regex(@"Specialists do not agree about how much they help", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"European guidelines are more careful", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"why you may hear different views from different doctors",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheWaferSectionCarriesTheTwoConsequencesAReaderCanActOn()
    {
        // Scans and trial eligibility. Both are decidable BEFORE the operation
        // and undecidable after it, which is why they are on the page at all.
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        // The first version also pinned "the wafers can make later scans harder
        // to read". Review checked the source: PMC9259966 contains the word
        // "imaging" once, in a definition of progression-free survival, and
        // "MRI" not at all. The claim came from the dossier, which bundles it
        // into ONE bullet with the trial-eligibility point — and the trial half
        // IS in the paper, verbatim. A true claim and an invented one, sharing
        // a citation, shipped as a pair. The test had cemented the invented
        // half under a name that called it a consequence a reader can act on.
        Assert.Matches(new Regex(@"clinical trials", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"before the operation, not after", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(
            new Regex(@"scans harder to read|harder to interpret", RegexOptions.IgnoreCase), section);

        // §12.8's "answer the frightening question in both directions": a
        // section headed "in both directions" that gives only the upside is
        // the WI-506 gadolinium defect. ACS supplies the harms.
        Assert.Matches(
            new Regex(@"swelling or an infection where they sit", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheAntibioticSectionSaysItIsPreventionAndNotAnInfection()
    {
        // The item's own note: "patients almost never understand why they are
        // on an antibiotic - explain it". The whole value is in the first line
        // being a denial: you do not have an infection.
        //
        // Worth recording what this section is NOT sourced from. The dossier
        // offered two citations and both were unusable: drugs.com is an AHFS
        // monograph, which PLAN.md §5 and §12.2 item 6 forbid outright, and
        // PMC3601076 is a MOUSE study. The section rests on PMC12803824
        // instead, a 2026 open-access systematic review that states the
        // recommendation and the "until lymphocyte recovery" condition.
        var section = CuratedPage.Flatten(Section("Why am I on an antibiotic?"));

        Assert.Matches(
            new Regex(@"an infection you do not have", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not a sign that you have caught something", RegexOptions.IgnoreCase), section);

        // "Until your counts recover" means a blood test ends it, not a date.
        // A reader who thinks it is a fixed course stops it early.
        Assert.Matches(
            new Regex(@"a blood test decides, not the calendar", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheDelaySectionSaysADelayIsNotTheTumorWinning()
    {
        // §12.8 slot 8 on a drugs page: the frightening moment with a mundane
        // answer. A cycle pushed back for low counts is the monitoring doing
        // its job, and BTC says plainly that a low platelet count is a reason a
        // doctor delays treatment or reduces the dose.
        var section = CuratedPage.Flatten(Section("Why has my treatment been delayed or stopped?"));

        Assert.Matches(
            new Regex(@"A delay is not the tumor winning", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"monitoring working, not failing", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageCarriesNoDoseNoCycleScheduleAndNoPercentage()
    {
        // §12.4 and the item's own instruction: no mg, and no cycle schedules —
        // no TMZ five-on/twenty-three-off, no PCV cycle structure. The
        // clinical facts are in the research dossier marked DO NOT PUBLISH and
        // they stay there.
        //
        // The fever threshold is the deliberate exception and is asserted
        // present elsewhere in this file, so the patterns below are written to
        // let a temperature through and stop everything else.
        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*%", Reader);
        Assert.DoesNotMatch(
            new Regex(@"\b\d+(\.\d+)?\s*(mg|mcg|ml|mmol|Gy|Gray)\b", RegexOptions.IgnoreCase), Reader);
        Assert.DoesNotMatch(
            new Regex(@"\b\d+\s*days? on\b|\b\d+\s*days? off\b|\bfive days on\b|\btwenty-three days off\b",
                RegexOptions.IgnoreCase),
            Reader);

        // Spelled-out proportions, the WI-511 lesson: a number written as a
        // word is still a number, and this corpus writes numbers in words.
        // Scoped to proportions OF PEOPLE so it cannot fire on a duration.
        // "Most" is the word the WI-511 lesson was actually about, and the
        // first version of this regex could not see it: it matched only
        // "a third of people"-shaped phrases. "Most people lose their hair"
        // sailed through. A proportion word attached to people is a number.
        Assert.DoesNotMatch(
            new Regex(@"\bper ?cent\b|\bpercent\b"
                    + @"|\b(a|one|two|three)\s+(third|quarter|half|fifth)s?\s+of\s+"
                    + @"(people|patients|them|those|everyone|men|women)\b"
                    + @"|\b(most|nearly all|almost all|the majority of)\s+(people|patients)\b",
                RegexOptions.IgnoreCase),
            Reader);

        // Cycle schedules written in words. The digit form was banned; CRUK's
        // real PCV cycle is six weeks long, and "each cycle lasts about six
        // weeks" is exactly the schedule the item forbids, in the format this
        // corpus actually writes numbers in.
        Assert.DoesNotMatch(
            new Regex(@"\b(each|every|a) cycle lasts \w+|cycle lasts about|"
                    + @"\b(one|two|three|four|five|six|seven|eight|nine|ten|twelve|twenty-three|thirty)\s+"
                    + @"(days?|weeks?) (on|off)\b",
                RegexOptions.IgnoreCase),
            Reader);

        // Doses written in words.
        Assert.DoesNotMatch(
            new Regex(@"\bmilligrams\b|\bmilligrammes\b", RegexOptions.IgnoreCase), Reader);
    }

    [Fact]
    public void ThePageNeverUsesARetiredTumorName()
    {
        // §12.1 and contract item 3. This matters here more than on most pages:
        // the dossier's whole which-tumor-gets-which-drug table is sourced to
        // NCI's patient PDQ, which is pre-CNS5, and one of its rows is headed
        // "Anaplastic astrocytoma / grade 3". Copying that table would have put
        // a retired diagnosis on a treatment page. The table was rebuilt from
        // ACS and EANO instead.
        foreach (var retired in new[] { "anaplastic", "oligoastrocytoma", "hemangiopericytoma" })
        {
            Assert.DoesNotContain(retired, Reader, StringComparison.OrdinalIgnoreCase);
        }

        // Arabic grades only.
        Assert.DoesNotMatch(new Regex(@"\bgrade\s+(I{1,3}V?|IV)\b"), Reader);
    }

    [Fact]
    public void ThePageNeverCharacterisesTheOutcome()
    {
        // The shared §12.8 ban list.
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageLinksRadiationAndSurgeryRatherThanExplainingThemAgain()
    {
        // /treatments/radiation-therapy and /treatments/craniotomy own those
        // treatments as of WI-511 and WI-510. Chemotherapy runs alongside
        // radiation, so the pull to re-explain it here is strong.
        //
        // Scoped to slot 2, where the claim and the door have to sit together —
        // the WI-511 lesson that a whole-page Contains() passes when the only
        // surviving link is the one in "Where to go next".
        var why = Section("Why am I having it?");

        Assert.Contains("/treatments/radiation-therapy", why, StringComparison.Ordinal);
        Assert.Contains("/treatments/craniotomy", why, StringComparison.Ordinal);

        // The list was four phrases and the break harness walked straight
        // through it: inserting "A mesh mask holds your head still." left the
        // test green, on the test named for stopping exactly that. A ban list
        // is only as wide as the vocabulary it names, and the radiation page's
        // vocabulary is wider than its four most memorable phrases.
        foreach (var owned in new[]
                 {
                     "warm washcloth", "mesh mask", "radiation mask", "simulation appointment",
                     "linear accelerator", "hippocampal", "memantine", "somnolence",
                     "fraction", "bone flap", "craniectomy", "titanium",
                 })
        {
            Assert.DoesNotContain(owned, Reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageDoesNotRedefineTheDrugTheGlossaryAlreadyOwns()
    {
        // temozolomide has had a glossary entry since WI-505 and this page
        // deliberately does not write a competing one-line definition of it.
        // Two definitions that disagree is worse than one, and the tooltip is
        // where the short form belongs.
        Assert.DoesNotMatch(
            new Regex(@"[Tt]emozolomide is a chemotherapy (taken|given) as a capsule",
                RegexOptions.IgnoreCase),
            Reader);
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheSharedBlockRatherThanRestatingIt()
    {
        // §12.7, with WI-510's shingle check.
        var section = Section(CaregiverHeading);

        Assert.Contains("[CAREGIVER]", section, StringComparison.Ordinal);

        var body = Reader.Replace("[CAREGIVER]", " ", StringComparison.Ordinal);
        var next = body.IndexOf("## Where to go next", StringComparison.Ordinal);
        var page = next > 0 ? body[..next] : body;

        var pageShingles = Shingles(page, 8).ToHashSet();
        var overlaps = Shingles(
                CuratedPage.Body(File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md"))), 8)
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
    public void TheCaregiverSectionCarriesTheFeverRuleAndTheRiskyWindow()
    {
        // The caregiver is the one who notices the temperature, and this is the
        // page where the patient is at home rather than on a ward. Repeating
        // the threshold here is deliberate duplication of the ONE fact that has
        // to survive being read in a panic — the same reason
        // /seizures/what-to-do repeats itself.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        Assert.Matches(new Regex(@"100\.4", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"at any hour", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Do not wait for morning", RegexOptions.IgnoreCase), section);

        // And the timing, because the caregiver is the one who can track it.
        Assert.Matches(
            new Regex(@"lowest about a week to ten days after a dose", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheFeverIsNeverFiledUnderTheSameDayList()
    {
        // Reading the page end to end found this, and it is the WI-511 blocker
        // reproduced inside the section written to prevent it.
        //
        // The body says a fever means calling "straight away, at any hour". The
        // caregiver section had a single list headed "When to call the team the
        // SAME DAY", and the first bullet on it was the temperature. A
        // caregiver who reads the list and not the paragraph above it has been
        // told, by this page, that a neutropenic fever can wait until a
        // convenient hour. One claim, two strengths, on one page — and the
        // strength that loses is the one that kills.
        //
        // Both other tests on this section passed throughout: "at any hour" and
        // "Do not wait for morning" were present, in the paragraph. Presence
        // was never the property. POSITION was.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        var sameDay = section.IndexOf("When to call the team the same day",
            StringComparison.OrdinalIgnoreCase);
        Assert.True(sameDay > 0, "the caregiver section has no 'call the same day' list");

        var straightAway = section.IndexOf("When to call the team straight away",
            StringComparison.OrdinalIgnoreCase);
        Assert.True(straightAway > 0, "the caregiver section has no 'call straight away' list");
        Assert.True(straightAway < sameDay,
            "the same-day list comes first, so a reader meets the lesser urgency first");

        // The temperature belongs to the straight-away list and must not appear
        // anywhere in the same-day one.
        Assert.DoesNotContain("100.4", section[sameDay..], StringComparison.Ordinal);
        Assert.Contains("100.4", section[straightAway..sameDay], StringComparison.Ordinal);
    }

    [Fact]
    public void TheCaregiverEscalationListDoesNotContradictTheSeizurePage()
    {
        // The WI-511 rule: diff every escalation list against the page that
        // owns the emergency, in BOTH directions.
        //
        // This page's list deliberately does not mention seizures at all — the
        // chemotherapy drugs on it are not the reason someone with a brain
        // tumor seizes, and the shared [CAREGIVER] block already links the page
        // that owns it. But "deliberately absent" has to be asserted, or it is
        // indistinguishable from an omission that drifts back in as a bare
        // "a seizure" bullet the way WI-511's did.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        foreach (Match match in Regex.Matches(section, @"seizure", RegexOptions.IgnoreCase))
        {
            var around = section[Math.Max(0, match.Index - 200)..Math.Min(section.Length, match.Index + 200)];
            Assert.True(
                Regex.IsMatch(around, @"first ever|more than five minutes|straight into another",
                    RegexOptions.IgnoreCase),
                "the caregiver list mentions a seizure without the qualifier "
                + "/seizures/what-to-do uses, which is the WI-511 blocker returning");
        }
    }

    [Fact]
    public void ThePageCarriesTheSlotsADrugsPageHasAndDropsSlotFiveOnPurpose()
    {
        // §12.8's first WI-510 rule: read the standard for the slot list, not
        // the previous page. A drugs page keeps 0, 1, 2, 3, 4, 6a, 6b, 6c, 7,
        // 8, 9, 10, 11 — and drops slot 5.
        //
        // Slot 5 is "what does it feel like?", and it has no referent here.
        // Four drugs, given four different ways, over months, do not share an
        // answer: temozolomide is capsules at home, vincristine is a line in
        // your arm, wafers are placed while you are asleep. Forcing the slot
        // would produce a section that answers nothing, which is the WI-506
        // heading trap. §12.8 already records that slots 2 and 5 are not
        // universal, and this is the third page to drop 5.
        var headings = Headings();

        Assert.Contains("Why am I having it?", headings);                        // slot 2
        Assert.Contains("The drugs, one at a time", headings);                   // slot 3
        Assert.Contains("How long does it go on?", headings);                    // slot 4
        Assert.Contains(FeverHeading, headings);                                 // slot 6a
        Assert.Contains("Why am I on an antibiotic?", headings);                 // slot 6c
        Assert.Contains("What you need first", headings);                        // slot 7
        Assert.Contains("Why has my treatment been delayed or stopped?", headings); // slot 8
        Assert.Contains("Who reads it, and how do I get the result?", headings); // slot 9

        // Slot 5, restored after review. Headed as a role rather than copied
        // verbatim (WI-507): the reader's question here is about the CYCLE,
        // which is the unit this treatment actually comes in.
        Assert.Contains("What does a cycle feel like?", headings);               // slot 5

        // Slot 6b, which the first version of this test omitted entirely — the
        // one slot §12.8 says a treatment page always carries, unpinned on a
        // treatment page. Both headings, because the pair is the content.
        Assert.Contains("Side effects, and what is done about them", headings);  // slot 6b
        Assert.Contains("Feeling sick, and the medicine for it", headings);      // slot 6b
    }

    [Fact]
    public void TheCycleSectionSaysYouFeelWorstWhenYouFeelFinished()
    {
        // The whole reason slot 5 earns its place here rather than being a
        // restatement of the side-effects list: the nadir lands in the week
        // after the dose, when the cycle feels over. That is the one thing
        // about the shape of a cycle a reader cannot infer, and it is the
        // same fact the fever rule depends on.
        var section = CuratedPage.Flatten(Section("What does a cycle feel like?"));

        Assert.Matches(
            new Regex(@"you feel worst when you\s*\*?\*?feel finished", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"about a week to ten days\s*after the dose", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheProcarbazineFoodAndDrinkRuleIsOnThePage()
    {
        // Review's blocker, and the dossier flagged it as "a genuine safety
        // item" that needed a non-AHFS source before publishing. CRUK — already
        // cited on this page — is that source, and covers it at length. It is
        // the one thing on a page about capsules-you-take-at-home that a reader
        // can get wrong at home, tonight, with no warning from anybody.
        var section = CuratedPage.Flatten(Section(DrugsHeading));

        Assert.Matches(new Regex(@"food and drink rule with procarbazine", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Aa]lcohol can cause a reaction", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"[Aa]sk for the list before your first capsule", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheAntibioticSectionPrintsTheDisagreementItsOwnSourceReports()
    {
        // The section cited PMC12803824 and then stated the pre-2026 position
        // as settled — "it is the standard thing to do" — while that paper's
        // entire argument is that universal prophylaxis is no longer supported
        // and only about half of patients in the pooled studies received it.
        // Citing a source for the position it argues against is a worse failure
        // than not citing it, and WI-511's "print the disagreement" rule is
        // exactly the fix.
        var section = CuratedPage.Flatten(Section("Why am I on an antibiotic?"));

        Assert.Matches(
            new Regex(@"Not every team gives it, and that is a real difference of opinion",
                RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"it is the standard thing to do", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCaregiverSectionIsPlacedAfterSlotSevenAndBeforeTheRepeatSection()
    {
        // §12.7 fixes the position.
        var headings = Headings();

        var preparation = headings.IndexOf("What you need first");
        var caregiver = headings.IndexOf(CaregiverHeading);
        var repeat = headings.IndexOf("Why has my treatment been delayed or stopped?");

        Assert.True(preparation >= 0 && caregiver >= 0 && repeat >= 0,
            "one of the three §12.8 slots around the caregiver section has been renamed");
        Assert.Equal(preparation + 1, caregiver);
        Assert.Equal(caregiver + 1, repeat);
    }

    [Fact]
    public void TheAnchorsOtherPagesWillDeepLinkAreWrittenExplicitly()
    {
        // §12.8, WI-509's mechanism. The four drug anchors are what the tumor
        // hubs will point at — "your tumor is usually treated with
        // [temozolomide](/treatments/chemotherapy#temozolomide)" is the shape
        // WI-513 onward will need, and a reworded heading breaks all of them
        // silently.
        //
        // Two of these four would NOT be caught by the render test alone,
        // because Markdig derives the same id from the heading text
        // ("Temozolomide" → temozolomide, "PCV" → pcv). The two that carry real
        // weight are `{#lomustine}` on "Lomustine on its own" and
        // `{#fever-rule}` on a long heading. Pinning the source here is what
        // proves the author wrote all of them (the WI-511 honesty note).
        foreach (var anchor in new[] { "temozolomide", "pcv", "lomustine", "carmustine-wafers" })
        {
            Assert.Matches(new Regex(@"^### .*\{#" + anchor + @"\}", RegexOptions.Multiline), Page);
        }

        Assert.Matches(new Regex(@"^## .*\{#fever-rule\}", RegexOptions.Multiline), Page);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        Assert.DoesNotContain(":::", Page, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        var headings = Headings();

        Assert.Equal("The short version", headings[0]);
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);
    }

    [Fact]
    public void TheQuestionsListLeadsWithTheOnesThatChangeWhatHappensAtHome()
    {
        // Shared contract item 7. On this page the printable list is not a
        // courtesy — the reader is going home with capsules and a phone
        // number, and two of these questions are the difference between a
        // manageable night and an emergency department at 4am.
        var section = CuratedPage.Flatten(Section("What to ask your team"));

        Assert.Matches(
            new Regex(@"What temperature means I call you, and what is the number out of hours",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"Am I going to be on an antibiotic", RegexOptions.IgnoreCase), section);
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

        // Named, not counted: ACS carries the drug list and the fever
        // threshold, EANO the nitrosourea timing, the Brain Tumour Charity the
        // temozolomide effects, CRUK the oral/IV split, and the PMC papers the
        // PCV toxicity, the wafer question and the antibiotic recommendation.
        foreach (var domain in new[]
                 {
                     "cancer.org", "ncbi.nlm.nih.gov", "thebraintumourcharity.org",
                     "cancerresearchuk.org",
                 })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheSourcesThatCannotBeUsedNeverComeBack()
    {
        // Four from this item, and one of them is a LICENSING rule rather than
        // a citation-quality one:
        //
        // - drugs.com hosts AHFS monographs. PLAN.md §5 and §12.2 item 6 forbid
        //   AHFS and MedlinePlus drug monographs outright. The dossier cites it
        //   for the antibiotic-prophylaxis claim. Not usable at any strength,
        //   however true the claim is.
        // - PMC3601076, cited for "temozolomide causes profound lymphopenia",
        //   is a MOUSE study — "Myeloablative Temozolomide Enhances CD8+ T-Cell
        //   Responses to Vaccine ... in Mice". Third rodent citation in six
        //   items, after WI-507's fixation figure and WI-511's mouse-brain
        //   review.
        // - cancercareontario.ca, the dossier's lomustine monograph, sits
        //   behind a WAF and returns 106 bytes to any fetch. A citation nobody
        //   can open is a citation nobody can verify (§12.8). The EANO
        //   guideline carries the same fact and is open.
        // - NBK66023 is NCI's patient PDQ, forbidden by §12.1 for naming, and
        //   the dossier's whole which-drug table is built on it — including a
        //   row headed "Anaplastic astrocytoma".
        var front = CuratedPage.FrontMatter(Page);

        foreach (var url in new[] { "drugs.com", "PMC3601076", "cancercareontario", "NBK66023" })
        {
            Assert.DoesNotContain(url, front, StringComparison.OrdinalIgnoreCase);
        }
    }

    // The trailing `{#id}` is stripped, so a caller names a section by the
    // words a reader sees rather than by the anchor bolted onto it — the same
    // reason CuratedPage.Section() does it. Without this the slot test failed
    // on "Your blood counts, and the fever rule {#fever-rule}", which is the
    // wording-versus-interface coupling WI-509 introduced explicit anchors to
    // break in the first place.
    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())];

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

/// <summary>The page as served, the door that leads to it, and the tooltips.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class ChemotherapyPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/chemotherapy";

    private readonly WebApplicationFactory<Program> _factory;

    public ChemotherapyPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // The page's own title, not the word "chemotherapy" — the WI-511
        // lesson that an assertion has to distinguish THIS page from any page
        // about the subject.
        Assert.Contains("your blood counts, and the fever rule", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageIsReachableFromWhereANewlyDiagnosedReaderStarts()
    {
        var html = await _factory.CreateClient().GetStringAsync("/start");

        Assert.Contains(Url, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors this page owes its reader: the two treatments it runs
        // alongside and must not re-explain, the gene results that decide which
        // drug it is, and a person to talk to.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/treatments/radiation-therapy", "/treatments/craniotomy",
            "/tests/molecular-markers", "/tests/waiting-for-results", "/tests/mri",
            "/get-help-now");

    [Fact]
    public async Task TheSharedCaregiverBlockComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.DoesNotContain("[CAREGIVER]", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAlreadyDefinedVocabularyStillFiresAsTooltipsHere()
    {
        // Contract item 9 from the other direction. temozolomide has had an
        // entry since WI-505 and this page names it constantly without writing
        // a competing definition, so the tooltip is where a reader gets the
        // short form.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("def-temozolomide", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheWordsThisPageDefinesItselfDoNotAlsoFireATooltip()
    {
        // §12.8, WI-509. Each of these six gets a paragraph or a section on
        // this page, so a popover repeating it is noise.
        var page = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[]
                 {
                     "procarbazine", "vincristine", "lomustine",
                     "carmustine-wafer", "neutropenia", "nadir",
                 })
        {
            Assert.DoesNotContain($"def-{slug}\"", page, StringComparison.Ordinal);
        }

        // A suppression only means something if the word is THERE to suppress,
        // and two of these six were suppressing nothing. Review found it:
        // "neutropenia" appeared nowhere in reader prose, so
        // `DoesNotContain("def-neutropenia")` passed because the term could
        // never render — on this page or any other, since no page in the
        // corpus used the word. And the glossary matcher is whole-word, so
        // `carmustine wafer` never matched this page's plural "wafers"
        // (fixed with an `also:` alias). WI-510's rule: a "both directions"
        // assertion is only worth writing if both directions are observable.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "chemotherapy.md")));

        foreach (var term in new[]
                 {
                     "procarbazine", "vincristine", "lomustine",
                     "carmustine wafer", "neutropenia", "nadir",
                 })
        {
            Assert.True(reader.Contains(term, StringComparison.OrdinalIgnoreCase),
                $"'{term}' is suppressed on this page but never appears in its prose, "
                + "so the suppression is a no-op and the glossary entry is unreachable");
        }

        // The leak that IS possible, per WI-510's review: a `!%term%` marker
        // written into a glossary entry or a shared block suppresses that term
        // on every including page at once, silently.
        foreach (var (_, text) in CuratedPage.SharedSources())
        {
            Assert.DoesNotContain("!%", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheNewVocabularyIsStillReachableEvenThoughItIsSuppressedHere()
    {
        // Suppressing a term here must not be the reason it exists nowhere.
        var html = await _factory.CreateClient().GetStringAsync("/glossary");

        foreach (var slug in new[]
                 {
                     "procarbazine", "vincristine", "lomustine",
                     "carmustine-wafer", "neutropenia", "nadir",
                 })
        {
            Assert.Contains($"id=\"{slug}\"", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheDeepLinkAnchorsRenderWithTheIdsOtherPagesWillUse()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var id in new[]
                 {
                     "temozolomide", "pcv", "lomustine", "carmustine-wafers",
                     "fever-rule", "where-to-go-next",
                 })
        {
            Assert.Contains($"id=\"{id}\"", html, StringComparison.Ordinal);
        }
    }
}