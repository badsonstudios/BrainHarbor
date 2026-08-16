# BrainHarbor — Work-Item Backlog

The tracker for this project (no GitHub issues). Derived from
[roadmap.md](roadmap.md) and the other design docs; live state lives in
[../PROGRESS.md](../PROGRESS.md).

**Format:** items are `WI-<phase><nn>`, one-evening sized (~1–3 h), each with a
Goal, testable Acceptance criteria, and Refs into the design docs. `[user]` =
Dan does it, not the assistant. Work top-to-bottom within a phase unless
*Depends on* says otherwise. `/pm` maintains this file; `/next-item` executes
items and checks them off. **Never renumber existing items — append.**

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

- [ ] **WI-429 Decide what happens to the readiness dial on /research**
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

  ### Recommended before spending anything
  1. **Deploy at quiet hours and batch changes into fewer releases.** Free, and
     costs nothing at all while the site is unlaunched.
  2. **Collect two or three more measurements first.** The check now logs this
     automatically on every deploy, at no effort. One sample of two minutes is
     not a trend, and the previous release (CI-only) passed on the first attempt
     with no window at all — so the length varies and the cause is not yet
     pinned. Deciding to buy a tier on a single data point would be guessing
     with money.
  3. Only then weigh the tier.

  WI-431 made the window visible and shorter; it cannot remove it. Removing it
  means deploying to a staging slot, warming it, and swapping — and **slots
  require Standard tier**, so this is a monthly bill, not a code change. The
  plan currently runs on a shared B1 (WI-401 chose it deliberately at ~$1-3/mo
  incremental).
  **Do not decide this until WI-431's body capture has caught the failure at
  least once.** If the cause turns out to be app start-up or the DbUp migration
  holding its advisory lock, it may be fixable in code for nothing, and paying
  for slots would be buying the wrong thing.
  Then weigh: how often deploys actually happen once the site is not being
  rebuilt daily, whether they can simply run at a quiet hour, and what a minute
  of 500s on the inner pages is worth when the custom error page still carries
  the helpline band and the CareLine number.
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

## Phase P2a — Benefits & Disability (static hub) — not yet itemized
## Phase P2b — Newly Diagnosed pathway — not yet itemized
## Phase P2c — Tumor types + glossary expansion — not yet itemized
## Phase P2d — Side effects / treatments / medications-lite — not yet itemized
## Phase P3 — Patient stories — not yet itemized

Run `/pm decompose P2a` (etc.) when the preceding phase nears completion.
