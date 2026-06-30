# Deploying to Azure

This guide deploys the API to **Azure Container Apps** with **Azure Service Bus** for the
queue, a free managed **PostgreSQL** (Neon), and the frontend on **Vercel**. It is designed
to cost **≈ $0** on a new Azure free account.

```
Vercel (React)  ──►  Azure Container Apps (API, scale-to-zero)
                          ├─►  PostgreSQL  (Neon free tier)
                          └─►  Azure Service Bus  ──►  email worker (in-process) ──►  logs
```

The messaging and email adapters are selected by configuration — in the cloud the API runs
with `Messaging:Provider=AzureServiceBus` and `Email:Provider=Logging` (no code changes vs. the
local RabbitMQ/MailHog setup; see [`infra/main.bicep`](infra/main.bicep)).

> **Cost:** Container Apps scales to zero, Service Bus **Basic** is cents/month, and Neon
> Postgres is free — so an idle demo is effectively free. Always set the budget alert in **Step 5**.

---

## Prerequisites

- An **Azure** account — https://azure.microsoft.com/free (card for verification only; $200 credit + always-free).
- **Azure CLI**: `winget install Microsoft.AzureCLI` (reopen the terminal, then `az version`).
- **Docker** (already used locally) and a **GitHub** account (this repo).

---

## Step 1 — Free PostgreSQL (Neon)

1. Create a project at https://neon.tech (free tier, no card).
2. Copy the connection string and convert it to the **Npgsql** form the API expects:

   ```
   Host=<host>.neon.tech;Database=<db>;Username=<user>;Password=<pwd>;SslMode=Require;Trust Server Certificate=true
   ```

   Keep this as `DB_CONNECTION` for later. The API migrates and seeds it automatically on first start.

---

## Step 2 — Build & publish the API image (GHCR)

The Container App pulls a **public** image from GitHub Container Registry.

```bash
# from the repo root — log in to GHCR with a PAT that has write:packages
echo "<github-pat>" | docker login ghcr.io -u lukascortes --password-stdin

docker build -t ghcr.io/lukascortes/timeoff-api:latest ./backend
docker push ghcr.io/lukascortes/timeoff-api:latest
```

Then in **GitHub → your profile → Packages → `timeoff-api` → Package settings → Change visibility → Public**,
so Container Apps can pull it without credentials.

> Prefer automation? Skip the manual build and use the **Deploy API** workflow in Step 6 instead —
> it builds and pushes for you on every run.

---

## Step 3 — Provision Azure (Bicep)

```bash
az login

az group create -n timeoff-rg -l eastus2

az deployment group create \
  -g timeoff-rg \
  -f infra/main.bicep \
  -p namePrefix=timeoff \
     containerImage='ghcr.io/lukascortes/timeoff-api:latest' \
     jwtKey='<a-random-secret-of-32-plus-chars>' \
     dbConnectionString='<DB_CONNECTION from Step 1>' \
     corsOrigin='http://localhost:5173'
```

The deployment outputs **`apiUrl`** (the public HTTPS URL). Verify it:

```bash
curl https://<apiUrl>/health        # -> Healthy
# Swagger: https://<apiUrl>/swagger
```

This creates: a Log Analytics workspace, a Container Apps environment, the API container app
(scale 0–2), and a Service Bus **Basic** namespace + the `email-notifications` queue.

---

## Step 4 — Frontend on Vercel

1. Import this repo at https://vercel.com → set **Root Directory** to `frontend`.
2. Add an environment variable: `VITE_API_BASE_URL = https://<apiUrl>/api`.
3. Deploy. Note the Vercel URL.
4. Allow that origin on the API (CORS) by redeploying with the new value:

   ```bash
   az deployment group create -g timeoff-rg -f infra/main.bicep \
     -p namePrefix=timeoff containerImage='ghcr.io/lukascortes/timeoff-api:latest' \
        jwtKey='<same-secret>' dbConnectionString='<DB_CONNECTION>' \
        corsOrigin='https://<your-vercel-url>'
   ```

---

## Step 5 — Budget alert (do this!)

Portal → **Cost Management → Budgets → Add** → scope the resource group, amount `$5`,
alert at **50%** and **90%** to your email. (Or `az consumption budget create ...`.)

---

## Step 6 — Continuous deployment (optional)

To roll out new images automatically via the [`Deploy API`](.github/workflows/deploy.yml) workflow:

```bash
# Create a service principal scoped to the resource group
az ad sp create-for-rbac --name timeoff-deployer \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/timeoff-rg \
  --sdk-auth
```

Add these **GitHub repo secrets** (Settings → Secrets and variables → Actions):

| Secret | Value |
| --- | --- |
| `AZURE_CREDENTIALS` | the full JSON printed by the command above |
| `AZURE_RESOURCE_GROUP` | `timeoff-rg` |

Then run **Actions → Deploy API → Run workflow**. It builds, pushes to GHCR, and updates the container app.

---

## Notes

- **Emails in the cloud:** with `Email:Provider=Logging`, the worker logs each notification
  (visible in the Container App logs / Log Analytics) instead of sending real mail. To send real
  email, set `Email:Provider=Smtp` and point `Smtp__*` at a provider such as SendGrid.
- **Service Bus:** the in-process consumer mirrors the local RabbitMQ worker. For production scale
  you'd move it to its own Container App and autoscale on queue length with KEDA.

## Teardown (stop all costs)

```bash
az group delete -n timeoff-rg --yes --no-wait
```
