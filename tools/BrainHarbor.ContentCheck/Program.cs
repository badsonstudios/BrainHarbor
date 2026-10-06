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

        Console.WriteLine();
        Console.WriteLine(
            $"{findings.Count} checks: {failures} failure(s), {warns.Count} warning(s).");

        if (warns.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"All {warns.Count} warning(s) again, in one place:");
            foreach (var finding in warns)
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
