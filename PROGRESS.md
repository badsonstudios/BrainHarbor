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
| **In progress** | nothing mid-flight. **WI-503 done 2026-08-30** (the reader-choice gate — `:::outlook`; see the log). **WI-501 done 2026-08-30** (shared content blocks — the P5 Wave 0 foundation; see the log). **Reader-facing work now gets a local review before deploy** (Dan's rule, 2026-08-29 — see memory `test-locally-before-deploy`). (2026-08-30: **Phase P5 filed** — WI-501…WI-557, tumor guides: 24 tumor hubs + a 17-page treatment library + a 12-page tests library, from five research tracks committed at `docs/research/tumor-guides/`. Start with **WI-501** (shared content blocks — a code item everything else inherits) and **WI-504** (`[user]`, fetch the 403-blocked NCCN patient guideline — it blocks Wave 1). **Caregiver question resolved 2026-08-30: yes, every tumor hub carries a prominent caregiver section** — Dan's reasons were surgery aftercare and living with someone who has seizures. That added **WI-558** (the standard + shared block) and **WI-559** ("what to do when someone has a seizure" — life-safety content the site did not have at all), both of which land BEFORE WI-513 because the template proves them. **59 items now, WI-501…WI-559.**) (2026-08-29: **WI-462** live country counts; **WI-458…461** trials/research review round; **WI-457** multi-select country picker + a research→trials signpost; **WI-455** trials filterable by country, research half dropped once the cost was clear.) (2026-08-26: **Phase P4 filed** — WI-442…WI-454, depth for the brain tumor reader. My own ranking after WI-455: **WI-442** reader-report notes (hours), **WI-443** restore rehearsal, **WI-444** pathology-report explainer. 2026-08-23: **WI-441** — page-open counts on `/admin/health`, no identifiers of any kind, `/privacy` rewritten to match. **WI-440** — mobile layout: hamburger nav, stacked filter forms, 1107px → ~890px before content; axe now scans at 390px. 2026-08-21: **WI-438** — pagination was broken on `/research` AND `/trials` since forever, `page` being a reserved Razor Pages route key; fixed + replaced with a real pager. 2026-08-19: **WI-437** journey path replaces the dial + badge, closes WI-429; **WI-436** `/start` rewritten. Open tickets: **WI-439** the Kestrel test-host flake that blocked a deploy on 2026-08-22; **WI-435** ContentCheck exits 0 after checking nothing if its pages-root argument is swallowed.) WI-401, WI-414, WI-415 all done and **released to prod** (PRs #17, #19). **Daily scheduled task registered 2026-08-13** ('BrainHarbor Pipeline', 06:00, runs as Dan, StartWhenAvailable) — the feed now updates itself, and since **WI-417** each run leaves a log behind. |
| **WI-401 record** | **Azure provisioning: SITE IS LIVE** at app-brainharbor-prod-eus2.azurewebsites.net (2026-08-11, shared-infra option A: web app on Moodathon's B1 plan `asp-shamoody-prod-eus2`, `brainharbor` DB + own role on `db-shamoody-prod-eus` PG17, schema owned by `brainharbor`, PUBLIC revoked). **Continuous deploy PROVEN end-to-end** (PR #11 merged 425ec9b): merge to `main` → build+test+ContentCheck → deploy → smoke check, all green live. Gotchas hit & fixed: PowerShell Compress-Archive writes backslash zip entries (Kudu chokes; workflow's ubuntu zip is fine), PG15+ public-schema perms (brainharbor now owns its schema), **SCM basic auth was disabled by default** (enabled for publish-profile deploys; OIDC upgrade deferred). Prod secrets in `.claude/.env` (BRAINHARBOR_PG_PASSWORD, SYNC_API_KEY_PROD, ADMIN_PASSWORD_PROD) + App Service settings. Plan memory 81% with both apps (77% before; escape hatch = B2 +$13/mo). **https://brainharbor.org + www LIVE with managed TLS (2026-08-11)** — Namecheap A/CNAME/asuid-TXTs verified, hostnames bound, SNI certs issued+bound (Dan still to delete Namecheap's conflicting `@` URL-Redirect record). Admin account seeded + 2FA enrolled (address in `.claude/.env` as ADMIN_EMAIL — not written down here: the repo is public and it is half of the admin login). Pipeline points at prod. **BACKFILL DONE for 5 of 6 sources (2026-08-12): 1,038 items published live**, 134 pending (114 classified + 20 one-off classify failures for a human), 106 flagged by the guardrails. Home shows real cards; /research shows 615 by default (early-stage behind the toggle). **Only `ctgov` remains** — it hit the usage limit and the new fail-fast held its cursor empty, so one more `dotnet run --project src/BrainHarbor.Pipeline -- --once` when a limit window is free finishes it. No cleanup needed. |
| **Next up** | **WI-505** (~40 glossary terms). Then **WI-558** (caregiver block + standard) and **WI-559** ("what to do when someone has a seizure"), which finishes Wave 0. Older list, for context: **WI-505** (~40 glossary terms), **WI-558** (caregiver block + standard), **WI-559** ("what to do when someone has a seizure"), which finishes Wave 0. Then Wave 1: **WI-506…WI-512**, and **WI-513** proves the template on low-grade glioma before it is copied 23 times. **WI-501, WI-502 and WI-504 are done and merged** (PRs #68, #69) — the block mechanism, the editorial standard, and the four hand-fetched sources. Older threads, unchanged: **WI-431** (harden the deploy smoke check — six deploys on 2026-08-15 each served 500s on the inner pages for ~a minute while `/` stayed up, so the check passed straight through it; Dan asked what it involves and has not yet said go), then the reader-report work (notes shown in the queue + a count of reports, Dan's call: count reports not people, no identity stored). Then Dan's calls: **WI-404** (digest — needs an ESP account), **WI-408** (soft launch). Assistant-buildable now: **WI-413** (classifier unavailable vs odd item — the last hole in the fail-fast, and the task now runs unattended nightly), **WI-412** (/tumors plain-English descriptions), **WI-418** (store WHY a summary was flagged), **WI-416** (one reading-level grader, not two), **WI-406** (maintenance run), **WI-407** (pre-launch hardening). |
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

- **2026-08-30** — **WI-503 done — outlook now sits behind a choice the reader
  makes, and a mistyped gate fails the build instead of publishing the
  prognosis.** P5 Wave 0. **PR #70.** Authors write `:::outlook` … `:::` in a curated page;
  it renders as a `<details>` that is **closed on load**, with the §12.5
  warning sentence and the Show/Hide label emitted by the component rather than
  typed per page, so 24 tumor hubs cannot each soften them.
  **It could not have been "just write the HTML":** `ContentStore` renders with
  `DisableHtml()`, so a `<details>` typed into a .md file renders escaped. It
  had to be a Markdig component, which is what made the failure mode below
  possible in the first place.
  **The whole item is one safety property, and my first implementation did not
  have it.** Markdig renders an unknown `:::name` as an anonymous `<div>` — so
  `:::outlok` would publish the outlook section wide open, no error, green
  build. I validated the parsed tree, which catches that one. **Review found
  four more that the tree cannot see, because the typo stops a container being
  built at all**, and I reproduced every one before fixing it:
  `::outlook` (one colon short) renders as a plain paragraph; a tab or four
  spaces before the fence makes it an indented CODE block, which renders the
  words in full **and** hides them from the 6.0 reading check, since
  ContentCheck grades inline text and a code block has none; an unclosed fence
  swallows the rest of the page — caregiver section, support links — into a
  disclosure the reader was told is only about outlook; and `::outlook::`
  inline is a different node type entirely. Fixed by auditing the raw source
  alongside the tree: every `:::`-like line must be accounted for by a
  container that actually parsed. Nine tests, written first and watched to
  fail.
  **Two more fail-open paths, both about renderer registration.** Setting the
  pipeline up twice on one renderer put Markdig's default renderer back (its
  extension re-inserts at index 0 when it finds its renderer missing, and my
  `Contains` early-return then bailed) — not live today, since every call site
  allocates a renderer per page, but "reuse the renderer" is an ordinary
  optimisation and the symptom is silent open prognosis. And registering the
  gate extension **before** `UseAdvancedExtensions()` loses the same way, which
  is a one-line mistake with no visible symptom. Rather than leave that to a
  comment, the pipeline now **renders a probe gate at start-up and refuses to
  build if it did not come out closed** — a mis-ordered pipeline fails loudly
  instead of publishing.
  **I also had the print story wrong.** I checked `site.css` for `@media print`,
  found none, and concluded the site had no print styles — `print.css` has
  existed since WI-101, loaded `media="print"` **after** site.css, so my rules
  were both in the wrong file and losing ties. Moved. Printing reflects the
  reader's choice: a gate left closed stays closed on paper (forcing it open
  hands the outlook section to someone who declined it), and the toggle is
  hidden once open, since "Hide the part about outlook" is an instruction that
  cannot be followed on paper. Verified by screenshot in print emulation, both
  states, not by reading the CSS.
  909 tests (875 before), ContentCheck 50/0 — all 50 existing pages pass the
  new fence audit, so no false positives. Eyeballed at desktop and 390px,
  closed/open/keyboard-focused, with JavaScript off.
  **`Content/blocks/` still ships empty and no tumor content was written** —
  WI-513 writes the first outlook section.

