# Design handoffs — which one governs what

Two approved handoffs live here. They do not conflict; the newer one replaces
part of the older one, and this file records exactly which part, so nobody has
to guess which spec is current.

| Folder | Date | Governs |
|---|---|---|
| `entry-hub-handoff/` | 2026-07-19 | The "Clear & Kind" visual language: tokens, header, helpline band, footer, curated pages, trials, search, admin. Still the spec for everything below. |
| `homepage-handoff/` | 2026-08-15 | **The homepage** ("Harbor Banner") and **the research item page**. Superseded for the evidence indicator by the journey handoff below. |
| `journey-handoff/` | 2026-08-19 | **The evidence indicator.** The journey path and the stage-note strip replace BOTH the evidence badge and the 1-to-10 readiness dial, on cards and on the item page. |

## What changed on 2026-08-15, and why

- **The homepage.** A hero paragraph, three doors and a full-width AI panel
  stacked up and pushed the first real update about a screen and a half down.
  Now: one hero band, two doors, then updates. Three doors became two —
  "browse all research" was cut because the feed underneath already does that
  job. The crisis door stays.
- **The evidence badge.** Four marks, not five, on a ladder with no gaps
  (the old one ran 5, 4, 2, 1 with nothing at 3). Ten steps — the readiness
  dial it replaces on the feed — is a finer distinction than anyone can feel,
  and a fraction invites "my treatment is 60% done".
- **The AI notice** moved to the foot of the page so it stops standing between
  the reader and the content.

## Two deliberate deviations from the homepage handoff

Both are safety copy, and both were kept on purpose. Change them knowingly.

1. **The AI admission leads in the hero band.** The handoff has no hero copy at
   all. But a reader must not be able to get through eight summaries before
   learning who wrote them, so one plain line leads: *"Scientists do the
   research. AI puts what they found into plain words. AI can make mistakes, so
   we always link to the study itself."* The fuller notice still closes the page.
2. **"A person does not check every one" survives.** The handoff's notice says
   checks run before publishing, which does not tell a reader that no person
   read theirs. Publishing mode is Auto; the site says so plainly.

Both are pinned by tests in `HomeFeedTests`. The test comments say the same
thing: rewording is fine, dropping them is not.

3. **The feed card keeps its photo.** The handoff specifies a plainer card —
   indicator, title, hook, meta — on the grounds that nothing should compete
   with the evidence indicator. Dan's call on 2026-08-15, and again on
   2026-08-19: keep the photo. One card renders on both pages, so they cannot
   drift apart again.

   *(This item used to read "keeps its photo and its readiness dial". The dial
   is gone — see the journey handoff below. The photo survived both rounds.)*

## What changed on 2026-08-19 — the journey handoff

**The evidence indicator is now one thing, not two.** Cards and the item page
used to carry a 4-mark badge ("how well tested") AND a 1-to-10 readiness dial
("how close to a patient"). Both are replaced by the **journey path**: four
named stages — Lab cells → Animals → Review → Tested in people — with a filled
dot for each stage reached and a larger ringed dot on the current one.

The reasoning, which is worth keeping because it is the whole argument for the
component:

- **A 10-point scale has no plain-language meaning at any single value.** Nobody
  can say what 7 out of 10 is. Every rung on the path has a name a patient can
  repeat to their doctor.
- **A percentage would have been worse.** "60% ready" reads as progress toward a
  finish line *on a schedule*, and 100% reads as *cure available now*. Most lab
  findings never reach people. A position on a road implies no timetable.
- **Two indicators in different units was a puzzle, not an answer.** A reader had
  to reconcile "4 of 4 marks" with "7 of 10" and work out that they measured
  different things.

**Items that are not findings get no path.** Trials, news and preprints use the
`.stage-note` strip instead — a path would imply they sit somewhere on the
evidence scale, and they don't.

### Deviations from the journey handoff, both deliberate

1. **The path is laid OVER the card photo, not stacked under it.** Dan's call:
   it inherits the dial's position and prominence, because it is the only
   element that meaningfully differs between cards. It gets a near-opaque dark
   plate — the same job the dial's dark disc did. A photo varies per card, so
   nothing on top of one can be trusted for contrast without its own surface.
   Measured white-on-plate: 11.8:1 over a white photo, 15.1:1 over a black one.
2. **`role="presentation"` on the `<li>` elements.** The handoff's markup puts
   `role="img"` on the `<ol>`, which overrides its implicit `list` role and
   leaves every `<li>` an orphaned `listitem`. axe-core flags it **serious**, and
   this component renders on every card on every page — so the markup as written
   would have shipped that site-wide. **Tell the designer.**

The stage badge survives for the **admin review queue and the dev style guide
only**: a compact pill triages better than four labelled stages in a dense list.
Do not "finish the job" by deleting it.

### The readiness score is not deleted

`readiness_score` stays in the database, the pipeline, the sync contract and the
review queue. Only what readers see changed, so this is reversible without a
data migration. Two tests exist to stop the number reappearing on a reader page
by accident: `TheHeroCarriesNoReadinessNumber` and
`NoReaderFacingPageShowsTheOneToTenReadinessScore`.

The `/research` "Most ready to use" sort became **"Furthest along"** and now
ranks by the same four stages the cards draw. The URL key stays `?sort=readiness`
so shared links keep working.

## Not yet implemented

The **research item page** restyle from `homepage-handoff/research-item.html`
(the "What this means, and doesn't mean" block, provenance styling). Filed as
WI-428. The badge change already reaches that page, so it is consistent, just
not restyled.

## The brand kit

`homepage-handoff/` ships a `brand/` folder identical to the one already in
`wwwroot/img/brand/` (WI-419) — same files, byte for byte. It is not copied in
here twice; the live assets are the ones under `wwwroot`, and the kit's spec is
at `entry-hub-handoff/brand/README.md`.
