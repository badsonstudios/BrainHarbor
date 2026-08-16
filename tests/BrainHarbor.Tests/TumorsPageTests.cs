using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-412: "what is this?" for every tumor type a reader can filter by.
///
/// The load-bearing property is that the page is driven by the SAME
/// `taxonomy.yml` the research filter uses. A hand-maintained second list would
/// drift, and the failure would be silent: a reader filters by a type, gets
/// results, then finds nothing explaining what it is.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public class TumorsPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TumorsPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));
    }

    private async Task<string> GetTumorsAsync() =>
        await _factory.CreateClient().GetStringAsync("/tumors");

    [Fact]
    public async Task EveryTypeInTheTaxonomyAppearsOnThePage()
    {
        var taxonomy = new BrainHarbor.Web.Content.TaxonomyStore(
            await File.ReadAllTextAsync(Path.Combine(
                RepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml")));

        var html = await GetTumorsAsync();

        foreach (var type in taxonomy.TumorTypes)
        {
            // The anchor id, so /research can deep-link "what is this?".
            Assert.Contains($"id=\"{type.Slug}\"", html);
            Assert.Contains(type.Label, html);
        }
    }

    /// <summary>
    /// A type with no description written must say so, not render blank. A
    /// blank row reads as "this site has nothing for you", which is false —
    /// the feed still filters by that type.
    /// </summary>
    [Fact]
    public async Task ATypeWithNoDescriptionSaysSoAndStillOffersTheResearch()
    {
        var html = await GetTumorsAsync();

        // At least one type is unwritten today (the taxonomy has 24 entries and
        // fewer descriptions). If that ever stops being true this assertion
        // should be deleted deliberately, not weakened.
        Assert.Contains("We are still writing this one", html);
        Assert.Contains("/research?tumor=", html);
    }

    [Fact]
    public async Task AWrittenTypeLinksToItsOwnPageAndThatPageRenders()
    {
        var html = await GetTumorsAsync();
        Assert.Contains("/tumors/glioblastoma", html);

        var page = await _factory.CreateClient().GetAsync("/tumors/glioblastoma");
        page.EnsureSuccessStatusCode();

        var body = await page.Content.ReadAsStringAsync();
        Assert.Contains("Glioblastoma", body);

        // The medical disclaimer rides on the front matter, like every other
        // curated page. These are pages a newly diagnosed person reads.
        Assert.Contains("not medical advice", body, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The spinal cord entry must not read as a brain tumor. taxonomy.yml is
    /// explicit that it is not one and must never surface under a brain filter;
    /// grouping it in with the brain types on this page would quietly undo that.
    /// </summary>
    [Fact]
    public async Task TheSpinalCordEntryIsNotPresentedAsABrainTumor()
    {
        var html = await GetTumorsAsync();

        Assert.Contains("Tumors of the spinal cord", html);

        var spinalHeading = html.IndexOf("Tumors of the spinal cord", StringComparison.Ordinal);
        var spinalEntry = html.IndexOf("id=\"spinal-cord-tumor\"", StringComparison.Ordinal);
        Assert.True(spinalHeading > 0 && spinalEntry > spinalHeading,
            "the spinal cord type must sit under its own heading, not among the brain types");
    }

    /// <summary>
    /// WI-412 shipped ORPHANED: the page existed, worked, and nothing on the
    /// site linked to it — not the nav, not the footer, not sitemap.xml. Dan
    /// found it by noticing he could not get there. The link check that already
    /// exists only proves links do not 404; it cannot see a page that no link
    /// points at.
    ///
    /// This asserts the general property rather than the one instance: every
    /// path the sitemap advertises must be reachable by following a link from
    /// the home page, because a page a reader cannot navigate to may as well
    /// not exist.
    /// </summary>
    [Fact]
    public async Task EverySitemapPathIsReachableByALinkFromTheHomePage()
    {
        var client = _factory.CreateClient();
        var home = await client.GetStringAsync("/");
        var sitemap = await client.GetStringAsync("/sitemap.xml");

        var advertised = System.Text.RegularExpressions.Regex
            .Matches(sitemap, @"<loc>(.*?)</loc>")
            .Select(m => new Uri(m.Groups[1].Value).AbsolutePath)
            .Where(p => p.Length > 1)          // "/" is the page we start from
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(advertised);

        foreach (var path in advertised)
        {
            Assert.True(
                home.Contains($"href=\"{path}\"", StringComparison.Ordinal),
                $"nothing on the home page links to {path}, so a reader cannot get there");
        }
    }

    /// <summary>
    /// WI-412's acceptance: the /research tumor filter offers "what is this?".
    /// The reader who picked a type off that dropdown because it is the word on
    /// their pathology report is the one who most needs the explanation.
    /// </summary>
    [Fact]
    public async Task FilteringResearchByATypeOffersTheExplanationOfThatType()
    {
        var client = _factory.CreateClient();

        var filtered = await client.GetStringAsync("/research?tumor=glioblastoma");

        // The whole link, not just the href. Asserting only the href let a
        // broken Razor expression ship text that read "What is
        // System.Collections.Generic.List`1[...]" while the test stayed green.
        Assert.Contains(
            "<a href=\"/tumors#glioblastoma\">What is Glioblastoma?</a>", filtered);

        // Unfiltered, there is no single type to explain — the prompt would be
        // meaningless, so it must not appear.
        var unfiltered = await client.GetStringAsync("/research");
        Assert.DoesNotContain("/tumors#", unfiltered);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BrainHarbor.slnx")))
        {
            dir = dir.Parent!;
        }
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
