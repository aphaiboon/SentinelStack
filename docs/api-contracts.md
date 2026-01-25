# API Contracts — SentinelStack

> Generated: 2026-01-21 | Source: Controller Analysis

## Base URL

- **Development:** `https://localhost:5001/api/v1`
- **Production:** `https://api.sentinelstack.io/api/v1` *(planned)*

## Authentication

*Not yet implemented. RBAC planned.*

---

## Implemented Endpoints

### Health Check

#### `GET /api/v1/health`

Returns system health status with performance metrics.

**Request:**
```http
GET /api/v1/health HTTP/1.1
Host: localhost:5001
```

**Response:** `200 OK`
```json
{
  "status": "ok",
  "service": "sentinelstack-api",
  "version": "1.0.0",
  "timestamp": "2026-01-21T10:30:00Z",
  "uptime": "00.01:23:45",
  "uptimeSeconds": 5025,
  "pid": 12345,
  "threadCount": 15,
  "cpuPercentSinceStart": 2.34,
  "memory": {
    "workingSetMb": 128.50,
    "privateMb": 145.25,
    "gcAllocatedMb": 45.75
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `status` | string | Health status ("ok") |
| `service` | string | Service identifier |
| `version` | string | API version |
| `timestamp` | datetime | Current UTC time |
| `uptime` | string | Formatted uptime (dd.hh:mm:ss) |
| `uptimeSeconds` | long | Uptime in seconds |
| `pid` | int | Process ID |
| `threadCount` | int | Active thread count |
| `cpuPercentSinceStart` | double | CPU usage since start |
| `memory.workingSetMb` | double | Working set memory (MB) |
| `memory.privateMb` | double | Private memory (MB) |
| `memory.gcAllocatedMb` | double | GC allocated memory (MB) |

---

## Planned Endpoints

Based on architecture documentation, the following endpoints are planned:

### Incidents

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/incidents` | List all incidents |
| `GET` | `/api/v1/incidents/{id}` | Get incident by ID |
| `POST` | `/api/v1/incidents` | Create new incident |
| `PATCH` | `/api/v1/incidents/{id}` | Update incident |
| `POST` | `/api/v1/incidents/{id}/resolve` | Resolve incident |
| `POST` | `/api/v1/incidents/{id}/archive` | Archive incident |
| `DELETE` | `/api/v1/incidents/{id}` | Soft-delete incident |

### Services

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/services` | List registered services |
| `GET` | `/api/v1/services/{id}` | Get service by ID |
| `POST` | `/api/v1/services` | Register new service |
| `PATCH` | `/api/v1/services/{id}` | Update service |
| `DELETE` | `/api/v1/services/{id}` | Remove service |

### Users

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/users` | List users |
| `GET` | `/api/v1/users/{id}` | Get user by ID |
| `POST` | `/api/v1/users` | Create user |
| `PATCH` | `/api/v1/users/{id}` | Update user |
| `DELETE` | `/api/v1/users/{id}` | Deactivate user |

### Audit Logs

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/audit-logs` | List audit logs (read-only) |
| `GET` | `/api/v1/audit-logs/{id}` | Get audit log entry |

*Note: Audit logs are immutable — no POST/PATCH/DELETE operations.*

---

## Error Responses

### Standard Error Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "errors": {
    "Title": ["The Title field is required."]
  }
}
```

### HTTP Status Codes

| Code | Description |
|------|-------------|
| `200` | Success |
| `201` | Created |
| `400` | Bad Request (validation error) |
| `401` | Unauthorized |
| `403` | Forbidden |
| `404` | Not Found |
| `500` | Internal Server Error |

---

## API Documentation

Swagger UI available at `/swagger` in development mode.

```bash
# Run API and access Swagger
dotnet run --project src/SentinelStack.Api
open https://localhost:5001/swagger
```