- **2026-08-30** — **WI-439: its central assumption is contradicted, with a
  captured `.trx`.** Chased down while finishing WI-503, because the flake
  attached itself to the new no-JS gate test twice and "it is probably the
  known flake" is exactly the assumption this file has been burned by before.
  Reproduced at **3 failures in ~16 full-suite runs** (~1 in 5) and captured
  the message rather than inferring it: `EnsureServer()` → `CreateClient()` →
  *"The server has not been started"*, thrown from `InitializeAsync`, no axe
  rule involved. **But 9 of the other 10 `A11ySmokeTests` passed in that same
  run**, and they share one `IClassFixture` factory instance — so they called
  `EnsureServer()` on the *same* object either side of the failure. WI-439 says
  "the factory poisons itself", and therefore that a plain retry "fixes
  nothing" and the fix must dispose and rebuild. **A permanently poisoned
  factory would have failed all ten.** The bad state is transient, so the cheap
  fix may be the right one. Recorded in the ticket; not fixed here.

- **2026-08-30** — **WI-502 done — the tumor-guide editorial standard is
  written down, at `docs/content-pipeline.md` §12.** Appended as §12 rather
  than inserted near the curated-content rules, because §2/§3/§4/§5/§6/§9/§10
  are cited by number from CLAUDE.md, the backlog and the startup references;
  renumbering would have broken every one of them silently.
  Covers source precedence, the 12-item shared contract, the 17-section order
  with the evidence for each position, the R1/R2/R3 number rulings,
  prognosis-without-figures plus the reader-choice gate, and the writing rules
  specific to these pages.
  **Two things came out of reading the NCCN guideline instead of the research
  summary of it.** (1) **It is CNS5-aligned**, verified against the document:
  Arabic grades throughout, `IDH-mutant`, and neither "anaplastic astrocytoma"
  nor "oligoastrocytoma" appears anywhere in 76 pages. That gives the
  precedence rule a clean shape — **NCCN governs naming and grading, NCI
  patient PDQ is framing-only** — and a page that cites NCI for a tumor name is
  now documented as a defect. (2) **It is not "licensing-clean", which is what
  the research reports call it three times.** Its copyright page forbids
  reproducing text or illustrations in any form without written permission. The
  safe reading is also how the site already works: read for facts, write every
  sentence ourselves, cite with a URL, never copy one. §12.1 states it, beside
  the existing NCI-images and AHFS rules from PLAN.md §5. **Worth a line in
  WI-433** (the lawyer questions) if that ever gets picked up.
  Also merged **PR #68** (WI-501 + the WI-504 record) into `develop` first, so
  the block mechanism is in before anything is written on top of it.

- **2026-08-30** — **WI-504 done — Wave 1 is UNBLOCKED.** Dan fetched the
  403-blocked sources by hand into `.claude/work_files/research/sources/`
  (git-ignored; third-party documents). Got: the **NCCN Guidelines for
  Patients: Brain Cancer — Glioma 2024** (~76 pages — the blocker, and the
  model for the "questions to ask" section every P5 page ends with);
  **Kurokawa et al. 2022 "Major Changes in 2021 WHO Classification of CNS
  Tumors"**; **Johns Hopkins "Understanding My Report"**; and **The Brain
  Tumour Charity "What causes brain tumours?"**. ABTA deliberately dropped —
  pre-CNS5 naming, already barred by §4 of the shared contract from governing
  naming or grading, and NCCN covers the rest better.
  **Two corrections to my own instructions, both of which cost Dan time.**
  (1) I labelled RadioGraphics `rg.210236` "the RSNA fMRI review" and told him
  to skip it if it was paywalled. It is the **CNS5 changes paper** — the single
  best crosswalk source in the set, and exactly what WI-501's `[CROSSWALK]`
  block is for. `tests-library.md` §13 uses that wrong label for a different,
  un-URL'd article; `glioma-family.md` files it correctly. He saved it anyway.
  (2) I gave him a dead URL for the causes page and a table whose `.pdf`
  filenames read as things to download — ABTA and BTC publish no PDFs, the
  pages have to be printed to one. Fixed both in the checklist.
  **Worth knowing for Wave 1: all four extract with `pdftotext`** (already at
  `/mingw64/bin/pdftotext`) — `pdftotext -f 1 -l 20 <file> -`. The `Read` tool
  cannot open them; it needs poppler's `pdftoppm`, which is not installed here.

