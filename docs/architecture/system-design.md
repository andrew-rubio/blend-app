# System Design

This page describes the technical design of the Blend system, including the Dev Container environment, agent configuration, and tooling integrations.

## Development Environment

The `.devcontainer/` folder provides a ready-to-use Docker-based development environment. Opening the repository in VS Code triggers the Dev Container, which installs all required tools automatically.

### Included Tooling

| Tool | Version | Purpose |
|---|---|---|
| Python | 3.12 | Scripting, MkDocs, APM CLI |
| Azure CLI | Latest | Azure resource management |
| Azure Developer CLI (`azd`) | Latest | Application deployment |
| Node.js / TypeScript | LTS | Frontend and tooling |
| Docker-in-Docker | Latest | Container builds inside the Dev Container |

### VS Code Extensions

The Dev Container pre-installs:

- GitHub Copilot Chat
- Azure Tools Pack
- AI Studio extension

## Agent Architecture

Each agent is defined as a `.agent.md` file in `.github/agents/`. The file specifies:

- **Model** — The underlying LLM (e.g., `o3-mini`, `gpt-4o`)
- **Tools** — Permitted tool access (file editing, web search, etc.)
- **Instructions** — Detailed behavioural guidance

Agents are invoked via prompt files (`.github/prompts/*.prompt.md`) or directly through Copilot Chat using `@agent-name` syntax.

## Standards Management (APM)

Coding standards are managed as versioned APM dependencies declared in `apm.yml`. Running `apm install` fetches the latest standards and generates an `AGENTS.md` file that all agents read automatically.

This ensures standards updates propagate consistently across projects without manual copy-paste.

## Documentation System

The documentation site (this site) is built with [MkDocs](https://www.mkdocs.org/) and the [Material theme](https://squidfunk.github.io/mkdocs-material/). It is automatically deployed to GitHub Pages via the `.github/workflows/docs.yml` CI workflow on every push to `main`.
