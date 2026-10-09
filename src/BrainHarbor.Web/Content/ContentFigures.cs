using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BrainHarbor.Web.Content;

/// <summary>
/// WI-561: one picture on a curated page, resolved — the Markdown line's
/// reader-facing half (alt text, caption) joined to the front matter's
/// site-facing half (credit, licence, pixel size).
///
/// <para>The values are plain strings by the time this exists, not inline
/// trees: the alt text goes into an HTML ATTRIBUTE, and anything that put
/// markup in there (a glossary tooltip's <c>&lt;button&gt;</c>, say) would
/// render as literal angle brackets to a screen reader. Flattening here is
/// what makes that impossible rather than merely unlikely.</para>
/// </summary>
public sealed record ContentFigure(
    string Src,
    string Alt,
    string Caption,
    string Credit,
    string License,
    string LicenseUrl,
    string? SourceUrl,
    int Width,
    int Height,
    bool Lazy)
{
    /// <summary>A declared-decorative image carries no alt text and no caption.</summary>
    public bool Decorative => Alt.Length == 0;
}

/// <summary>
/// The figure in the document tree. A <c>&lt;figure&gt;</c> may not live
/// inside a <c>&lt;p&gt;</c>, so the paragraph Markdig built around the image
/// is REPLACED by this rather than decorated — which is also what keeps the
/// alt text out of reach of anything that rewrites paragraph inlines.
/// </summary>
public sealed class FigureBlock(ContentFigure figure) : LeafBlock(null!)
{
    public ContentFigure Figure { get; } = figure;
}

/// <summary>
/// WI-561: turns a lone Markdown image into a real figure, or fails the page.
///
/// <para>Authors write one line, in ordinary Markdown:</para>
/// <code>![A sample report with its parts labelled.](/img/figures/report.svg "What a report looks like. Yours may order the parts differently.")</code>
///
/// <para>The alt text is the brackets, the caption is the quoted title, and
/// the credit and licence come from the page's <c>images:</c> front matter
/// keyed by the same <c>src</c>. Every one of those is REQUIRED and a missing
/// one fails the page by name, the way a missing shared block does (WI-501):
/// an image with no alt text is invisible to a screen reader, an image with no
/// caption is a picture with no explanation, and an image with no licence is a
/// legal problem that renders perfectly.</para>
///
/// <para><b>Why validation lives here and not only in ContentCheck.</b>
/// <see cref="ContentStore.Parse"/> calls this, so the same failure is a build
/// failure in CI (ContentCheck turns a <see cref="FormatException"/> into a
/// Fail) AND a loud failure at runtime. A gate that only runs in CI is a gate
/// that a hand-edited page on the server walks straight through.</para>
/// </summary>
public static partial class ContentFigures
{
    /// <summary>
    /// Hosts whose images this site may not reuse (PLAN.md §5,
    /// content-pipeline §12.2 rule 6). Their TEXT is cited all over the
    /// corpus — cancer.gov is a source on most tumor hubs — so the ban has to
    /// be scoped to images, which is exactly why it is enforced on an image's
    /// own <c>source_url</c> rather than on the page's citation list.
    /// </summary>
    public static readonly IReadOnlyList<string> BannedImageSourceHosts =
    [
        "cancer.gov",                 // NCI embedded images are licensed stock
        "medlineplus.gov",            // ships with the monographs we never ingest
        "ahfsdruginformation.com",
    ];

    /// <summary>
    /// The same ban read off the CREDIT, because <c>source_url</c> is optional
    /// and a one-directional guard is half a guard (<c>/review</c>).
    ///
    /// <para>Without this, an NCI diagram committed to the repo and declared
    /// <c>credit: "National Cancer Institute"</c> with no <c>source_url</c>
    /// rendered on a green build — the host list never ran. The field a human
    /// fills in honestly is the one to read: somebody reusing an NCI picture
    /// writes NCI in the credit, because that is what the licence they think
    /// they have would require.</para>
    /// </summary>
    /// <para><b>Public since WI-562</b>, like <see cref="BannedImageSourceHosts"/>
    /// above: <c>ImagesNeededInventoryTests</c> reads this list to scan the
    /// SOURCING NOTES in <c>docs/images-needed.md</c> for a note that tells the
    /// next person to go and fetch a picture from one of these. That note is
    /// read weeks before any front matter is written, and a hand-copied list in
    /// the test would stop matching the moment a fifth entry is added here.</para>
    public static readonly string[] BannedImageCredits =
    [
        "national cancer institute",
        "cancer.gov",
        "medlineplus",
        "ahfs",
    ];

