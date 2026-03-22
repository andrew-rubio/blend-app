# Environment Variables

This page lists all environment variables used by the Blend application.

## Backend Environment Variables

These variables configure the `Blend.Api` ASP.NET Core application. They can be set in `appsettings.Development.json` for local development or as environment variables in Azure Container Apps for production.

| Variable | Required | Description |
|---|---|---|
| `CosmosDb__ConnectionString` | Yes | Azure Cosmos DB connection string |
| `CosmosDb__DatabaseName` | Yes | Cosmos DB database name (default: `blend`) |
| `Spoonacular__ApiKey` | Yes | Spoonacular API key |
| `Jwt__Issuer` | Yes | JWT issuer URL |
| `Jwt__Audience` | Yes | JWT audience identifier |
| `Jwt__SecretKey` | Yes | JWT HMAC secret key (minimum 32 characters) |
| `Jwt__ExpiryMinutes` | No | JWT expiry in minutes (default: `60`) |
| `BlobStorage__ConnectionString` | Yes | Azure Blob Storage connection string |
| `BlobStorage__ContainerName` | Yes | Blob container name for media uploads |
| `ASPNETCORE_ENVIRONMENT` | No | Runtime environment: `Development`, `Production` |

## Frontend Environment Variables

These variables configure the Next.js frontend. Set them in `.env.local` for local development or in the Azure Static Web App configuration for production.

| Variable | Required | Description |
|---|---|---|
| `NEXT_PUBLIC_API_URL` | Yes | Base URL of the Blend API (e.g. `https://localhost:7000`) |
| `NEXT_PUBLIC_APP_URL` | Yes | Public URL of the frontend app |

## CI/CD Variables (GitHub Actions)

These secrets must be configured in the GitHub repository settings under **Settings → Secrets and variables → Actions**.

| Secret | Used By | Description |
|---|---|---|
| `AZURE_CLIENT_ID` | Deployment workflow | Service principal client ID |
| `AZURE_CLIENT_SECRET` | Deployment workflow | Service principal client secret |
| `AZURE_SUBSCRIPTION_ID` | Deployment workflow | Azure subscription ID |
| `AZURE_TENANT_ID` | Deployment workflow | Azure tenant ID |

Note: `GITHUB_TOKEN` is automatically provided by GitHub Actions and does not need to be configured manually.

## Setting Variables

### Local Development

Create `src/backend/Blend.Api/appsettings.Development.json` with required values (this file is excluded from git via `.gitignore`).

Create `src/Blend.Web/.env.local` with required values (also excluded from git).

### Production (Azure)

Use `azd env set` to manage environment-specific values:

```bash
azd env set SPOONACULAR__APIKEY <your-api-key>
```

Or set them directly in the Azure Portal for the Container App under **Settings → Environment variables**.

## TODO

- Document additional environment variables added during feature implementation
