# Task 021: Home Page — Frontend

> **GitHub Issue:** [#32](https://github.com/andrew-rubio/blend-app/issues/32)

## Description

Implement the Home page frontend per the Home FRD (HOME-01 through HOME-24). This is the main landing screen for authenticated users, containing a search bar, featured content sections, community recipes, and recently viewed recipes.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project with routing
- **007-task-auth-frontend** — requires authentication state (personalised content)
- **020-task-home-backend** — requires the home aggregation API endpoint

## Technical Requirements

### Page structure

- Route: `/home` (or root `/` for authenticated users)
- Fetch all home data from `GET /api/v1/home` via TanStack Query
- Skeleton loading states for each section independently

### Search bar (HOME-01 through HOME-04)

- Prominent search input at the top of the page
- Dynamic placeholder text cycling through ingredient-based prompts (e.g., "Try searching for chicken...")
- Tapping the search bar navigates to the Explore/Search page with focus on the search input
- Search icon and optional voice search icon (future)

### Featured recipes carousel (HOME-05 through HOME-08)

- Horizontally scrollable carousel of featured recipe cards
- Each card: large cover image, title, cuisine tag, attribution
- Auto-advance with pause on interaction (configurable interval)
- Tapping a card navigates to the recipe detail page
- Dot indicators showing current position

### Featured stories section (HOME-09 through HOME-12)

- Horizontally scrollable story cards
- Each card: cover image, title, author, reading time, excerpt
- Tapping opens a story detail view (rendered from HTML/markdown content)
- Story detail includes: full text, images, author info, related recipes

### Community recipes grid (HOME-13 through HOME-16)

- Grid of recently published community recipe cards
- Each card: thumbnail image, title, author name, cuisine tags, like count
- "See all" link navigates to the Explore page filtered to community recipes
- Tapping a card navigates to the recipe detail page

### Featured videos section (HOME-17 through HOME-20)

- Horizontally scrollable video cards
- Each card: thumbnail image with play icon overlay, title, duration, creator
- Tapping opens an inline video player or video detail page
- Video player supports basic controls (play/pause, mute, fullscreen)

### Recently viewed section (HOME-21 through HOME-24)

- Horizontally scrollable list of recently viewed recipe thumbnails
- Each item: small thumbnail, recipe title
- Tapping navigates to the recipe detail page
- Only shown for authenticated users with view history
- Hidden when empty (no placeholder for "no recently viewed")

### Pull-to-refresh

- Pull down gesture refreshes all home data
- Visual refresh indicator
- Invalidates TanStack Query caches for the home endpoint

### Responsive design

- Mobile: single column, horizontal scrolling sections, full-width search bar
- Desktop: wider sections, multi-column community grid, larger carousel

## Acceptance Criteria

- [ ] Home page loads all sections from the aggregated endpoint
- [ ] Search bar displays dynamic placeholder text and navigates to search on tap
- [ ] Featured recipes carousel scrolls horizontally with auto-advance
- [ ] Featured stories display with cover images and navigate to story detail
- [ ] Community recipes grid shows recent public recipes with "See all" link
- [ ] Featured videos display with thumbnails and play functionality
- [ ] Recently viewed shows the user's recent recipe history (authenticated only)
- [ ] Pull-to-refresh reloads all sections
- [ ] Empty sections are hidden gracefully (no empty state shown)
- [ ] Skeleton loading appears for each section while data loads
- [ ] Page is responsive across mobile and desktop viewports

## Testing Requirements

- Component tests for each section (search bar, carousel, stories, community, videos, recently viewed)
- Component test for carousel auto-advance and manual navigation
- Component test for pull-to-refresh
- Integration test for full home page load with all sections (mock API)
- Integration test for navigation from home page sections to detail pages
- Accessibility tests (keyboard navigation through carousels, ARIA labels)
- Minimum 85% code coverage