    /// <summary>"NCI" as a word, which a substring match would find inside "principal".</summary>
    [GeneratedRegex(@"\bnci\b", RegexOptions.IgnoreCase)]
    private static partial Regex NciAbbreviation();

    /// <summary>
    /// Alt text that opens by naming the medium. A screen reader has already
    /// said "image", so "Photo of a scanner" is heard as "image, photo of a
    /// scanner" — two words of noise in front of the only words that carry
    /// anything.
    ///
    /// <para><b>"Diagram of" was in this list and was taken out.</b> A screen
    /// reader announces "image", not "diagram", so naming the TYPE of a drawing
    /// is information rather than an echo — WAI's own tutorials write
    /// "Chart: …". Only the words that duplicate what the role already says
    /// belong here. This repo has ruled on the shape once already
    /// (<c>ThePageNeverAssertsAClosedCount</c>: a ban list that forbids the
    /// correct shape is worse than no ban list).</para>
    ///
    /// <para><b>Public since WI-562:</b> <c>ImagesNeededInventoryTests</c> holds the
    /// 148 DRAFT alt texts in <c>docs/images-needed.md</c> to this same list, so a
    /// draft cannot be written today that the build would reject at paste time. It
    /// reads this field rather than copying it — a sixth prefix added here would
    /// otherwise leave every draft checked against a list that no longer exists.</para>
    /// </summary>
    public static readonly string[] AltPrefixesToAvoid =
        ["image of", "picture of", "photo of", "photograph of", "graphic of"];

    /// <summary>
    /// Replaces every lone-image paragraph in <paramref name="document"/> with
    /// a <see cref="FigureBlock"/>, and returns how many it built.
    ///
    /// <para>Returns 0 and touches nothing when the page has no images and
    /// declares none, which is every page in the corpus today — the WI-501
    /// regression property (an untouched page renders byte-identically) is a
    /// consequence of this early return, not a hope.</para>
    /// </summary>
    public static int Apply(
        MarkdownDocument document,
        IReadOnlyList<ContentImage> declared,
        string markdown,
        string describedAs)
    {
        var images = document.Descendants<LinkInline>().Where(link => link.IsImage).ToList();

        // INDEXED BEFORE THE EARLY RETURN, so a malformed declaration is caught
        // on a page with no pictures too. Indexing only when an image exists
        // meant `images:` with a typo'd key — `scr:` for `src:` — was silently
        // ignored on exactly the page where the mistake is most likely: one
        // where the author has not written the Markdown line yet.
        var bySrc = IndexDeclarations(declared, describedAs);

        if (images.Count == 0)
        {
            // The audits run FIRST, because an image line that did not parse
            // also leaves its declaration looking unused — and "you declared a
            // picture that is not there" is a true sentence about the wrong
            // defect when the picture IS there and the caption broke the line.
            AuditUnparsedImages(document, markdown, 0, describedAs);

            // A declaration with no picture is not harmless: it is usually an
            // image whose Markdown line was deleted or never written, and the
            // page would publish with a credit for something nobody can see.
            RejectUnusedDeclarations(bySrc.Keys, [], describedAs);
            return 0;
        }

        var used = new HashSet<string>(StringComparer.Ordinal);
        var built = 0;

        foreach (var image in images)
        {
            var paragraph = RequireLoneImageParagraph(image, describedAs);
            var figure = Resolve(image, bySrc, lazy: built > 0, describedAs);

            if (!used.Add(figure.Src))
            {
                // One entry, one appearance. Two figures from one declaration
                // would also make the corpus sweep's "as many figures as the
                // page declares" false, and a rule the parser permits and a
                // test forbids is two rules (/review).
                throw new FormatException(
                    $"Content page '{describedAs}' shows the image '{figure.Src}' more than once. "
                    + "One picture, one place: a reader who meets the same figure twice reads the "
                    + "second one as a new one.");
            }

            var parent = paragraph.Parent
                ?? throw new FormatException(
                    $"Content page '{describedAs}' has an image in a paragraph with no parent block.");
            var index = parent.IndexOf(paragraph);
            parent.RemoveAt(index);
            parent.Insert(index, new FigureBlock(figure));
            built++;
        }

        AuditUnparsedImages(document, markdown, built, describedAs);
        RejectUnusedDeclarations(bySrc.Keys, used, describedAs);
        return built;
    }

