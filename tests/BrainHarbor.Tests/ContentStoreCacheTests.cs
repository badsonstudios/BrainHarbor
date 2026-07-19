using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-104: GetPage cache behavior against a real temp directory — hit,
/// mtime invalidation, eviction on delete, and slug-guard rejection.
/// </summary>
public sealed class ContentStoreCacheTests : IDisposable
{
    private readonly string _root;
    private readonly ContentStore _store;

    public ContentStoreCacheTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "bh-content-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Content:Root"] = _root,
                ["Glossary:Root"] = Path.Combine(_root, "no-glossary"),
            })
            .Build();
        var environment = new StubEnvironment();
        _store = new ContentStore(environment, configuration, new GlossaryStore(environment, configuration));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private void WritePage(string relativePath, string body)
    {
        var file = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, $"---\ntitle: Cache test\n---\n{body}");
    }

    [Fact]
    public void CachesParsedPagesUntilTheFileChanges()
    {
        WritePage("about.md", "First version.");
        var first = _store.GetPage("about");
        Assert.Contains("First version.", first!.Html);

        // Unchanged file → same cached instance.
        Assert.Same(first, _store.GetPage("about"));

        // Changed file (bump mtime past filesystem granularity) → re-parsed.
        WritePage("about.md", "Second version.");
        File.SetLastWriteTimeUtc(Path.Combine(_root, "about.md"), DateTime.UtcNow.AddMinutes(1));
        Assert.Contains("Second version.", _store.GetPage("about")!.Html);
    }

    [Fact]
    public void DeletedFileIsEvictedAndReturnsNull()
    {
        WritePage("gone.md", "Here today.");
        Assert.NotNull(_store.GetPage("gone"));

        File.Delete(Path.Combine(_root, "gone.md"));
        Assert.Null(_store.GetPage("gone"));
    }

    [Fact]
    public void SectionSlugPathsResolveSubdirectories()
    {
        WritePage(Path.Combine("benefits", "fast-track.md"), "Sectioned.");

        Assert.Contains("Sectioned.", _store.GetPage("benefits/fast-track")!.Html);
    }

    [Theory]
    [InlineData("../secrets")]
    [InlineData("a/b/c")]
    [InlineData("UPPER")]
    [InlineData("has space")]
    [InlineData("-leading-dash")]
    [InlineData("")]
    public void NonSlugPathsAreRejectedBeforeTouchingDisk(string path)
    {
        Assert.Null(_store.GetPage(path));
    }

    [Fact]
    public void RawHtmlInMarkdownRendersEscaped()
    {
        WritePage("html.md", "Safe <script>alert(1)</script> text.");

        var html = _store.GetPage("html")!.Html;
        Assert.DoesNotContain("<script>", html);
    }

    private sealed class StubEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
