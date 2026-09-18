using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-540: craniopharyngioma, deepened. A §12.3 SEVENTEEN-SECTION TUMOR HUB, and
/// the hub whose nearest neighbour is its biggest hazard: /tumors/pituitary-tumor
/// sits in the same small space and threatens the same two things, so almost
/// every ruling here had to be RE-DERIVED from sources rather than inherited.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * THE TWO TIERS, AND THEIR RANKING. This tumor's emergencies are NOT the
///     neighbour's. Acute obstructive hydrocephalus and adrenal crisis are what
///     the sources converge on; bleeding into the tumor ("CP apoplexy") is real
///     and named for THIS tumor (PMC8963871) but is verified "a very rare
///     syndrome" and is absent from BOTH general reference chapters. So it is
///     named, kept rare, and must never grow a tier of its own here.
///   * THE FLUID TIER'S ENTRY IS A DISJUNCTION. "arrive quickly, OR arrive
///     together" — narrowing that to AND would exclude the commonest
///     presentation, which is the axis /review round 3 caught on WI-539 and
///     which no per-sign assertion can see.
///   * "benign" used as comfort. Three sources refuse to let the word stand
///     alone ("histological benign, but locally aggressive" — AANS).
///   * THE FOUR BLOCK EXCLUSIONS. An excluded block leaves NO TRACE, so nothing
///     but an exact closed set stops a later item restoring one — and two of
///     these exclusions REVERSE the neighbouring hub, which makes them exactly
///     the kind of ruling a later editor would "correct" back.
///   * a prognosis figure, a dose, a hormone threshold, or a scan interval
///     (§12.4, §12.5, and WI-521's no-schedule ruling).
///
/// NO ALLOWLIST. <c>AssertDoesNotRestateTheCorpus</c> is called with no
/// deliberately-shared entries. That is not style: the guard is BIDIRECTIONAL,
/// and this page's first draft turned FOUR shipped pages red. An allowlist here
/// could not have fixed any of them, because their tests never read it. Closing
/// it took six passes and twenty distinct shared runs against the neighbour
/// alone — see .claude/work_files/wi540/all-collisions.py for why a capped
/// report is never an inventory.
/// </summary>
public sealed class CraniopharyngiomaPageContentTests
{
    private const string Slug = "tumors/craniopharyngioma";
    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a craniopharyngioma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string WhereHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string FindOutHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatedHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string BackHeading = "If it comes back";
    private const string CareHeading = "For the person caring for someone with this";

    private static string Page => CuratedPage.Read("tumors", "craniopharyngioma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The title and description, which <c>ReaderText</c> strips and
    /// <c>ContentPage.cshtml</c> renders as the first paragraph a reader meets
    /// (§12.8, WI-524). WI-539 shipped a uniqueness claim here that its own fix
    /// falsified two files away, and nothing guarded it.
    /// </summary>
    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            // `\s*$`, not `"$`: on a CRLF checkout the line ends `"\r\n`, the
            // match fails, and every headline guard runs on an empty string.
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    private static string Everything => Headline + " " + Body;

    /// <summary>Emphasis removed (§12.8, WI-526: a bolded word defeats a phrase guard structurally).</summary>
    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?)";