    /// <summary>
    /// Every check above can only inspect images Markdig actually built — and
    /// the likeliest mistake in an image line stops one from being built at
    /// all, so there is nothing in the tree to find and the page ships the
    /// source text as visible prose. The recorded case is a caption with a
    /// double quote in it: <c>"a "grade 2" tumor"</c> ends the title early,
    /// the whole line stops being an image, and a patient reads
    /// <c>![a scan](/img/figures/x.svg "a "grade 2" tumor")</c> off the page.
    ///
    /// <para>So the raw source is audited alongside the tree, the way
    /// <see cref="ReaderGate"/> audits its fence lines — and for the same
    /// reason it was written there: a typo whose only symptom is bracket text
    /// on a medical page has to fail the build.</para>
    ///
    /// <para>Runs after the per-image loop, so the tree-level failures (an
    /// image in a sentence, two on one line) are reported with their own
    /// messages first — and BEFORE the unused-declaration check, which an
    /// unparsed line also trips while naming the wrong cause.</para>
    ///
    /// <para><b>TWO AUDITS, because neither one sees what the other does</b>
    /// (<c>/review</c>). The line count alone was defeated by two characters:
    /// <c>- ![a scan](x "a "grade 2" tumor")</c> does not START with
    /// <c>![</c>, nor does an image with any prose in front of it, and both
    /// shipped the raw Markdown to a reader. The TREE pass catches those,
    /// wherever on the line they are. But the tree pass cannot see an image
    /// line indented by four spaces, because CommonMark makes that an indented
    /// CODE block with no inlines in it at all — which is
    /// <see cref="ReaderGate"/>'s recorded case, and the line count is what
    /// catches it. Deleting either one re-opens a hole with no symptom.</para>
    /// </summary>
    private static void AuditUnparsedImages(
        MarkdownDocument document, string markdown, int built, string describedAs)
    {
        // THE TREE PASS. Anything that still reads as image syntax after
        // parsing never became an image. Fenced and indented code blocks have
        // no inlines, so a page documenting the syntax is untouched, and so is
        // `![...]()` written as inline code (a CodeInline, not a literal).
        //
        // THE LITERALS OF A BLOCK ARE JOINED FIRST, because Markdig splits a
        // failed image into "![" and the rest: scanning literal by literal
        // matched neither half and the whole pass was dead on arrival. (Found
        // by probing the real parser rather than by reading it.)
        foreach (var block in document.Descendants().OfType<LeafBlock>())
        {
            if (block.Inline is null)
            {
                continue;
            }

            var match = UnparsedImagePattern().Match(FlattenInlines(block.Inline));
            if (match.Success)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' renders '{match.Value}' as literal text — it looks "
                    + "like an image and did not parse as one, so a reader sees the brackets. The "
                    + "usual cause is a double quote inside the caption: write the caption in single "
                    + "quotes instead, ![what it shows](/img/figures/x.svg 'a \"grade 2\" tumor'). An "
                    + "image also has to be a paragraph of its own, not a bullet or part of a "
                    + "sentence.");
            }
        }

        // THE LINE PASS, for the one case the tree cannot hold: four spaces or
        // a tab in front of the line makes it an indented code block, which
        // renders the words in full AND hides them from every inline check.
        var lines = 0;
        foreach (var (_, text) in ContentBlocks.LinesOutsideFencedCode(markdown))
        {
            if (text.TrimStart(' ', '\t').StartsWith("![", StringComparison.Ordinal))
            {
                lines++;
            }
        }

        if (lines <= built)
        {
            return;
        }

        throw new FormatException(
            $"Content page '{describedAs}' has {lines} line(s) starting with '![' but {built} "
            + "figure(s) parsed. One of them is not valid Markdown, so it renders as literal bracket "
            + "text to a reader. The likeliest cause left at this point is INDENTATION: four spaces "
            + "or a tab makes the line an indented code block, which prints the brackets and hides "
            + "them from every other check.");
    }

    /// <summary>
    /// Image syntax that survived parsing as plain text. Deliberately loose on
    /// the alt text and tight on the shape — <c>![</c>…<c>](</c> is not
    /// something a sentence produces by accident, and the two recorded ways to
    /// write a broken image (a quote in the caption, a bullet in front) both
    /// leave it intact.
    /// </summary>
    [GeneratedRegex(@"!\[[^\]]*\]\([^\s)]*")]
    private static partial Regex UnparsedImagePattern();

    private static Dictionary<string, ContentImage> IndexDeclarations(
        IReadOnlyList<ContentImage> declared, string describedAs)
    {
        var bySrc = new Dictionary<string, ContentImage>(StringComparer.Ordinal);

        var position = 0;
        foreach (var image in declared)
        {
            position++;

            // An empty list ITEM ("  -" with nothing after it) deserializes to
            // a null element, and a null element dereferenced is a 500 on a
            // medical page rather than a build failure (/review).
            if (image is null)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' has an empty entry at position {position} of its "
                    + "'images:' list. Either fill it in or delete the dash.");
            }

            var src = (image.Src ?? "").Trim();
            if (src.Length == 0)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' declares an image with no 'src' — there is nothing "
                    + "to match it to the picture in the body.");
            }

            if (!bySrc.TryAdd(src, image))
            {
                // Two entries for one file is two licences for one picture,
                // and whichever one rendered would be a coin toss.
                throw new FormatException(
                    $"Content page '{describedAs}' declares the image '{src}' twice in front matter. "
                    + "One entry per file: the second would be the credit nobody sees.");
            }
        }

        return bySrc;
    }

    /// <summary>
    /// Reads the INDEXED keys rather than the raw list, so it cannot disagree
    /// with <see cref="IndexDeclarations"/> about what was declared — the first
    /// version filtered empty sources out here, which quietly forgave the one
    /// malformed entry the index exists to reject.
    /// </summary>
    private static void RejectUnusedDeclarations(
        IEnumerable<string> declared, HashSet<string> used, string describedAs)
    {
        var unused = declared.Where(src => !used.Contains(src)).ToList();

        if (unused.Count > 0)
        {
            throw new FormatException(
                $"Content page '{describedAs}' declares image(s) no line of the body shows: "
                + $"{string.Join(", ", unused)}. Either write the Markdown image line or delete the "
                + "front-matter entry — a credit for a picture that is not there is a credit for "
                + "nothing.");
        }
    }

    /// <summary>
    /// A figure is a BLOCK, and a TOP-LEVEL one. An image mixed into a
    /// sentence would render as a bare <c>&lt;img&gt;</c> inside the paragraph
    /// — no caption, no credit, no licence anywhere near it — so it fails
    /// instead, with the fix in the message.
    ///
    /// <para><b>And a paragraph is not enough on its own</b> (<c>/review</c>).
    /// A bullet, a blockquote and a TABLE CELL all contain paragraphs, so all
    /// three produced a real figure — credited, but in a shape nobody
    /// designed, styled or measured. The table cell is the one that bites: a
    /// 1200px picture in an auto-layout <c>&lt;td&gt;</c> is the classic way
    /// to send a 390px screen sideways, and the phone test only ever sees the
    /// top-level case. A reader-choice gate is allowed, because a figure
    /// inside <c>:::outlook</c> renders correctly and is a sensible place for
    /// one.</para>
    /// </summary>
    private static ParagraphBlock RequireLoneImageParagraph(LinkInline image, string describedAs)
    {
        if (image.Parent is { } container
            && container.ParentBlock is ParagraphBlock paragraph
            && ReferenceEquals(OnlyMeaningfulInline(container), image))
        {
            if (paragraph.Parent is MarkdownDocument or Markdig.Extensions.CustomContainers.CustomContainer)
            {
                return paragraph;
            }

            throw new FormatException(
                $"Content page '{describedAs}' puts the image '{image.Url}' inside a list, a quote or a "
                + "table. A figure is a section of the page, not something nested in one: it has a "
                + "caption and a credit under it, and it is sized against the reading column. Put it "
                + "on its own between two blank lines.");
        }

        throw new FormatException(
            $"Content page '{describedAs}' puts the image '{image.Url}' in a line with other text (or "
            + "in a heading). An image has to be a paragraph of its own: a blank line, the image line, "
            + "a blank line. Anywhere else it renders with no caption and no credit.");
    }

    /// <summary>
    /// The one inline that carries content, ignoring whitespace and the soft
    /// line breaks a hard-wrapped source file is full of — or null when there
    /// is more than one. Markdig gives an image line a single
    /// <see cref="LinkInline"/>, but an author who wrapped the line, or left a
    /// trailing space, has a whitespace literal in there too and should not be
    /// told their figure is prose.
    /// </summary>
    private static Inline? OnlyMeaningfulInline(ContainerInline container)
    {
        Inline? only = null;

        foreach (var inline in container)
        {
            if (inline is LineBreakInline)
            {
                continue;
            }

            if (inline is LiteralInline literal && literal.Content.ToString().Trim().Length == 0)
            {
                continue;
            }

            if (only is not null)
            {
                return null;
            }

            only = inline;
        }

        return only;
    }

    private static ContentFigure Resolve(
        LinkInline image,
        Dictionary<string, ContentImage> bySrc,
        bool lazy,
        string describedAs)
    {
        var src = (image.Url ?? "").Trim();

        if (src.Length == 0)
        {
            throw new FormatException(
                $"Content page '{describedAs}' has an image with no file path.");
        }

        // A remote <img src> on a medical page tells somebody else's server
        // which page a reader opened, and it rots on their schedule rather
        // than ours. The file lives in the repo or the figure does not exist.
        //
        // NARROWED TO /img/ AND NO '..' SEGMENTS (/review): "starts with a
        // slash and has no //" also accepted '/appsettings.json' and
        // '/img/../secret.svg', and the second one walks out of wwwroot past
        // the test that checks the file is in the repo.
        if (!src.StartsWith("/img/", StringComparison.Ordinal)
            || src.Contains("//", StringComparison.Ordinal)
            || src.Split('/').Contains(".."))
        {
            throw new FormatException(
                $"Content page '{describedAs}' points an image at '{src}'. Images are served from this "
                + "site, out of wwwroot/img/: commit the file and use a path like "
                + "'/img/figures/x.svg'. A remote image tells another server who is reading this "
                + "page, and disappears when they move it.");
        }

        if (!bySrc.TryGetValue(src, out var declared))
        {
            var known = bySrc.Count == 0
                ? "the page declares none"
                : "declared: " + string.Join(", ", bySrc.Keys.OrderBy(k => k, StringComparer.Ordinal));
            throw new FormatException(
                $"Content page '{describedAs}' shows the image '{src}' with no front-matter entry for "
                + $"it ({known}). Add it under 'images:' with 'credit', 'license', 'license_url', "
                + "'width' and 'height'. (An image inside a SHARED BLOCK is not supported — a block "
                + "cannot carry its own credit yet, and an uncredited picture must not render.)");
        }

        var alt = AltText(image, declared, src, describedAs);
        var caption = Caption(image, declared, src, describedAs);

        if (!declared.Decorative
            && string.Equals(alt, caption, StringComparison.OrdinalIgnoreCase))
        {
            // The caption is on screen and the alt is read aloud. Identical,
            // a screen-reader user hears the same sentence twice and is told
            // nothing about the picture either time.
            throw new FormatException(
                $"Content page '{describedAs}' gives the image '{src}' the same words as its alt text "
                + "and its caption. The caption says what the picture MEANS; the alt text says what is "
                + "IN it, for someone who cannot see it.");
        }

        return new ContentFigure(
            src, alt, caption,
            RequireCredit(declared.Credit, src, describedAs),
            RequireField(declared.License, "license", src, describedAs,
                "the licence is the permission this site is relying on"),
            RequireUrl(declared.LicenseUrl, "license_url", src, describedAs),
            SourceUrl(declared, src, describedAs),
            RequireSize(declared.Width, "width", src, describedAs),
            RequireSize(declared.Height, "height", src, describedAs),
            lazy);
    }

    private static string AltText(
        LinkInline image, ContentImage declared, string src, string describedAs)
    {
        var alt = FlattenInlines(image).Trim();

        if (declared.Decorative)
        {
            if (alt.Length > 0)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' declares the image '{src}' decorative but gives it "
                    + $"alt text ('{alt}'). Decorative means a screen reader skips it: either drop "
                    + "'decorative: true' or empty the brackets.");
            }

            return "";
        }

        if (alt.Length == 0)
        {
            throw new FormatException(
                $"Content page '{describedAs}' shows the image '{src}' with no alt text. Write what is "
                + "in the picture inside the brackets: ![what it shows]({src} \"the caption\"). If the "
                + "picture really tells a reader nothing, say so on purpose with 'decorative: true' in "
                + "its front-matter entry (WCAG AA is a hard requirement).");
        }

        var lowered = alt.ToLowerInvariant();
        foreach (var prefix in AltPrefixesToAvoid)
        {
            if (lowered.StartsWith(prefix, StringComparison.Ordinal))
            {
                throw new FormatException(
                    $"Content page '{describedAs}' starts the alt text for '{src}' with "
                    + $"'{alt[..prefix.Length]}'. A screen reader already says 'image', so those words "
                    + "are heard twice. Start with what the picture shows.");
            }
        }

        return alt;
    }

    private static string Caption(
        LinkInline image, ContentImage declared, string src, string describedAs)
    {
        var caption = (image.Title ?? "").Trim();

        if (declared.Decorative)
        {
            if (caption.Length > 0)
            {
                throw new FormatException(
                    $"Content page '{describedAs}' declares the image '{src}' decorative but gives it a "
                    + $"caption ('{caption}'). A picture worth captioning is worth describing: drop "
                    + "'decorative: true' and write the alt text.");
            }

            return "";
        }

        if (caption.Length == 0)
        {
            throw new FormatException(
                $"Content page '{describedAs}' shows the image '{src}' with no caption. Put it in "
                + "quotes after the path: ![what it shows]({src} \"what it means\"). A figure with no "
                + "caption is a picture a reader has to interpret on their own.");
        }

        return caption;
    }

    private static string RequireField(
        string? value, string field, string src, string describedAs, string because)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException(
                $"Content page '{describedAs}' declares the image '{src}' with no '{field}' — "
                + $"{because} (PLAN.md §5).");
        }

        return value.Trim();
    }

    /// <summary>
    /// The credit, and the SECOND direction of the licensing ban.
    ///
    /// <para><c>source_url</c> is optional, so a ban that reads only the URL
    /// is off for every image that leaves it out — and an NCI diagram
    /// committed to the repo with <c>credit: "National Cancer Institute"</c>
    /// and no source URL rendered on a green build (<c>/review</c>). The
    /// credit is the field somebody reusing a picture fills in honestly,
    /// because the licence they believe they have is what tells them to.</para>
    /// </summary>
    private static string RequireCredit(string? value, string src, string describedAs)
    {
        var credit = RequireField(value, "credit", src, describedAs,
            "an image with no credit is one nobody can check the rights to");
        var lowered = credit.ToLowerInvariant();

        if (BannedImageCredits.Any(banned => lowered.Contains(banned, StringComparison.Ordinal))
            || NciAbbreviation().IsMatch(credit))
        {
            throw new FormatException(
                $"Content page '{describedAs}' credits the image '{src}' to '{credit}'. This site "
                + "never reuses NCI embedded images or anything shipped with the AHFS/MedlinePlus "
                + "drug monographs — they are licensed stock, not public domain (PLAN.md §5, "
                + "content-pipeline §12.2 rule 6). Their TEXT is still citable; their pictures are "
                + "not.");
        }

        return credit;
    }

    private static string RequireUrl(string? value, string field, string src, string describedAs)
    {
        var url = RequireField(value, field, src, describedAs,
            "a licence nobody can read is not a licence");

        if (!IsHttpUrl(url))
        {
            throw new FormatException(
                $"Content page '{describedAs}' gives the image '{src}' a '{field}' of '{url}', which is "
                + "not a web address. It has to start with http:// or https:// so a reader can open it.");
        }

        return url;
    }

    private static string? SourceUrl(ContentImage declared, string src, string describedAs)
    {
        var url = (declared.SourceUrl ?? "").Trim();

        // Optional, and deliberately so: a diagram drawn for this site has no
        // "where it came from". What it may NOT be is wrong.
        if (url.Length == 0)
        {
            return null;
        }

        if (!IsHttpUrl(url))
        {
            throw new FormatException(
                $"Content page '{describedAs}' gives the image '{src}' a 'source_url' of '{url}', which "
                + "is not a web address. Leave it out for a picture made for this site.");
        }

        var host = new Uri(url).Host;
        foreach (var banned in BannedImageSourceHosts)
        {
            if (host.Equals(banned, StringComparison.OrdinalIgnoreCase)
                || host.EndsWith("." + banned, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    $"Content page '{describedAs}' takes the image '{src}' from {host}. This site never "
                    + "reuses NCI embedded images or anything shipped with the AHFS/MedlinePlus drug "
                    + "monographs — they are licensed stock, not public domain (PLAN.md §5, "
                    + "content-pipeline §12.2 rule 6). Their TEXT is still citable; their pictures are "
                    + "not.");
            }
        }

        return url;
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static int RequireSize(int value, string field, string src, string describedAs)
    {
        if (value <= 0)
        {
            throw new FormatException(
                $"Content page '{describedAs}' declares the image '{src}' with no '{field}'. The pixel "
                + "size is what lets the browser hold the space open: without it a lazy-loaded figure "
                + "shoves the text under it down the screen when it arrives, moving the line the "
                + "reader is on.");
        }

        return value;
    }

    /// <summary>
    /// The alt text as plain characters. Markdig keeps an image's alt text as
    /// child inlines, so <c>![a **scan**](x)</c> arrives as three of them —
    /// and emphasis inside an attribute is either dropped or rendered as
    /// literal asterisks. Flattened, it is neither.
    /// </summary>
    private static string FlattenInlines(ContainerInline container) =>
        string.Concat(container.Descendants().Select(inline => inline switch
        {
            LiteralInline literal => literal.Content.ToString(),
            CodeInline code => code.Content,
            // `&copy;` is an HtmlEntityInline, not a literal. Unhandled it was
            // dropped SILENTLY, so "![&copy; 2024 Someone]" became alt text
            // reading "2024 Someone" — characters lost inside the one
            // attribute a blind reader has (/review).
            HtmlEntityInline entity => entity.Transcoded.ToString(),
            LineBreakInline => " ",
            _ => "",
        }));
}

