# BrainHarbor — Progress

> **The live state of the project.** Read this first in every session (via
> `/startup`). Updated the moment an item starts, finishes, or hits a blocker —
> a fresh session must be able to resume from this file alone.
> The item definitions live in [docs/backlog.md](docs/backlog.md).

## Current state

| | |
|---|---|
| **Phase** | M3 — Claude classification + plain-language summaries (M0–M2 complete & merged) |
| **Phase** | **M3 MERGED to `main`** (PR #5, 2026-07-31). Next: **M4 — Azure + trials + digest → v1 launch.** |
| **In progress** | nothing mid-flight. WI-411 done 2026-08-11 (PR pending). |
| **Next up** | **WI-401 `[user]`+assisted** (Azure provisioning — Dan says ready, 2026-08-11) is the gate for WI-404–408. Also buildable anytime: **WI-412** (/tumors plain-English tumor-type descriptions, Dan's ask 2026-08-11). |
| **Blockers** | none. WI-401, WI-404 (ESP), WI-408 (soft launch) need Dan's hands (accounts, DNS, money). |

**Publishing mode: AUTO, fully automatic.** Site publishes summaries that pass the automated safety checks; **no human-review claims anywhere in reader-facing copy** (deliberate — scrubbed 2026-07-31). The review queue still exists in code for flagged/reported items but is never promised to readers. Default model claude-opus-5.

**Feed card imagery (done 2026-08-01, on `main`).** Feed cards show a content-matched **photo backdrop** (faded ~20%) with the item's **readiness score as a dial** floating on top; feed is **2-up**. Images are a small human-vetted Unsplash pool in `wwwroot/img/cards/` (grouped brain/genetics/lab/data/abstract); `CardImages` picks by matching the post's words + stage to a theme — **no AI image generation**. Raw originals git-ignored; see `images/image-tags.yml` + `wwwroot/img/cards/IMAGE-CREDITS.md`. Also fixed a real **Windows pipeline bug** (claude .cmd shim needs cmd.exe) and **guardrail false-positives** (cure negation now sentence-scoped; prompt v3 forbids computed numbers) — found running the pipeline live locally.

**Local run:** the whole system runs on the PC (no Azure needed) — see `docs/run-local.md`. Dev DB holds demo items from live pipeline runs. The two `FeedTests` that used to fail locally against that data (UndatedItemsSortLastNotFirst, EarlyStageAppearsOnlyWhenTheReaderAsksForIt) were fixed in WI-402: they now page until they find their own rows instead of assuming an empty table, so the suite is green on a dirty DB and on a fresh one. `A11ySmokeTests` intermittently failed to start its Kestrel host ("The server has not been started"). WI-403 serialized `KestrelWebApplicationFactory.EnsureServer` (CreateClient is not thread-safe) and wrapped the real cause in a message that names it, so a recurrence is diagnosable instead of mute. Not proven fixed — it was never reproducible on demand.

### Found 2026-08-01 while Dan was testing the local site
- **WI-409 home page leads with the feed** — ⚠ **pre-launch blocker**. Home
  still reads "The daily research feed and the weekly digest are coming soon"
  and renders no items at all, but `/research` went live back in WI-209. Half
  that sentence is false, and PLAN.md §3 says the feed IS the front door, not a
  brochure. Small fix; it is also the first thing a stranger sees.
- **WI-410 sort the research feed** — date / readiness / type. Dan's ask: let a
  reader sort by how close something is to helping them, not just by date.
  (The digest half of that home-page sentence is TRUE — WI-404/405 are not
  built, there is no ESP account, and `/digest` honestly says sign-up opens
  soon.)

### Next up — everything left in M4 needs Dan
- **WI-401 Provision Azure** `[user]`+assisted — App Service + Postgres, DNS, TLS, prod secrets. **This is the gate.** WI-404/405 (digest, needs an ESP account), WI-406 (maintenance run) and WI-407 (pre-launch hardening) all depend on it, and WI-408 is the soft launch.
- Nothing left is buildable without cloud — **WI-401 is the gate.**
- Tiny polish backlog: `data` image theme matches 0 items (widen keywords or reassign slot).

### M3 shipped (all on `auto/M3`, PR #5)
- **WI-301–304** golden set, CLIwrapper, classify, summarize+guardrails (numeral/banned/reading-level); connection-pool infra fix.
- **Readiness score (1–10)** — Dan's ask: how close a finding is to everyday care, stage-capped, shown on item pages + queue.
- **WI-305** review queue v1 (side-by-side, inline edit, keyboard, readiness badge).
- **WI-306** item permalink pages (6 blocks, readiness, glossary tooltips, provenance, report-a-problem → admin queue).
- **WI-307** feed patient-first with persisted early-stage toggle.
- **WI-308** SEO (sitemap/robots/RSS, OG + JSON-LD) + honest `/how-we-write` rewrite (was falsely claiming mandatory human review).
- **WI-309** site search (Postgres FTS over items + static pages).
- Prompt/style: no em dashes or AI tells; summaries validated live with Opus.
- 513 tests green.

**Readiness score (Dan's ask, built 2026-07-30):** every summary now carries a
1-10 "how close is this to something a patient can get?" score + one plain
reason (`summarize-v2`). Two-layer safety: Opus proposes within a rubric, then
`Readiness.Clamp` caps by research stage (animal/cell→2, obs→5, review→6,
trial→8, news→10; only ever lowers), and `SyncRepository` re-clamps at the
API/DB boundary as a backstop. Migration 0005. Live-validated (mouse study→2,
observational→4, honest reasons). Not yet rendered on a page — the badge lands
with WI-306. Scale is documented in `docs/content-pipeline.md` §9.

## Notes for the next session

- **Approved visual design lives at `docs/design/entry-hub-handoff/`** ("Clear
  & Kind" theme + Entry Hub home, from Claude Design 2026-07-19). It is the
  visual spec for WI-108/WI-109 and restyles later feed/item work (WI-209,
  WI-306). M1 order changed: **WI-108 before WI-102** so the axe/Playwright
  smoke test runs against the final theme. Handoff URL names that differ from
  sitemap.md (/get-help, /start-here) do NOT override the sitemap
  (/get-help-now, /start). The handoff folder is not yet committed — it goes
  in with WI-108's branch.
- **Remaining dead links**: only `/digest` (M4, needs an ESP → WI-404).
  `/research` went live in WI-209, `/trials` in WI-403.
  `/get-help-now`, `/digest`, `/glossary`, `/about`, `/how-we-write`,
  `/start`, `/privacy`, `/terms` are all live, and a `ShellPagesTests` link
  check fails the build if any *other* internal link 404s. Custom 404/500
  pages exist (WI-103), so the dead nav targets degrade gracefully.
- **M0 fully closed 2026-07-19**: PR #1 squash-merged to `main` (ce5929d),
  `auto/M0` deleted; brainharbor.org purchased (WI-001); `.env` populated;
  NCBI + SYNC keys in user-secrets. No open follow-ups.
- Planning and design are **done** — `PLAN.md` + `docs/*.md` are the spec,
  `docs/backlog.md` is the itemized plan (M0–M4; P2a–P3 not yet decomposed).
- Solution note: SDK 10 generated `BrainHarbor.slnx` (new XML solution
  format) rather than `.sln`; `dotnet build/test` handle it fine.
- Next: `/next-item` for WI-101, or `/autopilot M1`.

## Log (newest first)

- **2026-08-11** — **WI-411 done — dedicated test database** (/next-item).
  DB tests now default to **`brainharbor_test`** in the same local/CI Postgres
  server — one word changed in `TestDatabase.cs`; DbUp's EnsureDatabase
  creates it on first run, so no compose/CI changes. Verified live: suite run
  created it (650/650), second run idempotent, dev-DB row counts identical
  before/after; dev DB had zero leftover test rows. The dirty-database rule
  (far-future seeds, page-until-found) now has ONE canonical home on
  `DatabaseFixture`'s doc comment; per-class comments point there. Dates kept
  as insurance. Deferred (review nit): the fixture guard accepts a REMOTE
  db named *test* — tighten when WI-401 makes remote DBs real.

- **2026-08-11** — **WI-410 done — sort the research feed** (/next-item).
  /research sortable by date (default, unchanged), readiness (highest first,
  **unscored last** — nullable score, the published_at NULLS LAST trap), and
  kind (research → news → trials → preprint grouping, newest first within a
  group, decided explicitly in SQL). Plain select in the existing GET filter
  form (no-JS + htmx for free), composes with tumor/early filters, canonical
  `?sort=` in the URL (garbage normalized away; input never reaches SQL —
  a whitelist switch picks among fixed ORDER BY strings). Review's two copy
  catches applied: "Closest to helping you" was a personal promise the
  anti-hype rules forbid → "Most ready to use"; "By type" → "By kind" (one
  word per concept). **/trials does not get the control** (no readiness score
  there; update recency already meaningful). Live-verified descending dials.
  650 tests.

- **2026-08-10** — **WI-409 done — home page leads with the feed** (/next-item,
  PR [#8](https://github.com/badsonstudios/BrainHarbor/pull/8) squash-merged).
  Home now renders the newest 4 published items ("Latest updates", same
  `_FeedCard` partial and safety rules as /research: published-only, closed
  trials excluded, early-stage hidden unless the reader's persisted WI-307
  cookie opts in — parse shared via `Research.IndexModel.ReadEarlyChoice` so
  the pages can't drift). Cards sit BELOW the three doors (deviation from the
  backlog's "above", approved: the crisis-help door must not scroll away).
  The false "research feed … coming soon" sentence now refers only to the
  digest, and a test fails if home ever claims the feed is coming while
  published items exist. Section omitted entirely at zero published items.
  Also fixed 4 **pre-existing** TrialsPageTests failures on `main` (Dan's 8/1
  live near-me testing put 20+ real trials in trials_cache and the 7/20-dated
  seeds fell off browse page 1) with far-future seed dates, and filed
  **WI-411** (dedicated test DB) so that idiom stops spreading. 640 tests.

- **2026-08-01** — **WI-403 done — the trial finder; M4 autopilot run ENDS
  here** (everything left needs Azure). `/trials` browse over `trials_cache`
  with tumor-type and phase filters, and `/trials/{nct-id}` pages.
  **Near me is a live, keyless `filter.geo` query to ClinicalTrials.gov at
  request time** (architecture.md §7), from either a typed ZIP or browser
  geolocation. The ZIP form is the PRIMARY path and geolocation is progressive
  enhancement on top: this audience should not have to grant a permission
  prompt (or run JavaScript) to find a trial. ZIP → point uses the Census ZCTA
  gazetteer shipped as a file (33,791 rows, public domain); the ZIP is used for
  the outgoing query only, never stored or logged. The live call **fails soft**
  by design — a slow registry degrades to "we could not reach ClinicalTrials.gov
  just now, here is the browse list", never an error page.
  Tumor-type filtering matches the registry's own condition strings against the
  taxonomy's labels and aliases (walking the tree, so "glioma" finds
  glioblastoma), because `trials_cache` holds trials that were never classified
  and so have no tumor_tags — that was the open question WI-402 left.
  Two safety rules pinned by tests: **only a PUBLISHED item may lend its
  plain-language text to a trial page** (the join must not become a side door
  around the review gate), and the registry's own words are always labelled as
  the registry's, never as our plain-language writing. Attribution + link back
  on every trial page (PLAN.md §5 licence requirement).
  Live-verified: 25 open brain-tumor trials within 50 miles of Columbus, 14 for
  glioblastoma, nearest sites resolved correctly. `/trials` added to the axe
  scan and to sitemap.xml.
  **Review caught four blockers, three of them the same shape as WI-402's:**
  (1) the live call did NOT fail soft on the case it was built for —
  `HttpClient.Timeout` throws `TaskCanceledException`, which IS an
  `OperationCanceledException`, so the exception filter meant to let real
  cancellation through was letting the 8-second timeout through too, giving the
  reader a 500. (2) an unknown status was rendered as "this trial is not taking
  new patients" — a fabricated claim directly above a sentence admitting we
  cannot tell (the exact rule `FeedRow.TrialHasClosed` exists to enforce; now
  three states, not two). (3) the outgoing near-me URL contains the reader's
  coordinates, and `IHttpClientFactory` logs request URIs at Information — so
  every search wrote a location to the logs while the page promised "we do not
  store it" (`RemoveAllLoggers`, plus `no-store`/`no-referrer` on those
  responses, and `/privacy` now says plainly what happens to a ZIP).
  (4) near-me searches the WHOLE registry but linked to `/trials/{id}`, which
  404s for anything outside our fetch window — on a fresh database nearly every
  result. Those rows now link to the registry.
  Also: registry count instead of our page size in the heading, closed trials
  don't show their frozen hook, deep `?page=` clamped, unknown-status trials no
  longer vanish from browse, the tumor menu drops slugs that match nothing, and
  near-me/browse now share one definition of a tumor type (with the label
  quoted — "DIPG (pontine)" carries live Essie grouping characters).
  635 tests.

- **2026-08-01** — **WI-402 done — trials fetcher** (autopilot M4):
  ClinicalTrials.gov v2 fetcher, `trials_cache` (migration 0007), a
  `trial_update` feed item for trials someone can still join, and a
  trial-specific summarization prompt. Live-verified against the real registry
  (80 trials in a 5-day window: all mapped, 79 with site coordinates, correct
  50/30 open-vs-closed split, cursor advanced clean).
  **The design changed twice under review, both times for the same reason —
  a trial's FACTS and its plain-language text obey opposite rules:**
  (1) the first cut wrote `plain_summary` into `trials_cache` through the
  unfrozen facts path, which would have carried summaries the safety checks
  FLAGGED, or a human REJECTED, to readers anyway. The cache now holds no
  plain-language column at all; editorial text lives only on
  `aggregated_items`, and facts move through their own `POST /api/sync/trials`.
  (2) refreshing the cache fixed browse but not the pages a reader lands on —
  a published trial page, its feed card, its search snippet and its RSS entry
  all kept saying "now enrolling" forever, because a known trial is never
  re-summarized. The item page now reads status live from the cache; closed
  trials leave the feed, search snippets and RSS but keep their permalink,
  which says plainly that they are not taking new patients.
  Also: facts upload BEFORE classification (an off-topic verdict no longer
  swallows a status change), fact-only trials create no review-queue rows, a
  known trial costs no LLM call, the truncation guard can no longer walk the
  cursor backwards, 5xx/network failures retry (a 400 does not), and three real
  trials were added to the golden set — a new versioned prompt was otherwise
  ungated. Stripped real investigator names, phones and emails from the
  recorded fixtures before committing (public repo). 590 tests.

- **2026-07-31** — **Autopilot M4 started** (branch `auto/M4`). WI-401 (Azure)
  is `[user]` + real money, so it is skipped; WI-402/403 (trials) are pure code
  and buildable without cloud. WI-404/405/406/407/408 all depend on WI-401, so
  the run stops after the trials feature.

- **2026-07-30** — **WI-303 golden-set accuracy run — DONE (by the assistant)**:
  the local `claude` CLI is invocable here, so ran the classify-v1 prompt
  against all 20 ratified golden items. **Stage 20/20 (100%), relevance 18/20
  (90%), primary-tag 18/20 (90%)**, exact-tag 13/20 (65%). The 2 relevance
  misses are borderline "excluded" reviews the model kept (safe direction);
  tag misses are completeness, not wrong tags. Note: `claude -p` used
  **Haiku 4.5** — consider a stronger model for classify/summarize before Auto
  mode. Recorded in the golden-set README. WI-303 acceptance now fully met.

- **2026-07-20** — **WI-302 done** (autopilot M3): Claude Code CLI wrapper.
  Invokes `claude -p --output-format json` (prompt on stdin), unwraps the JSON
  envelope, parses the model's JSON into the expected shape, validates, and
  retries ONCE on garbled output — failing fast on timeouts, auth-style exit
  codes, and validation (deterministic). A bad call NEVER returns a value
  (never a guess). Versioned PromptTemplate with a strict placeholder guard.
  Review caught a blocker (spawn failure threw instead of failing safe) +
  process-handling fixes (bounded stdin write, full stdout drain, kill on
  cancellation) — all fixed; real-runner "CLI not installed" test added. 410.
- **2026-07-20** — **Autopilot M3 started** (branch `auto/M3`, PR #5). Repo
  made public → scrubbed a personal email (NCBI contact → role address) and
  added a PII/secrets scan to the commit-push-pr + autopilot skills. Dan's
  call on the M3 quality gate: **build in Review mode** — full capability, but
  real AI summaries wait in the queue for Dan to judge before auto-publish;
  he flips Publishing:Mode=Auto when confident. Golden set ships as a DRAFT
  for Dan to ratify.
- **2026-07-20** — **WI-301 done** (autopilot M3): golden set — 20 real PubMed
  abstracts hand-classified (11 patient_relevant, 5 early_stage, 4 excluded)
  with ideal 6-block summaries for 10, numbers verbatim from source. Rubric +
  validation tests (real taxonomy slugs, documented vocab, complete
  summaries, every case has a rationale). Flagged a taxonomy gap
  (spinal-cord tumors) for later. DRAFT pending Dan's ratification. 393/393.
- **2026-07-20** — **WI-212 done — auto-publish mode (Dan's request)**: the
  human review gate is now **optional**. `Publishing:Mode` config, **Auto by
  default**: a summarized item that passes the automated safety checks
  publishes itself (slug generated, `review_events` row with actor `auto`);
  flagged or not-yet-summarized items stay pending for a person; Review mode
  restores mandatory review. The item page is **honest** — auto-published
  items say "written by AI and published automatically… a person did not
  review it," not "reviewed by a person." Chose "hold only the flagged ones"
  (Dan's pick) so the automated guardrails (numeral post-check, banned-phrase
  scan, reading level — all M3/WI-304) gate every auto-publish. **Safe-by-
  construction until M3**: no summarizer yet → nothing has a summary → nothing
  auto-publishes, even though the mode is on. Design docs (PLAN,
  content-pipeline §"Publish mode", data-model, architecture, both CLAUDE.md)
  updated — human review is a mode now, not a hard requirement. 384/384.
  (On `auto/M2`, extends PR #4.)

- **2026-07-20** — **WI-211 done — M2 COMPLETE**: live shakedown against the
  real PubMed, NCI, ScienceDaily, medRxiv and bioRxiv endpoints, from an
  empty database. The loop works: 5/5 sources → 1,360 items pending → approve
  one → exactly 1 visible on /research, 1,359 still behind the gate; a second
  run ingested **0 duplicates** and left the published item published (the
  WI-202 human-decision fix proven for real).
  **Three real bugs only a live run could find, all fixed:**
  (1) the pre-filter's keep-bias is right for sources that already selected
  for us, but bioRxiv/medRxiv return every field — it passed **91%** of the
  firehose (protein folding, chondrogenesis) into the review queue. Added a
  SourceScope so firehose sources require a POSITIVE brain-tumor match:
  bioRxiv 2863→77, medRxiv 784→11, total 4871→1360.
  (2) the feed filtered to relevance='patient_relevant', but nothing is
  classified until M3 — so approving an item in M2 did nothing visible.
  Unclassified-but-approved items are now shown; early-stage stays behind the
  toggle. (3) ScienceDaily stamps dates "EDT", which .NET won't parse — all
  48 items were undated, sorting to the bottom of the feed forever and never
  advancing the cursor. Now 0 undated of 48.
  Also hardened the feed ordering tests to stop assuming an empty table —
  the suite now passes *with* 1,360 real rows present. 374/374.

- **2026-07-19** — **WI-210 done** (autopilot M2): source health + the
  scheduled task. Added POST /api/sync/failure so a broken source actually
  writes last_error — until now nothing ever did, so a source that died a
  week ago would still show its last success. /admin/health lists every
  source with plain-language staleness ("5 days ago", "never"), flags
  failures first, and calls out any expected source that has never reported
  at all. The pipeline reports its own failures (best-effort — reporting must
  not break the run) and raises a desktop toast on finish. Task Scheduler
  registration script uses StartWhenAvailable so a sleeping PC catches up
  rather than skipping the day. 360/360.

- **2026-07-19** — **WI-209 done** (autopilot M2): the public feed. Two
  safety rules are enforced in the repository rather than a view — only
  status='published' is ever visible, and early-stage animal/cell work is
  hidden unless the reader ticks the box (a mouse-study headline reads as
  false hope). Tumor filters walk the taxonomy tree, so browsing "glioma"
  surfaces glioblastoma; filter values are normalized against a fixed set and
  never concatenated into SQL. Item permalinks render the badge with a
  plain-language explanation of what it means, and refuse to invent a summary
  when there isn't one. A pulled item's permalink 404s exactly like one that
  never existed. Tests pin that raw source text never reaches a public page.
  /research is now live — only /trials remains dead. 348/348.

- **2026-07-19** — **WI-208 done** (autopilot M2): the review queue — the
  human gate itself. Pending items with the badge a READER would see (same
  mapper the public feed uses, so the decision is made on what actually
  publishes), source text behind a details toggle for comparison, htmx
  approve/reject with a no-JS form fallback. Every transition writes an
  append-only review_events row (who, what, when, note) because "every
  published summary is human-reviewed" needs to be auditable, not assumed.
  Status transitions are guarded, so two open tabs can't double-apply, and
  slugs are generated from the plain-language title on approval with
  collision handling. Flagged items sort first. 22 new tests. 330/330.

- **2026-07-19** — **WI-207 done** (autopilot M2): admin auth — ASP.NET
  Identity (the only EF Core usage; its tables live in an `identity` schema so
  DbUp and EF never collide), ONE account seeded from config with no
  registration or password-reset endpoint, TOTP 2FA enrolment (manual key, no
  JS/QR dependency), hard lockout, anti-forgery on every admin POST, POST-only
  logout. Folder-level authorization means a new admin page is protected by
  default rather than by remembering an attribute. 12 boundary tests. Note:
  the seeder logs loudly and continues if the password is rejected — a weak
  config value must not silently leave the review queue unreachable. 308/308.

- **2026-07-19** — **WI-205 + WI-206 done** (autopilot M2): NCI + ScienceDaily
  RSS fetchers with per-source licensing enforced in the type system
  (FeedTextPolicy; ScienceDaily is headline+teaser+link only, and the enum
  now fails closed), and medRxiv/bioRxiv preprints with source_kind forced to
  "preprint" at all three layers. Review probed the LIVE APIs and found two
  silent breakages: the NCI feed URL 404d (would have failed every run
  forever — corrected to the publishedcontent path, verified 200/10 items),
  and the preprint API pages at 30 not 100, so the fetcher read 30 of ~745
  records and then advanced the cursor past the rest. Paging now follows the
  API's own total and a truncated window advances only to the newest record
  actually read. Also: relevance is judged on the FULL description before the
  licence truncates it (the teaser cut was dropping breast-cancer items whose
  brain-metastases mention fell past the cut), empty feeds warn instead of
  looking healthy, and PubMedPreFilter is renamed BrainTumorPreFilter now
  that three sources share it. 296/296.

- **2026-07-19** — **WI-204 done** (autopilot M2): PubMed fetcher (paged
  esearch + efetch XML, self-healing reldate window, NCBI throttling and key)
  and the hard-rule pre-filter. **The pre-filter decides what patients never
  see, and it was silently dropping real research** — found across my own
  tests and review: a trailing `\b` meant prefixes never matched plurals (so
  "brain metasta" missed "brain metastases" and breast-cancer brain-mets
  research vanished); multi-word terms assumed a literal space (missed
  "brain-tumor", "tumor-treating fields"); the notice rule ate ordinary
  titles starting "Response to"/"Withdrawal"/"Correction of"; the keep list
  lacked the words the audience uses ("brain mets", "CNS involvement",
  "leptomeningeal", and *"brain cancer"* itself); and broad neurology rules
  dropped late-effects research (stroke after cranial irradiation, dementia
  after whole-brain radiotherapy). All 15 titles are now regression tests.
  Also: pagination with **cursor held back** when a window is truncated
  (otherwise the remainder is invisible forever), esearch errors throw rather
  than burning the window, ArticleDate preferred for ahead-of-print,
  OtherAbstract excluded, and the NCBI key no longer lands in logs. 269/269.
- **2026-07-19** — **WI-203 done** (autopilot M2): Pipeline host (user-secrets
  config + validation, structured console logging, distinct exit codes for
  Task Scheduler, retry/backoff), typed sync client (chunking, cursor only on
  the last chunk, actionable auth errors, never logs the key), ISourceFetcher
  abstraction, and a runner with per-source isolation. Review caught a
  **blocker**: `Enumerable.Chunk` yields no chunks for an empty list, so the
  "advance the cursor when nothing is new" call made no HTTP request at all —
  a source's window could never move forward and would refetch a
  forever-growing range. My unit test had passed because it asserted the
  *stub's* recorded call, not the real client (mock hiding the bug). Added a
  real `/api/sync/cursor` endpoint + real-server tests. Also: AlwaysUpload so
  ClinicalTrials.gov updates aren't dropped by the new-only filter, full
  contract round-trip test, unknown-arg rejection, args no longer bind config
  (would put the key in the process list). 203/203.
- **2026-07-19** — **WI-202 done** (autopilot M2): sync API (state/check/items)
  with API-key auth (constant-time, fails CLOSED → 503 if unconfigured),
  key-partitioned rate limiting, per-item validation, and an idempotent
  upsert. Two real bugs found before commit: (1) API 401s were being
  re-executed into the HTML status page — machine clients got markup, and a
  POST re-execute degraded to a bogus 400; (2) **review blocker** — a
  classify-only rerun could null the plain_summary of an already-*published*
  item, leaving a live patient page contentless with no human involved.
  Content is now frozen once a human reviews it (`Frozen` count in the
  response). Also: cursor no longer advances on all-rejected batches (would
  skip that window forever), single-source cursor rule, field bounds +
  source whitelist, null-body 400s, DateOnly Dapper handler. 181/181.
- **2026-07-19** — **WI-201 done** (autopilot M2): aggregated_items +
  source_sync_state migration with CHECK constraints (incl. preprint can
  never be patient_relevant, enforced in the DB); taxonomy.yml as a **tree**
  (22 types, parent/child) + TaxonomyStore with alias resolution, Matches()
  ancestor walk, and a FilterTags gate that reports rejected tags.
  Review caught two **medically wrong aliases** — "grade 4 glioma" mapped to
  glioblastoma (WHO CNS5 grade 4 also covers IDH-mutant astrocytoma and H3
  K27-altered DMG) and DIPG treated as a synonym for diffuse midline glioma
  rather than its pontine subset. Both would have shown patients research
  about a different disease. Also fixed the DbUp journal race **in prod**
  (advisory lock, not just serialized tests) and the NULLS LAST feed index.
  data-model.md updated to match. 147/147.
- **2026-07-19** — **M1 MERGED**: Dan ran the site, visual review passed
  ("everything is looking good"); running it surfaced one real gap — no
  shipped page used a glossary term, so the tooltip was invisible; fixed with
  a real-pipeline sample on /dev/styleguide. PR #3 squash-merged to `main`
  (0f6be65), `auto/M1` deleted. Autopilot M2 starting.
- **2026-07-19** — **M1 COMPLETE** (autopilot): all 8 items shipped on
  `auto/M1`, 112/112 tests, ContentCheck clean, 0 build warnings. Awaiting
  Dan's visual review + merge of PR #3. Nothing merged to `main` by autopilot.
- **2026-07-19** — **WI-107 done** (autopilot): six curated shell pages
  (/about, /how-we-write, /start, /digest, /privacy, /terms) at reading
  grades 2.5–4.8; disclaimer partial rendering from front-matter flags;
  scaffolded Razor Privacy page deleted so /privacy is curated content.
  Review caught 3 blockers in the COPY, all fixed: /start had no emergency
  red flags before its calming copy (now leads with 911 signs), three "what
  to do next" CTAs pointed at the unbuilt /research, and a typo'd disclaimer
  flag rendered an empty box instead of the medical disclaimer (now a
  ContentCheck failure). Privacy/how-we-write claims trimmed to what the
  code actually does today.
- **2026-07-19** — **WI-106 done** (autopilot): tools/BrainHarbor.ContentCheck
  — Flesch-Kincaid gate (fail >8.5, warn ≥7.5) with block-aware sentence
  extraction (headings/bullets don't inflate the grade — review measured
  +1.6 grades before the fix), medical-hiatus syllable rule, front-matter
  validation, overdue review_due + missing-source warnings, 40-word glossary
  limit, loud warning on missing roots; CI step added (runs on all content —
  intentionally stronger than changed-only). 22 new tests. 98/98.
- **2026-07-19** — **WI-105 done** (autopilot): GlossaryStore (term files per
  content-pipeline §6, snapshot reloads), GlossaryMarker Markdig extension
  (first occurrence per page → native-popover button tooltip, WCAG 1.4.13;
  paragraphs only; %%term%% + !%term% escapes), /glossary A–Z, 3 seed terms.
  Review caught 2 real bugs pre-commit: terms split across source line wraps
  never matched (soft-break merge added) and "non-IDH-mutant" got a wrong
  tooltip (hyphen-aware boundaries) — both pinned with tests. 76/76.
- **2026-07-19** — **WI-104 done** (autopilot): ContentStore (Markdig with
  DisableHtml + YamlDotNet front matter per content-pipeline §3, mtime-keyed
  cache, slug-regex traversal guard, IO races → 404); catch-all Razor route
  renders /{slug} and /{section}/{slug} with provenance block; Content:Root
  config override for tests; publish glob for Content/pages. 22 new tests
  (parsing, routing, cache lifecycle, HTML-escape).
- **2026-07-19** — **WI-109 done** (autopilot): ResearchStage enum +
  StageBadge mapper (single source of truth incl. server-built aria-labels),
  _StageBadge (dot-meter/glyph per handoff) + _FeedCard partials,
  /dev/styleguide (dev-only, 404 in prod) rendering all 7 badge kinds + 4
  sample cards; axe scan of the styleguide added to the E2E gate. DB
  taxonomy→enum mapping decision recorded on the enum + WI-209.
- **2026-07-19** — **WI-103 done** (autopilot): helpline band on every page
  (aside landmark, CareLine tel link, → /get-help-now); /get-help-now with
  988, Crisis Text Line, CareLine, NCI, CancerCare as one-tap buttons; custom
  404 + calm Error page via status-code re-execute (direct /status/N hits
  404; large-text toggle points at the original URL on error pages). Nav +
  home "dead links" note: /get-help-now is now live.
- **2026-07-19** — **WI-102 done** (autopilot): large-text mode (22px base)
  via cookie-persisting middleware, plain-link toggle in the header (proven
  with JS disabled in Playwright); axe-core smoke tests on the shell in both
  text modes, 0 serious/critical; Kestrel dual-host test factory; CI installs
  Chromium. Review found an open-redirect blocker (protocol-relative path) —
  fixed + regression-tested; Secure cookie flag + shared URL helper applied.
- **2026-07-19** — **WI-108 done** (autopilot): Clear & Kind theme folded into
  site.css (new palette, band/card/badge tokens, 72rem container + 46rem read
  column), nav-cta pill, footer link list + ai-note, home rebuilt as Entry Hub
  (three doors → /start, /research, /get-help-now), print.css flattens the new
  surfaces incl. print-safe dot-meter ink (review Should-fix applied). Review:
  approve, no blockers. Needs Dan's visual eyeball at end of run.
- **2026-07-19** — **Design chosen & planned in**: static mock-up generated
  (`.claude/work_files/mockup/`), run through Claude Design by Dan; approved
  handoff ("Clear & Kind" + Entry Hub) moved to `docs/design/entry-hub-handoff/`.
  Backlog updated via /pm: new WI-108 (adopt theme + Entry Hub shell) and
  WI-109 (stage-badge dot-meter + feed-card partials); WI-102/103/107/209/306
  amended to depend on / reference the handoff. Next up is now WI-108.
- **2026-07-19** — **WI-101 done**: design tokens (18px-base scale, AA/AAA
  palette, spacing), real `_Layout` shell (landmarks, v1 nav, footer
  disclaimer), print.css (print-to-PDF verified), WI-005 htmx demo deleted,
  WebApplicationFactory render test added. Review clean, fixes applied.
  PR [#2](https://github.com/badsonstudios/BrainHarbor/pull/2).
- **2026-07-19** — **M0 closed**: WI-001 done (Dan bought brainharbor.org);
  PR #1 squash-merged to `main` (ce5929d) after Dan's review; `auto/M0`
  deleted. Secrets follow-ups all resolved same day.
- **2026-07-18** — **Autopilot M0 run COMPLETE**: WI-002..WI-006 shipped on
  `auto/M0` (PR #1, draft), CI green on the tip. WI-001 `[user]` outstanding.
- **2026-07-18** — **WI-006 done** (autopilot): GitHub Actions CI — build +
  test (Release) on push/PR to main, Postgres 16 service container so the
  Database-category tests run in CI too.
- **2026-07-18** — **WI-005 done** (autopilot): Htmx.Net + TagHelpers (htmx
  2.0.10 vendored), demo partial with no-JS fallback (curl-verified both
  paths); Dapper `IDbConnectionFactory` (NpgsqlDataSource DI); dev
  SYNC_API_KEY set in both apps' user-secrets. ⚠️ Dan: (1) populate
  `.claude/.env` from `.env.example` (autopilot may not touch it), (2) get a
  real NCBI_API_KEY and set it in Pipeline user-secrets + `.env`.
- **2026-07-18** — **WI-004 done** (autopilot): docker-compose (Postgres 16 @
  5433, named volume, healthcheck), DbUp on dev startup with 0001 baseline,
  connection string in user-secrets, DB smoke test. Verified on a fresh
  container; code review clean (fixes applied).
- **2026-07-18** — **WI-003 done** (autopilot): BrainHarbor.sln + Web (Razor
  Pages, net10.0) + Pipeline (console) + Tests (xUnit); build + test green.
- **2026-07-18** — **WI-002 done** (autopilot): private repo created + first
  commit pushed. ⚠️ Note: `gh` resolves to **badsonstudios**, not `danheinz`
  (`/users/danheinz` 404s — account renamed?). Repo is at
  `github.com/badsonstudios/BrainHarbor` (private ✓, `.env` untracked ✓).
  Dan: confirm the account; then docs/references mentioning `danheinz` get updated.
- **2026-07-18** — **Autopilot M0 started** (unattended run, branch `auto/M0`).
  WI-001 `[user]` (buy brainharbor.org) skipped — Dan's item, does not gate M0.
- **2026-07-18** — Domain changed to **brainharbor.org** (from .net); all docs
  updated. WI-001 now = buy brainharbor.org.
- **2026-07-18** — Workflow installed: `.claude/` adapted from ClaudeMon
  (skills: startup, pm, next-item, check-code, review, commit-push-pr, explain,
  deep-research; agents: code-reviewer, debugger, deep-research-agent; env
  hook, scripts, settings). Backlog created: M0–M4 decomposed into 34 work
  items. This file created.
- **2026-07-18** — Architecture pivot: pipeline moved local (console app +
  Task Scheduler + Claude Code CLI, no Anthropic API key); site gets a sync
  API + admin review queue; every published summary human-approved; Hangfire
  removed. Docs updated.
- **2026-07-18** — Decisions: brainharbor.net; weekly digest; local-first dev,
  Azure deferred to M4; private GitHub repo; toolchain verified (.NET 10 SDK,
  git, Docker, gh as danheinz).
- **2026-07-12** — Aggregation-first pivot (feed + plain-language summaries is
  the v1 product; static hub moved to Phase 2). Stack changed to Razor Pages +
  htmx on .NET 10 (Htmxor dead). Full design-doc set written: PLAN.md +
  docs/{architecture, sitemap, content-pipeline, data-model, roadmap}.

<!--
Maintenance rules (for the assistant):
- Starting an item  → set "In progress" (item + timestamp + current step).
- Finishing an item → move to Log with date, one-line outcome, PR link;
  update "Next up"; check the box in docs/backlog.md.
- Blocker/stopping mid-item → record exactly where things stand under
  "In progress" + "Blockers" so a cold session can resume.
- Keep "Notes for the next session" current; prune stale notes.
- Never delete Log entries; newest first.
-->
