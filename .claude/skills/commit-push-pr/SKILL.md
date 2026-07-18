---
name: commit-push-pr
description: Commit the current changes, push to GitHub, and open a pull request. Always asks for explicit approval before committing or pushing. Verifies PROGRESS.md and the backlog are updated when the work belongs to a work item.
user-invocable: true
---

Commit, push, and open a PR for the current work.

If the user provided a summary or PR title: $ARGUMENTS

## Step 1: Review what will be committed

```bash
git status
git diff
```

Summarize the changes for the user. If this work implements a backlog item,
confirm **`PROGRESS.md` and `docs/backlog.md` reflect it** (the /next-item
close-out) — those updates belong in the same commit.

## Step 2: Get explicit approval

**CRITICAL: Always ask the user for approval before committing or pushing.**
Present the plan (files, branch, commit message, PR base) and wait for an
explicit "yes" — unless the user already told you in this session to
commit/push without asking again.

## Step 3: Branch (if needed)

If on `main`, create a branch first: `git checkout -b feature/wi-<n>-<slug>`
(or `fix/<slug>` for non-item work).

## Step 4: Commit

- Stage the intended files (`git add ...`).
- Clear, present-tense message; prefix with the item: `WI-<n>: <what changed>`.
- Follow `references/git-workflow.md`.

## Step 5: Push and open the PR

After approval, prefer the helper script (branches if needed, commits staged
changes, pushes, opens the PR):

```bash
# bash
.claude/scripts/new-pr.sh -t "WI-<n>: <title>" -b "<body>" -B main
```
```powershell
# PowerShell
.\.claude\scripts\new-pr.ps1 -Title "WI-<n>: <title>" -Body "<body>" -Base main
```

Or by hand:

```bash
git push -u origin <branch>
gh pr create --base main --fill
```

Report the PR URL. Dan self-merges (prefer squash). After merge:
`git checkout main && git pull` before the next item.

## Notes

- Never commit `.claude/.env` or other secrets. Verify nothing sensitive is staged.
- No release/installer step in this project (that's a ClaudeMon thing). From M4,
  merging to `main` triggers the Azure deploy — never merge red.
