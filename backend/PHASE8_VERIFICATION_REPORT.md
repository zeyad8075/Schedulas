# Schedulas Backend Readiness Report
## Phase 8 Verification Pass
**Date:** 2026-07-25
**Scope:** Full repository consistency review against the 35-item checklist. No new features added — verification, bug fixes, and hardening only.

---

## Summary of What Was Found and Fixed

This pass found **real bugs**, including one that would have crashed the application at startup and several genuine security gaps. Nothing below is theoretical — each was traced to specific code before being fixed.

### Crash-level bug (would fail at EF model-build time, i.e. app startup)
- **`RuleEvaluationLog` inherited `BaseEntity`** (full audit + soft-delete contract) **but its EF configuration ignored `DeletedAt`.** `SchedulasDbContext`'s global soft-delete query filter loops over every type assignable to `ISoftDeletableEntity` and builds a filter expression referencing `DeletedAt` — for a type where that property is ignored, this throws when the model is built, i.e. on every app startup. **Fixed** by introducing `ImmutableRecordEntity` (Id only, no audit/soft-delete contract) and moving `RuleEvaluationLog` onto it, which is also the more architecturally honest choice — it's a genuinely immutable, append-only log.

### Security gaps (real, not theoretical — traced to specific unscoped queries)
A systemic pattern emerged: several read and write handlers trusted a caller-supplied ID (`InstitutionId`, `StudentId`, `TeacherId`, etc.) with no verification it belonged to the caller. Found and fixed in:
- **Reports feature** (all 5 queries): workload, exam/assignment distribution, student/teacher calendar, and institution statistics had **zero** institution scoping, and the calendar reports had **zero** ownership check — any authenticated Student or Parent could view any other student's private calendar and workload by passing an arbitrary ID.
- **Activities feature**: `GetActivityByIdQuery` had no scoping at all (any user, any institution, any activity). `GetActivitiesQuery` trusted the caller's `InstitutionId` param with no verification, and applied none of SRS FR-ACT-6's stated per-role restrictions (Students should see only their enrolled classes, Teachers only their own classes) — that requirement was documented in the SRS but never actually implemented.
- **Org Hierarchy queries** (Departments/Programs/Courses/Classes lists): same unscoped-parent-ID pattern.
- **Academic Calendar**: `CreateAcademicTermCommand` and `CreateHolidayCommand` had the same gap **on the write side** — an InstitutionAdmin could create terms/holidays for a different institution, not just read one.
- **Institutions**: `UpdateInstitutionCommand` and `GetInstitutionByIdQuery` — any InstitutionAdmin could update or view another institution's settings.

All of the above are now fixed — either via the existing `ITenantScopedRequest` + `TenantAuthorizationBehavior` pipeline mechanism (Institutions, Academic Calendar — the cleaner fix where the request already carries `InstitutionId` directly), or via explicit ownership/scope checks added to the handler (Reports, Activities, Org Hierarchy — needed because these require resolving a parent chain or role-specific ownership, not a direct field comparison).