/// <summary>
/// Renders a resolved figure: the picture, the caption a reader is meant to
/// read, and the credit the licence requires — in one <c>&lt;figcaption&gt;</c>
/// so the credit cannot be separated from the thing it credits.
/// </summary>
public sealed class FigureRenderer : HtmlObjectRenderer<FigureBlock>
{
    protected override void Write(HtmlRenderer renderer, FigureBlock block)
    {
        var figure = block.Figure;

        if (!renderer.EnableHtmlForBlock)
        {
            // UNREACHABLE TODAY — nothing in this repo builds a non-HTML
            // renderer — and it is the one place in this file that writes
            // author text without escaping it. That is correct for a plain
            // text sink and WRONG the moment anything renders this into
            // markup, so it must stay a plain-text-only branch (/review).
            //
            // A plain-text render (a digest, a search snippet) has no picture
            // in it. The caption is still content, so it is the one part that
            // survives — an <img> written into plain text would be noise and
            // the credit would be a dangling licence for nothing.
            renderer.WriteLine(figure.Caption);
            return;
        }

        renderer.EnsureLine();
        renderer.WriteLine("<figure class=\"figure\">");

        renderer.Write("<img class=\"figure__image\" src=\"");
        renderer.WriteEscapeUrl(figure.Src);
        renderer.Write("\" alt=\"");
        renderer.WriteEscape(figure.Alt);
        renderer.Write($"\" width=\"{figure.Width}\" height=\"{figure.Height}\" decoding=\"async\"");
        if (figure.Lazy)
        {
            // Everything after the first figure is below the fold on a 390px
            // screen. The FIRST one may be the picture already on screen, and
            // deferring that one delays the only image a reader ever sees.
            renderer.Write(" loading=\"lazy\"");
        }
        renderer.WriteLine(">");

        // A DECORATIVE IMAGE STILL GETS A FIGCAPTION, holding the credit
        // alone, and the trade-off is written down rather than discovered: it
        // means a screen reader announces a captioned figure for a picture
        // whose `alt=""` says there is nothing to see. The alternative is a
        // credit that is not attached to the thing it credits, which is the
        // defect this whole component exists to prevent. The licence does not
        // care whether a picture means anything.
        renderer.WriteLine("<figcaption class=\"figure__caption\">");

        if (!figure.Decorative)
        {
            renderer.Write("<span class=\"figure__text\">");
            renderer.WriteEscape(figure.Caption);
            renderer.WriteLine("</span>");
        }

        renderer.Write("<span class=\"figure__credit\">Image: ");
        renderer.WriteEscape(figure.Credit);
        renderer.Write(". <a href=\"");
        renderer.WriteEscapeUrl(figure.LicenseUrl);
        renderer.Write("\" rel=\"license\">");
        renderer.WriteEscape(figure.License);
        renderer.Write("</a>.");

        if (figure.SourceUrl is { } source)
        {
            renderer.Write(" <a href=\"");
            renderer.WriteEscapeUrl(source);
            renderer.Write("\">Where this picture came from</a>.");
        }

        renderer.WriteLine("</span>");

        renderer.WriteLine("</figcaption>");
        renderer.WriteLine("</figure>");
    }
}

