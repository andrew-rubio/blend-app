# REST API

!!! note
    The Blend REST API is planned for a future release. This page will be updated when the API is available.

## Planned Endpoints

The following API endpoints are under consideration for the Blend service layer:

### Health

| Method | Path | Description |
|---|---|---|
| `GET` | `/health` | Service health check |
| `GET` | `/health/ready` | Readiness probe |

### Workflows

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/v1/workflows/prd` | Trigger PRD generation |
| `POST` | `/api/v1/workflows/plan` | Trigger task planning |
| `GET` | `/api/v1/workflows/{id}/status` | Get workflow status |

### Specifications

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/v1/specs` | List all specification files |
| `GET` | `/api/v1/specs/{type}` | Get specification by type |

## Authentication

The API will use token-based authentication. Include the token in the `Authorization` header:

```http
Authorization: Bearer <token>
```

## Error Responses

All error responses follow this format:

```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "The requested resource was not found.",
    "details": {}
  }
}
```

## Contributing

If you'd like to contribute to the API design, open an issue on the [GitHub repository](https://github.com/andrew-rubio/blend-app/issues).
