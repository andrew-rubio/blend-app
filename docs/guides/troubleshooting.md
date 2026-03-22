# Troubleshooting

This page covers common issues encountered when developing or running the Blend application and how to resolve them.

## Local Development Issues

### Backend fails to start

**Symptom:** `dotnet run --project src/backend/Blend.AppHost` fails with an error.

**Solutions:**

1. Ensure the .NET 9 SDK is installed: `dotnet --version`
2. Ensure Docker Desktop is running (required for the Cosmos DB emulator)
3. Run `dotnet restore src/backend/Blend.slnx` to restore NuGet packages
4. Check `src/backend/Blend.Api/appsettings.Development.json` exists and has valid configuration

### Frontend fails to start

**Symptom:** `npm run dev` fails or the browser shows an error.

**Solutions:**

1. Ensure Node.js 20+ is installed: `node --version`
2. Run `npm install` in `src/Blend.Web/` to install dependencies
3. Check that `src/Blend.Web/.env.local` exists with `NEXT_PUBLIC_API_URL` set

### Cosmos DB connection errors

**Symptom:** API returns 500 errors with a Cosmos DB connection message.

**Solutions:**

1. Ensure the Cosmos DB emulator is running (started automatically by .NET Aspire AppHost)
2. Check the connection string in `appsettings.Development.json`
3. The integration tests will skip automatically if the emulator is not running

## Authentication Issues

### JWT token expired

**Symptom:** API returns `401 Unauthorized` after some time.

**Solution:** The frontend should automatically refresh the token. If not, log out and log back in.

### Cannot register a new account

**Symptom:** Registration returns a validation error.

**Solution:** Ensure the password meets the requirements (minimum 8 characters, at least one uppercase, one number, one special character).

## Documentation Build Issues

### `mkdocs build --strict` fails

**Symptom:** The documentation build fails with warnings treated as errors.

**Solutions:**

1. Check for broken internal links — all `[text](path.md)` links must resolve to existing files
2. Check for malformed Markdown (unclosed admonitions, invalid fences)
3. Run `mkdocs serve` for live preview to identify issues interactively

## Azure Deployment Issues

See the [Deployment Guide](deployment.md) for deployment-specific troubleshooting.

## Getting Help

If you cannot resolve an issue, please [open a GitHub issue](https://github.com/andrew-rubio/blend-app/issues) with:

- A description of the problem
- Steps to reproduce
- Relevant error messages or logs