- **2026-08-30** — **WI-501 done — curated pages can include shared blocks, so
  the retired-name crosswalk lives in one file instead of 24.** P5 Wave 0; a
  code item, no tumor content written. A directive is a line whose entire
  content is `[BLOCK-NAME]`, resolved from `Content/blocks/` before Markdig
  parses, which is what makes block text ordinary page text everywhere
  downstream — glossary tooltips mark it, site search finds it, and ContentCheck
  grades the **composed** page rather than a fragment that can pass alone while
  the assembled page fails.
  **A block may carry `sources` front matter, which merges into every including
  page.** Without that, the block's citations get copy-pasted into 24 files and
  the drift problem this item exists to kill just moves to the source list.
  **`Content/blocks/` ships EMPTY, deliberately.** WI-513 writes the crosswalk
  together with the page that includes it — writing it now would either fire the
  orphan-block warning on `develop` from day one (training everyone to ignore
  the gate) or force a Wave 1 content edit into a Wave 0 item.
  **Review caught two blockers I would have shipped, both proven by execution.**
  (1) **CRLF silently dropped every directive.** .NET's multiline `$` does not
  match before `\r`, and `core.autocrlf=true` on this repo, so a fresh clone
  hands the parser CRLF files and `[CROSSWALK]` renders as literal bracket text
  — no exception, no missing-block failure, and ContentCheck grades the bracket
  text as prose and passes. CI is Linux/LF, so **CI would have stayed green
  while the site was wrong**, the exact inversion of the test-locally rule.
  Every test I had written used `\n`. Fixed by scanning line by line instead of
  one regex over the document. (2) **One malformed block 500'd the whole site
  and silently emptied search** — `Load` threw for any bad block, taking down
  `/about` and `/privacy`, which include nothing, and `SearchPages` swallows
  `FormatException` per file so search returned zero results site-wide with no
  error. Blocks now carry their parse errors: only pages that include a broken
  block fail, and they fail by name.
  Also from review: blocks spliced without blank lines fused into the
  neighbouring paragraph, which turns the crosswalk **table** into pipe-mangled
  prose with no error; directives expanded inside fenced code blocks; URL-less
  print sources were silently discarded on a site whose rule is "every claim
  must trace". Added a Fail for a mistyped lowercase `[crosswalk]` (it renders
  literally, so the build should say so), a composed-size cap, and a regression
  test asserting **every shipped page renders byte-identically** with blocks
  available — the general property, rather than three hand-picked strings.
  Verified end-to-end against the running app, not only in tests: composition
  from the real content root, a block edited **while the app was running**
  changing the page, the missing-block failure, and CRLF files composing
  correctly. 875 tests (842 before), ContentCheck 50/0.
  **Not fixed here: WI-435** (ContentCheck swallows `--nologo` as its positional
  pages root). Separate ticket; CI invokes with no arguments.

- **2026-08-30** — **Phase P5 filed: 57 tickets (WI-501…WI-557), tumor guides.**
  Dan's ask: for every tumor type, everything a newly diagnosed person should
  know — what it is, how it starts, what it does to the brain — plus the
  treatments and **the tests you have to get through before each one**. Five
  parallel deep-research tracks ran first; **the reports are committed at
  `docs/research/tumor-guides/`** (moved out of `.claude/work_files/`, which is
  git-ignored — a backlog referencing scratch files is useless to a future
  session). **Read `SYNTHESIS.md` before touching any P5 item.**
  **The finding that justifies the phase:** the readability gap is measured and
  published. Across 91 US brain tumor centres and 8 patient organizations, mean
  Flesch-Kincaid grade level is **11**; under 10% of centre sites reach 8th
  grade and **no patient organization does**. A library that actually holds 6.0
  would be, on the published record, the first — the same shape of uncontested
  niche as P4's daily research feed.
  **Architecture: three layers.** 24 tumor hubs (18 deepened in place, same
  URLs; 6 new), a 17-page treatment library, a 12-page tests library. Writing
  treatments into each tumor page instead would be 24× the work and 24 places
  to fix an error.
  **Dan's calls, recorded in the phase header so they are not re-litigated per
  page:** prognosis concepts explained but **no figures published** (a
  deliberate change from `/tumors`' current line, and it comes with a mechanism
  — a reader-choice gate, WI-503, because the evidence says not everyone wants
  to know); drugs named with side effects but no doses or cycle schedules;
  orienting durations IN but prescriptive ones out; procedural risk percentages
  qualitative (awake-craniotomy seizure is reported from 2.9% to 54% — the
  spread is too wide to state a number honestly); and direction-only for the two
  numbers most likely to mislead.
  **Two operational findings worth knowing before drafting.** (1) **NCI's patient
  PDQ is pre-CNS5** — it still says "anaplastic astrocytoma", "oligoastrocytoma"
  and "hemangiopericytoma", uses Roman numerals, and predates both
  ASCO-SNO-ASTRO 2022 and vorasidenib. It is the source we would naturally lean
  on hardest, so the phase encodes a source-precedence rule: NCI for framing,
  never for naming or grading. (2) **The best available source is 403-blocked to
  automated fetching** — three tracks independently named NCCN Guidelines for
  Patients: Gliomas as the best CNS5-aligned, patient-level, licensing-clean
  source, and none could reach it. **WI-504 is a `[user]` item and it blocks
  Wave 1.**
  **Wave 1 deliberately stops** after one tumor hub (WI-513, low-grade glioma —
  chosen because the short version already exists to compare against) for a
  local review, before the template is copied 23 times. Standing rule since
  2026-08-29.
  **Housekeeping done at the same time:** **WI-444** closed as absorbed into
  WI-508/509 (the pathology report and the molecular results are the same
  physical document, and CNS5 makes their integration mandatory); **WI-451**
  re-scoped, since WI-521 now owns follow-up scans and scanxiety; **WI-446**
  strengthened with the caregiver evidence and left carrying one open question
  Dan should answer *before* WI-513 sets the template — do tumor hubs each carry
  a caregiver section, or does it all live in WI-446? **WI-450** re-pointed at
  WI-507/508. And **P2c and P2d are marked superseded** — they were reserved for
  exactly this work.
  **WI-501 is load-bearing and is a code item, not writing:** curated content is
  flat Markdown with no include mechanism, so without it the retired-name
  crosswalk gets copy-pasted into 24 files, and that is the content most likely
  to need correcting later.
  **Same day, caregiver question answered — and it grew the phase to 59.** Dan:
  make the caregiver section prevalent, because surgery is followed by a lot of
  aftercare and because someone living with a person who has a tumor needs to
  know what they will have to deal with. His example was a partner with a tumor
  who has seizures. So every tumor hub carries a real caregiver section
  (contract item 11), treatment pages with meaningful aftercare carry one too
  (item 12, WI-510 heaviest — a caregiver is discharged into wound care and
  medicines with no training), **WI-558** sets the standard and the shared block,
  and **WI-559** writes "what to do when someone has a seizure". That last one is
  the find: seizures are the commonest way a brain tumor announces itself and
  the symptom people live with for years, and **the site had nothing on what to
  actually do during one.** It is the one page where a reader may be acting
  rather than reading, so format is a safety feature — scannable in a panic,
  legible on a fridge, never behind a disclosure. Both land before WI-513, since
  the template proof has to prove them. **WI-446 re-scoped** from "the only place
  caregiver content lives" to "the fuller path all those sections link into".

