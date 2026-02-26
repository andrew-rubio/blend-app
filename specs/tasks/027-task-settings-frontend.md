# Task 027: App Settings — Frontend

> **GitHub Issue:** [#38](https://github.com/andrew-rubio/blend-app/issues/38)

## Description

Implement the App Settings page per the App Settings FRD (SETT-01 through SETT-24). This includes preference management access, ingredient catalogue and submission, unit toggle, splash replay, share app functionality, and account deletion flow.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project
- **007-task-auth-frontend** — requires authentication state
- **011-task-preferences-frontend** — requires the preference management screens (linked from settings)
- **026-task-notifications-settings-backend** — requires the settings and deletion API endpoints

## Technical Requirements

### Settings page

- Route: `/settings`
- Grouped list of settings sections:

### Preference management (SETT-01 through SETT-03)

- "Manage Preferences" row navigates to the preference editing screen (built in task 011)
- Shows a summary of current preferences (e.g., "3 cuisines, 2 diets, 1 intolerance")

### Ingredient catalogue (SETT-04 through SETT-07)

- "Ingredient Catalogue" row navigates to a browsable ingredient list
- Search input for finding ingredients in the Knowledge Base
- Each ingredient shows: name, category, flavour profile
- Tapping an ingredient opens the detail view (from Cook Mode KB integration)

### Ingredient submission (SETT-08 through SETT-12)

- "Submit New Ingredient" action opens a submission form
- Form fields: ingredient name, category (dropdown), description
- Submission confirmation message
- "My Submissions" list showing status of previous submissions (pending, approved, rejected)

### Unit toggle (SETT-13 through SETT-16)

- Toggle or segmented control: "Metric" / "Imperial"
- Saved to `PUT /api/v1/settings`
- Stored in Zustand for immediate local effect
- All ingredient amounts across the app display in the selected unit system

### Splash screen replay (SETT-05 through SETT-07)

- "Replay Introduction" row replays the onboarding splash intro
- Navigates to the splash flow from task 007 without resetting other state

### Share app (SETT-17)

- "Share Blend" action opens the system share sheet
- Shares a predefined message with the app store link (or web URL)

### Account deletion (SETT-17 through SETT-24)

- "Delete Account" row at the bottom of settings in a danger zone section
- Tapping opens a multi-step deletion flow:
  1. Warning screen explaining consequences (all data will be permanently removed)
  2. Re-authentication step (password entry or OAuth re-consent)
  3. Confirmation: "Type DELETE to confirm"
  4. After confirmation, call `POST /api/v1/users/me/delete-request`
  5. Success screen: "Your account will be deleted in 30 days. You can cancel within this period."
- Cancellation option: if the user logs back in during the grace period, show a banner with "Cancel deletion" action

### Danger zone styling

- Account deletion section visually separated with a red/danger accent
- Clear warning text about the irreversibility after the grace period

## Acceptance Criteria

- [ ] Settings page displays all sections with correct navigation
- [ ] "Manage Preferences" links to the preference editing screen
- [ ] Ingredient catalogue is browsable with search
- [ ] New ingredient submissions can be submitted and tracked
- [ ] Unit toggle switches between Metric and Imperial and takes immediate effect
- [ ] Splash replay navigates to the intro flow
- [ ] Share app opens the system share sheet
- [ ] Account deletion flow requires re-authentication and explicit confirmation
- [ ] Deletion request shows the 30-day grace period notice
- [ ] Deletion can be cancelled within the grace period via a banner
- [ ] Danger zone section is visually distinct

## Testing Requirements

- Component tests for each settings row (render, navigation)
- Component test for unit toggle (state change, persistence)
- Component test for ingredient submission form (validation, submission)
- Component test for account deletion multi-step flow (warning → auth → confirm → success)
- Component test for deletion cancellation banner
- Integration test for settings page load with current user preferences
- Integration test for account deletion round-trip (mock API)
- Accessibility tests (keyboard navigation, danger zone contrast, focus management)
- Minimum 85% code coverage
