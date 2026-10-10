using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-572: the words that travel with a picture — its alt text, its caption,
/// and the labels drawn inside it.
///
/// <para><b>Why this file exists.</b> The corpus-wide restatement probes stop
/// reading a figure's words here (<c>CuratedPage.ProseWithoutFigures</c>),
/// because a shared figure's words are identical on every page that shows it
/// BY DESIGN — that is §12.34's one-drawing ruling, not a duplication. But
/// "the probes cannot see it" is only safe if something else can, so this file
/// asks the questions those probes were reaching for and could not phrase:</para>
///
/// <list type="number">
///   <item>one file, one description, everywhere it appears;</item>
///   <item>a figure's words collide with nothing that is not that figure;</item>
///   <item>every label DRAWN on a figure is a claim the page can also make in
///   text, on the page itself or on the page it routes to — which is the
///   acceptance criterion of this item ("alt text and a text equivalent that
///   names every region the diagram labels") turned into a property over the
///   whole corpus rather than a sentence on one page.</item>
/// </list>
/// </summary>
public class FigureWordsTests
{
    /// <summary>One figure as it sits on one page: the placement, not the file.
    /// <paramref name="Text"/> is the whole page source, which the composer needs.</summary>
    private sealed record Shown(string Slug, string Text, string Src, string Alt, string Caption);

    /// <summary>
    /// Every figure on every curated page, read out of the rendered page the
    /// way a reader meets it: the Markdown line's alt and caption joined to
    /// the front matter that declares the file.
    /// </summary>
    private static List<Shown> Figures()
    {
        var shown = new List<Shown>();

        foreach (var (slug, text) in Pages())
        {
            foreach (Match image in Regex.Matches(
                CuratedPage.Body(text).Replace("\r\n", "\n"),
                // BOTH QUOTE STYLES. §3b tells an author to write the caption
                // in single quotes when it contains a double one, so a reader
                // of figure lines that only knows one style silently misses a
                // placement — which this test spotted as 30 figures where the
                // corpus has 33.
                @"(?m)^!\[([^\]]*)\]\((/img/figures/[^\s)]+)(?: ""([^""]*)""| '([^']*)')?\)$"))
            {
                shown.Add(new Shown(slug, text, image.Groups[2].Value,
                    image.Groups[1].Value,
                    image.Groups[3].Success ? image.Groups[3].Value : image.Groups[4].Value));
            }
        }

        return shown;
    }

    private static IEnumerable<(string Slug, string Text)> Pages() =>
        Directory.EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Select(file => (
                Slug: Path.GetRelativePath(CuratedPage.PagesDirectory, file)
                    .Replace('\\', '/')[..^3],
                Text: File.ReadAllText(file)));

    [Fact]
    public void TheCorpusReallyDoesShowTheLocationFamily()
    {
        // The dead-ratchet guard every sweep in this repo now carries: a
        // figure sweep that walks nothing passes forever (WI-582).
        var figures = Figures();

        Assert.Equal(33, figures.Count);
        Assert.Equal(23, figures.Select(f => f.Slug).Distinct().Count());
        Assert.All(figures, f => Assert.StartsWith("/img/figures/dia-", f.Src));
    }

    /// <summary>
    /// ONE FILE, ONE DESCRIPTION. The alt text says what is IN the picture, so
    /// it is a property of the FILE, not of the page: two pages describing the
    /// same drawing differently means one of them is wrong, and a reader who
    /// uses a screen reader is the only one who would ever find out.
    ///
    /// <para>A caption is allowed to differ — it says what the picture MEANS
    /// beside the prose it sits next to, and the inventory says so in as many
    /// words (§5). The corpus does not use that licence yet, and the day it
    /// does, this test still holds the alt text.</para>
    /// </summary>
    [Fact]
    public void OneFileCarriesOneDescriptionEverywhereItAppears()
    {
        foreach (var file in Figures().GroupBy(f => f.Src))
        {
            var descriptions = file.Select(f => f.Alt).Distinct(StringComparer.Ordinal).ToList();

            Assert.True(descriptions.Count == 1,
                $"{file.Key} is described {descriptions.Count} different ways across "
                + $"{file.Count()} pages:\n  " + string.Join("\n  ", descriptions));
        }
    }

