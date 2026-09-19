using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-536: <c>Content/blocks/spinal-cord.md</c>, the right-away rule for a tumor
/// IN the spinal cord.
///
/// WI-535 wrote it page-local on <c>/tumors/diffuse-midline-glioma</c>, after the
/// shared [ESCALATION] block, because that block files new weakness as same-day.
/// <c>/tumors/ependymoma</c> was the second hub to need it, which is §12.10's
/// moment: factor it rather than keep two copies. It is a SCOPED block, included
/// only by hubs whose tumor can sit in the cord, and not a conditional inside
/// escalation.md, which MeningiomaPageTests and BrainMetastasesPageTests pin free
/// of spinal words.
///
/// These tests are about the BLOCK: its tier, its source attribution, and where
/// every including page puts it. The rule's wording moved here from
/// DiffuseMidlineGliomaPageTests with it.
/// </summary>
public sealed class SpinalCordBlockTests
{
    /// <summary>
    /// Every hub that includes the block, stated rather than discovered, so a new
    /// includer has to be added here and its placement read (§12.10: read the
    /// tiers against the tumor before including).
    /// </summary>
    // WI-537: medulloblastoma joins them — it spreads through the fluid to the spine,
    // which is the reader the block's "or has spread to the spine" clause names.
    // WI-538: /tumors/pediatric-brain-tumor joins them, and it is the first includer
    // that is not a single tumor. Its first draft excluded the block on the grounds
    // that it "asserts a cord rule outright"; it does not, it opens with a conditional,
    // and §12.10 says a conditional survives the "hub you have thought about least"
    // test. Excluding it left a pre-diagnosis parent with the shared block's same-day
    // tier for signs this block files as right-away.
    // WI-544: /tumors/atrt is the fifth includer. It qualifies on the same CONDITIONAL
    // rule as the pediatric hub -- the block's opening clause names the reader whose
    // tumor is in the cord or has reached the spine, so it is false on nobody (§12.10).
    // It is included rather than left out because this tumor SEEDS THROUGH THE SPINAL
    // FLUID: one US hospital puts spread at diagnosis at roughly one in seven to one in
    // three, "usually to the CSF or the meninges", and about 4.6% of US cases arise in
    // the cord or cauda equina outright. Leaving it out would meet a parent whose child
    // has new cord signs with the shared block's SAME-DAY tier, where the corpus files
    // those signs as RIGHT AWAY -- the under-triage direction.
    private static readonly string[] IncludingHubs =
        ["atrt", "diffuse-midline-glioma", "ependymoma", "medulloblastoma", "pediatric-brain-tumor"];

    /// <summary>The same list for the theory, so a hub added above cannot miss the position check (/review round 1).</summary>
    public static TheoryData<string> IncludingHubData => [.. IncludingHubs];

    private const string CrukUrl =
        "https://www.cancerresearchuk.org/about-cancer/coping/physically/spinal-cord-compression";

