using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-201: the taxonomy is a CLOSED tree — the classifier may only emit these
/// slugs (content-pipeline.md §9). These tests guard the gate that stops an
/// invented (or medically wrong) tumor type reaching a patient.
/// </summary>
public class TaxonomyTests
{
    private const string Sample = """
        tumor_types:
          - slug: glioma
            label: "Glioma"
            also: [gliomas]
          - slug: high-grade-glioma
            label: "High-grade glioma"
            parent: glioma
            also: [HGG, "grade 4 glioma"]
          - slug: glioblastoma
            label: "Glioblastoma"
            parent: high-grade-glioma
            also: [GBM, "glioblastoma multiforme"]
          - slug: meningioma
            label: "Meningioma"
          - slug: all-brain-tumors
            label: "All brain tumors"
        """;

    private static TaxonomyStore Store => new(Sample);

    [Fact]
    public void LoadsSlugsAndLabels()
    {
        var store = Store;

        Assert.Equal(5, store.TumorTypes.Count);
        Assert.True(store.IsKnownSlug("glioma"));
        Assert.Equal("Glioblastoma", store.LabelFor("glioblastoma"));
    }

    [Fact]
    public void UnknownSlugIsRejected()
    {
        Assert.False(Store.IsKnownSlug("dragonoma"));
        Assert.Null(Store.Resolve("dragonoma"));
    }

    [Fact]
    public void AliasesResolveToTheCanonicalSlug()
    {
        var store = Store;

        Assert.Equal("glioblastoma", store.Resolve("GBM"));
        Assert.Equal("glioblastoma", store.Resolve("glioblastoma multiforme"));
        Assert.Equal("glioma", store.Resolve("gliomas"));
    }

    [Fact]
    public void ResolveIsLenientButIsKnownSlugIsStrict()
    {
        var store = Store;

        // Resolve normalizes casing and whitespace...
        Assert.Equal("glioblastoma", store.Resolve("gbm"));
        Assert.Equal("glioma", store.Resolve("  glioma  "));

        // ...but validation is exact, so nobody validates 'GLIOMA' and then
        // persists it where the exact-match GIN index would never find it.
        Assert.False(store.IsKnownSlug("GLIOMA"));
        Assert.True(store.IsKnownSlug("glioma"));
    }

    // ---------- hierarchy ----------

    [Fact]
    public void AncestorsWalkToTheRoot()
    {
        Assert.Equal(["glioblastoma", "high-grade-glioma", "glioma"],
            Store.WithAncestors("glioblastoma"));
    }

    [Fact]
    public void FilteringAParentMatchesItsDescendants()
    {
        var store = Store;

        // The whole point: someone browsing "glioma" must see GBM research.
        Assert.True(store.Matches(["glioblastoma"], "glioma"));
        Assert.True(store.Matches(["glioblastoma"], "high-grade-glioma"));
        Assert.True(store.Matches(["glioblastoma"], "glioblastoma"));
    }

    [Fact]
    public void FilteringAChildDoesNotMatchTheParent()
    {
        // A general glioma paper is not glioblastoma research.
        Assert.False(Store.Matches(["glioma"], "glioblastoma"));
    }

    [Fact]
    public void UnrelatedTypesDoNotMatch()
    {
        Assert.False(Store.Matches(["meningioma"], "glioma"));
    }

    [Fact]
    public void CatchAllMatchesEveryFilter()
    {
        var store = Store;

        Assert.True(store.Matches(["all-brain-tumors"], "glioblastoma"));
        Assert.True(store.Matches(["all-brain-tumors"], "meningioma"));
    }

    [Fact]
    public void MatchesResolvesAliasesOnBothSides()
    {
        Assert.True(Store.Matches(["GBM"], "gliomas"));
    }

    [Fact]
    public void UnknownFilterMatchesNothing()
    {
        Assert.False(Store.Matches(["glioblastoma"], "dragonoma"));
    }

    // ---------- the gate ----------

