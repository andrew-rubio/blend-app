# Development Guide

This guide covers the developer workflow for contributing to the Blend application, including local setup, coding standards, branching strategy, and the pull request process.

## Prerequisites

- Completed the [Installation](../getting-started/installation.md) steps
- Familiarity with .NET 9, Next.js, and TypeScript

## Running the Application Locally

**Backend** (with .NET Aspire):

```bash
dotnet run --project src/backend/Blend.AppHost
```

**Frontend:**

```bash
cd src/Blend.Web
npm run dev
```

**Documentation site:**

```bash
pip install -r requirements-docs.txt
mkdocs serve
```

## Running Tests

**Backend unit tests:**

```bash
dotnet test src/tests/Blend.Tests.Unit/
```

**Backend integration tests** (requires Cosmos DB emulator):

```bash
dotnet test src/tests/Blend.Tests.Integration/
```

**Frontend tests:**

```bash
cd src/Blend.Web
npm test
```

**Documentation build:**

```bash
mkdocs build --strict
```

## Coding Standards

- **Backend:** Follow C# conventions with nullable reference types enabled. Use vertical slice architecture — each feature has its own folder in `Blend.Api/Features/`.
- **Frontend:** TypeScript strict mode. Use TanStack Query for server state, Zustand for client state. Path aliases: `@/components`, `@/lib`, `@/hooks`, `@/types`, `@/stores`.
- **Commits:** Follow [Conventional Commits](https://www.conventionalcommits.org/) (e.g., `feat:`, `fix:`, `docs:`).

## Branching Strategy

| Branch | Purpose |
|---|---|
| `main` | Production-ready code; protected branch |
| `feature/<name>` | New features |
| `fix/<name>` | Bug fixes |
| `docs/<name>` | Documentation changes |

## Pull Request Process

1. Create a feature branch from `main`
2. Implement changes with tests
3. Ensure all CI checks pass (`dotnet test`, `npm test`, `mkdocs build --strict`)
4. Open a pull request against `main` with a descriptive title and linked issue
5. Address review feedback
6. Merge once approved and CI passes

## Project Structure

```
blend-app/
├── src/
│   ├── backend/           # .NET solution (Blend.slnx)
│   │   ├── Blend.Api/     # ASP.NET Core Web API
│   │   ├── Blend.AppHost/ # .NET Aspire host
│   │   └── Blend.ServiceDefaults/
│   ├── Blend.Web/         # Next.js frontend
│   └── tests/
│       ├── Blend.Tests.Unit/
│       └── Blend.Tests.Integration/
├── docs/                  # MkDocs documentation source
├── specs/                 # Architecture specs and task definitions
└── mkdocs.yml
```

## TODO

- Add code review checklist once team standards are finalised
- Document ADR process for proposing new architectural decisions
