using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-563: <c>Content/blocks/escalation.md</c>, the ambulance / same-day
/// escalation list.
///
/// It was hand-copied byte-identical onto three hubs before this item, and the
/// fever sentence beneath it had already diverged between two of them
/// (high-grade said "explains why, and what number your team will give you",
/// astrocytoma said "explains why."). §12.8's "factor at the second use" was
/// three uses overdue with twenty-one hubs still to write.
///
/// These tests are about the BLOCK. The per-hub tier checks live in
/// <see cref="CuratedPage.AssertEscalationTiers"/>, which every including page
/// calls with its own section heading.
/// </summary>
public class EscalationBlockTests
{
    /// <summary>The hubs that include the block today. WI-517 and WI-518 join them.</summary>
    public static TheoryData<string> Hubs =>
        ["glioma", "low-grade-glioma", "high-grade-glioma", "astrocytoma"];

    /// <summary>The same list as plain strings, for the tests that loop rather than theorise.</summary>
    private static readonly string[] HubSlugs =
        ["glioma", "low-grade-glioma", "high-grade-glioma", "astrocytoma"];

    private static string Hub(string slug) => CuratedPage.Read("tumors", $"{slug}.md");

    private static string Block => CuratedPage.EscalationBlock;

    [Theory]
    [MemberData(nameof(Hubs))]
    public void AHubIncludesTheBlockRatherThanRetypingIt(string slug)
    {
        var page = Hub(slug);

        // The directive is a line whose ENTIRE content is [ESCALATION] —
        // anything less strict would match a Markdown link (ContentBlocks).
        Assert.Contains("escalation", ContentBlocks.DirectBlockNames(page));

        // The negative is the point of the item. The bullets are read out of
        // the block rather than re-typed here, so rewording one cannot
        // silently retire this check.
        //
        // Matched loosely — punctuation and emphasis stripped, lowercased —
        // because the fork to catch is a NEAR copy, not a byte copy. Somebody
        // re-typing "- A first-ever seizure." rather than pasting
        // "- A **first ever** seizure." has forked the block just as
        // completely, and an Ordinal comparison would wave it through.
        foreach (var line in CuratedPage.EscalationLines())
        {
            Assert.DoesNotContain(Loose(line), Loose(page), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheBlockOwnsSentencesNoIncludingPageRepeats()
    {
        // §12.10: if a test's subject is the block, assert something ONLY the
        // block says — otherwise emptying the block leaves the test green
        // because the words had drifted onto the page.
        //
        // Both directions. The deletion half catches a block that stopped
        // saying it; the duplication half catches a page that started.
        string[] blockOnly =
        [
            "somewhere you will find it at two in the morning rather than somewhere sensible",
            "Some things should not wait for the next appointment",
            "fever is its own rule",
            "is not an ambulance call",
        ];

        foreach (var sentence in blockOnly)
        {
            Assert.Contains(sentence, CuratedPage.Flatten(Block), StringComparison.Ordinal);

            foreach (var slug in HubSlugs)
            {
                var page = Hub(slug);
                Assert.DoesNotContain(sentence, CuratedPage.Flatten(page), StringComparison.Ordinal);
                Assert.Contains(
                    sentence,
                    CuratedPage.Flatten(CuratedPage.Composed(page, $"tumors/{slug}")),
                    StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void AHubThatRoutesIntoATreatmentCarriesThatTreatmentsSafetyRule()
    {
        // §12.11, mechanised. WI-515 was blocked for nearly shipping a page
        // that sent every reader into chemotherapy with no fever rule. WI-563
        // found that defect LIVE on two more: /tumors/glioma linked
        // /treatments/chemotherapy and contained the word "fever" zero times,
        // and /tumors/low-grade-glioma mentioned chemotherapy four times with
        // the word "fever" only inside a link label.
        //
        // This is why the fever lines are in the block rather than page-local.
        // Both are written as CONDITIONALS ("if you are having chemotherapy",
        // "in the weeks after brain surgery"), which is what lets them pass
        // §12.10's test: they are true on the hub you have thought about
        // least, and false on none of them.
        (string Route, string Owed, string Anchor)[] rules =
        [
            ("/treatments/chemotherapy", "fever is its own rule",
                "/treatments/chemotherapy#fever-rule"),
            ("/treatments/craniotomy", "after brain surgery, a fever is its own rule too",
                "/treatments/craniotomy"),
        ];

        var offenders = new List<string>();

        foreach (var (slug, text) in TumorHubs())
        {
            var composed = CuratedPage.Flatten(CuratedPage.Composed(text, slug));

            foreach (var (route, owed, anchor) in rules)
            {
                if (!text.Contains(route, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!Regex.IsMatch(composed, owed, RegexOptions.IgnoreCase)
                    || !composed.Contains(anchor, StringComparison.Ordinal))
                {
                    offenders.Add($"{slug} routes into {route} without '{owed}'");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "these tumor hubs route readers into a treatment without its safety rule — "
            + "include [ESCALATION] (§12.11):\n  " + string.Join("\n  ", offenders));
    }

    [Fact]
    public void TheBlockRoutesToTheThresholdRatherThanRepublishingIt()
    {
        // /treatments/chemotherapy publishes 100.4 °F, sourced, and is the one
        // place on the site that does. UMass gives 100 °F for post-craniotomy
        // fever and CRUK gives 37.5 °C for PCV — three different numbers in
        // this block's own source list, which is exactly why it must publish
        // none of them. Teams differ, the sibling says so, and it tells the
        // reader to use theirs.
        var reader = CuratedPage.ReaderText(Block);

        // Any temperature-shaped number, not just the three literals. "over
        // 101" and "above 38 degrees" both passed the first version of this.
        Assert.DoesNotMatch(new Regex(@"\d+(\.\d+)?\s*°"), reader);
        Assert.DoesNotMatch(new Regex(@"\d+(\.\d+)?\s*degrees", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"(temperature|fever)[^.]{0,40}\b\d", RegexOptions.IgnoreCase), reader);
        Assert.DoesNotMatch(
            new Regex(@"\b\d[^.]{0,40}(temperature|fever)", RegexOptions.IgnoreCase), reader);

        Assert.Contains("/treatments/chemotherapy#fever-rule", reader, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBlockUsesNoBritishForms()
    {
        // WI-511's gate, run on the file with the widest blast radius. Three of
        // this block's sources are British (Cancer Research UK twice), and
        // their wording is where "advice line" and "A&E" would arrive from.
        //
        // The BODY, not the file: two source URLs contain "brain-tumours", so a
        // gate run over the raw file fails on a citation nobody reads as prose.
        //
        // Exemptions are STRIPPED and then the text is scanned — not skipped
        // with `continue`, which is the weaker of the two implementations in
        // the repo (RadiationTherapyPageTests strips; the astrocytoma and
        // high-grade copies skip). Skipping disables the whole check for a form
        // if any exemption phrase appears anywhere in the file, so one mention
        // of "The Brain Tumour Charity" would switch the `tumour` check off for
        // every hub's worth of block prose. §12.12: when a guard is copied, its
        // holes are copied too.
        var reader = CuratedPage.Flatten(CuratedPage.ReaderText(Block));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            reader = Regex.Replace(reader, Regex.Escape(exemption), "", RegexOptions.IgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, reader, StringComparison.OrdinalIgnoreCase);
        }

        // The idioms this block's own sources would have supplied. WI-563
        // shipped "Being sick over and over" in its first draft — which in US
        // English reads as "being unwell", not "vomiting", on a line that is a
        // same-day escalation trigger.
        foreach (var idiom in new[]
                 {
                     "advice line", "A&E", "being sick", "out of hours",
                     "straight away", "straight after", "come round",
                 })
        {
            Assert.DoesNotContain(idiom, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheAmbulanceTierIsNotSoftenedIntoTheSameDayTier()
    {
        // Position, not presence (WI-512). Each of these belongs above the
        // "same day" heading; a reordering that demotes one is the failure this
        // pins, and asserting the words are somewhere in the block cannot see
        // it.
        var flat = CuratedPage.Flatten(CuratedPage.ReaderText(Block));
        var split = flat.IndexOf("Call your team the same day", StringComparison.Ordinal);
        Assert.True(split > 0, "the block has no same-day tier");

        var ambulanceTier = flat[..split];
        var lines = CuratedPage.EscalationLines();

        // Read from the block, so adding a bullet to either tier does not
        // silently fall outside the check.
        var ambulanceCount = lines.TakeWhile(
            l => ambulanceTier.Contains(l, StringComparison.Ordinal)).Count();
        Assert.True(ambulanceCount >= 6,
            $"the ambulance tier has shrunk to {ambulanceCount} lines — a symptom has been "
            + "demoted to same-day, which is the under-triage direction (§12.10)");

        foreach (var line in lines.Skip(ambulanceCount))
        {
            Assert.DoesNotContain(line, ambulanceTier, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheBlocksSourcesReachTheReaderOnEveryIncludingPage()
    {
        // §12.10: block `sources` merge into the including page's front matter
        // and RENDER in the reader's source list. A citation that does not
        // reach the reader is not a citation, and nothing asserted this.
        var blockUrls = Regex.Matches(CuratedPage.FrontMatter(Block), @"url:\s*(\S+)")
            .Select(m => m.Groups[1].Value)
            .ToArray();

        Assert.True(blockUrls.Length >= 6,
            $"the escalation block cites {blockUrls.Length} sources — did the front matter move?");

        foreach (var slug in HubSlugs)
        {
            var composed = ContentBlocks.Compose(
                Hub(slug), ContentBlockStore.Load(CuratedPage.BlocksRoot), $"tumors/{slug}");

            foreach (var url in blockUrls)
            {
                Assert.Contains(composed.Sources, s => s.Url == url);
            }
        }
    }

    /// <summary>Punctuation, emphasis and case removed, so a near-copy reads as a copy.</summary>
    private static string Loose(string text) =>
        Regex.Replace(Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9 ]", ""), @"\s+", " ").Trim();

    private static IEnumerable<(string Slug, string Text)> TumorHubs()
    {
        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(CuratedPage.PagesDirectory, "tumors"), "*.md"))
        {
            yield return ($"tumors/{Path.GetFileNameWithoutExtension(file)}", File.ReadAllText(file));
        }
    }
}
