# BrainHarbor — Progress

> **The live state of the project.** Read this first in every session (via
> `/startup`). Updated the moment an item starts, finishes, or hits a blocker —
> a fresh session must be able to resume from this file alone.
> The item definitions live in [docs/backlog.md](docs/backlog.md).

## Current state

| | |
|---|---|
| **Phase** | M2 — Ingestion + sync API + browse (M0, M1 complete & merged) |
| **In progress** | nothing mid-flight — **M2 complete** on `auto/M2` (PR #4, draft), awaiting Dan's review + merge |
| **Next up** | Dan: review + merge PR #4, then `/autopilot M3` |
| **Blockers** | none |

## Notes for the next session

- **Approved visual design lives at `docs/design/entry-hub-handoff/`** ("Clear
  & Kind" theme + Entry Hub home, from Claude Design 2026-07-19). It is the
  visual spec for WI-108/WI-109 and restyles later feed/item work (WI-209,
  WI-306). M1 order changed: **WI-108 before WI-102** so the axe/Playwright
  smoke test runs against the final theme. Handoff URL names that differ from
  sitemap.md (/get-help, /start-here) do NOT override the sitemap
  (/get-help-now, /start). The handoff folder is not yet committed — it goes
  in with WI-108's branch.
- **Remaining dead links**: only `/trials` (M4). `/research` went live in
  WI-209.
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
