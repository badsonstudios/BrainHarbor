using System.Collections.Concurrent;
using YamlDotNet.Serialization;

namespace BrainHarbor.Web.Content;

/// <summary>
/// One glossary entry (content-pipeline.md §6): one Markdown file per term —
/// front matter has the term, optional aliases and pronunciation; the body is
/// the plain-language definition (≤ 40 words by editorial rule).
/// </summary>
public sealed record GlossaryTerm(
    string Slug,
    string Term,
    IReadOnlyList<string> Aliases,
    string? Pronunciation,
    string Definition);

public sealed class GlossaryFrontMatter
{
    [YamlMember(Alias = "term")]
    public string Term { get; set; } = "";

    [YamlMember(Alias = "also")]
    public List<string> Also { get; set; } = [];

    [YamlMember(Alias = "pronunciation")]
    public string? Pronunciation { get; set; }
}

/// <summary>
/// WI-105: loads glossary term files from {root}/*.md, keyed by slug
/// (the file name). Reloads when the directory contents change — same
/// dev-friendly, prod-cheap policy as ContentStore.
/// </summary>
public sealed class GlossaryStore(IWebHostEnvironment environment, IConfiguration configuration)
{
    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>Immutable terms + version pair — read atomically to avoid
    /// caching pages rendered with old terms under a new version.</summary>
    public sealed record GlossarySnapshot(string Version, IReadOnlyList<GlossaryTerm> Terms);

    private readonly object _reload = new();
    private volatile GlossarySnapshot _snapshot = new("", []);

    private string Root =>
        configuration["Glossary:Root"]
        ?? Path.Combine(environment.ContentRootPath, "Content", "glossary");

    /// <summary>The current terms + version, atomically.</summary>
    public GlossarySnapshot GetSnapshot()
    {
        var stamp = DirectoryStamp();
        if (stamp != _snapshot.Version)
        {
            lock (_reload)
            {
                if (stamp != _snapshot.Version)
                {
                    _snapshot = new GlossarySnapshot(stamp, Load());
                }
            }
        }
        return _snapshot;
    }

    /// <summary>All terms, alphabetical by term text.</summary>
    public IReadOnlyList<GlossaryTerm> GetTerms() => GetSnapshot().Terms;

    private string DirectoryStamp()
    {
        if (!Directory.Exists(Root))
        {
            return "missing";
        }

        return string.Join(";",
            Directory.EnumerateFiles(Root, "*.md")
                .OrderBy(f => f, StringComparer.Ordinal)
                .Select(f => $"{f}|{File.GetLastWriteTimeUtc(f).Ticks}"));
    }

    private List<GlossaryTerm> Load()
    {
        var terms = new List<GlossaryTerm>();
        if (!Directory.Exists(Root))
        {
            return terms;
        }

        foreach (var file in Directory.EnumerateFiles(Root, "*.md"))
        {
            try
            {
                terms.Add(ParseTerm(File.ReadAllText(file), Path.GetFileNameWithoutExtension(file)));
            }
            catch (IOException)
            {
                // Skip files mid-write; next stamp change reloads.
            }
        }

        return [.. terms.OrderBy(t => t.Term, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>Parses one term file (definition body is plain text — no
    /// Markdown). Throws FormatException on bad input — same policy as
    /// ContentStore. Slug = file name; must be url/id-safe.</summary>
    public static GlossaryTerm ParseTerm(string raw, string slug)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9][a-z0-9-]*$"))
        {
            throw new FormatException(
                $"Glossary file name '{slug}' must be a lowercase slug ([a-z0-9-]) — it becomes the anchor id and popover target.");
        }

        raw = raw.TrimStart('﻿');
        if (!raw.StartsWith("---"))
        {
            throw new FormatException($"Glossary term '{slug}' is missing front matter.");
        }

        var end = raw.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (end < 0)
        {
            throw new FormatException($"Glossary term '{slug}' has an unterminated front matter block.");
        }

        GlossaryFrontMatter frontMatter;
        try
        {
            frontMatter = Yaml.Deserialize<GlossaryFrontMatter>(raw[3..end].Trim('\r', '\n'))
                ?? throw new FormatException($"Glossary term '{slug}' has empty front matter.");
        }
        catch (YamlDotNet.Core.YamlException exception)
        {
            throw new FormatException($"Glossary term '{slug}' has invalid front matter: {exception.Message}", exception);
        }

        if (string.IsNullOrWhiteSpace(frontMatter.Term))
        {
            throw new FormatException($"Glossary term '{slug}' is missing the term field.");
        }

        var definition = raw[(end + 4)..].Trim();
        if (string.IsNullOrWhiteSpace(definition))
        {
            throw new FormatException($"Glossary term '{slug}' has no definition body.");
        }

        return new GlossaryTerm(slug, frontMatter.Term, frontMatter.Also, frontMatter.Pronunciation, definition);
    }
}
