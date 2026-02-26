# Task 014: Explore and Search — Frontend

> **GitHub Issue:** [#25](https://github.com/andrew-rubio/blend-app/issues/25)

## Description

Implement the Explore page and search UI per the Explore & Search FRD (EXPL-01 through EXPL-19). This includes the Explore landing page with trending and recommended sections, the search experience with real-time results, and ad-hoc filtering controls.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project with routing and state management
- **013-task-unified-search-backend** — requires the unified search API endpoint

## Technical Requirements

### Explore landing page

- Default view when user navigates to the Explore tab (EXPL-01)
- Sections:
  - **Trending recipes** — horizontally scrollable cards showing popular recipes (EXPL-02)
  - **Recommended for you** — personalised suggestions based on user preferences (EXPL-04)
  - **Category shortcuts** — quick filter chips for popular cuisines and dish types (EXPL-05)
- Pull-to-refresh or refresh button to reload content (EXPL-06)
- Skeleton loading states for each section

### Search experience

- Search input at the top of the Explore page (EXPL-07)
- Debounced input (300ms) triggering search as the user types (EXPL-08)
- Search results replace the Explore sections when a query is active
- Clear button to reset search and return to Explore view (EXPL-09)
- Display total result count and data source indicators (EXPL-13)
- "No results found" state with suggestions to broaden the search

### Search result cards

- Each card shows: recipe image (thumbnail), title, cuisine tags, prep time, like count, data source badge (EXPL-13)
- Tapping a card navigates to the recipe detail view
- Results display in a responsive grid (2 columns mobile, 3-4 columns desktop)

### Filter panel

- Filter controls accessible via a filter icon/button (EXPL-10)
- Filter categories: cuisines, diets, dish types, max ready time (EXPL-11)
- Filters displayed as chips or in a bottom sheet / side panel
- Active filter count shown on the filter icon (EXPL-12)
- Filters can be combined with search text (AND logic)
- "Clear all filters" button

### Pagination

- Infinite scroll or "Load more" button for paginated results
- Use cursor-based pagination from the API
- Loading indicator at the bottom while fetching the next page

### State management

- Search query and filters stored in Zustand (persisted across navigation within the session)
- Results cached via TanStack Query with appropriate cache keys
- URL query parameters reflect the current search state for shareability (EXPL-36)

## Acceptance Criteria

- [ ] Explore landing page shows trending recipes, personalised recommendations, and category shortcuts
- [ ] Search input triggers debounced API calls as the user types
- [ ] Search results appear in a responsive grid with image, title, tags, and source badge
- [ ] Ad-hoc filters can be applied standalone or combined with search text
- [ ] Active filter count is visible on the filter icon
- [ ] Infinite scroll or "Load more" loads additional pages seamlessly
- [ ] URL updates to reflect the current search/filter state
- [ ] Clear search returns to the Explore landing view
- [ ] Empty and error states are handled with appropriate UI
- [ ] Skeleton loading states appear while data is fetching

## Testing Requirements

- Component tests for Explore landing page sections (trending, recommended, categories)
- Component tests for search input (debounce, clear, empty state)
- Component tests for result cards (data rendering, source badge)
- Component tests for filter panel (selection, clear, active count)
- Integration test for search → results → pagination flow (mock API)
- Integration test for URL state sync
- Accessibility tests (keyboard navigation, ARIA labels, focus management)
- Minimum 85% code coverage
