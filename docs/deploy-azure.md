# Deploying BrainHarbor to Azure (WI-401)

Everything in this file is a **one-time setup you run yourself**. The repository
side is already done: [`.github/workflows/deploy.yml`](../.github/workflows/deploy.yml)
migrates the database and deploys the site whenever CI passes on `main`.

Companion to [architecture.md](architecture.md) §8/§9. Expect **~$30/month**.

---

## What you are creating

| Thing | Why | Rough cost |
|---|---|---|
| Resource group | Holds everything, so deleting it deletes the bill | free |
| App Service Plan **B1**, Always On | Runs the site. Always On matters: without it the app cold-starts and the first reader waits ~20s | ~$13/mo |
| App Service (Linux, .NET 10) | The site itself | included above |
| Postgres Flexible Server **B1ms**, 32 GB | The database | ~$15/mo |
| Managed TLS certificate | HTTPS on brainharbor.org | free |
| Entra app registration + federated credential | Lets GitHub deploy **without storing a password** in a public repo | free |

Set these once so the commands below can be copy-pasted:

```bash
RG=brainharbor-rg
LOC=eastus
APP=brainharbor                 # becomes brainharbor.azurewebsites.net — must be globally unique
PG=brainharbor-db               # also globally unique
PGADMIN=bhadmin
PGPASS='<a long random password you generate>'
SUB=$(az account show --query id -o tsv)
```

## 0. Log in and check the runtime

The cached `az` token is **expired** (issued May, inactive 90 days), so start
here or every command below fails with `AADSTS700082`:

```bash
az login
az account show          # confirm the Badson Studios subscription
az account set --subscription "<name or id>"   # only if it picked the wrong one
```

Then confirm what .NET the platform actually offers. **.NET 10 is new and may
not be there yet** — this is the one value in this file I could not verify:

```bash
az webapp list-runtimes --os linux | grep -i dotnet
```

Use the newest `DOTNETCORE:*` it prints in step 1. If 10.0 is missing, either
deploy self-contained or stay on the newest available and set
`<TargetFramework>` to match — tell me and I'll sort it.

> Flag names below were validated against Azure CLI **2.79.0**, the version on
> this machine. Values (SKUs, runtime strings) still depend on what your
> subscription and region offer.

---

## 1. Resource group, database, app

```bash
az group create --name "$RG" --location "$LOC"

# Postgres. Public access with NO firewall rules yet: nothing can reach it until
# we add a rule, which is the right default for a patient database.
az postgres flexible-server create \
  --resource-group "$RG" --name "$PG" --location "$LOC" \
  --tier Burstable --sku-name Standard_B1ms --storage-size 32 \
  --version 16 --admin-user "$PGADMIN" --admin-password "$PGPASS" \
  --public-access None --yes

az postgres flexible-server db create \
  --resource-group "$RG" --server-name "$PG" --database-name brainharbor

# App Service.
az appservice plan create \
  --resource-group "$RG" --name "$APP-plan" --location "$LOC" \
  --is-linux --sku B1

az webapp create \
  --resource-group "$RG" --plan "$APP-plan" --name "$APP" \
  --runtime "DOTNETCORE:10.0"

# Always On: without it the site sleeps and the first visitor after a quiet
# spell waits for a cold start.
az webapp config set --resource-group "$RG" --name "$APP" --always-on true

# Let the App Service itself reach Postgres. (0.0.0.0 is Azure's special
# "allow other Azure services" rule, not "allow the internet".)
az postgres flexible-server firewall-rule create \
  --resource-group "$RG" --name "$PG" \
  --rule-name allow-azure-services \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

## 2. Application settings

Generate a fresh sync key — this is what the pipeline on your PC authenticates
with, and it must **not** be the dev one:

```bash
PROD_SYNC_KEY=$(openssl rand -hex 32)
CONN="Host=$PG.postgres.database.azure.com;Port=5432;Database=brainharbor;Username=$PGADMIN;Password=$PGPASS;SslMode=Require"

az webapp config appsettings set --resource-group "$RG" --name "$APP" --settings \
  ASPNETCORE_ENVIRONMENT=Production \
  ConnectionStrings__BrainHarbor="$CONN" \
  SYNC_API_KEY="$PROD_SYNC_KEY" \
  Admin__Email="<your admin login email>" \
  Admin__Password="<a long random password>" \
  Publishing__Mode=Auto
```

`Publishing__Mode` is `Auto` to match the current site behaviour. Set it to
`Review` if you want every summary to wait for you before it publishes
(see [content-pipeline.md](content-pipeline.md) §"Publish mode").

**Keep `PROD_SYNC_KEY`, `PGPASS` and the admin password.** Put them in
`.claude/.env` locally (git-ignored) — you need them in steps 4 and 5.

## 3. Let GitHub deploy without a stored password

OIDC, so nothing long-lived lives in a public repository.

```bash
APPREG=$(az ad app create --display-name "brainharbor-github-deploy" --query appId -o tsv)
az ad sp create --id "$APPREG"

az role assignment create \
  --assignee "$APPREG" --role Contributor \
  --scope "/subscriptions/$SUB/resourceGroups/$RG"

