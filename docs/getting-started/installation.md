# Installation

This page covers the different ways to install and set up Blend in your environment.

## Prerequisites

Before you begin, ensure you have:

- Git
- [VS Code](https://code.visualstudio.com/) with the [GitHub Copilot Chat extension](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat)
- Docker Desktop (for the Dev Container option)
- Python 3.10+ (for local MkDocs documentation)

## Option 1: Use as a Template (Recommended)

The fastest way to get started is to use this repository as a GitHub template:

1. Click **Use this template** on the [GitHub repository](https://github.com/andrew-rubio/blend-app)
2. Create a new repository under your account or organisation
3. Clone your new repository
4. Open it in VS Code and reopen in the Dev Container when prompted

## Option 2: Install into an Existing Project

Use the install script to add Blend agents and prompts to any existing project:

```bash
curl -fsSL https://raw.githubusercontent.com/andrew-rubio/blend-app/main/scripts/install.sh | bash
```

For Windows (PowerShell):

```powershell
Invoke-RestMethod https://raw.githubusercontent.com/andrew-rubio/blend-app/main/scripts/install.ps1 | Invoke-Expression
```

## Option 3: Manual Installation

1. Download the latest release package from the [Releases page](https://github.com/andrew-rubio/blend-app/releases)
2. Extract the archive
3. Copy the `.github` folder to your project root

## Verifying the Installation

Once installed, open VS Code with GitHub Copilot Chat and verify you can access the agents:

```
@blend Hello
```

You should see the Blend agents available in the Copilot Chat panel.

## Next Steps

- [Quick Start](quick-start.md) — Run your first workflow
- [Configuration](configuration.md) — Customise Blend for your project
