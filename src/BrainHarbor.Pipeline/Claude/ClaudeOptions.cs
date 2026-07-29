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

    /// <summary>Kill a single invocation after this long — a hung model call
    /// must not stall the nightly run.</summary>
    [Range(10, 600)]
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>Directory holding the versioned prompt templates.</summary>
    public string? PromptsDirectory { get; set; }
}
