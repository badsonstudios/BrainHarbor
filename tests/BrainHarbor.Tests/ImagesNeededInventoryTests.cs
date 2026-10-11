using BrainHarbor.ContentCheck;
using BrainHarbor.Safety;
using BrainHarbor.Web.Content;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-562: <c>docs/images-needed.md</c> is a LIST, and a list about 55 files that
/// nothing reads goes stale the week after it is written. This class is what makes
/// it an artefact instead of a note.
///
/// <para><b>What it holds, and why each one is a real failure rather than tidiness:</b></para>
/// <list type="number">
///   <item><b>Coverage is closed in both directions.</b> Every page under
///     <c>Content/pages</c> either has slots or is in the recorded no-slots set —
///     so a page added by a later item cannot acquire "no pictures needed" by
///     being forgotten, which is the only way this file can quietly stop being
///     an inventory.</item>
///   <item><b>The density floor is re-measured from the live corpus</b>, with the
///     shared blocks COMPOSED IN. The rule is one figure per ~500 words of the
///     text a reader reads, and §3a text is text a reader reads: the
///     <c>caregiver</c> block alone is 413 words on 37 pages. Reading the raw
///     <c>.md</c> files instead would under-count the longest unbroken stretches
///     on the site by about 2,000 words a hub.</item>
///   <item><b>Every position is checked against the page.</b> A slot says which
///     heading it follows; a renamed heading has to fail here rather than leave a
///     position nobody can find. §12.17's rule in a second channel — assert the
///     anchor, do not trust the prose.</item>
///   <item><b>Every draft alt text and caption is graded at the same 6.0 the page
///     is</b>, and held to §3b's figure rules, against
///     <see cref="ContentFigures.AltPrefixesToAvoid"/> itself rather than a copy of
///     it. The acceptance asks for draft alt text precisely so accessibility is
///     "not an afterthought at paste time" — a draft nothing grades is exactly that
///     afterthought, one surface over (§12.33's ruling on the caption, pointed at
///     this file).</item>
///   <item><b>No slot is inside an <c>:::outlook</c> gate.</b> §3b permits a figure
///     there and this inventory places none, on any page: the outlook section's
///     subject is how long people live, and a picture there illustrates a
///     prognosis. Nothing else in the repo would catch one being added.</item>
///   <item><b>A kind-4 figure is used by exactly one page unless it is on a named
///     exemption list.</b> A shared photograph is honest — a waiting room is a
///     waiting room. A shared SCAN is a false claim: an MRI of a glioblastoma on
///     the meningioma page says "this is what yours looks like".</item>
///   <item><b>Every derived number in the file is checked against the file's own
///     tables</b> — the kind counts, both wave totals, every <c>used by</c> line,
///     every <c>Lands at</c> filename, and each page's "N listed". Those are pure
///     functions of the tables below them, so there is no corpus-drift cost to
///     pinning them and no reason to let the next hand edit break one silently.</item>
/// </list>
///
/// <para><b>What is deliberately NOT pinned: the word counts the file quotes.</b>
/// They are a measurement at a named commit, and pinning them would turn every
/// ordinary content edit anywhere in the corpus into a red build in this one file.
/// What is pinned is the FLOOR — slots listed ≥ slots required, re-measured every
/// run — and the file's internal consistency.</para>
/// </summary>
public sealed class ImagesNeededInventoryTests
{
    private const string KindsHeading = "## 2. The four kinds";
    private const string WavesHeading = "## 3. Sourcing order";
    private const string RulesHeading = "## 4. Before any picture is committed";
    private const string CatalogueHeading = "## 5. The figure catalogue";
    private const string SlotsHeading = "## 6. The per-page slots";
    private const string NoSlotsHeading = "## 7. The pages with no slots";

    /// <summary>
    /// The kind-4 figures that may appear on more than one page, and why each one
    /// is honest to repeat. The rule is about a picture <b>of the reader's own
    /// diagnosis</b>: a scan of a glioblastoma on the meningioma page claims
    /// something false. A scan that illustrates a MECHANISM — blocked fluid, a
    /// stain, a tract map — claims nothing about which tumor the reader has, so it
    /// may be shared like a photograph.
    ///
    /// <para>An explicit list rather than a name prefix, because <c>/review</c> was
    /// right that <c>StartsWith("pd-scan-")</c> made the guard a rule about
    /// FILENAMES: rename one diagnosis scan and it walks out of the check with no
    /// red build. Selecting on the KIND and naming the exemptions means an escape
    /// has to be written down here, in the most-reviewed place to put it.</para>
    /// </summary>
    private static readonly Dictionary<string, string> KindFourMayRepeat = new(StringComparer.Ordinal)
    {
        ["pd-hydrocephalus-ct"] =
            "big fluid spaces are the mechanism a shunt exists for, and the same picture is "
            + "true on the shunts page and the location page. Neither claims it is the "
            + "reader's tumor.",
    };

    private static readonly Lazy<Inventory> Cached = new(ReadInventory);
    private static readonly Lazy<IReadOnlyDictionary<string, LivePage>> CachedCorpus = new(LoadCorpus);

    private static Inventory Inv => Cached.Value;

    private static IReadOnlyDictionary<string, LivePage> Corpus => CachedCorpus.Value;

    private static string RepoRoot() => CuratedPage.RepoRoot();

    private static string InventoryPath => Path.Combine(RepoRoot(), "docs", "images-needed.md");

    private static string PagesRoot =>
        Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages");

    private static string BlocksRoot =>
        Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content", "blocks");

    // ------------------------------------------------------------- the corpus

