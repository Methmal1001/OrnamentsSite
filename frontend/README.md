# Lanka Ornaments – Frontend

Nuxt 4 · Vue 3 · Tailwind CSS 4 · TypeScript. A client-rendered SPA backed by the ASP.NET Core API in `../backend`.

## Run

```bash
npm install
npm run dev        # http://localhost:3000 (start the API first, see ../backend/README.md)
npm run typecheck
npm run build      # production build -> .output/
npm run preview
```

## Configuration (runtime, via environment variables)

| Variable | Default | Purpose |
|---|---|---|
| `NUXT_PUBLIC_API_BASE_URL` | `http://localhost:5196/api` | API base URL. **Must be `https://` when deployed.** |
| `NUXT_PUBLIC_USE_MOCK_API` | `false` | `true` = offline demo with local dummy data (no backend needed) |
| `NUXT_PUBLIC_SHOW_DEMO_ACCOUNTS` | `false` | Shows test-account shortcuts on the login page. Development only. |

The frontend holds no secrets. All values above are public.

## How it talks to the API

- `app/composables/useApi.ts` is the single HTTP client. It sends `credentials: 'include'` and the `X-Requested-With` anti-CSRF header.
  Every failure becomes an `ApiError` with a friendly message, field errors (`fieldErrors.email` …) and a `kind`:
  `http`, `network` or `timeout`.
- `app/services/*Service.ts` each have an API implementation and a mock implementation; `useServices()` picks one.

### Authentication

- **The cookie:** login and register call `/api/auth/*?useCookies=true`. The API sets the JWT in an HttpOnly,
  SameSite=Strict cookie (Secure in production). JavaScript never sees or stores the token; the app keeps only
  `{ user, expiresAt }` in memory.
- **Startup:** `app/plugins/auth.ts` restores the session from `GET /api/auth/session` before the first navigation.
  It also signs the user out when `expiresAt` passes.
- **Expired or revoked sessions:** any `401` from an authenticated call (expired token, or a password change on
  another device) clears the session. The user sees "Your session has expired" and is sent to `/login?redirect=…`.
- **Route guards:** `middleware/auth.ts` (customer pages), `admin.ts` and `guest.ts` only decide navigation.
  **The API enforces every permission**; the frontend's checks are not trusted.

## Pages and endpoints

| Page | API |
|---|---|
| Login / Register | `POST /auth/login`, `POST /auth/register`, `POST /auth/logout`, `GET /auth/session` |
| Profile | `GET`/`PUT /profile/me`, `POST /profile/me/change-password` |
| Ornaments (search, category, availability, sort) | `GET /ornaments?search=&category=&availability=&sort=`, `GET /ornaments/{id}` |
| Notifications | `GET /notifications`, `PUT /notifications/{id}/read`, `PUT /notifications/read-all`, `DELETE /notifications/{id}`, `GET`/`PUT /profile/me/notification-preferences` |
| Admin | `POST`/`PUT`/`DELETE /ornaments`, `GET /admin/customers`, `GET`/`POST /admin/notifications` |

Backend validation messages appear next to the matching form fields; conflicts (duplicate email or SKU) are shown on the field too.
