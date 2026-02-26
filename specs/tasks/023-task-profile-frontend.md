# Task 023: Profile Management — Frontend

> **GitHub Issue:** [#34](https://github.com/andrew-rubio/blend-app/issues/34)

## Description

Implement the user profile page and recipe collection management UI per the Profile & Social FRD (PROF-07 through PROF-16). This includes the user's own profile page, recipe tabs, public profile view, and recipe management actions.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project
- **007-task-auth-frontend** — requires authentication state
- **022-task-profile-backend** — requires the profile API endpoints

## Technical Requirements

### Own profile page

- Route: `/profile` or `/users/me`
- Fetch from `GET /api/v1/users/me/profile` via TanStack Query
- Header section: avatar, display name, bio, join date
- Edit profile button opening an edit form (inline or modal)
- Stats bar: total recipes, total likes received, followers, following

### Profile edit form

- Fields: display name, bio, avatar upload
- Avatar upload uses the media pipeline (SAS token → upload → CDN URL)
- Validation: display name (2-50 chars), bio (max 500 chars)
- Save persists via `PUT /api/v1/users/me/profile`

### Recipe tabs

- Tab bar below the profile header:
  1. **My Recipes** (PROF-07) — user's own created recipes
  2. **Liked Recipes** (PROF-10) — recipes the user has liked
- Each tab shows a grid/list of recipe cards
- Recipe cards: thumbnail, title, cuisine tags, like count, visibility badge (for own recipes)
- Infinite scroll pagination within each tab

### Recipe management actions (own recipes)

- Each own recipe card has a context menu (three-dot icon):
  - **Edit** — navigates to the recipe edit form (task 019)
  - **Toggle visibility** — switch between public/private (PROF-14)
  - **Delete** — confirmation dialog before deletion (PROF-29 through PROF-31)
- Visibility toggle shows current state (eye icon for public, lock for private)
- Deletion confirmation: "This recipe will be removed. You have 30 days to contact support for recovery."

### Public profile page

- Route: `/users/{userId}`
- Fetch from `GET /api/v1/users/{userId}/profile`
- Shows: avatar, display name, bio, join date, public recipe count
- Recipe list showing only public recipes
- "Add friend" button (connects to friends system, task 025)
- Does not show edit controls or private information

### Empty states

- "No recipes yet" when the user has no created recipes (with CTA to start cooking)
- "No liked recipes" when the user hasn't liked anything (with CTA to explore)

## Acceptance Criteria

- [ ] Own profile page displays user info, stats, and recipe tabs
- [ ] Profile can be edited with validation and avatar upload
- [ ] "My Recipes" tab shows all user's recipes (public and private)
- [ ] "Liked Recipes" tab shows liked recipes with pagination
- [ ] Own recipe cards show visibility badges and context menu actions
- [ ] Visibility can be toggled from the context menu
- [ ] Recipe deletion shows confirmation dialog and removes the recipe
- [ ] Public profile page shows only public information and public recipes
- [ ] Empty states display with appropriate CTAs
- [ ] Layout is responsive across mobile and desktop

## Testing Requirements

- Component tests for profile header (display data, edit button)
- Component tests for profile edit form (validation, save, cancel)
- Component tests for recipe tab switching and card rendering
- Component tests for context menu actions (edit, visibility toggle, delete confirmation)
- Component test for empty states
- Integration test for profile load → edit → save round-trip (mock API)
- Integration test for recipe deletion flow (mock API)
- Integration test for public profile view
- Accessibility tests (keyboard navigation, ARIA labels, dialog focus trapping)
- Minimum 85% code coverage
