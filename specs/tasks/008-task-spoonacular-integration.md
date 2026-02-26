# Task 008: Spoonacular API Integration and Caching

> **GitHub Issue:** [#7](https://github.com/andrew-rubio/blend-app/issues/7)

## Description

Implement the backend service layer for Spoonacular API integration with the two-tier caching strategy defined in ADR 0009. This is a critical infrastructure piece — every recipe search, recipe detail view, and ingredient substitute lookup flows through this service. The caching layer must protect the 150 requests/day free-tier limit.

## Dependencies

- **001-task-backend-scaffolding** — requires the API project and middleware
- **005-task-database-setup** — requires the Cosmos DB `cache` container for L2 cache

## Technical Requirements

### Spoonacular API client service

- Implement an `ISpoonacularService` interface with methods for:
  - `SearchByIngredients(ingredients[], options)` — maps to Spoonacular's `findByIngredients` endpoint
  - `ComplexSearch(query, filters)` — maps to Spoonacular's `complexSearch` endpoint (supports keywords, cuisine, diet, intolerances, maxReadyTime, number of results)
  - `GetRecipeInformation(recipeId)` — maps to Spoonacular's recipe information endpoint (full detail)
  - `GetRecipeBulkInformation(recipeIds[])` — batch recipe detail retrieval
  - `GetIngredientSubstitutes(ingredientName)` — maps to Spoonacular's ingredient substitutes endpoint
- Configure the Spoonacular API key securely (from configuration, never in source code) (PLAT-06)
- Use `HttpClient` with `IHttpClientFactory` for connection pooling and resilience
- Configure retry and circuit breaker policies via Microsoft.Extensions.Http.Resilience

### Two-tier caching (ADR 0009)

- Implement `ICacheService` interface:
  - `GetAsync<T>(key)` — check L1 (IMemoryCache) → L2 (Cosmos DB) → return null on miss
  - `SetAsync<T>(key, value, l1Ttl, l2Ttl)` — write to both L1 and L2
- L1 cache: ASP.NET Core `IMemoryCache` (in-process, lost on restart)
- L2 cache: Cosmos DB `cache` container with TTL-based auto-expiration
- Cache key strategy:
  - Search results: `spoon:search:{normalised-query-hash}` (normalise: lowercase, sort parameters)
  - Recipe detail: `spoon:recipe:{spoonacularId}`
  - Substitutes: `spoon:substitute:{ingredientName}`
- TTL configuration (from ADR 0009):
  - Search results: L1 = 1 hour, L2 = 24 hours
  - Recipe detail: L1 = 2 hours, L2 = 7 days
  - Substitutes: L1 = 4 hours, L2 = 30 days

### Cache-aware Spoonacular service

- The primary implementation of `ISpoonacularService` must check the cache before calling the external API
- On cache hit: return cached data (no API call)
- On cache miss: call Spoonacular, cache the response, return the data
- Cache key normalisation must ensure equivalent queries hit the same cache entry

### Rate limit monitoring

- Track the Spoonacular `X-API-Quota-Used` response header
- Log warnings at 80% quota usage
- At 95% quota or when receiving HTTP 402/429: serve only cached data, return a structured "limited results" response for cache misses (PLAT-07, PLAT-08)
- Expose a health check indicator for quota status

### Graceful degradation (REQ-59)

- When Spoonacular is unavailable (network error, rate limited, 5xx):
  - Return cached data if available
  - If no cached data, return an empty result set with a flag indicating external data is unavailable (PLAT-38)
  - Never crash or return raw exception details (PLAT-40)
  - Log the failure with structured context for monitoring
- The API response model must include a `dataSource` indicator so the frontend knows whether results include external data or are internal-only (PLAT-41)

### Response mapping

- Map Spoonacular API responses to internal domain types (not Spoonacular's raw JSON)
- Create mapping types for: recipe summary, recipe detail, ingredient substitute
- Handle missing/null fields gracefully

## Acceptance Criteria

- [ ] `ISpoonacularService` methods return data from Spoonacular API for all supported endpoints
- [ ] Repeated identical queries return cached data without calling the external API
- [ ] L1 cache returns data in < 1ms; L2 cache returns data in < 10ms
- [ ] Expired L1 cache falls through to L2; expired L2 falls through to Spoonacular
- [ ] API key is never exposed in logs, responses, or client-side code
- [ ] Rate limit at 95% triggers cache-only mode with a structured response
- [ ] Spoonacular unavailability returns cached data or an empty result with a degradation flag
- [ ] No raw exceptions or Spoonacular error details leak to the client
- [ ] Cache entries auto-expire per configured TTL in Cosmos DB
- [ ] Health check reports Spoonacular quota status

## Testing Requirements

- Unit tests for `ICacheService` (L1 hit, L2 hit, miss, TTL expiry, write-through)
- Unit tests for cache key normalisation (equivalent queries produce same key)
- Unit tests for Spoonacular response mapping (all fields, null handling)
- Unit tests for rate limit monitoring logic (warning threshold, cache-only threshold)
- Unit tests for graceful degradation (various failure scenarios)
- Integration tests for `ISpoonacularService` with cache (mock HTTP handler + real cache service)
- Integration tests for L2 cache operations against Cosmos DB emulator
- Minimum 85% code coverage for all caching and Spoonacular integration code
