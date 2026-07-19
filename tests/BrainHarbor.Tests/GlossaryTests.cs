using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-105 unit tests: term-file parsing and the first-occurrence tooltip
/// marker, including both escape hatches.
/// </summary>
public class GlossaryTests
{
    private static readonly GlossaryTerm Glioma =
        new("glioma", "glioma", [], "glee-OH-muh",
            "A tumor that starts in the glial cells.");

    private static readonly GlossaryTerm Idh =
        new("idh-gene-change", "IDH gene change", ["IDH mutation", "IDH-mutant"], null,
            "A small change in a tumor's IDH gene.");

    private static string Render(string markdown, params GlossaryTerm[] terms) =>
        ContentStore.Parse($"---\ntitle: Test\n---\n{markdown}", "test", terms).Html;

    // ---------- term file parsing ----------

    [Fact]
    public void ParsesATermFile()
    {
        var term = GlossaryStore.ParseTerm(
            "---\nterm: glioma\nalso: [brain glioma]\npronunciation: \"glee-OH-muh\"\n---\n\nA tumor that starts in glial cells.",
            "glioma");

        Assert.Equal("glioma", term.Slug);
        Assert.Equal("glioma", term.Term);
        Assert.Equal(["brain glioma"], term.Aliases);
        Assert.Equal("glee-OH-muh", term.Pronunciation);
        Assert.Equal("A tumor that starts in glial cells.", term.Definition);
    }

    [Fact]
    public void TermFileWithoutTermOrBodyThrows()
    {
        Assert.Throws<FormatException>(() =>
            GlossaryStore.ParseTerm("---\npronunciation: x\n---\nBody.", "bad"));
        Assert.Throws<FormatException>(() =>
            GlossaryStore.ParseTerm("---\nterm: x\n---\n", "bad"));
    }

    // ---------- tooltip marking ----------

    [Fact]
    public void FirstOccurrenceGetsATooltipLaterOnesDoNot()
    {
        var html = Render("A glioma is a tumor. Another glioma sentence.", Glioma);

        Assert.Single(FindAll(html, "popovertarget=\"def-glioma\""));
        Assert.Contains("<button type=\"button\" class=\"term\" popovertarget=\"def-glioma\">glioma</button>", html);
        Assert.Contains("Another glioma sentence.", html); // second is plain text
    }

    [Fact]
    public void TooltipCarriesDefinitionAndGlossaryFallbackLink()
    {
        var html = Render("A glioma is a tumor.", Glioma);

        Assert.Contains("id=\"def-glioma\" popover class=\"term-definition\"", html);
        Assert.Contains("A tumor that starts in the glial cells.", html);
        Assert.Contains("href=\"/glossary#glioma\"", html);
    }

    [Fact]
    public void AliasesTriggerTheTooltipAndConsumeTheFirstOccurrence()
    {
        var html = Render("The IDH mutation matters. The IDH gene change too.", Idh);

        // Alias matched first; the later full term stays plain.
        Assert.Contains(">IDH mutation</button>", html);
        Assert.Single(FindAll(html, "popovertarget=\"def-idh-gene-change\""));
    }

    [Fact]
    public void MatchingIsCaseInsensitiveAndWholeWord()
    {
        Assert.Contains(">Glioma</button>", Render("Glioma at sentence start.", Glioma));
        Assert.DoesNotContain("</button>", Render("Gliomatosis is a different word.", Glioma));
    }

    [Fact]
    public void HeadingsLinksAndCodeAreNeverMarked()
    {
        Assert.DoesNotContain("</button>", Render("## About glioma", Glioma));
        Assert.DoesNotContain("</button>", Render("[glioma info](https://example.org)", Glioma));
        Assert.DoesNotContain("</button>", Render("`glioma` in code", Glioma));
    }

    [Fact]
    public void EscapeHatchSkipsThatOccurrenceAndMarksTheNext()
    {
        var html = Render("First %%glioma%% plain. Second glioma gets the tooltip.", Glioma);

        Assert.Single(FindAll(html, "popovertarget=\"def-glioma\""));
        Assert.Contains("First glioma plain.", html);              // marker stripped
        Assert.Contains("Second <button", html);                   // second marked
        Assert.DoesNotContain("%%", html);
    }

    [Fact]
    public void SuppressionMarkerDisablesTheTermForTheWholePage()
    {
        var html = Render("!%glioma%A glioma page about glioma.", Glioma);

        Assert.DoesNotContain("</button>", html);
        Assert.DoesNotContain("!%", html);
        Assert.Contains("A glioma page about glioma.", html);
    }

    [Fact]
    public void LongerNameWinsAtTheSamePosition()
    {
        var shortIdh = new GlossaryTerm("idh", "IDH", [], null, "The IDH gene.");
        var html = Render("The IDH gene change matters.", Idh, shortIdh);

        Assert.Contains(">IDH gene change</button>", html);
    }

    [Fact]
    public void TermsMatchAcrossSourceLineWraps()
    {
        // Markdown convention wraps at ~80 cols — a multi-word term split by
        // a soft line break must still match.
        var html = Render("The tumor had an IDH gene\nchange that matters.", Idh);

        Assert.Contains(">IDH gene change</button>".Replace(' ', ' '), html);
        Assert.Contains("popovertarget=\"def-idh-gene-change\"", html);
    }

    [Fact]
    public void HyphenPrefixedCompoundsAreNotMisMatched()
    {
        // "non-IDH-mutant" means the OPPOSITE — a tooltip here would be
        // medically wrong. Hyphens are word-joiners, not boundaries.
        var html = Render("This applies to non-IDH-mutant tumors.", Idh);

        Assert.DoesNotContain("</button>", html);
        Assert.Contains("non-IDH-mutant", html);
    }

    [Fact]
    public void HyphenatedAliasOnItsOwnStillMatches()
    {
        var html = Render("IDH-mutant tumors grow more slowly.", Idh);

        Assert.Contains(">IDH-mutant</button>", html);
    }

    [Fact]
    public void SuppressionViaAnAliasSuppressesTheWholeTerm()
    {
        var html = Render("!%IDH-mutant%The IDH gene change stays plain.", Idh);

        Assert.DoesNotContain("</button>", html);
    }

    [Fact]
    public void TermAndDefinitionAreHtmlEscapedInTheTooltip()
    {
        var hostile = new GlossaryTerm("evil", "evil", [], null, "Def with <script>alert(1)</script> & \"quotes\".");
        var html = Render("An evil word.", hostile);

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void NonSlugFileNamesAreRejected()
    {
        Assert.Throws<FormatException>(() =>
            GlossaryStore.ParseTerm("---\nterm: x\n---\nDef.", "My Term"));
    }

    private static List<int> FindAll(string haystack, string needle)
    {
        var hits = new List<int>();
        for (var i = haystack.IndexOf(needle, StringComparison.Ordinal);
             i >= 0;
             i = haystack.IndexOf(needle, i + 1, StringComparison.Ordinal))
        {
            hits.Add(i);
        }
        return hits;
    }
}
