using BrainHarbor.ContentCheck;
using BrainHarbor.Safety;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-574 (§12.31): a shared block's citations are the BLOCK'S, and a flat
/// source list said they were the page's.
///
/// <para>§12.10 merges a block's <c>sources</c> into every including page's front
/// matter, and <c>ContentPage.cshtml</c> rendered the merged list undifferentiated
/// — so <c>/tumors/dipg</c> listed <i>"NIDCD: Vestibular Schwannoma (Acoustic
/// Neuroma) and Neurofibromatosis"</i> among the sources for a reader's child's
/// tumor. Three files recorded that and none fixed it.</para>
///
/// <para><b>What is NOT the defect, because the measurement says so.</b> The
/// citation is honest and the text it supports really is on the page:
/// <c>blocks/mechanism.md</c> renders its whole location list everywhere it is
/// included, skull-base entry and all, and NIDCD is that entry's source. So
/// nothing here drops a citation. What was wrong was the ATTRIBUTION, and the
/// remedy is in the renderer.</para>
///
/// <para><b>The trap these tests exist to hold shut.</b> The obvious fix — stop
/// folding block sources into <c>FrontMatter.Sources</c> — would switch off every
/// §12.10 citation check that reads the page's front matter, for eight blocks at
/// once. §12.10's own last word on this surface is that a dead URL in a block
/// ships onto 24 pages while a front-matter-only test stays green. So the union is
/// UNCHANGED and the split rides alongside it, which is what
/// <see cref="TheTwoListsPartitionTheFrontMatterExactly"/> pins.</para>
/// </summary>
public sealed class BlockSourceAttributionTests
{
    /// <summary>
    /// The 39 pages that render at least one BLOCK-attributed source, PINNED AS A
    /// LITERAL rather than derived from the live corpus.
    ///
    /// <para>WI-580/581/582's repeated finding: a population derived from the live
    /// complement of the thing under assertion goes VACUOUS instead of red when an
    /// entry stops qualifying. So this list is typed out, and it is asserted in BOTH
    /// directions — every slug here must still fold (catches a removal), and nothing
    /// outside it may fold (catches an addition).</para>
    ///
    /// <para><b>39 and not 40.</b> Forty pages include a block; <c>tests/mri</c> is
    /// the fortieth and folds NOTHING, because the one source its block carries is a
    /// source that page already declares itself. "Includes a block" and "renders a
    /// block's citation" are different sets, and this is the second one.</para>
    /// </summary>
    private static readonly string[] PagesWithBlockSources =
    [
        "tests/biopsy",
        "tests/getting-ready-for-surgery",
        "tests/waiting-for-results",
        "treatments/anti-seizure-medicines",
        "treatments/awake-craniotomy",
        "treatments/chemotherapy",
        "treatments/clinical-trials",
        "treatments/craniotomy",
        "treatments/proton-therapy",
        "treatments/radiation-therapy",
        "treatments/shunts",
        "treatments/stereotactic-radiosurgery",
        "treatments/steroids",
        "treatments/targeted-therapy",
        "treatments/tumor-treating-fields",
        "tumors/acoustic-neuroma",
        "tumors/all-brain-tumors",
        "tumors/astrocytoma",
        "tumors/atrt",
        "tumors/brain-metastases",
        "tumors/chordoma",
        "tumors/cns-germ-cell-tumor",
        "tumors/cns-lymphoma",
        "tumors/craniopharyngioma",
        "tumors/diffuse-midline-glioma",
        "tumors/dipg",
        "tumors/ependymoma",
        "tumors/glioblastoma",
        "tumors/glioma",
        "tumors/hemangioblastoma",
        "tumors/high-grade-glioma",
        "tumors/low-grade-glioma",
        "tumors/medulloblastoma",
        "tumors/meningioma",
        "tumors/oligodendroglioma",
        "tumors/pediatric-brain-tumor",
        "tumors/pituitary-tumor",
        "tumors/spinal-cord-tumor",
        "where-your-tumor-is",
    ];

