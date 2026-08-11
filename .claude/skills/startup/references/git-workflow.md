# Git Workflow — BrainHarbor

- **Host:** private GitHub repo under `badsonstudios` (via `gh` CLI, already
  authenticated). **No GitHub Issues** — `docs/backlog.md` is the tracker and
  `PROGRESS.md` the live state.
- **Branch model (since 2026-08-11): `develop` → `main`.**
  - **`develop`** (the GitHub default) is the integration branch — every
    feature branch PRs into it.
  - **`main`** is the RELEASE branch: merging a develop → main PR deploys to
    Azure (CI deploy job + smoke check). Only ever merge develop into main,
    and never red.
- **One branch per work item:** `feature/wi-<n>-<short-slug>` (e.g.
  `feature/wi-204-pubmed-fetcher`), branched from and PR'd back to
  `develop`. Fix branches: `fix/<slug>`.
- **PR per item**, title `WI-<n>: <title>`, base `develop`; body summarizes
  changes, test status, review outcome. Dan self-merges. Prefer squash-merge
  so history reads one commit per work item.
- **Releasing:** open a PR `develop` → `main` titled `Release: <summary>`;
  merging it is the deploy.
- **Commit messages:** present tense, descriptive, reference the item
  (`WI-204: add PubMed fetcher with date-window catch-up`).
- **Never commit:** `.claude/.env`, secrets, `work_files/` (hook + .gitignore
  both enforce the .env part).
- **Approval:** commits/pushes/PRs only after Gate 2 approval in `/next-item`
  (or explicit user request). Use `.claude/scripts/new-pr.sh` for the
  branch-commit-push-PR sequence.
- After merge: `git checkout develop && git pull` before starting the next item.
- From M4, deploys hang off `main` via GitHub Actions — never merge red.
