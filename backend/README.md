# Lanka Ornaments – Backend (ASP.NET Core + SQL Server)

.NET 9 · ASP.NET Core Web API · Entity Framework Core 9 (code-first) · ASP.NET Core Identity · JWT · SQL Server

```
src/LankaOrnaments.Core            entities, contracts (DTOs), constants, service interfaces
src/LankaOrnaments.Infrastructure  DbContext, entity configurations, migrations, seeding, services
src/LankaOrnaments.API             controllers, JWT auth, ProblemDetails, OpenAPI/Swagger
tests/LankaOrnaments.Tests         xUnit: SQL Server CRUD/constraint tests + API integration tests
postman/                           end-to-end collection for every endpoint
database/                          idempotent migration SQL script
```

## Database: `LankaOrnamentsDB`

| Table | Purpose |
|---|---|
| `Users` | Identity users. PK column `UserId`, `FullName`, `Email` (unique), `PasswordHash` (Identity PBKDF2), `CreatedAt` |
| `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `UserTokens`, `RoleClaims` | ASP.NET Core Identity |
| `Profiles` | `ProfileId`, `UserId` (unique FK, cascade), `PhoneNumber`, `Address`, `UpdatedAt` |
| `Ornaments` | `OrnamentId`, `Sku` (unique), `Name`, `Description`, `Category`, `Price`, `ImageUrl`, `StockQuantity`, `IsAvailable`, `Metal`, `WeightGrams`, `Gemstone`, `IsFeatured`, `CreatedAt`, `UpdatedAt` |
| `Notifications` | `NotificationId`, `UserId` (nullable, **NULL = shop-wide announcement**), `Title`, `Message`, `Type`, `IsRead`, `CreatedAt` |
| `NotificationReceipts` | Per-customer read/dismissed state for shop-wide announcements (PK `NotificationId`+`UserId`) |
| `NotificationPreferences` | `PreferenceId`, `UserId` (unique FK, cascade), `EmailNotificationsEnabled`, `InAppNotificationsEnabled`, `UpdatedAt` |

Constraints: check constraints on price, stock, weight, category and notification type.
Indexes: on category, name, featured/date, and notifications by user/read/date.
All timestamps are UTC `datetime2(3)`.

## Configuration (no secrets in source control)

| Setting | Where |
|---|---|
| `ConnectionStrings__DefaultConnection` | **Environment variable** (required) |
| `Jwt__SigningKey` (≥ 32 chars) | Environment variable, or user-secrets in Development |
| `Seed__AdminPassword`, `Seed__CustomerPassword` | Environment variable, or user-secrets. If unset, the default accounts are not created |

Local SQL Express with Windows authentication (no password):

```powershell
# Persist for your Windows user (open a new terminal afterwards)
[Environment]::SetEnvironmentVariable('ConnectionStrings__DefaultConnection',
  'Server=ISDU02\SQLEXPRESS;Database=LankaOrnamentsDB;Trusted_Connection=True;TrustServerCertificate=True;', 'User')

# Development secrets (stored in %APPDATA%\Microsoft\UserSecrets, outside the repo)
cd src/LankaOrnaments.API
dotnet user-secrets set "Jwt:SigningKey" "<random string of at least 32 characters>"
dotnet user-secrets set "Seed:AdminPassword" "<admin password>"
dotnet user-secrets set "Seed:CustomerPassword" "<customer password>"
```

With SQL authentication, put the password only in the environment variable / secret store, e.g.
`Server=myserver;Database=LankaOrnamentsDB;User Id=lanka_app;Password=...;TrustServerCertificate=True;`.
Passwords must meet the Identity rules: at least 8 characters, with upper- and lower-case letters and a digit.

## Migrations

```powershell
cd backend
dotnet tool restore                      # installs the local dotnet-ef 9.0.23

# Apply migrations (creates LankaOrnamentsDB if it does not exist)
dotnet ef database update --project src/LankaOrnaments.Infrastructure --startup-project src/LankaOrnaments.API

# Add a migration after changing the model
dotnet ef migrations add <Name> --project src/LankaOrnaments.Infrastructure --startup-project src/LankaOrnaments.API --output-dir Data/Migrations

