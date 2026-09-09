using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-520: T7, the checks before a brain operation. The ninth page under the
/// §12.8 LIBRARY template (the shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs).
///
/// The prose is reviewed, not tested. What is pinned here is the small set of
/// places where this page could leave a reader worse off than no page at all.
/// This page is unusual in the corpus because it publishes two things a reader
/// can act on tonight — a fasting interval and a rule about blood thinners —
/// and both of them fail in a direction that hurts somebody:
///
///   * the fasting figure read as permission to override the hospital's sheet;
///   * the blood-thinner rule narrowed, softened, or turned into a day count a
///     reader can apply to themselves.
///
/// Most of what follows exists to hold those two open.
/// </summary>
public sealed class GettingReadyForSurgeryPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "getting-ready-for-surgery.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section as the reader meets it. This exists because
    /// <c>CuratedPage.ReaderText</c> must NOT be fed a section:
    /// it strips front matter by index (<c>IndexOf("\n---")</c>), and on a
    /// string that has none the index is -1, so it silently returns
    /// <c>section[3..]</c> — three characters off the front, quietly breaking
    /// any assertion about how a section OPENS. That cost this item a round:
    /// the fasting position test could not see its own first sentence.
    /// <c>Section</c> already flattens, so nothing else is needed.
    /// </summary>
    private static string Reader(string section) => Regex.Replace(section, @"!%(.+?)%", "");

    /// <summary>
    /// A number, however this corpus happens to have written it. Every guard
    /// below that is about a QUANTITY uses this rather than <c>\d</c>: §12.8
    /// (WI-511) records a page whose no-percentages test matched digits on a
    /// corpus that writes every number in words, so the rule could not see the
    /// number it was written for.
    /// </summary>
    private const string NumberWord =
        @"(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"twenty|thirty|forty|fifty|sixty|a|an|half|first)";

    /// <summary>
    /// "before the operation", in the shapes English actually offers. The first
    /// version of the day-count guard required literally <c>before|prior</c>,
    /// and <c>/review</c> published a stop-day count three separate times by
    /// writing "ahead", "ahead of the operation" and "beforehand" instead.
    /// </summary>
    private const string BeforeWord =
        @"(?:before|prior|ahead|beforehand|in advance|earlier|previously|up front|out from)";

    [Fact]
    public void TheShortVersionCarriesBothRulesTheReaderCanActOnToday()
    {
        // §12.3's reason, applied to a library page: readers consume 20 to 28%
        // of a page. The two facts on this page that change what somebody does
        // tonight cannot sit four screens down where the reader who stops after
        // the summary never meets them.
        var shortVersion = Section("The short version");

        Assert.Contains("go all night without a drink of water", shortVersion);
        Assert.Contains("must not stop a blood thinner on your own", shortVersion);

        // AND THE FASTING SENTENCE CARRIES ITS CAVEAT HERE TOO. The first draft
        // put the permission in the summary with the deference to the reader's
        // own hospital four screens down in the fasting section — and pinned
        // the uncaveated sentence with this very test, so the suite held the
        // defect open. The section-scoped ordering test structurally could not
        // see the summary. For the documented majority who read only this far,
        // the summary IS the page.
        var caveat = shortVersion.IndexOf("those are the ones to follow", StringComparison.Ordinal);
        var permission = shortVersion.IndexOf("go all night without a drink of water", StringComparison.Ordinal);
        Assert.True(caveat >= 0,
            "the short version now offers the fasting permission with no deference to the reader's "
            + "own hospital, to the readers most likely to stop here");
        Assert.True(caveat < permission,
            "the fasting permission now precedes its caveat in the short version");
    }

    [Fact]
    public void TheTwoHourRuleCarriesTheScopeItsSourceAttachesToIt()
    {
        // PMC13436883, verbatim: clear liquids may be consumed up to 2 h before
        // anesthetic induction "FOR ELECTIVE PROCEDURES", and the ASA document
        // it summarises is titled "...Application to HEALTHY PATIENTS undergoing
        // elective procedures". Most of that paper is about the people the rule
        // does NOT hold for — GLP-1 receptor agonists ("may delay gastric
        // emptying ... despite adherence to guideline-recommended fasting
        // intervals"), gastroparesis, pregnancy in labor.
        //
        // The front matter quoted the qualifier and the prose printed the rule
        // without it, which is §12.6 ("keep the qualifier, shorten it") and the
        // §12.14 shape: the record right, the sentence wider than the record.
        //
        // It matters more here than it usually would, because the section then
        // arms the reader to push back on a longer instruction. For somebody on
        // a weight-loss injection, the longer time may be the rule FOR THEM, and
        // without this paragraph the page has told them it is probably
        // institutional habit.
        var section = Reader(Section("When do I stop eating and drinking?"));

        Assert.Contains("written for a planned operation, in people whose stomachs empty normally",
            section, StringComparison.Ordinal);
        Assert.Contains("your own time may genuinely be longer", section, StringComparison.Ordinal);

        // And the scope arrives BEFORE the sentence that invites the pushback,
        // or the reader meets the invitation without the exception.
        var scope = section.IndexOf("stomachs empty normally", StringComparison.Ordinal);
        var pushback = section.IndexOf("how late can I drink water", StringComparison.Ordinal);
        Assert.True(pushback > 0, "the actionable question has gone");
        Assert.True(scope < pushback,
            "the reader is now invited to question a longer fasting time before being told that a "
            + "longer time may be the right one for them");

        // Clear drinks are defined by what they exclude. "Drinks you can see
        // through with no milk in them" describes gin, vodka and white wine.
        Assert.Contains("no alcohol", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBloodThinnerRuleIsNotNarrowedAndForbidsActingOnThisPage()
    {
        // §12.8's WI-519 lesson, on the page where it costs the most.
        // CRUK says "You must not drive after having a brain biopsy"; the biopsy
        // draft wrote "you must not drive YOURSELF" and quietly scoped a blanket
        // restriction to the safer-sounding case. The same shape here would be
        // "do not stop it on the day", "do not stop it without asking first", or
        // any wording that leaves a reader room to decide they have asked
        // enough.
        //
        // The prohibition also has to name THIS PAGE, because a page that
        // publishes "for some of them it is around a week" three sentences later
        // is itself the most likely thing the reader acts on.
        var section = Section("Blood thinners: do not stop one on your own");
        var flat = Reader(section);

        Assert.Contains("not yours to make alone", flat);
        Assert.Contains("including this page", flat);

        // THE FIXED BLOCKLIST OF FIVE PHRASES COULD NOT FAIL ON ITS OWN DEFECT.
        // /review scoped the rule to a window nobody had thought of — "In the
        // last week before the operation, do not stop one on your own." — and
        // the suite stayed green. A narrowing is not a phrase; it is a
        // CONDITION attached to an unconditional rule, so what is checked is
        // that no sentence carrying the prohibition also carries a scope.
        foreach (var narrowing in new[]
                 {
                     "stop it yourself", "stop one yourself", "on the day of",
                     "the night before your operation, do not", "without asking first",
                 })
        {
            Assert.DoesNotContain(narrowing, flat, StringComparison.OrdinalIgnoreCase);
        }

        var scope = new Regex(
            @"\b(in the (last|final)|during the|within|until|after|once|as long as|so long as|unless|"
            + @"provided|if you have|if they have|on the day|the week before|the night before|"
            + @"that is enough|that counts)\b", RegexOptions.IgnoreCase);

        foreach (var sentence in CuratedPage.SentencesOf(flat)
                     .Where(s => Regex.IsMatch(s, @"\b(do not|must not|never|not yours)\b",
                         RegexOptions.IgnoreCase)
                         && Regex.IsMatch(s, @"\bstop|\bdecision\b", RegexOptions.IgnoreCase)))
        {
            Assert.False(scope.IsMatch(sentence),
                "the blanket prohibition now carries a scope, which is the WI-519 narrowing shape "
                + $"(\"you must not drive\" -> \"you must not drive yourself\"): {sentence}");
        }

        // The heading is the rule. If it is reworded into a topic ("Blood
        // thinners") the reader scanning headings loses the only instruction on
        // the page that is phrased as one.
        Assert.Contains("## Blood thinners: do not stop one on your own", Page, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBloodThinnerRuleReachesThePersonWhoCanEnforceIt()
    {
        // The other half of WI-519's finding. That page's driving prohibition
        // was filed under a packing list and never reached the caregiver
        // section, which is where /treatments/craniotomy deliberately puts
        // driving — because the person who can hold somebody to a rule is the
        // person standing next to them, not the person who wants to bend it.
        //
        // Pinned to the caregiver SECTION, not to the page. Presence somewhere
        // was never the property (WI-512).
        var caregiver = Reader(Section("For the person going with them"));

        Assert.Contains("blood thinner", caregiver, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not let anybody stop one on the strength of something they read",
            caregiver, StringComparison.Ordinal);
        Assert.Contains("That includes this page", caregiver, StringComparison.Ordinal);

        // AND THE PROHIBITION HAS TO BE THE LAST THING IN ITS PARAGRAPH.
        // Presence was never the property (WI-512): /review appended one
        // sentence — "If they have already spoken to a nurse about it, that is
        // enough." — directly after it, and all three assertions above still
        // passed while the paragraph now ended by licensing the thing it had
        // just forbidden. §12.8's WI-510 lesson: a positional property is
        // pinned to the LAST sentence, not to a window.
        // Extracted from the RAW markdown, where a paragraph is still delimited
        // by a blank line. On the flattened text the obvious "up to the next
        // bold lead-in" regex stops at the paragraph's OWN mid-paragraph bold
        // ("**Do not let anybody...**") and pins the wrong sentence — which is
        // how the first attempt at this check reported the wrong last sentence
        // rather than the softener it was written to catch.
        var paragraph = Regex.Match(Page,
            @"^\*\*The blood thinner, if there is one\.\*\*(.*?)(?=\r?\n[ \t]*\r?\n)",
            RegexOptions.Singleline | RegexOptions.Multiline);
        Assert.True(paragraph.Success, "the caregiver section no longer has a blood-thinner paragraph");

        // Emphasis is stripped BEFORE splitting. The sentence splitter needs
        // whitespace after the period, and this paragraph's prohibition ends
        // `...they read.**` — so with the asterisks left in, the final two
        // sentences arrive glued together as one and "the last sentence" is
        // whatever the pair happens to start with. Same trap WI-514 recorded
        // for `**first ever** seizure`.
        var plain = CuratedPage.Flatten(paragraph.Groups[1].Value).Replace("**", "", StringComparison.Ordinal);
        var sentences = CuratedPage.SentencesOf(plain.Trim());
        Assert.Equal("That includes this page.", sentences[^1].Trim());
    }

    [Fact]
    public void NoPerDrugStopDayCountIsPublishedAnywhereOnThePage()
    {
        // A deliberate omission, recorded here so no later session "completes"
        // the section by pasting the table back in.
        //
        // The dossier (tests-library.md §6.5) carries prasugrel 7 days,
        // clopidogrel 5, ticagrelor 3, aspirin 4 to 5, and the source is real.
        // The reason it stays off the page is §12.4 R2's reasoning pointed at a
        // different risk: a per-drug day count next to a heading that says do
        // not stop one on your own is an instruction a frightened reader CAN
        // follow, and following it is the harm. The page keeps one number, "for
        // some of them it is around a week", and it only ever pushes in the
        // safe direction — ask sooner.
        //
        // Aspirin is named, because whether it stops depends on why somebody is
        // on it and that is the whole point of the paragraph. It is named
        // WITHOUT a day count for the same reason as the rest.
        // THE FIRST VERSION OF THIS TEST COULD NOT FAIL ON ITS OWN DEFECT, THREE
        // WAYS, and /review proved all three by mutation while the suite stayed
        // green at 22/22:
        //
        //   "Anybody taking it for other reasons is usually told to stop aspirin
        //    about five days ahead of the operation."   -> `ahead`, not `before`
        //   "Rivaroxaban, for instance, is usually stopped three days ahead."
        //                                               -> not on the drug list
        //   "- I stop my aspirin five days ahead, is that still right?"
        //                                               -> in the questions list
        //
        // So the guard is no longer a list of five drug names and one
        // preposition. It is deny-by-default on the SHAPE: nothing on this page
        // may pair a quantity of days with a word meaning "before the
        // operation", whoever the sentence is about.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        var dayCount = new Regex(
            $@"\b{NumberWord}(\s+(to|or)\s+{NumberWord})?\s+days?\b[^.?!]{{0,40}}\b{BeforeWord}\b",
            RegexOptions.IgnoreCase);
        Assert.False(dayCount.IsMatch(body),
            "this page now publishes a number of days to stop a medicine before the operation: "
            + dayCount.Match(body).Value);

        // The named drugs stay banned as well, because a drug name is what makes
        // a count look like it is about the reader. Aspirin is the one
        // exception and is named deliberately — whether it stops depends on why
        // somebody is on it, and that IS the paragraph — so it gets its own
        // check below rather than an entry here.
        foreach (var drug in new[]
                 {
                     "clopidogrel", "prasugrel", "ticagrelor", "warfarin", "apixaban",
                     "rivaroxaban", "edoxaban", "dabigatran", "heparin", "enoxaparin",
                     "plavix", "xarelto", "coumadin", "eliquis", "brilinta", "effient",
                 })
        {
            Assert.DoesNotContain(drug, body, StringComparison.OrdinalIgnoreCase);
        }

        // Aspirin may be named; it may not be given a stopping instruction.
        foreach (var sentence in CuratedPage.SentencesOf(body)
                     .Where(s => s.Contains("aspirin", StringComparison.OrdinalIgnoreCase)))
        {
            Assert.DoesNotMatch(new Regex(@"\bstop\w*\s+(taking\s+)?(the\s+|your\s+)?aspirin",
                RegexOptions.IgnoreCase), sentence);
        }
    }

    [Fact]
    public void TheFastingSectionPutsTheHospitalsOwnTimesBeforeAnyNumberItPrints()
    {
        // A POSITION property, not a presence one — WI-512's defect, which
        // passed three tests that only asserted the urgency words were on the
        // page while the fever sat in the wrong list.
        //
        // A reader who meets "clear drinks can go up to two hours before" and
        // only afterwards meets "follow your hospital's times" has already
        // decided what to do. The order is the safety mechanism, so the order is
        // what is pinned.
        // THE FIRST VERSION OF THIS TEST COULD NOT FAIL ON ITS OWN DEFECT.
        // It compared the caveat's position against ONE exact phrase ("two hours
        // before you are put to sleep") while calling itself "before any number
        // it prints". /review inserted a REWORDED permission — "Clear drinks are
        // usually allowed up to two hours before surgery." — as the section's
        // first sentence, ahead of the caveat, and the suite stayed green.
        //
        // It now finds the FIRST hour-figure of any wording and requires the
        // caveat in front of it.
        var section = Reader(Section("When do I stop eating and drinking?"));

        var hospitalFirst = section.IndexOf("Your hospital gives you your own times", StringComparison.Ordinal);
        Assert.True(hospitalFirst >= 0, "the section no longer defers to the reader's own hospital");

        var hours = new Regex($@"\b{NumberWord}\s+hours?\b", RegexOptions.IgnoreCase);
        var firstHourFigure = hours.Match(section);
        Assert.True(firstHourFigure.Success, "the section no longer carries any fasting interval at all");
        Assert.True(section.Contains("two hours before you are put to sleep", StringComparison.Ordinal),
            "the section no longer carries the two-hour clear-fluid rule in the form the sibling "
            + "pages and the short version were written against");

        Assert.True(hospitalFirst < firstHourFigure.Index,
            "a fasting figure now appears BEFORE the sentence deferring to the reader's own "
            + $"hospital, so a reader can act on the number without having met the caveat: "
            + $"'{firstHourFigure.Value}' at {firstHourFigure.Index}, caveat at {hospitalFirst}");

        // And it closes on the action, not on the figure (§12.6). Pinned to the
        // LAST sentence rather than a window, per §12.8's WI-510 lesson.
        var sentences = CuratedPage.SentencesOf(Reader(Section("When do I stop eating and drinking?")));
        Assert.Contains("follow it exactly", sentences[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void TheEuropeanFastingFigureIsAttributedInTheSentenceThatPrintsIt()
    {
        // §12.8's WI-507 rule. R1 permits the number; nothing else stops a
        // reader in Ohio treating a European median as a promise about their own
        // hospital. The attribution has to be in the same SENTENCE, not in the
        // source list and not in the paragraph above.
        var section = Reader(Section("When do I stop eating and drinking?"));

        // THE FIRST VERSION OF THIS TEST COULD NOT FAIL ON ITS OWN DEFECT. It
        // selected the sentence containing the bare word "twelve" — which the
        // ATTRIBUTION also contains, in "twelve European countries". /review
        // split the two apart:
        //
        //   "One study looked at forty-six centers in twelve European countries.
        //    The middle patient there had gone twelve hours without a clear
        //    drink..."
        //
        // and the test picked up the first sentence, found "European" and
        // "centers" in it, and passed — with the figure sitting unattributed in
        // the next sentence, which is the entire defect.
        //
        // It now selects on the FIGURE ("twelve hours"), not on a word that
        // happens to appear in both.
        var sentence = CuratedPage.SentencesOf(section)
            .FirstOrDefault(s => Regex.IsMatch(s, @"\btwelve\s+hours\b", RegexOptions.IgnoreCase));

        Assert.False(sentence is null,
            "the twelve-hour figure has gone, or is no longer in a sentence of its own");
        Assert.Contains("European", sentence!, StringComparison.Ordinal);
        Assert.Contains("centers", sentence!, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFattyMealFigureIsNotPublishedAndNeitherIsAnyOtherUnsourcedFastingTier()
    {
        // The other deliberate omission, recorded for the same reason as the
        // day counts. The dossier's §6.4 gives eight hours after a full meal
        // high in calories or fat — and its citation
        // (guidelinecentral.com/guideline/8964/) contains NO fasting guidance of
        // any kind, so that figure arrived here with nothing behind it. No
        // source reachable for this page states it for an adult population.
        //
        // The light-meal versus fatty-meal distinction is exactly what a
        // hospital's own instruction sheet exists to settle, and a reader
        // deciding which of two tiers their dinner was in is a reader the page
        // has made worse off.
        // The word-only version of this regex was defeated by "8 hours if the
        // meal was a big one" — digits, on the one guard in the file that
        // forgot them, in a corpus whose OTHER rule is that numbers are written
        // as words. Both forms now.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        Assert.DoesNotMatch(new Regex(@"\b(eight|8)\s+hours?\b", RegexOptions.IgnoreCase), body);
        foreach (var phrase in new[]
                 {
                     "fatty meal", "fatty food", "fried food", "light meal", "heavy meal",
                     "big meal", "large meal", "full meal",
                 })
        {
            Assert.DoesNotContain(phrase, body, StringComparison.OrdinalIgnoreCase);
        }

        // The only hour-figures on the page are the ones the sources carry: the
        // two-hour clear-fluid rule, the six-hour food figure, the European
        // twelve-hour median, and the first hour of gastric emptying. Anything
        // else is a tier somebody has invented.
        var hourFigures = Regex.Matches(body, $@"\b{NumberWord}\s+hours?\b", RegexOptions.IgnoreCase)
            .Select(m => Regex.Replace(m.Value.ToLowerInvariant(), @"\s+", " "))
            .Distinct()
            .Order()
            .ToList();
        Assert.Equal(["first hour", "six hours", "twelve hours", "two hours"], hourFigures);
    }

    [Fact]
    public void TheHeartTraceIsNotPresentedAsRoutineAndAgeIsNotGivenAsAReasonForOne()
    {
        // The dossier is wrong here and its own co-citation is what proves it.
        // tests-library.md §6.2 says an ECG "is usually indicated with a cardiac
        // history or above roughly age 50-60"; the ASA advisory it cites says
        // "Age alone may not be an indication for ECG."
        //
        // This is the over-promising direction §12.12 records as the more
        // dangerous one — it tells a healthy fifty-five-year-old that a heart
        // trace is part of the package, and then the appointment does not
        // include one and the page has manufactured a worry.
        //
        // The source's hedge is kept and shortened rather than cut (§12.6):
        // "may not be", not "is not".
        var why = Reader(Section("Why am I having all this?"));

        Assert.Contains("There is no fixed list of tests", why, StringComparison.Ordinal);
        Assert.Contains("Your age on its own may not be a reason for a heart trace",
            why, StringComparison.Ordinal);

        // THE FIRST VERSION OF THIS TEST COULD NOT FAIL ON THE DOSSIER'S OWN
        // DEFECT. Its regex required `(over|above|older than)` immediately
        // followed by a number, and the dossier's phrasing is "above roughly age
        // 50-60" — an adverb and the word "age" in between. /review pasted that
        // phrasing straight in ("One is usual with a cardiac history or above
        // roughly age fifty to sixty.") and the suite stayed green, with the
        // page then contradicting itself two sections apart while both positive
        // assertions above still held.
        //
        // It is now two independent checks: no age figure anywhere near a word
        // meaning "older", and no sentence about a heart trace that also frames
        // it as usual or routine.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        Assert.DoesNotMatch(new Regex(
            @"\b(over|above|older than|from|beyond|past)\b[^.?!]{0,20}\b(age|aged|years old)?[^.?!]{0,10}"
            + @"\b(forty|fifty|sixty|seventy|\d{2})\b",
            RegexOptions.IgnoreCase), body);
        Assert.DoesNotMatch(new Regex(@"\bage\b[^.?!]{0,20}\b(forty|fifty|sixty|seventy|\d{2})\b",
            RegexOptions.IgnoreCase), body);

        // Scoped to the whole numbered STEP, not to the sentences that happen
        // to name the test. The break harness proved why: replacing the step's
        // closing sentence with "Everybody gets one as a matter of routine."
        // names no test at all — "one" refers back to the previous sentence —
        // so a per-sentence check walked straight past it. The routine framing
        // does not have to live in the sentence carrying the noun.
        var heartTraceStep = Regex.Match(Page,
            @"^\d+\. \*\*A heart trace.*?(?=^\d+\. )", RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.True(heartTraceStep.Success, "the walkthrough no longer has a heart-trace step");

        Assert.DoesNotMatch(new Regex(
            @"\b(usual|usually|routine|routinely|standard|always|everybody|everyone)\b",
            RegexOptions.IgnoreCase), CuratedPage.Flatten(heartTraceStep.Value));

        // And the same for any OTHER sentence on the page that names the test,
        // so the framing cannot simply move out of the step.
        foreach (var sentence in CuratedPage.SentencesOf(body)
                     .Where(s => Regex.IsMatch(s, @"heart trace|electrocardiogram|\bECG\b|\bEKG\b",
                         RegexOptions.IgnoreCase)))
        {
            Assert.DoesNotMatch(new Regex(
                @"\b(usual|usually|routine|routinely|standard|always|everybody|everyone)\b",
                RegexOptions.IgnoreCase), sentence);
        }

        // And the two conditional tests are written as conditional in the
        // walkthrough, where a numbered list otherwise reads as a running order
        // every reader gets.
        var steps = Section("What happens, step by step");
        Assert.Contains("**A heart trace, if there is a reason.**", steps, StringComparison.Ordinal);
        Assert.Contains("**A chest x-ray, if there is a reason.**", steps, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDeadAndMisreadCitationsAreAbsentAndSoAreTheClaimsTheyCarried()
    {
        // §12.14, which is the whole reason this test has a second half.
        // Banning a URL reads like the claim was handled. WI-518 banned three
        // and shipped two of their claims in prose anyway.
        var front = CuratedPage.FrontMatter(Page);
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        // The ban is on the CITED urls, not on the front matter as a string.
        // The `#` comment above the list names both dead URLs on purpose —
        // §12.14 says a source that failed has to be recorded where the next
        // reader of the front matter will see it, and a whole-front-matter
        // DoesNotContain makes writing that note impossible.
        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();

        // 403 on every attempt, cited twice in the dossier (§6.2, §6.3).
        Assert.DoesNotContain(urls, u => u.Contains("accessanesthesiology.mhmedical.com",
            StringComparison.OrdinalIgnoreCase));

        // The MRI page, cited by §6.5 for a claim about surgery. Note the ban is
        // on the PAGE and not the domain — §12.13's shape. radiologyinfo's chest
        // x-ray page is cited here and is right to be.
        Assert.DoesNotContain(urls, u => u.Contains("radiologyinfo.org/en/info/mri-brain",
            StringComparison.OrdinalIgnoreCase));
        Assert.Contains(urls, u => u.Contains("radiologyinfo.org/en/info/chestrad",
            StringComparison.OrdinalIgnoreCase));

        // Now the claims. This is the half that WI-518 skipped.
        //
        // (1) The MRI page was carrying "dexamethasone and anti-seizure
        //     medicines are generally CONTINUED". It says nothing of the kind
        //     about either. The anti-seizure half is re-sourced; the steroid
        //     half is NOT the same claim and is not written as one, so the
        //     "continued" framing must not appear for the steroid.
        //     THE FIRST VERSION OF THIS CHECK WAS FOUR LITERAL STRINGS AND
        //     COULD NOT FAIL ON ITS OWN DEFECT. /review wrote "Your steroid is
        //     generally continued right through the operation." — singular
        //     where the ban was plural, no "will be" — and the suite stayed
        //     green at 22/22. That is §12.14 one level up, on the item whose
        //     test is named for §12.14, which is exactly the trap that section
        //     says a named ban sets. It bans the SHAPE now.
        Assert.DoesNotMatch(new Regex(
            @"\bsteroids?\b[^.?!]{0,60}\b(continued|continues|kept going|carried on|carries on|carry on|"
            + @"stays? on|keep taking|keeps? taking)\b", RegexOptions.IgnoreCase), body);
        //     Asserted per sentence, with questions skipped. "Do I keep taking
        //     my seizure medicine and my steroid?" is the reader ASKING, which
        //     is the whole point of the questions list — a ban that fires on it
        //     is a rule that fails a correct page (§12.8, WI-511).
        foreach (var sentence in CuratedPage.SentencesOf(body).Where(s => !s.TrimEnd().EndsWith('?')))
        {
            Assert.DoesNotMatch(new Regex(
                @"\b(continue|continued|keep taking|carry on with)\b[^.?!]{0,40}\b(steroids?|dexamethasone)\b",
                RegexOptions.IgnoreCase), sentence);
        }

        // (2) The dossier's uncited "neurosurgery-specific additions in
        //     practice" paragraph. None of it is asserted; "Which blood tests am
        //     I having?" is in the questions list instead.
        foreach (var claim in new[] { "group and save", "type and screen", "coagulation studies",
                     "clotting studies", "visual fields" })
        {
            Assert.DoesNotContain(claim, body, StringComparison.OrdinalIgnoreCase);
        }

        // (3) The restart timings, which are about after the operation and
        //     belong to the surgical team.
        //
        //     The first version banned `restart(ed|ing)? (it|them|your)`, which
        //     the page's own seizure paragraph already evades in the innocent
        //     direction ("get back onto it as soon as possible afterwards") —
        //     so it was provably not a ban on the claim, and /review published
        //     "you normally start it again within a day of the operation"
        //     straight through it. What makes a restart claim a TIMING claim is
        //     the duration, so that is what is banned.
        Assert.DoesNotMatch(new Regex(
            $@"\b(re)?start\w*\b[^.?!]{{0,40}}\b(within|after|by)\b[^.?!]{{0,15}}\b{NumberWord}\s*"
            + @"(day|days|hour|hours|week|weeks)\b", RegexOptions.IgnoreCase), body);
        Assert.DoesNotMatch(new Regex(
            $@"\b{NumberWord}\s*(day|days|hour|hours)\b[^.?!]{{0,30}}\b(re)?start", RegexOptions.IgnoreCase),
            body);
    }

    [Fact]
    public void TheSteroidParagraphIsAboutStoppingRatherThanAboutContinuing()
    {
        // Following on from the citation above: what the page DOES say has to be
        // inside what a real source says. PMC11451960 is about the taper —
        // "abrupt discontinuation predisposes a patient to development of
        // cortisol insufficiency and even adrenal crisis". That supports a rule
        // about not stopping. It does not support a promise about what the
        // surgical team will do with the dose, which is the claim the dead
        // citation was making.
        var section = Reader(Section("Which of my medicines do I keep taking?"));

        // And the section does not open by asserting the claim the dead citation
        // was carrying. The first draft opened "Most of what you take, you keep
        // taking" — which is the radiologyinfo MRI page's "take your regular
        // medications as usual", banned three tests above and then written out
        // again in my own words. §12.14 exactly, on the item that wrote the ban.
        // The section now asserts nothing about the split and tells the reader
        // not to assume one.
        // The literal-string version of this was defeated by one auxiliary verb
        // — /review restored "Most of what you take, you WILL keep taking." and
        // the suite stayed green. Banned as a shape: no quantifier over the
        // reader's medicines paired with a claim that they carry on.
        Assert.Contains("do not assume either way", section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(
            @"\b(most|much|all|the rest|nearly all|almost all)\b[^.?!]{0,40}"
            + @"\b(keep taking|keeps taking|carry on|carries on|continue|continues|stay on|stays on|"
            + @"as usual|as normal)\b", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(new Regex(
            @"\b(take|taking)\b[^.?!]{0,30}\b(regular|usual|normal)\b[^.?!]{0,20}"
            + @"\bmedicines?\b[^.?!]{0,20}\b(as usual|as normal)\b", RegexOptions.IgnoreCase), section);

        Assert.Contains("not a medicine to stop on your own", section, StringComparison.Ordinal);
        Assert.Contains("brought down in steps rather than", section, StringComparison.Ordinal);
        Assert.Contains("Your team tells you what to do with it", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSeizureMedicineParagraphKeepsItsSourcesHedge()
    {
        // PMC5470849: antiepileptics "maintained ... as close to baseline as
        // possible", morning dose "with a sip of water" — and, in the same
        // review, that an individualised approach may be needed in certain
        // situations. A page that prints the sip-of-water instruction as
        // somebody's plan without the hedge has turned a general rule into
        // this reader's instruction. §12.6: keep the qualifier, shorten it.
        var section = Reader(Section("Which of my medicines do I keep taking?"));

        Assert.Contains("as close to normal as it can be", section, StringComparison.Ordinal);
        Assert.Contains("with a sip of water", section, StringComparison.Ordinal);
        Assert.Contains("Ask whether that is your plan", section, StringComparison.Ordinal);
        Assert.Contains("there are reasons a team sometimes wants it done differently",
            section, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePreparationListIsNotARestatementOfTheOperationPagesOwnList()
    {
        // This page exists because /treatments/craniotomy's slot 7 is a
        // five-bullet summary of a subject that needs a page. That makes
        // restatement the default failure, not an unlikely one — and the first
        // draft of this page did it, lifting "Your questions, written down. You
        // will not remember them otherwise." verbatim.
        //
        // Word shingles over the composed text, the shape §12.8 records as the
        // working one (a heading or lead-in check is not enough). "Where to go
        // next" is excluded on both sides: it is an index of onward doors by
        // design.
        var mine = Trimmed(CuratedPage.ReaderText(Page));
        var sibling = CuratedPage.Section(
            CuratedPage.Read("treatments", "craniotomy.md"), "What you need first, and what to bring");

        // `Section()` already flattens and the sibling carries no `!%…%`
        // markers, so it is passed straight in. Wrapping it in `ReaderText`
        // — which this file's own `Reader` docstring says never to do to a
        // section — silently dropped the sibling's first three characters and
        // with them its first shingle. /review caught it thirty lines from the
        // comment warning about it.
        var mineShingles = Shingles(mine, 8).ToHashSet();
        var overlaps = Shingles(sibling, 8)
            .Where(mineShingles.Contains)
            .Distinct()
            .ToList();

        Assert.True(overlaps.Count == 0,
            "this page restates /treatments/craniotomy's own preparation list rather than expanding "
            + "it, so the reader meets the same sentence twice:\n  " + string.Join("\n  ", overlaps));

        static string Trimmed(string body)
        {
            var next = body.IndexOf("## Where to go next", StringComparison.Ordinal);
            return next > 0 ? body[..next] : body;
        }
    }

    [Fact]
    public void ThePageCarriesNoEscalationListAndRoutesInstead()
    {
        // A deliberate absence, recorded because the reflex on a surgery page is
        // to add one. §12.8 (WI-511, WI-512) makes every escalation list on the
        // site a liability that has to be diffed against every other one, per
        // symptom, in both directions — and this page has no symptoms to sort.
        // Its reader has not had the operation yet. The one escalation-shaped
        // thing that can happen before the day is becoming unwell, and no source
        // fetched for this page supports a triage rule for it, so the caregiver
        // section asks the question rather than answering it (§12.8, WI-519).
        //
        // If a later item adds a list here, this test fails, and the tier diff
        // becomes that item's job rather than something nobody noticed.
        //
        // Read on the COMPOSED page, and the first version of this test was not.
        // It ran on the raw markdown, where the caregiver section is the five
        // characters "[CAREGIVER]" — so it proved the absence of a phrase from a
        // page the reader never sees, while the composed page says "what counts
        // as 'call an ambulance' for this tumor" in the shared block. The end-to-
        // end read caught it; eighteen green tests did not. §12.14's shape, on
        // the item that wrote a test named for §12.14.
        //
        // The block's line is correct here and stays. What is asserted is that
        // THIS PAGE's own prose adds no list of its own, so the block text is
        // subtracted rather than the check being weakened to match it.
        var composed = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Composed(Page)));
        var blockText = CuratedPage.Flatten(
            CuratedPage.Body(File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md"))));

        var ownProse = composed.Replace(blockText, " ", StringComparison.Ordinal);
        Assert.NotEqual(composed, ownProse); // the block really was composed in

        foreach (var phrase in new[]
                 {
                     "call an ambulance", "these cannot wait", "the same day if", "straight away",
                     "right away", "as soon as possible if", "urgently", "emergency room", "911",
                 })
        {
            Assert.DoesNotContain(phrase, ownProse, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE STRING LIST IS NOT THE CHECK. /review headed a brand-new,
        // undiffed escalation list "**Phone the hospital now if any of these
        // happen:**" and the suite stayed green at 22/22, because the four
        // banned strings are four ways of writing a heading and there are
        // dozens. What makes it an escalation list is the SHAPE: an
        // urgency-flavoured lead-in that introduces a run of symptom bullets.
        // Checked on the raw markdown, where the bullets are still bullets.
        var ownMarkdown = Page.Replace("[CAREGIVER]", " ", StringComparison.Ordinal);
        var listHeading = new Regex(
            @"\*\*[^*]{0,80}\b(now|today|immediately|at once|urgent|hospital|team|doctor|nurse|call|phone|ring)"
            + @"\b[^*]{0,80}\*\*\s*:?\s*(\r?\n){1,2}(\s*[-*]\s+.+(\r?\n)){3,}",
            RegexOptions.IgnoreCase);
        Assert.False(listHeading.IsMatch(ownMarkdown),
            "this page now carries what looks like an escalation list of its own. Every such list on "
            + "the site has to be diffed per symptom, in both directions, against /treatments/craniotomy, "
            + "/tests/biopsy and /seizures/what-to-do (§12.8, WI-511/WI-512/WI-519). Do that, or take "
            + "the list out:\n" + listHeading.Match(ownMarkdown).Value);

        var caregiver = Reader(Section("For the person going with them"));
        Assert.Contains("Ask what to do if they get ill before the day", caregiver, StringComparison.Ordinal);
        Assert.Contains("Find out now who you would call", caregiver, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAftercareIsHandedOffRatherThanStartedHere()
    {
        // The caregiver section on a page about PREPARATION has an obvious
        // failure mode: drifting into the aftercare, which /treatments/craniotomy
        // owns and does far better. The section says what it is not doing and
        // sends the reader there.
        var caregiver = Section("For the person going with them");

        Assert.Contains("What you are **not** doing yet is the aftercare", caregiver, StringComparison.Ordinal);
        Assert.Contains("/treatments/craniotomy", caregiver, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoPercentageAndNoPrognosisVocabulary()
    {
        // Contract item 5. Graded against reader text, so the WI-105
        // suppression markers do not put characters into the check that no
        // reader ever sees (WI-509).
        var body = CuratedPage.ReaderText(Page);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", body);

        foreach (var word in new[]
                 {
                     "survival", "life expectancy", "five-year", "prognosis", "how long you have",
                     "per cent",
                 })
        {
            Assert.DoesNotContain(word, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNeitherTheCharacterisationsNorTheMinimisations()
    {
        // "It is only a few blood tests" is the sentence this page invites, on
        // the appointment where somebody is told whether their brain operation
        // can go ahead. Negation-aware (§12.8, WI-511).
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "tests/getting-ready-for-surgery");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // WI-564 is still open, and this page's subject is the one where a
        // British form arrives without anybody deciding: "nil by mouth", "theatre",
        // "pre-med", "anaesthetist". Whitespace-normalised first, because the
        // corpus is hard-wrapped and the gate's own history is of walking past
        // "a\nlift" (§12.8, WI-511).
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // Three this page can reach that the shared list does not carry, because
        // they only have an obvious meaning around an operation. Run over the
        // whole corpus first, per §12.8: no occurrence anywhere.
        foreach (var form in new[] { "nil by mouth", "pre-med", "day case" })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8's warning. Read the section for the slot list, not the last page
        // written.
        //
        // Slot 7 ("what you need first, or need to bring — preparation,
        // clearance, the things a reader can act on before the day") IS this
        // page's subject, so it is four headings rather than one. That is the
        // WI-507 precedent — the slot is a role, not a heading — taken one step
        // further, and the order inside it runs most-actionable first.
        //
        // Slot 8 ("why am I having another one?") is absent and the absence is
        // deliberate: nobody is sent back for a repeat pre-operative
        // assessment, so there is no frightening moment for it to answer.
        // WI-507 established that slot 8 is a condition, not a universal.
        // The explicit `{#id}` is stripped before comparing, so this test is
        // about the words a reader sees and the anchor test below is about the
        // interface. Tying them together is what WI-509 wrote explicit anchors
        // to avoid.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

        Assert.Equal(
            [
                "The short version",                              // 0
                "What is it?",                                    // 1
                "Why am I having all this?",                      // 2
                "What happens, step by step",                     // 3
                "When does all of it happen?",                    // 4, as a role
                "Will any of it hurt?",                           // 5, as a role
                "The hard parts, and what helps",                 // 6a
                "When do I stop eating and drinking?",            // 7
                "Which of my medicines do I keep taking?",        // 7
                "Blood thinners: do not stop one on your own",    // 7
                "What to bring, and what to leave at home",       // 7
                "For the person going with them",                 // caregiver, §12.7
                "Who reads all of this, and what if something is not right?", // 9
                "What to ask your team",                          // 10
                "Where to go next",                               // 11
            ],
            headings);
    }

    [Fact]
    public void TheAnchorsTheBiopsyPageDeepLinksIntoArePinned()
    {
        // §12.8 (WI-508): heading anchors are a published interface. Markdig
        // derives an id from heading TEXT, so a reworded heading breaks every
        // inbound link silently — the reader lands at the top of a long page
        // with no sign anything went wrong. /tests/biopsy deep-links two of
        // these by name, so they are written explicitly and pinned here.
        foreach (var anchor in new[]
                 {
                     "{#what-it-is}", "{#why}", "{#step-by-step}", "{#when}", "{#does-it-hurt}",
                     "{#hard-parts}", "{#fasting}", "{#medicines}", "{#blood-thinners}",
                     "{#what-to-bring}", "{#caregiver}", "{#results}", "{#questions}", "{#where-to-go-next}",
                     "{#short-version}",
                 })
        {
            Assert.Contains(anchor, Page, StringComparison.Ordinal);
        }

        // Every `##` heading carries one, so a section added later cannot
        // quietly rely on a derived id.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline);
        Assert.All(headings, m => Assert.Matches(@"\{#[a-z0-9-]+\}\s*$", m.Groups[1].Value.Trim()));

        // And the sibling really does point at the two it was given, so this
        // test fails on either side of the contract rather than only on ours.
        var biopsy = CuratedPage.Read("tests", "biopsy.md");
        Assert.Contains("/tests/getting-ready-for-surgery#medicines", biopsy, StringComparison.Ordinal);
        Assert.Contains("/tests/getting-ready-for-surgery#fasting", biopsy, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheSharedBlockRatherThanRetypingIt()
    {
        // §12.7. The per-page half sits AFTER the directive; the shared half is
        // never reworded on a page (§12.10).
        var section = Section("For the person going with them");

        Assert.StartsWith("[CAREGIVER]", section, StringComparison.Ordinal);

        // Word SHINGLES, not the block's bold lead-ins — §12.8 records the
        // lead-in check as insufficient in as many words, and WI-519 shipped the
        // weaker version anyway. Copied from the shape already in the sibling
        // test files rather than re-derived.
        var body = CuratedPage.ReaderText(Page).Replace("[CAREGIVER]", " ", StringComparison.Ordinal);
        var next = body.IndexOf("## Where to go next", StringComparison.Ordinal);
        var pageText = next > 0 ? body[..next] : body;

        var pageShingles = Shingles(pageText, 8).ToHashSet();
        var blockPath = Path.Combine(CuratedPage.BlocksRoot, "caregiver.md");
        var block = CuratedPage.Body(File.ReadAllText(blockPath));
        var overlaps = Shingles(block, 8).Where(pageShingles.Contains).Distinct().ToList();

        Assert.True(overlaps.Count == 0,
            "this page restates the shared [CAREGIVER] block, so the reader meets it twice:\n  "
            + string.Join("\n  ", overlaps));

        // A duplicated LINK is not a duplicated sentence.
        foreach (var link in new[] { "/seizures/what-to-do", "/seizures/living-with", "/get-help-now" })
        {
            if (!block.Contains(link, StringComparison.Ordinal))
            {
                continue;
            }

            Assert.DoesNotContain(link, section.Replace("[CAREGIVER]", " ", StringComparison.Ordinal),
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone, and there is no outlook here.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8. ContentCheck only warns on a missing `accessed` date,
        // and this phase's whole source discipline rests on it.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The domains this page's claims actually rest on: the ASA advisory for
        // what the assessment is and how tests get chosen, PMC for the fasting
        // evidence and the medicines, MedlinePlus for the ECG at patient level,
        // radiologyinfo for the chest x-ray, ACS for the bloods, NBTS for the
        // day itself.
        foreach (var domain in new[]
                 {
                     "guidelinecentral.com", "ncbi.nlm.nih.gov", "medlineplus.gov",
                     "radiologyinfo.org", "cancer.org", "braintumor.org",
                 })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // §12.1: NCI patient PDQ is fine for framing and is never the source for
        // naming or grading. Nothing here needs it.
        Assert.DoesNotContain("cancer.gov", front, StringComparison.OrdinalIgnoreCase);

        // PLAN.md §5 bans AHFS DRUG MONOGRAPHS, which MedlinePlus hosts under
        // licence from ASHP. The MedlinePlus page cited here is a MEDICAL TEST
        // page written by the National Library of Medicine, and the distinction
        // is worth a test because the easy mistake is to widen the ban to the
        // domain (§12.13's shape) and lose the only patient-level source in the
        // set that says an ECG is painless.
        Assert.Contains(urls, u => u.StartsWith("https://medlineplus.gov/lab-tests/", StringComparison.Ordinal));
        Assert.DoesNotContain(urls, u => u.Contains("medlineplus.gov/druginfo", StringComparison.Ordinal));

        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor: .NET's multiline `$` does not match before `\r`, and
        // this repo has core.autocrlf=true (WI-501).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    private static IEnumerable<string> Shingles(string text, int n)
    {
        var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+").Select(m => m.Value).ToList();
        for (var i = 0; i + n <= words.Count; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(n));
        }
    }
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class GettingReadyForSurgeryPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/getting-ready-for-surgery";

    private readonly WebApplicationFactory<Program> _factory;

    public GettingReadyForSurgeryPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Getting ready for surgery", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // §12.8's WI-519 lesson: a new library page nothing links to is half
        // shipped, and /tests has no index to catch it. These are the pages a
        // reader is standing on when an operation is being planned.
        var client = _factory.CreateClient();

        foreach (var door in new[] { "/start", "/tests/mri", "/tests/biopsy", "/treatments/craniotomy" })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/treatments/craniotomy", "/tests/biopsy", "/get-help-now");

    [Fact]
    public async Task TheDeepLinkIntoTheScanPageLandsOnARealAnchor() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheDeepLinksFromTheBiopsyPageLandOnRealAnchorsHere() =>
        // The other side of the same contract. /tests/biopsy links
        // #fasting and #medicines on this page, and a reworded heading here
        // breaks them silently.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), "/tests/biopsy");

    [Fact]
    public async Task TheTermsThisPageDefinesAreSuppressedHereAndStillFireElsewhere()
    {
        // §12.8 (WI-509): a popover repeating the paragraph directly beneath it
        // is noise. WI-510's rule is that a "both directions" assertion is only
        // worth writing if both directions are observable — and here they are:
        //
        //   anesthesiologist    suppressed here, fires on /treatments/craniotomy
        //   advance directive   suppressed here, fires on /treatments/craniotomy
        //   blood thinner       suppressed here, fires on /tests/biopsy AND
        //                       /treatments/craniotomy — both of which this
        //                       item introduced the term to, undefined
        //
        // WI-512's other half first: assert the WORD is in this page's prose
        // before asserting its tooltip is not, or the suppression is a no-op and
        // the DoesNotContain passes for the wrong reason.
        var body = CuratedPage.ReaderText(CuratedPage.Read("tests", "getting-ready-for-surgery.md"));
        Assert.Contains("anesthesiologist", body, StringComparison.Ordinal);
        Assert.Contains("advance directive", body, StringComparison.Ordinal);
        Assert.Contains("blood thinner", body, StringComparison.Ordinal);

        var client = _factory.CreateClient();
        var html = await client.GetStringAsync(Url);

        Assert.DoesNotContain("def-anesthesiologist", html);
        Assert.DoesNotContain("def-advance-directive", html);
        Assert.DoesNotContain("def-blood-thinner", html);

        // WI-519's two traps, both live here. A glossary entry defined only on
        // the page that suppresses it is decoration — so each one is proven to
        // fire somewhere. And linking a word turns its tooltip off, because
        // GlossaryMarker skips links; all three are plain prose on the siblings,
        // which is why they fire there.
        var craniotomy = await client.GetStringAsync("/treatments/craniotomy");
        Assert.Contains("def-anesthesiologist", craniotomy);
        Assert.Contains("def-advance-directive", craniotomy);
        Assert.Contains("def-blood-thinner", craniotomy);
        Assert.Contains("def-blood-thinner", await client.GetStringAsync("/tests/biopsy"));
    }
}
