using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-510: T-X2 craniotomy, the FIRST page of the treatment library and the
/// first library page since WI-506 to use the full twelve slots of §12.8.
/// WI-507, WI-508 and WI-509 each dropped slots 2, 5, 7 and 8 because none of
/// them was a procedure with a day; this page has all four back.
///
/// The shared reading helpers live on <see cref="CuratedPage"/> in
/// TestsLibraryPagesTests.cs. They are shared across all 29 library pages on
/// purpose — see §12.8's "factor at the second use, not the fifth".
///
/// As with the tests library: the prose is reviewed, not tested. What is
/// pinned here is the handful of places this page could give a reader actively
/// wrong information, and the things later treatment pages inherit.
/// </summary>
public sealed class CraniotomyPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "craniotomy.md");

    // Reader text, not raw source: this page suppresses six tooltips with the
    // WI-105 `!%term%` marker, and those markers are authoring instructions
    // rather than prose (§12.8, WI-509).
    private static string Reader => CuratedPage.ReaderText(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string[] SentencesOf(string section) => CuratedPage.SentencesOf(section);

    private const string ResectionHeading = "\"How much did you get out?\"";

    [Fact]
    public void TheResectionSectionSaysTheAnswerComesFromTheScanAndNotFromTheRoom()
    {
        // The load-bearing correction on the page. "Gross total" is defined by
        // the post-operative MRI, not by what the surgeon believed they saw
        // (ACS: an MRI or CT is typically done 1 to 3 days after the operation
        // to confirm how much of the tumor has been removed). A reader who
        // thinks the surgeon's impression is the answer has no way to make
        // sense of a scan that later says otherwise.
        var section = Section(ResectionHeading);

        Assert.Matches(new Regex("scan", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not (come )?from what the surgeon saw|not .{0,40}surgeon saw in the room",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheResectionSectionHoldsBothHalvesOfTheGrossTotalMessage()
    {
        // §12.8's "answer the frightening question in both directions", applied
        // to the cruellest misunderstanding in SYNTHESIS §3.6.
        //
        // Half one: no visible tumor is not cure, because in a diffuse glioma
        // cells too small to see are left behind. Half two: for some tumor
        // types surgery alone really can deal with it. Publishing only the
        // first tells every meningioma reader they can never be rid of it;
        // publishing only the second is the lie that detonates months later.
        var section = Section(ResectionHeading);

        Assert.Matches(new Regex(@"does not mean cure", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"too small", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"surgery alone can deal with it", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheResectionSectionDoesNotLeaveTheReaderOnTheFrighteningHalf()
    {
        // §12.6: never end a section on a frightening sentence. This is a
        // review rule that a test can actually hold, because the failure is
        // positional — the reassurance drifting upward as the section is
        // edited is exactly how it would break.
        // Pins the LAST sentence, the way WI-506's original does. The first
        // version here pinned a three-sentence window and only required a
        // reassuring phrase somewhere inside it, so the reassurance could sit
        // third-from-last and the section could still close on two fresh
        // frightening sentences.
        var sentences = SentencesOf(Section(ResectionHeading));

        Assert.DoesNotMatch(
            new Regex(@"too small|does not mean cure|would not want to lose",
                RegexOptions.IgnoreCase),
            sentences[^1]);
        Assert.Matches(
            new Regex(@"right call|not a failed operation|the right decision",
                RegexOptions.IgnoreCase),
            string.Join(" ", sentences[^2..]));
    }

    [Fact]
    public void ThePageNeverTellsAReaderToAskTheSurgeonToTakeItAll()
    {
        // The WI-506 open-MRI rule in the place it matters most on this page.
        // The aim is maximal SAFE resection; how much that is, is decided by
        // what the tumor is sitting next to. A reader who walks in asking for
        // everything to come out is asking for the one thing that could cost
        // them a function they will not get back, and the page must never
        // nudge them there.
        // `[^.\n]` rather than `[^.]`: the first version of this test spanned a
        // heading and matched the page's OWN questions list, where "What is the
        // goal of my operation: to take it all out, to take some out..." is
        // exactly right — the reader is asking what the goal IS, not demanding
        // that everything comes out. A rule that fails a correct page is worse
        // than no rule (§12.8), and the newline is what separates the two
        // meanings here.
        Assert.DoesNotMatch(
            new Regex(@"ask (your |the )?(surgeon|team|doctor)[^.\n]{0,60}(take it all|remove it all|get it all)",
                RegexOptions.IgnoreCase),
            Reader);

        // And it says the opposite out loud, so the point is made rather than
        // merely not-contradicted.
        Assert.Matches(
            new Regex(@"goal is not always", RegexOptions.IgnoreCase), Reader);
    }

    [Fact]
    public void TheSmaSectionNamesTheSyndromeAndSaysItIsExpectedToPass()
    {
        // SYNTHESIS §3.5 and the ticket both single this out: unwarned patients
        // believe they have been left permanently disabled. Naming it without
        // saying it is expected to pass would be worse than not raising it,
        // so both halves are pinned together in the same section.
        var section = Section("What can go wrong");

        Assert.Matches(new Regex(@"SMA syndrome", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"expected to pass|usually begins to improve", RegexOptions.IgnoreCase),
            section);

        // Polarity-aware. The first version asserted "permanently disabled" was
        // merely PRESENT, which "you may be left permanently disabled" also
        // satisfies — the WI-508 pattern this file's header says it learned
        // from, reintroduced. The phrase is only ever allowed to appear as
        // something people wrongly BELIEVE.
        foreach (Match match in Regex.Matches(section, @"permanently disabled", RegexOptions.IgnoreCase))
        {
            var before = section[Math.Max(0, match.Index - 80)..match.Index];
            Assert.Matches(
                new Regex(@"believe|think|fear|convinced", RegexOptions.IgnoreCase), before);
        }
    }

    [Fact]
    public void TheRecoverySectionPairsTheDayTwoOrThreeDipWithTheImprovementAfterIt()
    {
        // "One sentence prevents a panic" — but only the pair does. The dip on
        // its own is a page telling a frightened person they are getting worse.
        //
        // Polarity-aware on purpose: WI-508 found its own NOS/NEC test
        // asserting that a word was merely PRESENT, which the exact sentence it
        // existed to prevent would also have satisfied. The improvement has to
        // be findable in the same paragraph, after the dip.
        var section = Section("What is different in the weeks after");

        var dip = Regex.Match(section,
            @"[Dd]ay two or three often feels worse than day one\.(.{0,400})",
            RegexOptions.Singleline);

        Assert.True(dip.Success, "the day two-or-three dip is no longer stated in its own sentence");
        Assert.Matches(
            new Regex(@"should be improving", RegexOptions.IgnoreCase), dip.Groups[1].Value);
    }

    [Fact]
    public void TheIsItTheDrugSectionNamesBothMedicinesAndTellsNobodyToStopThem()
    {
        // §12.6's recurring frame. Naming the effects without the "do not stop
        // it yourself" is a page that gets someone's steroid stopped abruptly,
        // which is its own emergency.
        var section = Section("What is different in the weeks after");

        Assert.Matches(new Regex(@"[Dd]examethasone", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Ll]evetiracetam", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"muscle weakness", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"Neither of these is a reason to stop a medicine on your own",
                RegexOptions.IgnoreCase),
            section);
        // The property is that nobody is told to stop a steroid themselves, and
        // it used to be pinned as "reduced slowly". WI-524 found that wording
        // false of a short post-operative course — PMC4059813 records a fast
        // taper "discontinued 3 days after resection", which is exactly this
        // page's reader — so the page now states the rule that holds in both
        // cases. /treatments/steroids owns the full version.
        Assert.Matches(new Regex(@"never one to stop by yourself", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"your team's decision", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(new Regex(@"(?:have|has) to be reduced slowly", RegexOptions.IgnoreCase), section);
        Assert.Contains("/treatments/steroids", section, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoRiskPercentageNoDoseAndNoRadiationFigure()
    {
        // §12.4 R2. This is the page R2 was written for: the SMA source alone
        // reports the deficit risk as anywhere from 23% to 100%, and awake
        // craniotomy seizure risk runs from 2.9% to 54% across the literature.
        // Any single number would be a fiction dressed as precision.
        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*%", Reader);
        Assert.DoesNotMatch(
            new Regex(@"\b\d+(\.\d+)?\s*(Gy|mg|ml|mmol|Tesla)\b", RegexOptions.IgnoreCase), Reader);
    }

    [Fact]
    public void ThePageSaysWhyItPublishesNoRiskNumberRatherThanSilentlyOmittingOne()
    {
        // A reader who came for a number and found none should learn that the
        // absence is a decision, not an oversight — otherwise the page reads as
        // evasive on exactly the question they are most afraid of, and they go
        // and find a worse number somewhere else.
        var section = Section("What can go wrong");

        Assert.Matches(
            new Regex(@"no percentage on this page, and that is deliberate",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"vary so widely", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"surgeon can tell you", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageNeverCharacterisesTheOutcome()
    {
        // The shared §12.8 ban list. Extended by this item with the three
        // treatment-page shapes; the six candidates that failed the corpus scan
        // are recorded in CuratedPage.RejectedCharacterisations with reasons.
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageNeverMinimisesTheOperation()
    {
        // Deliberately page-local rather than site-wide, per §12.8: "before a
        // per-page property is promoted to a site-wide one, run it against the
        // pages already shipped". These are clean corpus-wide today, but the
        // rule only has an obvious meaning on a page about an operation.
        // WI-511 or WI-512 is where it earns promotion to CuratedPage.
        // Negation-aware, because "this is not a minor operation" is the
        // natural and CORRECT sentence a treatment page wants — the identical
        // shape that got "good sign"/"bad sign" rejected at WI-509. A bare
        // substring ban would fail a correct page, which §12.8 says is worse
        // than no rule at all.
        foreach (var phrase in new[]
                 {
                     "routine operation", "routine procedure", "simple operation",
                     "minor operation", "straightforward operation",
                 })
        {
            foreach (Match match in Regex.Matches(Reader, Regex.Escape(phrase), RegexOptions.IgnoreCase))
            {
                var before = Reader[Math.Max(0, match.Index - 30)..match.Index];
                Assert.Matches(
                    new Regex(@"\bnot\b|\bnever\b|\bhardly\b", RegexOptions.IgnoreCase), before);
            }
        }
    }

    [Fact]
    public void TheLaserSectionNeverReadsAsTheBetterOptionEveryoneCouldHave()
    {
        // LITT is a section here rather than a page (the ticket, and the
        // research's own recommendation). The risk in describing a less
        // invasive option is that a reader asks for it instead of the operation
        // they actually need, so the honest limits travel with it: ACS says the
        // technique is still fairly new and doctors are still learning where it
        // is the right choice.
        var section = Section("Is there a way to do this without opening the skull?");

        Assert.Matches(new Regex(@"fairly new", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not offered everywhere", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not is a gentler route to the same result", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"whether it applies to you rather than asking for it",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void ThePageLinksTheMarkersRatherThanExplainingThemAgain()
    {
        // /tests/molecular-markers owns the markers as of WI-509. Two pages
        // explaining IDH is how they drift apart, and the marker page is the
        // one that was reviewed hard.
        Assert.Contains("/tests/molecular-markers", Reader, StringComparison.Ordinal);

        // The shapes an explanation would take. Naming a marker in a link text
        // is fine; telling the reader what it means is not.
        Assert.DoesNotMatch(
            new Regex(@"(IDH|MGMT|1p/19q|ATRX|TERT)[^.\n]{0,40}\b(means|tells|shows|is a gene)\b",
                RegexOptions.IgnoreCase),
            Reader);
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheSharedBlockRatherThanRestatingIt()
    {
        // §12.7: the shared half is a block, the page writes only its own half.
        // The second copy of shared prose is where drift starts — WI-507 found
        // the tumor-board paragraph had already lost its best line by the time
        // it was written out twice.
        var section = Section("For the person caring for someone after surgery");

        Assert.Contains("[CAREGIVER]", section, StringComparison.Ordinal);

        // A real overlap check, not a check on the block's four bold headings.
        //
        // The first version asserted only that the block's LEAD-INS were
        // absent. Review found four genuine duplications that were none of
        // them — "Nobody on that team will mind", the /seizures/what-to-do
        // link, "give them something real to do" and "Two people hear more
        // than one" — so the reader met each of them twice, a few paragraphs
        // apart, while the test that names this exact failure stayed green.
        //
        // Word shingles catch a restatement whatever shape it arrives in.
        // Eight words is long enough that shared vocabulary ("call your team",
        // "the hospital") does not trip it, and short enough that a lightly
        // reworded copy still does.
        // "Where to go next" is excluded: it is an index of onward doors by
        // design, so a link the block also makes is expected to reappear
        // there. Everywhere else, a repeat is the reader being told the same
        // thing twice.
        var body = CuratedPage.ReaderText(Page).Replace("[CAREGIVER]", " ", StringComparison.Ordinal);
        var next = body.IndexOf("## Where to go next", StringComparison.Ordinal);
        var page = next > 0 ? body[..next] : body;

        static IEnumerable<string> Shingles(string text, int n)
        {
            var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+")
                .Select(m => m.Value).ToList();
            for (var i = 0; i + n <= words.Count; i++)
            {
                yield return string.Join(' ', words.Skip(i).Take(n));
            }
        }

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
        // Split out from the shingle check because a duplicated LINK is not a
        // duplicated sentence, and this one is the most likely to come back:
        // the block already carries both seizure links, and a treatment page
        // writing about aftercare reaches for them naturally.
        var block = CuratedPage.Body(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")));

        foreach (var link in new[] { "/seizures/what-to-do", "/seizures/living-with" })
        {
            if (!block.Contains(link, StringComparison.Ordinal))
            {
                continue;
            }

            var section = Section("For the person caring for someone after surgery");
            Assert.DoesNotContain(link, section.Replace("[CAREGIVER]", " ", StringComparison.Ordinal),
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheCaregiverSectionSeparatesCallTodayFromCallAnAmbulance()
    {
        // The single most load-bearing thing in the section, and the reason
        // contract item 12 exists. A merged list is a list that gets read
        // slowly during an emergency.
        var section = Section("For the person caring for someone after surgery");

        var today = section.IndexOf("When to call the team the same day", StringComparison.Ordinal);
        var ambulance = section.IndexOf("When to call an ambulance", StringComparison.Ordinal);

        Assert.True(today > 0, "the caregiver section has no 'call the team today' list");
        Assert.True(ambulance > today, "the ambulance list is missing, or sits before the call-today list");

        // "If you are not sure, call" has to sit AFTER both lists. It is the
        // instruction that makes an incomplete list safe.
        var whenUnsure = section.IndexOf("If you are not sure which list this is",
            StringComparison.Ordinal);
        Assert.True(whenUnsure > ambulance, "the 'if you are not sure, call' line is missing or misplaced");

        // The ambulance list has to carry a seizure: it is the commonest way a
        // brain tumor announces itself.
        //
        // Scoped to the LIST, not to the rest of the section. The first version
        // searched from the ambulance heading to the end, which swept in a
        // later "[what to do when someone has a seizure]" link — so deleting
        // the seizure bullet outright left the test green.
        var ambulanceList = section[ambulance..whenUnsure];
        Assert.Matches(new Regex(@"[Aa] seizure", RegexOptions.IgnoreCase), ambulanceList);

        // And it must be the QUALIFIED seizure, not every seizure.
        // /seizures/what-to-do says most seizures do not need an ambulance, and
        // this page links there six lines later. A caregiver who reads "call an
        // ambulance: a seizure" calls one every time.
        Assert.Matches(
            new Regex(@"first|more than five minutes|runs straight into another",
                RegexOptions.IgnoreCase),
            ambulanceList);
    }

    [Fact]
    public void TheCaregiverSectionSaysHowLongTheJobLasts()
    {
        // Dan's ask, verbatim: "how long they should expect to be doing it".
        // Every comparator leaves this out, and it is the question a caregiver
        // cannot ask out loud in front of the patient.
        var section = Section("For the person caring for someone after surgery");

        Assert.Matches(
            new Regex(@"How long are you doing this for", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"month or two", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCaregiverSectionTeachesTheWoundCareNobodyIsTaught()
    {
        // NICE and the burden study both found caregivers doing dressing
        // changes with no instruction at all. This is the aftercare Dan named
        // when he asked for the block in the first place.
        var section = Section("For the person caring for someone after surgery");

        Assert.Matches(new Regex(@"[Kk]eep it dry", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"staples", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"steroid", RegexOptions.IgnoreCase), section);
        // Same correction as above (WI-524): the caregiver is told the rule
        // that is true of both a three-day course and a three-month one.
        Assert.Matches(new Regex(@"never stopped by you", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(new Regex(@"must not be stopped suddenly", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCaregiverSectionIsPlacedAfterSlotSevenAndBeforeTheRepeatSection()
    {
        // §12.7 fixes the position. Asserted because it is invisible in review:
        // the section reads fine anywhere, and the ordering is the part that
        // encodes "you have been given everything to act on before we ask you
        // to take on a job".
        //
        // Anchors stripped since WI-523, which pinned `{#caregiver}` on the
        // heading because /treatments/awake-craniotomy deep-links to it. The
        // words the reader sees are what this test is about (§12.8, WI-509).
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

        var preparation = headings.IndexOf("What you need first, and what to bring");
        var caregiver = headings.IndexOf("For the person caring for someone after surgery");
        var repeat = headings.IndexOf("Why am I having another operation?");

        Assert.True(preparation >= 0 && caregiver >= 0 && repeat >= 0,
            "one of the three §12.8 slots around the caregiver section has been renamed");
        Assert.Equal(preparation + 1, caregiver);
        Assert.Equal(caregiver + 1, repeat);
    }

    [Fact]
    public void ThePageUsesTheFullTwelveSlotTemplate()
    {
        // The point of the item. WI-507, WI-508 and WI-509 dropped slots 2, 5,
        // 7 and 8 because a document and a wait have no day and no procedure;
        // §12.8 records that only 0, 1, 3, 4, 9 and 10 are universal. This page
        // is a procedure with a day, so the dropped slots come back, and this
        // test exists so the next treatment page does not silently inherit
        // three items' worth of bends.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Contains("Why am I having one?", headings);                    // slot 2
        Assert.Contains("What does it feel like?", headings);                 // slot 5
        Assert.Contains("What you need first, and what to bring", headings);  // slot 7
        Assert.Contains("Why am I having another operation?", headings);      // slot 8
    }

    [Fact]
    public void TheAnchorsOtherPagesWillDeepLinkAreWrittenExplicitly()
    {
        // §12.8, WI-509's mechanism. Markdig derives an id from heading TEXT,
        // so rewording "How much did you get out?" would silently break every
        // inbound link and land the reader at the top of a long page with no
        // sign anything went wrong. These two are the ones the 23 tumor hubs
        // will point at.
        Assert.Matches(new Regex(@"^## .*\{#how-much-came-out\}", RegexOptions.Multiline), Page);
        Assert.Matches(new Regex(@"^### .*\{#sma-syndrome\}", RegexOptions.Multiline), Page);

        // WI-523: /treatments/awake-craniotomy hands the reader to three more
        // sections rather than restating them — the stay, the general risks,
        // and the caregiver's whole job after surgery.
        Assert.Matches(new Regex(@"^## How long does it take\? \{#how-long\}", RegexOptions.Multiline), Page);
        Assert.Matches(new Regex(@"^## What can go wrong \{#what-can-go-wrong\}", RegexOptions.Multiline), Page);
        Assert.Matches(new Regex(@"^## For the person caring for someone after surgery \{#caregiver\}",
            RegexOptions.Multiline), Page);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone, and §12.8 records that collapsing
        // anything else was considered and rejected. A fence here means
        // something has drifted.
        Assert.DoesNotContain(":::", Page, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        // Shared contract item 7, and the last two slots of §12.8. The .Trim()
        // is load-bearing on a CRLF checkout, not cosmetic: `.` matches `\r`.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

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

        // The domains the page's claims actually rest on, named rather than
        // counted: NBTS carries the patient experience and the day two-or-three
        // dip, ACS the mechanics and the post-operative scan, ABTA the reasons
        // for operating and the steroid effects, and the PMC papers SMA
        // syndrome and levetiracetam.
        foreach (var domain in new[]
                 {
                     "braintumor.org", "cancer.org", "abta.org",
                     "ncbi.nlm.nih.gov", "ummhealth.org", "kaiserpermanente.org",
                 })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor for the same CRLF reason recorded in WI-506's copy.
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheDossierSourcesThatDoNotSayWhatWasClaimedNeverComeBack()
    {
        // Every P5 item so far has found the research dossier mis-citing
        // something, and a wrong citation that RESOLVES is the worst failure
        // mode on a medical site. Three from this item, pinned by name:
        //
        // - PMC5358612 is cited for "gross total resection is defined by the
        //   post-op MRI". It is a conference abstract (PP32) on survival
        //   outcomes at a single centre. ACS carries the claim properly.
        // - PMC7093492 is cited for "GTR is not always the goal". It is a
        //   survival meta-analysis in elderly patients — wrong on the claim and
        //   wrong on a page that publishes no prognosis figures (§12.2 item 5).
        // - intechopen.com/chapters/64710 is the dossier's source for the
        //   resection vocabulary; ABTA defines all three terms itself.
        var front = CuratedPage.FrontMatter(Page);

        foreach (var url in new[] { "PMC5358612", "PMC7093492", "intechopen" })
        {
            Assert.DoesNotContain(url, front, StringComparison.OrdinalIgnoreCase);
        }
    }
}

/// <summary>The page as served, the door that leads to it, and the tooltips.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class CraniotomyPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/craniotomy";

    private readonly WebApplicationFactory<Program> _factory;

    public CraniotomyPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        // Also the first proof that /treatments/<slug> routes at all. WI-506
        // set the scheme; this is the page that opens it.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Brain surgery", html, StringComparison.Ordinal);
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
        // The doors this page owes its reader: the three pathology pages,
        // because the operation is where the tissue comes from and the answer
        // arrives later; the seizure page, because the caregiver block sends
        // them there; and a person to talk to.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/tests/waiting-for-results", "/tests/pathology-report",
            "/tests/molecular-markers", "/seizures/what-to-do", "/get-help-now");

    [Fact]
    public async Task TheSharedBlocksComposeIntoTheRenderedPage()
    {
        // [CAREGIVER] and [TUMOR-BOARD] are directives in the source. If a
        // block silently failed to resolve, the page would still render and the
        // fullest caregiver section on the site would simply be absent.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.DoesNotContain("[CAREGIVER]", html, StringComparison.Ordinal);
        Assert.DoesNotContain("[TUMOR-BOARD]", html, StringComparison.Ordinal);

        // A sentence from each block, so "resolved" means "composed", not
        // "quietly replaced with nothing".
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
        Assert.Contains("advice, not an order", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheNewVocabularyFiresAsTooltipsOnTheRealPage()
    {
        // Contract item 9. Asserted on the rendered page rather than on the
        // glossary directory: a term file nothing matches is a term nobody ever
        // sees. "def-<slug>" is the popover the tooltip button targets, so this
        // cannot pass on prose that was never marked up.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "awake-craniotomy", "eloquent-cortex", "dexamethasone" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheWordsThisPageDefinesItselfDoNotAlsoFireATooltip()
    {
        // §12.8, WI-509: a popover repeating the paragraph directly beneath it
        // is noise. Asserted in both directions — suppressed here, and still
        // firing on a page that does NOT define them — because a suppression
        // that leaked into the glossary itself would delete the term site-wide.
        var client = _factory.CreateClient();
        var page = await client.GetStringAsync(Url);

        foreach (var slug in new[] { "craniotomy", "gross-total-resection", "subtotal-resection", "debulking" })
        {
            Assert.DoesNotContain($"def-{slug}\"", page, StringComparison.Ordinal);
        }

        // The other direction, done properly.
        //
        // The first version fetched /glossary and asserted the six slugs
        // appear there. But /glossary renders from the glossary directory and
        // knows nothing about any page's suppression state, so a leaked
        // suppression could never have failed it — it asserted that six .md
        // files exist.
        //
        // The obvious replacement, "the term still fires on a page that does
        // NOT define it", is not available: no other page in the corpus uses
        // any of these six words in prose. /start contains "craniotomy" only
        // inside a URL, and tooltips never fire inside links. Asserting it
        // anyway would be a test that passes for a reason unrelated to what it
        // claims, which is the thing this whole pass is cleaning up.
        //
        // So assert the leak that IS possible and IS the one the comment names:
        // a `!%term%` marker written into a glossary entry or a shared block
        // rather than into a page. A block composes into every including page,
        // so a marker there suppresses the term across all of them at once,
        // silently.
        foreach (var (slug, text) in CuratedPage.SharedSources())
        {
            Assert.DoesNotContain("!%", text, StringComparison.Ordinal);
            _ = slug;
        }
    }

    [Fact]
    public async Task TheDeepLinkAnchorsRenderWithTheIdsOtherPagesWillUse()
    {
        // The published-interface half of the explicit-anchor rule. Pinning the
        // source (content test) proves the author wrote them; this proves
        // Markdig emitted them.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("id=\"how-much-came-out\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"sma-syndrome\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"where-to-go-next\"", html, StringComparison.Ordinal);
    }
}
