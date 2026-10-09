using System.Text.RegularExpressions;
using BrainHarbor.ContentCheck;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-561: images on curated pages — the mechanism.
///
/// <para>The authoring form is one line of ordinary Markdown, alone in its
/// paragraph, with the credit in the page's <c>images:</c> front matter:</para>
/// <code>![what is in it](/img/figures/x.svg "what it means")</code>
///
/// <para><b>What these tests are protecting, in order of how badly it fails
/// silently.</b> An uncredited image renders perfectly and is somebody else's
/// legal problem (PLAN.md §5). An image with no alt text renders perfectly and
/// is invisible to a screen reader (WCAG AA is a hard requirement). A caption
/// nothing grades is reader-facing prose outside the 6.0 gate, which is
/// WI-575's hole re-dug one surface over. None of the three has a visible
/// symptom, so each one fails the page instead.</para>
/// </summary>
public class FigureTests
{
    private const string Src = "/img/dev/figure-sample.svg";
    private const string Alt = "Three boxes in a row, joined by arrows.";
    private const string Caption = "What the picture means for you.";
    private const string LicenseUrl = "https://creativecommons.org/publicdomain/zero/1.0/";

    /// <summary>
    /// Every field a declaration must carry. The omission theory below drops
    /// them one at a time from THIS list, so a new required field is covered
    /// by the test that proves it is required without anyone adding a case.
    /// </summary>
    private static readonly string[] RequiredFields =
    [
        "credit: \"A Photographer\"",
        "license: \"CC0 1.0 (public domain)\"",
        $"license_url: {LicenseUrl}",
        "width: 1200",
        "height: 675",
    ];

    private static string Declaration(IEnumerable<string>? fields = null, string src = Src) =>
        $"images:\n  - src: {src}\n"
        + string.Concat((fields ?? RequiredFields).Select(field => $"    {field}\n"));

    private static string Compose(string body, string? declaration = null) =>
        "---\ntitle: Figure test\n" + (declaration ?? Declaration()) + "---\n\n" + body + "\n";

    private static string ImageLine(string alt = Alt, string src = Src, string? caption = Caption) =>
        caption is null ? $"![{alt}]({src})" : $"![{alt}]({src} \"{caption}\")";

    private static string Render(string raw) => ContentStore.Parse(raw, "figures/test").Html;

    private static FormatException Rejects(string raw) =>
        Assert.Throws<FormatException>(() => ContentStore.Parse(raw, "figures/test"));

    // ---------- what a figure renders as ----------

    [Fact]
    public void ALoneImageRendersAsARealFigureWithItsCaption()
    {
        var html = Render(Compose(ImageLine()));

        Assert.Contains("<figure class=\"figure\">", html);
        Assert.Contains($"src=\"{Src}\"", html);
        Assert.Contains($"alt=\"{Alt}\"", html);
        Assert.Contains("<figcaption class=\"figure__caption\">", html);
        Assert.Contains($"<span class=\"figure__text\">{Caption}</span>", html);

        // The whole point of replacing the paragraph rather than decorating
        // it: a <figure> inside a <p> is invalid HTML, and a bare <img> in a
        // paragraph is an image with nowhere to put a caption or a credit.
        Assert.DoesNotContain("<p><img", html);
        Assert.DoesNotContain("<p><figure", html);
    }

    [Fact]
    public void TheCreditRendersInsideTheSameFigcaptionAsTheCaption()
    {
        // Not a separate line under the figure, and not a page-level credits
        // list: the credit has to be impossible to separate from the thing it
        // credits, which is what wwwroot/img/cards/IMAGE-CREDITS.md is not.
        var html = Render(Compose(ImageLine()));

        // POSITIONS, NOT A SLICE. The slice version of this test was fooled by
        // the break harness: swapping the two closing tags leaves the credit
        // inside the text between `<figcaption` and `</figcaption>` while the
        // markup is malformed, and the test passed. Asserting the order of all
        // four landmarks is the property — the credit is inside the caption,
        // and the caption closes inside the figure.
        var figure = html.IndexOf("<figure", StringComparison.Ordinal);
        var captionOpen = html.IndexOf("<figcaption", StringComparison.Ordinal);
        var credit = html.IndexOf("<span class=\"figure__credit\">", StringComparison.Ordinal);
        var captionClose = html.IndexOf("</figcaption>", StringComparison.Ordinal);
        var figureClose = html.IndexOf("</figure>", StringComparison.Ordinal);

        Assert.True(
            figure < captionOpen && captionOpen < credit
            && credit < captionClose && captionClose < figureClose,
            $"the credit is not nested inside the figcaption: figure {figure}, "
            + $"figcaption {captionOpen}, credit {credit}, /figcaption {captionClose}, "
            + $"/figure {figureClose}");

        var caption = html[captionOpen..captionClose];
        Assert.Contains("Image: A Photographer.", caption);
        Assert.Contains($"<a href=\"{LicenseUrl}\" rel=\"license\">CC0 1.0 (public domain)</a>", caption);
        Assert.DoesNotContain("</figure>", caption);
    }

