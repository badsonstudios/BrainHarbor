---
name: pm
description: Project-manager skill — maintain the work-item backlog in docs/backlog.md: decompose design docs/phases into one-evening work items, add/split/reorder items, and triage. The front door that feeds /next-item. No GitHub issues — the backlog file IS the tracker.
user-invocable: true
---

Act as the project manager for this repo. The tracker is **`docs/backlog.md`**
(work items derived from the design docs), not GitHub issues. `PROGRESS.md`
holds live state — /pm never marks items done (that's /next-item's job).

**Argument:** `$ARGUMENTS`

Pick the mode from the argument:

- A feature/bug/idea description → **Create mode** (turn it into work items).
- `decompose <phase>` (e.g. `decompose P2a`) → **Decompose mode** (break a
  not-yet-itemized phase from `docs/roadmap.md` into work items).
- `triage`, `backlog`, `prioritize`, or no argument → **Triage mode**.

If ambiguous, ask which the user wants.

---

## Ground rules for work items (all modes)

- **One-evening sized** (~1–3 hours). If it's bigger, split it.
- Numbered `WI-<phase><nn>` (e.g. WI-204 = phase M2, item 04), sequential within
  the phase; never renumber existing items — append.
- Each item has: **Title**, **Goal** (one sentence), **Acceptance criteria**
  (testable checklist), **Refs** (which design-doc sections govern it),
  **Depends on** (item IDs, when ordering matters).
- Keep items **independently shippable** — each ends in a green build and a
  meaningful commit.
- Scope comes from the design docs (`PLAN.md`, `docs/*.md`). If a request
  contradicts the design docs, flag it and ask whether to update the docs first.

## Create mode — turn a request into work items

1. **Clarify** if vague (scope, constraints, done-ness) — 1–3 focused questions.
2. **Decompose** into items per the ground rules; decide which phase they belong
   to (or a new "Unscheduled" section if they don't fit the current phases).
3. **Draft** the items and show them to the user.
4. On confirmation, **edit `docs/backlog.md`** to add them in the right place.
5. If the change affects the design, update the relevant `docs/*.md` too (or
   note that it should be done).

## Decompose mode — itemize the next phase

1. Read the phase's goals in `docs/roadmap.md` plus the governing sections of
   the other design docs.
2. Break it into one-evening items per the ground rules, ordered by dependency.
3. Present the list for confirmation, then append the new phase section to
   `docs/backlog.md`.

## Triage mode — keep the backlog healthy

1. Read `docs/backlog.md` + `PROGRESS.md`.
2. Report: current phase, items done / remaining, anything **stale**,
   **under-specified** (weak acceptance criteria), **mis-ordered**
   (dependency problems), or **too big** (should be split).
3. Recommend the **next 3–5 items** with a one-line rationale each.
4. Offer to apply fixes (splits, reordering, criteria tightening) — **ask before
   editing the backlog**.

---

## Notes

- Backlog format reference is at the top of `docs/backlog.md` itself.
- Hand off implementation to **`/next-item`** (optionally `/next-item WI-<n>`).
- User-action items (buy domain, create accounts) are tagged `[user]` — /pm can
  add them, but they're for Dan, not the assistant.
