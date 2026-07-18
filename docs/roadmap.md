# BrainHarbor — Roadmap

Companion to [PLAN.md](../PLAN.md). Assumptions: **solo builder, nights & weekends, no external deadline.** V1 is the aggregation product; the effort is in pipeline engineering and prompt iteration rather than prose (~4–5k hand-written words vs the hub's eventual ~20k).

Each milestone ends in something shippable. The digest is deliberately last in v1 — it can slip without hurting launch.

## M0 — Skeleton (1–2 weekends) — local-first, $0 hosting

- [ ] Buy **brainharbor.org** (~$12/yr — the only spend until M4; grab it before someone else does).
- [ ] Create the **private GitHub repo** (`gh repo create` — CLI is already authenticated) and push the scaffold.
- [ ] Repo scaffold: **two projects** — `BrainHarbor.Web` (.NET 10 Razor Pages, Htmx.Net, Markdig, Dapper, DbUp) and `BrainHarbor.Pipeline` (console app). No Hangfire — the pipeline lives on your PC.
- [ ] `docker-compose.yml`: dedicated Postgres 16 container on port **5433** (alongside the existing container, no collision), named volume.
- [ ] GitHub Actions: build + tests on every push (no deploy yet — that arrives at M4).
- [ ] Keys into `dotnet user-secrets`: NCBI/PubMed (free, instant) + a generated sync-API key shared by Web and Pipeline. **No Anthropic API key** — summarization uses the installed Claude Code CLI.
- [ ] Legal boilerplate v0: medical disclaimer, privacy (no tracking, no list sharing), terms.

## M1 — Design system & shell (2–3 weekends)

- [ ] Layout + tokens with the audience constraints baked in: big type (18px+), high contrast, generous spacing, large-text toggle, visible focus, print stylesheet.
- [ ] Persistent helpline band + `/get-help-now` (988, Crisis Text Line, ABTA CareLine).
- [ ] Static-page engine (Markdown + front matter → cached pages) + glossary tooltip extension.
- [ ] The 6 hand-written pages: home shell, /about, /how-we-write (v0), /start interim, /digest landing, legal.
- [ ] CI gates: readability for static pages, axe smoke test.

**Exit:** the site shell is complete and honest (running locally) — it just has no feed yet.

## M2 — Ingestion + sync API + browse (3–4 weekends) ⭐ first real value

- [ ] `taxonomy.yml` + `aggregated_items` schema + `source_sync_state`.
- [ ] **Sync API** on the Web app: `GET /api/sync/state`, `POST /api/sync/check`, `POST /api/sync/items` (API-key auth, idempotent upsert).
- [ ] Pipeline fetchers: PubMed (brain-tumor query set), NCI RSS, ScienceDaily brain-tumor feed, medRxiv/bioRxiv metadata. Hard-rule pre-filter (wrong-disease keywords, junk) — the Claude classifier comes in M3; until then items upload with original titles, badged by source_kind only.
- [ ] Admin: Identity (+TOTP 2FA), review queue v0 (approve/reject raw items), per-source health page.
- [ ] Windows Task Scheduler: daily run with missed-start catch-up + "N items awaiting review" notification.
- [ ] `/research` feed: htmx filters (date/source/kind now; tumor type once classified), load-more, no-JS fallback.

**Exit:** the loop works end-to-end on your machine — scheduled task fetches, uploads pending, you approve, the feed shows it. Reads clinical (raw titles), but it's already a usable glioma radar.

## M3 — Classification + plain-language summaries (3–5 weekends) ⭐ the differentiator

- [ ] Golden set first: ~30 hand-verified items (classification + ideal summary) as test fixtures.
- [ ] **Claude Code CLI wrapper** in the Pipeline app (`claude -p --output-format json`): classify step (closed taxonomy, relevance tiers, stage detection — `excluded` items never uploaded) + summarize step (template per content-pipeline §9, JSON schema validation, numeral post-check, banned phrases). Prompt version logged per item.
- [ ] Review queue v1: side-by-side summary-vs-abstract view, approve/edit/reject, correction notes. Front page flips to `patient_relevant` default + early-stage toggle.
- [ ] Item permalink pages: template blocks, stage badge, provenance box ("reviewed by a human before publishing"), glossary tooltips, "report a problem" (→ flag queue), sitemap.xml + structured data.
- [ ] /how-we-write updated to describe the real pipeline, including the human-approval gate.

**Exit:** BrainHarbor now does the thing nothing else does — daily brain-tumor research in plain language. This is launchable.

## M4 — Azure + trials + digest → **v1 launch** (3–4 weekends)

- [ ] **Provision Azure now** (deferred from M0; the meter starts here, ~$30/mo): App Service B1 (Always On) + PostgreSQL Flexible B1ms; point brainharbor.org at it; add the deploy + migration steps to GitHub Actions; backfill the feed by running the pipeline against prod.
- [ ] ClinicalTrials.gov v2 fetcher in the Pipeline app → `trials_cache` + trial_update feed items; `/trials` browse + near-me (live server-side query; geolocation/ZIP → ZCTA centroids).
- [ ] Digest: ESP account (Buttondown/Kit), double opt-in signup; a **weekly Pipeline run** drafts the issue from approved items → review in admin → send via ESP API; past issues published at `/digest/{n}`.
- [ ] Pre-launch pass: Lighthouse + axe everywhere, cheap-Android + throttled-3G test, feed.xml, meta/OG tags so shared items unfurl well in Facebook groups.
- [ ] **Soft launch:** share in 2–3 communities (r/braintumor, glioma Facebook groups — read each community's rules first). The pitch is the honest one: "daily brain-tumor research, translated into plain English, free, no ads, no tracking."

## Phase 2 — The static hub (the old v1, unchanged content plan)

Order by differentiation, one content sprint each, ~1–2 verified pages/week:

- **P2a — Benefits & Disability** (~10 pages) — still the strongest static differentiator (3-listing decision tree; friend reviews against his SSDI experience). Also the long-tail SEO wedge.
- **P2b — Newly Diagnosed pathway** (~7 pages) — replaces the interim /start page.
- **P2c — Tumor types** (~6 pages) + glossary expansion.
- **P2d — Side effects, treatments, medications-lite** (~10 pages).

## Phase 3 — Stories

- Crisis-disclosure protocol drafted **before** the form opens; consent language; moderation dispositions.
- Submission form (spam defenses), moderation queue, story pages with taxonomy browsing, Blob storage + EXIF stripping.

## Continuous / recurring

| What | When |
|---|---|
| Source-health check (is anything silently stale?) | glance at admin page weekly; alert if a source >3 days stale |
| Review queue (approve/edit/reject pending items) | ~5 min daily habit |
| Outbound link check + PMID retraction check | monthly (automated) |
| Prompt changes → golden set in CI | every change |
| Benefits dollar figures (once P2a exists) | every December |
| Dependency/security updates | Dependabot + monthly pass |
| Community listening → taxonomy/filter/content backlog | ongoing |

## Success metrics (define now, measure from M2)

1. **Return visits** — the metric that matters for a feed: do people come back weekly? (Privacy-safe: returning-visitor ratio from the analytics counter, digest open proxy = click-throughs.)
2. **Digest health:** subscriber growth, near-zero spam complaints, unsubscribe < 1%/issue.
3. **Summary quality:** flag rate < ~2% of items; spot-check error rate trending down; zero uncorrected known errors.
4. **Long-tail search:** item permalinks ranking for "[drug/trial] glioma explained"-shaped queries by ~6 months.
5. **Human signal:** unsolicited "this is how I keep up now" messages / community reshares. Five of these beat 50k pageviews.
