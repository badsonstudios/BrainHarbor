using System.Text.RegularExpressions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-564: <b>the idiom list, promoted, and swept over the COMPOSED page.</b>
///
/// <para><b>WHY THIS FILE EXISTS AND THIRTY PER-PAGE ARRAYS DO NOT REPLACE IT.</b>
/// Thirty test classes each carry their own hand-written idiom array. Nineteen of them
/// ban "straight away", eighteen ban "being sick", sixteen each ban "out of hours" and
/// "A&amp;E", thirteen ban "GP", eleven ban "999" — and
/// every one of them reads the page it belongs to, so the corpus's answer to "is this
/// idiom shipped anywhere?" was thirty independent local answers and no global one — and
/// a page whose own class carried no entry from that family had no answer at all.
/// <c>/tests/getting-ready-for-surgery</c> shipped "on the ward" behind a page-local list
/// of three (nil by mouth, pre-med, day case), and <c>/tumors/craniopharyngioma</c>
/// shipped "passing water" with no idiom array in its class at all — while other pages
/// banned both by name.</para>
///
/// <para><b>AND MOST OF THEM READ THE RAW PAGE, which is WI-537's finding and the
/// structural half of this item.</b> A shared block is spliced in at render time
/// (§12.10, WI-514), so an idiom in <c>blocks/mechanism.md</c> is an idiom on
/// eighteen tumor hubs — and a guard reading the raw page sees the literal string
/// <c>[MECHANISM]</c>. <c>blocks/mechanism.md</c> carried "feeling sick" on its
/// raised-pressure line for the whole life of the corpus and <b>no per-page array could
/// ever have turned red on it</b>. This sweep reads
/// <see cref="CuratedPage.EverythingAReaderMeets"/> — the headline plus the composed
/// body — so the block's words are counted once per page that shows them, which is how
/// many readers meet them.</para>
///
/// <para><b>THE PER-PAGE ARRAYS ARE LEFT ALONE, deliberately.</b> They carry
/// page-specific entries this list has no business holding ("fractious", "late effects
/// clinic", "freephone", "high dependency", "DVLA"), and re-scoping thirty call
/// sites has a far larger blast radius than the hole being closed. That is WI-575's
/// choice in the same situation, made for the same reason (§12.21): <i>swept here rather
/// than by editing the call sites</i>. What they now are is harmless duplication of a
/// rule that holds globally — and where one of them is STRONGER than the shared entry,
/// the shared entry took the stronger form (<c>out[- ]of[- ]hours</c> from
/// <c>ShuntsPageTests</c>, <c>\b999\b</c> rather than the bare string). A promotion that
/// kept the commonest form rather than the best one would be a vote, not a decision.</para>
///
/// <para><b>EVERY ENTRY HAS A CANARY IN BOTH DIRECTIONS, driven off the table itself</b>
/// (§12.18, §12.19, and <c>HeadlineSweepTests</c>' round-4 finding): the corpus is clean
/// after this item, so without canaries this whole file is green over text it might not
/// be reading. A row cannot be added without being canaried, because the canary loop
/// iterates the same array the check does.</para>
/// </summary>
public sealed class BritishIdiomSweepTests
{
    /// <summary>
    /// Everything a reader meets on every curated page: the front-matter title and
    /// description plus the COMPOSED body, flattened.
    ///
    /// <para>The floor lives in the enumerator rather than in one test, because
    /// <c>dotnet test --filter</c> defeats a floor asserted by a sibling and each check
    /// below would otherwise pass just as happily over an empty enumeration (§12.17,
    /// and <c>HeadlineSweepTests</c> /review round 1).</para>
    /// </summary>
    private static List<(string Slug, string Text)> ReaderText()
    {
        var root = CuratedPage.PagesDirectory;
        var pages = new List<(string, string)>();

        foreach (var file in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var slug = Path.GetRelativePath(root, file).Replace('\\', '/')[..^3];
            pages.Add((slug, CuratedPage.EverythingAReaderMeets(File.ReadAllText(file), slug)));
        }

        var curated = pages.Count;

        // THE SHARED FILES IN THEIR OWN RIGHT, not only through composition. A block
        // reaches a reader through the pages that include it, so the sweep above
        // already covers every block any page reaches — but a glossary definition
        // fires as a tooltip SITE-WIDE and lives under neither `pages/` nor any
        // page's composed text, and a block nothing currently includes would rot
        // unswept (§12.8: `SharedSources` is the pair with the widest blast radius).
        foreach (var (slug, raw) in CuratedPage.SharedSources())
        {
            var body = raw.StartsWith("---", StringComparison.Ordinal)
                ? CuratedPage.Body(raw)
                : raw;
            pages.Add(("shared/" + slug, CuratedPage.Flatten(CuratedPage.ReaderTextOfBody(body))));
        }

        // TWO FLOORS, NOT ONE TOTAL (WI-564 /review, and §12.37's per-root rule: one
        // total is met by one root). A single `>= 150` is carried by the 105 glossary
        // files, so ~37 new glossary entries would let all 55 pages vanish silently
        // while the floor stayed green — the exact slack the other floors in this file
        // argue against.
        Assert.True(curated >= 50,
            $"only {curated} curated page(s) built from {root} — every check in this file "
            + "would pass over almost nothing, so this is a failure of the sweep and not "
            + "of the corpus");
        Assert.True(pages.Count - curated >= 100,
            $"only {pages.Count - curated} shared file(s) (blocks + glossary) built — the "
            + "glossary half of this sweep is reading almost nothing");

        return pages;
    }

