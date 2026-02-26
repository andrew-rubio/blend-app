# Task 031: Shareable URLs and Social Previews

> **GitHub Issue:** [#42](https://github.com/andrew-rubio/blend-app/issues/42)

## Description

Implement shareable recipe URLs with social media preview support per the Platform FRD (PLAT-47 through PLAT-49) and Explore FRD (EXPL-36 through EXPL-37, REQ-63). Public recipes must have SEO-friendly URLs with Open Graph and Twitter Card metadata for rich link previews.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project with App Router
- **012-task-recipe-crud-backend** — requires the recipe detail API endpoint
- **015-task-recipe-detail-frontend** — requires the recipe detail page

## Technical Requirements

### SEO-friendly recipe URLs (PLAT-47)

- Route pattern: `/recipes/{id}` or `/recipes/{slug}` (if slugs are generated)
- Next.js dynamic route with server-side metadata generation
- Recipe pages are server-rendered (SSR) for search engine crawling
- Public recipes are accessible without authentication
- Private recipes return a 404 (not 403, to avoid leaking existence)

### Open Graph metadata (PLAT-48)

- Generate Open Graph tags on the server for each recipe page:
  - `og:title` — recipe title
  - `og:description` — recipe description (first 200 chars) or auto-generated summary
  - `og:image` — primary recipe photo URL (CDN URL)
  - `og:url` — canonical URL
  - `og:type` — `article`
  - `og:site_name` — "Blend"
- Fallback image when no recipe photo exists

### Twitter Card metadata (PLAT-49)

- `twitter:card` — `summary_large_image`
- `twitter:title`, `twitter:description`, `twitter:image` — matching OG values

### Structured data (JSON-LD)

- Include Recipe schema.org structured data for rich search results:
  - `@type: Recipe`
  - `name`, `description`, `image`, `author`, `datePublished`
  - `prepTime`, `cookTime`, `totalTime` (ISO 8601 duration)
  - `recipeIngredient[]`, `recipeInstructions[]`
  - `recipeCategory`, `recipeCuisine`
  - `aggregateRating` (from like count, if applicable)

### Share functionality (EXPL-36, EXPL-37)

- Share button on the recipe detail page generates the shareable URL
- On mobile: use the Web Share API (`navigator.share`) when available
- Fallback: copy URL to clipboard with a toast notification
- Share URL includes UTM parameters for tracking: `?utm_source=share&utm_medium={platform}`

### User profile shareable URLs

- Route pattern: `/users/{userId}` — public profile pages
- Basic OG metadata for user profiles: name, avatar, recipe count

### Sitemap generation

- Generate a dynamic sitemap.xml listing all public recipes
- Route: `/sitemap.xml`
- Updated periodically or via incremental static regeneration
- Include: public recipe pages, explore landing page, home page

## Acceptance Criteria

- [ ] Recipe pages render with correct Open Graph metadata on the server
- [ ] Twitter Card metadata is present for all public recipe pages
- [ ] JSON-LD structured data is included for search engine rich snippets
- [ ] Share button uses the Web Share API on mobile with clipboard fallback
- [ ] Shared URLs generate correct social media previews (tested with OG validators)
- [ ] Private recipes are not accessible via shareable URLs (return 404)
- [ ] User profile pages have basic OG metadata
- [ ] Dynamic sitemap includes all public recipes
- [ ] Recipe pages are properly crawlable by search engines

## Testing Requirements

- Unit tests for metadata generation (OG tags, Twitter Cards, JSON-LD)
- Unit tests for share URL generation with UTM parameters
- Integration test for server-side rendered recipe page with metadata
- Integration test for private recipe 404 behaviour
- Integration test for sitemap generation with public recipes
- E2E test: verify OG metadata is present in the HTML response
- Minimum 85% code coverage
