# BrainHarbor — Information Architecture & Sitemap

Companion to [PLAN.md](../PLAN.md). Scope: **aggregation-first v1** (feed + plain-language summaries + trials + digest), with the static hub arriving in later phases on already-reserved URLs so nothing ever moves.

## Principles applied

- **The feed is the front door.** Home leads with "What's new in brain tumor research — in plain language," not a brochure.
- **But the scared newcomer still gets caught:** a prominent secondary "Just diagnosed? Start here" path exists from day one — pointing at a short interim page until the full pathway ships in Phase 2.
- **Low click-depth, stable URLs, print-friendly, helpline on every page** — unchanged from the original principles.
- Every aggregated item gets a **permalink page** (the SEO surface and the shareable artifact for support groups).

## URL structure

```
/                               Home — latest plain-language research highlights + digest signup
                                + "Just diagnosed? Start here" secondary entry

/research/                      THE FEED — filter by tumor type, kind (research/news/trials),
                                date; "show early-stage research" toggle (default: human-relevant only);
                                sort by date (default), readiness (unscored last), or kind (?sort=)
/research/{slug}                Item page: plain-language summary, stage badge, provenance,
                                link to original, "report a problem" affordance
/trials/                        Trial browse + "near me" search
/trials/{nct-id}                Trial page (plain-language summary + ClinicalTrials.gov link)
/digest/                        What the digest is + signup (double opt-in) + past issues
/digest/{issue}                 Published past issues (SEO + preview for non-subscribers)

/glossary                       A–Z; terms render as inline tooltips inside summaries too
/get-help-now                   Helplines + crisis resources (988, Crisis Text Line) — 1 click, always
/about                          Who runs this, why, the origin story
/how-we-write                   Editorial + AI policy: how items are selected, classified,
                                summarized, and corrected. THE trust page for an AI-summary site
/start                          Interim "just diagnosed" page (v1): calm orientation + best
                                external resources + helpline — honest stopgap until Phase 2
/privacy   /terms               Legal (privacy includes the no-tracking, no-list-sharing promises)

--- Phase 2: the static hub (reserved) ---
/start/                         Full Newly Diagnosed pathway (6-step, ABTA-style)
/benefits/                      Benefits & Disability Navigation (the 3-listing decision tree
                                + SSDI/SSI, fast-track, paperwork, appeals, insurance, work,
                                driving, drug costs — per PLAN.md §6)
/tumors/                        Tumor-type pages (low-grade glioma deepest first)
/side-effects/  /treatments/  /medications/

--- Phase 3 (reserved) ---
/stories/   /stories/{slug}   /stories/share

--- utility ---
/search                         Site search (htmx) — searches items + static pages
/sitemap.xml  /robots.txt  /feed.xml (RSS of the summarized feed — cheap once items exist)
```

## Navigation model

- **Header (v1):** logo, Research · Trials · Digest · Get Help Now, search. Phase 2 adds Start Here · Benefits · Tumor Types (nav stays ≤ 6 items; Research collapses under a "Latest" if needed).
- **Persistent helpline band** on every page: "Need to talk to someone? ABTA CareLine 800-886-2282."
- **Feed item anatomy (the core UI unit):** plain-language title → stage badge ("Tested in people" / "Early lab research" / "News" / "New trial" / "Preprint — not yet reviewed") → 1-sentence hook → tumor-type tags → date + source. One tap to the item page.
- **Footer:** disclaimers, crisis link, editorial policy, "AI-assisted summaries — how we do it" link.

## v1 writing budget (what still has to be hand-written)

The feed generates itself; these don't:

| Page | Est. effort |
|---|---|
| Home, /about, /how-we-write, /get-help-now, /start (interim), /digest landing | 6 short pages, ~2,500 words |
| /privacy, /terms | boilerplate + the real privacy promises |
| Glossary seed (~40 terms — driven by what recurs in summaries) | ~1,500 words |
| **Prompt engineering** for classifier + summarizer (see content-pipeline §9) | the real "writing" work of v1 — iterating templates against golden examples |

Total hand-written prose is ~4–5k words — a fraction of the old hub-first v1 (~20k). The effort moved from prose to pipeline. Phase 2's hub content keeps the original ~20k-word budget and the 1–2 pages/week pacing.
