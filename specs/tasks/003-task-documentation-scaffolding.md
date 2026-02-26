# Task 003: Documentation Scaffolding

> **GitHub Issue:** [#3](https://github.com/andrew-rubio/blend-app/issues/3)

## Description

Set up the MkDocs documentation site following the AGENTS.md documentation standards. This includes the MkDocs Material theme configuration, required folder structure, placeholder pages for all required sections, and CI workflow for automated documentation deployment. The documentation site serves as the living reference for the Blend application.

## Dependencies

- None (can be done in parallel with other scaffolding tasks)

## Technical Requirements

### MkDocs configuration

- Create `mkdocs.yml` at the repository root following the exact configuration specified in AGENTS.md Documentation Standards section
- Configure the Material theme with light/dark mode toggle
- Enable required plugins: search, git-revision-date-localized, minify
- Enable required Markdown extensions: admonition, superfences, tabbed, highlight, tables, TOC with permalinks
- Configure the `nav` structure for all required documentation sections

### Documentation folder structure

Create the following folder and file structure under `docs/`:

- `docs/index.md` — Landing page with Blend project overview, features, architecture summary, and quick links
- `docs/getting-started/installation.md` — Development environment setup (prerequisites, clone, install, run)
- `docs/getting-started/quick-start.md` — 5-minute tutorial to get the app running locally
- `docs/getting-started/configuration.md` — Environment variables and configuration options
- `docs/architecture/overview.md` — High-level architecture (frontend, backend, database, external services)
- `docs/architecture/system-design.md` — Detailed system design (domain model, data flow, service interactions)
- `docs/architecture/data-flow.md` — Data flow diagrams (Mermaid) for key user flows
- `docs/api/rest-api.md` — REST API reference (placeholder linking to OpenAPI spec)
- `docs/guides/development.md` — Developer workflow, coding standards, branching strategy, PR process
- `docs/guides/deployment.md` — Deployment procedures (Azure SWA, Container Apps)
- `docs/guides/troubleshooting.md` — Common issues and solutions
- `docs/reference/configuration.md` — Full configuration reference
- `docs/reference/environment-variables.md` — Environment variable documentation

### Content requirements

- `docs/index.md` must summarise the Blend application based on the PRD
- `docs/architecture/overview.md` must reference the ADR decisions (frontend: Next.js, backend: ASP.NET Core .NET 9, database: Cosmos DB, auth: Identity + JWT, deployment: Container Apps + SWA)
- All other pages should have clear section headings and TODO placeholders for content to be filled during feature implementation

### Python requirements

- Create a `requirements-docs.txt` or configure in `pyproject.toml` with:
  - `mkdocs`
  - `mkdocs-material`
  - `mkdocs-git-revision-date-localized-plugin`
  - `mkdocs-minify-plugin`

### CI workflow

- Create `.github/workflows/docs.yml` for automated documentation deployment to GitHub Pages on push to main (when docs or mkdocs.yml change)

### .gitignore update

- Ensure the `site/` directory (MkDocs build output) is in `.gitignore`

## Acceptance Criteria

- [ ] `mkdocs serve` starts without errors and serves the documentation site on localhost
- [ ] `mkdocs build --strict` completes without warnings or errors
- [ ] All navigation links resolve to existing pages
- [ ] The documentation site has the Material theme with light/dark mode toggle
- [ ] Search functionality works across all documentation pages
- [ ] `docs/index.md` contains a meaningful project overview
- [ ] `docs/architecture/overview.md` references key ADR decisions
- [ ] All required documentation sections have placeholder files with proper headings
- [ ] The GitHub Actions docs workflow is present at `.github/workflows/docs.yml`
- [ ] `site/` directory is excluded from version control

## Testing Requirements

- Verify `mkdocs build --strict` passes as a CI check (this is the primary quality gate)
- Verify all internal links resolve (MkDocs strict mode catches broken links)
- Verify navigation structure matches the `mkdocs.yml` nav configuration
- No additional unit tests required for documentation scaffolding
