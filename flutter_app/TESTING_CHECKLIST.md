# TESTING_CHECKLIST.md
## Schedulas — Manual Verification Checklist

Prerequisites: backend running and migrated (`BACKEND_RUN_GUIDE.md`),
Flutter app running against it (`FLUTTER_RUN_GUIDE.md`), at least one
`PlatformAdmin` account bootstrapped manually (see
`BACKEND_RUN_GUIDE.md` §9/§11).

Every item below is a **manual, human verification step** — none of this
has been executed automatically, since no automated tests exist yet for
either project (disclosed in both run guides). Check items off as you
personally confirm them, not as a proxy for "the code looks like it
should do this."

---

## 1. Authentication

- [ ] `POST /api/v1/auth/login` with valid credentials returns an access token and refresh token.
- [ ] `POST /api/v1/auth/login` with wrong password returns 401 with an Arabic error message, not an English/generic one.
- [ ] `GET /api/v1/auth/me` with a valid token returns the correct `profiles` row (id matches Supabase `auth.users.id` exactly — check both directly in the database).
- [ ] `GET /api/v1/auth/me` with no token / expired token returns 401.
- [ ] Flutter: login screen shows an Arabic validation error for an empty/malformed email before any network call fires.
- [ ] Flutter: successful login navigates to Home automatically (via the auth-state-driven router redirect, not a manual navigation call).
- [ ] Flutter: force-expire a session (revoke it in Supabase's dashboard) mid-use, then trigger any API call — confirm the app redirects to Login rather than showing a raw error or hanging.
- [ ] Forgot-password flow: request a reset, confirm Supabase sends the email, confirm the UI always shows success regardless of whether the email exists (this is deliberate — the screen never reveals whether an email is registered).

## 2. Registration

- [ ] Confirm `POST /api/v1/auth/register` **rejects an unauthenticated request** with 401 — this is intentional (see Security Audit Report), not a bug to "fix."
- [ ] As a `PlatformAdmin`, register a new `InstitutionAdmin` for a specific institution — confirm success and that the new `profiles` row has the correct `institution_id`.
- [ ] As that `InstitutionAdmin`, attempt to register another user with `role: PlatformAdmin` — confirm this is **rejected** (role-hierarchy check; an admin cannot grant a role at or above their own level).
- [ ] As an `InstitutionAdmin`, attempt to register a user into a **different** institution's `institutionId` — confirm this is **rejected** (tenant-scope check).
- [ ] As a `DepartmentAdmin`, attempt to register a `Teacher` — confirm success. Attempt to register another `DepartmentAdmin` — confirm rejected.
- [ ] Attempt registration with a `departmentId` that doesn't actually belong to the given `institutionId` — confirm rejected (data-integrity check).

## 3. Role Authorization

For each role (`PlatformAdmin`, `InstitutionAdmin`, `DepartmentAdmin`, `Teacher`, `Student`, `Parent`):
- [ ] Confirm the role can access every endpoint the Role & Permission Matrix (Constitution §17) says it should.
- [ ] Confirm the role is **rejected (403)** from at least one endpoint it should *not* have access to — don't only test the happy path.
- [ ] Specifically re-verify the Write-Side Ownership Audit's headline fixes: a `Student` cannot view another student's calendar/workload; a `Teacher` cannot edit/cancel an activity for a class they don't teach; an `InstitutionAdmin` cannot touch another institution's data via any endpoint.

## 4. Institution Creation

- [ ] As `PlatformAdmin`, `POST /api/v1/institutions` — confirm creation succeeds and appears in `GET /api/v1/institutions`.
- [ ] As `InstitutionAdmin`, attempt `POST /api/v1/institutions` — confirm 403 (PlatformAdmin-only).
- [ ] As `InstitutionAdmin`, `PUT /api/v1/institutions/{their own id}` — confirm success.
- [ ] As `InstitutionAdmin`, `PUT /api/v1/institutions/{a different institution's id}` — confirm rejected.
- [ ] `POST /api/v1/institutions/{id}/suspend` then confirm a user of that institution can no longer meaningfully operate (check what your build actually enforces here — suspension state exists on the entity; confirm what, if anything, currently gates on `IsSuspended` at the API layer, since this wasn't explicitly wired into every request path).

## 5. Organization Hierarchy

- [ ] Create a Department, Program, Course, Class in sequence, each nested under the previous — confirm each creation enforces the tenant/scope checks from the Write-Side Ownership Audit (attempt each as a caller from a different institution first, confirm rejection, then as the correct institution's admin, confirm success).
- [ ] Enroll a Student into a Class — confirm rejected if the student belongs to a different institution than the class.
- [ ] Assign a Teacher to a Class — same cross-institution rejection check.
- [ ] Link a Parent to a Student — confirm rejected if the student belongs to a different institution than the caller's own.
- [ ] `GET /api/v1/classes/mine` as the assigned Teacher — confirm it returns exactly the classes they teach, only active ones, and not classes they don't teach.
- [ ] `GET /api/v1/classes/mine` as a non-Teacher role — confirm 403.

## 6. Rule Engine

- [ ] Create a `MaxActivitiesPerDay` rule with `max: 1` for a Class. Create one activity that day — succeeds. Attempt a second — confirm `422` with the correct Arabic rejection message, and confirm a `rule_evaluation_logs` row was written either way.
- [ ] Create a `NoActivityOnHoliday` rule + a Holiday on a specific date. Attempt to schedule an activity on that date — confirm rejection.
- [ ] Create a `MinDaysBeforeExam` rule. Schedule two exams closer together than the minimum — confirm the second is rejected.
- [ ] Trigger a `RequiresOverride` outcome (depends on your rule configuration — e.g. a rule configured to flag rather than hard-reject). Confirm: (a) a Teacher sees the "needs admin approval" message and cannot proceed further in Flutter; (b) `POST /api/v1/activities/override` as a `DepartmentAdmin`+ succeeds with a justification note and the resulting activity's status is `RequiresOverrideApproved`.
- [ ] Edit an activity's date — confirm the Rule Engine re-evaluates. Edit only its title — confirm it does **not** re-evaluate (per SRS FR-ACT-5).
- [ ] Edit or deactivate a Rule — confirm previously-approved activities are **not** retroactively invalidated (Constitution §18).
- [ ] Attempt to edit/deactivate a rule belonging to a different institution — confirm rejected.

## 7. Calendar

- [ ] Flutter Calendar screen loads the current month's activities, grouped by day, color-coded by type matching `AppColors.activityColor`.
- [ ] Navigating to a previous/next month re-fetches correctly (confirm via network inspection that `dateFrom`/`dateTo` actually change).
- [ ] Empty month shows the empty state, not a blank screen or spinner stuck forever.
- [ ] Pull-to-refresh actually re-fetches (not just replays cached data).
- [ ] Cross-check: an activity visible to a Teacher in Calendar is the correct scoped subset per `ActivityAuthorization.ResolveVisibleClassIdsAsync` — a Teacher should not see another teacher's unrelated classes' activities.

## 8. Activities

- [ ] Teacher: FAB → Class Picker → select a class → Create Activity form → submit → confirm it appears in Calendar.
- [ ] Edit an existing activity (tap from Calendar) → change fields → save → confirm changes persist.
- [ ] Cancel an activity (delete icon in Edit mode) → confirm dialog appears → confirm → confirm it disappears from (or shows struck-through/cancelled in) Calendar.
- [ ] Attempt to edit an activity that isn't yours (e.g. a different teacher's, via direct API call with a captured ID) — confirm 403.
- [ ] Class Picker shows only active classes the signed-in Teacher actually teaches — confirm against the database directly, not just visually.

## 9. Notifications

- [ ] Create/approve an activity as a Teacher — confirm enrolled students, their linked parents, and (if relevant) other assigned teachers each receive a `notifications` row and, if a device token is registered, a push (check FCM delivery logs / the API's logs for the send attempt).
- [ ] Flutter Notifications tab shows the list, unread ones visually distinct, badge count on the bottom nav matches the actual unread count.
- [ ] Tapping an unread notification marks it read (both in the UI and confirmed via a subsequent `GET /notifications?isRead=true`).
- [ ] "Mark all read" clears the badge.
- [ ] Confirm a user only ever sees their **own** notifications — attempt `GET /notifications` as two different users, confirm no overlap beyond what's actually shared.

## 10. Settings

- [ ] `GET /api/v1/settings/user` returns the signed-in user's own data with no id parameter involved anywhere in the request.
- [ ] Update full name + phone number in Flutter Settings → save → confirm persisted (reload the app, confirm it's still there).
- [ ] `GET /api/v1/settings/institution` as `InstitutionAdmin` returns their own institution; as any other role, confirm 403.

## 11. Theme Switching

- [ ] Change theme to Dark in Settings → confirm the entire app (not just the Settings screen) switches immediately.
- [ ] Restart the app (kill and relaunch, not hot reload) → confirm the previously saved theme persists (proves it's actually being read from `GET /settings/user` on startup, not just held in memory).
- [ ] Confirm both Light and Dark themes render all screens without unreadable contrast (Material 3 `ColorScheme.fromSeed` should handle this, but verify visually — placeholder palette per Constitution §21, not final).

## 12. Arabic RTL

- [ ] Every screen's text aligns right-to-left correctly, including form labels, error messages, and list items.
- [ ] Icons that imply direction (back arrows, chevrons in navigation) are mirrored correctly, not showing an LTR arrow in an RTL layout.
- [ ] The "forgot password" link, AppBar action icons, and bottom nav all sit on the RTL-correct edge (test against the app, don't assume from code review — `Directionality` wrapping can have edge cases with specific widgets).
- [ ] Long Arabic text (e.g. a lengthy activity description) wraps correctly without overflow or truncation artifacts.
- [ ] Numbers (dates, counts, badge numbers) display in a form consistent with the rest of the Arabic UI — confirm this matches your actual locale expectations (Western Arabic numerals vs. Eastern Arabic-Indic numerals is a real product decision that hasn't been explicitly made in this codebase; Flutter/Dart's default `intl` formatting for `ar` locale may pick one or the other depending on version — verify it matches what you actually want).

## 13. Offline Behavior

**Scope note:** this app has no offline-first architecture, no local cache, and no request queue — this was never built, and is not a regression to "fix" during testing. What to actually verify is that the *absence* of connectivity degrades gracefully rather than crashing:
- [ ] Disable network on the test device entirely, then open the app — confirm a clear Arabic "تعذّر الاتصال بالخادم" (connection failed) message appears rather than an infinite spinner or an unhandled exception.
- [ ] Start an action (e.g. submitting the Create Activity form) then disable network mid-request — confirm the error path fires cleanly and the form remains usable/resubmittable, not stuck in a permanent loading state.
- [ ] Re-enable network and confirm the app recovers on the next user-triggered action (pull-to-refresh, retry button) without requiring a full app restart.

## 14. Error Handling

- [ ] Every API error surfaces as an Arabic message end-to-end — deliberately trigger a validation error, a 403, a 404, and a 500-class error (e.g. temporarily point the Flutter app at a wrong `API_BASE_URL` for the 500/connection-error case) and confirm each shows appropriately in Arabic, not a raw exception or English stack trace.
- [ ] Confirm the backend's `GlobalExceptionMiddleware` logs the *real* exception detail server-side (check Serilog console output) even while the client only sees the sanitized Arabic message — this is the intended split, verify it holds.
- [ ] Confirm a `RequiresOverride` response is visually distinct in Flutter from a hard rejection — a Teacher should understand "this needs admin approval" is different from "this is not allowed," per the UI built for that case.
- [ ] Trigger a rate-limit response (rapid repeated login attempts) — confirm `429` is returned and the client shows something reasonable rather than treating it as a generic failure.

---

## After completing this checklist
Record which items failed, with enough detail (exact request, exact
response, exact screen state) to hand back for a fix — this checklist's
job is to produce that list, not to fix anything itself.
