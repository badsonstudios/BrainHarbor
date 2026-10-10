using System.Net;
using System.Reflection;
using BrainHarbor.ContentCheck;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using YamlDotNet.Serialization;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-583: a front-matter list key with nothing under it — <c>sources:</c> and
/// then the closing <c>---</c>, which is exactly what a half-typed entry looks
/// like.
///
/// <para><b>Why this is not merely a worse error message.</b> YamlDotNet assigns
/// NULL over a <c>List&lt;&gt;</c> property initializer, and the resulting
/// <c>NullReferenceException</c> is a DIFFERENT FAILURE MODE in three places that
/// each catch a different exception type:</para>
/// <list type="bullet">
///   <item><description><c>ContentChecker.CheckPage</c> catches only
///     <c>FormatException</c>, so the CI gate CRASHES instead of failing the page
///     by name — an unexplained red build rather than a content error.</description></item>
///   <item><description><c>ContentStore.GetPage</c> catches only
///     <c>IOException</c>, so a reader gets a 500 ON A MEDICAL PAGE.</description></item>
///   <item><description><c>ContentStore.SearchPages</c> catches only
///     <c>FormatException</c>, so ONE dangling key takes site search down for
///     every page on the site, not just the broken one.</description></item>
/// </list>
///
/// <para><b>What each test here pins, and why "it did not crash" is not
/// enough.</b> WI-561 recorded that two checks in sequence let the second cover
/// for the first, so these assert the OUTCOME: the finding's message and the page
/// it is attributed to, the search result COUNT against a baseline measured in
/// the same run, and — in <see cref="DanglingFrontMatterKeyRenderTests"/> — a
/// served page's status code. A test that only caught the absence of an exception
/// would stay green if the fix made the page disappear instead.</para>
///
/// <para><b>Both line endings, every case.</b> <c>core.autocrlf=true</c> here and
/// CI is Linux/LF, so a front-matter test written with one ending only proves the
/// corpus its author happens to have. Every theory below carries both.</para>
/// </summary>
public sealed class DanglingFrontMatterKeyTests
{
    private static readonly DateOnly Today = new(2026, 10, 10);

    /// <summary>
    /// The three keys this item closes, plus <c>images</c> — which WI-561 closed
    /// one item earlier, and which is in this list so these tests pin the SHAPE
    /// of the hole rather than three instances of it.
    /// </summary>
    internal static readonly string[] ListKeys = ["sources", "tags", "disclaimers", "images"];

    /// <summary>LF and CRLF, named, so a failing case says which one it was.</summary>
    internal static readonly (string Name, string Ending)[] LineEndings =
        [("lf", "\n"), ("crlf", "\r\n")];

