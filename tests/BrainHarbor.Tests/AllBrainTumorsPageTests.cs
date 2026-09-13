using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-529: <c>/tumors/all-brain-tumors</c>, "we don't have a name for it yet".
/// A NEW page rather than a deepening, and the only page in the corpus written
/// for a reader who has no diagnosis at all.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * SAYING WHAT THE THING IS. Every other page in <c>/tumors/</c> names a
///     diagnosis. This one may not, in its own voice, anywhere — and the two
///     shared blocks it includes say "your tumor" six times between them, so
///     the page owes an explicit scoping note above the first of them.
///   * RESTATING THE LAB QUEUE. <c>/tests/waiting-for-results</c> (WI-507)
///     already owns the durations, the UK national audit, the batching and the
///     name changing. This page owns steps 1 to 4 and routes for 5 to 7; a
///     second copy of a number is a second copy to keep in step (WI-508).
///   * NCI PATIENT PDQ. The research pack sources five of this page's claims to
///     the adult-brain PDQ, which §12.1 bans on a tumor hub.
///   * "BENIGN" READ AS "HARMLESS", which is the one over-reassurance on this
///     page whose correction is the section's whole reason for existing.
///   * A PROGNOSIS, in figures or in words, for a reader who has no diagnosis
///     to attach one to.
///   * A SECOND-OPINION DISAGREEMENT RATE. The research pack attributes
///     "major disagreements occurred in 12% of cases" to PMID 25972322, which
///     reports no disagreement rate at all (it counts why cases were referred).
///     No figure is published and the ban is checked against the CLAIM as well
///     as the citation (§12.14).
///
/// Every guard starts from §12.8's WI-527 and WI-528 lessons rather than
/// rediscovering them: <c>CountWord</c> knows the teens, quantifiers allow a
/// word gap, numeric bans are threshold-shaped, phrase guards run on
/// <c>Plain</c> so the description is in scope, every block directive is named,
/// section order asserts PRESENCE before position, and every regex that touches
/// a line break uses <c>\r?\n</c> — never a bare <c>\n</c> and never
/// <c>\s*</c> standing in for a blank line.
/// </summary>
public sealed class AllBrainTumorsPageContentTests
{
    private const string Slug = "tumors/all-brain-tumors";

    private const string ToldHeading = "What have you actually been told?";
    private const string StepsHeading = "What happens next";
    private const string ImagingHeading = "Why the scan cannot say on its own";
    private const string MechanismHeading = "Where is this coming from?";
    private const string CallHeading = "When to call, and what for";
    private const string TissueHeading = "Will I need a piece of it taken?";
    private const string WaitHeading = "How long until it has a name?";
    private const string BenignHeading = "Why \"benign\" is the wrong comfort word here";
    private const string StageHeading = "Why nobody has given you a stage";
    private const string ForkHeading = "Did it start in my brain, or somewhere else?";
    private const string BoardHeading = "The meeting about your case";
    private const string OpinionHeading = "Asking somebody else to look";
    private const string WeekHeading = "What you can do this week";
    private const string BlameHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    // The STANDARD heading, not a better-fitting one. §12.7 requires the same
    // words on every hub so a reader who learns the shape on one finds it on
    // the next, and `CaregiverSectionTests` enforces it across the directory.
    // WI-529's first draft used "For the person waiting with them", which reads
    // better here and would have been the first crack in an eighteen-page
    // standard.
    private const string CareHeading = "For the person caring for someone with this";

