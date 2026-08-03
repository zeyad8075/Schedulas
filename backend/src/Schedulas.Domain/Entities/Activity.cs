using Schedulas.Domain.Common;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Events;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Domain.Entities;

/// <summary>
/// Single polymorphic table for Assignment / Exam / Project / Presentation /
/// Event, discriminated by <see cref="ActivityType"/> (Architecture §9,
/// Database Design §6). Type-specific fields live in <see cref="Metadata"/>.
///
/// An Activity is never constructed directly by a handler and then saved —
/// it must first pass through IRuleEngine.EvaluateAsync (Architecture §5).
/// The constructor here only builds the *candidate*; RuleEngineOrchestrator
/// decides whether SaveChangesAsync ever gets called for it.
/// </summary>
public class Activity : BaseEntity, IMustHaveTenant
{
    public Guid ClassId { get; private set; }
    public Guid InstitutionId { get; private set; } // denormalized, see Database Design §6 note
    public ActivityType ActivityType { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateOnly ScheduledDate { get; private set; }
    public TimeOnly? ScheduledTime { get; private set; }
    public decimal? EstimatedWeight { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public int Priority { get; private set; }
    public ActivityStatus Status { get; private set; }
    public string? MetadataJson { get; private set; }

    private Activity() { }

    /// <summary>Builds a not-yet-persisted candidate for Rule Engine evaluation.</summary>
    public static Activity CreateCandidate(
        Guid classId,
        Guid institutionId,
        ActivityType activityType,
        string title,
        string? description,
        DateOnly scheduledDate,
        TimeOnly? scheduledTime,
        TimeOnly? endTime,
        TimeSpan? duration,
        int priority,
        decimal? estimatedWeight,
        string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Activity title is required.", nameof(title));

        if (scheduledTime.HasValue && endTime.HasValue && scheduledTime.Value >= endTime.Value)
            throw new ArgumentException("EndTime must be after ScheduledTime.");

        return new Activity
        {
            ClassId = classId,
            InstitutionId = institutionId,
            ActivityType = activityType,
            Title = title,
            Description = description,
            ScheduledDate = scheduledDate,
            ScheduledTime = scheduledTime,
            EndTime = endTime,
            Duration = duration,
            Priority = priority,
            EstimatedWeight = estimatedWeight,
            MetadataJson = metadataJson,
            Status = ActivityStatus.Approved // set to final status by MarkApproved/MarkOverrideApproved before save
        };
    }

    /// <summary>Called by the handler only after IRuleEngine returns Approved.</summary>
    public void MarkApproved()
    {
        Status = ActivityStatus.Approved;
        RaiseDomainEvent(new ActivityScheduledEvent(Id, ClassId, ActivityType, wasOverride: false));
    }

    /// <summary>Called by the handler only after a valid Department-Admin+ override.</summary>
    public void MarkOverrideApproved()
    {
        Status = ActivityStatus.RequiresOverrideApproved;
        RaiseDomainEvent(new ActivityScheduledEvent(Id, ClassId, ActivityType, wasOverride: true));
    }

    public void Edit(string title, string? description, DateOnly scheduledDate,
        TimeOnly? scheduledTime, TimeOnly? endTime, TimeSpan? duration, int priority, decimal? estimatedWeight, string? metadataJson)
    {
        if (Status == ActivityStatus.Cancelled)
            throw new InvalidStateTransitionException(
                "ACTIVITY_CANNOT_EDIT_CANCELLED",
                $"Activity {Id} is cancelled and cannot be edited.");

        if (scheduledTime.HasValue && endTime.HasValue && scheduledTime.Value >= endTime.Value)
            throw new ArgumentException("EndTime must be after ScheduledTime.");

        Title = title;
        Description = description;
        ScheduledDate = scheduledDate;
        ScheduledTime = scheduledTime;
        EndTime = endTime;
        Duration = duration;
        Priority = priority;
        EstimatedWeight = estimatedWeight;
        MetadataJson = metadataJson;

        RaiseDomainEvent(new ActivityEditedEvent(Id, ClassId));
    }

    /// <summary>True when any scheduling semantics changed in a way that requires re-evaluation by the Rule Engine.</summary>
    public bool RequiresReEvaluation(DateOnly newDate, TimeOnly? newStartTime, TimeOnly? newEndTime, TimeSpan? newDuration, int newPriority, Guid newClassId, decimal? newWeight) =>
        newDate != ScheduledDate || 
        newStartTime != ScheduledTime || 
        newEndTime != EndTime || 
        newDuration != Duration || 
        newPriority != Priority || 
        newClassId != ClassId || 
        newWeight != EstimatedWeight;

    public void Cancel()
    {
        if (Status == ActivityStatus.Cancelled) return;

        Status = ActivityStatus.Cancelled;
        RaiseDomainEvent(new ActivityCancelledEvent(Id, ClassId));
    }

    public void Restore()
    {
        if (Status == ActivityStatus.Cancelled)
            throw new InvalidStateTransitionException("CANNOT_RESTORE_CANCELLED", "Cannot restore a cancelled activity.");

        DeletedAt = null;
        RaiseDomainEvent(new ActivityRestoredEvent(Id, ClassId));
    }
}