    /// <summary>
    /// Every (key, line ending) pair. A fifth list key added to the schema is
    /// NOT covered by this array and is not meant to be — it is caught by
    /// <see cref="EveryListKeyOnTheSchemaIsCoveredByTheseTests"/>, which reads
    /// the TYPE. A theory that hand-lists its cases cannot notice a new one.
    /// </summary>
    public static TheoryData<string, string> KeysByLineEnding
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var key in ListKeys)
            {
                foreach (var (_, ending) in LineEndings)
                {
                    data.Add(key, ending);
                }
            }
            return data;
        }
    }

    private const string PlainBody =
        "This page is about the brain. It says a few short things. "
        + "Your care team can tell you more.";

    // Deliberately above the 6.0 limit — the Fail arm below needs a REAL content
    // defect to prove the gate still reports one with a dangling key present.
    private const string UnreadableBody =
        "Notwithstanding contemporary advancements in neuro-oncological "
        + "therapeutics, the prognostic implications of isocitrate dehydrogenase "
        + "mutations necessitate comprehensive multidisciplinary evaluation "
        + "incorporating histopathological and molecular characterization.";

    /// <summary>The test pages' title, and the needle the render tests read.</summary>
    internal const string Title = "A half-typed key";

    internal static string PageWith(string danglingKey, string lineEnding, string body = PlainBody) =>
        string.Join(lineEnding,
        [
            "---",
            $"title: {Title}",
            "description: What a half typed key does.",
            danglingKey + ":",
            "---",
            "",
            body,
            "",
        ]);

    private static IEnumerable<PropertyInfo> ListProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<YamlMemberAttribute>() is not null
                        && p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>));

    private static IEnumerable<PropertyInfo> ListProperties() =>
        ListProperties(typeof(ContentFrontMatter));

    private static string Alias(PropertyInfo property) =>
        property.GetCustomAttribute<YamlMemberAttribute>()?.Alias
        ?? throw new InvalidOperationException(
            $"{property.DeclaringType?.Name}.{property.Name} has no YamlMember alias "
            + "— front matter maps from snake_case");

    // ---------- the invariant ----------

    [Theory]
    [MemberData(nameof(KeysByLineEnding))]
    public void AHalfTypedListKeyNeverReachesAConsumerAsNull(string key, string lineEnding)
    {
        var page = ContentStore.Parse(PageWith(key, lineEnding), $"dangling/{key}");

        // EVERY list property, not just the dangling one: the deserializer nulls
        // the key it was given, and a test that only looked at that key would
        // have been green on all three holes this item found.
        foreach (var property in ListProperties())
        {
            Assert.NotNull(property.GetValue(page.FrontMatter));
        }
    }

    [Fact]
    public void EveryListKeyOnThePageSchemaIsCoveredByTheseTests()
    {
        // The theories above hand-list their keys, so this ties that list to the
        // type: a new List<> property on ContentFrontMatter fails here until its
        // alias is in `ListKeys`, and the per-failure-mode theories then cover
        // it without anyone adding a case.
        Assert.Equal(
            ListKeys.OrderBy(k => k, StringComparer.Ordinal),
            ListProperties().Select(Alias).OrderBy(a => a, StringComparer.Ordinal));
    }

    /// <summary>
    /// Every type that carries a <c>List&lt;&gt;</c> the YAML deserializer can
    /// write to, from <see cref="ContentSource"/> up to <c>taxonomy.yml</c>'s
    /// root. One expected entry per property, so a tenth one has to be ADDED
    /// here rather than slipping through a count.
    /// </summary>
    public static readonly string[] EveryYamlListProperty =
    [
        "ContentBlockFrontMatter.Sources (sources)",
        "ContentFrontMatter.Disclaimers (disclaimers)",
        "ContentFrontMatter.Images (images)",
        "ContentFrontMatter.Sources (sources)",
        "ContentFrontMatter.Tags (tags)",
        "GlossaryFrontMatter.Also (also)",
        "GlossaryFrontMatter.Sources (sources)",
        "TaxonomyFile.TumorTypes (tumor_types)",
        "TumorType.Also (also)",
    ];

    [Fact]
    public void EveryYamlListPropertyOnEverySchemaRefusesNull()
    {
        // THE DURABLE PART OF THIS ITEM, AND IT TOOK A /review ROUND TO GET
        // RIGHT. The first version read `typeof(ContentFrontMatter)` — which
        // reproduces WI-561's mistake exactly one layer up: a guard written
        // where the crash happened teaches nothing to the TYPES that have not
        // crashed yet. There were five more, and two of them are worse than
        // anything the item was filed about: a dangling `also:` in a glossary
        // file is an ArgumentNullException during the parse of ALL 55 pages,
        // and either list key in taxonomy.yml is walked in TaxonomyStore's
        // CONSTRUCTOR, so the site does not boot.
        //
        // This asserts the PROPERTY, not a list of names: it hands each one the
        // null YamlDotNet hands a dangling key and reads it back.
        var found = new List<string>();
        var offenders = new List<string>();

        foreach (var type in typeof(ContentStore).Assembly.GetTypes()
                     .OrderBy(t => t.Name, StringComparer.Ordinal))
        {
            foreach (var property in ListProperties(type).OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                var where = $"{type.Name}.{property.Name} ({Alias(property)})";
                found.Add(where);

                // A parameterless constructor is not an assumption — YamlDotNet
                // needs one to deserialize this type at all, so a type reaching
                // here without one is a defect in its own right and throwing is
                // the right answer.
                var instance = Activator.CreateInstance(type, nonPublic: true)
                    ?? throw new InvalidOperationException($"{type.Name} has no parameterless constructor");

                property.SetValue(instance, null);
                if (property.GetValue(instance) is null)
                {
                    offenders.Add(where);
                }
            }
        }

        // THE SWEEP'S OWN LIVENESS FIRST. An exact set, not a count: a renamed
        // YamlMember or a type moved to another assembly empties this walk, and
        // an empty walk reports zero offenders and passes.
        Assert.Equal(EveryYamlListProperty, found);
        Assert.Empty(offenders);
    }

    // ---------- failure mode 1: the CI gate crashes instead of naming the page ----------

    [Theory]
    [MemberData(nameof(KeysByLineEnding))]
    public void ContentCheckReportsThePageByNameInsteadOfCrashing(string key, string lineEnding)
    {
        var where = $"dangling/{key}.md";

        var findings = ContentChecker.CheckPage(PageWith(key, lineEnding), where, Today, null);

        // IT RETURNED, and /review corrected the reason this comment used to
        // give. The proof is NOT that the disclaimers walk is the last thing
        // CheckPage does — it is that the walk is UNGUARDED: no `try` around it
        // swallows anything, so a normal return implies it executed. Which also
        // says what would break the proof: wrap that walk in a `catch` and this
        // test stays green while the walk is dead. `AnUnknownDisclaimerFlag...`
        // below is the positive control that survives that refactor, because it
        // pins what the walk EMITS.
        //
        // `NotEmpty` is near-vacuous on its own — `GradeFinding` is added
        // unconditionally for any input that gets past Parse, including the
        // `catch (FormatException)` path. It is kept to catch a `return []`
        // refactor and claims nothing more than that.
        Assert.NotEmpty(findings);

        // BY NAME. A crash is attributed to nothing, and the whole value of the
        // Fail/Warn path is that the build says which file to open. The
        // description findings carry a " [description]" suffix, so this is a
        // prefix match on purpose.
        Assert.All(findings, finding => Assert.StartsWith(where, finding.File, StringComparison.Ordinal));

        // A key with nothing under it is BENIGN — the author typed the key and
        // stopped, and a page with no pictures/tags/sources is what they have.
        // Scoped to THIS page's own findings (/review): the description findings
        // carry the marker suffix and are gated by a growing directory list, so
        // an unscoped claim over every verdict CheckPage can reach would go red
        // one day for a reason with nothing to do with front matter.
        Assert.DoesNotContain(findings, finding =>
            finding.Level == FindingLevel.Fail && finding.File == where);
    }

    [Theory]
    [MemberData(nameof(KeysByLineEnding))]
    public void AnUnknownDisclaimerFlagIsStillReportedWithADanglingKeyOnThePage(string key, string lineEnding)
    {
        // THE POSITIVE CONTROL FOR THE DISCLAIMERS WALK (/review). Every test
        // above proves the walk SURVIVED; this one proves it still does its job,
        // by what it emits. A `catch` added around that walk tomorrow would
        // leave all of those green and this one red, which is the right way
        // round.
        //
        // `disclaimers` itself is excluded by construction — a key cannot be
        // both dangling and carrying a bad flag — so that case asserts the
        // walk's OTHER output: with no flags declared there is nothing to
        // reject, and the complaint must name no flag at all.
        var where = $"dangling/{key}.md";
        var page = key == "disclaimers"
            ? PageWith(key, lineEnding)
            : PageWith(key, lineEnding).Replace(
                "---" + lineEnding + lineEnding,
                $"disclaimers:{lineEnding}  - notaflag{lineEnding}---{lineEnding}{lineEnding}",
                StringComparison.Ordinal);

        var findings = ContentChecker.CheckPage(page, where, Today, null);

        if (key == "disclaimers")
        {
            Assert.DoesNotContain(findings, f =>
                f.Message.Contains("disclaimer flag", StringComparison.Ordinal));
        }
        else
        {
            var flag = Assert.Single(findings, f =>
                f.Message.Contains("disclaimer flag", StringComparison.Ordinal));
            Assert.Equal(FindingLevel.Fail, flag.Level);
            Assert.Equal(where, flag.File);
            Assert.Contains("'notaflag'", flag.Message);
        }
    }

    [Theory]
    [MemberData(nameof(KeysByLineEnding))]
    public void ADanglingKeyStillLetsContentCheckFailARealDefectByName(string key, string lineEnding)
    {
        var where = $"dangling/{key}.md";

        var findings = ContentChecker.CheckPage(
            PageWith(key, lineEnding, UnreadableBody), where, Today, null);

        // The acceptance criterion in its literal form: a Fail, naming the page,
        // naming the defect — on a page that also carries the dangling key. The
        // reading-level gate is the whole reason ContentCheck exists, and with a
        // dangling `sources:` it never graded a word of this page.
        var failure = Assert.Single(findings, f =>
            f.Level == FindingLevel.Fail && f.File == where);
        Assert.Contains("is above the", failure.Message);
        Assert.Contains("simplify the language", failure.Message);
    }

    [Theory]
    [MemberData(nameof(KeysByLineEnding))]
    public void ADanglingKeyLeavesThePagesOwnSourceWarningIntact(string key, string lineEnding)
    {
        var findings = ContentChecker.CheckPage(
            PageWith(key, lineEnding), $"dangling/{key}.md", Today, null);

        // Every one of these pages declares no usable sources, so every one of
        // them earns the §2 warning. Asserting the MESSAGE rather than a count
        // is WI-561's lesson: the warning that matters is the one the crash
        // replaced, and a test that counted findings would pass on any other.
        Assert.Contains(findings, finding =>
            finding.Level == FindingLevel.Warn
            && finding.Message.StartsWith("no sources in front matter", StringComparison.Ordinal));
    }

    // ---------- the key whose failure mode is still LATENT ----------

    [Fact]
    public void NothingOnTheSiteReadsAPagesTagsYet()
    {
        // MEASURED, AND IT CHANGES WHAT THE `tags` CASE ABOVE CAN CLAIM. The
        // backlog entry said all three keys had the identical hole; two of them
        // do, with a live deref each (ContentChecker's disclaimers walk, and
        // ContentStore.Parse's `sources` spread). `tags` HAS NO CURATED-PAGE
        // CONSUMER AT ALL — the only `.Tags` on the site belongs to a feed
        // card, a different type — so a dangling `tags:` crashes nothing today
        // and no honest test can pin a failure mode for it.
        //
        // That is exactly why it is still fixed, and why this test exists
        // instead of a fabricated one: the type-level guarantee is the ONLY
        // thing standing between `tags:` and whoever writes the first consumer.
        // When this goes red, that consumer has arrived — give it a real
        // failure-mode assertion next to the other two before deleting this.
        //
        // A FLOOR, NOT A FENCE, and the break harness proved which: the first
        // version matched `FrontMatter.Tags` exactly, and the mutation that
        // plants a consumer wrote `frontMatter.Tags` — the LOCAL, which is how
        // ContentStore.Parse already names it — and walked straight through.
        // Case-insensitive now, so both the property-access and the local form
        // are caught. A consumer that copies the front matter into a
        // differently-named local still gets through; the invariant above is
        // what actually holds, and this only asks to be told when the stakes
        // change.
        // `bin`/`obj` are excluded (/review): a stale generated Razor .g.cs can
        // turn this red after the source is already clean, and it would name a
        // path that does not exist in git.
        var generated = new[]
        {
            $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
            $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
        };
        var hits = new List<string>();
        foreach (var directory in (string[])["src", "tools"])
        {
            var root = Path.Combine(CuratedPage.RepoRoot(), directory);
            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                         .Where(f => f.EndsWith(".cs", StringComparison.Ordinal)
                                     || f.EndsWith(".cshtml", StringComparison.Ordinal))
                         .Where(f => !generated.Any(g => f.Contains(g, StringComparison.Ordinal))))
            {
                if (File.ReadAllText(file)
                    .Contains("rontMatter.Tags", StringComparison.OrdinalIgnoreCase))
                {
                    hits.Add(Path.GetRelativePath(CuratedPage.RepoRoot(), file));
                }
            }
        }

        Assert.Empty(hits);
    }

    // ---------- failure mode 3: one dangling key takes site search down ----------

    [Fact]
    public void SiteSearchStillAnswersForTheWholeCorpusWhenAKeyDangles()
    {
        using var corpus = new CorpusCopy();
        var store = corpus.Store();

        // MEASURED IN THIS RUN, NOT TRANSCRIBED (§12.28): a baseline taken at
        // one moment and used at another is correct when taken and wrong when
        // used. The floor is only here to stop the comparison being between two
        // zeroes.
        var before = store.SearchPages("brain", 500);
        Assert.True(before.Count >= 40,
            $"the search baseline is {before.Count} pages — too few for the comparison to mean anything");

        var added = corpus.WriteDanglingPages();

        // `added` IS THE HELPER'S OWN RETURN VALUE, so it is pinned (/review):
        // a refactor that wrote zero pages and returned zero would degrade the
        // comparison below to `before == after`, turn the `Except` into a no-op
        // and leave this green with no broken page in the corpus at all.
        Assert.Equal(ListKeys.Length * LineEndings.Length, added);

        var after = store.SearchPages("brain", 500);

        // Before the fix this did not return a SHORTER list: the
        // NullReferenceException escaped SearchPages — which catches only
        // FormatException — so /search returned a 500 for every query on the
        // site, over one half-typed key on one page.
        Assert.Equal(before.Count + added, after.Count);

        // And the intact pages are still THERE, not merely still counted.
        Assert.Equal(
            before.Select(p => p.UrlPath).OrderBy(p => p, StringComparer.Ordinal),
            after.Select(p => p.UrlPath).Except(CorpusCopy.DanglingUrlPaths())
                .OrderBy(p => p, StringComparer.Ordinal));

        // A NAMED PAGE, not only a count (/review). A floor can say "lots of
        // things came back"; it cannot say the right things did. Only five page
        // titles contain "glioma" and no sibling URL contains this one as a
        // substring, so it discriminates.
        Assert.Contains("/tumors/glioma", after.Select(p => p.UrlPath));
    }

    // ---------- the two siblings /review found, which are worse than all of the above ----------

    [Theory]
    [MemberData(nameof(LineEndingsOnly))]
    public void AHalfTypedGlossaryKeyDoesNotTakeAllFiftyFivePagesDownWithIt(string lineEnding)
    {
        // THE WORST FAILURE MODE IN THIS ITEM, and it is not on a page at all.
        // `GlossaryFrontMatter.Also` reaches `GlossaryTooltips.BuildMatchers` as
        // `names.AddRange(term.Aliases)` — an ArgumentNullException, which is
        // neither of the two exception types anything upstream catches — during
        // the GlossaryMarker pass of EVERY curated page. One half-typed key in
        // one glossary file, 55 pages of 500s, and site search gone with them.
        using var corpus = new CorpusCopy();
        var store = corpus.Store();

        var before = store.SearchPages("brain", 500).Count;
        Assert.True(before >= 40, $"the baseline is {before} pages — too few to mean anything");

        corpus.WriteGlossaryEntry("dangling-also", "also", lineEnding);

        // `sources` is written too, and it is NOT what this test is pinning:
        // GlossaryTerm declares `Sources ?? []`, so that null was already
        // absorbed one hop downstream — /review reported a 500 on the
        // /glossary index for it and that is not reproducible. It is here as a
        // second half-typed file in the same corpus, nothing more; the sweep
        // above is what covers its property.
        corpus.WriteGlossaryEntry("dangling-sources", "sources", lineEnding);

        // A real page, parsed through the marker that walks every term.
        var page = store.GetPage("tumors/glioma");
        Assert.NotNull(page);
        Assert.Contains("glioma", page.Html, StringComparison.OrdinalIgnoreCase);

        // And the whole corpus, because the marker runs once per page.
        Assert.Equal(before, store.SearchPages("brain", 500).Count);
    }

    [Theory]
    [MemberData(nameof(LineEndingsOnly))]
    public void AHalfTypedTaxonomyKeyFailsByNameInsteadOfStoppingTheSiteBooting(string lineEnding)
    {
        // THE OTHER SIBLING: both of `taxonomy.yml`'s list keys are walked in
        // TaxonomyStore's CONSTRUCTOR, which runs at DI composition. A half-typed
        // key here never reached a page — it stopped the site starting.
        var entry = string.Join(lineEnding,
            ["tumor_types:", "  - slug: glioma", "    label: Glioma", "    also:", ""]);

        // A dangling `also:` is benign: the type has no aliases, which is what
        // the author wrote.
        var store = new TaxonomyStore(entry);
        Assert.Single(store.TumorTypes);
        Assert.Empty(store.TumorTypes[0].Also);

        // A dangling `tumor_types:` is NOT benign — a taxonomy with no types is
        // the gate that stops an invented tumor type reaching a patient, switched
        // off. It has to fail, and fail SAYING SO rather than as an NRE from
        // inside a constructor.
        var exception = Assert.Throws<FormatException>(
            () => new TaxonomyStore("tumor_types:" + lineEnding));
        Assert.Contains("defines no tumor types", exception.Message);
    }

    public static TheoryData<string> LineEndingsOnly
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var (_, ending) in LineEndings)
            {
                data.Add(ending);
            }
            return data;
        }
    }

    /// <summary>
    /// A throwaway copy of the shipped corpus — pages, blocks and glossary — so
    /// a test can add a broken page to a REAL 55-page corpus rather than to a
    /// two-page fixture. The acceptance criterion is about the OTHER 54 pages,
    /// and a fixture small enough to be convenient cannot say anything about
    /// them.
    /// </summary>
    internal sealed class CorpusCopy : IDisposable
    {
        private readonly string _root;

        public CorpusCopy()
        {
            _root = Path.Combine(Path.GetTempPath(), "bh-wi583-" + Guid.NewGuid().ToString("N"));
            var source = Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content");
            foreach (var directory in (string[])["pages", "blocks", "glossary"])
            {
                Copy(Path.Combine(source, directory), Path.Combine(_root, directory));
            }
        }

        public string PagesRoot => Path.Combine(_root, "pages");

        public void Dispose() => Directory.Delete(_root, recursive: true);

        /// <summary>One slug per key per line ending, in the order they are written.</summary>
        public static IEnumerable<string> DanglingSlugs() =>
            from key in ListKeys
            from ending in LineEndings
            select $"dangling/{key}-{ending.Name}";

        public static IEnumerable<string> DanglingUrlPaths() =>
            DanglingSlugs().Select(slug => "/" + slug);

        /// <summary>
        /// One glossary entry with a half-typed list key. A glossary file is not
        /// a page, which is exactly why it is worth writing one: its front
        /// matter is read during the parse of EVERY page.
        /// </summary>
        public void WriteGlossaryEntry(string slug, string danglingKey, string lineEnding) =>
            File.WriteAllText(
                Path.Combine(_root, "glossary", slug + ".md"),
                string.Join(lineEnding,
                [
                    "---",
                    $"term: {slug}",
                    danglingKey + ":",
                    "---",
                    "",
                    "A word this site uses, written out in plain language for a reader.",
                    "",
                ]));

        /// <summary>Writes one half-typed page per key per line ending; returns how many.</summary>
        public int WriteDanglingPages()
        {
            Directory.CreateDirectory(Path.Combine(PagesRoot, "dangling"));
            var written = 0;
            foreach (var key in ListKeys)
            {
                foreach (var (name, ending) in LineEndings)
                {
                    File.WriteAllText(
                        Path.Combine(PagesRoot, "dangling", $"{key}-{name}.md"),
                        PageWith(key, ending));
                    written++;
                }
            }
            return written;
        }

        public ContentStore Store()
        {
            var environment = new StubEnvironment();
            var configuration = Configuration();
            return new ContentStore(
                environment,
                configuration,
                new GlossaryStore(environment, configuration),
                new ContentBlockStore(environment, configuration));
        }

        /// <summary>The same three roots the web host needs, so both surfaces read one corpus.</summary>
        public Dictionary<string, string?> Settings() => new()
        {
            ["Content:Root"] = PagesRoot,
            ["Content:BlocksRoot"] = Path.Combine(_root, "blocks"),
            ["Glossary:Root"] = Path.Combine(_root, "glossary"),
        };

        private IConfiguration Configuration() =>
            new ConfigurationBuilder().AddInMemoryCollection(Settings()).Build();

        private static void Copy(string from, string to)
        {
            Directory.CreateDirectory(to);
            foreach (var file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(to, Path.GetRelativePath(from, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }
        }

        private sealed class StubEnvironment : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "BrainHarbor.Tests";
            public string WebRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}

/// <summary>
/// WI-583, failure mode 2: the one a reader meets. <c>ContentStore.GetPage</c>
/// catches only <c>IOException</c>, and <c>ContentPage.cshtml</c> reads
/// <c>FrontMatter.Disclaimers.Count</c> — so a dangling <c>disclaimers:</c>
/// threw inside the VIEW, after <c>GetPage</c> had already returned, and the
/// reader of a medical page got a 500.
///
/// <para>Served through the real host against a copy of the real corpus, because
/// that is the only place the Razor deref happens: every test in the sibling
/// class above would be green with <c>ContentPage.cshtml</c> still crashing.</para>
/// </summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class DanglingFrontMatterKeyRenderTests
    : IClassFixture<DanglingFrontMatterKeyRenderTests.Host>
{
    /// <summary>
    /// ONE corpus copy and ONE derived host for the whole class, not one per
    /// test (/review). xUnit builds a test class per test method, so the
    /// straightforward version copied 169 files and stood up a web host NINE
    /// times. <c>TestDatabase</c> records what that currency buys: derived
    /// <c>WebApplicationFactory</c> instances accumulate across a run and their
    /// idle connections have already hit Postgres's 100-client ceiling once in
    /// this repo, surfacing as a flaky E2E failure. A class fixture is built
    /// once and disposed.
    /// </summary>
    public sealed class Host : IDisposable
    {
        private readonly WebApplicationFactory<Program> _root = new();

        private readonly DanglingFrontMatterKeyTests.CorpusCopy _corpus = new();

        public WebApplicationFactory<Program> Factory { get; }

        public Host(DatabaseFixture database)
        {
            _corpus.WriteDanglingPages();
            Factory = _root.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:BrainHarbor", database.ConnectionString);
                foreach (var (name, value) in _corpus.Settings())
                {
                    builder.UseSetting(name, value!);
                }
            });
        }

        public void Dispose()
        {
            Factory.Dispose();
            _root.Dispose();
            _corpus.Dispose();
        }
    }

    private readonly WebApplicationFactory<Program> _factory;

    public DanglingFrontMatterKeyRenderTests(Host host) => _factory = host.Factory;

    public static TheoryData<string> DanglingPageUrls()
    {
        var data = new TheoryData<string>();
        foreach (var url in DanglingFrontMatterKeyTests.CorpusCopy.DanglingUrlPaths())
        {
            data.Add(url);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(DanglingPageUrls))]
    public async Task AHalfTypedKeyServesTheReaderThePageAndNotA500(string url)
    {
        var response = await _factory.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // THE PAGE'S OWN CONTENT, not only the status: a status check alone
        // would pass on an error page that happened to render with a 200.
        //
        // The TITLE and not a body sentence (/review): GlossaryMarker wraps the
        // first occurrence of any glossary term in markup, which would split a
        // body needle the day somebody defines a term in it. 105 entries today
        // and none of them is a bare "brain", but the title is not marked at
        // all, so it cannot be split by anything.
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(DanglingFrontMatterKeyTests.Title, html);
        Assert.DoesNotContain("An error occurred", html);
    }

    [Fact]
    public async Task SearchStillAnswersForTheWholeSiteWithAHalfTypedPageInTheCorpus()
    {
        // The third failure mode, asked of the SERVED endpoint. SearchPages
        // parses every page in the corpus on every query, so the dangling
        // page's exception came back as a 500 on /search?q=anything — site
        // search down for all 55 pages over one key on one of them.
        var client = _factory.CreateClient();

        // NAMED, not counted: the view caps at ten hits, so "brain" renders a
        // top-ten that a page can leave without anything going wrong. Asking
        // for a page by its own word is a hit that cannot drop out.
        var named = await client.GetAsync("/search?q=glioma");
        Assert.Equal(HttpStatusCode.OK, named.StatusCode);
        Assert.Contains("/tumors/glioma", await named.Content.ReadAsStringAsync());

        // And a broad term still reaches the curated set at all — the block
        // that renders page hits, not just a 200 on an empty result.
        var broad = await client.GetAsync("/search?q=brain");
        Assert.Equal(HttpStatusCode.OK, broad.StatusCode);
        Assert.Contains("<h2>Pages</h2>", await broad.Content.ReadAsStringAsync());
    }
}
