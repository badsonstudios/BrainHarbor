using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-544: <c>/tumors/atrt</c>, a NEW page rather than a deepened stub.
///
/// THE ITEM'S CENTRE IS AN URGENCY RULING BUILT ON A SCORED ABSENCE.
/// Seventeen ATRT-specific pages were fetched and NOT ONE gives an emergency-room or
/// 911 rule; four were re-tested by me by direct question and all four are silent.
/// So this page does not invent a tumor-level emergency rule. Every emergency
/// instruction it carries belongs to a SIGN or a DEVICE and is attributed to the
/// source that actually gives it. Evidence log:
/// <c>.claude/work_files/wi544/research/VERIFIED-by-me-urgency-treatment.md</c>.
///
/// The guards below therefore assert the ruling as SEPARATE PROPERTIES. A page
/// carrying only the reassuring half, or only one tier, would still read as a
/// complete page, and §12.12 records that over-reassurance is the failure that stops
/// being true later at the worst possible moment.
/// </summary>
public sealed class AtrtPageContentTests
{
    private const string Slug = "tumors/atrt";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is ATRT?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string WhereHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause in a baby or young child?";
    private const string DiagnosisHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my child's report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string LifeHeading = "Everyday life: feeding, development, hearing and school";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string RecurrenceHeading = "If it comes back, or changes";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CareHeading = "For the person caring for someone with this";
    private const string QuestionsHeading = "What to ask your team";
    private const string SupportHeading = "Where to get support";

    private static string Page => CuratedPage.Read("tumors", "atrt.md");

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string Plain => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section of the RAW page, unflattened, for assertions about paragraphs and
    /// POSITION. <c>CuratedPage.Section</c> flattens before it returns, and §12.11
    /// records the cost: a splitter handed flattened text finds no newlines, yields one
    /// chunk, and every per-chunk assertion quietly becomes a whole-section assertion.
    /// <c>\r?\n</c> throughout, never a bare <c>\n</c> -- this repo hands working trees
    /// CRLF while CI sees LF.
    /// </summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$").Select(m => m.Groups[1].Value.Trim())];

    /// <summary>
    /// The sentence of <paramref name="text"/> containing <paramref name="needle"/>.
    ///
    /// EMPHASIS IS STRIPPED FIRST, AND THE BREAK HARNESS IS WHY. `SentencesOf` splits
    /// on <c>(?&lt;=[.!?])\s+</c>, and <c>ReaderText</c> removes the <c>!%…%</c> markers
    /// but NOT markdown <c>**</c>. So in a bolded bullet list the character after
    /// "call 911." is <c>*</c> rather than whitespace, no split happens, and one
    /// "sentence" runs on through the NEXT bullet.
    ///
    /// That made this file's infection-venue assertion vacuous for the whole item: it
    /// asserted the infection rule keeps "emergency room", and passed on the venue
    /// belonging to the bullet ABOVE it. Stripping the venue from the infection rule
    /// left the suite green at 2,345 / 2,345 — a guard green on the defect it forbids,
    /// on the infection/malfunction split this page defended hardest.
    ///
    /// NOTHING FOUND IT BUT THE HARNESS. It is invisible to review (the assertion reads
    /// correctly), invisible to the suite (it passes), and invisible to the dry run
    /// (the anchor resolves). Only mutating the page and demanding red could see it.
    /// </summary>
    private static string SentenceWith(string text, string needle)
    {
        var plain = text.Replace("**", " ", StringComparison.Ordinal)
                        .Replace("*", " ", StringComparison.Ordinal);
        var sentence = CuratedPage.SentencesOf(plain)
            .FirstOrDefault(s => s.Contains(needle, StringComparison.OrdinalIgnoreCase));
        Assert.False(string.IsNullOrWhiteSpace(sentence), $"no sentence contains '{needle}'");
        return sentence!;
    }

    // ------------------------------------------------------------------ the ruling

