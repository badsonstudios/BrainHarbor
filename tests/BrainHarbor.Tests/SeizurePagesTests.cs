using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-559 and WI-560: the two seizure pages, written together because they are
/// one subject split by whether it is happening right now.
///
/// WI-559 is the only page on this site a reader may be ACTING from rather than
/// reading, so its format is a safety feature and is tested like one: the steps
/// are a real ordered list (they survive the stylesheet dying), they come
/// first, and nothing on the page sits behind a disclosure the reader has to
/// find and click while someone is on the floor.
///
/// WI-560's hard rule is the driving one, and it is checked across every
/// curated page rather than just this one: US seizure-free periods vary by
/// state, so a duration printed on a page is wrong for most of the people
/// reading it. Explain the shape, link the state tool, print no number.
/// </summary>
public sealed class SeizureContentTests
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

    private static string PagesRoot => Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages");

    private static string Read(string relative) =>
        File.ReadAllText(Path.Combine(PagesRoot, relative.Replace('/', Path.DirectorySeparatorChar) + ".md"));

    private static IEnumerable<(string Slug, string Text)> AllPages() =>
        Directory.EnumerateFiles(PagesRoot, "*.md", SearchOption.AllDirectories)
            .Select(f => (
                Path.GetRelativePath(PagesRoot, f).Replace('\\', '/')[..^3],
                File.ReadAllText(f)));

    // "six months", "12 months", "one year", "2 years" — the shapes a waiting
    // period actually gets written in. Spelled-out numbers matter as much as
    // digits: "six months" is the phrasing a drafting session reaches for.
    private static readonly Regex Duration = new(
        @"\b(\d+|one|two|three|four|five|six|seven|eight|nine|ten|twelve|eighteen)[\s-]+(week|month|year)s?\b",
        RegexOptions.IgnoreCase);

    [Fact]
    public void NoCuratedPagePrintsADrivingWaitingPeriod()
    {
        // Checked per sentence rather than per page: a page may legitimately
        // say "six weeks of radiation" and also mention driving.
        var offenders = new List<string>();
        var scanned = 0;

        foreach (var (slug, text) in AllPages())
        {
            foreach (var sentence in Regex.Split(text, @"(?<=[.!?])\s+"))
            {
                if (!sentence.Contains("driv", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                scanned++;
                if (Duration.IsMatch(sentence))
                {
                    offenders.Add($"{slug}: {sentence.Trim()}");
                }
            }
        }

        // Without this the test passes just as happily on a corpus that says
        // nothing about driving at all, which is the state it is meant to
        // protect us from drifting out of.
        Assert.True(scanned > 0, "no page mentioned driving — did the driving section get lost?");
        Assert.True(offenders.Count == 0,
            "driving rules are jurisdictional and no page may print a duration "
            + "(content-pipeline §12.3 section 9):\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void PrintKeepsGlossaryTermsAsWords()
    {
        // A glossary term renders as a <button> so its definition can pop over
        // with no JavaScript (WI-105). print.css hides every button as
        // interactive chrome, which silently DELETED those words from every
        // printed page — "if they have a rescue medicine" printed as "if they
        // have a". Found on this page, which is the one people print.
        var css = File.ReadAllText(Path.Combine(
            RepoRoot(), "src", "BrainHarbor.Web", "wwwroot", "css", "print.css"));

        Assert.Matches(@"button\.term\s*\{[^}]*display:\s*inline\s*!important", css);
    }

    [Fact]
    public void TheDrivingSectionSendsTheReaderToTheStateTool()
    {
        // Having removed the number, the page owes the reader the way to find
        // their own. Without this, "we do not print it here" is just a gap.
        var text = Read("seizures/living-with");

        Assert.Contains("epilepsy.com/lifestyle/driving-and-transportation/laws", text);
        Assert.Contains("ask your care team", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheEmergencyPageIsNotUSOnlyAboutCallingForHelp()
    {
        // WI-455/457 deliberately put non-US readers on this site. A page that
        // says only "call 911" tells a reader in Finland nothing.
        var text = Read("seizures/what-to-do");

        Assert.Contains("911", text);
        Assert.Contains("local emergency number", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheEmergencyPageStartsWithTheSteps()
    {
        // Not an aesthetic preference. A reader on this page may be standing
        // over someone; anything above the steps is something they have to
        // read past first.
        var text = Read("seizures/what-to-do");
        var body = text[(text.IndexOf("\n---", 3, StringComparison.Ordinal) + 4)..];
        var firstHeading = Regex.Match(body, @"^## .*$", RegexOptions.Multiline);

        Assert.True(firstHeading.Success, "the page has no headings");
        Assert.Equal("## Do this now", firstHeading.Value.Trim());
        Assert.True(body[..firstHeading.Index].Trim().Length == 0,
            "nothing may come before the steps: " + body[..firstHeading.Index].Trim());
    }

    [Fact]
    public void TheEmergencyPageCoversWhatTheSourcesSayToDoAndNotDo()
    {
        // The specific items are here because leaving any one of them out is a
        // real harm, not a gap: the folk advice ("hold them down", "something
        // in the mouth") is actively dangerous, and the ambulance triggers are
        // the whole reason a bystander reads this page.
        var text = Read("seizures/what-to-do").ToLowerInvariant();

        foreach (var required in new[]
        {
            "longer than 5 minutes",   // Epilepsy Foundation + CDC
            "in water",
            "first ever",
            "on their side",
            "do not put anything in their mouth",
            "do not hold them down",
        })
        {
            Assert.Contains(required, text);
        }
    }

    [Fact]
    public void NeitherSeizurePageHidesAnythingBehindTheReaderChoiceGate()
    {
        // WI-503's gate is for outlook alone. Steps a frightened person needs
        // must never be one click away from invisible.
        foreach (var slug in new[] { "seizures/what-to-do", "seizures/living-with" })
        {
            Assert.DoesNotContain(":::", Read(slug));
        }
    }

    [Fact]
    public void TheTwoPagesPointAtEachOther()
    {
        // They are one subject split by whether it is happening right now, so
        // a reader who lands on the wrong one has to be able to get to the
        // other in a single obvious step.
        Assert.Contains("/seizures/living-with", Read("seizures/what-to-do"));
        Assert.Contains("/seizures/what-to-do", Read("seizures/living-with"));
    }

    [Fact]
    public void LivingWithSeizuresHoldsBothSidesOfTheEvidence()
    {
        // The finding this page exists to carry cuts both ways: most
        // restriction is not evidence-based, AND exertion-triggered seizures
        // are real for a minority. A page that drops the second half tells the
        // reader who has observed it in herself that she is wrong about her
        // own body.
        // Whitespace-normalised: the source is hard-wrapped, and a sentence
        // that happens to break across lines is not a content change.
        var text = Regex.Replace(Read("seizures/living-with"), @"\s+", " ");

        Assert.Contains("told to do less than they need to", text);
        Assert.Contains("you are not imagining it", text);
    }

    /// <summary>
    /// THE DOOR BACK, so a vision reader who lands on this section is not told by omission
    /// that the question does not apply to them.
    ///
    /// <para><b>This section is seizure-scoped throughout</b> — <i>"rules about driving
    /// after a seizure"</i>, <i>"free of seizures for a set length of time"</i> — and its
    /// lookup tool is the Epilepsy Foundation's, which does not answer a vision question.
    /// Much of the corpus routes here, and WI-573 added a route for a reader whose licence
    /// problem is what they can SEE — §12.19 finding 6 is that a route can be word-perfect
    /// and still be addressed to the wrong reader. <b>An earlier version of this paragraph
    /// said "Four pages route here", and the copy of the same count in
    /// <c>WhereYourTumorIsPageTests</c> said three. Both were wrong — and their
    /// replacements both said "the enumeration says eleven" about an enumeration that did
    /// not exist.</b> §12.20: a count in prose about something the code can enumerate can be
    /// falsified by the sentence that states it, and two homes gave two answers twice over.
    /// <c>LocationObligationSweepTests</c> enumerates it now, which is the only form of
    /// this claim that maintains itself.</para>
    ///
    /// <para>So the bound is stated on both sides. The outbound half is asserted on
    /// <c>/where-your-tumor-is</c>; this is the inbound half. <b>The direction that
    /// matters is the omission one:</b> a reader with field loss and no seizures who reads
    /// only this section concludes there is no rule for them, and there is.</para>
    /// </summary>
    [Fact]
    public void TheDrivingSectionSaysSightIsASeparateRequirementAndRoutesToIt()
    {
        var text = Regex.Replace(Read("seizures/living-with"), @"\s+", " ");

        Assert.Contains("Sight is a separate requirement, set by whoever issues your license.", text);

        // AND IT DOES NOT NARROW ITS OWN AUDIENCE. /review round 4: the clause read
        // "what you can see RATHER THAN seizures", which is exclusive — so a reader
        // with both (common with an occipital or a large hemispheric tumor) read it,
        // decided the paragraph was about somebody else, used the seizure lookup,
        // cleared the seizure-free interval and drove. This is the only sight signal
        // on the corpus's driving destination, and it excluded the readers likeliest
        // to need it. **A route that narrows its own audience drops the overlap, and
        // the overlap is where the risk is.**
        Assert.Contains("That stays true whether or not you also have seizures", text,
            StringComparison.Ordinal);

        // NOT "it makes no difference whether you also have seizures". /review
        // round 5: the dummy `it` has no stated complement, so a tired reader can
        // land on "whether you also have seizures makes no difference" — which
        // cancels the seizure rule, in the under-triage direction, for the exact
        // overlap reader the clause was written to rescue.
        Assert.DoesNotContain("makes no difference whether you also have seizures", text,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("what you can see rather than seizures", text,
            StringComparison.OrdinalIgnoreCase);

        // AND IT SITS IN THE GAP AFTER THE STATE LOOKUP, which is where round 3 found
        // NOT. The paragraph was the last in `## Driving`, and its own closing clause
        // promised the reader that "everything below" was about getting around
        // without a car — while everything below it was employer disclosure and the
        // ADA. The transport material it meant is ABOVE it. It is now immediately
        // after the Epilepsy Foundation state lookup, which is the sentence that
        // would mislead a vision reader, rather than six paragraphs downstream of it.
        // THE BODY, NOT THE WHOLE FILE. The first version of this searched `text`,
        // which includes the front matter — and "Epilepsy Foundation" is the start of
        // FIVE source `title:` lines up there. So `lookupAt` resolved to the source
        // list, `lookupAt < sightAt` was trivially true wherever the paragraph sat,
        // and this assertion was insensitive to the exact regression it was written
        // for. /review round 4 proved it by moving the paragraph back and watching it
        // pass. **A positional assertion is only as good as its two landmarks.**
        var body = CuratedPage.ReaderText(Read("seizures/living-with").Replace("\r\n", "\n"));

        var sightAt = body.IndexOf("Sight is a separate requirement",
            StringComparison.Ordinal);
        var lookupAt = body.IndexOf("Epilepsy Foundation's driving laws tool",
            StringComparison.Ordinal);
        // THE CLOSING LANDMARK IS THE NEXT PARAGRAPH, NOT THE NEXT HEADING. Round 4
        // used "## Work", and `lookup < sight < work` is satisfied anywhere in the last
        // two thirds of this section — including the exact place round 3 moved the
        // paragraph OUT of. The harness caught it a second time. **A positional
        // assertion needs landmarks that BRACKET the position.**
        var outsideAt = body.IndexOf("Outside the US", StringComparison.Ordinal);

        // AND THE LINK IS A LANDMARK OF ITS OWN. /review round 5 built the regression
        // the previous bracket missed: leave the lead sentence where it is and move
        // only the DOOR to the end of the section. Every landmark stays ordered, the
        // string assertion searches the whole file and stays green, and the reader
        // meets the misleading lookup six paragraphs before the way out — which is the
        // round-3 defect, reproduced. **Bracket the thing that has to be adjacent.**
        var linkAt = body.IndexOf("/where-your-tumor-is#your-sight", StringComparison.Ordinal);

        Assert.True(sightAt > 0 && lookupAt > 0 && outsideAt > 0 && linkAt > 0,
            $"a landmark was not found in the body (sight {sightAt}, lookup {lookupAt}, "
            + $"link {linkAt}, outside {outsideAt}), so the ordering assertion below "
            + "would compare "
            + "positions that do not mean what they say");

        Assert.True(lookupAt < sightAt && sightAt < linkAt && linkAt < outsideAt,
            $"the sight paragraph (at {sightAt}) has to sit in the gap between the state "
            + $"lookup ({lookupAt}) and the \"Outside the US\" paragraph ({outsideAt}). "
            + "It is the only sight signal on this page: a vision reader who meets the "
            + "seizure-free lookup first has been handed somebody else's answer, and one "
            + "who meets this six paragraphs downstream has already acted on it.");
        // THE TARGET EXACTLY, THE LABEL CASE-INSENSITIVELY. The label begins a
        // sentence here and did not in the paragraph this assertion was carried over
        // from, so the first version looked for a lowercase "[when" and found "[When".
        // §12.20's rule — copy the string out of the artifact rather than retyping it
        // — was written for smoke needles and is just as true of an assertion. The
        // TARGET is an interface and is matched exactly; the LABEL is prose a rewrap
        // may legitimately recase.
        Assert.Contains("(/where-your-tumor-is#your-sight)", text, StringComparison.Ordinal);
        Assert.Contains("[when what changed is your sight]", text,
            StringComparison.OrdinalIgnoreCase);

        // AND IT ADDS NO RULE OF ITS OWN -- asserted as the PROPERTY rather than as
        // two strings. /review round 1: the first draft banned "Massachusetts" and
        // "horizontal field" here, and this page has never contained either and does
        // not cite the source they come from, so neither assertion could have fired.
        // §12.19: a guard measured on a page it cannot fire on has been measured on
        // nothing. What the page actually owes is that the new paragraph states no
        // duration, which is the same bar the section two paragraphs up sets for
        // itself in its own words.
        var newParagraph = text[text.IndexOf(
            "Sight is a separate requirement", StringComparison.Ordinal)..];
        newParagraph = newParagraph[..newParagraph.IndexOf("## ", StringComparison.Ordinal)];

        var duration = new Regex(
            @"\b(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten|twelve|eighteen)"
            + @"[\s-]+(?:day|week|month|year)s?\b", RegexOptions.IgnoreCase);

        Assert.Matches(duration, "You must not drive for twelve months.");
        Assert.DoesNotMatch(duration, "The rule about sight is a separate rule.");
        Assert.DoesNotMatch(duration, newParagraph);

        // AND THE TWO STRING BANS ARE BACK, because /review's reason for dropping
        // them stopped being true inside the same review round. The argument was that
        // this page could not contain "Massachusetts" or "horizontal field" since it
        // does not cite the source they come from — and the fix for a DIFFERENT
        // finding in that round added PMC11913653 to this page's sources, for the
        // sight-requirement sentence above. The paste is plausible now, and the break
        // harness proved it: a planted threshold walked straight through the duration
        // scan, which by construction cannot see a number that is not a duration.
        //
        // TWO FINDINGS IN ONE REVIEW ROUND CAN INVALIDATE EACH OTHER. The duration
        // scan is the property; these two are the specific figures this page's own
        // new source hands over, and this page's whole argument is that it does not
        // print the reader's own rule.
        Assert.DoesNotContain("Massachusetts", text);
        Assert.DoesNotMatch(new Regex("horizontal[ -]field", RegexOptions.IgnoreCase), text);
    }
}

/// <summary>The same pages as served, plus the links other pages now make to them.</summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class SeizurePagesRenderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SeizurePagesRenderTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Theory]
    [InlineData("/seizures/what-to-do", "What to do when someone has a seizure")]
    [InlineData("/seizures/living-with", "Living with seizures")]
    public async Task ThePageIsServed(string url, string title)
    {
        var html = await _factory.CreateClient().GetStringAsync(url);

        Assert.Contains(title, html);
    }

    [Fact]
    public async Task TheStepsAreARealOrderedList()
    {
        // The site has to work with the stylesheet dead (a standing
        // constraint). Numbered steps typed as paragraphs lose their order the
        // moment CSS does not load, and order is the whole content here.
        var html = await _factory.CreateClient().GetStringAsync("/seizures/what-to-do");

        var steps = html[html.IndexOf("Do this now", StringComparison.Ordinal)..];
        Assert.StartsWith("<ol>", steps[steps.IndexOf("<ol", StringComparison.Ordinal)..]);
        Assert.True(Regex.Matches(steps[..steps.IndexOf("</ol>", StringComparison.Ordinal)], "<li>").Count >= 5,
            "the steps must be a list of at least five items");
    }

    [Fact]
    public async Task NothingOnTheEmergencyPageIsInsideADisclosure()
    {
        // The layout's mobile nav is a <details>; the ARTICLE must not have
        // one. Scoped to the article for exactly that reason.
        var html = await _factory.CreateClient().GetStringAsync("/seizures/what-to-do");
        var start = html.IndexOf("<article", StringComparison.Ordinal);
        var end = html.IndexOf("</article>", StringComparison.Ordinal);

        Assert.True(start >= 0 && end > start, "the page did not render an article");
        Assert.DoesNotContain("<details", html[start..end]);
    }

    [Fact]
    public async Task TheSeizurePagesAreReachableFromWhereAFrightenedReaderStarts()
    {
        // A page nothing links to is invisible (the WI-412 orphan lesson).
        // These are the three doors: the caregiver block on every tumor hub,
        // the newly-diagnosed page, and the crisis page.
        var client = _factory.CreateClient();

        foreach (var from in new[] { "/tumors/low-grade-glioma", "/start", "/get-help-now" })
        {
            var html = await client.GetStringAsync(from);
            Assert.Contains("/seizures/what-to-do", html);
        }
    }

    [Fact]
    public async Task EveryLinkOnBothSeizurePagesResolves()
    {
        var client = _factory.CreateClient();
        var broken = new List<string>();

        foreach (var page in new[] { "/seizures/what-to-do", "/seizures/living-with" })
        {
            var html = await client.GetStringAsync(page);
            foreach (Match match in Regex.Matches(html, "href=\"(/[^\"#?]*)\""))
            {
                var target = match.Groups[1].Value;
                if (target.StartsWith("/css/") || target.StartsWith("/js/"))
                {
                    continue;
                }

                if ((await client.GetAsync(target)).StatusCode != HttpStatusCode.OK)
                {
                    broken.Add($"{page} -> {target}");
                }
            }
        }

        Assert.True(broken.Count == 0, string.Join("\n", broken.Distinct()));
    }

    [Fact]
    public async Task TheEmergencyPageIsInTheSitemap()
    {
        // It is a search-box page: people type "what to do when someone has a
        // seizure" while it is happening. /seizures/living-with is reached by
        // following the link from it, the same way the individual tumor types
        // are reached from /tumors.
        var xml = await _factory.CreateClient().GetStringAsync("/sitemap.xml");

        Assert.Contains("/seizures/what-to-do", xml);
    }
}
