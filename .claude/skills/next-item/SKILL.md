---
name: next-item
description: End-to-end orchestrator for a backlog work item — pick the next pending item from docs/backlog.md (or a named WI-###), plan, get the plan approved, implement, test until green, code review, iterate, then commit/push/PR after approval. Updates PROGRESS.md at start and finish. This replaces ClaudeMon's implement-issue for a project with no GitHub issues.
user-invocable: true
---

Drive one work item from backlog to merged PR.

**Argument (optional):** a work-item ID (`WI-204`) and/or extra notes — `$ARGUMENTS`.
No argument means **"do the next item"**. "Next phase" means: work items from the
next phase exist in the backlog — confirm the first one; if the phase isn't
decomposed yet, run `/pm decompose <phase>` first.

This skill **orchestrates** other skills and agents. It has **two mandatory
human approval gates** — plan approval (Step 3) and commit approval (Step 9) —
that are **never** skipped, even if the user said "go ahead" earlier for a
different step.

---

## Step 1 — Pick up the item

1. Read **`PROGRESS.md`** first:
   - If an item is already **in progress**, resume it — tell the user where it
     stands (per the progress notes) and continue from the right step below.
   - Otherwise take the argument's WI-###, or the first unchecked, unblocked
     item in the current phase of **`docs/backlog.md`**.
2. Read the item's **Goal / Acceptance criteria / Refs** and the referenced
   design-doc sections. The design docs are the spec — don't improvise scope.
3. Restate the goal and acceptance criteria in your own words, and **update
   `PROGRESS.md` now**: set the item to *in progress* with a timestamp.
4. If the item is ambiguous, under-specified, or contradicts the design docs,
   ask before planning — don't guess. If it's a `[user]` item, hand it to Dan.

## Step 2 — Create a plan

For a non-trivial item, delegate to the **Plan** agent; otherwise plan inline.
The plan must be concrete:

- Files/projects to change (and why) — respect the layout in
  `docs/architecture.md` §3.
- The approach and any trade-offs (refs: `docs/architecture.md`,
  `references/architecture.md`).
- Tests to add/update (see `references/testing.md`).
- Risks, edge cases, and anything explicitly **out of scope**.

Keep scope tight to the item — no unrelated work folded in.

## Step 3 — Approval gate #1 (plan)

**CRITICAL:** Present the plan and **wait for explicit approval**. No
implementation code before approval. If changes are requested, revise and
re-present.

## Step 4 — Implement

- If on `main`, branch first: `git checkout -b feature/wi-<n>-<short-slug>`.
- Implement exactly to the approved plan. Follow `references/code-style.md`.
- Content-touching work must respect `docs/content-pipeline.md` (reading level,
  disclaimers, anti-hype rules) — they're requirements, not suggestions.

## Step 5 — Test (iterate until green)

Build and test per `references/testing.md` (`dotnet build`, `dotnet test`; run
the affected app to see it actually work when there's a runtime surface). On
failure: diagnose (use the **debugger** agent for non-obvious causes) → fix →
re-run. Loop until green. If genuinely blocked, **record the blocker in
`PROGRESS.md`** and stop with the failing output — never report half-working
code as done.

## Step 6 — Code review

Run **`/review`** on the diff. Triage findings into **Blocker / Should-fix / Nit**.

## Step 7 — Iterate

Address Blockers/Should-fixes, then back to Step 5 and Step 6. Repeat until
green + no remaining Blocker/Should-fix (Nits may be noted). Cap ~3 rounds; if
not converging, record state in `PROGRESS.md` and report.

## Step 8 — Update documentation

- **Acceptance criteria check:** walk the item's checklist explicitly — every
  box either done or explained.
- If behavior/design changed vs the design docs, update the affected
  `docs/*.md` (and `README.md` once it exists) **before** committing.
- If nothing doc-worthy changed, say "no doc changes needed".

## Step 9 — Approval gate #2 (commit)

Summarize: what changed, test status, review outcome, files touched, acceptance
criteria status, docs updated (or why none). **Wait for explicit approval to
commit and open the PR.**

## Step 10 — Commit, push, PR, close out

1. Run **`/commit-push-pr`** — branch + PR, title `WI-<n>: <title>`. Dan
   self-merges.
2. **Close out the tracking (never skip):**
   - Check the item off in `docs/backlog.md`.
   - Update `PROGRESS.md`: mark the item **done** with date + one-line outcome
     (+ PR link), set **Next up** to the following item, clear stale notes.
3. Report: what shipped, the PR URL, and what's next.

---

## Notes

- The two approval gates are non-negotiable.
- **PROGRESS.md is the session-survival mechanism** — update it at pickup
  (Step 1), on any blocker (Step 5/7), and at close-out (Step 10), so a fresh
  session (or a cleared context) can resume from the file alone.
- Never commit `.env` or secrets (a hook also blocks staging `.env`).
- This skill is the back end of **`/pm`** — `/pm` shapes the backlog,
  `/next-item` ships it.
