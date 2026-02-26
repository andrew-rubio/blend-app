# Task 017: Cook Mode Sessions — Backend

> **GitHub Issue:** [#28](https://github.com/andrew-rubio/blend-app/issues/28)

## Description

Implement the Cook Mode backend service for session management, smart suggestions, multi-dish support, and session recovery per the Cook Mode FRD (COOK-01 through COOK-29, COOK-50 through COOK-51).

## Dependencies

- **005-task-database-setup** — requires the Cosmos DB containers
- **006-task-auth-backend** — requires authentication
- **010-task-preferences-backend** — requires user preferences for filtering suggestions
- **016-task-ingredient-kb-backend** — requires the Knowledge Base for pairing and substitution data

## Technical Requirements

### Cook Mode session model

- Sessions stored in a dedicated Cosmos DB container or as documents in the `activity` container (partition key: `userId`)
- Session document includes:
  - `sessionId`, `userId`, `status` (`active` | `paused` | `completed`)
  - `dishes: DishEntry[]` — each dish has a `name`, `ingredients: IngredientEntry[]`, `notes: string`
  - `addedIngredients: SessionIngredient[]` — ingredients added during the session (with timestamps)
  - `createdAt`, `updatedAt`, `pausedAt`
- Only one active session per user at a time (COOK-50)

### API endpoints

- `POST /api/v1/cook-sessions` — create a new Cook Mode session (COOK-01)
  - Optional: pre-populate with ingredients from a selected recipe (`recipeId` parameter)
  - If an active session exists, return conflict error with option to resume or start fresh
- `GET /api/v1/cook-sessions/active` — get the current active session (COOK-50, COOK-51)
- `GET /api/v1/cook-sessions/{id}` — get a specific session
- `PUT /api/v1/cook-sessions/{id}` — update session (add/remove ingredients, update notes)
- `POST /api/v1/cook-sessions/{id}/ingredients` — add an ingredient to the session (COOK-03 through COOK-05)
- `DELETE /api/v1/cook-sessions/{id}/ingredients/{ingredientId}` — remove an ingredient
- `POST /api/v1/cook-sessions/{id}/dishes` — add a new dish to the session (COOK-22, COOK-23)
- `DELETE /api/v1/cook-sessions/{id}/dishes/{dishId}` — remove a dish
- `POST /api/v1/cook-sessions/{id}/complete` — mark session as completed (triggers wrap-up flow)
- `POST /api/v1/cook-sessions/{id}/pause` — pause the session for later resumption

### Smart suggestions engine

- `GET /api/v1/cook-sessions/{id}/suggestions` — get ingredient suggestions based on what's already in the session (COOK-08 through COOK-10)
  - Query the Knowledge Base for pairings of all current session ingredients
  - Aggregate pairing scores — ingredients that pair well with multiple current ingredients rank higher
  - Exclude ingredients that match the user's intolerances or disliked list (from preferences)
  - Return top N suggestions with pairing score and reason text
- If Knowledge Base is unavailable, return an empty suggestion list with a `kbUnavailable` flag (REQ-66)

### Multi-dish support

- A session can contain multiple dishes (COOK-22)
- Each dish is an independent ingredient workspace
- Suggestions can be scoped to a specific dish or the overall session
- Ingredients can be moved between dishes

### Session recovery

- On login or app open, check for an active or paused session (COOK-50, COOK-51)
- If found, offer to resume— the frontend handles the UX, the backend just returns the session state
- Paused sessions expire after 24 hours (configurable)

### Ingredient detail in context

- `GET /api/v1/cook-sessions/{id}/ingredients/{ingredientId}/detail` — returns KB data for an ingredient in the session context (COOK-13 through COOK-15)
  - Includes: flavour profile, substitutes, and a "why it pairs" explanation based on co-occurring ingredients

## Acceptance Criteria

- [ ] Cook Mode session can be created (empty or pre-populated from a recipe)
- [ ] Ingredients can be added, removed, and listed within a session
- [ ] Smart suggestions return relevant pairings ranked by aggregate score
- [ ] User intolerances and disliked ingredients are excluded from suggestions
- [ ] Multi-dish support allows independent ingredient workspaces per dish
- [ ] Only one active session exists per user at a time
- [ ] Session can be paused and resumed within 24 hours
- [ ] Session recovery returns the active/paused session on app open
- [ ] When KB is unavailable, suggestions gracefully return empty with a flag
- [ ] Session completion triggers the wrap-up flow
- [ ] Ingredient detail provides contextual pairing explanations

## Testing Requirements

- Unit tests for suggestion scoring algorithm (single ingredient, multiple ingredients, aggregation)
- Unit tests for intolerance/dislike exclusion from suggestions
- Unit tests for session state transitions (active → paused → completed, active → completed)
- Integration tests for all session CRUD endpoints
- Integration test for multi-dish operations
- Integration test for session recovery flow
- Integration test for KB unavailability graceful degradation
- Minimum 85% code coverage
