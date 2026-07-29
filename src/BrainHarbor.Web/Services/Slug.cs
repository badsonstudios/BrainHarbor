namespace BrainHarbor.Web.Services;

/// <summary>
/// URL slug generation, shared by the review queue (human approval) and the
/// sync API (auto-publish) so a permalink reads the same however it was
/// published.
/// </summary>
public static class Slug
{
    public const int MaxLength = 80;

    public static string From(string title)
    {
        var chars = title.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        slug = slug.Trim('-');
        if (slug.Length > MaxLength)
        {
            slug = slug[..MaxLength].TrimEnd('-');
        }

        return slug.Length == 0 ? "item" : slug;
    }
}
