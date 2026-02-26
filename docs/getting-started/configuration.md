# Configuration

Blend is configured through a combination of files at the repository root. This page explains each configuration file and the options available.

## `apm.yml`

The APM configuration file manages coding standards as versioned dependencies:

```yaml
name: my-app
version: 1.0.0
description: My application

dependencies:
  apm:
    - EmeaAppGbb/spec2cloud-guidelines
    - EmeaAppGbb/spec2cloud-guidelines-backend
    - EmeaAppGbb/spec2cloud-guidelines-frontend
```

Run `apm install` to pull the latest standards into your project as `AGENTS.md`.

## `.vscode/mcp.json`

Configures the Model Context Protocol (MCP) servers available to agents:

| Server | Purpose |
|---|---|
| `context7` | Up-to-date library documentation |
| `github` | Repository management and operations |
| `microsoft.docs.mcp` | Official Microsoft/Azure documentation |
| `playwright` | Browser automation |
| `deepwiki` | External repository context |

## `.devcontainer/devcontainer.json`

Defines the development container environment. The default container includes:

- Python 3.12
- Azure CLI and Azure Developer CLI (`azd`)
- TypeScript / Node.js
- Docker-in-Docker
- Pre-installed VS Code extensions

## Environment Variables

See the [Environment Variables reference](../reference/environment-variables.md) for all supported variables.

## MkDocs Documentation

The documentation site is configured in `mkdocs.yml` at the repository root. See the [Configuration reference](../reference/configuration.md) for details.
