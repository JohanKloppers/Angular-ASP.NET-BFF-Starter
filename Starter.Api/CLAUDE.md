# Starter.Api — Internal Web API

## What this is
The internal business API. It is **never exposed to the browser** — only the BFF can call it. Every inbound request (except `/api/health`) must carry the `X-Internal-Key` header, validated by `ApiKeyMiddleware`. Runs on port 8080 internally; no port is published to the host in Docker.

## Tech stack
- ASP.NET Core 10 Minimal API
- ASP.NET Core Identity (custom Dapper-backed stores — no Entity Framework)
- Dapper 2.1 + Npgsql 9.0 (PostgreSQL)
- FluentMigrator 6.2 (schema migrations)
- Serilog (compact JSON to stdout)

## Security: API key guard
`ApiKeyMiddleware` runs before all routing. It checks every request for the `X-Internal-Key` header and returns 401 if absent or wrong. `/api/health` is exempt so Docker healthchecks work without a key.

The key value is read from `ApiKey:Value` in configuration. In Docker it is set via the `API_INTERNAL_KEY` environment variable (default: `changeme-internal-api-key`).

## Endpoints
| Method | Path | Rate limit | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | `auth` (10 req/min) | Create user via Identity |
| `POST` | `/api/auth/login` | `auth` (10 req/min) | Validate credentials |
| `GET` | `/api/auth/user/{id:guid}` | — | Fetch user by ID |
| `GET` | `/api/dashboard/stats` | — | Total users + registered today |
| `GET` | `/api/health` | — | Liveness probe (no API key required) |

## Database: Dapper + custom Identity stores
Entity Framework was deliberately removed. All database access uses Dapper with raw SQL.

**Connection factory pattern**: `IDbConnectionFactory` / `NpgsqlConnectionFactory` — inject `IDbConnectionFactory`, call `CreateConnection()`, dispose after use. Never hold open connections.

**Custom Identity stores** live in `Data/Stores/`:
- `ApplicationUserStore` — implements 8 Identity interfaces (`IUserStore`, `IUserPasswordStore`, `IUserEmailStore`, `IUserSecurityStampStore`, `IUserLockoutStore`, `IUserTwoFactorStore`, `IUserClaimStore`, `IUserRoleStore`)
- `ApplicationRoleStore` — implements `IRoleStore<IdentityRole<Guid>>`

All SQL uses double-quoted identifiers (`"AspNetUsers"`) for PostgreSQL compatibility. Set\* methods on the user store update the in-memory object only; the DB write happens in `UpdateAsync`.

**No `.AddDefaultTokenProviders()`** — adding it causes an `IDataProtectionProvider` DI error because DataProtection is not set up in this minimal Identity configuration. Token providers are not needed (no email confirmation, no 2FA).

## Migrations
FluentMigrator runs automatically on startup via `IMigrationRunner.MigrateUp()`. The only migration is `M001_InitialSchema` which creates all ASP.NET Identity tables.

Key details:
- Uses `System.Data.Rule` (not `FluentMigrator.Rule`) for cascade delete in foreign keys — this is a FluentMigrator 6.x requirement.
- `AspNetUsers` has custom columns: `FirstName (text NOT NULL)`, `LastName (text NOT NULL)`, `CreatedAt (timestamptz NOT NULL)`.
- `CreatedAt` is set to `DateTimeOffset.UtcNow` by `ApplicationUserStore.CreateAsync`.

To add a migration: create a new class in `Data/Migrations/`, increment the `[Migration(N)]` number, implement `Up()` and `Down()`.

## Rate limiting
Fixed window limiter named `"auth"`: 10 requests per minute per IP. Applied to `/api/auth/register` and `/api/auth/login`. Returns 429 on overflow.

## Input validation
`ValidationFilter<T>` endpoint filter runs DataAnnotations validation on request bodies. Applied to register and login endpoints.

## Project structure
```
Data/
  IDbConnectionFactory.cs       Abstraction — inject this, not NpgsqlConnection directly
  NpgsqlConnectionFactory.cs    Singleton; wraps NpgsqlConnection creation
  Migrations/
    M001_InitialSchema.cs       Creates all Identity tables
  Stores/
    ApplicationUserStore.cs     Custom Dapper-backed IUserStore (8 interfaces)
    ApplicationRoleStore.cs     Custom Dapper-backed IRoleStore
Endpoints/
  AuthEndpoints.cs              /api/auth/* — register, login, get user by ID
  DashboardEndpoints.cs         /api/dashboard/stats — user count queries
Middleware/
  ApiKeyMiddleware.cs           X-Internal-Key guard (all routes except /api/health)
Filters/
  ValidationFilter.cs           DataAnnotations validation for request bodies
Models/
  ApplicationUser.cs            Extends IdentityUser<Guid> with FirstName, LastName, CreatedAt
Program.cs
```

## appsettings.json key sections
- `ConnectionStrings:DefaultConnection` — Npgsql connection string
- `ApiKey:Value` — the shared secret checked by ApiKeyMiddleware

## Development
```bash
dotnet run   # in Starter.Api/
```
Requires a PostgreSQL instance at `localhost:5432` with database `starterdb`, user `starter`, password `starter_dev_pass` (matches `appsettings.json` defaults). Migrations run automatically.

OpenAPI endpoint is available at `/openapi/v1.json` in Development and Docker environments.