    [Fact]
    public void TheSourceLinkIsRenderedWhenDeclaredAndAbsentWhenItIsNot()
    {
        var withSource = Render(Compose(ImageLine(), Declaration(
            [.. RequiredFields, "source_url: https://commons.wikimedia.org/wiki/File:Example"])));

        Assert.Contains(
            "<a href=\"https://commons.wikimedia.org/wiki/File:Example\">Where this picture came from</a>",
            withSource);

        // Optional on purpose: a diagram drawn for this site has no "where it
        // came from", and an empty link would be a dead end on every figure
        // we draw ourselves.
        Assert.DoesNotContain("Where this picture came from", Render(Compose(ImageLine())));
    }

    [Fact]
    public void TheDeclaredPixelSizeIsWrittenOntoTheImage()
    {
        // Not decoration: the figures below the first are lazy-loaded, and a
        // lazy image with no declared size shoves the text under it down the
        // page when it arrives — moving the line the reader is on.
        var html = Render(Compose(ImageLine()));

        Assert.Contains("width=\"1200\" height=\"675\"", html);
    }

    [Fact]
    public void TheFirstFigureIsEagerAndEveryFigureBelowItIsLazy()
    {
        var second = "/img/figures/second.svg";
        var declaration = Declaration() + Declaration(src: second)["images:\n".Length..];

        var html = Render(Compose(
            ImageLine() + "\n\nSome words between them.\n\n" + ImageLine(
                alt: "A different picture.", src: second, caption: "A different caption."),
            declaration));

        var images = Regex.Matches(html, "<img [^>]*>").Select(m => m.Value).ToList();
        Assert.Equal(2, images.Count);

        // The first figure may be the one already on screen, and deferring
        // THAT one delays the only picture some readers ever see. Everything
        // after it is below the fold on a 390px screen (WI-440).
        Assert.DoesNotContain("loading=", images[0]);
        Assert.Contains("loading=\"lazy\"", images[1]);
        Assert.All(images, image => Assert.Contains("decoding=\"async\"", image));
    }

    [Fact]
    public void ADeclaredDecorativeImageGetsAnEmptyAltAndStillCarriesItsCredit()
    {
        var html = Render(Compose(
            $"![]({Src})",
            Declaration([.. RequiredFields, "decorative: true"])));

        Assert.Contains("alt=\"\"", html);
        Assert.DoesNotContain("figure__text", html);

        // The licence does not care whether a picture means anything.
        Assert.Contains("Image: A Photographer.", html);
    }

    [Fact]
    public void AltTextAndCaptionAreEscapedIntoTheMarkup()
    {
        // The caption is written in SINGLE quotes here, which is Markdown's
        // own answer to a title containing a double quote. Written in double
        // quotes it would not parse as an image at all — see
        // ALineThatLooksLikeAnImageButDoesNotParseFailsThePage.
        var html = Render(Compose(
            $"![A chart of 1 & 2 <three>]({Src} 'Tumors & \"grades\" <explained>')"));

        Assert.Contains("alt=\"A chart of 1 &amp; 2 &lt;three&gt;\"", html);
        Assert.Contains("Tumors &amp; &quot;grades&quot; &lt;explained&gt;", html);
        Assert.DoesNotContain("<three>", html);
    }

    [Fact]
    public void ALineThatLooksLikeAnImageButDoesNotParseFailsThePage()
    {
        // A double quote inside a double-quoted caption ends the title early,
        // the line stops being an image, and the page renders the source text
        // to a patient: ![a scan](/img/... "a "grade 2" tumor"). There is
        // nothing in the parse tree to find, so the raw source is audited —
        // the same hole, and the same remedy, as ReaderGate's fence audit.
        var exception = Rejects(Compose(
            $"![A scan with the edges marked.]({Src} \"A \"grade 2\" tumor.\")"));

        Assert.Contains("as literal text", exception.Message);
        Assert.Contains("single quotes", exception.Message);
    }

    [Fact]
    public void AGlossaryTermInAltTextNeverBecomesMarkup()
    {
        // The alt text is an HTML ATTRIBUTE. A tooltip's <button> inside it
        // would be read out as angle brackets, or would break the attribute
        // outright. The marker skips anything under a link and an image IS a
        // link — this holds that true against the real shipped glossary.
        var rendered = CuratedPage.Rendered(
            "figures/test",
            Compose(ImageLine(alt: "A glioma seen on a scan, with the edges marked.")));

        var alt = Regex.Match(rendered.Html, "alt=\"([^\"]*)\"").Groups[1].Value;
        Assert.Equal("A glioma seen on a scan, with the edges marked.", alt);
        Assert.DoesNotContain("<button", alt);
    }

