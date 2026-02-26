# Task 025: Friends and Notifications — Frontend

> **GitHub Issue:** [#36](https://github.com/andrew-rubio/blend-app/issues/36)

## Description

Implement the friends management UI and notification system per the Profile & Social FRD (PROF-01 through PROF-06, PROF-27 through PROF-28) and Platform FRD (PLAT-35 through PLAT-41, REQ-65). This covers the friends list, friend request management, user search for adding friends, the notification bell, and the notification list.

## Dependencies

- **002-task-frontend-scaffolding** — requires the Next.js project
- **007-task-auth-frontend** — requires authentication state
- **024-task-friends-backend** — requires the friends API endpoints
- **026-task-notifications-backend** — requires the notifications API endpoints

## Technical Requirements

### Friends page

- Route: `/friends`
- Tab bar:
  1. **Friends** — list of accepted friends (PROF-01)
  2. **Requests** — incoming friend requests with accept/decline buttons (PROF-02, PROF-04, PROF-05)
  3. **Sent** — outgoing pending requests with status indicator (PROF-27)
- Each friend card: avatar, display name, recipe count, "View profile" action
- Each request card: avatar, display name, accept/decline buttons
- Empty states for each tab

### Add friend flow

- Search input to find users by display name (PROF-28)
- Debounced search (300ms) calling `GET /api/v1/users/search`
- Search results show: avatar, display name, connection status
- "Add friend" button shown for users who are not yet connected
- "Pending" badge for users with an outstanding request
- "Friends" badge for already-connected users

### Notification bell (REQ-65, PLAT-35 through PLAT-41)

- Bell icon in the global navigation header
- Unread count badge displayed on the bell icon
- Poll `GET /api/v1/notifications/unread-count` using adaptive polling (ADR 0010):
  - 15 seconds when the app is active
  - 120 seconds when backgrounded
  - Pause polling after 5 minutes of inactivity
  - Resume on user interaction

### Notification list

- Route: `/notifications` (or slide-over panel)
- Fetch from `GET /api/v1/notifications` via TanStack Query
- Notification types with appropriate icons and formatting:
  - `friendRequestReceived` — "{user} sent you a friend request" → Navigate to requests tab
  - `friendRequestAccepted` — "{user} accepted your friend request" → Navigate to friend's profile
  - `recipeLiked` — "{user} liked your recipe {title}" → Navigate to recipe detail
  - `recipePublished` — "Your recipe {title} has been published" → Navigate to recipe detail
- Each notification: icon, message text, timestamp (relative: "2m ago", "1h ago")
- Mark as read on tap (or "Mark all as read" button)
- `POST /api/v1/notifications/{id}/read` and `POST /api/v1/notifications/read-all`
- Infinite scroll pagination for notification history

### State management

- Notification unread count in Zustand (updated by polling)
- Polling interval managed by a custom hook that detects app visibility
- Invalidate notification queries when user opens the notification list

## Acceptance Criteria

- [ ] Friends list shows accepted friends with profile info
- [ ] Incoming requests show accept/decline actions that work correctly
- [ ] Outgoing requests show pending status
- [ ] User search finds users and shows connection status
- [ ] "Add friend" sends a request and updates the UI optimistically
- [ ] Notification bell displays unread count badge
- [ ] Adaptive polling adjusts intervals based on app activity state
- [ ] Notification list shows all notification types with correct formatting
- [ ] Tapping a notification navigates to the relevant content
- [ ] Notifications can be marked as read individually and in bulk
- [ ] Empty states display for all tabs and the notification list

## Testing Requirements

- Component tests for friends list, request cards (accept/decline), sent request cards
- Component test for user search with connection status display
- Component test for notification bell with unread count badge
- Component test for notification list with different notification types
- Component test for adaptive polling hook (activity detection, interval changes)
- Integration test for friend request flow: search → add → accept → friends list
- Integration test for notification mark-as-read flow
- Accessibility tests (ARIA labels for badges, keyboard navigation, focus management)
- Minimum 85% code coverage
