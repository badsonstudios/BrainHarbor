using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-508: T6a, the pathology report walkthrough. The third page under the
/// §12.8 library template (shared <see cref="CuratedPage"/> helpers live in
/// TestsLibraryPagesTests.cs).
///
/// The prose is reviewed, not tested. What is pinned here is the editorial
/// spine and the places where this page could leave a reader worse off than no
/// page at all: a lab result characterised as good or bad news, a code on the
/// report read as a verdict, or a grade number misread because nobody said the
/// notation changed.
/// </summary>
public sealed class PathologyReportPageContentTests
{
    private static string Page => CuratedPage.Read("tests", "pathology-report.md");

    private static string Section(string heading) => CuratedPage.Section(Page, heading);

    [Fact]
    public void NoResultOnThisPageIsCharacterisedAsGoodOrBadNews()
    {
        // THE SPINE, and the reason it needs a machine rather than good
        // intentions. Under CNS5 these tests RENAME the diagnosis; they do not
        // predict. The sources say otherwise out loud and in the reader's own
        // vocabulary: Johns Hopkins' glossary — cited by this page, and open on
        // screen while it was drafted — calls IDH mutation "a diffuse glioma
        // subset associated with a better prognosis" and 1p/19q "associated
        // with a better outcome". Borrowed characterisation is how that lands
        // here, so the check is on the words, not on the intent.
        var body = CuratedPage.Body(Page);

        foreach (var phrase in CuratedPage.Characterisations)
        {
            Assert.DoesNotContain(phrase, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePageCarriesNoPercentageAndNoPrognosisVocabulary()
    {
        // Contract item 5 and §12.4. Nothing on this page needs a percentage,
        // so any number followed by a % sign is a claim that arrived from
        // somewhere it should not have.
        var body = CuratedPage.Body(Page);

        Assert.DoesNotMatch(@"\d+(\.\d+)?\s*(%|percent)", body);

        foreach (var word in new[] { "survival", "life expectancy", "five-year", "prognosis", "how long you have" })
        {
            Assert.DoesNotContain(word, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TheRenameSectionLeadsWithTheReassuranceAndEndsOnSomethingToDo()
    {
        // §12.6: answer in the first sentence under the heading, because most
        // readers never reach the second, and never end a frightening section
        // on the fright. This is the section a reader arrives at having been
        // told two different names, so the order of the sentences IS the
        // content.
        var section = Section("Why the name may not be the one you were told");
        var sentences = CuratedPage.SentencesOf(section);

        Assert.Contains("the tumor has not changed", sentences[0]);
        Assert.Contains("ask your team", string.Join(" ", sentences[^2..]), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheRenameSectionExplainsTheMechanismRatherThanListingRetiredNames()
    {
        // The retired-name crosswalk is WI-513's, on the glioma umbrella page
        // (SYNTHESIS §4.3 — one canonical home). What belongs here is WHY a
        // name can change: gene results became part of the name in 2021. A page
        // that grew a name-by-name table would be the second copy of content
        // that has not been written yet.
        var section = Section("Why the name may not be the one you were told");

        Assert.Contains("part of the name", section);
        Assert.Contains("2021", section);

        // The half that actually holds the boundary. Asserting the mechanism is
        // present does nothing to stop a crosswalk being pasted in beside it,
        // and a second home for the retired names is how they drift apart.
        // These two words are what any crosswalk must contain.
        Assert.DoesNotContain("anaplastic", section, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("oligoastrocytoma", section, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheGradeSectionGivesTheReasonTheNotationChangedAndNotJustTheFact()
    {
        // "Grades are written 1-4 now" tells a reader holding a report that
        // says "grade III" nothing at all. WHO CNS5's own reason is the useful
        // part and it is reassuringly mundane: a II and a III are easy to
        // mistake for one another and a typo in a grade has consequences.
        var section = Section("What the grade means");

        Assert.Contains("Roman", section);
        Assert.Contains("mistake", section, StringComparison.OrdinalIgnoreCase);

        // Grading now happens WITHIN a tumor type. Without this a reader
        // compares their grade 3 against a stranger's grade 3 on a forum, which
        // is the concrete harm the change was made to prevent.
        Assert.Contains("within a tumor type", section, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NeitherNosNorNecIsPresentedAsBadNews()
    {
        // Both are emotionally significant and universally unexplained
        // (SYNTHESIS §3.2). A reader who finds three unfamiliar letters after
        // their tumor's name will assume the worst unless the page says, in the
        // section itself, that they are not a grade and not a verdict.
        var section = Section("What NOS and NEC mean");
        var sentences = CuratedPage.SentencesOf(section);

        Assert.Contains("not otherwise specified", section, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not elsewhere classified", section, StringComparison.OrdinalIgnoreCase);

        // Said up front, not buried, and NEGATED. Asserting that the word
        // "worse" appears in the opening is not the property: "NOS usually
        // means the tumor is worse than they first thought" satisfies it
        // exactly, which is the sentence this test exists to make impossible.
        // The polarity has to be in the assertion.
        //
        // IgnoreCase, and it is not decoration: the page writes "Neither is a
        // grade" with a capital N, so the case-sensitive form of this regex
        // found nothing and failed a correct page. That is the third time this
        // exact trap has bitten (WI-505's `\bgrade` vs "Grade", WI-506's
        // marker rule) — a negation is very often sentence-initial, which is
        // precisely where the capital letter is.
        var opening = string.Join(" ", sentences[..3]);

        Assert.Matches(new Regex(@"\b(?:not|neither|does not|is no)\b[^.]{0,80}\bworse\b",
            RegexOptions.IgnoreCase), opening);
        Assert.Matches(new Regex(@"\b(?:not|neither)\b[^.]{0,40}\bgrade\b",
            RegexOptions.IgnoreCase), opening);
        Assert.Contains("ask what is missing", section, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheWalkthroughEnumeratesTheReportsPartsAndSaysTheLayoutVaries()
    {
        // The page's whole reason for existing (§12.8 slot 3 as a numbered
        // list). The canary matters: without it this test passes by finding no
        // parts at all, which is exactly what a well-meaning tightening pass
        // would produce.
        var section = Section("Your report, part by part");

        // Asserted on the RAW section, because CuratedPage.Section flattens
        // whitespace and the same nine names run together as one prose
        // paragraph would satisfy every Contains below. The list shape is the
        // content here: a reader with the document in front of them is matching
        // items, not reading.
        var raw = Regex.Match(Page, @"^## Your report, part by part\s*$(.*?)(?=^## |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value;

        // Multiline, so `^` means start-of-line rather than start-of-capture.
        Assert.True(Regex.IsMatch(raw, @"^1\. ", RegexOptions.Multiline), "the walkthrough is not a numbered list");
        Assert.True(Regex.IsMatch(raw, @"^9\. ", RegexOptions.Multiline), "the walkthrough stops before the addendum");

        string[] parts =
        [
            "Clinical information", "The diagnosis", "Gross description",
            "Microscopic description", "Stains", "Gene results", "Comment", "Addendum",
        ];

        foreach (var part in parts)
        {
            Assert.Contains(part, section, StringComparison.Ordinal);
        }

        // Reports genuinely differ, and ACS says plainly that some have no
        // microscopic section. A walkthrough that reads as a fixed running
        // order sends a reader looking for something that was never there and
        // lets them conclude their report is incomplete.
        Assert.Contains("may not have all of these", section);
        Assert.Contains("not always a mistake", section);
    }

    [Fact]
    public void TheReportIsNeverPresentedAsContainingThePlanOrTheAnswerAboutTheReader()
    {
        // What the report is FOR is sourced (NCI: it provides the diagnosis and
        // helps plan treatment). What it is not for has to be said in the same
        // breath, or a reader reads the document as their verdict rather than
        // as the input to a conversation they have not had yet.
        var section = Section("What is a pathology report?");

        Assert.Contains("description of the tumor", section);
        Assert.Contains("yours and your team", section);
    }

    [Fact]
    public void TheWaitAndTheTumorBoardAreLinkedRatherThanRestated()
    {
        // WI-507 owns the wait, the layers, the addendum's meaning, second
        // opinions, the tumor board and the patient portal. Two copies of a
        // paragraph is how the MRI and pathology-wait pages drifted apart
        // before [TUMOR-BOARD] existed (§12.8). The rule here is simpler than a
        // block: this page does not carry that material at all.
        var body = CuratedPage.Body(Page);

        Assert.Contains("/tests/waiting-for-results", body);

        // The batching explanation and the second-opinion answer are the two
        // most re-writable paragraphs on WI-507. If either appears here, they
        // now exist twice and one of them will go stale.
        Assert.DoesNotContain("eight samples", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("second opinion", body, StringComparison.OrdinalIgnoreCase);

        // [TUMOR-BOARD] is a shared block. A third copy pasted in as prose is
        // the failure §12.8 was written about, so the page links instead.
        Assert.DoesNotContain("[TUMOR-BOARD]", body, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMarkerNamesAppearOnlyAsExamplesOfWhereTheySitOnTheReport()
    {
        // WI-509 owns the markers, one entry each, with the hardest line on the
        // site to hold ("describe what is measured, never characterise a
        // result"). This page needs marker names only to point at a part of the
        // document. If it starts explaining them, the two pages disagree the
        // first time either is edited.
        var body = CuratedPage.Body(Page);
        string[] markers = ["IDH", "MGMT", "EGFR", "1p/19q", "ATRX", "TERT", "CDKN2A", "Ki-67"];

        // A density cap, not a ban: naming a word that sits on the report is
        // the whole job of the walkthrough. Three is what it takes to point at
        // the gene-results line, show a name gaining words, and show why the
        // full name is what you search for. A fourth mention is an explanation
        // starting.
        foreach (var marker in markers)
        {
            var mentions = Regex.Matches(body, Regex.Escape(marker), RegexOptions.IgnoreCase).Count;
            Assert.True(mentions <= 3, $"'{marker}' appears {mentions} times — WI-509 owns the markers");
        }

        // Per-marker caps do not bound the set: naming all thirteen once each
        // passes every one of them and is unmistakably WI-509's page. Three
        // markers are what the layout needs — one to point at the gene-results
        // line, and one name shown gaining words.
        var named = markers.Count(m => body.Contains(m, StringComparison.OrdinalIgnoreCase));
        Assert.True(named <= 4, $"{named} different markers named — WI-509 owns the markers");

        // The sharpest property. A section ABOUT a marker is WI-509's page
        // beginning to grow inside this one, and neither count catches a short
        // one. `.Trim()` because `.` matches `\r` on a CRLF checkout.
        foreach (var heading in Regex.Matches(body, @"^#+ (.+)$", RegexOptions.Multiline)
                     .Select(m => m.Groups[1].Value.Trim()))
        {
            Assert.DoesNotContain(markers, m => heading.Contains(m, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void NoCuratedPageWritesAGradeInRomanNumeralsExceptToShowTheOldStyle()
    {
        // Wider than this page on purpose, and added here because this page is
        // the first that could break it. Contract item 3 is CNS5 naming and
        // grading throughout, and WI-505 already made that mechanical for the
        // GLOSSARY — but nothing checked the 50-odd curated pages, and WI-509
        // plus 24 tumor hubs are about to be written against sources that use
        // Roman numerals freely (Johns Hopkins' own sample report says
        // "(WHO grade IV)").
        //
        // IgnoreCase is the part WI-505 got wrong first time: without it
        // `\bgrade` never matches "Grade", so a sentence-initial "Grade IV" —
        // the likeliest way anyone writes it — sails through the check whose
        // entire purpose is catching that.
        var pattern = new Regex(@"\bgrade\s+(I{1,3}|IV)\b", RegexOptions.IgnoreCase);

        var offenders = new List<string>();
        var scanned = 0;

        foreach (var (slug, text) in CuratedPage.AllPages())
        {
            // BODY only. The first version scanned the whole file, front matter
            // included, and WI-513 is the first page to cite an article whose
            // REAL TITLE carries a Roman numeral — "Grade II Gliomas: Not So
            // Low Grade". Rewriting a citation to satisfy a house style would
            // be a worse defect than the one this guard prevents, and it is the
            // same lesson the British-forms gate learned at WI-511: a source
            // title is not our prose.
            foreach (var sentence in Regex.Split(CuratedPage.Body(text), @"(?<=[.!?])\s+"))
            {
                if (!sentence.Contains("grade", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                scanned++;
                if (!pattern.IsMatch(sentence))
                {
                    continue;
                }

                // The legitimate use: showing a reader holding older paperwork
                // what the old notation looked like, so that "grade III" and
                // "grade 3" stop being two diagnoses.
                //
                // The allowance was pinned to ONE page by slug until WI-513.
                // That was right while only the report page taught the old
                // style, and wrong the moment contract item 3's "where an older
                // name was retired, say so" reached a tumor hub — every one of
                // the 24 hubs owes its readers a crosswalk slice, and each
                // would have had to be added here by name. It is the sentence's
                // own content that makes the use legitimate, not its address.
                var teachingTheOldStyle = Regex.IsMatch(
                    sentence,
                    @"older report|used to be|retired|no longer|old rules|older paperwork|changed in 2021|now written",
                    RegexOptions.IgnoreCase);

                if (!teachingTheOldStyle)
                {
                    offenders.Add($"{slug}: {CuratedPage.Flatten(sentence).Trim()}");
                }
            }
        }

        // Without this the check passes on a corpus that never mentions a grade
        // at all, which is the state it exists to keep us out of.
        Assert.True(scanned > 0, "no curated page mentions a grade any more");
        Assert.True(offenders.Count == 0,
            "CNS5 uses Arabic grade numerals, and dropped Roman ones in 2021 because II/III/IV "
            + "get transcribed wrong (contract item 3):\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void ThePageFollowsTheTemplateOpeningAndClosing()
    {
        // §12.8 slots 0, 10 and 11. The .Trim() is load-bearing on Windows:
        // `.` matches `\r`, so on a CRLF checkout a capture ends with one.
        var headings = Regex.Matches(Page, @"^## (.+)$", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        Assert.Equal("The short version", headings[0]);
        Assert.Equal("What to ask your team", headings[^2]);
        Assert.Equal("Where to go next", headings[^1]);
    }

    [Fact]
    public void NothingOnThePageIsBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone, and there is no outlook here.
        Assert.DoesNotContain(":::", Page);
    }

    [Fact]
    public void TheFrontMatterCarriesEverythingTheContractRequires()
    {
        // Contract item 8. ContentCheck only warns on a missing `accessed`
        // date, and this phase's whole source discipline rests on it.
        var front = CuratedPage.FrontMatter(Page);

        Assert.Contains("disclaimers: [medical]", front);
        Assert.Contains("reviewed:", front);
        Assert.Contains("review_due:", front);

        var urls = Regex.Matches(front, @"- url: (\S+)").Select(m => m.Groups[1].Value).ToList();
        Assert.All(urls, u => Assert.StartsWith("https://", u));

        // The domains this page's claims actually rest on. NCI and ACS carry
        // the report's sections; Johns Hopkins carries what a BRAIN tumor
        // report holds; PMC8328013 (WHO CNS5) carries every naming and grading
        // claim, including the Roman-to-Arabic reason and NOS/NEC.
        foreach (var domain in new[] { "cancer.gov", "cancer.org", "pathology.jhu.edu", "ncbi.nlm.nih.gov" })
        {
            Assert.Contains(urls, u => u.Contains(domain, StringComparison.Ordinal));
        }

        // tests-library.md §5.4 sources both "what a brain tumor report
        // contains" and "one of the most important documents guiding treatment
        // decisions" to PMC4300589. That paper is "Brain tumors: Special
        // characters for research and banking" (Adv Biomed Res, 2015), a
        // biobanking and cytogenetics review, and it says neither. Johns
        // Hopkins carries both sentences verbatim and is what this page cites.
        // Pinned so a future session copying from the dossier fails here rather
        // than shipping a citation that resolves and does not say it — the
        // WI-505 failure mode, now seen on six citations across four items.
        Assert.DoesNotContain("PMC4300589", front, StringComparison.Ordinal);

        // Indented, so the page's own `title:` is not counted as a source's.
        // `\s*$`, never a bare `$`: .NET's multiline `$` does not match before
        // `\r`, and this repo has core.autocrlf=true, so on a real checkout a
        // `$`-anchored line pattern matches nothing (WI-501, WI-506).
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+accessed: \d{4}-\d{2}-\d{2}\s*$",
            RegexOptions.Multiline).Count);
        Assert.Equal(urls.Count, Regex.Matches(front, @"^[ \t]+title: \S", RegexOptions.Multiline).Count);
    }
}

/// <summary>The report page as served, its anchors, and the doors that lead to it.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class PathologyReportPageRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/tests/pathology-report";

    private readonly WebApplicationFactory<Program> _factory;

    public PathologyReportPageRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task ThePageIsServed()
    {
        var html = await _factory.CreateClient().GetStringAsync(Url);

        Assert.Contains("Your pathology report, part by part", html);
    }

    [Fact]
    public async Task ThePageIsReachableFromAllThreeOfItsDoors()
    {
        // The WI-412 orphan lesson. /tests still has no index, so this page is
        // reachable only by the links other pages carry to it.
        var client = _factory.CreateClient();

        foreach (var door in new[] { "/start", "/tests/mri", "/tests/waiting-for-results" })
        {
            Assert.Contains(Url, await client.GetStringAsync(door));
        }
    }

    [Fact]
    public async Task TheSectionsOtherPagesDeepLinkToKeepTheirAnchors()
    {
        // Acceptance: deep-linkable, so /tumors and feed items can point at an
        // exact term. Markdig's auto-identifiers are derived from the heading
        // TEXT, so rewording a heading silently breaks every inbound link to it
        // — and an anchor that no longer exists fails softly, dropping the
        // reader at the top of a long page with no sign anything went wrong.
        // These ids are a published interface. Changing one means updating the
        // pages that link to it, and this test is where that gets noticed.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var anchor in new[]
                 {
                     "where-the-answer-sits-on-the-page", "your-report-part-by-part",
                     "why-the-name-may-not-be-the-one-you-were-told",
                     "what-the-grade-means", "what-nos-and-nec-mean",
                 })
        {
            Assert.Contains($"id=\"{anchor}\"", html);
        }
    }

    [Fact]
    public async Task TheDeepLinkIntoTheWaitPageStillLandsOnASection()
    {
        // This page sends the reader to one exact section of WI-507 rather than
        // restating the tumor board and the patient portal. The link check
        // below strips fragments, so without this the anchor could rot and
        // every gate would stay green.
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync(Url);

        var fragments = Regex.Matches(html, "href=\"(/[^\"?]*)#([a-z0-9-]+)\"")
            .Where(m => !m.Groups[1].Value.StartsWith("/glossary", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(fragments);

        foreach (var link in fragments)
        {
            var target = await client.GetStringAsync(link.Groups[1].Value);
            Assert.Contains($"id=\"{link.Groups[2].Value}\"", target);
        }
    }

    [Fact]
    public async Task EveryLinkOnThePageResolves() =>
        // The doors this page owes its reader: back to the wait page when
        // half the report is still missing (WI-507 -> WI-508 -> WI-509 is a
        // deliberate sequence), out to a person, and to the glossary, which
        // is where every unfamiliar word on the document resolves.
        await CuratedPage.AssertLinksResolve(
            _factory.CreateClient(), Url,
            "/tests/waiting-for-results", "/get-help-now", "/glossary");

    [Fact]
    public async Task TheReportVocabularyFiresAsTooltipsOnTheRealPage()
    {
        // Contract item 9. Asserted on the rendered page rather than on the
        // glossary directory, because a term nothing matches is a term nobody
        // ever sees. "def-<slug>" is the popover the tooltip button targets —
        // asserting the bare word would pass on prose never marked up at all.
        var html = await _factory.CreateClient().GetStringAsync(Url);

        foreach (var slug in new[]
                 {
                     "gross-description", "immunohistochemistry", "cns-who-grade",
                     "nos", "nec", "addendum", "neuropathologist",
                 })
        {
            Assert.Contains($"def-{slug}", html);
        }
    }
}
