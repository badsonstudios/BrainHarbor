using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-541: acoustic neuroma, deepened. A §12.3 SEVENTEEN-SECTION TUMOR HUB, and
/// the third benign, locally-pushy, extra-axial tumor in a row — which makes its
/// neighbours a bigger hazard than its sources.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * THE ONE RULE, AND ITS ROUTE. This hub carries exactly ONE urgent rule and
///     it is deliberately NOT a 911 rule. NIDCD calls sudden deafness "a medical
///     emergency" and says "visit a doctor immediately"; HLAA names "a primary
///     physician, urgent care or ear, nose and throat specialist" and does NOT
///     say emergency room. Escalating it to 911 would be as much a defect as
///     under-stating it, so the route is pinned in BOTH directions.
///   * THE RULE THIS PAGE REFUSES TO CARRY. Brainstem compression and
///     hydrocephalus are described everywhere and acted on nowhere: EANO 2020
///     was read live and contains no patient-facing urgency guidance at all,
///     its "surgery the only option" being an indication for surgeons. Two
///     patient-facing sources say the opposite of act now. Building a warning
///     list out of surgical findings would invent a rule this tumor's own
///     literature does not give — WI-540's apoplexy ruling, one item on.
///   * THE GRADE, IN TWO SEPARATELY ATTRIBUTED HALVES. WHO CNS5 prints NO grade
///     beside "Schwannoma" (verified live, twice). The ARABIC CONVENTION is its;
///     the GRADE ITSELF is the clinical references'. Citing CNS5 for "grade 1"
///     would be §12.14's defect — a citation that does not support the claim in
///     the form it is made.
///   * NF2 SCOPED BY SIDEDNESS **AND AGE**, NEVER INVERTED. "Bilateral tumors
///     are the hallmark of NF2" means most people with NF2 have bilateral
///     tumors. It does NOT mean most people with one of these has NF2, and
///     inverting it terrifies the reader this page is mostly for.
///   * a prognosis figure, a percentage, a dose, a size threshold or a scan
///     interval (§12.4, §12.5, WI-521).
///
/// NO ALLOWLIST, and this file carries TWO restatement guards rather than one.
/// WI-541 learned the expensive way that the corpus has two different shingle
/// implementations: the shared <c>AssertDoesNotRestateTheCorpus</c> drops WHOLE
/// markdown links and the ask-list, while the page-local <c>Shingles</c> copied
/// into many test files keeps LINK TEXT and keeps the ask-list. This page scored
/// zero on the first and still turned /treatments/watch-and-wait and
/// /tumors/meningioma red through the second. Both are asserted here so the next
/// page cannot repeat it.
/// </summary>
public sealed class AcousticNeuromaPageContentTests
{
    private const string Slug = "tumors/acoustic-neuroma";
    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is an acoustic neuroma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string WhereHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string FindOutHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatedHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string EverydayHeading = "Everyday life";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string BackHeading = "If it comes back";
    private const string CareHeading = "For the person caring for someone with this";

