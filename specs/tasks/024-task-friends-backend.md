# Task 024: Friends System — Backend

> **GitHub Issue:** [#35](https://github.com/andrew-rubio/blend-app/issues/35)

## Description

Implement the backend API for the friends/connections system per the Profile & Social FRD (PROF-01 through PROF-06, PROF-27 through PROF-28). This includes friend request management, user search for adding friends, and connection status queries.

## Dependencies

- **005-task-database-setup** — requires the `connections` Cosmos DB container
- **006-task-auth-backend** — requires authentication

## Technical Requirements

### Connection data model

- Stored in the `connections` container, partitioned by `userId`
- Document structure:
  - `id`, `userId` (partition key), `targetUserId`
  - `status`: `pending` | `accepted` | `declined` | `blocked`
  - `initiatedBy`: the userId who sent the request
  - `createdAt`, `updatedAt`
- Each connection is stored as two mirrored documents (one per user) for efficient querying by partition key

### API endpoints

- `GET /api/v1/friends` — list the current user's accepted friends (PROF-01)
  - Returns: friend profiles (display name, avatar, recipe count)
  - Cursor-based pagination
- `GET /api/v1/friends/requests/incoming` — list pending incoming friend requests (PROF-02)
- `GET /api/v1/friends/requests/outgoing` — list pending outgoing requests (PROF-27)
- `POST /api/v1/friends/requests` — send a friend request (PROF-03)
  - Body: `{ targetUserId }`
  - Validates: target user exists, not already connected, not self
  - Creates pending connection documents for both users
- `POST /api/v1/friends/requests/{requestId}/accept` — accept a friend request (PROF-04)
  - Updates both connection documents to `accepted`
- `POST /api/v1/friends/requests/{requestId}/decline` — decline a friend request (PROF-05)
  - Updates status to `declined`
- `DELETE /api/v1/friends/{friendUserId}` — remove a friend (PROF-06)
  - Removes connection documents for both users
- `GET /api/v1/users/search?q={query}` — search users by display name (PROF-28)
  - Returns matching users with connection status (none, pending, accepted)
  - Used for the "Add friend" flow

### Notifications integration

- When a friend request is sent, create a notification entry in the `notifications` container (see task 026)
- When a friend request is accepted, create a notification for the original sender
- Notification types: `friendRequestReceived`, `friendRequestAccepted`

### Privacy and safety

- Users can only see public profiles of non-friends
- Declined requests cannot be re-sent for 30 days
- Blocked users cannot send requests (block functionality is a future feature — for now, validate that the target hasn't blocked the requester)

## Acceptance Criteria

- [ ] Friend list returns accepted friends with profile data
- [ ] Incoming and outgoing pending requests are listed correctly
- [ ] Friend request can be sent to a valid target user
- [ ] Duplicate and self-requests are rejected with clear errors
- [ ] Requests can be accepted, transitioning both documents to `accepted`
- [ ] Requests can be declined, preventing re-send for 30 days
- [ ] Friends can be removed, cleaning up both connection documents
- [ ] User search returns matches with connection status indicators
- [ ] Notifications are created for friend request and acceptance events
- [ ] All list endpoints support cursor-based pagination

## Testing Requirements

- Unit tests for connection validation (duplicate, self-request, re-send cooldown)
- Unit tests for mirrored document creation and update logic
- Integration tests for the complete friend request lifecycle (send → accept → list → remove)
- Integration test for friend request decline and re-send cooldown
- Integration test for user search with connection status
- Integration test for notification creation on friend events
- Minimum 85% code coverage
