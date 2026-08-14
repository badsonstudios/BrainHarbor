using System.Text;
using System.Text.RegularExpressions;

namespace BrainHarbor.Safety;

/// <summary>
/// The automated safety checks that gate auto-publish (content-pipeline.md
/// §9/§11). A summary that trips any of these is flagged for a human — in Auto
/// mode it waits in the review queue instead of publishing. Conservative on
/// purpose, but tuned so it flags genuinely-wrong summaries, not correct ones
/// that merely use an unavoidable drug or tumor name.
/// </summary>
public static partial class Guardrails
{
    /// <summary>
    /// Reading-level ceiling — the audience may be cognitively impaired.
    ///
    /// 7.0, not the 6.0 the pages are held to (WI-414/415), because the PROMPT
    /// is the mechanism and this is only the backstop. `summarize-v4` asks for
    /// 6th grade and delivers it: measured live over 8 golden-set items,
    /// median 4.9, max 6.4. (The old prompt's median was 6.0, measured
    /// block-aware over the 1,038 published items — a different population,
    /// so treat it as a direction, not a like-for-like delta.) Setting the
    /// gate AT the target would flag ordinary variation around it, and a
    /// flagged item does not publish, so the feed would empty into the review
    /// queue instead of getting easier to read. This catches the genuine
    /// outliers, which is what a backstop is for.
    ///
    /// Re-measure against a real pipeline run before tightening further.
    /// </summary>
    public const double MaxGradeLevel = 7.0;

    // Numbers in a summary must trace to the source. Matches integers,
    // decimals, and percentages; commas in thousands are normalized out.
    [GeneratedRegex(@"\d[\d,]*\.?\d*")]
    private static partial Regex Number();

    // Sentence end followed by whitespace/end, so a decimal ("27.7") or an
    // abbreviation dot doesn't inflate the sentence count and understate grade.
    [GeneratedRegex(@"[.!?]+(?=\s|$)")]
    private static partial Regex SentenceEnd();

    [GeneratedRegex(@"[A-Za-z]+")]
    private static partial Regex Word();

    /// <summary>
    /// Hype the anti-hype framing forbids (content-pipeline.md §9). "cure" is
    /// included, but a clearly NEGATED "cure" ("not a cure", "does not cure")
    /// is allowed — the anti-hype block is *supposed* to say that.
    /// </summary>
    private static readonly string[] BannedPhrases =
        ["breakthrough", "miracle", "game-changer", "game changer", "miraculous", "wonder drug"];

    private static readonly string[] Negations =
        ["not", "no", "never", "without", "isn't", "isnt", "aren't", "arent", "doesn't", "doesnt", "don't", "dont", "n't"];

