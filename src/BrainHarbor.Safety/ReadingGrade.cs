using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace BrainHarbor.Safety;

/// <summary>
/// WI-416: which population is being graded, and therefore which allowance applies.
///
/// <para><b>AN OPTION, NOT A FORK.</b> Until this item there were two Flesch-Kincaid
/// implementations — <c>ReadabilityAnalyzer</c> for curated pages and
/// <c>Guardrails.GradeLevel</c> for AI summaries — and they disagreed in four ways
/// nobody had chosen (see <see cref="ReadingGrade"/>). "6th grade" meant two different
/// things depending on which one measured, so the 6.0 page limit and the 7.0 summary
/// backstop were <b>not comparable numbers</b> and no statement of the form "summaries
/// are allowed one grade more than pages" was true.</para>
///
/// <para>Now there is one implementation and the only legitimate difference between the
/// two populations is named here, with the reason it exists.</para>
/// </summary>
/// <para><b>A sealed class with a PRIVATE constructor, and both halves are /review
/// findings.</b> It was a positional <c>record</c>, whose constructor is public — so
/// <c>new ReadingGradeOptions(whatever)</c> could invent a third population anywhere in
/// the solution, which is the exact failure this type's own docstring claims to prevent.
/// There are two option sets and there is no way to make a third. (Record equality was
/// wrong here too: a record whose only member is a set compares by reference, so two
/// option sets holding the same 31 terms were unequal.)</para>
public sealed class ReadingGradeOptions
{
    private ReadingGradeOptions(IEnumerable<string> twoSyllableTerms) =>
        TwoSyllableTerms = twoSyllableTerms.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Words counted as two syllables regardless of their spelling (and their regular
    /// plural and possessive — see <see cref="ReadingGrade"/>). Empty for curated pages
    /// and populated for AI summaries.
    ///
    /// <para><b>A <see cref="FrozenSet{T}"/> and not an <c>IReadOnlySet</c> over a
    /// <c>HashSet</c>, because /review downcast the old one and added a term:</b>
    /// <c>((HashSet&lt;string&gt;)CuratedPages.TwoSyllableTerms).Add("glioblastoma")</c>
    /// compiled and silently gave every curated page an allowance, process-wide. A
    /// safety gate's "read-only" has to be read-only in the type, not by convention —
    /// and a frozen set is the faster shape for a collection that is only ever looked
    /// up in.</para>
    /// </summary>
    public FrozenSet<string> TwoSyllableTerms { get; }

    /// <summary>
    /// Curated pages, Razor copy, glossary definitions and front-matter descriptions:
    /// <b>no allowance at all</b>, and the 6.0 limit is held against that.
    ///
    /// <para>A page is WRITTEN by a person who can choose the words. Where it has to use
    /// a long one it can say it once and then say "this tumor" — which is house style
    /// (§4) — so an allowance here would excuse prose that a rewrite should fix. That is
    /// the opposite of the summary case: a summary has to name the drug in the study it
    /// is about.</para>
    /// </summary>
    public static readonly ReadingGradeOptions CuratedPages = new([]);

    /// <summary>
    /// AI summaries: brain-tumor vocabulary counts as two syllables, so a required
    /// drug or tumor name does not push an otherwise-plain summary over the backstop.
    /// The surrounding prose is still measured normally.
    ///
    /// <para><b>The allowance exists because a summary cannot choose its nouns.</b> An
    /// item about bevacizumab in glioblastoma must say both words; refusing to publish
    /// it over their syllable count would empty the feed into the review queue rather
    /// than make anything easier to read, which is the same argument
    /// <see cref="Guardrails.MaxGradeLevel"/>'s docstring makes for the 7.0 itself.</para>
    ///
    /// <para><b>What it is worth, measured rather than assumed (WI-416):</b> see
    /// <c>ReadingGradeTests.WhatTheSummaryAllowanceIsWorthOnTheShippedCorpus</c>, which
    /// grades 183 samples both ways — the 55 page bodies, the 105 glossary definitions
    /// and the 23 golden-set source abstracts — and pins the spread. §5a of
    /// content-pipeline.md states the numbers, including which of them are measured on
    /// text no gate actually grades.</para>
    /// </summary>
    /// <remarks>
    /// <b>Assigned in the static constructor below, so the declaration order of the two
    /// fields cannot matter.</b> It did matter for one round: this field was a
    /// <c>static readonly</c> initialiser reading <see cref="MedicalTwoSyllableTerms"/>,
    /// which is declared after it, and <c>static readonly</c> initialisers run in
    /// declaration order — so <c>TwoSyllableTerms</c> was <c>null</c>, silently and with
    /// no compiler warning, and every summary grade threw a
    /// <c>NullReferenceException</c> the first time one was measured. Twelve tests caught
    /// it at once, which is the only reason it cost minutes rather than a release.
    /// <c>NoOptionSetHasANullTermList</c> is the guard if anybody turns it back into a
    /// field initialiser. (/review: the first version of this remark opened by claiming
    /// the field was declared AFTER the list, which it is not — a comment about an
    /// ordering trap, with the ordering wrong.)
    /// </remarks>
    public static readonly ReadingGradeOptions AiSummaries;

    /// <summary>
    /// The vocabulary <see cref="AiSummaries"/> allows at two syllables.
    ///
    /// <para>Moved out of <c>Guardrails</c> by WI-416 so the list and the grader that
    /// reads it live together, and so a reader of the page side can see what the page
    /// side does NOT get.</para>
    /// </summary>
    /// <remarks>
    /// <b><c>OrdinalIgnoreCase</c> is belt-and-braces and the break harness proved it:
    /// making the comparer case-SENSITIVE changes nothing</b>, because
    /// <see cref="ReadingGrade.Syllables"/> lowercases the token before the lookup.
    /// Recorded rather than removed — it states the list's intent where the list is
    /// declared, and it is the thing that keeps the allowance working if a future caller
    /// looks a term up without lowercasing first. The lowercasing is what actually does
    /// the work today, and <c>no-lowercasing</c> is the mutation that reds.
    /// </remarks>
    public static readonly IReadOnlySet<string> MedicalTwoSyllableTerms =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "glioma", "glioblastoma", "astrocytoma", "oligodendroglioma", "meningioma",
            "medulloblastoma", "ependymoma", "craniopharyngioma", "schwannoma", "hemangioblastoma",
            "metastasis", "metastases", "metastatic", "radiosurgery", "radionecrosis",
            "chemotherapy", "immunotherapy", "radiotherapy", "temozolomide", "bevacizumab",
            "vorasidenib", "ivosidenib", "stereotactic", "intracranial", "leptomeningeal",
            "progression", "recurrence", "diagnosis", "diagnosed", "biomarker", "molecular",
        };

