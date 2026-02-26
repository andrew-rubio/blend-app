# Architecture Overview

Blend is built around a multi-agent architecture where specialised GitHub Copilot agents collaborate through structured specification files. Each agent has a defined role, a specific model, and a bounded set of responsibilities.

## Component Map

```
blend-app/
├── .github/
│   ├── agents/          # Agent definitions (chat modes)
│   └── prompts/         # Workflow prompt files
├── .devcontainer/       # Dev Container configuration
├── .vscode/
│   └── mcp.json         # MCP server configuration
├── docs/                # This documentation site
├── specs/               # Generated specification files (per project)
│   ├── prd.md
│   ├── features/
│   └── tasks/
├── templates/           # Scaffolding templates
├── scripts/             # Installation scripts
├── apm.yml              # APM dependency configuration
└── mkdocs.yml           # Documentation site configuration
```

## Agents

Blend includes eight specialised agents:

| Agent | File | Responsibility |
|---|---|---|
| PM | `pm.agent.md` | Product requirements, PRDs, FRDs |
| Dev Lead | `devlead.agent.md` | Technical planning, task breakdown |
| Developer | `dev.agent.md` | Code implementation |
| Azure | `azure.agent.md` | Azure deployment, infrastructure |
| Rev-Eng | `rev-eng.agent.md` | Reverse engineering existing codebases |
| Modernizer | `modernizer.agent.md` | Modernisation planning |
| Planner | `planner.agent.md` | Multi-step research and planning |
| Architect | `architect.agent.md` | Standards, AGENTS.md management |

## MCP Servers

Agents interact with external systems via Model Context Protocol (MCP) servers configured in `.vscode/mcp.json`. These provide agents with real-time access to documentation, repository state, and browser automation.

## Specification Files

All inter-agent communication happens through files in the `specs/` directory:

- `specs/prd.md` — Product Requirements Document (created by PM agent)
- `specs/features/*.md` — Feature Requirements Documents (created by Dev Lead)
- `specs/tasks/*.md` — Implementation tasks (created by Planner/Dev Lead)

## Architecture Decision Records (ADRs)

Significant architectural decisions are documented as ADRs in `specs/adr/`. Each ADR follows the standard format:

- **Status**: Proposed / Accepted / Deprecated / Superseded
- **Context**: The situation requiring a decision
- **Decision**: What was decided
- **Consequences**: Trade-offs and implications

See [System Design](system-design.md) for the technical architecture details and [Data Flow](data-flow.md) for how information moves through the system.