    // ---------- accessibility: the failures that render perfectly ----------

    [Fact]
    public void AnImageWithNoAltTextFailsThePage()
    {
        // The acceptance criterion asks for this failure to be PROVED by
        // removing one, rather than asserted by reading the code.
        var exception = Rejects(Compose($"![]({Src} \"{Caption}\")"));

        Assert.Contains("no alt text", exception.Message);
        Assert.Contains("decorative: true", exception.Message);
    }

    [Fact]
    public void AnEmptyAltIsOnlyAllowedWhereThePageDeclaresItDecorative()
    {
        // Same Markdown, two outcomes: the declaration is the difference
        // between a deliberate choice and a hurried author's default.
        //
        // THE CAPTION IS HERE BECAUSE THE BREAK HARNESS CAUGHT THIS TEST
        // PASSING FOR THE WRONG REASON. Written as `![](src)` with no caption,
        // a mutation that deleted the alt check entirely still threw — on the
        // MISSING CAPTION, one check later — and an unexamined
        // `Assert.Throws` cannot tell two failures apart. With the caption
        // present, the alt rule is the only thing left that can reject it, and
        // the message is asserted so it has to be the rule that did.
        var exception = Rejects(Compose($"![]({Src} \"{Caption}\")"));
        Assert.Contains("no alt text", exception.Message);

        var declared = Render(Compose(
            $"![]({Src})", Declaration([.. RequiredFields, "decorative: true"])));
        Assert.Contains("alt=\"\"", declared);
    }

    [Fact]
    public void ADecorativeImageMayNotAlsoCarryAltTextOrACaption()
    {
        var withAlt = Rejects(Compose(
            $"![{Alt}]({Src})", Declaration([.. RequiredFields, "decorative: true"])));
        Assert.Contains("declares the image", withAlt.Message);
        Assert.Contains("decorative", withAlt.Message);

        var withCaption = Rejects(Compose(
            $"![]({Src} \"{Caption}\")", Declaration([.. RequiredFields, "decorative: true"])));
        Assert.Contains("caption", withCaption.Message);
    }

    [Fact]
    public void AnImageWithNoCaptionFailsThePage()
    {
        var exception = Rejects(Compose(ImageLine(caption: null)));

        Assert.Contains("no caption", exception.Message);
    }

    [Fact]
    public void TheAltTextAndTheCaptionMayNotBeTheSameWords()
    {
        // One is read aloud, the other is on screen. Identical, a
        // screen-reader user hears the same sentence twice and learns nothing
        // about the picture either time.
        var exception = Rejects(Compose(ImageLine(alt: Caption, caption: Caption)));

        Assert.Contains("same words", exception.Message);
    }

    [Theory]
    [InlineData("Photo of an MRI scanner in a room.")]
    [InlineData("image of a lab bench.")]
    [InlineData("Graphic of the four steps.")]
    public void AltTextMayNotStartByNamingTheMedium(string alt)
    {
        var exception = Rejects(Compose(ImageLine(alt: alt)));

        Assert.Contains("already says 'image'", exception.Message);
    }

    [Fact]
    public void NamingTheKindOfDrawingIsAllowed()
    {
        // The other side of the ban, because a ban list that forbids the
        // correct shape is worse than no ban list. A screen reader says
        // "image", not "diagram" or "chart", so the KIND of drawing is
        // information — WAI's own tutorials write "Chart: …".
        Assert.Contains(
            "<figure", Render(Compose(ImageLine(alt: "Diagram of the four steps."))));
        Assert.Contains(
            "<figure", Render(Compose(ImageLine(alt: "Chart of how long each step takes."))));
    }

    // ---------- the licence, checked mechanically ----------

    [Fact]
    public void AnImageTheFrontMatterDoesNotDeclareFailsThePage()
    {
        var exception = Rejects(
            "---\ntitle: Figure test\n---\n\n" + ImageLine() + "\n");

        Assert.Contains("no front-matter entry", exception.Message);
        Assert.Contains("credit", exception.Message);

        // An image inside a shared block lands here too, and the message has
        // to say so: a block cannot carry its own credit yet, and an author
        // who put one there deserves better than "add it to the page".
        Assert.Contains("SHARED BLOCK", exception.Message);
    }

    [Fact]
    public void EveryRequiredFieldIsRequired()
    {
        // The positive control, first and in the same test: without it, a
        // typo in `Declaration` would make all five omissions throw for the
        // wrong reason and the theory would still be green.
        Assert.Contains("<figure", Render(Compose(ImageLine())));

        foreach (var omitted in RequiredFields)
        {
            var field = omitted[..omitted.IndexOf(':', StringComparison.Ordinal)];
            var exception = Rejects(Compose(
                ImageLine(), Declaration(RequiredFields.Where(f => f != omitted))));

            Assert.Contains($"'{field}'", exception.Message);
        }
    }

