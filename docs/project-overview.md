# Project Overview — SentinelStack

> Generated: 2026-01-21 | Scan Level: Quick

## Summary

| Attribute | Value |
|-----------|-------|
| **Project Name** | SentinelStack |
| **Type** | Backend API (Monolith) |
| **Architecture** | Clean Architecture (4-layer) |
| **Primary Language** | C# / .NET 10 |
| **Database** | PostgreSQL |
| **Cloud Target** | AWS |
| **Status** | Under Development |

## Purpose

SentinelStack is an open-source, healthcare-ready cloud platform that provides:

- **System reliability monitoring** — Observe service health
- **Security controls** — Role-based access control
- **Immutable audit trails** — Compliance-ready logging

Designed for regulated environments (healthcare, life sciences, enterprise SaaS) where downtime, data exposure, and undocumented decisions carry real risk.

## Core Principles

1. **Human-in-the-loop decision making** — No autonomous remediation
2. **Least-privilege access** — RBAC enforcement
3. **Audit-first architecture** — All actions logged immutably
4. **Operational accountability** — Clear ownership of every action

## Tech Stack Summary

| Layer | Technology |
|-------|------------|
| API | ASP.NET Core 10.0 |
| ORM | Entity Framework Core 8.0 |
| Database | PostgreSQL (Npgsql) |
| API Docs | Swagger/OpenAPI |

## Project Structure

```
src/
├── SentinelStack.Api/            # Web API (entry point)
├── SentinelStack.Application/    # Use cases
├── SentinelStack.Domain/         # Entities
└── SentinelStack.Infrastructure/ # Data access
```

## Domain Entities

| Entity | Purpose |
|--------|---------|
| User | User accounts and RBAC |
| Service | Monitored service registry |
| Incident | Incident tracking and response |
| AuditLog | Immutable audit trail |

## Quick Start

```bash
# Restore and build
dotnet restore
dotnet build

# Run the API
dotnet run --project src/SentinelStack.Api

# Access Swagger UI
open https://localhost:5001/swagger
```

## Documentation Index

| Document | Description |
|----------|-------------|
| [Project Architecture](./project-architecture.md) | Comprehensive architecture details |
| [Architecture Diagram](./architecture.md) | High-level visual overview |
| [Domain Model](./domain-model.md) | Entity relationships |
| [Data Flow](./data-flow.md) | Incident lifecycle |
| [API Documentation](./api.md) | Endpoint reference |
| [Development Guide](./development-guide.md) | Setup and commands |
| [Source Tree Analysis](./source-tree-analysis.md) | Code structure |

## Development Status

| Area | Status |
|------|--------|
| Core structure | ✅ Complete |
| Domain entities | ✅ Defined |
| API controllers | 🚧 1 of ~4 |
| Database migrations | ⏳ Pending |
| Authentication | ⏳ Not started |
| Tests | ⏳ Not started |
| CI/CD | ⏳ Not configured |
