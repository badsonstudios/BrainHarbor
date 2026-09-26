using System.Text.RegularExpressions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-570. §12.18 settled how ONE tumor-type page carries location. This is that
/// ruling asserted across the whole tumor corpus, and §12.15's rule is the shape
/// of it: <b>an obligation may be answered, re-headed or routed, but it may never
/// be dropped</b> — so for every hub the test says WHICH, and a hub with no entry
/// fails rather than being skipped.
///
/// <para><b>THE SWEEP CAME BEFORE THE SECTION, AND IT CHANGED WHAT THE ITEM WAS.</b>
/// WI-570 was commissioned to write a location section on nine hubs. Running
/// §12.18's property over all twenty-three first found the nine were already
/// carrying their own location prose, and found instead:</para>
/// <list type="number">
///   <item><b>Two ranked pairs on one page</b>, both on <c>/tumors/ependymoma</c>
///     and neither in a location section. The sweep found the outlook gate's "Tumors
///     in the spinal cord in adults tend to do better than tumors at the back of the
///     brain in young children." <c>/review</c> found the second, in "If it comes
///     back": "After a tumor at the back of the brain, it tends to come back in the
///     same place. After one higher up in the brain, it more often turns up somewhere
///     else" — and <b>the guard could only ever see half of that one</b>, because the
///     second sentence carries no difficulty word at all. Both deleted, both recorded
///     in the page's front matter, both pinned gone.</item>
///   <item><b>A dropped obligation on five hubs</b>, and this is the finding the
///     item turns on. The route to <c>/where-your-tumor-is</c> lives in
///     <c>blocks/mechanism.md</c>'s closing paragraph — so eighteen hubs have it
///     and the five that do NOT compose that block had no route to the location
///     page at all. §12.10's blast radius running the other way: a block being the
///     only home for an obligation is silent on every page that does not include
///     it, because no single page is individually wrong.</item>
/// </list>
///
/// <para><b>WHY THIS IS NOT IN <see cref="TumorHubTemplateSweepTests"/>.</b> That
/// sweep asserts §12.3's seventeen SECTIONS. This asserts one property that cuts
/// across them — the ependymoma defect was in section 12 and the five missing
/// routes are in section 3. Folding a cross-cutting property into a
/// per-heading table would have hidden the first one.</para>
/// </summary>
public sealed class LocationObligationSweepTests
{
    /// <summary>How the location material reaches this hub's reader.</summary>
    private enum Source
    {
        /// <summary>Composed in from <c>blocks/mechanism.md</c> — eighteen hubs.</summary>
        SharedBlock,

        /// <summary>Written on the page, because the page does not compose the block.</summary>
        ItsOwn,
    }

    private static string PagesDir =>
        Path.Combine(CuratedPage.PagesDirectory, "tumors");

    private static string Raw(string slug) => CuratedPage.Read("tumors", slug + ".md");

    /// <summary>
    /// The page as a reader meets it: COMPOSED, so a shared block's prose is in
    /// scope (§12.18 — a guard that reads the entry rather than the composed page
    /// sent a sphenoid-wing reader into the pituitary bullet); with the title and
    /// description on the front, because the description renders as the first
    /// paragraph and nothing else grades it (WI-575); flattened, because the source
    /// is hard-wrapped; and with emphasis stripped, because a bolded word plus a
    /// line wrap defeats a phrase guard structurally.
    /// </summary>
    private static string Plain(string slug) => PlainOfPage("tumors", slug + ".md");