    [Theory]
    [InlineData("license_url: creativecommons.org/publicdomain/zero/1.0/")]
    [InlineData("license_url: \"\"")]
    public void ALicenceNobodyCanOpenIsNotALicence(string field)
    {
        var fields = RequiredFields
            .Where(f => !f.StartsWith("license_url", StringComparison.Ordinal))
            .Append(field);

        Assert.Throws<FormatException>(
            () => ContentStore.Parse(Compose(ImageLine(), Declaration(fields)), "figures/test"));
    }

    [Theory]
    [InlineData("National Cancer Institute")]
    [InlineData("NCI")]
    [InlineData("MedlinePlus")]
    [InlineData("AHFS Patient Medication Information")]
    public void TheLicensingBanAlsoReadsTheCreditWhereThereIsNoSourceUrl(string credit)
    {
        // THE SECOND DIRECTION, and without it the ban was off for every
        // image that left `source_url` out — which it may, because a diagram
        // drawn here has nowhere to point. An NCI picture committed to the
        // repo and credited honestly rendered on a green build (/review).
        var fields = RequiredFields
            .Where(f => !f.StartsWith("credit", StringComparison.Ordinal))
            .Append($"credit: \"{credit}\"");

        var exception = Rejects(Compose(ImageLine(), Declaration(fields)));

        Assert.Contains("PLAN.md §5", exception.Message);
        Assert.Contains("TEXT is still citable", exception.Message);
    }

    [Fact]
    public void TheCreditBanDoesNotFireOnAWordThatMerelyContainsIt()
    {
        // "nci" is matched as a WORD. A substring match would reject
        // "Principal Investigator" and half the organisations on earth, which
        // is the ban-list-forbids-a-correct-shape defect again.
        Assert.Contains("<figure", Render(Compose(ImageLine(), Declaration(
            RequiredFields
                .Where(f => !f.StartsWith("credit", StringComparison.Ordinal))
                .Append("credit: \"Principal Investigator, Science Council\"")))));
    }

    [Theory]
    [InlineData("https://www.cancer.gov/images/example.jpg")]
    [InlineData("https://medlineplus.gov/ency/images/ency/fullsize/example.jpg")]
    [InlineData("https://ahfsdruginformation.com/example.jpg")]
    public void ImagesMayNotComeFromTheSourcesWhosePicturesAreLicensedStock(string sourceUrl)
    {
        // PLAN.md §5 and content-pipeline §12.2 rule 6. The ban is on the
        // IMAGE's source and not on the page's citations, deliberately:
        // cancer.gov is a cited source on most tumor hubs, and banning the
        // domain outright would be a different, false rule.
        var exception = Rejects(Compose(
            ImageLine(), Declaration([.. RequiredFields, $"source_url: {sourceUrl}"])));

        Assert.Contains("PLAN.md §5", exception.Message);
        Assert.Contains("TEXT is still citable", exception.Message);
    }

    [Theory]
    [InlineData("https://example.org/picture.png")]
    [InlineData("//example.org/picture.png")]
    [InlineData("img/figures/relative.svg")]
    // "starts with a slash and has no //" let both of these through, and the
    // second walks out of wwwroot past the test that checks the file is in
    // the repo (/review).
    [InlineData("/appsettings.json")]
    [InlineData("/img/../secret.svg")]
    public void AnImageMustBeServedFromThisSite(string src)
    {
        var exception = Rejects(Compose(ImageLine(src: src), Declaration(src: src)));

        Assert.Contains("Images are served from this site", exception.Message);
    }

    [Fact]
    public void AnEntityInTheAltTextKeepsItsCharacter()
    {
        // `&copy;` is an HtmlEntityInline, not a literal, and unhandled it was
        // dropped SILENTLY — "© 2024 Someone" became "2024 Someone" inside the
        // one attribute a blind reader has (/review).
        var html = Render(Compose(ImageLine(alt: "&copy; 2024, a scan of a brain.")));

        Assert.Contains("alt=\"© 2024, a scan of a brain.\"", html);
    }

    [Fact]
    public void ACreditForAPictureThatIsNotThereFailsThePage()
    {
        var exception = Rejects(Compose("Just words, no picture."));

        Assert.Contains("no line of the body shows", exception.Message);
        Assert.Contains(Src, exception.Message);
    }

