# Task 015: Recipe Detail View — Frontend

> **GitHub Issue:** [#14](https://github.com/andrew-rubio/blend-app/issues/14)

## Description

Implement the recipe detail view per the Explore & Search FRD (EXPL-20 through EXPL-37). This is the full recipe page displayed when a user selects a recipe from search results, the home page, or any recipe link. It includes a three-tab layout (overview, ingredients, directions), serving adjustment, like/share actions, and the "Cook this dish" entry point.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project
- **012-task-recipe-crud-backend** — requires the recipe detail API endpoint
- **014-task-explore-search-frontend** — requires navigation from search results to detail

## Technical Requirements

### Recipe detail page

- Route: `/recipes/{id}` (or `/recipes/{slug}`)
- Fetch recipe data from `GET /api/v1/recipes/{id}` via TanStack Query
- Record view in `POST /api/v1/recipes/{id}/view` for "recently viewed" tracking
- Hero section with primary photo, title, cuisine tags, author info, timestamps

### Three-tab layout

1. **Overview tab** (EXPL-20)
   - Description text
   - Key stats: prep time, cook time, total time, servings, difficulty
   - Diet and intolerance badges
   - Photo gallery (if multiple photos)

2. **Ingredients tab** (EXPL-22)
   - Full ingredient list with amounts and units
   - Serving adjuster: +/- buttons to scale the recipe (EXPL-24, EXPL-25)
   - Ingredient amounts recalculate dynamically when servings change (EXPL-26)
   - Optional: ingredient substitution suggestions (if Knowledge Base is available)

3. **Directions tab** (EXPL-27)
   - Ordered step list with step numbers
   - Each step shows text and optional step image
   - Steps are visually distinct and easy to follow

### Actions

- **Like button** — toggles like/unlike, updates count in real time (EXPL-30, EXPL-31)
- **Share button** — copies shareable URL to clipboard or opens share sheet (EXPL-36, EXPL-37)
- **"Cook this dish" button** — navigates to Cook Mode with this recipe pre-loaded (EXPL-28)
  - Only shown for authenticated users; guest users see a login prompt (EXPL-29)

### Author info

- Display author name and avatar for user-generated recipes
- Display "Spoonacular" branding for external recipes
- Link to user profile for community recipes

### Responsive layout

- Mobile: stacked layout, tabs as horizontal scrollable pills
- Desktop: wider content area, possible side-by-side ingredients + directions

### Loading and error states

- Skeleton loading while fetching recipe data
- Error state if recipe not found (404) or not accessible (403 for private recipes)
- Optimistic update on like/unlike

## Acceptance Criteria

- [ ] Recipe detail page loads and displays all recipe information
- [ ] Three-tab layout switches between overview, ingredients, and directions
- [ ] Serving adjuster recalculates ingredient amounts correctly
- [ ] Like/unlike toggles immediately with optimistic UI update
- [ ] Share button copies a shareable URL to the clipboard
- [ ] "Cook this dish" button navigates to Cook Mode (authenticated users only)
- [ ] Guest users see a login prompt instead of "Cook this dish"
- [ ] Author info links to the user profile for community recipes
- [ ] 404 and 403 error states display appropriate messages
- [ ] Page is responsive across mobile and desktop viewports

## Testing Requirements

- Component tests for each tab view (overview, ingredients, directions)
- Component tests for the serving adjuster with ingredient recalculation
- Component test for like/unlike button (optimistic update, error rollback)
- Component test for share functionality
- Component test for "Cook this dish" button (authenticated vs. guest)
- Integration test for full page load and tab switching (mock API)
- Accessibility tests (tab navigation, ARIA roles, screen reader support)
- Minimum 85% code coverage
