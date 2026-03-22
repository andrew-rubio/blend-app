# Architecture Overview

Blend is a cloud-native web application for home cooks, built on a modern full-stack architecture. This page provides a high-level overview of the system components and the key architectural decisions that shaped them.

## System Components

```
┌─────────────────────────────────────────────────────┐
│                    Clients                          │
│         Browser / Mobile Browser                    │
└──────────────────┬──────────────────────────────────┘
                   │ HTTPS
┌──────────────────▼──────────────────────────────────┐
│           Azure Static Web Apps (SWA)               │
│              Next.js Frontend                       │
│   (React, TanStack Query, Zustand, Tailwind CSS)    │
└──────────────────┬──────────────────────────────────┘
                   │ REST / JSON (JWT)
┌──────────────────▼──────────────────────────────────┐
│         Azure Container Apps                        │
│         ASP.NET Core .NET 9 Web API                 │
│      (Blend.Api — Vertical Slice Architecture)      │
└──────┬─────────────────┬───────────────┬────────────┘
       │                 │               │
┌──────▼──────┐  ┌───────▼──────┐  ┌────▼───────────┐
│ Cosmos DB   │  │ Spoonacular  │  │  Azure Blob     │
│ (NoSQL)     │  │ API          │  │  Storage        │
└─────────────┘  └──────────────┘  └────────────────┘
```

## Architectural Decision Records

The following ADRs document the key technology choices for Blend:

### ADR 0001 — Frontend: Next.js

The frontend is built with **Next.js** (React) deployed as an Azure Static Web App. Next.js was selected for its App Router, server-side rendering support, TypeScript integration, and strong ecosystem alignment with the chosen tech stack.

See [System Design](system-design.md) for frontend architecture details.

### ADR 0002 — Backend: ASP.NET Core .NET 9

The backend API is built with **ASP.NET Core .NET 9**. The `Blend.Api` project follows a vertical slice architecture (feature folders, not layer folders). Minimal APIs are used for simple endpoints; controllers for complex feature areas.

See [System Design](system-design.md) for backend architecture details.

### ADR 0003 — Database: Azure Cosmos DB

**Azure Cosmos DB** (NoSQL) is the primary data store. All entities are stored in a single `blend` database with containers partitioned by entity type (`/contentType`). Cosmos DB was selected for its schema flexibility, global distribution, and native Azure integration.

### ADR 0004 — Authentication: ASP.NET Core Identity + JWT

Authentication and authorisation use **ASP.NET Core Identity** with **JWT bearer tokens**. JWTs are issued by the API and validated on every request. This approach enables stateless authentication suitable for the SPA frontend.

### ADR 0008 — Deployment: Azure Container Apps + Azure Static Web Apps

The backend API is containerised and deployed to **Azure Container Apps** for scalable, serverless container hosting. The Next.js frontend is deployed to **Azure Static Web Apps (SWA)** for global CDN delivery and integrated SWA authentication.

## External Integrations

| Service | Purpose |
|---|---|
| Spoonacular API | Recipe search, ingredient data, nutritional information |
| Azure Blob Storage | User-uploaded recipe images and media |
| Azure AI Search | Full-text search indexing (planned) |

## Local Development

For local development, [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) orchestrates the API, Cosmos DB emulator, and supporting services via the `Blend.AppHost` project.

See the [Installation guide](../getting-started/installation.md) to set up a local development environment.

## Further Reading

- [System Design](system-design.md) — Domain model, data structures, and service interactions
- [Data Flow](data-flow.md) — Data flow diagrams for key user journeys
- [Development Guide](../guides/development.md) — Coding standards and branching strategy
