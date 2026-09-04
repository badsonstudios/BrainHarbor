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
        var sentences = SentencesOf(Section(ResectionHeading));
        var tail = string.Join(" ", sentences[^3..]);

        Assert.DoesNotMatch(new Regex(@"too small|does not mean cure", RegexOptions.IgnoreCase), tail);
        Assert.Matches(
            new Regex(@"right call|not a failed operation", RegexOptions.IgnoreCase), tail);
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
        Assert.Matches(
            new Regex(@"permanently disabled", RegexOptions.IgnoreCase), section);
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
        Assert.Matches(new Regex(@"reduced slowly", RegexOptions.IgnoreCase), section);
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
        foreach (var phrase in new[]
                 {
                     "routine operation", "routine procedure", "simple operation",
                     "minor operation", "straightforward operation",
                 })
        {
            Assert.DoesNotContain(phrase, Reader, StringComparison.OrdinalIgnoreCase);
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
            new Regex(@"[Nn]ot every hospital offers it", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"not a gentler version", RegexOptions.IgnoreCase), section);
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

        // Sentences that belong to the block, asserted absent from this page's
        // own source. If one appears here the reader meets it twice.
        foreach (var shared in new[]
                 {
                     "You are allowed to ask questions",
                     "Ask who your first call is",
                     "Get two numbers",
                     "Look after yourself",
                 })
        {
            Assert.DoesNotContain(shared, CuratedPage.Body(Page), StringComparison.OrdinalIgnoreCase);
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

        // The ambulance list has to carry a seizure: it is the commonest way a
        // brain tumor announces itself, and the caregiver block links to
        // /seizures/what-to-do for exactly this reader.
        Assert.Matches(
            new Regex(@"[Aa] seizure", RegexOptions.IgnoreCase), section[ambulance..]);

        // "If you are not sure, call" has to sit AFTER both lists. It is the
        // instruction that makes an incomplete list safe.
        var whenUnsure = section.IndexOf("If you are not sure which list this is",
            StringComparison.Ordinal);
        Assert.True(whenUnsure > ambulance, "the 'if you are not sure, call' line is missing or misplaced");
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
        Assert.Matches(
            new Regex(@"must not be stopped suddenly", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCaregiverSectionIsPlacedAfterSlotSevenAndBeforeTheRepeatSection()
    {
        // §12.7 fixes the position. Asserted because it is invisible in review:
        // the section reads fine anywhere, and the ordering is the part that
        // encodes "you have been given everything to act on before we ask you
        // to take on a job".
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
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

        var glossary = await client.GetStringAsync("/glossary");
        foreach (var slug in new[] { "bone-flap", "sma-syndrome", "laser-ablation", "levetiracetam", "5-ala", "craniectomy" })
        {
            Assert.Contains(slug, glossary, StringComparison.Ordinal);
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