    /// <summary>
    /// The restatement question, asked where it can be answered: a figure's
    /// words may be shared with another page SHOWING THE SAME FILE and with
    /// nothing else. The prose side excludes figure lines, so the comparison
    /// is figure-words against page-words rather than against itself.
    /// </summary>
    [Fact]
    public void AFiguresWordsCollideWithNothingButTheSameFigure()
    {
        var figures = Figures();
        var offenders = Collisions(figures);

        Assert.True(offenders.Count == 0,
            "a figure's alt text or caption says what a page already says in prose. The "
            + "picture is beside those words, so the reader meets the sentence twice and the "
            + "second one tells them nothing:\n  " + string.Join("\n  ", offenders));

        // THE POSITIVE CONTROL, and this guard needed one badly: it was made
        // to pass twice in a row by widening what the PROSE side ignores
        // (headings, then link text), and each widening is a step towards a
        // probe that can no longer see anything (WI-569). So a planted
        // caption, lifted verbatim out of /tumors/glioma's own prose, has to
        // be caught — by the same code path, with nothing adjusted.
        //
        // IT IS PLANTED ON AN EXISTING PLACEMENT, and the first version gave
        // it a src of its own. That control passed while the real defect
        // walked through: the comparison ran one placement per FILE, so a
        // restating caption on the eighth page showing a shared figure was
        // never read, and a planted figure with a unique src was the one
        // shape that could not reproduce it. A control that enters by a
        // different door than the defect pins nothing — this repo has now
        // paid for that sentence three times (WI-579, §12.32, and here, where
        // the break harness is what found it rather than reading).
        const string planted =
            "Tumor cells spread out into the normal brain around them, and they reach further "
            + "than the abnormal area you can see on a scan.";

        // AND IT IS PLANTED SO THAT `DistinctBy` WOULD HAVE DROPPED IT. Two
        // things make that true and both are load-bearing: the file has to be
        // one SHARED by several pages (`Where(g => g.Count() > 1)`), and the
        // planted placement has to come after another placement of the same
        // file in the sequence — which `.Append` guarantees, because it moves
        // it to the end. Replacing the element in place instead would restore
        // the defect silently on the pages enumerated first. The repair
        // before this one used `figures[^1]`, the only placement of its file,
        // so `DistinctBy` kept it and the control passed with the defect back
        // in: twice in one hour, the same mistake.
        var target = figures
            .GroupBy(f => f.Src, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .First()
            .OrderBy(f => f.Slug, StringComparer.Ordinal)
            .Last();

        var control = figures
            .Where(f => !ReferenceEquals(f, target))
            .Append(target with { Caption = planted });

        Assert.Contains(Collisions(control),
            c => c.Contains("/" + target.Slug + " shows", StringComparison.Ordinal));
    }

    private static List<string> Collisions(IEnumerable<Shown> figures)
    {
        var offenders = new List<string>();

        // THE PROSE SIDE IS THE CORPUS-WIDE NORMALISATION, not a third copy of
        // it: headings out, link text out, the ask-list and the next-steps
        // list out — `CuratedPage.ShinglesOfReaderText`, with the figure lines
        // already removed. Both exclusions earned their place here within an
        // hour of each other. A caption that echoes the HEADING it sits under
        // is the figure saying which section it belongs to, and a caption that
        // matches a DOOR's link text is matching that same heading a second
        // time: /where-your-tumor-is captions the shaded ventricles map "the
        // shaded part is deep in the middle, where the fluid runs", under the
        // entry of that name, which another section links to by its name.
        // Neither is prose a reader meets twice.
        // COMPOSED, because a caption restating the `[MECHANISM]` block's
        // words would be invisible against the raw files: that block is 854
        // words on 18 of these 23 pages, and the figure sits in the section
        // it composes into (§12.10, a rule about a block's prose has to run
        // on the composed page).
        var prose = Pages()
            .Select(p => (p.Slug, Shingles: CuratedPage.ShinglesOfReaderText(
                Regex.Replace(
                    CuratedPage.Composed(p.Text, "/" + p.Slug),
                    @"(?m)^[ 	]*!\[.*$", ""))))
            .ToList();

        // EVERY PLACEMENT, not one per file — and the first version of this
        // line was `.DistinctBy(f => f.Src)`, which is where the break harness
        // found its one survivor. A caption MAY differ from page to page (§5
        // of the inventory says so), so one placement per file is not a
        // sample of the others: a restating caption written onto the eighth
        // page showing the master was never looked at, because the first page
        // alphabetically had already stood in for it. The positive control
        // below passed the whole time, because a planted figure gets a src of
        // its own and walks in through a door the defect cannot use.
        foreach (var figure in figures)
        {
            var words = Shingles(figure.Alt + ". " + figure.Caption);

            foreach (var (slug, pageShingles) in prose)
            {
                var shared = words.Intersect(pageShingles, StringComparer.Ordinal).ToList();
                if (shared.Count > 0)
                {
                    // The PLACEMENT is named, not just the file: one figure is
                    // on up to eight pages and a caption may differ between
                    // them, so "dia-brain-regions restates /tumors/glioma"
                    // does not say which of the eight to go and look at.
                    offenders.Add(
                        $"/{figure.Slug} shows {figure.Src}, which restates /{slug}: "
                        + string.Join(" | ", shared));
                }
            }
        }

        return offenders;
    }

    private static HashSet<string> Shingles(string text)
    {
        var words = Regex.Matches(text.ToLowerInvariant(), "[a-z]+").Select(m => m.Value).ToList();
        var set = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i + 8 <= words.Count; i++)
        {
            set.Add(string.Join(' ', words.Skip(i).Take(8)));
        }

        return set;
    }

