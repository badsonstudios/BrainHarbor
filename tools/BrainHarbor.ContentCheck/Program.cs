namespace BrainHarbor.ContentCheck;

// WI-106: the readability promise, machine-enforced (content-pipeline §5).
// Usage: BrainHarbor.ContentCheck <pagesRoot> [glossaryRoot] [razorRoot] [blocksRoot]
// Exit 0 = clean (warnings allowed), 1 = at least one FAIL.
// (Named entry class, not top-level statements — a generated Program class
// would collide with BrainHarbor.Web's in the shared test project.)
public static class Cli
{
    public static int Main(string[] args)
    {
        var pagesRoot = args.Length > 0 ? args[0]
            : Path.Combine("src", "BrainHarbor.Web", "Content", "pages");
        var glossaryRoot = args.Length > 1 ? args[1]
            : Path.Combine("src", "BrainHarbor.Web", "Content", "glossary");
        var razorRoot = args.Length > 2 ? args[2]
            : Path.Combine("src", "BrainHarbor.Web", "Pages");
        // Defaults to the blocks directory beside whatever pages root we were
        // given, so the gate can never grade one content tree's pages against
        // another's blocks.
        var blocksRoot = args.Length > 3 ? args[3] : null;

        var findings = ContentChecker.CheckAll(
            pagesRoot, glossaryRoot, DateOnly.FromDateTime(DateTime.UtcNow), razorRoot, blocksRoot);

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
            Console.WriteLine($"{tag}  {finding.File}: {finding.Message}");
        }

        // WI-575: A TALLY, AND THE ACTIONABLE WARNS AGAIN AT THE END.
        //
        // Making the description grade visible took the WARN count from 20 to 61, and
        // 41 of those are a known, booked backlog (WI-578). CI reads only the exit
        // code, so this output is for a human reading a long log — and a gate that
        // prints forty-one expected warnings trains people to skip it, which is the
        // failure this file's own reasoning about admin pages warns of.
        //
        // Nothing is hidden and nothing is downgraded: all 61 print in place above.
        // What changes is that the ones needing a person are the last thing on screen.
        var warns = findings.Count(f => f.Level == FindingLevel.Warn);
        // PARTITIONED ON THE GRADE, NOT ON THE MARKER. Both the grade warnings and the
        // blank-description warning carry the `[description]` marker, so splitting on
        // the marker alone filed a page with NO opening paragraph under "41 of them
        // page descriptions above the reading limit — a known backlog" (false of it) and
        // dropped it from the list this block exists to put in front of a person
        // (/review round 5). A warning added to make a defect visible landing in the
        // skip-it bucket is the same shape as reporting a grade-19.7 description as
        // `ok`, which this item already did once.
        static bool IsBacklogWarn(Finding f) =>
            f.Level == FindingLevel.Warn
            && f.File.EndsWith(ContentChecker.DescriptionMarker, StringComparison.Ordinal)
            && f.Grade is not null;

        var descriptionWarns = findings.Count(IsBacklogWarn);
        var otherWarns = findings
            .Where(f => f.Level == FindingLevel.Warn && !IsBacklogWarn(f))
            .ToList();

        Console.WriteLine();
        Console.WriteLine(
            $"{findings.Count} checks: {failures} failure(s), {warns} warning(s) "
            + $"({descriptionWarns} of them page descriptions above the reading limit — "
            + "a known backlog, WI-578).");

        if (otherWarns.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"Warnings that are not the WI-578 backlog ({otherWarns.Count}):");
            foreach (var finding in otherWarns)
            {
                Console.WriteLine($"  {finding.File}: {finding.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine(failures == 0
            ? $"ContentCheck passed ({findings.Count} checks, 0 failures)."
            : $"ContentCheck FAILED: {failures} failure(s).");

        return failures == 0 ? 0 : 1;
    }
}
