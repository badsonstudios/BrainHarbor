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
}
