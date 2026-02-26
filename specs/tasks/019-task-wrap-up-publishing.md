# Task 019: Post-Cook Wrap-Up and Recipe Publishing

> **GitHub Issue:** [#18](https://github.com/andrew-rubio/blend-app/issues/18)

## Description

Implement the post-cook wrap-up flow (backend and frontend) per the Cook Mode FRD (COOK-30 through COOK-55). After a Cook Mode session completes, users are guided through rating ingredient pairings, uploading photos, and optionally publishing their creation as a community recipe.

## Dependencies

- **009-task-media-upload** — requires the media upload pipeline for recipe photos
- **012-task-recipe-crud-backend** — requires the recipe creation endpoint
- **017-task-cook-mode-backend** — requires the completed session data
- **018-task-cook-mode-frontend** — requires the Cook Mode workspace (preceding flow)

## Technical Requirements

### Wrap-up flow — Backend

- `POST /api/v1/cook-sessions/{id}/complete` triggers the wrap-up phase
- `POST /api/v1/cook-sessions/{id}/feedback` — submit ingredient pairing ratings (COOK-30 through COOK-35)
  - Body: array of `{ ingredientId1, ingredientId2, rating: 1-5, comment? }`
  - Ratings update the community pairing scores in the `ingredientPairings` container (aggregate average)
- `POST /api/v1/cook-sessions/{id}/publish` — convert the session into a published recipe (COOK-40 through COOK-44)
  - Creates a new recipe document from the session data
  - Requires: title (mandatory), at least 1 ingredient, and at least 1 direction step
  - Optional: description, photos, cuisine type, tags
  - Returns the new recipe ID

### Wrap-up flow — Frontend

- Step 1: **Session summary** (COOK-30)
  - Display all dishes, their ingredients, and notes from the completed session
  - Review screen before proceeding

- Step 2: **Ingredient pairing feedback** (COOK-31 through COOK-35)
  - For each dish, show ingredient pairs used
  - Star rating (1-5) for each pair, indicating how well they worked together
  - Optional text feedback per pair
  - Skip option for pairs the user doesn't want to rate (COOK-34)
  - Visual indicator showing which pairs have been rated

- Step 3: **Photo upload** (COOK-36 through COOK-39)
  - Upload up to 5 photos of the finished dish(es)
  - Uses the media upload pipeline (SAS token → direct upload → processing)
  - Photo preview with reordering and deletion
  - Mark one photo as primary
  - Skip option if the user doesn't want to upload photos

- Step 4: **Publish as recipe** (COOK-40 through COOK-44)
  - Toggle: "Publish as community recipe?" (default: no)
  - If yes, show a form:
    - Title (required)
    - Description (optional)
    - Direction steps (at minimum 1; optional step-by-step text entry)
    - Cuisine type and tags (optional, pre-populated from preferences)
    - Servings, prep time, cook time (optional)
  - Ingredients pre-populated from the session
  - Preview before publishing
  - Publish calls `POST /api/v1/cook-sessions/{id}/publish`

- Step 5: **Completion** (COOK-45)
  - Success message
  - Link to the published recipe (if published)
  - "Return to home" action

### Recipe editing (REQ-60)

- Users can edit their published recipes later from the profile page
- `PUT /api/v1/recipes/{id}` — edit recipe (from task 012)
- Frontend: recipe edit form identical to the publish form, pre-populated with current data
- Only the recipe owner can edit

### Feedback-driven learning

- Pairing feedback from step 2 feeds back into the Knowledge Base scores (COOK-52 through COOK-55)
- Backend aggregates ratings for each ingredient pair:
  - New community score = weighted average of existing score and new ratings
  - Source type changes from `reference` to `community` after first user feedback
- This process runs synchronously on feedback submission (small payload, acceptable latency)

## Acceptance Criteria

- [ ] Completed session shows a summary of all dishes and ingredients
- [ ] User can rate ingredient pairings with 1-5 stars
- [ ] Pairing ratings are persisted and update community scores in the KB
- [ ] User can upload up to 5 photos via the media pipeline
- [ ] User can optionally publish the session as a community recipe
- [ ] Published recipe requires a title and at least 1 ingredient
- [ ] Published recipe appears in the user's profile and in search results
- [ ] Published recipes can be edited later by the owner
- [ ] Each wrap-up step is skippable except the summary
- [ ] Success screen shows a link to the published recipe
- [ ] Feedback submission updates pairing scores in Cosmos DB

## Testing Requirements

- Unit tests for pairing score aggregation algorithm
- Component tests for each wrap-up step (summary, rating, photo, publish, completion)
- Component tests for the recipe edit form (pre-population, validation)
- Integration test for the full wrap-up flow (session → feedback → photos → publish)
- Integration test for feedback score update in the `ingredientPairings` container
- Integration test for photo upload pipeline within the wrap-up context
- Accessibility tests for the multi-step form flow
- Minimum 85% code coverage
