# Task 018: Cook Mode Workspace — Frontend

> **GitHub Issue:** [#29](https://github.com/andrew-rubio/blend-app/issues/29)

## Description

Implement the Cook Mode workspace UI per the Cook Mode FRD (COOK-01 through COOK-29). This is the interactive ingredient-based cooking workspace where users add ingredients, receive smart pairing suggestions, manage multiple dishes, and view ingredient details.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project
- **007-task-auth-frontend** — requires authentication (Cook Mode is authenticated-only)
- **017-task-cook-mode-backend** — requires the Cook Mode session API endpoints

## Technical Requirements

### Cook Mode entry points

- "Start cooking" action from the home page or navigation
- "Cook this dish" button from the recipe detail page (pre-populates ingredients)
- Session recovery prompt on app open if an active/paused session exists (COOK-50, COOK-51)

### Ingredient workspace

- Central area displaying all added ingredients as visual cards or chips (COOK-03)
- Ingredient search input with autocomplete dropdown (COOK-06, COOK-07)
  - Debounced (200ms) calls to `GET /api/v1/ingredients/search`
  - Typeahead suggestions displayed as a dropdown list
  - Selecting a suggestion adds the ingredient to the workspace
- Each ingredient card shows: name, category icon, remove button
- Drag-and-drop or long-press to reorder ingredients (COOK-05)

### Smart suggestions panel

- Sidebar or collapsible panel showing pairing suggestions (COOK-08 through COOK-10)
- Suggestions update automatically when ingredients are added or removed
- Each suggestion shows: ingredient name, pairing score indicator, brief reason
- Tapping a suggestion adds it to the workspace
- If KB is unavailable, show an informational message and hide the suggestions panel (REQ-66)
- Loading skeleton while suggestions are being fetched

### Ingredient detail view

- Tapping an ingredient in the workspace opens a detail overlay/modal (COOK-13 through COOK-15)
- Shows: flavour profile, "pairs well with" list, substitutes, nutrition summary
- If KB is unavailable, show limited info (name only) with a "KB unavailable" note

### Multi-dish management

- Tab/chip bar at the top showing active dishes (COOK-22 through COOK-25)
- "Add dish" button creates a new dish tab
- Each dish has its own ingredient workspace
- Ingredients can be moved between dishes via drag-and-drop or a "move to" action
- Dish names are editable (COOK-23)
- Remove dish with confirmation (COOK-25)

### Session controls

- "Pause session" button — saves state and navigates away (COOK-51)
- "Finish cooking" button — transitions to the post-cook wrap-up flow (task 019)
- Session auto-saves on every change (optimistic updates with server sync)

### Notes

- Per-dish notes input area (COOK-26 through COOK-29)
- Free-text field that auto-saves
- Notes are preserved across session pauses

### Responsive design

- Mobile: full-screen workspace with collapsible suggestions
- Desktop: side-by-side layout with workspace and suggestions panel

## Acceptance Criteria

- [ ] Ingredient search with autocomplete works and adds ingredients to the workspace
- [ ] Ingredients display as visual cards with remove capability
- [ ] Smart suggestions panel shows relevant pairings that update on ingredient changes
- [ ] Tapping a suggestion adds it to the workspace
- [ ] Ingredient detail overlay shows flavour profile, pairings, substitutes
- [ ] Multi-dish tabs allow independent ingredient management
- [ ] New dishes can be added, renamed, and removed
- [ ] Session auto-saves on every change
- [ ] Pause and resume session works correctly
- [ ] "Finish cooking" navigates to the wrap-up flow
- [ ] KB unavailability is handled gracefully with informational messaging
- [ ] Notes can be added and are preserved across pauses
- [ ] Layout is responsive across mobile and desktop

## Testing Requirements

- Component tests for ingredient search with autocomplete
- Component tests for ingredient card (render, remove, detail trigger)
- Component tests for suggestions panel (loading, results, KB unavailable)
- Component tests for multi-dish tab management (add, rename, remove, switch)
- Component test for ingredient detail overlay
- Component test for notes input
- Integration test for full session flow: create → add ingredients → get suggestions → finish
- Integration test for session recovery (navigate away → return → session restored)
- Accessibility tests (keyboard navigation, ARIA labels, focus trapping in modals)
- Minimum 85% code coverage
