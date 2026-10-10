using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-561: what a figure does on paper and on a phone, measured in a real
/// browser rather than read off the stylesheet.
///
/// <para><b>Why this file exists at all.</b> WI-560 found that
/// <c>print.css</c> had been deleting every glossary term from every printed
/// page since WI-101 — a blanket <c>button { display: none }</c> — and it was
/// invisible everywhere except on paper. A figure has the same two failure
/// modes: the picture vanishes, or it is so tall that the sheet is mostly
/// blank and the caption lands on the next one. Neither is visible in a unit
/// test, and neither is provable by grepping the CSS, which is why the
/// acceptance criterion asked for a printed PDF.</para>
///
/// <para>The surface is <c>/dev/styleguide</c>, which renders the figure
/// sample through the real pipeline — the only page carrying an image until
/// WI-562 sources the real ones.</para>
/// </summary>
[Trait("Category", "E2E")]
[Collection(DatabaseCollection.Name)]
public sealed class FigurePrintTests
    : IClassFixture<KestrelWebApplicationFactory>, IAsyncLifetime
{
    private readonly KestrelWebApplicationFactory _factory;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public FigurePrintTests(KestrelWebApplicationFactory factory) => _factory = factory;

    /// <summary>
    /// The height cap print.css puts on a figure's picture, in CSS pixels
    /// (4in at the 96dpi CSS inch). The point of the cap is that an
    /// unbreakable element taller than a sheet is how a print gets a blank
    /// page — so this is measured, not assumed.
    /// </summary>
    private const double PrintHeightCapPx = 4 * 96;

    /// <summary>The style guide sample's shape, 1200x675 (see its front matter).</summary>
    private const double SampleRatio = 1200.0 / 675;

    /// <summary>
    /// <c>ServerAddress</c> ends in a slash, so this is a join and not a
    /// concatenation — glued together directly it produced
    /// "http://127.0.0.1:56413dev/styleguide" and Chromium refused it.
    /// </summary>
    private string StyleGuideUrl =>
        new Uri(new Uri(_factory.ServerAddress), "dev/styleguide").ToString();

    public async Task InitializeAsync()
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"Playwright browser install failed (exit {exitCode}).");
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _factory.EnsureServer();
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
    }

    [Fact]
    public async Task TheFigurePrintsWithItsPictureAndKeepsTheCaptionWithIt()
    {
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(StyleGuideUrl);
        await page.EmulateMediaAsync(new() { Media = Media.Print });

        var image = page.Locator("figure.figure img").First;
        var figure = page.Locator("figure.figure").First;

        // 1. IT PRINTS AT ALL. The WI-560 failure, one component over: a
        //    `display: none` anywhere in print.css that happens to match.
        Assert.True(await image.IsVisibleAsync(),
            "the figure's picture is hidden when the page is printed");

        // 2. IT STAYS INSIDE THE COLUMN. The style guide's own landscape sample
        //    is ~665px wide on Letter (816px, less print.css's 2cm margins)
        //    and 374px tall — UNDER the 4in cap, which is why the cap is
        //    measured on a tall picture below instead of here. Asserting the
        //    cap on this one would be a guard that cannot fail (/review).
        var box = await image.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box!.Height > 0, "the picture printed with no height");
        Assert.InRange(box.Width / box.Height, SampleRatio - 0.05, SampleRatio + 0.05);

        // 3. THE WORDS STAY WITH THE PICTURE. A page break between a diagram
        //    and the caption explaining it leaves both halves useless.
        var breakInside = await figure.EvaluateAsync<string>(
            "element => getComputedStyle(element).breakInside");
        Assert.Equal("avoid", breakInside);

        // 4. A TALL PICTURE IS CAPPED, AND SCALED RATHER THAN SQUASHED. This
        //    is where both print findings live. `max-block-size: 70vh` from
        //    site.css means nothing on a sheet, where the height IS a sheet,
        //    so the inch cap has to win the cascade — and once it bites, an
        //    HTML width attribute acting as a SPECIFIED width is what turned a
        //    1200x675 drawing into an 828x384 box the first time. The sample
        //    is an SVG and letterboxed itself, so the PDF looked right: the
        //    ratio is asserted from the layout, not read off the picture.
        await MakeTheFigureTall(page);
        var tall = await page.Locator("figure.figure img").First.BoundingBoxAsync();
        Assert.NotNull(tall);
        Assert.True(tall!.Height <= PrintHeightCapPx + 1,
            $"the printed picture is {tall.Height:0}px tall, over the {PrintHeightCapPx:0}px cap — "
            + "a figure taller than a sheet prints a mostly-blank page");
        Assert.InRange(tall.Width / tall.Height, 1 / SampleRatio - 0.05, 1 / SampleRatio + 0.05);

        // 5. AND IT REALLY PRINTS. The computed styles above are a measurement
        //    of the cascade; this is the output the acceptance asked for.
        var pdf = await page.PdfAsync(new() { Format = "Letter", PrintBackground = false });
        Assert.True(pdf.Length > 5_000, $"the printed PDF is {pdf.Length} bytes");
    }

    /// <summary>
    /// Swaps the figure's picture for a genuinely portrait one, 675x1200.
    ///
    /// <para><b>Changing the width/height ATTRIBUTES is not enough</b>, which
    /// the break harness caught: once an image has loaded its intrinsic size
    /// comes from the file, and the attributes are only the placeholder ratio
    /// used before it arrives. So the src is swapped too.</para>
    /// </summary>
    private static Task MakeTheFigureTall(IPage page) => page.EvaluateAsync(
        """
        () => {
          const tall =
            "data:image/svg+xml," +
            encodeURIComponent(
              '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 675 1200" ' +
              'width="675" height="1200"><rect width="675" height="1200" ' +
              'fill="#eef3f7"/></svg>');
          const image = document.querySelector('figure.figure img');
          image.setAttribute('src', tall);
          image.setAttribute('width', '675');
          image.setAttribute('height', '1200');
          return image.decode();
        }
        """);

    [Fact]
    public async Task AFigureOnItsOwnPrintsOnOneSheet()
    {
        // THE BLANK-PAGE CHECK, asked of the PDF itself rather than of the
        // CSS. `break-inside: avoid` on something taller than a sheet is how a
        // print gets a blank page — the rule and the height cap are a pair,
        // and only the sheet count can say whether the pair works.
        //
        // THE PICTURE IS MADE TALL FIRST, and that is the whole test. With the
        // style guide's own 1200x675 sample this assertion COULD NOT FAIL: a
        // landscape picture is capped by the column width long before it is
        // tall enough to spill, so the break harness removed the height cap
        // entirely and the PDF was still one sheet — a guard that has never
        // been seen to fail has not been shown to work.
        //
        // AND SWAPPING THE width/height ATTRIBUTES WAS NOT ENOUGH, which the
        // harness also caught: once an image has LOADED, its intrinsic size
        // comes from the file and the attributes are only the placeholder
        // aspect ratio used before it arrives. So the src is swapped for a
        // genuinely portrait picture. Uncapped it lays out 1200px tall against
        // a 1056px sheet; capped at 4in it is 384px and fits.
        //
        // The markup is otherwise lifted from the live page, so it is the
        // renderer's own output and not a copy of it that can drift.
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(StyleGuideUrl);
        var markup = await page.Locator("figure.figure").EvaluateAsync<string>(
            """
            element => {
              const tall =
                "data:image/svg+xml," +
                encodeURIComponent(
                  '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 675 1200" ' +
                  'width="675" height="1200"><rect width="675" height="1200" ' +
                  'fill="#eef3f7"/></svg>');
              const copy = element.cloneNode(true);
              const image = copy.querySelector('img');
              image.setAttribute('src', tall);
              image.setAttribute('width', '675');
              image.setAttribute('height', '1200');
              return copy.outerHTML;
            }
            """);

        var site = new Uri(new Uri(_factory.ServerAddress), "css/site.css");
        var print = new Uri(new Uri(_factory.ServerAddress), "css/print.css");
        await page.SetContentAsync(
            "<!doctype html><html><head>"
            + $"<link rel=\"stylesheet\" href=\"{site}\">"
            + $"<link rel=\"stylesheet\" href=\"{print}\" media=\"print\">"
            + $"</head><body><article class=\"container--read\">{markup}</article></body></html>");

        var pdf = await page.PdfAsync(new() { Format = "Letter" });

        Assert.Equal(1, SheetCount(pdf));
    }

    /// <summary>
    /// How many sheets a Chromium-printed PDF has, read off the page tree's
    /// <c>/Count</c>. Crude on purpose: pulling in a PDF library to count
    /// pages would be a dependency for one integer.
    /// </summary>
    private static int SheetCount(byte[] pdf)
    {
        var text = System.Text.Encoding.Latin1.GetString(pdf);
        var match = System.Text.RegularExpressions.Regex.Match(text, @"/Count (\d+)");

        Assert.True(match.Success, "the PDF has no page tree count in it");
        return int.Parse(match.Groups[1].Value);
    }

    [Fact]
    public async Task TheFigureStaysInsideTheReadingColumnOnAPhone()
    {
        // The site is verified at 390px (WI-440). A 1200px-wide diagram that
        // overflows there takes the whole page sideways with it.
        var context = await _browser!.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            IsMobile = true,
            HasTouch = true,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(StyleGuideUrl);

        var image = page.Locator("figure.figure img").First;
        var box = await image.BoundingBoxAsync();
        Assert.NotNull(box);

        Assert.True(box!.Width <= 390, $"the picture is {box.Width:0}px wide on a 390px screen");

        // The shape has to survive the cap, or a diagram's labels stretch.
        var ratio = box.Width / box.Height;
        Assert.InRange(ratio, SampleRatio - 0.1, SampleRatio + 0.1);

        // AND NOTHING SCROLLS SIDEWAYS BECAUSE OF IT — measured against a
        // CONTROL, which is what makes the number mean anything. The dev
        // styleguide already overflows 390px without any figure on it (it
        // lays out wide badge and card samples side by side), so asserting
        // "no horizontal scroll" would have gone red for somebody else's
        // layout and told us nothing about this component. The question a
        // figure has to answer is whether it made the page any wider.
        var withFigure = await page.EvaluateAsync<int>(
            "() => document.documentElement.scrollWidth");
        await page.EvaluateAsync("() => document.querySelector('figure.figure').remove()");
        var withoutFigure = await page.EvaluateAsync<int>(
            "() => document.documentElement.scrollWidth");

        Assert.True(withFigure <= withoutFigure,
            $"the figure widens the page at 390px: {withoutFigure}px without it, {withFigure}px with it");

        await context.DisposeAsync();
    }

    [Fact]
    public async Task TheFigureAddsNoSeriousOrCriticalAxeViolations()
    {
        // A <figure>/<figcaption> with a required alt attribute is the whole
        // accessibility claim of this item, and axe is what holds it.
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(StyleGuideUrl);

        Assert.Equal(1, await page.Locator("figure.figure").CountAsync());

        // Scoped to the figure, not the whole styleguide: this test is about
        // the component, and a scan of the page would go red for anything else
        // on a dev-only surface that happens to drift.
        var results = await page.Locator("figure.figure").RunAxe();
        var serious = results.Violations
            .Where(v => v.Impact is "serious" or "critical")
            .Select(v => $"{v.Id}: {v.Help}")
            .ToList();

        Assert.True(serious.Count == 0, string.Join("\n", serious));
    }

    // ---------- the first REAL figure, on a real page (WI-572) ----------

    /// <summary>
    /// The style guide's sample is a 1200x675 SVG that letterboxes itself
    /// inside any box it is given, and §12.33's second finding is that a
    /// sample which cannot exhibit the defect is a sample that certifies it:
    /// the first version of the print rules printed that sample into an
    /// 828x384 box for a 1200x675 picture and the PDF looked right.
    ///
    /// <para>So the first real figure the corpus ships gets asked the same
    /// questions on a REAL PAGE, printed to PDF — a different shape (1200x800)
    /// through the whole content pipeline rather than a C# string, and eleven
    /// of them on the page that carries the most.</para>
    /// </summary>
    [Fact]
    public async Task TheRealBrainMapPrintsOnARealPageAtItsOwnShape()
    {
        // The shape is READ OFF THE FILE, not typed here. A constant would be
        // a second place the drawing's size is written down, and this test is
        // about the picture keeping its own shape: the first version held the
        // map to 1200x800 after the viewBox became 1100x760, which is a test
        // asserting the old drawing.
        var mapRatio = await MapRatioFromTheFile();

        var page = await _browser!.NewPageAsync();
        await page.GotoAsync(new Uri(new Uri(_factory.ServerAddress), "tumors/glioma").ToString());
        await page.EmulateMediaAsync(new() { Media = Media.Print });

        var image = page.Locator("figure.figure img").First;
        await image.ScrollIntoViewIfNeededAsync();

        Assert.Equal(1, await page.Locator("figure.figure").CountAsync());
        Assert.True(await image.IsVisibleAsync(), "the brain map is hidden when the page prints");
        Assert.Equal("/img/figures/dia-brain-regions.svg", await image.GetAttributeAsync("src"));
        Assert.False(string.IsNullOrWhiteSpace(await image.GetAttributeAsync("alt")));

        // THE SHAPE, MEASURED FROM THE PRINT LAYOUT. A squashed diagram is a
        // diagram with squashed words in it: these labels are the picture.
        var box = await image.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.Width / box.Height, mapRatio - 0.05, mapRatio + 0.05);
        Assert.True(box.Height <= PrintHeightCapPx + 1,
            $"the printed map is {box.Height:0}px tall, over the {PrintHeightCapPx:0}px cap");

        var pdf = await page.PdfAsync(new() { Format = "Letter", PrintBackground = false });
        Assert.True(pdf.Length > 5_000, $"the printed PDF is {pdf.Length} bytes");
    }

    /// <summary>
    /// The master map's own numbers, read from the file the page serves: its
    /// aspect ratio, its label size, and the width of its coordinate space.
    /// Read rather than typed, because a constant here is a second place the
    /// drawing's size is written down and the first version of this file held
    /// the map to 1200x800 after the viewBox became 1100x760.
    /// </summary>
    private async Task<(double Ratio, double FontSize, double ViewBoxWidth)> MapFacts()
    {
        using var http = new HttpClient();
        var svg = await http.GetStringAsync(
            new Uri(new Uri(_factory.ServerAddress), "img/figures/dia-brain-regions.svg"));

        var box = System.Text.RegularExpressions.Regex.Match(svg, @"viewBox=""0 0 (\d+) (\d+)""");
        var font = System.Text.RegularExpressions.Regex.Match(svg, @"font-size: (\d+)px");
        Assert.True(box.Success && font.Success, "the map has no viewBox or no font size");

        var width = double.Parse(box.Groups[1].Value);
        return (width / double.Parse(box.Groups[2].Value),
            double.Parse(font.Groups[1].Value), width);
    }

    private async Task<double> MapRatioFromTheFile() => (await MapFacts()).Ratio;

    /// <summary>
    /// ELEVEN FIGURES ON ONE PAGE, which no surface in this repo has had
    /// before: /where-your-tumor-is carries the master map, the twin labelled
    /// in a report's words, and one shaded variant per region. The lazy rule
    /// (first eager, rest lazy) only matters at this density, and so does the
    /// phone column — eleven pictures that each widen the page by a pixel is
    /// a page that scrolls sideways.
    /// </summary>
    [Fact]
    public async Task TheLocationPageCarriesElevenFiguresAndStaysInsideThePhoneColumn()
    {
        var context = await _browser!.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            IsMobile = true,
            HasTouch = true,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(
            new Uri(new Uri(_factory.ServerAddress), "where-your-tumor-is").ToString());

        var figures = page.Locator("figure.figure img");
        Assert.Equal(11, await figures.CountAsync());

        // The first is eager and every one after it is lazy (§12.33): the
        // first may be the picture already on screen, and ten eager images
        // on a phone is ten requests before the text arrives.
        Assert.Null(await figures.Nth(0).GetAttributeAsync("loading"));
        for (var i = 1; i < 11; i++)
        {
            Assert.Equal("lazy", await figures.Nth(i).GetAttributeAsync("loading"));
        }

        var widest = 0.0;
        for (var i = 0; i < 11; i++)
        {
            await figures.Nth(i).ScrollIntoViewIfNeededAsync();
            var box = await figures.Nth(i).BoundingBoxAsync();
            Assert.NotNull(box);
            widest = Math.Max(widest, box!.Width);
        }

        Assert.True(widest <= 390, $"a figure is {widest:0}px wide on a 390px screen");

        // THE WORDS INSIDE THE PICTURE, measured rather than assumed. A
        // diagram's labels are its content, and their size on a phone is a
        // RATIO: the font size in the SVG divided by the viewBox width, times
        // the rendered width. The first version of this map set 34px in a
        // 1200-unit box and rendered its labels at about 10px — under the
        // 16px floor site.css calls "the smallest allowed anywhere" (WI-440),
        // on the one surface where the type is a picture and no browser
        // setting enlarges it.
        //
        // THE BAR HERE IS 13px RATHER THAN 16, AND THE REASON IS WRITTEN
        // DOWN. Nine labels and a readable drawing do not both fit in a
        // 390px column at 16px — the labels would take the width the brain
        // needs. The acceptance for this item says the page has to work with
        // NO image at all: the nine places are nine headings on
        // /where-your-tumor-is, the alt text and the caption carry the
        // figure, and the drawing is an enhancement over them. So the number
        // is a floor that catches a regression (a wider viewBox, a smaller
        // font) rather than a claim that this is as legible as body text.
        var (_, fontSize, viewBoxWidth) = await MapFacts();
        var firstBox = await figures.Nth(0).BoundingBoxAsync();
        var labelPx = fontSize * firstBox!.Width / viewBoxWidth;

        Assert.True(labelPx >= 13,
            $"the map's labels render at {labelPx:0.0}px in a {firstBox.Width:0}px column "
            + $"({fontSize:0}px in a {viewBoxWidth:0}-unit viewBox). Widening the viewBox or "
            + "shrinking the font makes the only words inside the picture smaller.");
        Assert.True(await page.EvaluateAsync<int>(
            "() => document.documentElement.scrollWidth") <= 390,
            "the location page scrolls sideways on a phone");

        await context.DisposeAsync();
    }
}
