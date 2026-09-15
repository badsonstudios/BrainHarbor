using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-535: <c>/tumors/dipg</c>, the pontine child of
/// <c>/tumors/diffuse-midline-glioma</c>, written in the same item so the two
/// cannot drift (WI-412 pinned the taxonomy; this pins the prose).
///
/// **What this page owns and the umbrella does not:** the MRI-only diagnosis and
/// the biopsy CHOICE, the length of radiation, the stretch after radiation,
/// anesthesia for a young child, palliative care early, end-of-life text behind the
/// gate, and the one fact that changes the dordaviprone conversation for a tumor in
/// the pons: the FDA's efficacy studies excluded "Patients with diffuse intrinsic
/// pontine glioma" and were mostly adults.
///
/// **What it routes or shares rather than restates (§12.10):** the grade
/// explanation, the chemotherapy note, and the approval sentences, which are shared
/// word for word with the umbrella and pinned below.
///
/// **/review's blockers all landed on this page:** round 1 found tiredness explained
/// away, swallowing without a tier and a wish-organization line outside the gate;
/// round 2 found the new same-day lines silent about a reader with a shunt.
/// </summary>
public sealed class DipgPageContentTests
{
    private const string Slug = "tumors/dipg";

    private static string Page => CuratedPage.Read("tumors", "dipg.md");

