# Data Models — SentinelStack

> Generated: 2026-01-21 | Source: Entity Analysis

## Database

| Attribute | Value |
|-----------|-------|
| **Engine** | PostgreSQL 14+ |
| **Provider** | Npgsql.EntityFrameworkCore.PostgreSQL 8.0 |
| **ORM** | Entity Framework Core 8.0 |

---

## Entity Relationship Diagram

```
┌─────────────────┐
│      User       │
│─────────────────│
│ Id (PK)         │
│ Name            │
│ Email           │
│ Role            │
│ CreatedAtUtc    │
└────────┬────────┘
         │
         │ 1:N (creates/owns)
         ▼
┌─────────────────┐       ┌─────────────────┐
│    Incident     │◄──────│    AuditLog     │
│─────────────────│  N:1  │─────────────────│
│ Id (PK)         │       │ Id (PK)         │
│ Title           │       │ UserId (FK)     │
│ Description     │       │ Action          │
│ Severity        │       │ EntityType      │
│ Status          │       │ EntityId        │
│ CreatedAtUtc    │       │ Timestamp       │
│ ResolvedAtUtc   │       │ Details (JSON)  │
│ UpdatedAtUtc    │       └─────────────────┘
│ DeletedAtUtc    │
│ ArchivedAtUtc   │
└────────┬────────┘
         │
         │ N:1 (affects)
         ▼
┌─────────────────┐
│    Service      │
│─────────────────│
│ Id (PK)         │
│ Name            │
│ Status          │
│ Endpoint        │
│ CreatedAtUtc    │
└─────────────────┘
```

---

## Entities

### Incident *(Implemented)*

Represents a real operational incident in the system. Tracks status and changes for healthcare-compliant audit trails.

**Location:** `SentinelStack.Domain.Entities.Incident`

| Property | Type | Nullable | Description |
|----------|------|----------|-------------|
| `Id` | `Guid` | No | Primary key (auto-generated) |
| `Title` | `string` | No | Incident title |
| `Description` | `string` | No | Detailed description |
| `Severity` | `string` | No | "critical", "major", "minor" |
| `Status` | `string` | No | "open", "in_progress", "resolved" |
| `CreatedAtUtc` | `DateTime` | No | Creation timestamp |
| `ResolvedAtUtc` | `DateTime?` | Yes | Resolution timestamp |
| `UpdatedAtUtc` | `DateTime?` | Yes | Last update timestamp |
| `DeletedAtUtc` | `DateTime?` | Yes | Soft-delete timestamp |
| `ArchivedAtUtc` | `DateTime?` | Yes | Archive timestamp |

**Domain Methods:**
- `UpdateStatus(string status)` — Update incident status
- `UpdateDescription(string description)` — Update description
- `Resolve()` — Mark as resolved
- `Archive()` — Archive the incident
- `Delete()` — Soft-delete the incident

---

### User *(Placeholder)*

**Location:** `SentinelStack.Domain.Entities.User`

*Entity file exists but not yet implemented.*

**Planned Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Primary key |
| `Name` | `string` | User display name |
| `Email` | `string` | Email address (unique) |
| `Role` | `string` | RBAC role |
| `CreatedAtUtc` | `DateTime` | Creation timestamp |
| `LastLoginUtc` | `DateTime?` | Last login timestamp |

---

### Service *(Placeholder)*

**Location:** `SentinelStack.Domain.Entities.Service`

*Entity file exists but not yet implemented.*

**Planned Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Primary key |
| `Name` | `string` | Service name |
| `Status` | `string` | "healthy", "degraded", "down" |
| `Endpoint` | `string` | Health check URL |
| `LastCheckedUtc` | `DateTime?` | Last health check |
| `CreatedAtUtc` | `DateTime` | Registration timestamp |

---

### AuditLog *(Placeholder)*

**Location:** `SentinelStack.Domain.Entities.AuditLog`

*Entity file exists but not yet implemented.*

**Planned Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Primary key |
| `UserId` | `Guid` | Foreign key to User |
| `Action` | `string` | Action performed |
| `EntityType` | `string` | Affected entity type |
| `EntityId` | `Guid?` | Affected entity ID |
| `Timestamp` | `DateTime` | Action timestamp |
| `Details` | `string` | JSON payload with details |

**Constraints:**
- **Immutable** — No UPDATE or DELETE operations
- **Append-only** — Only INSERT allowed

---

## DbContext

**Location:** `SentinelStack.Infrastructure.Persistence.SentinelDbContext`

*DbContext file exists but not yet implemented.*

**Planned Configuration:**

```csharp
public class SentinelDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Incident configuration
        modelBuilder.Entity<Incident>()
            .HasQueryFilter(i => i.DeletedAtUtc == null);

        // AuditLog - prevent updates/deletes
        modelBuilder.Entity<AuditLog>()
            .ToTable(t => t.HasTrigger("prevent_audit_modification"));
    }
}
```

---

## Migrations

**Status:** Not yet created

**Commands:**
```bash
# Create initial migration
dotnet ef migrations add InitialCreate \
  --project src/SentinelStack.Infrastructure \
  --startup-project src/SentinelStack.Api

# Apply migration
dotnet ef database update \
  --project src/SentinelStack.Infrastructure \
  --startup-project src/SentinelStack.Api
```

---

## Implementation Status

| Entity | File Exists | Implemented | DbSet |
|--------|-------------|-------------|-------|
| Incident | ✅ | ✅ Full | ⏳ Pending |
| User | ✅ | ⏳ Empty | ⏳ Pending |
| Service | ✅ | ⏳ Empty | ⏳ Pending |
| AuditLog | ✅ | ⏳ Empty | ⏳ Pending |
