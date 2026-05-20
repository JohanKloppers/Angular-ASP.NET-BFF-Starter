# Starter.Api

ASP.NET Core 10 Web API. Handles user management via ASP.NET Core Identity and exposes data endpoints consumed exclusively by the BFF. Not publicly accessible — protected by an internal API key.

## Tech Stack

- ASP.NET Core 10 Minimal APIs
- ASP.NET Core Identity
- Entity Framework Core 9 + Npgsql (PostgreSQL)
- Serilog (structured JSON logging)

## Endpoints

All endpoints require the `X-Internal-Key` header matching the configured `ApiKey:Value`. The `/api/health` endpoint is exempt.

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | API key | Create a new user account |
| `POST` | `/api/auth/login` | API key | Validate credentials, return user info |
| `GET` | `/api/auth/user/{id}` | API key | Get user by ID |
| `GET` | `/api/dashboard/stats` | API key | Total users and registrations today |
| `GET` | `/api/health` | None | Health check |

Auth endpoints are rate-limited to 10 requests/minute per client.

## Configuration

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=starterdb;Username=starter;Password=..."
  },
  "ApiKey": {
    "Value": "dev-internal-key-not-for-production"
  }
}
```

In Docker, these are overridden via environment variables in `docker-compose.yml`.

## Local Development

Requires PostgreSQL running locally (or via Docker).

```bash
# Start postgres only
docker-compose up postgres -d

# Run the API
dotnet run --project Starter.Api
```

OpenAPI docs: `http://localhost:5110/openapi/v1.json`

## Database Migrations

Migrations live in `Data/Migrations/` and are applied automatically on startup via `MigrateAsync()`.

**Add a migration after changing a model:**
```bash
dotnet ef migrations add YourMigrationName --project Starter.Api --output-dir Data/Migrations
```

**First run on an existing database (created before migrations were introduced):**

Drop the volume and let migrations start fresh:
```bash
docker-compose down -v
docker-compose up --build
```
