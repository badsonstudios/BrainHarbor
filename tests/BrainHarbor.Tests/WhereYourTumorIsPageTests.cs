using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-567: <c>/where-your-tumor-is</c>. The first page on the site organised by
/// LOCATION rather than by type or by treatment, and the first §12.8 library page
/// whose subject is neither a test, a treatment, a wait, a document nor a
/// reference list.
///
/// THE RULING THIS PAGE TURNS ON (work_files/wi567/RULING.md, shipped as
/// content-pipeline §12.17): **this page does not repeat <c>[MECHANISM]</c>, it
/// answers the other half of the question.** The block is live on eighteen hubs
/// and owns *location to symptom* ("the symptom tells you where, the scan tells
/// you what"). This page owns *the word on your report, to plain language, to
/// what your team is weighing*. So a region entry here **teaches a word**: the
/// plain name, then the word the report uses, and a route where there is a sourced
/// one to give. What it never does is tell the reader **what a tumor there does to
/// them** — that is the block's job on eighteen pages, and it is routed to it.
///
/// That is not a tidiness rule. It is the structural reason this page cannot become
/// the thing the Wave 6 preamble forbids: an entry that teaches a word has nowhere
/// for a risk to go. An entry made of *name + what happens to you here* would grow
/// one, one review round at a time.
///
/// Three corrections to how that was first written down, because an overstated
/// docstring is what stops the next reader checking:
///
///   * **A route is not on every entry.** The four lobes and the cerebellum route
///     nowhere, because naming the types that sit there would be a claim this page
///     cannot source.
///     The skull base is the one entry whose list of types is SOURCED (MSKCC's own
///     list); the pituitary entry names two from this site's pages, openly, after
///     round 6 found it claiming they were the only two — which is false, and was
///     this item's third closed-count error.
///   * **Function is not the banned thing.** Some entries do say in a clause what a
///     part of the brain is for, because a report word with no meaning attached
///     teaches nothing. Only three of the nine do (front, upper back, back); the
///     other six say where the place is and stop, deliberately: the block owns
///     *balance, coordination and walking* and *understanding what people say*, and
///     borrowing those back is the restatement this page exists not to do.
///   * **What IS banned is what a tumor there does to the reader** — symptoms,
///     outcomes, rankings and plans. That is the guard below, and it is the ruling.
///
/// What is pinned here is where this page could leave a reader worse off:
///
///   * A LOCATION-TO-RISK TABLE, in any of its disguises. Only 23% of surveyed
///     neurosurgeons apply any eloquence grading scale and the authors say the
///     lack of consensus "limits the reliability of eloquence as a descriptor of
///     tumor location" (PMC12367934). A lookup table here would publish one
///     group's opinion as a fact about the reader's brain.
///   * THE REFUSED CLAIM. The backlog asked for "a tumor can be small, slow and
///     low-grade and still be an emergency because of the plumbing". Nothing
///     verified supports it and the best source prints the opposite qualifier
///     (tumors block CSF "when large enough"). It is banned by name in the one
///     section where inventing a reassurance-shaped fact would be least
///     forgivable. Same shape as WI-549's "10 to 20 minutes".
///   * PERCENTAGES AND PROGNOSIS. The item's own acceptance bans them outright,
///     and the tectal literature's own resection range across centres is 2.3% to
///     100%, which is the argument for publishing none of them.
///   * RESTATING /treatments/craniotomy, which owns *how much comes out* —
///     maximal safe resection, the report words, "a subtotal resection is not a
///     failed operation" — and the word *eloquent*. This page owns **whether
///     there is an operation at all** and routes for the rest.
///   * RESTATING THE SECOND-OPINION ANSWER. /tumors/all-brain-tumors already has
///     it, on the hub written for exactly this reader. This page supplies the
///     REASON and links.
///   * A FOURTH LIST OF RED FLAGS. <c>[ESCALATION]</c> owns the tiers on 24
///     pages; this page includes it rather than writing its own.
///   * PRE-EMPTING WAVE 6. No tectal section (WI-571), no diagram (WI-572), no
///     visual-field or driving content (WI-573), and the /tumors/meningioma
///     duplication MechanismBlockTests pins is left pinned (WI-569).
/// </summary>
public sealed class WhereYourTumorIsPageContentTests
{
    private const string Slug = "where-your-tumor-is";

    private const string ShortHeading = "The short version";
    private const string MeansHeading = "What \"where it is\" tells you, and what it does not";
    private const string WhyHeading = "Why your team keeps coming back to where it sits";
    private const string RegionsHeading = "The regions, in plain words and in your report's words";
    private const string HowLongHeading = "How long before this turns into a plan?";
    private const string FluidHeading = "When where it sits makes it urgent: the fluid";
    private const string SurgeryHeading = "Can they just take it out?";
    private const string NoListHeading = "Why there is no list here of which places are dangerous";
    private const string FindingHeading = "How to find out what yours is called";
    private const string DecidesHeading = "Who decides, and how will you hear?";
    private const string QuestionsHeading = "What to ask your surgeon";
    private const string NextHeading = "Where to go next";

    /// <summary>
    /// The page, with line endings normalised to LF.
    ///
    /// Not a nicety. This repo is checked out with <c>text=auto</c>, so every
    /// content file is CRLF in the working tree and LF in the index, and a pattern
    /// written with <c>$</c> after a literal <c>}</c> matches NOTHING on a CRLF
    /// checkout — the <c>\r</c> sits between them. Both of this file's
    /// region-scanning regexes were silently finding ZERO entries when first
    /// written, which made one of them report a failure and the other one pass
    /// vacuously. WI-568 recorded the same trap twice; it is now handled in one
    /// place, and every regex below reads this rather than the raw file.
    /// </summary>
    private static string Page =>
        CuratedPage.Read("where-your-tumor-is.md").Replace("\r\n", "\n");

    private static string Reader => CuratedPage.ReaderText(Page);

