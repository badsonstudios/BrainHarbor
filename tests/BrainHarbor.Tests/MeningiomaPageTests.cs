using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-527: meningioma, deepened. Wave 3's first item, the first §12.3
/// SEVENTEEN-SECTION TUMOR HUB since WI-518, and the first hub outside the
/// glioma family — so the template is §12.3's, not §12.8's twelve slots. The
/// previous five items were all library pages and the two are easy to confuse.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * THE THREE "SLOWER GROWTH" REPORT FEATURES. The dossier offers a reader
///     older age, calcification on CT and a hypointense T2 signal as things to
///     look for in their own report, cited to a paper that found the opposite
///     or found them irrelevant — under an editorial note saying they are
///     "far more useful than a generic we'll keep an eye on it", which is
///     §12.13's tell. None of them may appear.
///   * THE HORMONE SENTENCE. "Standard combined oral contraceptives do not
///     increase risk" is the dossier's simplification of a CUH sentence that
///     carries an exception — and cyproterone acetate IS in some combined
///     pills. Simplified, it tells those women they are fine.
///   * "benign" used as comfort. A grade 1 meningioma in a bad place can take
///     somebody's sight, and can come back a decade later.
///   * a grade percentage, a recurrence percentage, a dose, or a scan
///     interval (§12.4, §12.5, and WI-521's no-schedule ruling).
///   * "brain invasion" read as a verdict when its own authors call its
///     prognostic value unsettled.
///
/// Guards carry canaries (§12.8, WI-523), run PAGE-WIDE where a section scope
/// would leave a door beside them (WI-524), and are asserted over text with
/// the emphasis markers stripped (WI-526 — `**do**\nqualify` defeated a phrase
/// guard structurally on that item).
/// </summary>
public sealed class MeningiomaPageContentTests
{
    private const string Slug = "tumors/meningioma";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string HormoneHeading = "Am I on a medicine that should be looked at?";

