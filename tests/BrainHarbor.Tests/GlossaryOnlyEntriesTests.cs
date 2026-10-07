using System.Net;
using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-581, §12.29: the entries whose tooltip fires NOWHERE, partitioned by CAUSE.
///
/// <para><b>THE SET THIS FILE IS BUILT OVER</b> (§12.19 asks this first) is the 105
/// shipped <c>glossary/</c> entries crossed with the 55
/// <see cref="CuratedPage.ReaderPages"/>, through <see cref="CuratedPage.Rendered"/> —
/// the real <c>ContentStore.Parse</c>. Measured on this tree: <b>79 entries fire a
/// tooltip somewhere and 26 fire nowhere</b>, the same 26 WI-576 recorded and §12.26
/// wrote down.</para>
///
/// <para><b>THE QUESTION §12.28 ENDED ON IS THE ONE THIS FILE ANSWERS: how many ways
/// does a member of the set reach a reader?</b> An entry has two surfaces. A tooltip
/// fires per page and is computable; <c>/glossary</c> renders <c>GetTerms()</c>
/// unconditionally, so an entry that fires no tooltip anywhere is not an entry nobody
/// can read — it is a <b>glossary-only</b> entry, and <c>/glossary</c> is the whole of
/// its reader exposure. That is why the answer is not "delete it", and it is also why
/// the premise is asserted rather than quoted: see
/// <see cref="GlossaryOnlyEntriesRenderTests"/>.</para>
///
/// <para><b>WHY A RULE PER CAUSE RATHER THAN A RULE FOR THE NUMBER.</b> The 26 had at
/// least three causes when the item was raised and they want different answers. They
/// are computed POSITIVELY here — <b>never as "the rest"</b> — because §12.24's lesson
/// is that a set defined by subtraction is redefined by every new kind of X. That is
/// why there are FOUR kinds: three with a ruling, and a fourth asserted EMPTY by name
/// so a new kind cannot arrive disguised as an old one.</para>
///
/// <para><b>AND THE FOURTH KIND IS ALSO ASKED PER PAGE, which is the half /review found
/// missing.</b> As a cell of the entry partition it can only ever be non-empty for an
/// entry with NO owning page — one of 26 today — so on its own it is a nearly vacuous
/// claim. The property it is really about belongs to a (page, entry) PAIR, and it is
/// gated over all 105 × 55 of them by
/// <see cref="NoPageSaysAGlossaryTermThatFiresNothingOnIt"/>. That is what reds when a
/// page writes a glossary word and the reader silently gets no tooltip, whether or not
/// the entry fires somewhere else.</para>
///
/// <para><b>WHAT THIS SUPERSEDES.</b> WI-576's
/// <c>TheEntriesWhoseTooltipThisItemSuppressedEverywhereAreStillReachable</c> is
/// DELETED. §12.28's supersession rule is that the thing to diff is the ASSERTION LIST
/// and not the purpose, so: that test asserted (1) the entry fires nowhere, (2) it is
/// still shipped, (3) its definition is not empty, (4) the owning page carries the
/// sentence that defines the term, and (5) that most of the glossary still fires — all
/// five about TWO entries and with (4) hand-written. Here (1) is
/// <see cref="TheEntriesWhoseTooltipFiresNowhereArePartitionedByCause"/>, which
/// computes the set instead of taking two names for it; (2) and (3) are
/// <see cref="EveryGlossaryOnlyEntryIsStillReachableWithADefinition"/> over 26; (4) is
/// the <see cref="Owners"/> table — 30 (entry, page) pairs with the sentence of the
/// reader's FIRST MEETING pinned, derived from the rendered artifact rather than
/// transcribed; and (5) is the literal 79.</para>
/// </summary>
public sealed class GlossaryOnlyEntriesTests
{
    // ------------------------------------------------------------------ the measurement

