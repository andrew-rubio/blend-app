# Configuration Reference

This page provides a detailed reference for all configuration files used in the Blend application.

## Backend: `appsettings.json`

Located at `src/backend/Blend.Api/appsettings.json`. Override with environment-specific files:

- `appsettings.Development.json` — local development overrides
- `appsettings.Production.json` — production overrides (managed via Azure App Configuration or environment variables)

### `CosmosDb` section

| Key | Type | Required | Description |
|---|---|---|---|
| `ConnectionString` | string | Yes | Azure Cosmos DB connection string |
| `DatabaseName` | string | Yes | Cosmos DB database name (default: `blend`) |

### `Spoonacular` section

| Key | Type | Required | Description |
|---|---|---|---|
| `ApiKey` | string | Yes | Spoonacular API key |
| `BaseUrl` | string | No | Base URL (default: `https://api.spoonacular.com`) |

### `Jwt` section

| Key | Type | Required | Description |
|---|---|---|---|
| `Issuer` | string | Yes | JWT issuer URL |
| `Audience` | string | Yes | JWT audience identifier |
| `SecretKey` | string | Yes | HMAC secret key (minimum 32 characters) |
| `ExpiryMinutes` | int | No | Token expiry in minutes (default: `60`) |

### `BlobStorage` section

| Key | Type | Required | Description |
|---|---|---|---|
| `ConnectionString` | string | Yes | Azure Blob Storage connection string |
| `ContainerName` | string | Yes | Container name for media uploads |

## Frontend: `.env.local`

Located at `src/Blend.Web/.env.local` (not committed to source control).

| Variable | Type | Required | Description |
|---|---|---|---|
| `NEXT_PUBLIC_API_URL` | string | Yes | Base URL of the Blend API |
| `NEXT_PUBLIC_APP_URL` | string | Yes | Public URL of the frontend |

## MkDocs: `mkdocs.yml`

Located at the repository root. Controls the documentation site build.

| Key | Description |
|---|---|
| `site_name` | Site title |
| `site_url` | Canonical URL for the deployed docs site |
| `theme.name` | MkDocs theme (must be `material`) |
| `plugins` | Enabled plugins: `search`, `git-revision-date-localized`, `minify` |
| `nav` | Documentation navigation structure |

## TODO

- Document Azure App Configuration keys once infrastructure is provisioned
- Document all feature flag names once feature flags are implemented
