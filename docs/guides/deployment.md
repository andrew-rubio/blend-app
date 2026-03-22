# Deployment Guide

This guide covers deploying the Blend application to Azure using Azure Container Apps (backend API) and Azure Static Web Apps (frontend).

## Architecture

| Component | Azure Service |
|---|---|
| Backend API (`Blend.Api`) | Azure Container Apps |
| Frontend (`Blend.Web`) | Azure Static Web Apps (SWA) |
| Database | Azure Cosmos DB |
| Media Storage | Azure Blob Storage |

## Prerequisites

- Azure subscription
- Azure CLI (`az`) — included in the Dev Container
- Azure Developer CLI (`azd`) — included in the Dev Container
- Docker Desktop (for building container images)

## Initial Deployment

### Step 1: Log in to Azure

```bash
az login
azd auth login
```

### Step 2: Provision Azure resources

```bash
azd provision
```

This creates all required Azure resources (Container Apps environment, Cosmos DB, Blob Storage, SWA) using the Bicep templates in `infra/`.

### Step 3: Deploy the application

```bash
azd deploy
```

### Step 4: Configure environment variables

After provisioning, set any secrets that are not managed by `azd`:

```bash
azd env set SPOONACULAR_API_KEY <your-api-key>
```

## CI/CD Deployment

The repository includes GitHub Actions workflows for automated deployment. See `.github/workflows/` for the deployment workflow configuration.

Required GitHub repository secrets:

| Secret | Description |
|---|---|
| `AZURE_CLIENT_ID` | Service principal client ID |
| `AZURE_CLIENT_SECRET` | Service principal client secret |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_TENANT_ID` | Azure tenant ID |

## Rollback

To roll back to a previous deployment:

```bash
azd deploy --from-package <previous-package-path>
```

Or use the Azure Portal to revert to a previous Container Apps revision.

## TODO

- Document Bicep infrastructure templates once `infra/` scaffolding is complete
- Add zero-downtime deployment strategy details
- Document environment-specific configuration management

## Troubleshooting

See the [Troubleshooting guide](troubleshooting.md) for common deployment issues.