    [Fact]
    public void FilterTagsDropsInventedTagsAndReportsThem()
    {
        var result = Store.FilterTags(["glioma", "dragonoma", "GBM", "gliomas", ""]);

        Assert.Equal(["glioma", "glioblastoma"], result.Known);
        Assert.Equal(["dragonoma"], result.Rejected);
    }

    [Fact]
    public void FilterTagsNormalizesCasingSoStoredTagsAreCanonical()
    {
        var result = Store.FilterTags(["GLIOMA", "Gbm"]);

        Assert.Equal(["glioma", "glioblastoma"], result.Known);
        Assert.Empty(result.Rejected);
    }

    // ---------- config errors ----------

    [Fact]
    public void DuplicateSlugIsAConfigError()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("""
            tumor_types:
              - slug: glioma
                label: "Glioma"
              - slug: glioma
                label: "Glioma again"
            """));
    }

    [Fact]
    public void AliasClaimedByTwoTypesIsAConfigError()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("""
            tumor_types:
              - slug: glioma
                label: "Glioma"
                also: [brain-thing]
              - slug: meningioma
                label: "Meningioma"
                also: [brain-thing]
            """));
    }

    [Fact]
    public void AnEntrysOwnSlugWinsOverAnotherEntrysAlias()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("""
            tumor_types:
              - slug: glioma
                label: "Glioma"
                also: [meningioma]
              - slug: meningioma
                label: "Meningioma"
            """));
    }

    [Fact]
    public void BlankAliasIsAConfigError()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("""
            tumor_types:
              - slug: glioma
                label: "Glioma"
                also: ["  "]
            """));
    }

    [Fact]
    public void UnknownParentIsAConfigError()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("""
            tumor_types:
              - slug: glioblastoma
                label: "Glioblastoma"
                parent: nope
            """));
    }

    [Fact]
    public void ParentCycleIsAConfigError()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("""
            tumor_types:
              - slug: a
                label: "A"
                parent: b
              - slug: b
                label: "B"
                parent: a
            """));
    }

    [Fact]
    public void EntryMissingLabelIsAConfigError()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("""
            tumor_types:
              - slug: glioma
            """));
    }

    [Fact]
    public void EmptyTaxonomyIsAConfigError()
    {
        Assert.Throws<FormatException>(() => new TaxonomyStore("tumor_types: []"));
    }

    // ---------- the shipped file ----------

    [Fact]
    public void ShippedTaxonomyLoadsWithTheExpectedHierarchy()
    {
        var store = ShippedStore();

        Assert.True(store.TumorTypes.Count >= 15);
        foreach (var slug in new[]
                 { "glioma", "glioblastoma", "low-grade-glioma", "high-grade-glioma", "all-brain-tumors" })
        {
            Assert.True(store.IsKnownSlug(slug), $"shipped taxonomy is missing '{slug}'");
        }

        Assert.True(store.Matches(["glioblastoma"], "glioma"));
        Assert.True(store.Matches(["dipg"], "glioma"));
    }

    [Fact]
    public void ShippedTaxonomyDoesNotRepeatTheWhoCns5AliasMistakes()
    {
        var store = ShippedStore();

        // "Grade 4 glioma" under WHO CNS5 also covers IDH-mutant astrocytoma
        // and H3 K27-altered midline glioma — calling it glioblastoma would
        // show those patients research about a different disease.
        Assert.Equal("high-grade-glioma", store.Resolve("grade 4 glioma"));

        // DIPG is the pontine subset of diffuse midline glioma, not a synonym.
        Assert.Equal("diffuse-midline-glioma", store.Resolve("DMG"));
        Assert.Equal("dipg", store.Resolve("diffuse intrinsic pontine glioma"));
        Assert.True(store.Matches(["dipg"], "diffuse-midline-glioma"));
        Assert.False(store.Matches(["diffuse-midline-glioma"], "dipg"));
    }

    private static TaxonomyStore ShippedStore() =>
        new(File.ReadAllText(Path.Combine(
            FindRepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml")));

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
