---
name: startup
description: Initialize the session — load project context from .claude/CLAUDE.md and the startup references, read PROGRESS.md to see exactly where work left off, check the .env/secrets setup, and verify the environment. Run at the start of every session.
user-invocable: true
---

Initialize the development session for **BrainHarbor**.

## Step 1: Load project context

Read, in order:

1. `.claude/CLAUDE.md` — high-level project context and index (required).
2. **`PROGRESS.md`** (repo root) — the live state: current/in-progress item,
   next up, blockers, recent log. **This is how we resume across sessions.**
3. `docs/backlog.md` — skim the current phase's items.
4. The relevant files in `.claude/skills/startup/references/` —
   `project-info.md`, `tech-stack.md`, `architecture.md`, `git-workflow.md`,
   `code-style.md`, `testing.md`, `security.md`, `api-keys-config.md`.
5. `README.md` and the solution/csproj files (once they exist).

## Step 2: Check the secrets setup

- Confirm `.claude/.env.example` exists; note which variables the project
  expects (see `references/api-keys-config.md`).
- Confirm `.claude/.env` exists. If not, tell the user to copy
  `.claude/.env.example` to `.claude/.env` and fill it in. **Never print the
  contents of `.claude/.env`.**
- Confirm `.claude/.env` and `.claude/work_files/` are git-ignored.

## Step 3: Check the environment

```bash
git status --short 2>/dev/null || echo "No git repo yet (created at WI-002)"
git branch --show-current 2>/dev/null
git log --oneline -5 2>/dev/null
git remote -v 2>/dev/null
docker ps --format '{{.Names}}: {{.Ports}}' 2>/dev/null | grep -i postgres || echo "Local Postgres container not running (docker compose up -d)"
```

Only flag what's relevant to the current phase (e.g. no Docker check needed
while still in pre-scaffold planning; no Azure checks before M4).

## Step 4: Report

```
## Session Initialized — BrainHarbor

**Phase**: <current phase from PROGRESS.md>
**In progress**: <item + state, or "nothing mid-flight">
**Next up**: <next item id + title>
**Branch**: <branch or "no repo yet">
**Uncommitted changes**: <count or "none">
**Secrets**: <.env present / MISSING>
**Local DB**: <running / not running / n.a. this phase>

### Recent log (from PROGRESS.md)
<last few log lines>

### Ready to go
<anything needing attention; usually: "Say 'next item' (or /next-item) to start <WI-###>">
```

Keep it short — this is orientation, not a report card.