- **2026-08-16** — **The deploy window, measured at last — and my earlier
  reassurance about it was wrong.** WI-431's hardened smoke check caught the
  window on its first live run (the WI-412 release) and produced the evidence
  eight earlier sightings never did: **two minutes**, not one — 22:18:21 to
  22:20:19, nine failed rounds — with `/research`, `/trials`, `/search` and
  **`/get-help-now`** all 500 while `/` answered 200 throughout. The deploy
  still went green, correctly: it waited, confirmed health, then passed.
  **The bodies were empty**, which corrects what this file said on 2026-08-15.
  I recorded then that a visitor in the window still sees the calm error page
  with the helpline band and the CareLine number. That is false. An empty 500
  means the request never reaches the application, so the exception handler
  never runs and nothing of ours renders — the visitor gets the browser's own
  error screen, on the route a distressed reader is most likely to want. It also
  rules out an application exception: this is start-up or swap behaviour at the
  platform, not a bug in the code.
  **Recommendation recorded in WI-431b, and deliberately not acted on:** deploy
  at quiet hours and batch releases (free, and free of consequence while
  unlaunched), and collect two or three more measurements — the check now logs
  them automatically — before spending on Standard tier. The previous release
  had no window at all, so one sample of two minutes is not a trend, and buying
  a tier on it would be guessing with money.

- **2026-08-29** — **WI-462 — country counts follow the other filters, live.**
  Dan spotted that the numbers beside each country did not change when he
  picked a tumor type or stage. Real bug: `AvailableCountriesAsync` was a
  separate query that knew only about `includeClosed`, so the menu promised 27
  and the list delivered 4. **A count that disagrees with the list it produces
  is worse than no count.**
  He offered "hide them or update them" and preferred updating. Taken — but the
  fix was not to patch the second query. **Both the counts and the list now
  build from one shared `BuildFilter`**, so they cannot drift apart again by
  construction, and the test asserts the AGREEMENT across four filter
  combinations rather than fixed numbers. A fixed number would pass while the
  two silently diverged.
  Two calls: the counts ignore the COUNTRY selection (otherwise circular —
  picking Finland would zero every other country and you could never widen from
  the menu), and the live refresh fires on tumor type / stage / include-closed
  but **not** the country checkboxes, which would close the picker under your
  hand mid-selection. Countries apply on the button, which is also the no-JS
  path.
  Verified on the real cache: all → 311 (US 210); glioblastoma → 88 (US 64);
  glioblastoma + Phase 3 → 5 (US 4); ticking US then listed exactly 4.
  842 tests, ContentCheck 50/0. Reviewed locally before deploying.
  Also: WI-439's Kestrel flake fired 1 run in 5 here, and this time I captured
  the message instead of assuming — "The Kestrel test host did not start",
  not an axe violation.

- **2026-08-29** — **WI-458…WI-461 — a review round on trials and research, all
  reviewed locally before deploying.** Dan asked for that process change after
  four releases went out in one session on my screenshots alone: *"let me test
  locally before we deploy… you're a big fuck-up today."* Fair. Written to
  memory as a standing rule for reader-facing work.
  **WI-458** the country picker: always starts closed (it auto-opened after
  every search — my call in WI-457, and the reasoning did not survive contact
  with using it), moved beside the other filters, button below and sized to its
  words, labels above controls, "Select" then up to three countries then "...".
  **The browse form had no htmx at all**, which is why submitting jumped to the
  top; it now swaps in place, and the pager finally gets the `HxTarget` the
  partial was built to take.
  **WI-459** trials render as cards like the research feed, minus the photo.
  Found while building it: in the LOCAL dev database zero of 518 trials had a
  plain-language summary. **Corrected after deploying: on PRODUCTION 54 of 60
  do** — the fallback fires on ~10%, not 100%. I generalised one database to
  the system. The description falls back to the registry's own text, labelled
  "From the trial team:" and cut at a sentence end — never mid-sentence, since
  truncating "...did not improve survival" halfway is how a card says the
  opposite of the study.
  **WI-460** trials removed from `/research` and its filter; old
  `?kind=trial_update` links fall back to the whole feed. Home still shows them
  — asked, and Dan said fine. Research filters restacked to match trials.
  **WI-461** each of the two trial-finding methods gets its own tinted panel
  holding only heading + controls, with the results below on the page
  background. Headings now "Trials near you" / "Trials by countries"; button
  "Search trials".
  **Two corrections to my own work this round, both from measuring instead of
  trusting myself.** I "fixed" the scroll jump with `show:none` and wrote a
  comment claiming that was the fix — it was not; **my Playwright test was
  causing the jump** by scrolling to an element before clicking it. And I
  thought the country control looked washed out beside the selects; computed
  styles said identical. I was looking at a stale screenshot.
  **And a real bug I had twice waved away as "intermittent".**
  `TheAdminHealthPageShowsTheCounts` seeded a `/seen-<guid>` row with 42 views
  and never cleaned up. The admin page lists the **top 12** by
  `views DESC, path` — after 45 accumulated runs there were 45 rows all tied at
  42, so the tiebreaker was the random GUID and whether this run's row made the
  cut was a coin flip that got worse every run. It read as a mystery flake; it
  was the test poisoning its own database. Purged, cleanup added at both ends,
  and a second littering test fixed too. **Seven consecutive green suites, zero
  rows leaked.** The lesson: "passes in isolation" is not a diagnosis.
  840 tests, ContentCheck 50/0.

- **2026-08-29** — **WI-457 done — multi-select countries, and a signpost from
  research to trials.** Dan after using WI-455: he wants China AND Japan AND a
  few others, and wants readers on `/research` told that trials exist.
  Checkbox group in a `<details>` rather than a JS widget or `<select multiple>`
  — the site works with JavaScript off, and a native multi-select is ctrl-click
  on desktop and a cramped scroller on a phone. Same disclosure pattern as the
  mobile nav. Collapsed by default (44 countries would push the trials off
  screen), auto-opened when a filter is active so a shared link explains itself.
  Ordered by trial count. URL key unchanged, so single-country links from
  WI-455 still work — pinned by a test.
  Several countries mean EITHER, not both: a trial has to run somewhere the
  reader can reach, not everywhere they ticked. Verified on the real cache —
  China+Japan returns 38 where the two alone are 32 and 10, so overlapping
  trials are counted once rather than duplicated.
  The `/research` signpost sits ABOVE the filters, not below the feed: nobody
  should read twenty cards before finding out they are on the wrong page for
  "can I join a trial". A test pins that ordering.
  **A test-hygiene bug of mine worth remembering:** the new tests used NCT7778
  IDs while `CleanupAsync` only deletes `NCT7777%`, so rows leaked into the TEST
  database and collided on the second run. Renumbered into the cleaned range.
  Note the tests run against `brainharbor_test`, not the dev `brainharbor` DB —
  I cleaned the wrong database first.
  837 tests, ContentCheck 50/0.