    [Theory]
    [InlineData("images:\n", false)]
    [InlineData("images: ~\n", false)]
    [InlineData("images:\n  -\n", true)]
    public void AHalfTypedImagesKeyNeverCrashesThePage(string declaration, bool rejected)
    {
        // YamlDotNet assigns NULL over the property initializer, so a key with
        // nothing under it used to reach the figure pass as a
        // NullReferenceException (/review). That is not a worse error message,
        // it is a different FAILURE MODE: ContentCheck catches only
        // FormatException so the gate would crash instead of failing the page,
        // ContentStore.GetPage catches only IOException so a reader would get
        // a 500 on a medical page, and SearchPages would lose site search for
        // the whole site over one dangling key.
        var exception = Record.Exception(
            () => ContentStore.Parse(Compose("Just words.", declaration), "figures/test"));

        // A key with nothing under it is benign — the author typed the key and
        // stopped, and a page with no pictures is exactly what they have. An
        // empty list ITEM is not benign: that is half an entry, and half an
        // entry is a picture somebody meant to credit.
        if (rejected)
        {
            Assert.IsType<FormatException>(exception);
        }
        else
        {
            Assert.Null(exception);
        }
    }

    [Fact]
    public void AMalformedDeclarationFailsEvenOnAPageWithNoPictureYet()
    {
        // The page where this mistake is MOST likely is the one that has not
        // got its Markdown line yet — an author pastes the front-matter entry
        // first and typos the key. Validating declarations only when an image
        // exists forgave exactly that case: `scr:` for `src:` left an entry
        // with no source, and the unused-declaration check skipped it because
        // it had nothing to name.
        var exception = Rejects(Compose(
            "Just words, no picture.",
            "images:\n  - scr: /img/figures/typo.svg\n    credit: \"Someone\"\n"));

        Assert.Contains("no 'src'", exception.Message);
    }

    [Fact]
    public void OneFileMayNotBeDeclaredTwice()
    {
        var twice = Declaration() + Declaration()["images:\n".Length..];

        var exception = Rejects(Compose(ImageLine(), twice));

        Assert.Contains("twice in front matter", exception.Message);
    }

    [Fact]
    public void AnImageInsideASentenceFailsRatherThanRenderingBare()
    {
        var exception = Rejects(Compose($"Here is the picture: {ImageLine()} and more words."));

        Assert.Contains("a paragraph of its own", exception.Message);
    }

    [Theory]
    [InlineData("- ")]
    [InlineData("> ")]
    [InlineData("1. ")]
    public void AnImageNestedInAListOrAQuoteFailsRatherThanRenderingSomewhereNobodyDesigned(
        string marker)
    {
        // A bullet, a quote and a table cell all contain paragraphs, so all
        // three produced a real, credited figure in a shape nothing styled or
        // measured (/review). The table cell is the one that bites: a 1200px
        // picture in an auto-layout <td> is the classic way to send a 390px
        // screen sideways, and the phone test only ever sees the top-level
        // case.
        var exception = Rejects(Compose(marker + ImageLine()));

        Assert.Contains("inside a list, a quote or a table", exception.Message);
    }

    [Fact]
    public void AnImageInATableCellFailsToo()
    {
        var exception = Rejects(Compose(
            "| What | Picture |\n|---|---|\n| A step | " + ImageLine() + " |\n"));

        Assert.Contains("inside a list, a quote or a table", exception.Message);
    }

    [Fact]
    public void AFigureIsStillAllowedInsideAReaderChoiceGate()
    {
        // The other side of the rule, because a ban that forbids the correct
        // shape is worse than no ban. `:::outlook` is a section of the page,
        // and a picture belongs in one as readily as a paragraph does.
        var html = Render(Compose(":::outlook\n\n" + ImageLine() + "\n\n:::"));

        Assert.Contains("<figure class=\"figure\">", html);
        Assert.Contains("reader-gate__body", html);
    }

    [Theory]
    [InlineData("Here it is: ")]
    [InlineData("- ")]
    [InlineData("> ")]
    public void ABrokenImageLineFailsWhereverOnTheLineItStarts(string before)
    {
        // THE LINE-START AUDIT WAS DEFEATED BY TWO CHARACTERS (/review). It
        // counted lines whose first non-space character was `![`, so a bullet
        // or any prose in front of the image let the raw Markdown through to
        // a reader — the exact defect the audit exists to catch, and the
        // no-declaration case is the one an author hits while first writing
        // the figure.
        var exception = Assert.Throws<FormatException>(() => ContentStore.Parse(
            "---\ntitle: Figure test\n---\n\n"
            + before + $"![A scan.]({Src} \"A \"grade 2\" tumor.\")\n",
            "figures/test"));

        Assert.Contains("literal text", exception.Message);
    }

