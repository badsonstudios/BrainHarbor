using System.Text.RegularExpressions;

namespace BrainHarbor.ContentCheck;

/// <summary>
/// Flesch-Kincaid grade level (content-pipeline.md §5): the machine half of
/// the "≤ 8th grade" promise. Grade = 0.39·(words/sentences) +
/// 11.8·(syllables/words) − 15.59. Syllables use the standard vowel-group
/// heuristic — imperfect per word, stable in aggregate, which is what a
/// threshold gate needs.
/// </summary>
public static partial class ReadabilityAnalyzer
{
    [GeneratedRegex(@"[A-Za-z']+")]
    private static partial Regex WordPattern();

    [GeneratedRegex(@"[.!?]+(?=\s|$)")]
    private static partial Regex SentenceEndPattern();

    [GeneratedRegex(@"[aeiouy]+", RegexOptions.IgnoreCase)]
    private static partial Regex VowelGroupPattern();

    // Latin-derived medical words are full of vowel hiatus: gli-O-ma,
    // ra-di-A-tion, di-Ag-no-sis. "i + vowel" is two syllables — except
    // after t/s/c/g ("-tion", "-sion", "-cian", "-gion" collapse to one).
    [GeneratedRegex("(?<![tscg])i[aou]")]
    private static partial Regex HiatusPattern();

    public static double FleschKincaidGrade(string text)
    {
        var words = WordPattern().Matches(text).Select(m => m.Value).ToList();
        if (words.Count == 0)
        {
            return 0;
        }

        var sentences = Math.Max(1, SentenceEndPattern().Matches(text).Count);
        var syllables = words.Sum(CountSyllables);

        var grade = 0.39 * ((double)words.Count / sentences)
                    + 11.8 * ((double)syllables / words.Count)
                    - 15.59;
        return Math.Round(Math.Max(0, grade), 1);
    }

    public static int CountSyllables(string word)
    {
        word = word.Trim('\'').ToLowerInvariant();
        if (word.Length == 0)
        {
            return 0;
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

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';
}