/// <summary>
/// Registers <see cref="FigureRenderer"/>. Marking happens post-parse via
/// <see cref="ContentFigures.Apply"/>, the way the glossary marker works — the
/// credits live in a page's front matter, and the pipeline is shared by every
/// page, so a pipeline-level processor could not see them.
/// </summary>
public sealed class FigureExtension : Markdig.IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(Markdig.MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer html && !html.ObjectRenderers.Contains<FigureRenderer>())
        {
            html.ObjectRenderers.Add(new FigureRenderer());
        }
    }

    /// <summary>
    /// Renders a probe figure and throws unless it came out as one, the way
    /// <see cref="ReaderGateExtension.Verify"/> does — and for a failure of
    /// the same class. Markdig has no fallback renderer for an unknown
    /// <see cref="LeafBlock"/>, so a pipeline built without this extension
    /// does not render a broken picture: it renders NOTHING, silently, and
    /// the credit and the licence go with it. A one-line mistake in a pipeline
    /// definition would publish a page that has quietly dropped the only thing
    /// tying an image to its rights.
    /// </summary>
    public static Markdig.MarkdownPipeline Verify(Markdig.MarkdownPipeline pipeline)
    {
        const string probe = "![A probe.](/img/dev/probe.svg \"A probe figure.\")";
        var document = Markdig.Markdown.Parse(probe, pipeline);

        ContentFigures.Apply(
            document,
            [new ContentImage
            {
                Src = "/img/dev/probe.svg",
                Credit = "BrainHarbor",
                License = "CC0 1.0",
                LicenseUrl = "https://creativecommons.org/publicdomain/zero/1.0/",
                Width = 2,
                Height = 1,
            }],
            probe,
            "the figure pipeline probe");

        using var writer = new StringWriter();
        var html = new HtmlRenderer(writer);
        pipeline.Setup(html);
        html.Render(document);
        writer.Flush();

        var rendered = writer.ToString();
        if (!rendered.Contains("<figure class=\"figure\">", StringComparison.Ordinal)
            || !rendered.Contains("figure__credit", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The curated-content Markdown pipeline does not render a figure. FigureExtension has "
                + "to be registered on it, or a picture and its credit vanish from the page with no "
                + "error anywhere. Rendered instead:\n" + rendered);
        }

        return pipeline;
    }
}
