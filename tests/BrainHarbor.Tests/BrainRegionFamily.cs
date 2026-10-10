using System.Xml.Linq;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-572: the location family — one drawing, fifteen files.
///
/// <para><c>dia-brain-regions.svg</c> is the master. The thirteen
/// <c>dia-region-*.svg</c> variants and <c>dia-region-names.svg</c> are that
/// file with ONE style block appended: a variant sets four custom properties
/// on one region's group id and hides every label but its own; the names file
/// swaps the nine plain labels for the words a report uses. Nothing else
/// differs, which is the point — the drawing exists exactly once in the repo,
/// and <see cref="BrainRegionFamilyTests"/> rebuilds all fourteen from the
/// master and fails on a byte.</para>
///
/// <para><b>Why a derivation and not fifteen drawings.</b> §12.34 shrank this
/// family from 23 drawings to one master plus variants on the grounds that a
/// region is a region — <c>dia-region-sellar</c> is honest on the pituitary
/// page and on the craniopharyngioma page. That argument only holds while the
/// variants really are the same drawing. Fifteen hand-kept copies would be
/// the same claim with nothing behind it: the first edit to the master would
/// leave fourteen pages showing a map that no longer matches.</para>
/// </summary>
internal static class BrainRegionFamily
{
    public const string MasterId = "dia-brain-regions";
    public const string NamesId = "dia-region-names";

    /// <summary>
    /// What a shaded region looks like: the site's accent, and the internal
    /// detail lines flipped to the page surface so a striation does not
    /// disappear into the shading it is drawn on.
    ///
    /// <para><b>The weight matters as much as the colour, and for one region
    /// it matters more.</b> Most regions are filled shapes, where shading is a
    /// 3x luminance change nobody can miss. The skull base is a LINE: at the
    /// first weight tried (9 against a default of 7) a greyscale print — which
    /// is how a patient prints a page — moved it from about 81 to about 98 on
    /// a 0-255 scale and thickened it by a quarter. Colour that only works in
    /// colour is colour-only meaning. 12 against 7 is a change the paper can
    /// carry.</para>
    /// </summary>
    private const string ShadeDeclarations =
        "--shade: #0d6a86; --stroke-shade: #0d6a86;\n"
        + "                     --detail-shade: #eef3f7; --line-weight: 12;";

    public static string FiguresDirectory => Path.Combine(
        CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "img", "figures");

    public static string FileFor(string figureId) =>
        Path.Combine(FiguresDirectory, figureId + ".svg");

    /// <summary>The master as it sits on disk, line endings and all.</summary>
    public static string Master() => File.ReadAllText(FileFor(MasterId));

