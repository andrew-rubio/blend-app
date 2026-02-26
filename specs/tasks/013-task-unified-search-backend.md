# Task 013: Unified Recipe Search — Backend

> **GitHub Issue:** [#24](https://github.com/andrew-rubio/blend-app/issues/24)

## Description

Implement the backend search service that merges results from the Spoonacular API and the internal recipe store into a single, ranked result set per the Explore & Search FRD (EXPL-08 through EXPL-19, EXPL-34 through EXPL-35) and Platform FRD (PLAT-04 through PLAT-06).

## Dependencies

- **008-task-spoonacular-integration** — requires the Spoonacular client and caching layer
- **010-task-preferences-backend** — requires user preferences for filtering and ranking
- **012-task-recipe-crud-backend** — requires the internal recipe data model and repository

## Technical Requirements

### Unified search endpoint

- `GET /api/v1/search/recipes` — primary search endpoint (EXPL-08)
- Query parameters:
  - `q` — free-text search query (recipe name, ingredients, keywords)
  - `cuisines` — comma-separated cuisine filter
  - `diets` — comma-separated diet filter
  - `dishTypes` — comma-separated dish type filter
  - `maxReadyTime` — maximum prep + cook time in minutes
  - `sort` — `relevance` (default), `popularity`, `time`, `newest`
  - `cursor` — pagination cursor
  - `pageSize` — items per page (default 20, max 50)

### Search orchestration

- Query both Spoonacular and the internal `recipes` container in parallel
- Apply the user's saved preferences to the Spoonacular request via the preference service (PREF-07, PREF-08)
- For internal recipes, build a Cosmos DB query that filters by cuisine, diet, dish type and searches title/description/ingredients
- Merge results into a single response, applying ranking:
  1. Relevance to query terms
  2. User preference alignment (cuisine match, diet compatibility)
  3. Popularity (like count for internal, Spoonacular score for external)
- Each result includes a `dataSource` flag (`spoonacular` | `community`) so the frontend can display provenance (EXPL-13)

### Partial matching

- Support partial ingredient name matching (EXPL-15) — e.g., "chick" matches "chicken" and "chickpea"
- Support multi-word queries split into tokens for broader matching

### Ad-hoc filters

- Filters can be applied on top of a search query or standalone (EXPL-10)
- Filter combinations are AND-ed together
- If no query text is provided, return a filtered browse (trending/newest based on sort)

### Recently viewed tracking

- Record each recipe view in the `activity` container with timestamp
- `GET /api/v1/users/me/recently-viewed` — returns the most recently viewed recipes (HOME-23, HOME-24)

### Spoonacular quota protection

- Honour the caching layer from task 008 — only call Spoonacular when cache misses
- If the daily quota is exhausted, return only internal results with a `quotaExhausted` flag in the response metadata

## Acceptance Criteria

- [ ] Free-text search returns merged results from both Spoonacular and internal recipes
- [ ] Each result includes a `dataSource` flag indicating its origin
- [ ] User preferences are automatically applied (intolerances excluded, diets deprioritised)
- [ ] Ad-hoc filters (cuisine, diet, dish type, time) narrow results correctly
- [ ] Partial ingredient matching works (e.g., "chick" → "chicken", "chickpea")
- [ ] Results are sorted according to the requested sort parameter
- [ ] Cursor-based pagination works across merged result sets
- [ ] Recently viewed recipes are recorded and retrievable
- [ ] When Spoonacular quota is exhausted, only internal results are returned with a flag
- [ ] Response includes metadata: `totalResults`, `dataSource`, `quotaExhausted`

## Testing Requirements

- Unit tests for search query building (Cosmos DB query, Spoonacular parameter mapping)
- Unit tests for result merging and ranking logic
- Unit tests for partial matching tokenisation
- Unit tests for preference application to search (exclusion vs. deprioritisation)
- Integration tests for the unified search endpoint with mocked Spoonacular
- Integration test for quota exhaustion fallback
- Integration test for recently viewed tracking
- Minimum 85% code coverage
