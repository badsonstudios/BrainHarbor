using BrainHarbor.Web.Content;

namespace BrainHarbor.ContentCheck;

// WI-106: the readability promise, machine-enforced (content-pipeline §5).
// Usage: BrainHarbor.ContentCheck [pagesRoot [glossaryRoot [razorRoot [blocksRoot]]]]
// Exit 0 = clean (warnings allowed), 1 = at least one FAIL, 2 = the invocation is wrong.
// (Named entry class, not top-level statements — a generated Program class
// would collide with BrainHarbor.Web's in the shared test project.)
public static class Cli
{
    /// <summary>
    /// The content is wrong: a page over the reading limit, invalid front matter, a
    /// corpus smaller than its floor.
    /// </summary>
    public const int ContentFailed = 1;

    /// <summary>
    /// The INVOCATION is wrong, which is a different thing from the content being wrong
    /// and used to be indistinguishable from the content being right (WI-435).
    ///
    /// <para>Separated from <see cref="ContentFailed"/> on purpose even though CI only
    /// asks "is this zero?": a human staring at a red step needs to know whether to go
    /// and read a medical page or go and fix a command line.</para>
    /// </summary>
    public const int UsageError = 2;

    /// <summary>
    /// How the tool claims success, as a constant, so the tests that refuse to find it
    /// on a broken run and the one line that prints it agree by construction. WI-575's
    /// /review round 5 lesson applied to the verdict itself: a test matching a message
    /// by retyping it goes quietly vacuous the first time the message is reworded.
    /// </summary>
    public const string SuccessClaim = "ContentCheck passed";

    /// <summary>
    /// How the tool names a flag, so the test that proves a flag is refused AS A FLAG
    /// (and not merely as a path that does not exist) does not match it by retyping it.
    /// </summary>
    public const string UnrecognisedArgument = "unrecognised argument";

    /// <summary>
    /// WI-435: the roots are printed before the walk, absolute, with their file counts.
    ///
    /// <para>This is the line that makes the whole failure class self-evident. Run the
    /// tool from <c>tools/</c> instead of the repo root and every default relative path
    /// misses; before this item that printed three findings and
    /// <c>ContentCheck passed (3 checks, 0 failures)</c>, which is the gate on the entire
    /// medical corpus reporting success having opened nothing. Now the first four lines
    /// of output say which directories it is about to read and the floor refuses the
    /// run — but the header is what tells a person WHY in one glance, rather than
    /// sending them to re-derive it.</para>
    /// </summary>
    private static void WriteRoots(TextWriter output, Roots roots)
    {
        output.WriteLine($"ContentCheck — working directory: {Directory.GetCurrentDirectory()}");

        // The labels come from CorpusFloor.Roots, so a header that says `razor` and a
        // finding that says `(razor floor)` agree by construction. They agreed by two
        // independently typed literal lists for a round (/review).
        foreach (var (root, name) in roots.InOrder.Zip(CorpusFloor.Roots.Select(r => r.Name)))
        {
            var full = SafeFullPath(root);
            output.WriteLine($"  {name,-9} {full}{(Directory.Exists(root) ? "" : "   *** MISSING ***")}");
        }

        output.WriteLine();
    }

