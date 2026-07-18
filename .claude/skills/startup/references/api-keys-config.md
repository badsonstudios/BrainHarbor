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
