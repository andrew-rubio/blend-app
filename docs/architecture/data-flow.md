# Data Flow

This page describes how data flows through the Blend application for key user journeys.

## Recipe Discovery Flow

```mermaid
sequenceDiagram
    participant User
    participant Frontend (Next.js)
    participant API (Blend.Api)
    participant Cache (Cosmos DB)
    participant Spoonacular

    User->>Frontend (Next.js): Enters search query
    Frontend (Next.js)->>API (Blend.Api): GET /api/v1/recipes/search?q=pasta
    API (Blend.Api)->>Cache (Cosmos DB): Check L2 cache
    alt Cache hit
        Cache (Cosmos DB)-->>API (Blend.Api): Cached results
    else Cache miss
        API (Blend.Api)->>Spoonacular: Search recipes
        Spoonacular-->>API (Blend.Api): Results
        API (Blend.Api)->>Cache (Cosmos DB): Store in L2 cache
    end
    API (Blend.Api)-->>Frontend (Next.js): Recipe list
    Frontend (Next.js)-->>User: Displays results
```

## Authentication Flow

```mermaid
sequenceDiagram
    participant User
    participant Frontend (Next.js)
    participant Proxy (src/proxy.ts)
    participant API (Blend.Api)

    User->>Frontend (Next.js): Submits login form
    Frontend (Next.js)->>API (Blend.Api): POST /api/v1/auth/login
    API (Blend.Api)-->>Frontend (Next.js): JWT token
    Frontend (Next.js)->>Frontend (Next.js): Store token in memory

    User->>Proxy (src/proxy.ts): Navigate to protected route
    Proxy (src/proxy.ts)->>Proxy (src/proxy.ts): Check token validity
    alt Token valid
        Proxy (src/proxy.ts)-->>User: Allow access
    else Token missing or expired
        Proxy (src/proxy.ts)-->>User: Redirect to /login
    end
```

## Image Upload Flow

```mermaid
sequenceDiagram
    participant User
    participant Frontend (Next.js)
    participant API (Blend.Api)
    participant Blob Storage

    User->>Frontend (Next.js): Selects image file
    Frontend (Next.js)->>API (Blend.Api): POST /api/v1/media/upload-url
    API (Blend.Api)-->>Frontend (Next.js): Pre-signed upload URL
    Frontend (Next.js)->>Blob Storage: PUT image (direct upload)
    Blob Storage-->>Frontend (Next.js): 201 Created
    Frontend (Next.js)->>API (Blend.Api): PATCH /api/v1/recipes/{id} (with image URL)
```

## Cook Mode Session Flow

```mermaid
sequenceDiagram
    participant User
    participant Frontend (Next.js)
    participant API (Blend.Api)

    User->>Frontend (Next.js): Clicks "Start Cooking"
    Frontend (Next.js)->>Frontend (Next.js): Initialise Zustand cook mode store
    Frontend (Next.js)-->>User: Step 1 of N

    loop For each step
        User->>Frontend (Next.js): Advance to next step
        Frontend (Next.js)-->>User: Next step + timer
    end

    User->>Frontend (Next.js): Request substitution
    Frontend (Next.js)->>API (Blend.Api): GET /api/v1/cook-mode/substitute?ingredient=butter
    API (Blend.Api)-->>Frontend (Next.js): Substitution suggestions
    Frontend (Next.js)-->>User: Display substitutions
```

## TODO

- Add data flow diagram for the social feed (follow graph, activity aggregation)
- Add data flow diagram for personalised recommendations

See [System Design](system-design.md) for the domain model and component architecture.