    private static string Page => CuratedPage.Read("tumors", "acoustic-neuroma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// Title and description. <c>ReaderText</c> strips them, and
    /// <c>ContentPage.cshtml</c> renders the description as the first paragraph
    /// a reader meets (§12.3, WI-524) — so it is the least-guarded prose on the
    /// page unless a test reaches it deliberately.
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

    // ------------------------------------------------------------ the one rule

    [Fact]
    public void TheSuddenHearingLossRuleCarriesItsEntryItsHedgeAndItsRoute()
    {
        // THE ITEM'S ONLY EMERGENCY RULE, and the only one its sources support.
        // NIDCD, verified live: "you should consider sudden deafness symptoms a
        // medical emergency and visit a doctor immediately." HLAA: "Any sudden
        // hearing loss, especially in one ear, should be treated as a medical
        // emergency."
        var section = PlainOf(SymptomHeading);

        var rule = Regex.Match(section,
            @"If the hearing in one ear drops suddenly(.*?)(?=What is NOT on this page)",
            RegexOptions.Singleline);
        Assert.True(rule.Success, "the sudden-hearing-loss rule has gone from the symptoms section");
        var text = rule.Groups[1].Value;

        // SUDDEN IS DEFINED. Without this the rule is aimed at every reader whose
        // hearing has been slipping for years — which is most of them, and which
        // is the slow fade this same page describes three sections up.
        Assert.Matches(new Regex(@"over a few days, or all at once", RegexOptions.IgnoreCase), text);
        Assert.Matches(new Regex(@"not the\s*same as the slow fade", RegexOptions.IgnoreCase), text);

        // THE HEDGE, which is what keeps this page from frightening everyone who
        // has ever had a blocked ear. AAO-HNS, verified live: "SSNHL can rarely
        // be associated with benign tumors of the vestibular nerve."
        Assert.Matches(new Regex(
            @"Most sudden hearing loss is not caused by a growth like this one",
            RegexOptions.IgnoreCase), section);

        // THE DELAY DIRECTION, WITHOUT A FIGURE. NIDCD says "more than two to
        // four weeks"; AAO-HNS says "within the first 14 days". Two numbers for
        // one idea that do not agree are not a rule a reader can apply (§12.4,
        // WI-527's threshold shape), so the page carries the direction only.
        Assert.Matches(new Regex(@"works better the sooner it starts", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheRuleRoutesToPrimaryCareUrgentCareOrAnEntAndNotTo911()
    {
        // THE ROUTE IS THE RULING, and it is pinned in BOTH directions.
        //
        // HLAA, verified live: "Prompt medical attention by a primary physician,
        // urgent care or ear, nose and throat specialist (ENT) can improve your
        // chances of regaining hearing." It names those three and DOES NOT say
        // emergency room. NIDCD says "visit a doctor immediately".
        //
        // So this tier cannot be modelled on /tumors/pituitary-tumor's or
        // /tumors/craniopharyngioma's, both of which route to 911. Sending this
        // reader to an emergency department would be an over-escalation with no
        // source behind it, and under-stating it would cost hearing that treatment
        // can sometimes save. Both failures are guarded.
        var section = PlainOf(SymptomHeading);

        // THE WINDOW RUNS TO THE END OF THE REFUSAL, not just to the end of the
        // route paragraph (/review round 1, nit 10). The first version stopped at
        // "Most sudden hearing loss", leaving the two paragraphs after it — the
        // already-diagnosed route and the refusal — free to grow an
        // emergency-department route with nothing going red. The ban has to cover
        // every paragraph this page writes about what to do.
        // The anchor tracks the page: "straight away" was British idiom sitting
        // inside the one safety rule (/review round 2, nit 10), and moving it
        // moves this window with it — page edit and assertion are one change.
        var rule = Regex.Match(section,
            @"Ask to be seen quickly(.*?)(?=## |\z)",
            RegexOptions.Singleline);
        Assert.True(rule.Success, "the rule's route paragraph has gone");
        var route = rule.Groups[1].Value;

        foreach (var destination in new[] { "own doctor", "urgent care", "ear, nose and throat" })
        {
            Assert.Contains(destination, route, StringComparison.OrdinalIgnoreCase);
        }

        // NOT AN AMBULANCE CALL. Asserted as an absence INSIDE the rule, because
        // the shared block above it legitimately says 911 for other things — so a
        // whole-page ban would be false and a whole-page allowance would be blind.
        var ambulance = new Regex(
            @"\b911\b|ambulance|emergency room|emergency department", RegexOptions.IgnoreCase);
        Assert.Matches(ambulance, "Call 911 or go to the emergency department.");
        Assert.False(ambulance.IsMatch(route),
            "the sudden-hearing-loss rule has grown an ambulance route. No source gives one: "
            + "HLAA names primary care, urgent care or an ENT and never says emergency room, and "
            + "over-escalation here is a defect, not a safe default: " + route);

        // And the shared block still DOES carry 911, so the absence above is a
        // scoped decision rather than a page that forgot to escalate anything.
        //
        // EMPHASIS STRIPPED FIRST. The block reads `Call **911**`, so the bold
        // markers sit INSIDE the phrase and a flattened-only match cannot see it
        // — §12.8's WI-526 defect, a bolded word defeating a phrase guard
        // structurally, which this file's own `Plain` helper exists to avoid and
        // which the first version of this line walked straight into.
        Assert.Matches(new Regex(@"call\s*911", RegexOptions.IgnoreCase),
            Regex.Replace(CuratedPage.Flatten(CuratedPage.EscalationBlock), @"[*_]", ""));
    }

    [Fact]
    public void TheAlreadyDiagnosedReaderIsRoutedRatherThanGivenAnInventedRule()
    {
        // THE HONEST GAP, AND IT IS RECORDED RATHER THAN PAPERED OVER. Every
        // source for the rule above is written for NEW, UNEXPLAINED sudden
        // hearing loss. None says what a reader who already has this diagnosis
        // should do if their hearing drops — and that reader is this page's.
        //
        // Rather than extend the rule past its evidence, the page routes to the
        // corpus's own instruction for exactly that reader:
        // /treatments/watch-and-wait, "Something new, or something getting worse,
        // is a reason to get in touch. You do not have to wait for your next scan."
        var section = PlainOf(SymptomHeading);

        Assert.Matches(new Regex(
            @"If you already know you have one of these", RegexOptions.IgnoreCase), section);
        Assert.Contains("/treatments/watch-and-wait#step-by-step",
            Section(SymptomHeading), StringComparison.Ordinal);

        // READ THE SIBLING rather than trusting a belief about it (§12.10,
        // WI-514). If that instruction ever goes, this page's routing becomes a
        // pointer to nothing and the gap reopens silently.
        var watching = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "watch-and-wait.md")));
        Assert.Contains("You do not have to wait for your next scan", watching,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageRefusesToInventABrainstemOrHydrocephalusWarningList()
    {
        // THE RULING THAT MOST DISTINGUISHES THIS HUB, and the one a later editor
        // is most likely to "fix" by adding a helpful-looking checklist.
        //
        // EANO 2020 was read live and asked directly: it contains NO
        // patient-facing urgency guidance of any kind; every recommendation is
        // addressed to clinicians choosing treatment. Its "surgery the only
        // option" for large tumors, and StatPearls' "surgical resection is
        // indicated in ... brain stem compression", are INDICATIONS FOR SURGEONS.
        // Meanwhile the Vestibular Disorders Association says outright "Except
        // for very large tumors, it is not an emergency to treat most acoustic
        // neuromas", and the US patient organization for this tumor gives no
        // urgency guidance at all.
        //
        // So the refusal is stated on the page, and pinned here.
        var section = PlainOf(SymptomHeading);

        // THE REFUSAL IS SCOPED TO WHAT THIS PAGE *ADDS*, and rendered read 1 is
        // why. The first version said "What is NOT on this page is a list of
        // warning signs for a large growth" — while [MECHANISM], composed about
        // ninety lines above, had already given a raised-pressure pattern AND an
        // action. The sentence was true of this page's own prose and FALSE of
        // what the reader had just read. §12.10's mis-scoped-block shape from the
        // other direction: a block whose PRESENCE falsifies a sentence written as
        // though it were absent, and invisible to every source-reading guard.
        Assert.Matches(new Regex(
            @"What is NOT on this page is a checklist for a growth that has got large",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"general list above covers pressure signs", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"written for\s*surgeons", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"does not invent a rule its own sources do not give", RegexOptions.IgnoreCase),
            section);

        // AND THE ABSENCE ITSELF. A raised-pressure checklist would look like
        // this, and the page must not grow one.
        //
        // NO BLOCK SUBTRACTION IS NEEDED, and the first version's loop that did it
        // was DEAD CODE with a comment claiming otherwise (/review round 1, nit 9).
        // `Section()` reads the RAW page, where the block is still the literal
        // `[ESCALATION]` directive — so the loop removed nothing and the comment
        // asserting it was load-bearing would have misled the next editor. That is
        // the stale-rationale defect this item has now met four times. The scope is
        // already correct without it: this section's raw text IS the page's own
        // prose.
        var own = section;

        var invented = new Regex(
            @"worse in the morning|getting sleepier|much sleepier|throwing up again and again|"
            + @"double vision and a headache|drowsy",
            RegexOptions.IgnoreCase);
        Assert.Matches(invented, "Call if headaches are worse in the morning and you are drowsy.");
        Assert.False(invented.IsMatch(own),
            "this page has grown a raised-pressure warning list out of surgical findings, which "
            + "no patient-facing source for this tumor supports: " + own);

        // HEMORRHAGE IS OFF THE PAGE ENTIRELY. Under 1% of cases, presenting as
        // collapse, with no patient guidance anywhere — carrying it buys fear and
        // no action (§12.12).
        foreach (var word in new[] { "hemorrhage", "haemorrh", "bleeds inside", "bleeding into" })
        {
            Assert.DoesNotContain(word, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ------------------------------------------------------------- the grade

    [Fact]
    public void TheGradeIsAttributedInTwoHalvesAndNeverToTheNamingAuthority()
    {
        // §12.14. WHO CNS5 was read live, twice, as an ABSENCE: it lists
        // "Schwannoma" under cranial and paraspinal nerve tumors and PRINTS NO
        // GRADE BESIDE IT, in contrast to meningioma's 1, 2 and 3. What it does
        // support is the CONVENTION: "WHO CNS5 has changed all CNS WHO tumor
        // grades to Arabic numerals."
        //
        // StatPearls supplies the grade itself — "The World Health Organization
        // classifies schwannoma as a grade I benign tumor" — in a ROMAN numeral
        // and naming no edition. Those two facts are kept apart on the page.
        var grade = PlainOf(GradeHeading);

        Assert.Matches(new Regex(
            @"Clinical\s*references describe this growth as a grade 1", RegexOptions.IgnoreCase),
            grade);
        Assert.Matches(new Regex(@"The convention changed in 2021", RegexOptions.IgnoreCase), grade);

        // THE ABSENCE, SAID OUT LOUD. The page tells the reader the naming
        // document does not grade this tumor, which is what stops a later editor
        // "improving" the sentence by citing it.
        Assert.Matches(new Regex(
            @"lists this growth without\s*printing a grade beside it", RegexOptions.IgnoreCase),
            grade);

        // And the page never claims the naming authority graded it.
        var attributed = new Regex(
            @"(?:WHO|World Health Organization|CNS5|2021 document)[^.]{0,60}(?:classif|assign|grade[sd])"
            + @"[^.]{0,40}\bgrade\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(attributed, "The World Health Organization classifies it as a grade 1 tumor.");
        Assert.False(attributed.IsMatch(Plain),
            "the page attributes the grade to the naming authority, which prints no grade for this "
            + "tumor at all — a citation that does not support the claim in the form it is made");
    }

    [Fact]
    public void EveryRomanGradeSentenceCarriesItsOwnMarkerInTheSameSentence()
    {
        // A CORPUS-WIDE HOUSE RULE, enforced from another page's test file
        // (PathologyReportPageContentTests), which is where a failure would
        // surface and where nobody would look for it. WI-540 failed this on BOTH
        // occurrences of its draft. Pinned page-locally so the failure lands here.
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
        // that stopped mentioning the old notation would pass the loop above
        // while silently dropping the crosswalk slice §12.9 requires of every hub.
        var taught = CuratedPage.SentencesOf(Plain).Count(s => roman.IsMatch(s));
        Assert.True(taught >= 2,
            $"this page teaches the old grade notation in {taught} sentences; a reader holding "
            + "older paperwork is owed it in both the grade section and the report section");

        // The canary, run against planted text, so a pass means something.
        Assert.Matches(roman, "This one is a grade I tumor.");
        Assert.DoesNotMatch(marker, "This one is a grade I tumor.");
    }

    // ------------------------------------------------- naming and the rename

    [Fact]
    public void TheRenameCarriesATwoStepAttributionAndNeitherNameIsDeclaredDead()
    {
        // §12.1's highest-stakes claim class for this page, and the attribution
        // has to be TWO-STEP because the primary text is unreachable.
        //
        // The 2022 consensus abstract (read via repository mirrors) says: "the
        // term 'neurofibromatosis 2' has been retired to improve diagnostic
        // specificity." It does NOT contain the phrase "NF2-related
        // schwannomatosis". That comes from GeneReviews REPORTING the paper:
        // "The term 'NF2-related schwannomatosis' was proposed by Plotkin et al
        // [2022]". Asserting the paper's own wording for the new term would be a
        // citation that does not support the claim in the form it is made.
        var section = PlainOf(ReportHeading);

        Assert.Matches(new Regex(@"Older\s*letters use neurofibromatosis type 2", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"international group retired that\s*older name in 2022", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"A reference for doctors\s*records that the newer name was proposed", RegexOptions.IgnoreCase),
            section);

        // NEITHER NAME IS DECLARED DEAD. NORD, verified live: "Many physicians
        // prefer the use of the term vestibular schwannoma. However, the term
        // acoustic neuroma is still used more often in the medical literature."
        // A page that retires the name in its own URL would be lying to a reader
        // holding a letter that uses it.
        Assert.Matches(new Regex(@"Many doctors prefer the newer name", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"still used\s*more often in writing", RegexOptions.IgnoreCase),
            section);

        var declaresDead = new Regex(
            @"no longer called|is not called .{0,20}acoustic neuroma|the correct name is|"
            + @"should no longer be used",
            RegexOptions.IgnoreCase);
        Assert.Matches(declaresDead, "It is no longer called an acoustic neuroma.");
        Assert.False(declaresDead.IsMatch(Plain),
            "the page declares one of the two names dead, while the sources say both are in use");
    }

    [Fact]
    public void TheMisnomerIsExplainedInBothOfItsHalves()
    {
        // PMC7669921, verified live: "the majority of tumours arise from the
        // vestibular aspect of the vestibulocochlear nerve and the tumour cells
        // are Schwann cells rather than neuronal." TWO separate errors in the old
        // name, and a page that gives one of them has explained half of it.
        var section = PlainOf(WhereHeading);

        Assert.Matches(new Regex(@"usually starts on the balance branch", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"made of the wrapping\s*cells, not of nerve cells",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"names the wrong branch and\s*the wrong cell", RegexOptions.IgnoreCase), section);
    }

    // ------------------------------------------- the inherited condition (NF2)

    [Fact]
    public void TheInheritedConditionIsScopedBySidednessAndAgeAndIsNeverInverted()
    {
        // THE PASSAGE MOST LIKELY TO TERRIFY THE READER THIS PAGE IS MOSTLY FOR.
        //
        // GeneReviews, verified live, and the sentence the research pass never
        // surfaced: "Individuals younger than age 30 years with a symptomatic
        // unilateral vestibular schwannoma are at high risk of developing a
        // contralateral tumor and NF2 and should be monitored closely, while
        // individuals older than age 30 years who have a unilateral vestibular
        // schwannoma are at very low risk of developing NF2."
        //
        // One sourced sentence that protects the majority AND gives the genuinely
        // at-risk reader something real. Scoped by sidedness AND age, not by
        // sidedness alone.
        var section = PlainOf(ReportHeading);

        Assert.Matches(new Regex(
            @"whether there is a\s*growth on both sides, and your age", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"under thirty", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"over thirty", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"very low chance of having\s*the condition", RegexOptions.IgnoreCase), section);

        // THE MAJORITY CASE LEADS. PMC7669921: "The majority of tumors are
        // unilateral and sporadic." The share itself (under 5%) is a percentage
        // and stays off the page (§12.4 R1).
        Assert.Matches(new Regex(
            @"large majority of these growths are\s*on one side only and are not inherited",
            RegexOptions.IgnoreCase), section);

        // INHERITANCE IN BOTH HALVES. GeneReviews: "Approximately 50% ... have an
        // affected parent. Approximately 50% ... have the disorder as the result
        // of a de novo NF2 pathogenic variant." Carrying only "it is inherited"
        // is a defect: half the people who have it are the first in their family.
        Assert.Matches(new Regex(@"inherited it from a parent", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"first in their family to have it", RegexOptions.IgnoreCase), section);

        // AND THE INVERSION IS BANNED. "Bilateral tumors are the hallmark of NF2"
        // means most people with NF2 have bilateral tumors. It does NOT mean most
        // people with one of these has NF2. The inverted form is the single most
        // frightening sentence this page could accidentally write.
        var inverted = new Regex(
            @"(?:acoustic neuroma|vestibular schwannoma|this growth)[^.]{0,50}"
            + @"(?:is (?:a )?sign|means you (?:may )?have|suggests)[^.]{0,40}"
            + @"(?:genetic|inherited|NF2)",
            RegexOptions.IgnoreCase);
        Assert.Matches(inverted, "An acoustic neuroma is a sign of an inherited condition.");
        Assert.False(inverted.IsMatch(Plain),
            "the page implies that having one of these growths points to the inherited condition, "
            + "which inverts the hallmark claim and frightens the one-sided majority");
    }

    // -------------------------------------------------- numbers and schedules

    [Fact]
    public void ThePageCarriesNoShareNoDoseNoSizeThresholdAndNoInterval()
    {
        // §12.4, §12.5 and WI-521. The sources are thick with figures — hearing
        // preservation bands, the 12-26% sudden-loss range, incidence, growth in
        // mm per year, Gy margins, Koos size bands, facial palsy ranges, NF2
        // prevalence. Not one reaches this page.
        var share = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b" + CountWord + @"\s+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            + @"\b" + Fraction + @"\s+of\s+(?:all\s+)?(?:the\s+)?(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|tumors|cases|adults)\b|"
            + @"\b(?:survival|survive|live for|life expectancy|lifespan)\b|"
            + @"\b\d+(?:\.\d+)?\s*(?:Gy|gray|mg|milligrams?|cm|centimet\w*|mm|millimet\w*|mcg)\b|"
            + @"\bmilligram\w*|\bgrays?\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Sudden hearing loss happens in 12 to 26% of people.",
                     "Hearing is preserved in about half of them.",
                     "These grow about 1.4 mm per year.",
                     "The margin dose is 12 Gy.",
                     "Three out of four patients keep useful hearing.",
                 })
        {
            Assert.Matches(share, known);
        }
        // THE INHERITANCE SPLIT IS EXEMPT, DELIBERATELY AND WITH ITS REASON.
        // GeneReviews, verified live: "Approximately 50% of individuals diagnosed
        // with NF2 have an affected parent. Approximately 50% ... have the
        // disorder as the result of a de novo NF2 pathogenic variant." Carrying
        // only "it is inherited" is a DEFECT — it hides that a de novo case is
        // just as likely, which is the half that matters to a reader's relatives.
        //
        // "About half" is the QUALITATIVE form §12.4 R2 asks for rather than a
        // published percentage, and the fraction branch above exists to stop
        // OUTCOME and FREQUENCY shares ("half of all patients survive"), which is
        // a different claim class. Exempted by its own sentence rather than by
        // loosening the branch, so the branch keeps its teeth.
        // NOTE THE DENOMINATOR IN THE EXEMPTION. /review round 1 corrected the
        // page here: the first version said "Where the condition does run in a
        // family, about half ... inherited it from a parent, and about half are
        // the FIRST in their family" — two halves that exclude each other, because
        // GeneReviews' 50/50 is over EVERYONE diagnosed, not over familial cases.
        // The exemption tracks the corrected sentence; the reviewer predicted this
        // regex would still match, and it would NOT have, so the figures ban would
        // have fired on the very sentence the exemption exists for.
        var scanned = Regex.Replace(Plain,
            @"About half of the people who have that condition[^.]*\.[^.]*\.", " ",
            RegexOptions.IgnoreCase);
        Assert.DoesNotMatch(share, scanned);

        // And the branch must still catch what it is for, after the exemption.
        Assert.Matches(share, "Half of the people treated keep useful hearing for five years.");
        Assert.Matches(share, "About half of all patients need treatment eventually.");

        // NO SIZE THRESHOLD AS ADVICE. EANO bands treatment by Koos grade and by
        // centimetres, and §12.8 (WI-527) calls a magnitude beside a quantity a
        // threshold shape whether or not it carries a unit. The page says size is
        // one of the things that decides, and hands the reader the question.
        var threshold = new Regex(
            @"\b(?:under|over|larger than|smaller than|less than|more than)\s+" + CountWord
            + @"\s*(?:cm|centimet\w*|mm|millimet\w*)\b|\bKoos\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(threshold, "Radiosurgery suits tumors under 3 cm.");
        Assert.DoesNotMatch(threshold, Plain);

        // NO PER-TUMOR SCHEDULE (WI-521), which /treatments/watch-and-wait owns
        // as a ruling and which this page inherits: a reachable EANO interval
        // exists and is deliberately not printed, because a per-tumor timetable
        // is a number a frightened reader CAN act on (WI-520).
        var interval = new Regex(
            @"\b(?:every|each)\s+" + CountWord + @"?\s*(?:months?|years?|weeks?)\b|"
            + @"\bannual(?:ly)?\b|\b" + CountWord + @"[-\s]*(?:to|or)[-\s]*" + CountWord
            + @"\s*(?:months?|years?)\b|\b(?:six|three|twelve)[- ]month\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "A scan every six months, then annually.",
                     "Scans continue every year for five years.",
                 })
        {
            Assert.Matches(interval, known);
        }
        Assert.DoesNotMatch(interval, Plain);
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
        var flat = CuratedPage.Flatten(inside);

        Assert.DoesNotMatch(new Regex(@"\d"), inside);

        // AND NO FIGURE-SHAPED CLAIM IN WORDS, which is the half a digit ban
        // cannot see (§12.8, WI-527), including the DIRECTIONAL form: saying the
        // numbers are kinder than expected publishes their direction.
        var told = new Regex(
            @"\blive as long as\b|\blife expectancy\b|\blifespan\b|\bsurviv\w*|\bdie of\b|"
            + @"\bcure[ds]?\b|\bmost people\b|\bthe vast majority\b|"
            + @"\bkinder than\b|\bbetter than (?:you|people|most)\b|\bnot as bad as\b|"
            + @"\bmore hopeful than\b|\bless frightening than\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(told, "People with this live as long as anybody else.");
        Assert.Matches(told, "The numbers are kinder than the word tumor suggests.");
        Assert.DoesNotMatch(told, flat);

        // THIS GATE'S OWN ANGLE, and the reason it is not a copy of any sibling's:
        // for this tumor the published numbers are about HEARING rather than
        // survival, and the guideline that carries them rates its own confidence
        // at the lowest level it has. That is the honest thing to teach here.
        Assert.Matches(new Regex(@"not about how long anybody lives", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(@"rate their own confidence", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(@"cannot tell you what will happen to your ear",
            RegexOptions.IgnoreCase), flat);

        // It closes on a door rather than on a fact (§12.6).
        Assert.Matches(new Regex(@"nothing stops you coming back to it", RegexOptions.IgnoreCase),
            flat);

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
        // THE BLOCKER THAT WOULD BE AN ABSENCE. An excluded block leaves no trace
        // on the page — no heading, no directive, nothing to grep — so a later
        // item can restore one and every other guard in this file stays green.
        //
        // An EXACT SET, not a handful of DoesNotContain checks: a new block added
        // in two years fails here and has to be ruled on deliberately.
        Assert.Equal(
            ["caregiver", "causes", "escalation", "mechanism"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        Assert.Contains("[MECHANISM]", Section(WhereHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);

        // THE CROSSWALK EXCLUSION, and its reason is a THIRD distinct one. That
        // block is wholly the 2021 CNS rewrite: Roman to Arabic, gene results
        // entering the NAME, and NOS and NEC. This page's rename came from the
        // 2022 schwannomatosis consensus instead — a different document, year and
        // authority. Including it would put glioma vocabulary in this reader's
        // identity section (WI-514).
        foreach (var acronym in new[] { "NOS", "NEC", "IDH", "1p/19q" })
        {
            Assert.DoesNotContain(acronym, Plain, StringComparison.Ordinal);
        }

        // Two case policies, two loops (§12.8, WI-539): a sentence-initial
        // capital walks straight through a case-sensitive ban, and
        // sentence-initial is exactly where the capital lives.
        foreach (var word in new[] { "molecular", "gene panel", "methylation", "tumor board" })
        {
            Assert.DoesNotContain(word, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // THE POSTERIOR-FOSSA AND SPINAL-CORD EXCLUSIONS. The first is about
        // CHILDREN after surgery low at the back of the brain; the second is
        // scoped to a tumor IN the cord. This tumor is adult and sits at the
        // angle between brainstem and cerebellum, so both would assert something
        // false of this reader.
        foreach (var phrase in new[] { "stop talking", "cerebellar mutism", "saddle area", "bowel" })
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheMechanismBlockIsScopedBecauseThisGrowthSitsBesideTheBrainNotInIt()
    {
        // §12.10's mis-scoped block, and the scoping problem here is the INVERSE
        // of /tumors/craniopharyngioma's. THAT page says the block's brain map is
        // "about growths elsewhere" — which would be FALSE here, because two of
        // the map's six entries (the cerebellum, and the brainstem) describe this
        // tumor exactly. The problem here is the block's OPENING: it says "a
        // tumor in the brain", and this one grows beside it.
        //
        // So this page needs its own note, and copying the neighbour's would ship
        // a false sentence. That is why the note is pinned by its own wording.
        var section = PlainOf(WhereHeading);

        Assert.Matches(new Regex(@"Before you read the next part", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"describes growths sitting\s*inside the brain itself", RegexOptions.IgnoreCase),
            section);

        // THE NOTE MUST SUBTRACT SPECIFICALLY, NOT DISCOUNT VAGUELY — /review
        // round 1's second blocker, and the fourth defect of the family that only
        // exists once the page is composed.
        //
        // The first version said "several of the pressure effects listed there are
        // not how this behaves": unnamed and unbounded. The refusal paragraph ~110
        // rendered lines later says the general list "applies to you as much as to
        // anyone". Both cannot be operative — and the vague discount landed on the
        // WRONG material, because the two paragraphs a LARGE growth here really
        // does produce are raised pressure and blocked fluid flow (a
        // cerebellopontine-angle mass obstructing the fourth ventricle). A reader
        // with morning headaches and vomiting had been handed a reason to file
        // them under "not how this behaves", which is what made the refusal unsafe
        // as composed rather than unsafe in principle.
        //
        // So the note now names what does NOT apply, and says plainly that the two
        // that DO apply arrive only with size.
        foreach (var excluded in new[] { "does not set off seizures",
                                         "does not make the brain around it\\s*swell" })
        {
            Assert.Matches(new Regex(excluded, RegexOptions.IgnoreCase), section);
        }

        Assert.Matches(new Regex(
            @"only if a growth gets large: pressure building\s*up inside the head",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"fluid being blocked on its way out", RegexOptions.IgnoreCase), section);

        // AND THE MAP'S THREE ENTRIES ARE SCOPED RATHER THAN ENDORSED WHOLESALE.
        // /review ruled against my defence here: telling the reader "the brainstem
        // entry applies" hands them "facial weakness", which this page calls
        // unusual. The distinction I was relying on — a tumor IN the brainstem
        // versus one pressing on it, and only when large — was correct medicine
        // and entirely invisible to the reader.
        //
        // WI-568 MADE IT THREE ENTRIES, NOT TWO, AND CHANGED WHICH ONE IS CLOSEST.
        // The block's map gained a SKULL BASE entry — hearing, balance, the nerves
        // to the face — and a vestibular schwannoma is a skull-base growth, so that
        // entry describes it more exactly than the cerebellum one this note used to
        // put first. Leaving the note alone would have sent the reader to the
        // second-best entry while the best one went unmentioned. The note points at
        // the entries rather than restating their words, because the corpus
        // restatement guard fires on a page that echoes a block's prose — which is
        // how the first draft of this edit was caught.
        Assert.Matches(new Regex(
            @"entry about the skull base, below and behind the ear, is the closest fit",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"cerebellum entry is this growth's territory too", RegexOptions.IgnoreCase),
            section);

        // AND THE NEW ENTRY IS SCOPED, NOT ENDORSED. /review round 2: this page says
        // above that facial weakness is unusual here and is "what people fear", and
        // the skull base entry lists weakness down one side of the face. Promoting
        // that entry to "closest fit" without scoping it reintroduced, one bullet
        // over, the exact defect the brainstem sentence was scoped for.
        Assert.Matches(new Regex(
            @"facial weakness, which this growth usually causes only once it is large", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"brainstem entry describes what can happen only once a growth\s*is big enough",
            RegexOptions.IgnoreCase), section);

        // The vague form must never come back.
        var vague = new Regex(@"several of the pressure effects", RegexOptions.IgnoreCase);
        Assert.Matches(vague, "several of the pressure effects listed there are not how this behaves");
        Assert.DoesNotMatch(vague, section);

        // AND THE SEIZURE DENIAL MUST NOT OUTRANK THE AMBULANCE TIER — /review
        // round 2's finding 1, the FIFTH defect of the composed-page family and
        // round 1's blocker in miniature.
        //
        // The note denies seizures; the pressure paragraph it then ENDORSES lists
        // seizures among its five signs; and the shared block files "A first ever
        // seizure" as an ambulance call. A reader could strike one item off the
        // pattern, or — the dangerous direction — read a flat denial as a reason a
        // first seizure is not urgent. It survived fourteen edits because NOTHING
        // ASSERTED that the denial leaves the ambulance rule intact. This is that
        // assertion.
        Assert.Matches(new Regex(
            @"does not set off\s*seizures the way a growth inside the brain does",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"the ambulance rule below\s*applies to you as much as to anyone",
            RegexOptions.IgnoreCase), section);

        // The unbounded denial is what must never return.
        var flatDenial = new Regex(@"does not set off seizures[,.]", RegexOptions.IgnoreCase);
        Assert.Matches(flatDenial, "it does not set off seizures, and it does not swell.");
        Assert.DoesNotMatch(flatDenial, section);

        // And the block still carries the ambulance rule the note now preserves,
        // read out of the block rather than trusted (§12.10, WI-514).
        Assert.Contains("A **first ever seizure**",
            CuratedPage.Flatten(CuratedPage.EscalationBlock), StringComparison.Ordinal);

        // BEFORE the directive, not after it. A note a reader meets on the way
        // out has scoped nothing (§12.8, WI-512: position was the property).
        var raw = CuratedPage.Flatten(Section(WhereHeading));
        var note = raw.IndexOf("Before you read the next part", StringComparison.Ordinal);
        var directive = raw.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(note >= 0 && directive > note,
            "the scoping note does not sit above [MECHANISM], so the reader meets a map of the "
            + "brain's own regions before anything reconciles it with a growth outside the brain");

        // AND THE BLOCK STILL SAYS WHAT THE NOTE IS ABOUT. If a later item
        // rewrites the block, this note becomes a puzzle rather than silently
        // wrong — the shape AllBrainTumorsPageTests uses.
        var mechanism = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md")));
        Assert.Contains("a tumor in the brain", mechanism, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cerebellum, low at the back", mechanism, StringComparison.Ordinal);
        Assert.Contains("Brainstem", mechanism, StringComparison.Ordinal);
        Assert.Contains("It can set off seizures", mechanism, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSharedBlockIsIncludedUneditedAndThePageAddsItsOwnRuleBeneathIt()
    {
        // §12.10's remedy applied literally: include the block, then add what is
        // true HERE. Editing Content/blocks/escalation.md to carry a hearing rule
        // would put it on every glioma hub (WI-514's blast radius).
        var raw = Section(SymptomHeading);
        Assert.Contains("[ESCALATION]", raw, StringComparison.Ordinal);

        var directive = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        var own = raw.IndexOf("This growth has one rule the list above does not carry",
            StringComparison.Ordinal);
        Assert.True(own > directive,
            "the page's own rule sits above the shared block, so a reader meets the exception "
            + "before the rule");

        // THE FORWARD POINTER, ABOVE THE BLOCK. The corpus convention is
        // unanimous across every hub that adds a scoped rule — pointer above,
        // rule below — and WI-539 shipped the second half without the first.
        // Worded deliberately unlike the siblings that carry one.
        var pointerAt = raw.IndexOf("One sign here has a rule of its own",
            StringComparison.Ordinal);
        Assert.True(pointerAt >= 0 && directive > pointerAt,
            "the forward pointer is missing or sits below [ESCALATION], so a reader meets this "
            + "growth's own rule with no warning that it was coming");

        // THE BLOCK ITSELF IS UNTOUCHED.
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        foreach (var absent in new[]
                 {
                     "acoustic neuroma", "vestibular", "hearing loss", "tinnitus",
                     "ear, nose and throat", "audiologist",
                 })
        {
            Assert.DoesNotContain(absent, block, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);
    }

    // ------------------------------------ what this page owns and what it routes

    [Fact]
    public void WatchingIsRoutedRatherThanRestatedAndTheDoorsAreDeepLinks()
    {
        // §12.10. /treatments/watch-and-wait is the dominant coupling: it names
        // this tumor in its who-is-watched list, carries the hearing-test detail,
        // has its own paragraph on what growth means here, carries the worry
        // research, and LINKS BACK to this page. So this hub routes and writes
        // none of that material itself.
        //
        // The routes are DEEP LINKS, which also makes AssertFragmentLinksResolve
        // non-vacuous on this page (§12.9: its regex stops at the `#`, so a link
        // to an anchor that does not exist resolves as a healthy 200).
        foreach (var anchor in new[] { "#if-it-grows", "#worry", "#step-by-step" })
        {
            Assert.Contains("/treatments/watch-and-wait" + anchor, Body, StringComparison.Ordinal);
        }

        // READ THE SIBLING rather than trusting a belief about it (§12.10,
        // WI-514). If it ever stops naming this tumor, the routing ruling has to
        // be re-argued rather than left quietly in place.
        var watching = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "watch-and-wait.md")));
        Assert.Contains("Some acoustic neuromas", watching, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hearing tests are part of watching an acoustic neuroma", watching,
            StringComparison.Ordinal);

        // AND THIS PAGE DOES NOT RESTATE WHAT THAT PAGE OWNS. The worry research
        // in particular: it is that page's, argued from studies this page never
        // cites.
        foreach (var owned in new[] { "low mood", "depression", "anxiety", "German study" })
        {
            Assert.DoesNotContain(owned, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheFocusedRadiationTermIsSaidInProseAndItsLinkHangsOnOtherWords()
    {
        // §12.8 (WI-519): LINKING A WORD TURNS ITS TOOLTIP OFF. The glossary entry
        // for this term is suppressed on /treatments/stereotactic-radiosurgery
        // itself, and stays reachable only on pages that say the words in prose
        // WITHOUT linking them. That page's own test asserts the exact set of
        // pages keeping it reachable, and this page was registered there
        // deliberately when it became the fifth.
        //
        // So this page must say the term unlinked. If a later edit "tidies" it by
        // hanging the link on the term, the tooltip dies here and that page's
        // exact-set assertion fails somewhere nobody is looking.
        var treated = CuratedPage.Flatten(Reader(Section(TreatedHeading)));

        Assert.Matches(new Regex(
            @"Doctors\s*call this stereotactic radiosurgery", RegexOptions.IgnoreCase), treated);

        // The term must NOT sit inside a markdown link anywhere on the page.
        var linked = new Regex(@"\[[^\]]*stereotactic radiosurgery[^\]]*\]\([^)]*\)",
            RegexOptions.IgnoreCase);
        Assert.Matches(linked, "[stereotactic radiosurgery](/treatments/stereotactic-radiosurgery)");
        Assert.False(linked.IsMatch(Body),
            "the term sits inside a markdown link, which turns its tooltip off on this page and "
            + "breaks the reachability set asserted by StereotacticRadiosurgeryPageTests");

        // And the door is still there, hung on other words.
        Assert.Contains("/treatments/stereotactic-radiosurgery", Body, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTreatmentSectionNamesNoWinnerWhereTheSourcesDisagree()
    {
        // A GENUINE SOURCE DISAGREEMENT, and §12.4 R2 says the page states the
        // trade-off rather than picking a side. EANO 2020: "SRS is superior to
        // microsurgery for patients with VS <3 cm in terms of preserving facial
        // nerve and hearing function." PMC12765508: "Vertigo and tinnitus
        // outcomes generally favored microsurgery, while facial nerve outcomes
        // were comparable across treatments."
        var section = PlainOf(TreatedHeading);

        Assert.Matches(new Regex(@"No one option is better at\s*everything", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"The reports disagree", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"nobody has ever run the kind of study that would settle it", RegexOptions.IgnoreCase),
            section);

        // AND NO WINNER IS NAMED FOR THE FACIAL NERVE, in either direction.
        var winner = new Regex(
            @"(?:radiation|radiosurgery|surgery|an operation)[^.]{0,40}"
            + @"(?:is better|is safer|is superior|does better)[^.]{0,40}(?:facial|the face)|"
            + @"(?:facial nerve|the nerve to the face)[^.]{0,40}(?:is better|does better|is safer)",
            RegexOptions.IgnoreCase);
        Assert.Matches(winner, "Radiosurgery is better for the facial nerve.");
        Assert.False(winner.IsMatch(Plain),
            "the page names a winner for the facial nerve, where two reachable sources disagree");

        // The three options come from a FEDERAL PATIENT SOURCE, named as three.
        Assert.Matches(new Regex(
            @"three choices, and doing nothing for now is one of them", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheHearingSentenceCarriesBothHalvesOrNeither()
    {
        // THE MOST QUOTABLE AND MOST DANGEROUS SENTENCE IN THE SOURCES. The
        // CNS/AANS guideline, verified live and quoted to the end of the
        // qualification: "Across all studies, microsurgery and radiosurgery
        // appear to accelerate this decline over the natural history, ALTHOUGH
        // FURTHER RESEARCH IS NEEDED GIVEN LIMITATIONS OF AVAILABLE EVIDENCE."
        //
        // The first half alone reads as "treatment costs you your hearing", which
        // is not what it says and would push a reader away from treatment. Every
        // recommendation in that guideline is Level 3, its lowest tier.
        var section = CuratedPage.Flatten(Regex.Replace(
            CuratedPage.ComposedSubsection(Page, "What happens to hearing"), @"[*_]", ""));

        Assert.Matches(new Regex(
            @"appear to speed up\s*a decline that was already happening", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"more research\s*is needed, because the evidence is weak", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"lowest level of confidence", RegexOptions.IgnoreCase), section);

        // AND THE COUNTERWEIGHT TO "WATCHING KEEPS YOUR HEARING", which is the
        // belief this section exists to correct: hearing can get worse in a year
        // when the growth has not grown at all.
        Assert.Matches(new Regex(
            @"worse even in a year when the\s*growth has not grown", RegexOptions.IgnoreCase),
            section);

        // THE PAGE MUST NOT PROMISE THAT TREATMENT PROTECTS HEARING — AND THE BAN
        // HAS TO BE NEGATION-AWARE, OR IT FORBIDS THE SENTENCE THAT DOES THE
        // HONEST WORK. The first version was a flat phrase ban and it failed on
        // this page's own "it is not a promise that waiting keeps your hearing".
        // That is §12.9's recorded defect (WI-513: a ban list on retired names
        // must be negation-aware or it forbids the crosswalk it protects),
        // reproduced against the very clause the section exists to carry.
        //
        // So the property is asserted at SENTENCE level: the promise may appear
        // only where the same sentence denies it — the same shape this file uses
        // for the Roman-numeral marker rule.
        // THE NEGATION MUST ADJOIN THE PROMISE, not merely share a sentence with
        // it — /review round 2's nit 6. The first version accepted any `\bnot\b`
        // anywhere in the sentence, so "Treatment protects your hearing, though it
        // is not a cure" would have passed: a guard weaker than its own comment
        // claimed, which is the defect class this file has met four times.
        var promise = new Regex(
            @"(?:saves|protects|preserves|keeps) your hearing|"
            + @"hearing (?:is|will be) (?:saved|protected)",
            RegexOptions.IgnoreCase);
        var deniedPromise = new Regex(
            @"\b(?:not|never|no)\b[^.]{0,40}?"
            + @"(?:(?:saves|protects|preserves|keeps) your hearing|"
            + @"hearing (?:is|will be) (?:saved|protected))",
            RegexOptions.IgnoreCase);

        var asserted = CuratedPage.SentencesOf(Plain)
            .Where(s => promise.IsMatch(s) && !deniedPromise.IsMatch(s))
            .ToList();
        Assert.True(asserted.Count == 0,
            "the page promises that treatment protects hearing, which the evidence does not "
            + "support:\n  " + string.Join("\n  ", asserted));

        // Canaries in BOTH directions, including the sentence that defeated the
        // first version.
        Assert.Matches(promise, "Treating it early protects your hearing.");
        Assert.DoesNotMatch(deniedPromise, "Treating it early protects your hearing.");
        Assert.DoesNotMatch(deniedPromise,
            "Treatment protects your hearing, though it is not a cure.");
        Assert.Matches(deniedPromise, "It is not a promise that waiting keeps your hearing.");
    }

    // --------------------------------------------------------- onward doors

    [Fact]
    public void TheOnwardDoorsSitInTheSectionsWhoseReaderNeedsThem()
    {
        // WI-512's rule: presence was never the property, POSITION was. Each
        // route out is offered where the question arises, not collected in a
        // footer a reader reaches only after the part they came for.
        foreach (var (link, heading) in new[]
                 {
                     ("/treatments/watch-and-wait#step-by-step", SymptomHeading),
                     ("/tests/mri", FindOutHeading),
                     ("/tests/pathology-report", ReportHeading),
                     ("/treatments/watch-and-wait#if-it-grows", TreatedHeading),
                     ("/treatments/stereotactic-radiosurgery", TreatedHeading),
                     ("/treatments/craniotomy", TreatedHeading),
                     ("/tests/follow-up-scans", ScansHeading),
                     ("/tests/waiting-for-results", ScansHeading),
                     ("/treatments/watch-and-wait#worry", CareHeading),
                     ("/get-help-now", "Where to get support"),
                 })
        {
            Assert.Contains(link, Section(heading), StringComparison.Ordinal);
        }
    }

    // --------------------------------------------------------- housekeeping

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3, read from the spec rather than copied from the last hub —
        // §12.8's WI-510 lesson is that copying the previous page is how a
        // template quietly shrinks.
        Assert.Equal(
            [
                "The short version",                                               // 0
                "What is an acoustic neuroma?",                                    // 1
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

        foreach (var sub in new[] { "What happens to hearing", "Balance, the face, and the first weeks" })
        {
            Assert.Matches(new Regex($@"^### {Regex.Escape(sub)}\s*$", RegexOptions.Multiline),
                CuratedPage.Body(Page));
        }

        // §12.9's demotion: self-blame sits below "if it comes back" and above
        // the outlook gate.
        var headings = Headings();
        Assert.True(headings.IndexOf("Did I cause this?") > headings.IndexOf("If it comes back"));
        Assert.True(headings.IndexOf("What might happen over time")
            > headings.IndexOf("Did I cause this?"));
    }

    [Fact]
    public void TheCaregiverSectionAddsWhatIsOnlyTrueHere()
    {
        // §12.10: a caregiver section that paraphrases the block in different
        // words is the defect WI-525 wrote down and WI-526 repeated, and no
        // shingle check can see a paraphrase — so the section promises a COUNT
        // and this guard counts.
        var care = PlainOf(CareHeading);

        Assert.Matches(new Regex(@"Three things belong to this one", RegexOptions.IgnoreCase), care);
        var leads = Regex.Matches(CuratedPage.Flatten(Reader(Section(CareHeading))), @"\*\*(.+?)\*\*")
            .Select(m => m.Groups[1].Value)
            .ToList();
        Assert.True(leads.Count == 3,
            $"the caregiver section promises three things and carries {leads.Count}:\n  "
            + string.Join("\n  ", leads));

        // THE SUMMARY MUST NOT NARROW THE RULE — /review round 2 on WI-540, where
        // a caregiver summary silently re-gated an emergency rule eleven lines
        // after the page widened it, invisibly to every guard because the tier
        // guards read the symptoms section and this one read two phrases.
        //
        // Here the risk is the ROUTE: a summary that says "call an ambulance"
        // would contradict the rule it summarises.
        Assert.Matches(new Regex(@"Learn the one urgent rule with them", RegexOptions.IgnoreCase),
            care);
        Assert.Matches(new Regex(
            @"their own doctor, an urgent care\s*clinic or an ear specialist", RegexOptions.IgnoreCase),
            care);

        var escalated = new Regex(@"\b911\b|ambulance|emergency room|emergency department",
            RegexOptions.IgnoreCase);
        Assert.Matches(escalated, "Call an ambulance if their hearing drops.");
        Assert.False(escalated.IsMatch(care),
            "the caregiver summary escalates the one rule to an ambulance call, which contradicts "
            + "the rule it summarises and has no source behind it");

        // The one-sided-hearing insight, which is this page's own and is the
        // thing most often read as rudeness at home.
        Assert.Matches(new Regex(@"gets read as rudeness", RegexOptions.IgnoreCase), care);
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST, and that is the ruling. The guard is BIDIRECTIONAL: this
        // page's first draft turned /tumors/craniopharyngioma red with 55 shared
        // shingles. An allowlist here could not have fixed that, because that
        // page's test never reads this page's allowlist.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // THE SECOND RESTATEMENT GUARD, AND THE REASON THIS FILE CARRIES BOTH.
        //
        // WI-541 scored ZERO on AssertDoesNotRestateTheCorpus and still turned
        // /treatments/watch-and-wait and /tumors/meningioma RED, because the
        // page-local variant many test files carry normalises differently:
        //
        //     text = Regex.Replace(text, @"^#{1,6} .*$", " ", Multiline);
        //     text = Regex.Replace(text, @"\]\([^)]*\)", "] ");   // LINK TEXT SURVIVES
        //     words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+");
        //
        // The shared helper drops WHOLE links and the ask-list; this one keeps
        // both. The two failures came from exactly there — a door's link text and
        // an ask-list question. A clean report from one guard says nothing about
        // the other, so this page asserts itself against the one that caught it.
        var pageShingles = LinkTextShingles(CuratedPage.ReaderText(Page)).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("acoustic-neuroma.md", StringComparison.Ordinal))
            .Concat(Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"));

        foreach (var file in others)
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var overlaps = LinkTextShingles(CuratedPage.ReaderText(File.ReadAllText(file)))
                .Where(pageShingles.Contains)
                .Where(s => s.Split(' ').Count(w => !Stopwords.Contains(w)) >= 3)
                .Distinct()
                .ToList();

            Assert.True(overlaps.Count == 0,
                $"this page restates {slug} through text the shared guard cannot see "
                + "(link text, or the ask-list):\n  " + string.Join("\n  ", overlaps));
        }
    }

    private static readonly HashSet<string> Stopwords =
    [
        "the", "a", "an", "and", "or", "but", "is", "are", "was", "were", "be", "been",
        "it", "its", "that", "this", "those", "these", "of", "to", "in", "on", "at",
        "for", "with", "as", "by", "from", "you", "your", "they", "them", "their",
        "not", "no", "so", "if", "what", "which", "who", "how", "when", "there",
        "can", "cannot", "will", "would", "may", "might", "do", "does", "did", "have",
        "has", "had", "one", "than", "then", "out", "up", "about", "into", "over",
    ];

    /// <summary>
    /// Eight-word shingles under the PAGE-LOCAL rules: headings dropped, link
    /// TARGETS stripped but link TEXT kept, digits kept. Deliberately different
    /// from <c>CuratedPage.Shingles</c>; see the test above for why both exist.
    /// </summary>
    private static IEnumerable<string> LinkTextShingles(string text)
    {
        text = Regex.Replace(text, @"^#{1,6} .*$", " ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\]\([^)]*\)", "] ");
        var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+")
            .Select(m => m.Value).ToList();
        for (var i = 0; i + 8 <= words.Count; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(8));
        }
    }

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

        // "Benign" is handled rather than offered as comfort, and this page's
        // formulation is its own: three siblings already handle the word and a
        // fourth borrowed phrasing would turn one of them red.
        var grade = PlainOf(GradeHeading);
        Assert.Matches(new Regex(
            @"It says nothing about your hearing", RegexOptions.IgnoreCase), grade);

        var comfort = new Regex(
            @"\b(?:it is|its|it's) (?:only |just |merely )?benign\b|"
            + @"\bbenign,? so\b|\bgood news:? (?:it is |it's )?benign\b|"
            + @"\bthe good (?:part|thing|bit|news)\b|"
            + @"\bnothing to worry about\b|\bthe lucky (?:one|kind)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(comfort, "The good news is it is benign.");
        Assert.DoesNotMatch(comfort, Plain);
    }

    /// <summary>
    /// NO OTHER GATE IN THE CORPUS CAN SEE THIS CLASS OF DEFECT. ContentCheck
    /// measures reading grade; both restatement guards drop <c>[*_]</c> before
    /// comparing; <c>Plain</c> strips them too, deliberately (WI-526); and the
    /// render tests read flattened HTML. So a <c>**</c> that opens and never
    /// closes ships two literal asterisks to the reader while 2,247 tests stay
    /// green — which is exactly what this page did.
    /// </summary>
    [Fact]
    public void ThePageBalancesEveryEmphasisMarker()
    {
        // HOW IT ARRIVED, because the shape repeats: a /review round-2 fix
        // rewrote a sentence that used to end "...can grow again**, from a
        // piece deliberately left behind...", and the replacement swallowed the
        // closing marker. That new prose WAS pre-checked — for restatement
        // collisions, and later for British spelling. Neither check has
        // anything to say about well-formed markdown. A pre-check answers the
        // one question it implements, and a clean result from it is not general
        // clearance (§12.8).
        //
        // ASSERTS THE PROPERTY, NOT A PHRASING (§12.8: assert position,
        // adjacency or coverage, never the wording an author imagined).
        // Emphasis cannot span a blank line in markdown, so an odd count of a
        // marker inside one paragraph means it never closed — true of any
        // wording a later item writes here.
        var offenders = new List<string>();

        foreach (var paragraph in Regex.Split(CuratedPage.Body(Page), @"\r?\n\s*\r?\n"))
        {
            // An inline code span may legitimately carry a lone asterisk.
            var stripped = Regex.Replace(paragraph, "`[^`]*`", " ");

            foreach (var marker in new[] { "**", "%%" })
            {
                if (Regex.Matches(stripped, Regex.Escape(marker)).Count % 2 != 0)
                {
                    offenders.Add($"{marker} unclosed: {CuratedPage.Flatten(paragraph)}");
                }
            }
        }

        Assert.Empty(offenders);

        // THE GUARD MUST BE ABLE TO FAIL, twice over: that an odd count is
        // actually detected, and that it is not counting an empty string --
        // a balance check over nothing passes forever.
        Assert.Single(Regex.Matches("**this one never closes.", @"\*\*"));
        Assert.NotEqual(0, Regex.Matches(CuratedPage.Body(Page), @"\*\*").Count);
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // The corpus-wide gate reads BODY text only, and this page's first draft
        // failed it on "organisations" — a word that arrived because the sources
        // for this tumor include several written outside the US.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, and each checked against the corpus before being added
        // (§12.8: a phrase belongs on a ban list only if no correct sentence
        // contains it). These are the words this page will drift back toward,
        // because its sources use them.
        // "mobile phone" IS THE PREDICTABLE SLIP FOR THIS TUMOR, and rendered
        // read 2 found it: the page's own causes addition said "mobile phones"
        // three lines below the composed block's "Cell phones." Two registers for
        // one object, on a US page. NO GATE CAUGHT IT — CuratedPage.BritishForms
        // lists SPELLINGS, and this is an IDIOM, which is the same class as
        // WI-539's "tablets" and "optician". Acoustic neuroma is the tumor in the
        // mobile-phone literature, so this page will drift back toward it.
        // "straight away" and "come round" are named as British by §12.10 itself
        // and are pending a corpus-wide /pm sweep — but both instances on THIS
        // page were this item's own new prose, and one of them sat inside the one
        // safety rule. Banned page-locally rather than waiting for the sweep.
        //
        // NOTE THE SHAPE OF HOW THE SECOND ONE WAS FOUND. The first was fixed, and
        // a verification over the WHOLE flattened body then reported the idiom
        // still present — a second occurrence in the short version that no edit had
        // touched. §12.8's rule, earned again: a reported phrase is a property of
        // the whole page, and memory enumerates instances.
        foreach (var form in new[]
                 {
                     "mobile phone", "audiologist's", "ENT surgeon", "consultant", "GP",
                     "straight away", "come round",
                 })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // And the page must agree with the block composed above it, which says
        // "Cell phones." A page that uses the other register three lines later
        // reads as two voices (§12.10, one claim at one strength).
        Assert.Contains("cell phones", body, StringComparison.OrdinalIgnoreCase);
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

        // The sources this page's load-bearing claims rest on, by name. EVERY ONE
        // was verified against the LIVE publication rather than against the
        // item's research pack — see .claude/work_files/wi541/plan.md for what
        // that verification changed.
        foreach (var owed in new[]
                 {
                     "nidcd.nih.gov/health/vestibular-schwannoma",  // the three options
                     "nidcd.nih.gov/health/sudden-deafness",        // half the one rule
                     "hearingloss.org",                             // the other half, and the route
                     "guidelinecentral.com",                        // the source that disagrees on strength
                     "NBK562312",                                   // StatPearls: the grade, in Roman
                     "PMC8328013",                                  // CNS5: the convention, and NO grade
                     "NBK1201",                                     // GeneReviews: the rename and the age split
                     "PMC7669921",                                  // the misnomer, and unilateral/sporadic
                     "PMC6954440",                                  // EANO: treatment, and NO urgency guidance
                     "PMC12765508",                                 // the facial-nerve disagreement
                     "cns.org",                                     // hearing trajectory, all Level 3
                     "rarediseases.org",                            // both names still in use
                     "vestibular.org",                              // "not an emergency"
                 })
        {
            Assert.Contains(urls, u => u.Contains(owed, StringComparison.Ordinal));
        }

        // THE SOURCE THE STUB CITED, AND IT IS BARRED. §12.1: a page that cites
        // NCI for a tumor name is a defect, and naming is a large part of what
        // this page does. The fourth stub in a row to carry it.
        foreach (var barred in new[] { "cancer.gov", "cancerresearchuk.org", "mayoclinic.org" })
        {
            Assert.False(urls.Any(u => u.Contains(barred, StringComparison.OrdinalIgnoreCase)),
                $"{barred} is cited; the front matter records why it is not used");
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);

        // NO BRACKETED DIRECTIVE NAME ANYWHERE IN THE FRONT MATTER. This is the
        // WI-538 trap: CaregiverSectionTests locates the block with a raw IndexOf
        // over the WHOLE file, so a bracketed name in a comment becomes the first
        // match and the page is reported as not carrying the block.
        Assert.DoesNotMatch(new Regex(@"\[[A-Z][A-Z-]+\]"), front);
    }
}

/// <summary>The page as served.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class AcousticNeuromaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/acoustic-neuroma";

    private readonly WebApplicationFactory<Program> _factory;

    public AcousticNeuromaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Acoustic neuroma", html);

        // THE DESCRIPTION'S SOURCED FORM. /review round 1's first blocker: the
        // page led, in bold and in the description, with "what usually shapes the
        // decision is your hearing, not the size on the scan" — supported by NONE
        // of the thirteen sources, contradicted three times downstream by the page
        // itself ("Size matters", "the usual choice when a growth is large", "what
        // your team watches for is size and change"), and banded by size in EANO.
        // Over-reassurance on the first screen, which §12.12 calls the more
        // dangerous direction because it stops being true later. This assertion
        // previously PINNED the unsourced form into the render test.
        Assert.Contains("your hearing counts in the decision", html);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support", "/get-help-now");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page. This page
        // deep-links THREE anchors on /treatments/watch-and-wait, so the check is
        // emphatically not vacuous here.
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

        foreach (var directive in new[]
                 {
                     "[MECHANISM]", "[ESCALATION]", "[CAUSES]", "[CAREGIVER]",
                     "[CROSSWALK]", "[TUMOR-BOARD]", "[SPINAL-CORD]", "[POSTERIOR-FOSSA-SYNDROME]",
                 })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        Assert.Contains("Call an ambulance", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
        Assert.Contains("block the flow of fluid", html, StringComparison.Ordinal);

        // AND THE EXCLUDED BLOCKS' CONTENT DOES NOT — asserted on the RENDERED
        // page, because that is the only place a wrongly-composed block becomes
        // visible. Read out of the block files rather than re-typed, so rewording
        // a block cannot silently retire this check. ALL FOUR, not three: WI-540
        // named four and checked three, on the one test whose subject is the
        // blocker-that-is-an-absence.
        foreach (var name in new[]
                 { "crosswalk", "tumor-board", "spinal-cord", "posterior-fossa-syndrome" })
        {
            var block = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, name + ".md"));
            var sentence = Regex.Matches(
                    CuratedPage.Flatten(CuratedPage.ReaderText(block)), @"[A-Z][^.]{45,90}\.")
                .Select(m => m.Value)
                .FirstOrDefault();
            Assert.False(string.IsNullOrEmpty(sentence),
                $"no sentence could be read out of {name}.md, so this guard proves nothing");

            // HTML FLATTENED TOO — /review round 2's nit 7. The needle is
            // whitespace-flattened but the haystack was raw, so the guard only
            // fired because the renderer happens to join plain hard wraps with a
            // space. A wrap adjacent to inline markup KEEPS its newline in the
            // HTML (visible on this very page), so rewording a block could make
            // this assertion silently unfailable. Copied identically into
            // CraniopharyngiomaPageTests and PituitaryTumorPageTests — flagged for
            // /pm rather than fixed here, because those are shipped pages.
            Assert.DoesNotContain(sentence, CuratedPage.Flatten(html), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheFocusedRadiationTooltipStillFiresOnThisPage()
    {
        // THIS PAGE'S HALF OF A CONTRACT ASSERTED ON ANOTHER PAGE.
        // StereotacticRadiosurgeryPageTests asserts the EXACT set of pages
        // keeping that glossary entry reachable, and this page was registered
        // there when it became the fifth. It qualifies only because it says the
        // term in prose without linking it (§12.8, WI-519: linking a word turns
        // its tooltip off). Asserting it here too means a "tidy-up" that links
        // the term fails on THIS page, where the edit would be made, rather than
        // only in a sibling's test file where nobody would look.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("def-stereotactic-radiosurgery", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndex()
    {
        // WI-412 shipped a page nothing linked to. This one is listed from
        // /tumors because taxonomy.yml carries a `slug: acoustic-neuroma` entry —
        // a property of two files agreeing rather than something anybody
        // asserted, which is what this test fixes. WI-540's handproof established
        // that the listing depends on the taxonomy entry and NOT on the page
        // having a description.
        var html = await _factory.CreateClient().GetStringAsync("/tumors");

        Assert.Contains("/tumors/acoustic-neuroma", html, StringComparison.Ordinal);
        Assert.Contains("Acoustic neuroma", html, StringComparison.Ordinal);
    }
}