    /// <summary>
    /// <c>GlossaryMarker.BuildMatchers</c>' own matcher, duplicated because its rule is
    /// the subject here: lookarounds and not <c>\b</c>, because a hyphen is a
    /// word-joiner. That is why <c>gene panel</c> does NOT match the heading
    /// <i>"Gene panels"</i>, which is how §12.28's recorded cause for that marker came
    /// out wrong.
    /// </summary>
    private static Regex WholeWord(string name) =>
        new($@"(?<![\w-]){Regex.Escape(name)}(?![\w-])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>An entry reduced to what this file needs: every name it matches on, and its definition.</summary>
    private sealed record Entry(string Slug, string Term, string Definition, string[] Names);

    /// <summary>
    /// <c>BuildMatchers</c> skips a name that is null or whitespace, and so does this —
    /// not housekeeping. <c>WholeWord("")</c> is <c>(?&lt;![\w-])(?![\w-])</c>, which
    /// matches at index 0 of almost every string, so one blank alias would make
    /// <see cref="PresentOn"/> true on all 55 pages and the partition unreadable. No
    /// shipped entry has one today (/review measured it); the filter is here because the
    /// doc above says this matcher IS <c>BuildMatchers</c>', and half of it would not be.
    /// </summary>
    private static Entry[] Entries() =>
        [.. CuratedPage.GlossaryTerms.Select(t => new Entry(t.Slug, t.Term, t.Definition,
            [.. new[] { t.Term }.Concat(t.Aliases).Where(n => !string.IsNullOrWhiteSpace(n))]))];

    /// <summary>
    /// One page, measured once.
    ///
    /// <para><b>The two flattened strings are load-bearing rather than cosmetic.</b>
    /// <c>MergeSoftBreakRuns</c> joins literals across soft breaks before any term is
    /// matched, so <i>"Status\nepilepticus"</i> FIRES — a scan over unwrapped source
    /// lines reads a firing term as silent, and a needle spanning two source lines breaks
    /// on the next re-wrap (§12.24).</para>
    /// </summary>
    /// <param name="Raw">The page as authored, markers and front matter included.</param>
    /// <param name="Composed">
    /// The COMPOSED body, markers INTACT — exactly the string <c>GlossaryMarker</c> is
    /// handed. §12.10: a rule about a block's prose has to run on the composed page, and
    /// that applies to the markers too. A marker inside a block is forbidden (§12.26,
    /// WI-510) and scanning the raw page would be blind to one.
    /// </param>
    /// <param name="All">
    /// <paramref name="Composed"/> flattened, with <c>!%…%</c> and <c>%%…%%</c> handled
    /// the way <c>GlossaryMarker</c> handles them.
    /// </param>
    /// <param name="Prose">
    /// <paramref name="All"/> with the HEADING LINES DROPPED. <c>GlossaryMarker.Mark</c>
    /// walks <c>ParagraphBlock</c>s only, so a heading is not a place a tooltip can fire
    /// (§12.11, both directions). The difference between the two strings is precisely
    /// cause (c).
    /// </param>
    private sealed record Page(string Slug, string Raw, string Composed, string All, string Prose);

    private static Page Read(string slug, string raw)
    {
        var composed = CuratedPage.Rendered(slug, raw).Markdown
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var body = CuratedPage.ReaderTextOfBody(composed);

        var prose = string.Join('\n',
            body.Split('\n').Where(line => !Regex.IsMatch(line, @"^\s{0,3}#{1,6}\s")));

        return new Page(slug, raw, composed, CuratedPage.Flatten(body).Trim(),
            CuratedPage.Flatten(prose).Trim());
    }

    /// <summary>
    /// Inside a link — <b>text OR destination</b>, which is the whole of
    /// <c>GlossaryMarker.HasLinkAncestor</c>'s effect.
    ///
    /// <para><b>The destination half is not a nicety, and /review measured the bill for
    /// leaving it out: 43 (page, entry) pairs.</b> These strings are composed MARKDOWN,
    /// so <c>(/treatments/craniotomy)</c> is in the haystack — and <c>/</c> is neither
    /// <c>\w</c> nor <c>-</c>, so <c>WholeWord("craniotomy")</c> matches inside it.
    /// Markdig puts a URL in <c>LinkInline.Url</c> and never in a <c>LiteralInline</c>,
    /// so <c>GlossaryMarker</c> cannot see it at all. A page deep-linking
    /// <c>/glossary#flair</c> — which is what the tooltip's own fallback link does —
    /// would otherwise be reported as a defect that is not there.</para>
    /// </summary>
    private static bool InLink(string hay, int index)
    {
        foreach (Match m in Regex.Matches(hay, @"\[[^\]]*\]\([^)]*\)"))
        {
            if (index >= m.Index && index < m.Index + m.Length)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Every place in that prose where a tooltip for this entry could fire.</summary>
    private static List<int> MeetingsIn(Entry entry, string prose) =>
        [.. entry.Names
            .SelectMany(n => WholeWord(n).Matches(prose).Select(m => m.Index))
            .Where(i => !InLink(prose, i))
            .OrderBy(i => i)];

    /// <summary>The index of the reader's first meeting with this entry in that prose, or -1.</summary>
    private static int FirstMeeting(Entry entry, string prose) =>
        MeetingsIn(entry, prose).DefaultIfEmpty(-1).Min();

    private static bool SaidOn(Entry entry, Page page) => FirstMeeting(entry, page.Prose) >= 0;

    private static bool PresentOn(Entry entry, Page page) =>
        entry.Names.Any(n => WholeWord(n).IsMatch(page.All));

    /// <summary>The suppression markers on this page that name one of this entry's names.</summary>
    private static List<string> MarkersFor(Entry entry, Page page) =>
        [.. Regex.Matches(page.Composed, @"!%(.+?)%")
            .Select(m => m.Groups[1].Value.Trim())
            .Where(n => entry.Names.Contains(n, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// The pages that SUPPRESS this entry for real, by the two-render method WI-580
    /// shipped: remove that entry's markers and nothing else, render through
    /// <c>ContentStore.Parse</c>, and the tooltip must appear. A substring scan cannot
    /// answer this — it cannot see that a heading or a link never fires, and it cannot
    /// see a term arriving from a block.
    ///
    /// <para><b>ALL of the entry's markers go at once, not one at a time, and that is the
    /// correct probe here.</b> <c>BuildMatchers</c> skips a term if ANY of its names is
    /// suppressed, so on a page carrying both <c>!%H3 G34%</c> and
    /// <c>!%H3 G34-mutant%</c> a one-at-a-time probe would report a false no-op.</para>
    /// </summary>
    private static List<string> OwnersOf(Entry entry, IEnumerable<Page> pages)
    {
        var owners = new List<string>();
        foreach (var page in pages)
        {
            var markers = MarkersFor(entry, page);
            if (markers.Count == 0)
            {
                continue;
            }

            var without = page.Raw;
            foreach (var marker in markers)
            {
                without = without.Replace($"!%{marker}%", "", StringComparison.OrdinalIgnoreCase);
            }

            // The markers are read off the COMPOSED body and removed from the RAW page,
            // so a marker that arrived from an INCLUDED BLOCK cannot be removed and this
            // fails loudly. That is the right failure: §12.26 forbids a marker in a
            // block, because it suppresses the term on every including page at once.
            Assert.True(without != page.Raw,
                $"{page.Slug}: !%{markers[0]}% is in the COMPOSED body but not in the page "
                + "source, so it arrived from an included block — which suppresses that "
                + "term on every including page at once, silently (§12.26, WI-510)");

            if (CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered(page.Slug, without))
                .Contains(entry.Slug))
            {
                owners.Add(page.Slug);
            }
        }

        return owners;
    }

    private sealed record Corpus(Page[] Pages, Entry[] All, Entry[] Nowhere, int Firing);

    /// <summary>
    /// Every page read once and the firing surface unioned over all 55 of them. The union
    /// is the only honest way to ask "does this entry reach a reader through a tooltip at
    /// all".
    /// </summary>
    private static Corpus Measure()
    {
        var pages = CuratedPage.ReaderPages().Select(p => Read(p.Slug, p.Text)).ToArray();
        var firing = CuratedPage.GlossarySlugsFiringAnywhere();

        var all = Entries();
        var nowhere = all.Where(e => !firing.Contains(e.Slug))
            .OrderBy(e => e.Slug, StringComparer.Ordinal)
            .ToArray();

        return new Corpus(pages, all, nowhere, firing.Count);
    }

    // ------------------------------------------------------------------ §12.29, the partition

    /// <summary>
    /// The four kinds. Three carry a ruling; the fourth is asserted EMPTY and exists so
    /// that a new kind of X cannot arrive disguised as one of the other three.
    /// </summary>
    private enum Cause
    {
        /// <summary>(a) At least one page says the term and SUPPRESSES the tooltip there.</summary>
        OwnedByItsPage,

        /// <summary>(b) No page says the term, in any name the entry matches on.</summary>
        SaidOnNoPage,

        /// <summary>(c) A page writes the term, but only where no tooltip can fire.</summary>
        OnlyWhereNoTooltipFires,

        /// <summary>
        /// (d) THE FOURTH KIND, and the reason this is a partition rather than a list of
        /// three. A page says the term in PROSE, nothing suppresses it, and still no
        /// tooltip fires anywhere.
        ///
        /// <para><b>As a cell of THIS partition it is narrow, and that is said plainly
        /// rather than glossed: it can only be non-empty for an entry with no owning page
        /// at all.</b> The general property is per-(page, entry) and is gated by
        /// <see cref="NoPageSaysAGlossaryTermThatFiresNothingOnIt"/>, which covers the 25
        /// owned entries this cell cannot speak for.</para>
        /// </summary>
        SaidInProseAndStillSilent,
    }

    private static Cause CauseOf(Entry entry, Corpus corpus)
    {
        var owned = OwnersOf(entry, corpus.Pages).Count > 0;

        // POSITIVE, in this order, and never "the rest": the fourth kind is decided
        // FIRST, so it cannot be absorbed into the else-branch of the other three.
        if (!owned && corpus.Pages.Any(p => SaidOn(entry, p)))
        {
            return Cause.SaidInProseAndStillSilent;
        }

        if (owned)
        {
            return Cause.OwnedByItsPage;
        }

        return corpus.Pages.Any(p => PresentOn(entry, p))
            ? Cause.OnlyWhereNoTooltipFires
            : Cause.SaidOnNoPage;
    }

    /// <summary>
    /// Cause (a). <b>25 of the 26</b>, and the clustering is the ruling's own argument:
    /// <c>/treatments/craniotomy</c> owns eight of them, <c>/treatments/chemotherapy</c>
    /// four, <c>/treatments/radiation-therapy</c> four. A library page that teaches a
    /// vocabulary suppresses that vocabulary, because a tooltip repeating the paragraph
    /// under it is noise (WI-505).
    /// </summary>
    private static readonly string[] OwnedByItsPage =
    [
        "5-ala", "astrocyte", "bone-flap", "carmustine-wafer", "chemoradiation",
        "craniectomy", "debulking", "embryonal-tumor", "focal-seizure", "fractionation",
        "gross-total-resection", "h3-g34", "laser-ablation", "nadir", "neutropenia",
        "oligodendrocyte", "pediatric-type", "procarbazine", "radiation-mask", "rano",
        "simulation", "sma-syndrome", "somnolence-syndrome", "subtotal-resection",
        "transformation",
    ];

    /// <summary>
    /// Cause (b). <b>One</b>, and it is the one that proves the glossary is a reference
    /// work rather than an index of the site's own prose: <c>FLAIR</c> is a word a reader
    /// arrives with, off their own MRI report, and no page of this site writes it. WI-519
    /// raised that shape; §12.29 rules on it. <b>Nothing to fix.</b>
    /// </summary>
    private static readonly string[] SaidOnNoPage = ["flair"];

    /// <summary>
    /// Cause (c). <b>Zero, and it was ONE when this item started</b> — <c>h3-g34</c>,
    /// whose only two occurrences were an index link and a <c>### H3 G34</c> heading on
    /// <c>/tests/molecular-markers</c>. That is a finding about the PAGE, so it was fixed
    /// on the page: the section now says the term in the prose it is headed with, which
    /// moved the entry into cause (a) and turned a no-op marker into a real suppression.
    /// An empty cell proves nothing on its own, which is why
    /// <see cref="TheCausePartitionSeesAllFourOfItsKinds"/> exists (§12.18).
    /// </summary>
    private static readonly string[] OnlyWhereNoTooltipFires = [];

    [Fact]
    public void TheEntriesWhoseTooltipFiresNowhereArePartitionedByCause()
    {
        var corpus = Measure();

        // THE CORPUS CANARIES. The page count is a real cross-check — `ReaderPages()` has
        // a `Where` and a `Select` between the directory and the result — and it is
        // pinned as a LITERAL too, because §12.29 and this file's prose both say 55 and a
        // number stated in prose should not be the one number that floats.
        Assert.Equal(
            Directory.GetFiles(Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web",
                "Content", "pages"), "*.md", SearchOption.AllDirectories).Length,
            corpus.Pages.Length);
        Assert.Equal(55, corpus.Pages.Length);

        // The entry count is a LITERAL and not a comparison with the directory: nothing
        // sits between `Directory.EnumerateFiles` and `Entries()` to mutate the count, so
        // comparing the two would read as a cross-check without being one.
        Assert.Equal(105, corpus.All.Length);

        // THE TWO MEASURED NUMBERS, AS LITERALS. Not "nowhere + firing == 105", which is
        // true by construction for any pair of complements, including (0, 105) — §12.28's
        // shape of an assertion that reads as proof and cannot fail.
        Assert.Equal(79, corpus.Firing);
        Assert.Equal(26, corpus.Nowhere.Length);

        var byCause = corpus.Nowhere.ToDictionary(e => e.Slug, e => CauseOf(e, corpus),
            StringComparer.Ordinal);

        string[] Members(Cause cause) =>
            [.. byCause.Where(kv => kv.Value == cause).Select(kv => kv.Key)
                .OrderBy(s => s, StringComparer.Ordinal)];

        // THE FOURTH KIND FIRST, because its emptiness is a claim rather than a record.
        // Narrow here by construction — see the enum's doc — and gated in general by
        // NoPageSaysAGlossaryTermThatFiresNothingOnIt.
        var fourth = Members(Cause.SaidInProseAndStillSilent);
        Assert.True(fourth.Length == 0,
            "an entry is written in a page's PROSE, no marker suppresses it, and its "
            + "tooltip fires nowhere on the site. That is none of §12.29's three causes: "
            + "the marker is failing to match the words the page actually wrote (a "
            + "%%term%% escape span, a name the entry has no alias for, a longer glossary "
            + "name that claimed the position, or a surface GlossaryMarker does not walk). "
            + "Rule on it before adding it to a list:\n  "
            + string.Join("\n  ", fourth));

        // THE THREE RULED CAUSES, pinned as SETS and not as counts — a count of 25 is
        // satisfied by any 25 slugs, and the point of a partition by cause is which entry
        // is in which.
        Assert.Equal(OwnedByItsPage, Members(Cause.OwnedByItsPage));
        Assert.Equal(SaidOnNoPage, Members(Cause.SaidOnNoPage));
        Assert.Equal(OnlyWhereNoTooltipFires, Members(Cause.OnlyWhereNoTooltipFires));

        // THE PARTITION IS ALREADY ENFORCED BY THE FOUR LINES ABOVE, and asserting it
        // again here would be §12.28's own defect class. `CauseOf` returns exactly one
        // value per entry, so the four `Members()` sets partition the 26 BY CONSTRUCTION;
        // with three pinned to literals and the fourth asserted empty, "the union is the
        // 26" is true for every possible input and no mutation could red it. What is NOT
        // implied is a property of the LITERALS — a slug typed into two lists, or into one
        // list twice, would pass every assertion above and make the partition a lie on
        // paper.
        var listed = OwnedByItsPage.Concat(SaidOnNoPage).Concat(OnlyWhereNoTooltipFires)
            .ToArray();
        Assert.Equal(listed.Length, listed.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(26, listed.Length + fourth.Length);

        // Each list in its own order, so a later edit cannot hide a slug in the middle of
        // a long literal (the shape WI-580's order assertion caught on its first run).
        foreach (var list in new[] { OwnedByItsPage, SaidOnNoPage, OnlyWhereNoTooltipFires })
        {
            Assert.Equal(list.OrderBy(s => s, StringComparer.Ordinal), list);
        }
    }

    // ------------------------------------------------------- §12.29, the per-PAIR gate

    /// <summary>
    /// <b>No page writes a glossary term in its prose and silently gives the reader no
    /// tooltip for it.</b> Over all 105 entries × 55 pages, which is the scope cause (d)
    /// claims and cannot have: as a cell of the entry partition it is only reachable for
    /// an entry with no owning page, so /review planted <c>%%transformation%%</c> on
    /// <c>/tumors/glioma</c> — the textbook cause-(d) condition — and the whole suite
    /// stayed green, because that entry has three owners and filed as cause (a).
    ///
    /// <para><b>The one sanctioned shape is COMPUTED, not allowlisted</b> (§12.28: an
    /// allowlist forgives an overlap, and forgiving it is the same as not seeing it).
    /// <c>FindEarliestMatch</c> takes the LONGEST name at a position, so a term sitting
    /// inside a longer glossary name that fires on the page is not silent — the reader got
    /// a MORE SPECIFIC tooltip at exactly that spot. Measured: <b>two pairs, and both are
    /// that</b> — <c>glioma</c> inside <i>"In a diffuse glioma"</i> on
    /// <c>/treatments/craniotomy</c>, and <c>posterior fossa</c> inside <i>"**posterior
    /// fossa syndrome**"</i> on <c>/tumors/atrt</c>. Every other one of the 5,775 pairs is
    /// clean.</para>
    /// </summary>
    [Fact]
    public void NoPageSaysAGlossaryTermThatFiresNothingOnIt()
    {
        var corpus = Measure();
        var offenders = new List<string>();
        var covered = new List<string>();
        var pairsExamined = 0;
        var firingPairs = 0;
        var claimedSpans = 0;

        foreach (var page in corpus.Pages)
        {
            var firingHere = CuratedPage.GlossaryTooltipsFiringOn(
                CuratedPage.Rendered(page.Slug, page.Raw));

            // The spans a longer name took on THIS page, and only names that really fire
            // here: `BuildMatchers` skips a suppressed term entirely, so a suppressed
            // longer name claims no position at all.
            var claimed = corpus.All
                .Where(e => firingHere.Contains(e.Slug))
                .SelectMany(e => e.Names)
                .SelectMany(n => WholeWord(n).Matches(page.Prose)
                    .Select(m => (Start: m.Index, End: m.Index + m.Length, Length: n.Length)))
                .ToList();

            firingPairs += firingHere.Count;
            claimedSpans += claimed.Count;

            foreach (var entry in corpus.All)
            {
                pairsExamined++;

                if (firingHere.Contains(entry.Slug) || MarkersFor(entry, page).Count > 0)
                {
                    continue;
                }

                var here = MeetingsIn(entry, page.Prose);
                var longest = entry.Names.Max(n => n.Length);
                var silent = here
                    .Where(i => !claimed.Any(c => c.Length > longest && i >= c.Start && i < c.End))
                    .ToList();

                if (silent.Count > 0)
                {
                    var at = silent[0];
                    var from = Math.Max(0, at - 50);
                    var to = Math.Min(page.Prose.Length, at + 60);
                    offenders.Add($"{page.Slug} says \"{entry.Term}\" at {at} and no tooltip "
                        + $"for glossary/{entry.Slug} fires anywhere on the page. Nothing "
                        + "suppresses it and no longer glossary name claimed the position: "
                        + $"…{page.Prose[from..to]}…");
                }
                else if (here.Count > 0)
                {
                    covered.Add($"{page.Slug} :: {entry.Slug}");
                }
            }
        }

        // THE FLOORS, so a scan that examined nothing cannot pass — and they are counts
        // of what this loop REALLY does, taken before the `continue` above. The first
        // version counted occurrences AFTER it, which is the pairs under examination for
        // SILENCE: two of them, so a floor of 500 over that quantity reported the gate as
        // broken while the gate was right. A canary about a different quantity is worse
        // than no canary.
        //
        // `firingPairs` is the renderer having run over the corpus (measured: 244) and
        // `claimedSpans` is the prose scan having run (measured: 698). Floors rather
        // than literals, because both move with any new tooltip anywhere and neither is
        // this gate's subject.
        Assert.Equal(5775, pairsExamined);
        Assert.True(firingPairs > 200,
            $"only {firingPairs} (page, entry) tooltips fired across the whole corpus, so "
            + "this scan is looking at pages where the glossary reaches nobody");
        Assert.True(claimedSpans > 500,
            $"only {claimedSpans} prose spans were claimed by a firing glossary name, so "
            + "the prose scan under this gate cannot be evidence about positions");

        Assert.True(offenders.Count == 0,
            "a page writes a glossary term in its prose and the reader gets no tooltip for "
            + "it anywhere on that page (§12.29's cause (d), asked per page):\n  "
            + string.Join("\n  ", offenders));

        // THE SANCTIONED SHAPE, COUNTED AND NAMED. Two pairs, both longest-name-wins doing
        // its job; pinned so a third arrival is a decision somebody makes rather than a
        // number that drifted.
        Assert.Equal(
            ["treatments/craniotomy :: glioma", "tumors/atrt :: posterior-fossa"],
            covered.OrderBy(s => s, StringComparer.Ordinal).ToArray());
    }

    // ------------------------------------------------------------------ §12.29, rule for (a)

    /// <summary>
    /// One page that owns one glossary-only entry, and <b>the sentence in which the
    /// reader's first meeting with the term happens</b> on it.
    ///
    /// <para>That third field is deliberately NOT called "the definition". It is the
    /// sentence <c>GlossaryMarker</c> would have fired in, derived from the rendered
    /// artifact by this item's probe rather than transcribed, and it is what §12.26's
    /// named hazard needs: delete the sentence that explains the term, leave
    /// <c>!%term%</c> in place, and every shingle guard in the repo goes quiet. For 27 of
    /// the 30 that sentence names the term and explains it in one breath (<i>"That piece
    /// is called a bone flap."</i>); for three it is a bolded list label on
    /// <c>/treatments/craniotomy</c> (<i>"- **Debulking.**"</i>) and the explanation sits
    /// beside it, which is said plainly here rather than claimed away.</para>
    /// </summary>
    private sealed record Owner(string Entry, string Page, string FirstMeeting);

    /// <summary>
    /// All 30 (entry, page) pairs: 25 entries, three of them owned by more than one page
    /// (<c>embryonal-tumor</c> by three, <c>transformation</c> by three,
    /// <c>pediatric-type</c> by two). Every page that suppresses has to carry the
    /// sentence, not just one of them.
    /// </summary>
    private static readonly Owner[] Owners =
    [
        new("5-ala", "treatments/craniotomy", "It is called 5-ALA."),
        new("astrocyte", "tumors/astrocytoma", "One kind of helper cell is shaped like a star, and is called an **astrocyte**."),
        new("bone-flap", "treatments/craniotomy", "That piece is called a bone flap."),
        new("carmustine-wafer", "treatments/chemotherapy", "Carmustine wafers are sometimes considered if there is another operation."),
        new("chemoradiation", "tumors/high-grade-glioma", "Your team may call the combination chemoradiation, or the Stupp protocol, after the doctor who designed it."),
        new("craniectomy", "treatments/craniotomy", "That is called a craniectomy, and it is done to leave room if the brain is likely to swell."),
        new("debulking", "treatments/craniotomy", "- **Debulking.**"),
        new("embryonal-tumor", "tumors/atrt", "It is an **embryonal tumor**."),
        new("embryonal-tumor", "tumors/medulloblastoma", "It is an **embryonal tumor**."),
        new("embryonal-tumor", "tumors/pediatric-brain-tumor", "[ATRT](/tumors/atrt) is an embryonal tumor, a group this page explains further down, and that group is more common in young children than in older ones."),
        new("focal-seizure", "seizures/living-with", "A focal seizure starts in one part of the brain: some people stay aware right through one, and some do not."),
        new("fractionation", "treatments/radiation-therapy", "Each one is called a fraction, and dividing them up like this is called fractionation."),
        new("gross-total-resection", "treatments/craniotomy", "- **Gross total resection.**"),
        new("h3-g34", "tests/molecular-markers", "The spot is written G34, so the finding is written H3 G34."),
        new("laser-ablation", "treatments/craniotomy", "One alternative is **laser ablation**, also called LITT."),
        new("nadir", "treatments/chemotherapy", "The lowest point is called the nadir, and it is usually about seven to ten days after treatment."),
        new("neutropenia", "treatments/chemotherapy", "You may see this called neutropenia on your results, or hear your team say your neutrophils are low."),
        new("oligodendrocyte", "tumors/oligodendroglioma", "That cell is called an **oligodendrocyte**."),
        new("pediatric-type", "tumors/glioma", "You may see the words \"adult-type\" or \"pediatric-type\" on a report."),
        new("pediatric-type", "tumors/pediatric-brain-tumor", "**A report may say \"pediatric-type\" or \"adult-type\".**"),
        new("procarbazine", "treatments/chemotherapy", "Procarbazine and lomustine are capsules you swallow."),
        new("radiation-mask", "treatments/radiation-therapy", "You may see it called a radiation mask."),
        new("rano", "tests/follow-up-scans", "The rules are called RANO, short for Response Assessment in Neuro-Oncology."),
        new("simulation", "treatments/radiation-therapy", "This visit is called a simulation."),
        new("sma-syndrome", "treatments/craniotomy", "It has a name, SMA syndrome, and the important thing about it is that it is expected to pass."),
        new("somnolence-syndrome", "treatments/radiation-therapy", "That is somnolence syndrome, and it has its own section at the end of this one."),
        new("subtotal-resection", "treatments/craniotomy", "- **Subtotal resection.**"),
        new("transformation", "tumors/astrocytoma", "That is called transformation, and it can go to grade 3 or straight to grade 4."),
        new("transformation", "tumors/high-grade-glioma", "That is called transformation, and it can go to grade 3 or straight to grade 4."),
        new("transformation", "tumors/low-grade-glioma", "That is what people mean by transformation."),
    ];

    /// <summary>
    /// §12.29's rule for cause (a): <b>a suppression is a per-page decision, so the page
    /// that suppresses has to be the page that says the word.</b> WI-567's ruling is
    /// quoted rather than re-derived — the entry is KEPT, because "the suppression is a
    /// per-page decision that can be reversed by an edit, while a missing entry is a gap
    /// every future page inherits" — and this is the premise that ruling rests on,
    /// asserted (§12.27).
    /// </summary>
    [Fact]
    public void EveryGlossaryOnlyEntryOwnedByItsPageStillSaysTheWordThere()
    {
        var corpus = Measure();
        var bySlug = corpus.Nowhere.ToDictionary(e => e.Slug, StringComparer.Ordinal);
        var byPage = corpus.Pages.ToDictionary(p => p.Slug, StringComparer.Ordinal);

        // STALENESS FIRST, and ABOVE the sequence comparison rather than below it. Under
        // the comparison this loop could never fail — every element of `measured` comes
        // from `corpus.Nowhere`, which is what keys `bySlug` — so its message, the one a
        // reader can act on, was unreachable (/review). §12.24: a message a reader acts on
        // is the worst place for an assertion that cannot fire.
        foreach (var owner in Owners)
        {
            Assert.True(bySlug.ContainsKey(owner.Entry),
                $"glossary/{owner.Entry} now fires a tooltip somewhere, so it is not a "
                + "glossary-only entry any more and this record is stale");
            Assert.True(byPage.ContainsKey(owner.Page), owner.Page);
        }

        // EVERY MEASURED PAIR HAS A RECORD *AND* EVERY RECORD IS A MEASURED PAIR —
        // computed from the corpus rather than read off the table, which is the direction
        // a record list cannot check about itself (§12.28's DeliberatelyParallel converse,
        // WI-578's DescriptionsCleanDirectories).
        var measured = new List<(string Entry, string Page)>();
        foreach (var entry in corpus.Nowhere)
        {
            measured.AddRange(OwnersOf(entry, corpus.Pages).Select(page => (entry.Slug, page)));
        }

        Assert.Equal(
            measured.OrderBy(p => p.Entry, StringComparer.Ordinal)
                .ThenBy(p => p.Page, StringComparer.Ordinal).ToArray(),
            Owners.Select(o => (o.Entry, o.Page)).ToArray());

        // 30 PAIRS OVER 25 ENTRIES, AND THESE TWO LINES ARE ABOUT THE TABLE, not about the
        // corpus — the comparison above already forces `measured.Count` to be whatever
        // `Owners.Length` is, so asserting 30 of `measured` would be a fact about a literal
        // two hundred lines up dressed as a measurement (§12.28). Aimed at the array
        // instead they do a real job: they stop this file's prose and §12.29 drifting away
        // from the array as records are added.
        Assert.Equal(30, Owners.Length);
        Assert.Equal(25, Owners.Select(o => o.Entry).Distinct(StringComparer.Ordinal).Count());

        // A SECOND ROUTE to cause (a)'s membership, and it is worth saying exactly how
        // independent it is: the aggregation differs (the partition counts owners per
        // entry; this enumerates pairs and takes distinct entries) while the PRIMITIVE,
        // `OwnersOf`, is shared. So this catches a disagreement between the two literals
        // and between the two aggregations, and it does not catch a wrong `OwnersOf`. The
        // WI-580 DirectBlockNames shape, with its limit stated.
        Assert.Equal(
            OwnedByItsPage,
            Owners.Select(o => o.Entry).Distinct(StringComparer.Ordinal)
                .OrderBy(s => s, StringComparer.Ordinal).ToArray());

        var offenders = new List<string>();
        foreach (var owner in Owners)
        {
            var prose = byPage[owner.Page].Prose;

            if (!prose.Contains(owner.FirstMeeting, StringComparison.Ordinal))
            {
                offenders.Add($"{owner.Page} no longer carries the sentence where its "
                    + $"reader first meets \"{bySlug[owner.Entry].Term}\". It suppresses "
                    + $"glossary/{owner.Entry} with !%…%, and /glossary is that entry's "
                    + "only other surface, so this page is where the word is explained to "
                    + $"the reader who meets it. Expected:\n      {owner.FirstMeeting}");
                continue;
            }

            // A pin is only a pin if it is unique: a sentence that occurs twice can lose
            // one copy without this noticing.
            Assert.Single(Regex.Matches(prose, Regex.Escape(owner.FirstMeeting)));

            // AND IT IS THE *FIRST* MEETING, which is what the field is called and what
            // §12.29 claims. /review measured 12 of the 30 pages as carrying TWO or THREE
            // matching occurrences, so without this a record could pin a LATER one — and
            // then the explaining sentence could be deleted while a downstream restatement
            // kept the suite green, which is the exact edit the pin exists to catch.
            var at = prose.IndexOf(owner.FirstMeeting, StringComparison.Ordinal);
            Assert.InRange(FirstMeeting(bySlug[owner.Entry], prose),
                at, at + owner.FirstMeeting.Length - 1);
        }

        Assert.True(offenders.Count == 0, string.Join("\n  ", offenders));
    }

    // ------------------------------------------------------------------ §12.29, (b) and the premise

    /// <summary>
    /// The decision every cause here rests on: <b>an entry that fires no tooltip is still
    /// a page a reader can reach.</b> §12.28 established that <c>/glossary</c> renders
    /// <c>GetTerms()</c> unconditionally, which is what makes "keep the entry" safe — so
    /// an entry with no definition, or one that stopped being shipped, would turn the
    /// whole ruling into a row on <c>/glossary</c> that says nothing.
    ///
    /// <para>Over all 26 rather than the two WI-576 happened to create, and that is the
    /// widening that matters: <c>flair</c> is cause (b) and has no page at all, so this is
    /// the ONLY thing standing between it and a reader.</para>
    /// </summary>
    [Fact]
    public void EveryGlossaryOnlyEntryIsStillReachableWithADefinition()
    {
        var corpus = Measure();

        Assert.Equal(26, corpus.Nowhere.Length);

        foreach (var entry in corpus.Nowhere)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Term), entry.Slug);
            Assert.False(string.IsNullOrWhiteSpace(entry.Definition),
                $"glossary/{entry.Slug} fires no tooltip anywhere AND has no definition, "
                + "so /glossary lists an empty row and the word is explained nowhere");

            // A definition that is only the term restated is an empty row with extra
            // steps, and the shortest entries are the easiest ones to write that way.
            //
            // THE FLOOR IS CALIBRATED TO THE DEFECT, NOT TO THE CORPUS, and the measured
            // numbers are here so that is checkable: the smallest margin among the 26 is
            // 80 characters (`procarbazine`, a 92-character definition on a 12-character
            // term) and the floor is 20. A floor set near 80 would red on correct writing
            // the first time an entry was tightened; 20 is what it takes to rule out "The
            // drug procarbazine." — the row this premise exists to forbid.
            Assert.True(entry.Definition.Trim().Length > entry.Term.Length + 20,
                $"glossary/{entry.Slug}'s definition is {entry.Definition.Trim().Length} "
                + "characters, too short to be the reader's whole encounter with the word "
                + "(/glossary is this entry's only surface)");
        }
    }

    /// <summary>
    /// §12.29's rule for cause (b), asserted rather than described: <c>flair</c> is a word
    /// no page of this site writes, and that is <b>not</b> a defect. The glossary is a
    /// reference work for words a reader arrives with — off an MRI report, out of a clinic
    /// conversation, from a leaflet — and the alternative (delete the entry) takes the
    /// definition away from the only reader who was ever going to look it up.
    ///
    /// <para>What IS gated is the thing that would make it a lie: <b>no suppression marker
    /// may name a cause-(b) term.</b> A marker for a word the page never writes suppresses
    /// nothing and reads as a decision somebody made — which is exactly what
    /// <c>!%radiation mask%</c> was on <c>/treatments/radiation-therapy</c> before this
    /// item, and §12.28 recorded it as the purest of the six. Corpus-wide that property
    /// belongs to <c>SharedSourceRestatementTests</c>, whose <c>KnownNoOps</c> exemption
    /// list this item emptied and deleted.</para>
    /// </summary>
    [Fact]
    public void ACauseBEntryIsSaidOnNoPageAndIsNamedByNoSuppressionMarker()
    {
        var corpus = Measure();
        var entries = corpus.Nowhere
            .Where(e => SaidOnNoPage.Contains(e.Slug, StringComparer.Ordinal))
            .ToList();

        Assert.Equal(SaidOnNoPage.Length, entries.Count);

        // THE TWO HALVES BELOW ARE NOT WORTH THE SAME. `PresentOn` being false everywhere
        // is how cause (b) is DEFINED, so the partition test already covers it — it is
        // restated here only to fail with the page's name in the message. The MARKER half
        // is the new coverage, and it is the one §12.29 rules on: a marker naming a word
        // no page writes survives the partition untouched, because removing it still does
        // not make the tooltip fire, so the entry stays in cause (b) and nothing else
        // notices.
        foreach (var entry in entries)
        {
            foreach (var page in corpus.Pages)
            {
                Assert.False(PresentOn(entry, page),
                    $"{page.Slug} now writes \"{entry.Term}\", so glossary/{entry.Slug} is "
                    + "not cause (b) any more: either the tooltip fires there, or the term "
                    + "is in a position where it cannot, which is cause (c) and a finding "
                    + "about that page");

                Assert.Empty(MarkersFor(entry, page));
            }
        }
    }

    // ------------------------------------------------------------------ the detector, shown working

    /// <summary>
    /// <b>Two of the four cells are empty, so the classifier has never been seen to put
    /// anything in them</b> — §12.18's rule, which WI-569 spent ten review rounds
    /// learning: a property guard that has never been seen to fail has not been shown to
    /// work. So all four kinds are produced here on probe pages, <b>one edit apart</b>,
    /// with a real shipped entry so the matcher under test is the one the site runs.
    ///
    /// <para><b>And it asserts <see cref="CauseOf"/> itself, not only the primitives it is
    /// built from.</b> /review found the first version asserting <c>Fires</c>,
    /// <c>SaidOn</c>, <c>PresentOn</c> and <c>OwnersOf</c> separately and leaving the
    /// PRECEDENCE between them unexercised — so the doc's claim that "the classifier is
    /// shown putting something in all four" was not what the probe showed.</para>
    /// </summary>
    [Fact]
    public void TheCausePartitionSeesAllFourOfItsKinds()
    {
        var entry = Entries().Single(e => e.Slug == "frozen-section");

        static Page Probe(string prose) =>
            Read("probe", "---\ntitle: Probe\ndescription: A probe page.\n---\n\n" + prose + "\n");

        var inProse = Probe("A frozen section is one step in the operation.");
        var suppressed = Probe("A frozen section is one step in the operation. !%frozen section%");
        var unsaid = Probe("This page is about something else entirely.");
        var headingOnly = Probe("## A frozen section\n\nThis page is about something else.");
        var linkOnly = Probe("See [a frozen section](/tests/biopsy) for the details.");


        // THE DESTINATION PROBE NEEDS A SINGLE-WORD TERM and that is not a convenience:
        // a URL has no spaces, so the slug in `/glossary#frozen-section` is hyphenated
        // and `WholeWord("frozen section")` can never match it. That is why all 43 of
        // /review's live cases are one-word names — 28 of them `craniotomy` — and this
        // probe uses the real `craniotomy` entry for the same reason.
        var crani = Entries().Single(e => e.Slug == "craniotomy");
        var destinationOnly = Probe("See [what comes next](/treatments/craniotomy) for the rest.");
        var escaped = Probe("A %%frozen section%% is one step in the operation.");

        static bool Fires(Page page) =>
            CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered(page.Slug, page.Raw))
                .Contains("frozen-section");

        static Corpus Only(params Page[] pages) => new(pages, Entries(), [], 0);

        // THE CONTROL FIRST: the term in plain prose FIRES. Without it every absence below
        // has two possible causes and the probe proves nothing (§12.26).
        Assert.True(Fires(inProse));

        // (a) the same page ONE EDIT later, with the marker: nothing fires, the term is
        // still in prose, the two-render says the suppression is real, and the CLASSIFIER
        // says cause (a).
        Assert.False(Fires(suppressed));
        Assert.True(SaidOn(entry, suppressed));
        Assert.Equal(["probe"], OwnersOf(entry, [suppressed]));
        Assert.Equal(Cause.OwnedByItsPage, CauseOf(entry, Only(suppressed)));

        // (b) nothing says it.
        Assert.False(PresentOn(entry, unsaid));
        Assert.False(SaidOn(entry, unsaid));
        Assert.Empty(OwnersOf(entry, [unsaid]));
        Assert.Equal(Cause.SaidOnNoPage, CauseOf(entry, Only(unsaid)));

        // (c) THREE positions no tooltip can fire in, each on its own page, because they
        // are three different mechanisms inside GlossaryMarker — the ParagraphBlock walk,
        // HasLinkAncestor over the link TEXT, and a URL that is never a LiteralInline at
        // all. A classifier can get one right and the others wrong, and /review measured
        // 43 live pairs where the first version got the third one wrong.
        foreach (var (probe, subject) in new[]
        {
            (headingOnly, entry), (linkOnly, entry), (destinationOnly, crani),
        })
        {
            Assert.DoesNotContain(subject.Slug,
                CuratedPage.GlossaryTooltipsFiringOn(
                    CuratedPage.Rendered(probe.Slug, probe.Raw)));
            Assert.True(PresentOn(subject, probe));
            Assert.False(SaidOn(subject, probe));
            Assert.Empty(OwnersOf(subject, [probe]));
            Assert.Equal(Cause.OnlyWhereNoTooltipFires, CauseOf(subject, Only(probe)));
        }

        // AND THE DESTINATION PROBE'S CONTROL, because its silence has two possible
        // causes otherwise: the same term in plain prose on the same page FIRES, so the
        // absence above is the URL and not the word.
        Assert.Contains("craniotomy",
            CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered("probe",
                Probe("A craniotomy is an operation. See [what comes next](/treatments/craniotomy).").Raw)));

        // (d) THE FOURTH KIND, which the corpus has never contained: the term is in prose,
        // no marker suppresses it, and the tooltip does not fire — here because `%%…%%`
        // renders that occurrence plain. The classifier must SEE this rather than file it
        // under (b) or (c).
        Assert.False(Fires(escaped));
        Assert.True(SaidOn(entry, escaped));
        Assert.Empty(OwnersOf(entry, [escaped]));
        Assert.Equal(Cause.SaidInProseAndStillSilent, CauseOf(entry, Only(escaped)));

        // AND THE NARROWNESS OF THAT CELL, ASSERTED RATHER THAN DESCRIBED: put the
        // suppressing page beside the escaped one and the entry files as cause (a),
        // because `owned` short-circuits the fourth kind. That is what made /review's
        // planted %%transformation%% invisible, and it is why
        // NoPageSaysAGlossaryTermThatFiresNothingOnIt exists.
        Assert.Equal(Cause.OwnedByItsPage, CauseOf(entry, Only(suppressed, escaped)));

        // ...and the per-pair gate's own discriminator on those same two pages: the
        // escaped page is silent and unexplained, the suppressed one is explained.
        Assert.Empty(MarkersFor(entry, escaped));
        Assert.NotEmpty(MarkersFor(entry, suppressed));
    }

