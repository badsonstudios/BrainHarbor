using BrainHarbor.Pipeline.Claude;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-302: prompt templates are versioned artifacts and every placeholder
/// must be filled — a prompt that ships with a literal {{abstract}} in it
/// would produce a nonsense summary, so that's a build/test-time failure.
/// </summary>
public class PromptTemplateTests
{
    [Fact]
    public void ParsesTheVersionAndBody()
    {
        var t = PromptTemplate.Parse("version: classify-v1\nClassify {{title}}.");

        Assert.Equal("classify-v1", t.Version);
        Assert.Equal("Classify {{title}}.", t.Body);
    }

    [Fact]
    public void RendersEveryPlaceholder()
    {
        var t = PromptTemplate.Parse("version: v1\nTitle: {{title}}\nAbstract: {{abstract}}");

        var rendered = t.Render(new Dictionary<string, string>
        {
            ["title"] = "A glioma study",
            ["abstract"] = "We studied 331 people.",
        });

        Assert.Contains("Title: A glioma study", rendered);
        Assert.Contains("Abstract: We studied 331 people.", rendered);
        Assert.DoesNotContain("{{", rendered);
    }

    [Fact]
    public void AnUnfilledPlaceholderThrows()
    {
        var t = PromptTemplate.Parse("version: v1\nTitle: {{title}}\nAbstract: {{abstract}}");

        var ex = Assert.Throws<InvalidOperationException>(
            () => t.Render(new Dictionary<string, string> { ["title"] = "only the title" }));

        Assert.Contains("abstract", ex.Message);
    }

    [Fact]
    public void ATemplateWithoutAVersionLineIsRejected()
    {
        Assert.Throws<FormatException>(() => PromptTemplate.Parse("Classify {{title}}."));
        Assert.Throws<FormatException>(() => PromptTemplate.Parse("version:\nbody"));
    }

    [Fact]
    public void CrlfTemplatesParseTheSameAsLf()
    {
        var t = PromptTemplate.Parse("version: v1\r\nTitle: {{title}}");

        Assert.Equal("v1", t.Version);
        Assert.Equal("Title: Smith", t.Render(new Dictionary<string, string> { ["title"] = "Smith" }));
    }
}
