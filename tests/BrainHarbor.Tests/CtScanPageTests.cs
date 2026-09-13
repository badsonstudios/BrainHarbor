using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-530, first of two pages: <c>/tests/ct-scan</c>. A §12.8 library page, and
/// the only one in the corpus that is mostly RETROSPECTIVE — most readers meet
/// it after the scan, not before, because the CT is where somebody first said
/// there was something there.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * TELLING THEM THEY GOT THE WRONG TEST. ACS says outright that a CT
///     "aren't quite as good as MRI scans for looking at brain or spinal cord
///     tumors". A page that prints that without the other half — that a CT is
///     what gets used "if a scan is needed right away", and that in emergencies
///     it finds bleeding "quickly enough to help save lives" — tells somebody
///     their emergency room chose the weaker scan.
///   * A RADIATION DOSE. The dossier gives "about 4 mSv, roughly 16 months of
///     natural background radiation" sourced to WebMD, and its own §12 says to
///     range it or omit it. RadiologyInfo says "The radiation dose for this
///     procedure varies." No figure is published, in any shape.
///   * THE CANCER SENTENCE WITHOUT ITS OTHER HALF. RadiologyInfo's risk line is
///     "There is always a slight chance of cancer from excessive exposure to
///     radiation. However, the benefit of an accurate diagnosis far outweighs
///     the risk involved with CT scanning." Printing the first sentence alone,
///     to a reader who has just had one, is the §12.12 failure in its worst
///     direction — and /tumors/meningioma already tells its reader "your head
///     scan did not cause this".
///   * SAYING THE SCAN CAN NAME IT. §12.8's WI-506 rule. Only tissue can.
///   * CONTRADICTING /tests/mri ON THE DYE. Two different liquids with two
///     different sets of rules, and the breastfeeding answer is not the same
///     one. The MRI page's answer is read here rather than assumed.
///
/// Every guard starts from §12.8's WI-527, WI-528 and WI-529 lessons rather
/// than rediscovering them: <c>CountWord</c> knows the teens, quantifiers allow
/// a word gap, numeric bans are threshold-shaped, phrase guards run on
/// <c>Plain</c> so the description is in scope, section order asserts PRESENCE
/// before position, every claim-shaped guard has a canary, and every regex that
/// touches a line break uses <c>\r?\n</c> — never a bare <c>\n</c> and never
/// <c>\s*</c> standing in for a blank line.
/// </summary>
public sealed class CtScanPageContentTests
{
    private const string Slug = "tests/ct-scan";

    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is a CT scan?";
    private const string WhyHeading = "Why a CT and not an MRI?";
    private const string StepsHeading = "What happens, step by step";
    private const string LongHeading = "How long does it take?";
    private const string FeelHeading = "What does it feel like?";
    private const string HardHeading = "The hard part is usually not the scan";
    private const string DyeHeading = "The dye they may put in your arm";
    private const string RadiationHeading = "What about the radiation?";
    private const string BeforeHeading = "What to tell them before the day";
    private const string AnotherHeading = "Why am I having another scan?";
    private const string ResultsHeading = "Who reads it, and how do I get the result?";
    private const string QuestionsHeading = "What to ask your team";
    private const string NextHeading = "Where to go next";

