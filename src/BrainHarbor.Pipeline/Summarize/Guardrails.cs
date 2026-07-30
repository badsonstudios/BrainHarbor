using System.Text.RegularExpressions;

namespace BrainHarbor.Pipeline.Summarize;

/// <summary>
/// The automated safety checks that gate auto-publish (content-pipeline.md
/// §9/§11). A summary that trips any of these is flagged for a human — in Auto
/// mode it waits in the review queue instead of publishing. Conservative on
/// purpose, but tuned so it flags genuinely-wrong summaries, not correct ones
/// that merely use an unavoidable drug or tumor name.
/// </summary>
public static partial class Guardrails
{
    /// <summary>Reading-level ceiling — the audience may be cognitively impaired.</summary>
    public const double MaxGradeLevel = 8.5;

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

    public sealed record Result(bool Passed, IReadOnlyList<string> Reasons);

    /// <summary>Runs every check. summaryText = assembled summary,
    /// sourceText = original title + abstract.</summary>
    public static Result Check(string summaryText, string sourceText)
    {
        var reasons = new List<string>();

        var untraceable = UntraceableNumbers(summaryText, sourceText);
        if (untraceable.Count > 0)
        {
            reasons.Add($"numbers not found in the source: {string.Join(", ", untraceable)}");
        }

        var banned = BannedWordsIn(summaryText);
        if (banned.Count > 0)
        {
            reasons.Add($"banned hype phrase(s): {string.Join(", ", banned)}");
        }

        var grade = GradeLevel(summaryText);
        if (grade > MaxGradeLevel)
        {
            reasons.Add($"reading level {grade:0.0} is above {MaxGradeLevel}");
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

    /// <summary>True if a negation word appears within the few words before
    /// <paramref name="index"/> (e.g. "not a cure", "does not cure").</summary>
    private static bool IsNegated(string text, int index)
    {
        var before = text[..index];
        var words = Word().Matches(before).Select(m => m.Value).TakeLast(4).ToList();
        return words.Any(w => Negations.Contains(w, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>Flesch-Kincaid grade level, with a medical-vocabulary allowance
    /// (WI-106 uses the same formula for static pages).</summary>
    public static double GradeLevel(string text)
    {
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
