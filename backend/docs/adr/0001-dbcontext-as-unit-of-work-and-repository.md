# ADR-0001: DbContext as Unit of Work and Repository

**Status:** Accepted
**Date:** 2026-07-25
**Deciders:** Project owner, ratified following the Phase 8 Verification pass
**Amends:** PROJECT_CONSTITUTION.md §6 (Architecture Principles), §7 (Clean Architecture Guidelines)

---

## Context

PROJECT_CONSTITUTION.md's original tech-stack and architecture sections named
"Repository Pattern" and "Unit of Work" as distinct constructs the
Infrastructure layer would provide, separate from EF Core's `DbContext`
itself. The actual backend implementation (Parts 1–5) instead exposes a
single `IApplicationDbContext` interface — implemented by
`SchedulasDbContext` in Infrastructure — directly to Application-layer
CQRS handlers, which call `DbSet<T>` LINQ queries and `SaveChangesAsync()`
without an intermediate per-aggregate repository class or a separate
`IUnitOfWork` abstraction.

This divergence was surfaced explicitly in the Phase 8 Verification pass's
Backend Readiness Report (Architecture Status section) as a conflict
requiring a decision, per the Constitution's own rule that conflicts must
be explained rather than silently resolved either way.

## Decision

**`IApplicationDbContext` (Application layer) / `SchedulasDbContext`
(Infrastructure layer) serves as both the Unit of Work and the repository
for every aggregate in the system.** No Generic Repository, no per-entity
repository interfaces (`IStudentRepository`, `IActivityRepository`, etc.),
and no separate `IUnitOfWork` abstraction will be introduced.

Concretely:
- `IApplicationDbContext` exposes one `DbSet<T>` property per aggregate
  root, plus `SaveChangesAsync(CancellationToken)`.
- Application-layer Command/Query handlers depend on `IApplicationDbContext`
  directly, querying via LINQ on the exposed `DbSet<T>` properties.
- A single `SaveChangesAsync()` call per handler is the Unit of Work
  boundary — everything an individual Command touches is committed
  together in that one call, which is exactly what a hand-rolled
  `IUnitOfWork.CommitAsync()` would do, just without an extra layer of
  indirection around a mechanism EF Core already provides.
- `SchedulasDbContext` is the only class that implements
  `IApplicationDbContext`; it lives in `Schedulas.Infrastructure.Persistence`
  and is never referenced by type from the Application layer (only through
  the interface), preserving the inward-only dependency rule.

## Rationale

1. **EF Core's `DbContext` already *is* a Unit of Work.** It tracks
   changes across every entity touched during a logical operation and
   commits them atomically via a single `SaveChangesAsync()` call, inside
   an implicit transaction. Wrapping it in a hand-rolled `IUnitOfWork`
   interface would either (a) just forward to `DbContext.SaveChangesAsync()`
   — pure ceremony with no behavioral difference — or (b) attempt to
   manage transactions independently of EF Core's own tracking, which
   invites subtle bugs (double-commits, forgotten `SaveChanges` calls,
   transaction scope mismatches) rather than preventing them.

2. **`DbSet<T>` plus LINQ already *is* a repository abstraction** — it's
   `IQueryable<T>`, testable via EF Core's in-memory/SQLite providers or
   by mocking `IApplicationDbContext` itself, and it's exactly the
   abstraction a hand-written `IRepository<T>` would end up wrapping
   anyway. A Generic Repository over an already-abstracted, already-testable
   `DbSet<T>` adds a layer that translates one query capability
   (`IQueryable`) into a narrower one (whatever methods the repository
   interface happens to expose), which historically tends to grow either
   an ever-expanding set of specific query methods or a leaky
   `IQueryable`-returning escape hatch that recreates the exact surface
   `DbSet<T>` already had — at which point the repository has added
   indirection without adding capability.

3. **This project's actual persistence needs don't call for swapping ORMs.**
   The textbook argument for the Repository Pattern is persistence-ignorance
   — the ability to swap EF Core for another data-access technology without
   touching Application-layer code. Schedulas is explicitly and permanently
   built on Supabase-hosted PostgreSQL via EF Core (Constitution §4, Database
   design throughout); there is no planned or plausible scenario in this
   project's life where that changes. Paying the indirection cost for a
   flexibility the project will never use is the wrong trade here.

4. **Clean Architecture's actual requirement — dependency inversion — is
   still fully satisfied.** The rule that matters is "Application depends
   on an abstraction it owns, Infrastructure implements it, dependencies
   point inward." `IApplicationDbContext` satisfies this exactly:
   Application defines the interface, Infrastructure implements it,
   Application never references `Microsoft.EntityFrameworkCore` types by
   name (only through the interface's `DbSet<T>` surface, which is itself
   part of the deliberately-exposed abstraction boundary — see
   Consequence 2 below). Clean Architecture does not require Repository
   and Unit of Work specifically; it requires the dependency direction
   they're one common way of achieving. This project achieves the same
   direction with fewer moving parts.

5. **This is not a retrofit — it's already how 25+ handlers across five
   feature areas are written and, as of the Phase 8 Verification pass,
   already security-hardened** (the tenant-scoping fixes in that pass were
   applied directly to `IApplicationDbContext`-based handlers). Introducing
   a repository layer now would mean rewriting every handler to match a
   pattern the codebase has already outgrown the need for, purely to match
   a document that is easier to amend than the code is to rewrite.

## Consequences

**Accepted trade-offs:**
- Application-layer code is coupled to EF Core's `DbSet<T>`/`IQueryable`
  shape, not fully persistence-ignorant. This is accepted per Rationale
  #3 — the flexibility this would buy is not needed.
- Complex multi-step queries are written as LINQ directly in Query
  handlers rather than behind named repository methods. This is
  consistent with how CQRS Query handlers are commonly built in .NET
  (query logic belongs with the query, not spread across a generic
  repository's method list) and keeps each query's logic in one place
  next to the DTO it projects into.

**What does NOT change:**
- Clean Architecture layering (Domain → Application → Infrastructure →
  Presentation, dependencies pointing inward only) is unaffected and
  remains strictly enforced.
- The `TenantAuthorizationBehavior` / `ITenantScopedRequest` pattern for
  tenant-isolation checks is unaffected — it operates at the MediatR
  pipeline level, orthogonal to how data access itself is abstracted.
- Testability is unaffected: `IApplicationDbContext` is mockable/fakeable
  exactly as a repository interface would be; EF Core's InMemory or
  SQLite providers can back it for integration-style tests against the
  real `SchedulasDbContext`.

**Constitution amendment required:** PROJECT_CONSTITUTION.md §6 and §7
have been updated (v1.1) to reference this ADR instead of naming
"Repository Pattern" and "Unit of Work" as separate constructs.

## Alternatives Considered

- **Generic `IRepository<T>` + `IUnitOfWork` wrapping `DbContext`:**
  Rejected per Rationale #1–2 — adds indirection without adding capability
  for this project's actual needs.
- **Per-aggregate repositories** (`IStudentRepository`, `IRuleDefinitionRepository`,
  etc.) **+ `IUnitOfWork`:** Rejected for the same reason, at higher cost
  (one interface + implementation pair per aggregate, ~15+ additional
  files, for behavior `DbSet<T>` already provides).
- **Leave the Constitution's original wording unchanged and retrofit the
  code to match it:** Rejected — see Rationale #5; would mean discarding
  a working, already-hardened pattern to match a document, rather than
  updating the document to match a deliberate and defensible engineering
  decision.