    // A STATIC CONSTRUCTOR, NOT A FIELD INITIALISER, so the order of the two fields
    // above cannot matter at all. The ordering comment on `AiSummaries` records what
    // declaring it first cost; this is the version where there is nothing to get wrong.
    static ReadingGradeOptions()
    {
        AiSummaries = new(MedicalTwoSyllableTerms);
    }
}

/// <summary>
/// WI-416: <b>the one Flesch-Kincaid implementation.</b>
/// Grade = 0.39·(words/sentences) + 11.8·(syllables/words) − 15.59.
///
/// <para>Syllables use the standard vowel-group heuristic — imperfect per word, stable
/// in aggregate, which is what a threshold gate needs.</para>
///
/// <para><b>THE FOUR DISAGREEMENTS THIS REPLACES, and which side won each.</b> Two
/// implementations existed and the differences were accidents of two authors, not
/// decisions, so each one is settled here on its merits and the losing behaviour is
/// recorded so nobody reintroduces it as a "fix":</para>
/// <list type="number">
/// <item><b>The word pattern.</b> Pages used <c>[A-Za-z']+</c>, summaries
/// <c>[A-Za-z]+</c>, so <c>doesn't</c> was ONE word on a page and TWO in a summary —
/// inflating the summary's word count and deflating its syllables-per-word. The page
/// side wins: an apostrophe is part of an English word, and <c>Guardrails</c>'s own
/// <c>Negation</c> docstring records the identical tokenising mistake costing a live
/// defect (every contraction read as un-negated).</item>
/// <item><b>The vowel-hiatus rule.</b> Pages counted <c>i</c> + vowel as a second
/// syllable except after <c>t/s/c/g</c> (gli-O-ma, ra-di-A-tion, di-Ag-no-sis, but
/// -tion/-sion/-cian/-gion collapse); summaries had no such rule and under-counted
/// every Latin-derived medical word. The page side wins: this corpus is Latin-derived
/// medical vocabulary, and a grader that systematically under-counts it reports the
/// hardest prose on the site as the easiest.</item>
/// <item><b>The silent trailing <c>e</c>.</b> Pages required length &gt; 2 and exempted
/// consonant + <c>le</c> (so <c>table</c> and <c>little</c> keep their syllable);
/// summaries decremented whenever the count was above 1, making <c>table</c> one
/// syllable. The page side wins on being right.</item>
/// <item><b>Rounding and clamping.</b> Pages returned
/// <c>Round(Max(0, grade), 1)</c>; summaries returned the raw double. The page side
/// wins, and this is the one that makes the thresholds mean something: <b>6.0 and 7.0
/// are one-decimal numbers</b>, so a gate comparing an unrounded 6.0000001 against 6.0
/// fails a page that reads 6.0 — a verdict nobody can act on. And a 12-word sentence of
/// one-syllable words scores below zero; reporting −2.3 as a reading grade is not a
/// reading grade.</item>
/// </list>
///
/// <para><b>WHAT IS NOT IN HERE, deliberately: turning text into sentences.</b> That
/// step legitimately differs by population — Markdig for curated markdown, a Razor
/// extractor for <c>.cshtml</c>, and line-splitting for a summary's title-plus-blocks
/// (WI-415) — and all three exist to produce the same thing: one terminated sentence
/// per block. This method grades prepared prose and has no opinion about where it came
/// from. Folding any of those in would be a fifth fork wearing an option.</para>
/// </summary>
public static partial class ReadingGrade
{
    /// <summary>
    /// <b>Both apostrophes</b> (/review). <c>[A-Za-z']+</c> alone excludes U+2019, so
    /// <c>doesn’t</c> was still two words and <c>bevacizumab’s</c> still missed the
    /// allowance — and <c>Guardrails.Negation</c>'s own docstring says "a model emits the
    /// curly one freely", about the same text this grades. Fixing the straight one and
    /// not the curly one is the disagreement half-settled.
    /// </summary>
    [GeneratedRegex(@"[A-Za-z'’]+")]
    private static partial Regex WordPattern();

