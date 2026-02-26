# Configuration Reference

This page documents all configuration files used by Blend.

## `mkdocs.yml`

Controls the documentation site. Key settings:

| Key | Description | Default |
|---|---|---|
| `site_name` | Site title displayed in the header | `Blend` |
| `site_url` | Canonical URL for the deployed site | — |
| `repo_url` | Link to the GitHub repository | — |
| `theme.name` | MkDocs theme | `material` |
| `plugins` | Enabled MkDocs plugins | `search`, `git-revision-date-localized`, `minify` |

### Plugins

| Plugin | Purpose |
|---|---|
| `search` | Full-text search |
| `git-revision-date-localized` | Shows last-edited date on each page |
| `minify` | Minifies HTML output for production |

### Extensions

| Extension | Purpose |
|---|---|
| `admonition` | Note, warning, tip, danger callout blocks |
| `pymdownx.superfences` | Nested fenced code blocks |
| `pymdownx.tabbed` | Tabbed content blocks |
| `pymdownx.highlight` | Syntax highlighting with line numbers |
| `tables` | Markdown tables |
| `toc` | Table of contents with permalink anchors |

## `apm.yml`

Manages coding standards as versioned APM dependencies.

| Key | Description |
|---|---|
| `name` | Project name |
| `version` | Project version |
| `dependencies.apm` | List of APM standard packages to install |
| `scripts` | Shortcut commands runnable with `apm run <name>` |

## `.vscode/mcp.json`

Configures MCP servers for agent tool access. Each server entry specifies a `command` and `args` for launching the server process.

## `.devcontainer/devcontainer.json`

Standard Dev Container configuration. Key fields:

| Field | Description |
|---|---|
| `image` | Base Docker image |
| `features` | Dev Container features to install |
| `customizations.vscode.extensions` | VS Code extensions to pre-install |
| `postCreateCommand` | Script run after container creation |
