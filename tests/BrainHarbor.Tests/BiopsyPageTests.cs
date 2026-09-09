using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-519: T4, how tissue is taken. The eighth page under the §12.8 LIBRARY
/// template (the shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs), and the first page written after three §12.3
/// tumor hubs in a row.
///
/// The prose is reviewed, not tested. What is pinned here is the small set of
/// places where this page could leave a reader worse off than no page at all:
/// a needle biopsy read as an attempt to remove the tumor, an escalation list
/// that disagrees with the operation page a reader may be holding at the same
/// time, a same-day discharge read as a promise, and the wait restated here
/// badly instead of routed to the page that owns it.
/// </summary>
public sealed class BiopsyPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "biopsy.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    [Fact]
    public void TheOpeningSaysABiopsyIsNotATreatment()
    {
        // The expectation this page exists to correct. ACS: a biopsy may be a
        // procedure on its own OR part of surgery to remove the tumor, and the
        // first kind takes nothing out. A reader who books a day expecting the
        // tumor to be gone afterwards has been failed by the page, not by the
        // surgeon. §12.6 puts it in the first screen.
        var shortVersion = Section("The short version");

        Assert.Contains("It is not a treatment.", shortVersion);
    }

    [Fact]
    public void TheWhySectionSaysOutLoudThatANeedleBiopsyRemovesNothing()
    {
        // Said twice on purpose: once in the summary for the reader who stops
        // there, once where the decision is being explained. Pinned to the
        // section, not to the page, because presence somewhere is not the
        // property (WI-512) — a reader deciding between the two procedures has
        // to meet it here.
        var section = Section("Why am I having one?");

        Assert.Contains("not an attempt to remove the tumor", section);

        // And it ends on something to do rather than on the risk framing it
        // opens with (§12.6). Pinned to the LAST sentence, not a window.
        //
        // The first version of this used `sentences[^3..]`, and the break
        // harness walked straight through it: a mutation that moved the ask up
        // a paragraph and closed the section on "a needle biopsy carries a risk
        // of bleeding, and it may not give an answer at all" left the phrase
        // inside the window and the test green. That is §12.8's WI-510 lesson
        // verbatim — "a three-sentence window only proves the reassurance is
        // nearby" — re-committed by the item that had read it. The section was
        // reworded so the imperative genuinely is the last sentence.
        var sentences = CuratedPage.SentencesOf(section);
        Assert.StartsWith("Ask which of the two", sentences[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageSaysWhatAScanCannotDo()
    {
        // §12.8's first WI-506 rule. A reader who does not know that a scan
        // cannot name a tumor reads the whole tissue pathway as their team
        // being slow.
        var section = Section("What is a biopsy?");

        Assert.Contains("Only the tissue itself can do that.", section);
    }

    [Fact]
    public void TheEscalationTiersMatchTheOperationPageSymptomBySymptom()
    {
        // §12.12 / WI-512: nobody had contradicted a LIST; three pages had
        // sorted the same symptom into three different TIERS. The reader who
        // meets both this page and /treatments/craniotomy at once is the
        // ordinary reader, because an open biopsy IS a craniotomy.
        //
        // The sibling's lists are READ, never re-typed here (§12.10): a hard
        // coded copy of what craniotomy is believed to say has two copies to
        // keep in step, in the test whose subject is keeping two copies in
        // step.
        var craniotomy = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md")));

        var siblingAmbulance = Regex.Match(craniotomy,
            @"When to call an ambulance\.\*\*(.*?)(?=\*\*[A-Z]|\z)", RegexOptions.Singleline);
        Assert.True(siblingAmbulance.Success,
            "/treatments/craniotomy no longer has a 'When to call an ambulance' list, so the tiers "
            + "this page was checked against cannot be found");

        var siblingSameDay = Regex.Match(craniotomy,
            @"When to call the team the same day\.\*\*(.*?)(?=\*\*[A-Z]|\z)", RegexOptions.Singleline);
        Assert.True(siblingSameDay.Success,
            "/treatments/craniotomy no longer has a 'When to call the team the same day' list");

        var here = CuratedPage.Flatten(CuratedPage.ReaderText(Section("For the person going with them")));

        var ambulanceHere = Regex.Match(here,
            @"Call an ambulance\. These cannot wait:\*\*(.*?)(?=\*\*[A-Z]|If you are not sure|\z)",
            RegexOptions.Singleline);
        Assert.True(ambulanceHere.Success, "this page has no ambulance list");

        var sameDayHere = Regex.Match(here,
            @"Call the team the same day if:\*\*(.*?)(?=\*\*[A-Z]|\z)", RegexOptions.Singleline);
        Assert.True(sameDayHere.Success, "this page has no same-day list");

        // Each phrase has to be in the sibling's SAME tier and absent from the
        // other one. Absence from the other tier is the half that catches a
        // symptom quietly promoted or demoted on one page.
        //
        // `"that is their first"` is on this list because review proved its
        // absence: it mutated the ambulance bullet to a flat "A seizure" —
        // WI-510's blocker verbatim — and all eighteen tests stayed green,
        // including the one NAMED for that defect. §12.14's shape exactly: the
        // test read like the failure was handled.
        AssertTier(ambulanceHere.Groups[1].Value,
            siblingAmbulance.Groups[1].Value, siblingSameDay.Groups[1].Value,
            "not breathing properly", "cannot wake", "that is their first", "five minutes",
            "cannot speak, move, or see", "Chest pain");

        AssertTier(sameDayHere.Groups[1].Value,
            siblingSameDay.Groups[1].Value, siblingAmbulance.Groups[1].Value,
            "wound opens", "leaks from the wound", "fever", "stiff neck", "vision",
            "sleeping much more", "hit to the head", "pain medicine is not touching");

        static void AssertTier(string mine, string sameTier, string otherTier, params string[] phrases)
        {
            foreach (var phrase in phrases)
            {
                Assert.Contains(phrase, mine, StringComparison.OrdinalIgnoreCase);
                Assert.True(sameTier.Contains(phrase, StringComparison.OrdinalIgnoreCase),
                    $"'{phrase}' is in this tier here but not in the matching tier on /treatments/craniotomy");
                Assert.False(otherTier.Contains(phrase, StringComparison.OrdinalIgnoreCase),
                    $"'{phrase}' is sorted into a different tier on /treatments/craniotomy");
            }
        }
    }

    [Fact]
    public void NoSymptomOnTheOperationPagesListsIsQuietlyMissingFromThisPages()
    {
        // The tier test above is ONE-DIRECTIONAL, and review proved what that
        // costs: it can only see a symptom this page carries in the wrong tier,
        // never one the sibling carries that this page dropped. Three were
        // missing — chest pain, a fall or hit to the head, and pain the
        // medicine is not touching — and the more dangerous direction is
        // exactly the under-listing one (§12.8, WI-511).
        //
        // Compared as the sibling's own bullets, read out of the sibling, so a
        // reworded bullet there fails here rather than silently dropping out of
        // a hard-coded list (§12.10).
        var craniotomy = CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md"));

        // The page's OWN lists, unflattened: `CuratedPage.Section` collapses
        // whitespace, and a bullet count needs the line starts.
        var raw = CuratedPage.ReaderText(Page);

        foreach (var (heading, terminator) in new[]
                 {
                     ("When to call the team the same day", "**When to call an ambulance"),
                     ("When to call an ambulance", "If you are not sure"),
                 })
        {
            var start = craniotomy.IndexOf(heading, StringComparison.Ordinal);
            Assert.True(start > 0, $"/treatments/craniotomy no longer has a '{heading}' list");
            var end = craniotomy.IndexOf(terminator, start, StringComparison.Ordinal);
            Assert.True(end > start, $"could not find the end of craniotomy's '{heading}' list");

            var siblingBullets = Regex.Matches(craniotomy[start..end], @"^- ", RegexOptions.Multiline).Count;
            Assert.True(siblingBullets > 0, $"craniotomy's '{heading}' list has no bullets");

            var mineHeading = heading.Contains("ambulance", StringComparison.Ordinal)
                ? "Call an ambulance. These cannot wait:"
                : "Call the team the same day if:";
            var mineStart = raw.IndexOf(mineHeading, StringComparison.Ordinal);
            Assert.True(mineStart > 0, $"this page has no '{mineHeading}' list");
            // +2 to step over the heading's own closing `**`, or the slice ends
            // at the heading and counts zero bullets. And terminated on the
            // next SECTION as well as the next bold lead-in: the ambulance list
            // is the last bold thing on the page, so a `**`-only terminator ran
            // to end of file and counted the questions list as 23 symptoms.
            // Same trap §12.10 records for the craniotomy capture.
            var from = mineStart + mineHeading.Length + 2;
            var mineEnd = new[] { raw.IndexOf("**", from, StringComparison.Ordinal),
                                  raw.IndexOf("\n## ", from, StringComparison.Ordinal),
                                  raw.IndexOf("If you are not sure", from, StringComparison.Ordinal) }
                .Where(i => i > 0).DefaultIfEmpty(raw.Length).Min();
            var mineText = raw[mineStart..mineEnd];
            var mineBullets = Regex.Matches(mineText, @"^- ", RegexOptions.Multiline).Count;

            // COUNTED, not matched phrase by phrase, and that is the second
            // version of this check. The first compared each sibling bullet's
            // opening words and failed on its own correct page: craniotomy
            // writes "A headache that is new", this page writes "The headache
            // is new", and the same symptom in the same tier is not a defect.
            // A word-level comparison here would be a rewording detector.
            //
            // The limit, stated: equal counts can be satisfied by swapping one
            // symptom for another. That is covered by the per-phrase tier
            // assertions in the test above, which name eight of the same-day
            // list and six of the ambulance list. Together they catch what
            // review found — three symptoms on the sibling and silently absent
            // here, in the under-triage direction §12.8 (WI-511) calls the more
            // dangerous one. Alone, neither did.
            Assert.True(mineBullets == siblingBullets,
                $"craniotomy's '{heading}' list has {siblingBullets} symptoms and this page's has "
                + $"{mineBullets}. Under-listing is the dangerous direction: add the missing symptom "
                + "in the same tier, or record here why it does not apply to a needle biopsy.");
        }
    }

    [Fact]
    public void TheAmbulanceListDoesNotOverEscalateASeizureAgainstTheSeizurePage()
    {
        // WI-510's blocker was an ambulance list saying "a seizure"; WI-511's
        // was the same rule failing the other way. /seizures/what-to-do is the
        // site's authority and it is READ here rather than assumed. Markdown
        // emphasis is stripped first, or `**first ever** seizure` reads as
        // absent (§12.10).
        var seizurePage = Regex.Replace(
            CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("seizures", "what-to-do.md"))),
            @"[*_]", "");
        Assert.True(
            Regex.IsMatch(seizurePage, @"first[- ](ever[- ])?seizure", RegexOptions.IgnoreCase),
            "/seizures/what-to-do no longer singles out a first seizure, so this page's ambulance "
            + "tier was built on something that has changed");

        // The COMPOSED section, because the link to the seizure page now comes
        // from [CAREGIVER] rather than being repeated in the bullet.
        var section = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.ComposedSection(Page, "For the person going with them")));

        // The qualifier is the whole point: the list names WHICH seizures, and
        // then says out loud that not every seizure is one of them.
        //
        // The bullet ITSELF is pinned in the tier test above, and it is pinned
        // there because of what review proved about this one: asserting the
        // parenthetical and the link, without ever reading the bullet, let a
        // flat "A seizure" through — WI-510's blocker, under a test named for
        // it. Both halves are now checked, in the two places they belong.
        Assert.Contains("Not every seizure needs an ambulance", section, StringComparison.Ordinal);
        Assert.Contains("/seizures/what-to-do", section, StringComparison.Ordinal);
    }

    [Fact]
    public void GoingHomeTheSameDayIsDescribedAsTheHospitalsSetupAndNotAsAPromise()
    {
        // The outpatient review this rests on is ARGUING FOR same-day
        // discharge, and §12.8 (WI-512) records the shape of citing a source
        // for the position it takes rather than for what it establishes. What
        // it establishes is that both happen and that the difference between
        // centers is "attributable primarily to institutional infrastructure
        // ... rather than procedural risk". A reader kept in overnight must not
        // read this page and conclude something went wrong.
        var section = Section("How long does it take?");

        Assert.Contains("Some stay the night", section, StringComparison.Ordinal);
        Assert.Contains("how your hospital is set up", section, StringComparison.Ordinal);

        // The selection-criteria half is load-bearing and was missing from the
        // first draft, which said only that it was the hospital's setup and
        // told the reader not to read anything into it. The paper says
        // "institutional infrastructure, SELECTION CRITERIA, and cultural
        // adoption", and its published criteria exclude patients for
        // uncontrolled seizures, instability, and having nobody at home. So a
        // reader kept in overnight sometimes IS being kept in because of them,
        // and a page that forbids them to read anything into it is reassuring
        // past what the source supports (§12.12).
        Assert.Contains("whether you fit the checklist they use", section, StringComparison.Ordinal);
        Assert.DoesNotContain("do not read anything into it", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRiskSectionNamesBleedingWithoutANumberAndEndsOnWhatIsDoneAboutIt()
    {
        // The first draft of this page had NO risk section at all, and the
        // human-style read is what found it: the page told the person driving
        // the reader home to call an ambulance if they suddenly could not
        // speak, and never said why that might happen. §12.8's WI-506 rule is
        // to answer the frightening question in both directions; a page that
        // answers neither direction is worse.
        //
        // R2 governs the shape: qualitative, and the reason is specific and
        // publishable here. The reported rates for bleeding after a biopsy run
        // from 2.3% to 72%, and the source says outright that the spread is
        // driven by what each study COUNTED as a hemorrhage.
        var section = Section("What can go wrong?");
        var sentences = CuratedPage.SentencesOf(section);

        Assert.Contains("Bleeding is the main one", section, StringComparison.Ordinal);
        Assert.Contains("what each study counted as bleeding", section, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", section);

        // §12.6: never end a section on a frightening sentence. Pinned to the
        // LAST sentence rather than a window, for the reason the break harness
        // proved on this page's own why-section: a window is satisfied by a
        // frightening sentence appended after the reassurance.
        Assert.EndsWith("worth reading with whoever is taking you home.", sentences[^1],
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheBleedingRiskIsAttributedToTheFactorsThePaperActuallyFound()
    {
        // Review caught this one, and it was the wrong cause propping up a
        // reassurance. The draft said "where the tumor sits is what decides
        // most of that, and it is part of why your team chose this route" —
        // imported from /treatments/craniotomy, where location IS the story.
        //
        // The paper this page cites concludes the opposite: "the size of the
        // lesion is the essential factor determining diagnostic yield and
        // postoperative intracerebral hematoma complication", and its
        // multivariable analysis leaves only lesion diameter and intraoperative
        // bleeding standing. Location appears once, as one of six factors
        // reported in OTHER people's literature. §12.14: check the source.
        var section = Section("What can go wrong?");

        Assert.Contains("a small target", section, StringComparison.Ordinal);
        Assert.Contains("bleeding during the procedure itself", section, StringComparison.Ordinal);
        Assert.DoesNotContain("Where the tumor sits", section, StringComparison.Ordinal);

        // And the closed enumeration is open, because the worst outcome is not
        // on the list and the consent form names it (§12.8, WI-506: both
        // directions).
        Assert.Contains("The other risks include", section, StringComparison.Ordinal);
        Assert.Contains("consent form", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDrivingRuleIsNotNarrowedToTheDriveHomeAndReachesTheCaregiver()
    {
        // The source says "You must not drive after having a brain biopsy. Your
        // neurosurgeon will let you know how long you must not drive for."
        // The draft narrowed it to "you must not drive YOURSELF", sitting under
        // "someone to get you home", which reads as a rule about today. That is
        // §12.12's over-reassuring direction on a page that also lists seizure
        // as a risk, and it never reached the person who could stop them:
        // /treatments/craniotomy puts driving in the CAREGIVER section for
        // exactly that reason.
        //
        // No duration anywhere, per §12.3: driving rules are jurisdictional.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        Assert.Contains("You must not drive after a brain biopsy", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("drive yourself after", reader, StringComparison.Ordinal);

        var caregiver = CuratedPage.Flatten(Section("For the person going with them"));
        Assert.Contains("No driving.", caregiver, StringComparison.Ordinal);

        // The overnight adult is a CONDITION of going home the same day in both
        // sources ("the availability of a reliable adult caregiver for
        // assistance in the first 24 h"; "an adult needs to accompany you and
        // stay with you for the next 24 hours"), so the page states it rather
        // than telling the reader to ask whether it applies.
        Assert.Contains("an adult stays with you for the first day and night", reader,
            StringComparison.Ordinal);
        Assert.Contains("an adult stays with them for the first day and night", caregiver,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheRepeatSectionOpensByRemovingBlameAndClosesOnAQuestionToAsk()
    {
        // §12.8 slot 8: a frightening moment with a mundane answer. §12.6:
        // never end a section on a frightening sentence. Both ends pinned,
        // because a positional rule is not proved by a phrase being somewhere
        // in the middle (WI-510).
        var section = Section("Why am I having another one?");
        var sentences = CuratedPage.SentencesOf(section);

        Assert.Contains("not a sign that anything was done badly", sentences[0], StringComparison.Ordinal);

        // Last sentence, not a window (see the why-section test above).
        Assert.Contains("what happens to my treatment", sentences[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void TheWaitIsRoutedToThePageThatOwnsItRatherThanRestatedHere()
    {
        // WI-507 owns the wait, and the two pages are a deliberate sequence.
        // The failure mode is this page growing its own half-version of the lab
        // steps, which then drifts. Named vocabulary from the sibling's
        // step-by-step list, asserted absent here.
        var body = CuratedPage.ReaderText(Page);

        Assert.Contains("/tests/waiting-for-results", body, StringComparison.Ordinal);

        // The vocabulary of the lab pipeline itself. Review proved the first
        // version of this list was too narrow: it held five words, and a
        // genuine restatement written around them ("the tissue is set hard, cut
        // into slices thinner than a hair, stained, and read under a
        // microscope, and each of those steps takes its own day") went straight
        // through. The words below are the ones any description of the
        // processing has to reach for.
        //
        // The limit, stated rather than pretended away: this is a vocabulary
        // filter, not an ownership proof. Nothing can mechanically decide
        // whether a page has restated another page's subject. What it can do is
        // make the restatement expensive to write, and the break harness pins
        // it against the exact sentence review used.
        //
        // Deliberately NOT auto-derived from the sibling's numbered list: that
        // list's content words include tissue, needle, piece, surgeon, gene,
        // tests, report and tumor, every one of which this page needs.
        foreach (var owned in new[]
                 {
                     "formalin", "fixative", "methylation", "wax", "paraffin",
                     "slice", "slide", "stain", "microscope",
                     "addendum", "second opinion", "integrated diagnosis",
                 })
        {
            Assert.DoesNotContain(owned, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheDeadAndMisreadCitationsAreAbsentAndSoAreTheClaimsTheyCarried()
    {
        // §12.14, and this is the rule the item was told to carry: banning a
        // URL is not fixing the claim it was carrying.
        //
        // tests-library.md §4.1 sources "a meta-analysis found no significant
        // difference in diagnostic yield between the two systems" to
        // surgicalneurologyint.com, which returns a 24-word JavaScript shell,
        // and to PMC10219353 — which is not a meta-analysis at all but a
        // 72-patient single-centre comparison (42 frameless, 30 frame-based).
        // So the URL is banned AND the word is: the page may not describe any
        // of its sources as a meta-analysis, because none of them is one.
        var page = Page;

        // Scoped to the `- url:` lines rather than the whole file, because the
        // front matter NAMES the dead domain in a comment saying why it is not
        // cited. Banning the string outright would forbid writing that note
        // down, which is the one thing that stops the next session re-adding
        // the citation from the dossier.
        var citedUrls = Regex.Matches(CuratedPage.FrontMatter(page), @"^[ \t]*- url: (\S+)",
            RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToList();
        Assert.NotEmpty(citedUrls);
        Assert.DoesNotContain(citedUrls,
            u => u.Contains("surgicalneurologyint", StringComparison.OrdinalIgnoreCase));

        var body = CuratedPage.ReaderText(page);
        Assert.DoesNotContain("meta-analysis", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("meta analysis", body, StringComparison.OrdinalIgnoreCase);

        // §4.3 bundles "a small shaved patch" into a bullet cited to ACS, which
        // contains the string "shav" zero times (the WI-512 bundled-claim
        // shape). Every mention of shaving on this page has to be a QUESTION,
        // not an assertion about what will happen.
        foreach (var sentence in CuratedPage.SentencesOf(CuratedPage.Flatten(CuratedPage.ReaderText(page)))
                     .Where(s => s.Contains("shav", StringComparison.OrdinalIgnoreCase)))
        {
            Assert.True(
                sentence.Contains("ask", StringComparison.OrdinalIgnoreCase) || sentence.Contains('?'),
                $"this page asserts something about shaving that no source says: \"{sentence}\"");
        }
    }

    [Fact]
    public void TheFrameIsNotDescribedInSensoryDetailNoSourceSupplies()
    {
        // The dossier's §4.3 describes the frame going on as "pressure and a
        // sharp sting", with no citation, and the only fetchable sources for
        // that sensation are Gamma Knife radiosurgery pages — the right
        // sensation from the wrong procedure, which is the WI-515 defect.
        // §12.6 wants sensory description; it does not license inventing one.
        // The page says what the frame IS and hands the rest to the team.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var invented in new[] { "sharp sting", "pins", "screwed", "bolted" })
        {
            Assert.DoesNotContain(invented, body, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("ask them to talk you through how it goes on", body, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoPercentageAndNoPrognosisVocabulary()
    {
        // Contract item 5 and §12.4 R2, which is the ruling that matters most
        // here: this page's sources are wall-to-wall procedural risk
        // percentages, and the spread between them is enormous (post-operative
        // hemorrhage is reported from 2.3% to 72% depending only on how each
        // study DEFINED it). None of them belongs on the page.
        //
        // Graded against reader text, not raw source: the WI-105 suppression
        // markers put characters into the file that no reader ever sees
        // (WI-509).
        var body = CuratedPage.ReaderText(Page);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", body);

        foreach (var word in new[]
                 {
                     "survival", "life expectancy", "five-year", "prognosis", "how long you have",
                     // R2's own vocabulary: the numbers written as words are
                     // still numbers (WI-511). "Diagnostic yield" is the
                     // sources' phrase for the same thing.
                     "diagnostic yield", "per cent",
                 })
        {
            Assert.DoesNotContain(word, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNeitherTheCharacterisationsNorTheMinimisations()
    {
        // A procedure page is where "it is a simple procedure" arrives, and it
        // is exactly the sentence a frightened reader remembers when it turns
        // out not to have been. Negation-aware, so "there is no such thing as a
        // simple procedure" still passes (§12.8, WI-511).
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "tests/biopsy");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // Sharper here than on any page so far: this is the first page in the
        // corpus to cite Cancer Research UK, so a British phrasing has an open
        // door straight from the source into the draft. Whitespace-normalised
        // first, because the corpus is hard-wrapped and the gate's own history
        // is of walking past "a\nlift" (§12.8, WI-511).
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8's warning, and the reason this test exists rather than a
        // heading count: three bent pages in a row is how a template quietly
        // shrinks, and the three pages before this one were §12.3 HUBS, which
        // is a different template with seventeen sections. Read §12.8 for the
        // slot list, not the last page written.
        //
        // This page carries all twelve, plus the caregiver section §12.8 gives
        // to a test with a real aftercare tail: someone goes home the same day
        // with a hole in their skull and a person who has to watch them.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Equal(
            [
                "The short version",                        // 0
                "What is a biopsy?",                        // 1
                "Why am I having one?",                     // 2
                "What happens, step by step",               // 3
                "How long does it take?",                   // 4
                "What does it feel like?",                  // 5
                "What can go wrong?",                       // 6a, the risks
                "The hard parts, and what helps",           // 6a, and what to do
                "Is there a way to find out without one?",  // 6c
                "What you need first, and what to bring",   // 7
                "For the person going with them",           // caregiver, §12.7
                "Why am I having another one?",             // 8
                "Who reads it, and how do I get the result?", // 9
                "What to ask your team",                    // 10
                "Where to go next",                         // 11
            ],
            headings);
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheSharedBlockRatherThanRetypingIt()
    {
        // §12.7. The per-page half sits AFTER the directive and covers only
        // what is true for this one day; the shared half is never reworded on
        // a page (§12.10).
        var section = Section("For the person going with them");

        Assert.StartsWith("[CAREGIVER]", section, StringComparison.Ordinal);

        // Word SHINGLES, not the block's bold lead-ins. §12.8 records the
        // lead-in check as insufficient in as many words ("WI-510 asserted the
        // block's four bold lead-ins were absent and shipped four genuine
        // duplications that were none of them"), and this page shipped the
        // weaker version anyway until review pasted three verbatim block
        // sentences into the page's own caregiver half and watched eighteen
        // tests stay green. Copied from the shape already in
        // TreatmentLibraryPagesTests rather than re-invented.
        //
        // "Where to go next" is excluded: it is an index of onward doors by
        // design, so a link the block also makes is expected to reappear there.
        var body = CuratedPage.ReaderText(Page).Replace("[CAREGIVER]", " ", StringComparison.Ordinal);
        var next = body.IndexOf("## Where to go next", StringComparison.Ordinal);
        var pageText = next > 0 ? body[..next] : body;

        static IEnumerable<string> Shingles(string text, int n)
        {
            var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+")
                .Select(m => m.Value).ToList();
            for (var i = 0; i + n <= words.Count; i++)
            {
                yield return string.Join(' ', words.Skip(i).Take(n));
            }
        }

        var pageShingles = Shingles(pageText, 8).ToHashSet();
        var blockPath = Path.Combine(CuratedPage.BlocksRoot, "caregiver.md");
        var overlaps = Shingles(CuratedPage.Body(File.ReadAllText(blockPath)), 8)
            .Where(pageShingles.Contains)
            .Distinct()
            .ToList();

        Assert.True(overlaps.Count == 0,
            "this page restates the shared [CAREGIVER] block, so the reader meets it twice:\n  "
            + string.Join("\n  ", overlaps));

        // A duplicated LINK is not a duplicated sentence, so it needs its own
        // check. The block already sends the reader to both seizure pages, and
        // an ambulance list written on a page about a procedure reaches for
        // /seizures/what-to-do naturally — which is what this page did, six
        // lines below the block that had just linked it.
        foreach (var link in new[] { "/seizures/what-to-do", "/seizures/living-with" })
        {
            if (!CuratedPage.Body(File.ReadAllText(blockPath)).Contains(link, StringComparison.Ordinal))
            {
                continue;
            }

            Assert.DoesNotContain(link, section.Replace("[CAREGIVER]", " ", StringComparison.Ordinal),
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone, and there is no outlook here.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8. ContentCheck only warns on a missing `accessed`
        // date, and this phase's whole source discipline rests on it.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The domains this page's claims actually rest on. ACS governs the
        // procedure description; the PMC papers carry the frameless comparison,
        // the sufficiency of a needle sample for the gene tests, and the
        // same-day discharge; CRUK carries how it feels afterwards and nothing
        // else (see the front matter's own note).
        foreach (var domain in new[] { "cancer.org", "ncbi.nlm.nih.gov", "cancerresearchuk.org" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // §12.1: NCI patient PDQ is fine for framing and is never the source
        // for naming or grading. The dossier's §4.1 cites it twice for the
        // procedure description, which ACS carries at the same patient level
        // without the pre-CNS5 vocabulary, so it is simply not needed here.
        Assert.DoesNotContain("cancer.gov", front, StringComparison.OrdinalIgnoreCase);

        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor: .NET's multiline `$` does not match before `\r`, and
        // this repo has core.autocrlf=true (WI-501).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }
}

/// <summary>The biopsy page as served, and the doors that lead to it.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class BiopsyPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/biopsy";

    private readonly WebApplicationFactory<Program> _factory;

    public BiopsyPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Biopsy: how tissue is taken", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // The WI-412 orphan lesson. A new library page nothing links to is half
        // shipped, and /tests has no index to catch it. These are the pages a
        // reader is standing on when they learn tissue is needed: the scan that
        // could not name it, the operation, the wait that follows, and every
        // hub whose "how do doctors find out" section says "needle biopsy".
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/start", "/tests/mri", "/tests/waiting-for-results", "/treatments/craniotomy",
                     "/tumors/glioma", "/tumors/low-grade-glioma", "/tumors/high-grade-glioma",
                     "/tumors/astrocytoma", "/tumors/oligodendroglioma", "/tumors/glioblastoma",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors this page owes its reader: on to the wait (WI-519 -> WI-507
        // -> WI-508 is a deliberate sequence), across to the operation for the
        // reader whose tissue comes out that way, and out to a person.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/tests/waiting-for-results", "/treatments/craniotomy", "/get-help-now");

    [Fact]
    public async Task TheDeepLinkIntoTheScanPageLandsOnARealAnchor() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page with no
        // sign anything went wrong. This page deep-links
        // /tests/mri#navigation-scan, an anchor written explicitly on that page
        // for this purpose rather than derived from its heading text.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheProcedureVocabularyFiresAsTooltipsOnTheRealPage()
    {
        // Contract item 9, asserted on the rendered page: a term nothing
        // matches is a term nobody ever sees (WI-512).
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "neuropathologist", "craniotomy" })
        {
            Assert.Contains($"def-{slug}", html);
        }
    }

    [Fact]
    public async Task TheTermsThisPageDefinesAreSuppressedHereAndStillFireElsewhere()
    {
        // §12.8 (WI-509): a popover repeating the paragraph directly beneath it
        // is noise. WI-510's rule is that a "both directions" assertion is only
        // worth writing if both directions are observable — and here they are,
        // which is why this test exists at all:
        //
        //   stereotactic biopsy  suppressed here, fires on /tumors/astrocytoma
        //                        through its `needle biopsy` alias
        //   neuronavigation      suppressed here, fires on /tests/mri through
        //                        its `navigation scan` alias
        //
        // WI-512's other half first: assert the WORD is in this page's prose
        // before asserting its tooltip is not, or the suppression is a no-op
        // and the DoesNotContain passes for the wrong reason.
        var body = CuratedPage.ReaderText(CuratedPage.Read("tests", "biopsy.md"));
        Assert.Contains("stereotactic biopsy", body, StringComparison.Ordinal);
        Assert.Contains("neuronavigation", body, StringComparison.Ordinal);
        Assert.Contains("frozen section", body, StringComparison.Ordinal);

        var client = _factory.CreateClient();
        var html = await client.GetStringAsync(Url);

        Assert.DoesNotContain("def-stereotactic-biopsy", html);
        Assert.DoesNotContain("def-neuronavigation", html);
        Assert.DoesNotContain("def-frozen-section", html);

        Assert.Contains("def-stereotactic-biopsy", await client.GetStringAsync("/tumors/astrocytoma"));
        Assert.Contains("def-neuronavigation", await client.GetStringAsync("/tests/mri"));
        Assert.Contains("def-frozen-section", await client.GetStringAsync("/tests/waiting-for-results"));
    }
}
