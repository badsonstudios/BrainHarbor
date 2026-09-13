using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-530, second of two pages: <c>/tests/planning-scans</c>. A §12.8 library
/// page whose slot 3 bends into a list of entries, the way WI-509's marker
/// reference list did — because patients are never offered "an fMRI" in
/// isolation, they are told their team wants to add some sequences.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * SOFTENING THE fMRI LIMIT. Neurovascular uncoupling fails in ONE
///     direction: near a tumor, a working region can stop producing the signal,
///     so it looks quiet on the map. The dangerous reading is "that part is safe
///     to cut". A page that says the scan "can be wrong" without saying WHICH
///     WAY has told the reader nothing.
///   * CLAIMING THE SCAN SETTLES IT. RadiologyInfo's own limit is verbatim:
///     "Your physician may recommend additional tests to confirm the results of
///     fMRI if there are critical decisions to make (such as in planning brain
///     surgery)." /treatments/awake-craniotomy (WI-523) rests on the same
///     sentence, and the two must not drift apart (§12.10).
///   * TURNING THE TASKS INTO A TEST WITH A PASS MARK. The single most useful
///     sentence available is RadiologyInfo's: if a task is too hard, tell the
///     technologist, and they may give you different tasks or easier questions.
///     PMC4757221 found simpler tasks gave MORE reliable results. Losing that is
///     losing the reason this section is the longest on the page.
///   * RESTATING /tests/follow-up-scans. Perfusion and PET are used after
///     treatment to tell treatment change from growth, and WI-521 shipped all of
///     it from two sources that disagree. This page is scoped to BEFORE
///     treatment and routes for the rest.
///   * A REFERENCE-STANDARD CLAIM THE SOURCES DO NOT MAKE. The dossier
///     attributes "intraoperative direct cortical stimulation remains the
///     reference standard" to PMC4757221, which says close to the opposite about
///     fMRI's standing. Not published, and banned as a CLAIM rather than as a
///     citation (§12.14).
///
/// Every guard starts from §12.8's WI-527, WI-528 and WI-529 lessons rather than
/// rediscovering them: numeric bans are threshold-shaped, phrase guards run on
/// <c>Plain</c> so the description is in scope, section order asserts PRESENCE
/// before position, every claim-shaped guard has a canary, and every regex that
/// touches a line break uses <c>\r?\n</c>.
/// </summary>
public sealed class PlanningScansPageContentTests
{
    private const string Slug = "tests/planning-scans";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What are these extra scans?";
    private const string WhyHeading = "Why am I having one?";
    private const string DayHeading = "What happens on the day";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "What does it feel like?";
    private const string FmriHeading = "The scan where you have a job: functional MRI";
    private const string DtiHeading = "The map of the wiring: DTI";
    private const string MrsHeading = "The chemistry graph: MR spectroscopy";
    private const string PerfusionHeading = "The blood supply scan: perfusion";
    private const string PetHeading = "The tracer scan: PET";
    private const string VesselsHeading = "Pictures of the blood vessels";
    private const string HardHeading = "What is hard about these, and what helps";
    private const string BeforeHeading = "What to sort out before the day";
    private const string ResultsHeading = "Who reads it, and how do I get the result?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    private static string Page => CuratedPage.Read("tests", "planning-scans.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// Title and description. <c>ContentPage.cshtml</c> renders the description
    /// as the first paragraph a reader meets, and <c>ReaderText</c> strips the
    /// front matter, so a body-scoped guard cannot see it (§12.8, WI-524).
    /// </summary>
    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    private static string Everything => Headline + " " + Body;

    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private static string Sibling(params string[] path) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(path))), @"[*_]", "");

    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    private static string Redact(string text, IEnumerable<string> allowed)
    {
        foreach (var phrase in allowed)
        {
            text = Regex.Replace(text, Regex.Escape(phrase), " ", RegexOptions.IgnoreCase);
        }

        return text;
    }

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?)";

    // ------------------------------------------------------- the page's shape

    [Fact]
    public void EverySlotThePageOwesIsPresentByNameBeforeAnythingIsSaidAboutOrder()
    {
        // §12.8 (WI-528): -1 is less than everything, so an order test built on
        // `>` alone stays green when a whole section is deleted.
        var headings = Headings();

        string[] required =
        [
            ShortHeading, WhatHeading, WhyHeading, DayHeading, LongHeading, FeelHeading,
            FmriHeading, DtiHeading, MrsHeading, PerfusionHeading, PetHeading, VesselsHeading,
            HardHeading, BeforeHeading, ResultsHeading, QuestionsHeading, NextHeading,
        ];

        foreach (var heading in required)
        {
            Assert.True(headings.Contains(heading),
                $"the page has lost the '{heading}' section entirely:\n  "
                + string.Join("\n  ", headings));
        }

        var positions = required.Select(h => headings.IndexOf(h)).ToList();
        for (var i = 1; i < positions.Count; i++)
        {
            Assert.True(positions[i] > positions[i - 1],
                $"'{required[i]}' has moved above '{required[i - 1]}'");
        }
    }

    [Fact]
    public void EverySectionAndSubsectionCarriesAnExplicitAnchor()
    {
        // §12.8 (WI-509): Markdig derives an id from heading TEXT, and this page
        // is deep-linked BY NAME from three siblings and from its own jump list,
        // so a reworded heading silently drops readers at the top of a long page.
        var without = Regex.Matches(Page, @"(?m)^#{2,3}\s+(.+?)\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .Where(h => !h.EndsWith('}'))
            .ToList();

        Assert.True(without.Count == 0,
            "these headings have no explicit {#anchor}:\n  " + string.Join("\n  ", without));
    }

    [Fact]
    public void TheJumpListNamesEverySixEntriesAndEveryAnchorItNamesExists()
    {
        // §12.8 (WI-509): the scannability of a reference-shaped page comes from
        // a jump list, and a jump list that has drifted from the entries is
        // worse than none. Counted by MARKER, not by numeral (§12.8, WI-526),
        // and each target is proved to exist as a real anchor on this page.
        var list = PlainOf(WhatHeading);

        string[] anchors = ["#fmri", "#dti", "#mrs", "#perfusion", "#pet", "#vessels"];

        foreach (var anchor in anchors)
        {
            Assert.Contains($"]({anchor})", list, StringComparison.Ordinal);
            Assert.Contains("{" + anchor + "}", Page, StringComparison.Ordinal);
        }

        var bullets = Regex.Matches(
            Regex.Match(Page, @"(?ms)^## What are these extra scans\?.*?(?=^## )").Value,
            @"(?m)^\s*[-*]\s").Count;
        Assert.True(bullets == 6,
            $"the jump list has {bullets} items and the page has six entries — one of them has "
            + "been added or dropped without the other");
    }

    [Fact]
    public void TheFunctionalMriSectionIsTheLongestOnThePage()
    {
        // The item's own instruction, and it is not decoration: fMRI is the only
        // one of the five where the patient has a job, which is the whole reason
        // a merged page can still be useful.
        var lengths = new Dictionary<string, int>
        {
            [FmriHeading] = PlainOf(FmriHeading).Length,
            [DtiHeading] = PlainOf(DtiHeading).Length,
            [MrsHeading] = PlainOf(MrsHeading).Length,
            [PerfusionHeading] = PlainOf(PerfusionHeading).Length,
            [PetHeading] = PlainOf(PetHeading).Length,
            [VesselsHeading] = PlainOf(VesselsHeading).Length,
        };

        var longest = lengths.MaxBy(kv => kv.Value).Key;
        Assert.True(longest == FmriHeading,
            $"'{longest}' is now the longest entry. fMRI has to be, because it is the only scan "
            + "here the reader has to do anything for:\n  "
            + string.Join("\n  ", lengths.Select(kv => $"{kv.Value,6}  {kv.Key}")));
    }

    // ---------------------------------------------------- the fMRI safety line

    [Fact]
    public void TheLimitNamesTheDirectionTheScanFailsInRatherThanSayingItCanBeWrong()
    {
        // PMC5669348: neurovascular uncoupling "has the potential to impair or
        // decrease the BOLD response", producing FALSE NEGATIVES in eloquent
        // cortex. The direction is the whole content. "The scan is not perfect"
        // tells a reader nothing; "a working part can look quiet, and a quiet
        // spot looks safe to cut" tells them why the operating-room test exists.
        var limit = PlainOf(FmriHeading);

        Assert.Contains("neurovascular uncoupling", limit, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fail to light up", limit, StringComparison.Ordinal);
        Assert.Contains("safe to operate on", limit, StringComparison.Ordinal);
        Assert.Contains("wrong way round to be wrong", limit, StringComparison.Ordinal);

        // And the mechanism, so the breath-holding instruction is not a mystery.
        Assert.Contains("watches the blood", limit, StringComparison.Ordinal);
        Assert.Contains("Holding your breath", limit, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageNeverClaimsTheScanSettlesItAndSaysTheSameThingAsTheSiblingThatShares()
    {
        // RadiologyInfo, verbatim, and the claim /treatments/awake-craniotomy
        // already rests on. One claim, one wording (§12.8, WI-522).
        Assert.Contains("more tests to confirm what a functional MRI shows",
            PlainOf(FmriHeading), StringComparison.Ordinal);
        Assert.Contains("/treatments/awake-craniotomy", PlainOf(FmriHeading), StringComparison.Ordinal);

        // The sibling is READ, not assumed (§12.10). If its own limit is
        // softened, this goes red here.
        Assert.Contains("It does not replace the test in the room.",
            Sibling("treatments", "awake-craniotomy.md"), StringComparison.Ordinal);

        // The claim-shaped ban, page-wide rather than section-scoped (§12.8,
        // WI-524: a guard scoped to the section named for the defect has a door
        // next to it). Nothing on this page may assert that a scan decides it.
        // INVERTED, because a verb list is not a guard (§12.8, WI-525).
        // /review beat the closed-list version three ways and every one of them
        // is the dangerous direction: "the surgeon KNOWS EXACTLY which parts to
        // leave alone", "the scan SHOWS FOR CERTAIN which part is safe", and
        // "the test in the room is RARELY NEEDED once these scans are done".
        //
        // The rule now: any sentence pairing a scan noun with a certainty word
        // must ALSO carry a hedge, or it fails. Shapes nobody has thought of
        // fail by default instead of passing by default.
        var scanNoun = new Regex(
            @"(?i)\b(?:scan|scans|fmri|functional mri|map|maps|picture|pictures|sequence|sequences)\b");
        var certainty = new Regex(
            @"(?i)\b(?:settles?|settled|decides?|decided|proves?|proven|confirms?|confirmed|"
            + @"exactly|for sure|for certain|definitive|final|knows?|knowing|"
            + @"removes? (?:the |any )?doubt|no doubt|rarely needed|not needed|no need)\b");
        var hedge = new Regex(
            @"(?i)\b(?:may|might|can help|helps?|suggests?|not a verdict|more tests|"
            + @"cannot|does not|do not|confirm what|check(?:s|ed|ing)? (?:the |that )?same|"
            + @"one small spot|rather than)\b");

        // The reference-standard claim is banned outright, as a CLAIM and not
        // only as the citation that carried it (§12.14). No source in the
        // reachable set states it.
        var referenceStandard = new Regex(@"(?i)\b(?:reference|gold)\s+standard\b");
        Assert.DoesNotMatch(referenceStandard, Plain);
        Assert.Matches(referenceStandard, "Direct cortical stimulation remains the reference standard.");

        var unhedged = CuratedPage.SentencesOf(Plain)
            .Where(s => scanNoun.IsMatch(s) && certainty.IsMatch(s) && !hedge.IsMatch(s))
            .ToList();

        Assert.True(unhedged.Count == 0,
            "a sentence states what a planning scan settles, with no hedge in it:\n  "
            + string.Join("\n  ", unhedged));

        // Canaries: every one is a sentence review walked through the first
        // version, and each must fail the rule above.
        foreach (var claim in new[]
                 {
                     "The scan settles where the speech area is.",
                     "A functional MRI proves which part is safe.",
                     "Once the map is done, the surgeon knows exactly which parts to leave alone.",
                     "The scan shows for certain which part is safe.",
                     "The test in the room is rarely needed once these scans are done.",
                 })
        {
            Assert.True(scanNoun.IsMatch(claim) && certainty.IsMatch(claim) && !hedge.IsMatch(claim),
                $"the settles-it rule would wave through: {claim}");
        }
    }

    [Fact]
    public void TheTasksAreNeverATestWithAPassMarkAndTheActionSurvives()
    {
        var section = PlainOf(FmriHeading);

        Assert.Contains("You cannot fail this", section, StringComparison.Ordinal);
        Assert.Contains("tell the person running it", section, StringComparison.Ordinal);
        Assert.Contains("different tasks or easier questions", section, StringComparison.Ordinal);
        // PMC4757221's finding, which is what makes the permission credible
        // rather than kind.
        Assert.Contains("more reliable results", section, StringComparison.Ordinal);

        // §12.8 (WI-525): a membership check is not a tier check — the harness
        // can keep the sentence and soften the instruction attached to it. Ban
        // the downgrade shapes inside this section.
        // Three closed lists of a handful of phrases is not a guard (§12.8,
        // WI-525), and /review beat the first version three ways: "A WEAK result
        // means less to plan from" (only poor/bad/failed were listed), "DO your
        // best on each one" (only "try your best"), and "If you STRUGGLE with
        // the tasks, the scan may have to be done AGAIN" — the pass-mark framing
        // the test is named for, expressed as a consequence rather than a
        // judgement.
        var downgrade = new Regex(
            @"(?i)\b(?:poor|bad|failed|weak|unusable|inadequate|useless|wasted)\s+(?:\w+\s+){0,2}"
            + @"(?:results?|scans?|images?|pictures?|performance|attempt)\b"
            + @"|(?i)\b(?:try|do|give)\s+(?:it |them )?your\s+(?:very\s+)?(?:best|hardest)\b"
            + @"|(?i)\b(?:push through|get through it|it matters that you|"
            + @"you (?:need|have) to (?:manage|get|do) (?:them|it|all))\b"
            + @"|(?i)\bif you (?:cannot|can't|struggle|fail|do not manage)\b[^.]{0,60}"
            + @"\b(?:again|repeat|redo|another appointment|come back)\b");

        foreach (var planted in new[]
                 {
                     "Try your best, because a poor result means a repeat.",
                     "If you struggle with the tasks, the scan may have to be done again.",
                     "Do your best on each one.",
                     "A weak result means less to plan from.",
                 })
        {
            Assert.Matches(downgrade, planted);
        }

        // PAGE-WIDE, not section-scoped. The break harness put all three of
        // review's sentences into "What are these extra scans?" — two sections
        // above the one this test is named for — and every assertion stayed
        // green. §12.8 (WI-524): a guard scoped to the section named for the
        // defect is a guard with a door next to it. Run over the whole page the
        // exemptions would have to be a named allowlist; there are none,
        // because no correct sentence anywhere on this page contains any of
        // these shapes.
        Assert.DoesNotMatch(downgrade, Plain);

        // And the tasks themselves, so a rewrite cannot quietly turn the list
        // into an abstraction. Each is in PMC4757221 or in ACS.
        // Each is a paradigm PMC4757221 names, or ACS's own function list.
        // "Finishing a sentence" was here until /review checked it: sentence
        // completion is the dossier's word and appears in neither source, so it
        // was replaced by Reverse Word Reading in plain words. A test comment
        // asserting a source it has not checked is §12.14 one level up.
        foreach (var task in new[]
                 {
                     "Tapping a thumb and finger", "Moving a foot", "Reading words",
                     "Reading a word backwards", "Thinking of words without saying them out loud",
                     "Naming objects",
                 })
        {
            Assert.Contains(task, section, StringComparison.Ordinal);
        }

        // The scope the source actually attaches (§12.8, WI-520): which task you
        // get depends on where the tumor is. "based on location of the lesion",
        // verbatim in the paper; the hand-knob anatomy the dossier adds is not.
        Assert.Contains("depends on where your tumor is", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCaffeineInstructionSurvivesBecauseItIsTheOneThingTheReaderDoesBeforehand()
    {
        // RadiologyInfo's own prep line, and it appears nowhere else in the
        // corpus. It is also the only instruction on this page a reader can get
        // wrong the morning of, which is WI-520's test for what earns a slot.
        Assert.Contains("Keep your coffee normal", PlainOf(FmriHeading), StringComparison.Ordinal);
        Assert.Contains("do not skip it on the day", PlainOf(FmriHeading), StringComparison.Ordinal);
        Assert.Contains("caffeine normal", PlainOf(BeforeHeading), StringComparison.OrdinalIgnoreCase);
    }

    // --------------------------------------------- what belongs to the sibling

    [Fact]
    public void ThePageDoesNotStateAnyOfTheAfterTreatmentMaterialItRoutesFor()
    {
        // The scope line, and it is the item's biggest call. /tests/follow-up-scans
        // (WI-521) owns pseudoprogression, radiation necrosis, the twelve-week
        // rule and the honest "they help, and they do not settle it" — reached
        // from two sources that disagree and printed as a disagreement. A second
        // copy here is a second copy to keep in step, on the claim where drift is
        // most dangerous.
        // NO REDACTION LIST. The first version carried one, and /review proved
        // the regex matched nothing on the page even with the redactions
        // removed — so the allowlist exempted nothing and its comment documented
        // a decision the guard never made. §12.8 (WI-524): assert the match
        // count before trusting a filter; §12.8 (WI-527): prove a redaction with
        // a canary or do not write one.
        //
        // The page's one legitimate mention of the subject now uses ACS's own
        // hedge ("more likely to be tumor or scar tissue") and immediately
        // routes, so it is outside every branch below rather than exempted.
        var branches = new (string Name, string Pattern, string Canary)[]
        {
            ("pseudoprogression", @"(?i)\bpseudo-?progression\b",
                "Perfusion helps tell pseudoprogression from real growth."),
            ("radiation necrosis", @"(?i)\bradiation necrosis\b",
                "Radiation necrosis can look the same on a scan."),
            // /review's beat, and the reason a vocabulary ban was not enough:
            // the sibling's material can be restated with none of its nouns.
            ("treatment-versus-growth",
                @"(?i)\b(?:treatment|radiation|swelling|repair|healing|brighter)\b[^.]{0,70}"
                + @"\b(?:rather than|not|instead of|and not|but not)\b[^.]{0,40}"
                + @"\b(?:growth|growing|progression|the tumor|getting bigger)\b",
                "Radiation can make an area look brighter for months afterward, and that is "
                + "repair rather than the tumor getting bigger."),
            ("the post-radiation window",
                @"(?i)\b(?:twelve weeks|three months|first few months)\b[^.]{0,70}"
                + @"\b(?:radiation|treatment|chemoradiation)\b"
                + @"|(?i)\b(?:radiation|treatment)\b[^.]{0,40}"
                + @"\b(?:twelve weeks|three months|first few months)\b",
                "In the first three months after radiation a brighter area is often the "
                + "treatment, not the tumor."),
            ("the response rules", @"(?i)\bRANO\b|(?i)\bstable\b[^.]{0,40}\breport\b",
                "RANO is the set of rules behind the word on your report."),
            ("the confirmation scan",
                @"(?i)\brepeat scan\b[^.]{0,60}\b(?:weeks|settle|settles|decide|decides|confirm)\b",
                "A repeat scan a few weeks later is what settles it."),
            // THE BLOCKER /review FOUND, banned as its own branch. ACS says,
            // verbatim, "Tumor usually shows up on a PET scan, while scar tissue
            // does not" — so this is not a sourcing failure, it is a §12.10 one:
            // the sibling that owns the question refuses the binary, and a
            // correctly-cited sentence can still be the wrong STRENGTH for the
            // page it lands on.
            ("the tumor-versus-scar binary",
                @"(?i)\b(?:tumou?r)\b[^.]{0,45}\bshows? up\b[^.]{0,60}\bscar\b"
                + @"|(?i)\bscar tissue\b[^.]{0,30}\b(?:does not|doesn't|will not|won't)\b"
                + @"|(?i)\btells? (?:you |them )?(?:apart|which)\b[^.]{0,40}\bscar\b",
                "Tumor usually shows up on a PET. Scar tissue does not."),
        };

        foreach (var (name, pattern, canary) in branches)
        {
            var regex = new Regex(pattern);
            Assert.Matches(regex, canary);
            Assert.False(regex.IsMatch(Plain),
                $"this page states '{name}', which /tests/follow-up-scans owns and answers more "
                + $"carefully. The canary for this branch is: {canary}");
        }

        // ACS's own hedge, kept word for word. Dropping it is how the binary
        // gets back in without any of the branches above firing.
        Assert.Contains("more likely to be tumor or scar tissue",
            PlainOf(PetHeading), StringComparison.Ordinal);

        // And the routes themselves, because excluding material is only half the
        // decision: the questions it answered still have to go somewhere
        // (§12.8, WI-528).
        Assert.Contains("/tests/follow-up-scans#looks-worse",
            PlainOf(PerfusionHeading), StringComparison.Ordinal);
        Assert.Contains("/tests/follow-up-scans", PlainOf(PetHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheMachineAndTheDyeAreRoutedToTheMriPageRatherThanExplainedAgain()
    {
        // Every scan on this page happens in the machine /tests/mri owns. The
        // page says so once and links, and the claustrophobia material in
        // particular is not restated — WI-506 pinned a rule there ("ask what
        // your centre has", never "ask for an open MRI") that a second copy
        // would be free to lose.
        Assert.Contains("/tests/mri", PlainOf(WhatHeading), StringComparison.Ordinal);
        Assert.Contains("this page does not repeat it", PlainOf(WhatHeading), StringComparison.Ordinal);

        // The open-scanner branch is NOT adjacency-bound. /review beat the first
        // version with "ask whether the scanner they use is more open than the
        // one you were in before" — which is exactly the advice /tests/mri is
        // gated against, because open scanners are lower field strength and are
        // not equivalent for tumor imaging (§12.8, WI-506). The device list is
        // wider for the same reason.
        var owned = new Regex(
            @"(?i)\bopen\b[^.]{0,25}\b(?:mri|scanner|magnet|machine)\b"
            + @"|(?i)\b(?:wider|more open|less enclosed|upright)\b[^.]{0,25}"
            + @"\b(?:scanner|machine|mri|magnet)\b"
            // BOTH DIRECTIONS. /review's beat put the noun first — "ask whether
            // the scanner they use is more open than the one you were in
            // before" — and a one-way window could not see it. Comparatives
            // only on this side, so "open at both ends" style description of a
            // machine does not trip it.
            + @"|(?i)\b(?:mri|scanner|magnet|machine)\b[^.]{0,45}"
            + @"\b(?:more open|wider|less enclosed|upright)\b"
            + @"|(?i)\bclaustrophob"
            + @"|(?i)\bgadolinium\b|(?i)\bdevice card\b|(?i)\bearplugs?\b"
            + @"|(?i)\b(?:pacemaker|cochlear implant|defibrillator|nerve stimulator|"
            + @"medicine pump|aneurysm clip|metal in your eye)\b");

        Assert.Matches(owned, "Ask about an open MRI if you are claustrophobic.");
        Assert.Matches(owned, "Bring your device card on the day.");
        Assert.Matches(owned,
            "Ask whether the scanner they use is more open than the one you were in before.");
        Assert.Matches(owned, "Tell them if you have a nerve stimulator fitted.");

        // The page names the card once, inside the sentence that ROUTES to the
        // page that explains it. Redacted rather than exempted by a pattern
        // escape, because an escape is a door and a named sentence is not
        // (§12.8, WI-526).
        Assert.DoesNotMatch(owned, Redact(Plain, [
            "[Your MRI scan](/tests/mri) covers the device card, and bringing it solves most of it",
        ]));
    }

    // ----------------------------------------------------- numbers and claims

    [Fact]
    public void NoPrognosisNoShareOfPeopleAndNoFrequencyAnywhereOnThePage()
    {
        // §12.2 item 5 and §12.4. None of this page's sources gives a frequency
        // it could publish honestly, and a page that supplies one anyway is
        // WI-521's defect. The guard looks FORWARD from the count, because
        // English puts the population after it (§12.8, WI-521), and allows a
        // word gap, because `most (people|patients)` misses "most brain tumor
        // patients" (§12.8, WI-527).
        var share = new Regex(
            $@"(?i)\b{CountWord}\s*(?:%|percent)\b"
            + $@"|(?i)\b{CountWord}\s+(?:in|out of)\s+(?:every\s+)?(?:\w+\s+){{0,2}}{CountWord}\b"
            + $@"|(?i)\b(?:a|about|roughly|around|nearly|almost)?\s*{Fraction}\s+(?:of\s+)?(?:the\s+)?"
            + @"(?:\w+\s+){0,2}(?:people|patients|adults|scans?|cases|centers?|centres?|hospitals?)\b"
            // The bare-share branch, and it matters more here than on the CT
            // page because this page's availability ruling depends on no
            // frequency being publishable. /review beat the first version with
            // "Only a handful of hospitals have one" and "Most centers do not
            // have it" — neither has a numeral in it, and the old branch
            // demanded a survival verb after the population as well.
            // Populations and places only. "Most of the other scans here" is a
            // statement about a list, not a share of anybody, and including
            // `scans` in the noun set fails a correct page.
            + @"|(?i)\b(?:a (?:tiny |large |small |vast |good )?(?:minority|majority|handful|fraction|few)"
            + @"|the (?:vast |large |great )?majority|hardly any|almost all|nearly all|most|many)\b"
            + @"[^.]{0,35}\b(?:people|patients|adults|cases|centers?|centres?|hospitals?)\b"
            + @"|(?i)\b(?:survival|five-?year|median survival|life expectancy|prognosis)\b");

        Assert.Matches(share, "About 20 percent of people get an amino acid PET.");
        Assert.Matches(share, "Roughly a fifth of centers have one.");
        Assert.Matches(share, "Only a handful of hospitals have one.");
        Assert.Matches(share, "Most centers do not have it.");

        Assert.DoesNotMatch(share, Plain);
    }

    [Fact]
    public void TheAvailabilityOfAminoAcidPetIsAskedAboutRatherThanAsserted()
    {
        // The dossier says amino-acid PET is "concentrated in academic centres,
        // especially in the US", with no citation, and the guideline it cites
        // says nothing about availability. §12.8's WI-519 answer: where a source
        // does not support a detail the reader will still ask about, move it
        // into a question. The question asserts nothing and is more useful.
        var pet = PlainOf(PetHeading);

        Assert.Contains("Ask whether the kind your team has in mind is done where you are being treated",
            pet, StringComparison.Ordinal);

        // SHAPE, not a word list. /review beat the first version with "Not every
        // hospital has one" and "Only a few places do it" — the first carries no
        // banned adjective at all and the second uses `few`, which is not a
        // CountWord. The rule now: any sentence naming a place-noun near a
        // scarcity or coverage word is an availability claim, whether or not a
        // number is in it.
        var placeNoun = @"(?:centers?|centres?|hospitals?|sites?|places?|units?)";
        var asserted = new Regex(
            $@"(?i)\b(?:only|about|roughly|around|fewer than|more than|under|over|just)\s+"
            + $@"(?:{CountWord}|a few|a handful|some)\b[^.]{{0,40}}\b{placeNoun}\b"
            + $@"|(?i)\b(?:not every|not all|not many|few|a few|some|most|hardly any|"
            + $@"a handful of|only a)\s+(?:\w+\s+){{0,2}}{placeNoun}\b"
            + $@"|(?i)\b(?:academic|university|major|large|specialist|teaching)\s+(?:medical\s+)?"
            + $@"{placeNoun}\b"
            + $@"|(?i)\b(?:concentrated|found only|available only|restricted|widely available|"
            + $@"rare|uncommon|hard to find)\b[^.]{{0,40}}\b{placeNoun}\b"
            + $@"|(?i)\b{placeNoun}\b[^.]{{0,40}}\b(?:cannot offer|do not have|don't have|"
            + $@"can do it|offer it)\b");

        foreach (var planted in new[]
                 {
                     "There are only about fifty centers in the country that do it.",
                     "It is concentrated in academic centers.",
                     "Not every hospital has one.",
                     "Only a few places do it.",
                 })
        {
            Assert.Matches(asserted, planted);
        }

        Assert.DoesNotMatch(asserted, Plain);
    }

    [Fact]
    public void TheOnlyDurationsPublishedAreTheOnesTheSourcesCarry()
    {
        // §12.4 R1 allows an orienting duration, and every one on this page
        // comes from RadiologyInfo's PET page. The MRI add-ons deliberately
        // carry none: how many extra minutes depends entirely on how many
        // sequences were ordered, and a figure here would be read as a promise.
        var durations = Regex.Matches(
            Plain,
            $@"(?i)\b{CountWord}\s*(?:to|-|–)?\s*(?:{CountWord}\s*)?"
            + @"(?:minutes?|hours?|days?|weeks?|months?)\b")
            .Select(m => m.Value.Trim())
            .ToList();

        string[] allowed =
        [
            // RadiologyInfo: the tracer takes roughly 30 to 60 minutes to
            // collect, and the scan itself runs about 20 to 30 minutes.
            "30 to 60 minutes",
        ];

        var unexpected = durations
            .Where(d => !allowed.Any(a => d.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .ToList();

        Assert.True(unexpected.Count == 0,
            "a duration appeared that no source in this page's front matter carries:\n  "
            + string.Join("\n  ", unexpected));

        // §12.8 (WI-524): assert the match COUNT before trusting the filter, or
        // a regex that sees nothing passes a guard it never ran.
        Assert.True(durations.Count >= 1,
            "the duration guard matched nothing at all, so it proved nothing about the page");
    }

    // --------------------------------------------------------- what is absent

    [Fact]
    public void ThePageCarriesNoEscalationListAndTheCheckIsStructuralRatherThanLexical()
    {
        // Deliberate absence (§12.8, WI-520). Nothing happens to this reader
        // because of an extra MRI sequence, and no reachable source supports a
        // triage rule for one. On a page like this an invented tier list is the
        // worst available outcome (§12.8, WI-526): the corpus already sorts
        // every symptom it would carry, and a reader routed here instead of to
        // the page with the number has been routed away from the answer.
        CuratedPage.AssertNoEscalationList(Page, Slug);
    }

    [Fact]
    public void TheWordRadiologistFirstAppearsInTheSectionAboutWhoReadsTheScan()
    {
        // A GLOSSARY TOOLTIP FIRES ON THE FIRST OCCURRENCE, so where a word
        // first appears decides where its definition lands. A draft introduced
        // "radiologist" inside the PET section's breastfeeding paragraph, and
        // the rendered page put the definition of a radiologist in the middle of
        // advice about pumping milk — while slot 9, the section that exists to
        // explain who reads the scan, got no tooltip at all.
        //
        // Nothing in the suite could see it and no gate can: the markdown
        // contains neither the tooltip nor its position. Found by reading the
        // rendered page, which is the eighteenth consecutive item where that is
        // what caught it.
        // WHOLE-WORD, because that is how `GlossaryMarker` matches. A plural
        // possessive — "the radiologists' own guidance" — contains the term as a
        // substring and does NOT fire the tooltip, so a bare IndexOf models the
        // wrong thing and fails a correct page (§12.8, WI-512: grep the way the
        // matcher greps).
        var match = Regex.Match(Plain, @"(?i)\bradiologist\b");
        Assert.True(match.Success, "the page no longer says 'radiologist' as a whole word");
        var first = match.Index;

        var headingAt = Plain.IndexOf(ResultsHeading, StringComparison.Ordinal);
        Assert.True(headingAt >= 0,
            "slot 9's heading could not be found, so this guard has nothing to pin against");

        Assert.True(first > headingAt,
            "'radiologist' now appears before slot 9, so the glossary tooltip fires there instead "
            + "and the section about who reads the scan renders without one.");
    }

    [Fact]
    public void ThePageCarriesNoCaregiverSection()
    {
        // §12.8: a tests page carries one only where the test has an aftercare
        // tail. Nobody is discharged into anyone's care after an extra sequence.
        // /tests/biopsy is the tests page that does carry one, and reading it
        // here is what makes this absence a decision rather than an oversight.
        Assert.DoesNotContain("[CAREGIVER]", Page, StringComparison.Ordinal);
        Assert.DoesNotContain("For the person caring for someone", Plain, StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", CuratedPage.Read("tests", "biopsy.md"), StringComparison.Ordinal);
    }

    // --------------------------------------------------------------- sources

    [Fact]
    public void TheBannedSourcesAreAbsentAndTheClaimsTheyCarriedAreRederivedRatherThanDropped()
    {
        var urls = CitedUrls();

        // §12.14: banning a citation is not fixing a claim, so each ban sits
        // next to the sentence that replaced it.
        foreach (var dead in new[]
                 {
                     "researchgate.net", "academic.oup.com", "ajnr.org", "sciencedirect.com",
                     "journals.lww.com", "mdpi.com", "ascopubs.org", "redjournal.org",
                     "surgicalneurologyint.com", "medscape.com", "mayoclinic.org",
                     "hopkinsmedicine.org", "drugs.com", "medlineplus.gov/druginfo",
                 })
        {
            Assert.DoesNotContain(dead, urls, StringComparison.OrdinalIgnoreCase);
        }

        // §12.1's PDQ rule needs its own pattern; a domain ban cannot express it
        // (§12.8, WI-528).
        Assert.DoesNotMatch(@"cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);

        // The replacements, each proved present rather than assumed:
        // neurovascular uncoupling, recovered open after researchgate.
        Assert.Contains("PMC5669348", urls, StringComparison.Ordinal);
        // The DTI tract patterns, recovered open after journals.lww.com.
        Assert.Contains("PMC8050646", urls, StringComparison.Ordinal);
        // The fMRI experience and the limit, at patient level.
        Assert.Contains("radiologyinfo.org/en/info/fmribrain", urls, StringComparison.Ordinal);
        // The backbone that carries all five scans in one patient-level page.
        Assert.Contains(
            "cancer.org/cancer/types/brain-spinal-cord-tumors-adults/detection-diagnosis-staging",
            urls, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDroppedScansAreNotMentionedBecauseNothingReachableDescribesThem()
    {
        // §3.5's SPECT rests only on NCI patient PDQ, and every DOTATATE source
        // in the pack is gated. §3.3's 2-hydroxyglutarate material rests on a
        // dead academic.oup.com meta-analysis and needs spectral editing the
        // dossier itself calls unavailable in most places — and
        // /tests/molecular-markers (WI-509) owns IDH, from tissue.
        //
        // A scan this page cannot describe honestly is a scan it does not
        // mention. Asserted rather than believed, because the easy edit is to
        // "just add a line about PET for meningioma".
        // `\b` and case-SENSITIVE for the acronym: a bare substring check for
        // "SPECT" fires on "spectroscopy", which is one of the five scans this
        // page exists to explain. The guard written to prove an absence failed a
        // correct page on its first run, which is §12.8's WI-511 stemming
        // lesson in a new coat.
        (string Pattern, string Canary)[] dropped =
        [
            (@"\bSPECT\b", "A SPECT scan is another way of doing something similar."),
            (@"(?i)\bDOTATATE\b", "For a meningioma a DOTATATE tracer may be used."),
            (@"(?i)\bsomatostatin\b", "Meningiomas carry somatostatin receptors."),
            (@"(?i)\b2-?hydroxyglutarate\b", "It can pick up 2-hydroxyglutarate."),
            // `2-HG` is the commoner written form and matched neither the
            // hyphen-less acronym nor the spelled-out name (§12.8, WI-527).
            (@"(?i)\b2-?HG\b", "Spectroscopy can sometimes pick up 2-HG."),
        ];

        foreach (var (pattern, canary) in dropped)
        {
            var regex = new Regex(pattern);
            Assert.Matches(regex, canary);
            Assert.DoesNotMatch(regex, Plain);
        }

        // AND THE LIMIT, STATED (§12.8, WI-519): a vocabulary ban is a filter,
        // not an ownership proof. /review wrote "A few centers use a scan that
        // looks for a chemical made only by tumors with an IDH change" — 2-HG
        // spectroscopy described with none of the five strings above. Nothing
        // can mechanically decide whether a page has described a technique it
        // never names; what the availability guard above DOES catch is the
        // "a few centers" half of that sentence, which is why the two guards are
        // worth having together.

        // And the route for the question 2HG would have answered.
        Assert.Contains("/tests/molecular-markers", Plain, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryCitedUrlCarriesATitleAndAnAccessedDate()
    {
        var entries = Regex.Matches(
            CuratedPage.FrontMatter(Page),
            @"(?m)^  - url: (\S+)\r?\n\s+title: ""([^""]+)""\r?\n\s+accessed: (\d{4}-\d{2}-\d{2})");

        Assert.True(entries.Count == 8,
            $"expected eight cited sources with a title and an accessed date, found {entries.Count}");
    }

    // ------------------------------------------------- shared corpus hygiene

    [Fact]
    public void NoBritishSpellingsOrIdioms()
    {
        var text = Plain;
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            text = text.Replace(exemption, " ", StringComparison.OrdinalIgnoreCase);
        }

        var found = CuratedPage.BritishForms
            .Where(f => text.Contains(f, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(found.Count == 0,
            "British forms on a page written for a reader in the United States:\n  "
            + string.Join("\n  ", found));
    }

    [Fact]
    public void NoCharacterisationOfAResultAndNoMinimisationOfTheScan()
    {
        var found = CuratedPage.Characterisations
            .Where(p => Plain.Contains(p, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(found.Count == 0,
            "a banned characterisation reached the page:\n  " + string.Join("\n  ", found));

        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Page), Slug);
    }

    [Fact]
    public void ThePageDoesNotRestateAnySiblingsProse() =>
        CuratedPage.AssertDoesNotRestateTheCorpus(
            Page, Slug,
            // Slot 9's prescribed opening: §12.8 says the honest answer to "when
            // will I know" is that timings are local, so every library page
            // opens that section the same way on purpose.
            "how long it takes depends on where you are so ask before you leave",
            // The limit of any scan, load-bearing on four pages. §12.10 says two
            // pages must not state one safety claim at two strengths, so where
            // it is load-bearing on both, identical words are the right answer.
            "it is only a piece of the tumor itself looked at in a lab that can do that "
            + "only a piece of the tumor itself looked at in a lab can say for certain "
            + "only a piece of the tumor itself looked at in a lab can do that");
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class PlanningScansPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/planning-scans";

    private readonly WebApplicationFactory<Program> _factory;

    public PlanningScansPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("The extra scans before treatment", html, StringComparison.Ordinal);
        // §12.8 (WI-529): an authoring marker on a directive line stops the
        // directive composing and the literal renders inside a paragraph.
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\[[A-Z][A-Z-]+\]", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // §12.8 (WI-519): a new library page nothing links to is half shipped.
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/tests/mri", "/tests/ct-scan", "/tests/follow-up-scans",
                     "/treatments/craniotomy", "/treatments/awake-craniotomy",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/tests/mri", "/tests/ct-scan", "/tests/biopsy", "/tests/follow-up-scans",
            "/treatments/awake-craniotomy", "/treatments/craniotomy",
            "/treatments/radiation-therapy", "/glossary");

    [Fact]
    public async Task TheDeepLinksIntoTheSiblingPagesLandOnRealAnchors() =>
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheFunctionalMriTooltipIsSuppressedHereAndStillFiresWhereThePageDoesNotDefineIt()
    {
        // §12.8 (WI-512): assert the WORD is in this page's prose before
        // asserting its tooltip is not, or the suppression is a no-op and the
        // DoesNotContain passes for the wrong reason.
        var body = CuratedPage.ReaderText(CuratedPage.Read("tests", "planning-scans.md"));
        Assert.Contains("functional MRI", body, StringComparison.Ordinal);

        var client = _factory.CreateClient();
        Assert.DoesNotContain("def-functional-mri", await client.GetStringAsync(Url));

        // §12.8 (WI-519): an entry defined only on the page that suppresses it is
        // decoration. This one is live on the page that mentions the scan in
        // three sentences and explains it nowhere.
        Assert.Contains("def-functional-mri",
            await client.GetStringAsync("/treatments/awake-craniotomy"));
    }

    [Fact]
    public void TheDoorsOnTheSiblingPagesAreAppendedSentencesRatherThanReplacements()
    {
        // §12.8 (WI-526): a door has to sit NEXT TO the sentence it was appended
        // to, or replacing that sentence outright leaves the guard green.
        (string File, string Existing, string Door)[] doors =
        [
            ("tests/mri.md",
                "Before surgery or radiation, your team needs a very exact",
                "[The extra scans before treatment](/tests/planning-scans)"),
            ("treatments/awake-craniotomy.md",
                "It does not replace the test in the room.",
                "(/tests/planning-scans#fmri)"),
            ("treatments/craniotomy.md",
                "why brain mapping and monitoring are used",
                "[The extra scans before treatment](/tests/planning-scans)"),
            ("tests/ct-scan.md",
                "Sometimes your team adds scans that",
                "[The extra scans before treatment](/tests/planning-scans)"),
        ];

        foreach (var (file, existing, door) in doors)
        {
            var text = CuratedPage.Flatten(CuratedPage.Read(file.Split('/')));
            var at = text.IndexOf(existing, StringComparison.Ordinal);
            var doorAt = text.IndexOf(door, StringComparison.Ordinal);

            Assert.True(at >= 0, $"{file} no longer contains the sentence the door was appended to");
            Assert.True(doorAt >= 0, $"{file} has lost its door to {Url}");
            Assert.True(Math.Abs(doorAt - at) < 400,
                $"{file}'s door has drifted {Math.Abs(doorAt - at)} characters from the sentence it "
                + "was appended to, so replacing that sentence would not break this test");
        }
    }
}
