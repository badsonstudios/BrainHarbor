using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-538: <c>/tumors/pediatric-brain-tumor</c>, the stub rewritten for a PARENT.
///
/// **This page is a CROSS-CUTTING page, not a tumor hub (§12.11).** It is an age
/// axis, so the property that matters most is that it carries its own ANGLE and
/// ROUTES everything a named hub already owns. WI-529's first lesson is the one
/// this item kept hitting: the source pack's headline fact for this page — that
/// "pediatric-type" describes biology and not the reader's age — is already
/// shipped on <c>/tumors/glioma</c>, so the page links there instead of saying it
/// again. The same went for the radiation mask, the post-operative drain, proton
/// cost and travel, and when to stop eating before surgery.
///
/// **The safety decisions.** (1) <c>[MECHANISM]</c> and <c>[ESCALATION]</c> are
/// included together, with a scoping note ABOVE the first block, because both are
/// written to the patient and here the reader is the patient's parent (WI-529's
/// shape, in a new direction). (2) The shared escalation tiers are adult-shaped:
/// nothing in them covers a head growing too fast, a bulging soft spot, or a baby
/// who is only irritable, so the page adds its own tier BENEATH the block, which
/// is what §12.10 asks for when a hub's emergency is not in the list. (3) Every
/// same-day line the page writes itself carries the shunt clause. (4)
/// <c>[SPINAL-CORD]</c> IS included, and this page is its fourth includer. The first
/// draft excluded it on a reason that was false about the BLOCK rather than about the
/// page — it opens with a conditional, which §12.10 says survives the "hub you have
/// thought about least" test — and <c>/review</c> round 1 reversed that. See
/// TheSpinalCordBlockIsIncludedForTheReaderWhoseTumorReachesTheCord.
///
/// **The honesty decisions.** No prognosis figures and no survival figures, on a
/// page whose sources are full of them. No anesthesia threshold age, because the
/// sources disagree and one paper disagrees with itself on one page. NOTHING is
/// TAKEN from Cancer Research UK, despite it writing the best children's pages in
/// the set, because its idiom is in the exact sentences this page wants — but the
/// reader still MEETS two CRUK entries, which arrive with <c>blocks/escalation.md</c>
/// and render in the source list. "Not cited at all" was the false form of this
/// justification, removed from the page's front matter at round 1 and left standing
/// here until round 3.
/// </summary>
public sealed class PediatricBrainTumorPageContentTests
{
    private const string Slug = "tumors/pediatric-brain-tumor";

