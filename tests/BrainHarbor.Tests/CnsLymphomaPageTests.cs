using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-542: CNS lymphoma, deepened. A §12.3 SEVENTEEN-SECTION TUMOR HUB, and the
/// first one whose tumor is a BLOOD CANCER sitting in a brain. Almost everything
/// that makes it unlike its neighbours follows from that.
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * THE CLAIM THE ITEM TURNS ON. Diagnosis is by BIOPSY, not by removing the
///     tumor, and a steroid given before the sample can take the answer away.
///     Verified against the live publication, not the research pack: Blood 2022,
///     "Corticosteroids are toxic to lymphoma cells and can cause a complete
///     resolution of PCNSL lesions, resulting in poor diagnostic yield".
///   * AND ITS THREE SOFTENERS, which are what stop that rule frightening
///     somebody who is already on a steroid. The guideline grades the
///     avoid-steroids rule [IV, D], its WEAKEST tier, and grades the
///     deteriorating-patient exception [IV, A]. A page that states the rule more
///     firmly than its own guideline has inverted the evidence.
///   * THERE IS NO GRADE, AND THE PAGE SAYS SO. WHO CNS5 names this entity and
///     PRINTS NO GRADE beside it (verified live, by direct question). This is
///     NOT WI-541's shape reused: there, a tumor with a grade had it left
///     unprinted. Here the entity is classified as a lymphoma rather than graded
///     on the brain-tumor scale at all.
///   * THE URGENCY RULE IS A TREATMENT RULE, NOT A TUMOR RULE. No PCNSL-specific
///     source gives patient-facing action guidance -- StatPearls, EHA-ESMO, EANO
///     and every review are addressed to clinicians, and the US patient
///     organization was asked directly and gives none. So the page refuses to
///     invent one and routes the fever rule instead.
///   * a prognosis figure, a percentage, a dose, a threshold or a scan interval
///     (§12.4, §12.5).
///
/// NO ALLOWLIST, and this file carries TWO restatement guards rather than one.
/// The first draft scored 9 files on the shared guard and 11 on the page-local
/// one, and the page-local probe alone saw /tests/biopsy and
/// /tests/waiting-for-results. A clean report from one says nothing about the
/// other (§12.8, WI-541).
/// </summary>
public sealed class CnsLymphomaPageContentTests
{
    private const string Slug = "tumors/cns-lymphoma";
    private const string ShortHeading = "The short version";
    private const string WhatHeading = "What is CNS lymphoma?";
    private const string GradeHeading = "Is it cancer? What does its grade mean?";
    private const string WhereHeading = "Where does it grow, and why does it cause these symptoms?";
    private const string SymptomHeading = "What symptoms does it cause?";
    private const string FindOutHeading = "How do doctors find out it is this?";
    private const string ReportHeading = "What do the words on my report mean?";
    private const string TreatedHeading = "How is it usually treated?";
    private const string AfterHeading = "What is treatment actually like, and what is normal afterwards?";
    private const string EverydayHeading = "Everyday life";
    private const string ScansHeading = "Follow-up scans, and what to do while you wait";
    private const string BackHeading = "If it comes back";
    private const string CauseHeading = "Did I cause this?";
    private const string CareHeading = "For the person caring for someone with this";