    private static string Block => File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "spinal-cord.md"));

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
            .Where(f => ContentBlocks.DirectBlockNames(File.ReadAllText(f)).Contains("spinal-cord"))
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(IncludingHubs.Order(StringComparer.Ordinal), including);
    }

    [Theory]
    [MemberData(nameof(IncludingHubData))]
    public void AnIncludingHubPutsItStraightAfterTheEscalationBlockInTheSameSection(string slug)
    {
        // POSITION is the property (WI-512). It overrides a tier [ESCALATION] sets,
        // so it has to come after that list, and in the same `##` section: a rule
        // that drifts into another section reads as unrelated to the list.
        var lines = Regex.Split(Hub(slug), @"\r?\n");
        var escalation = Array.FindIndex(lines, l => l.Trim() == "[ESCALATION]");
        var spinal = Array.FindIndex(lines, l => l.Trim() == "[SPINAL-CORD]");

        Assert.True(escalation >= 0, $"{slug} has lost [ESCALATION]");
        Assert.True(spinal > escalation, $"{slug} must include [SPINAL-CORD] AFTER [ESCALATION]");

        // Nothing but blank lines between them, so no heading and no other prose
        // separates the override from the tier it overrides.
        Assert.All(lines[(escalation + 1)..spinal], l => Assert.True(l.Trim().Length == 0,
            $"{slug} puts '{l.Trim()}' between [ESCALATION] and [SPINAL-CORD]"));

        // And the section is the symptoms section (§12.9). WHICH SECTION is the
        // property; the exact wording is not. WI-538's cross-cutting hub is written to a
        // parent and heads it "What symptoms look like in a child", which is right for
        // its reader and is still the symptoms section. A literal-heading assertion would
        // have forced that page to either adopt a heading written for the patient or drop
        // the block, and dropping it is the under-triage direction (§12.10).
        var heading = lines[..escalation].Last(l => l.StartsWith("## ", StringComparison.Ordinal));
        Assert.Matches(new Regex(@"^## What symptoms\b", RegexOptions.IgnoreCase), heading);
    }

    [Fact]
    public void TheRulePutsEverySignFromTheCordInTheRightAwayTier()
    {
        var tier = Paragraph("**If the tumor is in the spinal cord");
        var signs = Paragraph("So are new trouble walking");

        // Scoped in its own first clause, before any symptom is named. /review round 1
        // blocker: a tumor that has SPREAD to the spine is addressed too. §12.10's
        // conditional rule is what makes that safe on both hubs — a conditional only has
        // to be true of the reader it names. (Round 3: an earlier version of this comment
        // said both including pages tell the reader the tumor can spread through the
        // fluid. /tumors/ependymoma does; /tumors/diffuse-midline-glioma does not.)
        Assert.StartsWith(
            "**If the tumor is in the spinal cord, or has spread to the spine, new signs from the cord are their own rule.**",
            tier, StringComparison.Ordinal);
        Assert.Contains("right-away call, not a same-day one", tier, StringComparison.Ordinal);

        // WI-535 rendered read 1: "newly weak" alone under-triaged "weakness that
        // keeps getting worse". Harness run 2 deleted this clause and nothing failed.
        Assert.Contains("weaker than it was", tier, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"\bnewly weak\b"), tier);

        // Every sign the source lists for pressure on the cord, as whole words, in
        // the sentence that puts them in the right-away tier.
        var soAre = CuratedPage.SentencesOf(signs).First();
        // /review round 1 blocker: neck pain was on the ependymoma page's spinal line and
        // in the source ("anywhere in your back, spine or neck") but not in the rule.
        foreach (var sign in new[] { @"\bwalking\b", @"\bnumbness\b", @"\bpain in the back or neck\b", @"\bbladder\b", @"\bbowel\b" })
        {
            Assert.Matches(new Regex(sign), soAre);
        }

        Assert.Contains("at any hour", signs, StringComparison.Ordinal);
        Assert.Contains("emergency department", signs, StringComparison.Ordinal);

        // §12.6, /review round 2: this block ends the whole when-to-call section on every
        // including page, displacing the escalation block's own landing. So it must not
        // end on the frightening half — the last sentence is the action.
        var last = CuratedPage.SentencesOf(signs).Last();
        Assert.Contains("Call your team right away, at any hour, or go to the emergency department.",
            last, StringComparison.Ordinal);
        Assert.DoesNotContain("emergency that needs treating quickly", last, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReasonIsAttributedToWhatItsSourceCovers()
    {
        // The source is about METASTATIC compression: attribute, do not assert it of
        // a tumor that started in the cord (WI-535 /review).
        var signs = Paragraph("So are new trouble walking");

        Assert.Contains("When cancer presses on the spinal cord, Cancer Research UK calls it an emergency",
            signs, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"(Pressure on|Compression of) the spinal cord is an emergency", RegexOptions.IgnoreCase),
            CuratedPage.Flatten(CuratedPage.ReaderText(Block)));
    }

    [Fact]
    public void TheBlockHasNoHeadingSoItStaysInsideTheSymptomsSection()
    {
        // A `#` line in the block would end the including page's symptoms section
        // at the point of composition, and the section-scoped tier checks would
        // stop seeing the rule.
        Assert.DoesNotMatch(new Regex(@"^[ \t]*#{1,6}[ \t]", RegexOptions.Multiline), CuratedPage.ReaderText(Block));
    }

    [Fact]
    public void EveryIncludingHubComposesTheRuleAndNoneRetypesIt()
    {
        // §12.10: assert something ONLY the block says, both directions.
        string[] blockOnly =
        [
            "right-away call, not a same-day one",
            "Cancer Research UK calls it an emergency",
            "new signs from the cord are their own rule",
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
    public void TheBlocksSourceReachesTheReaderOnEveryIncludingHub()
    {
        // Block `sources` merge into the including page and RENDER in its source
        // list (§12.10). The citation moved off the DMG page with the rule.
        Assert.Contains(CrukUrl, CuratedPage.FrontMatter(Block), StringComparison.Ordinal);

        foreach (var slug in IncludingHubs)
        {
            var composed = ContentBlocks.Compose(
                Hub(slug), ContentBlockStore.Load(CuratedPage.BlocksRoot), $"tumors/{slug}");
            Assert.Contains(composed.Sources, s => s.Url == CrukUrl);
        }
    }

    [Fact]
    public void TheStrengthSplitIsSettledAsTwoClaimsAndBothStrengthsArePinned()
    {
        // WI-536 DEFERRED THIS AS "ONE CLAIM, TWO STRENGTHS". WI-543 SETTLED IT, AND THE
        // ANSWER IS THAT IT WAS NEVER ONE CLAIM. Two claims wore one sentence, so levelling
        // the three hubs would have been as wrong as leaving them split:
        //
        //   * METASTATIC cord compression carries the strong rule. ACS tells patients to
        //     "call your doctor right away or go to the emergency room" and calls it "an
        //     oncology emergency"; OncoLink says "Call 911 or your care team right away".
        //   * A PRIMARY tumor in or beside the cord does not. NO patient-facing source
        //     found applies an emergency rule to a named primary cord tumor outside the
        //     cauda equina picture, and four that list the signs (AANS, ABTA, Columbia,
        //     NCI PDQ) give no urgency instruction at all. Those are recorded absences.
        //   * THE FAST CARVE-OUT APPLIES TO BOTH: new bladder or bowel trouble, saddle
        //     numbness, or weakness worsening over hours to days is emergency care now.
        //
        // The clincher sits inside ONE publisher: ACS says "right away or go to the
        // emergency room" for cord COMPRESSION and only "see a doctor" on its own spinal
        // cord TUMOR page. Evidence log: .claude/work_files/wi543/plan.md.
        //
        // SO THE HUBS ARE NOT LEVELLED. /tumors/meningioma keeps its wording, and that is a
        // RULING rather than an omission: a meningioma sits beside the cord, is
        // slow-growing, and its tier is what the sources support for that reader.
        // /tumors/spinal-cord-tumor now splits by scenario because it owns the subject and
        // serves both populations.
        const string SiblingTier =
            "New weakness or new bladder trouble is a reason to be seen quickly, not to wait";

        // THE TWO SIBLINGS NO LONGER CARRY ONE WORDING, AND THAT IS THE SETTLEMENT RATHER
        // THAN DRIFT. /review round 2: /tumors/meningioma is a PRIMARY tumor beside the
        // cord — the same population /tumors/spinal-cord-tumor tiers as same-day — and its
        // "seen quickly" sentence was also weaker than the [ESCALATION] block composing
        // directly beneath it. It is re-tiered. /tumors/brain-metastases is METASTATIC, its
        // passage opens on "that is its own emergency" and now closes on the emergency-room
        // route, so its wording stands.
        var meningioma = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "meningioma.md")));
        Assert.Matches(new Regex(
            @"New weakness or a\s+change in how you walk is a same-day call",
            RegexOptions.IgnoreCase), meningioma);
        Assert.DoesNotContain(SiblingTier, meningioma, StringComparison.Ordinal);

        var metastases = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "brain-metastases.md")));
        Assert.Contains(SiblingTier, metastases, StringComparison.Ordinal);

        // AND THE HUB THAT OWNS THE SUBJECT CARRIES THE SPLIT, BOTH HALVES AND THE
        // CARVE-OUT. Asserted as three separate properties, because a page that kept only
        // the reassuring half would be §12.12's dangerous direction and would still pass a
        // check for any one of them.
        var cord = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "spinal-cord-tumor.md")));

        Assert.Matches(new Regex(
            @"If your cancer has spread to your spine[^.]{0,120}"
            + @"calling your team\s+right away or going to the emergency room",
            RegexOptions.IgnoreCase), cord);
        // SAME-DAY, not "seen quickly": /review round 1 found the weaker wording was
        // strictly below the composed escalation block's own same-day tier for the same
        // sign, which recreated the very defect this test exists to close.
        Assert.Matches(new Regex(
            @"If your tumor started here[^.]{0,120}same-day call",
            RegexOptions.IgnoreCase), cord);
        Assert.Matches(new Regex(
            @"one picture is emergency care now, whichever kind of tumor you have",
            RegexOptions.IgnoreCase), cord);

        // NONE OF THE THREE REACHES THIS BLOCK, which is what keeps a reader from meeting
        // two strengths on one page. Asserted on the COMPOSED page rather than on direct
        // include names (/review round 4): blocks nest up to five levels, so a sibling
        // including a block that includes this one would leave a direct-name check green
        // while the reader met both tiers. This states the property, not the mechanism.
        foreach (var slug in new[] { "spinal-cord-tumor", "meningioma", "brain-metastases" })
        {
            var composed = CuratedPage.Flatten(
                CuratedPage.Composed(CuratedPage.Read("tumors", $"{slug}.md"), $"tumors/{slug}"));
            Assert.DoesNotContain("right-away call, not a same-day one", composed, StringComparison.Ordinal);
        }

        // The block's own tier, unchanged, so a change on either side fails here. WI-543
        // did NOT re-tier this block: it ships on four hubs, three of which reach the spine
        // by spread, and its own front matter records the over-triage there as knowing.
        Assert.Contains("right-away call, not a same-day one", CuratedPage.Flatten(Block), StringComparison.Ordinal);
    }

    [Fact]
    public void TheBlockUsesNoBritishFormsOrIdiom()
    {
        // Its only source is British. Strip-then-scan, the stronger form (§12.10).
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var idiom in new[] { "A&E", "GP", "straight away", "out of hours", "being sick", "999" })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.Ordinal);
        }
    }
}
