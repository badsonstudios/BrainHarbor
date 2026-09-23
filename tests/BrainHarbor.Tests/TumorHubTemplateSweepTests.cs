using System.Text.RegularExpressions;
using BrainHarbor.Web.Content;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-548: every slug in <c>taxonomy.yml</c> resolves to a page carrying the
/// FULL section template — not merely to a file that exists.
///
/// The file-exists half already shipped and is already guarded:
/// <c>TumorsPageTests.EveryShippedTypeHasAPageSoTheIndexSaysStillWritingNowhere</c>
/// asserts that the real index says "still writing" nowhere and that every slug
/// carries an href to its own page, and
/// <c>TumorsPageTests.ATypeWithNoDescriptionSaysSoAndStillOffersTheResearch</c>
/// still proves the live Razor fallback branch by supplying its OWN unwritten
/// type. Neither is weakened here. This class adds the complement.
///
/// <para><b>The ruling this item turns on.</b> "The full section template" is
/// not the same list for every slug, and a test demanding §12.3's seventeen
/// sections of all twenty-three goes red on pages that are correctly written.
/// So the unit of obligation is a <b>question</b>, not a heading string, and a
/// page discharges one of the seventeen in exactly one of three ways:</para>
/// <list type="number">
///   <item>answers it under the standard heading — the default, and what twenty
///     of the twenty-three pages do;</item>
///   <item>answers it under its OWN heading, because the standard wording would
///     be false for this reader;</item>
///   <item>ROUTES it to the page that owns the answer (§12.10: route, don't
///     restate).</item>
/// </list>
/// <para>A page may never simply drop one. That is what stops the exception
/// list from being "the pages that currently fail": an exception here is not a
/// skip, it is a DIFFERENT, EQUALLY COMPLETE obligation that is still checked.
/// Every class below is defined by a property of the SLUG, so its reason would
/// still hold for a page nobody has written yet.</para>
///
/// <para><b>What a new taxonomy entry inherits: <see cref="PageClass.Hub"/> —
/// all seventeen, standard headings, §12.3 order.</b> The three non-default
/// classes are a closed set pinned in <see cref="Classes"/>. A new slug cannot
/// acquire an exemption by omission, and cannot declare one in content:
/// acquiring one means editing this file, which is the most-reviewed place to
/// put it and where a reason has to be written down.</para>
/// </summary>
public sealed class TumorHubTemplateSweepTests
{
    // ---------------------------------------------------------------- template

    /// <summary>
    /// §12.3's order, as the corpus actually writes it. Section 16 ("Where this
    /// came from / last reviewed") is rendered from front matter rather than
    /// written as a heading, and the self-blame block is mandated in §12.3's
    /// prose rather than in its table — so seventeen `## ` sections, which is
    /// exactly what twenty of the twenty-three pages carry. The template is not
    /// an abstraction; it is the measured shape of the corpus.
    ///
    /// <see cref="TheTemplateThisTestEnforcesStillMatchesTheDesignDoc"/> fails if
    /// §12.3's SHAPE changes (its seventeen numbered rows) and if any of the
    /// TWELVE rows whose doc wording is the heading is reworded. Five rows are
    /// deliberately unpoliced and the test says which and why. They are
    /// hard-coded rather than parsed out
    /// of the doc because §12.3's table cells are prose ("What is a [tumor]?"),
    /// not the literal headings: parsing them would need a mapping layer that
    /// became the thing to keep in step, and its failures would be unreadable.
    /// One source of truth for the SHAPE, an explicit list for the WORDING.
    /// </summary>
    private static readonly (string Key, string Pattern)[] Template =
    [
        ("0  the short version",          @"^The short version$"),
        // The lookahead excludes section 8's heading, which otherwise satisfies
        // this matcher. Without it a page missing "What is a ...?" is scored as
        // having it, using a heading that sits nine sections further down.
        ("1  what is it",                 @"^What (is|are) (?!treatment actually like)"),
        ("2  is it cancer / what grade",  @"^Is it cancer\? What does (its|the) grade mean\?$"),
        ("3  where it grows, and why",    @"^Where does it grow, and why does it cause these symptoms\?$"),
        ("4  what symptoms",              @"^What symptoms does it cause"),
        ("5  how doctors find out",       @"^How do doctors find out it is this\?$"),
        ("6  the words on the report",    @"^What do the words on my (child's )?report mean\?$"),
        ("7  how it is treated",          @"^How is it usually treated\?$"),
        ("8  what treatment is like",     @"^What is treatment actually like, and what is normal afterwards\?$"),
        ("9  everyday life",              @"^Everyday life"),
        ("10 follow-up scans",            @"^Follow-up scans, and what to do while you wait$"),
        ("11 if it comes back",           @"^If it comes back"),
        ("SB did I cause this",           @"^Did I cause this\?$"),
        ("12 what might happen",          @"^What might happen over time$"),
        ("13 for the person caring",      @"^For the person caring for someone with this$"),
        ("14 what to ask your team",      @"^What to ask your team$"),
        ("15 where to get support",       @"^Where to get support$"),
    ];

    // ----------------------------------------------------------------- classes

    private enum PageClass
    {
        /// <summary>The slug names one diagnosis a person can be told they have.</summary>
        Hub,

        /// <summary>The slug names a tumor that did not start in the CNS.</summary>
        Secondary,

        /// <summary>The slug names a cross-cutting axis (age), not a histology.</summary>
        Axis,

        /// <summary>The slug names no diagnosis at all — §12.11's pre-diagnosis catch-all.</summary>
        Umbrella,
    }

    /// <summary>
    /// How a page discharges a question it does not answer under the standard
    /// heading. At least one of the two must be supplied, and every one that IS
    /// supplied is asserted — an override is an obligation, not a waiver.
    ///
    /// <paramref name="RouteFrom"/> is required alongside <paramref name="Route"/>:
    /// /review round 1 found the route check was a whole-page <c>Contains</c>, so
    /// a link anywhere — including the support list in the footer, or a URL in a
    /// YAML comment — discharged the question. That is the footer-drift failure
    /// <c>CuratedPage.Section</c>'s own doc comment warns about, and it was
    /// discharging TEN of the umbrella's questions off one link.
    /// </summary>
    private sealed record Discharge(
        string Why, string? OwnHeading = null, string? Route = null, string? RouteFrom = null);