    private const string NidcdTitle =
        "NIDCD: Vestibular Schwannoma (Acoustic Neuroma) and Neurofibromatosis";

    private const string NidcdUrl =
        "https://www.nidcd.nih.gov/health/vestibular-schwannoma-acoustic-neuroma-and-neurofibromatosis";

    /// <summary>
    /// The 17 pages on which the NIDCD citation is a BLOCK source — the defect the
    /// backlog named, pinned page by page. The eighteenth page that includes
    /// <c>[MECHANISM]</c> is <c>tumors/acoustic-neuroma</c>, which is handled by
    /// <see cref="ThePageThatDeclaresASharedUrlItselfKeepsItInItsOwnList"/>.
    /// </summary>
    private static readonly string[] NidcdFoldedOnto =
    [
        "tumors/all-brain-tumors",
        "tumors/astrocytoma",
        "tumors/brain-metastases",
        "tumors/cns-germ-cell-tumor",
        "tumors/cns-lymphoma",
        "tumors/craniopharyngioma",
        "tumors/diffuse-midline-glioma",
        "tumors/dipg",
        "tumors/ependymoma",
        "tumors/glioblastoma",
        "tumors/glioma",
        "tumors/hemangioblastoma",
        "tumors/high-grade-glioma",
        "tumors/medulloblastoma",
        "tumors/meningioma",
        "tumors/oligodendroglioma",
        "tumors/pediatric-brain-tumor",
    ];

    /// <summary>
    /// The three pages that cite nothing, and correctly: <c>/digest</c>,
    /// <c>/privacy</c> and <c>/terms</c> make no medical claim, so they have nothing
    /// to trace. ContentCheck WARNS rather than fails on an empty source list, which
    /// is why they ship. Pinned as a literal so the "every page cites its own work"
    /// assertion below can be exempted here WITHOUT becoming exemptable anywhere else.
    /// </summary>
    private static readonly string[] PagesThatCiteNothing = ["digest", "privacy", "terms"];

    private static List<(string Slug, ContentPage Rendered)> Corpus() =>
        [.. CuratedPage.ReaderPages().Select(p => (p.Slug, CuratedPage.Rendered(p.Slug, p.Text)))];

    private static (string Url, string Title) Key(ContentSource source) => (source.Url, source.Title);

    /// <summary>
    /// THE LOSSLESSNESS GUARANTEE, and the reason the §12.10 gates are untouched.
    /// <c>OwnSources</c> ++ <c>BlockSources</c> is <c>FrontMatter.Sources</c>
    /// ELEMENTWISE AND IN ORDER, on every page — so the split cannot drop, duplicate
    /// or reorder a citation, and every consumer that reads the front matter still
    /// sees exactly what it saw before this item.
    /// </summary>
    [Fact]
    public void TheTwoListsPartitionTheFrontMatterExactly()
    {
        var corpus = Corpus();
        Assert.Equal(55, corpus.Count);

        foreach (var (slug, rendered) in corpus)
        {
            // REFERENCE IDENTITY, not field equality. `ContentSource` is a class
            // with no Equals override, so xUnit's default comparer compares
            // references — which pins that these are the SAME objects in the same
            // order, across all three fields including `Accessed`. A Key-based
            // projection would miss a swap for an equal-looking copy (/review).
            Assert.Equal(
                rendered.FrontMatter.Sources,
                rendered.OwnSources.Concat(rendered.BlockSources));

            // Elementwise equality above would also hold if both sides were empty,
            // so the page-level count is pinned beside it rather than inferred.
            Assert.Equal(
                rendered.FrontMatter.Sources.Count,
                rendered.OwnSources.Count + rendered.BlockSources.Count);
            if (PagesThatCiteNothing.Contains(slug))
            {
                Assert.Empty(rendered.FrontMatter.Sources);
                continue;
            }

            Assert.NotEmpty(rendered.FrontMatter.Sources);
            Assert.True(
                rendered.OwnSources.Count > 0,
                $"{slug} renders no sources of its own — every curated page cites its own work (§2).");
        }

        // The exemption is pinned in BOTH directions, so a tumor page that loses
        // every citation cannot join the exempt set and stay green.
        Assert.Equal<IEnumerable<string>>(
            [.. PagesThatCiteNothing.OrderBy(s => s, StringComparer.Ordinal)],
            [.. corpus.Where(p => p.Rendered.FrontMatter.Sources.Count == 0)
                .Select(p => p.Slug).OrderBy(s => s, StringComparer.Ordinal)]);
    }

