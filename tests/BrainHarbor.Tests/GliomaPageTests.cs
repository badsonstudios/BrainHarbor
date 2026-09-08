using System.Net;
using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-514: the glioma umbrella — the family tree and the router, and the
/// canonical home for the shared <c>[CROSSWALK]</c> and <c>[MECHANISM]</c>
/// blocks (SYNTHESIS §4.3: canonical here, "with the per-tumor slice repeated
/// only where it differs"; confirmed by Dan 2026-09-07 over WI-501's
/// contradicting note).
///
/// This page follows **§12.3's seventeen-section hub order**, not §12.8's
/// twelve-slot library template — see §12.9. It is the second hub written to
/// that standard and the first written FROM it rather than proving it.
///
/// The umbrella's job is different from every other hub's: it is a **router**.
/// A reader who lands here mostly needs to find out which of the other pages is
/// theirs. So the assertions below care as much about where the page sends
/// people as about what it says.
/// </summary>
public sealed class GliomaPageContentTests
{
    private static string Page => CuratedPage.Read("tumors", "glioma.md");

    private static string Reader => CuratedPage.ReaderText(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The page as a reader meets it: blocks resolved. Every rule about the
    /// crosswalk's or the mechanism's words has to run on this, not on the raw
    /// source, where those sections are the literal text "[CROSSWALK]".
    /// </summary>
    private static string Composed => CuratedPage.Composed(Page, "tumors/glioma");

    private const string FamilyHeading = "What is a glioma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string OutlookHeading = "What might happen over time";
    private const string SymptomHeading = "What symptoms does it cause?";

    [Fact]
    public void TheOpeningSaysGliomaIsAFamilyNameRatherThanADiagnosis()
    {
        // The single organising fact of the page. A reader holding paperwork
        // that says only "glioma" has been told the neighborhood and not the
        // address, and every other section depends on them understanding that
        // before they read on.
        var shortVersion = CuratedPage.Flatten(Section("The short version"));

        Assert.Matches(new Regex(@"family name", RegexOptions.IgnoreCase), shortVersion);
        Assert.Matches(
            new Regex(@"not a diagnosis|and not the address", RegexOptions.IgnoreCase), shortVersion);

        // And it must point at the pathology report as the place the real name
        // lives, rather than leaving the reader to guess where to look.
        Assert.Matches(new Regex(@"pathology report", RegexOptions.IgnoreCase), shortVersion);
    }

    [Fact]
    public void AdultTypeAndPediatricTypeAreExplainedAsBiologyNotTheReadersAge()
    {
        // Research §0.2 calls this "CRITICAL and almost universally
        // misexplained". Adults are diagnosed with pediatric-type gliomas and
        // children with adult-type ones; in one series of 596 adults, 20 had a
        // pediatric-type glioma (PMC10196618). A reader who reads "pediatric-
        // type" on their own report and is not told this concludes their report
        // belongs to somebody else.
        var section = CuratedPage.Flatten(Section(FamilyHeading));

        Assert.Matches(new Regex(@"adult-type", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"pediatric-type", RegexOptions.IgnoreCase), section);

        // The claim itself, and it has to be the biology-not-age direction.
        Assert.Matches(
            new Regex(@"biology,? not the age|not the age of the person", RegexOptions.IgnoreCase),
            section);

        // And it must say so in BOTH directions. Saying only "adults can have
        // pediatric-type tumors" leaves the parent of a child with an
        // adult-type glioma exactly where they started.
        Assert.Matches(new Regex(@"[Aa]dults are sometimes", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"[Cc]hildren are sometimes", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageRoutesToEveryDiagnosisItNamesAsPartOfTheFamily()
    {
        // The router test. This page's whole reason to exist is sending a
        // reader to the page that is actually theirs, so naming a diagnosis
        // and not linking it is the defect this page most needs to avoid.
        var section = Section(FamilyHeading);

        foreach (var slug in new[]
                 {
                     "/tumors/astrocytoma",
                     "/tumors/oligodendroglioma",
                     "/tumors/glioblastoma",
                     "/tumors/ependymoma",
                     "/tumors/low-grade-glioma",
                     "/tumors/high-grade-glioma",
                 })
        {
            Assert.Contains(slug, section, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ThePageSeparatesGroupingsFromRealDiagnoses()
    {
        // "Low-grade glioma", "high-grade glioma" and "diffuse glioma" are
        // groupings, not diagnoses (research §1.1). A reader told any of them
        // has still not been told what they have, and the fair thing is to say
        // so and hand them the question to ask.
        var section = CuratedPage.Flatten(Section(FamilyHeading));

        Assert.Matches(
            new Regex(@"sound like diagnoses but are not|describe a group rather than name",
                RegexOptions.IgnoreCase),
            section);

        // The reader is left holding a usable question, not just a caution.
        Assert.Matches(
            new Regex(@"what is the full name of mine|fair to ask", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheCancerQuestionIsAnsweredBeforeTheGradeQuestion()
    {
        // §12.6: answer in the first sentence under the heading. The heading
        // asks two questions and §12.3 calls the first one "the documented core
        // confusion" — so a section that opens by defining what a grade is has
        // answered the easier one and left the frightening one four paragraphs
        // down.
        //
        // The first draft did worse than that: it answered "it depends which
        // one" and routed to five child pages, insisting the answer "is not a
        // dodge". Those five pages are Wave-0 stubs; the words cancer,
        // malignant, benign and curable appear in none of their bodies. A
        // forward reference is only an answer if the destination answers.
        var section = CuratedPage.Flatten(Section(GradeHeading));
        var opening = CuratedPage.SentencesOf(section).Take(2).ToArray();

        Assert.Contains(opening, s =>
            Regex.IsMatch(s, @"are cancer and some are not|is cancer", RegexOptions.IgnoreCase));

        // The two words a reader will actually be handed by a clinician.
        Assert.Matches(new Regex(@"\bmalignant\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"\bbenign\b", RegexOptions.IgnoreCase), section);

        // Research §1.7 Q1: "why 'benign' in the brain is not the same as
        // harmless". Leaving this out is the reassurance that stops being true.
        Assert.Matches(
            new Regex(@"benign in the brain does not mean\s+harmless", RegexOptions.IgnoreCase),
            section);

        // And it must not send the reader elsewhere for the answer.
        Assert.DoesNotMatch(
            new Regex(@"is where that question gets answered", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void CurabilityIsStatedAtTheSameStrengthAsTheChildPage()
    {
        // WI-513 wrote "they are not curable. That is a hard sentence and it is
        // here on purpose, because the alternative is worse" onto
        // /tumors/low-grade-glioma, and wrote a paragraph explaining why the
        // blunt version had to stay. The umbrella is the page more readers land
        // on first, and its first draft rendered the same fact as "treated as a
        // long-term condition" — quietly reversing a decision made one item
        // earlier, on the more-read page.
        //
        // Compared against the child page rather than a literal, so the two
        // cannot drift apart again without something going red.
        var section = CuratedPage.Flatten(Section(GradeHeading));
        var child = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("tumors", "low-grade-glioma.md")));

        Assert.True(
            Regex.IsMatch(child, @"they are not curable", RegexOptions.IgnoreCase),
            "/tumors/low-grade-glioma no longer says 'they are not curable', so the strength "
            + "this page was matched to has moved and both need rechecking");

        // The CLAIM, not merely the phrase. Asserting "not curable" appears
        // somewhere passed on a page whose only use of the words was the
        // softening line below it ("Not curable is not the same as
        // untreatable") — so a draft that deleted the hard sentence entirely
        // stayed green. Proven by deleting exactly that.
        Assert.Matches(
            new Regex(@"they are not curable", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"\bmalignant\b", RegexOptions.IgnoreCase), section);

        // Both halves. "Not curable" without this is a sentence that reads as a
        // timeline, which it is not (research §1.5).
        Assert.Matches(
            new Regex(@"not the same as untreatable", RegexOptions.IgnoreCase), section);

        // And the honest other half of the family: grade 1 gliomas genuinely
        // can be cured by surgery (StatPearls). A page that says only the hard
        // thing is as wrong as one that says only the soft thing.
        Assert.Matches(new Regex(@"cured by surgery", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheGradeSectionSaysGradeIsNotComparableAcrossDifferentTumors()
    {
        // WHO CNS5's own words (PMC9723092): "grading is now done within tumor
        // types ... there is not necessarily perfect equivalence between the
        // same numerical grade in different types of tumors", and its own
        // example is that a grade 4 medulloblastoma does not mean what a grade
        // 4 glioblastoma means. On the page that names six different tumors,
        // omitting this invites exactly the comparison it forbids.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(new Regex(@"within each tumor type|within tumor type", RegexOptions.IgnoreCase), section);
        Assert.Matches(
            new Regex(@"does not mean the same thing|not the same across", RegexOptions.IgnoreCase),
            section);
    }

    [Fact]
    public void TheGradeSectionSaysGeneResultsCanSetTheGrade()
    {
        // The reverse consequence of CNS5 (research §0.3): a tumor that looked
        // lower grade can be graded higher on molecular grounds alone. Without
        // it, a reader whose grade went up when the gene results came back
        // reads their own report as a contradiction or a mistake.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(
            new Regex(@"[Gg]ene results can now set the grade|gene result.{0,40}set the grade",
                RegexOptions.IgnoreCase),
            section);
        Assert.Contains("/tests/molecular-markers", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGradeSectionAnswersWhyThereIsNoStage()
    {
        // Research §0.4 and §1.7 question 2. Every other cancer a reader has
        // heard of has a stage; being handed a grade instead reads as an
        // omission unless somebody explains it.
        var section = CuratedPage.Flatten(Section(GradeHeading));

        Assert.Matches(new Regex(@"no stage|but no stage", RegexOptions.IgnoreCase), section);

        // And the reason has to be the biological one — that the thing staging
        // measures mostly does not happen here — rather than a bare assertion.
        Assert.Matches(
            new Regex(@"rarely leaves|spread through the body", RegexOptions.IgnoreCase), section);

        // Honest about the exception rather than absolute: extracranial spread
        // is reported, just rare enough to be written up (PMC10073898). "Never"
        // would be a claim the source does not support.
        Assert.Matches(
            new Regex(@"[Cc]ases have been reported|unusual enough", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheSharedBlocksThisPageIsCanonicalHomeForAreActuallyIncluded()
    {
        // WI-514's headline deliverable. Both blocks are NEW here, and a hub
        // that spelled either out inline instead would put the content most
        // likely to need correcting later into twenty-four separate files —
        // which is the whole problem WI-501 built the include mechanism to
        // solve, and the crosswalk changes every time WHO or cIMPACT-NOW moves.
        Assert.Contains("[CROSSWALK]", Section(ReportHeading), StringComparison.Ordinal);
        Assert.Contains(
            "[MECHANISM]",
            Section("Where does it grow, and why does it cause these symptoms?"),
            StringComparison.Ordinal);

        // And the two the contract requires of every hub (§12.3 deliberate
        // omissions; contract item 11), which WI-513's first draft omitted.
        Assert.Contains("[CAUSES]", Section("Did I cause this?"), StringComparison.Ordinal);
        Assert.Contains(
            "[CAREGIVER]",
            Section("For the person caring for someone with this"),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheCrosswalkBlockComposesIntoThisPageCarryingEveryRetiredName()
    {
        // Proving the include actually resolves, on the page that owns it. A
        // directive that silently failed to expand would leave the highest-
        // value block on the site as the literal characters "[CROSSWALK]".
        var section = CuratedPage.Flatten(CuratedPage.ComposedSection(Page, ReportHeading));

        // The BLOCK's own content first. Everything below this comes from the
        // page's glioma-specific slice, so an earlier version of this test
        // passed with the block emptied out — it was checking the page and
        // calling it the block. These three are what every hub inherits.
        Assert.Matches(
            new Regex(@"changed in 2021", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"\bNOS\b"), section);
        Assert.Matches(new Regex(@"\bNEC\b"), section);

        foreach (var retired in new[]
                 {
                     "anaplastic",
                     "diffuse astrocytoma",
                     "oligoastrocytoma",
                     "secondary glioblastoma",
                     "glioblastoma multiforme",
                     "DIPG",
                 })
        {
            Assert.Contains(retired, section, StringComparison.OrdinalIgnoreCase);
        }

        // The directive itself must be gone once composed.
        Assert.DoesNotContain("[CROSSWALK]", section, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryRetiredNameIsNamedAsRetiredRatherThanUsedAsALiveDiagnosis()
    {
        // §12.9's warning, and the reason WI-513's first draft's version of this
        // test was dangerous. It banned these words as substrings, passed, and
        // would have been copied onto all twenty-three remaining hubs —
        // forbidding the crosswalk with a test whose comment said it enforced
        // CNS5. There are two completely different things a retired name can be
        // doing on a page: used as a live diagnosis (forbidden) or NAMED as
        // retired (required, and this page is where it is most required).
        //
        // Asserted on the COMPOSED page: the names live in the shared block, so
        // the raw source contains none of them and this would prove nothing.
        //
        // Scoped to the CHUNK the name sits in — a list item or a paragraph —
        // and not to a character window around it. The first version used
        // +/-220 characters and was a false pass waiting to happen: the
        // crosswalk block is a dense list of negations, so a retired name used
        // as a live diagnosis in the section NEXT to it read "no longer" from
        // the crosswalk and went green. Proven by planting exactly that.
        foreach (var retired in new[] { "oligoastrocytoma", "anaplastic", "mixed glioma", "multiforme" })
        {
            foreach (var chunk in ChunksContaining(Composed, retired))
            {
                Assert.True(
                    Regex.IsMatch(chunk,
                        @"retired|no longer|old rules|older report|older paperwork|used to be|"
                        + @"changed in 2021|before that|still hear|still say|dropped|moved out from",
                        RegexOptions.IgnoreCase),
                    $"'{retired}' is used as a live diagnosis rather than named as a retired term, "
                    + $"in: {chunk}");
            }
        }
    }

    /// <summary>
    /// The list items and paragraphs of a page that mention <paramref name="term"/>.
    ///
    /// The unit a claim is made in. A character window spanning a blank line
    /// reads a neighbouring section's words as though they qualified this
    /// sentence, which is how a negation-aware check passes on a page that
    /// states the opposite one paragraph away.
    /// </summary>
    private static IEnumerable<string> ChunksContaining(string markdown, string term) =>
        Regex.Split(markdown.Replace("\r\n", "\n", StringComparison.Ordinal), @"\n\s*\n|\n(?=\s*[-*] )")
            .Select(CuratedPage.Flatten)
            .Where(chunk => chunk.Contains(term, StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void RomanNumeralGradesAppearOnlyAsTheOldNotationBeingTranslated()
    {
        // Same negation-aware treatment, same reason: the crosswalk has to be
        // able to PRINT "grade II" — that is the string a reader is holding —
        // while the page must never use one as a live grade. Chunk-scoped for
        // the same reason as the retired names above.
        var chunks = Regex.Split(Composed.Replace("\r\n", "\n", StringComparison.Ordinal),
                @"\n\s*\n|\n(?=\s*[-*] )")
            .Select(CuratedPage.Flatten);

        var checkedChunks = 0;
        foreach (var chunk in chunks)
        {
            if (!Regex.IsMatch(chunk, @"grade\s+(I{1,3}V?|IV)\b", RegexOptions.IgnoreCase))
            {
                continue;
            }

            checkedChunks++;
            Assert.True(
                Regex.IsMatch(chunk,
                    @"retired|no longer|old rules|older report|now written|ordinary numbers|Roman",
                    RegexOptions.IgnoreCase),
                $"a Roman-numeral grade is used as a live grade rather than shown as the old "
                + $"style, in: {chunk}");
        }

        // The page is SUPPOSED to print Roman numerals once, in the crosswalk.
        // If it stops doing that this test starts passing by finding nothing,
        // which is the failure mode WI-506's link-check canary was written for.
        Assert.True(checkedChunks > 0,
            "no Roman numeral appears anywhere on the composed page, so this proved nothing — "
            + "the crosswalk is supposed to print 'grade II' for the reader holding one");
    }

    [Fact]
    public void TheCrosswalkTeachesBothDirectionsOfTheGlioblastomaRename()
    {
        // Research §0.3's two "headline consequences", and they are the two
        // things that make a reader think somebody made a mistake:
        //   - an IDH-mutant tumor is NEVER called glioblastoma now, so a 2016
        //     report saying "secondary glioblastoma" was renamed, not
        //     re-diagnosed;
        //   - and the reverse, a tumor can be called grade 4 on gene results
        //     alone, so a reader can be told grade 4 when nobody saw the
        //     changes that used to be required.
        // Only one of the two is the reassuring direction. Shipping that one
        // alone would be the site picking the comfortable half.
        var composed = CuratedPage.Flatten(Composed);

        Assert.Matches(
            new Regex(@"never called glioblastoma", RegexOptions.IgnoreCase), composed);
        Assert.Matches(
            new Regex(@"renamed|not been given a different disease", RegexOptions.IgnoreCase), composed);
        Assert.Matches(
            new Regex(@"runs the other way|even when the cells did not look", RegexOptions.IgnoreCase),
            composed);
    }

    [Fact]
    public void TheMechanismBlockExplainsWhyGotItAllIsNotTheSameAsCured()
    {
        // Research §0.6 calls infiltration "the single most important fact for
        // explaining why 'they got it all' is not the same thing as cured", and
        // the Frontiers source says it plainly: tumor cells "invade beyond
        // regions visible as abnormal areas on MRI", so imaging understates the
        // true extent. A reader who is not told this hears a later recurrence
        // as their surgeon having missed something.
        var composed = CuratedPage.Flatten(Composed);

        Assert.Matches(
            new Regex(@"got it all.{0,40}cured|cured.{0,40}got it all", RegexOptions.IgnoreCase),
            composed);
        Assert.Matches(
            new Regex(@"further than the abnormal area|understates|not the true edge",
                RegexOptions.IgnoreCase),
            composed);

        // And it must not land as blame on the surgeon.
        Assert.Matches(
            new Regex(@"not that your surgeon missed", RegexOptions.IgnoreCase), composed);
    }

    [Fact]
    public void TheMechanismBlockSeparatesSwellingFromTheTumorItself()
    {
        // The patient-critical implication of research §0.6, in the Springer
        // source's own words: "edema plays a major role in determining symptoms
        // caused by tumors". It is the one part of the mechanism a reader can
        // act on — it explains why somebody can improve without the tumor
        // having changed — and it is what makes a steroid conversation make
        // sense rather than sound like a trick.
        var composed = CuratedPage.Flatten(Composed);

        // Hedged wording ("may not be") is deliberate and required: this is a
        // block for 24 hubs, and swelling is not every tumor's story.
        Assert.Matches(
            new Regex(@"not be the tumor itself|be the swelling around it", RegexOptions.IgnoreCase),
            composed);
        Assert.Matches(
            new Regex(@"without\s+the\s+tumor being any different", RegexOptions.IgnoreCase), composed);
    }

    [Fact]
    public void TheEscalationTiersMatchTheSiblingPagesTheySendPeopleTo()
    {
        // WI-563: the tiers moved into Content/blocks/escalation.md and this
        // assertion moved with them. It ran here as three byte-identical copies
        // that had ALREADY drifted apart — this one lacked checks the others had
        // — which is the same failure as the prose it guards, one layer up.
        CuratedPage.AssertEscalationTiers(Page, "tumors/glioma", SymptomHeading);
    }

    [Fact]
    public void EveryWarningSignTheMechanismBlockTeachesHasAnActionOnThisPage()
    {
        // §12.6: action at the end of every frightening block. The mechanism
        // block names the raised-pressure pattern a reader should watch for —
        // and a page that teaches somebody to recognise drowsiness or vomiting
        // as a warning sign, then never tells them what to do about it, has
        // done the frightening half and skipped the useful half.
        // Both sides read the COMPOSED page. WI-563 moved the action lists into
        // [ESCALATION], and the first run of this test after that move failed
        // on "Sleeping much more" — asserting against the raw page was
        // asserting against the literal string "[ESCALATION]" (§12.10, WI-514's
        // trap). The pairing is now block-to-block and neither half can go
        // stale silently.
        var symptoms = CuratedPage.Flatten(CuratedPage.ComposedSection(Page, SymptomHeading));
        var mechanism = CuratedPage.Flatten(Composed);

        foreach (var (taught, acted) in new[]
                 {
                     ("very drowsy", "Sleeping much more"),
                     // WI-563: the action line reads "Throwing up again and
                     // again" now. "Being sick" is British for vomiting and
                     // reads as "being unwell" to this page's audience, which
                     // is the wrong half of a same-day escalation trigger.
                     ("being sick", "Throwing up again and again"),
                     ("worse in the\n  morning", "wakes you from sleep"),
                 })
        {
            var flatTaught = CuratedPage.Flatten(taught);
            Assert.True(
                mechanism.Contains(flatTaught, StringComparison.OrdinalIgnoreCase),
                $"the mechanism block no longer teaches '{flatTaught}', so this pairing is stale");
            Assert.True(
                symptoms.Contains(acted, StringComparison.OrdinalIgnoreCase),
                $"'{flatTaught}' is taught as a warning sign but '{acted}' is not in an action list");
        }
    }

    [Fact]
    public void ThePageCarriesNoPrognosisFiguresAnywhere()
    {
        // Contract item 5, and this page is the family's front door, so a
        // number here would be a number about six different diagnoses at once.
        // Checked on the composed page and on the reader text, so a figure
        // arriving through a shared block counts.
        var composed = CuratedPage.Composed(Page, "tumors/glioma");
        var reader = CuratedPage.ReaderText(composed);

        Assert.DoesNotMatch(new Regex(@"\d+\s*(%|percent)", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\b(five|ten|two|three)[- ]year (survival|relative survival)",
                RegexOptions.IgnoreCase),
            reader);
        Assert.DoesNotMatch(
            new Regex(@"median (survival|overall survival)", RegexOptions.IgnoreCase), reader);
    }

    [Fact]
    public void TheOutlookGateIsClosedAndTeachesMedianRatherThanBanningTheWord()
    {
        // §12.9, and the correction WI-513 had to make to its own first draft:
        // a gate that publishes no figures still has to teach the vocabulary,
        // or it explains nothing. §12.5 wants four moves for "median" — that it
        // describes a GROUP, the distribution rather than the midpoint, the
        // right skew, and plainly that it is not a prediction about one person.
        var section = Section(OutlookHeading);

        Assert.Contains(":::outlook", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"middle of a group|large groups", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not a prediction about any one person", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"long tail|not spread evenly", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheRightTailSitsInsideTheGateAndNotOutsideIt()
    {
        // WI-513's own defect, and the one most likely to be copied: the most
        // hope-preserving sentence on the page sat in an ungated section, so a
        // reader who declined outlook met it anyway. That is precisely what the
        // gate exists to put behind a choice.
        var section = Section(OutlookHeading);

        var gate = Regex.Match(section, @":::outlook(.*?):::", RegexOptions.Singleline);
        Assert.True(gate.Success, "the outlook section has no closed ::: gate");

        var inside = CuratedPage.Flatten(gate.Groups[1].Value);
        var outside = CuratedPage.Flatten(section.Replace(gate.Value, " ", StringComparison.Ordinal));

        Assert.Matches(new Regex(@"live many years", RegexOptions.IgnoreCase), inside);
        Assert.DoesNotMatch(new Regex(@"live many years", RegexOptions.IgnoreCase), outside);
        Assert.DoesNotMatch(new Regex(@"\bmedian\b", RegexOptions.IgnoreCase), outside);

        // NOTHING may sit outside the fence in this section. §12.5: the warning
        // and the labels are emitted by the gate component, "not typed per page
        // — 24 tumor hubs cannot each water it down ... Nothing else needs
        // writing." An earlier version of this test REQUIRED an opt-out
        // sentence outside the gate, citing §12.6's landing rule — but that
        // rule was about the section the right tail was moved OUT of, not about
        // outlook. As written it mandated a near-duplicate of
        // ReaderGate.WarningText and would have been copied to 22 more hubs.
        Assert.True(string.IsNullOrWhiteSpace(outside.Replace("#", "").Trim()),
            $"prose sits outside the outlook gate, where the component already speaks: {outside}");
    }

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3, and §12.9's first warning: a hub follows the seventeen
        // sections, NOT §12.8's twelve slots, which seven Wave 1 pages in a row
        // made familiar. Ordering matters and set membership would not catch a
        // page that answered outlook before it answered symptoms — the whole
        // design principle is "action at the end of every frightening block".
        var headings = Regex.Matches(CuratedPage.Body(Page), @"^## (.+?)\s*$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        var expected = new[]
        {
            "The short version",
            FamilyHeading,
            GradeHeading,
            "Where does it grow, and why does it cause these symptoms?",
            SymptomHeading,
            "How do doctors find out it is this?",
            ReportHeading,
            "How is it usually treated?",
            "What is treatment actually like, and what is normal afterwards?",
            "Everyday life: work, driving, seizures and tiredness",
            "Follow-up scans, and what to do while you wait",
            "If it comes back, or changes",
            "Did I cause this?",
            OutlookHeading,
            "For the person caring for someone with this",
            "What to ask your team",
            "Where to get support",
        };

        Assert.Equal(expected, headings);
    }

    [Fact]
    public void OutlookSitsAfterEverythingActionableAndBeforeTheCaregiverSection()
    {
        // §12.3's ordering rationale stated as a rule: by the time a reader
        // reaches outlook they have already been given everything they can act
        // on. Pinned separately from the full order above so that the REASON
        // survives a future reshuffle of the sections around it.
        var body = CuratedPage.Body(Page);

        var treatment = body.IndexOf("## How is it usually treated?", StringComparison.Ordinal);
        var recurrence = body.IndexOf("## If it comes back, or changes", StringComparison.Ordinal);
        var outlook = body.IndexOf($"## {OutlookHeading}", StringComparison.Ordinal);
        var caregiver = body.IndexOf(
            "## For the person caring for someone with this", StringComparison.Ordinal);

        Assert.True(treatment < outlook, "outlook must come after treatment");
        Assert.True(recurrence < outlook, "outlook must come after 'if it comes back'");
        Assert.True(outlook < caregiver, "the caregiver section must follow outlook");
    }

    [Fact]
    public void TheSelfBlameSectionIsPresentButDemotedNearTheEnd()
    {
        // §12.3's "deliberate omissions" paragraph: the self-blame block "still
        // appears; it just is not near the top". WI-513 read that as an aside
        // and shipped a draft without it. Late enough not to greet a frightened
        // reader with "nobody knows what caused this", early enough that they
        // meet it before outlook.
        var body = CuratedPage.Body(Page);

        var causes = body.IndexOf("## Did I cause this?", StringComparison.Ordinal);
        var outlook = body.IndexOf($"## {OutlookHeading}", StringComparison.Ordinal);

        Assert.True(causes > 0, "the page has no self-blame section");
        Assert.True(causes < outlook, "the self-blame block belongs before the outlook gate");
        Assert.True(
            causes > body.Length / 2,
            "the self-blame block is near the top, which §12.3 explicitly rules out");
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);
        Assert.Contains("reviewed:", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);
        Assert.Contains("slug: tumors/glioma", front, StringComparison.Ordinal);

        // Contract item 8: every source carries an accessed date.
        var urls = Regex.Matches(front, @"^\s+- url:", RegexOptions.Multiline).Count;
        var accessed = Regex.Matches(front, @"^\s+accessed:", RegexOptions.Multiline).Count;
        Assert.True(urls > 0, "the page cites no sources");
        Assert.Equal(urls, accessed);
    }

    [Fact]
    public void TheSourcesTheDossierGotWrongNeverAppearOnThisPage()
    {
        // Four dossier attributions failed verification for WI-514, and each of
        // these URLs would resolve — which is the worst failure mode on a
        // medical site, because a citation that opens looks checked.
        //
        //  - moffitt.org (grade-vs-stage) is Cloudflare-gated and returns
        //    "Just a moment..."; a citation nobody can open is one nobody can
        //    verify (WI-509's rule for academic.oup.com).
        //  - mdpi.com/2072-6694/14/10/2507 (infiltration routes) returns a
        //    1-byte JavaScript shell, same as WI-511's ascopubs.org.
        //  - PMC12406498, cited by the dossier for "steroids can improve
        //    someone within a day or two", is a UK pharmacovigilance audit of
        //    steroid COMPLICATIONS and prescribing. It does not say it.
        //  - NCI patient PDQ is forbidden by §12.1 for naming and grading, and
        //    this page is the canonical home for both.
        //
        // Checked on the page front matter AND on the shared blocks it pulls
        // in. Block `sources` merge into the including page and RENDER in the
        // reader's source list, so a bad URL added to crosswalk.md or
        // mechanism.md would ship onto every hub that includes them while a
        // front-matter-only check stayed green.
        var surfaces = new List<(string Where, string Text)>
        {
            ("tumors/glioma front matter", CuratedPage.FrontMatter(Page)),
        };

        foreach (var file in Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"))
        {
            surfaces.Add(($"blocks/{Path.GetFileName(file)}", File.ReadAllText(file)));
        }

        foreach (var bad in new[]
                 {
                     "moffitt.org",
                     "mdpi.com",
                     "PMC12406498",
                     "cancer.gov/types/brain/patient",
                 })
        {
            foreach (var (where, text) in surfaces)
            {
                Assert.False(
                    text.Contains(bad, StringComparison.OrdinalIgnoreCase),
                    $"'{bad}' is cited in {where}, and it either cannot be fetched or is "
                    + "forbidden by §12.1 for the claims on this page");
            }
        }
    }

    [Fact]
    public void ThePageNeverTellsTheReaderTheirGradeIsTheGoodKind()
    {
        // WI-513's bluntest finding, applied to the umbrella: patients are
        // routinely told "this is the good kind" before referral. On the family
        // page, any reassurance would be a claim about six different diagnoses
        // at once, which is where it is least defensible.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Composed));

        foreach (var phrase in new[] { "the good kind", "nothing to worry about", "the best one to have" })
        {
            Assert.DoesNotContain(phrase, reader, StringComparison.OrdinalIgnoreCase);
        }
    }
}

/// <summary>
/// WI-514: the umbrella against the running site. §12.9 is explicit that the
/// gate must be VERIFIED rather than assumed, "because every fail-open mode
/// builds green" — and on a router page, the links are the product.
/// </summary>
public sealed class GliomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/glioma";

    private readonly WebApplicationFactory<Program> _factory;

    public GliomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        // Contract item 10: existing /tumors/* URLs are preserved. This page
        // existed as a 166-word stub and readers may have it bookmarked.
        var response = await _factory.CreateClient().GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The router's whole product. Every diagnosis this page names has to
        // have a working door, or the page has told a frightened reader that
        // somewhere else has their answer and then not taken them there.
        // The helper checks that EVERY link in the article resolves, and that
        // the named ones appear in the onward section specifically. Only the
        // last two belong to "Where to get support"; the routing links live in
        // the body, which is the point of a router — a reader should not have
        // to reach the bottom of the page to be sent somewhere.
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support",
            "/get-help-now", "/seizures/living-with");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // WI-513 added this check after its own draft linked
        // /tests/pathology-report#grade, which did not exist and returned a
        // healthy 200. This page deep-links /seizures/living-with#driving, and
        // that anchor is explicit on the target as a result.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        // Five documented fail-open modes, every one of which builds green.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        var heading = html.IndexOf("id=\"what-might-happen-over-time\"", StringComparison.Ordinal);
        var gate = html.IndexOf("reader-gate__disclosure", StringComparison.Ordinal);

        Assert.True(heading > 0, "the outlook heading is not on the rendered page");
        Assert.True(gate > 0, "the reader-choice gate did not render");
        Assert.True(gate > heading, "the outlook heading has been swallowed into the disclosure");

        // Closed on load, and the fence did not leak as literal text.
        Assert.DoesNotContain(":::", html, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"<details[^>]*\bopen\b"), html);
    }

    [Fact]
    public async Task TheGatedWordsAppearNowhereOutsideTheGate()
    {
        // The fail-open that matters most: a reader who declines outlook must
        // not meet it anyway. WI-513's draft had the most hope-preserving
        // sentence on the page sitting in an ungated section.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        var gate = Regex.Match(html, @"<details class=""reader-gate__disclosure"".*?</details>",
            RegexOptions.Singleline);
        Assert.True(gate.Success, "the reader-choice gate did not render as a closed disclosure");

        var outside = Regex.Replace(html.Replace(gate.Value, " ", StringComparison.Ordinal),
            "<[^>]+>", " ");

        foreach (var gated in new[] { "median", "live many years", "long tail" })
        {
            Assert.DoesNotContain(gated, outside, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task BothNewSharedBlocksComposeIntoTheRenderedPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var directive in new[]
                 { "[CROSSWALK]", "[MECHANISM]", "[CAUSES]", "[CAREGIVER]", "[ESCALATION]" })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        // A word from each block, so a directive that expanded to nothing is
        // not mistaken for one that expanded correctly.
        Assert.Contains("grade II", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("closed box", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ThePagesHeadlineVocabularyFiresAsTooltips()
    {
        // Contract item 9. "adult-type" and "pediatric-type" are the words
        // research §0.2 calls "almost universally misexplained", and until this
        // item neither had a glossary entry — so the page's own suppression
        // line was suppressing tooltips that could never have fired.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[] { "nos", "nec", "circumscribed" })
        {
            Assert.Contains($"def-{slug}", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void EverySuppressedTermIsARealGlossaryEntryTheProseActuallyUses()
    {
        // WI-512 shipped two suppressions that suppressed nothing, and this
        // page's first draft shipped four: three named terms that do not exist
        // in the glossary at all, and one for a word the prose never used.
        // Both halves are silent no-ops, so both are checked.
        var page = CuratedPage.Read("tumors", "glioma.md");
        var suppressed = Regex.Matches(page, @"!%(.+?)%")
            .Select(m => m.Groups[1].Value)
            .ToList();

        Assert.NotEmpty(suppressed);

        var terms = Directory.EnumerateFiles(CuratedPage.BlocksRoot.Replace("blocks", "glossary",
                StringComparison.Ordinal), "*.md")
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"^(?:term|also): *(.+)$",
                    RegexOptions.Multiline)
                .Select(m => m.Groups[1].Value.Trim().Trim('[', ']')))
            .SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(page));

        foreach (var term in suppressed)
        {
            Assert.True(terms.Contains(term),
                $"'{term}' is suppressed but is not a glossary term, so the suppression is a no-op");
            Assert.True(reader.Contains(term, StringComparison.OrdinalIgnoreCase),
                $"'{term}' is suppressed but the page never uses the word, so there was nothing "
                + "to suppress — and the reader holding that word on a report is told nothing");
        }
    }
}

/// <summary>
/// WI-514: the composition contract for the shared blocks, asserted across the
/// whole corpus rather than on one page. WI-501's acceptance asked for a test
/// proving that editing one block changes every page that includes it; the
/// crosswalk is the block that will actually need re-editing, because it moves
/// every time WHO or cIMPACT-NOW does.
/// </summary>
public sealed class SharedBlockCompositionTests
{
    [Fact]
    public void EveryBlockDirectiveInTheCorpusResolvesToARealBlock()
    {
        // A missing block must never render as an empty section: on a medical
        // page, silence reads as "there is nothing to say here". ContentBlocks
        // throws on a missing block, so this fails loudly rather than shipping
        // a hole.
        var pages = CuratedPage.AllPages().ToList();
        Assert.NotEmpty(pages);

        var directivesSeen = 0;
        foreach (var (slug, text) in pages)
        {
            directivesSeen += Regex.Matches(text, @"^\[[A-Z][A-Z-]*\]\s*$", RegexOptions.Multiline).Count;
            var exception = Record.Exception(() => CuratedPage.Composed(text, slug));
            Assert.True(exception is null, $"{slug} does not compose: {exception?.Message}");
        }

        Assert.True(directivesSeen > 0,
            "no page in the corpus uses a block directive, so this proved nothing");
    }

    [Fact]
    public void EditingTheCrosswalkBlockChangesEveryPageThatIncludesIt()
    {
        // WI-501's acceptance criterion, finally exercised on a block that is
        // included by more than one page.
        //
        // This genuinely alters the block and re-composes. An earlier version
        // composed the REAL block and asserted a word was present, which is
        // just the composition test run twice — it could not fail for the
        // reason its name claims, while its comment said it could. A test whose
        // comment overstates it is worse than no test, because the next person
        // trusts it.
        var including = CuratedPage.AllPages()
            .Where(p => Regex.IsMatch(p.Text, @"^\[CROSSWALK\]\s*$", RegexOptions.Multiline))
            .ToList();

        Assert.True(including.Count > 1,
            "fewer than two pages include [CROSSWALK], so this cannot prove a shared edit propagates");

        var temporary = Directory.CreateTempSubdirectory("bh-blocks-");
        try
        {
            foreach (var file in Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"))
            {
                File.Copy(file, Path.Combine(temporary.FullName, Path.GetFileName(file)));
            }

            var crosswalk = Path.Combine(temporary.FullName, "crosswalk.md");
            const string Sentinel = "WI-514 propagation sentinel";
            File.WriteAllText(crosswalk, File.ReadAllText(crosswalk) + "\n" + Sentinel + "\n");

            var edited = ContentBlockStore.Load(temporary.FullName);
            foreach (var (slug, text) in including)
            {
                var composed = ContentBlocks.Compose(text, edited, slug).Markdown;
                Assert.DoesNotContain("[CROSSWALK]", composed, StringComparison.Ordinal);
                Assert.Contains(Sentinel, composed, StringComparison.Ordinal);
            }

            // And the real block does NOT carry the sentinel, so a pass above
            // cannot come from having accidentally edited the shipped file.
            foreach (var (slug, text) in including)
            {
                Assert.DoesNotContain(Sentinel, CuratedPage.Composed(text, slug), StringComparison.Ordinal);
            }
        }
        finally
        {
            temporary.Delete(recursive: true);
        }
    }

    [Fact]
    public void TheCrosswalkBlockCarriesItsOwnSourcesSoIncludingPagesInheritThem()
    {
        // WI-501: "a block may carry `sources` front matter, which merges into
        // every including page — otherwise the drift problem just moves to the
        // citation list." The crosswalk makes claims about WHO CNS5 that no
        // including page should have to re-cite by hand.
        var block = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "crosswalk.md"));

        Assert.StartsWith("---", block, StringComparison.Ordinal);
        Assert.Contains("sources:", block, StringComparison.Ordinal);
        Assert.Contains("PMC9723092", block, StringComparison.Ordinal);

        var urls = Regex.Matches(block, @"^\s+- url:", RegexOptions.Multiline).Count;
        var accessed = Regex.Matches(block, @"^\s+accessed:", RegexOptions.Multiline).Count;
        Assert.Equal(urls, accessed);
    }
}
