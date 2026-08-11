# Testing — BrainHarbor

## Commands

```bash
dotnet build                 # must be clean before any commit
dotnet test                  # xUnit, tests/ — must be green before Gate 2
docker compose up -d         # local Postgres 16 @ 5433 (needed for DB tests)
```

## The test database (WI-411)

DB tests run against **`brainharbor_test`** — a separate database in the same
Postgres server (local container and CI alike), so test seeds never mix with
the rows the pipeline puts in the dev `brainharbor` database. It is created
and migrated automatically on first run (`DatabaseFixture` → MigrationRunner's
EnsureDatabase); there is no setup step. `BRAINHARBOR_TEST_DB` overrides the
full connection string, but the fixture refuses any target that is not local
or obviously a test database. Even so, tests must follow the **dirty-database
rule** documented on `DatabaseFixture` (never assume the tables hold only your
own rows).

Run the affected app to verify behavior for anything with a runtime surface —
tests passing is necessary, not sufficient:

```bash
dotnet run --project src/BrainHarbor.Web        # https://localhost:xxxx
dotnet run --project src/BrainHarbor.Pipeline -- --once   # single pipeline run
```

## What gets tested where

- **Unit tests:** services, classifiers' hard rules, numeral post-check,
  glossary/tooltip Markdig extension, front-matter parsing.
- **Golden-set tests (prompts):** ~30 hand-verified fixtures (abstract → ideal
  classification/summary) in `tests/`. **Any prompt-template change re-runs
  the golden set**; regressions block the change. These call the real
  `claude` CLI — mark them as an explicit category (not run in cloud CI).
- **Sync API integration tests:** state/check/items against the local Postgres
  (idempotency: uploading the same batch twice must be a no-op).
- **Content gates (CI):** readability (FK ≤ 8.5) + front-matter validation via
  `tools/BrainHarbor.ContentCheck`; axe-core smoke on key pages (Playwright)
  once pages exist.

## Rules

- New behavior ships with tests in the same work item (acceptance criteria
  usually say so explicitly).
- Never weaken or delete a failing test to get green — fix the cause, or stop
  and report (record it in `PROGRESS.md`).
- DB tests must clean up after themselves (or use a per-test schema).
