# BrainHarbor — Tech Architecture

Companion to [PLAN.md](../PLAN.md). Two applications now: a simple **cloud website** and a **local pipeline app** on Dan's PC that does all gathering and Claude processing. No Anthropic API key — summarization runs through the locally-installed **Claude Code CLI** (existing subscription) in headless mode.

## 1. Topology — the local-pipeline split

```
┌─ Dan's PC (Windows, daily scheduled task) ─────────────────┐
│  BrainHarbor.Pipeline (console app, stateless)             │
│    1. GET  /api/sync/state        → per-source cursors     │
│    2. fetch PubMed / NCI RSS / ScienceDaily /              │
│       medRxiv-bioRxiv / ClinicalTrials.gov                 │
│    3. POST /api/sync/check        → which IDs are new?     │
│    4. classify + summarize NEW items via Claude Code CLI   │
│       (claude -p, JSON out, local post-checks)             │
│    5. POST /api/items             → upsert as 'pending'    │
└────────────────────────────────────────────────────────────┘
                        │ HTTPS + API key (no DB credentials ever leave the server)
                        ▼
┌─ Website (Azure App Service + Postgres — or localhost+Docker in dev) ─┐
│  BrainHarbor.Web (Razor Pages + htmx)                                 │
│    Public: /research feed, item pages, /trials, /digest, static pages │
│    Sync API: state / check / items  (API-key auth, rate-limited)      │
│    Admin (Identity + 2FA): REVIEW QUEUE → approve/edit/reject         │
│      → approval flips items to 'published'                            │
│    No background jobs. No Hangfire. Read-mostly.                      │
└───────────────────────────────────────────────────────────────────────┘
```

Why this shape (decided 2026-07-18):

- **No Anthropic API key needed** — Claude Code's headless mode (`claude -p "<prompt>" --output-format json`) does classification and summarization under the existing subscription. Volume (~10–30 new items/day) is comfortably within normal interactive-scale use.
- **The local app is stateless.** Dedupe and cursors live in the cloud DB; the app asks the site's API "which of these are new?" before spending any Claude time, and the upload is an **idempotent upsert** on `(source, external_id)` — so a crashed or re-run batch can never create duplicates. A week with the PC off self-heals: the next run just processes a bigger window.
- **No direct DB connection from the PC** (explicitly preferred): the only credential on the PC is a long random API key, revocable server-side in seconds.
- **Publish mode (WI-212):** Auto by default — a summarized item that passes the automated safety checks (numeral traceability, banned-phrase scan, reading level) publishes itself; flagged or unsummarized items wait in the admin review queue. Review mode (`Publishing:Mode=Review`) requires a person for everything. Auto-published items are marked `reviewed_by='auto'` and the item page discloses it. See content-pipeline.md §"Publish mode".
- **Dev = prod, minus Azure:** until M4 the "cloud" is the site running locally against Docker Postgres, and the pipeline points at `localhost`. The publish path never changes, only the base URL and key.

## 2. Web stack: Razor Pages + htmx on .NET 10

**ASP.NET Core Razor Pages + htmx (via `Htmx.Net`), .NET 10 LTS, Dapper, DbUp migrations.** (Unchanged from the 2026-07-12 decision; abbreviated rationale:)

1. **Htmxor** (the Blazor↔htmx bridge the original plan assumed) has been unmaintained since Sept 2024; .NET 9/10 added no first-class Blazor-fragment story.
2. **`Htmx.Net`** is actively maintained (v1.12.0, Mar 2026); partial views are the natural fragment mechanism; standard antiforgery.
3. **It's the work stack** — the upcoming job pairs htmx with Razor Pages/MVC, so this is direct job prep. Blazor knowledge transfers via shared Razor DNA.

Alternatives (Blazor SSR + DIY glue, Rizzy, Hydro, SSGs) were evaluated and rejected — see git history of this file for the full table if ever needed.

## 3. The solution layout

```
src/BrainHarbor.Web/            The website
  Pages/                        Feed, item pages, trials, digest, static, admin (review queue)
  Api/                          Sync endpoints: GET state, POST check, POST items
  Services/ContentStore/        Markdig + cache for static pages & glossary tooltips
  wwwroot/                      htmx (~14KB), site.css, print.css — the whole JS budget

src/BrainHarbor.Pipeline/       The local console app (stateless)
  Sources/                      One ISourceFetcher per source — typed HttpClient + Polly each
  Claude/                       Claude Code CLI wrapper: prompt templating, JSON parse,
                                schema validation, one retry, numeral post-check
  Publishing/                   Sync-API client (state/check/upload)
  Prompts/                      Versioned prompt templates (classify, summarize)

tools/BrainHarbor.ContentCheck/ CI gate for static pages (readability, front matter)
tests/                          Web tests + pipeline tests + golden-set fixtures
docker-compose.yml              Local Postgres 16 on port 5433 (beside the existing container)
```

## 4. The sync API (the only write surface)

