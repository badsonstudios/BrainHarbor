using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-535: <c>/tumors/diffuse-midline-glioma</c>, the umbrella for DIPG, written in
/// the same item as <c>/tumors/dipg</c> because the two drift apart when written
/// separately (WI-412 already had to pin the taxonomy).
///
/// Seventeen sections per §12.3, written from §12.9 to §12.14.
///
/// **The safety decisions this page makes.** (1) The shared [ESCALATION] block
/// files new weakness as same-day; for a tumor IN the spinal cord the page carries
/// its own right-away rule after the block, with the reason attributed to what its
/// source (metastatic compression) covers. (2) Tiredness is never explained away:
/// St. Jude lists it among late-stage symptoms. (3) New swallowing trouble in the
/// brain stem is a same-day call. (4) /review round 2: every same-day line the page
/// writes itself says what changes for a reader with a shunt, because the block on
/// the same page makes the whole same-day tier right-away for them (WI-534).
///
/// **The honesty decisions.** Curability is stated in the grade section at the
/// strength the sibling hubs use, not softened inside the gate. Dordaviprone's
/// approval is accelerated, response-based, from studies of mostly adults that left
/// out DIPG and spinal tumors.
/// </summary>
public sealed class DiffuseMidlineGliomaPageContentTests
{
    private const string Slug = "tumors/diffuse-midline-glioma";

