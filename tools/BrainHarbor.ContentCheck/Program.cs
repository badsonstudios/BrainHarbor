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

        Console.WriteLine();
        Console.WriteLine(failures == 0
            ? $"ContentCheck passed ({findings.Count} checks, 0 failures)."
            : $"ContentCheck FAILED: {failures} failure(s).");

        return failures == 0 ? 0 : 1;
    }
}
