using System.Text.RegularExpressions;

namespace BrainHarbor.Tests;

/// <summary>
/// The no-em-dash rule, enforced for HAND-WRITTEN copy. AI summaries already
/// get it from ProseStyle.Normalize + the summarize prompt; this pins the same
/// rule for the static site copy (content pages + Razor views), which the
/// normalizer never touches — a gap that let an em dash reach /research.
/// Em dash (U+2014) and en dash (U+2013) are both banned; use plain
/// punctuation (a comma, a period, "to" for ranges).
/// </summary>
public class NoEmDashInCopyTests
{
    private static readonly char[] Dashes = ['—', '–'];

    [Fact]
    public void NoContentPageOrGlossaryTermUsesEmOrEnDashes()
    {
        var root = Path.Combine(FindRepoRoot(), "src", "BrainHarbor.Web", "Content");

        foreach (var file in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            var idx = text.IndexOfAny(Dashes);
            Assert.True(idx < 0,
                $"{Rel(root, file)} has an em/en dash near: \"{Snippet(text, idx)}\"");
        }
    }

    [Fact]
    public void NoRazorViewRendersAnEmOrEnDash()
    {
        var root = Path.Combine(FindRepoRoot(), "src", "BrainHarbor.Web", "Pages");

        foreach (var file in Directory.EnumerateFiles(root, "*.cshtml", SearchOption.AllDirectories))
        {
            // Strip Razor comments (@* ... *@) — developer notes, never rendered.
            var rendered = Regex.Replace(File.ReadAllText(file), @"@\*.*?\*@", "", RegexOptions.Singleline);
            var idx = rendered.IndexOfAny(Dashes);
            Assert.True(idx < 0,
                $"{Rel(root, file)} renders an em/en dash near: \"{Snippet(rendered, idx)}\"");
        }
    }

    private static string Rel(string root, string file) => Path.GetRelativePath(root, file);

    private static string Snippet(string s, int idx)
    {
        if (idx < 0) return "";
        var start = Math.Max(0, idx - 30);
        return s.Substring(start, Math.Min(60, s.Length - start)).Replace("\n", " ").Trim();
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