    /// <summary>
    /// NO PAGE, BLOCK OR GLOSSARY ENTRY USES A BRITISH IDIOM — on the text a reader
    /// actually meets, including every shared block spliced into it.
    ///
    /// <para>This is the item. 83 live occurrences across 31 files when the list was
    /// assembled; the message names the page, the phrase and the US form, because an
    /// idiom failure is a content fix and the author needs the replacement, not a
    /// pattern.</para>
    /// </summary>
    [Fact]
    public void NoReaderFacingTextUsesABritishIdiom()
    {
        var offenders = new List<string>();

        foreach (var (slug, text) in ReaderText())
        {
            offenders.AddRange(CuratedPage.BritishIdiomsIn(text).Select(f => $"{slug}: {f}"));
        }

        Assert.True(offenders.Count == 0,
            "British idiom in text a reader meets. Spelling gates do not catch idiom and "
            + "idiom is what actually misleads (§12.10, WI-563):\n  "
            + string.Join("\n  ", offenders.Distinct()));
    }

    /// <summary>
    /// EVERY ROW OF THE TABLE FIRES, AND EVERY ROW LETS THE US FORM THROUGH.
    ///
    /// <para><b>The second half is the load-bearing one.</b> §12.8's rule is that a guard
    /// which fails a CORRECT page is worse than no guard, and the way this list could be
    /// wrong is not by missing an idiom — it is by refusing the replacement it demands.
    /// Each row carries a <c>Sample</c> and a <c>Fixed</c> which are <b>the same sentence
    /// twice</b>, so a canary cannot drift onto a string that happens to match while
    /// proving nothing about the idiom.</para>
    ///
    /// <para><b>A first version derived the sample FROM the pattern</b>, by stripping the
    /// regex furniture, on the theory that a hand-typed canary can test a different
    /// string from the one the pattern holds. It fell over on the second entry with
    /// nested alternation (<c>out[- ]of[- ]hours</c>, then
    /// <c>(?:catch|catches|catching)</c>) because stripping to the first alternative cuts
    /// a group in half — a regex-parser-shaped job in a test helper. The pair below gets
    /// the same property more cheaply: a sample the pattern does not match fails here,
    /// and a sample unrelated to the idiom cannot also be a <c>Fixed</c> that differs
    /// from it by only the US form.</para>
    /// </summary>
    [Fact]
    public void EveryIdiomRowFiresOnItsSampleAndPassesTheSameSentenceFixed()
    {
        foreach (var idiom in CuratedPage.BritishIdioms)
        {
            Assert.False(string.IsNullOrWhiteSpace(idiom.Sample),
                $"'{idiom.Pattern}' has no sample, so it has never been seen to fire "
                + "(§12.18) — and this file's whole subject is a corpus that is clean");
            Assert.False(string.IsNullOrWhiteSpace(idiom.Fixed),
                $"'{idiom.Pattern}' has no fixed sentence, so nothing proves a page CAN "
                + "be made green — §12.8's half that matters more");
            Assert.NotEqual(idiom.Sample, idiom.Fixed);

            Assert.True(
                Regex.IsMatch(idiom.Sample, idiom.Pattern, RegexOptions.IgnoreCase),
                $"'{idiom.Pattern}' does not match its own sample '{idiom.Sample}'");

            Assert.False(
                Regex.IsMatch(idiom.Fixed, idiom.Pattern, RegexOptions.IgnoreCase),
                $"'{idiom.Pattern}' still fires on '{idiom.Fixed}', which is the SAME "
                + "sentence with the US form in it — so fixing a page cannot make it "
                + "green, and a rule that fails a correct page is worse than no rule");

            // AND ON THE REPLACEMENT IN ISOLATION, because `Fixed` is one sentence and
            // `Instead` is what the failure message tells the author to write. "wake up,
            // or come around" is prose rather than a single phrase, so each alternative
            // is checked.
            foreach (var replacement in idiom.Instead.Split(", or ", StringSplitOptions.TrimEntries))
            {
                Assert.False(
                    Regex.IsMatch($"A page. You may be {replacement} here.", idiom.Pattern,
                        RegexOptions.IgnoreCase),
                    $"'{idiom.Pattern}' fires on its own replacement '{replacement}'");
            }

            Assert.False(string.IsNullOrWhiteSpace(idiom.Why),
                $"'{idiom.Pattern}' carries no reason, and the measurement IS the entry");
        }

        // AND THE SWEEP'S OWN READER WOULD HAVE FLAGGED EVERY SAMPLE — BY NAME. The loop
        // above tests patterns; this tests the thing the corpus is actually run through,
        // which is where a Flatten or an exemption bug would hide.
        //
        // `Contains(pattern)` and not `NotEmpty` (/review): a sample routinely trips a
        // NEIGHBOURING entry too ("go to hospital" contains "in hospital"'s shape, and
        // the out-of-hours sample carries both spellings), so NotEmpty passes when the
        // row under test is the one that failed to fire.
        foreach (var idiom in CuratedPage.BritishIdioms)
        {
            Assert.Contains($"{idiom.Pattern} → {idiom.Instead}",
                CuratedPage.BritishIdiomsIn(idiom.Sample));
        }

        // THE FLOOR, one under the list. The list only grows; with slack, entries could
        // be deleted silently and the sweep would go quiet about whatever they caught
        // (HeadlineSweepTests /review round 3).
        Assert.True(CuratedPage.BritishIdioms.Length >= 17,
            $"the idiom list has {CuratedPage.BritishIdioms.Length} entries and had 17 "
            + "when this sweep was written. The list only grows; a drop means entries "
            + "were deleted, and this sweep would go quiet about whatever they caught.");
    }

