# Troubleshooting

This page covers common issues and how to resolve them.

## Dev Container Issues

### Dev Container fails to build

**Symptom:** VS Code shows an error when reopening in the container.

**Solutions:**

1. Ensure Docker Desktop is running
2. Check available disk space (at least 10 GB recommended)
3. Try rebuilding the container: **Command Palette** → `Dev Containers: Rebuild Container`
4. Check the Dev Container logs for specific errors

### Extensions not loading

**Symptom:** GitHub Copilot Chat or other extensions are not available inside the container.

**Solution:** Rebuild the container to reinstall extensions:

```
Dev Containers: Rebuild Container Without Cache
```

## Copilot Agent Issues

### Agent not responding

**Symptom:** `@agent-name` does not trigger a response or shows an error.

**Solutions:**

1. Ensure GitHub Copilot Chat is signed in and active
2. Check that the `.github/agents/` directory contains the agent file
3. Reload VS Code: `Ctrl+Shift+P` → `Developer: Reload Window`

### Agent produces incomplete output

**Symptom:** The agent stops mid-response or produces truncated files.

**Solutions:**

1. Break your request into smaller pieces
2. Ask the agent to continue: "Please continue from where you left off"
3. Check that the model context window has not been exceeded

## MCP Server Issues

### MCP server not connecting

**Symptom:** Agents report they cannot access external documentation or tools.

**Solutions:**

1. Check `.vscode/mcp.json` for correct server configuration
2. Restart VS Code to re-initialise MCP connections
3. Check network connectivity from inside the Dev Container

## Documentation Build Issues

### `mkdocs build --strict` fails

**Symptom:** The documentation build fails with warnings treated as errors.

**Solutions:**

1. Check for broken internal links — all `[text](path.md)` links must resolve
2. Check for malformed Markdown (unclosed admonitions, invalid fences)
3. Run `mkdocs serve` for live preview to identify issues interactively

### `mkdocs serve` shows 404 for pages

**Symptom:** A page exists in `docs/` but returns 404 in the dev server.

**Solution:** Ensure the page is listed in the `nav:` section of `mkdocs.yml`.

## Azure Deployment Issues

See the [Deployment Guide](deployment.md) for deployment-specific troubleshooting.

## Getting Help

If you cannot resolve an issue, please [open a GitHub issue](https://github.com/andrew-rubio/blend-app/issues) with:

- A description of the problem
- Steps to reproduce
- Relevant error messages or logs
