using System.Net;
using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// Shared reading helpers for the 29 P5 library pages (content-pipeline §12.8).
/// WI-506 wrote these inside its own test class; WI-507 is the second page, so
/// they move here rather than being copied 28 times.
/// </summary>
internal static class CuratedPage
{
    /// <summary>
    /// The repo root, walked up from the test output directory. Public since
    /// WI-548: a fourth and fifth hand-rolled copy appeared in one new file, and
    /// §12.8's factor-at-the-second-use rule is two helpers down this same class.
    /// </summary>
    public static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BrainHarbor.slnx")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName
            ?? throw new InvalidOperationException("could not find the repo root from the test output directory");
    }

    public static string Read(params string[] pathUnderPages) => File.ReadAllText(Path.Combine(
        [RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages", .. pathUnderPages]));

    private static string ContentRoot =>
        Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content");

    private static string PagesRoot => Path.Combine(ContentRoot, "pages");

    /// <summary>Where the shared blocks live (§3a), for tests that compare a page against one.</summary>
    public static string BlocksRoot => Path.Combine(ContentRoot, "blocks");

    /// <summary>
    /// The files whose content reaches MORE than one page: the shared blocks
    /// (composed into every including page, §3a) and the glossary entries
    /// (whose tooltips fire site-wide). A mistake in one of these has the
    /// widest blast radius on the site, and neither lives under `pages/`.
    /// </summary>
    public static IEnumerable<(string Slug, string Text)> SharedSources() =>
        new[] { BlocksRoot, Path.Combine(ContentRoot, "glossary") }
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
                .Select(f => (
                    Slug: Path.ChangeExtension(Path.GetRelativePath(ContentRoot, f), null)!.Replace('\\', '/'),
                    Text: File.ReadAllText(f))));

    /// <summary>
    /// Every curated page AND every shared block, as (slug, raw text), for
    /// rules that hold site-wide rather than page by page.
    ///
    /// Blocks are in here deliberately. A block is composed INTO the page at
    /// render time (§3a), so text that lands in `Content/blocks/caregiver.md`
    /// is text on eighteen tumor hubs — and a site-wide rule that reads only
    /// `pages/` is blind to the one file with the widest blast radius.
    /// </summary>
    public static IEnumerable<(string Slug, string Text)> AllPages() =>
        new[] { PagesRoot, Path.Combine(ContentRoot, "blocks") }
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
                .Select(f => (
                    Slug: Path.ChangeExtension(Path.GetRelativePath(ContentRoot, f), null)!.Replace('\\', '/'),
                    Text: File.ReadAllText(f))));

    /// <summary>
    /// The phrasings that turn "here is what the lab measured" into "here is
    /// whether that is good news". Shared, because 29 library pages inherit the
    /// rule and WI-509 is where it will be hardest to hold: the sources say
    /// these things out loud (Johns Hopkins' own glossary calls IDH mutation
    /// "associated with a better prognosis"), so borrowed phrasing is how it
    /// gets onto a page, not a decision anyone makes.
    /// </summary>
    /// <remarks>
    /// This is a plain substring check, deliberately: it is predictable, and a
    /// phrase only belongs here if there is no correct sentence that contains
    /// it. WI-509 proposed adding "good sign" and "bad sign" and then ran the
    /// candidate list over the pages already shipped, per §12.8 — the wait page
    /// says "That is normal and it is <em>not</em> a bad sign", which is the
    /// natural way to write that reassurance and is exactly right. Both were
    /// dropped rather than ship a rule that fails a correct page. The ten
    /// phrases added below have no occurrence anywhere in the corpus.
    ///
    /// WI-511 ran the WHOLE list over the WHOLE corpus for the first time —
    /// §12.8 asks for that before ADDING a phrase, and nobody had asked it of
    /// the phrases already here. <c>"bad news"</c> failed, on a page shipped
    /// eight items ago: <c>/seizures/what-to-do</c> says "a seizure is
    /// <em>not</em> automatically bad news about the tumor", which is correct,
    /// is the natural way to write it, and is the identical negation shape
    /// that got "good sign"/"bad sign" rejected at WI-509. Only the marker
    /// page and the craniotomy page asserted this list, and neither uses the
    /// phrase, which is why a substring ban sat over a correct sentence for
    /// eight items without a single test going red.
    ///
    /// <c>"bad news"</c> alone moved to <see cref="RejectedCharacterisations"/>.
    /// <c>"good news"</c> STAYS: review pushed back on dropping it as a pair,
    /// correctly — it has no occurrence anywhere in the corpus, and retiring a
    /// working guard for symmetry with a broken one is a net loss. WI-509
    /// dropped "good sign"/"bad sign" together because BOTH had correct uses
    /// in view; here only one does.
    /// </remarks>
    public static readonly string[] Characterisations =
    [
        "better outlook", "worse outlook", "better outcome", "worse outcome",
        "better prognosis", "worse prognosis", "favorable", "favourable",
        "good news", "more aggressive", "less aggressive",
        "responds better", "respond better", "responds well", "does better",
        "the good one", "the bad one",

        // WI-509. The marker page is where the sources say these out loud:
        // ACS writes "better outlook" for IDH and MGMT, and Johns Hopkins'
        // glossary "associated with a better prognosis". These are the
        // remaining shapes that vocabulary arrives in.
        // WI-517. The list banned "responds better" and "responds well" but not
        // the superlative, so an outlook gate saying this tumor is "the one that
        // responds best" sailed through the guard written to stop exactly that.
        // Run over the corpus per §12.8: no other occurrence.
        "responds best",
        "better response", "worse response", "poor response", "poorer response",
        "poor outcome", "poorer outcome", "longer survival", "shorter survival",
        "better type", "worse type",

        // WI-510, the first treatment page. A treatment page characterises an
        // OUTCOME rather than a lab value, so the vocabulary is different:
        // these are the ways "here is what was done" turns into "here is
        // whether it worked".
        "nothing to worry about", "successful surgery", "surgery was a success",

        // WI-528. The list held "more aggressive" and "less aggressive" and not
        // the bare word, so "they are aggressive once they reach the brain"
        // walked through the guard written to stop exactly that — the same
        // comparative-versus-absolute hole WI-517 found with "responds best".
        // Run over the corpus per §12.8 before adding: the word appears in no
        // page, block or glossary entry, so nothing legitimate loses by it.
        "aggressive",

    ];

    /// <summary>
    /// Candidates run over the corpus (§12.8) and deliberately REJECTED, kept
    /// here so the next page does not propose them again and re-do the work.
    ///
    /// All were corpus-clean; that is not sufficient. The list is a substring
    /// check, so a phrase only belongs on it if no CORRECT sentence contains
    /// it — and for each of these a correct sentence is easy to write, usually
    /// a negation, which is exactly how WI-509 lost "good sign"/"bad sign":
    ///
    /// - "good result" — failed on the spot. WI-510's own slot 2 says "it
    ///   changes what a good result looks like", which is the whole point of
    ///   asking what the goal of the operation is.
    /// - "completely gone", "all clear" — a page explaining that a clear scan
    ///   is NOT the same as cure has to be able to print the phrase it is
    ///   dismantling. WI-510 is that page.
    /// - "full recovery" — "we cannot promise a full recovery" is correct.
    /// - "back to normal" — "you may not feel back to normal for months" is
    ///   correct, and is what WI-511 and WI-512 will need to write.
    /// - "went well" — "even when surgery went well, recovery is hard" is
    ///   correct, and is close to the caregiver section's actual argument.
    /// - "bad news" — WI-511 demoted this one from the live list.
    ///   `/seizures/what-to-do` already says "a seizure is not automatically
    ///   bad news about the tumor", which is correct and is the natural way to
    ///   write that reassurance. Its partner "good news" stayed on the live
    ///   list: it is corpus-clean and there is no reason to retire a working
    ///   guard alongside a broken one.
    /// </summary>
    public static readonly string[] RejectedCharacterisations =
    [
        "good result", "completely gone", "all clear",
        "full recovery", "back to normal", "went well", "bad news",

        // WI-537 added "do well" to the LIVE list and /review round 2 sent it here,
        // correctly. It was written for one shape, "do well with today's treatment" on
        // /tumors/medulloblastoma, and as a bare substring it fails the standard this
        // file applies to everything else: a phrase belongs only if no CORRECT sentence
        // contains it. These pediatric hubs are written in a school register, where "ask
        // what support helps your child do well at school" is the natural sentence.
        // (/review round 3: that example is HYPOTHETICAL, not a quotation. The
        // medulloblastoma hub's everyday-life section says "ask what help can be put in
        // place", and "do well" appears nowhere under Content/. Recorded precisely because
        // WI-536 found a justification can be false of the page it cites, and a reason
        // deserves the same check as a claim.) WI-538's hub is next. It is the
        // same reason "went well" and "back to normal" are here, and the negation shape
        // ("we cannot promise she will do well") is how WI-509 lost good sign/bad sign.
        //
        // The sentence that prompted it is gone for a better reason anyway: it was an
        // ungated group-level outcome claim, and MedulloblastomaPageContentTests pins
        // the replacement and bans the "responds well/best/to today" family on that page.
        "do well",
    ];

    /// <summary>
    /// The phrasings that tell a reader a serious treatment is nothing much.
    /// WI-510 wrote this page-locally for the craniotomy page and §12.8 said
    /// the SECOND treatment page promotes it; WI-511 is that page, so here it
    /// is, generalised from "operation" to the treatment vocabulary.
    ///
    /// Use through <see cref="AssertNeverMinimises"/>, never as a bare
    /// substring ban: "this is not a routine treatment" is a correct sentence
    /// and is exactly what a page like this wants to write.
    /// </summary>
    public static readonly string[] Minimisations =
    [
        "routine operation", "routine procedure", "routine treatment",
        "simple operation", "simple procedure", "simple treatment",
        "minor operation", "minor procedure", "minor treatment",
        "straightforward operation", "straightforward procedure", "straightforward treatment",
        "easy operation", "easy procedure", "easy treatment",
        "harmless operation", "harmless procedure", "harmless treatment",
        "nothing to it", "no big deal", "piece of cake", "walk in the park",
    ];

    /// <summary>
    /// Minimisation candidates run over the corpus and REJECTED, with the
    /// reason, so the next treatment page does not re-propose them. All four
    /// are corpus-clean; corpus-clean is not the test (§12.8).
    ///
    /// - "quick operation", "quick procedure", "quick treatment" — speed is a
    ///   FACT, not a judgement. Stereotactic radiosurgery genuinely is a
    ///   quicker treatment than six weeks of daily visits, and a page saying
    ///   so is being accurate rather than reassuring. "Routine" is the word
    ///   that makes a claim about how much it matters; "quick" is not.
    /// - "painless procedure" — radiation IS painless. ACS says "the treatment
    ///   is not painful" and NBTS says "like an X-ray, radiation is painless",
    ///   and WI-511 prints it because a reader dreading pain deserves the
    ///   answer. Banning it would fail the page it was written for.
    /// - "not a big deal" — the negation-aware check would WAVE THIS THROUGH,
    ///   because the "not" is right there, while "this is not a big deal" is
    ///   precisely the minimising sentence the rule exists to catch. A
    ///   negation-aware substring test cannot express it, so it is left out
    ///   rather than shipped broken and believed.
    /// - "easy enough" — "it is easy enough to ask" is correct and natural.
    /// </summary>
    public static readonly string[] RejectedMinimisations =
    [
        "quick operation", "quick procedure", "quick treatment",
        "painless procedure", "not a big deal", "easy enough",
    ];

    /// <summary>
    /// No sentence tells the reader this treatment is nothing much — unless it
    /// is telling them the opposite, which is the point.
    ///
    /// Negation-aware for the WI-509 reason: a bare substring ban on this
    /// vocabulary fails "this is not a minor operation", which is the correct
    /// and natural sentence, and a rule that fails a correct page is worse
    /// than no rule (§12.8).
    /// </summary>
    public static void AssertNeverMinimises(string readerText, string slug)
    {
        // Whitespace-normalised first. The corpus is hard-wrapped, so a
        // two-word phrase routinely has a newline in the middle of it — which
        // is how a WI-509 fix test missed the exact string it was written to
        // catch, and how WI-511's first British-spelling gate walked straight
        // past "a\nlift" in the shared caregiver block.
        var text = Flatten(readerText);

        foreach (var phrase in Minimisations)
        {
            foreach (Match match in Regex.Matches(text, Regex.Escape(phrase), RegexOptions.IgnoreCase))
            {
                var before = text[Math.Max(0, match.Index - 40)..match.Index];

                // Anchored to the same CLAUSE, and widened to the negations
                // English actually uses. Review found the first version broken
                // in both directions with a bare 30-character lookback for
                // not/never/hardly:
                //
                //   FALSE PASS: "it is not painful, and it is a simple
                //   procedure" — the "not" belongs to the other clause but
                //   sits inside the window, so the minimisation goes through.
                //   Any nearby negation bought a free pass.
                //
                //   FALSE FAIL: "there is no such thing as a simple
                //   procedure", "far from a routine treatment", "isn't a minor
                //   procedure" — all correct, all flagged, and a rule that
                //   fails a correct page is worse than no rule (§12.8).
                //
                // `[^.,;:]{0,20}$` is what does the work: the negation has to
                // be close AND on this side of the nearest punctuation.
                Assert.True(
                    Regex.IsMatch(before, @"\b(not|never|hardly|no|n't|far from)\b[^.,;:]{0,20}$",
                        RegexOptions.IgnoreCase),
                    $"{slug} calls this a \"{phrase}\" without negating it");
            }
        }
    }

    /// <summary>
    /// The British forms that no gate looked for until WI-511 built one.
    ///
    /// WI-510's first draft shipped seven of them (`anaesthetist`,
    /// `jewellery`, `theatre`, `physiotherapist`, `tablets`, …) and every
    /// check passed: reading grade, ContentCheck and 1,039 tests are all blind
    /// to a page that is written correctly for a different country. The corpus
    /// is US throughout and a reader in Ohio should not meet a page that
    /// sounds like it is about somebody else's health system.
    ///
    /// An explicit list rather than an `-ise` suffix pattern, deliberately:
    /// "advise", "exercise", "promise", "raise" and "surprise" are all correct
    /// US English and a suffix rule fails every one of them.
    ///
    /// Five obvious-looking entries were written and then REMOVED, because
    /// each one is a substring of a word that is correct in US English — the
    /// same "a rule that fails a correct page is worse than no rule" trap the
    /// ban lists keep falling into, arriving here as a stemming bug:
    ///
    /// - `"specialis"` matches **specialist**, which the corpus uses 4 times.
    /// - `"characteris"` matches **characteristic**.
    /// - `"organis"` matches **organism**.
    /// - `"realis"` matches **realistic**.
    /// - `"analyse"` matches **analyses**, a correct US plural noun. The
    ///   British-only forms are the inflections, so those are listed instead.
    ///
    /// Also considered and left out: `"tablet"`, because US drug labeling uses
    /// it too ("take one tablet") and only "tablets" meaning *pills in
    /// general* is the British idiom — the word cannot carry the distinction.
    /// WI-511's own draft said "a tablet called memantine" and it was fixed by
    /// reading, not by this list.
    ///
    /// `"radiotherapy"` was on the list and came off after review, which is
    /// the same trap one level up: it is not a spelling variant at all. It is
    /// standard US medical vocabulary inside named techniques — Stereotactic
    /// Body Radiotherapy, hippocampal-avoidance whole-brain radiotherapy — and
    /// `glossary/stereotactic-radiosurgery.md` already cites a source title
    /// containing it. The first page that had to name SBRT in body prose would
    /// have failed a gate that was right about nothing. British *usage* of the
    /// word is caught by the idioms below and by reading.
    ///
    /// The entries are matched against WHITESPACE-NORMALISED body text, and
    /// that is load-bearing rather than tidy: the corpus is hard-wrapped, and
    /// the first version of this gate read raw text and walked straight past
    /// `"a lift"` in `blocks/caregiver.md`, where the wrap falls between the
    /// two words. That block composes into eighteen tumor hubs.
    /// </summary>
    public static readonly string[] BritishForms =
    [
        // Spellings, matched as prefixes so inflections are caught too.
        "tumour", "centre", "programme", "behaviour", "colour", "favour",
        "labour", "anaesth", "oesoph", "paediatr", "haemat", "haemorrh",
        "oedema", "jewellery", "physiotherap", "whilst", "amongst",
        "theatre", "plaster cast", "casualty department",
        "aluminium", "storey", "licence", "defence", "practise",
        "analysed", "analysing", "organise", "organised", "organising",
        "organisation", "recognis", "minimis", "apologis", "hospitalis",
        // Added after this very page shipped "follow-up for them is not
        // standardised" past the first version of the list. The `-ise` family
        // is larger than it looks from the outside, and the only way to find
        // the next one is to keep reading.
        "standardis", "normalis", "prioritis", "utilis", "emphasise",

        // WI-529 ran the whole corpus for doubled-consonant and -ement forms,
        // which no previous sweep had looked at, and found THREE live British
        // spellings on FOUR shipped files: `travelled` three times on
        // /tumors/brain-metastases, `judgement` on
        // /treatments/anti-seizure-medicines and /tumors/meningioma, and
        // `labelled` on /tests/waiting-for-results and in
        // glossary/amended-report.md, which is a tooltip that fires site-wide.
        // All fixed at source. None is a substring of a correct US word:
        // "judgment", "labeled" and "traveled" all lose a letter rather than
        // gaining one, so the British form cannot hide inside the US one.
        "travelled", "travelling", "traveller",
        "judgement", "labelled", "labelling",

        // WI-530. "grey" reached a draft of /tests/planning-scans ("your brain's
        // grey parts") and no sweep had ever asked about the colour family —
        // WI-529 swept doubled consonants and -ement, and every sweep before it
        // looked at -ise, -our and idiom. Corpus-clean at the time of adding
        // (the only other occurrence is a VERBATIM SOURCE QUOTE inside that
        // page's front matter, which this gate does not read: it is body text
        // only, for the reason the class comment gives). "grey" is not a
        // substring of "gray", so the US form cannot hide the British one.
        "grey",

        // WI-531. A draft of /treatments/stereotactic-radiosurgery wrote
        // "sitting in a car park" and "the hard bit" — and neither is a
        // spelling, so no previous sweep could have caught them. The everyday
        // NOUNS are the family nobody had asked about: a US reader parks in a
        // parking lot, buys gas rather than petrol, and drives on a highway.
        // All four are corpus-clean and none is a substring of a US word.
        "car park", "petrol", "motorway", "dual carriageway",
        //
        // TWO CANDIDATES WERE RUN OVER THE CORPUS AND REJECTED, and the reason
        // is recorded so the next item does not re-reach the wrong answer
        // (§12.8, WI-510, the RejectedCharacterisations pattern):
        //   * "straight away" — /review proposed it, and §12.10 does describe
        //     it as an idiom the escalation block avoids. But it is LIVE on
        //     /treatments/chemotherapy in four places, including the fever
        //     rule ("means calling your team straight away, at any hour"),
        //     which is the single most load-bearing sentence in the treatment
        //     library. Banning it would fail a shipped page, and §12.8's rule
        //     is that a phrase belongs on this list only if no correct
        //     sentence contains it.
        //   * "chemist" — matches "chemistry" on /tests/waiting-for-results and
        //     "immunohistochemistry" on /tests/molecular-markers and in a
        //     glossary entry. WI-511's stemming defect exactly: a substring of
        //     a correct word reads like a working rule.

        // Idioms. The half WI-510's draft actually got wrong ("you will be got
        // up", "tablets") was never about spelling, and a reader in Ohio is
        // offered "a lift" or given fluids "through a drip" by a page that
        // sounds like it is about a different health system.
        //
        // These are DETERMINER-BOUND on purpose, and that is a known limit
        // rather than an oversight. Bare "drip" cannot be banned: "IV drip"
        // and "post-nasal drip" are both standard US usage, so the rule would
        // fail a correct page (§12.8). WI-512's break harness proved the cost
        // of the compromise — it mutated "no IV" to "no drip" and the gate did
        // not fire, because "no drip" was not one of the forms listed. The
        // determiners below are the ones a sentence actually uses; extend the
        // list when a new one turns up rather than reaching for a bare stem.
        "a lift", "a drip", "the drip", "no drip", "on a drip", "by drip",
        "casualty",

        // WI-549. Found by the break harness, not by review: a planted
        // "Ask again in a fortnight" SURVIVED on both line endings, because
        // the word was British, unmistakably so, and on no list. A US reader
        // does not measure anything in fortnights. Checked over the whole
        // corpus before adding, per the rule above: zero occurrences, so no
        // correct sentence contains it.
        "fortnight",
    ];

    /// <summary>
    /// Exact phrases that contain a <see cref="BritishForms"/> entry and are
    /// nonetheless correct, because they are proper nouns. Stripped before the
    /// scan rather than removed from the list: "The Brain Tumour Charity" is
    /// an organization's actual name and Americanising a citation would be a
    /// worse defect than the one the gate prevents.
    /// </summary>
    public static readonly string[] BritishFormExemptions =
    [
        "Brain Tumour Charity", "brain tumour charity",
        // WI-547: a second UK organization, cited by name for the same reason.
        // Brain Tumour Research is a different charity from the one above, and
        // crediting a source means spelling its name the way it is spelled.
        "Brain Tumour Research", "brain tumour research",
    ];

    /// <summary>
    /// Every in-site link inside the page's &lt;article&gt; resolves, and the
    /// "Where to go next" list still offers the doors it is supposed to.
    ///
    /// Shared rather than copied because §12.8's own rule — factor at the
    /// SECOND use, not the fifth — applies to the tests as much as the prose,
    /// and the three copies this replaces had already drifted apart. The
    /// scoping is the load-bearing part: the layout contributes ~13 links, so a
    /// whole-page count can never fall to zero, and a canary counted over the
    /// whole article is satisfied by the body's own links (WI-506 and WI-508
    /// both found this the hard way).
    /// </summary>
    /// <inheritdoc cref="AssertLinksResolveIn"/>
    public static Task AssertLinksResolve(
        HttpClient client, string url, params string[] requiredOnward) =>
        AssertLinksResolveIn(client, url, "where-to-go-next", requiredOnward);

    /// <summary>
    /// As above, but naming the section that holds the onward doors.
    ///
    /// The id was hard-coded to <c>where-to-go-next</c> until WI-513, because
    /// every page that had used this helper was a §12.8 LIBRARY page and they
    /// all end that way. A §12.3 tumor hub does not: its last section is
    /// "Where to get support", section 15 of a different template. The helper
    /// was quietly asserting the library template on any page that called it,
    /// which is exactly the coupling this item exists to find before the shape
    /// is copied twenty-three more times.
    /// </summary>
    /// <remarks>
    /// Deliberately a DIFFERENT NAME rather than an overload. Adding
    /// <c>(client, url, string, params string[])</c> alongside
    /// <c>(client, url, params string[])</c> made C# prefer the new one for
    /// every existing caller, silently reinterpreting their first required
    /// link as the section id — seven pages went red at once, each looking for
    /// a section called "/tests/waiting-for-results". A params overload with a
    /// matching prefix is a trap; the compiler resolved it exactly as
    /// specified and the result was nonsense.
    /// </remarks>
    public static async Task AssertLinksResolveIn(
        HttpClient client, string url, string onwardSectionId, params string[] requiredOnward)
    {
        var html = await client.GetStringAsync(url);

        var start = html.IndexOf("<article", StringComparison.Ordinal);
        var end = html.IndexOf("</article>", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, $"{url} did not render an article");

        var broken = new List<string>();
        var checkedLinks = 0;

        foreach (Match match in Regex.Matches(html[start..end], "href=\"(/[^\"#?]*)\""))
        {
            var target = match.Groups[1].Value;
            if (target.StartsWith("/css/") || target.StartsWith("/js/"))
            {
                continue;
            }

            checkedLinks++;
            if ((await client.GetAsync(target)).StatusCode != HttpStatusCode.OK)
            {
                broken.Add(target);
            }
        }

        Assert.True(broken.Count == 0, $"{url} links to:\n" + string.Join("\n", broken.Distinct()));

        var next = html.IndexOf($"id=\"{onwardSectionId}\"", StringComparison.Ordinal);
        Assert.True(next > 0, $"{url} has no '{onwardSectionId}' section");

        var onward = Regex.Matches(html[next..end], "href=\"(/[^\"#?]*)\"")
            .Select(m => m.Groups[1].Value).ToList();

        // Named, not counted. A floor is met by any set of links, so the door
        // that actually matters can go while the count stays healthy — and for
        // pages written as a deliberate sequence, the hand-off to the next page
        // IS the content.
        foreach (var required in requiredOnward)
        {
            Assert.Contains(required, onward);
        }

        Assert.True(onward.Count >= requiredOnward.Length,
            $"'Where to go next' on {url} offers {onward.Count} doors out");

        // Deliberately NOT asserting that the body links anywhere outside this
        // section. The WI-508 page happens to (it deep-links into the wait
        // page), and generalising that broke the MRI page, whose every link is
        // legitimately in "Where to go next" — a rule that fails a correct page
        // is worse than no rule. Where a mid-body link IS load-bearing, the
        // page's own tests name it.
        Assert.True(checkedLinks >= onward.Count, $"{url}: fewer links checked than found");
    }

    /// <summary>
    /// Every <c>#fragment</c> link in the page's article points at an id that
    /// the target page actually renders.
    ///
    /// <see cref="AssertLinksResolve"/> cannot see these: its regex is
    /// <c>href="(/[^"#?]*)"</c>, which stops at the <c>#</c>, so a link to a
    /// heading that does not exist resolves as a perfectly healthy 200 and
    /// lands the reader at the top of a long page with no sign anything went
    /// wrong. That is the exact failure §12.8 (WI-508) describes for heading
    /// anchors, and the check for it did not exist until a page needed it:
    /// WI-513 is the first page in the corpus to deep-link another one, and its
    /// first draft pointed at <c>/tests/pathology-report#grade</c>, which was
    /// not an anchor on that page at all.
    /// </summary>
    public static async Task AssertFragmentLinksResolve(HttpClient client, string url)
    {
        var html = await client.GetStringAsync(url);

        var start = html.IndexOf("<article", StringComparison.Ordinal);
        var end = html.IndexOf("</article>", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, $"{url} did not render an article");

        var broken = new List<string>();
        var checkedFragments = 0;

        foreach (Match match in Regex.Matches(html[start..end], "href=\"(/[^\"?#]*)#([^\"]+)\""))
        {
            var target = match.Groups[1].Value;
            var fragment = match.Groups[2].Value;
            if (target.StartsWith("/css/") || target.StartsWith("/js/"))
            {
                continue;
            }

            checkedFragments++;
            var targetHtml = await client.GetStringAsync(target);
            if (!targetHtml.Contains($"id=\"{fragment}\"", StringComparison.Ordinal))
            {
                broken.Add($"{target}#{fragment}");
            }
        }

        Assert.True(broken.Count == 0,
            $"{url} deep-links anchors that do not exist on the target page:\n  "
            + string.Join("\n  ", broken.Distinct()));

        Assert.True(checkedFragments > 0,
            $"{url} has no fragment links, so this assertion proved nothing — "
            + "drop the call rather than leaving a green test that cannot fail");
    }

    /// <summary>Whitespace-normalised: the source is hard-wrapped, and a sentence that happens to break across lines is not a content change.</summary>
    public static string Flatten(string page) => Regex.Replace(page, @"\s+", " ");

    /// <summary>
    /// One "## " section, flattened. Section-scoped assertions are the whole
    /// point: a whole-page Contains() passes when the reassuring line has
    /// drifted into the footer, which is precisely the failure these rules
    /// exist to catch (§12.6 is about WHERE a sentence sits, not whether it is
    /// present somewhere).
    /// </summary>
    public static string Section(string page, string heading)
    {
        // The trailing `{#id}` is optional so a caller names the section by the
        // words a reader sees, not by the anchor bolted onto it. WI-509 made
        // explicit anchors the rule (§12.8) precisely so wording and interface
        // move independently; a Section() that demanded the anchor in its
        // argument would tie them straight back together.
        var match = Regex.Match(
            // `\s*$` on the tail, not `[ \t]*$`: this repo is core.autocrlf=true
            // and .NET's multiline `$` does not match before a `\r`, so the
            // whitespace class has to be the thing that eats it (WI-501, WI-507).
            page, $@"^## {Regex.Escape(heading)}(?:[ \t]*\{{\#[^}}]+\}})?\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);

        Assert.True(match.Success, $"the page has no '## {heading}' section");
        return Regex.Replace(match.Groups[1].Value, @"\s+", " ").Trim();
    }

    /// <summary>Sentences of a section, so "first" and "last" mean something.</summary>
    public static string[] SentencesOf(string section) =>
        [.. Regex.Split(section, @"(?<=[.!?])\s+").Where(s => s.Trim().Length > 0)];

    /// <summary>The body, with the YAML front matter removed.</summary>
    public static string Body(string page) => page[(page.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..];

    /// <summary>
    /// The body as the READER meets it: the WI-105 authoring markers removed,
    /// the way <c>GlossaryMarker</c> removes them before anything renders.
    ///
    /// Not cosmetic. WI-509 suppresses fifteen tooltips with <c>!%term%</c>, and
    /// <c>!%H3 G34%!%BRAF%</c> puts the characters "34%" into the source — which
    /// tripped that page's own no-percentages rule on text no reader will ever
    /// see. A prose rule asserted against raw source is asserting against
    /// something that is not the prose.
    /// </summary>
    public static string ReaderText(string page) =>
        Regex.Replace(Regex.Replace(Body(page), @"!%(.+?)%", ""), @"%%(.+?)%%", "$1");

    /// <summary>
    /// The page with its shared blocks resolved, through the REAL composer
    /// (<see cref="ContentBlocks.Compose"/>) rather than a test's own
    /// reimplementation of it — a private copy of the include rules would
    /// drift from the one the site actually runs.
    ///
    /// WI-514, and this is the trap the item was built on. Once a hub includes
    /// <c>[CROSSWALK]</c> instead of spelling the crosswalk out, every
    /// assertion about those words made against the RAW page is asserting
    /// against the literal string "[CROSSWALK]". WI-513's own retired-name
    /// test went green that way and proved nothing. A rule about a block's
    /// prose has to run on the composed page.
    /// </summary>
    public static string Composed(string page, string describedAs = "a curated page") =>
        ContentBlocks.Compose(page, ContentBlockStore.Load(BlocksRoot), describedAs).Markdown;

    /// <summary>One "## " section of the COMPOSED page — see <see cref="Composed"/>.</summary>
    public static string ComposedSection(string page, string heading) =>
        Section(Composed(page), heading);

    /// <summary>
    /// One <c>###</c> subsection of the COMPOSED page, up to the next heading of any
    /// level. LF-normalised and NOT flattened, so a caller can still slice paragraphs
    /// out of it.
    ///
    /// WI-537 needed this on two hubs in a single change, which is §12.8's
    /// factor-at-the-second-use threshold. It is shared rather than copied because that
    /// item had just promoted another guard for exactly this reason and then duplicated
    /// this helper anyway (/review round 1).
    /// </summary>
    public static string ComposedSubsection(string page, string heading)
    {
        var raw = Composed(page).Replace("\r\n", "\n");
        var start = raw.IndexOf($"\n### {heading}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"the composed page has no '### {heading}' subsection");
        var end = raw.IndexOf("\n#", start + 5, StringComparison.Ordinal);
        return end < 0 ? raw[start..] : raw[start..end];
    }

    /// <summary>The YAML front matter, without the body.</summary>
    public static string FrontMatter(string page) => page[..page.IndexOf("\n---", 3, StringComparison.Ordinal)];

    /// <summary>Where the curated pages live, for tests that walk a whole directory.</summary>
    public static string PagesDirectory => PagesRoot;

    /// <summary>The escalation block's own text.</summary>
    public static string EscalationBlock =>
        File.ReadAllText(Path.Combine(BlocksRoot, "escalation.md"));

    /// <summary>
    /// The escalation bullets, READ OUT OF THE BLOCK rather than re-typed here.
    ///
    /// The first version of this was a hard-coded literal list, which is the
    /// trap §12.11 names: a test that asserts the non-duplication of a block
    /// against its own private copy of the block's words has two copies to keep
    /// in step, in the helper whose subject is not keeping two copies in step.
    /// Reword a bullet and the literal list silently stops guarding it.
    /// </summary>
    public static string[] EscalationLines() =>
        [.. Regex.Matches(ReaderText(EscalationBlock), @"^- (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())];

    /// <summary>
    /// The escalation tiers, checked against the sibling pages they were built
    /// from. WI-563 factored this out of three byte-identical copies in
    /// GliomaPageTests, HighGradeGliomaPageTests and AstrocytomaPageTests —
    /// which were themselves the same drift the block fixes, one layer up: the
    /// copies had already diverged, and each was missing a check another had.
    ///
    /// Runs on the COMPOSED page, because the tiers now live in a block and an
    /// assertion against the raw page would be asserting against the literal
    /// string "[ESCALATION]" (§12.10, WI-514's trap).
    ///
    /// SECTION-SCOPED, and that is not cosmetic. The first version of this
    /// helper flattened the WHOLE page, which quietly dropped the scoping the
    /// three copies it replaced all had — so moving [ESCALATION] out of the
    /// symptoms section and down into "Where to get support" would have kept
    /// the suite green. That is WI-512's "presence was never the property,
    /// position was", re-committed by the factoring that cites it.
    ///
    /// Every cross-page claim READS the sibling rather than hard-coding what it
    /// is believed to say (§12.10), and checks the sibling's TIER rather than
    /// merely that it says the word.
    /// </summary>
    public static void AssertEscalationTiers(string page, string slug, string heading)
    {
        var section = Flatten(ReaderText(ComposedSection(page, heading)));

        // The list header ends with a period now, not a colon: it carries the
        // 911 instruction on its own line so a US reader gets the action and
        // not only the British-sounding noun (/seizures/what-to-do does the
        // same). `[.:]` so a revert to either shape is still matched.
        var ambulance = Regex.Match(section,
            @"Call an ambulance for any of these[.:](.*?)(?=A seizure like|Call your team)",
            RegexOptions.Singleline);
        Assert.True(ambulance.Success, $"{slug} has no ambulance list");

        var sameDay = Regex.Match(section,
            @"Call your team the same day for any of these:(.*?)(?=If you are having|Same day means|$)",
            RegexOptions.Singleline);
        Assert.True(sameDay.Success, $"{slug} has no same-day list");

        var here = ambulance.Groups[1].Value;
        var later = sameDay.Groups[1].Value;

        // /seizures/what-to-do is the site's authority on seizure escalation.
        // Strip markdown emphasis first, or `**first ever** seizure` reads as
        // absent (§12.10).
        var seizurePage = Regex.Replace(
            Flatten(ReaderText(Read("seizures", "what-to-do.md"))), @"[*_]", "");
        Assert.True(
            Regex.IsMatch(seizurePage, @"first[- ](ever[- ])?seizure", RegexOptions.IgnoreCase),
            "/seizures/what-to-do no longer singles out a first seizure — the escalation block's "
            + "ambulance tier was built on that and needs rechecking");
        Assert.Matches(new Regex(@"first ever seizure", RegexOptions.IgnoreCase), here);

        // WI-512's blocker: the same symptom sorted into different tiers on
        // different pages.
        //
        // The capture is anchored on craniotomy's REAL heading. The first
        // version terminated on "[Cc]all your team", which that page does not
        // contain anywhere — so the capture ran from the ambulance heading to
        // the end of the file, swallowing the questions list and two later
        // sections. It passed only because the words happened not to appear
        // down there; the first time somebody wrote "confusion" into that
        // page's questions list, four hub tests would have failed claiming
        // craniotomy had escalated it.
        var craniotomy = Flatten(ReaderText(Read("treatments", "craniotomy.md")));
        var craniotomyAmbulance = Regex.Match(craniotomy,
            @"When to call an ambulance\.\*\*(.*?)(?=\*\*[A-Z]|\z)", RegexOptions.Singleline);
        Assert.True(craniotomyAmbulance.Success,
            "/treatments/craniotomy no longer has a 'When to call an ambulance' list, so the "
            + "tier the escalation block was checked against cannot be found");

        // Concept sets, not single words. Craniotomy's ambulance tier says
        // "cannot ... see", never "vision" — so a literal Contains("vision")
        // check reported agreement while the two pages were in fact drawing
        // the line in different places. Lexical checks across pages written by
        // different hands prove nothing.
        var sameDayConcepts = new[]
        {
            (Name: "confusion", Words: new[] { "confusion", "confused" }),
            (Name: "vision", Words: new[] { "vision", "sight" }),
        };

        foreach (var (name, words) in sameDayConcepts)
        {
            Assert.True(
                words.Any(w => craniotomy.Contains(w, StringComparison.OrdinalIgnoreCase)),
                $"/treatments/craniotomy no longer mentions '{name}', so the tier the "
                + "escalation block copied from it cannot be checked");

            // The block files a NEW, partial deficit as same-day and a SUDDEN,
            // complete one as an ambulance; craniotomy draws the same line. So
            // the disagreement to catch is craniotomy filing the gradual form
            // as an ambulance, which is what these words name.
            Assert.False(
                words.Any(w => craniotomyAmbulance.Groups[1].Value.Contains(
                    w, StringComparison.OrdinalIgnoreCase)),
                $"/treatments/craniotomy now escalates '{name}' to an ambulance while "
                + $"{slug} files it as same-day — the two pages disagree");

            Assert.Matches(new Regex(name, RegexOptions.IgnoreCase), later);
            Assert.DoesNotMatch(new Regex(name, RegexOptions.IgnoreCase), here);
        }

        // One anchored regex, not an alternation: "breathing|choking" is
        // satisfied by either word alone, and the bullet is one item naming
        // both (WI-515). Two of the three copies this replaced used the weak
        // alternation; only one used the anchored form.
        Assert.Matches(
            new Regex(@"[Tt]rouble breathing, or someone who seems to be choking",
                RegexOptions.IgnoreCase),
            here);
        Assert.Matches(new Regex(@"cannot be woken", RegexOptions.IgnoreCase), here);

        // The sudden-and-complete deficit is an ambulance, and must not have
        // been dropped back down into the same-day tier (§12.10, the
        // under-triage direction).
        Assert.Matches(
            new Regex(@"[Ss]uddenly not being able to speak, move one side, or see",
                RegexOptions.IgnoreCase),
            here);

        // The converse. Without it the block teaches a reader who has seizures
        // every month to call an ambulance every month — which is what
        // /tumors/low-grade-glioma's compressed slice said and the block's
        // first draft did not.
        Assert.Matches(
            new Regex(@"is not an ambulance call", RegexOptions.IgnoreCase), section);

        Assert.Matches(new Regex(@"after[- ]hours", RegexOptions.IgnoreCase), section);

        // §12.11: a hub that routes readers into a treatment owes that
        // treatment's safety rule. WI-563 moved this INTO the block after
        // finding /tumors/glioma and /tumors/low-grade-glioma both offering
        // chemotherapy with no fever rule anywhere on the page — the exact
        // defect WI-515 was blocked on, live on two pages.
        //
        // The sibling is read AT THE SECTION THE BLOCK DEEP-LINKS, not at the
        // page. The first version matched the chemotherapy page's INTRO line,
        // so gutting the whole {#fever-rule} section would have left this green
        // while the block's link landed the reader on an empty heading.
        var feverRule = Flatten(ReaderText(
            Section(Read("treatments", "chemotherapy.md"), "Your blood counts, and the fever rule")));
        Assert.True(
            Regex.IsMatch(feverRule, @"straight away, at any hour", RegexOptions.IgnoreCase),
            "/treatments/chemotherapy's fever-rule section no longer says a fever means calling "
            + "straight away at any hour, so the escalation block's fever line has drifted from "
            + "the page that owns the rule");
        Assert.Matches(new Regex(@"fever is its own rule", RegexOptions.IgnoreCase), section);
        Assert.Contains("/treatments/chemotherapy#fever-rule", section, StringComparison.Ordinal);

        // The surgery half of the same rule. Every hub that routes into
        // /treatments/craniotomy owes it, and a post-op fever is not
        // chemo-conditional.
        Assert.Matches(
            new Regex(@"after brain surgery, a fever is its own rule too", RegexOptions.IgnoreCase),
            section);

        // The fever lines are call-right-away, and must not have been folded
        // into either tier list — a fever in the same-day bullets is the
        // over-reassuring direction (§12.12), and one in the ambulance bullets
        // sends people to an emergency room for something the team wants to
        // triage by phone.
        Assert.DoesNotMatch(new Regex(@"fever", RegexOptions.IgnoreCase), later);
        Assert.DoesNotMatch(new Regex(@"fever", RegexOptions.IgnoreCase), here);
    }

    /// <summary>
    /// This page has NOT grown an escalation list.
    ///
    /// For the pages whose reader has nothing to escalate — a plain head CT, an
    /// extra MRI sequence — the absence is the right answer, and it needs a
    /// SHAPE check rather than a phrase check (§12.8, WI-520): /review beat a
    /// four-heading ban by writing a fifth, and beat a bold-lead-in regex three
    /// ways. Inverted, it cannot be walked around, because the bullets are what
    /// make it a list (§12.8, WI-521 and WI-526).
    ///
    /// Two things this got wrong on its first outing, both found by review:
    ///
    ///   * <c>same day|today|tonight</c> were gated behind <c>call|ring|phone|
    ///     dial</c>, so "Get seen the same day if any of these happen:" — an
    ///     entire same-day tier — walked straight through. Under-triage is the
    ///     more dangerous direction (§12.8, WI-511), so the timing words are
    ///     their own top-level branch now.
    ///   * A LIST ITEM cannot be a lead-in. With the timing words ungated, a
    ///     questions-list bullet reading "Why a CT today…" followed by nine more
    ///     bullets reads as an urgent tier. The lead-in is the preceding
    ///     PARAGRAPH (§12.8, WI-521), so marker lines are skipped.
    /// </summary>
    public static void AssertNoEscalationList(string page, string slug)
    {
        // `ReaderText(page)`, NOT `ReaderText(Body(page))`. `ReaderText` strips
        // the front matter itself, and fed a body with no `\n---` in it,
        // `IndexOf` returns -1 and the helper returns `body[3..]` — three
        // characters off the front, silently. §12.8 (WI-520) records this, and
        // records it appearing again thirty lines from the comment warning
        // about it.
        var lines = Regex.Split(ReaderText(page), @"\r?\n");

        var urgency = new Regex(
            @"(?i)\b(?:call|ring|phone|dial|contact|tell|let)\b[^.]{0,40}"
            + @"\b(?:911|ambulance|straight away|right away|immediately|at once)\b"
            + @"|(?i)\b(?:same day|today|tonight|straight away|right away|immediately|"
            + @"at once|without waiting|as soon as)\b"
            + @"|(?i)\b(?:emergency|urgent(?:ly)?|cannot wait|can't wait|do not wait|don't wait)\b"
            + @"|(?i)\bgo\s+to\s+(?:the\s+)?(?:emergency|er\b|a&e)"
            + @"|(?i)\bget\s+seen\b"
            // WI-531. /review planted three complete escalation tiers on
            // /treatments/proton-therapy and all three walked through, because
            // a lead-in does not need an urgency WORD to be a lead-in — the
            // structure alone is enough. "Ring the number on your appointment
            // letter if any of these happen:", "Get in touch with your
            // radiation team if any of these happen:" and "Your team needs to
            // hear about any of these before your next visit:" are each a
            // complete tier with no timing word and no urgency vocabulary.
            // The shape is a CONTACT verb plus a conditional hand-off into a
            // list, and it has no innocent use directly above a run of symptom
            // bullets.
            + @"|(?i)\b(?:call|ring|phone|dial|contact|tell|let|get in touch|"
            + @"reach out|speak to|let .{0,20}know)\b[^.]{0,80}"
            + @"\b(?:if any of these|if you (?:get|have|notice|develop) any of these|"
            + @"about any of these|any of the following|for any of these)\b"
            + @"|(?i)\b(?:needs? to hear|should hear|wants? to know|needs? to know)\b"
            + @"[^.]{0,60}\b(?:any of these|any of the following)\b");

        var marker = new Regex(@"^\s*(?:[-*]|\d+\.)\s");

        // A WRAPPED BULLET'S CONTINUATION IS NOT A LEAD-IN EITHER, and that asymmetry was
        // a live hole (WI-543). The counting loop below already allows `^\s{2,}\S`
        // continuations — WI-522's lesson that a finder needing consecutive marker lines
        // finds nothing when every bullet wraps — but the SELECTION loop here did not. So a
        // question list whose wrapped second line happened to contain "emergency" was read
        // as an urgency lead-in introducing the ten bullets beneath it, and a page was
        // flagged for a list it had not grown. Never a genuine lead-in; always a false one.
        var continuation = new Regex(@"^\s{2,}\S");

        // A CONTINUATION IS A LINE GLUED TO A LIST ITEM, NOT MERELY AN INDENTED ONE, and
        // /review round 4 proved the difference is exploitable rather than academic. The
        // first version of this skip tested indentation alone, and CommonMark strips 1-3
        // leading spaces — so "  **Call your team the same day for any of these:**" above
        // two symptom bullets RENDERS IDENTICALLY to a reader and was silently missed. A
        // complete tier list could hide behind two spaces, on all six calling pages, in the
        // under-triage direction. Walking back to a marker line restores the property and
        // still suppresses the wrapped-question-bullet false positive the skip exists for.
        static bool GluedToAMarker(Regex markerRe, Regex contRe, string[] all, int index)
        {
            if (index <= 0 || !contRe.IsMatch(all[index]))
            {
                return false;
            }

            // A CONTINUATION THAT ENDS IN A COLON IS A LEAD-IN, NOT GLUE. /review round 5
            // found the residue of round 4's fix: an indented lead-in glued to a BULLET was
            // still skipped, so this shape went silent —
            //     - An ordinary question bullet?
            //       **Call your team the same day for any of these:**
            //     - A fever.
            //     - New weakness.
            // Weaker than round 4's walk-around, because CommonMark renders that line
            // inside the bullet rather than as a standalone lead-in — but the docstring
            // above claims this form "cannot be walked around", and until this it could.
            if (all[index].TrimEnd().TrimEnd('*').EndsWith(":", StringComparison.Ordinal))
            {
                return false;
            }

            var previous = all[index - 1];
            return previous.Trim().Length > 0
                && (markerRe.IsMatch(previous) || GluedToAMarker(markerRe, contRe, all, index - 1));
        }

        // COUNTED IN ONE PLACE, so the canary cannot drift from the guard. /review round 5:
        // the canary shared the SELECTION rule but still re-implemented the COUNTING one as
        // `Skip(i + 1).Count(...)`, which counts every marker to the end of the input while
        // this counts only the contiguous run and stops at the first non-blank,
        // non-indented line. Harmless while every planted shape is contiguous — and exactly
        // the divergence round 4 found one layer up.
        //
        // Continuations are allowed inside the run (§12.8, WI-522: a finder needing two
        // consecutive marker lines finds nothing when every bullet wraps).
        static bool IntroducesAList(string[] all, int index)
        {
            var markerRe = new Regex(@"^\s*(?:[-*]|\d+\.)\s");
            var bullets = 0;

            for (var j = index + 1; j < all.Length; j++)
            {
                if (markerRe.IsMatch(all[j]))
                {
                    bullets++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(all[j]) || Regex.IsMatch(all[j], @"^\s{2,}\S"))
                {
                    continue;
                }

                break;
            }

            return bullets >= 2;
        }

        var offenders = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
            // A bullet is not a lead-in, and neither is the rest of a wrapped one.
            if (marker.IsMatch(lines[i]) || GluedToAMarker(marker, continuation, lines, i)
                || !urgency.IsMatch(lines[i]))
            {
                continue;
            }

            if (IntroducesAList(lines, i))
            {
                offenders.Add(lines[i].Trim());
            }
        }

        Assert.True(offenders.Count == 0,
            $"{slug} has grown an escalation list. It must not: every symptom such a list would "
            + "carry is already tiered elsewhere in the corpus, and a second copy is a second copy "
            + "to keep in step. These lead-ins introduce symptom bullets:\n  "
            + string.Join("\n  ", offenders));

        // The canary, run over a synthetic page rather than the real one, so the
        // shape check is proved able to fire before its pass means anything
        // (§12.8, WI-523). All four shapes review used to beat the first
        // version are here.
        foreach (var planted in new[]
                 {
                     "**Call your team the same day for any of these:**\n\n- A fever.\n- New weakness.",
                     "### When to go to the emergency room\n\n- A seizure.\n- A sudden headache.",
                     "Get seen the same day if any of these happen:\n\n- A new headache.\n- New weakness.",
                     "Your team needs to know today if any of these happen:\n\n- A rash.\n- A fever.",
                     // WI-531: the three /review planted on
                     // /treatments/proton-therapy, each of which walked through
                     // the version above. None of them contains a timing word
                     // or an urgency word.
                     "Ring the number on your appointment letter if any of these happen:"
                     + "\n\n- A fever.\n- New weakness.",
                     "Get in touch with your radiation team if any of these happen:"
                     + "\n\n- A fever.\n- New weakness.",
                     "Your team needs to hear about any of these before your next visit:"
                     + "\n\n- A fever.\n- New weakness.",

                     // INDENTED LEAD-INS, added at /review round 4 because the skip that
                     // fixed the wrapped-bullet false positive had made exactly these
                     // invisible. Both render as ordinary lead-ins (CommonMark strips 1-3
                     // leading spaces), so a page could have hidden a complete tier behind
                     // two of them. Either one goes red against the indentation-only skip.
                     "  **Call your team the same day for any of these:**"
                     + "\n\n- A fever.\n- New weakness.",
                     "  Go to the emergency room if any of these happen:"
                     + "\n\n- New weakness.\n- New numbness.",

                     // THE ROUND-5 WALK-AROUND: an indented lead-in glued to a bullet,
                     // which the round-4 narrowing still skipped.
                     "- An ordinary question bullet?"
                     + "\n  **Call your team the same day for any of these:**"
                     + "\n\n- A fever.\n- New weakness.",
                 })
        {
            var plantedLines = Regex.Split(planted, @"\r?\n");
            var fired = false;
            for (var i = 0; i < plantedLines.Length && !fired; i++)
            {
                // THE SAME SELECTION RULE AS THE REAL LOOP, including the glue check.
                // /review round 4: this canary re-implemented selection and had already
                // drifted — it omitted the continuation skip entirely, so it could not
                // have caught the hole that skip opened.
                if (marker.IsMatch(plantedLines[i])
                    || GluedToAMarker(marker, continuation, plantedLines, i)
                    || !urgency.IsMatch(plantedLines[i]))
                {
                    continue;
                }

                fired = IntroducesAList(plantedLines, i);
            }

            Assert.True(fired, $"the escalation-shape guard cannot see this list:\n{planted}");
        }

        // AND THE NEGATIVE CANARY, because every shape above proves the guard can FIRE and
        // none proved it can stay quiet. A question list whose wrapped continuation carries
        // an urgency word is the false positive this helper produced on
        // /tumors/spinal-cord-tumor, and it is the shape the `continuation` skip exists for.
        foreach (var innocent in new[]
                 {
                     "- Which symptoms mean calling you today, which mean calling right away, and\n"
                     + "  which mean the emergency room?\n- Who do I call after hours?\n"
                     + "- How long will the scans go on?",
                 })
        {
            var innocentLines = Regex.Split(innocent, @"\r?\n");
            for (var i = 0; i < innocentLines.Length; i++)
            {
                if (marker.IsMatch(innocentLines[i])
                    || GluedToAMarker(marker, continuation, innocentLines, i)
                    || !urgency.IsMatch(innocentLines[i]))
                {
                    continue;
                }

                Assert.Fail("the escalation-shape guard reads a wrapped question bullet as a "
                    + $"tier lead-in:\n{innocentLines[i]}");
            }
        }
    }

    /// <summary>
    /// Eight-word shingles over a page's reader text, for the corpus-wide
    /// restatement check (§12.8, WI-521: a guard that checks the siblings you
    /// thought of is a guard that finds nothing).
    ///
    /// Four things are stripped first, each because leaving it in reports the
    /// corpus colliding with itself rather than a page restating another:
    /// HEADINGS, because slot 9's is identical on all 29 library pages by
    /// prescription; WHOLE MARKDOWN LINKS, label included, because a link label
    /// is a page TITLE and the same door appears on a dozen pages; the
    /// "Where to go next" section, which is an index by design; and the
    /// "What to ask your team" section, which §12.2 item 7 requires on every
    /// page and which necessarily repeats the question every scan page owes.
    ///
    /// A window needs three CONTENT words before it counts, because eight
    /// function words in a row collide by chance.
    ///
    /// Written page-locally by WI-530's CT page and promoted the same day,
    /// because the same item's second page needed it — §12.8's own threshold is
    /// the second use, not the fifth.
    /// </summary>
    public static HashSet<string> Shingles(string page)
    {
        var text = ReaderText(page);

        text = Regex.Replace(
            text, @"(?ms)^## Where to go next.*?(?=^## |\z)", " ", RegexOptions.Multiline);
        text = Regex.Replace(
            text, @"(?ms)^## What to ask your team.*?(?=^## |\z)", " ", RegexOptions.Multiline);

        text = Regex.Replace(text, @"(?m)^#{1,6} .*$", " ");
        text = Regex.Replace(text, @"\[[^\]]*\]\([^)]*\)", " ");
        text = Regex.Replace(text, @"[^A-Za-z]", " ").ToLowerInvariant();

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var set = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i + 8 <= words.Length; i++)
        {
            var window = words[i..(i + 8)];
            if (window.Count(w => !ShingleFunctionWords.Contains(w)) < 3)
            {
                continue;
            }

            set.Add(string.Join(' ', window));
        }

        return set;
    }

    private static readonly HashSet<string> ShingleFunctionWords = new(StringComparer.Ordinal)
    {
        "the", "a", "an", "and", "or", "but", "is", "are", "was", "were", "be", "been",
        "it", "its", "that", "this", "those", "these", "of", "to", "in", "on", "at",
        "for", "with", "as", "by", "from", "you", "your", "they", "them", "their",
        "not", "no", "so", "if", "what", "which", "who", "how", "when", "there",
        "can", "cannot", "will", "would", "may", "might", "do", "does", "did", "have",
        "has", "had", "one", "than", "then", "out", "up", "about", "into", "over",
    };

    /// <summary>
    /// No paragraph names a warning sign AND reassures about it without either a tier
    /// or the other cause, in that paragraph or the next.
    ///
    /// PARAGRAPH-scoped, and run on the COMPOSED page. WI-536 wrote this page-locally
    /// for <c>/tumors/ependymoma</c> after a sentence-window version never looked at
    /// the paragraph it was written for; WI-537 is its second use, which is §12.8's
    /// threshold, and the copy is what this replaces. The docs are explicit that
    /// **when a guard is copied, its holes are copied too** — both holes below were
    /// invisible while the guard ran on a raw page and became live the moment it ran
    /// on a composed one.
    ///
    /// <paramref name="mustHaveChecked"/> is the positive half, and it is not
    /// optional: WI-517's lesson is that an iterate-and-check guard is green on a page
    /// it never looked at, so each caller names the reassuring paragraphs this must
    /// have actually examined.
    /// </summary>
    public static void AssertNoWarningSignIsNormalised(
        string composedPage, string slug, params string[] mustHaveChecked)
    {
        var paragraphs = Paragraphs(composedPage);

        var normaliser = new Regex(
            // `(?<!than )usual`: the escalation block's own red flag is "a headache much
            // worse THAN USUAL", where the word is part of the warning rather than a
            // reassurance about it. Unqualified, this fired on the ambulance list.
            @"\b(normal|expected|nothing to worry|common|(?<!than )usual|side effects?"
            + @"|part of (the )?treatment|(can )?comes? from (the )?(treatment|surgery)"
            + @"|usually (comes?|is) from|go on for a while|get better)\b",
            RegexOptions.IgnoreCase);

        // WI-536 /review round 3: `behavio\w*` was missing, so the guard could not fire
        // on a behavior change, which was the one posterior fossa sign its tier stranded.
        var symptom = new Regex(
            @"\b(headaches?|vomit\w*|throwing up|sleep\w*|tired\w*|drows\w*|weak\w*|numb\w*"
            + @"|swallow\w*|talk\w*|speak\w*|speech|walk\w*|balance|mood|behavio\w*"
            + @"|irritab\w*|dizz\w*)\b",
            RegexOptions.IgnoreCase);

        // WI-543 ADDED THE EMERGENCY-ROOM TIER, AND IT WAS A REAL HOLE. This set knew
        // ambulance, right-away and same-day but NOT the strongest instruction the corpus
        // gives — so a paragraph telling the reader to go to the emergency room counted as
        // having no tier at all, and the guard demanded a WEAKER phrase before it was
        // satisfied. Counted rather than assumed before widening it (§12.8): that wording
        // is used as a tier in at least eight files, including `blocks/escalation.md`
        // itself ("go to the emergency department"), /tumors/pituitary-tumor,
        // /tumors/craniopharyngioma, /treatments/craniotomy, /treatments/shunts,
        // /tumors/meningioma, /tumors/brain-metastases and /start.
        //
        // This widens what counts as ANSWERED, which is the permissive direction — so it
        // is justified only because every added phrase is a stronger instruction than the
        // ones already here, never a weaker one. WI-543 declined to add "seen quickly" for
        // exactly that reason.
        // NEGATED TIERS DO NOT COUNT AS ANSWERS. The match was mention-based, so
        // "this is not an ambulance call" or "rather than going to the emergency room"
        // satisfied a SAFETY guard by DENYING the tier. That hole predates WI-543 --
        // blocks/escalation.md ships "is not an ambulance call" on all 19 hubs -- but
        // adding the emergency-room tier made it live on a page that calls this helper,
        // so it is closed here rather than left for the next item to meet.
        var answered = new Regex(
            @"(?<!\b(?:not|rather than|instead of|no need)\b[^.]{0,40})"
            + @"(?:also come from the tumor|same-day call|call your team the same day"
            + @"|right away|right-away|ambulance|emergency room|emergency department)",
            RegexOptions.IgnoreCase);

        // PROVED ABLE TO TELL THEM APART, because a tier guard that cannot see a negation
        // is the failure this whole helper exists to prevent (§12.8, WI-523's canary rule).
        Assert.Matches(answered, "Weakness that keeps getting worse means the emergency room now.");
        Assert.Matches(answered, "That is a same-day call.");
        Assert.DoesNotMatch(answered,
            "The organizations ask you to be seen rather than to go to the emergency room.");
        Assert.DoesNotMatch(answered, "A seizure that stops on its own is not an ambulance call.");

        var examined = new List<string>();
        for (var i = 0; i < paragraphs.Count; i++)
        {
            if (!symptom.IsMatch(paragraphs[i]) || !normaliser.IsMatch(paragraphs[i]))
            {
                continue;
            }

            examined.Add(paragraphs[i]);

            // A LIST ITEM's tier is in its lead-in, not in itself. This is the same
            // structural point AssertNoEscalationList already records from the other
            // direction ("the lead-in is the preceding PARAGRAPH"): composed, a hub
            // inherits the escalation block's bullets, and each one reads as an
            // untiered warning unless the sentence introducing the list is in view.
            var leadIn = "";
            if (paragraphs[i].StartsWith("- ", StringComparison.Ordinal))
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    if (!paragraphs[j].StartsWith("- ", StringComparison.Ordinal))
                    {
                        leadIn = paragraphs[j];
                        break;
                    }
                }
            }

            var next = i + 1 < paragraphs.Count ? paragraphs[i + 1] : "";
            var context = $"{leadIn} {paragraphs[i]} {next}";

            Assert.True(answered.IsMatch(context),
                $"{slug}: a paragraph names a warning sign and reassures, with no tier: "
                + $"\"{paragraphs[i]}\"");
        }

        foreach (var expected in mustHaveChecked)
        {
            Assert.Contains(examined, p => p.Contains(expected, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Reader-text paragraphs: blank-line bounded, heading lines removed so a heading
    /// never merges into a sentence (WI-535), and each list item its own paragraph.
    /// </summary>
    public static List<string> Paragraphs(string page) =>
        [.. Regex.Split(
                Regex.Replace(ReaderText(page).Replace("\r\n", "\n"),
                    @"^[ \t]*#{1,6}[ \t].*$", "", RegexOptions.Multiline),
                @"\n[ \t]*\n|\n(?=[ \t]*- )")
            .Select(p => Flatten(p).Trim())
            .Where(p => p.Length > 0)];

    /// <summary>
    /// This page states nothing that already lives somewhere else in the corpus,
    /// except the short, named list of sentences that are shared ON PURPOSE.
    ///
    /// <paramref name="deliberatelyShared"/> entries must be LONGER than a
    /// shingle: the comparison asks whether the allowed sentence contains the
    /// eight-word run, and WI-521's first implementation asked it the other way
    /// round and did nothing at all.
    /// </summary>
    public static void AssertDoesNotRestateTheCorpus(
        string page, string slug, params string[] deliberatelyShared)
    {
        var mine = Shingles(page);
        var offenders = new List<string>();

        foreach (var (otherSlug, text) in AllPages())
        {
            if (otherSlug.Equals("pages/" + slug, StringComparison.Ordinal))
            {
                continue;
            }

            var shared = mine.Intersect(Shingles(text), StringComparer.Ordinal)
                .Where(s => !deliberatelyShared.Any(a => a.Contains(s, StringComparison.Ordinal)))
                .Take(4)
                .ToList();

            if (shared.Count > 0)
            {
                offenders.Add($"{otherSlug}: {string.Join(" | ", shared)}");
            }
        }

        Assert.True(offenders.Count == 0,
            $"{slug} restates prose that already lives somewhere else in the corpus:\n  "
            + string.Join("\n  ", offenders));
    }

    // ------------------------------------------------------------ §12.18/§12.19
    // THE ANTI-RANKING GUARD, PROMOTED. WI-569 built it over ten review rounds
    // inside MeningiomaPageTests; §12.18 closes by saying WI-570 is where it gets
    // promoted, "because that item needs it on nine hubs at once". It is here
    // rather than copied twenty-three times for the reason §12.8's
    // factor-at-the-second-use rule exists — and for a second one §12.18 states
    // outright: when two guards test one property in two files, the newer one is
    // not automatically the stronger one, and a lexicon that only ever grows in
    // one file protects one page.

    /// <summary>
    /// The places a reader could be told their own tumor is. Plain words AND the
    /// report's words, because this corpus writes plain first — a lexicon holding
    /// only <c>sphenoid</c> is blind to the half of each entry a reader meets.
    ///
    /// <para>WI-570 widened it once, measured: the corpus is written in the second
    /// person and says <b>"the base of your skull"</b> where WI-569's lexicon said
    /// <c>base of the skull</c>. That one word hid
    /// <c>/tumors/chordoma</c>'s entire skull-base subsection from the scan. The
    /// widening cost <b>zero false positives</b> anywhere in the corpus, measured
    /// over all twenty-three tumor hubs plus <c>/where-your-tumor-is</c> and
    /// <c>/treatments/craniotomy</c> before it was taken — it did find the chordoma
    /// subsection, which is the point of it, and that is now a recorded
    /// <see cref="Kept"/> rather than a silence. It is round 4's
    /// report-words-versus-plain-words finding in a third costume, and the next one
    /// will be a fourth: <b>the lexicon is a floor, not a fence.</b></para>
    /// </summary>
    public const string LocationAddress =
        @"skull base|convexity|parasagittal|falx|sphenoid|olfactory groove|tuberculum"
        + @"|suprasellar|posterior fossa|petroclival|intraventricular|\bfloor\b"
        + @"|on the surface|spinal cord|fluid spaces"
        + @"|deep in the middle|midline|behind the eye|wing of bone"
        + @"|pituitary|brain ?stem|\bsell(?:a|ae|ar)\b|optic nerve|perioptic|\bspine\b"
        + @"|ventricle|cavernous sinus|foramen magnum|between the two halves"
        + @"|smelling nerves"
        + @"|temporal|frontal|parietal|occipital|\blobe|cerebell|tentori|\bcliv"
        + @"|\borbit|cerebellopontine|crown of the head"
        + @"|(?:top|back) of (?:the|your) head"
        + @"|optic chiasm|sagittal sinus|internal auditory canal|pineal"
        + @"|ridge behind the eyes"
        + @"|\bforamen\b|petrous|clinoid|\bplanum\b|falcine|parasellar"
        + @"|jugular|torcul"
        // THE PLAIN WORDS THIS CORPUS ACTUALLY USES FOR A COMPARTMENT (WI-570,
        // /review). Round 4 found the lexicon carried the REPORT words while the
        // pages lead with plain ones. This is that finding a fourth time, and worse,
        // because these are the words LocationObligationSweepTests PINS as FOUR of
        // the nine hubs' own address claims: "the upper part of the brain"
        // (glioblastoma, low-grade, high-grade), "the back of the brain"
        // (ependymoma). Attacks written in the pages' own register all walked
        // through — "A glioma in the upper part of the brain usually comes out
        // whole", and a bullet-lead-plus-deictic version of it.
        //
        // MEASURED OVER THE WHOLE CORPUS BEFORE BEING TAKEN, which is the only way
        // §12.18 allows a widening: `upper part of the|your brain` costs ZERO, and
        // `back of the brain` cost ONE when it was taken -- the recurrence pair on
        // /tumors/ependymoma, WHICH THIS ITEM THEN DELETED. So it costs ZERO today,
        // and what it is still load-bearing for is the `worst` qualification below,
        // which only fires because of it. §12.18: a rationale that stopped being true
        // is how one gets copied, so the rationale says which half is still live.
        // `front of the brain` costs TWO and is NOT taken — it is the residual, and
        // it is written down rather than hoped at: /tumors/meningioma's caregiver
        // line "common with tumors near the front of the brain, and easy to mistake
        // for the person choosing to be difficult" and /tumors/astrocytoma's "Ask
        // the team what to expect after surgery near the front of the brain" are
        // both correct, and that page is the one WI-569 measured at zero.
        + @"|upper part of (?:the|your) brain|back of the brain"
        // AND THE REST OF THE REGION NAMES THE CORPUS ACTUALLY TEACHES (/review
        // round 2). The first pass took the two that the pinned type claims use and
        // stopped, which left the guard blind to the ones `blocks/mechanism.md`
        // broadcasts onto every hub that composes it: **"Side of the brain, near the
        // temple"** and **"Upper back part of the brain"**, each reaching the
        // EIGHTEEN hubs that include that block. Attacks written straight off those
        // bullets walked through. Measured over all 23 hubs plus /where-your-tumor-is
        // and /treatments/craniotomy: every token below costs ZERO.
        //
        // AND THE FIRST VERSION OF THIS NOTE QUOTED THE WRONG FILE, which /review
        // round 7 caught by grepping for the quotations. It attributed "the side of
        // YOUR brain, near your EAR" and "the upper back part of YOUR brain" to the
        // block, at 20 and 19 pages. Those are `/where-your-tumor-is`'s own `### `
        // HEADINGS, and each occurs ONCE in the whole corpus. The tokens are right
        // either way, because they carry both branches — but the controls had been
        // written off the wrong quotations, so three of them exercised only the
        // `your` branch while the corpus is written in the `the` branch on eighteen
        // pages. **A quotation in a note is a claim; grep it like one.**
        + @"|side of (?:the|your) brain|upper back part of (?:the|your) brain"
        + @"|stalk the brain sits on|\bthalam|cauda equina"
        + @"|(?:top|bottom) end of the cord"
        // `lower back` went in with that batch and came out at /review round 5, and it
        // is `\bneck\b` VERBATIM one round later: 11 matches under Content/ and EIGHT
        // are procedure or symptom prose — seven name the lumbar-puncture site ("a
        // needle in the lower back", "an injection in the lower back", "the needle goes
        // into the lower back"), across five hubs, plus one "lower back pain". The only
        // real addresses are /tumors/hemangioblastoma's three "the lower back part of
        // the brain", which the bare token caught by being an accidental PREFIX of a
        // different phrase. So the PHRASE goes in, symmetrical with `upper back part
        // of (?:the|your) brain` above it. Removing the bare token moves the row count
        // by zero and deflates the address count on seven hubs, which is the point:
        // **"costs zero rows" is not the same test as "is an address."**
        //
        // AND IT HAD A POSITIVE CONTROL, written for it one round earlier, in a
        // register no page in this corpus uses ("A growth in the lower back is more
        // dangerous than one higher up"). **A control can keep a bad token alive** —
        // the ablation test reported it protected right up to the round that read it.
        // The control below was rewritten with the phrase.
        + @"|lower back part of (?:the|your) brain"
        // AND TWO OF THAT BATCH WERE TAKEN BACK OUT AT /review ROUND 3, which is the
        // part worth copying: "costs zero rows" is not the same test as "is an
        // address".
        //   `\bneck\b` has 42 matches under Content/ and FORTY-ONE of them are
        // not addresses — they are symptom or procedure prose ("a stiff neck", "pain
        // in the back or neck"), and one of them is in the shared escalation block,
        // so it was on ~20 pages. (The one real address is
        // /tumors/hemangioblastoma's "the neck is the most common part of the spine
        // for one". One in forty-two is the argument, not none in forty-two --
        // /review round 9 counted them.) It cost zero rows by luck, and it did real damage:
        // it gave /treatments/craniotomy two address matches, which FALSIFIED the
        // "zero addresses" measurement written three files away in the same round
        // that added the token. Without it that page measures 0 again.
        //   `near the surface` has ZERO matches anywhere under Content/. "Measured at
        // zero cost" is trivially true of a token that matches nothing, and a dead
        // token inflates the apparent width of the lexicon.
        //
        // AND THE DEAD-TOKEN RULE IS NOT SYMMETRIC BETWEEN THE TWO LEXICONS, which
        // /review round 4 was right to make explicit rather than leave to be applied
        // at random. THE ADDRESS lexicon's job is to RECOGNISE words the corpus
        // already uses, so a token matching nothing recognises nothing — dead weight.
        // THE DIFFICULTY lexicon's job is to FORBID words, so a token matching nothing
        // is a ban doing exactly what a ban is for, and several of its entries are
        // deliberately corpus-absent. Zero occurrences condemns an address token and
        // commends a difficulty one.
        // AND TWO REJECTED, measured, so the next item does not re-propose them.
        // `middle of (?:the|your) brain` costs one — /treatments/craniotomy's "It
        // looks exactly like the worst thing you were afraid of". `\bcord\b` costs
        // FOUR, and the fourth is a sentence WI-570 itself wrote (the corrected
        // number -- the doc was fixed a round before this comment was). The
        // interesting one is /tumors/spinal-cord-tumor's "the commonest
        // kind inside the cord, a tumor that comes back most often does so where the
        // first one was", which is keyed to a KIND at one address and ranked against
        // nothing, so it is not a row — but three allowances to reach it is the
        // wrong trade, and §12.18 is explicit that the lexicon is a floor.
        // WI-570: "the" OR "your", on WI-569's own skull and head tokens, measured.
        // (An earlier note said "four tokens and no others", which was false twice
        // over: the head pair gained `the` rather than `your`, and WI-570's own new
        // tokens carry both determiners as well. Three different counts of this one
        // widening were live in three files — /review round 13. The tokens are right
        // here in the code; a number describing them is not worth keeping.) See the
        // summary above — one word, and it was hiding a whole subsection.
        //
        // THE GREEDIER VERSION WAS WRITTEN FIRST, MEASURED, AND PUT BACK.
        // Collapsing these into `(?:base|top|front|back) of (?:the|your)
        // (?:brain|skull)` looks tidier and quietly adds THE FRONT AND BACK OF THE
        // BRAIN, which are places, not skull landmarks. Measured over the whole
        // corpus it costs TWO false positives and catches nothing:
        // /tumors/meningioma's "common with tumors near the front of the brain, and
        // easy to mistake for the person choosing to be difficult" (a symptom, in the
        // caregiver section — and that page is the one WI-569 measured at ZERO), and
        // /tumors/astrocytoma's "Ask the team what to expect after surgery near the
        // front of the brain" (a question the READER asks). An earlier version counted
        // THREE and named /tumors/ependymoma's "Headaches, often worst on waking up"
        // as the third, which is not attributable to this widening at all: `back of
        // the brain` is already its own token, and that sentence only becomes a row
        // with `worst` unqualified. The correction was written twice and lost twice to
        // an aborting script (§12.19 finding 8). §12.18 — measure the cost before
        // taking a widening, and re-measure when the lexicon moves under it.
        + @"|(?:top|front|back) of (?:the|your) skull"
        + @"|base of (?:the|your) (?:brain|skull)"
        // The sacral half of /tumors/chordoma, which is the only place in the
        // corpus where an address has no other word for itself.
        + @"|\btailbone\b|\bsacrum\b";

    /// <summary>
    /// The vocabulary that turns an address into a ROW: what an operation is like
    /// there, or what becomes of the reader who has one there. WI-569 rewrote this
    /// once per review round for ten rounds and every version was green on the page
    /// while being green on the defect; §12.18 narrates all ten.
    ///
    /// <para><b>Do not read this list as the guard.</b> §12.18's own conclusion is
    /// that the load-bearing halves are the POSITIVE CONTROLS and the pinned
    /// deletions — one hundred planted sentences across seven rounds and no round
    /// ever found none, and two of the four sentences WI-569 deleted carried no
    /// banned word at all ("the surgeon can usually only get part of it"), which is
    /// Mayfield's ranking written in plain English. Adding words is not how this
    /// gets stronger.</para>
    ///
    /// <para><b>AND THREE WORDS DELIBERATELY LEFT OUT, because an undocumented hole
    /// reads like an oversight to the next person.</b> Bare <c>risk</c>,
    /// <c>damage</c> and <c>harm</c> are not banned: <c>risk</c> alone appears seven
    /// times on <c>/tumors/meningioma</c>, five of them in the hormone section about
    /// medicines, and it is ordinary prose on every treatment page in the corpus.
    /// The QUALIFIED forms above are banned instead — and that is round 7's
    /// correction, not the original design: the first version of this note excluded
    /// the three words whole and <b>sixteen fresh attacks walked through</b> on
    /// "carries a higher risk", "the risk is greater for", "does more damage", plus
    /// a family of comparatives nothing had listed at all. The cost of what remains
    /// is two sentences the guard cannot see — "wherever the tumor is right up
    /// against something a surgeon will not risk" and "against the risk of the
    /// operation itself", both on <c>/tumors/meningioma</c>, both correct, both
    /// within four sentences of an address. <b>If the exclusion is ever lifted, both
    /// have to be re-read by hand.</b> Carried here in the promotion because the
    /// note was written next to the lexicon and would otherwise have stayed behind
    /// with the file it was written in (/review).</para>
    /// </summary>
    public const string LocationDifficulty =
        @"harder to|more difficult|difficult to (?:remove|take out|reach|get at)"
        + @"|costs? you|comes? out whole|completely remov|complication|recurr"
        + @"|comes? back|grows? back|second operation|second look|straightforward"
        + @"|higher rate|easier to|riskier|risky|more risk|dangerous|safer|safest"
        // "worse" is QUALIFIED rather than banned: /tumors/meningioma's spinal
        // entry says back pain is "typically worse at night", which is a symptom
        // and not a rank. §12.17 — a ban list that forbids the correct shape is
        // worse than no ban list.
        + @"|worse (?:outlook|outcome|place|spot|odds|chance|prospect)"
        // WI-570: "worst" is qualified for the SAME reason "worse" is, one word
        // over. Bare `\bworst\b` fires on /tumors/ependymoma's "Headaches, often
        // worst on waking up" — the timing of a symptom, not a rank. §12.18 found
        // this exact failure with the bare "risk"/"damage" exclusion and wrote down
        // that its own qualify-do-not-ban rule "had not been applied to the words
        // the exclusion note named". It had not been applied to this one either.
        // `the worst` is kept bare, so "a skull base tumor is the worst" still
        // fires; measured at zero false positives across the corpus.
        //
        // AND THE RATIONALE ABOVE IS CONDITIONAL, WHICH IS THE PART §12.18 SAYS TO
        // WRITE DOWN. Measured 2026-09-25: with WI-569's address lexicon alone, bare
        // `\bworst\b` costs NOTHING — that headache sentence only becomes a row
        // once `back of the brain` is an address, which is a widening WI-570 then
        // took twenty lines above. So the qualification is load-bearing in the
        // shipped configuration and would be dead weight without it. §12.18 put a
        // DATE on the "worse" re-measurement precisely because "a rationale that
        // stopped being true is how one gets copied"; this one has its dependency
        // named as well as its date, because /review found the first version of
        // this note asserting the unconditional form, which was false.
        + @"|worse than|worse for|worse off|do better|does better"
        + @"|worst (?:outlook|outcome|place|spot|odds|chance|prospect)|\bthe worst\b"
        + @"|better than|outlook|\boutcome|survival|prognosis|bigger operation"
        + @"|takes longer|trick|all comes out|all come out|more serious"
        + @"|less serious|rarely all"
        + @"|cur(?:e|es|ed|able)|hard to (?:remove|take out|reach|get at)"
        + @"|cannot\s+(?:\w+\s+){0,2}(?:be removed|come out|be taken out)"
        + @"|how well .{0,20}\bdo\b|\beasy\b|\bfatal\b|deadly|life-threatening"
        + @"|higher chance"
        + @"|impossible|live longer|live shorter|leaves? (?:more |anything )?behind"
        + @"|permanent damage|disabl|\bdie\b|\bdeath|mortality"
        + @"|take your (?:sight|hearing|speech)|needs more surgery|greater chance"
        + @"|(?:higher|greater|added|extra|more) risk|risk (?:is|was) (?:higher|greater)"
        + @"|more damage|lasting damage|\bkinder\b|\bgentler\b"
        + @"|smaller operation than|less of a (?:job|operation)|simpler"
        + @"|longer recovery"
        + @"|\bharder\b|\bhardest\b|\beasier\b|\btough|removal rate|resection rate"
        + @"|\bthe odds\b"
        + @"|what to expect|how things go|longer stay|asks? more of|forgiving"
        + @"|brighter|no picnic|nothing to sneeze at|drag on|\bdemands?\b"
        + @"|\bceiling\b|not an easy"
        + @"|(?:bigger|lower|better|smaller) chance|\bdeficit|\bstroke\b|bigger job"
        + @"|more than one operation|only (?:get|take|remove) part|rougher"
        + @"|sets? a limit|in one piece|full clearance|partial removal"
        // WI-570 /review round 3, AND THE FINDING IS WHICH HALF IS NOW THE
        // BOTTLENECK. After the address lexicon was widened twice, twenty-five fresh
        // attacks were written using ONLY addresses the guard already knew, varying
        // the difficulty phrasing — and twenty-four passed. That is the same 96% the
        // round before measured against the address half. **The address side is now
        // the strong side; the difficulty side is where the next twenty get in.**
        // That is the sentence to copy, and it is not the one §12.18 left.
        // Every token below measured at ZERO rows across all 23 tumor hubs plus
        // /where-your-tumor-is and /treatments/craniotomy. Almost all sit one word
        // from something already here, which is round 9's finding for the fourth
        // time: "do worse" beside "do better", "better place" beside "worse place",
        // "returns" beside "comes back", "slower recovery" beside "longer recovery".
        + @"|do(?:es)? worse|better (?:place|spot|odds|chance|prospect)"
        + @"|recovery is slower|slower recovery"
        // `returns` is QUALIFIED — and /review round 5 found this fix MISSING from the
        // file one round after it was recorded as applied. The multi-anchor script
        // carrying it aborted on a later anchor and wrote NOTHING, which is §12.17's
        // failure and §12.18's, for the third time in this one item. The abort was
        // correct; believing the round's own summary over the file was not.
        // **Re-grep for what you changed. A script's stdout is not the file.**
        //
        // The qualification itself: bare `\breturns?\b` matches seven times in reader
        // text and six are ordinary prose — "Some need time away and then return"
        // (to work), "the day your child returns" (to school), "the treatment section
        // returns to it" (a page talking about itself). Zero rows today, which is
        // exactly the conditional the `worst` note above exists to stop being
        // implicit. `poor` (below) was also qualified before a false positive arrived
        // — but unlike the note that used to sit here claimed, it is not corpus-absent:
        // it has fourteen matches.
        + @"|returns? (?:in|to the same|to the very spot)"
        + @"|less room|fewer options|more is left|more left behind"
        // `poor` is QUALIFIED rather than banned, and NOT pre-emptively -- /review
        // round 8 measured bare `\bpoor(er)?\b` at FOURTEEN matches under Content/,
        // NINE of them live correct prose in PAGE bodies -- ten counting a glossary
        // entry's "poor focus" -- ("Poor balance" twice, "a poor fit",
        // "a poor word for this", "a poor thing to hear on its own"). Earlier notes
        // here and in §12.19 said it had zero corpus matches, which was false and was
        // also the WEAKER argument: nine correct uses is the case for qualifying — "poor balance", "poor appetite" and
        // "feeding poorly" are all symptom prose this corpus writes. §12.18 had to
        // learn qualify-do-not-ban twice, on "worse" and then on the bare
        // risk/damage/harm exclusion, both times after the false positive arrived.
        // This is the same rule applied before it does.
        + @"|poor(?:er)? (?:outcome|result|results|chance|outlook|odds|prospect)"
        // AND FOUR MEASURED AT ZERO AND DELIBERATELY NOT TAKEN, because §12.18's
        // closing instruction is explicit and it is the one thing every round of the
        // previous item ignored: "Stop adding words when the positive controls and
        // the pinned deletions are in place; the next round will always find twenty
        // more." `takes more out of`, `undertaking`, `bounce back` and `best case`
        // are IDIOM rather than rank vocabulary — each would catch one attack
        // sentence and widen the surface a reviewer has to hold in their head. The
        // stop is the ruling, not the list.
        ;

    /// <summary>
    /// The back-reference gate. A row split across sentences has to carry one, and
    /// requiring it is what makes a four-sentence window safe at all: proximity
    /// cannot tell a split row from two unrelated sentences, and an anaphor can.
    ///
    /// <para>THE DEICTICS AND BARE PRONOUNS ARE THE HALF THAT MATTERS. This corpus
    /// writes address entries as bolded bullet leads, <c>SentencesOf</c> splits on
    /// sentence ends, so every lead is its own sentence and what follows refers back
    /// with "here", "there", "it" or "one" — never with "those". Six of ten attacks
    /// walked through WI-569's round-6 version on the format the pages are actually
    /// written in.</para>
    /// </summary>
    private const string LocationBackReference =
        @"\b(?:those|these|that kind|that sort|the ones|ones there"
        + @"|they|them|here|there|it|one|such)\b";

    /// <summary>
    /// Every window of four sentences that carries an address and the difficulty
    /// vocabulary together. That window is the row itself.
    /// </summary>
    /// <returns>
    /// The TRIGGER — the one sentence carrying the difficulty vocabulary — and the
    /// WINDOW it was judged in.
    ///
    /// <para><b>They are returned separately because conflating them let one
    /// allowance excuse a second, unreviewed sentence</b> (/review). An allowance is
    /// matched against the trigger; a window is up to four sentences wide, so
    /// matching against it meant an allowance written for sentence N also silenced
    /// the different trigger three sentences later whose window happened to contain
    /// it. On <c>/tumors/ependymoma</c> that was live: two written reasons were
    /// covering three rows, and the third — "It can also come back, even years
    /// later, so the scans go on for a long time" — had been reviewed by nobody. The
    /// prose was innocent. The mechanism was not.</para>
    /// </returns>
    // Compiled: this runs over every sentence of every tumor hub, once for the page
    // and once per positive control — around four hundred passes of two large
    // alternations over ~40 KB each. Interpreting them every time is the difference
    // between a fast suite and a slow one, and nothing else about them changes.
    private static readonly Regex DifficultyRegex =
        new(LocationDifficulty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AddressRegex =
        new(LocationAddress, RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex BackReferenceRegex =
        new(LocationBackReference, RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <param name="address">
    /// The address lexicon to judge with. Defaults to the real one; the ablation test
    /// passes a copy with one token removed, which is how "every token has a control"
    /// becomes a thing the suite asserts rather than a measurement somebody took once.
    /// </param>
    public static List<(string Trigger, string Window)> PlacesRankedIn(
        string text, Regex? address = null)
    {
        address ??= AddressRegex;
        var sentences = SentencesOf(text);
        var rows = new List<(string Trigger, string Window)>();

        for (var i = 0; i < sentences.Length; i++)
        {
            if (!DifficultyRegex.IsMatch(sentences[i]))
            {
                continue;
            }

            // THE WHOLE ROW IN ONE SENTENCE — the ordinary case.
            if (address.IsMatch(sentences[i]))
            {
                rows.Add((sentences[i], sentences[i]));
                continue;
            }

            if (!BackReferenceRegex.IsMatch(sentences[i]))
            {
                continue;
            }

            // FOUR, and a fixed window rather than "back to the start of the
            // bullet": the text is flattened so nothing marks where a bullet ENDS,
            // and a pointer that never resets gave every later sentence a window
            // reaching back to the last address entry. It fired nineteen times.
            var window = string.Join(" ", sentences[Math.Max(0, i - 3)..(i + 1)]);
            if (address.IsMatch(window))
            {
                rows.Add((sentences[i], window));
            }
        }

        return rows;
    }

    /// <summary>
    /// A sentence the scan finds and the page is keeping, with the reason. §12.19:
    /// <b>fixed or recorded with its reason, never a bare allowlist</b> — which is
    /// the WI-509 pattern this corpus already applies to shared prose.
    /// </summary>
    /// <param name="Sentence">
    /// A fragment of the TRIGGER sentence — the one carrying the difficulty
    /// vocabulary — long enough to be unique. Not of the window: see
    /// <see cref="PlacesRankedIn"/> for why that distinction is the whole point.
    /// </param>
    /// <param name="Why">Why it is not a row. This is the claim a reviewer checks.</param>
    public sealed record Kept(string Sentence, string Why);

    /// <summary>
    /// THE POSITIVE CONTROLS, and they are the half that does the work.
    ///
    /// <para>Each is a sentence of the banned property that an EARLIER version of
    /// WI-569's guard let through, and each is here because <b>a property guard that
    /// has never been seen to fail has not been shown to work.</b> They are run over
    /// every page this guard is asserted on, not over one — a lexicon that shrinks
    /// to fit one page's prose goes red here before a real defect has to.</para>
    ///
    /// <para>The shapes, deliberately spread: a whole row in one sentence, an
    /// outcome verb, a survival comparison, a compartment name no page uses, a
    /// bolded bullet lead followed by a deictic, and a row split across three
    /// sentences behind an anaphor.</para>
    /// </summary>
    public static readonly string[] LocationPositiveControls =
    [
        "A tumor on the skull base is harder to take out completely.",
        "A tumor on the skull base has a poor outcome.",
        "People with a convexity tumor live longer than people with a petroclival one.",
        "A tumor over the temporal lobe usually comes out whole.",
        "Tumors at the cerebellopontine angle rarely all come out.",
        "A tumor at the base of your skull cannot safely come out.",
        "An intraventricular tumor carries a much greater chance of permanent damage.",
        "Surgery on the floor of the skull leaves more behind.",
        "A clival tumor is more dangerous than one at the back of your head.",
        // Round 4's PLAIN-WORDS control, and the only one exercising `brain ?stem`.
        // It was dropped in the promotion and /review counted the list: WI-569 had
        // thirteen and twelve arrived. A control list is the one place a silent
        // off-by-one costs exactly what the list exists to prevent.
        "A tumor near the brainstem is harder to take out.",
        // The page's own FORMAT, which is the shape round 6 found and which the
        // nine above have no member of.
        "- On the floor at the front of the skull. Surgery there leaves more behind.",
        "The umbrella word is skull base. Tumors here are harder to take out completely.",
        // The split row, which is the hole rounds 3 and 4 each left open in a
        // different width.
        "On the floor of the skull, the nerves run through. Keeping those working is a "
        + "major concern. Those are harder to take out completely.",
        // WI-570's own additions, in the register the plain-compartment hubs write
        // in -- the pinned claims of glioblastoma, low-grade-glioma, high-grade-glioma
        // and ependymoma use these two phrases. (An earlier version said "five of the
        // nine", which is nobody's count: four claims use these phrases, and seven of
        // the nine write in the plain register.)
        // Every one of these PASSED before `upper part of the brain` went into the
        // address lexicon.
        "A glioma in the upper part of the brain usually comes out whole.",
        "- In the upper part of the brain. Tumors there come out whole more often.",
        // AND /review ROUND 3'S: A TOKEN WITH NO CONTROL IS NOT PROTECTED. Ablating
        // each of WI-570's eleven new address tokens one at a time turned NO control
        // red except the two above — so nine of them could have been deleted by a
        // later edit in silence, which is precisely what this list exists to stop.
        // The first two are the attacks that MOTIVATED the widening. They are written
        // in /where-your-tumor-is's HEADING form ("your"), which is where the earlier
        // draft of this comment wrongly said blocks/mechanism.md's bullets were --
        // see the note on that in LocationAddress. The block's own form ("the") is
        // covered by the three controls at the end of this list. The third here
        // covers the sacral end of /tumors/chordoma, the only place in the corpus
        // where an address has no other word for itself.
        "- On the side of your brain, near your ear. Tumors there are harder to take out completely.",
        "A tumor in the upper back part of your brain has a poor outcome.",
        // AND ONE CONTROL PER REMAINING NEW TOKEN (/review round 4). Round 3 wrote
        // the rule — "add the control in the same edit as the token, or the token is
        // decoration" — added three, and left EIGHT tokens still turning no control
        // red. Writing a rule is not applying it, and the ablation is the acceptance:
        // remove any one address token and exactly one control below must fail.
        //
        // THE CONTROL IT REPLACED IS THE REASON THE RULE IS "ONE PER TOKEN". It read
        // "Surgery at the bottom end of the cord, near the cauda equina, leaves more
        // behind" — TWO new tokens in one sentence, so under single-token ablation
        // each masked the other and it went red for neither. A control carrying two
        // of the things it is testing tests neither. (Its comment also claimed to
        // cover the sacral end of /tumors/chordoma while containing neither
        // `tailbone` nor `sacrum`.)
        "A tumor low in the back of the brain is harder to take out.",
        "A tumor on the stalk the brain sits on has a poor outcome.",
        "Tumors in the thalamus rarely all come out.",
        "Surgery near the cauda equina leaves more behind.",
        "A tumor at the bottom end of the cord cannot safely come out.",
        "A tumor in the lower back part of your brain is more dangerous than one higher up.",
        "A chordoma at the tailbone is harder to take out completely.",
        "Surgery at the sacrum leaves more behind.",
        // AND THE OTHER BRANCH OF THE THREE TWO-BRANCH TOKENS (/review round 7). The
        // three controls above them say "your"; `blocks/mechanism.md` says "the" --
        // its bullets are "Side of the brain, near the temple" and "Upper back part of
        // the brain", on the eighteen hubs that include it -- and
        // /tumors/hemangioblastoma says "the lower back part of the brain".
        // Whole-token ablation cannot tell the branches apart, so narrowing
        // `(?:the|your)` to `your` in a later tidy-up would have blinded the guard on
        // eighteen composed pages and stayed green.
        // `NarrowingTheOrYourInTheAddressLexiconTurnsAControlRed` is the assertion
        // that closes it; these are what it fires on.
        "- On the side of the brain, near the temple. Tumors there are harder to take out completely.",
        "A tumor in the upper back part of the brain has a poor outcome.",
        // (Its `your` branch matches nothing in the corpus either, and is kept for the
        // same prospective reason as `upper part of your brain` below. A dead BRANCH of
        // a live token is not a dead TOKEN -- see LocationAddress on that asymmetry.)
        "A tumor in the lower back part of the brain is more dangerous than one higher up.",
        // AND THE ONE BRANCH THAT STILL HAD NO CONTROL: `upper part of (?:the|your)
        // brain`'s `your` side. The corpus has ZERO occurrences of "upper part of your
        // brain" -- so by the dead-token rule above this branch recognises nothing
        // today. It is kept prospectively rather than trimmed, because unlike a dead
        // ADDRESS token it costs nothing and the corpus's own register does use "your"
        // for other regions; and a branch kept prospectively still owes a control, or
        // `NarrowingTheOrYourInTheAddressLexiconTurnsAControlRed` cannot be per-token.
        "A tumor in the upper part of your brain usually comes out whole.",
        // AND THE WIDEST-REACH BRANCH IN THE WHOLE LEXICON, which had no control at
        // all until /review round 11 measured the branches one at a time.
        // `base of (?:the|your) (?:brain|skull)`'s THE form is live in both halves
        // (seven "base of the brain", seven "base of the skull"), and one of them is
        // blocks/mechanism.md's "Behind the eyes, at the base of the brain" -- so it reaches all EIGHTEEN composing hubs, more than
        // any other branch here. Ablation could not see it, because the only control
        // carrying that token ("A tumor at the base of your skull cannot safely come
        // out") uses the OTHER branch. Nothing in this sentence but the token itself
        // is an address, so narrowing either way now reddens exactly one control.
        "A tumor at the base of the brain is harder to take out.",
    ];

    /// <summary>
    /// Sentences carrying NEITHER lexicon, placed between the page and the planted
    /// control so a four-sentence window cannot reach back into the page's own prose.
    ///
    /// <para><b>THIS IS THE THIRD AND LAST LAYER OF ONE FAILURE.</b> Round 2 found
    /// that asserting only "the row count went up" proves a row appeared, not that
    /// the planted one did. Round 3 required a fragment of the control in the found
    /// row — and for a multi-sentence control the fragment is inside the window <i>by
    /// adjacency</i>, whatever the lexicon matches, so four controls still "passed"
    /// on <c>/tumors/craniopharyngioma</c> with the address tokens they depend on
    /// removed: the window reached back into that page's trailing prose and found a
    /// real address there. **A control appended to a page is being scanned in that
    /// page's context, and the context is doing the work.** Isolate it, or it is the
    /// page under test that passes the control rather than the guard.</para>
    /// </summary>
    private const string ControlSpacer =
        " The rest of this note is here to keep two pieces of text apart. "
        + "It says nothing about anybody's diagnosis. "
        + "It is three sentences long and it means nothing at all. ";

    /// <summary>
    /// The fragment of each control that must appear in the row the guard finds.
    /// Keyed by index into <see cref="LocationPositiveControls"/>.
    ///
    /// <para>WITHOUT THIS THE CONTROLS PASS FOR THE WRONG REASON. Asserting only
    /// that the row COUNT went up proves a row appeared, not that the planted one
    /// did — and /review demonstrated it: with the lexicon artificially narrowed,
    /// two of the bullet-lead controls still "passed" on
    /// <c>/tumors/craniopharyngioma</c>, because the four-sentence window reached
    /// back into that page's own trailing prose and found a real address there. A
    /// control that can be satisfied by the page it is planted in is not a
    /// control.</para>
    /// </summary>
    private static readonly string[] LocationControlFragments =
    [
        "A tumor on the skull base is harder", "has a poor outcome",
        "live longer than people with a petroclival", "over the temporal lobe usually comes out",
        "cerebellopontine angle rarely all come out", "at the base of your skull cannot safely",
        "An intraventricular tumor carries", "Surgery on the floor of the skull leaves more behind",
        "A clival tumor is more dangerous", "near the brainstem is harder to take out",
        "Surgery there leaves more behind", "Tumors here are harder to take out completely",
        "Those are harder to take out completely",
        "in the upper part of the brain usually comes out whole",
        "Tumors there come out whole more often",
        "Tumors there are harder to take out completely",
        "in the upper back part of your brain has a poor outcome",
        "low in the back of the brain is harder to take out",
        "on the stalk the brain sits on has a poor outcome",
        "Tumors in the thalamus rarely all come out",
        "Surgery near the cauda equina leaves more behind",
        "at the bottom end of the cord cannot safely come out",
        "in the lower back part of your brain is more dangerous",
        "at the tailbone is harder to take out completely",
        "Surgery at the sacrum leaves more behind",
        "Tumors there are harder to take out completely",
        "in the upper back part of the brain has a poor outcome",
        "in the lower back part of the brain is more dangerous",
        "in the upper part of your brain usually comes out whole",
        "at the base of the brain is harder to take out",
    ];

    /// <summary>
    /// The ADDRESS half of each control, which is the half that has to have done the
    /// work. Same index as <see cref="LocationPositiveControls"/>.
    ///
    /// <para><b>WITHOUT THIS THE FRAGMENTS ABOVE ARE HALF A FIX.</b> For every
    /// multi-sentence control the fragment IS the trigger sentence, and a window
    /// always contains its own trigger — so the fragment proves the trigger was
    /// found and proves nothing about whether the planted ADDRESS is what made it
    /// fire. /review round 2 measured it: with the address lexicon narrowed, three
    /// of the multi-sentence controls still "passed" on
    /// <c>/tumors/craniopharyngioma</c>, because the four-sentence window reached
    /// back into that page's own prose and found a real address there. Requiring
    /// BOTH halves in the same window narrows it; <see cref="ControlSpacer"/> is what
    /// actually closes it, and the two together are the third attempt at one failure.
    /// For most single-sentence controls the two fragments are deliberately the same
    /// string — the address and the difficulty are in one sentence, so there is
    /// nothing to separate. Where they sit far enough apart to be worth naming
    /// separately, they are.</para>
    /// </summary>
    private static readonly string[] LocationControlAddressFragments =
    [
        "A tumor on the skull base is harder", "A tumor on the skull base has",
        "live longer than people with a petroclival", "over the temporal lobe usually comes out",
        "cerebellopontine angle rarely all come out", "at the base of your skull cannot safely",
        "An intraventricular tumor carries", "Surgery on the floor of the skull leaves more behind",
        "A clival tumor is more dangerous", "near the brainstem is harder to take out",
        // The four where the address sits in a DIFFERENT sentence from the trigger.
        "On the floor at the front of the skull",
        "The umbrella word is skull base",
        "On the floor of the skull, the nerves run through",
        "in the upper part of the brain usually comes out whole",
        "In the upper part of the brain",
        "On the side of your brain, near your ear",
        "in the upper back part of your brain",
        "low in the back of the brain",
        "on the stalk the brain sits on",
        "in the thalamus",
        "near the cauda equina",
        "at the bottom end of the cord",
        "in the lower back part of your brain",
        "at the tailbone",
        "at the sacrum",
        "On the side of the brain, near the temple",
        "in the upper back part of the brain",
        "in the lower back part of the brain",
        "in the upper part of your brain",
        "at the base of the brain",
    ];

    /// <summary>
    /// §12.18's ruling, as a guard that runs on more than one page: <b>an address
    /// entry may carry a SYMPTOM and must never carry a DIFFICULTY.</b> A symptom is
    /// a fact about what the reader notices and it belongs to them; a difficulty is a
    /// fact about an operation, and the moment one address carries one, every other
    /// address needs one for the list to look finished. That is the location-to-risk
    /// column the Wave 6 preamble forbids, growing one review round at a time.
    /// </summary>
    /// <param name="readerText">
    /// The COMPOSED page as a reader meets it, title and description included.
    /// Composed, because §12.18's worst near-miss was an entry stripped on the
    /// reasoning that a shared block covered it, when the block covered it under a
    /// different address. Title and description included, because the description
    /// renders as the first paragraph a reader meets and nothing else reads it
    /// (WI-575).
    /// </param>
    /// <param name="slug">Named in every failure, since this runs over many pages.</param>
    /// <param name="kept">
    /// Rows this page is keeping, with reasons. <b>Every one is asserted to still
    /// fire</b>: an exception that matches nothing is not protecting anything, it is
    /// only holding a door open (§12.18), and it is how a lexicon silently narrows.
    /// </param>
    /// <param name="minAddresses">
    /// How many address words this page must contain for the co-occurrence ban to
    /// have anything to fire on.
    ///
    /// <para><b>THE PROMOTION KEPT THE DIFFICULTY FLOOR AND DROPPED THE ADDRESS
    /// ONE</b>, which /review round 2 caught by measuring rather than reading:
    /// <c>/treatments/craniotomy</c> has 21 difficulty matches and <b>zero</b>
    /// addresses. So §12.18's much-quoted "the guard fires ZERO times on
    /// /treatments/craniotomy" is a fact about the lexicon and not about the page —
    /// no row could be found there whatever the page said. The zero is real and it
    /// is not evidence. Passing the number in makes each caller say what its page
    /// has, and craniotomy's 0 is now written down as structural instead of being
    /// read as a clean bill of health.</para>
    /// </param>
    public static void AssertNoPlaceIsRankedAgainstAnother(
        string readerText, string slug, int minAddresses, params Kept[] kept)
    {
        var addresses = AddressRegex.Matches(readerText).Count;
        Assert.True(addresses >= minAddresses,
            $"{slug}: {addresses} address words, and this call claims at least "
            + $"{minAddresses}. Either the page's location material has gone, or the "
            + "address lexicon has narrowed — and either way the co-occurrence ban below "
            + "is green over text it cannot fire on.");

        // A FLOOR, because a property guard is green over a page it never read
        // (§12.17). The lowest count measured across the twenty-three tumor hubs is
        // NINE, on /tumors/acoustic-neuroma, so the floor is eight — one under the
        // measurement rather than four under it. §12.18: measure the headroom, and
        // then measure it again against what shipped.
        var vocabulary = DifficultyRegex.Matches(readerText).Count;
        Assert.True(vocabulary >= 8,
            $"{slug}: the difficulty vocabulary matched {vocabulary} times, so this guard "
            + "ran over text it did not really read — check the page was composed and "
            + "flattened before it got here");

        var rows = PlacesRankedIn(readerText);

        // MATCHED AGAINST THE TRIGGER, NOT THE WINDOW. See PlacesRankedIn: matching
        // the window let one written reason silence a second sentence nobody read.
        var unexplained = rows
            .Where(r => !kept.Any(k => r.Trigger.Contains(k.Sentence, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(unexplained.Count == 0,
            $"{slug} keys a difficulty or an outcome to a place (§12.18 — an address entry "
            + "may carry a symptom and must never carry a difficulty). Either rewrite it so "
            + "the claim hangs off the FACTOR the source names, or add it to `kept` WITH the "
            + $"reason it is not a row:\n  "
            + string.Join("\n  ", unexplained.Select(r => r.Window)));

        foreach (var k in kept)
        {
            var matched = rows
                .Count(r => r.Trigger.Contains(k.Sentence, StringComparison.OrdinalIgnoreCase));

            Assert.True(matched > 0,
                $"{slug}: the kept row \"{k.Sentence}\" matches nothing on the page any more. "
                + "Delete it rather than leaving it: an exception that fires on nothing is a "
                + "door held open for nobody, and it hides a lexicon that has narrowed.");

            // EXACTLY ONE. A fragment that matches two triggers silences a sentence
            // its reason was not written about — which is the window-versus-trigger
            // failure one layer down, and the same failure it takes two review rounds
            // to notice, because the suite is green either way.
            Assert.True(matched == 1,
                $"{slug}: the kept row \"{k.Sentence}\" matches {matched} different "
                + "sentences, so one written reason is standing over more than one of "
                + "them. Lengthen the fragment until it identifies exactly one, and give "
                + "the others their own reasons.");
        }

        // AND THE POSITIVE CONTROLS. Run the SAME scan over the page and over the
        // page with a defect planted in it, and require the second to find THE
        // PLANTED ONE. This is the half that ended WI-569's ten-round cycle.
        // THE COUNT, because this was the one list in the item without one — and its
        // own comment narrates a control being lost in a promotion (thirteen went in,
        // twelve arrived). Three arrays asserted only to be the SAME length is a
        // check a synchronised deletion passes.
        Assert.Equal(30, LocationPositiveControls.Length);
        Assert.Equal(LocationPositiveControls.Length, LocationControlFragments.Length);
        Assert.Equal(LocationPositiveControls.Length, LocationControlAddressFragments.Length);

        for (var i = 0; i < LocationPositiveControls.Length; i++)
        {
            var planted = LocationPositiveControls[i];

            Assert.True(planted.Contains(LocationControlFragments[i], StringComparison.Ordinal)
                        && planted.Contains(LocationControlAddressFragments[i], StringComparison.Ordinal),
                $"control {i}'s fragments are not both substrings of the control itself, so "
                + "they could never identify it");

            Assert.True(ControlIsCaught(readerText, i, AddressRegex),
                $"{slug}: the guard did not catch a planted row — \"{planted}\". The lexicon "
                + "has narrowed, or the page's own prose is masking it. This is the control "
                + "that proves the guard can fail at all.");
        }
    }

    /// <summary>
    /// Does control <paramref name="i"/>, planted behind the spacer, come back as a
    /// row carrying BOTH of its fragments? Factored out so the ablation test can ask
    /// the same question with a token removed from <paramref name="address"/>.
    /// </summary>
    public static bool ControlIsCaught(string readerText, int i, Regex? address = null) =>
        PlacesRankedIn(readerText + ControlSpacer + LocationPositiveControls[i], address)
            .Any(r => r.Window.Contains(LocationControlFragments[i], StringComparison.OrdinalIgnoreCase)
                   && r.Window.Contains(LocationControlAddressFragments[i], StringComparison.OrdinalIgnoreCase));

    /// <summary>The address tokens WI-570 added, each of which owes a control.</summary>
    public static readonly string[] LocationAddressTokensAddedByWi570 =
    [
        @"|upper part of (?:the|your) brain", @"|back of the brain",
        @"|side of (?:the|your) brain", @"|upper back part of (?:the|your) brain",
        @"|stalk the brain sits on", @"|\bthalam", @"|cauda equina",
        @"|(?:top|bottom) end of the cord", @"|lower back part of (?:the|your) brain",
        @"|\btailbone\b", @"|\bsacrum\b",
    ];
}

public sealed class MriPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "mri.md");

    private static string Flat => CuratedPage.Flatten(Page);

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    private static string[] SentencesOf(string section) => CuratedPage.SentencesOf(section);

    [Fact]
    public void TheClaustrophobiaSectionNeverSendsTheReaderToAskForAnOpenScanner()
    {
        // The one place this page could do real harm. Open and upright scanners
        // are generally lower field strength and are NOT equivalent for brain
        // tumor protocol imaging, so "ask for an open MRI" is advice that can
        // cost a reader the picture their treatment is planned from. The page
        // says "ask what your centre has" and lets the centre choose, which is
        // what the source supports (RadiologyInfo: some centres have systems
        // that are "less confining or more open").
        Assert.DoesNotContain("open MRI", Flat, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("upright", Flat, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheClaustrophobiaSectionOffersSomethingAndDoesNotEndInDefeat()
    {
        // §12.6: never end a section on a frightening sentence. Scoped to the
        // section and to its LAST sentence, because "the reassurance exists
        // somewhere on the page" is not the property — a reader who stops
        // reading at the end of this section has to stop on something solid.
        var section = Section("If small spaces frighten you");
        var sentences = SentencesOf(section);

        Assert.Contains("calming medicine", section);            // ask in advance
        Assert.Contains("come into the room with you", section); // bring a person
        Assert.DoesNotContain("cannot have an MRI", section);
        Assert.DoesNotContain("unable to have", section);

        Assert.Contains("almost always possible", string.Join(" ", sentences[^3..]));
    }

    [Fact]
    public void TheDeviceSectionLeadsWithBringingTheCardRatherThanWithBeingTurnedAway()
    {
        // Under-communicated and worth a section: many newer pacemakers, ICDs,
        // stimulators, cochlear implants and pumps ARE acceptable for MRI
        // provided staff know the exact make and model. Asserting only that the
        // card advice appears would pass with it buried under three paragraphs
        // of exclusions, which is the exact page this test exists to prevent —
        // so it has to be in the FIRST sentence (§12.6, answer first).
        var section = Section("Metal, implants and your device card");

        Assert.Contains("card", SentencesOf(section)[0]);
        Assert.Contains("exact make and model", section);
    }

    [Fact]
    public void TheGadoliniumRetentionAnswerIsHonestInBothDirections()
    {
        // The question people actually type. Omitting it looks like hiding;
        // overstating it frightens someone into refusing a scan they need. The
        // source says both halves, so the page says both halves.
        var flat = Flat;

        Assert.Contains("Small amounts can stay", flat);
        Assert.Contains("no known health effects", flat);
    }

    [Fact]
    public void ThePageExplainsWhyASecondScanIsNotABadSign()
    {
        // The item's whole reason for covering the planning and post-op scans:
        // "I just had an MRI, why another one?" is a genuinely frightening
        // moment that has a mundane answer.
        var flat = Flat;

        Assert.Contains("Why am I having another scan?", flat);
        Assert.Contains("navigation scan", flat);
        Assert.Contains("is not a sign that the first was wrong", flat);
    }

    [Fact]
    public void ThePageSaysAScanAloneCannotNameTheTumor()
    {
        // ACS is explicit that imaging can suggest a type but only tissue
        // confirms it. A reader who does not know this reads the wait for
        // pathology (WI-507) as their team stalling.
        Assert.Contains("Only a piece of the tumor", Flat);
    }

    [Fact]
    public void ThePageCarriesNoRiskPercentageAndNoDoseFigure()
    {
        // §12.4 R2: procedural risk percentages are qualitative, because the
        // source spread is too wide to state one honestly. Tesla and millilitre
        // figures are the same class of number as Gy and mg — they belong in a
        // protocol, not on a patient page.
        var body = Page[(Page.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..];

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*%", body);
        Assert.DoesNotMatch(new Regex(@"\b\d+(\.\d+)?\s*(Tesla|T\b|mg|ml|mmol)", RegexOptions.IgnoreCase), body);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone. A test page has no prognosis on
        // it, so a fence here would mean something has drifted.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void ThePageEndsWithQuestionsToAskAndThenWhereToGoNext()
    {
        // Shared contract item 7, and the last two sections of the §12.8
        // library-page template. Sources and "last reviewed" render from front
        // matter, so the page does not hand-write a provenance section.
        // The .Trim() is load-bearing on Windows, not cosmetic: `.` matches
        // `\r`, so on a CRLF checkout the capture ends with one. If you copy
        // this method to another library page, keep it.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);
        Assert.Equal("The short version", headings[0]);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8. Checked here rather than trusting ContentCheck's
        // warning level: a missing `accessed` date is only a warning there, and
        // this phase's whole source discipline rests on it.
        var front = Page[..Page.IndexOf("\n---", 3, StringComparison.Ordinal)];

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The domains the page's claims actually rest on, rather than a count
        // that half the source set could vanish beneath. RadiologyInfo carries
        // the safety, dye and experience content; ACS the "only tissue names
        // it" and the pre-surgery mapping scan; NBTS the tumor board; the PMC
        // paper the post-op baseline.
        foreach (var domain in new[] { "radiologyinfo.org", "cancer.org", "braintumor.org", "ncbi.nlm.nih.gov" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }
        // Indented, so the page's own `title:` is not counted as a source's.
        // No `$` anchor: .NET's multiline `$` does not match before `\r`, and
        // this repo has core.autocrlf=true, so an anchored version passes on
        // LF and fails the moment the file round-trips through git (WI-501).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }
}

/// <summary>The page as served, and the door that leads to it.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class MriPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MriPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync("/tests/mri");

        Assert.Contains("Your MRI scan", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromWhereANewlyDiagnosedReaderStarts()
    {
        // The WI-412 orphan lesson. /tests has no index yet, and the tumor hubs
        // do not link into the library until WI-513, so /start is currently the
        // page's only door. If that link goes, the page is invisible.
        var html = await _factory.CreateClient().GetStringAsync("/start");

        Assert.Contains("/tests/mri", html);
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors named here are the ones this page owes its reader: the two
        // pathology pages, because an MRI cannot name a tumor and only tissue
        // can, and a person to talk to.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), "/tests/mri",
            "/tests/waiting-for-results", "/tests/pathology-report", "/glossary");

    [Fact]
    public async Task TheNewVocabularyFiresAsTooltipsOnTheRealPage()
    {
        // Contract item 9: new words join the glossary so the tooltip fires
        // site-wide. Asserted on the rendered page rather than on the glossary
        // directory, because a term file that nothing matches is a term nobody
        // ever sees.
        var html = await _factory.CreateClient().GetStringAsync("/tests/mri");

        // "def-<slug>" is the popover the tooltip button targets. Asserting the
        // bare word would pass on prose that never got marked up at all —
        // "radiologist" appears in the page either way.
        foreach (var slug in new[] { "tumor-board", "radiologist", "neuronavigation" })
        {
            Assert.Contains($"def-{slug}", html);
        }
    }
}