    private static string Page => CuratedPage.Read("tumors", "pediatric-brain-tumor.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string RawLf => Page.Replace("\r\n", "\n");

    private const string ShortHeading = "The short version";
    private const string MeansHeading = "What \"pediatric brain tumor\" means";
    private const string NotAdultsHeading = "Why children are not small adults";
    private const string LocationHeading = "Where it grows, and why that causes symptoms";
    private const string SymptomHeading = "What symptoms look like in a child";
    private const string FindHeading = "How doctors find out";
    private const string ReportHeading = "What the words on the report mean";
    private const string TreatmentHeading = "How treatment is different for a child";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string SchoolHeading = "Everyday life: school, and the years after";
    private const string ScansHeading = "Follow-up scans, and the check-ups that come with them";
    private const string ThinkingHeading = "Thinking, learning and memory";
    private const string TalkingHeading = "Talking to your child";
    private const string SiblingsHeading = "Brothers and sisters";
    private const string TeamHeading = "Who is on your child's team";
    private const string PalliativeHeading = "Palliative care, which is not what it sounds like";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CaregiverHeading = "For the person caring for someone with this";

    /// <summary>Reader text with heading lines removed, so a heading never merges into a sentence (WI-535).</summary>
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

    private static string Sibling(params string[] path) =>
        CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(path)));

    [Fact]
    public void TheSectionsRunInTheOrderThePagePromises()
    {
        var order = new[]
        {
            ShortHeading, MeansHeading, NotAdultsHeading, LocationHeading, SymptomHeading,
            FindHeading, ReportHeading, TreatmentHeading, AfterHeading, SchoolHeading,
            ScansHeading, ThinkingHeading, TalkingHeading, SiblingsHeading, TeamHeading,
            PalliativeHeading, CauseHeading, OutlookHeading, CaregiverHeading,
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

        // §12.2 item 7 and §12.3: the questions list and the caregiver section are
        // contract items, and the caregiver section comes BEFORE the questions
        // (CaregiverSectionTests enforces that across the directory; asserted here
        // too so this page fails in its own file rather than in a shared one).
        Assert.True(
            Page.IndexOf($"\n## {CaregiverHeading}", StringComparison.Ordinal)
            < Page.IndexOf("\n## What to ask your team", StringComparison.Ordinal));
    }

    [Fact]
    public void TheSharedBlocksThisPageOwesAreIncludedInTheRightSections()
    {
        Assert.Contains("[MECHANISM]", Section(LocationHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CaregiverHeading), StringComparison.Ordinal);
        Assert.Contains("[POSTERIOR-FOSSA-SYNDROME]", Section(AfterHeading), StringComparison.Ordinal);

        // AND NO OTHERS. A directive nobody names is a contract item nobody tests,
        // which is how [MECHANISM] and [CROSSWALK] composed silently into
        // /tumors/meningioma (§12.8, WI-527). The set is closed, so adding one
        // forces a test.
        string[] named =
        [
            "[MECHANISM]", "[ESCALATION]", "[SPINAL-CORD]", "[CROSSWALK]", "[CAUSES]",
            "[CAREGIVER]", "[POSTERIOR-FOSSA-SYNDROME]",
        ];
        var present = Regex.Matches(CuratedPage.ReaderText(Page), @"(?m)^\[([A-Z][A-Z-]+)\]\s*$")
            .Select(m => m.Value.Trim())
            .ToHashSet(StringComparer.Ordinal);
        Assert.Empty(present.Except(named));
    }

    [Fact]
    public void TheSpinalCordBlockIsIncludedForTheReaderWhoseTumorReachesTheCord()
    {
        // /review round 1 REVERSED this call, and the reason the first draft gave was
        // wrong about the block rather than merely debatable. It claimed the block
        // "asserts a cord rule outright"; it opens with a CONDITIONAL ("If the tumor is
        // in the spinal cord, or has spread to the spine"), and §12.10 says a
        // conditional survives the "hub you have thought about least" test because a
        // reader it does not name is not addressed by it. That is the same argument
        // this page uses to include [POSTERIOR-FOSSA-SYNDROME].
        //
        // The exclusion's cost was the dangerous direction: this page names two tumors
        // that reach the spine, its reader is often pre-diagnosis, and without the block
        // new leg weakness or new bladder trouble sat at the shared block's SAME-DAY
        // tier when the corpus files that reader RIGHT AWAY.
        Assert.Contains("[SPINAL-CORD]", Section(SymptomHeading), StringComparison.Ordinal);

        // THE SCOPING IS IN TWO PLACES, and /review round 2 corrected my reasoning here.
        // I had recorded that the lead-in "cannot precede" the block because
        // SpinalCordBlockTests fixes its position after [ESCALATION]. That guard forbids
        // anything BETWEEN the two directives; it does not forbid a forward-pointing
        // sentence earlier in the section, and BOTH sibling hubs carry exactly that
        // ("those signs have their own rule, at the end of the section on when to call
        // for help, below"). So a parent now learns the rule is conditional BEFORE
        // meeting "For you, an arm or a leg that is newly weak", and the paragraph after
        // the block confirms it.
        var section = CuratedPage.Flatten(Section(SymptomHeading));

        // Worded so it does not echo the block it points at: the first version opened
        // "is in the spinal cord, or has", which is the block's own opening clause, and
        // the corpus restatement check caught it against blocks/spinal-cord.md itself.
        //
        // /review round 3 then found TWO things wrong with it. It said the rule came
        // "at the end of this section", which is false here: three paragraphs follow the
        // block (the scoping confirmation, the baby tier, the pre-verbal tier), so a
        // parent taking it literally lands on the baby paragraph. The sibling hubs'
        // version of that phrase is TRUE on their pages, because the block IS last
        // there -- matching their shape imported a claim that does not hold here, the
        // same trap as matching the block's anatomy and importing its sentence. And it
        // named only ONE of the block's two readers ("has reached the spine"), so a
        // parent whose child's tumor IS in the cord could read the pointer as not about
        // them before ever meeting the block. It now names both and says "just after
        // the lists below".
        var pointer = section.IndexOf("Some of the signs below carry a different rule",
            StringComparison.Ordinal);
        var directive = section.IndexOf("[SPINAL-CORD]", StringComparison.Ordinal);
        Assert.True(pointer >= 0, "the forward pointer to the cord rule is gone");
        Assert.True(directive > pointer, "the forward pointer must come BEFORE the cord block");

        // AND IT MUST NOT CLAIM THE RULE IS AT THE END OF THE SECTION (/review round 3).
        // It is not: the baby tier and the pre-verbal tier both come after the block, so
        // that phrasing sends a parent past the rule it is pointing at. It is banned
        // rather than merely corrected, because the sibling hubs DO say it and copying
        // them is how it arrived the first time.
        Assert.DoesNotContain("at the end of this section", section, StringComparison.Ordinal);
        Assert.Contains("It comes just after the lists", section, StringComparison.Ordinal);

        // AND IT POINTS FORWARD, NOT BACK (/review round 4). It said "some of THESE
        // signs", whose nearest antecedent is the older-child list ABOVE it — but the
        // sign the cord block re-tiers ("New weakness in the face, an arm or a leg")
        // is in [ESCALATION]'s list BELOW, and the block's other signs (numbness, back
        // or neck pain, bladder or bowel trouble) are in neither list on this page.
        // A triage pointer that points at the wrong list is worse than none.
        Assert.Contains("Some of the signs below", section, StringComparison.Ordinal);
        Assert.DoesNotContain("Some of these signs", section, StringComparison.Ordinal);
        // Worded clear of /tumors/ependymoma, which the corpus restatement check caught
        // the first version colliding with on four shingles.
        // Rendered read 3: the lead-in has to say the rule may NOT apply, not merely who
        // it is for. Composed, a parent meets the cord block BEFORE this sentence (the
        // block's position after [ESCALATION] is fixed by SpinalCordBlockTests), so the
        // scoping arrives late by construction and has to do more work when it lands.
        // "if the tumor is in YOUR CHILD'S spinal cord" rather than "a child whose tumor
        // is in the spinal cord": the latter reproduced the block's own opening clause
        // word for word and collided with both blocks/spinal-cord.md and
        // /tumors/ependymoma on the corpus check. It arrived via a round-2 NIT asking the
        // lead-in's anatomy to match the block -- aligning the wording imported the
        // clause. Matching a block's anatomy is not the same as matching its sentence.
        Assert.Contains(
            "That last rule only applies if the tumor is in your child's spinal cord, or has reached the spine",
            section, StringComparison.Ordinal);

        // NOT a frequency claim. /review round 2 blocker: the first version said "Most
        // childhood brain tumors never do", which no source supports and which ACS
        // contradicts in direction for the tumors this page teaches -- embryonal tumors
        // "tend to grow quickly and often spread through the CSF", and this page teaches
        // "embryonal tumor" as the childhood word two sections earlier. A reassurance on
        // the sentence that decides whether a right-away rule is yours is §12.12's
        // dangerous direction.
        Assert.Contains("Some childhood tumors can reach it and some cannot",
            section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\b(most|rarely|seldom|never|few)\b[^.]{0,60}\bspine\b",
            RegexOptions.IgnoreCase), section);

        // And the page does not RETYPE the block (§12.10: assert something only the
        // block says, both directions).
        var mine = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        Assert.DoesNotContain("right-away call, not a same-day one", mine, StringComparison.Ordinal);
        Assert.Contains("right-away call, not a same-day one",
            CuratedPage.Flatten(CuratedPage.Composed(Page, Slug)), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSiblingPageThatGlossesTheNewGlossaryTermInlineAlsoSuppressesIt()
    {
        // WI-538 added glossary/embryonal-tumor.md at its SECOND use, so both pages that
        // gloss the term inline must suppress it or the tooltip prints the definition
        // directly above the definition (§12.8, WI-535).
        //
        // The marker on the sibling is deliberately NOT beside the term: a marker inside
        // that page's "What is a medulloblastoma?" section breaks its own
        // TheRetiredNameAppearsOnlyAsRetiredAndTheTrialNamesAreNotDiagnoses, which builds
        // a string from the RAW section and subtracts it from ReaderText (which strips
        // markers). /review round 1 suggested moving it; rejected, and this item's
        // harness carries a mutation that puts it back to prove the failure is real.
        // SCOPED TO THE BODY, AND THE HARNESS IS WHY. This read the RAW file, so the
        // sibling's front-matter comment EXPLAINING the marker satisfied the assertion
        // by itself: delete the real marker, keep the sentence describing it, and the
        // guard still passed. WI-537 recorded a URL in a comment making a check pass for
        // the wrong reason; this is the same surface, on a page's own authoring marker.
        //
        // Body, NOT ReaderText: ReaderText STRIPS `!%...%`, so asserting on it would be
        // vacuous in a new way -- the helper removes the very thing under test.
        var sibling = CuratedPage.Body(CuratedPage.Read("tumors", "medulloblastoma.md"));
        Assert.Contains("!%embryonal tumor%", sibling, StringComparison.Ordinal);
        Assert.Contains("It is an **embryonal tumor**.", sibling, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSpinalCordBlockIsNotEdited()
    {
        // §12.10, WI-528: the block is included UNEDITED, and this pins that. Editing it
        // to fit this page would push the change onto the three tumor hubs that also
        // include it, which is the WI-514 blast radius.
        //
        // An earlier draft of this method asserted the block's ABSENCE, on a reason that
        // was wrong about the block rather than merely arguable: it opens with a
        // conditional. /review round 1 reversed that call, and the inclusion is pinned by
        // the test above, so what is left here is the "not edited" half.
        var block = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "spinal-cord.md"))));
        Assert.Contains("If the tumor is in the spinal cord, or has spread to the spine",
            block, StringComparison.Ordinal);

        // The hubs that also carry it stay reachable from here, so a parent can get to
        // the page for their child's own tumor.
        var body = CuratedPage.ReaderText(Page);
        Assert.Contains("/tumors/medulloblastoma", body, StringComparison.Ordinal);
        Assert.Contains("/tumors/ependymoma", body, StringComparison.Ordinal);
    }

    [Fact]
    public void TheScopingNoteSitsAboveTheFirstSharedBlockAndCoversBothItsFramings()
    {
        // WI-529's shape, in the mirror direction. There, the page had no diagnosis
        // and the blocks assumed one. Here the blocks are written TO THE PATIENT and
        // this page's reader is the patient's parent: [MECHANISM] says "your tumor"
        // and opens by saying most people are told what they have before anyone
        // explains what it is doing.
        //
        // Flattened, which is load-bearing rather than tidy: the corpus is
        // hard-wrapped, so the phrases below have newlines inside them and a raw
        // match walks straight past.
        var section = CuratedPage.Flatten(SectionRawLf(LocationHeading));

        // Rendered read 3 found the voice reminder scoped too narrowly: it said "the list
        // below", and composed, the reader passes [ESCALATION]'s two tiers, both fever
        // rules, the shunt rule, two sign-off links AND the cord block before the patient
        // voice stops. Round 2 then narrowed the claim again, because the last four
        // paragraphs are the page's own and already in parent voice. The current wording
        // is "The two lists that follow ... After them, this section goes back to
        // speaking to you", asserted in
        // TheEscalationVoiceReminderCoversEveryPatientVoicedBlockInTheSection below.
        var note = Regex.Match(section,
            @"written for an adult patient.*?"
            + @"Read those words as ""your child"".*?"
            + @"most people are told what they have before anyone explains",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        Assert.True(note.Success,
            "the section that includes [MECHANISM] no longer scopes BOTH the block's "
            + "\"your tumor\" wording and its opening framing, so a block written to a "
            + "patient asserts itself of a parent");

        // POSITION is the property (WI-512): a note underneath the block it scopes has
        // already let the reader past it.
        var directive = section.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(directive > 0, "[MECHANISM] is not in this section");
        Assert.True(note.Index < directive,
            "the scoping note has drifted BELOW [MECHANISM] — a parent meets \"your tumor\" first");

        // The block still says both things the note is about. If a later item rewrites
        // it, this note becomes a puzzle and the test says so rather than sitting green.
        var mechanism = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md")));
        Assert.Contains("your tumor", mechanism, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Most people are told what they have long before anyone explains",
            mechanism, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void TheEscalationVoiceReminderCoversEveryPatientVoicedBlockInTheSection()
    {
        // WI-529's scoping-note lesson, extended twice by this item. Round 1 of the
        // rendered reads found [ESCALATION]'s tiers in patient voice six screens below
        // the note above [MECHANISM]; read 3 found the fix had not reached far enough.
        // Composed, this section carries TWO patient-voiced blocks, and between the
        // reminder and the second one a reader passes both tiers, both fever rules, the
        // shunt rule and two sign-off links. "The list below" had plainly ended by then.
        // NOT ReaderText(ComposedSection(...)). ReaderText strips front matter by seeking
        // "\n---"; a SECTION has none, so IndexOf returns -1 and it returns body[3..] --
        // three characters eaten, silently. Every assertion below is a Contains anchored
        // further in, so the test would go GREEN carrying the bug, and it did on the run
        // that caught this by reading rather than by failing. §12.8 (WI-520) records the
        // ReaderText(Body(page)) form of the same root cause.
        //
        // ComposedSection already flattens, and this section carries no authoring
        // markers, so nothing else is needed.
        var composed = CuratedPage.ComposedSection(Page, SymptomHeading);

        // Widened at read 3, then NARROWED again at /review round 2: "the whole of this
        // section" was false for its last four paragraphs, which the page writes itself
        // and which are already in parent voice ("if your child has a shunt"). Telling a
        // parent to read "you" as "your child" in a paragraph that already says "your
        // child" is a puzzle at 2am. It now names exactly the two blocks it covers.
        Assert.Contains("The two lists that follow are written to the person who has the tumor",
            composed, StringComparison.Ordinal);
        Assert.Contains("After them, this section goes back to speaking to you",
            composed, StringComparison.Ordinal);

        // POSITION is the property (WI-512): the reminder must precede BOTH blocks, and
        // the cord block is the later of the two.
        var reminder = composed.IndexOf("The two lists that follow", StringComparison.Ordinal);
        var escalation = composed.IndexOf("Some things should not wait", StringComparison.Ordinal);
        var cord = composed.IndexOf("new signs from the cord are their own rule", StringComparison.Ordinal);

        Assert.True(reminder >= 0 && escalation > reminder,
            "the voice reminder no longer precedes the escalation block");
        Assert.True(cord > escalation,
            "the cord block is not below the escalation block, so this ordering check is stale");
    }

    [Fact]
    public void ABabysSignsGetTheirOwnTierBecauseTheSharedBlockHasNone()
    {
        // §12.10's instruction in as many words: "add the page's own line beneath the
        // block if its emergency is not in the list". The shared [ESCALATION] tiers
        // are adult-shaped — nothing in them covers a head growing too fast, a
        // bulging soft spot, or a baby who is only irritable.
        var baby = ParagraphIn(SymptomHeading, "**In a baby or a toddler");

        // The WHY, which is the sentence that makes the list make sense rather than
        // read as a longer list: a baby's skull can still stretch, so pressure builds
        // for longer before anything shows.
        Assert.Contains("A baby's skull is not fixed shut yet", baby, StringComparison.Ordinal);
        Assert.Contains("the pressure can build for longer before anything shows",
            baby, StringComparison.Ordinal);

        foreach (var sign in new[]
                 {
                     "head that is growing too fast", "soft spot on top that bulges",
                     "sleepy or hard to settle", "feeding badly", "losing a skill",
                 })
        {
            Assert.Contains(sign, baby, StringComparison.Ordinal);
        }

        // The tier, and the shunt clause every same-day line in the corpus owes
        // (WI-535's blocker).
        Assert.Contains("same-day call, or a right-away call if your child has a shunt",
            baby, StringComparison.Ordinal);

        // POSITION: the page's own tier comes AFTER the block, so it reads as the
        // extra rule rather than as the first one (WI-512, and WI-529's fever line).
        var raw = CuratedPage.Flatten(SectionRawLf(SymptomHeading));
        var block = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        var ownTier = raw.IndexOf("In a baby or a toddler", StringComparison.Ordinal);
        Assert.True(block >= 0 && ownTier > block,
            "the page's own baby tier is not below [ESCALATION]");
    }

    [Fact]
    public void AChildWhoCannotTalkYetGetsTheSignThatStandsInForSayingItHurts()
    {
        // The single most useful escalation sentence found for a pre-verbal child,
        // and it exists nowhere else in the corpus: St. Jude's VP shunt page says a
        // worsening headache "may be the only sign of a headache in young children
        // who can't talk" and shows up as a child who is anxious, irritated or whiny.
        var section = CuratedPage.Flatten(Section(SymptomHeading));

        Assert.Contains("A young child who cannot talk yet does not say \"my head hurts\"",
            section, StringComparison.Ordinal);
        Assert.Contains("anxious, irritated or whiny", section, StringComparison.Ordinal);

        // AND THE SIGN CARRIES ITS OWN TIER (/review round 3's blocker). The paragraph
        // named a symptom and then stopped, in the section where tiers ARE the content:
        // the only instruction in it belonged to a DIFFERENT sign (cannot be woken ->
        // ambulance), so a parent at 2am with an irritable toddler chose between the
        // ambulance sentence beside it and nothing at all. [ESCALATION] does file a
        // worsening headache as same-day, but in PATIENT voice, and this page closes
        // that voice two paragraphs earlier ("this section goes back to speaking to
        // you") -- so the tier has to be in the page's own voice, which is what §12.10
        // asks for when a hub's emergency is not in the shared list. No guard could see
        // the omission: EverySameDayLineThePageWritesItselfHonoursTheShuntRule filters
        // on sentences already matching `same[- ]day`, and a sentence with NO tier never
        // enters the filter.
        Assert.Contains("That is a same-day call, or a right-away call if your child has a shunt",
            section, StringComparison.Ordinal);

        // And the one sign that is NOT same-day, kept at the tier the shared block
        // sets rather than softened into this page's own list.
        Assert.Contains("If your child cannot be woken, that is an ambulance call",
            section, StringComparison.Ordinal);
    }

    [Fact]
    public void EverySameDayLineThePageWritesItselfHonoursTheShuntRule()
    {
        // WI-535's blocker. The [ESCALATION] block makes the whole same-day tier
        // right-away for a reader with a shunt, so a same-day line the page writes
        // itself has to say so too. RAW page: the block carries its own rules, and
        // its "Same day means today" lines are not instructions to a shunted reader.
        var sameDay = PageSentences()
            .Where(s => Regex.IsMatch(s, @"\bsame[- ]day\b", RegexOptions.IgnoreCase))
            .ToList();

        Assert.True(sameDay.Count >= 1,
            $"only {sameDay.Count} same-day sentences, so this test checks almost nothing");

        foreach (var sentence in sameDay)
        {
            Assert.Matches(
                new Regex(@"right[- ]away[^.]{0,40}\bshunt|\bshunt[^.]{0,40}right[- ]away",
                    RegexOptions.IgnoreCase),
                sentence);
        }

        // And the reader can reach the shunt page's full list. Asserted on the COMPOSED
        // page, because /review round 2 found the page carrying its OWN copy of the shunt
        // rule four paragraphs below [ESCALATION]'s -- same claim, same destination, and
        // the page's copy dropped the tier change that is the whole point of the block's
        // version. The duplicate is gone; the block carries it, stronger, in the same
        // composed section. (The corpus restatement check could not see it: Shingles
        // strips markdown links, so the shared destination never enters the shingle set.)
        Assert.Contains("/treatments/shunts#warning-signs",
            CuratedPage.Flatten(CuratedPage.Composed(Page, Slug)), StringComparison.Ordinal);
        Assert.DoesNotContain("its warning signs are their own rule",
            CuratedPage.Flatten(CuratedPage.ReaderText(Page)), StringComparison.Ordinal);
    }

    [Fact]
    public void TheAnesthesiaSectionGivesTheShapeAndPublishesNoThresholdAge()
    {
        // THE ITEM'S CLEAREST §12.4 CALL. The sources disagree, and one paper
        // disagrees with ITSELF on one page: 12 in its abstract, 13 in its results,
        // 13 in its discussion. A second paper uses 4 for general anesthesia and 7
        // for deep sedation. So the page gives the shape and no number at all.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("Younger children usually need this, older children often do not, and some older children still do",
            treatment, StringComparison.Ordinal);
        Assert.Contains("Practice varies a lot between centers", treatment, StringComparison.Ordinal);

        // No age is offered as the threshold for needing anesthesia. The page DOES
        // carry "under about age 3", which is the sourced statement about brain
        // development and a different claim, so the ban is on the ages the sources
        // disagree over rather than on every numeral.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        Assert.DoesNotMatch(new Regex(@"\bage[sd]?\s+(4|5|6|7|8|9|10|11|12|13)\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b(7|8)\s*(to|or)\s*(8|9)\b"), reader);

        // Proof the guard can fire, in the shape a draft would actually use.
        Assert.Matches(new Regex(@"\bage[sd]?\s+(4|5|6|7|8|9|10|11|12|13)\b", RegexOptions.IgnoreCase),
            "about half of children over age 7 manage without it");

        // The under-3 concern IS carried, because it is the only sourced statement of
        // it in the set (the FDA page could not be fetched, so it is not cited).
        // Both halves live in "How doctors find out", not in the treatment section: the
        // page answers the anesthesia question where a parent first meets it, which is
        // the scan rather than the radiation course.
        var find = CuratedPage.Flatten(Section(FindHeading));
        Assert.Contains("Too much general anesthesia could affect how a young brain develops",
            find, StringComparison.Ordinal);
        Assert.Contains("It is a reason to ask what your center does, not a reason to refuse a scan your child needs",
            find, StringComparison.Ordinal);
    }

    [Fact]
    public void TheThingsAParentCanAskForAreNamedRatherThanGesturedAt()
    {
        // §12.11: "name the thing you tell the reader to ask about". A survey of
        // centers reports three specific non-drug measures, and each is something a
        // parent can say out loud.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));

        foreach (var askable in new[]
                 {
                     "letting a parent into the treatment area",
                     "letting a child use a tablet",
                     "having a child life specialist there",
                 })
        {
            Assert.Contains(askable, treatment, StringComparison.Ordinal);
        }

        // And the scan half, where the alternative to medicine is the actual answer.
        var find = CuratedPage.Flatten(Section(FindHeading));
        Assert.Contains("Deep breathing, music or a movie is often", find, StringComparison.Ordinal);
        Assert.Contains("You can usually stay with your child until they are asleep",
            find, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageRoutesToTheSiblingsThatOwnTheMaterialRatherThanRestatingThem()
    {
        // §12.11 and WI-529's first lesson, mechanised: for each claim, the SIBLING is
        // read, so moving a sentence between pages later goes red instead of creating
        // a silent second copy. Every one of these was in this item's source pack and
        // every one is already shipped somewhere else.
        var mine = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        (string Sibling, string Owned, string Route)[] owned =
        [
            // The source pack's HEADLINE fact for this page, already on the glioma hub.
            ("tumors/glioma", "Those describe the tumor's biology, not the age of the person who has it",
                "/tumors/glioma"),
            // /treatments/proton-therapy's own test forbids any other page restating this.
            ("treatments/proton-therapy", "stops at the tumor instead of carrying on through",
                "/treatments/proton-therapy"),
            ("treatments/radiation-therapy", "a mesh mask is molded to your face",
                "/treatments/radiation-therapy"),
            ("treatments/craniotomy", "thin tube coming out of the wound to let fluid drain away",
                "/treatments/craniotomy"),
        ];

        foreach (var (siblingSlug, sentence, route) in owned)
        {
            var parts = siblingSlug.Split('/');
            var sibling = Sibling(parts[0], parts[1] + ".md");

            Assert.True(sibling.Contains(sentence, StringComparison.OrdinalIgnoreCase),
                $"/{siblingSlug} no longer says \"{sentence}\", so this non-duplication check is "
                + "guarding something that has moved or been reworded");
            Assert.DoesNotContain(sentence, mine, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(route, mine, StringComparison.Ordinal);
        }

        // The pre-surgery page owns when to stop eating and drinking, and this page
        // does not repeat it even though its own sources carry it.
        Assert.DoesNotMatch(new Regex(@"stop (eating|food)|nothing to eat|not to eat", RegexOptions.IgnoreCase), mine);
    }

    [Fact]
    public void ThePosteriorFossaLeadInIsThisPagesOwnAndScopesTheBlockToTheOperation()
    {
        // This page is the THIRD includer of the block, and the first that is not a
        // single tumor. The block qualifies here on §12.10's CONDITIONAL rule — its
        // own opening clause scopes it to surgery low at the back of the brain — so
        // the lead-in has to make that condition visible BEFORE the block, or a
        // parent whose child was operated on elsewhere reads it as theirs.
        var sub = CuratedPage.Flatten(
            CuratedPage.ComposedSubsection(Page, "After surgery low at the back of the head"));

        Assert.Contains("If your child's was somewhere else, this section does not apply",
            sub, StringComparison.Ordinal);

        // The block's own paragraphs arrive by composition, and the carve-out is the
        // half that cannot wait for a diagnosis.
        Assert.Contains("some children stop talking", sub, StringComparison.Ordinal);
        Assert.Contains("**Suddenly not being able to speak is on the ambulance list above, and that rule still stands.**",
            sub, StringComparison.Ordinal);

        // Each including hub writes its OWN lead-in, and this page must not have
        // copied either of the two that came before it (§12.10: one claim, one
        // strength, but the per-hub sentences stay with the hubs).
        var mine = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        foreach (var siblingLine in new[]
                 {
                     "This is common after surgery for a medulloblastoma",
                     "Some children get this after surgery for an ependymoma",
                 })
        {
            Assert.DoesNotContain(siblingLine, mine, StringComparison.Ordinal);
        }

        // The anchor the glossary entry and the sibling hubs can deep-link.
        Assert.Contains("{#posterior-fossa-syndrome}", SectionRawLf(AfterHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheChemotherapyAtHomeSectionIsTheOnePlaceTheCorpusCoversHouseholdExposure()
    {
        // Genuinely unowned: no page in the corpus mentions that chemotherapy can be
        // taken in through the skin or breathed in, or that a household can be
        // exposed through food and surfaces. It belongs here because the person
        // handling it at home is this page's reader.
        var sub = CuratedPage.Flatten(
            CuratedPage.ComposedSubsection(Page, "Giving chemotherapy at home"));

        Assert.Contains("taken in through the skin or breathed in", sub, StringComparison.Ordinal);
        Assert.Contains("if it reaches food or the surfaces you use every day", sub, StringComparison.Ordinal);
        Assert.Contains("ask to be shown how to handle it", sub, StringComparison.Ordinal);

        // §12.6: it lands on an action and does not end on the frightening half.
        Assert.Contains("worth getting right rather than worrying about", sub, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLateEffectReframeIsCarriedBecauseItChangesHowAParentReadsEverythingElse()
    {
        // The most valuable sentence in the source set, and the both-directions
        // honesty §12.8 asks for: the problem is usually NOT that skills are lost.
        var section = CuratedPage.Flatten(Section(ThinkingHeading));

        Assert.Contains("the problem is not that skills are lost", section, StringComparison.Ordinal);
        Assert.Contains("new skills are picked up more slowly", section, StringComparison.Ordinal);
        Assert.Contains("Children keep learning and keep developing", section, StringComparison.Ordinal);

        // The delay, which is what makes a parent think something new has gone wrong.
        Assert.Contains("It may not show up for months or years", section, StringComparison.Ordinal);

        // The hedge that keeps it honest: whether chemotherapy ALONE causes lasting
        // effects is not settled, and the source says so in as many words.
        Assert.Contains("Whether chemotherapy on its own causes lasting effects is genuinely not settled",
            section, StringComparison.Ordinal);

        // §12.6: the section lands on the action, not on the damage.
        Assert.Contains("The thing that helps is testing, early", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFollowUpSectionSaysWhatItIsKeyedToAndNamesTheDocumentToAskFor()
    {
        var section = CuratedPage.Flatten(Section(ScansHeading));

        // The key insight for a parent, from the guidelines' own description: what
        // gets checked follows the TREATMENT the child received, not the tumor's name.
        Assert.Contains("depends on what your child was treated with", section, StringComparison.Ordinal);

        // The artifact, and the reason it matters rather than just its name (§12.11:
        // name the thing you tell the reader to ask for).
        Assert.Contains("Ask for a survivorship care plan", section, StringComparison.Ordinal);
        Assert.Contains("may see one or two such patients in a whole career", section, StringComparison.Ordinal);
        Assert.Contains("Ask for an updated copy at each follow-up visit", section, StringComparison.Ordinal);

        // What a visit actually contains, which is what parents ask for.
        foreach (var check in new[] { "balance, walking and reflexes", "hormone levels", "hearing test" })
        {
            Assert.Contains(check, section, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheSchoolSectionNamesTheTwoRoutesAndSaysWhichCountryTheyBelongTo()
    {
        // The jurisdiction trap, same class as the driving rule §12.3 section 9 keeps
        // off every tumor hub: IEP and 504 are US federal law and nothing else.
        var section = CuratedPage.Flatten(Section(SchoolHeading));

        Assert.Contains("In the United States there are two routes", section, StringComparison.Ordinal);
        Assert.Contains("These are US laws", section, StringComparison.Ordinal);
        Assert.Contains("your country will have its own route", section, StringComparison.Ordinal);

        // The actual difference between them, from the regulator rather than from
        // folklore: one is a funding statute, the other an anti-discrimination law.
        Assert.Contains("It does not come with funding attached, and an IEP does",
            section, StringComparison.Ordinal);

        // The age split, which matters because a baby treated at nine months is not
        // covered by the same part of the law as a school-age child.
        // FLATTENED, and that is load-bearing rather than tidy: this sentence is a list
        // item whose continuation line is indented two spaces, so a literal with a bare
        // "\n" in it can never match (§12.8's hard-wrapped-anchor lesson, WI-535).
        Assert.Contains("Babies and toddlers under 3 come under a different part",
            section, StringComparison.Ordinal);

        // The gap this section exists to close, and the year-two problem nobody warns
        // families about.
        // Worded TO the source rather than past it (/review round 3). The paper says
        // "While only a minority of PBT survivors access such supports": the denominator
        // is survivors, not the eligible subset, and "access" is not "never get".
        Assert.Contains("Most children treated for a brain tumor do not use the school support that exists",
            section, StringComparison.Ordinal);
        Assert.Contains("Ask again each year", section, StringComparison.Ordinal);
    }

    [Fact]
    public void PalliativeCareIsIntroducedByWhatItIsBeforeAnybodyCanMishearIt()
    {
        // The source's own anti-misreading sentence goes FIRST (§12.6, warn before you
        // disclose, applied in the other direction: correct the misreading before the
        // word can do its damage).
        var section = CuratedPage.Flatten(Section(PalliativeHeading));
        var sentences = CuratedPage.SentencesOf(section);

        Assert.Contains("can happen at any stage of an illness, at any age", sentences[0],
            StringComparison.Ordinal);
        Assert.Contains("asking for it does not mean anyone has given up", section,
            StringComparison.Ordinal);

        // The one fact here that is genuinely specific to children and appears on no
        // other page in the corpus.
        Assert.Contains("children can receive hospice support while still having treatment aimed at curing the tumor",
            section, StringComparison.Ordinal);

        // The sibling hub frames it its own way and this page must not restate that
        // page's sentence (§12.10).
        Assert.DoesNotContain("The word can sound like giving up", section, StringComparison.Ordinal);
        Assert.Contains("The word can sound like giving up", Sibling("tumors", "dipg.md"),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheParentSectionsAnswerTheQuestionThatHasNoOtherHomeOnTheSite()
    {
        // Telling a child, and telling their brothers and sisters, is unowned in the
        // corpus and is the thing a parent is doing on day one.
        var talking = CuratedPage.Flatten(Section(TalkingHeading));

        Assert.Contains("A child who is not told will fill the gap with their imagination",
            talking, StringComparison.Ordinal);
        Assert.Contains("what they imagine is often worse than the truth", talking, StringComparison.Ordinal);
        Assert.Contains("Children often think it is their fault", talking, StringComparison.Ordinal);

        // §12.6: it lands on something solid rather than on the difficulty.
        Assert.Contains("You know your child better than the team does", talking, StringComparison.Ordinal);

        var siblings = CuratedPage.Flatten(Section(SiblingsHeading));
        Assert.Contains("nothing anyone thinks, says or does can make somebody else ill",
            siblings, StringComparison.Ordinal);
        Assert.Contains("Keep their life going", siblings, StringComparison.Ordinal);
        Assert.Contains("They are being strong too, and nobody tells them", siblings, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCaregiverSectionSaysWhyItReadsStrangelyOnAPageAlreadyWrittenToTheParent()
    {
        // The doubling no other hub has: everywhere else the caregiver section is the
        // one part addressed to somebody other than the patient. Here the whole page
        // already is, so the section has to say so or it reads as a mistake.
        var section = CuratedPage.Flatten(Section(CaregiverHeading));

        Assert.Contains("this whole page is written to you", section, StringComparison.Ordinal);
        Assert.Contains("Here it is the other way round", section, StringComparison.Ordinal);

        // The three claims of its own, each true of a parent rather than of a
        // caregiver in general (§12.7: the per-page half).
        Assert.Contains("Two things specific to being a parent", section, StringComparison.Ordinal);
        Assert.Contains("You are allowed to need support of your own", section, StringComparison.Ordinal);
        Assert.Contains("You are the one holding the whole picture", section, StringComparison.Ordinal);

        // AND NOT A THIRD, because the draft's third claim was "ask to be shown anything
        // you are sent home to do" — the [CAREGIVER] block's own advice, almost word for
        // word, twelve lines under the block itself. That is the duplication WI-529's
        // rendered read found in this exact section, and here the corpus-wide restatement
        // check caught it instead. Read from the BLOCK, so if that advice ever moves off
        // it this goes red rather than silently permitting the copy again.
        var caregiverBlock = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")));
        Assert.Contains("Ask to be shown anything you are sent home to do",
            caregiverBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("Ask to be shown anything you are sent home to do",
            section, StringComparison.Ordinal);

        // Written TO the caregiver, never about them from outside (§12.7).
        Assert.DoesNotMatch(new Regex(@"(parents|caregivers) (often|usually|tend to)", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnTheComposedPage()
    {
        // PARAGRAPH-scoped and COMPOSED (WI-536/WI-537). The reassurance this page has
        // to be checked against arrives from the posterior fossa block, so read raw
        // this guard would lose its subject entirely.
        CuratedPage.AssertNoWarningSignIsNormalised(Composed, Slug,
            "Most children slowly get better");
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // Twenty of this item's fetched sources publish survival figures and none
        // reaches the page (§12.5). The proton literature states its own rationale in
        // terms of survival; the reason is carried and the number is not.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        Assert.DoesNotMatch(new Regex(@"\b(median|survival|survive)\b[^.]{0,40}\d", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\d[^.]{0,40}\b(median|survival|survive)\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"survival rate|cure rate", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*%"), reader);
        Assert.DoesNotMatch(new Regex(@"per cent|percent", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"(one|two|three|five|ten)[- ]year", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+\s+(in|out of)\s+\d+\b"), reader);
    }

    [Fact]
    public void TheOutlookGateTeachesTheWordAndRefusesToRankChildhoodTumorsAsAGroup()
    {
        var section = CuratedPage.Flatten(Section(OutlookHeading));
        var gate = Regex.Match(section, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");

        var inside = gate.Groups[1].Value;

        // §12.5 / §12.9: a gate that publishes no figures still has to teach the word.
        // Worded differently from the eight sibling hubs on purpose — §12.5 prescribes
        // WHAT a gate teaches, which made the first draft collide with all of them on
        // the corpus-wide restatement check.
        Assert.Contains("It means the middle of a group.", inside, StringComparison.Ordinal);
        Assert.Contains("It is not a prediction about one child", inside, StringComparison.Ordinal);

        // THE HONEST POSITION FOR A CROSS-CUTTING PAGE, and the reason this gate is
        // shorter than a hub's: outlook belongs to the tumor's own name, not to an age
        // group, and saying so is the content.
        Assert.Contains(
            "depends far more on which tumor your child has than on the fact that your child is a child",
            inside, StringComparison.Ordinal);

        // And it lands on the team and on permission to decline (§12.6).
        Assert.Contains("you are allowed to say not yet", inside, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGatedWordsAppearNowhereOutsideTheGate()
    {
        // COMPOSED minus the gate, case-insensitive, with the euphemisms and
        // kindnesses outlook arrives as (WI-535: a pointer, a euphemism or a kindness,
        // none of them a banned word).
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        var gate = Regex.Match(whole, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");
        var outside = whole.Replace(gate.Value, "", StringComparison.Ordinal);

        foreach (var gated in new[]
                 {
                     @"\bmedian\b", @"life expectancy", @"\bprognos\w*", @"long[- ]term survi\w*",
                     @"end[- ]of[- ]life", @"\bterminal\b", @"how long\b[^.?]{0,30}\b(have|live|left)\b",
                     @"\bdie\b", @"\bdies\b", @"\bdied\b", @"\bdying\b",
                 })
        {
            Assert.DoesNotMatch(new Regex(gated, RegexOptions.IgnoreCase), outside);
        }

        // "hospice" is the deliberate exception and it is NOT a leak: the palliative
        // section uses it to say a child can have it WHILE having treatment aimed at
        // cure, which is the concurrent-care fact. Pinned so the exception stays that
        // sentence rather than becoming a general licence.
        var hospice = Regex.Matches(outside, @"\bhospice\b", RegexOptions.IgnoreCase).Count;
        Assert.True(hospice == 1, $"'hospice' appears {hospice} times outside the gate; exactly one is expected");
        Assert.Contains("hospice support while still having treatment aimed at curing the tumor",
            outside, StringComparison.Ordinal);
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

        // "do well" is in RejectedCharacterisations precisely because of this page:
        // WI-537 put it on the live list and /review sent it back, on the grounds that
        // a pediatric hub is written in a school register where "help your child do
        // well at school" is the natural sentence. It is not banned, and this page
        // does not lean on it either.
        Assert.Contains("do well", CuratedPage.RejectedCharacterisations);
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        // The item's live risk rather than a theoretical one: the best-written
        // children's pages in the source set are Cancer Research UK's, and their idiom
        // sits in the exact sentences this page wants ("GP" thirteen times, and the
        // symptom list itself written as "feeling or being sick").
        //
        // RAW page, not composed: blocks/mechanism.md carries "feeling sick", which is
        // WI-564's subject and not this page's (WI-537 recorded the same deferral).
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var idiom in new[]
                 {
                     "A&E", "GP", "straight away", "straight after", "out of hours",
                     "being sick", "feeling sick", "fits", "999", "high dependency",
                     "late effects clinic", "freephone", "plaster",
                 })
        {
            Assert.DoesNotMatch(
                new Regex($@"(?<![\w&]){Regex.Escape(idiom)}(?![\w&])", RegexOptions.IgnoreCase), reader);
        }
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus()
    {
        // §12.8 (WI-521), corpus-wide rather than hand-picked neighbours. A
        // cross-cutting page is the one most likely to trip this, because every
        // sentence it writes is about a subject some hub already covers.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug,
            // THE SHUNT CLAUSE IS SHARED ON PURPOSE, and rewording it to satisfy this
            // check would be the defect rather than the fix. Four hubs carry it in the
            // same words because §12.10 wants one claim at one strength, and WI-537
            // recorded explicitly that changing either side creates the two-strengths
            // problem the corpus exists to prevent. Allowed, not rewritten.
            "any of those is a same day call or a right away call if your child has a shunt",
            // The scoping note QUOTES the block it scopes, which is exactly what
            // /tumors/all-brain-tumors' note does, for the same block and the same reason
            // (WI-529). Two pages teaching a reader how to read one shared sentence will
            // always overlap on that sentence.
            "it also opens by saying that most people are told what they have before anyone explains what it is doing");
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

        // The URL VALUES only. A ban run over the whole front matter fires on this
        // page's own rulings, which name the barred sources in order to explain why
        // they are barred — a guard that is red on a correct page is worse than no
        // guard (§12.8, WI-529).
        var cited = string.Join("\n", urls.Select(m => m.Groups[1].Value));

        // §12.1: the stub this replaced cited cancer.gov/types/brain, which is barred
        // for naming.
        Assert.DoesNotContain("cancer.gov/types/brain", cited, StringComparison.Ordinal);

        // NOTHING ON THIS PAGE IS TAKEN FROM CANCER RESEARCH UK. Its children's pages
        // are the best written in the set and every one is British in the sentences
        // this page wants; eight of the nine are also three years old and the same
        // eight carry their own overdue-review notice. (The undercount "five ... and
        // one" was corrected on the page at round 2 and left standing here until
        // round 3 — a recorded reason deserves the same check as a claim, WI-536.)
        Assert.DoesNotContain("cancerresearchuk.org", cited, StringComparison.OrdinalIgnoreCase);

        // BUT THE READER STILL MEETS TWO CRUK ENTRIES, and the first version of the
        // comment above claimed they did not — a recorded justification that was false
        // of the rendered page, which only rendered read 1 could show (WI-536: a reason
        // deserves the same check as a claim). `blocks/escalation.md` cites CRUK, block
        // sources merge in and RENDER (§12.10), and one of them carries the visibly
        // British title "Brain tumour symptoms". Asserted from the BLOCK so this stays
        // honest: if the block ever drops CRUK, this goes red and the comment above
        // stops being a half-truth rather than quietly becoming one.
        var escalationFront = CuratedPage.FrontMatter(CuratedPage.EscalationBlock);
        Assert.Contains("cancerresearchuk.org", escalationFront, StringComparison.OrdinalIgnoreCase);

        // NO FDA PAGE. The pediatric anesthesia warning could not be fetched at either
        // of two URLs, so the under-3 concern is carried first-hand from St. Jude
        // instead. A claim whose source could not be read is not a sourced claim.
        Assert.DoesNotContain("fda.gov", cited, StringComparison.OrdinalIgnoreCase);

        // The sources the page's load-bearing sections actually rest on.
        foreach (var required in new[]
                 {
                     "together.stjude.org", "cancer.org", "childrensoncologygroup.org",
                     "ed.gov", "PMC8068230", "PMC4362639", "PMC12188901", "PMC8700207",
                 })
        {
            Assert.Contains(required, cited, StringComparison.OrdinalIgnoreCase);
        }
    }
}

[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class PediatricBrainTumorPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/pediatric-brain-tumor";

    private readonly WebApplicationFactory<Program> _factory;

    public PediatricBrainTumorPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        // Contract item 10: the URL is preserved. It was a 47-line stub and is now a
        // full page, and the slug did not move.
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
                     "[MECHANISM]", "[CROSSWALK]", "[CAUSES]", "[CAREGIVER]",
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
                     "nobody knows the cause", "caring for someone",
                     "If you have a shunt", "Most children slowly get better",
                 })
        {
            Assert.Contains(canary, html, StringComparison.OrdinalIgnoreCase);
        }

        // The cord rule reaches the reader (/review round 1 reversed its exclusion), so
        // its canary is a positive one like every other block's.
        Assert.Contains("right-away call, not a same-day one", html, StringComparison.Ordinal);
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
    public async Task TheNewGlossaryTermIsSuppressedHereBecauseThePageDefinesItInline()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // WI-535's lesson: a new glossary entry can echo the sentence it lands in.
        // This page glosses "embryonal tumor" in its own next clause, so the tooltip
        // would print the definition immediately above the definition. Suppressed
        // here with !%embryonal tumor%, and the entry still fires everywhere else,
        // which is the whole reason for adding it at its second use.
        Assert.DoesNotContain("def-embryonal-tumor", html, StringComparison.Ordinal);
        Assert.Contains("embryonal tumors", html, StringComparison.OrdinalIgnoreCase);

        // The authoring marker itself must never survive into the page.
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.DoesNotContain("%%", html, StringComparison.Ordinal);
    }
}
