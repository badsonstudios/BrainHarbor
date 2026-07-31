using System.ComponentModel.DataAnnotations;

namespace BrainHarbor.Pipeline;

/// <summary>
/// Pipeline configuration. The API key comes from user-secrets locally (never
/// a config file, never the repo) — see api-keys-config.md.
/// </summary>
public sealed class PipelineOptions
{
    public const string SectionName = "Pipeline";

    /// <summary>Base URL of the site's sync API, e.g. https://brainharbor.org.</summary>
    [Required]
    [Url]
    public string SyncApiBaseUrl { get; set; } = "https://localhost:5001";

    /// <summary>Shared secret for /api/sync/* — user-secrets or environment only.</summary>
    [Required]
    public string SyncApiKey { get; set; } = "";

    /// <summary>Upload batch size; the API caps requests at 500.</summary>
    [Range(1, 500)]
    public int BatchSize { get; set; } = 100;

    /// <summary>Per-request timeout for the sync API.</summary>
    [Range(5, 300)]
    public int RequestTimeoutSeconds { get; set; } = 60;

    /// <summary>NCBI E-utilities key (WI-204); politeness rate rises with it.</summary>
    public string? NcbiApiKey { get; set; }

    /// <summary>
    /// Contact address NCBI asks API consumers to send (PLAN.md §5) — a
    /// mailbox they can reach if the tool misbehaves. A role address on the
    /// project domain, never a personal one: this ships in a public repo.
    /// Override via user-secrets/env if a monitored inbox is preferred.
    /// </summary>
    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = "contact@brainharbor.org";
}
