# Task 006: Authentication and Authorisation — Backend

> **GitHub Issue:** [#6](https://github.com/andrew-rubio/blend-app/issues/6)

## Description

Implement the full authentication and authorisation backend per ADR 0004 (ASP.NET Core Identity + JWT). This includes user registration (email and social OAuth), login/logout, password reset, JWT token issuance and refresh, social login integration (Google, Facebook, Twitter/X), role-based authorisation (user/admin), and account enumeration prevention.

## Dependencies

- **001-task-backend-scaffolding** — requires the API project and middleware pipeline
- **005-task-database-setup** — requires the User entity and Cosmos DB repository

## Technical Requirements

### ASP.NET Core Identity with Cosmos DB

- Implement a custom `IUserStore<BlendUser>` backed by Cosmos DB (not Entity Framework)
- Configure ASP.NET Core Identity with password policy enforcement: minimum length, character variety (per AUTH-08)
- Support role management (user, admin) with a custom `IRoleStore<BlendRole>`

### JWT token management

- Issue JWT access tokens on successful login (lifetime: 15-30 minutes, configurable)
- Store access tokens in memory on the client side (returned in response body)
- Issue refresh tokens stored in HTTP-only, Secure, SameSite cookies
- Implement a token refresh endpoint that issues a new access/refresh token pair
- Sign tokens with a configurable signing key (stored in app configuration / Azure Key Vault)
- Include user ID, email, display name, and roles in the JWT claims

### API endpoints

- `POST /api/v1/auth/register` — email registration (name, email, password). Returns JWT tokens on success. Directs to preference flow (AUTH-09, AUTH-10)
- `POST /api/v1/auth/login` — email login. Returns JWT tokens on success (AUTH-11)
- `POST /api/v1/auth/login/{provider}` — social login initiation (Google, Facebook, Twitter). Redirects to OAuth flow (AUTH-06, AUTH-12)
- `GET /api/v1/auth/callback/{provider}` — OAuth callback handler. Creates account on first use, logs in on subsequent uses. Returns JWT tokens
- `POST /api/v1/auth/refresh` — refresh token rotation. Accepts refresh token from cookie, returns new token pair
- `POST /api/v1/auth/logout` — invalidates refresh token, clears cookie (AUTH-15)
- `POST /api/v1/auth/forgot-password` — initiates password reset (AUTH-17). Always returns success to prevent enumeration (AUTH-21)
- `POST /api/v1/auth/reset-password` — completes password reset with token + new password (AUTH-19)

### Social OAuth integration

- Configure ASP.NET Core external authentication middleware for:
  - Google (OAuth 2.0)
  - Facebook (OAuth 2.0)
  - Twitter/X (OAuth 2.0)
- Client IDs and secrets stored in configuration (not hardcoded)
- On first social login, create a new user account and link the external provider
- On subsequent social logins, match the existing account and issue tokens

### Security requirements

- Failed login must return a generic error message: "Invalid email or password" — no account enumeration (AUTH-14, AUTH-21)
- Password reset endpoint must return a generic message regardless of whether the email exists (AUTH-21)
- Rate-limit authentication endpoints to prevent brute force attacks
- Refresh token rotation: issuing a new refresh token invalidates the previous one
- JWT signing key must not be committed to source control

### Authorisation

- Configure role-based authorisation policies:
  - `RequireAuthenticated` — any logged-in user
  - `RequireAdmin` — admin role only
- Apply authorisation attributes/policies to endpoint groups
- Create an admin user seed mechanism (e.g., seed script or configuration-based)

### Email service (abstraction)

- Define an `IEmailService` interface for sending password reset emails
- Implement a development stub that logs to console
- The production implementation (e.g., SendGrid, Azure Communication Services) is out of scope for this task but the interface must be in place

## Acceptance Criteria

- [ ] Email registration creates a user and returns valid JWT tokens
- [ ] Social login (Google, Facebook, Twitter) creates/links accounts and returns tokens
- [ ] Login with correct credentials returns valid JWT tokens
- [ ] Login with incorrect credentials returns a generic error (no enumeration)
- [ ] Access token is validated on protected endpoints (returns 401 if missing/expired)
- [ ] Refresh token rotation issues new tokens and invalidates the old refresh token
- [ ] Logout clears the refresh token cookie
- [ ] Password reset sends an email (logged in dev) with a time-limited token
- [ ] Password reset with a valid token allows setting a new password
- [ ] Password reset with an expired/invalid token returns an error
- [ ] Admin-only endpoints reject non-admin users with 403
- [ ] All auth endpoints return Problem Details for errors

## Testing Requirements

- Unit tests for JWT token generation and validation (claims, expiry, signing)
- Unit tests for password policy enforcement (various invalid passwords)
- Unit tests for account enumeration prevention (login and password reset responses)
- Integration tests for the complete registration → login → refresh → logout flow
- Integration tests for social login callback handling (mocked OAuth provider)
- Integration tests for password reset flow (forgot → reset → login with new password)
- Unit tests for role-based authorisation policy evaluation
- Minimum 85% code coverage for all authentication and authorisation code
