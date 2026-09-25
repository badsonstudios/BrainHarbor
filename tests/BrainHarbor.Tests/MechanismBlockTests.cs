using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-568: <c>Content/blocks/mechanism.md</c> — the block that answers "why are my
/// symptoms the ones I have", and the most widely composed prose in the corpus.
///
/// It is live on <b>EIGHTEEN</b> tumor hubs. The backlog said "~20" in three places
/// and that number was wrong in the dangerous direction: a bare grep for the word
/// MECHANISM returns 22 files, but four of those mention it only in front-matter
/// comments and NONE of the four includes the block. Counting them would put an
/// edit's blast radius on pages that are not in it.
///
/// Of those four, exactly ONE carries a real block ruling —
/// <c>/tumors/pituitary-tumor</c>, whose front matter says <c>MECHANISM -- OUT</c>
/// and gives its reasons. The other three use the word in unrelated senses and were
/// checked individually rather than assumed: <c>/tumors/spinal-cord-tumor</c> and
/// <c>/treatments/steroids</c> use it about a SOURCE's explanatory sentence and about
/// scoping a conditional, and <c>/tumors/atrt</c> uses it about a source's mechanism
/// sentence it declines to follow. (An earlier draft of this comment named the wrong
/// three pages as deliberate excluders; <c>/review</c> round 1 caught it, which is the
/// lesson two tests below are about.)
///
/// WI-568 fixed two things:
///
/// <list type="number">
/// <item><b>THE RULE WAS FALSE FOR THE READER WHO NEEDED IT MOST.</b> "The symptom
/// tells you where, the scan tells you what" does not hold for an obstructive
/// hydrocephalus presentation: a tectal or fourth-ventricle reader gets morning
/// headache, vomiting, double vision and unsteadiness, which are PRESSURE symptoms
/// produced by backed-up fluid and are much the same wherever the blockage is. The
/// reader applied the rule, found nothing in the list that matched, and concluded
/// the list was not about them. The rule is UNCHANGED — §12.6 keeps the qualifier
/// and shortens it, and a rule qualified into a paragraph of caveats is worse than
/// the wrong rule because nobody reads it. One named exception sits beneath it.</item>
/// <item><b>THREE REGIONS WERE MISSING</b> — the deep middle (where a tectal tumor
/// sits, and the one that disturbs FLOW rather than a function), the skull base, and
/// the pituitary/sellar region.</item>
/// </list>
///
/// <b>§12.10 governs and it is why this file exists.</b> A block is prose asserted on
/// every page that includes it and the failure is SILENT — nothing in ContentCheck or
/// the suite can tell you a block's words are wrong for the page pulling them in. The
/// reader is the only detector, and here there are eighteen of them.
/// </summary>
public sealed class MechanismBlockTests
{
    /// <summary>
    /// Every hub that includes the block, STATED rather than discovered, so a
    /// nineteenth includer has to be added here by hand and its prose read
    /// (the <see cref="SpinalCordBlockTests"/> convention).
    ///
    /// Discovering this list by globbing would defeat the point: the whole risk of a
    /// block edit is that a page nobody re-read inherits prose that is wrong for it.
    /// A list that grows by itself never makes anybody look.
    ///
    /// <c>internal</c> since WI-567, which needs the same eighteen to prove the door
    /// it added to this block reaches a reader on every one of them rather than on a
    /// sample of three.
    /// </summary>
    internal static readonly IReadOnlyList<string> IncludingHubs =
        ["acoustic-neuroma", "all-brain-tumors", "astrocytoma", "brain-metastases",
         "cns-germ-cell-tumor", "cns-lymphoma", "craniopharyngioma", "diffuse-midline-glioma",
         "dipg", "ependymoma", "glioblastoma", "glioma", "hemangioblastoma", "high-grade-glioma",
         "medulloblastoma", "meningioma", "oligodendroglioma", "pediatric-brain-tumor"];

    public static TheoryData<string> IncludingHubData => [.. IncludingHubs];

    private static string Block => File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md"));

    private static string Hub(string slug) => CuratedPage.Read("tumors", $"{slug}.md");

    private const string TheRule = "the symptom tells you where, the scan tells you what";

    /// <summary>
    /// The list is the block's, so the words asserted here are the block's. Each one
    /// was grepped against all eighteen pages before being chosen: the REGION NAMES
    /// themselves are unusable as needles, because the pages retype them — "skull
    /// base" is on /tumors/meningioma, "pituitary" is on four of the eighteen — and a
    /// needle with two homes cannot be broken by a single-point mutation (WI-549).
    /// </summary>
    private static readonly string[] BlockOnlyNeedles =
    [
        "There is one exception, and it is the fluid",
        "they tell you the pressure is up, not where the tumor is",
        "is the one about you, and it says what can be done",
        "often the pressure rising rather than anything that points to the spot",
        "numbness or weakness down one side of the face",
        "In a child, growth and puberty too",
        "so things start disappearing at the edge",

        // WI-567 appended the door this block owed to /where-your-tumor-is, and
        // that is prose on all eighteen hubs like everything above it. Verified
        // block-only before use: no including page says these words.
        //
        // /review round 3 replaced the second of these. It was "when the place itself
        // makes something urgent", which is also how /where-your-tumor-is's own
        // front-matter DESCRIPTION ended — and that description renders as the first
        // paragraph a reader meets. No including page carried it, so nothing here
        // failed, but the deploy smoke needles for this item are written from this
        // list, and WI-568's recorded probe defect is a needle with two homes.
        "The other half of the question is",
        "This list is about what the place explains",
    ];

