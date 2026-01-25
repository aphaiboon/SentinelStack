# Project Architecture — SentinelStack

> Generated: 2026-01-21 | Scan Level: Quick | Type: Backend (Clean Architecture)

## Executive Summary

SentinelStack is an open-source, healthcare-ready cloud platform providing system reliability monitoring, security controls, and immutable audit trails for regulated web applications. Built with .NET 10 using Clean Architecture principles.

**Target Environments:** Healthcare, life sciences, enterprise SaaS where compliance and auditability are critical.

## Technology Stack

| Category | Technology | Version | Purpose |
|----------|-----------|---------|---------|
| Runtime | .NET | 10.0 | Application framework |
| Language | C# | Latest | Primary language |
| Web Framework | ASP.NET Core | 10.0 | REST API |
| ORM | Entity Framework Core | 8.0 | Data access |
| Database | PostgreSQL | 14+ | Primary data store |
| API Documentation | Swashbuckle/Swagger | 10.0.1 | OpenAPI specs |
| Cloud Target | AWS | - | Deployment platform |

## Architecture Pattern

**Pattern:** Clean Architecture (Onion Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                    External Systems                          │
│              (Clients, Admin UI, Background Workers)         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER                         │
│                   SentinelStack.Api                          │
│          (Controllers, Middleware, API Configuration)        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                          │
│                SentinelStack.Application                     │
│       (Use Cases, Application Services, DTOs, Interfaces)    │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
┌─────────────────────────┐   ┌─────────────────────────────────┐
│      DOMAIN LAYER       │   │     INFRASTRUCTURE LAYER        │
│  SentinelStack.Domain   │   │  SentinelStack.Infrastructure   │
│   (Entities, Domain     │   │    (Data Access, External       │
│    Logic, Interfaces)   │   │     Services, Repositories)     │
└─────────────────────────┘   └─────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Project | Responsibilities |
|-------|---------|-----------------|
| **Presentation** | SentinelStack.Api | HTTP handling, routing, serialization, authentication, API documentation |
| **Application** | SentinelStack.Application | Use cases, business logic orchestration, DTOs, validation |
| **Domain** | SentinelStack.Domain | Entities, domain events, business rules, interfaces |
| **Infrastructure** | SentinelStack.Infrastructure | EF Core DbContext, repositories, external service clients |

### Dependency Rules

- Inner layers know nothing about outer layers
- Domain has no external dependencies
- Application depends only on Domain
- Infrastructure implements Domain interfaces
- Presentation orchestrates all layers

## Domain Model

### Core Entities

| Entity | Purpose | Key Fields |
|--------|---------|------------|
| **User** | User accounts and authentication | Id, Name, Email, Role |
| **Service** | Registered services being monitored | Id, Name, Status, Endpoint |
| **Incident** | Tracked incidents and responses | Id, ServiceId, Severity, Status, Timeline |
| **AuditLog** | Immutable audit trail (append-only) | Id, UserId, Action, Timestamp, Details |

### Entity Relationships

```
User ──────────────────┐
  │                    │
  │ creates/owns       │ performs actions
  ▼                    ▼
Incident ◄─────────── AuditLog
  │
  │ affects
  ▼
Service
```

## Data Architecture

### Database: PostgreSQL

**Primary Storage:** AWS RDS PostgreSQL

**Key Tables:**
- `Users` — User management with RBAC
- `Services` — Service registry
- `Incidents` — Incident tracking with timeline
- `AuditLogs` — Append-only immutable audit trail

### DbContext

**Location:** `SentinelStack.Infrastructure.Persistence.SentinelDbContext`

```csharp
public class SentinelDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
}
```

## API Design

### Current Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/health` | GET | Health check |

### Planned Endpoints (from architecture docs)

- `/api/users` — User management
- `/api/services` — Service registry
- `/api/incidents` — Incident management
- `/api/audit-logs` — Audit trail (read-only)

### API Documentation

Swagger/OpenAPI available at `/swagger` when running in development.

## Security Architecture

### Core Principles

1. **Human-in-the-loop decision making** — No autonomous actions
2. **Least-privilege access** — Role-based access control
3. **Audit-first architecture** — All actions logged
4. **Operational accountability** — Clear ownership

### Planned Security Features

- Role-based access control (RBAC)
- Immutable, append-only audit logs
- Encryption at rest and in transit
- Explicit ownership of actions
- No silent data modification

## Deployment Architecture

### Target: AWS Cloud

```
┌─────────────────────────────────────────────────────────┐
│                      AWS Cloud                           │
│  ┌─────────────────┐  ┌─────────────────────────────┐  │
│  │  ASP.NET Core   │  │    Background Workers       │  │
│  │    Web API      │  │  (Incident Evaluation,      │  │
│  │                 │  │   Alerting, Analytics/ML)   │  │
│  └────────┬────────┘  └──────────────┬──────────────┘  │
│           │                          │                  │
│           └──────────┬───────────────┘                  │
│                      ▼                                  │
│           ┌─────────────────────┐                       │
│           │  PostgreSQL (RDS)   │                       │
│           │  - Users            │                       │
│           │  - Services         │                       │
│           │  - Incidents        │                       │
│           │  - AuditLogs        │                       │
│           └─────────────────────┘                       │
└─────────────────────────────────────────────────────────┘
```

### Current Status

| Component | Status |
|-----------|--------|
| API | 🚧 Basic structure |
| Database schema | ⏳ Entities defined, migrations pending |
| Background workers | ⏳ Not implemented |
| Infrastructure as Code | ⏳ Placeholder only |

## Testing Strategy

### Planned Test Types

| Type | Project | Purpose |
|------|---------|---------|
| Unit Tests | SentinelStack.Domain.Tests | Domain logic |
| Integration Tests | SentinelStack.Api.Tests | API endpoints, database |
| Architecture Tests | - | Dependency rule enforcement |

### Current Status

No test projects configured yet.

## Related Documentation

- [High-Level Architecture Diagram](./architecture.md)
- [Domain Model](./domain-model.md)
- [Data Flow / Incident Lifecycle](./data-flow.md)
- [API Documentation](./api.md)
- [Development Guide](./development-guide.md)
- [Source Tree Analysis](./source-tree-analysis.md)
