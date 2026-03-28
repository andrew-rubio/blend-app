# Blend App — Deployment Guide

## Architecture Overview

The Blend application is deployed to Azure with the following services:

| Service | Azure Resource | Purpose |
|---------|---------------|---------|
| **API** | Azure Container Apps | ASP.NET Core 9 backend API |
| **Frontend** | Azure Static Web Apps | Next.js 15 frontend |
| **Database** | Azure Cosmos DB (NoSQL) | Recipe, user, and session data |
| **Storage** | Azure Blob Storage | Media uploads (images) |
| **CDN** | Azure Front Door (Standard) | CDN for blob storage assets |
| **Search** | Azure AI Search | Recipe search indexing |
| **Functions** | Azure Functions (.NET Isolated) | Image processing |
| **Key Vault** | Azure Key Vault | Secrets management (JWT keys) |
| **Monitoring** | Application Insights | API telemetry and logging |
| **Registry** | Azure Container Registry | Docker image storage |

## Environments

| Environment | Resource Group | Region | Parameters File |
|-------------|---------------|--------|-----------------|
| **Dev** | `rg-blend-dev` | `australiaeast` | `infra/main.dev.bicepparam` |
| **Prod** | `rg-blend-prod` | `australiaeast` | `infra/main.prod.bicepparam` |

## Live Endpoints (Dev)

| Resource | URL |
|----------|-----|
| API | `https://blend-dev-api-dev.<hash>.australiaeast.azurecontainerapps.io` |
| Frontend | `https://<swa-hash>.azurestaticapps.net` |
| Key Vault | `https://blend-dev-kv-dev.vault.azure.net/` |
| ACR | `blenddevacrdev.azurecr.io` |

## Prerequisites

- Azure CLI (`az`) v2.80+
- Docker CLI
- Azure Developer CLI (`azd`) v1.20+ (optional)
- Node.js 22+ (frontend builds)
- .NET SDK 9.0 (backend builds)

## Infrastructure Provisioning

### Initial Setup

```bash
# Login to Azure
az login

# Create the resource group
az group create --name rg-blend-dev --location australiaeast

# Deploy all infrastructure
az deployment group create \
  --resource-group rg-blend-dev \
  --parameters infra/main.dev.bicepparam \
  --name "blend-dev-$(date +%Y%m%d%H%M%S)"
```

### Preview Changes (What-If)

```bash
az deployment group what-if \
  --resource-group rg-blend-dev \
  --parameters infra/main.dev.bicepparam
```

## Backend Deployment (Container App)

### Manual Deployment

```bash
# Login to ACR
az acr login --name blenddevacrdev

# Build and push Docker image
docker build -t blenddevacrdev.azurecr.io/blend-api:sha-$(git rev-parse --short HEAD) -f Dockerfile .
docker push blenddevacrdev.azurecr.io/blend-api:sha-$(git rev-parse --short HEAD)

# Update the Container App
az containerapp update \
  --name blend-dev-api-dev \
  --resource-group rg-blend-dev \
  --image blenddevacrdev.azurecr.io/blend-api:sha-$(git rev-parse --short HEAD)
```

### CI/CD (Automated)

The `deploy-backend.yml` GitHub Actions workflow automates:
1. Build Docker image
2. Push to ACR
3. Deploy to Container Apps
4. Health check verification

**Required GitHub Secrets/Variables:**
- `ACR_LOGIN_SERVER` — ACR login server (e.g., `blenddevacrdev.azurecr.io`)
- `ACR_USERNAME` / `ACR_PASSWORD` — ACR credentials
- `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` — OIDC auth
- `AZURE_RESOURCE_GROUP` — Resource group name
- `BACKEND_URL` — API base URL for health checks

## Frontend Deployment (Static Web App)

The `deploy-frontend.yml` GitHub Actions workflow automates:
1. Install dependencies (`npm ci`)
2. Build Next.js (`npm run build`)
3. Deploy to Azure Static Web Apps

**Required GitHub Secrets/Variables:**
- `AZURE_STATIC_WEB_APPS_API_TOKEN` — SWA deployment token
- `NEXT_PUBLIC_API_URL` — Backend API URL
- `NEXT_PUBLIC_APP_URL` — Frontend app URL
- `FRONTEND_URL` — Frontend URL for smoke tests

