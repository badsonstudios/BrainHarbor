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

    /// <summary>
    /// WI-561: one entry per image the body shows, keyed by <c>src</c> — the
    /// credit, the licence and the pixel size the browser should reserve.
    ///
    /// <para><b>Why the credit lives here and not next to the picture.</b> The
    /// Markdown line carries what the READER needs (the alt text and the
    /// caption); this carries what the SITE needs, where ContentCheck and the
    /// test suite can see it per page. An uncredited image is a licensing
    /// defect, not a rendering one, and <c>wwwroot/img/cards/IMAGE-CREDITS.md</c>
    /// is one file nothing mechanically ties to a page (WI-502/WI-505: a
    /// citation nobody can follow is not a citation).</para>
    /// </summary>
    /// <para><b>Null-coalesced on read, deliberately</b> (<c>/review</c>).
    /// A key with nothing under it — <c>images:</c> and then the closing
    /// <c>---</c>, which is exactly what a half-typed entry looks like —
    /// makes YamlDotNet assign null OVER this initializer. Left as null it
    /// reached <see cref="ContentFigures"/> as a
    /// <c>NullReferenceException</c>: ContentCheck catches only
    /// <c>FormatException</c> so the gate would crash instead of failing the
    /// page, <c>ContentStore.GetPage</c> catches only <c>IOException</c> so a
    /// reader would get a 500 on a medical page, and
    /// <c>ContentStore.SearchPages</c> would lose site search for every page
    /// over one dangling key.</para>
    [YamlMember(Alias = "images")]
    public List<ContentImage> Images
    {
        get => _images ??= [];
        set => _images = value;
    }

    private List<ContentImage>? _images;
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

/// <summary>
/// WI-561: the licence and layout facts about one image on a curated page.
/// Every field except <c>source_url</c> and <c>decorative</c> is required —
/// <see cref="ContentFigures"/> fails the page by name when one is missing,
/// because an image with no licence is the one content defect on this site
/// that is somebody else's legal problem rather than only a reader's.
/// </summary>
public sealed class ContentImage
{
    /// <summary>Site-root-relative path, matched against the Markdown image's URL.</summary>
    [YamlMember(Alias = "src")]
    public string Src { get; set; } = "";

    /// <summary>Who made it — a photographer, an organisation, or us.</summary>
    [YamlMember(Alias = "credit")]
    public string Credit { get; set; } = "";

    /// <summary>The licence by name, as a reader would see it written.</summary>
    [YamlMember(Alias = "license")]
    public string License { get; set; } = "";

    /// <summary>Where the licence text itself can be read.</summary>
    [YamlMember(Alias = "license_url")]
    public string LicenseUrl { get; set; } = "";

    /// <summary>Where the image came from, when it came from somewhere.</summary>
    [YamlMember(Alias = "source_url")]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Intrinsic pixel size. Required, and not for tidiness: figures below the
    /// first one are lazy-loaded, and a lazy image with no declared size
    /// reflows the text under it when it arrives — on a phone that moves the
    /// line a reader is on, which is the reader this site is built for.
    /// </summary>
    [YamlMember(Alias = "width")]
    public int Width { get; set; }

    [YamlMember(Alias = "height")]
    public int Height { get; set; }

    /// <summary>
    /// An empty <c>alt=""</c> is a claim that a screen-reader user loses
    /// nothing by skipping this picture. It has to be made on purpose, so it
    /// is made here rather than by leaving the Markdown's brackets empty.
    /// </summary>
    [YamlMember(Alias = "decorative")]
    public bool Decorative { get; set; }
}
