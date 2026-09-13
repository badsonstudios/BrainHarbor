# BrainHarbor — Work-Item Backlog

The tracker for this project (no GitHub issues). Derived from
[roadmap.md](roadmap.md) and the other design docs; live state lives in
[../PROGRESS.md](../PROGRESS.md).

**Format:** items are `WI-<phase><nn>`, one-evening sized (~1–3 h), each with a
Goal, testable Acceptance criteria, and Refs into the design docs. `[user]` =
Dan does it, not the assistant. Work top-to-bottom within a phase unless
*Depends on* says otherwise. `/pm` maintains this file; `/next-item` executes
items and checks them off. **Never renumber existing items — append.**
`[ ]` open · `[x]` done · **`[~]` absorbed** — superseded by other items, never
built, and **not** available for `/next-item` to pick up; the item text says
what absorbed it and why.

Phases P2a–P3 (static hub, stories) are deliberately not itemized yet — run
`/pm decompose <phase>` when we get there.

---

## Phase M0 — Skeleton ($0 hosting)

- [x] **WI-001 `[user]` Buy brainharbor.org**
  Goal: own the domain before someone else does (~$12/yr; only spend until M4).
  Acceptance: registered, auto-renew on. No DNS setup needed yet.

- [x] **WI-002 Create the private GitHub repo and first commit**
  Goal: everything so far (docs, .claude, planning files) safely versioned.
  Acceptance: `gh repo create badsonstudios/BrainHarbor --private`; git init'd; first
  commit contains PLAN.md, docs/, .claude/, PROGRESS.md, .gitignore; pushed;
  `.claude/.env` confirmed untracked.
  Refs: architecture.md §8.

- [x] **WI-003 Solution scaffold**
  Goal: the two-app solution builds clean from a fresh clone.
  Acceptance: `BrainHarbor.sln` with `src/BrainHarbor.Web` (Razor Pages,
  net10.0), `src/BrainHarbor.Pipeline` (console), `tests/BrainHarbor.Tests`
  (xUnit, one placeholder test); `dotnet build` and `dotnet test` green;
  layout matches architecture.md §3.
  Depends on: WI-002.

- [x] **WI-004 Local Postgres + DbUp**
  Goal: a reproducible local database the Web app migrates on startup.
  Acceptance: `docker-compose.yml` (Postgres 16, port **5433**, named volume);
  DbUp wired into Web startup with a `0001` baseline script; connection string
  via user-secrets; app starts clean against a fresh container; a smoke test
  proves connectivity.
  Refs: architecture.md §2a/§8, data-model.md. Depends on: WI-003.

- [x] **WI-005 Htmx.Net + Dapper wiring, secrets setup**
  Goal: the web plumbing every later item builds on.
  Acceptance: Htmx.Net + tag helpers installed with one working demo partial
  (deleted later); Dapper connection factory service; `SYNC_API_KEY` +
  `NCBI_API_KEY` documented in user-secrets for both apps; `.claude/.env`
  populated from `.env.example`.
  Refs: tech-stack reference, api-keys-config.md. Depends on: WI-003.

- [x] **WI-006 GitHub Actions CI**
  Goal: every push builds and tests automatically.
  Acceptance: workflow runs `dotnet build` + `dotnet test` on push/PR to main;
  badge or checks visible on PRs; red CI blocks self-merge by convention.
  Depends on: WI-003.

## Phase M1 — Design system & shell

- [x] **WI-101 Design tokens + base layout + print stylesheet**
  Goal: the accessible visual foundation (audience constraint, not polish).
  Acceptance: `_Layout` with semantic landmarks + skip link; CSS custom
  properties (type scale ≥18px base, spacing, high-contrast palette); visible
  focus states; print.css produces a clean print of a sample page.
  Refs: PLAN.md §3, sitemap.md nav model. Depends on: WI-005.

- [x] **WI-102 Large-text toggle + a11y smoke test**
  Goal: WCAG AA is enforced by tooling from the start.
  Acceptance: large-text toggle (cookie-persisted, no-JS fallback via
  querystring); Playwright + axe-core smoke test on the shell passes with 0
  serious/critical, wired into CI.
  Depends on: WI-101, WI-006, WI-108 (test the final theme, not the interim one).

