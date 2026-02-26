# Task 004: Infrastructure Scaffolding

> **GitHub Issue:** [#4](https://github.com/andrew-rubio/blend-app/issues/4)

## Description

Set up the infrastructure foundation including Docker containerisation for the backend, CI/CD GitHub Actions workflows for build/test/deploy, and deployment manifest placeholders for Azure Container Apps (backend) and Azure Static Web Apps (frontend) per ADR 0008. Also configure Azure Container Registry integration and environment-specific deployment configuration.

## Dependencies

- **001-task-backend-scaffolding** — Dockerfile depends on the backend project structure
- **002-task-frontend-scaffolding** — SWA configuration depends on the frontend project structure

## Technical Requirements

### Docker configuration

- Create a multi-stage `Dockerfile` for the `Blend.Api` project:
  - Build stage: restore, build, publish
  - Runtime stage: minimal runtime image
- Create a `docker-compose.yml` (or `.docker-compose.override.yml`) for local development that includes the API and any local emulators (e.g., Cosmos DB emulator)
- Create a `.dockerignore` to exclude unnecessary files from the build context

### CI/CD workflows (GitHub Actions)

Create workflows in `.github/workflows/`:

- **`ci.yml`** — Runs on every PR and push to main:
  - Backend: restore → build → test → publish OpenAPI spec artifact
  - Frontend: install → lint → type-check → test → build
  - Documentation: `mkdocs build --strict`
  - Security: dependency review (GitHub dependency-review-action)
  - Report test results and coverage

- **`deploy-backend.yml`** — Runs on push to main (after CI passes):
  - Build Docker image
  - Push to Azure Container Registry
  - Deploy to Azure Container Apps
  - Run health check verification post-deploy

- **`deploy-frontend.yml`** — Runs on push to main (after CI passes):
  - Build Next.js application
  - Deploy to Azure Static Web Apps
  - Run smoke tests post-deploy

### Azure Static Web Apps configuration

- Create `staticwebapp.config.json` in the frontend project with:
  - Routing rules (fallback to index for SPA routes)
  - API proxy configuration to route `/api/*` to the Container Apps backend
  - Security headers (CSP, X-Frame-Options, etc.)
  - Custom error pages (404)

### Infrastructure as Code (IaC) placeholders

- Create `/infra` directory with placeholder Bicep (or Terraform) files for:
  - Azure Container Apps Environment
  - Azure Container Registry
  - Azure Static Web Apps
  - Azure Cosmos DB account (NoSQL API)
  - Azure AI Search service
  - Azure Blob Storage account + CDN profile
  - Azure Functions app (for image processing)
- These are placeholders with the resource structure defined but not fully parameterised

### Environment configuration

- Define environment-specific configuration strategy:
  - Development: local Aspire with emulators
  - Staging: Azure resources with staging slot configuration
  - Production: Azure resources with production configuration
- Create `.env.example` files documenting all required environment variables for backend and frontend

### Security

- Configure GitHub Dependabot for automated dependency updates (`.github/dependabot.yml`)
- Add CodeQL analysis workflow (`.github/workflows/codeql.yml`)

## Acceptance Criteria

- [ ] `docker build` successfully builds the backend API image
- [ ] `docker run` starts the API container and health checks pass
- [ ] CI workflow (`ci.yml`) is syntactically valid and includes all required stages (lint, type-check, build, test, security scan)
- [ ] Deploy workflows exist for both backend and frontend
- [ ] `staticwebapp.config.json` is present with routing rules and API proxy configuration
- [ ] `/infra` directory contains placeholder Bicep/Terraform files for all required Azure resources
- [ ] `.github/dependabot.yml` is configured for .NET, npm, and Docker dependencies
- [ ] CodeQL workflow is present
- [ ] `.env.example` files document all required environment variables
- [ ] Docker image size is reasonable (< 200MB for the runtime image)

## Testing Requirements

- Verify Docker build completes without errors
- Verify the container starts and health check endpoints respond
- Verify CI workflow YAML files are valid (use `actionlint` or GitHub's workflow validator)
- Verify `staticwebapp.config.json` is valid JSON with correct schema
- No additional unit tests required for infrastructure scaffolding