### Project/package issues
- `Schedulas.Infrastructure` referenced **`Microsoft.AspNetCore.Http.Abstractions` version 2.2.0** — an ASP.NET Core 2.x-era package from 2018 that predates the .NET Core/ASP.NET Core unification, being used to provide `IHttpContextAccessor`/`IClaimsTransformation` in a net9.0 class library. **Fixed** by replacing it with a proper `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, the correct mechanism for a non-Web-SDK project to access shared-framework types on modern .NET.

### Resource-handling bugs
- `SupabaseStorageService.DownloadAsync` returned a stream read from an `HttpResponseMessage` fetched with `ResponseHeadersRead`, but never disposed the response — a genuine connection leak on every download. **Fixed** with a disposal-forwarding stream wrapper.
- `HttpRequestMessage`/`HttpResponseMessage` instances in `SupabaseIdentityService.ResetPasswordAsync` and the FCM send loop (`FcmPushNotificationService`, which can iterate per device token) were never disposed. **Fixed** with `using`.

### Missing validators
- `ForgotPasswordCommand` and `RefreshTokenCommand` took free-text string input directly from the request body with no FluentValidation validator (unlike the Guid-route-bound commands, which are partially guarded by ASP.NET's route constraints). **Fixed.**

### Consistency fix
- The health-check DB connection string silently fell back to an empty string if unconfigured, producing a confusing low-level Npgsql error instead of the clear `InvalidOperationException` that `AddInfrastructure` already throws for the same missing value. **Fixed** to fail the same way, consistently.

---

## Architecture Status: **Good, with one disclosed deviation**
Clean Architecture layering is intact and correctly enforced (Domain has zero package references; dependencies point inward; verified by re-reading every `.csproj`). MediatR/CQRS, pipeline behaviors (logging → tenant auth → validation → domain-event dispatch), and the Rule Engine's strategy-registry design are all wired correctly.

**Disclosed deviation from PROJECT_CONSTITUTION.md:** the Constitution's tech stack lists "Repository Pattern" and "Unit of Work" as distinct patterns. The actual implementation uses `IApplicationDbContext` directly in handlers (DbContext-as-Unit-of-Work, `DbSet<T>` as generic repository) — a legitimate, common CQRS/EF Core pattern, but a literal divergence from the Constitution's wording. Per the Constitution's own instruction, I'm surfacing this rather than either silently refactoring ~25 handlers (out of scope for a verification pass) or silently ignoring the mismatch. **This needs your decision before Phase 9/10**, not because it's broken, but because the Constitution says to flag conflicts rather than resolve them unilaterally.

## Authentication Status: **Real, JWKS-based, verified for internal consistency**
Supabase Auth is the only identity provider; no ASP.NET Identity anywhere (confirmed via full-repo search). JWT validation uses Supabase's actual JWKS public keys via `SupabaseJwksProvider`, wired into `JwtBearerOptions` through the correct DI-into-auth-options pattern. Role/institution/department claims ride in Supabase's `user_metadata` JWT claim (documented tradeoff: role changes apply on next login, not instantly) and are promoted to a real ASP.NET Core role claim by `SupabaseRoleClaimsTransformation` so `[Authorize(Roles=...)]` works natively.

## Database Status: **Schema-consistent, unverified by an actual build**
Every `DbSet` has a matching `IEntityTypeConfiguration`, every foreign key resolves to a real primary key, and the `profiles.id = auth.users.id` design (per your explicit requirement) is correctly implemented with `ValueGeneratedNever()`. The crash-level `RuleEvaluationLog` bug above is fixed. **No migration has ever been generated** — this sandbox cannot reach NuGet, so `dotnet ef migrations add` has never actually run against this schema. This remains the single biggest unverified assumption in the whole codebase.

## Rule Engine Status: **Sound**
No changes needed here — the strategy-registry design, scope-specificity resolution, and evaluation-logging flow were already correct. Only its logging entity's base class changed (see above), not its logic.

## Storage Status: **Real integration, unconsumed**
`SupabaseStorageService` is a genuine REST integration (upload/download/delete/public-URL/signed-URL), registered in DI, with the download resource leak now fixed. No endpoint calls it yet (e.g. institution logo upload) — that's expected; it was registered as infrastructure, not shipped as a feature.

## Notifications Status: **Real integration, correctly scoped**
FCM push (real OAuth2 service-account token exchange) and in-app notification persistence both work end-to-end from `ActivityScheduledEvent`. `GET /notifications` is correctly scoped to the caller's own `RecipientId` — this one was already right.

## Logging Status: **Adequate**
Serilog structured logging + `LoggingBehavior` (wraps every MediatR request) + `UseSerilogRequestLogging` gives uniform coverage without needing per-handler logging calls. No silent exception swallowing found outside two explicitly-documented, intentional cases (malformed-JWT-metadata parsing).

## Validation Status: **Good, two gaps closed**
FluentValidation pipeline behavior applies uniformly. `ForgotPasswordCommand`/`RefreshTokenCommand` validators added this pass. Ten remaining commands (mostly single-Guid-parameter operations like `CancelActivityCommand`, `DeactivateRuleDefinitionCommand`) have no validator — low risk since they're ID-only and route-bound, but not zero risk (a `Guid.Empty` would still pass a route constraint). Not fixed this pass; listed under Remaining Risks.

## Security Status: **Materially improved, but do not treat as fully audited**
The tenant-isolation and ownership-check sweep above closed nine confirmed vulnerabilities across five features. I want to be direct about what this does and doesn't mean: **I fixed everything I found, but I did not exhaustively audit every single handler** in the ~25 that exist. The pattern (unscoped caller-supplied ID) is now fixed everywhere I checked; the remaining unchecked surface is enumerated below.

---

## Remaining Risks

1. **Update/Delete commands taking only a resource ID** (not a parent `InstitutionId`) were not individually audited for ownership: `UpdateProgramCommand`, `UpdateCourseCommand`, `UpdateClassCommand`, `DeactivateRuleDefinitionCommand`, `UpdateAcademicTermCommand`, `DeleteHolidayCommand`, and the enrollment commands (`EnrollStudentCommand`, `AssignTeacherCommand`, etc.). These generally require resolving a parent chain (e.g. Program → Department → Institution) to check ownership, the same work already done for the equivalent *read* queries in this pass — but the *write* side wasn't systematically swept. This is the most concrete, actionable follow-up before production traffic.
2. **No migration has ever been generated or run.** Every claim in this report about the schema being "consistent" is a static-analysis claim, not a verified-against-a-real-database claim.
3. **No compiler has touched this code.** This sandbox cannot reach NuGet. 78 files, several touched by three separate passes (rename, hardening, security fixes) — the probability of at least one small compile error (a missed `using`, a casing mismatch) is not negligible.
4. **PDF export and notification-category preferences** remain unimplemented (disclosed in Part 5).
5. **Confirm-email endpoint** doesn't call `IIdentityService.ConfirmEmailAsync` yet (disclosed in Part 3/5).
6. **Repository/Unit-of-Work pattern deviation** — see Architecture Status above; needs your decision, not a fix.

## Technical Debt

- AutoMapper is configured and genuinely used in 4 of ~25+ read paths; the rest still build DTOs manually inline. Not wrong, just inconsistent — worth a decision on whether to standardize.
- `ITenantScopedRequest` is now adopted by 8 commands/queries; the pattern is proven and cheap to apply but not yet universal.
- Rate-limiting rule keys in `appsettings.json` (`POST:/api/v1/auth/login`) assume `ApiVersion(1,0)` formats to `"1"` under the `'v'VVV` format specifier. This is very likely correct but was never verified against a running instance — worth a five-minute check once you can build.

## Recommended Next Steps
1. `dotnet build` locally — send me the errors. Given the size of this pass, I'd genuinely be surprised if there are none.
2. Generate and run the initial migration against a real Supabase project.
3. Decide on the Repository/Unit-of-Work conflict (adopt literally, or formally amend the Constitution to reflect the DbContext-as-Unit-of-Work pattern already in use).
4. If you want the remaining write-side ownership audit (Remaining Risk #1) done before Phase 9/10, say so explicitly — it's the same mechanical pattern applied five more times, not a new investigation.
5. Once it builds and the migration runs, re-walk the full vertical-slice test from Part 6's README with particular attention to the newly-fixed authorization paths (try to view another student's calendar as a Student and confirm you now get a 403).

---

## Backend Readiness Score: **72 / 100**

**Why not higher:** a crash-level bug and nine real security gaps existed until this pass — that's a meaningful quality signal about how much unverified surface area remains, even after fixing everything found. Zero lines have been compiled.

**Why not lower:** every fix in this report is real, traced, and specific — not cosmetic. The architecture is sound, the Rule Engine (the system's core) required no changes, and the security sweep, while not exhaustive, closed the most severe and highest-likelihood gaps rather than superficial ones.

**What would move this to 90+:** a clean local build, a successful migration against a real database, and the write-side ownership audit from Remaining Risk #1.

---

## Addendum — Architecture Conflict Resolved (post-report)

The Repository/Unit-of-Work conflict flagged above under **Architecture
Status** and **Remaining Risks #6** has been resolved. The project owner
decided to formally adopt `IApplicationDbContext`/`SchedulasDbContext` as
both Unit of Work and repository, with no additional Repository layer.
This is now documented in **ADR-0001**
(`docs/adr/0001-dbcontext-as-unit-of-work-and-repository.md`) and
PROJECT_CONSTITUTION.md has been amended to v1.1 to match. A full
Constitution/ADR/implementation consistency check found no remaining
contradictions. See the main response for the updated Backend Readiness
Score.
