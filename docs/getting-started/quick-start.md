# Quick Start

Get up and running with Blend in minutes. This guide walks through the most common workflow: building a new application from a product idea.

## Before You Begin

Complete the [Installation](installation.md) steps and ensure you have:

- VS Code open with the Dev Container running
- GitHub Copilot Chat active
- An Azure subscription (for deployment steps)

## Greenfield Workflow (New Application)

### Step 1: Define Your Product Idea

Open Copilot Chat and use the `/prd` prompt to create a Product Requirements Document:

```
/prd I want to build a task management app for small teams
```

The PM agent will ask clarifying questions and produce a structured `specs/prd.md`.

### Step 2: Break Down Features

Use `/frd` to generate Feature Requirements Documents:

```
/frd
```

The Dev Lead agent reads your PRD and creates detailed FRDs in `specs/features/`.

### Step 3: Create Technical Tasks

Use `/plan` to generate an implementation task breakdown:

```
/plan
```

Tasks are written to `specs/tasks/` with numbered files ready for implementation.

### Step 4: Implement Features

Use `/implement` or `/delegate` to start coding:

```
/implement specs/tasks/001-task-setup.md
```

### Step 5: Deploy to Azure

Use `/deploy` when your application is ready:

```
/deploy
```

## Brownfield Workflow (Existing Codebase)

If you have an existing codebase, use the reverse-engineering workflow:

```
/rev-eng
```

The Rev-Eng agent will analyse your code and generate `specs/prd.md` and architecture documentation.

## Next Steps

- [Configuration](configuration.md) — Tailor Blend to your project's needs
- [Architecture Overview](../architecture/overview.md) — Understand how agents coordinate
- [Development Guide](../guides/development.md) — Contribute to Blend itself
