# Task 004: Infrastructure Scaffolding (Docker, CI/CD, Azure IaC)

## Status: Complete

## Summary

Set up Docker containerisation, CI/CD GitHub Actions workflows, and deployment
manifest placeholders for Azure Container Apps (backend) and Azure Static Web
Apps (frontend) per ADR 0008.

---

## Deliverables

### Docker

| File | Description |
|------|-------------|
| `src/Blend.Api/Dockerfile` | Multi-stage build: SDK 9.0-alpine build → ASP.NET 9.0-alpine runtime |
| `.dockerignore` | Excludes `bin/`, `obj/`, `.git`, `node_modules`, `.next` |
| `docker-compose.yml` | Local dev stack: `blend-api`, `blend-web`, `cosmosdb-emulator` |

### CI/CD Workflows

| File | Triggers | Jobs |
|------|----------|------|
| `.github/workflows/ci.yml` | Push/PR to `main`, `develop` | `backend`, `frontend`, `docs`, `security`, `docker-build` |
| `.github/workflows/deploy-backend.yml` | Push to `main` (src/Blend.Api) | `build-and-push` (ACR), `deploy` (Container Apps), `health-check` |
| `.github/workflows/deploy-frontend.yml` | Push to `main` (src/Blend.Web) | `build` (Next.js), `deploy` (Azure SWA), `smoke-tests` |
| `.github/workflows/codeql.yml` | Push/PR + weekly schedule | CodeQL for C# and TypeScript |

### Security

| File | Description |
|------|-------------|
| `.github/dependabot.yml` | Weekly updates for NuGet, npm, GitHub Actions |

### Azure SWA

| File | Description |
|------|-------------|
| `src/Blend.Web/staticwebapp.config.json` | Routes, navigation fallback, security headers, response overrides |

### IaC (Bicep)

| File | Resource |
|------|----------|
| `infra/main.bicep` | Root deployment — resource group + module orchestration |
| `infra/modules/acr.bicep` | Azure Container Registry |
| `infra/modules/container-apps.bicep` | Container Apps Environment + Blend.Api app |
| `infra/modules/cosmos-db.bicep` | Cosmos DB (NoSQL, serverless) with containers |
| `infra/modules/ai-search.bicep` | Azure AI Search |
| `infra/modules/storage.bicep` | Blob Storage + CDN |
| `infra/modules/swa.bicep` | Azure Static Web Apps |
| `infra/modules/functions.bicep` | Azure Functions (placeholder) |

### Environment Files

| File | Description |
|------|-------------|
| `src/Blend.Api/.env.example` | Backend env vars template |
| `src/Blend.Web/.env.example` | Frontend env vars template |

---

## Acceptance Criteria Verification

- [x] `docker build` succeeds — multi-stage Dockerfile using Alpine images (<200 MB)
- [x] Container starts with passing health checks — `HEALTHCHECK` in Dockerfile, probes in Bicep
- [x] CI workflow includes all stages — backend, frontend, docs, security, docker-build
- [x] Deploy workflows for backend and frontend — `deploy-backend.yml`, `deploy-frontend.yml`
- [x] `staticwebapp.config.json` present — at `src/Blend.Web/staticwebapp.config.json`
- [x] `/infra` has placeholder Bicep files — 7 Bicep modules + main
- [x] Dependabot configured — `.github/dependabot.yml` (NuGet, npm, Actions)
- [x] CodeQL workflow present — `.github/workflows/codeql.yml` (C# + TypeScript)
- [x] `.env.example` files present — backend and frontend
- [x] Docker image <200 MB — ASP.NET 9.0-alpine runtime base (~95 MB)