    /// <summary>
    /// The closed set of slugs outside the default class, with the reason each
    /// is a real class rather than a page that happens to fail today.
    ///
    /// <c>spinal-cord-tumor</c> is deliberately NOT here. taxonomy.yml is
    /// emphatic that it is not a brain tumor, which is a FRAMING ruling, not a
    /// template one — the page carries all seventeen in §12.3 order and is a
    /// plain <see cref="PageClass.Hub"/>. Recorded so the next reader does not
    /// re-open it.
    /// </summary>
    private static readonly Dictionary<string, PageClass> Classes = new(StringComparer.Ordinal)
    {
        // WHO CNS5 grades tumors that START in the CNS. A deposit from a lung
        // or breast cancer has no CNS grade — this page's own front matter says
        // "this page has no WHO grade in it to get wrong" — and "is it cancer?"
        // is already answered by the word metastasis. A future secondary slug
        // inherits the same substitutions from the tumor's origin.
        ["brain-metastases"] = PageClass.Secondary,

        // The reader's child has a specific diagnosis, which has its own page,
        // so the per-diagnosis answers are routed or re-voiced rather than
        // restated. The reader is a parent: "my report" becomes "the report".
        ["pediatric-brain-tumor"] = PageClass.Axis,

        // §12.11. The reader has not been given a name — that is what the class
        // MEANS, so every "what is it" question is literally unanswerable and
        // the page owes the pre-diagnosis ladder instead.
        ["all-brain-tumors"] = PageClass.Umbrella,
    };

    /// <summary>
    /// What a class OWES a ruling for. The wording of a substitute heading is
    /// necessarily page-specific — "Why it is still called by the other cancer's
    /// name" cannot be inherited as a literal — but the OBLIGATION is not, and
    /// this is where the class earns its name.
    ///
    /// /review round 1 was right that without this the enum was decorative:
    /// every substitution hung off the slug, so the claim that "a future
    /// secondary slug inherits the same substitutions" was false as implemented.
    /// It is now true in the only sense it can be: classify a new slug as
    /// Secondary and the sweep fails until these questions have a recorded
    /// ruling, rather than silently accepting the standard headings.
    /// </summary>
    private static readonly Dictionary<PageClass, string[]> ClassOwesARulingFor = new()
    {
        // No CNS grade exists for it, and the self-blame question is a different
        // question when the reader has had cancer before.
        [PageClass.Secondary] = ["2  is it cancer / what grade", "SB did I cause this"],

        // The axis names no tumor, so grade belongs to the diagnosis page and the
        // identity question cannot be "what is it".
        [PageClass.Axis] = ["1  what is it", "2  is it cancer / what grade"],
    };

    /// <summary>
    /// Per-slug overrides: the question, and how this page discharges it
    /// instead. A question with no entry here must appear under its standard
    /// heading, whatever the page's class.
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, Discharge>> Overrides =
        new(StringComparer.Ordinal)
        {
            ["brain-metastases"] = new(StringComparer.Ordinal)
            {
                ["2  is it cancer / what grade"] = new(
                    "a secondary tumor carries no WHO CNS grade, and the word metastasis has already "
                    + "answered 'is it cancer'; the question this reader actually has is why it keeps "
                    + "the other cancer's name",
                    OwnHeading: @"^Why it is still called by the other cancer's name$"),

                ["SB did I cause this"] = new(
                    "this reader had a previous cancer, so the guilt is about a missed symptom or a "
                    + "skipped scan rather than about having caused a tumor",
                    OwnHeading: @"^Did I let this happen\?$"),
            },

            ["pediatric-brain-tumor"] = new(StringComparer.Ordinal)
            {
                ["1  what is it"] = new(
                    "the slug names an age axis, not a tumor, so there is no 'it' to define",
                    OwnHeading: @"^What ""pediatric brain tumor"" means$"),

                ["2  is it cancer / what grade"] = new(
                    "grade is a property of the child's actual diagnosis, which has its own page; "
                    + "§12.10 says route rather than restate",
                    Route: "/tests/pathology-report#what-the-grade-means",
                    RouteFrom: "What the words on the report mean"),

                ["3  where it grows, and why"] = new(
                    "re-voiced to a parent",
                    OwnHeading: @"^Where it grows, and why that causes symptoms$"),

                ["4  what symptoms"] = new(
                    "re-voiced to a parent: a child's symptoms are observed, not reported",
                    OwnHeading: @"^What symptoms look like in a child$"),

                ["5  how doctors find out"] = new(
                    "re-voiced to a parent",
                    OwnHeading: @"^How doctors find out$"),

                ["6  the words on the report"] = new(
                    "re-voiced to a parent: the report is the child's, not the reader's",
                    OwnHeading: @"^What the words on the report mean$"),

                ["7  how it is treated"] = new(
                    "the page's whole thesis is that a child's treatment differs from an adult's, "
                    + "so the heading carries the angle",
                    OwnHeading: @"^How treatment is different for a child$"),

                ["10 follow-up scans"] = new(
                    "a child's follow-up is scans plus growth, hearing and development check-ups",
                    OwnHeading: @"^Follow-up scans, and the check-ups that come with them$"),

                // §11 is deliberately NOT overridden. Before this item the page
                // said nothing whatsoever about the tumor coming back — no
                // section, no sentence, no route. Calling that a property of
                // the axis class would be precisely the failure this test
                // exists to prevent, so the class owes §11 like every other,
                // and WI-548 wrote it.
            },

            // The umbrella's obligations are its own list (see UmbrellaTemplate),
            // so it needs no per-question overrides.
            ["all-brain-tumors"] = new(StringComparer.Ordinal),
        };

