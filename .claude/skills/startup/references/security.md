# Security — BrainHarbor

## Secrets policy

- All tooling secrets in **`.claude/.env`** (git-ignored; staging blocked by
  hook). Placeholders documented in `.claude/.env.example`.
- App runtime secrets in **`dotnet user-secrets`** (dev) / App Service config
  (prod, from M4). Never in appsettings.json, code, or logs.
- The Pipeline PC holds **only the sync API key** — never DB credentials.

## The sync API

- API-key header auth on every `/api/sync/*` endpoint; 401 without it; HTTPS
  only; rate-limited. Key rotation = change one config value on both sides.
- Upserts are idempotent — replay of a batch must be harmless by design.

## Admin

- ASP.NET Core Identity, **single admin account, TOTP 2FA, no registration
  endpoint**. Admin pages and the review queue live behind auth; anti-forgery
  on all admin POSTs (standard tag-helper machinery).

## Data / privacy (see docs/data-model.md PII notes)

- Subscriber emails are de facto health data (the list identifies people
  tracking a brain-tumor diagnosis): double opt-in, one-click unsubscribe,
  never shared, purge unconfirmed after 7 days.
- No Google Analytics, no ad trackers, minimal logs — stated on /privacy and
  honored in code.
- (Phase 3) story submissions: hash IPs, strip photo EXIF, right-to-remove.

## Content safety (unique to this project)

- Nothing publishes without human approval (`pending` → admin queue →
  `published`). Do not add code paths that bypass the gate.
- Summarizer constraints (source-only, numeral post-check, banned phrases,
  stage badges) are safety controls — never relax them to "make output nicer".
- Licensing red lines from `PLAN.md` §5: never ingest AHFS/MedlinePlus drug
  monographs; never reuse NCI embedded images; preprints always badged.
