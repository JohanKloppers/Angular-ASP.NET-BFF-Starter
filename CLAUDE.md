# AngCSharpStarter — Architecture Overview

## What this project is
A production-ready starter template for an Angular + ASP.NET Core application. It demonstrates a BFF (Backend for Frontend) pattern with cookie-based auth, a hardened internal API, and a full Docker Compose deployment. The goal is to serve as a reusable foundation — not a demo project.

## Stack
| Layer | Technology |
|---|---|
| Frontend | Angular 21, standalone components, **no Zone.js** (zoneless) |
| BFF | ASP.NET Core 10, YARP reverse proxy, cookie auth |
| API | ASP.NET Core 10, Dapper + Npgsql, FluentMigrator, ASP.NET Core Identity |
| Database | PostgreSQL 16 |
| Container | Docker Compose (4 services) |

## Service layout
```
Starter.Client/    Angular SPA (served by Nginx on port 4200)
Starter.Bff/       BFF — the only backend the browser talks to (port 5000 in Docker)
Starter.Api/       Internal API — never exposed to the browser directly
Starter.Bff.Tests/ Integration tests for the BFF
docker-compose.yml Orchestrates all 4 services: postgres, api, bff, client
```

## Request lifecycle (browser → database)

```
Browser
  └─ HTTP → Nginx (port 4200)
       ├─ Static assets / SPA routes → serve dist/index.html
       ├─ POST /login, POST /register → proxy_pass http://bff:8080
       └─ /logout, /user, /health, /api/* → proxy_pass http://bff:8080
            └─ BFF (cookie auth)
                 ├─ /login, /register  → BFF calls API POST /api/auth/* (HTTP + X-Internal-Key)
                 ├─ /logout            → BFF signs out cookie, no API call
                 ├─ /user              → BFF reads ClaimsPrincipal from cookie, returns claims
                 └─ /api/**            → YARP reverse-proxies to API (adds X-Internal-Key header)
                      └─ API (ApiKeyMiddleware guards ALL routes except /api/health)
                           └─ Dapper → PostgreSQL
```

## Authentication model
- **Session**: ASP.NET Core cookie auth in the BFF. Cookie name `session`, HttpOnly, Secure, SameSite=Strict, 8-hour sliding expiry.
- **CSRF**: Double-submit cookie. BFF sets `X-CSRF-TOKEN` (non-HttpOnly) cookie. Angular reads it and sends it as the `X-CSRF-TOKEN` request header on all mutating requests. BFF `CsrfMiddleware` validates them match. `/login`, `/register`, `/health` are CSRF-exempt because no session exists yet.
- **Internal API key**: Every request from BFF → API must include `X-Internal-Key` header. Injected globally by YARP `AddTransforms` in `ServiceExtensions.cs`. `ApiKeyMiddleware` in the API validates it on every request (except `/api/health`).

## Critical non-obvious behaviours

### Angular 21 is zoneless
There is NO `zone.js` polyfill and no `provideZoneChangeDetection()` in `app.config.ts`. This means **plain property assignments inside `subscribe()` callbacks do NOT trigger change detection**. All async state that drives the template must use:
- `signal()` / `toSignal()` from `@angular/core/rxjs-interop`
- or the `async` pipe

Never use `this.someProperty = value` inside a `.subscribe()` callback and bind `someProperty` directly in a template — it will appear stale until the next CD cycle is triggered by something else (e.g., a router event or user interaction).

### YARP transform must use `AddTransforms`, not `ITransformFactory`
`ITransformFactory.Build()` is only called for routes that have explicit `Transforms` config entries. Since the YARP `api` route in `appsettings.json` has no `Transforms` block, using `ITransformFactory` silently never adds the `X-Internal-Key` header. Use `.AddTransforms(ctx => { ctx.AddRequestTransform(...) })` which applies unconditionally to all routes.

### Nginx routes GET /login and GET /register to Angular, not the BFF
The BFF only exposes POST `/login` and POST `/register`. Direct browser navigation (GET) to these URLs must be served by Angular (SPA fallback). Nginx uses `if ($request_method = POST)` to conditionally proxy to BFF; GET falls through to `try_files ... /index.html`.

### AuthService.isLoadingSubject starts as `true`
The `isLoading` BehaviorSubject in `auth.service.ts` is initialised to `true` (not `false`). This ensures `AuthGuard` always blocks until `checkAuthStatus()` completes its GET /user request. If it started as `false`, `combineLatest` would immediately emit `[false, false]` and redirect unauthenticated users before the check even fires.

## Docker Compose startup order
```
postgres (healthcheck: pg_isready)
  └─ api (depends_on postgres healthy; runs FluentMigrator migrations on startup)
       └─ bff (depends_on api healthy)
            └─ client (depends_on bff healthy)
```

## Environment variables (Docker)
| Variable | Default | Used by |
|---|---|---|
| `POSTGRES_DB` | `starterdb` | postgres, api |
| `POSTGRES_USER` | `starter` | postgres, api |
| `POSTGRES_PASSWORD` | `starter_dev_pass` | postgres, api |
| `API_INTERNAL_KEY` | `changeme-internal-api-key` | api (`ApiKey:Value`), bff (`Api:InternalKey`) |

Override in a `.env` file at the repo root for local Docker runs.

## Local development (without Docker)
- **API**: `dotnet run` in `Starter.Api/`. Needs a local PostgreSQL on port 5432 (matches `appsettings.json` defaults). Migrations run automatically on startup.
- **BFF**: `dotnet run` in `Starter.Bff/`. Talks to the API at `http://localhost:5110` (default in `appsettings.json`).
- **Client**: `npm start` in `Starter.Client/`. Uses `proxy.conf.json` to forward API calls to the BFF at `http://localhost:5000`.
