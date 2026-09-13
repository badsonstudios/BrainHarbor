using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-528: brain metastases, deepened. The second §12.3 SEVENTEEN-SECTION TUMOR
/// HUB outside the glioma family, and the first hub in the corpus where two of
/// the shared blocks are <em>wrong</em> rather than merely unnecessary.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * THE BACKLOG ITEM'S OWN HEADLINE NUMBER. WI-528 says "33-66% of brain
///     metastases are the first sign of cancer". Nothing supports it; the
///     largest series that asks the question gives 19.0%, and the neighbouring
///     figures are answers to different questions. No figure is published.
///   * "YOUR ORIGINAL ONCOLOGIST STILL LEADS", also from the item, also an
///     over-claim. The page may not assert who leads.
///   * [CAUSES], whose first sentence ("For most brain tumors, nobody knows the
///     cause") is FALSE on this hub, and [CROSSWALK], which describes naming
///     rules this reader's report does not follow. Both are excluded, and the
///     block files are not edited (§12.10, WI-514's blast radius).
///   * a lesion count, a dose, or a test sensitivity (§12.4 R2 and R3).
///   * a drug name in the medicine section, or anything that reads as "there
///     are pills for brain mets".
///   * "a clear spinal tap means you do not have it" — the one sentence on this
///     page whose opposite is dangerous.
///
/// Every guard here starts from §12.8's WI-527 lessons rather than rediscovering
/// them: allowlists REDACT rather than exempt, quantifiers allow a gap, numeric
/// bans are threshold-shaped, page-wide containment replaces section scope where
/// a copy elsewhere would be just as harmful, the outlook gate is checked for
/// claims in WORDS, every block directive is named, and every regex that touches
/// a line break uses <c>\r?\n</c> — never a bare <c>\n</c> and never <c>\s*</c>
/// standing in for a blank line, which is the bug this item found shipped on
/// /tumors/meningioma.
/// </summary>
public sealed class BrainMetastasesPageContentTests
{
    private const string Slug = "tumors/brain-metastases";
    private const string NamingHeading = "Why it is still called by the other cancer's name";
    private const string TeamHeading = "Who is looking after me now?";
    private const string MedicineHeading = "Is there a medicine that reaches the brain?";
    private const string CoveringHeading = "If the cancer is in the covering rather than in lumps";
    private const string BlameHeading = "Did I let this happen?";
    private const string CareHeading = "For the person caring for someone with this";

    private static string Page => CuratedPage.Read("tumors", "brain-metastases.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The section with its line breaks intact. <c>CuratedPage.Section</c>
    /// collapses all whitespace to single spaces, so anything that needs to see
    /// a PARAGRAPH — a count of a section's own claims, for instance — has to
    /// cut the markdown itself. `\r?\n`, never a bare `\n`.
    /// </summary>
    private static string RawSection(string heading)
    {
        var match = Regex.Match(
            Page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return match.Groups[1].Value;
    }

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// Title and description. <c>ReaderText</c> strips the front matter, so every
    /// body-scoped guard is blind to them while the template renders the
    /// description as the first paragraph a reader meets (§12.8, WI-524).
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

    /// <summary>
    /// THIRTEEN THROUGH NINETEEN ARE NOT OPTIONAL. Every copy of this constant
    /// in the corpus ran `one…twelve, twenty, thirty…` and skipped the teens —
    /// and the figure THIS page exists to refuse is <em>nineteen</em> percent.
    /// `/review` published it past the guard in one line. `dozen` and `couple`
    /// are here for the same reason: a count a reader can act on does not have
    /// to be a numeral or a decade.
    /// </summary>
    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?)";

    /// <summary>
    /// Removes an allowed phrase from a sentence instead of exempting the whole
    /// sentence (§12.8, WI-527: a substring allowlist is a free ride for
    /// everything else in the sentence).
    /// </summary>
    private static string Redact(string sentence, IEnumerable<string> allowed)
    {
        foreach (var phrase in allowed)
        {
            sentence = Regex.Replace(sentence, Regex.Escape(phrase), " ", RegexOptions.IgnoreCase);
        }

        return sentence;
    }

    private static List<string> Headings() =>
        Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

    // ---------------------------------------------------------------- numbers

    [Fact]
    public void TheBacklogsOwnFirstSignFigureAppearsNowhere()
    {
        // RULING 1, and the fourth time in six items that the backlog's headline
        // claim is the defect. WI-528 says "33-66% of brain metastases are the
        // first sign of cancer". The Vienna registry (PMC6267666, n = 2419)
        // gives "459/2419 (19.0%) BM patients presented with BM as first symptom
        // of advanced cancer". The 25% nearby is synchronous DIAGNOSIS and the
        // ~15% is unknown primary: three questions, three answers, welded into
        // one range that belongs to none of them.
        // No `(?:33|66|19|25)` branch: it is entirely subsumed by the CountWord
        // branch below it, and a dead branch that reads like extra protection is
        // worse than no branch (§12.8, WI-523's canary rule).
        var firstSign = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            // And the same claim in words, which is how it would come back.
            + @"\b(?:a third|two thirds|a fifth|a quarter|half)\b[^.]{0,60}"
            + @"\b(?:first sign|first symptom|found first|before anybody knew)\b|"
            + @"\b(?:first sign|first symptom)\b[^.]{0,60}\b(?:a third|two thirds|a fifth|a quarter|half)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Between 33% and 66% of brain metastases are the first sign of cancer.",
                     "In 19 percent of people the brain tumor was the first symptom.",
                     // `/review` published the refused figure past the first
                     // version of this guard by spelling it out.
                     "In nineteen percent of people, the brain tumor is the first sign of cancer.",
                     "Fifteen percent of people never have a starting point found.",
                     "For about a third of people, a brain metastasis is the first sign of cancer.",
                     "The first sign of cancer is a brain metastasis in two thirds of cases.",
                 })
        {
            Assert.Matches(firstSign, known);
        }

        Assert.DoesNotMatch(firstSign, Plain);

        // AND THE SHAPE IS ON THE PAGE, because a page that drops a claim
        // without replacing it leaves the reader where it found them
        // (§12.8, WI-521). The reassurance inside the shape is what the same
        // paper supports: the search usually finds the starting point, and
        // usually finds it soon.
        var found = PlainOf("How do doctors find out it is this?");
        Assert.Matches(new Regex(
            @"Sometimes the brain scan is what finds the cancer in the first place",
            RegexOptions.IgnoreCase), found);
        Assert.Matches(new Regex(
            @"In most people that search finds the starting\s*point, and finds it within a few months",
            RegexOptions.IgnoreCase), found);
        Assert.Matches(new Regex(@"cancer of unknown primary", RegexOptions.IgnoreCase), found);

