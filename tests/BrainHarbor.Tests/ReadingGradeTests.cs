using BrainHarbor.ContentCheck;
using BrainHarbor.Safety;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using Xunit.Abstractions;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-416: <b>"6th grade" has to mean one thing.</b>
///
/// <para>Two Flesch-Kincaid implementations existed — <c>ReadabilityAnalyzer</c> for
/// curated pages and <c>Guardrails.GradeLevel</c> for AI summaries — and they disagreed
/// in four ways nobody had chosen, so the 6.0 page limit and the 7.0 summary backstop
/// were not comparable numbers. <see cref="ReadingGrade"/> is the one implementation and
/// its docstring records which side won each disagreement.</para>
///
/// <para><b>This file holds both pre-WI-416 implementations verbatim and measures
/// against them.</b> That is the only honest way to say what a unification cost: a
/// refactor's claim is "nothing moved except what I said would move", and the way to
/// prove it is to keep the thing that moved away from.</para>
/// </summary>
public partial class ReadingGradeTests(ITestOutputHelper output)
{
    // ---------- the two implementations this item replaced, verbatim ----------

    [GeneratedRegex(@"[.!?]+(?=\s|$)")]
    private static partial Regex SentenceEnd();

    /// <summary>The pre-WI-416 page grader (`ReadabilityAnalyzer`), unchanged.</summary>
    private static double LegacyPageGrade(string text)
    {
        var words = Regex.Matches(text, @"[A-Za-z']+").Select(m => m.Value).ToList();
        if (words.Count == 0)
        {
            return 0;
        }

        var sentences = Math.Max(1, SentenceEnd().Matches(text).Count);
        var syllables = words.Sum(LegacyPageSyllables);

        var grade = 0.39 * ((double)words.Count / sentences)
                    + 11.8 * ((double)syllables / words.Count)
                    - 15.59;
        return Math.Round(Math.Max(0, grade), 1);
    }

    private static int LegacyPageSyllables(string word)
    {
        word = word.Trim('\'').ToLowerInvariant();
        if (word.Length == 0)
        {
            return 0;
        }

        var count = Regex.Matches(word, @"[aeiouy]+", RegexOptions.IgnoreCase).Count
                    + Regex.Matches(word, "(?<![tscg])i[aou]").Count;

        if (word.Length > 2 && word.EndsWith('e') &&
            !(word.EndsWith("le") && !"aeiouy".Contains(word[^3])))
        {
            count--;
        }

        return Math.Max(1, count);
    }

    /// <summary>The pre-WI-416 summary grader (`Guardrails.GradeLevel`), unchanged —
    /// `[A-Za-z]+` words, no vowel hiatus, the looser silent-e rule, no rounding.</summary>
    private static double LegacySummaryGrade(string text)
    {
        text = AsSentences(text);

        var sentences = Math.Max(1, SentenceEnd().Matches(text).Count);
        var words = Regex.Matches(text, @"[A-Za-z]+").Select(m => m.Value).ToList();
        if (words.Count == 0)
        {
            return 0;
        }

        var syllables = words.Sum(LegacySummarySyllables);
        return (0.39 * words.Count / sentences)
             + (11.8 * syllables / words.Count)
             - 15.59;
    }

