using BrainHarbor.ContentCheck;
using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-435: <b>the gate that reports success having read nothing.</b>
///
/// <para><c>dotnet run --project tools/BrainHarbor.ContentCheck -- --nologo</c> printed
/// <c>WARN  --nologo: pages root MISSING — no pages were checked</c>, graded zero of the
/// 55 curated pages, printed <c>ContentCheck passed (232 checks, 0 failures)</c> and
/// exited 0. A clean run is 345 checks, so 113 vanished — every curated page's reading
/// grade, which is this project's one hard requirement. Running the tool from the wrong
/// directory was worse still: <b>3 checks, 0 failures, exit 0.</b></para>
///
/// <para><b>This is §12.37's class — an instrument green because it measured nothing —
/// and the fix is in the same place: the thing that could not tell two situations apart
/// has to be able to.</b> There, a helper taking a <c>string</c> could not tell a page
/// from a section. Here, an <c>int</c> coverage count could not tell <i>this root is not
/// in scope</i> from <i>this root is in scope and I read nothing</i>, and
/// <c>DescriptionCorpusReport</c> read 0 as the first and took its "not our business"
/// exit. Two meanings, one value, and the broken run left through the gap.</para>
///
/// <para><b>Everything here is about the invocation, not about the content</b> — which
/// is why it is its own file. <c>ContentCheckTests</c> asks whether the tool grades a
/// page correctly; these ask whether it graded the pages at all.</para>
/// </summary>
public class ContentCheckCorpusFloorTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);

    private static string Repo => CuratedPage.RepoRoot();

    private static string PagesRoot =>
        Path.Combine(Repo, "src", "BrainHarbor.Web", "Content", "pages");

    private static string GlossaryRoot =>
        Path.Combine(Repo, "src", "BrainHarbor.Web", "Content", "glossary");

    private static string BlocksRoot =>
        Path.Combine(Repo, "src", "BrainHarbor.Web", "Content", "blocks");

    private static string RazorRoot => Path.Combine(Repo, "src", "BrainHarbor.Web", "Pages");

    /// <summary>A temp directory holding one legal one-page corpus. Deleted by the caller.</summary>
    private static string OnePageCorpus()
    {
        var root = Path.Combine(Path.GetTempPath(), "bh-floor-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "one.md"),
            "---\ntitle: One page\ndescription: \"\"\n---\nShort page. It has words in it.\n");
        return root;
    }

    // ---------- the positive control ----------

    /// <summary>
    /// <b>THE POSITIVE CONTROL, and it is not optional.</b> A floor that reds on an
    /// empty walk proves nothing on its own: a floor set one file too high reds on
    /// everything, including the corpus it is supposed to pass, and the first thing that
    /// would do is get somebody to lower it. §12.19's rule — ask which inputs a guard
    /// can fire on — applies to a guard whose whole job is to fire.
    /// </summary>
    [Fact]
    public void TheShippedCorpusClearsItsOwnFloor()
    {
        var findings = ContentChecker.CheckAll(
            PagesRoot, GlossaryRoot, Today, CorpusFloor.Shipped, RazorRoot, BlocksRoot).Findings;

        Assert.DoesNotContain(findings, CorpusFloor.IsFloorFinding);
    }

    /// <summary>
    /// <b>The floor is the corpus, not an old measurement of it.</b>
    ///
    /// <para>Asserted as EQUALITY in both directions, which costs one constant edit per
    /// content item and buys the only thing that keeps a floor honest. A floor left
    /// behind by a growing corpus is pure slack: at 55 with 63 pages on disk, eight
    /// pages could stop being walked with every gate green — which is this item's defect
    /// at one eighth scale. §12.24 made the same argument against a count that locks in
    /// a TOTAL.</para>
    ///
    /// <para>The message says what to do, because the person who sees this red will be
    /// somebody who just added a page and has never heard of a corpus floor — and for a
    /// round it did not: the assertion was a bare record-to-record <c>Assert.Equal</c>
    /// that printed two <c>ToString</c>s and no instruction, under this very sentence
    /// (/review).</para>
    /// </summary>
    [Fact]
    public void TheFloorIsTheShippedCorpusAndNotAnOldMeasurementOfIt()
    {
        var onDisk = new CorpusFloor(
            PagesOnDisk(), GlossaryOnDisk(), ReaderFacingRazorOnDisk().Count, BlocksOnDisk());

        Assert.True(onDisk == CorpusFloor.Shipped,
            $"""
            The corpus on disk is {onDisk} and the floor is {CorpusFloor.Shipped}.

            A FLOOR IS THE CORPUS, NOT AN OLD MEASUREMENT OF IT. If you have just added
            or removed content, re-measure: set CorpusWhenMeasured / GlossaryWhenMeasured
            / ReaderFacingRazorPagesWhenMeasured / BlocksWhenMeasured in
            tools/BrainHarbor.ContentCheck/ContentChecker.cs to the numbers above.

            This test exists because a floor BELOW the corpus is slack: at a floor of 55
            with 63 pages on disk, eight pages could stop being graded with every gate
            green. That is WI-435's defect at one eighth scale.
            """);
    }

    // ---------- the oracles: what is on disk, derived independently of the walk ----------

    private static int PagesOnDisk() =>
        Directory.EnumerateFiles(PagesRoot, "*.md", SearchOption.AllDirectories).Count();

    private static int GlossaryOnDisk() => Directory.EnumerateFiles(GlossaryRoot, "*.md").Count();

    /// <summary>
    /// <b>The same predicate the walk uses, not a bigger one</b> (/review).
    ///
    /// <para>This counted <c>AllDirectories</c> and every <c>*.md</c> for a round, while
    /// <see cref="ContentBlockStore.Load"/> reads the TOP DIRECTORY ONLY and skips any
    /// file whose name is not a block slug — with a docstring calling a <c>README.md</c>
    /// dropped in there "a normal thing to do". So dropping one in made this oracle say
    /// 9, told you to re-measure to 9, and <b>the CLI's blocks floor would then have been
    /// permanently unreachable: red forever, with lowering a floor as the only way
    /// out.</b> An oracle that walks differently from the walk manufactures exactly the
    /// failure <c>WriteFloorAdvice</c> is written to talk people out of.</para>
    /// </summary>
    private static int BlocksOnDisk() =>
        Directory.EnumerateFiles(BlocksRoot, "*.md")
            .Count(f => ContentBlocks.IsBlockFileName(Path.GetFileNameWithoutExtension(f)));

    /// <summary>
    /// The reader-facing Razor pages on disk, by relative path — the oracle this root did
    /// not have (/review). See <see cref="ContentChecker.IsReaderFacing"/>.
    /// </summary>
    private static List<string> ReaderFacingRazorOnDisk() =>
        Directory.EnumerateFiles(RazorRoot, "*.cshtml", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(RazorRoot, f).Replace('\\', '/'))
            .Where(ContentChecker.IsReaderFacing)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

    // ---------- the floor itself ----------

    /// <summary>
    /// <b>A FLOOR, NOT A LOUDER WARNING.</b> The missing-root WARN has existed since
    /// WI-106 and the run it was warning about still exited 0, because the only consumer
    /// of this tool is a CI step reading an exit code. Asserted on the LEVEL, not on the
    /// presence of a finding: the pre-WI-435 run produced a finding too.
    /// </summary>
    [Fact]
    public void AShortWalkIsAFailAndNotAWarning()
    {
        var root = OnePageCorpus();
        try
        {
            var findings = ContentChecker.CheckAll(
                root, null, Today, CorpusFloor.Shipped with { GlossaryTerms = null }).Findings;

            var floors = findings.Where(CorpusFloor.IsFloorFinding).ToList();

            Assert.NotEmpty(floors);
            Assert.All(floors, f => Assert.Equal(FindingLevel.Fail, f.Level));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// <b>ONE PAGE SHORT IS SHORT.</b> 54 of 55 is the case a floor exists to catch that
    /// a missing-root check cannot: the root is there, the walk works, the glob matches,
    /// and one page has quietly stopped being graded. WI-578 recorded the identical
    /// shape from the other side — deleting one page took the corpus from 55 to 54 and
    /// switched a slice gate off with every check green.
    /// </summary>
    [Fact]
    public void OnePageShortOfTheFloorStillReds()
    {
        var floor = CorpusFloor.None with { Pages = 55 };
        var shortfalls = floor.Shortfalls(new CorpusCoverage(54, null, null, 0)).ToList();

        var finding = Assert.Single(shortfalls);
        Assert.Equal(CorpusFloor.FileFor("pages"), finding.File);
        Assert.Equal(FindingLevel.Fail, finding.Level);
    }

    /// <summary>
    /// <b>EVERY ROOT HAS ITS OWN FLOOR, because one total is met by one root.</b>
    ///
    /// <para>§12.37 shipped exactly this mistake and <c>/review</c> caught it: a single
    /// floor of 55 across a 168-file sweep was satisfied by the pages alone, so a
    /// renamed <c>Content/blocks</c> left the guard green having read no block. Each
    /// root is emptied here with the other three intact, and each has to red on its own
    /// line — which a combined total cannot do.</para>
    ///
    /// <para>The blocks root is the one that matters most and is least obvious: blocks
    /// have no reading level of their own, because a page is graded COMPOSED. Lose them
    /// and 55 pages grade clean on prose they do not actually show a reader.</para>
    /// </summary>
    /// <remarks>
    /// Driven off <see cref="CorpusFloor.Roots"/> rather than four <c>InlineData</c>
    /// cases, so a fifth root is covered the moment it is added to the table — the
    /// property <see cref="TheRootTableCoversEveryRootCoverageCanReport"/> then forces
    /// into existence. And the full coverage is built FROM the floor's own members, not
    /// by retyping 55/105/20/8: a legitimate re-measure to 56 pages would otherwise make
    /// "full" coverage short of its own floor and fail this test about the wrong thing
    /// (/review).
    /// </remarks>
    [Fact]
    public void EachRootRedsOnItsOwnWithTheOthersIntact()
    {
        var shipped = CorpusFloor.Shipped;
        var full = new CorpusCoverage(
            shipped.Pages!.Value, shipped.GlossaryTerms, shipped.ReaderFacingRazorPages,
            shipped.Blocks!.Value);

        Assert.Empty(shipped.Shortfalls(full));

        foreach (var (name, _, _, _) in CorpusFloor.Roots)
        {
            // Emptied through the table's own accessor, so the case cannot name one root
            // and empty another.
            var coverage = Empty(full, name);

            var finding = Assert.Single(shipped.Shortfalls(coverage));
            Assert.Equal(CorpusFloor.FileFor(name), finding.File);
            Assert.Equal(FindingLevel.Fail, finding.Level);
        }
    }

    private static CorpusCoverage Empty(CorpusCoverage full, string root) => root switch
    {
        "pages" => full with { Pages = 0 },
        "glossary" => full with { GlossaryTerms = 0 },
        "razor" => full with { ReaderFacingRazorPages = 0 },
        "blocks" => full with { Blocks = 0 },
        _ => throw new ArgumentOutOfRangeException(nameof(root), root, "unknown root"),
    };

    /// <summary>
    /// <b>THE ROOT TABLE COVERS EVERY ROOT THE COVERAGE CAN REPORT.</b>
    ///
    /// <para><c>/review</c> counted seven parallel lists of the same four roots across
    /// this tool. The consequence is specific: add a fifth, wire it into
    /// <see cref="CorpusCoverage"/> and <c>Cli.Resolve</c>, forget
    /// <see cref="CorpusFloor.Roots"/>, and <b>the new root has no floor at all</b> —
    /// silently, with every test green. That is WI-435's own defect arriving through the
    /// fix for it, which is the shape §12.37 warns about.</para>
    ///
    /// <para>By reflection, because the point is to catch a member nobody remembered to
    /// think about. <see cref="Empty"/>'s <c>default</c> arm is the second half: a root
    /// in the table with no way to empty it throws rather than passing.</para>
    /// </summary>
    [Fact]
    public void TheRootTableCoversEveryRootCoverageCanReport()
    {
        var measured = typeof(CorpusCoverage)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(int) || p.PropertyType == typeof(int?))
            .Select(p => p.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        // The table's names are short ("razor"); the coverage members are long
        // ("ReaderFacingRazorPages"). Matched by the accessor each row carries, which is
        // the only binding that cannot be wrong.
        var floored = CorpusFloor.Roots
            .Select(root => measured.Single(name => ProbeMatches(root, name)))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        // As a SET and not a count — four out, four in and nothing moves is the defect
        // §12.36 records (WI-583's nine YAML keys), and the whole point here is that a
        // member nobody wired up is NAMED.
        Assert.Equal(measured, floored);

        // And the floor side is covered too: every row's Required accessor has to read a
        // real floor member, which `Shipped` having no nulls makes observable.
        Assert.All(CorpusFloor.Roots,
            root => Assert.NotNull(root.Required(CorpusFloor.Shipped)));
    }

    /// <summary>
    /// True when <paramref name="root"/>'s coverage accessor reads the member named
    /// <paramref name="member"/> — probed by setting that one member to a sentinel and
    /// asking the accessor what it sees. A name-to-name string match would only test
    /// that two strings were typed consistently.
    /// </summary>
    private static bool ProbeMatches(
        (string Name, string Unit, Func<CorpusFloor, int?> Required,
            Func<CorpusCoverage, int?> Walked) root,
        string member)
    {
        const int Sentinel = 4242;
        var zeroed = new CorpusCoverage(0, 0, 0, 0);
        var probed = member switch
        {
            nameof(CorpusCoverage.Pages) => zeroed with { Pages = Sentinel },
            nameof(CorpusCoverage.GlossaryTerms) => zeroed with { GlossaryTerms = Sentinel },
            nameof(CorpusCoverage.ReaderFacingRazorPages) =>
                zeroed with { ReaderFacingRazorPages = Sentinel },
            nameof(CorpusCoverage.Blocks) => zeroed with { Blocks = Sentinel },
            _ => throw new ArgumentOutOfRangeException(nameof(member), member, "unknown member"),
        };

        return root.Walked(probed) == Sentinel;
    }

    /// <summary>
    /// NOT IN SCOPE AND READ NOTHING ARE DIFFERENT, and keeping them different is the
    /// whole fix. A caller may legitimately check the glossary alone — <c>ContentCheckTests</c>
    /// does, and <c>ContentBlocksTests</c> checks a two-file fixture — so
    /// <see cref="CorpusFloor.None"/> has to let a fixture through. What it must not do
    /// is be reachable by accident, which is why the parameter is required.
    /// </summary>
    [Fact]
    public void AFixtureUnderNoFloorIsNotJudgedAgainstTheShippedCorpus()
    {
        var root = OnePageCorpus();
        try
        {
            var findings = ContentChecker.CheckAll(root, null, Today, CorpusFloor.None).Findings;

            Assert.DoesNotContain(findings, CorpusFloor.IsFloorFinding);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// A root the run was never pointed at is not held to a floor, and the coverage says
    /// <c>null</c> rather than 0 so that nothing downstream has to guess which it was.
    /// </summary>
    [Fact]
    public void ARootThatIsNotInScopeReportsNullCoverageRatherThanZero()
    {
        var coverage = ContentChecker.CheckAll(
            PagesRoot, null, Today, CorpusFloor.Shipped, null, BlocksRoot).Coverage;

        Assert.Null(coverage.GlossaryTerms);
        Assert.Null(coverage.ReaderFacingRazorPages);

        // Against the files on disk and NOT against CorpusWhenMeasured: this test is
        // about the difference between null and 0, and comparing a walk to the constant
        // it is checked against would make it red for a reason it is not about.
        Assert.Equal(PagesOnDisk(), coverage.Pages);
    }

    /// <summary>
    /// Coverage is COUNTED AT THE WALK, never recovered from the findings — WI-575's
    /// /review round 2 lost an entire ratchet to the other way round, with seven tests
    /// green, and WI-582 repeated the lesson for the glossary. Pinned against the files
    /// on disk so a plausible-looking count that is not the walk's reds.
    ///
    /// <para><b>All four roots</b>, which it did not do for a round: the Razor count was
    /// omitted here under a docstring claiming the opposite, and the only other place it
    /// appeared sourced it from the walk grading itself (/review).</para>
    /// </summary>
    [Fact]
    public void CoverageIsTheWalkAndNotAGuessAboutIt()
    {
        var coverage = ContentChecker.CheckAll(
            PagesRoot, GlossaryRoot, Today, CorpusFloor.Shipped, RazorRoot, BlocksRoot).Coverage;

        Assert.Equal(PagesOnDisk(), coverage.Pages);
        Assert.Equal(GlossaryOnDisk(), coverage.GlossaryTerms);
        Assert.Equal(BlocksOnDisk(), coverage.Blocks);
        Assert.Equal(ReaderFacingRazorOnDisk().Count, coverage.ReaderFacingRazorPages);
    }

    /// <summary>
    /// <b>THE RAZOR ROOT AS A SET, not as the number 20.</b>
    ///
    /// <para><c>/review</c>: a count cannot see a swap inside a fixed total. Change
    /// <see cref="ContentChecker.IsReaderFacing"/> to start grading
    /// <c>Admin/Queue.cshtml</c> and stop grading one reader page and the count is still
    /// 20 with every test green — on the text WI-414 exists to protect, which is the
    /// most-read text on the site. §12.36's expected-set rule, and §12.24's argument
    /// against a locked-in TOTAL, both land here.</para>
    ///
    /// <para>The set is named out in full on purpose. A reader-facing page added or
    /// removed is a deliberate act and should cost a line here; a page that quietly
    /// changes category should not be free.</para>
    /// </summary>
    [Fact]
    public void TheReaderFacingRazorPagesAreTheSetOnDiskAndNotACount()
    {
        string[] expected =
        [
            "ContentPage.cshtml",
            "Error.cshtml",
            "GetHelpNow.cshtml",
            "Glossary.cshtml",
            "Index.cshtml",
            "Research/Index.cshtml",
            "Research/Item.cshtml",
            "Search.cshtml",
            "Shared/_Disclaimers.cshtml",
            "Shared/_FeedCard.cshtml",
            "Shared/_JourneyPath.cshtml",
            "Shared/_Layout.cshtml",
            "Shared/_Pagination.cshtml",
            "Shared/_ReadinessBadge.cshtml",
            "Shared/_ReviewRow.cshtml",
            "Shared/_StageBadge.cshtml",
            "StatusCodePage.cshtml",
            "Trials/Detail.cshtml",
            "Trials/Index.cshtml",
            "Tumors.cshtml",
        ];

        Assert.Equal(expected, ReaderFacingRazorOnDisk());
        Assert.Equal(expected.Length, ContentChecker.ReaderFacingRazorPagesWhenMeasured);
    }

    // ---------- the command line ----------

    /// <summary>
    /// <b>THE DEFECT ITSELF.</b> <c>--nologo</c> reached <c>args[0]</c> because a
    /// positional parameter accepts any string, and the answer was a number.
    ///
    /// <para>Asserted three ways, and the third is the one that matters: a non-zero exit
    /// code, the argument named back, and <b>no claim of success anywhere in the
    /// output</b>. The shipped behaviour printed "ContentCheck passed" with the WARN
    /// above it, and a test that only checked the exit code would pass against a build
    /// that exits 2 while still telling a human it passed.</para>
    /// </summary>
    [Theory]
    [InlineData("--nologo")]
    [InlineData("-c")]
    [InlineData("--configuration")]
    [InlineData("--no-build")]
    public void AnUnrecognisedArgumentIsAnErrorAndNotAPagesRoot(string arg)
    {
        var output = new StringWriter();

        Assert.Equal(Cli.UsageError, Cli.Run([arg], output));
        Assert.Contains(arg, output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Cli.SuccessClaim, output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>AND REFUSED AS A FLAG, not as a path that happens not to exist.</b>
    ///
    /// <para><b>The break harness found this one, and it is the better kind of
    /// finding.</b> Deleting the flag check entirely left every argument test above
    /// green: <c>--nologo</c> is not a directory either, so the does-this-exist check
    /// refused it anyway and the exit code was the same. A mutation that survives means
    /// the tests pin the OUTCOME and not the reason — so the message is a constant and
    /// this asserts it. Without that, the flag check is dead weight a future cleanup
    /// deletes, and the two things it alone does (naming the problem as a flag, and
    /// refusing a flag that happens to BE a directory) go with it.</para>
    /// </summary>
    [Fact]
    public void AFlagIsRefusedAsAFlag()
    {
        var output = new StringWriter();

        Assert.Equal(Cli.UsageError, Cli.Run(["-c"], output));
        Assert.Contains(Cli.UnrecognisedArgument, output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>The one case only the flag check can catch: a flag that IS a directory.</b>
    /// A relative argument is resolved against the working directory, so a directory
    /// literally named <c>-x</c> beside the test binary makes <c>Directory.Exists("-x")</c>
    /// true and every later guard wave it through as the pages root. Contrived as a
    /// filename and not contrived as a mechanism — it is exactly how <c>--nologo</c> got
    /// as far as <c>args[0]</c>.
    /// </summary>
    [Fact]
    public void AFlagThatIsAlsoARealDirectoryIsStillRefused()
    {
        // Unique, because the working directory is shared by every test in the process.
        var name = "-bh" + Guid.NewGuid().ToString("N")[..8];
        Directory.CreateDirectory(name);
        try
        {
            Assert.True(Directory.Exists(name), "the premise of this test did not hold");

            var output = new StringWriter();

            Assert.Equal(Cli.UsageError, Cli.Run([name], output));
            Assert.Contains(Cli.UnrecognisedArgument, output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(name);
        }
    }

    /// <summary>
    /// <b>ZERO PAGES FROM A ROOT THAT EXISTS — the exact shape of the escape the floor
    /// had to be moved out from behind.</b>
    ///
    /// <para><c>DescriptionCorpusReport</c> reads <c>curatedPagesWalked == 0</c> as "no
    /// curated page in scope, not our business" and <c>yield break</c>s, which is right
    /// for a glossary-only caller and was the door the broken run left through. The
    /// floor therefore runs AFTER both corpus reports and behind no condition at all.
    /// The break harness found this test missing: wrapping the floor back up in
    /// <c>if (curatedPagesWalked &gt; 0)</c> survived, because every other floor test
    /// here walks at least one page.</para>
    /// </summary>
    [Fact]
    public void AnEmptyButExistingPagesRootIsStillAShortWalk()
    {
        var root = Path.Combine(Path.GetTempPath(), "bh-empty-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var findings = ContentChecker.CheckAll(
                root, null, Today, CorpusFloor.Shipped with { GlossaryTerms = null }).Findings;

            var floor = Assert.Single(findings.Where(f => f.File == CorpusFloor.FileFor("pages")));
            Assert.Equal(FindingLevel.Fail, floor.Level);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// The same flag in the position CI would actually put it — second, after a real
    /// pages root. The first argument resolving is not permission to swallow the rest,
    /// and "extra arguments used to be ignored in silence" is the other half of this
    /// item.
    /// </summary>
    /// <remarks>
    /// <c>--help</c> is in here deliberately. Honoured anywhere in the argument list it
    /// prints usage and <b>exits 0 having graded nothing</b>, which is this item's defect
    /// wearing a helpful face — so help is accepted only as the whole command line, and
    /// in any other position it is just another unrecognised flag.
    /// </remarks>
    [Theory]
    [InlineData("--nologo")]
    [InlineData("--help")]
    public void AFlagAfterAValidRootIsStillAnError(string flag)
    {
        var output = new StringWriter();

        Assert.Equal(Cli.UsageError, Cli.Run([PagesRoot, flag], output));
        Assert.DoesNotContain(Cli.SuccessClaim, output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void MoreArgumentsThanTheToolTakesIsAnError()
    {
        var output = new StringWriter();

        Assert.Equal(
            Cli.UsageError,
            Cli.Run([PagesRoot, GlossaryRoot, RazorRoot, BlocksRoot, PagesRoot], output));
    }

    /// <summary>
    /// An empty or whitespace argument is not a path.
    ///
    /// <para><b>This branch was untested and it is load-bearing</b> (/review). Remove it
    /// and <c>Run([""])</c> reaches <c>Path.GetFullPath("")</c>, which THROWS — so the
    /// refusal becomes a stack trace. It also has to run BEFORE the flag check, which
    /// dereferences the string: it ran after for a round, making <c>Run([null!])</c> a
    /// <c>NullReferenceException</c> rather than exit 2.</para>
    /// </summary>
    /// <remarks>
    /// <c>null</c> is a case because the ORDER matters only for null: <c>""</c> survives
    /// <c>StartsWith('-')</c> and <c>null</c> does not. And the assertion is on the
    /// MESSAGE, because the break harness found that deleting the check entirely left
    /// the exit code at 2 anyway — <see cref="Cli.EmptyArgument"/> records why the check
    /// is still worth having.
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void AnEmptyArgumentIsAnErrorAndNotAPath(string? arg)
    {
        var output = new StringWriter();

        Assert.Equal(Cli.UsageError, Cli.Run([arg!], output));
        Assert.Contains(Cli.EmptyArgument, output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Cli.SuccessClaim, output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// A path that is not a usable path at all — a NUL byte — is refused rather than
    /// crashing the diagnostic that was about to name it (/review).
    /// </summary>
    [Fact]
    public void AnUnusablePathIsRefusedRatherThanThrowing()
    {
        var output = new StringWriter();

        Assert.Equal(Cli.UsageError, Cli.Run(["a\0b"], output));
    }

    /// <summary>
    /// <b>A FLOOR OF ZERO IS A MISTAKE, NOT A SETTING.</b>
    ///
    /// <para><c>/review</c>: the first version left two floor members as plain
    /// <c>int</c> and skipped a floor of 0, so on the FLOOR side 0 meant both "no floor
    /// claimed" and "a floor of zero" — this item's own defect handed back one record
    /// over. <c>Shipped with { Pages = 0 }</c> read like <i>do not claim a page count</i>
    /// and switched off the most important root in the corpus. <c>null</c> is the only
    /// way to say "no floor" now, and a non-positive number throws.</para>
    /// </summary>
    [Fact]
    public void ANonPositiveFloorThrowsRatherThanSwitchingARootOff()
    {
        var full = new CorpusCoverage(55, 105, 20, 8);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            (CorpusFloor.Shipped with { Pages = 0 }).Shortfalls(full).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            (CorpusFloor.Shipped with { Blocks = -1 }).Shortfalls(full).ToList());

        // And null really is the way to say it, so the throw cannot be worked around by
        // accident on the way to a legitimate narrow scope.
        Assert.Empty((CorpusFloor.Shipped with { Pages = null }).Shortfalls(
            full with { Pages = 0 }));
    }

    /// <summary>
    /// A path handed in explicitly and not found is a typo, not a corpus to report zero
    /// findings about. The acceptance criterion asked for this directly: "a test asserts
    /// the tool exits non-zero when pointed at a directory that does not exist".
    /// </summary>
    [Fact]
    public void APathGivenOnTheCommandLineThatIsNotThereIsAnError()
    {
        var output = new StringWriter();
        var missing = Path.Combine(Path.GetTempPath(), "bh-no-such-" + Guid.NewGuid().ToString("N"));

        Assert.Equal(Cli.UsageError, Cli.Run([missing], output));
        Assert.Contains("pagesRoot", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Cli.SuccessClaim, output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// And the root that is wrong is NAMED, <b>in every position</b>. Four positional
    /// paths is already enough for the wrong one to be a guess, and "razorRoot" beside
    /// the resolved absolute path is the difference between a fix and a bisect.
    ///
    /// <para><b>All four positions, because two were not enough</b> (break harness): the
    /// names used to be typed again in <c>Cli.Run</c> rather than read from
    /// <see cref="CorpusFloor.Roots"/>, and a copy in the WRONG ORDER
    /// (pages, glossary, blocks, razor) survived a test that only ever looked at
    /// positions one and two.</para>
    ///
    /// <para>The expected names are literals here on purpose. Reading them from the same
    /// table the code reads would assert that a table equals itself.</para>
    /// </summary>
    [Theory]
    [InlineData(0, "pagesRoot")]
    [InlineData(1, "glossaryRoot")]
    [InlineData(2, "razorRoot")]
    [InlineData(3, "blocksRoot")]
    public void TheArgumentThatIsNotThereIsNamedByItsPosition(int position, string expected)
    {
        var output = new StringWriter();
        var missing = Path.Combine(Path.GetTempPath(), "bh-no-such-" + Guid.NewGuid().ToString("N"));

        string[] args = [PagesRoot, GlossaryRoot, RazorRoot, BlocksRoot];
        args[position] = missing;

        Assert.Equal(Cli.UsageError, Cli.Run(args, output));
        Assert.Contains(expected, output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Cli.SuccessClaim, output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void HelpIsNotAnError()
    {
        var output = new StringWriter();

        Assert.Equal(0, Cli.Run(["--help"], output));
        Assert.Contains(Cli.UsageHeading, output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>THE EXIT CODE IS WHAT CHANGED.</b> Pointed at a directory that exists and
    /// holds one page, the tool used to walk it, report the other three roots missing as
    /// warnings, and exit 0. Run through <see cref="Cli.Run"/> rather than
    /// <c>CheckAll</c> on purpose: the floor and the exit code are two different claims
    /// and only one of them was ever the problem.
    /// </summary>
    [Fact]
    public void TheCliRefusesToExitZeroOnACorpusItDidNotWalk()
    {
        var root = OnePageCorpus();
        try
        {
            var output = new StringWriter();

            Assert.Equal(Cli.ContentFailed, Cli.Run([root], output));
            Assert.Contains("FAILED", output.ToString(), StringComparison.Ordinal);

            // AND FAILED FOR THE RIGHT REASON. A one-page fixture could go red on its
            // own content — an unwritten description, a missing source — and then this
            // test would be green against a build with no floor in it at all. Asserting
            // the cause is what §12.37 called the difference between a red and a proof.
            Assert.Contains(CorpusFloor.FileFor("pages"), output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// <b>THE CLI HOLDS EVERY ROOT IN SCOPE.</b>
    ///
    /// <para><see cref="CorpusFloor.Shortfalls"/> skips a root the run was not pointed
    /// at, which is an escape hatch, and the only thing closing it is that
    /// <see cref="Cli.Run"/> cannot produce one — all four roots resolve from a default
    /// when no argument supplies them. An argument parser that could quietly drop a root
    /// would re-open this item exactly, so the claim is pinned rather than argued: the
    /// coverage line prints "no" for a root out of scope, and on the real corpus it
    /// prints four counts.</para>
    /// </summary>
    [Fact]
    public void EveryArgumentCountStillResolvesAllFourRoots()
    {
        // Every count CI or a person can actually invoke, including the no-argument form
        // CI uses — which no test could reach before WI-435 split `Resolve` out, and
        // which is the only form that matters in practice.
        foreach (var args in new[]
        {
            Array.Empty<string>(),
            [PagesRoot],
            [PagesRoot, GlossaryRoot],
            [PagesRoot, GlossaryRoot, RazorRoot],
            new[] { PagesRoot, GlossaryRoot, RazorRoot, BlocksRoot },
        })
        {
            var roots = Cli.Resolve(args);

            Assert.All(
                roots.InOrder,
                root => Assert.False(string.IsNullOrWhiteSpace(root),
                    $"a root came back empty for {args.Length} argument(s)"));

            // AND EVERY RESOLVED ROOT IS A REAL DIRECTORY IN THIS REPOSITORY.
            //
            // /review: "not null or whitespace" is satisfied by "x". Nothing pinned that
            // the DEFAULTS are the four content directories, so a default path mistyped
            // by one segment left the whole suite green while CI went red -- or, if the
            // typo landed on a directory with enough .md files in it, left CI green too.
            // The zero-argument form is literally what ci.yml runs.
            //
            // Combined with the repo root rather than by changing the working directory:
            // the defaults are relative, and CWD is process-global in a parallel suite.
            Assert.All(
                roots.InOrder,
                root => Assert.True(
                    Directory.Exists(Path.IsPathRooted(root) ? root : Path.Combine(Repo, root)),
                    $"resolved root '{root}' is not a directory under the repository"));
        }
    }

    /// <summary>
    /// And on the real corpus, every root is counted rather than reported "not in
    /// scope" — the same claim as the test above, read off the output a person sees.
    /// </summary>
    [Fact]
    public void TheCliHoldsEveryRootInScope()
    {
        var output = new StringWriter();

        Assert.Equal(0, Cli.Run([PagesRoot, GlossaryRoot, RazorRoot, BlocksRoot], output));

        var covered = output.ToString()
            .Split('\n')
            .Single(line => line.StartsWith(Cli.CoveredPrefix, StringComparison.Ordinal));

        Assert.DoesNotContain("no ", covered, StringComparison.Ordinal);
        Assert.Equal(
            Cli.Covered(new CorpusCoverage(
                ContentChecker.CorpusWhenMeasured,
                ContentChecker.GlossaryWhenMeasured,
                ContentChecker.ReaderFacingRazorPagesWhenMeasured,
                ContentChecker.BlocksWhenMeasured)),
            covered.Trim());
    }

    /// <summary>
    /// The acceptance criterion's own words: "the summary line states what was covered
    /// (N pages, M glossary, K Razor) so a collapse in coverage is visible at a glance".
    /// 232 against 345 was the only tell before, and it only tells you anything if you
    /// already know 345 is the right number.
    /// </summary>
    [Fact]
    public void TheSummarySaysWhatWasCoveredInUnitsAReaderKnows()
    {
        var covered = Cli.Covered(new CorpusCoverage(0, null, null, 0));

        Assert.Contains("0 curated page(s)", covered, StringComparison.Ordinal);
        Assert.Contains("no glossary term(s)", covered, StringComparison.Ordinal);
    }
}