    /// <summary>
    /// What §12.11's umbrella owes instead of §12.3. Not "whatever it happens to
    /// have": it is the pre-diagnosis ladder plus every section that is about
    /// the READER rather than about the tumor, which no class is excused from.
    /// The page's genuinely optional material — the tumor board, asking someone
    /// else to look, what you can do this week, what you were sent home with —
    /// is good content and is deliberately NOT required here.
    /// </summary>
    private static readonly (string Key, string Pattern)[] UmbrellaTemplate =
    [
        ("U0  the short version",        @"^The short version$"),
        ("U1  what you were told",       @"^What have you actually been told\?$"),
        ("U2  what happens next",        @"^What happens next$"),
        ("U3  why the scan cannot say",  @"^Why the scan cannot say on its own$"),
        ("U4  when to call",             @"^When to call, and what for$"),
        ("U5  will there be a biopsy",   @"^Will I need a piece of it taken\?$"),
        ("U6  how long until a name",    @"^How long until it has a name\?$"),
        ("U7  benign is wrong here",     @"^Why ""benign"" is the wrong comfort word here$"),
        ("U8  why there is no stage",    @"^Why nobody has given you a stage$"),
        ("U9  primary or secondary",     @"^Did it start in my brain, or somewhere else\?$"),
        ("USB did I cause this",         @"^Did I cause this\?$"),
        ("U10 what might happen",        @"^What might happen over time$"),
        ("U11 for the person caring",    @"^For the person caring for someone with this$"),
        ("U12 what to ask your team",    @"^What to ask your team$"),
        ("U13 where to get support",     @"^Where to get support$"),
    ];

    /// <summary>
    /// The umbrella cannot answer the diagnosis questions, so it owes the reader
    /// the place they get answered the moment there is a name — and it owes it
    /// FROM THE LADDER, not from the support list at the foot of the page.
    ///
    /// /review round 1: the only <c>/tumors</c> link on that page was a bullet in
    /// "Where to get support", and this test accepted it, so ten §12.3 questions
    /// were being discharged by a footer link. The page now carries the route at
    /// the point the reader actually needs it — the section about waiting for the
    /// name — and the assertion is scoped to that section.
    ///
    /// Expressed as a <see cref="Discharge"/> like every other ruling, not as two
    /// bare consts and a hand-written assert (/review round 2): this ONE route
    /// stands in for ten of §12.3's questions, so it is the last one that should
    /// escape the structure built to make rulings inspectable.
    /// </summary>
    private static readonly Discharge UmbrellaDischarge = new(
        "the reader has not been given a name, so every question about what 'it' is "
        + "is unanswerable here; the page owes them the place they get answered the "
        + "moment the name arrives",
        Route: "/tumors",
        RouteFrom: "How long until it has a name?");

    /// <summary>
    /// The §12.3 questions <see cref="UmbrellaDischarge"/> stands in for. Written
    /// down because "one link covers ten questions" is the kind of claim that
    /// should be counted rather than implied — and
    /// <see cref="TheUmbrellaReallyCannotAnswerTheQuestionsItRoutes"/> checks that
    /// the page genuinely does not answer them under its own headings.
    /// </summary>
    private static readonly string[] UmbrellaRoutesTheseQuestions =
    [
        "1  what is it", "3  where it grows, and why", "4  what symptoms",
        "5  how doctors find out", "6  the words on the report", "7  how it is treated",
        "8  what treatment is like", "9  everyday life", "10 follow-up scans",
        "11 if it comes back",
    ];

    /// <summary>
    /// The §12.3 questions the umbrella ANSWERS, under its own headings, and which
    /// of its headings do it.
    ///
    /// THIS TABLE EXISTS BECAUSE THE UMBRELLA HAD SILENTLY DROPPED ONE (/review
    /// round 3, and it was the round-2 fix that dropped it). Six of §12.3's
    /// seventeen were covered by <see cref="UmbrellaTemplate"/> and ten were
    /// routed: six plus ten is sixteen, and the missing one was
    /// "2  is it cancer / what grade" — the exact state §12.15 says may never
    /// exist. The umbrella really does answer it, in two halves, and "answers it
    /// in two halves" is a RECORDED MAPPING rather than an absence.
    ///
    /// The mismatch was visible the whole time as a stale number: the comments
    /// said eleven routed questions and the array held ten.
    /// </summary>
    private static readonly Dictionary<string, string[]> UmbrellaAnswersUnderItsOwnHeadings =
        new(StringComparer.Ordinal)
        {
            ["0  the short version"] = ["U0  the short version"],

            // The is-it-cancer/what-grade question, for a reader with no name
            // yet: what "benign" does and does not mean here, and why there is no
            // stage. It also routes the grade itself to /tests/pathology-report.
            ["2  is it cancer / what grade"] =
                ["U7  benign is wrong here", "U8  why there is no stage"],

            ["SB did I cause this"] = ["USB did I cause this"],
            ["12 what might happen"] = ["U10 what might happen"],
            ["13 for the person caring"] = ["U11 for the person caring"],
            ["14 what to ask your team"] = ["U12 what to ask your team"],
            ["15 where to get support"] = ["U13 where to get support"],
        };

    /// <summary>
    /// The smallest a required section's COMPOSED body may be. Derived by
    /// measurement rather than taste: across the 403 sections the tumor corpus
    /// carried before this item, the smallest composed body is 203 characters
    /// (<c>dipg</c> → "Follow-up scans"), the next 207
    /// (<c>diffuse-midline-glioma</c>, same section). 120 leaves roughly 40%
    /// headroom and still catches a bare heading or a one-line stub, which is
    /// what "carries the template, not merely exists" has to mean.
    ///
    /// COMPOSED, not raw: five pages discharge "Did I cause this?" with the
    /// bare <c>[CAUSES]</c> directive, which is eight characters on disk and a
    /// full section to a reader. A raw-text floor would fail correct pages.
    /// </summary>
    private const int MinimumComposedSectionLength = 120;

    // ------------------------------------------------------------------- tests