    /// <summary>
    /// The 31 terms as the pre-WI-416 <c>Guardrails</c> held them, <b>copied rather than
    /// read from the live list</b> (/review).
    ///
    /// <para>The legacy grader read <c>ReadingGradeOptions.MedicalTwoSyllableTerms</c>
    /// for a round, so adding one term would silently have redefined what "the old
    /// grader did" and moved every drift number in this file. A verbatim baseline whose
    /// behaviour can change is not a baseline.</para>
    /// </summary>
    private static readonly HashSet<string> LegacyMedicalTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "glioma", "glioblastoma", "astrocytoma", "oligodendroglioma", "meningioma",
        "medulloblastoma", "ependymoma", "craniopharyngioma", "schwannoma", "hemangioblastoma",
        "metastasis", "metastases", "metastatic", "radiosurgery", "radionecrosis",
        "chemotherapy", "immunotherapy", "radiotherapy", "temozolomide", "bevacizumab",
        "vorasidenib", "ivosidenib", "stereotactic", "intracranial", "leptomeningeal",
        "progression", "recurrence", "diagnosis", "diagnosed", "biomarker", "molecular",
    };

    private static int LegacySummarySyllables(string word)
    {
        if (LegacyMedicalTerms.Contains(word))
        {
            return 2;
        }

        // (The legacy grader tokenized with `[A-Za-z]+`, so it never saw an apostrophe
        // and never needed a possessive branch. Reproduced faithfully: this is what the
        // old numbers were, not what they should have been.)

        word = word.ToLowerInvariant();
        var count = 0;
        var previousVowel = false;
        foreach (var c in word)
        {
            var isVowel = "aeiouy".Contains(c);
            if (isVowel && !previousVowel)
            {
                count++;
            }
            previousVowel = isVowel;
        }

        if (word.EndsWith('e') && count > 1)
        {
            count--;
        }

        return Math.Max(1, count);
    }

    /// <summary>`Guardrails.AsSentences`, unchanged — block preparation, not grading.</summary>
    private static string AsSentences(string text)
    {
        var builder = new StringBuilder(text.Length + 16);
        foreach (var line in text.Split('\n'))
        {
            var block = line.Trim();
            if (block.Length == 0)
            {
                continue;
            }

            builder.Append(block);
            if (block[^1] is not ('.' or '!' or '?'))
            {
                builder.Append('.');
            }

            builder.Append(' ');
        }

        return builder.ToString();
    }

    // ---------- the measurement corpus ----------

    /// <summary>
    /// Every piece of real medical prose this repository can measure offline: the 55
    /// curated page bodies as ContentCheck grades them, the 105 glossary definitions, and
    /// the golden set's source abstracts — which are what the summariser actually reads.
    ///
    /// <para><b>The published summaries are NOT in here and cannot be.</b> They live in
    /// the production database; the only offline route to them is a live model run
    /// (`SummaryReadingLevelLiveTests`, opt-in, costs subscription). So every number this
    /// file reports about the summary grader is measured on medical prose of the right
    /// kind and the wrong provenance, and it says so rather than implying otherwise.</para>
    /// </summary>
    private static List<(string Name, string Text)> Corpus()
    {
        var samples = new List<(string, string)>();
        var pagesRoot = Path.Combine(CuratedPage.RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages");

        foreach (var file in Directory.EnumerateFiles(pagesRoot, "*.md", SearchOption.AllDirectories)
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            var raw = File.ReadAllText(file).Replace("\r\n", "\n");
            var body = raw.StartsWith("---\n", StringComparison.Ordinal)
                ? raw[(raw.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..]
                : raw;
            samples.Add((
                "page:" + Path.GetRelativePath(pagesRoot, file).Replace('\\', '/'),
                ContentChecker.ExtractSentences(body)));
        }

        foreach (var file in Directory.EnumerateFiles(CuratedPage.GlossaryRoot, "*.md")
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            var raw = File.ReadAllText(file).Replace("\r\n", "\n");
            var body = raw.StartsWith("---\n", StringComparison.Ordinal)
                ? raw[(raw.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..]
                : raw;
            samples.Add(("glossary:" + Path.GetFileNameWithoutExtension(file), body.Trim()));
        }

        using var golden = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            CuratedPage.RepoRoot(), "tests", "BrainHarbor.Tests", "GoldenSet", "golden-set.json")));
        foreach (var item in golden.RootElement.GetProperty("items").EnumerateArray())
        {
            var input = item.GetProperty("input");
            var abstractText = input.TryGetProperty("raw_summary", out var raw) ? raw.GetString() : null;
            if (!string.IsNullOrWhiteSpace(abstractText))
            {
                samples.Add(("abstract:" + input.GetProperty("external_id").GetString(), abstractText));
            }
        }

        return samples;
    }

    /// <summary>
    /// <b>The thirteen hand-written ideal summaries from the golden set — the right
    /// population, and the one the first version of this measurement missed.</b>
    ///
    /// <para>The corpus above is page prose and source abstracts; these are SUMMARIES,
    /// written by hand as the yardstick the model is held to, in exactly the
    /// title-plus-blocks shape `Guardrails.GradeLevel` grades. They are the closest
    /// thing to the published feed that exists offline, and
    /// `GoldenSetTests.EveryIdealSummaryPassesTheWI304Guardrails` holds them to the 7.0
    /// backstop — so a change to the summary grader shows up here as a red, which is how
    /// WI-416 found out it had moved a threshold's meaning rather than only its
    /// arithmetic.</para>
    /// </summary>
    private static List<(string Name, string Text)> IdealSummaries()
    {
        using var golden = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            CuratedPage.RepoRoot(), "tests", "BrainHarbor.Tests", "GoldenSet", "golden-set.json")));

        var samples = new List<(string, string)>();
        foreach (var item in golden.RootElement.GetProperty("items").EnumerateArray())
        {
            if (!item.TryGetProperty("ideal_summary", out var ideal) ||
                ideal.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var prose = string.Join("\n", new[]
                {
                    "plain_title", "what_studied", "what_found", "means", "doesnt_mean",
                }
                .Select(key => ideal.GetProperty(key).GetString()));

            samples.Add(("ideal:" + item.GetProperty("input").GetProperty("external_id").GetString(),
                prose));
        }

        return samples;
    }

    /// <summary>
    /// <b>THE RE-MEASUREMENT THE ACCEPTANCE CRITERIA ASKED FOR, on the right
    /// population.</b>
    ///
    /// <para>"The existing thresholds re-measured against it and adjusted if the numbers
    /// move." They moved: the unified grader reads medical prose harder, and on the
    /// thirteen ideal summaries the shift pushed one of them (42107211) from 6.9 to 7.3,
    /// through a 7.0 backstop it had been inside. That red is the finding, not an
    /// accident — and it is why `MaxGradeLevel` could not simply be left alone and
    /// declared unaffected.</para>
    ///
    /// <para>The distribution is printed and the maximum is pinned, so the next change to
    /// either the grader or the yardstick has to come back and look.</para>
    /// </summary>
    [Fact]
    public void TheIdealSummariesAreTheRightPopulationForTheBackstop()
    {
        var graded = IdealSummaries()
            .Select(s => (s.Name,
                Old: LegacySummaryGrade(s.Text),
                New: Guardrails.GradeLevel(s.Text)))
            .OrderByDescending(s => s.New)
            .ToList();

        Assert.Equal(13, graded.Count);

        output.WriteLine($"ideal summaries: {graded.Count}   backstop {Guardrails.MaxGradeLevel:0.0}");
        output.WriteLine($"old  median {Median(graded.Select(g => g.Old)):0.00}  max {graded.Max(g => g.Old):0.00}");
        output.WriteLine($"new  median {Median(graded.Select(g => g.New)):0.00}  max {graded.Max(g => g.New):0.00}");
        foreach (var g in graded)
        {
            output.WriteLine($"  {g.Old:0.0} -> {g.New:0.0}  {(g.New > Guardrails.MaxGradeLevel ? "FLAG" : "    ")}  {g.Name}");
        }

        // EVERY ONE OF THEM UNDER THE BACKSTOP. The yardstick failing its own gate means
        // either the gate or the yardstick is wrong (GoldenSetTests says the same thing
        // from the other side), and that is a decision to make deliberately rather than
        // a number to nudge.
        Assert.All(graded, g => Assert.True(g.New <= Guardrails.MaxGradeLevel,
            $"{g.Name} reads {g.New:0.0} against a backstop of {Guardrails.MaxGradeLevel:0.0}"));

        // THE NUMBERS THE RULING RESTS ON, PINNED (/review: they were printed and
        // never asserted, so editing the golden set could falsify every figure in §5a
        // and in MaxGradeLevel's docstring with the suite green).
        Assert.Equal(5.08, Median(graded.Select(g => g.Old)), 2);
        Assert.Equal(5.70, Median(graded.Select(g => g.New)), 2);
        Assert.Equal(6.61, graded.Max(g => g.Old), 2);
        Assert.Equal(0.44, Median(graded.Select(g => g.New - g.Old)), 2);

        // AND THE TIGHTEST ITEM, BY NAME AND VALUE. `ideal:42388439` reads EXACTLY at
        // the backstop under the corrected grader. It passes — `Check` compares with
        // `>` — but with no margin at all, which is a finding about the yardstick and
        // not about the gate: a hand-written exemplar sitting on a 7.0 ceiling, for an
        // audience that may be cognitively impaired, when the target is 6.0.
        //
        // NOT FIXED HERE, AND NOT PAPERED OVER EITHER. Rewriting an exemplar is
        // editorial work on the project's yardstick, which is /pm's call, not a grader
        // item's. What this item owes is to make it visible and to make it a tripwire:
        // this assertion reds the moment anything moves that summary in either
        // direction, and whoever sees it reads this comment.
        var tightest = graded.First();
        Assert.Equal("ideal:42388439", tightest.Name);
        Assert.Equal(Guardrails.MaxGradeLevel, tightest.New, 2);
    }

    /// <summary>
    /// <b>NOTHING PASSES NOW THAT WAS FLAGGED BEFORE.</b>
    ///
    /// <para>This is the safety claim of the whole item, and <c>/review</c> blocked its
    /// first version for being unpinnable. <see cref="Guardrails.MaxGradeLevel"/> does
    /// not move, so the claim is one-directional and testable: the corrected grader
    /// reads medical prose harder, therefore the set of texts it flags at 7.0 is a
    /// SUPERSET of the set the old grader flagged at 7.0. New flags are the correction
    /// working. <b>A text that was flagged and now passes would be a loosening</b>, and
    /// there must be none.</para>
    ///
    /// <para><b>The first draft of this item raised the ceiling to 7.6 and this is the
    /// assertion that would have refused it.</b> <c>/review</c> found a grammatical
    /// plain-English sentence — no medical vocabulary, so the grader correction is worth
    /// about nothing on it — that graded 7.54 under the old rules and 7.50 under the
    /// new: flagged at 7.0, passing at 7.6. It is a fixture here, because the band the
    /// claim is about has to be exercised by something and the 183-sample corpus has
    /// nothing in 7.0–7.6.</para>
    /// </summary>
    [Fact]
    public void NothingPassesNowThatWasFlaggedBefore()
    {
        var texts = Corpus().Concat(IdealSummaries()).Concat(NearTheCeiling()).ToList();

        var loosened = texts
            .Select(s => (s.Name,
                Old: LegacySummaryGrade(s.Text),
                New: Guardrails.GradeLevel(s.Text)))
            .Where(s => s.Old > Guardrails.MaxGradeLevel && s.New <= Guardrails.MaxGradeLevel)
            .Select(s => $"{s.Name}: {s.Old:0.00} (flagged) -> {s.New:0.00} (passes)")
            .ToList();

        Assert.True(loosened.Count == 0,
            "these texts were flagged before WI-416 and pass after it:\n"
            + string.Join("\n", loosened));

        // AND THE FIXTURES REALLY ARE IN THE BAND, or the assertion above is satisfied
        // by an empty intersection and proves nothing — which is exactly what /review
        // found the first version doing (thirteen `false == false`).
        var straddlers = NearTheCeiling()
            .Count(s => LegacySummaryGrade(s.Text) > Guardrails.MaxGradeLevel);
        Assert.True(straddlers >= 2,
            $"only {straddlers} fixture(s) were flagged by the OLD grader, so this test "
            + "is not exercising the band it is about");
    }

    /// <summary>
    /// Plain-English, summary-shaped prose that the OLD grader put just above 7.0 — the
    /// band in which a threshold move is a loosening. No listed medical terms, so the
    /// grader correction is worth ~0.00 on them: whatever a ceiling change does to
    /// these, it does for no reason connected to this item.
    /// </summary>
    private static List<(string Name, string Text)> NearTheCeiling() =>
    [
        ("near:plain-run-on",
            "The men and women who took the drug went longer before the growth got "
            + "bigger and most of the harms they had were mild."),
        ("near:plain-two-clause",
            "The people in the study who were given the newer drug stayed about the "
            + "same for longer than the people who were given the older one."),
    ];

    // ---------- the refactor's own safety proof ----------

    /// <summary>
    /// <b>THE PAGE SIDE MOVED BY EXACTLY NOTHING, proved file by file.</b>
    ///
    /// <para>The unified grader adopted the page implementation's answer to all four
    /// disagreements, so this is a claim the refactor can be held to absolutely rather
    /// than statistically: every one of the 55 page bodies, 105 glossary definitions and
    /// 23 abstracts grades to the same number as before, and the 6.0 gate therefore has
    /// not moved under any shipped page. ContentCheck reporting 345/0 says the gate still
    /// passes; this says the numbers behind it are the same numbers.</para>
    /// </summary>
    [Fact]
    public void ThePageGraderIsBitIdenticalToTheOneItReplaced()
    {
        var moved = Corpus()
            .Where(s => ReadingGrade.Of(s.Text, ReadingGradeOptions.CuratedPages)
                        != LegacyPageGrade(s.Text))
            .Select(s => $"{s.Name}: {LegacyPageGrade(s.Text):0.0} -> "
                         + $"{ReadingGrade.Of(s.Text, ReadingGradeOptions.CuratedPages):0.0}")
            .ToList();

        Assert.True(moved.Count == 0,
            "the page grader was supposed to be unchanged:\n" + string.Join("\n", moved));
    }

    /// <summary>
    /// <b>WHAT THE UNIFICATION COST THE SUMMARY SIDE.</b>
    ///
    /// <para>The summary grader DID change — that is the item — and the four changes all
    /// push the same way on medical prose: the vowel-hiatus rule adds syllables to every
    /// Latin-derived word, which is most of this vocabulary. So the new grader reads
    /// medical prose HARDER than the old one did, and a backstop calibrated against the
    /// old one is looser than it looks.</para>
    ///
    /// <para>The spread is pinned, not just printed, so the next change to the grader has
    /// to re-measure rather than discover this by accident.</para>
    /// </summary>
    [Fact]
    public void WhatTheUnificationCostTheSummaryGrader()
    {
        var deltas = Corpus()
            .Select(s => (s.Name,
                Old: LegacySummaryGrade(s.Text),
                New: ReadingGrade.Of(AsSentences(s.Text), ReadingGradeOptions.AiSummaries)))
            .Select(s => (s.Name, s.Old, s.New, Delta: s.New - s.Old))
            .OrderByDescending(s => s.Delta)
            .ToList();

        output.WriteLine($"samples: {deltas.Count}");
        output.WriteLine($"delta  min {deltas.Min(d => d.Delta):0.00}  "
                         + $"median {Median(deltas.Select(d => d.Delta)):0.00}  "
                         + $"max {deltas.Max(d => d.Delta):0.00}");
        output.WriteLine($"old    median {Median(deltas.Select(d => d.Old)):0.00}  "
                         + $"max {deltas.Max(d => d.Old):0.00}");
        output.WriteLine($"new    median {Median(deltas.Select(d => d.New)):0.00}  "
                         + $"max {deltas.Max(d => d.New):0.00}");
        foreach (var d in deltas.Take(5))
        {
            output.WriteLine($"  +{d.Delta:0.00}  {d.Name}  {d.Old:0.0} -> {d.New:0.0}");
        }

        Assert.All(deltas, d => Assert.InRange(d.Delta, MinSummaryDrift, MaxSummaryDrift));

        // AND THE MEDIAN, which is the statistic a threshold decision rests on: a
        // per-sample range is satisfied by a distribution that has shifted wholesale.
        Assert.InRange(Median(deltas.Select(d => d.Delta)), 0, MaxMedianSummaryDrift);
    }

    /// <summary>
    /// <b>WHAT THE SUMMARY ALLOWANCE IS WORTH, measured rather than asserted.</b>
    ///
    /// <para>This is the number §5 of content-pipeline.md needs in order to say anything
    /// comparative about 6.0 and 7.0: the allowance is the ONLY difference between the two
    /// populations' graders now, so the gap between the two thresholds is only meaningful
    /// against the gap the allowance itself opens on the same text.</para>
    /// </summary>
    [Fact]
    public void WhatTheSummaryAllowanceIsWorthOnTheShippedCorpus()
    {
        var pairs = Corpus()
            .Select(s => (s.Name,
                NoAllowance: ReadingGrade.Of(s.Text, ReadingGradeOptions.CuratedPages),
                Allowance: ReadingGrade.Of(s.Text, ReadingGradeOptions.AiSummaries)))
            .Select(s => (s.Name, s.NoAllowance, s.Allowance, Worth: s.NoAllowance - s.Allowance))
            .OrderByDescending(s => s.Worth)
            .ToList();

        output.WriteLine($"samples: {pairs.Count}");
        output.WriteLine($"allowance worth  min {pairs.Min(p => p.Worth):0.00}  "
                         + $"median {Median(pairs.Select(p => p.Worth)):0.00}  "
                         + $"max {pairs.Max(p => p.Worth):0.00}");
        foreach (var p in pairs.Take(5))
        {
            output.WriteLine($"  -{p.Worth:0.00}  {p.Name}  {p.NoAllowance:0.0} -> {p.Allowance:0.0}");
        }

        // It can only ever make text easier: the allowance replaces a syllable count
        // with 2, and no listed term is shorter than two syllables by the heuristic.
        Assert.All(pairs, p => Assert.True(p.Worth >= 0,
            $"{p.Name}: the allowance made the grade WORSE ({p.NoAllowance:0.0} -> {p.Allowance:0.0})"));

        // The max AND the median, because the docstring on these constants quotes both
        // and /review found only the max was pinned. The median being 0.00 is the
        // substantive fact: most prose contains no listed term at all, so the allowance
        // is not a general loosening — it is a carve-out for a named vocabulary.
        Assert.InRange(pairs.Max(p => p.Worth), 0, MaxAllowanceWorth);
        Assert.Equal(0, Median(pairs.Select(p => p.Worth)));
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        return sorted.Count % 2 == 1
            ? sorted[sorted.Count / 2]
            : (sorted[(sorted.Count / 2) - 1] + sorted[sorted.Count / 2]) / 2;
    }

    /// <summary>
    /// The measured bounds (WI-416, 183 samples: 55 page bodies, 105 glossary
    /// definitions, 23 source abstracts). Drift is min <b>-0.08</b>, median
    /// <b>+0.12</b>, max <b>+1.01</b>; the allowance is worth median <b>0.00</b> and
    /// max <b>5.60</b>. The constants sit a little outside those so an ordinary content
    /// edit cannot red them, and close enough that a change to the GRADER has to come
    /// back here and re-measure. §5 of content-pipeline.md carries the numbers.
    /// </summary>
    private const double MinSummaryDrift = -0.3;

    /// <inheritdoc cref="MinSummaryDrift"/>
    private const double MaxSummaryDrift = 1.3;

    /// <inheritdoc cref="MinSummaryDrift"/>
    private const double MaxMedianSummaryDrift = 0.25;

    /// <inheritdoc cref="MinSummaryDrift"/>
    private const double MaxAllowanceWorth = 6.0;

    // ---------- the four disagreements, one test each ----------

    /// <summary>
    /// <b>An apostrophe is part of an English word.</b> <c>[A-Za-z]+</c> made
    /// <c>doesn't</c> two words, which inflated a summary's word count and deflated its
    /// syllables-per-word — and <c>Guardrails.Negation</c>'s docstring records the
    /// identical tokenising mistake costing a live defect, every contraction reading as
    /// un-negated.
    /// </summary>
    [Fact]
    public void AContractionIsOneWord()
    {
        // Against the SAME words with the apostrophes dropped, which both patterns
        // tokenize identically — so the two grades are equal if and only if the
        // apostrophe did not split a word. ("doesn't" and "doesnt" also have the same
        // vowel groups, so nothing else can move.)
        const string WithApostrophes =
            "It doesn't help and it isn't a cure and it won't change the plan the "
            + "doctors wrote down for the person who has this tumor.";
        const string Without =
            "It doesnt help and it isnt a cure and it wont change the plan the "
            + "doctors wrote down for the person who has this tumor.";

        var contracted = ReadingGrade.Of(WithApostrophes, ReadingGradeOptions.CuratedPages);

        Assert.Equal(ReadingGrade.Of(Without, ReadingGradeOptions.CuratedPages), contracted);

        // Above the clamp, or the equality above is 0 == 0 and says nothing — which is
        // what the first version of this test actually asserted.
        Assert.True(contracted > 0, $"grade was {contracted:0.0}, so the clamp hid the test");

        // And genuinely shorter than the spelled-out form: three contractions removed
        // are three words.
        Assert.True(
            contracted < ReadingGrade.Of(
                WithApostrophes.Replace("doesn't", "does not")
                    .Replace("isn't", "is not").Replace("won't", "will not"),
                ReadingGradeOptions.CuratedPages),
            "a contraction must read as the shorter sentence it is");
    }

    /// <summary>
    /// <b>The vowel-hiatus rule, which the summary grader did not have.</b> gli-o-ma is
    /// three syllables and the old summary grader said two, so it reported the hardest
    /// vocabulary on the site as the easiest. The exceptions matter as much as the rule:
    /// -tion / -sion / -cian / -gion collapse.
    /// </summary>
    [Theory]
    [InlineData("glioma", 3)]
    [InlineData("radiation", 4)]
    [InlineData("diagnosis", 4)]
    [InlineData("vision", 2)]
    [InlineData("physician", 3)]
    [InlineData("region", 2)]
    public void VowelHiatusIsCountedAndItsExceptionsAreNot(string word, int expected)
    {
        Assert.Equal(expected, ReadingGrade.Syllables(word, ReadingGradeOptions.CuratedPages));
    }

    /// <summary>
    /// <b>The silent trailing e, and the consonant+le exception.</b> The summary grader
    /// decremented whenever the count was above 1, which made <c>table</c> one syllable.
    /// </summary>
    [Theory]
    [InlineData("change", 1)]
    [InlineData("gene", 1)]
    [InlineData("table", 2)]
    [InlineData("little", 2)]
    [InlineData("the", 1)]
    [InlineData("be", 1)]
    public void TheSilentEIsDroppedExceptAfterAConsonantAndL(string word, int expected)
    {
        Assert.Equal(expected, ReadingGrade.Syllables(word, ReadingGradeOptions.CuratedPages));
    }

    /// <summary>
    /// <b>One decimal and never below zero — the change that makes 6.0 and 7.0 mean
    /// something.</b> The summary grader returned a raw double, so a gate comparing
    /// 6.0000001 against 6.0 failed a page that reads 6.0, and a short plain sentence
    /// reported a negative grade.
    /// </summary>
    [Fact]
    public void TheGradeIsOneDecimalAndNeverNegative()
    {
        Assert.Equal(0, ReadingGrade.Of("The cat sat on the mat.", ReadingGradeOptions.CuratedPages));

        var grade = ReadingGrade.Of(
            "Investigators subsequently determined that stratification necessitated "
            + "additional multivariable adjustment procedures across heterogeneous cohorts.",
            ReadingGradeOptions.CuratedPages);
        Assert.Equal(Math.Round(grade, 1), grade);
        Assert.True(grade > 0);
    }

    // ---------- the allowance, as an option ----------

    /// <summary>
    /// <b>A CURATED PAGE GETS NO ALLOWANCE, and that is the whole reason the option
    /// exists.</b> A page is written by a person who can choose the words; a summary has
    /// to name the drug in the study it is about.
    /// </summary>
    [Fact]
    public void ACuratedPageGetsNoAllowanceAndASummaryDoes()
    {
        Assert.Empty(ReadingGradeOptions.CuratedPages.TwoSyllableTerms);
        Assert.NotEmpty(ReadingGradeOptions.AiSummaries.TwoSyllableTerms);

        Assert.Equal(5, ReadingGrade.Syllables("glioblastoma", ReadingGradeOptions.CuratedPages));
        Assert.Equal(2, ReadingGrade.Syllables("glioblastoma", ReadingGradeOptions.AiSummaries));
    }

    /// <summary>
    /// <b>THE POSSESSIVE KEEPS THE ALLOWANCE.</b>
    ///
    /// <para>Adopting the page side's <c>[A-Za-z']+</c> is what made this a question:
    /// the old summary grader's <c>[A-Za-z]+</c> dropped the apostrophe, so
    /// "bevacizumab's" reached the list as "bevacizumab" and matched. Taken naively,
    /// this item would have silently ENDED the allowance for every possessive while
    /// claiming to change only the word count.</para>
    ///
    /// <para>Measured, not assumed: the shipped corpus carries twelve possessives of
    /// listed terms and the golden set one more.</para>
    /// </summary>
    [Theory]
    [InlineData("bevacizumab's")]
    [InlineData("meningioma's")]
    [InlineData("chemotherapy's")]
    public void APossessiveOfAnAllowedTermKeepsTheAllowance(string word)
    {
        Assert.Equal(2, ReadingGrade.Syllables(word, ReadingGradeOptions.AiSummaries));

        // AND THE PLURAL, which is the form that actually mattered: the yardstick summary
        // for 42107211 says "schwannomas" and "meningiomas", the list held only the
        // singulars, and that one letter put a hand-written exemplar through the backstop.
        Assert.Equal(2, ReadingGrade.Syllables("schwannomas", ReadingGradeOptions.AiSummaries));
        Assert.Equal(2, ReadingGrade.Syllables("meningiomas", ReadingGradeOptions.AiSummaries));

        // A LISTED TERM THAT ENDS IN A STRIPPABLE SUFFIX IS STILL ALLOWED, which the
        // first version of the inflection code broke: it stripped and returned, so
        // "metastases" became "metastas", missed the list it is ON, and pushed a
        // different yardstick summary from 7.0 to 7.3 in the same run.
        Assert.Equal(2, ReadingGrade.Syllables("metastases", ReadingGradeOptions.AiSummaries));

        // And an unlisted word is not allowed, suffix or no suffix. "radiation" is
        // ordinary English and four syllables is the honest count.
        Assert.True(ReadingGrade.Syllables("radiation", ReadingGradeOptions.AiSummaries) > 2);
        Assert.True(ReadingGrade.Syllables("complications", ReadingGradeOptions.AiSummaries) > 2);
    }

    /// <summary>
    /// <b>BOTH APOSTROPHES.</b> <c>[A-Za-z']+</c> excludes U+2019, so <c>doesn’t</c> was
    /// still two words and <c>bevacizumab’s</c> still lost the allowance — and
    /// <c>Guardrails.Negation</c>'s own docstring says "a model emits the curly one
    /// freely", about the same text the summary gate grades. Fixing the straight one and
    /// not the curly one is the disagreement half-settled (/review).
    /// </summary>
    [Fact]
    public void TheCurlyApostropheIsAnApostropheToo()
    {
        Assert.Equal(
            ReadingGrade.Syllables("bevacizumab's", ReadingGradeOptions.AiSummaries),
            ReadingGrade.Syllables("bevacizumab’s", ReadingGradeOptions.AiSummaries));

        const string Straight =
            "It doesn't help and it isn't a cure and it won't change the plan the "
            + "doctors wrote down for the person who has this tumor.";

        Assert.Equal(
            ReadingGrade.Of(Straight, ReadingGradeOptions.CuratedPages),
            ReadingGrade.Of(Straight.Replace('\'', '’'), ReadingGradeOptions.CuratedPages));

        // Above the clamp, or both sides are 0 and the equality says nothing.
        Assert.True(ReadingGrade.Of(Straight, ReadingGradeOptions.CuratedPages) > 0);
    }

    /// <summary>
    /// The allowance is matched case-insensitively, so a sentence-initial term is still
    /// allowed.
    /// </summary>
    [Fact]
    public void TheAllowanceIsNotCaseSensitive()
    {
        Assert.Equal(2, ReadingGrade.Syllables("Glioblastoma", ReadingGradeOptions.AiSummaries));
    }

    /// <summary>
    /// <b>Neither option set has a null term list.</b>
    ///
    /// <para>This is not hypothetical. <c>AiSummaries</c> was declared ABOVE the list it
    /// reads, <c>static readonly</c> initialisers run in declaration order, and
    /// <c>TwoSyllableTerms</c> was therefore <c>null</c> with no compiler warning — every
    /// summary grade threw on the first measurement. The field is built in a static
    /// constructor now so there is no order to get wrong, and this is the guard that
    /// notices if it goes back to a field initialiser.</para>
    /// </summary>
    [Fact]
    public void NoOptionSetHasANullTermList()
    {
        foreach (var options in new[]
                 { ReadingGradeOptions.CuratedPages, ReadingGradeOptions.AiSummaries })
        {
            Assert.NotNull(options.TwoSyllableTerms);
            Assert.Equal(0, ReadingGrade.Syllables("", options));
            Assert.True(ReadingGrade.Of("A plain sentence about a tumor.", options) >= 0);
        }
    }

    /// <summary>
    /// <b>THERE IS ONE IMPLEMENTATION, and this is what says so.</b> <c>Guardrails</c>
    /// must not grow its own arithmetic back: its grade has to be exactly
    /// <see cref="ReadingGrade"/> under the summary option, over block-aware sentences.
    /// A behavioural identity rather than a source check, so a reimplementation that
    /// happens to agree today is still caught the day it drifts.
    /// </summary>
    [Fact]
    public void TheSummaryGateIsTheSharedGraderAndNotASecondOne()
    {
        foreach (var (name, text) in Corpus())
        {
            Assert.Equal(
                ReadingGrade.Of(AsSentences(text), ReadingGradeOptions.AiSummaries),
                Guardrails.GradeLevel(text));
            Assert.True(Guardrails.GradeLevel(text) >= 0, name);
        }
    }
}
