namespace BrainHarbor.Pipeline.Logging;

/// <summary>
/// WI-417: where the per-run log file goes and how little disk it is allowed
/// to take. Bound from the <c>Pipeline:Logging</c> configuration section
/// (a section of its own, so it can never collide with the host's standard
/// <c>Logging</c> section, which holds the level filters).
/// </summary>
public sealed class FileLogOptions
{
    public const string SectionName = "Pipeline:Logging";

    /// <summary>Off switch. On by default — the whole point of the item is that
    /// an unattended run leaves evidence without anyone remembering to ask.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Overrides <see cref="DefaultDirectory"/>. Empty means the default.</summary>
    public string? Directory { get; set; }

    /// <summary>Delete run logs older than this many days.</summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Hard ceiling on the number of run logs kept, whatever their age. The
    /// age limit alone cannot bound a task that is re-triggered in a loop.
    /// </summary>
    public int MaxFiles { get; set; } = 100;

    /// <summary>
    /// Ceiling for ONE run's file, after which the log stops with a line saying
    /// so. A normal day is tens of kilobytes and the 1,300-item backfill was a
    /// few hundred; this exists for a runaway loop inside a single run, which
    /// the other two limits cannot catch until the run is over.
    /// </summary>
    public int MaxFileMegabytes { get; set; } = 32;

    /// <summary>
    /// Ceiling for the whole directory. Without it the advertised worst case is
    /// <see cref="MaxFiles"/> × <see cref="MaxFileMegabytes"/> — over three
    /// gigabytes — which is not what "logs are pruned for you" should mean.
    /// </summary>
    public int MaxDirectoryMegabytes { get; set; } = 256;

    /// <summary>
    /// <c>%LOCALAPPDATA%\BrainHarbor\logs</c>. Deliberately outside the repo:
    /// the scheduled task runs from <c>artifacts/pipeline</c>, which
    /// <c>dotnet publish</c> rewrites on every re-registration, and logs must
    /// outlive that. (<c>SpecialFolder.LocalApplicationData</c> resolves on
    /// Linux/macOS too, so a non-Windows run still logs somewhere sensible.)
    /// </summary>
    public static string DefaultDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BrainHarbor",
        "logs");

    public string ResolvedDirectory =>
        string.IsNullOrWhiteSpace(Directory) ? DefaultDirectory : Directory;
}
