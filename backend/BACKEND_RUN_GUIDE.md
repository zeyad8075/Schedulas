# BACKEND_RUN_GUIDE.md
## Schedulas API — Run & Debug Guide
Grounded in the actual state of the repository as delivered — every command below matches what's really in `backend/src/*.csproj` and `appsettings.json`, not a generic template.

---

## 1. Required Software

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 9.0.x | All four projects target `net9.0` |
| PostgreSQL client tooling | any recent | For inspecting the Supabase Postgres instance directly if needed (`psql`, pgAdmin, etc.) |
| `dotnet-ef` CLI tool | matching EF Core 9 | `dotnet tool install --global dotnet-ef` |
| Docker | latest | **Not yet required to run locally** — no `Dockerfile` exists in this repo yet (see §11 and DEPLOYMENT_CHECKLIST.md). Only needed once you containerize for Render. |
| A Supabase project | — | Auth (JWKS), Postgres, Storage all come from here |
| A Firebase project | — | For push notifications (FCM); optional for initial backend verification — the app will run without it configured, but any code path touching `IPushNotificationService` will fail until it's set |

This has **never been built with `dotnet build` in the environment that produced it** (no NuGet access in that sandbox). Treat your first build as the real first verification step, not a formality.

---

## 2. Solution Setup (one-time)

No `.sln` file exists in the repo yet. Create it:

```bash
cd backend
dotnet new sln -n Schedulas
dotnet sln add src/Schedulas.Domain/Schedulas.Domain.csproj
dotnet sln add src/Schedulas.Application/Schedulas.Application.csproj
dotnet sln add src/Schedulas.Infrastructure/Schedulas.Infrastructure.csproj
dotnet sln add src/Schedulas.API/Schedulas.API.csproj
```

---

## 3. Environment Variables & Secrets

**Never commit real values.** `appsettings.json` ships with every secret-shaped field as an empty string by design. Use `dotnet user-secrets` locally; use your host's environment variable mechanism (Render's dashboard, etc.) in deployment.

```bash
cd src/Schedulas.API
dotnet user-secrets init
```

| Key | Required | Purpose |
|---|---|---|
| `ConnectionStrings:SchedulasDb` | Yes | Postgres connection string (Supabase). Alternatively set the `SCHEDULAS_DB_CONNECTION` env var — the code checks both, in that order. |
| `Supabase:Url` | Yes | e.g. `https://xxxxx.supabase.co` — used for JWKS fetch, Auth REST calls, and Storage REST calls |
| `Supabase:AnonKey` | Yes | The project's public `anon` key |
| `Firebase:ProjectId` | For push notifications | Firebase project id |
| `Firebase:ServiceAccountJsonPath` | For push notifications | Absolute path to a service-account JSON key file with the Firebase Cloud Messaging API scope |
| `Cors:AllowedOrigins` | Yes (array) | Origins allowed to call the API — e.g. your Flutter web build's origin, `localhost` ports for dev |

Set them:

```bash
dotnet user-secrets set "ConnectionStrings:SchedulasDb" "Host=...;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Supabase:Url" "https://<project>.supabase.co"
dotnet user-secrets set "Supabase:AnonKey" "<anon key>"
dotnet user-secrets set "Firebase:ProjectId" "<firebase project id>"
dotnet user-secrets set "Firebase:ServiceAccountJsonPath" "/absolute/path/to/service-account.json"
```

`Cors:AllowedOrigins` isn't secret — set it directly in `appsettings.Development.json` (already has `http://localhost:3000` and `http://localhost:5173` as placeholders; add your Flutter dev origin too if running Flutter web).

**Note on JWT validation:** there is no `Supabase:JwtSecret` setting — JWT validation uses Supabase's public JWKS keys (`SupabaseJwksProvider`, fetched from `{Supabase:Url}/auth/v1/.well-known/jwks.json`), not a shared secret. If you're on an older Supabase project still using HS256 symmetric signing, this will fail closed (every request unauthorized) rather than silently accepting tokens it shouldn't — check your Supabase project's JWT signing algorithm under Project Settings → API if authentication fails universally.

---

## 4. Supabase Configuration Checklist

1. **Create the project** (or use an existing one) at supabase.com.
2. **Auth → URL Configuration**: set your app's redirect URLs (for password reset emails, etc.) once you have a real frontend URL.
3. **Auth → Providers**: Email provider enabled (this project uses email/password only — no OAuth providers configured in the code).
4. **Confirm JWKS is available**: visit `https://<project>.supabase.co/auth/v1/.well-known/jwks.json` in a browser — should return a JSON key set, not a 404. (Very old Supabase projects on legacy JWT settings may not expose this; if so, you'll need to migrate to asymmetric signing keys in Supabase's dashboard.)
5. **Storage**: create the buckets your app will use (e.g. an `institution-logos` bucket) — `SupabaseStorageService` expects the bucket to already exist; it does not create one.
6. **Database**: no schema exists yet — that's §6 below.

---

## 5. Firebase Configuration Checklist

Only needed for push notifications; the API will start and serve most endpoints without it, but anything invoking `IPushNotificationService.SendAsync` will throw.

1. Create/use a Firebase project with Cloud Messaging enabled.
2. Generate a service account key: Firebase Console → Project Settings → Service Accounts → Generate new private key. This downloads a JSON file.
3. Save that file somewhere accessible to the API process and point `Firebase:ServiceAccountJsonPath` at it.
4. Confirm the service account has the "Firebase Cloud Messaging API" scope (default for a newly generated key).

---

## 6. Database Migration Steps

**No migration has ever been generated.** This is the single most important first step — the schema exists only as C# entity configurations until this runs.

