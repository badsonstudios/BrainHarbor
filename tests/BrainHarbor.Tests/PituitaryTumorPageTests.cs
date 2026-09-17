using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-539: pituitary tumor, deepened. A §12.3 SEVENTEEN-SECTION TUMOR HUB, and
/// the first hub in the corpus for a tumor that is not in the brain at all.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * THE TWO EMERGENCIES. This tumor has two that the shared escalation block
///     does not carry, and the block UNDER-TRIAGES both: it files "a headache
///     much worse than usual" and "a sudden change in your vision" as SAME-DAY,
///     and never mentions double vision at all. Apoplexy is "a medical and
///     surgical emergency in many cases" (NBK559222) where delay risks
///     "irreversible visual loss, or death". Adrenal crisis "can cause death"
///     untreated (Endocrine Society). Every sign of both is enumerated below,
///     ONE BY ONE, because a tier that loses a sign loses it silently.
///   * "benign" used as comfort. The Endocrine Society hands this page the
///     sentence — "almost always benign (non-cancerous)" — and a reader who
///     takes that home is not prepared to lose their sight to a small growth.
///   * THE TWO BLOCK EXCLUSIONS. [MECHANISM] and [CROSSWALK] are wrong here
///     rather than merely unnecessary, and an excluded block leaves NO TRACE —
///     so nothing but an exact closed set stops a later item restoring one.
///   * a prognosis figure, a dose, a hormone threshold, or a scan interval
///     (§12.4, §12.5, and WI-521's no-schedule ruling, which /treatments/watch-
///     and-wait owns for this tumor).
///   * the three renames read as three different diagnoses rather than as
///     renames.
///
/// NO ALLOWLIST. <c>AssertDoesNotRestateTheCorpus</c> is called with no
/// deliberately-shared entries at all: every collision this page had was
/// reworded rather than exempted. That is not a style choice — WI-539 found the
/// guard is BIDIRECTIONAL, and this page's first draft turned two SHIPPED pages
/// red. An allowlist on the new page could not have fixed the old pages'
/// failures, because their tests never read it.
/// </summary>
public sealed class PituitaryTumorPageContentTests
{
    private const string Slug = "tumors/pituitary-tumor";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string CareHeading = "For the person caring for someone with this";

