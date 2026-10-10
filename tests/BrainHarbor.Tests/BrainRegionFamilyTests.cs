using System.Text.RegularExpressions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-572: the fifteen files of the location family are one drawing.
///
/// <para>§12.34 shrank this family from 23 drawings to a master and its
/// variants on the grounds that <b>a region is a region</b> — the same shaded
/// map is honest on the pituitary page and on the craniopharyngioma page.
/// That argument is only true while the variants really are the same drawing,
/// and fifteen hand-kept copies would be the same claim with nothing behind
/// it: the first edit to the master would leave fourteen pages showing a map
/// that no longer matches, and no reader could tell.</para>
///
/// <para><b>So the variants are DERIVED and this file is the proof.</b> Every
/// one is rebuilt here from <c>dia-brain-regions.svg</c> and compared character
/// for character (line endings excluded, because this repo hands every working
/// tree CRLF and CI sees LF). To change any of them, change the master and
/// regenerate:</para>
/// <code>BH_WRITE_REGION_MAPS=1 dotnet test --filter BrainRegionFamilyTests.WritesTheFamilyOnlyWhenAskedTo</code>
/// <para>The filter names ONE test on purpose: with the variable set, the two
/// drift tests below would be certifying files this run may have just
/// rewritten, so they refuse to run at all.</para>
/// </summary>
public class BrainRegionFamilyTests
{
    private static string Master => BrainRegionFamily.Master();

    private static string Lf(string text) => text.Replace("\r\n", "\n");

    private static string Committed(string figureId) =>
        File.ReadAllText(BrainRegionFamily.FileFor(figureId));

    /// <summary>
    /// WI-583: the line endings of the DERIVED output, which is the one property
    /// <see cref="Lf"/> cannot stand in for.
    ///
    /// <para><c>Derive</c> re-expands LF to CRLF when the master has CRLF, and it
    /// was expanding over banner text that already carried CRLF of its own —
    /// producing nine <c>\r\r\n</c> per file. <c>Lf</c> collapses the trailing
    /// pair and leaves the bare CR, so <see cref="EveryVariantIsTheMasterRebuilt"/>
    /// failed with a diff a reader could not see. <b>It went red with nobody
    /// touching it</b>: the regenerator had just written the files as LF, git
    /// stores them LF and checks them out CRLF, and the first checkout after the
    /// commit flipped this method's input. <b>And it could not fail on CI</b>,
    /// where the master is LF and the expanding branch never runs — so the
    /// repo's own CRLF/LF discipline was being asserted by a guard that only
    /// ever saw one of the two.</para>
    /// </summary>
    [Fact]
    public void DerivingTwiceOverDifferentLineEndingsGivesOneDrawing()
    {
        RefuseToCertifyInRegenerationMode();

        var crlf = Lf(Master).Replace("\n", "\r\n");
        var lf = Lf(Master);

        foreach (var member in BrainRegionFamily.Members(lf)
                     .Where(m => m != BrainRegionFamily.MasterId))
        {
            var fromCrlf = BrainRegionFamily.Derive(crlf, member);
            var fromLf = BrainRegionFamily.Derive(lf, member);

            // THE ASSERTION THAT WAS MISSING: not "they agree once normalised"
            // — that is what hid this — but that each output's line endings are
            // UNIFORM. A bare CR survives every normaliser in this file.
            Assert.DoesNotContain("\r", fromLf);

            var pairs = fromCrlf.Split("\r\n").Length - 1;
            Assert.Equal(pairs, fromCrlf.Count(c => c == '\n'));
            Assert.Equal(pairs, fromCrlf.Count(c => c == '\r'));

            Assert.Equal(fromLf, Lf(fromCrlf));
        }
    }