    [Fact]
    public void AnIndentedImageLineFailsEvenThoughItHasNoInlinesToAudit()
    {
        // Four spaces makes it an indented CODE block, which prints the
        // brackets to a reader AND hides them from every inline check — so
        // the tree pass cannot see it and the line count is what catches it.
        // ReaderGate records the same case for its fences.
        var exception = Rejects(Compose("    " + ImageLine()));

        Assert.Contains("INDENTATION", exception.Message);
    }

    [Fact]
    public void ThePageMayNotShowOneDeclaredImageTwice()
    {
        var exception = Rejects(Compose(
            ImageLine() + "\n\nWords between them.\n\n"
            + ImageLine(alt: "The same picture again.", caption: "A second caption.")));

        Assert.Contains("more than once", exception.Message);
    }

    // ---------- the caption is content, so it is graded ----------

    [Fact]
    public void TheCaptionAndTheAltTextBothReachTheReadabilityGrader()
    {
        // ContentCheck grades `ExtractSentences(page.Markdown)`. The alt text
        // is a child inline and was always in there; the caption is the link's
        // TITLE, a property with no inline of its own, so until this item it
        // was reader-facing prose that nothing graded at all.
        // THE WHOLE STRING, not three Contains. `Assert.Contains(". ", …)`
        // was in here as the "two sentences, not one run-on" check and could
        // not fail: ExtractSentences ends every block with a terminator and a
        // space, so ". " is in the output with the caption arm deleted
        // (/review). The exact string is the only form that pins the ORDER,
        // the separation, and the absence of a doubled full stop at once.
        Assert.Equal(
            "What the picture means for you. Three boxes in a row, joined by arrows. ",
            ContentChecker.ExtractSentences(ImageLine()));

        // And a caption that already ends in a full stop does not get a
        // second one — §12.20's rule is that each block is ONE sentence.
        Assert.Equal(
            "Ends in a full stop. The alt text. ",
            ContentChecker.ExtractSentences(
                ImageLine(alt: "The alt text.", caption: "Ends in a full stop.")));
    }

    [Fact]
    public void AnUnreadableCaptionFailsTheSixthGradeGateAndAPlainOneDoesNot()
    {
        const string body = """
            Here is what this page says.

            A scan takes pictures of your brain. The pictures help your team.
            You lie still on a table. The table moves into the scanner.
            It is loud. You can ask for earplugs.
            """;

        var plain = ContentChecker.CheckPage(
            Compose(body + "\n\n" + ImageLine()), "figures/test.md", new DateOnly(2026, 10, 9));

        Assert.DoesNotContain(plain, finding => finding.Level == FindingLevel.Fail);

        var unreadable = ContentChecker.CheckPage(
            Compose(body + "\n\n" + ImageLine(caption:
                "Visualization demonstrating the interrelationship between successive "
                + "radiological acquisitions and the corresponding histopathological "
                + "interpretation subsequently communicated to the multidisciplinary "
                + "neuro-oncological conference.")),
            "figures/test.md",
            new DateOnly(2026, 10, 9));

        var failure = Assert.Single(unreadable.Where(finding => finding.Level == FindingLevel.Fail));
        Assert.Contains("reading grade", failure.Message);
    }

    // ---------- the page that has no images (the WI-501 property) ----------

    [Fact]
    public void APageWithNoImagesIsLeftExactlyAsItWas()
    {
        // The regression property every shipped page relies on today: the
        // figure pass must be a no-op on a corpus with no pictures in it.
        const string markdown = "# A heading\n\nA paragraph.\n\n- a bullet\n";
        var document = Markdig.Markdown.Parse(markdown, ContentStore.RenderPipeline);
        var blocks = document.ToList();

        Assert.Equal(0, ContentFigures.Apply(document, [], markdown, "figures/test"));
        Assert.Equal(blocks, document.ToList());
    }

    /// <summary>
    /// Every curated page, as the site parses it. Deliberately NOT
    /// <c>CuratedPage.AllPages()</c>, which also yields the shared blocks: a
    /// block has no title and is not a page, so it cannot be rendered on its
    /// own. Blocks get their own assertion below, because a picture in a block
    /// is the one case this mechanism does not support.
    /// </summary>
    private static IEnumerable<(string Slug, string Text)> Pages() =>
        Directory.EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Select(file => (
                Slug: Path.GetRelativePath(CuratedPage.PagesDirectory, file)
                    .Replace('\\', '/')[..^3],
                Text: File.ReadAllText(file)));

    [Fact]
    public void TheShippedCorpusRendersExactlyTheFiguresItDeclares()
    {
        // Phrased as an equality rather than "no page has a figure" on
        // purpose: this stays true the day WI-562's first real image lands,
        // and still catches a picture that renders uncredited or a credit
        // whose picture never renders.
        var walked = 0;

        foreach (var (slug, text) in Pages())
        {
            var rendered = CuratedPage.Rendered(slug, text);
            var declared = rendered.FrontMatter.Images.Count;

            Assert.Equal(declared, Regex.Matches(rendered.Html, "<figure class=\"figure\">").Count);
            Assert.Equal(declared, Regex.Matches(rendered.Html, "<img ").Count);
            walked++;
        }

        // A sweep that walked nothing passes (WI-582's dead-ratchet lesson).
        Assert.True(walked > 50, $"only {walked} curated pages were walked");
    }

