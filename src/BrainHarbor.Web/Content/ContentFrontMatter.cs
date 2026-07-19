using YamlDotNet.Serialization;

namespace BrainHarbor.Web.Content;

/// <summary>
/// Front matter for curated Markdown pages — the schema in
/// content-pipeline.md §3. Field names map from snake_case YAML.
/// </summary>
public sealed class ContentFrontMatter
{
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = "";

    [YamlMember(Alias = "slug")]
    public string Slug { get; set; } = "";

    [YamlMember(Alias = "section")]
    public string? Section { get; set; }

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = "";

    [YamlMember(Alias = "tags")]
    public List<string> Tags { get; set; } = [];

    [YamlMember(Alias = "sources")]
    public List<ContentSource> Sources { get; set; } = [];

    [YamlMember(Alias = "reviewed")]
    public DateOnly? Reviewed { get; set; }

    [YamlMember(Alias = "review_due")]
    public DateOnly? ReviewDue { get; set; }

    [YamlMember(Alias = "volatile_figures")]
    public bool VolatileFigures { get; set; }

    [YamlMember(Alias = "reading_grade")]
    public double? ReadingGrade { get; set; }

    [YamlMember(Alias = "disclaimers")]
    public List<string> Disclaimers { get; set; } = [];
}

public sealed class ContentSource
{
    [YamlMember(Alias = "url")]
    public string Url { get; set; } = "";

    [YamlMember(Alias = "title")]
    public string Title { get; set; } = "";

    [YamlMember(Alias = "accessed")]
    public DateOnly? Accessed { get; set; }
}