    /// <summary>
    /// <b>EVERY LABEL IS A CLAIM THE TEXT CAN ALSO MAKE.</b> WI-572's
    /// acceptance asks for "alt text and a text equivalent that names every
    /// region the diagram labels", and this is that criterion as a property
    /// over the corpus: for every page showing a family figure, each word the
    /// drawing prints has to be findable in the page's own text — or in the
    /// text of the page it routes to from the same section.
    ///
    /// <para><b>The route half is not a loophole, it is the reason the master
    /// can be on eight pages.</b> The master labels nine places; a hub page is
    /// about one tumor and names three or four of them. Requiring the page to
    /// name all nine would push the whole location topic onto every hub, which
    /// is exactly what §12.10 and §12.18 spent two items pulling back OUT of
    /// the hubs. So the hub carries the picture and the door, and
    /// /where-your-tumor-is carries the words.</para>
    ///
    /// <para><b>THE WORDS HAVE TO BE TOGETHER IN ONE SENTENCE OR HEADING, and
    /// the first version only asked whether they were somewhere on the page.</b>
    /// That version was close to vacuous and <c>/review</c> measured it: every
    /// content word of every label — <c>low</c>, <c>back</c>, <c>front</c>,
    /// <c>side</c>, <c>ear</c>, <c>middle</c>, <c>fluid</c> — already occurs
    /// somewhere on nearly every hub, so swapping any variant for any other
    /// left it green. A label is a PHRASE naming one place, and the thing that
    /// makes it a claim the text also makes is a single sentence or heading
    /// carrying all of it.</para>
    /// </summary>
    [Fact]
    public void EveryLabelIsAClaimThePageOrItsRouteCanAlsoMake()
    {
        var figures = Figures();
        var unsupported = Unsupported(figures);

        Assert.True(unsupported.Count == 0,
            "a picture is labeling a place its page never names, so a reader who cannot see "
            + "it is told less than a reader who can:\n  " + string.Join("\n  ", unsupported));

        // THE POSITIVE CONTROL IS A PLAUSIBLE MISLABEL, not invented Latin.
        // The first one was "the zygomatic parasellar recess", which proved
        // only that the matcher is not `return true`. This is the mistake
        // somebody will actually make: the wrong variant on a page.
        // /tumors/acoustic-neuroma is about the nerve behind the ear, and no
        // sentence on it, or on anything it links to, says "covering layers".
        var mislabeled = figures
            .Select(f => f.Slug == "tumors/acoustic-neuroma"
                ? f with { Src = "/img/figures/dia-region-meninges.svg" }
                : f);

        Assert.Contains(Unsupported(mislabeled),
            u => u.Contains("acoustic-neuroma", StringComparison.Ordinal));

        // AND THE LIMIT OF THIS GUARD, WRITTEN DOWN RATHER THAN LEFT TO BE
        // DISCOVERED. Six labels are a single content word — "The front",
        // "The back", "The stalk", and the one-word report labels
        // "cerebellum", "pituitary" and "ventricles" — and a one-word claim
        // is one almost any page can make, so swapping one of those variants
        // onto the wrong page is NOT caught here. What catches that is the
        // inventory binding (`EveryCommittedFigureIsOnExactlyThePages
        // TheInventoryGivesIt`). The set is pinned so that shortening a
        // seventh label into the same uselessness is a decision somebody
        // takes on purpose.
        var master = BrainRegionFamily.Master();
        var weak = BrainRegionFamily.Members(master)
            .SelectMany(id => BrainRegionFamily.VisibleLabels(master, id))
            .Distinct(StringComparer.Ordinal)
            .Where(label => ContentWords(label).Count() < 2)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            ["The back", "The coverings", "The front", "The stalk", "cerebellum",
             "pituitary", "ventricles"],
            weak);

