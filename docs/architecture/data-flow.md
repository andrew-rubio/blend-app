# Data Flow

This page describes how information flows through the Blend system during a typical greenfield development workflow.

## Greenfield Workflow Data Flow

```
User Idea
    │
    ▼
/prd prompt ──► PM Agent ──► specs/prd.md
                                  │
                                  ▼
/frd prompt ──► Dev Lead Agent ──► specs/features/*.md
                                         │
                                         ▼
/plan prompt ──► Planner Agent ──► specs/tasks/*.md
                                         │
                                         ▼
/implement ──► Developer Agent ──► Source Code
                                         │
                                         ▼
/deploy ──► Azure Agent ──► Deployed Application
```

## Specification File Lifecycle

1. **PRD** (`specs/prd.md`) — Created by the PM agent from user input. Treated as the source of truth for product requirements.
2. **FRDs** (`specs/features/*.md`) — Created by the Dev Lead agent from the PRD. Each FRD describes one feature in implementation-ready detail.
3. **Tasks** (`specs/tasks/*.md`) — Created by the Planner or Dev Lead from FRDs. Numbered sequentially; each task is a self-contained unit of work for the Developer agent.

## Agent Communication

Agents do not communicate directly. All coordination happens through files:

- Agents **read** upstream specification files as context
- Agents **write** output files for downstream agents to consume
- VS Code's file system and the MCP GitHub server provide shared state

## MCP Data Flow

MCP servers augment agents with real-time external data:

| MCP Server | Data Provided |
|---|---|
| `context7` | Library documentation fetched on demand |
| `github` | Repository file contents, issues, PRs |
| `microsoft.docs.mcp` | Azure service documentation |
| `playwright` | Web page content via browser automation |
| `deepwiki` | Summaries of external GitHub repositories |

## Brownfield Workflow Data Flow

```
Existing Codebase
    │
    ▼
/rev-eng ──► Rev-Eng Agent ──► specs/prd.md
                             └► docs/architecture/*.md
                                         │
                                         ▼
/modernize ──► Modernizer Agent ──► specs/modernization-plan.md
                                              │
                                              ▼
/plan ──► Planner Agent ──► specs/tasks/*.md
```
