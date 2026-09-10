using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-523: X3, awake craniotomy and brain mapping. The fifth TREATMENT page
/// under the §12.8 LIBRARY template (the shared <see cref="CuratedPage"/>
/// helpers live in TestsLibraryPagesTests.cs), split from WI-510 because the
/// patient has a job to do.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page, and on this page most of those
/// are places where the sources are more reassuring than they have earned:
///
///   * the page's spine — "you may lose a word for a moment, and it comes
///     back; that is the test working" — is true of the TEST. Allowed to drift
///     one sentence, it becomes "whatever you lose comes back", which is false
///     of a change that shows up while tumor is being removed.
///   * the dossier's "Publish as: that is planned for" for a failed awake
///     part, when the study it cites found those people had less tumor taken
///     out and more speech trouble (§12.8, WI-522: a cohort's best sentence).
///   * StatPearls' "gold standard ... improved survival", which a 2026
///     meta-analysis rates as low-certainty, selection-biased evidence.
///   * a seizure risk the literature reports at anywhere from a few in a
///     hundred to about half, depending on who counted (§12.4 R2).
///
/// So the guards are claim-shaped, per sentence (§12.8, WI-520).
/// </summary>
public sealed class AwakeCraniotomyPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "awake-craniotomy.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section as the reader meets it. Not <c>CuratedPage.ReaderText</c>,
    /// which strips front matter by index and returns <c>section[3..]</c> when
    /// fed a section (§12.8, WI-520). <c>Section</c> already flattens.
    /// </summary>
    private static string Reader(string section) => Regex.Replace(section, @"!%(.+?)%", "");

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    /// <summary>
    /// The "### When a word or a hand stops working" subsection alone, flattened.
    /// <c>CuratedPage.Section</c> only splits on <c>## </c>.
    /// </summary>
    private static string TheTestWorking()
    {
        var feels = Regex.Match(Page, @"^## What does it feel like\?.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        var sub = Regex.Match(feels, @"^### When a word or a hand stops working.*?(?=^### |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(sub), "the test-working subsection has gone");
        return CuratedPage.Flatten(Reader(sub));
    }

    private const string FeelsHeading = "What does it feel like?";
    private const string WrongHeading = "What can go wrong";
    private const string BetterHeading = "Is being awake the better way?";

    /// <summary>
    /// A count, however this corpus writes it, including the fractions. Copied
    /// from the WI-522 suite rather than shared, for the reason that suite
    /// gives: a frequency guard and a schedule guard need different
    /// vocabularies (§12.8, WI-521).
    /// </summary>
    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|"
        + @"fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|"
        + @"seventy|eighty|ninety|hundred|dozen)";

    private const string Fraction = @"(?:half|halves|thirds?|quarters?|fifths?|tenths?)";

    /// <summary>
    /// Word runs this page shares with another page ON PURPOSE. Kept tiny for
    /// the WI-521 reason: every entry is a link label or a safety claim.
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // The standard "Where to go next" door, worded identically on every
        // library page so a reader moving between them recognises it.
        "Get help now if you need to talk to a person today.",

        // The recovery claim, in /treatments/craniotomy's words. §12.8 (WI-510):
        // "a claim about how many people recover cannot have two different
        // strengths", and StatPearls' "Most deficits are transient" would have
        // put a stronger one here, one link away from the weaker. One claim,
        // one wording (§12.10).
        "many new problems after brain surgery get better over the following weeks. Some do not.",
    ];

    private static readonly HashSet<string> AllowedShingles =
        [.. DeliberatelyShared.SelectMany(s => Shingles(s, 8))];

    [Fact]
    public void TheShortVersionCarriesTheSpineAndItsScope()
    {
        // The backlog's sentence, the one "nobody says". §12.3: readers consume
        // 20 to 28% of a page, so it lives in the summary and not only below.
        var summary = Reader(Section("The short version"));
        var sentences = CuratedPage.SentencesOf(summary.Replace("**", ""));

        var loss = Array.FindIndex(sentences, s => Regex.IsMatch(s, @"lose a word"));
        Assert.True(loss >= 0, "the short version no longer says a word can be lost during the test");

        // Scoped to the TEST in the sentence that makes the claim, not merely
        // nearby: "you may lose a word ... It comes back" with no test in view
        // is a promise about the operation (§12.14: a proximity window is not
        // a scope).
        Assert.Matches(new Regex(@"^During the test\b"), sentences[loss]);
        Assert.Matches(new Regex(@"for a moment", RegexOptions.IgnoreCase), sentences[loss]);

        // "It comes back" is the NEXT sentence and "that is the test working"
        // the one after — adjacency, not a window.
        Assert.Equal("It comes back.", sentences[loss + 1]);
        Assert.Matches(new Regex(@"^That is the test working\.$"), sentences[loss + 2]);

        // And what follows is what the test SHOWS, not a widening of what comes
        // back. /review inserted "The same goes for anything you lose later in
        // the operation." at exactly this point and every guard stayed green.
        Assert.Equal("It shows the surgeon a part of your brain to leave alone.", sentences[loss + 3]);

        // Bold, so a scanning reader lands on it.
        Assert.Contains("**During the test you may lose a word", CuratedPage.Flatten(Section("The short version")),
            StringComparison.Ordinal);

        // And the summary says the reader is not awake for the opening, which is
        // the misconception the dossier says the whole fear rests on.
        Assert.Matches(new Regex(@"asleep, or very sleepy, while your skull is opened and closed"), summary);
    }

    [Fact]
    public void EveryComesBackClaimOnThePageIsAboutTheTest()
    {
        // The spine is true of the test and false of the operation: a change
        // that shows up while tumor is being taken out is the signal to STOP,
        // and whether it passes is the "some do not" of the risk section. So
        // every return-shaped sentence on the page must either name the test
        // (the current, the test, a spot) or be the shared recovery sentence
        // with its "Some do not" beside it.
        //
        // "the test" is NOT enough to scope a sentence, since /review: the
        // first version of step 7 called the removal phase "testing", so
        // "Like the test, any change then comes back" named the test and
        // passed. The current and the spot are what belong to the test alone.
        var returns = new Regex(
            @"\b(?:comes?|came|coming) back\b|\breturns?\b|\bwears? off\b|\bgoes away\b|\bgo away\b|"
            + @"\brecover(?:s|ed)?\b|\bget(?:s)? better\b|\bback to how\b|\bback to normal\b|"
            + @"\bclears? up\b|\bcleared up\b",
            RegexOptions.IgnoreCase);
        var aboutTheTest = new Regex(@"\b(?:current|a spot|this spot|that spot)\b", RegexOptions.IgnoreCase);

        // A universal over what is lost, however it is phrased, is the spine
        // generalised to the operation.
        Assert.DoesNotMatch(
            new Regex(@"\b(?:anything|everything|whatever|all)\b[^.]{0,30}\b(?:lose|lost|loses)\b|the same goes for",
                RegexOptions.IgnoreCase),
            Body);

        var sentences = CuratedPage.SentencesOf(Body.Replace("**", ""));
        for (var i = 0; i < sentences.Length; i++)
        {
            var s = sentences[i];
            if (!returns.IsMatch(s))
            {
                continue;
            }

            // The shared recovery sentence, only with its limit beside it.
            var shared = s.StartsWith("Many new problems after brain surgery get better", StringComparison.Ordinal)
                         && i + 1 < sentences.Length && sentences[i + 1] == "Some do not.";

            // The summary's bare "It comes back." is scoped by the sentence
            // before it, which the short-version test pins to "During the test".
            var spine = s == "It comes back." && i > 0
                        && sentences[i - 1].StartsWith("During the test", StringComparison.Ordinal);

            Assert.True(aboutTheTest.IsMatch(s) || shared || spine,
                "a sentence promises that something comes back or gets better without saying it is the TEST "
                + "that it comes back from:\n  " + s);
        }

        // The bolded return in the test-working section is tied to the current
        // stopping, in the same sentence.
        var working = TheTestWorking();
        Assert.Matches(new Regex(@"\*\*Then the current stops, and the word or the movement comes back\.\*\*"), working);

        // And the stop signal is stated separately, in the steps, and SAID to
        // be different: a change during removal is how they know where to
        // stop, and it may not pass (patpersp: "a permanent sensory impairment
        // as a result of the surgical resection"; UCSF: an "irreversible loss
        // ... associated with permanent postoperative paresis"). The front
        // matter claimed this from the first draft; the page did not say it
        // until /review.
        var steps = Reader(Section("What happens, step by step"));
        var removal = CuratedPage.SentencesOf(steps).Single(s => s.Contains("notice a change as it starts", StringComparison.Ordinal));
        Assert.DoesNotMatch(returns, removal);
        Assert.DoesNotMatch(new Regex(@"\btest", RegexOptions.IgnoreCase), removal);
        Assert.Contains(
            "That is how they know where to stop. A change at this stage is not the test. It may not pass, and "
            + "that is exactly why they stop.",
            steps, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTestWorkingSectionSaysAMistakeIsHelpAndEndsOnIt()
    {
        var working = TheTestWorking();

        // Both kinds of loss the backlog names: a word and a limb.
        Assert.Matches(new Regex(@"not be able to say the word"), working);
        Assert.Matches(new Regex(@"Your hand may not move"), working);

        // The reframe, attributed to the speech therapist it came from (NBTS),
        // and the section ends on the reader having helped — §12.6 never end a
        // section on a frightening sentence, pinned at the END of the last
        // sentence (§12.8, WI-521).
        Assert.Contains("best time to make a mistake", working, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"A speech therapist[^.]*best time to make a mistake"), working);
        Assert.Matches(new Regex(@"You have helped\.\s*$"), working);
    }

    [Fact]
    public void TheSeizureRiskCarriesNoNumberAndThePageSaysWhy()
    {
        // §12.4 R2, and the dossier's own §23.3 "do not publish a number". Five
        // answers in five sources (2.9-54%, 2-22%, a pooled 8%, 3.4-12.6%,
        // 2.1%), so nothing, and the reason is on the page (WI-521) — INSIDE the
        // seizure paragraph since /review, because the disagreement is about
        // seizures and not about the failed-awake figure, where the sources
        // agree.
        var wrong = Reader(Section(WrongHeading));
        var seizure = Regex.Match(wrong, @"\*\*A seizure during the test\.\*\*(.*?)(?=\*\*The awake part)").Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(seizure), "the seizure paragraph has gone");

        Assert.Matches(
            new Regex(@"^\s*The current can set off a seizure\. There is no number for this on the page\. "
                      + @"The studies disagree so much about how often it happens that any one figure would give "
                      + @"you the wrong idea\."),
            seizure);

        // Claim-shaped (§12.8, WI-520): a count or a fraction running forward
        // into a population or a denominator, anywhere on the page — "one in
        // ten people", "a few in every hundred operations", "about half of
        // everyone tested". The quantity words a correct sentence here needs
        // ("two things", "one large study", "one is serious") have no
        // population after them.
        var frequency = new Regex(
            $@"\b(?:{CountWord}|{Fraction}|a few|several|some)\b(?:\s+\w+){{0,3}}?\s+"
            // Interpolated: the WI-523 first version wrote this half as a plain
            // @"" string, so "{CountWord}" was literal text and the half could
            // never fire. The canaries below are what found it.
            + $@"(?:in|out of|of every|in every)\s+(?:{CountWord}|every|a)\b",
            RegexOptions.IgnoreCase);
        // The words between the count and the population may not be the study
        // itself: "in one small study of people" names a study, not a rate,
        // and it is the attribution §12.8 (WI-507) requires.
        var population = new Regex(
            $@"\b(?:{CountWord}|{Fraction})\b(?:\s+(?!stud|series|group|hospital|centers?\b)\w+){{0,2}}?\s+"
            + @"(?:of\s+)?(?:people|patients|operations|cases|times|everyone|everybody|those|them)\b",
            RegexOptions.IgnoreCase);

        // The guards prove they can fire before they are trusted to pass
        // (§12.10). /review walked the first population list with "about half
        // of everyone tested".
        Assert.Matches(frequency, "It happens to about one in ten people.");
        Assert.Matches(frequency, "It is a few in every hundred.");
        Assert.Matches(population, "At some hospitals it happened to about half of everyone tested.");
        Assert.Matches(population, "In some hospitals it is up to half of patients.");
        Assert.DoesNotMatch(population, "In one small study of people who had it done, some remembered nothing.");

        var sentences = CuratedPage.SentencesOf(Body);
        Assert.True(sentences.Length > 100, "the sentence splitter found almost nothing to check");
        foreach (var s in sentences)
        {
            Assert.DoesNotMatch(frequency, s);
            Assert.DoesNotMatch(population, s);
        }

        Assert.DoesNotMatch(new Regex(@"\d+(\.\d+)?\s*(%|percent)"), Body);
        Assert.DoesNotContain("per cent", Body, StringComparison.OrdinalIgnoreCase);

        // What the sources DO agree on is printed: the test sets it off, earlier
        // seizures and some tumor places make it likelier, it often stops on its
        // own, most pass without lasting harm but can leave weakness for a while
        // (StatPearls, "transient motor deterioration"), and the rare serious
        // case is named as what it is — both directions, §12.8 WI-506 — and the
        // paragraph ends on an action.
        Assert.Contains("more likely if you have had seizures before, and for tumors in some parts of the brain",
            seizure, StringComparison.Ordinal);
        Assert.Contains("Most pass without doing any lasting harm, though an arm or a leg can stay weak",
            seizure, StringComparison.Ordinal);
        Assert.Contains("Rarely, one is serious enough to affect breathing or the heart.", seizure, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"Ask what your own risk is\.\s*$"), seizure);
    }

    [Fact]
    public void TheFailedAwakePartCarriesItsCostNotOnlyItsReassurance()
    {
        // The dossier's "Publish as" was "that is planned for" and nothing else.
        // The series it cites found those people had less tumor removed and
        // more speech trouble. §12.8 (WI-522): a cohort's best sentence can hide
        // its main result. How often, then planned for, then the cost, then
        // avoidable, in one paragraph, in that order.
        var wrong = Reader(Section(WrongHeading));
        var para = Regex.Match(wrong, @"\*\*The awake part has to stop\.\*\*(.*?)(?=\*\*Waking up with)").Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(para), "the failed-awake paragraph has gone");

        // Here the sources AGREE on the size (about 2%, 0-6%, 6.4%), so R2's
        // plain-words form is printed rather than silence (WI-521).
        Assert.StartsWith("Sometimes it cannot go on. This is uncommon.", para.Trim(), StringComparison.Ordinal);

        var planned = para.IndexOf("That is planned for", StringComparison.Ordinal);
        var cost = para.IndexOf("less of their tumor taken out", StringComparison.Ordinal);
        var speech = para.IndexOf("more trouble with speech afterwards", StringComparison.Ordinal);
        var avoidable = para.IndexOf("could have been avoided", StringComparison.Ordinal);

        Assert.True(planned >= 0 && cost > planned && speech > planned && avoidable > cost,
            "the failed-awake paragraph must say it is planned for, THEN what it cost, THEN that most were avoidable");

        // The cost is attributed in the sentence that states it (§12.8,
        // WI-507), and is not taken back in the same breath: the speech
        // trouble in that series was still there at three months, so "all of
        // it short-lived" (a /review mutation) is false.
        var costSentence = CuratedPage.SentencesOf(para).Single(s => s.Contains("less of their tumor", StringComparison.Ordinal));
        Assert.Contains("in one large study", costSentence, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(@"short-lived|short lived|temporar|brief|did not last|went away|cleared|passed|for a while|at first",
                RegexOptions.IgnoreCase),
            costSentence);

        // And the avoidable-by-selection finding keeps both of the study's
        // reasons, not just the flattering one.
        Assert.Matches(new Regex(@"choosing carefully who has it done and by care with the medicines"), para);
    }

    [Fact]
    public void TheNewProblemParagraphSaysWhatTheTestCannotDoAtTheCraniotomyPagesStrength()
    {
        var wrong = Reader(Section(WrongHeading));
        var para = Regex.Match(wrong, @"\*\*Waking up with something new\.\*\*(.*)$").Groups[1].Value;

        // What the test cannot do (§12.8, WI-506), stated as purpose, not as
        // proven efficacy.
        Assert.Contains("The test is there to protect you from that. It cannot rule it out.", para,
            StringComparison.Ordinal);

        // An efficacy claim is allowed in exactly one place: the "better"
        // section's attributed "Some studies suggest", immediately followed by
        // "But most of those studies cannot settle it." Anywhere else it is the
        // page promising what the evidence has not settled.
        var efficacy = new Regex(
            @"\b(?:lowers?|reduces?|cuts?|halves)\b[^.]{0,30}\b(?:risk|chance)|\bsafer\b|"
            + @"\bfewer\s+(?:new\s+)?(?:problems|deficits)\b|\bless likely\b|\bmuch less\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(efficacy, "Being awake makes that much less likely.");
        var sentences = CuratedPage.SentencesOf(Body.Replace("**", ""));
        for (var i = 0; i < sentences.Length; i++)
        {
            if (!efficacy.IsMatch(sentences[i]))
            {
                continue;
            }

            Assert.True(
                sentences[i].StartsWith("Some studies suggest", StringComparison.Ordinal)
                && i + 1 < sentences.Length && sentences[i + 1] == "But most of those studies cannot settle it.",
                "an efficacy claim outside the attributed, qualified one:\n  " + sentences[i]);
        }

        // ONE strength for the recovery claim, and it is /treatments/craniotomy's.
        // A "most" here, off StatPearls, would sit one link away from that
        // page's "many" (§12.8, WI-510).
        const string shared = "many new problems after brain surgery get better over the following weeks. Some do not.";
        var cranio = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md")));
        Assert.Contains(shared, para, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(shared, cranio, StringComparison.OrdinalIgnoreCase);

        // Claim-shaped: a quantifier over a recovery verb, however phrased.
        // /review walked "most ... get better" with "Nearly always they clear up."
        var stronger = new Regex(
            @"\b(?:most|nearly all|almost all|all|nearly always|almost always|always|usually|mostly)\b"
            + @"[^.]{0,40}\b(?:get better|gets better|recover|improve|pass|clear up|clears up|go away|goes away|"
            + @"settle|come back|comes back|wear off)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(stronger, "Nearly always they clear up.");
        Assert.DoesNotMatch(stronger, para);

        // And SMA syndrome is handed off rather than re-told.
        Assert.Contains("/treatments/craniotomy#sma-syndrome", Section(WrongHeading), StringComparison.Ordinal);
        Assert.Matches(new Regex(@"Ask your team what they expect for you\.\s*$"), para);
    }

    [Fact]
    public void ThePageMakesNoBetterThanClaimAndDoesNotTalkAReaderOutOfAsking()
    {
        // StatPearls: "gold standard", "fewer postoperative neurological
        // deficits (7% vs 23%)", "improved survival". The 2026 meta-analysis:
        // low certainty, observational, selection bias, one small trial the
        // other way. So the section answers "Not for everyone." first (§12.6),
        // names why the studies cannot settle it, and says it is unproved.
        var better = Reader(Section(BetterHeading));
        Assert.StartsWith("Not for everyone.", better, StringComparison.Ordinal);
        Assert.Contains("Whether it is better has not been proved.", better, StringComparison.Ordinal);
        Assert.Contains("The people chosen to be awake may have been different", better, StringComparison.Ordinal);

        // Claim-shaped: a comparative about awake versus asleep outcomes,
        // anywhere on the page. /review walked the first list with "Being awake
        // lets more of your tumor come out, with your speech in better shape".
        var comparative = new Regex(
            @"gold standard|the best way|better than (?:being )?asleep|(?:gives|leads to|means) better|"
            + @"more (?:of (?:the|your) )?tumor (?:out|removed|come|comes|to come)|more of (?:the|your) tumor|"
            + @"better shape|better results|best results|proven to|"
            + @"lets? (?:the surgeon |them |you )?(?:take|get|remove) more",
            RegexOptions.IgnoreCase);
        Assert.Matches(comparative, "Being awake lets more of your tumor come out, with your speech in better shape.");
        Assert.DoesNotMatch(comparative, Body);

        // BLOCKER at /review. The first version told a reader they had "not
        // missed out" if it was not offered, while the meta-analysis the section
        // rests on CONCLUDES it "should be strongly considered" for high-grade
        // gliomas near eloquent areas (§12.8, WI-512: a source cited for the
        // position it argues against). The reviewers' conclusion is printed,
        // scoped as they scope it, and a reader whose tumor is next to speech
        // is told it is fair to ask.
        Assert.DoesNotMatch(new Regex(@"missed out|nothing to miss|no reason to ask", RegexOptions.IgnoreCase), Body);
        var considered = CuratedPage.SentencesOf(better).Single(s => s.Contains("strongly considered", StringComparison.Ordinal));
        Assert.Contains("high-grade gliomas", considered, StringComparison.Ordinal);
        Assert.Contains("at a hospital with the experience", considered, StringComparison.Ordinal);
        Assert.Contains("If your tumor is next to the parts for speech and being awake has not come up, it is fair to ask why.",
            better, StringComparison.Ordinal);

        // And the other direction (§12.8, WI-506): a reader who does not want
        // it may say so.
        Assert.Contains("If you do not want to be awake, say so.", better.Replace("**", ""), StringComparison.Ordinal);

        // The one surgeon's comparative is attributed to that surgeon.
        var easier = CuratedPage.SentencesOf(better).Single(s => s.Contains("easier on you", StringComparison.Ordinal));
        Assert.StartsWith("The surgeon quoted above says", easier, StringComparison.Ordinal);

        // Contract item 5, on the page whose best-known source prints survival.
        foreach (var word in new[]
                 {
                     "survival", "survive", "life expectancy", "live longer", "prognosis", "median", "death", "die ",
                 })
        {
            Assert.DoesNotContain(word, Body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageSaysWhatAScanCanAndCannotDo()
    {
        // §12.8 (WI-506): a library page says what it cannot do. A functional
        // MRI helps plan; RadiologyInfo says more tests may be used "to confirm
        // the results of fMRI" for brain surgery. Both halves, so neither "the
        // scan is enough" nor "the scan is useless" can be read off the page.
        var what = Reader(Section("What is an awake craniotomy?"));
        Assert.Contains("A scan before the day can suggest where speech or movement sit", what, StringComparison.Ordinal);
        Assert.Contains("it helps plan the operation", what, StringComparison.Ordinal);
        Assert.Contains("checks it in the moment", what, StringComparison.Ordinal);

        var steps = Reader(Section("What happens, step by step"));
        Assert.Contains("It does not replace the test in the room.", steps, StringComparison.Ordinal);
        Assert.Contains("Some hospitals do a functional MRI", steps, StringComparison.Ordinal);

        // Claim-shaped since /review ("It shows exactly where each job sits."):
        // no certainty word in any sentence about a scan, in the sections where
        // the scans are discussed.
        var certainty = new Regex(@"\b(?:exactly|precisely|for certain|for sure|pinpoints?|shows where|always shows)\b",
            RegexOptions.IgnoreCase);
        // A sentence about a scan, and the sentence right after one: the
        // /review mutation followed "it helps plan the operation."
        var aboutAScan = new Regex(@"\b(?:scan|scanner|MRI)\b", RegexOptions.IgnoreCase);
        var scanSentences = CuratedPage.SentencesOf(what + " " + steps);
        for (var i = 0; i < scanSentences.Length; i++)
        {
            if (aboutAScan.IsMatch(scanSentences[i]) || (i > 0 && aboutAScan.IsMatch(scanSentences[i - 1])))
            {
                Assert.DoesNotMatch(certainty, scanSentences[i]);
            }
        }

        // And /treatments/craniotomy says the pre-op mapping scans at the same
        // strength ("to suggest", "sometimes"), since /review found it saying
        // they "map where those areas sit" — one claim, two strengths.
        var cranio = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md")));
        Assert.Contains("Sometimes extra scans, if the tumor is near the areas for speech or movement, to suggest where",
            cranio, StringComparison.Ordinal);
        Assert.DoesNotContain("to map where those areas sit", cranio, StringComparison.Ordinal);
    }

    [Fact]
    public void NoOneInAStoryIsGivenAGender()
    {
        // WI-510 AND WI-511 each shipped a guessed gender for a real person in
        // a quoted source, and the second did it after the first was recorded.
        // NBTS genders two of its five people and not the other three. Tracking
        // which is which is where the guess gets in, so the page writes all of
        // them neutrally, and the guard is page-wide: /review walked the first
        // version (story sentence plus one) with a pronoun two sentences in.
        var gendered = new Regex(
            @"\b(?:he|she|him|her|his|hers|himself|herself|man|woman|men|women|husband|wife|mr|mrs|ms)\b",
            RegexOptions.IgnoreCase);
        Assert.DoesNotMatch(gendered, Body);

        // The stories are still there, so the guard is guarding something.
        Assert.True(Regex.Matches(Body, @"\bOne (?:person|patient)\b").Count >= 5,
            "expected the five stories (the alphabet, the night before, the hips, the swan, the face)");
    }

    [Fact]
    public void TheSmallStudyIsAttributedInTheSentencesThatUseIt()
    {
        // §12.8 (WI-507). Nine patients in one center; "some remembered
        // nothing", the sore frame and the tooth pain, and "none said it was
        // harder than expected" are that study's and nobody else's.
        var feels = Reader(Section(FeelsHeading));
        var sentences = CuratedPage.SentencesOf(feels);

        var remembered = Array.FindIndex(sentences, s => s.Contains("remembered nothing", StringComparison.Ordinal));
        Assert.True(remembered >= 0);
        Assert.StartsWith("In one small study", sentences[remembered], StringComparison.Ordinal);

        // The sore part, at its real size (five of nine remembered the frame,
        // two called it moderately painful, two had tooth pain) — /review found
        // the first version read as if the frame were the only sore thing and
        // hardly anyone felt it.
        Assert.StartsWith("In the same study, more than half remembered the frame", sentences[remembered + 1],
            StringComparison.Ordinal);
        Assert.Contains("two had pain in their teeth", sentences[remembered + 1], StringComparison.Ordinal);

        // Every "harder than expected" sentence is that study's, and says so by
        // position: "None of them", right after the same-study sentence.
        // /review turned it into "Nobody finds it harder than they expected."
        for (var i = 0; i < sentences.Length; i++)
        {
            if (!Regex.IsMatch(sentences[i], @"harder than (?:they|you|people|anyone)", RegexOptions.IgnoreCase))
            {
                continue;
            }

            Assert.StartsWith("None of them", sentences[i], StringComparison.Ordinal);
            Assert.StartsWith("In the same study", sentences[i - 1], StringComparison.Ordinal);
        }

        Assert.DoesNotMatch(new Regex(@"\b(?:nobody|no one|everyone|everybody|most people)\b[^.]{0,40}expect",
            RegexOptions.IgnoreCase), Body);
    }

    [Fact]
    public void ThePageMakesNoPromiseAboutWhatTheTeamWillDo()
    {
        // WI-521 and WI-522 both shipped a guarantee about somebody else's
        // behaviour ("your team will start again", "your team will think
        // again"), found by the end-to-end read both times. The same shape,
        // anywhere on the page, with any member of the team as the subject —
        // /review walked a narrower list with "the anesthesiologist will put
        // you back to sleep".
        var promise = new Regex(
            @"\b(?:team|surgeons?|anesthesiologists?|nurses?|therapists?|doctors?|neurologists?|staff|they|someone|"
            + @"somebody|everyone)\s+(?:will|'ll|always)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(promise, "If you ask, the anesthesiologist will put you back to sleep.");
        Assert.DoesNotMatch(promise, Body);

        // And where this page's first draft asserted what the pre-op talk
        // covers, it now hands the job to the reader.
        Assert.Contains("tell the team what you most need to keep, even if they do not ask",
            Reader(Section("What you need first")), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoEscalationListAndRoutesTheCaregiverToTheOneThatExists()
    {
        // The call-today and call-an-ambulance lists after this operation are
        // /treatments/craniotomy's, tier-diffed there against every other list
        // on the site (§12.8, WI-511/512/519). A second copy here would be a
        // third place the same symptom can land in a different tier.
        //
        // The shape guard is copied from WI-522, not re-derived (§12.8, WI-519):
        // find every list FIRST; list items include their indented continuation
        // lines; read the BODY. Then two judgements, because /review walked the
        // lead-in judgement alone with "**Watch for these in the first
        // days:**" over "- A seizure." — no urgency word, still a symptom list.
        var urgency = new Regex(
            @"\b(now|today|immediately|at once|urgent(ly)?|ambulance|emergency|911|"
            + @"call|phone|ring|same day|straight away|right away|do not wait|cannot wait|"
            + @"watch for|look out for|watch out|warning|signs? of|if you notice|if they have|first days)\b",
            RegexOptions.IgnoreCase);
        var symptom = new Regex(
            @"\b(seizures?|fits?|weak(ness)?|numb(ness)?|fever|headaches?|wound|vomit\w*|throwing up|confus\w*|"
            + @"drows\w*|trouble (?:finding|speaking|seeing)|vision|breath\w*|chest pain|bleed\w*|leak\w*)\b",
            RegexOptions.IgnoreCase);

        var markdown = CuratedPage.Body(Page);
        var runs = Regex.Matches(
            markdown, @"(?:^[ \t]*(?:[-*]|\d+\.)[ \t]+.+(?:\r?\n[ \t]+\S.*)*(?:\r?\n|$)){2,}", RegexOptions.Multiline);
        Assert.True(runs.Count >= 5, $"the list finder found {runs.Count} lists on a page that has at least five");

        foreach (Match run in runs)
        {
            var paragraphs = Regex.Split(markdown[..run.Index], @"\r?\n[ \t]*\r?\n")
                .Where(p => p.Trim().Length > 0).ToList();
            var before = paragraphs.Count > 0 ? paragraphs[^1] : "";

            if (Regex.IsMatch(before.TrimStart(), @"^(?:[-*]|\d+\.)\s"))
            {
                continue;
            }

            Assert.False(urgency.IsMatch(before),
                "this page now carries what looks like an escalation list of its own. The lists after this "
                + "operation belong to /treatments/craniotomy and are tier-diffed there. The list:\n" + run.Value
                + "\nIntroduced by:\n" + before);

            // Symptom words are allowed in two lists only: the questions, which
            // ask rather than triage, and "Where to go next", which names pages.
            if (before.TrimStart().StartsWith("## What to ask your team", StringComparison.Ordinal)
                || before.TrimStart().StartsWith("## Where to go next", StringComparison.Ordinal))
            {
                continue;
            }

            var bullets = Regex.Matches(run.Value, @"^[ \t]*(?:[-*]|\d+\.)[ \t]+(.+(?:\r?\n[ \t]+\S.*)*)", RegexOptions.Multiline);
            Assert.All(bullets, b => Assert.False(symptom.IsMatch(b.Groups[1].Value),
                "a list outside the questions names a symptom, which makes it a triage list:\n  " + b.Value));
        }

        foreach (var phrase in new[]
                 {
                     "call an ambulance", "these cannot wait", "911", "emergency room", "urgent care",
                     "go to the er", "the same day if", "call the team the same day",
                 })
        {
            Assert.DoesNotContain(phrase, Body, StringComparison.OrdinalIgnoreCase);
        }

        // The route, which is what makes the absence a decision.
        var care = Reader(Section("For the person going with them"));
        Assert.Contains("/treatments/craniotomy#caregiver", care, StringComparison.Ordinal);
        Assert.Contains("the two lists of when to call", care, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheBlockAndSitsAfterSlotSeven()
    {
        // §12.7 and contract item 12. The block, under the heading /tests
        // pages use for the person alongside ("going with them"), after slot 7
        // and before slot 9 (§12.8).
        var raw = Regex.Match(Page, @"^## For the person going with them.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.Contains("[CAREGIVER]", raw, StringComparison.Ordinal);

        var headings = Headings();
        Assert.Equal(headings.IndexOf("What you need first") + 1, headings.IndexOf("For the person going with them"));
        Assert.Equal(headings.IndexOf("For the person going with them") + 1,
            headings.IndexOf("Who reads it, and how do I get the result?"));

        // The page's own caregiver prose does not restate the block (the shingle
        // check, not the bold-lead-in check §12.8 calls insufficient, WI-519).
        var block = CuratedPage.Flatten(Regex.Replace(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")), @"^---.*?\n---", "",
            RegexOptions.Singleline));
        var blockShingles = Shingles(block, 8).ToHashSet();
        var own = Shingles(Reader(Section("For the person going with them")), 8)
            .Where(blockShingles.Contains).Where(CarriesContent).ToList();
        Assert.True(own.Count == 0, "the caregiver section restates the block:\n  " + string.Join("\n  ", own));
    }

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // §12.8 (WI-521): the restatement check runs over the WHOLE corpus.
        // This page's nearest neighbour is /treatments/craniotomy, which has
        // carried an awake-surgery passage since WI-510, and the guard
        // deliberately does not name it.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("awake-craniotomy.md", StringComparison.Ordinal));

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

        // The operation itself, the stay, and the weeks after are handed off.
        foreach (var anchor in new[] { "#how-long", "#what-can-go-wrong", "#caregiver", "#how-much-came-out", "#sma-syndrome" })
        {
            Assert.Contains("/treatments/craniotomy" + anchor, Body, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheStayIsNotComparedWithAnAsleepOperation()
    {
        // StatPearls: "3 to 4 days, compared to 9 days"; the 2026 meta-analysis
        // 4.1 v 6.95 — both observational, both selection-biased. The first
        // draft of this page said the stay was "much the same as for any brain
        // operation", which contradicted its own sources in the other
        // direction. Neither comparison is printed.
        var howLong = Reader(Section("How long does it take?"));
        Assert.DoesNotMatch(
            new Regex(@"much the same|the same as|shorter|longer than|sooner|fewer days|more days", RegexOptions.IgnoreCase),
            howLong);
        Assert.Contains("depends on the operation and on you", howLong, StringComparison.Ordinal);

        // Page-wide as well, since /review put "People who were awake usually go
        // home a few days sooner" in the caregiver section, where the
        // section-scoped check could not see it.
        var comparison = new Regex(
            @"\b(?:go(?:es)? home|home|hospital|stay(?:ed)?|discharged?)\b[^.]{0,60}"
            + @"\b(?:sooner|earlier|shorter|longer than|fewer days|more days|quicker|faster)\b|"
            + @"\b(?:sooner|earlier|shorter|quicker|faster)\b[^.]{0,40}\b(?:home|hospital|stay)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(comparison, "People who were awake usually go home a few days sooner than people who were asleep.");
        Assert.DoesNotMatch(comparison, Body);

        // R1 keeps the durations that orient: the current is on for seconds,
        // and one team's aim for the awake part, attributed in its sentence
        // (§12.8, WI-507) with its reason (§12.4 R1).
        Assert.Contains("usually less than four", howLong, StringComparison.Ordinal);
        var twoHours = CuratedPage.SentencesOf(howLong).Single(s => s.Contains("two hours", StringComparison.Ordinal));
        Assert.StartsWith("One team that does these tries to keep", twoHours, StringComparison.Ordinal);
        Assert.Contains("because people get tired", twoHours, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageUsesNeitherTheCharacterisationsNorTheMinimisations()
    {
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "treatments/awake-craniotomy");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, per §12.8 (WI-510: promote at the SECOND page). These only
        // mean something on a page about being awake for a brain operation; all
        // three are corpus-clean.
        foreach (var phrase in new[] { "just a test", "nothing to be scared of", "you will feel nothing" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // "practice" is the one this page needed most: the verb is "practise"
        // in the UK and the page asks the reader to practice twice.
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
                     "tumour", "consultant", "straight after", "straight away", "specialist nurse", "operating theatre",
                 })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("can I practice it more than once", body, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8: read the section for the slot list, not the last page written.
        //
        // Carried: 0, 1, 2, 3, 4, 5, 6a, 6b (as "What can go wrong", the
        // WI-510 precedent for an operation), 6c, 7, the caregiver section,
        // 9, 10, 11.
        //
        // NOT carried: 8, "why am I having another one?" A second operation is
        // a /treatments/craniotomy question and that page answers it; nothing
        // in this page's sources is about repeating the AWAKE part.
        Assert.Equal(
            [
                "The short version",                                    // 0
                "What is an awake craniotomy?",                         // 1
                "Why am I having one?",                                 // 2
                "What happens, step by step",                           // 3
                "How long does it take?",                               // 4
                "What does it feel like?",                              // 5
                "What is hard about it, and what can be done",          // 6a
                "What can go wrong",                                    // 6b
                "Is being awake the better way?",                       // 6c
                "What you need first",                                  // 7
                "For the person going with them",                       // caregiver
                "Who reads it, and how do I get the result?",           // 9
                "What to ask your team",                                // 10
                "Where to go next",                                     // 11
            ],
            Headings());

        // The reason given for dropping slot 8 is only true while that page
        // still answers it.
        var cranio = CuratedPage.Read("treatments", "craniotomy.md");
        Assert.Matches(new Regex(@"^## Why am I having another operation\?", RegexOptions.Multiline), cranio);
    }

    [Fact]
    public void TheAnchorsArePinned()
    {
        // §12.8 (WI-508): heading anchors are a published interface. The spine
        // gets its own, so a hub or a feed item can deep-link the one paragraph
        // that removes the most fear.
        //
        // Each anchor on ITS heading: a whole-page Contains passes when an
        // anchor has been moved onto a different heading (/review).
        foreach (var (heading, anchor) in new[]
                 {
                     ("## The short version", "short-version"), ("## What is an awake craniotomy?", "what-it-is"),
                     ("## Why am I having one?", "why"), ("## What happens, step by step", "step-by-step"),
                     ("## How long does it take?", "how-long"), ("## What does it feel like?", "what-it-feels-like"),
                     ("### When a word or a hand stops working", "the-test-working"),
                     ("## What is hard about it, and what can be done", "what-is-hard"),
                     ("## What can go wrong", "what-can-go-wrong"), ("## Is being awake the better way?", "is-it-better"),
                     ("## What you need first", "before"), ("## For the person going with them", "caregiver"),
                     ("## Who reads it, and how do I get the result?", "results"),
                     ("## What to ask your team", "questions"), ("## Where to go next", "where-to-go-next"),
                 })
        {
            Assert.Matches(new Regex("^" + Regex.Escape(heading) + @" \{#" + anchor + @"\}\s*$", RegexOptions.Multiline), Page);
        }

        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline);
        Assert.All(headings, m => Assert.Matches(@"\{#[a-z0-9-]+\}\s*$", m.Groups[1].Value.Trim()));
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate() =>
        Assert.DoesNotContain(":::", Page);

    [Fact]
    public void TheDossierClaimsThatDidNotSurviveTheirSourcesAreAbsent()
    {
        // §12.14: banning a URL is not fixing a claim, so each ban is paired
        // with a grep for what it was carrying.
        var front = CuratedPage.FrontMatter(Page);
        var cited = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();

        foreach (var (url, why) in new[]
                 {
                     ("pubmed.ncbi.nlm.nih.gov/19404147", "returned a cookie wall and was never read"),
                     ("PMC7281396", "is one centre's nTMS research, cited by the dossier as a general requirement"),
                     ("academic.oup.com", "is on the known-dead list"),
                     ("sciencedirect.com", "is on the known-dead list"),
                     ("mayoclinic.org", "is on the known-dead list"),
                     ("hopkinsmedicine.org", "is on the known-dead list"),
                 })
        {
            Assert.False(cited.Any(u => u.Contains(url, StringComparison.OrdinalIgnoreCase)),
                $"{url} is cited again, and it {why}");
        }

        // The claims: the eloquent-region proportion, a centre-volume
        // threshold, the stay comparison, and TMS.
        foreach (var claim in new[]
                 {
                     "46", "61", "nearly 100", "hundred", "three to four days", "nine days",
                     "TMS", "magnetic", "tractography", "gold standard",
                 })
        {
            Assert.DoesNotContain(claim, Body, StringComparison.OrdinalIgnoreCase);
        }

        // A volume threshold in any shape ("close to a hundred of them every
        // year", /review): a count of operations per year is a number a reader
        // would hold their own hospital to.
        Assert.DoesNotMatch(
            new Regex($@"\b(?:{CountWord}|dozens?|hundreds?|many)\b[^.]{{0,30}}\b(?:a|every|each|per) year\b",
                RegexOptions.IgnoreCase),
            Body);

        // And the front matter says WHY, where the next reader will see it
        // (§12.13). Flattened and de-commented: the block is hard-wrapped.
        var notes = CuratedPage.Flatten(Regex.Replace(front, @"(?m)^\s*#\s?", " "));
        Assert.Contains("THE PAGE'S SPINE HAD NO CITATION", notes, StringComparison.Ordinal);
        Assert.Contains("NO SEIZURE NUMBER", notes, StringComparison.Ordinal);
        Assert.Contains("were preventable by adequate patient selection", notes, StringComparison.Ordinal);
        Assert.Contains("NO BETTER-THAN CLAIM", notes, StringComparison.Ordinal);
        Assert.Contains("one small randomized controlled trial showing opposite results", notes, StringComparison.Ordinal);
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

        // The claims rest on these: NBTS for what it is like at patient level,
        // StatPearls and PMC for the procedure and its risks, thejns.org for
        // the failed-awake series, RadiologyInfo for what a functional MRI is.
        foreach (var domain in new[] { "braintumor.org", "ncbi.nlm.nih.gov", "thejns.org", "radiologyinfo.org" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheHubDoorsAreAppendedSentencesThatKeepTheScope()
    {
        // §12.8 (WI-519): a new library page nothing links to is half shipped,
        // and hub additions are APPENDED sentences so no hub's own assertions
        // can break. Each door carries the reason in the same sentence — "if the
        // tumor is close to ... speech or movement" — so no glioma reader reads
        // "you may be awake" as a plan for everybody.
        foreach (var hub in new[]
                 {
                     "glioma", "low-grade-glioma", "high-grade-glioma", "astrocytoma", "oligodendroglioma", "glioblastoma",
                 })
        {
            var text = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", hub + ".md")));
            var door = CuratedPage.SentencesOf(text).SingleOrDefault(s => s.Contains("you may be awake", StringComparison.Ordinal));
            Assert.True(door is not null, $"/tumors/{hub} has lost its door to this page");
            Assert.StartsWith("If the tumor is close to the parts that handle speech or movement", door, StringComparison.Ordinal);
            Assert.Contains("/treatments/awake-craniotomy", text, StringComparison.Ordinal);

            // APPENDED means the bullet ends there: the next line is the next
            // bullet. /review added "Most glioma operations are done this way."
            // after the door, and the scope check on the door sentence itself
            // could not see it.
            var raw = CuratedPage.Read("tumors", hub + ".md");
            Assert.Matches(new Regex(@"\(/treatments/awake-craniotomy\)\r?\n[ \t]+explains why\.[ \t]*\r?\n- "), raw);
        }
    }

    [Fact]
    public void TheCraniotomyPagesAwakeMentionIsWhereItsDoorHangs()
    {
        // /treatments/craniotomy has named awake craniotomy since WI-510, and
        // that paragraph is where its reader meets the idea. The door belongs
        // THERE, not only in "Where to go next" — the break harness deleted the
        // in-prose door and the render test stayed green on the footer link
        // alone. And the words "awake craniotomy" stay UNLINKED, because
        // linking a word turns its tooltip off and this is the entry's live
        // use (§12.8, WI-519).
        var cranio = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("treatments", "craniotomy.md")));
        var mention = cranio.IndexOf("done as an awake craniotomy.", StringComparison.Ordinal);
        Assert.True(mention >= 0, "/treatments/craniotomy no longer names awake craniotomy in prose");

        var paragraphEnd = cranio.IndexOf("While you are asleep, the surgeon", mention, StringComparison.Ordinal);
        Assert.True(paragraphEnd > mention);
        Assert.Contains("(/treatments/awake-craniotomy)", cranio[mention..paragraphEnd], StringComparison.Ordinal);
        Assert.DoesNotContain("[awake craniotomy]", cranio, StringComparison.OrdinalIgnoreCase);

        var next = CuratedPage.Section(CuratedPage.Read("treatments", "craniotomy.md"), "Where to go next");
        Assert.Contains("(/treatments/awake-craniotomy)", next, StringComparison.Ordinal);
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
public sealed class AwakeCraniotomyPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/awake-craniotomy";

    private readonly WebApplicationFactory<Program> _factory;

    public AwakeCraniotomyPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Awake brain surgery", html);
        Assert.Contains("That is the test working", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // The pages a reader is standing on when "part of it may be done with
        // you awake" is what they have just been told.
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/treatments/craniotomy", "/tumors/glioma", "/tumors/low-grade-glioma", "/tumors/high-grade-glioma",
                     "/tumors/astrocytoma", "/tumors/oligodendroglioma", "/tumors/glioblastoma",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/treatments/craniotomy", "/tests/getting-ready-for-surgery", "/tests/mri", "/tests/waiting-for-results",
            "/seizures/what-to-do", "/get-help-now");

    [Fact]
    public async Task TheDeepLinksIntoTheSiblingPagesLandOnRealAnchors() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page. This page
        // hands off five sections of /treatments/craniotomy by anchor.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheTermsThisPageDefinesAreSuppressedHereAndStillFireElsewhere()
    {
        // §12.8 (WI-509): this page defines "awake craniotomy", "brain mapping"
        // and "craniotomy", so their tooltips are noise here. WI-512's other
        // half first: the words are in the prose, so the suppression is not a
        // no-op.
        var body = CuratedPage.ReaderText(CuratedPage.Read("treatments", "awake-craniotomy.md"));
        Assert.Contains("awake craniotomy", body, StringComparison.Ordinal);
        Assert.Contains("brain\nmapping".Replace("\n", " "), CuratedPage.Flatten(body), StringComparison.Ordinal);

        var client = _factory.CreateClient();
        var html = await client.GetStringAsync(Url);
        Assert.DoesNotContain("def-awake-craniotomy", html);
        Assert.DoesNotContain("def-brain-mapping", html);

        // WI-519: a suppression must not switch off a term's only live use, and
        // linking a word turns its tooltip off. /treatments/craniotomy keeps
        // "awake craniotomy" in UNLINKED prose, and hangs its door to this page
        // on different words for exactly that reason. It also says "mapping"
        // twice, which is where the new brain-mapping entry is reachable
        // (§12.8, WI-519: grep the aliases, not the term).
        var cranio = await client.GetStringAsync("/treatments/craniotomy");
        Assert.Contains("def-awake-craniotomy", cranio);
        Assert.Contains("def-brain-mapping", cranio);
    }

    [Fact]
    public async Task TheAnesthesiologistTooltipFiresBecauseThisPageDoesNotDefineTheWord()
    {
        // The other direction: a word this page uses and does NOT define keeps
        // its tooltip here. It is the doctor the panic paragraph points at.
        Assert.Contains("def-anesthesiologist", await _factory.CreateClient().GetStringAsync(Url));
    }
}