| Endpoint | Purpose |
|---|---|
| `GET /api/sync/state` | Per-source cursor (`last_success_at`, last window fetched) so runs are incremental and self-healing |
| `POST /api/sync/check` | Body: list of `(source, external_id)` (+ DOIs/PMIDs for cross-source dedupe) → returns the subset that's new. **This is what saves Claude tokens** — only new items get processed |
| `POST /api/sync/items` | Batch upsert of finished items (classification + summary + provenance + prompt/model version) as `status='pending'`. Idempotent on `(source, external_id)` |
| `POST /api/sync/trials` | Batch refresh of trial **facts** into `trials_cache` (status, phase, conditions, sites). Separate from `/items` because facts obey the opposite rule: they refresh on every run regardless of the review freeze, since a closed trial shown as "Recruiting" sends a patient to a door that no longer opens. Carries no plain-language text — that stays on `aggregated_items` where the safety checks and the review queue can reach it |

Security: single long random API key in a header, HTTPS only, rate-limited, endpoints return 401 without it; key lives in `dotnet user-secrets` locally / App Service config in prod, rotated by changing one setting. (A personal-project-appropriate design; upgradeable to HMAC-signed requests later if ever needed.)

## 5. The Claude Code step (local)

- Invocation: `claude -p --output-format json` with a versioned prompt template; input is the item's title + abstract/record **only** (source-only rule). Output is parsed, schema-validated, retried once on malformed JSON, then post-checked locally (every numeral in the summary must appear in the source; banned-phrase scan). Failures → item uploaded **unsummarized** and flagged, never guessed.
- Two prompt templates: **classify** (closed taxonomy from `taxonomy.yml`, relevance tier, research stage) and **summarize** (the fixed template in [content-pipeline.md](content-pipeline.md) §9). Prompt version + model recorded per item for auditability.
- Batching: items processed sequentially per run (10–30/day typical; a week's catch-up ≈ 100–200 — still fine for an overnight scheduled run).
- Golden-set regression: `tests/` fixtures run the classify/summarize prompts against ~30 hand-verified items; run manually (or in CI on a self-hosted runner later) whenever a prompt changes.

## 6. Scheduling (local)

- **Windows Task Scheduler**, daily, with "run task as soon as possible after a scheduled start is missed" — so an off/asleep PC just catches up next boot. The run: fetch → check → process → upload → desktop notification "N items awaiting review."
- **Review:** admin queue on the site (any device): approve / edit / reject; approve publishes. Aim for a 5-minute daily habit.
- **Weekly digest run:** assembles the week's best *approved* items into a draft issue, uploads it as a pending digest for review in admin; sending goes through the ESP's API on approval. (ESP: Buttondown/Kit — deliverability and CAN-SPAM are their problem.)
- **Monthly run:** outbound link check + PubMed retraction check for summarized PMIDs → flags into the admin queue.

## 7. What the server no longer needs

- ❌ Hangfire / background jobs — gone entirely.
- ❌ Anthropic API key / billing — gone.
- ❌ Always On as a *requirement* — the site works without it; keep it enabled anyway on B1 (no extra cost) so visitors never hit a cold start.
- Trial "near me" remains a live, keyless server-side query to ClinicalTrials.gov v2 at request time (browser geolocation / ZIP → ZCTA centroid → `filter.geo`).

## 8. Cross-cutting ops

| Concern | Choice |
|---|---|
| Local dev | .NET 10 SDK ✓, Docker Postgres 16 on **5433** (compose file, named volume), `dotnet user-secrets` (NCBI key, sync API key), DbUp runs migrations on Web start in dev |
| Repo | **Private GitHub** (`badsonstudios`); GitHub Actions: build + tests from commit one; deploy step added at M4 |
| CI/CD (from M4) | Actions → App Service; DbUp migration step; content gates (readability, axe smoke) |
| Monitoring | App Insights + free uptime ping on `/` and `/get-help-now`; admin source-health page driven by `source_sync_state` (shows "PubMed last synced N days ago" — visible staleness) |
| Analytics | **No Google Analytics** — privacy-first counter (GoatCounter/Plausible). "We don't track you" is a feature for this audience |
| Caching | Feed pages: 5–15 min response cache; item permalinks: cache hard, bust on edit; static pages: cache until deploy |
| Error pages | Custom 404/500, calm plain language, helpline still visible |
| Old-device budget | htmx (~14KB gz) only script; ES2019 if anything custom; test on cheap Android + throttled 3G |

## 9. Cost

**$0 until M4** (everything local). From launch:

| Item | Monthly |
|---|---|
| App Service B1 (Always On) | ~$13 |
| PostgreSQL Flexible B1ms | ~$12–16 |
| Claude usage | **$0** (existing Claude Code subscription) |
| ESP (Buttondown/Kit free tier to start) | $0 → ~$9 as list grows |
| Domain (brainharbor.org), App Insights, uptime ping | ~$1–3 amortized |
| **Total** | **~$26–32/mo** |

Phase 3 (stories) adds Blob storage + a few dollars. No topology change — story submissions post to the site directly and moderate through the same admin.
