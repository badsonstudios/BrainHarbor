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
    private static string RepoRoot()
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
