# Task 028: Admin Content Management — Backend

> **GitHub Issue:** [#39](https://github.com/andrew-rubio/blend-app/issues/39)

## Description

Implement the backend API for administrative content management per the Platform FRD (PLAT-25 through PLAT-34). This covers CRUD operations for featured recipes, stories, and videos, plus the ingredient submission approval queue.

## Dependencies

- **005-task-database-setup** — requires the `content` Cosmos DB container
- **006-task-auth-backend** — requires authentication with admin role authorisation
- **009-task-media-upload** — requires the media pipeline for story images and video thumbnails

## Technical Requirements

### Admin authorisation

- All admin endpoints require the `admin` role
- Middleware validates the JWT token contains the admin role claim
- Non-admin requests receive 403 Forbidden

### Featured recipes CRUD (PLAT-25 through PLAT-27)

- `GET /api/v1/admin/content/featured-recipes` — list all featured recipe entries
- `POST /api/v1/admin/content/featured-recipes` — add a featured recipe
  - Body: `{ recipeId, source ('spoonacular' | 'community'), title, description, imageUrl, displayOrder }`
- `PUT /api/v1/admin/content/featured-recipes/{id}` — update a featured recipe entry
- `DELETE /api/v1/admin/content/featured-recipes/{id}` — remove from featured
- Featured recipes stored in the `content` container with `type: 'featured-recipe'`

### Stories CRUD (PLAT-28 through PLAT-30)

- `GET /api/v1/admin/content/stories` — list all stories
- `POST /api/v1/admin/content/stories` — create a new story
  - Body: `{ title, coverImageUrl, author, content (markdown/HTML), relatedRecipeIds, readingTimeMinutes }`
- `PUT /api/v1/admin/content/stories/{id}` — update a story
- `DELETE /api/v1/admin/content/stories/{id}` — delete a story
- Stories stored in the `content` container with `type: 'story'`

### Videos CRUD (PLAT-31 through PLAT-34)

- `GET /api/v1/admin/content/videos` — list all videos
- `POST /api/v1/admin/content/videos` — add a video
  - Body: `{ title, thumbnailUrl, videoUrl (embed URL), durationSeconds, creator }`
- `PUT /api/v1/admin/content/videos/{id}` — update a video
- `DELETE /api/v1/admin/content/videos/{id}` — delete a video
- Videos stored in the `content` container with `type: 'video'`

### Ingredient approval queue

- `GET /api/v1/admin/ingredients/submissions` — list pending ingredient submissions
  - Filterable by status: `pending`, `approved`, `rejected`
  - Sorted by submission date
- `POST /api/v1/admin/ingredients/submissions/{id}/approve` — approve a submission
  - Adds the ingredient to the Knowledge Base index (Azure AI Search)
  - Creates a notification for the submitting user
- `POST /api/v1/admin/ingredients/submissions/{id}/reject` — reject a submission
  - Body: `{ reason }` — optional rejection reason
  - Creates a notification for the submitting user with the reason

### Content ordering

- Featured recipes, stories, and videos have a `displayOrder` field
- Admins can reorder items by updating `displayOrder`
- The home page aggregation (task 020) uses `displayOrder` for sorting

## Acceptance Criteria

- [ ] All admin endpoints require the admin role (403 for non-admins)
- [ ] Featured recipes can be fully managed (create, read, update, delete)
- [ ] Stories can be fully managed with markdown content
- [ ] Videos can be fully managed with embed URLs
- [ ] Content items have configurable display ordering
- [ ] Ingredient submissions can be listed, filtered, approved, and rejected
- [ ] Approved ingredients are added to the Knowledge Base
- [ ] Notifications are created for submission approval and rejection
- [ ] All list endpoints support cursor-based pagination

## Testing Requirements

- Unit tests for admin role authorisation (allow admin, reject non-admin)
- Integration tests for featured recipe CRUD
- Integration tests for story CRUD
- Integration tests for video CRUD
- Integration test for content ordering
- Integration test for ingredient approval workflow (submit → approve → KB entry)
- Integration test for ingredient rejection with notification
- Minimum 85% code coverage
