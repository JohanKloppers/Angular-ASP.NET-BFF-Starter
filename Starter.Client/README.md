# Starter.Client

Angular 21 SPA built with TypeScript and Tailwind CSS v4. Communicates exclusively with the BFF — never directly with the API.

## Tech Stack

- Angular 21 (standalone components)
- TypeScript
- Tailwind CSS v4
- nginx (production Docker image)

## Key Architecture

- **No direct API calls** — all HTTP goes through the BFF at the same origin.
- **`credentialsInterceptor`** — adds `withCredentials: true` to every request so the session cookie is sent.
- **`csrfInterceptor`** — reads the `X-CSRF-TOKEN` cookie and adds it as a request header on all mutating requests (POST/PUT/PATCH/DELETE).
- **`errorInterceptor`** — redirects to `/login` on 401 responses.
- **`AuthGuard`** — waits for the auth check to complete before activating protected routes.

## Routes

| Path | Guard | Description |
|---|---|---|
| `/` | — | Redirects to `/dashboard` |
| `/dashboard` | `AuthGuard` | Main dashboard (protected) |
| `/login` | — | Sign in |
| `/register` | — | Create account |
| `/**` | — | Redirects to `/login` |

## Local Development

```bash
npm install
npm start
```

App available at **http://localhost:4200**. API and BFF requests are proxied via `proxy.conf.json`.

The BFF must be running at `http://localhost:5000` for auth and API calls to work.

## Building for Production

```bash
npm run build
```

Output: `dist/starter-client/browser/` — served by the nginx Docker image.

## Adding a New Component

```bash
ng generate component your-component-name
```

Use standalone components (no NgModules). Import only what the component directly uses in its `imports: []` array.

## Proxy Configuration

`proxy.conf.json` maps these paths to the BFF during local development:

| Path | Target |
|---|---|
| `/api` | `http://localhost:5000` |
| `/login` | `http://localhost:5000` |
| `/logout` | `http://localhost:5000` |
| `/register` | `http://localhost:5000` |
| `/user` | `http://localhost:5000` |
| `/health` | `http://localhost:5000` |
