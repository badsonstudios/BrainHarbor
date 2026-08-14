using BrainHarbor.Safety;
using BrainHarbor.Web.Admin;

namespace BrainHarbor.Tests;

/// <summary>
/// WI-418: the queue says WHICH check flagged an item.
///
/// The database records a `summary_flagged` boolean and no reason, so "flagged"
/// meant "read this one closely" across a queue of 137 items and the reviewer
/// re-derived every reason by hand. The checks are pure text analysis and the
/// summary is stored, so the answer is recomputed here — through the pipeline's
/// own library, never a second copy of the rules.
/// </summary>
public class ReviewFlagReasonsTests
{
    private static ReviewItem Item(
        string whatFound = "People went 27 months before the tumor grew.",
        string? rawSummary = "In a trial of 331 people with glioma, survival was 27 months versus 11 months.",
        string sourceKind = "research",
        string? trialPhase = null) => new()
        {
            Id = 1,
            Source = "pubmed",
            SourceKind = sourceKind,
            ExternalId = "x1",
            Title = "A trial of a pill for glioma",
            RawSummary = rawSummary,
            Url = "https://example.org",
            SummaryFlagged = true,
            Status = "pending",
            PlainTitle = "A pill slowed glioma growth",
            PlainSummary = "A daily pill helped people go longer before their tumor grew.",
            PlainWhatStudied = "Researchers gave a daily pill to 331 people with glioma.",
            PlainWhatFound = whatFound,
            PlainMeans = "For some people, the pill may add time before stronger care is needed.",
            PlainDoesntMean = "This is not a promise for everyone, and it does not get rid of the tumor.",
            ReadinessReason = "Being tested in people in trials, but not yet approved.",
            TrialPhase = trialPhase,
        };

    [Fact]
    public void AnInventedNumberIsNamedAsSuch()
    {
        var reasons = Item(whatFound: "The pill worked for 88% of people.").FlagReasons;

        var reason = Assert.Single(reasons);
        Assert.Equal(Guardrails.FlagKind.InventedNumbers, reason.Kind);
        Assert.Contains("88", reason.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HypeIsNamedAsSuch()
    {
        var reasons = Item(whatFound: "This breakthrough helped people live 27 months.").FlagReasons;

        Assert.Contains(Guardrails.FlagKind.BannedHype, reasons.Select(r => r.Kind));
    }

    /// <summary>
    /// The honest empty case, and why the queue says so out loud: a READER can
    /// set the flag, and the reading-level ceiling moved from 8.5 to 7.0 on
    /// 2026-08-13 — so an item flagged then may pass every check today.
    /// Showing an empty box would read as "no reason given" rather than "no
    /// reason found".
    /// </summary>
    [Fact]
    public void ACleanSummaryProducesNoReasonsEvenWhileFlagged()
    {
        var item = Item();

        Assert.True(item.SummaryFlagged);
        Assert.Empty(item.FlagReasons);
    }

    [Fact]
    public void AnItemWithNoSummaryYetHasNothingToCheck()
    {
        // The 20 one-off classify failures in the queue have no summary at all.
        // Running text checks over nothing would report a reading level for the
        // empty string rather than saying "not summarized".
        var item = Item();
        item.PlainSummary = null;

        Assert.False(item.HasSummary);
        Assert.Empty(item.FlagReasons);
    }

    /// <summary>
    /// A trial summary legitimately contains its phase — the prompt scores
    /// readiness BY phase. Without the trial facts in the source text, the
    /// numeral check reports "Phase 2" as invented and sends the reviewer
    /// hunting for a problem that does not exist.
    /// </summary>
    [Fact]
    public void ATrialsPhaseIsNotReportedAsAnInventedNumber()
    {
        // Every number in this summary is the phase, and the phase lives in the
        // trials cache rather than the abstract — so this passes only if the
        // queue joins those facts back in, exactly as the pipeline had them.
        static ReviewItem Trial(string? phase) => new()
        {
            Id = 2,
            Source = "ctgov",
            SourceKind = "trial_update",
            ExternalId = "NCT00000001",
            Title = "A study of a new drug for glioma",
            RawSummary = "This study is testing a new drug in people with glioma.",
            Url = "https://clinicaltrials.gov/study/NCT00000001",
            SummaryFlagged = true,
            Status = "pending",
            PlainTitle = "A trial is testing a new drug",
            PlainSummary = "Doctors are testing a new drug in people with glioma.",
            PlainWhatStudied = "This is a Phase 2 trial.",
            PlainWhatFound = "The trial has not reported results yet.",
            PlainMeans = "People with glioma may be able to join.",
            PlainDoesntMean = "It does not mean the drug works.",
            ReadinessReason = "Still being tested in people.",
            TrialPhase = phase,
        };

        Assert.Contains(
            Guardrails.FlagKind.InventedNumbers,
            Trial(phase: null).FlagReasons.Select(r => r.Kind));

        Assert.DoesNotContain(
            Guardrails.FlagKind.InventedNumbers,
            Trial(phase: "Phase 2").FlagReasons.Select(r => r.Kind));
    }

    /// <summary>
    /// The blocks must be assembled exactly as the pipeline assembles them.
    /// Joining them any other way is the WI-415 defect: a plain title has no
    /// full stop, so running it into the hook inflated every reading grade by
    /// about 0.7 and let a negation in the title excuse a claim in the hook.
    /// </summary>
    [Fact]
    public void TheQueueAssemblesTheBlocksTheSameWayThePipelineDoes()
    {
        var item = Item();

        var fromQueue = new SummaryText(
            item.PlainTitle, item.PlainSummary, item.PlainWhatStudied, item.PlainWhatFound,
            item.PlainMeans, item.PlainDoesntMean, item.ReadinessReason).AllProse;

        var fromPipeline = new BrainHarbor.Pipeline.Summarize.SummarizeOutput
        {
            PlainTitle = item.PlainTitle!,
            Hook = item.PlainSummary!,
            WhatStudied = item.PlainWhatStudied!,
            WhatFound = item.PlainWhatFound!,
            Means = item.PlainMeans!,
            DoesntMean = item.PlainDoesntMean!,
            ReadinessReason = item.ReadinessReason!,
        }.AllProse;

        Assert.Equal(fromPipeline, fromQueue);
    }
}
