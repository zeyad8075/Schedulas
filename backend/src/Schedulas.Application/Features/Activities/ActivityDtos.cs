using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.Activities;

public sealed class ActivityDto
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public ActivityType ActivityType { get; init; }
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public DateOnly ScheduledDate { get; init; }
    public TimeOnly? ScheduledTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public TimeSpan? Duration { get; init; }
    public int Priority { get; init; }
    public decimal? EstimatedWeight { get; init; }
    public ActivityStatus Status { get; init; }
    public string? MetadataJson { get; init; }
}

/// <summary>
/// Returned by Create/Edit when the Rule Engine outcome needs to reach the
/// client — either the created resource, or a rejection/override-required
/// explanation resolved to an Arabic message at the Presentation edge
/// (Architecture §7). Kept as a discriminated result rather than throwing
/// for the RequiresOverride case, since that's an expected, actionable
/// outcome — not an error.
/// </summary>
public sealed class ActivitySubmissionResult
{
    public bool Success { get; init; }
    public ActivityDto? Activity { get; init; }
    public bool RequiresOverride { get; init; }
    public string? ReasonCode { get; init; }
    public Guid? TriggeredRuleId { get; init; }

    public static ActivitySubmissionResult Created(ActivityDto dto) =>
        new() { Success = true, Activity = dto };

    public static ActivitySubmissionResult NeedsOverride(string reasonCode, Guid ruleId) =>
        new() { Success = false, RequiresOverride = true, ReasonCode = reasonCode, TriggeredRuleId = ruleId };
}