    private static List<string> Headings() =>
        Regex.Matches(CuratedPage.Body(Page), @"^## (.+?)\s*$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

    // ------------------------------------------------------- the two tiers

    [Fact]
    public void TheFluidTierIsCarriedSignBySignAndEntersOnEitherSpeedOrCombination()
    {
        // THE ITEM'S CENTRAL SAFETY CLAIM, and it is NOT the neighbour's.
        // StatPearls: "When the third ventricle is affected, hydrocephalus can
        // develop" and "External ventricular drainage may be needed in cases
        // presenting acutely due to hydrocephalus". Endotext reports
        // hydrocephalus in a large minority. Together by St. Jude says it in
        // plain words: "As it grows, the tumor may block the flow of
        // cerebrospinal fluid."
        var section = PlainOf(SymptomHeading);

        var tier = Regex.Match(section,
            @"If these signs arrive quickly(.*?)(?=If you take a steroid as replacement)",
            RegexOptions.Singleline);
        Assert.True(tier.Success, "the fluid tier has gone from the symptoms section");
        var fluid = tier.Groups[1].Value;

        // EVERY SIGN, SEPARATELY. A tier that loses one sign loses it silently:
        // the paragraph still reads correctly, every prose assertion still
        // passes, and the reader who had that one sign is the one who does not
        // call (§12.8, WI-539).
        foreach (var sign in new[]
                 {
                     "headache that is getting worse",
                     "throwing up again and again",
                     "much sleepier",
                     "being confused",
                     "sight changing quickly",
                 })
        {
            Assert.Contains(sign, fluid, StringComparison.OrdinalIgnoreCase);
        }

        // WHY it is a rule here rather than a general warning, which is the
        // sentence that makes the tier make sense instead of reading as a
        // louder copy of the block above it.
        Assert.Matches(new Regex(@"sits right where the fluid\s*drains", RegexOptions.IgnoreCase),
            fluid);

        // THE ROUTE, and its STRENGTH. Right away at any hour, explicitly NOT
        // same-day — the shared block files these same signs as same-day, so
        // this tier exists to lift them for this tumor's reader.
        Assert.Matches(new Regex(
            @"right away, at any hour, not a wait for the same day", RegexOptions.IgnoreCase), fluid);
        Assert.Matches(new Regex(
            @"Phone\s*your team whatever the hour, or head for the emergency department",
            RegexOptions.IgnoreCase), fluid);

        // And the ambulance floor is asserted POSITIVELY rather than by banning
        // the vocabulary of lower tiers. §12.8 (WI-539): a guard that bans the
        // demotions its author imagined missed a fall from AMBULANCE to a phone
        // call, because that wording was in none of its branches.
        Assert.Matches(new Regex(@"call 911, or your local emergency number", RegexOptions.IgnoreCase),
            fluid);

        // §12.6: it does not end on the fright.
        Assert.Matches(new Regex(@"treating it early is the whole point", RegexOptions.IgnoreCase),
            fluid);

        // The page routes to the sibling that owns the full list rather than
        // republishing it (§12.10).
        Assert.Contains("/treatments/shunts#warning-signs", fluid, StringComparison.Ordinal);

        // THE ENTRY IS A DISJUNCTION, AND THAT IS THE PROPERTY NO PER-SIGN CHECK
        // CAN SEE. "arrive quickly, OR arrive together" admits the reader with
        // one sign coming on fast AND the reader with several at once. Turning
        // the OR into an AND narrows the rule to people who have both, which is
        // the exact axis /review round 3 found on WI-539 — every sign assertion
        // above would still pass.
        //
        // Asserted over the RAW section, where the emphasis still exists: a
        // reader skimming bold text meets the lead-in and nothing else.
        var entry = Regex.Match(CuratedPage.Flatten(Section(SymptomHeading)),
            @"\*\*(If these signs arrive[^*]*)\*\*");
        Assert.True(entry.Success,
            "the fluid tier no longer opens on a bold lead-in naming its entry condition");

        // `(?:arrive\s+)?` because the likelier narrowing drops the second verb:
        // "arrive quickly AND together" reads more naturally than "arrive quickly
        // and ARRIVE together", and the first version of this regex could only
        // see the clumsier one — a ban list policing the phrasing its author
        // imagined. The disjunction is genuinely held by the positive assertion
        // below; this branch is the cheap half.
        var narrowed = new Regex(@"arrive quickly,?\s+and\s+(?:arrive\s+)?together",
            RegexOptions.IgnoreCase);
        Assert.Matches(narrowed, "If these signs arrive quickly and arrive together, treat them as urgent.");
        Assert.Matches(narrowed, "If these signs arrive quickly and together, treat them as urgent.");
        Assert.False(narrowed.IsMatch(entry.Groups[1].Value),
            "the fluid tier's entry has been narrowed from EITHER condition to BOTH, so a reader "
            + "with one fast-moving sign no longer matches the rule written for them: "
            + entry.Groups[1].Value);

        Assert.Matches(new Regex(@"\bor arrive together\b", RegexOptions.IgnoreCase),
            entry.Groups[1].Value);
    }

    [Fact]
    public void TheAdrenalTierIsConditionalAndItsRouteSitsOutsideTheInjectionClause()
    {
        // THE SECOND EMERGENCY, and the one that is CONDITIONAL — true only of a
        // reader whose body cannot make its own steroid, so the condition has to
        // come FIRST. An unconditional version frightens everybody else on the
        // page (§12.12) and is the WI-534 shunt-clause shape.
        var section = PlainOf(SymptomHeading);

        var tier = Regex.Match(section,
            // Terminates on "Both RULES are worth explaining", not on a frequency
            // sentence — and the word "rules" is load-bearing rather than tidy.
            // Deleting the frequency sentence orphaned the one after it: "Both"
            // had been leaning on "these" in the sentence just removed, so the
            // noun had to come back. Adding precision can strand a pronoun
            // (§12.8, WI-539); taking a sentence away can too.
            // The tier used to close "Neither of these is common",
            // which rendered read 2 caught as an UNSOURCED claim that is also
            // probably false of the first tier: Endotext reports hydrocephalus
            // in a large minority and StatPearls puts headache, nausea and
            // vomiting among the commonest presentations. WI-539 could say
            // "uncommon" because its two events are verified rare; this page
            // cannot, so the claim is gone rather than softened (§12.4).
            @"If you take a steroid as replacement(.*?)(?=Both rules are worth explaining)",
            RegexOptions.Singleline);
        Assert.True(tier.Success, "the adrenal tier has gone from the symptoms section");
        var adrenal = tier.Groups[1].Value;

        // The mechanism in plain words, and the stress list the Endocrine
        // Society gives ("illness, infection, surgery, or an accident") — this
        // page's readers have operations, so the list is not just illness.
        Assert.Matches(new Regex(@"no reserve to draw on", RegexOptions.IgnoreCase), adrenal);
        Assert.Matches(new Regex(@"An infection, an operation or an injury", RegexOptions.IgnoreCase),
            adrenal);

        // ATTRIBUTED, because "it can kill" is the strongest sentence on the
        // page and §12.13 wants the strongest claim carrying its source.
        Assert.Matches(new Regex(
            @"the Endocrine Society says it can kill", RegexOptions.IgnoreCase), adrenal);

        // THE SIGNS, separately. MedlinePlus verified: "Confusion, loss of
        // consciousness, or coma", "Fatigue, severe weakness", "Nausea,
        // vomiting", "Dizziness or lightheadedness".
        foreach (var sign in new[]
                 {
                     "very sick to your stomach", "confusion", "feeling faint", "sudden weakness",
                 })
        {
            Assert.Contains(sign, adrenal, StringComparison.OrdinalIgnoreCase);
        }

        // THE ROUTE MUST SIT OUTSIDE THE INJECTION CLAUSE — WI-539's /review
        // round 2 blocker, and a substring check cannot see the difference. If
        // the only destination lives inside "if you carry an emergency
        // injection", a reader who is fainting and was never given one gets a
        // planning instruction and no destination. MedlinePlus states it of
        // everyone: "Go to the emergency room or call the local emergency
        // number (such as 911) if you develop symptoms of acute adrenal crisis."
        //
        // So the assertion is POSITIONAL.
        var route = adrenal.IndexOf("do not wait: call 911", StringComparison.Ordinal);
        var conditional = adrenal.IndexOf("If you carry an emergency injection",
            StringComparison.Ordinal);
        Assert.True(route >= 0 && conditional > route,
            "the adrenal tier's only route out is inside the 'if you carry an injection' clause, "
            + "so a reader in crisis without one is given no destination");

        Assert.Matches(new Regex(@"A hospital is where this gets treated", RegexOptions.IgnoreCase),
            adrenal);

        // THE EXTENSION IS NOT GATED ON HAVING BEEN TOLD, AND THE CITED SOURCE
        // SAYS SO IN AS MANY WORDS. The Endocrine Society page this front matter
        // carries, verified against the live publication: "Some people don't know
        // they have AI until they have a sudden worsening of symptoms called an
        // adrenal crisis." That same page names the cause this hub's readers have
        // — "pituitary tumors, pituitary surgery, or radiation damage to the
        // pituitary" — which is what the page's "if this growth, or an operation
        // on it, has harmed the gland" rests on. A reader who has been told
        // nothing is the one most likely to explain the signs away.
        //
        // AN EARLIER VERSION OF THIS COMMENT CREDITED NIDDK, WHICH THIS PAGE DOES
        // NOT CITE. The CLAIM was never unsourced; the RATIONALE was wrong — the
        // same false-explanation defect the tumor-index guard was carrying, found
        // by the first rendered read. Checking paid in both directions:
        // MedlinePlus, which this page DOES cite, does not state that a crisis can
        // be the first presentation, so citing it would have been a fresh error
        // rather than a correction.
        //
        // For a later editor: the Endocrine Society also says "Adrenal crisis
        // occurs mainly in people with primary AI", and this tumor's readers have
        // the secondary kind. That is a frequency statement, not an exclusion;
        // this page deliberately implies no frequency either way (§12.4), and the
        // tier rests on StatPearls' mechanism for THIS tumor — thyroid replacement
        // "can increase the metabolic clearance of glucocorticoids and thus may
        // cause an adrenal crisis".
        Assert.Matches(new Regex(
            @"can be yours before anyone has put you on replacement", RegexOptions.IgnoreCase),
            adrenal);

        var gatedOnDiagnosis = new Regex(
            @"only if (?:you|your team) (?:have|has) been told", RegexOptions.IgnoreCase);
        Assert.Matches(gatedOnDiagnosis, "This applies only if you have been told a hormone is low.");
        Assert.DoesNotMatch(gatedOnDiagnosis, adrenal);

        // THE CONDITION OPENS ITS OWN BOLD LEAD-IN, and that is the property
        // rather than its presence somewhere in the paragraph. Read without the
        // condition in front of it, "being ill changes the rules" is aimed at
        // everybody on the page (§12.12). Asserted against the RAW section,
        // where PlainOf would have stripped exactly the thing being checked.
        var lead = Regex.Match(CuratedPage.Flatten(Section(SymptomHeading)),
            @"\*\*(If you take a steroid as replacement[^*]*)\*\*");
        Assert.True(lead.Success,
            "the adrenal rule's conditional clause no longer opens its own bold lead-in, so a "
            + "reader who takes no replacement steroid meets the warning as though it were theirs");

        // A REAL CANARY, run against PLANTED text (§12.8, WI-523). A second
        // negative about the real file would demonstrate nothing about whether
        // the pattern can fire at all.
        var leadPattern = new Regex(@"\*\*(If you take a steroid as replacement[^*]*)\*\*");
        Assert.Matches(leadPattern, "**If you take a steroid as replacement, being ill changes the rules.**");
        Assert.DoesNotMatch(leadPattern, "**Being ill changes the rules.**");
    }

    [Fact]
    public void TheAdrenalRuleReadsAtOneStrengthOnTheSiblingThatOwnsTheMedicine()
    {
        // §12.10, one claim at one strength, across three pages. WI-539 rescoped
        // /treatments/steroids' conditional onto the MECHANISM — "if your body
        // has stopped making its own steroid" — rather than onto a diagnosis,
        // which is precisely why it already reaches THIS page's reader and why
        // this item needed no edit there. That is a ruling worth pinning: if
        // somebody later narrows it back to pituitary surgery, this hub's
        // readers silently lose the rule.
        //
        // READ the sibling rather than asserting a belief about it (§12.10,
        // WI-514: a consistency check that never opens the other file cannot see
        // an inconsistency).
        var steroids = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "steroids.md")));

        Assert.Contains("If your body has stopped making its own steroid", steroids,
            StringComparison.Ordinal);
        Assert.Contains("long enough for your own supply to go quiet", steroids,
            StringComparison.Ordinal);

        // THE CONDITIONAL ESCALATES THE WHOLE LIST, and the guard OPENS THE LIST
        // rather than trusting the claim made about it — WI-539's round-3
        // blocker survived two review rounds because every guard on both pages
        // read the conditional and none read the list.
        var sameDay = Regex.Match(steroids,
            @"Call your team the same day if:(.*?)(?=Call an ambulance)", RegexOptions.Singleline);
        Assert.True(sameDay.Success && sameDay.Groups[1].Value.Length > 200,
            "/treatments/steroids has no readable same-day list, so the conditional above it "
            + "cannot be checked against what it actually escalates");

        foreach (var crisisSign in new[] { "confused", "being sick again and again", "faint" })
        {
            Assert.Contains(crisisSign, sameDay.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("the whole same-day list below is a", steroids, StringComparison.Ordinal);
        Assert.Contains("are what an adrenal crisis looks like", steroids, StringComparison.Ordinal);

        var scopedToOneBullet = new Regex(
            @"\bone (?:item|bullet|line|thing) on the list\b", RegexOptions.IgnoreCase);
        Assert.Matches(scopedToOneBullet, "one item on the list below is different for you");
        Assert.False(scopedToOneBullet.IsMatch(steroids),
            "the replacement-steroid conditional has been narrowed back to a single bullet, while "
            + "that page's same-day list still carries the whole adrenal-crisis triad");

        // And this page routes there, which is what makes one strength reachable
        // rather than merely consistent.
        // AGAINST THE READER'S TEXT, NOT THE RAW FILE. `Page` includes the front
        // matter, which NAMES /treatments/steroids while recording the
        // one-strength ruling — so the raw-file version of this assertion passed
        // whether or not a single body link survived, which is the opposite of
        // what its own comment claimed. This file already documents that trap in
        // the NEGATIVE direction for /treatments/watch-and-wait; this is the same
        // trap in the POSITIVE direction, caught by /review round 2.
        Assert.Contains("/treatments/steroids", Body, StringComparison.Ordinal);
    }

    // --------------------------------------------------------- the apoplexy ruling

    [Fact]
    public void BleedingIntoTheTumorIsNamedAndKeptRareAndNeverGivenATierOfItsOwn()
    {
        // THE RULING THAT MOST DISTINGUISHES THIS HUB FROM ITS NEIGHBOUR, and
        // the one a later editor is most likely to "correct" by promoting it.
        //
        // It IS real for this tumor and nothing is stretched from a
        // pituitary-adenoma source: PMC8963871 defines "CP apoplexy" in a
        // craniopharyngioma series. But it is verified "a very rare syndrome" in
        // that paper's introduction AND conclusion; it rests on one retrospective
        // series; it contains no patient-facing action guidance at all; and BOTH
        // general reference chapters are verified silent on it — StatPearls has
        // no acute section and Endotext never mentions bleeding anywhere.
        //
        // So: named, qualified as uncommon, folded into the fluid tier's action,
        // and WITHOUT a route or tier of its own.
        var section = PlainOf(SymptomHeading);

        Assert.Matches(new Regex(@"bleeds inside itself", RegexOptions.IgnoreCase), section);

        var bleed = Regex.Match(section,
            @"Rarely, one of these growths bleeds inside itself(.*?)(?=If you take a steroid)",
            RegexOptions.Singleline);
        Assert.True(bleed.Success, "the bleeding paragraph has gone from the symptoms section");

        // THE RARITY QUALIFIER IS LOAD-BEARING, and it is what keeps a
        // one-series finding from reading like a headline feature (§12.4 R3,
        // direction only).
        Assert.Matches(new Regex(@"uncommon enough that it is not what a headache usually",
            RegexOptions.IgnoreCase), bleed.Groups[1].Value);

        // AND IT MUST NOT GROW ITS OWN ROUTE. The action is the fluid tier's,
        // deliberately: the source that establishes the event gives no patient
        // action, so inventing one here would be a claim with no citation
        // behind it (§12.2).
        Assert.Matches(new Regex(@"the answer is the same either way", RegexOptions.IgnoreCase),
            bleed.Groups[1].Value);

        var ownRoute = new Regex(
            @"call 911|ambulance|emergency department|right away, at any hour", RegexOptions.IgnoreCase);
        Assert.Matches(ownRoute, "Call 911 if the tumor bleeds.");
        Assert.False(ownRoute.IsMatch(bleed.Groups[1].Value),
            "the bleeding paragraph has grown a route of its own, which promotes a verified-rare "
            + "event to the rank the two tiers hold and rests an instruction on a source that "
            + "gives none");

        // NO FIGURE. The paper puts a percentage on it; it does not reach the page.
        Assert.DoesNotMatch(new Regex(@"\d"), bleed.Groups[1].Value);

        // And the word the neighbour's page owns is NOT borrowed as a label here,
        // because for this tumor it is one paper's proposed term rather than the
        // name a reader will meet.
        Assert.DoesNotContain("apoplexy", Plain, StringComparison.OrdinalIgnoreCase);
    }

    // ----------------------------------------------------------- "benign"

    [Fact]
    public void BenignIsHandledRatherThanUsedAsComfort()
    {
        // The sources hand this page the same dangerous gift they handed WI-539,
        // and the CITED ones refuse to let the word stand alone: AANS
        // ("histological benign, but locally aggressive, tumors that develop near
        // the pituitary gland at the base of the brain") and StatPearls, which
        // has them "often extend to involve critical structures such as the
        // hypothalamus, optic chiasm, cranial nerves, third ventricle, and major
        // blood vessels".
        //
        // AN EARLIER VERSION OF THIS COMMENT ALSO CITED THE PITUITARY NETWORK
        // ASSOCIATION AND PMC12121368, AND NEITHER IS IN THIS PAGE'S FRONT
        // MATTER. They were consulted while drafting and are not carried, so
        // they cannot support anything here (§12.2) — and a comment naming them
        // implies to the next editor that they can. Found by the first rendered
        // read, which is also where the PNA's ONLY legitimate appearance shows
        // up: the support section, as an organisation to go to, not as a source.
        //
        // /tumors/meningioma's STANCE is borrowed; its sentences are not,
        // because the restatement guard objects to borrowed prose and is right to.
        var grade = PlainOf(GradeHeading);

        Assert.Matches(new Regex(@"left on its own\s*it will mislead you", RegexOptions.IgnoreCase),
            grade);
        Assert.Matches(new Regex(@"benign but locally pushy", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"Harmless is a different word", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"can still take part of your\s*sight", RegexOptions.IgnoreCase),
            grade);

        // Never offered as the reassurance, in any of the shapes that do not
        // need the word itself.
        var comfort = new Regex(
            @"\b(?:it is|its|it's) (?:only |just |merely )?benign\b|"
            + @"\bbenign,? so\b|\bgood news:? (?:it is |it's )?benign\b|"
            + @"\bbenign(?:,| and) (?:which means |meaning )?(?:harmless|nothing to worry)\b|"
            + @"\bthe good (?:part|thing|bit|news)\b|"
            + @"\b(?:cannot|can't|will not|won't|is not going to) kill you\b|"
            + @"\bnothing to worry about\b|\bthe lucky (?:one|kind)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(comfort, "The good news is it is benign.");
        Assert.Matches(comfort, "It is benign, so there is nothing to worry about.");
        Assert.Matches(comfort, "The good part is that it is not cancer.");
        Assert.DoesNotMatch(comfort, Plain);
    }

    // -------------------------------------------------- numbers and schedules

    [Fact]
    public void ThePageCarriesNoShareNoDoseAndNoSurveillanceInterval()
    {
        // §12.4, §12.5 and WI-521. The sources are thick with figures — the
        // hydrocephalus range, the endocrine-deficiency shares, the visual
        // percentages, ten-year survival, the incidence, the hemorrhage rate,
        // and milligram replacement doses. Not one reaches this page.
        var share = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b" + CountWord + @"\s+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            + @"\b" + Fraction + @"\s+of\s+(?:all\s+)?(?:the\s+)?(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|tumors|cases|children|adults)\b|"
            + @"\b(?:survival|survive|live for|life expectancy|lifespan)\b|"
            + @"\b\d+(?:\.\d+)?\s*(?:Gy|gray|mg|milligrams?|cm|centimet\w*|mm|mcg)\b|"
            + @"\bmilligram\w*|\bgrays?\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Hydrocephalus is reported in 20% to 38% of people.",
                     "Ten-year survival is about 80% to 90%.",
                     "Vision problems are present in three out of four patients.",
                     "Hydrocortisone 20 to 30 mg is given in divided doses.",
                     "Radiation is given as 54 Gy.",
                 })
        {
            Assert.Matches(share, known);
        }
        Assert.DoesNotMatch(share, Plain);

        // NO PER-TUMOR SCHEDULE (WI-521). This page publishes no interval, and
        // it hands the timetable over rather than being silently silent.
        var interval = new Regex(
            @"\b(?:every|each)\s+" + CountWord + @"?\s*(?:months?|years?|weeks?)\b|"
            + @"\bannual(?:ly)?\b|\b" + CountWord + @"[-\s]*(?:to|or)[-\s]*" + CountWord
            + @"\s*(?:months?|years?)\b|\bevery (?:other )?year\b|"
            + @"\b(?:six|three|twelve)[- ]month\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "A repeat scan every six months, then annually.",
                     "Scans continue every year for ten years.",
                 })
        {
            Assert.Matches(interval, known);
        }
        Assert.DoesNotMatch(interval, Plain);

        Assert.Matches(new Regex(@"Have your own schedule written down", RegexOptions.IgnoreCase),
            PlainOf(ScansHeading));

        // THE RECURRENCE HORIZON IS WORDS, NOT A WINDOW — and this is a ruling
        // rather than an omission. Two children's-hospital sources say
        // recurrences happen "up to two years after surgery", and one of them
        // contradicts itself on its own page by prescribing follow-up yearly to
        // ten years and then indefinitely. Endotext, verified: "Remote
        // recurrences as late as 30 years after initial therapy have been
        // reported." The long horizon is what a reader can act on safely.
        var backSection = PlainOf(BackHeading);
        Assert.Matches(new Regex(@"not only an early risk", RegexOptions.IgnoreCase), backSection);
        Assert.Matches(new Regex(@"reported many years later", RegexOptions.IgnoreCase), backSection);

        var twoYearWindow = new Regex(
            @"up to two years|within two years|for the first two years", RegexOptions.IgnoreCase);
        Assert.Matches(twoYearWindow, "Recurrences can occur up to two years after surgery.");
        Assert.DoesNotMatch(twoYearWindow, Plain);
    }

    [Fact]
    public void TheOutlookGateIsClosedAndCarriesNoFigureAtAll()
    {
        // §12.5. The gate is consent to READ, not a licence to publish.
        var raw = Regex.Match(Page, @"^## What might happen over time.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(raw), "the outlook section has gone");

        // `\r?\n` everywhere: on a CRLF checkout `\s*\n\n` cannot match at all,
        // and a test broken on CRLF fails a break harness for free (§12.8, WI-528).
        Assert.Matches(new Regex(
            @"^## What might happen over time[ \t]*\r?\n[ \t]*\r?\n:::outlook",
            RegexOptions.Multiline), raw);
        Assert.Matches(new Regex(@"\r?\n:::\r?\n"), raw);

        var inside = Regex.Match(raw, @":::outlook(.*?)\r?\n:::", RegexOptions.Singleline)
            .Groups[1].Value;

        // NO DIGIT AT ALL inside the gate. This tumor has no grade to allow in here.
        Assert.DoesNotMatch(new Regex(@"\d"), inside);

        // AND NO FIGURE-SHAPED CLAIM IN WORDS, which is the half a digit ban
        // cannot see (§12.8, WI-527).
        var told = new Regex(
            @"\blive as long as\b|\blife expectancy\b|\blifespan\b|\bsurviv\w*|\bdie of\b|"
            + @"\bcure[ds]?\b|\b(?:almost|nearly) (?:everybody|everyone|all of them)\b|"
            + @"\bmost people\b|\bthe vast majority\b|"
            // CHARACTERISING THE DISTRIBUTION IS PUBLISHING ITS DIRECTION, and
            // the first version of this list could not see that. It banned the
            // vocabulary of survival and missed "the numbers are kinder than the
            // word tumor leads people to expect" — a directional claim about the
            // very figures §12.5 forbids this page to carry, and the first
            // sentence a reader who opted IN to outlook would have met. Caught by
            // /review round 1, which also noted this page was the only gated hub
            // of thirteen that characterised the numbers before explaining them.
            // The property is the comparison, not the words I happened to use.
            + @"\bkinder than\b|\bbetter than (?:you|people|most)\b|\bnot as bad as\b|"
            + @"\bmore hopeful than\b|\bbrighter than\b|\bless frightening than\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(told, "People with this tumor live as long as anybody else.");
        Assert.Matches(told, "For this growth the numbers are kinder than the word tumor suggests.");
        Assert.Matches(told, "The outlook is not as bad as people expect.");
        Assert.DoesNotMatch(told, CuratedPage.Flatten(inside));

        var flat = CuratedPage.Flatten(inside);

        // The median is EXPLAINED rather than used, with the four moves §12.5
        // asks for: it describes a GROUP, it is not a prediction about one
        // person, and the right tail is named rather than hidden.
        Assert.Matches(new Regex(@"the median marks the middle of that line", RegexOptions.IgnoreCase),
            flat);
        Assert.Matches(new Regex(
            @"says nothing about which side you are on", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(@"hides the people\s*who did far better", RegexOptions.IgnoreCase),
            flat);

        // THE PAGE'S OWN ANGLE, which is the reason this gate is not a copy of
        // any sibling's: for this tumor the question a reader is really asking
        // is not how long but how well, and the three things that decide it are
        // the three the rest of the page is about.
        Assert.Matches(new Regex(@"the question is usually not how long", RegexOptions.IgnoreCase),
            flat);

        // And it closes on a door rather than on a fact (§12.6).
        Assert.Matches(new Regex(@"Deciding not to is a choice too", RegexOptions.IgnoreCase), flat);

        // Nothing sits outside the gate in that section but the heading.
        var outside = raw
            .Replace(Regex.Match(raw, @":::outlook.*?\r?\n:::", RegexOptions.Singleline).Value, "",
                StringComparison.Ordinal)
            .Replace("## What might happen over time", "", StringComparison.Ordinal);
        Assert.True(string.IsNullOrWhiteSpace(outside),
            "something sits outside the outlook gate in that section: " + outside.Trim());
    }

    // ------------------------------------------------------ the block rulings

    [Fact]
    public void ExactlyFourBlocksAreIncludedAndTheFourExclusionsAreAClosedSet()
    {
        // THE BLOCKER THAT WOULD BE AN ABSENCE. An excluded block leaves no
        // trace on the page — no heading, no directive, nothing to grep — so a
        // later item can restore one and every other guard in this file stays
        // green.
        //
        // An EXACT SET, not a handful of DoesNotContain checks. A new block
        // added in two years fails here and has to be ruled on deliberately.
        Assert.Equal(
            ["caregiver", "causes", "escalation", "mechanism"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        // Each included one is composed rather than restated, under its own
        // §12.3 heading.
        Assert.Contains("[MECHANISM]", Section(WhereHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);

        // [MECHANISM] IS INCLUDED HERE AND EXCLUDED ON /tumors/pituitary-tumor,
        // AND THE REVERSAL IS THE RULING. That page excluded it because a
        // pituitary tumor is not in the brain, rarely seizes and does not
        // obstruct the fluid pathways. All three differ here: the block's
        // blocked-fluid paragraph is this tumor's central mechanism, its
        // raised-pressure paragraph is close to this tumor's presentation, and
        // seizures are reported. The precedent that settles it is
        // /tumors/meningioma, which is also outside the brain substance and
        // includes the block.
        //
        // Read the SIBLING rather than trusting a belief about it (§12.10).
        Assert.DoesNotContain("mechanism",
            ContentBlocks.DirectBlockNames(CuratedPage.Read("tumors", "pituitary-tumor.md")));
        Assert.Contains("mechanism",
            ContentBlocks.DirectBlockNames(CuratedPage.Read("tumors", "meningioma.md")));

        // THE CROSSWALK BLOCK IS OUT, and for a different reason than the
        // neighbour's. It is wholly the 2021 CNS rewrite: Roman to Arabic, gene
        // results entering the NAME, and NOS/NEC. Only the first applies here —
        // the two genes are diagnostic and are not part of this tumor's name,
        // and this report carries neither other label. Including it would put
        // glioma vocabulary into this reader's identity section (WI-514).
        foreach (var acronym in new[] { "NOS", "NEC", "IDH", "1p/19q" })
        {
            Assert.DoesNotContain(acronym, Plain, StringComparison.Ordinal);
        }

        // Two case policies, two loops — review round 2's S8 on WI-539: a
        // sentence-initial "Molecular" walks straight through a case-sensitive
        // ban, and sentence-initial is exactly where the capital lives.
        foreach (var word in new[] { "molecular", "gene panel", "methylation" })
        {
            Assert.DoesNotContain(word, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // THE TUMOR-BOARD BLOCK IS OUT, decided by reading it rather than by
        // habit: it describes a MEETING about one case, while what the sources
        // describe for this tumor is ongoing care shared between neurosurgery,
        // hormone doctors and eye doctors for years. Different claims.
        Assert.DoesNotContain("tumor board", Plain, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheMechanismBlockIsScopedBecauseItsBrainMapIsNotThisTumorsAnatomy()
    {
        // §12.10's mis-scoped block, in the direction the neighbour did not have
        // to handle. The block is TRUE here and its fluid paragraph is the
        // reason it is included — but it closes with a lobe-by-lobe map of the
        // brain, and this tumor sits in none of those places. A reader who reads
        // that map as a description of their own tumor has been misled by a
        // block the page included on purpose.
        //
        // The remedy is the shape /tumors/all-brain-tumors already uses and
        // which its own test pins: a scoping note ABOVE the directive. Worded
        // deliberately unlike both that page and /tumors/pediatric-brain-tumor,
        // each of which carries its own note and both of which the restatement
        // guard watches.
        var section = PlainOf(WhereHeading);

        Assert.Matches(new Regex(@"written about brain tumors in\s*general", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"The piece that matters most here is the fluid",
            RegexOptions.IgnoreCase), section);
        // "the brain map that follows", not "the list of brain areas at the end":
        // the rendered read showed the map is not at the END of anything a reader
        // can see — composed, it arrives under the block's OWN "##" heading, as a
        // full section between this one and the symptoms section. The note has to
        // describe what the reader meets, not what the source file looks like.
        Assert.Matches(new Regex(@"brain map that\s*follows", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"about growths elsewhere", RegexOptions.IgnoreCase), section);

        // BEFORE the directive, not after it. A note a reader meets on the way
        // out has not scoped anything (§12.8, WI-512: position was the property).
        var raw = CuratedPage.Flatten(Section(WhereHeading));
        var note = raw.IndexOf("A note on what follows", StringComparison.Ordinal);
        var directive = raw.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(note >= 0 && directive > note,
            "the scoping note does not sit above [MECHANISM], so the reader meets a lobe-by-lobe "
            + "map of the brain before anything reconciles it with a tumor that is in none of it");

        // AND THE BLOCK STILL SAYS THE THING THE NOTE IS ABOUT. If a later item
        // rewrites the block to drop the map, this note becomes a puzzle — so
        // the test says so rather than sitting green (the shape
        // AllBrainTumorsPageTests uses).
        var mechanism = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md")));
        Assert.Contains("Front of the brain", mechanism, StringComparison.Ordinal);
        Assert.Contains("block the flow of fluid", mechanism, StringComparison.Ordinal);

        // THE OVERRIDE ITSELF, WHICH WAS THE WHOLE REMEDY AND WAS UNPINNED.
        // Including [MECHANISM] imported a WEAKER statement of this tumor's
        // fastest emergency: composed, the block tells the reader a new pressure
        // pattern is "worth a phone call rather than a wait", about ninety lines
        // ABOVE this page's tier lifting the same signs to right-away. The note
        // above the directive is what reconciles them, and an unpinned remedy is
        // one edit from gone (/review rounds 1 and 2).
        Assert.Matches(new Regex(@"phone call rather than wait", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"overridden for these signs", RegexOptions.IgnoreCase), section);

        // And the block still SAYS the thing the override outranks. If a later
        // item reworders that line, this page's override becomes a puzzle rather
        // than silently wrong — the shape AllBrainTumorsPageTests uses for the map.
        Assert.Contains("worth a phone call rather than a wait", mechanism,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheSelfBlameBlockIsIncludedWithoutAScopingBridgeAndThePageAddsWhatIsTrueHere()
    {
        // WI-539 needed a bridge above [CAUSES] because the composed block says
        // "brain tumors" repeatedly on a page whose thesis is that this is not
        // one. THAT DOES NOT TRANSFER, and copying the bridge would have
        // answered a question this page never raises: US patient sources call
        // this a brain tumor outright (St. Jude, Boston Children's, AANS — all
        // three cited here; an earlier version of this list also named the
        // Pituitary Network Association, which this page does not carry as a
        // source and which therefore cannot support the ruling).
        //
        // So the block's framing fits, and what the page adds is the part the
        // block cannot know — that this one starts from cells left over before
        // birth, which is a more definite answer than "nobody knows".
        var section = PlainOf("Did I cause this?");

        Assert.Matches(new Regex(@"more definite answer than usual", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"cells left behind before you were born", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"nothing to prevent and\s*nothing to avoid", RegexOptions.IgnoreCase),
            section);

        // The addition sits AFTER the directive: the block answers the general
        // question first, and this page sharpens it. (The opposite of the
        // neighbour's bridge, which had to come first because it reconciled a
        // framing the reader would otherwise reject.)
        var raw = CuratedPage.Flatten(Section("Did I cause this?"));
        var directive = raw.IndexOf("[CAUSES]", StringComparison.Ordinal);
        var addition = raw.IndexOf("more definite answer than usual", StringComparison.Ordinal);
        Assert.True(directive >= 0 && addition > directive,
            "this page's own causes material no longer follows [CAUSES]");

        // And the demotion §12.9 requires: self-blame sits below "if it comes
        // back" and above the outlook gate.
        var headings = Headings();
        Assert.True(headings.IndexOf("Did I cause this?") > headings.IndexOf("If it comes back"));
        Assert.True(headings.IndexOf("What might happen over time")
            > headings.IndexOf("Did I cause this?"));
    }

    [Fact]
    public void TheSharedBlockIsIncludedUneditedAndThePageAddsItsOwnTiersBeneathIt()
    {
        // §12.10's remedy applied literally: include the block, then add what is
        // true HERE and false on the pages that include it. Editing
        // Content/blocks/escalation.md to carry this tumor's fluid rule would
        // put it on every glioma hub (WI-514's blast radius).
        var raw = Section(SymptomHeading);
        Assert.Contains("[ESCALATION]", raw, StringComparison.Ordinal);

        // The page's own tiers come AFTER the block, so a reader meets the
        // general rules first and the exceptions second.
        var directive = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        var own = raw.IndexOf("This tumor has two rules the list above does not carry",
            StringComparison.Ordinal);
        Assert.True(own > directive,
            "the page's own tiers sit above the shared block, so a reader meets the exceptions "
            + "before the rule");

        // THE FORWARD POINTER, ABOVE THE BLOCK. The corpus convention is
        // unanimous across every hub that adds a scoped rule — pointer above,
        // tier below — and WI-539 shipped the second half without the first,
        // which is WI-538's blocker shape: a reader meeting the most specialised
        // rule on the page with no warning it was coming. Worded deliberately
        // unlike the four siblings that carry one.
        var pointerAt = raw.IndexOf("Two sets of signs here carry rules of their own",
            StringComparison.Ordinal);
        Assert.True(pointerAt >= 0 && directive > pointerAt,
            "the forward pointer is missing or sits below [ESCALATION], so a reader meets this "
            + "tumor's own rules with no warning that they were coming");

        // THE BLOCK ITSELF IS UNTOUCHED.
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        foreach (var absent in new[]
                 {
                     "craniopharyngioma", "adrenal", "steroid as replacement",
                     "medical alert", "chiasm", "stalk",
                 })
        {
            Assert.DoesNotContain(absent, block, StringComparison.OrdinalIgnoreCase);
        }

        // The tiers are diffed against the siblings by the shared helper, which
        // reads /seizures/what-to-do, /treatments/craniotomy and the
        // chemotherapy fever rule rather than hard-coding what they say.
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);
    }

    // ------------------------------------------------- naming and the crosswalk

    [Fact]
    public void TheTwoTypesAreNamedAsSeparateTypesRatherThanAsTwoFlavoursOfOne()
    {
        // §12.1's highest-stakes claim class, and the slice §12.9 requires from
        // every hub. WHO CNS5, verified against the live article: "In past
        // editions, Adamantinomatous craniopharyngioma and Papillary
        // craniopharyngioma were considered subtypes (variants) of
        // craniopharyngioma, whereas they are now classified as distinct tumor
        // types". Corroborated independently by PMC10033642.
        var section = PlainOf(ReportHeading);

        Assert.Matches(new Regex(@"Adamantinomatous, and papillary", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"treated as two versions of one growth", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"now counts them as two separate types", RegexOptions.IgnoreCase), section);

        // The reason, which is what stops it reading as bureaucracy: they differ
        // in who gets them, how they look, and their genes.
        Assert.Matches(new Regex(@"differ in who gets them", RegexOptions.IgnoreCase), section);

        // AND THE SENTENCE THAT KEEPS A NEWER REPORT FROM READING AS WORSE NEWS.
        Assert.Matches(new Regex(@"Nothing about your growth changed", RegexOptions.IgnoreCase),
            section);

        // THE GENE IS NOT PART OF THE NAME, which is precisely why [CROSSWALK]
        // is excluded: that block teaches that gene results have entered tumor
        // NAMES, and here they have not.
        Assert.Matches(new Regex(@"not part of the growth's name", RegexOptions.IgnoreCase), section);

        // THE RENAME THIS PAGE DOES NOT OWN. /tumors/pituitary-tumor carries the
        // arginine vasopressin story with its full reason; §12.10 says name it
        // and route, rather than restate it at a third strength.
        Assert.Matches(new Regex(@"arginine vasopressin deficiency", RegexOptions.IgnoreCase),
            section);
        Assert.Contains("/tumors/pituitary-tumor", Section(ReportHeading), StringComparison.Ordinal);

        var restatesTheReason = new Regex(
            @"desmopressin|withheld|serious adverse outcomes|eight (?:hormone )?societies",
            RegexOptions.IgnoreCase);
        Assert.Matches(restatesTheReason, "Eight hormone societies endorsed it.");
        Assert.False(restatesTheReason.IsMatch(Plain),
            "this page has started retelling the rename's reasoning, which /tumors/pituitary-tumor "
            + "owns in full — route to it instead (§12.10)");
    }

    [Fact]
    public void EveryRomanGradeOnThePageCarriesItsOwnMarkerInTheSameSentence()
    {
        // A CORPUS-WIDE HOUSE RULE THIS PAGE MET FOR THE FIRST TIME, and it
        // failed on both occurrences of the draft.
        // PathologyReportPageContentTests scans every curated page SENTENCE BY
        // SENTENCE and allows a Roman grade only where that same sentence also
        // carries a marker such as "older report" or "changed in 2021". Both of
        // this page's sentences put the marker in the NEIGHBOURING sentence.
        //
        // The irony is the useful part: this page's crosswalk slice exists to
        // explain the Roman-to-Arabic change, and the guard against Roman
        // numerals rejected it — the same shape as WI-513's finding that a
        // retired-name ban must be negation-aware or it forbids the crosswalk it
        // protects.
        //
        // Pinned page-locally so a later edit cannot strip the marker and push
        // the failure into another page's test file, where nobody will look for it.
        var marker = new Regex(
            @"older report|used to be|retired|no longer|old rules|older paperwork|"
            + @"changed in 2021|now written",
            RegexOptions.IgnoreCase);
        var roman = new Regex(@"\bgrade\s+(I{1,3}|IV)\b", RegexOptions.IgnoreCase);

        var offenders = CuratedPage.SentencesOf(Plain)
            .Where(s => roman.IsMatch(s) && !marker.IsMatch(s))
            .ToList();

        Assert.True(offenders.Count == 0,
            "a Roman grade appears in a sentence that does not say it is the older style:\n  "
            + string.Join("\n  ", offenders));

        // A POSITIVE COUNT BESIDE THE ITERATE-AND-CHECK (§12.10, WI-517): a page
        // that stopped mentioning a Roman grade at all would pass the loop above
        // while silently dropping the crosswalk slice this hub owes its readers.
        var taught = CuratedPage.SentencesOf(Plain).Count(s => roman.IsMatch(s));
        Assert.True(taught >= 2,
            $"this page teaches the old grade notation in {taught} sentences; it owes a reader "
            + "holding older paperwork both the grade section and the report section");

        // And the canary, run against planted text, so the guard is proved able
        // to fire before its pass means anything (§12.8, WI-523).
        Assert.Matches(roman, "This one is a grade I tumor.");
        Assert.DoesNotMatch(marker, "This one is a grade I tumor.");
    }

    // ------------------------------------ what this page owns and what it routes

    [Fact]
    public void HypothalamicWeightGainIsNamedAsAMechanismRatherThanAsWillpower()
    {
        // THE PAGE'S MOST DISTINCTIVE OWNED CLAIM, and the one most likely to be
        // softened into something kinder-sounding and wronger. PMC12109346,
        // verified live: "Morbid obesity due to hypothalamic damage is mostly
        // non-responsive to conventional lifestyle modifications such as
        // physical exercise or dietary interventions."
        var section = CuratedPage.Flatten(
            Regex.Replace(CuratedPage.ComposedSubsection(
                Page, "When hunger and weight change, and it is not willpower"), @"[*_]", ""));

        Assert.Matches(new Regex(@"controls hunger and how much energy your body\s*burns",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not a lack of effort", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"does not respond to\s*ordinary advice about eating less", RegexOptions.IgnoreCase),
            section);

        // AND THE ANTI-HYPE HALF, from the same source: "Treatment for
        // hypothalamic obesity after CP has thus far been rather disappointing",
        // and no drug is proven effective in randomized controlled trials. A
        // page that named the problem and promised a fix would be §12.12 in the
        // reassuring direction.
        Assert.Matches(new Regex(@"results so far have been disappointing", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"strongest kind of trial", RegexOptions.IgnoreCase), section);

        // The caregiver section carries it too, because at home this is the
        // thing most often read as someone not trying.
        Assert.Matches(new Regex(@"Treating it as willpower", RegexOptions.IgnoreCase),
            PlainOf(CareHeading));
    }

    [Fact]
    public void WatchingIsNotOfferedHereAndThatReversesTheNeighbouringHub()
    {
        // /treatments/watch-and-wait names pituitary tumors, acoustic neuromas,
        // meningiomas and some low-grade gliomas in its who-is-watched list. It
        // does NOT name this one, and the sources say an operation is almost
        // always the first step ("Surgery is almost always the first step in
        // treating a craniopharyngioma" — Boston Children's).
        //
        // WI-539 routed to that page; this hub must not, and the ruling is
        // pinned because "add a link to the watching page" is an obvious-looking
        // improvement that would offer this reader a plan the sources do not
        // support for them.
        // ASSERTED AGAINST THE READER'S TEXT, NOT THE RAW FILE, and the first
        // version got that wrong in the way §12.8 keeps recording. This page's
        // front matter EXPLAINS the ruling, and explaining it requires naming
        // the URL being ruled against — so a raw-file check found
        // "/treatments/watch-and-wait" inside the very comment justifying its
        // absence and reported the page as routing there. A guard cannot tell a
        // rationale from a link; it can only tell what the reader receives.
        Assert.DoesNotContain("/treatments/watch-and-wait", Body, StringComparison.Ordinal);

        // READ the sibling rather than trusting the belief (§12.10). If it ever
        // starts naming this tumor, this ruling has to be re-argued rather than
        // left quietly in place.
        var watching = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "watch-and-wait.md")));
        Assert.DoesNotContain("craniopharyngioma", watching, StringComparison.OrdinalIgnoreCase);

        // And surgery is stated as the usual first step, so the absence of a
        // watching route is a positive claim rather than a gap.
        Assert.Matches(new Regex(@"An operation is almost always the first step",
            RegexOptions.IgnoreCase), PlainOf(TreatedHeading));
    }

    [Fact]
    public void TheExtentOfResectionTradeOffIsGivenHonestlyInBothDirections()
    {
        // The genuinely unsettled decision this reader will be asked about, and
        // the one a page could quietly take sides on. Endotext: "In recent
        // years, many tertiary centers have adopted a more conservative surgical
        // approach, electing for partial or limited resection with radiotherapy
        // over complete resection, when possible, with aim of hypothalamic
        // sparing". StatPearls: "the pituitary stalk should be preserved when
        // possible."
        var section = PlainOf(TreatedHeading);

        Assert.Matches(new Regex(@"Taking out every last piece lowers the chance",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"can cause lasting\s*problems of its own", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"deliberately leave a piece alone", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"spare the stalk", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"no single right answer", RegexOptions.IgnoreCase), section);

        // AND THE TARGETED-THERAPY PARAGRAPH IS HEDGED, because the evidence is
        // small case series. Endotext records severe toxicities and people
        // stopping; the scoping review says the durability question is open.
        Assert.Matches(new Regex(@"reports so far are small", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"how long the benefit lasts is not yet settled",
            RegexOptions.IgnoreCase), section);

        var overclaims = new Regex(
            @"\bbreakthrough\b|\bgame[- ]chang\w*|\bcure[sd]?\b|\bmiracle\b|"
            + @"\bhighly effective\b|\bworks for (?:almost )?everyone\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(overclaims, "This targeted drug is a breakthrough.");
        Assert.DoesNotMatch(overclaims, Plain);
    }

    [Fact]
    public void TheOnwardDoorsSitInTheSectionsWhoseReaderNeedsThem()
    {
        // WI-512's rule: presence was never the property, POSITION was. Each
        // route out is offered where the question arises, not collected in a
        // footer a reader reaches only after the part they came for.
        foreach (var (link, heading) in new[]
                 {
                     ("/treatments/shunts#warning-signs", SymptomHeading),
                     ("/treatments/steroids", SymptomHeading),
                     ("/tests/mri", FindOutHeading),
                     ("/tumors/pituitary-tumor", ReportHeading),
                     ("/tests/pathology-report#what-the-grade-means", ReportHeading),
                     ("/treatments/craniotomy", TreatedHeading),
                     ("/treatments/radiation-therapy", TreatedHeading),
                     ("/tests/follow-up-scans", ScansHeading),
                     ("/tests/waiting-for-results", ScansHeading),
                     ("/get-help-now", "Where to get support"),
                     ("/tumors/pediatric-brain-tumor", "Where to get support"),
                 })
        {
            Assert.Contains(link, Section(heading), StringComparison.Ordinal);
        }
    }

    // --------------------------------------------------------- housekeeping

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3, read from the spec rather than copied from the last hub written
        // — §12.8's WI-510 lesson is that copying the previous page is how a
        // template quietly shrinks.
        Assert.Equal(
            [
                "The short version",                                               // 0
                "What is a craniopharyngioma?",                                    // 1
                "Is it cancer? What does its grade mean?",                         // 2
                "Where does it grow, and why does it cause these symptoms?",       // 3
                "What symptoms does it cause?",                                    // 4
                "How do doctors find out it is this?",                             // 5
                "What do the words on my report mean?",                            // 6
                "How is it usually treated?",                                      // 7
                "What is treatment actually like, and what is normal afterwards?", // 8
                "Everyday life",                                                   // 9
                "Follow-up scans, and what to do while you wait",                  // 10
                "If it comes back",                                                // 11
                "Did I cause this?",                                               // self-blame, demoted
                "What might happen over time",                                     // 12, gated
                "For the person caring for someone with this",                     // 13
                "What to ask your team",                                           // 14
                "Where to get support",                                            // 15
            ],
            Headings());

        // The two subsections of the treatment section, which carry the hormone
        // aftermath and the hypothalamic weight material.
        foreach (var sub in new[]
                 { "Hormones afterwards", "When hunger and weight change, and it is not willpower" })
        {
            Assert.Matches(new Regex($@"^### {Regex.Escape(sub)}\s*$", RegexOptions.Multiline),
                CuratedPage.Body(Page));
        }
    }

    [Fact]
    public void TheCaregiverSectionAddsWhatIsOnlyTrueHere()
    {
        // §12.10: a caregiver section that paraphrases the block in different
        // words is the defect WI-525 wrote down and WI-526 repeated, and no
        // shingle check can see a paraphrase.
        var care = PlainOf(CareHeading);

        // THREE, AND THE SECTION SAYS THREE. A fourth lead-in is how a
        // paraphrase of the block arrives.
        Assert.Matches(new Regex(@"Three things are specific to this one", RegexOptions.IgnoreCase),
            care);
        var leads = Regex.Matches(CuratedPage.Flatten(Reader(Section(CareHeading))), @"\*\*(.+?)\*\*")
            .Select(m => m.Groups[1].Value)
            .ToList();
        Assert.True(leads.Count == 3,
            $"the caregiver section promises three things and carries {leads.Count}:\n  "
            + string.Join("\n  ", leads));

        // Both emergencies are handed over, because for both of them the person
        // who notices first is often not the patient.
        Assert.Matches(new Regex(@"Learn both urgent rules alongside them", RegexOptions.IgnoreCase),
            care);

        // AND THE SUMMARY MUST NOT NARROW EITHER RULE — /review round 2's S1, and
        // the one finding of that round that could have cost a reader something.
        // A caregiver summary is a RESTATEMENT of two safety rules, and this one
        // quietly shrank both: it kept only the first branch of the fluid tier's
        // disjunction ("arrive quickly", dropping "or together"), and it re-gated
        // the adrenal rule on "taking a replacement steroid" eleven lines after
        // the page deliberately widened it to anyone whose gland has been harmed.
        //
        // Neither narrowing was visible to any guard: every assertion on the
        // tiers reads the SYMPTOMS section, and every assertion here read two
        // phrases. A rule summarised elsewhere needs the summary checked against
        // the rule (§12.8's "when a sentence makes a claim about a list, open the
        // list", one level up).
        Assert.Matches(new Regex(@"arrive quickly or\s*together", RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(
            @"whether or\s*not they are on a replacement steroid", RegexOptions.IgnoreCase), care);

        // Canaries, run against planted text so the guards are proved able to
        // fire (§12.8, WI-523) rather than passing because the section is short.
        var narrowedToSpeed = new Regex(@"signs that arrive quickly;", RegexOptions.IgnoreCase);
        Assert.Matches(narrowedToSpeed, "One is signs that arrive quickly; the other is illness.");
        Assert.DoesNotMatch(narrowedToSpeed, care);

        var gatedOnTaking = new Regex(@"illness while they are taking a replacement",
            RegexOptions.IgnoreCase);
        Assert.Matches(gatedOnTaking, "the other is illness while they are taking a replacement steroid");
        Assert.DoesNotMatch(gatedOnTaking, care);
        Assert.Matches(new Regex(@"neither can wait until morning", RegexOptions.IgnoreCase), care);
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST, and that is the ruling. The guard is BIDIRECTIONAL: this
        // page's first draft turned /tumors/pituitary-tumor,
        // /tumors/pediatric-brain-tumor, /treatments/targeted-therapy and
        // /treatments/steroids red. An allowlist here could not have fixed any of
        // them, because none of those tests reads this page's allowlist.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    [Fact]
    public void ThePageNeverMinimisesOrCharacterisesTheDiagnosis()
    {
        var reader = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(reader, Slug);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(reader),
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // Two of this item's sources are British and are barred for idiom
        // (Cancer Research UK, The Pituitary Foundation), so their phrasing is
        // exactly what would arrive in this page's own prose.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, and each was checked against the corpus before being added
        // (§12.8: a phrase belongs on a ban list only if no correct sentence
        // contains it). "junction" is here because WI-539 counted zero corpus
        // occurrences and a US reader reads it as a road — this page's first
        // draft used it for the optic chiasm, which is the same idiom class one
        // step further in.
        foreach (var form in new[] { "tablets", "optician", "junction", "spectacles", "theatre" })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotMatch(new Regex(@"\bGP\b"), body);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The sources this page's load-bearing claims rest on, by name. Each was
        // verified against the LIVE publication, not against the item's extract
        // pack — see .claude/work_files/wi540/source-verification.md for why that
        // distinction mattered here.
        foreach (var owed in new[]
                 {
                     "NBK519027",   // StatPearls: the backbone
                     "NBK538819",   // Endotext: hypothalamic syndrome, recurrence horizon
                     "PMC8328013",  // WHO CNS5: the naming authority
                     "PMC10033642", // the only source printing the grade in Arabic
                     "PMC12109346", // hypothalamic obesity, and its anti-hype half
                     "PMC8963871",  // the bleeding paper, kept rare
                     "together.stjude.org",
                     "aans.org",
                     "medlineplus.gov",
                     "endocrine.org",
                 })
        {
            Assert.Contains(urls, u => u.Contains(owed, StringComparison.Ordinal));
        }

        // THE SOURCE THE STUB CITED, AND IT IS BARRED. §12.1: "a page that cites
        // NCI for a tumor name is a defect", and naming is a large part of what
        // this page does. The third stub in a row to carry it.
        foreach (var barred in new[] { "cancer.gov", "cancerresearchuk.org", "pituitary.org.uk" })
        {
            Assert.False(urls.Any(u => u.Contains(barred, StringComparison.OrdinalIgnoreCase)),
                $"{barred} is cited; the front matter records why it is not used");
        }

        // The known-dead list.
        foreach (var dead in new[]
                 {
                     "mayoclinic.org", "cancer.org", "academic.oup.com", "sciencedirect.com",
                     "journals.lww.com", "medscape.com", "link.springer.com",
                 })
        {
            Assert.False(urls.Any(u => u.Contains(dead, StringComparison.OrdinalIgnoreCase)),
                $"{dead} is cited; it is JavaScript-rendered, 403s, or is on the known-dead list");
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S",
            RegexOptions.Multiline).Count);

        // NO BRACKETED DIRECTIVE NAME ANYWHERE IN THE FRONT MATTER. This is the
        // WI-538 trap and it bit WI-539's first draft: CaregiverSectionTests
        // locates the block with a raw IndexOf over the WHOLE file, so a
        // bracketed name in a comment becomes the first match and the page is
        // reported as not carrying the block under its standard heading.
        Assert.DoesNotMatch(new Regex(@"\[[A-Z][A-Z-]+\]"), front);
    }
}

/// <summary>The page as served.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class CraniopharyngiomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/craniopharyngioma";

    private readonly WebApplicationFactory<Program> _factory;

    public CraniopharyngiomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Craniopharyngioma", html);
        Assert.Contains("the three things a team watches", html);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // Two required doors in the support section: the person to call, and the
        // page for the reader who is a parent. This tumor's age distribution is
        // bimodal, so the child reader is a real slice of this page's audience
        // and is ROUTED rather than restated (WI-538 owns that material).
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/tumors/pediatric-brain-tumor");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page. This page
        // deep-links two anchors, so the check is not vacuous here.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("What might happen over time", html);
        Assert.Contains("<details", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheFourBlocksComposeAndTheFourExcludedOnesLeaveNoTrace()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // No literal directive reaches a reader (§12.10, WI-514's trap).
        foreach (var directive in new[]
                 {
                     "[MECHANISM]", "[ESCALATION]", "[CAUSES]", "[CAREGIVER]",
                     "[CROSSWALK]", "[TUMOR-BOARD]", "[SPINAL-CORD]", "[POSTERIOR-FOSSA-SYNDROME]",
                 })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // Each included block's own content does arrive.
        Assert.Contains("Call an ambulance", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
        Assert.Contains("block the flow of fluid", html, StringComparison.Ordinal);

        // AND THE EXCLUDED BLOCKS' CONTENT DOES NOT — asserted on the RENDERED
        // page, because that is the only place a wrongly-composed block becomes
        // visible. Read out of the block files rather than re-typed, so
        // rewording a block cannot silently retire this check.
        // ALL FOUR EXCLUSIONS, not three. The first version named four excluded
        // directives above and then checked the CONTENT of only three, leaving
        // posterior-fossa-syndrome out of the loop — on the one test whose whole
        // subject is the blocker-that-is-an-absence. /review round 1 caught it.
        foreach (var name in new[]
                 { "crosswalk", "tumor-board", "spinal-cord", "posterior-fossa-syndrome" })
        {
            var block = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, name + ".md"));
            // FLATTENED FIRST: `[^.\n]` cannot cross a line, so over raw block
            // text this could only ever pick a sentence that happens to fit on
            // one hard-wrapped line.
            var sentence = Regex.Matches(
                    CuratedPage.Flatten(CuratedPage.ReaderText(block)), @"[A-Z][^.]{45,90}\.")
                .Select(m => m.Value)
                .FirstOrDefault();
            Assert.False(string.IsNullOrEmpty(sentence),
                $"no sentence could be read out of {name}.md, so this guard proves nothing");
            Assert.DoesNotContain(sentence, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheGlossaryFiresOnTheOneTermThisPageDoesNotDefineItself()
    {
        // NEW GLOSSARY ENTRIES: none, and that is a ruling. Almost all of this
        // page's vocabulary is a FIRST use in the corpus — "adamantinomatous",
        // "chiasm", "pituitary stalk" and the field-loss pattern
        // appear nowhere else under Content/ — so none meets §12.8's second-use
        // threshold, and each is glossed inline instead.
        //
        // "BRAF" already exists and fires on its own, which is what this
        // asserts. NAMED BY SLUG, not counted: a bare "def-" would pass on any
        // tooltip at all, so the day another term fires here the guard would
        // keep passing while the term it exists for had gone (§12.8, WI-512 and
        // WI-539's handproof, where exactly that turned out to be load-bearing
        // only by accident).
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("def-braf", html, StringComparison.Ordinal);

        // And the page NAMES the term while the glossary DEFINES it. WI-535 and
        // WI-539 both shipped a tooltip that echoed the sentence it landed in;
        // here the page says only that medicines aimed at the change exist.
        Assert.DoesNotContain("a gene in a pathway that tells cells to divide",
            CuratedPage.Flatten(CuratedPage.Read("tumors", "craniopharyngioma.md")),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndex()
    {
        // WI-412 shipped a page nothing linked to. This one is listed from
        // /tumors because taxonomy.yml carries a `slug: craniopharyngioma` entry
        // — a property of another file agreeing with this one rather than
        // something anybody asserted, which is what this test exists to fix.
        //
        // THE EARLIER VERSION OF THIS COMMENT ALSO CLAIMED THE LISTING DEPENDS ON
        // THE PAGE HAVING A DESCRIPTION. It does not, and the handproof is what
        // established that: deleting the entire description left this guard
        // green, correctly, because taxonomy.yml holds only a slug and a label
        // and the assertions below read the link and the label. A comment naming
        // a dependency that does not exist is §12.8's stale rationale — it never
        // fails, and it sends the next editor to the wrong file.
        var html = await _factory.CreateClient().GetStringAsync("/tumors");

        Assert.Contains("/tumors/craniopharyngioma", html, StringComparison.Ordinal);
        Assert.Contains("Craniopharyngioma", html, StringComparison.Ordinal);
    }
}
