# BrainHarbor — Project Context

> **Read this at the start of EVERY session.** Run the `/startup` skill to load
> this file, the references in `skills/startup/references/`, check the
> environment, and — most importantly — read **`PROGRESS.md`** to see exactly
> where work left off. The root `CLAUDE.md` imports this file so it auto-loads.

---

## Project Overview

**BrainHarbor** (brainharbor.org) is a plain-language brain-tumor research hub
for patients and caregivers. Core product: a daily-updated feed of research /
news / trials with **AI-generated plain-language summaries**. By default the
site runs in **Auto publish mode** (WI-212): a summary that passes the
automated safety checks publishes itself, and only flagged items wait for a
person. A **Review mode** makes human approval mandatory again. Two
applications:

- **`BrainHarbor.Web`** — ASP.NET Core Razor Pages + htmx website (feed, item
  pages, trials, digest, admin review queue, secured sync API). Runs on Azure
  from M4; local + Docker Postgres until then.
- **`BrainHarbor.Pipeline`** — stateless console app on Dan's PC (daily
  scheduled task) that fetches sources, runs **Claude Code CLI** for
  classification + summaries, and uploads results as *pending* via the sync API.
  **No Anthropic API key** — the local `claude` CLI does the LLM work.

The audience may be cognitively impaired (tumor/seizures/treatment): reading
level ≤ 8th grade, WCAG AA, and anti-hype framing are **hard requirements**,
not preferences. The anti-hype guardrails (numeral post-check, banned-phrase
scan, reading-level check, stage badges, the means/doesn't-mean block) are
**automated**, so they hold whether or not a person reviews an item — which is
what makes the default Auto publish mode safe. Human review is optional (a
mode), not mandatory. See `docs/` for the full design.

**Design docs (the source of truth for what to build):**

| Doc | Contents |
|---|---|
| `PLAN.md` | Master plan — decisions, scope, phasing |
| `docs/architecture.md` | Two-app topology, sync API, stack rationale, ops, cost |
| `docs/sitemap.md` | IA, URLs, page inventory |
| `docs/content-pipeline.md` | Editorial rules + the summarization pipeline & guardrails |
| `docs/data-model.md` | Postgres schema, PII rules |
| `docs/roadmap.md` | Milestones M0–M4, later phases, metrics |
| `docs/backlog.md` | **The work-item backlog** — phases broken into one-evening items |
| `PROGRESS.md` (root) | **Live state** — current/next item, log. Always current |

Topic references live in `skills/startup/references/` (project-info, tech-stack,
architecture, git-workflow, code-style, testing, security, api-keys-config).
Keep those current; this CLAUDE.md is the high-level index.

---

## The Work Loop (no GitHub issues)

This project is **not** issue-driven. Work comes from `docs/backlog.md`, which
decomposes the design docs into phases and small work items (WI-###). The flow:

1. Dan says **"do the next item"** (or `/next-item`, or `/next-item WI-204`).
2. The skill reads `PROGRESS.md` → confirms the item → plans → **Gate 1: plan
   approval** → implements → tests until green → `/review` → iterates →
   **Gate 2: commit approval** → `/commit-push-pr` (branch + PR) → updates
   `PROGRESS.md` + the backlog checkbox.
3. `/pm` manages the backlog itself (add/split/reorder items, decompose the next
   phase) — it edits `docs/backlog.md`, it does **not** create GitHub issues.

**PROGRESS.md discipline (critical):** update it the moment an item starts
(mark in-progress) and the moment it finishes (mark done, one-line outcome) —
and when anything notable happens between (blocker found, scope change,
half-done state at session end). A fresh session must be able to read
PROGRESS.md and know *exactly* where things stand without asking.

---

## Environment & Shell

- **OS:** Windows 11, native (WSL exists but is not used).
- **Shell preference: bash first** (Git Bash) for scripts/commands; PowerShell
  only when bash genuinely can't do the job.
- Utility scripts ship in both `.sh` and `.ps1`; prefer the `.sh` version.
- **.NET 10 SDK**, Docker Desktop (Postgres 16 dev container on port **5433** —
  a *different* container/port than other projects' Postgres), `gh` CLI
  (authenticated as `badsonstudios`).

## Secrets & the `.env` file

All tokens, API keys, and passwords live in **`.claude/.env`**.

- **`.claude/.env` is NEVER committed** — it's git-ignored, and a PreToolUse
  hook (`.claude/hooks/block-env-staging.sh`) blocks `git add` of `.env` files.
  Never paste its contents into commits, code, logs, or chat.
- **`.claude/.env.example` IS committed** — placeholder values only. New secret →
  add a placeholder line there and tell Dan to fill in the real value.
- App-level secrets (connection strings, sync API key, NCBI key) also go into
  `dotnet user-secrets` for the running apps; `.claude/.env` is for tooling.
  See `skills/startup/references/api-keys-config.md`.

---

## Source Control — GitHub

- **Host:** private GitHub repo at `badsonstudios/BrainHarbor` (created at
  WI-002). No GitHub Issues — the backlog file is the tracker.
- **Branches:** `main` is always-working; one `feature/wi-<n>-<slug>` branch per
  work item; PR → self-merge. Commit/push only when Dan approves (Gate 2).
- Details: `skills/startup/references/git-workflow.md`.

---

## Working / Temporary Files

- Scratch scripts, downloads, throwaway files → `.claude/work_files/` (git-ignored).
- Never scatter temp files in the project root.

---

## Skills & Agents

Run skills with `/<name>`; agents are delegated to automatically.

| Skill | Purpose |
|-------|---------|
| `/startup` | Load context + read PROGRESS.md + verify environment (every session) |
| `/pm` | Backlog manager — decompose design docs/phases into work items, triage `docs/backlog.md` |
| `/next-item` | **Orchestrator** — pick up the next (or a named) work item → plan → **approve** → implement → test → review → **approve** → PR → update PROGRESS.md |
| `/autopilot` | **Unattended orchestrator** — run a whole milestone item-by-item with the gates replaced by self-checks; single `auto/<milestone>` branch + draft PR, never merges to `main`. For hands-off (e.g. Fable) runs |
| `/check-code` | Code-quality analysis of changed files |
| `/review` | Deeper architecture / correctness review (code-reviewer agent) |
| `/commit-push-pr` | Commit, push, open a PR (asks for approval) |
| `/explain` | Explain code or a concept (read-only) |
| `/deep-research` | Multi-source web research with citations |

**Commands** (`.claude/commands/`): `/commit` (stage + commit, asks first),
`/pr` (push + open a PR via the `new-pr` script).

| Agent | Purpose |
|-------|---------|
| `code-reviewer` | Read-only architecture & code review |
| `debugger` | Root-cause analysis of errors and failures |
| `deep-research-agent` | Comprehensive multi-source research |

---

## Keeping Skills & Agents Up to Date

The skills and agents are **living tooling** — they start as adapted templates
and must evolve with the codebase. Proactively:

- During `/startup`, flag skills/agents that have drifted from reality.
- After significant changes to stack/structure/commands, update the affected
  skill/agent and the relevant `startup/references/*.md`.
- Capture repeated manual tasks as new skills/scripts instead of redoing them.
- Fix bad or stale guidance at the source, and tell Dan what changed and why.

## Utility Scripts

In `.claude/scripts/` (see `scripts/README.md`); prefer these over re-typing:

| Script | Purpose |
|--------|---------|
| `new-pr` | Branch (if on `main`), commit, push, open a PR via `gh` |
| `load-env` | Load `.env` into the current shell |
| `get-secret` | Read one value from `.env` without printing the file |

## Hooks

Configured in `.claude/settings.json`:

- **block-env-staging (PreToolUse):** blocks `git add` of secrets files
  (`.env` etc.); `.env.example` allowed. Requires Git Bash.
- **build-test-gate (Stop) — opt-in:** builds (and optionally tests) before
  finishing; off by default, enable per `.claude/hooks/README.md`. Override via
  `BUILD_CMD`/`TEST_CMD` in `.claude/.env`.

## Other Claude Code config

- **Status line** (`settings.json` → `statusLine`): `dir | branch | model`.
- **Output styles** (`.claude/output-styles/`): `Concise` sample included.

---

## Project-Specific Notes

- **Build/run/test** (once WI-003 scaffolds the solution):
  - Build: `dotnet build`
  - Run site: `dotnet run --project src/BrainHarbor.Web`
  - Run pipeline: `dotnet run --project src/BrainHarbor.Pipeline`
  - Test: `dotnet test` (xUnit, `tests/`)
  - Local DB: `docker compose up -d` (Postgres 16 @ localhost:5433)
- **DbUp** migrations (plain SQL in the repo) run on Web start in dev.
- **The medical-content rules are non-negotiable:** reading level ≤ 8.5 grade
  (CI-gated for static pages), sources-only summarization, stage badges,
  banned hype phrases, and the automated safety checks that gate auto-publish.
  Human review is a *mode* (default Auto = optional; Review = mandatory), but
  the automated guardrails are not optional — they run in both modes. See
  `docs/content-pipeline.md` §"Publish mode" before touching anything
  content-related.
- **Never** ingest AHFS/MedlinePlus drug monographs or reuse NCI embedded
  images (licensing — see `PLAN.md` §5).
- The Claude Code CLI is invoked by the Pipeline as `claude -p --output-format
  json` — treat prompt templates in `src/BrainHarbor.Pipeline/Prompts/` as
  versioned artifacts (changes re-run the golden set).