        // AND THE PROPERTY THAT ACTUALLY MATTERS IS NOT THE WORD COUNT, IT IS
        // HOW MANY PAGES CAN MAKE THE CLAIM. "The coverings" is one word and
        // discriminating — two pages in the corpus say it. "The back" is one
        // word and says nothing about which page is looking at it. Measured,
        // so that a label nobody can distinguish is visible as a number
        // rather than inferred from its length.
        var everywhere = Pages()
            .Count(p => Units(CuratedPage.ProseWithoutFigures(p.Text))
                .Any(unit => unit.Contains("back")));

        Assert.True(everywhere > 20,
            $"'back' is on {everywhere} pages, so a label of that one word is not a claim "
            + "about any particular page — which is why the inventory binding, not this "
            + "test, is what catches a variant on the wrong page.");
    }

    /// <summary>
    /// Every label on every placement whose words are not carried together by
    /// one sentence or heading the reader can reach.
    /// </summary>
    private static List<string> Unsupported(IEnumerable<Shown> figures)
    {
        var master = BrainRegionFamily.Master();
        var corpus = Pages().ToDictionary(p => p.Slug, p => p.Text, StringComparer.Ordinal);
        var unsupported = new List<string>();
        var checkedLabels = 0;

        foreach (var figure in figures)
        {
            var figureId = Path.GetFileNameWithoutExtension(figure.Src);
            var composed = CuratedPage.Composed(figure.Text, "/" + figure.Slug);

            // THE PAGE'S OWN PROSE, WITHOUT ITS FIGURES. The first version
            // read the composed page whole, so a label was "supported" by the
            // alt text of the very picture being checked — on seven of the
            // fifteen variant pages that figure line was the ONLY sentence
            // carrying the label's words. The acceptance asks for "alt text
            // AND a text equivalent", and reading the alt text as the text
            // equivalent collapses the and (/review round 2).
            //
            // WI-566: THIS WAS READING THE FRONT MATTER AS PROSE, and nothing
            // could see it. `Composed` returns the WHOLE page, front matter
            // included, and `ReaderTextOfBody` strips authoring markers and
            // nothing else — so a region label counted as "said in the page's
            // own words" when the only place it appeared was a `description`,
            // a `tags` entry or a cited source's TITLE. The whole point of this
            // guard is that a picture may not make a claim the page's prose
            // does not, and a source title is not the page's prose. Found by
            // making `ReaderTextOfBody` refuse a whole page, not by reading.
            var reachable = Units(CuratedPage.ProseWithoutFigures(composed)).ToList();

            // A ROUTE COUNTS FOR THE WHOLE MAP AND NOT FOR A SHADED ONE.
            // The master labels nine places and a hub names three or four, so
            // its text equivalent is /where-your-tumor-is and the door is the
            // obligation (asserted separately below). A VARIANT labels one
            // place, and that place is what the page showing it is about: if
            // the page cannot say it in its own words, the picture is making
            // a claim the page does not. Round 2 measured the difference —
            // with routes allowed for variants, 18 of the 22 hubs reach a
            // page that names all nine regions, so every variant was
            // interchangeable with every other and the guard was nearly
            // vacuous.
            if (BrainRegionFamily.VisibleLabels(master, figureId).Count > 1)
            {
                foreach (var route in RoutesFrom(composed))
                {
                    if (corpus.TryGetValue(route, out var target))
                    {
                        reachable.AddRange(Units(CuratedPage.ProseWithoutFigures(
                            CuratedPage.Composed(target, "/" + route))));
                    }
                }
            }

            foreach (var label in BrainRegionFamily.VisibleLabels(master, figureId))
            {
                checkedLabels++;
                var words = ContentWords(label).ToList();

                if (!reachable.Any(unit => words.All(unit.Contains)))
                {
                    unsupported.Add(
                        $"/{figure.Slug} shows {figureId} labeled \"{label}\", and no sentence "
                        + "or heading it can reach says: " + string.Join(" + ", words));
                }
            }
        }

        // The ratchet is the real count, not a round number under half of it
        // (/review). The 33 placements carry 105 labels between them: nine
        // each on the eight pages showing the whole map and on the
        // report-words twin (81), and one each on the other 24.
        Assert.Equal(105, checkedLabels);
        return unsupported;
    }

    /// <summary>
    /// The page cut into the units a claim lives in: one heading, or one
    /// sentence of prose, as a set of content words. Markdown link targets go
    /// first, so a URL's slug cannot stand in for the words of a sentence.
    /// </summary>
    private static IEnumerable<HashSet<string>> Units(string page)
    {
        var text = Regex.Replace(page.Replace("\r\n", "\n"), @"\]\([^)]*\)", "] ");
        text = Regex.Replace(text, @"\{#[^}]*\}", " ");

        // THE CORPUS HARD-WRAPS ITS PROSE, so a sentence is not a line: the
        // first version split on newlines and reported a label unsupported
        // because "it grows on the nerve" and "that carries hearing and
        // balance from the inner ear to the brain" are two lines of one
        // sentence. A paragraph is joined first, then cut at sentence ends —
        // and a bullet starts a unit of its own, because a list is a set of
        // separate claims rather than one long one.
        foreach (var block in Regex.Split(text, @"\n[ \t]*\n"))
        {
            foreach (var item in Regex.Split(block, @"\n(?=[ \t]*[-*] )"))
            {
                var joined = Regex.Replace(item, @"\s+", " ").Trim();

                foreach (var sentence in Regex.Split(joined, @"(?<=[.!?:;])\s+"))
                {
                    var words = Words(sentence);
                    if (words.Count > 0)
                    {
                        yield return words;
                    }
                }
            }
        }
    }

    /// <summary>
    /// THE DOOR THE NINE-LABEL MAP OWES ITS READER. The master labels nine
    /// places and a hub page is about one tumor, so the words that stand in
    /// for the picture are on /where-your-tumor-is — and a picture whose text
    /// equivalent is on another page is only honest if this page gets the
    /// reader there. Stated separately from the label check above because
    /// that one is satisfied by ANY route that happens to carry the words,
    /// and this is the route that has to exist.
    /// </summary>
    [Fact]
    public void EveryPageShowingTheWholeMapRoutesToThePageThatNamesThePlaces()
    {
        var carriers = Figures()
            .Where(f => f.Src.EndsWith("/dia-brain-regions.svg", StringComparison.Ordinal))
            .Where(f => f.Slug != "where-your-tumor-is")
            .ToList();

        // Seven, derived rather than typed: the eight placements of the whole
        // map, less the page that carries the words itself.
        Assert.Equal(
            Figures().Count(f => f.Src.EndsWith("/dia-brain-regions.svg", StringComparison.Ordinal)) - 1,
            carriers.Count);
        Assert.NotEmpty(carriers);

        foreach (var figure in carriers)
        {
            Assert.Contains("](/where-your-tumor-is",
                CuratedPage.Composed(figure.Text, "/" + figure.Slug), StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The page slugs a page links to, in-site links only.
    ///
    /// <para><b>Read over the whole composed page, and the first version read
    /// one section.</b> Section scope is the rule a reader would want — the
    /// door beside the picture — and the corpus says it cannot be had: the
    /// route a hub has to /where-your-tumor-is lives inside the shared
    /// <c>[MECHANISM]</c> block, and that block opens a <c>##</c> heading of
    /// its own partway through. So the door is four paragraphs under the
    /// figure and in the NEXT section by the composer's reckoning. A guard
    /// scoped to the section would have been a rule about where a shared
    /// block puts its heading, which is not a fact about the picture.</para>
    /// </summary>
    private static IEnumerable<string> RoutesFrom(string page) =>
        Regex.Matches(page, @"\]\(/([a-z0-9/-]+)")
            .Select(m => m.Groups[1].Value.TrimEnd('/'))
            .Distinct(StringComparer.Ordinal);

    private static HashSet<string> Words(string text) =>
        [.. Regex.Matches(text.ToLowerInvariant(), "[a-z]+").Select(m => m.Value)];

    /// <summary>
    /// A label's words with the ones that carry nothing dropped. The test is
    /// "does the page name this place", not "does it use this phrasing" —
    /// the drawing says "The side, near the ear" and the page's heading says
    /// "The side of your brain, near your ear", and those are the same claim.
    /// </summary>
    private static IEnumerable<string> ContentWords(string label) =>
        Regex.Matches(label.ToLowerInvariant(), "[a-z]+")
            .Select(m => m.Value)
            .Where(w => !Stopwords.Contains(w))
            .Distinct(StringComparer.Ordinal);

    private static readonly HashSet<string> Stopwords =
    [
        "the", "a", "an", "of", "your", "my", "in", "on", "at", "to", "and", "is", "it",
        "this", "that", "with", "for",
    ];
}