- [x] **WI-103 Helpline band + /get-help-now**
  Goal: the always-visible "talk to a human" affordance.
  Acceptance: persistent helpline band on every page (ABTA CareLine, large tap
  target); /get-help-now page with 988, Crisis Text Line 741741, org helplines;
  present on custom 404/500 pages too. Band styling per the design handoff
  (`--color-band` dark band, bold tel link, 44px targets); URL stays
  /get-help-now (handoff's /get-help does not override sitemap.md).
  Refs: PLAN.md §3, sitemap.md, docs/design/entry-hub-handoff/README.md.
  Depends on: WI-101, WI-108.

- [x] **WI-104 ContentStore: Markdown static pages**
  Goal: static pages authored as Markdown + YAML front matter, per the schema.
  Acceptance: Markdig pipeline + in-memory cache; front-matter parsing
  (content-pipeline.md §3 schema); pages route by section/slug; unit tests for
  parsing and routing.
  Refs: content-pipeline.md §3, architecture.md §3. Depends on: WI-003.

- [x] **WI-105 Glossary + inline tooltip extension**
  Goal: the inline-definitions differentiator, accessible.
  Acceptance: glossary term file format; Markdig extension marks first
  occurrence per page → `<button>` tooltip meeting WCAG 1.4.13 (focusable,
  dismissible, touch-OK) with no-JS fallback link to /glossary#term; /glossary
  A–Z page; unit tests incl. escape hatches (`%%term%%`, `!%term%`).
  Refs: content-pipeline.md §6. Depends on: WI-104.

- [x] **WI-106 ContentCheck CI gate**
  Goal: the readability promise is machine-enforced.
  Acceptance: `tools/BrainHarbor.ContentCheck` computes Flesch-Kincaid per
  content page (fail > 8.5, warn ≥ 7.5), validates front matter, reports
  overdue `review_due`; runs in CI on changed content; unit-tested against
  known-grade sample texts.
  Refs: content-pipeline.md §5. Depends on: WI-104, WI-006.

- [x] **WI-107 Write the static shell pages**
  Goal: the ~6 hand-written pages that make the shell honest.
  Acceptance: home shell (Entry Hub copy per the design handoff), /about,
  /how-we-write v0, /start (interim; handoff's /start-here does not override
  sitemap.md), /digest landing (no signup yet), /privacy + /terms + disclaimer
  partials — all passing the ContentCheck gate; medical + not-legal-advice
  disclaimers render from front-matter flags.
  Refs: sitemap.md writing budget, content-pipeline.md §2/§4,
  docs/design/entry-hub-handoff/. Depends on: WI-105, WI-106, WI-103, WI-108.

- [x] **WI-108 Adopt the "Clear & Kind" theme + Entry Hub shell**
  Goal: the approved visual design (Claude Design handoff, 2026-07-19) becomes
  the site's real theme before more UI is built on the interim one.
  Acceptance: fold `docs/design/entry-hub-handoff/css/brainharbor.css` into
  `wwwroot/css/site.css` — new `--color-*` values, `--color-band`,
  `--radius`/`--card-*`/`--badge-*` tokens, 72rem `--container` + 46rem
  `--measure-read` (type/spacing scales unchanged); restyle `_Layout` to match
  (nav "Get Help Now" as `.nav-cta` pill, footer link list + `.ai-note`
  AI-transparency line); rebuild home `Index` as the Entry Hub (surface panel,
  H1 + lede, three-door grid — Start Here / Browse research / Talk to someone —
  doors link to sitemap URLs, dead targets allowed as before); handoff `@media
  print` rules merged into print.css; contrast ratios re-verified (ink 15.6:1,
  muted 7.6:1, accent 4.9:1 underlined, band text 11:1); render test updated.
  Refs: docs/design/entry-hub-handoff/README.md (the spec — recreate to match).
  Depends on: WI-101.

- [x] **WI-109 Stage-badge + feed-card partials**
  Goal: the design's core trust device exists as reusable, tested components
  before M2 needs them.
  Acceptance: research-stage enum + mapper (human→result 5/5, review→result
  4/5, animals→2/5, cells→1/5, trial→progress, news→info,
  preprint→unverified) with unit tests; `_StageBadge` partial emitting the
  handoff markup (dot-meter/glyph `aria-hidden`, whole badge `role="img"` with
  server-built aria-label incl. "Evidence strength N of 5"); `_FeedCard`
  partial (badge → title → hook → tags → date·source, meta pinned to bottom);
  a dev-environment-only preview page rendering all seven badge kinds + sample
  cards for visual/a11y checks; consumed later by WI-208/209 (feed) and
  WI-306 (item page).
  Refs: docs/design/entry-hub-handoff/README.md §Signature components,
  content-pipeline.md §stage badges. Depends on: WI-108.

## Phase M2 — Ingestion + sync API + browse

- [x] **WI-201 Core schema + taxonomy**
  Goal: the data foundation for everything aggregated.
  Acceptance: DbUp migration for `aggregated_items` + `source_sync_state`
  exactly per data-model.md; `Content/taxonomy.yml` with initial tumor slugs;
  taxonomy loader with tests.
  Refs: data-model.md. Depends on: WI-004.

- [x] **WI-202 Sync API**
  Goal: the only write surface, secure and idempotent.
  Acceptance: `GET /api/sync/state`, `POST /api/sync/check`, `POST
  /api/sync/items` per architecture.md §4; API-key header auth (401 without),
  rate limiting; upsert idempotency proven by an integration test that uploads
  the same batch twice; source_sync_state updated on success.
  Refs: architecture.md §4, security reference. Depends on: WI-201.

- [x] **WI-203 Pipeline skeleton + sync client**
  Goal: the console app frame every fetcher plugs into.
  Acceptance: config binding (base URL, API key via user-secrets), typed
  sync-API client, `--once` run mode, structured console logging, per-source
  isolation (one failing source doesn't kill the run); integration-tested
  against the locally-running Web app.
  Refs: architecture.md §3. Depends on: WI-202.

- [x] **WI-204 PubMed fetcher + hard-rule pre-filter**
  Goal: the primary research source flowing end-to-end.
  Acceptance: E-utilities query set for brain tumors with `reldate` windowing
  driven by sync state (self-healing catch-up); API-key + 10 rps politeness;
  wrong-disease/junk hard-rule filter with unit tests; new items upload as
  pending with raw titles.
  Refs: PLAN.md §5, roadmap M2. Depends on: WI-203.

- [x] **WI-205 RSS fetchers: NCI + ScienceDaily**
  Goal: the news sources, licensing rules respected.
  Acceptance: NCI RSS (full text OK) and ScienceDaily brain-tumor feed
  (headline+summary+link ONLY) fetchers; per-source licensing enforced in
  code; dedupe against existing items via /check; tests with recorded feeds.
  Refs: PLAN.md §5. Depends on: WI-203.

- [x] **WI-206 Preprint fetcher: medRxiv/bioRxiv**
  Goal: preprints in the pipeline, permanently badged.
  Acceptance: metadata-only fetcher; `source_kind='preprint'` forced; items
  can never carry `patient_relevant` (rule + test); "not peer-reviewed"
  badge data present.
  Refs: PLAN.md §5, content-pipeline.md §9. Depends on: WI-203.

- [x] **WI-207 Admin auth**
  Goal: a locked front door for moderation.
  Acceptance: ASP.NET Identity, single seeded admin, TOTP 2FA enforced, no
  registration endpoint; admin area route group requires auth; anti-forgery on
  POSTs; login/lockout tested.
  Refs: security reference. Depends on: WI-004.

- [x] **WI-208 Review queue v0**
  Goal: the human gate exists.
  Acceptance: admin list of pending items (newest first, source/kind badges);
  approve → published, reject → rejected (htmx actions, no-JS fallback);
  status transitions audit who/when; tests for transitions.
  Refs: data-model.md lifecycle. Depends on: WI-207, WI-201.

- [x] **WI-209 /research feed**
  Goal: the public product page, v0.
  Acceptance: published items with filters (date, source, kind) as htmx
  partials degrading to querystring links; "load more" paging; items rendered
  with the WI-109 `_FeedCard`/`_StageBadge` partials in the handoff's
  `.feed-grid` (minus plain-language fields until M3); string→ResearchStage
  mapper per the doc-comment on `ResearchStage` (observational→TestedInPeople,
  preprint detected from source_kind and always wins); response caching 5–15 min.
  Refs: sitemap.md, architecture.md §5, docs/design/entry-hub-handoff/.
  Depends on: WI-208, WI-109.

- [x] **WI-210 Source health + scheduled task**
  Goal: the loop runs itself and staleness is visible.
  Acceptance: admin source-health page ("PubMed last synced N days ago", last
  error); pipeline ends with a desktop notification ("N items awaiting
  review"); Task Scheduler registration script (daily, run-after-missed-start)
  checked into the repo with setup instructions.
  Refs: architecture.md §6/§8. Depends on: WI-204.

- [x] **WI-211 M2 end-to-end shakedown**
  Goal: prove the whole loop before building on it.
  Acceptance: from a fresh DB: scheduled run fetches all sources → items
  pending → approve in admin → visible on /research; a second run ingests 0
  duplicates; findings fixed or filed as new items; PROGRESS.md notes the
  shakedown result.
  Depends on: WI-205, WI-206, WI-209, WI-210.

- [x] **WI-212 Auto-publish mode (human review optional)**
  Goal: the human review gate is optional, not mandatory — Dan's call
  (2026-07-20). Auto by default: a summarized item that passes the automated
  safety checks publishes itself; only flagged or unsummarized items wait for
  a person.
  Acceptance: `Publishing:Mode` config (Auto default, Review opt-in); sync-API
  upsert auto-publishes a new item that has a plain summary AND is not
  `summary_flagged`, generating a slug and recording a `review_events` row with
  actor `auto`; flagged/unsummarized items stay `pending`; Review mode holds
  everything; item page discloses auto-published vs human-reviewed honestly;
  design docs (PLAN, content-pipeline §"Publish mode", data-model,
  architecture, CLAUDE) updated to reflect that human review is a mode, not a
  hard requirement; tests for each branch + the default. Safe-by-construction
  pre-M3 (no summarizer → nothing auto-publishes). **The automated guardrails
  themselves land in M3 (WI-304); until then Auto mode is on but dormant.**
  Refs: content-pipeline.md §9/§"Publish mode". Depends on: WI-202, WI-208, WI-209.

## Phase M3 — Claude classification + plain-language summaries

- [x] **WI-301 Golden set**
  Goal: the quality yardstick exists before any prompt is written.
  Acceptance: ~30 real fetched items hand-verified (correct tumor tags,
  relevance tier, stage; ideal summary for ~10 of them) as versioned fixtures
  in `tests/`; a documented rubric for adding cases.
  Refs: content-pipeline.md §10. Depends on: WI-211.

- [x] **WI-302 Claude Code CLI wrapper**
  Goal: reliable programmatic access to the local `claude` CLI.
  Acceptance: invokes `claude -p --output-format json` with a versioned prompt
  template; parses/validates against a JSON schema; one retry on malformed
  output; hard failure → item flagged unsummarized (never guessed); timeout
  handling; wrapper unit tests with a fake CLI.
  Refs: architecture.md §5, content-pipeline.md §9. Depends on: WI-203.

- [x] **WI-303 Classify step**
  Goal: items sorted for a patient audience automatically.
  Acceptance: classify prompt (closed taxonomy, relevance tier, research
  stage); `excluded` items are not uploaded; preprints capped at early_stage
  (rule + test); golden-set classification accuracy reviewed and recorded;
  `classify_model` + prompt version stamped per item.
  Refs: content-pipeline.md §9. Depends on: WI-301, WI-302.

- [x] **WI-304 Summarize step**
  Goal: the differentiator — plain-language summaries with guardrails.
  Acceptance: summarize prompt implementing the 6-block template; numeral
  post-check (every number traceable to source, mismatch → flagged);
  banned-phrase scan; reading-level check on output; golden-set run reviewed;
  `summary_model` + prompt version stamped.
  Refs: content-pipeline.md §9/§11. Depends on: WI-303.

- [x] **WI-305 Review queue v1**
  Goal: reviewing 10–30 summaries/day takes ~5 minutes and means something.
  Acceptance: side-by-side summary vs source abstract; inline edit before
  approve; correction-note field (rendered publicly per content-pipeline §10);
  keyboard-friendly approve/reject; flagged items surfaced first.
  Depends on: WI-304, WI-208.

- [x] **WI-306 Item permalink pages**
  Goal: the shareable, indexable unit of the site.
  Acceptance: `/research/{slug}` renders the 6 template blocks, stage badge,
  provenance box with human-review disclosure, glossary tooltips active in
  summaries, one-tap "report a problem" (→ summary_flagged + admin queue);
  slugs generated on approval. Layout per the handoff's `research-item.html`:
  46rem reading column, `.means-block` for means/doesn't-mean, `.ai-note`
  provenance styling.
  Refs: sitemap.md, content-pipeline.md §9,
  docs/design/entry-hub-handoff/research-item.html. Depends on: WI-305, WI-105.

- [x] **WI-307 Feed flip to patient-first**
  Goal: the front page now serves the audience.
  Acceptance: /research defaults to `patient_relevant`; "show early-stage
  research" toggle (persisted, no-JS fallback); tumor-type filter from
  taxonomy; plain titles shown where available.
  Depends on: WI-306.

- [x] **WI-308 SEO + real /how-we-write**
  Goal: discoverable and honest.
  Acceptance: sitemap.xml (items + static), Article/MedicalWebPage +
  BreadcrumbList structured data, meta/OG tags (items unfurl well when
  shared); /how-we-write rewritten to describe the real pipeline incl. the
  human gate; feed.xml RSS of published items.
  Refs: PLAN.md §10. Depends on: WI-306.

- [x] **WI-309 Site search**
  Goal: one search over items + static pages.
  Acceptance: /search with htmx live results (no-JS form fallback) across
  published items (Postgres FTS) and static pages; drug-name typo tolerance
  can wait (note for later).
  Depends on: WI-306.

## Phase M4 — Azure + trials + digest → v1 launch

- [x] **WI-401 `[user]`+assisted Provision Azure** (done 2026-08-13 — shared Moodathon infra, ~$1-3/mo incremental instead of ~$30; brainharbor.org live with managed TLS; deploy on merge to `main`; feed backfilled)
  Goal: the site exists on the internet (meter starts, ~$30/mo).
  Acceptance: App Service B1 (Always On) + Postgres Flexible B1ms; deploy +
  DbUp migration steps in GitHub Actions; brainharbor.org DNS + managed TLS;
  admin 2FA re-verified in prod; pipeline pointed at prod URL with prod
  SYNC_API_KEY; feed backfilled.
  Refs: architecture.md §8/§9. Depends on: WI-309 (M3 complete).

- [x] **WI-402 Trials fetcher**
  Goal: trials flow like everything else.
  Acceptance: ClinicalTrials.gov v2 fetcher → `trials_cache` + trial_update
  feed items (429-defensive); plain-summary treatment via existing pipeline;
  tests with recorded responses.
  Refs: PLAN.md §5, data-model.md. Depends on: WI-304.

- [x] **WI-403 /trials browse + near-me**
  Goal: the trial finder.
  Acceptance: /trials browse (condition/phase/status filters); near-me via
  browser geolocation with ZIP fallback → ZCTA centroid table → live
  `filter.geo` query; trial detail pages link to ClinicalTrials.gov with
  attribution.
  Refs: sitemap.md, architecture.md §7. Depends on: WI-402.

- [ ] **WI-404 Digest signup**
  Goal: the retention channel's front door, compliant.
  Acceptance: ESP account (`[user]` creates); /digest signup with tumor-type
  prefs, double opt-in via ESP; /privacy updated with the list promises;
  unsubscribe verified end-to-end.
  Refs: data-model.md subscribers, PLAN.md §9. Depends on: WI-401.

- [ ] **WI-405 Weekly digest build + send**
  Goal: the weekly loop.
  Acceptance: weekly Pipeline mode drafts an issue from the week's approved
  items → pending digest reviewed in admin → send via ESP API on approval;
  issue archived at /digest/{n}; digest_issues recorded.
  Refs: architecture.md §6. Depends on: WI-404.

- [ ] **WI-406 Maintenance run**
  Goal: rot is caught automatically.
  Acceptance: monthly Pipeline mode: outbound link check + PubMed retraction
  check for summarized PMIDs → flags into admin queue; scheduled task
  registered.
  Refs: content-pipeline.md §11. Depends on: WI-401.

- [ ] **WI-407 Pre-launch hardening**
  Goal: launch-ready by checklist, not vibes.
  Acceptance: Lighthouse + axe pass on all page types; cheap-Android +
  throttled-3G check; custom 404/500 verified in prod; privacy-first analytics
  counter live; uptime ping on / and /get-help-now; backup/restore of prod DB
  rehearsed once.
  Refs: roadmap M4. Depends on: WI-401, WI-403, WI-405.

- [x] **WI-409 Home page leads with the feed** (done 2026-08-10 — cards render
  *below* the three doors per the design handoff, not above as written here:
  the crisis-help door must not scroll away behind research cards; Dan
  approved the deviation at the plan gate)
  Goal: the front door stops saying the front door is closed.
  Problem (found 2026-08-01, Dan): home renders a static Entry Hub and a
  paragraph reading "The daily research feed and the weekly digest are coming
  soon." The feed shipped in WI-209 and `/research` is live, so the sentence is
  false about the feed and the home page never shows an item. PLAN.md §3 says
  "the feed is the front door… not a brochure" and sitemap.md specifies "Home —
  latest plain-language research highlights + digest signup".
  Acceptance: home renders the newest few published items (same `_FeedCard`
  partial as /research, same early-stage rule) above the three doors; the
  "coming soon" sentence is corrected so it refers only to the digest, which
  genuinely is not built; a test fails if the home page claims the feed is
  coming while published items exist.
  Refs: PLAN.md §3, sitemap.md, docs/design/entry-hub-handoff/.
  Depends on: nothing. **Do before WI-408.**

- [x] **WI-410 Sort the research feed** (done 2026-08-11 — /trials does NOT
  get the control: trials carry no readiness score and registry-update
  recency is already the meaningful order there)
  Goal: let a reader ask "what is closest to helping me?" not just "what is
  newest?" (Dan's ask, 2026-08-01.)
  Acceptance: `/research` sortable by **date** (default, current behaviour),
  **readiness** (the 1–10 score, highest first), and **type**
  (research/news/trials/preprint); sort is a plain link/select that works with
  JavaScript off and survives the existing tumor-type and early-stage filters;
  the chosen sort is reflected in the URL so it can be shared and bookmarked.
  Notes: readiness is nullable — unscored items must sort last, not first
  (the same NULLS LAST trap as `published_at`). Sorting by type is a grouping,
  not a ranking, so decide the within-group order (newest first) explicitly.
  Consider whether `/trials` wants the same control.
  Refs: docs/sitemap.md (feed anatomy), content-pipeline.md §9 (readiness).
  Depends on: nothing.

- [x] **WI-411 Dedicated test database** (done 2026-08-11)
  Goal: stop database tests from sharing the live dev DB, so seeds stop
  competing with real pipeline rows.
  Problem (found 2026-08-10, WI-409 review): `TestDatabase` defaults to the
  dev `brainharbor` DB @5433, so tests seed rows into the same tables the
  pipeline fills. Three test classes now use `DATE '2999-01-01'` seeds to stay
  on page 1 of a dirty DB, and a crashed run leaves far-future test rows at
  the top of the real dev home page/feed until the next cleanup.
  Acceptance: tests run against a dedicated `brainharbor_test` database (the
  `BRAINHARBOR_TEST_DB` env var + `DatabaseFixture` guard already exist);
  local + CI both use it; the far-future-date idiom removed or reduced to a
  documented note in one place (`testing.md` or `DatabaseFixture`), not
  repeated per test class.
  Refs: tests/BrainHarbor.Tests/TestDatabase.cs, WI-402 log entry.
  Depends on: nothing.

- [x] **WI-412 Tumor-type descriptions (/tumors)** (done 2026-08-16 — 18 of 24
  types written; Dan asked for all of it at once and to review it live)
  Delivered: `/tumors`, an index built from `taxonomy.yml` (the same file the
  research filter reads, so the two cannot drift), grouped the way the taxonomy
  groups — gliomas, other primary, secondary, spinal, and the cross-cutting
  axes — because "is mine a glioma?" is a question the grouping itself answers.
  **Deviation from the acceptance, deliberate:** each type gets its OWN page
  (`/tumors/glioblastoma`) rather than only an anchor on one long page. People
  arrive from a search engine having just been handed a word by a doctor, and a
  page about their diagnosis beats an anchor part-way down a list. The anchors
  exist too, so `/research` can still deep-link.
  Descriptions live in `Content/pages/tumors/*.md`, NOT in `taxonomy.yml`: that
  file is rendered into the classifier prompt on every call, so prose there
  would cost tokens per item and would escape the reading-level gate. As curated
  pages they inherit the 6.0 CI gate, glossary tooltips, the medical disclaimer,
  `sources` front matter and site search.
  Content discipline, because this is the riskiest writing on the site:
  descriptions only — no survival figures, no prognosis, no treatment
  recommendations, since those vary per person and are where a wrong word does
  real harm. WHO CNS5 naming held throughout (grade 4 ≠ glioblastoma; DIPG is
  the pontine subset of diffuse midline glioma; spinal cord tumor is not a brain
  tumor and sits under its own heading, pinned by a test). Every page ends with
  questions to ask a care team.
  Reading grades 2.4 to 5.4, all under the gate. 777 tests.
  **Not written yet (6):** ATRT, chordoma, CNS germ cell tumor, hemangioblastoma,
  pituitary/other rare types as noted, and the "all brain tumors" catch-all.
  They render "We are still writing this one" with a link to the research feed
  for that type — honest rather than blank.
  **Open question for Dan:** these pages were drafted by AI (this assistant) and
  read as the site's own writing. `/how-we-write` describes the FEED pipeline
  and says nothing about curated pages. Worth deciding whether curated pages
  should disclose authorship the way summaries do.

- [x] **WI-441 Count page opens, on the admin page only** (done 2026-08-23 —
  Dan: "I just want to know how many people are coming to the site. I don't
  want to track anything.")
  **The question, and the honest answer to it.** Nothing was being tracked: no
  analytics package, no App Insights linked to the app (the
  `app-shamoody-insights-prod-eus2` component in the subscription is
  Moodathon's), just App Service HTTP logs on a 3-day filesystem retention that
  nothing read. Azure's built-in `Requests` metric IS available with no setup
  and 93-day retention — but it counts every stylesheet, image, bot and deploy
  smoke-check poll, so it measures load rather than readers.
  **The constraint that shaped this.** `/privacy` promised "We do not run
  analytics and we do not build a profile of you", live. So the question was
  never only technical.
  **Told Dan the part he could not have:** distinguishing new from repeat
  visitors REQUIRES persisting an identifier across days. There is no
  privacy-preserving trick around it — even Plausible and Umami deliberately
  cannot do it, rotating their salt every 24 hours precisely so yesterday's
  visitor cannot be linked to today's. Dan's call: give up repeat-visitor
  counting rather than weaken the promise.
  Delivered: `page_views (viewed_on date, path text, views bigint)` and nothing
  else — no IP hashed or otherwise, no user agent, no session or device id, no
  referrer, no sub-day timestamp. Counts buffer in a singleton and flush every
  60s, so serving a page never waits on a database write. Shown on
  `/admin/health` only; readers never see it.
  Excluded from the count so the number is worth trusting: assets, htmx
  fragments, bots, curl, the deploy smoke check, requests with no user agent,
  `/admin`, `/api`, `/dev`, redirects and errors. **The user agent is read to
  make that decision and then discarded** — never stored.
  **The query string is dropped before anything is written.** On `/trials` it
  can carry a reader's ZIP or coordinates; a test is named for this.
  Day granularity is a privacy choice as much as a storage one: per-hour counts
  on a site this small would start to expose WHEN a single reader was here,
  which is the shape of a profile without a name on it.
  Privacy copy rewritten to match, and made STRONGER rather than merely
  accurate: the new "What we do not do" bullet states that the counts cannot
  tell a new reader from a returning one and that this was chosen. Reading
  grade 4.0; review date bumped.
  Verified end-to-end against a running site, not just in tests: 4 real page
  loads plus 5 pieces of noise produced exactly `/`=2, `/research`=1,
  `/trials`=1. 821 tests, ContentCheck 50/0.
  Refs: Analytics/, Database/Scripts/0008_page_views.sql,
  Pages/Admin/Health.cshtml, Content/pages/privacy.md.

- [x] **WI-440 Make the site work on a phone, starting with the home page**
  (done 2026-08-23 — Dan: "it looks pretty good on mobile for the most part,
  but the home page needs the little hamburger menu instead of all the links at
  the top. Things like that.")
  Goal: the layout most readers will actually use should not be the one nobody
  looked at.
  **What the measurements showed.** No horizontal overflow anywhere and no text
  under the 16px floor — the body content was already sound. The damage was all
  in chrome and forms: on a 390px phone a reader scrolled past **1107px** before
  reaching `<main>`, more than a full viewport, on every page.
  Delivered:
  - **Hamburger nav** using `<details>`/`<summary>`, so it needs no JavaScript —
    the site's standing constraint. Full-width dropdown panel; each link a
    full-width row rather than a word-sized target.
  - **Icon-only masthead** below 40rem. The lockup is ~200px wide and was single-
    handedly forcing the menu button onto its own row.
  - **"Get Help Now" stays OUTSIDE the menu** at every width. PLAN.md §3 is
    "always one tap to a human"; putting the crisis route behind a disclosure
    makes it two taps for the reader least able to spare one. Pinned by a test,
    because tidying it in with the other links is the obvious-looking change for
    anyone who does not know why it is out.
  - Helpline band and hero band tightened; `main` padding and heading margins
    trimmed on narrow screens (they were stacking into ~150px of nothing above
    every `h1`).
  - **Filter forms stack**: on `/research` the wrapped row separated every label
    from its control — the page read "Kind [Everything] Sort by" then
    "[Newest first]". A label beside the wrong control is worse than no label.
    Now a grid, so the pairing is structural.
  - **Checkboxes** went from 13px to 22px with a 44px label row, keeping the
    native control so its focus ring and assistive-tech behaviour survive.
  Result: 1107px → ~890px before content, header 245px → 137px.
  **The gap this closed in the gate:** every axe scan ran at the default desktop
  viewport, where the hamburger is `display: none`. The entire phone layout —
  including a menu panel that did not exist before — was outside the
  accessibility gate. Scans now run at 390px, menu closed and open, on `/`,
  `/research` and `/trials`.
  **Three bugs hit while doing it, none of which a test could see:**
  1. *A tap-target regression I introduced:* shaved the band links to 40.5px
     chasing four pixels, breaking the 44px floor on the two most important
     links on the site. The space came from padding instead.
  2. *Specificity:* `.site-name img { display: block }` (0-1-1) outranked
     `.site-name__lockup { display: none }` (0-1-0), so BOTH logos rendered and
     the masthead stayed 242px wide. The markup and the media query were both
     correct; the toggle simply never won.
  3. *`display: contents` on `<details>`* broke the desktop nav — links laid out
     0px wide, spilling below the header. Fixed by restructuring rather than
     patching: the nav is now a SIBLING of the toggle, so desktop never has to
     fight the element's built-in content hiding.
  Desktop is unchanged; every rule is scoped to `max-width: 40rem` except the
  checkbox sizing, which was too small at every width.
  806 tests, ContentCheck 50/0. Refs: Pages/Shared/_Layout.cshtml,
  wwwroot/css/site.css, A11ySmokeTests.

- [ ] **WI-439 The Kestrel test host must start deterministically, or retry**
  (Dan, 2026-08-21, after it blocked a production deploy)
  Goal: a red `main` always means something is actually wrong.
  **Problem.** `A11ySmokeTests` intermittently fails at start-up with
  *"The Kestrel test host did not start"* wrapping *"The server has not been
  started or no web application was configured."* It is not an accessibility
  failure — no axe rule is involved; the host never came up. Tracked since
  WI-403, which added the diagnostic message but was explicitly recorded as
  **not proven fixed** because it was never reproducible on demand.
  **Why it is no longer a nuisance.** On 2026-08-22 it fired on `main` for the
  WI-438 release. `build-test` gates the `deploy` job, so **the deploy silently
  did not happen** — the merge succeeded, CI went red, and production stayed on
  the old build with pagination still broken. A re-run of the same commit passed
  and deployed fine. The cost is no longer a local re-run; it is a release that
  looks shipped and is not.
  The second-order cost is worse: a `main` that goes red for reasons that are
  not real trains everyone to re-run first and read later, which is exactly the
  habit that lets a genuine failure through.
  **The strongest lead, and it constrains the fix.** In the CI run the failing
  test took **1 ms**. A real host start takes seconds, so nothing was attempted
  — `CreateClient()` returned an already-broken cached host. `EnsureServer()`
  calls `WebApplicationFactory.CreateClient()`, and once that has failed the
  base class holds the half-built `_host` and every later call fails instantly
  the same way. **The factory poisons itself.**
  That means a naive retry loop around `EnsureServer()` **fixes nothing** — it
  would spin on the same dead cached host. A retry has to dispose the factory
  and build a fresh one, which is why this needs doing properly rather than
  wrapping the call in a `for` loop.
  **CONTRADICTED 2026-08-30 (WI-503), with a captured trx.** "The factory
  poisons itself" is not the whole story, and the paragraph above should not be
  acted on as written. Reproduced locally at **3 failures in ~16 full-suite
  runs** (~1 in 5, matching the 2026-08-29 sighting) and captured the message
  from a `.trx` rather than inferring it: same `EnsureServer()` →
  `CreateClient()` → *"The server has not been started"*, thrown from
  `InitializeAsync`. **But in that same run, 9 of the other 10 `A11ySmokeTests`
  passed** — and `A11ySmokeTests` holds the factory as an
  `IClassFixture`, so every one of them called `EnsureServer()` on the **same
  instance** either side of the failure. A permanently poisoned factory would
  have failed all ten. So the bad state is **transient**, and a plain retry may
  in fact be enough. Establish which before building the dispose-and-rebuild
  machinery — it is the more expensive fix and the evidence no longer clearly
  demands it.
  Worth noting for whoever picks this up: it lands on whichever test in the
  class draws the short straw, so it reads as "that new test is flaky" and
  invites blaming the wrong change. It attached to WI-503's new no-JS gate test
  twice, which is how it was caught here.
  Also suspect (in `CreateHost`, which is order-dependent by design): if
  `_kestrelHost.Start()` throws — a port race on `127.0.0.1:0`, or the database
  not being ready — `testHost.Start()` never runs, and the factory is left in
  exactly the poisoned state above with the original cause swallowed by the
  wrapper.
  Acceptance: either the dual host starts deterministically (find and remove the
  race), or `EnsureServer` retries by DISPOSING and rebuilding rather than
  re-asking a poisoned factory, with a bounded number of attempts and the
  ORIGINAL inner exception preserved on final failure. Whichever route, the
  first failed attempt must log what actually went wrong — the current wrapper
  guesses at two causes ("the database... or a port") and names neither.
  Worth checking as part of this: whether the CI job should retry `Test` as a
  whole. It should not. A retry there would hide real flakes; the fix belongs at
  the fixture.
  Refs: tests/BrainHarbor.Tests/KestrelWebApplicationFactory.cs (`EnsureServer`,
  `CreateHost`), A11ySmokeTests, WI-403, .github/workflows/ci.yml.
  Depends on: nothing.

- [x] **WI-438 Pagination is broken everywhere, and "Show more" should be a real
  pager** (Dan, 2026-08-21: "the pagination is not working. When I click Show
  More, it sticks on page 1.")
  **The bug.** `?page=N` is ignored on every paged page. `/research?page=2`
  returns the same items as page 1, and the "Show more" link always points at
  `page=1`. Reproduced on `/research` AND `/trials` (311 trials, identical
  first rows on pages 0, 1 and 2). `/admin/queue` shares the pattern.
  **Root cause: `page` is a RESERVED route-value key in Razor Pages.** The
  routing system uses it to hold the page path ("/Research/Index"). A handler
  parameter named `page` therefore binds the ambient ROUTE value rather than the
  query string, fails to parse as an `int`, and falls back to the default `0` —
  silently, with no error anywhere. Every other query parameter on the same
  handler (`tumor`, `kind`, `sort`, `early`) binds correctly, which is why this
  looked like a paging bug rather than a binding one.
  Fix: bind explicitly from the query with `[FromQuery(Name = "page")]` and a
  C# parameter that is NOT called `page`.
  **Why the tests did not catch it.** `FeedTests.ShowMoreUrlKeepsEveryFilter`
  sets `model.Query` directly and asserts `NextPageUrl` contains `page=2`. That
  proves the URL is BUILT correctly and never proves that requesting it does
  anything. The whole defect lives in the gap between "the link is right" and
  "following the link works". Any replacement test has to make a real request
  for page 2 and assert it returns DIFFERENT items.
  **The feature.** Replace the single "Show more" link with a real pager, the
  way a product listing does it: previous, next, the current page marked, and a
  few pages either side with the first and last always reachable. Dan's words:
  "go back, go forward, and what page you're on, and maybe a few pages coming
  up. You know how it normally works on Amazon listings."
  Acceptance: `?page=N` actually pages, on `/research` and `/trials`; a shared
  pagination partial driven by a unit-tested window calculation (first, last,
  current ±2, ellipsis for gaps); page numbers in the URL are 1-based so a
  shared link matches what the reader sees, with out-of-range values clamped
  rather than 500ing or showing an empty page; every filter still rides along;
  works with JavaScript off (plain links) and swaps via htmx when it is on, like
  the filter form already does; `<nav aria-label>`, `aria-current="page"` on the
  current one, and link text a screen reader can use ("Go to page 3", not "3");
  tap targets ≥44px; reading level within the 6.0 gate.
  Refs: Pages/Research/Index.cshtml(.cs), Pages/Trials/Index.cshtml(.cs),
  Pages/Admin/Queue.cshtml.cs, Feed/FeedRepository.cs (`HasMore`).

  **Done 2026-08-21.** Binding fixed with `[FromQuery(Name = "page")]` and a C#
  parameter deliberately not called `page`, on `/research` and `/trials` both.
  Shared `_Pagination` partial + a unit-tested `Pagination` model (21 cases);
  URLs are 1-based and out-of-range values clamp to the last real page. htmx is
  opt-in via `ViewData["HxTarget"]`, which is what lets `/trials` — with no
  swappable container — share the partial and simply navigate.
  **Scope note:** Dan asked about `/research`. `/trials` was fixed too because
  the investigation proved it carried the identical defect, and knowingly
  leaving a broken page is not a defensible reading of "fix the pagination".
  `/admin/queue` was left alone: internal triage tool, its plain "show more"
  is adequate.
  **A third defect, found by screenshot:** a DUPLICATE `.pager` rule sat ~600
  lines further down site.css (the old trials-only one). Being later in the
  file it overrode the new component and squeezed "Page 8 of 16" into a
  three-line column. Every test was green. Removed, with a comment left in its
  place — a duplicated class name in a 1,500-line stylesheet is not something
  the next person will think to look for.
  804 tests, ContentCheck 50/0.

- [x] **WI-437 Journey path replaces the readiness dial and the stage badge**
  (done 2026-08-19 — new design handoff from Dan,
  `design_handoff_brainharbor_journey`)
  Goal: one evidence indicator a reader can act on, instead of two in different
  units. Cards and the item page carried a 4-mark badge ("how well tested") AND
  a 1-to-10 readiness dial ("how close to a patient"); both are replaced by a
  four-stage path — Lab cells → Animals → Review → Tested in people.
  **This also answers WI-429**, which asked what happens to the readiness dial:
  it is gone from reader pages, and the plain/photo card split it described no
  longer exists.
  Delivered: `JourneyPath`/`StageNote` models + `_JourneyPath.cshtml`, driven
  entirely off the existing `ResearchStage` enum (the handoff's "map the 0-10
  score onto four stages" worry did not apply — the enum already WAS the four
  stages, so nothing had to be guessed); the `.journey`/`.stage-note` CSS with a
  vertical variant, print rules, and the feed grid widened 320 → 340px because
  four labels need the room; the item page's badge, readiness callout and
  repeated badge folded into one "How far along is this research?" section; the
  `?sort=readiness` sort re-pointed at the stage ladder and relabelled
  "Furthest along".
  **Scope, per Dan:** indicator only. The hero band, doors, 8 cards, See-all
  buttons, AI notice and brand assets in the same bundle had already shipped in
  WI-428a and were left alone.
  **Dan's four calls:** replace both badge and dial (not one); re-point the sort
  rather than drop it; KEEP the card photos; and — after seeing it live — put
  the path OVER the photo where the dial was, "prevalent" rather than a row of
  dots underneath.
  **Two defects found that markup and tests could not have shown:**
  1. *In the handoff itself:* `role="img"` on the `<ol>` overrides its implicit
     `list` role, orphaning every `<li>`. axe-core: SERIOUS, 28 nodes. This
     component renders on every card on every page, so as written it would have
     shipped site-wide. Fixed with `role="presentation"` on the items. Worth
     telling the designer.
  2. *Mine:* at phone width the overlaid path flipped vertical and swallowed the
     whole photo — the `.journey--over` override tied on specificity with the
     base rule and lost on source order. Only a screenshot showed it; the
     structure and every test were green. Fixed by doubling the selector
     (`.journey.journey--over`) so it no longer depends on file order.
     **The lesson is the same one WI-412a taught in a different costume:** a
     test that checks structure cannot see what a page looks like. Screenshot
     anything visual before calling it done.
  Contrast was computed by hand rather than left to axe, which cannot reason
  about a translucent plate over an arbitrary photo: white label 11.8:1 over a
  white photo, 15.1:1 over a black one; secondary labels 9.2:1 / 11.4:1. All
  AAA. Unreached dots are ~2.8:1 and decorative only — meaning is carried by
  size and the ring, never colour.
  **Not touched:** `readiness_score` in the DB, pipeline, sync contract and
  review queue, so this is reversible without a migration. The stage badge
  survives for the admin queue and style guide (a compact pill triages better in
  a dense list) — do not "finish the job" by deleting it.
  781 tests, ContentCheck 49/0. Refs: docs/design/README.md,
  Models/JourneyPath.cs, Pages/Shared/_JourneyPath.cshtml, Feed/CardArt.cs.

- [x] **WI-436 Rewrite /start with Dan's copy** (done 2026-08-19 — Dan supplied
  new text for the Just Diagnosed page)
  Delivered Dan's rewrite of `Content/pages/start.md`: new emergency heading,
  an "find the nearest emergency room" link, softer framing in the take-a-breath
  section, "all of your questions" rather than three, and a link to the ABTA
  CareLine page alongside the phone number. Reading grade 4.2, under the 6.0
  gate. `/start` also added to `sitemap.xml` — it was linked from the home page
  door but invisible to search engines, which is the WI-412a orphan pointed the
  other way, and on the page a newly diagnosed person is most likely to search
  for.
  **Four deviations from the supplied copy, all flagged to Dan:**
  1. The closing "This is not medical advice…" paragraph was NOT added: it is
     word-for-word what `disclaimers: [medical]` already renders on every
     curated page, so including it would print the disclaimer twice.
  2. **Kept "It is open on weekdays during business hours."** Dan's text dropped
     it. The CareLine is Mon–Fri 8:30–5:00 CT (verified), so without the hours a
     reader calls at 9pm Saturday and gets nothing — on the page for people at
     their worst moment. Dan's "day or night → get help now" line redirects but
     does not say the CareLine itself is closed. Reversible on his word.
  3. "Find the nearest emergency room" had no target in the copy; pointed at a
     Google Maps search (`?api=1&query=emergency+room+near+me`), which needs no
     location permission and works with JS off. Outbound third-party link.
  4. The opening paragraph became the `description` front matter, which renders
     as the lead under the H1 — where Dan put it. The duplicate "Just diagnosed?
     This is a good place to start" was dropped because the H1 already reads
     "Just diagnosed? Start here".
  Note: `FrontMatter.Description` renders visibly but is NOT wired to the
  `<meta name="description">` tag (ContentPage sets only `ViewData["Title"]`),
  so every curated page currently shares the site-wide default meta
  description. Worth its own item if SEO matters before launch.

- [x] **WI-412a Nothing linked to /tumors** (done 2026-08-16, PR #46 — found by
  Dan, not by the tests: "There's no navigation to get to the tumors page")
  WI-412 shipped a page that worked and that no one could reach. Four gaps: no
  header link, no footer link, absent from `sitemap.xml`, and no "What is this?"
  link from the `/research` tumor filter — the last of which was WI-412's own
  stated acceptance. Only a typed URL reached the page.
  "Tumor types" went into the main nav, not just the footer: "what is this thing
  I have" is the first question a newly diagnosed person asks, so it earns the
  space. On `/research`, an active tumor filter now offers the plain-English
  explanation of that type inline — the reader who picked "oligodendroglioma"
  off the dropdown because it is the word on their pathology report is the one
  who most needs it. It sits outside `#feed-results` so an htmx swap cannot drop
  it, and the label comes from the taxonomy rather than the slug (a de-hyphenated
  slug is not how the list spells the name, and "dipg" is not a word).
  **Why no test caught it:** the existing link check walks the links that exist
  and proves they do not 404. It is structurally blind to a page nothing points
  at. The new test asserts the property, not the instance — every path
  `sitemap.xml` advertises must be reachable from the home page. Worth applying
  the same shape to future work: *can a reader get here?* is a different
  question from *does this link work?*, and only the second one was being asked.
  **Second defect, caught only by rendering the page:** the new link first went
  in as a multi-line Razor implicit expression, which terminates at the newline.
  It rendered "What is System.Collections.Generic.List`1[…]" while the test —
  which asserted only the `href` — passed. The test now pins the whole anchor
  including its visible text. A test that checks the attribute a human never
  reads will not catch the text a human only ever reads.

  Original acceptance, kept for the record:
  Goal: every tumor type a reader can pick in the /research filter has a
  plain-English "what is this?" explanation (Dan's ask, 2026-08-11 — an early
  slice of P2c; sitemap.md already reserves `/tumors/`).
  Acceptance: a `/tumors` page driven by the SAME `taxonomy.yml` the feed
  filter uses (the list and the descriptions can never drift apart); a reader
  picks a type (select or A–Z list, works with JS off) and gets a hand-written
  plain-language description **targeting ~6th grade** (curated pages are
  CI-gated at 6.0 since WI-414, so this is the bar, not a stretch); each type
  deep-linkable (`/tumors#low-grade-glioma`) so the
  /research tumor filter can link "What is this?"; descriptions are curated
  content through the ContentCheck reading-level gate; glossary tooltips
  active; a type whose description is not yet written says so honestly
  ("we are still writing this one") rather than rendering blank; medical
  naming follows the WI-201 lesson (WHO CNS5 — e.g. "grade 4 glioma" ≠
  glioblastoma, DIPG ⊂ diffuse midline glioma); no AHFS/MedlinePlus text
  (licensing, PLAN.md §5). Write the most common types first; finishing all
  22 may split into a follow-up item.
  Refs: docs/sitemap.md (`/tumors/`), content-pipeline.md §2/§5,
  Content/taxonomy.yml. Depends on: nothing.

- [x] **WI-413 Tell "the CLI is down" apart from "this item is odd"** (done 2026-08-13 — the CLI now says which; a health probe settles the ambiguous cases so an odd item can never stall a source)
  Goal: close the last hole in the WI-401 fail-fast work — the pipeline
  currently infers an outage from a STREAK of failures, because a classifier
  failure carries no cause.
  Problem (found 2026-08-12 in review): if an outage begins inside the last
  one or two items of a small window, the streak never reaches the threshold
  and those items upload as permanently unclassified — the state that needed
  532 rows hand-deleted from prod. Treating an all-failed window as an outage
  instead would stall a source forever on one item that can never be
  classified, so counting is the wrong signal in both directions.
  Acceptance: `ClaudeCli` distinguishes "the CLI never answered" (non-zero
  exit, timeout, spawn failure — infrastructure) from "it answered but the
  output was unusable" (validation) and carries that on `ClaudeResult`;
  `IItemClassifier` surfaces it (e.g. a `ClassifyDecision.Unavailable`), and
  `Classifier` also returns it when the taxonomy call failed; `PipelineRunner`
  stops on the FIRST unavailable rather than on a streak, and keeps uploading
  genuinely-unclassifiable items for a person; the streak counter and the
  known-residual comment/test in `PipelineRunnerTests` are removed as
  obsolete; a test proves a single unavailable result stops the source without
  uploading it, and that a persistently-odd item still reaches the queue and
  does NOT stall the cursor.
  Refs: PipelineRunner.MaxConsecutiveClassifyFailures, Claude/ClaudeCli.cs,
  Classify/Classifier.cs. Depends on: nothing.

- [x] **WI-414 Hold Razor page copy to the reading-level gate too** (done 2026-08-13, released in PR #19 — `RazorTextExtractor` + a 6.0 fail gate over reader-facing `.cshtml`; checkbox was missed at the time)
  Goal: the front page is held to the same automatic standard as the curated
  Markdown pages.
  Problem (found 2026-08-13 writing the home AI-disclosure copy): ContentCheck
  scans `Content/pages/*.md` and `Content/glossary/*.md`, so the ≤ 8.5 gate
  covers /about, /privacy and friends — but NOT the home page, /research,
  /trials or /search, whose copy lives in `.cshtml`. The most-read text on the
  site is the only text no tool checks. Today's home copy measures ~4.8 by
  hand; nothing stops the next edit from landing at 12.
  Acceptance: ContentCheck extracts reader-facing prose from the Razor pages
  (headings, paragraphs and list text — not markup, attributes, or code) and
  applies the same Flesch-Kincaid gate; a deliberately hard sentence in a
  `.cshtml` fails CI; the existing per-page warnings still work; document in
  `content-pipeline.md` §5 that both content types are gated.
  Notes: block-aware sentence extraction already exists (WI-106) — the work is
  a Razor-aware text extractor, not a new grader. Watch for false positives
  from things like the ABTA phone number and NCT ids.
  Refs: tools/BrainHarbor.ContentCheck, content-pipeline.md §5. Depends on: nothing.

- [x] **WI-415 Get AI summaries to a 6th-grade reading level** (done 2026-08-13 — prompt asks for 6th grade and delivers median 4.7; gate set to 7.0 as a backstop, not the target)
  Goal: the summaries meet the same bar the pages now do (WI-414) — without
  emptying the feed to get there.
  Measured 2026-08-13 over the 1,038 published summaries: median grade **6.7**,
  and a 6.0 gate would flag **73.5%** of them. Flipping the threshold alone
  would stop auto-publishing, not improve reading level.
  Acceptance, in order: (1) **fix the grader** — `Guardrails.GradeLevel` joins
  the plain title and six blocks with newlines and no terminators, so the title
  runs into the hook and inflates every score; make it block-aware like
  `ContentChecker.ExtractSentences` (the same summaries then measure median
  **6.0**, flag rate 50.3%); (2) **change the prompt** to ask for 6th grade
  explicitly, and re-run the golden set — a versioned prompt change requires it;
  (3) re-measure the distribution and only then lower `Guardrails.MaxGradeLevel`
  toward 6.0, choosing the number from the new data; (4) decide what happens to
  already-published summaries above the new bar (leave, re-summarize, or flag).
  Note: the two graders differ — the summarizer exempts a medical-terms list
  and skips the vowel-hiatus rule that `ReadabilityAnalyzer` applies. Consider
  making both call one implementation so "6th grade" means one thing.
  Refs: Summarize/Guardrails.cs, content-pipeline.md §5/§9, WI-414.
  Depends on: nothing (but do it before WI-408 soft launch).

- [ ] **WI-416 One reading-level grader, not two**
  Goal: "6th grade" should mean one thing.
  Problem (found 2026-08-13 in the WI-415 review): `ReadabilityAnalyzer`
  (pages) and `Guardrails.GradeLevel` (summaries) implement Flesch-Kincaid
  differently — the summary grader exempts a medical-terms list at 2 syllables
  and skips the vowel-hiatus rule, and its word pattern is `[A-Za-z]+` vs
  `[A-Za-z']+`, so "doesn't" counts as two words. The same text scores
  differently depending on which one measures it, which makes the page limit
  (6.0) and the summary backstop (7.0) not directly comparable.
  Acceptance: one implementation both call, with the medical-term allowance as
  an explicit option rather than a fork; the existing thresholds re-measured
  against it and adjusted if the numbers move; tests that pin the shared
  behaviour; content-pipeline §5 states which allowance applies where.
  Refs: tools/BrainHarbor.ContentCheck/ReadabilityAnalyzer.cs,
  Summarize/Guardrails.cs. Depends on: nothing.

- [ ] **WI-435 ContentCheck passes when it checked nothing**
  Goal: the gate that enforces reading level on medical copy must fail loudly
  when it is not actually checking anything.
  Problem (found 2026-08-19 while editing `start.md`): the tool takes the pages
  root as a positional argument, so ANY unrecognised argument is silently
  swallowed as that path. `dotnet run --project tools/BrainHarbor.ContentCheck
  --nologo` prints `WARN pages root MISSING — no pages were checked`, skips
  every Markdown page, and **exits 0** — reporting "ContentCheck passed
  (22 checks, 0 failures)" while having graded zero curated pages. The count is
  the only tell, and only if you know 48 is the right number. I hit this myself
  and quoted the partial number as verification on PR #46; CI invokes the tool
  correctly, so nothing shipped ungated, but the tool made a wrong claim look
  like a right one.
  Why it matters beyond the typo: this is the check standing between an
  AI-drafted tumor description and a frightened reader. A gate whose failure
  mode is "quietly grade nothing and report success" is worse than no gate,
  because it manufactures confidence. The same shape would hide a CI
  misconfiguration, a moved content directory, or a bad refactor of the path
  resolution — none of which would turn CI red.
  Acceptance: a missing/unresolvable pages root is a FAILURE (non-zero exit),
  not a WARN; unknown arguments are rejected rather than reinterpreted as a
  path; the summary line states what was covered ("N pages, M glossary, K
  Razor") so a collapse in coverage is visible at a glance; a test asserts the
  tool exits non-zero when pointed at a directory that does not exist.
  Consider also asserting a floor — if the pages root resolves but yields zero
  Markdown files, that is still a broken run, not a clean one.
  Refs: tools/BrainHarbor.ContentCheck/ContentChecker.cs, .github/workflows/ci.yml.
  Depends on: nothing. Related: [WI-416] (the two graders).

- [x] **WI-417 Real logs for the scheduled pipeline runs** (done 2026-08-13 — per-run file in `%LOCALAPPDATA%\BrainHarbor\logs`, self-pruning, key-scrubbed, plus a flags-by-cause tally)
  Goal: a daily run that fails at 06:00 leaves evidence Dan can read (Dan's
  ask, 2026-08-13).
  Problem: Task Scheduler captures no console output, so the nightly runs are
  invisible. Today the only signals are the task's exit code
  (`Get-ScheduledTaskInfo`: 0 all ok, 1 some sources failed, 2 cancelled,
  3 bad config, 4 blew up), a desktop notification nobody sees at 6am, and the
  admin health page's per-source last-error. Everything the console prints —
  which item was excluded and why, which summaries were flagged and for what,
  the classify/summarize failures — is lost. That is exactly the detail the
  last three production incidents were diagnosed from.
  Acceptance: pipeline writes a per-run log file (a file logging provider or
  the scheduled action redirecting stdout/stderr); one file per run, named by
  date so runs do not overwrite each other; **rotation/retention** so it cannot
  fill the disk (keep ~30 days); the path is documented in
  `docs/run-local.md` and printed at the end of a run; the registration script
  wires it up so a fresh registration gets logging without extra steps; the
  log NEVER contains the sync API key or the NCBI key (they are already
  filtered from HttpClient logging — keep it that way and test it).
  Consider: also POST a run summary to the sync API so the admin health page
  shows last-run status and counts, not just per-source errors.
  Refs: scripts/register-pipeline-task.ps1, architecture.md §6,
  Program.cs logging setup. Depends on: nothing.

- [x] **WI-418 Show WHY a summary was flagged, not just that it was** (done
  2026-08-14 — the queue names the check; solved by re-checking rather than by
  a migration, so all ~137 existing items are explained immediately)
  Goal: the review queue and the health page can say which check held an item
  back (Dan's ask, 2026-08-13 — split out of WI-417).
  Problem: `aggregated_items.summary_flagged` is a boolean with no reason, so
  the site can report a flag RATE ("4.8% of summarize-v4 items") and never a
  cause. WI-417 fixed this for runs going forward — the run log names the check
  per item and totals the flags by kind — but the log is per-run and local to
  Dan's PC: it cannot say why one of the 137 items already in the queue is
  there, and a reviewer opening an item still has to re-read it and guess what
  tripped.
  Acceptance: the flag reasons travel with the item (sync contract + a column
  or small table — they are already structured as `Guardrails.FlagKind` plus a
  message, so nothing needs parsing); the review queue shows them on the item
  it is judging; the admin health page can total them; and a decision recorded
  for the rows already flagged (backfill by re-running the checks over the
  STORED summary, or leave them blank and say so honestly in the UI —
  re-summarizing is not on the table, it would rewrite published wording).
  **How it was actually done, and what was deliberately NOT done:** the checks
  are pure text analysis and every summary is already stored, so the reason is
  *recoverable* — no migration, no sync-contract change, and the whole existing
  queue is explained the moment this deploys rather than only new items.
  `Guardrails` moved to a shared `BrainHarbor.Safety` project that both apps
  reference (a copy in the site would have been a second implementation of the
  same rule — the WI-415 defect). The queue's re-check joins `trials_cache` so
  a trial's phase is not reported as an invented number.
  Two limits, both stated in the queue rather than hidden: it reflects TODAY's
  rules (the reading ceiling moved 8.5 → 7.0 on 2026-08-13), and a
  reader-reported item has no automated reason at all.
  Refs: src/BrainHarbor.Safety, Admin\ReviewRepository.ReviewItem.FlagReasons,
  Pages\Shared\_ReviewRow.cshtml. Depends on: WI-417.

- [x] **WI-426 Stop flagging summaries for DENYING hype, and clear the backlog
  it caused** (done 2026-08-14 — Dan found it reading his own queue)
  Bug: the negation exemption was wired to "cure" alone. Every other banned
  phrase was a bare keyword match, so "this is not a breakthrough" and "this is
  not a game-changer" were flagged AS hype — in the "what this doesn't mean"
  block that ends every summary, whose entire purpose is to write sentences
  like that. The guardrail was punishing summaries for obeying the anti-hype
  rule, holding them out of Auto publish and piling them into the review queue.
  Fixed: the sentence-scoped negation check now applies to every banned phrase.
  A genuine "this IS a breakthrough" is still caught, and a denial in one
  sentence still does not license a claim in the next (or in the next block).
  Plus **bulk approve** in the queue: one action for every pending item that no
  check flags — which is exactly what Auto mode publishes by itself, so
  clearing them one click at a time was work the design never intended.
  **Deliberately not "approve everything":** an item flagged for an untraceable
  number stays (that is the site's central factual promise, and where a model
  may have invented a survival figure), and so does an item with no summary
  (approving it publishes an empty page to a patient). The audit row records
  who clicked AND that it was a bulk action — "reviewed by" must never imply
  someone read that particular summary.
  Refs: BrainHarbor.Safety\Guardrails.BannedWordsIn,
  Admin\ReviewRepository.GetPendingWithNoFailingCheckAsync, Pages\Admin\Queue.

- [x] **WI-427 Negation never worked for contractions** (done 2026-08-14 — Dan
  saw hype flags still coming through after WI-426 shipped)
  Bug, and the bigger half of the queue: negation was a word LIST matched
  against `[A-Za-z]+` tokens, which strip the apostrophe. "doesn't" tokenized
  to "doesn" + "t", so the list's `doesn't` / `isn't` / `n't` entries could
  never match anything. Every contraction read as un-negated — and the block
  these sentences live in is *called* "what this doesn't mean", so that is
  about the commonest phrasing in the corpus. It hit `cure` too, so this had
  been mis-flagging since WI-401, not since WI-426.
  Fixed with a negation REGEX (the apostrophe is part of the word), accepting
  straight and curly apostrophes. The `n't` branch requires the apostrophe: a
  bare `\w+nt` would match "importa-n-t" and quietly excuse "this is an
  important breakthrough". Pinned by tests in both directions.
  Refs: BrainHarbor.Safety\Guardrails.Negation/IsNegated.

- [x] **WI-428a Homepage redesign — "Harbor Banner"** (done 2026-08-15 — Dan
  brought a finished handoff back from Claude Design; reviewed locally and
  approved)
  Delivered: hero band (lockup as the `h1` with visually-hidden heading text,
  watermark, wave edge), two doors instead of three, "Latest updates" with the
  lighthouse mark and wave rule, **8 cards** instead of 4, "See all" as a filled
  button in the section head and a large outlined button below the feed, and the
  AI notice moved to the foot of the page. Evidence badge **4 marks, ladder with
  no gaps** (was 5, 4, 2, 1 — nothing at 3). New `Banner` layout section so a
  full-bleed band can sit before `main`.
  **Absorbs WI-425** (the prominent "See all" button) entirely.
  **Two deliberate deviations, both safety copy, both pinned by tests:** the AI
  admission leads in the hero band (the handoff has no hero copy, and a reader
  must not get through eight summaries before learning who wrote them), and
  "A person does not check every one" survives into the bottom notice (the
  handoff's copy says checks run, which is not the same statement).
  Reading grade 3.7, axe clean, 772 tests.
  Refs: docs/design/README.md (which handoff governs what), docs/design/homepage-handoff.

- [x] **WI-430 A suggestions address on the site** (done 2026-08-15 — Dan's ask,
  ahead of sharing the site publicly)
  Goal: people who find the site can say what would make it better.
  Delivered: `support@brainharbor.org` in three places — a "Tell us how to make
  this better" box at the foot of the home page, one line in the footer of every
  page, and a section on `/about`. A line in `/privacy` says what happens to an
  email (kept so we can act on it; no list, no other use).
  **The copy carries a safety steer, and that is the load-bearing part.** The
  mailbox is named support@ but is for site suggestions, and "support" is
  exactly what a frightened reader would email at 2am expecting help — then wait
  days. So every prominent placement says the inbox is not a way to get medical
  or urgent help, and puts the ABTA CareLine number right there rather than a
  link away. Pinned by a test.
  Reading grades: home 3.3, about 3.4, privacy 3.8.

- [x] **WI-428 Restyle the research item page to match the new homepage** (done
  2026-08-15 — and it was very nearly already done)
  Finding: every item-page style the 2026-08-15 handoff specifies —
  `.means-block` and its head/icon, `.original-title`, `.term`, `.provenance`,
  `.ai-note` — already matched it value for value, because the page was built
  from the previous handoff and this one barely changed it. The real gap was
  one thing: the handoff repeats the evidence badge under "How early is this?",
  beside the words that explain it. Added. By that point the reader has passed
  the whole summary, and asking them to scroll back up to count marks is asking
  them to give up.
  The badge itself already showed 4 marks there from WI-428a.
  Kept deliberately, against the handoff: the readiness callout (the handoff's
  item page has none, and Dan wants readiness MORE prominent, not less) and the
  heading "What this means, and what it doesn't" (the handoff writes it with an
  em dash, which the site's own copy rule forbids).

- [x] **WI-429 Homepage cards match /research** (done 2026-08-15 — Dan's call,
  the opposite of what the handoff specified)
  The handoff's card is badge, title, hook, meta: no photo, no readiness dial.
  Dan, after seeing both live: the `/research` card is the better one and the
  homepage should match it. **The dial is the reason** — the badge says how well
  TESTED a finding is; the dial says how close it is to something a patient can
  actually get, and the homepage was missing that second answer entirely.
  So the `PlainCard` flag is gone and one card renders on both pages, which
  means they cannot drift apart again. Recorded as deviation 3 in
  docs/design/README.md so nobody later "restores" the handoff version.
  Goal: finish the 2026-08-15 handoff — the homepage shipped, the item page did
  not.
  Acceptance: rebuild `Pages/Research/Item.cshtml` against
  `docs/design/homepage-handoff/research-item.html` — the "What this means, and
  doesn't mean" block (`.means-block` with its circled icon), `.original-title`,
  `.provenance` styling, and the glossary `.term` treatment. Keep every existing
  rule: only a PUBLISHED item renders, a pulled item 404s exactly like one that
  never existed, no summary is ever invented, and the registry's words stay
  labelled as the registry's. Reader-facing, so it rides the 6.0 ContentCheck
  gate and the axe scan.
  Note: the badge already renders 4 marks there; this is styling, not data.
  Refs: docs/design/homepage-handoff/research-item.html + README §Item page.
  Depends on: nothing.

- [x] **WI-429 Decide what happens to the readiness dial on /research**
  (answered 2026-08-19 by **WI-437**, not by a decision taken here)
  The question was "plain card or photo card, and does the reader see a
  readiness score". The journey handoff dissolved it: the score is gone from
  every reader-facing page, the photo stays, and one card renders on both pages
  so there is nothing left to keep in sync. The `PlainCard` flag this item was
  written to retire had already gone when the homepage adopted the /research
  card on 2026-08-15.
  Original text below for the record.

  Goal: one answer to "does the reader see a readiness score", instead of two.
  Problem (surfaced by WI-428a): the homepage handoff's card is badge, title,
  hook, meta — no photo backdrop, no 1-to-10 readiness dial. The homepage now
  renders that plain card; `/research` still renders the photo and the dial,
  because it is not in the handoff's scope AND it carries a "Most ready to use"
  sort (WI-410) that would otherwise sort by a number the reader cannot see.
  So the two pages currently disagree about what a card is. That is deliberate
  and temporary — `_FeedCard` takes a `PlainCard` flag — but it should not stay.
  Acceptance: Dan picks. Either `/research` adopts the plain card and the
  readiness sort goes (the score stays in the DB, the pipeline, and the review
  queue), or the homepage keeps the plain card as a deliberate exception and
  that is written down. Then the `PlainCard` flag goes away either way.
  Note: the card photo pool (`CardImages`, `wwwroot/img/cards`, IMAGE-CREDITS)
  is a real feature with licensing notes — retiring it is a decision, not a
  cleanup. Refs: Pages/Shared/_FeedCard.cshtml, WI-410, WI-306.

- [x] **WI-425 A prominent "See all" button under the home feed** (done
  2026-08-15 as part of WI-428a — the redesign ships both a filled button in the
  section head and a large outlined one below the feed, which is what this asked
  for)
  Goal: the way out of the home page's four cards is obvious (Dan's ask,
  2026-08-14: "the See all link next to Latest updates is way too small").
  Acceptance: **keep** the existing small "See all →" link beside the heading —
  Dan's call, it serves someone already scanning the heading row — and ADD a
  prominent button BELOW the four cards, where a reader who has just finished
  reading them is actually looking. Full-width or centred, real button styling
  (the `.nav-cta` pill or the door treatment, not a text link), a tap target
  comfortably over 44px, and wording that says where it goes ("See all research"
  beats "See all", which begs "all what?"). Same anti-hype, plain-language
  rules; it is reader-facing copy so it rides the 6.0 ContentCheck gate.
  Watch: it must not compete with the crisis-help door above it for attention —
  the hierarchy is help first, browse second.
  Notes: the home feed renders 4 cards (WI-409) and the section already has a
  heading-row link in `Pages/Index.cshtml`; `HomeFeedTests` covers this section.
  Refs: Pages\Index.cshtml, wwwroot\css\site.css (.door, .nav-cta).
  Depends on: nothing.

- [x] **WI-432 Fix the human-review claim in /terms, and say what the site is
  for** (done 2026-08-15 — Dan asked whether the site needs legal cover)
  The real defect: `/terms` said "We check each summary before it goes up",
  which reads as human review. Publishing mode is Auto and no person reads each
  summary; every other page was scrubbed of that implication on 2026-07-31 and
  this one was missed. It is the page most likely to be read as a promise, which
  makes it the worst place to leave it. Now says AI writes them, they must pass
  automatic checks, and a person does not read every one.
  Also: the page states plainly that the site is for learning and for asking a
  care team better questions, and a new "We are not part of these groups"
  section covers ABTA, NCI and ClinicalTrials.gov — the site points at all three
  and none of them checked what we wrote. `reviewed` stamp refreshed.
  Reading grade 4.3.

~~WI-433 Have a lawyer review the site~~ — **dropped 2026-08-16, Dan's call.**
Not doing it. Recorded rather than deleted silently so it is not re-filed every
time someone reads `/terms` and notices what is missing.
What still stands, as fact rather than argument: `/terms` and `/privacy` say
what the site is and is not, `/terms` no longer implies a human reads each
summary (WI-432), every page carries the not-medical-advice line, and there is
no warranty disclaimer, no limitation of liability and no governing-law clause.
One source note worth keeping for its own sake: publicly accessible is not the
same as public domain — PubMed abstracts often carry publisher copyright, which
is why the pipeline summarizes and links rather than republishing, and keeps raw
abstracts admin-only (pinned by a test). Do not change that behaviour.

- [ ] **WI-434 Fill out the glossary (three terms is not a glossary)**
  Goal: the words a patient meets on this site are explained where they meet
  them (Dan's ask, 2026-08-15).
  Today `Content/glossary/` holds exactly three: glioblastoma, glioma, IDH gene
  change. The machinery is done and good — `GlossaryStore` + the Markdig
  extension mark the FIRST occurrence per page as a native-popover tooltip
  (WI-105), `/glossary` lists them A to Z, and ContentCheck caps a definition at
  40 words. What is missing is the content.
  Acceptance: add terms in the vocabulary this audience actually hits, each in
  the existing format (`term`, optional `also` aliases, `pronunciation`, then a
  definition of 40 words or fewer at the site's reading level). Candidates,
  drawn from what the feed and the taxonomy already use rather than invented:
  - **tumor types:** astrocytoma, oligodendroglioma, meningioma, medulloblastoma,
    ependymoma, DIPG / diffuse midline glioma, acoustic neuroma / schwannoma,
    craniopharyngioma, brain metastases, low-grade glioma;
  - **words on a pathology or MRI report:** grade, IDH, MGMT, 1p/19q,
    resection (gross total / subtotal), biopsy, contrast enhancement, edema,
    progression, recurrence, pseudoprogression;
  - **treatment words:** temozolomide, bevacizumab, radiotherapy, proton therapy,
    stereotactic radiosurgery, tumor-treating fields, craniotomy, shunt,
    steroids / dexamethasone, clinical trial phase;
  - **words the summaries themselves use:** progression-free survival, overall
    survival, median, placebo, randomized, control group, adverse event.
  Rules that already apply and must hold: **no AHFS/MedlinePlus drug monograph
  text** (ASHP copyright, PLAN.md §5) — write plain definitions from public
  sources and cite; ≤40 words each; reading level at the site's bar; no hype
  words; a definition must not read as advice about someone's own care.
  Worth deciding while doing it: the tooltip fires on the first occurrence per
  page, so a term that appears in every summary (like "glioma") will mark
  constantly — check that it does not become visual noise on a long feed.
  Note: WI-105 shipped with a real gap of this shape — no page used a term, so
  the tooltip was invisible until a sample was added to the styleguide. More
  terms is also more coverage of that feature in the wild.
  ### How the page should work (planned 2026-08-16, Dan asked for a proposal)

  **One page, sections by first letter, anchor per term. Not pagination.**

  Pagination by letter was the alternative and it fails this audience on one
  point: it makes the reader guess the spelling before they can look anything
  up. Someone who has just been handed the word "oligodendroglioma" out loud, or
  who is reading it off a pathology report through treatment fog, cannot
  reliably pick the right letter tab — and a wrong guess returns an empty page
  that looks like the site does not have the word at all. A single page means
  the browser's own find (Ctrl+F, and "Find in page" on a phone) searches
  everything, which is what people actually do.
  It also keeps one URL to link and share, prints in one pass (patients print
  for appointments), and needs no JavaScript.

  Concretely:
  - **Letter jump bar** at the top: plain anchor links (`#a`, `#b`, …), tap
    targets at the site's 44px floor. Letters with no terms render as plain
    text, NOT as links to an empty section — a dead link is worse than an
    absent one.
  - **`<h2>` per letter, `<h3>` per term**, each term with a stable `id` of its
    slug, so anything can deep-link (`/glossary#mgmt`). Heading levels stay in
    order; heading navigation is a primary strategy for this audience.
  - **`scroll-margin-block-start`** on those targets, or a jumped-to term sits
    under the top of the viewport.
  - **"Back to top" after each letter section** — on a phone, 60 terms is a
    long way back.
  - **Print:** hide the jump bar and the back-to-top links; show every term.
  - **Search:** `/search` does NOT index glossary terms today (it covers feed
    items and `Content/pages`, not `Content/glossary`) — so someone searching
    "MGMT" gets nothing. Index the terms, or at minimum make the glossary page
    itself findable, and label the results so a definition is not mistaken for
    a research finding.
  - **Revisit if it passes ~150 terms.** Sixty entries of 40 words is about
    2,400 words, which is a comfortable page. It is not a rule that scales
    forever.

  Note the interaction with the tooltip: the marker fires on the FIRST
  occurrence per page. With 60 terms live, check a long feed page for visual
  noise before shipping — that is a content-volume problem the three-term
  version could never show.

  Refs: Content/glossary/*.md, GlossaryStore, GlossaryMarker, /glossary,
  Pages/Search.cshtml.cs, content-pipeline.md §6. Depends on: nothing.

- [x] **WI-431 Harden the deploy smoke check** (done 2026-08-15)
  Delivered: the check now requests all five main routes, on `brainharbor.org`
  (the domain visitors use) rather than the Azure hostname, and requires TWO
  consecutive clean passes so one lucky hit during App Service's old/new overlap
  cannot pass it. On failure it prints the response body per failing route, and
  then the same routes on the Azure hostname so app trouble can be told apart
  from domain trouble.
  **The body capture is the point as much as the gating is.** Eight sightings of
  this on 2026-08-15 produced no evidence of the CAUSE, because nobody ever
  captured a failing response. The next occurrence will produce one.
  Verified both paths locally against the live site before shipping: healthy
  exits 0 after two consecutive passes; a deliberately bad host captures the
  body, resets the counter and exits 1.
  **Known limit, stated in the workflow rather than implied away:** this makes a
  bad deploy fail loudly and warms the routes, which shortens the window. It
  does not remove it. Zero-downtime needs a staging slot and a swap, and slots
  need Standard tier — a monthly cost, not a code change. Free mitigation in the
  meantime: deploy at a quiet hour once the site has traffic.
  Refs: .github/workflows/ci.yml.

- [ ] **WI-431b `[user]` Decide whether zero-downtime deploys are worth Standard tier**
  Goal: close the deploy window properly, or decide not to and know why.

  ### First real measurement (2026-08-16, deploy of WI-412)

  The hardened check caught the window on its first live run and produced the
  evidence eight earlier sightings never did:

  - **Two minutes, not one.** 22:18:21 to 22:20:19, nine failed rounds before
    two consecutive clean passes. The deploy still finished green, correctly:
    it waited, confirmed health, and passed.
  - **Four routes down, `/` up throughout:** `/research`, `/trials`, `/search`
    and **`/get-help-now`** all 500 while the home page answered 200.
  - **The bodies were EMPTY.**

  **That last point corrects something recorded earlier in this file and in
  PROGRESS.** The previous note said a visitor caught in the window still gets
  the calm custom error page with the helpline band and the CareLine number.
  That is false. An empty 500 means the request never reached the application,
  so ASP.NET's exception handler never ran and no BrainHarbor page was rendered.
  The visitor sees the browser's own "this page isn't working" screen: no
  helpline, no phone number, no branding. The route most affected is the one a
  distressed reader is most likely to want.
  It also rules out an application exception as the cause. An empty 500 is the
  platform failing to get any response from the worker — the process is down or
  still starting while Azure keeps routing to it. So this is start-up or swap
  behaviour, not a bug in the code.

  ### The series is complete (2026-08-19) — this is now a decision, not a study

  | Release | Window | Notes |
  |---|---|---|
  | WI-412 (PR #45, 2026-08-16) | **2m00s** | nine failed rounds |
  | WI-412a (PR #47, 2026-08-16) | **1m23s** | six failed rounds |
  | WI-436 (PR #49, 2026-08-19) | **1m24s** | six failed rounds |

  Three measurements, the last two within one second of each other, every one
  with the same signature: `/research`, `/trials`, `/search` and
  `/get-help-now` all 500 with empty bodies while `/` answers 200 throughout.
  **The behaviour is settled: roughly 85 seconds, every deploy.** The two-minute
  first sample looks like the outlier, not the rule.

  The precondition this item set for itself — "collect two or three more
  measurements", "do not decide until the body capture has caught the failure at
  least once" — is met three times over. The bodies were empty every time, which
  rules out an application exception and rules out the cheap code-level fix that
  might have made this free. It is platform start-up/swap behaviour, so the only
  thing that removes it is a staging slot, and slots mean Standard tier.

  ### Recommended
  1. **Deploy at quiet hours and batch changes into fewer releases.** Free, and
     costs nothing at all while the site is unlaunched. Sufficient for now.
  2. **Revisit at soft launch (WI-408), not before.** ~85 seconds of hard 500s
     is worth nothing today and is a real defect the day the site has readers —
     specifically on `/get-help-now`, the page the band on every page points a
     distressed reader to. That change in stakes, not a new measurement, is what
     should trigger the spend.

  WI-431 made the window visible and shorter; it cannot remove it. Removing it
  means deploying to a staging slot, warming it, and swapping — and **slots
  require Standard tier**, so this is a monthly bill, not a code change. The
  plan currently runs on a shared B1 (WI-401 chose it deliberately at ~$1-3/mo
  incremental).
  ~~Do not decide this until WI-431's body capture has caught the failure at
  least once.~~ **Satisfied 2026-08-16, three times over by 2026-08-19.** The
  hoped-for cheap outcome (app start-up or the DbUp advisory lock, fixable in
  code for nothing) is ruled out: an empty body means no application code ran.
  What remains to weigh: how often deploys actually happen once the site is not
  being rebuilt daily, whether they can simply run at a quiet hour, and what
  ~85 seconds of 500s on the inner pages is worth once there are readers.
  Note the last of those is worse than this item originally assumed — the
  earlier text said the custom error page still carried the helpline band and
  the CareLine number during the window. It does not; see the correction above.
  A reader who lands on `/get-help-now` mid-deploy gets the browser's own error
  screen with no phone number on it.
  Refs: WI-431, WI-401 (plan sizing), .github/workflows/ci.yml.

- [ ] **WI-424 Record the flag reason at flag time, not just re-derive it**
  Goal: an audit trail of what the checks said WHEN they said it (the other
  half of WI-418, deliberately deferred).
  Problem: the queue now re-checks a stored summary and names the check, which
  is what a reviewer needs. But it answers "what fails today", not "what failed
  then" — the reading ceiling moved 8.5 → 7.0 on 2026-08-13, and prompt and
  check changes will keep moving it. For a site whose safety story is auditable
  automation, "why did this item not publish on the night it arrived" should be
  answerable from the record, not reconstructed.
  Acceptance: the pipeline sends the reasons it actually computed (it already
  has them — `SummaryResult.FlagReasons`) on the sync contract; a column or
  small table stores them; the queue prefers the stored reason and falls back
  to the re-check for older rows, labelled as such; the admin health page can
  total flags by cause without reading a log file.
  Refs: WI-418, Publishing\SyncContracts.cs, data-model.md. Depends on: WI-418.

- [x] **WI-419 Put the real logo on the site** (done 2026-08-13 — Dan supplied a
  finished logo kit; header lockup, favicons, app icons, PWA manifest, og:image)
  Goal: the site stops wearing a text placeholder (Dan's ask, 2026-08-13).
  Delivered: `brand/svg` + `brand/png` copied to `wwwroot/img/brand/`,
  `site.webmanifest` at the web root, header shows `lockup-no-tagline.svg`
  (alt "Brain Harbor", never "logo"), favicon SVG + 32px PNG + apple-touch icon
  + manifest + theme-color wired in `_Layout`, and `og:image`/`twitter:image`
  set to the lockup PNG with an ABSOLUTE url (a shared link previously
  unfurled with no image at all). Logo height in rem so it rides the
  large-text scale; print sized in points.
  Kit spec kept at `docs/design/entry-hub-handoff/brand/README.md`.
  Refs: that README (clear space, minimum sizes, don't-dos).
  **Open:** the wordmark spells "Brain Harbor" while the site title, og:site_name
  and domain are "BrainHarbor" — Dan's call which one is the brand (see WI-420).

- [ ] **WI-420 Settle the brand name: "Brain Harbor" or "BrainHarbor"**
  Goal: one spelling, everywhere (Dan's call — surfaced by WI-419).
  Problem: the new logo's wordmark reads **Brain Harbor** (two words) and its
  README specifies `alt="Brain Harbor"`, while the site title, `og:site_name`,
  the footer copy, the RSS title and the domain all say **BrainHarbor**. A
  screen-reader user hears one and a sighted user reads the other on the same
  page. Neither is wrong; they just have to match.
  Acceptance: Dan picks; then the losing spelling is replaced everywhere it is
  user-visible (layout title/og tags, footer, curated pages, RSS/sitemap titles,
  the manifest's `name`/`short_name`, and the logo alt text) with a test pinning
  the choice. The domain does not change either way.
  **Dan's call 2026-08-14: leave the mismatch as it stands for now** — it ships
  live in both spellings and that is accepted, so this is NOT a soft-launch
  blocker. Pick it up when the name is worth settling, not before.
  Note for whoever does it: the alt text is the accessibility half of this, so
  changing the spelling means changing the artwork or accepting that the alt no
  longer matches the picture. Cheapest moment to decide is before anyone links
  to the site in quantity.
  Depends on: nothing.

- [x] **WI-421 Brand the top-right of the home hero** (done 2026-08-14 — Dan
  reviewed the screenshots and approved the size)
  Goal: the branding carries more weight on the front page (Dan's ask,
  2026-08-14, with a screenshot of the hub panel).
  Delivered: `.hub` is a two-column grid — copy left, `lockup-no-tagline.svg`
  top-right in a 24rem column, doors spanning both columns beneath. Collapses
  to one column below 60rem (960px — media-query `rem` is the browser's 16px,
  not the site's 18px base) with the logo hidden, so the "talk to someone now"
  door is not pushed down a phone screen for a logo the header already shows.
  Hidden in print too.
  **The no-tagline lockup on purpose:** `lockup-horizontal.svg` carries the
  tagline "Real brain tumor research, in plain language." as artwork, which is
  word for word the h1 beside it.
  `alt=""` + `aria-hidden` per the brand README's rule for decorative repeats.
  Known trade, accepted: the h1 now wraps to two lines above the breakpoint,
  because the logo takes a third of the row.
  Refs: `docs/design/entry-hub-handoff/brand/README.md`. Depends on: WI-419.

- [x] **WI-422 Say plainly that AI can be wrong, on the home page** (done
  2026-08-14 — Dan's ask, reviewed locally and approved)
  Goal: a reader meets the admission that AI makes mistakes before they meet
  their first AI-written summary, and it is noticeable rather than tucked away.
  Delivered: an `.ai-caution` block between the hub and "Latest updates" —
  "AI can make mistakes. AI writes every summary here. Our safety checks catch
  many mistakes, but they miss some. Always read the study we link to, and talk
  with your care team before you act on what you read."
  Styled with the palette's existing attention treatment (`--color-notice*`,
  the closed-trial fill) rather than a red alarm: the audience is frightened
  enough, and the point is to be read, not to startle. Larger than body text,
  lead sentence on its own line, meaning carried by the words so it survives
  high-contrast mode, a failed stylesheet, and print.
  **Placement is the safety property:** above the feed, never below it. Pinned
  by a test, along with the sentence and both actions.
  Measured at reading grade **4.1** by ContentCheck (the real 6.0 CI gate for
  Razor pages since WI-414). Depends on: nothing.

- [ ] **WI-408 `[user]` Soft launch**
  Goal: first real users.
  Acceptance: shared in 2–3 communities (rules read first) with the honest
  origin story; feedback captured as new backlog items via /pm.
  Depends on: WI-407, WI-409.

---

## Phase P4 — Depth for the brain tumor reader (2026-08-26)

From Dan's ask for ways to improve or expand the site, and a web survey of what
comparable sites actually do. **The survey's headline finding, which shapes this
whole phase: nobody else runs a daily plain-language brain tumor research feed.**
ABTA, National Brain Tumor Society and The Brain Tumour Charity all offer
CareLines, support groups, mentor matching, financial aid and conferences — real
services, none of them this one. The niche is uncontested, so the strategy is to
go DEEPER for this audience rather than broader across cancers (see the volume
measurements under WI-454).

Two other findings worth carrying:

- **The approach is now validated in the literature.** A 2026 blinded randomised
  non-inferiority trial found ChatGPT-4o wrote Cochrane plain-language summaries
  at least as well as human researchers, with a second trial underway
  (PMC12302524; J Clin Epi vol 191). Worth citing on `/how-we-write` — it turns
  "AI writes these" from something to apologise for into something with evidence
  behind it.
- **Forum research names the gap.** "We are a club none of us wanted to join"
  (PMC12119380) found information-seeking clusters around chemo, imaging,
  surgery and fear, and that CAREGIVERS carry high unmet needs. Several items
  below come straight out of that.

Three ideas from the same discussion are already tracked and are NOT duplicated
here: the retraction check (**WI-406**), the glossary (**WI-434**) and the
digest (**WI-404**/**WI-405**).

---

- [ ] **WI-442 Show the reader's own words in the review queue**
  Goal: when a reader tells us a summary is wrong, the person who can fix it
  reads what they said.
  **Problem (found 2026-08-26, reading the code).** The loop is three-quarters
  built and silently broken at the last step. `/research/{slug}` shows a report
  form with a `reason` textarea; `FeedRepository.ReportProblemAsync` stores it
  (`INSERT INTO review_events (action='reported', actor='reader', note)`, capped
  at 1,000 chars) and sets `summary_flagged`. The item duly appears in the admin
  queue — **with no automated reason and no sign of what the reader actually
  wrote.** The only "note" field on `_ReviewRow` is the REVIEWER's correction
  note, an empty input.
  So a reader who spots that a summary says a drug worked when the study says it
  did not gets their message stored in a table nobody reads, and the reviewer
  sees an item flagged for no stated reason.
  This matters more here than it would anywhere else: publishing is Auto, no
  person reads each summary before it goes up, and the reader may be the only
  human who has compared the summary against the source.
  Acceptance: the queue shows every reader report on an item — the note text,
  when it was sent, newest first — and a COUNT of reports (Dan's call
  2026-08-16: count reports, not people; no identity is stored and none should
  start being). Reports are visibly distinct from automated flags. An item with
  a report but no failing check must not read as "nothing wrong here", which is
  what it currently reads as. Notes are rendered as text, never as markup.
  A test proves a reported item surfaces its note to a signed-in reviewer.
  Refs: Pages/Research/Item.cshtml(.cs) (`OnPostReportAsync`),
  Feed/FeedRepository.cs (`ReportProblemAsync`), Admin/ReviewRepository.cs,
  Pages/Shared/_ReviewRow.cshtml, Database/Scripts/0006_reader_reports.sql.
  Depends on: nothing. **Hours, not days.**

- [ ] **WI-443 Rehearse a production database restore**
  Goal: know the backups work before needing them to.
  **Problem.** This is one line inside WI-407's checklist and it is the most
  dangerous item in the whole backlog. The production database holds every
  published summary, the append-only review audit trail, the trials cache and
  now the page-view counts. **A restore has never been performed.** Untested
  backups are not backups; they are an assumption.
  Also worth confirming while in there: the database is a role on Moodathon's
  shared PG17 server (WI-401), so the blast radius of a mistake reaches another
  project.
  Acceptance: confirm what Azure is actually retaining and for how long (PITR
  window, backup frequency); restore prod to a scratch database and verify row
  counts on `aggregated_items`, `review_events` and `page_views`; write the
  steps down in `docs/run-local.md` or an ops note so it can be done under
  stress by someone who is not calm; record how long it took, because "how long
  until we are back" is the question that will actually be asked.
  Refs: WI-401, WI-407. Depends on: nothing. **Do this before WI-408.**

- [~] **WI-444 "What your pathology report says" — ABSORBED into Phase P5
  (WI-508 + WI-509), 2026-08-30. Do not build separately.**
  **Why absorbed.** The P5 research found that this page and the molecular-marker
  page are the same physical document: molecular results are *sections of* the
  pathology report, and under WHO CNS5 the integration is mandatory — the report
  **is** histology plus molecular findings combined into one integrated
  diagnosis. Two pages would misrepresent how the diagnosis is actually made, and
  would leave a report page that is a glossary of headings with the substance
  stripped out. Split instead by LENGTH, not by topic: **WI-508** walks through
  the report, **WI-509** is the anchor-linked marker glossary. The emotional
  content — the wait — went to **WI-507**, which is what keeps WI-508 to a
  manageable size.
  Everything below is preserved because the constraints still govern WI-508/509.
  Original goal: translate the words on the report a patient is holding.
  **Why this one first among the content ideas.** IDH mutation, MGMT promoter
  methylation, 1p/19q co-deletion and WHO grade are not trivia — they are the
  markers that DRIVE treatment decisions under WHO CNS5, and a newly diagnosed
  person is handed them with no translation whatsoever. Johns Hopkins has an
  "Understanding My Report" page; almost nobody else does, and none of it is
  reading-level gated. It is also the single most-searched thing this audience
  types after a diagnosis.
  Acceptance: a curated page (or short set) explaining, in plain words at the
  6.0 gate: WHO grade 1-4 and what "grade" means; IDH-mutant vs IDH-wildtype;
  MGMT methylated vs unmethylated; 1p/19q co-deletion; Ki-67 / proliferation
  index; and "molecular" vs "under the microscope" diagnosis. Each term joins
  the glossary so the tooltips fire across the whole site. Deep-linkable
  (`/pathology#mgmt`) so `/tumors` and feed items can point at the exact term.
  **The hard constraint, and the reason this needs care rather than speed:**
  explain what the WORDS mean, never what the reader's own result means. No
  survival figures, no prognosis, no "this is the good one". The line to hold is
  the same one `/tumors` holds — describe, do not interpret — and it is under
  more pressure here, because these markers genuinely do carry prognostic
  weight and the temptation to say so will be strong. Every page ends with
  questions to ask the care team.
  WHO CNS5 naming throughout (WI-201's lesson: grade 4 glioma ≠ glioblastoma).
  Refs: Content/pages/, Content/glossary/, GlossaryMarker, WI-412, WI-434.
  Depends on: nothing (WI-434 makes it better, not possible).

- [ ] **WI-445 Filter research by molecular marker, not only tumor type**
  Goal: let a reader find research about the disease they actually have.
  Problem: the feed filters by tumor type, but modern neuro-oncology is
  organised by molecular status — "IDH-mutant astrocytoma" is a different
  disease from "IDH-wildtype", with different research, and our taxonomy already
  respects WHO CNS5. A reader whose report says IDH-mutant currently cannot ask
  the feed for that.
  Acceptance: the classifier extracts marker mentions (IDH, MGMT, 1p/19q, EGFR,
  H3 K27M, BRAF) from title+abstract into a closed vocabulary, exactly as it
  already does for tumor tags — closed list, never invented, absent when not
  stated. A marker filter on `/research` alongside tumor type. Markers shown on
  the item page link to WI-444's explanation of them.
  **Do not infer.** A marker is recorded only when the source names it; guessing
  IDH status from a tumor type would be inventing a fact about someone's
  disease, which is the exact failure the numeral post-check exists to prevent.
  Refs: Prompts/classify.md, Content/taxonomy.yml, Feed/FeedRepository.cs,
  Pages/Research/Index.cshtml. Depends on: WI-444 (for the links to land
  somewhere), WI-416 ideally.

- [ ] **WI-446 A caregiver path** *(strengthened by P5's research, 2026-08-30)*
  **New evidence, and an open question for Dan.** P5's research confirms and
  sharpens this item: care coordination and advocacy were raised **almost
  exclusively by caregivers, not patients**; caregivers performed dressing
  changes and gave medications with no formal instruction; several feared
  "offending the physician by asking too many questions"; financial strain
  dominated the forum data. It also found that **every comparable site silos
  caregivers into a "support" section** — none gives them a lane *inside* the
  tumor page itself, which is an unoccupied position.
  **RESOLVED 2026-08-30 — both, and the tumor-page section is prominent.** Dan:
  *"I'd like to make the caregiver section prevalent because a lot of times
  somebody will have surgery and there's a lot of aftercare. Or somebody's living
  with somebody with a tumor and they need to know what they need to deal with."*
  His example — a partner with a tumor who has seizures — produced **WI-559**
  ("what to do when someone has a seizure"), which is life-safety content the
  site did not have at all, and later the same day **WI-560** (living with
  seizures day to day: activities, triggers, water, driving). The caregiver
  block links to both — a caregiver needs the emergency steps AND the everyday
  ones, and it is often the caregiver doing the over-restricting.
  So: **WI-558** sets the standard and the shared block, every P5 tumor hub
  carries a section, treatment pages with real aftercare carry one too
  (WI-510 heaviest), and **this item is now the fuller path they all link into**
  rather than the only place caregiver content lives. Re-scope accordingly when
  it is picked up — the "seizures and what to do" line below is now WI-559's job.
  Goal: speak to the other person in the room.
  Problem: every page on the site addresses the patient. Forum research
  (PMC12119380) found caregivers of people with high-grade glioma carry high
  unmet support needs, and they are often the ones doing the reading — the
  person with the tumor may have trouble concentrating, which is the premise
  the whole site is built on.
  Acceptance: a `/caregivers` path covering what to expect, going to
  appointments and what to write down, seizures and what to do, personality and
  cognitive change (the thing carers say blindsides them), practical help, and
  looking after yourself — including permission to. Links to CancerCare's free
  caregiver social workers and the ABTA CareLine. Curated pages at the 6.0 gate;
  no pipeline work. Reachable from the nav or the home page doors, not buried.
  Refs: Content/pages/, docs/sitemap.md. Depends on: nothing.

- [ ] **WI-447 "What else has been found on this"**
  Goal: stop a single study reading like a verdict.
  Problem: every item stands alone. One mouse study, presented by itself in
  confident plain language, reads as news — which is the exact false-hope
  failure the anti-hype rules, the stage badges and the journey path all exist
  to prevent. The one thing that reliably deflates a lone finding is the six
  that came before it.
  Acceptance: each item page shows a few related items — same tumor tag, and
  ideally same marker once WI-445 lands — with their journey stage visible, so
  a reader sees where this one sits. Ordered to be useful rather than flattering
  (a human trial on the same topic outranks another mouse study). Says plainly
  when there is nothing related rather than padding with loosely-matched items.
  Costs no pipeline work: the tags, stages and dates are already stored.
  Refs: Feed/FeedRepository.cs, Pages/Research/Item.cshtml, WI-437.
  Depends on: nothing.

- [ ] **WI-448 The appointment print-out**
  Goal: build the thing people are already printing.
  Problem: `print.css` says twice, in its own comments, that patients print
  these pages for appointments — WI-437 and WI-438 both went out of their way to
  keep the journey path and the pager legible on paper. The site has been
  designed *for* printing without anything being designed *as* a print-out.
  Acceptance: a per-item view (or print stylesheet variant) carrying the plain
  title, what it means and what it does not, the stage in words, the full source
  citation with its URL written out, the date, and blank ruled space for the
  reader's own questions. No site chrome, no navigation, no dead controls.
  One page where it fits on one page.
  Refs: wwwroot/css/print.css, Pages/Research/Item.cshtml. Depends on: nothing.
  **Small.**

- [ ] **WI-449 Plain-English trial eligibility**
  Goal: tell someone whether a trial is even worth asking about.
  Problem: `/trials` lists trials and links out; the eligibility criteria that
  decide whether a person can join are left in registry language, which is
  where most readers stop. Leal Health and Massive Bio do AI trial MATCHING —
  a regulated, clinical, well-funded space we should stay out of — but nobody
  translates the fine print, and we already fetch it.
  Acceptance: for each cached trial, a plain-language rendering of the key
  eligibility facts a reader can self-check: age range, whether it is for newly
  diagnosed or recurrent disease, prior-treatment requirements, and required
  molecular status. Same guardrails as summaries (sources-only, no invented
  numerals, banned phrases, reading gate).
  **Framing is the whole risk here.** This must never read as "you qualify" —
  it is "here is what the trial says it is looking for, ask your team". The
  existing "talk to your care team before you contact a trial" panel stays and
  gets stronger. A trial that has closed must not get a friendly eligibility
  summary (WI-402's lesson).
  Refs: Sources/CtGovFetcher.cs, Trials/, Prompts/. Depends on: nothing.

- [ ] **WI-450 Finish `/start` as a real first-week path**
  Goal: the page for the worst day should not describe itself as a draft.
  Problem: `/start` says, in its own copy, "This is the first, short draft of
  this page. A fuller step-by-step guide is coming." It is the primary door on
  the home page and the destination for anyone searching "just diagnosed brain
  tumor".
  Acceptance: a sequenced path rather than one page — the first 48 hours, the
  first week, the first month; what happens at each step (scans, surgery or
  biopsy, pathology, the treatment plan conversation); what to bring, what to
  ask, who to call. Links into **WI-507** (waiting for pathology) and **WI-508**
  (the report itself) at the pathology step — WI-444 was absorbed into those —
  and WI-446 for the person who came with them. The emergency block stays
  exactly where it is, at the top, unchanged. The "this is a draft" line goes
  only when it is no longer true.
  Refs: Content/pages/start.md, WI-436. Depends on: WI-507/WI-508, WI-446
  ideally.

- [ ] **WI-451 Survivorship and late effects** *(re-scoped 2026-08-30 by P5, twice)*
  **Scope change:** Phase P5's **WI-521** now owns follow-up scans, the
  surveillance rhythm, RANO, pseudoprogression, radiation necrosis and
  scanxiety. **WI-560 now owns seizures day to day — triggers, activities,
  water and driving.** This item keeps fatigue, memory and concentration,
  returning to work, and the years-long picture. Do not write scan content here
  (link to WI-521) and do not write driving rules here (link to WI-560).
  Goal: the site currently stops at treatment.
  Problem: nothing covers life after it — fatigue, seizures and what they mean
  for driving, memory and concentration, returning to work, scan anxiety,
  follow-up schedules. For low-grade glioma and meningioma especially, that
  period is measured in years and is where most of a reader's life with this
  actually happens.
  Acceptance: curated pages at the 6.0 gate covering the above, with the
  driving/seizure page especially careful — rules are jurisdictional, so it must
  say "your rules depend on where you live, here is who to ask" rather than
  stating any. Practical, not cheerful.
  Refs: Content/pages/, docs/sitemap.md (P2d). Depends on: nothing.

- [ ] **WI-452 Spanish**
  Goal: the largest expansion available in WHO the site helps.
  Roughly one in five people in the US speaks Spanish at home, NCI publishes in
  Spanish so the sources exist, and the pipeline already writes to a reading
  target — a second language is a prompt plus a second grader, not a rewrite.
  **The catch, and it is a real one:** Flesch-Kincaid is English-only. A Spanish
  reading gate needs INFLESZ or Fernández-Huerta, which means the "≤6th grade"
  promise has to be re-expressed rather than translated. And a golden set in
  Spanish needs a Spanish speaker; shipping unreviewed medical Spanish to
  frightened readers on the strength of an automated score would be worse than
  shipping nothing.
  Acceptance: decide scope first (curated pages only, or summaries too — the
  latter doubles pipeline cost per item); `lang` handling and hreflang;
  Spanish-appropriate readability gate wired into ContentCheck; a Spanish golden
  set; a way to switch that survives with JavaScript off.
  Refs: tools/BrainHarbor.ContentCheck/ReadabilityAnalyzer.cs, WI-414, WI-416.
  Depends on: WI-416 (one grader, not two — do not fork a third).
  **Weeks. A project, not an evening.**

- [ ] **WI-454 `[user]` PARKED: a sister site for cancer generally**
  Dan asked (2026-08-26) whether the platform could serve all cancers, then
  said to hold off. Parked here so the research is not lost and does not have to
  be redone.
  **The data is not the obstacle — you already have all of it.** Every source is
  a general cancer source narrowed by a query string: PubMed E-utilities (MeSH
  query), ClinicalTrials.gov v2 (condition query), medRxiv/bioRxiv (subject
  filter), ScienceDaily (a `cancer.xml` feed exists alongside `brain_tumor.xml`,
  60 items when checked), and the NCI news feed **which is already all-cancer
  and unfiltered**. Roughly four string constants.
  **Volume is the obstacle.** Measured 2026-08-26, PubMed, 30 days:
  | Scope | Records | Per day |
  |---|---|---|
  | Brain tumor (today) | 470 | ~16 |
  | All cancer (`Neoplasms`[MeSH]) | **8,331** | **~278** |
  | Breast alone | 850 | ~28 |
  | Lung alone | 665 | ~22 |
  Recruiting trials: 900 brain vs **19,129** all-cancer. That is ~18× through a
  Claude Code CLI on Dan's PC, and it makes the local-model question
  (see the 2026-08-21 discussion) load-bearing rather than optional. Breast
  cancer ALONE is nearly twice BrainHarbor's entire current scope.
  Two more costs: `taxonomy.yml` would grow from 24 types to hundreds, and it is
  rendered into the classifier prompt on every call, so it is a per-item token
  cost. And the editorial surface — staging systems, screening guidance, drug
  classes across dozens of diseases — is judgment that does not scale the way
  the automated guardrails do.
  **The strategic argument against, recorded because it is the real reason:**
  plain-language all-cancer information is crowded and well funded (NCI, ACS,
  Cancer Research UK, Macmillan, all with medical staff). BrainHarbor's edge is
  that nobody was doing daily plain-language brain tumor research. A general
  site would compete head-on with cancer.gov — which is one of the sources it
  would be summarising.
  **If this is ever revisited, the better shape is a second UNDERSERVED specific
  cancer** (pancreatic, sarcoma, cholangiocarcinoma, rare/orphan) where the same
  gap exists and volume stays in the tens per day. Same code, new query strings,
  new taxonomy.
  Acceptance: a decision, not a build. If yes, first step is measuring candidate
  volumes against current pipeline capacity.
  Depends on: the local-model question. Blocks nothing.

- [x] **WI-462 Country counts follow the other filters, live**
  (done 2026-08-29 — Dan: "the numbers next to each country aren't updated when
  I select a tumor type or stage of testing")
  **The bug.** `AvailableCountriesAsync` was its own query that considered only
  `includeClosed`, so picking a tumor type left every count showing all-tumor
  numbers. The menu promised 27 and the list delivered 4. **A count that
  disagrees with the list it produces is worse than no count** — it is the one
  thing on that control a reader might act on.
  Dan offered two options — hide the numbers, or update them — and preferred
  updating, live on change. Taken, but not by patching the second query:
  **both the counts and the list now build from one shared `BuildFilter`**, so
  they cannot drift apart again by construction. The test asserts the AGREEMENT
  rather than fixed numbers — for four filter combinations, what the menu
  promises equals what the list returns. That is the actual property; a fixed
  number would pass while the two silently diverged.
  **Two judgment calls:**
  - The counts deliberately ignore the COUNTRY selection (`includeCountryFilter:
    false`). They answer "how many are in this country given your OTHER
    filters"; folding the country in would be circular — picking Finland would
    drop every other country to zero and a reader could never widen their
    search from the menu. Pinned by its own test.
  - The live refresh fires on tumor type, stage and include-closed — **not the
    country checkboxes**. Re-rendering on each tick would close the picker under
    the reader's hand mid-selection. Countries still apply on the button, which
    is also the whole no-JavaScript path.
  htmx `hx-trigger="submit, change from:#tumorType, change from:#phase, change
  from:#includeClosed"`. Verified against the real cache: all → 311 (US 210);
  glioblastoma → 88 (US 64); glioblastoma + Phase 3 → 5 (US 4), and ticking US
  then listed exactly 4.
  Refs: Trials/TrialsRepository.cs (`BuildFilter`), Pages/Trials/Index.cshtml(.cs).

- [x] **WI-461 Trials page: sections, headings, and the search button**
  (done 2026-08-29 — Dan, reviewing locally)
  "Trials near you" and "Trials by countries" are two different jobs and ran
  together as bare headings on a white page. Each is now a tinted panel holding
  **only its heading and its controls** — the results sit below on the page
  background, so the panel reads as "here is how you ask" and the cards as
  "here is the answer". Cards stay white inside, so the nesting reads.
  Headings renamed to match each other: "Browse trials" → "Browse trials by
  countries" → **"Trials by countries"**, since "Trials near you" above does
  not say "browse" either. Naming the country in the heading is also what tells
  a reader outside the US that this page has something for them; the ZIP box
  does not. Button "Show trials" → **"Search trials"**.
  **The wiring detail worth knowing:** the htmx swap region wraps BOTH the
  panel and the results even though only the panel is tinted. It has to —
  submitting must update the country summary inside the form as well as the
  trials. Verified the cards and count line sit outside the tint, the form
  inside it, and paging still holds scroll position.
  Refs: Pages/Trials/Index.cshtml, wwwroot/css/site.css (`.trial-section`).

- [x] **WI-460 Trials off the research feed, and stacked research filters**
  (done 2026-08-29 — Dan: "remove trials from the kind drop-down and don't show
  trials in the lists, since we're going to just show them separately")
  `/research` no longer returns trial updates, and "Trials" is gone from the
  kind filter — leaving it would have offered a filter whose results are
  deliberately empty. A link shared before this carrying `?kind=trial_update`
  falls back to the whole feed rather than an empty page the reader cannot
  explain; pinned by a test.
  New `FeedQuery.ExcludeTrials`, set only by `/research`. **The home page still
  shows trials in Latest updates — Dan's call, asked and answered ("Join trials
  on the home page is fine").**
  Filters restacked to match `/trials`: Tumor type, **Research kind**, Sort by,
  the early-stage toggle, then Apply at its own size. Label above each control,
  each pair its own block so a wrap can never separate them — the fault
  `/research` had before WI-440.
  Refs: Feed/FeedRepository.cs, Pages/Research/Index.cshtml(.cs).

- [x] **WI-459 Trials as cards** (done 2026-08-29 — Dan: "it just doesn't look
  that great")
  The trial list was rows separated by hairlines; it is now the same card and
  grid the research feed uses, minus the photo. Status and phase lead, then the
  title, then a sentence or two, then where it runs — pinned to the bottom so
  the meta lines up across a row however long the titles are.
  **What the description had to be.** Checked before building: in the LOCAL dev
  database zero of 518 cached trials had a plain-language summary, so the blurb
  would have been blank on every card there.
  **Correction, measured on production after deploying:** 54 of 60 trials
  across three pages DO have our plain-language summary — the registry fallback
  fires on roughly 10%, not 100%. The original claim generalised one database
  to the system and was stated with more confidence than it had earned. The
  code is right either way; the fallback covers the gap and gets out of the way
  when a summary exists. It falls back to the
  registry's brief summary, labelled **"From the trial team:"** — this site's
  rule is that ClinicalTrials.gov's words are visibly theirs. When plain-language
  trial summaries do land they take precedence and the label disappears.
  The fallback **cuts at a sentence end, never mid-sentence**. This is clinical
  prose, and truncating "...did not improve survival" halfway is how a card
  comes to say the opposite of the study. If no sentence break falls inside the
  220-character budget it takes the whole first sentence however long — a
  taller card beats a misleading one.
  Refs: Trials/TrialsRepository.cs (`CardBlurb`), Pages/Trials/Index.cshtml.

- [x] **WI-458 Country picker: layout, closing, and in-place updates**
  (done 2026-08-29 — Dan, after using WI-457)
  Three complaints, all fair. The picker **auto-opened whenever a filter was
  active**, so it stayed sprawled across the page after every search and had to
  be dismissed by hand — my call in WI-457, and the reasoning ("a shared link
  should explain itself") did not survive contact with actually using it,
  because you submit far more often than you follow a shared link. It now
  always starts closed and the summary line carries the state. The control
  moved up beside the other two filters and the button moved below them.
  **Submitting no longer jumps to the top:** the browse form had no htmx at all
  and was doing a full page load. It now swaps in place, and the pager gets the
  `HxTarget` the partial was built to take and had been passing null.
  Labels above controls, "Select" until countries are picked then up to three
  then "...", checkbox above the button, button sized to its words.
  **Two corrections to my own work, both from measuring instead of trusting
  myself.** I first "fixed" the scroll jump with `show:none` and wrote a comment
  claiming that was the fix — it was not; **my test was causing the jump**,
  because Playwright scrolls an element into view before clicking it. And I
  thought the country control looked washed out beside the selects; computed
  styles said identical border, width, colour and background. I had been
  looking at a stale screenshot.
  Refs: Pages/Trials/Index.cshtml(.cs), wwwroot/css/site.css.

- [x] **WI-457 Pick several countries, and point research readers at trials**
  (done 2026-08-29 — Dan, after using WI-455: "I may want to search in China
  and Japan and a few other countries", plus "on the research page… make it
  clear they can go to the trials page")
  **Multi-select.** The single dropdown could not express "China and Japan",
  which is what a reader comparing options actually wants. Now a checkbox group:
  `?country=China&country=Japan`, ORed — several countries mean EITHER, not
  both, because a trial has to run somewhere the reader can reach, not
  everywhere they ticked. The URL key did not change, so single-country links
  shared before this still work (pinned by a test).
  **Why `<details>` and not a widget.** The site works with JavaScript off, so a
  JS multi-select was out; `<select multiple>` means ctrl-click on desktop and a
  cramped scroller on a phone. A disclosure gives keyboard handling and expanded
  state for free, and it is the pattern the mobile nav already uses. Collapsed
  by default (44 countries would otherwise push the trials off screen), opened
  automatically when a filter is active so a reader following a shared link sees
  WHY the list is short. The summary states the selection without opening.
  Ordered by trial count, so the countries worth picking are at the top rather
  than buried under alphabetical accident. Three columns on desktop, one on a
  phone — two would put wrapped labels beside checkboxes, and tap accuracy
  matters more than scroll length for this audience. A "clear countries" link,
  because unticking forty boxes by hand is not a reasonable ask.
  **Research → trials signpost.** `/research` and `/trials` answer different
  questions — "what has been found" versus "what could I join" — and a reader
  who lands on the feed looking for the second had no way to know. Placed ABOVE
  the filters, not below the feed: nobody should read twenty research cards
  before discovering they are on the wrong page. A test pins that ordering.
  Verified against the real cache: China+Japan returns 38 where the two alone
  are 32 and 10, so overlapping trials are counted once rather than duplicated.
  837 tests, ContentCheck 50/0. Screenshotted desktop and phone.
  Refs: Trials/TrialsRepository.cs, Pages/Trials/Index.cshtml(.cs),
  Pages/Research/Index.cshtml, wwwroot/css/site.css.

- [x] **WI-455 Filter TRIALS by country** (Dan, 2026-08-26 — "sorted by country
  as well, with the ability to filter by country". **Next up.**)
  Goal: a reader outside the US, or in a particular country, can see the trials
  they could actually reach.
  **The data is already here.** `trials_cache.locations` is jsonb holding one
  entry per site with `{facility, city, state, country, lat, lon}` — verified
  against real cached rows. Nothing new needs fetching; CtGov already returns it
  and the near-me search already reads it.
  Acceptance: a country filter on `/trials` beside tumor type and phase, listing
  only countries that actually appear in the cache (never a hard-coded world
  list — an empty filter option is a dead end a reader has to discover by
  clicking); the count of trials per country visible so the control is
  informative before it is used; combines with the existing filters and with
  paging (WI-438), and rides the URL so a filtered view can be shared; works
  with JavaScript off.
  Sorting: group by country, then the existing order within a group.
  **Two things to get right, both about not misleading someone:**
  - A trial with sites in eight countries is not "a US trial". Filtering must
    match ANY of its locations, and the card should say how many countries it
    runs in rather than implying one.
  - This must not quietly become "trials near you" for people outside the US.
    The ZIP box is US-only (ZCTA centroids); a reader in Germany filtering to
    Germany still gets no distance, and the page should not pretend otherwise.
  Query shape: `locations @> '[{"country": "..."}]'` with a GIN index on
  `locations`, or a normalized country array column populated on sync if the
  jsonb containment query proves slow at 8,900+ rows.
  Refs: Database/Scripts/0007_trials_cache.sql, Trials/TrialsRepository.cs,
  Pages/Trials/Index.cshtml, Sources/CtGovFetcher.cs. Depends on: nothing.
  **A day, roughly.**

  **Done 2026-08-29.** Country filter on `/trials`, built from the cache (41
  countries with trial counts), matching ANY site. GIN index
  (`jsonb_path_ops`) on `locations` since the filter unnests on every browse.
  **The multi-country trap was real, not theoretical:** of the 20 trials under
  Germany in the live cache, **17 run in more than one country** and the first
  is a 14-country study. Matching only the first location would have hidden or
  mislabelled most of them; cards say "Runs in N countries" instead of implying
  one. Menu counts are trials not sites (a trial with 40 US hospitals is one),
  and honour the closed filter so a count of 12 never leads to a list of 3.
  Non-US readers are told the ZIP box is US-only and pointed here, with the
  honest caveat that it gives no distances. `?country=Wakanda` falls back to
  every country. Verified against the real 8,900-trial cache: menu count and
  filter result agree exactly; 0.02s filtered. 830 tests, ContentCheck 50/0.
  **Two stale comments fixed while in here, both mine.** `PageUrl` claimed it
  deliberately strips the reader's ZIP from pager links — it does not, it is
  built from `FilterUrl` which appends one. A false comment about a privacy
  property is worse than none; corrected to describe reality, with the real
  open question noted (the handler sets `no-store`/`no-referrer` because the
  URL carries a location, but that does not stop a reader SHARING the URL —
  worth deciding deliberately, not changed here). And `StateSummary` was
  documented as "US states"; with non-US trials now surfacing, a card can
  legitimately read "Gelderland, Rome" — the registry's own words, which is the
  rule this page follows everywhere.

  **Country on RESEARCH items: decided against, 2026-08-26.** Dan asked for both
  and then dropped the research half once the cost was clear. Recorded so it is
  not re-proposed: nothing in the pipeline knows where a paper came from —
  `FetchedItem` carries no affiliation at any layer — so it would need PubMed's
  `AffiliationInfo` parsed out of the EFetch XML, a country inferred from free
  text ("…Boston, MA 02114, USA" vs "…Beijing, China" vs nothing at all, often
  first author only), a new column, a filter and a backfill of 1,000+ published
  items. And the payoff is weak: for a trial, country answers "can I get to it";
  for a paper it is where the authors work, which tells a reader little about
  whether the finding applies to them. Asking the classifier to guess it from
  the abstract is not an option — that is the invented-fact failure the numeral
  post-check exists to prevent.

- [ ] **WI-453 `[user]` Pediatric brain tumors: section, or sister site?**
  Goal: decide before building either.
  The audience is not the patient — it is a parent. Different tumors
  (medulloblastoma, DIPG, ATRT, ependymoma), different vocabulary, different
  fears, and a voice this site does not currently have. The taxonomy already
  carries the types and the pipeline needs nothing, so the question is
  editorial, not technical: a `/children` section speaking to parents inside a
  site written for patients, or a separate site on the same platform.
  My read: separate. The voice difference is the whole thing, and one site
  trying to speak to both will speak well to neither. But it is Dan's call and
  it is not urgent.
  Acceptance: a decision, written down here, with the reason.
  Depends on: nothing. Blocks nothing.

---

## Phase P5 — Tumor guides: what you should know about the one you have (2026-08-30)

From Dan's ask: for every tumor type, research everything a newly diagnosed
person should know — what it is, how it starts, what it does to the brain — plus
the treatments and, specifically, **the tests you have to get through before each
treatment**. Five parallel research tracks ran on 2026-08-30; the reports are
committed at **`docs/research/tumor-guides/`** and are the source material for
every item below. **Read `SYNTHESIS.md` first** — it holds the page inventory,
the resolved conflicts and the rulings.

**The strategic finding, which is why this phase is worth 60 items.** The
readability gap is measured and published: across 91 US brain tumor centers and
8 patient organizations, mean Flesch-Kincaid grade level is **11**; fewer than
10% of center sites reach 8th grade and **no patient organization does**
(`academic.oup.com/nop/article/12/5/901/8124733`). A separate study of 180
documents from 50 academic centers found median FKGL 12.5, with treatment
content *harder* than diagnostic content. A brain tumor library that actually
holds 6.0 would be, on the published record, the first. Same shape as P4's
"nobody runs a daily plain-language research feed" — an uncontested niche.

**Architecture: three layers, each written once.** 24 tumor hubs at
`/tumors/<slug>` (18 deepened in place, 6 new), a **treatment library** of 17
pages, a **tests library** of 12 pages. A tumor hub says which treatments and
tests apply to *that* tumor and how they differ there, then links in. Writing
treatments into each tumor page instead would be 24× the work and 24 places to
fix an error.

### Decisions already taken (do not re-open per item)

Dan's calls, 2026-08-29/30:

- **Structure:** shared treatment + tests libraries, tumor hubs link in.
- **Prognosis:** explain what *grade*, *median survival* and *five-year survival*
  MEAN and why a population number does not predict one person — **publish no
  figures.** This is a deliberate change from `/tumors`' current line and it is
  validated: prognosis disclosure requires negotiation and readiness-checking
  across visits, which a page cannot do, and 25 newly diagnosed glioma patients
  produced *"not all patients want to know it all, one size does not fit all."*
  Hence the reader-choice gate (WI-503).
- **Depth:** all 24 types at full depth.
- **Existing pages:** deepen in place, same URLs.
- **Drugs:** name them, say what they do, describe common side effects. No doses,
  no mg, no cycle schedules.
- **Tests:** their own pages, because one scan serves diagnosis, planning AND
  follow-up.
- **`all-brain-tumors`** becomes the general "we don't have a name for it yet"
  page.
- **Navigation:** keep the grouped index, add an "I was diagnosed with…" picker.
- **R1 — orienting durations: IN.** "Radiation is usually Monday to Friday for
  about six weeks", "Optune at least 18 hours a day" (that one IS the decision),
  "staples out roughly 1–2 weeks". Lomustine reworded to carry the reason rather
  than the interval: *"taken only occasionally, not every day, because it lowers
  blood counts for weeks after you take it."* All Gy and mg figures stay out.
- **R2 — procedural risk percentages: qualitative.** "Bleeding is uncommon, but
  it is the main risk." The source spread is too wide to state honestly —
  awake-craniotomy seizure is reported anywhere from 2.9% to 54%.
- **R3 — the two numbers most likely to mislead: direction only.** No percentages
  for the whole-brain-radiation cognitive figures (most people in **both** arms
  declined, so the raw numbers mislead in the reader's favour) and none for the
  ~90% glioblastoma relapse rate — *"recurrence is expected and is planned for"*
  carries the useful part.

### Shared acceptance contract

**Every content item in this phase must satisfy all of the following.** Stated
once here so 60 items do not each repeat it; item-level Acceptance lists only
what is specific to that page.

1. Reading grade **≤ 6.0** measured by ContentCheck (CI-gated).
2. **Sources-only.** Every substantive claim traceable to a source in the
   research reports or a source added and cited in the page's `sources` front
   matter. No invented facts, no invented numbers.
3. **WHO CNS5 naming and grading throughout.** Arabic numerals, grading within
   tumor type. Where an older name has been retired, say so — readers arrive
   holding old paperwork.
4. **NCI patient PDQ is pre-CNS5 and must not be used for naming, grading, or
   brain-metastasis radiation recommendations.** It still says "anaplastic
   astrocytoma", "oligoastrocytoma" and "hemangiopericytoma", uses Roman
   numerals, and predates both ASCO-SNO-ASTRO 2022 and vorasidenib. Use it for
   supportive-care and general framing language only. Take naming from
   CNS5/cIMPACT-NOW. Same caution for ABTA's legacy PDFs and StatPearls'
   oligodendroglioma chapter, both of which carry retired terms.
5. **No prognosis figures.** No survival statistics, no median survival, no
   five-year rates, anywhere, in any page.
6. **Never AHFS or MedlinePlus drug monographs; never NCI embedded images**
   (licensing — PLAN.md §5). Also: replace the manufacturer's site
   (avastin.com) as a side-effect source before publishing.
7. Ends with **questions to ask your care team**.
8. Front matter carries `sources` (with `accessed` dates), `reviewed`,
   `review_due`, and `disclaimers: [medical]`.
9. New vocabulary joins the glossary so tooltips fire site-wide.
10. Existing `/tumors/*` URLs are preserved. No redirects, no renames.
11. **Every tumor hub carries a caregiver section** — "for the person caring for
    someone with this" — and it is a real section, not a footnote. **Dan's call,
    2026-08-30**, with the reason: after surgery there is a lot of aftercare, and
    someone living with a person who has a tumor needs to know what they will
    have to deal with. His own example: a partner with a tumor who has seizures.
    Every comparable site silos caregivers into a separate "support" area and
    none gives them a lane inside the tumor page, so this is also an unoccupied
    position. See WI-558 for the standard and WI-446 for the fuller path.
12. **Every treatment page with meaningful aftercare carries a caregiver
    section too** — what the person at home actually has to do, what to watch
    for, and when to call. Craniotomy (WI-510) is the load-bearing case.

### Wave 0 — foundations

Small, and everything downstream inherits it. Do not start Wave 1 first.

- [x] **WI-501 Shared content blocks for curated pages** *(done 2026-08-30)*
  Goal: write the repeated blocks once so 24 pages cannot drift apart.
  Problem: curated content is flat Markdown with **no include mechanism**. Six
  blocks recur across the tumor hubs — `[CROSSWALK]` (retired names),
  `[CAUSES]` (the self-blame block), `[MECHANISM]` (how a tumor causes
  symptoms), `[MARKERS]`, `[TREATMENTS]`, `[PROGNOSIS-WORDS]`. Copy-pasting
  them means 24 copies of the content **most likely to need correcting later**:
  the crosswalk changes every time WHO or cIMPACT-NOW moves.
  Acceptance: an include directive resolved by `ContentStore`; blocks under
  `Content/blocks/`; **ContentCheck grades the composed page, not the fragment**
  (a fragment graded alone can pass while the assembled page fails); glossary
  markers and site search see composed text; a test proves editing one block
  changes every page that includes it; a missing block fails the build rather
  than rendering an empty section.
  Refs: `Content/ContentStore.cs`, `tools/BrainHarbor.ContentCheck/`,
  docs/research/tumor-guides/SYNTHESIS.md §3.2. Depends on: nothing.
  **Done.** Directive is a whole line reading `[BLOCK-NAME]`; blocks live in
  `Content/blocks/`, ship EMPTY. ~~WI-513 writes the crosswalk together with the
  page that includes it.~~ **Superseded 2026-09-07 (Dan's call): `[CROSSWALK]`
  is WI-514's, per that item's own description.** WI-513 wrote a page-specific
  slice as inline prose (`low-grade-glioma.md`, the oligoastrocytoma /
  diffuse-astrocytoma / Roman-numeral lines) and did NOT create
  `blocks/crosswalk.md`. The two-layer split is the decision: **the shared block
  carries the CNS5-wide renames every hub owes, page-specific slices stay on
  their own pages on top of it.** A block may carry `sources` front matter, which
  merges into every including page — otherwise the drift problem just moves to
  the citation list. Mechanism documented in content-pipeline.md §3a.
  **Two review findings worth carrying into Wave 1:** matching must be line
  based, not one regex over the document (`core.autocrlf=true`, and .NET's
  multiline `$` will not match before `\r`, so on a fresh clone every directive
  rendered as literal bracket text with no error while CI stayed green); and a
  block must be spliced with blank lines around it, or the crosswalk TABLE
  fuses into the neighbouring paragraph as pipe-mangled prose.

- [x] **WI-502 The tumor-guide editorial standard** *(done 2026-08-30)*
  Goal: write the rules down where a future session will find them, not in a
  chat log.
  Acceptance: a new section in `docs/content-pipeline.md` covering the standard
  17-section order (WI-506's template), the shared acceptance contract above,
  the R1/R2/R3 rulings with their reasoning, and — most importantly — the
  **source-precedence rule** (§4 of the contract: which source governs naming vs
  framing vs radiation recommendations, and which named sources carry retired
  terminology). A page that cites NCI for a tumor name is a defect; that has to
  be findable.
  Refs: docs/content-pipeline.md, docs/research/tumor-guides/SYNTHESIS.md §7.1.
  Depends on: nothing.
  **Done — `docs/content-pipeline.md` §12**, appended rather than inserted
  because §2/§3/§4/§5/§6/§9/§10 are cited by name from CLAUDE.md, the backlog
  and the startup references, and renumbering would break all of them.
  Covers: source precedence (§12.1), the 12-item shared contract (§12.2), the
  17-section order with the reason for each position (§12.3), R1/R2/R3
  (§12.4), prognosis-without-figures and the gate (§12.5), and the
  page-specific writing rules (§12.6).
  **Two findings from reading the NCCN guideline rather than the research
  summary of it.** (1) It **is** CNS5-aligned — checked its actual vocabulary:
  Arabic grades throughout, `IDH-mutant`, and neither "anaplastic astrocytoma"
  nor "oligoastrocytoma" appears anywhere. That makes the precedence rule
  clean: NCCN governs naming and grading, NCI patient PDQ is framing-only.
  (2) **It is NOT "licensing-clean"**, which is what the research reports call
  it three times. Its copyright page forbids reproduction of text or
  illustrations in any form without written permission. Read it, write our own
  sentences, cite it. §12.1 says so explicitly, because a drafting session with
  a 76-page plain-language PDF open is exactly where phrasing gets borrowed
  without anyone deciding to.

- [x] **WI-503 Reader-choice gate for the outlook section** *(done 2026-08-30, PR #70)*
  Shipped as a Markdig custom container, `:::outlook`, because curated pages
  render with `DisableHtml()` — a `<details>` typed into a .md file renders
  escaped, so this could never have been "just write the HTML". Authoring
  syntax and the three rules that follow from it are in
  `docs/content-pipeline.md` §12.5.
  **The safety property is that a typo fails rather than falls open.** Markdig
  renders an unknown `:::name` as an anonymous `<div>`, so `:::outlok` would
  have published the outlook section wide open, with no error and a green
  build. `ReaderGate.Validate` fails the page by name instead, and ContentCheck
  turns that into a build failure for free.
  Goal: let a reader decide whether to read the frightening part.
  The evidence says some patients want full honesty, some want generalities, and
  some want only positive information — so outlook sits at position 12, behind
  an explicit choice: *"The next part is about outlook. Some people want to read
  it. Some people would rather not. You can skip it and come back another day.
  Nothing else on this page depends on it."*
  Acceptance: a disclosure component usable from curated Markdown; **works with
  JavaScript off** (same `<details>`/`<summary>` approach as the mobile nav and
  the country picker); closed by default; keyboard reachable; axe clean at
  desktop and 390px; readable in print. A test pins that it renders closed.
  Refs: `Pages/Shared/`, wwwroot/css/site.css, WI-440 (disclosure pattern),
  docs/research/tumor-guides/SYNTHESIS.md §5. Depends on: nothing.

- [x] **WI-504 `[user]` Fetch the sources that block automated access** *(done 2026-08-30 — Wave 1 UNBLOCKED)*
  Goal: get the best available source into the repo.
  Three separate research tracks independently named **NCCN Guidelines for
  Patients: Gliomas** as the best CNS5-aligned, patient-level, licensing-clean
  source available — and it returned HTTP 403 to every automated fetch, as did
  ABTA, The Brain Tumour Charity, Johns Hopkins "Understanding My Report", and
  the RSNA fMRI review. The NCCN patient guideline is also the best available
  model for the "questions to ask" blocks.
  Acceptance: Dan opens them in a browser and saves them to
  `.claude/work_files/research/sources/` (git-ignored — they are third-party
  documents, not ours to commit). A note here listing what was retrieved.
  Depends on: nothing. **Blocks Wave 1.**

  **Retrieved 2026-08-30**, in `.claude/work_files/research/sources/`:
  - `brain-gliomas-patient.pdf` — **NCCN Guidelines for Patients: Brain Cancer
    — Glioma, 2024.** ~76 pages. The blocker; Wave 1 is now unblocked. Also the
    model for the "questions to ask your care team" section every P5 page ends
    with.
  - `kurokawa-et-al-2022-...-who-classification...pdf` — RadioGraphics
    `10.1148/rg.210236`, Kurokawa et al. **"Major Changes in 2021 WHO
    Classification of CNS Tumors."** The single best crosswalk source for
    WI-501's `[CROSSWALK]` block: CNS5 renames, cIMPACT-NOW, Roman→Arabic.
    **Note for anyone re-reading the research reports:** `tests-library.md`
    §13 calls the blocked RSNA item "the RSNA fMRI review". That is a DIFFERENT
    article whose URL was never captured. `rg.210236` is the CNS5 paper, filed
    correctly under "WHO CNS5 classification and grading" in
    `glioma-family.md`. Do not deprioritise it on the strength of the tests
    report's label.
  - `jhu-understanding-my-report.pdf` — Johns Hopkins Pathology, the model for
    WI-508. Its sample report reads *"Glioblastoma, IDH wildtype (WHO grade
    IV)"* — a Roman numeral, retired by CNS5 in 2021 precisely to stop
    II/III/IV transcription errors. Useful **because** it is dated: it is a
    real example of the confusion WI-508 has to walk a reader through.
  - `what-causes-brain-tumours.pdf` — The Brain Tumour Charity. Feeds
    `[CAUSES]`. Carries the line that block exists for: only ~3% of UK brain
    tumours are thought preventable, and *"there's nothing you could have done,
    or not done, to prevent brain cancer."* **The URL in the research reports
    is dead** — the live one has an extra `/brain-tumour-biology/` segment:
    `.../how-brain-tumours-are-diagnosed/brain-tumour-biology/what-causes-brain-tumours/`
  - **ABTA pages: deliberately NOT retrieved.** Their naming is pre-CNS5, and
    §4 of the shared contract already bars them from governing naming or
    grading. That leaves tone and "what patients ask", which the NCCN guideline
    covers better. Recorded as decided-against so it is not re-proposed.

  **All four are text-extractable** with `pdftotext` (already on this machine at
  `/mingw64/bin/pdftotext`), so a session can read them directly:
  `pdftotext -f 1 -l 20 <file> -`. The `Read` tool cannot — it needs poppler's
  `pdftoppm`, which is not installed.

- [x] **WI-505 Glossary terms for the new vocabulary** *(done 2026-08-30, PR #71)*
  40 new terms, 43 total. Glossary front matter gained a `sources:` field —
  the glossary was the one content surface where a medical claim could ship
  with nowhere to record where it came from.
  **Read this before adding a term.** Nine of the thirteen StatPearls
  citations in the first draft were wrong: written from memory, they resolved
  to real but unrelated chapters (`NBK534244`, cited on four surgery terms, is
  *Carbon Dioxide Angiography*). **A wrong citation that resolves is worse than
  no citation, because nothing looks broken.** Every URL here was fetched and
  its title read before use. Do the same.
  Goal: the tooltips fire before the pages that need them ship.
  Acceptance: roughly 40 terms added — extra-axial, dural tail, mass effect,
  vasogenic edema, gross total resection, debulking, frozen section, integrated
  diagnosis, NOS, NEC, methylation profiling, pseudoprogression, radiation
  necrosis, fractionation, eloquent cortex, and the markers (IDH, MGMT, 1p/19q,
  ATRX, TERT, CDKN2A/B, H3 K27M, EGFR, BRAF, Ki-67). **Definitions describe, they
  do not interpret** — no marker is characterised as favourable.
  Refs: `Content/glossary/`, `GlossaryMarker`, WI-434. Depends on: nothing.

- [x] **WI-558 The caregiver section: shared block and standard** *(done 2026-08-30, PR #72 — `[CAREGIVER]` block on all 18 tumor hubs; standard at content-pipeline §12.7; the seizure links land with WI-559/560)*
  Goal: give the other person in the room a lane on every page, consistently.
  **Dan's call, 2026-08-30**, answering the question WI-446 left open: yes,
  every tumor hub carries one, and he wants it prevalent rather than tucked
  away. Two reasons he gave, both concrete: surgery is followed by a lot of
  aftercare that falls to someone else, and a person living with someone who has
  a tumor needs to know what they will have to deal with — his example was a
  partner with a tumor who has seizures.
  This is the standard, not the content: each page writes its own specifics.
  Acceptance: a `[CAREGIVER]` shared block (WI-501 mechanism) holding what is
  genuinely common across every tumor — you are allowed to ask questions; who
  coordinates care and how to get one named contact; what to write down at
  appointments; **when to call, and what "call today" versus "call an ambulance"
  looks like**; looking after yourself, including permission to. Plus a
  documented per-page pattern for the specifics each hub adds.
  **The evidence this is built on:** care coordination and advocacy were raised
  almost exclusively by caregivers, not patients; caregivers performed dressing
  changes and gave medications with **no formal instruction**; several feared
  "offending the physician by asking too many questions"; six of 25 families
  regretted not accepting hospice sooner.
  **Written to the caregiver, in the second person, not about them.** A section
  that says "caregivers often find..." has already failed the person reading it
  at 2am.
  Refs: docs/research/tumor-guides/patient-questions-and-ia.md §(c)H and §(d)2,
  SYNTHESIS.md §3.9. Depends on: WI-501. **Before WI-513 — it is part of the
  template that gets proven.**

### Wave 1 — the spine, proven end to end

Six library pages plus **one** tumor hub taken all the way, so the template is
tested before it is replicated 23 times. **Ends with a localhost URL for Dan**
(standing rule since 2026-08-29) before Wave 2 starts.

- [x] **WI-506 T1 MRI — what it's like and what to ask** *(defines the page template)*
  **Done 2026-08-31.** `/tests/mri`, reading grade 4.0. The library-page
  template is written up at content-pipeline.md **§12.8** (12 slots, with a
  variable middle — the "ten sections" in this ticket did not survive contact
  with the first page). URL scheme for the libraries set here: `/tests/<slug>`,
  and `/treatments/<slug>` to follow.
  **Revisit when T11 and a `/tests` index exist:** the page's only door today is
  `/start`, and its "where to go next" links only to pages that already ship.
  Pseudoprogression, radiation necrosis and scanxiety are deliberately absent —
  SYNTHESIS §4.3 makes T11 their canonical home.
  Goal: the scan every reader has, explained by someone who has been in the room.
  This item also **establishes the 17-section tumor-page template** and the
  10-section library-page template that every later content item follows: what it
  is in one sentence · why you might have it · what happens step by step · how
  long · what it feels like · side effects soon · side effects later · what you
  need first · questions to ask · related pages.
  Acceptance, beyond the shared contract: covers the machine, the noise, holding
  still, and **claustrophobia with what can actually be done about it** (coaching,
  music, sedation arranged in advance — not "you can't have an MRI"); contrast
  and the questions people ask about it; **implant screening framed as "bring
  your device card"**, since MRI-compatible pacemakers and cochlear implants
  exist; the **navigation/"stealth" scan the day before surgery**, which baffles
  people; the scan right after surgery. Serves diagnosis, planning and
  surveillance in one page — deliberately not split.
  Refs: docs/research/tumor-guides/tests-library.md,
  patient-questions-and-ia.md §(b). Depends on: WI-501, WI-502, WI-504.

- [x] **WI-507 T5 Waiting for pathology results** *(done 2026-09-03, PR #79)*
  **Done.** `/tests/waiting-for-results`, reading grade 4.8, 998 tests,
  ContentCheck 156/0. Second page under the §12.8 template, which gained four
  rules from it (slot 7 is a role not a heading; slot 4 may carry a "why"
  section; a one-country audit figure is attributed in the paragraph that
  prints it; shared prose becomes a block on its SECOND use, not its fifth —
  the tumor-board paragraph is now `[TUMOR-BOARD]`).
  **Three of the numbers in this ticket are wrong or misattributed, and the
  next session should trust the fetched sources over the dossier.** The
  `academic.oup.com` URL behind every turnaround figure 403s; the same paper is
  open at **PMC13161907**. The 90% frozen-section agreement figure belongs to
  **PMC4287923**, not the PMC4322495 that `tests-library.md` §4.4 cites. The
  "6-72 hours" fixation figure comes from a guide that is explicitly about
  **rodent** tissue; Leica says 6-24 hours for human diagnostic specimens, and
  that is what shipped.
  **"Amended reports" shipped thinner than this ticket asks**: no patient-level
  source defines the term, and the only papers that do are about error rates,
  which would imply mistakes are common. The page names the situation and says
  what to ask instead of defining the mechanics. Revisit if a source turns up.
  **CLOSED at WI-508 (2026-09-03): a source turned up and the section is now
  full.** `MyPathologyReport.ca`'s "Amendment: Definition" is patient-level, is
  written and reviewed by practising pathologists, and defines an amendment
  *against* an addendum. It also supplies the two things that were missing: the
  commonest reason is a small correction such as a typing mistake, not a wrong
  diagnosis, and *"amendments reflect the pathology system working as
  intended"*. The paragraph and a new `amended report` glossary term ship with
  WI-508.
  Goal: the highest-value page in the phase, and nobody else has one.
  The wait is fully explainable and completely unexplained. Frozen section
  ~20–26 minutes, agreeing with the final diagnosis ~90% of the time. Formalin
  fixation alone 6–72 hours before processing starts. Gene panel ~23 days.
  Methylation array ~23 days — **batched in groups of about 8, so samples
  queue**. Integrated diagnosis median 15 days (2021) rising to 21 (2024),
  against a 14-day benchmark only 30% of centres meet.
  Acceptance, beyond the shared contract: states plainly that **the answer
  arrives in layers and the name of the diagnosis can legitimately change
  between them — and that this is the system working correctly, not someone
  changing their story.** Explains batching as the mundane cause. Covers
  addendums and amended reports, what "sent out to another lab" means, second
  opinions on the pathology itself, and what to do while waiting. Deliberately
  split from the biopsy page (WI-519) so a reader who is already home and
  already waiting does not scroll past surgical technique to reach it.
  Refs: docs/research/tumor-guides/tests-library.md §(b)5,
  meningioma-mets-general.md. Depends on: WI-501, WI-502, WI-504.

- [x] **WI-508 T6a Your pathology report — the walkthrough** *(absorbs WI-444)*
  *(done 2026-09-03)*
  **Done.** `/tests/pathology-report`, reading grade 5.2, 1018 tests,
  ContentCheck 163/0. Third page under the §12.8 template, which gained four
  more rules from it (a document page needs a "where the answer sits" section
  before the walkthrough and must say the layout varies; heading anchors are a
  published interface once other pages deep-link them; the shared-prose rule
  applies to the tests too; and a per-page property must be run against the
  already-shipped pages before it is promoted to a site-wide one).
  **Slots 2 and 5 are dropped**, and §12.8 now records that they are not
  universal: both assume a day and a procedure, and neither has a referent on a
  page about a document.
  **The dossier was wrong twice more, and one is the WI-505 failure mode
  exactly.** `tests-library.md` §5.4 sources both "what a brain tumor report
  contains" and "one of the most important documents guiding treatment
  decisions" to **PMC4300589**. Fetched, that paper is *"Brain tumors: Special
  characters for research and banking"* (Adv Biomed Res, 2015), a biobanking
  and cytogenetics review, and it says neither. Johns Hopkins carries both
  sentences verbatim and is what shipped; a test fails if PMC4300589 ever
  appears in this page's front matter. The CAP PDF the dossier cites
  (`documents.cap.org/.../how-to-read-pathology-report.pdf`) is **dead** — it
  returns nothing at all. Seven bad citations across four items now.
  **Every naming and grading claim rests on WHO CNS5 itself** (Louis et al,
  PMC8328013), not on a summary of it — including the Roman-to-Arabic reason in
  the authors' own words, grading within a tumor type, a gene result setting the
  grade where the cells look lower, and NOS/NEC.
  **Johns Hopkins is cited for what a brain tumor report CONTAINS and never for
  anything evaluative**: its own glossary calls IDH mutation "associated with a
  better prognosis", and its sample report says "(WHO grade IV)". Both are the
  dated paperwork this page exists to translate, not a source to write from.
  Goal: translate the document the reader is holding.
  Acceptance, beyond the shared contract: what the report is and who wrote it;
  the report's layout section by section; **why molecular results change the
  NAME of the diagnosis** under CNS5 (the safe, non-prognostic editorial spine —
  these tests rename, they do not predict); grades and why the numbering changed
  in 2021; how long results take. Deep-linkable so `/tumors` and feed items can
  point at an exact term.
  Refs: docs/research/tumor-guides/tests-library.md §(b)6 and §(d).
  Depends on: WI-501, WI-502, WI-504, WI-505.

- [x] **WI-509 T6b The molecular marker glossary** *(absorbs WI-444)*
  *(done 2026-09-03)*
  **Done.** `/tests/molecular-markers`, reading grade 4.1, 1039 tests,
  ContentCheck 170/0. Fifteen entries, each with an explicit `{#anchor}`.
  **Entries are visible anchored sections with a jump list, NOT collapsibles.**
  A `<details>` closed on load defeats deep-linking, prints empty on the one
  page people hold beside the document, and needs a second `:::` container with
  an argument — a new fail-open surface (WI-503 documents five) guarding words
  that are descriptions, not prognosis. Reasoning is in §12.8 so WI-510 onward
  do not re-argue it.
  **One clause of the somatic/germline block is deliberately absent.** "You
  cannot pass them on to your children" is verbatim-supported by NCI ("somatic
  changes cause most cancers and cannot be passed on to family members"). **"You
  did not do anything to cause them" has no source** — SYNTHESIS §3.7 presents
  the whole paragraph as validated framing, but the cited paper (PMC8062319) is
  a breast-cancer genetic-counselling lexicon and does not contain it. The
  section delivers the substance without it, and adds the honest other half: a
  tumor test can occasionally turn up something inherited, and the team tells you
  first. Self-blame belongs to §12.3's block and WI-513.
  **The gated source is Sahm et al, EANO, open at PMC10547522.** The dossier
  hangs nearly every row of its §5.2 marker table on one Cloudflare-gated
  `academic.oup.com` URL. It is Neuro-Oncology 25(10):1731-1749, found by DOI in
  Europe PMC, and it is the backbone for what all fifteen markers measure. A test
  fails if `academic.oup.com` ever appears in this page's front matter.
  **Two more dossier errors.** Its Ki-67 caveat cites PMC10644968, a paper about
  thyroid, lung and breast cancer; replaced with a neuropathology source that
  says it of brain tumors. And §5.2's MGMT rows are fine, but the shipped
  glossary entry for MGMT cited a **fabricated title** on an unrelated URL (see
  below).
  **Four wrong citation titles fixed in shipped glossary entries, 16 files.**
  `mgmt-methylation.md` cited PMC12467656 as "MGMT promoter methylation and
  response to alkylating chemotherapy"; that URL is *"Radiotherapy in
  Glioblastoma Multiforme"* (Biomedicines 2025) and is now repointed to the EANO
  guideline. Also: "Molecular markers in adult diffuse glioma" is really Thomas
  et al's 2021 WHO update review (3 entries), "Major changes in..." is really
  "Major **Features** of..." (11 entries), and the BRAF citation is really
  Houghton et al on MAPK inhibitors. **Nine bad citations across five items.**
  Also: 3 glossary terms (TP53, H3 G34, chromosome 7 gain and chromosome 10
  loss), the 15 markers this page defines have their tooltips suppressed here
  (`!%term%`) so 3 fire rather than 15, the shared `Characterisations` list
  gained 10 phrases, and §12.8 gained six rules.
  Goal: one entry per word on the report, as an anchor-linked reference.
  Acceptance, beyond the shared contract: IDH · 1p/19q · ATRX · TERT ·
  CDKN2A/B · EGFR · chromosome 7 and 10 · H3 K27M · H3 G34 · BRAF · MGMT ·
  Ki-67 · TP53, plus NGS panels and methylation profiling. Three lines each:
  what it is / what your team does with it / **what it does not mean**. Every
  entry individually anchor-linked so a reader searching "what is 1p/19q" lands
  on it. Each entry collapsible so the default view is a scannable list.
  **The hardest line on the site, and it must hold: describe what is measured,
  never characterise a result.** Sources say this out loud — ACS literally
  writes "better outlook" for IDH and MGMT — so the drafting prompt has to
  forbid it explicitly or it will drift back in. MGMT is the hardest case
  because it is genuinely treatment-selecting rather than merely prognostic:
  say what is measured, say the team uses it as one input when choosing
  chemotherapy, and route "what does *my* result mean" to the reader's own team.
  Includes the somatic-vs-germline block — *"the labels are about the tumor, not
  about you. You did not do anything to cause them, and you cannot pass them on
  to your children"* — which is the #1 family question and is answered clearly
  nowhere on the consumer web.
  Refs: docs/research/tumor-guides/tests-library.md §(b)6 and §(e)1–2.
  Depends on: WI-508.

- [x] **WI-510 X2 Craniotomy** *(done 2026-09-04 — `/treatments/craniotomy`, the first page of the treatment library)*
  Goal: the operation, from arriving at the hospital to being back at home.
  Acceptance, beyond the shared contract: the step-by-step from the patient's
  side; ICU, hospital stay, the incision, hair, headaches, the recovery
  timeline; risks by location; 5-ALA and navigation; LITT as a section, not a
  page. Two specific things that prevent real distress: **"gross total
  resection" is defined by the post-op MRI, not by cure** (microscopic disease
  remains in diffuse glioma), and **cognition often feels worse on day 2–3 and
  then improves** — one sentence that prevents a panic. Also SMA syndrome:
  transient loss of speech initiation and one-sided movement after surgery near
  the supplementary motor area, recovering over days to weeks; unwarned patients
  believe they have been permanently disabled.
  **Carries the fullest caregiver section on the site** (contract item 12) —
  this is the aftercare Dan named. What the person at home actually does: wound
  and staple care they were never taught, medication schedules, what "normal"
  looks like day by day, when to call the team and when to call an ambulance,
  and how long they should expect to be doing it. A caregiver is discharged into
  this job with no training; the research found them performing dressing changes
  and giving medicines with none.
  Refs: docs/research/tumor-guides/treatment-library.md.
  Depends on: WI-501, WI-502, WI-504, WI-558.
  **Shipped notes.** Full twelve slots (§12.8) — the first page since WI-506 to
  use them, and it carries 6c as well as 6b (LITT, with its limits attached).
  Reading grade 5.0, 1070 tests, ContentCheck 183/0. Six glossary terms.
  **Three more dossier citation errors**, taking the running total to twelve
  across six items: the ICU claim is attributed to ABTA, which never mentions
  intensive care (it is in StatPearls NBK560922); the "neurological checks
  through the night" detail is attributed to NBTS, which says nothing about
  them anywhere; and the gross-total-resection definition rests on PMC5358612,
  a **conference abstract**. PMC7093492 was dropped too, being a survival
  meta-analysis cited on a page that publishes no prognosis figures.
  **Deliberately thinner than the ticket in one place:** "risks by location"
  is qualitative and names SMA syndrome specifically, but publishes no figure
  for anything, per R2 — the SMA source alone reports the deficit risk as 23%
  to 100%. Awake craniotomy is named in one sentence and **not** linked;
  WI-523 owns it and the page does not exist yet.

- [x] **WI-511 X5 Radiation therapy** *(done 2026-09-04 — `/treatments/radiation-therapy`)*
  Goal: the hub page for every form of radiation.
  Acceptance, beyond the shared contract: the simulation appointment and
  **mask-making, which is a distinct and under-acknowledged fear point**;
  fractionation in plain words; what a daily session is actually like (lead with
  the sensory experience, then the mechanism, then the schedule); acute side
  effects and late effects. **Somnolence syndrome gets its own named
  subsection** — profound drowsiness 4–6 weeks *after* radiation ends,
  self-resolving, and families read it as the tumor growing. Absorbs IMRT,
  conformal, and whole-brain radiation including hippocampal avoidance and
  memantine; R3 applies to the cognitive comparison.
  Refs: docs/research/tumor-guides/treatment-library.md.
  Depends on: WI-501, WI-502, WI-504.
  **Shipped notes.** Full twelve slots (§12.8), second treatment page. Reading
  grade 5.4, 1120 tests (1070 before), ContentCheck 197/0. Seven glossary terms.
  Slot 8 (re-irradiation) has **no patient-level source anywhere** — ACS, MSK
  and NBTS are all silent — so it rests on the EANO guideline and is published
  at that strength: an option after roughly a year, indications controversial,
  no trial settles it.
  **Three dossier citation errors**, taking the running total to eighteen across
  seven items: the somnolence "resolves on its own" claim is attributed to the
  charity's **jargon-buster** page, which is one sentence long and does not
  contain it (the adults side-effects page does); PMC7017115, cited for the
  cognitive late effect, is a mechanistic review largely about the **mouse**
  brain; and `ascopubs.org` returns a JavaScript shell, so the whole-brain claim
  rests on **PMC7106984** (NRG CC001) directly. NBK66023 stays out per §12.1.
  **Two rules promoted to `CuratedPage`** as §12.8 asked at the second treatment
  page: `AssertNeverMinimises` (negation-aware, clause-anchored) and
  `BritishForms` — the first gate on the site that looks for British usage,
  which found `"a lift"` in the shared caregiver block (eighteen tumor hubs) and
  `standardised` in this page's own draft.
  **Deliberately absent: a "call an ambulance" list.** The shared `[CAREGIVER]`
  block already teaches the escalation and links the seizure page that owns it;
  a third copy is a third copy to keep in step. The call-today list carries the
  first-seizure carve-out inline instead.

- [x] **WI-512 X8 Chemotherapy** *(done 2026-09-04 — `/treatments/chemotherapy`)*
  Goal: one page for the drugs, because the shared content is where the value is.
  Acceptance, beyond the shared contract: temozolomide, PCV, lomustine and
  carmustine wafers as sections; oral vs IV; which tumors use which; **blood-count
  monitoring and the fever rule**; anti-nausea support. R1's lomustine wording is
  mandatory here. Carmustine wafers get a brief, neutral mention — a 2022 review
  is literally titled *"Is It Still an Option?"* — and current practice should be
  re-verified before publishing.
  Refs: docs/research/tumor-guides/treatment-library.md §(e)4.
  Depends on: WI-501, WI-502, WI-504.
  **Shipped notes.** Reading grade 5.4, 1163 tests (1120 before), ContentCheck
  209/0, all 75 break-mutations proven on LF and CRLF. Six glossary terms.
  **The fever rule is the page**, and 100.4 F is published deliberately — the
  one place the site's number discipline points toward including a figure
  rather than omitting one, hedged with "ask your team for their number".
  **The carmustine-wafer framing changed from the ticket.** The ticket cites the
  2022 review's title, *"Is It Still an Option?"*, as grounds for a sceptical
  mention. Read, that paper ANSWERS ITS OWN TITLE YES. EANO is genuinely more
  cautious. The page prints the disagreement instead of either framing.
  **Four sources the dossier offers are unusable**, one on licensing grounds:
  `drugs.com` is an AHFS monograph (PLAN.md §5 forbids it outright); PMC3601076,
  cited for TMZ lymphopenia, is a **mouse study**; the Cancer Care Ontario
  lomustine monograph sits behind a WAF and returns 106 bytes; and NBK66023 is
  NCI patient PDQ, whose which-drug table contains **"Anaplastic astrocytoma"**,
  a retired CNS5 name. The lomustine timing came from EANO instead, and the
  antibiotic section from PMC12803824.
  **R1's lomustine wording is on the page verbatim in spirit** and now properly
  sourced: taken only occasionally, not every day, *because it lowers blood
  counts for weeks after each dose*.
  **Slot 5 was dropped and put back after review** — see §12.8.

- [x] **WI-559 "What to do when someone has a seizure"** *(done 2026-08-30, with WI-560 in one PR — `/seizures/what-to-do`)*
  Goal: the one page on this site where a reader may be acting, not reading.
  **This is life-safety content and it exists nowhere on the site today.**
  Seizures are the commonest way a brain tumor announces itself and, in
  low-grade glioma and oligodendroglioma, the symptom a person lives with for
  years. Every caregiver section written under WI-558 links here, so it must
  exist early rather than at the end of the phase.
  Acceptance, beyond the shared contract: what to do, in order, as short
  imperative steps a frightened person can follow while it is happening — time
  it, cushion the head, nothing in the mouth, turn them on their side once the
  movements stop, stay until they are fully aware. **What NOT to do**, since the
  folk advice is actively harmful. **When to call an ambulance**, stated
  unmistakably: over five minutes, one seizure straight into another, injury,
  trouble breathing, first-ever seizure, or it happens in water. What happens
  afterwards and why the person may be confused, exhausted or not remember. What
  to write down for the care team, because that record changes treatment.
  **Format is the safety feature.** It must be scannable in a panic, legible in
  print (people put this on a fridge), and readable with the stylesheet dead.
  The steps must not sit behind a disclosure, ever. Print layout is part of the
  acceptance, not a nicety.
  **Do not state driving rules** — they are jurisdictional. "Your rules depend
  on where you live, here is who to ask." Same line WI-451 and WI-525 hold.
  Sources must be first-tier for this one (Epilepsy Foundation, NHS, CDC) and
  cited on the page.
  Refs: WI-525 (medicines, a different page — this is what to DO),
  docs/research/tumor-guides/patient-questions-and-ia.md §(c)D.
  Depends on: WI-558. **Before WI-513** — the template links to it.
  **Pairs with WI-560**, which is the rest of the time.

- [x] **WI-560 Living with seizures: what you can do, and what to be careful about** *(done 2026-08-30, with WI-559 in one PR — `/seizures/living-with`)*
  *(Dan, 2026-08-30, from his own example)*
  Goal: the day-to-day question WI-559 does not answer — not "what do I do
  during a seizure" but "what am I allowed to do for the other 364 days".
  **Why it exists.** Dan: his ex-wife has a low-grade glioma with seizures,
  controlled on medication, and she has worked out for herself that anything
  strenuous can bring on small ones — so she manages what she does. She cannot
  drive. Nothing on this site helps with any of that, and the phase as filed
  did not cover it: **WI-559** is the emergency, **WI-451** is late effects,
  **WI-525** is the medicines, and section 9 of a tumor hub is one paragraph per
  tumor. Nobody owned the practical list. This item does.

  **The finding that decides the whole shape of this page, and it cuts both
  ways.** Most of the restriction people with seizures are handed is not
  evidence-based. People with epilepsy are measurably less active than the
  general population because of "prejudice, overprotection, unawareness, stigma,
  fear of seizure induction and lack of knowledge of health professionals";
  exercise generally *reduces* seizure frequency; the guidance position is
  individualised, risk-stratified counselling **instead of** blanket restriction
  (ILAE; `sciencedirect.com/science/article/pii/S1059131114002660`).
  **And exercise-triggered seizures are still real for a minority.** In a series
  of 400 people with epilepsy only **two** could identify physical activity as a
  precipitant — but where it happens it is reproducible, and the degree of
  exertion tracks the likelihood (`neurology.org/doi/10.1212/WNL.38.4.633`).
  **So the page has to hold both, and the order matters.** A page that only says
  "exercise is good for you, go and live your life" tells the reader in that
  minority that she is wrong about her own body — and she is not; she has
  observed it repeatedly. A page that only lists prohibitions makes everyone
  else's life smaller for no reason. Say: most people are told to do less than
  they need to, **and** if you have noticed a pattern in yourself, you are not
  imagining it and it is worth writing down and taking to your team.

  **The organising principle, which is the actually useful idea here: it is
  rarely the activity that is dangerous, it is what you would fall into, onto,
  or from.** Give the reader that sentence and they can reason about activities
  nobody wrote a page on. A list of banned things cannot do that, and it goes
  stale the moment someone asks about a hobby that is not on it.

  Acceptance, beyond the shared contract:
  - **Water is the one to state most plainly**, because it is where seizures
    kill people. Shower rather than bath; do not lock the bathroom door and hang
    a sign instead; a door that opens outward cannot be blocked by someone who
    falls against it; non-slip strips; a fabric curtain rather than a glass
    screen; never swim alone, and a companion who knows what to do beats a
    lifeguard who does not.
  - **Small changes that keep the activity**: back burners and a microwave
    rather than reaching over a gas flame; a food processor rather than knives
    when alone; care on ladders, at heights and near open water. Framed as
    "keep doing it, change how", never as "do not".
  - **Triggers worth acting on**, with the honest caveat that they differ per
    person: a regular sleep schedule (sleep loss is the best-attested one),
    alcohol — including that the risk peaks while it wears off, not while
    drinking — dehydration, heat, and missed doses.
  - **A seizure diary, and why it is worth the bother**: it is the thing that
    actually changes a medication decision, and it is how you find out whether
    your own suspected trigger is real.
  - **Being the one who over-restricts yourself** is a real cost too, as is a
    family member doing it for you. Say so.
  - **Practical kit**: medical ID, a written seizure action plan for work and
    family, spare medication in a bag, alarms for doses.
  - **Work**: what to tell an employer, and that adjustments exist.

  **Driving gets its own section and NEVER a number.** It is the loss people
  feel hardest and the thing this page most needs to handle honestly. US rules
  vary enormously: 28 states set a fixed seizure-free period (median 6 months,
  range 3 to 12), 23 leave it to clinical judgement, and Florida requires 2
  years with reconsideration possible at 6; every state requires notifying the
  DMV and some place the reporting duty on the doctor
  (`epilepsy.com/lifestyle/driving-and-transportation/laws`). So: explain the
  shape of the rules, link the state-by-state tool, say plainly that this is one
  to ask the care team about rather than guess, and **do not print a duration on
  the page**. Same line WI-451, WI-525 and WI-559 already hold.
  Then say the part nobody says: losing the licence is usually the biggest
  practical change, it is a legitimate grief, and there are workarounds worth
  knowing about. Per §12.6 this section must not end on the loss.

  **Sources first-tier and cited** (Epilepsy Foundation, ILAE, NBTS), and note
  for whoever writes it: `braintumor.org/news/9-tips-for-managing-seizures-caused-by-brain-tumors/`
  is good on medication routine and useless on daily activities — the activity
  material has to come from the epilepsy sources, not the brain-tumor ones.
  **Not US-only in tone.** WI-455/457 put non-US readers on this site
  deliberately; a page written as though every reader has a DMV fails them.
  Refs: WI-559 (the emergency; cross-link both ways), WI-558 (the caregiver
  block links here), WI-525 (medicines), WI-451 (which now sheds "seizures and
  driving" to this item). Depends on: nothing — it needs no tumor content.
  **Wave 0**, beside WI-559: the two are one subject split by whether it is
  happening right now, and writing them together is how that split gets decided
  once instead of twice.

- [x] **WI-513 Low-grade glioma, deepened — the template proof** *(done 2026-09-05 — `/tumors/low-grade-glioma`)*
  Goal: one tumor hub taken all the way, reviewed, before the pattern is copied.
  Chosen deliberately: the short version already exists to compare against.
  Acceptance, beyond the shared contract: the full 17-section order; **"low
  grade" is not "benign", and grade 1 is not grade 2** (grade 1 gliomas are
  circumscribed and often curable by surgery; grade 2 diffuse gliomas are
  malignant and incurable — and WHO actively recommends against the term
  "low-grade glioma", which is why the page leads with the caveat and routes to
  the actual diagnosis); why a "low-grade-looking" tumor can be called grade 4
  (molecular glioblastoma, CDKN2A/B); watch-and-wait; transformation;
  **vorasidenib** (FDA 6 Aug 2024 — the first glioma approval in decades, absent
  from most existing patient material including NCI's, and the clearest single
  proof this library is current); the age-40 line presented as trial eligibility
  criteria, **not** as a rule.
  Also proves the **caregiver section** (contract item 11, WI-558) in place —
  for this tumor that means living alongside seizures over years, linking to
  WI-559.
  **Ends with a localhost URL for Dan and an explicit stop** — the template gets
  adjusted here, not after 23 more pages exist.
  Refs: docs/research/tumor-guides/glioma-family.md §(b)2,
  patient-questions-and-ia.md §(b). Depends on: WI-506…WI-512, WI-558, WI-559.
  **Shipped notes.** 202 words and 4 sections became the full §12.3 seventeen.
  Reading grade 5.3, 1195 tests (1163 before), ContentCheck 219/0, all 60
  break-mutations proven on LF and CRLF. Five glossary terms. **The hub template
  is now written down at content-pipeline.md §12.9** — read that, not §12.8,
  before the next hub.
  **Two firsts.** The first page ever to use the `:::outlook` reader-choice gate
  (WI-503 built it ten items ago and nothing had used it), verified in rendered
  HTML and in print. And the first tumor hub to link into the tests and
  treatment libraries, which is what the seven Wave 1 pages were built for.
  **Review found two required blocks missing from the first draft**, both now
  added: the **self-blame block** (new shared `[CAUSES]` block, demoted per
  §12.3) and a **retired-name crosswalk slice**. A test in that draft banned the
  retired names outright and would have foreclosed the crosswalk on all 23 hubs
  that copied it.
  **Four of six citation TITLES were fabricated** — written from the dossier's
  description rather than the fetched page — and titles render as visible link
  text. Corrected, and §12.9 now says to paste the fetched `<title>`.
  **Also corrected `glioma-family.md` §2.3**, which sourced "most of these
  tumors do not cause neurologic deficits at diagnosis" to an article containing
  zero occurrences of "deficit", so Wave 2 does not inherit it.
  ~~**Open question for Dan, flagged by review:** WI-501's note says blocks ship
  empty and WI-513 writes the crosswalk, while WI-514 is named canonical home
  for `[CROSSWALK]`.~~ **ANSWERED 2026-09-07 — this item's call was upheld.**
  The page-specific slice stays here; the canonical block is WI-514's. See
  WI-501 and WI-514.

### Wave 2 — the glioma family and what it pulls in

Start only after Dan has signed off WI-513's template.

- [x] **WI-514 Glioma (umbrella), deepened** *(done 2026-09-07 — `/tumors/glioma`)* — the family tree and the router:
  which of the specific pages does the reader actually need. Canonical home for
  the `[CROSSWALK]` and `[MECHANISM]` blocks — **confirmed by Dan 2026-09-07,
  over WI-501's contradicting note.** This item WRITES `blocks/crosswalk.md`,
  which today does not exist (`Content/blocks/` holds only `tumor-board.md`,
  `caregiver.md`, `causes.md`). **Two layers, and the split is the point:** the
  block carries the CNS5-wide renames every hub owes (the 2021 rewrite, Roman
  numerals to Arabic, NOS/NEC); a page keeps its own slice for names specific to
  its diagnosis, as `/tumors/low-grade-glioma` does for oligoastrocytoma. When
  the block lands, reconcile that page so the shared lines are not said twice —
  its Roman-numeral line is the overlap. Source: Kurokawa et al,
  `10.1148/rg.210236` (see WI-504). Diffuse vs circumscribed;
  "adult-type" and "pediatric-type" mean biology, not the reader's age; grade,
  not stage. Depends on: WI-513.
- [x] **WI-515 High-grade glioma, deepened** *(done 2026-09-07 — `/tumors/high-grade-glioma`)* — grades 3 and 4 as a grouping, not
  a diagnosis; two tumors can both be grade 4 and be very different; there is no
  grade 4 oligodendroglioma; de novo vs transformed. Depends on: WI-513.
  **Shipped notes.** 194 words and 4 sections became the full §12.3 seventeen.
  Reading grade 5.6, 1275 tests (1233 before), ContentCheck 226/0, all 91
  break-mutations proven on LF and CRLF. Two glossary terms (chemoradiation,
  PCV). Beyond the four listed claims the page carries **pseudoprogression** —
  research §3.5 calls it "one of the most distressing and least-explained
  experiences in the whole disease" — with radiation necrosis as the later
  look-alike, and a "why is my friend on different chemotherapy?" section that
  is the practical proof of claim 2 (three regimens, from EANO and CATNON).
  **`/review` found the §12.10 defect re-committed here**: the incurability and
  infiltration claim was asserted of every high-grade glioma, and CNS5 defines
  circumscribed astrocytic gliomas with "more well-delineated borders", two of
  them grade 3. Now scoped. It also found the page had **no fever rule** despite
  routing every reader into chemotherapy. Both written into **§12.11**, along
  with the three shared-helper traps and the rule that a hub owes the safety
  rules of the treatments it routes into.
  **Five more bad dossier citations (25 across nine items)**, including §3.3's
  raised-pressure comparative, which is not in the Springer paper it cites —
  corrected in `glioma-family.md` at source so WI-516/517/518 do not inherit it.
- [x] **WI-516 Astrocytoma, deepened** *(done 2026-09-08 — `/tumors/astrocytoma`)* — three different families share the name;
  **personality and behaviour change**, which can precede diagnosis by months;
  the crosswalk slice that matters most (both directions of the
  "secondary glioblastoma" rename). Depends on: WI-514.
  **Shipped notes.** 182 words and 4 sections became the full §12.3 seventeen,
  4,129 words. Reading grade 5.7, 1311 tests (1275 before), ContentCheck 228/0,
  all 77 break-mutations proven on LF and CRLF. One glossary term (astrocyte)
  plus a `CDKN2A/B` alias that made an existing entry reachable from the prose.
  **`/review` found two blockers and both were over-reassuring**, which §12.12
  now records as the more dangerous direction of §12.11's scoping defect.
  (1) The draft told the whole circumscribed group they were grade 1 and curable;
  CNS5's table grades that family 1, 1, 2-3, 2-3, not established, 3. (2) **The
  "secondary glioblastoma" claim had no source at all** — the phrase appears in
  no body text across every source fetched for three items, and "secondary"
  described how the tumor arose rather than its IDH status, so the bolded
  promise was wrong for IDH-wildtype readers. Now routed by the IDH line.
  **Also fixed beyond this page:** five copies of the Roman-numeral guard across
  four test files were case-sensitive and all stayed green on a planted
  "**Grade III.**" — the IgnoreCase trap's fourth occurrence. Bad citation #26:
  the dossier's source for the entire circumscribed group is a bot-blocked
  PubMed URL that is not open access anywhere.
- [x] **WI-563 `[ESCALATION]` shared block** *(done 2026-09-08 — `Content/blocks/escalation.md`)* —
  the twelve-line ambulance / same-day list was hand-copied **byte-identical**
  across three hubs and the fever sentence beneath it had **already diverged**
  (high-grade said "explains why, and what number your team will give you",
  astrocytoma said "explains why."). Factored at the fourth use, three uses
  after §12.8's rule said to, with 21 hubs still to write. Now included by
  **four** pages; `/tumors/low-grade-glioma` was switched off its compressed
  one-paragraph version too. §12.10 gained the case, §12.9 names the block.
  **The item's own split was WRONG and the item reversed it.** This entry
  originally said the tiers were universal and the fever line was page-local
  routing, and that `/tumors/glioma` should deliberately carry none. Checking
  that assumption found the opposite: **`/tumors/glioma` linked
  `/treatments/chemotherapy` and contained the word "fever" zero times, and
  `/tumors/low-grade-glioma` mentioned chemotherapy four times with "fever" only
  inside a link label** — two live pages carrying §12.11's defect, the one
  WI-515 was blocked for nearly shipping. A **conditional** ("if you are having
  chemotherapy") is true on every hub and false on none, so it belongs in the
  block; the surgery half went in with it, because a post-craniotomy fever is
  not chemo-conditional and every hub routes into `/treatments/craniotomy`.
  **`/review` found two blockers, and the first was an editorial defect the
  automated gates cannot see.** (1) *"Being sick over and over"* is British for
  vomiting and reads as *being unwell* to a US reader — on a same-day escalation
  trigger, promoted into the file with the widest blast radius on the site. The
  block now says "Throwing up again and again", carries the 911 instruction
  `/seizures/what-to-do` already uses, and drops `straight away`, `straight
  after` and `out of hours`. (2) Factoring the three page-level tests into one
  helper **silently dropped the section scoping all three had**, so moving
  `[ESCALATION]` to the bottom of a page kept the suite green — WI-512's
  presence-versus-position defect, re-committed by the factoring that cites it.
  **Three more of my own checks were theatre.** The craniotomy cross-check
  terminated on "call your team", a string that page **does not contain**, so
  the capture ran to end of file and passed only because the words happened not
  to appear down there; it also compared "vision" against a page that says
  "cannot ... see", reporting agreement where the two pages differed. The
  chemotherapy check matched that page's **intro line**, so gutting the whole
  `{#fever-rule}` section would have stayed green while the block's link landed
  on an empty heading. And the British-forms guard copied the weaker of the
  repo's two implementations, which disables a whole check when any exemption
  appears in the file.
  **Also:** three gaps closed that predate the item — the ambulance tier had no
  sudden-complete-deficit line while `/treatments/craniotomy` did (the
  under-triage direction), the block never said which seizures are **not** an
  ambulance (which `/tumors/low-grade-glioma`'s compressed slice did say), and
  the "while your white cells are low" clause in my first draft handed a reader
  whose last count was normal a reason to discount a fever. **29 breaks proven
  on LF and CRLF**, including a NEAR copy of the list rather than a byte copy —
  the first version of the fork test used `Ordinal` and would have waved a
  re-typed "first-ever" straight through. **1322 tests** (1311 before),
  ContentCheck 228/0, grades unchanged (low-grade 5.3 -> 5.2).
  **One self-inflicted scare worth recording:** the script that trimmed the
  helper file cut from the insertion point **to end of file**, deleting the two
  MRI page test classes that lived below it. Caught by the test COUNT going
  DOWN (1308) rather than by any failure — a green suite that is 14 tests
  smaller is the WI-512 stale-assembly lesson in a new coat. **Always read the
  count, not just the colour.**
- [ ] **WI-564 Corpus-wide British idiom sweep** *(not a Wave 2 blocker — do it
  before the corpus doubles)* — WI-563 found that the spelling gate
  (`CuratedPage.BritishForms`) does not catch **idiom**, and idiom is what
  actually misleads. *"Being sick"* means vomiting in Britain and *being unwell*
  in the US, and it was sitting in a same-day escalation trigger. Corpus counts
  at the time: `being sick` 6, `out of hours` 9, `throwing up` 1 — and
  `/treatments/radiation-therapy` uses **both** forms on the same page.
  **WI-518 adds one the sweep would otherwise miss: `feeling sick`.** WI-563
  fixed "being sick" in `blocks/mechanism.md` and left "feeling sick" beside it
  in the same sentence. In US English that reads as *feeling unwell*, not
  *nauseated*, and it sits on a raised-pressure symptom line on every hub the
  block composes onto. Half-fixing an idiom pair is worse than not starting:
  the remaining half now looks deliberate.
  **Goal:** promote `being sick`, `feeling sick`, `straight away`, `straight
  after`, `out of hours`, `come round` and `advice line` into a shared idiom
  list, fix the
  pages they land on (`/treatments/chemotherapy`, `/treatments/craniotomy`,
  `/treatments/radiation-therapy`, `blocks/mechanism.md`), and run the whole
  list over the whole corpus per §12.8 **before** adding it — WI-511's "bad
  news" lesson is that a rule which fails a correct page is worse than no rule.
  Note `straight away` is not wrong in US English, only less natural; decide
  deliberately and record the decision either way. **Depends on:** WI-563.
- [ ] **WI-565 Source the "at any hour" clause on `/treatments/chemotherapy`,
  or retire it** — WI-563 fetched every source in that page's front matter and
  in the escalation block and found **nothing supporting the phrase "at any
  hour"**. CRUK says "contact your advice line straight away", ACS neutropenia
  gives the threshold and "call your cancer care team or get medical help", NCI
  says an infection is "life threatening and require[s] urgent medical
  attention" — none says "at any hour". The page publishes it **twice** and it
  is the most safety-critical sentence on the site.
  **Do not simply delete it.** WI-511's lesson: deleting a bad citation does not
  delete the claim, and this one is operationally right — the same page tells
  the reader to get "the number to call out of hours, not the clinic's daytime
  number", which is a team that expects out-of-hours calls. Either find a
  patient-level source that says it (a 24-hour advice line page would), or
  re-word to what is sourced **without weakening it** (§12.12: the
  over-reassuring direction is the more dangerous one). Whatever is decided,
  `blocks/escalation.md` must match — WI-563 pinned the two together with a test
  that reads the sibling's `{#fever-rule}` section, so they cannot drift again.
  **Depends on:** WI-563.
- [x] **WI-517 Oligodendroglioma, deepened** *(done 2026-09-08 — `/tumors/oligodendroglioma`)* —
  a 172-word stub became the §12.3 seventeen-section hub. Reading grade **5.8**,
  **1357 tests** (1322 before), ContentCheck **232/0**, **all 47
  break-mutations proven on LF AND CRLF**, two new glossary terms
  (oligodendrocyte, calcification). **The first hub written AGAINST
  `[ESCALATION]` rather than hand-copying it**, which is why WI-563 was slotted
  ahead of it.
  **This is the page where §12.1 costs the most, and it is now written up as
  §12.13.** The single richest source on this tumor is the StatPearls
  oligodendroglioma chapter — the exact chapter §12.1 names as one never to use
  for naming or grading. It is cited here for symptoms, imaging and treatment
  mechanics; naming and grading rest on the CNS5-aligned sources, with a
  front-matter comment saying so and tests looking for the vocabulary that would
  leak in if the line slipped.
  **Three dossier claims are not in the sources they are attributed to.**
  "Patients often have a long history of seizures before diagnosis" appears
  **zero** times in that chapter; **PMC10475770**, cited for "transformation is
  not associated with worse outcomes in oligodendrogliomas", is about
  transformation *patterns* and says nothing of the kind; and "no inherited
  syndrome is characteristically associated with oligodendroglioma" — which the
  dossier flags as *"say this, because it is reassuring and true"* — has no
  citation anywhere. **That parenthetical is the tell.** `ascopubs.org`
  re-confirmed dead (403, 5,620-byte shell) for the third item running.
  **`/review` found FIVE blockers and the first was a whole missing block.**
  (1) **No retired-name crosswalk slice**, on the one hub whose central fact IS
  a retired name — oligoastrocytoma was not renamed, it was **eliminated**, and
  the 1p/19q test this page spends its length explaining is what eliminated it.
  The page printed "Oligoastrocytoma" in a visible source title with no
  explanation, while `/tumors/high-grade-glioma` already explained "anaplastic
  oligodendroglioma" and routed here. **And the test guarding it iterated over
  regex MATCHES, so on a page with none the loop never ran and it was green.**
  (2) The page's headline claim **overstated its only live source by one word**:
  "prediction of the best drug RESPONSE" became "predicts WHICH DRUGS it will
  respond to best", under a heading naming PCV — drug selection rather than
  responsiveness — and contradicted the "genuinely open" section three screens
  below. (3) The caregiver section reported what caregivers "most often say",
  a quantified population claim no source makes. (4) The outlook gate turned
  "relative chemosensitivity ... among diffuse gliomas" into two **superlatives**;
  the ban list held "responds better" and "responds well" but not "responds
  best". (5) The radiation-timing trade-off is a **grade-2** conversation and
  was offered to grade-3 readers, for whom waiting is not on the table.
  **The break harness then found six more, four of them real test weaknesses.**
  The §12.6 landing check read `sentences[^1]` of a section with three `###`
  subsections under it, so it was examining a different subsection and **could
  never fire**. The retired-name positives passed on any mention anywhere, so
  deleting the bullet left them green. Nothing banned an invented quantifier
  ("a large majority" for a 35-to-91% spread). And `BritishForms` holds
  spellings, not idiom, so putting "out of hours" back passed every guard.
  **Also:** `blocks/mechanism.md` said "feeling sick and being sick" — the same
  idiom WI-563 fixed in the escalation block one item earlier, on the block that
  composes onto every hub — now fixed; `[MECHANISM]` was sitting before the
  page's own answer to "where does it grow" (§12.11) and the sibling's test for
  that was not copied across; and the `[ESCALATION]` include is pinned to the
  symptoms section, so relocating it goes red.
- [x] **WI-518 Glioblastoma, deepened** *(done 2026-09-08 - `/tumors/glioblastoma`)* -
  a 230-word stub became the §12.3 seventeen-section hub. Reading grade **5.8**,
  **1388 tests** (1358 before), ContentCheck **236/0**, **all 42
  break-mutations proven on LF AND CRLF**, two new glossary terms (bevacizumab,
  tumor treating fields).
  **The engine had no reachable source.** The dossier hangs the entire growth
  story on `academic.oup.com/jnen`, which returns 403 with a 5,558-byte shell -
  the third item to find that domain closed. Rebuilt from PMC12564729 (rapid
  growth lowers oxygen; hypoxia stabilises HIF which upregulates VEGF; GBM
  vasculature is "immature, nonfunctional, tortuous") and PMC2588896 (leaky
  vessels let water and protein out), with NBK537272 added as the firmer source
  for the edema half. The two spatial claims that belonged only to the dead
  source were softened rather than kept.
  **`/review` found five blockers and one of them was a mirror of §12.12's.**
  The short version told every holder of *secondary glioblastoma* that their
  diagnosis has a different name today. **That is false for the IDH-wildtype
  ones, and `/tumors/astrocytoma` routes those readers HERE** - two hubs
  disagreeing about who belongs where, with the wrong one in the higher-traffic
  position. "Secondary" described how a tumor AROSE and said nothing about IDH;
  the slice now has three bullets and routes by the IDH line, as the sibling
  does.
  **Two claims shipped in prose whose only source this item's own test bans** -
  steroids working "quickly" (PMC12406498, the complications audit WI-514
  rejected) and "it usually comes back at or near the same place" (PMC3643853,
  the 21-patient study WI-515 dropped). **Banning a citation is not fixing a
  claim, and the ban makes it look handled.** Written up as **§12.14**.
  **One review finding was REJECTED with evidence.** The shorter-radiation
  paragraph was called an unsourced comparative, quoting the dossier's "a safe,
  well-tolerated *alternative*". The paper says hypofractionation "**is the
  preferred standard of care** ... offering comparable overall survival", and
  EANO independently says "similar activity to irradiation with 60 Gy in 30
  fractions". The dossier had quoted a weaker sentence from the same paper. The
  verbatim is now in the front matter so nobody re-opens it.
  **The honesty note on `/tumors/high-grade-glioma` had become false and its
  guard could not see it.** With three of four destinations filled in, a reader
  was told the glioblastoma page was thin and pointed back to high-grade-glioma
  as "the fuller one". The note now names diffuse midline glioma specifically,
  and the guard requires the named page to be one of the thin ones. **WI-535
  retires it.**
  **The harness found the retired-name guard was a proximity window, not a
  scope.** A ±220-character allowance passed on the superseded name planted in
  an unrelated bullet; tightening to sentence-plus-next still passed off
  "especially common in **older** people" next door. Same-sentence now.
  **Also:** the drug and the device were both unnamed in the first draft
  (WI-515's defect, third occurrence) and the guard was scoped to one section
  while the recurrence paragraph said "a different drug"; Optune's eighteen
  hours a day is published per §12.4 R1; "feeling sick" was the British idiom
  the escalation-block sweep left behind; and a `necrosis` glossary entry was
  written and **withdrawn** because the term fires inside "radiation necrosis".
- [x] **WI-519 T4 Biopsy — how tissue is taken** *(done 2026-09-08)*
  **Done.** `/tests/biopsy`, a new page, eighth under the §12.8 LIBRARY template
  and the first written after three §12.3 hubs in a row. Reading grade **4.3**
  (the lowest of any P5 page so far), ContentCheck **237/0**, all twelve slots
  plus a caregiver section, and **nine inbound links** added so the page is not
  an orphan (`/tests/mri`, `/tests/waiting-for-results`,
  `/treatments/craniotomy` and all six adult glioma hubs).
  **The blocker was mine and no gate could see it: the first draft had no risk
  section at all.** The page described a needle going into someone's brain, told
  the person driving them home to call an ambulance if they suddenly could not
  speak, and never said what could go wrong or why. Found by reading the
  rendered page end to end, which is the **seventh** consecutive item where that
  read caught what the suite could not. §12.8's rule is to answer the
  frightening question in both directions; answering it in neither is worse.
  **Two more bad dossier citations.** (1) §4.1's "a meta-analysis found no
  significant difference in diagnostic yield between the two systems" rests on
  `surgicalneurologyint.com`, which returns a **24-word JavaScript shell**, and
  on PMC10219353 — **not a meta-analysis** but a 72-patient single-centre
  comparison. Per §12.14 the test bans the URL **and the word**. (2) §4.3 bundles
  "a small shaved patch" into a bullet cited to ACS, which contains the string
  "shav" **zero** times and never writes "burr hole" either. The shaved patch
  moved into the questions list, where a question asserts nothing.
  **A paragraph that met the factor-at-the-second-use bar was deliberately not
  factored.** `/treatments/craniotomy`'s "there is no percentage on this page"
  framing was about to be copied verbatim. This page has a better, page-specific
  reason: the published bleeding rates run 2.3% to 72% and the source says the
  spread is about **what each study counted**. Written up in §12.8.
  **A glossary entry was written and withdrawn** (`burr hole`): the page defines
  the word, so the tooltip is suppressed here, and no other page uses it, so the
  entry would have fired nowhere. The two entries that ARE suppressed here were
  checked through their **aliases** first (`needle biopsy` on
  `/tumors/astrocytoma`, `navigation scan` on `/tests/mri`) — the first grep said
  both were unreachable site-wide and it was wrong twice.
  **`/review` found three blockers and ten should-fixes, and it PROVED three of
  them by mutation.** (1) The risk section blamed bleeding on **where the tumor
  sits** — imported from `/treatments/craniotomy`, where location is the story.
  The paper this page cites concludes the opposite (**lesion size** and
  intraoperative bleeding are the two factors that survived its multivariable
  analysis; location is one of six from other people's literature), and the
  wrong cause was propping up a reassurance. (2) The driving prohibition was
  **narrowed**: the source says "you must not drive after having a brain
  biopsy", the draft said "you must not drive **yourself**", filed under a
  packing list, and it never reached the caregiver section where craniotomy
  deliberately puts it. (3)
  `TheAmbulanceListDoesNotOverEscalateASeizureAgainstTheSeizurePage` **could
  not fail on the defect it is named after** — review flattened the bullet to
  "A seizure", WI-510's blocker verbatim, and all eighteen tests stayed green.
  **Three more test weaknesses, all proven the same way.** The tier check was
  **one-directional** (three symptoms on the sibling were silently missing here:
  chest pain, a fall or knock to the head, and pain the medicine is not
  touching); the caregiver-duplication guard was the **bold lead-in** check
  §12.8 already records as insufficient, while the shingle version existed in
  three sibling files (three verbatim block sentences pasted in went green); and
  the lab-pipeline guard was five nouns, which a genuine restatement written
  around them walked through.
  Also: linking a word turns its tooltip off, caught by this page's own tooltip
  test; CRUK is cited for how it feels afterwards and explicitly **not** for
  anesthesia, where it asserts UK practice that contradicts ACS; the outpatient
  review is cited for what it establishes rather than the position it argues
  (the **selection criteria** half was missing, so an overnight stay was
  described as nothing to read anything into); the overnight adult is now stated
  as the condition both sources make it rather than something to ask about; and
  §12.13/§12.14 were reordered in the doc, which had them backwards since
  WI-518.
- [x] **WI-520 T7 Getting ready for surgery — tests and clearance** *(done
  2026-09-09)* — `/tests/getting-ready-for-surgery`, the ninth §12.8 LIBRARY
  page. Reading grade **5.2**, **1444 tests** (1415 before), ContentCheck
  **244/0**, **all 55 break-mutations proven on LF AND CRLF**, three new
  glossary terms (anesthesiologist, advance directive, blood thinner), four
  inbound doors (`/start`, `/tests/biopsy`, `/tests/mri`,
  `/treatments/craniotomy`).
  **The dossier (`tests-library.md` §6) is wrong or unusable in six places** and
  the first two are on this page's headline claims: the ASA fasting table is
  attributed to a document containing **no fasting guidance at all**, and
  "dexamethasone and anti-seizure medicines are generally continued" is
  attributed to an **MRI page** that mentions neither. Also: the ECG age
  threshold is contradicted by its own co-citation ("Age alone may not be an
  indication for ECG"); `accessanesthesiology.mhmedical.com` is 403; the
  group-and-save paragraph has no citation; the restart timings are unconfirmed
  and post-operative. All six recorded in the page's front matter with
  replacements.
  **`/review` ran sixteen mutations and THIRTEEN passed** — six against tests
  named for the defect being reintroduced. Written up as nine new lessons in
  §12.8; every one is now in the mutation table and fails on both line endings.
  **Deliberate omissions, each with a test and a reason:** no per-drug
  antiplatelet stop-day counts (a number a frightened reader CAN follow, next to
  a heading saying not to), no 8-hour fatty-meal tier (no reachable adult
  source states it), no anticoagulant restart timings, no escalation list.
- [x] **WI-521 T11 Follow-up scans and what the results mean** — the canonical
  home for RANO in plain terms, **pseudoprogression**, **radiation necrosis**,
  and scanxiety. On scanxiety the honest position is that no intervention has
  proven effective, so promise nothing and give logistics (book the scan and
  the results close together). Depends on: WI-506.
  **Done 2026-09-09.** `/tests/follow-up-scans`, a new page and the tenth under
  the §12.8 LIBRARY template — the third in the corpus to use **all twelve
  slots**. Reading grade **5.2**, **1477 tests** (1444 before), ContentCheck
  **245/0**, **all 72 break-mutations proven on LF and CRLF**, no new glossary
  terms, and **nine inbound doors** (`/tests/mri`, `/tests/waiting-for-results`,
  `/treatments/radiation-therapy` and all six adult glioma hubs).
  **THE PAGE PUBLISHES NO FREQUENCY, AND THAT IS THE ITEM'S RULING.** This
  ticket asked for "10-30% of glioblastoma patients"; the dossier (§10.3) says
  "20-30% ... range 12% to 64%"; the ASCO Post says "28% to 66%"; PMC10412732
  says "approximately 36%" and reports a biopsy series at 12.4%. Five answers,
  none of them each other, because each counted a different thing — so §12.4
  R2's reasoning applies and the page says it is common **and says why there is
  no number**. Four more dossier defects recorded in the front matter: the
  radiation-necrosis timing (dossier "median 7 months, range 1-25"; the paper
  says **8 months, range 1-41**, in **stereotactic radiosurgery** patients,
  mostly metastases and meningiomas), a biopsy-agreement claim attributed to a
  paper stating **biopsies were never performed**, scanxiety figures absent
  from both open sources, and "four categories" of RANO 2.0 where the reachable
  paper's table has **five**. The surveillance schedule is deliberately absent:
  its source is 403, it would be wrong for most of this page's readers, and it
  is a number a frightened reader can act on.
  **`/review` found the page shared 108 eight-word shingles with
  `/tumors/high-grade-glioma`** — whole paragraphs, already drifted inside one
  commit — while the restatement guard, scoped to three hand-picked tests
  pages, reported zero. Both sections rewritten; the guard now runs corpus-wide
  with a four-entry allowlist of deliberately-shared safety claims. Eleven more
  guards were walked by mutation and all are now claim-shaped. Written up as
  thirteen new lessons in §12.8.
  **Also fixed on a sibling:** `/tumors/high-grade-glioma` listed only two of
  RANO's three routes out of the twelve-week rule, omitting the mandatory
  confirmation scan — the most reassuring one.
  **Known follow-up:** the `rano` glossary entry is reachable nowhere (this is
  the first page to use the word in prose, and it defines it, so the tooltip is
  suppressed here). Pinned by a test that goes red when a later page makes it
  live.
- [x] **WI-522 X1 Watch and wait** — that it is a plan, not a delay; what is
  monitored and how often; the reassuring growth data; **and the measured cost**
  — watch-and-wait carries 4.26× higher risk of a pathological depression score,
  which every other site treats as a footnote. Depends on: WI-502.
  **Done 2026-09-10.** `/treatments/watch-and-wait`, a new page and the fourth
  TREATMENT page under the §12.8 LIBRARY template. Reading grade **5.2**,
  **1512 tests** (1477 before), ContentCheck **246/0**, **all 89 break-mutations
  proven on LF and CRLF**, no new glossary terms, and **eight inbound doors**
  (`/tumors/meningioma`, `/tumors/acoustic-neuroma`, `/tumors/pituitary-tumor`,
  `/tumors/low-grade-glioma`, `/tumors/astrocytoma`, `/tumors/glioma`,
  `/tumors/oligodendroglioma`, `/tests/follow-up-scans`).
  **THE 4.26 FIGURE IS NOT PUBLISHED, AND THAT IS THE ITEM'S RULING — against
  this ticket.** PMC7761113 is a cross-sectional survey of 31 watched and 31
  operated people; 4.26 is a univariate odds ratio (CI 1.19-15.25) that the
  abstract mislabels "Multivariate"; anxiety did not differ; its own conclusion
  is that distress is high "independent of management strategy"; three other
  studies found no watch-and-wait excess; and the dossier's note under the
  figure already said not to print it. The page keeps the cost as its centre of
  gravity (a whole section, and the summary) and prints what the evidence
  supports: worry is common either way in meningioma, one study found more low
  mood in the watched group, another did not, and for an acoustic neuroma people
  watched felt about as well or better.
  **The "reassuring growth data" is meningioma data and was on the wrong paper**
  (Nakamura 2003, 41 patients, via a review, not PMC10180371). Printed as
  direction only, one paragraph per tumor, with diffuse low-grade glioma's
  opposite behaviour stated plainly and guarded by paragraph. Nine dossier
  defects in all, recorded in the front matter, including a "675-patient cohort"
  that is a page number.
  **No per-tumor schedule** (WI-521's ruling held): one attributed range, "from
  every few months to every couple of years".
  **`/review` found three blockers** (a practice survey framed as expert
  disagreement against the guideline the page cites; the same unsourced
  comparative still on `/tumors/low-grade-glioma` in different words; the
  Norway cohort's "most were treated" hidden behind its best sentence), nine
  should-fixes, and twelve mutations that walked the first suite; a second
  `/review` round found four more (the emergency line over-triaged a usual
  seizure; "the rest never needed it" where many had died of other causes). All
  fixed and all in the mutation table. **Also fixed on siblings:** `/tumors/low-grade-glioma`
  said early radiation "buys nothing" and that watching is "harder to live with
  than treating", and EORTC 22845 was stated three ways on three pages (now one
  wording); WI-521's escalation-list guard could not see a list whose every
  bullet wraps (fixed in both suites). The end-to-end read, **tenth item
  running**, found the plan-changes-when-it-grows contradiction and the missing
  vorasidenib option.
- [x] **WI-523 X3 Awake craniotomy and brain mapping** *(done 2026-09-10 —
  `/treatments/awake-craniotomy`)* — split from WI-510
  because the patient has a job to do. **"During mapping you may briefly lose a
  word or the use of a limb, and it comes back — that is the test working."**
  Nobody says this, and it is the sentence that removes the most fear on the
  page. R2 applies to the seizure risk. Depends on: WI-510.
  **Outcome:** reading grade 4.2, **1542 tests** (1512 before), ContentCheck
  249/0, **all 101 break-mutations proven on LF AND CRLF**, one new glossary
  term (`brain mapping`, reachable on `/treatments/craniotomy`), seven inbound
  doors (craniotomy and all six glioma hubs). **The spine had no citation in the
  dossier**; it now rests on NBTS, StatPearls and PMC12414027, and is scoped to
  the electrical TEST — the removal step says outright that a change then "is
  not the test. It may not pass". **No seizure number** (five sources, five
  answers); **"uncommon" printed for the failed awake part**, where the sources
  agree, with its cost (less tumor out, more speech trouble) and the study's
  finding that most were avoidable. `/review` found two blockers: the page told a
  reader they had "not missed out" while its own meta-analysis concludes awake
  surgery "should be strongly considered" near eloquent areas; and "it comes
  back" leaked into the removal phase through the word "testing". Also fixed on
  a sibling: `/treatments/craniotomy` now says pre-op scans "suggest" rather than
  "map" where speech and movement sit, and carries three explicit anchors.
- [x] **WI-524 X11 Steroids** *(done 2026-09-11 — `/treatments/steroids`)* — its
  own URL because of one message: **this might be the drug, not the tumor.**
  Also why you feel better without the tumor shrinking (they treat the swelling,
  not the mass), and why they are tapered. Depends on: WI-502.
  **Outcome:** reading grade **4.7**, **1572 tests** (1542 before), ContentCheck
  **250/0**, **all 72 break-mutations proven on LF AND CRLF**, no new glossary
  terms (both candidates would have fired nowhere), and **ten inbound doors**
  (craniotomy, radiation-therapy, glioblastoma, high-grade-glioma, astrocytoma,
  low-grade-glioma, cns-lymphoma, biopsy, getting-ready-for-surgery,
  follow-up-scans).
  **THE TICKET'S OWN "~28%" IS NOT PUBLISHED, AND IT IS NOT IN THE PAPER IT WAS
  CITED TO.** PMC12406498 contains "proximal" zero times; its only 28 is 28.8%,
  the share of patients with a documented steroid *plan*. The figure is Sturdza
  2008 (88 patients, brain metastases, palliative radiation) reported secondhand
  by cns.org, and six other sources give 10%, 2-60%, 10-90%, 10.6%, 60% and
  4.5%. The page prints no frequency, says why, and prints the direction all
  seven agree on. Corrected at source in SYNTHESIS §3.5, content-pipeline §12.6
  and the dossier.
  **Also ruled on:** "it does not treat the tumor" is **scoped**, because it is
  false for lymphoma (and the page carries the biopsy-first reason); "never stop
  suddenly" became **"not on your own"**, because a three-day post-operative
  course is stopped outright; and the speed claim §12.14 recorded as unsourced
  now rests on two guidelines, printed without the improvement share (two
  sources disagree, and the first draft published the reassuring one).
  **`/review` found five blockers** (the description and title asserting what
  the page refutes; `/treatments/craniotomy` still carrying the absolute stop
  rule four lines above the new door; a fall filed below the tier three sibling
  pages give it; a "nearly all of them fade" sentence governing bullets that do
  not; a headache trigger scoped to the taper) **and walked fourteen of
  eighteen mutations through green.** All are in the harness table.
  **The end-to-end read, twelfth item running,** found that the page explained
  the taper without ever saying why stopping suddenly is dangerous.
  **Also fixed on siblings:** `/treatments/radiation-therapy`'s "somebody who
  **suddenly** cannot get out of a low chair may be having a drug effect" (the
  weakness builds over weeks, and the corpus files sudden weakness as same-day
  or as an ambulance) and its caregiver steroid paragraph, which owned this
  page's spine; `/tumors/glioblastoma`'s "if you feel unlike yourself on them,
  **that is** the drug"; and two `/treatments/craniotomy` sentences plus the two
  tests that pinned their wording. Nine lessons in §12.8.
- [x] **WI-525 X12 Anti-seizure medicines** *(done 2026-09-11 — `/treatments/anti-seizure-medicines`)*
  — same reason: **levetiracetam causes irritability and aggression**, and
  families attribute the personality change to the tumor. Also answers "why
  won't they give me seizure medicine?" — there is a Level A recommendation
  *not* to give them prophylactically to someone who has not had a seizure, and
  readers experience that as being denied something. Driving is jurisdictional:
  say "your rules depend on where you live, here is who to ask", never state
  any. Depends on: WI-502.
  **THE BACKLOG'S HEADLINE CLAIM SURVIVED THE CHECK**, the first time in four
  items: the Level A is verbatim in the **SNO/EANO 2021 practice guideline
  update** (PMC8563323, Walbert et al), and the 2000 AAN parameter it replaces
  is marked [RETIRED]. **The scope trap is in the same guideline and cuts the
  other way** — peri- and postoperative prophylaxis is **Level C, insufficient
  evidence** — so the page carries both halves or it tells a reader given
  levetiracetam for their craniotomy that their team defied the strongest grade
  of advice there is. Three dossier defects: the enzyme-inducing interaction
  claim is cited to PMC8787304, **which mentions none of phenytoin,
  carbamazepine, chemotherapy or steroids** (replaced by PMC6657392, which also
  supplies the half that makes it the reader's problem — a steroid moves the
  seizure-medicine level too); the short-course claim rests on a
  **clinicaltrials.gov protocol**, a plan rather than a result; and "levetiracetam
  and lacosamide are the most commonly used" overstates a source that says LEV
  "is still the most widely used". **No frequency published** for the mood
  effect (seven figures, seven populations). Reading grade **5.2**, **1605
  tests**, ContentCheck **251/0**, **117 break-mutations on LF and CRLF**,
  eleven inbound doors, no new glossary terms. `/review` returned 4 blockers,
  13 should-fixes and **26 proven guard walk-throughs**; the end-to-end read
  (thirteenth item running) found 8 more, the worst an unsourced *"It is not
  common"* on the thoughts-of-self-harm warning. Sibling corrections:
  `/seizures/what-to-do`'s absolute "Never stop seizure medicine suddenly", and
  `/treatments/craniotomy`'s "one of the commonest medicines used to prevent
  seizures". Ten lessons in §12.8.
- [x] **WI-526 X17 Clinical trials as an option** *(done 2026-09-12 — `/treatments/clinical-trials`)*
  — how to think about them, framed as "ask your team". **Never matching** —
  Leal Health and Massive Bio do AI trial matching and that is a regulated
  space this site stays out of (see WI-449). Depends on: WI-502.
  **The scope had to be decided before a word was written**, because `/trials`
  already exists: that Razor page does the FINDING and explains the subject in
  three sentences, while the curated corpus mentioned trials **sixteen times
  across nine pages and explained them nowhere**. So `/trials` answers "what
  exists and where" and this page answers "should I be thinking about this at
  all". **Four dossier defects**, and one is the framing rule's own citation:
  §16 attributes "patients should take trial options TO their health care team"
  to a page that does not contain it, and whose one real sentence is a selling
  line. The phase participant counts are **not NCI's** (20-80/100-300/
  1,000-3,000 against "around 15 to 30"/"50 to 100"/"100 to several thousand"),
  so no counts are printed. **"No one is given a placebo when an effective
  treatment is available" is on neither NCI page it could be cited to**; what
  NCI does say is better, because in the common design the placebo goes **on
  top of** standard treatment. And the dossier lost NBTS's "**beginning a new
  therapy doesn't automatically rule you out**", which matters because most of
  this page's readers arrive mid-treatment. The spine is sourced verbatim: a
  trial is not a last resort, there are **five windows**, and three of them are
  before recurrence. Beside it, the two omissions that make a trials page
  dangerous: **a phase 1 trial is not designed to find out whether the
  treatment works**, and **neither you nor your doctor chooses your group**.
  Reading grade **4.9**, **1633 tests**, ContentCheck **252/0**, **115
  break-mutations on LF and CRLF**, thirteen inbound doors, no new glossary
  terms (and **no glossary term fires on the page at all**, pinned rather than
  hidden). `/review` returned 1 blocker, 13 should-fixes and **47 proven guard
  walk-throughs**; the end-to-end read found 9 more. Nine lessons in §12.8.

### Wave 3 — meningioma, metastases, and the general page

- [x] **WI-527 Meningioma, deepened** — it starts on the covering, not in the
  brain, and the CSF cleft on the scan is the visible proof (genuinely
  reassuring, and explained nowhere) — with the honest limit that compression
  still causes real damage; **location matters more than size**; why so many are
  found by accident; grade 1–3 and what "atypical" changes; defusing "brain
  invasion" on a report; the hormone findings at their three different evidence
  levels (high-dose progestogens accepted causal by European regulators and must
  be stopped on diagnosis; HRT mixed; **standard combined oral contraceptives do
  not increase risk**) — these must not be merged into "hormones cause
  meningioma". Depends on: WI-513.
- [ ] **WI-528 Brain metastases, deepened** — **this is your cancer in a new
  place, not a new cancer**, and it is still named for where it started, which
  is the single most confusing thing for these readers and decides the
  treatment; your original oncologist still leads; the one-way traffic and why;
  one spot or many; focused vs whole-brain radiation with **R3 applied**;
  CNS-penetrant drugs framed as *"ask whether your cancer has been tested for a
  marker with a brain-active drug"*, never "there are pills for brain mets";
  leptomeningeal disease as its own signposted section, including that a
  negative spinal tap does not rule it out. 33–66% of brain metastases are the
  first sign of cancer, which bridges to WI-529. Depends on: WI-513.
- [ ] **WI-529 `all-brain-tumors` — "we don't have a name for it yet"** — the
  page for someone told there is something on their scan. The 7-step pathway,
  which converts silence into "step 3 of 7"; why imaging alone often cannot say
  (the ring-enhancing differential includes an abscess, demyelination and an
  infarct — things that are not cancer at all); **why "benign" is the wrong
  comfort word in the brain** and why registries say *non-malignant* instead;
  there is no stage; primary vs secondary; the tumor board the patient does not
  attend; second opinions including on the tissue itself. Depends on: WI-507.
- [ ] **WI-530 T2 CT + T3 Extra scans for planning** — two pages, one item. CT
  is deliberately short and mostly retrospective: it explains the ER scan that
  started everything. The planning page merges fMRI, DTI, MR spectroscopy,
  perfusion and PET, because patients are never offered "an fMRI" in isolation —
  they are told "we're adding some sequences". fMRI gets the longest section, as
  the only one where the patient has a task. Depends on: WI-506.
- [ ] **WI-531 X6 Proton therapy + X7 Stereotactic radiosurgery** — two pages,
  one item. Proton splits out because **the reader's real question is access,
  not physics** (~50 US centres, travel, and insurance denial as a routine
  appealable step rather than a verdict). SRS splits out because **the name
  misleads** — people think it is surgery — and the day is entirely different.
  Do not state that frame or frameless is standard; the literature is actively
  arguing it. Depends on: WI-511.
- [ ] **WI-532 X9 Targeted and other systemic drugs** — led by "your tumor's test
  result decides this". Vorasidenib, bevacizumab, BRAF/MEK, and the CNS-penetrant
  drugs for metastases. **Bevacizumab is the anti-hype teaching case**: it
  improved progression-free survival but not overall survival in newly diagnosed
  glioblastoma — it helps the scan, not the outcome. Depends on: WI-512.
- [ ] **WI-533 X10 Tumor Treating Fields (Optune)** — a lived-experience decision
  rather than a clinical one: 18 hours a day, shaved head, scalp care, carrying
  the device, caregiver dependency. R1 keeps the 18 hours because it *is* the
  decision. Depends on: WI-502.
- [ ] **WI-534 X13 Shunts and hydrocephalus** — obstructive vs communicating in
  plain words; what a shunt is and what living with one means. R2 applies to
  failure rates. Depends on: WI-502.

### Wave 4 — the remaining tumor types

Each inherits complete libraries, so these are writing items rather than
research items. Same shared contract throughout.

- [ ] **WI-535 Diffuse midline glioma + DIPG, deepened** — written as one item
  because DIPG is the pontine subset of DMG and writing them apart is how they
  drift (WI-412 already had to pin this with a test). H3 K27M. If dordaviprone
  is mentioned, mention that its approval is contested. Depends on: WI-515.
- [ ] **WI-536 Ependymoma, deepened.** Depends on: WI-514.
- [ ] **WI-537 Medulloblastoma, deepened** — including craniospinal radiation and
  the 14-day rule before staging lumbar puncture. Depends on: WI-513.
- [ ] **WI-538 Pediatric brain tumor, deepened** — **the audience is a parent,
  not the patient.** Coordinate with WI-453, which asks whether children get a
  section or a sister site; this item writes the page that exists either way.
  Depends on: WI-513.
- [ ] **WI-539 Pituitary tumor, deepened** — links to WI-553 (transsphenoidal)
  and WI-551 (vision and hormone tests) as its primary paths. Depends on: WI-513.
- [ ] **WI-540 Craniopharyngioma, deepened.** Depends on: WI-539.
- [ ] **WI-541 Acoustic neuroma, deepened** — NF2-related schwannomatosis, renamed
  2022; links to WI-551 (hearing) and WI-531 (SRS). Depends on: WI-513.
- [ ] **WI-542 CNS lymphoma, deepened** — biopsy, **not** resection, and the
  caveat that steroids given before biopsy can obscure the diagnosis (verify
  before publishing). Depends on: WI-519.
- [ ] **WI-543 Spinal cord tumor, deepened** — stays under its own heading. The
  taxonomy is explicit that a spinal cord tumor is not a brain tumor and must
  never surface under a brain filter; WI-412 pinned that with a test and this
  item must not undo it. Depends on: WI-513.
- [ ] **WI-544 ATRT** *(new page)*. Depends on: WI-513.
- [ ] **WI-545 Chordoma** *(new page)*. Depends on: WI-513.
- [ ] **WI-546 CNS germ cell tumor** *(new page)* — links to WI-552 (lumbar
  puncture, AFP/beta-hCG markers) as its primary path. Depends on: WI-513.
- [ ] **WI-547 Hemangioblastoma** *(new page)*. Depends on: WI-513.
- [ ] **WI-548 Sweep: every taxonomy type has a full guide**
  Goal: close the phase honestly rather than approximately.
  Acceptance: a test asserts that **every** slug in `taxonomy.yml` resolves to a
  page carrying the full section template — not merely that a file exists.
  WI-412 shipped with 18 of 24 written and the index said so out loud; this is
  the item that makes that count zero, and the test is what stops a new taxonomy
  entry silently reintroducing a stub. Any type discovered without a guide gets
  written here. Depends on: WI-535…WI-547.

### Wave 5 — the long tail

- [ ] **WI-549 T8 Neurological and thinking/memory testing** — the bedside exam
  and formal neuropsych, together because they are the same conversation and the
  same "am I being judged?" anxiety. **There is no pass or fail**, the day is
  long and tiring, and the results are used for rehab, work and driving.
  Depends on: WI-502.
- [ ] **WI-550 T9 EEG and seizure tests** — often the first test the reader ever
  had. **A normal EEG does not mean you did not have a seizure.**
  Depends on: WI-502.
- [ ] **WI-551 T10 Tests for tumors in specific places** — vision, hormones,
  hearing, as three anchor-linked parts. Readers arrive by link from WI-539 and
  WI-541, not by browsing. Visual field testing gets the "why it feels like
  failing" treatment. Depends on: WI-502.
- [ ] **WI-552 T12 Lumbar puncture and spinal fluid tests** — including that
  atraumatic needles roughly halve post-puncture headache, which gives readers a
  concrete thing to ask for. Depends on: WI-502.
- [ ] **WI-553 X4 Transsphenoidal (pituitary) surgery** — **nothing like a
  craniotomy**: no head incision, no shaved head, nasal recovery, hormone
  follow-up. Inside WI-510 it would mislead a large group of readers, which is
  why it is its own page. Depends on: WI-510.
- [ ] **WI-554 X14 Rehabilitation** — PT, OT, speech; what inpatient rehab is.
  Depends on: WI-502.
- [ ] **WI-555 X15 Palliative care — and what it is not** — the title is the
  content: it is not hospice, and the conflation costs people months of
  available help. Six of 25 caregiver families regretted not accepting help
  sooner. Depends on: WI-502.
- [ ] **WI-556 X16 Fertility and treatment** — **time-critical**: options close
  once treatment starts, and ASCO says discuss it with every patient of
  reproductive age. Cuts across surgery, radiation and chemo, and is
  systematically missed. Depends on: WI-502.
- [ ] **WI-557 The tumor picker and the /tumors index rebuild**
  Goal: two ways in — one for the reader who knows their words, one for the
  reader who does not.
  Acceptance: an "I was diagnosed with…" picker that jumps straight to a tumor
  page, on the home page and reachable from the nav, **working with JavaScript
  off** (a form that submits, not a JS handler); the existing grouped index
  kept and updated — the grouping answers "is mine a glioma?" before a word is
  read, and the "we are still writing this one" state is gone once WI-548
  passes; the index's written/total counter removed or reduced to a provenance
  line. Every new library page reachable from somewhere (the WI-412a lesson:
  `/tumors` shipped with nothing linking to it, and the link check was
  structurally blind to it — the sitemap-reachability test added then must cover
  these). Depends on: WI-548.

- [ ] **WI-561 Images on curated pages — the mechanism** *(code, blocks WI-562)*
  Goal: give a curated page a way to carry an image, with everything the site's
  existing rules already demand of one.
  **Why it is a separate item from WI-562:** there is no image support on
  curated pages today at all. Markdig will emit a bare `<img>` from `![]()`,
  but nothing styles it, nothing carries a caption or a credit, nothing sizes it
  on a phone, and `print.css` has no rule for it. Sourcing 30 images before that
  exists means 30 images with nowhere to go.
  Acceptance:
  - An author writes one image per figure in Markdown; it renders as a real
    `<figure>` with a `<figcaption>`. The caption is **content**, not
    decoration: it is graded by ContentCheck along with the rest of the page,
    so it obeys the 6.0 gate like every other sentence.
  - **Alt text is required and the build fails without it.** WCAG AA is a hard
    requirement (`.claude/CLAUDE.md`), and an empty `alt=""` must be a
    deliberate, declared choice for a decorative image rather than the default
    a hurried author gets. Prove the failure by removing one.
  - **Every image carries a source and a licence, checked mechanically.** The
    glossary learned this at WI-505 and the pages learned it at WI-502: a
    citation nobody can follow is not a citation. Model it on
    `wwwroot/img/cards/IMAGE-CREDITS.md`, but make it a **front-matter field on
    the page** so the credit travels with the image and ContentCheck can see it.
  - Sized for a phone first (the site is verified at 390px since WI-440) and
    lazy-loaded below the fold.
  - **Print behaviour decided deliberately, and verified by printing to PDF,
    not by reading the CSS.** WI-560 found that `print.css` had been silently
    deleting every glossary term from every printed page since WI-101, and it
    was only ever visible on paper. An image that becomes a full blank page, or
    vanishes, is the same class of bug.
  - A page with no images renders byte-identically to today (the WI-501
    regression property).
  - **CRLF-safe.** Any parsing added here gets proven on a CRLF copy — this repo
    has `core.autocrlf=true` and CI is Linux/LF, so a Windows-only break stays
    green in CI forever (WI-501, WI-506, WI-508).
  Out of scope: choosing or sourcing any actual image. That is WI-562.
  Refs: docs/content-pipeline.md §5 (the automated gates), §12.8;
  wwwroot/img/cards/IMAGE-CREDITS.md; PLAN.md §5. Depends on: nothing.

- [ ] **WI-562 Images Needed — the per-page slot inventory** *(Dan sources)*
  Goal: for every curated page, say what images it wants, what kind each one is,
  and where on the page it goes — so Dan can go and find them without having to
  re-read each page first.
  **Why:** the P5 pages are walls of text. `/tests/waiting-for-results` is 2,533
  words with no picture in it, and the audience may be cognitively impaired.
  Dan's call, 2026-09-03: he sources the images himself, public domain or free.
  **Density: one image per ~500 words of body text, minimum one per page.**
  Words, not source lines — the Markdown is hard-wrapped, so a line count
  measures the author's editor rather than the reader's screen. It also scales
  itself: a tumor hub is ~180 words today and becomes a 17-section hub at
  WI-513, and the rule moves it from one slot to four without anyone editing
  this ticket. As the pages stand that gives:
  | Page | Words | Slots |
  |---|---|---|
  | `/tests/waiting-for-results` | 2533 | 5 |
  | `/tests/mri` | 2109 | 4 |
  | `/tests/pathology-report` | 1963 | 4 |
  | `/seizures/living-with` | 1748 | 3 |
  | `/seizures/what-to-do` | 963 | 2 |
  | `/start` | 577 | 1 |
  | each `/tumors/*` (18) | ~180 | 1 |
  **Four kinds of image, and they are not equally easy to get. Say which kind
  each slot is, because two of them cannot be shopped for:**
  1. **Sourceable photo** — free stock or public domain, Dan can search for it
     directly. *Examples: an MRI scanner in a room; a lab bench with slide
     trays; a microscope; a gloved hand holding a specimen pot; an empty
     waiting room; hands holding paperwork at a kitchen table.*
  2. **Mock document** — a made-up example with the parts labelled. **Never a
     real report**: a real one carries PHI, and no stock site has one. Someone
     has to build it. *Examples: a sample pathology report with the nine parts
     called out (the single highest-value image on the whole site — it is
     `/tests/pathology-report`'s entire subject); a report showing "final
     diagnosis" at the top with the evidence underneath; a before/after of a
     report and its addendum.*
  3. **Simple diagram** — a line drawing we make. Highest explanatory value,
     and it is a design job rather than a search. *Examples: tissue → fixative →
     wax block → slide → microscope, as five boxes (this is WI-507's
     step-by-step, which is currently nine numbered paragraphs); the layered
     report as stacked bands; a timeline of the wait showing the quick answer,
     the microscope answer and the gene results arriving at different points.*
  4. **Public-domain medical imagery** — real scans and slides from Wikimedia
     Commons or NIH open sets. **Each licence checked individually**, and
     **never an NCI embedded image** (PLAN.md §5 bars them outright).
     *Examples: a normal brain MRI; an H&E-stained slide; a contrast-enhanced
     scan.*
  Acceptance:
  - Every curated page has its slots listed: position (which heading it follows),
    which of the four kinds, what it should show, and one sentence of draft alt
    text so the accessibility requirement is not an afterthought at paste time.
  - **The mock-document and diagram slots are called out separately from the
    photo slots**, because they are work rather than shopping and Dan should not
    discover that halfway through.
  - No image is chosen or committed in this item. It produces the list.
  - **No AI-generated imagery**, consistent with the standing rule on feed cards.
  Refs: PLAN.md §5; docs/content-pipeline.md §12.8;
  wwwroot/img/cards/IMAGE-CREDITS.md. Depends on: WI-561 (the slots need
  somewhere to go).

---

## Phase P2a — Benefits & Disability (static hub) — not yet itemized
## Phase P2b — Newly Diagnosed pathway — not yet itemized
## Phase P2c — Tumor types + glossary expansion — **superseded by Phase P5**
## Phase P2d — Side effects / treatments / medications-lite — **superseded by Phase P5**
## Phase P3 — Patient stories — not yet itemized

P2c and P2d were reserved for exactly the work Phase P5 now itemizes — tumor
types plus glossary expansion, and treatments plus side effects. They are marked
superseded rather than deleted so the roadmap's numbering still resolves; do not
decompose them.

Run `/pm decompose P2a` (etc.) when the preceding phase nears completion.
