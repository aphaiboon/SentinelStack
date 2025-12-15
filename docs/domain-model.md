# SentinelStack — Domain Model

This document describes the core domain entities within SentinelStack and how they relate to one another.  
The model is intentionally minimal and audit-focused, reflecting regulated healthcare environments.

```mermaid
erDiagram
    USER {
        uuid id
        string email
        string role
        datetime created_at
    }

    SERVICE {
        uuid id
        string name
        datetime created_at
    }

    INCIDENT {
        uuid id
        string status
        string severity
        datetime created_at
        datetime resolved_at
    }

    AUDIT_LOG {
        uuid id
        string entity_type
        uuid entity_id
        string action
        string actor
        datetime timestamp
    }

    USER ||--o{ INCIDENT : creates
    SERVICE ||--o{ INCIDENT : owns
    INCIDENT ||--o{ AUDIT_LOG : records
    USER ||--o{ AUDIT_LOG : performs
```

## Design Notes

- **AuditLog** is append-only and immutable
- **Incident** represents a real operational event, not a synthetic alert
- **User actions** are always explicitly recorded
- The model favors traceability over optimization
