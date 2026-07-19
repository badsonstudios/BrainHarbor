# Tech Stack — BrainHarbor

| Area | Choice | Notes |
|---|---|---|
| Runtime | **.NET 10 LTS** (10.0.3xx SDK installed) | Both apps |
| Web | **ASP.NET Core Razor Pages** + **htmx** via `Htmx.Net` (+ `Htmx.TagHelpers`) | htmx ~14KB is the entire JS budget; every interaction has a no-JS fallback. Client htmx **2.0.10** vendored at `wwwroot/js/htmx.min.js` (from unpkg; update manually) |
| Pipeline | .NET console app (`BrainHarbor.Pipeline`) | Stateless; runs on Dan's PC via Task Scheduler |
| LLM | **Claude Code CLI**, headless: `claude -p --output-format json` | NO Anthropic API key. Prompt templates are versioned artifacts in `Pipeline/Prompts/` |
| Data access | **Dapper** | Matches work stack; no EF |
| Migrations | **DbUp** (plain SQL scripts) | Run on Web start in dev; CI step in prod |
| DB | **PostgreSQL 16** — Docker on `localhost:5433` (dev), Azure Flexible Server B1ms (prod from M4) | Port 5433 to avoid the other project's container |
| Markdown | **Markdig** (+ custom glossary-tooltip extension) | Static pages + glossary |
| Auth (admin) | ASP.NET Core Identity, single admin, TOTP 2FA | No public accounts |
| Tests | xUnit in `tests/` | Golden-set fixtures for classify/summarize prompts |
| CI | GitHub Actions: build + tests from commit one; deploy added at M4 | |
| Email (M4) | Hosted ESP (Buttondown/Kit) via API | Deliverability/compliance outsourced |
| Analytics | Privacy-first counter (GoatCounter/Plausible) — **never Google Analytics** | Trust requirement |

External APIs (fetched by the Pipeline; all free): PubMed E-utilities (key in
`.env`, 10 rps), ClinicalTrials.gov v2 (no key), NCI RSS, ScienceDaily RSS,
medRxiv/bioRxiv (metadata only). Licensing verdicts per source: `PLAN.md` §5.

Old-device constraint: public pages must work on cheap Android / throttled 3G;
ES2019 target for any custom JS; test with DevTools throttling.
