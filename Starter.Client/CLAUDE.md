# Starter.Client — Angular 21 SPA

## What this is
The frontend SPA. Served by Nginx inside Docker on port 4200. Communicates exclusively with the BFF — never directly with the API.

## Tech stack
- Angular 21 standalone components
- Tailwind CSS v4
- **No Zone.js** — zoneless change detection
- No state management library (signals + services)

## Zoneless Angular — the most important rule
This app has **no `zone.js` polyfill** and no `provideZoneChangeDetection()`. Angular's change detection does not run automatically after async operations.

**Rule**: Every value set inside a `.subscribe()` callback that the template reads must be a `signal`. Use `toSignal()` from `@angular/core/rxjs-interop` to bridge Observables into signals.

```ts
// Wrong — template won't update after HTTP response
this.stats = await fetchStats();  // or inside .subscribe(next => this.stats = s)

// Correct
stats = toSignal(this.dashboardService.getStats());
// Template: {{ stats()?.totalUsers }}
```

Error/loading state set in `subscribe` error/complete callbacks also needs signals:
```ts
private readonly _error = signal(false);
readonly error = this._error.asReadonly();

someObs.pipe(catchError(() => { this._error.set(true); return EMPTY; }));
```

## Project structure
```
src/app/
  app.ts                     Root component — just <router-outlet>
  app.config.ts              provideRouter, provideHttpClient with interceptors
  app.routes.ts              Route definitions
  auth.service.ts            Auth state (isAuthenticated, isLoading signals via BehaviorSubject)
  auth.guard.ts              CanActivate — waits for isLoading=false before deciding
  home/                      Dashboard page (Home component)
  header/                    Top navigation bar
  sidebar/                   Left navigation panel
  login/                     Login page
  register/                  Registration page
  dashboard/dashboard.service.ts  Fetches /api/dashboard/stats
  interceptors/
    credentials.interceptor.ts  Adds withCredentials: true to all requests (sends cookies)
    csrf.interceptor.ts         Reads X-CSRF-TOKEN cookie, adds it as header on mutating requests
    error.interceptor.ts        Redirects to /login on 401 (except /login and /user URLs)
```

## Auth flow
1. App starts → `AuthService` constructor → `checkAuthStatus()` sets `isLoading = true` → GET /user
2. `AuthGuard` subscribes to `combineLatest([isAuthenticated$, isLoading$])`, filtered by `!loading`
3. If GET /user returns 200: `isAuthenticated = true`, guard passes, Home renders
4. If GET /user returns 401: `isAuthenticated = false`, guard redirects to `/login`
5. After login/register: `isAuthenticatedSubject.next(true)` in the `tap()`, then `router.navigate(['/dashboard'])`

`isLoadingSubject` starts as `true` (not `false`) so the guard always blocks until the check completes.

## Routes
| Path | Component | Guard |
|---|---|---|
| `/` | — | Redirects to `/dashboard` |
| `/dashboard` | `Home` | `AuthGuard` |
| `/login` | `Login` | — |
| `/register` | `Register` | — |
| `**` | — | Redirects to `/login` |

## HTTP interceptors (applied in order)
1. **credentials** — `withCredentials: true` on every request so cookies are sent
2. **csrf** — for POST/PUT/PATCH/DELETE, reads `X-CSRF-TOKEN` cookie and adds it as a header
3. **error** — on 401 responses, navigates to `/login` unless the URL contains `/login` or `/user`

## Nginx routing (Docker)
Nginx serves the built Angular app. Key routing rules:
- `POST /login`, `POST /register` → proxy to BFF (Angular form submissions)
- `GET /login`, `GET /register` → serve `index.html` (direct browser navigation to these URLs)
- `/logout`, `/user`, `/health`, `/api/*` → always proxy to BFF
- Everything else → `try_files $uri /index.html` (Angular HTML5 routing fallback)

## Development
```bash
npm start        # ng serve with proxy.conf.json (proxies /login, /register, /logout, /user, /api to http://localhost:5000)
npm run build    # Production build into dist/starter-client/
```

## Key packages
| Package | Purpose |
|---|---|
| `@angular/core` v21 | Framework (signals, toSignal, inject) |
| `@angular/router` v21 | Client-side routing |
| `@angular/forms` v21 | Template-driven forms (ngModel) |
| `tailwindcss` v4 | Utility CSS |
| `rxjs` v7.8 | BehaviorSubject-based auth state, toSignal bridge |