    /// <summary>
    /// THE REJECTED CANDIDATES ARE STILL REJECTED FOR CAUSE — the positive control.
    ///
    /// <para><see cref="CuratedPage.RejectedBritishIdioms"/> records patterns left
    /// out because the corpus's live occurrences of them are CORRECT. That is a claim
    /// about the corpus, not a judgement, and a claim about the corpus rots: if
    /// <c>/tests/ct-scan</c> stopped calling the scanner a ring, the reason for leaving
    /// <c>\brings?\b</c> out would have evaporated and nobody would know.</para>
    ///
    /// <para>So each of those is asserted to STILL match. A failure here is not a
    /// content defect — it is an invitation to re-decide, and the message says so. This
    /// is §12.36's shape: the only thing standing between a recorded reason and a stale
    /// one is a test that fails when the reason stops being true.</para>
    /// </summary>
    [Fact]
    public void EveryIdiomRejectedForHavingCorrectLiveUsesStillHasThem()
    {
        var corpus = string.Join(" ", ReaderText().Select(p => p.Text));

        // Driven off the array, not off a second copy of it: the entries rejected for
        // being 0-live say so in their own `Why`, and those cannot be asserted this way.
        //
        // `[1-9]\d*` AND NOT `\d+`, which is the bug this test found in itself on its
        // first run: `\bconsultant\b` is rejected for a reason that has nothing to do
        // with the corpus, its `Why` opens "0 live", and `^\d+ live` swept it into the
        // control — which then demanded the corpus contain a word the entry says it
        // does not. A control whose selector is a text pattern has to read the text
        // precisely.
        var measured = CuratedPage.RejectedBritishIdioms
            .Where(i => Regex.IsMatch(i.Why, @"^[1-9]\d* live", RegexOptions.IgnoreCase))
            .ToList();

        // A FLOOR WITH NO SLACK, counted from the array rather than guessed: seven
        // rejections state a live count today, and `>= 6` would let one of them fall out
        // of the control silently — which is the slack every other floor in this file
        // argues against (/review).
        Assert.True(measured.Count >= 7,
            $"only {measured.Count} rejected idiom(s) state a live count, so this control "
            + "is reading less than it did when it was written — the `Why` text is the "
            + "oracle here, and an entry that stopped stating its count stopped being "
            + "controlled");

        foreach (var idiom in measured)
        {
            Assert.True(
                Regex.IsMatch(corpus, idiom.Pattern, RegexOptions.IgnoreCase),
                $"'{idiom.Pattern}' was left OUT of the shared idiom list because the "
                + $"corpus's live uses of it are correct — {idiom.Why} — and it no longer "
                + "matches anything. The reason has gone stale: either the correct use was "
                + "removed, or this can now be promoted. Re-decide rather than deleting "
                + "this assertion.");
        }
    }

