# Task 002: Frontend Project Scaffolding

> **GitHub Issue:** [#2](https://github.com/andrew-rubio/blend-app/issues/2)

## Description

Set up the Next.js frontend project (`Blend.Web`) following ADR 0001 and AGENTS.md standards. This includes the App Router structure, TypeScript configuration, Tailwind CSS, state management setup, API client generation from the backend OpenAPI spec, component library foundations, and development tooling (linting, formatting, testing framework).

## Dependencies

- **001-task-backend-scaffolding** — the backend must be running and serving the OpenAPI spec so the TypeScript client can be generated

## Technical Requirements

### Next.js project (`Blend.Web`)

- Create a Next.js application using the App Router (not Pages Router) with TypeScript strict mode enabled
- Use the latest stable Next.js version
- Configure `next.config.js` with `output: 'standalone'` for Azure Static Web Apps deployment compatibility (per ADR 0008)
- Set up the project within the monorepo at the appropriate path

### TypeScript configuration

- Enable strict mode (`strict: true`) in `tsconfig.json`
- Configure path aliases for clean imports (e.g., `@/components`, `@/lib`, `@/hooks`, `@/types`)

### Styling

- Configure Tailwind CSS with a design system foundation:
  - Colour palette (primary, secondary, accent, semantic colours)
  - Typography scale
  - Spacing scale
  - Breakpoints for responsive design (mobile, tablet, desktop per REQ-50)
- Set up CSS reset / base styles
- Configure dark mode support (class-based toggle)

### State management

- Set up Zustand for client-side global state (e.g., auth state, notification count, Cook Mode session)
- Set up TanStack Query (React Query) for server state management (API data fetching, caching, mutations)
- Create typed query client configuration with default stale times and retry policies

### API client generation

- Set up Microsoft Kiota (or openapi-typescript-codegen) to generate a typed TypeScript API client from the backend's OpenAPI spec
- Configure the generation as a build/dev script (e.g., `npm run generate-api`)
- The generated client should be output to a dedicated directory (e.g., `src/lib/api/generated/`)

### Component architecture

- Create a component folder structure:
  - `src/components/ui/` — reusable primitive UI components (Button, Input, Card, Modal, etc.)
  - `src/components/layout/` — layout components (Header, Footer, Navigation, PageLayout)
  - `src/components/features/` — feature-specific composite components
- Create placeholder layout components: root layout with metadata, global navigation placeholder, and error boundary

### App Router structure

- Set up route groups for the main sections:
  - `(auth)` — login, register, password reset
  - `(main)` — home, explore, cook, profile, settings
  - `(admin)` — admin content management (protected)
- Set up a root `layout.tsx` with shared providers (QueryClientProvider, Zustand stores)
- Set up a global `error.tsx` and `not-found.tsx`
- Set up a root `loading.tsx` with skeleton/spinner

### Development tooling

- Configure ESLint with Next.js recommended rules + strict TypeScript rules
- Configure Prettier for code formatting
- Set up Vitest (or Jest) with React Testing Library for unit and component testing
- Configure pre-commit hooks (lint-staged + husky) for linting and formatting
- Add npm scripts: `dev`, `build`, `start`, `lint`, `test`, `test:coverage`, `generate-api`

### Environment configuration

- Set up `.env.local.example` with placeholder variables:
  - `NEXT_PUBLIC_API_URL` — backend API base URL
  - `NEXT_PUBLIC_APP_URL` — frontend app URL (for social previews)

## Acceptance Criteria

- [ ] `npm run dev` starts the Next.js dev server without errors
- [ ] `npm run build` produces a production build without errors or type errors
- [ ] `npm run lint` passes with zero warnings or errors
- [ ] `npm run test` runs the test suite (even if only placeholder tests)
- [ ] `npm run generate-api` generates a typed TypeScript client from the backend OpenAPI spec
- [ ] TypeScript strict mode is enforced — no `any` types allowed without explicit override
- [ ] Tailwind CSS classes are applied and working (verified with a placeholder component)
- [ ] App Router structure has route groups for auth, main, and admin
- [ ] Root layout includes QueryClientProvider and Zustand store providers
- [ ] Global error boundary (`error.tsx`) renders a user-friendly error page
- [ ] 404 page (`not-found.tsx`) renders a user-friendly not-found page
- [ ] ESLint and Prettier configurations are consistent and enforced via pre-commit hooks

## Testing Requirements

- Unit test for Zustand store setup (initial state, basic state mutations)
- Unit test for TanStack Query client configuration (default options)
- Component test for the root layout rendering children correctly
- Component test for error boundary rendering the error UI
- Component test for 404 page rendering
- Minimum 85% code coverage for all custom utility code
- Snapshot or visual regression test for at least one UI primitive component (e.g., Button)
