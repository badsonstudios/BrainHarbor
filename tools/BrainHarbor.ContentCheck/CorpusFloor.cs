namespace BrainHarbor.ContentCheck;

/// <summary>
/// WI-435: how much of the corpus a run <b>actually walked</b>, per root.
///
/// <para><b>THE WHOLE DEFECT WAS ONE NUMBER MEANING TWO THINGS.</b> Before this item
/// every root's coverage was an <c>int</c> and <c>0</c> meant both <i>this root is not
/// in scope, do not judge me on it</i> and <i>this root is in scope and I read nothing
/// from it</i>. <see cref="ContentChecker.DescriptionCorpusReport"/> read that 0 as the
/// first — <c>yield break</c>, "not our business" — and a run that had read not one of
/// the 55 curated pages took the same exit as a run that was never asked to. So
/// <c>dotnet run --project tools/BrainHarbor.ContentCheck -- --nologo</c> swallowed the
/// flag as the pages root, graded zero medical pages, printed
/// <c>ContentCheck passed (232 checks, 0 failures)</c> and exited 0. A clean run is 345
/// checks: <b>113 of them disappeared silently, and every curated page's reading grade
/// was among them.</b></para>
///
/// <para><b>So the two meanings are two different values here.</b> <c>null</c> is NOT IN
/// SCOPE — a caller may legitimately check the glossary alone, and
/// <c>ContentCheckTests</c> does. <c>0</c> is IN SCOPE AND EMPTY, which is a broken run
/// and can no longer borrow the other one's silence. The roots that are always in scope
/// (<see cref="Pages"/>, <see cref="Blocks"/>) are plain <c>int</c> and cannot express
/// the escape at all.</para>
///
/// <para><b>Counted at the walk, never inferred from the findings.</b> That rule is
/// WI-575's (/review round 2: a coverage figure recovered from findings was always 0,
/// which killed a whole ratchet while seven tests stayed green) and WI-582's, and it is
/// the reason this record is an output of <see cref="ContentChecker.CheckAll"/> rather
/// than something <c>Program.cs</c> greps back out of its own log.</para>
/// </summary>
/// <para>The member order is the CLI's POSITIONAL ARGUMENT order — pages, glossary,
/// razor, blocks — and matches <see cref="Cli.Roots"/>, <c>WriteRoots</c> and the
/// argument-name list. It did not, for a round: this record and the floor ran
/// pages/glossary/blocks/razor while everything on the command-line side ran
/// pages/glossary/razor/blocks, and the only thing standing between that and a silent
/// swap was an accident of which members happened to be nullable (/review).</para>
public sealed record CorpusCoverage(
    int Pages, int? GlossaryTerms, int? ReaderFacingRazorPages, int Blocks);

