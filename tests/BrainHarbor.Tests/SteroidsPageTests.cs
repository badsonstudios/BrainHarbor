using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-524: X11, steroids. The sixth TREATMENT page under the §12.8 LIBRARY
/// template (the shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs), and the canonical home for a message the corpus
/// has been carrying in pieces since WI-510: this might be the drug, not the
/// tumor.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * the backlog's own headline figure. "Dexamethasone causes proximal
///     muscle weakness in ~28%" is in docs/backlog.md, SYNTHESIS §3.5 and
///     content-pipeline §12.6, cited by the dossier to PMC12406498 — a paper
///     that contains the word "proximal" zero times. Seven sources give seven
///     answers (10%, 2-60%, 10-90%, 10.6%, 60%, 4.5%, and the dossier's 28%
///     from 88 people with brain metastases). No frequency is published.
///   * "it does not treat the tumor", which is true of nearly every reader of
///     this page and FALSE for lymphoma (§12.12).
///   * "never stop suddenly", which is true of a long course and not of a
///     three-day one — and the rule that actually holds is "not on your own".
///   * a page that teaches a frightened reader to decide for themselves
///     whether new weakness is the drug or the tumor. The two need opposite
///     things done about them, so deciding at home is the one outcome this
///     page must never produce.
///
/// Every claim-shaped guard below carries a CANARY: a known-bad sentence it
/// must match before its pass on the real page means anything (§12.8, WI-523 —
/// that item's frequency guard had a half that could never fire).
/// </summary>
public sealed class SteroidsPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "steroids.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section as the reader meets it. Not <c>CuratedPage.ReaderText</c>,
    /// which strips front matter by index and returns <c>section[3..]</c> when
    /// fed a section (§12.8, WI-520).
    /// </summary>
    private static string Reader(string section) =>
        // Both marker shapes, as the renderer strips them. The first version
        // handled only `!%term%`; the first `%%term%%` added to this page would
        // have silently corrupted every section-scoped assertion below.
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The title and description, which <c>ReaderText</c> strips and every
    /// body-scoped guard below is therefore blind to — while
    /// <c>ContentPage.cshtml</c> renders the description as the first
    /// paragraph under the heading, where §12.3 says most of the reading
    /// happens. /review found the description asserting the exact claim this
    /// page exists to correct.
    /// </summary>
    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            // `\s*$` before the anchor, not `"$`: on a CRLF checkout the line
            // ends `"\r\n`, the match fails, and every guard over the headline
            // then runs on an empty string. The break harness caught this by
            // reporting a mutation as "correctly failing" on CRLF while it
            // walked through green on LF (§12.10: prove it can fire).
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    /// <summary>
    /// The "### The weakness in your legs and arms" subsection alone.
    /// <c>CuratedPage.Section</c> only splits on <c>## </c>.
    /// </summary>
    private static string Subsection(string parent, string heading)
    {
        var outer = Regex.Match(Page, @"^## " + Regex.Escape(parent) + @".*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        var sub = Regex.Match(outer, @"^### " + Regex.Escape(heading) + @".*?(?=^### |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(sub), $"the '{heading}' subsection has gone");
        return CuratedPage.Flatten(Reader(sub));
    }

    /// <summary>
    /// A count, however this corpus writes it. Copied rather than shared, for
    /// the reason WI-522 gives: a frequency guard and a schedule guard need
    /// different vocabularies (§12.8, WI-521).
    /// </summary>
    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|"
        + @"fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|"
        + @"seventy|eighty|ninety|hundred|dozen)";

    private const string Fraction = @"(?:half|halves|thirds?|quarters?|fifths?|tenths?)";

    /// <summary>
    /// Word runs this page shares with another page ON PURPOSE. Kept tiny for
    /// the WI-521 reason: every entry is a link label or a safety claim stated
    /// at one strength across the corpus (§12.10).
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // The standard "Where to go next" door, worded identically on every
        // library page so a reader moving between them recognises it.
        "Get help now if you need to talk to a person today.",

        // The corpus's ambulance line for a sudden, complete deficit, in the
        // words Content/blocks/escalation.md uses on every hub. A safety claim
        // that must not have two strengths (§12.10), so it is the identical
        // sentence rather than this page's own version of it.
        "suddenly not being able to speak, move one side, or see",
    ];

    private static readonly HashSet<string> AllowedShingles =
        [.. DeliberatelyShared.SelectMany(s => Shingles(s, 8))];

    [Fact]
    public void TheShortVersionCarriesTheSpineAndBothHalvesOfIt()
    {
        // §12.3: readers consume 20 to 28% of a page, so the two claims that
        // make this page worth having live in the summary, not only below.
        var summary = Reader(Section("The short version"));
        var sentences = CuratedPage.SentencesOf(summary.Replace("**", ""));

        // Half one: the relief is real AND the tumor has not changed. Dropping
        // either half produces a different page (§12.8, WI-506).
        var swelling = Array.FindIndex(sentences, s =>
            Regex.IsMatch(s, @"swelling", RegexOptions.IgnoreCase));
        Assert.True(swelling >= 0, "the short version no longer says what the steroid acts on");
        Assert.Matches(new Regex(@"does nothing to the tumor itself|not on the tumor", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(summary));
        Assert.Matches(new Regex(@"feel much better .*? while the tumor is exactly as it was",
            RegexOptions.IgnoreCase), CuratedPage.Flatten(summary));

        // Half two: the drug has effects that look like the tumor winning, and
        // the instruction that follows from it.
        Assert.Matches(new Regex(@"look like the tumor getting worse when they are not", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(summary));
        Assert.Contains("**Do not stop a steroid on your own.**", CuratedPage.Flatten(Section("The short version")),
            StringComparison.Ordinal);

        // And the honest half of the benefit: not everybody improves. A summary
        // that promises improvement is the over-reassuring direction (§12.12).
        Assert.Matches(new Regex(@"Not everybody does", RegexOptions.IgnoreCase), CuratedPage.Flatten(summary));
    }

    [Fact]
    public void NoFrequencyIsPublishedForTheMuscleWeaknessAndThePageSaysWhyNot()
    {
        // THE ITEM'S RULING. §12.4 R2 as WI-521 extended it to frequencies:
        // when the spread is about what each study counted, the page publishes
        // none of them AND says so, because a page that is silently silent
        // leaves the reader to supply their own figure.
        var weakness = Subsection("Is it the tumor, or is it the steroid?", "The weakness in your legs and arms");

        // A frequency claim: a count or a fraction, then a population, looking
        // FORWARD from the number (§12.8, WI-521 — English puts the population
        // after the count).
        // THE GUARD RUNS OVER THE WHOLE PAGE, not this subsection. /review put
        // "Proximal weakness turns up in about twenty-eight in every hundred
        // people on a high dose" into "The rest of the side effects" and the
        // section-scoped version never looked there — the item's own headline
        // ruling, reintroduced two screens below the section named for it.
        //
        // The population list is wide because /review walked the narrow one
        // three ways: "long-course users", "everyone on a long course", "those
        // on a high dose". Anything that names a group of humans counts.
        var frequency = new Regex(
            $@"\b(?:{CountWord}|{Fraction}|most|many|few|rare|common|plenty|majority|almost|nearly)\b[^.]{{0,40}}"
            + @"\b(?:of )?(?:people|patients|users|everyone|everybody|anyone|anybody|nobody|adults|men|women|"
            + @"sufferers|cases|those|them|us|in every|in a hundred)\b",
            RegexOptions.IgnoreCase);

        // CANARIES FIRST (§12.8, WI-523), including the four /review beat the
        // first version with. Without these the pass below proves only that the
        // regex compiles.
        foreach (var known in new[]
                 {
                     "It happens to about one in three people.",
                     "Around a quarter of people on a steroid get it.",
                     "It affects most people who take it for a month.",
                     "It affects something like a third of long-course users.",
                     "Around a quarter of everyone on a long course gets it.",
                     "Proximal weakness turns up in about twenty-eight in every hundred people on a high dose.",
                     "Most people have their strength back within a couple of weeks.",
                     "It is what this medicine does to almost everybody who takes it for long.",
                 })
        {
            Assert.Matches(frequency, known);
        }

        // Sentences on the page that ARE allowed to carry one, named rather
        // than pattern-exempted (§12.8, WI-521: a deliberately shared or
        // deliberately kept sentence is a short, commented allowlist).
        string[] allowed =
        [
            // PMC4059813 verbatim in substance: "Most patients begin to improve
            // symptomatically within hours". A speed claim, not a share of
            // people who benefit — the share is the one /review caught the page
            // publishing from the more reassuring of two disagreeing sources.
            "Most people start to feel better within hours",
            "Most people improve on it",

            // The sentence that REFUSES a share. It has to be able to say
            // "everybody" and "how many" to do that.
            "Not everybody improves",

            // ULH: "Many patients are able to stop taking their Dexamethasone
            // at the end of their treatments." One source, said at that
            // source's strength.
            "Many people come off it once their treatment is over",

            // The audit, attributed in its own sentence to one UK center over
            // three months (§12.8, WI-507).
            "In one audit at a UK center, over three months",
        ];

        var offenders = CuratedPage.SentencesOf(Body)
            .Where(s => frequency.IsMatch(s))
            .Where(s => !allowed.Any(a => s.Contains(a, StringComparison.Ordinal)))
            .ToList();

        Assert.True(offenders.Count == 0,
            "this page publishes a frequency:\n  " + string.Join("\n  ", offenders));

        // Every exemption is still earning its place, so the list cannot rot
        // into a blanket (§12.10: prove it can fire).
        foreach (var sentence in allowed)
        {
            Assert.Contains(sentence, Body, StringComparison.Ordinal);
        }

        // And the guard must not read the studies-agree sentence as a count of
        // people. Same shape as WI-521's "a lot of people read it as one on the
        // drive home": the population has to be named, not pronouned.
        Assert.Contains("What they all agree on is the direction", weakness, StringComparison.Ordinal);

        // And the absence is stated rather than left as a hole.
        Assert.Contains("no honest number to give you", weakness, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"counted it in different doses over different lengths of time",
            RegexOptions.IgnoreCase), weakness);

        // What the sources DO agree on is printed, because "no number" is not
        // the same as "no information" (§12.8, WI-523: silence is for spreads).
        Assert.Contains("the higher the dose and the longer you are on it", weakness, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDossiersTwentyEightPercentIsNowhereOnThePageOrInItsCitations()
    {
        // §12.14: banning a URL is not fixing a claim, so the ban and the grep
        // for the claim it was carrying run together. Here the claim came with
        // no bad URL at all — the citation resolves, and simply does not
        // contain the figure.
        foreach (var figure in new[] { "28", "46", "24", "20 percent", "%" })
        {
            Assert.DoesNotContain(figure, Body, StringComparison.OrdinalIgnoreCase);
        }

        // The same four figures written out. /review beat the first version of
        // this list with "twenty-eight in every hundred people", which is the
        // dossier's own number in words (§12.8, WI-511: a number written as a
        // word is still a number).
        foreach (var claim in new[]
                 {
                     "one in three", "one in four", "one in five", "a quarter of", "a fifth of",
                     "half of people", "nearly half", "twenty-eight", "twenty eight", "forty-six",
                     "in every hundred", "in a hundred",
                 })
        {
            Assert.DoesNotContain(claim, Body, StringComparison.OrdinalIgnoreCase);
        }

        // And the title and description, which every body-scoped guard here is
        // blind to (§12.3: the description renders under the heading).
        foreach (var claim in new[] { "28", "%", "twenty-eight", "one in " })
        {
            Assert.DoesNotContain(claim, Headline, StringComparison.OrdinalIgnoreCase);
        }

        // And the front matter says WHY, where the next reader will see it
        // (§12.13). Flattened and de-commented: the block is hard-wrapped.
        var notes = CuratedPage.Flatten(Regex.Replace(CuratedPage.FrontMatter(Page), @"(?m)^\s*#\s?", " "));
        Assert.Contains("THE BACKLOG'S HEADLINE FIGURE IS NOT PUBLISHED", notes, StringComparison.Ordinal);
        Assert.Contains("contains the word \"proximal\" ZERO times", notes, StringComparison.Ordinal);
        Assert.Contains("Seven answers, none of them each other", notes, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoDoseAndNoMilligrams()
    {
        // §12.4 R1: every mg figure stays off the site, and a dose is the
        // number a frightened reader can act on tonight (§12.8, WI-520).
        var dose = new Regex(@"\b\d+(?:\.\d+)?\s?(?:mg|milligram)|\b(?:mg|milligrams?)\b", RegexOptions.IgnoreCase);
        Assert.Matches(dose, "The usual starting dose is 16 mg a day.");
        Assert.DoesNotMatch(dose, Body);

        // Including the shapes that avoid the unit: "four times a day",
        // "two tablets in the morning".
        var schedule = new Regex(
            $@"\b{CountWord}\b[^.]{{0,25}}\b(?:times a day|tablets?|pills)\b|"
            + $@"\b(?:take|takes|taking)\b[^.]{{0,20}}\b{CountWord}\b[^.]{{0,20}}\b(?:a day|each day|daily)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(schedule, "Most people take it two times a day.");
        Assert.Matches(schedule, "You take four pills in the morning.");
        Assert.DoesNotMatch(schedule, Body);
    }

    [Fact]
    public void ThePageNeverTellsTheReaderWhichCauseTheirWeaknessIs()
    {
        // The defect this page could do the most harm with. Steroid weakness
        // and tumor weakness need OPPOSITE changes to the dose, so a sentence
        // that lets a reader sort it out at home is worse than no page.
        //
        // Claim-shaped: a pattern word, then a verdict.
        // /review beat the first version with "comes from the steroid rather
        // than the tumor" and with "the steroid is the likelier cause" — the
        // verdict half held only `it is|that is|means|it will be`. The trigger
        // half is wider too, and the guard now runs over the title as well,
        // where the first version of this page promised the discriminator in
        // the heading (§12.3: most readers get no further).
        var verdict = new Regex(
            @"\b(?:both (?:legs|sides|arms|thighs)|if it is gradual|if it came on slowly|slowly|gradual|"
            + @"the feeling is normal|reflexes|if it is the (?:thighs|shoulders)|when it is on both|"
            + @"stairs|a low chair)\b[^.]{0,80}"
            + @"(?:\b(?:it is|that is|means|it will be|comes? from|points? to|is (?:usually|probably|likely)|"
            + @"likelier|more likely to be|tells you)\b[^.]{0,40}"
            + @"\b(?:the (?:drug|steroid|medicine|tumor)|not the (?:tumor|drug|steroid))\b"
            // Both word orders. /review's mutation put the noun first: "the
            // steroid is the likelier cause".
            + @"|\bthe (?:drug|steroid|medicine)\b[^.]{0,25}\b(?:is|as|being)\b[^.]{0,25}"
            + @"\b(?:likelier|more likely|likely cause|usual cause|probably|the cause)\b)",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "If both legs are weak, it is the drug and not the tumor.",
                     "If it came on slowly, that is the steroid.",
                     "Weakness in both legs usually comes from the steroid rather than the tumor.",
                     "If both thighs are weak and the feeling in them is normal, the steroid is the likelier cause.",
                     "Trouble with the stairs points to the drug.",
                 })
        {
            Assert.Matches(verdict, known);
        }

        Assert.DoesNotMatch(verdict, Body);
        Assert.DoesNotMatch(verdict, Headline);

        // The headline's own shape, which needs no symptom trigger to promise
        // the discriminator: "when it IS the drug and not the tumor" is the
        // verdict, and it is the second line of the page.
        var promise = new Regex(@"\bwhen it is the (?:drug|steroid|medicine)\b", RegexOptions.IgnoreCase);
        Assert.Matches(promise, "Steroids: when it is the drug and not the tumor");
        Assert.DoesNotMatch(promise, Headline);
        Assert.Matches(new Regex(@"when it might be the drug and not the tumor", RegexOptions.IgnoreCase), Headline);

        // The same verdict grammar on the siblings that carry the frame. This
        // item deleted one from /tumors/glioblastoma ("that is the drug"), and
        // /review put it back in a different shape ("Nine times out of ten it
        // is the steroid") without either sibling test noticing.
        foreach (var (folder, slug) in new[]
                 {
                     ("tumors", "glioblastoma"), ("tumors", "high-grade-glioma"), ("tumors", "astrocytoma"),
                     ("tumors", "low-grade-glioma"), ("treatments", "craniotomy"), ("treatments", "radiation-therapy"),
                 })
        {
            var sibling = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(folder, slug + ".md")));
            var odds = new Regex(
                @"\b(?:nine times out of ten|usually|most often|probably|normally)\b[^.]{0,30}"
                + @"\bit is the (?:drug|steroid|medicine)\b|\bthat is the (?:drug|steroid|medicine)\b",
                RegexOptions.IgnoreCase);
            Assert.Matches(odds, "Nine times out of ten it is the steroid.");
            Assert.DoesNotMatch(odds, sibling);
        }

        // The page says the opposite outright, and routes.
        var weakness = Subsection("Is it the tumor, or is it the steroid?", "The weakness in your legs and arms");
        Assert.Contains("not something to work out at home", weakness, StringComparison.Ordinal);
        Assert.Contains("The two need opposite things", weakness, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"It is a phone call, not a wait for the next appointment", RegexOptions.IgnoreCase),
            weakness);

        // §12.6: never end a section on the frightening sentence. Anchored to
        // the END of the last sentence, not a window (§12.8, WI-521).
        Assert.Matches(new Regex(@"which matters if you are already unsteady\.?\s*$"), weakness.TrimEnd());
    }

    [Fact]
    public void NothingOnThePageReadsAsPermissionToChangeTheDose()
    {
        // The corollary. Every sentence about changing a dose has to be an
        // instruction NOT to, or an instruction to phone.
        // /review beat the first version three ways: "leave a dose out" was not
        // on the verb list, "halve your dose ... until your team can see you"
        // satisfied a check that only asked whether a team word was PRESENT,
        // and the whole loop ran over zero matches because the page writes
        // "stop a steroid" rather than "stop your steroid".
        var selfDosing = new Regex(
            @"\b(?:lower\w*|reduc\w*|cut|halv\w*|skip\w*|stop\w*|space out|come off|leave|leaving|"
            + @"miss\w*|take less|drop\w*|come down|bring\w* down|stretch\w*)\b[^.]{0,30}"
            + @"\b(?:your|the|a|an) (?:dose|doses|steroid|steroids|pills?|prescription)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "If the hunger is too much, lower your dose for a few days.",
                     "If the indigestion is bad, it is reasonable to halve your dose until your team can see you.",
                     "It is fine to leave a dose out if you are being sick.",
                     "You can skip the dose on a day you feel well.",
                 })
        {
            Assert.Matches(selfDosing, known);
        }

        // Questions are excluded: "What do I do if I miss a dose?" is the page
        // ASKING the team, which is the behaviour the guard exists to enforce.
        var mentions = CuratedPage.SentencesOf(Body + " " + Headline)
            .Where(s => selfDosing.IsMatch(s))
            .Where(s => !s.TrimEnd().EndsWith('?'))
            .ToList();
        Assert.True(mentions.Count >= 2,
            $"only {mentions.Count} sentence(s) about changing a dose were found, so the polarity check below "
            + "is running over almost nothing — widen the verb list rather than trusting the pass");

        // Named as well as counted, so the guard cannot pass on a page that has
        // quietly lost the prohibition itself (§12.10: prove it can fire).
        Assert.Contains("Do not stop, skip or halve a dose on your own", Body, StringComparison.Ordinal);
        Assert.Contains("do not let the bottle run\nout".Replace("\n", " "), Body, StringComparison.Ordinal);

        // POLARITY, not the presence of a team word. Every one of these has to
        // be a prohibition or a route, and none may be permission.
        foreach (var sentence in mentions)
        {
            Assert.True(
                Regex.IsMatch(sentence,
                    @"\b(?:do not|does not|never|not on your own|not (?:a|the) decision|your team|their decision|"
                    + @"call|tell them|by you|by yourself|yourself)\b", RegexOptions.IgnoreCase),
                "this page mentions changing a dose without the prohibition or a route to the team: " + sentence);

            Assert.False(
                Regex.IsMatch(sentence,
                    @"\b(?:it is (?:fine|ok|okay|reasonable|safe)|you can|you could|feel free|"
                    + @"if you feel ready|no harm in)\b", RegexOptions.IgnoreCase),
                "this page reads as permission to change a dose: " + sentence);
        }

        Assert.Contains("changing your own dose is the one thing not to do", Body, StringComparison.Ordinal);

        // A taper schedule is the other shape of the same danger: a number a
        // frightened reader can follow tonight (§12.8, WI-520).
        var schedule = new Regex(
            $@"\b(?:come down|drop|reduce|lower|cut)\b[^.]{{0,25}}\b(?:by )?(?:one |a |{CountWord} )?"
            + $@"(?:step|steps|level)\b[^.]{{0,25}}\bevery\b[^.]{{0,15}}\b(?:{CountWord}|other)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(schedule, "Come down by one step every three or four days if you feel ready.");
        Assert.DoesNotMatch(schedule, Body);
    }

    [Fact]
    public void TheStopRuleIsScopedToNotOnYourOwnRatherThanToAlwaysSlowly()
    {
        // §12.8 (WI-523): a sentence true of one case is false of another.
        // "A steroid always has to come down slowly" is false of a three-day
        // post-operative course, which PMC4059813 records as a fast taper and
        // PMC11451960 says is fine to stop outright under about three weeks.
        // The rule that holds in both cases is the one the page prints.
        var taper = Reader(Section("Why can I not just stop taking it?"));

        Assert.Contains("The rule is not \"always slowly\". The rule is \"not on your own\"",
            CuratedPage.Flatten(taper), StringComparison.Ordinal);
        Assert.Matches(new Regex(@"After a short course[^.]*a team may simply stop it", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(taper));

        // /review beat the first version with "A steroid is always brought down
        // slowly" — the verb list held `come down|reduced|tapered` and nothing
        // else — and found the same claim still live in the DESCRIPTION, which
        // renders under the heading and which ReaderText strips.
        var always = new Regex(
            @"\b(?:always|must always|has to|have to|is|are|must)\b[^.]{0,40}"
            + @"\b(?:come down|comes down|brought down|stepped down|reduced|tapered|lowered|weaned)\b"
            + @"[^.]{0,25}\b(?:slowly|gradually|in steps|over weeks)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "A steroid always has to be reduced slowly.",
                     "A steroid is always brought down slowly.",
                     "It must be tapered gradually.",
                     "A steroid always comes down in steps.",
                 })
        {
            Assert.Matches(always, known);
        }

        foreach (var sentence in CuratedPage.SentencesOf(CuratedPage.Flatten(taper) + " " + Headline)
                     .Where(s => always.IsMatch(s)))
        {
            Assert.True(
                Regex.IsMatch(sentence, @"\b(?:after a longer|longer course|if you have been on it|"
                    + @"more than a short|not (?:the rule|always))\b", RegexOptions.IgnoreCase),
                "the stop rule is stated as universal, which is false of a short course: " + sentence);
        }

        // And the sibling that a post-operative reader meets first, which is
        // the one case the universal version is false of (§12.10: read the
        // other page rather than assuming it agrees). /review found
        // /treatments/craniotomy still saying "have to be reduced slowly" and
        // "must not be stopped suddenly" four lines above this page's door.
        var cranio = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md")));
        foreach (var sentence in CuratedPage.SentencesOf(cranio).Where(s => always.IsMatch(s)))
        {
            Assert.True(
                Regex.IsMatch(sentence, @"\b(?:after a longer|longer course|never stopped by you|"
                    + @"not (?:the rule|always))\b", RegexOptions.IgnoreCase),
                "/treatments/craniotomy states the stop rule at a strength this page rules false: " + sentence);
        }

        Assert.DoesNotMatch(new Regex(@"must not be stopped suddenly", RegexOptions.IgnoreCase), cranio);
        Assert.Matches(new Regex(@"never stopped by you", RegexOptions.IgnoreCase), cranio);

        // And the reason, which is the thing that makes the rule stick. The
        // first draft of this page had the taper and not the why, and only the
        // end-to-end read found it.
        Assert.Matches(new Regex(@"your body stops making its own", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(taper));
        Assert.Matches(new Regex(@"nothing to step in, and\s+that can make you very ill", RegexOptions.IgnoreCase),
            taper);

        // The sick-day half, which is the other thing that gets people hurt:
        // PMC11451960 says the dose may need to GO UP during an acute illness,
        // and that only works if the person treating them knows.
        Assert.Matches(new Regex(@"tell anybody treating you that you are on a steroid", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(taper));
        Assert.Matches(new Regex(@"dose may have to go up for a few days", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(taper));
    }

    [Fact]
    public void TheDoesNotTreatTheTumorClaimIsScopedAndCarriesItsException()
    {
        // §12.12: a claim about "these tumors" on a page serving several must
        // name which ones. PMC4483077: "in lymphoma and leukemia, steroids
        // exert oncolytic effects", so the flat claim is false for the readers
        // of /tumors/cns-lymphoma, who are sent here.
        var what = CuratedPage.Flatten(Reader(Section("What is it, and what is it doing?")));

        Assert.Contains("There is one exception", what, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"For lymphoma in the brain, a steroid can shrink the tumor itself",
            RegexOptions.IgnoreCase), what);
        Assert.Contains("it can cost you the diagnosis", what, StringComparison.Ordinal);
        Assert.Contains("(/tumors/cns-lymphoma)", what, StringComparison.Ordinal);

        // The unqualified claim never appears anywhere on the page, title and
        // description included.
        //
        // /review walked the first version twice: "does nothing to the tumor"
        // matched none of `never|does not|do not`, and "never touches" fell
        // outside `touch` with its word boundary. It also found the loop
        // running over ZERO matches, which is §12.10's "a guard that iterates
        // over matches passes on a page with none" — so the count is asserted
        // before the scope is.
        var flat = new Regex(
            @"\b(?:steroids?|it|dexamethasone|this kind)\b[^.]{0,30}"
            + @"\b(?:never|does not|do not|does nothing|is not|are not|not on|rather than)\b[^.]{0,25}"
            // The verb is optional: "does nothing TO the tumor" carries the
            // claim with no verb at all, and that was /review's mutation.
            + @"(?:\b(?:treat\w*|touch\w*|shrink\w*|act\w* on|working on|aimed at|do\w* anything)\b[^.]{0,20})?"
            + @"\bthe tumou?r\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "A steroid does not treat the tumor.",
                     "A steroid does nothing to the tumor itself.",
                     "A steroid never touches the tumor.",
                     "It is not acting on the tumor.",
                 })
        {
            Assert.Matches(flat, known);
        }

        // Questions excluded: the questions list asks "which of the things I am
        // feeling are the steroid rather than the tumor?", which asserts
        // nothing about either.
        var claims = CuratedPage.SentencesOf(Body + " " + Headline)
            .Where(s => flat.IsMatch(s))
            .Where(s => !s.TrimEnd().EndsWith('?'))
            .ToList();
        Assert.True(claims.Count >= 2,
            $"only {claims.Count} sentence(s) on this page state the no-tumor-effect claim — either the page "
            + "has stopped making its central claim, or it has been phrased in a way this guard cannot see, "
            + "and the scope check below is then running over nothing");

        foreach (var sentence in claims)
        {
            Assert.True(
                Regex.IsMatch(sentence, @"\b(?:most|other|except|apart from|lymphoma)\b", RegexOptions.IgnoreCase),
                "this page states the no-tumor-effect claim without the scope that makes it true: " + sentence);
        }

        // The sibling still says the same thing, read rather than assumed
        // (§12.10). If /tumors/cns-lymphoma drops its biopsy-first rule, the
        // exception here needs rechecking.
        var lymphoma = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "cns-lymphoma.md")));
        Assert.Matches(new Regex(@"biopsy before starting steroids", RegexOptions.IgnoreCase), lymphoma);
        Assert.Contains("/treatments/steroids", lymphoma, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSpeedClaimIsSourcedAndArrivesWithItsNotEverybodyHalf()
    {
        // §12.14 recorded "steroids work quickly" as a claim whose only source
        // was a complications audit. It is now on two guideline-level sources,
        // and it is printed with the half that stops it becoming a promise:
        // one guideline puts improvement at about three in four.
        var steps = CuratedPage.Flatten(Reader(Section("What happens, step by step")));

        Assert.Matches(new Regex(@"Most people start to feel better within\s+hours", RegexOptions.IgnoreCase), steps);

        // And NO share is printed with it. /review found the first version
        // publishing PMC4059813's "three quarters", which is about people with
        // raised pressure from a primary brain tumor, while this step addresses
        // everybody on a steroid — and the page's other cited source
        // (PMC6700052) reports 50% improving in its own cohort. Two sources,
        // two answers, and the page had picked the more reassuring one and
        // widened its population (§12.8, WI-511).
        var honest = CuratedPage.SentencesOf(steps)
            .Where(s => s.Contains("Not everybody improves", StringComparison.Ordinal))
            .ToList();
        Assert.Single(honest);
        Assert.Contains("disagree about how many do", honest[0], StringComparison.Ordinal);
        Assert.Contains("nobody can promise you will", honest[0], StringComparison.Ordinal);

        foreach (var share in new[] { "three in four", "three quarters", "half of", "most of them" })
        {
            Assert.DoesNotContain(share, steps, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheEscalationTiersMatchTheRestOfTheCorpus()
    {
        // §12.8 (WI-512): an escalation tier is a site-wide property. This
        // page's list is steroid-specific rather than a second general list,
        // so the check is per SYMPTOM against the pages that already sort it.
        var section = CuratedPage.Flatten(Reader(Section("When to call, and what about")));

        var sameDay = Regex.Match(section, @"Call your team the same day if:(.*?)(?=\*\*Call an ambulance)",
            RegexOptions.Singleline);
        Assert.True(sameDay.Success, "the same-day list has gone");
        var today = sameDay.Groups[1].Value;

        var ambulance = Regex.Match(section, @"\*\*Call an ambulance(.*?)(?=\*\*Tell your team about these as well)",
            RegexOptions.Singleline);
        Assert.True(ambulance.Success, "the ambulance line has gone");
        var now = ambulance.Groups[1].Value;

        // Both captures carry text: a lazy group that matched nothing would
        // make every DoesNotContain below pass for the wrong reason.
        Assert.True(today.Length > 200 && now.Length > 60,
            "one of the two tiers captured almost nothing, so the checks below prove nothing");

        // Chest pain: /treatments/craniotomy files it as an ambulance, and
        // WI-512's blocker was /treatments/chemotherapy filing it as same-day.
        // ABTA says of the steroid list that it "may signal a medical
        // emergency", so this page sides with craniotomy.
        var cranio = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md")));
        var cranioAmbulance = Regex.Match(cranio, @"When to call an ambulance\.\*\*(.*?)(?=\*\*[A-Z]|\z)",
            RegexOptions.Singleline);
        Assert.True(cranioAmbulance.Success,
            "/treatments/craniotomy no longer has an ambulance list, so this page's tiers cannot be checked");
        Assert.Contains("chest pain", cranioAmbulance.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("chest pain", now, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("chest pain", today, StringComparison.OrdinalIgnoreCase);

        // New weakness: Content/blocks/escalation.md files it as same-day on
        // every hub, and this page's whole subject is a drug that causes it.
        // Filing it lower would be the under-triage direction (§12.8, WI-511).
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        Assert.Matches(new Regex(@"New weakness in the face, an arm or a leg", RegexOptions.IgnoreCase), block);
        Assert.Matches(new Regex(@"new weakness in an arm or a leg", RegexOptions.IgnoreCase), today);
        Assert.DoesNotMatch(new Regex(@"weakness", RegexOptions.IgnoreCase), now);

        // Confusion: same-day everywhere, and never an ambulance.
        Assert.Matches(new Regex(@"confused", RegexOptions.IgnoreCase), today);
        Assert.DoesNotMatch(new Regex(@"confus", RegexOptions.IgnoreCase), now);

        // The fever, which is the one this page could have softened. The block
        // sends a reader on chemotherapy to call RIGHT AWAY at any hour; a
        // flat "same day" bullet here would have been a quieter rule on the
        // page about the drug that hides a temperature. The sibling is read at
        // the section the link lands on (§12.10).
        var feverRule = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Section(
            CuratedPage.Read("treatments", "chemotherapy.md"), "Your blood counts, and the fever rule")));
        Assert.Matches(new Regex(@"straight away, at any hour", RegexOptions.IgnoreCase), feverRule);
        Assert.Matches(new Regex(@"a fever is a call right away, at any hour", RegexOptions.IgnoreCase), today);
        Assert.Contains("/treatments/chemotherapy#fever-rule", today, StringComparison.Ordinal);

        // A seizure is NOT on this page's list in any tier: /seizures/what-to-do
        // draws that line, and a flat "A seizure" bullet is WI-510's blocker,
        // which WI-519 then found re-committed. /review added one here and the
        // first version of this test, which only banned two phrasings of the
        // conclusion, stayed green.
        // Link labels stripped first: the ambulance paragraph ROUTES to
        // /seizures/what-to-do, which is the opposite of ruling on a seizure
        // itself, and a raw check reads the route as a bullet.
        var withoutLinks = new Func<string, string>(s => Regex.Replace(s, @"\[[^\]]*\]\([^)]*\)", " "));
        // Singular and word-bounded: the ambulance paragraph's closing sentence
        // says the seizure page "draws the line for seizures", which is the
        // route again rather than a bullet.
        var seizureBullet = new Regex(@"\bseizure\b", RegexOptions.IgnoreCase);
        Assert.Matches(seizureBullet, "- A seizure.");
        Assert.DoesNotMatch(seizureBullet, withoutLinks(today));
        Assert.DoesNotMatch(seizureBullet, withoutLinks(now));

        // The reverse direction: a bullet COUNT plus the concepts, because
        // deleting a bullet left every DoesNotContain above passing (§12.8,
        // WI-519: when you diff two lists, diff them both ways).
        var bullets = Regex.Matches(today, @"(?:^|\s)- ").Count;
        Assert.True(bullets >= 9,
            $"the same-day list is down to {bullets} bullets — a symptom has been dropped, "
            + "which is the under-triage direction (§12.8, WI-511)");

        foreach (var owed in new[]
                 {
                     "keep your pills down", "headache keeps building", "faint", "fever",
                     "being sick again and again", "confused", "new weakness", "trouble speaking",
                     "blood in your stool", "stumble or fall", "chickenpox",
                 })
        {
            Assert.Contains(owed, today, StringComparison.OrdinalIgnoreCase);
        }

        // A fall is same-day on /treatments/craniotomy and /tests/biopsy, and
        // /review found this page had invented a gentler third tier for it
        // while saying 50 lines earlier that a steroid makes bones easier to
        // break. Read the siblings rather than trusting the memory of them.
        foreach (var (folder, slug) in new[] { ("treatments", "craniotomy"), ("tests", "biopsy") })
        {
            var sibling = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(folder, slug + ".md")));
            Assert.Matches(new Regex(@"A fall, or a hit to the head", RegexOptions.IgnoreCase), sibling);
        }

        var later = Regex.Match(section, @"\*\*Tell your team about these as well:\*\*(.*)$",
            RegexOptions.Singleline);
        Assert.True(later.Success, "the third tier has gone");
        Assert.DoesNotMatch(new Regex(@"\bfall\b|stumbl", RegexOptions.IgnoreCase), later.Groups[1].Value);

        // And the list says outright that it is not the reader's whole list,
        // because a steroid list read as complete is a tier-diff liability
        // with no reader benefit (§12.8, WI-520).
        Assert.Contains("It does not replace the list your own team gives you", section, StringComparison.Ordinal);
        Assert.Contains("(/get-help-now)", section, StringComparison.Ordinal);

        // The seizure line routes rather than ruling, so this page cannot
        // contradict /seizures/what-to-do (§12.8, WI-510).
        Assert.Contains("(/seizures/what-to-do)", section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"a seizure is an ambulance|any seizure", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheRecoveryFromTheWeaknessIsNotPromisedQuickly()
    {
        // PMC4483077: "recovery usually takes weeks to months"; Batchelor
        // followed people for more than three months off steroids. A page that
        // says the strength comes back "soon" sets up a second fright when it
        // does not.
        var weakness = Subsection("Is it the tumor, or is it the steroid?", "The weakness in your legs and arms");

        Assert.Contains("weeks to months rather than days", weakness, StringComparison.Ordinal);

        var quick = new Regex(
            @"\b(?:strength|weakness|muscles?)\b[^.]{0,40}\b(?:quickly|soon|straight away|right away|within days|in a few days)\b|"
            + @"\b(?:quickly|soon|straight away|within days)\b[^.]{0,30}\b(?:strength|back to normal)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(quick, "The strength comes back quickly once the dose drops.");
        Assert.DoesNotMatch(quick, Body);

        // The phrase ban above is not the property. The property is that this
        // page makes ONE claim about how long the strength takes to come back:
        // /review appended "Most people have their strength back within a
        // couple of weeks", which uses none of the banned words and contradicts
        // the sentence three lines above it (§12.8, WI-510: a claim about
        // recovery cannot have two strengths on one page).
        var recovery = new Regex(
            @"\b(?:strength|weakness|muscles?)\b[^.]{0,60}\b(?:days?|weeks?|months?)\b|"
            + @"\b(?:days?|weeks?|months?)\b[^.]{0,40}\b(?:strength|weakness|muscles?)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(recovery, "Most people have their strength back within a couple of weeks.");

        var spans = CuratedPage.SentencesOf(Body).Where(s => recovery.IsMatch(s)).ToList();
        Assert.NotEmpty(spans);
        foreach (var sentence in spans)
        {
            Assert.True(
                sentence.Contains("weeks to months", StringComparison.Ordinal)
                || sentence.Contains("over weeks", StringComparison.Ordinal)
                || sentence.Contains("unusual in the first days", StringComparison.Ordinal),
                "this page gives a second, different answer for how long the weakness takes: " + sentence);
        }
    }

    [Fact]
    public void TheScanSectionDoesNotMakeTheRanoEntryLiveByAccident()
    {
        // /tests/follow-up-scans owns RANO and pins the glossary entry as
        // reachable nowhere (WI-521). This page describes the same rules in
        // plain words and deliberately does not name them, so that pin stays
        // true and this page stays at 4.6.
        Assert.DoesNotContain("RANO", Body, StringComparison.OrdinalIgnoreCase);

        var scans = CuratedPage.Flatten(Reader(Section("Why does my team ask about my steroid dose before a scan?")));
        Assert.Matches(new Regex(@"take your steroid dose into account", RegexOptions.IgnoreCase), scans);
        Assert.Contains("(/tests/follow-up-scans)", scans, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // §12.8 (WI-521): the restatement check runs over the WHOLE corpus.
        // This page's nearest neighbours are /treatments/craniotomy and
        // /treatments/radiation-therapy, which have carried dexamethasone
        // paragraphs since WI-510 and WI-511, and the guard deliberately does
        // not name them.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("steroids.md", StringComparison.Ordinal))
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
                $"this page restates {slug}:\n  " + string.Join("\n  ", overlaps)
                + "\n\nWrite this page's own version, or — if the two really are meant to say the identical "
                + "thing — add the sentence to DeliberatelyShared with the reason.");
        }
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheBlockAndSitsAfterSlotSeven()
    {
        // §12.7 and contract item 12: a treatment page with meaningful
        // aftercare carries one, built from the block plus what is true only
        // here. Placed after slot 7 and before slot 8 (§12.8).
        var raw = Regex.Match(Page, @"^## For the person caring for someone on steroids.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.Contains("[CAREGIVER]", raw, StringComparison.Ordinal);

        var headings = Headings();
        Assert.Equal(headings.IndexOf("What you can do") + 1,
            headings.IndexOf("For the person caring for someone on steroids"));
        Assert.Equal(headings.IndexOf("For the person caring for someone on steroids") + 1,
            headings.IndexOf("Why has my dose gone up again?"));

        // The page's own caregiver prose does not restate the block — the
        // shingle check, not the bold-lead-in check §12.8 calls insufficient
        // (WI-519).
        var block = CuratedPage.Flatten(Regex.Replace(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")), @"^---.*?\n---", "",
            RegexOptions.Singleline));
        var blockShingles = Shingles(block, 8).ToHashSet();
        var own = Shingles(Reader(Section("For the person caring for someone on steroids")), 8)
            .Where(blockShingles.Contains).Where(CarriesContent).ToList();
        Assert.True(own.Count == 0, "the caregiver section restates the block:\n  " + string.Join("\n  ", own));

        // And it carries the two things that need a call today rather than
        // inventing a third tier of its own.
        var care = CuratedPage.Flatten(Reader(Section("For the person caring for someone on steroids")));
        Assert.Matches(new Regex(@"not being able to keep the pills down", RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"old symptoms creeping back as the dose drops", RegexOptions.IgnoreCase), care);

        // And it does not claim those are the only two worth calling about:
        // the same-day list above has more, and four of them are ones a
        // caregiver notices first (§12.8, WI-509: a uniqueness claim on a page
        // of many entries is a claim about all the others).
        Assert.DoesNotMatch(new Regex(@"the (?:only )?two that need a call", RegexOptions.IgnoreCase), care);
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8: read the section for the slot list, not the last page written.
        //
        // Carried: 0, 1, 2, 3, 4, the WI-507 "why" section immediately after
        // slot 4 (on a page whose whole difficulty is the duration, splitting
        // how-long from why-you-cannot-just-stop puts the reader's question two
        // screens from its answer), 5, 6a, 6b, 6c, the steroid-specific call
        // list, 7, the caregiver section, 8, 9, 10, 11.
        Assert.Equal(
            [
                "The short version",                                        // 0
                "What is it, and what is it doing?",                        // 1
                "Why am I taking one?",                                     // 2
                "What happens, step by step",                               // 3
                "How long will I be on it?",                                // 4
                "Why can I not just stop taking it?",                       // 4b (WI-507)
                "What does it feel like?",                                  // 5
                "Is it the tumor, or is it the steroid?",                   // 6a
                "The rest of the side effects",                             // 6b
                "Why does my team ask about my steroid dose before a scan?", // 6c
                "When to call, and what about",                             // escalation
                "What you can do",                                          // 7
                "For the person caring for someone on steroids",            // caregiver
                "Why has my dose gone up again?",                           // 8
                "Who is in charge of my steroid?",                          // 9
                "What to ask your team",                                    // 10
                "Where to go next",                                         // 11
            ],
            Headings());
    }

    [Fact]
    public void TheAnchorsArePinned()
    {
        // §12.8 (WI-508): heading anchors are a published interface, and this
        // page is the canonical home other pages will deep-link into.
        // Each anchor on ITS heading: a whole-page Contains passes when an
        // anchor has been moved onto a different heading.
        foreach (var (heading, anchor) in new[]
                 {
                     ("## The short version", "short-version"),
                     ("## What is it, and what is it doing?", "what-it-is"),
                     ("## Why am I taking one?", "why"),
                     ("## What happens, step by step", "step-by-step"),
                     ("## How long will I be on it?", "how-long"),
                     ("## Why can I not just stop taking it?", "why-taper"),
                     ("## What does it feel like?", "what-it-feels-like"),
                     ("## Is it the tumor, or is it the steroid?", "tumor-or-steroid"),
                     ("### The weakness in your legs and arms", "muscle-weakness"),
                     ("### The other things that get blamed on the tumor", "not-the-tumor"),
                     ("## The rest of the side effects", "side-effects"),
                     ("## Why does my team ask about my steroid dose before a scan?", "steroids-and-scans"),
                     ("## When to call, and what about", "when-to-call"),
                     ("## What you can do", "what-you-can-do"),
                     ("## For the person caring for someone on steroids", "caregiver"),
                     ("## Why has my dose gone up again?", "dose-up"),
                     ("## Who is in charge of my steroid?", "who-is-in-charge"),
                     ("## What to ask your team", "questions"),
                     ("## Where to go next", "where-to-go-next"),
                 })
        {
            Assert.Matches(new Regex("^" + Regex.Escape(heading) + @" \{#" + anchor + @"\}\s*$",
                RegexOptions.Multiline), Page);
        }

        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline);
        Assert.All(headings, m => Assert.Matches(@"\{#[a-z0-9-]+\}\s*$", m.Groups[1].Value.Trim()));
    }

    [Fact]
    public void ThePageUsesNeitherTheCharacterisationsNorTheMinimisations()
    {
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "treatments/steroids");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, per §12.8 (WI-510: promote at the SECOND page). These
        // only mean something on a page about a drug people are afraid of, and
        // all three are corpus-clean.
        foreach (var phrase in new[] { "just a steroid", "nothing to worry about", "harmless" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // Three of this page's four patient-level sources are NHS documents,
        // so the pull toward "tablets", "specialist nurse" and "consultant" is
        // stronger here than anywhere in the corpus (§12.8, WI-510).
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
                     "tumour", "oedema", "consultant", "specialist nurse", "physiotherapist", "tablets",
                     "district nurse", "steroid card", "chemist",
                 })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // Word-bounded: a bare case-insensitive "GP" fires on the first
        // innocent word that happens to contain those letters.
        Assert.DoesNotMatch(new Regex(@"\bGP\b"), body);

        // The US forms the page uses instead, so a rewrite cannot quietly drop
        // the concept rather than translating it.
        Assert.Contains("physical therapist", body, StringComparison.Ordinal);
        Assert.Contains("family doctor", body, StringComparison.Ordinal);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate() =>
        Assert.DoesNotContain(":::", Page);

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        var headings = Headings();
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);

        // The questions carry the two that protect the reader between
        // appointments, per the audit this page's slot 9 is built on.
        var questions = Reader(Section("What to ask your team"));
        Assert.Matches(new Regex(@"can I have it in writing", RegexOptions.IgnoreCase), questions);
        Assert.Matches(new Regex(@"Who do I call about the steroid", RegexOptions.IgnoreCase), questions);
        Assert.Matches(new Regex(@"cannot keep it down", RegexOptions.IgnoreCase), questions);
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

        // The claims rest on these: ABTA for the US patient-level list, the
        // Christie and ULH leaflets for what it is like to take one, and PMC
        // for the guideline and review material.
        foreach (var domain in new[] { "abta.org", "christie.nhs.uk", "ulh.nhs.uk", "ncbi.nlm.nih.gov" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // §12.14 and the known-dead list.
        foreach (var dead in new[]
                 {
                     "medscape.com", "sciencedirect.com", "link.springer.com", "dipg.org",
                     "academic.oup.com", "mayoclinic.org", "hopkinsmedicine.org",
                 })
        {
            Assert.False(urls.Any(u => u.Contains(dead, StringComparison.OrdinalIgnoreCase)),
                $"{dead} is cited, and the front matter records why it was not used");
        }

        // PLAN.md §5: never an AHFS drug monograph, which is the easiest
        // mistake to make on the one page in the corpus about a drug.
        Assert.DoesNotContain("medlineplus.gov/druginfo", front, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("drugs.com", front, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheAuditIsAttributedInTheSentenceThatUsesIt()
    {
        // §12.8 (WI-507): a finding from one country's audit is attributed in
        // the sentence that prints it, not in the source list. This one is a
        // single UK center over three months, and it is being used to tell a
        // reader that the plan they were not given is a normal thing to be
        // missing.
        var who = CuratedPage.Flatten(Reader(Section("Who is in charge of my steroid?")));

        var sentence = CuratedPage.SentencesOf(who)
            .Single(s => s.Contains("nothing written down", StringComparison.Ordinal));
        Assert.Contains("In one audit at a UK center", sentence, StringComparison.Ordinal);

        // And it is used for what it found, not for the figure the dossier
        // hung on it.
        Assert.DoesNotMatch(new Regex(@"\b28|\bproximal", RegexOptions.IgnoreCase), who);
    }

    [Fact]
    public void TheDoorsOnTheSiblingPagesAreAppendedSentences()
    {
        // §12.8 (WI-519): a new library page nothing links to is half shipped.
        // SYNTHESIS §3.5 makes X11 the canonical home, so every page already
        // carrying a steroid paragraph routes here — and each door is an
        // APPENDED sentence, so no sibling's own assertions can break.
        // APPENDED, not swapped in. /review's point: a door that REPLACED a
        // sibling's paragraph passes a bare Contains identically, and the test
        // name is then doing the reassuring (§12.14). So each sibling's
        // pre-existing sentence is pinned beside the new door.
        foreach (var (folder, slug, kept) in new[]
                 {
                     ("treatments", "craniotomy", "Dexamethasone**, the steroid used to settle swelling, causes muscle"),
                     ("treatments", "radiation-therapy", "is the steroid used to settle swelling around a tumor"),
                     ("tumors", "glioblastoma", "They also affect sleep, appetite, mood and blood sugar"),
                     ("tumors", "high-grade-glioma", "Steroids cause muscle weakness"),
                     ("tumors", "astrocytoma", "Steroids cause muscle weakness and can change mood"),
                     ("tumors", "low-grade-glioma", "levetiracetam is linked with irritability and low mood"),
                     ("tumors", "cns-lymphoma", "teams often want a biopsy before starting steroids"),
                     ("tests", "biopsy", "You may be given steroid medicine before or after"),
                     ("tests", "getting-ready-for-surgery", "this is not a medicine to stop on your own"),
                     ("tests", "follow-up-scans", "Steroids often settle it"),
                 })
        {
            var text = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(folder, slug + ".md")));
            Assert.Contains("/treatments/steroids", text, StringComparison.Ordinal);
            Assert.Contains(kept, text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheSiblingClaimsThisPageCorrectedStayCorrected()
    {
        // Two defects found while writing the canonical page (§12.8, WI-521:
        // budget an edit to the siblings).
        //
        // (1) /treatments/radiation-therapy said "Somebody who SUDDENLY cannot
        // get out of a low chair may be having a drug effect". Steroid weakness
        // builds over weeks in every source on this page, and the corpus files
        // sudden weakness as same-day or as an ambulance. The reassurance was
        // attached to the one presentation where it is wrong.
        var radiation = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "radiation-therapy.md")));

        // The whole section, not the one sentence: /review reinstated the claim
        // in a NEW sentence next to the corrected one ("Weakness can come on
        // suddenly."), which a check selected on "low chair" cannot see
        // (§12.8, WI-520: selecting on a word the sentence next door shares).
        var drugSection = CuratedPage.Flatten(Reader(Regex.Match(
            CuratedPage.Read("treatments", "radiation-therapy.md"),
            @"^### Is this the tumor, or is it the medicine\?.*?(?=^### )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value));
        Assert.False(string.IsNullOrEmpty(drugSection),
            "/treatments/radiation-therapy no longer has the 'is it the medicine' section");
        // "Sudden" survives in exactly one place in that section: the rule
        // against stopping a steroid suddenly. Anywhere else it is back
        // attached to the weakness, which is the defect this item fixed.
        foreach (var sentence in CuratedPage.SentencesOf(drugSection)
                     .Where(s => s.Contains("sudden", StringComparison.OrdinalIgnoreCase)))
        {
            Assert.Contains("stop a steroid suddenly", sentence, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Matches(new Regex(@"builds up over weeks", RegexOptions.IgnoreCase), radiation);

        // (2) /tumors/glioblastoma said "If you feel unlike yourself on them,
        // THAT IS the drug" — a verdict, on the frame whose whole point is
        // that it MIGHT be.
        var gbm = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "glioblastoma.md")));
        Assert.DoesNotContain("that is the drug", gbm, StringComparison.OrdinalIgnoreCase);
        Assert.Matches(new Regex(@"may well be the\s+drug rather than the tumor", RegexOptions.IgnoreCase), gbm);
    }

    private static List<string> Headings() =>
        Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

    /// <summary>Function words, which an eight-word run can be made of by coincidence.</summary>
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

    /// <summary>Word shingles, headings and link targets removed first (§12.8, WI-521).</summary>
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

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class SteroidsPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/steroids";

    private readonly WebApplicationFactory<Program> _factory;

    public SteroidsPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Steroids for a brain tumor", html);
        Assert.Contains("Do not stop a steroid on your own", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // The pages a reader is standing on when they have just been handed a
        // steroid or told that one is coming.
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/treatments/craniotomy", "/treatments/radiation-therapy", "/tumors/glioblastoma",
                     "/tumors/high-grade-glioma", "/tumors/astrocytoma", "/tumors/low-grade-glioma",
                     "/tumors/cns-lymphoma", "/tests/biopsy", "/tests/getting-ready-for-surgery",
                     "/tests/follow-up-scans",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            // The doors out of "Where to go next" (the helper reads that
            // section, so the in-body links to /treatments/chemotherapy#fever-rule,
            // /tumors/cns-lymphoma and /seizures/what-to-do are checked by the
            // whole-article sweep above rather than named here).
            "/treatments/craniotomy", "/treatments/radiation-therapy",
            "/tests/follow-up-scans", "/tests/getting-ready-for-surgery",
            "/tumors", "/get-help-now");

    [Fact]
    public async Task TheDeepLinksIntoTheSiblingPagesLandOnRealAnchors() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page. This page
        // deep-links the craniotomy drug section and the chemotherapy fever
        // rule, and the first of those anchors was added by this item.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheTermThisPageDefinesIsSuppressedHereAndStillFiresElsewhere()
    {
        // §12.8 (WI-509): this page defines dexamethasone, so a popover
        // repeating the paragraph beneath it is noise. WI-512's other half
        // first: the word is in the prose, so the suppression is not a no-op.
        var body = CuratedPage.ReaderText(CuratedPage.Read("treatments", "steroids.md"));
        Assert.Contains("dexamethasone", body, StringComparison.OrdinalIgnoreCase);

        var client = _factory.CreateClient();
        Assert.DoesNotContain("def-dexamethasone", await client.GetStringAsync(Url));

        // And the entry is still reachable, so suppressing it here does not
        // switch off its only live use (§12.8, WI-519).
        Assert.Contains("def-dexamethasone", await client.GetStringAsync("/treatments/radiation-therapy"));
    }

    [Fact]
    public async Task TheTooltipsForWordsThisPageDoesNotDefineStillFire()
    {
        // The other direction, which is the one WI-510 got wrong: a word this
        // page uses and does NOT define keeps its tooltip here.
        Assert.Contains("def-contrast-dye", await _factory.CreateClient().GetStringAsync(Url));
    }
}
