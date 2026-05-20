# Starter.Bff

ASP.NET Core 10 Backend-for-Frontend (BFF). The sole entry point for the Angular SPA. Manages cookie sessions, validates CSRF tokens, and proxies authenticated API requests via YARP.

## Tech Stack

- ASP.NET Core 10 Minimal APIs
- YARP 2.3 (reverse proxy)
- ASP.NET Core cookie authentication
- Serilog (structured JSON logging)

## How It Works

```
Angular ──► BFF ──► API
            │
            ├── Cookie session (HttpOnly, Secure, SameSite=Strict)
            ├── CSRF double-submit validation
            └── X-Internal-Key header injected by YARP transform
```

1. Angular calls `/login` or `/register` on the BFF with credentials.
2. The BFF calls the API to validate/create the user.
3. On success, the BFF issues a signed cookie session and sets a CSRF cookie.
4. All subsequent API calls from Angular go through `/api/*` on the BFF, which YARP proxies to the internal API — automatically injecting the `X-Internal-Key` header.

## Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/login` | Anonymous | Sign in, issue session cookie |
| `POST` | `/register` | Anonymous | Register, issue session cookie |
| `GET` | `/logout` | Required | Sign out, clear session |
| `GET` | `/user` | Required | Return current user's claims |
| `GET` | `/health` | Anonymous | Health check |
| `ANY` | `/api/{**}` | Required | Proxy to internal API |

## Security

- **Cookie**: HttpOnly, Secure (`Always`), SameSite=Strict, 8-hour sliding expiration.
- **CSRF**: `/login` and `/register` are exempt (no session yet). All other mutating requests require a matching `X-CSRF-TOKEN` cookie and header.
- **Internal API key**: Injected via `InternalApiKeyTransform` (YARP `ITransformFactory`). The Angular client never sees this key.
- **Security headers**: X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Content-Security-Policy — configured in `appsettings.json` under `SecurityHeaders.Headers`.
- **Forwarded headers**: `UseForwardedHeaders()` is configured to trust `X-Forwarded-Proto` from nginx, ensuring `CookieSecurePolicy.Always` works correctly behind a reverse proxy.

## Configuration

`appsettings.json`:
```json
{
  "Api": {
    "BaseUrl": "http://localhost:5110",
    "InternalKey": "dev-internal-key-not-for-production"
  }
}
```

In Docker, `Api__BaseUrl` and `Api__InternalKey` are set via environment variables.

## Local Development

```bash
# Start postgres and api first
docker-compose up postgres api -d

# Run the BFF
dotnet run --project Starter.Bff
```

BFF available at `http://localhost:5000`.