    private static string Page => CuratedPage.Read("tumors", "all-brain-tumors.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The section with its line breaks intact. <c>CuratedPage.Section</c>
    /// collapses all whitespace to single spaces, so anything that needs to see
    /// a PARAGRAPH — a count of a section's own claims, for instance — has to
    /// cut the markdown itself (§12.8, WI-528). <c>\r?\n</c>, never a bare
    /// <c>\n</c>.
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
    /// Title and description. <c>ReaderText</c> strips the front matter, so a
    /// guard scoped to the body is blind to them while the template renders the
    /// description as the first paragraph a reader meets (§12.8, WI-524 and
    /// WI-528: the British-form scan ran on <c>Body</c> and missed a "tumour"
    /// sitting in a shipped description).
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
    /// word defeats a phrase guard structurally, and no vocabulary fixes that).
    /// </summary>
    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    /// <summary>
    /// The page as a reader meets it, blocks resolved. A rule about a BLOCK's
    /// prose asserted against the raw page is asserting against the literal
    /// string "[MECHANISM]" (§12.10, WI-514).
    /// </summary>
    private static string Composed =>
        Regex.Replace(
            CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Composed(Page, Slug))),
            @"[*_]", "");

    /// <summary>
    /// Thirteen through nineteen are not optional: every copy of this constant
    /// in the corpus ran <c>one…twelve, twenty, thirty…</c> until WI-528 found
    /// the gap. <c>dozen</c> and <c>couple</c> are here because a count a reader
    /// can act on does not have to be a numeral or a decade.
    /// </summary>
    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?)";

    /// <summary>The page's own "## " headings, in order, anchors stripped.</summary>
    private static List<string> Headings() =>
        [.. Regex.Matches(Page, @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())];

    /// <summary>
    /// The URL VALUES from the front matter, without the comments around them.
    /// A ban run over the whole front matter fires on this page's own rulings,
    /// which quote the banned URL in order to explain why it is banned — so the
    /// guard would be red on a correct page, which is worse than no guard
    /// (§12.8).
    /// </summary>
    private static string CitedUrls() =>
        string.Join("\n", Regex.Matches(CuratedPage.FrontMatter(Page), @"(?m)^  - url: (\S+)")
            .Select(m => m.Groups[1].Value));

    /// <summary>
    /// Removes an allowed phrase from the text instead of exempting the whole
    /// sentence that contains it (§12.8, WI-527: a substring allowlist is a free
    /// ride for everything else in the sentence).
    /// </summary>
    private static string Redact(string text, IEnumerable<string> allowed)
    {
        foreach (var phrase in allowed)
        {
            text = Regex.Replace(text, Regex.Escape(phrase), " ", RegexOptions.IgnoreCase);
        }

        return text;
    }

    private static string Sibling(params string[] path) =>
        Regex.Replace(CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(path))), @"[*_]", "");

    // ------------------------------------------------------- the page's spine

    [Fact]
    public void TheSevenStepPathwayIsThereInOrderAndTellsTheReaderTheyCanAskWhichStep()
    {
        // The item's whole reason for existing: the reader experiences this as
        // silence, and naming the steps turns "nothing is happening" into
        // "step 3 of 7". Seven numbered items, each pinned by the phrase that
        // makes it that step rather than by its number, so renumbering cannot
        // quietly drop one.
        var steps = PlainOf(StepsHeading);

        string[] ordered =
        [
            "A scan finds something",
            "A better picture",
            "A referral to a specialist team",
            "A decision about tissue",
            "The first look",
            "The lab work",
            "A plan",
        ];

        var at = -1;
        foreach (var step in ordered)
        {
            var index = steps.IndexOf(step, StringComparison.OrdinalIgnoreCase);
            Assert.True(index > at, $"'{step}' is missing from the pathway or is out of order");
            at = index;
        }

        // Seven list items, and exactly seven: an eighth would break the "step 3
        // of 7" framing the short version promises, and a count is the only
        // thing that can see that.
        var items = Regex.Matches(RawSection(StepsHeading), @"(?m)^\d+\. ").Count;
        Assert.True(items == ordered.Length,
            $"the pathway has {items} numbered steps but the page promises {ordered.Length}");

        // THE ACTIONABLE HALF. A list of steps a reader cannot locate themselves
        // on is a list; the question is what makes it a map.
        Assert.Matches(new Regex(@"you can ask which number you are on", RegexOptions.IgnoreCase),
            steps);

        // And the two honesties that stop the list becoming a promise.
        Assert.Matches(new Regex(@"Some people stop at step 4", RegexOptions.IgnoreCase), steps);
        Assert.Matches(new Regex(@"do not always run in that order", RegexOptions.IgnoreCase), steps);

        // The short version has to carry the framing, or the reader who stops
        // after three sentences never meets the one useful idea on the page.
        Assert.Matches(new Regex(@"seven steps", RegexOptions.IgnoreCase),
            PlainOf("The short version"));
    }

    [Fact]
    public void ThePageNeverSaysWhatTheThingIs()
    {
        // THE CENTRAL SAFETY PROPERTY, and the one every other /tumors page is
        // allowed to break. Asserted against the page's OWN voice — the raw
        // markdown, not the composed page — because the shared blocks are
        // written for a reader who has a diagnosis and the page scopes them
        // explicitly instead (see the next test).
        //
        // On PLAIN, not on Body: the title and the description are the first
        // words a reader meets and a body-scoped guard cannot see them (§12.8,
        // WI-528). The scoping note QUOTES the phrase it is teaching the reader
        // to discount, so it is redacted rather than exempted (§12.8, WI-527) —
        // an allowlist over that sentence would hand the rest of the sentence a
        // free ride.
        var own = Redact(Plain, ["\"your tumor\""]);

        var asserts = new Regex(
            @"\byou have a (?:brain )?tumor\b|"
            + @"\bwhat you have is\b|"
            // The bare possessive, not the possessive plus a verb: "where your
            // tumor sits decides what you feel" is the natural rewrite of this
            // page's own careful sentence and walked through the first version.
            + @"\byour (?:brain )?tumou?rs?\b|"
            + @"\bthis is (?:a |your )?(?:brain )?(?:tumor|cancer)\b|"
            + @"\bit is (?:a )?(?:brain )?(?:tumor|cancer)\b|"
            + @"\bit is not (?:a )?(?:brain )?(?:tumor|cancer)\b|"
            + @"\b(?:the|your) (?:tumor|cancer) (?:you have|in your brain)\b|"
            + @"\bsince you have a (?:brain )?tumor\b",
            RegexOptions.IgnoreCase);

        // Proof the guard can fire, in the shapes a draft would actually use.
        foreach (var known in new[]
                 {
                     "Now that you have a brain tumor, the next question is the grade.",
                     "Your tumor is somewhere near the front of the brain.",
                     "This is a brain tumor, and here is what happens next.",
                     "The good news is it is not cancer.",
                     "The tumor in your brain will be treated by a neurosurgeon.",
                     "Where your tumor sits decides what you feel.",
                     // THE REDACTION CANARY. Removing the quoted phrase must not
                     // exempt the sentence that contains it.
                     "Read \"your tumor\" as a fact: your tumor is near the front.",
                 })
        {
            Assert.Matches(asserts, Redact(known, ["\"your tumor\""]));
        }

        Assert.DoesNotMatch(asserts, own);

        // The redaction removed the quoted phrase and nothing around it: if it
        // had swallowed the sentence, every assertion above would pass for the
        // wrong reason.
        Assert.Contains("Read those words as", own, StringComparison.OrdinalIgnoreCase);

        // And the page says the refusal out loud, twice, in the two places a
        // reader actually stops: the opening and the section that explains why.
        Assert.Matches(new Regex(@"That is not a diagnosis", RegexOptions.IgnoreCase),
            PlainOf("The short version"));
        Assert.Matches(new Regex(@"It is not a diagnosis either", RegexOptions.IgnoreCase),
            PlainOf(ToldHeading));
        Assert.Matches(new Regex(@"It does not show what the cells are", RegexOptions.IgnoreCase),
            PlainOf(ToldHeading));
    }

    [Fact]
    public void TheScopingNoteForTheWordTumorSitsAboveTheFirstSharedBlock()
    {
        // §12.10, and the direction WI-528 did not have to handle. Neither
        // [MECHANISM] nor [CAREGIVER] is FALSE here — the closed-skull material
        // is true of anything that takes up room — but both address a reader
        // who has been given a name, and between them they say "your tumor",
        // "what your tumor is" and "for this tumor". On the one page in the
        // corpus whose reader has no name, that is an assertion the page spends
        // three sections refusing to make.
        //
        // The blocks are NOT edited: doing that would push this page's answer
        // onto eighteen hubs where the reader does have a diagnosis (the WI-514
        // blast radius). The page scopes them instead, ONCE, above the first
        // one — and the position is the property, because a note underneath the
        // block it scopes has already let the reader past it.
        // FLATTENED, and that is load-bearing rather than tidy: the corpus is
        // hard-wrapped, so `says "your tumor" in places` has a newline inside it
        // and a raw match walks straight past — the same wrap that defeated
        // WI-511's first British-forms gate. Positions inside the flattened
        // string keep their order, which is all the assertion below needs.
        var section = CuratedPage.Flatten(RawSection(MechanismHeading));

        var note = Regex.Match(section,
            @"written for somebody further along than you are.*?"
            // (1) THE BLOCK'S OPENING FRAMING. `/review` found the first version
            // of this note calibrated to a narrower defect: it repaired the
            // words "your tumor" and left mechanism.md's first sentence — "Most
            // people are told what they have long before anyone explains what it
            // is doing" — asserting, three lines under "Nobody knows yet whether
            // that is what you have", that most people already have a name. That
            // is the WI-528 class recurring on the page most exposed to it.
            + @"For you that is the other way round.*?"
            // (2) The possessive, which reaches into the caregiver block too.
            + @"says ""your tumor"" in several places.*?"
            + @"Read those words as ""whatever this turns out to be\.""",
            RegexOptions.IgnoreCase);
        Assert.True(note.Success,
            "the section that includes [MECHANISM] no longer scopes BOTH the block's opening "
            + "framing and the words \"your tumor\", so a shared block asserts a diagnosis this "
            + "page refuses to make");

        var directive = section.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(directive > 0, "[MECHANISM] is not in this section");
        Assert.True(note.Index < directive,
            "the scoping note has drifted BELOW [MECHANISM] — a reader meets \"your tumor\" first");

        // The block still says both things the note is about. If a later item
        // rewrites the block to drop them, this note becomes a puzzle and the
        // test says so rather than sitting green.
        var mechanism = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md")));
        Assert.Contains("your tumor", mechanism, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Most people are told what they have long before anyone explains",
            mechanism, StringComparison.OrdinalIgnoreCase);

        // And the note has to reach the caregiver block, which says "for this
        // tumor" ten sections lower with nothing else in view.
        var caregiver = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")));
        Assert.Contains("this tumor", caregiver, StringComparison.OrdinalIgnoreCase);
        Assert.Matches(new Regex(@"caregiver section near the end", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheMapPointerPointsAtTheSectionTheBlockActuallyMakes()
    {
        // WI-528's lesson, on the page most likely to repeat it: a composed
        // block MOVES the thing a pointer points at. The brain map lives inside
        // [MECHANISM] under the block's OWN "## " heading, so it is not "at the
        // end of this section" — it is the next section, and the pointer is
        // read against the block file rather than against a belief about it.
        var mechanism = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md"));
        var blockHeading = Regex.Match(mechanism, @"(?m)^##\s+(.+?)\s*$").Groups[1].Value.Trim();
        Assert.False(string.IsNullOrWhiteSpace(blockHeading),
            "blocks/mechanism.md no longer has a '## ' heading, so it no longer starts a section "
            + "and this page's pointer is wrong in the other direction");

        Assert.Matches(new Regex(@"The next section is a map", RegexOptions.IgnoreCase),
            PlainOf(MechanismHeading));

        // And the block's heading really is the next one in the COMPOSED page.
        var composedHeadings = Regex.Matches(
                CuratedPage.Composed(Page, Slug), @"(?m)^##\s+(.+?)\s*(?:\{#.*\})?\s*$")
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();
        var here = composedHeadings.IndexOf(MechanismHeading);
        Assert.True(here >= 0, $"the composed page has no '{MechanismHeading}' section");
        Assert.True(here + 1 < composedHeadings.Count,
            "nothing follows the mechanism section, so the pointer points at nothing");
        Assert.Equal(blockHeading, composedHeadings[here + 1]);
    }

    // --------------------------------------------------------- what it is not

    [Fact]
    public void TheImagingSectionNamesTheThingsThatAreNotCancerAndCountsThemRight()
    {
        // The item's acceptance criterion: the differential "includes an
        // abscess, demyelination and an infarct — things that are not cancer at
        // all". The list is pinned IN ORDER, and the page's own count word is
        // derived from it — so adding or dropping an entry turns the sentence
        // red instead of leaving a number that used to be true. The first draft
        // of this page said "Three" over a list of four.
        var section = PlainOf(ImagingHeading);

        string[] items =
        [
            "a tumor that started in the brain",
            "a tumor that spread from a cancer somewhere else",
            "an infection",
            "an area of inflammation",
            "an old stroke",
            "old bleeding that is breaking down",
        ];

        var at = -1;
        foreach (var item in items)
        {
            var index = section.IndexOf(item, StringComparison.OrdinalIgnoreCase);
            Assert.True(index > at, $"'{item}' is missing from the differential or is out of order");
            at = index;
        }

        // The first two are tumors; everything after them is not. "Not a tumor",
        // not "not cancer": `/review` pointed out that "four of those are not
        // cancer" implies the other two ARE, and a tumor that starts in the
        // brain is not necessarily cancer — which is the distinction the benign
        // section three headings later is built on.
        string[] numbers = ["Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven"];
        Assert.Matches(
            new Regex($@"\b{numbers[items.Length - 2]} of those are not a tumor at all\b"),
            section);

        // The two mimics that carry their own sources, named rather than left
        // as a list entry a reader cannot look up.
        Assert.Matches(new Regex(@"called an abscess", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"the answer comes back as inflammation instead", RegexOptions.IgnoreCase), section);

        // §12.6: the section ends on what closes the question, not on the list
        // of frightening possibilities.
        var sentences = CuratedPage.SentencesOf(section);
        Assert.Contains("conversation rather than something that happens quietly",
            string.Join(" ", sentences[^2..]), StringComparison.OrdinalIgnoreCase);

        // RULING 1 applied to the OTHER sibling. /tests/biopsy owns both the
        // needle-versus-operation walkthrough and the "is there a way to find
        // out without one?" exception; `/review` found the first draft restating
        // both, entirely unguarded because nothing in this file read that page.
        var biopsy = Sibling("tests", "biopsy.md");
        foreach (var owned in new[]
                 {
                     "The surgeon makes a small cut in your scalp",
                     "Surgeons call the small hole a burr hole",
                     "A needle biopsy is not an attempt to remove the tumor",
                     "a scan may sometimes give enough information on its own",
                 })
        {
            Assert.True(biopsy.Contains(owned, StringComparison.OrdinalIgnoreCase),
                $"/tests/biopsy no longer says \"{owned}\", so this non-duplication check is "
                + "guarding something that has moved or been reworded");
            Assert.DoesNotContain(owned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("/tests/biopsy#is-there-a-way-to-find-out-without-one", section,
            StringComparison.Ordinal);

        // And the reason it is a limit rather than a doctor hedging.
        Assert.Matches(new Regex(
            @"that is not hedging", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheBenignSectionRefusesTheComfortAndLandsOnAnAction()
    {
        // §12.12's more dangerous direction, and the item names it: "benign" is
        // the wrong comfort word in the brain. The four moves, all sourced, all
        // required, because dropping any one of them turns the section into
        // either a scare or the reassurance it exists to take apart.
        var section = PlainOf(BenignHeading);

        // (1) What the word does still mean. Leaving this out makes the section
        // a scare, and it is the half ACS states plainly.
        Assert.Matches(new Regex(@"a growth that stays where it is", RegexOptions.IgnoreCase),
            section);

        // (2) What it does not mean, in the page's own bluntest sentence.
        Assert.Matches(new Regex(@"What it does not mean is harmless", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"Both kinds can be life-threatening", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"because of where they sit rather than because of what they are",
            RegexOptions.IgnoreCase), section);

        // (3) The vocabulary the item asks for, and the reason it is better.
        Assert.Matches(new Regex(@"does not use the word benign at all", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"\bnon-malignant\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"says what a growth does not do", RegexOptions.IgnoreCase), section);

        // (4) §12.6: the landing is an action, not the frightening sentence.
        var sentences = CuratedPage.SentencesOf(section);
        Assert.Matches(new Regex(
            @"where is it sitting, and what would it do if we left it alone",
            RegexOptions.IgnoreCase), sentences[^1]);
    }

    [Fact]
    public void TheRegistryPointCarriesNoIncidenceFigure()
    {
        // RULING 5. CBTRUS gives non-malignant 19.19 per 100,000 against
        // malignant 6.86, which reads as "most brain tumors are not malignant" —
        // true of the registry and a comfort this reader has not earned, because
        // the count is dominated by small tumors found by accident and this
        // reader has one that started a workup. The page takes the VOCABULARY
        // from that source and no number.
        var section = PlainOf(BenignHeading);

        var figure = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\bper\s+" + CountWord + @"[\s,]*" + CountWord + @"\b|"
            + @"\b" + CountWord + @"\s+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            + @"\b" + Fraction + @"\s+(?:of\s+)?(?:all\s+)?(?:the\s+)?(?:\w+\s+){0,2}"
            + @"(?:brain tumors|tumors|people|cases)\b|"
            // A share does not need the word "of", and a quantifier does not sit
            // next to its noun (§12.8, WI-527).
            + @"\b(?:most|almost all|nearly all|the majority of|hardly any|a tiny minority of)\s+"
            + @"(?:\w+\s+){0,3}(?:brain tumors|tumors|people|cases|of them)\b|"
            + @"\bmajorit(?:y|ies)\b|\bminorit(?:y|ies)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "About 74 percent of brain tumors counted are non-malignant.",
                     "Nineteen per 100,000 are non-malignant each year.",
                     "Three in every four brain tumors are non-malignant.",
                     "Two thirds of all brain tumors are non-malignant.",
                     "Most brain tumors counted this way are non-malignant.",
                     "The majority of them are non-malignant.",
                     "Almost all of the brain tumors in the registry are non-malignant.",
                 })
        {
            Assert.Matches(figure, known);
        }

        Assert.DoesNotMatch(figure, section);

        // AND THE CLAIM PAGE-WIDE, because a section-scoped guard proves nothing
        // about the rest of the page (§12.8, WI-527) — "most brain tumors are
        // not malignant" in the short version does exactly the harm ruling (5)
        // exists to prevent. Narrowed to the SUBJECT rather than run as the full
        // figure ban page-wide: the page's second section carries ACS's own
        // "Most brain tumors are found because they start to cause something",
        // which is correct, sourced and quantified, and a rule that fails a
        // correct page is worse than no rule.
        var share = new Regex(
            @"\b(?:most|almost all|nearly all|the majority of|hardly any|a tiny minority of|"
            + Fraction + @"|" + CountWord + @")\b[^.]{0,50}"
            + @"\b(?:non-malignant|not malignant|malignant|benign|cancerous)\b|"
            + @"\b(?:non-malignant|not malignant|malignant|benign|cancerous)\b[^.]{0,50}"
            + @"\b(?:most|almost all|nearly all|the majority of|hardly any|"
            + Fraction + @"|" + CountWord + @")\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Most brain tumors counted this way are not malignant.",
                     "Three quarters of them are non-malignant.",
                     "About 74 percent are benign.",
                     "Non-malignant tumors are the majority of what gets counted.",
                 })
        {
            Assert.Matches(share, known);
        }

        Assert.DoesNotMatch(share, Plain);

        // The canary: the section this ruling is about still exists and still
        // makes the vocabulary point, so the bans above are not passing because
        // the whole section was deleted.
        Assert.Matches(new Regex(@"\bnon-malignant\b", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheStageSectionSaysNothingIsBeingHeldBack()
    {
        // The documented confusion: everybody knows cancer "has stages", so a
        // reader who is not given one assumes they are being protected from it.
        var section = PlainOf(StageHeading);

        Assert.Matches(new Regex(
            @"no formal staging system", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"it is not coming, and nothing is being kept back", RegexOptions.IgnoreCase), section);

        // What replaces it, and the honest note that both of them need tissue —
        // which is what connects this section to the rest of the page.
        Assert.Matches(new Regex(@"\btype\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"\bgrade\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"both of them usually wait for tissue", RegexOptions.IgnoreCase), section);
        Assert.Contains("/tests/pathology-report#what-the-grade-means", section,
            StringComparison.Ordinal);

        // And the exception, because a reader whose thing turns out to have come
        // from elsewhere WILL be given a stage and would otherwise meet a page
        // that told them there is not one.
        Assert.Matches(new Regex(@"one situation where a stage does turn up",
            RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheForkRoutesToTheMetastasesPageRatherThanRestatingIt()
    {
        // PROGRESS.md's instruction from WI-528: the brain-metastases page now
        // carries the honest version of "sometimes the brain scan is what finds
        // the cancer", including cancer of unknown primary, so WI-529 routes to
        // it rather than saying it again.
        var section = PlainOf(ForkHeading);

        Assert.Contains("/tumors/brain-metastases", section, StringComparison.Ordinal);

        var sibling = Sibling("tumors", "brain-metastases.md");
        foreach (var owned in new[]
                 {
                     "cancer of unknown primary",
                     "Sometimes the brain scan is what finds the cancer in the first place",
                 })
        {
            Assert.Contains(owned, sibling, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(owned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // The direction, in words, and never as a share (§12.4).
        Assert.Matches(new Regex(
            @"more came from somewhere else than started in the brain", RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"\b" + CountWord + @"\s*(?:%|percent|per cent)|\bover half\b|\bmost of them\b",
                RegexOptions.IgnoreCase),
            section);

        // §12.6: the section that tells somebody their body is about to be
        // scanned does not end on that sentence.
        Assert.Matches(new Regex(
            @"not assuming the worst", RegexOptions.IgnoreCase), section);
    }

    // ------------------------------------------------ what belongs elsewhere

    [Fact]
    public void TheLabQueueIsRoutedAndNotRestated()
    {
        // RULING 1, and the biggest scope call in the item. The research pack
        // calls its turnaround-time table "the most valuable section in the
        // whole brief"; it is already shipped on /tests/waiting-for-results.
        // Asserted against the SIBLING'S OWN WORDS rather than a literal list,
        // so moving a sentence between the two pages later goes red instead of
        // creating a silent second copy (§12.11).
        var sibling = Sibling("tests", "waiting-for-results.md");

        string[] owned =
        [
            "One national study looked at 21 brain tumor centers",
            "national target of 14 days",
            "run with about eight samples at a time",
            "It matches the final answer about nine times out of ten",
            "twenty to thirty minutes",
        ];

        foreach (var sentence in owned)
        {
            Assert.True(sibling.Contains(sentence, StringComparison.OrdinalIgnoreCase),
                $"/tests/waiting-for-results no longer says \"{sentence}\", so this test is "
                + "checking the non-duplication of something that has moved or been reworded");
            Assert.DoesNotContain(sentence, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // And the shape of the claim, not only those sentences: no interval with
        // a quantity in it anywhere on this page.
        //
        // `months` and the vague quantifiers are in because of `/review`:
        // /treatments/watch-and-wait publishes "anything from every few months
        // to every couple of years" AND records that its two sources disagree on
        // the short end, and this page's first draft printed only "the next one
        // in a few months" — the sibling's range, narrowed, unattributed, past a
        // guard that looked for `days|weeks` and a `CountWord` with no `few` in
        // it.
        var duration = new Regex(
            @"\b(?:about|around|roughly|up to|at least|nearly|within|every)?\s*"
            + @"(?:" + CountWord + @"|a few|a couple of|several)"
            + @"[-\s]+(?:\w+[-\s]+){0,2}(?:days?|weeks?|months?|years?|working days?)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "The gene tests take about three weeks.",
                     "The full answer takes 21 days.",
                     "The panel meets within two working days.",
                     "Expect the report in seven to ten days.",
                     "A plan of scans, with the next one in a few months.",
                     "They will scan you again in a couple of years.",
                     "You will hear in several weeks.",
                 })
        {
            Assert.Matches(duration, known);
        }

        // `five-year survival` is REDACTED, not exempted (§12.8, WI-527): §12.5
        // requires the outlook gate to teach that term, and `five` + `-year` is
        // a duration to any regex that reads it. The canary below proves the
        // redaction removes the phrase and not the sentence around it.
        string[] taughtTerm = ["five-year survival"];
        Assert.Matches(duration, Redact("Five-year survival, and then three weeks of tests.",
            taughtTerm));
        var scanned = Redact(Plain, taughtTerm);
        Assert.Contains("is the share of a group still there at that mark", scanned,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotMatch(duration, scanned);

        // The sibling still carries the range this page refuses to narrow.
        Assert.True(
            Sibling("treatments", "watch-and-wait.md")
                .Contains("every few months", StringComparison.OrdinalIgnoreCase),
            "/treatments/watch-and-wait no longer publishes the scan interval, so this page's "
            + "refusal to publish one is routing to nothing");

        // The page still ANSWERS the question, in shape, and hands the reader
        // the door. Dropping a claim without replacing it leaves the reader
        // where it found them (§12.8, WI-521).
        var wait = PlainOf(WaitHeading);
        Assert.Matches(new Regex(@"takes minutes", RegexOptions.IgnoreCase), wait);
        Assert.Matches(new Regex(@"takes days", RegexOptions.IgnoreCase), wait);
        Assert.Matches(new Regex(@"take weeks", RegexOptions.IgnoreCase), wait);
        Assert.Contains("/tests/waiting-for-results#how-long-does-it-take", wait,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheSecondOpinionOnTissueIsRoutedAndTheOneThisReaderCanUseIsOnThePage()
    {
        // The item asks for "second opinions including on the tissue itself".
        // The tissue half is /tests/waiting-for-results'. What is genuinely this
        // page's is the half that exists BEFORE there is tissue — an opinion on
        // the plan — because that is the decision that has not been made yet.
        var section = PlainOf(OpinionHeading);

        Assert.Matches(new Regex(
            @"Before there is tissue, the thing to ask about is the plan", RegexOptions.IgnoreCase),
            section);
        Assert.Contains("/tests/waiting-for-results#can-someone-else-look-at-my-slides", section,
            StringComparison.Ordinal);

        var sibling = Sibling("tests", "waiting-for-results.md");
        foreach (var owned in new[]
                 {
                     "Asking does not offend anyone",
                     "it does not usually mean going without treatment",
                     "Ask about the cost first",
                 })
        {
            Assert.True(sibling.Contains(owned, StringComparison.OrdinalIgnoreCase),
                $"/tests/waiting-for-results no longer says \"{owned}\"");
            Assert.DoesNotContain(owned, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // RULING 9. The research pack's disagreement rates are attributed to a
        // paper that reports none. No figure, in digits or in words.
        var disagreement = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b(?:" + Fraction + @"|" + CountWord + @")\b[^.]{0,60}"
            + @"\b(?:disagree|changed the diagnosis|second look|reviewed again|expert review)\b|"
            + @"\b(?:disagree\w*|the diagnosis changed)\b[^.]{0,60}\b(?:" + Fraction + @")\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Major disagreements happen in 12% of cases sent for expert review.",
                     "In one in eight cases the diagnosis changed on a second look.",
                     "A quarter of cases sent for expert review came back with a disagreement.",
                 })
        {
            Assert.Matches(disagreement, known);
        }

        Assert.DoesNotMatch(disagreement, Plain);
    }

    // ---------------------------------------------------------------- safety

    [Fact]
    public void TheEscalationBlockIsIncludedInTheCallSection()
    {
        // §12.9: a hub INCLUDES the block and never re-types it, and the
        // directive has to sit inside the symptoms section — a test that reads
        // the whole page keeps passing while [ESCALATION] drifts to the bottom.
        CuratedPage.AssertEscalationTiers(Page, Slug, CallHeading);

        Assert.Contains("[ESCALATION]", RawSection(CallHeading), StringComparison.Ordinal);

        // The lead-in says why the block is here at all on a page with no
        // diagnosis, which is the thing no other hub has to explain.
        Assert.Matches(new Regex(
            @"nobody has told you what your warning signs are", RegexOptions.IgnoreCase),
            PlainOf(CallHeading));
    }

    [Fact]
    public void TheFeverLineIsThisPagesOwnAndSitsBelowTheBlock()
    {
        // RULING 8, and §12.10's instruction in as many words: "add the page's
        // own line beneath the block if its emergency is not in the list". The
        // block's two fever rules are conditional on chemotherapy and on recent
        // brain surgery, and a reader waiting at home before any operation is
        // covered by neither — while an infection is on this page's own
        // differential.
        // Flattened for the same reason as the scoping note: "If you run\na
        // fever" has the wrap inside the phrase.
        var raw = CuratedPage.Flatten(RawSection(CallHeading));
        var section = PlainOf(CallHeading);

        Assert.Matches(new Regex(
            @"If you run a fever while you are waiting, call your team today",
            RegexOptions.IgnoreCase), section);

        // THE HONEST HALF, which is what stops the line becoming a screening
        // test. StatPearls: the classic triad "is observed in fewer than half of
        // patients". Without this sentence the page implies no fever means no
        // infection, which is §12.12's more dangerous direction.
        Assert.Matches(new Regex(
            @"not having one settles nothing either way", RegexOptions.IgnoreCase), section);

        // Position, not presence (§12.8, WI-512). A page-local escalation line
        // above the block reads as the first rule rather than the extra one.
        var directive = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        var fever = raw.IndexOf("If you run a fever", StringComparison.Ordinal);
        Assert.True(directive >= 0 && fever > directive,
            "the page's own fever line is not below [ESCALATION]");

        // And it does not contradict the block's two rules, which are a tier
        // higher. Three pages sorting one symptom into three tiers is WI-512's
        // blocker, and this is the shape it would take here.
        Assert.Matches(new Regex(
            @"sits on top of the two rules above rather than instead of them",
            RegexOptions.IgnoreCase), section);
    }

    // --------------------------------------------------------- blocks, named

    [Fact]
    public void EveryBlockDirectiveOnThisPageIsNamedHere()
    {
        // §12.8, WI-527: a contract item no test names is not a contract item.
        // [MECHANISM] and [CROSSWALK] were composed into the meningioma page and
        // listed in no assertion, so deleting the mechanism block broke nothing.
        var body = CuratedPage.ReaderText(Page);

        string[] named = ["[MECHANISM]", "[ESCALATION]", "[TUMOR-BOARD]", "[CAUSES]", "[CAREGIVER]"];

        foreach (var directive in named)
        {
            Assert.Contains(directive, body, StringComparison.Ordinal);
        }

        // AND NO SIXTH. A directive nobody names is a contract item nobody
        // tests, which is how [MECHANISM] and [CROSSWALK] composed silently into
        // /tumors/meningioma. The set is closed, so adding one forces a test.
        var present = Regex.Matches(body, @"(?m)^\[([A-Z][A-Z-]+)\]\s*$")
            .Select(m => m.Value.Trim())
            .ToHashSet(StringComparer.Ordinal);
        Assert.Empty(present.Except(named));

        // Contract item 11: the caregiver section is a real section under the
        // §12.3 heading, not a footnote.
        Assert.Contains("[CAREGIVER]", RawSection(CareHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", RawSection(BlameHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", RawSection(BoardHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCrosswalkBlockIsExcludedAndIsNotEdited()
    {
        // RULING 3. Not false here, unlike WI-528's case — addressed to somebody
        // else. The block opens "If your paperwork was written before that" and
        // closes "Seeing an older name on your own report", and THIS READER IS
        // DEFINED BY NOT HAVING A REPORT. Contract item 3 exists because readers
        // arrive holding old paperwork; this is the one page whose reader does
        // not.
        // The BODY, not the whole page: the front matter's own rulings name the
        // directive in order to record why it is absent.
        Assert.DoesNotContain("[CROSSWALK]", CuratedPage.ReaderText(Page), StringComparison.Ordinal);

        // THE EXCLUSION IS PROVED BY THE BLOCK STILL SAYING WHAT IT SAYS
        // (§12.10, WI-528). Editing it to fit this page would push the change
        // onto every hub that includes it.
        var crosswalk = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md")));
        Assert.Contains("If your paperwork was written before that", crosswalk,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Seeing an older name on your own report", crosswalk,
            StringComparison.OrdinalIgnoreCase);

        // AND THE CLAIM SHAPE IS BANNED IN THE PAGE'S OWN VOICE, because
        // excluding a block does not stop a draft writing the block (§12.14,
        // and WI-528's review wrote the excluded claim past two checks in one
        // line).
        var crosswalkClaim = new Regex(
            @"\brules for naming brain tumors changed\b|"
            + @"\byour (?:old )?(?:paperwork|report) (?:was |were )?written before\b|"
            + @"\bgrade I{1,3}V?\b|\bgrade IV\b|"
            + @"\b(?:NOS|NEC)\b[^.]{0,60}\bnext to your diagnosis\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "The rules for naming brain tumors changed in 2021.",
                     "If your report was written before 2021, some of its words are gone.",
                     "An older report may say grade II where a new one says grade 2.",
                 })
        {
            Assert.Matches(crosswalkClaim, known);
        }

        Assert.DoesNotMatch(crosswalkClaim, Plain);

        // The half that IS this reader's lives on the sibling, so the page has
        // to actually route there rather than just drop it.
        Assert.True(
            Sibling("tests", "waiting-for-results.md")
                .Contains("the genes are part of the name", StringComparison.OrdinalIgnoreCase),
            "/tests/waiting-for-results no longer explains that gene results are part of the "
            + "name, which is the one piece of the crosswalk this page's reader needs");
        Assert.Matches(new Regex(
            @"the gene results are part of the name", RegexOptions.IgnoreCase),
            PlainOf(WaitHeading));
    }

    [Fact]
    public void TheCausesBlockIsIncludedAndThePageAddsTheQuestionItDoesNotAnswer()
    {
        // RULING 4, and the opposite call from WI-528. There, [CAUSES] opens on
        // a sentence that is FALSE ("nobody knows the cause", when the cause is
        // in the reader's chart). Here it is true and it asserts nothing about
        // this reader's scan. What the block does not answer is this reader's
        // version of the question.
        var raw = RawSection(BlameHeading);
        var section = Regex.Replace(
            CuratedPage.Flatten(Reader(CuratedPage.Section(CuratedPage.Composed(Page, Slug), BlameHeading))),
            @"[*_]", "");

        Assert.Contains("[CAUSES]", raw, StringComparison.Ordinal);
        Assert.Contains("For most brain tumors, nobody knows the cause", section,
            StringComparison.OrdinalIgnoreCase);

        // The page's own slice, and its ANGLE rather than merely its words —
        // a slice that repeats the block has added a second maintenance site
        // and no information (§12.11).
        Assert.Matches(new Regex(@"Should I have come in sooner", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"no screening program that would have caught this earlier", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"usually caused by something else entirely", RegexOptions.IgnoreCase), section);

        // And the slice is not a copy of the block's own sentences.
        var causes = Regex.Replace(CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "causes.md"))), @"[*_]", "");
        var own = Regex.Replace(CuratedPage.Flatten(Reader(raw)), @"[*_]", "");
        foreach (var sentence in CuratedPage.SentencesOf(causes)
                     .Where(s => s.Trim().Length > 60))
        {
            Assert.DoesNotContain(sentence.Trim(), own, StringComparison.OrdinalIgnoreCase);
        }

        // §12.6: the lead-in does not contradict the block underneath it
        // (WI-510: the sentence above a block is shared prose too). The block
        // already promises a plain answer; the lead-in scopes, it does not
        // repeat.
        Assert.Matches(new Regex(
            @"this question does not wait for one", RegexOptions.IgnoreCase), own);
    }

    [Fact]
    public void TheCaregiverSectionAddsExactlyThreeClaimsOfItsOwn()
    {
        // §12.8, WI-527 and WI-528: the section promises "three more", so assert
        // THREE — a guard that pins three claims without counting them lets a
        // fourth in, and counting paragraph-leading bold rather than bold runs
        // is what stops an unbolded fourth sliding past.
        var raw = RawSection(CareHeading);

        // "Three things below", not "Three more": the corpus-wide shingle guard
        // on /tumors/brain-metastases went red on
        // "caring for someone with this [CAREGIVER] Three more" — eight words of
        // overlap made entirely of the MANDATED heading, the standard directive
        // and a stock opener. The heading and the directive have to stay, so the
        // opener is the part that moves.
        Assert.Matches(new Regex(@"Three things below", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(raw));

        // PARAGRAPHS, not bold runs. WI-528's lesson is exactly this: `/review`
        // added a fourth caregiver claim with no bold on it and a count of `^\*\*`
        // stayed at three, which is WI-527's lesson defeated by deleting the
        // markup the count was reading. So split the section's own prose — the
        // part after [CAREGIVER] and its lead-in — and count what is left.
        // `\r?\n`, never a bare `\n`.
        var afterBlock = raw[(raw.IndexOf("[CAREGIVER]", StringComparison.Ordinal)
                              + "[CAREGIVER]".Length)..];
        var paragraphs = Regex.Split(afterBlock.Trim(), @"\r?\n[ \t]*\r?\n")
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        // The first is the "Three things below…" lead-in; the rest are claims.
        Assert.True(paragraphs.Count == 4,
            $"the caregiver section adds {paragraphs.Count - 1} paragraphs of its own but "
            + "promises three");
        Assert.StartsWith("Three things below", paragraphs[0], StringComparison.OrdinalIgnoreCase);

        var section = PlainOf(CareHeading);
        Assert.Matches(new Regex(@"The job this week is writing it down", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"there is one search to avoid", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"You are allowed to call the team yourself, between appointments",
            RegexOptions.IgnoreCase), section);

        // Written to the caregiver in the second person (§12.7), never about
        // them from outside.
        Assert.DoesNotMatch(new Regex(@"caregivers (?:often|usually|tend to)",
            RegexOptions.IgnoreCase), section);
    }

    // --------------------------------------------------------------- outlook

    [Fact]
    public void TheOutlookGateIsAFenceAndTheHeadingIsOutsideIt()
    {
        // §12.5, and WI-527's bug: `^## heading\s*\n\n:::outlook` cannot match a
        // CRLF checkout at all — greedy `\s*` eats both breaks and every shorter
        // split leaves a `\r` where a `\n` is required — so the guard failed for
        // free and the harness reported every mutation as caught. `\r?\n`, and
        // never `\s*` standing in for a blank line.
        Assert.Matches(
            new Regex($@"(?m)^## {Regex.Escape(OutlookHeading)}[ \t]*\r?\n\r?\n:::outlook\r?\n"),
            Page);

        // A closing fence on a line of its own. Without it the container is
        // unclosed, which fails the page build rather than publishing wide open
        // — but a test that cannot see the difference is the reason to have one.
        Assert.Matches(new Regex(@"\r?\n:::[ \t]*\r?\n"), Page);

        // Exactly one gate: a second `:::outlook` anywhere is a second
        // disclosure the reader consented to once.
        //
        // `\s*$`, not `[ \t]*$`, and that is the whole WI-527 bug in one
        // character class: this repo is core.autocrlf=true, `[ \t]` does not
        // consume a `\r`, and .NET's multiline `$` does not match before one —
        // so on a CRLF checkout this counted 0, failed for free, and made the
        // break harness report `ok` for every mutation aimed at it. `/review`
        // caught it here before the file was ever tracked.
        Assert.Equal(1, Regex.Matches(Page, @"(?m)^:::outlook\s*$").Count);
    }

    [Fact]
    public void TheOutlookGateRefusesAnOutlookAndStillTeachesTheVocabulary()
    {
        var gate = Regex.Replace(CuratedPage.Flatten(Reader(
            Regex.Match(Page, @":::outlook\r?\n(.*?)\r?\n:::", RegexOptions.Singleline)
                .Groups[1].Value)), @"[*_]", "");

        Assert.False(string.IsNullOrWhiteSpace(gate),
            "the outlook gate's body could not be read, so every guard below proves nothing");

        // THE REFUSAL, and its reason. On every other hub the gate hides numbers
        // the page decided not to print; here there is no diagnosis to attach
        // one to, and saying so is the content.
        Assert.Matches(new Regex(
            @"outlook is attached to a diagnosis, and you do not have one", RegexOptions.IgnoreCase),
            gate);

        // §12.5's four moves for explaining what a figure IS, without printing
        // one. WI-513's first draft did one of the four and its own test BANNED
        // the vocabulary the gate exists to teach, which would have cemented an
        // explaining-nothing gate across 23 copies. So the two words a reader is
        // about to meet in a search engine are REQUIRED here, and the ban below
        // is on the figures and the claims rather than on the words.
        Assert.Matches(new Regex(@"describe a group", RegexOptions.IgnoreCase), gate);
        Assert.Matches(new Regex(@"\bMedian\b", RegexOptions.IgnoreCase), gate);
        Assert.Matches(new Regex(
            @"half of the people did better than it, half did worse", RegexOptions.IgnoreCase),
            gate);
        Assert.Matches(new Regex(@"\bFive-year survival\b", RegexOptions.IgnoreCase), gate);
        Assert.Matches(new Regex(@"wide spreads", RegexOptions.IgnoreCase), gate);
        Assert.Matches(new Regex(
            @"runs a long way in the good direction", RegexOptions.IgnoreCase), gate);
        Assert.Matches(new Regex(
            @"not a prediction about one person", RegexOptions.IgnoreCase), gate);

        // The warning that is specific to a reader holding a LIST rather than a
        // name, which is the only thing here no other hub's gate can say.
        Assert.Matches(new Regex(
            @"an average across them would be a number about nobody", RegexOptions.IgnoreCase),
            gate);

        // §12.8, WI-527: A DIGIT BAN IS NOT A PROGNOSIS BAN. The gate is consent
        // to read, not a licence to publish, so the claim is banned in WORDS
        // too — "people in that group live as long as anybody else" contains no
        // digit at all.
        // The two terms the gate is REQUIRED to teach are redacted before the
        // scan rather than exempted, because a substring allowlist exempts the
        // whole sentence and everything else in it (§12.8, WI-527). The
        // redaction is proved by a canary below: a figure appended to the
        // allowed phrase still has to fire.
        string[] taught = ["five-year survival", "five years"];
        var scanned = Redact(gate, taught);

        var prognosis = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            // A figure near the vocabulary, in either direction. The words
            // themselves are allowed; a number beside one is a published
            // prognosis whichever side it sits.
            + @"\b(?:median|survival|life expectancy)\b[^.]{0,60}\b" + CountWord + @"\b|"
            + @"\b" + CountWord + @"\b[^.]{0,60}\b(?:median|survival|life expectancy)\b|"
            + @"\b" + CountWord + @"[-\s]+(?:\w+[-\s]+){0,2}(?:months?|years?)\b|"
            // §12.8, WI-527: A DIGIT BAN IS NOT A PROGNOSIS BAN. The gate is
            // consent to read, not a licence to publish, so the claim is banned
            // in WORDS too — none of these contains a digit.
            + @"\b(?:as long as anybody else|as long as anyone else|done with it|"
            + @"never bothers them again|outlive|cured|curable|terminal)\b|"
            + @"\b(?:most|almost all|nearly all|hardly any)\s+(?:\w+\s+){0,3}"
            + @"(?:people|patients)\b[^.]{0,40}\b(?:live|survive|do well|are fine)\b|"
            + @"\bmost people (?:with (?:this|it)|in that group)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "The median survival is 15 months.",
                     "About 35 percent are alive at five years.",
                     "Many people live ten years or more with this.",
                     "People in that group live as long as anybody else.",
                     "Almost everybody in that group is done with it.",
                     "Most people with this are fine.",
                     "Most people in this position live five years or more.",
                     // THE REDACTION CANARY. Removing the allowed phrase must
                     // not buy the rest of the sentence a free ride, which is
                     // exactly what an allowlist would have done here.
                     "Five-year survival for this group is about 35 percent.",
                     "Five-year survival is around 20 months of median survival.",
                 })
        {
            Assert.Matches(prognosis, Redact(known, taught));
        }

        Assert.DoesNotMatch(prognosis, scanned);

        // And the redaction removed only what it was meant to: if it swallowed
        // the surrounding sentence, every assertion above would pass for the
        // wrong reason (§12.8, WI-527).
        Assert.Contains("is the share of a group still there at that mark", scanned,
            StringComparison.OrdinalIgnoreCase);

        // §12.6: the gate lands on something to do, not on the refusal.
        var sentences = CuratedPage.SentencesOf(gate);
        Assert.Matches(new Regex(
            @"Save it for the appointment", RegexOptions.IgnoreCase), sentences[^1]);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisAnywhereOutsideTheGate()
    {
        // §12.8, WI-527: a section-scoped guard proves nothing about the rest of
        // the page. The right-tail sentence belongs INSIDE the gate (§12.9), and
        // the failure mode is a reassuring line sitting in section 2 where a
        // reader who declined outlook meets it anyway.
        // Two halves, because `/review` found the first version STRICTER INSIDE
        // the gate than outside it — the worded claims were banned where the
        // reader had consented and allowed where they had not, so "some of the
        // things on that list people live with for years" passed everywhere on
        // the page.
        //
        // (1) The VOCABULARY, which the gate is required to teach (§12.5) and
        //     the rest of the page may not use. The canary is the gate itself.
        // (2) The CLAIM IN WORDS, which is banned in both places: §12.5's gate
        //     is consent to read, not a licence to publish, and a claim with no
        //     digit in it is invisible to a figure ban (§12.8, WI-527).
        var prognosis = new Regex(
            @"\b(?:median|five[- ]year survival|survival rate|life expectancy|"
            + @"how long (?:you|they) have(?: left)?)\b",
            RegexOptions.IgnoreCase);

        var wordedClaim = new Regex(
            @"\b(?:as long as anybody else|as long as anyone else|done with it|"
            + @"never bothers them again|outlive)\b|"
            // A gap, not adjacency (§12.8, WI-527): the object is routinely
            // fronted — "some of the things on that list people LIVE WITH FOR
            // YEARS" — and `live with (?:it|this) for years` misses every one.
            + @"\blive\b[^.]{0,25}\bfor years\b|"
            + @"\b(?:most|almost all|nearly all|hardly any)\s+(?:\w+\s+){0,3}"
            + @"(?:people|patients)\b[^.]{0,40}\b(?:live|survive|do well|are fine)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Some of the things on that list people live with for years.",
                     "Most people in this position do well.",
                     "People in that group live as long as anybody else.",
                 })
        {
            Assert.Matches(wordedClaim, known);
        }

        var gate = Regex.Match(Page, @":::outlook\r?\n(.*?)\r?\n:::", RegexOptions.Singleline)
            .Groups[1].Value;
        var outside = Regex.Replace(
            Regex.Replace(CuratedPage.ReaderText(Page), Regex.Escape(gate), " "), @"[*_]", "");

        Assert.Matches(prognosis, Regex.Replace(gate, @"[*_]", ""));   // canary: it can fire
        Assert.DoesNotMatch(prognosis, CuratedPage.Flatten(outside));
        Assert.DoesNotMatch(wordedClaim, CuratedPage.Flatten(outside));
    }

    // ----------------------------------------------------------- the sources

    [Fact]
    public void NoNciPatientPdqUrlIsCitedAndNoDeadDomainReappears()
    {
        // RULING 2. §12.1 bans NCI patient PDQ on a tumor hub, and a
        // banned-DOMAIN list structurally cannot express that (WI-528), so the
        // rule gets its own pattern.
        var front = CuratedPage.FrontMatter(Page);
        var urls = CitedUrls();

        var pdq = new Regex(@"cancer\.gov/[^\s""]*(?:/patient/|-pdq\b)", RegexOptions.IgnoreCase);
        Assert.Matches(pdq, "https://www.cancer.gov/types/brain/patient/adult-brain-treatment-pdq");
        Assert.Matches(pdq, "https://www.cancer.gov/types/brain/hp/adult-brain-treatment-pdq");
        Assert.DoesNotMatch(pdq, urls);
        // And it must not reach the reader through the body either, which is the
        // surface a front-matter check cannot see.
        Assert.DoesNotMatch(pdq, CuratedPage.ReaderText(Page));

        // NOR THROUGH A BLOCK. Block `sources` merge into the including page and
        // render in the reader's source list, so a page-scoped front-matter
        // check is blind to the files with the widest blast radius (§12.10).
        // `/review` found the first version of this test claiming a ban it could
        // not enforce.
        foreach (var block in new[]
                 { "mechanism", "escalation", "tumor-board", "causes", "caregiver" })
        {
            var text = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, $"{block}.md"));
            Assert.DoesNotMatch(pdq, text);
        }

        // THE DOMAINS THIS ITEM ACTUALLY FETCHED AND FOUND UNREADABLE, and no
        // others. `/review` pointed out that the first version of this list
        // carried `mayoclinic.org`, `hopkinsmedicine.org` and `ascopubs.org`
        // under a comment calling them dead — and ascopubs.org is the host of
        // the ASCO-SNO-ASTRO guideline §12.1 names as GOVERNING for
        // brain-metastasis radiation. A future item reads a list like this as
        // fact, so it says only what was checked:
        //
        //   appliedradiology.com  403 to two fetches. Carries the research
        //                         pack's whole ring-enhancing differential.
        //   academic.oup.com      dead. Carries the pack's turnaround-time table
        //                         and the two-working-day triage claim.
        //
        // link.springer.com is NOT on this list and is not dead: ruling (12)
        // records the re-fetch. Scoped to THIS PAGE'S OWN citations — the
        // composed source list also holds the blocks' URLs, and the rule that
        // must hold there is §12.1's, which is checked separately above.
        foreach (var dead in new[] { "academic.oup.com", "appliedradiology.com" })
        {
            Assert.DoesNotContain(dead, urls, StringComparison.OrdinalIgnoreCase);
        }

        // The canary: the ban list is checked against the URL VALUES, so prove
        // the extraction found some rather than handing every assertion above an
        // empty string.
        Assert.Contains("cancer.org", urls, StringComparison.Ordinal);
        Assert.Contains("pmc.ncbi.nlm.nih.gov", urls, StringComparison.Ordinal);

        // RULING 9 as a citation as well as a claim: the paper the research pack
        // hangs its disagreement rates on reports none.
        Assert.DoesNotContain("pubmed.ncbi.nlm.nih.gov/25972322", urls,
            StringComparison.OrdinalIgnoreCase);

        // Every claim on this page needs a source, so the front matter has to
        // actually hold some — a `sources:` key that lost its list is the shape
        // that makes the bans above pass for the wrong reason.
        Assert.True(Regex.Matches(front, @"(?m)^  - url: https://").Count >= 8,
            "the page's source list has shrunk below what its claims need");
    }

    [Fact]
    public void EveryCitedUrlIsFetchableInShapeAndCarriesATitle()
    {
        // §12.9: a citation renders as visible link text, so a fabricated title
        // is a fabricated citation on the reader's screen and no test can catch
        // the CONTENT of one. What a test can catch is a url with no title
        // beside it, which is how a hand-composed one gets in.
        var front = CuratedPage.FrontMatter(Page);

        var entries = Regex.Matches(front,
            @"(?m)^  - url: (\S+)\r?\n    title: ""(.+?)""\r?\n    accessed: (\d{4}-\d{2}-\d{2})\s*$");

        var urls = Regex.Matches(front, @"(?m)^  - url: (\S+)").Count;
        Assert.True(entries.Count == urls,
            $"{urls} sources are listed but only {entries.Count} have a title and an accessed "
            + "date in the expected shape");

        foreach (Match entry in entries)
        {
            Assert.StartsWith("https://", entry.Groups[1].Value, StringComparison.Ordinal);
            Assert.True(entry.Groups[2].Value.Trim().Length > 10,
                $"{entry.Groups[1].Value} has a title too short to be a real one");
        }
    }

    // ----------------------------------------------------- the shared guards

    [Fact]
    public void ThePageUsesNoBritishFormsAndNoRomanNumeralGrade()
    {
        // Run on PLAIN, which includes the title and the description: a guard
        // scoped to the body cannot see the first paragraph a reader meets
        // (§12.8, WI-528). Strip-then-scan, never continue-past — the weaker
        // form disables a whole file's check on one exemption (§12.10, WI-563).
        var text = Plain;
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            text = Regex.Replace(text, Regex.Escape(exemption), " ", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, text, StringComparison.OrdinalIgnoreCase);
        }

        // WI-529 added `travelled`, `judgement` and `labelled` after finding all
        // three shipped. The canary proves the list can still fire rather than
        // having been quietly emptied.
        Assert.Contains("travelled", CuratedPage.BritishForms);
        Assert.Contains("judgement", CuratedPage.BritishForms);
        Assert.Contains("labelled", CuratedPage.BritishForms);

        // Roman numeral grades, IgnoreCase and with the inflections (§12.8,
        // WI-516 and WI-527: five copies of this guard were case-sensitive, and
        // `\bgrade\b` is blind to "graded" and "grades").
        var roman = new Regex(@"\bgrade[sd]?\s+(?:I{1,3}V?|IV)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        // The canary is the block that teaches the old notation on purpose.
        // Without it a regex that matches nothing anywhere looks like a clean
        // page (§12.10).
        Assert.Matches(roman, File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md")));
        Assert.DoesNotMatch(roman, Plain);
    }

    [Fact]
    public void ThePageNeverCharacterisesAndNeverMinimises()
    {
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertNeverMinimises(Plain, Slug);

        // And the shape specific to this page: nothing may tell the reader the
        // wait is short, or that the scan is probably nothing. Both are the
        // over-reassurance direction (§12.12) and both are the natural thing to
        // write to somebody frightened.
        var reassurance = new Regex(
            @"\b(?:probably|most likely|almost certainly|usually) (?:nothing|fine|benign|not cancer)\b|"
            + @"\btry not to worry\b|\bit will not be long\b|\bwon'?t be long\b|"
            + @"\bthe odds are\b|\bchances are\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "It is probably nothing.",
                     "Most likely benign, but they have to check.",
                     "Try not to worry while you wait.",
                     "Chances are this is not cancer.",
                 })
        {
            Assert.Matches(reassurance, known);
        }

        Assert.DoesNotMatch(reassurance, Plain);
    }

    [Fact]
    public void TheSectionsArePresentByNameAndInTheOrderTheReaderNeeds()
    {
        // WI-528: `List.IndexOf` returns -1 and -1 is less than everything, so
        // three order assertions written with `>` stayed green when a whole
        // section was deleted. ASSERT PRESENCE FIRST, then the order.
        var headings = Headings();

        string[] order =
        [
            "The short version",
            ToldHeading,
            StepsHeading,
            ImagingHeading,
            MechanismHeading,
            CallHeading,
            TissueHeading,
            WaitHeading,
            BenignHeading,
            StageHeading,
            ForkHeading,
            BoardHeading,
            OpinionHeading,
            WeekHeading,
            BlameHeading,
            OutlookHeading,
            CareHeading,
            "What to ask your team",
            "Where to get support",
        ];

        foreach (var heading in order)
        {
            Assert.Contains(heading, headings);
        }

        for (var i = 1; i < order.Length; i++)
        {
            Assert.True(headings.IndexOf(order[i]) > headings.IndexOf(order[i - 1]),
                $"'{order[i]}' does not come after '{order[i - 1]}'");
        }

        // §12.3's principle, which is the thing the order encodes: everything
        // actionable comes before anything frightening. Outlook is second to
        // last before the caregiver section, and the self-blame block is
        // demoted out of the first screens (§12.9, WI-513).
        Assert.True(headings.IndexOf(BlameHeading) > headings.IndexOf(StepsHeading));
        Assert.True(headings.IndexOf(OutlookHeading) > headings.IndexOf(BlameHeading));

        // A DELIBERATE OMISSION, recorded so a later item does not "fix" it:
        // §12.3 slot 4 is "What symptoms does it cause?", and this page has no
        // such section because nobody knows what "it" is. The symptom material
        // is the brain map inside [MECHANISM] and the escalation block, and
        // both are asserted elsewhere in this file.
        Assert.DoesNotContain("What symptoms does it cause?", headings);
    }

    [Fact]
    public void TheWhatYouCanDoSectionTellsTheReaderNotToGradeTheirOwnReport()
    {
        // The research pack's one "what NOT to do", and it is the thing a
        // frightened reader with a portal login will do tonight. ACS is explicit
        // that only tissue confirms a type, so the report structurally cannot
        // hold the answer however closely it is read.
        var section = PlainOf(WeekHeading);

        Assert.Matches(new Regex(
            @"Do not try to read your own radiology report as a diagnosis",
            RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"A name and a grade almost always wait for tissue", RegexOptions.IgnoreCase), section);

        // The rest of the week's actions, each of which is something a reader
        // can do without anybody's permission.
        Assert.Matches(new Regex(@"Ask for copies of your scan", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Write down the exact words", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Take somebody with you", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Ask who your first call is", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheQuestionsListAsksTheThingsOnlyThisReaderNeeds()
    {
        // §12.2 item 7. The list is the printable one, so it has to carry the
        // questions the page spent its length arguing for rather than a generic
        // set that any tumor page could end with.
        var section = PlainOf("What to ask your team");

        Assert.Matches(new Regex(@"Which step am I on", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"including the ones that are not cancer", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"tumor board", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"after-hours number", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"copies of my scan", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"started in my brain, or somewhere else", RegexOptions.IgnoreCase), section);

        // A floor is met by any set (§12.8, WI-528), so the SPECIFIC questions
        // above carry the check and the count only guards the list's shape —
        // a printable list that has quietly become three bullets is a different
        // page from the one §12.2 item 7 asks for.
        var bullets = Regex.Matches(RawSection("What to ask your team"), @"(?m)^- ").Count;
        Assert.InRange(bullets, 10, 16);
    }
}

/// <summary>
/// The half that has to be proved against the SERVED page: every fail-open mode
/// of the reader-choice gate builds green (§12.5, WI-503), a composed block that
/// silently stops composing leaves its directive on the screen, and a deep link
/// to an anchor that does not exist resolves as a perfectly healthy 200.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class AllBrainTumorsPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/all-brain-tumors";

    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// The connection string is pushed in, exactly as every other render fixture
    /// in this suite does it: taking the factory as-is works where
    /// <c>dotnet user-secrets</c> supplies one and fails on CI, so the whole
    /// class goes green locally and red on the first push (§12.8, WI-528).
    /// </summary>
    public AllBrainTumorsPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    private HttpClient Client => _factory.CreateClient();

    [Fact]
    public async Task ThePageRenders()
    {
        var response = await Client.GetAsync(Url);
        Assert.True(response.IsSuccessStatusCode, $"{Url} returned {(int)response.StatusCode}");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("The short version", html, StringComparison.Ordinal);
        Assert.Contains("it has no name yet", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TheOutlookGateRendersClosed()
    {
        var html = await Client.GetStringAsync(Url);

        // Scoped to the outlook section: matching the first `<details>` in the
        // document reads the layout's mobile-nav disclosure and keeps passing
        // while the gate itself renders open.
        var start = html.IndexOf("What might happen over time", StringComparison.OrdinalIgnoreCase);
        Assert.True(start > 0, "the outlook heading did not render");

        var gate = Regex.Match(html[start..], @"<details[^>]*>");
        Assert.True(gate.Success, "the outlook gate did not render as a details element");
        Assert.DoesNotContain(" open", gate.Value, StringComparison.Ordinal);

        // The heading is OUTSIDE the gate, so the page outline is complete for
        // a reader who declines (§12.5). Derived from the <h2>'s CLOSING tag,
        // not from the id: `/review` found the first version unfailable, because
        // `id="…"` sits inside the same <h2> as the heading text and therefore
        // always precedes the gate whatever the gate is doing.
        Assert.Contains("id=\"what-might-happen-over-time\"", html, StringComparison.Ordinal);
        var headingEnd = html.IndexOf("</h2>", start, StringComparison.Ordinal);
        Assert.True(headingEnd > 0 && headingEnd < start + gate.Index,
            "the outlook heading rendered inside the gate");

        // The §12.5 warning is the container's, emitted once, not typed per page.
        Assert.Equal(1, Regex.Matches(html, "The next part is about outlook").Count);

        // No fence leaked, in any of WI-503's five fail-open spellings.
        Assert.DoesNotContain(":::", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSharedBlocksRenderTheirContentRatherThanTheirDirective()
    {
        var html = await Client.GetStringAsync(Url);

        foreach (var directive in new[]
                 { "[MECHANISM]", "[ESCALATION]", "[TUMOR-BOARD]", "[CAUSES]", "[CAREGIVER]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // A sentence from each, so a block that composes to nothing is caught
        // rather than a directive that merely disappeared.
        Assert.Contains("The skull is a closed box", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("When to call for help right now", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("For most brain tumors, nobody knows the cause", html,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Two people in the room hear more than one", html,
            StringComparison.OrdinalIgnoreCase);
        // Not "A tumor board is a meeting": the renderer wraps the glossary term,
        // so a literal that straddles the auto-linked words never matches.
        Assert.Contains("is a meeting where specialists look at one person's case", html,
            StringComparison.OrdinalIgnoreCase);

        // And the excluded block is not on the reader's screen by some other
        // route (§12.10, WI-528).
        Assert.DoesNotContain("The rules for naming brain tumors changed in 2021", html,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EveryLinkAndEveryDeepLinkResolves()
    {
        // The anchor id, not the heading text: the helper looks the section up
        // by id and passing prose silently checks nothing (§12.8).
        await CuratedPage.AssertLinksResolveIn(
            Client, Url, "where-to-get-support",
            "/get-help-now", "/tests/waiting-for-results", "/tests/biopsy", "/tests/mri",
            "/seizures/what-to-do", "/tumors");

        await CuratedPage.AssertFragmentLinksResolve(Client, Url);

        // EVERY DEEP LINK BY NAME. The helper is satisfied by any one fragment
        // resolving, so dropping the one that carries the page's whole hand-off
        // is green — and each of these is the reason the door exists rather than
        // a link to the top of a long page.
        var html = await Client.GetStringAsync(Url);
        foreach (var deep in new[]
                 {
                     "/tests/waiting-for-results#how-long-does-it-take",
                     "/tests/waiting-for-results#can-someone-else-look-at-my-slides",
                     "/tests/pathology-report#what-the-grade-means",
                 })
        {
            Assert.Contains(deep, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TheTooltipsThatShouldFireDoAndNoMarkerReachesTheReader()
    {
        // Checked in BOTH directions, because both are no-ops in the corpus's
        // history: a suppressed term the prose never uses suppresses nothing
        // (WI-512), and an expected tooltip for a term the prose never uses can
        // never fire (WI-515).
        var html = await Client.GetStringAsync(Url);

        // `needle biopsy` is an ALIAS of `stereotactic biopsy`, and this page
        // names the operation without explaining it — the explaining is
        // /tests/biopsy's, which is where the page routes. So the tooltip is
        // required here rather than suppressed, and the alias is the only thing
        // that makes it fire.
        var glossary = File.ReadAllText(
            Path.Combine(CuratedPage.BlocksRoot, "..", "glossary", "stereotactic-biopsy.md"));
        Assert.Contains("needle biopsy", glossary, StringComparison.OrdinalIgnoreCase);

        foreach (var term in new[] { "needle biopsy", "contrast dye", "radiologist", "tumor board" })
        {
            Assert.Contains(term, html, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var slug in new[]
                 { "def-stereotactic-biopsy", "def-contrast-dye", "def-radiologist" })
        {
            Assert.Contains($"popovertarget=\"{slug}\"", html, StringComparison.Ordinal);
        }

        // `tumor board` IS SUPPRESSED, and this is the one page where the
        // corpus-wide duplication it causes is unmissable. [TUMOR-BOARD] now has
        // a heading of its own here, so the block is the first thing in the
        // section — and the tooltip fires on its opening words and then repeats
        // the sentence it is attached to, verbatim, on the reader's screen.
        // Suppressing it is page-local and touches neither the block nor the
        // glossary entry; the composed page defines the term itself two
        // sections before the questions list, which is §12.8's own rule.
        //
        // Checked in BOTH directions (WI-512): the term is in the prose, so the
        // suppression is real rather than a no-op.
        Assert.DoesNotContain("popovertarget=\"def-tumor-board\"", html, StringComparison.Ordinal);
        Assert.Contains("is a meeting where specialists look at one person's case", html,
            StringComparison.OrdinalIgnoreCase);

        // And no authoring marker reaches the reader.
        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.DoesNotContain("%%", html, StringComparison.Ordinal);

        // The glossary entry this item reworded says the true-everywhere version
        // rather than the one that asserts a diagnosis (§12.10, WI-528: correct
        // an entry to be true everywhere, never patch it for one hub).
        Assert.DoesNotContain("when a tumor is hard or risky to reach", glossary,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("the area to be sampled is hard or", CuratedPage.Flatten(glossary),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TheTumorsIndexOffersTheDoorToThisPage()
    {
        // A list of names is the wrong shape for a reader who has not been given
        // one, and taxonomy.yml files this slug under "Ways to group tumors —
        // these are not types", at the bottom of the page. Without a door at the
        // top, the page written for the most frightened reader on the site is
        // the hardest one to reach.
        var html = await Client.GetStringAsync("/tumors");

        var start = html.IndexOf("<h1", StringComparison.Ordinal);
        var firstGroup = html.IndexOf("<h2", StringComparison.Ordinal);
        Assert.True(start >= 0 && firstGroup > start, "/tumors did not render its heading and groups");

        Assert.Contains("Not been given a name yet?", html[start..firstGroup],
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Url, html[start..firstGroup], StringComparison.Ordinal);

        // AND THE ROW AT THE BOTTOM. The slug sits in `CrossCutting` under a
        // heading that says "these are not types", and the moment this page
        // existed that row started rendering the taxonomy label over the page's
        // own description — so a reader who DOES have a diagnosis clicks
        // "All brain tumors" expecting a filter and lands on a page that opens
        // by telling them they have no diagnosis. Before this item the row
        // honestly said "We are still writing this one."
        //
        // The LABEL stays: `TumorsPageTests` requires every taxonomy label to
        // appear here so the filter and this list cannot drift apart, and that
        // invariant is worth more than the wording. The SUMMARY is what carries
        // who the link is for.
        var row = html.IndexOf($"href=\"{Url}\"", firstGroup, StringComparison.Ordinal);
        Assert.True(row > 0, "the grouped index no longer links to this page at all");
        Assert.Contains("All brain tumors", html[row..], StringComparison.Ordinal);
        Assert.Contains("has not been given a name for it yet", html[row..],
            StringComparison.Ordinal);
    }
}
