---
name: autopilot
description: Autonomous milestone runner — drive consecutive backlog work items end-to-end WITHOUT per-item approval gates. Plans, implements, tests, reviews, and commits each item to a single milestone branch with a draft PR that Dan reviews asynchronously. Stops only for [user] items, genuine blockers, or the milestone boundary. Use /next-item instead when Dan is at the keyboard and wants the gates.
user-invocable: true
---

Run a whole milestone (or item range) unattended.

**Argument (optional):** a milestone (`M0`, `M1`, …), an item range (`WI-002..WI-010`),
or extra notes — `$ARGUMENTS`. No argument means **the current phase in
`PROGRESS.md`, from the next pending item to the end of that milestone**.

## Authority & boundaries

Dan invoked `/autopilot` **specifically to run without interference** — so,
*within this skill only*, the two `/next-item` approval gates are replaced by
the self-checks below. Everything else about the work loop is unchanged.

Hard boundaries that still apply, always:

- **Never merge to `main`.** All work lands on one milestone branch behind a
  draft PR. Dan reviews and merges when he's back. `main` stays always-working.
- **Never commit red.** Tests green before every commit — no exceptions.
- **Never touch `.claude/.env`** or put secrets anywhere git-tracked.
- **The repo is PUBLIC.** Before every commit, scan the staged diff for
  personal PII (real names, personal emails/phones — not org contacts) and
  secrets, per `commit-push-pr` SKILL §"Privacy & secrets scan". Contact
  addresses in code must be role/domain addresses, never a personal inbox. A
  hit blocks the commit until fixed.
- Medical-content rules (`docs/content-pipeline.md`) are hard requirements.
- Nothing outward-facing beyond the repo: no deploys, no purchases, no emails,
  no DNS. Those are `[user]` territory even if an item implies them.

## Setup (once per run)

1. Run `/startup` context load if not already loaded this session; read
   `PROGRESS.md` and `docs/backlog.md`.
2. If the target phase isn't decomposed into work items yet, run
   `/pm decompose <phase>` yourself, then continue.
3. Branch: `git checkout -b auto/<milestone>` from up-to-date `main` (or switch
   to it if resuming a prior run).
4. After the first commit, push and open a **draft PR** titled
   `Autopilot: <milestone>` — its description is the live run log; append a
   one-line summary per completed item.
5. Note in `PROGRESS.md` that an autopilot run started (milestone, branch,
   timestamp).

## Per-item loop

Follow `/next-item` Steps 1–10 with these substitutions:

- **Gate 1 (plan approval) → self-check.** Validate the plan against the item's
  acceptance criteria and the referenced design-doc sections. Proceed when they
  agree. Do **not** proceed when the item is ambiguous, under-specified, or
  contradicts the design docs — and don't guess: log it as skipped in
  `PROGRESS.md` (with the specific question Dan needs to answer) and move on,
  unless later items depend on it, in which case stop the run.
- **`[user]` items:** skip, log, continue — unless they gate the remaining
  items, in which case stop the run.
- **Gate 2 (commit approval) → commit to the milestone branch.** Message
  `WI-<n>: <title>`. Push, update the draft PR description, check the backlog
  box, update `PROGRESS.md` (done + one-line outcome). No per-item branches or
  PRs in autopilot mode.
- **Test/review loop:** unchanged (iterate until green + no Blocker/Should-fix
  findings, ~3 rounds). If not converging: revert/stash the broken attempt so
  the branch stays green, record the blocker in `PROGRESS.md`, and move to the
  next item that doesn't depend on it — or stop if everything does.
- **PROGRESS.md discipline is the resume mechanism.** Update at item start,
  finish, and on any blocker — if this session dies mid-run, a fresh
  `/autopilot` must resume from the file alone.

## Stop conditions (end the run and report)

- Milestone/range complete.
- A blocked or skipped item that the remaining items depend on.
- An action needed that crosses the hard boundaries above.
- Environment breakage (repeated unrelated build/test failures, Docker/DB
  down) that a debugger-agent pass can't resolve.

## Final report

When the run ends (complete or stopped), report:

1. Items shipped — one line each.
2. Items skipped/blocked — why, and the exact question or action Dan owes.
3. The draft PR link, and test status on the branch tip.
4. Recommended next actions (review + merge PR, answer the open questions,
   `[user]` items to do, then `/autopilot <next>`).

## Notes

- Subagents (Plan, code-reviewer, debugger) inherit the session model — running
  this under Fable means Fable orchestrates and Fable reviews. That's intended.
- This skill is the unattended sibling of `/next-item`: same spec, same
  quality bar, different approval model. If Dan is present and wants gates,
  use `/next-item`.
