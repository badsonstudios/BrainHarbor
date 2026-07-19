using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-104 unit tests: front-matter parsing per content-pipeline.md §3.
/// Routing/integration coverage lives in ContentPageRoutingTests.
/// </summary>
public class ContentStoreTests
{
    private const string FullPage = """
        ---
        title: "Compassionate Allowances: the fast track"
        slug: fast-track
        section: benefits
        description: "Some brain tumors qualify in weeks."
        tags: [ssdi, glioblastoma]
        sources:
          - url: https://www.ssa.gov/compassionateallowances/
            title: "SSA Compassionate Allowances"
            accessed: 2026-07-12
        reviewed: 2026-07-12
        review_due: 2027-01-15
        volatile_figures: true
        reading_grade: 7.2
        disclaimers: [medical, benefits]
        ---

        Some tumors are on the **fast-track** list.
        """;

    [Fact]
    public void ParsesTheFullFrontMatterSchema()
    {
        var page = ContentStore.Parse(FullPage, "benefits/fast-track");
        var fm = page.FrontMatter;

        Assert.Equal("Compassionate Allowances: the fast track", fm.Title);
        Assert.Equal("fast-track", fm.Slug);
        Assert.Equal("benefits", fm.Section);
        Assert.Equal("Some brain tumors qualify in weeks.", fm.Description);
        Assert.Equal(["ssdi", "glioblastoma"], fm.Tags);
        var source = Assert.Single(fm.Sources);
        Assert.Equal("https://www.ssa.gov/compassionateallowances/", source.Url);
        Assert.Equal("SSA Compassionate Allowances", source.Title);
        Assert.Equal(new DateOnly(2026, 7, 12), source.Accessed);
        Assert.Equal(new DateOnly(2026, 7, 12), fm.Reviewed);
        Assert.Equal(new DateOnly(2027, 1, 15), fm.ReviewDue);
        Assert.True(fm.VolatileFigures);
        Assert.Equal(7.2, fm.ReadingGrade);
        Assert.Equal(["medical", "benefits"], fm.Disclaimers);
    }

    [Fact]
    public void RendersMarkdownBodyToHtml()
    {
        var page = ContentStore.Parse(FullPage, "benefits/fast-track");

        Assert.Contains("<strong>fast-track</strong>", page.Html);
        Assert.DoesNotContain("---", page.Html); // front matter never leaks
    }

    [Fact]
    public void MissingFrontMatterThrows()
    {
        var exception = Assert.Throws<FormatException>(
            () => ContentStore.Parse("Just some markdown.", "about"));

        Assert.Contains("missing YAML front matter", exception.Message);
    }

    [Fact]
    public void UnterminatedFrontMatterThrows()
    {
        Assert.Throws<FormatException>(
            () => ContentStore.Parse("---\ntitle: x\nno closing fence", "about"));
    }

    [Fact]
    public void MissingTitleThrows()
    {
        var exception = Assert.Throws<FormatException>(
            () => ContentStore.Parse("---\nslug: about\n---\nBody.", "about"));

        Assert.Contains("missing a title", exception.Message);
    }

    [Fact]
    public void InvalidYamlThrowsFormatException()
    {
        Assert.Throws<FormatException>(
            () => ContentStore.Parse("---\ntitle: [unclosed\n---\nBody.", "about"));
    }

    [Fact]
    public void UnknownFrontMatterKeysAreIgnored()
    {
        var page = ContentStore.Parse(
            "---\ntitle: Test\nfuture_field: whatever\n---\nBody.", "about");

        Assert.Equal("Test", page.FrontMatter.Title);
    }
}