    /// <summary>Number words → digits, so "Ten studies" (source) matches "10"
    /// (summary), and a spelled-out invented number is still caught.</summary>
    private static readonly Dictionary<string, string> NumberWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["one"] = "1", ["two"] = "2", ["three"] = "3", ["four"] = "4", ["five"] = "5",
        ["six"] = "6", ["seven"] = "7", ["eight"] = "8", ["nine"] = "9", ["ten"] = "10",
        ["eleven"] = "11", ["twelve"] = "12", ["thirteen"] = "13", ["fourteen"] = "14",
        ["fifteen"] = "15", ["sixteen"] = "16", ["seventeen"] = "17", ["eighteen"] = "18",
        ["nineteen"] = "19", ["twenty"] = "20", ["thirty"] = "30", ["forty"] = "40",
        ["fifty"] = "50", ["sixty"] = "60", ["seventy"] = "70", ["eighty"] = "80",
        ["ninety"] = "90", ["hundred"] = "100", ["thousand"] = "1000",
    };

    /// <summary>
    /// Common brain-tumor vocabulary counted as 2 syllables for the reading
    /// level, so a required drug/tumor name (glioblastoma, bevacizumab) doesn't
    /// push an otherwise-plain summary over the ceiling. The surrounding prose
    /// is still measured normally.
    /// </summary>
    private static readonly HashSet<string> MedicalTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "glioma", "glioblastoma", "astrocytoma", "oligodendroglioma", "meningioma",
        "medulloblastoma", "ependymoma", "craniopharyngioma", "schwannoma", "hemangioblastoma",
        "metastasis", "metastases", "metastatic", "radiosurgery", "radionecrosis",
        "chemotherapy", "immunotherapy", "radiotherapy", "temozolomide", "bevacizumab",
        "vorasidenib", "ivosidenib", "stereotactic", "intracranial", "leptomeningeal",
        "progression", "recurrence", "diagnosis", "diagnosed", "biomarker", "molecular",
    };

    /// <summary>
    /// Which check flagged an item (WI-417). The reason text has always been
    /// logged per item, but nothing counted the kinds — so a run could report
    /// "4.8% flagged" without saying whether that was reading level, invented
    /// numbers, or hype. An enum rather than string-matching the message,
    /// because the tally must not break the next time the wording is improved.
    /// </summary>
    public enum FlagKind
    {
        InventedNumbers,
        BannedHype,
        ReadingLevel,
    }

    /// <summary>One tripped check: the kind for counting, the message for reading.</summary>
    public sealed record Flag(FlagKind Kind, string Message)
    {
        public override string ToString() => Message;
    }

    public sealed record Result(bool Passed, IReadOnlyList<Flag> Reasons);

    /// <summary>Plain-language name for a run summary line.</summary>
    public static string Describe(FlagKind kind) => kind switch
    {
        FlagKind.InventedNumbers => "invented numbers",
        FlagKind.BannedHype => "hype phrases",
        FlagKind.ReadingLevel => "reading level",
        _ => kind.ToString(),
    };

    /// <summary>Runs every check. summaryText = assembled summary,
    /// sourceText = original title + abstract.</summary>
    public static Result Check(string summaryText, string sourceText)
    {
        var reasons = new List<Flag>();

        var untraceable = UntraceableNumbers(summaryText, sourceText);
        if (untraceable.Count > 0)
        {
            reasons.Add(new Flag(
                FlagKind.InventedNumbers,
                $"numbers not found in the source: {string.Join(", ", untraceable)}"));
        }

        var banned = BannedWordsIn(summaryText);
        if (banned.Count > 0)
        {
            reasons.Add(new Flag(
                FlagKind.BannedHype,
                $"banned hype phrase(s): {string.Join(", ", banned)}"));
        }

        var grade = GradeLevel(summaryText);
        if (grade > MaxGradeLevel)
        {
            reasons.Add(new Flag(
                FlagKind.ReadingLevel,
                $"reading level {grade:0.0} is above {MaxGradeLevel}"));
        }

        return new Result(reasons.Count == 0, reasons);
    }

    /// <summary>
    /// Digit numbers in the summary that don't appear in the source — the
    /// classic hallucination (an invented "62%"). The SOURCE set includes
    /// spelled-out numbers as digits, so a source that says "Ten studies"
    /// matches a summary that says "10". The summary side only checks digits:
    /// spelled words like "one"/"two" are far more often articles than counts,
    /// so flagging them is noise — a spelled hallucination is left to the human
    /// gate, a digit one is caught here.
    /// </summary>
    public static IReadOnlyList<string> UntraceableNumbers(string summaryText, string sourceText)
    {
        var sourceNumbers = SourceNumbers(sourceText);

        var untraceable = new List<string>();
        foreach (Match m in Number().Matches(summaryText))
        {
            var value = Normalize(m.Value);
            if (value.Length > 0 && !sourceNumbers.Contains(value))
            {
                untraceable.Add(m.Value);
            }
        }
        return untraceable;
    }

    private static HashSet<string> SourceNumbers(string text)
    {
        var set = Number().Matches(text).Select(m => Normalize(m.Value)).ToHashSet();
        foreach (Match m in Word().Matches(text))
        {
            if (NumberWords.TryGetValue(m.Value, out var digits))
            {
                set.Add(digits);
            }
        }
        return set;
    }

    public static IReadOnlyList<string> BannedWordsIn(string text)
    {
        var found = new List<string>();
        foreach (var phrase in BannedPhrases)
        {
            if (Regex.IsMatch(text, $@"\b{Regex.Escape(phrase)}\b", RegexOptions.IgnoreCase))
            {
                found.Add(phrase);
            }
        }

        // "cure" only when it is NOT negated — a plain "a cure for glioma" is
        // hype; "this is not a cure" is the anti-hype block doing its job.
        foreach (Match m in Regex.Matches(text, @"\bcures?\b", RegexOptions.IgnoreCase))
        {
            if (!IsNegated(text, m.Index))
            {
                found.Add("cure");
            }
        }

        return found.Distinct().ToList();
    }

    /// <summary>
    /// True if "cure" is negated somewhere earlier in its own sentence — the
    /// anti-hype block, which is exactly what we want to allow ("this is not a
    /// cure", "it is not a promise of a cure", "this does not mean it is a
    /// cure"). A fixed few-word window missed the longer, natural phrasings and
    /// false-flagged legitimate anti-hype summaries, holding most items in Auto
    /// mode. Scope to the current sentence so a negation in a PRIOR sentence
    /// can't excuse a fresh affirmative cure claim.
    /// </summary>
    private static bool IsNegated(string text, int index)
    {
        var before = text[..index];

        var sentenceStart = 0;
        foreach (Match end in SentenceEnd().Matches(before))
        {
            sentenceStart = end.Index + end.Length;
        }

        // A block boundary ends a sentence too, even without a full stop
        // (WI-415 — the same defect the grader had). Otherwise a title reading
        // "this is not a cure" would excuse a hype claim in the hook below it.
        var lastBreak = before.LastIndexOf('\n');
        if (lastBreak >= sentenceStart)
        {
            sentenceStart = lastBreak + 1;
        }

        var clause = text[sentenceStart..index];
        return Word().Matches(clause).Any(m => Negations.Contains(m.Value, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>Flesch-Kincaid grade level, with a medical-vocabulary allowance
    /// (WI-106 uses the same formula for static pages).</summary>
    public static double GradeLevel(string text)
    {
        // Block-aware (WI-415), matching ContentChecker.ExtractSentences: the
        // plain title and each template block arrive newline-separated, and a
        // title almost never ends in a full stop — so grading the raw run
        // merged the title into the hook and made one long sentence out of
        // two short ones. Measured over the 1,038 published summaries, that
        // inflated the median by 0.7 of a grade (6.7 reported vs 6.0 real).
        text = AsSentences(text);

        var sentences = Math.Max(1, SentenceEnd().Matches(text).Count);
        var words = Word().Matches(text).Select(m => m.Value).ToList();
        if (words.Count == 0)
        {
            return 0;
        }

        var syllables = words.Sum(CountSyllables);
        return (0.39 * words.Count / sentences)
             + (11.8 * syllables / words.Count)
             - 15.59;
    }

    /// <summary>A block boundary is a sentence boundary: each block gets a
    /// terminator when the writer left it off (blocks may hold several
    /// sentences of their own). Blank lines are dropped rather than counted.</summary>
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

    // "1,383" -> "1383", "331." (end of sentence) -> "331", "0.15" -> "0.15".
    private static string Normalize(string number) =>
        number.Replace(",", "").TrimEnd('.');

    private static int CountSyllables(string word)
    {
        // Required medical terms don't get penalized for their length.
        if (MedicalTerms.Contains(word))
        {
            return 2;
        }

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
}
