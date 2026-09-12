using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-525: X12, anti-seizure medicines. The seventh TREATMENT page under the
/// §12.8 LIBRARY template (the shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs), and the sibling of /treatments/steroids: the two
/// halves of "this might be the drug, not the tumor".
///
/// The prose is reviewed, not tested. What is pinned here is where this page
/// could leave a reader worse off than no page:
///
///   * THE SCOPE TRAP, which is the sharp end of this item. The guideline's
///     Level A is about a newly diagnosed person who has NOT had a seizure. The
///     same guideline says the evidence around SURGERY is insufficient either
///     way. A page that prints the first without the second tells a reader
///     whose team gave them levetiracetam for an operation that their team
///     broke a rule (§12.12).
///   * a reader deciding at home that the mood change is the medicine and
///     acting on it. The thing the tablet is stopping has not gone anywhere, so
///     a dose lowered in the kitchen is how a seizure gets back in. Telling the
///     team is the only move this page may produce.
///   * "never stop suddenly", which is true of the reader who has had a seizure
///     and false of the post-operative course the guideline says to stop after
///     a week — one item after /treatments/steroids found the identical shape.
///   * a driving rule. Jurisdictional, and /seizures/living-with#driving owns
///     the framing. No waiting time, no state, no "you can drive when".
///   * a frequency for the mood effect. Seven figures across seven
///     populations (see the front matter), so none of them.
///
/// Every claim-shaped guard below carries a CANARY: a known-bad sentence it
/// must match before its pass on the real page means anything (§12.8, WI-523).
/// Guards that could be walked around by moving a sentence one section down run
/// PAGE-WIDE with a short, named allowlist (§12.8, WI-524).
/// </summary>
public sealed class AntiSeizureMedicinesPageContentTests
{
    private static string Page => CuratedPage.Read("treatments", "anti-seizure-medicines.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    /// <summary>
    /// A section as the reader meets it. Not <c>CuratedPage.ReaderText</c>,
    /// which strips front matter by index and returns <c>section[3..]</c> when
    /// fed a section (§12.8, WI-520). Both marker shapes, as the renderer
    /// strips them.
    /// </summary>
    private static string Reader(string section) =>
        Regex.Replace(Regex.Replace(section, @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The title and description, which <c>ReaderText</c> strips and every
    /// body-scoped guard below is therefore blind to — while
    /// <c>ContentPage.cshtml</c> renders the description as the first paragraph
    /// under the heading (§12.8, WI-524, where the description asserted the
    /// exact claim its page existed to correct).
    /// </summary>
    private static string Headline
    {
        get
        {
            var front = CuratedPage.FrontMatter(Page);
            // `\s*$` before the anchor, not `"$`: on a CRLF checkout the line
            // ends `"\r\n`, the match fails, and every guard over the headline
            // then runs on an empty string (§12.8, WI-524).
            var title = Regex.Match(front, @"(?m)^title: ""(.+)""\s*$").Groups[1].Value;
            var description = Regex.Match(front, @"(?m)^description: ""(.+)""\s*$").Groups[1].Value;
            Assert.False(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description),
                "the title or description could not be read, so the guards over them prove nothing");
            return title + " " + description;
        }
    }

    private static string Body => CuratedPage.Flatten(CuratedPage.ReaderText(Page));

    /// <summary>Body plus the two lines the body-scoped guards cannot see.</summary>
    private static string Everything => Headline + " " + Body;

    /// <summary>
    /// A `### ` subsection alone. <c>CuratedPage.Section</c> only splits on
    /// <c>## </c>.
    /// </summary>
    private static string Subsection(string parent, string heading)
    {
        var outer = Regex.Match(Page, @"^## " + Regex.Escape(parent) + @".*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        var sub = Regex.Match(outer, @"^### " + Regex.Escape(heading) + @".*?(?=^### |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.False(string.IsNullOrEmpty(sub), $"the '{heading}' subsection has gone");
        return CuratedPage.Flatten(Reader(sub));
    }

    /// <summary>
    /// A count, however this corpus writes it. Copied rather than shared, for
    /// the reason WI-522 gives: a frequency guard and a schedule guard need
    /// different vocabularies (§12.8, WI-521).
    /// </summary>
    private const string CountWord =
        @"(?:\d+(?:\.\d+)?|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve|thirteen|"
        + @"fourteen|fifteen|sixteen|seventeen|eighteen|nineteen|twenty|thirty|forty|fifty|sixty|"
        + @"seventy|eighty|ninety|hundred|dozen)";

    private const string Fraction = @"(?:half|halves|thirds?|quarters?|fifths?|tenths?)";

    /// <summary>
    /// Word runs this page shares with another page ON PURPOSE. Kept tiny for
    /// the WI-521 reason: every entry is a link label or a safety claim stated
    /// at one strength across the corpus (§12.10).
    /// </summary>
    private static readonly string[] DeliberatelyShared =
    [
        // The two standard "Where to go next" doors, worded identically on
        // every library page so a reader moving between them recognises them.
        // Kept as one run including the "an operation" that precedes them on
        // both drug pages: the shingle window spans the sentence boundary, so
        // allowlisting the doors alone leaves a gram that is nobody's claim.
        "an operation. Brain tumor types if you want the page for your own tumor. "
        + "Get help now if you need to talk to a person today.",

        // The corpus's line for the seizure that IS an emergency, in the words
        // Content/blocks/escalation.md and /seizures/what-to-do both use. A
        // safety claim that must not have two strengths (§12.10), and this page
        // is about the medicine that is meant to prevent it.
        "another seizure starting before they have come round from the first",

        // The corpus's sentence for what is NOT an ambulance call, likewise.
        // /treatments/steroids and the escalation block both carry it; three
        // pages saying it three ways is how the line moves.
        "A seizure like the ones you usually have, that stops on its own, is not an",

        // The third tier's closing line, worded identically on /treatments/steroids
        // for the same reason: a reader who is unsure must get the same answer
        // whichever drug page they are standing on.
        // Including the "phone." that precedes it: /review found the two drug
        // pages saying "a smaller thing" and "a much smaller thing", which is
        // the §12.10 drift the comment on this very list claimed to have
        // fixed. They are identical now, so the shared run reaches back across
        // the sentence boundary.
        "is phone. That call is a much smaller thing than any of the problems on this page.",

        // The three escalation bullets /review's BLOCKER 4 put back, in the
        // corpus's own words. These are the clearest case the allowlist exists
        // for: /treatments/steroids and Content/blocks/escalation.md both carry
        // them, and a symptom sorted into a tier is the definitive §12.10
        // "must not have two strengths" claim. Rewording them here to dodge the
        // shingle check is exactly how the corpus ends up with three spellings
        // of one ambulance rule.
        // Carried as one run with the sentence that follows it in the
        // ambulance tier, because the shingle window spans the boundary and
        // the two are adjacent in the block as well as here.
        "suddenly not being able to speak, move one side, or see. A seizure like the "
        + "ones you usually have, that stops on its own, is not an",
        "You are confused, or somebody says you are not making sense",

        // The corpus's tier label, in Content/blocks/escalation.md's exact
        // words. Deliberate: the first draft used /treatments/steroids'
        // variant ("Call your team the same day if:"), and a reader who meets
        // two spellings of the same tier on two drug pages has to work out
        // whether they mean the same thing. They do, so it is the block's
        // wording (§12.10).
        "Call your team the same day for any of these",

        // The sentence that stops a drug-specific call list being read as the
        // reader's WHOLE call list, and the route out of it. §12.10: a safety
        // claim must not have two strengths, and this one had already started
        // to drift — the first draft here said "gave you" where
        // /treatments/steroids says "gives you", which is how two pages end up
        // meaning slightly different things about who owns the list. Made
        // identical on purpose rather than reworded.
        "It does not replace the list your own team gives you for your tumor, and "
        + "[get help now](/get-help-now) has the numbers to keep on a phone. Call",
    ];

    private static readonly HashSet<string> AllowedShingles =
        [.. DeliberatelyShared.SelectMany(s => Shingles(s, 8))];

    [Fact]
    public void TheShortVersionCarriesTheSpineAndBothHalvesOfIt()
    {
        // §12.3: readers consume 20 to 28% of a page, so the claims the page
        // exists for are in the first five sentences or they are not read.
        var shortVersion = CuratedPage.Flatten(Reader(Section("The short version")));

        // Half one: the mood change may be the pill.
        Assert.Matches(new Regex(@"short-tempered, angry or low", RegexOptions.IgnoreCase), shortVersion);
        Assert.Matches(new Regex(@"put that down to the tumor when it\s+may be the pill", RegexOptions.IgnoreCase),
            shortVersion);

        // Half two, and it is the half a reader arrives angry about: not being
        // given one is the guidance rather than a snub.
        Assert.Matches(new Regex(@"never had a seizure, you will probably not be\s+offered one", RegexOptions.IgnoreCase),
            shortVersion);
        Assert.Matches(new Regex(@"guidance rather than your team being mean", RegexOptions.IgnoreCase), shortVersion);

        // The rule that keeps a reader safe between here and the section that
        // explains it.
        Assert.Matches(new Regex(@"dose is never yours to change", RegexOptions.IgnoreCase), shortVersion);

        // And the thing it does NOT do, because a reader who thinks this is
        // tumor treatment reads a switch of medicine as a change of plan.
        Assert.Matches(new Regex(@"works on the brain, not on the tumor", RegexOptions.IgnoreCase), shortVersion);
    }

    [Fact]
    public void TheHeadlineDoesNotAssertWhatThePageCorrects()
    {
        // §12.8 (WI-524): `ReaderText` strips the front matter, so 24 tests can
        // be blind to a title and a description that shipped the defect. The
        // same regexes that run over the body run over these two lines.
        var headline = Headline;

        // No verdict grammar. The page teaches that it MIGHT be the drug; a
        // headline saying it IS is the sentence /treatments/steroids deleted
        // from a sibling one item ago.
        Assert.DoesNotMatch(new Regex(@"\bit is the (?:drug|medicine|tablet)\b", RegexOptions.IgnoreCase), headline);
        Assert.Matches(new Regex(@"\bmight be the medicine\b", RegexOptions.IgnoreCase), headline);

        // No absolute stop rule, which the page's own "how long" section
        // corrects for the post-operative reader.
        Assert.DoesNotMatch(new Regex(@"never (?:be )?stop(?:ped|ping)? (?:it )?suddenly", RegexOptions.IgnoreCase),
            headline);

        // No driving answer in the one line every reader reads.
        Assert.DoesNotMatch(new Regex(@"you can(?:not)? drive|when you can drive", RegexOptions.IgnoreCase), headline);

        // Canaries: each ban must be able to fire.
        Assert.Matches(new Regex(@"\bit is the (?:drug|medicine|tablet)\b", RegexOptions.IgnoreCase),
            "when it is the drug and not the tumor");
        Assert.Matches(new Regex(@"never (?:be )?stop(?:ped|ping)? (?:it )?suddenly", RegexOptions.IgnoreCase),
            "why it must never be stopped suddenly");
        Assert.Matches(new Regex(@"you can(?:not)? drive|when you can drive", RegexOptions.IgnoreCase),
            "and when you can drive again");
    }

    [Fact]
    public void NoFrequencyIsPublishedForTheMoodEffectAndThePageSaysWhyNot()
    {
        // §12.4 R2 as WI-521 extended it to frequencies. Seven figures, seven
        // populations: >13% in phase III trials, 30.1% pediatric and 6.9%
        // having to stop (PMC8922949); 14.1% in IDH-wildtype glioblastoma
        // (PMC10951696); 13% "emotional instability" in 29 post-craniotomy
        // patients and 58.5% reporting any effect in 41 (PMC3717617); 46.4%
        // agitation in a prophylaxis group of TEN (PMC8787304).
        //
        // PAGE-WIDE, not scoped to the mood section (§12.8, WI-524: a guard
        // scoped to the section named for the defect has a door next to it —
        // /review put the banned sentence two screens down and every test
        // stayed green). The exemptions are a short NAMED allowlist below.
        // /review wrote five frequencies that walked this guard and ran them
        // against the regex. Each alternative below carries the one it was
        // widened for:
        //   * "About one person in three"    — a NOUN between the two counts
        //   * "13 percent of patients"       — the WORD, where only `%` was banned
        //   * "a quarter of those taking it" — a population that is not
        //                                      people/patients/them
        //   * "three times as likely"        — a multiplier is not an n-in-m shape
        //   * "most families notice it"      — "most people" was banned, "most
        //                                      families" and "most carers" were not
        var frequency = new Regex(
            @"\b(?:about |around |roughly |up to |as many as |nearly )?" + CountWord
            + @"\s+(?:\w+\s+){0,2}(?:in|out of|of)\s+(?:every\s+)?" + CountWord + @"\b|"
            + @"\b" + CountWord + @"\s*(?:percent|per cent)\b|"
            + @"\b" + Fraction + @"\s+of\s+(?:all\s+)?(?:the\s+)?\w+\b|"
            + @"\b" + CountWord + @"\s+times\s+(?:as|more)\s+likely\b|"
            + @"\b(?:most|majority|nearly all|almost all|hardly any|a minority|a small number) of "
            + @"(?:the\s+)?(?:people|patients|them|those|carers|caregivers|families)\b|"
            + @"\b(?:most|almost all|nearly all|hardly any) "
            + @"(?:people|patients|families|carers|caregivers)\b|"
            + @"%",
            RegexOptions.IgnoreCase);

        // Canaries, one per alternative, before the page's pass means anything.
        // The last five are /review's walk-throughs, each of which was GREEN
        // against the first version of this regex.
        Assert.Matches(frequency, "It happens to about one in three people.");
        Assert.Matches(frequency, "A quarter of people get it.");
        Assert.Matches(frequency, "Most of the people on it notice something.");
        Assert.Matches(frequency, "Nearly all people feel it in the first week.");
        Assert.Matches(frequency, "It happens in 46.4% of them.");
        Assert.Matches(frequency, "About one person in three on this medicine notices a change in temper.");
        Assert.Matches(frequency, "In one study 13 percent of patients had emotional instability.");
        Assert.Matches(frequency, "Roughly a quarter of those taking it get some of this.");
        Assert.Matches(frequency, "People who have had depression before are three times as likely to get it.");
        Assert.Matches(frequency, "Most families notice it within the first month.");

        // The allowlist. ONE entry, and it is one entry because /review ran the
        // other two over the page and found they rescued nothing: the page no
        // longer says "Most people who have had a seizure ... stay on medicine
        // for years" (an unsourced count, replaced), and it never said "having
        // to stop for it is the exception" at all — that one was an allowlist
        // entry for a sentence the front matter CLAIMED the page printed and it
        // did not (§12.8, WI-523: the front matter can claim what the page does
        // not do). The claim is now on the page, in prose the guard does not
        // match, and the dead entries are gone rather than left looking load-bearing.
        var allowed = new[]
        {
            // The page's own slot 2 answer: why this reader is on it at all.
            // Not a frequency about an effect — it is who the medicine is for,
            // and it is the guideline's own scope.
            "Almost always because you have had a seizure",
        };

        var offenders = CuratedPage.SentencesOf(Everything)
            .Where(s => frequency.IsMatch(s))
            .Where(s => !allowed.Any(a => s.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(offenders.Count == 0,
            "this page publishes a frequency the sources do not agree on:\n  "
            + string.Join("\n  ", offenders)
            + "\n\nSee the front matter: seven figures, seven populations. Print the direction, or "
            + "add the sentence to the allowlist with the source quote that earns it.");

        // A SUPERLATIVE IS NOT A FREQUENCY, AND THE FREQUENCY GUARD CANNOT SEE
        // IT. PMC3717617 says people with previous depression are "at GREATER
        // RISK for developing increased aggression"; the first draft said they
        // "are the ones MOST LIKELY to get it", which promotes greater-risk to
        // first-place-among-all-risk-factors and is a claim about every other
        // reader as well (§12.8, WI-509: a uniqueness claim on a page of many
        // is a claim about all the others). The harness walked the widened
        // frequency regex with it on both line endings.
        var superlativeRisk = new Regex(
            @"\bthe ones? (?:most|least) likely\b|\bmost likely to (?:get|have|develop)\b|"
            + @"\bthe (?:main|biggest|commonest|most common) (?:reason|cause|risk)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(superlativeRisk,
            "People who have had depression before are the ones most likely to get it.");
        Assert.DoesNotMatch(superlativeRisk, Everything);

        // And the sourced version is what is printed instead.
        Assert.Matches(new Regex(@"trouble with their behavior before are more likely to get it",
            RegexOptions.IgnoreCase), Body);

        // And the page says WHY there is no number, because a page that is
        // silently silent leaves the reader to supply their own (§12.8, WI-521).
        var mood = Subsection("Is it the tumor, or is it the medicine?", "The change in mood and temper");
        Assert.Matches(new Regex(@"Nobody can give you the odds", RegexOptions.IgnoreCase), mood);
        Assert.Matches(new Regex(@"do not\s*land anywhere near one another", RegexOptions.IgnoreCase), mood);
        Assert.Matches(new Regex(@"borrowed from\s*somebody who is not you", RegexOptions.IgnoreCase), mood);
    }

    [Fact]
    public void TheProphylaxisRulingCarriesItsScopeAndTheSurgeryExceptionBesideIt()
    {
        // THE ITEM'S SHARP END (§12.12). PMC8563323 gives Level A for "newly
        // diagnosed brain tumors who have not had a seizure" and — in the same
        // guideline — "insufficient evidence" (Level C) for the peri- and
        // postoperative period. A page carrying the first without the second
        // tells a reader whose team gave them levetiracetam around an operation
        // that their team defied the strongest grade of advice there is.
        var section = CuratedPage.Flatten(Reader(Section("Why has nobody given me one?")));

        // The claim, scoped in the sentence that makes it.
        var ruling = CuratedPage.SentencesOf(section)
            .Where(s => Regex.IsMatch(s, @"should not be put on one", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(ruling.Count >= 1,
            "the prophylaxis ruling has gone, so everything below this checks nothing "
            + "(§12.8, WI-524: assert the match COUNT before iterating)");
        foreach (var sentence in ruling)
        {
            Assert.Matches(new Regex(@"not had a seizure", RegexOptions.IgnoreCase), sentence);
        }

        // The exception, in the same section rather than three screens away.
        Assert.Matches(new Regex(@"does not settle: the operating room", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not enough evidence either way", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"they have not broken a rule", RegexOptions.IgnoreCase), section);

        // And the page never states the Level A as a universal. The canary is
        // the sentence a summariser would write.
        // BOTH SUBJECT ORDERS. The first version demanded "(nobody|people) with
        // a brain tumor (should|is|are) …", and the harness walked it by
        // reversing the sentence: "Brain tumor patients are not put on these
        // unless they have had a seizure" is the §12.12 defect in its purest
        // form — it tells the surgery reader their team broke a rule — and it
        // is invisible to a pattern keyed on word order.
        var universal = new Regex(
            @"(?:nobody|no one|no-one|anyone|people|patients) with a brain tumou?r "
            + @"(?:should|is|are|do not|does not)\s*(?:not )?(?:be )?(?:given|put on|offered|get)|"
            + @"brain tumou?r (?:patients|readers)[^.]{0,30}"
            + @"(?:are not|should not|do not)\s*(?:be )?(?:given|put on|offered|get)",
            RegexOptions.IgnoreCase);
        Assert.Matches(universal, "Nobody with a brain tumor should be given one of these.");
        Assert.Matches(universal, "Brain tumor patients are not put on these unless they have had a seizure.");
        Assert.DoesNotMatch(universal, Everything);

        // The reason is printed, because a refusal with a reason is a different
        // experience from a refusal (the guideline's own rationale).
        // The guideline says the evidence is thin and side-effect-weighted; it
        // does NOT report a settled negative trial. /review found the first
        // draft converting "lack of definitive evidence that the potential
        // benefits might outweigh the side effects" into "the studies did not
        // show these pills preventing that first seizure" — and this assertion
        // then PINNED the overstatement in place (§12.14's shape: the test name
        // made it look handled).
        Assert.Matches(new Regex(@"studies that were done were small, and several", RegexOptions.IgnoreCase),
            section);
        Assert.Matches(new Regex(@"stopped early", RegexOptions.IgnoreCase), section);
        Assert.DoesNotMatch(new Regex(@"did not show these pills preventing", RegexOptions.IgnoreCase),
            Everything);
        Assert.Matches(new Regex(@"get in the way of\s*cancer treatment", RegexOptions.IgnoreCase), section);

        // And the reader is given the question that gets an answer rather than
        // the one that gets a no.
        Assert.Contains("what would make you start me on one?", section, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheShortCourseClaimIsScopedToTheReaderItIsTrueOf()
    {
        // PMC8563323 scopes the post-operative taper to people "who have not had
        // a seizure". A reader who HAS had one, reading that a week is enough,
        // is the most dangerous misreading this page can produce.
        var section = CuratedPage.Flatten(Reader(Section("How long will I be on it?")));

        var week = CuratedPage.SentencesOf(section)
            .Where(s => Regex.IsMatch(s, @"first week", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(week.Count >= 1, "the short-course sentence has gone, so its scope check proves nothing");
        foreach (var sentence in week)
        {
            Assert.Matches(new Regex(@"For that person", RegexOptions.IgnoreCase), sentence);
        }

        // Said again, in the reader's own grammar, immediately after.
        Assert.Matches(new Regex(@"does not apply to you if you have had a seizure", RegexOptions.IgnoreCase),
            section);

        // And the question that tells the two apart is on the page, because the
        // two situations are indistinguishable from where the reader stands.
        Assert.Matches(new Regex(@"treat seizures I have had, or to prevent ones I have not", RegexOptions.IgnoreCase),
            section);

        // PAGE-WIDE, because a section-scoped scope check has a door next to it
        // (§12.8, WI-524). The harness walked the section version by putting
        // "Some people stop a week or so after their operation, and that is the
        // plan" into the step-by-step list — unscoped, four sections above, and
        // every test stayed green on both line endings.
        var shortCourse = new Regex(
            @"\b(?:stop|stopping|stopped|off it|come off)\b[^.]{0,60}"
            + @"\b(?:a week|first week|week or so|one week)\b|"
            + @"\b(?:a week|first week|week or so|one week)\b[^.]{0,60}"
            + @"\b(?:stop|stopping|stopped|off it|come off)\b",
            RegexOptions.IgnoreCase);
        var scoped = new Regex(
            @"never had a seizure|not had a seizure|For that person|have not had one",
            RegexOptions.IgnoreCase);

        Assert.Matches(shortCourse, "Some people stop a week or so after their operation, and that is the plan.");
        Assert.DoesNotMatch(scoped, "Some people stop a week or so after their operation, and that is the plan.");

        var shortCourseSentences = CuratedPage.SentencesOf(Everything)
            .Where(s => shortCourse.IsMatch(s))
            .ToList();
        Assert.True(shortCourseSentences.Count >= 1,
            "the short-course claim has gone from the page, so its scope check proves nothing");

        var unscoped = shortCourseSentences.Where(s => !scoped.IsMatch(s)).ToList();
        Assert.True(unscoped.Count == 0,
            "this page tells a reader the course stops after a week without saying WHICH reader:\n  "
            + string.Join("\n  ", unscoped)
            + "\n\nPMC8563323 scopes it to people who have NOT had a seizure, and the two readers are "
            + "indistinguishable from where they stand.");
    }

    [Fact]
    public void NothingOnThePageReadsAsPermissionToChangeTheDose()
    {
        // §12.8 (WI-524): check the POLARITY of the sentence, not the presence
        // of a safe word. "It is reasonable to halve your dose until your team
        // can see you" names the team and is still permission.
        //
        // This matters more here than it did on the steroid page. The reader
        // has just been told the mood change may be the pill, which is
        // exactly the moment somebody skips a dose — and the thing the tablet
        // is stopping has not gone anywhere.
        // TWO CLOSED LISTS OF EIGHT WORDS IS NOT A GUARD. /review beat the
        // first version four times over, and every one of the four is a canary
        // below:
        //   * "It is SAFE to skip a single dose"  — a permission synonym
        //   * "HOLD tonight's dose"               — a verb that was not listed,
        //                                           so polarity was never tested
        //   * "You are ALLOWED TO miss one dose"  — another permission synonym
        //   * "it is FAIR TO leave the next one"  — beat this guard AND the
        //                                           caregiver one
        //
        // So the rule is inverted. Instead of hunting for permission words, every
        // sentence that mentions changing a dose must EARN its place: it is
        // either a prohibition, or the change belongs to somebody who is not the
        // reader. Anything else fails, including shapes nobody has thought of.
        // The OBJECT is a dose or a pill, not "medicine". Widening it to
        // "medicine" fired on four of this page's own correct sentences —
        // "this medicine raises the bar", "what is the medicine costing you",
        // "a change caused by this medicine" — none of which is about changing
        // anything. A guard that fails a correct page is worse than no guard
        // (§12.8, WI-508), and the useful scope here is the thing a reader can
        // physically alter tonight.
        const string window = "{0,35}";

        // `miss` is spelled out separately from the rest, and NOT as `miss\w*`.
        // "a missed dose" and "doses get missed" are the accident this page
        // treats sympathetically; only the deliberate act belongs here.
        const string verbs =
            @"(?:(?:stop|skip|halve|lower|reduce|cut|double|split|hold|pause|delay|leave|"
            + @"adjust|raise|increase|decrease|change)\w*|miss(?:es|ing)?)\b";

        var change = new Regex(
            @"\b" + verbs + @"[^.]" + window + @"\b(?:dose|doses|pills?)\b|"
            + @"\b(?:dose|doses|pills?)\b[^.]" + window + @"\b" + verbs,
            RegexOptions.IgnoreCase);

        // A prohibition, or a change that is somebody else's to make. The
        // passive forms matter: the page's own correct reversal sentence is
        // "When the dose is lowered or the medicine is swapped…", which has no
        // prohibition in it and should not need one.
        var earnsItsPlace = new Regex(
            @"\b(?:do not|don't|never|not yours|rather than|instead of|dangerous|not one to|"
            + @"your team|the team|your doctor|a team|they may|is lowered|is swapped|is changed|"
            + @"is tried|comes down|be changed|gets swapped|gets changed|running out is stopping|"
            + @"whoever changes|whoever prescribes)\b",
            RegexOptions.IgnoreCase);

        var permission = new Regex(
            @"\b(?:it is (?:fine|ok|okay|reasonable|sensible|worth|safe|alright|all right|fair)|"
            + @"you can|you could|you may|you are allowed|feel free|no harm in|there is no harm|"
            + @"if you feel ready|nothing wrong with|it does no harm)\b",
            RegexOptions.IgnoreCase);

        // Canaries: the original, plus /review's four walk-throughs.
        foreach (var known in new[]
                 {
                     "It is reasonable to halve your dose until your team can see you.",
                     "It is safe to skip a single dose while you wait for the clinic to call back.",
                     "If the anger is bad, hold tonight's dose and call in the morning.",
                     "You are allowed to miss one dose if you are being sick.",
                     "If they are furious after a dose, it is fair to leave the next one until you "
                     + "have spoken to the team.",
                 })
        {
            Assert.Matches(change, known);
            Assert.True(permission.IsMatch(known) || !earnsItsPlace.IsMatch(known),
                "this guard would let through: " + known);
        }

        // Questions are out of scope: "What do I do if I miss a dose?" IS
        // handing the decision to the team, which is the property this guard
        // exists to enforce. Sentences, not questions.
        //
        // And the verb list matches the ACT, not the past participle: "a missed
        // dose" and "doses get missed" describe an accident that has already
        // happened, which this page treats sympathetically on purpose. Widening
        // `miss` to `miss\w*` flagged four of the page's own correct sentences.
        var matches = CuratedPage.SentencesOf(Everything)
            .Where(s => !s.TrimEnd().EndsWith('?'))
            .Where(s => change.IsMatch(s))
            .ToList();
        Assert.True(matches.Count >= 4,
            $"only {matches.Count} sentences mention changing a dose, which is fewer than this page "
            + "has ever had — the regex has stopped seeing the page's own prose and is checking nothing "
            + "(§12.8, WI-524)");

        // A PROHIBITION WINS OVER A STRAY PERMISSION WORD elsewhere in the same
        // sentence. The page's own "**The dose is never yours to change**, and
        // how much you can do about driving is a question for your own team"
        // carries both "never" and "you can", and it is correct; flagging it
        // would be the rule-that-fails-a-correct-page shape again.
        var forbids = new Regex(
            @"\b(?:do not|don't|never|not yours|dangerous|not one to)\b", RegexOptions.IgnoreCase);

        var unearned = matches
            .Where(s => !earnsItsPlace.IsMatch(s) || (permission.IsMatch(s) && !forbids.IsMatch(s)))
            .ToList();
        Assert.True(unearned.Count == 0,
            "this page mentions changing a dose without either forbidding it or handing it to the "
            + "team:\n  " + string.Join("\n  ", unearned));

        // And the prohibition is stated outright, in the section a frightened
        // reader is standing in when they think of it.
        var mood = Subsection("Is it the tumor, or is it the medicine?", "The change in mood and temper");
        Assert.Matches(new Regex(@"Do not lower it, split it or skip it yourself", RegexOptions.IgnoreCase), mood);
        Assert.Matches(new Regex(@"a dose dropped at home is how a\s*seizure gets back in", RegexOptions.IgnoreCase),
            mood);
    }

    [Fact]
    public void TheStopRuleIsNotOnYourOwnRatherThanNeverSuddenly()
    {
        // §12.8 (WI-524), one item later and the same shape. "Never stop
        // suddenly" is right for the reader who has had a seizure and wrong for
        // the post-operative reader the guideline says to stop after a week.
        // The rule that holds in both cases is "not on your own".
        var absolute = new Regex(
            @"\bnever\b[^.]{0,40}\bstop\w*\b[^.]{0,40}\bsudden|"
            + @"\bmust (?:never|always)\b[^.]{0,40}\b(?:stop|taper|come off)\w*\b|"
            + @"\balways (?:tapered|reduced|brought down) (?:slowly|gradually)\b",
            RegexOptions.IgnoreCase);

        Assert.Matches(absolute, "A seizure medicine must never be stopped suddenly.");
        Assert.Matches(absolute, "It is always brought down slowly.");
        Assert.DoesNotMatch(absolute, Everything);

        // The rule the page states instead, and it is stated more than once
        // because it is the page's load-bearing safety claim.
        var notOnYourOwn = CuratedPage.SentencesOf(Everything)
            .Count(s => Regex.IsMatch(s, @"(?:not|never) (?:yours|one) to (?:change|make|stop|settle)|"
                + @"on your own|for the care team", RegexOptions.IgnoreCase));
        Assert.True(notOnYourOwn >= 2,
            $"the 'not on your own' rule appears {notOnYourOwn} time(s); it is this page's load-bearing "
            + "safety claim and the short version and the mood section both need it");

        // COUNTED OVER `Everything`, THE FRONT MATTER CAN SATISFY THIS ON ITS
        // OWN, AND FOR A WHILE IT DID. /review enumerated the two hits and
        // found them to be the title/description string and the short version;
        // the mood section — the one this test's own comment says needs it —
        // contributed nothing, and the literal words "on your own" appeared
        // NOWHERE in the page body. That is BLOCKER 2 of that review, and the
        // fix is the property rather than the count: the phrase has to be in
        // the BODY, and it has to be attached to STOPPING, because stopping is
        // the thing the guideline lets one group do and forbids the other.
        Assert.Matches(new Regex(@"\bon your own\b", RegexOptions.IgnoreCase), Body);

        var mood = Subsection("Is it the tumor, or is it the medicine?", "The change in mood and temper");
        Assert.Matches(new Regex(@"never stop it on your own", RegexOptions.IgnoreCase), mood);

        // And it is stated as holding for the short course too, which is the
        // whole reason the absolute was wrong.
        Assert.Matches(new Regex(@"true however short your course is meant to be", RegexOptions.IgnoreCase),
            mood);
    }

    [Fact]
    public void TheTwoThingsThatSeparateTheDrugFromTheTumorAreBothOnThePage()
    {
        // The half the dossier does not have, and the reason the page is worth
        // writing (PMC8922949): the TIMING, and that it REVERSES.
        var mood = Subsection("Is it the tumor, or is it the medicine?", "The change in mood and temper");

        // Timing. Stated as the discriminator, with the tumor comparison that
        // makes it usable.
        Assert.Matches(new Regex(@"days after\s*it was begun, or in the days after the dose went up",
            RegexOptions.IgnoreCase), mood);
        Assert.Matches(new Regex(@"rarely changes somebody's temper\s*the day after a prescription changed",
            RegexOptions.IgnoreCase), mood);

        // Reversal, hedged where the source hedges ("usually") and never
        // promised. PAGE-WIDE rather than mood-scoped, and with a verb list
        // /review could not walk: it beat the first version with "the temper
        // will FADE within days", and beat the scoping entirely by putting the
        // promise in the short version ("That is fixable, and often quickly"),
        // 160 lines above a guard that only ever read this subsection.
        Assert.Matches(new Regex(@"the mood change usually lifts", RegexOptions.IgnoreCase), mood);

        var promise = new Regex(
            @"\bwill (?:lift|go away|stop|settle|fade|clear|pass|ease|die down|improve|resolve)\b|"
            + @"\b(?:is|are) (?:fixable|reversible|curable|easily fixed)\b|"
            + @"\bsoon (?:goes|go) away\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "the mood change will lift within two days",
                     "Once the dose comes down the temper will fade within days.",
                     "That is fixable, and often quickly, once a team knows.",
                 })
        {
            Assert.Matches(promise, known);
        }
        Assert.DoesNotMatch(promise, Everything);

        // The reversal is the TEAM'S move. A reversal claim next to a dose the
        // reader controls is an instruction, whatever the verb (§12.8, WI-524).
        var reversal = CuratedPage.SentencesOf(mood)
            .Where(s => Regex.IsMatch(s, @"lowered|swapped|calmer", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(reversal.Count >= 2, "the reversal claim has gone, so its polarity check proves nothing");

        // CHECK THE AGENT, NOT THE MODAL. The first version banned
        // "you (can|could|may) (lower|swap|reduce|try)" and the break harness
        // walked it — on both line endings — with *"When you lower the dose or
        // try a different one, the mood change usually lifts"*, which has no
        // modal at all. It is WI-524's polarity lesson one level down: a
        // permission sentence does not need a permission word, it only needs
        // the reader as the SUBJECT of the verb.
        //
        // So: find every sentence that makes the reader the agent of a dose
        // change, and require each one to be a PROHIBITION. The page's own
        // correct sentences ("Do not lower it, split it or skip it yourself")
        // pass because they are negated; the mutation fails because it is not.
        // Three shapes, because the reader can be the agent without being the
        // grammatical subject: "you lower the dose", "lower it yourself", and
        // the bare imperative with a possessive ("skip your dose").
        var readerChangesDose = new Regex(
            @"\byou\b[^.]{0,40}\b(?:lower|raise|skip|halve|split|stop|change|reduce|double|adjust)\b"
            + @"[^.]{0,40}\b(?:dose|doses|pill|pills|medicine|it)\b|"
            + @"\b(?:lower|raise|skip|halve|split|stop|change|reduce|double|adjust)\b[^.]{0,60}\byourself\b|"
            + @"\byou\b[^.]{0,20}\b(?:lower|raise|skip|halve|split|reduce|double)\s+(?:the|your|a)\b",
            RegexOptions.IgnoreCase);
        var prohibition = new Regex(
            @"\b(?:do not|don't|never|not yours|rather than|instead of|dangerous|not one to)\b",
            RegexOptions.IgnoreCase);

        // Canaries for both halves before the page's pass means anything.
        Assert.Matches(readerChangesDose, "When you lower the dose or try a different one, the mood change lifts.");
        Assert.Matches(readerChangesDose, "Do not lower it, split it or skip it yourself.");
        Assert.Matches(prohibition, "Do not lower it, split it or skip it yourself.");
        Assert.DoesNotMatch(prohibition, "When you lower the dose or try a different one, the mood change lifts.");

        var agentSentences = CuratedPage.SentencesOf(Everything)
            .Where(s => readerChangesDose.IsMatch(s))
            .ToList();
        // Exactly one today: "Do not lower it, split it or skip it yourself".
        // The count is asserted before the loop for the WI-524 reason — a
        // regex that stops seeing the page's own prose would make the loop
        // below iterate over nothing and pass for the wrong reason. If a
        // second such sentence is ever added it must be a prohibition too,
        // which is what the loop is for.
        Assert.True(agentSentences.Count >= 1,
            "no sentence on this page makes the reader the agent of a dose change, which means either the "
            + "prohibition has gone or the regex has stopped seeing it — check both before relaxing this");

        var instructions = agentSentences.Where(s => !prohibition.IsMatch(s)).ToList();
        Assert.True(instructions.Count == 0,
            "this page has the reader changing a dose without telling them not to:\n  "
            + string.Join("\n  ", instructions));

        // And the one action the page permits.
        Assert.Matches(new Regex(@"tell the team", RegexOptions.IgnoreCase), mood);
    }

    [Fact]
    public void TheReassuranceAboutThinkingIsScopedToThinking()
    {
        // PMC3717617: "Levetiracetam is not generally associated with cognitive
        // side effects." Worth printing — a reader told a drug affects the mind
        // assumes it affects the thinking, which is what they are most afraid
        // of. But it is a claim about COGNITION, and widening it to "it does not
        // affect your brain" would contradict the section it sits in.
        var mood = Subsection("Is it the tumor, or is it the medicine?", "The change in mood and temper");

        var claim = CuratedPage.SentencesOf(mood)
            .Where(s => Regex.IsMatch(s, @"not generally linked", RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(claim.Count == 1,
            $"the cognitive reassurance appears {claim.Count} times; it needs exactly one statement so it "
            + "cannot drift into two strengths");
        Assert.Matches(new Regex(@"thinking, memory or understanding", RegexOptions.IgnoreCase), claim[0]);

        // Hedged as the source hedges it, and never absolute. The verb and
        // object lists are wider than the obvious ones because /review walked
        // the first version with "It does not fog up your thinking the way
        // people fear" — which uses none of affect/damage/harm/touch — and
        // walked the count check above by putting a second, wider version in a
        // different section, where a subsection-scoped count cannot see it.
        var absolute = new Regex(
            @"\b(?:does not|will not|cannot|never) (?:affect|damage|harm|touch|fog|cloud|dull|slow|blunt)"
            + @"\w*\b(?: up| down)?[^.]{0,20}\b(?:brain|mind|thinking|memory|head|concentration)\b|"
            + @"\bno effect on (?:thinking|memory|the brain|your mind)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(absolute, "It does not affect your thinking.");
        Assert.Matches(absolute, "It does not fog up your thinking the way people fear.");
        Assert.DoesNotMatch(absolute, Everything);

        // And the hedged version appears exactly once ACROSS THE PAGE, not just
        // once in this subsection.
        var hedged = CuratedPage.SentencesOf(Everything)
            .Where(s => Regex.IsMatch(s, @"not generally linked|not (?:usually|normally) linked",
                RegexOptions.IgnoreCase))
            .ToList();
        Assert.True(hedged.Count == 1,
            $"the cognitive reassurance appears {hedged.Count} times on the page; one statement, so it "
            + "cannot drift into two strengths (§12.8, WI-510)");

        // And the scope word is there: this is the drug's best-known effect
        // being CATEGORISED, not minimised.
        Assert.Matches(new Regex(@"It is mood and temper, and that is a\s*different thing", RegexOptions.IgnoreCase),
            mood);
    }

    [Fact]
    public void ThePageStatesNoDrivingRuleAndRoutesToThePageThatOwnsIt()
    {
        // Jurisdictional. /seizures/living-with#driving owns the framing, the
        // state-by-state tool and the grief, and the same line is held by
        // WI-451, WI-559 and WI-560.
        // /review published a waiting time past the first version of this
        // guard twice, and evaded the rest twice more. Each widening below is
        // one of those four:
        //   * "six SEIZURE-FREE months"     — the count and the unit were
        //                                     required to be ADJACENT
        //   * "a SIX-MONTH rule"            — `\s+` could not match a hyphen
        //   * "you can START DRIVING once"  — only "drive again after" was banned
        //   * "the LICENSING office"        — `licen[sc]e` does not match "licensing"
        var rule = new Regex(
            @"\b(?:seizure[- ]free|free of seizures|no seizures?) for\s+" + CountWord + @"\b|"
            + @"\b" + CountWord + @"[-\s]+(?:\w+[-\s]+){0,2}(?:months?|years?|weeks?)\b[^.]{0,40}"
            + @"\b(?:driv|licen[sc]|DMV|DVLA)\w*|"
            + @"\b(?:driv|licen[sc])\w*\b[^.]{0,50}\b" + CountWord
            + @"[-\s]+(?:\w+[-\s]+){0,2}(?:months?|years?|weeks?)\b|"
            + @"\byou can (?:drive|start driving|get back behind)\b[^.]{0,20}"
            + @"\b(?:again|after|once|when)\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "You must be seizure-free for six months before you can drive.",
                     "Most states ask for 6 months without a seizure before a licence comes back.",
                     "You can drive again after three months.",
                     "In most states the licensing office asks for six seizure-free months before you drive again.",
                     "In most states you can start driving once your doctor signs a form.",
                 })
        {
            Assert.Matches(rule, known);
        }
        Assert.DoesNotMatch(rule, Everything);

        var driving = CuratedPage.Flatten(Reader(Section("Can I drive?")));

        // "Most states use a six-month rule" carries a waiting time and no
        // driving word at all, so no page-wide pattern keyed on driv/licen can
        // see it. Inside THIS section the scope does the work the vocabulary
        // cannot: a length of time in the driving section is a waiting time,
        // whatever sentence it is wearing.
        var anyDuration = new Regex(
            @"\b" + CountWord + @"[-\s]+(?:\w+[-\s]+){0,2}(?:day|days|week|weeks|month|months|year|years)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(anyDuration, "Most states use a six-month rule.");
        Assert.Matches(anyDuration, "You will be asked for twelve months without one.");
        Assert.DoesNotMatch(anyDuration, driving);

        // The route, at the anchor rather than the top of a long page.
        Assert.Contains("/seizures/living-with#driving", driving, StringComparison.Ordinal);

        // What this page's own paragraph is FOR. /review found the first
        // version's "Starting a seizure medicine does not by itself change what
        // you are allowed to do" to be a negative claim about every
        // jurisdiction that no source makes — and one that
        // /tumors/oligodendroglioma flatly contradicts ("ask again after any
        // change in your medicine or your seizures"). The page now says the
        // sourced half and sends the reader to ask.
        Assert.Matches(new Regex(@"around seizures rather than around being on a pill", RegexOptions.IgnoreCase),
            driving);
        Assert.Matches(new Regex(@"a change to your medicine is worth asking about before you drive",
            RegexOptions.IgnoreCase), driving);
        Assert.DoesNotMatch(new Regex(@"does not by itself change what you are allowed", RegexOptions.IgnoreCase),
            Everything);

        // And the sibling this page must agree with, read rather than
        // remembered (§12.10).
        var oligo = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("tumors", "oligodendroglioma.md")));
        Assert.Matches(new Regex(@"ask\s*again after any change in your medicine or your seizures",
            RegexOptions.IgnoreCase), oligo);

        // And the one driving rule that belongs to a medicine page.
        Assert.Matches(new Regex(@"sleepy, dizzy or unsteady, do not drive", RegexOptions.IgnoreCase), driving);

        // No state is named anywhere on the page — a named state is a rule for
        // somebody else's reader, and /review's point was that the first
        // version listed five of fifty. All of them now, plus DC.
        var states = new Regex(
            @"\b(?:Alabama|Alaska|Arizona|Arkansas|California|Colorado|Connecticut|Delaware|Florida|"
            + @"Georgia|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|"
            + @"Massachusetts|Michigan|Minnesota|Mississippi|Missouri|Montana|Nebraska|Nevada|"
            + @"New Hampshire|New Jersey|New Mexico|New York|North Carolina|North Dakota|Ohio|"
            + @"Oklahoma|Oregon|Pennsylvania|Rhode Island|South Carolina|South Dakota|Tennessee|"
            + @"Texas|Utah|Vermont|Virginia|Washington|West Virginia|Wisconsin|Wyoming)\b");
        Assert.Matches(states, "In Ohio your doctor has to report you to the state.");
        Assert.DoesNotMatch(states, Everything);
    }

    [Fact]
    public void TheSelfHarmWarningIsPresentAndRoutesSomewhereRealToday()
    {
        // PMC3717617: "suicidality in some patients prescribed LEV has been
        // reported." NHS lists low mood and thoughts of self-harm among the
        // reasons the drug may not suit somebody. A page that names the risk
        // and gives the reader nowhere to go with it is worse than one that
        // does not name it.
        var section = CuratedPage.Flatten(Reader(Section("Thoughts of harming yourself")));

        Assert.Matches(new Regex(@"have been reported in people taking", RegexOptions.IgnoreCase), section);

        // A number a reader can dial, in the sentence that tells them to,
        // rather than one click away (§12.8, WI-520: the page keeps a number
        // when it only ever pushes toward asking for help).
        Assert.Contains("988", section, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"Suicide and Crisis Lifeline", RegexOptions.IgnoreCase), section);
        Assert.Contains("/get-help-now", section, StringComparison.Ordinal);

        // Today, not at the next appointment.
        Assert.Matches(new Regex(@"say so today", RegexOptions.IgnoreCase), section);

        // Not characterised as weakness or as the tumor.
        Assert.Matches(new Regex(@"not a sign that you are weak", RegexOptions.IgnoreCase), section);

        // And it prints NO frequency for this one. The first draft said "It is
        // not common", which no source supports: PMC3717617 says only that
        // "suicidality in some patients prescribed LEV has been reported".
        // Found by the end-to-end read, not by a gate — the page-wide frequency
        // guard has no word for "not common", and this is the one claim on the
        // page where the over-reassuring direction (§12.12) costs the most.
        var rarity = new Regex(
            @"\b(?:not common|uncommon|rare|rarely|unusual|very few|hardly ever)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(rarity, "It is not common, and it is not a sign that you are weak.");
        Assert.DoesNotMatch(rarity, section);

        // And the same-day list repeats it, because a reader in that state is
        // not reading the whole page in order.
        var calls = CuratedPage.Flatten(Reader(Section("When to call, and what about")));
        Assert.Contains("988", calls, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePageDoesNotClaimTheMedicineTreatsTheTumor()
    {
        // PMC8563323, Level C: "There is insufficient evidence to support
        // prescribing valproic acid or levetiracetam with the intent to prolong
        // progression-free or overall survival." People have read the headlines.
        var section = CuratedPage.Flatten(Reader(Section("Does it do anything to the tumor?")));

        Assert.Matches(new Regex(@"^\s*\*\*No, and you may have read otherwise", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"not enough\s*evidence", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"open question in\s*research", RegexOptions.IgnoreCase), section);

        // No outlook claim in either direction, and no prognosis vocabulary:
        // the guideline is a statement about evidence, not about how long
        // anybody lives (§12.5).
        //
        // STEMS, not literals. /review beat the literal list with "People on
        // this medicine have LIVED longer in some studies" — "lived longer"
        // does not contain "live longer", and neither does "living longer".
        var outlook = new Regex(
            @"\bsurviv\w*\b|\bliv\w*\s+longer\b|\blonger\s+(?:surviv|liv)\w*\b|"
            + @"\blife expectancy\b|\bprognos\w*\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(outlook, "People on this medicine have lived longer in some studies.");
        Assert.Matches(outlook, "It is linked with living longer.");
        Assert.DoesNotMatch(outlook, Everything);

        // And no positive claim about an effect on the tumor, however it is
        // phrased. The vocabulary ban above cannot see "There is early evidence
        // that it may hold a glioma back", which /review walked through: it
        // contains no outcome word and no comparative. So every sentence in
        // this section that puts the medicine and the tumor together has to
        // carry a negation.
        var effectOnTumor = new Regex(
            @"\b(?:may|might|can|could|does|will|seems? to|appears? to)\s+"
            + @"(?:also\s+)?(?:hold|slow|shrink|stop|fight|treat|help with|act on|work on)\b",
            RegexOptions.IgnoreCase);
        var negation = new Regex(
            @"\b(?:no|not|nothing|never|does not|is not|there is not|open question|unsettled)\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(effectOnTumor, "There is early evidence that it may hold a glioma back.");
        Assert.DoesNotMatch(negation, "There is early evidence that it may hold a glioma back.");

        var positives = CuratedPage.SentencesOf(Everything)
            .Where(s => effectOnTumor.IsMatch(s) && Regex.IsMatch(s, @"\btumou?r|glioma\b", RegexOptions.IgnoreCase))
            .Where(s => !negation.IsMatch(s))
            .ToList();
        Assert.True(positives.Count == 0,
            "this page claims the medicine acts on the tumor:\n  " + string.Join("\n  ", positives));

        // And the page does not rank one drug over another. PMC10951696's
        // levetiracetam-over-valproate line is reported secondhand, and a
        // reader already on valproate does not need to be told their drug lost.
        //
        // An explicit comparative, not a proximity window. The first version
        // was "(better|best|...) within 40 characters of (than|medicine|drug)"
        // and it fired on "It is one of the best-known things about this
        // medicine", which is neither a comparison nor about a drug — a
        // proximity window is not a scope (§12.8, WI-522).
        var ranking = new Regex(
            @"\b(?:better|stronger|safer|more effective|superior|works better)\s+(?:than|to)\b|"
            + @"\bthe best (?:choice|medicine|drug|pill|one)\b|"
            + @"\b(?:first|second) choice\b",
            RegexOptions.IgnoreCase);
        Assert.Matches(ranking, "Levetiracetam works better than valproate for these seizures.");
        Assert.Matches(ranking, "It is the best medicine for seizures from a brain tumor.");
        Assert.DoesNotMatch(ranking, "It is one of the best-known things about this medicine.");
        Assert.DoesNotMatch(ranking, Everything);
        Assert.DoesNotContain("valproate", Everything, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("valproic", Everything, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheInteractionSectionRunsInBothDirectionsAndCarriesNoNumbers()
    {
        // PMC6657392, which is the REPLACEMENT for a dossier citation that does
        // not contain the claim (see the front matter). Its useful half is that
        // the interaction runs both ways: a steroid moves the seizure-medicine
        // level, not only the other way round.
        var section = CuratedPage.Flatten(Reader(Section("Tell them about everything else you take")));

        Assert.Matches(new Regex(@"weaken cancer treatment", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"And it goes the other way", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"A steroid, usually dexamethasone, can speed up", RegexOptions.IgnoreCase),
            section);

        // §12.4 R1: no multiplier, no milligrams, no drug-by-drug table. The
        // paper's "a factor of 3 up to 12" is exactly the shape that reads as a
        // reason to stop something.
        var numbers = new Regex(@"\b" + CountWord + @"[- ]?fold\b|\ba factor of\b|\bmg\b|\bmilligram", RegexOptions.IgnoreCase);
        Assert.Matches(numbers, "Phenytoin clears dexamethasone up to twelve-fold faster.");
        Assert.DoesNotMatch(numbers, Everything);

        // The action, which is the only part a reader can do anything with.
        Assert.Matches(new Regex(@"Carry a list", RegexOptions.IgnoreCase), section);
        Assert.Matches(new Regex(@"anything herbal", RegexOptions.IgnoreCase), section);
    }

    [Fact]
    public void ThePageCarriesNoDoseAndNoMilligrams()
    {
        // §12.4 R1 again, page-wide. A dose is the one number on a drug page a
        // frightened reader can act on tonight.
        // /review got two doses past the first version:
        //   * "HALF A GRAM morning and night" — `\d+\s*g` needs digits,
        //     `CountWord mg` has no "gram", and "morning and night" is not
        //     "twice a day"
        //   * "steps of 500 at a time"         — a BARE number with no unit,
        //     next to the word dose, which is exactly the figure a frightened
        //     reader can act on tonight
        var dose = new Regex(
            @"\b\d+\s*(?:mg|milligrams?|grams?|g)\b|"
            + @"\b" + CountWord + @"\s+(?:mg|milligrams?|grams?)\b|"
            + @"\b" + Fraction + @"\s+a\s+gram\b|"
            + @"\b(?:twice|three times|four times) a day\b|"
            + @"\bmorning and night\b|"
            + @"\b" + CountWord + @"\s+pills? (?:a|per) day\b|"
            + @"\bdoses?\b[^.]{0,30}\b\d+\b|\b\d+\b[^.]{0,30}\bdoses?\b",
            RegexOptions.IgnoreCase);

        foreach (var known in new[]
                 {
                     "The usual starting dose is 500 mg twice a day.",
                     "Most people take two pills a day.",
                     "A common starting dose is half a gram morning and night.",
                     "The dose goes up in steps of 500 at a time.",
                 })
        {
            Assert.Matches(dose, known);
        }
        Assert.DoesNotMatch(dose, Everything);
    }

    [Fact]
    public void TheEscalationTiersMatchTheRestOfTheCorpus()
    {
        // §12.8 (WI-512): an escalation tier is a site-wide property. This
        // page's list is medicine-specific rather than a second general list,
        // so the check is per SYMPTOM against the pages that already sort it —
        // and it runs in BOTH directions (§12.8, WI-519), because a bullet
        // DELETED leaves every DoesNotContain passing.
        var section = CuratedPage.Flatten(Reader(Section("When to call, and what about")));

        var ambulance = Regex.Match(section,
            @"\*\*Call an ambulance\.\*\*(.*?)(?=A seizure like)", RegexOptions.Singleline);
        Assert.True(ambulance.Success, "the ambulance list has gone");
        var now = ambulance.Groups[1].Value;

        var sameDay = Regex.Match(section,
            @"\*\*Call your team the same day for any of these:\*\*(.*?)(?=\*\*If you are not sure)",
            RegexOptions.Singleline);
        Assert.True(sameDay.Success, "the same-day list has gone");
        var today = sameDay.Groups[1].Value;

        Assert.True(now.Length > 150 && today.Length > 300,
            "one of the two tiers captured almost nothing, so every check below passes for the wrong reason");

        // The seizure tiers come from /seizures/what-to-do, which is the site's
        // authority. Read the sibling rather than trusting the memory of it.
        var seizurePage = Regex.Replace(
            CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read("seizures", "what-to-do.md"))), @"[*_]", "");
        foreach (var owed in new[] { "longer than 5 minutes", "first ever", "do not wake up" })
        {
            Assert.Contains(owed, seizurePage, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Matches(new Regex(@"more than five minutes", RegexOptions.IgnoreCase), now);
        Assert.Matches(new Regex(@"first ever", RegexOptions.IgnoreCase), now);
        Assert.Matches(new Regex(@"cannot be woken", RegexOptions.IgnoreCase), now);

        // THE DEAD ASSERTION, AND IT IS WI-524's OWN LESSON RE-COMMITTED ONE
        // ITEM LATER. The first version of the four lines below read the
        // escalation BLOCK, asserted the BLOCK says "New weakness in the face,
        // an arm or a leg" and "New confusion", and never looked at this page
        // at all — under a comment claiming "This page could have quietly filed
        // them lower". /review found that the page did not carry either symptom
        // in any tier and the test was green. So: read the block to learn where
        // the corpus files a symptom, then assert THIS PAGE files it there.
        var block = CuratedPage.Flatten(CuratedPage.EscalationBlock);

        // Confusion and new weakness: same-day in the block, and both matter
        // for this reader in particular — confusion is a known sign of too much
        // anti-seizure medicine, and one-sided weakness after a seizure is the
        // thing nobody should be deciding about at home.
        foreach (var (inBlock, onThisPage) in new[]
                 {
                     ("New confusion", @"You are confused, or somebody says you are not making sense"),
                     ("New weakness in the face, an arm or a leg",
                      @"new weakness in your face, an arm or a leg"),
                     ("Throwing up again and again", @"throwing up again and again"),
                 })
        {
            Assert.Contains(inBlock, block, StringComparison.OrdinalIgnoreCase);
            Assert.Matches(new Regex(onThisPage, RegexOptions.IgnoreCase), today);
            Assert.DoesNotMatch(new Regex(onThisPage, RegexOptions.IgnoreCase), now);
        }

        // And the corpus's ambulance line for a sudden, complete deficit, which
        // the block and /tests/biopsy and /treatments/steroids all carry and
        // this page had dropped entirely.
        Assert.Contains("suddenly not being able to speak, move one side, or see", block,
            StringComparison.OrdinalIgnoreCase);
        Assert.Matches(new Regex(@"suddenly not being able to speak, move one side, or see",
            RegexOptions.IgnoreCase), now);

        // NO TIER MAY BE SOFTENED IN PLACE. /review kept every bullet where it
        // was and walked the guard four times by editing the INSTRUCTION
        // attached to one — "This is rare, and it can usually wait until the
        // morning" on the blistering rash, "There is no rush about this one" on
        // the mood change, and a hedge on the five-minute rule. Membership was
        // the only thing being checked, so all four stayed green.
        var downgrade = new Regex(
            @"\b(?:can|could|will) (?:usually |often |probably )?wait\b|"
            + @"\bno (?:rush|hurry)\b|"
            + @"\bsoon enough\b|"
            + @"\buntil the morning\b|"
            + @"\bwaiting a little longer\b|"
            + @"\bnot urgent\b|"
            + @"\bnext appointment is\b",
            RegexOptions.IgnoreCase);
        foreach (var known in new[]
                 {
                     "This is rare, and it can usually wait until the morning.",
                     "There is no rush about this one; the next appointment is soon enough.",
                     "Most stop sooner, and waiting a little longer is usually fine.",
                 })
        {
            Assert.Matches(downgrade, known);
        }
        Assert.DoesNotMatch(downgrade, now);
        Assert.DoesNotMatch(downgrade, today);

        // The status-epilepticus bullet is ASSERTED PRESENT, not merely
        // allowlisted for sharing. /review deleted it and the ambulance capture
        // was still 390 characters, so the length check below could not notice.
        Assert.Matches(new Regex(@"another seizure starting before they have come round from the first",
            RegexOptions.IgnoreCase), now);

        // Breathing stays in the ambulance tier. /review moved it down to
        // same-day and the bullet COUNT went UP, so the reverse check rewarded
        // the downgrade.
        //
        // Asserted on ITS OWN BULLET, not on the tier. A bare
        // `Matches("trouble breathing", now)` was satisfied by a DIFFERENT
        // bullet two lines below ("sudden trouble breathing after a dose"), so
        // deleting the breathing clause from the cannot-be-woken bullet walked
        // through on both line endings. When two bullets share a phrase, the
        // guard has to name the one it means.
        Assert.Matches(new Regex(@"cannot be woken, or is having trouble breathing", RegexOptions.IgnoreCase),
            now);
        Assert.DoesNotMatch(new Regex(@"short of breath|trouble breathing", RegexOptions.IgnoreCase), today);

        // A FALL is same-day on /treatments/craniotomy, /tests/biopsy and — as
        // of WI-524 — /treatments/steroids. This page's drug causes
        // unsteadiness, so a gentler tier here would be the same invention
        // /review found one item ago.
        foreach (var (folder, slug) in new[] { ("treatments", "craniotomy"), ("tests", "biopsy") })
        {
            var sibling = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(folder, slug + ".md")));
            Assert.Matches(new Regex(@"A fall, or a hit to the head", RegexOptions.IgnoreCase), sibling);
        }
        Assert.Matches(new Regex(@"you have started falling", RegexOptions.IgnoreCase), today);
        Assert.DoesNotMatch(new Regex(@"\bfall", RegexOptions.IgnoreCase), now);

        // The reverse direction: a bullet COUNT plus the named concepts.
        var bullets = Regex.Matches(today, @"(?:^|\s)- ").Count;
        Assert.True(bullets >= 7,
            $"the same-day list is down to {bullets} bullets — a symptom has been dropped, which is the "
            + "under-triage direction (§12.8, WI-511)");

        foreach (var owed in new[]
                 {
                     "harming yourself", "mood, temper or behavior has changed", "sleepier",
                     "seizures come back", "cannot keep the pills down", "rash", "run out",
                 })
        {
            Assert.Contains(owed, today, StringComparison.OrdinalIgnoreCase);
        }

        // The ambulance tier owes the two drug emergencies NHS calls 999 for.
        foreach (var owed in new[] { "blisters, peeling skin", "swollen face, lips or tongue" })
        {
            Assert.Contains(owed, now, StringComparison.OrdinalIgnoreCase);
        }

        // A USUAL seizure is not on this page's list in any tier: that line is
        // /seizures/what-to-do's to draw, and a flat "A seizure" bullet is
        // WI-510's blocker, re-committed at WI-519 and again at WI-524. Link
        // labels stripped first, because the paragraph ROUTES to that page.
        var withoutLinks = new Func<string, string>(s => Regex.Replace(s, @"\[[^\]]*\]\([^)]*\)", " "));
        Assert.DoesNotMatch(new Regex(@"^\s*- A seizure\.", RegexOptions.Multiline), withoutLinks(now));
        Assert.DoesNotMatch(new Regex(@"\bany seizure\b|a seizure is an ambulance", RegexOptions.IgnoreCase),
            section);
        Assert.Contains("(/seizures/what-to-do)", section, StringComparison.Ordinal);

        // And the list says outright that it is not the reader's whole list.
        Assert.Matches(new Regex(@"does not replace the list your own team gives", RegexOptions.IgnoreCase), section);
        Assert.Contains("(/get-help-now)", section, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMissedDoseAdviceDoesNotOverrideTheReadersOwnMedicine()
    {
        // NHS gives the standard rule for levetiracetam specifically. This page
        // covers several medicines, so a flat instruction here is advice about
        // a drug the page cannot see. The page gives the usual rule AND sends
        // the reader to get their own.
        var actions = CuratedPage.Flatten(Reader(Section("What you can do")));

        Assert.Matches(new Regex(@"take it as soon as you remember", RegexOptions.IgnoreCase), actions);
        Assert.Matches(new Regex(@"Do not double up", RegexOptions.IgnoreCase), actions);
        Assert.Matches(new Regex(@"ask your team what they\s*want you to do next time", RegexOptions.IgnoreCase),
            actions);
        Assert.Matches(new Regex(@"it depends on the medicine", RegexOptions.IgnoreCase), actions);
    }

    [Fact]
    public void ThePageDoesNotRestateWhatOtherPagesAlreadyOwn()
    {
        // §12.8 (WI-521): the restatement check runs over the WHOLE corpus, not
        // over hand-picked neighbours. This page's nearest are
        // /treatments/steroids (written one item ago, same frame),
        // /seizures/what-to-do and /seizures/living-with.
        var pageShingles = Shingles(CuratedPage.ReaderText(Page), 8).ToHashSet();

        var others = Directory
            .EnumerateFiles(CuratedPage.PagesDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("anti-seizure-medicines.md", StringComparison.Ordinal))
            .Concat(Directory.EnumerateFiles(CuratedPage.BlocksRoot, "*.md"));

        foreach (var file in others)
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var overlaps = Shingles(CuratedPage.ReaderText(File.ReadAllText(file)), 8)
                .Where(pageShingles.Contains)
                .Where(s => !AllowedShingles.Contains(s))
                .Where(CarriesContent)
                .Distinct()
                .ToList();

            Assert.True(overlaps.Count == 0,
                $"this page restates {slug}:\n  " + string.Join("\n  ", overlaps)
                + "\n\nWrite this page's own version, or — if the two really are meant to say the identical "
                + "thing — add the sentence to DeliberatelyShared with the reason.");
        }
    }

    [Fact]
    public void TheCaregiverSectionIncludesTheBlockAndSitsAfterSlotSeven()
    {
        // §12.7 and contract item 12. Placed after slot 7 (§12.8).
        var raw = Regex.Match(Page,
            @"^## For the person caring for someone on a seizure medicine.*?(?=^## )",
            RegexOptions.Multiline | RegexOptions.Singleline).Value;
        Assert.Contains("[CAREGIVER]", raw, StringComparison.Ordinal);

        var headings = Headings();
        Assert.Equal(headings.IndexOf("What you can do") + 1,
            headings.IndexOf("For the person caring for someone on a seizure medicine"));

        // The page's own caregiver prose does not restate the block — the
        // shingle version, not the bold-lead-in check §12.8 calls insufficient.
        var block = CuratedPage.Flatten(Regex.Replace(
            File.ReadAllText(Path.Combine(CuratedPage.BlocksRoot, "caregiver.md")), @"^---.*?\n---", "",
            RegexOptions.Singleline));
        var blockShingles = Shingles(block, 8).ToHashSet();
        var own = Shingles(Reader(Section("For the person caring for someone on a seizure medicine")), 8)
            .Where(blockShingles.Contains).Where(CarriesContent).ToList();
        Assert.True(own.Count == 0, "the caregiver section restates the block:\n  " + string.Join("\n  ", own));

        // It carries the thing only a caregiver can do here, and says it in
        // front of the person rather than behind their back.
        var care = CuratedPage.Flatten(Reader(Section("For the person caring for someone on a seizure medicine")));
        // The end-to-end read found the first draft opening "You will notice the
        // mood change before they do", which is the CAREGIVER block's own "You
        // may see a change before they do" ten lines above it. The shingle
        // check could not see it (different words, same claim) and neither
        // could the bold-lead-in check §12.8 already calls insufficient. What
        // this page owes on top of the block is the TIMING, so that is what the
        // paragraph now carries.
        Assert.Matches(new Regex(@"Bring the dates, not the diagnosis", RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"against what the prescription was doing", RegexOptions.IgnoreCase), care);
        Assert.DoesNotMatch(new Regex(@"before they do", RegexOptions.IgnoreCase), care);
        Assert.Matches(new Regex(@"Say it in front of them", RegexOptions.IgnoreCase), care);

        // And it does not invent a tier of its own or hand the caregiver the
        // dose (§12.8, WI-524).
        Assert.DoesNotMatch(new Regex(@"you (?:can|could|should) (?:lower|skip|stop|hold)", RegexOptions.IgnoreCase),
            care);
    }

    [Fact]
    public void ThePageCarriesTheLibrarySlotsInTheTemplateOrder()
    {
        // §12.8: read the section for the slot list, not the last page written.
        //
        // Carried: 0, 1, 2, the WI-507 "why" section immediately after slot 2
        // (on a page whose second spine IS the refusal, splitting "why am I on
        // one" from "why am I not" puts the reader's question two screens from
        // its answer), 3, 4, 5, 6a, the self-harm section, 6c twice, the call
        // list, 7, caregiver, 9, 10, 11.
        Assert.Equal(
            [
                "The short version",                                 // 0
                "What is it, and what is it doing?",                 // 1
                "Why am I taking one?",                              // 2
                "Why has nobody given me one?",                      // 2b (WI-507)
                "What happens, step by step",                        // 3
                "How long will I be on it?",                         // 4
                "What does it feel like?",                           // 5
                "Is it the tumor, or is it the medicine?",           // 6a
                "Thoughts of harming yourself",                      // 6b
                "Does it do anything to the tumor?",                 // 6c
                "Tell them about everything else you take",          // 6c
                "Can I drive?",                                      // 6c
                "How do we know it is working?",                     // 9
                "When to call, and what about",                      // escalation
                "What you can do",                                   // 7
                "For the person caring for someone on a seizure medicine", // caregiver
                "What to ask your team",                             // 10
                "Where to go next",                                  // 11
            ],
            Headings());
    }

    [Fact]
    public void TheAnchorsArePinned()
    {
        // §12.8 (WI-508): heading anchors are a published interface, and
        // /treatments/steroids already deep-links #tumor-or-medicine here.
        foreach (var (heading, anchor) in new[]
                 {
                     ("## The short version", "short-version"),
                     ("## What is it, and what is it doing?", "what-it-is"),
                     ("## Why am I taking one?", "why"),
                     ("## Why has nobody given me one?", "no-medicine"),
                     ("## What happens, step by step", "step-by-step"),
                     ("## How long will I be on it?", "how-long"),
                     ("## What does it feel like?", "what-it-feels-like"),
                     ("## Is it the tumor, or is it the medicine?", "tumor-or-medicine"),
                     ("### The change in mood and temper", "mood-and-temper"),
                     ("### The other things that get blamed on the tumor", "not-the-tumor"),
                     ("## Thoughts of harming yourself", "self-harm"),
                     ("## Does it do anything to the tumor?", "does-it-treat-the-tumor"),
                     ("## Tell them about everything else you take", "interactions"),
                     ("## Can I drive?", "driving"),
                     ("## How do we know it is working?", "is-it-working"),
                     ("## When to call, and what about", "when-to-call"),
                     ("## What you can do", "what-you-can-do"),
                     ("## For the person caring for someone on a seizure medicine", "caregiver"),
                     ("## What to ask your team", "questions"),
                     ("## Where to go next", "where-to-go-next"),
                 })
        {
            Assert.Matches(new Regex("^" + Regex.Escape(heading) + @" \{#" + anchor + @"\}\s*$",
                RegexOptions.Multiline), Page);
        }

        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline);
        Assert.All(headings, m => Assert.Matches(@"\{#[a-z0-9-]+\}\s*$", m.Groups[1].Value.Trim()));
    }

    [Fact]
    public void ThePageUsesNeitherTheCharacterisationsNorTheMinimisations()
    {
        var body = CuratedPage.ReaderText(Page);

        CuratedPage.AssertNeverMinimises(body, "treatments/anti-seizure-medicines");

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }

        // Page-local, per §12.8 (WI-510: promote at the SECOND page). All three
        // are corpus-clean, and all three are ways a page about a drug people
        // resent taking talks them out of saying something.
        foreach (var phrase in new[] { "just a pill", "nothing to worry about", "part of the deal" })
        {
            Assert.DoesNotContain(phrase, CuratedPage.Flatten(body), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageUsesNoBritishForms()
    {
        // Two of this page's patient-level sources are NHS pages, so the pull
        // toward "999", "GP" and "DVLA" is strong here (§12.8, WI-510) — and a
        // driving page is where a British form does most damage.
        var body = CuratedPage.Flatten(CuratedPage.ReaderText(Page));

        foreach (var exemption in CuratedPage.BritishFormExemptions)
        {
            body = body.Replace(exemption, "", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in CuratedPage.BritishForms)
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var form in new[]
                 {
                     "tumour", "consultant", "specialist nurse", "chemist", "DVLA", "999",
                     "epilepsy nurse", "licence", "surgery hours",
                     // Found by the end-to-end read, in the caregiver section:
                     // the first draft had a family member saying the dose went
                     // up "a fortnight ago". WI-564's sweep, one page early.
                     "fortnight",
                 })
        {
            Assert.DoesNotContain(form, body, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotMatch(new Regex(@"\bGP\b"), body);

        // The US forms used instead, so a rewrite cannot drop the concept
        // rather than translating it.
        Assert.Contains("911", body, StringComparison.Ordinal);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate() =>
        Assert.DoesNotContain(":::", Page);

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        var headings = Headings();
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);

        // The questions carry the one that decides everything else on the page.
        var questions = Reader(Section("What to ask your team"));
        Assert.Matches(new Regex(@"treat seizures I have had, or to prevent ones I have not",
            RegexOptions.IgnoreCase), questions);
        Assert.Matches(new Regex(@"changing my mood or my temper", RegexOptions.IgnoreCase), questions);
        Assert.Matches(new Regex(@"What do I do if I miss a dose", RegexOptions.IgnoreCase), questions);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The guideline the page's second spine rests on, by its PMC id rather
        // than by the academic.oup.com URL it is usually cited at.
        Assert.Contains(urls, u => u.Contains("PMC8563323", StringComparison.Ordinal));
        foreach (var domain in new[] { "nhs.uk", "ncbi.nlm.nih.gov" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // §12.14 and the known-dead list.
        foreach (var dead in new[]
                 {
                     "medscape.com", "sciencedirect.com", "academic.oup.com", "mayoclinic.org",
                     "hopkinsmedicine.org", "mdpi.com", "journals.lww.com", "ascopubs.org",
                 })
        {
            Assert.False(urls.Any(u => u.Contains(dead, StringComparison.OrdinalIgnoreCase)),
                $"{dead} is cited, and the front matter records why it was not used");
        }

        // The dossier's own citation for the short-course claim is a PROTOCOL,
        // not a result (see the front matter). Banned as a CITATION and the
        // claim is sourced to the guideline instead — §12.14's rule is that
        // banning the citation is not the same as fixing the claim, which is
        // why TheShortCourseClaimIsScopedToTheReaderItIsTrueOf exists too.
        //
        // Checked over `urls` rather than over `front`, because the front
        // matter's own comment NAMES the rejected URL while explaining why it
        // was not used. A whole-front-matter ban makes recording a rejection
        // impossible, which is the one thing §12.13 asks these comments to do.
        foreach (var rejected in new[] { "clinicaltrials.gov", "medlineplus.gov/druginfo", "drugs.com" })
        {
            Assert.False(urls.Any(u => u.Contains(rejected, StringComparison.OrdinalIgnoreCase)),
                $"{rejected} is cited as a source; the front matter records why it was not used");
        }

        // PLAN.md §5: never an AHFS drug monograph, on the second page in the
        // corpus whose subject is a drug. The two NHS pages are not that, and
        // nothing is scraped or stored — the page is written from them and
        // links out, as WI-524 did with two NHS trust booklets.
        Assert.Contains(urls, u => u.Contains("nhs.uk/medicines", StringComparison.Ordinal));

        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }

    [Fact]
    public void TheDoorsOnTheSiblingPagesAreAppendedSentences()
    {
        // §12.8 (WI-519): a new library page nothing links to is half shipped.
        // Each door is an APPENDED sentence, so no sibling's own assertions
        // break — and each sibling's pre-existing sentence is pinned beside the
        // new door, because a door that REPLACED a paragraph passes a bare
        // Contains identically (§12.14).
        foreach (var (folder, slug, kept) in new[]
                 {
                     ("treatments", "craniotomy", "It is linked with irritability, low mood and anger"),
                     ("treatments", "steroids", "Seizure medicines cause their own version of this"),
                     ("seizures", "what-to-do", "Stopping it suddenly"),
                     ("seizures", "living-with", "For many people this is the most common trigger there is"),
                     ("tumors", "glioma", "treatment for a"),
                     ("tumors", "glioblastoma", "They also affect sleep, appetite, mood and blood sugar"),
                     ("tumors", "high-grade-glioma", "Say them at the appointment"),
                     ("tumors", "astrocytoma", "both change mood and"),
                     ("tumors", "low-grade-glioma", "levetiracetam is linked with irritability and low mood"),
                     ("tumors", "oligodendroglioma", "Driving rules depend on where you live"),
                     ("tests", "getting-ready-for-surgery", "take your dose with a sip of water"),
                 })
        {
            var text = CuratedPage.Flatten(CuratedPage.ReaderText(CuratedPage.Read(folder, slug + ".md")));
            Assert.Contains("/treatments/anti-seizure-medicines", text, StringComparison.Ordinal);
            Assert.Contains(kept, text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheSiblingClaimThisPageCorrectedStaysCorrected()
    {
        // §12.8 (WI-524), the identical shape one item later.
        // /seizures/what-to-do said "**Never stop seizure medicine suddenly.**"
        // That is right for its own reader and false for the post-operative
        // course PMC8563323 says to stop after a week — and the seizure page is
        // where a reader lands first. The rule that holds in both cases is "not
        // on your own", and the DANGER is kept rather than softened.
        var seizures = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("seizures", "what-to-do.md")));

        Assert.DoesNotMatch(new Regex(@"Never stop seizure medicine suddenly", RegexOptions.IgnoreCase), seizures);
        Assert.Matches(new Regex(@"Never change or stop seizure medicine on your own", RegexOptions.IgnoreCase),
            seizures);

        // Softening it would be the other failure. The consequence stays.
        Assert.Matches(new Regex(@"Stopping it suddenly\s*can bring on a seizure", RegexOptions.IgnoreCase),
            seizures);

        // /treatments/craniotomy carried BOTH claims this item corrected, and
        // the doors test could not see either: it pins "It is linked with
        // irritability, low mood and anger", which the defective version also
        // contains, so reverting the sentence in front of it walked through on
        // both line endings. (a) "one of the commonest medicines" is the
        // dossier over-ranking ruling 7 rejected for the new page; (b) "used to
        // prevent seizures", on a page presenting the medicine as a routine
        // part of the operation, is the Level C territory the new page teaches
        // is unsettled. /tumors/glioma was corrected the same way.
        var craniotomy = CuratedPage.Flatten(CuratedPage.ReaderText(
            CuratedPage.Read("treatments", "craniotomy.md")));
        Assert.DoesNotMatch(new Regex(@"one of the commonest medicines used to prevent seizures",
            RegexOptions.IgnoreCase), craniotomy);
        Assert.Matches(new Regex(@"the seizure medicine most often used here", RegexOptions.IgnoreCase),
            craniotomy);
        Assert.Matches(new Regex(@"Whether one is\s*given around an operation at all is unsettled",
            RegexOptions.IgnoreCase), craniotomy);

        // And it now carries the reason the absolute was wrong, so the two
        // pages cannot drift apart again.
        // D1: the short-course fact arrived on that page without its scope, on
        // the one page in the corpus where every reader HAS had a seizure. The
        // guideline scopes the short course to people who have NOT, so the
        // sibling now says which is which instead of offering both.
        Assert.Matches(new Regex(
            @"Somebody who has never had a seizure is\s*sometimes put on it only around an operation",
            RegexOptions.IgnoreCase), seizures);
        Assert.Matches(new Regex(@"Somebody who has had one usually stays on it for years",
            RegexOptions.IgnoreCase), seizures);
    }

    private static List<string> Headings() =>
        Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim())
            .ToList();

    /// <summary>Function words, which an eight-word run can be made of by coincidence.</summary>
    private static readonly HashSet<string> Stopwords =
    [
        "a", "an", "and", "are", "as", "at", "be", "been", "before", "but", "by", "can", "do",
        "does", "for", "from", "get", "go", "had", "has", "have", "how", "i", "if", "in", "into",
        "is", "it", "its", "like", "may", "me", "more", "much", "my", "no", "not", "of", "on",
        "one", "or", "other", "our", "out", "over", "own", "so", "some", "than", "that", "the",
        "their", "them", "then", "there", "these", "they", "this", "to", "up", "was", "we",
        "were", "what", "when", "which", "who", "will", "with", "would", "you", "your",
    ];

    private static bool CarriesContent(string shingle) =>
        shingle.Split(' ').Count(w => !Stopwords.Contains(w)) >= 3;

    /// <summary>Word shingles, headings and link targets removed first (§12.8, WI-521).</summary>
    private static IEnumerable<string> Shingles(string text, int n)
    {
        text = Regex.Replace(text, @"^#{1,6} .*$", " ", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\]\([^)]*\)", "] ");
        var words = Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9']+").Select(m => m.Value).ToList();
        for (var i = 0; i + n <= words.Count; i++)
        {
            yield return string.Join(' ', words.Skip(i).Take(n));
        }
    }
}

/// <summary>The page as served, and the doors that lead to it.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public sealed class AntiSeizureMedicinesPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/treatments/anti-seizure-medicines";

    private readonly WebApplicationFactory<Program> _factory;

    public AntiSeizureMedicinesPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Anti-seizure medicines for a brain tumor", html);
        Assert.Contains("The dose is never yours to change", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromEveryDoorItWasGiven()
    {
        // The pages a reader is standing on when they have just been handed a
        // seizure medicine, or told they are not getting one.
        var client = _factory.CreateClient();

        foreach (var door in new[]
                 {
                     "/treatments/craniotomy", "/treatments/steroids", "/seizures/what-to-do",
                     "/seizures/living-with", "/tumors/glioma", "/tumors/glioblastoma",
                     "/tumors/high-grade-glioma", "/tumors/astrocytoma", "/tumors/low-grade-glioma",
                     "/tumors/oligodendroglioma", "/tests/getting-ready-for-surgery",
                 })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/seizures/what-to-do", "/seizures/living-with", "/treatments/steroids",
            "/treatments/craniotomy", "/tests/getting-ready-for-surgery", "/tumors", "/get-help-now");

    [Fact]
    public async Task TheDeepLinksIntoTheSiblingPagesLandOnRealAnchors() =>
        // §12.8 (WI-508): a link to a heading that does not exist resolves as a
        // healthy 200 and drops the reader at the top of a long page. This page
        // deep-links /seizures/living-with#driving and
        // /treatments/steroids#tumor-or-steroid.
        await CuratedPage.AssertFragmentLinksResolve(_factory.CreateClient(), Url);

    [Fact]
    public async Task TheTermThisPageDefinesIsSuppressedHereAndStillFiresElsewhere()
    {
        // §12.8 (WI-509): this page defines levetiracetam, so a popover
        // repeating the paragraph beneath it is noise. WI-512's other half
        // first: the word is in the prose, so the suppression is not a no-op.
        var body = CuratedPage.ReaderText(CuratedPage.Read("treatments", "anti-seizure-medicines.md"));
        Assert.Contains("levetiracetam", body, StringComparison.OrdinalIgnoreCase);

        var client = _factory.CreateClient();
        Assert.DoesNotContain("def-levetiracetam", await client.GetStringAsync(Url));

        // And the entry is still reachable, so suppressing it here does not
        // switch off its only live use (§12.8, WI-519) — which is the risk on
        // this page specifically. /treatments/craniotomy uses the word and
        // SUPPRESSES the tooltip (its marker line at the top of the file), so
        // after this page suppresses it too, /tumors/low-grade-glioma is the
        // one page in the corpus where the popover still fires. The first
        // version of this assertion pointed at craniotomy and went red for
        // exactly that reason. If low-grade-glioma ever stops saying the word,
        // this goes red rather than the entry going quietly dead.
        Assert.Contains("def-levetiracetam", await client.GetStringAsync("/tumors/low-grade-glioma"));
    }

    [Fact]
    public async Task TheTooltipsForWordsThisPageDoesNotDefineStillFire()
    {
        // The other direction, which is the one WI-510 got wrong: a word this
        // page uses and does NOT define keeps its tooltip here.
        Assert.Contains("def-dexamethasone", await _factory.CreateClient().GetStringAsync(Url));
    }
}