    /// <summary>
    /// The cause-(c) fix this item shipped, pinned on the page it was a finding about —
    /// because the cell it emptied is now a zero, and a zero does not say which page it
    /// came from.
    ///
    /// <para><c>/tests/molecular-markers</c> carries fifteen suppression markers in one
    /// run, because it is the page that DEFINES every marker term. Measured at the start
    /// of this item: <b>ten of the fifteen fired without their marker and five did
    /// not</b> — the blanket was written as the complete set while the prose had slack in
    /// it. Each of the five sections now says the term its <c>###</c> heading is, which is
    /// what the other ten already did, so all fifteen markers suppress something and the
    /// page's own standard is met in fifteen places out of fifteen.</para>
    /// </summary>
    [Fact]
    public void TheFifteenMarkersOnTheMarkerPageAllSuppressSomething()
    {
        const string Slug = "tests/molecular-markers";
        var page = Measure().Pages.Single(p => p.Slug == Slug);

        var markers = Regex.Matches(page.Composed, @"!%(.+?)%")
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Equal(15, markers.Count);

        var byName = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in Entries())
        {
            foreach (var name in e.Names)
            {
                byName[name] = e;
            }
        }

        foreach (var name in markers)
        {
            var entry = byName[name];

            Assert.Equal([Slug], OwnersOf(entry, [page]));

            // AND IN PROSE — which is a CROSS-CHECK between the renderer and this file's
            // own scan rather than extra coverage, and it is better for being that. The
            // two-render above already implies a prose occurrence, because that is the only
            // kind `GlossaryMarker` fires on. So if this line ever reds while the line
            // above passes, `Prose`/`InLink` disagrees with the thing that actually
            // renders, and the partition built on them is wrong.
            Assert.True(FirstMeeting(entry, page.Prose) >= 0,
                $"the section headed \"{name}\" never says the term in its prose, so a "
                + "reader who skims the prose meets the word only in a heading and a jump "
                + "link, where no tooltip can fire (§12.11)");
        }

