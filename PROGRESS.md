# BrainHarbor — Progress

> **The live state of the project.** Read this first in every session (via
> `/startup`). Updated the moment an item starts, finishes, or hits a blocker —
> a fresh session must be able to resume from this file alone.
> The item definitions live in [docs/backlog.md](docs/backlog.md).

## Current state

| | |
|---|---|
| **Phase** | M3 — Claude classification + plain-language summaries (M0–M2 complete & merged) |
| **Phase** | **M3 MERGED to `main`** (PR #5, 2026-07-31). Next: **M4 — Azure + trials + digest → v1 launch.** |
| **In progress** | nothing mid-flight. WI-401, WI-414, WI-415 all done and **released to prod** (PRs #17, #19). **Daily scheduled task registered 2026-08-13** ('BrainHarbor Pipeline', 06:00, runs as Dan, StartWhenAvailable) — the feed now updates itself, and since **WI-417** each run leaves a log behind. |
| **WI-401 record** | **Azure provisioning: SITE IS LIVE** at app-brainharbor-prod-eus2.azurewebsites.net (2026-08-11, shared-infra option A: web app on Moodathon's B1 plan `asp-shamoody-prod-eus2`, `brainharbor` DB + own role on `db-shamoody-prod-eus` PG17, schema owned by `brainharbor`, PUBLIC revoked). **Continuous deploy PROVEN end-to-end** (PR #11 merged 425ec9b): merge to `main` → build+test+ContentCheck → deploy → smoke check, all green live. Gotchas hit & fixed: PowerShell Compress-Archive writes backslash zip entries (Kudu chokes; workflow's ubuntu zip is fine), PG15+ public-schema perms (brainharbor now owns its schema), **SCM basic auth was disabled by default** (enabled for publish-profile deploys; OIDC upgrade deferred). Prod secrets in `.claude/.env` (BRAINHARBOR_PG_PASSWORD, SYNC_API_KEY_PROD, ADMIN_PASSWORD_PROD) + App Service settings. Plan memory 81% with both apps (77% before; escape hatch = B2 +$13/mo). **https://brainharbor.org + www LIVE with managed TLS (2026-08-11)** — Namecheap A/CNAME/asuid-TXTs verified, hostnames bound, SNI certs issued+bound (Dan still to delete Namecheap's conflicting `@` URL-Redirect record). Admin account seeded + 2FA enrolled (address in `.claude/.env` as ADMIN_EMAIL — not written down here: the repo is public and it is half of the admin login). Pipeline points at prod. **BACKFILL DONE for 5 of 6 sources (2026-08-12): 1,038 items published live**, 134 pending (114 classified + 20 one-off classify failures for a human), 106 flagged by the guardrails. Home shows real cards; /research shows 615 by default (early-stage behind the toggle). **Only `ctgov` remains** — it hit the usage limit and the new fail-fast held its cursor empty, so one more `dotnet run --project src/BrainHarbor.Pipeline -- --once` when a limit window is free finishes it. No cleanup needed. |
| **Next up** | **WI-425** (prominent "See all" button under the home feed — Dan's ask 2026-08-14, agreed to do next), then the reader-report work (notes shown in the queue + a count of reports, Dan's call: count reports not people, no identity stored). Then Dan's calls: **WI-404** (digest — needs an ESP account), **WI-408** (soft launch). Assistant-buildable now: **WI-413** (classifier unavailable vs odd item — the last hole in the fail-fast, and the task now runs unattended nightly), **WI-412** (/tumors plain-English descriptions), **WI-418** (store WHY a summary was flagged), **WI-416** (one reading-level grader, not two), **WI-406** (maintenance run), **WI-407** (pre-launch hardening). |
| **Blockers** | none. WI-401, WI-404 (ESP), WI-408 (soft launch) need Dan's hands (accounts, DNS, money). |

**Branch model (since 2026-08-11): feature → `develop` (default branch) → release PR → `main` → auto-deploy to Azure.** Merging develop into main IS the deploy (CI deploy job + smoke check). Never merge main red.

**Publishing mode: AUTO, fully automatic.** Site publishes summaries that pass the automated safety checks; **no human-review claims anywhere in reader-facing copy** (deliberate — scrubbed 2026-07-31). The review queue still exists in code for flagged/reported items but is never promised to readers. Default model claude-opus-5.

**Feed card imagery (done 2026-08-01, on `main`).** Feed cards show a content-matched **photo backdrop** (faded ~20%) with the item's **readiness score as a dial** floating on top; feed is **2-up**. Images are a small human-vetted Unsplash pool in `wwwroot/img/cards/` (grouped brain/genetics/lab/data/abstract); `CardImages` picks by matching the post's words + stage to a theme — **no AI image generation**. Raw originals git-ignored; see `images/image-tags.yml` + `wwwroot/img/cards/IMAGE-CREDITS.md`. Also fixed a real **Windows pipeline bug** (claude .cmd shim needs cmd.exe) and **guardrail false-positives** (cure negation now sentence-scoped; prompt v3 forbids computed numbers) — found running the pipeline live locally.

**Local run:** the whole system runs on the PC (no Azure needed) — see `docs/run-local.md`. Dev DB holds demo items from live pipeline runs. The two `FeedTests` that used to fail locally against that data (UndatedItemsSortLastNotFirst, EarlyStageAppearsOnlyWhenTheReaderAsksForIt) were fixed in WI-402: they now page until they find their own rows instead of assuming an empty table, so the suite is green on a dirty DB and on a fresh one. `A11ySmokeTests` intermittently failed to start its Kestrel host ("The server has not been started"). WI-403 serialized `KestrelWebApplicationFactory.EnsureServer` (CreateClient is not thread-safe) and wrapped the real cause in a message that names it, so a recurrence is diagnosable instead of mute. Not proven fixed — it was never reproducible on demand.

### Open threads (2026-08-13)
- **Daily pipeline is scheduled** ('BrainHarbor Pipeline', 06:00 daily, published
  to `artifacts/pipeline`). Reversible:
  `./scripts/register-pipeline-task.ps1 -Unregister`. Task Scheduler still
  captures no console output, but since WI-417 the run writes its own log to
  `%LOCALAPPDATA%\BrainHarbor\logs` (newest file = last run; path printed at the
  end of every run). Exit code (`Get-ScheduledTaskInfo`) and the admin health
  page are still the quick check; the log is where the per-item detail lives.
- **WI-409 and WI-410 shipped** (home leads with the feed; feed sorting) — both live.
- **Dan's review queue holds 134 items** (106 guardrail-flagged, 20 unclassified
  one-offs). A first pass would show whether Auto mode's bar is right before WI-408.
- **Brand name mismatch is a known, accepted state** (WI-420): the logo says
  "Brain Harbor", the title/og:site_name/RSS/domain say "BrainHarbor". Dan's
  call 2026-08-14 — leave it; not a soft-launch blocker. Don't "fix" it in
  passing.
- Tiny polish backlog: `data` image theme matches 0 items (widen keywords or reassign slot).
- Namecheap still has a conflicting `@` URL-Redirect record — harmless now that
  the A record answers, but worth deleting.

### M3 shipped (all on `auto/M3`, PR #5)
- **WI-301–304** golden set, CLIwrapper, classify, summarize+guardrails (numeral/banned/reading-level); connection-pool infra fix.
- **Readiness score (1–10)** — Dan's ask: how close a finding is to everyday care, stage-capped, shown on item pages + queue.
- **WI-305** review queue v1 (side-by-side, inline edit, keyboard, readiness badge).
- **WI-306** item permalink pages (6 blocks, readiness, glossary tooltips, provenance, report-a-problem → admin queue).
- **WI-307** feed patient-first with persisted early-stage toggle.
- **WI-308** SEO (sitemap/robots/RSS, OG + JSON-LD) + honest `/how-we-write` rewrite (was falsely claiming mandatory human review).
- **WI-309** site search (Postgres FTS over items + static pages).
- Prompt/style: no em dashes or AI tells; summaries validated live with Opus.
- 513 tests green.

**Readiness score (Dan's ask, built 2026-07-30):** every summary now carries a
1-10 "how close is this to something a patient can get?" score + one plain
reason (`summarize-v2`). Two-layer safety: Opus proposes within a rubric, then
`Readiness.Clamp` caps by research stage (animal/cell→2, obs→5, review→6,
trial→8, news→10; only ever lowers), and `SyncRepository` re-clamps at the
API/DB boundary as a backstop. Migration 0005. Live-validated (mouse study→2,
observational→4, honest reasons). Not yet rendered on a page — the badge lands
with WI-306. Scale is documented in `docs/content-pipeline.md` §9.

## Notes for the next session

- **Approved visual design lives at `docs/design/entry-hub-handoff/`** ("Clear
  & Kind" theme + Entry Hub home, from Claude Design 2026-07-19). It is the
  visual spec for WI-108/WI-109 and restyles later feed/item work (WI-209,
  WI-306). M1 order changed: **WI-108 before WI-102** so the axe/Playwright
  smoke test runs against the final theme. Handoff URL names that differ from
  sitemap.md (/get-help, /start-here) do NOT override the sitemap
  (/get-help-now, /start). The handoff folder is not yet committed — it goes
  in with WI-108's branch.
- **Remaining dead links**: only `/digest` (M4, needs an ESP → WI-404).
  `/research` went live in WI-209, `/trials` in WI-403.
  `/get-help-now`, `/digest`, `/glossary`, `/about`, `/how-we-write`,
  `/start`, `/privacy`, `/terms` are all live, and a `ShellPagesTests` link
  check fails the build if any *other* internal link 404s. Custom 404/500
  pages exist (WI-103), so the dead nav targets degrade gracefully.
- **M0 fully closed 2026-07-19**: PR #1 squash-merged to `main` (ce5929d),
  `auto/M0` deleted; brainharbor.org purchased (WI-001); `.env` populated;
  NCBI + SYNC keys in user-secrets. No open follow-ups.
- Planning and design are **done** — `PLAN.md` + `docs/*.md` are the spec,
  `docs/backlog.md` is the itemized plan (M0–M4; P2a–P3 not yet decomposed).
- Solution note: SDK 10 generated `BrainHarbor.slnx` (new XML solution
  format) rather than `.sln`; `dotnet build/test` handle it fine.
- Next: `/next-item` for WI-101, or `/autopilot M1`.

## Log (newest first)

- **2026-08-14** — **WI-426 done — the hype check was flagging summaries for
  DENYING hype.** Dan found it within minutes of WI-418 making the reasons
  visible, which is the best argument for WI-418 there is. The negation
  exemption had been wired to "cure" alone (WI-401); every other banned phrase
  was a bare keyword match. So "this is not a breakthrough" and "this is not a
  game-changer" were flagged AS hype — and the block they live in, "what this
  doesn't mean", exists precisely to write sentences like that. **The guardrail
  was punishing summaries for obeying the anti-hype rule**, holding them out of
  Auto publish and filling the review queue. Verified live before fixing: 5 of
  6 negated samples flagged, only "cure" clean.
  Fixed by applying the existing sentence-scoped negation check to every phrase.
  A real "this IS a breakthrough" is still caught, and a denial in one sentence
  does not license a claim in the next or in the next block.
  **Plus bulk approve in the queue** (Dan asked to "approve all ~137"). It
  approves every pending item that no check flags — exactly what Auto mode
  publishes by itself. It deliberately does NOT approve everything: an item
  flagged for an untraceable number stays for a person (that is the site's
  central factual promise, and where a model may have invented a survival
  figure), and so does an item with no summary (approving it publishes an empty
  page to a patient — the ~20 classify failures are exactly this). The audit row
  records who clicked and that it was a bulk action.
  **Note: I could not do the approving myself** — prod Postgres refuses
  connections from here (firewall admits Azure services only) and the admin UI
  needs Dan's TOTP. The button puts the action behind his login, which is where
  it belongs anyway. 761 tests.

- **2026-08-14** — **WI-418 done — the review queue says WHICH check flagged an
  item** (Dan's ask: he opened the queue, found 137 items marked "read this one
  closely", and could not tell why any of them was there).
  **Solved by re-checking, not by a migration.** The checks are pure text
  analysis and every summary is already stored, so the reason was *recoverable*:
  the queue re-runs them over the stored blocks at render time. That means the
  whole existing backlog is explained the moment this deploys — a
  store-it-going-forward design would have left those 137 exactly as opaque as
  they were.
  `Guardrails` moved out of the Pipeline into a new shared **`BrainHarbor.Safety`**
  project both apps reference. Copying the rules into the site was the obvious
  shortcut and the wrong one: a second implementation of the same rule is
  precisely the defect WI-415 spent a day fixing and WI-416 still exists to
  finish. `SummaryText` now owns the block assembly for the same reason — a
  title has no full stop, and joining it into the hook is what inflated every
  reading grade by ~0.7.
  The re-check **joins `trials_cache`**: the summarize-trial prompt scores
  readiness BY phase, so "Phase 2" legitimately appears in a trial summary and
  without the join every trial would report its own phase as an invented number
  and send Dan chasing a ghost.
  **Two limits stated in the UI rather than hidden:** it reflects TODAY's rules
  (the reading ceiling moved 8.5 → 7.0 on 2026-08-13, so an item flagged then
  may pass now), and a reader-reported item has no automated reason — that case
  now says so out loud instead of rendering an empty box.
  Verified through the repository against the real SQL including the new join,
  plus unit tests over invented numbers, hype, the trial phase, the
  no-summary case, and that the queue assembles blocks identically to the
  pipeline. **Not verified in the browser**: `Admin:Email`/`Admin:Password` were
  never set in this machine's Web user-secrets, so the LOCAL admin area has
  never been reachable here (prod is unaffected). Follow-up **WI-424** filed for
  recording the reason at flag time — the queue answers "what fails today", not
  "what failed that night". 750 tests.

- **2026-08-14** — **WI-422 done — the home page says plainly that AI can be
  wrong** (Dan's ask; reviewed locally and approved). An `.ai-caution` block
  between the hub and "Latest updates": *"AI can make mistakes. AI writes every
  summary here. Our safety checks catch many mistakes, but they miss some.
  Always read the study we link to, and talk with your care team before you act
  on what you read."* The hub already named AI as the writer and said a person
  does not check every one; what was missing was the admission that the writer
  is FALLIBLE, plus what to do about it — an admission with no action is not
  much use. **Above the feed, never below it** — a caution a reader meets after
  the summaries has already failed — and a test pins that ordering along with
  the sentence and both actions. Styled with the palette's existing attention
  treatment (`--color-notice*`, the closed-trial fill), NOT a red alarm: the
  palette's own rule is no reds outside true warnings, and this audience is
  frightened enough. Larger than body text, lead sentence on its own line,
  meaning carried by the words so it survives high-contrast mode, a failed
  stylesheet and print. **Measured grade 4.1 by ContentCheck** — which is a
  real gate, not a courtesy: Razor pages have been failed above 6.0 since
  WI-414. Checked at desktop and 390px. 742 tests.
  **Two stale records fixed while here:** WI-414's backlog checkbox was never
  ticked though it shipped in PR #19, and the note above claiming ContentCheck
  does not gate the home page was wrong from that same date.

- **2026-08-14** — **WI-421 done — the brand owns the top-right of the home
  hero** (Dan's ask, with a screenshot; he reviewed the rendered result and
  approved the size). `.hub` is now a two-column grid: copy left,
  `lockup-no-tagline.svg` in a 24rem column top-right, three doors spanning
  both columns beneath. **The no-tagline lockup deliberately** —
  `lockup-horizontal.svg` carries the tagline "Real brain tumor research, in
  plain language." *as artwork*, which is word for word the h1 next to it, so
  the full lockup would print that sentence twice side by side. `alt=""` +
  `aria-hidden`: a decorative repeat, per the brand README, since the header
  already announces the name. Sized in rem so it grows with large-text mode;
  hidden in print (the masthead already identifies the page). **Collapses to
  one column below 60rem and the logo disappears** — note that media-query
  `rem` is the browser's 16px, NOT the site's 18px base, so that is 960px —
  because the "talk to someone now" door must not be pushed down a phone
  screen for a logo the header shows two inches above. Accepted trade: the h1
  wraps to two lines above the breakpoint. Verified at 1440/1100/900 px and in
  large-text mode. 741 tests, axe clean.
  **Session note:** the tool-approval queue went unavailable mid-item, so this
  sat written-but-unbuilt for a while; PROGRESS carried the exact resume steps
  until it cleared.

- **2026-08-14** — **WI-419 done — the site wears the real logo** (Dan's ask;
  he supplied a finished logo kit). Header lockup (`lockup-no-tagline.svg`,
  `alt="Brain Harbor"` — the kit is explicit that the alt is the NAME, never
  "logo"), favicon SVG + 32px PNG, apple-touch icon, PWA manifest at the web
  root, `theme-color`, and **`og:image`, which the site did not have at all** —
  every shared link was unfurling as a grey box. Absolute URL, because
  `og:image` silently ignores a relative one. Logo height in **rem** so it
  rides the large-text scale (checked in both modes); print sized in points so
  a printed page gets a masthead, not a banner. The kit's teal is byte-identical
  to the existing `--color-accent` (#0d6a86), so nothing needed recoloring.
  **Verified the kit's opacity claim rather than trusting it**: iOS paints alpha
  in an apple-touch-icon BLACK, the files are colour-type RGBA (so the channel
  proves nothing), and a wrong icon is invisible until it reaches a phone home
  screen — so the test decodes the top scanline and asserts every pixel is
  opaque. One existing test broke legitimately:
  `MarkdownLinksAndImagesInSummaryBlocksAreNeutralized` asserted the WHOLE page
  had no `<img>` as a proxy for "injected markdown did not render"; the header
  logo is now a legitimate one, so it is scoped to `<main>` and additionally
  proves the payload survives as inert TEXT (a reviewer has to see what the
  model produced) without ever becoming a `src`. **NOTE: the download folder
  also holds an UPDATED full design bundle** (README/index/research-item/CSS all
  differ from `docs/design/entry-hub-handoff/`) — deliberately NOT applied; it
  is a separate, larger piece of work. Filed **WI-420**: the wordmark says
  "Brain Harbor" and the site title/og:site_name/RSS/domain say "BrainHarbor",
  so a screen reader and a sighted reader get different names on one page —
  Dan's call which wins. 741 tests.

- **2026-08-13** — **WI-413 done — the CLI says WHY it failed, so an outage and
  an odd item stop looking alike** (PR
  [#21](https://github.com/badsonstudios/BrainHarbor/pull/21), merged to
  `develop`). `ClaudeCli` now returns `Unavailable` (never answered: spawn
  failure, timeout, non-zero exit, or stdout that is not its documented
  envelope) vs `UnusableOutput` (answered; answer unusable). **The envelope is
  the CLI's own output — a garbled MODEL answer still arrives inside a
  well-formed one — which is what makes that the right line to draw.**
  `ClassifyDecision.Unavailable` covers a failed taxonomy fetch too (the site
  being unreachable is identical for every item), `SummaryResult` carries the
  same flag, and the runner stops on the FIRST unavailable. **The streak
  counter and the whole `deferred` list are deleted** — net fewer lines.
  **Where the signal is genuinely ambiguous, it now asks instead of guessing:**
  a timeout looks exactly like a dead CLI, and stopping on one would hold the
  cursor so the same slow item leads the window tomorrow, forever — so an
  `Unavailable` verdict triggers one trivial health prompt. Alive means the
  item is merely odd (queue it, advance the cursor); no answer means stop.
  **Review caught three real defects, two of them introduced by this change:**
  envelope failures were tagged `UnusableOutput`, which would have let a CLI
  printing a banner mark a whole window unclassifiable with NO bound (worse
  than the streak it replaced); the timeout stall above, which violated this
  item's own acceptance criteria; and the skip path froze trial facts for a
  day, though facts need no model call. **Live testing caught a fourth:** the
  facts-only pass fetched every source during an outage — four minutes, almost
  all of it on sources with no facts — now gated by
  `ISourceFetcher.ProducesTrialFacts`, eleven seconds. Verified live with a
  nonexistent `claude`: stopped on item 1, cursor held, nothing uploaded,
  exit 1. Also fixed WI-417's log encoding (UTF-8 **with** BOM — PowerShell
  5.1's `Get-Content` assumes ANSI without one, so every em dash read back as
  mojibake in the reader `run-local.md` hands you). `artifacts/pipeline`
  republished so tonight's 06:00 run uses this code. 733 tests.

- **2026-08-13** — **WI-417 done — the daily run leaves evidence behind.**
  Everything the pipeline printed already said what was wanted (which item was
  excluded and why, which summary was flagged and by which check); Task
  Scheduler simply threw it away. Now a per-run file lands in
  `%LOCALAPPDATA%\BrainHarbor\logs\pipeline-<date>-<time>.log` — **outside the
  repo deliberately**, since `artifacts/pipeline` is rewritten by every
  re-publish. Named to the second, not the day, so a manual run cannot clobber
  the 06:00 one; AutoFlush on, because the run whose log matters most is the
  one Task Scheduler kills at the 2-hour limit. Hand-rolled ~150-line provider
  rather than a Serilog dependency: retention, redaction and the printed path
  were all going to be custom anyway. **Retention is three limits, not one** —
  30 days, 100 files, 32 MB per run: the age limit alone cannot bound a task
  re-triggered in a loop, and neither bounds a runaway loop *inside* one run.
  **Secrets**: the `HttpClient → Warning` filter (the NCBI key rides in the
  query string) is unchanged but now **pinned by a test** that builds the real
  logging graph — nothing would have noticed the line being deleted, and the
  URIs now go to a file, not a console that scrolls away; on top of it a
  redactor scrubs configured key values and key-shaped text on the way to disk.
  Also **the reason the item was really asked for**: guardrail reasons carry a
  `FlagKind` now, so a run ends with "flagged because: reading level 1,
  invented numbers 1" instead of a bare count — the thing the DB cannot answer,
  because it stores a `summary_flagged` boolean and no reason. Verified with a
  real run against a dead endpoint (exit 4, 162 lines, stack trace, path
  printed last, neither configured key present in the file). **Known limit,
  documented not papered over**: a crash before the host builds writes no file;
  there the exit code is still the only signal.
  **Review caught six, three of them real bugs:** (1) the tally was declared
  INSIDE the try, so the catch-all zeroed it — and the likeliest exception is
  the upload at the very end, meaning the run that did two hours of LLM work
  and then failed to upload would report "summarized 0"; (2) the size cap
  measured UTF-16 chars against a byte limit, and these log lines are full of
  em dashes, so the file could run past its own ceiling (now the stream's
  position); (3) pruning could delete another RUN's live log — on Windows that
  delete succeeds against an open handle and the other process writes into
  nothing (now: never touch a file written in the last 10 minutes, and the
  writer no longer shares Delete). Also: the sink was never disposed in
  production (a provider added as an instance is disposed by neither the
  container nor LoggerFactory — harmless only because AutoFlush is on, which
  the next performance tweak would have quietly broken), the truncation write
  sat outside the try so a full disk could throw out of `ILogger.Log` and
  replace the exit code with a crash, and two runs starting in the same second
  left one with no log at all (now suffixed). Added a directory-wide 256 MB
  ceiling: 100 files x 32 MB is 3.2 GB, which is not what "pruned for you"
  should mean. Follow-up **WI-418** filed for putting the reason in the
  database, where the review queue can show it and the 137 already-pending
  items can be explained. 703 tests.

- **2026-08-13** — **The feed now runs itself, and the new prompts are proven
  in production.** Daily scheduled task registered ('BrainHarbor Pipeline',
  06:00, runs as Dan, StartWhenAvailable so a sleeping PC catches up rather
  than skipping; `-Unregister` reverses it). Triggered once to prove the chain
  rather than trust it: **exit code 0**, all six sources, ~51 new items.
  First production numbers for WI-415's prompts: `summarize-v4` flagged
  **2 of 42 (4.8%)** and `summarize-trial-v2` **0 of 9**, against 9.8% for the
  old `summarize-v3` at the OLD looser 8.5 gate — a stricter ceiling that
  rejects LESS, because the writing improved rather than the bar dropping.
  Prod now 1,179 published / 137 pending. **Task Scheduler captures no console
  output**, so the flag REASONS are lost (the DB stores only a boolean) —
  filed as **WI-417** (Dan's ask): per-run log files with rotation, secrets
  never logged.

- **2026-08-13** — **WI-415 done — AI summaries now written for 6th grade.**
  Three parts. (1) **The grader was measuring wrong**: `AllProse` joins the
  plain title and six blocks with newlines and a title has no full stop, so
  title ran into hook and every score was inflated ~0.7 of a grade (6.7
  reported vs 6.0 real across the 1,038 published items). Now block-aware, like
  the page grader. The SAME defect existed in the hype check — a negation in
  the title could excuse a "cure" claim in the hook — fixed and tested.
  (2) **The prompts now ask for 6th grade** with concrete rules (sentences
  under ~15 words, short everyday word over long): `summarize-v3→v4`,
  `summarize-trial-v1→v2`. Measured live through the real CLI: research median
  **4.7** (max 6.5), trials median **3.4** (max 3.6). (3) **Gate set to 7.0**,
  not 6.0 — deliberately: the prompt is the mechanism, the gate is a backstop,
  and gating AT the target would flag ordinary variation and drain the feed
  into the review queue (a flagged item does not publish). Was 8.5.
  New opt-in `Category=Live` test re-runs the golden set through the real CLI
  and prints the distribution — **CI now filters `Category!=Live`** and the
  body no-ops without `BRAINHARBOR_LIVE_TESTS=1`, because a [Trait] alone
  excludes nothing and it would otherwise have failed every PR build. One
  golden "ideal summary" was itself at 7.1 and got simplified — the yardstick
  has to meet the bar. **Published summaries are NOT retro-fixed**; they keep
  their older prompts' wording until re-summarized. Follow-up **WI-416**: the
  two graders still differ (medical-term allowance, hiatus rule), so the page
  6.0 and summary 7.0 are not strictly comparable. 675 tests.

- **2026-08-13** — **WI-401 DONE — the backfill finished; brainharbor.org is a
  real site.** All 6 sources green, 0 classification failures. **1,130 items
  published**, 134 pending (20 of them one-off classify failures for a human),
  106 held by the guardrails. `/trials` shows 207 trials, the feed 185 trial
  items. **The self-healing resume proved itself**: ctgov's cursor had been
  held back by the WI-401 fail-fast, so this run refetched that window,
  classified the 74 it had not reached, and advanced the cursor — no cleanup,
  no lost work, exactly what the last two attempts needed and did not have.
  WI-401 acceptance fully met: shared App Service + Postgres (~$1–3/mo
  incremental vs the ~$30 budgeted), DNS + managed TLS, deploy-on-merge with a
  smoke check, admin 2FA in prod, pipeline pointed at prod, feed backfilled.

- **2026-08-13** — **Home page now says who wrote what; filter bars fixed**
  (PR #14, released via #15, live on brainharbor.org). Dan's catches from the
  live site. (1) The hero read "we read new research and news, then explain
  it" — sounding like people — while AI was named only in the footer and on
  item pages. It now leads **"Scientists do the research. AI puts what they
  found into plain words"**; naming AI *without* that first sentence invites
  the worse misreading, that AI did the science. Also states plainly that
  summaries publish on their own and that a person does not check every one.
  Short sentences deliberately: the same content as one paragraph measures
  reading grade 8.3, this measures ~4.8. **Note, SUPERSEDED the next day by
  WI-414: ContentCheck did not gate the home page then** (it scanned only
  `Content/pages/*.md`; home is a Razor view), so reading level here was
  measured by hand. It gates reader-facing `.cshtml` now, failing above 6.0.
  Title + social description match;
  a test pins both halves of the disclosure. (2) `.feed-filters` had no CSS
  rule at all, so the submit button wrapped alone while the long toggle sat
  among the dropdowns — now one control row ending in the button, toggle
  beneath, on /research and /trials alike. 662 tests.

- **2026-08-12** — **brainharbor.org has real content.** First release through
  the new `develop → main` flow (PR #13) deployed itself and passed the smoke
  check, then the backfill ran with the WI-401 resilience fixes: **1,038 items
  published live**, 134 pending, 106 held by the guardrails (invented numerals,
  banned hype words, reading level > 8.5 — all doing their job on real data).
  PubMed alone: 1,099 fetched, 117 excluded as off-topic, 852 auto-published.
  **The fail-fast proved itself in production**: the usage limit died during
  `ctgov`, so that source stopped, its cursor stayed EMPTY, and the run ended
  with five clean sources instead of a poisoned database. Finishing it needs
  one more run and no cleanup at all. The 20 one-off classify failures went to
  the review queue as designed.

- **2026-08-12** — **WI-401 backfill: two failed attempts, both root-caused and
  fixed.** (1) **Hidden HTTP caps:** `AddStandardResilienceHandler()` replaces
  `HttpClient.Timeout` with an infinite one and applies its OWN 30s-total /
  10s-attempt defaults — so the `client.Timeout = 60s` line sitting right above
  it was decoration. It killed PubMed's catch-up fetch, then killed the sync
  UPLOAD after 3h20m of finished classify+summarize work (0 rows landed).
  Fixed with a shared `AllowLongRequests` helper on every resilient client, the
  inert `client.Timeout` lines deleted, and — because no test had ever touched
  `Program.cs`'s DI graph, which is how this reached prod — a new
  `PipelineHttpTimeoutTests` that asserts the timeouts the handler ACTUALLY
  runs with. (2) **Usage-limit death:** when the limit expires every item comes
  back Unclassified, and uploading those made the server *know* them, so no
  later run would ever classify them (532 rows hand-deleted across two
  attempts). Now a streak of 3 stops the source — processed items still upload,
  the cursor is HELD, the next run resumes the window — and because the
  classifier is shared infrastructure, the first source to prove it dead
  **latches the whole run** (small sources would never reach the streak alone).
  One-off failures still go up for a person, unchanged. Known residual filed as
  **WI-413** (counting is the wrong signal; the CLI should say *why* it failed).
  661 tests.

- **2026-08-11** — **WI-411 done — dedicated test database** (/next-item).
  DB tests now default to **`brainharbor_test`** in the same local/CI Postgres
  server — one word changed in `TestDatabase.cs`; DbUp's EnsureDatabase
  creates it on first run, so no compose/CI changes. Verified live: suite run
  created it (650/650), second run idempotent, dev-DB row counts identical
  before/after; dev DB had zero leftover test rows. The dirty-database rule
  (far-future seeds, page-until-found) now has ONE canonical home on
  `DatabaseFixture`'s doc comment; per-class comments point there. Dates kept
  as insurance. Deferred (review nit): the fixture guard accepts a REMOTE
  db named *test* — tighten when WI-401 makes remote DBs real.

- **2026-08-11** — **WI-410 done — sort the research feed** (/next-item).
  /research sortable by date (default, unchanged), readiness (highest first,
  **unscored last** — nullable score, the published_at NULLS LAST trap), and
  kind (research → news → trials → preprint grouping, newest first within a
  group, decided explicitly in SQL). Plain select in the existing GET filter
  form (no-JS + htmx for free), composes with tumor/early filters, canonical
  `?sort=` in the URL (garbage normalized away; input never reaches SQL —
  a whitelist switch picks among fixed ORDER BY strings). Review's two copy
  catches applied: "Closest to helping you" was a personal promise the
  anti-hype rules forbid → "Most ready to use"; "By type" → "By kind" (one
  word per concept). **/trials does not get the control** (no readiness score
  there; update recency already meaningful). Live-verified descending dials.
  650 tests.

- **2026-08-10** — **WI-409 done — home page leads with the feed** (/next-item,
  PR [#8](https://github.com/badsonstudios/BrainHarbor/pull/8) squash-merged).
  Home now renders the newest 4 published items ("Latest updates", same
  `_FeedCard` partial and safety rules as /research: published-only, closed
  trials excluded, early-stage hidden unless the reader's persisted WI-307
  cookie opts in — parse shared via `Research.IndexModel.ReadEarlyChoice` so
  the pages can't drift). Cards sit BELOW the three doors (deviation from the
  backlog's "above", approved: the crisis-help door must not scroll away).
  The false "research feed … coming soon" sentence now refers only to the
  digest, and a test fails if home ever claims the feed is coming while
  published items exist. Section omitted entirely at zero published items.
  Also fixed 4 **pre-existing** TrialsPageTests failures on `main` (Dan's 8/1
  live near-me testing put 20+ real trials in trials_cache and the 7/20-dated
  seeds fell off browse page 1) with far-future seed dates, and filed
  **WI-411** (dedicated test DB) so that idiom stops spreading. 640 tests.

- **2026-08-01** — **WI-403 done — the trial finder; M4 autopilot run ENDS
  here** (everything left needs Azure). `/trials` browse over `trials_cache`
  with tumor-type and phase filters, and `/trials/{nct-id}` pages.
  **Near me is a live, keyless `filter.geo` query to ClinicalTrials.gov at
  request time** (architecture.md §7), from either a typed ZIP or browser
  geolocation. The ZIP form is the PRIMARY path and geolocation is progressive
  enhancement on top: this audience should not have to grant a permission
  prompt (or run JavaScript) to find a trial. ZIP → point uses the Census ZCTA
  gazetteer shipped as a file (33,791 rows, public domain); the ZIP is used for
  the outgoing query only, never stored or logged. The live call **fails soft**
  by design — a slow registry degrades to "we could not reach ClinicalTrials.gov
  just now, here is the browse list", never an error page.
  Tumor-type filtering matches the registry's own condition strings against the
  taxonomy's labels and aliases (walking the tree, so "glioma" finds
  glioblastoma), because `trials_cache` holds trials that were never classified
  and so have no tumor_tags — that was the open question WI-402 left.
  Two safety rules pinned by tests: **only a PUBLISHED item may lend its
  plain-language text to a trial page** (the join must not become a side door
  around the review gate), and the registry's own words are always labelled as
  the registry's, never as our plain-language writing. Attribution + link back
  on every trial page (PLAN.md §5 licence requirement).
  Live-verified: 25 open brain-tumor trials within 50 miles of Columbus, 14 for
  glioblastoma, nearest sites resolved correctly. `/trials` added to the axe
  scan and to sitemap.xml.
  **Review caught four blockers, three of them the same shape as WI-402's:**
  (1) the live call did NOT fail soft on the case it was built for —
  `HttpClient.Timeout` throws `TaskCanceledException`, which IS an
  `OperationCanceledException`, so the exception filter meant to let real
  cancellation through was letting the 8-second timeout through too, giving the
  reader a 500. (2) an unknown status was rendered as "this trial is not taking
  new patients" — a fabricated claim directly above a sentence admitting we
  cannot tell (the exact rule `FeedRow.TrialHasClosed` exists to enforce; now
  three states, not two). (3) the outgoing near-me URL contains the reader's
  coordinates, and `IHttpClientFactory` logs request URIs at Information — so
  every search wrote a location to the logs while the page promised "we do not
  store it" (`RemoveAllLoggers`, plus `no-store`/`no-referrer` on those
  responses, and `/privacy` now says plainly what happens to a ZIP).
  (4) near-me searches the WHOLE registry but linked to `/trials/{id}`, which
  404s for anything outside our fetch window — on a fresh database nearly every
  result. Those rows now link to the registry.
  Also: registry count instead of our page size in the heading, closed trials
  don't show their frozen hook, deep `?page=` clamped, unknown-status trials no
  longer vanish from browse, the tumor menu drops slugs that match nothing, and
  near-me/browse now share one definition of a tumor type (with the label
  quoted — "DIPG (pontine)" carries live Essie grouping characters).
  635 tests.

- **2026-08-01** — **WI-402 done — trials fetcher** (autopilot M4):
  ClinicalTrials.gov v2 fetcher, `trials_cache` (migration 0007), a
  `trial_update` feed item for trials someone can still join, and a
  trial-specific summarization prompt. Live-verified against the real registry
  (80 trials in a 5-day window: all mapped, 79 with site coordinates, correct
  50/30 open-vs-closed split, cursor advanced clean).
  **The design changed twice under review, both times for the same reason —
  a trial's FACTS and its plain-language text obey opposite rules:**
  (1) the first cut wrote `plain_summary` into `trials_cache` through the
  unfrozen facts path, which would have carried summaries the safety checks
  FLAGGED, or a human REJECTED, to readers anyway. The cache now holds no
  plain-language column at all; editorial text lives only on
  `aggregated_items`, and facts move through their own `POST /api/sync/trials`.
  (2) refreshing the cache fixed browse but not the pages a reader lands on —
  a published trial page, its feed card, its search snippet and its RSS entry
  all kept saying "now enrolling" forever, because a known trial is never
  re-summarized. The item page now reads status live from the cache; closed
  trials leave the feed, search snippets and RSS but keep their permalink,
  which says plainly that they are not taking new patients.
  Also: facts upload BEFORE classification (an off-topic verdict no longer
  swallows a status change), fact-only trials create no review-queue rows, a
  known trial costs no LLM call, the truncation guard can no longer walk the
  cursor backwards, 5xx/network failures retry (a 400 does not), and three real
  trials were added to the golden set — a new versioned prompt was otherwise
  ungated. Stripped real investigator names, phones and emails from the
  recorded fixtures before committing (public repo). 590 tests.

- **2026-07-31** — **Autopilot M4 started** (branch `auto/M4`). WI-401 (Azure)
  is `[user]` + real money, so it is skipped; WI-402/403 (trials) are pure code
  and buildable without cloud. WI-404/405/406/407/408 all depend on WI-401, so
  the run stops after the trials feature.

- **2026-07-30** — **WI-303 golden-set accuracy run — DONE (by the assistant)**:
  the local `claude` CLI is invocable here, so ran the classify-v1 prompt
  against all 20 ratified golden items. **Stage 20/20 (100%), relevance 18/20
  (90%), primary-tag 18/20 (90%)**, exact-tag 13/20 (65%). The 2 relevance
  misses are borderline "excluded" reviews the model kept (safe direction);
  tag misses are completeness, not wrong tags. Note: `claude -p` used
  **Haiku 4.5** — consider a stronger model for classify/summarize before Auto
  mode. Recorded in the golden-set README. WI-303 acceptance now fully met.

- **2026-07-20** — **WI-302 done** (autopilot M3): Claude Code CLI wrapper.
  Invokes `claude -p --output-format json` (prompt on stdin), unwraps the JSON
  envelope, parses the model's JSON into the expected shape, validates, and
  retries ONCE on garbled output — failing fast on timeouts, auth-style exit
  codes, and validation (deterministic). A bad call NEVER returns a value
  (never a guess). Versioned PromptTemplate with a strict placeholder guard.
  Review caught a blocker (spawn failure threw instead of failing safe) +
  process-handling fixes (bounded stdin write, full stdout drain, kill on
  cancellation) — all fixed; real-runner "CLI not installed" test added. 410.
- **2026-07-20** — **Autopilot M3 started** (branch `auto/M3`, PR #5). Repo
  made public → scrubbed a personal email (NCBI contact → role address) and
  added a PII/secrets scan to the commit-push-pr + autopilot skills. Dan's
  call on the M3 quality gate: **build in Review mode** — full capability, but
  real AI summaries wait in the queue for Dan to judge before auto-publish;
  he flips Publishing:Mode=Auto when confident. Golden set ships as a DRAFT
  for Dan to ratify.
- **2026-07-20** — **WI-301 done** (autopilot M3): golden set — 20 real PubMed
  abstracts hand-classified (11 patient_relevant, 5 early_stage, 4 excluded)
  with ideal 6-block summaries for 10, numbers verbatim from source. Rubric +
  validation tests (real taxonomy slugs, documented vocab, complete
  summaries, every case has a rationale). Flagged a taxonomy gap
  (spinal-cord tumors) for later. DRAFT pending Dan's ratification. 393/393.
- **2026-07-20** — **WI-212 done — auto-publish mode (Dan's request)**: the
  human review gate is now **optional**. `Publishing:Mode` config, **Auto by
  default**: a summarized item that passes the automated safety checks
  publishes itself (slug generated, `review_events` row with actor `auto`);
  flagged or not-yet-summarized items stay pending for a person; Review mode
  restores mandatory review. The item page is **honest** — auto-published
  items say "written by AI and published automatically… a person did not
  review it," not "reviewed by a person." Chose "hold only the flagged ones"
  (Dan's pick) so the automated guardrails (numeral post-check, banned-phrase
  scan, reading level — all M3/WI-304) gate every auto-publish. **Safe-by-
  construction until M3**: no summarizer yet → nothing has a summary → nothing
  auto-publishes, even though the mode is on. Design docs (PLAN,
  content-pipeline §"Publish mode", data-model, architecture, both CLAUDE.md)
  updated — human review is a mode now, not a hard requirement. 384/384.
  (On `auto/M2`, extends PR #4.)

- **2026-07-20** — **WI-211 done — M2 COMPLETE**: live shakedown against the
  real PubMed, NCI, ScienceDaily, medRxiv and bioRxiv endpoints, from an
  empty database. The loop works: 5/5 sources → 1,360 items pending → approve
  one → exactly 1 visible on /research, 1,359 still behind the gate; a second
  run ingested **0 duplicates** and left the published item published (the
  WI-202 human-decision fix proven for real).
  **Three real bugs only a live run could find, all fixed:**
  (1) the pre-filter's keep-bias is right for sources that already selected
  for us, but bioRxiv/medRxiv return every field — it passed **91%** of the
  firehose (protein folding, chondrogenesis) into the review queue. Added a
  SourceScope so firehose sources require a POSITIVE brain-tumor match:
  bioRxiv 2863→77, medRxiv 784→11, total 4871→1360.
  (2) the feed filtered to relevance='patient_relevant', but nothing is
  classified until M3 — so approving an item in M2 did nothing visible.
  Unclassified-but-approved items are now shown; early-stage stays behind the
  toggle. (3) ScienceDaily stamps dates "EDT", which .NET won't parse — all
  48 items were undated, sorting to the bottom of the feed forever and never
  advancing the cursor. Now 0 undated of 48.
  Also hardened the feed ordering tests to stop assuming an empty table —
  the suite now passes *with* 1,360 real rows present. 374/374.

- **2026-07-19** — **WI-210 done** (autopilot M2): source health + the
  scheduled task. Added POST /api/sync/failure so a broken source actually
  writes last_error — until now nothing ever did, so a source that died a
  week ago would still show its last success. /admin/health lists every
  source with plain-language staleness ("5 days ago", "never"), flags
  failures first, and calls out any expected source that has never reported
  at all. The pipeline reports its own failures (best-effort — reporting must
  not break the run) and raises a desktop toast on finish. Task Scheduler
  registration script uses StartWhenAvailable so a sleeping PC catches up
  rather than skipping the day. 360/360.

- **2026-07-19** — **WI-209 done** (autopilot M2): the public feed. Two
  safety rules are enforced in the repository rather than a view — only
  status='published' is ever visible, and early-stage animal/cell work is
  hidden unless the reader ticks the box (a mouse-study headline reads as
  false hope). Tumor filters walk the taxonomy tree, so browsing "glioma"
  surfaces glioblastoma; filter values are normalized against a fixed set and
  never concatenated into SQL. Item permalinks render the badge with a
  plain-language explanation of what it means, and refuse to invent a summary
  when there isn't one. A pulled item's permalink 404s exactly like one that
  never existed. Tests pin that raw source text never reaches a public page.
  /research is now live — only /trials remains dead. 348/348.

- **2026-07-19** — **WI-208 done** (autopilot M2): the review queue — the
  human gate itself. Pending items with the badge a READER would see (same
  mapper the public feed uses, so the decision is made on what actually
  publishes), source text behind a details toggle for comparison, htmx
  approve/reject with a no-JS form fallback. Every transition writes an
  append-only review_events row (who, what, when, note) because "every
  published summary is human-reviewed" needs to be auditable, not assumed.
  Status transitions are guarded, so two open tabs can't double-apply, and
  slugs are generated from the plain-language title on approval with
  collision handling. Flagged items sort first. 22 new tests. 330/330.

- **2026-07-19** — **WI-207 done** (autopilot M2): admin auth — ASP.NET
  Identity (the only EF Core usage; its tables live in an `identity` schema so
  DbUp and EF never collide), ONE account seeded from config with no
  registration or password-reset endpoint, TOTP 2FA enrolment (manual key, no
  JS/QR dependency), hard lockout, anti-forgery on every admin POST, POST-only
  logout. Folder-level authorization means a new admin page is protected by
  default rather than by remembering an attribute. 12 boundary tests. Note:
  the seeder logs loudly and continues if the password is rejected — a weak
  config value must not silently leave the review queue unreachable. 308/308.

- **2026-07-19** — **WI-205 + WI-206 done** (autopilot M2): NCI + ScienceDaily
  RSS fetchers with per-source licensing enforced in the type system
  (FeedTextPolicy; ScienceDaily is headline+teaser+link only, and the enum
  now fails closed), and medRxiv/bioRxiv preprints with source_kind forced to
  "preprint" at all three layers. Review probed the LIVE APIs and found two
  silent breakages: the NCI feed URL 404d (would have failed every run
  forever — corrected to the publishedcontent path, verified 200/10 items),
  and the preprint API pages at 30 not 100, so the fetcher read 30 of ~745
  records and then advanced the cursor past the rest. Paging now follows the
  API's own total and a truncated window advances only to the newest record
  actually read. Also: relevance is judged on the FULL description before the
  licence truncates it (the teaser cut was dropping breast-cancer items whose
  brain-metastases mention fell past the cut), empty feeds warn instead of
  looking healthy, and PubMedPreFilter is renamed BrainTumorPreFilter now
  that three sources share it. 296/296.

- **2026-07-19** — **WI-204 done** (autopilot M2): PubMed fetcher (paged
  esearch + efetch XML, self-healing reldate window, NCBI throttling and key)
  and the hard-rule pre-filter. **The pre-filter decides what patients never
  see, and it was silently dropping real research** — found across my own
  tests and review: a trailing `\b` meant prefixes never matched plurals (so
  "brain metasta" missed "brain metastases" and breast-cancer brain-mets
  research vanished); multi-word terms assumed a literal space (missed
  "brain-tumor", "tumor-treating fields"); the notice rule ate ordinary
  titles starting "Response to"/"Withdrawal"/"Correction of"; the keep list
  lacked the words the audience uses ("brain mets", "CNS involvement",
  "leptomeningeal", and *"brain cancer"* itself); and broad neurology rules
  dropped late-effects research (stroke after cranial irradiation, dementia
  after whole-brain radiotherapy). All 15 titles are now regression tests.
  Also: pagination with **cursor held back** when a window is truncated
  (otherwise the remainder is invisible forever), esearch errors throw rather
  than burning the window, ArticleDate preferred for ahead-of-print,
  OtherAbstract excluded, and the NCBI key no longer lands in logs. 269/269.
- **2026-07-19** — **WI-203 done** (autopilot M2): Pipeline host (user-secrets
  config + validation, structured console logging, distinct exit codes for
  Task Scheduler, retry/backoff), typed sync client (chunking, cursor only on
  the last chunk, actionable auth errors, never logs the key), ISourceFetcher
  abstraction, and a runner with per-source isolation. Review caught a
  **blocker**: `Enumerable.Chunk` yields no chunks for an empty list, so the
  "advance the cursor when nothing is new" call made no HTTP request at all —
  a source's window could never move forward and would refetch a
  forever-growing range. My unit test had passed because it asserted the
  *stub's* recorded call, not the real client (mock hiding the bug). Added a
  real `/api/sync/cursor` endpoint + real-server tests. Also: AlwaysUpload so
  ClinicalTrials.gov updates aren't dropped by the new-only filter, full
  contract round-trip test, unknown-arg rejection, args no longer bind config
  (would put the key in the process list). 203/203.
- **2026-07-19** — **WI-202 done** (autopilot M2): sync API (state/check/items)
  with API-key auth (constant-time, fails CLOSED → 503 if unconfigured),
  key-partitioned rate limiting, per-item validation, and an idempotent
  upsert. Two real bugs found before commit: (1) API 401s were being
  re-executed into the HTML status page — machine clients got markup, and a
  POST re-execute degraded to a bogus 400; (2) **review blocker** — a
  classify-only rerun could null the plain_summary of an already-*published*
  item, leaving a live patient page contentless with no human involved.
  Content is now frozen once a human reviews it (`Frozen` count in the
  response). Also: cursor no longer advances on all-rejected batches (would
  skip that window forever), single-source cursor rule, field bounds +
  source whitelist, null-body 400s, DateOnly Dapper handler. 181/181.
- **2026-07-19** — **WI-201 done** (autopilot M2): aggregated_items +
  source_sync_state migration with CHECK constraints (incl. preprint can
  never be patient_relevant, enforced in the DB); taxonomy.yml as a **tree**
  (22 types, parent/child) + TaxonomyStore with alias resolution, Matches()
  ancestor walk, and a FilterTags gate that reports rejected tags.
  Review caught two **medically wrong aliases** — "grade 4 glioma" mapped to
  glioblastoma (WHO CNS5 grade 4 also covers IDH-mutant astrocytoma and H3
  K27-altered DMG) and DIPG treated as a synonym for diffuse midline glioma
  rather than its pontine subset. Both would have shown patients research
  about a different disease. Also fixed the DbUp journal race **in prod**
  (advisory lock, not just serialized tests) and the NULLS LAST feed index.
  data-model.md updated to match. 147/147.
- **2026-07-19** — **M1 MERGED**: Dan ran the site, visual review passed
  ("everything is looking good"); running it surfaced one real gap — no
  shipped page used a glossary term, so the tooltip was invisible; fixed with
  a real-pipeline sample on /dev/styleguide. PR #3 squash-merged to `main`
  (0f6be65), `auto/M1` deleted. Autopilot M2 starting.
- **2026-07-19** — **M1 COMPLETE** (autopilot): all 8 items shipped on
  `auto/M1`, 112/112 tests, ContentCheck clean, 0 build warnings. Awaiting
  Dan's visual review + merge of PR #3. Nothing merged to `main` by autopilot.
- **2026-07-19** — **WI-107 done** (autopilot): six curated shell pages
  (/about, /how-we-write, /start, /digest, /privacy, /terms) at reading
  grades 2.5–4.8; disclaimer partial rendering from front-matter flags;
  scaffolded Razor Privacy page deleted so /privacy is curated content.
  Review caught 3 blockers in the COPY, all fixed: /start had no emergency
  red flags before its calming copy (now leads with 911 signs), three "what
  to do next" CTAs pointed at the unbuilt /research, and a typo'd disclaimer
  flag rendered an empty box instead of the medical disclaimer (now a
  ContentCheck failure). Privacy/how-we-write claims trimmed to what the
  code actually does today.
- **2026-07-19** — **WI-106 done** (autopilot): tools/BrainHarbor.ContentCheck
  — Flesch-Kincaid gate (fail >8.5, warn ≥7.5) with block-aware sentence
  extraction (headings/bullets don't inflate the grade — review measured
  +1.6 grades before the fix), medical-hiatus syllable rule, front-matter
  validation, overdue review_due + missing-source warnings, 40-word glossary
  limit, loud warning on missing roots; CI step added (runs on all content —
  intentionally stronger than changed-only). 22 new tests. 98/98.
- **2026-07-19** — **WI-105 done** (autopilot): GlossaryStore (term files per
  content-pipeline §6, snapshot reloads), GlossaryMarker Markdig extension
  (first occurrence per page → native-popover button tooltip, WCAG 1.4.13;
  paragraphs only; %%term%% + !%term% escapes), /glossary A–Z, 3 seed terms.
  Review caught 2 real bugs pre-commit: terms split across source line wraps
  never matched (soft-break merge added) and "non-IDH-mutant" got a wrong
  tooltip (hyphen-aware boundaries) — both pinned with tests. 76/76.
- **2026-07-19** — **WI-104 done** (autopilot): ContentStore (Markdig with
  DisableHtml + YamlDotNet front matter per content-pipeline §3, mtime-keyed
  cache, slug-regex traversal guard, IO races → 404); catch-all Razor route
  renders /{slug} and /{section}/{slug} with provenance block; Content:Root
  config override for tests; publish glob for Content/pages. 22 new tests
  (parsing, routing, cache lifecycle, HTML-escape).
- **2026-07-19** — **WI-109 done** (autopilot): ResearchStage enum +
  StageBadge mapper (single source of truth incl. server-built aria-labels),
  _StageBadge (dot-meter/glyph per handoff) + _FeedCard partials,
  /dev/styleguide (dev-only, 404 in prod) rendering all 7 badge kinds + 4
  sample cards; axe scan of the styleguide added to the E2E gate. DB
  taxonomy→enum mapping decision recorded on the enum + WI-209.
- **2026-07-19** — **WI-103 done** (autopilot): helpline band on every page
  (aside landmark, CareLine tel link, → /get-help-now); /get-help-now with
  988, Crisis Text Line, CareLine, NCI, CancerCare as one-tap buttons; custom
  404 + calm Error page via status-code re-execute (direct /status/N hits
  404; large-text toggle points at the original URL on error pages). Nav +
  home "dead links" note: /get-help-now is now live.
- **2026-07-19** — **WI-102 done** (autopilot): large-text mode (22px base)
  via cookie-persisting middleware, plain-link toggle in the header (proven
  with JS disabled in Playwright); axe-core smoke tests on the shell in both
  text modes, 0 serious/critical; Kestrel dual-host test factory; CI installs
  Chromium. Review found an open-redirect blocker (protocol-relative path) —
  fixed + regression-tested; Secure cookie flag + shared URL helper applied.
- **2026-07-19** — **WI-108 done** (autopilot): Clear & Kind theme folded into
  site.css (new palette, band/card/badge tokens, 72rem container + 46rem read
  column), nav-cta pill, footer link list + ai-note, home rebuilt as Entry Hub
  (three doors → /start, /research, /get-help-now), print.css flattens the new
  surfaces incl. print-safe dot-meter ink (review Should-fix applied). Review:
  approve, no blockers. Needs Dan's visual eyeball at end of run.
- **2026-07-19** — **Design chosen & planned in**: static mock-up generated
  (`.claude/work_files/mockup/`), run through Claude Design by Dan; approved
  handoff ("Clear & Kind" + Entry Hub) moved to `docs/design/entry-hub-handoff/`.
  Backlog updated via /pm: new WI-108 (adopt theme + Entry Hub shell) and
  WI-109 (stage-badge dot-meter + feed-card partials); WI-102/103/107/209/306
  amended to depend on / reference the handoff. Next up is now WI-108.
- **2026-07-19** — **WI-101 done**: design tokens (18px-base scale, AA/AAA
  palette, spacing), real `_Layout` shell (landmarks, v1 nav, footer
  disclaimer), print.css (print-to-PDF verified), WI-005 htmx demo deleted,
  WebApplicationFactory render test added. Review clean, fixes applied.
  PR [#2](https://github.com/badsonstudios/BrainHarbor/pull/2).
- **2026-07-19** — **M0 closed**: WI-001 done (Dan bought brainharbor.org);
  PR #1 squash-merged to `main` (ce5929d) after Dan's review; `auto/M0`
  deleted. Secrets follow-ups all resolved same day.
- **2026-07-18** — **Autopilot M0 run COMPLETE**: WI-002..WI-006 shipped on
  `auto/M0` (PR #1, draft), CI green on the tip. WI-001 `[user]` outstanding.
- **2026-07-18** — **WI-006 done** (autopilot): GitHub Actions CI — build +
  test (Release) on push/PR to main, Postgres 16 service container so the
  Database-category tests run in CI too.
- **2026-07-18** — **WI-005 done** (autopilot): Htmx.Net + TagHelpers (htmx
  2.0.10 vendored), demo partial with no-JS fallback (curl-verified both
  paths); Dapper `IDbConnectionFactory` (NpgsqlDataSource DI); dev
  SYNC_API_KEY set in both apps' user-secrets. ⚠️ Dan: (1) populate
  `.claude/.env` from `.env.example` (autopilot may not touch it), (2) get a
  real NCBI_API_KEY and set it in Pipeline user-secrets + `.env`.
- **2026-07-18** — **WI-004 done** (autopilot): docker-compose (Postgres 16 @
  5433, named volume, healthcheck), DbUp on dev startup with 0001 baseline,
  connection string in user-secrets, DB smoke test. Verified on a fresh
  container; code review clean (fixes applied).
- **2026-07-18** — **WI-003 done** (autopilot): BrainHarbor.sln + Web (Razor
  Pages, net10.0) + Pipeline (console) + Tests (xUnit); build + test green.
- **2026-07-18** — **WI-002 done** (autopilot): private repo created + first
  commit pushed. ⚠️ Note: `gh` resolves to **badsonstudios**, not `danheinz`
  (`/users/danheinz` 404s — account renamed?). Repo is at
  `github.com/badsonstudios/BrainHarbor` (private ✓, `.env` untracked ✓).
  Dan: confirm the account; then docs/references mentioning `danheinz` get updated.
- **2026-07-18** — **Autopilot M0 started** (unattended run, branch `auto/M0`).
  WI-001 `[user]` (buy brainharbor.org) skipped — Dan's item, does not gate M0.
- **2026-07-18** — Domain changed to **brainharbor.org** (from .net); all docs
  updated. WI-001 now = buy brainharbor.org.
- **2026-07-18** — Workflow installed: `.claude/` adapted from ClaudeMon
  (skills: startup, pm, next-item, check-code, review, commit-push-pr, explain,
  deep-research; agents: code-reviewer, debugger, deep-research-agent; env
  hook, scripts, settings). Backlog created: M0–M4 decomposed into 34 work
  items. This file created.
- **2026-07-18** — Architecture pivot: pipeline moved local (console app +
  Task Scheduler + Claude Code CLI, no Anthropic API key); site gets a sync
  API + admin review queue; every published summary human-approved; Hangfire
  removed. Docs updated.
- **2026-07-18** — Decisions: brainharbor.net; weekly digest; local-first dev,
  Azure deferred to M4; private GitHub repo; toolchain verified (.NET 10 SDK,
  git, Docker, gh as danheinz).
- **2026-07-12** — Aggregation-first pivot (feed + plain-language summaries is
  the v1 product; static hub moved to Phase 2). Stack changed to Razor Pages +
  htmx on .NET 10 (Htmxor dead). Full design-doc set written: PLAN.md +
  docs/{architecture, sitemap, content-pipeline, data-model, roadmap}.

<!--
Maintenance rules (for the assistant):
- Starting an item  → set "In progress" (item + timestamp + current step).
- Finishing an item → move to Log with date, one-line outcome, PR link;
  update "Next up"; check the box in docs/backlog.md.
- Blocker/stopping mid-item → record exactly where things stand under
  "In progress" + "Blockers" so a cold session can resume.
- Keep "Notes for the next session" current; prune stale notes.
- Never delete Log entries; newest first.
-->
