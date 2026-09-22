using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-546: <c>/tumors/cns-germ-cell-tumor</c>, a NEW page. The tumor appeared nowhere
/// in the corpus before this item.
///
/// TWO TUMORS WEAR ONE NAME. A germinoma and the non-germinomatous kinds are
/// diagnosed, treated and followed differently, so several guards below assert
/// that each plan stays under its OWN subheading, by position. A sentence that
/// drifts from one to the other still passes every presence check and misleads
/// half the readers.
///
/// THE URGENCY RULING IS BUILT ON A SCORED ABSENCE, the WI-544 and WI-545 shape.
/// Ten pages about this tumor were read and none gives a 911 or emergency room
/// rule; two more were blocked and are counted as unknown. So every urgent line
/// belongs to a SIGN and names its source. Evidence log:
/// <c>.claude/work_files/wi546/plan.md</c>.
///
/// OVER-REASSURANCE IS THE LIVE RISK HERE, not fear (§12.12). Nearly every patient
/// page about this tumor prints a cure figure or says it "rarely" comes back, so
/// the cure-word ban and the both-halves growing-teratoma guard are the ones a
/// later editor is likeliest to wear down.
/// </summary>
public sealed class CnsGermCellTumorPageContentTests
{
    private const string Slug = "tumors/cns-germ-cell-tumor";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a germ cell tumor in the brain?";
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

    private static string Page => CuratedPage.Read("tumors", "cns-germ-cell-tumor.md");

    private static string Composed => CuratedPage.Composed(Page, Slug);