    private sealed record LivePage(string Slug, string Body, string Composed)
    {
        /// <summary>
        /// A word is a whitespace-run token with a letter or a digit in it, counted
        /// over the COMPOSED body. That drops the Markdown furniture (<c>##</c>,
        /// <c>|</c>, <c>-</c>, <c>&gt;</c>) and nothing a reader reads. The
        /// inventory counts the same way.
        ///
        /// <para>It counts over the raw composed Markdown rather than
        /// <see cref="CuratedPage.ReaderTextOfBody"/>, so WI-509's 113
        /// <c>!%term%</c> tooltip-suppression markers count as words nobody reads.
        /// Measured (<c>/review</c>): 101 words in 218,765, and <b>no page changes
        /// its required-slot count</b> either way. Left as the simpler definition
        /// with the measurement recorded, rather than as an assumption.</para>
        /// </summary>
        public int Words => Composed
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Count(token => token.Any(char.IsLetterOrDigit));

        public int RequiredSlots => Math.Max(1, (int)Math.Ceiling(Words / 500.0));

        /// <summary>
        /// Every <c>##</c>/<c>###</c> heading of the page's OWN file, as written. Two
        /// levels, because the corpus has no <c>####</c> — a slot therefore cannot
        /// reference one, and would fail here if it tried.
        /// </summary>
        public IReadOnlyList<string> Headings => [.. Body
            .Split('\n')
            .Select(line => Regex.Match(line.TrimEnd('\r'), @"^#{2,3}\s+(.+?)\s*$"))
            .Where(match => match.Success)
            .Select(match => HeadingText(match.Groups[1].Value))];

        /// <summary>
        /// Every <c>[BLOCK]</c> directive line that is flush with the margin, as
        /// <c>[CAREGIVER]</c>.
        ///
        /// <para>Indentation is the point, not pedantry: <c>ContentBlocks</c>
        /// deliberately supports an indented directive so a block can be spliced
        /// INTO a list item, and §3b fails a figure that is not a top-level block.
        /// So a directive that moves inside a list takes 37 of this inventory's
        /// slots with it into a shape the build rejects. Dropping indented
        /// directives here means such a slot stops resolving and
        /// <see cref="EverySlotPositionIsAHeadingOrADirectiveThePageActuallyHas"/>
        /// goes red.</para>
        /// </summary>
        public IReadOnlyList<string> BlockDirectives => [.. Body
            .Split('\n')
            .Select(line => Regex.Match(line.TrimEnd('\r'), @"^\[([A-Z0-9][A-Z0-9-]*)\]$"))
            .Where(match => match.Success)
            .Select(match => "[" + match.Groups[1].Value + "]")];

        /// <summary>
        /// The slot POSITIONS whose section holds an <c>:::outlook</c> gate — the
        /// heading the gate sits under, and any directive line inside it.
        ///
        /// <para>The first draft of this looked for headings <i>between</i> the
        /// fences and found none on any of the 55 pages, because the corpus writes
        /// the heading ABOVE the fence every time: <c>## What might happen over
        /// time</c> and then <c>:::outlook</c>. A guard that returns an empty set
        /// over the whole corpus passes forever and measures nothing, which is why
        /// <see cref="TheOutlookSectionIsRealSoThatGuardCanActuallyFail"/> exists
        /// and asserts a floor rather than merely "more than none".</para>
        /// </summary>
        public IReadOnlySet<string> PositionsOwningAnOutlookGate
        {
            get
            {
                var owners = new HashSet<string>(StringComparer.Ordinal);
                var current = (string?)null;
                var open = false;
                foreach (var raw in Body.Split('\n'))
                {
                    var line = raw.TrimEnd('\r').Trim();
                    var heading = Regex.Match(line, @"^#{2,3}\s+(.+?)\s*$");
                    var directive = Regex.Match(line, @"^\[([A-Z0-9][A-Z0-9-]*)\]$");
                    if (heading.Success)
                    {
                        current = HeadingText(heading.Groups[1].Value);
                        open = false;
                    }
                    else if (line == ":::outlook")
                    {
                        open = true;
                        if (current is not null)
                        {
                            owners.Add(current);
                        }
                    }
                    else if (line == ":::")
                    {
                        open = false;
                    }
                    else if (open && directive.Success)
                    {
                        // A directive inside the gate: the block's words are the
                        // outlook's words, so a slot there is a slot in the outlook.
                        owners.Add("[" + directive.Groups[1].Value + "]");
                    }
                }
                return owners;
            }
        }

        private static string HeadingText(string raw) =>
            Regex.Replace(raw, @"\s*\{#[^}]*\}$", "").Trim();
    }

    private static IReadOnlyDictionary<string, LivePage> LoadCorpus()
    {
        var blocks = ContentBlockStore.Load(BlocksRoot);
        var pages = new Dictionary<string, LivePage>(StringComparer.Ordinal);
        foreach (var file in Directory
                     .EnumerateFiles(PagesRoot, "*.md", SearchOption.AllDirectories)
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            var slug = Path.GetRelativePath(PagesRoot, file).Replace('\\', '/')[..^3];
            var raw = File.ReadAllText(file).TrimStart('﻿');

            // The slice is CuratedPage.Body's since WI-566, not a fourth copy of
            // it. This loop re-rolled the split and got both conditions right by
            // hand — and a word count off by three characters is a word count
            // nothing would ever question, which is why the helper now refuses
            // rather than relying on the next copy being as careful. The two
            // assertions stay because they name the SLUG; the throw cannot.
            Assert.StartsWith("---", raw, StringComparison.Ordinal);
            Assert.True(raw.IndexOf("\n---", 3, StringComparison.Ordinal) > 0,
                $"{slug}: unterminated front matter.");
            var body = CuratedPage.Body(raw).TrimStart('\r', '\n');

            var (composed, _) = ContentBlocks.Compose(body, blocks, slug);
            pages[slug] = new LivePage(slug, body, composed);
        }

        Assert.NotEmpty(pages);
        return pages;
    }

    // ---------------------------------------------------------- the inventory

    private sealed record Figure(
        string Id, int Kind, int Wave, string Alt, string Caption, string Getting,
        string LandsAt, IReadOnlyList<string> UsedBy, int? UsedByCount);

