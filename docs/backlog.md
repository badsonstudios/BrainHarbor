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

- [ ] **WI-101 Design tokens + base layout + print stylesheet**
  Goal: the accessible visual foundation (audience constraint, not polish).
  Acceptance: `_Layout` with semantic landmarks + skip link; CSS custom
  properties (type scale ≥18px base, spacing, high-contrast palette); visible
  focus states; print.css produces a clean print of a sample page.
  Refs: PLAN.md §3, sitemap.md nav model. Depends on: WI-005.

- [ ] **WI-102 Large-text toggle + a11y smoke test**
  Goal: WCAG AA is enforced by tooling from the start.
  Acceptance: large-text toggle (cookie-persisted, no-JS fallback via
  querystring); Playwright + axe-core smoke test on the shell passes with 0
  serious/critical, wired into CI.
  Depends on: WI-101, WI-006.

- [ ] **WI-103 Helpline band + /get-help-now**
  Goal: the always-visible "talk to a human" affordance.
  Acceptance: persistent helpline band on every page (ABTA CareLine, large tap
  target); /get-help-now page with 988, Crisis Text Line 741741, org helplines;
  present on custom 404/500 pages too.
  Refs: PLAN.md §3, sitemap.md. Depends on: WI-101.

- [ ] **WI-104 ContentStore: Markdown static pages**
  Goal: static pages authored as Markdown + YAML front matter, per the schema.
  Acceptance: Markdig pipeline + in-memory cache; front-matter parsing
  (content-pipeline.md §3 schema); pages route by section/slug; unit tests for
  parsing and routing.
  Refs: content-pipeline.md §3, architecture.md §3. Depends on: WI-003.

- [ ] **WI-105 Glossary + inline tooltip extension**
  Goal: the inline-definitions differentiator, accessible.
  Acceptance: glossary term file format; Markdig extension marks first
  occurrence per page → `<button>` tooltip meeting WCAG 1.4.13 (focusable,
  dismissible, touch-OK) with no-JS fallback link to /glossary#term; /glossary
  A–Z page; unit tests incl. escape hatches (`%%term%%`, `!%term%`).
  Refs: content-pipeline.md §6. Depends on: WI-104.

- [ ] **WI-106 ContentCheck CI gate**
  Goal: the readability promise is machine-enforced.
  Acceptance: `tools/BrainHarbor.ContentCheck` computes Flesch-Kincaid per
  content page (fail > 8.5, warn ≥ 7.5), validates front matter, reports
  overdue `review_due`; runs in CI on changed content; unit-tested against
  known-grade sample texts.
  Refs: content-pipeline.md §5. Depends on: WI-104, WI-006.

- [ ] **WI-107 Write the static shell pages**
  Goal: the ~6 hand-written pages that make the shell honest.
  Acceptance: home shell, /about, /how-we-write v0, /start (interim), /digest
  landing (no signup yet), /privacy + /terms + disclaimer partials — all
  passing the ContentCheck gate; medical + not-legal-advice disclaimers render
  from front-matter flags.
  Refs: sitemap.md writing budget, content-pipeline.md §2/§4. Depends on: WI-105, WI-106, WI-103.

## Phase M2 — Ingestion + sync API + browse

- [ ] **WI-201 Core schema + taxonomy**
  Goal: the data foundation for everything aggregated.
  Acceptance: DbUp migration for `aggregated_items` + `source_sync_state`
  exactly per data-model.md; `Content/taxonomy.yml` with initial tumor slugs;
  taxonomy loader with tests.
  Refs: data-model.md. Depends on: WI-004.

- [ ] **WI-202 Sync API**
  Goal: the only write surface, secure and idempotent.
  Acceptance: `GET /api/sync/state`, `POST /api/sync/check`, `POST
  /api/sync/items` per architecture.md §4; API-key header auth (401 without),
  rate limiting; upsert idempotency proven by an integration test that uploads
  the same batch twice; source_sync_state updated on success.
  Refs: architecture.md §4, security reference. Depends on: WI-201.

