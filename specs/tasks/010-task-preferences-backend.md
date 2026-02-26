# Task 010: User Preferences — Backend

> **GitHub Issue:** [#9](https://github.com/andrew-rubio/blend-app/issues/9)

## Description

Implement the backend API endpoints and service layer for user preferences management per the User Preferences FRD (REQ-5 through REQ-8). This includes CRUD operations for cuisine preferences, diet preferences, intolerances, and disliked ingredients. Preferences must be persisted on the user document and automatically applied to personalise search results and Cook Mode suggestions.

## Dependencies

- **005-task-database-setup** — requires the User entity with embedded preferences
- **006-task-auth-backend** — requires authentication to identify the current user

## Technical Requirements

### API endpoints

- `GET /api/v1/users/me/preferences` — retrieve the current user's saved preferences
- `PUT /api/v1/users/me/preferences` — update the full preferences object (overwrite)
- `PATCH /api/v1/users/me/preferences` — partial update (e.g., add a cuisine without resending everything)
- All endpoints require authentication

### Preference data model

- Preferences are embedded in the User document (not a separate container) for atomic reads
- Fields:
  - `favoriteCuisines: string[]` — selected cuisine types (PREF-01)
  - `favoriteDishTypes: string[]` — selected dish types (PREF-02)
  - `diets: string[]` — selected dietary plans (PREF-05)
  - `intolerances: string[]` — selected intolerances (PREF-06)
  - `dislikedIngredientIds: string[]` — ingredient IDs the user dislikes (PREF-11)

### Predefined lists

- Expose predefined lists as reference endpoints:
  - `GET /api/v1/preferences/cuisines` — list of available cuisines
  - `GET /api/v1/preferences/dish-types` — list of available dish types
  - `GET /api/v1/preferences/diets` — list of available diets
  - `GET /api/v1/preferences/intolerances` — list of available intolerances
- These lists must align with Spoonacular's supported categories (PREF-09) to ensure filtering works at the API level
- Lists can be static/hardcoded initially (from Spoonacular's documented categories)

### Preference application service

- Implement a `IPreferenceService` that resolves the current user's preferences for use by other services:
  - `GetUserPreferences(userId)` — returns preferences (cached per request)
  - `ApplyPreferencesToSearch(searchParams, preferences)` — enriches Spoonacular search parameters with diet, intolerance, and cuisine filters
  - `GetExcludedIngredientIds(userId)` — returns disliked ingredient IDs for Cook Mode filtering
- Intolerances must be applied as **strict exclusion** — recipes with flagged allergens never appear (PREF-07)
- Diets must be applied as **deprioritisation** — non-matching recipes are ranked lower but not hidden (PREF-08)

### Validation

- Validate that submitted cuisine, dish type, diet, and intolerance values are from the predefined lists
- Validate that disliked ingredient IDs reference real ingredients (when the Knowledge Base is available)
- Return clear validation errors for invalid values

### Immediate effect

- Changes to preferences must take effect on the very next API call (PREF-18)
- No caching of preferences across requests (or invalidate preference cache on update)

## Acceptance Criteria

- [ ] `GET /api/v1/users/me/preferences` returns the current user's saved preferences
- [ ] `PUT /api/v1/users/me/preferences` updates all preference fields atomically
- [ ] `PATCH /api/v1/users/me/preferences` partially updates specific fields
- [ ] Predefined list endpoints return values aligned with Spoonacular's supported categories
- [ ] Invalid preference values are rejected with clear error messages
- [ ] Updated preferences take effect immediately on the next search or suggestion request
- [ ] Intolerance-flagged recipes are strictly excluded from search results
- [ ] Diet-incompatible recipes are deprioritised but still findable
- [ ] Disliked ingredients are excluded from Cook Mode suggestions
- [ ] Preferences persist across sessions (verified by logout/login cycle)

## Testing Requirements

- Unit tests for preference validation (valid values accepted, invalid values rejected)
- Unit tests for `ApplyPreferencesToSearch` (intolerance exclusion, diet deprioritisation, cuisine boosting)
- Unit tests for `GetExcludedIngredientIds`
- Integration tests for preference CRUD operations via API endpoints
- Integration test for preference persistence across sessions
- Integration test for preference application to search results (end-to-end with Spoonacular mock)
- Minimum 85% code coverage for preference service and endpoints