    private static string Page => CuratedPage.Read("tumors", "diffuse-midline-glioma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string RawLf => Page.Replace("\r\n", "\n");

    private const string WhatHeading = "What is a diffuse midline glioma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string LocationHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string LifeHeading = "Everyday life: school, work, seizures and tiredness";
    private const string ComesBackHeading = "If it comes back, or changes";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CaregiverHeading = "For the person caring for someone with this";

    private static readonly Regex CureFamily =
        new(@"\b(cure[sd]?|curable|incurable|curative)\b", RegexOptions.IgnoreCase);

    /// <summary>The raw (LF) text of one `##` section, heading included.</summary>
    private static string SectionRawLf(string heading)
    {
        var raw = RawLf;
        var start = raw.IndexOf($"\n## {heading}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"the page has no '## {heading}' section");
        var end = raw.IndexOf("\n## ", start + 4, StringComparison.Ordinal);
        return end < 0 ? raw[start..] : raw[start..end];
    }

    /// <summary>
    /// The blank-line-bounded paragraph inside <paramref name="heading"/> that starts
    /// with <paramref name="opening"/>. Section-scoped (/review round 2): a paragraph
    /// that moves to another section no longer satisfies its test.
    /// </summary>
    private static string ParagraphIn(string heading, string opening)
    {
        var raw = SectionRawLf(heading);
        var at = raw.IndexOf(opening, StringComparison.Ordinal);
        Assert.True(at >= 0, $"no paragraph in '{heading}' starts with '{opening}'");
        var end = raw.IndexOf("\n\n", at, StringComparison.Ordinal);
        return CuratedPage.Flatten(end < 0 ? raw[at..] : raw[at..end]);
    }

    [Fact]
    public void TheSectionsRunInTheOrderTheStandardSets()
    {
        var order = new[]
        {
            "The short version", WhatHeading, GradeHeading, LocationHeading, SymptomHeading,
            "How do doctors find out it is this?", ReportHeading, TreatmentHeading,
            "What is treatment actually like, and what is normal afterwards?", LifeHeading,
            "Follow-up scans, and what to do while you wait", ComesBackHeading, CauseHeading,
            OutlookHeading, CaregiverHeading, "What to ask your team", "Where to get support",
        };

        // "\n## ", not "## ": IndexOf("## X") also matches inside "### X".
        var positions = order
            .Select(h => (Heading: h, At: Page.IndexOf($"\n## {h}", StringComparison.Ordinal)))
            .ToArray();

        foreach (var (heading, at) in positions)
        {
            Assert.True(at > 0, $"the page has no '## {heading}' section");
        }

        for (var i = 1; i < positions.Length; i++)
        {
            Assert.True(positions[i].At > positions[i - 1].At,
                $"'{positions[i].Heading}' must come after '{positions[i - 1].Heading}'");
        }
    }

    [Fact]
    public void TheSharedBlocksEveryHubOwesAreIncluded()
    {
        Assert.Contains("[MECHANISM]", Section(LocationHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CaregiverHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheLocationAnswerComesBeforeTheSharedMechanismBlock()
    {
        // §12.11: a heading with two questions owes two answers, and the WHERE has
        // to be on the page before the block that answers the WHY. Word boundaries:
        // a bare IndexOf("pons") is satisfied by "responds".
        var section = CuratedPage.Flatten(Section(LocationHeading));
        var block = section.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(block > 0, "the section has lost [MECHANISM]");

        foreach (var place in new[] { "pons", "thalamus", "spinal cord" })
        {
            var match = Regex.Match(section, $@"\b{Regex.Escape(place)}\b", RegexOptions.IgnoreCase);
            Assert.True(match.Success && match.Index < block, $"'{place}' is not named before [MECHANISM]");
        }
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

    [Fact]
    public void TheSpinalCordRuleFollowsTheBlockAndPutsEverySignInTheRightAwayTier()
    {
        var raw = SectionRawLf(SymptomHeading);
        var block = raw.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        var rule = raw.IndexOf("If the tumor is in the spinal cord", StringComparison.Ordinal);

        Assert.True(block >= 0, "the symptoms section has lost [ESCALATION]");
        Assert.True(rule > block,
            "the spinal cord rule must come AFTER the shared block, because it overrides a tier the block sets");

        var tier = ParagraphIn(SymptomHeading, "**If the tumor is in the spinal cord");
        var signs = ParagraphIn(SymptomHeading, "So are new trouble walking");

        Assert.Contains("right-away call, not a same-day one", tier, StringComparison.Ordinal);
        // Rendered read 1: "newly weak" alone under-triaged "weakness that keeps
        // getting worse". Harness run 2 deleted this clause and nothing failed.
        Assert.Contains("weaker than it was", tier, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"\bnewly weak\b"), tier);

        // Every sign the source lists for pressure on the cord, as whole words, in
        // the sentence that puts them in the right-away tier.
        var soAre = CuratedPage.SentencesOf(signs).First();
        foreach (var sign in new[] { @"\bwalking\b", @"\bnumbness\b", @"\bback pain\b", @"\bbladder\b", @"\bbowel\b" })
        {
            Assert.Matches(new Regex(sign), soAre);
        }

        Assert.Contains("at any hour", signs, StringComparison.Ordinal);
        Assert.Contains("emergency department", signs, StringComparison.Ordinal);

        // The source is about metastatic compression: attribute, do not assert.
        Assert.Contains("When cancer presses on the spinal cord, Cancer Research UK calls it an emergency",
            signs, StringComparison.Ordinal);
        Assert.DoesNotContain("Pressure on the spinal cord is an emergency", signs, StringComparison.Ordinal);
    }

    [Fact]
    public void EverySameDayLineOnThePageHonoursTheShuntRule()
    {
        // /review round 2's blocker. The escalation block on this page says that for
        // a reader with a shunt "everything on the same-day list above is a right-away
        // call instead", and /treatments/shunts lists getting sleepier as a warning
        // sign. A same-day line the page writes itself has to say so too (WI-534: a
        // conditional wherever that reader lands). Raw page, not composed: the block
        // carries its own shunt rule.
        var sameDay = CuratedPage.SentencesOf(CuratedPage.Flatten(Regex.Replace(CuratedPage.ReaderText(Page), @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline)))
            .Where(s => Regex.IsMatch(s, @"\bsame[- ]day\b", RegexOptions.IgnoreCase))
            .ToList();

        Assert.True(sameDay.Count >= 3, $"only {sameDay.Count} same-day sentences, so this test checks almost nothing");

        foreach (var sentence in sameDay)
        {
            // Exempt: the swallowing tier (not a shunt sign), and the spinal rule,
            // which is already stronger than same-day.
            if (Regex.IsMatch(sentence, @"\bswallow|right-away call, not a same-day", RegexOptions.IgnoreCase))
            {
                continue;
            }

            // /review round 3: any mention of "shunt" passed, including the wrong
            // direction ("the next day if there is a shunt"). The clause must point at
            // right away.
            Assert.Matches(
                new Regex(@"right[- ]away[^.]{0,30}\bshunt|\bshunt[^.]{0,40}right[- ]away", RegexOptions.IgnoreCase),
                sentence);
        }
    }

    [Fact]
    public void TirednessIsNeverExplainedAwayAndPointsAtItsTier()
    {
        // /review round 1 B1: St. Jude lists "Fatigue or drowsiness" among late-stage
        // DIPG symptoms, and the escalation block's same-day tier has "Sleeping much
        // more than being awake". A tired parent picks the reassurance.
        var life = CuratedPage.Flatten(Section(LifeHeading));

        Assert.Contains("it can also come from the tumor", life, StringComparison.Ordinal);
        Assert.Contains("Sleeping much more than being awake is a same-day call, or a right-away call if there is a shunt",
            life, StringComparison.Ordinal);
        Assert.DoesNotContain("lack of effort", CuratedPage.ReaderText(Page), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NoWarningSignIsNormalisedAnywhereOnThePage()
    {
        // /review round 2: the old guard read one way only and could not cross a
        // sentence, so round 1's own blocker text ("... is common. It comes from the
        // treatment.") passed it. Both orders now, within one sentence either side,
        // and a normalising sentence near a warning sign must be answered by a tier or
        // by the other cause, within the same neighbourhood.
        var sentences = CuratedPage.SentencesOf(CuratedPage.Flatten(Regex.Replace(CuratedPage.ReaderText(Page), @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline))).ToList();
        var normaliser = new Regex(
            @"\b(normal|expected|nothing to worry|common|usual|side effects?|part of (the )?treatment|(can )?comes? from the treatment|usually (comes?|is) from)\b",
            RegexOptions.IgnoreCase);
        var symptom = new Regex(
            @"\b(headaches?|vomit\w*|throwing up|sleep\w*|tired\w*|drows\w*|weak\w*|drool\w*|swallow\w*)\b",
            RegexOptions.IgnoreCase);
        var answered = new Regex(
            @"also come from the tumor|same-day call|call your team the same day|right away|right-away|ambulance",
            RegexOptions.IgnoreCase);

        for (var i = 0; i < sentences.Count; i++)
        {
            if (!symptom.IsMatch(sentences[i]))
            {
                continue;
            }

            var near = string.Join(" ", sentences.Skip(Math.Max(0, i - 1)).Take(i == 0 ? 2 : 3));
            if (!normaliser.IsMatch(near))
            {
                continue;
            }

            var context = string.Join(" ", sentences.Skip(Math.Max(0, i - 1)).Take(4));
            Assert.True(answered.IsMatch(context),
                $"a warning sign sits beside a normalising word with no tier: \"{near}\"");
        }

        // The specific shape /review named, across up to two sentence boundaries.
        Assert.DoesNotMatch(
            new Regex(@"\b(tired\w*|drows\w*)\b(?:[^.]*\.){0,2}[^.]*(?<!can )\b(comes?|is) from the treatment", RegexOptions.IgnoreCase),
            string.Join(" ", sentences));
    }

    [Fact]
    public void NewSwallowingTroubleInTheBrainStemHasASameDayTier()
    {
        // /review round 1 B2: "worth telling the team" is not a tier. Round 2: the
        // source is about DIPG, so the rule is scoped to the brain stem on this page.
        var paragraph = ParagraphIn(CaregiverHeading, "**If the tumor is in the brain stem, watch swallowing.**");

        Assert.Contains("New trouble swallowing, or new drooling, means calling your team the same day",
            paragraph, StringComparison.Ordinal);
        Assert.Contains("into the lungs", paragraph, StringComparison.Ordinal);
        Assert.Contains("Choking, or trouble breathing, is an ambulance call", paragraph, StringComparison.Ordinal);
        Assert.DoesNotContain("worth telling the team", CuratedPage.ReaderText(Page), StringComparison.Ordinal);
    }

    [Fact]
    public void TrialsFirstNeverDelaysUrgentRadiation()
    {
        // PMC10778507: trial evaluation first "unless radiation therapy is urgently
        // needed". Every "ask about trials early" sentence carries the caveat. The
        // pattern is /review round 2's, widened to match §12.8's own wording.
        var ask = new Regex(
            @"\b(ask|asking)\b[^.]{0,40}(\btrials?\b[^.]{0,60}\b(start|first|early|before radiation)\b|\bat the start\b)",
            RegexOptions.IgnoreCase);
        var hits = CuratedPage.SentencesOf(CuratedPage.Flatten(Regex.Replace(CuratedPage.ReaderText(Page), @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline)))
            .Where(s => ask.IsMatch(s))
            .ToList();

        Assert.True(hits.Count >= 2, "the page no longer tells the reader to ask about trials early");
        foreach (var sentence in hits)
        {
            Assert.Contains("radiation your team says is urgent", sentence, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheCancerQuestionIsAnsweredInTheFirstSentence()
    {
        // Flatten(Section), as HighGradeGliomaPageTests does: ReaderText trimmed
        // "**Yes." to "es." on the first run. POSITION is the property (WI-512).
        var section = CuratedPage.Flatten(Section(GradeHeading));
        var first = CuratedPage.SentencesOf(section).First();

        Assert.Matches(new Regex(@"^\W*Yes\b"), first);
        Assert.Contains("is cancer", section, StringComparison.Ordinal);
    }

    [Fact]
    public void CurabilityIsStatedInTheGradeSectionAtTheSiblingHubsStrength()
    {
        // /review round 2: "today's treatments do not usually cure it", inside the
        // gate, was weaker than St. Jude ("DIPG has no cure at this time") and than
        // /tumors/glioma, which says it outside any gate for grade 2 and above. One
        // claim, one strength (§12.10), read from the sibling rather than a literal.
        var grade = CuratedPage.Flatten(Section(GradeHeading));
        var glioma = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "glioma.md")));

        const string landing = "Not curable is not the same as untreatable, and it is not a timeline.";
        Assert.Matches(new Regex(@"not curable", RegexOptions.IgnoreCase), glioma);
        Assert.Contains(landing, glioma, StringComparison.Ordinal);

        Assert.Contains("With today's treatments, it is not curable.", grade, StringComparison.Ordinal);
        Assert.Contains(landing, grade, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"not usually cur", RegexOptions.IgnoreCase), CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheGradeIsAlwaysFourAndTheLooksLowerGradeReadingIsAnswered()
    {
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Contains("always grade 4", section, StringComparison.Ordinal);
        Assert.Contains("a tumor with this name is grade 4", section, StringComparison.Ordinal);
        Assert.Contains("does not change the grade", section, StringComparison.Ordinal);

        // /review round 2: H3 K27-altered also covers routes that are not a gene change.
        Assert.DoesNotContain("from its gene change alone", section, StringComparison.Ordinal);
        Assert.DoesNotContain("not how long anybody has", section, StringComparison.Ordinal);

        var roman = new Regex(@"\bgrade\s+(I|II|III|IV)\b", RegexOptions.IgnoreCase);
        Assert.Matches(roman, "an older report said Grade IV"); // canary: the guard can fire
        Assert.DoesNotMatch(roman, CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void DipgIsThePontineSubsetAndNeverASynonym()
    {
        var what = CuratedPage.Flatten(Section(WhatHeading));

        Assert.Contains("when it sits in the **pons**", what, StringComparison.Ordinal);
        Assert.Contains("is not a DIPG", what, StringComparison.Ordinal);

        // The sources' conflation, quoted only to correct it, in the NEXT sentence.
        var sentences = CuratedPage.SentencesOf(CuratedPage.Flatten(Regex.Replace(CuratedPage.ReaderText(Page), @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline))).ToList();
        for (var i = 0; i < sentences.Count; i++)
        {
            if (!Regex.IsMatch(sentences[i], @"(used to be|previously|also) called DIPG", RegexOptions.IgnoreCase))
            {
                continue;
            }

            Assert.True(i + 1 < sentences.Count
                        && sentences[i + 1].Contains("only true of the ones in the pons", StringComparison.Ordinal),
                $"the conflation is repeated without its correction: \"{sentences[i]}\"");
        }

        Assert.DoesNotMatch(
            new Regex(@"DIPG,? also (called|known as) diffuse midline glioma", RegexOptions.IgnoreCase),
            CuratedPage.ReaderText(Page));

        // The older name "brain stem glioma" carries its caveat here too (§12.10).
        Assert.Contains("Brain stem glioma can also include low-grade tumors",
            CuratedPage.Flatten(Section(ReportHeading)), StringComparison.Ordinal);
    }

    [Fact]
    public void SurgeryInThePonsIsStatedAtTheDipgPagesStrength()
    {
        // /review round 2 (§12.10): "usually cannot" here against "cannot safely
        // remove DIPG" on the child page.
        var treatment = CuratedPage.Flatten(Section(TreatmentHeading));
        var dipg = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "dipg.md")));

        Assert.Contains("Surgery cannot safely remove DIPG", dipg, StringComparison.Ordinal);
        Assert.Contains("In the pons it cannot be done safely", treatment, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"usually cannot", RegexOptions.IgnoreCase), treatment);
    }

    [Fact]
    public void TheDordavipronePassageSaysWhatTheApprovalIsAndIsNot()
    {
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        foreach (var required in new[]
                 {
                     "**Dordaviprone**", "once a week", "aged 1 and over",
                     "**accelerated approval**", "how many tumors shrank and for how long",
                     "still checking whether it helps people live longer",
                     "nobody can tell you that it does",
                     "Most of the people in the studies behind the approval were adults",
                     "left out DIPG and tumors that started in the spinal cord",
                 })
        {
            Assert.Contains(required, section, StringComparison.Ordinal);
        }

        Assert.DoesNotMatch(
            new Regex(@"durabl|well[- ]tolerated|safer|breakthrough|promising|game[- ]chang|larger trial|left out tumors in the (spinal cord|pons)",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheDordaviproneConditionMatchesTheTargetedDrugsPagesOwnBullet()
    {
        // §12.10, scoped to the sibling's dordaviprone BULLET (/review round 2: a
        // character window is not the bullet).
        var raw = CuratedPage.Read("treatments", "targeted-therapy.md").Replace("\r\n", "\n");
        var start = raw.IndexOf("\n- **An H3 K27M change", StringComparison.Ordinal);
        Assert.True(start >= 0, "the targeted drugs page has lost its H3 K27M bullet");
        var end = raw.IndexOf("\n- ", start + 3, StringComparison.Ordinal);
        var bullet = CuratedPage.Flatten(end < 0 ? raw[start..] : raw[start..end]);

        Assert.Contains("kept growing after", bullet, StringComparison.Ordinal);
        Assert.Contains("Dordaviprone", bullet, StringComparison.Ordinal);

        Assert.Contains("has kept growing after other treatment",
            CuratedPage.Flatten(Section(TreatmentHeading)), StringComparison.Ordinal);
    }

    [Fact]
    public void TheChemotherapyNoteIsHonestAndNamesNoStandard()
    {
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("have not been shown to help", section, StringComparison.Ordinal);
        Assert.Contains("there is no standard plan", section, StringComparison.Ordinal);
        Assert.DoesNotContain("often as part of a trial", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReirradiationDisagreementIsPrintedAndAttributedToWhatTheReviewSays()
    {
        var section = CuratedPage.Flatten(Section(ComesBackHeading));

        Assert.Contains("doctors disagree about", section, StringComparison.Ordinal);
        Assert.Contains("can be done safely", section, StringComparison.Ordinal);
        Assert.Contains("A 2022 review of DIPG says it can help slow the tumor", section, StringComparison.Ordinal);
        Assert.Contains("significant risks and burdens", section, StringComparison.Ordinal);
        Assert.Contains("not usually recommended", section, StringComparison.Ordinal);

        // /review round 2: the slowing was a SIOP analysis the review cites, not the
        // review's own finding.
        Assert.DoesNotMatch(new Regex(@"review found", RegexOptions.IgnoreCase), section);

        // The radiation page's second-course section describes "around a year or
        // more" of gap, which does not fit this tumor.
        Assert.DoesNotContain("#why-am-i-having-radiation-again", section, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        var reader = CuratedPage.ReaderText(Composed);

        Assert.DoesNotMatch(new Regex(@"\b(median|survival|survive)\b[^.]{0,40}\d", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\d[^.]{0,40}\b(median|survival|survive)\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"survival rate", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*%", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"(one|two|three|five|ten)[- ]year", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\b(median|survival|survive|live|lived)\b[^.]{0,60}\b(one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|fifteen|eighteen|twenty)\b",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(new Regex(@"per cent|percent", RegexOptions.IgnoreCase), reader);

        // Round 1: a count of months or years, digits or words.
        Assert.DoesNotMatch(
            new Regex(@"\b(\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|eighteen|twenty|thirty)\s+(to\s+\w+\s+)?(months?|years?)\b",
                RegexOptions.IgnoreCase),
            reader);

        // Round 2: "a year", "a few months" near a life or death word. "Most children
        // do not live a year." passed everything above.
        Assert.DoesNotMatch(
            new Regex(@"\b(live|lives|lived|living|survive\w*|die|dies|died|dying)\b[^.]{0,60}\b(a|a few|several|many|some|about a|less than a|more than a)\s+(months?|years?)\b",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(
            new Regex(@"\b(a|a few|several|many|some)\s+(months?|years?)\b[^.]{0,60}\b(live|lives|lived|survive\w*|die|dies|died)\b",
                RegexOptions.IgnoreCase),
            reader);
    }

    [Fact]
    public void TheOutlookGateTeachesTheVocabularyAndNamesThisPagesCaveats()
    {
        var section = CuratedPage.Flatten(Section(OutlookHeading));
        var gate = Regex.Match(section, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");

        var inside = gate.Groups[1].Value;
        // The DEFINITION, not the word (harness run 1).
        Assert.Contains("A median is the middle of a group.", inside, StringComparison.Ordinal);
        Assert.Contains("the median is the person standing in the middle", inside, StringComparison.Ordinal);
        Assert.Contains("not a prediction about any one person", inside, StringComparison.Ordinal);

        Assert.Contains("grouped tumors by where they sat", inside, StringComparison.Ordinal);
        Assert.Contains("studied less in adults than in children", inside, StringComparison.Ordinal);

        // /review round 2: the right tail, scoped and hedged as PMC11640674 hedges it.
        // /review round 3: "Some" was stronger than the DIPG page's "A few children".
        Assert.Contains("A few live well beyond it", inside, StringComparison.Ordinal);
        Assert.DoesNotContain("Some live well beyond it", inside, StringComparison.Ordinal);
        Assert.Contains("Some studies suggest the tumor can move more slowly in adults", inside, StringComparison.Ordinal);
        Assert.Contains("though it varies widely", inside, StringComparison.Ordinal);

        Assert.DoesNotContain("shapes your own situation", inside, StringComparison.Ordinal);
        // Curability lives in the grade section now, at the sibling hubs' strength.
        Assert.DoesNotMatch(CureFamily, inside);
    }

    [Fact]
    public void TheGatedWordsAppearNowhereOutsideTheGate()
    {
        // COMPOSED minus the gate (/review round 2: text arriving through a block was
        // never checked), case-insensitive, with the euphemisms and kindnesses outlook
        // arrives as.
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        var gate = Regex.Match(whole, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");
        var outside = whole.Replace(gate.Value, "", StringComparison.Ordinal);

        foreach (var gated in new[]
                 {
                     @"\bmedian\b", @"well beyond", @"end[- ]of[- ]life", @"\bhospice\b", @"\bwish",
                     @"\b(die|dies|died|death|dying)\b", @"\bterminal\b", @"life expectancy",
                     @"how long\b[^.?]{0,30}\b(have|live|left)\b", @"good days", @"something special",
                     @"use the time", @"\bnot at the end\b",
                 })
        {
            Assert.DoesNotMatch(new Regex(gated, RegexOptions.IgnoreCase), outside);
        }

        // The cure family outside the gate: only the grade section's two sentences.
        var allowed = new[]
        {
            "With today's treatments, it is not curable.",
            "Not curable is not the same as untreatable, and it is not a timeline.",
        };
        foreach (var sentence in allowed)
        {
            // Exactly once (/review round 3): Replace removes every copy, so a second
            // copy anywhere outside the gate used to pass.
            Assert.Equal(1, Regex.Matches(outside, Regex.Escape(sentence)).Count);
            outside = outside.Replace(sentence, "", StringComparison.Ordinal);
        }

        Assert.DoesNotMatch(CureFamily, outside);
    }

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        CuratedPage.AssertNeverMinimises(CuratedPage.ReaderText(Composed), Slug);

        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var idiom in new[] { "out of hours", "A&E", "GP", "straight away", "feeling sick", "being sick", "catches people out", "caught out" })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheFrontMatterCarriesTheContractAndNoBarredSource()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"^reviewed: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline), front);
        Assert.Matches(new Regex(@"^review_due: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline), front);

        var urls = Regex.Matches(front, @"^\s+- url: (\S+)", RegexOptions.Multiline);
        Assert.True(urls.Count >= 10, $"only {urls.Count} sources");
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}", RegexOptions.Multiline).Count);

        // §12.1: NCI patient PDQ never governs naming; the stub cited it for everything.
        Assert.DoesNotContain("cancer.gov/types/brain", front, StringComparison.Ordinal);
        foreach (var barred in new[] { "virtualtrials.org", "healthline.com", "verywell" })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("cancerresearchuk.org/about-cancer/coping/physically/spinal-cord-compression",
            front, StringComparison.Ordinal);
        Assert.Contains("drug-trials-snapshots-modeyso", front, StringComparison.Ordinal);
    }
}

[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class DiffuseMidlineGliomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/diffuse-midline-glioma";

    private readonly WebApplicationFactory<Program> _factory;

    public DiffuseMidlineGliomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var response = await _factory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await response.Content.ReadAsStringAsync()).Length > 20_000,
            "a 200 with a near-empty body is not a page (WI-533)");
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/seizures/what-to-do", "/seizures/living-with");

    [Fact]
    public async Task EveryFragmentLinkLandsOnAnAnchorThatExists() =>
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task EveryBlockDirectiveComposesIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var directive in new[]
                 { "[MECHANISM]", "[CROSSWALK]", "[CAUSES]", "[CAREGIVER]", "[TUMOR-BOARD]", "[ESCALATION]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        foreach (var canary in new[]
                 {
                     "closed box", "Call your team the same day", "Roman numeral grades",
                     "nobody knows the cause", "tumor board", "caring for someone",
                     // The shunt rule the [MECHANISM] door obliges (WI-534).
                     "If you have a shunt",
                 })
        {
            Assert.Contains(canary, html, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task TheOutlookGateRendersClosedAndLeaksNoFence()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.DoesNotContain(":::", html, StringComparison.Ordinal);
        Assert.Contains("<details", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"<details[^>]*\bopen\b"), html);
    }

    [Fact]
    public async Task ThePagesNewVocabularyFiresAndItsOwnSubjectDoesNot()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "pons", "thalamus", "palliative-care" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }

        // The H3 K27 entry's tooltip says "called a diffuse midline glioma". On
        // this page that is the page defining itself in a popover (WI-533).
        Assert.DoesNotContain("def-h3-k27-altered", html, StringComparison.Ordinal);
    }
}
