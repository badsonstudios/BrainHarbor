using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-521: T11, the scans that carry on after treatment. The tenth page under
/// the §12.8 LIBRARY template (the shared <see cref="CuratedPage"/> helpers live
/// in TestsLibraryPagesTests.cs), and the third page in the corpus, after
/// WI-506 and WI-510, to use all twelve slots.
///
/// The prose is reviewed, not tested. What is pinned here is the small set of
/// places where this page could leave a reader worse off than no page at all,
/// and on this page they are almost all NUMBERS:
///
///   * a pseudoprogression frequency, which three reachable sources give three
///     different answers to (28-66%, ~36%, 12.4%) because each counted a
///     different thing;
///   * a radiation-necrosis timing printed without the population it came from
///     (stereotactic radiosurgery, mostly metastases and meningiomas);
///   * a surveillance schedule, which would be wrong for most of this page's
///     readers and is a number a frightened person CAN act on (§12.8, WI-520);
///   * a scanxiety intervention presented as working, when the trials say none
///     of them did.
///
/// So the guards below are claim-shaped rather than string-shaped: a QUANTITY
/// next to a MEANING, checked per section. That is WI-520's lesson, where
/// thirteen of sixteen adversarial mutations went through a string-shaped suite.
/// </summary>
public sealed class FollowUpScansPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "follow-up-scans.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section as the reader meets it. <c>CuratedPage.ReaderText</c> must NOT
    /// be fed a section: it strips front matter by index, and on a string with
    /// none the index is -1, so it silently returns <c>section[3..]</c> — three
    /// characters off the front, quietly breaking any assertion about how a
    /// section OPENS. Recorded in §12.8 (WI-520) and repeated here because the
    /// comment did not stop it the first time. <c>Section</c> already flattens.
    /// </summary>
    private static string Reader(string section) => Regex.Replace(section, @"!%(.+?)%", "");

    /// <summary>
    /// A quantity, however this corpus happens to have written it. Every guard
    /// below that is about a NUMBER uses this rather than <c>\d</c>: §12.8
    /// (WI-511) records a page whose no-percentages test matched digits on a
    /// corpus that writes every number in words, so it could not see the number
    /// it was written for.
    ///
    /// Deliberately WITHOUT "a"/"an"/"first"/"second". Those are articles and
    /// ordinals doing ordinary English work ("a few weeks", "the first three
    /// months", "a second look"), and including them makes the guard fail
    /// correct prose — which §12.8 calls worse than no guard at all.
    /// </summary>
    private const string Quantity =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|half|third|quarter|dozen"
        // Added after /review walked the first version of the frequency and
        // schedule guards with "a few months", "several", "twice a year" and
        // "a couple of weeks". English counts without numerals more often than
        // it counts with them, and every one of those is a schedule or a
        // frequency in ordinary clothes.
        + @"|few|several|couple|once|twice|most|many)";

    /// <summary>
    /// The subset of <see cref="Quantity"/> that can express a PROPORTION.
    ///
    /// Separate from <see cref="Quantity"/> deliberately. The schedule guard
    /// needs "few", "several", "once" and "twice", because "every few months"
    /// and "twice a year" are schedules. The frequency guard must NOT have them,
    /// because "for most of them the worst part was not the scan" is a correct,
    /// sourced sentence about scanxiety and "most" is not a proportion anybody
    /// can act on. Sharing one list made the guard fail the page it was
    /// protecting, which §12.8 calls worse than no guard.
    /// </summary>
    private const string ProportionQuantity =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|half|third|quarter|dozen)";

    /// <summary>A unit of time, for the guards that have to tell a duration from a quantity.</summary>
    private const string TimeUnit = @"(?:day|week|month|year|hour|minute)s?";

    /// <summary>
    /// Word runs this page shares with another page ON PURPOSE, and which the
    /// restatement guard must therefore not flag.
    ///
    /// The list is deliberately tiny and every entry is a SAFETY CLAIM. §12.10's
    /// whole argument is that two pages saying the same thing at two different
    /// strengths is this project's worst repeat defect, so where a claim is
    /// load-bearing on more than one page the right answer is identical words,
    /// not a paraphrase. Everything else — mechanism, description, framing,
    /// reassurance — this page writes for itself.
    ///
    /// If this list grows past a handful, the prose has been copied rather than
    /// written and the fix is to write it, or to make it a shared block (§12.8).
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // The headline pseudoprogression claim. /tumors/high-grade-glioma and
        // /tumors/glioblastoma both carry it, both link here, and a softer or
        // stronger version on the canonical page would be the §12.10 defect.
        "A scan that looks worse does not always mean the tumor has grown.",

        // The scanxiety finding, which /tumors/astrocytoma, /tumors/glioma and
        // /tumors/high-grade-glioma all carry in these words. It is the one
        // sentence on this subject that changes what somebody does with their
        // week, and a paraphrase here would be the same claim at a second
        // strength — §12.10's defect — on the page the hubs point at.
        "For most people the hardest part is not the scan. It is the gap between the scan and the",

        // The standard "Where to go next" door, worded identically on every
        // library page on purpose, so a reader moving between them recognises
        // it. A shared link label is not a restatement (§12.8, WI-510).
        "Get help now if you need to talk to a person today.",

        // Same reason: the library pages all link the report page by the same
        // name, and "for the" is the natural word after it. Deliberate.
        "Your pathology report, part by part for the",

        // The three routes out of the twelve-week rule, which this page and
        // /tumors/high-grade-glioma now state identically ON PURPOSE. WI-521
        // added the first of them to the hub, which was listing only the two
        // harsher routes (outside the field, or a sample) and leaving out RANO
        // 2.0's actual mandatory-confirmation mechanism — the more reassuring
        // one. This is a rule a reader is told their team follows; two versions
        // of it is §12.10's defect.
        "another scan a few weeks later shows the same thing, or the new area sits outside "
        + "where the radiation was aimed, or a sample shows it.",
    ];

    /// <summary>
    /// <see cref="DeliberatelyShared"/> as shingles, built with the SAME
    /// tokenizer the check uses. Comparing a shingle against the raw sentence
    /// with <c>Contains</c> is backwards — the shingle is the shorter string —
    /// and that mistake made the allowance silently do nothing.
    /// </summary>
    private static readonly HashSet<string> AllowedShingles =
        [.. DeliberatelyShared.SelectMany(s => Shingles(s, 8))];

    [Fact]
    public void TheShortVersionCarriesTheThreeThingsAReaderCannotAffordToMiss()
    {
        // §12.3's number applied to a library page: readers consume 20 to 28% of
        // a page, so the summary IS the page for most of them. WI-520's blocker
        // was a summary that carried a permission without its caveat while the
        // section-scoped position test structurally could not see the summary.
        var shortVersion = Reader(Section("The short version"));

        // 1. The scan that looks worse and is not worse. This is the single most
        //    load-bearing sentence on the page: without it, a reader who sees
        //    "increased enhancement" on a portal report at week six concludes
        //    the treatment failed.
        Assert.Contains("does not always mean the tumor has grown", shortVersion, StringComparison.Ordinal);

        // 2. "Stable" is a band. The word is easy to misread on a follow-up
        //    report and the summary is where it has to be defused.
        Assert.Contains("inside an agreed band", shortVersion, StringComparison.Ordinal);

        // 3. The one piece of logistics the research actually supports.
        Assert.Contains("booked close together", shortVersion, StringComparison.Ordinal);

        // AND THE SUMMARY COVERS BOTH LOOK-ALIKES, not only the early one.
        //
        // The first version scoped the reassurance to "the first months after
        // radiation" — and the page then documents a SECOND look-alike arriving
        // months to years later. A reader eighteen months out whose scan has
        // changed reads the summary, which is where 20 to 28% of readers stop,
        // and concludes they are outside the window so it must be growth. The
        // page contradicted its own summary two screens down, and the test
        // written for the summary PINNED THE NARROW WORDING IN PLACE — WI-520's
        // defect, on the item that cites WI-520's defect.
        Assert.Contains("in the months after radiation and sometimes years later",
            shortVersion, StringComparison.Ordinal);

        // The explanation arrives with the claim rather than after it, and the
        // ordering is asserted on the SENTENCE that carries the claim, not on
        // first occurrences anywhere in the section: /review put a decoy
        // qualifier in an earlier sentence and the index comparison went green
        // while the claim itself stood unqualified.
        //
        // Index proximity rather than a sentence index, because the claim ends
        // "…has grown.**" and SentencesOf splits on terminal punctuation
        // FOLLOWED BY WHITESPACE — the closing bold markers mean the claim and
        // its explanation arrive as one chunk. Proximity is the property that
        // matters anyway: the explanation has to be the next thing the reader
        // meets, not merely somewhere in the summary.
        var claim = shortVersion.IndexOf("does not always mean the tumor has grown",
            StringComparison.Ordinal);
        var scoped = shortVersion.IndexOf("Treatment itself causes changes that look like growth",
            StringComparison.Ordinal);
        Assert.True(scoped > claim && scoped - claim < 60,
            "the looks-worse claim and the sentence that scopes it have drifted apart in the summary, "
            + "so the reader who stops here meets a flat promise about every follow-up scan");
    }

    [Fact]
    public void TheWorstPartIsTheGapAndItIsSaidWhereTheReaderWhoStopsEarlyWillSeeIt()
    {
        // The one finding both scanxiety reviews agree on, and the only one that
        // changes what somebody does. Asserted over the WHOLE reader text AND in
        // the summary, because §12.8 (WI-520) records that a section-scoped
        // position test cannot see the short version — and the short version is
        // where the documented majority stop.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        var shortVersion = Reader(Section("The short version"));

        Assert.Contains("gap between the scan", shortVersion, StringComparison.Ordinal);
        Assert.Contains("hardest part is not the scan", shortVersion, StringComparison.Ordinal);
        Assert.Contains("days between the scan and the answer", body, StringComparison.Ordinal);
    }

    [Fact]
    public void NoPseudoprogressionOrNecrosisFrequencyIsPublishedInAnyShape()
    {
        // THE RULING THIS PAGE EXISTS TO MAKE. The backlog asked for "10-30% of
        // glioblastoma patients"; the dossier says "20-30% ... range 12% to 64%";
        // the ASCO Post says "28% to 66%"; PMC10412732 says "approximately 36% in
        // a recent meta-analysis" and separately reports a biopsy series at
        // 12.4%. Five answers, none of them each other, because each counted a
        // different thing (radiological change, confirmed change, tissue-proven
        // change). §12.4 R2's reasoning applies exactly: the spread is about
        // what was counted, not about risk.
        //
        // The guard is SHAPE-based, not a list of banned numbers, because a list
        // of banned numbers is beaten by the next number somebody writes
        // (§12.8, WI-520). The shape of a frequency is A QUANTITY NEXT TO A
        // POPULATION, so that is what is banned, per sentence.
        //
        // An earlier version required every quantity in these sections to be a
        // DURATION, which is a stronger rule and a wrong one: it failed on "tell
        // the two apart" and on "does not automatically mean one thing", where
        // the number word is a pronoun rather than a count. Both are correct
        // English on a page that has to keep saying "these two things look the
        // same", and §12.8's most-repeated lesson is that a rule which fails a
        // correct page is worse than no rule. Widening the exclusion list until
        // it passed would have hollowed the guard out one idiom at a time.
        // RUN OVER THE WHOLE READER TEXT, not over the two sections that happen
        // to be about the subject. The first version looped those two headings
        // and /review put "It happens to about a third of patients after
        // chemoradiation" into THE SHORT VERSION, where nothing fired — which is
        // WI-520's own recorded lesson ("a section-scoped position test cannot
        // see the short version") repeated by the file that cites it three
        // times, on the section 20 to 28% of readers stop at.
        foreach (var sentence in CuratedPage.SentencesOf(CuratedPage.Flatten(CuratedPage.ReaderText(Page))))
        {
            foreach (Match match in Regex.Matches(sentence, $@"\b{ProportionQuantity}\b",
                         RegexOptions.IgnoreCase))
            {
                // FORWARD ONLY. A frequency always puts the population AFTER
                // the number — "a third of people", "one in three patients",
                // "36% of scans". Looking backwards as well flagged "a lot of
                // people read it as one on the drive home", where the
                // population belongs to a different clause entirely and the
                // number is a pronoun.
                var window = sentence[match.Index..Math.Min(sentence.Length, match.Index + 45)];

                Assert.False(
                    Regex.IsMatch(window,
                        // A population, with up to two words of padding between
                        // the preposition and the noun. /review beat the first
                        // version with "Half OF THE PEOPLE who…" and "nearly a
                        // third OF FIRST SCANS", both of which the strict
                        // `of (people|scans)` form walked straight past.
                        @"percent|per cent|%"
                        // ONE padding word, not two. "of the people who" and
                        // "of first scans" each need one; allowing two let
                        // "one of the things people most often meet" through as
                        // a frequency, which it is not.
                        + @"|\bof\s+(?:\w+\s+){0,1}(?:patients|people|them|cases|scans)\b"
                        + @"|\b(?:patients|people|cases)\b\s*(?:get|have|develop|show|see)"
                        // "one in three", the commonest way a frequency is
                        // spoken. The first version only knew "in every", "in
                        // ten" and "in a hundred".
                        // "N in M". The DENOMINATOR has to be a number — a bare
                        // `\w+` fired on "the one in the section above", which
                        // is a pronoun and a preposition rather than a rate.
                        // The NUMERATOR is any quantity: the break harness put
                        // "seven in ten people having scans" on the page and a
                        // numerator list of one/two/three could not see it.
                        + $@"|\b{ProportionQuantity}\s+in\s+"
                        + @"(?:\d+|two|three|four|five|six|seven|eight|nine|ten|twenty|"
                        + @"a hundred|every)\b"
                        + @"|\bin (?:every|ten|a hundred)\b|\bout of\b|\bin\s+\d",
                        RegexOptions.IgnoreCase),
                    $"the page now prints a frequency: \"{window.Trim()}\". Three reachable sources give "
                    + "three different answers for how common pseudoprogression is (28-66%, ~36%, 12.4%) "
                    + "because each counted a different thing, so no figure goes on the page (§12.4 R2). "
                    + "Say it is common and say why there is no number.");
            }

        }

        // The bare quantifier forms, which have no number in them at all and
        // which the window above therefore cannot reach. Generalised from eight
        // literal phrases after /review beat every one of them by dropping or
        // changing a single word ("most people HAVING", "Half OF THE people
        // who").
        //
        // SCOPED BY SECTION rather than by keyword. The scanxiety section says
        // "close to universal" and says it correctly, from a source that says
        // "nearly universal" — so a blanket ban would fail the correct page
        // (§12.8). These are the four places a LOOK-ALIKE frequency could
        // land: the summary, the two look-alike sections, and the repeat-scan
        // section that explains them.
        foreach (var heading in new[]
                 {
                     "The short version",
                     "What if the scan looks worse?",
                     "What if something shows up much later?",
                     "Why do they want to scan me again so soon?",
                 })
        {
            foreach (var sentence in CuratedPage.SentencesOf(Reader(Section(heading))))
            {
                // Per sentence, and only where the sentence is about a
                // look-alike. The summary legitimately says "for most people
                // the hardest part is not the scan" — that is the scanxiety
                // finding, it is sourced, and every glioma hub says it too.
                if (!Regex.IsMatch(sentence,
                        @"pseudoprogression|necrosis|looks? worse|treatment effect|grow(n|th)|"
                        + @"chemoradiation|radiation",
                        RegexOptions.IgnoreCase))
                {
                    continue;
                }

                Assert.False(
                    Regex.IsMatch(sentence,
                        @"\b(?:most|many|nearly all|almost all|the majority|a minority)\s+(?:\w+\s+){0,1}"
                        + @"(?:patients|people)\b",
                        RegexOptions.IgnoreCase),
                    $"'{heading}' now quantifies a look-alike without using a number, which is the "
                    + $"same claim in different clothes: \"{sentence}\"");
            }
        }

        // And the page says so, rather than being quietly silent. A page that
        // simply omits the frequency leaves a reader to supply their own, and
        // the number they supply is usually worse than the truth.
        var looksWorse = Reader(Section("What if the scan looks worse?"));
        Assert.Contains("**It is common.**", looksWorse, StringComparison.Ordinal);
        Assert.Contains("no reliable figure to put here", looksWorse, StringComparison.Ordinal);
        Assert.Contains("counted it in different ways", looksWorse, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageSaysWhatAScanCannotDo()
    {
        // §12.8's first WI-506 rule, and the end-to-end read is what found it
        // missing: a library page must say what it cannot do. The MRI page says
        // a scan can suggest a tumor type and only tissue can name it. The thing
        // a FOLLOW-UP scan cannot do is see what is too small to measure — which
        // is why the scans carry on after an operation that went well, and why a
        // report with nothing on it is not a discharge.
        //
        // A reader without this sentence has two ways to go wrong and both are
        // live: reading a clear scan as cure, or reading continued scanning as
        // their team knowing something they are not saying.
        var why = Reader(Section("Why do the scans carry on?"));

        Assert.Contains("There is one thing a scan cannot do", why, StringComparison.Ordinal);
        Assert.Contains("what is big enough to see", why, StringComparison.Ordinal);
        Assert.Contains("not the same as saying nothing is there", why, StringComparison.Ordinal);

        // BOTH halves. §12.8 (WI-506): answer the frightening question in both
        // directions — dropping either one is a different kind of dishonesty.
        // Without the first clause this is a page telling somebody their clear
        // scan means nothing; without the second it is a page promising cure.
        Assert.Contains("real and welcome thing", why, StringComparison.Ordinal);
        Assert.Contains("not a sign that your team is holding something back",
            why, StringComparison.Ordinal);

        // AND THE SECTION ENDS THERE. Four true sentences and a fifth that
        // cancels them is the shape /review used against the first version of
        // this test, which was four bare Contains and no position at all:
        // "In practice, when the surgeon gets it all and the first scan
        // afterward is clear, that is usually the end of it." went through
        // green, and the section then closed on the over-reassuring sentence
        // rather than on the honest one. §12.12: the reassuring direction is
        // the dangerous one.
        var last = CuratedPage.SentencesOf(why)[^1];
        Assert.Matches(new Regex(@"holding something back\.?\s*$"), last);
    }

    [Fact]
    public void ProgressionIsNotLeftAsADeadEnd()
    {
        // Found by reading the page end to end, and invisible to everything
        // else. The report-words section defined "progression" as a measured
        // band and then moved on — so the one reader most likely to have
        // searched their way to this page, the one holding a report with that
        // word on it, reached the bottom with nowhere to go.
        //
        // §12.6: never end a frightening block without a concrete action. The
        // page does not itself say what the options are, because the hubs do
        // that properly and restating them here would be the §12.8 duplication
        // trap. What it owes is the door.
        var words = Reader(Section("What the words on the report mean"));

        Assert.Contains("it is not the end of the plan", words, StringComparison.Ordinal);
        Assert.Contains("/tumors", words, StringComparison.Ordinal);

        // And the routing sits with the definition rather than three screens
        // below it, where the reader who stopped at the frightening word never
        // reaches it.
        var definition = words.IndexOf("**Progression** means", StringComparison.Ordinal);
        var door = words.IndexOf("it is not the end of the plan", StringComparison.Ordinal);
        Assert.True(definition > 0 && door > definition && door - definition < 300,
            "the progression definition and the door out of it have drifted apart");
    }

    [Fact]
    public void NoSurveillanceScheduleIsPublishedAnywhereOnThePage()
    {
        // §10.1's meningioma schedule (MRI at 3, 6 and 12 months, then every
        // 6-12 months for 5 years) rests on academic.oup.com, 403 and on this
        // project's known-dead list since WI-514. Its high-grade figure the
        // dossier itself calls "convention rather than a strong evidence base",
        // and its 7.4-week interval comes from a modelling study the dossier
        // calls "explicitly a research-derived schedule, not official guidance".
        //
        // This page serves every tumor type, so one schedule is wrong for most
        // of its readers — and WI-520's ruling is the one that decides it: ask
        // which way a number pushes if the reader acts on it alone. "Every three
        // months" pushes a reader whose team scans them every six into believing
        // they are being under-watched, on a page they were sent to for
        // reassurance.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        // Every shape below except the first was added after /review published
        // §10.1's meningioma schedule and §10.1's high-grade convention through
        // the first version, which only knew the literal words "every N months".
        // A schedule is an INTERVAL, and English has at least four ways to write
        // one without ever saying "every".
        foreach (var pattern in new[]
                 {
                     // "every three months", "every 6 to 12 months", "every few months"
                     $@"every\s+{Quantity}\s+(to\s+{Quantity}\s+)?{TimeUnit}",
                     $@"{Quantity}[- ]monthly",
                     // "three months apart", "a few weeks apart"
                     $@"{Quantity}\s+{TimeUnit}\s+apart",
                     // "twice a year", "once every year", "three times a year"
                     $@"\b(?:once|twice|{Quantity}\s+times)\s+(?:a|each|every|per)\s+{TimeUnit}",
                     $@"\b(?:yearly|annually|half[- ]yearly|quarterly)\b",
                     // "at three months, six months and twelve months" — the
                     // meningioma schedule, in words rather than numerals, and
                     // with a unit permitted after each number.
                     $@"at\s+{Quantity}(\s+{TimeUnit})?,\s*{Quantity}(\s+{TimeUnit})?\s+and\s+{Quantity}\s+{TimeUnit}",
                     // "scanned every six months", the narrow form. A looser
                     // `scan .. {Quantity} {TimeUnit}` was tried and REJECTED:
                     // it fired on "a scan about four weeks after the last
                     // session", which is the RANO baseline — an OFFSET FROM AN
                     // EVENT, not an interval, and an R1 duration this page is
                     // allowed to print.
                     $@"scan(ned|s)?\s+(?:you\s+)?every\s+{Quantity}",
                 })
        {
            Assert.False(Regex.IsMatch(body, pattern, RegexOptions.IgnoreCase),
                $"the page now publishes a scan interval (\"{Regex.Match(body, pattern, RegexOptions.IgnoreCase).Value}\"). "
                + "It serves every tumor type, so any single schedule is wrong for most of its readers.");
        }

        // And it says so, rather than being quietly silent about it. A page that
        // simply omits the schedule leaves the reader to conclude nobody knows.
        var howOften = Reader(Section("How often, and how long until I hear?"));
        Assert.Contains("There is no single schedule", howOften, StringComparison.Ordinal);
        Assert.Contains("ask for yours", howOften, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheTwelveWeekRuleIsPrintedWithWhatItMeansForTheReader()
    {
        // RANO 2.0, verbatim: "Mandatory confirmation scans after 4 or 8 weeks
        // for new enhancement within 12 weeks post-treatment if the patient is
        // clinically stable."
        //
        // The rule on its own is a piece of trivia. What makes it useful is the
        // consequence — your team saying "let us scan again" is the rule working,
        // not your team stalling — and a reader who does not have that reads the
        // delay as being managed rather than treated.
        var section = Reader(Section("What if the scan looks worse?"));

        Assert.Contains("twelve weeks", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("a repeat scan is often the answer", section, StringComparison.OrdinalIgnoreCase);

        // The consequence sits with the rule, not three screens from it.
        var consequence = section.IndexOf("a repeat scan is often the answer", StringComparison.OrdinalIgnoreCase);
        var rule = section.IndexOf("twelve weeks", StringComparison.OrdinalIgnoreCase);
        Assert.True(Math.Abs(consequence - rule) < 400,
            "the twelve-week rule and what it means for the reader have drifted apart");

        // §12.6, and §12.8's WI-510 form of it: ending on the reassurance is a
        // POSITIONAL property, so pin the LAST sentence rather than proving the
        // reassurance is somewhere nearby. This section is the most frightening
        // one on the page and it must not close on the fear.
        //
        // ANCHORED TO THE END of the last sentence, not merely present in it.
        // SentencesOf splits on terminal punctuation, so /review appended
        // ", but if the next scan shows the same thing, it is usually growth"
        // to the closing sentence and the pinned words were still inside the
        // last "sentence" — the section now ended on the fear and the guard
        // written to stop that said nothing.
        var last = CuratedPage.SentencesOf(section)[^1];
        Assert.Matches(new Regex(@"something you can plan around\.?\s*$"), last);
    }

    [Fact]
    public void TheExtraScansAreNotSoldAsAbleToSettleTheQuestion()
    {
        // Two sources disagree and §12.8 (WI-511) says print what both support.
        // The ASCO Post: perfusion, spectroscopy and FDG-PET "cannot reliably
        // distinguish tumor progression from pseudoprogression". PMC10412732,
        // eleven years later: DSC MRI at 84% sensitivity and 78% specificity,
        // and PET better than that — but with tissue confirmation still the
        // "gold standard". Splitting the difference with an unattributed
        // "usually tells them apart" would be a third answer belonging to
        // nobody, and it is the answer that hurts: a reader promised a test
        // that settles it experiences the repeat scan as their team dithering.
        var section = Reader(Section("What if the scan looks worse?"));

        Assert.Contains("Those help, and they do not settle it", section, StringComparison.Ordinal);
        Assert.Contains("a sample of tissue, or time", section, StringComparison.Ordinal);

        // Claim-shaped: no sentence may say a scan decides it.
        //
        // THE NEGATION IS ANCHORED TO THE CLAUSE, not to the sentence. The first
        // version excluded any sentence containing "not" anywhere, and /review
        // wrote "A perfusion scan is not always needed, and it tells your team
        // which of the two it is" — one "not", belonging to the other clause,
        // bought the promise a free pass. That is §12.8 (WI-511) in as many
        // words, and CuratedPage.AssertNeverMinimises already carries the fix.
        //
        // The verb and object sets are also wider than the first version's two.
        // /review beat it with "identifies the difference" and "shows the
        // difference" while `Assert.Contains("they do not settle it")` above
        // stayed green — the §12.14 shape, where the test's name does the
        // reassuring.
        var promise = new Regex(
            @"\b(tells?|shows?|proves?|confirms?|settles?|answers?|decides?|identifies|identify|"
            + @"sorts?|separates?|rules? out|distinguishe?s?|gives? the answer)\b"
            + @"[^.,;:]{0,40}\b(which|whether|the two|them apart|the difference|the answer|"
            + @"what it is|for (?:sure|certain))\b",
            RegexOptions.IgnoreCase);

        foreach (var sentence in CuratedPage.SentencesOf(section))
        {
            foreach (Match match in promise.Matches(sentence))
            {
                var before = sentence[Math.Max(0, match.Index - 40)..match.Index];

                Assert.True(
                    Regex.IsMatch(before, @"\b(not|never|cannot|can't|hardly|no|n't|far from)\b[^.,;:]{0,20}$",
                        RegexOptions.IgnoreCase),
                    $"this sentence promises a scan can settle pseudoprogression: \"{sentence}\". "
                    + "Two sources disagree about how well the extra scans do, and both agree the "
                    + "certainty comes from tissue or from time (§12.8, WI-511).");
            }
        }
    }

    [Fact]
    public void TheRadiationNecrosisTimingCarriesTheScopeItsSourceAttachesToIt()
    {
        // §12.8 (WI-520): print the scope the source attaches to a rule, not
        // just the rule. PMC8507553's 15.7% and its median-8-months are 388
        // patients treated with STEREOTACTIC RADIOSURGERY — 15.8% for brain
        // metastases, 34.6% for meningiomas, 4.8% for vestibular schwannomas.
        // A reader on six weeks of fractionated chemoradiation for a glioma is
        // not in that population, and this page is read by far more of them.
        //
        // The dossier also gets this paper's own numbers wrong: it says median
        // 7 months, range 1-25. The paper says 8 months, range 1-41.
        var section = Reader(Section("What if something shows up much later?"));

        Assert.Contains("stereotactic radiosurgery", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("one kind of radiation, and it may not be yours",
            section, StringComparison.Ordinal);

        // THE FIGURE APPEARS EXACTLY ONCE, and the scope is in ITS OWN SENTENCE.
        //
        // The first version compared IndexOf("stereotactic radiosurgery") with
        // IndexOf("eight months"), which proves the ordering of FIRST
        // occurrences and nothing else. /review appended "Around eight months
        // after radiation is when it usually turns up." to the end of the
        // section: both indexes unchanged, test green, and the SRS-only figure
        // generalised to everybody in the sentence after the one saying it is
        // not a rule for everybody. §12.8 (WI-520) in a new coat.
        Assert.Equal(1, Regex.Matches(section, @"eight months", RegexOptions.IgnoreCase).Count);

        var figureSentence = CuratedPage.SentencesOf(section)
            .Single(s => s.Contains("eight months", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("In that group", figureSentence, StringComparison.Ordinal);

        // And the population is named BEFORE the sentence that prints it, or the
        // reader has already taken the number as theirs by the time the
        // qualifier turns up (§12.8, WI-507).
        var scope = section.IndexOf("stereotactic radiosurgery", StringComparison.OrdinalIgnoreCase);
        var figure = section.IndexOf("eight months", StringComparison.OrdinalIgnoreCase);
        Assert.True(scope < figure && figure - scope < 300,
            "the radiation-necrosis figure and the population it came from have drifted apart");

        // The dossier's wrong numbers, banned by value as well as by shape,
        // because these are the ones a future edit would reach for.
        Assert.DoesNotContain("seven months", section, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"\btwenty[- ]five\b", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheBiopsyClaimTheDossierInventedIsNotOnThePage()
    {
        // §10.4 says "in one small biopsy series imaging diagnosis matched
        // biopsy in all cases assessed", attributed to PMC8507553. That paper
        // states biopsies were NEVER PERFORMED, and that "histological proof is
        // not mandatory in international guidelines". It is the strongest
        // reassurance in the dossier's whole §10 and it rests on nothing.
        //
        // §12.14: banning the source would not have removed the claim, so the
        // claim is grepped for directly.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var phrase in new[]
                 {
                     "matched biopsy", "agreed with biopsy", "matched biopsy in all cases",
                     "always right", "never wrong", "imaging alone can tell",
                 })
        {
            Assert.DoesNotContain(phrase, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void StableIsExplainedAsABandRatherThanAsNoChange()
    {
        // The most misread word on a follow-up report, and the reason RANO is on
        // this page at all. RANO 2.0 sets stable disease as everything between
        // the response threshold and the progression threshold, so a tumor that
        // shrank a little and one that grew a little both land there. A reader
        // told "stable means nothing changed" and then told at the next
        // appointment that it had been creeping is a reader who stops believing
        // the reports.
        var section = Reader(Section("What the words on the report mean"));

        Assert.Contains("Stable does not mean identical", section, StringComparison.Ordinal);

        // BOTH directions, in the same section. Half of this is a different and
        // more comforting claim than the whole of it.
        Assert.Contains("too small to count as shrinking", section, StringComparison.Ordinal);
        Assert.Contains("too small to count as growing", section, StringComparison.Ordinal);
        Assert.Contains("got a little smaller", section, StringComparison.Ordinal);
        Assert.Contains("got a little bigger", section, StringComparison.Ordinal);

        // And it does not end on the deflation. §12.6: never end a section on a
        // frightening sentence, follow it with something the reader can do.
        // Anchored to the END of the last sentence: /review appended ", though
        // the number underneath it can still be moving" and the pinned word was
        // still inside the final sentence while the section now closed on the
        // deflation.
        var last = CuratedPage.SentencesOf(section)[^1];
        Assert.Matches(new Regex(@"the measurement it came from\.?\s*$"), last);
    }

    [Fact]
    public void RanoIsPresentedAsAResearchMeasuringStickAndNotAsARuleAboutTheReader()
    {
        // PMC12479667, verbatim: "While that treatment response criteria are
        // primarily intended for clinical trials..." That admission is the whole
        // reason the page names RANO rather than hiding it. Without it, a reader
        // who finds the thresholds online applies a trial measuring stick to
        // their own report and concludes something nobody on their team has.
        var section = Reader(Section("What the words on the report mean"));

        Assert.Contains("RANO", section, StringComparison.Ordinal);
        Assert.Contains("built for research studies", section, StringComparison.Ordinal);
        Assert.Contains("primarily intended for clinical trials", section, StringComparison.Ordinal);
        Assert.Contains("not a set of instructions about you", section, StringComparison.Ordinal);

        // No threshold is published. They differ between the two-dimensional and
        // the volume methods, they moved between RANO and RANO 2.0, and a reader
        // who applies one to their own report gets it wrong. The dossier also
        // calls this "four categories" when the paper's table has five.
        //
        // The first version banned a quantity next to
        // `percent|reduction|increase|volume|diameter`, and /review wrote
        // "**Response** means it shrank by at least half. **Progression** means
        // it grew by at least a quarter." — a RANO threshold, in words, and a
        // wrong one, on the page whose front matter argues at length that no
        // threshold may be printed.
        //
        // Requiring every quantity here to be a duration was tried next and
        // REJECTED, because it fails the correct page: this section has to be
        // able to say "a tumor that got a little smaller and ONE that got a
        // little bigger", "TWO things about those rules", "ask ONE question".
        // Every one of those is a pronoun or a determiner, and widening an
        // exclusion list until they pass is how a guard gets hollowed out.
        //
        // What a threshold actually looks like is A MAGNITUDE PREPOSITION next
        // to a quantity, or a quantity next to a proportion. Neither shape has
        // any innocent use in this section, and both catch the mutation.
        foreach (var shape in new[]
                 {
                     $@"\b(?:by|at least|more than|over|under|less than|about|around|roughly)\s+"
                     + $@"(?:an?\s+)?{Quantity}\b",
                     $@"\b{Quantity}\s*(?:percent|per cent|%)",
                 })
        {
            Assert.False(Regex.IsMatch(section, shape, RegexOptions.IgnoreCase),
                $"the report-words section now prints a RANO threshold "
                + $"(\"{Regex.Match(section, shape, RegexOptions.IgnoreCase).Value}\"). The cut-offs are "
                + "measurement rules for trials, they differ between the two-dimensional and the volume "
                + "methods, and a reader who applies one to their own report will get it wrong.");
        }
    }

    [Fact]
    public void TheScanxietySectionPromisesNothingAndGivesLogistics()
    {
        // WI-514 established the position and this page is its canonical home:
        // the peak is scan-TO-RESULT, and NO intervention has been shown to work.
        // PMC11659363, verbatim on five randomized trials: "none led to reduced
        // rates of scanxiety", and "Intensified surveillance also was shown to
        // have no significant impact on anxiety compared with usual care."
        //
        // A page that offers a breathing technique here is not being kind. It is
        // telling somebody the thing that did not work for anybody is the thing
        // they failed at.
        var section = Reader(Section("The waiting, and what actually helps"));

        Assert.Contains("None of them reduced", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("five trials tested", section, StringComparison.Ordinal);
        Assert.Contains("not going to promise you a technique", section, StringComparison.Ordinal);

        // The other half of the finding, which is the one a reader is most
        // likely to act against: asking for more frequent scans.
        Assert.Contains("Scanning people more often did not help", section, StringComparison.Ordinal);

        // The advice that IS given is scheduling, which is what both reviews
        // point at and the only part anybody can change.
        Assert.Contains("booked close together", section, StringComparison.Ordinal);

        // Claim-shaped: nothing in this section may promise an effect.
        //
        // THE NEGATION IS ANCHORED TO THE CLAUSE. The first version skipped any
        // sentence containing "not" or "no" anywhere, so /review wrote
        // "Counselling before a scan will not cure it, but it does ease the
        // worry" and it passed — while "**None of them reduced it.**" two
        // paragraphs up stayed asserted. §12.8 (WI-511), and the fix already
        // exists in CuratedPage.AssertNeverMinimises.
        //
        // The verb set is wider too: /review beat the first one with "calms the
        // waiting" and "help many people through the waiting days", neither of
        // which is a word the original list contained.
        var promisesRelief = new Regex(
            @"\b(reduces?|eases?|relieves?|cures?|prevents?|fixes?|helps?|calms?|settles?|soothes?|"
            + @"lessens?|proven|shown to (?:work|help)|takes the edge off|makes?)\b"
            + @"[^.,;:]{0,45}\b(anxiety|scanxiety|worry|worrying|fear|stress|wait|waiting|days|it)\b",
            RegexOptions.IgnoreCase);

        foreach (var sentence in CuratedPage.SentencesOf(section))
        {
            foreach (Match match in promisesRelief.Matches(sentence))
            {
                var before = sentence[Math.Max(0, match.Index - 40)..match.Index];

                Assert.True(
                    Regex.IsMatch(before,
                        // "nobody"/"nothing" added: the page's own honest
                        // sentence is "Nobody has shown that closing that gap
                        // fixes the waiting", and a negation list without them
                        // fails the correct page.
                        @"\b(not|never|none|nobody|nothing|cannot|can't|hardly|no|n't|far from)\b"
                        + @"[^.,;:]{0,45}$", RegexOptions.IgnoreCase),
                    $"this sentence promises an intervention works, and five trials say none did: "
                    + $"\"{sentence}\"");
            }
        }
    }

    [Fact]
    public void TheNearUniversalClaimKeepsTheScopeOfTheStudyItCameFrom()
    {
        // §12.8 (WI-520), the harder half. PMC11659363 is titled
        // "Surveillance-Associated Anxiety After CURATIVE-INTENT CANCER
        // SURGERY". Its 34-85% and its "nearly universal" belong to that
        // population, not to brain-tumor follow-up — and the dossier's own
        // figures ("over 70%", "71.2% in one prospective longitudinal study")
        // are in NEITHER open source.
        //
        // So the page says near-universal, scopes it to cancer follow-up rather
        // than to brain tumors, and publishes no number.
        var section = Reader(Section("The waiting, and what actually helps"));

        Assert.Contains("after cancer surgery", section, StringComparison.Ordinal);

        foreach (Match match in Regex.Matches(section, $@"\b{ProportionQuantity}\b",
                     RegexOptions.IgnoreCase))
        {
            var context = section[match.Index..Math.Min(section.Length, match.Index + 45)];
            Assert.False(
                Regex.IsMatch(context,
                    @"percent|%|per cent|of (?:patients|people|them)|in every"
                    // "seven in ten people having scans" went through the first
                    // version of this guard AND through the corpus-wide one,
                    // because neither knew the "N in M" shape with a numerator
                    // above three. The dossier's own unsupported figure is
                    // "over 70%", which is what that sentence is.
                    + @"|\bin\s+(?:\d+|ten|twenty|a hundred)\b",
                    RegexOptions.IgnoreCase),
                $"the waiting section now publishes a scanxiety frequency: \"{context.Trim()}\"");
        }
    }

    [Fact]
    public void ThePageDoesNotContradictTheHubsOnPseudoprogression()
    {
        // Three hubs already carry pseudoprogression in short form and this page
        // is the fuller version they now point at. §12.10's rule: READ the
        // sibling rather than hard-coding what it is believed to say, so a change
        // there goes red here instead of quietly diverging.
        var section = Reader(Section("What if the scan looks worse?"));

        foreach (var hub in new[] { "high-grade-glioma", "glioblastoma" })
        {
            var hubText = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", hub + ".md")));

            Assert.True(
                Regex.IsMatch(hubText, @"first three months", RegexOptions.IgnoreCase),
                $"/tumors/{hub} no longer says pseudoprogression shows up in about the first three "
                + "months, so the window this page agrees with it on cannot be checked");

            // And the hubs point HERE, so the two are one story rather than two.
            Assert.Contains("/tests/follow-up-scans", hubText, StringComparison.Ordinal);
        }

        Assert.True(
            Regex.IsMatch(section, @"first three months", RegexOptions.IgnoreCase),
            "this page now gives a different pseudoprogression window from the hubs that link to it");

        // The hubs say it can cause real symptoms; the canonical page cannot say
        // less than they do. "It is only a picture problem" is the reassuring
        // direction §12.12 calls the more dangerous one.
        Assert.Contains("not only a problem on the screen", section, StringComparison.Ordinal);
        Assert.Contains("Those symptoms are treatable", section, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatTheSiblingPagesAlreadyOwn()
    {
        // §12.8's threshold in the other direction. /tests/mri owns the machine,
        // the radiologist and the tumor board; /tests/waiting-for-results owns
        // the laboratory pipeline and the whole Cures Act portal decision. This
        // page routes to both and must not re-tell either, or a reader meeting
        // all three gets three slightly different versions and believes the
        // differences are meaningful.
        //
        // Word SHINGLES, not a vocabulary ban. §12.8 (WI-519) records a
        // five-noun ban that /review walked around with a genuine restatement
        // using none of them, and the lesson was that the shingle version was
        // already written in three sibling test files. Copied, not re-derived.
        // RUN OVER THE WHOLE CORPUS, not over three hand-picked tests pages.
        //
        // The first version named `mri`, `waiting-for-results` and
        // `pathology-report` and reported zero — while this page shared **108**
        // eight-word shingles with /tumors/high-grade-glioma, whole paragraphs
        // of pseudoprogression and radiation-necrosis prose lifted wholesale.
        // The two copies had already drifted apart inside a single commit
        // ("dead tissue" here, "damaged tissue" there), which is exactly what
        // §12.8 (WI-510) says a shingle check exists to catch. A guard that
        // checks the siblings you thought of is a guard that finds nothing.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("follow-up-scans.md", StringComparison.Ordinal));

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
                + "\n\nEither write this page's own version, or — if the two really are meant to say "
                + "the identical thing — add the sentence to DeliberatelyShared with the reason.");
        }

        // And the portal decision in particular is handed off rather than
        // re-argued, because /tests/waiting-for-results is where the Cures Act,
        // the specialist survey and the "ask whether release can be held" ask
        // all live.
        var waiting = Reader(Section("The waiting, and what actually helps"));
        Assert.Contains("/tests/waiting-for-results", waiting, StringComparison.Ordinal);

        foreach (var phrase in new[] { "April 2021", "the law says", "released to you as soon" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.ReaderText(Page), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageCarriesNoEscalationListAndAsksTheQuestionInstead()
    {
        // A deliberate absence. §12.8 (WI-511, WI-512, WI-519) makes every
        // escalation list on the site a liability that has to be diffed per
        // symptom, in both directions, against every other one — and what a
        // reader should call about between scans is TUMOR-SPECIFIC. A generic
        // list here would be the under-triage direction for one reader and the
        // over-triage direction for the next.
        //
        // So the page asks the question and routes, which is WI-519's answer and
        // WI-520's. If a later item adds a list, this fails and the tier diff
        // becomes that item's job rather than something nobody noticed.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var phrase in new[]
                 {
                     "call an ambulance", "these cannot wait", "straight away",
                     "as soon as possible if", "emergency room", "911",
                     // "the same day IF" was the only form the first version
                     // knew; /review wrote "the same day FOR any of these" and
                     // sailed through. The preposition is not the claim.
                     "the same day if", "the same day for", "the same day about",
                     "right away", "urgent care", "go to the er", "do not wait",
                 })
        {
            Assert.DoesNotContain(phrase, body, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE STRING LIST IS NOT THE CHECK. §12.8 (WI-520): /review headed a
        // brand-new undiffed escalation list "**Phone the hospital now if any of
        // these happen:**" and a suite with four banned strings stayed green,
        // because there are dozens of ways to write a heading. What makes it an
        // escalation list is the SHAPE — an urgency-flavoured lead-in
        // introducing a run of symptom bullets. Read on the raw markdown, where
        // the bullets are still bullets.
        //
        // The first version was ONE regex requiring a `**bold**` lead-in
        // immediately followed by three or more bullets, and /review beat it
        // three separate ways: with a `###` heading instead of the bold, with an
        // ordinary sentence sitting between the lead-in and the list, and with a
        // list of three bullets. Each mutation planted an undiffed list
        // containing a flat "a seizure", WI-510's blocker verbatim.
        //
        // Inverted, so the shape is found first and judged second: locate EVERY
        // run of two or more list items, then read what introduces it. That
        // cannot be walked around by changing the introduction's punctuation,
        // its markup or its length, because the bullets are what make it a list.
        //
        // The trigger words are URGENCY words only. "team", "doctor" and "nurse"
        // were tried and dropped: this page's own questions list sits under
        // "What to ask your team", and a guard that fails a correct page is
        // worse than no guard (§12.8).
        var urgency = new Regex(
            @"\b(now|today|immediately|at once|urgent(ly)?|ambulance|emergency|911|"
            + @"call|phone|ring|same day|straight away|do not wait|cannot wait)\b",
            RegexOptions.IgnoreCase);

        foreach (Match run in Regex.Matches(
                     Page, @"(?:^[ \t]*(?:[-*]|\d+\.)[ \t]+.+(?:\r?\n|$)){2,}", RegexOptions.Multiline))
        {
            // The lead-in is THE PARAGRAPH IMMEDIATELY BEFORE the list, not a
            // fixed character window. A hard-wrapped bullet breaks the run, so a
            // long list arrives here as several runs and a character window
            // reads the list's own earlier bullets as its introduction — which
            // is how the first version condemned this page's "What to ask your
            // team" list off its own "Do you call me either way?" question.
            var paragraphs = Regex.Split(Page[..run.Index], @"\r?\n[ \t]*\r?\n")
                .Where(p => p.Trim().Length > 0).ToList();
            var before = paragraphs.Count > 0 ? paragraphs[^1] : "";

            // Mid-list: this run is the tail of a list whose lead-in was already
            // judged on the previous iteration.
            if (Regex.IsMatch(before.TrimStart(), @"^(?:[-*]|\d+\.)\s"))
            {
                continue;
            }

            Assert.False(urgency.IsMatch(before),
                "this page now carries what looks like an escalation list of its own. What a reader "
                + "should call about between scans depends on their diagnosis, so every such list has "
                + "to be diffed per symptom, in both directions, against /treatments/craniotomy, "
                + "/tests/biopsy and /seizures/what-to-do (§12.8, WI-511/WI-512/WI-519). Do that, or "
                + "take the list out. The list:\n" + run.Value + "\nIntroduced by:\n" + before);
        }

        // The route, which is what makes the absence a decision rather than a
        // gap. Both halves: the reader is told they may bring a scan forward,
        // and is told to get the answer from the people who can give it.
        var howOften = Reader(Section("How often, and how long until I hear?"));
        Assert.Contains("you do not have to wait for the next one", howOften, StringComparison.Ordinal);

        var questions = Reader(Section("What to ask your team"));
        Assert.Contains("What should I call you about between scans?", questions, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoCaregiverSectionAndTheReasonIsRecorded()
    {
        // Contract item 12 puts a caregiver section on every TREATMENT page with
        // meaningful aftercare, and §12.8 says tests pages mostly do not carry
        // one. WI-519 is the exception that proves the shape of the rule: a
        // biopsy sends somebody home with a hole in their skull and a person who
        // has to watch them overnight.
        //
        // A follow-up scan has no aftercare tail at all. The [CAREGIVER] block
        // is built around the two phone numbers, being shown what you are sent
        // home to do, and what counts as an ambulance — none of which this page's
        // reader is facing. Its siblings /tests/mri and /tests/waiting-for-results
        // carry none either, which is the corpus check §12.8 asks for before a
        // per-page property is generalised.
        //
        // What the person alongside actually needs here is one line, and it sits
        // in slot 7 with the rest of what a reader can act on.
        Assert.DoesNotContain("[CAREGIVER]", Page, StringComparison.Ordinal);

        foreach (var sibling in new[] { "mri", "waiting-for-results" })
        {
            Assert.DoesNotContain("[CAREGIVER]", CuratedPage.Read("tests", sibling + ".md"),
                StringComparison.Ordinal);
        }

        var canDo = Reader(Section("What you can do around a scan"));
        Assert.Contains("Take somebody with you to the results appointment", canDo, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoPercentageAndNoPrognosisVocabulary()
    {
        // Contract item 5. Graded against reader text, so the WI-105 suppression
        // markers do not put characters into the check that no reader ever sees
        // (§12.8, WI-509).
        var body = CuratedPage.ReaderText(Page);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", body);

        foreach (var word in new[]
                 {
                     "survival", "life expectancy", "five-year", "prognosis", "how long you have",
                     // "percent" was missing from the first version while
                     // "per cent" was present, so "About thirty percent of
                     // people see it" was invisible to the digit regex above AND
                     // to this list. The word form is the one this corpus
                     // actually writes.
                     "per cent", "percent", "percentage",
                 })
        {
            Assert.DoesNotContain(word, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNeitherTheCharacterisationsNorTheMinimisations()
    {
        // This page is where the vocabulary arrives without anybody deciding:
        // "a good scan", "the news was good", "a routine scan". The last one is
        // the dangerous one, because a surveillance scan genuinely IS routine in
        // the scheduling sense and is not routine for the person having it.
        // Negation-aware (§12.8, WI-511).
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "tests/follow-up-scans");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }

        // Three more this page can reach that the shared list does not carry,
        // because they only have an obvious meaning on a page about results.
        // Run over the whole corpus first, per §12.8: no occurrence anywhere.
        foreach (var phrase in new[] { "good scan", "bad scan", "clean scan" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // WI-564 is still open. Whitespace-normalised first, because the corpus
        // is hard-wrapped and this gate's own history is of walking past
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

        // Two this page can reach that the shared list does not carry, because
        // they are the British words for the people and the place a follow-up
        // appointment involves. Corpus-clean, checked before adding (§12.8), and
        // kept page-local rather than promoted: §12.8 says promote at the SECOND
        // page that needs a rule, not the first.
        foreach (var form in new[] { "consultant", "registrar" })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8's warning: read the section for the slot list, not the last page
        // written, because three bent pages in a row is how a template quietly
        // shrinks. This is only the third page in the corpus to carry all twelve
        // slots, after WI-506 and WI-510.
        //
        // Slots 3, 4 and 7 are roles rather than headings, which is the WI-507
        // precedent. On a page whose subject is the RESULT rather than the
        // procedure, "what happens, step by step" is what happens to the
        // pictures, and "what you need first" is what you can arrange around a
        // scan. Slot 6c is three headings, one per genuinely-asked worry.
        //
        // The explicit `{#id}` is stripped before comparing, so this test is
        // about the words a reader sees and the anchor test below is about the
        // interface. Tying them together is what WI-509 wrote explicit anchors
        // to avoid.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

        Assert.Equal(
            [
                "The short version",                          // 0
                "What is a follow-up scan?",                  // 1
                "Why do the scans carry on?",                 // 2
                "What happens to the pictures",               // 3, as a role
                "How often, and how long until I hear?",      // 4, as a role
                "What is the day itself like?",               // 5
                "The waiting, and what actually helps",       // 6a
                "What the words on the report mean",          // 6c
                "What if the scan looks worse?",              // 6c
                "What if something shows up much later?",     // 6c
                "What you can do around a scan",              // 7, as a role
                "Why do they want to scan me again so soon?", // 8
                "Who reads it, and how do I get the result?", // 9
                "What to ask your team",                      // 10
                "Where to go next",                           // 11
            ],
            headings);
    }

    [Fact]
    public void TheAnchorsTheOtherPagesDeepLinkIntoArePinned()
    {
        // §12.8 (WI-508): heading anchors are a published interface. Markdig
        // derives an id from heading TEXT, so a reworded heading breaks every
        // inbound link silently — the reader lands at the top of a long page
        // with no sign anything went wrong. Two hubs deep-link #looks-worse.
        foreach (var anchor in new[]
                 {
                     "{#short-version}", "{#what-it-is}", "{#why}", "{#what-happens}", "{#how-often}",
                     "{#the-day}", "{#waiting}", "{#the-words}", "{#looks-worse}", "{#much-later}",
                     "{#what-you-can-do}", "{#again-so-soon}", "{#results}", "{#questions}",
                     "{#where-to-go-next}",
                 })
        {
            Assert.Contains(anchor, Page, StringComparison.Ordinal);
        }

        // Every `##` heading carries one, so a section added later cannot quietly
        // rely on a derived id.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline);
        Assert.All(headings, m => Assert.Matches(@"\{#[a-z0-9-]+\}\s*$", m.Groups[1].Value.Trim()));

        // And the siblings really do point at the one they were given, so this
        // fails on either side of the contract rather than only on ours.
        foreach (var hub in new[] { "glioblastoma", "oligodendroglioma" })
        {
            Assert.Contains("/tests/follow-up-scans#looks-worse",
                CuratedPage.Read("tumors", hub + ".md"), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone, and there is no outlook here. A
        // page about follow-up scans is exactly where an outlook section would
        // arrive by accident.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void TheDeadAndMisreadCitationsAreAbsentAndSoAreTheClaimsTheyCarried()
    {
        // §12.14, sixth item running: banning a URL is not fixing a claim, and
        // the ban PROVIDES FALSE ASSURANCE because a test naming a bad URL reads
        // like the claim was handled. So each ban below is paired with a grep for
        // what that URL was carrying.
        var front = CuratedPage.FrontMatter(Page);
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        // The known-dead domains. §10.2 cites two of them for RANO 2.0 and §10.1
        // cites a third for the whole surveillance rhythm.
        //
        // Checked against the extracted `- url:` values, NOT the raw front
        // matter: the comment block above them NAMES these domains, because
        // §12.13 says the resolution has to be written where the reader of the
        // front matter will see it. The first version of this test read the raw
        // text and failed on the explanation of why the URL is absent, which is
        // a rule failing a correct page (§12.8).
        var citedUrls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        foreach (var dead in new[] { "ascopubs.org", "academic.oup.com", "sciencedirect.com" })
        {
            Assert.DoesNotContain(citedUrls,
                u => u.Contains(dead, StringComparison.OrdinalIgnoreCase));
        }

        // §10.1's meningioma schedule was the only thing academic.oup.com was
        // carrying here, and NoSurveillanceScheduleIsPublished... proves it gone.
        // What this adds is the softer shape of the same claim.
        foreach (var claim in new[] { "for five years", "for the first three years", "as clinically indicated" })
        {
            Assert.DoesNotContain(claim, body, StringComparison.OrdinalIgnoreCase);
        }

        // §10.5's scanxiety figures came through sciencedirect and are in
        // NEITHER open source. The frequency guards above prove no number is
        // printed; this proves the specific inherited framing is gone too.
        foreach (var claim in new[] { "clinically significant", "longitudinal study" })
        {
            Assert.DoesNotContain(claim, body, StringComparison.OrdinalIgnoreCase);
        }

        // §12.1: NCI patient PDQ is fine for framing and is never the source for
        // naming or grading. Nothing here needs it.
        Assert.DoesNotContain("cancer.gov", front, StringComparison.OrdinalIgnoreCase);

        // And the front matter says WHY, so the next reader does not re-open it
        // (§12.13: write the resolution where the reader of the front matter
        // will see it).
        //
        // Flattened, and the `#` comment markers stripped, because the block is
        // hard-wrapped like everything else in this corpus — a two-line quote
        // has a newline and a "# " in the middle of it, which is §12.8's
        // (WI-511) "flatten before matching" one level up.
        var notes = CuratedPage.Flatten(Regex.Replace(front, @"(?m)^\s*#\s?", " "));

        Assert.Contains("28% to 66%", notes, StringComparison.Ordinal);
        Assert.Contains("median 8 months, range 1-41 months", notes, StringComparison.Ordinal);
        Assert.Contains("biopsies were NEVER PERFORMED", notes, StringComparison.Ordinal);
        Assert.Contains("CURATIVE-INTENT CANCER", notes, StringComparison.Ordinal);
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

        // The domains this page's claims actually rest on: PMC for RANO,
        // pseudoprogression, radiation necrosis and both scanxiety reviews; the
        // ASCO Post for the imaging limits at a readable level; ACS for what
        // follow-up is for at patient level.
        foreach (var domain in new[] { "ncbi.nlm.nih.gov", "ascopost.com", "cancer.org" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor: .NET's multiline `$` does not match before `\r`, and
        // this repo has core.autocrlf=true (WI-501).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    /// <summary>
    /// Word shingles, with markdown LINK TARGETS removed first.
    ///
    /// Without that, the tokenizer turns `/tests/mri#who-reads-my-scan-and-how-
    /// do-i-get-the-result` into eleven words and reports this page as
    /// restating the section it is politely handing off to. The same happened
    /// on the shared label of a shared link. §12.8's WI-510 rule already says a
    /// duplicated link is not a duplicated sentence; a duplicated URL is even
    /// less of one.
    /// </summary>
    /// <summary>
    /// Function words, which is what an eight-word run can be made entirely of
    /// by coincidence.
    ///
    /// Running the restatement check over the WHOLE corpus rather than three
    /// hand-picked siblings raises the chance of an accidental collision, and
    /// the first corpus-wide run produced "and it is not a sign that your" —
    /// eight words shared with /treatments/chemotherapy, one of them carrying
    /// any meaning. A guard that reports coincidences trains people to ignore
    /// it, which is the same failure as a guard that reports nothing.
    /// </summary>
    private static readonly HashSet<string> Stopwords =
    [
        "a", "an", "and", "are", "as", "at", "be", "been", "before", "but", "by", "can", "do",
        "does", "for", "from", "get", "go", "had", "has", "have", "how", "i", "if", "in", "into",
        "is", "it", "its", "like", "may", "me", "more", "much", "my", "no", "not", "of", "on",
        "one", "or", "other", "our", "out", "over", "own", "so", "some", "than", "that", "the",
        "their", "them", "then", "there", "these", "they", "this", "to", "up", "was", "we",
        "were", "what", "when", "which", "who", "will", "with", "would", "you", "your",
    ];

    /// <summary>Three or more content words, or it is a coincidence rather than a restatement.</summary>
    private static bool CarriesContent(string shingle) =>
        shingle.Split(' ').Count(w => !Stopwords.Contains(w)) >= 3;

    private static IEnumerable<string> Shingles(string text, int n)
    {
        // Headings go too. Slot 9's heading is "Who reads it, and how do I get
        // the result?" on EVERY library page, because §12.8 prescribes it — so
        // an unfiltered shingle check reports every page in the library as
        // restating every other one. The template is not a restatement.
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
public sealed class FollowUpScansPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/follow-up-scans";

    private readonly WebApplicationFactory<Program> _factory;

    public FollowUpScansPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Follow-up scans", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // §12.8's WI-519 lesson: a new library page nothing links to is half
        // shipped, and /tests has no index to catch it. These are the pages a
        // reader is standing on when the next scan is the thing on their mind.
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/tests/mri", "/tests/waiting-for-results", "/treatments/radiation-therapy",
                     "/tumors/glioma", "/tumors/low-grade-glioma", "/tumors/high-grade-glioma",
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
            "/tests/mri", "/tests/waiting-for-results", "/treatments/radiation-therapy",
            "/tests/pathology-report", "/tumors", "/get-help-now");

    [Fact]
    public async Task TheDeepLinksIntoTheSiblingPagesLandOnRealAnchors() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheDeepLinksFromTheHubsLandOnRealAnchorsHere() =>
        // The other side of the contract. /tumors/glioblastoma deep-links
        // #looks-worse, and a reworded heading here breaks it silently.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), "/tumors/glioblastoma");

    [Fact]
    public async Task TheTermsThisPageDefinesAreSuppressedHereAndStillFireElsewhere()
    {
        // §12.8 (WI-509): a popover repeating the paragraph directly beneath it
        // is noise, and this page defines all three of these at length.
        //
        // WI-512's other half first: assert the WORD is in this page's prose
        // before asserting its tooltip is not, or the suppression is a no-op and
        // the DoesNotContain passes for the wrong reason.
        var body = CuratedPage.ReaderText(CuratedPage.Read("tests", "follow-up-scans.md"));
        Assert.Contains("pseudoprogression", body, StringComparison.Ordinal);
        Assert.Contains("radiation necrosis", body, StringComparison.Ordinal);
        Assert.Contains("RANO", body, StringComparison.Ordinal);

        var client = _factory.CreateClient();
        var html = await client.GetStringAsync(Url);

        Assert.DoesNotContain("def-pseudoprogression", html);
        Assert.DoesNotContain("def-radiation-necrosis", html);
        Assert.DoesNotContain("def-rano", html);

        // WI-519's rule: a glossary entry defined only on the page that
        // suppresses it is decoration, so each suppression is proven not to have
        // switched off the term's only live use.
        Assert.Contains("def-pseudoprogression",
            await client.GetStringAsync("/treatments/radiation-therapy"));
        Assert.Contains("def-radiation-necrosis",
            await client.GetStringAsync("/tumors/high-grade-glioma"));
    }

    [Fact]
    public async Task TheRanoGlossaryEntryIsReachableNowhereAndThatIsRecordedRatherThanHidden()
    {
        // An honest finding rather than a passing test dressed up as one.
        //
        // `glossary/rano.md` was written by an earlier item and the word "RANO"
        // appears in NO page's prose anywhere in the corpus — this is the first
        // page to say it, and this page defines it, so §12.8 says suppress it
        // here. The entry therefore fires nowhere, which is exactly the
        // decoration WI-519 dropped a `burr hole` entry to avoid.
        //
        // It is kept rather than deleted because the word is on real reports and
        // the entry is correct; what is not acceptable is believing it works.
        // This test pins the state so the next page that says RANO in prose —
        // WI-522's watch-and-wait is the likely one — makes it live and this
        // test goes red, which is the prompt to delete the test rather than the
        // entry.
        var client = _factory.CreateClient();
        var pages = Directory.EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories);
        var others = pages
            .Where(f => !f.EndsWith("follow-up-scans.md", StringComparison.Ordinal))
            .Where(f => Regex.IsMatch(CuratedPage.ReaderText(File.ReadAllText(f)), @"\bRANO\b"))
            .ToList();

        Assert.True(others.Count == 0,
            "another page now uses RANO in prose, so the glossary entry is live. Delete this test and "
            + "assert the tooltip fires there instead:\n  " + string.Join("\n  ", others));

        Assert.DoesNotContain("def-rano", await client.GetStringAsync(Url));
    }
}