- **2026-08-29** — **WI-455 done — filter trials by country.** 41 countries with
  trial counts, built from the cache so the menu can never offer a dead choice.
  Matches ANY of a trial's sites.
  **The multi-country trap was real, not theoretical.** Of the 20 trials under
  Germany in the live cache, **17 run in more than one country** — the first is
  a 14-country study. Matching only the first location would have hidden or
  mislabelled most of them. Cards say "Runs in N countries" rather than
  implying the filtered one.
  Counts are trials not sites (40 US hospitals is one US trial) and honour the
  closed filter, so a count of 12 never leads to a list of 3. Non-US readers are
  now told the ZIP box is US-only and pointed at the country filter, with the
  honest caveat that it gives no distances — the first time this site has been
  usable outside the US. GIN index (`jsonb_path_ops`) on `locations`, migration
  0009. Verified against the real 8,900-trial cache, not just seeds: menu count
  and filter result agree exactly, 0.02s filtered. 830 tests, ContentCheck 50/0.
  **A bug in a comment I wrote, worth recording because it was about privacy.**
  `PageUrl` claimed it deliberately strips the reader's ZIP from pager links. It
  does not — it is built from `FilterUrl`, which appends one. Corrected to
  describe reality, and the real question named: the handler sets
  `no-store`/`no-referrer` because the URL carries a location, but nothing stops
  a reader SHARING that URL. Not changed, just no longer misdescribed.

- **2026-08-26** — **WI-455 filed: filter trials by country. This is next.**
  Dan asked for country filtering on research AND trials. Checked the data
  first, and the two halves are not the same job:
  - **Trials: already there.** `trials_cache.locations` is jsonb with
    `{facility, city, state, country, lat, lon}` per site — verified against
    real rows. Nothing new to fetch; a filter, a control and an index.
  - **Research: no location data anywhere.** `FetchedItem` carries no
    affiliation at any layer, so it would mean parsing PubMed's
    `AffiliationInfo`, inferring a country from free text, a new column and a
    backfill of 1,000+ items — for a weak payoff, since a paper's country is
    where the authors work, not whether the finding applies to the reader.
  **Dan's call once he saw the cost: trials only.** The research half is
  recorded as decided-against under WI-455 rather than left as an open ticket,
  so it does not get re-proposed.
  Two traps written into the ticket: a trial with sites in eight countries is
  not "a US trial" (match ANY location, and say how many countries it runs in),
  and this must not become "trials near you" for non-US readers — the ZIP box is
  US-only ZCTA centroids, so a reader in Germany filtering to Germany still gets
  no distance and the page must not imply otherwise.

- **2026-08-26** — **Phase P4 filed: 13 new tickets (WI-442…WI-454)** from Dan's
  ask for ways to improve or expand the site, after a web survey of comparable
  sites.
  **The finding that shaped the whole phase: nobody else runs a daily
  plain-language brain tumor research feed.** ABTA, National Brain Tumor Society
  and The Brain Tumour Charity all offer CareLines, support groups, mentor
  matching, financial aid and conferences — real services, none of them this
  one. So the strategy recorded is DEEPER for this audience, not broader across
  cancers.
  Also worth carrying: a 2026 blinded randomised trial found ChatGPT-4o wrote
  Cochrane plain-language summaries at least as well as human researchers
  (PMC12302524) — citable on `/how-we-write`, and it turns "AI writes these"
  from an apology into a claim with evidence behind it. And forum research
  (PMC12119380) documents that CAREGIVERS carry high unmet needs, which is where
  WI-446 comes from.
  **Found while surveying, not asked for: the reader-report loop is broken at
  the last step (WI-442).** A reader types why a summary is wrong,
  `ReportProblemAsync` stores it in `review_events.note`, the item is flagged —
  and the queue never displays the note. On an Auto-publish medical site the
  reader may be the only human who compared the summary against the source, and
  their message goes into a table nobody reads. Hours to fix; my pick for next.
  **WI-443 splits the database-restore rehearsal out of WI-407's checklist**
  because it is the most dangerous line in the backlog: 1,000+ published
  summaries, the audit trail and the view counts, on a shared PG17 server, and a
  restore has never once been performed.
  WI-454 parks the general-cancer sister site WITH its measurements, so the
  research is not redone: all sources already cover every cancer (~4 query
  strings), but volume is 8,331 PubMed records/month vs 470 — ~18× — and the
  space is crowded by NCI, ACS and Cancer Research UK. Better shape if revisited
  is another underserved SPECIFIC cancer.
  Not duplicated as new tickets, since already tracked: **WI-406** (retraction
  check), **WI-434** (glossary), **WI-404/405** (digest).

- **2026-08-23** — **WI-441 — page-open counts, admin-only.** Dan asked whether
  visitors were being tracked. They were not: no analytics package, no App
  Insights linked to the app, only 3 days of App Service HTTP logs nothing
  read. Azure's `Requests` metric is free and already there (93-day retention,
  Portal → App Service → Metrics) but counts every stylesheet, bot and
  smoke-check poll — load, not readers.
  **The constraint:** `/privacy` promised "we do not run analytics and we do
  not build a profile of you", live. And the metric Dan actually asked for —
  new vs repeat — REQUIRES persisting an identifier across days; there is no
  privacy-preserving trick around it, which is why Plausible and Umami
  deliberately cannot do it either. His call: give that up rather than weaken
  the promise.
  Built `page_views (viewed_on, path, views)` and nothing else — no IP, no user
  agent, no session id, no referrer, no sub-day timestamp. Buffered in memory,
  flushed every 60s so a page never waits on a write. Assets, htmx fragments,
  bots, curl, the smoke check, `/admin`, `/api`, redirects and errors are all
  excluded, so the number means something. **The query string is dropped before
  storage** — on `/trials` it can carry a reader's ZIP.
  Privacy copy rewritten and made stronger rather than merely accurate: the new
  bullet states the counts cannot tell a new reader from a returning one and
  that this was chosen. Verified end-to-end on a running site (4 real loads + 5
  pieces of noise → exactly `/`=2, `/research`=1, `/trials`=1), not only in
  tests. 821 tests, ContentCheck 50/0.

- **2026-08-23** — **WI-440 — the site works on a phone now.** Dan asked for a
  hamburger menu on the home page and "things like that", then for the rest of
  the pages. Measured first: no horizontal overflow anywhere and no text under
  the 16px floor, so the body content was already sound — the damage was all in
  chrome and forms. A reader on a 390px phone scrolled past **1107px** before
  reaching `<main>`, more than a full viewport, on every page. Now ~890px, with
  the header down from 245px to 137px.
  Hamburger via `<details>`/`<summary>` (no JavaScript, per the standing
  constraint), icon-only masthead on phones, **"Get Help Now" deliberately kept
  OUTSIDE the menu** at every width — PLAN.md §3 is "always one tap to a human"
  and a disclosure makes it two. That is pinned by a test. Filter forms now
  stack as label-above-control grids; on `/research` the wrapped row had been
  separating every label from its control, which is worse than no label at all.
  Checkboxes went 13px → 22px inside a 44px row.
  **Gap closed in the gate:** every axe scan ran at desktop width, where the
  hamburger is `display: none` — so the whole phone layout, including a menu
  panel that did not previously exist, was outside the accessibility gate.
  Scans now run at 390px, menu closed and open.
  **Three bugs hit, none visible to any test:** a tap-target regression I
  introduced myself (40.5px band links, chasing four pixels); a specificity
  fault where `.site-name img` (0-1-1) outranked `.site-name__lockup` (0-1-0)
  so both logos rendered; and `display: contents` on `<details>` laying the
  desktop nav out 0px wide. The last was fixed by restructuring rather than
  patching — the nav is now a sibling of the toggle. **That is four sessions
  running where the screenshot found what the suite could not.**
  Desktop unchanged; all rules scoped to `max-width: 40rem` except checkbox
  sizing, which was too small everywhere. 806 tests, ContentCheck 50/0.

