# PROJECT_CONSTITUTION.md
## Schedulas — Academic Activity Management Platform
**Version:** 1.1
**Status:** Ratified — Backend Foundation Complete (amended per ADR-0001)
**Scope:** This document is the single source of truth for the Schedulas project. Every future phase (SRS, Architecture, Database, ER/Use-Case/Sequence Diagrams, API Design, Backend, Flutter App, React Dashboard, Testing, Deployment) must comply with it. If a future request conflicts with any rule here, the conflict must be explained before proceeding, and this document — not the new request — wins unless explicitly amended.

> **Amendment history:** v1.1 — §6 and §7 amended to reflect ADR-0001 (DbContext as Unit of Work and Repository), ratified during the Phase 8 Verification pass. This resolves the conflict flagged in that pass's Backend Readiness Report between this document's original wording ("Repository + Unit of Work pattern") and the actual implementation (`IApplicationDbContext` directly). See `docs/adr/0001-dbcontext-as-unit-of-work-and-repository.md` for full rationale.

> **Note on language:** This constitution itself is written in English because it is an internal engineering reference (naming, folder structure, coding rules). It does not govern end-user-facing content. Every UI surface, message, notification, email, report, dashboard label, and calendar item in the actual product **must** be 100% Arabic (RTL), as mandated below in Sections 19–20. Tell me if you'd instead like this constitution document itself translated to Arabic.

---

## 1. Project Vision
Schedulas becomes the standard rule-based academic scheduling backbone for Arabic-speaking educational institutions of every kind — schools, universities, institutes, academies, and training centers — replacing ad-hoc spreadsheets and manual coordination with a fair, conflict-free, centrally governed academic calendar.

## 2. Project Mission
To give institution administrators a configurable **Rule Engine** — not an AI black box — that enforces academic-load policy automatically, so no student or teacher is ever double-booked or overloaded, and every academic activity (assignment, exam, project, presentation, event) is scheduled transparently and fairly.

## 3. System Scope

**In scope (MVP and near-term):**
- Multi-tenant SaaS: Institution → Department → Program → Course → Class → Student hierarchy
- Six roles: Platform Admin, Institution Admin, Department Admin, Teacher, Student, Parent
- Rule Engine with DB-stored, admin-editable rules (no redeploys to change policy)
- Core modules: Auth, Institutions, Departments, Programs, Courses, Classes, Students, Teachers, Parents, Academic Terms, Assignments, Projects, Presentations, Exams, Calendar, Notifications, Reports, Settings
- Flutter mobile/tablet app (students, teachers, parents) — Material 3, Arabic RTL, light/dark
- React admin dashboard (institution/department admins) — Material UI, RTL
- ASP.NET Core 9 Web API backend, Clean Architecture, CQRS
- Supabase (PostgreSQL, Auth, Storage), Firebase Cloud Messaging, Render hosting

**Explicitly out of scope (unless later amended):**
- AI/ML-based auto-scheduling or prediction — this is a deterministic Business Rules Engine only
- Payment/billing/subscription management (may be a future phase)
- Native iOS/Android code outside Flutter
- LMS features (grading, content delivery, video lectures)

## 4. Functional Requirements (high-level; detailed FRs belong in the Phase 1 SRS)
- FR-1: System supports onboarding multiple independent institutions (tenants) with isolated data.
- FR-2: Institution Admins manage their institution's org hierarchy (departments → programs → courses → classes → students/teachers).
- FR-3: Teachers/Admins can create Assignments, Exams, Projects, and Presentations, each of which must pass through the Rule Engine before being persisted.
- FR-4: The Rule Engine evaluates a configurable, DB-stored rule set (e.g., max assignments/day, max exams/week, min days before exam, no scheduling on holidays, no conflicts, priority resolution) and either approves, rejects, or flags-for-override the activity.
- FR-5: Institution/Department Admins can view, create, edit, enable/disable, and prioritize rules through an admin UI without code changes.
- FR-6: All users see a centralized, color-coded academic calendar (daily/weekly/monthly) filtered to their role and scope.
- FR-7: System sends notifications (push via FCM, and in-app) for new activities, upcoming deadlines, rule violations, and schedule changes.
- FR-8: System generates reports: Workload, Exam Distribution, Assignment Distribution, Student Calendar, Teacher Calendar, Institution Statistics.
- FR-9: Full auth lifecycle: registration, email confirmation, login, forgot/reset password, refresh tokens, role-based authorization.
- FR-10: Soft delete and full audit trail (CreatedAt/By, UpdatedAt/By, DeletedAt) on all domain entities.

