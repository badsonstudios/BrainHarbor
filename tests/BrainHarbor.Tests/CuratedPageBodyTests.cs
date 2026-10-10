using BrainHarbor.Web.Content;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-566. <c>CuratedPage.Body</c> strips front matter by seeking <c>\n---</c>,
/// and for six items it did nothing else: handed a string with no front matter
/// it found none, added 4 to -1, and returned the text with ITS FIRST THREE
/// CHARACTERS MISSING — silently, with no throw and no red.
///
/// <para><b>These are the guards for the refusal, not for the slice.</b> The
/// slice is exercised by every one of the ~460 <c>ReaderText</c> calls in this
/// assembly; what nothing exercised was the -1 branch, which is why it survived
/// six re-commits of the comment warning about it (§12.8, WI-520). Each test
/// below pins one condition of the refusal, and the harness reverts that
/// condition and demands this file go red.</para>
///
/// <para><b>Why both helpers throw.</b> Making <c>Body</c> refuse a fragment
/// makes <see cref="CuratedPage.ReaderTextOfBody"/> the escape hatch every
/// corrected call site reaches for — and handed a WHOLE page it would return
/// the YAML as prose. A pair where one half throws and the other silently
/// scans metadata has moved the defect, not fixed it, so the converse is pinned
/// here too.</para>
/// </summary>
public class CuratedPageBodyTests
{
    /// <summary>
    /// The exact string from the item, and it is a real sentence off a real
    /// page: <c>"Some things should not wait…"</c> came back as
    /// <c>"e things should not wait…"</c>. An <c>Assert.Contains</c> anchored
    /// further in cannot see that, which is why six guards carried it green.
    /// </summary>
    [Fact]
    public void BodyRefusesASectionInsteadOfEatingItsFirstThreeCharacters()
    {
        const string Section = "Some things should not wait. Call 911 if you cannot wake them.";

        var thrown = Assert.Throws<FormatException>(() => CuratedPage.Body(Section));

        Assert.Contains("WI-566", thrown.Message, StringComparison.Ordinal);

        // The message QUOTES the rejected string, because "not a whole page" is
        // not actionable on its own when the caller is three helpers deep. (The
        // first version of this test asserted the message does NOT contain the
        // truncated "e things should not wait" — which the excerpt of a correct
        // message always does, since it quotes the whole opening. An assertion
        // satisfiable only by a message that names nothing.)
        Assert.Contains("Some things should not wait", thrown.Message, StringComparison.Ordinal);

        // And the message has to send the caller somewhere, or the throw just
        // moves the problem from a silent wrong answer to a stuck author.
        Assert.Contains("ReaderTextOfBody", thrown.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>The second condition, and the reason there are two.</b> Checking only
    /// for the CLOSING <c>\n---</c> would wave this through: the fragment
    /// contains one, so <c>IndexOf</c> succeeds, and the helper returns the
    /// fragment cut in half at a markdown horizontal rule where no front matter
    /// ever was. A silent wrong answer of exactly the kind this guard exists to
    /// stop, with different characters.
    /// </summary>
    [Fact]
    public void BodyRefusesAFragmentThatMerelyCONTAINSAFrontMatterDelimiter()
    {
        const string Fragment = "A sentence a reader meets.\n---\nAnother below the rule.";

        var thrown = Assert.Throws<FormatException>(() => CuratedPage.Body(Fragment));

        Assert.Contains("does not OPEN with front matter", thrown.Message, StringComparison.Ordinal);
    }

    /// <summary>Front matter that opens and never closes has no body to return, and used to lose three characters saying so.</summary>
    [Fact]
    public void BodyRefusesAPageWhoseFrontMatterIsNeverClosed()
    {
        const string Unclosed = "---\ntitle: Half-typed\ndescription: and then nothing\n";

        var thrown = Assert.Throws<FormatException>(() => CuratedPage.Body(Unclosed));

        Assert.Contains("never closes it", thrown.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// THE NEGATIVE CONTROL. A guard that only ever demands a throw is
    /// satisfied by a helper that throws on everything, and a helper that
    /// throws on everything fails all 55 pages — which would at least be loud,
    /// but is not what is being claimed here.
    /// </summary>
    [Fact]
    public void BodyStillReturnsTheBodyOfARealPage()
    {
        var page = CuratedPage.Read("tests", "biopsy.md");

        var body = CuratedPage.Body(page);

        Assert.NotEqual(page, body);
        Assert.EndsWith(body, page, StringComparison.Ordinal);
        Assert.DoesNotContain("description:", body, StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>The invariant that discriminates, and the one a suffix check cannot.</b>
    /// The old bug returned <c>page[3..]</c>, which IS a suffix of the input —
    /// so "the body is a suffix of the page" is true of the defect as well and
    /// proves nothing. What is only true of a correct answer: the prefix the
    /// helper removed ENDS at a front-matter delimiter. Under the defect that
    /// prefix is the first three characters of a sentence.
    ///
    /// <para><b>"Ends with <c>---</c>" is not enough on its own, and the break
    /// harness is what said so.</b> The first version of this test asserted
    /// only that — and <c>page[3..]</c> removes exactly the three characters
    /// <c>---</c>, which ends with <c>---</c>. Mutation A4 put the defect back
    /// on whole pages and this test stayed GREEN. The front matter is at
    /// minimum <c>---</c>, a newline, and <c>---</c> again, so all three are
    /// demanded.</para>
    ///
    /// <para>Run over every front-matter file in the corpus, and the count is
    /// asserted so the sweep cannot pass by scanning nothing (WI-435). <b>A
    /// FLOOR PER ROOT, not one total</b>: both <c>AllPages</c> and
    /// <c>SharedSources</c> filter their roots through
    /// <c>.Where(Directory.Exists)</c>, so a renamed <c>Content/blocks</c>
    /// silently yields zero blocks — and a single total of 55 would still be
    /// met by the pages alone, leaving this green having read no block at all
    /// (/review). That is the hole WI-583 closed by pinning an expected SET
    /// instead of a count. Each floor is a floor because the corpus grows.</para>
    /// </summary>
    [Fact]
    public void EveryFrontMatterFileInTheCorpusLosesExactlyItsFrontMatterAndNoProse()
    {
        var read = new List<string>();
        var wrong = new List<string>();

        // Pages + blocks + the 105 glossary entries, which carry the same
        // two-fence shape and are read by the tooltip matcher on every page.
        foreach (var (slug, text) in CuratedPage.AllPages()
                     .Concat(CuratedPage.SharedSources())
                     .DistinctBy(p => p.Slug, StringComparer.Ordinal))
        {
            var removed = text[..^CuratedPage.Body(text).Length];
            if (!removed.StartsWith("---", StringComparison.Ordinal)
                || !removed.EndsWith("---", StringComparison.Ordinal)
                || !removed.Contains('\n'))
            {
                wrong.Add($"{slug}: removed {removed.Length} characters, ending "
                    + removed[^Math.Min(8, removed.Length)..].Replace("\n", "\\n"));
            }
            read.Add(slug);
        }

        Assert.True(wrong.Count == 0,
            "CuratedPage.Body removed something other than the front matter from:\n  "
            + string.Join("\n  ", wrong));

        foreach (var (root, floor) in new[] { ("pages/", 55), ("blocks/", 8), ("glossary/", 100) })
        {
            var found = read.Count(s => s.StartsWith(root, StringComparison.Ordinal));
            Assert.True(found >= floor,
                $"only {found} files were read under {root} (expected at least {floor}), so this "
                + "swept a root it was written against and found almost none of it — the walk is "
                + "broken, not the corpus");
        }
    }

    /// <summary>
    /// The converse refusal. <c>ReaderTextOfBody</c> strips authoring markers
    /// and nothing else, so handed a whole page it returns the YAML as if it
    /// were prose — and §12.8 records that surface making a guard FAIL for the
    /// wrong reason (a front-matter comment quoting a block "retyped" it) and
    /// PASS for the wrong reason (a <c>Contains</c> satisfied by a URL in a
    /// comment).
    /// </summary>
    [Fact]
    public void ReaderTextOfBodyRefusesAWholePage()
    {
        var page = CuratedPage.Read("tests", "biopsy.md");

        var thrown = Assert.Throws<FormatException>(() => CuratedPage.ReaderTextOfBody(page));

        Assert.Contains("WHOLE PAGE", thrown.Message, StringComparison.Ordinal);

        // THE WHOLE PHRASE, not the word. The first version asserted
        // `Contains("ReaderText")`, which cannot fail: the message opens
        // "CuratedPage.ReaderTextOfBody was handed…", so the substring is there
        // whether or not the message points the caller anywhere (/review). Its
        // twin in Body's test is discriminating for the opposite reason —
        // "ReaderTextOfBody" is not a substring of anything else in it.
        Assert.Contains("Use ReaderText for a whole page", thrown.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// <b>THE LIMIT, PINNED.</b> A fragment whose first line is a horizontal
    /// rule and which carries a second one satisfies both of <c>Body</c>'s
    /// conditions, and no string-taking helper can do better: a rule and a fence
    /// are the same three characters in the same position. So <c>Body</c>
    /// ACCEPTS it and slices at the second rule, and
    /// <see cref="CuratedPage.ReaderTextOfBody"/> REFUSES it for looking like a
    /// page — the same residue in opposite directions.
    ///
    /// <para>Neither is live: no file under <c>Content/</c> has a third
    /// <c>^---</c> line, and no fragment any caller in this assembly produces
    /// opens with one, because a section starts after a <c>##</c> heading. This
    /// test exists so that is a DECISION rather than an accident — /review found
    /// the doc comment claiming the two conditions resolved it, and the only
    /// acceptance test for the pair used the one shape that dodges the
    /// ambiguity, which is §12.28's "a control that enters by a different door
    /// than the defect pins nothing".</para>
    /// </summary>
    [Fact]
    public void BodyCannotTellAPageFromAFragmentThatOpensAndClosesWithAHorizontalRule()
    {
        const string Ambiguous = "---\nA rule, then prose.\n---\nMore prose below it.";

        Assert.Equal("\nMore prose below it.", CuratedPage.Body(Ambiguous));
        Assert.Throws<FormatException>(() => CuratedPage.ReaderTextOfBody(Ambiguous));
    }

    /// <summary>
    /// And it accepts a body whose FIRST LINE is a horizontal rule — the same
    /// two-condition reason <see cref="CuratedPage.Body"/> checks the opening
    /// delimiter. A one-condition refusal here would reject a legitimate body
    /// for looking like a page, which is how a rule that fails a correct caller
    /// gets shipped (§12.8, WI-511).
    /// </summary>
    [Fact]
    public void ReaderTextOfBodyAcceptsABodyThatOpensWithAHorizontalRule()
    {
        const string Body = "---\nA rule, then prose, and no closing fence anywhere.";

        var text = CuratedPage.ReaderTextOfBody(Body);

        Assert.Equal(Body, text);
    }

    /// <summary>
    /// It is still the marker-stripping half: <c>!%term%</c> suppressions go,
    /// <c>%%term%%</c> keeps its word. Without this the refusal above could be
    /// satisfied by a helper that throws and otherwise does nothing.
    /// </summary>
    [Fact]
    public void ReaderTextOfBodyStillStripsTheAuthoringMarkers()
    {
        var text = CuratedPage.ReaderTextOfBody("A !%glioma% and a %%tumor board%% in one line.");

        Assert.Equal("A  and a tumor board in one line.", text);
    }

    /// <summary>
    /// THE ROUND TRIP. <c>ReaderText</c> is <c>ReaderTextOfBody(Body(page))</c>,
    /// and after WI-566 those two halves refuse each other's input — so the
    /// composition only works if the split is where the item says it is. A
    /// corpus page in, prose out, no YAML, no throw.
    /// </summary>
    [Fact]
    public void ReaderTextStillComposesTheTwoHalvesOnAWholePage()
    {
        var page = CuratedPage.Read("tumors", "glioma.md");

        var reader = CuratedPage.ReaderText(page);

        Assert.DoesNotContain("sources:", reader, StringComparison.Ordinal);
        Assert.Contains("glioma", reader, StringComparison.OrdinalIgnoreCase);
    }
}
