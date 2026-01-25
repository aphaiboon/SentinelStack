# SentinelStack Documentation Index

> Generated: 2026-01-21 | Mode: Initial Scan | Level: Quick
>
> **This is the primary entry point for AI-assisted development.**

---

## Project Overview

| Attribute | Value |
|-----------|-------|
| **Type** | Monolith (Backend API) |
| **Primary Language** | C# / .NET 10 |
| **Architecture** | Clean Architecture (4-layer) |
| **Database** | PostgreSQL |
| **Cloud Target** | AWS |

---

## Quick Reference

| Item | Value |
|------|-------|
| **Tech Stack** | ASP.NET Core 10, EF Core 8, PostgreSQL, Swagger |
| **Entry Point** | `src/SentinelStack.Api/Program.cs` |
| **Architecture Pattern** | Clean Architecture |
| **Source Files** | 7 files across 4 layers |
| **Domain Entities** | User, Service, Incident, AuditLog |

---

## Generated Documentation

### Core Documents

| Document | Description |
|----------|-------------|
| [Project Overview](./project-overview.md) | Executive summary and quick start |
| [Project Architecture](./project-architecture.md) | Comprehensive architecture details |
| [Source Tree Analysis](./source-tree-analysis.md) | Code structure and folder purposes |
| [Development Guide](./development-guide.md) | Setup, commands, configuration |
| [API Contracts](./api-contracts.md) | Endpoint specifications and examples |
| [Data Models](./data-models.md) | Database schema and entity details |
| [Deployment Guide](./deployment-guide.md) | AWS deployment instructions |

### Existing Documentation (Pre-scan)

| Document | Description |
|----------|-------------|
| [Architecture Diagram](./architecture.md) | High-level visual architecture |
| [Domain Model](./domain-model.md) | Entity relationships and design |
| [Data Flow](./data-flow.md) | Incident lifecycle / data flow |
| [API Documentation](./api.md) | Endpoint reference |
| [Plan](./plan.md) | Development planning notes |

---

## Getting Started

### For Developers

```bash
# Clone and setup
git clone https://github.com/aphaiboon/sentinelstack.git
cd sentinelstack
dotnet restore
dotnet build

# Run the API
dotnet run --project src/SentinelStack.Api

# Access Swagger
open https://localhost:5001/swagger
```

### For AI Agents

When working on this codebase:

1. **Start here** — This index provides navigation to all documentation
2. **Architecture context** — See [Project Architecture](./project-architecture.md) for design decisions
3. **Code structure** — See [Source Tree Analysis](./source-tree-analysis.md) for folder purposes
4. **Development setup** — See [Development Guide](./development-guide.md) for commands

---

## Project Status

| Area | Status | Notes |
|------|--------|-------|
| Core structure | ✅ Complete | Clean Architecture in place |
| Domain entities | ✅ Defined | 4 entities |
| API controllers | 🚧 Partial | 1 of ~4 controllers |
| Database | ⏳ Pending | Entities defined, migrations needed |
| Authentication | ⏳ Pending | RBAC planned |
| Tests | ⏳ Pending | No test projects |
| CI/CD | ⏳ Pending | Not configured |
| Docker | ⏳ Pending | Not configured |

---

## Scan Metadata

| Field | Value |
|-------|-------|
| Scan Date | 2026-01-21 |
| Scan Mode | Initial Scan |
| Scan Level | Quick |
| State File | [project-scan-report.json](./project-scan-report.json) |