    [Fact]
    public void ASharedBlockCarriesNoImage()
    {
        // Out of scope in writing, and this is what makes "out of scope" mean
        // something: a block's front matter can only carry `sources`, so a
        // picture inside one would need the INCLUDING page to declare a credit
        // for something that page's author never wrote. It fails at parse time
        // either way — this says so where an author will see it.
        foreach (var (slug, text) in CuratedPage.SharedSources())
        {
            Assert.DoesNotMatch(@"(?m)^[ \t]*!\[", text.Replace("\r\n", "\n"));
            Assert.NotEqual("", slug);
        }
    }

    private static string Wwwroot => Path.Combine(
        CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "wwwroot");

    /// <summary>
    /// Every image any page declares, plus the style guide's sample — which is
    /// declared in C# rather than in a page's front matter and is the only
    /// live subject these sweeps have until WI-562 sources a real picture.
    /// Without it both loops below walk zero items and pass, which is the dead
    /// ratchet this file warns about two tests up (/review).
    /// </summary>
    private static List<(string Where, ContentImage Image)> DeclaredImages()
    {
        var declared = Pages()
            .SelectMany(page => CuratedPage.Rendered(page.Slug, page.Text).FrontMatter.Images
                .Select(image => (Where: page.Slug, Image: image)))
            .ToList();

        declared.AddRange(ContentStore
            .Parse(Web.Pages.Dev.StyleGuideModel.FigureSample, "dev/styleguide")
            .FrontMatter.Images
            .Select(image => (Where: "the style guide sample", Image: image)));

        return declared;
    }

