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

## Stopping / resetting

- Stop Web/Pipeline with `Ctrl+C` in their terminals.
- Stop the DB: `docker compose down` (keeps data) or
  `docker compose down -v` (wipes the data volume for a clean slate).

## Flipping back to human-review mode (optional)

The site publishes automatically and never claims a person reviewed a summary.
If you ever want a person to approve everything first, set
`Publishing:Mode` to `Review` in `src/BrainHarbor.Web/appsettings.json` and
approve items in `/admin/queue`. (Leaving it `Auto` is the current design.)