    /// <summary>
    /// The corpus-wide totals, as a CHECKSUM. Every per-page assertion above still
    /// passes if the split quietly moves entries from one list to the other on one
    /// page; only a total reds. 711 block + 690 own = 1,401, which is what the
    /// pre-item flat list rendered.
    ///
    /// <para><b>WI-577 MOVED ONE OF THESE NUMBERS, AND THE SHAPE OF THE MOVE IS THE
    /// PROOF THAT NOTHING BROKE.</b> It declared one new source on
    /// <c>/tests/biopsy</c> (PMC8972311, for the second exception to the tissue
    /// rule), so own went 690 → <b>691</b> and the union 1,401 → <b>1,402</b> —
    /// and <b>the block total is UNCHANGED at 711</b>. That asymmetry is the whole
    /// check: a citation added on the DECLARED side must land in
    /// <c>OwnSources</c> and nowhere else, and this test is what would have caught
    /// it landing in the wrong list or in both. The direction matters too — the union
    /// GREW. WI-574's own ruling is that narrowing it is the one change the gate
    /// exists to prevent, because every §12.10 citation gate reads the union.</para>
    ///
    /// <para><b>Updating a checksum is not the same as re-deriving one.</b> The new
    /// figures are the old ones plus the one citation the diff adds, arithmetic
    /// first and measured second; if a future item finds itself editing these to
    /// make a test green without a citation in the diff to account for the
    /// difference, the number is right and the code is wrong.</para>
    /// </summary>
    [Fact]
    public void TheCorpusWideTotalsAreWhatWi574Measured()
    {
        var corpus = Corpus();

        Assert.Equal(711, corpus.Sum(p => p.Rendered.BlockSources.Count));
        Assert.Equal(691, corpus.Sum(p => p.Rendered.OwnSources.Count));
        Assert.Equal(1402, corpus.Sum(p => p.Rendered.FrontMatter.Sources.Count));

        // THE WORST PAGES, AND THERE ARE TWO OF THEM — pinned by name rather than
        // by MaxBy, which picks one of a tie arbitrarily and would have made this
        // assertion depend on enumeration order. 32 of pediatric-brain-tumor's 76
        // citations came from a block and read as its own.
        Assert.Equal<IEnumerable<string>>(
            ["tumors/medulloblastoma", "tumors/pediatric-brain-tumor"],
            [.. corpus.Where(p => p.Rendered.BlockSources.Count == 32)
                .Select(p => p.Slug).OrderBy(s => s, StringComparer.Ordinal)]);
        Assert.Empty(corpus.Where(p => p.Rendered.BlockSources.Count > 32));

        var pediatric = Assert.Single(corpus.Where(p => p.Slug == "tumors/pediatric-brain-tumor"));
        Assert.Equal(44, pediatric.Rendered.OwnSources.Count);
        Assert.Equal(76, pediatric.Rendered.FrontMatter.Sources.Count);
    }

