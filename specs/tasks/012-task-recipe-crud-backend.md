# Task 012: Recipe Data Model and CRUD — Backend

> **GitHub Issue:** [#23](https://github.com/andrew-rubio/blend-app/issues/23)

## Description

Implement the backend data model, storage, and CRUD API endpoints for user-generated recipes. This covers recipe creation, reading, updating, deletion, and visibility toggling as defined across the Cook Mode FRD (COOK-40 through COOK-55), Profile & Social FRD (PROF-07 through PROF-16, PROF-29 through PROF-31), and Platform FRD (PLAT-21 through PLAT-24).

## Dependencies

- **005-task-database-setup** — requires the `recipes` Cosmos DB container and entity definitions
- **006-task-auth-backend** — requires authentication and authorization
- **009-task-media-upload** — requires the media pipeline for recipe photos

## Technical Requirements

### Recipe data model

- Recipe entity stored in the `recipes` container, partitioned by `userId`
- Fields include:
  - `id`, `userId` (partition key), `title`, `description`
  - `ingredients: IngredientEntry[]` — each with `name`, `amount`, `unit`, `originalText`
  - `directions: DirectionStep[]` — ordered steps with text and optional image URL
  - `cuisineType`, `dishType`, `diets: string[]`
  - `servings`, `prepTimeMinutes`, `cookTimeMinutes`
  - `photos: PhotoEntry[]` — URLs from the media pipeline, one marked as `isPrimary`
  - `visibility: 'public' | 'private'` — controls whether others can see the recipe (PROF-14)
  - `source: 'user' | 'spoonacular'` — distinguishes user-created from cached external recipes
  - `createdAt`, `updatedAt`
  - `likeCount`, `viewCount` — denormalised counters for ranking

### API endpoints

- `POST /api/v1/recipes` — create a new recipe (COOK-40 through COOK-44)
- `GET /api/v1/recipes/{id}` — get a single recipe by ID
- `PUT /api/v1/recipes/{id}` — update a recipe the user owns (REQ-60)
- `DELETE /api/v1/recipes/{id}` — delete a recipe the user owns (REQ-64, PROF-29 through PROF-31)
- `PATCH /api/v1/recipes/{id}/visibility` — toggle public/private (PROF-14)
- `GET /api/v1/users/{userId}/recipes` — list recipes by a specific user (PROF-07, PROF-08)
- `GET /api/v1/users/me/recipes` — list the current user's own recipes (including private)
- `GET /api/v1/recipes/{id}/liked-by` — list users who liked the recipe

### Like system

- `POST /api/v1/recipes/{id}/like` — like a recipe (PROF-11)
- `DELETE /api/v1/recipes/{id}/like` — unlike a recipe (PROF-12)
- `GET /api/v1/users/me/liked-recipes` — list recipes the current user has liked (PROF-10)
- Like events update the `likeCount` on the recipe document and create an entry in the `activity` container

### Authorization

- Only the recipe owner can update, delete, or change visibility of their recipe
- Private recipes are only visible to the owner
- Public recipes are visible to all authenticated users
- Guest users can view public recipes but cannot like or create

### Pagination

- All list endpoints use cursor-based pagination (per ADR 0006)
- Default page size: 20, max: 50

### Validation

- Title is required, max 200 characters
- At least one ingredient and one direction step required for publishing
- Ingredient amounts must be positive numbers
- Direction steps must be in order

## Acceptance Criteria

- [ ] Recipes can be created with all required fields
- [ ] Recipes can be retrieved by ID (respecting visibility)
- [ ] Recipes can be updated only by the owner
- [ ] Recipes can be deleted only by the owner (with cascade cleanup)
- [ ] Recipe visibility can be toggled between public and private
- [ ] User's own recipes endpoint returns both public and private recipes
- [ ] Other user's recipe endpoint returns only public recipes
- [ ] Like/unlike operations update the recipe's `likeCount`
- [ ] Liked recipes list returns the current user's liked recipes
- [ ] All list endpoints support cursor-based pagination
- [ ] Validation errors return RFC 9457 Problem Details responses

## Testing Requirements

- Unit tests for recipe validation (title, ingredients, directions, amounts)
- Unit tests for authorization logic (owner-only operations, visibility filtering)
- Integration tests for all CRUD endpoints
- Integration test for like/unlike with counter update
- Integration test for pagination (multiple pages, cursor continuation)
- Integration test for cascade cleanup on recipe deletion (likes, activity entries)
- Minimum 85% code coverage
