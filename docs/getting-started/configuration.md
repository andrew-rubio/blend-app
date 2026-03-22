# Configuration

This page describes the environment variables and configuration options for the Blend application.

## Backend Configuration (`appsettings.json`)

The backend API is configured through `appsettings.json` and environment-specific overrides (`appsettings.Development.json`, etc.).

### Cosmos DB

```json
"CosmosDb": {
  "ConnectionString": "<your-cosmos-db-connection-string>",
  "DatabaseName": "blend"
}
```

### Spoonacular API

```json
"Spoonacular": {
  "ApiKey": "<your-spoonacular-api-key>",
  "BaseUrl": "https://api.spoonacular.com"
}
```

### JWT Authentication

```json
"Jwt": {
  "Issuer": "https://localhost:7000",
  "Audience": "blend-app",
  "SecretKey": "<your-jwt-secret-key>"
}
```

### Azure Blob Storage

```json
"BlobStorage": {
  "ConnectionString": "<your-azure-storage-connection-string>",
  "ContainerName": "blend-media"
}
```

## Frontend Configuration (`.env.local`)

The Next.js frontend reads configuration from `.env.local`:

| Variable | Required | Description |
|---|---|---|
| `NEXT_PUBLIC_API_URL` | Yes | Base URL of the Blend API (e.g. `https://localhost:7000`) |
| `NEXT_PUBLIC_APP_URL` | Yes | Public URL of the frontend app (e.g. `http://localhost:3000`) |

## Local Development

For local development, the .NET Aspire AppHost (`Blend.AppHost`) manages service configuration and connection strings automatically. See the [Installation guide](installation.md) for setup instructions.

## Further Reference

- [Environment Variables](../reference/environment-variables.md) — Full list of all environment variables
- [Configuration Reference](../reference/configuration.md) — Detailed configuration schema reference
