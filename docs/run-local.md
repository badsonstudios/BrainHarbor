# Running BrainHarbor locally (no Azure)

The whole system runs on your PC. Azure (M4) is only for putting it on the
public internet later — nothing needs the cloud to work end-to-end.

There are three pieces:

1. **Postgres** — a Docker container (port **5433**).
2. **BrainHarbor.Web** — the website (feed, item pages, admin queue, sync API).
3. **BrainHarbor.Pipeline** — the console app that fetches sources, runs the
   local `claude` CLI to classify + summarize, and uploads results to the Web.

Config is already set in user-secrets: the DB connection + `SYNC_API_KEY` for
Web, and `SYNC_API_KEY` + `NCBI_API_KEY` + `Pipeline:SyncApiBaseUrl`
(`http://localhost:5268`) for the Pipeline.

## 1. Start the database

```bash
docker compose up -d          # Postgres 16 on localhost:5433
```

## 2. Start the website

```bash
dotnet run --project src/BrainHarbor.Web --launch-profile http
```

- Serves at **http://localhost:5268** (use the `http` profile so there's no
  self-signed-cert hassle — the Pipeline talks to it over http).
- DbUp runs the SQL migrations automatically on startup.
- Leave this terminal running. Open http://localhost:5268 in your browser.

Pages to try: `/` · `/research` · `/how-we-write` · `/search` ·
`/admin/queue` (asks you to sign in — that's the admin login from WI-207).

The **feed will be empty** until the Pipeline has uploaded something (next step).

## 3. Run the pipeline to fill the feed

In a **second terminal** (leave the website running):

```bash
dotnet run --project src/BrainHarbor.Pipeline
```

What happens: it fetches new items from PubMed / NCI / preprints, runs each one
through your local `claude` CLI (classify, then a plain-language summary on
`claude-opus-5`), and uploads the results to the running website.

Because publish mode is **Auto**, a summary that passes the automated safety
checks (numbers trace to the source, no hype words, reading level, readiness
cap) **publishes itself** and shows up on `/research` immediately. Anything a
check flags is held in `/admin/queue`.

Notes:
- The first run can take a while and use a fair number of Claude tokens — it
  processes every new item in the fetch window. Re-runs only touch genuinely
  new items (it asks the site what's new first).
- No Anthropic API key is involved; it uses your local `claude` login.

## Where the run logs go

Every pipeline run writes its own log file (WI-417), because the scheduled
daily run has no console anyone can see:

```
%LOCALAPPDATA%\BrainHarbor\logs\pipeline-<date>-<time>.log
```

The path is printed as the last line of every run, including a failed one. To
read the end of the newest one:

```powershell
Get-ChildItem $env:LOCALAPPDATA\BrainHarbor\logs |
    Sort-Object LastWriteTime | Select-Object -Last 1 | Get-Content -Tail 40
```

What is in there: every source's fetch/upload counts, every item excluded as
off-topic (with its id and title), every summary the safety checks flagged
**and which check flagged it**, and a run summary that totals the flags by
cause. That last part is the answer to "4.8% were flagged — for what?", which
the database cannot give you: it stores a `summary_flagged` boolean and no
reason.

- **One file per run**, named to the second, so a manual run never overwrites
  the 06:00 scheduled one. Readable with `Get-Content` while the run is going.
- **Retention**: pruned at the start of the next run — 30 days, 100 files,
  256 MB across the directory, and 32 MB within any one run. Nothing to prune
  by hand. A file another run is still writing to is never a candidate.
- **Secrets never land in it**: `HttpClient` request URIs are filtered out (the
  NCBI key travels in the query string because E-utilities requires it there),
  and anything key-shaped that reaches a log line is scrubbed on the way to
  disk. Both are pinned by tests in `PipelineLoggingTests`.
- **Limit**: a crash *before* the app starts (a broken publish, a missing
  runtime) writes no file — there the task's exit code is still the only
  signal. Check `Get-ScheduledTaskInfo -TaskName 'BrainHarbor Pipeline'`.

Settings live under `Pipeline:Logging` (`Enabled`, `Directory`,
`RetentionDays`, `MaxFiles`, `MaxFileMegabytes`, `MaxDirectoryMegabytes`) if
you ever need to move or shrink them.

## Stopping / resetting

- Stop Web/Pipeline with `Ctrl+C` in their terminals.
- Stop the DB: `docker compose down` (keeps data) or
  `docker compose down -v` (wipes the data volume for a clean slate).

## Flipping back to human-review mode (optional)

The site publishes automatically and never claims a person reviewed a summary.
If you ever want a person to approve everything first, set
`Publishing:Mode` to `Review` in `src/BrainHarbor.Web/appsettings.json` and
approve items in `/admin/queue`. (Leaving it `Auto` is the current design.)