## 5. Non-Functional Requirements
- **Performance:** API p95 response time < 300ms for standard CRUD; Rule Engine evaluation < 500ms per activity submission.
- **Scalability:** Stateless API layer, horizontally scalable on Render; DB schema designed for tenant growth without redesign.
- **Availability:** Target 99.5% uptime for MVP.
- **Security:** JWT + refresh tokens, HTTPS-only, role-based authorization on every endpoint, rate limiting, input validation on every boundary.
- **Localization:** 100% Arabic RTL for all user-facing surfaces across Flutter app, React dashboard, notifications, emails, and reports; English reserved strictly for code-level artifacts.
- **Maintainability:** SOLID, DRY, KISS, Clean Architecture layering strictly enforced; no cross-layer leakage.
- **Auditability:** Every write operation traceable to a user and timestamp; nothing is hard-deleted.
- **Portability:** Backend containerizable and deployable to Render without code changes; DB is Supabase-hosted PostgreSQL accessed only via EF Core migrations (no manual schema drift).

## 6. Architecture Principles
- Clean Architecture with strict dependency inversion: **Domain** has zero external dependencies; **Application** depends only on Domain; **Infrastructure** implements Application interfaces; **Presentation** (API) depends only on Application.
- CQRS: all writes are Commands, all reads are Queries, handled via MediatR-style handlers (or equivalent) — no fat controllers, no business logic in controllers.
- `IApplicationDbContext` (Application-layer interface, implemented by `SchedulasDbContext` in Infrastructure) serves as both Unit of Work and generic repository — see **ADR-0001** (`docs/adr/0001-dbcontext-as-unit-of-work-and-repository.md`) for the full rationale. No separate Generic Repository or per-entity repository classes exist or should be added; `DbSet<T>` plus LINQ *is* the repository abstraction for this project. This is a deliberate, documented exception to strict persistence-ignorance: the Application layer references `Microsoft.EntityFrameworkCore` only for the `DbSet<T>` type exposed through `IApplicationDbContext`'s own surface — it never references `SchedulasDbContext`, EF Core's `DbContext` base class, migrations, or any Infrastructure-layer persistence code directly. ADR-0001 Consequences section explains why this specific, narrow coupling is accepted.
- Every cross-cutting concern (validation, logging, exception handling) is implemented as a pipeline behavior/middleware — not scattered inline.
- The Rule Engine is a first-class Domain/Application service, not a hardcoded `if` chain — rules are data, evaluated by an interpreter.

## 7. Clean Architecture Guidelines
Layers, inner to outer:
1. **Domain** — Entities, Value Objects, Domain Events, Enums, Domain Exceptions, Rule Engine contracts. No references to any other layer or external package (beyond base .NET).
2. **Application** — CQRS Commands/Queries + Handlers, DTOs, Validators (FluentValidation), AutoMapper profiles, `IApplicationDbContext` (the Unit-of-Work/repository abstraction, per ADR-0001) and other service interfaces (implemented in Infrastructure). References Domain only.
3. **Infrastructure** — `SchedulasDbContext : IApplicationDbContext` (EF Core, doubling as Unit of Work and repository per ADR-0001 — no additional Repository layer), Supabase Auth/Storage integration, FCM integration, external service clients. References Application (to implement its interfaces) and Domain.
4. **Presentation (API)** — Controllers (thin, delegate to MediatR), Middleware, Swagger config, DI composition root (`Program.cs`). References Application and Infrastructure only for wiring.

Rule: **dependencies point inward only.** Domain never knows Infrastructure or Presentation exist.