    /// <summary>
    /// The regions the master DRAWS, read off the drawing rather than listed
    /// here: every <c>id="region-…"</c> group. A region added to the master
    /// with no variant file therefore fails the family test, and a variant for
    /// a region nobody drew cannot be derived at all.
    /// </summary>
    public static IReadOnlyList<string> RegionIds(string master) =>
        [.. System.Text.RegularExpressions.Regex
            .Matches(master, @"id=""region-([a-z-]+)""")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>Every file in the family, master first.</summary>
    public static IReadOnlyList<string> Members(string master) =>
        [MasterId, NamesId, .. RegionIds(master).Select(r => "dia-region-" + r)];

    public static bool IsMember(string figureId) =>
        figureId == MasterId || figureId.StartsWith("dia-region-", StringComparison.Ordinal);

    /// <summary>
    /// A derived file, built from the master. Deterministic, and the body
    /// between the opening tag and the appended block is the master's own
    /// bytes — <see cref="BrainRegionFamilyTests"/> asserts that separately,
    /// because "derived" is only worth anything if the drawing cannot drift.
    /// </summary>
    public static string Derive(string master, string figureId)
    {
        if (figureId == MasterId)
        {
            return master;
        }

        var lf = master.Replace("\r\n", "\n");
        var region = figureId == NamesId ? null : figureId["dia-region-".Length..];

        var banner = region is null
            ? Banner(figureId,
                "the master drawing with the nine labels swapped for the words a report\n     uses")
            : Banner(figureId,
                $"the master drawing with the {region} region shaded and every label but\n     its own hidden");

        var ariaLabel = region is null
            ? "A side view of the brain, labelled with the words a report uses."
            : "A side view of the brain with one part shaded.";

        var rules = region is null
            ? """
                  #labels .label .plain { display: none; }
                  #labels .label .report { display: inline; }
              """
            : $$"""
                  #region-{{region}} { {{ShadeDeclarations}} }
                  #labels .label { display: none; }
                  #labels .label[data-region="{{region}}"] { display: inline; }
              """;

        var withBanner = ReplaceLeadingComment(lf, banner, figureId);
        var withAria = ReplaceAriaLabel(withBanner, ariaLabel, figureId);

        var closing = "</svg>\n";
        if (!withAria.EndsWith(closing, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{MasterId}.svg does not end with a lone </svg> line, so a variant cannot be "
                + "appended to it.");
        }

        var appended = withAria[..^closing.Length]
            + "\n  <!-- DERIVED. See the banner at the top of this file. -->\n"
            + "  <style>\n" + rules + "\n  </style>\n"
            + closing;

        return master.Contains("\r\n", StringComparison.Ordinal)
            ? appended.Replace("\n", "\r\n")
            : appended;
    }

    private static string Banner(string figureId, string what) =>
        $"""
        <!-- {figureId}: DERIVED FROM {MasterId}.svg (WI-572). DO NOT EDIT.

             This file is {what}.
             BrainRegionFamilyTests rebuilds it from the master and fails on any
             difference, so an edit here is lost and an edit to the master is carried
             to the whole family. Change the drawing in the master.

             Drawn for BrainHarbor, CC0. NEVER an NCI embedded image, and nothing out
             of the AHFS or MedlinePlus monographs (PLAN.md §5). -->
        """;

    /// <summary>
    /// Swaps the master's own comment for the derived file's banner — the
    /// master's says "this file is the master of a family of fifteen", which
    /// is a false sentence at the top of a variant and exactly the kind of
    /// copied prose this repo keeps finding wrong in the copies (§12.20).
    /// </summary>
    private static string ReplaceLeadingComment(string svg, string banner, string figureId)
    {
        var start = svg.IndexOf("<!--", StringComparison.Ordinal);
        var end = svg.IndexOf("-->", StringComparison.Ordinal);

        if (start < 0 || end < start)
        {
            throw new InvalidOperationException(
                $"{MasterId}.svg has no leading comment to replace while deriving {figureId}.");
        }

        return svg[..start] + banner + svg[(end + 3)..];
    }

    private static string ReplaceAriaLabel(string svg, string label, string figureId)
    {
        const string pattern = @"aria-label=""[^""]*""";

        if (System.Text.RegularExpressions.Regex.Matches(svg, pattern).Count != 1)
        {
            throw new InvalidOperationException(
                $"{MasterId}.svg needs exactly one aria-label to rewrite while deriving "
                + $"{figureId}; a variant with the master's description would tell a screen "
                + "reader the whole map is labelled when one part is shaded.");
        }

        return System.Text.RegularExpressions.Regex.Replace(
            svg, pattern, $@"aria-label=""{label}""");
    }

    /// <summary>
    /// The labels a reader actually SEES on one file, read out of the master's
    /// markup and the derivation's own rules rather than by evaluating CSS:
    /// the master shows the nine it labels, a variant shows the one for its
    /// region, and the names file shows the nine in a report's words.
    ///
    /// <para>This is what <c>EveryLabelIsAClaimThePageOrItsRouteCanAlsoMake</c> checks
    /// against the pages — a label naming a place the text never names is a
    /// picture carrying information no screen reader can reach.</para>
    /// </summary>
    public static IReadOnlyList<string> VisibleLabels(string master, string figureId)
    {
        var labels = XDocument.Parse(master)
            .Descendants()
            .Where(e => e.Name.LocalName == "g"
                && ((string?)e.Attribute("class"))?.Split(' ').Contains("label") == true)
            .ToList();

        var wanted = figureId == MasterId || figureId == NamesId
            ? labels.Where(e => ((string?)e.Attribute("class"))?
                .Contains("label--variant-only", StringComparison.Ordinal) != true)
            : labels.Where(e => (string?)e.Attribute("data-region")
                == figureId["dia-region-".Length..]);

        var wantedClass = figureId == NamesId ? "report" : "plain";

        return [.. wanted
            .Select(label => label.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "text"
                    && (string?)e.Attribute("class") == wantedClass))
            .Where(text => text is not null)
            .Select(text => Words(text!))];
    }

    /// <summary>
    /// A label's words with its line breaks put back as spaces. A label is one
    /// <c>&lt;text&gt;</c> with a <c>&lt;tspan&gt;</c> per line, and
    /// <c>XElement.Value</c> would run the lines together — "The side,near the
    /// ear" is not a phrase any page contains.
    /// </summary>
    private static string Words(XElement text) => string.Join(' ', text
        .DescendantNodesAndSelf()
        .OfType<XText>()
        .Select(node => node.Value.Trim())
        .Where(value => value.Length > 0));
}
