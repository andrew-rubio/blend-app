# Task 016: Ingredient Knowledge Base — Backend

> **GitHub Issue:** [#27](https://github.com/andrew-rubio/blend-app/issues/27)

## Description

Set up the Ingredient Knowledge Base service per ADR 0005 and the Cook Mode FRD (COOK-06 through COOK-15) and Platform FRD (PLAT-15 through PLAT-20, PLAT-50 through PLAT-52). This provides ingredient autocomplete, structured ingredient data, pairing scores, substitution suggestions, and a health-check mechanism for graceful degradation when the KB is unavailable.

## Dependencies

- **005-task-database-setup** — requires the `ingredientPairings` Cosmos DB container
- **001-task-backend-scaffolding** — requires the API project and service registration

## Technical Requirements

### Azure AI Search index

- Index name: `ingredients`
- Source: proprietary ingredient data (initially seeded, later enriched via PDF cracking with Azure AI Document Intelligence)
- Fields:
  - `ingredientId` (key)
  - `name` — searchable, filterable
  - `aliases: string[]` — alternative names (e.g., "aubergine" ↔ "eggplant")
  - `category` — e.g., "vegetable", "protein", "spice"
  - `flavourProfile` — e.g., "sweet", "savoury", "umami"
  - `substitutes: string[]` — common substitution ingredient IDs
  - `nutritionSummary` — basic nutritional info
- Configure suggester for autocomplete on the `name` field
- Scoring profile that boosts exact-match and common ingredients

### Pairing scores in Cosmos DB

- Container: `ingredientPairings` (partition key: `ingredientId`)
- Document structure: `{ ingredientId, pairedIngredientId, score, sourceType, updatedAt }`
- `sourceType`: `reference` (from static data), `community` (from user feedback)
- Community scores are updated via aggregation of post-cook feedback (see task 019)

### API endpoints

- `GET /api/v1/ingredients/search?q={query}` — autocomplete ingredient search (COOK-06, COOK-07)
  - Returns top 10 suggestions ranked by relevance
  - Supports partial matching (e.g., "tom" → "tomato", "tomato paste")
- `GET /api/v1/ingredients/{id}` — ingredient detail (COOK-13 through COOK-15)
  - Returns name, category, flavour profile, substitutes, nutrition summary
- `GET /api/v1/ingredients/{id}/pairings` — pairing suggestions (COOK-08 through COOK-10)
  - Returns scored list of ingredients that pair well, sorted by score descending
  - Optionally filtered by category
- `GET /api/v1/ingredients/{id}/substitutes` — substitution suggestions
  - Returns alternative ingredients with compatibility notes
- `GET /api/v1/ingredients/health` — KB availability check (PLAT-50 through PLAT-52)
  - Returns `{ status: 'healthy' | 'degraded' | 'unavailable', lastChecked }` 

### Knowledge Base service

- `IKnowledgeBaseService` abstraction with methods:
  - `SearchIngredients(query, limit)` → calls Azure AI Search suggest API
  - `GetIngredient(id)` → calls Azure AI Search lookup + Cosmos DB for pairing scores
  - `GetPairings(ingredientId, category?, limit?)` → queries `ingredientPairings` container
  - `GetSubstitutes(ingredientId)` → from Azure AI Search index data
  - `IsAvailable()` → health check with circuit breaker pattern
- Circuit breaker: after 3 consecutive failures, mark KB as unavailable for 60 seconds before retry (PLAT-51)
- When unavailable, all dependent features degrade gracefully — Cook Mode disables smart suggestions but still allows manual ingredient entry (REQ-66)

### Data seeding

- Provide a seed script or startup task that populates the Azure AI Search index with an initial ingredient dataset
- Provide seed data for `ingredientPairings` container with reference pairing scores
- Seeding must be idempotent (safe to re-run)

## Acceptance Criteria

- [ ] Autocomplete search returns relevant ingredient suggestions for partial queries
- [ ] Ingredient detail endpoint returns structured data (name, category, flavour profile, substitutes)
- [ ] Pairing endpoint returns scored pairings sorted by relevance
- [ ] Substitutes endpoint returns alternative ingredients
- [ ] Health check endpoint accurately reports KB availability status
- [ ] Circuit breaker activates after consecutive failures and recovers after the timeout
- [ ] When KB is unavailable, dependent services receive a clear unavailable signal
- [ ] Seed script populates the ingredient index and pairing data idempotently
- [ ] All search results respect the scoring profile (exact matches ranked higher)

## Testing Requirements

- Unit tests for `IKnowledgeBaseService` methods (mocking Azure AI Search and Cosmos DB)
- Unit tests for circuit breaker logic (failure count, timeout, recovery)
- Unit tests for autocomplete ranking and partial matching
- Integration tests for API endpoints with seeded test data
- Integration test for health check and circuit breaker behaviour
- Integration test for degraded mode (KB unavailable → graceful response)
- Minimum 85% code coverage
