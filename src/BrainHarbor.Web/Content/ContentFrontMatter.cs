using YamlDotNet.Serialization;

namespace BrainHarbor.Web.Content;

/// <summary>
/// Front matter for curated Markdown pages — the schema in
/// content-pipeline.md §3. Field names map from snake_case YAML.
///
/// <para><b>THE RULE FOR EVERY <c>List&lt;&gt;</c> IN EVERY YAML SCHEMA ON THIS
/// SITE LIVES HERE</b> (WI-561 for <c>images</c>, WI-583 for the other eight),
/// and it is load-bearing rather than defensive. A key with nothing under it —
/// <c>sources:</c> and then the closing <c>---</c>, which is exactly what a
/// half-typed entry looks like — makes YamlDotNet assign NULL OVER the property
/// initializer. Left as null that is not a worse error message, it is a
/// different FAILURE MODE in every place that catches a narrower exception than
/// the one it now gets:</para>
/// <list type="bullet">
///   <item><description><c>ContentChecker.CheckPage</c> catches only
///     <c>FormatException</c>, so the CI gate CRASHES instead of failing the
///     page by name — an unexplained red build, not a content error.</description></item>
///   <item><description><c>ContentStore.GetPage</c> catches only
///     <c>IOException</c>, so a reader gets a 500 ON A MEDICAL PAGE.</description></item>
///   <item><description><c>ContentStore.SearchPages</c> catches only
///     <c>FormatException</c>, so ONE dangling key takes site search down for
///     every page on the site, not just the broken one.</description></item>
///   <item><description>And off the page entirely: a dangling <c>also:</c> in a
///     GLOSSARY file is an <c>ArgumentNullException</c> inside the tooltip
///     matcher during the parse of all 55 pages, and either list key in
///     <c>taxonomy.yml</c> is walked in <see cref="TaxonomyStore"/>'s
///     CONSTRUCTOR — so the site does not boot.</description></item>
/// </list>
///
/// <para><b>Coalesced on WRITE, not on read.</b> WI-561 wrote
/// <c>get =&gt; _images ??= [];</c> and WI-583 copied it eight times before
/// <c>/review</c> found two things wrong with that shape: a getter that assigns
/// MUTATES an object this site caches and shares across requests
/// (<c>ContentStore</c>'s page cache), and a backing field that can be null
/// breaks the value-equality of a <c>record</c> schema like
/// <see cref="TumorType"/> — two instances reading equal could compare unequal.
/// Normalising in the setter holds the invariant strictly more often: the field
/// is never null, for a caller who assigns null as much as for YamlDotNet.</para>
///
/// <para><b>On the type and not at the call sites.</b> WI-561 fixed
/// <c>images</c> where it crashed, and the identical hole then sat in eight
/// siblings — a fix written where the crash happened teaches nothing to the
/// properties, or the TYPES, that have not crashed yet. Two of the nine
/// (<c>ContentBlockFrontMatter.Sources</c> and <c>GlossaryFrontMatter.Sources</c>)
/// were safe only because their single call site coalesced downstream, which is
/// a property of that call site and not of the schema.
/// <c>DanglingFrontMatterKeyTests.EveryYamlListPropertyOnEverySchemaRefusesNull</c>
/// sweeps the whole assembly by reflection, so a tenth one is covered the day it
/// is added rather than the day it breaks a page.</para>
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

    /// <summary>
    /// Coalesced on write — see the class remarks. <c>tags</c> is the one key of
    /// the nine with NO consumer anywhere today, so a dangling <c>tags:</c>
    /// crashes nothing: it is fixed here anyway because the hole is in the
    /// schema, not in whoever reads it next.
    /// </summary>
    [YamlMember(Alias = "tags")]
    public List<string> Tags
    {
        get => _tags;
        set => _tags = value ?? [];
    }

    /// <summary>
    /// Coalesced on write — see the class remarks. The worst of this type's
    /// four: <c>ContentStore.Parse</c> spreads this list into a collection
    /// expression, so a dangling <c>sources:</c> threw during PARSE and took the
    /// CI gate, the page and site search with it.
    /// </summary>
    [YamlMember(Alias = "sources")]
    public List<ContentSource> Sources
    {
        get => _sources;
        set => _sources = value ?? [];
    }

    [YamlMember(Alias = "reviewed")]
    public DateOnly? Reviewed { get; set; }

    [YamlMember(Alias = "review_due")]
    public DateOnly? ReviewDue { get; set; }

    [YamlMember(Alias = "volatile_figures")]
    public bool VolatileFigures { get; set; }

    [YamlMember(Alias = "reading_grade")]
    public double? ReadingGrade { get; set; }

    /// <summary>
    /// Coalesced on write — see the class remarks. Both of this one's
    /// dereferences are AFTER a successful parse, which is what made it the
    /// quietest of the four: <c>ContentPage.cshtml</c> reads
    /// <c>Disclaimers.Count</c> (a 500 inside the view, with <c>GetPage</c>
    /// already returned) and <c>ContentChecker</c> walks it as the last thing it
    /// does to a page.
    /// </summary>
    [YamlMember(Alias = "disclaimers")]
    public List<string> Disclaimers
    {
        get => _disclaimers;
        set => _disclaimers = value ?? [];
    }

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
    ///
    /// <para><b>Coalesced on write</b> (<c>/review</c>) — the first of the nine
    /// to be found, and the class remarks above are its finding generalised.
    /// Left as null it reached <see cref="ContentFigures"/> as a
    /// <c>NullReferenceException</c>.</para>
    /// </summary>
    [YamlMember(Alias = "images")]
    public List<ContentImage> Images
    {
        get => _images;
        set => _images = value ?? [];
    }

    private List<string> _tags = [];
    private List<ContentSource> _sources = [];
    private List<string> _disclaimers = [];
    private List<ContentImage> _images = [];
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