    [Fact]
    public void EveryDeclaredImageExistsAndEveryFigureFileIsDeclared()
    {
        var figuresDirectory = Path.Combine(Wwwroot, "img", "figures");
        var declared = DeclaredImages();

        Assert.NotEmpty(declared);

        foreach (var (where, image) in declared)
        {
            Assert.True(File.Exists(FileFor(image)),
                $"{where} declares the image {image.Src}, which is not in the repo");
        }

        // The other direction (WI-574's lesson: a check with one direction is
        // half a check). A picture sitting in the figures directory that no
        // page declares is a picture whose licence nothing records.
        Assert.True(Directory.Exists(figuresDirectory),
            "wwwroot/img/figures is where curated-page images live — see its README");

        var sources = declared.Select(d => d.Image.Src).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.EnumerateFiles(figuresDirectory))
        {
            if (Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                continue; // the README that says what belongs in here
            }

            Assert.Contains("/img/figures/" + Path.GetFileName(file), sources);
        }
    }

    [Fact]
    public void TheDeclaredPixelSizeMatchesTheFile()
    {
        // `RequireSize` only rejects a zero, so a transposed or stale pair
        // passed every check while defeating the whole stated reason the
        // fields are mandatory — the browser reserves the WRONG box, and the
        // picture is distorted until it loads (/review). Nothing in the parse
        // path can open the file; this is the gate that can.
        var declared = DeclaredImages();
        Assert.NotEmpty(declared);

        foreach (var (where, image) in declared)
        {
            var actual = PixelSizeOf(FileFor(image));
            Assert.True(actual == (image.Width, image.Height),
                $"{where} declares {image.Src} as {image.Width}x{image.Height}; the file is "
                + $"{actual.Width}x{actual.Height}");
        }
    }

    private static string FileFor(ContentImage image) => Path.Combine(
        Wwwroot, image.Src.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

    /// <summary>
    /// The intrinsic size of an image file, read from its header. Three
    /// formats, and anything else fails rather than being skipped: a format
    /// this cannot measure is a format whose declared size nothing checks,
    /// and WI-562 should find that out here rather than on a phone.
    /// </summary>
    private static (int Width, int Height) PixelSizeOf(string file)
    {
        var extension = Path.GetExtension(file).ToLowerInvariant();

        if (extension == ".svg")
        {
            var svg = File.ReadAllText(file);
            var box = Regex.Match(svg, @"viewBox\s*=\s*""[\d.]+\s+[\d.]+\s+([\d.]+)\s+([\d.]+)""");
            if (box.Success)
            {
                return ((int)double.Parse(box.Groups[1].Value), (int)double.Parse(box.Groups[2].Value));
            }

            return (Attribute(svg, "width"), Attribute(svg, "height"));
        }

        var bytes = File.ReadAllBytes(file);

        if (extension == ".png")
        {
            // IHDR is always the first chunk: width and height are two
            // big-endian 32-bit values at offset 16.
            return (ReadBigEndian(bytes, 16), ReadBigEndian(bytes, 20));
        }

        if (extension is ".jpg" or ".jpeg")
        {
            // Walk the marker segments to the start-of-frame, which carries
            // the real dimensions; everything before it is metadata.
            for (var i = 2; i + 9 < bytes.Length && bytes[i] == 0xFF;)
            {
                var marker = bytes[i + 1];
                var length = (bytes[i + 2] << 8) | bytes[i + 3];
                if (marker is >= 0xC0 and <= 0xCF && marker is not (0xC4 or 0xC8 or 0xCC))
                {
                    return ((bytes[i + 7] << 8) | bytes[i + 8], (bytes[i + 5] << 8) | bytes[i + 6]);
                }
                i += 2 + length;
            }
        }

        Assert.Fail(
            $"{file} is a format this check cannot measure. Add a reader for it — a declared size "
            + "nothing verifies is the defect this test exists for.");
        return (0, 0);
    }

    private static int Attribute(string svg, string name) =>
        int.TryParse(Regex.Match(svg, name + @"\s*=\s*""(\d+)").Groups[1].Value, out var value)
            ? value
            : 0;

    private static int ReadBigEndian(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16)
        | (bytes[offset + 2] << 8) | bytes[offset + 3];

    // ---------- CRLF (this repo hands every working tree CRLF; CI sees LF) ----------

    [Fact]
    public void AFigureRendersIdenticallyFromACrlfSource()
    {
        var source = Compose(ImageLine());

        Assert.Equal(
            Render(source.Replace("\r\n", "\n")),
            Render(source.Replace("\r\n", "\n").Replace("\n", "\r\n")));
    }

    // ---------- the styleguide sample, and the CSS the reader meets ----------

    [Fact]
    public void TheStyleGuideSampleIsARealFigure()
    {
        // The dev styleguide is the only surface a figure has until WI-562
        // sources a real image, so its sample has to parse through the real
        // pipeline — and the same string the page renders is the one asserted
        // here, not a copy of it.
        var html = ContentStore
            .Parse(Web.Pages.Dev.StyleGuideModel.FigureSample, "dev/styleguide").Html;

        Assert.Contains("<figure class=\"figure\">", html);
        Assert.Contains("/img/dev/figure-sample.svg", html);
        Assert.True(File.Exists(Path.Combine(
            CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "img", "dev",
            "figure-sample.svg")));
    }

    [Fact]
    public void PrintKeepsTheFigureAndCapsItsHeightInInches()
    {
        // WI-560: print.css had been deleting every glossary term from every
        // printed page since WI-101, and it was only ever visible on paper.
        // The equivalent failures for a figure are the picture vanishing, and
        // a tall picture printing a mostly-blank sheet.
        var print = File.ReadAllText(Path.Combine(
            CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "css", "print.css"));

        Assert.Matches(@"\.figure\s*\{[^}]*break-inside:\s*avoid", print);

        // The cap has to be written at a specificity that can WIN against
        // site.css's `.figure__image` screen rule, and with the !important
        // this whole file uses for that reason — `figure img` is two element
        // names against one class, so it lost, and the only place that was
        // visible was the printed PDF (467pt tall, over a 384pt cap).
        Assert.Matches(
            @"\.figure__image\s*\{[^}]*max-block-size:\s*\d+(\.\d+)?in\s*!important", print);

        // Nothing in here may hide a figure or its picture: this is the
        // `button { display: none }` trap, one component over.
        foreach (Match rule in Regex.Matches(print, @"([^{}]+)\{([^}]*)\}"))
        {
            var selector = rule.Groups[1].Value;
            if (!Regex.IsMatch(selector, @"(^|,|\s)(figure|figure img|\.figure[\w_-]*)\s*(,|$)"))
            {
                continue;
            }

            Assert.DoesNotMatch(@"display:\s*none", rule.Groups[2].Value);
        }
    }

    [Fact]
    public void TheFigureFitsThePhoneColumnRatherThanItsOwnPixelWidth()
    {
        // The site is verified at 390px (WI-440). A 1200px-wide diagram has to
        // be capped by the reading column, and `height: auto` is what keeps
        // the shape when it is — the width/height attributes are there to
        // reserve space, not to set the final size.
        var css = File.ReadAllText(Path.Combine(
            CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "css", "site.css"));

        var rule = Regex.Match(css, @"\.figure__image\s*\{([^}]*)\}").Groups[1].Value;
        Assert.Contains("max-inline-size: 100%", rule);
        Assert.Contains("height: auto", rule);
        Assert.Contains("max-block-size", rule);
    }
}