- [ ] **WI-203 Pipeline skeleton + sync client**
  Goal: the console app frame every fetcher plugs into.
  Acceptance: config binding (base URL, API key via user-secrets), typed
  sync-API client, `--once` run mode, structured console logging, per-source
  isolation (one failing source doesn't kill the run); integration-tested
  against the locally-running Web app.
  Refs: architecture.md §3. Depends on: WI-202.

- [ ] **WI-204 PubMed fetcher + hard-rule pre-filter**
  Goal: the primary research source flowing end-to-end.
  Acceptance: E-utilities query set for brain tumors with `reldate` windowing
  driven by sync state (self-healing catch-up); API-key + 10 rps politeness;
  wrong-disease/junk hard-rule filter with unit tests; new items upload as
  pending with raw titles.
  Refs: PLAN.md §5, roadmap M2. Depends on: WI-203.

- [ ] **WI-205 RSS fetchers: NCI + ScienceDaily**
  Goal: the news sources, licensing rules respected.
  Acceptance: NCI RSS (full text OK) and ScienceDaily brain-tumor feed
  (headline+summary+link ONLY) fetchers; per-source licensing enforced in
  code; dedupe against existing items via /check; tests with recorded feeds.
  Refs: PLAN.md §5. Depends on: WI-203.

- [ ] **WI-206 Preprint fetcher: medRxiv/bioRxiv**
  Goal: preprints in the pipeline, permanently badged.
  Acceptance: metadata-only fetcher; `source_kind='preprint'` forced; items
  can never carry `patient_relevant` (rule + test); "not peer-reviewed"
  badge data present.
  Refs: PLAN.md §5, content-pipeline.md §9. Depends on: WI-203.

- [ ] **WI-207 Admin auth**
  Goal: a locked front door for moderation.
  Acceptance: ASP.NET Identity, single seeded admin, TOTP 2FA enforced, no
  registration endpoint; admin area route group requires auth; anti-forgery on
  POSTs; login/lockout tested.
  Refs: security reference. Depends on: WI-004.

- [ ] **WI-208 Review queue v0**
  Goal: the human gate exists.
  Acceptance: admin list of pending items (newest first, source/kind badges);
  approve → published, reject → rejected (htmx actions, no-JS fallback);
  status transitions audit who/when; tests for transitions.
  Refs: data-model.md lifecycle. Depends on: WI-207, WI-201.

- [ ] **WI-209 /research feed**
  Goal: the public product page, v0.
  Acceptance: published items with filters (date, source, kind) as htmx
  partials degrading to querystring links; "load more" paging; item rows per
  sitemap.md anatomy (minus plain-language fields); response caching 5–15 min.
  Refs: sitemap.md, architecture.md §5. Depends on: WI-208.

- [ ] **WI-210 Source health + scheduled task**
  Goal: the loop runs itself and staleness is visible.
  Acceptance: admin source-health page ("PubMed last synced N days ago", last
  error); pipeline ends with a desktop notification ("N items awaiting
  review"); Task Scheduler registration script (daily, run-after-missed-start)
  checked into the repo with setup instructions.
  Refs: architecture.md §6/§8. Depends on: WI-204.

- [ ] **WI-211 M2 end-to-end shakedown**
  Goal: prove the whole loop before building on it.
  Acceptance: from a fresh DB: scheduled run fetches all sources → items
  pending → approve in admin → visible on /research; a second run ingests 0
  duplicates; findings fixed or filed as new items; PROGRESS.md notes the
  shakedown result.
  Depends on: WI-205, WI-206, WI-209, WI-210.

## Phase M3 — Claude classification + plain-language summaries

- [ ] **WI-301 Golden set**
  Goal: the quality yardstick exists before any prompt is written.
  Acceptance: ~30 real fetched items hand-verified (correct tumor tags,
  relevance tier, stage; ideal summary for ~10 of them) as versioned fixtures
  in `tests/`; a documented rubric for adding cases.
  Refs: content-pipeline.md §10. Depends on: WI-211.

- [ ] **WI-302 Claude Code CLI wrapper**
  Goal: reliable programmatic access to the local `claude` CLI.
  Acceptance: invokes `claude -p --output-format json` with a versioned prompt
  template; parses/validates against a JSON schema; one retry on malformed
  output; hard failure → item flagged unsummarized (never guessed); timeout
  handling; wrapper unit tests with a fake CLI.
  Refs: architecture.md §5, content-pipeline.md §9. Depends on: WI-203.

- [ ] **WI-303 Classify step**
  Goal: items sorted for a patient audience automatically.
  Acceptance: classify prompt (closed taxonomy, relevance tier, research
  stage); `excluded` items are not uploaded; preprints capped at early_stage
  (rule + test); golden-set classification accuracy reviewed and recorded;
  `classify_model` + prompt version stamped per item.
  Refs: content-pipeline.md §9. Depends on: WI-301, WI-302.

- [ ] **WI-304 Summarize step**
  Goal: the differentiator — plain-language summaries with guardrails.
  Acceptance: summarize prompt implementing the 6-block template; numeral
  post-check (every number traceable to source, mismatch → flagged);
  banned-phrase scan; reading-level check on output; golden-set run reviewed;
  `summary_model` + prompt version stamped.
  Refs: content-pipeline.md §9/§11. Depends on: WI-303.

- [ ] **WI-305 Review queue v1**
  Goal: reviewing 10–30 summaries/day takes ~5 minutes and means something.
  Acceptance: side-by-side summary vs source abstract; inline edit before
  approve; correction-note field (rendered publicly per content-pipeline §10);
  keyboard-friendly approve/reject; flagged items surfaced first.
  Depends on: WI-304, WI-208.

- [ ] **WI-306 Item permalink pages**
  Goal: the shareable, indexable unit of the site.
  Acceptance: `/research/{slug}` renders the 6 template blocks, stage badge,
  provenance box with human-review disclosure, glossary tooltips active in
  summaries, one-tap "report a problem" (→ summary_flagged + admin queue);
  slugs generated on approval.
  Refs: sitemap.md, content-pipeline.md §9. Depends on: WI-305, WI-105.

- [ ] **WI-307 Feed flip to patient-first**
  Goal: the front page now serves the audience.
  Acceptance: /research defaults to `patient_relevant`; "show early-stage
  research" toggle (persisted, no-JS fallback); tumor-type filter from
  taxonomy; plain titles shown where available.
  Depends on: WI-306.

- [ ] **WI-308 SEO + real /how-we-write**
  Goal: discoverable and honest.
  Acceptance: sitemap.xml (items + static), Article/MedicalWebPage +
  BreadcrumbList structured data, meta/OG tags (items unfurl well when
  shared); /how-we-write rewritten to describe the real pipeline incl. the
  human gate; feed.xml RSS of published items.
  Refs: PLAN.md §10. Depends on: WI-306.

- [ ] **WI-309 Site search**
  Goal: one search over items + static pages.
  Acceptance: /search with htmx live results (no-JS form fallback) across
  published items (Postgres FTS) and static pages; drug-name typo tolerance
  can wait (note for later).
  Depends on: WI-306.

## Phase M4 — Azure + trials + digest → v1 launch

- [ ] **WI-401 `[user]`+assisted Provision Azure**
  Goal: the site exists on the internet (meter starts, ~$30/mo).
  Acceptance: App Service B1 (Always On) + Postgres Flexible B1ms; deploy +
  DbUp migration steps in GitHub Actions; brainharbor.org DNS + managed TLS;
  admin 2FA re-verified in prod; pipeline pointed at prod URL with prod
  SYNC_API_KEY; feed backfilled.
  Refs: architecture.md §8/§9. Depends on: WI-309 (M3 complete).

- [ ] **WI-402 Trials fetcher**
  Goal: trials flow like everything else.
  Acceptance: ClinicalTrials.gov v2 fetcher → `trials_cache` + trial_update
  feed items (429-defensive); plain-summary treatment via existing pipeline;
  tests with recorded responses.
  Refs: PLAN.md §5, data-model.md. Depends on: WI-304.

- [ ] **WI-403 /trials browse + near-me**
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

- [ ] **WI-408 `[user]` Soft launch**
  Goal: first real users.
  Acceptance: shared in 2–3 communities (rules read first) with the honest
  origin story; feedback captured as new backlog items via /pm.
  Depends on: WI-407.

---

## Phase P2a — Benefits & Disability (static hub) — not yet itemized
## Phase P2b — Newly Diagnosed pathway — not yet itemized
## Phase P2c — Tumor types + glossary expansion — not yet itemized
## Phase P2d — Side effects / treatments / medications-lite — not yet itemized
## Phase P3 — Patient stories — not yet itemized

Run `/pm decompose P2a` (etc.) when the preceding phase nears completion.