## 8. Folder Structure
```
Schedulas/
├── src/
│   ├── Schedulas.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   ├── Events/
│   │   ├── Exceptions/
│   │   └── RuleEngine/
│   ├── Schedulas.Application/
│   │   ├── Common/ (Behaviors, Interfaces, Models)
│   │   ├── Features/
│   │   │   ├── Institutions/ (Commands, Queries, Validators, DTOs, Mappings)
│   │   │   ├── Assignments/
│   │   │   ├── Exams/
│   │   │   ├── RuleEngine/
│   │   │   └── ... (one folder per module)
│   ├── Schedulas.Infrastructure/
│   │   ├── Persistence/ (DbContext, Configurations, Migrations, Repositories, UnitOfWork)
│   │   ├── Identity/ (Supabase Auth integration)
│   │   ├── Storage/ (Supabase Storage)
│   │   ├── Notifications/ (FCM)
│   │   └── Logging/ (Serilog setup)
│   └── Schedulas.API/
│       ├── Controllers/
│       ├── Middleware/
│       ├── Extensions/ (DI registration)
│       └── Program.cs
├── tests/
│   ├── Schedulas.Domain.Tests/
│   ├── Schedulas.Application.Tests/
│   └── Schedulas.API.IntegrationTests/
├── flutter_app/
│   └── lib/
│       ├── core/ (theme, localization, network, di)
│       ├── features/ (one folder per module, feature-first)
│       └── shared/ (widgets, utils)
└── admin_dashboard/
    └── src/
        ├── app/
        ├── features/
        ├── shared/
        └── theme/
```

## 9. Naming Conventions
- **C#:** PascalCase for classes/methods/properties, camelCase for locals/params, `I` prefix for interfaces (`IRuleEngine`), suffix Commands/Queries with intent (`CreateAssignmentCommand`, `GetTeacherCalendarQuery`), Handlers suffixed `Handler`.
- **Dart/Flutter:** lowerCamelCase for variables/functions, UpperCamelCase for classes/widgets, `snake_case` for file names, feature-first folder naming.
- **React/TypeScript:** PascalCase for components, camelCase for functions/variables, hooks prefixed `use`.
- **Routes/Namespaces:** all English, kebab-case for URL segments (`/api/v1/academic-terms`), PascalCase for C# namespaces mirroring folder structure.

## 10. Database Naming Standards
- Tables: `snake_case`, plural (`institutions`, `academic_terms`, `rule_definitions`).
- Columns: `snake_case` (`created_at`, `updated_by`, `deleted_at`).
- Primary keys: `id` (UUID, generated via `gen_random_uuid()` or app-side).
- Foreign keys: `<singular_table>_id` (`institution_id`, `department_id`).
- Junction tables: `<table_a>_<table_b>` alphabetically ordered.
- Indexes: `ix_<table>_<column(s)>`. Unique constraints: `uq_<table>_<column(s)>`.
- Soft delete column: `deleted_at TIMESTAMPTZ NULL` — a row is "deleted" iff this is non-null; all queries filter it via EF Core global query filters.

## 11. API Standards
- Versioned routes: `/api/v1/...`.
- RESTful nouns for resources; verbs only for non-CRUD actions (`/api/v1/assignments/{id}/submit-for-review`).
- Every write endpoint requires JWT + role authorization attribute.
- Pagination via `pageNumber`/`pageSize` query params on all list endpoints; response includes total count.
- All endpoints documented in Swagger/OpenAPI with example payloads.

## 12. API Response Format
Consistent envelope for every response:
```json
{
  "success": true,
  "data": { },
  "message": "تمت العملية بنجاح",
  "errors": null,
  "meta": { "timestamp": "2026-07-24T10:00:00Z", "traceId": "..." }
}
```
On failure, `success: false`, `data: null`, `errors` populated with field-level detail, `message` in Arabic (as this is user-facing text).

## 13. Error Handling Standards
- Global exception-handling middleware maps exceptions to standardized error responses (Section 12).
- Domain exceptions (e.g., `RuleViolationException`) map to `422 Unprocessable Entity`.
- Validation failures map to `400 Bad Request` with per-field errors.
- Auth failures map to `401`/`403`.
- Unhandled exceptions map to `500`, logged with full context via Serilog, never leak stack traces to the client.
- All error messages shown to end users are Arabic; internal logs remain in English for engineering clarity.

