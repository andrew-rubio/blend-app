# Task 007: Authentication and Onboarding — Frontend

> **GitHub Issue:** [#1](https://github.com/andrew-rubio/blend-app/issues/1)

## Description

Implement the frontend authentication flows and onboarding experience: splash intro walkthrough, email registration form, social login buttons, login form, password reset flow, and post-registration redirect to the preference-setting flow. This covers AUTH-01 through AUTH-21 from the Onboarding & Authentication FRD.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project, App Router structure, and API client
- **006-task-auth-backend** — requires the auth API endpoints to be available

## Technical Requirements

### Splash intro (REQ-1)

- Implement a multi-step walkthrough overlay/carousel that explains Blend's key features (AUTH-01)
- Show the splash intro on first visit (use local storage flag to track first visit) (AUTH-01)
- Make the splash intro dismissible (AUTH-02)
- After dismissal, present options: Register, Log In, or Continue as Guest (AUTH-04)
- Re-accessible from App Settings (AUTH-03) — create a reusable component that can be triggered from settings

### Registration page (REQ-2)

- Email registration form with fields: display name, email, password, confirm password (AUTH-05)
- Real-time form validation: email format, password strength indicator, password match
- Password requirements displayed clearly to the user (AUTH-08)
- Social login buttons for Google, Facebook, and Twitter/X (AUTH-06)
- Submit calls the backend registration endpoint; on success, store tokens and redirect to preference flow (AUTH-09)
- Error handling: display specific error messages for validation failures (AUTH-07)
- "Skip preferences" option available to go directly to Home (AUTH-10)

### Login page (REQ-3)

- Login form with email and password fields (AUTH-11)
- Social login buttons (AUTH-12)
- "Forgot your password?" link (AUTH-17)
- Submit calls the backend login endpoint; on success, store tokens, load user data, redirect to Home (AUTH-13)
- Error handling: display the generic error from the backend (AUTH-14)
- "Don't have an account? Register" link

### Logout

- Logout action callable from any screen (AUTH-15)
- Calls backend logout endpoint to clear refresh token
- Clears client-side auth state (Zustand store)
- Redirects to the login/registration screen (AUTH-16)

### Password reset (REQ-4)

- "Forgot password" page: email input form (AUTH-17)
- On submit, always show generic success message regardless of backend response (AUTH-21)
- Reset password page (accessed via email link): new password + confirm password form (AUTH-19)
- Password validation with same rules as registration (AUTH-19)
- On success, show confirmation and redirect to login (AUTH-20)
- Handle expired/invalid tokens with a clear error message

### Auth state management

- Zustand store for auth state: user object (id, name, email, role), access token, isAuthenticated flag
- Automatic token refresh: intercept 401 responses, call refresh endpoint, retry the original request
- Auth-aware route protection: middleware or layout-level guard that redirects unauthenticated users to login when accessing protected routes
- Persist auth state across page refreshes (refresh token in cookie handles re-authentication)

### Guest access (REQ-52)

- Guest users can browse Home, search recipes, view recipe details, and browse ingredient catalogue (PLAT-09)
- When a guest attempts a restricted action (Cook Mode, like, preferences, social), show a login/register prompt modal (PLAT-11)

## Acceptance Criteria

- [ ] First-time visitors see the splash intro walkthrough
- [ ] Splash intro is not shown again on subsequent visits (unless accessed from settings)
- [ ] Email registration form validates inputs and creates an account successfully
- [ ] Social login buttons initiate the OAuth flow and complete registration/login
- [ ] Login with valid credentials stores tokens and redirects to Home
- [ ] Login with invalid credentials shows a generic error message
- [ ] Logout clears state and redirects to login screen
- [ ] Password reset flow sends email and allows setting a new password
- [ ] Protected routes redirect unauthenticated users to login
- [ ] Guest users can browse public content without login
- [ ] Guest users see a login prompt when attempting restricted actions
- [ ] Token refresh happens transparently when the access token expires

## Testing Requirements

- Component tests for splash intro (renders, dismisses, tracks first-visit state)
- Component tests for registration form (validation, submission, error display)
- Component tests for login form (submission, error display)
- Component tests for password reset forms (both initiation and reset)
- Unit tests for auth Zustand store (login, logout, token refresh state transitions)
- Unit tests for route protection logic (authenticated vs. unauthenticated redirects)
- Component tests for guest access prompt modal
- Integration test for the complete registration → preferences redirect flow
- Minimum 85% code coverage for all auth-related components and utilities
