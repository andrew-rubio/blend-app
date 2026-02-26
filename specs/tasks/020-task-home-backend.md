# Task 020: Home Page — Backend

> **GitHub Issue:** [#19](https://github.com/andrew-rubio/blend-app/issues/19)

## Description

Implement the backend aggregation endpoint and supporting services for the Home page per the Home FRD (HOME-01 through HOME-24). The endpoint returns all sections in a single response to minimise round-trips.

## Dependencies

- **005-task-database-setup** — requires Cosmos DB containers (content, activity, recipes)
- **006-task-auth-backend** — requires authentication for personalised content
- **008-task-spoonacular-integration** — requires Spoonacular client for featured recipe enrichment
- **010-task-preferences-backend** — requires user preferences for personalisation
- **012-task-recipe-crud-backend** — requires the recipe data model for community recipes

## Technical Requirements

### Home aggregation endpoint

- `GET /api/v1/home` — returns all home page sections in a single response (per ADR 0006 aggregation pattern)
- Response shape:
  ```
  {
    "search": { "placeholder": "..." },
    "featured": { "recipes": [...], "stories": [...], "videos": [...] },
    "community": { "recipes": [...] },
    "recentlyViewed": { "recipes": [...] }
  }
  ```

### Featured content

- **Featured recipes** (HOME-05 through HOME-08)
  - Curated list managed via admin content tools (see task 028)
  - Stored in the `content` container with `type: 'featured-recipe'`
  - Each entry references a recipe (Spoonacular ID or internal recipe ID)
  - Returns enriched data: title, image, attribution, short description
- **Featured stories** (HOME-09 through HOME-12)
  - Editorial content stored in the `content` container with `type: 'story'`
  - Returns: title, cover image, author, excerpt, reading time
- **Featured videos** (HOME-17 through HOME-20)
  - Video references stored in the `content` container with `type: 'video'`
  - Returns: title, thumbnail, video URL (embed), duration, creator

### Community recipes

- **Latest community recipes** (HOME-13 through HOME-16)
  - Query the `recipes` container for recently published public recipes
  - Ordered by `createdAt` descending
  - Apply user preference filters (exclude intolerance-violating recipes)
  - Returns top 10 recipes: title, image, author, cuisine, like count

### Recently viewed

- **Recently viewed recipes** (HOME-21 through HOME-24)
  - Query the `activity` container for the current user's recent recipe views
  - Returns the last 10 viewed recipes with timestamp
  - Excludes deleted recipes (handle gracefully if a viewed recipe was removed)

### Search placeholder

- Dynamic search placeholder text that cycles through ingredient-based prompts (HOME-01 through HOME-04)
- Placeholder values stored in configuration or the `content` container

### Caching

- Featured content is cached aggressively (changes infrequently) — respect the L1/L2 caching strategy from task 008
- Community recipes cached with short TTL (5 minutes)
- Recently viewed is always fresh (no caching — user-specific real-time data)

## Acceptance Criteria

- [ ] `GET /api/v1/home` returns all sections in a single aggregated response
- [ ] Featured recipes section returns curated recipes with enriched data
- [ ] Featured stories section returns editorial content with cover images
- [ ] Featured videos section returns video references with thumbnails
- [ ] Community recipes section returns recently published public recipes
- [ ] Recently viewed section returns the current user's last 10 viewed recipes
- [ ] User preference filters apply to community recipes (intolerance exclusion)
- [ ] Featured content is served from cache when available
- [ ] Response handles empty sections gracefully (no error if a section has no content)
- [ ] Guest users receive featured and community content but no recently viewed

## Testing Requirements

- Unit tests for each section's data assembly (featured, community, recently viewed)
- Unit tests for preference filtering on community recipes
- Integration test for the aggregated home endpoint
- Integration test for cache hit/miss behaviour on featured content
- Integration test for recently viewed with recipe deletion handling
- Integration test for guest user response (no recently viewed)
- Minimum 85% code coverage