    /// <summary>
    /// A sentence end followed by whitespace or end-of-text, so a decimal ("27.7") or an
    /// abbreviation dot does not inflate the sentence count and understate the grade.
    /// Both implementations already agreed on this one.
    /// </summary>
    [GeneratedRegex(@"[.!?]+(?=\s|$)")]
    private static partial Regex SentenceEndPattern();

    [GeneratedRegex(@"[aeiouy]+", RegexOptions.IgnoreCase)]
    private static partial Regex VowelGroupPattern();

    // Latin-derived medical words are full of vowel hiatus: gli-O-ma, ra-di-A-tion,
    // di-Ag-no-sis. "i + vowel" is two syllables — except after t/s/c/g ("-tion",
    // "-sion", "-cian", "-gion" collapse to one).
    [GeneratedRegex("(?<![tscg])i[aou]")]
    private static partial Regex HiatusPattern();

    /// <summary>
    /// The grade, rounded to one decimal and clamped at 0 — the number a threshold is
    /// compared against and the number that gets printed, which must be the same number.
    /// </summary>
    public static double Of(string text, ReadingGradeOptions options)
    {
        var words = WordPattern().Matches(text).Select(m => m.Value).ToList();
        if (words.Count == 0)
        {
            return 0;
        }

        var sentences = Math.Max(1, SentenceEndPattern().Matches(text).Count);
        var syllables = words.Sum(word => Syllables(word, options));

        var grade = 0.39 * ((double)words.Count / sentences)
                    + 11.8 * ((double)syllables / words.Count)
                    - 15.59;
        return Math.Round(Math.Max(0, grade), 1);
    }

