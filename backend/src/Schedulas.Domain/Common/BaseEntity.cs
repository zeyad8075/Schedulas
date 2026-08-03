namespace Schedulas.Domain.Common;

/// <summary>
/// Base for genuinely immutable, append-only records — e.g. an audit/event
/// log row that is written once and never updated or (soft-)deleted.
/// Deliberately does NOT implement IAuditableEntity or ISoftDeletableEntity:
/// SchedulasDbContext's global soft-delete query filter applies to every
/// type assignable to ISoftDeletableEntity, so an entity that ignores its
/// DeletedAt mapping (as an immutable log has no reason to keep one) while
/// still implementing that interface would crash at model-build time —
/// the filter expression would reference a property EF doesn't know about.
/// Using a distinct, narrower base avoids that failure mode entirely
/// instead of working around it per-entity.
/// </summary>
public abstract class ImmutableRecordEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

/// <summary>
/// Marker + contract for the standard audit block required on every table
/// per PROJECT_CONSTITUTION.md §10. Applied via EF Core interceptors, never
/// set manually by handlers.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Contract for soft-deletable entities. Rows are never physically deleted;
/// DeletedAt is stamped instead and every query filters WHERE DeletedAt IS NULL.
/// </summary>
public interface ISoftDeletableEntity
{
    DateTimeOffset? DeletedAt { get; set; }
    bool IsDeleted => DeletedAt.HasValue;
}

/// <summary>
/// Base class for every aggregate/entity in the system. Provides the UUID
/// primary key, the audit block, soft-delete support, and a domain-event
/// buffer that the SaveChangesAsync interceptor dispatches after a
/// successful commit (Architecture §3.1 — Domain raises events, Application
/// layer reacts to them; Domain itself never knows who's listening).
/// </summary>
public abstract class BaseEntity : IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Base class for all domain events. Kept intentionally minimal — events
/// carry only IDs and language-neutral data, never Arabic strings
/// (Architecture §7 — localization is resolved at the edge, not in Domain).
/// </summary>
public abstract class DomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