    private sealed record Slot(string Slug, int Number, string Position, int Kind, string Figure)
    {
        public bool IsBlockDirective => Position.StartsWith('[');
    }

    private sealed record PageEntry(string Slug, int StatedWords, int StatedRequired, int StatedListed);

    private sealed record WaveSummary(int Wave, int StatedFigures, int StatedSlots,
        IReadOnlyList<(string Figure, int Kind, int Slots)> Rows);

    private sealed record Inventory(
        IReadOnlyDictionary<string, Figure> Figures,
        IReadOnlyList<Slot> Slots,
        IReadOnlyDictionary<string, PageEntry> Pages,
        IReadOnlyDictionary<string, string> NoSlots,
        IReadOnlyDictionary<int, int> StatedKindCounts,
        IReadOnlyList<WaveSummary> Waves,
        IReadOnlyList<string> GettingItLines,
        int StatedListed,
        int StatedPages);

    private static int Num(string text) => int.Parse(text, CultureInfo.InvariantCulture);

    private static string SectionOf(string text, string heading, string? until)
    {
        var start = text.IndexOf(heading, StringComparison.Ordinal);
        Assert.True(start >= 0, $"docs/images-needed.md has no '{heading}' section. "
            + "This test reads the file by its own section headings, so renaming one is a "
            + "deliberate change to the contract and has to be made here too.");
        if (until is null)
        {
            return text[start..];
        }
        var end = text.IndexOf(until, start, StringComparison.Ordinal);
        Assert.True(end > start, $"'{until}' does not follow '{heading}'.");
        return text[start..end];
    }

    private static Inventory ReadInventory()
    {
        Assert.True(File.Exists(InventoryPath),
            "docs/images-needed.md is missing. It IS WI-562's deliverable.");
        return ParseInventory(File.ReadAllText(InventoryPath));
    }

    /// <summary>
    /// A pure function of the file's text, so
    /// <see cref="TheInventoryParsesTheSameOnEitherLineEnding"/> can run it over both
    /// endings. This repo has <c>core.autocrlf=true</c> and CI is Linux/LF, so a
    /// parser that only works on one of them stays green in CI forever (WI-501,
    /// WI-506, WI-508).
    /// </summary>
    private static Inventory ParseInventory(string raw)
    {
        var text = raw.Replace("\r\n", "\n");

        // ---- §5, the catalogue
        var figures = new Dictionary<string, Figure>(StringComparer.Ordinal);
        foreach (Match entry in Regex.Matches(
                     SectionOf(text, CatalogueHeading, SlotsHeading),
                     @"^#### `(?<id>[a-z0-9-]+)` — kind (?<kind>\d), wave (?<wave>\d)$"
                     + @"(?<body>.*?)(?=^#### |^## |\z)",
                     RegexOptions.Multiline | RegexOptions.Singleline))
        {
            var id = entry.Groups["id"].Value;
            var body = entry.Groups["body"].Value;

            string Field(string label)
            {
                var match = Regex.Match(body, $@"^- \*\*{label}:\*\* (.+)$", RegexOptions.Multiline);
                Assert.True(match.Success, $"catalogue entry '{id}' has no '{label}' line. "
                    + "Every entry needs all four, because each one is read by somebody at a "
                    + "different moment: what to look for, what to write in the alt attribute, "
                    + "what to write in the caption, and whether this is a search or a job.");
                return match.Groups[1].Value.Trim();
            }

            var lands = Field("Lands at");
            figures[id] = new Figure(
                id, Num(entry.Groups["kind"].Value), Num(entry.Groups["wave"].Value),
                Field("Draft alt text"), Field("Draft caption"), Field("Getting it"),
                Regex.Match(lands, @"`wwwroot/img/figures/([a-z0-9-]+)\.").Groups[1].Value,
                [.. Regex.Matches(lands, @"`/([a-z0-9/-]+)`").Select(m => m.Groups[1].Value)],
                Regex.Match(lands, @"\*\*(\d+) pages\*\*") is { Success: true } count
                    ? Num(count.Groups[1].Value)
                    : null);
        }

        Assert.NotEmpty(figures);

        // ---- §6, the slot tables
        var slots = new List<Slot>();
        var pages = new Dictionary<string, PageEntry>(StringComparer.Ordinal);
        foreach (Match page in Regex.Matches(
                     SectionOf(text, SlotsHeading, NoSlotsHeading),
                     @"^### `/(?<slug>[a-z0-9/-]+)`$(?<body>.*?)(?=^### |^## |\z)",
                     RegexOptions.Multiline | RegexOptions.Singleline))
        {
            var slug = page.Groups["slug"].Value;
            var body = page.Groups["body"].Value;
            var stated = Regex.Match(body, @"^(\d+) words, (\d+) required, (\d+) listed\.$",
                RegexOptions.Multiline);
            Assert.True(stated.Success, $"/{slug} does not state its words/required/listed line.");
            Assert.False(pages.ContainsKey(slug), $"'{slug}' has two slot tables in the inventory.");
            pages[slug] = new PageEntry(slug,
                Num(stated.Groups[1].Value), Num(stated.Groups[2].Value), Num(stated.Groups[3].Value));

            foreach (Match row in Regex.Matches(
                         body,
                         @"^\| (?<n>\d+) \| (?<pos>.+?) \| (?<kind>\d) \| `(?<fig>[a-z0-9-]+)` \|$",
                         RegexOptions.Multiline))
            {
                var cell = row.Groups["pos"].Value.Trim().Replace("\\|", "|");
                // Unwrap a FULLY fenced cell only. Trim('`') on both ends would also
                // eat the backticks of a heading that merely starts or ends with an
                // inline code span, and `Headings` keeps those (/review).
                var directive = Regex.Match(cell, @"^`(\[[A-Z0-9-]+\])`$");
                slots.Add(new Slot(slug, Num(row.Groups["n"].Value),
                    directive.Success ? directive.Groups[1].Value : cell,
                    Num(row.Groups["kind"].Value), row.Groups["fig"].Value));
            }

            Assert.Contains(slug, slots.Select(s => s.Slug));
        }

        // ---- §7, the pages with none
        var noSlots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match row in Regex.Matches(
                     SectionOf(text, NoSlotsHeading, null),
                     @"^\| `/(?<slug>[a-z0-9/-]+)` \| (?<words>\d+) \| (?<why>.+?) \|$",
                     RegexOptions.Multiline))
        {
            noSlots[row.Groups["slug"].Value] = row.Groups["why"].Value.Trim();
        }

