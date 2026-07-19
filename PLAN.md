# BrainHarbor — Master Plan

*Daily brain tumor research and news, translated into plain language — plus a plain-language hub of tumor, treatment, and benefits information.*
Status: **Planning fleshed out (aggregation-first pivot 2026-07-12) → ready to scaffold (M0).** Last updated 2026-07-12.

**Companion docs** (the detail lives there; this file is the map):

| Doc | Covers |
|---|---|
| [docs/architecture.md](docs/architecture.md) | Stack (Razor Pages + htmx, .NET 10), the aggregation pipeline, hosting, ops, cost |
| [docs/sitemap.md](docs/sitemap.md) | Information architecture, URL structure, writing budget |
| [docs/content-pipeline.md](docs/content-pipeline.md) | Curated-page workflow AND the automated summarization pipeline + its guardrails |
| [docs/data-model.md](docs/data-model.md) | Postgres schema (items, subscribers, trials, stories), PII handling |
| [docs/roadmap.md](docs/roadmap.md) | Milestones M0–M4 + Phases 2–3, recurring maintenance, success metrics |
| [docs/backlog.md](docs/backlog.md) | **The work-item backlog** — milestones decomposed into one-evening items (WI-###); the project's tracker (no GitHub issues) |
| [PROGRESS.md](PROGRESS.md) | **Live state** — current/next item, blockers, log; updated every work session |

---

## 1. Decisions locked so far

| Question | Decision |
|---|---|
| **Core product** | **Aggregation-first** *(pivoted 2026-07-12)*: auto-aggregated latest research / trials / news with **AI-generated plain-language summaries** for patients & caregivers. The curated static hub (newly diagnosed, benefits, tumor types) still gets built — as Phase 2, written once and rarely updated |
| Feed audience & stance | **Patients & caregivers first**: filter hard (human-relevant results on the front page; animal/cell work behind an "early-stage" toggle), translate jargon, actively anti-hype |
| v1 scope | Feed (browse + filter) + item permalink pages + **plain-language summaries** + **clinical trial finder** + **weekly email digest**. Static pages in v1: only a thin shell (about, how-we-write, interim start-here, get-help-now, glossary seed) |
| Community | **Moderated story submissions** — Phase 3. No public user accounts |
| Medical scope | **All brain tumors**, glioma / low-grade glioma deepest first (that's the first real user) |
| Extra pillar | **Benefits & disability navigation** — first static-hub section built in Phase 2 (strongest static differentiator + long-tail SEO wedge) |
| Tech | **Razor Pages + htmx (Htmx.Net) on .NET 10 LTS**, Dapper, DbUp, PostgreSQL, Azure. *(Changed 2026-07-12 from Blazor static SSR + htmx — Htmxor is unmaintained; Razor Pages + htmx is the mature pairing and exactly the upcoming work stack. Rationale: [docs/architecture.md](docs/architecture.md) §1)* |
| Database | **PostgreSQL from day one** — aggregation is the product |
| Pipeline topology | **Local-first pipeline** *(decided 2026-07-18)*: a console app on Dan's PC (daily Windows scheduled task, self-healing catch-up) fetches sources, processes them, and uploads to the site over a **secured sync API** — no direct DB connection from the PC, stateless local app, idempotent upserts. The website is a read-mostly presentation layer with **no server-side background jobs (no Hangfire)** |
| Summarization | **Claude Code CLI locally** (`claude -p`, JSON output) under the existing subscription — **no Anthropic API key**. Fixed template with mandatory "what this means — and doesn't mean" block, closed-taxonomy classification, numeral post-checks, golden-set regression tests. Items upload as *pending*; **a human approves every item in the site's admin queue before it publishes**. Full contract: [docs/content-pipeline.md](docs/content-pipeline.md) §9–11 |
| Email | **Weekly** digest via a hosted ESP (Buttondown/Kit) — double opt-in, human-reviewed before send. Daily option reconsidered post-launch if volume warrants |
| Domain / entity | **brainharbor.org** (changed from .net 2026-07-18; .com unavailable — and .org reads right for a health resource). Personal project — no LLC/nonprofit until money is involved |
| Repo & dev environment | **Private GitHub repo** (badsonstudios). **Local-first development**: Docker Postgres + `dotnet run`; Azure provisioned as part of the launch milestone (M4), not M0 — $0 hosting until launch-ready |
| Content authorship (static pages) | **Solo (Dan), AI-assisted drafting** with per-claim source verification; friend (low-grade glioma patient) reviews benefits + glioma pages; clinician review recruited later if possible |
| Cadence | **Nights & weekends, no deadline** — v1 effort is pipeline engineering + prompt iteration, not prose |

---

## 2. Competitive landscape — the gaps we exploit

I profiled ABTA, National Brain Tumor Society (NBTS), NCI/cancer.gov, Mayo, Cleveland Clinic, MD Anderson, American Cancer Society, EndBrainCancer, Glioblastoma Foundation, CancerCare, Brain Tumor Network, and Musella/VirtualTrials.

**What they collectively do well (table stakes we must match):**
- Tumor-type guides, a clinical-trial finder (best: NCI + MD Anderson, both pulling ClinicalTrials.gov), an always-visible helpline, a caregiver section, financial-resource lists, patient stories, and a research/news blog.
- Best-in-class individual pieces to borrow: **ABTA's 6-step "Newly Diagnosed" linear pathway**; **Cleveland Clinic's single-page, low-click-depth format** (great for fatigued/cognitively-impaired readers); **NCI's PDQ patient-vs-professional dual rendering** of the same facts; **CancerCare/ACS 24-7 human helpline**; **NBTS's excellent life-stage-categorized financial-assistance page**.

**What NOBODY does well — our differentiation (updated for the aggregation-first pivot):**
1. **No one translates the research firehose for patients.** The existing "research news" blogs are sporadic, org-filtered PR; the real sources (PubMed, ClinicalTrials.gov, preprints) are unreadable to the audience. A daily, filtered, plain-language, anti-hype feed is the new signature feature — and it's a *habit-forming* product (people check "anything new for my disease?" weekly; nobody rereads a tumor-type page).
2. **Benefits navigation is fragmented and generic.** No one connects SSA's *cancer* rules (Listing 13.13), *benign-tumor* rules (11.05), and *seizure* rules (11.02) into a single patient-facing decision tree — exactly the fork the first real user (low-grade glioma + seizures) had to navigate. First Phase 2 build.
3. **No one combines** all-tumor coverage + guided newly-diagnosed on-ramp + plain-language reading level + low click-depth simultaneously (Phase 2 hub).
4. **Consolidated side-effect / symptom content is thin everywhere** — including NCI (Phase 2).
5. **Glossary/definitions are a separate destination everywhere.** Inline tooltips — including *inside feed summaries* — are a cheap, genuine differentiator.
6. **Jargon leakage** even on good sites ("IDH-wildtype / MGMT-methylation" with no explanation) — our summarizer's whole job.

---

## 3. Design principles (non-negotiable, given the audience)

Our users may have cognitive impairment from the tumor itself, from seizures, or from treatment. This is not a nice-to-have; it's the core constraint.

- **Reading level 6th–8th grade** — for static pages (CI-gated, FK ≤ 8.5) AND for every AI summary (part of the prompt contract). ~90% of brain-tumor education materials online sit at 11th–13th grade; beating this is a measurable edge.
- **Anti-hype is a safety property.** A mouse-study headline reads as false hope to a scared patient. Hence: hard relevance filtering, stage badges on every item ("Tested in people" / "Early lab research" / "Preprint — not yet checked by other scientists"), and a mandatory "what this means — and doesn't mean" block in every summary.
- **Cognitive load matters medically.** 30–60%+ of glioma patients show cognitive impairment even before treatment; "chemo brain" affects ~1 in 3. Short chunked sections, one primary action per screen, big text, generous spacing, no dense nav walls, repetition of key info.
- **Always-visible "talk to a human" affordance** — persistent helpline band (ABTA CareLine 800-886-2282) on every page, including error pages.
- **Low click-depth.** Feed → item page → original source is the whole depth of the core loop.
- **Accessibility as a hard requirement:** WCAG AA, large-text mode, high contrast, keyboard nav, works on old/cheap Android (htmx is the entire JS budget; axe-core in CI).
- **Inline plain-language definitions** (tap tooltips, WCAG 1.4.13, no-JS fallback) site-wide — including inside summaries.
- **Print is a first-class output** — print stylesheet from M1; printable checklists throughout Phase 2.

---

## 4. Feature set & phasing

Full page inventory: [docs/sitemap.md](docs/sitemap.md). Milestones: [docs/roadmap.md](docs/roadmap.md).

### v1 (M0–M4): the aggregation product
- **`/research` feed** — daily-updated, filterable (tumor type, kind, date), patient-relevant by default with an early-stage toggle.
- **Plain-language summaries** on item permalink pages — the differentiator (pipeline: [docs/content-pipeline.md](docs/content-pipeline.md) §9–11).
- **`/trials`** — ClinicalTrials.gov v2 browse + "near me"; new/updated trials also flow into the feed.
- **Weekly email digest** — tumor-type preferences, double opt-in, human-reviewed before send; past issues published on-site.
- **Thin static shell:** home, about, **/how-we-write** (the AI-transparency trust page), interim /start page, /get-help-now (crisis + helplines), glossary seed, legal.
- Site search, feed.xml, persistent helpline band.

### Phase 2: the static hub (the original plan's core, content unchanged)
Built in differentiation order, ~1–2 verified pages/week:
- **Benefits & Disability Navigation** (see §6) — signature static section, first.
- **Newly Diagnosed pathway** — linear, ABTA-style, 6 steps (replaces interim /start).
- **Tumor Types** — 5 pages first, low-grade glioma deepest.
- **Side Effects & Symptoms** (the consolidated competitive gap), **Treatments**, **Medications-lite** (RxNorm search → MedlinePlus Connect blurb + label link-out; deepen only on demand).

### Phase 3: community
- **Patient Stories** — moderated submissions (see §7). Crisis protocol drafted **before** the form opens.

### Later / nice-to-haves
- Printable toolkits (seizure log, benefits checklist — some arrive with Phase 2 sections).
- State-selector tools (driving-after-seizure laws, Medicaid) — link out until then.
- Optional accounts; saved searches / "alert me about my tumor type" (a natural digest upgrade).
- Spanish translation — huge underserved audience; revisit once English stabilizes.

---

## 5. Content & data sourcing — with licensing verdicts

This is load-bearing for the architecture — now doubly so, since the sources ARE the v1 product. Verdicts from reading the actual terms/live APIs.

| Source | Use for | Verdict |
|---|---|---|
| **PubMed / NCBI E-utilities** | Research feed backbone | Free; get an API key (10 req/sec). Daily poll trivially within limits. `reldate` windowing for incremental pulls. Abstracts via `efetch` XML (no JSON for pubmed). Titles/abstracts: facts aren't copyrightable, but abstracts can carry publisher rights — we summarize/link rather than republish full abstracts. Check retraction notices monthly for summarized PMIDs |
| **ClinicalTrials.gov API v2** | Trial finder + trial_update feed items | **Public domain, attribution required, no key.** `query.cond` + `filter.geo=distance(lat,lon,50mi)`. Refreshes daily M–F ~9am ET. Handle HTTP 429 defensively (limit undocumented) |
| **News feeds** | News items in the feed | **NCI RSS = full text OK** (public domain, text only). **ScienceDaily = headline+summary+link only** (has a brain-tumor RSS feed). **medRxiv/bioRxiv = metadata OK; must badge "not peer-reviewed."** **EurekAlert = verify feed status first.** **Google Scholar = avoid, no legal API** |
| **NCI / cancer.gov PDQ** (patient versions) | Phase 2 tumor/treatment pages | **Public domain (17 U.S.C. §105)** — text freely reusable with attribution. Syndication program closed to new partners → copy the public-domain text ourselves. **Do NOT reuse embedded images** (licensed iStock) |
| **openFDA** (drug label) | Phase 2 drug reference | **CC0 — safe.** Label text is prescriber-facing → paraphrase to plain language. FAERS data only with heavy caveats |
| **DailyMed** (NLM) | Same SPL data, human-readable | Public domain |
| **MedlinePlus Connect** | Per-drug indication blurb (RxNorm code) | **Safe to display + attribute.** Indication/mechanism only — NOT side effects. Link out for detail |
| **MedlinePlus drug pages (AHFS)** | — | ⛔ **DO NOT scrape/store** — copyright ASHP, licensed only for MedlinePlus display. Link out only |
| **RxNorm / RxNav** | Brand→generic mapping, typo-tolerant drug search, RXCUI join key | **Free, no license for API use.** 20 req/sec. Drug-interaction API discontinued Jan 2024 — don't plan on it |
| **SSA (ssa.gov, POMS, Blue Book)** | Phase 2 benefits backbone | **Public domain.** Cite + link live listings; dollar figures carry "as of [year]" stamps |

**Plain-language strategy:** two pipelines, one standard. Automated: the summarizer translates each item from its own source text only, under a strict template with anti-hallucination checks ([docs/content-pipeline.md](docs/content-pipeline.md) §9). Curated (Phase 2): sources-first AI drafting with human per-claim verification (§2 there). Curated content lives in **Markdown in the repo** (git-reviewed); aggregated content lives in **Postgres**.

---

## 6. Static-hub signature — Benefits & Disability Navigation (Phase 2a)

Still the clearest *static* differentiator, grounded in a real success story (low-grade glioma + seizures → full SSDI + Medicaid).

**Priority content (from the research):**
1. **SSDI vs SSI** — which applies (work-credits vs need-based; the Medicare-vs-Medicaid consequence).
2. **The three-listing decision tree** — Listing **13.13** (malignant CNS, incl. WHO grade II–IV) vs **11.05** (benign) vs **11.02** (seizures). A low-grade glioma with seizures usually wins on the *combination*. Built as a guided htmx flow + printable one-page flowchart.
3. **Compassionate Allowances** — GBM and grade III/IV fast-track; **grade I–II does NOT auto-qualify** — say so explicitly; this is where people get blindsided.
4. **Documentation checklist** — seizure diary, neuropsych testing, avoiding vague "doing well" chart language.
5. **Timelines & the appeals ladder** (initial ~7–8 mo, reconsideration, ALJ hearing).
6. **The Medicare 24-month gap** and the state-dependent Medicaid bridge.
7. **Employment (ADA/FMLA), driving-after-seizures (link to Epilepsy Foundation's live state tool), insurance appeals, drug cost assistance** (live directories like NeedyMeds, not hand-maintained lists).

**Format:** printable checklists + the decision-tree flow + the real first-person "how I actually got approved" story paired with the checklist it inspired. The friend reviews the whole section before it ships.

**Maintenance & legal caveats:** "not legal advice / not affiliated with SSA" disclaimers; every dollar figure stamped "as of [year]" (2026 SGA = $1,690/mo) with a mandatory December review (SSA COLA lands October); link to live directories because named programs vanish (PAN Foundation → "TotalAssist," July 2026); monthly automated link check.

---

## 7. Patient Stories module (Phase 3)

Moderated, no accounts. Modeled on the good actors (The Patient Story, The Mighty, NBTS, Brain Tumour Charity). Schema: [docs/data-model.md](docs/data-model.md).

- **Submission form:** display-name, email, who-you-are (patient/caregiver/family), tumor type, grade, treatment path, story (~1,000–1,500 words), optional photo, **explicit consent checkboxes** (publish / edit-for-clarity / where it may appear), all recorded with timestamps.
- **Pre-publication review** — the standard model for health nonprofits. Moderation events audit-logged.
- **Taxonomy browsing** by tumor type, grade, treatment path, role, years-since-diagnosis — same taxonomy file as the feed and curated pages.
- **Misinformation stance:** personal experience of alternative treatments may be *mentioned*, not *endorsed*; decline unproven-cure promotion. Mayo-Connect-style 3-way disposition: publish / publish-with-note / decline.
- **Crisis content:** static crisis-resources page live from M1; a written moderator escalation protocol drafted **before submissions open** (hard roadmap gate).
- **Privacy:** HIPAA doesn't apply (not a covered entity) but act like consent matters anyway: solid consent language, "remove my story" path (unpublish immediately, hard-delete on request), EXIF-stripped photos, spam defenses (honeypot + fill-time + rate limit).

---

## 8. Tech architecture (summary — full detail in [docs/architecture.md](docs/architecture.md))

**Two applications.** `BrainHarbor.Web`: Razor Pages + htmx (`Htmx.Net`) on .NET 10 LTS, Dapper, DbUp, PostgreSQL — a read-mostly site with a secured sync API and an admin review queue, no background jobs. `BrainHarbor.Pipeline`: a stateless console app on Dan's PC (daily scheduled task) that fetches sources, asks the site's API what's new, runs **Claude Code CLI** for classification + plain-language summaries, and uploads results as *pending* for human approval.

Stack changed 2026-07-12 from Blazor static SSR + htmx (Htmxor unmaintained since Sept 2024; Razor Pages + htmx is the mature pairing and the exact upcoming work stack); topology changed 2026-07-18 to the local pipeline (no Anthropic API key — existing Claude Code subscription does the summarization).

Key shape:
- **Pipeline stages (local):** fetch (per source, isolated failures) → dedupe via `POST /api/sync/check` (only new items spend Claude time) → classify (closed taxonomy, relevance tiers; `excluded` never uploaded) → summarize (fixed template, JSON-validated, numeral post-check) → upload (idempotent upsert as `pending`). Model + prompt version logged per item.
- **Human gate:** approve/edit/reject in the site's admin queue (Identity + 2FA) — every published summary is human-reviewed.
- **Security:** the PC holds only a revocable API key, never DB credentials; HTTPS + rate-limited sync endpoints.
- Progressive enhancement: htmx (~14KB) is the whole JS budget; every interaction has a no-JS fallback; tested on cheap Android + throttled 3G.
- Digest drafted by a weekly local run from approved items → reviewed → sent via hosted ESP (Buttondown/Kit).
- Ops: local-first dev (Docker Postgres 5433) with **$0 hosting until launch (M4)**; GitHub Actions build+tests from commit one; App Insights + uptime ping; privacy-first analytics (**no Google Analytics** — a stated trust feature); custom error pages that still show the helpline.
- Cost: **$0 pre-launch → ~$26–32/mo** (App Service + Postgres + domain; Claude usage $0 via subscription; ESP free tier to start).

---

## 9. Cross-cutting legal / compliance guardrails

- Site-wide **"informational, not medical advice"** disclaimer.
- **Every AI summary:** visible disclosure ("written with AI assistance, **reviewed by a human before publishing** — report a problem"), stage badge, link to the original as the primary source, public correction notes when fixed.
- **Preprints:** permanently badged "not yet checked by other scientists"; never front-page-tier.
- **Aggregated content:** per-source attribution (ClinicalTrials.gov, NCI, ScienceDaily) + "fetched" stamps + no-warranty language. Summarize-and-link, never republish full abstracts.
- **Benefits section (Phase 2):** "not legal advice / not affiliated with SSA."
- **Never ingest AHFS drug monographs** (ASHP copyright); **don't reuse NCI embedded images** (licensed stock).
- **Subscriber list = de facto health data** (it identifies people tracking a brain-tumor diagnosis): double opt-in, one-click unsubscribe, never shared, purge policy — and say all of that on /privacy.
- Story consent + right-to-remove (Phase 3); PII handling per [docs/data-model.md](docs/data-model.md).
- `/how-we-write` publicly discloses the full pipeline, including AI summarization and its guardrails.

---

## 10. Discoverability & trust strategy

- **The feed solves the YMYL cold-start problem the static plan had.** A new site can't outrank Mayo for "glioblastoma" — but *nobody* is publishing fresh plain-language pages for "[drug name] glioma trial results explained." Item permalinks = a steady stream of unique, fresh, long-tail-indexable pages. Freshness is the one SEO axis where a solo site can beat institutions.
- **E-E-A-T posture:** visible provenance on every item, correction log, real named human on /about with the honest origin story, transparent /how-we-write, outbound links to primary sources. Structured data: Article/MedicalWebPage, BreadcrumbList.
- **Acquisition channel #1 is support communities, not Google:** r/braintumor, glioma Facebook groups, Inspire/ABTA forums — where "did you see this new study?" threads happen weekly with raw PubMed links. A clean plain-language summary page is the naturally shareable artifact; OG tags make it unfurl well.
- **The digest is the retention engine** — the feed brings them in, the weekly email brings them back.
- **Later:** advocacy orgs link the feed/digest once it has testimonials — the first authoritative backlinks are earned, not asked for.

---

## 11. Open questions

1. **Clinician reviewer** — none for now (solo). Pipeline is designed to be defensible without one (source-only summarization, golden set, spot-checks, flagging); revisit recruiting an RN/NP/MD spot-reviewer once the site is live and there's something concrete to show.
2. ~~Domain~~ → **brainharbor.org**, personal project. ~~Digest cadence~~ → weekly, revisit daily post-launch. ~~Tech stack~~ → Razor Pages + htmx, .NET 10. ~~v1 scope~~ → aggregation product. ~~Azure timing~~ → deferred to launch milestone; local-first dev. ~~Medication depth~~ → thin wrappers, Phase 2. ~~Hand-write vs auto-summarize~~ → both, per pipeline.

---

*Research corpus behind this plan: profiles of 12 competitor sites; patient-story/moderation practices across 8 platforms; full API + licensing analysis of PubMed, ClinicalTrials.gov v2, openFDA, DailyMed, MedlinePlus Connect, RxNorm, NCI PDQ, and news feeds; the SSA disability landscape; health-literacy & cognitive-impairment evidence; and (2026-07-12) a verification pass on the Blazor-SSR/htmx ecosystem that triggered the stack change. Source URLs are captured in the session research logs and can be pulled into per-section reference lists when we build each section.*
