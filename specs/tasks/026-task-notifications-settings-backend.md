# Task 026: Notifications and App Settings — Backend

> **GitHub Issue:** [#37](https://github.com/andrew-rubio/blend-app/issues/37)

## Description

Implement the backend API for the notification system and app settings per the Platform FRD (PLAT-35 through PLAT-41) and App Settings FRD (SETT-01 through SETT-24, REQ-61).

## Dependencies

- **005-task-database-setup** — requires the `notifications` Cosmos DB container
- **006-task-auth-backend** — requires authentication
- **010-task-preferences-backend** — requires user preferences

## Technical Requirements

### Notification data model

- Stored in the `notifications` container, partitioned by `recipientUserId`
- Document structure:
  - `id`, `recipientUserId` (partition key)
  - `type`: `friendRequestReceived` | `friendRequestAccepted` | `recipeLiked` | `recipePublished` | `system`
  - `title`, `message`, `actionUrl` — display content and deep-link target
  - `sourceUserId` — the user who triggered the notification (optional)
  - `read: boolean`
  - `createdAt`
- TTL: notifications expire after 90 days (Cosmos DB TTL)

### Notification endpoints

- `GET /api/v1/notifications` — list notifications for the current user (PLAT-37)
  - Sorted by `createdAt` descending
  - Supports cursor-based pagination
  - Optional filter: `?unreadOnly=true`
- `GET /api/v1/notifications/unread-count` — returns the count of unread notifications (PLAT-35)
  - Lightweight endpoint optimised for frequent polling
  - Returns: `{ count: number }`
- `POST /api/v1/notifications/{id}/read` — mark a single notification as read (PLAT-39)
- `POST /api/v1/notifications/read-all` — mark all notifications as read (PLAT-40)

### Notification creation service

- `INotificationService` abstraction:
  - `CreateNotification(recipientUserId, type, title, message, actionUrl, sourceUserId?)`
  - Called by other services (friends, recipes, admin) when events occur
  - Batch create for multi-recipient notifications (e.g., admin announcements)

### App settings endpoints

- `GET /api/v1/settings` — get the current user's app settings
  - Returns: unit system preference, notification preferences, theme preference
- `PUT /api/v1/settings` — update app settings
- Settings stored as a sub-document on the User entity

### Unit toggle (SETT-13 through SETT-16)

- Setting: `unitSystem: 'metric' | 'imperial'`
- The frontend uses this to display ingredient amounts in the preferred unit
- Backend stores the preference; conversion logic is frontend-side

### Ingredient submission (SETT-08 through SETT-12)

- `POST /api/v1/ingredients/submissions` — submit a new ingredient for review (SETT-10)
  - Body: `{ name, category, description }`
  - Creates a pending submission in the `content` container with `type: 'ingredient-submission'`
  - Submission goes to the admin approval queue (task 028)
- `GET /api/v1/ingredients/submissions/mine` — list the user's submitted ingredients with status

### Account deletion (SETT-17 through SETT-24, REQ-61)

- `POST /api/v1/users/me/delete-request` — initiate account deletion (SETT-17)
  - Requires re-authentication (password confirmation or OAuth re-consent)
  - Creates a deletion request with a 30-day grace period (SETT-22)
  - During grace period, the account is deactivated but data is retained
- `POST /api/v1/users/me/cancel-deletion` — cancel a pending deletion request (SETT-23)
  - Reactivates the account if within the grace period
- After 30 days, a background process permanently deletes:
  - User document, all recipes, activity records, connections, notifications, media assets
  - Must comply with GDPR/CCPA data removal requirements (SETT-24)

## Acceptance Criteria

- [ ] Notifications are created and retrieved for the current user
- [ ] Unread count endpoint returns the correct count
- [ ] Notifications can be marked as read individually and in bulk
- [ ] Notifications expire after 90 days via Cosmos DB TTL
- [ ] Unit system preference is stored and retrievable
- [ ] Ingredient submissions are created and queryable by the submitting user
- [ ] Account deletion request deactivates the account with a 30-day grace period
- [ ] Deletion can be cancelled within the grace period
- [ ] After 30 days, all user data is permanently removed
- [ ] Re-authentication is required before deletion

## Testing Requirements

- Unit tests for notification creation service
- Unit tests for unread count logic
- Unit tests for account deletion state machine (request → grace → permanent)
- Integration tests for notification CRUD endpoints
- Integration test for notification polling endpoint performance
- Integration test for ingredient submission lifecycle
- Integration test for account deletion and cancellation
- Integration test for data cascade on permanent deletion
- Minimum 85% code coverage