        // And it is not left as a frightening dead end.
        Assert.Matches(new Regex(@"Treatment does not wait for the answer", RegexOptions.IgnoreCase),
            found);
    }

    [Fact]
    public void ThePageCarriesNoShareNoLesionCountThresholdAndNoDose()
    {
        // §12.4 R2 and R3. StatPearls gives "one to four brain metastases" and
        // "up to ten", and those are thresholds a reader measures their own scan
        // against — the same defect as meningioma's "under 2 cm" (WI-527). The
        // page says a few, and many, and hands the line to the team.
        //
        // The numeric branches are THRESHOLD-SHAPED rather than unit-shaped,
        // because taking the unit out of a threshold does not stop it being one
        // (§12.8, WI-527).
        var figure = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b" + CountWord + @"\s+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            + @"\b" + Fraction + @"\s+(?:of\s+)?(?:all\s+)?(?:the\s+)?(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|cases|metastases|tumors)\b|"
            + @"\b(?:roughly|about|around|nearly|approximately|between|just over|just under)\s+"
            + @"(?:a|one)\s+" + Fraction + @"\b|"
            + @"\bmajorit(?:y|ies)\b|\bminorit(?:y|ies)\b|\ba handful\b|"
            + @"\b(?:most|almost all|nearly all|hardly any|the majority of)\s+(?:\w+\s+){0,3}"
            + @"(?:people|patients|cases|metastases|tumors|them)\b|"
            // The lesion-count threshold, in digits or in words. The count has
            // to be followed by something that makes it a THRESHOLD: a bare
            // `more than one` is ordinary English for "several" and appears
            // three times on this page describing the scan honestly, and a
            // guard that fails a correct page is itself the defect (§12.8,
            // WI-508).
            + @"\b(?:more than|less than|fewer than|no more than|at least|up to|under|over|"
            + @"above|below)\s+(?:a |an )?" + CountWord + @"\s*(?:\w+\s+)?(?:spots?|metastases|lesions|tumors|"
            + @"them|cm|mm|mg|centimet\w*|millimet\w*|tablets?|pills?|doses?|fractions?|"
            + @"visits?|sessions?)\b|"
            // THE LINE ITSELF, with no threshold word and no noun. `/review`
            // published "Teams usually draw the line at four" and "Focused
            // radiation is standard for one to four of them" past the branch
            // above: a threshold does not need the word "than" to be one.
            + @"\b(?:draws?|drawn|set|sets?|falls?) the line at\s+" + CountWord + @"\b|"
            + @"\b(?:standard|usual|offered|considered|used) (?:for|when there are)\s+"
            + @"(?:up to\s+)?(?:a |an )?" + CountWord + @"\b|"
            + @"\b" + CountWord + @"\s*(?:to|or)\s*" + CountWord + @"\s+(?:of them|spots|"
            + @"metastases|lesions)\b|"
            + @"\b" + CountWord + @"\s+or (?:more|fewer|less)\b|"
            + @"\b" + CountWord + @"\s*(?:to|-)\s*" + CountWord + @"\s+(?:spots|metastases|lesions)\b|"
            // Doses and units.
            + @"\bgrays?\b|\bGy\b|\bmillimet\w*|\bcentimet\w*|\bmilligram\w*|"
            + @"\b\d+(?:\.\d+)?\s*(?:mg|cm|mm)\b|\b\d+\s+fractions?\b",
            RegexOptions.IgnoreCase);

        var allowed = new[]
        {
            // The page's own refusal to draw the line, which is the sentence the
            // whole ruling exists to produce. Redacted rather than exempted.
            "Where the line falls between a few and many is your team's to draw",

            // THE ONE SHARE THE PAGE CARRIES, and it is §12.4 R2's shape rather
            // than a figure: the reassurance that replaced the backlog's made-up
            // range. Listed by name so that adding a second is a deliberate act.
            "In most people that search finds the starting point",
        };

        foreach (var known in new[]
                 {
                     "Focused radiation is standard for one to four spots.",
                     "It may be acceptable for up to ten brain metastases.",
                     "About 40% of people have more than one.",
                     "The usual course is 30 Gy in 10 fractions.",
                     "Fewer than four spots means focused radiation.",
                     "Most people have more than one.",
                     // The four `/review` walked through.
                     "Teams usually draw the line at four.",
                     "Focused radiation is standard for one to four of them.",
                     "Focused radiation is used when there are no more than four of them.",
                     "It may be offered for up to a dozen spots.",
                 })
        {
            Assert.Matches(figure, known);
        }

        var offenders = CuratedPage.SentencesOf(Plain)
            .Where(s => figure.IsMatch(Redact(s, allowed)))
            .ToList();
        Assert.True(offenders.Count == 0,
            "this page publishes a number a reader would measure themselves against:\n  "
            + string.Join("\n  ", offenders));

        // And it says why the line is not here, rather than being silently
        // silent (§12.8, WI-521).
        var treatment = PlainOf("How is it usually treated?");
        Assert.Matches(new Regex(
            @"Where the line falls between a few and many is your team's to draw",
            RegexOptions.IgnoreCase), treatment);
        Assert.Matches(new Regex(@"a number from a web page is not your number",
            RegexOptions.IgnoreCase), treatment);
    }

    [Fact]
    public void NoTestSensitivityFigureIsPublishedForTheSpinalTap()
    {
        // §12.4 R2 at its clearest: the two sources give 45%, 50-60%, 85-90% and
        // 90% for the sensitivity of the same test. Four numbers, one question.
        // The page prints the direction and the action.
        // `(?:in|out of|of)` and a generic `<count> of every <count>`: `/review`
        // published "six of every ten people" past a branch that only knew
        // "in" and "out of", and past a fraction list of five hand-picked
        // phrases. A proportion does not need the word "in".
        const string Proportion =
            @"(?:" + Fraction + @"|" + CountWord + @"\s+(?:in|out of|of)\s+(?:every\s+)?"
            + CountWord + @")";
        var sensitivity = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b" + Proportion + @"\b[^.]{0,60}"
            + @"\b(?:tap|puncture|sample|cytology|test|cells)\w*\b|"
            + @"\b(?:tap|puncture|sample|cytology|test|cells)\w*\b[^.]{0,60}"
            + @"\b" + Proportion + @"\b|"
            + @"\bsensitivit\w*",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "The first tap misses it in about half of people.",
                     "Cytology is positive in 50 percent after one tap.",
                     "The sensitivity of the test rises with repeat samples.",
                     // The one `/review` walked through.
                     "Cytology finds cancer cells in six of every ten people after the first tap.",
                 })
        {
            Assert.Matches(sensitivity, known);
        }

        Assert.DoesNotMatch(sensitivity, Plain);
    }

    // ------------------------------------------------------------- the spine

    [Fact]
    public void TheCancerKeepsTheNameOfWhereItStartedAndThePageSaysWhyThatMatters()
    {
        // RULING 3 and the reason the page exists. NCI's own framing, written in
        // our words: "Metastatic cancer has the same name as the primary cancer."
        var naming = PlainOf(NamingHeading);

        Assert.Matches(new Regex(@"A cancer is named for where it began", RegexOptions.IgnoreCase),
            naming);
        Assert.Matches(new Regex(@"Not for where it has got to", RegexOptions.IgnoreCase), naming);

        // With the worked example, because the abstract version does not land.
        Assert.Matches(new Regex(
            @"Breast cancer that spreads to the brain is called breast cancer that has\s*spread",
            RegexOptions.IgnoreCase), naming);

        // And the consequence, which is the part that changes what happens to
        // the reader: the name decides the treatment.
        Assert.Matches(new Regex(@"the treatment follows breast cancer", RegexOptions.IgnoreCase),
            naming);

        // It is in the short version too, because §12.3 says most readers get
        // 20 to 28% of a page.
        var shortVersion = PlainOf("The short version");
        Assert.Matches(new Regex(
            @"It keeps the name of where it started, and that name is what\s*decides your treatment",
            RegexOptions.IgnoreCase), shortVersion);
        Assert.Matches(new Regex(@"not a second, different cancer", RegexOptions.IgnoreCase),
            shortVersion);

        // And the misreading it exists to stop is named rather than left to be
        // inferred.
        Assert.Matches(new Regex(
            @"that is a statement about the address, not about the disease changing\s*into "
            + @"something else", RegexOptions.IgnoreCase), naming);

        // THE TAIL OF THE SPINE SENTENCE IS NOT FREE. `/review` appended "…so
        // nothing about your cancer treatment has to change" to the pinned
        // clause: green, false, and contradicting the medicine section eight
        // screens later. Pinning a first clause leaves its tail free
        // (§12.8, WI-526).
        var nothingChanges = new Regex(
            @"\bnothing (?:about (?:your|the) (?:cancer )?treatment|else) (?:has to|needs to|"
            + @"will) change\b|\byour treatment (?:does not|will not|need not) change\b|"
            + @"\bcarry on exactly as (?:before|you were)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(nothingChanges,
            "The treatment follows breast cancer, so nothing about your cancer treatment "
            + "has to change.");
        Assert.DoesNotMatch(nothingChanges, Plain);
    }

    [Fact]
    public void TheOneWayTrafficIsRoutedRatherThanRestated()
    {
        // §12.10, one claim one strength. /tumors/glioma already owns the
        // outbound half inside its grade-versus-stage answer. This page carries
        // the INBOUND half and routes for the other, and the sibling is read
        // rather than assumed.
        var naming = PlainOf(NamingHeading);

        // `/review`'s BLOCKER B2: the first version ANSWERED the outbound
        // question in three sentences ("Almost never...") at a strength the
        // sibling hedges, and the reassurance's subject arrived a sentence
        // after the reassurance — so a reader holding "can it spread anywhere
        // else?" read it as "no". It already has spread once. The two questions
        // are now separated and the frightening one is answered first.
        Assert.Matches(new Regex(
            @"Can a tumor that STARTED in the brain travel out to the rest of the body\?",
            RegexOptions.IgnoreCase), naming);
        Assert.Matches(new Regex(@"That one is not about you", RegexOptions.IgnoreCase), naming);
        Assert.Matches(new Regex(
            @"Can the cancer you have spread somewhere else again\?", RegexOptions.IgnoreCase),
            naming);
        Assert.Matches(new Regex(
            @"Yes. It already has\s*once, and that is what the word metastatic means",
            RegexOptions.IgnoreCase), naming);
        Assert.Matches(new Regex(
            @"That is a hard sentence and it is the true one", RegexOptions.IgnoreCase), naming);
        Assert.Contains("/tumors/glioma", naming, StringComparison.Ordinal);

        // AND THE ABSOLUTE VERSION IS BANNED. `/review` wrote "A tumor that
        // starts in the brain will not spread to the rest of your body" —
        // false, and a stronger claim than the sibling's hedged one.
        var absolute = new Regex(
            @"\b(?:will not|cannot|can't|does not|never)\s+(?:ever\s+)?spread\b|"
            + @"\bstays? in the (?:brain|nervous system)\b|\bno risk of (?:it )?spreading\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(absolute, "A tumor that starts in the brain will not spread to the rest "
            + "of your body.");
        Assert.Matches(absolute, "It stays in the nervous system.");
        Assert.DoesNotMatch(absolute, Plain);

        var glioma = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "glioma.md")));
        Assert.Contains(
            "very rarely leaves the central nervous system for the rest of the body",
            glioma, StringComparison.OrdinalIgnoreCase);

        // AND THE MECHANISM IS NOT ASSERTED. The textbook reason (no lymph
        // drainage in the brain) stopped being settled when lymphatic vessels
        // were described in the meninges in 2015. A page that explains a
        // contested mechanism to a frightened reader has spent its credibility
        // on something it did not need.
        // `does not have` as well as `has no`: `/review` put the contested 2015
        // mechanism back on the page by choosing a different negation.
        var mechanism = new Regex(
            @"\bno lymph\w*|\blymphatic\w*|\bdrainage channels?\b|\bhas no drainage\b|"
            + @"\bbecause the brain (?:has|lacks|does not have|doesn't have)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(mechanism, "That is because the brain has no lymphatic drainage.");
        Assert.Matches(mechanism, "That is because the brain does not have the drainage channels "
            + "that carry cancer cells around the rest of the body.");
        Assert.DoesNotMatch(mechanism, Plain);

        // And stage words are defused rather than left to frighten.
        Assert.Matches(new Regex(@"They are not a countdown, and they\s*are not a grade",
            RegexOptions.IgnoreCase), naming);
    }

    // ------------------------------------------------------------- the team

    [Fact]
    public void ThePageNeverSaysWhoLeads()
    {
        // RULING 2. The backlog item asserts "your original oncologist still
        // leads". The source names a different team and no leader at all: "A
        // multidisciplinary treatment team of a neurosurgeon, radiation
        // oncologist, and neuro-oncologist should participate in the formulation
        // of the treatment plan."
        //
        // §12.14: banning the phrase is not banning the claim, so this bans the
        // CLAIM SHAPE — anybody being put in charge.
        // `lead[s]?`, not `leads`. `/review` re-published the refused claim as
        // "takes the lead" — the same sentence, one letter shorter — and as
        // "your CANCER doctor stays the one who decides", which the old second
        // branch could not see because it required the noun to sit next to
        // "your". One missing inflection and one missing word gap.
        var leads = new Regex(
            @"\b(?:still |now )?(?:leads?\b|is in charge|takes over|takes the lead|"
            + @"runs your care|remains in charge|stays in charge|is the lead)|"
            + @"\byour (?:\w+\s+){0,2}(?:oncologist|team|doctor|surgeon)\b[^.]{0,50}"
            + @"\b(?:leads?\b|in charge|decides (?:everything|what happens)|"
            + @"the one who decides|has the final say|calls the shots)|"
            + @"\bis led by\b|\bstays? the one who\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Your original oncologist still leads your care.",
                     "Your cancer doctor is in charge of the whole plan.",
                     "Care here is led by the neuro-oncologist.",
                     // The two `/review` got through.
                     "Your original oncologist takes the lead, and the brain team advises.",
                     "Your cancer doctor stays the one who decides what happens next.",
                 })
        {
            Assert.Matches(leads, known);
        }

        // The page says "it is not a sign that nobody is in charge" in order
        // to REFUSE the claim, so that sentence comes out and the ban holds
        // over everything else (§12.14, and the WI-508 defect).
        const string NotInCharge = "it is not a sign that nobody is in charge";
        Assert.DoesNotMatch(leads, Redact(Plain, [NotInCharge]));

        // WHAT IS SUPPORTED IS ON THE PAGE, and it is more useful than the claim
        // it replaces: you are not handed over, and the action is to find out who
        // is joining things up.
        var team = PlainOf(TeamHeading);
        // CONDITIONAL, because the page itself says four screens earlier that
        // "some people arrive at this page having had no idea they had cancer at
        // all" — and that reader has no team to not be handed over from. An
        // unconditional claim about how care is organised is under-scoped for
        // the reader the page went to the trouble of naming (§12.10).
        Assert.Matches(new Regex(
            @"If you already have a cancer team, they are not replaced",
            RegexOptions.IgnoreCase), team);
        Assert.Matches(new Regex(
            @"If this is all new, your team is being built this week",
            RegexOptions.IgnoreCase), team);
        Assert.Matches(new Regex(
            @"it is not a sign that nobody\s*is in charge", RegexOptions.IgnoreCase), team);
        Assert.Matches(new Regex(@"So ask this, early: who is coordinating\?",
            RegexOptions.IgnoreCase), team);

        // And the reason the question matters, which is the honest part.
        Assert.Matches(new Regex(
            @"exactly the situation where something gets dropped\s*between two sets of notes",
            RegexOptions.IgnoreCase), team);
    }

    // ------------------------------------------------------- the medicine ask

    [Fact]
    public void TheMedicineSectionAsksAQuestionAndNamesNoDrug()
    {
        // RULING 6, and the item's own instruction: never "there are pills for
        // brain mets". The literature offers 91%, 86.6% and 79% intracranial
        // response rates and a dozen drug names. None of it goes on the page: a
        // named drug reads as a named drug FOR YOU, and a response rate reads as
        // your odds.
        var medicine = PlainOf(MedicineHeading);

        // NOT A NAME LIST. §12.14's argument applies to drugs as much as to
        // citations: thirteen hard-coded names is thirteen names, and `/review`
        // walked "entrectinib" straight through it. Targeted cancer drugs are
        // named by suffix convention, so the SHAPE is bannable — and the two
        // names this corpus does use legitimately (memantine, dexamethasone)
        // end in neither.
        var drugs = new Regex(
            @"\b\w{4,}(?:tinib|ciclib|parib|zomib|nib|mab)\b|"
            + @"\bcapecitabine\b|\btemozolomide\b|\bimmunotherapy drugs? called\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(drugs, "Osimertinib reaches the brain in EGFR lung cancer.");
        Assert.Matches(drugs, "Entrectinib gets into the brain in ROS1 lung cancer.");
        Assert.Matches(drugs, "Tucatinib is used in HER2 breast cancer.");
        Assert.Matches(drugs, "Pembrolizumab is given for melanoma.");
        Assert.DoesNotMatch(drugs, Plain);

        // The "there is a pill" reading, banned by shape rather than by word.
        // CLAIM-SHAPED, not frame-shaped. `/review` wrote "Tablets now reach the
        // brain, and for many people they shrink the spots without radiation" —
        // which is "there are pills for brain mets" in a sentence the literal
        // frame cannot see. The claim is: a medicine, taken by mouth, doing
        // something to the tumors.
        var pill = new Regex(
            @"\bthere (?:is|are) (?:a |an )?(?:pill|drug|medicine|treatment)s?\b[^.]{0,40}"
            + @"\b(?:for|that treats?)\b[^.]{0,30}\b(?:brain mets|brain metastases|this)\b|"
            + @"\b(?:a|the) pill for\b|\bcan be treated with tablets\b|"
            + @"\bnew drugs (?:can|now) (?:treat|cure|clear)\b|"
            + @"\b(?:tablets?|pills?|drugs?|medicines?)\b[^.]{0,80}"
            + @"\b(?:shrink|clear|get rid of|melt|wipe out|take away)\b|"
            + @"\b(?:shrink|clear|get rid of)\b[^.]{0,60}\b(?:without|instead of) "
            + @"(?:radiation|surgery)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(pill, "There are drugs for brain mets now.");
        Assert.Matches(pill, "It can be treated with tablets.");
        Assert.Matches(pill, "Tablets now reach the brain, and for many people they shrink the "
            + "spots without radiation.");
        // REDACTED, NOT EXEMPTED. The page says "None of this means there is a
        // pill for brain metastases" in order to REFUSE the claim, and a ban
        // list that fires on the sentence refuting it is the WI-508 defect. The
        // answer is not to weaken the ban (§12.14) — it is to take the one
        // sentence out and hold the ban over everything else.
        const string Refusal = "None of this means there is a pill for brain metastases";
        Assert.Matches(pill, Refusal);
        Assert.DoesNotMatch(pill, Redact(Plain, [Refusal]));

        // THE QUESTION, IN THOSE WORDS. This is the whole point of the section
        // and the item asked for it by name.
        Assert.Matches(new Regex(
            @"Has my cancer been tested for a\s*marker with a drug that reaches the brain\?",
            RegexOptions.IgnoreCase), medicine);

        // AND THE SECOND HALF ALMOST NOTHING PRINTS: the tumor in the brain can
        // carry a different change from the one the reader started with, so a
        // test on the original sample may not answer the question.
        Assert.Matches(new Regex(
            @"The tumor in your brain\s*can carry a different change from the one you started with",
            RegexOptions.IgnoreCase), medicine);
        Assert.Matches(new Regex(
            @"it is fair to ask whether that tissue was tested too", RegexOptions.IgnoreCase),
            medicine);

        // And the honest close, which stops the section reading as a promise.
        Assert.Matches(new Regex(
            @"None of this means there is a pill for brain metastases", RegexOptions.IgnoreCase),
            medicine);

        // The blood-brain lining is explained as a property of the MEDICINE
        // rather than as somebody failing the reader (§12.12 runs both ways).
        Assert.Matches(new Regex(
            @"That is a property of the\s*medicine, not a sign that anything is being done badly",
            RegexOptions.IgnoreCase), medicine);
    }

    // ------------------------------------------------- leptomeningeal disease

    [Fact]
    public void AClearSpinalTapIsNeverPresentedAsAnAllClear()
    {
        // RULING 7, and the sentence on this page whose opposite is dangerous.
        // The source is blunter than the backlog item was: "A negative cytology
        // after three lumbar punctures does not rule out the diagnosis of LM in
        // the context of other positive testing."
        var covering = PlainOf(CoveringHeading);

        Assert.Matches(new Regex(@"a clear spinal tap is not an all-clear", RegexOptions.IgnoreCase),
            covering);
        Assert.Matches(new Regex(
            @"The\s*test misses this often enough that a single clear result settles nothing",
            RegexOptions.IgnoreCase), covering);

        // Both directions of the two-test point, because a reader told only one
        // of them will conclude the other test is the reliable one.
        Assert.Matches(new Regex(
            @"a scan can show it when the fluid\s*looks normal, and the fluid can show it when "
            + @"the scan looks normal", RegexOptions.IgnoreCase), covering);

        // AND THE ACTION, because a fact without one leaves the reader holding
        // it (§12.8). It is framed as a question their team would also ask, so
        // it does not read as an instruction to fight with them.
        Assert.Matches(new Regex(
            @"it\s*is fair to ask whether it should be repeated", RegexOptions.IgnoreCase),
            covering);
        Assert.Matches(new Regex(@"That is not arguing with your\s*team", RegexOptions.IgnoreCase),
            covering);

        // THE OPPOSITE CLAIM IS BANNED PAGE-WIDE, not inside this section. A
        // section-scoped guard proves nothing about the rest of the page, which
        // is the WI-527 lesson this item is applying rather than rediscovering.
        // NO ADJACENCY ASSUMPTION. `/review` beat the first version twice by
        // moving one word: "A tap that comes BACK clear rules this out", and
        // "If the FIRST tap is clear". The guard now works from the noun
        // outward in both directions, and the page's own refusal is redacted
        // rather than the ban weakened (§12.14).
        var allClear = new Regex(
            @"\b(?:tap|puncture)\b[^.]{0,50}\b(?:clear|negative|normal)\b[^.]{0,60}"
            + @"\b(?:rules? (?:it |this )?out|means you do not have|means you have not|"
            + @"is an all-clear|settles it|settles this|is the answer|"
            + @"you can stop worrying|you are in the clear|you do not have it)\b|"
            + @"\b(?:clear|negative|normal)\b[^.]{0,30}\b(?:tap|puncture)\b[^.]{0,60}"
            + @"\b(?:rules? (?:it |this )?out|means you do not have|is an all-clear|"
            + @"settles it|settles this|you can stop worrying)\b|"
            + @"\bone tap is enough\b|\bone tap settles\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(allClear, "A clear tap rules it out.");
        Assert.Matches(allClear, "If the tap is clear you do not have it.");
        Assert.Matches(allClear, "One tap is enough.");
        Assert.Matches(allClear, "A tap that comes back clear rules this out.");
        Assert.Matches(allClear, "If the first tap is clear, you can stop worrying about this.");

        // The page's own refusal contains the words the ban is made of, so it
        // is taken out and the ban holds over everything that is left.
        const string Refusal = "a clear spinal tap is not an all-clear";
        Assert.DoesNotMatch(allClear, Redact(Plain, [Refusal]));

        // The section is signposted rather than buried, as the item asked: it
        // has its own heading and its own anchor, and both survive a rename.
        Assert.Contains("{#leptomeningeal}", Page, StringComparison.Ordinal);
        Assert.Contains(CoveringHeading, Headings());

        // And the picture that gets a reader to raise it at all: several things
        // at once rather than one thing you could point to.
        Assert.Matches(new Regex(@"The clue is symptoms in several places at once",
            RegexOptions.IgnoreCase), covering);
    }

    [Fact]
    public void CordCompressionIsAnEmergencyHereRatherThanAThingToRaiseAtTheNextAppointment()
    {
        // `/review`'s BLOCKER B1, and it was right. The leptomeningeal symptom
        // list carried "numbness in the legs, trouble passing water" with one
        // action attached — ask whether the spinal tap should be repeated — and
        // in somebody with metastatic cancer that pair is the textbook picture
        // of the spinal cord being pressed on, where the window is hours.
        //
        // /tumors/spinal-cord-tumor and /tumors/meningioma already file it, at a
        // far higher tier. Two pages, one claim, two strengths (§12.10), and
        // this page had the weak one.
        var symptoms = CuratedPage.Flatten(Reader(Section("What symptoms does it cause?")));

        var urgent = Regex.Match(symptoms,
            @"\*\*If you have cancer and your legs go weak.*?(?=\[ESCALATION\])",
            RegexOptions.Singleline);
        Assert.True(urgent.Success, "the scoped cord-compression line has gone");
        var line = urgent.Value;

        // Scoped in its own first clause, before any symptom is named.
        Assert.StartsWith("**If you have cancer and your legs go weak", line.Trim(),
            StringComparison.Ordinal);

        foreach (var owed in new[]
                 {
                     "new weakness in the legs", "a change in\nhow you walk",
                     "numbness around the saddle area", "bladder or bowel",
                     "not to wait for the next appointment",
                 })
        {
            Assert.Contains(CuratedPage.Flatten(owed), line, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Matches(new Regex(@"Do not wait for it to get bad\s*first", RegexOptions.IgnoreCase),
            line);
        Assert.Contains("/tumors/spinal-cord-tumor", line, StringComparison.Ordinal);

        // ONE CLAIM, ONE STRENGTH. The corpus's own sentence, word for word,
        // read rather than assumed (§12.10).
        foreach (var sibling in new[] { "spinal-cord-tumor", "meningioma" })
        {
            var text = CuratedPage.Flatten(CuratedPage.ReaderText(
                CuratedPage.Read("tumors", sibling + ".md")));
            Assert.Contains(
                "New weakness or new bladder trouble is a reason to be seen quickly, not to wait",
                text, StringComparison.Ordinal);
        }

        // AND THE PAIR IS NOT ALSO FILED SOMEWHERE WEAKER. The leptomeningeal
        // section may mention them, but only to point back up here — it must
        // not attach its own, slower action to them.
        var covering = PlainOf(CoveringHeading);
        Assert.Matches(new Regex(
            @"They are in the urgent list above rather than here", RegexOptions.IgnoreCase),
            covering);
        var weakFiling = new Regex(
            @"\b(?:legs? (?:go |going )?weak|weakness in (?:the|your) legs|bladder|"
            + @"passing water|trouble peeing)\b[^.]{0,120}"
            + @"\b(?:next appointment|mention it|raise it|worth asking|ask whether|bring it up)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(weakFiling,
            "New numbness in the legs and trouble passing water are worth asking about.");
        // The urgent line contains the words the ban is made of, so it is
        // redacted and the ban holds over the rest of the page.
        const string Urgent =
            "New weakness or new bladder trouble is a reason to be seen quickly, not to wait\nfor the next appointment.";
        Assert.DoesNotMatch(weakFiling, Redact(Plain, [CuratedPage.Flatten(Urgent)]));

        // The shared block is NOT edited to carry it. Doing that would put a
        // cord line on every glioma hub (§12.10, the WI-514 blast radius), and
        // it is the trap WI-527 avoided the same way.
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        foreach (var absent in new[]
                 { "spinal cord", "bladder", "bowel", "saddle", "your water", "both legs" })
        {
            Assert.DoesNotContain(absent, block, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ------------------------------------------------------- the shared blocks

    [Fact]
    public void TheTwoBlocksThatAreWrongOnThisHubAreExcludedAndUnedited()
    {
        // RULINGS 3 and 4 in the front matter, and §12.10's warning at its
        // sharpest. The shared self-blame block opens "For most brain tumors,
        // nobody knows the cause." On THIS hub that sentence is false — the
        // cause is the cancer the reader already knows they have — so including
        // it would hand a frightened reader a lie in a soothing voice (§12.12).
        //
        // [CROSSWALK] fails the same way for a different reason: it describes
        // the 2021 rules for naming tumors that START in the brain, and this
        // reader's report is written under the rules of their first cancer.
        // The BODY, not the whole file: the front matter names both directives
        // in the note explaining why they are absent, and a guard that cannot
        // tell a directive from a comment about one would force the reasoning
        // off the page it belongs on.
        var body = CuratedPage.Body(Page);
        foreach (var wrongHere in new[] { "[CAUSES]", "[CROSSWALK]" })
        {
            Assert.DoesNotContain(wrongHere, body, StringComparison.Ordinal);
            Assert.Contains(wrongHere, CuratedPage.FrontMatter(Page), StringComparison.Ordinal);
        }

        // THE BLOCKS THEMSELVES ARE NOT EDITED. Rewriting causes.md to fit this
        // page would put this page's answer on every glioma hub, where it is
        // wrong — the WI-514 blast radius, and the trap WI-527 avoided by
        // scoping a line instead of editing a block.
        var causes = CuratedPage.Flatten(File.ReadAllText(
            Path.Combine(CuratedPage.BlocksRoot, "causes.md")));
        Assert.Contains("For most brain tumors, nobody knows the cause", causes,
            StringComparison.OrdinalIgnoreCase);
        foreach (var absent in new[]
                 { "metastasis", "metastases", "spread to the brain", "first cancer" })
        {
            Assert.DoesNotContain(absent, causes, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE PAGE STILL ANSWERS THE QUESTION, in its own words, because
        // dropping a block is not the same as dropping a contract item. The
        // question here is a different one and it gets a different answer.
        var blame = PlainOf(BlameHeading);
        Assert.Matches(new Regex(
            @"Cancer spreading is something the cancer does. It is not something you did",
            RegexOptions.IgnoreCase), blame);
        Assert.Matches(new Regex(
            @"I should have gone to the doctor sooner", RegexOptions.IgnoreCase), blame);
        Assert.Matches(new Regex(@"Somebody missed it", RegexOptions.IgnoreCase), blame);

        // Including the one this reader is told by other people, which is the
        // §12.12 direction that does the most harm.
        Assert.Matches(new Regex(
            @"The idea that staying positive changes\s*the outcome puts the blame in the worst "
            + @"possible place, and it is not true", RegexOptions.IgnoreCase), blame);

        // AND THE FALSE SENTENCE IS BANNED BY SHAPE, not by the block's wording.
        // `/review` hand-wrote "Nobody can say why the cancer chose your brain,
        // and usually there is no answer to find" in the page's own voice: no
        // shared shingle, no literal match, and the exact claim the exclusion
        // exists to keep off this hub.
        var noCauseKnown = new Regex(
            @"\bnobody (?:knows|can say|can tell you) why\b|\bno (?:answer|reason) to find\b|"
            + @"\bthe cause is (?:not )?(?:unknown|a mystery)\b|"
            + @"\bnobody knows (?:the|what) cause\b|\bthere is no known cause\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(noCauseKnown,
            "Nobody can say why the cancer chose your brain, and usually there is no answer "
            + "to find.");
        Assert.DoesNotMatch(noCauseKnown, Plain);

        // The inherited-risk question the block WAS carrying for other hubs is
        // answered here rather than dropped, because on this hub it has a
        // different answer and a real referral behind it.
        Assert.Matches(new Regex(@"Could I have passed this on\?", RegexOptions.IgnoreCase), blame);
        Assert.Matches(new Regex(@"no part of this is catching", RegexOptions.IgnoreCase), blame);

        // The page does not restate the block it declined to include.
        var blockShingles = Shingles(causes, 8).ToHashSet();
        var own = Shingles(Reader(Section(BlameHeading)), 8)
            .Where(blockShingles.Contains).Where(CarriesContent).ToList();
        Assert.True(own.Count == 0, "the self-blame section restates the block it excluded:\n  "
            + string.Join("\n  ", own));
    }

    [Fact]
    public void TheBlocksThisHubDoesOweAreIncluded()
    {
        // Named explicitly, because a contract item no test names is not a
        // contract item (§12.8, WI-527: deleting [MECHANISM] broke nothing on
        // /tumors/meningioma until a test said its name).
        // THE BODY, not the whole file. `/review` deleted `[CAREGIVER]` from the
        // body and mentioned it in a front-matter note, and the guard passed on
        // the comment — the sister test two methods up already reads `Body` for
        // exactly this reason, and this one did not.
        var body = CuratedPage.Body(Page);
        foreach (var directive in new[]
                 { "[MECHANISM]", "[ESCALATION]", "[CAREGIVER]", "[TUMOR-BOARD]" })
        {
            Assert.Matches(new Regex(@"(?m)^" + Regex.Escape(directive) + @"\s*$"), body);
        }

        // And the page composes, so a missing block fails here rather than
        // rendering as an empty section.
        var exception = Record.Exception(() => CuratedPage.Composed(Page, Slug));
        Assert.True(exception is null, $"{Slug} does not compose: {exception?.Message}");

        CuratedPage.AssertEscalationTiers(Page, Slug, "What symptoms does it cause?");
    }

    [Fact]
    public void TheCaregiverSectionAddsThreeThingsAndParaphrasesNothing()
    {
        // The defect WI-525 wrote down, WI-526 repeated and WI-527 let through a
        // paraphrase of: no shingle check can see a restatement that shares no
        // eight-word run, so the section's own claims are pinned AND counted.
        var care = PlainOf(CareHeading);

        Assert.Matches(new Regex(@"Three more, and they come with this situation rather than with tumors in general",
            RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(
            @"You are dealing with two teams, and you will be the one holding both",
            RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(
            @"only one of\s*them is the tumor", RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(
            @"People will hear .it has spread. and treat it as the end of the conversation",
            RegexOptions.IgnoreCase), care);

        // COUNT PARAGRAPHS, NOT BOLD RUNS. `/review` added a fourth claim with
        // no bold on it at all and the count stayed at three — which is the
        // WI-527 lesson ("count the section's own claims when a paraphrase is
        // the failure mode") defeated by dropping the markup the count was
        // reading. The section's own tail is everything after the promise, and
        // it gets exactly three paragraphs.
        var raw = Reader(RawSection(CareHeading));
        var promise = raw.IndexOf("Three more,", StringComparison.Ordinal);
        Assert.True(promise > 0, "the caregiver section has lost its promise of three");
        var tail = raw[promise..];
        var paragraphs = Regex.Split(tail, @"\r?\n[ \t]*\r?\n")
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();
        Assert.True(paragraphs.Count == 4,
            "the caregiver section promises three things and carries "
            + $"{paragraphs.Count - 1}:\n  " + string.Join("\n  ", paragraphs.Skip(1)));

        var leads = paragraphs.Skip(1)
            // Singleline: a bold lead-in wraps across a line in the source, and
            // without it `.` stops at the newline and the match comes back
            // empty on exactly the claims that are long enough to matter.
            .Select(p => Regex.Match(p, @"\*\*(.+?)\*\*", RegexOptions.Singleline).Groups[1].Value)
            .ToList();
        Assert.All(leads, l => Assert.False(string.IsNullOrWhiteSpace(l),
            "a caregiver claim has no bold lead-in, so it reads as part of the one above it"));

        var block = CuratedPage.Flatten(Regex.Replace(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")), @"^---.*?\n---",
            "", RegexOptions.Singleline));
        var blockShingles = Shingles(block, 8).ToHashSet();
        var own = Shingles(Reader(Section(CareHeading)), 8)
            .Where(blockShingles.Contains).Where(CarriesContent).ToList();
        Assert.True(own.Count == 0, "the caregiver section restates the block:\n  "
            + string.Join("\n  ", own));
    }

    // -------------------------------------------------------- routing, not restating

    [Fact]
    public void WholeBrainRadiationIsRoutedAndItsDetailIsNotRepeatedHere()
    {
        // RULING 5. /treatments/radiation-therapy already owns whole-brain
        // radiation, hippocampal avoidance, memantine, the trial and R3 stated
        // in its own words. It was written for exactly this reader. This page
        // owns the DECISION and hands over for the rest — and the door points at
        // the anchor, not at the top of a long page.
        var treatment = PlainOf("How is it usually treated?");
        Assert.Contains("/treatments/radiation-therapy#whole-brain-radiation", treatment,
            StringComparison.Ordinal);
        Assert.Matches(new Regex(
            @"it carries a question to ask by name about\s*protecting memory",
            RegexOptions.IgnoreCase), treatment);
        Assert.Matches(new Regex(
            @"Read that before the planning appointment rather than\s*after it",
            RegexOptions.IgnoreCase), treatment);

        // THE DETAIL IS NOT RE-EXPLAINED. A second explanation of hippocampal
        // avoidance is a second strength for one claim (§12.10), and the sibling
        // is where the trial and its no-numbers ruling live.
        // The anatomy PARAPHRASED is the same second strength as the anatomy
        // named: `/review` wrote "the part of the brain that stores new
        // memories" and re-owned the sibling's claim without using its word.
        var explained = new Regex(
            @"\bhippocamp\w*|"
            + @"\bmemantine\b[^.?]{0,80}\b(?:protects?|helps?|works?|lowers?|reduces?|"
            + @"is a medicine)\b|"
            + @"\b(?:part|area|bit) of (?:the|your) brain that (?:stores|makes|handles|holds)\b|"
            + @"\b(?:steer|plan|aim)\w*\s+(?:the (?:beam|radiation|dose)\s+)?(?:around|away from)\s+"
            + @"(?:the |your )?(?:part|area|bit|memory|hippocamp)|"
            + @"\bthe memory (?:part|area) of (?:the|your) brain can be\b|"
            + @"\ba (?:large )?trial (?:tested|showed|found)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(explained, "The hippocampus can be spared by planning the beam.");
        Assert.Matches(explained, "A large trial tested it in people whose cancer had spread.");
        Assert.Matches(explained, "Ask whether the beam can be planned to steer around the part "
            + "of the brain that stores new memories.");
        Assert.Matches(explained, "Memantine is given at the same time and lowers the chance of "
            + "memory trouble.");
        Assert.DoesNotMatch(explained, Plain);

        // The one mention of memantine that IS allowed is in the questions list,
        // as a word to say out loud rather than a thing this page explains.
        var questions = PlainOf("What to ask your team");
        Assert.Matches(new Regex(@"should I be on memantine\?", RegexOptions.IgnoreCase), questions);

        // The sibling still carries what this page is leaning on — read, not
        // assumed (§12.10).
        var radiation = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "radiation-therapy.md")));
        Assert.Contains("hippocampal avoidance", radiation, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("{#whole-brain-radiation}",
            CuratedPage.Read("treatments", "radiation-therapy.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheFourTreatmentsAreAllPresentAndSwellingComesFirst()
    {
        // RULING 10: the source says "limited ... and surgically accessible
        // lesions" and nothing about size or count, so neither does the page.
        // Order matters here: the medicine for swelling is often the fastest
        // thing to change how somebody feels, and a reader who meets surgery
        // first reads the section as being about operations.
        // PARAGRAPH-LEADING bold, from the unflattened section. Counting every
        // bold run counts emphasis inside a paragraph too — the whole-brain
        // entry carries one — and a count that moves when a sentence is
        // emphasised is not counting what it says it is.
        var leads = Regex.Matches(Reader(RawSection("How is it usually treated?")),
                @"(?m)^\*\*(.+?)\*\*")
            .Select(m => m.Groups[1].Value)
            .ToList();

        // AN EXACT COUNT. `>= 5` is a floor, and `/review` added a sixth
        // lead-in — "**Chemotherapy.** … they work in the brain as well" —
        // which contradicts the whole medicine section two screens later and
        // left every `StartsWith` below passing.
        Assert.True(leads.Count == 5,
            $"the treatment section carries {leads.Count} lead-ins, not five:\n  "
            + string.Join("\n  ", leads));
        Assert.StartsWith("Where the line falls", leads[4], StringComparison.Ordinal);
        Assert.StartsWith("Medicine for the swelling", leads[0], StringComparison.Ordinal);
        Assert.Contains(leads, l => l.StartsWith("Surgery", StringComparison.Ordinal));
        Assert.Contains(leads, l => l.StartsWith("Focused radiation", StringComparison.Ordinal));
        Assert.Contains(leads, l => l.StartsWith("Whole-brain radiation", StringComparison.Ordinal));

        var treatment = PlainOf("How is it usually treated?");
        Assert.Matches(new Regex(
            @"when there are only a few spots, when one of them can be\s*reached safely",
            RegexOptions.IgnoreCase), treatment);

        // Every door in the section goes somewhere that exists.
        foreach (var owed in new[]
                 {
                     "/treatments/steroids", "/treatments/craniotomy",
                     "/treatments/radiation-therapy", "/treatments/clinical-trials",
                 })
        {
            Assert.Contains(owed, treatment, StringComparison.Ordinal);
        }
    }

    // --------------------------------------------------------------- outlook

    [Fact]
    public void TheScanLooksWorseClaimIsRoutedWithItsActionIntact()
    {
        // `/review`'s S9, and the shingle guard structurally cannot hold this:
        // a paraphrase shares no eight-word run, so "the swelling that shows up
        // after focused radiation can look exactly like the tumor growing
        // again, and it usually settles by itself" restates the sibling's claim
        // AND drops the half that matters, with every restatement check green.
        //
        // /tests/follow-up-scans#looks-worse owns it, including the part this
        // page must not lose: the swelling is not only a picture, it can make
        // somebody feel different, and that is treatable if it is reported.
        var scans = PlainOf("Follow-up scans, and what to do while you wait");

        Assert.Contains("/tests/follow-up-scans#looks-worse", scans, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"It is not only a change on the screen", RegexOptions.IgnoreCase),
            scans);
        Assert.Matches(new Regex(
            @"Tell somebody at the time rather than saving it for the next scan",
            RegexOptions.IgnoreCase), scans);

        // THE REASSURANCE WITHOUT THE ACTION IS THE DEFECT, so that is what is
        // banned — not the sibling's wording.
        var settlesItself = new Regex(
            @"\b(?:settles?|goes? away|sorts? itself|resolves?|clears? up|calms? down)\b"
            + @"[^.]{0,40}\b(?:by itself|on its own|in time|eventually)\b|"
            + @"\bnothing (?:to do|needs doing) about it\b|"
            + @"\bjust (?:a|the) (?:picture|scan)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(settlesItself,
            "The swelling that shows up after focused radiation can look exactly like the tumor "
            + "growing again, and it usually settles by itself.");
        Assert.Matches(settlesItself, "It is just a picture on the screen.");
        Assert.DoesNotMatch(settlesItself, Plain);

        // And the sibling still carries what this page is leaning on, read
        // rather than assumed (§12.10).
        var sibling = CuratedPage.Read("tests", "follow-up-scans.md");
        Assert.Contains("{#looks-worse}", sibling, StringComparison.Ordinal);
    }

    [Fact]
    public void TheOutlookGateIsClosedAndCarriesNoFigureAndNoFigureInWords()
    {
        // §12.5, and WI-527's lesson that a digit ban is not a prognosis ban.
        //
        // EVERY LINE-BREAK PATTERN HERE IS `\r?\n`. The meningioma copy of this
        // guard used `\s*\n\n` and could not match a CRLF checkout at all —
        // greedy `\s*` eats both breaks and every shorter split leaves a `\r`
        // where a `\n` is required. The break harness cannot see that class of
        // bug, because a test that is broken on CRLF fails for free.
        var raw = Regex.Match(Page, @"^## What might happen over time.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(raw), "the outlook section has gone");

        Assert.Matches(new Regex(
            @"^## What might happen over time[ \t]*\r?\n[ \t]*\r?\n:::outlook",
            RegexOptions.Multiline), raw);
        Assert.Matches(new Regex(@"\r?\n:::\r?\n"), raw);

        var inside = CuratedPage.Flatten(
            Regex.Match(raw, @":::outlook(.*?)\r?\n:::", RegexOptions.Singleline).Groups[1].Value);
        Assert.False(string.IsNullOrWhiteSpace(inside), "the gate is empty");

        // Nothing sits outside the gate in that section but the heading. The
        // §12.5 warning text is rendered BY the container, so writing it by hand
        // above the gate would show it to the reader twice.
        var outside = raw
            .Replace(Regex.Match(raw, @":::outlook.*?\r?\n:::", RegexOptions.Singleline).Value, "",
                StringComparison.Ordinal)
            .Replace("## What might happen over time", "", StringComparison.Ordinal);
        Assert.True(string.IsNullOrWhiteSpace(outside),
            "something sits outside the outlook gate in that section: " + outside.Trim());
        Assert.DoesNotContain("The next part is about outlook", raw, StringComparison.OrdinalIgnoreCase);

        // No digits at all in there: unlike a glioma hub, this page has no grade
        // inside the gate, so there is nothing a digit could legitimately be.
        Assert.DoesNotMatch(new Regex(@"\d"), inside);

        // AND NO FIGURE-SHAPED CLAIM IN WORDS.
        // A VOCABULARY LIST IS NOT A CLAIM BAN, and WI-527 wrote that lesson
        // down one item ago: `/review` put "Plenty of people are still doing
        // well many years later" inside the gate, which is an outcome claim
        // with a time horizon and contains none of the banned words. So the
        // second half of this pattern bans the SHAPE: a quantity of people,
        // an outcome, and a length of time.
        var told = new Regex(
            @"\blive as long as\b|\blife expectancy\b|\blifespan\b|\bsurviv\w*|\bmonths to live\b|"
            + @"\bterminal\b|\bcure[ds]?\b|\b(?:almost|nearly) (?:everybody|everyone|all of them)\b|"
            + @"\bmost people\b|\bthe vast majority\b|\brun out of\b|"
            + @"\b(?:plenty of|many|some|a lot of|most) (?:people|patients)\b[^.]{0,60}"
            + @"\b(?:years|months|decades|later|still)\b|"
            + @"\b(?:doing|do) well\b|\bstill (?:here|going|with us)\b|"
            + @"\b(?:years|decades) later\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(told, "Most people live as long as anybody else.");
        Assert.Matches(told, "Survival is measured in months.");
        Assert.Matches(told, "Plenty of people are still doing well many years later.");
        Assert.DoesNotMatch(told, inside);

        // The reason this gate exists HERE, which no other hub has: the figures
        // date faster than anywhere else, and they are about the first cancer.
        Assert.Matches(new Regex(
            @"Here they go out of date faster than almost anywhere else in cancer",
            RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(
            @"the figures are usually about the first cancer, not about the brain",
            RegexOptions.IgnoreCase), inside);
    }

    // ---------------------------------------------------------- housekeeping

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3. The two page-specific sections (the team question and the
        // medicine ask) sit where they are for a reason: both are ACTIONS, and
        // §12.3 puts actions before anything frightening.
        var headings = Headings();

        // EVERY SLOT BY NAME FIRST. `List.IndexOf` returns -1 for a heading that
        // is not there, and three of the order assertions below are `>`, so
        // `/review` deleted the entire everyday-life section — and with it every
        // route to the seizure pages and the driving authority — and the test
        // stayed green because -1 is less than everything.
        foreach (var owed in new[]
                 {
                     "The short version", "What is a brain metastasis?", NamingHeading,
                     "Where does it grow, and why does it cause these symptoms?",
                     "What symptoms does it cause?", "How do doctors find out it is this?",
                     "What do the words on my report mean?", TeamHeading,
                     "How is it usually treated?", MedicineHeading, CoveringHeading,
                     "What is treatment actually like, and what is normal afterwards?",
                     "Everyday life: work, driving, seizures and tiredness",
                     "Follow-up scans, and what to do while you wait", "If it comes back, or changes",
                     BlameHeading, "What might happen over time", CareHeading,
                     "What to ask your team", "Where to get support",
                 })
        {
            Assert.True(headings.Contains(owed), $"the page has lost its \"{owed}\" section");
        }

        // §12.3's DELIBERATE OMISSIONS stay omitted. "How common is it?" is
        // named in the design doc as filler that competes for the 20 to 28% of
        // a page that gets read, and every relative-order check below survives
        // its insertion.
        foreach (var banned in new[] { "How common is it?", "What causes it?", "Epidemiology" })
        {
            Assert.DoesNotContain(banned, headings);
        }

        Assert.Equal("The short version", headings[0]);
        Assert.Equal("What is a brain metastasis?", headings[1]);
        Assert.Equal(NamingHeading, headings[2]);

        // The medicine ask is the page's action section and sits immediately
        // after treatment, before anything about what treatment feels like.
        Assert.Equal(headings.IndexOf("How is it usually treated?") + 1,
            headings.IndexOf(MedicineHeading));

        // The team question comes BEFORE treatment, because knowing who to ask
        // is what makes the treatment conversation possible.
        Assert.True(headings.IndexOf(TeamHeading) < headings.IndexOf("How is it usually treated?"),
            "the team question belongs above the treatment options");

        // Self-blame is not near the top (§12.3's deliberate demotion), and
        // outlook sits after everything actionable.
        Assert.True(headings.IndexOf(BlameHeading) > headings.IndexOf("Everyday life: work, driving, seizures and tiredness"));
        Assert.True(headings.IndexOf("What might happen over time") > headings.IndexOf(BlameHeading));
        Assert.True(headings.IndexOf(CareHeading) > headings.IndexOf("What might happen over time"));
        Assert.Equal("Where to get support", headings[^1]);
    }

    [Fact]
    public void ThePageNeverMinimisesOrCharacterisesTheDiagnosis()
    {
        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Page), Slug);

        foreach (var banned in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(banned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // And the over-reassurance this page in particular could reach for:
        // "spread" is the word the reader is most frightened of, and softening
        // it is §12.12's more dangerous direction.
        var soft = new Regex(
            @"\bnothing to worry about\b|\bjust a few spots\b|\bonly a small\b|"
            + @"\btry not to (?:worry|panic|dwell)\b|\bthe good (?:part|thing|news)\b|"
            // `sounds worse than it is`, in every arrangement: `/review` beat a
            // list of literals with "It sounds far worse than it usually turns
            // out to be."
            + @"\bsounds?\b[^.]{0,30}\bworse than\b|\bworse than it (?:is|sounds|turns out)\b|"
            + @"\bnot as bad as\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(soft, "It is just a few spots, so try not to worry.");
        Assert.Matches(soft, "It sounds far worse than it usually turns out to be.");
        Assert.Matches(soft, "It is not as bad as it sounds.");
        Assert.DoesNotMatch(soft, Plain);

        // "stay positive" is banned only where the page is NOT refusing it. The
        // best paragraph on the page is the one that names the myth in order to
        // take it apart, and a ban that fires on the refutation is the WI-508
        // defect (§12.14: the answer is not to drop the ban).
        const string Refusal =
            "The idea that staying positive changes the outcome puts the blame in the worst "
            + "possible place, and it is not true";
        Assert.Matches(new Regex(Regex.Escape(Refusal), RegexOptions.IgnoreCase), Plain);
        Assert.DoesNotMatch(new Regex(@"\bstay positive\b|\bpositive (?:attitude|thinking)\b",
            RegexOptions.IgnoreCase), Redact(Plain, [Refusal]));
    }

    [Fact]
    public void OnlyOneSectionClaimsToBeTheOneThatMatters()
    {
        // §12.8's WI-526 defect, found again at twice the size: SIX sections
        // each told the reader they were the important one, or the thing nobody
        // else tells you. Six is none. There was no guard for it on that item;
        // there is one now, and it counts rather than lists.
        var uniqueness = new Regex(
            @"\bif you read one (?:section|thing|part)\b|"
            + @"\bthe (?:one|part|thing) (?:that )?(?:matters most|to hold on to)\b|"
            + @"\beverything else follows from it\b|"
            + @"\b(?:that )?nobody (?:explains|warns|tells you|mentions)\b|"
            + @"\balmost nothing tells you\b|\bthe most important (?:part|thing|section)\b|"
            + @"\bmore than anything else on this page\b|"
            + @"\bthis is the part most worth\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "If you read one section here, make it this one.",
                     "Here is the part to hold on to.",
                     "Something changes here that nobody explains.",
                     "And there is a second half almost nothing tells you.",
                     "And a thing nobody warns people about.",
                 })
        {
            Assert.Matches(uniqueness, known);
        }

        // ZERO, NOT ONE. `<= 1` looked like "the page is allowed one", but the
        // sentence the page actually keeps is not in the banned shape at all —
        // so the allowance was a free extra, and the harness used it twice.
        // The shape is banned outright, and the surviving claim is pinned
        // separately below because it is a different kind of sentence: it says
        // what the section CONTAINS, not that the section beats the others.
        var claims = CuratedPage.SentencesOf(Plain).Where(s => uniqueness.IsMatch(s)).ToList();
        Assert.True(claims.Count == 0,
            $"{claims.Count} sections claim to be the one that matters, so none of them does:\n  "
            + string.Join("\n  ", claims));

        // And the one sentence that does point at a section points at the one
        // with an action in it, which is the only kind worth the reader's
        // attention budget.
        Assert.Matches(new Regex(
            @"This is the section with something in it to actually do", RegexOptions.IgnoreCase),
            PlainOf(MedicineHeading));
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        foreach (var form in CuratedPage.BritishForms)
        {
            if (CuratedPage.BritishFormExemptions.Contains(form))
            {
                continue;
            }

            // `Plain`, not `Body`. ReaderText strips the front matter, and the
            // description renders as the first paragraph the reader meets — so
            // `description: "...a tumour that has spread..."` was green
            // (§12.8, WI-524; this file's own `Headline` helper exists for
            // exactly that reason and this guard had not been given it).
            Assert.DoesNotMatch(new Regex(@"\b" + Regex.Escape(form), RegexOptions.IgnoreCase),
                Plain);
        }

        // The regulator names too, which a US reader cannot act on.
        foreach (var name in new[] { "MHRA", "NICE guideline", "NHS trust" })
        {
            Assert.DoesNotContain(name, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // THREE IDIOMS `BritishForms` DOES NOT HOLD, two of them on symptom
        // lists. In US English "feeling sick" means unwell rather than
        // nauseated, which is the defect §12.10 records WI-563 fixing in the
        // escalation block; "passing water" appears nowhere else in the corpus
        // and this page would have introduced it.
        foreach (var idiom in new[] { "passing water", "being sick", "go to hospital" })
        {
            Assert.DoesNotContain(idiom, Plain, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotMatch(new Regex(@"\bfeeling sick\b(?!\s+to your stomach)",
            RegexOptions.IgnoreCase), Plain);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        foreach (var owed in new[] { "title:", "slug:", "description:", "tags:", "sources:",
                                     "reviewed:", "review_due:", "disclaimers:" })
        {
            Assert.Contains(owed, front, StringComparison.Ordinal);
        }

        // Every source has a title and an accessed date.
        var urls = Regex.Matches(front, @"(?m)^\s*- url: (\S+)\s*$").Select(m => m.Groups[1].Value)
            .ToList();
        Assert.True(urls.Count >= 7, $"only {urls.Count} sources, which is fewer than were checked");
        Assert.Equal(urls.Count, Regex.Matches(front, @"(?m)^\s+title: "".+""\s*$").Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"(?m)^\s+accessed: \d{4}-\d{2}-\d{2}\s*$").Count);

        // The sources the rulings rest on, by name.
        foreach (var owed in new[]
                 {
                     "PMC6267666",  // 19.0%, the figure that replaces the backlog's range
                     "PMC8750580",  // the brain lesion can differ from the primary
                     "PMC3523178",  // three taps do not rule it out
                     "NBK499862",   // the cranial nerve picture
                     "NBK470246",   // the treatment shape and the team sentence
                 })
        {
            Assert.Contains(owed, front, StringComparison.Ordinal);
        }

        // THE KNOWN-DEAD AND BANNED DOMAINS ARE NOT CITED. ascopubs.org carries
        // the guideline that covers this subject best and is unreachable, which
        // is exactly when a citation gets added without being read.
        foreach (var dead in new[]
                 {
                     "academic.oup.com", "mdpi.com", "ascopubs.org", "sciencedirect.com",
                     "journals.lww.com", "medscape.com", "mayoclinic.org", "hopkinsmedicine.org",
                     "ajnr.org",
                 })
        {
            Assert.DoesNotContain(dead, front, StringComparison.OrdinalIgnoreCase);
        }

        // §12.1's ACTUAL BAN, which a domain list cannot express here: NCI
        // PATIENT PDQ is the source never to cite on a tumor hub, and this page
        // must cite cancer.gov for its framing sentence — so the dead-domain
        // loop above structurally cannot hold the rule. `/review` added the
        // adult-brain PDQ URL and every check passed.
        var pdq = new Regex(@"cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", RegexOptions.IgnoreCase);
        Assert.Matches(pdq,
            "  - url: https://www.cancer.gov/types/brain/patient/adult-brain-treatment-pdq");
        Assert.DoesNotMatch(pdq, front);

        // §12.1: NCI is cited, and the front matter says in so many words that
        // it is cited for framing only. A reader of this file in six months is
        // the person that note is for.
        Assert.Contains("cancer.gov/types/metastatic-cancer", front, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"NCI is cited here for FRAMING ONLY", RegexOptions.IgnoreCase),
            front);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // §12.8 (WI-521): corpus-wide, not hand-picked neighbours. This page was
        // written days after /tumors/meningioma and lifted nine passages from it
        // before this guard ran, which is the argument for pointing it at
        // everything rather than at the obvious siblings.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("brain-metastases.md", StringComparison.Ordinal))
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

    /// <summary>
    /// Word runs shared with another page ON PURPOSE (§12.10). Kept tiny: each
    /// is a link label or a prompt that must read identically wherever a reader
    /// meets it.
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // THE CORD-COMPRESSION RED FLAGS, word for word. §12.10's "one claim,
        // one strength" applied to the sentence with the shortest fuse in the
        // corpus: a reader who meets it on /tumors/spinal-cord-tumor, on
        // /tumors/meningioma and here must meet the same words, because a
        // rewrite is a rewording of an emergency threshold. Sharing this is the
        // fix `/review` asked for, not a restatement to be excused.
        "new weakness in the legs, a change in how you walk, numbness around the "
        + "saddle area, and any new trouble with your bladder or bowel. **New weakness "
        + "or new bladder trouble is a reason to be seen quickly, not to wait for the "
        + "next appointment.** Do not wait for it to get bad first, because the early "
        + "signs are often vague ones. [Spinal cord tumors](/tumors/spinal-cord-tumor)",

        // The standard support-section door, worded as every hub words it. The
        // heading is included because the window straddles it and the first
        // bullet, which is true on every page that has both.
        "Get help now if you want to talk to a person today.",
        "## Where to get support - [Get help now](/get-help-now) if you want to talk to a "
        + "person today.",

        // §12.3's section headings. A hub that renames them to dodge a shingle
        // check has broken the template to satisfy a test.
        "Where does it grow, and why does it cause these symptoms?",
        "What symptoms does it cause?",
        "How do doctors find out it is this?",
        // The escalation block is the last thing in the symptoms section on
        // every hub, so the window straddles its directive and the next
        // heading. Structure again, not prose.
        "[ESCALATION] ## How do doctors find out it is this?",
        "What do the words on my report mean?",
        "How is it usually treated?",
        "What is treatment actually like, and what is normal afterwards?",
        "Everyday life: work, driving, seizures and tiredness",
        "Follow-up scans, and what to do while you wait",
        "If it comes back, or changes",
        "What might happen over time",

        // THE OUTLOOK GATE'S OPENING. §12.5 prescribes it, and a reader meeting
        // it on a second hub has to recognise it — so it is deliberately
        // identical rather than rewritten, which is §12.10's "must not have two
        // strengths" applied to a consent prompt. The heading and the container
        // marker are included because the window straddles them.
        "## What might happen over time :::outlook Doctors sometimes describe outlook using "
        + "numbers. It is worth knowing what those numbers are before you decide whether you "
        + "want them.",

        "For the person caring for someone with this",
        // The heading runs straight into the directive, so the eight-word window
        // straddles them on every hub that owes the block. That is structure,
        // not prose.
        "For the person caring for someone with this [CAREGIVER] Three things",
        "What to ask your team",
        "Where to get support",
        "And why does that cause symptoms at all?",
        // The mechanism sub-heading, its directive, and the heading after it:
        // three pieces of §12.3 structure with no prose between them.
        "### And why does that cause symptoms at all? [MECHANISM] ## What symptoms does it cause?",
    ];

    private static readonly HashSet<string> AllowedShingles =
        DeliberatelyShared.SelectMany(s => Shingles(s, 8)).ToHashSet();

    private static IEnumerable<string> Shingles(string text, int size)
    {
        // A token has to START with a letter or a digit. `[a-z0-9'-]+` alone
        // treats a bare list-marker hyphen as a word, so a window slides off the
        // end of one bullet and onto the punctuation of the next — and the run
        // it reports ("...to a person today -") belongs to no sentence anybody
        // wrote. Markdown syntax is not prose.
        var words = Regex.Matches(CuratedPage.Flatten(text).ToLowerInvariant(), @"[a-z0-9][a-z0-9'-]*")
            .Select(m => m.Value)
            .ToArray();
        for (var i = 0; i + size <= words.Length; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(size));
        }
    }

    private static readonly HashSet<string> Stop =
        new(["a", "an", "the", "and", "or", "but", "of", "to", "in", "on", "at", "for", "with",
             "from", "by", "is", "are", "was", "were", "be", "been", "being", "it", "its", "this",
             "that", "these", "those", "you", "your", "they", "them", "their", "we", "our", "as",
             "if", "so", "not", "no", "than", "then", "there", "here", "what", "which", "who",
             "when", "where", "why", "how", "all", "any", "both", "each", "more", "most", "other",
             "some", "such", "only", "own", "same", "too", "very", "can", "will", "just", "do",
             "does", "did", "have", "has", "had", "having", "i", "me", "my", "one", "two"]);

    /// <summary>
    /// A shingle with fewer than three content words is grammar, not content.
    /// </summary>
    private static bool CarriesContent(string shingle) =>
        shingle.Split(' ').Count(w => !Stop.Contains(w)) >= 3;
}

[Collection(DatabaseCollection.Name)]
public sealed class BrainMetastasesPageRenderTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/brain-metastases";

    private HttpClient Client => factory.CreateClient();

    [Fact]
    public async Task ThePageRenders()
    {
        var response = await Client.GetAsync(Url);
        Assert.True(response.IsSuccessStatusCode, $"{Url} returned {(int)response.StatusCode}");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Brain metastases", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The short version", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheOutlookGateRendersClosed()
    {
        var html = await Client.GetStringAsync(Url);

        // SCOPED TO THE OUTLOOK SECTION. Matching the first `<details>` in the
        // document reads whatever disclosure the layout happens to put above
        // the article — the mobile nav is one — and would have kept passing
        // while the gate itself rendered open.
        var start = html.IndexOf("What might happen over time", StringComparison.OrdinalIgnoreCase);
        Assert.True(start > 0, "the outlook heading did not render");
        var after = html[start..];

        var gate = Regex.Match(after, @"<details[^>]*>");
        Assert.True(gate.Success, "the outlook gate did not render as a details element");
        Assert.DoesNotContain(" open", gate.Value, StringComparison.Ordinal);

        // And the §12.5 warning is the container's, rendered once.
        Assert.Equal(1, Regex.Matches(html, "The next part is about outlook").Count);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves()
    {
        // The anchor id, not the heading text (§12.8): the helper looks the
        // section up by id and passing prose silently checks nothing.
        await CuratedPage.AssertLinksResolveIn(
            Client, Url, "where-to-get-support",
            "/get-help-now", "/treatments/steroids", "/treatments/radiation-therapy",
            "/treatments/clinical-trials", "/tumors");
    }

    [Fact]
    public async Task TheLeptomeningealAnchorIsReachable()
    {
        var html = await Client.GetStringAsync(Url);
        Assert.Contains("id=\"leptomeningeal\"", html, StringComparison.Ordinal);
        await CuratedPage.AssertFragmentLinksResolve(Client, Url);

        // EVERY DEEP LINK BY NAME. The helper is satisfied by any one fragment
        // link resolving, so dropping `#driving` or `#looks-worse` and leaving
        // `#whole-brain-radiation` behind was green — and each of those three
        // anchors is the whole reason the door exists rather than a link to the
        // top of a long page.
        foreach (var deep in new[]
                 {
                     "/treatments/radiation-therapy#whole-brain-radiation",
                     "/seizures/living-with#driving",
                     "/tests/follow-up-scans#looks-worse",
                     "/treatments/chemotherapy#fever-rule",
                 })
        {
            Assert.Contains(deep, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheSharedBlocksRenderTheirContentRatherThanTheirDirective()
    {
        var html = await Client.GetStringAsync(Url);

        foreach (var directive in new[]
                 { "[MECHANISM]", "[ESCALATION]", "[CAREGIVER]", "[TUMOR-BOARD]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        Assert.Contains("When to call for help right now", html, StringComparison.OrdinalIgnoreCase);
        // The caregiver block was the one directive whose CONTENT nothing
        // asserted, so contract item 11 could break in silence.
        Assert.Contains("Two people in the room hear more than one", html,
            StringComparison.OrdinalIgnoreCase);
        // Not "A tumor board is a meeting": "tumor board" is a glossary term and
        // the renderer wraps it, so the phrase a test looks for must not straddle
        // the auto-linked words.
        Assert.Contains("is a meeting where specialists look at one person's case", html,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("The skull is a closed box", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TheExcludedBlocksDoNotAppearInTheRenderedPage()
    {
        // The rendered proof of the content test: a reader must not meet "For
        // most brain tumors, nobody knows the cause" on a page about a tumor
        // whose cause they already know.
        var html = await Client.GetStringAsync(Url);
        Assert.DoesNotContain("nobody knows the cause", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("The rules for naming brain tumors changed in 2021", html,
            StringComparison.OrdinalIgnoreCase);
    }
}
