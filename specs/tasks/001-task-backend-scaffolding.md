# Task 001: Backend Project Scaffolding

## Description

Set up the backend project structure following the AGENTS.md monorepo layout and ADR decisions. This includes the .NET Aspire AppHost for local orchestration, the ASP.NET Core .NET 9 Web API project (`Blend.Api`), and the shared ServiceDefaults project (`Blend.ServiceDefaults`). Configure the solution structure, middleware pipeline, dependency injection, OpenAPI spec generation, health checks, structured logging, and the project-level configuration system.

## Dependencies

- None (this is the first task)

## Technical Requirements

### Solution structure

- Create a .NET 9 solution following the AGENTS.md monorepo layout:
  - `Blend.AppHost` — .NET Aspire orchestration project defining the app model and service dependencies for local development
  - `Blend.Api` — ASP.NET Core Web API project (the primary backend service)
  - `Blend.ServiceDefaults` — shared project providing telemetry, service discovery, resilience defaults reused across services
- Use the latest stable .NET 9 SDK and NuGet packages

### API project (`Blend.Api`)

- Configure the ASP.NET Core middleware pipeline: HTTPS redirection, CORS, authentication (placeholder), authorization (placeholder), exception handling, request logging
- Enable .NET 9 built-in OpenAPI spec generation (Microsoft.AspNetCore.OpenApi)
- Set up health check endpoints: `/healthz` (liveness) and `/ready` (readiness)
- Configure structured logging using the built-in logging abstractions (Serilog or OpenTelemetry-compatible)
- Set up environment-based configuration (`appsettings.json`, `appsettings.Development.json`) with placeholder sections for Cosmos DB, Azure AI Search, Spoonacular API key, JWT settings, and Azure Blob Storage
- Follow vertical slice architecture within the project (feature folders, not layer folders)
- Use Minimal API for simple endpoints and Controllers for complex feature areas (per ADR 0002)
- Set up global exception handling middleware returning RFC 9457 Problem Details
- Configure API versioning with URL path prefix (`/api/v1/`)

### ServiceDefaults project (`Blend.ServiceDefaults`)

- Configure OpenTelemetry for distributed tracing and metrics
- Configure health check defaults
- Configure resilience defaults (retry policies, circuit breakers) using Microsoft.Extensions.Http.Resilience
- Configure service discovery integration

### AppHost project (`Blend.AppHost`)

- Register the API project
- Configure placeholder service references for Cosmos DB, Azure AI Search, and Azure Blob Storage (Aspire resource abstractions)
- Enable the Aspire dashboard for local development observability

### Code quality setup

- Add `.editorconfig` with C# coding conventions
- Add `Directory.Build.props` for centralised package versioning and common build properties (nullable reference types enabled, implicit usings, treat warnings as errors)
- Add a `.gitignore` that covers .NET build artifacts, IDE files, environment files, user secrets, and the `apm_modules/` directory

## Acceptance Criteria

- [ ] Running `dotnet build` from the solution root completes without errors
- [ ] Running `dotnet run` on the AppHost starts the Aspire dashboard and the API project
- [ ] `GET /healthz` returns 200 OK
- [ ] `GET /ready` returns 200 OK
- [ ] `GET /openapi/v1.json` returns a valid OpenAPI 3.x spec
- [ ] Structured logs are emitted to the console in development
- [ ] The Aspire dashboard shows the API service and its health status
- [ ] Invalid API routes return a Problem Details JSON response (not a raw exception)
- [ ] CORS is configured to allow the frontend origin (configurable)
- [ ] All projects target .NET 9 and use latest stable NuGet packages

## Testing Requirements

- Unit tests for the global exception handling middleware (verifies Problem Details format for different exception types)
- Unit tests for health check endpoints (liveness and readiness)
- Integration test that starts the API host and verifies OpenAPI spec is served
- Integration test that verifies CORS headers are returned correctly
- Minimum 85% code coverage for all custom middleware and configuration code
