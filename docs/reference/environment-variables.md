# Environment Variables

This page lists all environment variables supported by Blend and the applications it generates.

## Blend System Variables

| Variable | Required | Description |
|---|---|---|
| `GITHUB_TOKEN` | Yes (CI) | GitHub personal access token for API access |
| `AZURE_SUBSCRIPTION_ID` | Deployment | Azure subscription ID for resource provisioning |
| `AZURE_TENANT_ID` | Deployment | Azure tenant ID |
| `AZURE_CLIENT_ID` | Deployment (CI) | Service principal client ID for CI deployments |
| `AZURE_CLIENT_SECRET` | Deployment (CI) | Service principal client secret for CI deployments |

## MCP Server Variables

| Variable | Required | Description |
|---|---|---|
| `GITHUB_PERSONAL_ACCESS_TOKEN` | MCP GitHub | Token for the GitHub MCP server |

## Documentation Variables

| Variable | Required | Description |
|---|---|---|
| `GOOGLE_ANALYTICS_KEY` | No | Google Analytics measurement ID for the docs site |

## Setting Variables

### Local Development (Dev Container)

Create a `.env` file at the repository root (excluded from git via `.gitignore`):

```bash
GITHUB_TOKEN=ghp_xxxxxxxxxxxx
AZURE_SUBSCRIPTION_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

Or use the Azure Developer CLI to manage environment-specific values:

```bash
azd env set AZURE_SUBSCRIPTION_ID xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

### GitHub Actions

Set variables as repository secrets:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Enter the name and value

### Required Secrets for CI/CD

The following secrets must be set for the GitHub Actions workflows to function:

| Secret | Used By |
|---|---|
| `GITHUB_TOKEN` | Automatically provided by GitHub Actions |
| `AZURE_CLIENT_ID` | Deployment workflow |
| `AZURE_CLIENT_SECRET` | Deployment workflow |
| `AZURE_SUBSCRIPTION_ID` | Deployment workflow |
| `AZURE_TENANT_ID` | Deployment workflow |