    [Fact]
    public void TheEighteenIncludersAreExactlyThePagesThatIncludeTheBlock()
    {
        // The guard on the list above: it is hand-maintained, so something has to
        // notice when the corpus and the list disagree. Discovery is used HERE, to
        // check the stated list, and nowhere else.
        var found = CuratedPage.AllPages()
            .Where(p => p.Slug.StartsWith("pages/", StringComparison.Ordinal))
            .Where(p => Regex.IsMatch(p.Text, @"^\[MECHANISM\]", RegexOptions.Multiline))
            .Select(p => p.Slug.Split('/')[^1])
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(IncludingHubs.OrderBy(s => s, StringComparer.Ordinal).ToArray(), found);

        // And the number itself, because the backlog got it wrong by saying "~20"
        // and an approximate blast radius is not a blast radius.
        Assert.Equal(18, found.Length);
    }

    [Fact]
    public void TheRuleParagraphIsUnchangedAndCarriesNoCaveat()
    {
        // The Gate 1 ruling, pinned. The fix was NOT to weaken the rule: eighteen
        // pages were written against it, and §12.6 says accuracy is lost when a hedge
        // is cut, not when a sentence is simplified. If a later item softens the rule
        // to "usually tells you where", this fails and the ruling gets re-read.
        var flat = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        Assert.Contains(TheRule, flat, StringComparison.Ordinal);
        Assert.DoesNotContain("the symptom usually tells you", flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("the symptom often tells you", flat, StringComparison.OrdinalIgnoreCase);

        // THE SCOPE IS THE WHOLE RULE PARAGRAPH, NOT THE RULE'S FIRST SENTENCE, AND
        // THE BREAK HARNESS IS WHY.
        //
        // This guard used to slice `SentencesOf(...).Single(s => s.Contains(TheRule))`,
        // which is the sentence "The short rule: the symptom tells you where, the scan
        // tells you what." The paragraph has a SECOND sentence — "A tumor disturbs
        // whatever job the brain does in that spot, so where it sits decides what you
        // notice." — and the mutation `rule-grows-a-caveat` appended ", unless the
        // fluid is blocked." to THAT one. It survived on LF and on CRLF, through five
        // review rounds that all read this test.
        //
        // That is precisely option (B) the Gate 1 ruling refused in writing: the rule
        // qualified in place, so the exception below it becomes redundant and the rule
        // stops being a rule. The guard was aimed at one sentence of a two-sentence
        // statement. WI-549's lesson runs the other way here — the correction for a
        // too-NARROW guard is a WIDER guard — and the paragraph is the honest unit,
        // because the paragraph is what the reader reads as "the rule".
        var deEmphasised = flat.Replace("**", "");
        var start = deEmphasised.IndexOf("The short rule:", StringComparison.Ordinal);
        var end = deEmphasised.IndexOf("There is one exception", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start,
            "the rule paragraph or the exception that follows it has moved");
        var rulePara = deEmphasised[start..end];

        var caveat = new Regex(@"\b(unless|except|but|however|although)\b", RegexOptions.IgnoreCase);

        // The guard can fire: prove it on the exact mutation that defeated its
        // predecessor, not on a phrase invented for the canary (§12.10's canary rule,
        // and WI-549's "prove the instrument can say yes before accepting a no").
        Assert.Matches(caveat,
            "so where it sits decides what you notice, unless the fluid is blocked.");
        Assert.DoesNotMatch(caveat, rulePara);

        // And the rule's own sentence is still ONE sentence. Emphasis is stripped
        // before splitting: the rule ends "...the scan tells you what.**", and
        // SentencesOf splits on `(?<=[.!?])\s+`, so the `**` between the period and
        // the space stops the split firing at all (/review round 1 nit).
        var ruleSentence = CuratedPage.SentencesOf(rulePara)
            .Single(s => s.Contains(TheRule, StringComparison.Ordinal));
        Assert.EndsWith("what.", ruleSentence, StringComparison.Ordinal);

        // The paragraph is two sentences, and a third would be the caveat paragraph
        // arriving by another route.
        Assert.Equal(2, CuratedPage.SentencesOf(rulePara.Trim()).Length);
    }

    [Fact]
    public void TheExceptionSitsBetweenTheRuleAndTheListSoTheReaderMeetsItInTime()
    {
        // POSITION IS THE PROPERTY (WI-512, and §12.10's landing rule). An exception
        // a reader meets AFTER the list has already sent them to the wrong place has
        // scoped nothing. Presence alone would pass with the paragraph in the footer.
        var flat = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        var rule = flat.IndexOf(TheRule, StringComparison.Ordinal);
        var exception = flat.IndexOf("There is one exception, and it is the fluid", StringComparison.Ordinal);
        var firstBullet = flat.IndexOf("- **Front of the brain.**", StringComparison.Ordinal);

        Assert.True(rule >= 0 && exception > rule,
            "the pressure exception does not sit below the rule it corrects");
        Assert.True(firstBullet > exception,
            "the pressure exception sits BELOW the location list, so the reader this item "
            + "exists for has already scanned the list and concluded it is not about them");
    }

    [Fact]
    public void TheExceptionBridgesToTheEntryItSendsTheReaderTo()
    {
        // THE DEFECT WAS A MISSING BRIDGE, NOT MISSING MATERIAL. The block already
        // carried "Pressure can build up inside the head" and "It can block the flow
        // of fluid" ABOVE the list; nothing told the reader those answered a
        // different question from the list, so the list read as the whole answer.
        // The exception is only a fix if it lands the reader somewhere.
        var flat = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        Assert.Contains("Pressure can build up inside the head", flat, StringComparison.Ordinal);
        Assert.Contains("It can block the flow of fluid", flat, StringComparison.Ordinal);

        // The exception names the signs rather than assuming the reader scrolls back
        // for them. SLICED BY POSITION, not by counting sentences: the old .Take(4)
        // reached past the paragraph into the first list bullet, so it would have
        // stayed green on an exception that drifted into the list (/review round 1).
        var start = flat.IndexOf("There is one exception", StringComparison.Ordinal);
        var end = flat.IndexOf("- **Front of the brain.**", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "the exception paragraph is not above the list");
        var exception = flat[start..end];

        Assert.Contains("headaches worse in the morning", exception, StringComparison.Ordinal);
        Assert.Contains("throwing up", exception, StringComparison.Ordinal);
        Assert.Contains("double vision", exception, StringComparison.Ordinal);
        Assert.Contains("feeling unsteady", exception, StringComparison.Ordinal);

        // IT DOES NOT CLAIM TO REPRINT THE PRESSURE SECTION'S OWN LIST. /review round 1:
        // the first draft said "you get the pressure pattern above instead:" and then
        // printed a DIFFERENT list — double vision and unsteadiness are not in the
        // section above, and seizures and drowsiness are not in this one. Two lists
        // four lines apart, both called "the pressure pattern", on eighteen pages.
        Assert.DoesNotContain("the pressure pattern above", exception, StringComparison.Ordinal);

        // AND IT LANDS ON AN ACTION, not on a restatement of the problem. §12.6: never
        // end on a frightening sentence, follow it with something concrete. The first
        // draft routed to the deep-middle entry, which told the reader what the
        // sentence that sent them there had just told them — circular, and no action.
        // The blocked-fluid section owns the action AND the /treatments/shunts door.
        // It names the destination VERBATIM, so it is findable by eye and by Ctrl-F —
        // the section is a bold lead, not a heading, so there is no anchor to link
        // (/review round 2).
        Assert.Contains("the part above called **It can block the flow of fluid** is the one about you",
            exception, StringComparison.Ordinal);
        Assert.Contains("**It can block the flow of fluid.**", flat, StringComparison.Ordinal);
        Assert.Contains("it says what can be done", exception, StringComparison.Ordinal);
        Assert.Contains("[Shunts and hydrocephalus](/treatments/shunts)", flat, StringComparison.Ordinal);

        // And the entry a location-seeking reader needs exists too.
        Assert.Contains("- **Deep in the middle, near the spaces the fluid runs through.**",
            flat, StringComparison.Ordinal);
    }

    [Fact]
    public void TheListCarriesNineRegionsAndTheOriginalSixAreUntouched()
    {
        // The three new regions are appended, NOT interleaved, and that is deliberate:
        // three live pages scope the block by naming entries in it ("the cerebellum
        // entry", "the brainstem entry"), so reordering the six would put their prose
        // out of step with what follows it.
        var flat = CuratedPage.Flatten(CuratedPage.ReaderText(Block));
        var listStart = flat.IndexOf("- **Front of the brain.**", StringComparison.Ordinal);
        var listEnd = flat.IndexOf("If something on this list matches", StringComparison.Ordinal);
        Assert.True(listStart >= 0 && listEnd > listStart,
            "the region list or the paragraph that closes it has moved");
        var list = flat[listStart..listEnd];

        string[] inOrder =
        [
            "- **Front of the brain.**",
            "- **Side of the brain, near the temple.**",
            "- **Upper back part of the brain.**",
            "- **Back of the brain.**",
            "- **Cerebellum, low at the back.**",
            "- **Brainstem.**",
            "- **Deep in the middle, near the spaces the fluid runs through.**",
            "- **The floor of the skull, below and behind the ear.**",
            "- **Behind the eyes, at the base of the brain.**",
        ];

        // Scanned FORWARD from the last match rather than by first occurrence, so a
        // bullet duplicated earlier cannot mask a later one being out of place
        // (/review round 1).
        var at = 0;
        foreach (var bullet in inOrder)
        {
            var next = list.IndexOf(bullet, at, StringComparison.Ordinal);
            Assert.True(next >= 0, $"the region list is missing or out of order at: {bullet}");
            at = next + bullet.Length;
        }

        // Exactly nine — a tenth added without reading this file is a block edit
        // nobody re-read the eighteen pages for. Counted over the LIST only: the slice
        // used to run to the end of the block, so a bullet added in the closing prose
        // would have inflated it (/review round 1).
        Assert.Equal(9, Regex.Matches(list, @"(?:^|\s)- \*\*").Count);
    }

    [Fact]
    public void TheThreeNewRegionsTeachTheWordTheReadersReportWillUse()
    {
        // §12.6: say the outcome, then name the word. A reader who was handed
        // "sellar" or "skull base" on a report has to be able to find it here, and a
        // reader who was not must not have to learn it to use the list.
        var flat = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        Assert.Contains("Your team may call the whole floor of the skull the **skull base**",
            flat, StringComparison.Ordinal);
        Assert.Contains("Your team may call this the **pituitary** or the **sellar** region",
            flat, StringComparison.Ordinal);

        // THE TERM IS TAUGHT ONCE, ON BULLET 8, AND BULLET 9 IS DELIBERATELY LEFT
        // UNLABELLED. Round 2 noted that bullet 9 — behind the eyes — is also part of
        // the skull base and does not say so, and round 2's fix added "and the next
        // entry is another" to cover it. ROUND 3 DROPPED THAT CLAUSE: on the composed
        // /tumors/meningioma page it collided with meningioma.md:304, which treats
        // behind-the-eyes as an ADDITION to the floor rather than a part of it, on a
        // page where "behind the eyes" already names the sphenoid wing.
        //
        // So this is an OMISSION, not a contradiction, and it is left standing on
        // purpose. The block says the term covers the whole floor and that bullet 8's
        // spot is one part of it; meningioma says the umbrella covers the floor and the
        // ridge behind the eyes. Those agree. WI-569 owns meningioma's location
        // material and is where the two vocabularies get reconciled.
        Assert.Contains("Your team may call the whole floor of the skull the **skull base**. This spot is one part of it.",
            flat, StringComparison.Ordinal);

        // THE SKULL BASE IS THE WHOLE FLOOR, AND THE BULLET SAYS SO. /review round 1's
        // blocker: the first draft taught "skull base" as meaning the one spot below
        // and behind the ear, while /tumors/meningioma — a page this block composes
        // ONTO — says in reader-facing prose thirteen lines above the directive that
        // the term is "an umbrella word for the ones growing on the floor of the skull
        // and the ridge behind the eyes". Two definitions of a word the reader carries
        // to an appointment, on one composed page. MSKCC settles it in meningioma's
        // favour ("the base or floor of the cranium, the part of the skull on which the
        // brain rests"), so the bullet now scopes itself as one part of it.
        Assert.DoesNotContain("Your team may call this the **skull base**", flat, StringComparison.Ordinal);

        // The outcome comes FIRST in both: what the reader would notice precedes the
        // term. (The failure messages used to say the opposite of what is asserted,
        // which would have sent the next reader the wrong way — /review round 1.)
        Assert.True(
            flat.IndexOf("hearing loss or ringing in one ear", StringComparison.Ordinal)
                < flat.IndexOf("**skull base**", StringComparison.Ordinal),
            "the skull base entry names the word BEFORE it says what the reader would "
            + "notice; §12.6 is the other way round — say the outcome, then name the word");
        Assert.True(
            flat.IndexOf("Side vision is what usually goes first", StringComparison.Ordinal)
                < flat.IndexOf("**sellar**", StringComparison.Ordinal),
            "the pituitary entry names the word BEFORE it says what the reader would "
            + "notice; §12.6 is the other way round — say the outcome, then name the word");

        // AND THE CHILD HALF OF THE HORMONE LIST. /review round 1: the bullet said
        // "your energy, your weight, your periods and your thirst", and it composes
        // onto /tumors/pediatric-brain-tumor, which tells the reader in bold to read
        // "your" as "your child". "Your periods" resolved to nonsense for a parent,
        // and the pediatric presentation was missing outright. Sourced to the same
        // Endocrine Society page: "Growth failure in children", "Delayed puberty in
        // children".
        Assert.Contains("In a child, growth and puberty too.", flat, StringComparison.Ordinal);
        Assert.DoesNotContain("your periods", flat, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [MemberData(nameof(IncludingHubData))]
    public void EveryIncludingHubComposesTheNewMaterialAndNoneRetypesIt(string slug)
    {
        // §12.10, asserted in BOTH directions, which is the only shape that works here.
        //
        // THE TRAP THIS EXISTS TO AVOID, and it is live: a composed-page test asserting
        // "skull base" on /tumors/meningioma PASSES WITH AN EMPTY BLOCK, because that
        // page says the words itself. Same for "pituitary" on /tumors/craniopharyngioma
        // (17 mentions), /tumors/cns-germ-cell-tumor (9) and /tumors/acoustic-neuroma (2).
        // A property with two homes cannot be broken by a single-point mutation
        // (WI-549), and a block's prose has eighteen homes.
        var page = Hub(slug);
        var composed = CuratedPage.Flatten(CuratedPage.Composed(page, $"tumors/{slug}"));
        var flatBlock = CuratedPage.Flatten(Block);
        var flatPage = CuratedPage.Flatten(page);

        foreach (var needle in BlockOnlyNeedles)
        {
            Assert.Contains(needle, flatBlock, StringComparison.Ordinal);
            Assert.DoesNotContain(needle, flatPage, StringComparison.Ordinal);
            Assert.Equal(1, Regex.Matches(composed, Regex.Escape(needle)).Count);
        }
    }

    [Fact]
    public void TheBlocksNewSourcesReachTheReaderAndAreNotDoubledWhereAPageAlreadyCitesThem()
    {
        // §12.10's surface a front-matter check cannot see: block sources merge into
        // every including page and RENDER in the reader's source list.
        //
        // EXACTLY ONE of the eighteen includers already cites a matching URL —
        // /tumors/acoustic-neuroma cites the NIDCD page. /review round 1 corrected an
        // earlier claim of "three": /tumors/craniopharyngioma cites a DIFFERENT
        // Endocrine Society page (adrenal insufficiency), and /tumors/pituitary-tumor
        // cites the exact Endocrine URL but EXCLUDES the block, so neither can double.
        //
        // IT READS THE PAGE'S OWN FRONT MATTER, because /review round 2 found the
        // previous version COULD NOT FAIL. `ContentBlocks.Compose(...).Sources`
        // returns BLOCK sources only — the page's declared list is merged separately,
        // in ContentStore.cs, by SameSource. So the old per-hub loop was eighteen
        // identical repetitions of a check on one list that by construction holds each
        // URL once, and the comment claiming it pinned the dedupe was false. That is
        // this project's own named defect: A CHECK THAT CANNOT PASS IS NOT EVIDENCE
        // WHEN IT FAILS, and its mirror — one that cannot fail is not evidence when it
        // passes. The merge is exercised through ContentStore.Parse here (see below --
        // round 3 deleted an earlier replica of it), and a canary proves it can fire.
        string[] added =
        [
            "https://www.aans.org/patients/conditions-treatments/classification-of-brain-tumors/",
            "https://www.aans.org/patients/conditions-treatments/hydrocephalus/",
            "https://www.nidcd.nih.gov/health/vestibular-schwannoma-acoustic-neuroma-and-neurofibromatosis",
            "https://www.mskcc.org/cancer-care/types/skull-base-tumors",
            "https://www.endocrine.org/patient-engagement/endocrine-library/pituitary-tumors",
        ];

        // Hoisted out of the eighteen-iteration loop (/review round 1 nit).
        var blocks = ContentBlockStore.Load(CuratedPage.BlocksRoot);
        var blockSources = blocks.Blocks["mechanism"].Sources;
        foreach (var url in added)
        {
            Assert.Contains(blockSources, s => string.Equals(s.Url, url, StringComparison.Ordinal));
        }

        // Normalised AFTER the query string is cut, not before — `.../page/?utm=1`
        // trimmed first keeps its slash and never matches the bare URL, which is how
        // the round-1 version of this check was quietly inert (/review round 2).
        static string Normalise(string url) => url.Split('?')[0].TrimEnd('/').ToLowerInvariant();

        // THROUGH THE REAL PARSER, not a replica of it. /review round 3: round 2's fix
        // hand-rolled ContentStore's merge and a regex YAML reader, which is the thing
        // CuratedPage.Composed's own doc comment forbids — "a private copy of the
        // include rules would drift from the one the site actually runs". The replica
        // had already diverged: SameSource falls back to the TITLE when either URL is
        // blank, so a URL-less print source dedupes in production and would not have
        // here. ContentStore.Parse returns the merged list the reader gets.
        static List<string> ReaderSources(string page, string slug, ContentBlockSet blocks) =>
            [.. ContentStore.Parse(page, $"tumors/{slug}", [], blocks).FrontMatter.Sources.Select(s => s.Url)];

        // THE CANARY: prove the guard can say no before trusting it to say yes. A hub
        // citing one of these with a trailing slash is a near-miss the exact-match
        // dedupe does NOT catch — both survive into the reader's list — while Normalise
        // sees one source. That gap is what the per-hub assertion below closes. Built
        // by really parsing a really-modified page, so the canary tests the same code
        // path the assertion does.
        var poisoned = Hub("glioma").Replace(
            "sources:", $"sources:\n  - url: {added[2]}/\n    title: \"canary\"", StringComparison.Ordinal);
        var canary = ReaderSources(poisoned, "glioma", blocks);
        Assert.Equal(2, canary.Count(u => Normalise(u) == Normalise(added[2])));
        Assert.Equal(1, canary.Count(u => string.Equals(u, added[2], StringComparison.OrdinalIgnoreCase)));

        foreach (var slug in IncludingHubs)
        {
            var merged = ReaderSources(Hub(slug), slug, blocks);

            foreach (var url in added)
            {
                Assert.Equal(1, merged.Count(u => string.Equals(u, url, StringComparison.OrdinalIgnoreCase)));

                // And not doubled by a near-miss the exact-match dedupe let through.
                Assert.Equal(1, merged.Count(u => Normalise(u) == Normalise(url)));
            }
        }
    }

    [Fact]
    public void ThePagesThatScopeTheBlockByNamingAnEntryStillAgreeWithTheBlock()
    {
        // WI-549's lesson, applied to the thing that caused it: A COMMENT OR A NOTE
        // THAT DISAGREES WITH ITS OWN PAGE IS A DEFECT REPORT NOBODY HAS READ.
        //
        // FIVE live pages scope [MECHANISM] by naming what is IN its map or by
        // restating its rule. This edit made two of those notes FALSE, one incomplete,
        // one unhedged where the block is now hedged, and left one page inheriting a
        // bullet that is right for the other seventeen hubs and wrong for it. Each is
        // pinned here against the block clause that makes it true, so a later block
        // rewrite turns the note into a failing test rather than a silent lie on a
        // reader-facing page.
        //
        // THE COUNT WENT 3 -> 4 -> 5 ACROSS THREE REVIEW ROUNDS, and this header was
        // left saying FOUR until round 5. A stale blast-radius count is the exact
        // mistake that opened this item (the backlog's "~20" for a block on 18 pages),
        // committed inside the file that exists to stop it. Recorded rather than
        // quietly corrected.
        //
        // IT WAS THREE UNTIL /review ROUND 1. The search that found them looked for
        // notes that NAMED AN ENTRY, and /tumors/all-brain-tumors does not — it
        // restates the RULE, in its own words, unhedged, two lines above the directive,
        // on the hub written for the PRE-DIAGNOSIS reader. That is the reader most
        // likely to have the obstructive presentation this whole item is about, and
        // the page primed them with the flat version before the block's exception
        // could reach them. Looking for one shape of collision is how the worst one
        // gets missed.
        var flatBlock = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        // /tumors/all-brain-tumors restated the rule as "Where a thing sits in the
        // brain decides what you feel" — flatly, and the block no longer does.
        var allTumors = CuratedPage.Flatten(CuratedPage.ReaderText(Hub("all-brain-tumors")));
        Assert.Contains("Where a thing sits in the brain usually decides what you feel",
            allTumors, StringComparison.Ordinal);
        Assert.Contains("it names the one exception", allTumors, StringComparison.Ordinal);
        Assert.DoesNotContain("Where a thing sits in the brain decides what you feel",
            allTumors, StringComparison.Ordinal);
        Assert.Contains("There is one exception, and it is the fluid", flatBlock, StringComparison.Ordinal);

        // /tumors/craniopharyngioma used to say the map "will not describe this one".
        // It is a sellar tumor, so the pituitary entry made that false — and worse,
        // it steered the reader PAST the one entry that was theirs. The pointer names
        // BOTH the bullet's lead words and the term, because a reader scans one or the
        // other and §12.6 puts the term at the END of the bullet (/review round 1).
        var cranio = CuratedPage.Flatten(CuratedPage.ReaderText(Hub("craniopharyngioma")));
        Assert.Contains("the entry to read is the one about behind the eyes, where the pituitary sits",
            cranio, StringComparison.Ordinal);
        Assert.DoesNotContain("so it will not describe this one", cranio, StringComparison.Ordinal);
        Assert.Contains("- **Behind the eyes, at the base of the brain.**", flatBlock, StringComparison.Ordinal);

        // /tumors/cns-germ-cell-tumor used to say the list "does not include the deep
        // middle, where these tumors grow". It does now.
        var germCell = CuratedPage.Flatten(CuratedPage.ReaderText(Hub("cns-germ-cell-tumor")));
        // TWO entries, not one. /review round 2: this tumor has two homes — the deep
        // middle AND the suprasellar spot, which is where its thirst, growth and
        // puberty material comes from — and bullet 9 covers the second. Saying "the
        // one" was false in the same way craniopharyngioma's sentence had been.
        Assert.Contains("two entries are about where these grow: the deep middle one, and the one about behind the eyes",
            germCell, StringComparison.Ordinal);
        Assert.DoesNotContain("does not include the deep middle", germCell, StringComparison.Ordinal);
        Assert.Contains("- **Behind the eyes, at the base of the brain.**", flatBlock, StringComparison.Ordinal);

        // AND THE PAGE RECONCILES ITS OWN DEEP PARAGRAPH WITH THE BULLET. /review
        // round 1: this page says a deeper tumor "tends to cause weakness on one side
        // of the body", ten lines before a bullet that leads on pressure. Both are
        // true and the bullet is hedged ("often ... rather than"), but the reader
        // needed telling which is which — and the weakness comes from the OFF-TO-ONE-
        // SIDE spot rather than the middle, which round 2 corrected.
        // "the spot off to one side instead", NOT "the less common spot": this page's
        // AssertNoWarningSignIsNormalised guard fires on a paragraph that names a
        // warning sign and then reassures without a tier, and "less common" beside
        // "weakness" is exactly that shape. The guard was right and the prose changed.
        Assert.Contains("which comes from the spot off to one side instead",
            germCell, StringComparison.Ordinal);

        // /tumors/cns-lymphoma is the FIFTH, and it was missed twice. Round 1 left it
        // on the argument that hedging the bullet to "often" resolved it; round 2
        // showed it did not. This page says it grows "deep, near the fluid spaces in
        // the middle of the brain" — the bullet's own words — and then says the first
        // changes are in THINKING, because PCNSL is not an obstructive presentation.
        // Near-identical location, opposite "often", fourteen lines apart. The bullet
        // is right for the hubs it was written from and wrong for this one, which is
        // §12.10's whole test, so the PAGE scopes it rather than the block hedging
        // further.
        var lymphoma = CuratedPage.Flatten(CuratedPage.ReaderText(Hub("cns-lymphoma")));
        Assert.Contains("Its deep middle entry starts with pressure", lymphoma, StringComparison.Ordinal);
        Assert.Contains("This one does not usually start that way", lymphoma, StringComparison.Ordinal);
        // AND IT RESTORES THE TIER IT NARROWED. /review round 3: the first draft
        // narrowed pressure and stopped, on a page that says pressure signs are common
        // and that uses narrow-then-restore two paragraphs above for seizures.
        Assert.Contains("Pressure can still come later", lymphoma, StringComparison.Ordinal);
        Assert.Contains("A tumor here can block the flow", flatBlock, StringComparison.Ordinal);
        Assert.Contains("- **Deep in the middle, near the spaces the fluid runs through.**",
            flatBlock, StringComparison.Ordinal);

        // /tumors/acoustic-neuroma pointed at the CEREBELLUM entry as the closest fit.
        // A vestibular schwannoma is a skull-base growth, so after this edit there is
        // a nearer entry and the note names it first. The other two sentences stand.
        var acoustic = CuratedPage.Flatten(CuratedPage.ReaderText(Hub("acoustic-neuroma")));
        Assert.Contains("the entry about the skull base, below and behind the ear, is the closest fit",
            acoustic, StringComparison.Ordinal);

        // AND IT IS SCOPED, not endorsed. /review round 2's blocker: this page says a
        // few lines earlier that facial weakness is unusual here and is "what people
        // fear", and the skull base entry lists weakness down one side of the face.
        // An unscoped "closest fit" handed the reader the one symptom the page had
        // just talked them down from — the exact defect /review scoped the BRAINSTEM
        // sentence for, reintroduced by the fix that promoted a new entry above it.
        Assert.Contains("One thing in it is uncommon here: facial weakness, which this growth usually causes only once it is large",
            acoustic, StringComparison.Ordinal);
        Assert.Contains("numbness or weakness down one side of the face", flatBlock, StringComparison.Ordinal);
        // "this growth's territory", not "its" — the nearest antecedent of "its" was
        // the cerebellum entry, so the sentence read as the entry being its own
        // territory (/review round 2).
        Assert.Contains("The cerebellum entry is this growth's territory too", acoustic, StringComparison.Ordinal);
        Assert.Contains("- **The floor of the skull, below and behind the ear.**", flatBlock, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePituitaryHubStillExcludesTheBlockAndItsStatedReasonStillHolds()
    {
        // The exclusion is NOT reopened by this item. The block still opens "a tumor in
        // the brain" and still asserts seizures and blocked fluid needing a shunt, and a
        // pituitary tumor is none of those. What WI-568 changed is that one of the
        // note's stated reasons — that this tumor's anatomy was "absent from the block
        // entirely" — became false when the sellar entry landed. The clause was deleted
        // rather than left to rot.
        var pituitary = CuratedPage.Read("tumors", "pituitary-tumor.md");

        Assert.DoesNotMatch(new Regex(@"^\[MECHANISM\]", RegexOptions.Multiline), pituitary);
        Assert.DoesNotContain("is absent from the block entirely", pituitary, StringComparison.Ordinal);
        Assert.Contains("MECHANISM -- OUT", pituitary, StringComparison.Ordinal);

        // The reasons that DO still hold, pinned against the block rather than trusted.
        var flatBlock = CuratedPage.Flatten(CuratedPage.ReaderText(Block));
        Assert.Contains("a tumor in the brain", flatBlock, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("It can set off seizures", flatBlock, StringComparison.Ordinal);
        Assert.Contains("the usual treatment is a **shunt**", flatBlock, StringComparison.Ordinal);
    }

    /// <summary>
    /// THE HANDOVER, SPENT AND REPLACED RATHER THAN DELETED.
    ///
    /// WI-568 left this as <c>TheMeningiomaCollisionIsRecordedBecauseWI569OwnsIt</c>,
    /// asserting that the duplication STILL EXISTED so that it would go red the day
    /// WI-569 resolved it. It did, which is the handover working. What replaces it is
    /// the opposite assertion, for the same reason WI-567 replaced WI-568's deferred
    /// door rather than deleting it: the tripwire proved the duplication was OWED a
    /// fix, and this proves the fix is THERE.
    ///
    /// WHAT THE DUPLICATION WAS. The block gained the pituitary/sellar, the ventricles
    /// and the skull base at WI-568. <c>/tumors/meningioma</c> already covered all
    /// three in its own eight-address list, forty lines above where the block composes
    /// — so one composed page said three things twice, in different words. The fluid
    /// entry was the worst of the three: it stated the blocked-drainage case as an
    /// ordinary location-to-symptom row, which is the rule WI-568 shipped an
    /// EXCEPTION for, so the page's own list contradicted the block it composes.
    ///
    /// READ OFF THE COMPOSED PAGE, because that is where the duplication was visible
    /// and the raw page cannot show it (§12.10, WI-514).
    /// </summary>
    [Fact]
    public void TheMeningiomaCollisionIsResolvedAndEachRegionIsStatedOnce()
    {
        var composed = CuratedPage.Flatten(
            CuratedPage.ReaderText(CuratedPage.Composed(Hub("meningioma"))));
        var flatBlock = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        // 1. THE THREE ADDRESSES ARE STILL THERE — the fix was not to delete the
        //    reader's word for where their tumor is. Each now carries the report's
        //    word instead of a symptom the block states forty lines below.
        var addresses = new[]
        {
            ("- **Near the pituitary and the crossing of the optic nerves.**", "tuberculum sellae"),
            ("- **Inside the fluid spaces of the brain.**", "intraventricular"),
            ("- **At the back, near the brainstem and the nerves to the face and ear.**", "posterior"),
        };
        foreach (var (entry, word) in addresses)
        {
            Assert.Contains(entry, composed, StringComparison.Ordinal);
            var from = composed.IndexOf(entry, StringComparison.Ordinal) + entry.Length;
            var to = composed.IndexOf("- **", from, StringComparison.Ordinal);
            Assert.True(to > from, $"the entry '{entry}' is no longer followed by another");
            Assert.Contains(word, composed[from..to], StringComparison.OrdinalIgnoreCase);
        }

        // THE FLOOR, AND THE FIRST VERSION OF IT WAS A TAUTOLOGY. It asserted
        // `addresses.Length == 3` — the length of an array literal three lines above,
        // which is a comment wearing an assertion's clothes. §12.17's floors are
        // about how much of the PAGE was read. This one is.
        Assert.True(composed.Length > 40_000,
            $"the composed page is {composed.Length} characters, so this proves little");

        // 2. AND THE SYMPTOM EACH ONE USED TO CARRY IS GONE FROM THE PAGE, while the
        //    block's version of it is still there. Scoped to the whole composed page
        //    rather than to the entry, because a guard scoped to the paragraph the
        //    defect was in is green where the next one arrives (§12.17).
        var movedToTheBlock = new[]
        {
            ("Losing parts of your field of vision", "Side vision is what usually goes first"),
            ("A build-up of fluid and pressure", "what you notice is often the pressure rising"),
            ("Facial pain or numbness, facial weakness, hearing loss",
             "hearing loss or ringing in one ear"),
        };
        foreach (var (gone, itsHomeInTheBlock) in movedToTheBlock)
        {
            Assert.DoesNotContain(gone, composed, StringComparison.Ordinal);
            Assert.Contains(itsHomeInTheBlock, flatBlock, StringComparison.Ordinal);
        }

        // 2b. AND THE SPHENOID WING IS THE COUNTER-EXAMPLE, PINNED, because /review
        //     round 1 found this item stripping that entry's symptoms on the reasoning
        //     that the block covered them — while the block covers double vision under
        //     BRAINSTEM and facial numbness under THE FLOOR OF THE SKULL, neither of
        //     which is where a sphenoid-wing reader would look. Worse, the entry used
        //     to open "Behind the eyes", which is the opening of the block's PITUITARY
        //     bullet, so the routing sentence this item added sent that reader to a
        //     paragraph about hormones, periods and puberty.
        //
        //     THE RULE, because WI-570 will meet it on every hub: a give-away the
        //     block carries for a DIFFERENT address is one the block cannot say for
        //     THIS one.
        //     FLATTENED, because the first version of this pin ended mid-sentence at
        //     the markdown's hard wrap — so reflowing the bullet would have turned it
        //     red with the prose unchanged. §12.8/WI-526: a line wrap between two
        //     words defeats a phrase guard structurally, and no vocabulary fixes it.
        Assert.Contains(
            "**On the wing of bone behind the eye socket.** Loss of vision, double vision, "
            + "numbness in the face, or an eye that slowly starts to bulge.",
            CuratedPage.Flatten(CuratedPage.Read("tumors", "meningioma.md")),
            StringComparison.Ordinal);
        Assert.DoesNotContain("**Behind the eyes, on the wing of bone", composed,
            StringComparison.Ordinal);
        Assert.Contains("**Behind the eyes, at the base of the brain.**", flatBlock,
            StringComparison.Ordinal);

        // 3. THE TERM THE READER CARRIES TO AN APPOINTMENT NOW HAS ONE DEFINITION.
        //    The page said "an umbrella word for the ones growing on the floor of the
        //    skull and the ridge behind the eyes"; the block says the whole floor.
        //    Those agreed, but they were two definitions of one word on one composed
        //    page. The page keeps the half only it can say — which of ITS addresses
        //    sit under the umbrella — and routes for the meaning.
        Assert.Contains("Your team may call the whole floor of the skull the **skull base**",
            flatBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("for the ones growing on the floor of the skull",
            composed, StringComparison.Ordinal);
        Assert.Contains("(/where-your-tumor-is#skull-base)", composed, StringComparison.Ordinal);
    }

    /// <summary>
    /// WI-568 deferred a route to `/where-your-tumor-is` because that page did not
    /// exist and §12.8 bars linking to one that does not: the link would have been
    /// eighteen dead links at once. The obligation was carried by a test
    /// (<c>TheRouteToTheLocationPageIsOwedAndNotYetWritten</c>) that went RED the day
    /// the page shipped, which is WI-554's rehab-door shape.
    ///
    /// WI-567 shipped the page, so that tripwire is spent. This is what replaces it,
    /// and it is deliberately NOT a deletion. The tripwire proved the door was OWED;
    /// this proves it is THERE, in both directions, and a later edit that tidies the
    /// block's closing paragraph away cannot do it silently.
    ///
    /// The loose slug match is kept from the tripwire, for the same reason it was
    /// written loosely: the assertion is about the obligation, not about the exact
    /// word WI-567 chose.
    /// </summary>
    [Fact]
    public void TheRouteToTheLocationPageIsWrittenNowThatThePageExists()
    {
        var page = CuratedPage.AllPages()
            .FirstOrDefault(p => p.Slug.Contains("where-your-tumor", StringComparison.Ordinal)
                              || p.Slug.Contains("where-it-is", StringComparison.Ordinal)
                              || p.Slug.Contains("tumor-location", StringComparison.Ordinal));

        Assert.False(page.Slug is null,
            "the location page has been renamed or removed; blocks/mechanism.md links to "
            + "/where-your-tumor-is from its closing paragraph and that link is now dead");

        // The door itself, and the sentence that makes it a door rather than a bare
        // URL. WI-568's lesson: a needle must be a phrase only the BLOCK says, so the
        // wording is asserted against the block and its absence against the page the
        // reader is sent to, which must not carry the same sentence back.
        Assert.Contains("[Where your tumor is, and what that changes](/where-your-tumor-is)",
            Block, StringComparison.Ordinal);
        Assert.Contains("The other half of the question is", CuratedPage.Flatten(Block),
            StringComparison.Ordinal);
        Assert.DoesNotContain("The other half of the question is",
            CuratedPage.Flatten(page.Text), StringComparison.Ordinal);

        // And the route runs the other way too, so neither half is a dead end.
        // Scoped to the sentence, because "/tumors/all-brain-tumors" has four homes
        // on that page and a bare Contains() cannot be broken by any single edit
        // (WI-549's rule, applied to the test that quotes it).
        Assert.Contains(
            "[what a tumor in each place tends to cause](/tumors/all-brain-tumors#where-is-this-coming-from)",
            CuratedPage.Flatten(page.Text), StringComparison.Ordinal);
    }
}
