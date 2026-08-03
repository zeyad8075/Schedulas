using Schedulas.Domain.Common;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.Events;

/// <summary>
/// Raised when an Activity is successfully persisted (Approved or
/// RequiresOverride-approved). Consumed by an Application-layer handler
/// that fans out Notifications — Domain never talks to NotificationService
/// directly (Architecture §3.1).
/// </summary>
public sealed class ActivityScheduledEvent : DomainEvent
{
    public Guid ActivityId { get; }
    public Guid ClassId { get; }
    public ActivityType ActivityType { get; }
    public bool WasOverride { get; }

    public ActivityScheduledEvent(Guid activityId, Guid classId, ActivityType activityType, bool wasOverride)
    {
        ActivityId = activityId;
        ClassId = classId;
        ActivityType = activityType;
        WasOverride = wasOverride;
    }
}

public sealed class ActivityEditedEvent : DomainEvent
{
    public Guid ActivityId { get; }
    public Guid ClassId { get; }

    public ActivityEditedEvent(Guid activityId, Guid classId)
    {
        ActivityId = activityId;
        ClassId = classId;
    }
}

public sealed class ActivityCancelledEvent : DomainEvent
{
    public Guid ActivityId { get; }
    public Guid ClassId { get; }

    public ActivityCancelledEvent(Guid activityId, Guid classId)
    {
        ActivityId = activityId;
        ClassId = classId;
    }
}

/// <summary>
/// Raised when the Rule Engine rejects or flags an activity, independent of
/// whether the exception path is also used — lets Application-layer
/// listeners (e.g., analytics, admin dashboards) react without coupling to
/// the exception flow.
/// </summary>
public sealed class RuleViolatedEvent : DomainEvent
{
    public Guid? ActivityId { get; }
    public Guid TriggeredRuleId { get; }
    public RuleOutcome Outcome { get; }
    public string ReasonCode { get; }

    public RuleViolatedEvent(Guid? activityId, Guid triggeredRuleId, RuleOutcome outcome, string reasonCode)
    {
        ActivityId = activityId;
        TriggeredRuleId = triggeredRuleId;
        Outcome = outcome;
        ReasonCode = reasonCode;
    }
}

public sealed class ActivityRestoredEvent : DomainEvent
{
    public Guid ActivityId { get; }
    public Guid ClassId { get; }

    public ActivityRestoredEvent(Guid activityId, Guid classId)
    {
        ActivityId = activityId;
        ClassId = classId;
    }
}