    [Fact]
    public void EveryTaxonomySlugResolvesToAPageCarryingItsClassTemplate()
    {
        var failures = new List<string>();

        foreach (var slug in TaxonomySlugs())
        {
            var path = PagePath(slug);
            if (!File.Exists(path))
            {
                failures.Add($"{slug}: taxonomy lists it and there is no page at {path}");
                continue;
            }

            var headings = HeadingsOf(File.ReadAllText(path));
            var required = RequiredSections(slug);
            var overrides = Overrides.TryGetValue(slug, out var o) ? o : [];

            var found = new List<(string Key, int Index)>();
            foreach (var (key, pattern) in required)
            {
                // Reported, not thrown, so one ambiguous heading cannot hide the
                // other 22 pages' findings.
                if (AmbiguityIn(headings, pattern, key) is { } ambiguous)
                {
                    failures.Add($"{slug}: {ambiguous}");
                }

                // The standard heading first; then the page's own, if it has a
                // ruling. A route-only discharge is checked separately, by
                // AQuestionNotAnsweredUnderTheStandardHeadingIsRoutedNotDropped.
                var index = IndexOfHeading(headings, pattern);
                if (index < 0 && overrides.TryGetValue(key, out var discharge))
                {
                    if (discharge.OwnHeading is not null)
                    {
                        index = IndexOfHeading(headings, discharge.OwnHeading);
                        if (index < 0)
                        {
                            failures.Add(
                                $"{slug}: '{key}' is ruled to be answered under its own heading "
                                + $"/{discharge.OwnHeading}/ ({discharge.Why}), and that heading is not there");
                        }
                    }
                    else
                    {
                        continue;   // routed; asserted by the routing test
                    }
                }
                else if (index < 0)
                {
                    failures.Add(
                        $"{slug}: '{key}' is missing. Every taxonomy slug owes it under the standard "
                        + "heading unless this file records a ruling that it is answered under the "
                        + "page's own heading or routed elsewhere (content-pipeline §12.3, §12.10).");
                }

                if (index >= 0)
                {
                    found.Add((key, index));
                }
            }

            for (var i = 1; i < found.Count; i++)
            {
                if (found[i].Index < found[i - 1].Index)
                {
                    failures.Add(
                        $"{slug}: '{found[i].Key}' comes before '{found[i - 1].Key}'. The order is the "
                        + "point of a standard — a reader who learns the shape on one page finds it on "
                        + "the next (§12.3).");
                }
            }
        }

        Assert.True(failures.Count == 0,
            "every taxonomy slug must resolve to a page carrying its class's full section template:\n  "
            + string.Join("\n  ", failures));
    }

    [Fact]
    public void EverySectionTheTemplateRequiresHasABodyNotJustAHeading()
    {
        // "Carries the template" cannot mean a list of headings with nothing
        // under them — that is a stub with extra steps, and a stub is the one
        // state this item exists to make impossible.
        var thin = new List<string>();
        var measured = 0;

        foreach (var slug in TaxonomySlugs())
        {
            var composed = CuratedPage.Composed(File.ReadAllText(PagePath(slug)), $"/tumors/{slug}");
            var sections = SectionsOf(composed);
            var overrides = Overrides.TryGetValue(slug, out var o) ? o : [];

            foreach (var (key, pattern) in RequiredSections(slug))
            {
                var patterns = new List<string> { pattern };
                if (overrides.TryGetValue(key, out var discharge) && discharge.OwnHeading is not null)
                {
                    patterns.Add(discharge.OwnHeading);
                }

                // Not FirstOrDefault (/review round 2): that silently resolved an
                // ambiguous matcher to whichever heading came first, so this test
                // and the sweep above disagreed about whether ambiguity is fatal.
                var matched = sections
                    .Where(s => patterns.Any(p => Regex.IsMatch(s.Heading, p)))
                    .ToList();

                // Collected, not thrown (/review round 3): round 2 removed the
                // throw from the sweep and left an identical one here, one test
                // over, where it would discard the accumulated list for every
                // remaining page and never reach the `measured` cross-check.
                if (matched.Count > 1)
                {
                    thin.Add($"{slug}: '{key}' matches {matched.Count} sections ("
                        + string.Join(" | ", matched.Select(s => s.Heading)) + ")");
                    continue;
                }

                var body = matched.Count == 1 ? matched[0].Body : null;

                if (body is null)
                {
                    continue;   // routed, or already reported by the sweep above
                }

                measured++;
                if (body.Length < MinimumComposedSectionLength)
                {
                    thin.Add($"{slug}: '{key}' has {body.Length} characters under it, below the "
                        + $"{MinimumComposedSectionLength}-character floor");
                }
            }
        }

        // The canary §12.11 asks for: a helper that quietly stops finding
        // anything reports every page as passing.
        //
        // DERIVED, NEVER TYPED. The first version asserted `> 300` against a real
        // 388, which is five or six whole pages of slack — the exact defect this
        // item fixes in CaregiverSectionTests, reintroduced by the item fixing it
        // (/review round 1). Every slug owes every section of its class's
        // template, minus the ones it routes, so that is the number.
        // The shared-zero hole closed first (/review round 2): if the slug list or
        // the template ever came back empty, both sides of the equality would be
        // 0 and it would pass.
        // The readable report first: a thin or ambiguous section would otherwise
        // surface as a bare "388 != 387" from the count below, with no slug and
        // no section name in the message.
        Assert.True(thin.Count == 0,
            "a required section must carry a body, not just a heading:\n  " + string.Join("\n  ", thin));

        Assert.True(measured > 0, "no section was measured at all");
        Assert.Equal(HeadingsOwedByTheCorpus, measured);
    }