        // ---- §2, the kind table
        var kinds = new Dictionary<int, int>();
        foreach (Match row in Regex.Matches(
                     SectionOf(text, KindsHeading, WavesHeading),
                     @"^\| (?<kind>\d) \| [^|]+ \| \*\*(?<count>\d+)\*\* \|$", RegexOptions.Multiline))
        {
            kinds[Num(row.Groups["kind"].Value)] = Num(row.Groups["count"].Value);
        }

        // ---- §3, the wave tables
        var waves = new List<WaveSummary>();
        foreach (Match wave in Regex.Matches(
                     SectionOf(text, WavesHeading, RulesHeading),
                     @"^### Wave (?<n>\d) — (?<figures>\d+) figures, (?<slots>\d+) slots$"
                     + @"(?<body>.*?)(?=^### |\z)",
                     RegexOptions.Multiline | RegexOptions.Singleline))
        {
            waves.Add(new WaveSummary(Num(wave.Groups["n"].Value),
                Num(wave.Groups["figures"].Value), Num(wave.Groups["slots"].Value),
                [.. Regex.Matches(wave.Groups["body"].Value,
                        @"^\| `(?<fig>[a-z0-9-]+)` \| (?<kind>\d) \| (?<slots>\d+) \|$",
                        RegexOptions.Multiline)
                    .Select(r => (r.Groups["fig"].Value, Num(r.Groups["kind"].Value),
                        Num(r.Groups["slots"].Value)))]));
        }

        var listed = Regex.Match(text, @"This inventory lists \*\*(\d+)\*\*");
        var pageCount = Regex.Match(text, @"\*\*\d+ slots across (\d+) reader-facing pages\*\*");
        Assert.True(listed.Success && pageCount.Success,
            "§1 no longer states the totals this test reads back against the tables.");

