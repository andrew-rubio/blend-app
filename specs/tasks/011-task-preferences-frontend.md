# Task 011: User Preferences — Frontend

> **GitHub Issue:** [#10](https://github.com/andrew-rubio/blend-app/issues/10)

## Description

Implement the frontend UI for user preferences selection and management per the User Preferences FRD (REQ-5 through REQ-8, PREF-01 through PREF-18). This covers the onboarding preference wizard shown after first registration and the editable preference screens accessible from App Settings.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project with routing and state management
- **007-task-auth-frontend** — requires the auth flow (preferences are set post-registration)
- **010-task-preferences-backend** — requires the preference API endpoints

## Technical Requirements

### Onboarding wizard

- Multi-step wizard presented after first registration or when preferences are empty (PREF-01, PREF-05, PREF-11)
- Steps:
  1. **Cuisine selection** — grid/chip layout of available cuisines, multi-select (PREF-01)
  2. **Dish type selection** — grid/chip layout of available dish types, multi-select (PREF-02)
  3. **Dietary preferences** — list of diets with descriptions, multi-select (PREF-05)
  4. **Intolerances** — list of intolerances with clear "these will be strictly excluded" warning (PREF-06, PREF-07)
  5. **Disliked ingredients** — typeahead search to add specific ingredients (PREF-11)
- Each step is skippable (PREF-17)
- Progress indicator showing current step and total steps
- Back navigation between steps
- Final "Save & Continue" submits all preferences in a single API call

### Selection UI

- Cuisines and dish types: visual chip/card grid with icons or representative images (PREF-03)
- Diets: list items with name and short description
- Intolerances: list items with clear severity indicator (strict exclusion label)
- Disliked ingredients: typeahead input with autocomplete and added items displayed as removable chips (PREF-12)
- All selections show a clear selected/unselected state (PREF-04)

### State management

- Store preference selections in Zustand during the wizard flow
- On save, call `PUT /api/v1/users/me/preferences` and update local state on success
- On settings edit, fetch current preferences from `GET /api/v1/users/me/preferences` and pre-populate the form
- Handle API errors with clear user-facing messages

### Settings integration

- Provide a "Manage Preferences" screen accessible from App Settings (PREF-15)
- Same selection UI as onboarding but pre-populated with current saved values
- Individual sections can be edited independently (inline save per section or save all)
- Show confirmation before clearing all preferences

### Immediate feedback

- After saving preferences, any search or recommendation visible on screen refreshes to reflect the new settings (PREF-18)
- Invalidate TanStack Query caches for search results and home page recommendations on preference save

## Acceptance Criteria

- [ ] Multi-step onboarding wizard is displayed after first registration
- [ ] User can select multiple cuisines from a visual grid
- [ ] User can select multiple dish types from a visual grid
- [ ] User can select multiple dietary preferences with descriptions
- [ ] User can select intolerances with a clear "strict exclusion" warning
- [ ] User can search and add disliked ingredients via typeahead
- [ ] Each step is independently skippable
- [ ] Back navigation works between wizard steps
- [ ] Preferences are persisted via the API on final save
- [ ] Preferences screen in Settings shows current saved values
- [ ] Updated preferences immediately reflect in search results and recommendations
- [ ] Loading and error states are handled gracefully

## Testing Requirements

- Component tests for each wizard step (render, selection, deselection)
- Component test for the full wizard flow (step navigation, save)
- Component test for typeahead ingredient search and chip management
- Integration test for preference save round-trip (mock API)
- Integration test for cache invalidation on preference update
- Accessibility tests (keyboard navigation, screen reader labels)
- Minimum 85% code coverage