    /// <summary>
    /// WI-583: the normaliser has to map a LONE CR too, and the break harness is
    /// what forced this test to exist.
    ///
    /// <para><c>/review</c> argued it from <c>"\r\r\n"</c>, and the break harness
    /// showed that case is ALREADY covered — the CRLF branch normalises once
    /// into <c>lf</c> and once into <c>normalized</c>, and two sequential
    /// single-pass replaces between them collapse the doubled CR. The mutation
    /// stayed green until this test fed the input that really is uncovered: a
    /// LONE CR, which <c>Replace("\r\n", "\n")</c> cannot see at all. <b>A fix
    /// whose mutation survives has not been shown to do anything</b>, and the
    /// first version of this test proved the wrong thing.</para>
    /// </summary>
    [Fact]
    public void ADerivationFromAMasterWithAStrayCarriageReturnIsStillUniform()
    {
        RefuseToCertifyInRegenerationMode();

        // A CR at the START of a line, mid-file: it removes no newline, splits
        // no token, and leaves the lone "</svg>\n" ending Derive insists on, so
        // every structural check still passes and the byte reaches the
        // normaliser. Corrupting the whole file instead would be rejected
        // before the normaliser is ever reached, which is why this is precise.
        // THE LAST indented line, not the first (which cost a round): the
        // master opens with a banner comment that `Derive` replaces WHOLESALE,
        // so a corruption placed there is discarded before the normaliser sees
        // it and the mutation stayed green. The last one is inside the drawing,
        // which is the half that is copied through byte for byte.
        var lf = Lf(Master);
        var corrupted = ReplaceLast(lf, "\n  ", "\n\r  ");
        Assert.Contains("\n\r", corrupted);
        Assert.DoesNotContain("\r\n", corrupted);

        foreach (var member in BrainRegionFamily.Members(lf)
                     .Where(m => m != BrainRegionFamily.MasterId))
        {
            var derived = BrainRegionFamily.Derive(corrupted, member);

            // UNIFORMITY, not equality with the clean derivation: mapping the
            // stray CR to a newline legitimately adds a line, so the content
            // differs. What must hold is that no CR escapes — this master is
            // CRLF-free, so the output has to be too.
            Assert.DoesNotContain("\r", derived);
        }
    }

    private static string ReplaceLast(string text, string find, string replacement)
    {
        var at = text.LastIndexOf(find, StringComparison.Ordinal);
        Assert.True(at >= 0, $"'{find}' is not in the master");
        return string.Concat(text.AsSpan(0, at), replacement, text.AsSpan(at + find.Length));
    }

    /// <summary>
    /// THE REGENERATOR, and it only runs when asked. A test that rewrites the
    /// files it is checking would turn every drift into a silent pass, so this
    /// one does nothing at all unless <c>BH_WRITE_REGION_MAPS=1</c> is in the
    /// environment — which is how the committed variants were produced, and
    /// the only supported way to change one.
    /// </summary>
    [Fact]
    public void WritesTheFamilyOnlyWhenAskedTo()
    {
        var master = Master;

        if (Environment.GetEnvironmentVariable("BH_WRITE_REGION_MAPS") != "1")
        {
            // A test that returns early reports GREEN while doing nothing, so
            // the ordinary run still asserts the thing that makes the early
            // return correct: the fourteen files it would have written are
            // already there to be checked by the two tests below.
            Assert.All(
                BrainRegionFamily.Members(master),
                member => Assert.True(File.Exists(BrainRegionFamily.FileFor(member))));
            return;
        }

        foreach (var member in BrainRegionFamily.Members(master).Where(m => m != BrainRegionFamily.MasterId))
        {
            File.WriteAllText(BrainRegionFamily.FileFor(member),
                BrainRegionFamily.Derive(master, member));
        }
    }

    /// <summary>
    /// A RUN IN REGENERATION MODE CERTIFIES NOTHING, and the two drift tests
    /// below say so rather than racing it. They are facts in the same class as
    /// the regenerator with no ordering between them, so running the whole
    /// class with the variable set would report red or green depending on
    /// xUnit's order — and if that variable ever leaked into CI or into a
    /// hook's environment, every drift would become a silent self-heal, which
    /// is the exact failure these tests exist to prevent (<c>/review</c>).
    /// </summary>
    private static void RefuseToCertifyInRegenerationMode() =>
        Assert.True(
            Environment.GetEnvironmentVariable("BH_WRITE_REGION_MAPS") != "1",
            "BH_WRITE_REGION_MAPS=1 is set, so the variants may have been rewritten by this "
            + "very run. Regenerate first, then run the suite with the variable unset.");

    [Fact]
    public void EveryVariantIsTheMasterRebuilt()
    {
        RefuseToCertifyInRegenerationMode();

        var master = Master;
        var members = BrainRegionFamily.Members(master);

        Assert.Equal(15, members.Count);

        foreach (var member in members.Where(m => m != BrainRegionFamily.MasterId))
        {
            var file = BrainRegionFamily.FileFor(member);
            Assert.True(File.Exists(file), $"{member}.svg is missing from wwwroot/img/figures");

            Assert.True(
                Lf(BrainRegionFamily.Derive(master, member)) == Lf(File.ReadAllText(file)),
                $"{member}.svg is not what the master derives. It is generated, not drawn: "
                + "change dia-brain-regions.svg and re-run with BH_WRITE_REGION_MAPS=1 "
                + "dotnet test --filter "
                + "BrainRegionFamilyTests.WritesTheFamilyOnlyWhenAskedTo, then run the suite "
                + "with the variable unset.");
        }
    }

