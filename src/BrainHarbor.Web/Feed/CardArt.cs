namespace BrainHarbor.Web.Feed;

/// <summary>
/// The card hero: a vetted photo backdrop, chosen by content — see
/// <see cref="CardImages"/>. Picked from a small human-reviewed pool, never
/// generated, so it cannot invent a misleading medical image. Decorative.
///
/// It carried the readiness score as a "N of 10 ready" dial until the journey
/// handoff (2026-08-19) retired that number: a 10-point scale has no
/// plain-language meaning at any single value, and a dial filled most of the
/// way round reads as "nearly cured" to exactly the reader least able to
/// discount it. The journey path took over BOTH its job and its position —
/// overlaid on the photo, not stacked under it.
/// </summary>
public static class CardArt
{
    /// <summary>
    /// The inline background for the hero. Only the style value, not the whole
    /// element: the journey path is rendered INSIDE the hero by a partial
    /// (Dan's call, 2026-08-19 — the indicator overlays the photo where the
    /// dial used to be, rather than sitting under it), and building nested
    /// markup as a raw string here would put a Razor partial's output inside a
    /// hand-concatenated string. The element lives in _FeedCard.cshtml instead.
    /// </summary>
    /// <param name="imageUrl">
    /// From <see cref="CardImages"/>; null falls back to a plain gradient so a
    /// card with no matching photo still gets a surface for the path to sit on.
    /// </param>
    public static string HeroStyle(string? imageUrl) =>
        imageUrl is not null
            ? $"background-image:url('{imageUrl}')"
            : "background:linear-gradient(135deg,hsl(205,30%,90%),hsl(190,28%,94%))";
}