# Idempotent SQL script (safe to run repeatedly; for DBAs/production)
dotnet ef migrations script --idempotent --project src/LankaOrnaments.Infrastructure --startup-project src/LankaOrnaments.API --output database/migrations-idempotent.sql
```

The API never changes the schema on its own (`Database:ApplyMigrationsOnStartup` is `false`).
If migrations are pending, it logs a warning and skips seeding.
Seeding is insert-only and idempotent:
- roles `Admin` / `Customer`
- the 16 sample ornaments, when the table is empty
- sample notifications, when the table is empty
- the default admin and customer accounts, when their passwords are configured

## Run

```powershell
dotnet run --project src/LankaOrnaments.API --launch-profile http   # http://localhost:5196
```

- Swagger UI (Development only): `http://localhost:5196/swagger`
- Health check: `GET /api/health`
- Frontend: set `NUXT_PUBLIC_USE_MOCK_API=false` and `NUXT_PUBLIC_API_BASE_URL=http://localhost:5196/api`.
  CORS allows `http://localhost:3000` (`Cors:AllowedOrigins`).

## Endpoints

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/login`, `/api/auth/register` (`?useCookies=true` for browsers) | anonymous, rate-limited |
| GET | `/api/auth/session` (200 session / 204 signed out) | anonymous |
| POST | `/api/auth/logout` | anonymous |
| GET/PUT | `/api/profile/me` (alias `/api/users/me`) | signed in |
| POST | `/api/profile/me/change-password` | signed in |
| GET/PUT | `/api/profile/me/notification-preferences` | signed in |
| GET | `/api/ornaments?search=&category=&availability=&sort=`, `/api/ornaments/{id}` | anonymous |
| POST/PUT/DELETE | `/api/ornaments[/{id}]` | Admin |
| GET | `/api/notifications` | signed in |
| PUT | `/api/notifications/{id}/read`, `/api/notifications/read-all` | signed in |
| DELETE | `/api/notifications/{id}` | owner deletes (or hides an announcement); Admin deletes permanently |
| GET | `/api/admin/customers`, `/api/admin/notifications` | Admin |
| POST | `/api/admin/notifications` (`recipient`: customer id or `all`) | Admin |

Errors are RFC 7807 ProblemDetails; validation errors include an `errors` dictionary.

## Security model

- **Browser sessions:** with `?useCookies=true` the JWT is set as an **HttpOnly** cookie (`Auth:CookieName`,
  default `lo_session`), **SameSite=Strict**, **Secure** outside Development. It is never in the response body,
  so the frontend can't store it. Without `useCookies` the token is returned for `Authorization: Bearer` (API clients).
- **CSRF:** POST/PUT/DELETE requests without a bearer header must send `X-Requested-With`. CORS allows that header
  only for `Cors:AllowedOrigins` (with credentials, never `*`), so other sites can't forge requests.
- **Revocation:** each token carries the user's security stamp, which is checked on every request. Changing the
  password or email revokes all other sessions; the current browser gets a renewed cookie.
- **Brute force:** `/api/auth/login` and `/register` are rate-limited per IP (`RateLimiting:AuthPermitsPerMinute`),
  and Identity locks an account for 15 minutes after 5 failed passwords.
- **Authorization:** every permission is enforced here (`[Authorize]`, `[Authorize(Roles = "Admin")]`, ownership checks
  in services). The frontend's route guards are only for navigation.

### Deployment checklist

- Serve the API and frontend over **HTTPS**. Outside Development the API enables HSTS and HTTPS redirection,
  and the cookie is Secure.
- Set `Cors__AllowedOrigins__0=https://www.your-domain.lk` and `NUXT_PUBLIC_API_BASE_URL=https://api.your-domain.lk/api`.
- Same site (e.g. `www.` and `api.` of one domain): keep `Auth__SameSite=Strict`.
  Different sites: use `Auth__SameSite=None` (HTTPS only).
- Optionally use `Auth__CookieName=__Host-lo_session` in production.
- Keep `ConnectionStrings__DefaultConnection`, `Jwt__SigningKey` and `Seed__*Password` in environment variables or a secret store.

## Tests

```powershell
dotnet test                       # uses LankaOrnamentsDB_Tests on .\SQLEXPRESS (override: LANKA_TEST_CONNECTION)

postman collection run postman/LankaOrnaments.API.postman_collection.json `
  --env-var baseUrl=http://localhost:5196 --env-var adminPassword=<...> --env-var customerPassword=<...>
```

- The DbContext tests roll back their transactions.
- The Postman run creates one `api-test-<timestamp>@example.lk` customer and removes everything else it creates.
