# DEPLOYMENT_CHECKLIST.md
## Schedulas — Pre-Production Deployment Checklist

This consolidates every gap already identified across the Phase 8
Verification Report, Security Audit Report, and Production Readiness
Review into one actionable checklist. Nothing here is newly invented —
it's the accumulated "must happen before real users touch this" list
from this project's own history, organized for execution rather than
narrative.

**Do not deploy to production until every item in §1–3 is checked.**
§4 onward are strongly recommended but use your own judgment on timing.

---

## 1. Build & Verification (Critical — blocks everything else)

- [ ] `dotnet build` succeeds with zero errors, backend, all four projects.
- [ ] `flutter analyze` reports zero issues, Flutter app.
- [ ] `dotnet ef migrations add InitialCreate` generated successfully and `dotnet ef database update` ran against a real Supabase Postgres instance with no errors.
- [ ] The full `TESTING_CHECKLIST.md` has been executed manually at least once, end to end, against a real deployed-like environment (not just localhost), with all failures triaged and either fixed or explicitly accepted as known issues.
- [ ] At least minimal automated test coverage exists for the Rule Engine's three outcomes (Approved/Rejected/RequiresOverride) — flagged as the single highest-value test target in the Production Readiness Review, and still true.

## 2. Security (Critical)

- [ ] Re-confirm the Write-Side Ownership Audit's fixes are actually present in the deployed build — spot-check at minimum: `POST /auth/register` rejects unauthenticated calls; a Student cannot fetch another student's calendar; an InstitutionAdmin cannot touch another institution's data.
- [ ] `Supabase:Url` in production config points at your **production** Supabase project, not a dev/staging one.
- [ ] All secrets (`ConnectionStrings:SchedulasDb`, `Supabase:AnonKey`, `Firebase:ServiceAccountJsonPath` contents) are set via your hosting platform's secret/environment mechanism — never committed, never present in any `appsettings.*.json` that reaches source control.
- [ ] Confirm `.gitignore` actually excludes `appsettings.Development.json` if it ever contains real values, and any local `.pfx`/service-account files.
- [ ] HTTPS is enforced end to end (Render terminates TLS; confirm `app.UseHttpsRedirection()` and that no plain-HTTP path is reachable externally).
- [ ] Rate limiting rules (`appsettings.json`'s `IpRateLimiting` section) are reviewed against realistic production traffic expectations — the current limits (10 login attempts/5min, 10 registrations/hour, 120 req/min general) were reasonable defaults, not load-tested numbers.
- [ ] CORS `AllowedOrigins` in production config lists **only** your real production Flutter web / React dashboard origins — remove any `localhost` entries before going live.
- [ ] A second reviewer (human, or a fresh review session with no prior context) re-reads the Activities and Rules authorization code specifically — the Security Audit Report explicitly named this as a residual risk of single-reviewer blind spots, not a closed item.
- [ ] Get a second reviewer to walk through the "first PlatformAdmin bootstrap" process and confirm there's no window where an unintended account gets elevated privileges during that manual step.

## 3. Database Readiness (Critical)

- [ ] Confirm your Supabase project's backup tier and point-in-time-recovery (PITR) window explicitly — this was never configured or verified anywhere in this project; check Supabase's dashboard under Database → Backups.
- [ ] Document the actual Recovery Point Objective (RPO) and Recovery Time Objective (RTO) your Supabase tier provides — don't assume "Supabase probably backs this up" is sufficient documentation.
- [ ] Confirm the migration has been tested as a **repeatable, idempotent** operation (run it against a fresh empty database from scratch once, not just incrementally on a database that's already had manual fixes applied).
- [ ] Verify `profiles.id` truly has no default/auto-generation at the Postgres level (`\d profiles` in `psql`) — this is load-bearing for the entire "auth.users.id is the primary identifier" design; a misconfigured migration here would be a serious, hard-to-detect data integrity bug.

## 4. Deployment Infrastructure

- [ ] Write a `Dockerfile` for `Schedulas.API` — **does not exist yet**, despite being specified in the Constitution as the deployment mechanism. Multi-stage build (SDK → runtime), respecting Render's `PORT` env var.
- [ ] Set up CI (even minimal: build + `flutter analyze`/`dotnet build` on every push) — **does not exist yet**. GitHub Actions or equivalent.
- [ ] Configure Render: Web Service pointing at the Dockerfile, environment variables from §2, health check path set to `/health/ready`.
- [ ] Separate Dev/Staging/Production Supabase projects (Constitution §29 specifies this) — confirm this separation actually exists and isn't just documented as an intention.
- [ ] Confirm the migration pipeline runs against Staging before Production on every deploy, not directly against Production.

## 5. Observability

- [ ] Confirm Render's (or your chosen host's) log retention and searchability actually meets your needs — Serilog currently writes to console only, which relies entirely on the hosting platform's log capture.
- [ ] Wire an uptime monitor against `/health/ready` (not just `/health/live` — liveness alone won't catch a database connectivity problem).
- [ ] Consider, even minimally, exposing request-rate/error-rate metrics before real traffic patterns make debugging production issues harder than it needs to be.

## 6. Firebase Cloud Messaging

- [ ] Production Firebase project configured separately from any dev/test Firebase project.
- [ ] Flutter app's FCM integration actually built and wired (per `FLUTTER_RUN_GUIDE.md` §5 — not yet done as of this checklist's writing; the API-side half works, the client-side token-registration half doesn't exist yet).
- [ ] Test actual push delivery to a real device before relying on it for production notifications — the backend integration has never been exercised against a real Firebase send in this project's history, only reviewed as code.

## 7. Flutter Release Build

- [ ] Platform folders generated (`flutter create`) and committed to source control — currently absent (see `FLUTTER_RUN_GUIDE.md`'s opening warning).
- [ ] Android: signing keystore configured, `minSdk`/`targetSdk` reviewed, app icon and name set (currently using Flutter's generated defaults, not final branding).
- [ ] iOS (if targeting iOS): provisioning profile, bundle identifier, App Store Connect setup — none of this has been touched in this project.
- [ ] `flutterfire configure` run for the production Firebase project, once FCM integration (§6) is built.
- [ ] Confirm `--dart-define` values for the production build point at the production API and Supabase project — a debug build accidentally shipped pointing at a dev backend is an easy, embarrassing mistake to make.

## 8. Content & Localization

- [ ] Every user-facing string reviewed by a native/fluent Arabic speaker for correctness and tone — this project's Arabic strings were written carefully but not reviewed by a human Arabic speaker at any point in its history.
- [ ] Confirm the final color palette and typography (Constitution §21–22 explicitly deferred both to a dedicated UI/UX phase) — the current palette is a reasonable placeholder, not a signed-off brand identity.
- [ ] Numeral display (Western vs. Eastern Arabic-Indic digits) reviewed and made a deliberate decision, not left to whatever `intl`'s default happens to be (flagged in `TESTING_CHECKLIST.md` §12).

## 9. Legal / Operational (outside this project's code, but blocking real launch)

- [ ] Privacy policy and terms of service exist and are linked somewhere in the app — not addressed anywhere in this project.
- [ ] Confirm what, if any, data-residency requirements apply to storing Arabic educational institutions' student data in your chosen Supabase region.
- [ ] Confirm who has production database access and how that access is itself secured/audited — separate from the application-level tenant isolation this project has hardened extensively.

---

## Explicitly Not Blocking (do later, don't gate launch on these)
- PDF report export (disclosed gap since Part 5 of the backend).
- Notification category preferences (disclosed gap since Part 5).
- Institution logo upload via Supabase Storage (the abstraction exists and works; no UI/endpoint consumes it yet).
- Month/week grid Calendar view (current agenda/list view is functional; grid is a presentation upgrade, not a correctness gap).
- Consolidating the three near-duplicate authorization helper classes (`ActivityAuthorization`, `OrgHierarchyAuthorization`, `EnrollmentAuthorization`) — technical debt, not a defect.

---

## Sign-off
This checklist should be attached to whatever change/release process you
use, with each box checked by the person who actually verified it — not
checked off from memory of having built the feature originally.
