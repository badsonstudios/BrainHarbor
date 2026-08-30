namespace BrainHarbor.Web.Content;

/// <summary>
/// WI-501: loads the shared content blocks from {root}/*.md, keyed by file
/// name. Reloads when the directory contents change — same dev-friendly,
/// prod-cheap policy as <see cref="GlossaryStore"/>.
///
/// The version matters as much as the blocks: <see cref="ContentStore"/>
/// caches a rendered page by its own write time, so without a blocks version
/// in that key, editing the crosswalk would change nothing until the file it
/// is included from was also touched — or the app restarted.
/// </summary>
public sealed class ContentBlockStore(IWebHostEnvironment environment, IConfiguration configuration)
{
    /// <summary>Immutable blocks + version pair — read atomically, so a page
    /// is never cached under a new version having been rendered with old blocks.</summary>
    public sealed record BlockSnapshot(string Version, ContentBlockSet Blocks);

    private static readonly BlockSnapshot Empty = new("", ContentBlockSet.Empty);

    private readonly object _reload = new();
    private volatile BlockSnapshot _snapshot = Empty;

    /// <summary>
    /// Blocks live beside the pages they compose into, so a test (or a
    /// future second content root) that redirects Content:Root gets the
    /// matching blocks rather than the shipped ones.
    /// </summary>
    private string Root =>
        configuration["Content:BlocksRoot"]
        ?? DefaultBlocksRootFor(
            configuration["Content:Root"]
            ?? Path.Combine(environment.ContentRootPath, "Content", "pages"));

    /// <summary>The blocks directory that belongs to a given pages root: its sibling "blocks".</summary>
    public static string DefaultBlocksRootFor(string pagesRoot) =>
        Path.Combine(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(pagesRoot)) ?? ".", "blocks");

    /// <summary>The current blocks + version, atomically.</summary>
    public BlockSnapshot GetSnapshot()
    {
        var stamp = DirectoryStamp(Root);
        if (stamp != _snapshot.Version)
        {
            lock (_reload)
            {
                if (stamp != _snapshot.Version)
                {
                    _snapshot = new BlockSnapshot(stamp, Load(Root));
                }
            }
        }
        return _snapshot;
    }

    private static string DirectoryStamp(string root)
    {
        if (!Directory.Exists(root))
        {
            return "missing";
        }

        return string.Join(";",
            Directory.EnumerateFiles(root, "*.md")
                .OrderBy(f => f, StringComparer.Ordinal)
                .Select(f => $"{f}|{File.GetLastWriteTimeUtc(f).Ticks}"));
    }

    /// <summary>
    /// Reads every block under <paramref name="root"/>. A malformed block is
    /// RECORDED, not thrown: throwing here would take down /about and
    /// /privacy — pages with no blocks at all — over a typo in a file they
    /// never reference, and would make SearchPages return nothing site-wide
    /// (it swallows FormatException per file). Instead the error travels with
    /// the set, so a page that includes the broken block fails by name and
    /// every other page is unaffected. ContentCheck turns the same errors
    /// into build failures.
    ///
    /// Files whose name is not a valid block slug are ignored outright — a
    /// README.md dropped in this directory is a normal thing to do and must
    /// not be a site outage.
    /// </summary>
    public static ContentBlockSet Load(string root)
    {
        var blocks = new Dictionary<string, ContentBlock>(StringComparer.Ordinal);
        var errors = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!Directory.Exists(root))
        {
            return ContentBlockSet.Empty;
        }

        foreach (var file in Directory.EnumerateFiles(root, "*.md")
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (!ContentBlocks.IsBlockFileName(name))
            {
                continue;
            }

            try
            {
                blocks[name] = ContentBlocks.ParseBlock(File.ReadAllText(file), name);
            }
            catch (FormatException exception)
            {
                errors[name] = exception.Message;
            }
            catch (IOException exception)
            {
                // Mid-write in dev; the next stamp change reloads it.
                errors[name] = exception.Message;
            }
        }

        return new ContentBlockSet(blocks, errors);
    }
}
