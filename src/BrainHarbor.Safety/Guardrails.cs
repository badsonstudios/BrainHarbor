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
    /// median 4.7, max 6.5. (4.9 / 6.4 until WI-416 — stale, and contradicted by
    /// §5, §5a and PROGRESS.md, which all record 4.7 / 6.5.) (The old prompt's median was 6.0, measured
    /// block-aware over the 1,038 published items — a different population,
    /// so treat it as a direction, not a like-for-like delta.) Setting the
    /// gate AT the target would flag ordinary variation around it, and a
    /// flagged item does not publish, so the feed would empty into the review
    /// queue instead of getting easier to read. This catches the genuine
    /// outliers, which is what a backstop is for.
    ///
    /// Re-measure against a real pipeline run before tightening further.
    ///
    /// <para><b>IT STAYS AT 7.0 THROUGH WI-416, AND THAT WAS THE SECOND ANSWER.</b>
    /// The unified <see cref="ReadingGrade"/> corrects a grader that had no
    /// vowel-hiatus rule and so under-counted Latin-derived medical syllables. Measured
    /// over the thirteen hand-written ideal summaries in the golden set, the correction
    /// is worth a per-item median of <b>+0.49</b> — and one of them went from 6.61 to
    /// 7.30, through this ceiling, which is how the change was found to have altered a
    /// threshold's MEANING and not only its arithmetic.</para>
    ///
    /// <para><b>The first response was to raise this number to 7.6 and <c>/review</c>
    /// refused it, correctly.</b> The correction is a function of Latin density: ~+0.5 on
    /// medical prose, and <b>~0.00 on plain Germanic prose</b> — which is exactly what
    /// <c>summarize-v4</c> is prompted to produce. A flat +0.6 on the THRESHOLD would
    /// therefore have been a real loosening for the plainest writing, and <c>/review</c>
    /// produced a grammatical plain-English sentence ("The men and women who took the
    /// drug went longer before the growth got bigger and most of the harms they had were
    /// mild") that was flagged at 7.0 and would have passed at 7.6. <b>A number that
    /// looks like a re-calibration on dense prose is a loosening on plain prose, and the
    /// plain prose is the point.</b></para>
    ///
    /// <para><b>What was actually wrong was the ALLOWANCE, not the ceiling.</b> The
    /// yardstick summary that broke reads "schwannomas" and "meningiomas"; the list held
    /// only the singulars, so a summary of a ten-person case series paid full syllable
    /// price for the letter s. The allowance covers regular plurals and the possessive
    /// now (see <see cref="ReadingGradeOptions.MedicalTwoSyllableTerms"/>), which is the
    /// allowance doing its stated job rather than the gate doing less of its own.</para>
    ///
    /// <para><b>So the gate is unchanged for plain prose and STRICTER in real terms for
    /// Latin-dense prose</b>, which is the only direction a reading-level backstop for a
    /// cognitively-impaired audience should ever move by accident.
    /// <c>ReadingGradeTests.NothingPassesNowThatWasFlaggedBefore</c> pins the half of
    /// that which can be pinned offline. <b>Expect the live flag rate to tick up from
    /// 4.8%</b> — that is the correction working, not a regression, and the next
    /// <c>Category=Live</c> run is the measurement.</para>
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

    /// <summary>
    /// Tokens for the number-word lookup only. <b>This is no longer the grader's
    /// tokenizer</b> (WI-416): it was, and <c>[A-Za-z]+</c> drops the apostrophe, which
    /// is why <c>doesn't</c> counted as two words in a summary and one on a page. Here
    /// the pattern is right — every key in <see cref="NumberWords"/> is a bare word, and
    /// an apostrophe would only ever add noise.
    /// </summary>
    [GeneratedRegex(@"[A-Za-z]+")]
    private static partial Regex Word();

    /// <summary>
    /// Hype the anti-hype framing forbids (content-pipeline.md §9). "cure" is
    /// included, but a clearly NEGATED "cure" ("not a cure", "does not cure")
    /// is allowed — the anti-hype block is *supposed* to say that.
    /// </summary>
    private static readonly string[] BannedPhrases =
        ["breakthrough", "miracle", "game-changer", "game changer", "miraculous", "wonder drug"];

    /// <summary>
    /// Negation, including contractions.
    ///
    /// This was a word LIST, matched against `[A-Za-z]+` tokens — which strip
    /// the apostrophe. So "doesn't" tokenized to "doesn" + "t" and the list's
    /// "doesn't" / "isn't" / "n't" entries could never match anything, ever.
    /// Every contraction therefore read as un-negated: "it isn't a cure" was
    /// flagged as a cure claim, and "this doesn't mean it is a breakthrough" as
    /// hype. The block those sentences live in is *called* "what this doesn't
    /// mean", so that phrasing is about the commonest in the corpus (found
    /// 2026-08-14, Dan reading the queue after the first negation fix).
    ///
    /// A regex, not a token list, because the apostrophe is part of the word.
    /// The suffix branch REQUIRES the apostrophe: bare "\w+nt" would match
    /// "important" and quietly negate "this is an important breakthrough".
    /// Both apostrophes are accepted — a model emits the curly one freely.
    /// </summary>
    [GeneratedRegex(
        @"\b(?:not|no|never|without|nor|neither|cannot|isnt|arent|doesnt|dont|didnt|cant|wont|wasnt|werent|hasnt|havent)\b"
        + @"|\w+n['’]t\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex Negation();

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

    // The medical two-syllable allowance moved to ReadingGradeOptions (WI-416): the
    // list and the only grader that reads it belong together, and keeping a copy here
    // is how two implementations happen in the first place.

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

        // Every phrase gets the negation test, not just "cure".
        //
        // It used to be a bare keyword match for all of them, and only "cure"
        // was allowed to be negated. That punished summaries for doing exactly
        // what the anti-hype design asks: every summary ends with a "what this
        // doesn't mean" block, and the natural sentence to write there is "this
        // is not a breakthrough" or "this is not a game-changer". Those were
        // flagged as hype, held out of Auto publish, and piled into the review
        // queue for a person to approve one at a time (found 2026-08-14, Dan
        // reading his own queue). A genuine "this is a breakthrough" is still
        // caught — the negation check is scoped to the sentence.
        foreach (var phrase in BannedPhrases)
        {
            foreach (Match match in Regex.Matches(
                text, $@"\b{Regex.Escape(phrase)}\b", RegexOptions.IgnoreCase))
            {
                if (!IsNegated(text, match.Index))
                {
                    found.Add(phrase);
                    break;   // one un-negated use is enough to flag the summary
                }
            }
        }

        // "cure" keeps its own pass for the plural: "cures" must be caught too.
        foreach (Match match in Regex.Matches(text, @"\bcures?\b", RegexOptions.IgnoreCase))
        {
            if (!IsNegated(text, match.Index))
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

        return Negation().IsMatch(text[sentenceStart..index]);
    }

    /// <summary>
    /// The reading grade of a summary: <see cref="ReadingGrade"/> under
    /// <see cref="ReadingGradeOptions.AiSummaries"/>, over block-aware sentences.
    ///
    /// <para><b>WI-416 took the arithmetic out of here.</b> This method held a second
    /// Flesch-Kincaid implementation that disagreed with the page one in four ways —
    /// the word pattern, the vowel-hiatus rule, the silent-e rule, and rounding — so
    /// "6th grade" meant two different things depending on which gate measured, and the
    /// 6.0 page limit and the 7.0 backstop below were not comparable numbers.
    /// <see cref="ReadingGrade"/> records which side won each disagreement and why. What
    /// is left here is the two things that are genuinely about summaries: the block-aware
    /// preparation, and the medical-vocabulary allowance, which is now a named option
    /// instead of a fork.</para>
    /// </summary>
    public static double GradeLevel(string text) =>
        ReadingGrade.Of(AsSentences(text), ReadingGradeOptions.AiSummaries);

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

}
