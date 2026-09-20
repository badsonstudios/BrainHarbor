using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-545: <c>/tumors/chordoma</c>, a NEW page rather than a deepened stub.
///
/// THIS TUMOR IS MOSTLY NOT IN THE BRAIN, and almost every ruling below follows
/// from that. It grows in BONE: skull base, mobile spine, or sacrum. Roughly a
/// third of readers have it at the tailbone end, nowhere near the head. Three
/// shared blocks written for a tumor growing INSIDE the brain are excluded for
/// that reason, and this file asserts each exclusion against the excluded
/// block's OWN WORDS rather than a literal list, so moving a sentence into or
/// out of a block later turns this red instead of leaving a stale copy behind.
///
/// THE ITEM'S CENTRE IS AN URGENCY RULING BUILT ON A SCORED ABSENCE.
/// Twenty-two chordoma pages were swept and NOT ONE gives an emergency-room or
/// 911 rule; the Chordoma Foundation's own symptoms-and-diagnosis page was
/// re-checked directly and contains no such instruction. So this page invents no
/// tumor-level emergency rule. Every emergency instruction it carries belongs to
/// a SIGN and is attributed to the source that gives it. Evidence log:
/// <c>.claude/work_files/wi545/plan.md</c>.
///
/// THE ABSENCE IS SCOPED, NOT UNIVERSAL. NCCN's patient bone-cancer guideline
/// returned HTTP 403 to the research pass and to a direct re-check, so it counts
/// as neither presence nor absence, and the page never claims all oncology
/// material is silent.
/// </summary>
public sealed class ChordomaPageContentTests
{
    private const string Slug = "tumors/chordoma";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a chordoma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string WhereHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string DiagnosisHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string LifeHeading = "Everyday life";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string RecurrenceHeading = "If it comes back";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CareHeading = "For the person caring for someone with this";
    private const string QuestionsHeading = "What to ask your team";
    private const string SupportHeading = "Where to get support";

    private static string Page => CuratedPage.Read("tumors", "chordoma.md");

    private static string Composed => CuratedPage.Composed(Page, Slug);

    /// <summary>
    /// The reader text of the COMPOSED page, flattened.
    ///
    /// COMPOSED, NOT RAW, AND /review ROUND 1 CAUGHT IT BEING RAW. The Gy ban, the
    /// "promising" ban and the whole prognosis-figure guard all read this, and while
    /// it was the uncomposed page none of them could see a single word contributed by
    /// the five shared blocks. This file argues two screens lower that a phrase
    /// arriving through a block reaches the reader exactly as one typed here would
    /// (§12.10); the bans have to be held to the same standard they state.
    /// </summary>
    private static string Plain => CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

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
    /// Reader text of a section, flattened, so an assertion can cross the file's hard
    /// wraps without caring where they fall.
    /// </summary>
    private static string Flat(string heading) =>
        CuratedPage.Flatten(CuratedPage.ReaderText(RawSection(heading)));

    // NO SentenceWith HELPER HERE, AND THAT IS DELIBERATE RATHER THAN AN OMISSION.
    // Sibling hubs carry one that strips ** before splitting, because SentencesOf
    // splits on (?<=[.!?])\s+ while ReaderText leaves markdown ** in place, so in a
    // bolded list one "sentence" runs on into the next bullet. WI-544 shipped a whole
    // item with a vacuous emergency-venue assertion for exactly that reason.
    //
    // THIS FILE DOES NOT NEED IT, because /review round 1 found its one sentence-level
    // venue assertion was tautological (the needle and the assertion were the same
    // clause) and it was replaced by POSITION assertions over the raw section. Those
    // do not split sentences at all, so the hazard does not arise. A helper kept for a
    // caller that no longer exists reads as a decision somebody made (§12.11).

    // ------------------------------------------------------------------ the structure

    [Fact]
    public void TheSeventeenSectionsAreAllPresentAndInOrder()
    {
        // THE HARNESS BASELINE. A structural guard is the right one to break every
        // line-ending pass against: it fails if the file is corrupt in any way that
        // would make every other result meaningless.
        Assert.Equal(
            [ShortHeading, WhatHeading, GradeHeading, WhereHeading, SymptomHeading,
             DiagnosisHeading, ReportHeading, TreatmentHeading, AfterHeading, LifeHeading,
             ScansHeading, RecurrenceHeading, CauseHeading, OutlookHeading, CareHeading,
             QuestionsHeading, SupportHeading],
            Headings());
    }

    // ------------------------------------------------------------------- the ruling

