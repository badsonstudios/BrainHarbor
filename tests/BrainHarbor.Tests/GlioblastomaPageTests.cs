using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-518: <c>/tumors/glioblastoma</c> — the hub built around ONE growth story
/// that explains four separate things at once, and the third Wave 2 page.
///
/// Seventeen sections per §12.3, written from §12.9 to §12.13.
///
/// **§12.13's problem again, in a new shape.** Two of this page's sources carry
/// the superseded name *glioblastoma multiforme* in their own titles, so they
/// are scoped in the front matter and never cited for a name or a grade —
/// StatPearls for the scan appearance and the surgical limit, PMC12467656 for
/// the radiation schedule, MGMT and the device. And the NBTS page publishes
/// one-, five- and ten-year relative survival rates, which §12.2 item 5 forbids
/// carrying; §12.1 already names an NBTS page as a never-use for prognosis
/// figures for exactly this reason.
///
/// **The engine had no reachable source and had to be re-sourced.** The dossier
/// hangs the whole mechanism on `academic.oup.com/jnen`, which returns 403 with
/// a 5,558-byte shell — the third item to find that domain closed. PMC12564729
/// and PMC2588896 carry the chain between them, in open access.
/// </summary>
public sealed class GlioblastomaPageContentTests
{
    private static string Page => CuratedPage.Read("tumors", "glioblastoma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Composed => CuratedPage.Composed(Page, "tumors/glioblastoma");

    private const string WhatHeading = "What is a glioblastoma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string LocationHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatmentHeading = "How is it usually treated?";
    private const string ScanHeading = "Follow-up scans, and what to do while you wait";
    private const string CauseHeading = "Did I cause this?";
    private const string OutlookHeading = "What might happen over time";
    private const string CaregiverHeading = "For the person caring for someone with this";

    [Fact]
    public void TheGrowthStoryExplainsAllFourThingsItPromisesTo()
    {
        // The item's whole reason for existing. Necrosis, the ring, the swelling
        // and the relentless growth are four things a reader is told separately
        // by every comparator, and they are one mechanism. A page that lists
        // them without connecting them has done the frightening half.
        var section = CuratedPage.Flatten(Section(LocationHeading));

        // The chain, and ORDER is asserted rather than described. The first
        // version's comment said "in order" and checked nothing of the kind:
        // shuffling the three paragraphs passed.
        var chain = new[]
        {
            @"grows more quickly than it can build blood\s*vessels",
            @"starve and die",
            @"[Ll]ow oxygen switches on a chemical alarm",
            @"build new\s*blood vessels",
            @"immature, badly formed\s*and leaky",
        };

        var at = -1;
        foreach (var step in chain)
        {
            var hit = Regex.Match(section, step, RegexOptions.IgnoreCase);
            Assert.True(hit.Success, $"the growth story is missing a step: {step}");
            Assert.True(hit.Index > at,
                $"the growth story step '{step}' has moved out of order — the chain only "
                + "explains anything if it runs in sequence");
            at = hit.Index;
        }

        // Each payoff needs its LABEL and its EXPLANATION. Asserting the bold
        // label alone let the sentence underneath be deleted (§12.10).
        (string Label, string Explanation)[] payoffs =
        [
            (@"\*\*The ring on your scan\.\*\*", @"lights up the living rim"),
            (@"\*\*The swelling\.\*\*", @"leak out of those same\s*vessels"),
            (@"\*\*Why it keeps growing\.\*\*", @"cycle starts again a little further out"),
            (@"\*\*Why an operation cannot get all of it\.\*\*",
                @"past the edge that shows up on the scan"),
        ];

        foreach (var (label, explanation) in payoffs)
        {
            Assert.Matches(new Regex(label, RegexOptions.IgnoreCase), section);
            Assert.Matches(new Regex(explanation, RegexOptions.IgnoreCase), section);
        }

        // §12.6: the section must not end on the frightening one.
        Assert.Matches(
            new Regex(@"[Nn]one of that is a reason to expect less of an operation",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheLocationAnswerComesBeforeTheSharedMechanismBlock()
    {
        // §12.11, and the defect WI-517 shipped: a heading with two questions
        // owes two answers, and the "where" has to be on the page BEFORE the
        // block, for the reader who stops after the first paragraph.
        var section = Section(LocationHeading);

        var mechanism = section.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        var where = section.IndexOf("upper part of the brain", StringComparison.OrdinalIgnoreCase);

        Assert.True(where > 0, "the location section never says where this tumor grows");
        Assert.True(
            where < mechanism,
            "the shared [MECHANISM] block comes before this page's own answer to "
            + "'where does it grow' — §12.11");
    }

    [Fact]
    public void ThePageSaysItIsAlwaysGradeFourAndNeverAnythingElse()
    {
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(new Regex(@"[Ii]t is always grade 4", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"no grade 1, 2 or 3 glioblastoma", RegexOptions.IgnoreCase), section);

        // §12.3: answer in cell-behaviour terms, never survival terms.
        Assert.Matches(
            new Regex(@"describes how the cells look and behave rather than\s*how long anybody has",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"[Gg]rade 4 is not a prediction about you", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheCancerQuestionIsAnsweredInTheOpeningOfItsSection()
    {
        // §12.6: answer first, and the frightening half of the heading first.
        var sentences = CuratedPage.SentencesOf(Section(GradeHeading));

        Assert.Contains(
            sentences.Take(2),
            s => Regex.IsMatch(s, @"is cancer", RegexOptions.IgnoreCase));
        Assert.Contains(
            sentences.Take(3),
            s => Regex.IsMatch(s, @"\bmalignant\b", RegexOptions.IgnoreCase));
    }

    [Fact]
    public void TheMolecularGlioblastomaAnswerIsOnThePageAndDoesNotBlameAnybody()
    {
        // The dossier's own framing: "People will be told 'grade 4' after being
        // told their tumour 'looked lower grade.' Explain it or they will assume
        // someone made a mistake." So the section owes both the mechanism AND
        // the reassurance that nothing went wrong.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(
            new Regex(@"called a glioblastoma on its \*\*gene results alone\*\*", RegexOptions.IgnoreCase),
            section);

        // All three of the qualifying findings, named. A reader holding the
        // report needs to recognise which one is theirs.
        foreach (var marker in new[] { "TERT", "EGFR", "chromosome 7", "chromosome 10" })
        {
            Assert.Contains(marker, section, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Matches(
            new Regex(@"[Nn]obody\s*changed their mind and nobody got it wrong", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void MgmtIsSeparatedFromTheDiagnosisBecauseReadersConflateThem()
    {
        // The dossier flags this explicitly: "Readers routinely confuse the
        // two." MGMT guides a treatment decision; it does not name or grade the
        // tumor, and a reader who thinks it does will read their own report
        // wrongly.
        var section = CuratedPage.Flatten(Section(ReportHeading));

        Assert.Matches(
            new Regex(@"\*\*MGMT is not part of the\s*diagnosis\.\*\*", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"does not decide what your tumor is called", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheRenameSliceRoutesTheIdhMutantReaderToTheOtherPage()
    {
        // §12.2 item 3 and §12.9. This hub owes TWO retired names and they are
        // not the same kind of thing, which is the angle (§12.11):
        //
        //   - "glioblastoma multiforme" / GBM is cosmetic. Same tumor.
        //   - "secondary glioblastoma" / "glioblastoma, IDH-mutant" is NOT.
        //     Those readers have a different diagnosis today and a different
        //     page, and the treatment conversation differs.
        //
        // Getting the second one wrong sends somebody to the wrong page about
        // their own tumor.
        var section = CuratedPage.Flatten(Section(ReportHeading));

        // THREE bullets, because there are three different things and the first
        // draft fused two of them. §12.12: "'Secondary' described how the tumor
        // AROSE, and not every secondary glioblastoma is IDH-mutant." Writing
        // "Secondary glioblastoma, OR glioblastoma IDH-mutant" asserts they are
        // the same, and then tells every holder of the older word that their
        // diagnosis has changed — which is false for the IDH-wildtype ones, who
        // /tumors/astrocytoma routes to THIS page.
        Assert.Matches(
            new Regex(@"- \*\*Glioblastoma multiforme\*\*", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"- \*\*Glioblastoma, IDH-mutant\.\*\*", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"- \*\*Secondary glioblastoma\.\*\*", RegexOptions.IgnoreCase), section);

        // And the distinction itself, which is the whole point.
        Assert.Matches(
            new Regex(@"[Ss]ame tumor under older wording", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"described how a tumor \*\*arose\*\*", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"did not say anything about IDH", RegexOptions.IgnoreCase), section);

        // Never the fused form.
        Assert.DoesNotMatch(
            new Regex(@"[Ss]econdary glioblastoma\*?\*?,? or \*?\*?glioblastoma, IDH-mutant",
                RegexOptions.IgnoreCase),
            section);

        // And the routing, which is the actionable half.
        Assert.Matches(
            new Regex(@"astrocytoma, IDH-mutant, grade 4", RegexOptions.IgnoreCase), section);
        Assert.Contains("/tumors/astrocytoma", section, StringComparison.Ordinal);

        // The sibling is READ (§12.10): if the astrocytoma page ever stops
        // accepting this handover, the routing goes red rather than stale.
        // The sibling's RULE, not merely that it mentions the phrase. The first
        // version asserted the mention, which is why the two pages could
        // contradict each other on the routing and this stayed green — the
        // exact defect it was written to prevent.
        var astrocytoma = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "astrocytoma.md")));

        Assert.True(
            Regex.IsMatch(astrocytoma, @"secondary glioblastoma", RegexOptions.IgnoreCase),
            "/tumors/astrocytoma no longer mentions 'secondary glioblastoma', so this page is "
            + "routing those readers somewhere that no longer explains their rename");
        Assert.True(
            Regex.IsMatch(astrocytoma,
                @"IDH-mutant means this page\. IDH-wildtype means", RegexOptions.IgnoreCase),
            "/tumors/astrocytoma no longer routes by the IDH line, so this page's rename slice "
            + "and its sibling may now disagree about who belongs where");

        // This page must route the same way, or a reader bounces between two
        // hubs that contradict each other.
        Assert.Matches(
            new Regex(@"go by the IDH line, not by the old word", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheSurgicalLimitIsExplainedByTheGrowthStoryRatherThanAsserted()
    {
        // "Why couldn't they remove all of it" is question 3 in the dossier.
        // Answering it with "because it is infiltrative" is a restatement. The
        // page owes the reason, and it owes the reader protection from
        // concluding the surgeon underperformed.
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(
            new Regex(@"already spread past the edge that shows on\s*the scan", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"not a failure of the operation or the surgeon", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"asking\s*for a bigger operation does not fix it", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheDrugAndTheDeviceAreBothNamed()
    {
        // §12.11: name the thing. WI-515's blocker was a device paragraph that
        // told the reader to ask about an unnamed thing, and WI-517's review
        // found the same shape with an unnamed trial. This page's first draft
        // had BOTH — "there is a wearable device" and "there is a drug used for
        // the swelling" — and a reader cannot ask about either.
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(new Regex(@"[Tt]umor treating fields", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"Optune", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Bb]evacizumab", RegexOptions.IgnoreCase), section);

        Assert.DoesNotMatch(
            new Regex(@"[Tt]here is a (wearable )?device that|[Tt]here is a drug used for",
                RegexOptions.IgnoreCase),
            section);

        // WHOLE PAGE, not just the treatment section. The recurrence section
        // said "a different drug" and this guard could not see it, because it
        // was scoped to one heading and its negative was two literal phrasings.
        // Third occurrence of WI-515's unnamed-thing defect.
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        Assert.DoesNotMatch(
            new Regex(@"a different drug\b|another drug\b|a drug that|some drugs", RegexOptions.IgnoreCase),
            whole);
    }

    [Fact]
    public void TheBevacizumabNoteKeepsTheScanAndTheOutcomeApart()
    {
        // The dossier calls this "a good honest example of 'helps the scan, not
        // the outcome' for the anti-hype framing", and it is the clearest
        // anti-hype opportunity on the page. Softening it into "it can help"
        // would be the over-reassuring direction (§12.12).
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(
            new Regex(@"\*\*did not help people live longer\*\*", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"difference between a treatment that improves a picture and one that changes an\s*outcome",
                RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheShorterRadiationCourseIsNotFramedAsGivingUp()
    {
        // The source calls hypofractionation "the preferred standard of care"
        // for this group, with comparable survival and less toxicity. A reader
        // offered it will otherwise hear "they have stopped trying", which is
        // both wrong and cruel.
        var section = CuratedPage.Flatten(Section(TreatmentHeading));

        Assert.Matches(
            new Regex(@"preferred plan for people over about\s*seventy", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(
            new Regex(@"not a smaller version handed\s*out to save trouble", RegexOptions.IgnoreCase),
            section);
        Assert.DoesNotMatch(
            new Regex(@"less aggressive treatment|palliative course|nothing more to", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void PseudoprogressionIsExplainedAndPointsAtThePageThatOwnsIt()
    {
        // WI-517 shipped this link pointing at a page that never uses the word.
        // The link resolved 200 and no test could see it.
        var section = CuratedPage.Flatten(Section(ScanHeading));

        Assert.Matches(
            new Regex(@"worse-looking scan does not always mean the tumor has grown",
                RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"pseudoprogression", RegexOptions.IgnoreCase), section);

        // It must not be softened into a picture-only problem: the source says
        // it can cause real symptoms.
        Assert.Matches(
            new Regex(@"can cause real\s*symptoms", RegexOptions.IgnoreCase), section);

        // The destination is READ, not assumed.
        var target = Regex.Match(section, @"\[[Pp]seudoprogression\]\((/[^)#]+)");
        Assert.True(target.Success, "pseudoprogression is not linked anywhere in this section");

        var parts = target.Groups[1].Value.TrimStart('/').Split('/');
        parts[^1] += ".md";
        var slug = string.Join('/', parts);
        var destination = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(parts)));
        Assert.True(
            Regex.IsMatch(destination, @"pseudoprogression", RegexOptions.IgnoreCase),
            $"/{slug} does not use the word 'pseudoprogression', so this link lands the reader "
            + "on a page that cannot answer the question it was clicked for");
    }

    [Fact]
    public void TheInheritanceAnswerIsGivenRatherThanRoutedAway()
    {
        // Question 2 in the dossier: "Will my children get this." The answer is
        // that the defining changes are somatic — in the tumor, not the person.
        // The dossier says to make it explicit, and it is the answer people are
        // most afraid to ask out loud.
        var section = CuratedPage.Flatten(Section(CauseHeading));

        Assert.Matches(
            new Regex(@"[Ww]ill my children get this", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"changes \*\*in the tumor\*\*, not\s*in you", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not passed on", RegexOptions.IgnoreCase), section);

        // And the honest other half, or the reassurance is the kind that stops
        // being true (WI-509's rule on the somatic/germline block).
        Assert.Matches(
            new Regex(@"inherited condition that raises the\s*chance", RegexOptions.IgnoreCase),
            section);
        // US spelling: the British-forms gate correctly caught "counselling"
        // in the first draft of this page.
        Assert.Matches(new Regex(@"genetic\s*counseling", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // Two of this page's sources publish them: NBTS gives one-, five- and
        // ten-year relative survival rates, and PMC12467656 gives medians in
        // months. Both are cited for other things.
        var reader = CuratedPage.ReaderText(Composed);

        Assert.DoesNotMatch(
            new Regex(@"\b(median|survival)\b[^.]{0,40}\d", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\d[^.]{0,40}\b(median|survival)\b", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"survival rate", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*%", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"(one|two|three|five|ten)[- ]year", RegexOptions.IgnoreCase), reader);

        // Written as WORDS. This corpus spells numbers out ("thirty sessions",
        // "seventy"), so a digit-only guard is blind to the style the page
        // actually uses: "median survival is about fifteen months" passed every
        // assertion above.
        Assert.DoesNotMatch(
            new Regex(@"\b(median|survival)\b[^.]{0,60}\b(one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|fifteen|eighteen|twenty|thirty)\b",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(new Regex(@"per cent", RegexOptions.IgnoreCase), reader);
    }

    [Fact]
    public void NoRomanNumeralGradeAndTheSupersededNameIsAlwaysMarkedAsOld()
    {
        // Two of this page's sources carry "glioblastoma multiforme" in their
        // TITLES, which render as visible link text under Sources. The page
        // itself may use the name only where it is explaining that it is the
        // old one — never as its own voice.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        // IgnoreCase. §12.12 recorded this trap's fourth occurrence one item
        // before WI-517 shipped a fifth.
        Assert.DoesNotMatch(new Regex(@"[Gg]rade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase), reader);

        // The canary: prove the pattern can fire at all.
        Assert.Matches(
            new Regex(@"[Gg]rade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(File.ReadAllText(
                Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md"))));

        // Negation-aware, and the loop must not be vacuous (§12.10, WI-517):
        // there has to be at least one occurrence for the check to mean
        // anything, because this page is REQUIRED to name the old term.
        var hits = Regex.Matches(reader, @"glioblastoma multiforme", RegexOptions.IgnoreCase);
        Assert.True(hits.Count > 0,
            "the page never names 'glioblastoma multiforme', so the reader holding a report or "
            + "a source title with that word on it is not told it is the same tumor");

        // SENTENCE-SCOPED, not a character window. The first version took ±220
        // characters either side, and the break harness proved that useless:
        // planting the superseded name in an unrelated symptom bullet still
        // passed, because a 440-character window on a page whose whole subject
        // is old names will find a marker somewhere almost every time. WI-511's
        // clause anchor, one page later.
        //
        // SAME SENTENCE ONLY. An earlier version allowed the marker to sit in
        // the NEXT sentence, so the heading could carry the term and be
        // explained beneath it — and the break harness proved that leaky too:
        // planting the name in a symptom bullet passed because the bullet after
        // it says "especially common in OLDER people". The markers are ordinary
        // English words, so any adjacency allowance leaks. The heading carries
        // its own marker instead.
        var sentences = CuratedPage.SentencesOf(reader);

        for (var i = 0; i < sentences.Length; i++)
        {
            if (!sentences[i].Contains("glioblastoma multiforme", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var scope = sentences[i];

            Assert.True(
                // \b on every branch. Bare "old" matches TOLD, hold, sold, bold —
                // and this page says "you will be told" repeatedly.
                Regex.IsMatch(scope,
                    @"\bolder\b|\bold\b|\bsuperseded\b|\bpreviously\b|\bsame (tumor|diagnosis)\b|\bused to\b",
                    RegexOptions.IgnoreCase),
                $"'glioblastoma multiforme' is used without anything marking it as the older "
                + $"name: {scope}");
        }
    }

    [Fact]
    public void TheFrontMatterCitesTheCns5SourcesAndNoneOfTheDeadOnes()
    {
        // §12.11's closing rule: every citation surface the item ships, not
        // just the page. This item shipped a glossary entry too.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("PMC9723092", front, StringComparison.Ordinal);
        Assert.Contains("PMC10216527", front, StringComparison.Ordinal);
        Assert.Contains("PMC12564729", front, StringComparison.Ordinal);

        // YAML comment lines stripped first. This page's front matter EXPLAINS
        // which sources are dead and why, and a comment naming a banned domain
        // is not a citation to it — nothing parses it and nothing renders it.
        // The first version of this check failed on its own explanation.
        static string WithoutComments(string yaml) =>
            string.Join("\n", yaml.Split('\n')
                .Where(line => !line.TrimStart().StartsWith('#')));

        var surfaces = new List<(string Where, string Text)>
        {
            ("tumors/glioblastoma front matter", WithoutComments(front)),
        };
        foreach (var (slug, text) in CuratedPage.SharedSources())
        {
            surfaces.Add((slug, WithoutComments(text)));
        }

        foreach (var bad in new[]
                 {
                     // Re-confirmed 403 with a 5,558-byte shell at WI-518. The
                     // dossier hangs this page's ENTIRE mechanism on it.
                     "academic.oup.com",
                     // 403, 402 bytes. WI-514 found it returning a 1-byte shell.
                     "mdpi.com",
                     // WI-514: a UK pharmacovigilance audit of steroid
                     // COMPLICATIONS, cited by the dossier for steroids working
                     // within a day or two, which it does not say.
                     "PMC12406498",
                     // WI-515 dropped it: a 21-patient single-centre study.
                     "PMC3643853",
                     "ascopubs.org", "moffitt.org", "drugs.com",
                     "cancer.gov/types/brain/patient", "cancer.gov/types/brain/hp",
                 })
        {
            foreach (var (where, text) in surfaces)
            {
                Assert.False(
                    text.Contains(bad, StringComparison.OrdinalIgnoreCase),
                    $"{bad} is cited in {where} — it is unreachable, banned by §12.1, or does "
                    + "not carry the claim it was cited for");
            }
        }

        // Scoped to THIS PAGE, not to every shared surface. NBK559184 is the
        // StatPearls OLIGODENDROGLIOMA chapter, which the dossier cites here
        // for 5-ALA and bevacizumab — the wrong tumor's chapter for a general
        // claim. It is legitimately cited by /tumors/oligodendroglioma and by
        // the calcification glossary entry, where it IS the right chapter
        // (§12.1's check is per-claim, not per-domain, and §12.13 records the
        // distinction). The first version of this test banned it site-wide and
        // failed on a correct citation — a rule that fails a correct page is
        // worse than no rule.
        Assert.DoesNotContain("NBK559184", WithoutComments(front), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheSharedBlocksEveryHubOwesAreIncluded()
    {
        Assert.Contains("[MECHANISM]", Section(LocationHeading), StringComparison.Ordinal);
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CaregiverHeading), StringComparison.Ordinal);
        Assert.Contains("[TUMOR-BOARD]", Section(TreatmentHeading), StringComparison.Ordinal);

        // WI-563, pinned to the SECTION so relocating it goes red.
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo() =>
        CuratedPage.AssertEscalationTiers(Page, "tumors/glioblastoma", SymptomHeading);

    [Fact]
    public void TheOutlookGateTeachesTheVocabularyAndNamesTheClassificationCaveat()
    {
        var section = CuratedPage.Flatten(Section(OutlookHeading));

        Assert.Contains(":::outlook", section, StringComparison.Ordinal);

        var gate = Regex.Match(section, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");

        var inside = gate.Groups[1].Value;
        Assert.Matches(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), inside);
        Assert.Matches(
            new Regex(@"not a prediction about any one person", RegexOptions.IgnoreCase), inside);
        Assert.Matches(new Regex(@"long tail", RegexOptions.IgnoreCase), inside);

        // The caveat that is specific to THIS page: the 2021 rules moved a
        // group of tumors OUT of this diagnosis, so older figures describe a
        // mixed group. Without it the numbers a reader finds elsewhere look
        // more applicable than they are.
        Assert.Matches(
            new Regex(@"moved a group of tumors out of this diagnosis", RegexOptions.IgnoreCase),
            inside);
    }

    [Fact]
    public void TheGatedWordsAppearNowhereOutsideTheGate()
    {
        // The WHOLE page minus the fence. WI-513's defect was the right-tail
        // sentence sitting four screens ABOVE the gate, which a section-scoped
        // check cannot see (WI-517 shipped that narrower version).
        var whole = CuratedPage.Flatten(CuratedPage.ReaderText(Page));
        var gate = Regex.Match(whole, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook gate is not closed");

        var outside = whole.Replace(gate.Value, "", StringComparison.Ordinal);

        Assert.DoesNotMatch(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), outside);
        Assert.DoesNotMatch(new Regex(@"long tail", RegexOptions.IgnoreCase), outside);
    }

    [Fact]
    public void ThePageNeverMinimisesAndCarriesNoCharacterisations()
    {
        CuratedPage.AssertNeverMinimises(
            CuratedPage.ReaderText(Composed), "tumors/glioblastoma");

        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishFormsOrIdiom()
    {
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // WI-563/WI-517: the spelling list does not hold idiom, and idiom is
        // what actually misleads. WI-564 owns the corpus sweep.
        foreach (var idiom in new[]
                 {
                     "out of hours", "advice line", "A&E", "being sick", "come round",
                 })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheSectionsRunInTheOrderTheStandardSets()
    {
        var order = new[]
        {
            "The short version", WhatHeading, GradeHeading, LocationHeading, SymptomHeading,
            "How do doctors find out it is this?", ReportHeading, TreatmentHeading,
            "What is treatment actually like, and what is normal afterwards?",
            "Everyday life: work, driving, seizures and tiredness",
            ScanHeading, "If it comes back, or changes", CauseHeading, OutlookHeading,
            CaregiverHeading, "What to ask your team", "Where to get support",
        };

        // "\n## ", not "## ": IndexOf("## X") matches inside "### X" at offset
        // one, so demoting a section to H3 left WI-517's version green.
        var positions = order
            .Select(h => (Heading: h, At: Page.IndexOf($"\n## {h}", StringComparison.Ordinal)))
            .ToArray();

        foreach (var (heading, at) in positions)
        {
            Assert.True(at > 0, $"the page has no '## {heading}' section");
        }

        for (var i = 1; i < positions.Length; i++)
        {
            Assert.True(
                positions[i].At > positions[i - 1].At,
                $"'{positions[i].Heading}' must come after '{positions[i - 1].Heading}'");
        }
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);
        Assert.Contains("slug: tumors/glioblastoma", front, StringComparison.Ordinal);
    }
}

/// <summary>
/// The render half. <c>[Collection(DatabaseCollection.Name)]</c> is required —
/// see <see cref="TestCollectionHygieneTests"/> for the 397-failure CI run that
/// established why.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GlioblastomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/glioblastoma";

    private readonly WebApplicationFactory<Program> _factory;

    public GlioblastomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var response = await _factory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

        // One canary word PER block (WI-517): checking two of six left four
        // blocks able to empty without a single test going red.
        foreach (var canary in new[]
                 {
                     "closed box",                    // mechanism
                     "Call your team the same day",   // escalation
                     "Roman numeral grades",          // crosswalk
                     "nobody knows the cause",        // causes
                     "tumor board",                   // tumor-board
                     "caring for someone",            // caregiver
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
        Assert.Contains("What might happen over time", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePagesNewVocabularyFiresAsTooltips()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "temozolomide", "mgmt-methylation", "microvascular-proliferation" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }

        // NOTE: there is deliberately no `necrosis` glossary entry. One was
        // written for this item and withdrawn: the term collides with the
        // existing `radiation-necrosis` and would fire inside it, which
        // ShippedGlossaryTests catches. The page defines the word inline
        // instead. An assertion about suppressing a term that does not exist
        // would be a no-op (WI-515, WI-512), so there is none.
    }
}
