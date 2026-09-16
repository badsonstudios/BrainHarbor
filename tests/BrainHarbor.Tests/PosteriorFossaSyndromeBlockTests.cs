using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-537: <c>Content/blocks/posterior-fossa-syndrome.md</c>, the change that can
/// follow surgery low at the back of the brain.
///
/// WI-536 wrote it page-local on <c>/tumors/ependymoma</c>;
/// <c>/tumors/medulloblastoma</c> was the second hub to need the same paragraphs,
/// which is §12.8's "factor at the SECOND use" moment. Like
/// <see cref="SpinalCordBlockTests"/> it is a SCOPED block — included only by hubs
/// whose surgery is low at the back of the brain, not by every hub — and it is
/// included inside each hub's "what is treatment actually like" section, under that
/// hub's own <c>###</c> heading.
///
/// THE CARVE-OUT IS WHY THIS IS A BLOCK RATHER THAN TWO COPIES. The shared
/// [ESCALATION] block, composed higher up every including page, files "Suddenly not
/// being able to speak" as an AMBULANCE call. A section describing a syndrome whose
/// first sign is exactly that has to say the ambulance rule still stands, or its own
/// at-home tier undercuts it — WI-536 `/review` round 2's blocker, and WI-512's
/// defect in the dangerous direction. Every page that includes this gets the
/// carve-out for free.
///
/// These tests are about the BLOCK: its timing, its tiers, its attribution, and
/// where every including page puts it. The per-hub lead-in sentences stay with the
/// hubs.
/// </summary>
public sealed class PosteriorFossaSyndromeBlockTests
{
    /// <summary>
    /// Every hub that includes the block, stated rather than discovered, so a new
    /// includer has to be added here and its placement read (§12.10: read the tiers
    /// against the tumor before including). WI-538's pediatric hub is the likely third.
    /// </summary>
    private static readonly string[] IncludingHubs = ["ependymoma", "medulloblastoma"];

    /// <summary>The same list for the theory, so a hub added above cannot miss the position check.</summary>
    public static TheoryData<string> IncludingHubData => [.. IncludingHubs];

    private const string StJudeUrl =
        "https://together.stjude.org/en-us/treatment-tests-procedures/symptoms-side-effects/posterior-fossa-syndrome.html";

    private const string StatPearlsUrl = "https://www.ncbi.nlm.nih.gov/books/NBK538244/";

    /// <summary>The subsection heading anchor both hubs hang the block under.</summary>
    private const string Anchor = "{#posterior-fossa-syndrome}";

    /// <summary>
    /// The medulloblastoma cohort study is deliberately NOT a source of this block, and
    /// this pins that decision.
    ///
    /// Rendered read 1 found that on /tumors/medulloblastoma the only support for the
    /// complication sentence was a StatPearls chapter titled "Ependymoma", so the cohort
    /// study was added here. /review round 1 showed that trades one hub's confusion for
    /// the other's: /tumors/ependymoma would render a source titled "...in Survivors of
    /// Childhood Medulloblastoma" supporting no sentence its reader can find. Moving the
    /// impairment sentence into the block instead was rejected, because that finding is
    /// medulloblastoma survivors and the block asserts its prose on every including hub
    /// (§12.10). The citation is page-local, where the page needs it anyway.
    /// </summary>
    private const string PageLocalCohortUrl = "https://pmc.ncbi.nlm.nih.gov/articles/PMC12921523/";

    /// <summary>
    /// The cerebellum-mutism narrative review: the one cited source for the complication
    /// sentence whose TITLE names no tumor type, so it is true on every including hub.
    /// </summary>
    private const string NeutralUrl = "https://pmc.ncbi.nlm.nih.gov/articles/PMC9856273/";