    /// <summary>
    /// The population, asserted in BOTH directions off the pinned literal.
    /// </summary>
    [Fact]
    public void ExactlyThePinnedPagesRenderABlockAttributedSource()
    {
        var corpus = Corpus();
        Assert.Equal(39, PagesWithBlockSources.Length);

        // Direction 1: every pinned page still folds. Reds on a removal.
        foreach (var slug in PagesWithBlockSources)
        {
            var page = Assert.Single(corpus.Where(p => p.Slug == slug));
            Assert.True(
                page.Rendered.BlockSources.Count > 0,
                $"{slug} is pinned as folding a block source and now folds none.");
        }

        // Direction 2: nothing outside the pinned list folds. Reds on an addition.
        var live = corpus.Where(p => p.Rendered.BlockSources.Count > 0).Select(p => p.Slug).ToList();
        Assert.Equal<IEnumerable<string>>([.. PagesWithBlockSources.OrderBy(s => s, StringComparer.Ordinal)],
            [.. live.OrderBy(s => s, StringComparer.Ordinal)]);

        // The fortieth page: includes a block, folds nothing, because it declares
        // that block's only source itself. Pinned so "includes" and "folds" cannot
        // silently collapse into one set again.
        var mri = Assert.Single(corpus.Where(p => p.Slug == "tests/mri"));
        Assert.Empty(mri.Rendered.BlockSources);
        Assert.Equal(7, mri.Rendered.OwnSources.Count);
        Assert.NotEmpty(ContentBlocks.DirectBlockNames(CuratedPage.Body(
            CuratedPage.ReaderPages().Single(p => p.Slug == "tests/mri").Text)));
    }

    /// <summary>
    /// The named defect, page by page: the NIDCD citation is attributed to the block
    /// on all 17 pages, and on NONE of them is it one of the page's own.
    /// </summary>
    [Fact]
    public void TheNidcdCitationIsAttributedToTheBlockOnEveryPageThatInheritsIt()
    {
        var corpus = Corpus();
        Assert.Equal(17, NidcdFoldedOnto.Length);

        foreach (var slug in NidcdFoldedOnto)
        {
            var page = Assert.Single(corpus.Where(p => p.Slug == slug));

            Assert.Contains(NidcdTitle, page.Rendered.BlockSources.Select(s => s.Title));
            Assert.DoesNotContain(NidcdUrl, page.Rendered.OwnSources.Select(s => s.Url));

            // And it is still CITED — the union keeps it, because dropping a real
            // citation to tidy a title trades honesty for presentation.
            Assert.Contains(NidcdUrl, page.Rendered.FrontMatter.Sources.Select(s => s.Url));
        }

        var live = corpus
            .Where(p => p.Rendered.BlockSources.Any(s => s.Url == NidcdUrl))
            .Select(p => p.Slug)
            .OrderBy(s => s, StringComparer.Ordinal);
        Assert.Equal<IEnumerable<string>>(
            [.. NidcdFoldedOnto.OrderBy(s => s, StringComparer.Ordinal)], [.. live]);
    }