    /// <summary>
    /// The property the test above only implies: the DRAWING is byte-identical
    /// in all fifteen files. Read off the committed files alone, with no help
    /// from the deriving code — if <c>Derive</c> were wrong in both directions
    /// the comparison above would still pass, and this one would not.
    /// </summary>
    [Fact]
    public void TheDrawingItselfIsByteIdenticalInEveryFile()
    {
        RefuseToCertifyInRegenerationMode();

        var master = Master;
        var reference = DrawingOnly(Lf(master));

        Assert.Contains("id=\"region-frontal\"", reference);

        foreach (var member in BrainRegionFamily.Members(master))
        {
            Assert.True(DrawingOnly(Lf(Committed(member))) == reference,
                $"{member}.svg draws something the master does not. There is one drawing in "
                + "this family and fourteen style blocks over it.");
        }
    }

    /// <summary>
    /// A family file with its banner comment, its <c>aria-label</c> and any
    /// appended style block taken off — i.e. the drawing and nothing else.
    /// </summary>
    private static string DrawingOnly(string svg)
    {
        var start = svg.IndexOf("<!--", StringComparison.Ordinal);
        var end = svg.IndexOf("-->", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "a family file with no banner comment");

        var body = svg[..start] + svg[(end + 3)..];
        body = Regex.Replace(body, @"aria-label=""[^""]*""", "aria-label=\"\"");
        body = Regex.Replace(
            body, @"\n+  <!-- DERIVED\. See the banner at the top of this file\. -->\n  <style>.*?</style>\n",
            "\n", RegexOptions.Singleline);
        return body;
    }

    /// <summary>
    /// The family is closed in both directions, and it is read off the drawing
    /// rather than listed anywhere: a region drawn with no variant file fails,
    /// and a <c>dia-region-*</c> file shading nothing the master draws fails.
    /// (WI-574's lesson, which this repo has now paid for several times: a
    /// check with one direction is half a check.)
    /// </summary>
    [Fact]
    public void TheFamilyIsTheMasterPlusOneFilePerRegionPlusTheReportLabelling()
    {
        var master = Master;
        var expected = BrainRegionFamily.Members(master).Order(StringComparer.Ordinal).ToList();

        var onDisk = Directory
            .EnumerateFiles(BrainRegionFamily.FiguresDirectory, "*.svg")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(id => BrainRegionFamily.IsMember(id!))
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(expected, onDisk);

        // And the thirteen regions are the ones the location topic is written
        // about — named here so that deleting a region from the drawing is a
        // decision somebody makes on purpose rather than a file that quietly
        // stops being referenced.
        Assert.Equal(
            [
                "brainstem", "cerebellum", "frontal", "hearing-nerve", "meninges", "occipital",
                "parietal", "pineal", "sellar", "skull-base", "spinal-cord", "temporal",
                "ventricles",
            ],
            BrainRegionFamily.RegionIds(master));
    }

    /// <summary>
    /// A positive control for the shading itself (WI-569: a guard that has
    /// never been seen to fail has not been shown to work). Every variant has
    /// to shade ITS region and no other — a style block naming the wrong id
    /// renders a map with nothing highlighted, which looks like the master and
    /// reads, with the caption under it, as a lie about where the tumor is.
    /// </summary>
    [Fact]
    public void EveryVariantShadesItsOwnRegionAndOnlyThatOne()
    {
        var master = Master;
        var regions = BrainRegionFamily.RegionIds(master);

        foreach (var region in regions)
        {
            var svg = Lf(Committed("dia-region-" + region));

            Assert.Contains($"#region-{region} {{ --shade: #0d6a86;", svg);
            Assert.Contains($"#labels .label[data-region=\"{region}\"] {{ display: inline; }}", svg);

            foreach (var other in regions.Where(r => r != region))
            {
                Assert.DoesNotContain($"#region-{other} {{ --shade", svg);
                Assert.DoesNotContain($"[data-region=\"{other}\"] {{ display: inline; }}", svg);
            }
        }

        // The master shades nothing, which is the other half of the control:
        // "the shading rule is present" means nothing unless its absence is
        // also what the unshaded file looks like.
        Assert.DoesNotContain("--shade: #0d6a86", Lf(master));
    }