        return new Inventory(figures, slots, pages, noSlots, kinds, waves,
            [.. text.Split('\n').Where(l => l.StartsWith("- **Getting it:**", StringComparison.Ordinal))],
            Num(listed.Groups[1].Value), Num(pageCount.Groups[1].Value));
    }

    // ------------------------------------------------------------- the checks

    [Fact]
    public void EveryCuratedPageEitherHasSlotsOrIsRecordedAsHavingNone()
    {
        var withSlots = Inv.Slots.Select(s => s.Slug).ToHashSet(StringComparer.Ordinal);

        var missing = Corpus.Keys
            .Where(slug => !withSlots.Contains(slug) && !Inv.NoSlots.ContainsKey(slug))
            .ToList();

        Assert.True(missing.Count == 0,
            "docs/images-needed.md says nothing about: " + string.Join(", ", missing)
            + ". A page added later must get slots or an explicit reason it has none — "
            + "being forgotten is not one of the two.");

        var both = withSlots.Intersect(Inv.NoSlots.Keys, StringComparer.Ordinal).ToList();
        Assert.True(both.Count == 0, "listed as both having slots and having none: "
            + string.Join(", ", both));

        var ghosts = withSlots.Concat(Inv.NoSlots.Keys)
            .Where(slug => !Corpus.ContainsKey(slug))
            .ToList();
        Assert.True(ghosts.Count == 0,
            "the inventory lists pages that do not exist: " + string.Join(", ", ghosts));
    }

    [Fact]
    public void EveryPageListsAtLeastAsManySlotsAsTheDensityRuleAsksFor()
    {
        var byPage = Inv.Slots.GroupBy(s => s.Slug)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        var tooFew = new List<string>();
        foreach (var (slug, page) in Corpus)
        {
            if (byPage.TryGetValue(slug, out var listed) && listed < page.RequiredSlots)
            {
                tooFew.Add($"/{slug}: {page.Words} words needs {page.RequiredSlots} slots, "
                    + $"the inventory lists {listed}");
            }
        }

        Assert.True(tooFew.Count == 0,
            "the density rule is one figure per ~500 words of the text a READER reads "
            + "(shared blocks composed in), minimum one per page. These pages have outgrown "
            + "their entry in docs/images-needed.md:\n  " + string.Join("\n  ", tooFew));
    }

    [Fact]
    public void EverySlotPositionIsAHeadingOrADirectiveThePageActuallyHas()
    {
        var rotten = new List<string>();

        foreach (var slot in Inv.Slots)
        {
            if (!Corpus.TryGetValue(slot.Slug, out var page))
            {
                continue;
            }
            var found = slot.IsBlockDirective
                ? page.BlockDirectives.Contains(slot.Position, StringComparer.Ordinal)
                : page.Headings.Contains(slot.Position, StringComparer.Ordinal);
            if (!found)
            {
                rotten.Add($"/{slot.Slug} slot {slot.Number}: '{slot.Position}'");
            }
        }

        Assert.True(rotten.Count == 0,
            "a slot names a position the page does not have, so nobody can place the figure. "
            + "A directive counts only at the margin — an indented [BLOCK] is spliced into a "
            + "list item, and §3b fails a figure that is not a top-level block:\n  "
            + string.Join("\n  ", rotten));
    }

    [Fact]
    public void NoSlotSitsInTheOutlookSection()
    {
        var inside = new List<string>();

        foreach (var slot in Inv.Slots)
        {
            if (Corpus.TryGetValue(slot.Slug, out var page)
                && page.PositionsOwningAnOutlookGate.Contains(slot.Position))
            {
                inside.Add($"/{slot.Slug}: '{slot.Position}'");
            }
        }

        Assert.True(inside.Count == 0,
            "a figure is slotted in the section that holds an ':::outlook' gate: "
            + string.Join(", ", inside)
            + ". §3b permits one there and this inventory places none, on purpose: the only "
            + "thing a picture in the outlook section can illustrate is a prognosis.");
    }

    [Fact]
    public void TheOutlookSectionIsRealSoThatGuardCanActuallyFail()
    {
        // WI-569: a property guard that has never been seen to fail has not been
        // shown to work. The check above is worthless if no page has an outlook
        // gate under a heading, so prove the shape exists before trusting it.
        var gated = Corpus.Values
            .Where(page => page.PositionsOwningAnOutlookGate.Count > 0)
            .ToList();

        Assert.True(gated.Count >= 20,
            $"only {gated.Count} pages have a position owning an ':::outlook' gate. "
            + "NoSlotSitsInTheOutlookSection is measuring nothing if this is near zero.");
    }

    [Fact]
    public void EveryFigureASlotNamesIsInTheCatalogueAndEveryCatalogueEntryIsUsed()
    {
        var used = Inv.Slots.Select(s => s.Figure).ToHashSet(StringComparer.Ordinal);

        var undeclared = used.Where(f => !Inv.Figures.ContainsKey(f)).Order().ToList();
        Assert.True(undeclared.Count == 0,
            "a slot points at a figure with no catalogue entry, so nobody knows what to go "
            + "and get: " + string.Join(", ", undeclared));

        var orphans = Inv.Figures.Keys.Where(f => !used.Contains(f)).Order().ToList();
        Assert.True(orphans.Count == 0,
            "the catalogue describes figures no slot asks for: " + string.Join(", ", orphans)
            + ". Sourcing one of those is wasted work, and a file in wwwroot/img/figures that "
            + "no page declares fails FigureTests anyway.");
    }

    [Fact]
    public void TheKindColumnAgreesWithTheCatalogue()
    {
        var wrong = Inv.Slots
            .Where(s => Inv.Figures.TryGetValue(s.Figure, out var f) && f.Kind != s.Kind)
            .Select(s => $"/{s.Slug} slot {s.Number} calls {s.Figure} kind {s.Kind}")
            .ToList();

        Assert.True(wrong.Count == 0,
            "the kind in a slot table disagrees with the catalogue. Kind 2 and kind 3 are "
            + "WORK and kinds 1 and 4 are shopping, so this column is what the sourcing plan "
            + "is read off:\n  " + string.Join("\n  ", wrong));
    }

    [Fact]
    public void NoPageShowsTheSameFigureTwice()
    {
        var doubled = Inv.Slots
            .GroupBy(s => (s.Slug, s.Figure))
            .Where(g => g.Count() > 1)
            .Select(g => $"/{g.Key.Slug}: {g.Key.Figure} x{g.Count()}")
            .ToList();

        Assert.True(doubled.Count == 0,
            "§3b: one declaration, one appearance — a page may not show the same picture "
            + "twice, so an inventory that asks it to cannot be carried out: "
            + string.Join(", ", doubled));
    }

    [Fact]
    public void AKindFourFigureIsOnOnePageUnlessItIsAnExemptionWithAReason()
    {
        var kindFour = Inv.Figures.Values.Where(f => f.Kind == 4).OrderBy(f => f.Id,
            StringComparer.Ordinal).ToList();

        Assert.True(kindFour.Count >= 25,
            $"only {kindFour.Count} kind-4 figures: this check is about the pictures that ARE "
            + "the diagnosis, and it is measuring nothing if the kind has gone.");

        var shared = kindFour
            .Where(f => !KindFourMayRepeat.ContainsKey(f.Id))
            .Select(f => (f.Id, Pages: Inv.Slots
                .Where(s => s.Figure == f.Id).Select(s => s.Slug).Distinct().ToList()))
            .Where(x => x.Pages.Count != 1)
            .Select(x => $"{x.Id} -> {string.Join(", ", x.Pages)}")
            .ToList();

        Assert.True(shared.Count == 0,
            "a kind-4 figure is on more than one page (or none) and is not on the exemption "
            + "list above. A shared PHOTOGRAPH is honest; a shared SCAN OF A DIAGNOSIS is a "
            + "false claim — an MRI of one tumor on another tumor's page says 'this is what "
            + "yours looks like'. If the picture illustrates a mechanism rather than a "
            + "diagnosis, add it to KindFourMayRepeat with the reason:\n  "
            + string.Join("\n  ", shared));

        // An exemption is an obligation, not a waiver (TumorHubTemplateSweepTests'
        // rule): a member that no longer repeats is a member whose reason nobody
        // has had to think about, and the list must not become a place to park a
        // name. Every entry has to still be exercising the exception it claims.
        foreach (var (id, why) in KindFourMayRepeat)
        {
            Assert.True(Inv.Figures.ContainsKey(id), $"exempted figure '{id}' is not in the catalogue.");
            Assert.True(why.Length > 40, $"'{id}' is exempted without a real reason.");
            Assert.True(
                Inv.Slots.Where(s => s.Figure == id).Select(s => s.Slug).Distinct().Count() > 1,
                $"'{id}' is on the kind-4 exemption list but appears on one page. Delete the "
                + "entry rather than leaving an exemption nothing uses.");
        }
    }

    [Fact]
    public void EveryDraftAltTextObeysTheFigureRules()
    {
        var problems = new List<string>();
        foreach (var figure in Inv.Figures.Values.OrderBy(f => f.Id, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(figure.Alt))
            {
                problems.Add($"{figure.Id}: no alt text");
                continue;
            }
            var lowered = figure.Alt.ToLowerInvariant();
            // ContentFigures' OWN list, not a copy of it: a sixth prefix added
            // there has to reach these 148 drafts without anyone remembering to
            // edit this file (/review).
            foreach (var prefix in ContentFigures.AltPrefixesToAvoid)
            {
                if (lowered.StartsWith(prefix, StringComparison.Ordinal))
                {
                    problems.Add($"{figure.Id}: alt text opens '{figure.Alt[..prefix.Length]}'");
                }
            }
            if (string.Equals(figure.Alt.TrimEnd('.'), figure.Caption.TrimEnd('.'),
                    StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"{figure.Id}: the alt text and the caption are the same words");
            }
        }

        Assert.True(problems.Count == 0,
            "§3b fails a page for each of these, so a draft that carries one is a draft that "
            + "cannot be pasted:\n  " + string.Join("\n  ", problems));

        Assert.NotEmpty(ContentFigures.AltPrefixesToAvoid);
    }

    /// <summary>
    /// <b>A PICTURE THAT EXISTS IS ON EXACTLY THE PAGES THIS FILE PLANNED FOR
    /// IT (WI-572).</b> Until a figure is committed the inventory is a list of
    /// intentions and nothing can check it against reality; the moment one
    /// lands, the two can disagree — a page that was given a figure nobody
    /// planned, or a slot the inventory still promises and the corpus never
    /// filled.
    ///
    /// <para>Runs over the figures that EXIST, so it says nothing about the
    /// 133 not sourced yet and everything about the 15 that are. The pair of
    /// set comparisons is deliberate (WI-574: a check with one direction is
    /// half a check) — and it is what would catch a figure quietly dropped
    /// from a page during an unrelated edit, which is otherwise invisible: the
    /// page still renders, and so does every other page showing that file.</para>
    /// </summary>
    [Fact]
    public void EveryCommittedFigureIsOnExactlyThePagesTheInventoryGivesIt()
    {
        var figuresDirectory = Path.Combine(
            RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "img", "figures");

        var committed = Directory.EnumerateFiles(figuresDirectory, "*.svg")
            .Concat(Directory.EnumerateFiles(figuresDirectory, "*.png"))
            .Concat(Directory.EnumerateFiles(figuresDirectory, "*.jpg"))
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();

        Assert.NotEmpty(committed);

        foreach (var id in committed.Order(StringComparer.Ordinal))
        {
            var planned = Inv.Slots
                .Where(s => s.Figure == id)
                .Select(s => s.Slug)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToList();

            var shown = Corpus
                .Where(page => page.Value.Body.Contains(
                    $"](/img/figures/{id}.", StringComparison.Ordinal))
                .Select(page => page.Key)
                .Order(StringComparer.Ordinal)
                .ToList();

            Assert.True(planned.SequenceEqual(shown, StringComparer.Ordinal),
                $"'{id}' is committed, and the pages showing it are not the pages this "
                + $"inventory gives it.\n  planned: {string.Join(", ", planned)}"
                + $"\n  shown:   {string.Join(", ", shown)}");

            // AND THE WORDS WENT WITH IT. The draft alt text is written here
            // and pasted onto the page; nothing was holding the two together,
            // so rewording a page would have left this file quietly wrong —
            // and this file is the single source of truth every later item
            // reads (/review). The alt describes the FILE, so it is pinned;
            // the caption may differ per page and §5 says so in as many words.
            foreach (var slug in shown)
            {
                var alt = Regex.Match(
                    Corpus[slug].Body,
                    @"!\[(?<alt>[^\]]*)\]\(/img/figures/" + Regex.Escape(id) + @"\.");

                Assert.True(alt.Success, $"/{slug} shows '{id}' in a shape this cannot read");
                Assert.True(alt.Groups["alt"].Value == Inv.Figures[id].Alt,
                    $"/{slug} describes '{id}' differently from this inventory.\n"
                    + $"  page:      {alt.Groups["alt"].Value}\n"
                    + $"  inventory: {Inv.Figures[id].Alt}");
            }
        }
    }

    /// <summary>
    /// <b>THE DRAFTS ARE PAGE PROSE, SO EVERY RULE ABOUT PAGE PROSE APPLIES —
    /// and the reading grade was the only one being asked (WI-572).</b>
    ///
    /// <para>This inventory was written to be PASTED: "the page edit is a
    /// paste rather than a fresh piece of writing", in the figures README's
    /// own words. The first paste found a British spelling in the very first
    /// alt text — "each part labelled in plain words" — and the corpus-wide
    /// American-forms sweep turned eight pages red at once. Twenty-one of the
    /// 296 drafts carried one: labelled, colour, grey, tablets, licence,
    /// drip. Nothing had looked, because the inventory's gates were §3b's
    /// figure rules and the 6.0 grade, and a British spelling is neither.</para>
    ///
    /// <para>It reads <c>CuratedPage.BritishForms</c> rather than listing
    /// anything, so the next word added to the corpus list reaches these
    /// drafts without anyone remembering this file exists.</para>
    /// </summary>
    [Fact]
    public void EveryDraftIsWrittenInTheAmericanFormsTheCorpusUses()
    {
        var problems = new List<string>();

        foreach (var figure in Inv.Figures.Values.OrderBy(f => f.Id, StringComparer.Ordinal))
        {
            foreach (var (what, prose) in new[] { ("alt", figure.Alt), ("caption", figure.Caption) })
            {
                var lowered = " " + prose.ToLowerInvariant() + " ";

                problems.AddRange(CuratedPage.BritishForms
                    .Where(form => lowered.Contains(form, StringComparison.Ordinal))
                    .Where(form => !CuratedPage.BritishFormExemptions.Any(allowed =>
                        allowed.Contains(form, StringComparison.OrdinalIgnoreCase)
                        && lowered.Contains(allowed.ToLowerInvariant(), StringComparison.Ordinal)))
                    .Select(form => $"{figure.Id} {what}: '{form}' — \"{prose}\""));
            }
        }

        Assert.True(problems.Count == 0,
            "a draft is written in British English and the corpus is American throughout. "
            + "These are pasted onto pages verbatim, where the corpus-wide sweep fails them:\n  "
            + string.Join("\n  ", problems));

        // A control: the list is the corpus's and it can still fire.
        Assert.Contains("tumour", CuratedPage.BritishForms);
    }

    [Fact]
    public void EveryDraftAltTextAndCaptionIsGradedAtTheSameSixPointZeroThePageIs()
    {
        var over = new List<string>();
        foreach (var figure in Inv.Figures.Values.OrderBy(f => f.Id, StringComparer.Ordinal))
        {
            foreach (var (what, prose) in new[] { ("alt", figure.Alt), ("caption", figure.Caption) })
            {
                // Graded the way the page will grade it: through the same extractor,
                // which terminates the sentence before Flesch-Kincaid sees it.
                var grade = ReadingGrade.Of(
                    ContentChecker.ExtractSentences(prose), ReadingGradeOptions.CuratedPages);
                if (grade > ContentChecker.FailGrade)
                {
                    over.Add($"{figure.Id} {what}: grade {grade:F1} — \"{prose}\"");
                }
            }
        }

        Assert.True(over.Count == 0,
            $"a figure's caption and its alt text are both prose a reader reads, and §3b "
            + $"grades them with the rest of the page. This check is STRICTER than the gate on "
            + $"purpose — ContentCheck scores one Flesch-Kincaid over the whole composed page, "
            + $"so a {ContentChecker.FailGrade:F1}+ caption can hide inside an easier page — and "
            + "a draft written to the page limit is a draft that can be pasted anywhere:\n  "
            + string.Join("\n  ", over));
    }

    [Fact]
    public void TheGraderUsedHereIsTheOneThatCanActuallyFail()
    {
        // Two assertions that cannot pass for the wrong reason (§12.33, finding 1):
        // the grader has to reject real prose, and the extractor has to be reached.
        var hard = ReadingGrade.Of(ContentChecker.ExtractSentences(
            "Postoperative neuroradiological surveillance demonstrates considerable "
            + "interobserver variability in the characterisation of pseudoprogression."), ReadingGradeOptions.CuratedPages);
        Assert.True(hard > ContentChecker.FailGrade,
            $"the grader scored a deliberately unreadable sentence at {hard:F1}, "
            + "so the gate above would pass anything.");

        var easy = ReadingGrade.Of(
            ContentChecker.ExtractSentences("A wide white ring with a bed in front of it."), ReadingGradeOptions.CuratedPages);
        Assert.True(easy <= ContentChecker.FailGrade, $"a plain sentence scored {easy:F1}.");
    }

    [Fact]
    public void TheInventoryParsesTheSameOnEitherLineEnding()
    {
        var text = File.ReadAllText(InventoryPath).Replace("\r\n", "\n");
        var lf = ParseInventory(text);
        var crlf = ParseInventory(text.Replace("\n", "\r\n"));

        Assert.Equal(lf.Slots, crlf.Slots);
        Assert.Equal(lf.NoSlots, crlf.NoSlots);
        Assert.Equal(lf.Pages, crlf.Pages);
        Assert.Equal(lf.StatedKindCounts, crlf.StatedKindCounts);
        Assert.Equal(lf.GettingItLines.Count, crlf.GettingItLines.Count);
        Assert.Equal(lf.Figures.Count, crlf.Figures.Count);

        // Compared as flattened strings, NOT as the records: a record with a list
        // member compares that member BY REFERENCE, so Assert.Equal on two
        // identical WaveSummary lists fails — and two genuinely different ones
        // would fail the same way. A comparison that is always false proves
        // nothing in either direction (/review found this one by running it).
        static string[] Flat(Inventory inventory) =>
        [
            .. inventory.Waves.Select(w =>
                $"{w.Wave}|{w.StatedFigures}|{w.StatedSlots}|"
                + string.Join(";", w.Rows.Select(r => $"{r.Figure},{r.Kind},{r.Slots}"))),
            // The figure bodies too: a `$`-anchored match that stopped before \r
            // would leave every alt text ending in a carriage return, and the same
            // draft would then grade differently in CI than it does here.
            .. inventory.Figures.Values.OrderBy(f => f.Id, StringComparer.Ordinal).Select(f =>
                $"{f.Id}|{f.Kind}|{f.Wave}|{f.Alt}|{f.Caption}|{f.Getting}|{f.LandsAt}|"
                + $"{string.Join(";", f.UsedBy)}|{f.UsedByCount}"),
        ];

        Assert.Equal(Flat(lf), Flat(crlf));

        // And Flat() has to be able to tell two inventories apart, or the line
        // above is the same always-true comparison with extra steps.
        Assert.NotEqual(
            Flat(lf),
            Flat(ParseInventory(text.Replace(
                "- **Draft alt text:** A wide white ring with a narrow bed in front of it.",
                "- **Draft alt text:** A narrow bed in front of a wide white ring."))));
    }

    [Fact]
    public void EveryDerivedNumberInTheFileMatchesItsOwnTables()
    {
        // Pure functions of the tables in §5 and §6 — no corpus drift can move any
        // of them, so there is no cost to pinning and no reason to let a hand edit
        // break one quietly (/review). The word counts are NOT here: see the class
        // comment for why those stay a documented snapshot.
        Assert.Equal(Inv.StatedListed, Inv.Slots.Count);
        Assert.Equal(Inv.StatedPages, Inv.Slots.Select(s => s.Slug).Distinct().Count());

        foreach (var (slug, page) in Inv.Pages)
        {
            Assert.Equal(Inv.Slots.Count(s => s.Slug == slug), page.StatedListed);
        }

        foreach (var (kind, stated) in Inv.StatedKindCounts)
        {
            Assert.Equal(Inv.Figures.Values.Count(f => f.Kind == kind), stated);
        }
        Assert.Equal(4, Inv.StatedKindCounts.Count);
        Assert.Equal(Inv.Figures.Count, Inv.StatedKindCounts.Values.Sum());

        Assert.Equal(3, Inv.Waves.Count);
        foreach (var wave in Inv.Waves)
        {
            var members = Inv.Figures.Values.Where(f => f.Wave == wave.Wave).ToList();
            Assert.Equal(members.Count, wave.StatedFigures);
            Assert.Equal(members.Count, wave.Rows.Count);
            Assert.Equal(
                members.Sum(f => Inv.Slots.Count(s => s.Figure == f.Id)), wave.StatedSlots);
            foreach (var (figure, kind, slots) in wave.Rows)
            {
                Assert.Equal(Inv.Figures[figure].Kind, kind);
                Assert.Equal(Inv.Slots.Count(s => s.Figure == figure), slots);
            }
        }
        Assert.Equal(Inv.Figures.Count, Inv.Waves.Sum(w => w.StatedFigures));

        foreach (var figure in Inv.Figures.Values)
        {
            Assert.Equal(figure.Id, figure.LandsAt);
            var pages = Inv.Slots.Where(s => s.Figure == figure.Id)
                .Select(s => s.Slug).Distinct().Order(StringComparer.Ordinal).ToList();
            // Over three pages the line says a COUNT instead of listing them, which
            // is the one case there is nothing to compare — so require the names
            // whenever the line claims to carry them.
            if (figure.UsedByCount is { } count)
            {
                Assert.Equal(pages.Count, count);
            }
            else
            {
                Assert.Equal(pages, figure.UsedBy.Order(StringComparer.Ordinal).ToList());
            }
        }
    }

    [Fact]
    public void TheNoSlotsSetIsClosedAndEveryMemberGivesAReason()
    {
        Assert.Equal(
            ["about", "digest", "how-we-write", "privacy", "terms"],
            Inv.NoSlots.Keys.Order(StringComparer.Ordinal).ToArray());

        foreach (var (slug, why) in Inv.NoSlots)
        {
            Assert.False(string.IsNullOrWhiteSpace(why), $"/{slug} is excluded with no reason.");
            Assert.True(why.Length > 20,
                $"/{slug}'s reason is '{why}'. \"Minimum one per page\" is being set aside for "
                + "this page, so the reason has to be a sentence somebody can disagree with.");
        }
    }

    [Fact]
    public void TheCatalogueCallsTheWorkKindsOutAsWork()
    {
        // The acceptance: the mock-document and diagram slots are called out
        // separately "because they are work rather than shopping and Dan should not
        // discover that halfway through". That is only true if every kind 2 and 3
        // entry says so where it is read, not only in the summary table.
        var work = Inv.Figures.Values.Where(f => f.Kind is 2 or 3)
            .OrderBy(f => f.Id, StringComparer.Ordinal).ToList();

        Assert.True(work.Count >= 50,
            $"only {work.Count} kind-2/3 figures. Most of this catalogue is work rather than "
            + "shopping, so this check is measuring nothing at that count.");

        var silent = work
            .Where(f => !f.Getting.StartsWith("DRAW.", StringComparison.Ordinal)
                && !f.Getting.StartsWith("BUILD.", StringComparison.Ordinal))
            .Select(f => f.Id)
            .ToList();

        Assert.True(silent.Count == 0,
            "these kind 2/3 figures do not open with DRAW. or BUILD., so somebody reading the "
            + "catalogue could take one for a search: " + string.Join(", ", silent));
    }

    [Fact]
    public void NoSourcingNotePointsAtABannedSource()
    {
        // The one rule here that is somebody else's legal problem. ContentFigures
        // enforces it on a committed image's source_url AND its credit (both
        // directions, since WI-561's review); nothing would enforce it on a
        // SOURCING NOTE that told the next person to go to cancer.gov for a brain
        // diagram — and the note is read weeks before the front matter is written,
        // which makes it the moment the mistake is actually made.
        var offenders = BannedSourcePointers(Inv.GettingItLines);

        Assert.True(offenders.Count == 0,
            "a sourcing note names a source whose pictures are barred (PLAN.md §5) without "
            + "saying not to use it. Their TEXT is citable; their images are not:\n  "
            + string.Join("\n  ", offenders));

        // The scan has to be reaching the notes at all. /review found the first
        // version of this proof re-implemented the predicate inline, so the one
        // thing that could kill the guard — the "- **Getting it:**" prefix the
        // caller filters on — was not covered by its own self-test. Rename that
        // label and the real call above iterates nothing; these two lines go red.
        Assert.Equal(Inv.Figures.Count, Inv.GettingItLines.Count);
        Assert.Single(BannedSourcePointers(
            ["- **Getting it:** Grab the brain diagram from cancer.gov."]));
    }

    /// <summary>
    /// Sourcing notes that NAME a banned source without saying not to use it.
    /// Checked per sentence rather than per window of characters: the first version
    /// counted back 60 characters and read "…nothing shipped with the AHFS…" as a
    /// pointer, because its negation was the word before the window opened.
    /// </summary>
    private static List<string> BannedSourcePointers(IEnumerable<string> lines)
    {
        string[] negations = ["never", "not ", "nothing", "no ", "barred", "banned", "ban "];
        // ContentFigures' own two ban lists rather than a third hand-written copy.
        var banned = ContentFigures.BannedImageSourceHosts
            .Concat(ContentFigures.BannedImageCredits)
            .Concat(["nci"])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var offenders = new List<string>();
        foreach (var line in lines)
        {
            foreach (var sentence in line.Split(". ", StringSplitOptions.TrimEntries))
            {
                var named = banned
                    .Where(b => Regex.IsMatch(sentence, @"\b" + Regex.Escape(b) + @"\b",
                        RegexOptions.IgnoreCase))
                    .ToList();
                if (named.Count > 0
                    && !negations.Any(n => sentence.Contains(n, StringComparison.OrdinalIgnoreCase)))
                {
                    offenders.Add($"'{string.Join("/", named)}': {sentence}");
                }
            }
        }
        return offenders;
    }
}
