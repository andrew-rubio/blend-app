# Blend

**Blend** is an AI-powered development workflow that transforms high-level product ideas into production-ready applications—using specialized GitHub Copilot agents working together in a structured, spec-driven process.

## Overview

Blend combines a curated set of AI agents, prompts, and tooling to accelerate the full software development lifecycle:

- **Spec-Driven Development** — Start from a product idea; agents produce structured PRDs, FRDs, and task breakdowns before a single line of code is written.
- **Greenfield Workflows** — Build new applications from scratch or from predefined shell baselines.
- **Brownfield Workflows** — Reverse-engineer existing codebases into comprehensive documentation, then optionally modernize them.
- **Azure Deployment** — Integrated deployment workflows targeting Azure via the Azure Developer CLI (`azd`).

## Key Features

| Feature | Description |
|---|---|
| Specialized Agents | PM, Dev Lead, Developer, Azure, Rev-Eng, Modernizer, Planner, Architect |
| Prompt Library | 13+ workflow prompts covering the full SDLC |
| MCP Servers | Context7, GitHub, Microsoft Docs, Playwright, DeepWiki |
| Dev Container | Pre-configured environment with all required tooling |
| APM Integration | Manage coding standards as versioned dependencies |

## Quick Navigation

- [Installation](getting-started/installation.md) — Get Blend set up in your environment
- [Quick Start](getting-started/quick-start.md) — Build your first app with Blend
- [Architecture Overview](architecture/overview.md) — Understand how the components fit together
- [REST API](api/rest-api.md) — API reference documentation
- [Development Guide](guides/development.md) — Contributing and local development

## Project Status

Blend is actively developed. See the [GitHub repository](https://github.com/andrew-rubio/blend-app) for the latest updates, issues, and releases.