- **2026-08-22** — **The WI-403 Kestrel flake cost a production deploy; filed as
  WI-439.** The WI-438 release merged to `main`, `build-test` went red on
  `A11ySmokeTests` failing to start its host, and because `build-test` gates the
  `deploy` job **the deploy never ran** — the merge looked done and production
  stayed on the old build with pagination still broken. A re-run of the same
  commit passed and deployed. Not an axe failure and nothing to do with the
  pagination work; it passed on the feature PR and on `develop` first.
  **Lead worth keeping:** the failing test took **1 ms** in CI. A real host
  start takes seconds, so nothing was attempted — `CreateClient()` handed back
  an already-broken cached host. The factory poisons itself after one failure,
  which means a retry loop around `EnsureServer()` would spin on the same dead
  host and fix nothing; a retry has to dispose and rebuild. That is in the
  ticket, because it is the kind of thing that gets "fixed" wrongly in ten
  minutes otherwise.
  Also: **my deploy-window poller missed the window a second time** (8s single
  route vs a ~75s event). Read the smoke-check log for those numbers, not the
  watcher. This release is NOT counted as a fourth measurement — there is no
  clean reading for it.

- **2026-08-21** — **WI-438 — pagination never worked, on `/research` OR
  `/trials`.** Dan: "When I click Show More, it sticks on page 1." Reproduced,
  and it was broader than reported: `?page=N` was ignored on both pages
  (`/trials` served the same first rows on pages 1, 2 and 3 of 16).
  **Root cause: `page` is a RESERVED route-value key in Razor Pages.** Routing
  stores the page path in it, so a handler parameter of that name binds the
  ambient ROUTE value, fails to parse as an `int`, and falls back to the default
  — silently, no exception, no warning. Every other query parameter on the same
  handler bound fine, which is exactly why it read as a paging bug rather than a
  binding one. Fixed with `[FromQuery(Name = "page")]` plus a C# parameter
  deliberately not named `page`.
  **Why no test caught it:** the existing test set the model's state directly
  and asserted the "Show more" URL contained `page=2`. It proved the link was
  BUILT right and never followed it. The replacement requests page 2 for real
  and asserts a disjoint set of items — the only shape that can catch this.
  Replaced "Show more" with a real pager (Back · 1 … 6 7 [8] 9 10 … 16 · Next,
  "Page 8 of 16"), shared by both pages, unit-tested window logic, 1-based URLs,
  out-of-range clamped to the last real page, htmx opt-in so `/trials` can share
  it and just navigate.
  **A third defect, found only by screenshot:** a duplicate `.pager` rule ~600
  lines further down site.css overrode the new component and squeezed the status
  line into a three-line column. Tests green throughout. **That is now three
  times running that the picture caught what the suite could not** (WI-412a's
  orphan page, WI-437's phone-width flip, this). Screenshot anything visual
  before calling it done.
  Scope note: Dan asked about `/research`; `/trials` was fixed too because the
  same defect was proven live there. `/admin/queue` left alone (internal).
  804 tests, ContentCheck 50/0.

- **2026-08-19** — **WI-437 — the journey path replaces BOTH the readiness dial
  and the stage badge.** New design handoff from Dan
  (`design_handoff_brainharbor_journey`). Cards and the item page used to carry
  a 4-mark badge ("how well tested") and a 1-to-10 dial ("how close to a
  patient") in different units; now one four-stage path — Lab cells → Animals →
  Review → Tested in people. **This also closes WI-429.**
  The handoff worried about mapping a 0-10 score onto four stages; that did not
  apply, because `ResearchStage` already WAS the four stages, so the component
  is driven straight off the enum and a content author can never type a number.
  **Dan's calls:** replace both indicators; keep the card photos; re-point the
  `?sort=readiness` sort at the stage ladder ("Furthest along") rather than drop
  it; and — after seeing it live — lay the path OVER the photo where the dial
  sat, prominent, not as a row of dots underneath.
  **Two defects neither markup nor tests could show:**
  (1) *in the handoff itself* — `role="img"` on the `<ol>` orphans every `<li>`;
  axe called it SERIOUS across 28 nodes, and this renders on every card on every
  page, so it would have shipped site-wide. Fixed with `role="presentation"`.
  **Worth telling the designer.**
  (2) *mine* — at phone width the overlaid path flipped vertical and swallowed
  the whole photo; my override tied on specificity and lost on source order.
  Every test was green and the structure was correct; only a screenshot showed
  it. **Same lesson WI-412a taught in a different costume: a test that checks
  structure cannot see what a page looks like.** Screenshot anything visual.
  Contrast computed by hand rather than trusted to axe (which cannot reason
  about a translucent plate over an arbitrary photo): 11.8:1 to 15.1:1 for the
  current label, 9.2:1 to 11.4:1 for the rest — AAA either way.
  **`readiness_score` is untouched** in the DB, pipeline, sync contract and
  review queue, so this is reversible without a migration; two tests guard
  against the number reappearing on a reader page. The stage badge survives for
  the admin queue and style guide only. 781 tests, ContentCheck 49/0.

- **2026-08-19** — **Deploy window, third measurement: 84 seconds** (WI-436
  release, PR #49). 20:49:10 → 20:50:34 by an independent poll of `/start`; the
  smoke check agrees (six failing rounds, first clean at 20:50:40, second
  consecutive at 20:50:52). Same signature a third time: inner pages 500 while
  `/` stays up.
  **Series: 2m00s, 1m23s, 1m24s. WI-431b's precondition is now met** — three
  measurements, and the last two are within a second of each other. The shape is
  settled: roughly 85 seconds of 500s on every page except `/`, every deploy.
  **This is Dan's call now, and it is a real one:** ~85s of hard 500s on
  `/get-help-now` is the part that matters, because that is the page a
  distressed reader is pointed to from the band on every page and from `/start`.
  While unlaunched it costs nothing. At soft launch (WI-408) it is a defect.
  Options unchanged: deploy at quiet hours + batch releases (free), or Standard
  tier for slot swaps (money, removes it).
  **Also this release: CI hung for 35 minutes** on `Install Playwright browsers`
  (`playwright.ps1 install --with-deps chromium` — the `--with-deps` half shells
  out to apt-get). Cancelled and re-ran; both passed. First occurrence, so not
  filed yet, but note the step has no `timeout-minutes`, so a hang there blocks
  until the job's 6-hour default and a stalled pipeline looks exactly like a slow
  one. If it recurs, that is the fix.

- **2026-08-19** — **WI-436 — /start rewritten from Dan's copy.** Dan supplied
  new text for the Just Diagnosed page; implemented at reading grade 4.2 (gate
  is 6.0). `/start` also added to `sitemap.xml` — it was reachable from the home
  page door but invisible to search engines, the WI-412a orphan pointed the
  other way, on the page a newly diagnosed person is most likely to search for.
  **Four deviations from the supplied copy, all told to Dan:** the closing "not
  medical advice" paragraph was left out (it is verbatim what
  `disclaimers: [medical]` already renders — it would have printed twice); the
  CareLine's weekday hours were KEPT although his copy dropped them (Mon–Fri
  8:30–5:00 CT verified — without them a reader calls at 9pm Saturday and gets
  silence); the "find the nearest emergency room" link had no target in the copy
  so it points at a Google Maps search; and his opening paragraph became the
  `description` front matter, which renders as the lead under the H1.
  **New ticket WI-435, and a correction to my own reporting:** ContentCheck
  takes the pages root as a positional argument, so `--nologo` was swallowed as
  that path — the tool then printed `WARN pages root MISSING`, graded ZERO
  Markdown pages, and exited 0 saying "passed (22 checks, 0 failures)". I had
  quoted that partial number as verification on PR #46. CI invokes it correctly
  (48 checks) so nothing shipped ungated, but a gate that reports success after
  checking nothing manufactures confidence, which is worse than no gate on the
  check standing between an AI-drafted tumor page and a frightened reader.
  Also noted for later: `FrontMatter.Description` renders visibly but is not
  wired to `<meta name="description">`, so every curated page shares the
  site-wide default.

- **2026-08-16** — **Deploy window, second measurement: 83 seconds** (WI-412a
  release, PR #47). 23:54:01 → 23:55:24 by an independent poll of `/tumors`
  running alongside the workflow; the smoke check agrees (six failing rounds,
  last 500 at 23:55:11, first clean at 23:55:26, then the second consecutive
  clean pass at 23:55:38 → "site healthy after deploy"). Same signature as the
  first: `/research`, `/trials`, `/search`, `/get-help-now` all 500 while `/`
  stayed up, and **the bodies were empty again** — the request is not reaching
  the app, so this is platform start-up/swap behaviour, confirmed twice now and
  not an application exception.
  **Series so far: 2m00s, 1m23s.** Two samples, same shape, roughly a minute and
  a half. That is enough to call it a consistent behaviour but WI-431b's advice
  stands unchanged — one more measurement before spending on Standard tier, and
  the cheap mitigations (deploy at quiet hours, batch releases) cost nothing
  while the site is unlaunched.
  Worth noting the check earned its keep on its second run: it measured the
  window rather than passing straight through it, which is exactly what WI-431
  was for.

- **2026-08-16** — **WI-412a — /tumors shipped ORPHANED; navigation added.**
  Dan: "There's no navigation to get to the tumors page." He was right, and the
  gap was wider than the nav: no header link, no footer link, `/tumors` missing
  from `sitemap.xml`, and no "What is this?" link from the `/research` tumor
  filter — which was WI-412's own acceptance criterion. The page worked
  perfectly and nothing on the site pointed at it, so only a typed URL reached
  it. Fixed all four. **Why the tests missed it:** the existing link check walks
  the links that exist and proves they do not 404 — it is structurally blind to
  a page no link points at. The new test asserts the general property instead of
  this one instance: every path `sitemap.xml` advertises must be reachable from
  the home page. **Second bug, caught only by looking at the rendered page:** the
  "What is this?" link first shipped as a multi-line Razor implicit expression,
  which terminates at the newline — it rendered "What is
  System.Collections.Generic.List`1[BrainHarbor.Web.Content.TumorType]" while the
  test, which asserted only the `href`, stayed green. The test now asserts the
  whole anchor including its visible text. Lesson worth keeping: a test that
  checks the attribute a human never reads will not notice the text a human
  only ever reads.

- **2026-08-16** — **WI-412 done — /tumors, "what is this?" for 18 of 24 types.**
  Dan asked for all of it in one go, shipped live, and said he would review it
  there. The index is built from `taxonomy.yml` — the same file the research
  filter reads — so the list and the descriptions cannot drift apart.
  **Deviation, deliberate:** each type gets its own page (`/tumors/glioblastoma`)
  rather than only an anchor on one page. People arrive from a search engine
  having just been handed a word by a doctor; a page about their diagnosis beats
  an anchor part-way down a list. Anchors still exist for `/research` to link.
  Descriptions live in `Content/pages/tumors/*.md`, NOT in `taxonomy.yml` —
  that file is rendered into the classifier prompt on every call, so prose there
  would cost tokens per item and escape the reading gate. As curated pages they
  inherit the 6.0 CI gate, glossary tooltips, the medical disclaimer, `sources`
  front matter and site search.
  **Content discipline, because this is the riskiest writing on the site:**
  descriptions only. No survival figures, no prognosis, no treatment
  recommendations — those vary per person and are where a wrong word does real
  harm. WHO CNS5 naming held throughout (grade 4 ≠ glioblastoma; DIPG is the
  pontine subset; spinal cord tumor is not a brain tumor and has its own
  heading, pinned by a test). Every page ends with questions to ask a care team.
  Grades 2.4-5.4. 777 tests.
  Fixed on the way: `ShellPagesTests` pinned the TOTAL graded-page count at 6,
  so it failed the moment curated content was added even though every page
  passed — now it names the six shell pages and asserts every `.md` under the
  pages root is graded, so a new folder cannot escape the gate.
  **Two things for Dan:** six rare types still say "We are still writing this
  one" (honest, and they link to the feed for that type), and these pages were
  drafted by AI yet read as the site's own writing — `/how-we-write` covers the
  FEED pipeline and says nothing about curated pages. Worth a decision.

- **2026-08-15** — **WI-431 done — the deploy smoke check now asks whether the
  SITE is healthy, not whether something answered.** Eight deploys today each
  served 500s on `/research`, `/get-help-now`, `/trials` and `/search` for about
  a minute while `/` returned 200 throughout — and the check went green every
  time, because it hit only `/`, on the azurewebsites hostname, and exited on the
  FIRST 200 (which during App Service's old/new overlap can come from the
  instance being replaced). Now: all five main routes, on `brainharbor.org`, and
  **two consecutive clean passes** so one lucky hit cannot pass it.
  **It also prints the failing response body**, which matters as much as the
  gating: eight sightings produced no evidence of the CAUSE because nobody ever
  captured one. The next occurrence will. Until then, "it is the app restarting"
  is inference, not diagnosis — do not treat it as known.
  Both paths verified locally against the live site before shipping (healthy →
  exit 0 after two passes; bad host → body captured, counter reset, exit 1), and
  the release that carries this change is itself the first real exercise of it.
  **CORRECTED 2026-08-16 — this was wrong.** The entry originally said a visitor
  caught in the window still gets the calm custom error page with the helpline
  band and CareLine number on screen. The first real capture shows the 500
  bodies are EMPTY: the request never reaches the application, so ASP.NET's
  exception handler never runs and no BrainHarbor page renders. The visitor sees
  the browser's own error screen. See WI-431b for the measurement.
  Filed **WI-431b** (`[user]`): whether to buy Standard tier for slot swaps —
  explicitly NOT to be decided until the body capture identifies the cause,
  since a start-up or migration-lock cause may be free to fix in code.

- **2026-08-15** — **WI-432 done — /terms was still claiming a human reads every
  summary.** Dan asked whether the site needs legal cover; looking for it turned
  up a real defect rather than a missing paragraph. `/terms` said "We check each
  summary before it goes up", which reads as human review. Publishing mode is
  Auto and nobody reads each one; every other page was scrubbed of that
  implication on 2026-07-31 and this page was missed — the one page most likely
  to be read as a promise. Now: AI writes them, they must pass automatic checks,
  and a person does not read every one.
  Also added: the site says plainly it is for learning and for asking a care
  team better questions, and a "We are not part of these groups" section for
  ABTA, NCI and ClinicalTrials.gov, since the site points at all three and none
  of them checked what we wrote. Grade 4.3, 773 tests.
  **Deliberately NOT written** (Dan: "don't include anything you're not sure
  about"): warranty disclaimer, limitation of liability, governing law, and the
  operating-entity question. Filed as **WI-433**, a `[user]` item, with the
  context a lawyer would need. Also recorded there: publicly accessible is not
  public domain — PubMed abstracts often carry publisher copyright, which is why
  the pipeline summarizes and links and keeps raw abstracts admin-only.

- **2026-08-15** — **WI-429 + WI-428 done — the homepage card gets its readiness
  dial back, and the item page finishes the handoff.**
  **WI-429 is Dan's call and it reverses the handoff.** That design specified a
  plain card (badge, title, hook, meta) with no photo and no readiness dial.
  Dan, after seeing both live: the `/research` card is better and the homepage
  should match it. The dial is the reason — the badge says how well TESTED a
  finding is, the dial says how close it is to something a patient can actually
  get, and the homepage was answering only the first question. The `PlainCard`
  flag is gone; ONE card renders on both pages, so they cannot drift again.
  Recorded as deviation 3 in `docs/design/README.md`.
  **WI-428 turned out to be nearly done already.** Every item-page style the new
  handoff specifies (`.means-block` + head/icon, `.original-title`, `.term`,
  `.provenance`, `.ai-note`) already matched value for value — the page was
  built from the previous handoff and this one barely changed it. The one real
  gap: the badge now repeats under "How early is this?", beside the words that
  explain it, because by then the reader has passed the whole summary and
  scrolling back up to count marks is how you lose someone. Kept against the
  handoff: the readiness callout (Dan wants readiness more prominent, not less)
  and "What this means, and what it doesn't" (the handoff writes that heading
  with an em dash, which the site's own copy rule forbids). 773 tests.

- **2026-08-15** — **WI-430 done — a suggestions address, with a steer attached.**
  Dan wants `support@brainharbor.org` on the site before he shares it publicly,
  for ideas on making the site better rather than for support. It now appears in
  three places: a box at the foot of the home page, a line in the footer of
  every page, and a section on `/about`; `/privacy` says what happens to an
  email that arrives.
  **The safety-relevant part is the copy, not the address.** The mailbox is
  called support@ but is not a support line, and "support" is precisely what a
  frightened reader would write to at 2am expecting help — then wait days for an
  answer. So every prominent placement states that the inbox is not a way to get
  medical or urgent help and prints the ABTA CareLine number inline, not as a
  link away. A test pins that steer, not just the address, because the address
  without the steer is the part that could hurt someone.
  Reading grades: home 3.3, about 3.4, privacy 3.8. 773 tests.

- **2026-08-15** — **Homepage redesign live — "Harbor Banner"** (WI-428a; Dan
  brought a finished handoff back from Claude Design, reviewed it locally and
  approved). The shape is the point: a hero paragraph, three doors and a
  full-width AI panel used to stack up and push the first real update about a
  screen and a half down. Now one navy hero band (lockup as the `h1`, watermark,
  wave edge), **two** doors instead of three ("browse all research" cut, because
  the feed underneath already does that job; the crisis door stays), then
  updates — **8 cards**, not 4 — with "See all" as a filled button in the
  section head and a large outlined one beneath. **Absorbs WI-425.**
  **Evidence badge is 4 marks now, on a ladder with no gaps.** The old one ran
  5, 4, 2, 1 with nothing at 3, so the step from "review of existing research"
  to "animal study" was twice the step anywhere else for no reason a reader
  could infer; a test now fails if a gap reappears. Ten steps (the readiness
  dial this replaces on the feed) is a finer distinction than anyone can feel,
  and a fraction invites "my treatment is 60% done".
  **Two deliberate deviations from the handoff, both safety copy, both pinned
  by tests.** The handoff gives the hero no copy at all and drops "a person does
  not check every one" from the notice. Kept both: the AI admission leads in the
  band (a reader must not get through eight summaries before learning who wrote
  them) and the non-review sentence went into the bottom notice, because "checks
  run before it publishes" does not tell anyone that no person read theirs.
  New `Banner` layout section so a full-bleed band can sit before `main`.
  Reading grade **3.7** (was 4.1), axe clean, 772 tests.
  **Left deliberately unfinished:** the research item page restyle (**WI-428**)
  — the badge change already reaches it, so it is consistent, just not restyled.
  And the homepage's plain card (no photo, no readiness dial) is NOT yet applied
  to `/research`, which still has both plus a "Most ready to use" sort that
  would otherwise sort by an invisible number (**WI-429** — Dan's call).
  `docs/design/README.md` records which handoff governs what.

- **2026-08-14** — **WI-427 done — negation never worked for contractions, and
  that was the bigger half of the queue.** Dan shipped WI-426, reloaded, and
  still saw hype flags. Cause: negation was a word LIST matched against
  `[A-Za-z]+` tokens, which strip the apostrophe — so "doesn't" became
  "doesn" + "t" and the list's `doesn't` / `isn't` / `n't` entries could never
  match anything at all. **Every contraction read as un-negated**, and the block
  those sentences live in is CALLED "what this doesn't mean", so it is about the
  commonest phrasing in the corpus. It hit `cure` as well, which means this had
  been silently mis-flagging since WI-401 — WI-426 fixed which phrases got the
  negation test, this fixed whether the test ever worked.
  Measured before: `doesn't`, `isn't`, `won't`, `can't` all flagged. After: all
  clean, while "this is an **important** breakthrough" and "Doctors call this a
  breakthrough" stay flagged, which is correct.
  Fixed with a negation REGEX rather than a token list, since the apostrophe is
  part of the word; straight and curly both accepted. **The `n't` branch
  requires the apostrophe** — a bare `\w+nt` matches "importa-n-t" and would
  have quietly excused hype sitting right after it. Both directions pinned by
  tests. 771 tests.

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