    [Fact]
    public void AQuestionNotAnsweredUnderTheStandardHeadingIsRoutedNotDropped()
    {
        // The assertion that stops a class being a skip. Every ruling that says
        // "this page routes the question instead" has to produce the route —
        // FROM THE SECTION THAT OWES IT, not from anywhere on the page.
        var failures = new List<string>();
        var perSlugRoutes = 0;

        foreach (var (slug, overrides) in Overrides)
        {
            var composed = CuratedPage.Composed(File.ReadAllText(PagePath(slug)), $"/tumors/{slug}");

            foreach (var (key, discharge) in overrides)
            {
                if (discharge.OwnHeading is null && discharge.Route is null)
                {
                    failures.Add($"{slug}: the ruling for '{key}' discharges the question in no way at all");
                    continue;
                }

                if (discharge.Route is null)
                {
                    continue;
                }

                if (discharge.RouteFrom is null)
                {
                    failures.Add(
                        $"{slug}: the ruling for '{key}' routes to {discharge.Route} without naming the "
                        + "section it routes FROM, so the check would pass on a link in the footer");
                    continue;
                }

                perSlugRoutes++;
                if (SectionBody(composed, discharge.RouteFrom) is not { } routingSection)
                {
                    failures.Add(
                        $"{slug}: '{key}' is ruled to be routed from '{discharge.RouteFrom}', "
                        + "and that section does not exist on the page");
                }
                else if (!routingSection.Contains($"]({discharge.Route})", StringComparison.Ordinal))
                {
                    failures.Add(
                        $"{slug}: '{key}' is ruled to be ROUTED to {discharge.Route} from "
                        + $"'{discharge.RouteFrom}' ({discharge.Why}), and that section does not link "
                        + "there. A question may be answered elsewhere; it may not be dropped (§12.10).");
                }
            }
        }

        // The umbrella's whole class rests on one route: it cannot name the
        // tumor, so it owes the reader the place every one of §12.3's questions
        // gets answered the moment there is a name — from the ladder, where the
        // reader is waiting for that name, not from the support list.
        var umbrella = CuratedPage.Composed(
            File.ReadAllText(PagePath("all-brain-tumors")), "/tumors/all-brain-tumors");
        if (SectionBody(umbrella, UmbrellaDischarge.RouteFrom!) is not { } ladder
            || !ladder.Contains($"]({UmbrellaDischarge.Route})", StringComparison.Ordinal))
        {
            failures.Add(
                $"all-brain-tumors: '{UmbrellaDischarge.RouteFrom}' must link to "
                + $"{UmbrellaDischarge.Route} ({UmbrellaDischarge.Why}); it does not. That one route "
                + $"stands in for {UmbrellaRoutesTheseQuestions.Length} of §12.3's questions.");
        }

        // THE COUNT IS FOLDED INTO THE REPORT RATHER THAN ASSERTED UNDER IT
        // (/review round 5, a dead assertion created by round 4's own reorder).
        // Moving the readable report above the numeric canary made that canary
        // unfailable: the only way the checked count can differ from the declared
        // one is a ruling with `Route` and no `RouteFrom`, and that branch already
        // adds to `failures` — so a mismatch always threw one assertion earlier.
        // The reorder was right and safe in the body-floor test, where a missing
        // heading leaves no entry behind; here it nullified the thing it moved.
        var declared = Overrides.Sum(o => o.Value.Count(kv => kv.Value.Route is not null));
        if (perSlugRoutes != declared)
        {
            failures.Add($"only {perSlugRoutes} of {declared} routed rulings were actually checked");
        }

        Assert.True(failures.Count == 0,
            "a routed question must be routed from the section that owes it:\n  "
            + string.Join("\n  ", failures));

        // Still live: deleting every `Route:` in the table makes both sides zero,
        // which the report above cannot see.
        Assert.True(perSlugRoutes > 0, "no per-slug route was checked at all");
    }

    /// <summary>
    /// One link discharging ten §12.3 questions is a large claim, so it is checked
    /// rather than asserted: the umbrella must genuinely NOT answer them under the
    /// standard headings. The day somebody writes "What is a brain tumor?" onto
    /// that page, the routing ruling has stopped being true and this goes red.
    /// </summary>
    [Fact]
    public void TheUmbrellaReallyCannotAnswerTheQuestionsItRoutes()
    {
        var headings = HeadingsOf(File.ReadAllText(PagePath("all-brain-tumors")));
        var byKey = Template.ToDictionary(t => t.Key, t => t.Pattern, StringComparer.Ordinal);

        // THE POSITIVE CONTROL COMES FIRST (/review round 3). Every assertion
        // below is an ABSENCE, so without this the test goes green the day the
        // heading reader stops finding headings — the splitter-with-nothing-to-
        // split-on failure, in a test written to catch its own class of hole.
        Assert.NotEmpty(headings);
        var unmatched = UmbrellaTemplate
            .Where(t => MatchingHeadings(headings, t.Pattern).Count == 0)
            .Select(t => t.Key)
            .ToList();
        Assert.True(unmatched.Count == 0,
            "the instrument cannot see the umbrella's own sections, so the absences below "
            + "prove nothing: " + string.Join(", ", unmatched));

        // EVERY §12.3 QUESTION IS ACCOUNTED FOR — answered under the page's own
        // heading, or routed. This is the check whose absence let question 2 fall
        // out of both lists in round 2, and it is the umbrella's version of the
        // rule the whole item exists to establish: never simply dropped.
        var accounted = UmbrellaRoutesTheseQuestions
            .Concat(UmbrellaAnswersUnderItsOwnHeadings.Keys)
            .ToList();

        Assert.Empty(UmbrellaRoutesTheseQuestions.Intersect(UmbrellaAnswersUnderItsOwnHeadings.Keys));
        Assert.Equal(
            Template.Select(t => t.Key).OrderBy(k => k, StringComparer.Ordinal),
            accounted.OrderBy(k => k, StringComparer.Ordinal));

        // The headings named as answering a question must exist in the umbrella's
        // own template, so the mapping cannot point at nothing.
        var umbrellaKeys = UmbrellaTemplate.Select(t => t.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var answers in UmbrellaAnswersUnderItsOwnHeadings.Values)
        {
            Assert.NotEmpty(answers);
            Assert.All(answers, a => Assert.Contains(a, umbrellaKeys));
        }

        // AND NO HEADING ANSWERS TWO QUESTIONS. Without this the mapping is the
        // laundering vector it was built to close (/review round 4): membership
        // alone is satisfied by ANY heading that exists, so "1  what is it" could
        // be moved out of the routed list and pointed at "U0  the short version"
        // — a heading already spent on question 0 — and the disjointness, the
        // set equality and the membership check would all still pass. Making the
        // arithmetic work by pointing at something that exists is exactly the
        // move §12.15 says an exception may never be.
        var used = UmbrellaAnswersUnderItsOwnHeadings.Values.SelectMany(v => v).ToList();
        Assert.Equal(used.Count, used.Distinct(StringComparer.Ordinal).Count());

        // And the routed ones really are unanswerable here. MatchingHeadings, not
        // IndexOfHeading (/review round 3): that helper returns -1 for TWO matches
        // as well as none, so writing the heading onto the page TWICE would have
        // read as "not answered" and passed — contradicting this test's own name.
        var answered = UmbrellaRoutesTheseQuestions
            .Where(key => MatchingHeadings(headings, byKey[key]).Count > 0)
            .ToList();

        Assert.True(answered.Count == 0,
            "/tumors/all-brain-tumors routes these questions because it cannot answer them, "
            + "and it now answers: " + string.Join(", ", answered)
            + " — so they must move out of the routed list and into the mapping");
    }

