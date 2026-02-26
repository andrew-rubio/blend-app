# Task 005: Database Setup and Data Models

> **GitHub Issue:** [#5](https://github.com/andrew-rubio/blend-app/issues/5)

## Description

Set up Azure Cosmos DB for NoSQL integration and define the complete data model for the Blend application per ADR 0003. This includes configuring the Cosmos DB client in the backend, creating the container definitions with partition keys and TTL policies, implementing the repository pattern for data access, and defining all domain entity types.

## Dependencies

- **001-task-backend-scaffolding** — requires the API project structure and Aspire AppHost

## Technical Requirements

### Cosmos DB client configuration

- Configure the Azure Cosmos DB .NET SDK in the API project via dependency injection
- Register the Cosmos DB client in the Aspire AppHost for local development (use the Cosmos DB emulator or Aspire's Cosmos DB resource)
- Support both serverless (development) and provisioned throughput (production) modes via configuration
- Implement connection resilience (retry policies, timeout configuration)

### Container definitions

Define the following Cosmos DB containers with their partition keys (per ADR 0003 and ADR 0009):

| Container | Partition Key | Purpose |
|-----------|--------------|---------|
| `users` | `/id` | User accounts, preferences, profile data |
| `recipes` | `/authorId` | User-generated recipes (published and private) |
| `connections` | `/userId` | Friend connections and friend requests |
| `activity` | `/userId` | Recently viewed history, cooking sessions |
| `content` | `/contentType` | Admin-managed featured recipes, stories, videos |
| `notifications` | `/recipientUserId` | User notifications (friend requests, etc.) |
| `cache` | `/cacheKey` | L2 cache for Spoonacular responses (per ADR 0009) |
| `ingredientPairings` | `/ingredientId` | Mutable ingredient pairing scores (per ADR 0005) |

### Domain entity types

Define strongly-typed C# record/class types for:

- **User**: id, email, displayName, profilePhotoUrl, passwordHash (reference), preferences (embedded), measurementUnit, createdAt, updatedAt, unreadNotificationCount, role (user/admin)
- **UserPreferences** (embedded in User): favoriteCuisines[], favoriteDishTypes[], diets[], intolerances[], dislikedIngredientIds[]
- **Recipe**: id, authorId, title, description, ingredients[] (structured: quantity, unit, ingredientName, ingredientId), directions[] (stepNumber, text, mediaUrl), prepTime, cookTime, servings, cuisineType, dishType, tags[], featuredPhotoUrl, photos[], isPublic, likeCount, viewCount, createdAt, updatedAt
- **Connection**: id, userId, friendUserId, status (pending/accepted/declined), initiatedBy, createdAt, updatedAt
- **Activity**: id, userId, type (viewed/cooked/liked), referenceId, referenceType, timestamp
- **CookingSession**: id, userId, dishes[] (embedded: name, cuisineType, ingredients[], notes), status (active/completed), startedAt, updatedAt
- **Notification**: id, recipientUserId, type (friend_request_received/accepted, recipe_liked, new_follower, system), sourceUserId, referenceId, message, read, createdAt, ttl
- **Content**: id, contentType (featured_recipe/story/video), title, body, thumbnailUrl, mediaUrl, authorName, isPublished, publishedAt, createdAt, updatedAt
- **CacheEntry**: cacheKey, data (JSON string), createdAt, ttl

### Repository pattern

- Implement a generic `IRepository<T>` interface with CRUD operations (GetById, GetByQuery, Create, Update, Delete, GetPaged)
- Implement concrete Cosmos DB repositories for each container
- Support pagination (cursor-based for feeds, offset-based for search results) per ADR 0006
- Support partial updates (patch operations) for efficient field-level updates
- Implement a unit of work or transactional batch pattern for operations that span multiple documents in the same partition

### Data seeding

- Create a seed script or startup task that ensures all containers exist with correct configuration (partition keys, TTL, indexing policies)
- Create a seed data mechanism for development (sample users, recipes, content)

## Acceptance Criteria

- [ ] Cosmos DB client is configured and resolves via dependency injection
- [ ] All 8 containers are created with correct partition keys on application startup
- [ ] All domain entity types are defined with proper C# types (no `object` or `dynamic`)
- [ ] Repository CRUD operations work for all entity types (verified by integration tests against emulator)
- [ ] Pagination returns correct page sizes and cursor tokens
- [ ] TTL is configured on the `cache` and `notifications` containers
- [ ] Cosmos DB connection is resilient to transient failures (retry policy configured)
- [ ] The Aspire dashboard shows Cosmos DB as a connected resource
- [ ] Seed data can be loaded for development environments

## Testing Requirements

- Integration tests for each repository (CRUD operations) against Cosmos DB emulator
- Unit tests for entity type serialisation/deserialisation (ensure JSON roundtrip fidelity)
- Unit tests for pagination logic (cursor encoding/decoding)
- Unit tests for partition key resolution
- Integration test for container creation on startup
- Minimum 85% code coverage for repository and entity code
