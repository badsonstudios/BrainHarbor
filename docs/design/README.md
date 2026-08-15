# Design handoffs — which one governs what

Two approved handoffs live here. They do not conflict; the newer one replaces
part of the older one, and this file records exactly which part, so nobody has
to guess which spec is current.

| Folder | Date | Governs |
|---|---|---|
| `entry-hub-handoff/` | 2026-07-19 | The "Clear & Kind" visual language: tokens, header, helpline band, footer, curated pages, trials, search, admin. Still the spec for everything below. |
| `homepage-handoff/` | 2026-08-15 | **The homepage** ("Harbor Banner"), **the research item page**, and **the evidence badge**. Supersedes the entry-hub handoff for those three. |

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

3. **The feed card keeps its photo and its readiness dial.** The handoff
   specifies a plainer card — badge, title, hook, meta — on the grounds that
   nothing should compete with the evidence badge. Dan's call on 2026-08-15,
   after seeing both live: the `/research` card is the better one and the
   homepage should match it. The dial is the reason. The badge says how well
   TESTED a finding is; the dial says how close it is to something a patient can
   actually get, and those are different questions. One card renders on both
   pages, so they cannot drift apart again.

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