    /// <summary>Syllables in one word, under <paramref name="options"/>' allowance.</summary>
    public static int Syllables(string word, ReadingGradeOptions options)
    {
        word = word.Trim('\'', '’').ToLowerInvariant();
        if (word.Length == 0)
        {
            return 0;
        }

        // THE ALLOWANCE COVERS THE FORMS OF ITS OWN TERMS: the plural and the
        // possessive, not just the dictionary headword (WI-416, after /review).
        //
        // THE PLURAL IS THE ONE THAT MATTERED, and the project's own yardstick is the
        // evidence. `schwannoma` and `meningioma` are on the list; the ideal summary for
        // 42107211 says "schwannomas" and "meningiomas", because that is what a summary
        // of a ten-person series says. Neither plural was allowed, the summary measured
        // 7.30 against a 7.0 backstop under the corrected grader, and
        // `GoldenSetTests.EveryIdealSummaryPassesTheWI304Guardrails` went red — a
        // summary this project WROTE BY HAND as the standard, failing its own gate over
        // the letter s.
        //
        // The first response to that red was to raise the backstop to 7.6. /review
        // refused it, and was right: the grader correction is worth ~+0.5 on Latin-dense
        // prose and ~0.00 on plain Germanic prose, so a flat +0.6 on the THRESHOLD is a
        // real loosening for exactly the plain writing the prompt is built to produce —
        // and /review produced a grammatical plain-English sentence that was flagged at
        // 7.0 before and would pass at 7.6. The allowance was the thing that was wrong,
        // not the ceiling.
        //
        // THE POSSESSIVE, for a different reason: adopting the page side's `[A-Za-z']+`
        // word pattern is what made it a question at all. The old summary grader
        // tokenized with `[A-Za-z]+`, which DROPS the apostrophe, so "bevacizumab's"
        // arrived at the list as "bevacizumab" and matched. Taken naively this item would
        // have silently ENDED the allowance for every possessive while claiming to change
        // only the word count. Measured on the text a gate actually grades, that is two
        // occurrences (`chemotherapy's`, twice, in `pages/treatments/targeted-therapy.md`)
        // and none in the ideal summaries — /review corrected a first draft of this
        // comment that counted twelve by including YAML comments and front matter no
        // grader reads. It stays because it preserves the old grader's behaviour and
        // costs nothing, not because the corpus needs it today.
        //
        // IRREGULAR FORMS ARE STILL NOT COVERED and the list carries them itself:
        // "metastasis" and "metastases" are separate entries, because -es off "metastases"
        // is not a word. An unlisted word like "radiation" is not allowed and should not
        // be — it is ordinary English and four syllables is the honest count.
        if (IsAllowed(word, options))
        {
            return 2;
        }

        var count = VowelGroupPattern().Matches(word).Count
                    + HiatusPattern().Matches(word).Count;

        // Silent trailing e ("change", "gene", "while") — but consonant+"le"
        // endings ("little", "table") keep their syllable.
        if (word.Length > 2 && word.EndsWith('e') &&
            !(word.EndsWith("le") && !IsVowel(word[^3])))
        {
            count--;
        }

        return Math.Max(1, count);
    }

    /// <summary>
    /// Is <paramref name="word"/> on the allowance list, as itself or with a possessive
    /// or a regular plural taken off?
    ///
    /// <para><b>THE WORD ITSELF IS TRIED FIRST, and the first version of this did not
    /// do that — it stripped a suffix and returned, so a LISTED term ending in a
    /// strippable suffix stopped matching.</b> <c>metastases</c> is on the list, ends in
    /// "es", became "metastas", missed, and the ideal summary for 42388439 ("brain
    /// metastases", twice) jumped from 7.0 to 7.3 — a fix for one yardstick summary
    /// breaking a different one, in the same run. Caught by the measurement this item
    /// already had, which is the argument for having it.</para>
    ///
    /// <para>Bounded on purpose — three suffixes, no stemmer. A stemmer would start
    /// allowing words the list never named, and the list is the whole point: a short,
    /// reviewed set of nouns a summary cannot avoid, not a rule about long words.</para>
    /// </summary>
    private static bool IsAllowed(string word, ReadingGradeOptions options)
    {
        if (options.TwoSyllableTerms.Contains(word))
        {
            return true;
        }

        // `word.Length > suffix.Length` is what keeps the slice safe, and it has to be
        // strict: a token of nothing but a suffix ("es") would otherwise look up "" and
        // an empty entry in the list would allow it.
        foreach (var suffix in Inflections)
        {
            if (word.Length > suffix.Length
                && word.EndsWith(suffix, StringComparison.Ordinal)
                && options.TwoSyllableTerms.Contains(word[..^suffix.Length]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Longest first, so the possessive is tried before the bare plural — otherwise
    /// "bevacizumab's" loses its s and nothing else, and still misses the list. Both
    /// apostrophes, for the reason <see cref="WordPattern"/> gives.
    /// </summary>
    private static readonly string[] Inflections = ["'s", "’s", "es", "s"];

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';
}