    /// <summary>
    /// The reader text of the COMPOSED page, flattened. Composed, not raw: WI-545's
    /// /review round 1 found its bans reading the raw page, blind to every word the
    /// shared blocks contribute, while the same file argued a block's words reach the
    /// reader exactly as typed ones do (§12.10).
    /// </summary>
    private static string Plain => CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section of the RAW page, unflattened, for POSITION assertions.
    /// <c>\r?\n</c> throughout: working trees are CRLF, CI is LF.
    /// </summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    /// <summary>
    /// One <c>###</c> subsection of a raw section, up to the next <c>###</c> or the end.
    /// Scoping a guard to its subsection is what stops a phrase in a NEIGHBOURING
    /// subsection satisfying it (§12.8, WI-545's chondrosarcoma survivor).
    /// </summary>
    private static string Subsection(string heading, string subheading)
    {
        var match = Regex.Match(
            RawSection(heading),
            $@"^### {Regex.Escape(subheading)}\s*$(.*?)(?=^### |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"'{heading}' has no '### {subheading}' subsection");
        return match.Groups[1].Value;
    }

    private static string Flat(string text) => CuratedPage.Flatten(CuratedPage.ReaderText(text));

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$").Select(m => m.Groups[1].Value.Trim())];

    /// <summary>The reader text outside the outlook gate: where cure words are banned.</summary>
    private static string OutsideTheGate()
    {
        var gate = Regex.Match(Composed, @":::outlook.*?:::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate has gone, so 'outside it' means nothing");
        return CuratedPage.Flatten(CuratedPage.ReaderText(Composed.Remove(gate.Index, gate.Length)));
    }

    // ------------------------------------------------------------------ the structure

    [Fact]
    public void TheSeventeenSectionsAreAllPresentAndInOrder()
    {
        // THE HARNESS BASELINE: fails if the file is corrupt in any way that would
        // make every other result meaningless.
        Assert.Equal(
            [ShortHeading, WhatHeading, GradeHeading, WhereHeading, SymptomHeading,
             DiagnosisHeading, ReportHeading, TreatmentHeading, AfterHeading, LifeHeading,
             ScansHeading, RecurrenceHeading, CauseHeading, OutlookHeading, CareHeading,
             QuestionsHeading, SupportHeading],
            Headings());
    }

    // ------------------------------------------------------------------ the urgency ruling

    [Fact]
    public void TheEmergencyAbsenceIsScopedBySubjectAndTheBlockedPagesAreUnknown()
    {
        var section = Flat(RawSection(SymptomHeading));

        // THE HONEST STATEMENT, SCOPED BY SUBJECT. WI-545 shipped a publisher-scoped
        // version first and the page's own next screen falsified it; this page quotes
        // Cleveland Clinic's 911 rule for low sodium, so the same trap is live here.
        Assert.Matches(new Regex(
            @"None of the\s+pages about this tumor that we could read says when to call 911",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"none of the rules here is a rule about the tumor itself", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"each one names who gives it", RegexOptions.IgnoreCase), section);

        // THE COUNT IS PART OF THE CLAIM, and /review round 1 noted nothing pinned it:
        // "we read twenty pages" would have passed. Ten is what was read and scored.
        Assert.Matches(new Regex(@"We read ten pages written about\s+germ cell tumors",
            RegexOptions.IgnoreCase), section);

        // THE TWO BLOCKED PAGES ARE UNKNOWN, NOT SILENT. A 403 is neither presence nor
        // absence (WI-545), and folding them into the count would overstate the sweep.
        Assert.Matches(new Regex(
            @"count them as unknown rather than silent", RegexOptions.IgnoreCase), section);

        // THE PUBLISHER-SCOPED FORMS STAY OUT. Anaphora binding the negation to the
        // publishers, and a negation sitting right before a publisher noun. Both
        // canaries each, so neither ban can be green because it cannot fire.
        var publisherScoped = new Regex(@"\b(not one|none|neither) of them\b", RegexOptions.IgnoreCase);
        Assert.Matches(publisherScoped, "we read the hospital pages and none of them says when to go in");
        Assert.Matches(publisherScoped, "not one of them gives an emergency rule");
        Assert.DoesNotMatch(publisherScoped, section);

        var publisherNegated = new Regex(
            @"\b(no|not one|none of the)\s+(big\s+)?(hospital|clinic|children's hospital|charit(y|ies))\s+"
            + @"(page|site|websit)",
            RegexOptions.IgnoreCase);
        Assert.Matches(publisherNegated, "no hospital page gives an emergency rule");
        Assert.Matches(publisherNegated, "none of the children's hospital sites say when to go in");
        Assert.DoesNotMatch(publisherNegated, section);

        // AND NEVER A UNIVERSAL: two pages were unreadable.
        Assert.DoesNotMatch(new Regex(
            @"no (source|page|organization|guideline)s? anywhere", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void EachSignLevelRuleKeepsItsOwnVenueAndItsOwnSource()
    {
        // FOUR RULES, FOUR VENUES, FOUR SOURCES. POSITION IS THE PROPERTY: each venue
        // must sit after its own sign and before the next sign begins, or a reader takes
        // one rule's venue for another's (WI-545's vacuous-adjacency finding).
        var raw = RawSection(SymptomHeading);

        int At(string needle)
        {
            var i = raw.IndexOf(needle, StringComparison.Ordinal);
            Assert.True(i >= 0, $"the symptoms section has lost '{needle}'");
            return i;
        }

        var etv = At("**If you have had an ETV.**");
        var etvVenue = At("needs urgent care, from your doctor or the emergency");
        var dry = At("**If you cannot drink enough to keep up, or no longer feel thirsty.**");
        var dryVenue = At("going to the hospital as soon as possible");
        var salt = At("**If you take the medicine for the thirst, too much water is a danger too.**");
        var saltVenue = At("advice is 911, or the");
        var adrenal = At("**If your body no longer makes enough of its own stress hormone.**");
        var adrenalVenue = At("need emergency treatment");

        Assert.True(etv < etvVenue && etvVenue < dry, "the ETV venue has left its own rule");
        Assert.True(dry < dryVenue && dryVenue < salt, "the drying-out venue has left its own rule");
        Assert.True(salt < saltVenue && saltVenue < adrenal, "the low-salt venue has left its own rule");
        Assert.True(adrenal < adrenalVenue, "the adrenal venue must follow its own sign");

        // THE ROUND-1 UNDER-TRIAGE FIXES ARE PINNED IN THEIR OWN PARAGRAPHS (/review
        // round 2 found them unguarded). Slow weakness keeps its same-day tier AND the
        // ambulance tier for the sudden kind; new sleepiness keeps the right-away tier
        // for anyone with a shunt or an ETV, which is common with pineal tumors.
        var slowWeakness = Regex.Match(raw, @"\*\*Deeper and off to one side\*\*.*?(?=\r?\n\r?\n)",
            RegexOptions.Singleline);
        Assert.True(slowWeakness.Success, "the slow-weakness paragraph has gone");
        Assert.Matches(new Regex(
            @"weakness down one side that comes on suddenly is an\s+ambulance call", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(slowWeakness.Value));

        var life = RawSection(LifeHeading);
        var sleepy = Regex.Match(life, @"\*\*Tiredness is common\*\*.*?(?=\r?\n\r?\n|\z)", RegexOptions.Singleline);
        Assert.True(sleepy.Success, "the tiredness paragraph has gone");
        Assert.Matches(new Regex(
            @"With a shunt or an ETV, it means getting help right away", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(sleepy.Value));

        // EACH RULE NAMES WHO GIVES IT, inside its own paragraph.
        Assert.Contains("Hydrocephalus Association", raw[etv..dry], StringComparison.Ordinal);
        Assert.Contains("Cleveland Clinic", raw[dry..salt], StringComparison.Ordinal);
        Assert.Contains("Cleveland Clinic", raw[salt..adrenal], StringComparison.Ordinal);
        Assert.Contains("St. Jude", raw[adrenal..], StringComparison.Ordinal);

        // THE ADRENAL SOURCE SAYS "EMERGENCY TREATMENT RIGHT AWAY" AND NEVER SAYS 911.
        // Putting 911 in St. Jude's mouth would be a citation that does not support the
        // claim in the form it is made (§12.14). Bounded at the next section so the
        // escalation block's own 911 lines above cannot reach in.
        //
        // WIDENED AT /review ROUND 1: the first version banned only "911", so "call an
        // ambulance" or "go to the emergency room" in St. Jude's mouth passed. The
        // positive half is St. Jude's own verb, the emergency dose "right away".
        var adrenalParagraph = Regex.Match(raw[adrenal..], @"^.*?(?=\r?\n\r?\n)", RegexOptions.Singleline).Value;
        var venue = new Regex(
            @"\b911\b|\bambulance\b|\bemergency (room|department)\b|\bhospital\b|\bER\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(venue, "St. Jude says to go to the hospital");             // canary (round 2)
        Assert.Matches(venue, "St. Jude says to call an ambulance");               // canary
        Assert.Matches(venue, "go to the emergency room for this");                // canary
        Assert.DoesNotMatch(venue, adrenalParagraph);
        Assert.Matches(new Regex(@"emergency dose, which is a shot, is\s+needed right away",
            RegexOptions.IgnoreCase), CuratedPage.Flatten(adrenalParagraph));
    }

    [Fact]
    public void TheWaterRulesRunInBothDirections()
    {
        // TOO LITTLE AND TOO MUCH ARE BOTH DANGERS, and a page carrying only "drink
        // enough" would push a reader on the medicine toward the other emergency.
        // Cleveland Clinic gives both, on one page, with opposite advice.
        var section = Flat(RawSection(SymptomHeading));

        Assert.Matches(new Regex(@"they can dry out quickly", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"too much water is a danger too", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"drinking too much while on it, can lower the salt\s+in\s+your blood",
            RegexOptions.IgnoreCase), section);

        // AND THE CARER IS TOLD THERE ARE TWO, not one.
        Assert.Matches(new Regex(
            @"Too little water and too much can\s+each be dangerous", RegexOptions.IgnoreCase),
            Flat(RawSection(CareHeading)));
    }

    [Fact]
    public void TheScopingNotesComeBeforeTheBlocksTheyQualify()
    {
        // §12.6: warn before you disclose. A caveat placed after the list reads as a
        // correction the reader meets too late (WI-545's first rendered read).
        var symptoms = RawSection(SymptomHeading);
        var scoping = symptoms.IndexOf("Something to know before the list below", StringComparison.Ordinal);
        var escalation = symptoms.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        Assert.True(scoping >= 0 && escalation > scoping,
            "the scored-absence note must come BEFORE the escalation block");

        var where = RawSection(WhereHeading);
        var note = where.IndexOf("Before the general part below", StringComparison.Ordinal);
        var mechanism = where.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(note >= 0 && mechanism > note,
            "the mechanism scope note must come BEFORE the block it scopes");

        // THE NOTE OVERRIDES THE BLOCK'S "A PHONE CALL" ADVICE, which is milder than
        // this page's own rules for the same signs.
        Assert.Matches(new Regex(
            @"the rules in the symptoms section are\s+stronger than the advice here",
            RegexOptions.IgnoreCase), Flat(where));
    }

    [Fact]
    public void ThePageCarriesTheEscalationBlockAndItsTiersAgreeWithTheCorpus() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnTheComposedPage() =>
        // THE POSITIVE HALF names the paragraph this must have examined, and the first
        // version named one the helper never looks at: the growing-teratoma warning has
        // no word from the helper's symptom list, so it was never examined, and the call
        // failed on an EMPTY examined set. That was the positive half working. The page
        // had just been rewritten twice to answer this guard, and after the second
        // rewrite no paragraph paired a sign with a reassurance at all, so the guard
        // examined nothing and would have been green on anything (§12.8, WI-517).
        //
        // The tiredness paragraph is the true pairing: tiredness IS common in treatment,
        // and on this page new sleepiness is also a fluid sign. It keeps the reassurance
        // and carries the same-day tier beside it, which is what this guard asks of a
        // reassurance, not that it be deleted.
        CuratedPage.AssertNoWarningSignIsNormalised(Composed, Slug, "Tiredness is common");

    // -------------------------------------------------------------- the block rulings

    [Fact]
    public void ExactlySixBlocksAreIncludedAndTheTwoExclusionsAreAClosedSet()
    {
        // AN EXACT SET: a block added later fails here and has to be ruled on.
        Assert.Equal(
            ["caregiver", "causes", "escalation", "mechanism", "spinal-cord", "tumor-board"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        Assert.Contains("[MECHANISM]", Section(WhereHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[SPINAL-CORD]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);

        // THE SPINAL-CORD BLOCK IS IN BECAUSE THIS TUMOR REACHES THE SPINE THROUGH THE
        // FLUID, and the page has to say so or the block's "or has spread to the spine"
        // conditional addresses nobody.
        Assert.Matches(new Regex(
            @"These tumors can travel down through the fluid", RegexOptions.IgnoreCase),
            Flat(RawSection(WhereHeading)));
    }

    [Fact]
    public void TheCrosswalkBlockIsExcludedBecauseItsGeneBulletIsFalseHere()
    {
        // THE EXCLUSION A LATER EDITOR IS LIKELIEST TO "CORRECT": fifteen hubs include
        // it. Its gene bullet is FALSE for this tumor: these are named by type, and the
        // name carries no gene result. Its Roman-numeral bullet is not claimed false
        // (round 1 removed the page's unsupported "not graded" claim); it is simply not
        // what a reader of this report needs, and the page answers old names and the
        // grade itself. Asserted against the BLOCK'S OWN WORDS.
        var block = Flat(File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md")));

        foreach (var blockOnly in new[]
                 {
                     "Only the way of writing it did",
                     "Gene results are now part of the name",
                 })
        {
            Assert.Contains(blockOnly, block, StringComparison.Ordinal);
            Assert.DoesNotContain(blockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        }

        Assert.DoesNotContain("crosswalk", ContentBlocks.DirectBlockNames(Page));

        // AND THE PAGE DOES THE BLOCK'S JOB ITSELF: old names, and the missing grade.
        var report = Flat(RawSection(ReportHeading));
        Assert.Matches(new Regex(@"Neither is on the WHO's current list", RegexOptions.IgnoreCase), report);
        Assert.Matches(new Regex(@"A grade on your report", RegexOptions.IgnoreCase), report);

        // A TERATOMA GRADE IS ON A DIFFERENT SCALE (/review round 2): the consensus
        // says the immature part may be graded by a non-WHO system, so a reader holding
        // that grade must not be sent to a section about the WHO's.
        Assert.Matches(new Regex(@"grade on a different scale", RegexOptions.IgnoreCase), report);
    }

    [Fact]
    public void ThePosteriorFossaBlockIsExcludedBecauseThisIsNotCerebellarSurgery()
    {
        var block = Flat(File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "posterior-fossa-syndrome.md")));

        const string BlockOnly = "Most children slowly get better";
        Assert.Contains(BlockOnly, block, StringComparison.Ordinal);
        Assert.DoesNotContain(BlockOnly, CuratedPage.Flatten(Composed), StringComparison.Ordinal);
        Assert.DoesNotContain("posterior-fossa-syndrome", ContentBlocks.DirectBlockNames(Page));
    }

    // ------------------------------------------------------------------ the grade

    [Fact]
    public void NoGradeIsPrintedAndTheWhoGradeFourSentenceIsCarriedWhereItIsNeeded()
    {
        var grade = Flat(RawSection(GradeHeading));

        // /review ROUND 1 BLOCKER, AND THIS GUARD USED TO PIN IT AS TRUE. The first draft
        // said these tumors are "not usually given a grade", on the strength of the WHO
        // summary's grade table leaving them out. That table is "grades of selected
        // types"; medulloblastoma and craniopharyngioma are missing from it too, and the
        // same summary says a germinoma "can be assigned" grade 4. No recorded source
        // says these tumors go ungraded, so the page no longer says it either way.
        Assert.Matches(new Regex(
            @"the type matters more than any grade", RegexOptions.IgnoreCase), grade);
        var ungraded = new Regex(
            @"\b(not (usually )?(given|assigned) a grade|(are|is) not graded|have no grade|leaves? [^.]{0,30}out entirely"
            + @"|do(es)? not (get|have|carry) a grade|no grade is (given|normal|usual)|ungraded"
            + @"|not (usually )?(given|assigned) grades|(is|there is) (usually )?no grade)",
            RegexOptions.IgnoreCase);
        Assert.Matches(ungraded, "these tumors do not get a grade");                   // canary (round 2)
        Assert.Matches(ungraded, "no grade is normal for these tumors");               // canary (round 2)
        Assert.Matches(ungraded, "there is usually no grade");                         // canary (round 3)
        Assert.Matches(ungraded, "these tumors are not usually given a grade");         // canary
        Assert.Matches(ungraded, "germ cell tumors are not graded");                   // canary
        Assert.DoesNotMatch(ungraded, Plain);
        Assert.Matches(new Regex(
            @"can be given grade 4 even when it now has treatments that\s+work well",
            RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"It is not a prediction about you", RegexOptions.IgnoreCase), grade);

        // NOWHERE ELSE DOES A GRADE NUMBER APPEAR. A "grade 4" that escaped the one
        // section that explains it would hand a frightened reader the number without
        // the sentence that defuses it.
        var gradeNumber = new Regex(
            @"\bgrades?(?:[\s-]+|\s+of\s+)(?:[1-4]|I{1,3}|IV|one|two|three|four)\b", RegexOptions.IgnoreCase);
        Assert.Matches(gradeNumber, "it has a grade of 4");            // canary (round 2)
        Assert.Matches(gradeNumber, "this is a grade 4 tumor");       // canary
        Assert.Matches(gradeNumber, "an older report says grade IV"); // canary
        Assert.Matches(gradeNumber, "a grade-four germinoma");        // canary (round 1)
        var elsewhere = CuratedPage.Flatten(CuratedPage.ReaderText(
            Composed.Replace(RawSection(GradeHeading), "")));
        Assert.DoesNotMatch(gradeNumber, elsewhere);
    }

    [Fact]
    public void OnlyTheMatureTeratomaIsSaidNotToBeCancerAndGerminomaNeverIs()
    {
        var grade = Flat(RawSection(GradeHeading));

        Assert.Matches(new Regex(@"A germinoma is a cancer", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"The clearest exception is a mature teratoma", RegexOptions.IgnoreCase), grade);

        // AND THE OTHER KINDS ARE "MOST", NOT "ALL" (/review round 1): "all the others
        // are cancer, with one exception" labelled an IMMATURE teratoma a cancer while
        // the cited source groups it with the mature kind.
        Assert.Matches(new Regex(@"Most of the other kinds are cancers too", RegexOptions.IgnoreCase), grade);

        // CLEVELAND CLINIC'S "GERMINOMAS CAN BE BENIGN" IS CONTRADICTED BY THE WHO AND
        // MUST NOT ARRIVE. And the source that excludes teratomas names BOTH mature and
        // immature, while only the mature half is uncontested, so "immature" never gets
        // the not-cancer claim either.
        var benignGerminoma = new Regex(
            @"germinoma\w*[^.]{0,60}\b(benign|not (always )?(a )?cancer|noncancerous)\b", RegexOptions.IgnoreCase);
        Assert.Matches(benignGerminoma, "a germinoma is not always cancer");                // canary (round 1)
        Assert.Matches(benignGerminoma, "germinomas can be benign");                        // canary
        Assert.Matches(benignGerminoma, "a germinoma is sometimes not cancer at all");      // canary
        Assert.DoesNotMatch(benignGerminoma, Plain);

        var immatureNotCancer = new Regex(
            @"immature teratoma\w*[^.]{0,60}\b(benign|not (a )?cancer|noncancerous)\b", RegexOptions.IgnoreCase);
        Assert.Matches(immatureNotCancer, "an immature teratoma is not cancer");            // canary
        Assert.DoesNotMatch(immatureNotCancer, Plain);
    }

    // --------------------------------------------------------------------- diagnosis

    [Fact]
    public void TheMarkerCutOffsAreNotPrintedAndTheReaderIsSentToTheirTeam()
    {
        var diagnosis = Flat(RawSection(DiagnosisHeading));

        Assert.Matches(new Regex(@"draw that line in different places", RegexOptions.IgnoreCase), diagnosis);
        Assert.Matches(new Regex(@"this page\s+does not print a number", RegexOptions.IgnoreCase), diagnosis);

        // NO THRESHOLD UNITS AND NO NUMBER BESIDE A MARKER, anywhere the reader looks.
        var cutoff = new Regex(
            @"\b(ng/mL|IU/L|mIU)\b|\b(AFP|hCG)\b[^.]{0,30}\b\d+\b", RegexOptions.IgnoreCase);
        Assert.Matches(cutoff, "an AFP of 8 is normal");                   // canary (round 1)
        Assert.Matches(cutoff, "an AFP above 25 counts as high");        // canary
        Assert.Matches(cutoff, "beta-hCG over 50 IU/L");                  // canary
        Assert.DoesNotMatch(cutoff, Plain);
    }

    [Fact]
    public void TheBiopsyRuleKeepsItsNamedException()
    {
        // CLEVELAND CLINIC'S "YOU ONLY NEED A BIOPSY IF..." IS THE MISLEADING FORM for a
        // pure germinoma, whose markers are usually not raised. The page carries the
        // consensus rule instead, with the one exception the consensus names.
        var diagnosis = Flat(RawSection(DiagnosisHeading));

        Assert.Matches(new Regex(
            @"only tumors without raised markers should have a piece taken", RegexOptions.IgnoreCase), diagnosis);
        Assert.Matches(new Regex(
            @"a tumor in both of the usual spots, with\s+markers that are not raised, is treated as a germinoma",
            RegexOptions.IgnoreCase), diagnosis);
        Assert.DoesNotMatch(new Regex(
            @"only need a biopsy if", RegexOptions.IgnoreCase), Plain);

        // THE MARKERS-ALONE RULE KEEPS ITS SCOPE (/review round 1). The consensus states
        // it under "Strategy for NGGCT". Placed beside a germinoma sentence it read as
        // markers naming a germinoma, which is the misreading ruling 3 exists to stop.
        //
        // AND ROUND 2 FOUND THE ROUND-1 FIX TOO STRONG: "high markers point to the other
        // kinds, NOT to a germinoma" contradicts the consensus, which says a germinoma
        // can raise hCG a little and that there is no agreed line between the two. A
        // family told their germinoma has a raised hCG would read it as a wrong
        // diagnosis. So VERY high markers point elsewhere, and the overlap is said.
        Assert.Matches(new Regex(
            @"Very\s+high markers point to one of the other kinds", RegexOptions.IgnoreCase), diagnosis);
        Assert.Matches(new Regex(
            @"A germinoma can\s+raise beta-hCG a little too", RegexOptions.IgnoreCase), diagnosis);
        Assert.DoesNotMatch(new Regex(@"not to a germinoma", RegexOptions.IgnoreCase), Plain);
        var markersNameGerminoma = new Regex(
            @"\bmarkers?\b[^.]{0,60}\b(name|diagnos\w*|identif\w*|confirm\w*|show|tell|point\w*|call)\b[^.]{0,30}\ba germinoma\b"
            + @"|\ba germinoma\b[^.]{0,30}\b(named|diagnosed|identified|confirmed)\b[^.]{0,20}\bmarkers?\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(markersNameGerminoma, "the markers alone can diagnose a germinoma");  // canary
        Assert.Matches(markersNameGerminoma, "markers alone can tell you it is a germinoma"); // canary (round 2)
        Assert.Matches(markersNameGerminoma, "a germinoma can be named by its markers");     // canary (round 2)
        Assert.Matches(markersNameGerminoma, "high markers point to a germinoma");           // canary (round 3: the page's own verb)
        Assert.DoesNotMatch(markersNameGerminoma, Plain);
    }

    [Fact]
    public void TheSpinalTapIsExplainedHereAndNothingLinksToAPageThatDoesNotExist()
    {
        // WI-552 IS UNBUILT (Gate 1, following /tumors/atrt). When it ships, this goes
        // red on purpose: the door then has to be added, and this test rewritten to
        // assert it.
        var diagnosis = Flat(RawSection(DiagnosisHeading));

        Assert.Matches(new Regex(
            @"no separate page on this site about the spinal tap yet", RegexOptions.IgnoreCase), diagnosis);
        Assert.Matches(new Regex(@"a scan is usually done first", RegexOptions.IgnoreCase), diagnosis);
        Assert.DoesNotMatch(new Regex(
            @"\]\(/tests/(lumbar|spinal)[^)]*\)", RegexOptions.IgnoreCase), Page);
        // GLOBBED, NOT A GUESSED FILE NAME (/review round 1): the backlog fixes no slug,
        // so a check for one name would never trip on "spinal-tap.md".
        var testsPages = Path.Combine(CuratedPage.BlocksRoot, "..", "pages", "tests");
        var tapPages = Directory.EnumerateFiles(testsPages, "*.md")
            .Where(f => Regex.IsMatch(Path.GetFileName(f), "lumbar|spinal|tap|csf", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(tapPages.Count == 0,
            $"a spinal tap page now exists ({string.Join(", ", tapPages)}): add the door to it and rewrite this test");
    }

    [Fact]
    public void TheThirstDelayIsExplainedAndNoScheduleIsGiven()
    {
        var delay = Flat(Subsection(SymptomHeading, "Why the thirst can come years before the scan shows anything"));

        Assert.Matches(new Regex(@"\*\*years\*\* before the tumor is found|years before the tumor is found",
            RegexOptions.IgnoreCase), delay);
        Assert.Matches(new Regex(
            @"a spinal fluid test showed a tumor substance before the\s+scan did", RegexOptions.IgnoreCase), delay);

        // BOTH SCOPES KEPT (/review round 1). The consensus says it of thirst "with
        // isolated pituitary stalk thickening", and the study was of young people
        // LATER FOUND TO HAVE A GERMINOMA. Without them the passage reads as "this
        // thirst means a tumor", which is the frightening misreading.
        Assert.Matches(new Regex(@"when all a\s+scan shows is a thicker stalk", RegexOptions.IgnoreCase), delay);
        Assert.Matches(new Regex(
            @"nine young people\s+who were later found to have a germinoma", RegexOptions.IgnoreCase), delay);
        Assert.Matches(new Regex(
            @"A clear first scan is where the checking starts", RegexOptions.IgnoreCase), delay);

        // A FINDING IS NOT A SCHEDULE. "Within 14 months" reports what one study saw;
        // "every six months" would be an instruction no patient source gives.
        var schedule = new Regex(
            @"\bevery (\d+|few|two|three|four|six|twelve)( to (\d+|six|twelve))? (weeks?|months?|years?)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(schedule, "scan every 6 months for 5 years");   // canary
        Assert.Matches(schedule, "a scan every few months");           // canary (round 1)

        // AND THE YEARLY FORMS, WITHOUT THE BARE "every year" (round 2): Boston's
        // survivorship-clinic line uses that legitimately, and it is a visit, not a scan.
        var yearlyScan = new Regex(@"\b(scan|MRI)s?\b[^.]{0,30}\b(yearly|each year|once a year|every year)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(yearlyScan, "an MRI once a year");              // canary
        Assert.Matches(yearlyScan, "an MRI every year");               // canary (round 3: the clinic line is a sentence away)
        Assert.DoesNotMatch(yearlyScan, Plain);
        Assert.DoesNotMatch(schedule, Plain);
    }

    // --------------------------------------------------------------------- treatment

    [Fact]
    public void FertilityIsTheFirstThingInTheTreatmentSection()
    {
        // RULING: DECIDED BEFORE TREATMENT, SO IT SITS BEFORE TREATMENT. Position is
        // the property: moved below the treatment plans it reads as an afterthought to
        // a reader who stops at the plan for their kind.
        var raw = RawSection(TreatmentHeading);
        var subheadings = Regex.Matches(raw, @"(?m)^### (.+?)\s*$").Select(m => m.Groups[1].Value).ToList();
        Assert.Equal("Before treatment starts: saving eggs or sperm", subheadings.FirstOrDefault());

        var fertility = Flat(Subsection(TreatmentHeading, "Before treatment starts: saving eggs or sperm"));
        Assert.Matches(new Regex(@"may take 1 to 2 weeks", RegexOptions.IgnoreCase), fertility);

        // THE PART OF THE SOURCE'S LIST THAT MATTERS MOST HERE (/review round 1): the
        // pituitary is where these tumors sit, and dropping it understated the risk.
        Assert.Matches(new Regex(@"especially near the hormone gland", RegexOptions.IgnoreCase), fertility);
        Assert.Matches(new Regex(@"they belong in this\s+conversation", RegexOptions.IgnoreCase), fertility);

        // THE HOPEFUL HALF IS SCOPED TO ITS MECHANISM: hormone treatment fixes signal
        // damage only when the organs themselves are undamaged.
        Assert.Matches(new Regex(
            @"when the ovaries or testicles themselves are\s+not damaged, hormone treatment helps fix this",
            RegexOptions.IgnoreCase), fertility);
    }

    [Fact]
    public void EachKindKeepsItsOwnPlanUnderItsOwnSubheading()
    {
        // TWO TUMORS, TWO PLANS. Presence anywhere in the section would pass with the
        // germinoma sentence under the other heading, which is the defect.
        var germinoma = Flat(Subsection(TreatmentHeading, "A germinoma"));
        var others = Flat(Subsection(TreatmentHeading, "The other kinds"));

        Assert.Matches(new Regex(
            @"chemotherapy is usually added so less\s+radiation is needed", RegexOptions.IgnoreCase), germinoma);
        Assert.Matches(new Regex(@"removing a germinoma is generally not needed", RegexOptions.IgnoreCase), germinoma);
        Assert.Matches(new Regex(@"Chemotherapy alone is not the\s+standard", RegexOptions.IgnoreCase), germinoma);

        Assert.Matches(new Regex(
            @"Chemotherapy comes first, then an operation on anything left, then\s+radiation",
            RegexOptions.IgnoreCase), others);
        // NOT "NOT LIVE TUMOR" (/review round 1): a teratoma IS live tumor, and this
        // page's own growing-teratoma section says it keeps growing. The claim is that
        // it is often not the CANCER part, and that it still has to come out.
        Assert.Matches(new Regex(@"often not the cancer part", RegexOptions.IgnoreCase), others);
        Assert.Matches(new Regex(@"That still has to\s+come out", RegexOptions.IgnoreCase), others);
        Assert.DoesNotMatch(new Regex(@"not live tumor", RegexOptions.IgnoreCase), Plain);

        // AND NEITHER CARRIES THE OTHER'S DEFINING CLAIM.
        Assert.DoesNotMatch(new Regex(@"operation on anything left", RegexOptions.IgnoreCase), germinoma);
        Assert.DoesNotMatch(new Regex(@"generally not needed", RegexOptions.IgnoreCase), others);
    }

    [Fact]
    public void GrowingDuringTreatmentCarriesBothHalvesInOneSubsection()
    {
        // BOTH HALVES OR NEITHER. The first alone ("growth is not failure") would teach
        // a family to discount a real warning; the second alone would frighten every
        // family whose scan grows while the markers fall. Scoped to the subsection so
        // one half elsewhere cannot stand in for it.
        var gts = Flat(Subsection(TreatmentHeading, "If the tumor grows during treatment"));

        Assert.Matches(new Regex(@"\*\*not\*\* a sign that chemotherapy has failed|not a sign that chemotherapy has failed",
            RegexOptions.IgnoreCase), gts);

        // THE HEDGE IS THE CLAIM (/review round 1): without "always", "has not stopped
        // responding" tells every family whose tumor grows that nothing is wrong.
        Assert.Matches(new Regex(@"has not always stopped responding", RegexOptions.IgnoreCase), gts);
        Assert.Matches(new Regex(@"Falling marker levels never cancel new symptoms", RegexOptions.IgnoreCase), gts);
        Assert.Matches(new Regex(
            @"tell your team, even if\s+the blood tests look better", RegexOptions.IgnoreCase), gts);
    }

    [Fact]
    public void TheProtonPassageClaimsNoSuperiority()
    {
        var radiation = Flat(Subsection(TreatmentHeading, "Radiation, and protons"));

        Assert.Matches(new Regex(@"protons worked about as well as standard radiation", RegexOptions.IgnoreCase), radiation);
        Assert.Matches(new Regex(@"protons have not been shown to be\s+better here", RegexOptions.IgnoreCase), radiation);

        // WIDENED AT /review ROUND 1. The first list let "fewer side effects", "spares
        // more healthy brain" and "better, as studies have shown" through (the last via
        // a lookahead meant to exempt the negated sentence). The meta-analysis could not
        // compare side effects OR normal-tissue exposure, so every one of those is
        // unsupported. The two negated sentences are now removed EXACTLY, not exempted
        // by pattern.
        var superiority = new Regex(
            @"\b(protons?|proton therapy)\b[^.]{0,60}\b(better|safer|gentler|superior|kinder|spares?|sparing|fewer"
            + @"|less (damag\w*|harm\w*)|lower risk|easier|protect\w*)\b"
            + @"|\bpromising\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(superiority, "protons are better for young people");                // canary
        Assert.Matches(superiority, "a promising new kind of radiation");                  // canary
        Assert.Matches(superiority, "proton therapy, which spares more healthy brain");    // canary (round 1)
        Assert.Matches(superiority, "protons are better, as studies have shown");          // canary (round 1)
        Assert.Matches(superiority, "proton therapy, sparing healthy brain");              // canary (round 2)
        Assert.Matches(superiority, "protons cause less harm");                            // canary (round 2)
        Assert.Matches(superiority, "protons are less harmful");                           // canary (round 3)
        var withoutNegations = Plain
            .Replace("So protons have not been shown to be better here.", "")
            .Replace("Whether they cause fewer problems later is still being studied.", "");
        Assert.DoesNotMatch(superiority, withoutNegations);

        // §12.4 R1: no Gy anywhere.
        Assert.DoesNotMatch(new Regex(@"\bGray\b|\d\s*c?Gy\b"), Plain);
    }

    // ------------------------------------------------------------- numbers and outlook

    [Fact]
    public void NoFigureAndNoCureWordAppearsOutsideTheOutlookGate()
    {
        // RULING 8 (§12.12). Nearly every source prints a cure figure, and several say
        // it "rarely" comes back. Cure words are allowed ONLY behind the reader's-choice
        // gate, where the reader has chosen to read about outlook.
        var outside = OutsideTheGate();

        var cure = new Regex(@"\bcur(e|es|ed|ing|able|ative)\b", RegexOptions.IgnoreCase);
        Assert.Matches(cure, "curing it is the aim");              // canary (round 1)
        Assert.Matches(cure, "radiation alone is curative");       // canary
        Assert.Matches(cure, "most germinomas are curable");       // canary
        Assert.DoesNotMatch(cure, outside);

        // THE GATE REALLY DOES CARRY ONE, so this guard is not green because the word
        // is gone everywhere (the negative canary).
        var gate = Regex.Match(Page, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.Matches(cure, gate.Groups[1].Value);

        foreach (var banned in new[]
                 {
                     "survival rate", "5-year", "five-year", "10-year", "median survival",
                     "life expectancy", "rarely come back", "rarely comes back", "virtually",
                 })
        {
            Assert.DoesNotContain(banned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // NO PERCENTAGE ANYWHERE. This page has no legitimate one to print.
        var percent = new Regex(@"\d\s*(%|percent)", RegexOptions.IgnoreCase);
        Assert.Matches(percent, "about 90% are cured");           // canary
        Assert.DoesNotMatch(percent, Plain);
    }

    [Fact]
    public void TheOutlookGateTeachesTheConceptsAndCarriesItsCounterweight()
    {
        var gate = Regex.Match(Page, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook section has lost its reader-choice gate");
        // ReaderText expects a page and strips everything up to the closing front-matter
        // fence, so the gate's inside is wrapped in an EMPTY front matter to be read as
        // body text rather than swallowed as YAML.
        var inside = CuratedPage.Flatten(CuratedPage.ReaderText("---\n---\n" + gate.Groups[1].Value));

        Assert.Matches(new Regex(@"describe groups, not you", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"half\s+of a group did better and half did worse", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"older than it looks", RegexOptions.IgnoreCase), inside);

        // THE COUNTERWEIGHT TO THE HOPEFUL OPENING, and it is the reason the gate is
        // worth opening: the question becomes what treatment leaves behind.
        Assert.Matches(new Regex(@"how well, not only how long", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"harder to treat", RegexOptions.IgnoreCase), inside);

        Assert.DoesNotContain(OutlookHeading, gate.Groups[1].Value, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------- the report

    [Fact]
    public void TheOldNamesAreCarriedAndAtypicalTeratomaIsToldApartFromAtrt()
    {
        var report = Flat(RawSection(ReportHeading));

        // THE LOOK-ALIKE NAME, ADJACENT TO ITS LINK. A reader who searched the old
        // name may have landed on ATRT; the disambiguation has to sit where the link is.
        Assert.Matches(new Regex(
            @"Atypical teratoma is not ATRT\.\*\*\s*\[ATRT\]\(/tumors/atrt\) is a different tumor",
            RegexOptions.IgnoreCase), CuratedPage.Flatten(RawSection(ReportHeading)));

        // PINEALOMA NAMES A PLACE, NOT A TYPE (the NCI dictionary counts non-germ-cell
        // tumors under it), so the page must never say it IS a germinoma.
        Assert.Matches(new Regex(@"ask which tumor it\s+really means", RegexOptions.IgnoreCase), report);
        var pinealomaIsGerminoma = new Regex(
            @"\bpinealoma\b(?! and)[^.]{0,40}\b(is|means|was)\b[^.]{0,20}\bgerminoma\b", RegexOptions.IgnoreCase);
        Assert.Matches(pinealomaIsGerminoma, "a pinealoma is another name for a germinoma");   // canary
        Assert.DoesNotMatch(pinealomaIsGerminoma, report);

        // THE WHO'S LIST, AND NGGCT IS NOT ON IT.
        Assert.Matches(new Regex(@"not\s+one of the WHO's own type names", RegexOptions.IgnoreCase), report);
    }

    // ------------------------------------------------------------------ causes

    [Fact]
    public void TheSelfBlameSectionAnswersAndCreditsItsSources()
    {
        var causes = Flat(RawSection(CauseHeading));

        Assert.Matches(new Regex(@"For this tumor, the answer is no", RegexOptions.IgnoreCase), causes);
        Assert.Matches(new Regex(@"American Cancer Society says", RegexOptions.IgnoreCase), causes);
        Assert.Matches(new Regex(
            @"found only very rarely in more than one family member", RegexOptions.IgnoreCase), causes);

        // THE BORN-WITH CONDITIONS ARE NAMED WITHOUT BLAME, in the same breath.
        Assert.Matches(new Regex(
            @"They are not something anyone did", RegexOptions.IgnoreCase), causes);
    }

    // ------------------------------------------------------------- the shared prose gates

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        // OPT-IN SHARED GUARDS, called explicitly (WI-544 /review round 1 found a hub
        // calling neither). On the COMPOSED page.
        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Composed), Slug);

        Assert.NotEmpty(CuratedPage.Characterisations);
        Assert.Contains("aggressive", CuratedPage.Characterisations, StringComparer.OrdinalIgnoreCase);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        // A Canadian hospital page is cited for DI permanence, and the UK Pituitary
        // Foundation was read and rejected for "999" and "A&E". Strip-then-scan.
        var reader = Plain;
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        Assert.NotEmpty(CuratedPage.BritishForms);
        Assert.Contains("tumour", CuratedPage.BritishForms, StringComparer.OrdinalIgnoreCase);

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // "being sick" means unwell in US English, not vomiting; this page's first
        // draft carried "being sick to your stomach" on a drying-out rule and was
        // rewritten before it ever ran.
        foreach (var idiom in new[]
                 {
                     "A&E", "straight away", "straight after", "out of hours", "being sick",
                     "come round", "car park", "999", "steroid-dependant", "GP",
                 })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST. The first draft shared 26 windows with seven files, a quoted
        // American Cancer Society sentence among them, and every one was rewritten.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    // -------------------------------------------------------------- the front matter

    [Fact]
    public void TheSourcesAreRealAndTheBarredOnesAreNotCited()
    {
        var front = CuratedPage.FrontMatter(Page);

        foreach (var barred in new[]
                 {
                     "mayoclinic.org", "mhsystem.org", "ahfs", "medlineplus.gov/druginfo",
                     "cancer.gov/types/brain", "academic.oup.com", "webapis.cancer.gov",
                 })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }

        // §12.1: NO NCI PATIENT OR PROFESSIONAL TREATMENT SUMMARY. A domain list cannot
        // express that (WI-528), so it gets a pattern. The dictionary is cited and must
        // NOT match, which is the negative canary.
        var pdq = new Regex(@"cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", RegexOptions.IgnoreCase);
        Assert.Matches(pdq, "https://www.cancer.gov/types/brain/hp/child-cns-germ-cell-treatment-pdq");
        Assert.Matches(pdq, "https://webapis.cancer.gov/glossary/v1/Terms/Cancer.gov/Patient/en/germinoma");
        Assert.DoesNotMatch(pdq, front);
        Assert.Contains("cancer.gov/publications/dictionaries", front, StringComparison.OrdinalIgnoreCase);

        // NO BRACKETED BLOCK NAME ANYWHERE IN FRONT MATTER (CaregiverSectionTests).
        Assert.DoesNotMatch(new Regex(@"\[[A-Z][A-Z-]+\]"), front);

        // THE GAPS ARE RECORDED WHERE THE NEXT READER WILL SEE THEM.
        Assert.Matches(new Regex(@"HTTP 403", RegexOptions.IgnoreCase), front);
        Assert.Matches(new Regex(@"HTTP 429", RegexOptions.IgnoreCase), front);
        Assert.Matches(new Regex(@"WI-552", RegexOptions.IgnoreCase), front);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.True(urls.Count >= 20, $"only {urls.Count} sources recorded");
        Assert.All(urls, u => Assert.StartsWith("https://", u));
        Assert.Equal(urls.Count, urls.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void ThePediatricPageCarriesTheDoorToThisPage()
    {
        // THE ONE SIBLING EDIT (Gate 1 decision C). A link nothing asserts is a link the
        // next editor can delete. ADJACENCY: the href sits in the sentence making the
        // age claim, beside ATRT's, which is the contrast the sentence draws.
        var pediatric = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "pediatric-brain-tumor.md")));

        Assert.Matches(new Regex(
            @"\[germ cell tumor in the brain\]\(/tumors/cns-germ-cell-tumor\) runs the other\s+way: it is found most often around the early teenage years",
            RegexOptions.IgnoreCase), pediatric);

        // AND ITS SOURCE IS ON THAT PAGE, not only on this one: a sibling's front matter
        // is not the page's source (WI-544).
        Assert.Contains(
            "together.stjude.org/en-us/conditions/cancers/germ-cell-tumors-brain.html",
            CuratedPage.FrontMatter(CuratedPage.Read("tumors", "pediatric-brain-tumor.md")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheSupportSectionSaysNoTumorSpecificGroupWasFound()
    {
        // RECORDED ABSENCE, SAID OUT LOUD, so a reader does not go looking for a group
        // this page implies exists.
        var support = Flat(RawSection(SupportHeading));
        Assert.Matches(new Regex(
            @"We did not find a US support group just for this tumor", RegexOptions.IgnoreCase), support);
        Assert.Matches(new Regex(@"meet every other week", RegexOptions.IgnoreCase), support);
    }
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class CnsGermCellTumorPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/cns-germ-cell-tumor";

    private readonly WebApplicationFactory<Program> _factory;

    public CnsGermCellTumorPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    private HttpClient Client => _factory.CreateClient();

    [Fact]
    public async Task ThePageIsServedAtItsUrl()
    {
        var response = await Client.GetAsync(Url);
        Assert.True(response.IsSuccessStatusCode, $"{Url} returned {(int)response.StatusCode}");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("CNS germ cell tumor", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The short version", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSixBlocksComposeAndTheTwoExcludedOnesLeaveNoTrace()
    {
        // THE HALF THE CONTENT GUARDS CANNOT SEE: they read directive NAMES from the
        // source, so they stay green if composition breaks. Every string is tooltip-free.
        var html = await Client.GetStringAsync(Url);

        foreach (var composed in new[]
                 {
                     "Call an ambulance",                          // ESCALATION
                     "The skull is a closed box",                  // MECHANISM
                     "right-away call, not a same-day one",        // SPINAL-CORD
                     "You are allowed to ask questions",           // CAREGIVER
                     "nobody knows the cause",                     // CAUSES
                     "What they decide is advice, not an order",   // TUMOR-BOARD
                 })
        {
            Assert.Contains(composed, html, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var excluded in new[]
                 {
                     "Only the way of writing it did",             // CROSSWALK
                     "Most children slowly get better",            // POSTERIOR-FOSSA-SYNDROME
                 })
        {
            Assert.DoesNotContain(excluded, html, StringComparison.OrdinalIgnoreCase);
        }

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
        // NOT VACUOUS: the composed escalation block deep-links the fever rule and the
        // shunt warning signs.
        await CuratedPage.AssertFragmentLinksResolve(Client, Url);

    [Fact]
    public async Task TheTwoSuppressedTooltipsStayQuietAndTheGlossaryStillFiresElsewhere()
    {
        // TWO SUPPRESSIONS, BOTH RULINGS.
        //
        // TUMOR BOARD: the block defines a tumor board in nearly the glossary's words,
        // so the popover would print the definition above itself (§12.8's echo shape).
        //
        // HYDROCEPHALUS: THE FIRST RENDERED READ FOUND IT FIRING INSIDE A NAME. The page
        // credits the Hydrocephalus Association, and the popover split that name in two
        // mid-sentence, on the ETV rule of all places. It was the ONLY popover on the
        // whole page, so suppressing it costs the reader nothing: every other use of the
        // word here is link text, which the glossary does not decorate.
        //
        // The absence anchors are GLOSSARY-ONLY wording, proved to exist first, or
        // rewording a glossary file would retire this guard.
        var html = CuratedPage.Flatten(await Client.GetStringAsync(Url));
        var glossary = Path.Combine(CuratedPage.BlocksRoot, "..", "glossary");

        var board = CuratedPage.Flatten(File.ReadAllText(Path.Combine(glossary, "tumor-board.md")));
        var hydro = CuratedPage.Flatten(File.ReadAllText(Path.Combine(glossary, "hydrocephalus.md")));
        const string HydroDefinition = "Fluid building up inside the brain because it cannot drain the usual way";

        foreach (var (file, glossaryOnly) in new[]
                 {
                     (board, "You do not attend"),
                     (board, "advice for your own doctor to talk through with you"),
                     (hydro, HydroDefinition),
                 })
        {
            Assert.Contains(glossaryOnly, file, StringComparison.Ordinal);
            Assert.DoesNotContain(glossaryOnly, html, StringComparison.Ordinal);
        }

        // THE NAME RENDERS WHOLE, which is the defect the suppression exists to fix.
        Assert.Contains("The Hydrocephalus Association says an ETV can close", html, StringComparison.Ordinal);

        // AND THE BLOCK STILL COMPOSES, so the tumor-board absence is not passing
        // because the whole section vanished.
        Assert.Contains("What they decide is advice, not an order", html, StringComparison.Ordinal);

        // THE PAIRED POSITIVE, AND IT HAS TO LIVE ON ANOTHER PAGE. Suppression is
        // page-wide, so an absence-only test would pass just as happily if the glossary
        // stopped firing altogether (§12.10, WI-514). This page has no unsuppressed
        // popover left to prove that with, so the proof is the same definition still
        // firing on /tests/ct-scan, the one page that uses the word unsuppressed.
        Assert.Contains(HydroDefinition,
            CuratedPage.Flatten(await Client.GetStringAsync("/tests/ct-scan")), StringComparison.Ordinal);

        foreach (var marker in new[] { "!%", "%%" })
        {
            Assert.DoesNotContain(marker, await Client.GetStringAsync(Url), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndexAndThePediatricHub()
    {
        // WI-412 shipped a page nothing linked to. Two doors: the index (taxonomy.yml
        // and the listing agreeing) and the sibling this item edited.
        Assert.Contains("/tumors/cns-germ-cell-tumor", await Client.GetStringAsync("/tumors"),
            StringComparison.Ordinal);
        Assert.Contains("/tumors/cns-germ-cell-tumor",
            await Client.GetStringAsync("/tumors/pediatric-brain-tumor"), StringComparison.Ordinal);
    }
}