## 14. Validation Standards
- FluentValidation validators per Command/Query, run automatically via a MediatR pipeline behavior before the handler executes.
- Validation messages are authored in Arabic (user-facing) with English validator/class names.
- Domain invariants enforced in entities themselves as a second line of defense (never trust Application-layer validation alone).

## 15. Logging Standards
- Serilog structured logging throughout; correlation/trace ID attached to every request.
- Log levels: `Information` for business events (activity created, rule evaluated), `Warning` for rule violations/retries, `Error` for exceptions, `Debug` for local dev only.
- No PII (student/parent personal data) logged at Information level or above without redaction.
- Logs sinks: console (Render) + structured file/external sink for production retention.

## 16. Security Standards
- JWT access tokens (short-lived) + refresh tokens (rotated, stored securely), issued/validated against Supabase Auth.
- HTTPS enforced everywhere; HSTS enabled.
- Role-based authorization enforced at the endpoint level, and additionally at the query level (tenant isolation — an Institution Admin can never read another institution's data).
- Rate limiting on public/auth endpoints to mitigate brute force.
- All secrets (connection strings, API keys) via environment variables / secret manager — never committed to source control.

## 17. Role & Permission Matrix (summary — full matrix belongs in SRS)

| Capability | Platform Admin | Institution Admin | Department Admin | Teacher | Student | Parent |
|---|---|---|---|---|---|---|
| Manage institutions | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Manage departments/programs | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Manage courses/classes | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ |
| Create assignments/exams | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| Edit Rule Engine rules | ❌ | ✅ | ✅ (scoped) | ❌ | ❌ | ❌ |
| View own calendar | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ (child's) |
| View institution reports | ❌ | ✅ | ✅ (scoped) | ❌ | ❌ | ❌ |
| Receive notifications | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |

## 18. Rule Engine Principles
- Rules are **data, not code** — stored in a `rule_definitions` table with fields for rule type, scope (institution/department/course/class), parameters (JSON), priority, and active status.
- Every activity-creation request (Assignment/Exam/Project/Presentation) is piped through `IRuleEngine.EvaluateAsync(activity)` before persistence.
- The engine returns a structured result: `Approved`, `Rejected` (with Arabic reason), or `RequiresOverride` (for admins with elevated permission to force-save with justification, logged in the audit trail).
- Rule types at launch: max-activities-per-day, max-exams-per-week, min-days-before-exam, no-activity-on-holiday, conflict-detection, priority-based-resolution. New rule types are added as strategy implementations registered in a rule-type registry — never as ad-hoc conditionals in handlers.
- Rule changes made by admins take effect immediately for new activities; they never retroactively invalidate already-approved activities (an explicit re-validation action is a separate, deliberate operation).

## 19. UI/UX Design System
- Material Design 3 (Flutter) and Material UI (React), both configured for RTL as the primary and only layout direction.
- Consistent design tokens (spacing scale, elevation, radius) shared conceptually between Flutter and React so the two clients feel like one product.
- Light and Dark themes required on the Flutter app; React dashboard at minimum supports Light, Dark optional/future.
- Calendar is the emotional center of the product — color coding must be immediately scannable: distinct colors per activity type (Assignment, Project, Exam, Event), consistent across calendar, notifications, and reports.

## 20. Arabic RTL Standards
- All layouts mirror correctly for RTL (padding/margin direction, icon direction for back/forward/chevrons, text alignment).
- All user-facing strings (UI, buttons, menus, validation messages, notifications, emails, reports, dashboard, calendar) are Arabic — no exceptions, no mixed-language labels.
- Numbers, dates, and calendar labels use Arabic-appropriate formatting conventions consistent with the institution's locale settings (Gregorian calendar assumed unless SRS specifies Hijri support).
- Source code (variable/class/route/namespace/db object names) remains English per the original mandate — only what a human user reads on screen is Arabic.
- All Arabic copy is centralized in localization resource files (never hardcoded inline) so wording stays consistent and editable without redeploying logic.

## 21. Color Palette (placeholder — to be finalized with UI/UX phase, not implementation)
- Primary: institutional trust-blue tone
- Secondary/Accent: warm accent for calls-to-action
- Semantic: distinct, colorblind-considerate colors for Assignment / Project / Exam / Event, and separate semantic colors for Success / Warning / Error / Info states
- Exact hex values to be defined and locked during the UI/UX design phase, then never altered ad hoc afterward — changes go through this constitution.

## 22. Typography
- Arabic-first font family with excellent RTL glyph support and good legibility at small sizes (calendar cells, notification chips) — exact family to be selected in the UI/UX phase and locked here.
- Consistent type scale (display/headline/title/body/label) shared conceptually across Flutter and React.

## 23. Icon Guidelines
- Use a single consistent icon set across Flutter and React (e.g., Material Symbols) to avoid visual inconsistency between clients.
- Directional icons (arrows, chevrons) must be mirrored for RTL automatically via the framework's RTL support — never hardcoded LTR icons.

## 24. Git Branch Strategy
- `main` — always production-deployable.
- `develop` — integration branch.
- `feature/<module>-<short-description>` — e.g., `feature/rule-engine-max-exams`.
- `fix/<short-description>` for bug fixes.
- `release/<version>` for release stabilization.
- No direct commits to `main` or `develop` — all changes via Pull Request with at least one review.

## 25. Commit Message Convention
Conventional Commits format:
```
<type>(<scope>): <short description>

[optional body]
```
Types: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`, `perf`, `style`. Example: `feat(rule-engine): add max-exams-per-week rule type`.

## 26. Coding Standards
- Follow Microsoft C# Coding Guidelines for backend; `dotnet format` / analyzers enforced in CI.
- Follow official Dart/Flutter style guide (`dart format`, `flutter analyze` clean before merge).
- Follow standard React/TypeScript conventions with ESLint + Prettier enforced in CI.
- No magic strings/numbers — constants or enums. No commented-out dead code merged. No placeholder/fake implementations merged, per the original mandate.

## 27. Documentation Standards
- Every public API endpoint documented in Swagger with example request/response.
- Every module has a short `README.md` describing its responsibility and key flows.
- Architecture Decision Records (ADRs) for any significant deviation or addition to this constitution — stored under `docs/adr/`.

## 28. Testing Standards
- Domain and Application layers: unit tests (xUnit) with meaningful coverage of Rule Engine logic in particular — this is the system's core, so its rules deserve the heaviest test investment.
- API layer: integration tests against a test database (e.g., Testcontainers for Postgres) covering auth, tenant isolation, and rule enforcement end-to-end.
- Flutter: widget tests for core screens, unit tests for business logic (state management layer).
- React: component tests (React Testing Library) for critical admin flows (rule editing, activity approval).
- No feature is "done" without corresponding tests, per Phase 11 of the workflow.

## 29. Deployment Standards
- Backend: containerized (Docker) deployment to Render, environment-specific configuration via environment variables, health-check endpoint required for Render's monitoring.
- Database: Supabase-managed Postgres; all schema changes via EF Core migrations only, applied through a controlled CI/CD step — never manual `ALTER TABLE` in production.
- CI/CD: build → lint/analyze → test → migrate (staging) → deploy, gated by passing tests.
- Separate environments: Development, Staging, Production, each with isolated Supabase project/database.

## 30. Future Scalability Guidelines
- Multi-tenancy designed from day one (shared schema + `institution_id` discriminator, not per-tenant databases) to keep operational overhead low while institution count grows.
- Rule Engine's strategy-registry design allows new rule types to be added without touching existing rules or requiring migrations to logic (only data).
- Reporting module designed to be extended with new report types without restructuring the underlying event/audit data model.
- Architecture leaves room for a future billing/subscription module and future AI-assisted *suggestions* (explicitly layered on top of, never replacing, the deterministic Rule Engine) without violating Clean Architecture boundaries.

---

## Governance
This document must remain consistent throughout the project. No phase (SRS, Architecture, Database Design, ER Diagram, Use Case Diagram, Sequence Diagrams, API Design, Backend, Flutter App, React Dashboard, Testing, Deployment) may contradict it. If a future request conflicts with a rule defined here, the conflict must be surfaced and explained before any change is made — and any accepted change must be reflected back into this document so it stays the single source of truth.

**This is the end of the constitution. Per the defined workflow, the next step is Phase 1 — Software Requirements Specification (SRS) — which will not begin until this document is reviewed and approved.**
