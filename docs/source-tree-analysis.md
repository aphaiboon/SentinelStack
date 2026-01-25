# Source Tree Analysis — SentinelStack

> Generated: 2026-01-21 | Scan Level: Quick

## Project Structure

```
SentinelStack/
├── SentinelStack.sln              # Solution file
├── README.md                       # Project overview
├── CLAUDE.md                       # AI assistant guidelines
├── .gitignore                      # Git ignore rules
│
├── src/                            # Source code (Clean Architecture)
│   │
│   ├── SentinelStack.Api/          # [PRESENTATION LAYER]
│   │   ├── Controllers/            # API endpoints
│   │   │   └── HealthController.cs # Health check endpoint
│   │   ├── Program.cs              # ★ Entry point
│   │   ├── appsettings.json        # Configuration
│   │   └── appsettings.Development.json
│   │
│   ├── SentinelStack.Application/  # [APPLICATION LAYER]
│   │   └── (empty - use cases to be added)
│   │
│   ├── SentinelStack.Domain/       # [DOMAIN LAYER]
│   │   └── Entities/               # Domain entities
│   │       ├── AuditLog.cs         # Immutable audit trail
│   │       ├── Incident.cs         # Incident tracking
│   │       ├── Service.cs          # Service registry
│   │       └── User.cs             # User management
│   │
│   └── SentinelStack.Infrastructure/ # [INFRASTRUCTURE LAYER]
│       └── Persistence/            # Data access
│           └── SentinelDbContext.cs # EF Core DbContext
│
├── docs/                           # Documentation
│   ├── architecture.md             # High-level architecture diagram
│   ├── api.md                      # API documentation
│   ├── data-flow.md                # Incident lifecycle
│   ├── domain-model.md             # Domain model docs
│   └── plan.md                     # Development plan
│
└── infra/                          # Infrastructure as Code
    └── terraform/                  # (placeholder - empty)
```

## Layer Dependencies

```
┌─────────────────────┐
│   SentinelStack.Api │  ← Entry point, Controllers
└──────────┬──────────┘
           │ references
           ▼
┌─────────────────────────────┐
│ SentinelStack.Application   │  ← Use cases, business logic orchestration
└──────────┬──────────────────┘
           │ references
           ▼
┌─────────────────────────────┬─────────────────────────────────┐
│  SentinelStack.Domain       │  SentinelStack.Infrastructure   │
│  (Entities, Domain Logic)   │  (Data Access, External Svcs)   │
└─────────────────────────────┴─────────────────────────────────┘
           ▲                              │
           └──────────── references ──────┘
```

## Critical Folders

| Folder | Purpose | Key Files |
|--------|---------|-----------|
| `src/SentinelStack.Api/` | Web API entry point | `Program.cs`, Controllers |
| `src/SentinelStack.Domain/Entities/` | Domain model | 4 entity classes |
| `src/SentinelStack.Infrastructure/Persistence/` | Data access | `SentinelDbContext.cs` |
| `docs/` | Documentation | Architecture, API, domain docs |

## Entry Points

| Entry Point | Location | Purpose |
|-------------|----------|---------|
| `Program.cs` | `src/SentinelStack.Api/` | Application bootstrap and configuration |

## Source File Inventory

| Layer | Files | Status |
|-------|-------|--------|
| Api | 2 | `Program.cs`, `HealthController.cs` |
| Application | 0 | Empty (use cases pending) |
| Domain | 4 | `User.cs`, `Service.cs`, `Incident.cs`, `AuditLog.cs` |
| Infrastructure | 1 | `SentinelDbContext.cs` |
| **Total** | **7** | Early-stage implementation |
