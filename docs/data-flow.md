# SentinelStack — Incident Lifecycle & Data Flow

This diagram illustrates how SentinelStack handles incidents from initial detection through resolution,
with audit logging at every step.

```mermaid
flowchart TD
    Event[Service Event / Signal]
    API[ASP.NET Core Web API]
    Incident[Create or Update Incident]
    Audit1[Write Audit Log Entry]
    Resolution[Incident Resolved]
    Audit2[Write Resolution Audit Log]

    Event --> API
    API --> Incident
    Incident --> Audit1
    Incident --> Resolution
    Resolution --> Audit2
```

## Design Notes

- All state changes are mediated through the API
- Audit logs are written for **every meaningful action**
- Resolution is a first-class, auditable event
- No automated remediation occurs without human involvement