    /// <summary>
    /// Every variant shows exactly one label, the master and the report-words
    /// file show nine, and no file shows a label for a region it does not
    /// shade. A map that labels nine places on a page about one of them is
    /// eight claims that page's text never makes (see
    /// <c>FigureWordsTests</c>, which holds every label against the pages).
    /// </summary>
    [Fact]
    public void AVariantKeepsOneLabelAndTheMasterKeepsNine()
    {
        var master = Master;

        Assert.Equal(9, BrainRegionFamily.VisibleLabels(master, BrainRegionFamily.MasterId).Count);
        Assert.Equal(9, BrainRegionFamily.VisibleLabels(master, BrainRegionFamily.NamesId).Count);

        foreach (var region in BrainRegionFamily.RegionIds(master))
        {
            Assert.Single(BrainRegionFamily.VisibleLabels(master, "dia-region-" + region));
        }

        // The nine are the plain words; the nine on the report-labelled file
        // are a different nine words for the same nine places.
        Assert.Equal(
            ["Behind the nose", "Deep in the middle", "Low at the back", "The back",
             "The floor of the skull", "The front", "The side, near the ear",
             "The stalk", "The upper back part"],
            BrainRegionFamily.VisibleLabels(master, BrainRegionFamily.MasterId)
                .Order(StringComparer.Ordinal));

        Assert.Equal(
            ["brain stem", "cerebellum", "frontal lobe", "occipital lobe", "parietal lobe",
             "pituitary", "skull base", "temporal lobe", "ventricles"],
            BrainRegionFamily.VisibleLabels(master, BrainRegionFamily.NamesId)
                .Order(StringComparer.Ordinal));
    }

    /// <summary>
    /// EVERY FILE IN THE FAMILY IS WELL-FORMED XML, which is not a tidiness
    /// check: an SVG loaded through <c>&lt;img&gt;</c> is parsed as XML, and
    /// one malformed character renders the broken-image icon on a medical
    /// page with the caption and the credit still sitting under it.
    ///
    /// <para><b>The recorded case, found in a browser and not by reading.</b>
    /// A comment in the master explained the shading by naming the custom
    /// property, <c>[two hyphens]shade</c> — and two hyphens in a row are
    /// illegal inside an XML comment. The drawing was fine; the file would not
    /// parse, and all fifteen went with it, because fourteen are copies of it.
    /// A derivation multiplies a mistake in the master by fifteen, so the
    /// master gets the cheapest possible check that it is a file at all.</para>
    /// </summary>
    [Fact]
    public void EveryFileParsesAsXmlAtAll()
    {
        foreach (var member in BrainRegionFamily.Members(Master))
        {
            var svg = Committed(member);
            var error = Record.Exception(() => System.Xml.Linq.XDocument.Parse(svg));

            Assert.True(error is null,
                $"{member}.svg is not well-formed XML, so a browser renders a broken-image "
                + $"icon where the picture goes: {error?.Message}");

            foreach (Match comment in Regex.Matches(svg, "<!--.*?-->", RegexOptions.Singleline))
            {
                Assert.DoesNotContain("--", comment.Value[4..^3]);
            }
        }

        // THE POSITIVE CONTROL for the one failure this item met in a browser
        // rather than by reading: put the two hyphens back and the parse that
        // passes above has to throw.
        var committed = Committed(BrainRegionFamily.MasterId);
        var withTwoHyphens = new Regex("<!--")
            .Replace(committed, "<!-- the -- property", 1);

        Assert.ThrowsAny<Exception>(
            () => System.Xml.Linq.XDocument.Parse(withTwoHyphens));
    }

    /// <summary>
    /// The licence claim, asserted on the drawing rather than only on the
    /// front matter that carries it to a reader. The NCI/AHFS/MedlinePlus ban
    /// (PLAN.md §5) is the thing most likely to be forgotten on THIS file,
    /// because an existing brain diagram is one search away — so the master
    /// says in writing what it is and where it came from, and every derived
    /// file carries that sentence because it carries the banner.
    /// </summary>
    [Fact]
    public void EveryFileSaysItIsOursAndNotTheirs()
    {
        var master = Master;

        foreach (var member in BrainRegionFamily.Members(master))
        {
            var svg = Committed(member);
            Assert.Contains("brainharbor", svg.ToLowerInvariant());
            Assert.Contains("PLAN.md §5", svg);
            Assert.Matches(@"NEVER an NCI embedded image", svg);
            Assert.Contains("role=\"img\"", svg);
            Assert.Matches(@"aria-label=""[^""]+""", svg);
        }
    }
}
