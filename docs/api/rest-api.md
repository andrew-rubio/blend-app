# REST API

The Blend REST API provides the backend for the Blend web application. The API is built with ASP.NET Core .NET 9 and follows REST conventions with JSON request/response bodies.

!!! note "OpenAPI Specification"
    The full interactive API specification will be available at `/swagger` when running the application locally. This page provides a high-level reference; the OpenAPI spec is the authoritative source.

## Base URL

| Environment | Base URL |
|---|---|
| Local development | `https://localhost:7000/api/v1` |
| Production | `https://<your-container-app>.azurecontainerapps.io/api/v1` |

## Authentication

All protected endpoints require a JWT bearer token in the `Authorization` header:

```http
Authorization: Bearer <token>
```

Obtain a token by calling `POST /auth/login` or `POST /auth/register`.

## Endpoints

### Health

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/healthz` | None | Liveness probe |
| `GET` | `/ready` | None | Readiness probe |

### Authentication

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/auth/register` | None | Register a new user account |
| `POST` | `/auth/login` | None | Log in and receive a JWT |
| `POST` | `/auth/refresh` | None | Refresh an expired JWT |

### Recipes

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/recipes/search` | Optional | Search recipes by query, ingredient, or filter |
| `GET` | `/recipes/{id}` | Optional | Get recipe detail |
| `POST` | `/recipes` | Required | Create a new recipe |
| `PUT` | `/recipes/{id}` | Required | Update an existing recipe |
| `DELETE` | `/recipes/{id}` | Required | Delete a recipe |

### Cook Mode

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/cook-mode/substitute` | Optional | Get ingredient substitution suggestions |

### User Preferences

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/preferences` | Required | Get current user preferences |
| `PUT` | `/preferences` | Required | Update user preferences |

### Profile

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/profile/{userId}` | Optional | Get a user's public profile |
| `PUT` | `/profile` | Required | Update the authenticated user's profile |

### Social

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/social/follow/{userId}` | Required | Follow a user |
| `DELETE` | `/social/follow/{userId}` | Required | Unfollow a user |
| `GET` | `/social/feed` | Required | Get the authenticated user's social feed |

### Media

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/media/upload-url` | Required | Get a pre-signed upload URL for Azure Blob Storage |

## Error Responses

All error responses follow [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) format:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Recipe with ID 'abc123' was not found.",
  "traceId": "00-abc123..."
}
```

## TODO

- Link to the generated OpenAPI spec once available
- Document pagination parameters for list endpoints
- Document rate limiting behaviour
