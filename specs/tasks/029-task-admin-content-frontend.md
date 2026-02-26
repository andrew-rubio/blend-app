# Task 029: Admin Content Management — Frontend

> **GitHub Issue:** [#40](https://github.com/andrew-rubio/blend-app/issues/40)

## Description

Implement the admin dashboard for managing featured content and the ingredient approval queue per the Platform FRD (PLAT-25 through PLAT-34).

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project
- **007-task-auth-frontend** — requires authentication with admin role detection
- **028-task-admin-content-backend** — requires the admin content API endpoints

## Technical Requirements

### Admin route protection

- Admin pages under `/admin/*` route group
- Route guard checks for admin role in the JWT token
- Non-admin users are redirected to the home page
- Admin navigation link only visible to admin users

### Admin dashboard

- Route: `/admin`
- Overview cards showing counts: featured recipes, stories, videos, pending ingredient submissions
- Quick links to each management section

### Featured recipes management

- Route: `/admin/featured-recipes`
- Table/list of current featured recipes with: title, source badge, display order, actions
- "Add featured recipe" form:
  - Search input to find recipes (by title or ID)
  - Set display order, custom title, and description
  - Upload or select a cover image
- Edit inline or in a modal
- Drag-and-drop reordering of display order
- Delete with confirmation

### Stories management

- Route: `/admin/stories`
- Table/list of stories with: title, author, reading time, publish date, actions
- "Create story" form:
  - Title, author, cover image upload, reading time
  - Content editor (markdown or rich text) with preview mode
  - Related recipe references (searchable multi-select)
- Edit and delete with confirmation

### Videos management

- Route: `/admin/videos`
- Table/list of videos with: title, creator, duration, thumbnail, actions
- "Add video" form:
  - Title, video embed URL, thumbnail upload, duration, creator
  - Video preview (embed player)
- Edit and delete with confirmation

### Ingredient approval queue

- Route: `/admin/ingredients`
- Table of pending submissions with: ingredient name, category, submitted by, submission date
- Filter tabs: Pending, Approved, Rejected
- Each pending row has:
  - "Approve" button — adds to Knowledge Base, notifies user
  - "Reject" button — opens a dialog for optional rejection reason, notifies user
- Batch approve/reject for multiple selections

## Acceptance Criteria

- [ ] Admin routes are only accessible to admin-role users
- [ ] Dashboard shows content counts and quick links
- [ ] Featured recipes can be added, edited, reordered, and removed
- [ ] Stories can be created with markdown content and previewed
- [ ] Videos can be added with embed URLs and previewed
- [ ] Ingredient submissions can be filtered by status
- [ ] Submissions can be approved or rejected with optional reason
- [ ] All management lists support pagination and search
- [ ] Confirmation dialogs appear before destructive actions

## Testing Requirements

- Component tests for admin route guard (admin allowed, non-admin redirected)
- Component tests for each management table (render, sort, paginate)
- Component tests for create/edit forms (validation, submission)
- Component test for drag-and-drop reordering
- Component test for ingredient approval/rejection flow
- Integration test for full CRUD cycle for each content type (mock API)
- Accessibility tests (table keyboard navigation, form labels, focus management)
- Minimum 85% code coverage