## Environment Variables (API)

| Variable | Description | Source |
|----------|-------------|--------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Bicep |
| `CosmosDb__EndpointUri` | Cosmos DB endpoint | Bicep (from Cosmos module) |
| `CosmosDb__DatabaseName` | Database name | Default: `blend` |
| `CosmosDb__EnsureCreated` | Auto-create DB/containers | `true` |
| `AzureBlobStorage__BlobEndpoint` | Blob storage endpoint | Bicep (from storage module) |
| `AzureBlobStorage__ContainerName` | Blob container name | `blend-media` |
| `KeyVault__Uri` | Key Vault URI | Bicep (from KV module) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights | Bicep (from AI module) |
| `Cors__AllowedOrigins__0` | CORS origin (frontend URL) | Bicep (from SWA module) |
| `Jwt__SecretKey` | JWT signing key | Key Vault → env var |
| `Jwt__Issuer` | JWT issuer | Manual config |
| `Jwt__Audience` | JWT audience | Manual config |

## Security

### Authentication
- **Managed Identities**: API Container App uses system-assigned managed identity
- **RBAC Grants** (via `role-assignments.bicep`):
  - API → Cosmos DB Built-in Data Contributor
  - API → Blob Storage Data Contributor
  - API → ACR Pull
  - API → Key Vault Secrets User
  - Functions → Blob Storage Data Contributor
  - Functions → Key Vault Secrets User

### Key Vault
- RBAC authorization model (no access policies)
- Soft delete enabled (90-day retention)
- JWT secret stored as `JwtSecretKey`

### Network
- Storage: Public blob access disabled
- Key Vault: Public network access (restrict in production)
- Container App: External ingress with CORS policy

## Monitoring

- **Application Insights**: Connected to API via `APPLICATIONINSIGHTS_CONNECTION_STRING`
- **Log Analytics**: All container logs streamed to workspace
- **Health Probes**:
  - Liveness: `GET /healthz` (port 8080)
  - Readiness: `GET /ready` (port 8080)

### Querying Logs

```bash
# Get workspace ID
WS_ID=$(az monitor log-analytics workspace list -g rg-blend-dev --query "[0].customerId" -o tsv)

# Query console logs
az monitor log-analytics query -w $WS_ID \
  --analytics-query "ContainerAppConsoleLogs_CL | where ContainerAppName_s == 'blend-dev-api-dev' | order by TimeGenerated desc | take 20"
```

## Troubleshooting

### Container App not starting
1. Check revision status: `az containerapp revision list -n blend-dev-api-dev -g rg-blend-dev -o table`
2. Check system logs: `az containerapp logs show -n blend-dev-api-dev -g rg-blend-dev --type system`
3. Check console logs via Log Analytics (see Monitoring section)

### Common Issues
- **Globalization crash on Alpine**: Ensure `icu-libs` is installed in Dockerfile runtime stage
- **ACR pull unauthorized**: Verify AcrPull RBAC assignment and wait for propagation (~5 min)
- **JWT key length zero**: Set `Jwt__SecretKey` env var (min 32 bytes, Base64 encoded)
- **Functions storage 403**: Subscription may block shared key access — use identity-based connections

## Bicep Module Structure

```
infra/
├── main.bicep                    # Orchestrator
├── main.dev.bicepparam           # Dev parameters
├── main.prod.bicepparam          # Prod parameters
└── modules/
    ├── ai-search.bicep           # Azure AI Search
    ├── app-insights.bicep        # Application Insights
    ├── container-app-api.bicep   # API Container App
    ├── container-apps-environment.bicep  # CAE + Log Analytics
    ├── container-registry.bicep  # ACR
    ├── cosmos-db.bicep           # Cosmos DB (NoSQL)
    ├── functions.bicep           # Azure Functions
    ├── key-vault.bicep           # Key Vault
    ├── role-assignments.bicep    # RBAC role assignments
    ├── static-web-app.bicep      # Static Web App
    └── storage.bicep             # Blob Storage + Front Door CDN
```
