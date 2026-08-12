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

## Step 1b: Privacy & secrets scan (the repo is PUBLIC)

`badsonstudios/BrainHarbor` is a **public** repo — everything committed is
world-readable, and history is forever. Before staging, scan the diff (and,
for a new file, the file) for anything that must not be public. Treat a hit as
a blocker: fix it, don't commit around it.

Scan only **added** lines (`^\+`), so deletions of old PII and the scanner's
own keywords don't trip it:

```bash
# Personal PII — real names, personal emails/phones, home locations, usernames
# tied to a person. (Org contacts like the ABTA CareLine are fine; GitHub
# noreply commit addresses are fine.)
git diff --cached | grep -E '^\+' \
  | grep -iE 'dheinz|dan[ _-]?heinz|[a-z0-9._%+-]+@(gmail|outlook|yahoo|hotmail|icloud)\.com' \
  && echo "REVIEW: possible personal PII being ADDED"

# Secrets — keys, tokens, passwords, connection strings with real creds.
git diff --cached | grep -E '^\+' \
  | grep -iE '(api[_-]?key|secret|token|password|pwd|bearer|connectionstring)\s*[:=]\s*["'"'"']?[A-Za-z0-9/_+.-]{12,}' \
  | grep -ivE 'brainharbor_dev|placeholder|example|<secret>|replace-with|user-secrets|getsection|configuration\[' \
  && echo "REVIEW: possible secret being ADDED"
```

Rules of thumb:
- Real secrets live in `.claude/.env` (git-ignored) and `dotnet user-secrets`
  (outside the repo) — never in tracked files. `brainharbor_dev` is the
  intentional throwaway local/CI Postgres password and is safe to be public.
- Contact addresses in code must be **role/domain** addresses
  (`contact@brainharbor.org`), never a personal inbox.
- If the diff would expose PII or a secret, fix the source and re-scan before
  continuing. If something is already in a **past** commit (public history),
  stop and tell Dan — history rewrites are his call.

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
.claude/scripts/new-pr.sh -t "WI-<n>: <title>" -b "<body>" -B develop
```
```powershell
# PowerShell
.\.claude\scripts\new-pr.ps1 -Title "WI-<n>: <title>" -Body "<body>" -Base main
```

Or by hand:

```bash
git push -u origin <branch>
gh pr create --base develop --fill
```

Report the PR URL. Dan self-merges (prefer squash). After merge:
`git checkout develop && git pull` before the next item.

## Notes

- Never commit `.claude/.env` or other secrets. Verify nothing sensitive is staged.
- No release/installer step in this project (that's a ClaudeMon thing). From M4,
  merging to `main` triggers the Azure deploy — never merge red.