        // The five sentences this item added, by name, so reverting any one of them reds
        // HERE as well as in the partition — a count of fifteen cannot say which five.
        foreach (var added in new[]
        {
            "Losing both is written CDKN2A/B homozygous deletion.",
            "Extra copies are called amplification, so a report may say EGFR amplification.",
            "The spot is written G34, so the finding is written H3 G34.",
            "It is called a gene panel.",
            "Reading it that way is called methylation profiling.",
        })
        {
            Assert.Contains(added, page.Prose, StringComparison.Ordinal);
        }
    }
}

/// <summary>
/// The premise cause (a) and cause (b) both rest on, asserted against the page the
/// reader actually gets: <b><c>/glossary</c> carries every one of the 105 entries with
/// its definition, and the 26 whose tooltip fires nowhere have no other surface.</b>
///
/// <para>§12.28 established this by reading <c>Pages/Glossary.cshtml</c> — it renders
/// <c>GetTerms()</c> unconditionally — and §12.27's rule is to assert the premise a
/// decision rests on rather than facts about it. Nothing in the repo asserted it over
/// the SHIPPED glossary before this item: <c>GlossaryPageTests</c> runs on a fixture
/// entry, so a change that filtered <c>GetTerms()</c> would have left it green while
/// taking 26 definitions off the site, with no other surface to find them on.</para>
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class GlossaryOnlyEntriesRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GlossaryOnlyEntriesRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task TheGlossaryPageCarriesEveryEntryWhoseTooltipFiresNowhere()
    {
        // Decoded, because Razor escapes an apostrophe to &#x27; and several of these
        // definitions have one — a needle taken from the source would miss on the markup
        // rather than on the content.
        var html = WebUtility.HtmlDecode(
            await _factory.CreateClient().GetStringAsync("/glossary"));

        // EVERY entry's anchor, which subsumes the 26 — the loop below adds the
        // DEFINITION check, which is the half a layout change is most likely to drop
        // while keeping the anchor.
        Assert.Equal(105, CuratedPage.GlossaryTerms.Count);
        foreach (var term in CuratedPage.GlossaryTerms)
        {
            Assert.Contains($"id=\"{term.Slug}\"", html, StringComparison.Ordinal);
        }

        var firing = CuratedPage.GlossarySlugsFiringAnywhere();
        var nowhere = CuratedPage.GlossaryTerms.Where(t => !firing.Contains(t.Slug)).ToList();
        Assert.Equal(26, nowhere.Count);

        foreach (var term in nowhere)
        {
            var opening = string.Join(' ',
                CuratedPage.Flatten(term.Definition).Trim().Split(' ').Take(8));

            // AND THE NEEDLE IS NOT EMPTY, or `Assert.Contains("", html)` would pass for
            // an entry with no definition at all — the very row this premise exists to
            // rule out (/review).
            Assert.NotEmpty(opening);
            Assert.Contains(opening, html, StringComparison.Ordinal);
        }
    }
}
