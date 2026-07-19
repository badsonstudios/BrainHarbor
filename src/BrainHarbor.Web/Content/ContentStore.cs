using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Markdig;
using YamlDotNet.Serialization;

namespace BrainHarbor.Web.Content;

/// <summary>One parsed curated page: front matter + rendered HTML.</summary>
public sealed record ContentPage(
    ContentFrontMatter FrontMatter,
    string Html,
    string Markdown,
    string UrlPath);

/// <summary>
/// WI-104: loads curated Markdown pages (content-pipeline.md §3 schema) from
/// disk, renders them with Markdig, and caches by file write time — so
/// editing a page in dev shows up on refresh without a restart, and steady
/// state serves from memory. URL paths map to files:
/// /about → {root}/about.md, /benefits/fast-track → {root}/benefits/fast-track.md.
/// </summary>
public sealed partial class ContentStore(
    IWebHostEnvironment environment, IConfiguration configuration, GlossaryStore glossary)
{
    // section/slug segments only — blocks traversal and anything non-slug.
    [GeneratedRegex("^[a-z0-9][a-z0-9-]*(/[a-z0-9][a-z0-9-]*)?$")]
    private static partial Regex UrlPathPattern();

    // DisableHtml: curated pages are pure Markdown; raw HTML in a source file
    // renders escaped. The glossary extension still emits markup — it renders
    // through its own object renderer, not raw HTML inlines.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Use<GlossaryTooltipExtension>()
        .Build();

    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly ConcurrentDictionary<string, (DateTime WriteTimeUtc, string GlossaryVersion, ContentPage Page)> _cache = new();

    private string Root =>
        configuration["Content:Root"]
        ?? Path.Combine(environment.ContentRootPath, "Content", "pages");

    /// <summary>Returns the page for a URL path like "about" or "benefits/fast-track", or null.</summary>
    public ContentPage? GetPage(string urlPath)
    {
        urlPath = urlPath.Trim('/').ToLowerInvariant();
        if (!UrlPathPattern().IsMatch(urlPath))
        {
            return null;
        }

        var file = Path.Combine(Root, urlPath.Replace('/', Path.DirectorySeparatorChar) + ".md");
        try
        {
            if (!File.Exists(file))
            {
                _cache.TryRemove(urlPath, out _);
                return null;
            }

            var snapshot = glossary.GetSnapshot();
            var writeTime = File.GetLastWriteTimeUtc(file);
            if (_cache.TryGetValue(urlPath, out var cached) &&
                cached.WriteTimeUtc == writeTime &&
                cached.GlossaryVersion == snapshot.Version)
            {
                return cached.Page;
            }

            var page = Parse(File.ReadAllText(file), urlPath, snapshot.Terms);
            _cache[urlPath] = (writeTime, snapshot.Version, page);
            return page;
        }
        catch (IOException)
        {
            // File vanished/locked between checks — treat as missing (404),
            // never a patient-facing 500.
            _cache.TryRemove(urlPath, out _);
            return null;
        }
    }

    /// <summary>
    /// Parses a curated page: required YAML front matter fenced by ---, then
    /// Markdown. Throws on malformed input — a bad page is a build/content
    /// error, never something to render half-broken to a patient.
    /// </summary>
    public static ContentPage Parse(string raw, string urlPath) => Parse(raw, urlPath, []);

    /// <summary>Parse with glossary terms: first occurrences get tooltips (WI-105).</summary>
    public static ContentPage Parse(string raw, string urlPath, IReadOnlyList<GlossaryTerm> glossaryTerms)
    {
        raw = raw.TrimStart('﻿'); // BOM
        if (!raw.StartsWith("---"))
        {
            throw new FormatException($"Content page '{urlPath}' is missing YAML front matter.");
        }

        var end = raw.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (end < 0)
        {
            throw new FormatException($"Content page '{urlPath}' has an unterminated front matter block.");
        }

        var yamlBlock = raw[3..end].Trim('\r', '\n');
        var body = raw[(end + 4)..].TrimStart('\r', '\n');

        ContentFrontMatter frontMatter;
        try
        {
            frontMatter = Yaml.Deserialize<ContentFrontMatter>(yamlBlock)
                ?? throw new FormatException($"Content page '{urlPath}' has empty front matter.");
        }
        catch (YamlDotNet.Core.YamlException exception)
        {
            throw new FormatException($"Content page '{urlPath}' has invalid front matter: {exception.Message}", exception);
        }

        if (string.IsNullOrWhiteSpace(frontMatter.Title))
        {
            throw new FormatException($"Content page '{urlPath}' is missing a title.");
        }

        var document = Markdig.Markdown.Parse(body, Pipeline);
        GlossaryMarker.Mark(document, glossaryTerms);

        using var writer = new StringWriter();
        var renderer = new Markdig.Renderers.HtmlRenderer(writer);
        Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();

        return new ContentPage(frontMatter, writer.ToString(), body, urlPath);
    }
}
