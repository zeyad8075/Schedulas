# Schedulas Flutter App — Phase 9, Part 1 (Foundation + Auth vertical slice)

This delivers the app foundation and the first vertical slice: theming
(Material 3, Arabic RTL, light/dark), routing with auth guards, and a
complete Login → Home flow proving the full chain works end to end:
**Supabase Auth → JWT → Schedulas API (`GET /auth/me`) → rendered Profile.**

## What's built
- `core/theme` — Material 3 light/dark themes on a placeholder color
  palette (Constitution §21 explicitly deferred exact hex values to a
  UI/UX design phase; swap `app_colors.dart`'s values, not its structure,
  once that's finalized).
- `core/network` — a single `ApiClient` (Dio) that attaches the current
  Supabase JWT to every request and parses the backend's `ApiResponse<T>`
  envelope exactly, including a session-refresh retry on a stray 401.
- `core/routing` — `go_router` with centralized auth-state redirects.
- `core/localization` — Arabic ARB file (`assets/l10n/app_ar.arb`) as the
  single source of UI strings; Arabic is the only supported locale, not
  one of several.
- `features/auth` — Supabase-direct sign-in and password-reset (not
  proxied through the backend's Auth endpoints, which exist for other
  purposes — see the doc comment in `auth_repository.dart` for why),
  Riverpod state (`authStateProvider`, `currentProfileProvider`), Login
  and Forgot Password screens.
- `features/home` — a minimal landing screen that renders the loaded
  Profile, whose entire purpose is proving the chain above actually works.

## Important: this backend contract just changed
The backend's `POST /auth/register` now requires an **authenticated staff
caller** (Institution/Department Admin or Platform Admin) as of the
Write-Side Ownership Audit — public self-registration was a critical
privilege-escalation vulnerability there and was closed. **This app
intentionally does not include a self-registration screen.** New accounts
are created by staff, not by end users signing themselves up. If a
staff-facing "invite a user" screen is wanted in Flutter (as opposed to
the React admin dashboard, Phase 10), that's a new, explicit feature to
request — not something this slice assumed.

## Why there's no `pubspec.lock`, build output, or verified run
This sandbox's network allowlist does not include `pub.dev`, so
`flutter pub get` cannot fetch any of this project's packages here — the
same limitation that applied to `dotnet restore` throughout the backend.
Every file is written to be correct, but none of it has been mechanically
verified by `flutter analyze`, `flutter pub get`, or a real run. Please
treat a clean `flutter pub get && flutter analyze` as the actual first
milestone, the same standing recommendation given for the backend at every stage.

## To run locally

```bash
cd flutter_app
flutter pub get
flutter gen-l10n   # generates lib/l10n/app_localizations.dart from the ARB file

flutter run \
  --dart-define=SUPABASE_URL=https://<project>.supabase.co \
  --dart-define=SUPABASE_ANON_KEY=<anon key> \
  --dart-define=API_BASE_URL=http://localhost:5000/api/v1
```

Point `API_BASE_URL` at your locally-running Schedulas.API instance (see
the backend's own README for how to run that first — this app has
nothing to talk to until the backend is running and migrated).

## What to check when you run it
1. `flutter analyze` — same expectation as the backend: with this much
   code never having touched a real toolchain, a first-pass issue or two
   wouldn't be surprising.
2. Login with a real Supabase user (created via the backend's now-staff-only
   registration, or directly in the Supabase dashboard for initial testing)
   and confirm you land on the Home screen with your actual profile data —
   this is the real end-to-end proof this slice exists to provide.
3. Confirm RTL layout looks correct (text alignment, icon mirroring,
   the "forgot password" link sitting at the correct edge).
4. Force a 401 (e.g. revoke the session in Supabase's dashboard mid-session)
   and confirm the app redirects to Login rather than showing a raw error.

## Next steps
This is one vertical slice, matching the backend's own incremental
approach. Natural next slices: Institution/Org-Hierarchy views (read-only
first, matching whichever role is signed in), then Calendar (the
emotional center of the product per Constitution §19), then
Activities/Rule Engine interaction. Stopping here for the same kind of
checkpoint used throughout the backend build — confirm this foundation
before more screens get built on top of it.

---

## Part 2 — Activities / Calendar slice + navigation shell

Adds the "emotional center of the product" (Constitution §19):

- `features/activities` — domain model mirroring `ActivityDto`/`ActivityType`/
  `ActivityStatus` exactly; `ActivitiesRepository` calling `GET /activities`
  with the signed-in user's own `institutionId` (never any other — the
  backend rejects a mismatch per the Security Audit's Activities fixes,
  and this app never gives it the chance to by not taking institutionId
  from anywhere but the loaded Profile); Riverpod providers keyed to a
  visible date range; an agenda-style Calendar screen (grouped by day,
  color-coded by activity type, month navigation) plus the shared
  `ActivityCard` widget those color tokens will also serve as reports/
  notifications get built.
- `core/widgets/app_shell.dart` — a bottom-navigation shell built on
  `go_router`'s `StatefulShellRoute`, so Home and Calendar each keep
  their own navigation stack and scroll position when switching tabs
  (not just swapped widgets losing state).
- A generic `PaginatedList<T>` parser in `core/network`, since every list
  endpoint in the API returns this same shape — built once, not
  per-feature.

**Scope note:** this is an agenda (list) view, not yet a month/week grid.
The underlying data flow (institution-scoped, date-ranged, fully honoring
the backend's authorization) is the part that's expensive to get wrong;
a grid view is a presentation layer on top of the same
`calendarActivitiesProvider` and is a natural, lower-risk follow-up.

**A near-miss worth flagging:** `activities_providers.dart` originally
defined its own `DateTimeRange` class — which collides with Flutter's own
built-in `material.dart` class of the exact same name. Caught before it
shipped and renamed to `DateRange`. Mentioning it because it's exactly
the kind of small thing that's invisible until `flutter analyze` (or a
real build) surfaces it — another concrete reason the standing
"run this for real" recommendation isn't boilerplate.

---

## Part 3 — Course correction, Activities write-side, and Notifications

### A course correction, disclosed rather than buried
I started this part by building Institution/Org-Hierarchy management
screens — then caught that this directly contradicts a decision already
locked in during the backend's System Architecture phase: **"Institution/
Department Admins are React-dashboard-only for MVP (no Flutter admin
surface)."** Stopped and built the right thing instead: Activity
management for Teachers (Flutter's actual primary user) and Notifications
(relevant to every Flutter-facing role). Mentioning this because silently
discarding the wrong work and pretending it never happened would be worse
than admitting the wrong turn.

### A real bug caught before it shipped
The backend's Create/Edit Activity endpoints return their RequiresOverride
outcome as HTTP 422 with `success: false` **but a populated, usable
`data` field** — a deliberate API design choice (RequiresOverride is an
expected, actionable outcome, not a plain error). My original `ApiClient`
threw an exception on any `success: false`, which would have silently
discarded that data before `ActivitiesRepository` ever saw it — the
RequiresOverride flow would have looked like a generic error to the user
instead of the specific "needs admin approval" message it's supposed to
show. Fixed by adding `postAllowingFailureData`/`putAllowingFailureData`,
used only where the backend actually has this two-faced response shape.

### Activities (write side)
- `ActivitiesRepository.createActivity` / `editActivity` — mirror
  `CreateActivityCommand`/`EditActivityCommand` exactly, including that
  `EditActivityCommand` no longer accepts an `InstitutionId` field at all
  (removed in your Write-Side Ownership Audit as a false-security field).
- `ActivityFormScreen` — one form for both Create and Edit. Deliberately
  does **not** offer an "override anyway" option on RequiresOverride —
  only Department Admin+ may approve an override per the Constitution's
  Role & Permission Matrix, and that's React-dashboard territory. A
  Teacher sees a clear Arabic explanation and is told to contact their
  admin, full stop.
- Wired: tapping an activity in Calendar → Edit screen.
- **Not wired, disclosed rather than faked:** a "Create new activity" entry
  point. Creating requires knowing which `classId` to create it for, and
  there is currently **no backend endpoint to list "classes I teach."**
  Rather than fake a class picker with mock data, `ActivityFormScreen`'s
  Create mode is fully built and ready — it just has no real navigation
  path to it yet. Closing this needs either a small, narrowly-scoped
  backend addition (a `GET /me/classes`-shaped query) or a future "My
  Classes" browsing screen. Flagging it plainly rather than routing
  around it with placeholder data.

### Notifications
Full read + mark-read/mark-all-read, matching `GET /notifications` and
its two `PUT` endpoints exactly (both of which the Security Audit already
found correctly self-scoped server-side — nothing to hardan on the client
beyond consuming them correctly). `registerDeviceToken` is wired to the
matching API endpoint, ready for real Firebase Cloud Messaging token
registration once that's added to the app — the API-side half already
works; obtaining the actual device token from Firebase is a distinct,
not-yet-built piece.

### Navigation
Notifications joined Home and Calendar as the shell's third tab, with a
live unread-count badge derived from `notificationsProvider` — never
fetched as a separate "just the count" call.

---

## Part 4 — Closing the "classes I teach" gap for real

The backend now has a real `GET /classes/mine` endpoint (Teacher-only,
identity resolved from the JWT, tenant isolation structural not bolted
on — see the backend README's Part 7 for full detail). This part wires
it in and removes every trace of the disclosed gap from Part 3.

- `MyClassesRepository` + `MyClass` domain model — thin, direct mirrors
  of the new endpoint and its DTO.
- `ClassPickerScreen` — the real entry point Part 3 explicitly refused to
  fake. Lists the teacher's actual classes; empty/error states handled
  properly (no silent failure, no fallback to fake data).
- A **Teacher-only FAB** on the Calendar screen, gated on
  `profile.role == UserRole.teacher` (an Institution/Department Admin
  signed into this app — if that ever happens — sees no such button),
  launching the picker.
- Navigation cleaned up to be consistently `go_router`-based end to end
  (`/activities/pick-class` → `/activities/create`), rather than mixing
  raw `Navigator.push` with `go_router` as an earlier draft of this part
  briefly did.

## ⚠️ Important disclosure: I cannot run `flutter pub get` / `flutter analyze` here
Checked directly before writing anything in this part: **the Flutter SDK
itself is not installed in this sandbox** — no `flutter` or `dart` binary
exists at all. This is a harder limitation than the earlier `pub.dev`
network restriction; it means I cannot execute these commands as real
processes, and I have not fabricated their output anywhere in this
project. What I did instead, and what it is **not** a substitute for:

- A full manual re-read of every file touched in this part and the two
  before it, checking for unused imports/variables, type consistency,
  and brace/paren balance across all 32 Dart files.
- This caught two real issues before you'd have seen them: an
  `unused_local_variable` (a `theme` variable orphaned by a refactor) and
  a `dynamic`-typed parameter that should have been the proper
  `ApiEnvelope<ActivitySubmissionResult>` type.
- **It also caught a bug I introduced myself**: an earlier edit in this
  same part accidentally deleted the `_send<T>` method's signature line
  in `api_client.dart` while inserting a new method nearby, leaving
  orphaned parameter declarations that would not have compiled. Found it
  on a full re-read immediately after, before ever presenting this to
  you, and fixed it. Mentioning this not to alarm you but because it's
  the exact kind of mistake a real `flutter analyze` run would catch in
  under a second — and confirming, again, why that real run still needs
  to happen before this code is trusted, regardless of how carefully I
  read it back.

**None of the above is equivalent to actually running the analyzer.**
Please run `flutter pub get && flutter analyze` yourself as the next
real step — I have done everything short of that, not instead of it.

---

## Part 5 — Settings, and closing the Cancel Activity gap

Two additions, both closing gaps rather than opening new speculative
scope:

### Cancel Activity
`ActivitiesRepository.cancelActivity` existed since Part 3 with no UI
entry point — an oversight, not a deliberate deferral like the class
picker was. Added a delete icon to `ActivityFormScreen`'s AppBar (Edit
mode only), with a confirmation dialog before calling it, since
cancellation is irreversible on the backend (soft-delete, but not
something to trigger accidentally from a stray tap).

### Settings
- `SettingsRepository` / `UserSettings` — mirror `GET`/`PUT /settings/user`
  exactly. Like Notifications, this endpoint takes no user-id parameter
  at all; there's nothing for the client to get wrong here even in
  principle.
- `themeModeProvider` now **actually drives `MaterialApp.themeMode`** —
  `main.dart` previously hardcoded `ThemeMode.system`, which quietly
  ignored the `preferredTheme` field the backend has stored per-user
  since Phase 8. That's fixed; light/dark is now a real per-user setting,
  not just whatever the OS happens to be set to (Constitution §19).
- `SettingsScreen` — theme toggle + profile (name/phone) editing, reached
  via a gear icon on the Home AppBar.

## Manual review disclosure (same standing limitation)
No Flutter SDK in this sandbox, same as every part before this one. Full
manual re-read of every file touched this round; brace/paren balance
verified across all 36 files. This is not a substitute for
`flutter pub get && flutter analyze` — please run both.
