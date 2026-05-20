# Starter.Bff — Backend for Frontend

## What this is
The only backend the browser communicates with. It owns the user session (cookie auth), handles CSRF protection, proxies API requests, and shields the internal API from the internet. Runs on port 8080 internally; Docker Compose exposes port 5000.

## Tech stack
- ASP.NET Core 10 Minimal API
- YARP 2.3 reverse proxy
- Cookie authentication (ASP.NET Core built-in)
- Serilog (compact JSON to stdout)

## Endpoints
| Method | Path | Auth required | Description |
|---|---|---|---|
| `POST` | `/login` | No | Calls API `/api/auth/login`, issues session cookie + CSRF cookie |
| `POST` | `/register` | No | Calls API `/api/auth/register`, issues session cookie + CSRF cookie |
| `GET` | `/logout` | Yes | Signs out cookie session |
| `GET` | `/user` | Yes | Returns claims from the current session cookie |
| `GET/POST/...` | `/api/**` | Yes | YARP-proxied to internal API with `X-Internal-Key` header added |
| `GET` | `/health` | No | Liveness probe |

## Middleware pipeline (in order)
```
ForwardedHeaders       — trusts X-Forwarded-Proto / X-Forwarded-For from Nginx
ExceptionHandler       — returns ProblemDetails on unhandled exceptions
SecurityHeaders        — adds X-Content-Type-Options, X-Frame-Options, CSP, etc.
Routing
Authentication         — decrypts session cookie, populates HttpContext.User
Authorization
CsrfMiddleware         — validates X-CSRF-TOKEN header on mutating requests
SerilogRequestLogging
```

## CSRF protection
Double-submit cookie pattern:
- On any GET request to the BFF, if no `X-CSRF-TOKEN` cookie exists, one is created (non-HttpOnly, Secure, SameSite=Strict, value = random GUID).
- `/login`, `/register`, and `/health` are **CSRF-exempt** — no cookie exists yet when these are called.
- On POST/PUT/PATCH/DELETE, the BFF checks that the `X-CSRF-TOKEN` cookie value matches the `X-CSRF-TOKEN` request header exactly. Mismatch → 403.
- Angular's `csrf.interceptor.ts` reads the cookie with `document.cookie` and sends it as a header on all mutating requests.

## Session cookie
- Name: `session`
- HttpOnly: true (JavaScript cannot read it)
- Secure: true (HTTPS only)
- SameSite: Strict
- Sliding expiry: 8 hours

## YARP configuration
Routes and clusters are defined in `appsettings.json` under `ReverseProxy`. In Docker the cluster destination is overridden via environment variable:
```
ReverseProxy__Clusters__api-cluster__Destinations__api-destination__Address=http://api:8080/
```

**Important**: The `X-Internal-Key` header is added to all proxied requests via `AddTransforms()` in `ServiceExtensions.cs`, NOT via `ITransformFactory`. Using `ITransformFactory` only works for routes that have explicit `Transforms` config entries — since the `api` route has none, the factory is never called. `AddTransforms()` applies unconditionally to every route.

## Communication with the API
`ApiAuthService` (typed `HttpClient`) calls:
- `POST /api/auth/login` — validate credentials
- `POST /api/auth/register` — create user
- `GET /api/auth/user/{id}` — fetch user by ID

All calls include `X-Internal-Key` via the HttpClient default headers set in `ServiceExtensions.cs`.

## Project structure
```
Configuration/
  ApiSettings.cs            Strongly-typed config for Api:BaseUrl and Api:InternalKey
  SecurityHeadersConfig.cs  Dictionary of header name → value pairs
Endpoints/
  AuthEndpoints.cs          /login, /register, /logout
  UserEndpoints.cs          /user — returns filtered claims from HttpContext.User
  HealthEndpoints.cs        /health
Extensions/
  AuthenticationExtensions.cs   Cookie auth + authorization setup
  ServiceExtensions.cs          YARP + ApiAuthService + UserService registration
  EndpointExtensions.cs         Groups and maps all endpoints
Middleware/
  SecurityHeadersMiddleware.cs  Injects security response headers
  CsrfMiddleware.cs             Double-submit CSRF validation
Models/
  UserInfo.cs               Response model for /user endpoint
Services/
  ApiAuthService.cs         HttpClient wrapper for API auth endpoints
  UserService.cs            Extracts/filters claims from ClaimsPrincipal
Proxy/
  InternalApiKeyTransform.cs  Kept for reference but NOT used (see YARP note above)
Program.cs
```

## appsettings.json key sections
- `Api:BaseUrl` — API base URL (local: `http://localhost:5110`, Docker: env override)
- `Api:InternalKey` — shared secret with the API (local: `dev-internal-key-not-for-production`)
- `ReverseProxy` — YARP routes and clusters
- `SecurityHeaders:Headers` — dictionary of headers to inject on every response

## Development
```bash
dotnet run   # in Starter.Bff/
```
Requires the API to be running at `http://localhost:5110` (or change `Api:BaseUrl` in appsettings.json).
