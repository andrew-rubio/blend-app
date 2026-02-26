# Task 032: Cross-Cutting Concerns — Error Handling, Logging, and Graceful Degradation

> **GitHub Issue:** [#43](https://github.com/andrew-rubio/blend-app/issues/43)

## Description

Implement cross-cutting concerns that apply to the entire application per the Platform FRD (PLAT-01 through PLAT-14, PLAT-53 through PLAT-56) and the operational requirements in the PRD (REQ-50 through REQ-59). This covers global error handling, structured logging, health checks, rate limiting, and graceful degradation when external services are unavailable.

## Dependencies

- **001-task-backend-scaffolding** — requires the API project with middleware pipeline
- **002-task-frontend-scaffolding** — requires the Next.js project with error boundaries
- **008-task-spoonacular-integration** — requires the Spoonacular service for degradation scenarios
- **016-task-ingredient-kb-backend** — requires the KB service for degradation scenarios

## Technical Requirements

### Backend — Global error handling (PLAT-01 through PLAT-03)

- Consistent error response format across all endpoints using RFC 9457 Problem Details
- Global exception middleware catches unhandled exceptions and returns structured error responses
- Map known exception types to appropriate HTTP status codes:
  - `ValidationException` → 400 with field-level error details
  - `UnauthorisedException` → 401
  - `ForbiddenException` → 403
  - `NotFoundException` → 404
  - `ConflictException` → 409
  - `RateLimitException` → 429 with `Retry-After` header
  - Unhandled exceptions → 500 with a correlation ID (no stack trace in production)
- Correlation ID (`X-Correlation-Id`) propagated through all requests for distributed tracing

### Backend — Structured logging (PLAT-53 through PLAT-56)

- Use structured logging (JSON format) with a logging framework
- Log levels: Debug, Information, Warning, Error, Critical
- Log context includes: correlation ID, user ID (if authenticated), endpoint, HTTP method
- Log all:
  - Request/response metadata (not bodies in production — opt-in for debugging)
  - Authentication events (login, logout, failed attempts)
  - External service calls (Spoonacular, Azure AI Search) with duration
  - Error details with stack traces (Error level only)
- Health check endpoints:
  - `GET /health` — basic liveness probe
  - `GET /health/ready` — readiness probe checking Cosmos DB and external service connectivity

### Backend — Rate limiting (PLAT-07, PLAT-08)

- Apply rate limiting middleware:
  - Anonymous users: 30 requests/minute
  - Authenticated users: 100 requests/minute
  - Admin users: no rate limit
- Return 429 Too Many Requests with `Retry-After` header when exceeded
- Rate limit by IP for anonymous, by user ID for authenticated

### Backend — Graceful degradation (PLAT-12 through PLAT-14)

- When Spoonacular is unavailable (quota exhausted or service down):
  - Search returns only internal results with `degradedMode: true` in response metadata
  - Home page featured recipes fall back to cached versions
  - No error shown to users — transparent fallback
- When Knowledge Base is unavailable:
  - Cook Mode disables smart suggestions with informational message (REQ-66)
  - Ingredient search returns empty results with `kbUnavailable: true`
  - Recipe detail still shows basic ingredient data (from recipe document, not KB)
- Health check endpoints report service status: `{ cosmosDb, spoonacular, knowledgeBase }`

### Frontend — Error boundaries (PLAT-04 through PLAT-06)

- React Error Boundary components at:
  - Root level — catches catastrophic errors, shows "Something went wrong" with retry
  - Page level — catches page-specific errors, allows navigation to other pages
  - Section level — catches section-specific errors in home page, shows "Could not load this section" with retry
- Toast notification system for transient errors (network timeout, save failure)
- User-friendly error messages (no technical jargon)

### Frontend — Offline and connectivity handling

- Detect offline status and show a banner: "You are offline. Some features may be unavailable."
- Queue failed mutations for retry when connectivity is restored (via TanStack Query retry)
- Display cached data when available and offline

### Frontend — Loading states

- Consistent loading patterns across the app:
  - Page-level skeleton screens
  - Section-level skeleton blocks (home page sections)
  - Inline loading spinners for actions (like, share, save)
  - Button loading state (disabled with spinner) for form submissions

## Acceptance Criteria

- [ ] All API errors return RFC 9457 Problem Details with correlation IDs
- [ ] Unhandled exceptions return 500 without leaking implementation details
- [ ] Structured logging captures request metadata, auth events, and external calls
- [ ] Health check endpoints report service connectivity status
- [ ] Rate limiting enforces per-tier limits with correct 429 responses
- [ ] Spoonacular unavailability triggers transparent fallback to internal results
- [ ] KB unavailability disables smart suggestions with informational messaging
- [ ] React error boundaries catch and display errors at appropriate levels
- [ ] Toast notifications show for transient errors with retry option
- [ ] Offline banner appears when connectivity is lost
- [ ] Consistent loading states (skeletons, spinners) are used throughout the app

## Testing Requirements

- Unit tests for error middleware (exception type → status code mapping)
- Unit tests for correlation ID propagation
- Unit tests for rate limiting logic (per-tier, cooldown)
- Unit tests for degradation decision logic (Spoonacular, KB)
- Component tests for React error boundaries (error trigger, recovery)
- Component tests for offline banner and connectivity detection
- Component tests for loading state components (skeleton, spinner)
- Integration test for health check endpoints
- Integration test for rate limiting (exceed threshold → 429)
- Integration test for Spoonacular degradation (mock failure → internal-only results)
- Integration test for KB degradation (mock failure → suggestions disabled)
- Minimum 85% code coverage
