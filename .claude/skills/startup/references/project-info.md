# Project Info — BrainHarbor

## Purpose

BrainHarbor (brainharbor.org) is a plain-language brain-tumor research hub for
**patients and caregivers**. Core product: a daily feed of research/news/trials
with AI-generated plain-language summaries (via local Claude Code CLI), each
human-approved before publishing. Supporting pillars arrive in later phases:
a Newly Diagnosed pathway, a Benefits & Disability navigator (SSDI/SSI decision
tree), tumor-type pages, and moderated patient stories.

Origin: built after helping a friend with a low-grade glioma + seizures navigate
SSDI/Medicaid. Personal project (no LLC/nonprofit), solo developer, nights &
weekends.

## Status

- **Current phase:** see `PROGRESS.md` (always current).
- Planning/design is complete — `PLAN.md` + `docs/*.md` are the spec;
  `docs/backlog.md` is the itemized work list.

## Key documentation

- `PLAN.md` — master plan (decisions, scope, phasing, licensing verdicts).
- `docs/architecture.md` — two-app topology (Web + local Pipeline), sync API.
- `docs/content-pipeline.md` — **read before touching anything content-related**
  (reading-level, anti-hype, summarization guardrails — these are hard rules).
- `docs/roadmap.md` / `docs/backlog.md` / `PROGRESS.md` — what to build, in
  what order, and where we are.

## Links

- **Domain:** brainharbor.org (registration is a `[user]` backlog item; changed from .net 2026-07-18).
- **Repo:** private GitHub at `badsonstudios/BrainHarbor` (created at WI-002).
- **Hosting (from M4):** Azure App Service B1 + PostgreSQL Flexible B1ms.
