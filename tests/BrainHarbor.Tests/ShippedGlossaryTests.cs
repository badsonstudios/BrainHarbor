using System.Text.RegularExpressions;
using BrainHarbor.Safety;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-505: the terms actually shipped in Content/glossary, as opposed to the
/// fixture terms GlossaryTests uses. Forty-odd hand-written files is exactly
/// the size where one bad line of YAML, one duplicated alias, or one sentence
/// that quietly editorialises gets in unnoticed.
/// </summary>
public sealed class ShippedGlossaryTests
{
    private static readonly string Root = Path.Combine(
        FindRepoRoot(), "src", "BrainHarbor.Web", "Content", "glossary");

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BrainHarbor.slnx")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName
            ?? throw new InvalidOperationException("could not find the repo root from the test output directory");
    }

    private static List<GlossaryTerm> LoadAll() =>
        [.. Directory.EnumerateFiles(Root, "*.md")
            .OrderBy(f => f, StringComparer.Ordinal)
            .Select(f => GlossaryStore.ParseTerm(File.ReadAllText(f), Path.GetFileNameWithoutExtension(f)))];

    [Fact]
    public void NoDefinitionClaimsAFindingCannotBeInherited()
    {
        // Found by an independent review of WI-509, after it merged.
        //
        // `tp53.md` said "It is a change in the tumor, not one you were born
        // with." TP53 is the Li-Fraumeni gene, and CRUK — already a source on
        // this project — lists Li-Fraumeni among the inherited syndromes that
        // raise brain-tumor risk. The claim was false, it fired site-wide as a
        // tooltip, and it contradicted the two places that handle this
        // correctly: the marker page's own TP53 entry and its somatic/germline
        // section, which both say a tumor test can occasionally turn up
        // something inherited.
        //
        // A popover is the worst place on the site to settle a question this
        // one-sided: a reader whose family history matters could be talked out
        // of asking for genetic counselling by it. Definitions may say a result
        // WAS somatic; they may not rule inheritance out as a general fact.
        // Whitespace-normalised first. Definitions are hard-wrapped, so the
        // real defect text arrives as "...not one you were born\nwith." and a
        // regex with a literal space misses it entirely. Proving this test by
        // breaking it is the only reason that was caught.
        foreach (var term in LoadAll())
        {
            var flat = Regex.Replace(term.Definition, @"\s+", " ");

            Assert.DoesNotMatch(
                new Regex(@"not\s+(a change\s+|one\s+)?(you were|you are)\s+born with",
                    RegexOptions.IgnoreCase),
                flat);
        }
    }

    [Fact]
    public void TheThreeGlioblastomaFindingsCarryTheirIdhWildtypeCondition()
    {
        // The glossary half of the WI-509 correction. CAP Recommendation 9 and
        // WHO CNS5 both state the condition explicitly: TERT promoter
        // mutation, EGFR amplification and +7/-10 only mean "glioblastoma
        // despite lower-grade-looking cells" in an IDH-WILDTYPE tumor.
        //
        // Unconditioned, the tooltip tells an oligodendroglioma reader that
        // their result names a glioblastoma — and TERT mutation is present in
        // a majority of oligodendrogliomas, so that reader is common.
        foreach (var term in LoadAll())
        {
            if (!Regex.IsMatch(term.Definition, @"glioblastoma", RegexOptions.IgnoreCase))
            {
                continue;
            }

            Assert.Matches(
                new Regex(@"IDH", RegexOptions.IgnoreCase),
                term.Definition);
        }
    }

    [Fact]
    public void EveryShippedTermParses()
    {
        // ParseTerm throws on bad front matter, a bad slug or an empty body,
        // so simply loading all of them is the check.
        var terms = LoadAll();

        Assert.True(terms.Count >= 40, $"expected the WI-505 vocabulary, found {terms.Count} terms");
    }

    [Fact]
    public void EveryShippedTermCitesASource()
    {
        // The site's prime directive (content-pipeline §1): never publish a
        // medical claim that cannot be pointed at a source. A one-sentence
        // definition a reader is handed as fact is still a medical claim.
        // "Non-blank" is not enough: `url: TBD` would pass, and WI-505 shipped
        // nine citations that were plausible-looking NBK ids pointing at
        // entirely unrelated chapters. A wrong link that RESOLVES is the worst
        // failure mode here, because nothing looks broken.
        var bad = LoadAll()
            .SelectMany(t => t.Sources.Count == 0
                ? [$"{t.Slug}: no sources"]
                : t.Sources
                    .Where(s => string.IsNullOrWhiteSpace(s.Title)
                        || !Uri.TryCreate(s.Url, UriKind.Absolute, out var uri)
                        || uri.Scheme != Uri.UriSchemeHttps)
                    .Select(s => $"{t.Slug}: '{s.Url}' is not an https URL with a title"))
            .ToList();

        Assert.True(bad.Count == 0, string.Join("; ", bad));
    }

    [Fact]
    public void NoTwoTermsClaimTheSameName()
    {
        // GlossaryMarker tooltips the first match per page and only once per
        // slug, so a duplicated name or alias does not error — one of the two
        // terms simply never appears anywhere, silently, forever.
        var claims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var clashes = new List<string>();

        foreach (var term in LoadAll())
        {
            foreach (var name in new[] { term.Term }.Concat(term.Aliases))
            {
                if (claims.TryGetValue(name, out var owner) && owner != term.Slug)
                {
                    clashes.Add($"'{name}' claimed by both {owner} and {term.Slug}");
                    continue;
                }
                claims[name] = term.Slug;
            }
        }

        Assert.True(clashes.Count == 0, string.Join("; ", clashes));
    }

    /// <summary>
    /// WI-505's editorial rule, made mechanical: **definitions describe, they
    /// do not interpret — no marker is characterised as favourable.**
    ///
    /// This is not fussiness. WI-503 has just put prognosis behind an explicit
    /// reader choice (content-pipeline §12.5) because not every patient wants
    /// it. A tooltip is the one place on the site that choice cannot be
    /// offered — it fires on first mention, unasked, mid-sentence. So a
    /// definition that says a marker means longer survival hands a reader the
    /// exact thing the gate exists to let them decline, and does it on a page
    /// about something else entirely.
    /// </summary>
    [Theory]
    [InlineData("survival")]
    [InlineData("prognosis")]
    [InlineData("prognostic")]
    [InlineData("life expectancy")]
    [InlineData("favorable")]
    [InlineData("favourable")]
    [InlineData("outlook")]
    [InlineData("how long")]
    [InlineData("survive")]
    [InlineData("median")]
    public void NoDefinitionTalksAboutPrognosisAtAll(string word)
    {
        var offenders = LoadAll()
            .Where(t => t.Definition.Contains(word, StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Slug)
            .ToList();

        Assert.True(offenders.Count == 0,
            $"'{word}' appears in a glossary definition ({string.Join(", ", offenders)}). "
            + "Definitions describe what a finding IS and what it decides — a tooltip cannot "
            + "offer the reader-choice gate that prognosis sits behind (WI-503).");
    }

    /// <summary>
    /// The ONLY terms allowed to describe how a tumor behaves. Deliberately an
    /// allowlist rather than a list of markers to check: a marker list makes
    /// every future term (WI-509 adds H3 G34, TP53, chromosome 7/10) exempt by
    /// default, and the strictest guardrail on the site should not opt new
    /// content out silently.
    ///
    /// These four earn it because behaviour is part of what the word MEANS —
    /// "a grade 4 glioma that grows quickly" defines glioblastoma. "Tumors
    /// with this marker grow more slowly" defines nothing; it is a ranking the
    /// reader turns into a personal forecast.
    /// </summary>
    private static readonly string[] MayDescribeBehaviour =
    [
        "glioblastoma", "glioma", "diffuse-glioma", "cns-who-grade",
    ];

    [Theory]
    // Bare "quick" and "worse" are deliberately NOT here. Both have senses
    // with nothing to do with ranking a tumor — frozen section is "a quick
    // look at a piece of tissue", and pseudoprogression is a scan that "looks
    // worse". A substring check cannot tell those senses apart, so blocking
    // the words would force worse writing to satisfy the test. The
    // outcome-phrases below carry the meaning that actually matters.
    [InlineData("slow")]
    [InlineData("fast")]
    [InlineData("aggressive")]
    [InlineData("better")]
    [InlineData("improve")]
    [InlineData("benefit")]
    [InlineData("effective")]
    [InlineData("respond well")]
    [InlineData("longer")]
    [InlineData("shorter")]
    [InlineData("good news")]
    [InlineData("worse outcome")]
    [InlineData("better outcome")]
    [InlineData("does worse")]
    [InlineData("does better")]
    public void NoDefinitionRanksTheFindingGoodOrBad(string word)
    {
        // The version of idh-gene-change this item replaced read "Tumors with
        // this change often grow more slowly and may respond to different
        // treatments." It trips on "slow" and on nothing in the prognosis list
        // above, which is why this second, stricter check exists — the general
        // one would have let it through.
        var offenders = LoadAll()
            .Where(t => !MayDescribeBehaviour.Contains(t.Slug))
            .Where(t => t.Definition.Contains(word, StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Slug)
            .ToList();

        Assert.True(offenders.Count == 0,
            $"'{word}' appears in a glossary definition ({string.Join(", ", offenders)}). "
            + "WI-505: definitions describe, they do not interpret — nothing is "
            + "characterised as favourable. If the word is genuinely part of what the "
            + "term MEANS, add the slug to MayDescribeBehaviour and say why.");
    }

    [Fact]
    public void EveryTermAllowedToDescribeBehaviourStillExists()
    {
        // Otherwise a rename quietly widens the exemption list into nothing,
        // or leaves a stale entry exempting a slug someone later re-creates.
        var slugs = LoadAll().Select(t => t.Slug).ToHashSet(StringComparer.Ordinal);
        var stale = MayDescribeBehaviour.Where(s => !slugs.Contains(s)).ToList();

        Assert.True(stale.Count == 0, "stale exemptions: " + string.Join(", ", stale));
    }

    /// <summary>
    /// Names that would fire the tooltip somewhere it does not belong. Every
    /// one of these was a real candidate during WI-505: `steroids` (broader
    /// than dexamethasone), `fractions` (ordinary English — "a fraction of
    /// patients responded"), `brain swelling` (broader than vasogenic edema,
    /// and steroids are wrong for some of what it covers), bare `EGFR` (means
    /// the mutation more often than amplification), `contrast` and `grade`
    /// (everyday words). GlossaryMarker matches case-insensitively across
    /// 1,000+ live feed summaries, so a bad name is not a typo, it is a wrong
    /// definition attached to someone else's sentence.
    /// </summary>
    [Fact]
    public void NoTermOrAliasIsAWordThatWouldFireInTheWrongPlace()
    {
        string[] stoplist =
        [
            "fractions", "fraction", "contrast", "grade", "stage", "mass",
            "response", "steroids", "brain swelling", "EGFR", "CDKN2A",
            "partial seizure", "diffuse", "midline", "necrosis",
        ];

        var offenders = LoadAll()
            .SelectMany(t => new[] { t.Term }.Concat(t.Aliases).Select(n => (t.Slug, Name: n)))
            .Where(x => stoplist.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => $"{x.Slug} claims '{x.Name}'")
            .ToList();

        Assert.True(offenders.Count == 0, string.Join("; ", offenders));
    }

    [Fact]
    public void NoDefinitionUsesABannedHypePhrase()
    {
        // The same list the summarizer is held to. A curated definition should
        // not be able to say something an AI summary would be flagged for.
        var offenders = LoadAll()
            .Select(t => (t.Slug, Banned: Guardrails.BannedWordsIn(t.Definition)))
            .Where(x => x.Banned.Count > 0)
            .Select(x => $"{x.Slug}: {string.Join(", ", x.Banned)}")
            .ToList();

        Assert.True(offenders.Count == 0, string.Join("; ", offenders));
    }

    [Fact]
    public void RetiredTumorNamesAreNotTaughtAsCurrent()
    {
        // content-pipeline §12.1: NCCN and CNS5 govern naming. These three
        // were retired in 2021 and are exactly what a reader arrives holding
        // from an older leaflet — they belong in the WI-513 crosswalk, which
        // explains that they are retired, not in a tooltip that presents them
        // as the current word.
        string[] retired = ["anaplastic astrocytoma", "oligoastrocytoma", "hemangiopericytoma"];

        // Aliases too, not just the term: `also: [anaplastic astrocytoma]` on
        // the astrocytoma entry is the natural mistake, and it is exactly the
        // crosswalk-versus-tooltip confusion §12.1 warns about — a retired
        // name presented on the glossary page as "Also called".
        var offenders = LoadAll()
            .SelectMany(t => new[] { t.Term }.Concat(t.Aliases)
                .SelectMany(name => retired
                    .Where(r => name.Contains(r, StringComparison.OrdinalIgnoreCase))
                    .Select(r => $"{t.Slug} claims '{r}'")))
            .ToList();

        Assert.True(offenders.Count == 0, string.Join("; ", offenders));
    }

    [Fact]
    public void GradesAreWrittenInArabicNumeralsNotRomanNumerals()
    {
        // CNS5 dropped Roman numerals in 2021 precisely to stop II/III/IV
        // being transcribed wrong. A glossary that reintroduces them would
        // undo that on the page explaining the grade.
        // IgnoreCase matters more than it looks: without it "\bgrade" never
        // matches "Grade", so "CNS WHO Grade IV" — sentence-initial, the
        // likeliest way anyone would actually write it — passes the test whose
        // entire purpose is catching that. Term and aliases are scanned too.
        var pattern = new Regex(@"\bgrade\s+(I{1,3}|IV)\b", RegexOptions.IgnoreCase);

        var offenders = LoadAll()
            .Where(t => new[] { t.Definition, t.Term }.Concat(t.Aliases).Any(pattern.IsMatch))
            .Select(t => t.Slug)
            .ToList();

        Assert.True(offenders.Count == 0,
            "Roman numeral grades in: " + string.Join(", ", offenders));
    }

    /// <summary>
    /// WI-567. A WI-105 suppression marker broken by a LINE WRAP is inert, and it
    /// fails in the worst available direction: it suppresses nothing AND prints the raw
    /// <c>!%frontal lobe%</c> characters to the reader. Nothing else catches it: the
    /// build is green, ContentCheck is green, the page renders, and the tooltip the
    /// author decided to switch off is still firing.
    ///
    /// <b>And the cause is not the regex.</b> <c>GlossaryTooltips.SuppressMarker</c> is
    /// <c>!%(.+?)%</c>, so the obvious diagnosis is that <c>.</c> does not match a
    /// newline — and a <c>RegexOptions.Singleline</c> "fix" would change nothing.
    /// <c>CollectAndStripSuppressions</c> walks <c>LiteralInline</c>s; Markdig has
    /// already split the wrapped paragraph at the soft break, and that pass runs
    /// <em>before</em> <c>MergeSoftBreakRuns</c> puts it back together, so the marker
    /// never exists as one string for the pattern to see. Recorded because an outcome
    /// correctly observed and wrongly explained is how the next person fixes the wrong
    /// thing (§12.17).
    ///
    /// This is here because it actually happened while writing
    /// <c>/where-your-tumor-is</c> — a marker landed inside a wrapped sentence and
    /// three of that page's own tests failed in three different-looking ways before
    /// the cause was one line break. It is asserted corpus-wide rather than on that
    /// page, because the next one will be wrapped by whoever is typing at the time.
    /// The corpus was swept when this was added: this was the only occurrence.
    ///
    /// Read over the raw file with line endings normalised, since the working tree
    /// is CRLF under <c>text=auto</c> and the index is LF.
    /// </summary>
    [Fact]
    public void NoGlossaryMarkerIsBrokenAcrossALineBreakOrLeftUnterminated()
    {
        var content = Path.Combine(
            FindRepoRoot(), "src", "BrainHarbor.Web", "Content");

        var offenders = new List<string>();
        var checkedFiles = 0;

        foreach (var file in Directory.EnumerateFiles(content, "*.md", SearchOption.AllDirectories))
        {
            checkedFiles++;
            var text = File.ReadAllText(file).Replace("\r\n", "\n");

            var relative = Path.GetRelativePath(content, file).Replace('\\', '/');

            offenders.AddRange(Regex.Matches(text, @"!%([^%]*)%")
                .Where(m => m.Groups[1].Value.Contains('\n', StringComparison.Ordinal))
                .Select(m => $"{relative}: "
                    + $"!%{m.Groups[1].Value.Replace("\n", "\\n", StringComparison.Ordinal)}%"));

            // THE SIBLING FAILURE, added at WI-567's /review round 3: a marker with no
            // closing `%` at all. `!%(.+?)%` never matches it, so production leaves the
            // characters `!%frontal lobe` on the page and suppresses nothing — the same
            // outcome as the line-wrap case, reached a different way, and the check
            // above cannot see it because it also requires the closing `%`.
            offenders.AddRange(Regex.Matches(text, @"(?m)!%[^%\r\n]*$")
                .Select(m => $"{relative}: {m.Value} (no closing %)"));
        }

        // The positive half. An enumerate-and-check guard is green over an empty
        // directory, which is the one way this could stop being a guard (WI-517).
        Assert.True(checkedFiles > 100, $"only {checkedFiles} content files scanned");

        Assert.True(offenders.Count == 0,
            "a glossary suppression marker is split across a line break, so it "
            + "suppresses nothing and prints its own characters to the reader:\n  "
            + string.Join("\n  ", offenders));
    }

    // ------------------------------------------------------------------ §12.26
    // WI-576. THE RESTATEMENT RULE, OVER THE DIRECTORY IT COULD NOT SEE — AND
    // OVER THE PAGES THAT WERE IN NO ENTRY AT ALL.
    //
    // `CuratedPage.AssertDoesNotRestateTheCorpus` walked `pages/` + `blocks/`
    // and never `glossary/`. Widening that set was the obvious fix and was NOT
    // the fix: only 19 of the 55 pages call the probe, so 36 pages are in no
    // entry at all, and when the corpus was measured ALL EIGHT real
    // restatements turned out to be on pages in that silent 36. Widening the
    // set alone would have found zero of them.
    //
    // So the gate lives here instead, keyed on nothing but the corpus: every
    // page, composed and rendered, against every definition whose tooltip
    // actually fires on it. There is no list of pages to forget to add to.

    /// <summary>
    /// A page and a glossary entry that share a window ON PURPOSE, with the
    /// reason, and with the SIZE of the overlap pinned (§12.8's WI-509 pattern,
    /// which is a reasoned record and never a bare allowlist).
    ///
    /// <paramref name="Shingles"/> is pinned because a pair-level exemption
    /// would otherwise hide the NEXT collision between the same two files.
    ///
    /// <paramref name="Shared"/> is the WINDOWS THEMSELVES, not a count of them,
    /// and /review found why that distinction is the whole value of the field: a
    /// count only notices an overlap that GROWS OR SHRINKS. Reword
    /// <c>tumors/atrt</c>'s coincidental sentence away and let a genuine
    /// one-window restatement of <c>glossary/nec</c> arrive in its place, and the
    /// count is still 1 — the gate stays silent forever, beside a recorded reason
    /// describing text that no longer exists. The count bought nothing the text
    /// does not.
    /// </summary>
    private sealed record SharedWithTheGlossary(
        string Page, string Entry, string[] Shared, string Why);

    /// <summary>
    /// The collisions WI-576 swept up that are NOT restated definitions, and
    /// where every available fix would make the site worse.
    ///
    /// Suppressing the tooltip is wrong here: the page does not gloss the term
    /// the entry defines, so the tooltip is the only place that reader learns
    /// what the word means. Rewording the page is wrong too — the sentence is
    /// correct, plain and doing a job — and editing correct prose to settle a
    /// shingle is how a guard starts shaping the content instead of checking it.
    ///
    /// <para><b>WI-576 recorded TWO and WI-579 took the second away</b>, which is
    /// what a handed-on record is for. <c>tumors/high-grade-glioma</c> ×
    /// <c>glossary/cdkn2a-b-deletion</c> was the 2021 grading rule, stated by the
    /// hub and restated by the entry; §12.27 settled that the rule's owner is
    /// <c>/tests/pathology-report#what-the-grade-means</c> and that the entry's job
    /// is what no page says, so the entry's second sentence went and the overlap
    /// went with it. <b>Deleted deliberately rather than by test failure</b> — the
    /// converse check below reds on a stale record, so this would have failed
    /// loudly, and being told by a guard is not the same as having decided.</para>
    /// </summary>
    private static readonly SharedWithTheGlossary[] DeliberatelyShared =
    [
        new("tumors/atrt", "glossary/nec",
            ["ask your team what it means for your"],
            "COINCIDENCE, not a restatement. The entry closes 'Ask your team what it "
            + "means for your plan'; the page says 'ask your team what it means for your "
            + "child' about an M number on a staging line, and NEC is not mentioned "
            + "anywhere near it — the tooltip fires from the [CROSSWALK] block further "
            + "down the composed page. 'Ask your team what it means for your X' is the "
            + "instruction §12.2 item 7 requires everywhere, which is why Shingles() "
            + "already strips the 'What to ask your team' section; this is the same "
            + "phrase leaking outside that section. Same family as WI-511 keeping "
            + "/seizures/what-to-do's correct 'not automatically bad news'."),
    ];

    /// <summary>Every curated page, as (slug relative to `pages/`, raw file text).</summary>
    // PROMOTED AT WI-581 to `CuratedPage.ReaderPages()`: this filter existed three
    // times in two files, which is §12.8's factor-at-the-second-use rule, and the
    // set a gate is built over is the first thing §12.19 asks of it.
    private static List<(string Slug, string Text)> EveryPage() => CuratedPage.ReaderPages();

    /// <summary>
    /// No page's prose restates a glossary definition that the same page's own
    /// reader can open as a tooltip.
    ///
    /// The reader is the subject, which is why the tooltip has to actually fire:
    /// a page that defines a term inline and writes <c>!%term%</c> shows the
    /// reader ONE sentence, and that is the sanctioned shape (§12.8, WI-509) —
    /// thirteen pages were already relying on it before anything checked.
    /// A page that leaves the tooltip on and then writes the definition out
    /// again shows the same reader the same sentence twice, in two places that
    /// can drift apart, which is what §12.10 is about.
    ///
    /// COMPOSED, so a block that restates a definition is caught on every page
    /// that includes it rather than in a fragment no reader meets alone.
    /// </summary>
    [Fact]
    public void NoPageRestatesAGlossaryDefinitionItsReaderAlsoMeetsAsATooltip()
    {
        // ONE RECORD PER PAIR, asserted before anything reads the list. Two
        // records for the same (page, entry) is the natural mistake when an
        // overlap is re-measured and APPENDED rather than edited, and without
        // this the lookup below throws a bare InvalidOperationException that says
        // nothing about what to do (/review).
        var pairs = DeliberatelyShared.Select(e => (e.Page, e.Entry)).ToList();
        Assert.Equal(pairs.Count, pairs.Distinct().Count());

        var offenders = new List<string>();
        var scanned = 0;
        var tooltipsSeen = 0;
        var exemptionsUsed = new List<SharedWithTheGlossary>();

        foreach (var (slug, text) in EveryPage())
        {
            scanned++;
            tooltipsSeen += CuratedPage.GlossaryTooltipsFiringOn(
                CuratedPage.Rendered(slug, text)).Count;

            foreach (var (entry, shared) in CuratedPage.GlossaryRestatements(slug, text))
            {
                var recorded = DeliberatelyShared.SingleOrDefault(
                    e => e.Page == slug && e.Entry == entry);

                if (recorded is not null)
                {
                    // USED means the recorded WINDOWS are the ones found, not
                    // merely that this pair collides somehow.
                    exemptionsUsed.Add(recorded);

                    if (recorded.Shared.OrderBy(s => s, StringComparer.Ordinal)
                        .SequenceEqual(shared.OrderBy(s => s, StringComparer.Ordinal),
                            StringComparer.Ordinal))
                    {
                        continue;
                    }

                    offenders.Add($"{slug} <-> {entry}: the overlap is not the one "
                        + "recorded, so the reason beside it is about text that has moved. "
                        + $"found [{string.Join(" | ", shared)}], "
                        + $"recorded [{string.Join(" | ", recorded.Shared)}]");
                    continue;
                }

                offenders.Add($"{slug} <-> {entry} ({shared.Count}): "
                    + string.Join(" | ", shared.Take(3)));
            }
        }

        // THE POSITIVE HALF, and it is not decoration. §12.18: a property guard
        // that has never been seen to fail has not been shown to work — and this
        // one is three mechanisms deep (the real composer, the real marker, the
        // real renderer), every one of which could go quiet without a word.
        //
        // The page floor is COMPUTED, not a round number near the real one: 50 was
        // a four-page margin against an actual 55, which is the thinnest canary in
        // this file (/review).
        var pagesOnDisk = Directory
            .GetFiles(Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web",
                "Content", "pages"), "*.md", SearchOption.AllDirectories).Length;
        Assert.Equal(pagesOnDisk, scanned);
        Assert.True(tooltipsSeen > 100,
            $"only {tooltipsSeen} tooltips fired across the whole corpus, so this scan is "
            + "looking at pages where the glossary reaches nobody and cannot be evidence "
            + "of anything");

        // OFFENDERS BEFORE STALENESS, because the other order tells the author to do
        // the wrong thing (/review). A pair whose overlap has MOVED is both an
        // offender and — on the old ordering — the first thing reported, as "stale,
        // delete it", which is false and sends them away from the finding they never
        // get to read.
        Assert.True(offenders.Count == 0,
            "a page states a glossary definition in its own prose AND leaves that "
            + "definition's tooltip firing on the same page, so the reader meets it twice "
            + "and the two can drift apart (§12.26). Either the page owns the definition "
            + "and suppresses the tooltip with !%term%, or the prose stops restating it:\n  "
            + string.Join("\n  ", offenders));

        // THE CONVERSE, computed from the corpus rather than from the list — the
        // shape WI-578 left standing for DescriptionsCleanDirectories, for the
        // same reason. A list that is only ever read as "skip these" cannot tell
        // you that one of them stopped being true: fix the prose on one of these
        // two pages and the record becomes a lie that silently exempts nothing.
        var stale = DeliberatelyShared.Except(exemptionsUsed).ToList();
        Assert.True(stale.Count == 0,
            "a DeliberatelyShared record no longer matches any collision, so it is stale "
            + "and should be deleted rather than left to exempt something later:\n  "
            + string.Join("\n  ", stale.Select(s => $"{s.Page} <-> {s.Entry}")));
    }

    // ------------------------------------------------------------------------ §12.29
    //
    // SUPERSEDED AND DELETED AT WI-581: `TheEntriesWhoseTooltipThisItemSuppressed-
    // EverywhereAreStillReachable`. It pinned the TWO entries WI-576 took from one
    // firing page to none, with the sentence that made each owning page the owner —
    // deliberately not a gate over the whole corpus, because "pinning all 26 would
    // mean inventing twenty-four reasons this item cannot source".
    //
    // WI-581 is the item that sourced them, so the narrow test is gone rather than
    // left beside the wider one saying the same thing about two of 26 (§12.26's own
    // fourth acceptance criterion, and §12.28's worked precedent for three page-local
    // copies). Its home is `GlossaryOnlyEntriesTests`, and §12.28's rule is that a
    // supersession is a merge whose diff is the ASSERTION LIST: all five assertions
    // moved, widened from 2 entries to 26 and from 2 hand-written needles to 30
    // derived ones. That file's class comment carries the item-by-item diff.
    //
    // WI-567's ruling is the one both files rest on and neither re-derives: the entry
    // is KEPT, because "the suppression is a per-page decision that can be reversed by
    // an edit, while a missing entry is a gap every future page inherits".

    /// <summary>
    /// <see cref="NoPageRestatesAGlossaryDefinitionItsReaderAlsoMeetsAsATooltip"/>,
    /// shown failing and shown discriminating — on text built here rather than on
    /// the corpus, so it keeps proving this after the corpus is clean.
    ///
    /// <para>Named rather than called "the guard above": WI-581 deleted the test
    /// that used to sit between the two, so a relative pointer now points at a
    /// comment.</para>
    ///
    /// Four probes, ONE EDIT APART each, because a probe that changes two things
    /// cannot attribute what it measures to either:
    /// <list type="number">
    /// <item>the definition copied verbatim into the page, tooltip left on — MUST fire;</item>
    /// <item>the same page with <c>!%term%</c> added and nothing else — MUST NOT fire,
    ///   which is the legitimate shape proven rather than asserted;</item>
    /// <item>the definition paraphrased, tooltip left on — MUST NOT fire, or a zero
    ///   anywhere above would mean nothing;</item>
    /// <item>the term never said at all, definition copied in — MUST NOT fire, because
    ///   with no occurrence there is no tooltip and so nothing is met twice.</item>
    /// </list>
    /// </summary>
    [Fact]
    public void TheGlossaryRestatementGuardFiresAndCanTellARestatementFromAParaphrase()
    {
        // A real entry, so the definition under test is the one the site ships.
        var frozen = LoadAll().Single(t => t.Slug == "frozen-section");

        static string PageSaying(string prose) =>
            "---\ntitle: Probe\ndescription: A probe page.\n---\n\n## A heading\n\n"
            + prose + "\n";

        var verbatim = PageSaying(
            "A frozen section is one step in the operation. " + frozen.Definition);
        var suppressed = PageSaying(
            "A frozen section is one step in the operation. " + frozen.Definition
            + " !%frozen section%");
        var paraphrased = PageSaying(
            "A frozen section is one step in the operation. While the operation is still "
            + "going on, the lab takes a fast look at a sliver of tissue and phones the "
            + "surgeon back with a first impression that is not the final word.");
        var neverSaysIt = PageSaying(
            "This page is about something else entirely. " + frozen.Definition);

        var hits = CuratedPage.GlossaryRestatements("probe", verbatim);
        Assert.Contains("glossary/frozen-section", hits.Select(h => h.EntrySlug));

        Assert.DoesNotContain("glossary/frozen-section",
            CuratedPage.GlossaryRestatements("probe", suppressed).Select(h => h.EntrySlug));

        Assert.DoesNotContain("glossary/frozen-section",
            CuratedPage.GlossaryRestatements("probe", paraphrased).Select(h => h.EntrySlug));

        Assert.DoesNotContain("glossary/frozen-section",
            CuratedPage.GlossaryRestatements("probe", neverSaysIt).Select(h => h.EntrySlug));

        // AND THE MECHANISM BENEATH IT, separately, because all four probes above
        // would also pass if the tooltip never fired on any of them — which is
        // exactly how a three-deep guard goes quiet.
        Assert.Contains("frozen-section",
            CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered("probe", verbatim)));
        Assert.DoesNotContain("frozen-section",
            CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered("probe", suppressed)));

        // THE PARAPHRASE PROBE NEEDS IT TOO, and /review found why: it is the one
        // probe whose silence has two possible causes. "The paraphrase does not
        // collide" and "the tooltip never fired on that page at all" look
        // identical from the outside. It fires today because the lead-in sentence
        // says the term — which is an accident of that sentence, not a property
        // anyone asserted, so it is asserted now.
        Assert.Contains("frozen-section",
            CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered("probe", paraphrased)));

        // `neverSaysIt` needs none: the definition is copied in VERBATIM there, so
        // the only thing that can keep it quiet is the tooltip not firing, which is
        // the very thing that probe is about.
        Assert.DoesNotContain("frozen-section",
            CuratedPage.GlossaryTooltipsFiringOn(CuratedPage.Rendered("probe", neverSaysIt)));
    }
}