    private static string Page => CuratedPage.Read("tumors", "cns-lymphoma.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// Title and description. <c>ReaderText</c> strips them, and the description
    /// renders as the first paragraph a reader meets (§12.3, WI-524) -- so it is
    /// the least-guarded prose on the page unless a test reaches it deliberately.
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

    /// <summary>Emphasis removed (§12.8, WI-526: a bolded word defeats a phrase guard structurally).</summary>
    private static string Plain => Regex.Replace(Everything, @"[*_]", "");

    private static string PlainOf(string heading) =>
        Regex.Replace(CuratedPage.Flatten(Reader(Section(heading))), @"[*_]", "");

    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|"
        + @"thirteen|fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|"
        + @"twenty|thirty|forty|fifty|sixty|seventy|eighty|ninety|hundred|thousand|"
        + @"dozen|couple)";

    private const string Fraction =
        @"(?:half|halves|thirds?|quarters?|fifths?|sixths?|sevenths?|eighths?|ninths?|"
        + @"tenths?|twentieths?|hundredths?)";

    private static List<string> Headings() =>
        Regex.Matches(CuratedPage.Body(Page), @"^## (.+?)\s*$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

    // ------------------------------------------- the claim the item turns on

    [Fact]
    public void TheBiopsyBeforeSteroidsRuleCarriesItsMechanismAndItsReason()
    {
        // THE CENTRAL CLAIM, verified live: Blood 2022, "Corticosteroids are
        // toxic to lymphoma cells and can cause a complete resolution of PCNSL
        // lesions, resulting in poor diagnostic yield". StatPearls supplies the
        // name for what is left behind: "'ghost' or 'vanishing' tumors".
        var section = PlainOf(FindOutHeading);

        // THE LEAD CARRIES THE LIMIT ITSELF, and the pairing guard is why.
        // The first version read "...shrink within days, and it can cost you the
        // diagnosis" -- a shrink claim whose only companion was the DIAGNOSTIC
        // harm, with the recurrence limit six sentences and most of a screen
        // below. That is round 1's blocker in miniature, sitting at the exact
        // sentence a skimming reader lands on first.
        //
        // Fixed in the PAGE rather than by widening the guard's window. Loosening
        // is how this guard lost its teeth twice already.
        Assert.Matches(new Regex(
            @"A steroid can make this tumor shrink within days\. That does not last, and it\s*"
            + @"can cost you the diagnosis",
            RegexOptions.IgnoreCase), section);

        // THE MECHANISM, not just the instruction. A reader told only "they want
        // the biopsy first" has been given a rule with no reason, and a rule with
        // no reason is the first thing to get argued with at two in the morning.
        Assert.Matches(new Regex(@"act on lymphoma cells directly", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"ghost tumor", RegexOptions.IgnoreCase), section);

        // AND THE CONSEQUENCE FOR THE SAMPLE.
        Assert.Matches(new Regex(
            @"may not\s*hold enough lymphoma to name", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePhraseAnotherPagesSuiteReadsIsPinnedHereToo()
    {
        // THIS PAGE'S HALF OF A CONTRACT ASSERTED ON A SHIPPED PAGE.
        // SteroidsPageTests.TheDoesNotTreatTheTumorClaimIsScopedAndCarriesItsException
        // opens this file and requires BOTH of these. That page's central claim
        // -- a steroid works on swelling, not on the tumor -- is true everywhere
        // on this site EXCEPT here, and it is scoped by naming this page as the
        // exception.
        //
        // Asserted here as well so that an edit which breaks it fails in the file
        // where the edit was made, rather than only in a sibling's suite where
        // nobody would look. That is the shape WI-541 used for the glossary
        // reachability set.
        Assert.Matches(new Regex(@"biopsy before starting steroids", RegexOptions.IgnoreCase), Body);
        Assert.Contains("/treatments/steroids", Body, StringComparison.Ordinal);

        // AND THE SIBLING STILL MAKES THE CLAIM THIS PAGE IS THE EXCEPTION TO,
        // read rather than assumed (§12.10, WI-514). If that page stops scoping
        // its claim, this page's whole framing needs rechecking.
        var steroids = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "steroids.md")));
        Assert.Contains("There is one exception", steroids, StringComparison.Ordinal);
        Assert.Contains("/tumors/cns-lymphoma", steroids, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRuleIsStatedNoMoreFirmlyThanTheGuidelineThatGradesIt()
    {
        // §12.13: the strongest available wording is not automatically the
        // sourced one, and here the evidence grades say so explicitly.
        //
        // EHA-ESMO, verified live WITH its grades: "Corticosteroid therapy before
        // tissue biopsy should be avoided whenever clinically possible [IV, D]"
        // -- the WEAKEST tier -- while "In case of clinical deterioration, urgent
        // biopsy should be carried out before the start of corticosteroids
        // [IV, A]" is graded A. So the exception is the firmer recommendation of
        // the two, and a page that states the rule as a law has inverted its own
        // source.
        var section = PlainOf(FindOutHeading);

        Assert.Matches(new Regex(
            @"a rule for your team, not a decision you have to police", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"weakest kind of recommendation", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"grades the\s*exception more strongly", RegexOptions.IgnoreCase), section);

        // THE DETERIORATING READER IS SENT TOWARD CARE, NOT AWAY FROM IT. The
        // answer to getting worse is an EARLIER biopsy, never a withheld
        // treatment.
        Assert.Matches(new Regex(
            @"the answer is to do the biopsy\s*\*{0,2}sooner", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(Reader(Section(FindOutHeading))));

        // AND THE PAGE NEVER TELLS ANYONE TO REFUSE OR STOP A DRUG. This is the
        // single most dangerous sentence this page could accidentally write: a
        // reader with rising pressure talking their team out of a steroid.
        // THE PHRASINGS THIS PAGE ACTUALLY DRIFTS TOWARD ARE IN THE LIST, not just
        // the obvious ones. /review round 1 pointed out that the first version
        // covered "refuse" and "stop taking" but none of "come off", "skip a
        // dose", "hold off" or "wait before starting" -- and the page's own draft
        // had said "Never come off one without being told to", which is the same
        // vocabulary from the safe direction.
        var refuses = new Regex(
            @"(?:refuse|say no to|turn down|ask them not to give|do not take|don't take|"
            + @"stop taking|come off|skip a dose|hold off|wait before starting)"
            + @"[^.]{0,40}steroid|steroid[^.]{0,40}(?:should be refused|can be refused)",
            RegexOptions.IgnoreCase);
        Assert.Matches(refuses, "You can refuse the steroid until the biopsy is done.");
        Assert.Matches(refuses, "It is fine to come off the steroid for a day.");

        // ONE SENTENCE IS EXEMPT, AND IT IS EXEMPT BECAUSE IT SAYS THE OPPOSITE.
        // The page's own safety line -- "Coming off a steroid suddenly can make
        // you very ill" -- warns AGAINST the thing this ban forbids. It escapes
        // the pattern today only because the gerund "Coming" is not the literal
        // string "come off", which means the most load-bearing safety sentence in
        // the section is ONE ORDINARY REPHRASE away from turning the suite red.
        //
        // That is this item's thrice-recorded shape (a ban firing on a correct
        // sentence) caught BEFORE it fired rather than after, so the exemption is
        // enumerated now, in the same style as the interval exemptions.
        var scanned = Regex.Replace(Plain,
            @"Com(?:e|ing) off a steroid[^.]*\.", " ", RegexOptions.IgnoreCase);
        Assert.True(scanned != Plain,
            "the safety sentence this exemption names has been reworded, so the exemption has "
            + "stopped being an exemption and started being a hole");

        Assert.False(refuses.IsMatch(scanned),
            "the page tells a reader to refuse or stop a steroid, which no source supports and "
            + "which is dangerous for anyone whose pressure is rising");
    }

    [Fact]
    public void TheAlreadyOnASteroidReaderIsReassuredRatherThanAlarmed()
    {
        // The reader most likely to be frightened by this page is the one who
        // was started on a steroid in an emergency room days ago. Blood 2022
        // gives the fallback, verified live: "If it is necessary to start
        // steroids, a brain biopsy should be performed within 24 to 48 hours of
        // the first dose to optimize the diagnostic yield."
        var section = PlainOf(FindOutHeading);

        // THE LEAD NO LONGER SAYS "NOTHING IS RUINED", and /review round 1 is why:
        // the page contradicted it twice within twenty lines ("it can cost you the
        // diagnosis", "more likely to come back with no answer at all"), and the
        // source is weaker than the absolute -- Batchelor says a diagnosis "can be
        // difficult OR IMPOSSIBLE to achieve after exposure to these drugs".
        Assert.Matches(new Regex(
            @"that is common and\s*it is not a mistake you have to fix", RegexOptions.IgnoreCase),
            section);

        // AND BOTH WAYS ROUND IT ARE NAMED. The first version offered only a
        // biopsy "within a day or two of the first dose" while saying in the same
        // breath that the steroid usually started "days before" -- so the window
        // had already closed for the exact reader the paragraph addresses, and the
        // page said nothing about what happens then.
        Assert.Matches(new Regex(@"soon after that first dose", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"wait and\s*look again later", RegexOptions.IgnoreCase), section);

        // AND THE WORDING IT REPLACED IS GONE, WHICH IS THE HALF THAT WAS MISSING.
        //
        // /review objected to the fallback being stated as a fixed window ("within
        // a day or two of the first dose") while the same paragraph said the
        // steroid usually started "days before" -- so the window had already
        // closed for the reader being addressed. The first attempt at that fix
        // APPENDED the new sentence instead of replacing the old one, and
        // EVERYTHING STAYED GREEN: suite 2,287/2,287, ContentCheck 273/0, all
        // three collision probes 0/0. None of them implements redundancy, and the
        // two assertions above passed happily beside the sentence they were meant
        // to displace. Only the rendered read saw it.
        //
        // A guard for a REPLACEMENT has to assert the OLD wording is ABSENT.
        // Presence of the new text says nothing about absence of the old -- the
        // same shape §12.8 records for banned sources ("when you ban a source,
        // grep the page for the claim it was carrying"), arriving through the
        // editing tool rather than through a citation.
        var replaced = new Regex(@"within a day\s*or two of the first dose", RegexOptions.IgnoreCase);
        Assert.Matches(replaced, "Take the sample within a day or two of the first dose.");
        Assert.DoesNotMatch(replaced, section);

        // The stutter that came with it, likewise.
        Assert.DoesNotMatch(new Regex(@"That happens often", RegexOptions.IgnoreCase), section);

        // THE SAFETY SENTENCE IS BOLD, HAS ITS REASON, AND NAMES WHAT IT REFERS TO.
        // The first version was an unbolded nine-word tail ("Never come off one
        // without being told to") whose nearest antecedent for "one" was the first
        // dose, with no reason given and in British register.
        Assert.Matches(new Regex(
            @"If you are already on a steroid, do not stop it yourself", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"Coming off a steroid\s*suddenly can make you very ill", RegexOptions.IgnoreCase),
            section);

        // AND THE NON-DIAGNOSTIC PATH IS NAMED RATHER THAN LEFT AS A HOLE.
        Assert.Matches(new Regex(
            @"A sample that cannot be named is a known situation with a known path",
            RegexOptions.IgnoreCase), section);

        // §12.6's landing rule, SCOPED TO THE SUBSECTION rather than the section.
        // The first version asserted only that the section was non-empty while its
        // comment promised a landing check -- /review round 1's finding, and
        // §12.10 (WI-517) records exactly why: `SentencesOf(Section(heading))[^1]`
        // is the SECTION's last sentence, and this heading has `###` subsections
        // under it, so the check was reading the end of "What else gets checked".
        var steroidSub = Regex.Replace(
            CuratedPage.Flatten(CuratedPage.ComposedSubsection(
                Page, "Why the steroid timing matters so much")), @"[*_]", "");

        var last = CuratedPage.SentencesOf(steroidSub).Last(s => s.Trim().Length > 0);
        Assert.DoesNotMatch(
            new Regex(@"cost you the diagnosis|no answer at all|cannot be named", RegexOptions.IgnoreCase),
            last);
        Assert.Matches(new Regex(@"team|ask|tell|decide", RegexOptions.IgnoreCase), last);
    }

    [Fact]
    public void EveryShrinkingSentenceCarriesTheFactThatItComesBack()
    {
        // THE MANDATORY PAIRING, and the reason it is mandatory. PMC13525931,
        // verified live: "After initial tumor regression following CST, most
        // tumors recur after a short period of time, meaning that CST alone is
        // not a viable treatment option."
        //
        // This is the ONE page in the corpus where "the steroid shrank it" is
        // even available to misread as treatment, and §12.12 says the
        // over-reassuring direction is the dangerous one.
        // THE FIRST VERSION OF THIS GUARD WAS GREEN ON A PAGE THAT HAD EXACTLY THE
        // DEFECT ITS NAME FORBIDS, and /review round 1 caught it. It collected the
        // matching sentences into a list, asserted the list was non-empty, and
        // then NEVER USED THE LIST -- checking the pairing words against `Plain`,
        // the whole page, where "comes back" matches the "## If it comes back"
        // heading and "returns" matches "a minority of returns are found on a
        // scan". A method name asserting a per-sentence property over a body
        // asserting a page-wide OR. §12.10's recorded trap: a test asserting a
        // phrase where the claim was the property.
        //
        // The pairing is now checked IN THE SECTION THAT CARRIES THE CLAIM.
        var shrink = new Regex(
            @"steroids?[^.]{0,60}\bshrink|shrink[^.]{0,40}\b(?:tumor|lymphoma)",
            RegexOptions.IgnoreCase);
        var pairing = new Regex(
            @"does not last|comes back|not a treatment|only for a while|for a time",
            RegexOptions.IgnoreCase);

        var section = PlainOf(FindOutHeading);

        // ASSERTED PER SENTENCE, WHICH IS WHAT THE METHOD NAME CLAIMS.
        //
        // /review said this twice about the same guard, and the second time is the
        // useful one. Version 1 checked the pairing against the whole PAGE, where
        // "comes back" matched the `## If it comes back` heading. Version 2
        // narrowed the scope to the section -- which killed that false positive
        // and STILL did not assert the property, because the collected sentences
        // were never used: a shrink sentence at the far end of the section passed.
        //
        // NARROWING A SCOPE IS NOT THE SAME AS ASSERTING A PROPERTY. The limit now
        // has to appear within three sentences of each claim, which is the thing
        // the name has promised all along.
        //
        // Verified live, PMC13525931: "After initial tumor regression following
        // CST, most tumors recur after a short period of time, meaning that CST
        // alone is not a viable treatment option."
        // HEADINGS STRIPPED BEFORE SPLITTING. A `###` heading carries no terminal
        // punctuation, so `SentencesOf` glues it onto the sentence beneath it --
        // which both shifts the window and makes a failure message read as though
        // the heading were part of the claim. Found in this guard's own output.
        var whole = CuratedPage.SentencesOf(
            Regex.Replace(Plain, @"(?m)^#{1,6} .*$", " "));
        var claims = whole.Where(s => shrink.IsMatch(s)).ToList();
        Assert.True(claims.Count >= 1,
            "the page has stopped saying a steroid shrinks this tumor, which is the fact the "
            + "diagnosis section depends on -- or it is phrased in a way this guard cannot see, "
            + "in which case the pairing check below is running over nothing");

        var unpaired = new List<string>();
        for (var i = 0; i < whole.Length; i++)
        {
            if (!shrink.IsMatch(whole[i]))
            {
                continue;
            }
            if (!whole.Skip(i).Take(3).Any(pairing.IsMatch))
            {
                unpaired.Add(whole[i]);
            }
        }
        Assert.True(unpaired.Count == 0,
            "a sentence says a steroid shrinks this tumor and nothing within the next three "
            + "sentences says the shrinking does not last. On the one page in the corpus where "
            + "that sentence can be read as treatment, that is the over-reassuring direction "
            + "(§12.12):\n  " + string.Join("\n  ", unpaired));
        Assert.Matches(new Regex(@"The shrinking does not last", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"a steroid on its own is not a treatment", RegexOptions.IgnoreCase), section);

        // AND THE SHORT VERSION, which is the only part many readers reach.
        Assert.Matches(pairing, PlainOf(ShortHeading));

        // THE CANARY THAT THE FIRST VERSION LACKED: the pairing regex must FAIL on
        // an unpaired shrink sentence, or it cannot distinguish the defect from
        // the fix.
        Assert.Matches(shrink, "A steroid can make this tumor shrink within days.");
        Assert.DoesNotMatch(pairing, "A steroid can make this tumor shrink within days.");
        Assert.Matches(pairing, "The shrinking does not last.");
    }

    // ------------------------------------------------------------- the grade

    [Fact]
    public void ThePageSaysThereIsNoGradeAndNeverBorrowsOne()
    {
        // WHO CNS5 verified live, by direct question, twice over: it lists
        // "Primary diffuse large B-cell lymphoma of the CNS" and PRINTS NO GRADE
        // beside it, in contrast to meningioma's 1, 2 and 3.
        //
        // NOT WI-541's ruling reused. There, a tumor that HAS a grade had it left
        // unprinted by the naming authority. Here the entity is classified as a
        // lymphoma rather than graded on the brain-tumor scale at all, so the
        // honest answer to §12.3's section 2 is that there is no number to give.
        var grade = PlainOf(GradeHeading);

        Assert.Matches(new Regex(@"Yes, it is cancer", RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(
            @"There is no grade for this one, and that is not an oversight",
            RegexOptions.IgnoreCase), grade);
        Assert.Matches(new Regex(
            @"prints no grade\s*beside it", RegexOptions.IgnoreCase), grade);

        // THE REASON, which is what stops a later editor "fixing" the absence.
        Assert.Matches(new Regex(@"because this is a lymphoma|reason is that this is a lymphoma",
            RegexOptions.IgnoreCase), grade);

        // AND NO GRADE IS EVER ASSERTED -- BUT THE BAN HAS TO BE NEGATION-AWARE,
        // OR IT FORBIDS THE SENTENCE THAT DOES THE HONEST WORK.
        //
        // The first version was a flat phrase ban and it failed on this page's
        // own "there is no grade 1 to 4 for this one", which is the clearest
        // sentence in the section. That is §12.9's recorded defect (WI-513: a ban
        // list on retired names must be negation-aware or it forbids the
        // crosswalk it protects), reproduced against the very clause this section
        // exists to carry.
        //
        // So the property is asserted at SENTENCE level: a grade may appear only
        // where the same sentence denies that this tumor has one. Same shape as
        // the corpus-wide Roman-numeral marker rule.
        var graded = new Regex(
            @"\bgrade\s*(?:1|2|3|4|I{1,3}|IV)\b|\bCNS WHO grade\b", RegexOptions.IgnoreCase);

        // THE NEGATION MUST ADJOIN THE GRADE, not merely share a sentence with it
        // (§12.8, WI-541's nit 6): a loose `\bnot\b` anywhere in the sentence
        // would wave through "It is a grade 4 tumor, though it is not curable".
        var deniedGrade = new Regex(
            @"\b(?:no|not|never)\b[^.]{0,30}?\bgrade\s*(?:1|2|3|4|I{1,3}|IV)\b",
            RegexOptions.IgnoreCase);

        var asserted = CuratedPage.SentencesOf(Plain)
            .Where(s => graded.IsMatch(s) && !deniedGrade.IsMatch(s))
            .ToList();
        Assert.True(asserted.Count == 0,
            "the page states a grade for a tumor the naming authority does not grade:\n  "
            + string.Join("\n  ", asserted));

        // A POSITIVE COUNT BESIDE THE ITERATE-AND-CHECK (§12.10, WI-517): a page
        // that stopped naming the grade scale at all would pass the loop above
        // while silently dropping the ruling this section exists to make, leaving
        // a reader hunting for a number nobody told them does not exist.
        var taught = CuratedPage.SentencesOf(Plain).Count(s => graded.IsMatch(s));
        Assert.True(taught >= 1,
            $"this page names the grade scale in {taught} sentences; a reader looking for the "
            + "number their report does not carry is owed the explanation");

        // Canaries in BOTH directions, including the sentence that defeated the
        // first version.
        Assert.Matches(graded, "It is a grade 4 tumor.");
        Assert.DoesNotMatch(deniedGrade, "It is a grade 4 tumor.");
        Assert.DoesNotMatch(deniedGrade, "It is a grade 4 tumor, though it is not curable.");
        Assert.Matches(deniedGrade, "There is no grade 1 to 4 for this one.");
    }

    // ----------------------------------------------------------- the naming

    [Fact]
    public void TheReportNameIsTaughtInItsPartsAndBothWhoNamesAreExplained()
    {
        // §12.1: WHO CNS5 is the naming authority, verified live. The reader's
        // report is likely to carry the long form, so the page teaches it.
        var section = PlainOf(ReportHeading);

        Assert.Matches(new Regex(
            @"primary diffuse large B-cell\s*lymphoma of the CNS", RegexOptions.IgnoreCase),
            section);

        // EACH PART IS ASSERTED WITH ITS EXPLANATION, NOT AS A BARE WORD.
        //
        // The break harness caught the first version as a SURVIVOR on both LF and
        // CRLF: it asserted `Contains("Primary", IgnoreCase)` over the section,
        // and the section still contains the lowercase "primary" inside the full
        // report name -- so deleting the bullet that EXPLAINS what "primary"
        // means changed nothing. The guard checked that a word appeared, while
        // the property is that the name is taken apart and each piece given a job
        // (§12.10: a test asserting a phrase where the claim was the property).
        foreach (var explained in new[]
                 {
                     @"Primary means it started here rather than arriving from somewhere else",
                     @"Diffuse large B-cell names the cell",
                     @"Lymphoma of the CNS places it in the brain and spinal cord",
                 })
        {
            Assert.Matches(new Regex(explained.Replace(" ", @"\s+"), RegexOptions.IgnoreCase),
                section);
        }

        // THE SECOND WHO NAME, one year later. WHO-HAEM5, verified live, folded
        // this into an umbrella term with lymphoma of the eye and testis. A
        // reader meeting two names from two books needs to know neither is a
        // mistake.
        Assert.Matches(new Regex(@"immune sanctuaries", RegexOptions.IgnoreCase), Plain);
        Assert.Matches(new Regex(@"Two books, two names, one disease|two names", RegexOptions.IgnoreCase),
            section);

        // THE RETIRED NAMES, which is §12.9's required crosswalk slice written by
        // the page rather than inherited from the block. Only these two are
        // carried: "primary cerebral lymphoma" was proposed by the research pass
        // and NO source called it retired, so it is not on the page.
        foreach (var old in new[] { "Reticulum cell sarcoma", "microglioma" })
        {
            Assert.Contains(old, section, StringComparison.OrdinalIgnoreCase);
        }
        Assert.DoesNotContain("primary cerebral lymphoma", Plain, StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------- the urgency ruling

    [Fact]
    public void ThePageRefusesToInventATumorSpecificUrgencyRule()
    {
        // THE RULING THAT MOST DISTINGUISHES THIS HUB, and the one a later editor
        // is most likely to "fix" by adding a helpful-looking checklist.
        //
        // Across three research passes and sixty-odd sources, NOT ONE
        // PCNSL-specific source gives patient-facing action guidance. StatPearls,
        // EHA-ESMO, EANO and every review are addressed to clinicians -- EANO was
        // asked directly and confirmed it. The US patient organization was asked
        // directly and gives none either.
        var section = PlainOf(SymptomHeading);

        // THE RULING IS PINNED AS A PROPERTY OF THE PAGE, NOT AS AN ESSAY ON IT.
        // The first version printed the editorial argument to the reader, and
        // /review round 1 showed it falsified itself once composed: it said
        // "Nothing tumor-specific is added beneath it" immediately above a
        // tumor-specific fever paragraph, and "Not one of them tells a reader what
        // to do, or how fast" directly beneath a list that had just done exactly
        // that. The reasoning belongs in the front matter, where it now lives
        // alone; what the reader gets is the instruction.
        Assert.Matches(new Regex(@"Use the list above", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"does not add a\s*separate set of warning signs", RegexOptions.IgnoreCase), section);

        // AND THE ABSENCE ITSELF. An invented tier would look like this.
        var invented = new Regex(
            @"go to the emergency (?:room|department) if[^.]{0,60}(?:confus|drowsy|weak)|"
            + @"call 911 if[^.]{0,60}(?:confus|drowsy|weak)",
            RegexOptions.IgnoreCase);
        Assert.Matches(invented, "Go to the emergency room if you become more confused.");
        Assert.False(invented.IsMatch(section),
            "this page has grown a tumor-specific urgency tier out of clinician-facing reports, "
            + "which no patient-facing source for this tumor supports");
    }

    [Fact]
    public void TheOnlyUrgentRuleIsTheTreatmentsAndItIsRoutedNotRepublished()
    {
        // §12.10: a threshold belongs to one page. The shared block routes to
        // /treatments/chemotherapy#fever-rule, which owns the number and says
        // teams differ.
        //
        // That ruling also sidesteps a real conflict in this item's own sources:
        // two US bodies give one temperature and a third gives another. A page
        // that picked one would have published the disagreement as a fact.
        var section = PlainOf(SymptomHeading);

        Assert.Matches(new Regex(
            @"arrives with treatment, not with the\s*tumor", RegexOptions.IgnoreCase), section);
        Assert.Contains("/treatments/chemotherapy#fever-rule",
            Section(SymptomHeading), StringComparison.Ordinal);

        // NO TEMPERATURE IS PRINTED ANYWHERE ON THIS PAGE.
        var temperature = new Regex(
            @"\d+(?:\.\d+)?\s*°|\d+(?:\.\d+)?\s*degrees|(?:temperature|fever)[^.]{0,40}\b\d",
            RegexOptions.IgnoreCase);
        Assert.Matches(temperature, "A temperature over 100.4 means calling.");
        Assert.False(temperature.IsMatch(Plain),
            "this page publishes a fever threshold; /treatments/chemotherapy owns that number "
            + "and this page routes to it");

        // THE SIBLING STILL OWNS IT, READ AT THE SECTION THIS PAGE DEEP-LINKS,
        // NOT AT THE PAGE.
        //
        // The break harness caught the first version as a SURVIVOR on both LF and
        // CRLF: it searched the WHOLE chemotherapy page for "fever rule", which
        // also appears in that page's caregiver section -- so gutting the section
        // this page's link actually lands on left the guard green while the
        // deep link pointed at an empty heading. That is the identical defect
        // CuratedPage.AssertEscalationTiers records having fixed for the same
        // sibling, arriving here because this guard was written page-wide.
        //
        // Section() throws when the heading is gone, so a rename fails loudly
        // rather than silently widening the search.
        var feverSection = CuratedPage.Section(
            CuratedPage.Read("treatments", "chemotherapy.md"),
            "Your blood counts, and the fever rule");
        Assert.Matches(new Regex(@"straight away, at any hour", RegexOptions.IgnoreCase),
            feverSection);
    }

    // ------------------------------------------------------ the block rulings

    [Fact]
    public void ExactlyFourBlocksAreIncludedAndTheFourExclusionsAreAClosedSet()
    {
        // THE BLOCKER THAT WOULD BE AN ABSENCE. An excluded block leaves no trace
        // on the page -- no heading, no directive, nothing to grep -- so a later
        // item can restore one and every other guard in this file stays green.
        //
        // An EXACT SET, not a handful of DoesNotContain checks: a new block added
        // in two years fails here and has to be ruled on deliberately.
        Assert.Equal(
            ["caregiver", "causes", "escalation", "mechanism"],
            ContentBlocks.DirectBlockNames(Page).Order(StringComparer.Ordinal).ToArray());

        Assert.Contains("[MECHANISM]", Section(WhereHeading), StringComparison.Ordinal);
        Assert.Contains("[ESCALATION]", Section(SymptomHeading), StringComparison.Ordinal);
        Assert.Contains("[CAUSES]", Section(CauseHeading), StringComparison.Ordinal);
        Assert.Contains("[CAREGIVER]", Section(CareHeading), StringComparison.Ordinal);

        // THE CROSSWALK EXCLUSION, and its reason is unlike any sibling's. That
        // block is wholly the 2021 CNS GRADING rewrite: Roman to Arabic, gene
        // results entering the name, NOS and NEC. This page's answer is that
        // there is no brain-tumor grade here at all, so a block teaching
        // grade-number conversion would contradict it on the reader's screen.
        foreach (var acronym in new[] { "NOS", "NEC", "IDH", "1p/19q" })
        {
            Assert.DoesNotContain(acronym, Plain, StringComparison.Ordinal);
        }

        // Two case policies, two loops (§12.8, WI-539): a sentence-initial
        // capital walks straight through a case-sensitive ban.
        foreach (var word in new[] { "tumor board", "methylation", "gene panel" })
        {
            Assert.DoesNotContain(word, Plain, StringComparison.OrdinalIgnoreCase);
        }

        // THE SPINAL-CORD AND POSTERIOR-FOSSA EXCLUSIONS. The first was the
        // arguable one: this disease IS defined over the cord as well as the
        // brain. It is excluded because no source verified for this item gives
        // patient-facing cord guidance for this tumor, and the block's own source
        // is about METASTATIC cord compression. Handed to WI-543.
        foreach (var phrase in new[]
                 { "stop talking", "cerebellar mutism", "bladder or bowel", "saddle area" })
        {
            Assert.DoesNotContain(phrase, Plain, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheMechanismBlockIsScopedOnSeizuresWithoutDisarmingTheAmbulanceTier()
    {
        // §12.10's mis-scoped block. The block's "It can set off seizures"
        // paragraph over-states for this tumor: NBTS, verified live, says
        // "Seizures, which are more common in a lot of other brain tumors, such
        // as gliomas or brain metastases, are less common in primary CNS
        // lymphoma".
        //
        // BUT THE SCOPING NOTE IS THE DANGEROUS PART, and WI-541's fifth defect
        // is why. There, a flat denial ("it does not set off seizures") silently
        // outranked the shared block's "A first ever seizure" ambulance rule,
        // and survived fourteen edits because nothing asserted that the denial
        // left the tier intact. This is that assertion, written before the
        // defect rather than after it.
        var section = PlainOf(WhereHeading);

        Assert.Matches(new Regex(@"Before you read the next part", RegexOptions.IgnoreCase), section);

        // LESS COMMON, never absent. The comparative is what the source supports.
        Assert.Matches(new Regex(
            @"less common with it\s*than with most other brain tumors", RegexOptions.IgnoreCase),
            section);

        // AND THE AMBULANCE RULE IS PRESERVED EXPLICITLY.
        Assert.Matches(new Regex(
            @"A first ever seizure is an ambulance call", RegexOptions.IgnoreCase), section);

        // The flat denial must never appear.
        var flatDenial = new Regex(
            @"does not (?:set off|cause) seizures|no seizures|seizures do not happen",
            RegexOptions.IgnoreCase);
        Assert.Matches(flatDenial, "It does not set off seizures.");
        Assert.False(flatDenial.IsMatch(section),
            "the scoping note denies seizures outright, which is false here and which would "
            + "let a reader strike a sign off the shared block's ambulance tier");

        // BEFORE the directive, not after it. A note a reader meets on the way
        // out has scoped nothing (§12.8, WI-512: position was the property).
        var raw = CuratedPage.Flatten(Section(WhereHeading));
        var note = raw.IndexOf("Before you read the next part", StringComparison.Ordinal);
        var directive = raw.IndexOf("[MECHANISM]", StringComparison.Ordinal);
        Assert.True(note >= 0 && directive > note,
            "the scoping note does not sit above [MECHANISM], so the reader meets the seizure "
            + "paragraph before anything reconciles it with this tumor");

        // AND THE BLOCK STILL SAYS WHAT THE NOTE IS ABOUT. If a later item
        // rewrites the block, this note becomes a puzzle rather than silently
        // wrong (§12.10, WI-514: read the sibling, do not trust it).
        var mechanism = CuratedPage.Flatten(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md")));
        Assert.Contains("It can set off seizures", mechanism, StringComparison.Ordinal);

        Assert.Contains("A **first ever seizure**",
            CuratedPage.Flatten(CuratedPage.EscalationBlock), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSharedBlockIsIncludedUneditedAndTheTiersStillAgreeWithTheSiblings()
    {
        var raw = Section(SymptomHeading);
        Assert.Contains("[ESCALATION]", raw, StringComparison.Ordinal);

        // THE BLOCK ITSELF IS UNTOUCHED. Editing it to carry this tumor's
        // vocabulary would put it on every glioma hub (WI-514's blast radius).
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);
        foreach (var absent in new[] { "lymphoma", "methotrexate", "floaters", "slit lamp" })
        {
            Assert.DoesNotContain(absent, block, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertEscalationTiers(Page, Slug, SymptomHeading);
    }

    [Fact]
    public void TheSelfBlameBlockIsScopedForTheReaderWhoDoesHaveAKnownReason()
    {
        // §12.10's "would this be true on the reader you have thought about
        // least?" The block says "For most brain tumors, nobody knows the cause."
        // That is true here for most readers and INCOMPLETE for one group: NORD,
        // verified live, records that people with a weakened immune system are at
        // raised risk of this particular tumor.
        //
        // A flat "nobody knows" would tell a transplant patient something false
        // about their own diagnosis, in the one section of the page written to
        // stop people blaming themselves.
        var section = PlainOf(CauseHeading);

        Assert.Matches(new Regex(@"for some people there is a\s*known reason", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"immune system is weakened", RegexOptions.IgnoreCase), section);

        // AND IT MUST NOT BECOME BLAME. Naming a risk factor in a self-blame
        // section is exactly where that could go wrong.
        // The wording tracks the page: /review round 2 found this section ending
        // on "nobody knows", so the addendum's last two clauses were reordered to
        // land on the reassurance instead (§12.6, never end on the bleak half).
        Assert.Matches(new Regex(
            @"changes nothing at all about whose\s*fault this is|not your fault",
            RegexOptions.IgnoreCase), section);

        // The block is BELOW "if it comes back" and ABOVE the outlook gate
        // (§12.9's demotion).
        var headings = Headings();
        Assert.True(headings.IndexOf(CauseHeading) > headings.IndexOf(BackHeading));
        Assert.True(headings.IndexOf("What might happen over time") > headings.IndexOf(CauseHeading));
    }

    // -------------------------------------------------- treatment and disagreement

    [Fact]
    public void SurgeryIsRuledOutWithItsReasonRatherThanAsserted()
    {
        // NCI PDQ, verified live, in six words: "Surgery is not used to treat
        // primary CNS lymphoma." StatPearls gives the reason: "CNS lymphoma is
        // not amenable to complete surgical resection due to its infiltrative
        // nature, multifocality, and microscopic seedlings protected by the
        // intact blood-brain barrier."
        var section = PlainOf(TreatedHeading);

        Assert.Matches(new Regex(@"Surgery does not treat it", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"spreads through brain\s*tissue rather than pushing it aside", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"often in several places", RegexOptions.IgnoreCase), section);

        // AND THE SAMPLE IS NOT AN ATTEMPT TO REMOVE IT. This is the distinction
        // the whole page rests on, and /tests/biopsy makes the same one for a
        // different reason.
        Assert.Matches(new Regex(
            @"The sample is taken to name it, not to remove it", RegexOptions.IgnoreCase),
            PlainOf(FindOutHeading));
    }

    [Fact]
    public void TheDisagreementsAreStatedAsDisagreementsAndNoWinnerIsNamed()
    {
        // §12.4 R2 and §12.12. Three genuine disputes, all verified live from the
        // guideline panels themselves rather than from commentators:
        //   consolidation -- "Only 1 randomized trial ... major protocol
        //     violations in one-third of randomized patients"
        //   rituximab -- EANO, "no consensus was met in the panel for a
        //     recommendation"
        //   chemotherapy into the spinal fluid -- studies both ways, a real harm,
        //     and "recently reported or ongoing PCNSL trials do not use" it
        var section = PlainOf(TreatedHeading);

        Assert.Matches(new Regex(@"this is where specialists disagree most", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"Trials have not\s*settled which is best", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"could not reach\s*agreement", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"genuinely\s*disputed", RegexOptions.IgnoreCase), section);

        // NO WINNER IS NAMED for the second phase, in either direction.
        var winner = new Regex(
            @"(?:transplant|radiation|chemotherapy)[^.]{0,40}(?:is better|is best|is safer|"
            + @"is the best option|works better)",
            RegexOptions.IgnoreCase);
        Assert.Matches(winner, "A transplant is better than radiation here.");
        Assert.False(winner.IsMatch(Plain),
            "the page names a winner among consolidation options, where the trials do not");
    }

    [Fact]
    public void TheWholeBrainRadiationWarningCarriesDirectionAndNoFigure()
    {
        // §12.4 R3. EANO and EHA-ESMO both verified live: the risk of delayed
        // harm to thinking is higher in older people and the guideline grades
        // avoiding it [I, D]. The incidence figures (a 5-year cumulative
        // 25%-35%) are NOT carried: this is the family of number §12.4 says gets
        // direction only, because the raw figure misleads in both directions.
        var section = PlainOf(TreatedHeading);

        Assert.Matches(new Regex(
            @"lasting problems with thinking and memory", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"higher in older\s*people", RegexOptions.IgnoreCase), section);
        Assert.Contains("/treatments/radiation-therapy#whole-brain-radiation",
            Section(TreatedHeading), StringComparison.Ordinal);
    }

    [Fact]
    public void TheTrialSentenceCarriesItsStrengthAndItsHonestLimit()
    {
        // The strongest recommendation in the whole source set, verified live:
        // "Enrolment in suitable prospective clinical trials should be offered to
        // every patient with PCNSL [I, A]." Grade I, A -- unusual, and worth the
        // reader knowing.
        //
        // AND THE CAVEAT THAT KEEPS IT HONEST, also verified: "suitable studies
        // may not be accessible for many patients". Without it, a reader whose
        // center has no trial reads a closed door as a failure.
        var section = PlainOf(TreatedHeading);

        // The wording tracks the page: /review round 1 replaced "enrolment in a
        // suitable trial should be offered to every patient" because `enrolment`
        // is a British form that no corpus gate catches, and because §12.6 asks
        // for a verb rather than a nominalisation. The claim is unchanged and the
        // grade behind it ([I, A], the strongest in the whole source set) is what
        // this guard exists for.
        Assert.Matches(new Regex(
            @"every patient should be offered a suitable trial", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"not always available near you", RegexOptions.IgnoreCase), section);
        Assert.Contains("/treatments/clinical-trials", Section(TreatedHeading),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheHospitalStayIsUsualAndNotUniversalBecauseItsSourceSaysSo()
    {
        // §12.13's shape, caught by reading the paper rather than the pack. The
        // source for "4-5 days" is a paper whose PURPOSE is to move this
        // treatment out of hospital: "While inpatient administration has been the
        // norm, outpatient administration, has been shown to be safe, effective,
        // and patient centered."
        //
        // Taking its duration without its argument would have published a rule
        // its only source is arguing against.
        var section = CuratedPage.Flatten(Regex.Replace(
            CuratedPage.ComposedSubsection(Page, "The hospital stays"), @"[*_]", ""));

        Assert.Matches(new Regex(@"usually means a few days in hospital", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"Not everybody stays in", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"without an\s*overnight stay", RegexOptions.IgnoreCase), section);

        // AND THE REASON THE STAY IS NOT THE INFUSION, which is the thing that
        // makes a multi-day admission make sense to somebody dreading it.
        Assert.Matches(new Regex(
            @"reason is not the medicine going in", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"getting it safely out again", RegexOptions.IgnoreCase), section);
    }

    // ------------------------------------------------- numbers and follow-up

    [Fact]
    public void ThePageCarriesNoShareNoDoseNoThresholdAndNoInterval()
    {
        // §12.4, §12.5. The sources are thick with figures: non-diagnostic biopsy
        // rates, a relative risk of 2.1 and 3.0, "up to 50%" ghost tumors,
        // seizures at 11-14% against 50-80%, eye involvement at 15-20%, relapse
        // at "up to 50%", neurotoxicity at 25-35%, methotrexate doses in g/m2,
        // creatinine thresholds, and a clearance level. NOT ONE reaches this page.
        var share = new Regex(
            @"\b" + CountWord + @"\s*(?:%|percent|per cent)|"
            + @"\b" + CountWord + @"\s+(?:in|out of)\s+(?:every\s+)?" + CountWord + @"\b|"
            // A MULTIPLE IS A FIGURE TOO, and it is the one this tumor's sources
            // press hardest: "Inconclusive biopsies are three times more likely."
            // The first version of this guard had NO branch for it, so its own
            // canary could not fire -- which made the whole ban unfalsifiable
            // rather than merely incomplete (§12.8: prove a guard can fire).
            + @"\b" + CountWord + @"\s+times\b|"
            + @"\b" + Fraction + @"\s+of\s+(?:all\s+)?(?:the\s+)?(?:\w+\s+){0,2}"
            + @"(?:people|patients|them|tumors|cases|adults)\b|"
            + @"\b(?:survival|survive|live for|life expectancy|lifespan|median)\b|"
            // Doses and levels, INCLUDING the per-square-metre form this disease's
            // literature states its main drug in, and the blood level the hospital
            // stay is ended by. Neither was covered by the first version.
            + @"\b\d+(?:\.\d+)?\s*(?:Gy|gray|mg|milligrams?|cm|mm|mcg|micromol|umol|g/m\d?)",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "Inconclusive biopsies are three times more likely.",
                     "Ghost tumors happen in up to 50% of cases.",
                     "Seizures occur in about 14% of people.",
                     "The median survival is two years.",
                     "The dose is 3 g/m2.",
                 })
        {
            Assert.Matches(share, known);
        }
        Assert.DoesNotMatch(share, Plain);

        // NO SCAN OR APPOINTMENT INTERVAL (WI-521). A reachable annual eye-exam
        // interval exists in the guideline and is deliberately not printed,
        // because a per-tumor timetable is a number a frightened reader CAN act
        // on (WI-520).
        // WIDENED AFTER /review ROUND 1, which pointed out that the first version
        // could not see either interval this page actually carries: `every few
        // weeks` (the CountWord alternation has no branch for "few") and a span
        // measured in days. A guard that misses the only two instances on the page
        // it guards is doing nothing, whatever its name says.
        var interval = new Regex(
            @"\b(?:every|each)\s+(?:" + CountWord + @"|few|couple(?: of)?)?\s*"
            + @"(?:days?|weeks?|months?|years?)\b|"
            + @"\bannual(?:ly)?\b|\b(?:six|three|twelve)[- ]month\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "An eye examination every year.",
                     "A scan every few months.",
                     "Bloods every couple of days.",
                 })
        {
            Assert.Matches(interval, known);
        }

        // ONE INTERVAL IS EXEMPT, DELIBERATELY AND WITH ITS REASON. §12.4 R1 keeps
        // ORIENTING DURATIONS IN ("radiation is usually Monday to Friday for about
        // six weeks" is the canonical example), and the treatment rhythm is one:
        // PMC8811119, cited in this page's own front matter and verified live,
        // states admissions "most commonly are scheduled every 2 weeks".
        //
        // What the ban is actually for is a SURVEILLANCE timetable -- a number a
        // frightened reader can act on between appointments (WI-520, WI-521) --
        // and this page publishes none, even though a reachable annual eye-exam
        // interval exists in the guideline. Exempted by its own sentence rather
        // than by loosening the branch, so the branch keeps its teeth.
        // TWO SENTENCES ARE EXEMPT, and both are the same class: WARD ROUTINE,
        // not a timetable the reader acts on between appointments.
        //
        //   * "Rounds usually come every few weeks" -- the treatment rhythm,
        //     sourced to PMC8811119 ("most commonly are scheduled every 2 weeks").
        //   * "Blood is taken every day to measure how much of the drug is left"
        //     -- what happens on the ward, and the sentence that explains why the
        //     stay has no fixed length.
        //
        // Widening the branch to see "few" and "days" made it fire on BOTH. That
        // is the THIRD time in this item a widened ban has fired on a CORRECT
        // sentence: the grade ban forbade "there is no grade 1 to 4 for this one",
        // and the figures canary failed in the opposite direction by being unable
        // to fire at all. The answer is the same each time -- enumerate the
        // legitimate cases rather than loosen the branch, so the branch keeps its
        // teeth (§12.8: a rule that fails a correct page is worse than no rule).
        //
        // What the ban is FOR is a surveillance timetable, a number a frightened
        // reader can act on between scans (WI-520, WI-521). This page publishes
        // none, even though a reachable annual eye-exam interval exists in the
        // guideline it cites.
        var scanned = Plain;
        foreach (var allowed in new[]
                 {
                     @"Rounds usually come every few weeks[^.]*\.",
                     @"Blood is taken every day[^.]*\.",
                 })
        {
            var before = scanned;
            scanned = Regex.Replace(scanned, allowed, " ", RegexOptions.IgnoreCase);

            // EACH EXEMPTION MUST STILL MATCH SOMETHING. An exemption whose
            // sentence has been reworded silently widens the allowance instead of
            // narrowing it, and nothing else would notice (§12.8, WI-541: the
            // exemption has to track the sentence it exempts).
            Assert.True(before != scanned,
                $"the exemption /{allowed}/ no longer matches anything on this page, so it has "
                + "stopped being an exemption and started being a hole");
        }
        Assert.DoesNotMatch(interval, scanned);

        // And the branch must still catch what it is for, after both exemptions.
        Assert.Matches(interval, "Your next scan is every three months.");
        Assert.Matches(interval, "An eye examination every year.");
    }

    [Fact]
    public void TheReportNewSymptomsRuleCarriesTheReasonThatMakesItWorthFollowing()
    {
        // EHA-ESMO, verified live: "only 20% of relapses are diagnosed on
        // surveillance MRI" [III, B]. The share is not printed; the DIRECTION is,
        // because it is what turns a piece of filler into an instruction somebody
        // will actually act on.
        var section = PlainOf(ScansHeading);

        Assert.Matches(new Regex(@"this is not a\s*formality", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(
            @"only a minority of returns are\s*found on a scan that was already booked",
            RegexOptions.IgnoreCase), section);

        // The eye examinations belong in follow-up too, which is what makes this
        // hub's follow-up unlike every other hub's.
        Assert.Matches(new Regex(@"eye examinations", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void TheRelapseSectionSaysThereIsNoStandardAndDoesNotEndOnThat()
    {
        // Verified live: "there are no approved therapies, and no widely accepted
        // 'standard-of-care' approaches for the treatment of refractory or
        // recurrent primary central nervous system lymphoma".
        //
        // §12.6: never end a section on a frightening sentence. A gap in the
        // evidence is frightening, so the page states it and then says what IS
        // used.
        var section = PlainOf(BackHeading);

        Assert.Matches(new Regex(@"no agreed standard for what to do next", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(
            @"not the same as nothing being available", RegexOptions.IgnoreCase), section);

        var last = CuratedPage.SentencesOf(section).Last();
        Assert.DoesNotMatch(new Regex(@"no approved|nothing", RegexOptions.IgnoreCase), last);
    }

    [Fact]
    public void TheOutlookGateIsClosedAndCarriesNoFigureAtAll()
    {
        // §12.5. The gate is consent to READ, not a licence to publish.
        var raw = Regex.Match(Page, @"^## What might happen over time.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(raw), "the outlook section has gone");

        // `\r?\n` everywhere: on a CRLF checkout `\s*\n\n` cannot match at all,
        // and a test broken on CRLF fails a break harness for free (§12.8, WI-528).
        Assert.Matches(new Regex(
            @"^## What might happen over time[ \t]*\r?\n[ \t]*\r?\n:::outlook",
            RegexOptions.Multiline), raw);
        Assert.Matches(new Regex(@"\r?\n:::\r?\n"), raw);

        var inside = Regex.Match(raw, @":::outlook(.*?)\r?\n:::", RegexOptions.Singleline)
            .Groups[1].Value;
        var flat = CuratedPage.Flatten(inside);

        Assert.DoesNotMatch(new Regex(@"\d"), inside);

        // AND NO FIGURE-SHAPED CLAIM IN WORDS, which is the half a digit ban
        // cannot see (§12.8, WI-527), including the DIRECTIONAL form.
        var told = new Regex(
            @"\blive as long as\b|\blife expectancy\b|\blifespan\b|\bsurviv\w*|\bdie of\b|"
            + @"\bcure[ds]?\b|\bthe vast majority\b|\bkinder than\b|\bworse than\b|"
            + @"\bnot as bad as\b|\bmore hopeful than\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(told, "People with this rarely survive long.");
        Assert.DoesNotMatch(told, flat);

        // THIS GATE'S OWN ANGLE, and the reason it is not a copy of any sibling's.
        // The corpus opening "Doctors sometimes describe outlook using numbers"
        // ships on eleven hubs; this one was written from scratch because the
        // honest problem here is different: the disease is rare, so the numbers
        // come from small studies of treatment that is no longer given.
        Assert.Matches(new Regex(@"unusually hard to read", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(@"rare", RegexOptions.IgnoreCase), flat);

        // §12.5's Kirkeboen framing: a group, a spread, and not a prediction --
        // asserted as the SENTENCES THAT TEACH each idea, not as bare words.
        //
        // The break harness caught the bare-word version as a SURVIVOR on both LF
        // and CRLF: deleting the sentence that teaches the spread left the word
        // "spread" alive in two later clauses that merely refer back to it, so
        // the gate could lose its explanation while the guard stayed green.
        // §12.12: a mutation has to remove the property rather than an instance
        // of it -- and a guard has to assert the property rather than a token.
        Assert.Matches(new Regex(
            @"describe what happened to a\s*\*{0,2}group\*{0,2} of people", RegexOptions.IgnoreCase),
            flat);
        Assert.Matches(new Regex(
            @"Inside any group there is a\s*spread", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(
            @"Some people do far better than its middle point", RegexOptions.IgnoreCase), flat);
        Assert.Matches(new Regex(@"not a forecast|never about one person", RegexOptions.IgnoreCase),
            flat);

        // It closes on a door rather than on a fact (§12.6).
        Assert.Matches(new Regex(@"still be here|ask whenever", RegexOptions.IgnoreCase), flat);

        // Nothing sits outside the gate in that section but the heading.
        var outside = raw
            .Replace(Regex.Match(raw, @":::outlook.*?\r?\n:::", RegexOptions.Singleline).Value, "",
                StringComparison.Ordinal)
            .Replace("## What might happen over time", "", StringComparison.Ordinal);
        Assert.True(string.IsNullOrWhiteSpace(outside),
            "something sits outside the outlook gate in that section: " + outside.Trim());
    }

    // ---------------------------------------------------------- the caregiver

    [Fact]
    public void NoGlossaryTermOnThisPageFiresADefinitionThatContradictsThePage()
    {
        // FOUND BY RENDERED READ 1, AND NOTHING ELSE COULD SEE IT.
        //
        // An earlier draft described leucovorin as "a rescue medicine". That is a
        // glossary term on this site, and it means something else entirely: a
        // seizure medicine kept at home, given up the nose or under the tongue.
        // The popover fired mid-sentence on a chemotherapy paragraph and handed
        // the reader a definition that was wrong for it.
        //
        // No gate could catch that. The tooltip's text lives in another file;
        // ContentCheck grades prose; both restatement guards normalise it away;
        // the render tests read flattened HTML. WI-541 met the ECHO form of this
        // (a tooltip repeating the page's own clause); this is the CONTRADICTION
        // form, which is worse, because the reader is told something untrue.
        //
        // Asserted as a property rather than a phrase: this page must not use any
        // glossary term whose definition is about something it is not discussing.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var term in new[] { "rescue medicine", "seizure action plan", "status epilepticus" })
        {
            Assert.DoesNotContain(term, body, StringComparison.OrdinalIgnoreCase);
        }

        // And the drug is still named, so the paragraph did not lose its content
        // when the term was removed (§12.8, WI-512: the other half of a removal).
        Assert.Contains("leucovorin", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("folinic acid", body, StringComparison.OrdinalIgnoreCase);

        // The canary: the term really is a glossary entry, so this ban is not
        // guarding nothing.
        Assert.True(
            File.Exists(Path.Combine(
                CuratedPage.BlocksRoot, "..", "glossary", "rescue-medicine.md")),
            "glossary/rescue-medicine.md has gone, so this guard no longer protects anything");
    }

    [Fact]
    public void TheCaregiverSectionAddsWhatIsOnlyTrueHere()
    {
        // §12.10: a caregiver section that paraphrases the block in different
        // words is the defect WI-525 wrote down and WI-526 repeated, and no
        // shingle check can see a paraphrase -- so the section promises a COUNT
        // and this guard counts.
        var care = PlainOf(CareHeading);

        // THIS ONE SENTENCE HAS BEEN WRONG TWICE, IN TWO DIFFERENT WAYS, AND BOTH
        // WERE CAUGHT BY SOMETHING THE OTHER COULD NOT SEE.
        //
        // Version 1 ("Three things belong to this one in particular") was
        // /tumors/acoustic-neuroma's sentence, borrowed with the template.
        // Version 2 ("Three of them are specific to this diagnosis") dodged that
        // collision and broke the prose: composed, it follows the block's seven
        // items, so "them" had no antecedent a reader could resolve. Only the
        // RENDERED read showed that.
        // Version 3 ("Three more things matter especially here") read correctly
        // and turned /tumors/brain-metastases RED, because that page's test uses a
        // shingle variant that strips NEITHER headings NOR link targets -- so the
        // §12.3-mandated heading, the literal `caregiver` directive token, and the
        // opening two words form one window, and that page opens "Three more".
        //
        // So the opening deliberately does NOT start with a count-word pair. Six
        // hubs open this section with "Three things" and three with "Two things";
        // they are invisible to each other only because their own tests strip
        // headings. See the /pm note in plan.md.
        Assert.Matches(new Regex(@"Beyond that, three things matter especially here",
            RegexOptions.IgnoreCase), care);
        var leads = Regex.Matches(CuratedPage.Flatten(Reader(Section(CareHeading))), @"\*\*(.+?)\*\*")
            .Select(m => m.Groups[1].Value)
            .ToList();
        Assert.True(leads.Count == 3,
            $"the caregiver section promises three things and carries {leads.Count}:\n  "
            + string.Join("\n  ", leads));

        // THIS HUB'S OWN CAREGIVER INSIGHT, and it is genuinely particular to
        // this tumor: the commonest first sign is a change in thinking or
        // personality, which the person living it usually cannot see.
        Assert.Matches(new Regex(
            @"You may be the reason the diagnosis gets made", RegexOptions.IgnoreCase), care);

        // THE SUMMARY MUST NOT NARROW OR RE-TIER THE RULE -- WI-540's round-2
        // defect, where a caregiver summary silently re-gated an emergency rule
        // eleven lines after the page widened it.
        Assert.Contains("/treatments/chemotherapy#fever-rule", Section(CareHeading),
            StringComparison.Ordinal);

        // And it must not invent a tumor-specific escalation the page refuses
        // to give anywhere else.
        var invented = new Regex(
            @"call an ambulance if[^.]{0,60}(?:confus|drowsy|weak)", RegexOptions.IgnoreCase);
        Assert.Matches(invented, "Call an ambulance if they get more confused.");
        Assert.False(invented.IsMatch(care),
            "the caregiver section invents an urgency tier the rest of the page declines to give");
    }

    // --------------------------------------------------------- onward doors

    [Fact]
    public void TheOnwardDoorsSitInTheSectionsWhoseReaderNeedsThem()
    {
        // WI-512's rule: presence was never the property, POSITION was.
        foreach (var (link, heading) in new[]
                 {
                     ("/treatments/chemotherapy#fever-rule", SymptomHeading),
                     ("/tests/mri", FindOutHeading),
                     ("/tests/biopsy", FindOutHeading),
                     ("/treatments/steroids", FindOutHeading),
                     ("/tests/pathology-report", ReportHeading),
                     ("/treatments/radiation-therapy#whole-brain-radiation", TreatedHeading),
                     ("/treatments/clinical-trials", TreatedHeading),
                     ("/tests/follow-up-scans", ScansHeading),
                     ("/tests/waiting-for-results", ScansHeading),
                     ("/get-help-now", "Where to get support"),
                 })
        {
            Assert.Contains(link, Section(heading), StringComparison.Ordinal);
        }
    }

    // --------------------------------------------------------- housekeeping

    [Fact]
    public void TheSectionsFollowTheSeventeenSectionHubOrder()
    {
        // §12.3, read from the spec rather than copied from the last hub --
        // §12.8's WI-510 lesson is that copying the previous page is how a
        // template quietly shrinks.
        Assert.Equal(
            [
                "The short version",                                               // 0
                "What is CNS lymphoma?",                                           // 1
                "Is it cancer? What does its grade mean?",                         // 2
                "Where does it grow, and why does it cause these symptoms?",       // 3
                "What symptoms does it cause?",                                    // 4
                "How do doctors find out it is this?",                             // 5
                "What do the words on my report mean?",                            // 6
                "How is it usually treated?",                                      // 7
                "What is treatment actually like, and what is normal afterwards?", // 8
                "Everyday life",                                                   // 9
                "Follow-up scans, and what to do while you wait",                  // 10
                "If it comes back",                                                // 11
                "Did I cause this?",                                               // self-blame, demoted
                "What might happen over time",                                     // 12, gated
                "For the person caring for someone with this",                     // 13
                "What to ask your team",                                           // 14
                "Where to get support",                                            // 15
            ],
            Headings());

        foreach (var sub in new[] { "The hospital stays", "What it does to you" })
        {
            Assert.Matches(new Regex($@"^### {Regex.Escape(sub)}\s*$", RegexOptions.Multiline),
                CuratedPage.Body(Page));
        }
    }

    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhereInTheCorpus() =>
        // NO ALLOWLIST, and that is the ruling. The first draft scored 38 shared
        // shingles here, 24 of them against /tumors/acoustic-neuroma, because
        // §12.9's template travels with the previous hub's sentences.
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // THE SECOND RESTATEMENT GUARD, AND THE REASON THIS FILE CARRIES BOTH.
        // The page-local variant keeps LINK TEXT and keeps the ask-list, and on
        // the first draft it alone saw /tests/biopsy and
        // /tests/waiting-for-results. A clean report from one guard says nothing
        // about the other (§12.8, WI-541).
        var pageShingles = LinkTextShingles(CuratedPage.ReaderText(Page)).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("cns-lymphoma.md", StringComparison.Ordinal))
            .Concat(Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"));

        foreach (var file in others)
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var overlaps = LinkTextShingles(CuratedPage.ReaderText(File.ReadAllText(file)))
                .Where(pageShingles.Contains)
                .Where(s => s.Split(' ').Count(w => !Stopwords.Contains(w)) >= 3)
                .Distinct()
                .ToList();

            Assert.True(overlaps.Count == 0,
                $"this page restates {slug} through text the shared guard cannot see "
                + "(link text, or the ask-list):\n  " + string.Join("\n  ", overlaps));
        }
    }

    private static readonly HashSet<string> Stopwords =
    [
        "the", "a", "an", "and", "or", "but", "is", "are", "was", "were", "be", "been",
        "it", "its", "that", "this", "those", "these", "of", "to", "in", "on", "at",
        "for", "with", "as", "by", "from", "you", "your", "they", "them", "their",
        "not", "no", "so", "if", "what", "which", "who", "how", "when", "there",
        "can", "cannot", "will", "would", "may", "might", "do", "does", "did", "have",
        "has", "had", "one", "than", "then", "out", "up", "about", "into", "over",
    ];

    /// <summary>
    /// Eight-word shingles under the PAGE-LOCAL rules: headings dropped, link
    /// TARGETS stripped but link TEXT kept, digits kept.
    /// </summary>
    private static IEnumerable<string> LinkTextShingles(string text)
    {
        text = Regex.Replace(text, @"^#{1,6} .*$", " ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\]\([^)]*\)", "] ");
        var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+")
            .Select(m => m.Value).ToList();
        for (var i = 0; i + 8 <= words.Count; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(8));
        }
    }

    [Fact]
    public void ThePageNeverMinimisesOrCharacterisesTheDiagnosis()
    {
        var reader = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(reader, Slug);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(reader),
                StringComparison.OrdinalIgnoreCase);
        }

        // THE PARTICULAR RISK ON THIS PAGE is the steroid response reading as
        // good news. The sources describe a tumor that can vanish off a scan in
        // days, which is the most quotable and most misleading fact about it.
        var comfort = new Regex(
            @"\bgood news\b|\bthe lucky (?:one|kind)\b|\bresponds (?:well|beautifully)\b|"
            + @"\bmelts away\b|\bdisappears for good\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(comfort, "The good news is it melts away on steroids.");
        Assert.DoesNotMatch(comfort, Plain);
    }

    [Fact]
    public void ThePageBalancesEveryEmphasisMarker()
    {
        // §12.8, WI-541: an unclosed `**` renders as two literal asterisks to a
        // patient while every other gate stays green, because both restatement
        // guards drop `[*_]`, `Plain` strips them deliberately, and the render
        // tests read flattened HTML.
        var offenders = new List<string>();

        foreach (var paragraph in Regex.Split(CuratedPage.Body(Page), @"\r?\n\s*\r?\n"))
        {
            var stripped = Regex.Replace(paragraph, "`[^`]*`", " ");

            foreach (var marker in new[] { "**", "%%" })
            {
                if (Regex.Matches(stripped, Regex.Escape(marker)).Count % 2 != 0)
                {
                    offenders.Add($"{marker} unclosed: {CuratedPage.Flatten(paragraph)}");
                }
            }
        }

        Assert.Empty(offenders);

        Assert.Single(Regex.Matches("**this one never closes.", @"\*\*"));
        Assert.NotEqual(0, Regex.Matches(CuratedPage.Body(Page), @"\*\*").Count);
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, and each is a form THIS page's sources would have supplied.
        // Several of the reader-level accounts of a methotrexate admission are
        // British, and the first draft of this page did say "the drip" before a
        // mechanical scan caught it. "a drip" and "the drip" are already on the
        // corpus list; these are the ones that are not.
        foreach (var form in new[]
                 {
                     "chemo ward", "day unit", "consultant", "GP", "sister",
                     "straight away", "being sick", "come round", "out of hours",
                     // /review round 1. Neither is on CuratedPage.BritishForms and
                     // both arrived from this page's sources: "enrolment" from the
                     // guideline's own wording, "carers" from the caregiver
                     // research. Flagged to /pm as corpus-wide gaps.
                     "enrolment", "carers",
                 })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }
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

        // The sources this page's load-bearing claims rest on, by name. EVERY ONE
        // was re-fetched and re-read against the LIVE publication rather than
        // taken from the item's research pack -- see
        // .claude/work_files/wi542/plan.md for what that verification changed.
        foreach (var owed in new[]
                 {
                     "PMC9437714",    // Blood 2022: the central steroid claim, and the 24-48h fallback
                     "PMC11148853",   // EHA-ESMO: the graded recommendations
                     "NBK545145",     // StatPearls: why resection is not the treatment
                     "PMC6142465",    // Batchelor: the plainest form of the caveat
                     "PMC8328013",    // WHO CNS5: the name, and NO grade
                     "PMC9214472",    // WHO-HAEM5: the second name, and immune sanctuaries
                     "PMC13525931",   // the pairing: it comes back
                     "PMC8811119",    // the admission, and "the norm" not universal
                     "PMC10000886",   // why the dose is high
                     "PMC6142584",    // what is unsettled
                     "aol.amegroups.org", // relapse: no standard of care
                     "rarediseases.org",  // tempo, seizures, the eye exam
                     "braintumor.org",    // the US patient-facing steroid sentence
                 })
        {
            Assert.Contains(urls, u => u.Contains(owed, StringComparison.Ordinal));
        }

        // THE SOURCE THE STUB CITED. NCI is cited here, and §12.1 permits that
        // for framing and supportive care while barring it for NAMING and
        // GRADING -- which is why this page's name comes from the CNS5 summary.
        // The barred-for-idiom domains stay off entirely.
        foreach (var barred in new[] { "cancerresearchuk.org", "mayoclinic.org", "macmillan.org.uk" })
        {
            Assert.False(urls.Any(u => u.Contains(barred, StringComparison.OrdinalIgnoreCase)),
                $"{barred} is cited; the front matter records why it is not used");
        }

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);

        // NO BRACKETED DIRECTIVE NAME ANYWHERE IN THE FRONT MATTER. This is the
        // WI-538 trap: CaregiverSectionTests locates the block with a raw IndexOf
        // over the WHOLE file, so a bracketed name in a comment becomes the first
        // match and the page is reported as not carrying the block.
        Assert.DoesNotMatch(new Regex(@"\[[A-Z][A-Z-]+\]"), front);
    }
}

/// <summary>The page as served.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class CnsLymphomaPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tumors/cns-lymphoma";

    private readonly WebApplicationFactory<Program> _factory;

    public CnsLymphomaPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServedAtItsExistingUrl()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("CNS lymphoma", html);

        // The description renders as the first paragraph a reader meets, so it
        // is pinned in its SOURCED form: an operation is not the treatment, and
        // a steroid can take the answer away.
        Assert.Contains("why a steroid can take the answer away", html);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolveIn(
            _factory.CreateClient(), Url, "where-to-get-support", "/get-help-now");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page. This page
        // deep-links the chemotherapy fever rule (twice) and the whole-brain
        // radiation section, so the check is not vacuous here.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheOutlookGateRendersClosedWithItsHeadingOutsideIt()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("What might happen over time", html);
        Assert.Contains("<details", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<details open", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheFourBlocksComposeAndTheFourExcludedOnesLeaveNoTrace()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var directive in new[]
                 {
                     "[MECHANISM]", "[ESCALATION]", "[CAUSES]", "[CAREGIVER]",
                     "[CROSSWALK]", "[TUMOR-BOARD]", "[SPINAL-CORD]", "[POSTERIOR-FOSSA-SYNDROME]",
                 })
        {
            Assert.DoesNotContain(directive, html, StringComparison.Ordinal);
        }

        Assert.Contains("Call an ambulance", html, StringComparison.Ordinal);
        Assert.Contains("You are allowed to ask questions", html, StringComparison.Ordinal);
        Assert.Contains("block the flow of fluid", html, StringComparison.Ordinal);

        // AND THE EXCLUDED BLOCKS' CONTENT DOES NOT -- asserted on the RENDERED
        // page, because that is the only place a wrongly-composed block becomes
        // visible. Read out of the block files rather than re-typed.
        //
        // BOTH SIDES FLATTENED. WI-541 recorded that the needle was flattened
        // while the haystack was raw, so the guard only fired because the
        // renderer happens to join plain hard wraps with a space -- a wrap
        // adjacent to inline markup KEEPS its newline in the HTML. That defect is
        // live, copied verbatim, in CraniopharyngiomaPageTests and
        // PituitaryTumorPageTests; both are shipped, so it is flagged for /pm
        // rather than fixed here, and this file uses the corrected form.
        var flatHtml = CuratedPage.Flatten(html);
        foreach (var name in new[]
                 { "crosswalk", "tumor-board", "spinal-cord", "posterior-fossa-syndrome" })
        {
            var block = File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, name + ".md"));
            var sentence = Regex.Matches(
                    CuratedPage.Flatten(CuratedPage.ReaderText(block)), @"[A-Z][^.]{45,90}\.")
                .Select(m => m.Value)
                .FirstOrDefault();
            Assert.False(string.IsNullOrEmpty(sentence),
                $"no sentence could be read out of {name}.md, so this guard proves nothing");

            Assert.DoesNotContain(sentence, flatHtml, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ThePageIsReachableFromTheTumorIndex()
    {
        // WI-412 shipped a page nothing linked to. This one is listed from
        // /tumors because taxonomy.yml carries a `slug: cns-lymphoma` entry -- a
        // property of two files agreeing rather than something anybody asserted.
        var html = await _factory.CreateClient().GetStringAsync("/tumors");

        Assert.Contains("/tumors/cns-lymphoma", html, StringComparison.Ordinal);
        Assert.Contains("CNS lymphoma", html, StringComparison.Ordinal);
    }
}
