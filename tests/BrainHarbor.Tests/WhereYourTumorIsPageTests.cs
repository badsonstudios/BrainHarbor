using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
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
///     and the tectal literature's own resection range BETWEEN THE STUDIES ONE
///     SYSTEMATIC REVIEW POOLED is 2.3% to
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
///   * PRE-EMPTING WAVE 6. No diagram (WI-572), no visual-field or driving content
///     (WI-573), and the /tumors/meningioma duplication MechanismBlockTests pins is
///     left pinned (WI-569). <b>The tectal ban is GONE, because WI-571 shipped the
///     section it was holding the door open for</b> — see
///     <see cref="ThePageDoesNotTakeOverTheWorkOfTheItemsThatFollowIt"/>, which was
///     rewritten rather than deleted (WI-547's precedent: an assertion that depended
///     on unwritten work gets a new subject, not a funeral).
///
/// WI-571 ADDED ONE SECTION AND TWO DOORS, and its ruling is content-pipeline §12.20.
/// The section is <c>## When the word you were given is a place</c>, for the reader
/// handed an address where they expected the name of a growth. What is load-bearing about
/// it, each part with its own assertion below:
///
///   * <b>IT IS <c>##</c> ONLY, NO <c>###</c>.</b>
///     <see cref="TheNineRegionsAreThereInTheBlocksOrderWithWrittenAnchors"/> counts
///     <c>^### … {#anchor}</c> over the WHOLE page and asserts exactly nine, so a
///     <c>###</c> anywhere would red a region guard for a reason that has nothing to
///     do with regions.
///   * <b>THE ROW IT KEYS TO AN ADDRESS IS LICENSED, NAMED RATHER THAN INFERRED.</b>
///     §12.19: an address may key what is aimed at ONLY where the source's own scope is
///     the site, and never how the reader does. (Unquoted, so it is a paraphrase rather than
///     a modified quotation — but an earlier version dropped the "only", which is the
///     permission without its limit. /review round 14.) The systematic review behind
///     "most are watched" is scoped to nothing but this one spot.
///   * <b>NOTHING WENT INTO THE SHORT VERSION, because it is at its cap</b> — eight
///     sentences, which
///     <see cref="TheRoundNineFixesArePinnedAndSlotZeroIsWithinItsContract"/> measures
///     as the longest shipped in the corpus. Two doors were written instead, one in
///     the regions preamble and one at the end of slot 7.
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
    private const string PlaceWordHeading = "When the word you were given is a place";
    private const string SightHeading =
        "When what changed is your sight, and what that means for driving";
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
    /// The title and description a reader meets before the body — <b>promoted to
    /// <see cref="CuratedPage.Headline"/> by WI-575</b>, which found this property
    /// duplicated in NINETEEN files with one implementation and five different
    /// narrations of the same CRLF lesson. The lesson, the <c>\s*$</c> anchor and the
    /// non-empty assertion now live in one place; see
    /// <see cref="CuratedPage.EverythingAReaderMeets"/> for the widened reader text a
    /// prose rule should usually be written against.
    /// </summary>
    private static string Headline => CuratedPage.Headline(Page);

    /// <summary>Everything a reader meets: the headline and the body.</summary>
    private static string Flat =>
        Headline + " " + CuratedPage.Flatten(Reader);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// The page's reader text with WI-573's section AND ITS HEADING removed.
    ///
    /// <para><b>The heading itself carries <c>driving</c></b>, and
    /// <see cref="CuratedPage.Section"/> returns the body WITHOUT it — so a helper that
    /// strips only the body leaves a <c>driv</c> match that has nothing to do with
    /// containment. That artifact showed up in this item's own occurrence count before
    /// the assertion was written from it, which is the only reason it is not in the
    /// assertion: <b>derive a count from the page, then read what the count is made
    /// of.</b></para>
    /// </summary>
    private static string OutsideTheSightSection
    {
        get
        {
            // FLAT, NOT Flatten(Reader) -- and this is WI-575's hole, found by this
            // item's own break harness rather than by reading. `Reader` is the BODY:
            // `ReaderText` strips the front matter by design, so the `description`
            // that ContentPage.cshtml renders as the FIRST PARAGRAPH a reader meets
            // is invisible to it. This item ADDED a driving clause to that
            // description, and deleting the clause left this test green. A
            // containment count that cannot see half the page a reader reads was
            // getting the right answer for the wrong reason.
            var reader = Flat;
            var section = FlatSection(SightHeading);

            Assert.Contains(section, reader, StringComparison.Ordinal);

            return reader
                .Replace(section, " ", StringComparison.Ordinal)
                .Replace(SightHeading, " ", StringComparison.Ordinal);
        }
    }

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

    /// <summary>
    /// The regions section's PREAMBLE — everything between its <c>##</c> heading and the
    /// first region entry. §12.17 records this as the part of a section no guard reads, and
    /// round 6's fix for "the reader whose report word is none of the nine" landed exactly
    /// there.
    ///
    /// <para>Factored out at WI-571's <c>/review</c> round 2, which found it computed in
    /// two places with two different reader-text treatments — one through
    /// <see cref="ReaderFragment"/> and one not. They agree today. §12.8's rule is that the
    /// second use is where a thing becomes a factor.</para>
    /// </summary>
    private static string RegionsPreamble()
    {
        var section = Section(RegionsHeading);
        var first = section.IndexOf("### ", StringComparison.Ordinal);

        // CHECKED BEFORE THE SLICE (/review round 9). A range expression off a -1 throws
        // ArgumentOutOfRangeException from inside a helper, which reddens two tests with a
        // stack trace instead of the message written for that case — the same failure round 8
        // closed for the place-word slice, and worse here because this one is shared.
        Assert.True(first > 0,
            "the regions section has no `### ` entries, so there is no preamble to read and "
            + "every assertion below this helper is about to be green over nothing");

        return CuratedPage.Flatten(ReaderFragment(section[..first]));
    }

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
        // WI-571 moved this from 2026-09-24. The page gained a section and a set of sources
        // the front matter enumerates — this note gave the number as two, then three, and
        // round 11 found the fourth had arrived at round 10 without reaching either copy,
        // so the date the content was last read against its sources moved with it — a
        // `reviewed` date that stays put through an edit is the one field on the page
        // that lies about itself.
        Assert.Contains("reviewed: 2026-09-30", front, StringComparison.Ordinal);
        Assert.Contains("review_due:", front, StringComparison.Ordinal);
        Assert.Contains("disclaimers: [medical]", front, StringComparison.Ordinal);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        // WI-571 added four tectal papers — three at the start, and a fourth at /review
        // round 10 when the item found it had recorded a readable source as unreadable for
        // nine rounds. The floor stays at six because it is a floor, and this comment names
        // no totals because round 11 found both of the ones it used to give were wrong, in
        // the body of the method that computes `urls.Count`.
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
        var preamble = RegionsPreamble();

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

        // THREE OF THE FOUR WORDS STAY BANNED HERE, AND THE FOURTH IS NOW A PROPERTY
        // RATHER THAN A BAN (WI-571).
        //
        // The list was `tectal`, `pineal`, `suprasellar`, `thalam`, and its recorded
        // reason was that the preamble "answers that reader WITHOUT naming any of them,
        // which is what keeps this item out of that one". That reason expired for ONE of
        // the four the day WI-571 shipped `#a-place-for-a-name`, and §12.19's rule is
        // that a rationale which stopped being true is how a ban gets copied forward.
        // So it was rewritten rather than deleted (WI-547's precedent).
        //
        // The other three are still not this item's subject, and naming one of them here
        // would be §12.17's closed-count trap again: the preamble's job
        // is the GENERAL answer for a reader whose word is none of the nine, and a list
        // of examples in a general answer asserts there is no next example.
        foreach (var word in new[] { "pineal", "suprasellar", "thalam" })
        {
            Assert.DoesNotContain(word, preamble, StringComparison.OrdinalIgnoreCase);
        }

        // `tectal` APPEARS HERE AS A DOOR, AND BOTH HALVES ARE UNCONDITIONAL. The preamble
        // hands the reader their word so they can see it and follow it; what it may not do
        // is ANSWER them here, which the symptom-and-outcome ban above already covers.
        //
        // THE FIRST VERSION OF THIS WAS A CONDITIONAL AND /review ROUND 1 SHOWED IT COULD
        // GO VACUOUS AS A PAIR. It read `if (preamble.Contains("tectal")) Assert.Contains(
        // "#a-place-for-a-name", preamble)`, which is a no-op the moment the word goes —
        // and the only unconditional door assertion is scoped to `Section(RegionsHeading)`,
        // which per §12.19 finding 1 spans all nine `###` entries. So an edit that dropped
        // the word from the preamble and moved the link down into a region entry left both
        // green and left the preamble's reader with nothing. **A conditional guard and a
        // loosely-scoped one can be green about the same deletion.**
        Assert.Contains("tectal", preamble, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#a-place-for-a-name", preamble, StringComparison.Ordinal);
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
                // AssertNoWarningSignIsNormalised's own lookbehind — **which it no longer is**:
                // that one uses a 40-character window with several negators, and /review round 9
                // of WI-571 recorded that this repo has already ruled the fixed-width form
                // broken in both directions. This copy stays because its three negators are
                // exactly the shapes its own sentence needs and it has no `n't` alternative to
                // be silently dead (see ThePageNeverAssertsAClosedCount for the one that did).
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
        // MOST OF THIS LIST IS NOW REACHABLE ONLY THROUGH ThePageNeverAssertsAClosedCount, and
        // **`two places carry rules it does not cover` is the one entry nothing page-wide
        // covers** — neither `the two places` nor `only two places` is a substring of it. The
        // rest are wholly covered except `these are the only`, which the page-wide loop bans
        // only unnegated. They stay because this list's subject is the exclusivity claims THIS
        // section kept producing, and deleting the covered ones makes the history harder to
        // read (/review round 10, the same treatment the `low-grade` entry got in
        // TheFluidSectionDoesNotClaimSizeOrGradeIsIrrelevant).
        //
        // **AND THE COUNT IS GONE BECAUSE THREE ATTEMPTS AT IT GAVE THREE WRONG ANSWERS**
        // ("three of these", then "two and a half", then a pair that named the wrong two) —
        // five strings, in the `closedCount` loop just below, in the note whose own subject
        // is that a count in prose beside a count in code is the defect. Naming the LIVE one is
        // what a reader needs; the covered ones are derivable from the two loops.
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
        //
        // `low-grade` IS NOW A DEAD ENTRY HERE AND `low grade` IS NOT, which is the correction
        // /review round 17 made to this note: WI-571 bans `low-grade` page-wide as a SUBSTRING,
        // and "low grade" with a space does not contain it — so the spaced spelling is still
        // guarded only here. It stays either way, because this list's subject is the REFUSED
        // CLAIM ("small, slow and low-grade and still an emergency"), which is a sentence shape
        // rather than a vocabulary. (Round 5 wrote "two of its words"; one.)
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
        //
        // AND THE 911 THIS SEES IS THE PAGE'S OWN, not the block's. `Flat` is
        // Headline + Flatten(ReaderText(Page)) and nothing here COMPOSES, so
        // `[ESCALATION]`'s own 911 is outside every bar in this file; what is in scope is
        // the fluid section's pituitary-apoplexy tier ("that one is a 911 call rather than
        // a wait for morning"). WI-571's /review round 8 wrote the opposite into a comment
        // beside a duplicate of this assertion, and round 9 deleted both. The composed
        // page's distinct-digit set is unchanged either way — the block prints 911 and
        // nothing else — which is what makes this absolute true of what a reader sees.
        //
        // WI-571 TRIED TO REBUILD THIS THREE TIMES, and the reason is worth having here
        // rather than in the item that kept losing it: its new section says out loud "No
        // number from any of those papers is on this page", and every attempt to guard that
        // sentence started from a LIST of the figures in its sources — nine strings, then
        // fourteen. **A hand-listed subset cannot carry an absolute**: the page's own front
        // matter names 24% and 25% figures neither list held. The digit check is the whole
        // of it, it is self-maintaining, and the day a legitimate number is wanted on this
        // page this reds and the reader-facing sentence has to change with it.
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
    /// about location is where all of them would naturally creep in.
    ///
    /// <para><b>THE TECTAL BAN IS GONE AND THE TEST IS NOT.</b> It read
    /// <c>Assert.DoesNotContain("tectal", Flat)</c>, with the reason that WI-571 owned
    /// the word. WI-571 is this edit, so the ban was rewritten rather than deleted —
    /// WI-547's ruling one page over: an assertion that depended on unwritten work gets
    /// a new subject rather than a funeral. What replaced it is the positive half, in
    /// <see cref="TheSectionForTheReaderWhoseOnlyWordIsAPlace"/>: the word is now
    /// REQUIRED to be in reader text, because that is the only thing that makes the
    /// page findable by a reader who types it into the search box.</para>
    ///
    /// <para><b>AND WI-573's HALF GOT HARDER TO KEEP, ON THE THREE OF THIS ITEM'S FOUR
    /// SOURCES THAT CARRY SYMPTOM OR PRESENTATION LISTS.</b> (This said "all three of this
    /// item's sources" until <c>/review</c> round 12 — the last copy of the count round 11
    /// corrected in §12.20 and in the front matter, and the item has four sources.) The
    /// systematic review names <i>"visual field changes"</i> among the
    /// common findings for this exact tumor; Imoto's symptom sentence says <i>"along with
    /// visual field deficits and cognitive dysfunction"</i>; and the third source names
    /// <i>"extraocular eye movement abnormalities at presentation"</i> and, in that paper's
    /// introduction, <i>"Neurological deficits are less commonly observed but may include
    /// nystagmus, diplopia, seizures, and visual deficits"</i> — the DOI serves its full
    /// text, not just its abstract. So the ban stood between the page and a correctly quoted
    /// source <b>four times, across three sources</b>. (This said "three times over" until
    /// <c>/review</c> round 7, one round after the fourth was added to §12.20 and to the
    /// page's front matter — the item's own "correcting a claim in one file does not correct
    /// its copies", for the third time.) It stays: a
    /// jurisdictional driving rule with no waiting time is WI-573's whole problem, and half
    /// of it published early is worse than none of it. (This paragraph said <i>"this is the
    /// one place"</i> until <c>/review</c> round 5 — a closed count, and <b>a count of one is
    /// the easiest closed count to believe because nobody recounts it</b>. Round 5 also added
    /// that this file's guards banned the string in reader text; they did not, until round 6
    /// widened them below.)</para>
    /// </summary>
    [Fact]
    public void ThePageDoesNotTakeOverTheWorkOfTheItemsThatFollowIt()
    {
        // WI-573 IS THIS EDIT, so its three bans are retired and replaced by
        // CONTAINMENT rather than deleted — WI-547's precedent, and the same move
        // WI-571 made on `tectal` one item ago. "Require the word in the section" is
        // a DIFFERENT property from "forbid it everywhere else", and §12.20 records
        // that nothing was stopping the retired subject spreading into the fluid or
        // surgery sections. Those two are the least able to carry a jurisdictional
        // driving rule, so both halves are asserted.
        Assert.Contains("visual field", FlatSection(SightHeading),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("visual field", OutsideTheSightSection,
            StringComparison.OrdinalIgnoreCase);

        // `driv` HAS EXACTLY THREE LICENSED HOMES OUTSIDE THE SECTION, and this
        // count has been wrong twice, in opposite directions, which is why all three
        // are NAMED as well as counted. It was ONE until the break harness pointed
        // out that the scope could not see the front-matter description at all
        // (WI-575's hole), and TWO until /review round 1 pointed out that the
        // `#pituitary` door was the one that should mention driving — the sellar
        // reader is the likelier driving reader, so the asymmetry ran backwards.
        //
        // The three: the door in the `#back` entry (the region whose own text says
        // it is where seeing happens), the door in the `#pituitary` entry, and the
        // front-matter `description`, which is the first paragraph a reader meets.
        // The count is asserted rather than the absence, because a door is a
        // sentence a later edit can move — and each is named too, because a count of
        // three is satisfied by any three and this item has already been caught
        // twice by a count that was right for the wrong reason.
        var strays = Regex.Matches(OutsideTheSightSection, "driv", RegexOptions.IgnoreCase);
        Assert.True(strays.Count == 3,
            $"`driv` occurs {strays.Count} times in the text a reader meets outside "
            + "the sight section, and exactly three are licensed (the doors in the "
            + "`#back` and `#pituitary` entries, and the front-matter description). "
            + "Driving is routed on this page, never stated, and a fourth home for it "
            + "is how a jurisdictional rule reaches a section that cannot carry one.");
        Assert.Contains("and it covers driving", OutsideTheSightSection,
            StringComparison.Ordinal);
        Assert.Contains("covers it, including what it means", OutsideTheSightSection,
            StringComparison.Ordinal);
        Assert.Contains("it says what that means for driving", Headline,
            StringComparison.Ordinal);

        // WI-572 owns the diagram, and the corpus has no images at all today.
        Assert.DoesNotContain("![", Page, StringComparison.Ordinal);
        Assert.DoesNotContain("<figure", Page, StringComparison.Ordinal);
    }

    /// <summary>
    /// WI-573's SECTION: the general answer said ONCE, the rule ROUTED, and no duration
    /// anywhere.
    ///
    /// <para><b>THE BACKLOG'S NAMED SOURCE DOES NOT CARRY THE CLAIM</b>, which is this
    /// item's first finding and the reason the citation in the front matter is not the one
    /// the acceptance asked for. Cancer Research UK's driving page was read live: the
    /// string <c>visual field</c> occurs ZERO times on it and <c>visual</c> zero times as
    /// a word. Its two vision sentences are pituitary-scoped UK regulation — one a waiting
    /// time, one a notification duty — so there is nothing on it this page may publish. The
    /// source that carries the link is PMC11913653, a brain-tumor fitness-to-drive review
    /// the corpus ALREADY cites on <c>/tests/neuro-exam-and-memory-testing</c>, which makes
    /// it a corpus source rather than one page's find. (This said "its second user"
    /// until <c>/review</c> round 4, after the <c>.md</c> note carrying the same
    /// claim had already been corrected in round 3 — §12.20's <i>correcting a claim
    /// in one file does not correct its copies</i>, inside the item that cites
    /// it.)</para>
    ///
    /// <para><b>AND THE ROUTE'S BOUND IS MEASURED RATHER THAN ASSUMED</b> —
    /// §12.19 finding 6, <i>a route can be word-perfect and still be addressed to the
    /// wrong reader.</i> <c>/seizures/living-with#driving</c> is SEIZURE-SCOPED throughout
    /// (<i>"rules about driving after a seizure"</i>, <i>"free of seizures for a set length
    /// of time"</i>, and its lookup tool is the Epilepsy Foundation's). A reader whose
    /// problem is vision and who has never had a seizure is not served by its rules half.
    /// The route is still right, because the SHAPE and the practical half are the whole
    /// point — so the bound is written into the prose and the destination got a door back,
    /// and both are asserted here.</para>
    /// </summary>
    [Fact]
    public void TheSightSectionAnswersTheQuestionAndRoutesTheRuleAway()
    {
        var section = FlatSection(SightHeading);

        // THE ANSWER. The report word a reader is handed, and the plain-language
        // meaning, because the word on the paper is often the only clue they have.
        Assert.Contains("visual field", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hemianopia", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("scotoma", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("perimetry", section, StringComparison.OrdinalIgnoreCase);

        // THE SENTENCE THE SECTION TURNS ON, and it is the one that makes the whole
        // thing a measurement rather than a hedge: the source reports a wide range in
        // hazard detection among people with SIMILAR amounts of loss. Without it,
        // "we cannot tell you" reads as evasion instead of as the finding it is.
        Assert.Contains("Two people missing the same amount can drive very differently",
            section, StringComparison.Ordinal);

        // NOT A GLASSES PROBLEM. The source's own clause is "cannot be addressed with
        // prescription glasses", and this is the most directly useful thing on the
        // page for a reader who passed an eye chart and was told they were fine.
        Assert.Contains("not a glasses problem", section, StringComparison.OrdinalIgnoreCase);

        // THE ROUTE, WITH ITS BOUND. Both halves: the link, and the sentence saying
        // what the destination was written around. A route whose promise is unbounded
        // is §12.19's over-claim, and the over-claim direction that matters here is
        // telling a vision reader that a seizure page has their rule.
        Assert.Contains("(/seizures/living-with#driving)", Section(SightHeading),
            StringComparison.Ordinal);
        Assert.Contains("It was written for seizures", section, StringComparison.Ordinal);

        // AND THE STATE LOOKUP IS NAMED AS INAPPLICABLE, not just the requirement.
        // /review round 1: the destination's practical half includes the Epilepsy
        // Foundation's state tool, which returns SEIZURE-FREE INTERVALS. A vision
        // reader told "the practical half is the whole point" could read one of those
        // as their own answer, which is the under-triage direction. Naming the
        // requirement as separate did not name the TOOL as inapplicable.
        Assert.Contains("The state lookup on it is about seizures too", section,
            StringComparison.Ordinal);

        // AND THE RULE ITSELF IS NOT RESTATED. No requirement, no threshold, no state
        // named. §12.20's closed-count lesson applies to a licensing figure too: a
        // number from the wrong place is the one thing this corpus refuses hardest,
        // and the source hands over both a degree threshold and two named states.
        Assert.DoesNotContain("Massachusetts", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New Hampshire", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("degree", section, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(new Regex(@"\d"), section);

        // A LEGAL YES IS NOT A SAFE YES, and this is the under-triage-safe direction.
        // The section says a state can set no minimum at all; a reader who stops
        // reading there has been told they may drive. That is the one direction a
        // mistake on this page may not run, so the correction is pinned.
        Assert.Contains("Being allowed to drive is not the same as being safe to drive",
            section, StringComparison.Ordinal);

        // AND THE ONE INSTRUCTION IN THE SECTION, which /review round 2 found was
        // missing entirely. TWO of the four pages that route in told the reader to
        // get checked before driving — /review round 4 counted, after an earlier
        // version of this comment said all four — and the other two say only "ask
        // your team". Both have the instruction now, so the claim is true as well
        // as corrected. The section they route TO did not have it at all,
        // while supplying the permissive reading itself ("The state next door can
        // set none at all"). That is the one direction a mistake here may not run.
        Assert.Contains("Get your sight checked before you drive again", section,
            StringComparison.Ordinal);

        // AND BOTH PATTERNS, which is round 2's BLOCKER. The section said the loss
        // is "most often the same side in both eyes" — true of brain tumor patients
        // as a group, FALSE for the sellar reader, whose loss is the outer edge on
        // both sides. /tumors/craniopharyngioma and /tumors/pituitary-tumor both say
        // so in their own words and both route into this section, and round 1's own
        // #pituitary door is what aimed that reader at that sentence. The source
        // carves it out in its very next sentence. Naming both patterns and telling
        // the reader to ask which is theirs hands nobody a lookup, so it is not a
        // Wave 6 row — which is what the first draft wrongly claimed it would be.
        Assert.Contains("the missing piece is on the same side in both eyes", section,
            StringComparison.Ordinal);
        Assert.Contains("For others it is the outer edge in both eyes instead", section,
            StringComparison.Ordinal);
        Assert.Contains("ask your team which one is yours", section,
            StringComparison.Ordinal);

        // NOT "which of the two". /review round 4: the paragraph opens "There is
        // more than one pattern" (open), narrowed to "which of the two" (closed),
        // and the very next sentence named a third thing — and the source names a
        // fourth and fifth this page does not teach. §12.17's closed count, on the
        // page §12.17 was written about, introduced by the fix for round 2's blocker.
        Assert.DoesNotContain("which of the two", Flat, StringComparison.OrdinalIgnoreCase);

        // AND THE SELLAR READER IS TOLD THE EVIDENCE IS NOT ABOUT THEM. Round 3
        // re-scoped every driving-evidence sentence to the same-side pattern, which
        // was correct — and left the reader with the OTHER pattern reading a section
        // in which nothing applied to them, including the caution. Absence of
        // evidence must not read as absence of a problem.
        Assert.Contains("Most of what has been studied is the same-side pattern",
            section, StringComparison.Ordinal);

        // AND THE DOUBLE-VISION PARAGRAPH IS PINNED. It is the only double-vision
        // content in the section, nothing asserted it, and the sentence floor had
        // exactly enough headroom to lose it silently — /review round 4 measured
        // that deleting it landed the count ON the floor rather than under it.
        Assert.Contains("Double vision matters here too", section,
            StringComparison.Ordinal);

        // AND THE SECTION'S OWN OPENER, which was the one paragraph of seventeen
        // that no string in the whole suite asserted — so deleting the section's
        // headline landed the sentence count exactly ON the floor and stayed green.
        // /review round 5 found it by constructing the regression rather than by
        // reading the guard. **Ask what regression a guard is for, then build it.**
        Assert.Contains("If sight is the thing that changed, this is the section for it",
            section, StringComparison.Ordinal);

        // AND THE CORRECTION COMES BEFORE THE WAY OUT. Pinning its STRING is not
        // pinning its PLACE: it used to sit after the outbound link, so a reader who
        // took the permissive reading ("Another state can set none at all") and
        // followed the link never met it. Four rounds rewrote these two paragraphs
        // and none re-checked the sequence.
        var permissiveAt = section.IndexOf("The same eyes, two answers",
            StringComparison.Ordinal);
        var correctionAt = section.IndexOf("Being allowed to drive is not the same",
            StringComparison.Ordinal);
        var wayOutAt = section.IndexOf("/seizures/living-with#driving",
            StringComparison.Ordinal);

        Assert.True(permissiveAt > 0 && correctionAt > 0 && wayOutAt > 0,
            "a landmark was not found, so the ordering assertion below proves nothing");
        Assert.True(permissiveAt < correctionAt && correctionAt < wayOutAt,
            $"the correction (at {correctionAt}) has to sit between the permissive "
            + $"sentence (at {permissiveAt}) and the link out (at {wayOutAt}). A "
            + "correction placed after an exit is a correction the reader can miss, "
            + "and the reading it corrects is that they may drive.");

        // AND THE EVIDENCE IS SCOPED TO THE PATTERN THE SOURCE STUDIED, which is
        // /review round 3's blocker and which round 2's fix created. Every
        // driving-evidence sentence in the source is HOMONYMOUS-scoped; the moment
        // the section described a second pattern, "people with similar losses" and
        // "people with this loss" silently covered both. The direction was
        // reassuring, on the page whose reader may not drive safely. **A fix's blast
        // radius is every sentence that depended on the old scope.**
        Assert.Contains("people who had lost the same side in both eyes ranged widely",
            section, StringComparison.Ordinal);
        Assert.Contains("Some people who have lost the same side are assessed as safe",
            section, StringComparison.Ordinal);
        Assert.Contains("the side or sides you cannot see", section,
            StringComparison.Ordinal);

        // AND `hemianopia` IS NOT ATTACHED TO ONE PATTERN. Bitemporal hemianopia is
        // hemianopia, and /tumors/pituitary-tumor's own source list names hemianopia
        // for exactly the reader the `#pituitary` door sends here. Teaching the word
        // as the same-side pattern would hand that reader the wrong answer from the
        // one word on their report they recognise.
        Assert.Contains("You may hear either one called **hemianopia**", Page,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// NO WAITING TIME IN THE SIGHT SECTION, asserted with both canaries.
    ///
    /// <para>WI-560 owns waiting times and this item prints none, and
    /// <c>SeizureContentTests.NoCuratedPagePrintsADrivingWaitingPeriod</c> already scans
    /// the whole corpus for a duration in a sentence mentioning driving. <b>That guard
    /// found this item's own front matter</b>, where the first draft quoted CRUK's barred
    /// waiting-time sentence verbatim in order to record that it is barred — the corpus's
    /// usual way of refusing a claim, which puts the figure in the file. The guard was not
    /// touched and the note was rewritten. <i>A quotation is not exempt from a ban on the
    /// claim it quotes.</i></para>
    ///
    /// <para>This test is the section-scoped half, and it exists because the corpus guard
    /// only looks at sentences containing <c>driv</c>: a duration two sentences away from
    /// the word is invisible to it and would read to a reader as exactly the same promise.
    /// <b>Both canaries are asserted</b>, because a scan that has never been seen to fire
    /// has not been shown to work (§12.18) — and §12.8's prescribed negation pattern was
    /// wrong for a year for want of exactly this.</para>
    /// </summary>
    [Fact]
    public void NoWaitingTimeAppearsAnywhereInTheSightSection()
    {
        const string number =
            @"(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten|twelve|eighteen)";
        var duration = new Regex($@"\b{number}[\s-]+(?:day|week|month|year)s?\b",
            RegexOptions.IgnoreCase);

        // THE POSITIVE CANARY: a sentence this section must never carry, which the
        // pattern MUST catch. Without it a typo in the pattern reads as a clean page.
        Assert.Matches(duration, "You cannot drive for six months after that.");

        // THE NEGATIVE CANARY: prose the section does carry, which the pattern must
        // NOT catch. "A no now is not always a no later" is the shape that replaced
        // every duration, and a pattern that fired on it would push the next author
        // back toward printing the number.
        Assert.DoesNotMatch(duration, "A no now is not always a no later.");

        Assert.DoesNotMatch(duration, FlatSection(SightHeading));
    }

    /// <summary>
    /// THE ANCHOR THIS ITEM ROUTES TO EXISTS, verified rather than assumed.
    ///
    /// <para>Many pages in this corpus point at <c>/seizures/living-with#driving</c> and
    /// this item adds another. §12.19 finding 6's ruling is that a route's FILING and ANCHOR
    /// need checking as well as its promise, and an anchor is the half that fails silently:
    /// a wrong <c>#id</c> lands the reader at the top of a long page with no error anywhere.
    /// <b>An earlier version of this paragraph said "three pages, and this is the fourth",
    /// and its replacement said "the enumeration says eleven" when no enumeration
    /// existed.</b> §12.20: never write a count in prose about something the code can
    /// enumerate — and the sibling copy of this count, in <c>SeizurePagesTests</c>, gave a
    /// different wrong number, which is the two-homes-two-answers shape that ruling is
    /// written about. The enumeration is real now and lives in
    /// <see cref="LocationObligationSweepTests.EverySightAndDrivingSentenceInTheCorpusCarriesARoute"/>.</para>
    ///
    /// <para>It also asserts the door BACK, so the pair cannot be half-deleted. A vision
    /// reader who lands on a seizure page and finds only seizure rules concludes the
    /// question does not apply to them, which is the under-triage direction.</para>
    /// </summary>
    [Fact]
    public void TheDrivingAnchorExistsAndTheDoorRunsBothWays()
    {
        var destination = CuratedPage.Read("seizures", "living-with.md");

        Assert.Matches(new Regex(@"(?m)^## .*\{\#driving\}\s*$"), destination);
        Assert.Contains("/where-your-tumor-is#your-sight", destination,
            StringComparison.Ordinal);

        // AND THE ANCHOR ON THIS SIDE, which the other three pages' route depends on
        // too now that the destination points back at it.
        Assert.Matches(new Regex(@"(?m)^## .*\{\#your-sight\}\s*$"), Page);
    }

    /// <summary>
    /// THE RANKING GUARD OVER THE SIGHT SECTION: a THIRD configuration of §12.19's zero,
    /// and the one no earlier item has recorded.
    ///
    /// <para>§12.20 names two: <c>/treatments/craniotomy</c> has zero ADDRESSES with
    /// twenty-one difficulty matches, and WI-571's section has zero DIFFICULTIES with
    /// several addresses. <b>This section has zero of BOTH</b>, which makes the guard
    /// unable to fire from both ends at once — the weakest possible silence, and the one
    /// most likely to be read as a clean bill of health. §12.19: <i>a guard measured on a
    /// page it cannot fire on has been measured on nothing.</i></para>
    ///
    /// <para><b>That is a property of the section rather than an accident.</b> Its subject
    /// is a SYMPTOM and what to do about it, and the one sentence that could have keyed a
    /// place to it — the source's <i>"most commonly from a pituitary tumor near the optic
    /// chiasm"</i> — was deliberately not written, because the chiasm mechanism already
    /// belongs to <c>/tumors/craniopharyngioma</c> and restating it here would have put a
    /// Wave 6 row on the location page to say something another page already says. The
    /// section routes instead.</para>
    ///
    /// <para>So both zeros are asserted, and a row carrying BOTH halves is planted to
    /// prove the scanner reaches this text at all.</para>
    /// </summary>
    [Fact]
    public void TheRankingGuardCanFireInsideTheSightSectionOnceThereIsAnythingToFireOn()
    {
        var section = FlatSection(SightHeading);

        var addresses = Regex.Matches(section, CuratedPage.LocationAddress,
            RegexOptions.IgnoreCase).Count;
        var difficulties = Regex.Matches(section, CuratedPage.LocationDifficulty,
            RegexOptions.IgnoreCase).Count;

        Assert.True(addresses == 0 && difficulties == 0,
            $"the sight section now matches {addresses} address word(s) and "
            + $"{difficulties} difficulty word(s). Both were zero when it shipped, and "
            + "that is why the ranking guard is green over it. The moment either is "
            + "non-zero the row test has to be re-asked of the sentence that brought the "
            + "vocabulary in, and §12.21's account of why the guard is silent here has to "
            + "change with it. This is not a ban — it is the conversation a silent green "
            + "never starts.");

        // NOTHING IS FOUND TODAY, which is what the two zeros predict.
        Assert.Empty(CuratedPage.PlacesRankedIn(section));

        // AND THE PLANTED ROW, carrying BOTH halves in one sentence, because a plant
        // missing either one tests the lexicon rather than the reach of the scan. This
        // is what turns "no row was found" into "no row is there".
        const string planted =
            " A tumor at the base of the skull is harder to take out completely.";
        var withRow = CuratedPage.PlacesRankedIn(section + planted);

        Assert.Contains(withRow, r =>
            r.Trigger.Contains("base of the skull is harder", StringComparison.Ordinal));
    }

    /// <summary>
    /// NO SENTENCE IN THE SIGHT SECTION RUNS PAST 24 WORDS.
    ///
    /// <para>WI-571 shipped this ceiling for its own section
    /// (<see cref="NoSentenceInTheSectionRunsPastTheCeilingAPageAverageCannotSee"/>) after
    /// §12.17 measured a planted 36-word sentence moving this page from grade 5.6 to 5.7
    /// with ContentCheck passing at a 6.0 limit. <b>The gate is a page-average gate by
    /// construction</b>, and on a site whose audience may be cognitively impaired one
    /// unreadable sentence is enough to lose a reader.</para>
    ///
    /// <para>It is a second copy of the pattern rather than a shared helper on purpose:
    /// the two sections have different floors, and §12.8's factor-at-the-second-use rule
    /// is about the PROPERTY, which is here the ceiling and not the plumbing. If a third
    /// section needs one, that is the second use of the ceiling and it gets factored.</para>
    /// </summary>
    [Fact]
    public void NoSentenceInTheSightSectionRunsPastTheCeilingEither()
    {
        var prose = Regex.Replace(FlatSection(SightHeading), @"\[([^\]]*)\]\([^)]*\)", "$1");
        prose = Regex.Replace(prose, "[*#\"]", "");

        var sentences = Regex.Split(prose, @"(?<=[.!?])\s+")
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        // A FLOOR ON WHAT IT READ (§12.17: an iterate-and-check guard says out loud
        // how much it looked at). It was 45 until the break harness deleted the
        // section's last two paragraphs and this guard STAYED GREEN — eight sentences
        // of headroom is enough room to gut a section in, which is the criticism
        // WI-571's round 7 made of a floor of 30 against 45. The floor now sits a few
        // sentences under what the section had when it shipped, so losing a paragraph
        // is a red. The measurement is deliberately not written here: the split
        // enumerates it, and a number in a comment beside a number in code is the
        // shape §12.20 spends two sections on.
        //
        // IT WAS RAISED FIVE TIMES, and that is the lesson rather than the number.
        // Every readability pass in this item SPLIT sentences and grew the section, so
        // each raise was measured against the draft in front of the author and was
        // loose again by the next round; the gutting mutation stayed green twice more
        // after the first fix. **A floor is a constant and the thing it measures is
        // not.** The operational rule, which this item learned the expensive way and
        // which belongs here rather than in a ruling nobody reads mid-edit: <b>run the
        // break harness after the LAST prose edit</b>. A floor raised against an
        // earlier draft has been raised against nothing.
        Assert.True(sentences.Count >= 62,
            $"only {sentences.Count} sentences were found in the sight section, so either "
            + "it has been gutted or this split is reading the wrong thing");

        var over = sentences
            .Where(s => s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length > 24)
            .ToList();

        Assert.True(over.Count == 0,
            "a sentence in the sight section has run past 24 words, and the reading-grade "
            + "gate cannot see it. Split it.\n  " + string.Join("\n  ", over));
    }

    /// <summary>
    /// §12.17's CLOSED-COUNT BAN, PAGE-WIDE, AND IT IS HERE BECAUSE THREE NOTES CLAIMED IT
    /// ALREADY EXISTED.
    ///
    /// <para>Those notes said <i>"the page's own suite bans that string in reader text"</i>,
    /// and it did not. <see cref="ThePageNeverSaysTheFluidIsTheOnlyUrgentThing"/> bans
    /// <c>"the one place"</c> only inside sentences matching
    /// <c>urgen\w*|emergenc\w*|911|ambulance</c> — anywhere on the page, but only in an
    /// urgency window, and with link labels stripped first — and
    /// <see cref="ThePageNeverSaysTheFluidIsTheOnlyUrgentThing"/>'s section-scoped half
    /// covers the fluid section alone. A sentence saying it anywhere else passed the whole
    /// suite. <b>That is §12.17's most-repeated failure running BACKWARDS: not a guard
    /// scoped to the paragraph the defect was in, but a NOTE claiming coverage the guard
    /// does not have — which tells the next reader to stop checking.</b> (An earlier version
    /// of this comment said the old ban was "only inside the fluid section", which is the
    /// same mirror error in the sentence written to fix it. <c>/review</c> round 7.)</para>
    ///
    /// <para><b>ITS OWN FACT, NOT A CLAUSE INSIDE A WAVE 6 TEST</b> (<c>/review</c> round 7).
    /// It was first written inside
    /// <see cref="ThePageDoesNotTakeOverTheWorkOfTheItemsThatFollowIt"/>, whose whole subject
    /// is pre-empting later items — a subject that EXPIRES, and this item is the proof: WI-571
    /// had to rewrite that test because its own subject shipped, and WI-572 and WI-573 will
    /// each do it again. <b>A standing rule filed inside a test whose subject expires gets
    /// retired with its host.</b></para>
    ///
    /// <para><b>AND THE TWO "one thing / one time" PHRASES ARE HANDLED DIFFERENTLY FROM EACH
    /// OTHER, which took two rounds to get right.</b></para>
    ///
    /// <para><c>"the one thing"</c> is live in correct sentences on other pages, in this page's
    /// own register — <i>"time is the one thing you did not want to spend on this"</i>,
    /// <i>"The one thing to know before you start"</i>, <i>"being here early is the one thing
    /// that is different"</i> — so this page could legitimately acquire one tomorrow. It is
    /// banned here <b>behind the same negation lookbehind the urgency scan uses</b>, because
    /// the negated form is the correct way to write this page's own promise. <b>A ban list
    /// that forbids the correct shape is worse than no ban list.</b></para>
    ///
    /// <para><c>"the one time"</c> is banned bare, and round 7 was wrong to drop it: it has
    /// <b>zero</b> occurrences anywhere in the corpus's reader text, so §12.8's test passes at
    /// no cost — and <b>this page's own round-1 blocker was that exact phrase</b>, in the
    /// front-matter description. Round 7 deleted it on a cost measured for its neighbour.
    /// <b>Two phrases in one bullet are not one decision.</b> (Round 8 also deleted the count
    /// that justified the deletion: it was a number in prose about something a grep
    /// enumerates, and the two rounds that quoted it quoted two different figures.)</para>
    /// </summary>
    [Fact]
    public void ThePageNeverAssertsAClosedCount()
    {
        // FIVE BARE, and the two that came out are §12.8 again (/review round 11):
        // `"the only place"` and `"these are the only"` are banned only in their UNNEGATED
        // form below, because `not the only` is live correct prose on six files including this
        // one — "It is not the only reason to call anybody" is this page's own sentence, and
        // "this is not the only place where…" is a plausible correct future one.
        foreach (var closed in new[]
        {
            "the one place", "one place has", "the two places",
            "only two places", "the one time",
        })
        {
            Assert.True(!Flat.Contains(closed, StringComparison.OrdinalIgnoreCase),
                $"`{closed}` has appeared in this page's reader text. §12.17: naming N asserts "
                + "there is no N+1, and this page has been wrong about that more times than "
                + "any other property — including inside the fixes for earlier instances. "
                + "Write \"some\", \"includes\" or \"two of the\", and let the reader's own "
                + "case be the one you did not list.");
        }

        // AND `"the one thing"` BEHIND THE LOOKBEHIND, which the docstring above argued for
        // and the first version of this test did not actually do — the claim and the code
        // disagreed inside one method, which is this item's own most-repeated defect arriving
        // in the fix for it. The negated form ("it is not the one thing that…") is the
        // correct way to write this page's promise, so only the bare assertion is banned.
        // A WINDOWED LOOKBEHIND, not three fixed-width ones. /review round 9: this repo has
        // already ruled the fixed form broken in both directions —
        // CuratedPage.AssertNeverMinimises was fixed AWAY from it, and
        // AssertNoWarningSignIsNormalised uses a 40-character window with several negators.
        // Three four-character lookbehinds miss "not always", "not really", "no longer",
        // "never quite", "hardly" and "far from", every one of which is a correct sentence.
        //
        // AND THE EXEMPTION MATCHES NOTHING IN THE CORPUS TODAY, which §12.18 says to write
        // down rather than leave implied: all of the corpus's uses of this phrase are
        // unnegated, on pages where unnegated is correct. It is here because the negated form
        // is the correct way to write THIS page's own promise, not because it is in use.
        // THE WORD BOUNDARY GOES INSIDE THE ALTERNATION, NOT IN FRONT OF IT (/review round 16),
        // and this is a live bug rather than a tidy-up. Written `\b(?:not|never|no|hardly|n't|
        // far from)\b`, the leading `\b` applies to the whole group — and there is no word
        // boundary between the `s` and the `n` of "doesn't", so **the `n't` alternative could
        // never match** and "this isn't the only place where…" would have tripped the ban this
        // exemption exists to prevent. Measured against the real .NET engine, because Python
        // cannot compile a variable-width lookbehind at all: old fires on "it isn't the one
        // thing", new does not, and both still fire on the unnegated form.
        //
        // **A dead alternative in a negation guard does not make it weaker. It makes it fire on
        // the correct sentence the alternative was added to protect.**
        foreach (var qualified in new[] { "the one thing", "the only place", "these are the only" })
        {
            var asserted = Regex.Matches(
                Flat,
                @"(?<!(?:\bnot|\bnever|\bno|\bhardly|n't|\bfar from)\b[^.,;:]{0,25})"
                    + Regex.Escape(qualified),
                RegexOptions.IgnoreCase);
            Assert.True(asserted.Count == 0,
                $"this page now asserts \"{qualified}\" without negating it. Each of these is "
                + "live and correct elsewhere in the corpus in the negated form — this page's "
                + "own \"It is not the only reason to call anybody\" is one — so none is "
                + "banned outright. Unnegated, on this page, each is a closed count. Negate it "
                + "or drop the count.");
        }
    }

    // ------------------------------------------------------------------ §12.20 / WI-571
    // The reader whose only word is a place. "Tectal glioma" is a LOCATION descriptor,
    // not a WHO CNS5 name, so it cannot be tumor type #24 — §12.2 item 3 requires CNS5
    // naming throughout and taxonomy.yml's own header says an alias must be a "true
    // synonym, never 'close enough'". It is a section here instead.

    /// <summary>
    /// THE SECTION, AND THE FOUR THINGS THAT MAKE IT A SECTION RATHER THAN A TABLE ROW.
    ///
    /// <para><b>1. It teaches a WORD.</b> §12.17's central ruling, which is the whole
    /// reason this page cannot grow the location-to-risk table Wave 6 forbids: an entry
    /// that teaches a word has nowhere for a risk to go. So the section says where the
    /// tectum is, and then says what the word does NOT tell the reader.</para>
    ///
    /// <para><b>2. The one thing it keys to the address is LICENSED, and the licence is
    /// named.</b> §12.19: <i>"an address may key <b>what is aimed at</b> only where the source
    /// itself is scoped by site, and never <b>how the reader does</b>"</i> — lower case inside
    /// the marks, because §12.19 prints it that way and a quotation capitalised for emphasis is
    /// a modified quotation (<c>/review</c> round 13, the third instance of that defect and the
    /// first on this one).
    /// The systematic
    /// review behind "most of these were watched" is scoped to nothing but this one
    /// spot, and what it keys to the spot is what a team aims at. Nothing here is ranked
    /// against another address at all — so unlike <c>/tumors/chordoma</c>'s licensed
    /// pair there is not even a harder half and an easier half needing to balance.</para>
    ///
    /// <para><b>AND THE GUARD'S SILENCE HERE IS STRUCTURAL, NOT EVIDENCE — which is
    /// §12.19 FINDING 6 ARRIVING A THIRD TIME, INSIDE THE ITEM WHOSE SUBJECT IS THE ROW
    /// TEST.</b> The first version of this docstring said the ranking guard and the corpus
    /// sweep "both run over this prose and both stay green … which is the measurement
    /// rather than the intention". <c>/review</c> round 1 measured it: the section
    /// contains <b>zero</b> matches of <c>CuratedPage.LocationDifficulty</c>, so no window
    /// inside it can carry a difficulty word and no row could be found here whatever it
    /// said. That is exactly what §12.19 wrote down about
    /// <c>/treatments/craniotomy</c>'s famous zero — <i>"a guard measured on a page it
    /// cannot fire on has been measured on nothing"</i>, and it had already been quoted
    /// forward twice. So the zero is now ASSERTED as a property of the section, and the
    /// guard is shown able to fire here by planting a row in it —
    /// <see cref="TheRankingGuardCanActuallyFireInsideTheNewSection"/>. The row verdict
    /// rests on the written licence, not on a silence.</para>
    ///
    /// <para><b>3. It says the evidence is about CHILDREN.</b> The review's title, its
    /// abstract and every proportion in it are pediatric. This page already warns that
    /// two of its three surgery sources are written about adults; an adult holding this
    /// word needs the mirror of that warning, and it belongs on the page rather than in
    /// a front-matter comment where §12.17 records nothing reads it.</para>
    ///
    /// <para><b>4. It adds NO escalation rule.</b> The fluid section names two
    /// exceptions and then says <i>"those two are not the whole of it"</i>. Tectal
    /// hydrocephalus IS the fluid — already tiered inside <c>[ESCALATION]</c> and
    /// already that section's own subject — so this routes there. A third rule would
    /// also turn that section's <i>"two of those are just below"</i> into a closed
    /// count, which is the error §12.17 records this page making five times.</para>
    /// </summary>
    [Fact]
    public void TheSectionForTheReaderWhoseOnlyWordIsAPlace()
    {
        Assert.Contains($"## {PlaceWordHeading} {{#a-place-for-a-name}}", Page,
            StringComparison.Ordinal);

        var section = FlatSection(PlaceWordHeading);
        // 2,700 against a measured 3,093, not 900 (/review round 10). Round 7 rejected exactly
        // this looseness for the sentence floor and moved it from 30 to 40 against 45; a floor
        // at 29% of the measurement says almost nothing.
        Assert.True(section.Length > 2700,
            $"the section is {section.Length} characters, so either it has been gutted "
            + "or this guard is reading the wrong thing");

        // THE WORD IS IN READER TEXT. This is the positive half of the ban this item
        // retired, and it is not decoration: ContentStore.SearchPages scores
        // page.Markdown — the COMPOSED BODY, because Parse slices the front matter off
        // before storing it — so a word in one of this page's source comments is
        // invisible to search and to the reader by the same mechanism. Reader text is
        // the only thing that makes the page findable.
        Assert.Contains("tectal glioma", section, StringComparison.OrdinalIgnoreCase);

        // IT TEACHES A WORD (§12.17), and the new one is bolded the way the nine region
        // entries bold theirs.
        //
        // AND IT SAYS ONLY WHAT A SOURCE SAYS, which is /review round 1's sixth finding.
        // The first draft said "the roof of the midbrain" and that the fluid's tube "runs
        // right by that spot". Neither is in Imoto: `roof` does not appear, and `aqueduct`
        // appears ZERO times in that paper. Both were true anatomy and both were
        // inferences — on a page where every other sentence in this section has a verbatim
        // quote recorded beside it. What is here now is the source's own word: "the dorsal
        // part of the midbrain, consisting of the superior and inferior colliculi".
        Assert.Contains("**tectum**", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("back part of the", section, StringComparison.Ordinal);
        Assert.DoesNotContain("roof of the midbrain", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("runs right by that spot", Flat, StringComparison.OrdinalIgnoreCase);

        // AND IT SAYS WHAT THE WORD DOES NOT TELL THEM, which is the half a reader
        // cannot get anywhere else. Routed to the page's own general form rather than
        // restated (§12.10), and that route is asserted because deleting it would leave
        // the claim standing with nothing behind it.
        Assert.Contains("It does not say what the growth is made of", section,
            StringComparison.Ordinal);
        Assert.Contains("#what-it-means", Section(PlaceWordHeading), StringComparison.Ordinal);
        Assert.Contains("a different thing in different people", section,
            StringComparison.Ordinal);

        // THE LICENSED SENTENCE, IN BOTH ITS HALVES, AND THE THIRD SENTENCE THAT KEEPS
        // THE FIRST HONEST. The "watched" half without the "fluid" half would leave a
        // reader thinking nothing is ever done; the "fluid" half without the "watched"
        // half is the sentence the item was commissioned for.
        //
        // AND /review ROUND 1's BLOCKER LIVED IN THE JOIN BETWEEN THEM. The first draft
        // read "watched with repeat scans rather than TREATED", and the review's very
        // next sentence is "However, patients often need treatment for obstructive
        // hydrocephalus" — with CSF diversion in 89.3% of the cohort. 65.4% monitored
        // supports "most were watched"; "rather than treated" is false of a group where
        // nine in ten had a procedure, and `treated` is the SOURCE's own word for the
        // thing they needed. On the one page that owns the fluid escalation, an
        // understatement runs in the under-triage direction. The contrast is now with
        // having the GROWTH removed, which is what the review actually contrasts, and
        // the middle sentence is pinned because it is the one that cannot be dropped.
        Assert.Contains("watched with repeat scans", section, StringComparison.Ordinal);
        Assert.Contains("That is not the same as nothing being done", section,
            StringComparison.Ordinal);
        Assert.Contains("patients often need treatment", section, StringComparison.Ordinal);
        Assert.Contains("get the fluid moving", section, StringComparison.Ordinal);
        // PAGE-WIDE (/review round 10): this measured zero over `Flat` and the under-triage
        // wording is exactly what could next land in the FLUID section, which is the one
        // place on this page it would do real damage. Round 4 widened the prognosis and size
        // lists for the same reason, in
        // TheSectionPublishesNoFigureNoPrognosisAndNoClassification.
        Assert.DoesNotContain("rather than treated", Flat, StringComparison.OrdinalIgnoreCase);

        // THE EVIDENCE'S SCOPE, AND THE THING TO DO ABOUT IT. §12.6 asks a section to
        // land on something the reader can do.
        //
        // AND THE FIRST DRAFT'S VERSION OF THIS WAS AN ABSENCE CLAIM THAT DID NOT
        // SURVIVE ONE ROUND. It said "nothing read for this page says the same of
        // adults", which /review round 1 falsified by finding a 170-patient series whose
        // median age is 24 (PMID 37347621, now cited). **An absence claim is only as good
        // as the last search**, and this corpus has a rule about that: §12.17 records
        // every unreachable source so the next item does not re-investigate — the other
        // half of the same rule is that an absence is worth re-searching before it is
        // published. So the page now says what BOTH sources say, and the adult reader is
        // given the two-part shape rather than a shrug.
        Assert.Contains("it is not only a childhood word", section,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("more often found by chance", section, StringComparison.Ordinal);
        Assert.Contains("if you are an adult holding this word", section,
            StringComparison.Ordinal);
        Assert.DoesNotContain("nothing read for this page", Flat,
            StringComparison.OrdinalIgnoreCase);

        // AND THE PAPER IS NOT CALLED BIGGER THAN THE ONE BESIDE IT — /review round 2's
        // blocker, and it was inside round 1's fix. The paragraph said "A LARGER SERIES
        // that included adults"; it is 170 patients and the pediatric review beside it is
        // 355. The front matter knew both numbers. A reader going straight down the page
        // was told the adult evidence is the bigger half of the case, and it is less than
        // half the size. A comparative is a claim about the thing you are NOT looking at,
        // which is the routing lesson (§12.19) applied to a citation.
        Assert.DoesNotContain("larger series", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("with adults as well as children in", section,
            StringComparison.Ordinal);

        // THE CLOSED COUNT THAT ARRIVED WHILE FIXING THE OTHER CLOSED COUNT. Round 1
        // replaced "Two things to hold that with" with "Some things…", and round 2 found
        // the next sentence was "ONE OF THOSE TWO SOURCES is about children only" — a
        // closed count with no antecedent, in a section that names three studies — §12.17 on
        // this page again. (Two hand-kept ordinals for this family disagreed inside this file
        // until /review round 10, on top of the five §12.17 records. **The property is the
        // record; a tally nobody enumerates is the defect this page keeps making.**) It was
        // DELETED rather than reworded: the
        // pediatric scope is already carried where the source is introduced, which is
        // where a reader meets it.
        Assert.Contains("a review of how this is managed in children", section,
            StringComparison.Ordinal);
        // PAGE-WIDE for the same reason, and all five measured zero over `Flat` first.
        foreach (var closed in new[]
        {
            "Two things to hold", "Some things to hold", "one of those two sources",
            "those two sources", "both sources",
        })
        {
            Assert.DoesNotContain(closed, Flat, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE CAVEAT THE FIRST DRAFT LEFT OUT, which is this page's own central thesis
        // and the strongest thing in the section — now under its own lead, because round 2
        // found it buried at the end of a paragraph four sentences after a sentence that
        // broke it ("expect the same two parts", a prediction about one person).
        Assert.Contains("\"Most\" is a word about a group", section, StringComparison.Ordinal);
        Assert.Contains("never a prediction about one person", section, StringComparison.Ordinal);
        Assert.DoesNotContain("expect the same two parts", Flat, StringComparison.OrdinalIgnoreCase);

        // AND ITS POSITION, because round 2 gave the caveat its own lead and left it THREE
        // paragraphs from the word it is about (/review round 3). Both uses of "most" are in
        // the management paragraph; the caveat now follows that paragraph directly and
        // before the adult one. Only `Contains` was holding it, and a caveat a reader meets
        // after two intervening paragraphs is a caveat about something they have stopped
        // thinking about.
        var raw = Section(PlaceWordHeading);
        var mostUsed = raw.IndexOf("Most of these were watched", StringComparison.Ordinal);
        var caveat = raw.IndexOf("**\"Most\" is a word about a group.**", StringComparison.Ordinal);
        var adult = raw.IndexOf("**And it is not only a childhood word.**", StringComparison.Ordinal);
        Assert.True(mostUsed > 0 && mostUsed < caveat && caveat < adult,
            "the \"most\" caveat has drifted away from the paragraph that uses the word");

        // ADJACENT, NOT MERELY ORDERED. /review round 4: an ordering pin still permits a new
        // paragraph to be inserted between the two, which is the exact distance the pin was
        // added to close — the caveat spent round 2 and round 3 three paragraphs from its
        // own word. So the caveat has to be the NEXT bolded lead after the management
        // paragraph's.
        // OFF THE RAW PAGE, not off Section(): CuratedPage.Section FLATTENS, so `(?m)^` has
        // no line to anchor to and the first version of this matched nothing and failed with
        // a message about the wrong thing. §12.17's own trap — a guard that reads a
        // transformed copy of the text it thinks it is reading.
        //
        // AND IT SPLITS ON PARAGRAPHS RATHER THAN MATCHING "BOLD AT LINE START", which is
        // /review round 5's correction and it is two defects in one. `(?m)^\*\*[^*]+\*\*`
        // returned TEN matches over NINE paragraph leads — `**midbrain**` is an inline bold
        // that happens to begin a wrapped line — so (a) a pure REWRAP of the management
        // paragraph could red this with a message blaming the caveat for drifting, which is
        // the false-cause failure this assertion was rewritten to escape, and (b) it was
        // never adjacency: a paragraph with no bolded lead inserted between the two left it
        // green, and this section's own opening paragraph proves unbolded paragraphs are in
        // its style. Paragraphs are the unit the claim is about, so paragraphs are the unit.
        var rawPage = Page.Replace("\r\n", "\n", StringComparison.Ordinal);
        var from = rawPage.IndexOf($"## {PlaceWordHeading}", StringComparison.Ordinal);
        var to = rawPage.IndexOf($"## {HowLongHeading}", StringComparison.Ordinal);

        // ORDER CHECKED BEFORE THE SLICE. /review round 8: a range expression throws
        // ArgumentOutOfRangeException if the section ever moves below slot 4, which is a
        // stack trace instead of the message written for exactly that case — and the position
        // assertion that WOULD have said so runs sixty lines later.
        Assert.True(from >= 0 && to > from,
            "the place-word section is no longer above slot 4, so this cannot read it");
        var body = rawPage[from..to];
        var paragraphs = body.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        var management = Array.FindIndex(paragraphs,
            p => p.Contains("What a team often does", StringComparison.Ordinal));
        Assert.True(management >= 0 && management + 1 < paragraphs.Length,
            "the management paragraph is not where this expects it");
        Assert.Contains("\"Most\" is a word about a group", paragraphs[management + 1],
            StringComparison.Ordinal);

        // AND NEITHER COMPARATIVE SURVIVES. Round 3's should-fixes, and both are the
        // "larger series" defect in smaller costumes.
        //
        // "by far the most common" claimed a MARGIN. The margin is real — 89.3% for CSF
        // diversion against 19.4% for resection — and **that is the problem, not the
        // defence**: the only thing that could support the word is the pair of figures this
        // item bars from the page, so in reader text it is a comparative resting on
        // nothing a reader can check. "The most common" is the source's own claim, needs no
        // figures, and is a word shorter. (/review round 4 corrected this note: its first
        // version said the source gives "no rate for the second-most-performed procedure",
        // which is false — the abstract gives 19.4%. A rationale is what gets copied
        // forward, so a conservative edit with a wrong reason is still a wrong reason.)
        //
        // "That study also REPORTS that ... is still an open question" attributed this
        // page's inference to the paper. What the paper does is say two things that
        // disagree about which named category the group belongs to, which is what the page
        // now says it does — an observation about the text, checkable by a reader, rather
        // than a conclusion in the authors' mouths. §12.20: the OBJECT of the verb is the
        // whole difference, not the verb — this page uses "reports" correctly twice, for
        // things that are in the paper.
        Assert.DoesNotContain("by far the most", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("was the most common operation in it", section, StringComparison.Ordinal);
        Assert.Contains("do not agree with each other", section, StringComparison.Ordinal);
        Assert.DoesNotContain("still an open question", Flat, StringComparison.OrdinalIgnoreCase);

        // AND THE ROUTE ROUND 2 ASKED FOR. The section makes a licensed management claim
        // keyed to a location, about the one compartment this page carries a specific
        // warning for: the brain stem entry says "You may have read that nothing can be
        // done here. Do not settle that from a location." A reader of this section reached
        // that only by following #brainstem first.
        Assert.Contains("#can-they-take-it-out", Section(PlaceWordHeading),
            StringComparison.Ordinal);

        // NO THIRD ESCALATION RULE HERE, and no fourth list of red flags: the route
        // goes to the section that carries [ESCALATION].
        Assert.Contains("#the-fluid", Section(PlaceWordHeading), StringComparison.Ordinal);
        foreach (var ownRule in new[] { "911", "ambulance", "emergency room", "right away" })
        {
            Assert.DoesNotContain(ownRule, section, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE SECTION IS `##` ONLY. TheNineRegionsAreThereInTheBlocksOrderWith
        // WrittenAnchors counts `^### … {#anchor}` over the WHOLE page and asserts
        // exactly nine, so one `###` here reddens a REGION guard for a reason that has
        // nothing to do with regions. Asserted where the temptation is rather than left
        // to be discovered by the other test's failure message.
        Assert.DoesNotContain("### ", Section(PlaceWordHeading), StringComparison.Ordinal);

        // POSITION. It sits between the region list and slot 4, so the reader who has
        // just been told their word may not be one of the nine meets it immediately,
        // and it still sits ABOVE the fluid section it routes into — which is why the
        // section says "below" rather than "above".
        var regions = Page.IndexOf($"## {RegionsHeading}", StringComparison.Ordinal);
        var placeWord = Page.IndexOf($"## {PlaceWordHeading}", StringComparison.Ordinal);
        var howLong = Page.IndexOf($"## {HowLongHeading}", StringComparison.Ordinal);
        var fluid = Page.IndexOf($"## {FluidHeading}", StringComparison.Ordinal);
        Assert.True(regions < placeWord && placeWord < howLong && howLong < fluid,
            "the place-word section has moved out from between the region list and "
            + "slot 4, and its own prose says the fluid section is BELOW it");
        Assert.Contains("the fluid section below", section, StringComparison.Ordinal);

        // TWO DOORS, because nothing could go into the short version: it is at the
        // eight-sentence cap TheRoundNineFixesArePinnedAndSlotZeroIsWithinItsContract
        // measures. One in the regions preamble, one at the end of slot 7 — and slot 7's
        // is the one that REDEEMS a promise rather than adding one.
        Assert.Contains("#a-place-for-a-name", Section(RegionsHeading),
            StringComparison.Ordinal);
        Assert.Contains("#a-place-for-a-name", Section(FindingHeading),
            StringComparison.Ordinal);

        // AND THE PROMISE IS REDEEMED, NOT LEFT. Slot 7 used to end "we are still
        // writing about those", which is WI-547's "We are still writing this one" in a
        // different coat — an unwritten-work promise on a shipped page. This item is
        // what redeems it, so the sentence became the route.
        Assert.DoesNotContain("still writing about those", Flat, StringComparison.OrdinalIgnoreCase);

        // AND THE WORD IS CONTAINED, which is the half the retired ban used to carry
        // (/review round 1). "Require the word in the section" is not the same property as
        // "forbid it everywhere else", and nothing was stopping tectal material spreading
        // into the fluid section or the surgery range — where it would be a location-keyed
        // plan claim in the two sections least able to take one. Every occurrence on the
        // page is either the section or the preamble's door, and the door is exactly one.
        // AND ONE BLIND SPOT, RECORDED BECAUSE IT IS EMPTY TODAY RATHER THAN BECAUSE IT IS
        // SAFE (/review round 5). `Flat` is title + description + body, so it does NOT see
        // the SOURCE LIST that `ContentPage.cshtml` renders from the front matter — and this
        // item's source titles carry the word `tectal` in every one of them. Checked: no
        // source title on this page carries any barred string, and `[ESCALATION]` (the one composed
        // block here) carries none either. So the arithmetic below is true of `Flat` and not
        // of every word a reader sees, and the day a source title carries a barred phrase
        // nothing in this file will say so. WI-574 is the item that owns rendered block
        // sources; a page's own rendered source titles belong with it.
        //
        // AND THE SECOND HALF OF THE SAME GAP: `Flat` is not COMPOSED either, so
        // `[ESCALATION]`'s reader prose is outside every bar in this file (/review round 7).
        // Checked: that block's only `tect` and its only "the one place" are both in its FRONT
        // MATTER — a source URL and a source comment — so nothing is live today. Two blind
        // spots, one empty, both written down rather than assumed.
        var total = Regex.Matches(Flat, "tectal", RegexOptions.IgnoreCase).Count;
        var inSection = Regex.Matches(section, "tectal", RegexOptions.IgnoreCase).Count;
        var inPreamble = Regex.Matches(RegionsPreamble(), "tectal", RegexOptions.IgnoreCase).Count;

        Assert.Equal(1, inPreamble);
        Assert.Equal(total, inSection + inPreamble);
    }

    /// <summary>
    /// NO SENTENCE IN THE SECTION IS LONGER THAN 24 WORDS, and this is the one guard in the
    /// item that exists because of a measurement somebody else took.
    ///
    /// <para>§12.17 measured that a planted 36-word sentence moved this page's average from
    /// 5.6 to 5.7 against a 6.0 gate and ContentCheck passed — <b>so the reading-level gate is
    /// a page-average gate by construction, and on a site whose audience may be cognitively
    /// impaired one sentence is enough to lose a reader.</b> WI-571 enforced a per-sentence
    /// ceiling in its own tooling and it earned its keep twice: the first draft carried two
    /// 43-word sentences, and the fix for <c>/review</c> round 1's blocker pushed the longest
    /// back from 22 words to 28. <b>A correctness fix undoing a readability fix is a trade
    /// nobody notices unless the number is re-measured.</b></para>
    ///
    /// <para>It is a test rather than a scratch script because <c>/review</c> round 6 asked
    /// the obvious question: the section that regressed twice had no shipped guard. This is
    /// not the corpus-wide per-sentence check §12.17 hands to <c>/pm</c> — that one needs a
    /// sweep first, because it will not be this page alone. It is one section's ceiling,
    /// measured at 23 when it was written.</para>
    ///
    /// <para>NOT <c>CuratedPage.SentencesOf</c> — and the reason is the STRIPPING, not the
    /// pattern, which this note had backwards. It uses the same <c>(?&lt;=[.!?])\s+</c>; what
    /// makes it work here is removing <c>[*#"]</c> FIRST, because a sentence ending
    /// <c>…it.**</c> does not split on that pattern while the asterisks are still there. That
    /// is a systematic undercount on exactly this section's bold-lead style, and it is the
    /// same trap <see cref="TheRoundNineFixesArePinnedAndSlotZeroIsWithinItsContract"/>
    /// records for slot 0. Links are reduced to their LABELS, because that is the text a
    /// reader reads.</para>
    /// </summary>
    [Fact]
    public void NoSentenceInTheSectionRunsPastTheCeilingAPageAverageCannotSee()
    {
        var prose = Regex.Replace(FlatSection(PlaceWordHeading), @"\[([^\]]*)\]\([^)]*\)", "$1");
        prose = Regex.Replace(prose, "[*#\"]", "");

        var sentences = Regex.Split(prose, @"(?<=[.!?])\s+")
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        // A FLOOR ON WHAT IT READ, because §12.17's rule for an iterate-and-check guard is
        // that it must say out loud how much it looked at. 40 against a measured 45, not 30:
        // /review round 7 pointed out that a third of the section could go before a floor of
        // 30 spoke.
        Assert.True(sentences.Count >= 40,
            $"only {sentences.Count} sentences were found in the section, so either it has "
            + "been gutted or this split is reading the wrong thing");

        // RemoveEmptyEntries, to agree with the tooling that measured 23. `Split(' ')` counts
        // a double space as a word, and with one word of headroom under the ceiling a
        // cosmetic edit that leaves two spaces in one sentence would red this with a message
        // blaming sentence length — the false-cause failure round 5 rewrote the adjacency
        // assertion to escape (/review round 7).
        var over = sentences
            .Where(s => s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length > 24)
            .ToList();
        Assert.True(over.Count == 0,
            "a sentence in this section has run past 24 words, and the reading-grade gate "
            + "cannot see it: §12.17 measured a planted 36-word sentence moving this page "
            + "5.6 -> 5.7 against a 6.0 limit with ContentCheck passing. Split it.\n  "
            + string.Join("\n  ", over.Select(s =>
                $"{s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length}: {s}")));
    }

    /// <summary>
    /// THE GUARD CAN FIRE IN HERE, WHICH IS THE ONLY THING THAT MAKES ITS SILENCE WORTH
    /// ANYTHING (/review round 1, §12.19 finding 6).
    ///
    /// <para>The section carries <b>zero</b> difficulty-lexicon matches, so
    /// <see cref="NoPlaceIsRankedAgainstAnotherAnywhereOnThePage"/> and
    /// <c>LocationObligationSweepTests</c> are both green over it for a structural reason
    /// rather than an editorial one. §12.19: <i>"a guard measured on a page it cannot fire
    /// on has been measured on nothing"</i> — and that zero had already been quoted
    /// forward twice as though it were evidence. Both halves are asserted here: the zero,
    /// and that a planted row IS found, so the day this section acquires difficulty
    /// vocabulary the guard is standing over it rather than arriving green.</para>
    /// </summary>
    [Fact]
    public void TheRankingGuardCanActuallyFireInsideTheNewSection()
    {
        var section = FlatSection(PlaceWordHeading);

        // THE ADDRESSES ARE REAL — the section names the brain stem, the midbrain and the
        // ventricles — so the address half of the guard is live here.
        var addresses = Regex.Matches(section, CuratedPage.LocationAddress,
            RegexOptions.IgnoreCase).Count;
        Assert.True(addresses >= 3,
            $"the section matched {addresses} address words, so a planted row could not "
            + "fire here for lack of a place rather than for lack of a difficulty");

        // AND THE DIFFICULTY HALF IS AT ZERO, which is the fact the docstring used to
        // mis-state as a measurement. Not a ban: if a later edit needs difficulty
        // vocabulary here it should say so by changing this number and writing the reason,
        // which is precisely the conversation a silent green never starts.
        var difficulties = Regex.Matches(section, CuratedPage.LocationDifficulty,
            RegexOptions.IgnoreCase).Count;
        Assert.True(difficulties == 0,
            $"the section now carries {difficulties} difficulty-lexicon match(es), so the "
            + "ranking guard's silence over it has stopped being structural. That is not "
            + "automatically wrong — but the row test has to be re-asked of the sentence "
            + "that brought the vocabulary in, and §12.20's account of why the guard is "
            + "green here has to change with it.");

        // THE PLANTED ROW, which is what turns "no row was found" into "no row is there".
        // Both halves in one sentence, in the section's own register.
        const string planted =
            " A tumor in the back part of the midbrain is harder to take out completely.";
        var withRow = CuratedPage.PlacesRankedIn(section + planted);

        Assert.Contains(withRow, r =>
            r.Trigger.Contains("back part of the midbrain is harder", StringComparison.Ordinal));

        // And nothing was found without it, so the assertion above is about the plant.
        Assert.Empty(CuratedPage.PlacesRankedIn(section));
    }

    /// <summary>
    /// EVERYTHING THE ITEM WAS FORBIDDEN TO PUBLISH, AND THE BARRED WORDING WAS IN ITS
    /// OWN SOURCES RATHER THAN ONLY IN THE HOSPITAL PAGES THE BACKLOG WARNED ABOUT.
    ///
    /// <para>The backlog barred Dana-Farber and Boston Children's cure-rate and
    /// "excellent prognosis" language. <b>All FOUR of this item's sources carry the same
    /// shape</b>: Imoto writes <i>"tends to have a good prognosis"</i>, the systematic
    /// review's abstract opens <i>"generally have a benign clinical course"</i>, the third
    /// source concludes <i>"lends to excellent long-term survival"</i>, and the fourth — the
    /// Childs Nerv Syst review this item spent nine rounds recording as unreadable — opens
    /// <i>"generally benign neoplastic lesions"</i>. (This docstring said "both of this
    /// item's usable sources" until <c>/review</c> round 10, and the homes that said "all
    /// three" were still wrong, because the fourth source arrived in round 10's own fix.
    /// Round 11 found the last of them — in §12.20. <b>Adding a source means re-counting
    /// every sentence that counts them</b>, and the sentence most likely to be missed is the
    /// one in the ruling.) <b>So the bar has to be a property, not a blocklist of two
    /// publishers</b> — §12.14's ruling, which is that banning a citation is not fixing
    /// a claim.</para>
    ///
    /// <para><b>AND THE FIGURES ARE BARRED BY THEIR OWN SPREAD.</b> The review reports
    /// resection <i>"ranging from 2.3 to 100.0%"</i> across studies for one tumor. The
    /// page publishes that qualitatively and routes to <c>#no-list</c>, which already
    /// owns the argument — §12.10: route, do not restate.</para>
    /// </summary>
    [Fact]
    public void TheSectionPublishesNoFigureNoPrognosisAndNoClassification()
    {
        var section = FlatSection(PlaceWordHeading);

        // NO DIGITS AT ALL in this section. Wider than the rule needs and deliberately
        // so: the source's numbers are a percentage, a patient count and a study count,
        // and there is no number in that review this page has a use for. (It reads
        // FlatSection, which keeps URLs, so a future link with a digit in its path would
        // red this. That is the fail-loud direction and the page-wide check below is what
        // carries the actual property.)
        Assert.DoesNotMatch(@"\d", section);

        // AND THE FIGURES THEMSELVES ARE BARRED PAGE-WIDE, which is /review round 1's
        // seventh finding: the check above is scoped to where the defect WOULD be, and
        // §12.17's most-repeated failure is that a guard scoped to the paragraph is green
        // where it arrives. `Flat` includes the title and the front-matter description —
        // the description being exactly where a blocker survived three rounds on this page
        // — and each of these strings is unique enough in this corpus to cost nothing.
        // AND THE PAGE-WIDE HALF IS NOT ASSERTED HERE, BECAUSE IT ALREADY WAS — which is
        // /review round 9's finding and the cleanest instance of §12.18's rule this item
        // produced. NoNumberOnThisPageCouldBeMistakenForARiskOrAnOutcome has asserted "no
        // digit in Flat except 911" since WI-567's round 4. Round 4 of THIS item guarded the
        // same absolute with nine figure strings, round 7 added five more, and round 8
        // replaced the list with a fresh digit check while arguing at
        // length that a subset cannot carry an absolute. **All three were rebuilding a guard
        // that was 900 lines up the same file.** §12.18: when two guards test one property,
        // the newer one is not automatically the stronger one — and this one had the worse
        // message and the worse home.
        //
        // (Round 8 also justified the replacement by claiming round 7's `8.4` entry was "not
        // a figure in any source at all, only a substring of `18.4`". Both halves are false:
        // `8.4%` is the review's RADIOTHERAPY rate, in the same abstract sentence this file
        // quotes twice for 19.4% and 65.4%, and `18.4` is a real value in the third source's
        // tables. Round 7 was right to list it; the list was replaced for a different and
        // correct reason — a subset cannot carry an absolute, which the front matter's own
        // 24% and 25% figures prove. **A correct conclusion reached through a false premise
        // is what gets copied forward.** /review round 10.)
        //
        // The section-scoped `\d` check above stays, because the SECTION is this item's
        // subject and its zero is what the reader-facing sentence is about locally.

        // NO PROGNOSIS, in the wording the sources actually use — AND PAGE-WIDE, not
        // section-wide. /review round 4: this list and the size list below were scoped to
        // `section` while the figure and classification lists in this same method were on
        // `Flat`, in a test whose own comment quotes §12.17's most-repeated failure — a
        // guard scoped to the paragraph the defect was in is green where it arrives. The
        // fluid and surgery sections are exactly where prognosis language would land, and
        // `Flat` carries the front-matter description, which §12.17 records as the line a
        // blocker survived three rounds inside. This list and the size list below — twelve
        // strings in all — each measured at zero over the whole page's reader text before the
        // widening, so it costs nothing today. (Round 11: "all twelve" sat above a nine-entry
        // loop, which reads as a miscount until you find the other three in the size loop
        // further down this same method. Round 15: and "three tests down" was both a repeated
        // word and the wrong place. **A cross-reference written as a DISTANCE is a count in
        // prose about something the file enumerates, and it rots on the next insertion** —
        // four of them in this diff were wrong. Name the thing.)
        // AND EACH ONE CARRIES ITS OWN MESSAGE, because /review round 5 pointed out what a
        // bare DoesNotContain costs the next person: `outlook` is live and correct on 23
        // other files and `benign` on 10, INCLUDING /tumors/all-brain-tumors' own
        // `## Why "benign" is the wrong comfort word here {#benign}`. `Flat` keeps URLs, so
        // the first legitimate route from this page to that anchor trips this loop with no
        // explanation of what to do about it. The answer is to exclude link targets, not to
        // delete the ban — §12.8: a ban that would fail a correct sentence is worse than no
        // ban, and the fix for that is a narrower scope rather than a shorter list.
        foreach (var barred in new[]
        {
            "good prognosis", "benign", "excellent", "cure rate", "survival",
            "prognosis", "outlook", "low-grade", "more aggressive",
        })
        {
            Assert.True(!Flat.Contains(barred, StringComparison.OrdinalIgnoreCase),
                $"`{barred}` has appeared in this page's reader text. Every one of these was "
                + "measured at zero page-wide when the ban was widened from the section to "
                + "the page (/review round 4). If what has arrived is a legitimate ROUTE, "
                + "strip link targets from the text this loop reads rather than dropping the "
                + "word: `Flat` keeps URLs, `/tumors/low-grade-glioma` is a live slug, and "
                + "/tumors/all-brain-tumors carries both `#benign` and an outlook gate.");
        }

        // NO CLASSIFICATION SENTENCE. The claim the backlog flagged as unverified is
        // 10.1007/s00401-026-03066-7, which was CHASED and cannot be read: paywalled
        // with no abstract, and Europe PMC has it as not-open-access, not-in-EPMC, no
        // PMCID, null abstract. §12.17's Moffitt rule — a source we cannot open live is
        // a source we cannot verify — so nothing rests on it and nothing quotes it.
        foreach (var unverified in new[]
        {
            "MAPK", "pathway-altered", "pilocytic", "CNS5", "grade 1", "grade 2",
        })
        {
            Assert.DoesNotContain(unverified, Flat, StringComparison.OrdinalIgnoreCase);
        }

        // `WHO` IS CASE-SENSITIVE, AND THAT IS NOT FUSSINESS. Added to the list above it
        // failed immediately, on slot 9's own heading — "Who decides, and how will you
        // hear?" — which is §12.8's rule arriving the moment the ban was written: a ban
        // list that forbids the correct shape is worse than no ban list. The
        // classification body's acronym is upper case in every source that prints it, and
        // the page uses the pronoun. (An earlier version of this note said "four times".
        // /review round 2 counted: eight word-occurrences, one of them capitalised, in a
        // note that had no business carrying a number at all.)
        Assert.DoesNotContain("WHO", Flat, StringComparison.Ordinal);

        // AND THE PAPER IS RECORDED AS CHASED, with its identifiers, so the next item
        // does not start the chase again — §12.17's rule that an absence nobody wrote
        // down gets re-investigated.
        var front = CuratedPage.FrontMatter(Page);
        Assert.Contains("10.1007/s00401-026-03066-7", front, StringComparison.Ordinal);
        Assert.Contains("PMID 42622715", front, StringComparison.Ordinal);
        Assert.Contains("CHASED AND UNREADABLE", front, StringComparison.Ordinal);

        // NO SIZE RULE. The review says resection "should be reserved for large tumors".
        // This page refused a size claim twice already (§12.17) in the OPPOSITE
        // direction, and a reader who measures themselves against "large" has been
        // handed a rule nobody meant them to apply.
        // PAGE-WIDE for the same reason as the prognosis list above, and with more force
        // here: this page already refused a size claim twice in the OPPOSITE direction, and
        // both refusals are recorded against the FLUID section, not against this one.
        foreach (var size in new[] { "large tumor", "big enough", "if it is large" })
        {
            Assert.DoesNotContain(size, Flat, StringComparison.OrdinalIgnoreCase);
        }

        // AND THE REASON NO FIGURE IS PRINTED IS PRINTED, qualitatively, and routed to
        // the section that already owns the argument.
        //
        // "ALMOST NEVER TO ALWAYS", NOT "TO NEARLY ALWAYS" (/review round 1). The review's
        // range is "2.3 to 100.0%", and 100.0% is ALWAYS. Understating the top of it
        // weakens the item's own argument, because the WIDTH of that spread is the whole
        // reason no number is published. And the range is across STUDIES — "between
        // individual studies", 14 of them — not across hospitals, which the first draft
        // said and no source supports.
        Assert.Contains("from almost never to always", section, StringComparison.Ordinal);
        Assert.Contains("which study was counted", section, StringComparison.Ordinal);
        Assert.DoesNotContain("nearly always", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("which hospital was counted", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#no-list", Section(PlaceWordHeading), StringComparison.Ordinal);

        // AND THE ROUTE TO `#no-list` CLAIMS A RELATIONSHIP, NOT AN IDENTITY (/review round
        // 3). It said the resection spread "is [the reason there is no list here], in a
        // single line" — and the destination's reason is a different argument: a survey in
        // which neurosurgeons disagreed about which parts of the brain matter, and the
        // conclusion that a table would publish one group's opinion as a fact. Cousins, not
        // the same claim, and §12.19 records six failures of exactly this shape. A route may
        // say the destination goes further; it may not say the two arguments are one.
        // "THE LONGER FORM OF THE SAME ARGUMENT" WAS STILL THE IDENTITY CLAIM, one round
        // later (/review round 4): the destination's case is a survey in which neurosurgeons
        // disagreed about which parts of the brain matter, and this section's is one review's
        // resection rate running across studies. Same SHAPE, different evidence, different
        // conclusion. "The same kind of case" is what a route may say.
        Assert.Contains("makes the same kind of case about the whole page", section,
            StringComparison.Ordinal);
        Assert.DoesNotContain("in a single line", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("longer form of the same argument", Flat,
            StringComparison.OrdinalIgnoreCase);

        // AND "figure" BECAME "number", on the page whose thesis is that a picture cannot
        // name a growth — the one word in that sentence a reader could take for "picture".
        Assert.Contains("No number from any of those papers", section, StringComparison.Ordinal);
        Assert.DoesNotContain("No figure from", Flat, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// THE "SEARCH ALIAS" HALF OF THE ITEM NEEDED NO MECHANISM, AND THIS IS THE
    /// MEASUREMENT RATHER THAN THE CLAIM.
    ///
    /// <para><c>ContentStore.SearchPages</c> (WI-309) scores every file under
    /// <c>Content/pages</c> as <c>title.Contains(term) ? 5 : 0</c> plus
    /// <c>body.Contains(term) ? 1 : 0</c>, where <c>body</c> is <c>page.Markdown</c> —
    /// which <c>Parse</c> sets to the COMPOSED BODY, after the front matter has been
    /// sliced off. So the word in the PROSE is the whole mechanism, and there is no
    /// field to add.</para>
    ///
    /// <para><b>BOTH CANDIDATE ALIAS HOMES ARE REFUSED, ON THEIR MERITS RATHER THAN
    /// BECAUSE THE WORD "alias" MATCHED.</b></para>
    ///
    /// <list type="bullet">
    ///   <item><b><c>taxonomy.yml</c>'s <c>also</c></b> — refused, and that file's own
    ///     header is the reason: aliases are what <i>"the classifier may be given; they
    ///     never render"</i> and must be <i>"true synonyms, never 'close enough'"</i>. A
    ///     location is not a synonym of a type, and §12.2 item 3 requires WHO CNS5
    ///     naming throughout, so this would put a place into the closed list of
    ///     diagnoses the classifier may emit.</item>
    ///   <item><b>The glossary's <c>also</c></b> — a REAL candidate, and it gets a real
    ///     answer rather than a reflex: unlike the taxonomy it does render
    ///     (<c>Glossary.cshtml</c>, <i>"Also called:"</i>) and it does feed tooltips
    ///     (<c>GlossaryTooltips</c> adds <c>term.Aliases</c> to the trigger names).
    ///     Refused by WI-519's rule — <b>an entry defined and used in ONE place fires
    ///     nowhere.</b> This page defines the word and no other page says it, which is
    ///     the same refusal WI-567 wrote for <c>temporal</c>, <c>parietal</c> and
    ///     <c>occipital lobe</c>. It would not buy the search half anything either:
    ///     <c>/glossary</c> is a Razor page, not a curated page under
    ///     <c>Content/pages</c>, so <c>SearchPages</c> never enumerates it.</item>
    /// </list>
    ///
    /// <para><b>AND THE TRIGGER FOR REVISITING IS DERIVED, NOT REMEMBERED.</b> The
    /// second-use count is read off the corpus here rather than written into a comment
    /// somebody has to re-check — §12.19's lesson, which cost four review rounds: never
    /// write a count in prose about something the code can enumerate. The day a second
    /// page says the word, this goes red and the entry gets written.</para>
    /// </summary>
    [Fact]
    public void NeitherAliasHomeWasReachedForAndTheSearchHalfNeededNoMechanism()
    {
        var root = Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content");

        // A LOCATION DOES NOT GO INTO A LIST OF DIAGNOSES. This is the item's central
        // refusal and the one most likely to be undone by somebody who reads the
        // backlog's word "alias" and reaches for the nearest field called that.
        Assert.DoesNotContain("tectal",
            File.ReadAllText(Path.Combine(root, "taxonomy.yml")),
            StringComparison.OrdinalIgnoreCase);

        // NO GLOSSARY ENTRY, and the reason is measured rather than asserted: the word
        // appears in the reader text of exactly ONE page, which is the page that defines
        // it, so an entry would be suppressed there and fire nowhere.
        //
        // AND IT GOES THROUGH THE REAL PARSER, which took three rounds to arrive at.
        //
        // Round 1 checked two guessed filenames, `tectal-glioma.md` and `tectal.md` — which
        // an entry filed under any other slug walks straight past. Round 2 replaced that
        // with a regex over `^also:` lines, and round 3 found TWO holes in it, both the
        // shape round 2 had just closed:
        //
        //   * `term:` was never scanned, and it is a trigger too — `GlossaryTooltips`
        //     builds its matcher names from `term.Term` FIRST and then `term.Aliases`. An
        //     entry with `term: tectal glioma` and no `also:` line fires tooltips
        //     corpus-wide and the regex stayed green, while the failure message claimed to
        //     cover "by slug or by alias".
        //   * `also:` written as a YAML block sequence walks past a line regex. All 66
        //     `also:` lines in the corpus are inline flow style today, so the hole was
        //     unexercised — which is precisely the state the two guessed filenames were in
        //     before round 2 looked at them.
        //
        // **A guard on a parsed field should use the parser.** `GlossaryStore.ParseTerm`
        // is what the site loads these files with, so a scan through it cannot diverge from
        // what actually fires — the same reason `CuratedPage.Composed` uses the real
        // composer rather than re-implementing `[BLOCK]`.
        var glossaryFiles = Directory.EnumerateFiles(
            Path.Combine(root, "glossary"), "*.md").ToList();
        Assert.True(glossaryFiles.Count > 90,
            $"only {glossaryFiles.Count} glossary files, so this scan is reading almost "
            + "nothing");

        var claiming = glossaryFiles
            .Select(f => (File: Path.GetFileName(f)!,
                          Term: GlossaryStore.ParseTerm(
                              File.ReadAllText(f), Path.GetFileNameWithoutExtension(f))))
            .Where(t => t.File.Contains("tect", StringComparison.OrdinalIgnoreCase)
                     || t.Term.Term.Contains("tect", StringComparison.OrdinalIgnoreCase)
                     || t.Term.Aliases.Any(a =>
                            a.Contains("tect", StringComparison.OrdinalIgnoreCase)))
            .Select(t => t.File)
            .ToList();

        Assert.True(claiming.Count == 0,
            "a glossary entry now claims this word — by slug, by `term` or by alias, all "
            + "three of which fire tooltips — and WI-519's refusal was written on the "
            + "measurement that no second page says it: " + string.Join(", ", claiming));

        var elsewhere = Directory
            .EnumerateFiles(Path.Combine(root, "pages"), "*.md", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "blocks"), "*.md"))
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "glossary"), "*.md"))
            .Where(f => Path.GetFileName(f) != $"{Slug}.md")
            .Where(f => CuratedPage.Flatten(CuratedPage.ReaderText(File.ReadAllText(f)))
                .Contains("tectal", StringComparison.OrdinalIgnoreCase))
            .Select(f => Path.GetFileName(f))
            .ToList();

        Assert.True(elsewhere.Count == 0,
            "a second page now says `tectal` in READER text, so WI-519's refusal has "
            + "expired: a glossary entry would fire somewhere other than the page that "
            + "defines the word, and it should be written. Files: "
            + string.Join(", ", elsewhere));

        // AND THE WORD IS WHERE SEARCH CAN SEE IT — in the BODY, not only in a source
        // comment. The two failure modes coincide for once: what search cannot see, a
        // reader cannot either.
        Assert.Contains("tectal", CuratedPage.Body(Page), StringComparison.OrdinalIgnoreCase);
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

    /// <summary>
    /// WI-571's SECOND half, end to end through the reader's own route: the word a
    /// reader was actually handed has to find something when they type it in.
    ///
    /// <para><b>THIS IS THE WHOLE "SEARCH ALIAS" DELIVERABLE, AND IT IS A TEST RATHER
    /// THAN A MECHANISM.</b> <c>ContentStore.SearchPages</c> scores the composed body,
    /// so the word in the PROSE is what makes the page findable — no field, no index, no
    /// alias table. Asserting it through <c>/search</c> rather than against
    /// <c>SearchPages</c> directly is deliberate: the unit call being right and the page
    /// not rendering the hit is a failure a reader would meet and a unit test would
    /// not.</para>
    ///
    /// <para><b>AND THE TWO-TERM CASE IS THE ONE WORTH MEASURING.</b> Scoring is
    /// title×5 + body×1 per term, so a search for <i>"tectal glioma"</i> puts every page
    /// with <i>glioma</i> in its TITLE (6: five for the title term, one for the body) above
    /// this one, which scores 2 and has neither word in its title. That is fine and it is
    /// why the assertion is membership in the results
    /// rather than first place — but it is only fine while the page limit is comfortably
    /// above the number of glioma-titled pages, which
    /// <see cref="TheResultLimitLeavesRoomForAPageThatOnlyMatchesInItsBody"/> asserts and
    /// explains. (An earlier version of this paragraph added that if <c>PageLimit</c>
    /// dropped "nothing else in the suite would say so", which is false — THIS test is
    /// what would say so — and it sat thirty lines above the note recording that
    /// correction. <c>/review</c> round 2: two adjacent docstrings making opposite
    /// claims.)</para>
    /// </summary>
    [Theory]
    [InlineData("tectal")]
    [InlineData("tectal glioma")]
    [InlineData("Tectal Glioma")]
    public async Task TheWordAReaderWasHandedFindsThePage(string query)
    {
        var html = await _factory.CreateClient()
            .GetStringAsync($"/search?q={Uri.EscapeDataString(query)}");

        Assert.Contains("<h2>Pages</h2>", html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{Url}\"", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// THE OTHER TWO HALVES OF §12.20's SEARCH RULING, WHICH WERE PROSE AND ARE NOW
    /// ASSERTIONS (<c>/review</c> round 6).
    ///
    /// <para>§12.20 states three consequences of <c>SearchPages</c> scoring
    /// <c>page.Markdown</c> — the composed BODY — and claimed all three were asserted. One
    /// was: the test above. <b>The other two were asserted nowhere</b>, in a ruling whose own
    /// subject is that a claim in prose beside a claim in code is the defect. So:</para>
    ///
    /// <list type="bullet">
    ///   <item><b>A word in a front-matter source COMMENT is invisible to search.</b>
    ///     <c>Tauziede-Espariat</c> — the unreadable Acta paper's first author, named only in
    ///     the comment recording that it was chased — occurs exactly once in the whole
    ///     corpus. <b>The witness was <c>Igboechi</c> until <c>/review</c> round 11</b>, and
    ///     the reason it had to change is the point of the bullet: round 10 turned that
    ///     source into a real citation, so the word moved from a comment into a
    ///     <c>title:</c> — which <c>ContentPage.cshtml</c> RENDERS into the reader's source
    ///     list. It stayed invisible to search and became visible to the reader, so the two
    ///     failure modes stopped coinciding and the test passed for a different reason than
    ///     its name. If search ever finds this one, <c>Parse</c> has stopped slicing the
    ///     front matter off and every
    ///     "NOT USED" and "REFUSED" note in the corpus has become reader-facing search bait.
    ///     <b>That is the same mechanism that makes the word in the PROSE the whole
    ///     alias</b>, seen from the other side.</item>
    ///   <item><b><c>/glossary</c> is not searched at all.</b> <c>astrocytes</c> occurs in one
    ///     glossary file and in no page or block body, so it must return no PAGE hit. This is
    ///     half of why a glossary entry could not have been the "search alias" the backlog
    ///     asked for, and it was the half resting on a reading of <c>ContentStore.Root</c>
    ///     rather than on a run.</item>
    /// </list>
    ///
    /// <para><b>AND THE FIRST ATTEMPT AT THAT SECOND CASE FAILED, WHICH IS A FINDING ABOUT
    /// THE SEARCH AND NOT ABOUT THE TEST.</b> It used <c>status epilepticus</c> — glossary-only
    /// as a phrase — and the page list came back non-empty. <c>SearchPages</c> splits the
    /// query on whitespace and scores <b>per term</b>, so <i>status</i> alone matches page
    /// bodies: <b>a multi-word query is not a phrase query.</b> That is the same mechanism
    /// that makes <i>"tectal glioma"</i> rank every glioma-TITLED page above this one, seen
    /// from the other side, and it is why the negative case has to be a single word.</para>
    /// </summary>
    [Theory]
    // THE UNDIACRITICKED SPELLING, DELIBERATELY, and the reason is the witness's whole point:
    // the front matter and the backlog write "Tauziede-Espariat" because this corpus keeps its
    // source comments ASCII, while §12.20 writes "Tauziède-Espariat", which is the author's
    // actual name. /review round 18: if a later edit makes the front matter agree with the
    // ruling, this InlineData reds with a message about SEARCH VISIBILITY — the "witness stops
    // witnessing" failure arriving from the other side. The assertion that fires is
    // `Assert.Contains(query, front)`, so the failure is loud; this comment is what tells the
    // next person it is a spelling and not a search bug.
    [InlineData("Tauziede-Espariat", true)]
    [InlineData("astrocytes", false)]
    public async Task WhatSearchCannotSeeIsAssertedRatherThanRead(string query, bool inFrontMatter)
    {
        var html = await _factory.CreateClient()
            .GetStringAsync($"/search?q={Uri.EscapeDataString(query)}");

        // The feed may legitimately return research items for either term; the claim is
        // about the CURATED PAGE list, which the view only renders when it is non-empty.
        Assert.DoesNotContain("<h2>Pages</h2>", html, StringComparison.Ordinal);

        // AND THE WITNESS'S OWN PROPERTY IS ASSERTED, NOT DESCRIBED (/review round 12). The
        // previous witness, `Igboechi`, silently stopped witnessing when round 10 promoted it
        // from a comment into a source `title:` — which ContentPage.cshtml RENDERS — so the
        // test kept passing while the thing it was named for had changed. **Changing the
        // witness without pinning the property leaves the next promotion equally silent.**
        // UNCONDITIONALLY, which is /review round 13's correction. The first version gated the
        // three assertions on `front.Contains(query)` — so DELETING the witness from the front
        // matter skipped them all and left `DoesNotContain("<h2>Pages</h2>")` passing trivially
        // over a word that is nowhere in the corpus. **The witness would stop witnessing exactly
        // as silently as `Igboechi` did**, which is the failure this whole test was rewritten
        // for. §12.20: a conditional guard and a loosely-scoped one can be green about the same
        // deletion; assert both halves unconditionally.
        var page = CuratedPage.Read("where-your-tumor-is.md");
        var front = CuratedPage.FrontMatter(page);

        if (inFrontMatter)
        {
            Assert.Contains(query, front, StringComparison.Ordinal);
            Assert.DoesNotContain(query, CuratedPage.Flatten(CuratedPage.ReaderText(page)),
                StringComparison.Ordinal);
            Assert.DoesNotMatch($@"(?m)^\s*title: .*{Regex.Escape(query)}", front);
        }
        else
        {
            // The glossary witness must NOT be on this page at all, in any form — otherwise it
            // is testing this page's front matter rather than the glossary's invisibility.
            Assert.DoesNotContain(query, page, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(query, front, StringComparison.OrdinalIgnoreCase);

            // AND ITS HOME IS ASSERTED, NOT DESCRIBED (/review round 14). The docstring said
            // this witness "occurs in one glossary file and in no page or block body", and
            // nothing checked it — so renaming or deleting that glossary term would leave
            // `DoesNotContain("<h2>Pages</h2>")` green over a word that is nowhere in the
            // corpus. **That is the `Igboechi` failure on the other branch**: round 13 closed
            // it for the front-matter witness and left this one described.
            var content = Path.Combine(
                CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content");
            var inGlossary = Directory
                .EnumerateFiles(Path.Combine(content, "glossary"), "*.md")
                .Where(f => File.ReadAllText(f).Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.Single(inGlossary);

            var elsewhere = Directory
                .EnumerateFiles(Path.Combine(content, "pages"), "*.md", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(Path.Combine(content, "blocks"), "*.md"))
                .Where(f => CuratedPage.Flatten(CuratedPage.ReaderText(File.ReadAllText(f)))
                    .Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName)
                .ToList();
            Assert.True(elsewhere.Count == 0,
                $"`{query}` is the witness for \"the glossary is not searched\", and it has "
                + "appeared in a page or block body — so a PAGE hit would now be correct and "
                + "this test would be asserting the opposite of its name. Files: "
                + string.Join(", ", elsewhere));
        }
    }

    /// <summary>
    /// The headroom the assertion above depends on, measured rather than assumed. A
    /// query for <i>"tectal glioma"</i> scores every glioma-TITLED page at <b>6</b> — five
    /// for the title term plus one for the body, since each of them carries the word in its
    /// body too — and this page at 2, so it only appears at all while the result limit
    /// exceeds the number of pages that beat it. (This said 5 until <c>/review</c> round 7,
    /// seventy lines from the docstring round 6 had already corrected. Two copies of one
    /// number inside one file.)
    /// </summary>
    [Fact]
    public void TheResultLimitLeavesRoomForAPageThatOnlyMatchesInItsBody()
    {
        var pages = Directory
            .EnumerateFiles(
                Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages"),
                "*.md", SearchOption.AllDirectories)
            .Count(f => Regex.IsMatch(
                File.ReadAllText(f), @"(?m)^title: .*glioma", RegexOptions.IgnoreCase));

        // SearchModel.PageLimit is 10, duplicated here because it is `private const`.
        // A strictly-greater comparison, because a tie means this page is the one cut off.
        //
        // AND THIS TEST IS A BETTER MESSAGE, NOT A SECOND SAFETY NET — /review round 1
        // corrected the first version of this note, which claimed that if PageLimit
        // dropped "nothing else in the suite would say so". The [Theory] above WOULD say
        // so: it asks for "tectal glioma" through the real page, so a PageLimit of five
        // fails it directly. What this adds is a failure that names the cause, because the
        // Theory's own failure looks like the section went missing.
        Assert.True(pages < 10,
            $"{pages} curated pages have `glioma` in their TITLE, so each of them scores "
            + "above /where-your-tumor-is for the query \"tectal glioma\", and "
            + "SearchModel.PageLimit is 10. The reader who types the word they were "
            + "handed no longer sees the page written for them — raise PageLimit, or "
            + "weight body matches, before adding another glioma-titled page.");
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
