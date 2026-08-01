# API Keys & Config — BrainHarbor

Two places, two purposes:

1. **`.claude/.env`** — secrets for tooling/scripts in Claude Code sessions.
2. **`dotnet user-secrets`** (per project) — what the running apps read in dev;
   App Service configuration replaces it in prod (M4).

## Variables

| Variable | Where | Purpose |
|---|---|---|
| `GITHUB_TOKEN` | `.env` | `gh` CLI / API fallback (gh is already authed via keyring; usually unneeded) |
| `GITHUB_PROJECT` | `.env` | Repo URL reference for scripts (not secret) |
| `NCBI_API_KEY` | `.env` + Pipeline user-secrets | PubMed E-utilities (free; 10 rps with key). Get: https://account.ncbi.nlm.nih.gov/settings/ |
| `SYNC_API_KEY` | `.env` + BOTH apps' user-secrets | Shared secret for `/api/sync/*`. Generate: `openssl rand -hex 32` |
| `ConnectionStrings:BrainHarbor` | Web user-secrets | `Host=localhost;Port=5433;Database=brainharbor;Username=brainharbor;Password=<dev password>` |
| `ESP_API_KEY` | `.env` + user-secrets (from M4) | Buttondown/Kit API for digest sends |
| `BUILD_CMD` / `TEST_CMD` | `.env` (optional) | Overrides for the opt-in build-test-gate hook |

Notes:

- **There is intentionally NO `ANTHROPIC_API_KEY`** — summarization uses the
  locally-installed Claude Code CLI under Dan's subscription. Don't add one.
- ClinicalTrials.gov v2, NCI RSS, ScienceDaily RSS, medRxiv/bioRxiv: no keys.
- When adding a variable: placeholder line in `.claude/.env.example`, row in
  this table, and tell Dan to set the real value.

## Production (WI-401)

The live site reads its configuration from **App Service configuration**, not
from `.claude/.env` and not from `dotnet user-secrets`. Setting them is a step
in [`docs/deploy-azure.md`](../../../../docs/deploy-azure.md); this is just the
list.

| Setting | What it is |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__BrainHarbor` | Postgres Flexible Server, `SslMode=Require` |
| `SYNC_API_KEY` | The pipeline's key. **Must differ from the dev one** — otherwise a dev pipeline run could write to the live site |
| `Admin__Email` / `Admin__Password` | Seeds the single admin account. 2FA is enrolled separately, per environment |
| `Publishing__Mode` | `Auto` (default) or `Review` |

GitHub Actions deploys with **OIDC**: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`,
`AZURE_SUBSCRIPTION_ID` and `PROD_DB_CONNECTION_STRING` are secrets on the
`production` environment; the app/resource-group/server names are variables.
There is no publish profile or stored password — the repository is public.

Migrations do **not** run at startup in production. The deploy workflow runs
`dotnet BrainHarbor.Web.dll --migrate` first, so the schema is in place before
the new code serves and a bad migration fails the deploy instead of
crash-looping a live site.