    [Fact]
    public void TheOnlyPagesOutsideTheDefaultClassAreTheThreeWithARuling()
    {
        // The tripwire. A new taxonomy entry is a Hub and owes all seventeen;
        // acquiring an exemption means editing this file, deliberately, in a
        // review, with the reason written down. That is the point.
        var slugs = TaxonomySlugs().ToList();

        Assert.Equal(
            ["all-brain-tumors", "brain-metastases", "pediatric-brain-tumor"],
            Classes.Keys.OrderBy(k => k, StringComparer.Ordinal));

        var unknown = Classes.Keys.Where(k => !slugs.Contains(k)).ToList();
        Assert.True(unknown.Count == 0,
            "a ruling names a slug that is no longer in the taxonomy: " + string.Join(", ", unknown));

        Assert.True(Overrides.Keys.All(Classes.ContainsKey),
            "a page has per-question rulings without being given a class");

        // Every ruling names a question the template actually has. A typo used to
        // surface only indirectly, as "the standard heading is missing".
        var templateKeys = Template.Select(t => t.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var (slug, overrides) in Overrides)
        {
            var strays = overrides.Keys.Where(k => !templateKeys.Contains(k)).ToList();
            Assert.True(strays.Count == 0,
                $"{slug}: ruling(s) for question(s) that are not in the template: "
                + string.Join(", ", strays));
        }

        // THE CLASS EARNS ITS NAME: a slug in a non-default class must carry a
        // recorded ruling for the questions that class cannot answer the standard
        // way. Classify a new slug Secondary and this fails until its rulings
        // exist, which is what makes the class inheritable rather than decorative.
        foreach (var (slug, pageClass) in Classes)
        {
            if (!ClassOwesARulingFor.TryGetValue(pageClass, out var owed))
            {
                continue;
            }

            var rulings = Overrides.TryGetValue(slug, out var o) ? o : [];
            var missing = owed.Where(k => !rulings.ContainsKey(k)).ToList();
            Assert.True(missing.Count == 0,
                $"{slug} is classified {pageClass}, which owes a recorded ruling for "
                + string.Join(", ", missing)
                + " — a class is a different obligation, not a waiver");
        }

        // Not vacuous: the default really is the strict one, for the great
        // majority of the corpus.
        //
        // The round-2 fix for the typed `>= 20` floor put an EQUALITY here that
        // could not fail (/review round 3): `unknown.Count == 0` above already
        // proves Classes.Keys is a subset of slugs, and TaxonomyStore throws on a
        // duplicate slug, so `slugs.Count - Classes.Count` and the filtered count
        // were the same number by construction — round 2's own blocker pattern,
        // a derived number compared with itself. Deleted rather than elaborated;
        // the ratio below is the check that can actually fail.
        Assert.True(Classes.Count * 4 <= slugs.Count,
            "the non-default classes have stopped being exceptions");
    }