    /// <summary>
    /// THE DISCRIMINATING CASE, and it occurs naturally in the corpus rather than in
    /// a fixture. <c>/tumors/acoustic-neuroma</c> includes <c>[MECHANISM]</c> and so
    /// inherits the NIDCD citation — but it also DECLARES that same URL itself, under
    /// its own title (no <c>NIDCD:</c> prefix), because on that page the citation is
    /// squarely about the subject. <c>SameSource</c> matches on URL, so the page's own
    /// declaration wins: the citation stays in its OWN list, under its OWN wording,
    /// and is not repeated below.
    ///
    /// <para>This is what proves the filter reads the DECLARED side. A filter written
    /// the other way round — block-sources-minus-nothing — passes every other test in
    /// this file and reds only here.</para>
    /// </summary>
    [Fact]
    public void ThePageThatDeclaresASharedUrlItselfKeepsItInItsOwnList()
    {
        var page = CuratedPage.Rendered(
            "tumors/acoustic-neuroma",
            CuratedPage.ReaderPages().Single(p => p.Slug == "tumors/acoustic-neuroma").Text);

        var own = Assert.Single(page.OwnSources.Where(s => s.Url == NidcdUrl));
        Assert.Equal("Vestibular Schwannoma (Acoustic Neuroma) and Neurofibromatosis", own.Title);
        Assert.DoesNotContain(NidcdUrl, page.BlockSources.Select(s => s.Url));

        // Cited exactly once on the page, not twice.
        Assert.Single(page.FrontMatter.Sources.Where(s => s.Url == NidcdUrl));

        // And the block it inherits really does carry that URL — without this the
        // assertions above would pass on a page that simply never meets the block.
        Assert.Contains("mechanism", ContentBlocks.DirectBlockNames(CuratedPage.Body(
            CuratedPage.ReaderPages().Single(p => p.Slug == "tumors/acoustic-neuroma").Text)));
        Assert.Contains(
            NidcdUrl,
            ContentBlocks.ParseBlock(
                File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "mechanism.md")), "mechanism")
                .Sources.Select(s => s.Url));
    }

    /// <summary>
    /// THE GATE THAT WAS OFF, and this item turns it on.
    ///
    /// <para><c>ContentPage.cshtml</c> carried three words of prose before this item
    /// ("Last reviewed:", "Sources:") — under <c>CheckRazorPage</c>'s 25-word floor,
    /// so it was reported Info and HELD TO NO READING LEVEL AT ALL. That is §12.22's
    /// and §12.30's defect exactly: a reader-facing line nothing grades. The new
    /// label carries the file over the floor on purpose, so the 6.0 limit now applies
    /// to it.</para>
    ///
    /// <para>Which means shortening the label back under the floor would switch the
    /// gate off again, and that is what this test refuses. It asserts the finding is a
    /// GRADE rather than the "too little to grade" Info — pinning the gate's state,
    /// not just the number.</para>
    /// </summary>
    [Fact]
    public void TheProvenanceLabelsAreGradedAndReadBelowSixthGrade()
    {
        var path = Path.Combine(
            CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Pages", "ContentPage.cshtml");
        var raw = File.ReadAllText(path);

        var text = RazorTextExtractor.ExtractSentences(raw);

        // THE GATE IS ON, asserted on the thing that switches it off. CheckRazorPage
        // reports Info and grades NOTHING below MinimumWordsToGrade (25); this file
        // carried 3 words before WI-574. The floor is spelled out rather than imported
        // because the constant is private.
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        Assert.True(
            words >= 25,
            $"ContentPage.cshtml carries {words} words of reader-facing prose and 25 are "
            + "needed before CheckRazorPage grades it at all — under the floor these "
            + "labels are held to NO reading level (§12.22, §12.30).");

        var finding = Assert.Single(ContentChecker.CheckRazorPage(raw, "ContentPage.cshtml"));
        Assert.StartsWith("reading grade", finding.Message);
        Assert.DoesNotContain("too little to grade", finding.Message);

        // THE LEVEL IS THE ASSERTION, not the number. `Finding.Grade` is documented as
        // never populated by the page-level GradeFinding (ContentChecker.cs:83 — it
        // already killed a ratchet at WI-575), and re-parsing the number out of the
        // message is the comma-decimal locale defect WI-575 removed. Info is the band
        // BELOW WarnGrade 5.5, so this pins the labels under 5.5 and hence under the
        // 6.0 limit, with no number to read back.
        Assert.Equal(FindingLevel.Info, finding.Level);

        // The prose a reader actually meets, pinned. A paraphrase that keeps the word
        // count but drops the attribution would pass every assertion above.
        Assert.Contains("Sources for this page:", text);
        Assert.Contains("Some parts of this page are shared with other pages on this site.", text);
        Assert.Contains("These are the sources for those shared parts.", text);
        Assert.Contains("They are here so you can check any part of this page.", text);

        // AND THE LABELS ARE GRADED ON THEIR OWN, because the gate above is WIDER
        // than this test's name. `CheckRazorPage` grades every word of prose in the
        // file; today that IS the labels, but ~40 words of easy prose added anywhere
        // else would let the label be rewritten to grade 9 while the file average
        // stayed Info and this test stayed green — the §12.31 scope problem running
        // in the opposite direction (/review). Measured at 1.9 today.
        var labels = string.Join(" ", Labels);
        Assert.True(
            ReadingGrade.Of(labels, ReadingGradeOptions.CuratedPages) <= ContentChecker.FailGrade,
            $"the provenance labels, graded alone, read above the {ContentChecker.FailGrade:0.0} limit");

        // Every pinned label is really in the file, so the grade above is not the
        // grade of a string that only this test holds.
        foreach (var label in Labels)
        {
            Assert.Contains(label, text);
        }
    }

    /// <summary>
    /// The three sentences of the shared-source label plus the page's own label, as
    /// the reader meets them. Pinned here so
    /// <see cref="TheProvenanceLabelsAreGradedAndReadBelowSixthGrade"/> can grade the
    /// LABELS rather than the file that happens to contain only them.
    /// </summary>
    private static readonly string[] Labels =
    [
        "Sources for this page:",
        "Some parts of this page are shared with other pages on this site.",
        "These are the sources for those shared parts.",
        "They are here so you can check any part of this page.",
    ];
}