    private static string Block =>
        File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "posterior-fossa-syndrome.md"));

    private static string Hub(string slug) => CuratedPage.Read("tumors", $"{slug}.md");

    /// <summary>The block's blank-line-bounded paragraph that starts with <paramref name="opening"/>, flattened.</summary>
    private static string Paragraph(string opening)
    {
        var body = CuratedPage.ReaderText(Block).Replace("\r\n", "\n");
        var at = body.IndexOf(opening, StringComparison.Ordinal);
        Assert.True(at >= 0, $"the block has no paragraph starting '{opening}'");
        var end = body.IndexOf("\n\n", at, StringComparison.Ordinal);
        return CuratedPage.Flatten(end < 0 ? body[at..] : body[at..end]);
    }

    [Fact]
    public void ExactlyTheListedHubsIncludeIt()
    {
        var including = Directory
            .EnumerateFiles(Path.Combine(CuratedPage.PagesDirectory, "tumors"), "*.md")
            .Where(f => ContentBlocks.DirectBlockNames(File.ReadAllText(f)).Contains("posterior-fossa-syndrome"))
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(IncludingHubs.Order(StringComparer.Ordinal), including);
    }

    [Theory]
    [MemberData(nameof(IncludingHubData))]
    public void AnIncludingHubPutsItUnderItsOwnSubheadingInsideTheTreatmentSection(string slug)
    {
        // POSITION is the property (WI-512). The block carries a tier, and a tier that
        // drifts out of the section describing the operation reads as unrelated to it.
        var lines = Regex.Split(Hub(slug), @"\r?\n");
        var directive = Array.FindIndex(lines, l => l.Trim() == "[POSTERIOR-FOSSA-SYNDROME]");
        Assert.True(directive >= 0, $"{slug} has lost [POSTERIOR-FOSSA-SYNDROME]");

        var before = lines[..directive];

        // Its own `###` subsection, carrying the anchor, so the glossary entry and any
        // sibling page can deep-link the description rather than the page top.
        var subheading = before.Last(l => l.StartsWith("### ", StringComparison.Ordinal));
        Assert.Contains(Anchor, subheading, StringComparison.Ordinal);

        // And that subsection sits in the section about what treatment is like (§12.3
        // slot 8), not in the symptoms section: this is a complication of the operation,
        // not a presenting sign.
        var section = before.Last(l => l.StartsWith("## ", StringComparison.Ordinal));
        Assert.StartsWith("## What is treatment actually like, and what is normal afterwards?",
            section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCarveOutKeepsTheAmbulanceTierTheEscalationBlockSets()
    {
        // §12.10, read from the sibling rather than hard-coded: the carve-out is only
        // correct while [ESCALATION] really does file a sudden loss of speech as an
        // ambulance call. If that block is ever re-tiered, this fails here rather than
        // leaving two blocks quietly disagreeing on one sign.
        var escalation = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.EscalationBlock));
        Assert.Contains("Suddenly not being able to speak, move one side, or see.",
            escalation, StringComparison.Ordinal);

        var carveOut = Paragraph("**Suddenly not being able to speak");

        Assert.StartsWith(
            "**Suddenly not being able to speak is on the ambulance list above, and that rule still stands.**",
            carveOut, StringComparison.Ordinal);

        // It says where the watching happens, so the carve-out does not read as "call an
        // ambulance from the ward" — and it HEDGES that, which /review round 2 caught the
        // factoring dropping. WI-536's page-local wording was "usually starts in
        // hospital"; unhedged it contradicted the block's own next paragraph, which
        // exists precisely because onset is not always witnessed on the ward ("start or
        // get worse after you are home"). §12.10: a hedge in a block is scope, and the
        // scope widened when the block gained a second hub.
        Assert.Contains("usually starts in hospital", carveOut, StringComparison.Ordinal);
        Assert.Contains("where the team is watching for it", carveOut, StringComparison.Ordinal);

        // The carve-out comes BEFORE the at-home tier it protects. Reversed, a parent
        // meets the weaker instruction first, which is the order WI-536 found on the page.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Block));
        var stands = body.IndexOf("that rule still stands", StringComparison.Ordinal);
        var atHome = body.IndexOf("after you are home", StringComparison.Ordinal);
        Assert.True(stands > 0 && atHome > stands,
            "the ambulance carve-out must come before the at-home tier");
    }

    [Fact]
    public void TheAtHomeTierNamesEverySignTheBlockListsAndCarriesTheShuntRule()
    {
        // WI-536 `/review` round 3: the tier named three of the four signs the block's
        // own first paragraph lists, stranding "behavior" with no instruction. The signs
        // are read OUT OF that paragraph here, so narrowing either one fails.
        var opening = Paragraph("**A day or two after surgery");
        var tier = Paragraph("It is frightening to see.");

        var listed = Regex.Match(opening, @"It can also change ([^.]+)\.");
        Assert.True(listed.Success, "the block no longer lists what else can change");

        var signs = listed.Groups[1].Value
            .Replace(" and ", ", ", StringComparison.Ordinal)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.True(signs.Length >= 4, $"only {signs.Length} signs listed, so this test checks almost nothing");

        foreach (var sign in signs)
        {
            Assert.Matches(new Regex($@"\b{Regex.Escape(sign)}\b"), tier);
        }

        // The tier itself, and the shunt clause every same-day line in the corpus owes
        // (WI-535's blocker): for a reader with a shunt the same-day tier is right-away.
        Assert.Contains(
            "call your team the same day, or right away if there is a shunt",
            tier, StringComparison.Ordinal);

        // "New or worse", not "new": WI-536 `/review` round 1 found "new" alone missed a
        // child who came home already struggling and then got worse.
        Assert.Contains("start or get worse after you are home", tier, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTimingAndTheRecoveryAreTheSourcesAndTheReassuranceIsNotTheAnswer()
    {
        var opening = Paragraph("**A day or two after surgery");
        var tier = Paragraph("It is frightening to see.");

        // St. Jude: "usually begin within 1-2 days after surgery. But symptoms always
        // start within the first week after surgery."
        Assert.Contains("A day or two after surgery low at the back of the brain, some children stop talking",
            opening, StringComparison.Ordinal);
        Assert.Contains("It always starts within the first week", opening, StringComparison.Ordinal);

        // StatPearls: "A well-known complication of posterior fossa surgery". WI-536
        // `/review` round 1: "it is not the tumor coming back" was in no cited source,
        // and a reassurance needs a source as much as a warning does.
        Assert.Contains("Doctors know it as a complication of the operation itself",
            tier, StringComparison.Ordinal);
        Assert.DoesNotContain("tumor coming back", CuratedPage.ReaderText(Block), StringComparison.Ordinal);

        // The honest recovery shape: mostly better, sometimes months, sometimes years.
        // Dropping the long tail is the over-reassuring direction.
        foreach (var required in new[]
                 {
                     "**Most children slowly get better.**",
                     "Speech usually comes back over days or weeks, and for some it takes months",
                     "Most children walk on their own again",
                     "can last longer, even years",
                     "Speech therapy can help",
                 })
        {
            Assert.Contains(required, CuratedPage.Flatten(CuratedPage.ReaderText(Block)), StringComparison.Ordinal);
        }

        // And the reassurance is not offered as the answer to the warning signs: the
        // paragraph that reassures is the paragraph that carries the tier (WI-536's
        // normalisation guard found reassurance standing in for an instruction).
        Assert.Contains("same day", tier, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBlockCarriesNoFrequencyFigure()
    {
        // The sources' figures for this tumor family run from about 8% to 40%, and each
        // including hub says in its own words how common it is for its own tumor
        // (§12.4 R2). A number in a block is that number on every hub (§12.10).
        var frequency = new Regex(
            @"\d\s*%|\b\d+\s+(in|out of)\s+\d+\b|\bin (four|three|five)\b|percent",
            RegexOptions.IgnoreCase);

        Assert.Matches(frequency, "it happens in 1 in 4 children"); // canary: the guard can fire
        Assert.DoesNotMatch(frequency, CuratedPage.ReaderText(Block));
    }

    [Fact]
    public void EveryIncludingHubComposesItAndNoneRetypesIt()
    {
        // §12.10: assert something ONLY the block says, both directions. The deletion
        // half catches a block that stopped saying it; the duplication half catches a
        // hub that started.
        string[] blockOnly =
        [
            "is on the ambulance list above, and that rule still stands",
            "Doctors know it as a complication of the operation itself",
            "**Most children slowly get better.**",
        ];

        foreach (var slug in IncludingHubs)
        {
            var page = Hub(slug);
            var composed = CuratedPage.Flatten(CuratedPage.Composed(page, $"tumors/{slug}"));

            foreach (var sentence in blockOnly)
            {
                Assert.Contains(sentence, CuratedPage.Flatten(Block), StringComparison.Ordinal);
                Assert.DoesNotContain(sentence, CuratedPage.Flatten(page), StringComparison.Ordinal);
                Assert.Equal(1, Regex.Matches(composed, Regex.Escape(sentence)).Count);
            }
        }
    }

    [Fact]
    public void TheBlocksSourcesReachTheReaderOnEveryIncludingHub()
    {
        // Block `sources` merge into the including page and RENDER in its source list
        // (§12.10). The citations moved off the ependymoma page with the paragraphs.
        var front = CuratedPage.FrontMatter(Block);
        Assert.Contains(StJudeUrl, front, StringComparison.Ordinal);
        Assert.Contains(StatPearlsUrl, front, StringComparison.Ordinal);

        // The tumor-neutral attribution (/review round 2). The StatPearls chapter is
        // titled "Ependymoma", and a block asserts its prose on every including hub, so
        // the sentence needs at least one source whose title is true of all of them.
        //
        // Asserted as a `- url:` ENTRY (/review round 3): a bare Contains would stay green
        // if the entry were deleted and the URL survived in one of this file's comments,
        // which is exactly the shape this block's front matter already contains for the
        // page-local cohort study. The reader's source list is what the check is about.
        Assert.Equal(1, Regex.Matches(front, Regex.Escape("- url: " + NeutralUrl)).Count);

        // The block does NOT carry the medulloblastoma cohort study: see the field
        // comment. A block source lands on every including hub, so this is the test that
        // stops it being re-added to fix one hub's source list at the other's expense.
        Assert.DoesNotContain(PageLocalCohortUrl, front, StringComparison.Ordinal);

        foreach (var slug in IncludingHubs)
        {
            var composed = ContentBlocks.Compose(
                Hub(slug), ContentBlockStore.Load(CuratedPage.BlocksRoot), $"tumors/{slug}");

            Assert.Contains(composed.Sources, s => s.Url == StJudeUrl);
            Assert.Contains(composed.Sources, s => s.Url == StatPearlsUrl);

            // /review round 3: the neutral source was checked in the front matter and not
            // here, so nothing asserted it actually REACHES either hub's rendered source
            // list, which is the entire reason it was added.
            Assert.Contains(composed.Sources, s => s.Url == NeutralUrl);
        }

        // And the hub that makes the impairment claim is the one that cites it, in its
        // OWN front matter, exactly once.
        //
        // Read from the front matter, NOT from Compose(...).Sources. That collection is
        // documented as "the sources contributed by the blocks that were pulled in" and
        // deliberately excludes the page's own, which the first version of this assertion
        // got wrong. Worth recording rather than just fixing: a test that reads the wrong
        // collection can as easily pass for the wrong reason, on a hub where some block
        // happens to cite the same URL.
        var medulloblastomaFront = CuratedPage.FrontMatter(Hub("medulloblastoma"));
        Assert.Equal(1, Regex.Matches(
            medulloblastomaFront, Regex.Escape("- url: " + PageLocalCohortUrl)).Count);

        // The other including hub does not carry it at all, which is the whole point of
        // keeping it page-local: its reader never meets a source titled for another tumor.
        Assert.DoesNotContain(PageLocalCohortUrl,
            CuratedPage.FrontMatter(Hub("ependymoma")), StringComparison.Ordinal);
    }

    [Fact]
    public void TheBlockHasNoHeadingSoItStaysInsideTheIncludingPagesSubsection()
    {
        // A `#` line in the block would end the including hub's subsection at the point
        // of composition, and every subsection-scoped check on both hubs would stop
        // seeing the paragraphs.
        Assert.DoesNotMatch(new Regex(@"^[ \t]*#{1,6}[ \t]", RegexOptions.Multiline),
            CuratedPage.ReaderText(Block));
    }

    [Fact]
    public void TheBlockUsesNoBritishFormsOrIdiom()
    {
        // One of its two sources is US and one is a reference work, but the WI-536
        // paragraphs it was factored from were written beside British sources.
        // Strip-then-scan, the stronger form (§12.10, WI-563).
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // Case-insensitive and word-bounded: WI-536's harness beat an Ordinal check with
        // a list item that began with a capital.
        foreach (var idiom in new[] { "A&E", "GP", "straight away", "out of hours", "being sick", "999" })
        {
            Assert.DoesNotMatch(new Regex($@"(?<![\w&]){Regex.Escape(idiom)}(?![\w&])", RegexOptions.IgnoreCase), reader);
        }
    }
}
