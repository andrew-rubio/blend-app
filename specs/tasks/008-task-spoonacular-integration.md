# Task 008: Spoonacular API Integration and Caching

> **GitHub Issue:** [#7](https://github.com/andrew-rubio/blend-app/issues/7)
> **Branch:** `copilot/integrate-spoonacular-api-caching`
> **Status:** Implemented ✅

## Implemented

### Project structure
- `src/Blend.Api/` — ASP.NET Core .NET 9 Web API
- `tests/Blend.Api.Tests/` — xUnit test project (46 tests)

### Domain models (`src/Blend.Api/Domain/Models/`)
- `RecipeSummary` — lightweight recipe card (id, title, image, ingredient counts, likes)
- `RecipeDetail` — full recipe (ingredients, instructions, dietary flags)
- `IngredientSubstitute` — substitute list for an ingredient
- `SpoonacularResult<T>` — envelope with `DataSource`, `IsFromCache`, `IsLimitedResults`
- `SearchByIngredientsOptions`, `ComplexSearchOptions` — typed search parameter objects

### Cache service (`src/Blend.Api/Services/Cache/`)
- `ICacheService` — `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`
- `CacheService` — L1 (IMemoryCache) → L2 (Cosmos DB) with graceful L2 fallback
- Cache keys (`SpoonacularCacheKeys`): `spoon:search:{hash}`, `spoon:recipe:{id}`, `spoon:substitute:{name}`
- TTLs per ADR 0009: Search (L1=1h, L2=24h), Recipe (L1=2h, L2=7d), Substitutes (L1=4h, L2=30d)

### Spoonacular service (`src/Blend.Api/Services/Spoonacular/`)
- `ISpoonacularService` + `SpoonacularService` (cache-aware)
- `QuotaTracker` — tracks `X-API-Quota-Used` header; warns at 80%, cache-only at 95%
- `SpoonacularMapper` — maps API DTOs to domain types; handles nulls gracefully
- `SpoonacularQuotaHealthCheck` — reports quota status (Healthy / Degraded / Unhealthy)
- `SpoonacularOptions` — all thresholds and TTLs are configuration-driven; API key never in source

### Registration (`src/Blend.Api/Extensions/ServiceCollectionExtensions.cs`)
- `AddSpoonacularServices(IConfiguration)` — registers all services, HTTP client with resilience handler, health check

### Tests (46 passing)
- Cache service: L1 hit, miss, removal, TTL expiry, complex object round-trip
- Cache key normalisation: case-insensitive, order-independent, prefix format
- Mapper: all fields, null handling, empty collections
- Quota tracker: thresholds, header parsing, invalid values
- SpoonacularService (with mock HTTP + real cache): all endpoints, cache hits, degradation, rate-limit responses
