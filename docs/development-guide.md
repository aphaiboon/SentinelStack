# Development Guide — SentinelStack

> Generated: 2026-01-21 | Scan Level: Quick

## Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| .NET SDK | 10.0+ | Target framework |
| PostgreSQL | 14+ | Primary database (per architecture) |
| IDE | VS 2022 / Rider / VS Code | Any .NET-compatible IDE |

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/aphaiboon/sentinelstack.git
cd sentinelstack
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the API

```bash
cd src/SentinelStack.Api
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Project Structure

```
src/
├── SentinelStack.Api/          # Run this project
├── SentinelStack.Application/  # Business logic (to be implemented)
├── SentinelStack.Domain/       # Domain entities
└── SentinelStack.Infrastructure/ # Data access
```

## Common Commands

| Command | Purpose |
|---------|---------|
| `dotnet restore` | Restore NuGet packages |
| `dotnet build` | Build all projects |
| `dotnet run --project src/SentinelStack.Api` | Run the API |
| `dotnet test` | Run tests (none configured yet) |
| `dotnet ef migrations add <Name>` | Create new migration |
| `dotnet ef database update` | Apply migrations |

## Configuration

Configuration files are located in `src/SentinelStack.Api/`:

| File | Purpose |
|------|---------|
| `appsettings.json` | Base configuration |
| `appsettings.Development.json` | Development overrides |

### Database Connection

Database connection string should be configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=sentinelstack;Username=postgres;Password=your_password"
  }
}
```

## Entity Framework Core

The project uses EF Core with PostgreSQL (Npgsql provider).

**DbContext:** `SentinelStack.Infrastructure.Persistence.SentinelDbContext`

**Domain Entities:**
- `User` — User management
- `Service` — Service registry
- `Incident` — Incident tracking
- `AuditLog` — Immutable audit trail

## Development Status

| Area | Status |
|------|--------|
| Project structure | ✅ Complete |
| Domain entities | ✅ Defined |
| DbContext | ✅ Created |
| Migrations | ⏳ Not yet created |
| API Controllers | 🚧 1 controller (Health) |
| Authentication | ⏳ Not implemented |
| Tests | ⏳ Not configured |
| CI/CD | ⏳ Not configured |
| Docker | ⏳ Not configured |

## Next Steps

1. Configure database connection string
2. Create initial EF Core migration
3. Implement remaining API controllers
4. Add authentication/authorization
5. Set up test projects
6. Configure CI/CD pipeline