    /// <summary>
    /// THE ONE CORRECT US FORM A BARE BAN WOULD HAVE KILLED, pinned on the two pages
    /// that actually carry it.
    ///
    /// <para>Every per-page idiom array in the suite bans "feeling sick" as a plain
    /// substring. Promoted in that form it fails <c>/tumors/brain-metastases</c>
    /// ("headache, feeling sick to your stomach, a seizure") and
    /// <c>/tumors/cns-germ-cell-tumor</c> ("a dry mouth, and feeling sick to your
    /// stomach") — both shipped, both correct, both the US form this item moved the whole
    /// corpus ONTO. The exemption is the lookahead inside the entry, and this test is
    /// what stops a later tidy-up flattening it back to a substring.</para>
    /// </summary>
    [Fact]
    public void TheUsFormOfFeelingSickIsLiveAndTheGuardLetsItThrough()
    {
        var carriers = ReaderText()
            .Where(p => p.Text.Contains("feeling sick to your stomach",
                StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Slug)
            .ToList();

        // NAMED, not counted. This item put the phrase onto most of the corpus, so a
        // floor is met by pages that gained it this week; these two had it BEFORE, which
        // is what makes them the proof that a substring ban would have failed a shipped
        // page (§12.20 — and a count here would have to be rewritten by the next item
        // that mentions nausea).
        Assert.Contains("tumors/brain-metastases", carriers);
        Assert.Contains("tumors/cns-germ-cell-tumor", carriers);

        var entry = CuratedPage.BritishIdioms.Single(i => i.Pattern.Contains("feeling sick"));

        Assert.Matches(entry.Pattern, "you may be feeling sick for a day or two");
        Assert.DoesNotMatch(entry.Pattern, "headache, feeling sick to your stomach, a seizure");

        // AND ACROSS A HARD WRAP, which is where the first version of every one of these
        // gates has failed (§12.8, WI-511's "a\nlift" in a block on eighteen hubs).
        // Through BritishIdiomsIn, because Flatten is what makes it true.
        Assert.Empty(CuratedPage.BritishIdiomsIn("headache, feeling sick\nto your stomach, a seizure"));
        Assert.NotEmpty(CuratedPage.BritishIdiomsIn("a headache, or feeling\nsick, or a seizure"));
    }

    /// <summary>
    /// THE BLOCK IS SWEPT AS THE PAGES THAT INCLUDE IT, which is the whole of WI-537's
    /// structural finding and cannot be proved by any assertion about the block alone.
    ///
    /// <para>A guard over <c>blocks/mechanism.md</c> read directly would have caught
    /// "feeling sick" too. What it would NOT show is that the phrase was on twenty-two
    /// tumor hubs — and the reason thirty per-page arrays stayed green is that
    /// every one of them reads the raw page, where the block is the literal string
    /// <c>[MECHANISM]</c>. So this asserts the sweep's text is COMPOSED: the block's
    /// words are present on a hub that does not contain them in its own source.</para>
    /// </summary>
    [Fact]
    public void TheSweepReadsBlockProseThroughEveryPageThatIncludesIt()
    {
        const string BlockPhrase = "there is a pattern doctors watch for";

        var raw = CuratedPage.Read("tumors", "glioma.md");
        Assert.Contains("[MECHANISM]", raw, StringComparison.Ordinal);
        Assert.DoesNotContain(BlockPhrase, raw, StringComparison.OrdinalIgnoreCase);

        var swept = ReaderText().ToDictionary(p => p.Slug, p => p.Text);
        Assert.Contains(BlockPhrase, swept["tumors/glioma"], StringComparison.OrdinalIgnoreCase);

        // AND ON MORE THAN ONE, because "composed" and "this one page happens to say it"
        // are the same observation on a single page.
        var hubs = swept.Count(p =>
            p.Key.StartsWith("tumors/", StringComparison.Ordinal)
            && p.Value.Contains(BlockPhrase, StringComparison.OrdinalIgnoreCase));
        Assert.True(hubs >= 10,
            $"the mechanism block's prose reaches only {hubs} hub(s) in the swept text, "
            + "so this sweep is not reading composed pages and WI-537's finding is live "
            + "again");

        // THE NEGATIVE CONTROL FOR THE SAME CLAIM. `ReaderText` must not be reading the
        // front matter as prose: `/tumors/glioma`'s front matter names sources this
        // sweep would otherwise scan, and §12.8 records a guard made to FAIL by a page
        // quoting a block in a front-matter comment.
        Assert.DoesNotContain("review_due:", swept["tumors/glioma"], StringComparison.Ordinal);
    }
}