    private static string Page => CuratedPage.Read("tumors", "pituitary-tumor.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The title and description, which <c>ReaderText</c> strips and
    /// <c>ContentPage.cshtml</c> renders as the first paragraph a reader meets
    /// (§12.8, WI-524).
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

    /// <summary>
    /// Everything with the emphasis markers removed (§12.8, WI-526: a bolded
    /// word and a line wrap between two words defeats a phrase guard
    /// structurally, and no vocabulary fixes that). This page is dense with
    /// bold lead-ins, so the plain form is the one nearly every ban runs on.
    /// </summary>
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

    // ------------------------------------------------- the two emergencies

    [Fact]
    public void TheApoplexyEmergencyIsCarriedSignBySignAndIsNeverDemotedToSameDay()
    {
        // THE ITEM'S CENTRAL SAFETY CLAIM. NBK559222: "Pituitary apoplexy is a
        // medical and surgical emergency in many cases"; "Delay in recognition
        // or treatment may result in permanent hypopituitarism, irreversible
        // visual loss, or death"; "A sudden onset of headache located behind
        // the eyes is the most common symptom"; "Other symptoms include
        // decreased visual acuity, hemianopia, diplopia, ptosis, nausea and
        // vomiting, altered mental status".
        //
        // ENUMERATED ONE BY ONE rather than asserted as a paragraph. A tier
        // that loses one sign loses it silently: the paragraph still reads
        // correctly, every prose assertion still passes, and the reader who had
        // that one sign is the one who does not call.
        var section = PlainOf(SymptomHeading);

        var tier = Regex.Match(section,
            @"A sudden, severe headache\.(.*?)(?=If you take steroid)",
            RegexOptions.Singleline);
        Assert.True(tier.Success, "the apoplexy tier has gone from the symptoms section");
        var apoplexy = tier.Groups[1].Value;

        // THE ENTRY IS THE HEADACHE ALONE — /review round 3's BLOCKER B1, and the
        // whole of this guard was blind to the axis it failed on. Every assertion
        // here is about what the tier CONTAINS; the defect was what its opening
        // SCOPED OUT. The lead-in read "A sudden, severe headache with a change in
        // your sight" — a CONJUNCTION — so a reader with the commonest
        // presentation did not match the entry and read on. NBK559222: "A sudden
        // onset of headache located behind the eyes is the MOST COMMON symptom",
        // with everything else filed under "Other symptoms include"; and, put
        // disjunctively, "Any acute visual change OR significant headache should
        // prompt immediate medical assessment". /start files a lone sudden severe
        // headache under CALL 911, so the conjunction was also one symptom at two
        // strengths on two pages (§12.10).
        //
        // Asserted over the RAW section, where the emphasis markers still exist: a
        // reader skimming bold text meets the lead-in and nothing else, which is
        // exactly why narrowing it is invisible to every Contains() below.
        var entry = Regex.Match(CuratedPage.Flatten(Section(SymptomHeading)),
            @"\*\*(A sudden, severe headache[^*]*)\*\*");
        Assert.True(entry.Success,
            "the apoplexy tier no longer opens on a bold lead-in naming the headache");

        var narrowed = new Regex(
            @"\b(?:with|and|plus|together with|along with|followed by)\b", RegexOptions.IgnoreCase);
        Assert.Matches(narrowed, "A sudden, severe headache with a change in your sight.");
        Assert.Matches(narrowed, "A sudden, severe headache and double vision.");
        Assert.False(narrowed.IsMatch(entry.Groups[1].Value),
            "the apoplexy entry has been narrowed from the headache to the headache PLUS "
            + "something else, so the most common presentation no longer matches the rule "
            + "written for it: " + entry.Groups[1].Value);

        // And the disjunction is stated outright, because a list of signs reads as
        // a set of things that arrive together unless the page says otherwise.
        Assert.Matches(new Regex(@"It can come on its own", RegexOptions.IgnoreCase), apoplexy);

        // The event, in the reader's words and then in the word their team uses.
        Assert.Matches(new Regex(@"bleeds, or loses its blood supply", RegexOptions.IgnoreCase),
            apoplexy);
        Assert.Matches(new Regex(@"\bapoplexy\b", RegexOptions.IgnoreCase), apoplexy);

        // EVERY SIGN, SEPARATELY.
        foreach (var sign in new[]
                 {
                     "comes on suddenly",
                     "behind the eyes",
                     "sight that drops",
                     "double vision",
                     "a drooping eyelid",
                     "feeling sick",
                     // /review round 3's S4 — and its quotation was NOT exact, which
                     // is recorded in the front matter. NBK559222's sign paragraph
                     // says "altered mental status", and, for the adrenal crisis
                     // that follows apoplexy, "hypotension, hypothermia, lethargy,
                     // and, on occasion, coma". It never says "fainting" or
                     // "collapse". "Passing out" is what those support, so that is
                     // what the page carries. The finding was right that the sign
                     // belonged here; the evidence offered for it was not.
                     "passing out",
                     "feeling confused",
                 })
        {
            Assert.Contains(sign, apoplexy, StringComparison.OrdinalIgnoreCase);
        }

        // THE TIER ITSELF, in the three parts a reader at two in the morning
        // needs: that it cannot wait, the 911 route for the sign the shared block
        // files as an ambulance, and a destination for everything else.
        //
        // THIS COMMENT USED TO SAY "what to do when nobody answers", and asserted
        // "get to an emergency room" — both from the version review round 2
        // replaced, where the emergency department was the FALLBACK for an
        // unanswered phone rather than the route. The destination assertion now
        // lives below with the rest of the S4 fix; leaving this one here made the
        // file describe a page that no longer existed.
        Assert.Matches(new Regex(@"This one cannot wait for morning", RegexOptions.IgnoreCase),
            apoplexy);

        // ONE ROUTE FOR THE WHOLE ENTRY. The previous version split it — 911 "if
        // your sight is suddenly going", a phone call for everything else — which,
        // once B1 made the entry the headache alone, would have filed a lone
        // thunderclap headache BELOW the strength /start gives it. It also carried
        // /review round 3's two-actions-in-one-sentence nit ("call your team now,
        // whatever the hour, and go to the emergency department").
        Assert.Matches(new Regex(
            @"Call 911, or your local emergency number,\s*or go straight to the emergency department",
            RegexOptions.IgnoreCase), apoplexy);
        Assert.Matches(new Regex(@"Tell your team as well, at whatever hour it is",
            RegexOptions.IgnoreCase), apoplexy);

        // AND IT DOES NOT END ON THE FRIGHT (§12.6). The last thing in the
        // paragraph is that this is treated, and that speed is what protects
        // sight — which is also the reason to call rather than wait.
        Assert.Matches(new Regex(@"It is treated, and treating\s*it early is what protects your sight",
            RegexOptions.IgnoreCase), apoplexy);

        // THE AMBULANCE ROUTE, AND THIS IS /review's BLOCKER A1. The shared
        // block files "suddenly not being able to speak, move one side, or see"
        // under CALL AN AMBULANCE / 911 (blocks/escalation.md). This page's
        // apoplexy signs include sight that drops and confusion — so the tier
        // BELOW it, the one introduced as "the list above does not cover them"
        // and therefore read as the authoritative one for this tumor, used to
        // offer a PHONE CALL. That is a demotion from 911 to the team, four
        // paragraphs after 911, and the word "911" appeared nowhere on the page.
        //
        // It matters past apoplexy: a sudden severe headache behind the eyes
        // with a vision change is also how a subarachnoid hemorrhage presents.
        // The page must not teach that the answer to a thunderclap headache is
        // a phone call.
        // AND /start IS READ RATHER THAN REMEMBERED (§12.10). It is the corpus's
        // front door for somebody with no diagnosis yet, and it files a lone sudden
        // severe headache under 911. That is what forces this tier's strength once
        // the headache is the entry; if that page ever softens, this has to be
        // re-argued rather than left quietly higher than the site's own front door.
        var start = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.PagesDirectory, "start.md"))));
        Assert.Matches(new Regex(@"A sudden severe headache", RegexOptions.IgnoreCase), start);
        Assert.Matches(new Regex(@"Call 911", RegexOptions.IgnoreCase), start);

        // THE HOSPITAL IS THE ROUTE, NOT A FALLBACK — review round 2's S4. The
        // first version read "Otherwise call your team now … and if you cannot
        // reach anyone, get to an emergency room", which sent the textbook
        // presentation minus visual loss (sudden severe headache behind the eyes,
        // double vision, a drooping eyelid, vomiting, confusion) to a phone call,
        // with the ER as what you do when nobody answers. NBK559222 calls
        // apoplexy "a medical and surgical emergency in many cases" whose
        // management is urgent imaging, high-dose steroid and sometimes emergency
        // surgery — none of it deliverable by telephone.
        //
        // It also stopped DESCRIBING the block's list. The bullet above says
        // "suddenly not being able to SEE"; sight that is GOING is not that, so
        // the old wording told the reader the list said something it does not,
        // and a reader who checked would not find their symptom there.
        Assert.Matches(new Regex(@"go straight to the emergency department",
            RegexOptions.IgnoreCase), apoplexy);
        Assert.Matches(new Regex(@"a scan and treatment\s*that a phone call cannot give you",
            RegexOptions.IgnoreCase), apoplexy);
        Assert.DoesNotMatch(new Regex(@"the ambulance list above", RegexOptions.IgnoreCase),
            apoplexy);

        // NEVER DEMOTED. A demotion would read perfectly and cost somebody their
        // sight. THE FIRST VERSION OF THIS GUARD COULD NOT SEE THE REAL ONE: it
        // knew only same-day/next-day/morning vocabulary, so a fall from
        // AMBULANCE to "call your team" matched nothing and passed. A guard that
        // polices only the demotions its author imagined is the shape §12.8
        // keeps recording.
        var demoted = new Regex(
            @"\b(?:the same day|same-day|next day|in the morning|when the clinic opens|"
            + @"at your next appointment|within a day|wait until)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(demoted, "Call your team the same day if this happens.");
        Assert.Matches(demoted, "Ring them in the morning when the clinic opens.");
        Assert.DoesNotMatch(demoted, apoplexy);

        // AND NO SIGN THE BLOCK FILES AS AMBULANCE MAY BE FILED LOWER HERE. Read
        // out of the block rather than re-typed, so rewording it cannot retire
        // this check (§12.11's trap, and CuratedPage.EscalationLines' reason).
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        Assert.Contains("Suddenly not being able to speak, move one side, or see", block,
            StringComparison.Ordinal);
        Assert.Contains("Call an ambulance", block, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAdrenalCrisisRuleIsConditionalAndCarriesTheSameHourTrigger()
    {
        // THE SECOND EMERGENCY, and the one that is CONDITIONAL — it is true
        // only of a reader on steroid replacement, so the condition has to come
        // FIRST. An unconditional version frightens everybody else on the page
        // (§12.12) and is the WI-534 shunt-clause shape.
        var section = PlainOf(SymptomHeading);

        var tier = Regex.Match(section,
            @"If you take steroid replacement, being ill is its own rule\.(.*?)"
            + @"(?=Both of these are uncommon)",
            RegexOptions.Singleline);
        Assert.True(tier.Success, "the adrenal-crisis tier has gone from the symptoms section");
        var adrenal = tier.Groups[1].Value;

        // The mechanism, in plain words and without naming a hormone the reader
        // has no use for.
        Assert.Matches(new Regex(@"a plain illness can become dangerous", RegexOptions.IgnoreCase),
            adrenal);

        // ATTRIBUTED, because "it can kill" is the strongest sentence on the
        // page and §12.13 wants the strongest claim carrying its source.
        Assert.Matches(new Regex(@"The Endocrine Society calls that an adrenal\s*crisis",
            RegexOptions.IgnoreCase), adrenal);
        Assert.Matches(new Regex(@"without treatment it can kill", RegexOptions.IgnoreCase), adrenal);

        // THE THREE THINGS TO ASK FOR, separately. Each is a different action on
        // a different day, and a reader who gets two of three is missing one.
        // THE STRESS LIST, THE SIGNS, AND THE DESTINATION — all three added after
        // /review's BLOCKER A3, and all three were already sitting in this page's
        // OWN front matter as verbatim quotes it had recorded and then not used.
        // The Endocrine Society gives the stress list as "illness, infection,
        // surgery, or an accident" (this page had only illness, on a page whose
        // readers have operations); the crisis signs as severe nausea and
        // vomiting, dehydration and confusion, low blood pressure and fainting;
        // and the action as an injection "right away" followed by going "to the
        // hospital immediately". A tier with a trigger, one sign and no
        // destination is the one instruction a frightened person cannot execute.
        foreach (var owed in new[]
                 {
                     "an infection, an operation, or an accident",
                     "very sick to your stomach",
                     "confusion",
                     // NO EMBEDDED NEWLINES: `adrenal` comes from PlainOf, which
                     // flattens whitespace to single spaces, so a needle carrying
                     // the page's hard wrap can never match. The first version of
                     // these three did, copied from the source's line breaks.
                     "feeling faint or passing out",
                     "that is what it is for",
                     "you still go straight to a hospital",
                     "what to do when you are ill",
                     "cannot keep pills down",
                     "carry an injection",
                     "medical alert bracelet",
                 })
        {
            Assert.Contains(owed, adrenal, StringComparison.OrdinalIgnoreCase);
        }

        // THE TRIGGER, AT ITS OWN STRENGTH. "Same hour, not the next day" is
        // the whole tier: vomiting is the sign that the medicine is not getting
        // in, and it is the one a reader explains away.
        Assert.Matches(new Regex(
            @"Being sick and unable to keep pills down needs help\s*the same hour, not the next day",
            RegexOptions.IgnoreCase), adrenal);

        // THE ROUTE MUST SIT OUTSIDE THE INJECTION CLAUSE — review round 2's B1,
        // and a substring check could not see the difference. The destination
        // used to live inside "If you have been given an emergency injection…",
        // so a reader who is confused and fainting and was never given one got a
        // planning instruction in the future tense and a rule about vomiting.
        // The Endocrine Society states it of everyone in crisis: "need an
        // injection … right away. THEN THEY NEED TO GO TO THE HOSPITAL
        // IMMEDIATELY for more treatment."
        //
        // So the assertion is POSITIONAL: an unconditional emergency route must
        // appear BEFORE the conditional sentence, not inside it.
        var route = adrenal.IndexOf("that is an emergency: call 911", StringComparison.Ordinal);
        var conditional = adrenal.IndexOf("If you have been given an emergency injection",
            StringComparison.Ordinal);
        Assert.True(route >= 0 && conditional > route,
            "the adrenal tier's only route out is inside the 'if you have been given an "
            + "injection' clause, so a reader in crisis without one is given no destination");

        // THE EXTENSION IS NOT GATED ON HAVING BEEN TOLD — /review round 3's S3.
        // It used to open "If your team has told you a hormone is low but you are
        // not on replacement yet", which excluded precisely the people the sources
        // describe. The Endocrine Society: "Some people don't know they have AI
        // until they have a sudden worsening of symptoms called an adrenal
        // crisis." NIDDK: "Sometimes symptoms appear for the first time during
        // adrenal crisis." A reader who has been told nothing is the one most
        // likely to explain the signs away, and the old gate handed them a reason.
        Assert.Matches(new Regex(@"You do not have to be on replacement for this to be yours",
            RegexOptions.IgnoreCase), adrenal);
        Assert.Matches(new Regex(@"a crisis is the first sign of it", RegexOptions.IgnoreCase),
            adrenal);

        var gatedOnDiagnosis = new Regex(
            @"If your team has told you a hormone is low", RegexOptions.IgnoreCase);
        Assert.Matches(gatedOnDiagnosis,
            "If your team has told you a hormone is low but you are not on replacement yet.");
        Assert.DoesNotMatch(gatedOnDiagnosis, adrenal);

        // THE CONDITION IS THE START OF ITS OWN BOLD LEAD-IN, and that is the
        // property rather than its presence somewhere in the paragraph. A
        // reader skimming the bold text has to meet the scope, not the warning:
        // "being ill is its own rule" read without "if you take steroid
        // replacement" in front of it is a sentence aimed at everybody on the
        // page, which is §12.12 and is the shape WI-534 fixed for shunts.
        //
        // Asserted against the RAW section, where the emphasis markers still
        // exist — PlainOf strips exactly the thing this guard is about.
        var lead = Regex.Match(CuratedPage.Flatten(Section(SymptomHeading)),
            @"\*\*(If you take steroid replacement[^*]*)\*\*");
        Assert.True(lead.Success,
            "the adrenal rule's conditional clause no longer opens its own bold lead-in, so a "
            + "reader who takes no steroid replacement meets the warning as though it were theirs");

        // ONE CLAIM, ONE STRENGTH, ACROSS TWO PAGES (§12.10) — /review's BLOCKER
        // A2. /treatments/steroids files "You cannot keep your pills down" as
        // SAME-DAY; this page files the same words as the same HOUR. Neither was
        // wrong and neither said why: dexamethasone taken for swelling and a
        // steroid REPLACING a hormone the body can no longer make are different
        // emergencies, and that page's list was written for the first. The
        // sibling now carries the conditional, and it is READ here rather than
        // assumed, the way AssertEscalationTiers reads craniotomy.
        var steroids = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "steroids.md")));
        // SCOPED ON THE MECHANISM, NOT THE DIAGNOSIS, and review round 2 is why.
        // The first version read "If your steroid is replacing a hormone your body
        // can no longer make … after pituitary surgery", which told a reader on
        // long-term dexamethasone that the urgent rule was not theirs — 160 lines
        // after that page teaches "Because your body stops making its own while
        // you are taking this one". An internal contradiction introduced into a
        // SHIPPED page, in the over-reassuring direction (§12.12). The rescope
        // needs no new citation: it rests on that page's own sourced mechanism.
        Assert.Contains("If your body has stopped making its own steroid", steroids,
            StringComparison.Ordinal);
        Assert.Contains("long enough for your own supply to go quiet", steroids,
            StringComparison.Ordinal);
        Assert.Contains("/tumors/pituitary-tumor", steroids, StringComparison.Ordinal);

        // THE CONDITIONAL ESCALATES THE WHOLE LIST — /review round 3's BLOCKER B2.
        // The assertion that used to sit here pinned the sentence "Being unable to
        // keep the pills down is a right-away call at any hour, not a same-day
        // one", under a lead-in claiming "one item on the list below is different
        // for you". That lead-in was FALSE ABOUT ITS OWN PAGE, and no guard on
        // either side could see it, because both only ever read the conditional.
        //
        // So this one READS THE SIBLING'S SAME-DAY LIST instead of trusting the
        // claim made about it.
        var sameDay = Regex.Match(steroids,
            @"Call your team the same day if:(.*?)(?=Call an ambulance)", RegexOptions.Singleline);
        Assert.True(sameDay.Success && sameDay.Groups[1].Value.Length > 200,
            "/treatments/steroids has no readable same-day list, so the conditional above it "
            + "cannot be checked against what it actually escalates");

        // Three of its bullets are the adrenal-crisis triad the Endocrine Society
        // names ("Severe nausea and vomiting", "Dehydration and confusion", "Low
        // blood pressure and fainting") and that THIS page files as a 911 call. A
        // reader whose body has stopped making its own steroid was being told to
        // wait for the same day on the textbook presentation of a condition the
        // same paragraph says can kill.
        foreach (var crisisSign in new[] { "confused", "being sick again and again", "faint" })
        {
            Assert.Contains(crisisSign, sameDay.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
        }

        // Therefore the conditional must lift ALL of it, in the shape
        // Content/blocks/escalation.md uses for a reader with a shunt, and the
        // triad must carry the 911 route there at the strength it has here.
        Assert.Contains("the whole same-day list below is a", steroids, StringComparison.Ordinal);
        Assert.Contains("do not wait for the same day on any of it", steroids,
            StringComparison.Ordinal);
        Assert.Contains("are what an adrenal crisis looks like", steroids,
            StringComparison.Ordinal);
        Assert.Contains("call 911, or your local emergency number, and get to a hospital",
            steroids, StringComparison.Ordinal);

        var scopedToOneBullet = new Regex(
            @"\bone (?:item|bullet|line|thing) on the list\b", RegexOptions.IgnoreCase);
        Assert.Matches(scopedToOneBullet, "one item on the list below is different for you");
        Assert.False(scopedToOneBullet.IsMatch(steroids),
            "the replacement-steroid conditional has been narrowed back to a single bullet, "
            + "while that page's same-day list still carries the whole adrenal-crisis triad");

        // And this page routes there, which it did not before: a reader on
        // replacement can reach that page from craniotomy or pre-surgery and had
        // no way back to the rule that is theirs.
        Assert.Contains("/treatments/steroids", Page, StringComparison.Ordinal);

        // A REAL CANARY, which the first version was not. It asserted the
        // unscoped string was absent FROM THE PAGE — a second negative about the
        // real file, which demonstrates nothing about whether `leadPattern` can
        // fire at all. A canary has to be run against planted text (§12.8,
        // WI-523), the way the apoplexy and rename guards in this file do it.
        var leadPattern = new Regex(@"\*\*(If you take steroid replacement[^*]*)\*\*");
        Assert.Matches(leadPattern, "**If you take steroid replacement, being ill is its own rule.**");
        Assert.DoesNotMatch(leadPattern, "**Being ill is its own rule.**");
    }

    [Fact]
    public void BothEmergenciesAreFlaggedInTheShortVersionAndHandedToTheCaregiver()
    {
        // §12.3: most readers get a fifth of a page. An emergency that only
        // exists eight sections down has not been told to most of the people it
        // is for — so the short version says they exist and where they are.
        var shortVersion = PlainOf("The short version");
        Assert.Matches(new Regex(@"Two things on this page are emergencies", RegexOptions.IgnoreCase),
            shortVersion);
        Assert.Matches(new Regex(@"worth knowing before you need them", RegexOptions.IgnoreCase),
            shortVersion);

        // And the caregiver is given them explicitly, because for both
        // emergencies the person who notices first is often not the patient:
        // apoplexy can come with confusion, and adrenal crisis with vomiting.
        var care = PlainOf(CareHeading);
        Assert.Matches(new Regex(@"Learn the two emergency rules with them", RegexOptions.IgnoreCase),
            care);
        Assert.Matches(new Regex(@"You may be the one who\s*notices first", RegexOptions.IgnoreCase),
            care);
        Assert.Matches(new Regex(@"neither can wait for morning", RegexOptions.IgnoreCase), care);

        // THE ENTRY IS THE HEADACHE ALONE IN ALL THREE PLACES IT APPEARS —
        // /review round 3's BLOCKER B1. Fixing the tier and leaving the summary
        // and the caregiver section conjoined would leave the narrower rule in the
        // two places most readers actually reach: §12.3 says most people get a
        // fifth of a page, and for this emergency the person who notices first is
        // often not the patient.
        var conjoined = new Regex(
            @"headache with (?:a )?(?:change in your sight|sight change|vision change)",
            RegexOptions.IgnoreCase);
        Assert.Matches(conjoined, "a sudden bad headache with a change in your sight");
        Assert.Matches(conjoined, "The sudden headache with a sight change");

        foreach (var (where, text) in new[]
                 {
                     ("the short version", shortVersion),
                     ("the caregiver section", care),
                     ("the symptoms section", PlainOf(SymptomHeading)),
                 })
        {
            Assert.False(conjoined.IsMatch(text),
                $"{where} still enters this emergency on a headache AND a sight change, so the "
                + "commonest presentation — the headache on its own — does not match the rule");
        }

        // AND THE GUARD ABOVE POLICES ONLY THE CONJUNCT I HAPPENED TO WRITE —
        // /review round 4's should-fix 1, which is this item's recurring defect in
        // a new place. `conjoined` bans a SIGHT-CHANGE conjunct, while the tier's
        // own guard bans any conjunction at all, so "A sudden, severe headache
        // WITH DOUBLE VISION, and being ill…" would narrow the two places most
        // readers actually reach and leave every assertion green. That is exactly
        // the axis the round-3 blocker failed on, guarded on one wording.
        //
        // The property is ADJACENCY, not vocabulary: in both sentences the entry
        // is followed immediately by the second emergency, so anything inserted
        // between them is a narrowing whatever word introduces it.
        var entryIntact = new Regex(@"sudden, severe headache,\s*and being\s+ill\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(entryIntact, "A sudden, severe headache, and being ill while on steroid replacement.");
        Assert.DoesNotMatch(entryIntact, "A sudden, severe headache with double vision, and being ill on steroid replacement.");
        Assert.DoesNotMatch(entryIntact, "A sudden, severe headache with a change in your sight, and being ill.");

        foreach (var (where, text) in new[]
                 {
                     ("the short version", shortVersion),
                     ("the caregiver section", care),
                 })
        {
            Assert.True(entryIntact.IsMatch(text),
                $"{where} no longer names the headache as the whole entry: something has been "
                + "inserted between it and the second emergency, which narrows the rule");
        }

        // THE DESCRIPTION MADE A UNIQUENESS CLAIM THIS ITEM'S OWN FIX FALSIFIED —
        // round 4's should-fix 3. It ended "the two emergencies this tumor has
        // that other brain tumors do not", while B2 rescoped the adrenal rule onto
        // the SUPPRESSED AXIS, so it now reaches a reader on long-term
        // dexamethasone for a glioma. It also filed this tumor among "other brain
        // tumors" on the one page whose thesis is that it is not one. §12.3 and
        // WI-524: the description renders as the first paragraph a reader meets,
        // and nothing guarded a claim made there.
        var uniqueness = new Regex(
            @"that other brain tumors do not\b|\bunlike (?:any |all )?other brain tumors\b|"
            + @"\bno other brain tumor\b|\bthe only brain tumor\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(uniqueness,
            "the two emergencies this tumor has that other brain tumors do not");
        Assert.Matches(uniqueness, "This is unlike other brain tumors.");
        Assert.DoesNotMatch(uniqueness, Plain);

        // The symptoms section hands them on to the household too.
        Assert.Matches(new Regex(@"worth telling the people you live with", RegexOptions.IgnoreCase),
            PlainOf(SymptomHeading));
    }

    [Fact]
    public void TheSharedBlockIsIncludedUneditedAndThePageAddsItsOwnTierBeneathIt()
    {
        // §12.10's remedy, applied literally: include the block, then add what
        // is true HERE and false on the eight other pages that include it.
        // Editing Content/blocks/escalation.md to carry apoplexy would put a
        // pituitary emergency on every glioma hub (WI-514's blast radius).
        var raw = Section(SymptomHeading);
        Assert.Contains("[ESCALATION]", raw, StringComparison.Ordinal);

        // The page's own tier comes AFTER the block, so a reader meets the
        // general rules first and the exceptions second.
        var directive = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        var own = raw.IndexOf("This tumor has two emergencies of its own", StringComparison.Ordinal);
        Assert.True(own > directive,
            "the page's own emergency tier sits above the shared block, so a reader meets the "
            + "exceptions before the rule");

        // And it says WHY it is adding one, rather than appearing to contradict
        // the list directly above it.
        Assert.Matches(new Regex(@"the list above does not cover\s*them", RegexOptions.IgnoreCase),
            PlainOf(SymptomHeading));

        // THE FORWARD POINTER, ABOVE THE BLOCK. Every other hub that adds a
        // scoped rule signposts it first — /tumors/pediatric-brain-tumor,
        // /tumors/medulloblastoma, /tumors/diffuse-midline-glioma and
        // /tumors/ependymoma all say the rule "comes just after the lists" or "at
        // the end of the section … below". This page put the tier in the right
        // place and omitted the signpost, so a reader met the most specialised
        // rule on the page with no warning it was coming — WI-538's own blocker
        // shape. Worded deliberately UNLIKE the siblings: "of the signs below
        // carry a different rule" is an eight-word run with four content words
        // and would collide with the pediatric hub.
        var pointer = Section(SymptomHeading);
        // "warning signs" was /review round 3's nit and it was right: apoplexy and
        // an adrenal crisis are EVENTS, and the block immediately below this
        // pointer is itself a list of warning signs — so the old wording read as
        // though two bullets of that list were being singled out.
        var pointerAt = pointer.IndexOf("Two emergencies here have rules of their own",
            StringComparison.Ordinal);
        var blockAt = pointer.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        Assert.True(pointerAt >= 0 && blockAt > pointerAt,
            "the forward pointer is missing or sits below [ESCALATION], so a reader meets the "
            + "tumor's own rule with no warning that it was coming");

        // THE BLOCK ITSELF IS UNTOUCHED.
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        foreach (var absent in new[]
                 {
                     "apoplexy", "pituitary", "steroid replacement", "adrenal",
                     "keep pills down", "medical alert", "drooping eyelid",
                 })
        {
            Assert.DoesNotContain(absent, block, StringComparison.OrdinalIgnoreCase);
        }

        // The tiers are diffed against the siblings by the shared helper.
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

        // ONE ESCALATION AUTHORITY ON THE PAGE (§12.10). Every guard above is
        // scoped to the symptoms section, so a second tier list anywhere else
        // would be invisible to all of them.
        var urgency = new Regex(
            @"\b(?:right away|straight away|immediately|urgent\w*|999|911|the same day|"
            + @"the same hour|as soon as|quickly|cannot wait|whatever the hour)\b",
            RegexOptions.IgnoreCase);
        var section = CuratedPage.Flatten(Reader(Section(SymptomHeading)));
        var strays = Regex.Matches(CuratedPage.Flatten(Reader(CuratedPage.ReaderText(Page))),
                @"\*\*(.+?)\*\*")
            .Where(m => urgency.IsMatch(m.Groups[1].Value))
            .Select(m => m.Value)
            .Where(bold => !section.Contains(bold, StringComparison.Ordinal))
            .ToList();
        Assert.True(strays.Count == 0,
            "an urgency directive sits outside the symptoms section, where it competes with the "
            + "shared block and with this page's own tier:\n  " + string.Join("\n  ", strays));
    }

    // ------------------------------------------------------- the two renames

    [Fact]
    public void AllThreeRenamesAreNamedAsRenamesRatherThanAsSeparateDiagnoses()
    {
        // The reader's problem is a report carrying two words for one thing.
        // A page that explains both words without saying they are THE SAME
        // THING has given somebody a second diagnosis.
        var section = PlainOf(ReportHeading);

        Assert.Matches(new Regex(@"may carry two names for the same thing, and neither is a mistake",
            RegexOptions.IgnoreCase), section);

        // 1. adenoma -> PitNET. The RENAME is attributed to WHO; the OBJECTION is
        //    attributed to the editors of the journal, NOT to the Pituitary
        //    Society — see the note at the next assertion, which records why.
        //    Nothing from PMC9170656 is asserted as fact (§12.13).
        //
        //    This comment used to say "the objection attributed to the Pituitary
        //    Society", seven lines above a comment saying the opposite, because
        //    /review's S2 was fixed in the prose and not in the reasoning around
        //    it. A stale rationale is worse than a stale assertion: it argues,
        //    with apparent authority, for putting the error back.
        Assert.Matches(new Regex(@"The older name is adenoma", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"World Health\s*Organization now prefers pituitary neuroendocrine tumor",
            RegexOptions.IgnoreCase), section);
        // ATTRIBUTED TO WHAT THE FETCHED SOURCE ACTUALLY IS. The page said "The
        // Pituitary Society … has objected in print" until /review's S2. The
        // document behind it (PMC9170656) says outright "This editorial expresses
        // the personal views of the authors" and is written by the journal's
        // Editor-in-Chief and board — NOT a Society position. The real Society
        // statement is a different paper it cites, from 2019, which was never
        // fetched and is therefore not cited (§12.2: no claim without a live
        // citation; WI-538 shipped nine citations written from memory).
        Assert.Matches(new Regex(
            @"The editors of the medical journal for this field have\s*objected in\s*print",
            RegexOptions.IgnoreCase), section);

        // And the Society is named only where the fetched text names it: the
        // AVP-D position statement lists its eight endorsing societies in full,
        // and the Pituitary Society is among them. So "opposite reasons" holds
        // while "the same body" did not.
        Assert.Matches(new Regex(@"Eight hormone societies endorsed it, including the Pituitary\s*Society",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"does not change what they are\s*under a microscope",
            RegexOptions.IgnoreCase), section);

        // 2. carcinoma -> metastatic PitNET, and the sentence that stops it
        //    reading as a new and worse thing.
        Assert.Matches(new Regex(@"Both names mean the same rare thing", RegexOptions.IgnoreCase),
            section);

        // 3. diabetes insipidus -> arginine vasopressin deficiency, WITH THE
        //    REASON, which is the part that makes it useful rather than
        //    bureaucratic: PMC9759163 records "desmopressin treatment was
        //    withheld with serious adverse outcomes, including death".
        Assert.Matches(new Regex(@"nothing to do with diabetes of the sugar kind",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"That shared word\s*caused real harm", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"treatment was held back", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"arginine vasopressin deficiency", RegexOptions.IgnoreCase),
            section);

        // THE PARENTHESES RULE, which is why a reader sees both at once and is
        // not watching two doctors disagree.
        Assert.Matches(new Regex(@"keep the old name in brackets", RegexOptions.IgnoreCase), section);

        // AND WHY THAT RENAME IS ON A TUMOR PAGE AT ALL, at the specificity the
        // source has. /review round 3's last nit: "one of the things that most
        // often follows an operation on the pituitary" was vaguer than NBK470458,
        // which names the ROUTE — "neurosurgery with a transsphenoidal approach
        // usually induces AVP-D". The precision added is the route and NOT the
        // frequency word, deliberately: this page also tells a post-operative
        // reader that most thirst in that setting is not AVP-D (round 2's S1), and
        // "usually" in both places would read to a reader as a contradiction.
        // NAMED, NOT PRONOUNED, and the read-back is what caught it. The reworded
        // sentence opened "It often follows the operation through the nose" — and
        // the nearest antecedent, one blank line up, is A REPORT that says
        // "arginine vasopressin deficiency (cranial diabetes insipidus)". So the
        // sentence parsed as "the report often follows the operation". The vaguer
        // original ("one of the things that most often follows an operation") was
        // imprecise but carried its own noun; sharpening it exposed the pronoun.
        // A fix is new prose and gets the same reading the draft did.
        Assert.Matches(new Regex(
            @"Arginine vasopressin deficiency often follows the operation through the\s*nose",
            RegexOptions.IgnoreCase), section);

        // AND THE SENTENCE THAT TIES THE THREE TOGETHER. Two renames made for
        // OPPOSITE reasons is the most useful thing this section can say, and
        // it is the sentence most likely to be trimmed as editorialising.
        Assert.Matches(new Regex(@"Two renames on one page, made for opposite reasons",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"a word was getting\s*people hurt", RegexOptions.IgnoreCase),
            section);

        // NOTHING IS ASSERTED AS THE PAGE'S OWN VIEW. The renaming argument is
        // live among specialists, and a page that picks a side is telling a
        // reader their report is wrong.
        var takesSides = new Regex(
            @"\bthe (?:new|old) name is (?:better|worse|right|wrong)\b|"
            + @"\bshould (?:never )?have been renamed\b|\bthe rename was a mistake\b|"
            + @"\bdoctors are wrong\b|\bignore the (?:new|old) name\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(takesSides, "The rename was a mistake and the old name is better.");
        Assert.DoesNotMatch(takesSides, Plain);
    }

    [Fact]
    public void ThePitnetReadingIsDefusedRatherThanConfirmed()
    {
        // PMC11226279's patient-facing objection: the new label risks "needless
        // frustrations and apprehension among most of the patients diagnosed
        // with benign PAs". A reader who reads "tumor" where their parent read
        // "adenoma" has been frightened by a word change and nothing else, and
        // this page is the only place in the corpus that can say so.
        var section = PlainOf(ReportHeading);

        Assert.Matches(new Regex(
            @"the new name risks needless worry for people whose tumor is not\s*dangerous",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"that reading is one\s*that specialists themselves predicted and argued against",
            RegexOptions.IgnoreCase), section);

        // And it never turns the defusing into a promise about this reader's
        // own tumor, which is the §12.12 direction that does the damage.
        var promise = new Regex(
            @"\byour tumor is not dangerous\b|\byours is (?:benign|harmless|fine)\b|"
            + @"\bnothing has changed (?:for|about) you\b|\bthe name is all that changed\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(promise, "The name is all that changed, so nothing has changed for you.");
        Assert.DoesNotMatch(promise, Plain);
    }

    // --------------------------------------------------------- "benign"

    [Fact]
    public void BenignIsHandledRatherThanUsedAsComfort()
    {
        // THE SOURCE HANDS THIS PAGE ITS OWN WORST SENTENCE. The Endocrine
        // Society: pituitary tumors "are not brain tumors and are almost always
        // benign (non-cancerous)". True, citable, and the natural way to open —
        // and a reader who takes it home is not prepared to lose part of their
        // sight to a growth the size of a pea.
        //
        // /tumors/meningioma solved this first. The STANCE is borrowed; the
        // sentences are this page's own, because the restatement guard objects
        // to borrowed prose and is right to.
        var grade = PlainOf(GradeHeading);

        Assert.Matches(new Regex(@"that word does more work\s*than it can carry",
            RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"It does not mean harmless", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"can take part of your sight", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(
            @"Not\s*cancer and not a problem are two different sentences", RegexOptions.IgnoreCase),
            grade);

        // Never offered as the reassurance, in any of the shapes that do not
        // need the word itself.
        var comfort = new Regex(
            @"\b(?:it is|its|it's) (?:only |just |merely )?benign\b|"
            + @"\bbenign,? so\b|\bgood news:? (?:it is |it's )?benign\b|"
            + @"\bbenign(?:,| and) (?:which means |meaning )?(?:harmless|nothing to worry)\b|"
            + @"\bthe good (?:part|thing|bit|news)\b|"
            + @"\b(?:never|cannot|can't|will not|won't|does not|doesn't) spread\b|"
            + @"\b(?:cannot|can't|will not|won't|is not going to) kill you\b|"
            + @"\bnothing to worry about\b|\bthe lucky (?:one|kind)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(comfort, "The good news is it is benign.");
        Assert.Matches(comfort, "It is benign, so there is nothing to worry about.");
        Assert.Matches(comfort, "The good part is that it is not cancer.");
        Assert.Matches(comfort, "It will never spread anywhere else in your body.");
        Assert.DoesNotMatch(comfort, Plain);

        // THE SAME TRAP ONE LEVEL UP, AND IT IS THIS PAGE'S OWN. "Not a brain
        // tumor" is true, is the single most repeatable sentence here, and is
        // the one a reader will use to conclude this is not serious. The page
        // states it plainly AND closes the wrong reading in the same breath.
        var what = PlainOf("What is a pituitary tumor?");
        Assert.Matches(new Regex(@"These are not brain tumors, and that is worth saying plainly",
            RegexOptions.IgnoreCase), what);
        Assert.Matches(new Regex(@"That matters, and it does not settle everything",
            RegexOptions.IgnoreCase), what);
        Assert.Matches(new Regex(
            @"What it\s*tells you is where the trouble comes from, not how much trouble you have",
            RegexOptions.IgnoreCase), what);

        // And the reader who was told "brain tumor" by their own team is not
        // told their team was wrong.
        Assert.Matches(new Regex(@"That is not a mistake either", RegexOptions.IgnoreCase), what);
    }

    // -------------------------------------------------- numbers and schedules

    [Fact]
    public void ThePageCarriesNoDoseNoHormoneThresholdAndNoMeasurement()
    {
        // §12.4 R1: all mg and Gy figures are out. The sources are thick with
        // them — NBK559222 alone carries hydrocortisone milligram doses and two
        // sets of deficiency percentages — and none of it is a number this
        // reader acts on.
        var clinicalNumber = new Regex(
            @"\b\d+(?:\.\d+)?\s*(?:Gy|gray|mg|milligrams?|cm|centimet\w*|mm|mcg|micrograms?)\b|"
            + @"\b" + CountWord + @"\s*(?:Gy|gray|mg|milligrams?|centimet\w*|micrograms?)\b|"
            + @"\b\d+\s+fractions?\b|\b" + CountWord + @"\s+fractions?\b|"
            + @"\bgrays?\b|\bmillimet\w*|\bmilligram\w*|\bcentimet\w*|\bmicrogram\w*|"
            + @"\b(?:smaller|larger|bigger|less) than\s+" + CountWord + @"\s*(?:cm|centimet\w*|mm)\b|"
            // A THRESHOLD IN ANY UNIT, OR NONE. A reader measuring their own
            // report against "under 10 mm" has been handed a decision that is
            // not theirs, and dropping the unit does not give it back.
            + @"\b(?:more than|less than|fewer than|no more than|at least|up to|"
            + @"under|over|above|below)\s+" + CountWord + @"\b|"
            + @"\bthe size of a\b|\bor smaller\b|\bor bigger\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "The usual replacement dose is 20 mg of hydrocortisone a day.",
                     "Radiation is given as 45 Gy in 25 fractions.",
                     "A macroadenoma is one larger than 10 mm.",
                     "Tumors under one centimetre are usually watched.",
                     "Double the dose to forty milligrams on sick days.",
                     "It is about the size of a pea once it is more than twenty millimetres.",
                 })
        {
            Assert.Matches(clinicalNumber, known);
        }

        // "about the size of a pea" describes the GLAND, not the tumor, and is
        // the page's one deliberate size image — a reader cannot act on the
        // size of their own pituitary. Redacted rather than exempted, so the
        // rest of its sentence still faces the guard (§12.8, WI-527).
        var plain = Plain.Replace("about the size of a pea", " ", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(clinicalNumber, plain);

        // And the page hands the numbers over rather than being silently silent
        // (§12.8, WI-521).
        Assert.Matches(new Regex(@"Your team will tell you\s*which", RegexOptions.IgnoreCase),
            PlainOf("How do doctors find out it is this?"));
    }

    [Fact]
    public void ThePageCarriesNoShareAndNoPrognosisFigure()
    {
        // §12.5 and §12.2 item 5. The sources offer vision-recovery rates,
        // CSF-leak rates, AVP-D incidence, the Endocrine Society's "about 40%
        // of pituitary tumors", and two survey figures. None reaches the page.
        var share = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b" + CountWord + @"\s+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            + @"\b" + Fraction + @"\s+of\s+(?:all\s+)?(?:the\s+)?(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|tumors|cases|adenomas)\b|"
            + @"\b" + Fraction + @"\s+(?:all\s+)?the\s+(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|tumors|cases|adenomas)\b|"
            + @"\b(?:roughly|about|around|nearly|approximately|between|something|"
            + @"just over|just under)\s+(?:a|one)\s+" + Fraction + @"\b|"
            + @"\b(?:is|are|was|were|makes? up|accounts? for)\s+"
            + @"(?:roughly\s+|about\s+|around\s+|nearly\s+)?(?:a|one)\s+" + Fraction + @"\b|"
            + @"\b" + Fraction + @"\s+the\s+time\b|"
            + @"\bmajorit(?:y|ies)\b|\bminorit(?:y|ies)\b|\ba handful\b|"
            + @"\b(?:survival|survive|live for|life expectancy|lifespan)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "About 40% of pituitary tumors make prolactin.",
                     "Sight comes back in three out of four people.",
                     "A quarter of them need hormone replacement afterwards.",
                     "Most survival figures for this tumor are reassuring.",
                     "Life expectancy is normal for most people.",
                     "It is the large majority.",
                 })
        {
            Assert.Matches(share, known);
        }
        Assert.DoesNotMatch(share, Plain);

        // "Almost all", "most" and "usually" ARE used — they are the §12.4 R3
        // direction-only shape, and banning them would force the page to stop
        // saying true things it has a source for. What is banned is the number
        // behind them, which is the branch above.
        //
        // NAMED, NOT COUNTED (/review's N4). This used to be
        // Assert.Contains("Almost", Plain) — a floor any page in the corpus
        // clears, which documented the intent without guarding it. The property
        // is that the page's central claim survives IN THE DIRECTION-ONLY SHAPE,
        // with no figure attached.
        Assert.Matches(new Regex(@"Almost\s*all of them are not cancer", RegexOptions.IgnoreCase),
            PlainOf("The short version"));
        Assert.Matches(new Regex(@"Almost always, no", RegexOptions.IgnoreCase),
            PlainOf(GradeHeading));

        // TWO SENTENCES REMOVED AS UNSOURCED — /review round 3's S5. Both were
        // checked against all nine files in the item's pack BEFORE being cut,
        // which is the opposite of round 2's S7, where a truncated grep made me
        // delete a sentence that was sourced after all. "Most of them are slow,
        // and many are found by chance" is an epidemiological claim; "Most people
        // go back to work and to ordinary life" is a prognosis claim in §12.5's
        // territory, on a page that publishes no prognosis at all.
        //
        // The reviewer proposed citing an incidentaloma guideline for the first.
        // That paper is in NO fetched pack, so citing it would have been WI-538's
        // citation-written-from-memory defect (§12.2: no claim without a live
        // citation) — the third time this item has had to decline a remedy whose
        // underlying finding was right.
        // A THIRD JOINED THEM AT ROUND 4, filed as a nit and promoted on checking:
        // "Some hormone tests are timed, and some take a morning". The pack says
        // only "Additional tests, called stimulation tests might be needed to check
        // your pituitary function" — neither "timed" nor "a morning" appears in any
        // of the nine files. The detail is real and sits in tests-library.md §9,
        // but that is a DOSSIER rather than a fetched source, which is precisely
        // the footing the two sentences above were deleted for. WI-551 owns it.
        var unsourced = new Regex(
            @"found by chance\b|\bgo back to work\b|\bmost of them are slow\b|"
            + @"\btests are timed\b|\btake a morning\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(unsourced, "Many are found by chance on a scan taken for something else.");
        Assert.Matches(unsourced, "Most people go back to work and to ordinary life.");
        Assert.Matches(unsourced, "Some hormone tests are timed, and some take a morning.");
        Assert.DoesNotMatch(unsourced, Plain);
    }

    [Fact]
    public void TheOutlookGateIsClosedAndCarriesNoFigureAtAll()
    {
        // §12.5. The gate is consent to READ, not a licence to publish.
        var raw = Regex.Match(Page, @"^## What might happen over time.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(raw), "the outlook section has gone");

        // `\r?\n` EVERYWHERE, never a bare `\n` or an `\s*` standing in for a
        // blank line: on a CRLF checkout `\s*\n\n` cannot match `\r\n\r\n` at
        // all, and a test that is broken on CRLF fails a break harness for free
        // (§12.8, WI-528).
        Assert.Matches(new Regex(
            @"^## What might happen over time[ \t]*\r?\n[ \t]*\r?\n:::outlook",
            RegexOptions.Multiline), raw);
        Assert.Matches(new Regex(@"\r?\n:::\r?\n"), raw);

        var inside = Regex.Match(raw, @":::outlook(.*?)\r?\n:::", RegexOptions.Singleline)
            .Groups[1].Value;

        // NO DIGIT AT ALL inside the gate. Meningioma's version has to allow
        // grades; this page has none to allow, so the rule is absolute.
        Assert.DoesNotMatch(new Regex(@"\d"), inside);

        // AND NO FIGURE-SHAPED CLAIM IN WORDS, which is the half a digit ban
        // cannot see (§12.8, WI-527).
        var told = new Regex(
            @"\blive as long as\b|\blife expectancy\b|\blifespan\b|\bsurviv\w*|\bdie of\b|"
            + @"\bcure[ds]?\b|\b(?:almost|nearly) (?:everybody|everyone|all of them)\b|"
            + @"\bmost people\b|\bthe vast majority\b|\bdone with it\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(told, "People with this tumor live as long as anybody else.");
        Assert.DoesNotMatch(told, CuratedPage.Flatten(inside));

        // The median is EXPLAINED rather than used, and the explanation ends on
        // the part that matters to one reader rather than to a group.
        var flat = CuratedPage.Flatten(inside);
        Assert.Matches(new Regex(@"the median is the halfway mark", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(
            @"It tells you\s*nothing about which side you are on", RegexOptions.IgnoreCase), flat);

        // And the gate closes on a door rather than on a fact (§12.6): asking is
        // a choice, and it is reversible.
        Assert.Matches(new Regex(@"Choosing not to is also a choice, and it is one you\s*can change",
            RegexOptions.IgnoreCase), flat);

        // Nothing sits outside the gate in that section but the heading.
        var outside = raw
            .Replace(Regex.Match(raw, @":::outlook.*?\r?\n:::", RegexOptions.Singleline).Value, "",
                StringComparison.Ordinal)
            .Replace("## What might happen over time", "", StringComparison.Ordinal);
        Assert.True(string.IsNullOrWhiteSpace(outside),
            "something sits outside the outlook gate in that section: " + outside.Trim());
    }

    [Fact]
    public void ThePagePublishesNoSurveillanceIntervalAndRoutesToTheSiblingThatOwnsWatching()
    {
        // WI-521's ruling, and /treatments/watch-and-wait already names this
        // tumor in its who-is-watched list and carries the per-tumor note. A
        // second schedule here is the two-strengths problem (§12.10); an
        // interval a reader can act on wrongly is the reason the ruling exists.
        var interval = new Regex(
            @"\b(?:every|each)\s+" + CountWord + @"?\s*(?:months?|years?|weeks?)\b|"
            + @"\bannual(?:ly)?\b|\b" + CountWord + @"[-\s]*(?:to|or)[-\s]*" + CountWord
            + @"\s*(?:months?|years?)\b|"
            + @"\bevery (?:other )?year\b|\b" + CountWord + @"\s*(?:monthly|yearly)\b|"
            + @"\bscan(?:s|ned)? (?:at|after)\s+" + CountWord + @"\s*(?:months?|years?)\b|"
            + @"\b(?:six|three|twelve)[- ]month\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "A repeat scan at six months, then annually.",
                     "Hormone levels are checked every three months at first.",
                     "Most people have a six-month scan after surgery.",
                     "Sight is tested every other year once things settle.",
                 })
        {
            Assert.Matches(interval, known);
        }
        Assert.DoesNotMatch(interval, Plain);

        // The interval is HANDED OVER rather than silently omitted.
        var scans = PlainOf(ScansHeading);
        Assert.Matches(new Regex(@"Ask your team for your own timetable", RegexOptions.IgnoreCase),
            scans);
        Assert.Matches(new Regex(@"ask what would make it change", RegexOptions.IgnoreCase), scans);

        // And the sibling owns the subject.
        Assert.Contains("/treatments/watch-and-wait", PlainOf("How is it usually treated?"),
            StringComparison.Ordinal);

        // READ rather than assumed (§12.10): the sibling still names this tumor.
        var sibling = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "watch-and-wait.md")));
        Assert.Contains("pituitary", sibling, StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------ the block rulings

    [Fact]
    public void ExactlyThreeBlocksAreIncludedAndTheTwoExclusionsAreClosedSet()
    {
        // THE BLOCKER THAT WOULD BE AN ABSENCE. An excluded block leaves no
        // trace on the page — no heading, no directive, nothing to grep — so a
        // later item can restore one and every other guard in this file stays
        // green. §12.8's WI-527 lesson, stated the other way round: a contract
        // item this test does not name is not a contract item, and an exclusion
        // this test does not name is not an exclusion.
        //
        // An EXACT SET, not a pair of DoesNotContain checks. A new block added
        // in two years fails here and has to be ruled on deliberately.
        Assert.Equal(
            ["caregiver", "causes", "escalation"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        // Each included one is composed rather than restated, under its own
        // §12.3 heading.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);

        // THE MECHANISM BLOCK IS WRONG HERE, NOT MERELY UNNECESSARY. It opens
        // "These are the ways a tumor in the brain causes symptoms" and then
        // asserts brain swelling, seizures and blocked fluid needing a shunt,
        // with a lobe-by-lobe map and NO scoping clause. This tumor is not in
        // the brain, rarely seizes, and does not obstruct the fluid pathways.
        //
        // So the vocabulary it would have imported is banned in THIS PAGE'S OWN
        // PROSE. Read that literally: `Plain` comes from the RAW file, so these
        // bans prove the page does not RETYPE the mechanism block's material
        // itself. They do NOT guard the exclusion — reinstating the directive
        // inserts the string "[MECHANISM]", not the block's words, so not one of
        // them would fire. The exact closed set above is the whole guard for
        // that, which is why it is an exact set and not a pair of negatives.
        //
        // /review proposed running these against `Composed` so they would also
        // describe what the reader receives. THEY WOULD FAIL: the composed page
        // legitimately carries "shunt" and "seizure" from [ESCALATION]'s
        // conditional rules, which the rendered read confirms a reader meets.
        // A guard that fails a correct page is worse than no guard (§12.8,
        // WI-508), so the comment is corrected instead of the assertion.
        foreach (var absent in new[] { "shunt", "hydrocephalus", "lobe", "seizure" })
        {
            Assert.DoesNotContain(absent, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // THE CROSSWALK BLOCK IS ENTIRELY THE 2021 CNS REWRITE. This report has
        // no CNS grade, no NOS and no NEC, and importing glioma vocabulary into
        // this reader's identity section is WI-514's defect verbatim. §12.9
        // still wants a crosswalk SLICE from every hub, and this page's is three
        // pairs in its own prose — asserted above, and asserted here to be the
        // page's own rather than the block's.
        // TWO CASE POLICIES, TWO LOOPS — review round 2's S8. These used to share
        // one `Ordinal` loop, which is right for the acronyms and WRONG for
        // "molecular": a sentence-initial "Molecular testing is not used here"
        // walks straight through a case-sensitive ban, and sentence-initial is
        // exactly where the capital lives. §12.12 records that trap four times
        // over, and putting two policies in one loop reintroduced it.
        foreach (var acronym in new[] { "NOS", "NEC", "IDH", "1p/19q" })
        {
            Assert.DoesNotContain(acronym, Plain, StringComparison.Ordinal);
        }

        foreach (var word in new[] { "molecular", "gene panel", "methylation" })
        {
            Assert.DoesNotContain(word, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // And the grade sentence says what this reader's report DOES carry,
        // rather than leaving the absence to be filled in from a sibling hub.
        // /review's S8: the old sentence was "There is no grade 1 to 4 here of
        // the kind you may have read about", and the hedge "of the kind" carried
        // all the weight. Pituitary operative notes and radiology reports
        // routinely carry a Knosp grade (0-4) and older ones a Hardy grade, so a
        // reader holding "Knosp grade 3" could read the page as saying their
        // paperwork was wrong. Knosp itself is NOT published here — no fetched
        // source supports a description of it, and §12.2 does not publish a claim
        // without a live citation — so the sentence is tightened instead, and the
        // reader is given an action for any number they do not recognize.
        Assert.Matches(new Regex(
            @"The grade 1 to 4 you may have read about for other brain tumors is not used for\s*this one",
            RegexOptions.IgnoreCase), PlainOf(GradeHeading));
        Assert.Matches(new Regex(
            @"If a number does turn\s*up beside a word you do not know, ask what it measures",
            RegexOptions.IgnoreCase), PlainOf(GradeHeading));
    }

    [Fact]
    public void TheSelfBlameBlockIsScopedBecauseThisTumorIsNotABrainTumor()
    {
        // FOUND BY THE RENDERED READ, AND ONLY THE RENDERED READ COULD FIND IT.
        // Composed, [CAUSES] says "brain tumors" five times -- "For most brain
        // tumors, nobody knows the cause", "does not cause brain tumors",
        // "stress causes brain tumors", "a brain tumor", "Brain tumors are not
        // contagious" -- on a page whose thesis is that this is NOT a brain
        // tumor. A reader can reasonably conclude the section is not about them,
        // and self-blame is the one thing on this page it exists to answer.
        //
        // §12.10's mis-scoped block: true, but framed for a different reader.
        // The remedy is the one /tumors/all-brain-tumors already uses -- a
        // bridge sentence before the directive -- written in this page's own
        // words, because that page scopes for a different reason (no diagnosis
        // yet) and the restatement guard is watching.
        var section = PlainOf("Did I cause this?");

        Assert.Matches(new Regex(@"What follows is written about brain tumors in general",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"This tumor is not\s*one", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"the question is the same, and so are\s*the answers",
            RegexOptions.IgnoreCase), section);

        // BEFORE the directive, not after it. A bridge a reader meets on the way
        // out has not bridged anything (§12.8, WI-512: position was the property).
        var raw = CuratedPage.Flatten(Section("Did I cause this?"));
        var bridge = raw.IndexOf("What follows is written about brain tumors in general",
            StringComparison.Ordinal);
        var directive = raw.IndexOf("[CAUSES]", StringComparison.Ordinal);
        Assert.True(bridge >= 0 && directive > bridge,
            "the scoping bridge does not sit above [CAUSES], so the reader meets the block's "
            + "'brain tumors' framing before anything reconciles it with this page");
    }

    [Fact]
    public void TheCaregiverSectionAddsWhatIsOnlyTrueHere()
    {
        // §12.10: a caregiver section that paraphrases the block in different
        // words is the defect WI-525 wrote down and WI-526 repeated, and no
        // shingle check can see a paraphrase.
        var care = PlainOf(CareHeading);

        // TWO, AND THE SECTION SAYS TWO. A third lead-in is how a paraphrase of
        // the block arrives.
        Assert.Matches(new Regex(@"Two things are specific to this tumor", RegexOptions.IgnoreCase),
            care);
        var leads = Regex.Matches(CuratedPage.Flatten(Reader(Section(CareHeading))), @"\*\*(.+?)\*\*")
            .Select(m => m.Groups[1].Value)
            .ToList();
        Assert.True(leads.Count == 2,
            $"the caregiver section promises two things and carries {leads.Count}:\n  "
            + string.Join("\n  ", leads));

        // The second is the one that exists nowhere else in the corpus: hormone
        // failure presenting as personality, which a caregiver reads as someone
        // giving up.
        Assert.Matches(new Regex(@"Hormone changes can look like mood or personality",
            RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"not from someone\s*giving up", RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"worth saying to the team rather than absorbing at home",
            RegexOptions.IgnoreCase), care);
    }

    // --------------------------------------------------------- housekeeping

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3, read from the spec rather than copied from the last hub
        // written — §12.8's WI-510 lesson is that copying the previous page is
        // how a template quietly shrinks.
        Assert.Equal(
            [
                "The short version",                                               // 0
                "What is a pituitary tumor?",                                      // 1
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

        // Outlook sits after everything actionable (§12.3's design principle),
        // and the self-blame block is demoted below "if it comes back" per §12.9.
        var headings = Headings();
        Assert.True(headings.IndexOf("What might happen over time")
            > headings.IndexOf("If it comes back"));
        Assert.True(headings.IndexOf("Did I cause this?") > headings.IndexOf("If it comes back"));

        // The two subsections of the treatment section, which carry the nose
        // route and the hormone aftermath.
        foreach (var sub in new[] { "The operation goes through your nose", "Hormones afterwards" })
        {
            Assert.Matches(new Regex($@"^### {Regex.Escape(sub)}\s*$", RegexOptions.Multiline),
                CuratedPage.Body(Page));
        }
    }

    [Fact]
    public void TheTranssphenoidalSliceIsProportionateAndLeavesTheRestToItsOwnPage()
    {
        // WI-553 does not exist yet and will own "no head incision, no shaved
        // head, nasal recovery, hormone follow-up". This page owns the route and
        // why it is not a craniotomy, and deliberately less than that page will.
        var section = PlainOf("What is treatment actually like, and what is normal afterwards?");

        Assert.Matches(new Regex(@"through your nose and the sinus behind it",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"no cut in your scalp and no shaved head", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"transsphenoidal", RegexOptions.IgnoreCase), section);

        // THE ONE THING A READER MUST ACT ON, and the reason this slice cannot
        // be deferred wholesale: clear fluid from the nose after this operation
        // needs checking, and nothing else in the corpus says so.
        Assert.Matches(new Regex(@"clear fluid running from your nose, which needs\s*checking",
            RegexOptions.IgnoreCase), section);

        // And the alternative route is named without being dwelt on.
        Assert.Matches(new Regex(@"the surgeon goes through the scalp instead",
            RegexOptions.IgnoreCase), section);

        // What belongs to WI-553 is NOT here. Asserted as absence so that
        // building that page does not silently duplicate this one.
        foreach (var deferred in new[] { "packing", "splint", "nasal rinse", "saline spray" })
        {
            Assert.DoesNotContain(deferred, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheHormoneAftermathIsNotCalledAComplication()
    {
        // The sentence a reader needs after an operation that leaves them on
        // replacement for life: this is an OUTCOME, not evidence that something
        // went wrong. §12.12's kinder direction, and it is also true.
        var section = PlainOf("What is treatment actually like, and what is normal afterwards?");

        Assert.Matches(new Regex(@"That is not a complication that went wrong",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"It is a known outcome and it is treatable",
            RegexOptions.IgnoreCase), section);

        // And it routes back to the emergency rule rather than restating it,
        // because a reader on steroid replacement meets that rule twice and it
        // must read the same both times (§12.10).
        Assert.Matches(new Regex(@"read the emergency rule further up this\s*page again",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"ask for your own written plan before you leave",
            RegexOptions.IgnoreCase), section);

        // /review's S4, and the source was already in this page's own front
        // matter: NBK470458 says "neurosurgery with a transsphenoidal approach
        // usually induces AVP-D". The page named the condition only in the REPORT
        // section, as vocabulary, with no action attached and nothing in the part
        // a reader actually reads after an operation. Thirst and urine output are
        // the other thing a person at home can observe, beside the CSF leak the
        // page already covers — and it is the classic reason people come back in.
        Assert.Matches(new Regex(
            @"Tell your team if you are very thirsty and passing a lot of water",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"arginine vasopressin deficiency", RegexOptions.IgnoreCase),
            section);

        // IT IS A POSSIBILITY, NOT AN IDENTIFICATION — review round 2's S1, and
        // the source is this page's own NBK470458: "Despite the relatively high
        // frequency of AVP-D in patients undergoing neurosurgery, MOST CASES OF
        // POLYURIA IN THIS SETTING ARE NOT DUE TO AVP-D. More common causes are
        // the excretion of excess fluid administered during surgery and an
        // osmotic diuresis induced by mannitol or glucocorticoids." The first
        // version of this rule said the symptom IS arginine vasopressin
        // deficiency, handing a frightened post-operative reader the diagnosis
        // its own citation calls the less likely one. The ACTION was right and is
        // unchanged; only the certainty moved.
        // A REAL CANARY, AND THE PROPERTY IS CERTAINTY RATHER THAN WORDING —
        // /review round 3's S6. The negative that used to close this block
        // asserted that "it is the thing named further up" was absent from the
        // page: a SECOND NEGATIVE about the real file, which demonstrates nothing
        // about whether the pattern can fire at all. It was also pinned to a
        // phrase the same review asked to be reworded, so it would have gone green
        // for the wrong reason. A canary runs against planted text (§12.8,
        // WI-523), the way the apoplexy and rename guards in this file do it.
        Assert.Matches(new Regex(@"It can be arginine vasopressin deficiency",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"there are other reasons for it too", RegexOptions.IgnoreCase),
            section);

        var identified = new Regex(
            @"\b(?:it|that|this) is arginine vasopressin deficiency\b", RegexOptions.IgnoreCase);
        Assert.Matches(identified,
            "Very thirsty and passing a lot of water after the operation? That is arginine "
            + "vasopressin deficiency.");
        Assert.DoesNotMatch(identified, section);

        // AND THE SIGHT SENTENCE IS BACK, in direction-only form. It was removed
        // in the round-1 pass as unsourced, on the strength of a grep that had
        // truncated and suppressed the line carrying it. The Endocrine Society
        // does say it: "After surgery, vision problems improve in most people, or
        // they can go away all together." Cutting it left a subsection about
        // losing your sight ending on an administrative sentence, which is what
        // §12.6 forbids.
        Assert.Matches(new Regex(@"Sight often improves once the pressure is off",
            RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheOnwardDoorsSitInTheSectionsWhoseReaderNeedsThem()
    {
        // WI-512's rule: presence was never the property, POSITION was. Each of
        // these routes out is offered where the question arises, not collected
        // in a footer a reader reaches only after the part they came for. A
        // whole-page Contains() passes just as well when every door has drifted
        // into the support list, which is the drift this guard exists to catch.
        //
        // This is also why EveryLinkOnThePageResolves names only /get-help-now:
        // the doors are real and checked, they are simply not where a library
        // page would put them.
        foreach (var (link, heading) in new[]
                 {
                     ("/tests/mri", "How do doctors find out it is this?"),
                     ("/tests/pathology-report", ReportHeading),
                     ("/treatments/watch-and-wait", "How is it usually treated?"),
                     ("/treatments/radiation-therapy", "How is it usually treated?"),
                     ("/treatments/craniotomy",
                         "What is treatment actually like, and what is normal afterwards?"),
                     ("/tests/follow-up-scans", ScansHeading),
                     ("/tests/waiting-for-results", ScansHeading),
                     ("/get-help-now", "Where to get support"),
                 })
        {
            Assert.Contains(link, Section(heading), StringComparison.Ordinal);
        }

        // NO BACK-LINK TO /tumors, and that is a finding rather than an
        // oversight. Only four of the sixteen tumor pages carry one, and on
        // /tumors/pediatric-brain-tumor it sits mid-body rather than in support
        // — so it is a common door, not a contract item. Asserted as absent so
        // that adding one later is a deliberate act with a reason attached.
        Assert.DoesNotContain("](/tumors)", Page, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST, and that is the ruling. Every collision this page had —
        // sixteen files at the first count — was reworded rather than exempted.
        //
        // The guard is BIDIRECTIONAL and that is what makes the ruling load-
        // bearing: this page's first draft turned /tumors/pediatric-brain-tumor
        // and /treatments/watch-and-wait red, two pages that had been green for
        // days. An allowlist here could not have fixed either, because neither
        // page's test reads this page's allowlist.
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

        // Page-local. "Benign" attracts a vocabulary of its own, and each of
        // these is corpus-clean. "harmless" is NOT banned outright — the grade
        // section exists to name the wrong conclusion and refute it, and banning
        // the word would stop the page saying the thing it is for (§12.8,
        // WI-509's rule applied to a list I wrote myself). It is banned OUTSIDE
        // the sentence that refutes it instead.
        foreach (var phrase in new[] { "nothing to worry about", "the good kind", "lucky" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(reader),
                StringComparison.OrdinalIgnoreCase);
        }

        var harmless = CuratedPage.SentencesOf(Plain)
            .Where(s => s.Contains("harmless", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.All(harmless, s => Assert.Contains("not mean", s, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // Two of the item's nine sources are British-adjacent in idiom, and the
        // first draft shipped "recognise".
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // Page-local idioms this tumor's sources reach for. "specialist nurse"
        // is NOT on this list and that is deliberate: ten other hubs use it, so
        // it is corpus convention rather than an outlier, and a ban here would
        // put this page out of step with all of them (§12.8 — a phrase belongs
        // on a ban list only if no correct sentence in the corpus contains it).
        //
        // THE NEXT THREE CAME FROM THE SECOND RENDERED READ, and the corpus
        // decided each one rather than my ear.
        //   * "tablets" — the plural ONLY, which is the distinction
        //     TestsLibraryPagesTests draws: `"tablet"` is deliberately off the
        //     shared list because US labeling says "take one tablet", and only
        //     "tablets" meaning PILLS IN GENERAL is the idiom. This page had
        //     eight of the second kind and none of the first. The corpus says
        //     "pills" 56 times across 14 files; body copy meaning medicine says
        //     "tablets" twice. SteroidsPageTests and TumorTreatingFieldsPageTests
        //     already ban it page-locally for the same reason a substring ban
        //     cannot: it would fail the pages where "one tablet" is correct.
        //   * "optician" and "junction" — zero occurrences anywhere else under
        //     Content/. Both were mine alone, and both name a thing a US reader
        //     calls something else (an eye doctor; an intersection). This is the
        //     idiom class that got Cancer Research UK barred as a source for this
        //     very page, so shipping it in my own prose would be the same defect
        //     one step further in.
        foreach (var form in new[]
                 {
                     "tumour", "commonest", "consultant", "chemist", "999", "theatre",
                     "tablets", "optician", "junction",
                     // THE VERB FORMS ONLY, and the reason is the rule itself: a
                     // phrase belongs here only if no correct sentence contains
                     // it. The stem "specialis" cannot be banned, because this
                     // page correctly says "specialist nurse" and "hormone
                     // specialist". Added because the fix for /review's S2
                     // introduced "the journal that specialises" — British
                     // spelling, written into the very edit that was removing
                     // British idiom, and invisible to CuratedPage.BritishForms.
                     "specialises", "specialised", "specialising",
                 })
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

        // The sources the page's load-bearing claims rest on, by name.
        foreach (var owed in new[]
                 {
                     "endocrine.org", "NBK559222", "PMC9170656", "PMC11226279",
                     "PMC9759163", "NBK470458", "niddk.nih.gov",
                 })
        {
            Assert.Contains(urls, u => u.Contains(owed, StringComparison.Ordinal));
        }

        // THE TWO THE STUB CITED, AND BOTH ARE BARRED. §12.1: "a page that
        // cites NCI for a tumor name is a defect", and naming is most of what
        // this page does. CRUK is barred for idiom (WI-538).
        foreach (var barred in new[] { "cancer.gov", "cancerresearchuk.org" })
        {
            Assert.False(urls.Any(u => u.Contains(barred, StringComparison.OrdinalIgnoreCase)),
                $"{barred} is cited; §12.1 bars it for naming and this page is mostly naming");
        }

        // The known-dead list.
        foreach (var dead in new[]
                 {
                     "academic.oup.com", "mdpi.com", "ascopubs.org", "mayoclinic.org",
                     "hopkinsmedicine.org", "sciencedirect.com", "journals.lww.com",
                     "medscape.com", "ajnr.org", "link.springer.com",
                     "connect.mayoclinic.org", "pituitarysociety.org",
                 })
        {
            Assert.False(urls.Any(u => u.Contains(dead, StringComparison.OrdinalIgnoreCase)),
                $"{dead} is cited; the front matter records why it was not used");
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S",
            RegexOptions.Multiline).Count);

        // THE UNCHECKABLE SOURCE IS DECLARED. cancer.org is JavaScript-rendered
        // and cannot be script-checked, so the front matter says so rather than
        // letting a claim-to-source map imply coverage it does not have (§12.13).
        Assert.Contains(urls, u => u.Contains("cancer.org", StringComparison.Ordinal));
        Assert.Matches(new Regex(@"CANNOT BE SCRIPT-CHECKED", RegexOptions.IgnoreCase), front);

        // THE FREQUENCY NOTE — /review round 3's S1. The note on the adrenal source
        // ended "but the page must not imply frequency, and it does not", four
        // hundred lines above a tier that closes "Both of these are uncommon". A
        // note describing a rule the page is already breaking is §12.14's shape,
        // and §12.13 makes the front matter the first place the next editor looks
        // to find out why a sentence reads as it does — so it would have argued,
        // with apparent authority, for deleting a correct and sourced word.
        //
        // The word stays: it is direction-only and both halves are sourced. The
        // verbatim that licenses the apoplexy half is now recorded where a script
        // can check it, and the false claim is gone.
        Assert.Contains("Apoplexy in pituitary adenomas is", front, StringComparison.Ordinal);

        const string falseNote = "frequency, and it does not";
        Assert.Contains(falseNote, "but the page must not imply frequency, and it does not.",
            StringComparison.Ordinal);
        Assert.DoesNotContain(falseNote, front, StringComparison.Ordinal);

        // NO BRACKETED DIRECTIVE NAME ANYWHERE IN THE FRONT MATTER. This is the
        // WI-538 trap and it bit this page's first draft: CaregiverSectionTests
        // locates the block with a raw IndexOf over the WHOLE file, so a
        // bracketed name in a comment becomes the first match and the page is
        // reported as not carrying the block under its heading.
        Assert.DoesNotMatch(new Regex(@"\[[A-Z][A-Z-]+\]"), front);
    }
}

/// <summary>The page as served.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class PituitaryTumorPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/pituitary-tumor";

    private readonly WebApplicationFactory<Program> _factory;

    public PituitaryTumorPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Pituitary tumor", html);
        Assert.Contains("the two things a team watches", html);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // ONE required door, and that is the page's shape rather than a
        // weakening. The helper scopes `requiredOnward` to the named section AND
        // asserts `onward.Count >= requiredOnward.Length`, so every link named
        // here is a claim that the SUPPORT SECTION carries it. This page routes
        // contextually instead — the MRI door from the diagnosis section, the
        // pathology door from the report section, watch-and-wait from treatment
        // — which is better placement and is pinned by
        // TheOnwardDoorsSitInTheSectionsWhoseReaderNeedsThem.
        //
        // The first version of this test named five, copied from
        // MeningiomaPageTests without checking where THIS page puts its doors.
        // Every link on the page is still fetched and still has to return 200;
        // it is only the "which section holds it" claim that changes.
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support", "/get-help-now");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page.
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
    public async Task TheThreeBlockDirectivesComposeAndTheTwoExcludedOnesLeaveNoTrace()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // No literal directive reaches a reader (§12.10, WI-514's trap).
        foreach (var directive in new[]
                 { "[ESCALATION]", "[CAUSES]", "[CAREGIVER]", "[MECHANISM]", "[CROSSWALK]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // Each included block's own content does arrive.
        Assert.Contains("Call an ambulance", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);

        // AND THE EXCLUDED BLOCKS' CONTENT DOES NOT — asserted on the RENDERED
        // page, because that is the only place a wrongly-composed block becomes
        // visible. Read out of the block files rather than re-typed, so
        // rewording a block cannot silently retire this check.
        foreach (var name in new[] { "mechanism", "crosswalk" })
        {
            var block = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, name + ".md"));
            // FLATTENED FIRST (/review's N3). `[^.\n]` cannot cross a line, so
            // over raw block text this could only ever pick a sentence that
            // happens to fit on one hard-wrapped line — rewrapping the block
            // would silently change which sentence is being guarded, with nothing
            // to show it had.
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
    public async Task TheGlossaryFiresOnTheWordThisPageDoesNotDefineItself()
    {
        // NO NEW GLOSSARY ENTRIES on this item: "prolactinoma", "acromegaly",
        // "Cushing", "hypopituitarism", "apoplexy", "PitNET" and
        // "transsphenoidal" appear ZERO times elsewhere under Content/, so each
        // is a FIRST use and none meets §12.8's second-use threshold. Each is
        // glossed inline instead. "extra-axial" already exists and fires on its
        // own, which is what this asserts.
        //
        // NAMED, NOT COUNTED. The first version asserted a bare "def-", which the
        // handproof showed was load-bearing ONLY BY ACCIDENT: extra-axial is this
        // page's single tooltip today, so any tooltip and THE tooltip were the
        // same assertion. The day another glossary term fires here, "def-" keeps
        // appearing while the term this guard exists for could be gone. The
        // renderer emits popovertarget="def-{slug}" (GlossaryTooltips), so the
        // slug is the thing to name (§12.8, WI-512: presence was never the
        // property).
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("def-extra-axial", html, StringComparison.Ordinal);

        // And the page NAMES the term while the glossary DEFINES it. The rendered
        // read caught the first draft doing both: the page said "a word for
        // growing outside the brain itself" and the popover answered "Growing
        // outside the brain itself, not inside it" -- WI-535's echo, word for
        // word. Suppressing the tooltip would have been the wrong fix, because
        // /tumors/meningioma glosses the same term inline and lets it fire, and
        // one term must not behave two ways on two pages.
        Assert.DoesNotContain("word for growing outside the brain itself",
            CuratedPage.Flatten(CuratedPage.Read("tumors", "pituitary-tumor.md")),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TheSiblingThatRoutesHereStillDoes()
    {
        // /treatments/watch-and-wait names this tumor in its who-is-watched
        // list and links back. Read rather than assumed (§12.10).
        var watch = await _factory.CreateClient().GetStringAsync("/treatments/watch-and-wait");

        Assert.Contains(Url, watch, StringComparison.Ordinal);
    }
}