    [Fact]
    public void TheEmergencyRulesAreAttributedToTheSignsAndNotToTheTumor()
    {
        var section = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(SymptomHeading)));

        // THE HONEST STATEMENT, and it is the claim the whole item turns on. The page
        // says out loud where its emergency advice comes from, because no ATRT source
        // gives any. Without this sentence a reader reasonably assumes the tumor's own
        // organizations said "go to an emergency room", and none of them did.
        Assert.Matches(new Regex(
            @"These rules come from the signs, not from the tumor", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"do not tell you when to go to an emergency room", RegexOptions.IgnoreCase), section);

        // 1. THE FONTANELLE RULE. The only 911-level instruction in the whole source
        //    set, and the one a parent is most likely to need at 2am.
        Assert.Matches(new Regex(
            @"bulging while your baby is calm and sitting up means going\s+in now",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"Call 911 or go to the nearest emergency room", RegexOptions.IgnoreCase), section);

        // 2. RAISED PRESSURE, the one emergency source that names a brain tumor as a
        //    cause -- which is what connects the sign-level rules to this page at all.
        Assert.Matches(new Regex(
            @"Rising pressure inside the head is treated as an emergency in its own right",
            RegexOptions.IgnoreCase), section);

        // 3. THE SHUNT RULES, all three, each named individually. A shortened list is
        //    the §12.12 direction, and the sign most likely to be dropped is the one
        //    with no venue attached (below).
        Assert.Matches(new Regex(
            @"If you cannot wake your child, go to the nearest emergency room right\s+away,\s*or call 911",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"If you think the shunt is infected, tell your neurosurgeon immediately, or go\s+to\s+the emergency room",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"If you think the shunt has stopped working, get medical help immediately",
            RegexOptions.IgnoreCase), section);

        // THE LEAD-IN'S COUNT MATCHES THE BULLETS, AND NOTHING CHECKED THAT BEFORE.
        // /review round 1 found the lead-in promising TWO rules above three bullets --
        // in the emergency copy, where the third is the malfunction rule that carries
        // no venue and is the one this item fought hardest to keep distinct. Each rule
        // was asserted individually above, so the mismatch was invisible to the suite:
        // the guards saw three present rules and no claim about how many there were.
        var raw = RawSection(SymptomHeading).Replace("\r\n", "\n");
        var lead = Regex.Match(raw, @"(Two|Three|Four) things are worth knowing by heart\.");
        Assert.True(lead.Success, "the shunt lead-in has gone");

        // THE SLICE ENDS AT THE NEXT PARAGRAPH THAT IS NOT PART OF THE LIST, AND THIS
        // TOOK THREE GOES. Worth recording all three, because each failure is a
        // different lesson:
        //
        //   1. `IndexOf("\n\n[")` worked, but returns -1 the moment the following
        //      paragraph stops starting with a bracket -- so a real regression would
        //      have surfaced as a range exception rather than as this guard's message.
        //   2. `^(?!- |\s*$).+$` was the "hardened" replacement and it BROKE THE GUARD:
        //      the bullets are hard-wrapped, so each continuation line begins with two
        //      spaces, matches "not a bullet and not blank", and ended the slice after
        //      the FIRST bullet. It reported 3 promised against 1 found, on a page that
        //      genuinely has three. I fixed a theoretical crash and created a real
        //      false failure.
        //   3. A list item's continuation is INDENTED, so the list ends at the first
        //      line starting with a character that is neither whitespace nor a hyphen.
        //
        // The saving grace at step 2 was that the assertion prints BOTH numbers, so a
        // broken slice is one step from diagnosis instead of a bisect. A guard that
        // fails should say what it saw.
        var after = raw[(lead.Index + lead.Length)..];
        var stop = Regex.Match(after, @"(?m)^[^\s-].+$");
        var bullets = Regex.Matches(stop.Success ? after[..stop.Index] : after, @"(?m)^- ").Count;

        // AND THE SLICE ITSELF IS PROVED NON-VACUOUS. A slicing bug that collapses the
        // list to nothing would otherwise read as a count mismatch and send the next
        // person editing the prose instead of the regex.
        Assert.True(bullets >= 2,
            $"the bullet slice found {bullets} item(s), so it is cutting the list short "
            + "rather than measuring it");
        var promised = new Dictionary<string, int>
        {
            ["Two"] = 2, ["Three"] = 3, ["Four"] = 4,
        }[lead.Groups[1].Value];
        Assert.True(promised == bullets,
            $"the shunt lead-in promises {promised} rules and {bullets} bullets follow");
    }

    [Fact]
    public void TheShuntMalfunctionRuleIsNotUpgradedToAnEmergencyRoom()
    {
        // THE RESEARCH PACK MERGED TWO RULES THAT THE SOURCE KEEPS APART, and re-reading
        // the live page is what separated them again. The Hydrocephalus Association
        // attaches "go to the emergency room" to suspected INFECTION, and gives suspected
        // MALFUNCTION "seek immediate medical assistance" with NO VENUE NAMED.
        //
        // Over-triage is usually the safe direction in this corpus, but not here: adding
        // a venue the source does not give would be inventing an instruction and
        // attributing it, which is the §12.14 defect class (a citation that does not
        // support the claim in the form it is made).
        //
        // WHAT THIS GUARD PINS IS WHAT THE PAGE ADDS, NOT WHAT THE READER RECEIVES, AND
        // THE FIRST VERSION OF THIS COMMENT OVERSTATED IT (/review round 1). Composed,
        // blocks/escalation.md tells any reader with a shunt that those signs mean "call
        // your team, or go to the emergency department" -- so the distinction does not
        // survive composition. That is not a safety defect, because the block
        // over-triages and that is the direction this corpus chooses; but it is a
        // property of the SOURCE file, which is all RawSection can see. A guard whose
        // comment claims more than its assertion can reach is the shape §12.8 records
        // again and again, so the claim is scoped here rather than left flattering.
        var section = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(SymptomHeading)));
        var malfunction = SentenceWith(section, "If you think the shunt has stopped working");

        foreach (var venue in new[] { "emergency room", "911", "emergency department" })
        {
            Assert.DoesNotContain(venue, malfunction, StringComparison.OrdinalIgnoreCase);
        }

        // PROVED ABLE TO FIRE. A ban checked only against the sentence that prompted it
        // is a ban proved by nothing (§12.8, WI-523).
        Assert.Contains("emergency room",
            "If you think the shunt has stopped working, go to the emergency room.",
            StringComparison.OrdinalIgnoreCase);

        // And the INFECTION rule keeps its venue, so this guard cannot be satisfied by
        // stripping venues from both.
        Assert.Contains("emergency room",
            SentenceWith(section, "If you think the shunt is infected"),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheFontanelleRuleKeepsTheCalmAndUprightTestThatMakesItUsable()
    {
        // THE TEST IS THE RULE. A soft spot looks full whenever a baby cries, lies down
        // or throws up, and that is ordinary -- so "a bulging soft spot is an emergency"
        // without the qualifier sends every parent of a crying baby to an emergency room,
        // and a rule that fires on the normal case gets ignored on the real one.
        //
        // BOTH HALVES ARE ASSERTED. The first draft of this guard checked only that the
        // emergency instruction was present, which a page carrying the dangerous short
        // form would also satisfy.
        var section = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(SymptomHeading)));

        Assert.Matches(new Regex(
            @"often looks\s+full when a baby is crying, lying down or throwing up, and that is ordinary",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"If it goes back\s+to soft and flat, that is the normal kind", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"If it stays bulging when your baby\s+is calm and upright, that is the emergency",
            RegexOptions.IgnoreCase), section);

        // AND THE QUALIFIER MUST TRAVEL WITH THE INSTRUCTION. Asserting both strings
        // exist somewhere would pass on a page that put them four screens apart.
        // BOTH WORD ORDERS, AND THE CANARY IS WHY THIS COMMENT EXISTS. The first
        // version required "soft spot" BEFORE "bulging" -- and its own canary, "A
        // bulging soft spot means call 911", writes them the other way round, which is
        // also the most natural way anyone would phrase the dangerous short form. So the
        // ban was blind to exactly the sentence it existed to catch, and only the canary
        // said so. Identical to the bidirectional holes WI-543 fixed three times.
        const string Spot = @"(?:soft spot|fontanel\w*)";
        var shortForm = new Regex(
            $@"{Spot}[^.]{{0,40}}bulg\w*[^.]{{0,40}}\b(?:911|emergency room)\b"
            + $@"|bulg\w*[^.]{{0,40}}{Spot}[^.]{{0,40}}\b(?:911|emergency room)\b",
            RegexOptions.IgnoreCase);

        Assert.Matches(shortForm, "A bulging soft spot means call 911.");
        Assert.Matches(shortForm, "A soft spot that is bulging means the emergency room.");
        Assert.Matches(shortForm, "A bulging fontanelle means call 911.");

        // The page's own instruction keeps the qualifier BETWEEN the sign and the venue,
        // which is what puts it out of reach of this ban rather than luck.
        Assert.DoesNotMatch(shortForm, section);

        // AND THE BAN RUNS ON THE WHOLE PAGE, NOT JUST THIS SECTION (/review round 5).
        // It used to check the symptoms section alone -- but the CAREGIVER section
        // repeats the same sign ("A soft spot that stays bulging when your baby is calm
        // and upright"), and the dangerous short form is just as dangerous there. The
        // page is clean today, so this changes nothing now; it changes what happens when
        // somebody adds the short form to the section the ban could not see.
        //
        // This is the same family as the bidirectional hole recorded above: a guard that
        // is right about its rule and wrong about its REACH. Scope is part of a ban.
        Assert.DoesNotMatch(shortForm, Plain);
    }

    [Fact]
    public void ThePagesOwnBabyRuleComesBeforeTheSharedEscalationBlock()
    {
        // WI-543's rendered read is why this exists. Composed, [ESCALATION] puts twenty
        // lines of ADULT brain emergencies above the reader -- seizures, a headache worse
        // than usual, confusion, a shunt conditional -- and a parent looking for the one
        // sign that is their baby's had to wade through all of it first.
        //
        // The source file shows none of this. Only a rendered read does.
        var raw = RawSection(SymptomHeading);
        // THE HEADING IS DELIBERATELY NOT "When to get help now", AND THE FIRST RENDERED
        // READ IS WHY. That was its original wording, and composed, the [ESCALATION] block
        // brings its own "### When to call for help right now" twenty-four lines below it.
        // Two near-identical subheadings in one section is a page a frightened parent
        // cannot scan -- and the SOURCE FILE shows only one of them, so nothing but a
        // rendered read could have caught it.
        var own = raw.IndexOf("### The signs that mean going in now", StringComparison.Ordinal);
        var shared = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);

        Assert.True(own >= 0 && shared >= 0, "the symptoms section has lost one of its two rules");
        Assert.True(own < shared,
            "the shared escalation block now composes ABOVE this page's own baby rule, so a "
            + "parent meets adult seizure and headache tiers before the bulging soft spot");

        // And the general list is INTRODUCED rather than dumped, so its adult content
        // reads as "not all of this is yours" instead of as this page's own advice.
        Assert.Matches(new Regex(
            @"The general list below speaks straight to a patient", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(raw)));

        // THE [SPINAL-CORD] LEAD-OUT IS PINNED BY POSITION, AND NOTHING PINNED IT
        // BEFORE. /review round 1: delete that paragraph and the whole suite stayed
        // green while a parent inherited an UNSCOPED cord rule -- a block written in
        // the second person to a patient, telling them an arm that is "newly weak" is
        // a right-away call, with nothing saying who it is for. The scoping must come
        // AFTER the directive here (unlike [CAUSES], which is scoped before it),
        // because the block is what raises the question the paragraph answers.
        var cord = raw.IndexOf("[SPINAL-CORD]", StringComparison.Ordinal);
        var scoped = raw.IndexOf("That last rule applies only where the growth sits",
            StringComparison.Ordinal);
        Assert.True(cord >= 0, "the symptoms section has lost [SPINAL-CORD]");
        Assert.True(scoped > cord,
            "the cord block's scoping paragraph has gone or drifted above it, so a parent "
            + "meets a cord rule written to an adult patient with nothing saying who it is for");

        // And it keeps the caveat that is genuinely this page's own: a bladder or bowel
        // sign cannot be read the usual way in a child who is not toilet trained.
        Assert.Matches(new Regex(
            @"not toilet trained yet", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(raw)));
    }

    // ------------------------------------------------------------- the block rulings

    [Fact]
    public void ExactlySixBlocksAreIncludedAndTheTwoExclusionsAreAClosedSet()
    {
        // AN EXCLUDED BLOCK LEAVES NO TRACE ON THE PAGE -- no heading, no directive,
        // nothing to grep -- so a later item can restore one and every other guard in
        // this file stays green. An EXACT SET, not a handful of DoesNotContain checks:
        // a block added in two years fails here and has to be ruled on deliberately.
        Assert.Equal(
            ["caregiver", "causes", "crosswalk", "escalation", "posterior-fossa-syndrome", "spinal-cord"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        // Each included block sits in the section that gives it its meaning. POSITION is
        // the property (WI-512): a directive that drifts into another section still
        // composes, and still reads as unrelated to the thing it qualifies.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[SPINAL-CORD]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[POSTERIOR-FOSSA-SYNDROME]", Section(AfterHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheMechanismBlockIsExcludedBecauseABabysSkullCanStillGive()
    {
        // THIS ITEM'S SHARPEST RULING, and the one a later editor is likeliest to
        // "correct", because [MECHANISM] is included on nearly every other hub and its
        // swelling, pressure and blocked-fluid material is exactly this tumor's story.
        //
        // IT IS EXCLUDED BECAUSE ITS CENTRAL SENTENCE IS THE OPPOSITE OF THIS PAGE'S
        // CENTRAL EXPLANATORY FACT. The block opens "The skull is a closed box. It cannot
        // stretch." That is FALSE for an infant, and this page's cardinal sign is a head
        // that IS stretching. The single most useful thing this page tells a parent is
        // why the head grows and why pressure can build for longer before anything shows
        // -- precisely what the block denies. §12.10's test ("would this be true on the
        // hub you have thought about least?") fails outright.
        //
        // Asserted against the BLOCK'S OWN WORDS rather than a literal list, so moving a
        // sentence into or out of that block later turns this red instead of leaving a
        // stale copy behind (§12.11).
        var mechanism = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md"))));

        foreach (var blockOnly in new[]
                 {
                     "The skull is a closed box",
                     "It cannot stretch",
                     "the symptom tells you where, the scan tells you what",
                 })
        {
            Assert.Contains(blockOnly, mechanism, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        Assert.DoesNotContain("mechanism", ContentBlocks.DirectBlockNames(Page));

        // AND THE PAGE WRITES THE OPPOSITE ITSELF, because excluding a block without
        // replacing what it did would leave §12.3's section 3 unanswered. BOTH BULLETS
        // ARE ASSERTED, and they now point the SAME way: the warning is harder to read,
        // once because it moves into the head and once because the early signs are
        // vague.
        //
        // THE PAIRING USED TO BE A TRADE-OFF AND IS NOT ANY MORE, so the old reason for
        // it has been removed rather than left standing. It read: "A page that kept only
        // 'it buys time' would be the reassuring half on its own." That described the
        // pre-round-3 bullets, where the first half ("It delays the warning") really was
        // the reassuring one -- which is exactly why round 3 killed it as unsourced and
        // §12.12-dangerous. A reason that outlives the thing it justified is the WI-536
        // shape, and it is how the next reader restores the half that was deleted for
        // cause.
        //
        // THE FIRST HALF USED TO READ "It delays the warning", AND /review ROUND 3 WAS
        // RIGHT TO KILL IT. The page said "Your child can seem well for far longer than
        // you would expect" -- which no source carries, and which contradicts the
        // best-sourced tempo sentence on the page forty lines below ("ATRT symptoms can
        // come on quickly, in a matter of days or weeks", Boston Children's). A guard can
        // pin the SHAPE of a §12.12 pairing and still be pinning an unsourced sentence:
        // this one faithfully required both halves while the half it protected was
        // telling a parent it was reasonable to wait. What Boston actually supports is
        // the CONTRAST -- "increased head size in infants or headaches and vomiting in
        // older children" -- so the warning is different in a baby, not later.
        var where = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(WhereHeading)));
        Assert.Matches(new Regex(@"In a baby the skull can still give", RegexOptions.IgnoreCase), where);
        Assert.Matches(new Regex(
            @"bony plates have not knitted together", RegexOptions.IgnoreCase), where);
        Assert.Matches(new Regex(@"It changes the warning", RegexOptions.IgnoreCase), where);
        // THE SECOND HALF WAS UNSOURCED TOO, AND THIS GUARD WAS PINNING IT
        // (/review round 7). It read "Early signs are vague, and they look like ordinary
        // baby troubles" -- the individual signs are sourced, but the INTERPRETIVE claim
        // that they read as ordinary infant trouble is recorded nowhere. So this
        // assertion was protecting an ungrounded sentence, which is exactly what the
        // comment above says happened to the half round 3 deleted. Both halves of this
        // pairing have now been unsourced at different times, which is worth saying
        // plainly: the pairing was designed for §12.12 balance and inherited no
        // grounding from that design. Reworded to the sourced fact -- the early signs
        // ARE the ones the page lists below.
        Assert.Matches(new Regex(@"It disguises things", RegexOptions.IgnoreCase), where);
        Assert.Matches(new Regex(
            @"the ordinary ones on the list\s+below", RegexOptions.IgnoreCase), where);
        Assert.Matches(new Regex(
            @"a head that is growing too fast is taken seriously", RegexOptions.IgnoreCase), where);

        // AND THE ABSENCE PHRASING CANNOT COME BACK. This is the round-4 safety blocker
        // pinned (/review round 5), and it needed pinning because NOTHING caught it:
        // it passed ContentCheck, the full suite and THREE rendered reads.
        //
        // The bullet had read "The pressure shows up in the head itself, INSTEAD OF the
        // headaches and throwing up an older child would get" -- which asserts that a
        // baby does not throw up. This same page contradicts that three times over: the
        // symptom list carries "Throwing up, often in the morning", Cleveland's INFANT
        // pressure list names throwing up, and the fontanelle rule assumes a vomiting
        // baby. Boston's actual wording is "increased head size in infants OR headaches
        // and vomiting in older children" -- a contrast between typical presentations,
        // not an exclusion.
        //
        // BANNED AS A CLAIM SHAPE, NOT AS THE DELETED STRING, which is the lesson round 4
        // taught on the school-route ban: a ban keyed to the sentence already fixed
        // protects against nothing.
        // THE SECOND CANARY EARNED ITS KEEP IMMEDIATELY. The first version of this
        // pattern was `(?:headache|vomit|throwing up)\b`, and `\bvomit\b` cannot match
        // "vomiting" -- nor `\bheadache\b` "headaches". The canary quoting the real
        // deleted sentence passed anyway, because "throwing up" carried it, so the ban
        // LOOKED proved while being blind to the commonest inflections of the two words
        // it exists to catch. A ban that only fires on one of its three terms is the
        // vacuous-guard shape again, and only the second canary exposed it.
        var absenceForm = new Regex(
            @"\binstead of\b[^.]{0,60}\b(?:headaches?|vomit\w*|throwing up)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(absenceForm,
            "the pressure shows up in the head itself, instead of the headaches and "
            + "throwing up an older child would get");                            // canary
        Assert.Matches(absenceForm,
            "it shows in the head instead of vomiting");                          // canary
        Assert.DoesNotMatch(absenceForm, where);
    }

    [Fact]
    public void TheTumorBoardBlockIsExcludedBecauseTheRouteHereIsATrial()
    {
        // [TUMOR-BOARD] describes a MEETING. This tumor's decision framework is a TRIAL
        // PROTOCOL: "Most children with a diagnosis of ATRT will be treated as part of a
        // brain tumor clinical trial" is the best-sourced instruction on the page, and a
        // block about a meeting would compete with it for the same attention.
        var board = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "tumor-board.md"))));

        const string BoardOnly = "What they decide is advice, not an order";
        Assert.Contains(BoardOnly, board, StringComparison.Ordinal);
        Assert.DoesNotContain(BoardOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        Assert.DoesNotContain("tumor-board", ContentBlocks.DirectBlockNames(Page));

        // And the page routes where the sources actually point.
        var treatment = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(TreatmentHeading)));
        Assert.Matches(new Regex(
            @"treated as part of a brain tumor trial", RegexOptions.IgnoreCase), treatment);
        Assert.Contains("/treatments/clinical-trials", treatment, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePosteriorFossaSectionCarriesTheBlockAndClaimsNoRateForThisTumor()
    {
        // THE HEDGE, AND IT RESTS ON A VERIFIED ABSENCE. The block's own St. Jude source
        // names medulloblastoma, astrocytoma and ependymoma and DOES NOT MENTION ATRT --
        // confirmed by direct question against the live page. So this hub may carry the
        // syndrome and may NOT carry a rate, because the block's source cannot support
        // one for this tumor (§12.4 R2).
        var after = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(AfterHeading)));

        Assert.Matches(new Regex(
            @"How often it follows an operation for this particular tumor is not published",
            RegexOptions.IgnoreCase), after);
        Assert.Matches(new Regex(
            @"written for different growths in that\s+same region", RegexOptions.IgnoreCase), after);

        // NO RATE FOR THE SYNDROME, in digits or in words. WI-527: a digit ban is not a
        // frequency ban.
        //
        // THE SCOPE IN THIS COMMENT IS LOAD-BEARING AND USED TO BE WRONG. It read "NO
        // FREQUENCY FIGURE", which the regex does not implement and MUST NOT: this very
        // section opens "About half of these tumors grow in the lower rear of the brain",
        // a frequency in words sitting four lines above. That is a LOCATION share, not a
        // syndrome rate, and widening the pattern to catch "half" would fail the page for
        // a sentence §12.9 requires. The assertion was right and the comment described a
        // stricter guard than exists -- which is how the next reader "fixes" the regex
        // and breaks the page (/review round 3).
        // `\bin (?:four|three|five)\b` USED TO SIT IN THIS PATTERN AND IT WAS A TRIPWIRE
        // POINTED AT CORRECT PROSE (/review round 4): it matches "in three days" and "in
        // four weeks" as readily as a rate. Nothing in this section says either today,
        // which is the point -- the guard would have fired on a perfectly good sentence
        // somebody added later, and this file calls out that exact class elsewhere.
        // Anchored to the quantifier so it can only match a FRACTION.
        var frequency = new Regex(
            @"\d\s*%|\b\d+\s+(?:in|out of)\s+\d+\b"
            + @"|\b(?:one|two|three|1|2|3)\s+in\s+(?:three|four|five|ten|\d+)\b"
            + @"|\b(?:a third|a quarter|a fifth)\s+of\b|percent",
            RegexOptions.IgnoreCase);

        // ONE CANARY PER BRANCH, BECAUSE ONLY THE DIGIT BRANCH WAS EVER PROVED
        // (/review round 5). The single canary here was "it happens in 1 in 4 children",
        // which exercises the DIGIT branch -- so the WORDS branch, the half round 4
        // actually rewrote, had never been seen to fire. A guard whose newest half is
        // unproven is the shape this file bans everywhere else, and it is how the
        // vacuous suppression in round 3 survived.
        //
        // "half" STAYS OUT OF THE PATTERN ON PURPOSE. This section opens "About half of
        // these tumors grow in the lower rear of the brain", which is a LOCATION share
        // §12.9 requires, not a syndrome rate. See the scope note above.
        Assert.Matches(frequency, "it happens in 1 in 4 children");        // digits
        Assert.Matches(frequency, "it happens in two in three children");  // words
        Assert.Matches(frequency, "it happens in a third of children");    // fraction
        Assert.DoesNotMatch(frequency, after);

        // The subsection carries the anchor the block tests require, inside the treatment
        // section rather than the symptoms one: this is a complication of the OPERATION,
        // not a presenting sign.
        Assert.Contains("{#posterior-fossa-syndrome}", RawSection(AfterHeading), StringComparison.Ordinal);
    }

    // ------------------------------------------------------- naming, grade and report

    [Fact]
    public void TheGradeIsStatedPlainlyAndIsNotAttributedToAClassificationTableRow()
    {
        // VERIFIED TWICE, AGAINST TWO INDEPENDENT RENDERINGS: the CNS5 summary prints NO
        // grade beside AT/RT and AT/RT is absent from its grade table. But that table is
        // "CNS WHO Grades of Selected Types" and it contains NO EMBRYONAL TUMOR AT ALL --
        // not medulloblastoma either. So the honest claim is that the grade rides on a
        // FAMILY-LEVEL rule, never that a table prints it and never that WHO declined to
        // grade this tumor.
        var grade = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(GradeHeading)));

        Assert.Matches(new Regex(@"doctors grade it 4", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"Every tumor in this group is grade 4", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(
            @"rather than giving this one its own\s+number", RegexOptions.IgnoreCase), grade);

        // GRADE IS EXPLAINED IN CELL TERMS AND NEVER IN SURVIVAL TERMS (§12.3 section 2).
        Assert.Matches(new Regex(
            @"Grade describes how the cells look, not how long anyone will live",
            RegexOptions.IgnoreCase), grade);

        // AND /tumors/acoustic-neuroma's FRAMING IS DELIBERATELY NOT REUSED. That page
        // carries the genuine version of a printed-absence claim -- CNS5 gives schwannoma
        // no grade WHILE printing 1, 2 and 3 for meningioma, an asymmetry inside one
        // document. There is no such asymmetry here, so borrowing its wording would be
        // reasoning from a false parallel. The sibling is READ, not assumed (§12.10).
        var acoustic = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "acoustic-neuroma.md")));
        const string AcousticOnly = "lists this growth without\nprinting a grade beside it";
        Assert.Contains(CuratedPage.Flatten(AcousticOnly), acoustic, StringComparison.Ordinal);
        Assert.DoesNotContain(CuratedPage.Flatten(AcousticOnly), Plain, StringComparison.Ordinal);

        var borrowedFraming = new Regex(
            @"without printing a grade beside it|no grade at all, and that is an answer",
            RegexOptions.IgnoreCase);
        Assert.Matches(borrowedFraming, "The 2021 rules list it without printing a grade beside it.");
        Assert.DoesNotMatch(borrowedFraming, Plain);
    }

    [Fact]
    public void TheReportSectionSaysThereIsNoOfficialStageAndDoesNotApplyAnotherTumorsStaging()
    {
        // "There is no standard staging system for central nervous system (CNS) atypical
        // teratoid/rhabdoid tumor (AT/RT)" -- NCI PDQ, verbatim, re-read by me. So a
        // parent may meet staging WORDS without there being an official stage, and the
        // page says that rather than teaching a ladder that does not exist.
        var report = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(ReportHeading)));

        Assert.Matches(new Regex(
            @"there is no agreed staging system for ATRT", RegexOptions.IgnoreCase), report);

        // M0-M4 IS NOT APPLIED. The only source defining it is a MEDULLOBLASTOMA page,
        // and PDQ says this tumor has no standard system -- so publishing the ladder here
        // would be borrowing another tumor's staging and presenting it as this one's.
        Assert.Matches(new Regex(
            @"borrowed from another tumor, and no\s+source applies it to this one",
            RegexOptions.IgnoreCase), report);
        var mStage = new Regex(@"\bM[0-4]\b");
        Assert.Matches(mStage, "Your report may say M1.");    // canary
        Assert.DoesNotMatch(mStage, Plain);

        // THE PAGE'S OWN CROSSWALK SLICE: the retired PNET label, and the two names that
        // are NOT this tumor.
        //
        // AN EARLIER VERSION OF THIS COMMENT SAID "which no sibling owns", AND THAT WAS
        // FALSE (/review round 6). /tumors/medulloblastoma and /tumors/pediatric-brain-tumor
        // both carry the retired PNET label, and §12.9 EXPECTS every hub to carry its own
        // slice -- so the content is right and only the uniqueness claim was wrong. Same
        // WI-536 shape this file already corrected once at round 2: a reason deserves the
        // same check as a claim, and a wrong reason ships as easily as a wrong claim.
        // What is actually this page's own is the pairing of PNET with ETMR and the
        // family name, which is the slice asserted below.
        Assert.Matches(new Regex(@"This name is retired", RegexOptions.IgnoreCase), report);

        // ATT/RhT, THE 1996 ABBREVIATION, IS DELIBERATELY ABSENT and this pins the
        // decision rather than leaving it to look like an omission. It was verified --
        // but only from a machine-readable bibliographic record, because the article
        // page refuses fetches. §12.14: an unreachable citation reads like a handled
        // claim, and the fact was not load-bearing, so it was dropped rather than cited
        // to a URL no reader can open. /review round 1 flagged it as dossier-only.
        Assert.DoesNotContain("ATT/RhT", Plain, StringComparison.OrdinalIgnoreCase);

        Assert.Matches(new Regex(
            @"ETMR\*{0,2} is a different embryonal tumor, not this one", RegexOptions.IgnoreCase), report);
        Assert.Matches(new Regex(@"Rhabdoid tumor\.?\*{0,2} This is the family name",
            RegexOptions.IgnoreCase), report);
    }

    // ----------------------------------------------------------------- self-blame

    [Fact]
    public void TheSelfBlameSectionIsScopedBEFORETheBlockBecauseInheritedChangeIsNotRareHere()
    {
        // §12.10, and this block needed a scoping paragraph rather than exclusion. It
        // says "A small number of people do have an inherited condition that raises the
        // risk" -- which is TRUE on the other hubs and UNDERSTATED here: roughly one
        // child in four to one in three with a rhabdoid tumor carries a germline change.
        //
        // BOTH OF THE BLOCK'S SENTENCES SURVIVE COMPOSITION, AND AN EARLIER VERSION OF
        // THIS COMMENT CLAIMED OTHERWISE. It said "'a small number' does not survive",
        // which is false of the page a reader receives: blocks/causes.md ships that
        // phrase onto this hub unchanged, three paragraphs below the scoping. What the
        // scoping does is arrive FIRST and tell the reader the general answer runs the
        // other way -- which is why the position assertion below is the property, not
        // the phrase. A reason deserves the same check as a claim (WI-536), and this
        // reason was wrong in the direction that flattered the page.
        var section = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(CauseHeading)));

        Assert.Matches(new Regex(
            @"an inherited gene change is more common than it is for most brain\s+tumors",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"That is not most children, and it is not\s+rare either", RegexOptions.IgnoreCase), section);

        // THE REFERRAL IS FOR EVERY CHILD, NOT A SUBSET. NCI's patient page softens this
        // to "may be recommended"; the specialist consensus (AACR workshop + SIOPE, via
        // NCI's RTPS1 summary, and GeneReviews) is every child. The page follows the
        // consensus and keeps the offer-not-order framing.
        Assert.Matches(new Regex(
            @"every child diagnosed with this tumor is referred", RegexOptions.IgnoreCase), section);

        // THE SCOPE COMES BEFORE THE BLOCK, NOT AFTER IT. WI-543's first rendered read
        // found the same shape the wrong way round: the reader met five paragraphs of the
        // general answer before being told the section was written for a different
        // question. POSITION IS THE PROPERTY (WI-512), so it is asserted by position.
        var raw = RawSection(CauseHeading);
        var scoped = raw.IndexOf("runs the other way", StringComparison.Ordinal);
        var directive = raw.IndexOf("[CAUSES]", StringComparison.Ordinal);
        Assert.True(scoped >= 0, "the scoping paragraph has gone from the self-blame section");
        Assert.True(directive > scoped,
            "the self-blame block now composes ABOVE its scoping paragraph, so a parent "
            + "meets 'a small number' before being told it is more common for this tumor");

        // §12.6: never end a section on the frightening half. Checked on the COMPOSED
        // section, because read raw this section's last "sentence" is the literal token
        // "[CAUSES]" -- which is how WI-543's first version of this assertion passed for
        // a reason that had nothing to do with what a reader meets.
        //
        // /review ROUND 5 READ THIS AS A VACUOUS GUARD AND IT IS NOT. The finding was
        // that "worth saying out loud" belongs to blocks/causes.md, so "no edit to THIS
        // page can turn it red". The first half is correct -- the block composes below
        // the scoping paragraph (asserted above), so the block's closing sentence is
        // what lands. The conclusion does not follow. This binds the LAST sentence of
        // the composed section, so two ordinary edits to this page turn it red:
        // deleting the [CAUSES] directive, or appending any prose after it. Both are
        // exactly the changes that would break the landing for a reader.
        //
        // KEPT AS IS, DELIBERATELY, and the reason is written here because the next
        // reader will reach the same wrong conclusion. Re-homing this onto page-only
        // prose would assert a landing the reader never reaches, which is the defect
        // WI-543 recorded when the raw version passed on the literal token above.
        // Three guard-vacuity findings were raised across this item: the tooltip
        // suppression (true, proved by removing it and rebuilding), the school-route
        // ban (true, it matched only its own deleted string), and this one (false).
        // A vacuity claim deserves the same check as any other claim.
        var composedSection = CuratedPage.Flatten(CuratedPage.ComposedSection(Page, CauseHeading));
        Assert.Matches(new Regex(@"worth saying out loud", RegexOptions.IgnoreCase),
            CuratedPage.SentencesOf(composedSection)[^1]);

        // §12.9's demotion: the block sits BELOW "if it comes back" and ABOVE the gate.
        var order = Headings();
        Assert.True(order.IndexOf(RecurrenceHeading) < order.IndexOf(CauseHeading));
        Assert.True(order.IndexOf(CauseHeading) < order.IndexOf(OutlookHeading));
    }

    // --------------------------------------------------------------- house rules

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAndNoForbiddenNumbers()
    {
        // §12.5: concepts, never figures. The sources for this page publish survival
        // figures freely -- St. Jude, Yale, ABTA, Dana-Farber and the population paper all
        // do -- and none of it reaches the page.
        foreach (var banned in new[]
                 {
                     @"\b\d+(?:\.\d+)?\s*(?:Gy|gray)\b",
                     @"\b(?:survival|survive|live for)\b[^.]{0,40}\b\d",
                     // NO TRAILING \b AFTER `%`. WI-543's harness found that exact ban
                     // DEAD: `%` and the space after it are both non-word characters, so
                     // the boundary could never match and the guard had been incapable of
                     // firing since the day it was written.
                     @"\b\d+(?:\.\d+)?\s*(?:%|percent\b)",
                     @"\bfive[- ]year\b",
                 })
        {
            Assert.DoesNotMatch(new Regex(banned, RegexOptions.IgnoreCase), Plain);
        }

        // EVERY BAN CARRIES A CANARY, because a ban that cannot fire proves nothing.
        Assert.Matches(new Regex(@"\b\d+(?:\.\d+)?\s*(?:Gy|gray)\b"), "A dose of 54 Gy is used.");
        Assert.Matches(new Regex(@"\b\d+(?:\.\d+)?\s*(?:%|percent\b)"), "About 40% of cases.");
        Assert.Matches(new Regex(@"\b(?:survival|survive|live for)\b[^.]{0,40}\b\d",
            RegexOptions.IgnoreCase), "Median survival is 2 years.");
        Assert.Matches(new Regex(@"\bfive[- ]year\b", RegexOptions.IgnoreCase),
            "The five-year rate is published elsewhere.");

        // THE SPREAD-AT-DIAGNOSIS FACT IS EPIDEMIOLOGY, NOT PROGNOSIS, and it is carried
        // IN WORDS so the bans above cannot strip it silently.
        // "one in SEVEN", not "one in six": the source range starts at 15%, which is
        // nearer one in seven, and /review round 1 noted the lower bound was the one
        // rounded in the direction that flatters the page.
        Assert.Matches(new Regex(
            @"somewhere between about one in seven and one in three", RegexOptions.IgnoreCase), Plain);
    }

    [Fact]
    public void ThePagePublishesNoFeverNumberNoScanScheduleAndNoSurveillanceInterval()
    {
        // THREE NUMBERS THE SOURCES OFFER AND THIS PAGE REFUSES, each for a recorded
        // reason. A fever threshold: three St. Jude pages give three different framings
        // and one says "Always follow your care team's guidelines for fever", so
        // /treatments/chemotherapy owns the number and this page routes to it. A scan
        // schedule and an RTPS surveillance interval: NO fetched patient-facing source
        // carries either, and the RTPS page says only "and then regularly after that".
        // BOTH BANS WERE NARROWER THAN THEIR NAMES (/review round 7), and in the two
        // directions the sources actually write in. The fever ban was FAHRENHEIT-ONLY --
        // while St. Jude's own page prints "38.0 C" beside "100.4 F", so the number this
        // page refuses could have arrived in the other unit untouched. The interval ban
        // matched only a bare count of months, missing "every 3 to 6 months", "every few
        // months" and "twice a year", which are the commonest ways a schedule is
        // actually written. A ban that covers one spelling of the thing it forbids is
        // the same reach defect round 6 found three times.
        // WIDENED AGAIN AT /review ROUND 8, AND BOTH HOLES WERE THE COMMONEST FORMS.
        // The fever ban required a UNIT LETTER, so a bare threshold -- "call for anything
        // above 100.4", "a temperature of 38" -- walked past the ban whose entire purpose
        // is that this page publishes no fever number. The interval ban required the
        // range to be spelled " to ", so "every 3-6 months" and "every 3-6 months" with a
        // dash both missed, as did "every other month". Round 7 widened this ban and
        // still left the two ways a schedule is most often written.
        //
        // MEASURED BEFORE BEING APPLIED: both candidate patterns were run over the
        // rendered capture and matched NOTHING on the live page, and every canary below
        // was checked to fire on its own branch and no other. The live-page zero matters
        // because the fever branch keys on a word near a number, and this page carries
        // "911" a few sentences from "fever".
        var feverNumber = new Regex(
            @"\b\d{2,3}(?:\.\d)?\s*(?:°|degrees)?\s*(?:F|C)\b"
            + @"|\b(?:fever|temperature)\b[^.]{0,40}\b\d{2,3}(?:\.\d)?\b",
            RegexOptions.IgnoreCase);
        // THE RANGE NOW TAKES WORDS ON BOTH SIDES (/review round 9). It accepted only
        // DIGITS after the connector, so "every three to six months" -- the way a
        // patient-facing page would most naturally write a schedule -- did not match:
        // "three" was consumed, the optional group failed on "six" and matched empty,
        // and the unit alternation then hit "to". Round 7 widened this ban for
        // "every 3 to 6 months" and round 8 for "every 3-6 months" and "every other
        // month"; the all-words form was never covered and no canary exercised it.
        // Three widenings, and each time the branch that was missing was the one
        // nothing tested. Also added: annually, yearly, monthly, twice yearly.
        // FOURTH WIDENING, AND THE MISSING BRANCH WAS THE PLAINEST ONE (/review round 10).
        // The count token was REQUIRED after "every", so the bare unit form never matched:
        // "a brain MRI every year after that", "an ultrasound every month", "each year".
        // That is how a surveillance sentence would most naturally be written, and this
        // page's only surveillance passage ("regularly afterwards. How often is not set")
        // is exactly where such a sentence would land.
        //
        // ROUND 9'S OWN NOTE SAID "each time the branch that was missing was the one
        // nothing tested" -- and then shipped a widening with no bare-unit canary. Writing
        // the lesson down is not applying it. The count group is now OPTIONAL, "each" is
        // accepted beside "every", and the canaries below cover the bare form explicitly.
        //
        // MEASURED BEFORE APPLYING, over the composed capture AND all six blocks that
        // compose onto this page: zero matches either way, so the widening cannot be the
        // change that fails correct prose.
        // AND NARROWED AGAIN IMMEDIATELY (/review round 11), BECAUSE THE FOURTH WIDENING
        // OVERSHOT. Making the count optional turned a SURVEILLANCE-INTERVAL ban into a
        // ban on any "every/each + time unit" -- and the corpus already contains correct
        // prose of that shape. /tumors/pediatric-brain-tumor:785 says "Ask again each
        // year."; an incidence sentence would say "about twenty children each year".
        // Written here, either would have failed the suite on CORRECT text, and the
        // instinct then is to weaken the ban, which is the failure this file records
        // four times over.
        //
        // So the cadence must now sit next to a SURVEILLANCE NOUN. Measured over the
        // composed capture and all six composing blocks before applying:
        //   zero false positives, same as the wide version;
        //   all eleven real schedule forms still caught, including the three this pass
        //   adds (quarterly, hyphenated twice-yearly, "at 3-month intervals");
        //   "Ask again each year" and "about twenty children each year" NO LONGER match,
        //   where the wide version matched both.
        // Narrowing a ban is not always weakening it: this one guards the same claim and
        // stops firing on prose that never carried it.
        const string Count =
            @"(?:\d+|a|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|few|other|second)";
        const string Cadence =
            @"(?:\b(?:every|each)\s+(?:" + Count + @"(?:\s*(?:to|–|—|-)\s*" + Count + @")?\s+)?(?:week|month|year)s?"
            + @"|\b(?:twice|once)\s+(?:a|per)\s+(?:year|month)\b"
            + @"|\b(?:annually|yearly|monthly|quarterly|twice[- ]yearly)\b"
            + @"|\bat\s+\d+-month\s+intervals\b)";
        const string Surveillance =
            @"(?:MRI|scans?|ultrasound|imaging|check\w*|seen|appointment|follow-?up|audiogram|tests?|tested)";
        var interval = new Regex(
            Surveillance + @"[^.]{0,40}" + Cadence + @"|" + Cadence + @"[^.]{0,40}" + Surveillance,
            RegexOptions.IgnoreCase);

        // RUN ON THE COMPOSED PAGE, NOT THE SOURCE FILE (/review round 8). These are
        // the page's other two REFUSALS, and the fontanelle ban was widened for exactly
        // this reasoning at round 5: a threshold or a cadence arriving through a block
        // reaches the reader just as surely as one written here, and a source-file check
        // cannot see it.
        //
        // SAFE BY MEASUREMENT, NOT BY HOPE: both patterns were run over the RENDERED
        // capture -- which is the composed page -- and matched nothing, so this widening
        // cannot be the reach change that turns the suite red on a block's prose.
        var composed = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        Assert.DoesNotMatch(feverNumber, composed);
        Assert.DoesNotMatch(interval, composed);

        // AND THE FEVER RULE STAYS UNCONDITIONAL. This is the round-10 blocker pinned at
        // round 11, and it needed pinning because it was THE ONLY SAFETY REPAIR IN THIS
        // ITEM THAT GOT NO GUARD. The page had said "A fever WHILE THE COUNTS ARE DOWN
        // means telling your team right away" -- a condition a parent at home cannot
        // check, contradicting the [ESCALATION] block composed onto this very page and
        // /treatments/chemotherapy, which that sentence routes to. Re-adding it, or any
        // of its synonyms ("if the counts are low", "when your child is neutropenic"),
        // would have left the suite green at 2,345 / 2,345.
        //
        // RUN ON THE COMPOSED PAGE, because that is where the contradiction lived: the
        // block's unconditional rule and the page's conditioned one reached the same
        // reader twenty screens apart.
        var conditionalFever = new Regex(
            @"\bfever\b[^.]{0,60}\b(?:counts?|neutrophils?|neutropeni\w*)\b[^.]{0,30}\b(?:down|low)\b"
            + @"|\b(?:counts?|neutrophils?|neutropeni\w*)\b[^.]{0,30}\b(?:are|is)\s+(?:down|low)\b[^.]{0,60}\bfever\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(conditionalFever,
            "a fever while the counts are down means calling");                   // canary
        Assert.Matches(conditionalFever,
            "when the counts are low, a fever means calling your team");          // canary
        Assert.DoesNotMatch(conditionalFever, composed);

        // THE PAIRED POSITIVE, so deleting the rule cannot satisfy the ban above -- the
        // shape §12.10 requires and the one the vacuous suppression in round 3 lacked.
        Assert.Matches(new Regex(
            @"A fever during treatment means telling your\s+team right away",
            RegexOptions.IgnoreCase), composed);

        Assert.Matches(feverNumber, "Call for a fever of 100.4 F.");              // canary, F
        Assert.Matches(feverNumber, "Call for a fever of 38.0 C.");               // canary, C
        Assert.Matches(feverNumber, "call if the temperature is above 100.4");    // canary, bare
        Assert.Matches(interval, "Scans are every three months at first.");       // canary, words
        Assert.Matches(interval, "Scans are every 3 to 6 months at first.");      // canary, range
        Assert.Matches(interval, "Scans are every 3-6 months at first.");         // canary, dash
        Assert.Matches(interval, "Scans are every three to six months at first."); // canary, words
        Assert.Matches(interval, "Scans are every other month.");                 // canary, other
        Assert.Matches(interval, "Scans are twice a year after that.");           // canary, phrase
        Assert.Matches(interval, "Scans are done yearly.");                       // canary, adverb
        Assert.Matches(interval, "a brain MRI every year after that");            // canary, bare unit
        Assert.Matches(interval, "an MRI each year");                             // canary, "each"
        Assert.Matches(interval, "an ultrasound every month");                    // canary, bare unit
        Assert.Matches(interval, "seen every seven months");                      // canary, new count

        // AND THE ROUTING IS PRESENT, so a later widening of the bans above fails here
        // instead of silently stripping the page's most actionable instructions.
        // BOUND TO THE CLAIM, NOT TO THE WORDS THAT HAPPENED TO CARRY IT. The first
        // version anchored on "that number is theirs rather than one from a page", and a
        // LATER FIX TO THIS PAGE rewrote that clause to "it is theirs rather than one
        // from a page" -- so my own edit turned my own guard red. The property being
        // pinned is that the fever threshold belongs to the team rather than to us, and
        // that survives either phrasing.
        Assert.Matches(new Regex(
            @"is theirs rather than one from a page", RegexOptions.IgnoreCase), Plain);
        Assert.Contains("/treatments/chemotherapy#fever-rule", Plain, StringComparison.Ordinal);
        Assert.Matches(new Regex(
            @"No published schedule\s+exists for us to point you to", RegexOptions.IgnoreCase), Plain);
        Assert.Matches(new Regex(@"How often is not\s+set, so ask yours", RegexOptions.IgnoreCase), Plain);
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        // The corpus-wide gate caught `travelled` in this page's first draft, in a
        // sentence added by a REWORDING PASS -- and my own offline scan had reported the
        // body clean, because it ran in the same batch as the edit that introduced the
        // word and therefore read a stale file. Strip-then-scan, the stronger form
        // (§12.10).
        var reader = Plain;
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // PAGE-LOCAL, because these are idiom rather than spelling and the shared list
        // cannot carry them all. "being sick" is here for a specific reason: §12.10
        // records WI-563 shipping it in an escalation trigger, where a US reader parses
        // it as "unwell" rather than "vomiting" -- and this page's own first draft used
        // it in the fontanelle rule, which is the worst line in the corpus to be
        // ambiguous on.
        // TWO FORMS ADDED AT /review ROUND 9, AND THEY HAD WALKED PAST BOTH LISTS.
        // The shared list is SPELLINGS only; this one is idiom, and it carried neither
        // "ring" for a phone call nor "time round". The page said "a parent who RINGS
        // about nothing has still done the right thing" -- three lines under the 911
        // instruction, where a US parent reads a doorbell before a phone call -- and it
        // was the only "rings" in the entire corpus. It also said "the first time ROUND",
        // the British form of "around", while this list already banned "come round" and
        // missed its sibling.
        foreach (var idiom in new[]
                 { "A&E", "GP", "straight away", "out of hours", "being sick", "999", "come round",
                   "time round" })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.Ordinal);
        }

        // THE PHONE-CALL "RING" IS BANNED BY REGEX, NOT BY SUBSTRING, AND THE NEGATIVE
        // CANARIES ARE THE POINT. My first attempt at this widening put "ring your" and
        // "a ring" into the plain-substring list above -- and this loop uses Ordinal
        // Contains with no word boundary, so:
        //
        //   "ring your"  is a substring of "BRING your", and "bring your questions" is
        //                exactly the prose a caregiver section wants;
        //   "a ring"     fires on jewellery, which scan-preparation material discusses;
        //   "rings"      fires on "coveRINGS", which this page's own leptomeningeal
        //                bullet carries -- correct, sourced text.
        //
        // I widened a ban to avoid the narrow-string mistake this file has made four
        // times and landed on its mirror image in the same edit. `\b` fixes all three,
        // because \brings?\b cannot match inside "bring" or "coverings" -- and the two
        // DoesNotMatch canaries below prove that rather than asserting it.
        // BARE AND PAST FORMS ADDED (/review round 10): "if in doubt, ring", "ring for
        // advice", "we rang the ward". The first version needed an object after the verb,
        // which is the same too-narrow shape this ban was widened to escape.
        var phoneRing = new Regex(
            @"\brings?\s+(?:your|the|them|us|about|for|back)\b"
            + @"|\brang\b"
            + @"|\b(?:doubt|worried|unsure),?\s+ring\b"
            + @"|\bgive\s+(?:them|us|your team|the team)\s+a\s+ring\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(phoneRing, "a parent who rings about nothing");          // canary
        Assert.Matches(phoneRing, "ring your team at any hour");                // canary
        Assert.Matches(phoneRing, "if in doubt, ring the ward");                // canary, bare
        Assert.Matches(phoneRing, "we rang the ward that night");               // canary, past
        Assert.Matches(phoneRing, "ring for advice before you travel");         // canary, "for"
        Assert.DoesNotMatch(phoneRing, "bring your notepad and a pen");         // negative
        Assert.DoesNotMatch(phoneRing, "the thin coverings around the brain");  // negative
        Assert.DoesNotMatch(phoneRing, "arranged in the right order");          // negative, "rang"
        Assert.DoesNotMatch(phoneRing, reader);
    }

    [Fact]
    public void TheSeventeenSectionsAreAllPresentAndInOrder()
    {
        // §12.3's order, asserted as an ORDER rather than a set: a page can carry every
        // heading and still meet the reader with outlook before treatment.
        string[] expected =
        [
            ShortHeading, WhatHeading, GradeHeading, WhereHeading, SymptomHeading,
            DiagnosisHeading, ReportHeading, TreatmentHeading, AfterHeading, LifeHeading,
            ScansHeading, RecurrenceHeading, CauseHeading, OutlookHeading, CareHeading,
            QuestionsHeading, SupportHeading,
        ];

        Assert.Equal(expected, Headings());

        // SLOT 9 IS ADAPTED, AND THE ADAPTATION IS THE POINT. §12.3 words it "driving,
        // work, money, seizures" and every adult hub follows that. This page's reader is
        // the parent of a child under 3, so the page says out loud that it is NOT about
        // those things rather than silently omitting them -- an omission a reader cannot
        // tell from an oversight.
        // SCOPED TO THE SECTION, NOT THE PAGE, AND THE RENDERED READ IS WHY. The first
        // wording was "Nothing on this PAGE is about driving, work or money" -- and the
        // composed [CAREGIVER] block goes on to discuss driving, in the seizure routing it
        // carries on every hub. The claim was false of the page a reader actually receives,
        // while being true of the section it sits in. Source review cannot see that.
        var life = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(LifeHeading)));
        Assert.Matches(new Regex(
            @"Nothing in this section is about driving, work or money", RegexOptions.IgnoreCase), life);

        // And no actual driving or work GUIDANCE is given. Banning the WORDS would fire
        // on the page's own correct sentence above, which is the mistake this corpus
        // keeps re-learning -- so the ban is on a RULE, not on a token.
        var drivingRule = new Regex(
            @"\b(?:you|they)\b[^.]{0,40}\b(?:must not|cannot|should not)\b[^.]{0,30}\bdriv\w+"
            + @"|\bstop driving\b|\bdriving ban\b|\bback to work\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(drivingRule, "You must not drive for six months.");   // canary
        Assert.DoesNotMatch(drivingRule, Plain);
    }

    [Fact]
    public void TheOutlookGateIsClosedAndCarriesNoFigureAndNoFigureInWords()
    {
        // §12.5. EVERY LINE-BREAK PATTERN IS `\r?\n`: this repo hands working trees CRLF
        // while CI sees LF, and a test that cannot match on CRLF fails for free -- the
        // break harness cannot see that class of bug.
        var raw = RawSection(OutlookHeading);

        Assert.Matches(new Regex(@"\A[ \t]*\r?\n[ \t]*:::outlook"), raw);
        Assert.Matches(new Regex(@"\r?\n:::\r?\n?\s*\z"), raw);

        var inside = CuratedPage.Flatten(
            Regex.Match(raw, @":::outlook(.*?)\r?\n:::", RegexOptions.Singleline).Groups[1].Value);
        Assert.False(string.IsNullOrWhiteSpace(inside), "the gate is empty");

        // Nothing sits outside the gate but the heading. The §12.5 warning is rendered BY
        // the container, so writing it by hand would show it to the reader twice.
        var outside = raw.Replace(
            Regex.Match(raw, @":::outlook.*?\r?\n:::", RegexOptions.Singleline).Value, "",
            StringComparison.Ordinal);
        Assert.True(string.IsNullOrWhiteSpace(outside),
            "something sits outside the outlook gate: " + outside.Trim());

        // NO DIGITS, and no figure written as words either (WI-527: a digit ban is not a
        // prognosis ban).
        Assert.DoesNotMatch(new Regex(@"\d"), inside);

        // BOUND TO THE CLAIM, NOT TO THE TOKEN, AND THE FIRST VERSION WAS NOT.
        // It banned the bare phrase "half of" and fired on this gate's own required
        // sentence -- "Half of that crowd is above it, and the upper half has no
        // ceiling" -- which is §12.9's Kirkebøen teaching: name the distribution and
        // name the right tail. A ban that fails the correct case is worse than no ban,
        // and WI-513 shipped a draft whose own test BANNED the word "median", which
        // would have cemented a gate explaining nothing across 23 copies.
        //
        // What is actually forbidden is a SURVIVAL FIGURE written in words (§12.5,
        // WI-527: a digit ban is not a prognosis ban), so the quantifier now has to be
        // paired with an outcome before it counts.
        var figureInWords = new Regex(
            @"\b(?:half|a third|a quarter|two thirds|most)\b[^.]{0,40}"
            + @"\b(?:survive\w*|live|lived|living|alive|die|died|make it)\b"
            + @"|\b(?:survive\w*|live|lived|alive|die|died)\b[^.]{0,40}"
            + @"\b(?:half|a third|a quarter|two thirds)\b"
            + @"|\byears to live\b",
            RegexOptions.IgnoreCase);

        // PROVED ABLE TO FIRE, IN BOTH WORD ORDERS. A quantifier can sit on either side
        // of the outcome, and this file's ancestor had to learn that three times.
        Assert.Matches(figureInWords, "About half of children survive five years.");
        Assert.Matches(figureInWords, "A third of them are still alive.");
        Assert.Matches(figureInWords, "Most children live for years.");
        Assert.Matches(figureInWords, "Children who survive are about a third.");
        Assert.Matches(figureInWords, "Some families are told how many years to live.");

        // AND THE GATE'S OWN REQUIRED SENTENCES MUST SURVIVE IT.
        Assert.DoesNotMatch(figureInWords,
            "Half of that crowd is above it, and the upper half has no ceiling.");
        Assert.DoesNotMatch(figureInWords, inside);

        // THE GATE'S SPINE IS A SOURCED REFUSAL, not policy compliance alone. Cleveland
        // Clinic says in as many words that the data does not support a rate, which is a
        // far better reason to publish none.
        Assert.Matches(new Regex(
            @"there is not enough information to\s+give a survival rate or a cure rate",
            RegexOptions.IgnoreCase), inside);

        // AND §12.9's four Kirkebøen moves survive without any figure: it describes a
        // GROUP, it has a DISTRIBUTION, the upper tail is named, and it is not a
        // prediction. WI-513 shipped a draft doing one of the four and BANNING the word
        // "median", which would have cemented a gate that explains nothing.
        Assert.Matches(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"a fact about a crowd", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"the upper half has no ceiling", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"cannot forecast one child", RegexOptions.IgnoreCase), inside);
    }

    // ------------------------------------------------------- the shared corpus guards

    [Fact]
    public void ThePageCarriesTheEscalationBlockAndItsTiersAgreeWithTheCorpus() =>
        // Composed and section-scoped. Moving [ESCALATION] out of the symptoms section
        // would keep the suite green without this (WI-512: presence was never the
        // property, position was).
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    // `AssertNoEscalationList` IS DELIBERATELY NOT CALLED, AND THAT IS A RULING RATHER
    // THAN AN OMISSION. Its own docstring scopes it to "the pages whose reader has
    // nothing to escalate -- a plain head CT, an extra MRI sequence", where the ABSENCE
    // of a tier list is the right answer. This page is the opposite case: §12.10
    // (WI-563) says in as many words to add the page's own line beneath the block when
    // the hub's emergency is not in it, and a bulging fontanelle is not in it.
    //
    // /review round 1 showed it was passing for a reason unrelated to its property. The
    // page DOES have an urgency lead-in followed by bullets; it escapes only because no
    // urgency token happens to fall inside the regex's 40-character window. So it proved
    // nothing here, and worse, it was a tripwire pointed at correct prose: rewording the
    // lead-in to "Three things mean getting help right away" -- an improvement -- would
    // have turned the suite red.
    //
    // What guards the real risk instead is the lead-in count assertion in
    // TheEmergencyRulesAreAttributedToTheSignsAndNotToTheTumor, which pins the page's
    // own tier to a stated shape, and AssertEscalationTiers, which keeps the shared
    // block's tiers in step with the corpus.

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnTheComposedPage() =>
        // PARAGRAPH-scoped and COMPOSED, and the named paragraph is the POSITIVE half: an
        // iterate-and-check guard is green on a page it never looked at (WI-517).
        //
        // The named paragraph is the posterior fossa block's recovery paragraph, which is
        // the one place on this composed page where a real warning sign (speech, walking,
        // mood, swallowing) sits beside a reassurance ("Most children slowly get better")
        // -- and it carries its own tier, which is what makes this bind.
        CuratedPage.AssertNoWarningSignIsNormalised(Composed, Slug,
            "Most children slowly get better");

    [Fact]
    public void TheEverydayClaimsStayInsideWhatTheirSourcesActuallySay()
    {
        // ADDED AT /review ROUND 5, FOR FOUR CLAIMS THAT NOTHING PINNED. Three of them
        // had already been wrong once, and one of them twice:
        //
        //   central line  -> "will have" (round 3) -> "will PROBABLY have" (round 4,
        //                    still a frequency claim with no source) -> the conditional
        //                    the source actually gives. Wrong twice, pinned never.
        //   the scan      -> "Small children USUALLY need medicine to sleep for a scan",
        //                    which pointed the opposite way from the source AND from the
        //                    page it routes the reader to for that exact topic.
        //   stem cells    -> grounded only in the research pack (§12.8: a dossier cannot
        //                    support a published claim).
        //   feeding tube  -> "common" survived re-grounding, because it is the source's
        //                    own word -- the one of the four that held.
        //
        // WHY A GUARD AND NOT A CAREFUL EDIT. Every one of these passed ContentCheck,
        // the full suite and three rendered reads. The suite cannot see a claim drifting
        // away from its source; only a pin can.
        var diagnosis = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(DiagnosisHeading)));
        var treatment = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(TreatmentHeading)));
        var after = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(AfterHeading)));

        // THE SCAN, BOTH HALVES. St. Jude: "In some cases, your child may get sedation
        // medicines or general anesthesia", and "Many children can have diagnostic
        // imaging tests without anesthesia or sedation medicines." The counterweight is
        // what makes this honest, so it is asserted rather than assumed.
        Assert.Matches(new Regex(
            @"some children are given medicine to\s+help them sleep", RegexOptions.IgnoreCase),
            diagnosis);
        Assert.Matches(new Regex(@"Many children manage without", RegexOptions.IgnoreCase),
            diagnosis);

        // AND THE DIRECTION CANNOT FLIP BACK. Keyed to the CLAIM SHAPE, not to the
        // sentence round 5 deleted -- the lesson from the school-route ban, which was
        // keyed to its own deleted string and so protected nothing.
        var scanFrequency = new Regex(
            @"\b(?:usually|normally|always|most)\b[^.]{0,40}"
            + @"\b(?:need|needs|given)\b[^.]{0,40}\b(?:medicine|sedation|anaesthetic|anesthetic)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(scanFrequency,
            "Small children usually need medicine to sleep for a scan");          // canary

        // RUN ON THE WHOLE PAGE, NOT ON ONE SECTION (/review round 6). All three bans in
        // this guard were section-scoped, which is the REACH defect round 5 fixed on the
        // fontanelle ban and then reproduced here in the same sitting. A ban that is
        // right about its rule and wrong about its reach guards less page than its
        // comment implies, and the caregiver section discusses high-dose treatment with
        // stem cells -- so the deleted radiation rationale could have been re-added
        // there with the suite green.
        Assert.DoesNotMatch(scanFrequency, Plain);

        // AND NOW THE WHOLE DIAGNOSIS LIST, BECAUSE THE CLASS KEPT OUTLIVING THE FIX.
        // Round 4 blocked "will probably have a central line". Round 5 blocked "Small
        // children USUALLY need medicine to sleep for a scan". Round 6 found the SAME
        // SHAPE one and two bullets away: "so the kidney is OFTEN checked too" (the
        // source says "may also be used") and "the tissue that names the tumor ALMOST
        // ALWAYS comes from surgery" (no source at all).
        //
        // The page's front matter had already recorded, in round 5, that "fixing the
        // flagged instance and not the class is its own recurring failure" -- and the
        // very guard above then scoped itself to one bullet's wording. Writing the lesson
        // down is not applying it. So this ban is keyed to the CLASS: a frequency adverb
        // sitting next to a diagnostic step, in either order.
        //
        // SECTION-SCOPED ON PURPOSE, AND THIS IS NOT THE REACH MISTAKE ABOVE. Widening it
        // to `Plain` would fire on the questions list, which legitimately asks "Does my
        // child need a kidney ultrasound, and how often?" -- a QUESTION carries no claim.
        // Wider is not automatically better; the fontanelle ban needed the whole page and
        // this one must not have it.
        // THE NOUNS ARE PLURALISED (/review round 7). They were singular-only, so
        // `\bscan\b` could not match "Gene tests on the tumor tissue" -- a line this very
        // bullet list contains -- and "Scans are usually needed first" would have walked
        // straight past the ban written for that list. Both canaries used singulars, so
        // the hole was invisible to the thing meant to expose it.
        var diagnosisFrequency = new Regex(
            @"\b(?:often|usually|almost always|always|normally|most often)\b[^.]{0,60}"
            + @"\b(?:kidney|surger(?:y|ies)|biops(?:y|ies)|scans?|ultrasounds?|spinal fluid|gene tests?)\b"
            + @"|\b(?:kidney|surger(?:y|ies)|biops(?:y|ies)|scans?|ultrasounds?|spinal fluid|gene tests?)\b"
            + @"[^.]{0,60}\b(?:often|usually|almost always|always|normally|most often)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(diagnosisFrequency, "so the kidney is often checked too");   // canary
        Assert.Matches(diagnosisFrequency,
            "The tissue that names the tumor almost always comes from surgery");    // canary
        Assert.Matches(diagnosisFrequency,
            "Gene tests are usually done on the tissue");                           // canary, plural

        // STILL SECTION-SCOPED, AND NOW FOR A MEASURED REASON RATHER THAN A GUESSED ONE.
        // Round 7 proposed running this on the whole page minus the questions list,
        // having checked that nothing else matched. I ran the candidate pattern over the
        // RENDERED CAPTURE instead of reasoning about it, and it fires on THREE
        // legitimate, sourced sentences outside this section:
        //
        //     "usually spreads through the spinal fluid"    (St. Jude, recorded verbatim)
        //     "often put straight into the spinal fluid"    (Boston, intrathecal chemo)
        //     "most often the kidney"                       (RTPS predisposition)
        //
        // All three are correct prose this page is supposed to carry. Widening would have
        // turned the suite red on grounded claims and invited somebody to weaken the ban
        // to fix it. A frequency adverb beside a procedure is a DEFECT in the diagnosis
        // checklist and ordinary English everywhere else, so the scope is the claim, not
        // a compromise. Count, do not reason: the same rule that rejected six smoke
        // negatives.
        // ONE SENTENCE IN THIS SECTION ESCAPES ONLY BY PUNCTUATION, AND THE NEXT PERSON
        // SHOULD KNOW BEFORE THEY "TIDY" IT. The lumbar-puncture bullet reads "A test of
        // the spinal fluid. A small amount of fluid is taken, usually from the lower
        // back" -- `[^.]` cannot cross the sentence break, so "spinal fluid" and
        // "usually" never meet. Rephrase it as "The spinal fluid is usually taken from
        // the lower back" and this guard goes red on ACCURATE prose.
        // THE ANSWER THEN IS TO REWORD THE PAGE, NOT TO WEAKEN THE BAN. The claim being
        // guarded is a frequency adverb attached to a diagnostic step; "usually from the
        // lower back" is a claim about WHERE, which the sources do support.
        Assert.DoesNotMatch(diagnosisFrequency, diagnosis);

        // THE CENTRAL LINE, held to the source's CONDITIONAL.
        Assert.Matches(new Regex(
            @"Your child may have a central line", RegexOptions.IgnoreCase), after);
        Assert.Matches(new Regex(
            @"seriously\s+ill and need treatment for a long time", RegexOptions.IgnoreCase),
            after);

        var lineFrequency = new Regex(
            @"\b(?:will|usually|probably|most children)\b[^.]{0,40}\bcentral line\b"
            + @"|\bhundreds of needles\b", RegexOptions.IgnoreCase);
        Assert.Matches(lineFrequency, "Your child will probably have a central line");  // canary
        Assert.Matches(lineFrequency, "it saves hundreds of needles");                  // canary
        Assert.DoesNotMatch(lineFrequency, Plain);

        // THE FEEDING TUBE, in the source's own word.
        Assert.Matches(new Regex(
            @"a feeding tube is a common procedure in\s+children with cancer",
            RegexOptions.IgnoreCase), after);

        // THE STEM CELLS. "their own" is the whole point of an autologous transplant and
        // is what neither Boston's nor Colorado's recorded verbatim supports.
        Assert.Matches(new Regex(
            @"their own stem cells given back", RegexOptions.IgnoreCase), treatment);
        Assert.Matches(new Regex(
            @"own blood-forming stem\s+cells first, and they are frozen for later",
            RegexOptions.IgnoreCase), treatment);

        // AND THE RATIONALE STAYS DELETED. The autologous source was queried directly and
        // gives NO reason connecting this treatment to avoiding radiation; the only
        // support was a pack entry from a page that publishes survival figures. Buying a
        // citation to license a RATIONALE is what round 3 deleted the proton reason for.
        var radiationRationale = new Regex(
            @"\b(?:so|because)\b[^.]{0,60}\bwithout using radiation\b"
            + @"|\btreat hard without\b", RegexOptions.IgnoreCase);
        Assert.Matches(radiationRationale,
            "Teams do this partly so they can treat hard without using radiation on a "
            + "very young brain");                                                // canary
        Assert.DoesNotMatch(radiationRationale, Plain);
    }

    [Fact]
    public void ThePediatricHubLinksBackToThisPage()
    {
        // THE DOOR IS PINNED FROM BOTH SIDES, AND IT WAS NOT (/review round 2). This
        // page links TO /tumors/pediatric-brain-tumor three times and that hub now links
        // back -- but nothing asserted the return door, so deleting it left the whole
        // suite green. That is the WI-412 shape ("a page nothing linked to") which round
        // 1's index fix closed on one side only.
        //
        // The SIBLING IS READ rather than assumed (§12.10), and the assertion lives in
        // THIS item's file because this is the item that depends on the door existing.
        var hub = CuratedPage.Read("tumors", "pediatric-brain-tumor.md");
        Assert.Contains("/tumors/atrt", hub, StringComparison.Ordinal);

        // And it is in the READER's text, not merely in a front-matter comment, which is
        // the trap WI-537 recorded: a URL in a comment makes a check pass for the wrong
        // reason.
        Assert.Contains("/tumors/atrt", CuratedPage.ReaderText(hub), StringComparison.Ordinal);

        // ------------------------------------------------------------------------
        // AND NOW THE CLAIM BEHIND THE DOOR, WHICH IS WHAT /review ROUND 3 BLOCKED ON.
        //
        // Pinning the LINK is not pinning the PROMISE. This page told a parent the hub
        // set out "the two school routes and which one covers a child under 3" -- and
        // the hub says the opposite: an IEP covers ages 3 through 21, a 504 plan is a
        // different law, and "Babies and toddlers under 3 come under a different part,
        // for early intervention." NEITHER ROUTE COVERS AN UNDER-3. The sentence posed a
        // question with a false premise, on the one action this page calls the most
        // useful thing on it, to an audience defined by being under 3.
        //
        // Round 1 found an UNCITED early-intervention claim; the fix replaced the claim
        // with a route; the route was wrong. That is §12.14 -- routing a reader
        // elsewhere is not the same as grounding a claim -- and it is the third time in
        // this item that a fix produced the next defect.
        //
        // So the destination is READ, and the two things this page promises are behind
        // it are asserted THERE. Rewrite the hub's school section and this goes red on
        // THIS page, which is the only place the promise is made.
        var hubText = CuratedPage.Flatten(CuratedPage.ReaderText(hub));

        // THE FAILURE MESSAGE NAMES THE REAL CAUSE. A bare Assert.Contains here fails
        // with "not found in [50 KB of hub text]" inside a test named
        // ThePediatricHubLinksBackToThisPage -- so a reworded hub school section would
        // read to the next person as a broken LINK, and they would go looking in the
        // wrong file (/review round 4).
        const string Misread =
            "the pediatric hub's school section was reworded, so /tumors/atrt's promise "
            + "about what is behind that link is now unverified -- re-read "
            + "pages/tumors/pediatric-brain-tumor.md and fix the sentence in this page's "
            + "'Everyday life' section";

        Assert.True(hubText.Contains("In the United States there are two routes", StringComparison.Ordinal),
            Misread);
        Assert.True(Regex.IsMatch(hubText,
            @"under 3 come under a different part, for early intervention",
            RegexOptions.IgnoreCase), Misread);

        // AND THE FALSE PREMISE CANNOT COME BACK. The page must not claim one of the two
        // school routes covers a child under 3.
        //
        // THE PROPERTY IS THE CLAIM SHAPE, NOT THE DELETED STRING. The first version of
        // this ban matched only "which one covers a child under 3" -- the exact sentence
        // round 3 removed -- so "which route an under-3 falls under", or naming the IEP
        // directly, would have walked straight past it. A ban keyed to the wording that
        // was already fixed protects against nothing (/review round 4). Canaried like
        // every other ban in this file, because a ban nobody proved can fire is a ban
        // nobody has tested.
        var life = CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(LifeHeading)));
        var falsePremise = new Regex(
            @"(?:which|what)\s+(?:one|route)[^.]{0,40}\bunder 3\b"
            + @"|\b(?:IEP|504)\b[^.]{0,40}\bunder 3\b", RegexOptions.IgnoreCase);
        Assert.Matches(falsePremise,
            "the two school routes and which one covers a child under 3");   // canary
        Assert.Matches(falsePremise,
            "an IEP is the one that covers a child under 3");                // canary
        Assert.DoesNotMatch(falsePremise, life);

        // The paired positive, so a rewrite that simply deletes the routing sentence
        // cannot pass: what this page says about the destination must still be there.
        // "THE SAME LAW" HAD NO ANTECEDENT ON THIS PAGE (/review round 5). This page
        // never names IDEA, and the two routes it mentions are under DIFFERENT laws --
        // so "a different part of the same law" pointed at nothing a reader could
        // resolve. The hub is clear; the borrowed summary was not. Now "the law behind
        // one of them", which is true and resolvable without naming the statute.
        Assert.Matches(new Regex(
            @"different part of the\s+law behind one of them, for early intervention",
            RegexOptions.IgnoreCase), life);
    }

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        // TWO SHARED GUARDS EVERY RECENT HUB CALLS, AND THIS FILE CALLED NEITHER
        // (/review round 1). /tumors/spinal-cord-tumor omits them too, so this repeats
        // that item's gap rather than inventing one -- which is exactly how a gap
        // becomes a convention.
        //
        // THE TWO CALLS DO DIFFERENT AMOUNTS OF WORK HERE, AND THE FIRST VERSION OF THIS
        // COMMENT CLAIMED OTHERWISE (/review round 2). It justified both by pointing at
        // sentences like "It is not a defeat" and "that is ordinary" -- but
        // AssertNeverMinimises only ever matches the Minimisations list
        // (routine/simple/minor/straightforward/easy/harmless procedure, "no big deal",
        // "piece of cake"), and not one of those sentences can match it. On today's
        // composed page that call examines an EMPTY match set.
        //
        // It is kept anyway, as a STANDING guard: this is a page about an operation on a
        // baby, and "a simple procedure" is exactly the reassurance a later edit reaches
        // for. The Characterisations ban is the one that binds today -- "aggressive" is
        // on that list, every ATRT source in the set uses it freely, and this page
        // deliberately does not.
        //
        // WI-536: a reason deserves the same check as a claim, and this reason was wrong
        // inside the fix for round 1's own finding.
        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Composed), Slug);

        // Run on the COMPOSED page: a hype phrase arriving through a block reaches the
        // reader exactly as one typed here would (§12.10).
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST, DELIBERATELY. The first draft collided with fourteen files -- 44
        // windows against /tumors/medulloblastoma alone, plus a corpus-wide outlook-gate
        // template that nine hubs share -- and EVERY ONE was reworded rather than
        // exempted. WI-543 records why that matters: an allowlist entry that excuses
        // collisions you do not have is worse than none, because it hides the ones you
        // do. Three of the five rewriting passes CREATED new collisions, which is §12.8's
        // "a fix is where the next defect comes from" arriving three times in one item.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    // -------------------------------------------------------------- the front matter

    [Fact]
    public void TheSourcesAreRealAndTheBarredOnesAreNotCited()
    {
        var front = CuratedPage.FrontMatter(Page);

        // mayoclinic.org is barred corpus-wide, and PLAN.md §5 bars AHFS and MedlinePlus
        // DRUG MONOGRAPHS.
        //
        // THE MEDLINEPLUS BAR IS ON THE PATH, NOT THE DOMAIN, and getting that wrong
        // would have cost this page its second fontanelle source. The corpus bans
        // `medlineplus.gov/druginfo` specifically; /tests/getting-ready-for-surgery
        // REQUIRES a `medlineplus.gov/lab-tests/` citation, and /treatments/shunts and
        // /tumors/craniopharyngioma both cite the domain. This page cites an encyclopedia
        // article on bulging fontanelles, which is not a drug monograph.
        foreach (var barred in new[] { "mayoclinic.org", "ahfs", "medlineplus.gov/druginfo" })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Contains("medlineplus.gov/ency", front, StringComparison.OrdinalIgnoreCase);

        // NO BRACKETED BLOCK NAME ANYWHERE IN FRONT MATTER. ContentBlocks.Directives walks
        // every line outside a fenced code block, front matter included, so a bracketed
        // name in a comment registers as a real directive -- which would break the
        // exact-set ruling above and CaregiverSectionTests' raw IndexOf.
        Assert.DoesNotMatch(new Regex(@"^\s*\[[A-Z0-9][A-Z0-9-]*\]\s*$", RegexOptions.Multiline), front);

        // THE TWO SOURCE HAZARDS ARE NAMED WHERE THE NEXT READER WILL SEE THEM. Nationwide
        // prints "SMARCB4", which is not a gene, so it must never be the citation for a
        // gene name; and the 911 venue rests on ONE source, which the front matter records
        // rather than implying two sources say it.
        Assert.Matches(new Regex(@"SMARCB4", RegexOptions.IgnoreCase), front);
        Assert.Matches(new Regex(@"NAMES NO VENUE", RegexOptions.IgnoreCase), front);

        // Contract item 8.
        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class AtrtPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/atrt";

    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// The connection string is pushed in, exactly as every other render fixture does it.
    /// Taking the factory as-is works on a developer machine because user-secrets supplies
    /// one, and fails on CI where nothing does -- a fixture that only works where the
    /// secrets are is not a test.
    /// </summary>
    public AtrtPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    private HttpClient Client => _factory.CreateClient();

    [Fact]
    public async Task ThePageIsServedAtItsUrl()
    {
        var response = await Client.GetAsync(Url);
        Assert.True(response.IsSuccessStatusCode, $"{Url} returned {(int)response.StatusCode}");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("ATRT", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The short version", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSixBlocksComposeAndTheTwoExcludedOnesLeaveNoTrace()
    {
        // THE HALF THE CONTENT-SIDE GUARDS STRUCTURALLY CANNOT SEE. They read directive
        // NAMES out of the source file, so they stay green if composition itself breaks.
        // Only a rendered assertion catches that, and only from both directions.
        //
        // EVERY STRING IS TOOLTIP-FREE. A glossary popover injects TEXT into the middle of
        // a sentence, so a contiguous match fails in HTML even when the block composed
        // perfectly -- WI-543 lost a round to "A tumor board is a meeting" for exactly
        // that reason.
        var html = await Client.GetStringAsync(Url);

        foreach (var composed in new[]
                 {
                     "Call an ambulance",                          // ESCALATION
                     "You are allowed to ask questions",           // CAREGIVER
                     "nobody knows the cause",                     // CAUSES
                     // CROSSWALK, and it must be a string ONLY that block says. "changed
                     // in 2021" was the first choice and this page's own grade section
                     // contains the same idea, so it would have passed with the block
                     // emptied (§12.10).
                     "NOS means the tests needed to be more exact were not available",
                     "right-away call, not a same-day one",        // SPINAL-CORD
                     "Most children slowly get better",            // POSTERIOR-FOSSA-SYNDROME
                 })
        {
            Assert.Contains(composed, html, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var excluded in new[]
                 {
                     "The skull is a closed box",                  // MECHANISM
                     "What they decide is advice, not an order",   // TUMOR-BOARD
                 })
        {
            Assert.DoesNotContain(excluded, html, StringComparison.OrdinalIgnoreCase);
        }

        // And no directive survives into the HTML unresolved.
        foreach (var marker in new[] { "[MECHANISM]", "[ESCALATION]", "[CAUSES]", "[CAREGIVER]",
                                       "[CROSSWALK]", "[TUMOR-BOARD]", "[SPINAL-CORD]",
                                       "[POSTERIOR-FOSSA-SYNDROME]", ":::" })
        {
            Assert.DoesNotContain(marker, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        var html = await Client.GetStringAsync(Url);

        // SCOPED to the outlook section: matching the first <details> in the document
        // reads whatever disclosure the layout puts above the article.
        var start = html.IndexOf("What might happen over time", StringComparison.OrdinalIgnoreCase);
        Assert.True(start > 0, "the outlook heading did not render");

        var gate = Regex.Match(html[start..], @"<details[^>]*>");
        Assert.True(gate.Success, "the outlook gate did not render as a details element");
        Assert.DoesNotContain(" open", gate.Value, StringComparison.Ordinal);

        Assert.Equal(1, Regex.Matches(html, "The next part is about outlook").Count);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(Client, Url, "where-to-get-support",
            "/get-help-now", "/tumors/pediatric-brain-tumor", "/tumors/medulloblastoma");

    [Fact]
    public async Task EveryDeepLinkedAnchorExists() =>
        // AssertLinksResolve cannot see fragments: its regex stops at the `#`, so a link
        // to an anchor that does not exist resolves as a healthy 200 and lands the reader
        // at the top of a long page (§12.8, WI-508). This page deep-links the fever rule,
        // which is the number it deliberately does not publish.
        await CuratedPage.AssertFragmentLinksResolve(Client, Url);

    [Fact]
    public async Task TheSuppressedTooltipsStayQuietAndTheUnsuppressedOneStillFires()
    {
        // TWO SUPPRESSIONS, NOT THREE, AND THE THIRD WAS REMOVED FROM THE PAGE BECAUSE
        // IT SUPPRESSED NOTHING (/review round 3).
        //
        // Every sibling that suppresses a tooltip pins the result in a render test, and
        // for a reason: the page glosses each of these terms INLINE in the next clause,
        // so an unsuppressed tooltip prints the definition directly above the definition
        // (§12.8, WI-535). Remove a marker and nothing went red.
        //
        // THE PAGE ALSO CARRIED `!%posterior fossa%`, AND THIS GUARD WAS GREEN ON IT IN
        // BOTH STATES -- which is the WI-542 shape (a guard green on the defect it
        // forbids) hiding inside the fix for it. "posterior fossa" occurs exactly once
        // in anything this page renders, inside "posterior fossa syndrome" at the
        // subsection heading; BuildMatchers orders longest-first and FindEarliestMatch
        // breaks an index tie in favour of the longer name, so the SYNDROME term wins
        // that position with or without the marker.
        //
        // PROVED BY REMOVING IT AND REBUILDING, NOT BY READING THE MATCHER. The
        // candidate objection was real: `tooltippedSlugs` retires a slug after its first
        // use, and blocks/posterior-fossa-syndrome.md contributes a source title
        // ("Posterior Fossa Syndrome - Together by St. Jude") that DOES render, so the
        // bare term might have fired there unopposed. It does not -- marker deleted,
        // rebuilt, this test still passed, and the definition stayed absent from the
        // HTML, which it could not have done had any tooltip fired. So the marker was
        // noise and is gone (§12.11, WI-512/WI-514: a no-op marker reads as a decision
        // somebody made).
        //
        // BOTH DIRECTIONS, AND THE POSITIVE HALF IS WHAT MAKES IT BIND. Each definition
        // is proved to EXIST in its glossary file before it is proved ABSENT from the
        // page -- otherwise rewording a glossary entry silently retires the guard,
        // because the words would simply have gone (§12.10, WI-514).
        var html = await Client.GetStringAsync(Url);
        var glossary = Path.Combine(CuratedPage.BlocksRoot, "..", "glossary");

        foreach (var (file, definition) in new[]
                 {
                     ("embryonal-tumor", "A tumor that grows from very early forms of nerve cells"),
                     ("hydrocephalus", "Fluid building up inside the brain because it cannot drain"),
                 })
        {
            var entry = CuratedPage.Flatten(File.ReadAllText(Path.Combine(glossary, file + ".md")));
            Assert.Contains(definition, entry, StringComparison.Ordinal);
            Assert.DoesNotContain(definition, CuratedPage.Flatten(html), StringComparison.Ordinal);
        }

        // THE PAIRED POSITIVE. Suppression is page-wide (GlossaryMarker collects every
        // !%...% in the document), so a test that only checked absences would pass just
        // as happily if the glossary stopped firing altogether. "posterior fossa
        // syndrome" is a DISTINCT term from the suppressed "posterior fossa", and it
        // must still reach the reader.
        var syndrome = CuratedPage.Flatten(File.ReadAllText(
            Path.Combine(glossary, "posterior-fossa-syndrome.md")));
        const string SyndromeDefinition = "A change that can follow surgery low at the back of the brain";
        Assert.Contains(SyndromeDefinition, syndrome, StringComparison.Ordinal);
        Assert.Contains(SyndromeDefinition, CuratedPage.Flatten(html), StringComparison.Ordinal);

        // And no authoring marker survives into the HTML.
        foreach (var marker in new[] { "!%", "%%" })
        {
            Assert.DoesNotContain(marker, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndex()
    {
        // WI-412 shipped a page nothing linked to. The listing is a property of two files
        // agreeing -- taxonomy.yml and the index -- rather than of anyone remembering.
        var html = await Client.GetStringAsync("/tumors");

        // THE HREF IS THE DOOR. The first version asserted the strings "atrt" and
        // "ATRT", neither of which pins a link: the slug appears in the anchor id and
        // the label appears as text, so both would survive the listing losing its link
        // entirely (/review round 1, and WI-412 shipped a page nothing linked to).
        Assert.Contains("/tumors/atrt", html, StringComparison.Ordinal);
        Assert.Contains("ATRT", html, StringComparison.Ordinal);
    }
}