    private static string Page => CuratedPage.Read("tumors", "meningioma.md");

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The title and description. <c>ReaderText</c> strips the front matter, so
    /// every body-scoped guard is blind to them while
    /// <c>ContentPage.cshtml</c> renders the description as the first paragraph
    /// under the heading (§12.8, WI-524).
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
    /// Everything, with the emphasis markers removed (§12.8, WI-526: a bolded
    /// word and a line wrap between two words defeats a phrase guard
    /// structurally, and no vocabulary fixes that).
    /// </summary>
    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand)";

    /// <summary>
    /// The break harness walked through the old list with "roughly a fifth",
    /// "a twentieth" and "a tenth" (§12.8, WI-527): a share guard that only
    /// knows halves, thirds and quarters knows almost no fractions.
    /// </summary>
    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?)";

    /// <summary>
    /// Removes an allowed phrase from a sentence instead of exempting the whole
    /// sentence. §12.8, WI-527: a substring allowlist is a free ride for
    /// everything else in the sentence — the harness parked "and almost all
    /// people who have one die of something else" on the end of an allowed
    /// clause and the guard waved it through.
    /// </summary>
    private static string Redact(string sentence, IEnumerable<string> allowed)
    {
        foreach (var phrase in allowed)
        {
            sentence = Regex.Replace(sentence, Regex.Escape(phrase), " ", RegexOptions.IgnoreCase);
        }

        return sentence;
    }

    /// <summary>
    /// Word runs shared with another page ON PURPOSE (§12.10). Kept tiny: each
    /// is a link label or a safety claim that must read identically wherever a
    /// reader meets it.
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // The standard support-section doors, worded as every hub words them.
        "Get help now if you want to talk to a person today.",

        // THE OUTLOOK GATE'S OPENING. §12.5 prescribes the gate, and a reader
        // meeting it on a second hub has to recognise it — so this one is
        // deliberately identical rather than rewritten, which is §12.10's
        // "must not have two strengths" applied to a consent prompt.
        //
        // AND THE CORPUS HAS ALREADY DRIFTED, which this fifth hub is what
        // finally surfaced: /tumors/astrocytoma says "before DECIDING whether
        // you want them", /tumors/glioma says "worth UNDERSTANDING what those
        // numbers are", and oligodendroglioma and glioblastoma say "before YOU
        // DECIDE". Three spellings across four hubs. This page matches the
        // majority; unifying the other two is a /pm item rather than something
        // a meningioma item should do to four sibling pages, and it is logged
        // in PROGRESS.
        // Carried with the fence word, because `:::outlook` is not a heading
        // and `Shingles` only strips headings — so the run reaches back across
        // the fence into the paragraph.
        "outlook Doctors sometimes describe outlook using numbers. It is worth knowing what "
        + "those numbers are before you decide whether you want them.",

        // THE TWO SENTENCES /review ASKED ME TO MIRROR. Its A5 and A8 findings
        // were that this page stated the watch-and-wait trigger at a different
        // strength from /treatments/watch-and-wait, and dropped the
        // counterweight that page carries. Fixing both means quoting the
        // sibling — which is the point, because the alternative is two pages
        // saying nearly-the-same-thing about when a plan changes (§12.10).
        "ask your team how much change would make them act",
        "more people ended up having treatment than not",

        // And the cord-compression sentence, for the same reason and more
        // sharply: /review's A3 found this page stating it one strength ABOVE
        // /tumors/spinal-cord-tumor. A symptom sorted into a tier is the
        // definitive §12.10 "must not have two strengths" claim, so the two
        // pages now carry it identically rather than each in their own words.
        "New weakness or new bladder trouble is a reason to be seen quickly, not to wait "
        + "for the next appointment. Do not wait for it to get bad first, because the early "
        + "signs are often vague ones.",

        // The gate's closing move, likewise prescribed rather than invented:
        // what shapes a reader's own situation is what their team can see and a
        // page written for everybody cannot. Shared with every hub that has a
        // gate.
        "and how it behaves over the first few years. Those are things your own team can see",
    ];

    private static readonly HashSet<string> AllowedShingles =
        [.. DeliberatelyShared.SelectMany(s => Shingles(s, 8))];

    [Fact]
    public void TheThreeSlowerGrowthReportFeaturesAppearNowhere()
    {
        // RULING 1, AND IT IS THE ITEM'S BEST-LOOKING REASSURANCE. The dossier
        // §A.6 offers a reader three things to look for in their own report and
        // be reassured by: older age at discovery, calcification on CT, and a
        // hypointense or isointense T2 signal. It cites PMC10180371 and adds
        // that they are "far more useful than a generic we'll keep an eye on
        // it" — §12.13's tell, a note arguing for a claim BECAUSE it is
        // reassuring.
        //
        // The paper says otherwise: "Mean baseline tumor volume, however, was
        // larger the elder patients were at diagnosis", and growth was "not
        // affected" by "signal intensity on T2 and FLAIR sequence".
        //
        // A reader who hunts through a radiology report for calcification and
        // relaxes on finding it has been reassured by nothing.
        // SHAPE, NOT VOCABULARY. /review walked the first version four ways
        // with synonyms the stem list could not see -- "calcium" for
        // `calcif\w*`, "dark on the T2 pictures" for `hypointense`, "found
        // later in life" and "found by chance late in life" for `older`. What
        // the ruling forbids is any REPORT FEATURE offered as a growth
        // prediction, so both halves are now open sets.
        const string reportFeature =
            @"(?:calcif\w*|calcium|hypointense|isointense|T2 signal|signal on T2|dark on (?:the )?T2|"
            + @"low signal|density|older|later in life|late in life|age at diagnosis)";
        const string growthPrediction =
            @"(?:slow\w*|quiet\w*|less|lower|reassur\w*|good sign|grow\w*|take their time|"
            + @"hardly ever need|never need)";

        var deadReassurance = new Regex(
            @"\b" + reportFeature + @"\b[^.]{0,90}\b" + growthPrediction + @"\b|"
            + @"\b" + growthPrediction + @"\b[^.]{0,90}\b" + reportFeature + @"\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Calcified meningiomas grow more slowly, so look for that on your report.",
                     "A hypointense T2 signal goes with a lower growth rate.",
                     "If the signal on T2 is low, it tends to grow less.",
                     "Meningiomas found when you are older grow slower.",
                     // /review's four, all green against the first version.
                     "Tumors with a lot of calcium in them usually take their time.",
                     "If the report calls the tumor dark on the T2 pictures, that goes with a "
                     + "quieter tumor too.",
                     "The ones found later in life are the quietest of all.",
                     "The ones found by chance late in life hardly ever need anything doing.",
                 })
        {
            Assert.Matches(deadReassurance, known);
        }
        Assert.DoesNotMatch(deadReassurance, Plain);

        // And the page does not invite the reader to look for growth clues in
        // their report at all, however phrased.
        var hunt = new Regex(
            @"\blook for\b[^.]{0,40}\b(?:report|scan|MRI|CT)\b[^.]{0,40}\b(?:slow|grow)\w*|"
            + @"\bif your report (?:says|mentions|shows)\b[^.]{0,60}\b(?:slow|reassur|good)\w*",
            RegexOptions.IgnoreCase);
        Assert.Matches(hunt, "If your report says calcification, that is reassuring about growth.");
        Assert.DoesNotMatch(hunt, Plain);

        // WHAT THE SAME PAPER DOES SUPPORT IS ON THE PAGE INSTEAD, because a
        // page that drops a reassurance without replacing it leaves the reader
        // where it found them (§12.8, WI-521).
        var scans = PlainOf("Follow-up scans, and what to do while you wait");
        Assert.Matches(new Regex(@"Growth tends to slow down after the first couple of years",
            RegexOptions.IgnoreCase), scans);
        Assert.Matches(new Regex(@"after several more years it slows almost to nothing",
            RegexOptions.IgnoreCase), scans);

        // AND IT SAYS WHICH "IT". The end-to-end read of the RENDERED page
        // caught this and nothing else could: the sentence used to end "after
        // several more it becomes very small", in a paragraph whose subject is
        // a tumor. A reader tracking the nearest noun is told their meningioma
        // shrinks, which is false and is §12.12's more dangerous direction.
        // The pronoun is now named, and the wrong reading is closed by hand.
        Assert.Matches(new Regex(@"That is the growth slowing, not the tumor shrinking",
            RegexOptions.IgnoreCase), scans);
        Assert.DoesNotMatch(new Regex(
            @"\b(?:tumors?|meningiomas?) (?:can |may |will |often )?(?:shrink|get smaller|"
            + @"go away|disappear)\b", RegexOptions.IgnoreCase), Plain);
    }

    [Fact]
    public void TheHormoneSectionKeepsThreeLevelsAtThreeStrengths()
    {
        // THE BACKLOG'S OWN INSTRUCTION: "these must not be merged into
        // 'hormones cause meningioma'". §12.12's shape three times in one
        // section, and the merged version is dangerous in both directions —
        // it frightens a woman on an ordinary pill and it under-warns a woman
        // on cyproterone acetate.
        var section = PlainOf(HormoneHeading);

        // Level 1: regulator-backed and actionable, with the instruction.
        Assert.Matches(new Regex(@"cyproterone acetate", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"stop it permanently if somebody is diagnosed with a\s*meningioma",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"chlormadinone acetate and nomegestrol\s*acetate",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"similar but lower risk", RegexOptions.IgnoreCase), section);

        // Level 2: hedged exactly as the source hedges it, AND carrying the
        // trade-off clause the source attaches. Dropping that clause is how a
        // page gets somebody to stop HRT alone and suffer for nothing.
        Assert.Matches(new Regex(@"\*\*seems to\*\* raise the risk", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(Reader(Section(HormoneHeading))));
        Assert.Matches(new Regex(@"menopausal symptoms can be\s*worse to live with than the meningioma",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Do not stop HRT on your own", RegexOptions.IgnoreCase), section);

        // Level 3: no apparent increase, WITH THE EXCEPTION. This is the
        // dossier's defect: it says "standard combined oral contraceptives do
        // not appear to increase meningioma risk", where CUH says "oral
        // contraceptive pills OTHER THAN THOSE CONTAINING THE DRUG CPA ... do
        // not seem to increase the risk". Cyproterone acetate is in some
        // combined pills, prescribed for acne and hirsutism.
        // The label itself carries the qualifier, so a reader who reads only
        // the bold lead-in is not told their pill is safe. /review's A14 and
        // the guard below between them forced that: the label used to read
        // "No apparent increase: most other contraceptive pills", which is a
        // reassurance that stands alone.
        Assert.Matches(new Regex(
            @"Other contraceptive pills: no apparent increase, with one exception",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"here is the\s*exception that matters", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"some combined pills contain cyproterone acetate",
            RegexOptions.IgnoreCase), section);

        // The unqualified version, which is the defect. Banned page-wide.
        // THE VOCABULARY IS AN OPEN SET AND THE FIRST VERSION USED A CLOSED
        // ONE. /review beat it with "birth control pill", with "Pills taken for
        // acne are nothing to do with this" (which never says "contraceptive"
        // at all, and is the item's headline defect restored), and with "the
        // implant, the coil and the contraceptive injection" -- a widening to
        // devices, where the dossier records concern extending to
        // medroxyprogesterone acetate, i.e. the injection.
        const string contraceptive =
            @"(?:(?:standard |ordinary |normal |regular |combined |oral )*contraceptive(?: pill)?s?|"
            + @"birth control(?: pill)?s?|the pill|pills taken for acne|implant|coil|injection|"
            + @"patch(?:es)?)";
        // BOTH ORDERS. "The same goes for the implant, the coil and the
        // contraceptive injection" puts the reassurance in front of the thing
        // being reassured about, and a one-directional pattern cannot see it.
        const string reassured =
            @"(?:do not|does not|no|nothing to do with|not the problem|safe|fine|"
            + @"the same goes for|likewise)";
        var unqualified = new Regex(
            @"\b" + contraceptive + @"\b[^.]{0,70}\b" + reassured + @"\b|"
            + @"\b" + reassured + @"\b[^.]{0,70}\b" + contraceptive + @"\b",
            RegexOptions.IgnoreCase);
        var carriesException = new Regex(
            @"\bexception\b|\bother than\b|\bcyproterone\b|\bwhich kind\b", RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Standard combined oral contraceptives do not increase meningioma risk.",
                     "Pills taken for acne are nothing to do with this.",
                     "Your birth control pill is almost certainly not the problem.",
                     "The same goes for the implant, the coil and the contraceptive injection.",
                 })
        {
            Assert.Matches(unqualified, known);
            Assert.DoesNotMatch(carriesException, known);
        }

        var merged = CuratedPage.SentencesOf(Plain)
            .Where(s => unqualified.IsMatch(s))
            .Where(s => !carriesException.IsMatch(s))
            .ToList();
        Assert.True(merged.Count == 0,
            "this page tells a reader her contraceptive pill is safe without the exception that "
            + "some of them contain cyproterone acetate:\n  " + string.Join("\n  ", merged));

        // The action, which is to ask rather than to reason.
        Assert.Matches(new Regex(@"take the box to your\s*team and ask which kind it is",
            RegexOptions.IgnoreCase), section);

        // And the merged headline claim never appears in any form.
        // THE BACKLOG'S OWN INSTRUCTION IS THAT THE THREE LEVELS MUST NOT
        // MERGE, and /review merged them in one sentence the literal ban could
        // not see: "any pill with a hormone in it is worth asking about,
        // because they all carry some of this". So the ban is on the FLATTENING
        // as well as on the causal claim.
        var mergedClaim = new Regex(
            @"\bhormones? (?:cause|causes|can cause)\b[^.]{0,30}\bmeningioma\b|"
            + @"\bmeningiomas? (?:are|is) caused by hormones?\b|"
            + @"\b(?:any|every|all) (?:pill|medicine|hormone)\w*\b[^.]{0,60}"
            + @"\b(?:worth asking|all carry|some of this|same risk)\b|"
            + @"\bthey all\b[^.]{0,30}\b(?:carry|have|share)\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "Hormones cause meningioma.",
                     "In practice, any pill with a hormone in it is worth asking about, "
                     + "because they all carry some of this.",
                 })
        {
            Assert.Matches(mergedClaim, known);
        }
        Assert.DoesNotMatch(mergedClaim, Plain);

        // AND THE STRENGTH WORDS ARE PINNED PER LEVEL, because /review raised
        // level 2 to level 1's strength ("HRT is now accepted as a cause of
        // meningioma too") and lowered level 1 to level 2's ("regulators have
        // wondered about a possible link") while every sentence assertion above
        // stayed green -- the page said both things at once.
        var raisesHrt = new Regex(
            @"\bHRT\b[^.]{0,60}\b(?:accepted|established|proven|is a cause|causes)\b|"
            + @"\b(?:accepted|established|proven)\b[^.]{0,40}\bHRT\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(raisesHrt, "HRT is now accepted as a cause of meningioma too.");
        Assert.DoesNotMatch(raisesHrt, Plain);

        var lowersCpa = new Regex(
            @"\bcyproterone\b[^.]{0,80}\b(?:might|may be|possible link|wondered|"
            + @"unclear|not certain)\b|"
            + @"\b(?:wondered|possible link)\b[^.]{0,60}\bcyproterone\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(lowersCpa, "Regulators have wondered about a possible link with cyproterone.");
        Assert.DoesNotMatch(lowersCpa, Plain);

        // And no permission to stop, or comparative between routes, anywhere in
        // the section — WI-525's "check the AGENT, not the modal", which
        // /review beat with "coming off it is usually the right call" sitting
        // directly under "Do not stop HRT on your own".
        var permission = new Regex(
            @"\b(?:coming off it|stopping it|to stop it)\b[^.]{0,50}"
            + @"\b(?:right call|sensible|the answer|best)\b|"
            + @"\bpatches are the safer\b|\b(?:safer|better) kind\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "For most women the meningioma is the bigger worry, so coming off it is "
                     + "usually the right call.",
                     "Patches are the safer kind if you do stay on it.",
                 })
        {
            Assert.Matches(permission, known);
        }
        Assert.DoesNotMatch(permission, Plain);
    }

    [Fact]
    public void CyproteroneAcetateIsNamedAsGenderAffirmingTherapyWithoutJudgement()
    {
        // The dossier's own instruction: "cyproterone acetate is used in
        // gender-affirming hormone therapy; this needs to be named explicitly
        // and non-judgementally." A trans woman taking it who does not learn
        // this from a meningioma page will not learn it — nothing else in the
        // corpus says it.
        var section = PlainOf(HormoneHeading);

        Assert.Matches(new Regex(@"gender-affirming hormone therapy for trans women",
            RegexOptions.IgnoreCase), section);

        // Named alongside its other uses, so it does not read as a warning
        // aimed at one group.
        Assert.Matches(new Regex(@"heavy\s*hair growth", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"\bacne\b", RegexOptions.IgnoreCase), section);

        // And the sentence ends on a door rather than on a loss: there are
        // alternatives, and this is a conversation.
        Assert.Matches(new Regex(@"there are\s*alternatives", RegexOptions.IgnoreCase), section);

        // Nothing that reads as a judgement or an instruction to stop.
        // /review got three past the five-phrase list, including an invented
        // behavioural claim about a named group and a guarantee about somebody
        // else's behaviour (WI-521's and WI-523's defect).
        var judgement = new Regex(
            @"\bshould not (?:be taking|take|have been)\b|\brisky choice\b|\bworth it\b|"
            + @"\bworth carrying\b|\bstop taking it\b|\bcome off it\b|"
            + @"\byour team will (?:move|switch|change) you\b|"
            + @"\ba choice between\b|\bonly you can (?:make|decide)\b|"
            + @"\bmany trans (?:women|people) (?:decide|choose|find)\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "You should not be taking that if you have a meningioma.",
                     "Many trans women decide the risk is not one worth carrying.",
                     "Your team will move you onto something else.",
                     "It is a choice between your hormones and your brain, and only you can "
                     + "make it.",
                 })
        {
            Assert.Matches(judgement, known);
        }
        Assert.DoesNotMatch(judgement, section);
    }

    [Fact]
    public void BrainInvasionIsDefusedRatherThanConfirmed()
    {
        // §D.1: "if a reader's report says 'brain invasion,' do not let the
        // page imply this is settled bad news." PMC10339387 gives the strict
        // definition and its own hedges.
        var section = PlainOf(ReportHeading);

        // The definition, in the two directions that matter to a reader
        // holding a report.
        Assert.Matches(new Regex(@"Indenting the brain is not invasion", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"growing\s*along the outside of a blood vessel", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"break through the innermost covering layer", RegexOptions.IgnoreCase),
            section);

        // It does move the grade, which the page says rather than softening.
        Assert.Matches(new Regex(@"moves the tumor to grade 2", RegexOptions.IgnoreCase), section);

        // And the contested status is part of the defusing, not a footnote.
        Assert.Matches(new Regex(@"unsettled among the specialists who wrote the rule",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"no consistent link with the tumor coming back",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"more testing and a closer plan, not a verdict",
            RegexOptions.IgnoreCase), section);

        // The verdict grammar the source does not support.
        // SCOPED TO THE PARAGRAPH, because /review beat the noun-phrase
        // version four ways with pronouns and paraphrase -- "It does make
        // coming back more likely", "treat it as a tumor that behaves like a
        // higher grade", "the group that is watched hardest", "the feature that
        // marks out the aggressive ones". The paragraph is the unit: no
        // sentence in it may make a prognostic claim.
        var invasionPara = Regex.Match(section,
            @"Brain invasion\.(.*?)(?=Ki-67)", RegexOptions.Singleline);
        Assert.True(invasionPara.Success, "the brain-invasion entry has gone");

        var prognostic = new Regex(
            @"\b(?:means|predicts|is a sign|marks out|behaves like|more likely|"
            + @"watched hardest|treated soonest|aggressive ones|higher grade|worse outlook|"
            + @"will come back)\b",
            RegexOptions.IgnoreCase);
        var allowedInvasion = new[]
        {
            // The two the source DOES support: it moves the grade, and its
            // prognostic value is unsettled.
            "moves the tumor to grade 2",
            "unsettled among the specialists who wrote the rule",
            "no consistent link with the tumor coming back",
            "more testing and a closer plan, not a verdict",
        };
        var claims = CuratedPage.SentencesOf(invasionPara.Groups[1].Value)
            .Where(s => prognostic.IsMatch(s))
            .Where(s => !allowedInvasion.Any(a => s.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        Assert.True(claims.Count == 0,
            "the brain-invasion entry makes a prognostic claim its own source calls unsettled:\n  "
            + string.Join("\n  ", claims));

        // Canary, with a pronoun rather than the noun phrase.
        Assert.Matches(prognostic, "It does make coming back more likely.");
        Assert.Matches(prognostic, "It is the feature that marks out the aggressive ones.");
    }

    [Fact]
    public void ThePapillaryAndRhabdoidRegradingClaimIsAbsent()
    {
        // RULING 5. The dossier says "papillary and rhabdoid meningiomas are
        // no longer automatically grade 3", cited to www.ajnr.org (403) and to
        // PMC10477988, which does not contain it — the fetch reports that
        // paper treating them as grade 3 features. It would have been a kind
        // thing to tell somebody holding a 2015 report, and there is no
        // reachable source for it (§12.14: a claim with no live citation is
        // not published, whatever else the page bans).
        foreach (var word in new[] { "papillary", "rhabdoid" })
        {
            Assert.DoesNotContain(word, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE CLAIM SHAPE, not just the two words. §12.14's rule again:
        // banning the URL and the noun is not banning the claim. /review wrote
        // the whole unpublished paragraph without either word -- "A few
        // unusual-looking subtypes used to be called grade 3 automatically.
        // They are not any more."
        var regrading = new Regex(
            @"\b(?:used to be|once were|were once)\b[^.]{0,60}\bgrade 3\b|"
            + @"\bgrade 3\b[^.]{0,40}\b(?:automatically|always|by default)\b|"
            + @"\b(?:no longer|not any more|not anymore)\b[^.]{0,60}\bgrade[sd]?\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "A few unusual-looking subtypes used to be called grade 3 automatically.",
                     "They are not any more, and are graded on their own merits.",
                 })
        {
            Assert.Matches(regrading, known);
        }
        Assert.DoesNotMatch(regrading, Plain);
    }

    [Fact]
    public void ThePageCarriesNoGradeOrRecurrencePercentage()
    {
        // §12.4 R2 for the grades: three sources give 80/18/2-ish and the
        // dossier attributes 85-95/5-10/1-5 to a paper that gives the other
        // set. §12.5 for the recurrence figures, and the dossier's own §A.8
        // marks them "[NUMBERS — EDITORIAL DECISION NEEDED]" and recommends
        // the shape.
        // No trailing `\b` after the percent alternatives: `%` is a non-word
        // character, so a word boundary after it never matches and the whole
        // branch is dead. The canary caught it (§12.8, WI-523).
        var share = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b" + CountWord + @"\s+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            // The fraction branch needs a POPULATION noun. A bare `\w+` fired
            // on the page's own location list — "the dura between the two
            // halves of the brain" — which is a rule that fails a correct page
            // (§12.8, WI-508).
            + @"\b" + Fraction + @"\s+of\s+(?:all\s+)?(?:the\s+)?(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|meningiomas|tumors|cases)\b|"
            // A fraction does not need the word "of" to be a share. "half the
            // people who have the condition" is the commonest phrasing there
            // is, and the branch above cannot see it.
            + @"\b" + Fraction + @"\s+(?:all\s+)?the\s+(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|meningiomas|tumors|cases)\b|"
            // A fraction with NO noun at all: "grade 2 is roughly a fifth".
            + @"\b(?:roughly|about|around|nearly|approximately|between|something|"
            + @"just over|just under)\s+(?:a|one)\s+" + Fraction + @"\b|"
            + @"\b(?:is|are|was|were|makes? up|accounts? for)\s+"
            + @"(?:roughly\s+|about\s+|around\s+|nearly\s+)?(?:a|one)\s+" + Fraction + @"\b|"
            + @"\b" + Fraction + @"\s+the\s+time\b|"
            // A share in words is still a share.
            + @"\bmajorit(?:y|ies)\b|\bminorit(?:y|ies)\b|\ba handful\b|"
            // "most" with words in between. The old branch demanded the noun
            // sit next to the quantifier, which is a one-word door: "most
            // GRADE 2 tumors" strolled through it.
            + @"\b(?:most|almost all|nearly all|hardly any|the majority of)\s+(?:\w+\s+){0,3}"
            + @"(?:people|patients|meningiomas|tumors|cases|them|series)\b",
            RegexOptions.IgnoreCase);

        var allowed = new[]
        {
            // "Most meningiomas are grade 1" is the shape §12.4 R2 asks for
            // when the counts disagree, and it is the page's own refusal to
            // print one.
            "Most meningiomas are not cancer",
            "Most meningiomas are grade 1 and grow slowly",
            "Grade 1 is most of them",
            // The page saying why it prints no share.
            "We do not print what share falls into each grade",

            // THE ONE SHARE THE PAGE DOES CARRY, listed rather than left to a
            // loose regex, so that adding a second is a deliberate act.
            // NF2-related schwannomatosis: a genetics fact a reader uses for
            // their relatives, not to place themselves — they already have the
            // meningioma this page is about. It only ever pushes toward asking,
            // which is the WI-527 test for an endpoint over a target.
            //
            // "Most watched meningiomas grow a little" USED TO BE HERE, and the
            // rendered read is what took it off: it was an unsourced majority
            // claim in the reassuring direction, in a section that already
            // routes to the sibling carrying the study where MORE people ended
            // up treated than not. The action survived it; the share did not.
            "Around half the people who have the condition have meningiomas",
        };

        foreach (var known in new[]
                 {
                     "About 80% of meningiomas are grade 1.",
                     "Grade 2 makes up 5 to 10 percent of them.",
                     "A quarter of grade 2 tumors come back within five years.",
                     "Nearly all patients with a grade 1 tumor stay clear.",
                     // The nine the harness got through (§12.8, WI-527).
                     "Grade 2 is roughly a fifth.",
                     "Without radiation they come back about half the time.",
                     "Most grade 2 tumors come back within five years.",
                     "Grade 1 is the large majority.",
                     "Only a handful in every hundred are grade 3.",
                     "Grade 3 is a tiny minority.",
                     "Grade 2 accounts for something between a twentieth and a tenth.",
                     "It is four in five in every series.",
                     "Half the people who have it never need anything.",
                 })
        {
            Assert.Matches(share, known);
        }

        // THE ALLOWLIST REDACTS RATHER THAN EXEMPTS, and here is the proof: an
        // allowed clause with a banned one welded onto it still fails.
        Assert.Matches(share, Redact(
            "Most meningiomas are grade 1 and grow slowly, and almost all people who have one "
            + "die of something else.", allowed));
        Assert.Matches(share, Redact(
            "We do not print what share falls into each grade, though it is about 80 percent "
            + "grade 1.", allowed));

        var offenders = CuratedPage.SentencesOf(Plain)
            .Where(s => share.IsMatch(Redact(s, allowed)))
            .ToList();
        Assert.True(offenders.Count == 0,
            "this page publishes a share a reader would use to place themselves:\n  "
            + string.Join("\n  ", offenders));

        // And it says why, because a page that is silently silent leaves the
        // reader to supply their own (§12.8, WI-521).
        var grade = PlainOf("Is it cancer? What does its grade mean?");
        // Not "the published figures disagree with each other" — that is
        // /treatments/clinical-trials' sentence, written one item earlier, and
        // the corpus-wide restatement check caught it coming back out of my
        // own fingers.
        Assert.Matches(new Regex(@"The counts that exist do not\s*agree with one another",
            RegexOptions.IgnoreCase), grade);

        // The recurrence answer is the SHAPE, in words.
        var recurrence = PlainOf("If it comes back, or changes");
        Assert.Matches(new Regex(@"grade 1 comes back rarely", RegexOptions.IgnoreCase), recurrence);
        Assert.Matches(new Regex(@"Grade 3 comes back often and early", RegexOptions.IgnoreCase),
            recurrence);
    }

    [Fact]
    public void ThePageCarriesNoDoseNoSizeThresholdAndNoMitoticCount()
    {
        // §12.4 R1. The dossier offers 54 Gy in 30 fractions, 60 Gy, 1.8-2.0
        // Gy per fraction, "SRS for tumours smaller than 3 cm", single-fraction
        // SRS "typically under 2 cm", observation supported "especially under
        // 2 cm", a 25 mg cyproterone threshold, and mitotic counts in two
        // different unit systems. None of it is a number this reader acts on,
        // and the size thresholds are the worst of them: a reader who measures
        // their own report against "under 2 cm" has been handed a decision
        // that is not theirs.
        var clinicalNumber = new Regex(
            @"\b\d+(?:\.\d+)?\s*(?:Gy|gray|mg|milligrams?|cm|centimet\w*|mm)\b|"
            + @"\b" + CountWord + @"\s*(?:Gy|gray|mg|milligrams?|centimet\w*)\b|"
            + @"\b\d+\s+fractions?\b|\b" + CountWord + @"\s+fractions?\b|"
            + @"\bper 10 high[- ]power fields\b|\bper mm2\b|\bmitoses\b|"
            + @"\b(?:smaller|larger|bigger|less) than\s+" + CountWord + @"\s*(?:cm|centimet\w*)\b|"
            // THE UNIT WORDS THEMSELVES. "fifty-four grays in thirty visits"
            // walked past every branch above: the spelled compound is not in
            // CountWord, and `gray\b` does not match the plural (§12.8, WI-527).
            + @"\bgrays?\b|\bmillimet\w*|\bmilligram\w*|\bcentimet\w*|\bdividing cells\b|"
            // A THRESHOLD IN ANY UNIT, OR NONE. It is the shape rather than the
            // unit that does the damage — a reader measuring their own report
            // against "under twenty millimetres" has been handed a decision
            // that is not theirs, and taking the unit out does not give it back.
            + @"\b(?:more than|less than|fewer than|no more than|at least|up to|"
            + @"under|over|above|below)\s+" + CountWord + @"\b|"
            + @"\b" + CountWord + @"\s+or (?:more|fewer|less)\b|"
            + @"\bthe size of a\b|\bor smaller\b|\bor bigger\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "The standard dose for a grade 2 tumor is 54 Gy in 30 fractions.",
                     "Focused radiation is used for tumors smaller than 3 cm.",
                     "Monitoring is advised above 25 mg a day.",
                     "Grade 2 needs 4 to 19 mitoses per 10 high-power fields.",
                     // The six the harness got through (§12.8, WI-527).
                     "The usual course is fifty-four grays in thirty visits.",
                     "A typical course is 54 grays.",
                     "It suits a tumor about the size of a walnut or smaller.",
                     "The single treatment is for tumors under twenty millimetres.",
                     "A grade 2 report needs at least three dividing cells, and a grade 3 twelve or more.",
                     "Doctors are told to watch anyone taking more than one tablet a day.",
                 })
        {
            Assert.Matches(clinicalNumber, known);
        }
        Assert.DoesNotMatch(clinicalNumber, Plain);

        // And the page says the doses are the team's, rather than being silent
        // about why it has none.
        var treatment = PlainOf("How is it usually treated?");
        Assert.Matches(new Regex(@"Doses and schedules are your team's to set", RegexOptions.IgnoreCase),
            treatment);

        // The two mitotic unit systems ARE worth a reader knowing about,
        // without either number. Not asserted as present — the page does not
        // carry it, and this records that as a decision rather than an
        // oversight: a reader comparing two reports is a real situation, but
        // both sources for the thresholds are on the dead list, and a page
        // that says "the units changed" without being able to say to what
        // leaves the reader worse off.
        Assert.DoesNotContain("high-power field", Plain, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheElevenYearEndpointIsOnThePageAndNoScanIntervalIs()
    {
        // The best fact in the whole research pack, and it was on a dead URL
        // (academic.oup.com). Found open at PMC12137216: "no recurrences were
        // observed after 11 years of follow-up for completely resected WHO
        // Grade 1 meningiomas", and stopping surveillance after eleven clear
        // years "is reasonable".
        //
        // ELEVEN YEARS IS AN ENDPOINT, NOT AN INTERVAL. WI-521 ruled that a
        // general page prints no surveillance SCHEDULE, and that ruling is
        // about intervals — the thing a frightened reader can act on wrongly.
        // An endpoint only ever pushes toward asking, and "your scans can stop
        // one day" is the most under-communicated good news available here.
        var scans = PlainOf("Follow-up scans, and what to do while you wait");

        Assert.Matches(new Regex(@"the scans can eventually stop", RegexOptions.IgnoreCase), scans);
        Assert.Matches(new Regex(@"no tumor\s*of that kind came back after eleven clear years",
            RegexOptions.IgnoreCase), scans);

        // Scoped to the reader it is true of, in the same sentence. A reader
        // with a grade 2 tumor who takes this home has been told something
        // false about themselves (§12.12).
        var claim = CuratedPage.SentencesOf(scans)
            .Where(s => Regex.IsMatch(s, @"eleven|scans can eventually stop", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(claim.Count >= 2,
            $"only {claim.Count} sentences carry the eleven-year claim, which is fewer than the page "
            + "has ever had — the regex has stopped seeing its own prose");
        Assert.Contains(claim, s =>
            s.Contains("grade 1", StringComparison.OrdinalIgnoreCase)
            && s.Contains("completely", StringComparison.OrdinalIgnoreCase));

        // AND IT LIVES IN EXACTLY ONE SECTION. A section-scoped guard proves
        // nothing about the rest of the page: the harness put the endpoint in
        // the short version, unscoped, and every assertion above still passed
        // because every assertion above was reading a different section
        // (§12.8, WI-524's rule, which this guard had not been given).
        var endpoint = new Regex(
            @"\beleven\b|scans can (?:eventually )?stop|(?:scans|they) do not go on forever",
            RegexOptions.IgnoreCase);
        var carried = CuratedPage.SentencesOf(Plain).Where(s => endpoint.IsMatch(s)).ToList();
        Assert.True(carried.Count >= 2,
            $"only {carried.Count} sentences carry the endpoint page-wide, which is fewer than the "
            + "page has ever had — the regex has stopped seeing its own prose");
        foreach (var sentence in carried)
        {
            Assert.Contains(CuratedPage.Flatten(sentence).Trim(), scans, StringComparison.Ordinal);
        }

        // And the other grades are told, in the same section, that it does not
        // apply to them.
        Assert.Matches(new Regex(@"For a grade 2 or grade 3 tumor, or one where some was left behind",
            RegexOptions.IgnoreCase), scans);

        // NO INTERVAL ANYWHERE. The source gives 3-4 monthly, 6 monthly and
        // annual; the dossier adds 6 months then yearly, and 1 to 2 years.
        var interval = new Regex(
            @"\b(?:every|each)\s+" + CountWord + @"?\s*(?:months?|years?|weeks?)\b|"
            + @"\bannual(?:ly)?\b|\b" + CountWord + @"[-\s]*(?:to|or)[-\s]*" + CountWord
            + @"\s*(?:months?|years?)\b|"
            + @"\bevery (?:other )?year\b|\b" + CountWord + @"\s*(?:monthly|yearly)\b|"
            + @"\bscan(?:s|ned)? (?:at|after)\s+" + CountWord + @"\s*(?:months?|years?)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "A repeat scan at about six months, then annually.",
                     "Grade 3 tumors are scanned every 3 to 4 months for two years.",
                     "Follow-up MRI every other year is reasonable.",
                 })
        {
            Assert.Matches(interval, known);
        }
        Assert.DoesNotMatch(interval, Plain);

        // And the interval is handed over rather than omitted silently.
        Assert.Matches(new Regex(@"Your own intervals are your team's to set", RegexOptions.IgnoreCase),
            scans);
    }

    [Fact]
    public void TheWhoGradeAndTheSimpsonGradeAreToldApart()
    {
        // RULING 11, and this page is the first in the corpus where it bites.
        // A meningioma reader can hold a pathology report with a WHO grade in
        // ARABIC numerals and an operative note with a Simpson grade in ROMAN
        // numerals, measuring completely different things. Nothing in the
        // corpus explains that, and the collision is worse here than anywhere
        // because CNS5 retired Roman numerals for WHO grades — so the glioma
        // hubs teach that Roman numerals mean a stale report, and on THIS page
        // they mean nothing of the kind.
        var section = PlainOf(ReportHeading);

        Assert.Matches(new Regex(@"Simpson describes \*{0,2}how much came out", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(Reader(Section(ReportHeading))));
        Assert.Matches(new Regex(@"not a second opinion on your WHO grade", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"I, II and III\s*all count as a complete removal",
            RegexOptions.IgnoreCase), section);

        // The part that stops a reader concluding their report is out of date.
        // /review's BLOCKER A1: the page used to assert the Roman-numeral
        // retirement itself, with no reachable source — the dossier hangs it on
        // ajnr.org (403) and academic.oup.com (dead), and this suite bans
        // ajnr.org BY NAME, which is §12.14 exactly. `[CROSSWALK]` carries the
        // claim AND its citation (PMC9723092), so the page defers to the block
        // and keeps only the part that is true nowhere else in the corpus.
        Assert.Matches(new Regex(@"Simpson\s*grades are the exception, and always have been",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Roman numerals beside the word Simpson are expected",
            RegexOptions.IgnoreCase), section);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Matches(new Regex(@"Your WHO grade is\s*the one that should be an ordinary number",
            RegexOptions.IgnoreCase), section);

        // THE SENTENCES THE HARNESS REPLACED. Each of these was pinned nowhere,
        // so the claim could be contradicted outright and every assertion above
        // still passed (§12.8, WI-527).
        Assert.Matches(new Regex(
            @"Roman numeral grades belong to the old way of writing things\s*down",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"a report can say .grade 1, Simpson II., and\s*each number is telling you something "
            + @"different", RegexOptions.IgnoreCase), section);

        // The Simpson grade is ALWAYS in Roman numerals on this page. An Arabic
        // one re-creates the exact collision the section exists to undo.
        var arabicSimpson = new Regex(@"\bSimpson\s+\d", RegexOptions.IgnoreCase);
        Assert.Matches(arabicSimpson, "a report can say \"grade 1, Simpson 2\"");
        Assert.DoesNotMatch(arabicSimpson, Plain);

        // And the two scales are never ranked against each other, which is the
        // reading the whole section exists to prevent.
        var ranked = new Regex(
            @"\bSimpson\s+(?:I{1,3}V?|IV|V|\d)\b[^.]{0,60}\b(?:better|worse|higher|lower)\b|"
            + @"\b(?:better|worse|higher|lower)\b[^.]{0,60}\bthan a (?:WHO )?grade\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(ranked, "A Simpson II is a better result than a grade 2.");
        Assert.DoesNotMatch(ranked, Plain);

        // AND THE CORPUS-WIDE RULE STILL HOLDS FOR WHO GRADES. §12.9 records
        // this guard biting three times (WI-505, WI-506, WI-508) and five
        // copies of it being case-sensitive until WI-516's harness planted
        // "**Grade III.**". A Roman numeral immediately after the word "grade"
        // is still a defect on this page — it is only the SIMPSON numerals
        // that are legitimate, and they never follow the bare word "grade".
        // `grade[sd]?`, not `grade`: "older reports have them GRADED I, II and
        // III" and "WHO GRADES I to III" are the same defect with an inflection
        // on the end, and both walked through the bare form (§12.8, WI-527).
        var romanWhoGrade = new Regex(@"\bgrade[sd]?\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase);
        Assert.Matches(romanWhoGrade, "Grade III meningiomas are treated with surgery and radiation.");
        Assert.Matches(romanWhoGrade, "a grade II tumor");
        Assert.Matches(romanWhoGrade, "Older reports have them graded I, II and III.");
        Assert.Matches(romanWhoGrade, "You may still see WHO grades I to III written out.");
        Assert.DoesNotMatch(romanWhoGrade, Plain);

        // And the glioma hubs' version of the rule is unchanged by this page
        // existing — read rather than assumed (§12.10).
        foreach (var sibling in new[] { "astrocytoma", "oligodendroglioma", "glioblastoma" })
        {
            var text = CuratedPage.Flatten(CuratedPage.ReaderText(
                CuratedPage.Read("tumors", sibling + ".md")));
            Assert.DoesNotMatch(romanWhoGrade, text);
        }
    }

    [Fact]
    public void BenignIsHandledRatherThanUsedAsComfort()
    {
        // The dossier's §C.5 makes the point and this is the page where it
        // matters most: "benign" means "not cancer" and readers hear
        // "harmless". A grade 1 meningioma in the wrong place takes somebody's
        // sight, and can come back a decade later.
        var grade = PlainOf("Is it cancer? What does its grade mean?");

        Assert.Matches(new Regex(@"the word .benign. does more harm than good here",
            RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"people\s*hear .harmless.", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(@"can take your sight", RegexOptions.IgnoreCase), grade);

        // It is never offered as the reassurance.
        var comfort = new Regex(
            @"\b(?:it is|its|it's) (?:only |just |merely )?benign\b|"
            + @"\bbenign,? so\b|\bgood news:? (?:it is |it's )?benign\b|"
            + @"\bbenign(?:,| and) (?:which means |meaning )?(?:harmless|nothing to worry)\b|"
            // THE COMFORT WITHOUT THE WORD. Every one of these got past the
            // three branches above, and not one of them needs "benign" to do
            // the damage the section exists to stop (§12.8, WI-527).
            + @"\bthe good (?:part|thing|bit|news)\b|"
            + @"\b(?:never|cannot|can't|will not|won't|does not|doesn't) spread\b|"
            + @"\b(?:cannot|can't|will not|won't|is not going to) kill you\b|"
            + @"\bnothing to worry about\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(comfort, "The good news is it is benign.");
        Assert.Matches(comfort, "It is benign, so there is nothing to worry about.");
        Assert.Matches(comfort, "The good part is that it is not cancer.");
        Assert.Matches(comfort, "It will never spread to anywhere else in your body.");
        Assert.Matches(comfort, "What not cancer does mean is that it cannot kill you.");
        Assert.DoesNotMatch(comfort, Plain);

        // "harmless" MAY APPEAR ONLY INSIDE QUOTATION MARKS — as a word other
        // people say, which is the whole point of the passage, and never as the
        // page's own description. Stripping the quoted spans is what lets the
        // ban be absolute: a page-local ban list containing a word the page
        // correctly quotes in order to refute is the WI-508 defect, and the
        // fix is not to drop the ban (§12.14).
        var unquoted = Regex.Replace(Plain, "\"[^\"]*\"", " ");
        Assert.Contains("harmless",
            Regex.Replace("Most of them are harmless in themselves.", "\"[^\"]*\"", " "),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("harmless", unquoted, StringComparison.OrdinalIgnoreCase);

        // And the two sentences that do the honest work, pinned, because the
        // harness rewrote both of them into their own opposites and nothing in
        // this test noticed.
        Assert.Matches(new Regex(
            @"The useful questions are where it is and what it\s*is doing, not which side of that "
            + @"word it falls on", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(
            @"Both things are true and you do not\s*have to pick one to say out loud",
            RegexOptions.IgnoreCase),
            PlainOf("For the person caring for someone with this"));

        // And the honest limit of the outside-the-brain reassurance is on the
        // page, in its own subsection, because that is the sentence this whole
        // page could be read as contradicting.
        var what = PlainOf("What is a meningioma?");
        Assert.Matches(new Regex(@"Leaving it out is how .it is not in your\s*brain. turns into",
            RegexOptions.IgnoreCase), what);
        Assert.Matches(new Regex(@"Pressing causes real symptoms and\s*real damage",
            RegexOptions.IgnoreCase), what);
    }

    [Fact]
    public void LocationBeatsSizeIsStatedRatherThanImplied()
    {
        // The backlog: "location matters more than size". It is the organising
        // idea, and an implication is not a statement — a reader holding a
        // report with a measurement on it will read the measurement unless
        // told not to.
        var section = PlainOf("Where does it grow, and why does it cause these symptoms?");

        Assert.Matches(new Regex(@"location decides almost\s*everything, and size decides less than you think",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"The measurement on your report is not the headline",
            RegexOptions.IgnoreCase), section);

        // With the concrete version, which is what makes it land.
        Assert.Matches(new Regex(@"A small meningioma against the nerve to your eye",
            RegexOptions.IgnoreCase), section);

        // And it is in the short version too, because §12.3 says most readers
        // get 20 to 28% of a page.
        var shortVersion = PlainOf("The short version");
        Assert.Matches(new Regex(@"not how big it is", RegexOptions.IgnoreCase), shortVersion);
        Assert.Matches(new Regex(@"where it sits and what it is\s*pressing on", RegexOptions.IgnoreCase),
            shortVersion);

        // The location list names the ones with a distinctive give-away, since
        // those are the ones a reader recognises themselves in.
        foreach (var owed in new[] { "Loss of\nsmell", "bulge", "worse at night", "field of vision" })
        {
            Assert.Contains(CuratedPage.Flatten(owed), section, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheSpinalEmergencyIsAddedAndScopedAndTheBlockIsNotEdited()
    {
        // §12.10's warning, applied: the shared escalation block is "not
        // automatically right for a hub whose emergencies differ". It covers
        // this hub's intracranial emergencies well — sudden loss of vision is
        // in its ambulance tier ("suddenly not being able to speak, move one
        // side, or see") and the hydrocephalus picture is its same-day tier.
        //
        // What it does NOT cover is SPINAL CORD COMPRESSION, which is a spinal
        // meningioma's own emergency and appears nowhere in the corpus. It is
        // added as a SCOPED line above the block rather than by editing the
        // block, because it is true of one location and false of every other
        // hub that includes the block.
        var section = CuratedPage.Flatten(Reader(Section(SymptomHeading)));

        var spinal = Regex.Match(section,
            @"\*\*If your meningioma is on your spinal cord.*?(?=\[ESCALATION\])",
            RegexOptions.Singleline);
        Assert.True(spinal.Success, "the scoped spinal-cord line has gone");
        var line = spinal.Value;

        // Scoped in its own first clause, before any symptom is named.
        Assert.StartsWith("**If your meningioma is on your spinal cord", line.Trim(),
            StringComparison.Ordinal);

        // /review's BLOCKER A3: the first version was an invented tier with no
        // source at all, stated one strength ABOVE /tumors/spinal-cord-tumor,
        // which already files the symptom. It now uses the corpus's own
        // sentence, cites PMC4080478 for the red flags, and routes to the page
        // that owns the subject.
        foreach (var owed in new[]
                 {
                     "new weakness in the legs", "a change in how you walk",
                     "numbness around the\nsaddle area", "bladder or bowel",
                     "not to wait for the next\nappointment",
                 })
        {
            Assert.Contains(CuratedPage.Flatten(owed), line, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Matches(new Regex(@"Do not wait for it to get bad first", RegexOptions.IgnoreCase),
            line);
        Assert.Contains("/tumors/spinal-cord-tumor", line, StringComparison.Ordinal);

        // ONE CLAIM, ONE STRENGTH. The sibling now carries the identical
        // sentence, read rather than assumed (§12.10).
        var sibling = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "spinal-cord-tumor.md")));
        Assert.Contains(
            "New weakness or new bladder trouble is a reason to be seen quickly, not to wait",
            sibling, StringComparison.Ordinal);
        Assert.Contains("Do not wait for it to get bad first", sibling, StringComparison.Ordinal);

        // THE BLOCK ITSELF IS UNTOUCHED. Editing Content/blocks/escalation.md
        // to carry a spinal line would put it on every glioma hub, where it is
        // false — the WI-514 blast-radius trap.
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        // "saddle" and "your water" are on this list because the harness put
        // the spinal line into the block in the reader's OWN words rather than
        // the clinical ones, and a ban list made of clinical nouns never saw it
        // (§12.8, WI-527). "legs" is deliberately NOT here — the block's
        // same-day tier says "an arm or a leg", which is true on every hub.
        foreach (var absent in new[]
                 {
                     "spinal cord", "bladder", "bowel", "both legs",
                     "saddle", "your water", "numbness", "continence",
                 })
        {
            Assert.DoesNotContain(absent, block, StringComparison.OrdinalIgnoreCase);
        }

        // ONE ESCALATION AUTHORITY ON THE PAGE (§12.10). A second tier list
        // somewhere else in the page is the failure mode the shared block
        // exists to prevent, and every guard above is scoped to this section,
        // so the harness simply built its list in a different one.
        var urgency = new Regex(
            @"\b(?:right away|straight away|immediately|urgent\w*|999|911|the same day|"
            + @"as soon as|quickly)\b", RegexOptions.IgnoreCase);
        var whole = CuratedPage.Flatten(Reader(CuratedPage.ReaderText(Page)));
        var strays = Regex.Matches(whole, @"\*\*(.+?)\*\*")
            .Where(m => urgency.IsMatch(m.Groups[1].Value))
            .Select(m => m.Value)
            .Where(bold => !section.Contains(bold, StringComparison.Ordinal))
            .ToList();
        Assert.True(strays.Count == 0,
            "an urgency directive sits outside the symptoms section, where it competes with the "
            + "shared block:\n  " + string.Join("\n  ", strays));

        // And the block is included rather than restated, with the tiers diffed
        // against the siblings by the shared helper.
        Assert.Contains("[ESCALATION]", section, StringComparison.Ordinal);
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);
    }

    [Fact]
    public void TheRadiationAssociationCannotBeBlamedOnAModernScan()
    {
        // The dossier's own honesty note, and it is right: the evidence comes
        // from tinea capitis irradiation, atomic bomb exposure and high-dose
        // childhood cranial radiotherapy. "Do not let a reader conclude their
        // head CT caused this."
        var section = PlainOf("Did I cause this?");

        Assert.Matches(new Regex(@"radiotherapy to the head as\s*children", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"not the same\s*thing as a dental x-ray or a CT scan",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"your head scan did not cause this", RegexOptions.IgnoreCase),
            section);

        // And the action, because a reader who did have childhood radiotherapy
        // needs something to do with that rather than a fact to sit with.
        Assert.Matches(new Regex(@"it changes how closely\s*you are watched", RegexOptions.IgnoreCase),
            section);

        // No relative risk, no percentage, no latency figure.
        var figures = new Regex(
            @"\brelative risk\b|\b" + CountWord + @"[- ]?fold\b|\brisk of \d|"
            + @"\b" + CountWord + @"\s*(?:to|-)\s*" + CountWord + @"\s+years? (?:later|after)\b|"
            + @"\bby age " + CountWord + @"\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(figures, "Childhood cranial radiotherapy carries a 5 to 12% risk by age 40.");
        Assert.Matches(figures, "The relative risk was 9.5 compared with controls.");
        Assert.DoesNotMatch(figures, Plain);

        // And the door stays shut. Two sentences got past every assertion
        // above: one reopening the link to a modern scan without naming a
        // figure, and one closing it with an unsourced quantifier and an
        // instruction about how to feel — which is §12.12's more dangerous
        // direction, dressed as kindness (§12.8, WI-527).
        var reopened = new Regex(
            @"\b(?:does |do |can |will )?uses?\s+radiation\b|"
            + @"\bthe (?:risk|chance) is (?:small|low|tiny|slight|minimal|very small)\b|"
            + @"\btry not to (?:worry|dwell|think)\b|\bnot worth worrying\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(reopened, "A CT does use radiation, and nobody pretends otherwise.");
        Assert.Matches(reopened, "The risk is small, so try not to dwell on it.");
        Assert.DoesNotMatch(reopened, Plain);

        // The shared self-blame block is included rather than restated.
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheWatchAndWaitDepressionFigureAppearsNowhere()
    {
        // RULING 7. The dossier §A.6 repeats the 4.26-fold depression figure,
        // cited to a charity page "(via search summary)" and a Mayo Clinic
        // DISCUSSION FORUM, on a domain that is on the known-dead list.
        // WI-522 already ruled it unpublishable: univariate odds ratio, 31
        // versus 31, the abstract mislabels it as multivariate, and three
        // other studies found no difference independent of management
        // strategy. A ruling made on one page reaches every page whose dossier
        // repeats the claim.
        // `(?:4|four)`: the numeral ban is a spelling away from useless.
        Assert.DoesNotMatch(new Regex(@"4\.26|fourfold|four-fold|\b(?:4|four) times as likely\b",
            RegexOptions.IgnoreCase), Plain);

        var depression = new Regex(
            @"\b(?:depress\w*|anxiet\w*|anxious|low mood)\b[^.]{0,80}"
            + @"\b(?:more likely|more common|higher|risk|times)\b|"
            + @"\b(?:more likely|more common|higher risk|times more|times as likely|fold)\b"
            + @"[^.]{0,80}\b(?:depress\w*|anxiet\w*|anxious|low mood)\b|"
            // THE COMPARISON ITSELF, whatever noun it uses for the feeling.
            // RULING 7 is about the WATCHED-versus-OPERATED claim, and "low
            // enough to need help" is that claim with the clinical word taken
            // out — which is exactly how it got past the two branches above.
            + @"\b(?:watched|watching|watch and wait)\b[^.]{0,100}"
            + @"\b(?:more likely|more common|times as likely|four times|far more)\b|"
            + @"\b(?:more likely|more common|times as likely|four times|far more)\b[^.]{0,100}"
            + @"\b(?:watched|watching|watch and wait)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(depression,
            "People on watch and wait are four times as likely to score for depression.");
        Assert.Matches(depression,
            "People who are watched are four times as likely to end up low enough to need help.");
        Assert.Matches(depression,
            "Low mood is far more common in people being watched than in people who had surgery.");
        Assert.DoesNotMatch(depression, Plain);

        // And the page routes to the sibling that owns the framing, rather
        // than restating any of it.
        var treatment = PlainOf("How is it usually treated?");
        Assert.Contains("/treatments/watch-and-wait", treatment, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"it has a section on meningioma specifically",
            RegexOptions.IgnoreCase), treatment);
    }

    [Fact]
    public void TheGradeIsAnEstimateWithoutTissueAndThePageSaysSo()
    {
        // The dossier's §A.10 question 3 calls this "a genuine gap in most
        // patient material", and it is: a grade comes from looking at cells,
        // and before an operation there are no cells. A reader told "grade 1"
        // off a scan and told nothing else does not know they were given an
        // estimate.
        var section = PlainOf("How do doctors find out it is this?");

        Assert.Matches(new Regex(@"if you have not had surgery, your\s*grade is an estimate",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Grade comes from looking at cells, and that needs\s*tissue",
            RegexOptions.IgnoreCase), section);

        // And it does not swing into telling the reader the estimate is
        // worthless, because it is not.
        Assert.Matches(new Regex(@"genuinely\s*informative", RegexOptions.IgnoreCase), section);

        // With the question that resolves it.
        Assert.Matches(new Regex(@"fair to ask which of those two you are being given",
            RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3, and this is the first hub since WI-518. The five items in
        // between were all §12.8 LIBRARY pages, and §12.8's own WI-510 lesson
        // is that copying the last page written is how a template quietly
        // shrinks — so this is read from §12.3 rather than from WI-526.
        Assert.Equal(
            [
                "The short version",                                             // 0
                "What is a meningioma?",                                         // 1
                "Is it cancer? What does its grade mean?",                       // 2
                "Where does it grow, and why does it cause these symptoms?",     // 3
                "What symptoms does it cause?",                                  // 4
                "How do doctors find out it is this?",                           // 5
                "What do the words on my report mean?",                          // 6
                "How is it usually treated?",                                    // 7
                "Am I on a medicine that should be looked at?",                   // 7b
                "What is treatment actually like, and what is normal afterwards?", // 8
                "Everyday life: work, driving, seizures and tiredness",          // 9
                "Follow-up scans, and what to do while you wait",                // 10
                "If it comes back, or changes",                                  // 11
                "Did I cause this?",                                             // self-blame, demoted
                "What might happen over time",                                   // 12, gated
                "For the person caring for someone with this",                   // 13
                "What to ask your team",                                         // 14
                "Where to get support",                                          // 15
            ],
            Headings());

        // The hormone section sits with treatment rather than with causes, and
        // that is deliberate: it is an ACTION, not an explanation of how the
        // tumor got there. §12.3 puts "Did I cause this?" late on purpose, and
        // burying the one instruction on the page down there would have put it
        // below the outlook gate for most readers.
        var headings = Headings();
        Assert.Equal(headings.IndexOf("How is it usually treated?") + 1,
            headings.IndexOf(HormoneHeading));
        Assert.True(headings.IndexOf(HormoneHeading) < headings.IndexOf("Did I cause this?"),
            "the hormone section is an action and belongs above the self-blame block");

        // Outlook sits after everything actionable (§12.3's design principle).
        Assert.True(headings.IndexOf("What might happen over time")
            > headings.IndexOf("If it comes back, or changes"));
    }

    [Fact]
    public void TheOutlookGateIsClosedAndCarriesTheRecurrenceVersusSurvivalWarning()
    {
        // §12.5. And this page has an extra reason for the gate that the
        // glioma hubs do not: most meningiomas are not cancer, so the figures
        // that circulate are usually about RECURRENCE rather than survival,
        // and the two get read as each other.
        var raw = Regex.Match(Page, @"^## What might happen over time.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(raw), "the outlook section has gone");

        // Gate opens after the heading and closes before the next section.
        Assert.Matches(new Regex(@"^## What might happen over time\s*\n\n:::outlook", RegexOptions.Multiline),
            raw);
        Assert.Contains("\n:::\n", raw, StringComparison.Ordinal);

        var inside = Regex.Match(raw, @":::outlook(.*?)\n:::", RegexOptions.Singleline).Groups[1].Value;
        Assert.Matches(new Regex(@"usually about\s*whether the tumor comes back rather than about how long anybody lives",
            RegexOptions.IgnoreCase), CuratedPage.Flatten(inside));

        // Nothing sits outside the gate in that section but the heading.
        var outside = raw
            .Replace(Regex.Match(raw, @":::outlook.*?\n:::", RegexOptions.Singleline).Value, "",
                StringComparison.Ordinal)
            .Replace("## What might happen over time", "", StringComparison.Ordinal);
        Assert.True(string.IsNullOrWhiteSpace(outside),
            "something sits outside the outlook gate in that section: " + outside.Trim());

        // No figure inside it either -- the gate is about consent to read, not a
        // licence to publish numbers (§12.5). A bare `\d` ban is too blunt: the
        // section legitimately says "grade 1" and "grade 2", and a grade is not
        // a prognosis figure. So: every digit in there has to be a grade.
        var digits = Regex.Matches(inside, @"\d+");
        Assert.All(digits, m => Assert.Matches(
            new Regex(@"grade\s*$", RegexOptions.IgnoreCase),
            inside[..m.Index]));

        // AND NO FIGURE-SHAPED CLAIM IN WORDS. A digit ban is not a prognosis
        // ban: "people in that group live as long as anybody else" and "almost
        // everybody in that group is done with it" are both survival claims
        // with no digit in them, and both sat inside the gate untouched
        // (§12.8, WI-527). The gate is consent to read, not a licence.
        var told = new Regex(
            @"\blive as long as\b|\blife expectancy\b|\blifespan\b|\bsurviv\w*|\bdie of\b|"
            + @"\bcure[ds]?\b|\b(?:almost|nearly) (?:everybody|everyone|all of them)\b|"
            + @"\bmost people\b|\bthe vast majority\b|\bdone with it\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(told, "People in that group live as long as anybody else.");
        Assert.Matches(told, "Almost everybody in that group is done with it.");
        Assert.DoesNotMatch(told, CuratedPage.Flatten(inside));
    }

    [Fact]
    public void TheSharedBlocksEveryHubOwesAreIncluded()
    {
        // Contract items, and §12.10: composed rather than restated.
        // [MECHANISM] and [CROSSWALK] are on this list because they were on no
        // list: deleting the mechanism block and its heading broke nothing
        // (§12.8, WI-527). A contract item this test does not name is not a
        // contract item.
        foreach (var directive in new[]
                 { "[ESCALATION]", "[CAUSES]", "[CAREGIVER]", "[MECHANISM]", "[CROSSWALK]" })
        {
            Assert.Contains(directive, Page, StringComparison.Ordinal);
        }

        // And the caregiver section adds what is only true here, rather than
        // restating the block in different words — which is the defect WI-525
        // wrote down and WI-526 repeated, and which no shingle check can see.
        var care = PlainOf("For the person caring for someone with this");
        Assert.Matches(new Regex(@"The changes you are seeing may be the location", RegexOptions.IgnoreCase),
            care);
        Assert.Matches(new Regex(@".Not cancer. will be said to you, and it will not settle anything",
            RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"Go with them to the appointment where the medicine list comes up",
            RegexOptions.IgnoreCase), care);

        // THREE, AND THE SECTION SAYS THREE. A fourth lead-in is how a
        // paraphrase of the block arrives — "get two phone numbers before you
        // leave" is the block's own advice in different words, and no shingle
        // check can see a paraphrase (this test said so and then let one
        // through: §12.8, WI-527).
        Assert.Matches(new Regex(@"Three things that belong to this tumor in particular",
            RegexOptions.IgnoreCase), care);
        var leads = Regex.Matches(CuratedPage.Flatten(Reader(
                Section("For the person caring for someone with this"))), @"\*\*(.+?)\*\*")
            .Select(m => m.Groups[1].Value)
            .ToList();
        Assert.True(leads.Count == 3,
            $"the caregiver section promises three things and carries {leads.Count}:\n  "
            + string.Join("\n  ", leads));

        var block = CuratedPage.Flatten(Regex.Replace(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")), @"^---.*?\n---", "",
            RegexOptions.Singleline));
        var blockShingles = Shingles(block, 8).ToHashSet();
        var own = Shingles(Reader(Section("For the person caring for someone with this")), 8)
            .Where(blockShingles.Contains).Where(CarriesContent).ToList();
        Assert.True(own.Count == 0, "the caregiver section restates the block:\n  "
            + string.Join("\n  ", own));
    }

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // §12.8 (WI-521): corpus-wide, not hand-picked neighbours. The nearest
        // here is /treatments/watch-and-wait, which already carries its own
        // meningioma paragraph — so this is the page most likely to be
        // duplicated, and the guard must not be pointed at it by hand.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("meningioma.md", StringComparison.Ordinal))
            .Concat(Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"));

        foreach (var file in others)
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var overlaps = Shingles(CuratedPage.ReaderText(File.ReadAllText(file)), 8)
                .Where(pageShingles.Contains)
                .Where(s => !AllowedShingles.Contains(s))
                .Where(CarriesContent)
                .Distinct()
                .ToList();

            Assert.True(overlaps.Count == 0,
                $"this page restates {slug}:\n  " + string.Join("\n  ", overlaps));
        }
    }

    [Fact]
    public void ThePageNeverMinimisesOrCharacterisesTheDiagnosis()
    {
        var reader = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(reader, Slug);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(reader), StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, promoted here rather than site-wide (§12.8, WI-510:
        // promote at the SECOND page). Each is the vocabulary a "mostly not
        // cancer" diagnosis attracts, and each is corpus-clean.
        //
        // "harmless" WAS on this list and came straight off it, which is
        // §12.8's (WI-509) rule biting on a list I wrote myself: a substring
        // ban belongs only if NO correct sentence contains the phrase, and
        // this page's whole opening exists to name the wrong conclusion and
        // refute it — *"leaving it out is how 'it is not in your brain' turns
        // into 'it is harmless'"*. Banning the word would have forced the page
        // to stop saying the thing it is for.
        foreach (var phrase in new[] { "nothing to worry about", "the good kind", "lucky" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(reader), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // The first draft shipped FOUR — "tumour", "organising", "recognisable"
        // and "commonest" — which is the most any page in the corpus has
        // carried, because five of this page's nine sources are British or
        // European (CUH, and the dossier's own spelling throughout).
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in new[]
                 {
                     "tumour", "commonest", "oedema", "consultant", "specialist nurse", "chemist",
                     "MHRA", "999", "petrol", "paediatr", "hospitalis", "randomis",
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

        // The four the page's load-bearing claims rest on, by name.
        foreach (var owed in new[] { "NBK560538", "PMC10477988", "PMC10339387", "PMC12137216", "cuh.nhs.uk" })
        {
            Assert.Contains(urls, u => u.Contains(owed, StringComparison.Ordinal));
        }

        // The known-dead list, plus the two this item added to it.
        foreach (var dead in new[]
                 {
                     "academic.oup.com", "mdpi.com", "ascopubs.org", "mayoclinic.org",
                     "hopkinsmedicine.org", "sciencedirect.com", "journals.lww.com", "medscape.com",
                     // Added by this item: 403 on fetch, and it carried the
                     // whole CNS5 grading section in the dossier.
                     "ajnr.org",
                     // link.springer.com returns a JavaScript shell (WI-524),
                     // and insightsimaging redirects into it.
                     "link.springer.com", "insightsimaging",
                     // A discussion forum, cited by the dossier for the
                     // watch-and-wait emotional cost.
                     "connect.mayoclinic.org",
                 })
        {
            Assert.False(urls.Any(u => u.Contains(dead, StringComparison.OrdinalIgnoreCase)),
                $"{dead} is cited; the front matter records why it was not used");
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void NciIsCitedForFramingAndNeverForNamingOrGrading()
    {
        // §12.1, "the rule most likely to be broken by accident, because the
        // offending source is the one we would naturally lean on hardest".
        // NCI's patient PDQ uses Roman numeral grades and still says
        // "hemangiopericytoma". It is cited here for the plain definition of
        // the meninges and for active surveillance as a treatment category,
        // which §12.1 permits — and for nothing else.
        var front = CuratedPage.FrontMatter(Page);
        Assert.Contains("cancer.gov", front, StringComparison.Ordinal);

        // The front matter records the scope, so the next editor does not
        // widen it by accident.
        Assert.Matches(new Regex(@"NCI is cited for the plain definition", RegexOptions.IgnoreCase),
            front);

        // And the retired vocabulary NCI would have supplied is absent.
        foreach (var retired in new[] { "hemangiopericytoma", "anaplastic astrocytoma", "oligoastrocytoma" })
        {
            Assert.DoesNotContain(retired, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static List<string> Headings() =>
        Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

    private static readonly HashSet<string> Stopwords =
    [
        "a", "an", "and", "are", "as", "at", "be", "been", "before", "but", "by", "can", "do",
        "does", "for", "from", "get", "go", "had", "has", "have", "how", "i", "if", "in", "into",
        "is", "it", "its", "like", "may", "me", "more", "much", "my", "no", "not", "of", "on",
        "one", "or", "other", "our", "out", "over", "own", "so", "some", "than", "that", "the",
        "their", "them", "then", "there", "these", "they", "this", "to", "up", "was", "we",
        "were", "what", "when", "which", "who", "will", "with", "would", "you", "your",
    ];

    private static bool CarriesContent(string shingle) =>
        shingle.Split(' ').Count(w => !Stopwords.Contains(w)) >= 3;

    private static IEnumerable<string> Shingles(string text, int n)
    {
        text = Regex.Replace(text, @"^#{1,6} .*$", " ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\]\([^)]*\)", "] ");
        var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+").Select(m => m.Value).ToList();
        for (var i = 0; i + n <= words.Count; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(n));
        }
    }
}

/// <summary>The page as served.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class MeningiomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/meningioma";

    private readonly WebApplicationFactory<Program> _factory;

    public MeningiomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Meningioma", html);
        Assert.Contains("where it sits and what it is", html);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The third argument is the section's ANCHOR ID, not its heading text.
        // The named onward doors are asserted rather than counted, because a
        // floor is met by any set of links while the door that matters goes.
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/tumors", "/treatments/watch-and-wait",
            "/tests/follow-up-scans");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page. This page
        // deep-links /seizures/living-with#driving.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("What might happen over time", html);
        // The gate renders as a details element that is closed on load, so the
        // reader chooses (§12.5). Asserted on the rendered page rather than on
        // the source, because the source only carries the fence.
        Assert.Contains("<details", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryBlockDirectiveComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        // The literal directive never reaches a reader (§12.10, WI-514's trap).
        foreach (var directive in new[] { "[ESCALATION]", "[CAUSES]", "[CAREGIVER]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // And each block's own content does.
        Assert.Contains("Call an ambulance", html, StringComparison.Ordinal);
        Assert.Contains("Cell phones", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePagesVocabularyFiresAsTooltipsWhereTheGlossaryOwnsTheWord()
    {
        // The page uses several words the glossary already defines and does not
        // define itself, so they keep their tooltips (§12.8, WI-509's other
        // direction, which is the one WI-510 got wrong).
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("def-", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSiblingThatLinksHereStillDoes()
    {
        // /treatments/watch-and-wait is the only curated page that routes to
        // this hub, and it carries its own meningioma paragraph. Read rather
        // than assumed (§12.10).
        var watch = await _factory.CreateClient().GetStringAsync("/treatments/watch-and-wait");

        Assert.Contains(Url, watch, StringComparison.Ordinal);
        Assert.Contains("Most meningiomas that are watched do grow a little", watch,
            StringComparison.Ordinal);
    }
}
