# Starter — Angular + C# BFF

A professional full-stack starter template built with **Angular 21**, **ASP.NET Core 10**, **TypeScript**, **Tailwind CSS v4**, and **PostgreSQL**, following the Backend-for-Frontend (BFF) pattern.

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21 · TypeScript · Tailwind CSS v4 |
| BFF | ASP.NET Core 10 · YARP reverse proxy · Cookie auth · CSRF protection |
| API | ASP.NET Core 10 · ASP.NET Core Identity · EF Core 9 · Npgsql |
| Database | PostgreSQL 16 |
| Logging | Serilog (structured JSON) |
| Infrastructure | Docker · Docker Compose · nginx |

## Architecture

```
Browser
  │
  └─► nginx (port 4200)
        ├── /login, /logout, /register, /user, /health
        │       └─► BFF (port 8080, internal)
        │             ├── Cookie session auth
        │             ├── CSRF double-submit validation
        │             └─► API (port 8080, internal only)
        │                   ├── ASP.NET Core Identity
        │                   ├── Internal API key validation
        │                   └─► PostgreSQL
        └── /* (Angular SPA)
```

The API is **not exposed outside Docker**. Only the nginx client port (4200) is public. The BFF acts as the sole gateway — it manages sessions, validates CSRF tokens, and proxies authenticated requests to the API using a shared internal key.

## Project Structure

```
AngCSharpStarter/
├── Starter.Api/          # ASP.NET Core 10 Web API (Identity + PostgreSQL)
├── Starter.Bff/          # ASP.NET Core 10 BFF (YARP + cookie auth)
├── Starter.Bff.Tests/    # xUnit tests for BFF middleware and services
├── Starter.Client/       # Angular 21 SPA (TypeScript + Tailwind CSS)
├── docker-compose.yml    # 4-service Docker stack
├── .env.example          # Environment variable template
└── Starter.slnx          # Visual Studio solution
```

## Quick Start (Docker)

**1. Create your `.env` file:**
```bash
cp .env.example .env
```

Edit `.env` with your values:
```env
POSTGRES_DB=starterdb
POSTGRES_USER=starter
POSTGRES_PASSWORD=your-strong-password
API_INTERNAL_KEY=your-strong-internal-key
```

**2. Build and start all services:**
```bash
docker-compose up --build
```

The app will be available at **http://localhost:4200**.

> **First run note:** If you previously ran the app with an older version that used `EnsureCreatedAsync`, you need to wipe the database volume before starting:
> ```bash
> docker-compose down -v
> docker-compose up --build
> ```
> The `-v` flag removes the `postgres_data` volume so EF Core migrations can create the schema cleanly.

## Local Development

Run each service individually for a faster dev loop with hot reload.

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)

**1. Start PostgreSQL only:**
```bash
docker-compose up postgres -d
```

**2. Run the API:**
```bash
dotnet run --project Starter.Api
```
API available at `http://localhost:5110`. OpenAPI docs at `http://localhost:5110/openapi/v1.json`.

**3. Run the BFF:**
```bash
dotnet run --project Starter.Bff
```
BFF available at `http://localhost:5000`.

**4. Run the Angular dev server:**
```bash
cd Starter.Client
npm install
npm start
```
App available at **http://localhost:4200**. The Angular dev server proxies all BFF routes via `proxy.conf.json`.

## Key Features

- **BFF pattern** — Angular never calls the API directly. The BFF owns the session cookie and proxies requests via YARP.
- **Cookie authentication** — HttpOnly, SameSite=Strict, Secure. No tokens in localStorage.
- **CSRF protection** — Double-submit cookie pattern. Angular reads the `X-CSRF-TOKEN` cookie and echoes it back as a request header.
- **ASP.NET Core Identity** — Full user management with password hashing, unique email enforcement, and EF Core migrations.
- **Rate limiting** — Auth endpoints capped at 10 requests/minute per client.
- **Input validation** — DataAnnotations enforced via a reusable `ValidationFilter<T>`, returning RFC 7807 `ValidationProblem`.
- **Structured logging** — Serilog with compact JSON output, configurable log levels per environment.
- **Security headers** — X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Content-Security-Policy.
- **EF Core migrations** — Schema is versioned and applied automatically on startup.

## Running Tests

```bash
dotnet test Starter.Bff.Tests
```

## Adding a New Migration

When you change a model in `Starter.Api`:
```bash
dotnet ef migrations add YourMigrationName --project Starter.Api --output-dir Data/Migrations
```

The migration is applied automatically the next time the API starts.

## Environment Variables

| Variable | Description | Default |
|---|---|---|
| `POSTGRES_DB` | Database name | `starterdb` |
| `POSTGRES_USER` | PostgreSQL user | `starter` |
| `POSTGRES_PASSWORD` | PostgreSQL password | `starter_dev_pass` |
| `API_INTERNAL_KEY` | Shared secret between BFF and API | `changeme-internal-api-key` |
