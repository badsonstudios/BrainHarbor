# Git Workflow — BrainHarbor

- **Host:** private GitHub repo under `badsonstudios` (via `gh` CLI, already
  authenticated). **No GitHub Issues** — `docs/backlog.md` is the tracker and
  `PROGRESS.md` the live state.
- **`main`** is always-working (builds green, tests pass).
- **One branch per work item:** `feature/wi-<n>-<short-slug>` (e.g.
  `feature/wi-204-pubmed-fetcher`). Fix branches: `fix/<slug>`.
- **PR per item**, title `WI-<n>: <title>`; body summarizes changes, test
  status, review outcome. Dan self-merges. Prefer squash-merge so `main` reads
  one commit per work item.
- **Commit messages:** present tense, descriptive, reference the item
  (`WI-204: add PubMed fetcher with date-window catch-up`).
- **Never commit:** `.claude/.env`, secrets, `work_files/` (hook + .gitignore
  both enforce the .env part).
- **Approval:** commits/pushes/PRs only after Gate 2 approval in `/next-item`
  (or explicit user request). Use `.claude/scripts/new-pr.sh` for the
  branch-commit-push-PR sequence.
- After merge: `git checkout main && git pull` before starting the next item.
- From M4, deploys hang off `main` via GitHub Actions — never merge red.