```bash
cd src/Schedulas.Infrastructure
dotnet ef migrations add InitialCreate --startup-project . --output-dir Persistence/Migrations
dotnet ef database update --startup-project .
```

`DesignTimeDbContextFactory` reads the connection string from the `SCHEDULAS_DB_CONNECTION` environment variable (not from user-secrets, since `dotnet ef` runs outside the API host's DI container) — set it in your shell first if you haven't already:

```bash
export SCHEDULAS_DB_CONNECTION="Host=...;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

**What to check after migration runs:**
- Every table listed in `03_Database_Design.md` exists, snake_case throughout.
- `profiles.id` has no default/identity generation (it's populated exclusively by the app from `auth.users.id` — confirm via `\d profiles` in `psql` that there's no `DEFAULT` on the `id` column).
- Soft-delete columns (`deleted_at`) exist on every table except the intentionally-immutable `rule_evaluation_logs`.

---

## 7. Build Commands

```bash
dotnet restore
dotnet build
```

Expect this to surface issues — see §12.

---

## 8. Run Commands

```bash
cd src/Schedulas.API
dotnet run
```

Default Kestrel ports apply unless overridden (typically `https://localhost:7xxx` and `http://localhost:5xxx` — check the console output for the exact URLs on first run, or set `ASPNETCORE_URLS`).

**Swagger UI** is available at `/swagger` in the Development environment only (`app.Environment.IsDevelopment()` gate in `Program.cs`).

**Health checks:**
- `GET /health/live` — process is up, no dependency checks
- `GET /health/ready` — includes a real Postgres connectivity check
- `GET /health` — combined view

---

## 9. Smoke-Test Sequence (minimum viable "it's alive" check)

1. `curl https://localhost:<port>/health/live` → expect `Healthy`.
2. `curl https://localhost:<port>/health/ready` → expect `Healthy` (this is the one that actually proves the DB connection string and migration worked).
3. Open `/swagger`, confirm all 14 controllers appear with their endpoints.
4. Create a Supabase user directly in the Supabase dashboard (Authentication → Users → Add User) with a real email, and give it `PlatformAdmin` role/institution via `user_metadata` manually (since public self-registration is intentionally disabled — see §12) — or insert a matching `profiles` row directly via SQL if you'd rather bootstrap that way for testing. This is your first real login.
5. `POST /api/v1/auth/login` with that user's credentials → confirm you get an access token back.
6. `GET /api/v1/auth/me` with that token → confirm the `profiles` row comes back correctly, including `role`.

---

## 10. Render Configuration

**Not yet ready as delivered** — this repo has no `Dockerfile`, despite the Constitution specifying a Dockerized deployment. Before deploying to Render:

1. Write a `Dockerfile` for `Schedulas.API` (multi-stage: SDK image to build/publish, ASP.NET runtime image to run).
2. In Render: create a new Web Service, connect the repo, select Docker as the environment.
3. Set every environment variable from §3 in Render's Environment tab (Render doesn't use `dotnet user-secrets` — plain environment variables, matching the same key names via `:` → `__` convention, e.g. `ConnectionStrings__SchedulasDb`).
4. Set the health check path to `/health/ready`.
5. Confirm `ASPNETCORE_URLS` is set appropriately for Render's expected port binding (Render sets `PORT`; your Dockerfile/entrypoint needs to respect it).

Full pre-deployment checklist: see `DEPLOYMENT_CHECKLIST.md`.

---

## 11. Common Startup Issues

| Symptom | Likely Cause | Fix |
|---|---|---|
| `InvalidOperationException: No database connection string configured` | Missing `ConnectionStrings:SchedulasDb` / `SCHEDULAS_DB_CONNECTION` | Set via user-secrets or env var (§3) |
| App starts, but every request returns 401 | JWKS fetch failing, or wrong `Supabase:Url` | Check `https://<project>.supabase.co/auth/v1/.well-known/jwks.json` resolves; check for typos in the URL |
| `/health/ready` returns Unhealthy but `/health/live` is fine | DB reachable at the network level but migration never ran, or wrong credentials | Run §6; double check the connection string's password/SSL mode |
| Build fails with missing package errors | `dotnet restore` wasn't run, or a package version in a `.csproj` no longer resolves (packages listed in this repo were current as of writing but NuGet versions age) | Run `dotnet restore` first; if a specific version 404s, bump it to the latest patch of the same major/minor |
| Swagger shows no XML doc comments / descriptions | `GenerateDocumentationFile` didn't produce the `.xml` file next to the built `.dll` (e.g. a `Release` vs `Debug` output path mismatch) | Confirm `Schedulas.API.xml` and `Schedulas.Application.xml` exist in the build output directory alongside the `.dll`s |
| `POST /auth/register` returns 401/403 for every caller | This is intentional, not a bug — see below | Register the first user directly via Supabase's dashboard + a manual `profiles` row, or via a `PlatformAdmin` account once one exists |
| FCM push silently does nothing | No `device_tokens` row for the recipient yet (expected until a client registers one), or `Firebase:ServiceAccountJsonPath` misconfigured | Check logs — `FcmPushNotificationService` logs a warning (not an error) when there's no device token, and an error if the Firebase token exchange itself fails |

**Why `/auth/register` rejects everyone at first:** as of the Write-Side Ownership Audit, public self-registration was closed as a critical privilege-escalation vulnerability. Registration now requires an authenticated PlatformAdmin/InstitutionAdmin/DepartmentAdmin caller. There is currently no bootstrapping endpoint for "create the very first PlatformAdmin" — that first account must be created by inserting directly into Supabase Auth + the `profiles` table, or via Supabase's dashboard. This is a known, disclosed gap (see `SECURITY_AUDIT_REPORT.md`), not an oversight in this guide.
