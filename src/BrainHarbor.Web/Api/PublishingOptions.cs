namespace BrainHarbor.Web.Api;

public enum PublishMode
{
    /// <summary>
    /// Hands-off (the default). A summarized item that passes every automated
    /// safety check publishes itself; anything flagged by those checks — a
    /// number that doesn't trace to the source, a banned hype phrase, too-high
    /// reading level — or not yet summarized is held in the review queue.
    /// </summary>
    Auto,

    /// <summary>
    /// Nothing publishes without a person approving it in the review queue.
    /// The original M2 behavior, kept as an opt-in.
    /// </summary>
    Review,
}

/// <summary>
/// How uploaded items reach readers. Set via configuration (Publishing:Mode);
/// defaults to Auto. See content-pipeline.md §"Publish mode".
/// </summary>
public sealed class PublishingOptions
{
    public const string SectionName = "Publishing";

    public PublishMode Mode { get; set; } = PublishMode.Auto;
}