    [Fact]
    public void TheEmergencyRulesAreAttributedToTheSignsAndNotToTheTumor()
    {
        var section = Flat(SymptomHeading);

        // THE HONEST STATEMENT, and the claim the whole item turns on. Without it a
        // reader reasonably assumes the tumor's own organizations said "go to an
        // emergency room", and none of them did.
        // THE ABSENCE IS SCOPED BY SUBJECT, NOT BY PUBLISHER, AND /review ROUND 1
        // CAUGHT THE PAGE GETTING THAT BACKWARDS. An earlier draft said no "hospital
        // pages" give an emergency rule, and then four paragraphs later quoted
        // Cleveland Clinic -- a hospital page -- saying "visit an emergency room
        // immediately". The sentence was not merely loose, it was falsified by the
        // page's own next screen, which undercuts the attribution the whole section
        // rests on. The claim that is TRUE and that the sweep actually supports is
        // about SUBJECT: no page ABOUT THIS TUMOR gives the rule.
        Assert.Matches(new Regex(
            @"not one page about this tumor says when to head for an emergency\s+room",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"nothing below is a chordoma rule", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"credited to the people who wrote it", RegexOptions.IgnoreCase), section);

        // AND THE SCOPE NAMES WHAT WAS CHECKED. A page claiming nobody anywhere gives
        // such a rule would be asserting something NCCN's HTTP 403 makes unknowable.
        Assert.Matches(new Regex(
            @"chordoma organizations, and the chordoma pages on the big hospital\s+sites",
            RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(new Regex(
            @"no (source|organization|guideline)s? anywhere", RegexOptions.IgnoreCase), section);

        // THE PUBLISHER-SCOPED FORM MUST NOT COME BACK, because it is the shape the
        // page's own sign-level rules disprove two screens later.
        //
        // THE FIRST VERSION OF THIS BAN FIRED ON THE CORRECTED SENTENCE, which is the
        // too-wide direction and the more dangerous one: the reflex when a ban fires
        // on correct prose is to weaken the ban rather than question the widening
        // (§12.8, WI-544). It keyed on "hospital" NEAR a speech verb NEAR "emergency
        // room", and the fixed sentence legitimately contains all three -- it says we
        // looked at the chordoma pages on hospital SITES, and that not one page ABOUT
        // THIS TUMOR gives the rule. Proximity was never the defect.
        //
        // THE DEFECT WAS ANAPHORA: the negation was bound to the PUBLISHERS ("not one
        // of them") instead of to the SUBJECT. That construction is what makes the
        // claim false, and the corrected sentence does not use it.
        var publisherScoped = new Regex(
            @"\b(not one|none|neither) of them\b", RegexOptions.IgnoreCase);
        Assert.Matches(publisherScoped,
            "we looked through the chordoma organizations and the hospital pages, and not "
            + "one of them says when to head for an emergency room");            // canary
        Assert.Matches(publisherScoped,
            "none of them tells you when to go to an emergency room");           // canary
        Assert.DoesNotMatch(publisherScoped, section);

        // AND THE SAME CLAIM WRITTEN WITHOUT THE ANAPHOR, which /review round 2 pointed
        // out the first pattern misses: "no hospital page gives an emergency rule",
        // "none of the big hospital sites say...". The negation has to sit immediately
        // before the publisher noun, so the corrected sentence -- which says we looked
        // at "the chordoma pages on the big hospital sites" with the negation attached
        // to "not one page about this tumor" -- does not match.
        var publisherNegated = new Regex(
            @"\b(no|not one|none of the)\s+(big\s+)?(hospital|clinic|cancer cent(er|re))\s+"
            + @"(page|site|websit)",
            RegexOptions.IgnoreCase);
        Assert.Matches(publisherNegated, "no hospital page gives an emergency rule");   // canary
        Assert.Matches(publisherNegated,
            "none of the big hospital sites say when to go in");                        // canary
        Assert.DoesNotMatch(publisherNegated, section);

        // AND THE POSITIVE HALF, so the ban cannot be satisfied by the sentence simply
        // going away: the scope must still be stated, and stated by subject.
        Assert.Matches(new Regex(@"about this tumor", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void EachSignLevelRuleKeepsItsOwnVenueAndItsOwnAttribution()
    {
        var section = Flat(SymptomHeading);

        // 1. CORD COMPRESSION. The strongest instruction on the page, and the one
        //    picture where waiting costs the most.
        Assert.Matches(new Regex(
            @"Call 911, or get to the closest\s+emergency room", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"American Cancer Society warns that without fast treatment a person\s+can be left paralyzed",
            RegexOptions.IgnoreCase), section);

        // 2. CAUDA EQUINA, which is the SACRAL reader's rule and is the one most
        //    likely to be dropped, because that reader is not the one a brain-tumor
        //    site pictures first.
        Assert.Matches(new Regex(
            @"Cleveland Clinic says to\s+visit an emergency room immediately",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"More than one in four people with a tumor in the\s+sacrum",
            RegexOptions.IgnoreCase), section);

        // 3. RAISED PRESSURE.
        Assert.Matches(new Regex(
            @"go to the nearest hospital", RegexOptions.IgnoreCase), section);

        // EACH VENUE IS PINNED TO ITS OWN SIGN, AND /review ROUND 1 CAUGHT THE FIRST
        // VERSION BEING VACUOUS. It read
        //     Assert.Contains("emergency room", SentenceWith(section, "get to the closest"))
        // where the needle and the assertion are the SAME CLAUSE, so it could not fail
        // unless both changed together. Worse, every other assertion here is
        // section-scoped presence, so the cord-compression venue could be moved under
        // the double-vision paragraph and this whole test stayed green. That is the
        // §12.8 shape: a guard asserting a TOKEN where the claim is a STRUCTURE.
        //
        // THE PROPERTY IS SIGN-TO-VENUE ADJACENCY, so it is asserted by POSITION over
        // the raw section, the way the en-bloc guard does it. Each sign must be
        // followed by its own venue BEFORE the next sign begins.
        // ANCHORED ON THE CLAUSE THAT IS THE SIGN, NOT A COMMON WORD. An earlier
        // version anchored on the bare token "losing", so any later sentence using
        // that word earlier in the section would silently re-bind the anchor and this
        // guard would pass or fail for the wrong reason.
        var raw = RawSection(SymptomHeading);
        var cordSign = raw.IndexOf("New weakness in your legs", StringComparison.Ordinal);
        var cordVenue = raw.IndexOf("get to the closest", StringComparison.Ordinal);
        var caudaSign = raw.IndexOf("The same signs at the tailbone end", StringComparison.Ordinal);
        var caudaVenue = raw.IndexOf("visit an emergency room immediately", StringComparison.Ordinal);

        Assert.True(cordSign >= 0 && cordVenue > cordSign && cordVenue < caudaSign,
            "the cord-compression venue must follow its own sign and stay inside its own "
            + "paragraph, or a reader reads it as belonging to the next rule");
        Assert.True(caudaSign > 0 && caudaVenue > caudaSign,
            "the cauda equina venue must follow the sign it belongs to");
    }

    [Fact]
    public void TheDoubleVisionRuleKeepsBothHalvesAndDoesNotOverTriageTheSlowKind()
    {
        // BOTH DIRECTIONS, AND THIS PAGE IS WHERE IT MATTERS MOST. Double vision is
        // this tumor's signature symptom and it is usually the SLOW kind, which the
        // Chordoma Foundation files under "let your doctors know". The emergency rule
        // belongs to the SUDDEN presentation, which may be something else entirely.
        // Carrying only the emergency half would send every chordoma reader with
        // long-standing diplopia to an emergency room and teach them to ignore the
        // rule on the night it matters.
        var section = Flat(SymptomHeading);

        Assert.Matches(new Regex(
            @"go to an emergency room for double vision\s+that does not settle within a few hours",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"the slow pattern is the\s+usual one, and that means phoning your team instead",
            RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheEscalationBlockIsScopedBecauseThisTumorDoesNotGrowInsideTheBrain()
    {
        // The shared block is written for tumors INSIDE the brain. Two of its
        // premises are false here, and a page that composed it unscoped would tell a
        // chordoma reader to expect seizures and brain swelling. /tumors/acoustic-
        // neuroma set this precedent for an extra-axial tumor; this page follows it.
        var section = Flat(SymptomHeading);

        Assert.Matches(new Regex(
            @"this tumor does not\s+usually cause seizures", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"does not swell the brain around it", RegexOptions.IgnoreCase), section);

        // AND THE CARVE-OUT, so the scoping never reads as "the ambulance list does
        // not apply to you". Under-triage is the dangerous direction (§12.10).
        Assert.Matches(new Regex(
            @"that ambulance\s+rule still covers you", RegexOptions.IgnoreCase), section);

        // THE POST-OPERATIVE FEVER RULE IS CLAIMED TOO, and it needs claiming because
        // the block words it for "brain surgery" and a craniotomy. This reader's
        // operation went through the nose, and may not file itself under either word.
        Assert.Matches(new Regex(
            @"through your nose rather\s+than through an opening in the skull",
            RegexOptions.IgnoreCase), section);

        // POSITION IS THE PROPERTY, AND THE FIRST RENDERED READ IS WHY THIS EXISTS.
        // The paragraph says "Before you read the list below" and originally sat AFTER
        // the directive, so the reader met the whole ambulance list first and the
        // caveat afterwards. That is §12.6's warn-before-you-disclose rule inverted,
        // and no content guard could see it: every string was present in the section.
        var rawSymptoms = RawSection(SymptomHeading);
        var scoping = rawSymptoms.IndexOf("Before you read the list below", StringComparison.Ordinal);
        var directive = rawSymptoms.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        Assert.True(scoping >= 0, "the escalation scoping paragraph has gone");
        Assert.True(directive > scoping,
            "the scoping paragraph must come BEFORE [ESCALATION]; below it, the reader "
            + "meets the whole list first and reads the caveat as a correction");
    }

    [Fact]
    public void ThePageCarriesTheEscalationBlockAndItsTiersAgreeWithTheCorpus() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnTheComposedPage() =>
        // The positive half names the paragraph this MUST have examined, because an
        // iterate-and-check guard is green on a page it never looked at (§12.8,
        // WI-517). On this page the double-vision paragraph is the one that pairs a
        // warning sign with a reassurance, and it answers with a tier.
        CuratedPage.AssertNoWarningSignIsNormalised(
            Composed, Slug, "Double vision that comes on suddenly");

    // -------------------------------------------------------- the surgery location split

    [Fact]
    public void TheOnePieceRuleIsSplitByLocationAndIsNeverStatedAsUniversal()
    {
        // THE ITEM'S SHARPEST RULING, and the one a later editor is likeliest to
        // "simplify" into a single sentence.
        //
        // En bloc removal is the goal in the MOBILE SPINE AND SACRUM. The Chordoma
        // Global Consensus Group excepts the skull base in its own words: resection
        // there "may be necessarily piecemeal". A page stating one rule for every
        // location would be WRONG for exactly the clival readers who arrive here from
        // a brain-tumor feed, and wrong in the reassuring direction: it would tell
        // them a piecemeal operation was a failure when it was the plan.
        //
        // POSITION IS THE PROPERTY, not presence (WI-512). Each half must sit under
        // the subheading that scopes it; a one-piece sentence that drifted out of the
        // spine subsection would read as a rule for the whole page.
        var treatment = RawSection(TreatmentHeading);

        var spineHeading = treatment.IndexOf("### If it is in your spine or at your tailbone",
            StringComparison.Ordinal);
        var skullHeading = treatment.IndexOf("### If it is at the base of your skull",
            StringComparison.Ordinal);
        var onePiece = treatment.IndexOf("The aim is to take it out in one piece",
            StringComparison.Ordinal);
        var piecemeal = treatment.IndexOf("removal may have to be piecemeal",
            StringComparison.Ordinal);

        Assert.True(spineHeading >= 0, "the treatment section has lost its spine and sacrum subheading");
        Assert.True(skullHeading > spineHeading, "the treatment section has lost its skull base subheading");
        Assert.True(onePiece > spineHeading && onePiece < skullHeading,
            "the one-piece goal must sit INSIDE the spine and sacrum subsection, or it reads "
            + "as a rule for every location");
        Assert.True(piecemeal > skullHeading,
            "the skull base subsection must carry the piecemeal exception");

        // AND THE EXCEPTION IS NOT SOFTENED INTO A FAILURE. The sentence that tells a
        // clival reader this is not a failure is the whole point of carrying it.
        Assert.Matches(new Regex(
            @"is often not possible, and that is not a\s+failure", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(treatment)));
    }

    // ------------------------------------------------------------------- the grade

    [Fact]
    public void TheGradeIsStatedAsAbsentAndItsTwoHalvesAreAttributedSeparately()
    {
        // WI-541's split, applied: cite the document for what it SAYS, not for what it
        // is authoritative about. WHO CNS5 is the naming authority and prints NO grade
        // beside chordoma; the "low grade" characterisation comes from the clinical
        // reference instead. Citing the naming document for the grade would be a
        // citation that does not support the claim in the form it is made (§12.14).
        var section = Flat(GradeHeading);

        Assert.Matches(new Regex(
            @"lists chordoma and prints no grade", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"Clinical references call it low grade", RegexOptions.IgnoreCase), section);

        // THE COUNTERWEIGHT, and it is the §12.12 direction that matters here: "low
        // grade" read alone is over-reassuring for a tumor that recurs in more than
        // half of patients.
        Assert.Matches(new Regex(
            @"Low grade here does not mean mild", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"Slow is\s+not the same as harmless", RegexOptions.IgnoreCase), section);
    }

    // -------------------------------------------------------------- the block rulings

    [Fact]
    public void ExactlyFiveBlocksAreIncludedAndTheThreeExclusionsAreAClosedSet()
    {
        // AN EXCLUDED BLOCK LEAVES NO TRACE ON THE PAGE -- no heading, no directive,
        // nothing to grep -- so a later item can restore one and every other guard in
        // this file stays green. An EXACT SET, not a handful of DoesNotContain checks:
        // a block added in two years fails here and has to be ruled on deliberately.
        Assert.Equal(
            ["caregiver", "causes", "crosswalk", "escalation", "tumor-board"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        // Each included block sits in the section that gives it its meaning. POSITION
        // is the property (WI-512): a directive that drifts into another section still
        // composes, and still reads as unrelated to the thing it qualifies.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);

        // AND [CROSSWALK] SITS ABOVE THE PAGE'S OWN SLICE, because that slice says the
        // NOS part is "the one explained just above" and the block is what explains it.
        // Position, not presence: a directive that drifted below the slice still
        // composes and leaves the cross-reference pointing at nothing.
        var report = RawSection(ReportHeading);
        Assert.True(
            report.IndexOf("[CROSSWALK]", StringComparison.Ordinal)
                < report.IndexOf("### Older names", StringComparison.Ordinal),
            "the crosswalk block must compose ABOVE the page's own retired-name slice, "
            + "which refers back to it");
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheMechanismBlockIsExcludedBecauseThisTumorGrowsInBoneOutsideTheBrain()
    {
        // [MECHANISM] is included on nearly every other hub, so this is the exclusion a
        // later editor is likeliest to "correct".
        //
        // IT IS EXCLUDED BECAUSE ITS SUBJECT IS A TUMOR INSIDE THE BRAIN. The block
        // explains swelling of the brain around a tumor, seizures, and a map of brain
        // areas. Chordoma grows in BONE, and for the roughly one third of readers whose
        // tumor is at the tailbone end the entire block is about a different part of the
        // body. §12.10's test ("would this be true on the hub you have thought about
        // least?") fails outright.
        //
        // Asserted against the BLOCK'S OWN WORDS rather than a literal list, so moving a
        // sentence into or out of that block later turns this red instead of leaving a
        // stale copy behind (§12.11).
        var mechanism = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md"))));

        foreach (var blockOnly in new[]
                 {
                     "The skull is a closed box",
                     "the symptom tells you where, the scan tells you what",
                 })
        {
            Assert.Contains(blockOnly, mechanism, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        Assert.DoesNotContain("mechanism", ContentBlocks.DirectBlockNames(Page));

        // AND THE PAGE ANSWERS §12.3's SECTION 3 ITSELF, because excluding a block
        // without replacing what it did would leave the heading's second question
        // unanswered (§12.11: a heading with two questions in it owes two answers).
        var where = Flat(WhereHeading);
        Assert.Matches(new Regex(
            @"Your symptoms come from what the tumor is pressed against",
            RegexOptions.IgnoreCase), where);
        Assert.Matches(new Regex(@"It is called the clivus", RegexOptions.IgnoreCase), where);
        Assert.Matches(new Regex(
            @"the nerves it meets are the ones that run your bladder", RegexOptions.IgnoreCase), where);
    }

    [Fact]
    public void TheSpinalCordBlockIsExcludedAndThePageStatesItsOwnCordTierInstead()
    {
        // WI-543'S PRECEDENT EXACTLY. That hub excluded this block "even though its
        // name matches the subject" and stated its own tier. The same applies here and
        // for a sharper reason: the block opens on a conditional that is FALSE of this
        // tumor. It addresses a reader whose tumor "is in the spinal cord, or has
        // spread to the spine". A chordoma is in the BONE, and it presses on the cord
        // and the nerve roots from outside. Composing it would hand this reader a
        // conditional that never names them, in place of a rule that does.
        var block = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "spinal-cord.md"))));

        foreach (var blockOnly in new[]
                 {
                     "right-away call, not a same-day one",
                     "new signs from the cord are their own rule",
                 })
        {
            Assert.Contains(blockOnly, block, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        Assert.DoesNotContain("spinal-cord", ContentBlocks.DirectBlockNames(Page));

        // AND THE REPLACEMENT IS REAL, not an omission. The page carries its own
        // sourced tier for the same signs, at emergency strength, attributed.
        var section = Flat(SymptomHeading);
        Assert.Matches(new Regex(
            @"losing\s+control of peeing or pooping is an emergency", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePosteriorFossaBlockIsExcludedBecauseThisIsNotCerebellarSurgery()
    {
        // Ruled out on evidence, not assumption: the block's subject is a syndrome
        // following surgery low at the back of the brain, in children. Chordoma surgery
        // is transnasal at the skull base, or spinal. Nothing about it is posterior
        // fossa.
        var block = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "posterior-fossa-syndrome.md"))));

        const string BlockOnly = "Most children slowly get better";
        Assert.Contains(BlockOnly, block, StringComparison.Ordinal);
        Assert.DoesNotContain(BlockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        Assert.DoesNotContain("posterior-fossa-syndrome", ContentBlocks.DirectBlockNames(Page));
    }

    // --------------------------------------------------------------- diagnosis routing

    [Fact]
    public void TheBiopsyRouteRulingIsCarriedAndTheRoutingIsNotAnUrgencyInstruction()
    {
        // THE MOST ACTIONABLE THING ON THE PAGE, and the one place where what a reader
        // does in the next week changes what is possible for them later.
        var section = Flat(DiagnosisHeading);

        Assert.Matches(new Regex(@"should be done from the back", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Surgeons call that\s+seeding", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"avoid having a biopsy or surgery outside a referral center",
            RegexOptions.IgnoreCase), section);

        // AND THE TWO IDEAS ARE KEPT APART. "As soon as chordoma is suspected" is about
        // WHERE you go, not HOW FAST you must be seen. Merging them would imply an
        // emergency that no source declares, on the one page whose central ruling is
        // that no chordoma source declares one.
        Assert.Matches(new Regex(
            @"about where you go, not how fast", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"not an instruction to rush to an emergency room", RegexOptions.IgnoreCase), section);
    }

    // ------------------------------------------------------------------ the crosswalk

    [Fact]
    public void TheRetiredNameSliceCarriesOnlyWhatWasVerifiedAndClaimsNoCleanSlate()
    {
        var section = Flat(ReportHeading);

        // WHAT IS VERIFIED, and therefore what ships.
        Assert.Matches(new Regex(
            @"Chondroid chordoma is not a separate diagnosis any more", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"Parachordoma is an old name and it is not this tumor", RegexOptions.IgnoreCase), section);

        // THE UNVERIFIED CLAIM MUST NOT COME BACK. A search-engine summary asserted that
        // poorly differentiated chordoma was formerly called "undifferentiated",
        // "cellular" or "atypical" chordoma. Two targeted Europe PMC searches returned
        // no support, and pathologyoutlines.com -- the canonical home of retired
        // terminology -- was HTTP 429 with a 24-hour lockout to the research pass AND to
        // a direct re-check. It is an open gap, so the page must not fill it either way.
        //
        // BANNED AS A CLAIM SHAPE, not as the deleted string (§12.8, WI-544: a ban keyed
        // to the sentence already fixed protects against nothing). Both canaries: the
        // first proves the pattern fires, the second proves it fires on the inflection
        // an editor would actually write.
        var formerName = new Regex(
            @"\b(formerly|previously|used to be|once)\b[^.]{0,60}\b(undifferentiated|cellular|atypical)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(formerName,
            "poorly differentiated chordoma was formerly called undifferentiated chordoma");  // canary
        Assert.Matches(formerName,
            "this type used to be known as atypical chordoma");                               // canary
        Assert.DoesNotMatch(formerName, section);

        // AND THE PAGE NEVER CLAIMS THERE ARE NO OLD NAMES, which is the absence the
        // 429 makes unknowable.
        Assert.DoesNotMatch(new Regex(
            @"chordoma has (no|never had) (any )?(old|former|retired) names", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheChondrosarcomaContrastKeepsAllThreeDiscriminators()
    {
        // The single most important differential for a skull-base reader, because
        // treatment and outlook differ. All three discriminators or none: any one of
        // them alone is not enough to tell the two apart, and a reader holding the
        // wrong one of these two diagnoses is reading the wrong page entirely.
        //
        // SCOPED TO THE CHONDROSARCOMA BULLET, AND THE BREAK HARNESS IS WHY. The first
        // version asserted all three discriminators against the WHOLE report section,
        // and the brachyury half was satisfied by the PARACHORDOMA paragraph further up,
        // which says "It does not carry brachyury, which is how the two are told apart."
        // So the chondrosarcoma entry could lose its brachyury discriminator entirely
        // and this guard stayed green -- on BOTH line endings, the only survivor of 142
        // mutation runs. That is the §12.8 shape exactly: a guard asserting a TOKEN
        // where the claim is a STRUCTURE, green on the defect its own name forbids.
        //
        // THE MUTATION WAS NOT WEAKENED TO SUIT THE GUARD. A survivor is either a weak
        // guard or a weak mutation, and this one was a weak guard, so the guard was
        // rewritten (§12.8, WI-537's distinction).
        var bullet = Regex.Match(
            RawSection(ReportHeading),
            @"^- \*\*Chondrosarcoma\.\*\*(.*?)(?=^- \*\*|\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(bullet.Success, "the report section has lost its chondrosarcoma entry");
        var entry = CuratedPage.Flatten(bullet.Groups[1].Value);

        Assert.Matches(new Regex(
            @"starts in cartilage rather than notochord leftovers", RegexOptions.IgnoreCase), entry);
        Assert.Matches(new Regex(
            @"to the side rather than in the midline", RegexOptions.IgnoreCase), entry);
        Assert.Matches(new Regex(@"does not\s+carry brachyury", RegexOptions.IgnoreCase), entry);

        // AND THE PARACHORDOMA ENTRY KEEPS ITS OWN, so tightening the scope did not
        // silently drop the other place this page tells a look-alike apart by the same
        // marker. Two entries, two assertions, neither standing in for the other.
        Assert.Matches(new Regex(
            @"Parachordoma[\s\S]{0,400}?does not\s+carry brachyury", RegexOptions.IgnoreCase),
            Flat(ReportHeading));
    }

    // ------------------------------------------------------------------- the radiation

    [Fact]
    public void TheProtonSectionExplainsTheAnatomyWithoutClaimingSuperiority()
    {
        var section = Flat(TreatmentHeading);

        // THE COUNTERWEIGHT, IN THE FOUNDATION'S OWN SCOPE. Its dose recommendation
        // applies "for particle and photon therapies", so the requirement is the DOSE,
        // not the machine. That is what lets this page agree with
        // /treatments/proton-therapy without over-claiming.
        Assert.Matches(new Regex(
            @"for particle beams and for X-ray treatments alike", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"requirement is the dose, not the machine", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"evidence as low or very low quality", RegexOptions.IgnoreCase), section);

        // "PROMISING" IS BANNED FRAMING and is exactly the word the systematic review
        // uses. It is the qualifier doing the work the evidence grade says it cannot.
        Assert.DoesNotMatch(new Regex(@"\bpromising\b", RegexOptions.IgnoreCase), Plain);

        // NO Gy FIGURE ANYWHERE IN READER TEXT (§12.4 R1: all Gy and mg figures stay
        // out). This page was the only one in the corpus to carry one, and it is gone.
        Assert.DoesNotMatch(new Regex(@"\bGray\b|\d\s*Gy\b"), Plain);
    }

    // -------------------------------------------------------------- numbers and outlook

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAndNoSurveillanceSchedule()
    {
        // §12.2 item 5: no survival statistics, no median survival, no five-year rates,
        // anywhere. Eight sources in this item's research publish them, and cancer.org's
        // bone-cancer survival page carries CHORDOMA-SPECIFIC five-year figures while the
        // corpus already cites cancer.org elsewhere -- so an editor could wander in.
        foreach (var banned in new[]
                 {
                     "survival rate", "5-year", "five-year", "median survival",
                     "life expectancy", "years to live",
                 })
        {
            Assert.DoesNotContain(banned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // A percentage ATTACHED to surviving, in either order. The page legitimately
        // prints "more than 95 percent" about a gene variant, so a bare percentage ban
        // would fail correct prose -- the too-wide direction, which is the more
        // dangerous one because the reflex is then to weaken the guard (§12.8, WI-544).
        var survivalFigure = new Regex(
            @"\d+\s*(%|percent)[^.]{0,40}\b(surviv|alive)\b"
            + @"|\b(surviv|alive)\w*[^.]{0,40}\d+\s*(%|percent)",
            RegexOptions.IgnoreCase);
        Assert.Matches(survivalFigure, "about 89 percent of people were still alive");   // canary
        Assert.Matches(survivalFigure, "survival was 54 percent");                        // canary
        Assert.DoesNotMatch(survivalFigure, Plain);
        Assert.Contains("More than 95 percent", Plain, StringComparison.Ordinal);         // negative canary

        // RULING 8: the durable idea, not a schedule. The Chordoma Foundation's own two
        // pages give DIFFERENT intervals and the consensus group says there is
        // insufficient data to recommend one, so publishing any of them as settled fact
        // would be asserting a precision the sources do not have.
        var scans = Flat(ScansHeading);
        Assert.Matches(new Regex(@"not going to print a schedule", RegexOptions.IgnoreCase), scans);
        Assert.DoesNotMatch(new Regex(
            @"every \d+( to \d+)? (months?|years?)", RegexOptions.IgnoreCase), scans);
    }

    [Fact]
    public void TheOutlookGateTeachesTheConceptsAndPublishesNoFigure()
    {
        // §12.5: explain the concepts, publish no figures. The Kirkebøen framing wants
        // the group, the distribution, the right skew, and the plain statement that it
        // is not a prediction about one person. A gate that publishes no figures still
        // has to teach the vocabulary, or it explains nothing (§12.9, WI-513).
        var gate = Regex.Match(Page, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook section has lost its reader-choice gate");
        var inside = CuratedPage.Flatten(CuratedPage.ReaderText("---\n---\n" + gate.Groups[1].Value));

        Assert.Matches(new Regex(
            @"half of that group came out above it and half\s+below", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(
            @"those above it can be a very long way above", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(
            @"predicts nothing about any one person", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(
            @"older than they look", RegexOptions.IgnoreCase), inside);

        // THE RIGHT TAIL BELONGS INSIDE THE GATE (§12.9), and the actionable
        // slow-growing point is what makes the gate worth opening.
        Assert.Matches(new Regex(
            @"time to get a second opinion", RegexOptions.IgnoreCase), inside);

        // AND THE HEADING STAYS OUTSIDE IT, so the page outline is complete and the
        // reader meets heading, then warning, then choice (§12.6).
        Assert.DoesNotContain(OutlookHeading, gate.Groups[1].Value, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSelfBlameSectionKeepsTheInheritedRiskWordingExactly()
    {
        // The distinction MedlinePlus draws is the whole point: an inherited
        // duplication raises RISK, it is not the condition. And the common variant is
        // in more than 95 percent of patients, which is far too common to be a cause --
        // letting that read as "95 percent of chordoma is genetic" would be the exact
        // opposite of what the source says, on the page a reader arrives at blaming
        // themselves.
        var section = Flat(CauseHeading);

        // The emphasis is part of the claim: "not the condition itself" is the half a
        // reader skims past, so the page bolds it and this pins it bolded.
        Assert.Matches(new Regex(
            @"inherit an increased risk of the condition, \*\*not\s+the condition itself\*\*",
            RegexOptions.IgnoreCase),
            CuratedPage.Flatten(RawSection(CauseHeading)));
        Assert.Matches(new Regex(
            @"far too\s+common to be a cause on its own", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"does not by itself cause chordoma", RegexOptions.IgnoreCase), section);
    }

    // ------------------------------------------------------------- the shared prose gates

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        // BOTH SHARED GUARDS, CALLED EXPLICITLY, BECAUSE THEY ARE OPT-IN. There is no
        // corpus-wide sweep for either: every use is a page calling it in its own test
        // file. AtrtPageTests records /review round 1 finding that file calling NEITHER,
        // and /tumors/spinal-cord-tumor still omits both. A guard nobody calls is not a
        // guard.
        //
        // "aggressive" IS ON THE CHARACTERISATIONS LIST AND IS THE LIVE RISK HERE.
        // Chordoma sources use it constantly ("locally aggressive"), and the Chordoma
        // Foundation calls the poorly differentiated subtype "more aggressive". Every
        // one is reframed in this page's prose. The word appears in this file's front
        // matter comments, quoting sources, which ReaderText never reads.
        //
        // Run on the COMPOSED page: a phrase arriving through a block reaches the
        // reader exactly as one typed here would (§12.10).
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Composed), Slug);

        // PROVE THE LIST CAN FIRE BEFORE TRUSTING THAT IT DID NOT (§12.10). An empty
        // or renamed list leaves the loop below green forever, which is the shape of a
        // guard that has quietly stopped guarding. The named word is this page's live
        // risk: chordoma sources use "aggressive" constantly.
        Assert.NotEmpty(CuratedPage.Characterisations);
        Assert.Contains("aggressive", CuratedPage.Characterisations, StringComparer.OrdinalIgnoreCase);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        // Several of this item's sources are non-US: a Cochrane protocol, a Canadian
        // pathology reference, a British-spelled paediatric series. None of their
        // wording is reused, and this is what holds that.
        //
        // STRIP-THEN-SCAN, the stronger of the two implementations in the repo
        // (§12.10): skipping with `continue` disables a whole form if any exemption
        // phrase appears anywhere in the file.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        // Same proof-it-can-fire check as the characterisations loop above. Both of
        // this page's real British slips were caught by scanning the FULL list, not a
        // sample of it, so the list being intact is load-bearing.
        Assert.NotEmpty(CuratedPage.BritishForms);
        Assert.Contains("tumour", CuratedPage.BritishForms, StringComparer.OrdinalIgnoreCase);

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // "being sick" reads as "being unwell" in US English, not "vomiting", and this
        // page's first draft carried it on a RAISED-PRESSURE escalation line, where the
        // difference is the whole instruction (§12.8, WI-563).
        foreach (var idiom in new[]
                 {
                     "A&E", "straight away", "out of hours", "being sick", "come round",
                     "advice line", "car park", "999",
                 })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST, and that is the ruling. The guard is BIDIRECTIONAL: this page's
        // first draft shared 61 windows across eight files and turned
        // /tumors/acoustic-neuroma RED on two of its own tests. An allowlist here could
        // not have fixed that, because that page's test never reads this page's.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    // -------------------------------------------------------------- the front matter

    [Fact]
    public void TheSourcesAreRealAndTheBarredOnesAreNotCited()
    {
        var front = CuratedPage.FrontMatter(Page);

        // mayoclinic.org is barred corpus-wide, and §12.8 records WHY: it returns HTTP
        // 403 to automated fetching, so it cannot be script-checked. This item's
        // research reached Mayo wording through a licensee mirror; citing that would
        // launder an unverifiable source past a bar that exists for verifiability. So
        // neither the domain nor the mirror appears.
        foreach (var barred in new[]
                 {
                     "mayoclinic.org", "mhsystem.org", "ahfs", "medlineplus.gov/druginfo",
                     "cancer.gov/types/brain", "academic.oup.com",
                 })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }

        // THE NEAREST PROGNOSIS LANDMINE, NAMED SO IT CANNOT BE WANDERED INTO.
        // cancer.org's bone-cancer survival page publishes chordoma-specific five-year
        // figures, and this page legitimately cites cancer.org for the cord-compression
        // rule -- so the bar is on the PATH, not the domain (the same shape as the
        // MedlinePlus druginfo bar).
        Assert.DoesNotContain("survival-statistics", front, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cancer.org/cancer/supportive-care", front, StringComparison.OrdinalIgnoreCase);

        // medlineplus.gov/genetics IS cited and is not a drug monograph.
        Assert.Contains("medlineplus.gov/genetics", front, StringComparison.OrdinalIgnoreCase);

        // NO BRACKETED BLOCK NAME ANYWHERE IN FRONT MATTER. CaregiverSectionTests
        // locates the caregiver block with a raw IndexOf over the WHOLE file, so a
        // bracketed name in a comment becomes the FIRST match and this page would be
        // reported as not carrying the block.
        Assert.DoesNotMatch(new Regex(@"^\s*\[[A-Z0-9][A-Z0-9-]*\]\s*$", RegexOptions.Multiline), front);

        // THE TWO GAPS THIS ITEM COULD NOT CLOSE ARE RECORDED WHERE THE NEXT READER
        // WILL SEE THEM, rather than left to look like completeness.
        Assert.Matches(new Regex(@"HTTP 403", RegexOptions.IgnoreCase), front);
        Assert.Matches(new Regex(@"HTTP 429", RegexOptions.IgnoreCase), front);

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

    [Fact]
    public void TheProtonTherapyPageCarriesTheDoorBackToThisPage()
    {
        // THE RECIPROCAL DOOR, AND IT WAS UNGUARDED UNTIL /review ROUND 1 SAID SO.
        // /treatments/proton-therapy named chordoma in prose and linked nowhere, which
        // is how WI-412 shipped a page nothing pointed at. This item added the link, and
        // a link nothing asserts is a link the next editor can delete without a test
        // going red.
        //
        // THE PROPERTY IS ADJACENCY, not mere presence: the href has to sit in the
        // sentence that names this tumor, because that is the sentence a reader is
        // reading when they want the page. Asserting the href anywhere on the file
        // would pass with the link moved into an unrelated section.
        var proton = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("treatments", "proton-therapy.md")));

        var named = proton.IndexOf("as that kind of tumor", StringComparison.Ordinal);
        Assert.True(named > 0,
            "/treatments/proton-therapy no longer names chordoma as the tumor the ACS "
            + "proton claim is about, so the door this page relies on has lost its anchor");

        // ASSERTED DIRECTLY RATHER THAN THROUGH A PROXY WINDOW. An earlier version
        // measured a 120-character gap from "as that kind of tumor", an anchor that
        // does not itself contain the word chordoma, so the stated property (the href
        // sits in the sentence NAMING this tumor) was only implied. This says it.
        Assert.Matches(new Regex(
            @"\[chordoma\]\(/tumors/chordoma\)[^.]{0,20}as that kind of tumor",
            RegexOptions.IgnoreCase), proton);
    }

    [Fact]
    public void TheSupportSectionCarriesTheNavigatorNumberExactly()
    {
        // A PHONE NUMBER IS THE ONE THING ON THIS PAGE THAT IS USELESS IF IT IS ONE
        // DIGIT WRONG, and it is the line most likely to be read at 2am. Verified
        // against the live page rather than the research pack.
        var section = Flat(SupportHeading);

        Assert.Contains("1-888-502-6109", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(
            @"Monday to\s+Friday, 8am to 5pm Eastern", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"open to anyone\s+affected by chordoma, including family and friends",
            RegexOptions.IgnoreCase), section);

        // RECORDED ABSENCE, CHECKED DELIBERATELY: the Foundation does NOT give money
        // directly. Navigators explain assistance programs and help with appeals.
        // Implying otherwise would send someone to ask for something that is not there.
        Assert.DoesNotMatch(new Regex(
            @"(gives|provides|pays|grants)[^.]{0,40}(money|grants|funding) (directly )?to (patients|you)",
            RegexOptions.IgnoreCase), section);
    }
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class ChordomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/chordoma";

    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// The connection string is pushed in, exactly as every other render fixture does
    /// it. Taking the factory as-is works on a developer machine because user-secrets
    /// supplies one, and fails on CI where nothing does.
    /// </summary>
    public ChordomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    private HttpClient Client => _factory.CreateClient();

    [Fact]
    public async Task ThePageIsServedAtItsUrl()
    {
        var response = await Client.GetAsync(Url);
        Assert.True(response.IsSuccessStatusCode, $"{Url} returned {(int)response.StatusCode}");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Chordoma", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The short version", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheFiveBlocksComposeAndTheThreeExcludedOnesLeaveNoTrace()
    {
        // THE HALF THE CONTENT-SIDE GUARDS STRUCTURALLY CANNOT SEE. They read directive
        // NAMES out of the source file, so they stay green if composition itself breaks.
        // Only a rendered assertion catches that, and only from both directions.
        //
        // EVERY STRING IS TOOLTIP-FREE. A glossary popover injects TEXT into the middle
        // of a sentence, so a contiguous match fails in HTML even when the block
        // composed perfectly (§12.8, WI-543).
        var html = await Client.GetStringAsync(Url);

        foreach (var composed in new[]
                 {
                     "Call an ambulance",                          // ESCALATION
                     "You are allowed to ask questions",           // CAREGIVER
                     "nobody knows the cause",                     // CAUSES
                     "Only the way of writing it did.",            // CROSSWALK
                     "What they decide is advice, not an order",   // TUMOR-BOARD
                 })
        {
            Assert.Contains(composed, html, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var excluded in new[]
                 {
                     "The skull is a closed box",                  // MECHANISM
                     "right-away call, not a same-day one",        // SPINAL-CORD
                     "Most children slowly get better",            // POSTERIOR-FOSSA-SYNDROME
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
        await CuratedPage.AssertLinksResolveIn(Client, Url, "where-to-get-support", "/get-help-now");

    [Fact]
    public async Task EveryDeepLinkedAnchorExists() =>
        // AssertLinksResolve cannot see fragments: its regex stops at the `#`, so a link
        // to an anchor that does not exist resolves as a healthy 200 and lands the reader
        // at the top of a long page (§12.8, WI-508). NOT VACUOUS HERE: the composed
        // escalation block deep-links the chemotherapy fever rule and the shunt warning
        // signs, so this really does have anchors to resolve.
        await CuratedPage.AssertFragmentLinksResolve(Client, Url);

    [Fact]
    public async Task TheSuppressedTooltipStaysQuietAndAnUnsuppressedOneStillFires()
    {
        // ONE SUPPRESSION, AND IT IS A RULING. The [TUMOR-BOARD] block opens by
        // defining a tumor board in almost the same words as the glossary entry, so an
        // unsuppressed popover prints the definition directly above the definition --
        // §12.8's echo shape (WI-535). Ten of the twelve hubs including this block do
        // not suppress it; /tumors/all-brain-tumors and /tumors/spinal-cord-tumor do,
        // and this page follows the more recent call. The marker sits on the prose line
        // immediately before the directive, the same placement both siblings use.
        //
        // THE ABSENCE ANCHOR IS GLOSSARY-ONLY WORDING, AND THAT IS THE WHOLE
        // DIFFICULTY. The block renders "is a meeting where specialists look at one
        // person's case together: surgeons, cancer doctors, the lab, and the people who
        // read scans" -- so asserting the glossary's opening clause is ABSENT would go
        // red on correct composition, and the natural next move would be to weaken or
        // delete the guard. "You do not attend" appears in the glossary entry and
        // nowhere in the block, which says "You do not go to the meeting."
        //
        // BOTH DIRECTIONS, AND THE POSITIVE HALF IS WHAT MAKES IT BIND. Suppression is
        // page-wide (GlossaryMarker collects every marker in the document), so a test
        // that only checked an absence would pass just as happily if the glossary
        // stopped firing altogether (§12.10, WI-514). Each definition is proved to
        // EXIST in its glossary file first, or rewording that file silently retires the
        // guard.
        var html = await Client.GetStringAsync(Url);
        var glossary = Path.Combine(CuratedPage.BlocksRoot, "..", "glossary");

        var board = CuratedPage.Flatten(File.ReadAllText(Path.Combine(glossary, "tumor-board.md")));
        foreach (var glossaryOnly in new[]
                 {
                     "You do not attend",
                     "advice for your own doctor to talk through with you",
                 })
        {
            Assert.Contains(glossaryOnly, board, StringComparison.Ordinal);
            Assert.DoesNotContain(glossaryOnly, CuratedPage.Flatten(html), StringComparison.Ordinal);
        }

        // AND THE BLOCK ITSELF STILL COMPOSES, so the suppression cannot be passing
        // because the whole section vanished.
        Assert.Contains("What they decide is advice, not an order", html, StringComparison.Ordinal);

        // THE PAIRED POSITIVE. "glioma" is NOT suppressed here: this page names
        // chordoid glioma in its crosswalk without defining what a glioma is, so the
        // popover is the reader's only explanation and is doing real work. Suppressing a
        // term the prose never defines is the over-suppression direction (§12.11).
        var glioma = CuratedPage.Flatten(File.ReadAllText(Path.Combine(glossary, "glioma.md")));
        const string GliomaDefinition = "A tumor that starts in the glial cells";
        Assert.Contains(GliomaDefinition, glioma, StringComparison.Ordinal);
        Assert.Contains(GliomaDefinition, CuratedPage.Flatten(html), StringComparison.Ordinal);

        // And no authoring marker survives into the HTML.
        foreach (var marker in new[] { "!%", "%%" })
        {
            Assert.DoesNotContain(marker, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndex()
    {
        // WI-412 shipped a page nothing linked to. The listing is a property of two
        // files agreeing -- taxonomy.yml and the index -- rather than of anyone
        // remembering.
        var html = await Client.GetStringAsync("/tumors");

        // THE HREF IS THE DOOR. Asserting the label alone pins nothing: it appears as
        // text and the slug appears in an anchor id, so both would survive the listing
        // losing its link entirely.
        Assert.Contains("/tumors/chordoma", html, StringComparison.Ordinal);
        Assert.Contains("Chordoma", html, StringComparison.Ordinal);
    }
}