/// <summary>
/// WI-574: the two lists through the REAL renderer, over HTTP. The split is only
/// worth anything if a reader sees it, and §12.30's carry-forward is that a needle
/// can miss on markup rather than content — so the haystack is flattened and every
/// needle is COUNTED rather than merely found.
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class BlockSourceAttributionRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BlockSourceAttributionRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task DipgRendersItsOwnSourcesAndTheSharedOnesAsTwoLabelledLists()
    {
        var html = CuratedPage.Flatten(await _factory.CreateClient().GetStringAsync("/tumors/dipg"));

        // Both labels, in reader order: the page's own list comes first.
        var ownLabel = html.IndexOf("Sources for this page:", StringComparison.Ordinal);
        var sharedLabel = html.IndexOf(
            "Some parts of this page are shared with other pages on this site.", StringComparison.Ordinal);
        Assert.True(ownLabel > 0, "the page's own source label is missing from the render");
        Assert.True(sharedLabel > ownLabel, "the shared-source label must follow the page's own list");

        // The flat list is GONE, asserted MARKUP-FREE. The previous form was
        // `DoesNotContain("<p>Sources:</p>")`, which only reds on a byte-identical
        // revert — the same label re-wrapped across lines would have survived it
        // (/review). No
        // .cshtml and no page or block body contains the bare string "Sources:",
        // and "Sources for this page:" does not contain it either.
        Assert.DoesNotContain("Sources:", html);

        // THE DEFECT, where the reader meets it: the NIDCD title is on the page, and
        // it is BELOW the shared label rather than among the page's own citations.
        var nidcd = html.IndexOf(
            "Vestibular Schwannoma (Acoustic Neuroma) and Neurofibromatosis", StringComparison.Ordinal);
        Assert.True(nidcd > sharedLabel, "the NIDCD citation still renders as one of this page's own");

        // And the counts, so a truncated list cannot pass: 17 own + 29 shared.
        var provenance = html[html.IndexOf("class=\"provenance\"", StringComparison.Ordinal)..];
        var between = provenance[..provenance.IndexOf("</div>", StringComparison.Ordinal)];
        var own = between[..between.IndexOf(
            "Some parts of this page are shared", StringComparison.Ordinal)];
        var shared = between[between.IndexOf(
            "Some parts of this page are shared", StringComparison.Ordinal)..];

        Assert.Equal(17, own.Split("<li>").Length - 1);
        Assert.Equal(29, shared.Split("<li>").Length - 1);
    }

    /// <summary>
    /// A page with no blocks renders ONE list and no shared label — the second list
    /// must not appear empty-headed on 15 pages.
    /// </summary>
    [Fact]
    public async Task APageWithNoSharedBlocksRendersNoSharedSourceLabel()
    {
        var html = CuratedPage.Flatten(await _factory.CreateClient().GetStringAsync("/tests/mri"));

        Assert.Contains("Sources for this page:", html);
        Assert.DoesNotContain("Some parts of this page are shared with other pages on this site.", html);

        var provenance = html[html.IndexOf("class=\"provenance\"", StringComparison.Ordinal)..];
        var between = provenance[..provenance.IndexOf("</div>", StringComparison.Ordinal)];
        Assert.Equal(7, between.Split("<li>").Length - 1);
    }
}