    /// <summary>
    /// The WI-105 authoring markers stripped from a FRAGMENT — a section, a region
    /// entry — rather than from a whole page.
    ///
    /// <c>CuratedPage.ReaderText</c> cannot be used on a fragment: it calls
    /// <c>Body</c>, which looks for the end of the front matter, finds nothing, gets
    /// <c>-1</c> and slices from index 3. /review round 1 found one call site doing
    /// that; round 2 found three more, including the guard for this item's central
    /// ruling, which was reading every region entry minus its first three
    /// characters. One helper now, so there is no fourth.
    /// </summary>
    private static string ReaderFragment(string fragment) =>
        Regex.Replace(Regex.Replace(fragment, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The title and description, which <c>ContentPage.cshtml</c> renders as the
    /// heading and the first paragraph under it.
    ///
    /// Round 2 added this because every prose guard on this page was body-scoped and
    /// the description carried one of round 1's blockers in it: the page said "the
    /// one time where it sits makes it urgent" in the first sentence a reader meets,
    /// after that claim had been rescoped everywhere a test could see. §12.8,
    /// WI-524 and WI-528, and the CtScanPageTests convention.
    /// </summary>
    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    /// <summary>Everything a reader meets: the headline and the body.</summary>
    private static string Flat =>
        Headline + " " + CuratedPage.Flatten(Reader);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section, flattened, with the WI-105 authoring markers removed.
    ///
    /// NOT <c>ReaderText(Section(...))</c>, which is what this was at /review round
    /// 1 and which was silently dropping the first three characters of every
    /// section it read: <c>Section</c> returns a fragment with no front matter, so
    /// <c>ReaderText</c>'s <c>Body</c> found no <c>---</c>, got <c>-1</c>, and
    /// sliced from index 3. Eight assertions below were reading each section minus
    /// its opening word, and the property they exist to check is §12.6's "answer in
    /// the first sentence under the heading" — the one thing that reading path
    /// cannot see.
    /// </summary>
    private static string FlatSection(string heading) =>
        CuratedPage.Flatten(ReaderFragment(Section(heading)));

    // ------------------------------------------------------------------
    // The §12.2 contract and the §12.8 slot ruling
    // ------------------------------------------------------------------

    /// <summary>
    /// THE TISSUE RULE IS HEDGED, BECAUSE THIS IS THE PAGE ABOUT THE EXCEPTION TO IT.
    ///
    /// The page said flatly "The name comes from tissue, tested in a lab" and "When the
    /// lab result comes, that is the name". Three things the corpus says disagree, and
    /// all three are keyed to LOCATION, which is this page's whole subject:
    /// <c>/tumors/all-brain-tumors</c> ("a narrow exception, for a thing sitting
    /// somewhere that would be too risky to take a piece of"),
    /// <c>/treatments/watch-and-wait</c> ("a best guess for a name, not a confirmed
    /// one"), and this page's OWN recorded Columbia quote ("A biopsy is typically not
    /// performed because the brainstem is a difficult area to access").
    ///
    /// The readers it was false for are the ones this page reaches hardest: the
    /// brainstem reader its own entry reassures, the parent the block's new door sends
    /// here from <c>/tumors/dipg</c>, and the watched reader in its own list of five.
    /// WI-568's defect shape, in a new item. Found at /review round 7.
    /// </summary>
    [Fact]
    public void TheNameComesFromTissueIsHedgedAndItsExceptionIsRouted()
    {
        foreach (var unhedged in new[]
        {
            "The name comes from tissue",
            "When the lab result comes, that is the name",
            "only tissue can name it",
            "the name always comes from",
        })
        {
            Assert.DoesNotContain(unhedged, Flat, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE PROPERTY, not only those four wordings, because a flat restatement in
        // fresh words passes the list above while the canary below still stands (round
        // 8's note on this file's own recurring weakness): every sentence that mentions
        // tissue, a lab result or taking a piece carries a hedge or a condition.
        // Links stripped whole first: a route's label is the destination's title, and
        // "[How tissue is taken](/tests/biopsy) is that appointment" is the shape this
        // ruling asks for rather than a claim about tissue.
        // The trigger is the CO-OCCURRENCE, not either half. "before there is tissue and
        // after" is a point in time, not a claim about naming, and a one-word trigger
        // fired on it — a guard that forbids a correct sentence is worse than no guard.
        var claims = Regex
            .Matches(Regex.Replace(Flat, @"\[[^\]]*\]\([^)]*\)", " "),
                @"[^.!?]*\b(tissue|lab result|a piece|a sample)\b[^.!?]*")
            .Select(m => m.Value.Trim())
            .Where(s => s.Length > 25)
            // The naming trigger is a NOUN PHRASE about the name, not any inflection of
            // "name". Round 9: the broader form fired on ACS's "to get a piece of it so
            // it can be named", which is the PURPOSE of a biopsy rather than a claim
            // about where a name comes from — and it only passed before because the
            // hedge list still contained "can ", the entry that made the guard
            // unbreakable in the first place.
            .Where(s => Regex.IsMatch(s, @"\b(the name|a name|confirmed|the diagnosis)\b",
                RegexOptions.IgnoreCase))
            .ToList();

        Assert.True(claims.Count >= 2, // two sentences assert where a name comes from
            $"only {claims.Count} sentence(s) mention tissue or a lab result, so this "
            + "guard is reading almost nothing");

        string[] hedges =
        [
            // "can " and "would" were REMOVED at /review round 9: "Only a lab can give
            // it a name from tissue" is a flat restatement in fresh words and "can "
            // let it through, which is the exact case this guard's own comment says it
            // exists to catch. The TWO sentences the guard collects today survive on
            // "usually" and "If ", so the page stays green and the guard becomes
            // breakable. (Round 9's narrowing of the trigger took a third out of scope;
            // "going to be" is the entry it used to need and is kept for when it returns.)
            // ROUND 11's honest note: "where " is ALSO satisfied by the unrelated clause
            // "and where it sits" inside the first of those two sentences, so for that one
            // the property half is weaker than it looks and the exact-phrase pin below is
            // what actually protects it. Kept anyway, because "where a piece is taken" is
            // a real hedge shape this page uses and dropping it would fail a correct
            // future sentence (§12.8).
            "usually", "often", "if ", "where ", "may ", "most", "going to be",
            "rather than", "some",
        ];
        var flatClaims = claims
            .Where(s => !hedges.Any(h => s.Contains(h, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(flatClaims.Count == 0,
            "a sentence about tissue or a lab result carries no hedge or condition, on the "
            + "page whose own subject is the location-keyed exception to that rule:\n  "
            + string.Join("\n  ", flatClaims));

        // The hedge, in the short version, where most readers stop.
        Assert.Contains("A name usually comes from tissue", FlatSection(ShortHeading),
            StringComparison.Ordinal);

        // And the exception, routed rather than restated, in the section that is the
        // reader's path from a place to a name.
        var finding = FlatSection(FindingHeading);
        Assert.Contains("if no piece is taken", finding, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/tests/biopsy#is-there-a-way-to-find-out-without-one", finding,
            StringComparison.Ordinal);
        Assert.Contains("/treatments/watch-and-wait#without-a-sample", finding,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The three round-6 and round-7 fixes that nothing was pinning. /review round 7:
    /// a grep over this file returned zero for every one of them, and this file's own
    /// convention is that a review fix gets a guard — otherwise the next edit undoes it
    /// silently and the round that found it has to find it again.
    /// </summary>
    [Fact]
    public void TheFixesFromTheLastTwoRoundsArePinned()
    {
        var fluid = FlatSection(FluidHeading);

        // The shunt route's SCOPE. /treatments/shunts#warning-signs makes the whole
        // list right-away or the emergency department, which is stronger than the
        // general block — and it is written for somebody who already has a shunt.
        // blocks/escalation.md links the same anchor on the same composed page, so
        // without this the page's own sentence could go and nothing would notice.
        Assert.Contains("/treatments/shunts#warning-signs", fluid, StringComparison.Ordinal);
        Assert.Contains("written for you rather than for everybody",
            fluid, StringComparison.Ordinal);

        // The pediatric warning is SCOPED to the surgery section rather than thrown
        // over the whole page, and it keeps the parent in the fluid section.
        var means = FlatSection(MeansHeading);
        Assert.Contains("One part of it needs a warning, and it is the surgery section",
            means, StringComparison.Ordinal);
        // Round 8 reworded this: "the part of this page most likely to be about your
        // child" is a pediatric epidemiology claim with no quote behind it and no
        // corpus home to route it to. What is left is a statement about the SECTION,
        // which that section's own sources carry.
        // Round 9 went one step further: "it applies to a child as much as to an adult"
        // was itself unsourced, so it is now a negative claim about the section plus a
        // route to the page that carries the child-specific version (a baby warning
        // list on /treatments/shunts).
        Assert.Contains("is not adult-only", means, StringComparison.Ordinal);

        // AND IT ROUTES TO THE UNGATED LIST. /review round 10: round 9 sent the parent
        // to /treatments/shunts for "the signs to watch for in a baby", and that page's
        // only baby list sits under "If you have a shunt and any of these happen". The
        // ungated, correctly tiered one is on /tumors/pediatric-brain-tumor, which this
        // paragraph already links — so the clause now points there and nowhere else.
        Assert.Contains("/tumors/pediatric-brain-tumor", means, StringComparison.Ordinal);
        Assert.DoesNotContain("/treatments/shunts", means, StringComparison.Ordinal);
        Assert.Contains("the signs of it in a baby", means, StringComparison.Ordinal);
        foreach (var unsourced in new[]
        {
            "most likely to be about your child",
            "applies to a child as much as to an adult",
        })
        {
            Assert.DoesNotContain(unsourced, means, StringComparison.Ordinal);
        }
        Assert.DoesNotContain("The rest of it does not transfer cleanly", means,
            StringComparison.Ordinal);

        // And the tumor board is not asserted as the universal path: blocks/tumor-board.md
        // says it "tends to happen when a case is complicated or something is hard to
        // read", and /tests/mri says a scan "may" go to one.
        // PAGE-WIDE. /review round 8: this was scoped to slot 4, and slot 9 carried the
        // flat version of the same claim 190 lines later ("The decision is usually not
        // made alone"), contradicting [TUMOR-BOARD]'s own hedge on the page it links
        // to. Four corpus pages hedge it and none of them says "usually".
        Assert.Contains("for a case that is complicated or hard", Flat,
            StringComparison.Ordinal);
        foreach (var flat in new[]
        {
            "usually not made alone", "usually comes out of a meeting",
            "your case will be discussed", "is very likely to be discussed",
        })
        {
            Assert.DoesNotContain(flat, Flat, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Contains("often not made by one person alone", FlatSection(DecidesHeading),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The three round-9 fixes that would otherwise have shipped unpinned, and the
    /// §12.8 length contract for slot 0, which nothing was measuring.
    ///
    /// The first is the one that matters. Round 8 added a route to
    /// <c>/tumors/brain-metastases</c> for the reader told there is more than one spot
    /// — which keys a TYPE page off a SCAN FEATURE, on the page whose own argument is
    /// that a place is not a diagnosis. The destination opens "A brain metastasis is
    /// the cancer you already have"; <c>/tumors/cns-lymphoma</c> records multifocality
    /// in a primary. <c>/tumors/all-brain-tumors</c> already has the corpus-correct
    /// version and it is CONDITIONAL, gated on "if it turns out to have come from
    /// somewhere else". This page now routes to that fork instead.
    /// </summary>
    [Fact]
    public void TheRoundNineFixesArePinnedAndSlotZeroIsWithinItsContract()
    {
        // More than one spot is a scan feature, and it routes to the fork rather than
        // to a diagnosis.
        // Scoped, because the property is that the fork sits where the multi-spot
        // question is asked. A page-wide Contains() is satisfied by it drifting into
        // "Where to go next", which is this file's most-repeated lesson about itself.
        Assert.Contains("/tumors/all-brain-tumors#primary-or-secondary",
            FlatSection(WhyHeading), StringComparison.Ordinal);
        Assert.DoesNotContain("/tumors/brain-metastases", Flat, StringComparison.Ordinal);
        Assert.Contains("More than one spot does not by itself say where they", Flat,
            StringComparison.Ordinal);

        // The positional anatomy the recorded AANS quotes do not carry. The reason is
        // in the front matter; this is what stops it coming back.
        foreach (var unsourced in new[]
        {
            "midbrain** at the top", "medulla** at the bottom",
            "fourth ventricle** lower down", "pons** in the middle",
        })
        {
            Assert.DoesNotContain(unsourced, Page, StringComparison.OrdinalIgnoreCase);
        }
        // And the one position that IS recorded, so this is not a bare ban.
        Assert.Contains("in the center of the brain", Flat, StringComparison.Ordinal);

        // §12.8 slot 0 is "3 to 5 sentences", for the reason the template gives:
        // readers consume 20 to 28% of a page. The shipped corpus runs 4 to 8, and this
        // page reached 15 before anything measured it — on the one section where length
        // costs the most. The ceiling is the corpus's, not the contract's, because no
        // shipped page meets the contract literally and failing them all would be a
        // rule that fails a correct page (§12.8).
        // NOT CuratedPage.SentencesOf. It splits on `(?<=[.!?])\s+`, and slot 0 is
        // written almost entirely in the bold-lead style, so a sentence ending `…it.**`
        // does not split — a systematic undercount on exactly this page's style, on the
        // one property this guard exists to measure (/review round 10). And the ceiling
        // is the number the message quotes, not one more than it.
        var sentences = Regex.Split(FlatSection(ShortHeading), @"(?<=[.!?])\**\s+")
            .Where(s => s.Trim().Length > 0)
            .ToList();
        Assert.True(sentences.Count <= 8,
            $"the short version is {sentences.Count} sentences; the longest shipped one "
            + "in the corpus is 8, and §12.8 asks for 3 to 5");
    }

    [Fact]
    public void TheFrontMatterCarriesTheContractFields()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("slug: where-your-tumor-is", front, StringComparison.Ordinal);
        Assert.Contains("reviewed: 2026-09-24", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);
        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.True(urls.Count >= 6, $"only {urls.Count} sources for a page this load-bearing");
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // Every source carries an accessed date, because "re-fetched and read live"
        // is the claim the front matter makes about itself.
        var accessed = Regex.Matches(front, @"accessed: \d{4}-\d{2}-\d{2}").Count;
        Assert.Equal(urls.Count, accessed);
    }

    /// <summary>
    /// The universal library slots are 0, 1, 3, 4, 9 and 10 (§12.8, narrowed at
    /// WI-507 and WI-508). §12.15's rule governs what may happen to one: answered,
    /// re-headed, or routed, and never dropped.
    ///
    /// Slot 3 is the region list, bent into itself the way WI-509's reference list
    /// bent it. Slot 4 answers itself with the SHAPE and then routes to the page
    /// that owns the wait, which is WI-509's precedent exactly. Slot 5 has no
    /// referent at all — a location does not feel like anything, its symptoms do —
    /// and is ROUTED rather than dropped, which the next test pins.
    /// </summary>
    [Fact]
    public void EveryUniversalSlotIsAnsweredUnderItsOwnHeading()
    {
        // Six universal slots plus slot 11 ("Where to go next"), which §12.8 makes
        // CONDITIONAL — "whenever there is somewhere real to send the reader" — and
        // which this page plainly has. It is in the list because it is CARRIED, not
        // because it is owed.
        string[] slots =
        [
            ShortHeading, MeansHeading, RegionsHeading, HowLongHeading,
            DecidesHeading, QuestionsHeading, NextHeading,
        ];

        foreach (var heading in slots)
        {
            Assert.Contains($"## {heading}", Page, StringComparison.Ordinal);

            // A heading with nothing under it satisfies presence and answers
            // nobody. WI-508's trap.
            Assert.True(FlatSection(heading).Length > 200,
                $"'{heading}' is a heading with barely anything under it");
        }

        // Slot 4's heading asks a question, so it must answer it rather than only
        // pointing elsewhere (§12.8, WI-509: "a heading that asks a question and
        // answers only 'see that other page' is the WI-506 trap in a new coat").
        //
        // WHAT IT ANSWERS WITH IS A SEQUENCE, NOT A DURATION, and that is the
        // /review round 1 fix. The first draft printed "within days" and "days to
        // weeks", neither of which any source on this page carries, while
        // /tests/waiting-for-results publishes sourced figures for the same wait
        // and says which parts vary by hospital. WI-549's "10 to 20 minutes" shape.
        // So the shape is the three-step order, and the numbers are routed.
        var howLong = FlatSection(HowLongHeading);
        Assert.Contains("The place is written down first", howLong, StringComparison.Ordinal);
        Assert.Contains("comes later", howLong, StringComparison.Ordinal);
        // Round 7 reworded this: the old version said "for MOST PEOPLE the plan is only
        // settled once a piece of the tumor has been tested", which is an unsourced
        // majority claim of the shape round 6 removed from the surgery section, and it
        // also asserted the tumor board as the universal path. The three-step sequence
        // is what survives, and the third step is conditional.
        Assert.Contains("Where a piece of the tumor is going to be tested", howLong,
            StringComparison.Ordinal);
        Assert.DoesNotContain("for most people", howLong, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/tests/waiting-for-results", howLong, StringComparison.Ordinal);

        // And the durations stay out, PAGE-WIDE. /review round 2: the first version
        // of this ban was scoped to this section, and two more unsourced durations
        // were standing in the section above it ("usually written down within days",
        // "offered surgery in a week") under a paragraph claiming the page prints no
        // timings at all. /tests/mri, which owns that wait, refuses to give a figure
        // for it. A guard scoped to the section the defect was found in is green
        // where it arrives, which is the same lesson the ranking guard was widened
        // for one round earlier.
        foreach (var duration in new[]
        {
            // ROUND 3 dropped "a few days" and "in days" from this list: §12.4 R1
            // keeps orienting durations IN, and "in the days before surgery" is a
            // sequence rather than a promised timing. A ban that would fail a correct
            // sentence is worse than no ban (§12.8, WI-509).
            "within days", "days to weeks", "seven to ten", "in a week",
            "within a week", "a matter of days", "takes about",
        })
        {
            Assert.DoesNotContain(duration, Flat, StringComparison.OrdinalIgnoreCase);
        }

        // The positive half, so the page still says whose the wait is. Scoped to the
        // section that used to carry the deleted durations, because that is where the
        // reader asks — a page-wide Contains() is satisfied by the route drifting
        // into "Where to go next", which is the inverse of this test's own lesson.
        Assert.Contains("/tests/mri", FlatSection(MeansHeading), StringComparison.Ordinal);

        // Slot 9's own §12.8 warning: do not promise a timing the page cannot give.
        // The property is that the page concedes the answer is LOCAL and then gives
        // the reader the thing it can give, which is what to ask for before leaving.
        var decides = FlatSection(DecidesHeading);
        Assert.Contains("differs from hospital to hospital", decides, StringComparison.Ordinal);
        Assert.Contains("write down a name", decides, StringComparison.Ordinal);
    }

    /// <summary>
    /// Slot 5 ("what does it feel like?") is ROUTED, not dropped, and the route is
    /// the SAME door the block sends a reader here through, run the other way. The
    /// two-way door is the shipped shape of WI-568's deferral, so the route being
    /// present is the item's own acceptance criterion and not a nicety.
    /// </summary>
    [Fact]
    public void TheSymptomQuestionIsRoutedToTheBlockThatOwnsItRatherThanAnswered()
    {
        // Scoped to the region list, where a reader looking for symptoms actually
        // stands. A whole-page Contains() would also pass with the route sitting
        // alone in "Where to go next", which is not where the question is asked.
        Assert.Contains("/tumors/all-brain-tumors#where-is-this-coming-from",
            Section(RegionsHeading), StringComparison.Ordinal);

        Assert.DoesNotContain("## What does it feel like", Page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------
    // THE CENTRAL RULING: nine regions, report words, and no symptom list
    // ------------------------------------------------------------------

    /// <summary>
    /// The nine regions, in the order <c>[MECHANISM]</c> uses, each with an
    /// EXPLICITLY WRITTEN anchor. WI-509's rule: Markdig derives an id from heading
    /// TEXT, so a reworded heading silently breaks every inbound link, and these
    /// anchors are a published interface WI-569 through WI-573 will link into.
    /// </summary>
    [Fact]
    public void TheNineRegionsAreThereInTheBlocksOrderWithWrittenAnchors()
    {
        (string Anchor, string ReportWord)[] regions =
        [
            ("front", "frontal lobe"),
            ("side", "temporal lobe"),
            ("upper-back", "parietal lobe"),
            ("back", "occipital lobe"),
            ("cerebellum", "cerebellum"),
            ("brainstem", "brain stem"),
            ("ventricles", "ventricles"),
            ("skull-base", "skull base"),
            ("pituitary", "pituitary"),
        ];

        var entries = Regex.Matches(Page, @"(?m)^### (.+?) \{#([a-z-]+)\}$")
            .Select(m => (Heading: m.Groups[1].Value, Anchor: m.Groups[2].Value))
            .ToList();

        Assert.Equal(regions.Length, entries.Count);
        Assert.Equal([.. regions.Select(r => r.Anchor)], [.. entries.Select(e => e.Anchor)]);

        // WI-548's "derive, never type": the property is that this page runs in the
        // SAME ORDER as the block, and a typed list is green after the block is
        // reordered. Derived from an independent walk of the block's own bullets,
        // which is the half WI-548 found "derive" is not enough without.
        // Body, not the whole file: the front-matter source comments quote several
        // of these phrases, and the only reason an IndexOf over the raw file worked
        // was that the cases happened to differ (/review round 2).
        var block = CuratedPage.Body(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md"))
                .Replace("\r\n", "\n"));
        var bullets = Regex.Matches(block, @"(?m)^- \*\*(.+?)\*\*").Count;
        Assert.Equal(regions.Length, bullets);

        var blockOrder = new[]
        {
            "Front of the brain", "Side of the brain", "Upper back part",
            "Back of the brain", "Cerebellum", "Brainstem", "Deep in the middle",
            "The floor of the skull", "Behind the eyes",
        };
        var positions = blockOrder
            .Select(marker => block.IndexOf(marker, StringComparison.Ordinal))
            .ToList();
        Assert.DoesNotContain(-1, positions);
        Assert.Equal([.. positions.OrderBy(i => i)], positions);

        // The plain-language name comes FIRST and the report's word SECOND. The
        // heading is the plain name, so the report word must NOT be in it.
        foreach (var (anchor, word) in regions)
        {
            var entry = entries.Single(e => e.Anchor == anchor);
            Assert.DoesNotContain(word, entry.Heading, StringComparison.OrdinalIgnoreCase);

            // The report word must be in the entry's PROSE, in bold, which is how
            // the page teaches it. /review round 6: a bare Contains() for
            // "pituitary" was satisfied by the URL /tumors/pituitary-tumor, so
            // deleting the taught word left the test green.
            var body = CuratedPage.Flatten(ReaderFragment(
                CuratedPage.ComposedSubsection(Page, entry.Heading)));
            Assert.Contains($"**{word}**", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// THE REGIONS PREAMBLE, which is the part of that section nothing was reading.
    ///
    /// <c>NoRegionEntryTellsTheReaderWhatATumorThereDoesToThem</c> matches
    /// <c>### … {#anchor}</c> blocks only, so the paragraphs between the <c>##</c>
    /// heading and the first entry were covered by a <c>length > 200</c> check and one
    /// link assertion the other paragraphs satisfied. /review round 7 found round 6's
    /// own fix sitting in there — the paragraph for the reader whose report word is
    /// none of the nine, which is the reader Wave 6 exists because of. Deleting it was
    /// green.
    ///
    /// So the preamble is held to the same rule as the entries, and its two promises
    /// are pinned: the list is places in the HEAD (the cord is a different subject
    /// with different escalation rules), and a word that is not one of the nine is
    /// answerable by a question the reader can ask.
    /// </summary>
    [Fact]
    public void TheRegionsPreambleIsHeldToTheSameRuleAsTheEntries()
    {
        var section = Section(RegionsHeading);
        var preamble = CuratedPage.Flatten(ReaderFragment(
            section[..section.IndexOf("### ", StringComparison.Ordinal)]));

        Assert.True(preamble.Length > 400,
            $"the regions preamble is {preamble.Length} characters, so either it has "
            + "been gutted or this guard is reading the wrong thing");

        // The two promises.
        Assert.Contains("These nine are places in the head", preamble, StringComparison.Ordinal);
        Assert.Contains("/tumors/spinal-cord-tumor", preamble, StringComparison.Ordinal);
        Assert.Contains("is not one of these nine", preamble, StringComparison.Ordinal);
        Assert.Contains("Ask which of these nine yours is in, or nearest to", preamble,
            StringComparison.Ordinal);
        Assert.Contains("/tumors/all-brain-tumors#where-is-this-coming-from", preamble,
            StringComparison.Ordinal);

        // And the same ban the entries carry, because a symptom or a plan claim is no
        // more welcome four lines above an entry than inside one.
        string[] banned =
        [
            "seizure", "headache", "vomit", "weakness", "numbness", "dizzi",
            "double vision", "hearing loss", "survival", "prognosis", "risky",
            "dangerous", "safer", "more serious", "worse", "may not be a good option",
        ];
        var found = banned
            .Where(b => preamble.Contains(b, StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.True(found.Count == 0,
            "the regions preamble has started saying what a tumor somewhere does to the "
            + "reader: " + string.Join(", ", found));

        // WI-571 owns tectal glioma and the tumors whose only name is a location. The
        // preamble answers that reader WITHOUT naming any of them, which is what keeps
        // this item out of that one.
        foreach (var word in new[] { "tectal", "pineal", "suprasellar", "thalam" })
        {
            Assert.DoesNotContain(word, preamble, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// THE RULING, AS A BREAKABLE ASSERTION. A region entry teaches a WORD and
    /// offers a ROUTE. The moment one starts saying what a tumor there does to you,
    /// this page has begun reimplementing <c>[MECHANISM]</c> on one side and
    /// growing a location-to-risk table on the other.
    ///
    /// Asserted over the region entries only, and over READER text, so the
    /// front-matter source comments (which quote the symptom wording verbatim, as
    /// they must) are not what is being read — WI-509's rule.
    /// </summary>
    [Fact]
    public void NoRegionEntryTellsTheReaderWhatATumorThereDoesToThem()
    {
        // Symptom and outcome vocabulary. Every one of these is corpus-legitimate
        // elsewhere — that is the point: they belong to the pages that own them.
        string[] banned =
        [
            "seizure", "headache", "vomit", "throwing up", "weakness", "numbness",
            "dizzi", "double vision", "hearing loss", "memory", "personality",
            "confus", "drowsi", "unsteady", "balance problem",
            "survival", "prognosis", "outlook", "risky", "dangerous", "safer",
            "more serious", "less serious", "worse", "curable", "incurable",

            // AND THE PLAN, which is the shape the decay actually took. /review
            // round 2: the brain stem entry had acquired "the American Cancer
            // Society gives this as an example of somewhere an operation to remove a
            // tumor may not be a good option" — a location-to-plan claim, in the one
            // entry whose location has the worst reputation, read by the parents
            // routed here from /tumors/dipg. An entry may ROUTE to the section that
            // weighs an operation; it may not weigh one itself.
            "may not be a good option", "cannot be taken out", "can be removed",
            "resect", "operate", "operation on", "biopsy", "watch and wait",

            // ROUND 3 found two more shapes the list above could not see, both of
            // them unsourced ON THIS PAGE even where the corpus supports them
            // elsewhere: an urgency claim ("a tumor near one gets attention
            // quickly") and a surgical-approach claim ("reached by a different
            // route"). An entry may ROUTE to the section or page that carries
            // either; it may not carry one itself.
            "gets attention quickly", "seen quickly", "reached by a different route",
            "through the nose",

            // CLOSED COUNTS. /review round 6: the pituitary entry said
            // "[pituitary tumor] and [craniopharyngioma] are the two growths here we
            // have written about", and /tumors/meningioma and
            // /tumors/cns-germ-cell-tumor both name that same spot in their own
            // reader text. This is the THIRD closed-count error in this item — the
            // escalation exception said "one place", then "two places" — so it is a
            // ban rather than a third correction. Naming N asserts there is no N+1,
            // and on a page whose job is getting a reader from a place to a name,
            // the reader it fails is the one whose type is the N+1.
            // "both of the" was proposed and REJECTED: "both of the lateral
            // ventricles" and "both of the frontal lobes" are correct sentences this
            // page could reasonably acquire, and §12.8 says a ban entry belongs on a
            // list only if no correct sentence contains it.
            "are the two growths", "are the only", "the two types",
            "the two we have written", "are the two we",
            // ROUND 11: "This is one of the TWO places on this page with a warning of
            // its own" slipped past every list above, in the entry that had just been
            // opened up. The number is the defect, not the noun.
            "one of the two", "one of only", "the two places",

            // ROUND 12, AND THE BREAK HARNESS IS WHAT PROVED IT WAS MISSING. The
            // ventricles entry had acquired "A tube is an easy thing to block" — a
            // likelihood claim, unsourced, and the one risk-flavoured property in an
            // entry whose whole ruling is that an entry teaches a word. Round 12
            // removed the sentence and added these entries in the same script; the
            // script aborted on an earlier assertion and wrote nothing, so the
            // sentence was gone and the ban was not. Two rounds of /review read the
            // list afterwards and could not see it. The harness put the sentence back
            // and the guard let it through, on LF and on CRLF.
            "easy to block", "easy thing to block", "likely to block", "blocks easily",
            "easily blocked",
        ];

        var entries = Regex
            .Matches(Page, @"(?ms)^### (.+?) \{#[a-z-]+\}$(.*?)(?=^#{2,3} |\z)")
            .ToList();

        // WI-517's lesson, and this guard was green on zero entries when it was
        // first written: an iterate-and-check assertion proves nothing until it has
        // said out loud how much it looked at.
        Assert.Equal(9, entries.Count);

        var offenders = new List<string>();
        foreach (var match in entries)
        {
            var heading = match.Groups[1].Value;
            // ONLY THE URL IS STRIPPED, NOT THE LABEL. Round 3 stripped whole links,
            // because it had banned "urgent" and the ventricles entry failed on the
            // legitimate label "[When where it sits makes it urgent: the fluid]" — and
            // round 4 pointed out what that bought: a claim written INTO a label
            // ("[a tumor here is reached through the nose](/x)") became invisible to
            // the very words round 3 had just added. So the label stays in scope and
            // "urgent" comes off the list instead, which is §12.8's actual rule: a
            // ban entry belongs on a list only if no correct sentence contains it.
            var body = CuratedPage.Flatten(
                    Regex.Replace(ReaderFragment(match.Groups[2].Value),
                        @"\]\([^)]*\)", "] "))
                .ToLowerInvariant();

            Assert.True(body.Length > 80, $"{heading}: nothing under it to check");

            offenders.AddRange(banned
                .Where(b => body.Contains(b, StringComparison.Ordinal))
                .Select(b => $"{heading}: \"{b}\""));
        }

        Assert.True(offenders.Count == 0,
            "a region entry has started saying what a tumor there does to the reader. "
            + "That is [MECHANISM]'s job on eighteen hubs, and it is also how this page "
            + "grows the location-to-risk table Wave 6 forbids:\n  "
            + string.Join("\n  ", offenders));
    }

    /// <summary>
    /// AND NO PLACE IS RANKED AGAINST ANOTHER, ANYWHERE ON THE PAGE. This is the
    /// same rule as the test above and it is separate because of how the first
    /// version failed: the region-scoped guard was green while the surgery section
    /// carried "the brain stem, which is the location with the worst reputation of
    /// all" — a ranking of every location against every other, on the page whose
    /// own promise is that it publishes no such thing, read by the parents routed
    /// here from /tumors/dipg and /tumors/diffuse-midline-glioma.
    ///
    /// A guard scoped to where the defect was expected is green where it arrives.
    /// </summary>
    /// <remarks>
    /// THIS IS A BAN LIST AND IT IS NOT THE ONLY GUARD ON THIS PROPERTY ANY MORE.
    /// Since WI-570, <c>LocationObligationSweepTests.TheTwoPagesTheRulingWasMeasured
    /// AgainstAreSweptToo</c> runs §12.18's PROPERTY over this same page — the
    /// address-plus-difficulty co-occurrence, with positive controls — and pins this
    /// page's one known false positive with its reason. Keep both: the property
    /// generalises and this list pins words no property reaches ("reputation"),
    /// which is the belt-and-property arrangement §12.18 settled on.
    ///
    /// <b>But widen one and you must re-measure the other.</b> §12.18's whole
    /// argument for promoting the scanner was that when two guards test one
    /// property in two files, the newer is not automatically the stronger — and the
    /// lexicon WI-569 needed already existed here, unnoticed, for two items.
    /// </remarks>
    [Fact]
    public void NoPlaceIsRankedAgainstAnotherAnywhereOnThePage()
    {
        var flat = Flat;

        // Comparatives that can only be about one location against another. Each is
        // corpus-legitimate elsewhere; none has a use on this page.
        string[] rankings =
        [
            "worst", "the most dangerous", "most serious", "more serious than",
            "less serious", "safer than", "the safest", "riskiest", "more risky",
            "the worst place", "worse place", "best place", "bad place",
            "reputation",
        ];

        var found = rankings
            .Where(r => flat.Contains(r, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(found.Count == 0,
            "this page ranks one location against another, which is the one artefact "
            + "every Wave 6 item is forbidden to publish (only a minority of surveyed "
            + "neurosurgeons apply any eloquence scale, so the field cannot standardise "
            + "which places are risky): " + string.Join(", ", found));

        // The positive half: the page says out loud that it does no ranking, and
        // says it where a reader who just read about the brain stem is standing.
        Assert.Contains("this page ranks no place against another",
            FlatSection(SurgeryHeading), StringComparison.Ordinal);
    }

    /// <summary>
    /// THE FLUID IS NOT THE ONLY URGENT THING, AND THE PAGE MUST NOT SAY IT IS.
    ///
    /// /review round 2 found this claim standing in three places after round 1 had
    /// rescoped it in the block: the front-matter description (which renders as the
    /// first paragraph a reader meets, and which no guard on this page could see),
    /// the short version, and the fluid section's own opening. The composed page
    /// carries <c>[ESCALATION]</c>, whose FIRST tier is an ambulance and whose first
    /// line is "a first ever seizure" — and the mechanism block says a seizure is
    /// how this starts for some people. A reader whose first sign was a seizure,
    /// told in sentence one that the only urgent thing here is fluid, stops reading.
    ///
    /// What the page may say is that the fluid is the one thing WHERE IT SITS makes
    /// urgent on its own. That is the true version and it is the useful one.
    /// </summary>
    [Fact]
    public void ThePageNeverSaysTheFluidIsTheOnlyUrgentThing()
    {
        // THE PROPERTY IS EXCLUSIVITY, and it took three rounds to name it.
        //
        // Round 1 banned the wording it deleted. Round 2 banned the next wording it
        // deleted. Round 3 banned SUPERLATIVES and scanned sentences containing
        // "urgent" — and round 4 found that scan counting the section HEADING and two
        // LINK LABELS toward its own floor (so deleting the only real claim left it
        // green), while matching neither of the page's two actual superlatives.
        //
        // A comparative is fine and often true: "the clearest case where the place
        // itself makes something urgent is the fluid" is correct, because it is a
        // comparison rather than a claim that nothing else is urgent. What is never
        // fine is EXCLUSIVITY, because the composed page carries [ESCALATION] (first
        // tier: an ambulance, first line: a first ever seizure) and because
        // /tumors/pituitary-tumor, /tumors/craniopharyngioma and blocks/spinal-cord.md
        // each say in writing that that list is wrong for their reader.
        var prose = Regex.Replace(Flat, @"\[[^\]]*\]\([^)]*\)", " ");
        prose = Regex.Replace(prose, @"#{2,3} [^#]*?(?= \{#)", " ");

        // THE TRIGGER IS WIDER THAN THE WORD "urgent". Round 5: keying the scan on
        // that one stem let "Apart from the fluid, nothing about where it sits is an
        // emergency" straight through, and it also meant the FRONT MATTER description
        // was one of only two windows — so the floor of 2 was satisfied by counting a
        // line nobody would call prose.
        var urgentWindows = Regex.Matches(prose,
                @"[^.!?]*\b(urgen\w*|emergenc\w*|911|ambulance)[^.!?]*")
            .Select(m => m.Value.Trim())
            .Where(s => s.Length > 20)
            .ToList();

        Assert.True(urgentWindows.Count >= 4,
            $"only {urgentWindows.Count} sentence(s) of PROSE mention urgency once "
            + "headings and link labels are stripped, so this guard is reading almost "
            + "nothing");

        string[] exclusivity =
        [
            "the one time", "the one thing", "the one part", "the one place",
            "the only", "one thing urgent", "nothing else", "the sole",
            "no other place", "only place", "only thing", "only reason",
            // ROUND 5 added these: the phrasings that carry exclusivity without
            // using the word "only".
            "nothing about", "one thing and one thing", "and nothing more",
            "apart from the fluid", "other than the fluid",
        ];

        var offenders = urgentWindows
            .SelectMany(s => exclusivity
                // Negations must not fire, and the honest note about this is that
                // TODAY IT HAS NOTHING TO DO. Rounds 4 and 5 both claimed it was
                // there for the short version's "It is not the only reason to call
                // anybody", and that sentence contains none of the trigger words, so
                // it is never collected and the lookbehind never runs on it. Round 6
                // stopped the comment claiming otherwise. It stays because the
                // negated form is the CORRECT way to write this page's own promise,
                // and the first edit that adds "urgent" or "emergency" to such a
                // sentence would otherwise turn a correct page red — §12.8's rule
                // that a ban entry belongs on a list only if no correct sentence
                // contains it. Same mechanism as
                // AssertNoWarningSignIsNormalised's own lookbehind.
                .Where(w => Regex.IsMatch(s,
                    $@"(?<!not )(?<!never )(?<!n't ){Regex.Escape(w)}",
                    RegexOptions.IgnoreCase))
                .Select(w => $"\"{w}\" in: {s}"))
            .ToList();

        Assert.True(offenders.Count == 0,
            "a superlative has attached itself to urgency on this page. The composed page "
            + "carries [ESCALATION], whose first tier is an ambulance, and /tumors/pituitary-tumor "
            + "says in writing that that list does NOT cover its two emergencies. Telling a "
            + "reader the fluid is the only urgent thing here is false for the sellar reader:\n  "
            + string.Join("\n  ", offenders));

        // Scoped to the HEADLINE as well as the body, explicitly, because that is
        // where it was hiding and the assertion above would also pass if Flat ever
        // went back to being body-only.
        Assert.DoesNotContain("the one time", Headline, StringComparison.OrdinalIgnoreCase);

        // The positive half: the short version points at the section, and says out
        // loud that the section names the places the general list does not cover —
        // PLACES, plural. Round 4: round 3's fix said "the one place ... and it is
        // the pituitary", which is false. blocks/spinal-cord.md exists because the
        // same list under-triages a cord reader, /tumors/craniopharyngioma carries a
        // third version of the sellar rule, and content-pipeline §12.10 names two
        // such cases in writing. Naming one exception asserts there are no others.
        var shortVersion = FlatSection(ShortHeading);
        Assert.Contains("#the-fluid", shortVersion, StringComparison.Ordinal);
        Assert.Contains("It is not the only reason to call anybody", shortVersion,
            StringComparison.Ordinal);
        // Round 11 opened this too: "it names THE PLACES whose warnings the general list
        // does not cover" was the same closed claim from the top of the page, and the
        // set is not closed — /tumors/hemangioblastoma and /tumors/cns-germ-cell-tumor
        // each escalate the shared list for a region that IS on this page.
        Assert.Contains("what to do when a place or a type has a", shortVersion,
            StringComparison.Ordinal);
        Assert.DoesNotContain("the places whose warnings the general list", shortVersion,
            StringComparison.Ordinal);

        // BOTH EXCEPTIONS ARE NAMED WHERE THE LIST IS PRINTED, not two screens away,
        // and each routes to the page that carries the right rule.
        var fluid = FlatSection(FluidHeading);
        Assert.Contains("some places have rules of their own", fluid, StringComparison.Ordinal);
        Assert.Contains("pituitary or sellar", fluid, StringComparison.Ordinal);
        Assert.Contains("/tumors/pituitary-tumor", fluid, StringComparison.Ordinal);
        Assert.Contains("/tumors/craniopharyngioma", fluid, StringComparison.Ordinal);
        Assert.Contains("/tumors/spinal-cord-tumor", fluid, StringComparison.Ordinal);

        // THE COUNT IS OPEN. Round 4 replaced "one place" with "two places", which is
        // the same closed claim one increment along — and /review round 5 named the
        // case it omits: /tumors/craniopharyngioma escalates the pressure signs the
        // shared list files as same-day, keyed to POSITION ("because it sits right
        // where the fluid drains"), which is this section's own reader.
        Assert.Contains("Some places carry rules of their own", fluid,
            StringComparison.Ordinal);
        foreach (var closedCount in new[]
        {
            "one place has warnings", "two places carry rules it does not cover",
            "the one place", "only two places", "these are the only",
        })
        {
            Assert.DoesNotContain(closedCount, fluid, StringComparison.OrdinalIgnoreCase);
        }

        // THE PITUITARY TIER IS NAMED, because the block files "a headache much worse
        // than usual" as a SAME-DAY call and /tumors/pituitary-tumor files apoplexy as
        // "Call 911".
        Assert.Contains("911 call rather than a wait for morning", fluid,
            StringComparison.Ordinal);

        // AND THE CORD TIER IS NOT RE-TIERED HERE, which is round 5's blocker.
        // /tumors/spinal-cord-tumor's own Gate 1 ruling REFUSES the levelling round 4
        // wrote: the right-away tier belongs to metastatic cord compression and the
        // cauda-equina carve-out, and a PRIMARY cord tumor keeps the same-day tier
        // ("new weakness in your legs, new numbness or new trouble walking are a
        // same-day call rather than a trip to the emergency room"). That page also
        // says the general list is partly theirs, not none of it. So this page says
        // the signs are DIFFERENT and routes, and names no tier for the cord at all.
        foreach (var retiering in new[]
        {
            "a same-day call above are a right-away call for you",
            "the list above is not yours",
            "right-away call for you", "are a right-away call",
        })
        {
            Assert.DoesNotContain(retiering, fluid, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Contains("the signs that matter most for you are", fluid,
            StringComparison.Ordinal);
        // Round 7: the three tiers were named here in /tumors/spinal-cord-tumor's own
        // three-sentence order and almost its words, which the 8-gram probe misses by
        // one substitution. The claim that matters is that the tiers DIFFER and that
        // page is the one to read; enumerating them was the echo.
        Assert.Contains("it does not treat them all as one kind of urgent",
            CuratedPage.Flatten(fluid), StringComparison.Ordinal);
        Assert.DoesNotContain("a few mean the emergency room", CuratedPage.Flatten(fluid),
            StringComparison.Ordinal);

        // §12.6: and the section lands on an action rather than on somebody else's
        // escalation. The two shipped pages this shape was copied from both do.
        // Round 6: "act on the stronger one" had no referent for a reader whose place
        // is neither of the two named, and it asked them to rank tiers. What the
        // section lands on now is what the corpus asks for everywhere else — tell your
        // team what changed — plus the plain statement that the general list is theirs
        // if neither exception is.
        // THE EXCEPTION SET IS OPEN, AND THE ESCAPE HATCH IS NOT SCOPED TO ONE READER.
        //
        // Round 10 wrote the right sentence into the wrong paragraph: "if your own tumor
        // type's page gives you a stronger rule than EITHER OF THEM" sat inside the cord
        // paragraph, where "either of them" had two local antecedents — so a non-cord
        // reader had just been told the paragraph was not theirs. The closing sentence
        // then asserted completeness ("the list above is yours AS IT STANDS"), which is
        // false for at least two hubs that now link here and escalate the shared list
        // for a region that IS on this page: /tumors/hemangioblastoma for the cerebellum
        // ("that beats the same-day rule for a bad headache in the list above") and
        // /tumors/cns-germ-cell-tumor for the deep middle ("adds four rules of its own").
        // That was the FIFTH closed set in this item, so it is a position assertion and
        // a ban rather than a sixth correction.
        foreach (var closed in new[]
        {
            "is yours as it stands", "that is the whole of what is being asked",
            "those are the only", "no other place has", "if neither of those is your place",
        })
        {
            Assert.DoesNotContain(closed, fluid, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("And those two are not the whole of it", fluid,
            StringComparison.Ordinal);
        Assert.Contains("the stronger one wins", fluid, StringComparison.Ordinal);
        Assert.Contains("tell your team what has changed", fluid, StringComparison.Ordinal);

        // POSITION: the stronger-rule-wins paragraph sits AFTER both named exceptions,
        // so it reads as general rather than as part of either one.
        var fluidSection = Section(FluidHeading);
        var sellarAt = fluidSection.IndexOf("If your report says pituitary or sellar",
            StringComparison.Ordinal);
        var cordAt = fluidSection.IndexOf("If it is in or pressing on your spinal cord",
            StringComparison.Ordinal);
        var generalAt = fluidSection.IndexOf("And those two are not the whole of it",
            StringComparison.Ordinal);
        Assert.True(sellarAt > 0 && cordAt > sellarAt && generalAt > cordAt,
            "the stronger-rule-wins paragraph must follow both named exceptions, or it "
            + "reads as belonging to whichever one it sits inside");
        Assert.EndsWith("on the day you need it.", fluid.TrimEnd(), StringComparison.Ordinal);

        // THE EXCEPTIONS SIT BENEATH THE BLOCK, with a forward pointer above it.
        // That is the corpus shape and content-pipeline §12.10 states it: "add the
        // page's own line BENEATH the block if its emergency is not in the list".
        // /tumors/pituitary-tumor and /tumors/craniopharyngioma both do it that way.
        // Round 3 put it above and pinned the inversion with no recorded reason.
        var pointerAt = fluidSection.IndexOf("two of those are just below",
            StringComparison.Ordinal);
        var blockAt = fluidSection.IndexOf("[ESCALATION]", StringComparison.Ordinal);
        var ruleAt = fluidSection.IndexOf("some places have rules of their own",
            StringComparison.Ordinal);

        Assert.True(pointerAt > 0 && blockAt > pointerAt && ruleAt > blockAt,
            "the order must be forward pointer, then the block, then the page's own "
            + "rules, which is the shape §12.10 documents and three shipped pages use");
    }

    /// <summary>
    /// The lead-in to <c>[ESCALATION]</c> must not flatten its tiers. The block's
    /// first tier is "Call <b>911</b>, or your local emergency number"; its second is
    /// a same-day call to your team. /review round 2: the first version of this
    /// page's lead-in said "treat it as the phone call it is", which demotes the
    /// ambulance tier to a phone call — the identical defect
    /// <c>PituitaryTumorPageTests</c> records as a caught blocker and pins against.
    /// </summary>
    [Fact]
    public void TheLeadInToTheEscalationBlockDoesNotFlattenItsTiers()
    {
        var fluid = FlatSection(FluidHeading);

        foreach (var flattening in new[]
        {
            "treat it as the phone call it is",
            "the phone call it is",
            "call your team about any of",
            "any of the signs below means a phone call",
            // ROUND 4: the guard was blind to the paragraph round 3 added beneath the
            // block, which described a 911 rule as one that "cannot wait until the
            // next day" — the block's own same-day language.
            "cannot wait until the next day",
            "cannot wait for the next day",
            "should not wait until tomorrow",
        })
        {
            Assert.DoesNotContain(flattening, fluid, StringComparison.OrdinalIgnoreCase);
        }

        // And it says there are two different answers in there, which is the thing
        // the demoted version took away.
        Assert.Contains("which signs mean an ambulance", fluid, StringComparison.Ordinal);
        Assert.Contains("does not treat those as the same thing", fluid,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The term this page and <c>[MECHANISM]</c> and <c>/tumors/meningioma</c> must
    /// all agree about. WI-568's round 1 found the block teaching "skull base" as
    /// one spot while a page it composes onto taught it as the whole floor — two
    /// definitions of a word the reader carries to an appointment. MSKCC settled it
    /// in the whole-floor direction. A third file now says it, so the agreement is
    /// asserted here rather than left to hold by luck.
    /// </summary>
    [Fact]
    public void TheSkullBaseIsTaughtAsTheWholeFloorLikeEverywhereElseInTheCorpus()
    {
        var entry = CuratedPage.Flatten(ReaderFragment(
            CuratedPage.ComposedSubsection(Page, "The floor of the skull")));

        Assert.Contains("the whole floor of the skull", entry, StringComparison.Ordinal);
        Assert.Contains("umbrella word", entry, StringComparison.Ordinal);

        // And the two files it has to agree with still say it.
        var block = CuratedPage.Flatten(CuratedPage.ReaderText(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md"))));
        Assert.Contains("Your team may call the whole floor of the skull the **skull base**",
            block, StringComparison.Ordinal);

        // AND THE THIRD FILE NOW ROUTES INSTEAD OF DEFINING, WHICH IS STRONGER THAN
        // AGREEING (WI-569). /tumors/meningioma used to teach the term itself — "an
        // umbrella word for the ones growing on the floor of the skull and the ridge
        // behind the eyes" — thirteen lines above the block that also teaches it.
        // Those two agreed, but they were two definitions of one word on one composed
        // page, and agreement between two copies has to be maintained forever. The
        // page keeps the half only it can say (which of ITS OWN addresses sit under
        // the umbrella) and sends the reader here for the meaning, so there is now
        // one definition rather than two that happen to match.
        var meningioma = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "meningioma.md")));
        Assert.Contains("umbrella word you are likely to meet", meningioma, StringComparison.Ordinal);
        Assert.Contains("(/where-your-tumor-is#skull-base)", meningioma, StringComparison.Ordinal);
        Assert.DoesNotContain("for the ones growing on the floor of the skull",
            meningioma, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------
    // The fluid, and the claim that was refused
    // ------------------------------------------------------------------

    /// <summary>
    /// THE GATE 1 REFUSAL, BANNED BY NAME. The backlog asked this section to say a
    /// tumor can be "small, slow and low-grade and still be an emergency because of
    /// the plumbing rather than the tumor". Nothing verified supports it. The one
    /// source read for this item that addresses size prints the opposite qualifier:
    /// CNS tumors block CSF flow "when large enough". Publishing the backlog's
    /// version would have invented a fact in the only section of the page that is
    /// about an emergency.
    ///
    /// Canary, not just a ban: the sourced sentences that replaced it must be here,
    /// or this test passes over a section that says nothing at all.
    /// </summary>
    [Fact]
    public void TheFluidSectionDoesNotClaimSizeOrGradeIsIrrelevant()
    {
        var fluid = FlatSection(FluidHeading).ToLowerInvariant();

        // Word-bounded, because "small" as a substring fails "smaller channels" — a
        // correct sentence this section would naturally acquire (§12.8: a ban list
        // entry belongs here only if no correct sentence contains it).
        string[] unsupported =
        [
            "small", "tiny", "slow-growing", "slow growing", "low-grade", "low grade",
            "however big", "no matter how big", "size does not", "size doesn't",
            "does not have to be large", "whatever its grade",
        ];

        var found = unsupported
            .Where(u => Regex.IsMatch(fluid, $@"\b{Regex.Escape(u)}\b", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(found.Count == 0,
            "the fluid section has acquired the claim WI-567 refused at Gate 1 — that a "
            + "small, slow or low-grade tumor is enough to block the fluid. The best source "
            + "read for this item says tumors block CSF flow \"when large enough\", and no "
            + "verified source says otherwise: " + string.Join(", ", found));

        // AND THE REFUSAL AGAIN, IN THE WORDS IT CAME BACK IN. /review round 1
        // found the first draft carrying "being watched is not the opposite of being
        // urgent: some people have a tumor nobody is in a hurry to remove and fluid
        // that needs sorting out this week" — the refused claim with the size words
        // removed, asserting a co-occurrence no cited source carries. The first
        // version of THIS TEST then required that sentence as its canary, so the
        // guard against the refusal depended on the refusal being present. Both are
        // gone. A refusal that only bans the words it was written against is not a
        // refusal.
        // Read PAGE-WIDE, for the reason above. And "instead of it for now" is here
        // because /review round 2 found it: round 1 deleted the paraphrase paragraph
        // and left one clause of it standing inside the sourced sentence ("a drain
        // or a shunt can go in before tumor surgery, OR INSTEAD OF IT FOR NOW"),
        // which is the same unsourced co-occurrence in five words.
        foreach (var paraphrase in new[]
        {
            "not the opposite of being urgent",
            "nobody is in a hurry to remove",
            "needs sorting out this week",
            "instead of it for now",
            "instead of the tumor",
        })
        {
            Assert.DoesNotContain(paraphrase, Flat, StringComparison.OrdinalIgnoreCase);
        }

        // The canary. These are what IS sourced, and they carry the useful part.
        var sourced = FlatSection(FluidHeading);
        Assert.Contains("life-threatening", sourced, StringComparison.Ordinal);
        Assert.Contains("without taking the tumor out", sourced, StringComparison.Ordinal);
        Assert.Contains("the first operation may be about the fluid rather than the tumor",
            sourced, StringComparison.OrdinalIgnoreCase);

        // §12.6: the answer is in the first sentence under the heading, which is the
        // property the old FlatSection could not see because it sliced the first
        // three characters off every section it read.
        Assert.StartsWith("**A tumor sitting where the fluid has to pass can block it",
            sourced, StringComparison.Ordinal);
    }

    /// <summary>
    /// The fluid section sits above everything the reader would have to read past
    /// to reach it: the surgery range, the no-list argument and the questions.
    ///
    /// It does NOT sit above the region list, and the first version of this comment
    /// said it did — /review round 1 caught the claim, not the code. §12.8's slot
    /// order is fixed and 6a follows slot 3, so the region list legitimately comes
    /// first; what the page owes instead is a route to the section from the short
    /// version, which the last assertion here pins. POSITION is the property
    /// (WI-512), and a docstring that overstates the position is how the next
    /// reader stops checking it.
    /// </summary>
    [Fact]
    public void TheUrgentSectionSitsAboveEverythingItCouldHaveBeenBuriedUnder()
    {
        var fluid = Page.IndexOf($"## {FluidHeading}", StringComparison.Ordinal);
        var surgery = Page.IndexOf($"## {SurgeryHeading}", StringComparison.Ordinal);
        var noList = Page.IndexOf($"## {NoListHeading}", StringComparison.Ordinal);
        var questions = Page.IndexOf($"## {QuestionsHeading}", StringComparison.Ordinal);

        Assert.True(fluid > 0 && fluid < surgery && surgery < noList && noList < questions,
            "the fluid section has moved below the surgery material");

        // And the short version flags it, so a reader who stops after four
        // paragraphs still knows the urgent part exists.
        Assert.Contains("#the-fluid", Section(ShortHeading), StringComparison.Ordinal);
    }

    /// <summary>
    /// <c>[ESCALATION]</c> carries the tiers; this page does not write a fourth
    /// list of red flags. <c>AssertEscalationTiers</c> is the shared guard, and it
    /// reads the COMPOSED page, which is the only place the tiers exist.
    /// </summary>
    [Fact]
    public void TheEscalationTiersComeFromTheBlockRatherThanFromThisPage()
    {
        Assert.Contains("[ESCALATION]", Page, StringComparison.Ordinal);

        // The raw page, like every other caller: the helper composes. Passing an
        // already-composed page works only because Compose is a no-op once the
        // directives are gone, which is the kind of accident that stops being one.
        CuratedPage.AssertEscalationTiers(Page, Slug, FluidHeading);

        // AssertNoWarningSignIsNormalised WAS CALLED HERE AND REMOVED, and the reason
        // belongs in the file rather than in a review thread (§12.8, the
        // RejectedCharacterisations pattern). Its contract is that the caller names a
        // reassuring paragraph the guard must have ACTUALLY EXAMINED, because WI-517's
        // lesson is that an iterate-and-check guard is green on a page it never looked
        // at. On the composed version of this page it collects NO paragraphs: nothing
        // here names a warning sign and then normalises it, because every warning sign
        // on this page arrives inside [ESCALATION], already tiered. So there is no
        // paragraph to name, and calling it would either fail on an empty collection
        // or need its positive half dropped — which turns it into exactly the guard
        // WI-517 warns about. If a later edit reassures about a symptom in this page's
        // own prose, add the call back WITH the paragraph it examines.
    }

    // ------------------------------------------------------------------
    // The surgery range, and the sentence that makes a table impossible
    // ------------------------------------------------------------------

    [Fact]
    public void TheSurgeryRangeCarriesAllFiveStatesAndCallsNoneOfThemAFailure()
    {
        var surgery = FlatSection(SurgeryHeading);

        Assert.Contains("All of it comes out", surgery, StringComparison.Ordinal);
        Assert.Contains("As much as is safe comes out", surgery, StringComparison.Ordinal);
        Assert.Contains("A piece comes out, to find out what it is", surgery, StringComparison.Ordinal);
        Assert.Contains("No operation on the tumor", surgery, StringComparison.Ordinal);
        Assert.Contains("watching instead", surgery, StringComparison.Ordinal);

        // §12.6: never end a section on a frightening sentence, and never let "we
        // are not operating" read as abandonment. POSITION is the property. Round 3:
        // the reassurance was present but sat ABOVE the Columbia paragraph, so the
        // section landed on "surgery is not usually recommended at all" for the
        // brain-stem and DIPG readers who are routed here. Presence was green over
        // that the whole time.
        Assert.Contains("none of the five is the surgeon giving up", surgery,
            StringComparison.OrdinalIgnoreCase);

        var reassurance = surgery.IndexOf("none of the five is the surgeon giving up",
            StringComparison.OrdinalIgnoreCase);
        var bleakest = surgery.IndexOf("surgery is not usually recommended at all",
            StringComparison.Ordinal);
        Assert.True(bleakest > 0 && reassurance > bleakest,
            "the surgery section's reassurance sits above its bleakest sentence, so the "
            + "section lands on the bleakest one");

        // And the routes, because this page owns WHETHER there is an operation and
        // /treatments/craniotomy owns HOW MUCH comes out.
        Assert.Contains("/treatments/craniotomy#how-much-came-out", surgery, StringComparison.Ordinal);
        Assert.Contains("/tests/biopsy", surgery, StringComparison.Ordinal);
        Assert.Contains("/treatments/watch-and-wait", surgery, StringComparison.Ordinal);
    }

    /// <summary>
    /// THE LOAD-BEARING SENTENCE OF THE WHOLE PAGE. Columbia's neurosurgeons
    /// describe two tumors in the brain stem getting opposite surgical answers,
    /// patient-facing, in two sentences. It is why nine regions can be listed
    /// without nine risks beside them, so it is asserted as a PROPERTY (the two
    /// answers, and that the place is the same) rather than as a phrase.
    /// </summary>
    [Fact]
    public void TheSamePlaceIsShownGivingTwoOppositeAnswers()
    {
        var surgery = FlatSection(SurgeryHeading);

        Assert.Contains("the same place can give two different answers",
            surgery, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Same place, opposite plans", surgery, StringComparison.Ordinal);

        var sentence = surgery[surgery.IndexOf("the same place can give two different answers",
            StringComparison.OrdinalIgnoreCase)..];

        // Both halves, or the point is lost: one kind can be worth removing, the
        // other cannot safely be removed at all.
        Assert.Contains("removing it can be worth doing", sentence, StringComparison.Ordinal);
        Assert.Contains("cannot safely be removed", sentence, StringComparison.Ordinal);

        // And it stays a statement about the TUMOR, not about the place, which is
        // the whole reason it defeats a location-keyed table.
        Assert.Contains("because the tumor is not the same tumor",
            surgery, StringComparison.Ordinal);

        // /tumors/dipg owns the DIPG specifics, including that centers differ on
        // whether to biopsy. This page must not have taken them over.
        Assert.DoesNotContain("DIPG", Reader, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("centers differ", Reader, StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------------------
    // No table, no percentages, no prognosis
    // ------------------------------------------------------------------

    /// <summary>
    /// The Wave 6 preamble's first constraint, asserted structurally rather than by
    /// vocabulary: there is no table on this page at all. A location-to-risk lookup
    /// is the one artefact every item in this wave is forbidden to publish, and a
    /// Markdown table is how it would arrive.
    /// </summary>
    [Fact]
    public void ThereIsNoTableOnThePage()
    {
        var rows = Regex.Matches(Reader, @"(?m)^\s*\|.*\|\s*$").Count;
        Assert.True(rows == 0,
            $"{rows} table row(s) on a page whose whole ruling is that a location-keyed "
            + "lookup table must not exist");
    }

    /// <summary>
    /// No percentages and no prognosis, which is the item's own acceptance and
    /// §12.2 item 5. Read over READER text: the front matter quotes the survey's
    /// own figures verbatim, as a citation must, and those are not on the page.
    ///
    /// WHAT THIS DOES NOT DO, said plainly because the first version of this
    /// comment claimed it did: it is not a ban on numbers. The page spells out two
    /// counts from the eloquence survey ("twenty-five countries", "twelve cases"),
    /// each attributed in the sentence that prints it, which is §12.8's rule for a
    /// figure from one study rather than a violation of it. A digit ban is defeated
    /// by orthography and it was never the point. The point is that no number here
    /// can be read as a risk, a proportion or an outcome, which is what the
    /// vocabulary list below is for.
    /// </summary>
    [Fact]
    public void NoNumberOnThisPageCouldBeMistakenForARiskOrAnOutcome()
    {
        // READ OVER Flat, which includes the title and description. Round 2 widened
        // the vocabulary half of this test and left these two reading the body only,
        // which is the exact hole round 2 existed to close.
        Assert.DoesNotContain("%", Flat, StringComparison.Ordinal);

        // "911" IS NOT A FIGURE, and it is the one number this page owes a reader.
        // /start and blocks/escalation.md both print it. Round 4 added it here when the
        // pituitary rule was finally given its correct tier, and a digit ban that
        // forbids an emergency number is a guard that makes a page worse (§12.8).
        var withoutTheEmergencyNumber = Flat.Replace("911", " ", StringComparison.Ordinal);
        var digits = Regex.Matches(withoutTheEmergencyNumber, @"\d")
            .Select(m => m.Value).ToList();
        Assert.True(digits.Count == 0,
            "a digit has reached the reader text of a page that publishes no figures at "
            + "all: " + string.Join(", ", digits));

        string[] banned =
        [
            "survival", "five-year", "5-year", "median", "life expectancy",
            "prognosis", "cure rate", "outlook", "chance of",
            // Spelled-out proportions, because the digit check above cannot see
            // them and a proportion is the shape a risk arrives in here.
            //
            // "one in " WAS PROPOSED AND REJECTED, and the reason is recorded rather
            // than the phrase quietly dropped (§12.8, WI-510's RejectedCharacterisations
            // pattern): the ventricles entry says "the two lateral ventricles, one in
            // each half of the brain", which is correct and is the natural way to write
            // it. A ban list entry belongs here only if no correct sentence contains
            // it, and this one fails that on the page it was written for.
            "per cent", "percent", "two-thirds", "a third of", "half of all",
            "out of ten", "in every ",
        ];
        var found = banned.Where(b => Flat.Contains(b, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.True(found.Count == 0, string.Join(", ", found));
    }

    /// <summary>
    /// The "why there is no list" section is the item's promise to the reader, and
    /// its two halves both have to be there: surgeons do not agree, AND the same
    /// scans produced different plans. §12.6 then requires it not to end on the
    /// frightening half — and the thing it ends on is ROUTED, because
    /// /tumors/all-brain-tumors already owns the second-opinion answer.
    /// </summary>
    [Fact]
    public void TheNoListSectionGivesTheReasonAndThenTheThingToDoAboutIt()
    {
        var noList = FlatSection(NoListHeading);

        Assert.Contains("Surgeons do not agree on it", noList, StringComparison.Ordinal);
        Assert.Contains("only a minority use any of the scales that already exist",
            noList, StringComparison.Ordinal);

        // The four levels the survey found variation at, stated as four things
        // rather than as a number.
        Assert.Contains("Four separate things", noList, StringComparison.Ordinal);

        // AND NOT ONE STEP FURTHER THAN THE SOURCE. /review round 2: the first
        // version said the survey "showed real patients' scans" and asked surgeons
        // "to rate how risky different parts of the brain are to operate on", and
        // pinned "the scan was identical every time". The recorded quote says twelve
        // "cases" were "presented" to assess opinions on "eloquence". A case is not
        // a scan and eloquence is not operative risk, and a pinned embellishment is
        // worse than an unpinned one because it is now load-bearing.
        foreach (var overreach in new[]
        {
            // ROUND 3: round 2 deleted "showed real patients' scans" and left
            // "twelve REAL cases", which is the same claim in one word, and it
            // reversed the source's own sentence — the survey says the lack of
            // consensus limits the reliability of ELOQUENCE as a descriptor of tumor
            // location, not that a location is an unreliable description of a tumor.
            "showed real patients' scans", "the scan was identical",
            "how risky different parts of the brain are to operate on",
            "twelve real cases", "real cases",
            "the place of a tumor can be relied on as a description",
            // ROUND 5: round 4 fixed the definition-duplication by using the word
            // "eloquent area" so the glossary tooltip supplies the meaning, and then
            // ALSO wrote /treatments/craniotomy's own sentence back in ("and it is a
            // word you may see in your notes" against "and you may hear the word or
            // see it in your notes"). The 8-gram probe cannot see it, and this file's
            // own docstring lists that exact restatement as a pinned failure mode.
            "a word you may see in your notes", "you may hear the word",
            // ROUND 8: round 5 banned the TAIL of /treatments/craniotomy's sentence and
            // left the head. "Surgeons call a place like that an eloquent area" against
            // "Surgeons call a part of the brain like that an eloquent area" is one
            // substitution apart, and the glossary alias already supplies the meaning,
            // so the naming sentence bought nothing either way.
            "call a place like that", "like that an eloquent area",
        })
        {
            Assert.DoesNotContain(overreach, noList, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Contains("on the same twelve cases", noList, StringComparison.Ordinal);

        // The route out, not a restatement of it.
        Assert.Contains("/tumors/all-brain-tumors#asking-somebody-else-to-look",
            noList, StringComparison.Ordinal);

        // §12.6: the section does not END on the frightening half. The property is
        // what it lands on, not which four words it avoids, so assert the landing
        // directly: the last thing said is the route to the thing to DO about it.
        var sentences = CuratedPage.SentencesOf(noList);
        Assert.Contains("/tumors/all-brain-tumors#asking-somebody-else-to-look",
            sentences[^1], StringComparison.Ordinal);

        // And the disagreement is stated BEFORE it, not after, so the order the
        // reader meets them in is reason then remedy.
        var disagreement = noList.IndexOf("Surgeons do not agree on it",
            StringComparison.Ordinal);
        var remedy = noList.IndexOf("better than a table anyway", StringComparison.Ordinal);
        Assert.True(disagreement >= 0 && disagreement < remedy);
    }

    // ------------------------------------------------------------------
    // Shared corpus rules
    // ------------------------------------------------------------------

    /// <summary>
    /// THE TWO SECTIONS NOTHING WAS READING. /review round 5 found
    /// <c>WhyHeading</c> and <c>FindingHeading</c> declared and never referenced, which
    /// meant either section could be deleted whole with a green suite — including the
    /// one carrying the EANO four-factor claim the ruling calls "the one thing
    /// /treatments/craniotomy does not say", and slot 7, the reader's only path from a
    /// place to a name. §12.15: an obligation may be answered, re-headed or routed,
    /// never dropped, and nothing was enforcing that for these two.
    ///
    /// An unused <c>const</c> raises no compiler warning, so the gap was invisible.
    /// This asserts what each section is FOR, not that it exists.
    /// </summary>
    [Fact]
    public void TheTwoSectionsThatCarryTheRestOfTheRulingAreActuallyThere()
    {
        // Slot 2: what a surgeon weighs, which is EANO's list and the one thing
        // /treatments/craniotomy does not spell out.
        var why = FlatSection(WhyHeading);
        Assert.Contains("how firm it is", why, StringComparison.Ordinal);
        Assert.Contains("blood vessels and nerves that matter", why, StringComparison.Ordinal);
        Assert.Contains("the place is one of them", why, StringComparison.Ordinal);

        // And it does not let the place become the thing that settles the plan, which
        // is the Wave 6 inference in four words.
        // PAGE-WIDE, because /treatments/craniotomy already ships the flat form
        // ("Where the tumor sits decides how much that is") and this page's surgery
        // section is where it would land. The hedged form this page uses ("is one of the
        // things that decides which") is deliberately still allowed.
        Assert.DoesNotContain("the place decides", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("where it sits decides", Flat, StringComparison.OrdinalIgnoreCase);

        // Slot 7: the actionable path from a place to a name. Each step has to be a
        // thing the reader can do, and the last one has to land on the index.
        var finding = FlatSection(FindingHeading);
        Assert.Contains("Ask what it is being called for now", finding, StringComparison.Ordinal);
        Assert.Contains("Ask for a copy of the report on your scan", finding,
            StringComparison.Ordinal);
        Assert.Contains("/tests/pathology-report", finding, StringComparison.Ordinal);
        Assert.Contains("](/tumors)", finding, StringComparison.Ordinal);

        // And it does not promise a right it cannot source (WI-549's hhs.gov shape).
        Assert.DoesNotContain("entitled", finding, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThePageNeverCharacterisesOrMinimises()
    {
        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, Flat, StringComparison.OrdinalIgnoreCase);
        }

        CuratedPage.AssertNeverMinimises(Reader, Slug);
    }

    [Fact]
    public void ThePageUsesAmericanFormsThroughout()
    {
        var body = CuratedPage.Body(Page);
        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, " ", StringComparison.Ordinal);
        }

        var found = CuratedPage.BritishForms
            .Where(f => body.Contains(f, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(found.Count == 0, string.Join(", ", found));
    }

    /// <summary>
    /// §12.10's rule, mechanically. The comparison includes
    /// <c>Content/blocks/</c>, so <c>[MECHANISM]</c> is one of the files this page
    /// is measured against — which is the single most important comparison for this
    /// item, because repeating the block is the failure the ruling exists to
    /// prevent.
    /// </summary>
    [Fact]
    public void ThePageDoesNotRestateProseThatAlreadyLivesElsewhere() =>
        CuratedPage.AssertDoesNotRestateTheCorpus(Page, Slug);

    /// <summary>
    /// The one word this item adds to the glossary, and the three it REFUSED, with
    /// the refusal pinned so the next item does not re-reach the wrong answer.
    ///
    /// <c>frontal lobe</c> already appears on three pages with no entry, so an entry
    /// fires site-wide and earns its place (contract item 9). <c>temporal</c>,
    /// <c>parietal</c> and <c>occipital lobe</c> appear NOWHERE else in the corpus
    /// today, and WI-519's rule is that an entry defined and used in one place fires
    /// nowhere. The trigger for adding them as a set is WI-569 and WI-570, which put
    /// location sections on the glioma family and will be the second user — and this
    /// test is what will say so at the time.
    /// </summary>
    [Fact]
    public void OnlyTheLobeWordThatFiresElsewhereGotAGlossaryEntry()
    {
        var glossary = Path.Combine(
            CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content", "glossary");

        Assert.True(File.Exists(Path.Combine(glossary, "frontal-lobe.md")));

        foreach (var word in new[] { "temporal", "parietal", "occipital" })
        {
            Assert.False(File.Exists(Path.Combine(glossary, $"{word}-lobe.md")),
                $"a glossary entry for '{word} lobe' now exists. If a second page says the "
                + "word, that is correct and this assertion is what should change; if it is "
                + "still only said here, the tooltip fires nowhere (§12.8, WI-519).");

            // READER TEXT, not raw. WI-568's lesson in a new place: a bare grep for
            // MECHANISM returned 22 files and four were front-matter comments. A
            // source comment quoting the AANS sentence would otherwise make this
            // test demand an entry whose tooltip fires for nobody.
            var elsewhere = CuratedPage.AllPages()
                .Where(p => !p.Slug.EndsWith(Slug, StringComparison.Ordinal))
                .Count(p => CuratedPage.ReaderText(p.Text)
                    .Contains($"{word} lobe", StringComparison.OrdinalIgnoreCase));

            Assert.True(elsewhere == 0,
                $"'{word} lobe' is now in the reader text of {elsewhere} other file(s), so it "
                + "has a second home and has earned the glossary entry this test forbids");
        }

        // This page defines all four inline, so the one tooltip that exists is
        // suppressed here (§12.8, WI-509) and the other three have nothing to
        // suppress.
        Assert.Contains("!%frontal lobe%", Page, StringComparison.Ordinal);

        // AND THE COUNT THAT JUSTIFIED THE ENTRY, derived rather than typed. The
        // front matter first claimed "frontal lobe" was on THREE other pages; two of
        // them are reader-facing and the third is a front-matter comment on
        // /treatments/awake-craniotomy. That is WI-568's "22 files, four of them
        // comments" error in a new place, so the number is now asserted from reader
        // text. Two is still two homes, which is what earns a site-wide tooltip —
        // and ROUND 3's correction to that reasoning: one of the two, /tumors/
        // astrocytoma, glosses the term inline ("the frontal lobes, the front of the
        // brain"), so this item suppresses the tooltip there per §12.8/WI-509. The
        // entry therefore FIRES on exactly one page today. It is kept because the
        // suppression is a per-page decision that can be reversed by an edit, while
        // a missing entry is a gap every future page inherits — and because Wave 6's
        // remaining items will say the word.
        var readerFacing = CuratedPage.AllPages()
            .Where(p => !p.Slug.EndsWith(Slug, StringComparison.Ordinal))
            .Where(p => CuratedPage.ReaderText(p.Text)
                .Contains("frontal lobe", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Slug)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(["pages/tumors/astrocytoma", "pages/tumors/oligodendroglioma"], readerFacing);
    }

    /// <summary>
    /// The source we could not read is cited nowhere. moffitt.org returned 403 on
    /// every path tried with three browser user agents; it was read through the
    /// Wayback Machine only to confirm that no comparator answers this page's
    /// question, and a source nobody can open live is a source nobody can verify
    /// (§12.8, WI-509). Same shape as WI-549's hhs.gov absence.
    /// </summary>
    [Fact]
    public void TheSourceThatCouldNotBeReadLiveIsCitedNowhere()
    {
        Assert.DoesNotContain("- url: https://www.moffitt.org", Page, StringComparison.Ordinal);
        Assert.DoesNotContain("- url: https://moffitt.org", Page, StringComparison.Ordinal);
        Assert.DoesNotContain("web.archive.org", Page, StringComparison.Ordinal);

        // The finding it produced is recorded in the front matter, because an
        // absence nobody wrote down gets re-investigated by the next item.
        Assert.Contains("moffitt.org returned HTTP 403", CuratedPage.FrontMatter(Page),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Wave 6's later items are not pre-empted. Each of these is another item's
    /// subject, and the reason this is a test rather than a note is that a page
    /// about location is where all four would naturally creep in.
    /// </summary>
    [Fact]
    public void ThePageDoesNotTakeOverTheWorkOfTheItemsThatFollowIt()
    {
        // WI-571 owns tectal glioma and the CNS5 naming question it says is unverified.
        Assert.DoesNotContain("tectal", Flat, StringComparison.OrdinalIgnoreCase);

        // WI-573 owns visual field loss and the driving consequence, and driving is
        // routed to /seizures/living-with#driving wherever it appears.
        Assert.DoesNotContain("visual field", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("driving", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("drive", Flat, StringComparison.OrdinalIgnoreCase);

        // WI-572 owns the diagram, and the corpus has no images at all today.
        Assert.DoesNotContain("![", Page, StringComparison.Ordinal);
        Assert.DoesNotContain("<figure", Page, StringComparison.Ordinal);
    }

    /// <summary>
    /// The /tumors/meningioma location duplication WI-568 handed to WI-569 is still
    /// handed over. This item must not quietly resolve it while writing the page
    /// that made it visible — the handover is pinned in MechanismBlockTests, and
    /// this asserts from the other side that nothing here has pre-empted it.
    /// </summary>
    [Fact]
    public void TheMeningiomaHandoverToTheNextItemIsLeftAlone()
    {
        var meningioma = CuratedPage.Read("tumors", "meningioma.md");

        Assert.Contains("- **Near the pituitary and the crossing of the optic nerves.**",
            meningioma, StringComparison.Ordinal);
        Assert.Contains("- **Inside the fluid spaces of the brain.**",
            meningioma, StringComparison.Ordinal);

        // And this page does not link into that list, which would make the
        // duplication look deliberate before the item that owns it has ruled.
        Assert.DoesNotContain("/tumors/meningioma#", Page, StringComparison.Ordinal);
    }
}

[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class WhereYourTumorIsPageRenderTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public WhereYourTumorIsPageRenderTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("ConnectionStrings:BrainHarbor",
                TestDatabase.ConnectionString));
    }

    // The class constructs its own host, so it owns disposing it. /review round 3:
    // the two classes in the suite that build their own factory both leaked one.
    public void Dispose() => _factory.Dispose();

    private const string Url = "/where-your-tumor-is";

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);
        Assert.Contains("Where your tumor is", html, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The orphan guard, and this page's is unusual: the door comes from a BLOCK,
    /// so it lands on all eighteen hubs at once. WI-568 deferred that link because
    /// the page did not exist; the obligation was carried by a test in
    /// MechanismBlockTests that went red the day it shipped. Assert it reaches a
    /// reader on a real composed page, not just that the block file contains it —
    /// §12.10: a block's words only exist once they compose.
    /// </summary>
    [Fact]
    public async Task ThePageIsReachableFromTheHubsThatSendReadersHere()
    {
        var client = _factory.CreateClient();

        // All EIGHTEEN, not a sample. /review round 1: the first version checked
        // three, while the block's own [Theory] over all eighteen was already there
        // and WI-567 had added no needle to it. Both are now covered — the needle
        // in MechanismBlockTests proves the prose composes everywhere, and this
        // proves the rendered anchor does.
        foreach (var slug in MechanismBlockTests.IncludingHubs)
        {
            var html = await client.GetStringAsync($"/tumors/{slug}");
            Assert.Contains($"href=\"{Url}\"", html, StringComparison.Ordinal);
        }

        // And from where a reader with no type actually starts. Scoped to the
        // paragraph, because /start carries nine of these doors and a whole-page
        // Contains() would be satisfied by any of them moving anywhere.
        var start = await client.GetStringAsync("/start");
        Assert.Contains("told where it is, but not yet what it is", start,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"href=\"{Url}\"", start, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(_factory.CreateClient(), Url,
            "/tumors", "/treatments/craniotomy", "/treatments/shunts");

    [Fact]
    public async Task EveryDeepLinkPointsAtAnAnchorThatActuallyExists() =>
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    /// <summary>
    /// The nine region anchors are a published interface (§12.8, WI-509), and
    /// WI-569 through WI-573 will link into them. Assert the ids are in the
    /// rendered HTML, because that is the thing an inbound link lands on.
    /// </summary>
    [Fact]
    public async Task EveryRegionAnchorIsInTheRenderedHtml()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var anchor in new[]
        {
            "front", "side", "upper-back", "back", "cerebellum",
            "brainstem", "ventricles", "skull-base", "pituitary",
        })
        {
            Assert.Contains($"id=\"{anchor}\"", html, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// EVERY SAME-PAGE LINK LANDS SOMEWHERE. <c>AssertFragmentLinksResolve</c> cannot
    /// see these: its pattern is <c>href="(/[^"?#]*)#([^"]+)"</c>, which requires a
    /// leading slash, so a bare <c>href="#the-fluid"</c> is invisible to it. This
    /// page has four of them, including the one the short version uses to point a
    /// reader at the urgent section. Rename that heading's anchor and the reader
    /// lands at the top of a long page with a green suite — the WI-508 failure the
    /// helper's own docstring describes.
    /// </summary>
    [Fact]
    public async Task EverySamePageLinkLandsOnAnIdThatExists()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        var targets = Regex.Matches(html, "href=\"#([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        // The positive half. A page whose in-page links were all deleted would
        // otherwise satisfy this in silence.
        Assert.True(targets.Count >= 3,
            $"only {targets.Count} same-page link(s) found, so this guard is reading "
            + "almost nothing");

        foreach (var target in targets)
        {
            Assert.Contains($"id=\"{target}\"", html, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// No authoring marker reaches the reader. <c>ShippedGlossaryTests</c> now catches
    /// a marker broken across a LINE, which is the failure this item hit; this catches
    /// the other one — an unterminated or mistyped marker, which renders its own
    /// characters. The <c>CtScanPageRenderTests</c> convention, copied here because
    /// this page is the reason the line-wrap guard exists.
    /// </summary>
    [Fact]
    public async Task NoAuthoringMarkerReachesTheReader()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.DoesNotContain("!%", html, StringComparison.Ordinal);
        Assert.DoesNotContain("%%", html, StringComparison.Ordinal);
        Assert.DoesNotContain("[ESCALATION]", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// The escalation tiers reach the reader. A block that fails to compose leaves a
    /// page whose own prose promises a list of warning signs and then does not show
    /// one, with a green build (§12.10).
    /// </summary>
    [Fact]
    public async Task TheEscalationBlockComposesOntoThisPage()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Call an ambulance for any of these", html, StringComparison.Ordinal);
        Assert.Contains("Call your team the same day for any of these", html,
            StringComparison.Ordinal);
        Assert.DoesNotContain("[ESCALATION]", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// The frontal-lobe tooltip is SUPPRESSED here, because this page defines the
    /// word in the sentence beside it, and it FIRES on a page that says the word
    /// without defining it. Both directions, because a suppression marker that
    /// silently killed the entry everywhere would satisfy the first half alone
    /// (§12.8, WI-509).
    /// </summary>
    [Fact]
    public async Task TheFrontalLobeTooltipIsSuppressedHereAndFiresWhereItIsNotDefined()
    {
        var client = _factory.CreateClient();

        var mine = await client.GetStringAsync(Url);
        Assert.Contains("frontal lobe", mine, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("def-frontal-lobe", mine, StringComparison.Ordinal);

        // The OTHER suppression this page carries. It defines "posterior fossa" in the
        // sentence the popover would attach to, so §12.8/WI-509 says suppress it — and
        // /review round 11 found nothing reading it. The entry still exists and still
        // fires elsewhere, which the second half asserts.
        Assert.DoesNotContain("def-posterior-fossa", mine, StringComparison.Ordinal);
        Assert.Contains("posterior fossa", mine, StringComparison.OrdinalIgnoreCase);

        var stillFires = await client.GetStringAsync("/tumors/atrt");
        Assert.Contains("def-posterior-fossa", stillFires, StringComparison.Ordinal);

        var elsewhere = await client.GetStringAsync("/tumors/oligodendroglioma");
        Assert.Contains("def-frontal-lobe", elsewhere, StringComparison.Ordinal);

        // AND THE OTHER SUPPRESSION THIS ITEM ADDED. /tumors/astrocytoma glosses the
        // term in its own prose ("near or in the frontal lobes, the front of the
        // brain"), so §12.8/WI-509 says suppress it there too. /review round 6: no
        // assertion read that page, so deleting the marker left the suite green while
        // a popover started repeating a gloss the sentence already gives.
        var astrocytoma = await client.GetStringAsync("/tumors/astrocytoma");
        Assert.Contains("frontal lobes", astrocytoma, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("def-frontal-lobe", astrocytoma, StringComparison.Ordinal);
    }

    /// <summary>
    /// The door added to <c>blocks/mechanism.md</c> is an APPENDED paragraph, not a
    /// rewrite of the block's closing one — §12.8 (WI-519): a door added to a file
    /// eighteen pages compose is prose on eighteen pages, and replacing a sentence
    /// there breaks those pages' own assertions. WI-568's own closing sentence must
    /// still be above it, on a composed page.
    /// </summary>
    [Fact]
    public async Task TheDoorInTheBlockFollowsTheClosingSentenceRatherThanReplacingIt()
    {
        var html = await _factory.CreateClient().GetStringAsync("/tumors/glioma");

        var closing = html.IndexOf("whether it fits", StringComparison.OrdinalIgnoreCase);
        var door = html.IndexOf($"href=\"{Url}\"", StringComparison.Ordinal);

        Assert.True(closing > 0, "the block's closing sentence has been rewritten");
        Assert.True(door > closing,
            "the door now sits above the block's closing sentence, so it replaced prose "
            + "rather than following it");
    }
}