/// <summary>
/// WI-435: the smallest corpus a run may report success on — <b>a FLOOR, not a louder
/// warning.</b>
///
/// <para><b>Why a floor and not a better-worded WARN.</b> The broken run printed
/// <c>WARN  --nologo: pages root MISSING — no pages were checked</c> and it changed
/// nothing: the line was one of several hundred, the last line still said
/// <c>passed</c>, the exit code was 0, and <b>CI reads the exit code</b>. A warning is a
/// message to a person who is reading; this gate's audience is a process that is not.
/// Making the message louder leaves the only consumer unaffected. (The backlog entry's
/// own history is the evidence: the author of WI-435 hit this himself, quoted the
/// partial check count as verification on PR #46, and the WARN was on screen while he
/// did it.)</para>
///
/// <para><b>PER ROOT, because one total is met by one root.</b> §12.37 shipped this
/// mistake and caught it in <c>/review</c>: a single floor of 55 over a 168-file sweep
/// was satisfied by the pages alone, so a renamed <c>Content/blocks</c> left the guard
/// green having read no block. Four roots, four floors, and a root that stops matching
/// reds on its own line.</para>
///
/// <para><b>These numbers are corpus SIZES and are never lowered to track a count of
/// anything fixable.</b> That conflation re-armed two closed blockers in WI-578
/// (/review round 4) purely by following the instructions the tool printed. Lowering a
/// floor is a deliberate act that says <i>the corpus really is smaller now</i>, and
/// <c>TheFloorIsTheShippedCorpusAndNotAnOldMeasurementOfIt</c> reds the moment it stops
/// matching the files on disk in either direction.</para>
/// </summary>
/// <para><b>EVERY MEMBER IS NULLABLE AND <c>null</c> IS THE ONLY WAY TO SAY "NO FLOOR"
/// (/review).</b> The first version left <see cref="Pages"/> and <see cref="Blocks"/> as
/// plain <c>int</c> and skipped a floor of 0 — so on the FLOOR side, 0 meant both "no
/// floor claimed" and "a floor of zero", which is this file's own headline defect handed
/// back one record over. <c>Shipped with { Pages = 0 }</c> read like <i>do not claim a
/// page count</i> and silently switched off the most important root in the corpus.
/// A non-positive floor is now an <see cref="ArgumentOutOfRangeException"/> from
/// <see cref="Shortfalls"/> rather than a quiet skip.</para>
public sealed record CorpusFloor(
    int? Pages, int? GlossaryTerms, int? ReaderFacingRazorPages, int? Blocks)
{
    /// <summary>
    /// The corpus as shipped. Every root the CLI resolves is in scope, so no member is
    /// <c>null</c> — see <see cref="CorpusCoverage"/> for what <c>null</c> buys and why
    /// the CLI is not allowed to buy it.
    ///
    /// <para>Every figure is a named constant and not a fresh literal: there is exactly
    /// one 55, one 105, one 8 and one 20 in this tool, so a re-measure cannot land in one
    /// place and miss another. (The usage text printed a second 55 in prose for a round,
    /// which /review caught — it is interpolated from the constant now.)</para>
    /// </summary>
    public static readonly CorpusFloor Shipped = new(
        ContentChecker.CorpusWhenMeasured,
        ContentChecker.GlossaryWhenMeasured,
        ContentChecker.ReaderFacingRazorPagesWhenMeasured,
        ContentChecker.BlocksWhenMeasured);

    /// <summary>
    /// No floor: a fixture, not the corpus.
    ///
    /// <para><b>It has to be said out loud, which is why
    /// <see cref="ContentChecker.CheckAll"/> takes the floor as a REQUIRED argument and
    /// not an optional one with a safe-looking default.</b> Every one of the fifteen
    /// existing call sites had to answer "is this the shipped corpus or a three-page
    /// temp directory?", and the SEVEN that turned out to be the real corpus now say so.
    /// A defaulted parameter would have answered for all fifteen, silently, the way the
    /// positional <c>args[0]</c> answered "what is the pages root?" silently.</para>
    ///
    /// <para><b>The error is asymmetric and that is worth knowing.</b> A fixture wrongly
    /// given <see cref="Shipped"/> fails loudly and immediately. A real-corpus call site
    /// wrongly given <see cref="None"/> passes SILENTLY, and nothing pins it. That is
    /// tolerable only because the gate is the executable and not the suite:
    /// <see cref="Cli.Run"/> passes <see cref="Shipped"/> unconditionally, and
    /// <c>TheCliRefusesToExitZeroOnACorpusItDidNotWalk</c> plus
    /// <c>TheCliHoldsEveryRootInScope</c> pin that it does.</para>
    /// </summary>
    public static readonly CorpusFloor None = new(null, null, null, null);

    /// <summary>
    /// How a floor finding's <see cref="Finding.File"/> is built, so the three places
    /// that recognise one agree by construction rather than by three literals.
    ///
    /// <para>The same reason <see cref="ContentChecker.DescriptionMarker"/> exists
    /// (/review round 2 of WI-575): <c>Program.cs</c> sniffing for the string
    /// <c>" floor)"</c> it typed itself is a value recovered by parsing a message, which
    /// is the defect <see cref="Finding.Grade"/>'s docstring records costing a whole
    /// build gate on every comma-decimal locale.</para>
    /// </summary>
    public static string FileFor(string root) => $"({root}{FloorSuffix}";

    /// <summary>
    /// True when <paramref name="finding"/> came from <see cref="Shortfalls"/>.
    ///
    /// <para>The suffix alone, matched against the one constant <see cref="FileFor"/>
    /// builds from. It also tested the <c>(</c> prefix for a round, which was a SECOND
    /// literal three lines under a docstring claiming the pair agreed by construction
    /// (/review) — and bought nothing: no page path, and neither of the two existing
    /// corpus-report pseudo-paths (<c>(corpus)</c>, <c>(glossary)</c>), ends in
    /// <c>" floor)"</c>.</para>
    /// </summary>
    public static bool IsFloorFinding(Finding finding) =>
        finding.File.EndsWith(FloorSuffix, StringComparison.Ordinal);

    /// <summary>
    /// One literal, read by both halves of the pair above — <see cref="FileFor"/>
    /// building the string and <see cref="IsFloorFinding"/> recognising it. Two copies
    /// is how a recogniser goes quietly vacuous when the builder is reworded.
    /// </summary>
    private const string FloorSuffix = " floor)";

    /// <summary>
    /// <b>THE ONE PLACE THE FOUR ROOTS ARE LISTED.</b> Name, the noun its count is
    /// counted in, and the two accessors that read its floor and its coverage.
    ///
    /// <para><b>/review found SEVEN parallel lists of these same four things</b> — this
    /// record, <see cref="CorpusCoverage"/>, <see cref="Cli.Roots"/>,
    /// <c>WriteRoots</c>'s labels, the argument-name array in <c>Cli.Run</c>,
    /// <c>Cli.Covered</c>, and a literal array inside <see cref="Shortfalls"/> — and the
    /// consequence is specific: add a fifth root, wire it into the coverage and the
    /// resolver, forget the array in <c>Shortfalls</c>, and <b>the new root has no floor
    /// at all</b>, silently, with every test green. That is this item's defect reappearing
    /// through the fix for it.</para>
    ///
    /// <para>Everything that needs the roots reads this now, and
    /// <c>TheRootTableCoversEveryRootCoverageCanReport</c> asserts by reflection that it
    /// covers every property of <see cref="CorpusCoverage"/> — so a fifth root that is
    /// measured and not floored is a red rather than a silence.</para>
    /// </summary>
    public static readonly IReadOnlyList<(
        string Name,
        string Unit,
        Func<CorpusFloor, int?> Required,
        Func<CorpusCoverage, int?> Walked)> Roots =
    [
        ("pages", "curated page", f => f.Pages, c => c.Pages),
        ("glossary", "glossary term", f => f.GlossaryTerms, c => c.GlossaryTerms),
        ("razor", "reader-facing Razor page",
            f => f.ReaderFacingRazorPages, c => c.ReaderFacingRazorPages),
        ("blocks", "content block", f => f.Blocks, c => c.Blocks),
    ];

    /// <summary>
    /// The roots whose walk came up short, as findings — <see cref="FindingLevel.Fail"/>
    /// every time, so the exit code moves.
    ///
    /// <para>A root the floor does not cover (<c>null</c>) is skipped, and so is a root
    /// the run did not have in scope. Both are deliberate: the floor is a claim about
    /// the corpus a run was POINTED at.</para>
    ///
    /// <para><b>That "not in scope" skip is an escape hatch, and the thing that closes
    /// it is that <see cref="Cli.Run"/> never produces one</b> — all four roots resolve
    /// from a default when no argument supplies them, so the executable CI runs holds
    /// every root to <see cref="Shipped"/> or exits non-zero.
    /// <c>TheCliHoldsEveryRootInScope</c> is what pins that, because an argument parser
    /// that could quietly drop a root would re-open this item.</para>
    /// </summary>
    public IEnumerable<Finding> Shortfalls(CorpusCoverage coverage)
    {
        foreach (var (root, unit, readRequired, readWalked) in Roots)
        {
            // NO FLOOR CLAIMED FOR THIS ROOT. The only way to say that, and a number
            // is not one of them — see the record's own note on why 0 used to mean this.
            if (readRequired(this) is not { } required)
            {
                continue;
            }

            // A FLOOR OF ZERO OR LESS IS A MISTAKE, NOT A SETTING. A floor no walk can
            // fall under is a gate that cannot fire, and this one would be reached only
            // by somebody editing a constant to make a red go away — the exact move
            // WriteFloorAdvice is written to talk them out of.
            ArgumentOutOfRangeException.ThrowIfLessThan(required, 1, $"floor for {root}");

            // NOT IN SCOPE: the run was never pointed at this root. Distinct from 0,
            // which is in scope and read nothing, and is the whole item.
            if (readWalked(coverage) is not { } count || count >= required)
            {
                continue;
            }

            // ONE SHORT LINE PER ROOT. The reasoning and the what-to-do-about-it live in
            // Cli.WriteFloorAdvice, printed ONCE — four copies of the same paragraph is
            // the "trains people to skip it" failure Program.cs already reasons about,
            // and on a wrong-working-directory run all four fire at once.
            yield return new(FindingLevel.Fail, FileFor(root),
                $"{count} {unit}(s) were walked, against a floor of {required} — "
                + "ContentCheck cannot report success on a corpus it did not read.");
        }
    }
}