    [Fact]
    public void TheTemplateThisTestEnforcesStillMatchesTheDesignDoc()
    {
        // The matchers above are hard-coded. This is what stops them drifting
        // away from §12.3 unnoticed: change the doc's shape and this goes red,
        // which sends the author here to update the wording deliberately.
        var doc = File.ReadAllText(Path.Combine(RepoRoot(), "docs", "content-pipeline.md"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);

        var start = doc.IndexOf("\n### 12.3 ", StringComparison.Ordinal);
        Assert.True(start > 0, "content-pipeline.md no longer has a §12.3");
        var end = doc.IndexOf("\n### 12.4 ", start, StringComparison.Ordinal);
        Assert.True(end > start, "content-pipeline.md no longer has a §12.4 to bound §12.3");

        var section = doc[start..end];

        var numbered = Regex.Matches(section, @"^\| (\d+) \|", RegexOptions.Multiline)
            .Select(m => int.Parse(m.Groups[1].Value))
            .ToList();

        Assert.Equal(Enumerable.Range(0, 17), numbered);

        // Seventeen rows, 0..16, and row 16 is the one this test does NOT look
        // for as a heading, because it is rendered from front matter.
        Assert.Contains("| 16 | Where this came from", section, StringComparison.Ordinal);

        // FLATTENED, and this caught itself on the first run. docs/ is
        // hard-wrapped at ~76 columns, so "The self-blame block still appears"
        // is split across a line break and a substring search cannot see it —
        // standing trap #1 (§12.8), the same mechanism that made WI-541's
        // location pass report zero for eight fragments that were certainly
        // present. A test that searches un-flattened doc prose is asserting
        // against the line wrapping, not against the words.
        var flattened = Regex.Replace(section, @"\s+", " ");

        // The template here is the same length by a different route: sixteen
        // heading-bearing rows (0..15) plus the self-blame block, which §12.3
        // mandates in prose rather than in the table.
        Assert.Equal(17, Template.Length);

        // A WORDING CROSS-CHECK, not only a shape one. /review round 2: the
        // comment on Template claimed the matchers "cannot drift away from the
        // spec unnoticed", and the test only counted rows — rename a row and
        // every matcher stayed green.
        //
        // TWELVE OF THE SEVENTEEN ARE POLICED. The first version of this check
        // did six and its comment implied the other eleven were unpolicable
        // (/review round 3); six of them were verbatim-checkable and simply had
        // not been done. The five that genuinely are not: row 1 is parameterised
        // ("What is a [tumor]?"), row 9 lists the everyday-life topics rather
        // than naming the heading, row 12 is "Outlook — behind a reader-choice
        // gate" against the heading "What might happen over time", row 14 is
        // "Questions to ask your team (printable)" against "What to ask your
        // team", and row 16 is rendered from front matter and has no heading at
        // all. Asserting a match that was never intended is how a guard starts
        // failing on a correct document, so those five claim nothing.
        foreach (var (row, token) in new[]
                 {
                     (0, "The short version"),
                     (2, "Is it cancer? What does its grade mean?"),
                     (3, "Where does it grow, and why does it cause these symptoms?"),
                     (4, "What symptoms does it cause? + When should I call for help right now?"),
                     (5, "How do doctors find out it is this?"),
                     (6, "What do the words on my report mean?"),
                     (7, "How is it usually treated?"),
                     (8, "What is treatment actually like, and what is normal afterwards?"),
                     (10, "Follow-up scans, and what to do while you wait"),
                     (11, "If it comes back"),
                     (13, "For the person caring for someone with this"),
                     (15, "Where to get support"),
                 })
        {
            Assert.Contains($"| {row} | {token}", flattened, StringComparison.Ordinal);
        }
        Assert.Contains("The self-blame block still appears", flattened, StringComparison.Ordinal);

        // A positive control BUILT FROM THE FILE, per WI-541, rather than
        // pinned to the phrase above: a phrase that this doc's own wrapping
        // splits must be findable flattened and unfindable raw. That proves the
        // instrument works without going red the day somebody reflows a
        // paragraph, which a hard-coded control would.
        var wrapped = WrappedPhrase(section);
        Assert.NotNull(wrapped);
        // `flattened`, not a fresh Regex.Replace of the same text (/review round 4):
        // re-deriving it made the control true by construction — it could not fail
        // for any change to the document OR to the variable the twelve token
        // assertions above actually read.
        Assert.Contains(wrapped, flattened, StringComparison.Ordinal);
        Assert.DoesNotContain(wrapped, section, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryTaxonomySlugHasExactlyOnePageAndNoPageIsOrphaned()
    {
        // The two directions of the same property. A file with no slug is a page
        // no reader can filter to; a slug with no file is the stub state.
        var slugs = TaxonomySlugs().ToList();
        var files = Directory
            .EnumerateFiles(Path.Combine(RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages", "tumors"), "*.md")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(slugs.OrderBy(s => s, StringComparer.Ordinal), files);
    }

    // ----------------------------------------------------------------- helpers

    /// <summary>
    /// The last word of a hard-wrapped prose line joined to the first word of
    /// the next — a phrase that exists in the text a reader sees and does not
    /// exist as a substring of the file. Returns null if the passage is not
    /// wrapped at all.
    /// </summary>
    private static string? WrappedPhrase(string passage)
    {
        var lines = passage.Split('\n');
        for (var i = 0; i < lines.Length - 1; i++)
        {
            var here = lines[i].Trim();
            var next = lines[i + 1].Trim();
            if (here.Length == 0 || next.Length == 0 || "|#->*".Contains(here[0]) || "|#->*".Contains(next[0]))
            {
                continue;
            }

            var last = here[(here.LastIndexOf(' ') + 1)..];
            var first = next.Split(' ')[0];
            if (last.Length > 2 && first.Length > 2)
            {
                return $"{last} {first}";
            }
        }
        return null;
    }

    /// <summary>
    /// How many `##` headings the corpus owes, from the OBLIGATION TABLE rather
    /// than from any file walk — so a test that walks files can use it as a floor
    /// without comparing a number with itself. A routed question owes no heading,
    /// so it is subtracted.
    /// </summary>
    internal static int HeadingsOwedByTheCorpus => TaxonomySlugs().Sum(slug =>
        RequiredSections(slug).Length
        - (Overrides.TryGetValue(slug, out var o)
            ? o.Count(kv => kv.Value.OwnHeading is null)
            : 0));

    private static (string Key, string Pattern)[] RequiredSections(string slug) =>
        Classes.TryGetValue(slug, out var c) && c == PageClass.Umbrella ? UmbrellaTemplate : Template;

    private static IEnumerable<string> TaxonomySlugs() =>
        new TaxonomyStore(File.ReadAllText(Path.Combine(
                RepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml")))
            .TumorTypes.Select(t => t.Slug);

    private static string PagePath(string slug) => Path.Combine(
        RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages", "tumors", $"{slug}.md");

    /// <summary>
    /// The index of the one heading matching <paramref name="pattern"/>, or -1.
    ///
    /// AMBIGUITY IS A FAILURE, NOT A FIRST MATCH (/review round 1). The section-1
    /// matcher carries a hand-maintained negative lookahead to stop it claiming
    /// section 8's heading; a blacklist has to be extended by hand for every
    /// future "What is …" section and nothing forces that. Raising on a second
    /// match makes the collision announce itself instead of being silently
    /// resolved in favour of whichever heading comes first.
    /// </summary>
    private static int IndexOfHeading(IReadOnlyList<string> headings, string pattern) =>
        MatchingHeadings(headings, pattern) is [var only] ? only : -1;

    /// <summary>
    /// Every heading matching <paramref name="pattern"/>. More than one means the
    /// matcher's meaning depends on heading order, which is not a property a
    /// template should have.
    ///
    /// It RETURNS the ambiguity rather than asserting on it (/review round 2).
    /// The first version threw from inside the per-slug loop, which discarded the
    /// whole accumulated failure list — so one ambiguous heading on page 3 hid
    /// every defect on the other 22, in the loop built precisely to report them
    /// all — and its message named the pattern but not the slug.
    /// </summary>
    private static List<int> MatchingHeadings(IReadOnlyList<string> headings, string pattern) =>
        [.. Enumerable.Range(0, headings.Count).Where(i => Regex.IsMatch(headings[i], pattern))];

    private static string? AmbiguityIn(IReadOnlyList<string> headings, string pattern, string key)
    {
        var matches = MatchingHeadings(headings, pattern);
        return matches.Count <= 1
            ? null
            : $"'{key}' matches {matches.Count} headings ("
              + string.Join(" | ", matches.Select(i => headings[i]))
              + "), so which section it names depends on their order — tighten the matcher";
    }

    /// <summary>
    /// The `## ` headings, in order, with the explicit `{#anchor}` stripped —
    /// WI-509 made anchors the rule precisely so wording and interface move
    /// independently, so a matcher naming the words a reader sees must not be
    /// tied back to the anchor bolted onto them.
    /// </summary>
    /// <summary>
    /// One section's flattened body, or null if the page has no such heading.
    ///
    /// NOT <c>CuratedPage.Section</c>, which asserts on a missing heading
    /// (/review round 5): called from inside a per-slug accumulation loop, that
    /// throw would discard every other slug's findings — the pattern round 2
    /// removed from the sweep and round 3 removed from the body-floor test, left
    /// standing here until a third round found it in a third place.
    /// </summary>
    private static string? SectionBody(string page, string heading) =>
        SectionsOf(page)
            .Where(s => string.Equals(s.Heading, heading, StringComparison.Ordinal))
            .Select(s => s.Body)
            .FirstOrDefault();

    private static IReadOnlyList<string> HeadingsOf(string page) =>
        [.. SectionsOf(page).Select(s => s.Heading)];

    private static IReadOnlyList<(string Heading, string Body)> SectionsOf(string page)
    {
        // LF-normalised first. This repo is core.autocrlf=true, so a splitter
        // written against "\n" finds nothing to split on in a CRLF checkout —
        // and §12.11's rule is that a splitter with nothing to split on does not
        // fail, it stops being a splitter.
        var body = page.Replace("\r\n", "\n", StringComparison.Ordinal);
        var matches = Regex.Matches(body, @"^## (.+?)[ \t]*$", RegexOptions.Multiline);

        var sections = new List<(string, string)>();
        for (var i = 0; i < matches.Count; i++)
        {
            var heading = Regex.Replace(matches[i].Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim();
            var from = matches[i].Index + matches[i].Length;
            var to = i + 1 < matches.Count ? matches[i + 1].Index : body.Length;
            sections.Add((heading, Regex.Replace(body[from..to], @"\s+", " ").Trim()));
        }
        return sections;
    }

    private static string RepoRoot() => CuratedPage.RepoRoot();
}

/// <summary>
/// The same sweep, served by the running site rather than read off disk.
///
/// §12.11 records that six defects in this phase have been composed-page-only:
/// a heading that is present in the source and eaten by composition, a fix that
/// appended instead of replacing, a tooltip firing the wrong definition. A
/// template sweep that only ever reads the file cannot see any of them.
/// </summary>
[Trait("Category", "Database")]
[Collection(DatabaseCollection.Name)]
public sealed class TumorHubTemplateRenderSweepTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TumorHubTemplateRenderSweepTests(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:BrainHarbor", TestDatabase.ConnectionString));

    [Fact]
    public async Task EveryTaxonomySlugServesAPageWhoseSectionsSurviveComposition()
    {
        var client = _factory.CreateClient();
        var taxonomy = new TaxonomyStore(await File.ReadAllTextAsync(Path.Combine(
            RepoRoot(), "src", "BrainHarbor.Web", "Content", "taxonomy.yml")));

        var failures = new List<string>();
        var checkedHeadings = 0;

        // Read once, and proved non-empty before it is used as a ban list.
        var directives = BlockDirectives();
        Assert.NotEmpty(directives);

        foreach (var type in taxonomy.TumorTypes)
        {
            var response = await client.GetAsync($"/tumors/{type.Slug}");
            if (!response.IsSuccessStatusCode)
            {
                failures.Add($"/tumors/{type.Slug} returned {(int)response.StatusCode}");
                continue;
            }

            var html = await response.Content.ReadAsStringAsync();

            // Every heading the source carries must reach the reader. This
            // compares the served page against the page's OWN source rather
            // than against a list typed here, so it cannot go stale — and it
            // catches exactly the failure a disk-only sweep is blind to.
            foreach (var heading in HeadingsOfSource(type.Slug))
            {
                checkedHeadings++;
                if (!html.Contains(System.Net.WebUtility.HtmlEncode(heading), StringComparison.Ordinal)
                    && !html.Contains(heading, StringComparison.Ordinal))
                {
                    failures.Add($"/tumors/{type.Slug}: the source has '## {heading}' and the served page does not");
                }
            }

            // An unresolved directive renders as literal bracket text with no
            // error anywhere — the WI-501 CRLF failure.
            // Derived from the blocks directory, so a ninth block is covered on
            // the day it is written rather than the day somebody remembers to add
            // it here (/review round 1).
            foreach (var directive in directives)
            {
                if (html.Contains(directive, StringComparison.Ordinal))
                {
                    failures.Add($"/tumors/{type.Slug}: the directive {directive} reached the reader unresolved");
                }
            }
        }

        // NOT VACUOUS — and the FIRST attempt at this was worse than the typed
        // floor it replaced (/review round 2, a blocker found inside round 1's
        // fix). It derived the expected count by calling HeadingsOfSource over
        // the same taxonomy the loop had just walked, so it compared a number
        // with itself: if the heading extractor ever stopped finding headings,
        // both sides went to zero and Assert.Equal(0, 0) passed while this test's
        // whole subject evaporated. A canary derived from the walk it is checking
        // proves nothing.
        //
        // The floor now comes from the OBLIGATION TABLE in the sibling class —
        // Template, Overrides and UmbrellaTemplate — which knows nothing about
        // how headings are extracted from a file. Two independent sources.
        var owed = TumorHubTemplateSweepTests.HeadingsOwedByTheCorpus;
        Assert.True(checkedHeadings > 0, "no heading was checked at all");
        Assert.True(checkedHeadings >= owed,
            $"only {checkedHeadings} headings were checked, and the corpus owes at least "
            + $"{owed} — the heading extractor has stopped finding them");
        Assert.True(failures.Count == 0, string.Join("\n  ", failures));
    }

    /// <summary>
    /// The block directives, from the blocks directory the COMPOSER uses.
    ///
    /// /review round 3: the first version hand-built the path, and
    /// <c>ContentBlockStore.Load</c> returns an EMPTY set rather than throwing
    /// when the directory is missing — so a typo or a rename turned the
    /// unresolved-directive check into a loop over nothing, green. Round 1's fix
    /// had traded a stale list for a vacuous one. <c>CuratedPage.BlocksRoot</c>
    /// is the path composition itself uses, and the caller asserts non-empty.
    /// </summary>
    private static string[] BlockDirectives() =>
        [.. ContentBlockStore.Load(CuratedPage.BlocksRoot)
            .Blocks.Keys.Select(k => $"[{k.ToUpperInvariant()}]")];

    private static IEnumerable<string> HeadingsOfSource(string slug)
    {
        var text = File.ReadAllText(Path.Combine(
                RepoRoot(), "src", "BrainHarbor.Web", "Content", "pages", "tumors", $"{slug}.md"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);

        return Regex.Matches(text, @"^## (.+?)[ \t]*$", RegexOptions.Multiline)
            .Select(m => Regex.Replace(m.Groups[1].Value, @"\s*\{#[^}]+\}\s*$", "").Trim());
    }

    private static string RepoRoot() => CuratedPage.RepoRoot();
}
