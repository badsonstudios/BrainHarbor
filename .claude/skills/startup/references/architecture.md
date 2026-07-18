# Architecture — BrainHarbor

Full detail: `docs/architecture.md` (the authority). Summary:

## Two applications

- **`src/BrainHarbor.Web`** — the website. Public pages (feed, item permalinks,
  trials, digest, static content), a **secured sync API** (`/api/sync/state`,
  `/api/sync/check`, `/api/sync/items` — API-key header auth, idempotent
  upserts), and an **admin area** (Identity + 2FA): review queue, flag queue,
  source health. **No background jobs on the server. No Blazor. No circuits.**
- **`src/BrainHarbor.Pipeline`** — stateless local console app: fetch sources →
  ask sync API what's new → classify + summarize via Claude Code CLI → upload
  as `pending`. One `ISourceFetcher` per source, typed HttpClient + Polly each.

## Data flow / item lifecycle

fetch → `POST /api/sync/check` (dedupe before spending Claude time) → classify
(closed taxonomy from `Content/taxonomy.yml`; `excluded` never uploaded) →
summarize (fixed template, JSON-validated, numeral post-check) → upload
`pending` → **human approves in admin queue** → `published` → public feed.

## Layout conventions

```
src/BrainHarbor.Web/        Pages/ (Razor Pages), Api/, Services/ContentStore/, wwwroot/
src/BrainHarbor.Pipeline/   Sources/, Claude/, Publishing/, Prompts/
tools/BrainHarbor.ContentCheck/   readability + front-matter CI gate
tests/                      xUnit; golden-set fixtures for prompts
docker-compose.yml          Postgres 16 @ 5433
```

## Rules that shape code decisions

- Progressive enhancement: htmx fragments come from Razor partials; every
  htmx interaction needs a working no-JS path.
- The Web app is read-mostly; the ONLY write surfaces are the sync API,
  admin actions, and (M4) digest signup.
- The Pipeline holds no state — if it seems to need local persistence, the
  answer is a sync-API capability instead.
- Curated content = Markdown + YAML front matter in the repo; dynamic content =
  Postgres (`docs/data-model.md` is the schema authority).
- Accessibility (WCAG AA), ≤8th-grade reading level, and anti-hype summary
  rules are architectural constraints, not polish (`docs/content-pipeline.md`).