    /// <summary>
    /// <see cref="Path.GetFullPath(string)"/> throws on an empty path and on one holding
    /// a NUL, and every caller here is already on an error path printing a diagnostic —
    /// so a crash there turns a refusal into a stack trace (/review).
    /// </summary>
    private static string SafeFullPath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (ArgumentException)
        {
            return $"<not a usable path: '{path}'>";
        }
    }

    private static void WriteUsage(TextWriter output)
    {
        output.WriteLine(
            $"""
            BrainHarbor.ContentCheck — the reading-level and front-matter gate
            (content-pipeline §5).

            {UsageHeading}
              BrainHarbor.ContentCheck [{ArgumentList}]
              BrainHarbor.ContentCheck --help

            Arguments are POSITIONAL PATHS. There are no flags, and an unrecognised
            argument is an error rather than a path — WI-435: `-- --nologo` used to be
            swallowed as the pages root, so the tool graded zero of the
            {ContentChecker.CorpusWhenMeasured} curated pages and still reported success
            and exited 0. (That number is interpolated, not typed: it was a second
            literal 55 in prose for a round, in the one place a re-measure would never
            think to look — /review.)

            With no arguments the roots default to src/BrainHarbor.Web/Content/pages,
            .../glossary, .../blocks and src/BrainHarbor.Web/Pages — resolved RELATIVE TO
            THE WORKING DIRECTORY, so run it from the repository root.

            Exit codes:
              0  clean — warnings are allowed
              1  at least one FAIL, including a corpus smaller than its floor
              2  the invocation is wrong (unknown argument, too many, a path that is not
                 there)
            """);
    }

    /// <summary>
    /// The four directories a run will read. <b>Every member is non-nullable, and that
    /// is the claim</b> (WI-435).
    ///
    /// <para><see cref="CorpusFloor.Shortfalls"/> skips a root the run was not pointed
    /// at — <c>null</c> coverage, "not in scope" — which is an escape hatch the floor
    /// cannot close by itself. What closes it is this record: the executable CI runs
    /// cannot express a root out of scope, so every root it resolves is held to
    /// <see cref="CorpusFloor.Shipped"/>. A future flag that made one of these nullable
    /// would re-open WI-435 exactly, which is why they are pinned by
    /// <c>EveryArgumentCountStillResolvesAllFourRoots</c> rather than left as a
    /// property of the code somebody reads.</para>
    /// </summary>
    public sealed record Roots(string Pages, string Glossary, string Razor, string Blocks)
    {
        /// <summary>
        /// The four roots in <see cref="CorpusFloor.Roots"/>'s order, which is also the
        /// positional-argument order and <see cref="CorpusCoverage"/>'s member order.
        /// One order everywhere; it was two for a round (/review).
        /// </summary>
        public IEnumerable<string> InOrder => [Pages, Glossary, Razor, Blocks];
    }

    /// <summary>
    /// What each positional argument is called, in order, read from the one root table —
    /// so the message naming a bad path and the finding naming a short root cannot drift
    /// apart. <c>razorRoot</c> is third because the arguments are, which is why
    /// <see cref="CorpusFloor.Roots"/> is ordered that way too.
    /// </summary>
    private static string ArgumentName(int index) =>
        CorpusFloor.Roots[index].Name + "Root";

    /// <summary>The positional arguments, named, for the usage text and the too-many message.</summary>
    private static string ArgumentList =>
        string.Join(" ", CorpusFloor.Roots.Select((_, i) => ArgumentName(i)));

    /// <summary>
    /// Which directories the positional arguments mean.
    ///
    /// <para>Separated from <see cref="Run"/> so that it can be asserted on at all. The
    /// defect this item closes lived in four lines exactly like these, inside an entry
    /// point with no seam and no tests — and the untestability was not incidental to it.
    /// </para>
    /// </summary>
    public static Roots Resolve(string[] args)
    {
        var pages = args.Length > 0 ? args[0]
            : Path.Combine("src", "BrainHarbor.Web", "Content", "pages");
        var glossary = args.Length > 1 ? args[1]
            : Path.Combine("src", "BrainHarbor.Web", "Content", "glossary");
        var razor = args.Length > 2 ? args[2]
            : Path.Combine("src", "BrainHarbor.Web", "Pages");

        // Resolved beside whatever pages root we were given, so the gate can never
        // grade one content tree's pages against another's blocks — and resolved HERE
        // rather than left null for CheckAll to derive, because a null here would be
        // indistinguishable from "no blocks root in scope", and that is the exact
        // conflation this item exists to remove.
        var blocks = args.Length > 3 ? args[3]
            : ContentBlockStore.DefaultBlocksRootFor(pages);

        return new(pages, glossary, razor, blocks);
    }

    public static int Main(string[] args) => Run(args, Console.Out);

    /// <summary>
    /// The whole CLI, writing to <paramref name="output"/> rather than to
    /// <see cref="Console"/>.
    ///
    /// <para><b>Split out so that the argument handling can be tested at all</b> — which
    /// it could not be before WI-435, and that is not a coincidence. The defect lived in
    /// <c>args[0]</c>, in an entry point with no seam, with zero tests; the first test
    /// ever written against it found it. Redirecting <c>Console.Out</c> from a test would
    /// have worked too and is process-global, which xUnit's parallel collections make a
    /// race.</para>
    /// </summary>
    public static int Run(string[] args, TextWriter output)
    {
        // HELP ONLY WHEN IT IS THE WHOLE COMMAND LINE, and the word "help" is not a
        // form of it (WI-435). Both halves of that are this item's own defect in
        // miniature: `args.Any(...)` made `ContentCheck <pagesRoot> --help` print usage
        // and EXIT 0 having graded nothing, and accepting a bare `help` did the same to
        // anybody with a directory of that name. A path is a path.
        //
        // THIS BRANCH EXITS 0 HAVING READ NOTHING, WHICH IS THE EXEMPTION AND IT IS
        // WRITTEN DOWN ON PURPOSE (/review). Everything else in this file argues that
        // the exit code is the only thing with a consumer, so an exemption from it
        // cannot be left implied. Asking a tool what it does is not asking it to check
        // anything, every CLI answers 0 to it, and `ci.yml` passes no arguments at all
        // so this branch is unreachable from the one caller that reads the code.
        if (args is ["--help"] or ["-h"] or ["-?"] or ["/?"])
        {
            WriteUsage(output);
            return 0;
        }

        // AN UNRECOGNISED ARGUMENT IS AN ERROR, NOT A PATH (WI-435).
        //
        // This is the cause; the floor below is the backstop. `--nologo` reached
        // `pagesRoot` because a positional parameter accepts any string, and the only
        // report was a WARN in a log read by a process that reads exit codes. Refusing
        // here means the typo can never again be answered with a number.
        //
        // CI IS SAFE TODAY BY LUCK AND THAT IS THE ARGUMENT, not a reassurance: its
        // args (`--configuration Release --no-build`) land before the `--` and so are
        // eaten by `dotnet run`. One flag added on the wrong side of that separator
        // muted the reading-level gate for the whole medical corpus while reporting
        // success.
        //
        // AND IT IS NOT MADE REDUNDANT BY THE does-this-directory-exist CHECK BELOW,
        // which was the first thing the break harness found: deleting this block left
        // every argument test green, because `--nologo` is not a directory either and
        // the next check refused it anyway. Two things are left that only this block
        // does — it names the problem as a FLAG rather than as a missing path, which is
        // the difference between a fix and a hunt, and it refuses a flag that happens
        // to BE a directory, which `-c` on a machine with a `-c` directory is. The
        // message is therefore a constant and `AFlagIsRefusedAsAFlag` asserts it.
        foreach (var arg in args)
        {
            // EMPTINESS FIRST, because the flag check below dereferences the string.
            // It was second for a round, which made `Run([null!], …)` a NullReference
            // instead of exit 2 — a usage error answered with a stack trace (/review).
            if (string.IsNullOrWhiteSpace(arg))
            {
                output.WriteLine(
                    $"ContentCheck: {EmptyArgument}. Pass a directory or pass nothing.");
                output.WriteLine();
                WriteUsage(output);
                return UsageError;
            }

            // Only the dash. `/anything` is a legitimate absolute path on the CI runner
            // and rejecting it would be the same mistake in the other direction — a
            // guard that refuses a real corpus is as useless as one that accepts none.
            if (arg.StartsWith('-'))
            {
                output.WriteLine(
                    $"ContentCheck: {UnrecognisedArgument} '{arg}'. This tool takes "
                    + "positional PATHS and has no flags; an argument it does not "
                    + "recognise is not reinterpreted as the pages root (WI-435).");
                output.WriteLine();
                WriteUsage(output);
                return UsageError;
            }
        }

        if (args.Length > CorpusFloor.Roots.Count)
        {
            output.WriteLine(
                $"ContentCheck: {args.Length} arguments, but this tool takes at most "
                + $"{CorpusFloor.Roots.Count} ({ArgumentList}). Extra arguments used to "
                + "be ignored in silence.");
            output.WriteLine();
            WriteUsage(output);
            return UsageError;
        }

        var roots = Resolve(args);

        // A PATH HANDED IN EXPLICITLY AND NOT FOUND IS A TYPO, NOT A STATE (WI-435).
        // The defaults are allowed to be missing — the floor then refuses the run and
        // the header above says which one it was — but a directory somebody typed and
        // got wrong is answered now rather than walked around.
        //
        // SO A MISSING DEFAULT ROOT IS EXIT 1 AND A MISSING EXPLICIT ONE IS EXIT 2, AND
        // THE ASYMMETRY IS DELIBERATE (/review raised that the advice text and the exit
        // code read as disagreeing). An explicit path is a claim the caller made; a
        // default is a claim about where the caller is standing. The first is a mistake
        // in the command and nothing is checkable until it is fixed; the second is the
        // corpus not being where this run expected, which is a finding ABOUT the corpus
        // and belongs with the other findings behind the floor — which is also the only
        // one of the two that can happen inside CI.
        foreach (var (index, root) in args.Select((root, index) => (index, root)))
        {
            if (Directory.Exists(root))
            {
                continue;
            }

            output.WriteLine(
                $"ContentCheck: {ArgumentName(index)} '{root}' is not a directory "
                + $"(resolved to '{SafeFullPath(root)}'). A path given on the command "
                + "line and not found is a mistake in the command, not a corpus to "
                + "report zero findings about.");
            return UsageError;
        }

        WriteRoots(output, roots);

        // THE FLOOR IS ALWAYS Shipped HERE, and never None. This executable exists to
        // gate the corpus in this repository; a run that walked less than that corpus
        // has not done the job it was invoked for, whichever directory it was pointed
        // at. `CorpusFloor.None` is for a fixture, and a fixture is never this tool.
        var (findings, coverage) = ContentChecker.CheckAll(
            roots.Pages, roots.Glossary, DateOnly.FromDateTime(DateTime.UtcNow),
            CorpusFloor.Shipped, roots.Razor, roots.Blocks);

        var failures = 0;
        foreach (var finding in findings)
        {
            var tag = finding.Level switch
            {
                FindingLevel.Fail => "FAIL",
                FindingLevel.Warn => "WARN",
                _ => "  ok",
            };
            if (finding.Level == FindingLevel.Fail)
            {
                failures++;
            }
            output.WriteLine($"{tag}  {finding.File}: {finding.Message}");
        }

        // WI-575: A TALLY, AND THE ACTIONABLE WARNS AGAIN AT THE END.
        //
        // CI reads only the exit code, so this output is for a human reading a long
        // log — and a gate that prints dozens of expected warnings trains people to
        // skip it, which is the failure this file's own reasoning about admin pages
        // warns of. Nothing is hidden and nothing is downgraded: every warning prints
        // in place above. What changes is that the ones needing a person are the last
        // thing on screen.
        //
        // AND THE BACKLOG PARTITION IS GONE (WI-578 slice 3), because the backlog is.
        // Making the description grade visible took the WARN count from 20 to 61, and
        // 41 of those were one booked item: a known backlog printed as a warning
        // because it could not yet be gated. That number went 41 → 29 → 21 → 0 across
        // three slices and the grade is a Fail everywhere now, so there is no bucket of
        // expected warnings left to keep out of a reader's way — which is the whole
        // thing the partition existed for.
        //
        // NOT BECAUSE A DESCRIPTION WARNING IS UNREACHABLE, which is what the first
        // version of this comment claimed and what /review caught: slice 3 also gave
        // `GradeDescription` the three-level shape the body gate has had since WI-414,
        // so a description in the 5.5-6.0 approach band warns — /tumors/glioblastoma
        // reads 5.6 today. `IsBacklogWarn` would still MATCH it. The point is that such
        // a warning is now ACTIONABLE: it means one line is drifting toward the gate,
        // which is exactly the kind of thing this block exists to put in front of a
        // person rather than file under "expected".
        //
        // (It was partitioned on the GRADE and not on the marker, for a reason worth
        // keeping even as the code goes: the blank-description warning carries the
        // `[description]` marker too, so splitting on the marker alone filed a page
        // with NO opening paragraph under "above the reading limit — a known backlog",
        // which was false of it, and dropped it from the list this block exists to put
        // in front of a person. /review round 5. A warning added to make a defect
        // visible landing in the skip-it bucket is the same shape as reporting a
        // grade-19.7 description as `ok`, which this item already did once.)
        //
        // ONE ENUMERATION, AND THE RECAP SAYS WHAT IT IS (/review). This was `warns`
        // and `actionable` — the identical predicate counted twice — under a heading
        // that still implied a filtered list.
        var warns = findings.Where(f => f.Level == FindingLevel.Warn).ToList();

        output.WriteLine();
        output.WriteLine(
            $"{findings.Count} checks: {failures} failure(s), {warns.Count} warning(s).");

        // WI-435: WHAT WAS COVERED, IN THE UNITS A READER OF THIS LOG THINKS IN.
        // The check count alone was the only tell that 113 checks had vanished, and it
        // only tells you if you already know that 345 is the right number. "0 curated
        // pages" needs no prior knowledge at all.
        output.WriteLine(Covered(coverage));

        if (warns.Count > 0)
        {
            output.WriteLine();
            output.WriteLine($"All {warns.Count} warning(s) again, in one place:");
            foreach (var finding in warns)
            {
                output.WriteLine($"  {finding.File}: {finding.Message}");
            }
        }

        if (findings.Any(CorpusFloor.IsFloorFinding))
        {
            WriteFloorAdvice(output);
        }

        output.WriteLine();
        output.WriteLine(failures == 0
            ? $"{SuccessClaim} ({findings.Count} checks, 0 failures)."
            : $"ContentCheck FAILED: {failures} failure(s).");

        return failures == 0 ? 0 : ContentFailed;
    }

    /// <summary>
    /// Printed once when any root came up short — the reasoning that used to be copied
    /// into every floor finding.
    /// </summary>
    private static void WriteFloorAdvice(TextWriter output)
    {
        output.WriteLine();
        output.WriteLine(
            """
            A FLOOR WENT RED, which means this run did not read the whole corpus.

            Every reading grade this tool publishes is a claim about the files it
            actually opened, so a short walk does not prove anything about the pages it
            missed. Check the resolved roots printed at the top: the usual causes are a
            wrong working directory (the defaults are relative — run from the repository
            root), a renamed or moved content directory, and an argument swallowed as a
            path.

            If the corpus genuinely shrank, re-measure and lower CorpusFloor.Shipped on
            purpose. Those numbers are corpus SIZES and never counts of anything that can
            be fixed — lowering one to make a red go away switches this gate off.
            """);
    }

    /// <summary>
    /// How the coverage line starts, and how the usage text's first section heading
    /// reads. Constants because two tests matched them by retyping the prefix (/review),
    /// which is what <see cref="SuccessClaim"/> and <see cref="UnrecognisedArgument"/>
    /// were extracted to stop doing two tests earlier in the same file.
    /// </summary>
    public const string CoveredPrefix = "Covered:";

    /// <inheritdoc cref="CoveredPrefix"/>
    public const string UsageHeading = "Usage:";

    /// <summary>
    /// How the tool names an empty argument.
    ///
    /// <para>A constant for the reason <see cref="UnrecognisedArgument"/> is one, and the
    /// break harness made the same finding twice: with the emptiness check deleted,
    /// <c>Run([""])</c> still exits 2 — <c>Directory.Exists("")</c> is false and
    /// <see cref="SafeFullPath"/> catches the throw — so a test asserting only the exit
    /// code could not tell the check was gone. What the check alone buys is <b>this
    /// sentence instead of "pagesRoot '' is not a directory (resolved to &lt;not a usable
    /// path&gt;)"</b>, and refusing a <c>null</c> before the flag check dereferences
    /// it.</para>
    /// </summary>
    public const string EmptyArgument = "an empty argument is not a path";

    /// <summary>
    /// The coverage line, built from the one root table, so it cannot name three of four
    /// roots. <c>null</c> prints as "no" rather than as 0, because those are the two
    /// meanings WI-435 had to pull apart — see <see cref="CorpusCoverage"/>.
    /// </summary>
    public static string Covered(CorpusCoverage coverage) =>
        CoveredPrefix + " "
        + string.Join(", ", CorpusFloor.Roots.Select(root =>
            $"{root.Walked(coverage)?.ToString() ?? "no"} {root.Unit}(s)"))
        + ".";
}
