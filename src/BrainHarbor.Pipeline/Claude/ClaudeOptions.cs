using System.ComponentModel.DataAnnotations;

namespace BrainHarbor.Pipeline.Claude;

/// <summary>
/// How the pipeline invokes the local Claude Code CLI (architecture.md §5).
/// No Anthropic API key — the installed `claude` CLI does the LLM work under
/// Dan's subscription.
/// </summary>
public sealed class ClaudeOptions
{
    public const string SectionName = "Claude";

    /// <summary>The CLI on PATH, or an absolute path to it.</summary>
    [Required]
    public string Executable { get; set; } = "claude";

    /// <summary>
    /// Model for the classify + summarize calls, passed as <c>--model</c>.
    /// Defaults to the latest Opus: these are medical summaries for patients,
    /// so quality outranks cost (Dan's call). Set to a cheaper model
    /// (e.g. "sonnet" or "haiku") in config if cost becomes a concern.
    /// </summary>
    [Required]
    public string Model { get; set; } = "claude-opus-5";

    /// <summary>Kill a single invocation after this long — a hung model call
    /// must not stall the nightly run.</summary>
    [Range(10, 600)]
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>Directory holding the versioned prompt templates.</summary>
    public string? PromptsDirectory { get; set; }
}