    private static string Page => CuratedPage.Read("tests", "ct-scan.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// Title and description. <c>ReaderText</c> strips the front matter, and
    /// <c>ContentPage.cshtml</c> renders the description as the first paragraph
    /// under the heading, so a body-scoped guard is blind to the first thing a
    /// reader meets (§12.8, WI-524 and WI-528).
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
    /// Everything, emphasis markers removed. A bolded word defeats a phrase
    /// guard structurally and no vocabulary fixes that (§12.8, WI-526).
    /// </summary>
    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private static string Sibling(params string[] path) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(path))), @"[*_]", "");

    /// <summary>The page's own "## " headings, in order, anchors stripped.</summary>
    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    /// <summary>
    /// The URL VALUES from the front matter, without the comments around them.
    /// A ban run over the whole front matter fires on this page's own rulings,
    /// which quote the banned URL in order to explain the ban — a guard red on a
    /// correct page is worse than no guard (§12.8).
    /// </summary>
    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    /// <summary>
    /// Removes an allowed phrase instead of exempting the whole sentence that
    /// contains it (§12.8, WI-527: a substring allowlist is a free ride for
    /// everything else in the sentence).
    /// </summary>
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
        // §12.8 (WI-528): `List.IndexOf` returns -1 and -1 is less than
        // everything, so an order test built from `>` stays green when a whole
        // section is deleted. Presence first, then position.
        //
        // §12.8 (WI-510): read the STANDARD for the slot list, not the previous
        // page. A CT has a day and a procedure, so slots 2, 3, 5 and 7 are all
        // here. WI-507, WI-508 and WI-509 each correctly dropped some of them
        // and copying the last page written is how a template quietly shrinks.
        var headings = Headings();

        string[] required =
        [
            ShortHeading, WhatHeading, WhyHeading, StepsHeading, LongHeading, FeelHeading,
            HardHeading, DyeHeading, RadiationHeading, BeforeHeading, AnotherHeading,
            ResultsHeading, QuestionsHeading, NextHeading,
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
                $"'{required[i]}' has moved above '{required[i - 1]}'. §12.8's slot order is fixed; "
                + "which of the middle three a page carries is not.");
        }
    }

    [Fact]
    public void EverySectionCarriesAnExplicitAnchorSoRewordingAHeadingCannotBreakAnInboundLink()
    {
        // §12.8 (WI-508, WI-509): Markdig derives an id from heading TEXT, so a
        // reworded heading silently breaks every inbound deep link and the
        // reader lands at the top of a long page with no sign anything is wrong.
        var without = Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .Where(h => !h.EndsWith('}'))
            .ToList();

        Assert.True(without.Count == 0,
            "these headings have no explicit {#anchor}, so their ids are derived from the wording:\n  "
            + string.Join("\n  ", without));
    }

    // --------------------------------------------- the fast scan was not wrong

    [Fact]
    public void TheComparisonWithMriNeverReadsAsTheReaderHavingHadTheWrongTest()
    {
        // The page's central reassurance, and the reason it exists as a
        // retrospective page at all. ACS says a CT "aren't quite as good as MRI
        // scans", which this page prints because it is true — and the reader
        // most likely to read it has already had one, in an emergency, for a
        // reason.
        var why = PlainOf(WhyHeading);

        // Both halves, in the section that carries the comparison.
        Assert.Matches(@"not as good as an MRI", why);
        Assert.Matches(@"needed right away", why);
        Assert.Matches(@"the fast scan was the right scan", why);

        // And the claim-shaped ban, page-wide. A sentence saying the scan was a
        // mistake, or that an MRI should have been done instead, is the defect
        // whatever section it sits in (§12.8, WI-524: a guard scoped to the
        // section named for the defect is a guard with a door next to it).
        // Word gaps everywhere. /review beat the first version four ways, all of
        // them one word wide: "the wrong KIND of scan", "should have SENT you",
        // "they picked the wrong ONE", and a should-have frame built on
        // "the BETTER first test" rather than on a negative adjective at all.
        // §12.8 (WI-527): adjacency is a one-word door.
        var wrong = new Regex(
            @"(?i)\b(?:the|a|an|that)\s+(?:\w+\s+){0,3}?"
            + @"(?:wrong|weaker|inferior|lesser|second-best|worse)\s+(?:\w+\s+){0,2}"
            + @"(?:scan|test|choice|option|picture|kind|one)\b"
            + @"|(?i)\b(?:should|would|ought to)\s+have\s+(?:\w+\s+){0,2}"
            + @"(?:had|been|done|ordered|got|sent|gone|given)\b[^.]{0,60}\bMRI\b"
            + @"|(?i)\bMRI\b[^.]{0,50}\b(?:should|would|ought to)\s+have\s+been\b"
            + @"|(?i)\b(?:should|would|ought to)\s+have\b[^.]{0,50}"
            + @"\b(?:better|right|proper|correct)\s+(?:\w+\s+){0,2}(?:scan|test|picture)\b"
            + @"|(?i)\b(?:they|somebody|someone|your team|the hospital)\s+"
            + @"(?:got|chose|picked|did)\s+(?:\w+\s+){0,2}(?:it|this|that|one)\s+wrong\b");

        // Canaries, every one of them a sentence /review walked through the
        // first version. A claim-shaped guard nobody has proved can fire is a
        // guard nobody has run (§12.8, WI-523).
        Assert.Matches(wrong, "They should have done an MRI instead.");
        Assert.Matches(wrong, "The emergency room chose the weaker scan.");
        Assert.Matches(wrong, "If yours was a CT, you had the wrong kind of scan.");
        Assert.Matches(wrong, "The emergency room should have sent you for an MRI.");
        Assert.Matches(wrong, "They picked the wrong one.");
        Assert.Matches(wrong, "An MRI would have been the better first test.");

        // The page's own sentence contains "Nobody chose the weaker test", which
        // is the refusal rather than the claim, so it is redacted rather than
        // exempted whole (§12.8, WI-527).
        var text = Redact(Plain, ["Nobody chose the weaker test"]);
        Assert.DoesNotMatch(wrong, text);
    }

    [Fact]
    public void ThePageSaysWhatAScanCannotDoAndRoutesToTheThingThatCan()
    {
        // §12.8's WI-506 rule: a library page must say what it cannot do. A
        // reader who does not know that only tissue names a tumor reads the wait
        // for pathology as their team stalling.
        var why = PlainOf(WhyHeading);

        Assert.Contains("Only a piece of the tumor itself, looked at in a lab", why, StringComparison.Ordinal);
        Assert.Contains("/tests/biopsy", why, StringComparison.Ordinal);

        // The short version carries it too, because readers consume 20 to 28% of
        // a page (§12.3) and this is the sentence that stops the rest being
        // misread. A section-scoped position test cannot see the summary
        // (§12.8, WI-520).
        var summary = PlainOf(ShortHeading);
        // "cannot say for certain", not "cannot tell you". /review caught the
        // summary stating this claim at a HARDER strength than the body, which
        // says "Pictures narrow the list, and sometimes they narrow it a long
        // way" — and the summary is what 20 to 28% of readers get. ACS's own
        // verbatim is "can only be CONFIRMED by removing some of the tumor
        // tissue", which is the softer of the two.
        Assert.Contains("It cannot say for certain what it is", summary, StringComparison.Ordinal);

        // And the deliberately shared wording with /tests/mri, which says the
        // same thing about the same limit. One claim, one wording (§12.8,
        // WI-522): if that page's sentence is softened, this assertion goes red.
        Assert.Contains("Only a piece of the tumor itself, looked at in a lab",
            Sibling("tests", "mri.md"), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------- radiation

    [Fact]
    public void NoRadiationDoseIsPublishedInAnyShape()
    {
        // The dossier's §2.4 gives "about 4 mSv, roughly 16 months of natural
        // background radiation", sourced to WebMD, under its own note saying to
        // range it or omit it. §12.4 R2's reasoning applies and RadiologyInfo
        // says why in as many words: "The radiation dose for this procedure
        // varies."
        //
        // Threshold-shaped rather than unit-shaped (§12.8, WI-527): taking the
        // unit out of a dose does not stop it being one, and a number written as
        // a word is still a number (§12.8, WI-511).
        // Every unit is PLURALISED and `sievert` is in the list on its own.
        // /review beat the first version with "about 2 millisieverts" — `\b`
        // after `millisievert` sits between two word characters, so the
        // singular-only entry could not match the commonest written form — and
        // with "four thousandths of a sievert", a unit the list did not hold at
        // all. §12.8 (WI-527): an inflection is a door.
        var dose = new Regex(
            // A count anywhere in the same sentence as a dose unit, not adjacent
            // to it: /review's "around four thousandths of a sievert" puts three
            // words between them, and the page carries no radiation unit at all,
            // so the wide window costs nothing.
            $@"(?i)\b{CountWord}\b[^.]{{0,30}}"
            + @"\b(?:mSv|milli-?sieverts?|sieverts?|rems?|grays?|Gy)\b"
            + $@"|(?i)\b{CountWord}[\s\w]{{0,20}}\b(?:months?|years?|weeks?|days?)\b[^.]{{0,30}}"
            + @"\bbackground\s+radiation\b"
            + $@"|(?i)\bbackground\s+radiation\b[^.]{{0,40}}\b{CountWord}\b"
            // The comparison branch, widened from `(equivalent|equal|same) to`.
            // "like having about a hundred chest x-rays" is the single commonest
            // way a CT dose is published for patients and the first version
            // could not see it.
            + $@"|(?i)\b(?:equivalent|equal|same as|comparable|like|no more than|worth of)\b"
            + $@"[^.]{{0,40}}\b{CountWord}\b[^.]{{0,25}}"
            + @"\b(?:x-?rays?|scans?|months?|years?|flights?)\b"
            + $@"|(?i)\b{CountWord}\s+times\b[^.]{{0,30}}\b(?:x-?ray|radiation|dose)\b"
            + $@"|(?i)\b(?:dose|exposure)\b[^.]{{0,30}}\b{CountWord}\s*(?:%|percent)");

        Assert.Matches(dose, "A head CT with dye is about 4 mSv.");
        Assert.Matches(dose, "That is roughly sixteen months of natural background radiation.");
        Assert.Matches(dose, "It is about two hundred times an ordinary x-ray.");
        Assert.Matches(dose, "A head CT is about 2 millisieverts.");
        Assert.Matches(dose, "A head CT is like having about a hundred chest x-rays.");
        Assert.Matches(dose, "It gives a dose of around four thousandths of a sievert.");

        Assert.DoesNotMatch(dose, Plain);

        // And the reason is on the page, not only in the front matter, because a
        // page that simply omits a figure leaves the reader to supply their own
        // and the one they supply is usually worse than the truth (§12.8,
        // WI-521).
        Assert.Contains("varies", PlainOf(RadiationHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRadiationSectionAnswersInBothDirectionsAndDoesNotEndOnTheFear()
    {
        // §12.8 (WI-506): answer the frightening question in both directions.
        // Dropping either half is a different kind of dishonesty, and here the
        // fear half is the one a reader arrives already carrying.
        var section = PlainOf(RadiationHeading);

        // The fear, stated.
        Assert.Contains("slight chance of cancer", section, StringComparison.Ordinal);
        // The other half, from the same source and the same paragraph.
        Assert.Contains("far outweighs", section, StringComparison.Ordinal);
        // The reassurance nobody is told, and the only one that is about the
        // scan the reader has ALREADY had.
        Assert.Contains("no radiation stays in your body", section, StringComparison.OrdinalIgnoreCase);

        // Position, not presence (§12.8, WI-510): the fear must not be the last
        // thing in the section, and the pin is END-anchored, because /review
        // appended a sentence past a correctly-pinned one on WI-521.
        var sentences = CuratedPage.SentencesOf(section);
        var last = sentences[^1];
        Assert.DoesNotMatch(@"(?i)\b(?:cancer|risk|harm|damage)\b\.?\s*$", last);
        Assert.Matches(@"(?i)\b(?:rude|ordinary)\b\.?\s*$", last);

        // The order matters too: the fear is stated before it is answered, not
        // after, or the answer reads as a hedge on a reassurance.
        Assert.True(
            section.IndexOf("slight chance of cancer", StringComparison.Ordinal)
            < section.IndexOf("far outweighs", StringComparison.Ordinal),
            "the reassurance now comes before the thing it is reassuring about");
    }

    [Fact]
    public void TheRadiationSectionDoesNotContradictTheMeningiomaPageAboutWhatCausedThis()
    {
        // /tumors/meningioma tells its reader, in bold, that childhood
        // radiotherapy is a real link and "It is not the same thing as a dental
        // x-ray or a CT scan" — "your head scan did not cause this". A CT page
        // that says or implies the opposite is one claim at two strengths on the
        // two pages a reader meets in the same week (§12.10).
        Assert.Contains("your head scan did not cause this",
            Sibling("tumors", "meningioma.md"), StringComparison.Ordinal);

        // All four inflections of the verb, `a|an` in the determiner list, and a
        // branch for the reason-shaped phrasing. /review beat the first version
        // with "A CT scan causes a small number of tumors" (only `caused` and
        // `causing` were listed, and `a` was not a determiner) and with "Your
        // scan may be why this is here" (the verb list held the single literal
        // "is why you have").
        var caused = new Regex(
            @"(?i)\b(?:this|the|your|that|a|an|one)\s+(?:\w+\s+){0,2}?(?:ct|scan|x-?ray)s?\b"
            + @"[^.]{0,40}\b(?:caus(?:e|es|ed|ing)|gave you|brought on|led to|"
            + @"(?:is|be|was|are)\s+(?:the |a )?(?:reason|why)|set (?:this|it) off)\b"
            + @"|(?i)\bcaus(?:e|es|ed|ing)\s+(?:by|your)\b[^.]{0,30}"
            + @"\b(?:ct scan|head ct|the scan|an x-?ray)\b");

        Assert.Matches(caused, "It is possible the scan caused your tumor.");
        Assert.Matches(caused, "A CT scan causes a small number of tumors over a lifetime.");
        Assert.Matches(caused, "Your scan may be why this is here.");
        Assert.DoesNotMatch(caused, Plain);
    }

    // ------------------------------------------------------------------- dye

    [Fact]
    public void TheCtDyeIsNamedAsADifferentLiquidFromTheMriOneAndTheSiblingAgrees()
    {
        var dye = PlainOf(DyeHeading);

        Assert.Contains("not the same dye as the one used for an MRI", dye, StringComparison.Ordinal);
        Assert.Contains("iodine", dye, StringComparison.Ordinal);

        // The sibling is READ rather than believed (§12.8, WI-514): if
        // /tests/mri's answer changes, this goes red here.
        var mri = Sibling("tests", "mri.md");
        Assert.Contains("Is it the same as the dye for a CT scan? No. It has no iodine in it",
            mri, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBreastfeedingAnswerPrintsTheDisagreementAndDoesNotOverwriteTheMriPagesAnswer()
    {
        // §12.8 (WI-511): where two sources disagree, print the disagreement and
        // name whose number it is. RadiologyInfo carries both sides in one
        // paragraph — the manufacturers say 24 to 48 hours, the ACR says the
        // amount absorbed is extremely low — and splitting the difference with
        // an unattributed answer belonging to nobody is the defect.
        var before = PlainOf(BeforeHeading);

        Assert.Contains("companies that make the dye", before, StringComparison.Ordinal);
        Assert.Contains("American College of Radiology", before, StringComparison.Ordinal);
        Assert.Contains("Ask which one the department follows", before, StringComparison.Ordinal);

        // The scope, which is the part that stops this contradicting /tests/mri.
        // That page says doctors take it as safe to carry on feeding after the
        // MRI dye. Both are correct BECAUSE they are different dyes, and this
        // page has to say so in the same breath or it reads as a correction.
        Assert.Contains("This is about the CT dye", before, StringComparison.Ordinal);
        Assert.Contains("/tests/mri", before, StringComparison.Ordinal);
        Assert.Contains("the answer there is not the same one", before, StringComparison.Ordinal);

        Assert.Contains("safe to carry on feeding after the dye",
            Sibling("tests", "mri.md"), StringComparison.Ordinal);

        // And neither side is stated as the answer. A page that resolves a real
        // disagreement invents a third position (§12.8, WI-511).
        //
        // INVERTED, because two closed lists of phrases is not a guard (§12.8,
        // WI-525). /review beat the first version with "Pumping and throwing
        // away a day of milk is the safer choice" and "Most departments take the
        // ACR line, so carry on feeding as normal" — neither contains a banned
        // shape — and it ALSO fired on "There is no need to stop feeding",
        // which is the same resolution in the other direction. Hunting bad words
        // could express neither.
        //
        // The rule instead: every sentence on this page that mentions feeding or
        // milk must EARN its place. It attributes (names one of the two sources,
        // or the department), or it asks, or it scopes the answer to a
        // particular dye. Shapes nobody has thought of then fail by default.
        var earnsIt = new Regex(
            @"(?i)companies that make the dye|American College of Radiology"
            + @"|(?i)\bthe department\b|(?i)/tests/mri|(?i)\bthe answer there\b"
            + @"|(?i)\badvice genuinely splits\b|(?i)\bAsk\b|\?");

        // In scope: a sentence that mentions feeding AND hands the reader a
        // verdict. A bare statement of fact — the questions list's "I am
        // breastfeeding." — states nothing and owes nobody.
        var verdict = new Regex(
            @"(?i)\b(?:safe|safer|safest|fine|ok|okay|do not|don't|not to|stop|stopping|"
            + @"carry on|continue|keep|no need|as normal|can|should|must|better to|"
            + @"best to|choice)\b");

        var inScope = CuratedPage.SentencesOf(Plain)
            .Where(s => Regex.IsMatch(s, @"(?i)\b(?:breast\s?)?feed(?:ing|s)?\b|\bmilk\b"))
            .Where(s => verdict.IsMatch(s))
            .ToList();

        // §12.8 (WI-524): assert the match COUNT before iterating, or a guard
        // that sees nothing passes without ever running.
        Assert.True(inScope.Count >= 1,
            $"only {inScope.Count} sentences give a verdict about feeding, so this guard barely "
            + "ran. The breastfeeding answer has been cut down rather than corrected.");

        var unearned = inScope.Where(s => !earnsIt.IsMatch(s)).ToList();
        Assert.True(unearned.Count == 0,
            "a sentence about feeding gives the reader an answer without saying whose it is:\n  "
            + string.Join("\n  ", unearned));

        // Canaries: each of these is a resolution /review wrote, and each has to
        // fail the rule above rather than merely avoid a banned word.
        foreach (var resolution in new[]
                 {
                     "It is safe to carry on breastfeeding.",
                     "Do not breastfeed for two days.",
                     "Pumping and throwing away a day of milk is the safer choice.",
                     "There is no need to stop feeding.",
                 })
        {
            Assert.False(earnsIt.IsMatch(resolution),
                $"the earns-its-place rule would wave through: {resolution}");
        }
    }

    // ----------------------------------------------------- numbers and claims

    [Fact]
    public void NoPrognosisFigureAndNoShareOfPeopleAnywhereOnThePage()
    {
        // §12.2 item 5 and §12.4. A share does not need the word "of" and does
        // not need a percentage: "half the people who have it" is the commonest
        // phrasing there is (§12.8, WI-527). Quantifiers allow a word gap,
        // because `most (people|patients)` misses "most head CT patients".
        var share = new Regex(
            $@"(?i)\b{CountWord}\s*(?:%|percent)\b"
            + $@"|(?i)\b{CountWord}\s+(?:\w+\s+){{0,2}}(?:in|out of)\s+(?:every\s+)?"
            + $@"(?:\w+\s+){{0,2}}{CountWord}\b"
            + $@"|(?i)\b(?:a|about|roughly|around|nearly|almost)?\s*{Fraction}\s+(?:of\s+)?(?:the\s+)?"
            + @"(?:\w+\s+){0,2}(?:people|patients|adults|scans?|cases)\b"
            // The bare-share branch. §12.8 (WI-527) lists "the large majority"
            // and "a tiny minority" as shares with no numeral in them, and
            // /review beat the first version with exactly that shape plus
            // "fewer than one person in every few thousand", where the word gap
            // broke the one-in-N branch twice.
            // Populations only. "Most of the other scans here" is a statement
            // about a list, not a share of anybody, and including `scans` in the
            // noun set fails a correct page (§12.8: a rule that fails a correct
            // page is worse than no rule).
            + @"|(?i)\b(?:a (?:tiny |large |small |vast |good )?(?:minority|majority|handful|fraction|few)"
            + @"|the (?:vast |large |great )?majority|hardly any|almost all|nearly all|most)\b"
            + @"[^.]{0,35}\b(?:people|patients|adults|cases)\b"
            + @"|(?i)\b(?:survival|five-?year|median survival|life expectancy)\b");

        Assert.Matches(share, "About 30 percent of patients have a reaction.");
        Assert.Matches(share, "Roughly a fifth of the people who have one react.");
        Assert.Matches(share, "one in ten patients");
        Assert.Matches(share, "Only a tiny minority of people ever react to the dye.");
        Assert.Matches(share, "Serious reactions happen to fewer than one person in every few thousand.");

        Assert.DoesNotMatch(share, Plain);
    }

    [Fact]
    public void TheOnlyDurationsPublishedAreTheOnesTheSourcesCarry()
    {
        // §12.4 R1 lets an orienting duration through, and this page has exactly
        // one: ACS's "A CT scan usually takes less than 30 minutes." Everything
        // else a reader might treat as a promise about their own hospital stays
        // off, which is WI-520's ruling about which way a number pushes.
        var durations = Regex.Matches(
            Plain,
            $@"(?i)\b{CountWord}\s*(?:to|-|–)?\s*(?:{CountWord}\s*)?"
            + @"(?:minutes?|hours?|days?|weeks?|months?)\b")
            .Select(m => m.Value.Trim())
            .ToList();

        string[] allowed =
        [
            // ACS, verbatim: "A CT scan usually takes less than 30 minutes."
            "30 minutes",
            // RadiologyInfo's prep line: "your doctor may tell you not to eat or
            // drink anything for a few hours beforehand". "a few hours" carries
            // no count word, so it does not reach this guard at all; the one
            // that does is the dye taste, from ACS: "can last for several hours".
        ];

        var unexpected = durations
            .Where(d => !allowed.Any(a => d.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .ToList();

        Assert.True(unexpected.Count == 0,
            "a duration appeared that no source in this page's front matter carries:\n  "
            + string.Join("\n  ", unexpected));

        // Assert the match COUNT before trusting the filter (§12.8, WI-524): a
        // regex that sees nothing passes a guard it never ran.
        Assert.True(durations.Count >= 1,
            "the duration guard matched nothing at all, so it proved nothing about the page");
    }

    // --------------------------------------------------- what is NOT here

    [Fact]
    public void ThePageCarriesNoEscalationListAndTheCheckIsStructuralRatherThanLexical()
    {
        // Deliberate absence (§12.8, WI-520). Nothing happens to this reader
        // because of the scan: no aftercare, nobody discharged into anyone's
        // care, and no reachable source supports a triage rule for a plain head
        // CT. An invented tier list would contradict the corpus by omission,
        // which is WI-524's defect, and the corpus already sorts every symptom
        // it would list.
        //
        // The guard is SHAPE, not vocabulary. /review beat a four-heading ban on
        // WI-520 by writing a fifth, and beat a bold-lead-in regex on WI-521
        // three ways. Inverted, it cannot be walked around, because the bullets
        // are what make it a list (§12.8, WI-521 and WI-526).
        CuratedPage.AssertNoEscalationList(Page, Slug);
    }

    [Fact]
    public void ThePageCarriesNoCaregiverSectionAndThatIsRecordedRatherThanForgotten()
    {
        // §12.8: tests pages carry one only "where a test does have an aftercare
        // tail". Nobody is discharged home to look after somebody after a CT.
        // /tests/biopsy is the tests page that does carry one, and reading it
        // here is what proves this absence is a decision rather than an
        // oversight.
        Assert.DoesNotContain("[CAREGIVER]", Page, StringComparison.Ordinal);
        Assert.DoesNotContain("For the person caring for someone", Plain, StringComparison.Ordinal);

        Assert.Contains("[CAREGIVER]", CuratedPage.Read("tests", "biopsy.md"), StringComparison.Ordinal);
    }

    // --------------------------------------------------------------- sources

    [Fact]
    public void TheBannedSourcesAreAbsentAndTheClaimsTheyCarriedAreRederivedRatherThanDropped()
    {
        // §12.14: banning a citation is not fixing a claim, so each ban is
        // asserted next to the sentence that replaced it.
        var urls = CitedUrls();

        // (1) WebMD's dose figure. Banned, and the DIRECTION is published in its
        // place, which is what a reader can act on.
        Assert.DoesNotContain("webmd.com", urls, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("more than from a plain x-ray", Plain, StringComparison.Ordinal);

        // (2) ABTA. The dossier's quoted sentence was not on the page on two
        // fetches, so ABTA is not cited at all and ACS carries the comparison.
        Assert.DoesNotContain("abta.org", urls, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "cancer.org/cancer/types/brain-spinal-cord-tumors-adults/detection-diagnosis-staging",
            urls, StringComparison.Ordinal);

        // (3) The patient-experience claims. RadiologyInfo's head CT page, as
        // served, carries none of them; ACS's own CT page carries all of them.
        // Assert the REPLACEMENT is cited, because dropping the wrong citation
        // and keeping the sentence is a worse state than before (§12.8, WI-510).
        Assert.Contains("imaging-tests/ct-scan-for-cancer", urls, StringComparison.Ordinal);
        Assert.Contains("warmth spreading through your body", Plain, StringComparison.Ordinal);
        Assert.Contains("bitter or metallic taste", Plain, StringComparison.Ordinal);

        // (4) NCI patient PDQ. §12.1's own pattern rather than a domain ban,
        // because a domain ban cannot express it (§12.8, WI-528).
        Assert.DoesNotMatch(@"cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", urls);

        // (5) The project's standing dead list, so a later edit cannot quietly
        // reintroduce one.
        foreach (var dead in new[]
                 {
                     "academic.oup.com", "mdpi.com", "ascopubs.org", "sciencedirect.com",
                     "journals.lww.com", "ajnr.org", "researchgate.net", "medscape.com",
                     "mayoclinic.org", "hopkinsmedicine.org", "drugs.com", "medlineplus.gov/druginfo",
                 })
        {
            Assert.DoesNotContain(dead, urls, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void EveryCitedUrlCarriesATitleAndAnAccessedDate()
    {
        // A cheap structural check with a real job: the front matter's rulings
        // are long, and a source listed without an `accessed` date is one nobody
        // fetched (§12.2 item 8).
        var entries = Regex.Matches(
            CuratedPage.FrontMatter(Page),
            @"(?m)^  - url: (\S+)\r?\n\s+title: ""([^""]+)""\r?\n\s+accessed: (\d{4}-\d{2}-\d{2})");

        Assert.True(entries.Count == 3,
            $"expected three cited sources with a title and an accessed date, found {entries.Count}");
    }

    // ------------------------------------------------- shared corpus hygiene

    [Fact]
    public void NoBritishSpellingsOrIdioms()
    {
        // Runs on `Plain`, not `Body`: the description is the first paragraph a
        // reader meets and a body-scoped scan is blind to it (§12.8, WI-528).
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
        // §12.8 (WI-521): the restatement check has to run over the WHOLE corpus,
        // because a guard that checks the siblings you thought of is a guard that
        // finds nothing. The mechanics live on CuratedPage; what belongs here is
        // the SHORT named allowlist of sentences this page shares on purpose.
        // §12.10 says two pages must not state one safety claim at two
        // strengths, so where a claim is load-bearing on both, identical words
        // are the right answer.
        CuratedPage.AssertDoesNotRestateTheCorpus(
            Page, Slug,
            // The limit of any scan. /tests/mri and /treatments/craniotomy both
            // say it; this page says it; and saying it differently is how a
            // reader ends up with two answers to one safety question (§12.10).
            "it is only a piece of the tumor itself looked at in a lab that can do that "
            + "only a piece of the tumor itself looked at in a lab can say for certain",
            // Slot 9's prescribed opening. §12.8 says the honest answer to
            // "when will I know" is that timings are local, so every library
            // page opens that section the same way on purpose.
            "how long it takes depends on where you are so ask before you leave what follows "
            + "is who does what so you know what the wait is made of");
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class CtScanPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/ct-scan";

    private readonly WebApplicationFactory<Program> _factory;

    public CtScanPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Your CT scan", html, StringComparison.Ordinal);
        // No authoring marker and no unresolved directive reaches the reader.
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\[[A-Z][A-Z-]+\]", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // §12.8 (WI-519): a new library page nothing links to is half shipped,
        // and /tests has no index to catch it.
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/tests/mri", "/tests/biopsy", "/tumors/all-brain-tumors",
                     "/treatments/radiation-therapy",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/tests/mri", "/tests/planning-scans", "/tests/biopsy",
            "/tumors/all-brain-tumors", "/treatments/radiation-therapy", "/get-help-now");

    [Fact]
    public async Task TheDeepLinksIntoTheSiblingPagesLandOnRealAnchors() =>
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public void TheDoorsOnTheSiblingPagesAreAppendedSentencesRatherThanReplacements()
    {
        // §12.8 (WI-526): a door has to sit NEXT TO the sentence it was appended
        // to, or replacing the sentence outright leaves the guard green. Each
        // pair below is (the sibling's pre-existing sentence, the door), and the
        // distance between them is asserted rather than their mere presence.
        (string File, string Existing, string Door)[] doors =
        [
            ("tests/mri.md",
                "It has no iodine in it",
                "[Your CT scan](/tests/ct-scan)"),
            ("tumors/all-brain-tumors.md",
                "Often a scan done for a symptom",
                "[your CT scan](/tests/ct-scan)"),
            ("tests/biopsy.md",
                "The scan just before surgery](/tests/mri#navigation-scan)",
                "[your CT scan](/tests/ct-scan)"),
            ("treatments/radiation-therapy.md",
                "The planning CT scan, in your mask",
                "[Your CT scan](/tests/ct-scan)"),
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
