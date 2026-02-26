# Task 022: Profile Management — Backend

> **GitHub Issue:** [#33](https://github.com/andrew-rubio/blend-app/issues/33)

## Description

Implement the backend API for user profile management per the Profile & Social FRD (PROF-07 through PROF-16, PROF-29 through PROF-31). This includes profile data endpoints, recipe collection management, public profile views, and recipe deletion.

## Dependencies

- **005-task-database-setup** — requires the User entity and recipes container
- **006-task-auth-backend** — requires authentication
- **012-task-recipe-crud-backend** — requires recipe CRUD operations

## Technical Requirements

### Profile endpoints

- `GET /api/v1/users/me/profile` — get the current user's full profile
  - Returns: display name, email, avatar URL, bio, join date, recipe count, follower count, following count
- `PUT /api/v1/users/me/profile` — update profile fields
  - Editable fields: display name, bio, avatar URL
  - Validates display name (2-50 chars, no special characters), bio (max 500 chars)
- `GET /api/v1/users/{userId}/profile` — get another user's public profile (PROF-16)
  - Returns only public information: display name, avatar, bio, join date, public recipe count
  - Does not expose email or private settings

### Recipe collections

- `GET /api/v1/users/me/recipes` — list the current user's own recipes (PROF-07)
  - Includes both public and private recipes
  - Supports sorting: newest, oldest, most liked
  - Cursor-based pagination
- `GET /api/v1/users/{userId}/recipes` — list another user's public recipes (PROF-08)
  - Only returns public-visibility recipes
- `GET /api/v1/users/me/liked-recipes` — list recipes the user has liked (PROF-10)
  - Cursor-based pagination
  - Returns enriched recipe data (title, image, author, cuisine)

### Recipe visibility and deletion

- `PATCH /api/v1/recipes/{id}/visibility` — toggle recipe visibility (PROF-14)
  - Body: `{ visibility: 'public' | 'private' }`
  - Only the recipe owner can change visibility
- `DELETE /api/v1/recipes/{id}` — delete a recipe (PROF-29 through PROF-31)
  - Only the recipe owner can delete
  - Requires confirmation in the API (e.g., `?confirm=true` query parameter)
  - Cascade cleanup: remove associated likes from `activity`, clean up media references
  - Soft-delete with a 30-day grace period, after which hard delete runs (REQ-64)

### Profile statistics

- Profile stats are denormalised on the user document:
  - `recipeCount`, `likeCount` (total likes received on all recipes), `followerCount`, `followingCount`
- These counters are updated when recipes are created/deleted and likes are given/removed
- A background reconciliation process can fix drift (not required for initial implementation)

## Acceptance Criteria

- [ ] User can retrieve and update their own profile
- [ ] Public profile endpoint returns only public information
- [ ] Own recipe list includes both public and private recipes
- [ ] Other user's recipe list includes only public recipes
- [ ] Liked recipes list returns paginated results with enriched data
- [ ] Recipe visibility can be toggled by the owner
- [ ] Recipe deletion requires owner authorisation and triggers cascade cleanup
- [ ] Deleted recipes enter a 30-day soft-delete window
- [ ] Profile statistics (recipe count, like count) are maintained accurately
- [ ] Validation errors return clear messages for invalid profile updates

## Testing Requirements

- Unit tests for profile validation (display name, bio length)
- Unit tests for authorisation (own profile vs. public profile data)
- Integration tests for all profile endpoints
- Integration test for recipe visibility toggle and its effect on public queries
- Integration test for recipe deletion cascade (likes, media)
- Integration test for soft-delete and the 30-day retention
- Integration test for profile stats counter accuracy
- Minimum 85% code coverage