    private static string Umbrella => CuratedPage.Read("tumors", "diffuse-midline-glioma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, Slug);

    private static string RawLf => Page.Replace("\r\n", "\n");

    private const string WhatHeading = "What is DIPG?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string LocationHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string FindingOutHeading = "How do doctors find out it is this? {#finding-out}";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string LikeHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string LifeHeading = "Everyday life: school, seizures and tiredness";
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

    /// <summary>The blank-line-bounded paragraph inside a section that starts with <paramref name="opening"/>.</summary>
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
            FindingOutHeading, ReportHeading, TreatmentHeading, LikeHeading, LifeHeading,
            "Follow-up scans, and what to do while you wait", "If it comes back, or changes",
            CauseHeading, OutlookHeading, CaregiverHeading, "What to ask your team", "Where to get support",
        };

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
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo() =>
        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);

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
        // /review round 2: St. Jude says "DIPG has no cure at this time"; /tumors/glioma
        // says "not curable" outside any gate. Read from the sibling (§12.10).
        var grade = CuratedPage.Flatten(Section(GradeHeading));
        var glioma = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("tumors", "glioma.md")));

        const string landing = "Not curable is not the same as untreatable, and it is not a timeline.";
        Assert.Contains(landing, glioma, StringComparison.Ordinal);

        Assert.Contains("With today's treatments, DIPG is not curable.", grade, StringComparison.Ordinal);
        Assert.Contains(landing, grade, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"not usually cur", RegexOptions.IgnoreCase), CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheGradeRuleIsScopedToTheTumorsItAppliesTo()
    {
        // Most DIPGs carry the H3 K27 change, not all. The grade rule and the "looks
        // low grade" answer are scoped to the ones that do.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Contains("**A DIPG with the H3 K27 change is grade 4**", section, StringComparison.Ordinal);
        Assert.Contains("For a DIPG with the H3 K27 change, it does not change the grade.", section, StringComparison.Ordinal);
        Assert.DoesNotContain("not how long anybody has", section, StringComparison.Ordinal);
    }

    [Fact]
    public void DipgIsNamedByPlaceAndNeverAsASynonymForTheUmbrella()
    {
        var what = CuratedPage.Flatten(Section(WhatHeading));

        Assert.Contains("says **where** the tumor is", what, StringComparison.Ordinal);
        Assert.Contains("those are not DIPG", what, StringComparison.Ordinal);
        Assert.Contains("/tumors/diffuse-midline-glioma", what, StringComparison.Ordinal);

        Assert.DoesNotMatch(
            new Regex(@"(also (called|known as)|previously called|used to be called) (DIPG|diffuse midline glioma)",
                RegexOptions.IgnoreCase),
            CuratedPage.ReaderText(Page));

        var roman = new Regex(@"\bgrade\s+(I|II|III|IV)\b", RegexOptions.IgnoreCase);
        Assert.Matches(roman, "an older report said Grade IV"); // canary: the guard can fire
        Assert.DoesNotMatch(roman, CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheBiopsyIsPresentedAsAChoiceWithBothSides()
    {
        var section = CuratedPage.Flatten(Section(FindingOutHeading));

        foreach (var required in new[]
                 {
                     "diagnosed from the scan and the symptoms alone",
                     "a real choice, and centers differ",
                     "can be done safely", "still some risk",
                     "gene result", "without one",
                 })
        {
            Assert.Contains(required, section, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheRadiationLengthIsTheSourcesAndTheHardFactLandsOnAnAction()
    {
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Contains("about six to seven weeks", section, StringComparison.Ordinal);
        Assert.Contains("about three weeks", section, StringComparison.Ordinal);

        // §12.6: warn before you disclose, and never end on the frightening sentence.
        var warning = section.IndexOf("One hard thing to know before radiation starts", StringComparison.Ordinal);
        var fact = section.IndexOf("It is not a cure.", StringComparison.Ordinal);
        Assert.True(warning >= 0 && fact > warning, "the hard fact is not preceded by its warning");

        var after = section[(fact + "It is not a cure.".Length)..];
        Assert.StartsWith(" Ask your team what radiation is meant to do for your child.", after, StringComparison.Ordinal);

        // /review round 2: "helps you plan what matters", after "not a cure" and
        // outside the gate, tells a reader who declined outlook what is coming.
        Assert.DoesNotContain("plan what matters", CuratedPage.ReaderText(Page), StringComparison.Ordinal);
    }

    [Fact]
    public void ThePonsExclusionIsStatedBesideTheDrugAndScopedToTheEvidence()
    {
        var paragraph = ParagraphIn(TreatmentHeading, "**The studies the FDA used to judge whether it shrinks tumors");

        Assert.Contains("left out DIPG.", paragraph, StringComparison.Ordinal);
        Assert.Contains("Most of the people in them were adults", paragraph, StringComparison.Ordinal);
        Assert.Contains("American Cancer Society says it may be an option", paragraph, StringComparison.Ordinal);
        Assert.Contains("the evidence that it shrinks tumors comes from other tumors", paragraph, StringComparison.Ordinal);

        // /review: several studies, not "the study"; the safety population did include
        // DIPG; and the FDA excluded DIPG, not every tumor in the pons.
        var reader = CuratedPage.ReaderText(Page);
        Assert.DoesNotContain("The study the FDA", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("the evidence for it comes", reader, StringComparison.Ordinal);
        Assert.DoesNotContain("left out tumors in the pons", CuratedPage.Flatten(reader), StringComparison.Ordinal);

        Assert.Contains("/tumors/diffuse-midline-glioma#dordaviprone",
            CuratedPage.Flatten(Section(TreatmentHeading)), StringComparison.Ordinal);

        // §12.11: strip the comment markers BEFORE matching across a wrapped comment.
        var front = Regex.Replace(CuratedPage.FrontMatter(Page), @"\r?\n[ \t]*#[ \t]*", " ");
        Assert.Contains("Patients with diffuse intrinsic pontine glioma, primary spinal", front,
            StringComparison.Ordinal);

        Assert.Contains("/tumors/dipg#dordaviprone", Umbrella, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBetterStretchIsNotPromisedAndRoutesBackToARealTier()
    {
        var stretch = ParagraphIn(LikeHeading, "**The better stretch after radiation.**");
        var comingBack = ParagraphIn(LikeHeading, "If symptoms that had eased start to come back");

        Assert.Contains("hard to predict", stretch, StringComparison.Ordinal);
        Assert.Contains("do not improve at all", stretch, StringComparison.Ordinal);

        Assert.Contains("call your team the same day", comingBack, StringComparison.Ordinal);
        Assert.Contains("right away if there is a shunt", comingBack, StringComparison.Ordinal);
        Assert.Contains("Anything on the ambulance list is still an ambulance call", comingBack, StringComparison.Ordinal);
        Assert.DoesNotContain("call right away still applies", CuratedPage.ReaderText(Page), StringComparison.Ordinal);
    }

    [Fact]
    public void EverySameDayLineOnThePageHonoursTheShuntRule()
    {
        // /review round 2's blocker: the page mentions shunts, the block on the same
        // page makes the whole same-day tier right-away for a shunt reader, and the
        // page's own same-day lines said nothing about it.
        var sameDay = CuratedPage.SentencesOf(CuratedPage.Flatten(Regex.Replace(CuratedPage.ReaderText(Page), @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline)))
            .Where(s => Regex.IsMatch(s, @"\bsame[- ]day\b", RegexOptions.IgnoreCase))
            .ToList();

        Assert.True(sameDay.Count >= 3, $"only {sameDay.Count} same-day sentences, so this test checks almost nothing");

        foreach (var sentence in sameDay)
        {
            if (Regex.IsMatch(sentence, @"\bswallow", RegexOptions.IgnoreCase))
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
    public void NoWarningSignIsNormalisedAnywhereOnThePage()
    {
        // /review round 2: both orders, within one sentence either side, answered by a
        // tier or by the other cause; and the exact two-sentence shape of round 1's B1.
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

        Assert.DoesNotMatch(
            new Regex(@"\b(tired\w*|drows\w*)\b(?:[^.]*\.){0,2}[^.]*(?<!can )\b(comes?|is) from the treatment", RegexOptions.IgnoreCase),
            string.Join(" ", sentences));
    }

    [Fact]
    public void TirednessIsNeverExplainedAwayAndPointsAtItsTier()
    {
        var life = CuratedPage.Flatten(Section(LifeHeading));

        Assert.Contains("it can also come from the tumor", life, StringComparison.Ordinal);
        Assert.Contains("Sleeping much more than being awake is a same-day call, or a right-away call if there is a shunt",
            life, StringComparison.Ordinal);
    }

    [Fact]
    public void NewSwallowingTroubleHasASameDayTierAndChokingStaysAnAmbulanceCall()
    {
        var paragraph = ParagraphIn(CaregiverHeading, "**Watch swallowing.**");

        Assert.Contains("New trouble swallowing, or new drooling, means calling your team the same day",
            paragraph, StringComparison.Ordinal);
        Assert.Contains("into the lungs", paragraph, StringComparison.Ordinal);
        Assert.Contains("Choking, or trouble breathing, is an ambulance call", paragraph, StringComparison.Ordinal);
        Assert.DoesNotContain("worth telling the team", CuratedPage.ReaderText(Page), StringComparison.Ordinal);
    }

    [Fact]
    public void TrialsFirstNeverDelaysUrgentRadiation()
    {
        // /review round 2: this page only checked that the caveat appeared somewhere,
        // so cutting it from the short version stayed green.
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
    public void AnesthesiaForAYoungChildIsNamedAndNeverCalledPuttingToSleep()
    {
        var section = CuratedPage.Flatten(Section(LikeHeading));

        Assert.Contains("given medicine to sleep through each session", section, StringComparison.Ordinal);
        Assert.Contains("anesthesia", section, StringComparison.Ordinal);
        Assert.Contains("practice lying still", section, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"put\w* to sleep", RegexOptions.IgnoreCase), CuratedPage.ReaderText(Page));
    }

    [Fact]
    public void TheSharedClaimsAreWordedIdenticallyOnBothPages()
    {
        // §12.10: the umbrella and the child say these things once, at one strength.
        var child = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        var parent = CuratedPage.Flatten(CuratedPage.ReaderText(Umbrella));

        foreach (var shared in new[]
                 {
                     "Weight gain and a round, puffy face come mainly from the medicine, not from the tumor.",
                     "Asking at the start keeps the most options open",
                     "radiation your team says is urgent",
                     "**Radiation a second time is something doctors disagree about.**",
                     "A 2022 review of DIPG says it can help slow the tumor, but it has significant risks and burdens, and is not usually recommended.",
                     "**No choice here is the wrong one.**",
                     "This tumor is not passed down in families.",
                     "Not curable is not the same as untreatable, and it is not a timeline.",
                     "hearing the grade without hearing this can leave people shocked later",
                     "Treatment can ease symptoms and slow the tumor for a time, and there are still choices if it grows.",
                     "not a prediction about any one person",
                     "It can come from the treatment, and it can also come from the tumor.",
                     "Sleeping much more than being awake is a same-day call, or a right-away call if there is a shunt.",
                     "New trouble swallowing, or new drooling, means calling your team the same day.",
                     "The team is meant to be there from the start.",
                     // /review round 2: this page said "approved" without "accelerated".
                     "It was given **accelerated approval**.",
                     "That means it was approved early, based on how many tumors shrank and for how long.",
                     "**Trials are still checking whether it helps people live longer.**",
                     // /review round 3: the caveat is needed most on the page with the weakest evidence.
                     "Until they report, nobody can tell you that it does.",
                 })
        {
            var flat = CuratedPage.Flatten(shared);
            Assert.Contains(flat, child, StringComparison.Ordinal);
            Assert.Contains(flat, parent, StringComparison.Ordinal);
        }
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

        Assert.DoesNotMatch(
            new Regex(@"\b(\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|eighteen|twenty|thirty)\s+(to\s+\w+\s+)?(months?|years?)\b",
                RegexOptions.IgnoreCase),
            reader);

        // /review round 2: "Most children do not live a year." passed everything above.
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
    public void TheOutlookGateTeachesTheVocabularyAndHoldsEverythingAboutTheEnd()
    {
        // COMPOSED minus the gate (/review round 2), case-insensitive, with the
        // euphemisms and kindnesses outlook arrives as.
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        var gate = Regex.Match(whole, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");

        var inside = gate.Groups[1].Value;
        Assert.Contains("A median is the middle of a group.", inside, StringComparison.Ordinal);
        Assert.Contains("the median is the person standing in the middle", inside, StringComparison.Ordinal);
        Assert.Contains("not a prediction about any one person", inside, StringComparison.Ordinal);
        Assert.Contains("before the gene test", inside, StringComparison.Ordinal);

        // Both halves of St. Jude's qualifier on the right tail (/review round 2).
        Assert.Contains("live well beyond it", inside, StringComparison.Ordinal);
        Assert.Contains("turned out not to be DIPG", inside, StringComparison.Ordinal);
        Assert.Contains("very young children", inside, StringComparison.Ordinal);

        foreach (var inWords in new[] { "end of life", "hospice", "median", "well beyond", "Wish-granting", "use the time" })
        {
            Assert.Contains(inWords, inside, StringComparison.OrdinalIgnoreCase);
        }

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

        // The cure family outside the gate: the grade section's two sentences and the
        // warned sentence before radiation, nothing else.
        var allowed = new[]
        {
            "With today's treatments, DIPG is not curable.",
            "Not curable is not the same as untreatable, and it is not a timeline.",
            "It is not a cure.",
        };
        foreach (var sentence in allowed)
        {
            // Exactly once (/review round 3): Replace removes every copy, so a second
            // copy anywhere outside the gate used to pass.
            Assert.Equal(1, Regex.Matches(outside, Regex.Escape(sentence)).Count);
            outside = outside.Replace(sentence, "", StringComparison.Ordinal);
        }

        Assert.DoesNotMatch(CureFamily, outside);
        Assert.DoesNotMatch(CureFamily, inside);
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

        foreach (var idiom in new[] { "out of hours", "A&E", "straight away", "feeling sick", "being sick", "catches people out", "caught out" })
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

        Assert.DoesNotContain("cancer.gov/types/brain", front, StringComparison.Ordinal);
        foreach (var barred in new[] { "virtualtrials.org", "healthline.com", "verywell" })
        {
            Assert.DoesNotContain(barred, front, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("drug-trials-snapshots-modeyso", front, StringComparison.Ordinal);
    }
}

[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class DipgPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/dipg";

    private readonly WebApplicationFactory<Program> _factory;

    public DipgPageRenderTests(WebApplicationFactory<Program> factory) =>
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
            "/get-help-now", "/seizures/what-to-do");

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
                     "nobody knows the cause", "tumor board", "caring for someone", "If you have a shunt",
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
    public async Task ThePagesNewVocabularyFiresAsTooltips()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "pons", "palliative-care", "h3-k27-altered" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }
    }
}