# Trust deploys from this repo's `production` environment.
az ad app federated-credential create --id "$APPREG" --parameters '{
  "name": "github-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:badsonstudios/BrainHarbor:environment:production",
  "audiences": ["api://AzureADTokenExchange"]
}'

echo "AZURE_CLIENT_ID       = $APPREG"
echo "AZURE_TENANT_ID       = $(az account show --query tenantId -o tsv)"
echo "AZURE_SUBSCRIPTION_ID = $SUB"
```

Then in GitHub — **Settings → Environments → New environment → `production`**
(the name must match; the federated credential above is scoped to it):

| Kind | Name | Value |
|---|---|---|
| Secret | `AZURE_CLIENT_ID` | from above |
| Secret | `AZURE_TENANT_ID` | from above |
| Secret | `AZURE_SUBSCRIPTION_ID` | from above |
| Secret | `PROD_DB_CONNECTION_STRING` | the `$CONN` string from step 2 |
| Variable | `AZURE_APP_NAME` | `brainharbor` |
| Variable | `AZURE_RESOURCE_GROUP` | `brainharbor-rg` |
| Variable | `AZURE_PG_SERVER` | `brainharbor-db` |

Variables (not secrets) for the three names: they appear in log output anyway,
and masking them makes failures harder to read.

## 4. First deploy

```bash
gh workflow run Deploy --repo badsonstudios/BrainHarbor
gh run watch --repo badsonstudios/BrainHarbor
```

The workflow migrates first, then deploys, then smoke-tests `/`,
`/get-help-now`, `/research` and `/trials`. If the smoke test fails the run goes
red — a deploy that "succeeded" onto a broken site is worse than a failed one,
because nobody looks.

Then check by hand:

- <https://brainharbor.azurewebsites.net> loads
- `/admin` → log in → **re-enrol 2FA**. The TOTP secret is per-environment;
  your dev authenticator entry will not work here.

## 5. Point the pipeline at production

On your PC:

```bash
dotnet user-secrets set "Pipeline:SyncApiBaseUrl" "https://brainharbor.org" --project src/BrainHarbor.Pipeline
dotnet user-secrets set "Pipeline:SyncApiKey" "$PROD_SYNC_KEY" --project src/BrainHarbor.Pipeline
```

Run it once by hand (`dotnet run --project src/BrainHarbor.Pipeline`) and watch
the log before you trust the scheduled task. The first run backfills the feed
and will take a while: it summarizes every new item through the local Claude
CLI.

## 6. DNS and TLS — brainharbor.org

**This part is yours; nothing in the repo can do it.** The domain is at your
registrar, and TLS cannot be issued until DNS resolves.

```bash
# Azure gives you the two records to create.
az webapp config hostname get-external-ip --resource-group "$RG" --webapp-name "$APP"
az webapp show --resource-group "$RG" --name "$APP" --query customDomainVerificationId -o tsv
```

At your registrar, for `brainharbor.org`:

| Record | Name | Points to |
|---|---|---|
| `A` | `@` | the IP printed above |
| `TXT` | `asuid` | the verification id printed above |
| `CNAME` | `www` | `brainharbor.azurewebsites.net` |
| `TXT` | `asuid.www` | the same verification id |

Wait for propagation (`dig brainharbor.org +short`), then bind and secure:

```bash
az webapp config hostname add --resource-group "$RG" --webapp-name "$APP" \
  --hostname brainharbor.org
az webapp config hostname add --resource-group "$RG" --webapp-name "$APP" \
  --hostname www.brainharbor.org

# Free managed certificates, one per hostname.
for HOST in brainharbor.org www.brainharbor.org; do
  az webapp config ssl create --resource-group "$RG" --name "$APP" --hostname "$HOST"
  THUMB=$(az webapp config ssl list --resource-group "$RG" \
    --query "[?subjectName=='$HOST'].thumbprint | [0]" -o tsv)
  az webapp config ssl bind --resource-group "$RG" --name "$APP" \
    --certificate-thumbprint "$THUMB" --ssl-type SNI
done

az webapp update --resource-group "$RG" --name "$APP" --https-only true
```

The app already sends HSTS in production (`Program.cs`), so do this only once
you are sure HTTPS works — HSTS is sticky in browsers.

---

## If something goes wrong

**The deploy fails on migrations.** Read the step's log. The firewall rule for
the runner is removed even when the migration fails (`if: always()`), so a
failed run does not leave the database exposed. Nothing was deployed — the site
is still serving the previous version against the old schema, which is why
migrate runs first.

**The site returns 500 after deploying.** Almost always a missing app setting.
`az webapp log tail --resource-group "$RG" --name "$APP"`.

**Rolling back.** `git revert` the commit and let the pipeline run. There is no
automatic schema rollback: migrations are additive by convention, so old code
runs against a newer schema safely. If you ever need a destructive migration,
that convention needs revisiting first.

**Turning the meter off.** `az group delete --name "$RG" --yes` removes
everything, including the database. Take a backup first:
`az postgres flexible-server backup list --resource-group "$RG" --name "$PG"`.

## Still outstanding after this

WI-401's acceptance also lists **feed backfill** (step 5 covers it) and
**admin 2FA re-verified in prod** (step 4). WI-407 adds the pre-launch checks:
Lighthouse, a cheap-Android pass, an uptime ping, and a rehearsed
backup/restore.
