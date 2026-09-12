using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-522: X1, watch and wait. The fourth TREATMENT page under the §12.8
/// LIBRARY template (the shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs), and the first treatment page with no treatment
/// on it.
///
/// The prose is reviewed, not tested. What is pinned here is the handful of
/// places where this page could leave a reader worse off than no page at all,
/// and on this page they are almost all claims the BACKLOG ASKED FOR:
///
///   * "watch-and-wait carries 4.26x higher risk of a pathological depression
///     score". The paper is a 31-versus-31 cross-sectional survey, the figure
///     is a univariate odds ratio the abstract mislabels as multivariate, the
///     paper's own conclusion is that distress is high "independent of
///     management strategy", and three other studies found no difference.
///   * "the reassuring growth data", which is MENINGIOMA data, misattributed
///     in the dossier to a paper that contains none of it — on a page that also
///     serves diffuse low-grade glioma readers, whose tumors "grow continuously
///     and usually transform".
///   * a scan schedule, which WI-521 ruled out for a page serving many tumor
///     types unless a reachable source gives one for the situation described.
///
/// So the guards are claim-shaped rather than string-shaped: a QUANTITY next
/// to a MEANING, a REASSURANCE next to the wrong tumor, checked per sentence
/// or per paragraph. That is WI-520's lesson, where thirteen of sixteen
/// adversarial mutations went through a string-shaped suite.
/// </summary>
public sealed class WatchAndWaitPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "watch-and-wait.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section as the reader meets it. Not <c>CuratedPage.ReaderText</c>,
    /// which strips front matter by index and returns <c>section[3..]</c> when
    /// fed a section (§12.8, WI-520). <c>Section</c> already flattens.
    /// </summary>
    private static string Reader(string section) => Regex.Replace(section, @"!%(.+?)%", "");

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    /// <summary>
    /// A quantity, however this corpus happens to write it. Copied from the
    /// WI-521 suite rather than shared, deliberately: §12.8 (WI-521) records
    /// that a schedule guard and a frequency guard need DIFFERENT vocabularies,
    /// and a shared constant is how one of them quietly borrows the other's.
    /// </summary>
    private const string Quantity =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|half|third|quarter|dozen"
        + @"|few|several|couple|once|twice|most|many)";

    /// <summary>The subset of <see cref="Quantity"/> that can express a proportion or a count.</summary>
    private const string ProportionQuantity =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|half|third|quarter|dozen)";

    private const string TimeUnit = @"(?:day|week|month|year|hour|minute)s?";

    private const string GrowthHeading = "What if it grows while we are watching?";
    private const string WorryHeading = "Worry and low mood, and what you can do";

    /// <summary>
    /// Word runs this page shares with another page ON PURPOSE. Kept tiny for
    /// the WI-521 reason: every entry is a link label or a safety claim, and if
    /// the list grows past a handful the prose has been copied, not written.
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // The standard "Where to go next" door, worded identically on every
        // library page so a reader moving between them recognises it.
        "Get help now if you need to talk to a person today.",

        // EORTC 22845's survival half, in the words /tumors/oligodendroglioma
        // printed first and /tumors/low-grade-glioma now prints too. /review
        // found the three pages stating it three ways ("lived about as long",
        // "has not been shown to help them live longer", "has not been shown
        // to change how long people live"); §12.10 says one claim, one
        // strength, and for a claim this close to prognosis the answer is
        // identical words.
        "it has not been shown to change how long people live.",
    ];

    /// <summary>
    /// A count, however this corpus writes it, including the teens and the
    /// fractions. /review walked the first version of the growth guard with
    /// "within EIGHTEEN months" and "nearly TWO-THIRDS of the tumors", neither
    /// of which <see cref="ProportionQuantity"/> could see.
    /// </summary>
    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|"
        + @"fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|"
        + @"seventy|eighty|ninety|hundred|dozen)";

    private const string Fraction = @"(?:half|halves|thirds?|quarters?|fifths?|tenths?)";

    private static readonly HashSet<string> AllowedShingles =
        [.. DeliberatelyShared.SelectMany(s => Shingles(s, 8))];

    [Fact]
    public void TheShortVersionCarriesThePlanTheRouteAndTheCost()
    {
        // §12.3's number applied to a library page: readers consume 20 to 28% of
        // a page, so the summary IS the page for most of them (§12.8, WI-520).
        var shortVersion = Reader(Section("The short version"));

        // 1. The reframe the whole page exists for.
        Assert.Contains("It is a plan, not a delay.", shortVersion, StringComparison.Ordinal);

        // 2. The route out, which is what makes the absence of an escalation
        //    list a decision rather than a gap. Said HERE because a reader who
        //    stops at the summary must still know they can bring a scan forward.
        Assert.Contains("you do not\nhave to wait for the next scan".Replace("\n", " "),
            shortVersion, StringComparison.Ordinal);

        // 3. The cost, which the backlog asked to be the page's centre of
        //    gravity and which every comparator treats as a footnote. It is
        //    stated here WITHOUT a figure — see the next test for why.
        Assert.Contains("The waiting can be hard", shortVersion, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDepressionFigureIsNotPublishedInAnyShape()
    {
        // THE RULING THIS PAGE EXISTS TO MAKE, and it is against the backlog.
        //
        // The backlog: "watch-and-wait carries 4.26x higher risk of a
        // pathological depression score". PMC7761113, read in full: a
        // cross-sectional survey of 31 watched and 31 operated people at one
        // German centre; 4.26 is an UNIVARIATE ODDS RATIO, 95% CI 1.19-15.25,
        // which the abstract mislabels "Multivariate" (the multivariate figure
        // is 5.83); anxiety did not differ; and the paper's own conclusion is
        // "a high prevalence of psychological distress in meningioma patients
        // INDEPENDENT OF MANAGEMENT STRATEGY". QUALMS (243 people), a 30-person
        // mixed-methods study, and the same cohort re-surveyed all failed to
        // find a watch-and-wait excess. An odds ratio read at 6th grade as
        // "four times as likely" is also a risk ratio it is not.
        //
        // So no figure, in any shape. A list of banned numbers is beaten by
        // the next number somebody writes (§12.8, WI-520), so the SHAPE is
        // banned: a quantity next to "times", or a magnitude word next to "more
        // likely / more common".
        foreach (var shape in new[]
                 {
                     @"4\.26|5\.83|\bodds\b",
                     // "fourfold", "twice as likely", "double the rate" — the
                     // multiplier words. /review wrote "a fourfold higher share"
                     // and "more than twice as likely", and the first version,
                     // which knew only a bare "fold", saw neither.
                     @"\b(?:twice|double|doubled|triple|tripled|\w+fold)\b",
                     $@"\b{ProportionQuantity}\s+times\b",
                     @"\b(?:much|far|a lot|many times|way)\s+more\s+(?:likely|common|often)\b",
                     // NOT "times the": that failed "At times the wait feels
                     // endless", which is correct English (§12.8: a rule that
                     // fails a correct page is worse than no rule).
                     @"\btimes\s+(?:more|higher|as (?:likely|common|often|many))\b",
                     @"percent|per cent|%",
                     // The proportion itself, looking FORWARD from the count
                     // (§12.8, WI-521). /review: "nearly nine in ten of the
                     // people being watched had low mood, against six in ten
                     // after surgery".
                     $@"\b{CountWord}\s+(?:in|out of)\s+(?:\d+|two|three|four|five|six|seven|eight|nine|ten|a hundred|every)\b",
                     $@"\b(?:{CountWord}[- ])?{Fraction}\s+of\b",
                 })
        {
            Assert.False(Regex.IsMatch(Body, shape, RegexOptions.IgnoreCase),
                $"the page now prints a size for the depression finding (\"{Regex.Match(Body, shape, RegexOptions.IgnoreCase).Value}\"). "
                + "It is a univariate odds ratio from 31 people, the paper's own conclusion is that distress "
                + "is high whatever the plan, and other studies did not find the difference at all.");
        }

        // And the CAUSAL version, which needs no number at all and is the claim
        // the figure was being used to carry.
        // A modal is allowed between subject and verb: /review wrote "Watch and
        // wait CAN cause depression", which the first version, requiring the
        // verb immediately, did not see.
        Assert.DoesNotMatch(
            new Regex(@"(?:watching|watch and wait|waiting|being watched)\s+(?:(?:can|may|might|will|does|often|tends? to)\s+)?"
                + @"(?:causes?|leads? to|raises?|increases?|brings?|triggers?)\b"
                + @"[^.]{0,40}\b(?:depress|low mood|distress)",
                RegexOptions.IgnoreCase),
            Body);
        Assert.DoesNotMatch(new Regex(@"makes? (?:you|people) depressed", RegexOptions.IgnoreCase), Body);
    }

    [Fact]
    public void TheOneStudyIsNamedInItsOwnSentenceAndTheDisagreementIsTheNextOne()
    {
        // §12.8 (WI-507 and WI-511): a finding from one place is attributed in
        // the sentence that prints it, and where sources disagree the page
        // prints the disagreement rather than a third answer belonging to
        // nobody. Both halves are positional, so both are pinned by position.
        var sentences = CuratedPage.SentencesOf(Reader(Section(WorryHeading)));

        var finding = Array.FindIndex(sentences,
            s => Regex.IsMatch(s, @"more common in the people being watched", RegexOptions.IgnoreCase));
        Assert.True(finding >= 0, "the worry section no longer reports the German finding at all");

        // Attributed IN the sentence, not in the one before it.
        Assert.Contains("One study", sentences[finding], StringComparison.Ordinal);
        Assert.Contains("Germany", sentences[finding], StringComparison.Ordinal);

        // The disagreement is the very next thing the reader meets.
        Assert.True(finding + 1 < sentences.Length, "the finding is the last sentence of its section");
        // "Another study", singular, since /review: the first version said
        // "Other studies did not find that difference", and the plural rested on
        // one 30-person study plus QUALMS, which measured quality of life rather
        // than low mood. The page says what the one comparable study found.
        Assert.Matches(new Regex(@"^Another study found no such difference\b"), sentences[finding + 1].Trim());

        // EXACTLY ONCE across the whole page. §12.8 (WI-521): an ordering test
        // on first occurrences says nothing about a restatement further down —
        // "People being watched are more likely to feel low" appended to the
        // summary would be the unattributed version, with no disagreement.
        Assert.Equal(1, Regex.Matches(Body,
            @"(?:more common|more likely|more often)[^.]{0,40}\b(?:watched|watching|waiting)\b"
            + @"|\b(?:watched|watching|waiting)\b[^.]{0,40}(?:more common|more likely|more often)",
            RegexOptions.IgnoreCase).Count);
    }

    [Fact]
    public void TheWorryFindingsCarryTheirMeningiomaScope()
    {
        // §12.12: a claim about "people" on a page serving several tumor types
        // must name WHICH ones. Every study behind this section is of people
        // with a meningioma (plus acoustic neuroma and pituitary series that
        // also found no difference). The Cochrane review says outright that
        // there are "limited data on the psychological impact" for glioma.
        var section = Reader(Section(WorryHeading));

        var scope = section.IndexOf("Most of that research is in people with a meningioma", StringComparison.Ordinal);
        var finding = section.IndexOf("whether it was being watched or had been taken out", StringComparison.Ordinal);
        Assert.True(scope >= 0 && finding > scope,
            "the worry findings now arrive before the tumor they were measured in");

        // The common-either-way sentence names the tumor in the sentence itself.
        var either = CuratedPage.SentencesOf(section)
            .Single(s => s.Contains("whether it was being watched or had been taken out", StringComparison.Ordinal));
        Assert.Contains("meningioma", either, StringComparison.Ordinal);

        // The half that pushes against "just take it out" for peace of mind,
        // which the sources support (high distress in the operated group too).
        Assert.Contains("Having it removed did not make that go away", section, StringComparison.Ordinal);

        // And the limit of the evidence, said rather than implied — for GLIOMA,
        // since /review. The first version said "less is known about other
        // kinds of tumor" while citing EANO's acoustic-neuroma guideline, which
        // says people being watched "responded more favorably in the
        // questionnaires than those who were treated up front", and a
        // 16-study meta-analysis found comparable quality of life.
        Assert.Contains("Less is known\nabout people being watched for a glioma".Replace("\n", " "),
            section, StringComparison.Ordinal);
        Assert.Contains("felt about as well as people who had been treated, or better",
            section, StringComparison.Ordinal);

        // CLAIM-SHAPED, not only phrase-pinned. /review appended "Worry and low
        // mood are common in everyone being watched, whatever the tumor." and
        // every Contains above stayed green. Any sentence here that says
        // something is COMMON must name the tumor it was measured in, or be the
        // attributed one-study sentence — and nothing may generalise it.
        foreach (var sentence in CuratedPage.SentencesOf(section))
        {
            if (!Regex.IsMatch(sentence, @"\bcommon\b|\busual\b|\bmany people\b|\bmost people\b",
                    RegexOptions.IgnoreCase))
            {
                continue;
            }

            Assert.False(
                Regex.IsMatch(sentence, @"whatever the tumor|any tumor|every tumor|all tumors|everyone|everybody|anyone",
                    RegexOptions.IgnoreCase),
                $"the worry section now generalises a meningioma finding to every reader: \"{sentence}\"");
            Assert.True(
                Regex.IsMatch(sentence, @"meningioma|acoustic neuroma|^One study|^Another study"),
                $"this sentence says worry or low mood is common without naming the tumor it was measured "
                + $"in: \"{sentence}\"");
        }
    }

    [Fact]
    public void NothingOnThePagePromisesThatTheWorryWillLift()
    {
        // No intervention has been tested in people being watched. The nearest
        // evidence is one mixed-tumor trial and a review calling CBT results
        // "mixed". A page that tells a frightened reader something works, when
        // it has not been shown to, turns their not-feeling-better into their
        // own failure (the WI-521 scanxiety ruling, one subject over).
        //
        // Over the summary and the two sections about feeling, not only the
        // one named for it: §12.8 (WI-520) records that a section-scoped
        // guard cannot see the short version.
        var promisesRelief = new Regex(
            @"\b(reduces?|eases?|relieves?|cures?|prevents?|fixes?|helps?|calms?|settles?|soothes?|"
            + @"lessens?|lifts?|proven|shown to (?:work|help)|takes the edge off|makes?|"
            // /review: "Talking to a counselor will GET YOU THROUGH it".
            + @"gets? you through|see you through|pulls? you through|carr(?:y|ies) you through)\b"
            + @"[^.,;:]{0,45}\b(anxiety|worry|worrying|mood|fear|stress|distress|wait|waiting|easier|better|it)\b",
            RegexOptions.IgnoreCase);

        // The slot-7 list too: /review put a promise there and the first
        // version, which scanned three sections, never read it.
        foreach (var heading in new[]
                 {
                     "The short version", "What is it like to be watched?", WorryHeading,
                     "What you can do while you are being watched",
                 })
        {
            foreach (var sentence in CuratedPage.SentencesOf(Reader(Section(heading))))
            {
                // A negation that is part of a SUPERLATIVE is not a negation:
                // "There is NO better way to ease the worry than talking to a
                // counselor" bought itself a pass from the clause-anchored
                // negation check below, which is the WI-511 fix defeated by the
                // one English construction that uses "no" to mean "most".
                Assert.False(
                    Regex.IsMatch(sentence,
                        @"\b(?:no|nothing)\s+(?:better|more effective|works better)\b|\bthe best way to\b",
                        RegexOptions.IgnoreCase),
                    $"'{heading}' now ranks a way of coping as the best one: \"{sentence}\"");

                foreach (Match match in promisesRelief.Matches(sentence))
                {
                    // Negation anchored to the CLAUSE (§12.8, WI-511).
                    var before = sentence[Math.Max(0, match.Index - 40)..match.Index];
                    Assert.True(
                        Regex.IsMatch(before,
                            @"\b(not|never|none|nobody|nothing|cannot|can't|hardly|no|n't|far from)\b[^.,;:]{0,45}$",
                            RegexOptions.IgnoreCase),
                        $"'{heading}' now promises relief nothing has been shown to give: \"{sentence}\"");
                }
            }
        }

        // What IS said is the paper's own recommendation, attributed, at the
        // paper's own strength. /review: the first version wrote "suggested that
        // people being watched SHOULD be asked", which turned "might be helpful"
        // into "should" and narrowed "patients with even relatively harmless
        // intracranial findings" to the watched group.
        var worry = Reader(Section(WorryHeading));
        Assert.Contains("might be helpful", worry, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"researchers[^.]{0,80}\bshould\b"), worry);
    }

    [Fact]
    public void TheMeningiomaReassuranceNeverReachesAGliomaReader()
    {
        // §12.12, and the most dangerous sentence this page could write. The
        // "reassuring growth data" the backlog asked for is meningioma data:
        // growth that slows or stops, treatment avoided in more than four in
        // ten, nobody symptomatic before treatment. A diffuse low-grade glioma
        // does the opposite — it "grow[s] continuously and usually
        // transform[s]" (Jakola 2017). Moved one paragraph down, the meningioma
        // sentences tell a glioma reader their tumor will probably settle.
        var section = Reader(Section(GrowthHeading));

        var meningioma = section.IndexOf("**Meningioma.**", StringComparison.Ordinal);
        var glioma = section.IndexOf("**Low-grade glioma.**", StringComparison.Ordinal);
        var acoustic = section.IndexOf("**Acoustic neuroma.**", StringComparison.Ordinal);
        Assert.True(meningioma >= 0 && glioma > meningioma && acoustic > glioma,
            "the growth section is no longer split by tumor, in the order the reassurance scoping relies on");

        var gliomaParagraph = section[glioma..acoustic];

        // PARAGRAPH-scoped, not proximity-scoped: §12.14 records a ±220
        // character window that leaked across a bullet boundary. The shape is
        // a REASSURANCE ABOUT GROWTH, in any of the ways English writes it —
        // widened after /review planted "Many people never GO ON TO need
        // radiation", "it stays STABLE for DECADES" and "NO ONE who is watched
        // gets symptoms", none of which the first list could see.
        var reassurance = new Regex(
            @"\b(?:slow(?:s|ed)? down|stop(?:s|ped)? growing|level(?:s|led)? off|settle(?:s|d)?"
            + @"|never (?:\w+ ){0,3}(?:need|needed|needs|turns?)|do(?:es)? not grow|did not grow"
            + @"|stay(?:s|ed)? (?:the same|still|quiet|stable|small)|\bstable\b|\bdecades?\b"
            + @"|did not make[^.]{0,30}harder|(?:nobody|no one|none of)[^.]{0,40}symptoms|no symptoms)",
            RegexOptions.IgnoreCase);

        // The reassurance may live ONLY in the paragraphs of the tumors it is
        // true of. /review planted "Most tumors that grow while they are watched
        // slow down or stop on their own" in the section's OPENING paragraph,
        // which was neither the glioma paragraph nor "outside the section", so
        // the first version checked neither. Everything that is not the
        // meningioma, acoustic-neuroma or pituitary paragraph is now checked.
        var pituitary = section.IndexOf("**Pituitary tumor.**", StringComparison.Ordinal);
        var closing = section.IndexOf("Whatever the tumor,", StringComparison.Ordinal);
        Assert.True(pituitary > acoustic && closing > pituitary,
            "the growth section no longer ends with the pituitary paragraph and the closing question");

        foreach (var (name, text) in new[]
                 {
                     ("the opening", section[..meningioma]),
                     ("the glioma paragraph", gliomaParagraph),
                     ("the closing line", section[closing..]),
                 })
        {
            Assert.False(reassurance.IsMatch(text),
                $"{name} of the growth section now carries a growth reassurance "
                + $"(\"{reassurance.Match(text).Value}\"). The sources say diffuse low-grade gliomas keep "
                + "growing and can change type; the reassuring growth data belongs to named tumors.");
        }

        // The honest half is there, in words a reader cannot soften by skimming.
        Assert.Contains("usually keeps growing slowly", gliomaParagraph, StringComparison.Ordinal);
        Assert.Contains("change into a faster-growing type", gliomaParagraph, StringComparison.Ordinal);

        // AND OUTSIDE THE GROWTH SECTION, any reassurance-shaped sentence has to
        // name a tumor it is true of IN THE SAME SENTENCE. The first draft of
        // this page said "for a tumor that may never have needed treating at
        // all" in the why-section and "for some it never turns into treatment
        // at all" in the how-long section — both unscoped, both read by the
        // glioma reader before they reach the paragraph that is about them.
        var outside = Body.Replace(CuratedPage.Flatten(Section(GrowthHeading)), " ", StringComparison.Ordinal);
        foreach (var sentence in CuratedPage.SentencesOf(outside))
        {
            if (!reassurance.IsMatch(sentence))
            {
                continue;
            }

            Assert.True(
                Regex.IsMatch(sentence, @"meningioma|acoustic neuroma|pituitary", RegexOptions.IgnoreCase),
                $"this sentence reassures about growth without naming a tumor it is true of: \"{sentence}\"");

            // Naming A tumor is not enough if it also names a glioma: /review
            // wrote "Whether it is a meningioma or a glioma, the growth usually
            // slows down in time", which satisfied the check above.
            Assert.DoesNotMatch(new Regex(@"glioma", RegexOptions.IgnoreCase), sentence);
        }
    }

    [Fact]
    public void TheGliomaParagraphCarriesBothHalvesOfTheRadiationTimingTrial()
    {
        // EORTC 22845, via two reachable reviews: early radiation gave a longer
        // stretch before the tumor grew again, and overall survival was the
        // same. §12.8 (WI-506): answer the frightening question in both
        // directions. Without the first half the page promises waiting costs
        // nothing; without the second it tells a watched reader they are
        // losing time. No figures (contract item 5).
        var section = Reader(Section(GrowthHeading));

        Assert.Contains("held the tumor back for longer", section, StringComparison.Ordinal);
        Assert.Contains("it has not been shown to change how long people live", section, StringComparison.Ordinal);

        // The seizure half, which /tumors/oligodendroglioma prints and the
        // first version of this page and the low-grade hub both dropped —
        // "a low incidence of seizure in patients treated straight after
        // surgery" (Cancers 2020), for readers whose main symptom is seizures.
        Assert.Contains("fewer\nseizures".Replace("\n", " "), section, StringComparison.Ordinal);

        // PMC6587541's qualifier, which the dossier dropped: observation for a
        // glioma is "after resection of the tumor". Printed where the glioma
        // reader meets it, and in the list at the top of the page.
        Assert.Contains("watching comes after an operation, as a way of putting off radiation and chemotherapy",
            section, StringComparison.Ordinal);

        // /review's first blocker. The first version said "Experts also differ
        // on whether to operate on a glioma that was found by chance", resting
        // on a German PRACTICE survey — while the guideline this page cites
        // says "Surgical resection is the first step in the diagnosis of LGG,
        // even in incidentally discovered tumors". A page that cites a source
        // for the position it argues against is §12.8 (WI-512). The reader
        // watched without an operation is the one this sentence is for.
        Assert.Contains("an operation should still come first", section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"experts? (?:also )?(?:differ|disagree|are split)", RegexOptions.IgnoreCase),
            Body);
        Assert.Contains("often after an operation", Reader(Section("What is watch and wait?")),
            StringComparison.Ordinal);

        // And it ends on the question rather than on the disagreement between
        // experts (§12.6), before the next tumor begins.
        var glioma = section.IndexOf("**Low-grade glioma.**", StringComparison.Ordinal);
        var acoustic = section.IndexOf("**Acoustic neuroma.**", StringComparison.Ordinal);
        //
        // The question now sits right after the guideline sentence it is about,
        // since the second /review: at the end of the paragraph it followed the
        // vorasidenib sentences and read as a question about the drug. The
        // paragraph ends on a door rather than on the disagreement.
        //
        // WI-526 added a second door on the end of it. This page is the only
        // other one in the corpus that uses placebo vocabulary, and its
        // reader's live alternative IS a trial, so /treatments/clinical-trials
        // is the natural second destination — and the end-anchor is what
        // caught the append, which is the anchor doing its job.
        Assert.Matches(new Regex(@"what a placebo\s*is and what it means for the group you would be in\.\s*$"),
            section[glioma..acoustic]);
        Assert.Contains("has a section on it", section[glioma..acoustic], StringComparison.Ordinal);
        Assert.Contains("it is a fair question to ask why your team chose that", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGliomaReaderIsToldAboutTheMedicineTestedInPeopleLikeThem()
    {
        // Found by the end-to-end read, not by any gate: the first version of
        // this page told a grade 2 glioma reader about watching and about
        // radiation timing, and never mentioned the one option that was tested
        // in EXACTLY the people who would otherwise be watched. PMC12416742,
        // the FDA approval summary: "a placebo was considered acceptable given
        // the option for observation in routine clinical care". A reader being
        // watched after surgery who does not know to ask is the reader the
        // dossier's own recommendation page called "the single biggest recent
        // change for grade 2 glioma patients".
        var section = Reader(Section(GrowthHeading));
        var sentence = CuratedPage.SentencesOf(section)
            .Single(s => s.Contains("vorasidenib", StringComparison.Ordinal));

        // The approval's scope, in the sentence that names the drug — the same
        // scope /tumors/low-grade-glioma prints (§12.10: one claim, one
        // strength). Without it, every glioma reader hears "there is a pill".
        Assert.Contains("grade 2", sentence, StringComparison.Ordinal);
        Assert.Contains("IDH change", sentence, StringComparison.Ordinal);
        Assert.Contains("watched after surgery",
            string.Join(" ", CuratedPage.SentencesOf(section)), StringComparison.Ordinal);

        // And the scope is not widened in the same breath. /review kept both
        // required literals and wrote "for anyone with a glioma of grade 2 OR
        // ABOVE, WITH OR WITHOUT an IDH change". FDA: "insufficient evidence to
        // broaden the indication" to higher grades.
        Assert.DoesNotMatch(
            new Regex(@"or above|or higher|and above|any grade|with or without|anyone|everyone|every glioma",
                RegexOptions.IgnoreCase),
            sentence);

        // And it arrives as a question to ask, scoped to the readers it can
        // apply to. /review: the first version put "Could vorasidenib apply to
        // me?" in front of every meningioma and pituitary reader too.
        Assert.Contains("If I have a grade 2 glioma, could vorasidenib apply to me?", Reader(Section("What to ask your team")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void GrowthIsNotPresentedAsAutomaticallyChangingThePlan()
    {
        // Found by the end-to-end read. The summary said "If the tumor grows ...
        // the plan changes", step six said "If something changes, the plan
        // changes", and the growth section opened "Then the plan changes" — two
        // screens above the sentence "Most meningiomas that are watched do grow
        // a little". A meningioma reader whose report says it grew by a
        // millimetre was told, three times, that treatment was coming. EANO and
        // RANO put the trigger at SUSTAINED growth, and Strømsnes watched
        // growing tumors "until they reach a stable state".
        Assert.DoesNotMatch(
            new Regex(@"\bgrows?\b[^.]{0,60}\bthe plan changes\b|\bthen the plan changes\b", RegexOptions.IgnoreCase),
            Body);

        // Claim-shaped too. /review: "If the tumor gets any bigger, you will
        // move on to treatment" and "If it grew at all, treatment starts" —
        // the same promise, with no "plan changes" in it. The shape is ANY
        // growth, followed in the same sentence by treatment starting.
        Assert.DoesNotMatch(
            new Regex(@"\b(?:any bigger|grows? at all|grew at all|any growth|grows?|grew|gets? bigger)\b"
                + @"[^.]{0,60}\b(?:treatment (?:starts|begins|follows)|move on to treatment|go on to treatment|"
                + @"you will (?:need|have|start|move)|means treatment|you start treatment)\b",
                RegexOptions.IgnoreCase),
            Body);

        Assert.Contains("grows more than expected", Reader(Section("The short version")), StringComparison.Ordinal);
        Assert.Contains("it does not always mean\ntreatment".Replace("\n", " "),
            Reader(Section(GrowthHeading)), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageMakesNoPromisesAboutWhatTheTeamWillDo()
    {
        // §12.8 (WI-521): "your team WILL start again from the top with you" was
        // a guarantee about somebody else's behaviour, found by the end-to-end
        // read. This page's first version said "your team will think again
        // about what it is" and "that doctor goes through it with you" — the
        // same shape, found the same way. A page can say what is a reason for a
        // team to act; it cannot promise that they will.
        // No exemptions: the first version let "will call" through, and /review
        // pointed out that "They will call you the day the result is in" is
        // itself the promise. Wider subjects too — "Your doctor will go through
        // it with you" walked straight past a list that knew "the doctor".
        // The questions list is excluded, because a question asserts nothing
        // ("who will tell me the result?").
        var prose = Body.Replace(CuratedPage.Flatten(Section("What to ask your team")), " ", StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(@"\b(?:your team|they|that doctor|the doctor|your doctor|your nurse|the radiologist|"
                + @"someone|somebody|the hospital)\s+(?:will|always|is going to)\b",
                RegexOptions.IgnoreCase),
            prose);
    }

    [Fact]
    public void TheGrowthDataIsPrintedAsDirectionOnly()
    {
        // The dossier's growth numbers (0.796 cm3 a year, 66% under 1 cm3, a
        // doubling time of 1.27 to 143.5 years with a mean of 21.6) are
        // attributed to a paper that contains none of them. They are one
        // 41-patient series from 2003, quoted in a review. A mean doubling time
        // over a range of one to 143 years describes almost nobody. The page
        // gives the direction and prints no figure.
        var section = Reader(Section(GrowthHeading));

        foreach (var shape in new[]
                 {
                     // A size or a rate.
                     @"\b(?:cm|mm|centimet|millimet|cubic|doubling|volume)",
                     // A time attached to a count ("within eighteen months",
                     // "a year and a half"): the plateau timing, which would
                     // tell a reader whose tumor is still growing after it
                     // that theirs is the bad kind.
                     // CountWord, not ProportionQuantity: /review wrote "within
                     // EIGHTEEN months", the exact case this comment claimed to
                     // catch, and the teens were not on the list.
                     $@"\b{CountWord}\s+{TimeUnit}\b",
                     @"year and a half",
                     // A proportion of tumors or people, looking FORWARD from
                     // the number (§12.8, WI-521).
                     $@"\b{CountWord}\s+(?:in|out of)\s+(?:\d+|two|three|four|five|six|seven|eight|nine|ten|every)\b",
                     $@"\b{CountWord}\s+of\s+(?:\w+\s+){{0,1}}(?:patients|people|them|cases|tumors|meningiomas|gliomas)\b",
                     // Fractions, singular and plural, with or without "of":
                     // "nearly TWO-THIRDS of the tumors", "More than A THIRD
                     // never needed treatment".
                     $@"\b(?:{CountWord}[- ])?{Fraction}\b",
                 })
        {
            Assert.False(Regex.IsMatch(section, shape, RegexOptions.IgnoreCase),
                $"the growth section now prints a figure (\"{Regex.Match(section, shape, RegexOptions.IgnoreCase).Value}\"). "
                + "The dossier's numbers are misattributed and come from one small series; direction only.");
        }

        // And the direction IS printed, both ways, for the meningioma reader.
        Assert.Contains("do grow a little", section, StringComparison.Ordinal);
        Assert.Contains("slows down or stops", section, StringComparison.Ordinal);
        // /review's third blocker: in the Norwegian cohort MOST people were
        // treated — "39/68 (57.4%) tumors in 34/62 (54.1%) patients were treated
        // due to tumor growth". The first version said "some tumors kept growing
        // and were treated", which after "slows down or stops" reads as the
        // exception. The proportion goes in as a comparison, not a figure.
        Assert.Contains("more people ended up having treatment\nthan not".Replace("\n", " "),
            section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\bsome tumors kept growing\b", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheStrongestMeningiomaReassuranceKeepsItsStudyInItsSentence()
    {
        // "Nobody got symptoms before treatment began, and waiting did not make
        // the treatment harder" is verbatim Strømsnes ("None developed symptoms
        // prior to intervention"; "Treatment was not compromised by tumor
        // growth") — 62 people, one Norwegian centre. It is the strongest
        // reassurance on the page, so it is the one that must never float free
        // of "in that study" (§12.8, WI-507; §12.12).
        var sentences = CuratedPage.SentencesOf(Reader(Section(GrowthHeading)));

        // Selected on the SHAPE, not the word "Nobody": /review wrote "No one
        // who is watched gets symptoms before treatment starts" and a test
        // looking for "Nobody" never found it. EVERY such sentence on the page
        // must be the attributed one.
        foreach (var claim in CuratedPage.SentencesOf(Body)
                     .Where(s => Regex.IsMatch(s, @"(?:nobody|no one|none of)[^.]{0,40}symptoms", RegexOptions.IgnoreCase)))
        {
            Assert.Contains("in that study", claim, StringComparison.Ordinal);
        }

        var nobody = sentences.Single(s => s.Contains("Nobody", StringComparison.Ordinal));
        Assert.Contains("in that study", nobody, StringComparison.Ordinal);

        var at = Array.IndexOf(sentences, nobody);
        Assert.Contains("Norway", sentences[at - 1], StringComparison.Ordinal);
    }

    [Fact]
    public void TheGrowthSectionEndsOnTheQuestion()
    {
        // §12.6 and §12.8 (WI-510, WI-521): ending on something the reader can
        // do is POSITIONAL, so the last sentence is pinned and anchored to its
        // END, or an appended clause defeats it.
        var last = CuratedPage.SentencesOf(Reader(Section(GrowthHeading)))[^1];
        Assert.Matches(new Regex(@"how much change would make them act\.?\s*$"), last);
    }

    [Fact]
    public void TheAcousticNeuromaParagraphCarriesTheCostOfWatchingAsWellAsTheReassurance()
    {
        // EANO 2020: observation is "appropriate for incidental, asymptomatic
        // VS", AND "50% of patients lost functional hearing during a 3-4 year
        // period". A paragraph that kept only "many small ones do not grow"
        // would leave this reader to discover the other half in an audiology
        // booth. Both halves, no figure.
        var section = Reader(Section(GrowthHeading));
        var acoustic = section[section.IndexOf("**Acoustic neuroma.**", StringComparison.Ordinal)..];

        Assert.Contains("Many small ones do not grow", acoustic, StringComparison.Ordinal);
        // "often", since /review: EANO says "50% of patients lost functional
        // hearing during a 3-4 year period", and "can get worse" understated a
        // coin-flip as a possibility.
        Assert.Contains("hearing on that side often\ngets worse while the tumor is being watched".Replace("\n", " "),
            acoustic, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWithoutASampleSectionSaysWhatWatchingCannotDoInBothDirections()
    {
        // §12.8's first WI-506 rule: a library page must say what it cannot do.
        // What watching without an operation cannot do is CONFIRM what the
        // tumor is — EANO 2021 calls these "suspected" and "radiologically
        // assumed" meningiomas, and says rapid growth means "a higher grade
        // meningioma ... has to be suspected".
        var section = Reader(Section("How can they know what it is without a sample?"));

        Assert.Contains("Only a sample of tissue can say for certain", section, StringComparison.Ordinal);
        Assert.Contains("a best guess for a\nname, not a confirmed one".Replace("\n", " "), section, StringComparison.Ordinal);
        Assert.Contains("grows fast", section, StringComparison.Ordinal);
        Assert.Contains("think again about what it is", section, StringComparison.Ordinal);

        // No invented accuracy figure for the guess, in words or otherwise.
        // Nothing reachable says how often a scan's guess is right.
        Assert.DoesNotMatch(
            new Regex(@"\b(?:almost|nearly|usually|mostly|generally)\s+(?:always\s+)?(?:right|correct|accurate)\b",
                RegexOptions.IgnoreCase),
            Body);

        // And it ends on the question (§12.6), anchored to the end.
        var last = CuratedPage.SentencesOf(section)[^1];
        Assert.Matches(new Regex(@"what would make them want a sample\.?\s*$"), last);
    }

    [Fact]
    public void OnlyTheAttributedRangeIsPrintedAsASchedule()
    {
        // WI-521's ruling: no surveillance schedule on a page serving several
        // tumor types, unless a reachable source gives one for the situation
        // described, attributed in the sentence. Reachable schedules DO exist
        // here (EANO 2021 meningioma, EANO 2020 acoustic neuroma, Endocrine
        // Society pituitary) and none is printed: two editions of the EANO
        // meningioma guideline disagree on the first scan, glioma schedules are
        // "pragmatic rather than evidence-based" (Cochrane), and a per-tumor
        // timetable is a number a frightened reader CAN act on (WI-520).
        //
        // What IS printed is the one range that covers everybody and pushes
        // nobody, attributed in its own sentence: The Brain Tumour Charity's
        // "from every few months to every couple of years".
        var sentences = CuratedPage.SentencesOf(Body);

        foreach (var pattern in new[]
                 {
                     $@"every\s+{Quantity}\s+(to\s+{Quantity}\s+)?{TimeUnit}",
                     $@"every\s+(?:a\s+)?couple of\s+{TimeUnit}",
                     // Weeks, months and years only. "every day" is how a
                     // correct sentence says a pill is taken daily, and the
                     // first version failed it (/review).
                     @"every\s+(?:other\s+)?(?:week|month|year)s?\b",
                     $@"{Quantity}[- ]monthly",
                     $@"{Quantity}\s+{TimeUnit}\s+apart",
                     // "a year apart", "six months in", "3 to 6 months",
                     // "one scan a year", "in 3 to 6 months" — /review wrote
                     // three schedules the first version could not see.
                     $@"\ban?\s+{TimeUnit}\s+apart\b",
                     $@"\b{Quantity}\s+{TimeUnit}\s+(?:in|later|after that|on)\b(?!\s+(?:your|the|a)\b)",
                     $@"\b\d+\s*(?:to|-)\s*\d+\s+{TimeUnit}",
                     $@"\b(?:one|a)\s+scans?\s+(?:a|each|every|per)\s+{TimeUnit}",
                     $@"\bin\s+{Quantity}\s+(?:to\s+{Quantity}\s+)?{TimeUnit}\b",
                     $@"\b(?:once|twice|{Quantity}\s+times)\s+(?:a|each|every|per)\s+{TimeUnit}",
                     // Not "each year": "each year" is ordinary English about
                     // anything. The schedule form is a scan attached to it.
                     $@"\b(?:yearly|annually|annual|half[- ]yearly|quarterly)\b|\bscans?\b[^.]{{0,20}}\beach\s+{TimeUnit}",
                     // EANO's "for 5 years", "after five years" — the length of
                     // the meningioma schedule, which is half of it.
                     $@"\b(?:for|after|at)\s+(?:the\s+first\s+)?{ProportionQuantity}\s+{TimeUnit}\b",
                     $@"scan(ned|s)?\s+(?:you\s+)?every\s+{Quantity}",
                 })
        {
            foreach (var sentence in sentences.Where(s => Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase)))
            {
                Assert.True(
                    sentence.Contains("charity", StringComparison.OrdinalIgnoreCase)
                    && sentence.Contains("puts the range at", StringComparison.Ordinal),
                    $"the page now prints a scan interval outside the one attributed range: \"{sentence}\". "
                    + "It serves several tumor types, and every per-tumor schedule is either disputed between "
                    + "guideline editions or, for glioma, pragmatic rather than evidence-based.");
            }
        }

        // The range itself appears exactly once — a second, unattributed copy
        // in the summary would be the WI-521 first-occurrence trap.
        Assert.Equal(1, Regex.Matches(Body, @"every few\s+months", RegexOptions.IgnoreCase).Count);

        // And the page says why, rather than being quietly silent.
        var howOften = Reader(Section("How often, and for how long?"));
        Assert.Contains("there is no single timetable to print here", howOften, StringComparison.Ordinal);
        Assert.Contains("ask for your own timetable, in writing", howOften, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStoppingSentenceKeepsTheScopeItsSourceAttachesToIt()
    {
        // §12.8 (WI-520): print the scope the source attaches to a rule. The
        // dossier cites IMPACT's PROTOCOL (PMC8768908), which has no results.
        // The model paper (PMC7032634) proposes discharge ONLY for low- and
        // medium-risk patients who are ALSO frail or seriously ill. "Low-risk
        // people can stop being scanned" is a different, and wrong, claim.
        //
        // The two groups the page names are the two its sources support: a
        // meningioma that has settled (Stromsnes, "Clinical follow-up seems
        // sufficient beyond 5 years if self-limiting growth is established"),
        // and older people whose other health problems matter more (IMPACT).
        //
        // CLAIM-SHAPED, over every sentence that talks about fewer scans or
        // stopping, not a Single() on the word "stopping". /review wrote "Many
        // people can stop having scans after some years" (no "stopping") and a
        // widened version naming "younger people and not only older people",
        // and the first version saw neither. Questions assert nothing, so the
        // questions list is excluded.
        var prose = Body.Replace(CuratedPage.Flatten(Section("What to ask your team")), " ", StringComparison.Ordinal);
        var all = CuratedPage.SentencesOf(prose);
        var stoppingSentences = all
            .Where(s => Regex.IsMatch(s,
                @"\bstopping\b|\bstop(?:s|ped)?\s+(?:having\s+|getting\s+|the\s+|your\s+){0,2}(?:scans?|scanning|follow-up|monitoring|check-ups)"
                + @"|\b(?:no more|fewer|less often|less frequent)\b[^.]{0,20}\bscan",
                RegexOptions.IgnoreCase))
            .ToList();

        Assert.NotEmpty(stoppingSentences);
        foreach (var sentence in stoppingSentences)
        {
            Assert.Contains("meningioma", sentence, StringComparison.Ordinal);
            Assert.Contains("older people", sentence, StringComparison.Ordinal);
            Assert.Contains("other health", sentence, StringComparison.Ordinal);
            Assert.DoesNotMatch(
                new Regex(@"younger|not only|anyone|everyone|any age|most people|many people", RegexOptions.IgnoreCase),
                sentence);

            // And the decision is handed back to the team, in the next sentence.
            var at = Array.IndexOf(all, sentence);
            Assert.True(at + 1 < all.Length, "the stopping sentence is the last on the page");
            Assert.Contains("talk through with your team", all[at + 1], StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ThePageCarriesNoEscalationListAndGivesTheRouteInstead()
    {
        // A deliberate absence, and WI-521's ruling carried over: what a reader
        // should call about between scans is TUMOR-SPECIFIC, and this page
        // serves meningioma, glioma, acoustic neuroma and pituitary readers.
        // §12.10 names the pituitary case exactly — a glioma-shaped ambulance
        // list would file a sudden vision change as "same day" for a reader
        // whose emergency it is.
        //
        // The shape guard is copied from WI-521, not re-derived (§12.8,
        // WI-519): find every list FIRST, then judge the paragraph that
        // introduces it. A banned-string list alone was beaten by a fifth
        // heading on WI-520 and three ways on WI-521.
        var urgency = new Regex(
            @"\b(now|today|immediately|at once|urgent(ly)?|ambulance|emergency|911|"
            + @"call|phone|ring|same day|straight away|do not wait|cannot wait)\b",
            RegexOptions.IgnoreCase);

        // A list item INCLUDES its indented continuation lines. /review wrote a
        // lead-in of "Contact your team today, rather than waiting for the next
        // scan, if you notice:" over three bullets that each wrapped onto a
        // second line — and the WI-521 regex, which needed two CONSECUTIVE
        // marker lines, found zero runs and judged nothing. §12.8 (WI-521)
        // assumed a wrapped list arrives as several runs; if EVERY bullet wraps
        // it arrives as none.
        // On the BODY: with continuations allowed, the front matter's `- url:`
        // / `title:` source list becomes a run of its own.
        var markdown = CuratedPage.Body(Page);
        var runs = Regex.Matches(
            markdown,@"(?:^[ \t]*(?:[-*]|\d+\.)[ \t]+.+(?:\r?\n[ \t]+\S.*)*(?:\r?\n|$)){2,}", RegexOptions.Multiline);
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
                "this page now carries what looks like an escalation list of its own. What to call about "
                + "between scans depends on the tumor, so every such list has to be diffed per symptom, in "
                + "both directions, against /treatments/craniotomy, /tests/biopsy and /seizures/what-to-do "
                + "(§12.8, WI-511/WI-512/WI-519). The list:\n" + run.Value + "\nIntroduced by:\n" + before);
        }

        foreach (var phrase in new[]
                 {
                     "call an ambulance", "these cannot wait", "straight away", "right away", "911",
                     "emergency room", "urgent care", "go to the er", "as soon as possible",
                     "the same day if", "the same day for", "the same day about",
                 })
        {
            Assert.DoesNotContain(phrase, Body, StringComparison.OrdinalIgnoreCase);
        }

        // The route, which is what makes the absence a decision rather than a
        // gap. In the steps, in the reader's own actions, and as a question.
        Assert.Contains("You do not have to wait for your next scan",
            Reader(Section("What happens, step by step")), StringComparison.Ordinal);
        Assert.Contains("get your own list rather than trusting a\n  general one".Replace("\n  ", " "),
            Reader(Section("What you can do while you are being watched")), StringComparison.Ordinal);
        Assert.Contains("what counts as an emergency for my tumor",
            Reader(Section("What to ask your team")), StringComparison.Ordinal);

        // The one universal line, added after /review pointed out that step
        // five's "get in touch" was the only between-scans rule a PITUITARY
        // reader met — and §12.10 names sudden sight loss with that tumor as an
        // emergency. The line is deliberately SYMPTOM-FREE: the moment it names
        // a headache or a seizure it is a partial escalation list with no tier
        // diff behind it, which is the thing this test exists to stop.
        var steps = Reader(Section("What happens, step by step"));
        var emergency = CuratedPage.SentencesOf(steps)
            .Single(s => s.Contains("treat it as an emergency", StringComparison.Ordinal));
        // "new to you" since the second /review: "sudden and severe" alone told
        // a reader having their usual seizure to treat it as an emergency, the
        // over-triage /seizures/what-to-do and [ESCALATION] both warn against.
        Assert.Contains("sudden, severe and new to you", emergency, StringComparison.Ordinal);
        Assert.Contains("/seizures/what-to-do", steps, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(@"headache|sight|vision|seizure|fit\b|weak|numb|speech|speak|confus|vomit|sick|faint",
                RegexOptions.IgnoreCase),
            emergency);
        Assert.Contains("/get-help-now", steps, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageSaysWhatWatchingIsAndIsNot()
    {
        // The reframe the dossier says patients are not given, and the two
        // honest limits on it: it is chosen, and it is not for everyone.
        Assert.Contains("watching is a decision, not the absence of one",
            Reader(Section("Why would my team suggest it?")), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("It is not offered to everyone",
            Reader(Section("Why would my team suggest it?")), StringComparison.Ordinal);

        // And the list of who it is for says "some" on every line. A bullet
        // reading "Meningiomas" flat would tell every meningioma reader they
        // qualify, including the ones pressing on something.
        // EVERY bullet, bold or not: /review added an unbolded fifth line,
        // "- Meningiomas of any size, if an operation would be risky", and a
        // count of bold bullets stayed at four. Read on the raw page, where
        // bullets are still line-initial.
        var raw = Regex.Match(Page, @"^## What is watch and wait\?.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        var bullets = Regex.Matches(raw, @"^[ \t]*[-*][ \t]+(.+)$", RegexOptions.Multiline);
        Assert.Equal(4, bullets.Count);
        Assert.All(bullets, m => Assert.StartsWith("**Some ", m.Groups[1].Value, StringComparison.Ordinal));
    }

    [Fact]
    public void TheLowGradeHubAgreesWithThisPageAboutWaiting()
    {
        // §12.10: one claim, one strength, on every page that makes it. Writing
        // the canonical page exposed two sentences on /tumors/low-grade-glioma
        // that the sources do not support, and both now read the sibling here
        // so a change there goes red on this page too (§12.8, WI-521: budget an
        // edit to the hubs when you write the canonical page).
        var lgg = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "low-grade-glioma.md")));
        var oligo = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "oligodendroglioma.md")));
        var growth = Reader(Section(GrowthHeading));

        // 1. "Starting treatment before you need it spends that cost early and
        //    BUYS NOTHING." EORTC 22845 says early radiation bought a longer
        //    stretch before regrowth — the half this page and
        //    /tumors/oligodendroglioma both print.
        Assert.DoesNotContain("buys nothing", lgg, StringComparison.OrdinalIgnoreCase);

        //    ONE WORDING on all three pages since /review, which found three
        //    ("lived about as long", "has not been shown to help them live
        //    longer" — one-directional — and oligodendroglioma's "has not been
        //    shown to change how long people live"). Both halves everywhere:
        //    the tumor held back for longer, and the seizure benefit.
        const string survival = "has not been shown to change how long people live";
        foreach (var (name, text) in new[] { ("this page", growth), ("low-grade-glioma", lgg), ("oligodendroglioma", oligo) })
        {
            Assert.True(text.Contains(survival, StringComparison.Ordinal),
                $"{name} no longer states EORTC 22845's survival half in the shared wording");
            Assert.True(Regex.IsMatch(text, @"fewer seizures|better seizure control"),
                $"{name} has dropped the seizure half of EORTC 22845");
        }

        Assert.Contains("hold the tumor\nback for longer".Replace("\n", " "), lgg, StringComparison.Ordinal);
        Assert.Contains("held the tumor back for longer", growth, StringComparison.Ordinal);
        Assert.Contains("longer stretch before the tumor grows again", oligo, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"lived about as long|help them live longer", RegexOptions.IgnoreCase),
            growth + " " + lgg);

        // 2. "Watching is HARDER to live with THAN TREATING." No source says
        //    so, and the ones this page checked say distress is high whatever
        //    the plan. This page prints "Having it removed did not make that go
        //    away"; the hub cannot print the opposite comparative.
        //
        //    CLAIM-SHAPED since /review, which found the same comparative still
        //    on the hub 150 lines higher — "Waiting is hard IN A WAY THAT
        //    TREATMENT IS NOT" — invisible to a two-phrase ban. And the same
        //    paragraph promised the gap between scans would be "much easier to
        //    live in", which this page's own promise guard forbids here.
        Assert.DoesNotMatch(
            new Regex(@"harder to live with than|harder than treat|in a way that treat\w*|"
                + @"(?:harder|worse|tougher)\s+than\s+(?:treating|treatment|having treatment|an operation)|"
                + @"much easier to live",
                RegexOptions.IgnoreCase),
            lgg);
        Assert.Contains("Having it removed did not make that go away",
            Reader(Section(WorryHeading)), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // §12.8 (WI-521): the restatement check runs over the WHOLE corpus.
        // Three hand-picked siblings reported zero on WI-521 while 108 shingles
        // were shared with a hub. This page's nearest neighbours are
        // /tests/follow-up-scans (the scans, the wait, the report) and
        // /tumors/low-grade-glioma (whose watch section predates this page),
        // and the guard deliberately does not name them.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("watch-and-wait.md", StringComparison.Ordinal));

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

        // Scanxiety is handed off to its canonical page rather than re-argued.
        var worry = Reader(Section(WorryHeading));
        Assert.Contains("/tests/follow-up-scans#waiting", worry, StringComparison.Ordinal);
        foreach (var phrase in new[] { "scanxiety", "five trials", "None of them reduced" })
        {
            Assert.DoesNotContain(phrase, Body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageCarriesNoCaregiverSectionAndTheReasonIsRecorded()
    {
        // Contract item 12 puts a caregiver section on every treatment page
        // WITH MEANINGFUL AFTERCARE. Watching has none: nobody is sent home to
        // dress a wound or give a medicine. The [CAREGIVER] block is built
        // around the two phone numbers and "what counts as an ambulance",
        // which on this page would be the tumor-specific escalation list the
        // page deliberately does not carry.
        //
        // No study found checked a watched patient's family either (PMC7761113
        // did not study caregivers; the one brain-tumor therapy trial found
        // benefit "for people with PBT but not caregivers"), so there is no
        // sourced caregiver-specific content to put in one. What the person
        // alongside can do sits in slot 7.
        Assert.DoesNotContain("[CAREGIVER]", Page, StringComparison.Ordinal);
        Assert.Contains("Take someone with you when results are due",
            Reader(Section("What you can do while you are being watched")), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoPercentageAndNoPrognosisVocabulary()
    {
        // Contract item 5. The glioma counterweight sources are full of median
        // survival figures (Jakola: 5.8 versus 14.4 years; EORTC 22845: 7.4
        // versus 7.2) and none of them may reach the page.
        var body = CuratedPage.ReaderText(Page);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", body);

        foreach (var word in new[]
                 {
                     "survival", "survive", "life expectancy", "five-year", "prognosis", "how long you have",
                     "median", "per cent", "percent", "percentage", "death", "die ",
                 })
        {
            Assert.DoesNotContain(word, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNeitherTheCharacterisationsNorTheMinimisations()
    {
        // A page about NOT treating is where minimising arrives without anyone
        // deciding: "it is only being watched", "nothing to worry about".
        // Negation-aware (§12.8, WI-511).
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "treatments/watch-and-wait");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }

        // Three this page can reach that the shared lists do not carry, because
        // they only have an obvious meaning on a page about not treating. Run
        // over the corpus first, per §12.8: no occurrence anywhere. Page-local
        // rather than promoted: promote at the SECOND page (§12.8, WI-510).
        foreach (var phrase in new[] { "harmless tumor", "safe to ignore", "nothing to be done" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // WI-564 is still open. Four of this page's sources are UK patient
        // pages ("GP", "tumour", "counsellor", "specialist nurse"), which is
        // exactly how British forms arrive on a page without anybody deciding.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // "straight after" / "straight away" since /review: the first version
        // said "radiation given straight after surgery", the only instance in
        // the corpus, where a US reader says "right after".
        foreach (var form in new[]
                 {
                     "tumour", "counsellor", "specialist nurse", "consultant", "straight after", "straight away",
                 })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotMatch(new Regex(@"\bGPs?\b"), body);
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8: read the section for the slot list, not the last page written.
        //
        // Carried: 0, 1, 2, 3, 4, 5 (as a role: there is no procedure to feel,
        // so the slot holds what being watched is like), 6a (the measured
        // cost), 6c twice (the two genuinely-asked worries), 7 (as a role,
        // WI-507 precedent), 9, 10, 11.
        //
        // NOT carried, each for a reason:
        //   * 6b, side effects — watching has none of its own. The one real
        //     cost of waiting for a specific tumor (hearing, for an acoustic
        //     neuroma) is in the growth section with that tumor's name on it.
        //   * 8, "why am I having another one?" — the whole plan IS repeats, and
        //     the frightening version (a scan brought forward) is owned by
        //     /tests/follow-up-scans#again-so-soon, linked from step 6, where
        //     "a scan sooner than planned" is exactly when it happens. (The
        //     first version of this comment said "linked from step 2", which
        //     was not true — /review. The link is now asserted, below.)
        //   * the caregiver section — see the test above.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

        Assert.Equal(
            [
                "The short version",                                    // 0
                "What is watch and wait?",                              // 1
                "Why would my team suggest it?",                        // 2
                "What happens, step by step",                           // 3
                "How often, and for how long?",                         // 4
                "What is it like to be watched?",                       // 5, as a role
                "Worry and low mood, and what you can do",              // 6a
                "What if it grows while we are watching?",              // 6c
                "How can they know what it is without a sample?",       // 6c
                "What you can do while you are being watched",          // 7, as a role
                "Who reads it, and how do I get the result?",           // 9
                "What to ask your team",                                // 10
                "Where to go next",                                     // 11
            ],
            headings);

        // The reason given for dropping slot 8 is only true while this link is.
        Assert.Contains("/tests/follow-up-scans#again-so-soon", Reader(Section("What happens, step by step")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheAnchorsArePinned()
    {
        // §12.8 (WI-508): heading anchors are a published interface.
        foreach (var anchor in new[]
                 {
                     "{#short-version}", "{#what-it-is}", "{#why}", "{#step-by-step}", "{#how-often}",
                     "{#what-it-is-like}", "{#worry}", "{#if-it-grows}", "{#without-a-sample}",
                     "{#what-you-can-do}", "{#results}", "{#questions}", "{#where-to-go-next}",
                 })
        {
            Assert.Contains(anchor, Page, StringComparison.Ordinal);
        }

        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline);
        Assert.All(headings, m => Assert.Matches(@"\{#[a-z0-9-]+\}\s*$", m.Groups[1].Value.Trim()));
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone. A page whose glioma sources are
        // full of survival figures is exactly where one would arrive by accident.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void TheMisattributedCitationsAreAbsentAndSoAreTheClaimsTheyCarried()
    {
        // §12.14: banning a URL is not fixing a claim, so each ban is paired
        // with a grep for what that URL was carrying.
        var front = CuratedPage.FrontMatter(Page);
        var citedUrls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();

        foreach (var (url, why) in new[]
                 {
                     // "675 untreated meningiomas, especially under 2 cm" — 675 is
                     // a page number in a reference, and there is no 2 cm threshold.
                     ("thejns.org", "claims a cohort that is a page number"),
                     // "Most benign meningiomas grow 5.82% a year" — the mean of 21.
                     ("PMC10477988", "overstates a 21-patient mean as 'most'"),
                     // IMPACT's PROTOCOL, cited as if it had results.
                     ("PMC8768908", "is a protocol with no results"),
                     // The mirror of PMC7761113 the dossier also cited, and a
                     // known-dead domain.
                     ("mdpi.com", "is on the known-dead list"),
                     // The review the dossier credits with the ">60%
                     // self-limiting" sentence, which it does not contain.
                     ("frontiersin.org", "does not contain the sentence attributed to it"),
                     ("academic.oup.com", "is on the known-dead list"),
                     ("sciencedirect.com", "is on the known-dead list"),
                 })
        {
            Assert.False(citedUrls.Any(u => u.Contains(url, StringComparison.OrdinalIgnoreCase)),
                $"{url} is cited again, and it {why}");
        }

        foreach (var claim in new[]
                 {
                     "675", "2 cm", "two centimeters", "doubling", "5.82", "self-limiting",
                     "unaddressed", "high risk", "too risky", "calcium", "calcification",
                 })
        {
            Assert.DoesNotContain(claim, Body, StringComparison.OrdinalIgnoreCase);
        }

        // And the front matter says WHY, where the next reader of it will see
        // it (§12.13). Flattened and de-commented: the block is hard-wrapped.
        var notes = CuratedPage.Flatten(Regex.Replace(front, @"(?m)^\s*#\s?", " "));

        Assert.Contains("UNIVARIATE", notes, StringComparison.Ordinal);
        Assert.Contains("independent of management strategy", notes, StringComparison.Ordinal);
        Assert.Contains("PAGE NUMBER", notes, StringComparison.Ordinal);
        Assert.Contains("AFTER RESECTION OF THE TUMOR", notes, StringComparison.Ordinal);
        Assert.Contains("Do not publish the odds ratio", notes, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The domains the page's claims actually rest on: PMC for the
        // guidelines and studies, NCI for the definition only (§12.1 allows
        // framing), and the two charities for what it is like at patient level.
        foreach (var domain in new[]
                 {
                     "ncbi.nlm.nih.gov", "cancer.gov", "thebraintumourcharity.org", "cancerresearchuk.org",
                 })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // §12.1: NCI is cited for the definition, and the front matter says so.
        var notes = CuratedPage.Flatten(Regex.Replace(front, @"(?m)^\s*#\s?", " "));
        Assert.Contains("cited for the DEFINITION and the general framing only", notes, StringComparison.Ordinal);

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

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
public sealed class WatchAndWaitPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/watch-and-wait";

    private readonly WebApplicationFactory<Program> _factory;

    public WatchAndWaitPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Watch and wait", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // §12.8 (WI-519): a new library page nothing links to is half shipped.
        // These are the pages a reader is standing on when "we will watch it"
        // is what they have just been told. Every door is an APPENDED sentence
        // on the hub, so no hub's own assertions could break (§12.8, WI-519).
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/tumors/meningioma", "/tumors/acoustic-neuroma", "/tumors/pituitary-tumor",
                     "/tumors/low-grade-glioma", "/tumors/astrocytoma", "/tumors/glioma",
                     "/tumors/oligodendroglioma", "/tests/follow-up-scans",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/tests/mri", "/tests/follow-up-scans", "/tumors/low-grade-glioma", "/tumors/meningioma",
            "/tumors/acoustic-neuroma", "/tumors/pituitary-tumor", "/tumors", "/get-help-now");

    [Fact]
    public async Task TheDeepLinksIntoTheSiblingPagesLandOnRealAnchors() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheTermThisPageDefinesIsSuppressedHereAndStillFiresElsewhere()
    {
        // §12.8 (WI-509): this page defines "watch and wait" at length, so its
        // tooltip is noise here. WI-512's other half first: the words are in
        // the prose, so the suppression is not a no-op. Both the term and an
        // alias ("active surveillance"): suppression covers every name for a
        // term, and the alias is the one a reader's letter may use.
        var body = CuratedPage.ReaderText(CuratedPage.Read("treatments", "watch-and-wait.md"));
        Assert.Contains("Watch and wait", body, StringComparison.Ordinal);
        Assert.Contains("active surveillance", body, StringComparison.Ordinal);

        var client = _factory.CreateClient();
        Assert.DoesNotContain("def-watch-and-wait", await client.GetStringAsync(Url));

        // WI-519: a suppression must not switch off the term's only live use.
        // /tumors/astrocytoma says "watch and wait" in unlinked prose, and its
        // door to this page is hung on DIFFERENT words for exactly that reason
        // (linking a word turns its tooltip off, §12.8 WI-519).
        Assert.Contains("def-watch-and-wait", await client.GetStringAsync("/tumors/astrocytoma"));
    }
}