    /// <summary>
    /// <inheritdoc cref="Plain(string)" path="/summary/node()"/>
    ///
    /// <para>NOT an overload of <see cref="Plain(string)"/>, and the first draft
    /// made it one. A <c>params string[]</c> overload and a <c>string</c> overload
    /// are ambiguous at exactly one call site — the single-argument one — and C#
    /// silently picks the non-params candidate, so
    /// <c>Plain("where-your-tumor-is.md")</c> went looking for
    /// <c>pages/tumors/where-your-tumor-is.md.md</c>. It failed loudly here; a
    /// resolution that had happened to find a file would not have.</para>
    /// </summary>
    private static string PlainOfPage(params string[] pathUnderPages)
    {
        var raw = CuratedPage.Read(pathUnderPages);
        var front = CuratedPage.FrontMatter(raw);
        var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
        var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
        var where = string.Join("/", pathUnderPages);

        // BOTH asserted, not just the title. The description is in here because it
        // renders as the first paragraph a reader meets and nothing else grades it
        // (WI-575) — so a description the regex cannot read (a folded YAML scalar,
        // say) would silently shrink every guard below while the title kept them
        // green. /review: assert the thing you added the field for.
        Assert.False(string.IsNullOrWhiteSpace(title),
            $"{where}: the title could not be read, so every guard below ran on less "
            + "text than it claims to");
        Assert.False(string.IsNullOrWhiteSpace(description),
            $"{where}: the description could not be read as a single quoted line, so the "
            + "one piece of reader-facing prose no other guard can see is not in scope here "
            + "either");

        var body = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Composed(raw)));
        return Regex.Replace($"{title} {description} {body}", @"[*_]", "");
    }

    /// <summary>
    /// The page's OWN location section — uncomposed — flattened and unemphasised.
    ///
    /// <para><b>TWO WAYS THIS WAS WRONG FIRST, both of them silent.</b></para>
    ///
    /// <para><b>One: <c>Section</c> on a COMPOSED page stops at the block's
    /// heading.</b> <c>blocks/mechanism.md</c> contains its own <c>## Why your
    /// symptoms are the ones you have</c>, and <c>Section</c> cuts at the next
    /// <c>## </c> — so on all eighteen composing hubs, "the location section of the
    /// composed page" is only the part ABOVE the include. Every one of those pages
    /// puts its type-specific prose BELOW it. A section-scoped assertion over a
    /// composed page is therefore reading a different section than its author
    /// thinks, and it fails or passes for reasons nothing in the test names. This
    /// is §12.10 in a new costume: composing is right for a prose PROPERTY over the
    /// whole page and wrong for a structural cut.</para>
    ///
    /// <para><b>Two: <c>ReaderText</c> must run on the PAGE, never on a section.</b>
    /// It calls <c>Body</c>, which slices from the front matter's closing
    /// <c>---</c>; a section has none, <c>IndexOf</c> returns -1, and the slice
    /// starts at index 3. It ate exactly three characters, which is the kind of
    /// damage a suite stays green through indefinitely.</para>
    ///
    /// <para>So: markers off the page, cut the section, and do not compose — what
    /// is wanted here is what THIS page says, which is the whole question.</para>
    ///
    /// <para>The flattening is <see cref="CuratedPage.Section"/>'s, not this helper's.
    /// Two ordered <c>Assert.Matches</c> in this class span line wraps because of it;
    /// if <c>Section</c> ever stops flattening, both stop spanning and go quiet rather
    /// than red.</para>
    /// </summary>
    private static string LocationSection(string slug, string heading) =>
        Regex.Replace(
            CuratedPage.Section(CuratedPage.ReaderText(Raw(slug)), heading),
            @"[*_]", "");

    private const string StandardHeading =
        "Where does it grow, and why does it cause these symptoms?";

    // ------------------------------------------------------------------ the table

    /// <summary>
    /// Every tumor hub, and where its reader's route to <c>/where-your-tumor-is</c>
    /// comes from. Closed: <see cref="EveryTumorHubIsInTheTable"/> fails if a page
    /// exists with no entry, so a new hub cannot inherit an exemption by omission.
    /// </summary>
    private static readonly Dictionary<string, (Source Source, string Why)> Routes =
        new(StringComparer.Ordinal)
        {
            // THE EIGHTEEN. The route is the last paragraph of blocks/mechanism.md
            // ("the other half of the question is what the place changes"), so it is
            // asserted here rather than on each page: one edit to that paragraph
            // moves eighteen pages at once, which is exactly why §12.10 wants the
            // composed page tested rather than the block.
            ["acoustic-neuroma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["all-brain-tumors"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["astrocytoma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["brain-metastases"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["cns-germ-cell-tumor"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["cns-lymphoma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["craniopharyngioma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["diffuse-midline-glioma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["dipg"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["ependymoma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["glioblastoma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["glioma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["hemangioblastoma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["high-grade-glioma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["medulloblastoma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["meningioma"] = (Source.SharedBlock, "composes [MECHANISM]; WI-569 also "
                + "wrote four routes of its own, which is why this axis is about where "
                + "the route COMES FROM rather than how many there are"),
            ["oligodendroglioma"] = (Source.SharedBlock, "composes [MECHANISM]"),
            ["pediatric-brain-tumor"] = (Source.SharedBlock, "composes [MECHANISM]"),

            // THE FIVE WI-570 FOUND, and they are not an arbitrary set: they are
            // exactly the five hubs that do not compose [MECHANISM]. Each route is
            // worded for its own reader, because the destination is shared and the
            // link LABEL is shared — §12.18: a shared label pointing at a shared
            // destination is a corpus convention, and the words AFTER it are where a
            // restatement would hide.
            // THESE TWO REASONS DESCRIBED ROUTES THAT NO LONGER EXIST until /review
            // round 2, which is §12.18's "a note that describes text a later round
            // replaced" — caught four times on the previous item and twice here. The
            // earlier drafts are kept BELOW the shipped wording, marked as refused,
            // because the reason each was wrong is the transferable part.
            ["low-grade-glioma"] = (Source.ItsOwn, "no [MECHANISM], and no region map "
                + "of its own either — this was the emptiest of the five, so its route "
                + "names each part in plain words and in the report's, and points at the "
                + "five things a team can land on. REFUSED FIRST DRAFT: 'sets out what "
                + "each part does' — the destination declines to in its own words, "
                + "'These entries do not list symptoms. That question has a better "
                + "answer of its own, region by region, and it lives on the page for "
                + "the first week'"),
            ["atrt"] = (Source.ItsOwn, "no [MECHANISM]; its location section is about "
                + "the fluid and a baby's skull, and its reader is a parent, so the "
                + "route names the destination's own note on reading it for a child"),
            ["chordoma"] = (Source.ItsOwn, "no [MECHANISM]; the route teaches the "
                + "skull-base vocabulary and bounds BOTH of the destination's "
                + "head-scoped parts by name. REFUSED FIRST DRAFT: 'it fits a chordoma "
                + "up there and not one lower down' — two of that page's parts are "
                + "head-scoped, its regions list and its urgent-signs list, and the "
                + "rest of it is location-general, so the "
                + "sentence told the two thirds of this page's readers whose tumor is "
                + "in the spine or sacrum that the page was not for them. AND THE FIX "
                + "FOR IT OVER-CORRECTED, which is the one that mattered: 'Only its "
                + "list of places is about the head. The rest of it works wherever "
                + "your tumor is' was false in the URGENT-SIGNS direction, because "
                + "the destination says out loud that its signs-to-call-about list "
                + "was written for a tumor in the brain. The SECOND sentence is "
                + "still live and correct, because the two in front of it now name "
                + "both head-scoped parts -- an unbounded remainder is the defect, "
                + "not the word 'rest'"),

            ["pituitary-tumor"] = (Source.ItsOwn, "no [MECHANISM]; the page explains "
                + "its own anatomy better than the destination does, so the route points "
                + "at how long a scan result takes to turn into a plan. REFUSED FIRST "
                + "DRAFT: 'the range a team picks from' — that range is five SURGICAL "
                + "options, and this page says twice that a prolactinoma is usually "
                + "treated with a pill, so the route would have sent that reader past "
                + "their own first-line treatment"),
            ["spinal-cord-tumor"] = (Source.ItsOwn, "no [MECHANISM]; the destination "
                + "hands the spine BACK to this page, twice, so this completes a "
                + "two-way door — and the route carries the scope limit with it"),
        };

    /// <summary>
    /// How many address words each hub carries, as a floor at roughly four fifths of
    /// the measurement (taken 2026-09-25, with the shipped lexicon).
    ///
    /// <para><b>PER HUB, because one blanket number is a floor measured on nothing.</b>
    /// §12.19's own finding 6 rules that every caller states its own minimum — and the
    /// sweep is ONE caller for twenty-three pages, so a single figure has to clear the
    /// sparsest of them. That was 2, against a median of 23: on twenty-two of the
    /// twenty-three hubs it could never fire, and <c>/tumors/ependymoma</c> could have
    /// lost 67 of its 69 address words and stayed green. The rule was being obeyed in
    /// letter by the code that broke it.</para>
    ///
    /// <para>Four fifths rather than one-under: these are whole pages, and an ordinary
    /// edit moves the count by a few. The floor is for a page whose location material
    /// has been gutted, or a lexicon that has collapsed — not for pinning a number that
    /// is nobody's business to hold still.</para>
    ///
    /// <para><b>AND THEY ARE MEASURED THROUGH <see cref="Plain(string)"/>, WHICH IS
    /// WHAT THE GUARD READS.</b> The first set was taken on the body alone, and on
    /// twelve of the twenty-three hubs the title and description carry address words
    /// too — so those figures described text the guard never sees
    /// (<c>/tumors/ependymoma</c> 67 body-only against 69 as the guard reads it). The
    /// floors were conservative either way, so nothing was fragile and nothing was
    /// red; the CLAIM was wrong, which is the part that gets copied forward.
    /// <b>Measure through the same call you assert against.</b></para>
    /// </summary>
    private static readonly Dictionary<string, int> AddressFloors = new(StringComparer.Ordinal)
    {
        ["acoustic-neuroma"] = 19, ["all-brain-tumors"] = 11, ["astrocytoma"] = 15,
        ["atrt"] = 16, ["brain-metastases"] = 14, ["chordoma"] = 40,
        ["cns-germ-cell-tumor"] = 33, ["cns-lymphoma"] = 17, ["craniopharyngioma"] = 18,
        ["diffuse-midline-glioma"] = 38, ["dipg"] = 31, ["ependymoma"] = 55,
        ["glioblastoma"] = 12, ["glioma"] = 14, ["hemangioblastoma"] = 32,
        ["high-grade-glioma"] = 15, ["medulloblastoma"] = 40, ["meningioma"] = 44,
        ["oligodendroglioma"] = 19, ["pediatric-brain-tumor"] = 21,

        // THE OUTLIER, AND IT IS THE CORRECT SHAPE FOR THAT PAGE, NOT A GAP.
        // /tumors/low-grade-glioma's three are "the upper part of the brain",
        // `tentori` matched inside its own word "supratentorial", and a "spinal cord"
        // that arrives from the shared [CAUSES] block rather than from the page. So
        // the page names ONE compartment and routes for the rest, which is the correct
        // shape for it. The next lowest hub CARRIES fourteen address words (the
        // numbers in this table are floors, not counts, and its floor is 11).
        ["low-grade-glioma"] = 2,

        ["pituitary-tumor"] = 14, ["spinal-cord-tumor"] = 16,
    };

    /// <summary>
    /// The nine location-VARIABLE hubs from WI-570's backlog scope, and the claim
    /// each one makes about where its own type sits. This is the "carries the
    /// section" half of the acceptance, pinned: the sweep found all nine already
    /// carrying one, and a pin is what stops a later tidy-up from quietly making a
    /// tenth page that routes for everything and says nothing.
    ///
    /// <para>Scoped to the location SECTION, not the page, because the failure this
    /// guards against is the claim drifting into a footer — and to the page's OWN
    /// section rather than the composed one, so the shared block's words cannot
    /// stand in for the type's. <see cref="TheTypeClaimsAreNotTheSharedBlocksWords"/>
    /// says the same thing from the other end, and is kept because these two fail
    /// differently: this one goes red if a page deletes its claim, that one if a
    /// page's claim is quietly replaced by the block's wording.</para>
    /// </summary>
    private static readonly (string Slug, string Claim)[] TypeClaims =
    [
        ("glioma", "for tumors at the front of the brain, changes in behavior and personality"),
        ("astrocytoma", "near or in the frontal lobes"),
        ("glioblastoma", "Most glioblastomas grow in the large upper part of the brain"),
        ("oligodendroglioma", "It turns up most often in the frontal lobe"),
        ("low-grade-glioma", "grade 2 diffuse gliomas are usually in the upper part of the brain"),
        ("high-grade-glioma", "most often they are in the upper part of the brain"),
        ("ependymoma", "Low at the back of the brain"),
        ("cns-lymphoma", "It tends to grow deep, near the fluid spaces in the middle of the brain"),
        ("brain-metastases", "A metastasis can land almost anywhere in the brain, and there is often more than one"),
    ];

    // ------------------------------------------------------------------ the tests

    [Fact]
    public void EveryTumorHubIsInTheTable()
    {
        var onDisk = Directory.EnumerateFiles(PagesDir, "*.md")
            .Select(f => Path.GetFileNameWithoutExtension(f)!)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();

        var missing = onDisk.Where(s => !Routes.ContainsKey(s)).ToList();
        Assert.True(missing.Count == 0,
            "these tumor hubs have no location-route ruling, so nobody has decided "
            + "whether their reader can reach /where-your-tumor-is at all: "
            + string.Join(", ", missing));

        var unfloored = onDisk.Where(s => !AddressFloors.ContainsKey(s)).ToList();
        Assert.True(unfloored.Count == 0,
            "these tumor hubs have no measured address floor, so the co-occurrence ban "
            + "runs over them with nothing asserting it has anything to fire on: "
            + string.Join(", ", unfloored));

        // BOTH tables, because a floor left behind for a renamed hub is never
        // noticed — the same orphan check `kept` already has (/review round 6).
        var stale = Routes.Keys.Concat(AddressFloors.Keys).Distinct(StringComparer.Ordinal)
            .Where(s => !onDisk.Contains(s, StringComparer.Ordinal)).ToList();
        Assert.True(stale.Count == 0,
            "these slugs are in a table and not on disk: " + string.Join(", ", stale));

        // The counts, so the shape of the corpus is a claim and not a coincidence.
        Assert.Equal(18, Routes.Count(r => r.Value.Source == Source.SharedBlock));
        Assert.Equal(5, Routes.Count(r => r.Value.Source == Source.ItsOwn));
    }

    /// <summary>
    /// THE ASSERTION THE WHOLE ITEM IS FOR: no tumor hub drops the route. Before
    /// WI-570 five of twenty-three did, and no page was individually wrong.
    /// </summary>
    [Fact]
    public void NoTumorHubDropsTheRouteToTheLocationPage()
    {
        foreach (var slug in Routes.Keys.OrderBy(s => s, StringComparer.Ordinal))
        {
            // Named, not a bare Contains: a bare one dumps a whole composed page into
            // the failure and does not say which of the twenty-three it was.
            Assert.True(Plain(slug).Contains("(/where-your-tumor-is", StringComparison.Ordinal),
                $"/tumors/{slug} has no route to /where-your-tumor-is on the composed page. "
                + "That is the obligation this whole class exists for, and before WI-570 "
                + "five hubs dropped it without any page being individually wrong.");
        }
    }

    /// <summary>
    /// And WHICH of the two it is, asserted both ways round — the half that makes
    /// the table a ruling rather than a description of today.
    /// </summary>
    [Fact]
    public void TheRouteComesFromWhereTheTableSaysItDoes()
    {
        foreach (var (slug, (source, why)) in Routes.OrderBy(r => r.Key, StringComparer.Ordinal))
        {
            var composesTheBlock = Regex.IsMatch(Raw(slug), @"(?m)^[ \t]*\[MECHANISM\][ \t]*\r?$");

            Assert.True(composesTheBlock == (source == Source.SharedBlock),
                $"/tumors/{slug}: the table says the route comes from "
                + $"{(source == Source.SharedBlock ? "the shared block" : "the page itself")} "
                + $"({why}), but the page {(composesTheBlock ? "does" : "does not")} compose "
                + "[MECHANISM]. If the include changed, the ruling has to change with it — "
                + "a page that stops composing the block loses its route silently.");

            if (source != Source.ItsOwn)
            {
                continue;
            }

            // A ROUTE HAS TO BE WHERE THE READER IS, NOT ANYWHERE ON THE PAGE.
            // TumorHubTemplateSweepTests found the same thing at its own /review
            // round 1: a whole-page Contains discharged ten questions off one link
            // in a footer. These five are asserted in the location SECTION.
            //
            // All five use the standard heading. An earlier draft carried a ternary
            // for /tumors/pediatric-brain-tumor's re-headed version, which was dead
            // code — that page composes the block, so the `continue` above filters
            // it out before this line. A branch for a case that cannot arrive reads
            // like coverage and is not (/review).
            Assert.Contains("(/where-your-tumor-is", LocationSection(slug, StandardHeading),
                StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The "carries the section" half: each of the nine says where its own type
    /// sits, in its own location section.
    /// </summary>
    [Fact]
    public void EveryLocationVariableHubCarriesItsOwnAddressClaim()
    {
        // THE FLOOR, because an iterate-and-check list with no count is a guard that
        // shrinks silently. WhereYourTumorIsPageTests does the same on its nine
        // regions, with WI-517's reason written beside it, and §12.18 asks for it in
        // as many words: assert the number rather than writing it in a comment.
        // Nine is WI-570's whole backlog scope, so a tenth or an eighth is a scope
        // change and should have to be made on purpose.
        Assert.Equal(9, TypeClaims.Length);

        foreach (var (slug, claim) in TypeClaims)
        {
            Assert.Contains(claim, LocationSection(slug, StandardHeading),
                StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// And that those claims are the PAGE'S, not the shared block's read back.
    ///
    /// <para>Without this the test above is satisfiable by a page that deletes its
    /// own location prose entirely, because the composed section contains the
    /// block. That is the §12.14 trap in a new place: a guard that reads the
    /// composed page proves the reader sees the words, and proves nothing about who
    /// wrote them.</para>
    /// </summary>
    [Fact]
    public void TheTypeClaimsAreNotTheSharedBlocksWords()
    {
        // ReaderText, so the block's FRONT MATTER is out of scope. Its source
        // comments quote page prose verbatim by design (§12.10), so reading the file
        // whole would fail this test for a reason that is not about what a reader
        // sees — and would fail it hardest on the pages whose claims are best cited.
        var block = Regex.Replace(
            CuratedPage.Flatten(CuratedPage.ReaderText(
                File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md")))),
            @"[*_]", "");

        // A FLOOR ON THE BLOCK TEXT, because every DoesNotContain below passes over an
        // empty string. This is the one list-shaped guard in the item that had none.
        Assert.True(block.Length > 2_000,
            $"blocks/mechanism.md read as {block.Length} characters, so the assertions "
            + "below are passing over text that is not there");

        foreach (var (slug, claim) in TypeClaims)
        {
            Assert.DoesNotContain(claim, block, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// §12.18's ruling over the whole tumor corpus: an address entry may carry a
    /// SYMPTOM and must never carry a DIFFICULTY.
    ///
    /// <para><b>Every kept row below is a claim, not a waiver.</b> The guard asserts
    /// each one still fires, so a lexicon that narrows goes red here rather than
    /// going quiet — and each carries the reason it is not a row, because a silent
    /// allowance reads like an oversight to the next person (the WI-509 pattern,
    /// not a bare allowlist).</para>
    ///
    /// <para><b>The row test, which is what WI-570 adds to §12.18 and what nine
    /// hubs needed and one did not:</b> could a reader read their OWN address off
    /// this sentence and get a different answer from a reader with a different
    /// one? If yes it is a row. If no — if it is true of the type wherever it sits,
    /// or keyed to a factor, an age, a gene or a structure — it is the type's own
    /// fact and it belongs here. That is what tells
    /// <c>/tumors/cns-lymphoma</c>'s "those two facts are the whole reason surgery
    /// is not the treatment" (true of every reader of that page) apart from
    /// Mayfield's closing paragraph (a different answer per address).</para>
    /// </summary>
    [Fact]
    public void NoTumorHubRanksOnePlaceAgainstAnother()
    {
        var kept = new Dictionary<string, CuratedPage.Kept[]>(StringComparer.Ordinal)
        {
            ["atrt"] =
            [
                new("how much of the tumor is left after surgery, and whether it has spread",
                    "NCI's own FACTOR list — age, how much is left, whether it has spread. "
                    + "The scan reaches it because the spread is TO the brain and spine, "
                    + "which is an extent and not an address anybody is told they have."),
            ],

            ["chordoma"] =
            [
                new("The aim is to take it out in one piece",
                    "§12.18 part 3 done right, and the best example of it in the corpus. "
                    + "The page splits surgery by site on purpose and says why — 'this is "
                    + "the part most likely to be flattened into one rule'. What changes "
                    + "is what the team AIMS at, sourced to the expert consensus group, "
                    + "which §12.18 permits in as many words."),
                new("taking it out in one piece is often not possible",
                    "The same subsection's other half, and it is the one that would be a "
                    + "row if it were written as a rank. It is not: it is labelled 'and "
                    + "that is not a failure', and its reason is the STRUCTURES in the way "
                    + "— brain stem, big vessels, the nerves for eyes and swallowing — "
                    + "which is EANO's factor, the thing a reader cannot look themselves "
                    + "up in."),
            ],

            ["diffuse-midline-glioma"] =
            [
                new("Trials are still checking whether it helps people live longer",
                    "About a DRUG's accelerated approval. The scan reaches it only because "
                    + "this type's NAME contains an address — 'midline' — which is a "
                    + "systematic false positive for any tumor named after where it sits."),
            ],

            ["dipg"] =
            [
                new("Trials are still checking whether it helps people live longer",
                    "The same sentence on the sibling page, reached the same way: the "
                    + "brainstem is in this type's definition, not in its reader's report "
                    + "as one option among several."),
            ],

            ["ependymoma"] =
            [
                new("Some ependymomas can be cured that way, when all of it can be taken out",
                    "Keyed to EXTENT, which is this tumor's factor and is sourced. The scan "
                    + "reaches it because the short version names two addresses three "
                    + "sentences earlier, as epidemiology — where it is usually found in "
                    + "children and in adults — and the anaphor is 'that way', pointing at "
                    + "surgery rather than at either place."),
                new("It can also come back, even years later, so the scans go on for a long time",
                    "THE ROW NOBODY HAD READ. /review found this one being silenced by the "
                    + "reason written for the sentence above it, because an allowance used "
                    + "to be matched against the four-sentence WINDOW rather than against "
                    + "the trigger. It earns its own reason now: 'it can come back' is true "
                    + "of every ependymoma on this page, at any address — the sentence is "
                    + "the argument for long follow-up, and a reader cannot look their own "
                    + "address up in it."),
                new("It was moved because it comes back about as often as other ependymomas",
                    "The history of a WHO regrade, and it REFUSES to rank: it says this "
                    + "kind behaves like its neighbours at the same address. A sentence "
                    + "whose whole content is 'no difference here' cannot be the row."),
                new("It can come back in the place it started, or somewhere else in the brain or spine",
                    "THE REPLACEMENT FOR THIS ITEM'S SECOND DELETION. What stood here was "
                    + "a two-address pair — 'After a tumor at the back of the brain, it "
                    + "tends to come back in the same place. After one higher up in the "
                    + "brain, it more often turns up somewhere else in the brain or spine' "
                    + "— and it is sourced (StatPearls: posterior fossa ependymomas 'recur "
                    + "locally, whereas supratentorial ependymomas tend to be disseminated "
                    + "at relapse'). A true sentence can still be the forbidden artifact. "
                    + "What replaced it names the whole territory the disease can occupy "
                    + "rather than handing two addresses two answers, so every reader of "
                    + "this page gets the same one: the row test passes rather than being "
                    + "argued around. It reaches the scan only because 'spine' is in the "
                    + "territory it names. AND THE FIRST DRAFT OF THIS ALLOWANCE IS THE "
                    + "WARNING: it excused the deleted pair on the grounds that it changed "
                    + "'what the team WATCHES, not what happens to the reader', and "
                    + "nothing on the page said that."),
            ],

            ["hemangioblastoma"] =
            [
                new("In the brainstem, symptoms can be more serious",
                    "A SYMPTOM, which §12.18's line permits, with its mechanism attached "
                    + "('so many nerves pass through that small space'). Sourced to Brain "
                    + "Tumour Research, and deliberately carried in the under-triage-safe "
                    + "direction: that page's own /review round 1 called it a BLOCKER when "
                    + "a draft took 'more serious' and dropped 'needing emergency "
                    + "attention'. Deleting it now would re-open that blocker."),
                new("tumors that come with VHL are also more likely to come back after surgery",
                    "Keyed to a GENETIC CONDITION, not to a place. 'the brain and spinal "
                    + "cord' in the same sentence is where hemangioblastomas are, full "
                    + "stop — it is not one option a reader is picking out of a list."),
            ],

            ["medulloblastoma"] =
            [
                new("every child under 3 is in the higher risk group",
                    "AGE-keyed and sourced twice over. 'radiation to the whole brain and "
                    + "spine' is a treatment FIELD, not an address."),
                new("There is no single agreed plan for a medulloblastoma that comes back",
                    "A flattening artifact: the window reaches back across a '## If it "
                    + "comes back' heading into the scans section. Nothing in the sentence "
                    + "names a place. Kept rather than silenced so that if the paragraph "
                    + "ever does grow an address, the guard is already looking at it."),
            ],
        };

        // NO ALLOWANCE MAY BE ORPHANED. A renamed or deleted slug would otherwise
        // take its written reasons out of the run without anything going red, which
        // is the same silence the `kept`-still-fires assertion exists to break.
        var orphans = kept.Keys.Where(k => !Routes.ContainsKey(k)).ToList();
        Assert.True(orphans.Count == 0,
            "these slugs have kept rows and are not in the route table, so their reasons "
            + "are asserted against nothing: " + string.Join(", ", orphans));

        // AND THE FLOOR ON THE COUNT. THIRTEEN rows over seven hubs is the measured
        // number; §12.18 asks for the assertion rather than the comment. (/review
        // round 2 found the comment here still saying twelve above an Assert of
        // thirteen — a count in prose beside a count in code is two things to keep
        // in step, which is the argument for not writing the prose one.) If a later
        // edit makes a row disappear, that is a content change and it should have to
        // be made on purpose rather than found later.
        Assert.Equal(13, kept.Sum(k => k.Value.Length));

        foreach (var slug in Routes.Keys.OrderBy(s => s, StringComparer.Ordinal))
        {
            // The floor comes from AddressFloors, which carries its own reasons —
            // including /tumors/low-grade-glioma's outlier. An earlier 19-line comment
            // here argued for the single blanket number that table replaced (/review
            // round 6): a comment defending a deleted argument is worse than none.
            CuratedPage.AssertNoPlaceIsRankedAgainstAnother(
                Plain(slug), "/tumors/" + slug, AddressFloors[slug],
                kept.TryGetValue(slug, out var k) ? k : []);
        }
    }

    /// <summary>
    /// THE TWO ROUTES THAT BOUND THE DESTINATION'S HEAD-SCOPED PARTS SAY SO IN
    /// WRITING, AND NOTHING ELSE CAN SEE IT.
    ///
    /// <para>§12.19 records that the routing promise failed SIX times in this item —
    /// three under-claims, two OVER-claims introduced by the fixes for them, and one
    /// that was not about wording at all: the paragraph was filed under one of three
    /// site subsections and deep-linked to the entry for a different one. The
    /// over-claims are the pair that mattered: <c>/tumors/chordoma</c> and
    /// <c>/tumors/spinal-cord-tumor</c> both told a reader whose tumor is not in the
    /// head that the rest of <c>/where-your-tumor-is</c> was theirs, when that page
    /// says its <b>urgent-signs</b> list was written for a tumor in the brain. **The
    /// over-claim ran in the under-triage direction**, which is the one direction a
    /// mistake on this site may not run.</para>
    ///
    /// <para>No property guard can see a route that promises too much, and the
    /// eight-gram corpus probe cannot either. So the bound is pinned by name. Delete
    /// the qualifying sentence and "the rest of it works wherever your tumor is"
    /// silently becomes the unbounded claim that was refused. WI-569 pinned its own
    /// route wording for the same reason.</para>
    /// </summary>
    [Fact]
    public void TheTwoRoutesThatBoundTheDestinationStillSayWhatTheyBound()
    {
        // ORDERED, not merely present. /review round 6: asserting that the bounding
        // sentences exist SOMEWHERE in the section stays green if they drift into
        // another paragraph while the remainder claim keeps standing where it is —
        // which is the defect this test exists to stop, so the remainder claim is in
        // the match and the order is the assertion.
        // AND ITS POSITION, NOT JUST ITS ORDER — because the blocker on this page was
        // PLACEMENT, and nothing was guarding it (/review round 7). The route used to
        // sit at the END of `### In the spine and at the tailbone` and deep-link
        // `#skull-base`, so the two thirds of this page's readers whose tumor is lower
        // down finished THEIR subsection on a lead about the skull and landed in the
        // one destination entry that is not theirs. The prose was corrected over three
        // rounds while the filing stayed wrong: **a route can be word-perfect and
        // still be addressed to the wrong reader.** The match below spans the end of
        // the section PREAMBLE, through the route, to the first `###` subsection, so
        // moving it back under either site subsection goes red.
        // The gaps are measured -- 1, 116, 25, 24, 136 through THIS pattern, because
        // each anchor's trailing "." is itself a wildcard that eats one character
        // before `.{0,N}` starts counting -- and rounded well up, so an ordinary
        // reword survives and a paragraph moving does not. (/review round 8: the first
        // version of this note gave the five raw inter-string gaps, one convention off
        // from the code beside it.)
        Assert.Matches(
            "The same tumor against a nerve causes trouble early.{0,40}"
            + "Looking it up by place instead of by name.{0,200}"
            + "Two parts of it are written for the head.{0,80}"
            + "the signs it says to call about.{0,60}"
            + "The rest of it works wherever your tumor is.{0,220}"
            + "At the base of the skull",   // the literal heading, not "### " generally:
                                            // reordering the two site subsections
                                            // is a legitimate edit and SHOULD red
                                            // this, because it moves the preamble.
            LocationSection("chordoma", StandardHeading));

        // AND NO ANCHOR ON THAT LINK AT ALL -- wider than the reason strictly needs,
        // on purpose. The defect was a REGION anchor (`#skull-base`) on a page with
        // three sites; `#no-list` or `#questions` would be harmless. The ban is on
        // every anchor anyway, because this paragraph is the one route on the page and
        // its job is to be true of all three sites, so there is no anchor it should
        // want. /tumors/pituitary-tumor keeps `#pituitary` and is right to: that page
        // has ONE site. If a later edit wants a second, anchored link here, loosening
        // this is the deliberate act it should be.
        Assert.DoesNotContain("/where-your-tumor-is#", LocationSection("chordoma", StandardHeading),
            StringComparison.Ordinal);

        Assert.Matches(
            "Its list of regions was written for the head.{0,60}"
            + "So was its list of urgent signs.{0,160}"
            + "What is left over is still yours",
            LocationSection("spinal-cord-tumor", StandardHeading));

        // AND THE DESTINATION STILL SAYS BOTH THINGS, because a bound is only honest
        // while the page it describes still describes itself that way. If
        // /where-your-tumor-is ever stops scoping either list to the head, these two
        // routes are telling a reader to distrust something that is now theirs.
        var destination = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Read("where-your-tumor-is.md")));
        Assert.Contains("These nine are places in the head", destination, StringComparison.Ordinal);
        Assert.Contains("parts of the list above were written for a tumor in the brain",
            destination, StringComparison.Ordinal);
    }

    /// <summary>
    /// EVERY ADDRESS TOKEN WI-570 ADDED OWES A CONTROL, AND THIS IS THE ACCEPTANCE
    /// RATHER THAN THE PROMISE. Remove any one of the eleven from the lexicon and at
    /// least one positive control must stop catching its own planted row.
    ///
    /// <para>§12.19 ships the sentence <i>"add the control in the same edit as the
    /// token, or the token is decoration"</i>. It was written at `/review` round 3,
    /// which added three controls — and round 4 measured that <b>eight of the eleven
    /// tokens still turned no control red</b>. Writing a rule is not applying it, and
    /// a measurement taken once is a claim about the afternoon it was taken. So the
    /// rule is a test.</para>
    ///
    /// <para>It also catches the shape that made round 3's third control useless: a
    /// control carrying TWO new tokens masks each of them under single-token
    /// ablation and goes red for neither. One token per control, and this goes red if
    /// that is ever violated for a token with no other control.</para>
    /// </summary>
    [Fact]
    public void EveryAddressTokenThisItemAddedHasAControlThatFailsWithoutIt()
    {
        // THE PAGE IS DELIBERATELY IRRELEVANT, and saying so is the honest version.
        // ControlSpacer is three sentences, so no control's four-sentence window can
        // reach the page behind it — the result is page-independent by construction,
        // and /review confirmed identical results on /tumors/ependymoma (69 address
        // words) and this one (3). An earlier comment here claimed the sparsest hub
        // was chosen so a control could not be "flattered" by a rich page, which
        // reads as load-bearing and is not: the spacer is what does that, and if it
        // ever stopped doing it the two pages would disagree.
        var page = Plain("low-grade-glioma");

        // THE COUNT, and it is the same omission this array was written to close:
        // delete an entry and the test below silently checks ten tokens and stays
        // green. Every other list in this item carries one.
        Assert.Equal(11, CuratedPage.LocationAddressTokensAddedByWi570.Length);

        foreach (var token in CuratedPage.LocationAddressTokensAddedByWi570)
        {
            Assert.Contains(token, CuratedPage.LocationAddress, StringComparison.Ordinal);

            var ablated = new Regex(
                CuratedPage.LocationAddress.Replace(token, "", StringComparison.Ordinal),
                RegexOptions.IgnoreCase);

            var lost = Enumerable.Range(0, CuratedPage.LocationPositiveControls.Length)
                .Where(i => CuratedPage.ControlIsCaught(page, i)
                         && !CuratedPage.ControlIsCaught(page, i, ablated))
                .ToList();

            Assert.True(lost.Count > 0,
                $"removing the address token `{token.TrimStart('|')}` from the lexicon turns "
                + "no positive control red, so nothing would notice if a later edit deleted "
                + "it. Add a control that depends on this token ALONE — a control carrying "
                + "two new tokens masks both and proves neither.");
        }
    }

    /// <summary>
    /// NARROWING <c>(?:the|your)</c> TO EITHER BRANCH MUST TURN A CONTROL RED.
    ///
    /// <para><b>FOUR</b> of the tokens in
    /// <see cref="CuratedPage.LocationAddressTokensAddedByWi570"/> carry both
    /// determiners, because this corpus writes a region one way in a shared block
    /// (<i>"Side of the brain, near the temple"</i>, on eighteen hubs) and the other
    /// way in a page heading (<i>"The side of your brain, near your ear"</i>). An
    /// earlier draft of this note said five. Whole-token ablation cannot tell the
    /// branches apart — so until <c>/review</c> round 7, three controls exercised only
    /// the <c>your</c> branch, and a later tidy-up narrowing the alternation would have
    /// blinded the guard on eighteen composed pages while the whole suite stayed
    /// green.</para>
    ///
    /// <para><b>AND THE RESIDUAL — AND ROUND 9'S VERSION OF THIS PARAGRAPH GOT IT
    /// WRONG IN THE ONE DIRECTION THAT MATTERS.</b> WI-570 also put
    /// <c>(?:the|your)</c> into three of WI-569's own tokens:
    /// <c>base of (?:the|your) (?:brain|skull)</c>,
    /// <c>(?:top|front|back) of (?:the|your) skull</c> and
    /// <c>(?:top|back) of (?:the|your) head</c>. Two of those three are not in this
    /// loop; <c>base of (?:the|your) (?:brain|skull)</c> is, and both of its branches
    /// have a control of their own.</para>
    ///
    /// <para><b>AND FOUR REVIEW ROUNDS IN A ROW FAILED ON THIS ONE NOTE'S ARITHMETIC,
    /// WHICH IS THE FINDING.</b> Round 9 wrote <i>"the remaining five branches match
    /// nothing in the corpus"</i> (false — most are live). Round 10 corrected the
    /// headline and named the wrong branch as unwatched. Round 11 got the assignment
    /// right and miscounted the inventory. Round 12 found the recount wrong again. Each
    /// version was a hand-maintained census of branches, live-or-dead, watched-or-not —
    /// <b>which is a count in prose beside a count in code, the exact thing this file
    /// polices everywhere else.</b> So the census is gone. What replaces it is the rule
    /// plus the one place the answer actually lives:</para>
    ///
    /// <list type="bullet">
    ///   <item><b>THE RECORD IS THE ITERATED SET, NOT A PARAGRAPH.</b> Whatever this
    ///     loop covers is covered; whatever it does not, is not. Adding a token here is
    ///     how a branch gets watched, and the loop fails loudly if a branch it covers
    ///     loses its control. No prose can drift from that.</item>
    ///   <item><b>TWO WAYS A BRANCH GOES UNWATCHED, and the second is the
    ///     non-obvious one.</b> Either no control exercises it — or a control does, and
    ///     that control carries a SECOND address token which masks it, so narrowing the
    ///     first cannot show a loss. <c>(?:top|front|back) of the skull</c> is masked
    ///     inside its control by <c>floor</c>; <c>(?:top|back) of your head</c> by
    ///     <c>cliv</c>. Closing either means writing a control that carries exactly
    ///     ONE address, or de-masking the existing one.</item>
    ///   <item><b>A DEAD BRANCH IS NOT A TRIMMABLE ONE HERE.</b> Some branches match
    ///     nothing in the corpus today and are kept prospectively, because unlike a dead
    ///     ADDRESS TOKEN a dead branch of a live token costs nothing. Do not read
    ///     "matches nothing" as "delete it" — see <c>CuratedPage.LocationAddress</c> for
    ///     why that asymmetry exists.</item>
    /// </list>
    ///
    /// <para><b>The one branch that was worth acting on rather than recording</b> was
    /// <c>base of (?:the|your) (?:brain|skull)</c>'s <c>the</c> form: it is in
    /// <c>blocks/mechanism.md</c>'s <i>"Behind the eyes, at the base of the brain"</i>,
    /// so it reaches all eighteen composing hubs, and it had no control at all while its
    /// sibling branch had one. It got one, and the token joined this loop. <b>Eighteen
    /// hubs is not a residual.</b> The rest is §12.18's stop rule, and what generalises
    /// is one sentence: <b>a branch can be alive in the corpus, load-bearing, and still
    /// invisible to ablation, because ablation measures the CONTROLS and not the
    /// pages.</b></para>
    ///
    /// <para>That the tokens were RIGHT was never in doubt. What was wrong was that
    /// the note claiming which wording they covered had quoted the wrong file, and the
    /// controls had been written off the quotation instead of off the corpus.</para>
    /// </summary>
    [Fact]
    public void NarrowingTheOrYourInTheAddressLexiconTurnsAControlRed()
    {
        const string both = "(?:the|your)";
        Assert.Contains(both, CuratedPage.LocationAddress, StringComparison.Ordinal);

        var page = Plain("low-grade-glioma");
        var caught = Enumerable.Range(0, CuratedPage.LocationPositiveControls.Length)
            .Where(i => CuratedPage.ControlIsCaught(page, i))
            .ToList();
        Assert.Equal(CuratedPage.LocationPositiveControls.Length, caught.Count);

        // PER TOKEN, PER BRANCH. Narrowing ALL of them at once was the first version,
        // and /review round 8 measured that it is satisfied by any ONE token's branch:
        // deleting the three controls written for this very hole left it green. A test
        // named for a hole has to fail when the hole is open.
        //
        // THE FIFTH TOKEN IS ONE OF WI-569's, WIDENED BY WI-570 AND WATCHED SINCE
        // ROUND 11. `base of (?:the|your) (?:brain|skull)` is not in the ADDED list,
        // but both its branches now have a control of their own, so it belongs in the
        // loop — and its `the` form is the widest-reach branch in the lexicon, on
        // eighteen hubs through blocks/mechanism.md. The other two widened tokens stay
        // out: see the summary for which of their branches are live, which are masked
        // inside a control by a second address token, and why closing them is the next
        // item's measurement.
        var watched = CuratedPage.LocationAddressTokensAddedByWi570
            .Append(@"|base of (?:the|your) (?:brain|skull)")
            .Where(t => t.Contains(both, StringComparison.Ordinal))
            .ToList();

        // THE MEMBERSHIP IS ASSERTED, because the loop's contents ARE the record and
        // nothing else was holding them. /review round 13: a maintainer reading a stale
        // sentence beside this loop could have deleted the Append to match it, dropping
        // the widest-reach branch in the lexicon (eighteen hubs) out of watch — and the
        // suite would have stayed green, because a smaller iterated set simply asserts
        // less. A record that can shrink silently is not a record.
        Assert.Equal(5, watched.Count);
        Assert.Contains(@"|base of (?:the|your) (?:brain|skull)", watched);

        foreach (var token in watched)
        {
            foreach (var branch in new[] { "the", "your" })
            {
                var narrowed = new Regex(
                    CuratedPage.LocationAddress.Replace(
                        token, token.Replace(both, branch, StringComparison.Ordinal),
                        StringComparison.Ordinal),
                    RegexOptions.IgnoreCase);

                Assert.Contains(caught, i => !CuratedPage.ControlIsCaught(page, i, narrowed));
            }
        }
    }

    /// <summary>
    /// The two pages §12.18 measured this lexicon against before handing it over,
    /// so the measurement is a test rather than a paragraph.
    ///
    /// <para>§12.18 records that the guard fires ZERO times on
    /// <c>/treatments/craniotomy</c> and ONCE, FALSELY, on
    /// <c>/where-your-tumor-is</c>. Both still hold; the second is pinned with its
    /// reason rather than left as prose somebody has to re-derive.</para>
    /// </summary>
    [Fact]
    public void TheTwoPagesTheRulingWasMeasuredAgainstAreSweptToo()
    {
        // Through the same Plain() the tumor hubs go through, including its two
        // assertions. The first draft copied those eight lines here and dropped the
        // title check — which is §12.8's factor-at-the-second-use rule, broken by
        // the item whose whole argument for promoting the scanner is that rule.
        // ZERO ADDRESSES, AND THAT IS THE POINT OF PASSING THE NUMBER. This page has
        // 21 difficulty matches and NO address words at all,
        // so its famous "fires zero times" is a property of the lexicon rather than
        // a clean bill of health for the page — no row could be found here whatever
        // it said. Written down as structural rather than left to be read as
        // evidence. It stays in the sweep because the day this page DOES name an
        // address, the ban should already be standing over it.
        CuratedPage.AssertNoPlaceIsRankedAgainstAnother(
            PlainOfPage("treatments", "craniotomy.md"), "/treatments/craniotomy",
            minAddresses: 0);

        CuratedPage.AssertNoPlaceIsRankedAgainstAnother(
            PlainOfPage("where-your-tumor-is.md"), "/where-your-tumor-is",
            // 55 against a measured 61, and DELIBERATELY not one-under: the
            // region COUNT is already pinned exactly, by
            // WhereYourTumorIsPageTests' Assert.Equal(9, entries.Count). This
            // floor is here to catch the lexicon collapsing, not to re-pin a
            // number another file owns.
            minAddresses: 55,
            new CuratedPage.Kept(
                "When it is somewhere that cannot be taken out, such as the brain stem",
                "§12.18's one recorded false positive, and it is correct ACS-sourced "
                + "prose: the page's whole argument is that surgery is a RANGE a team "
                + "picks from, and this is the end of the range being named. It names "
                + "the brain stem as an EXAMPLE of that end, not as a row in a list "
                + "where other addresses get a better answer — and ten lines later the "
                + "same page refuses the ranking outright under 'Why there is no list "
                + "here of which places are dangerous'."));
    }
}
