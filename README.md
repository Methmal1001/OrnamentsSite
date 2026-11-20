# Lanka Ornaments

A jewellery shop web application for browsing handcrafted Sri Lankan gold ornaments, with customer accounts, notifications and an admin console.

| Folder | Stack |
|---|---|
| [`frontend/`](frontend/README.md) | Nuxt 4 · Vue 3 · Tailwind CSS 4 · TypeScript |
| [`backend/`](backend/README.md) | ASP.NET Core (.NET 9) · EF Core 9 · ASP.NET Core Identity · SQL Server |

## Quick start

Prerequisites: Node.js 22.22+ (or 24.11+), .NET 9 SDK, SQL Server (Express is fine).

```bash
# 1. Database + API  (see backend/README.md for configuration and secrets)
cd backend
dotnet tool restore
dotnet ef database update --project src/LankaOrnaments.Infrastructure --startup-project src/LankaOrnaments.API
dotnet run --project src/LankaOrnaments.API --launch-profile http     # http://localhost:5196

# 2. Frontend (in a second terminal)
cd frontend
npm install
npm run dev                                                          # http://localhost:3000
```

The frontend can also run alone with offline demo data: `NUXT_PUBLIC_USE_MOCK_API=true npm run dev`.

No secrets are stored in this repository. The connection string, JWT signing key and seed-account passwords are supplied through environment variables or .NET user-secrets.
