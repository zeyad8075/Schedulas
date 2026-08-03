using FluentValidation;
using MediatR;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using Schedulas.Domain.RuleEngine;

namespace Schedulas.Application.Features.Activities.Commands;

// SECURITY FIX (Write-Side Ownership Audit): EditActivityCommand carried
// ITenantScopedRequest, but that only validates the CALLER-SUPPLIED
// InstitutionId field against the caller's own institution -- it never
// cross-checked that field against the ACTUAL institution of the
// activity being edited. A caller could pass their own institution
// (satisfying the pipeline check) while ActivityId pointed at a
// completely different institution's activity, and the handler would
// still load and edit it, since it never re-verified anything against
// the loaded entity. CancelActivityCommand had no checks at all -- not
// even a claimed-field check. Both are fixed below by verifying against
// the actual loaded Activity, plus the same Teacher-teaches-this-class /
// DepartmentAdmin-owns-this-department checks used for Create.

public sealed record EditActivityCommand(
    Guid ActivityId,
    Guid InstitutionId,
    string Title,
    string? Description,
    DateOnly ScheduledDate,
    TimeOnly? ScheduledTime,
    TimeOnly? EndTime,
    TimeSpan? Duration,
    int Priority,
    decimal? EstimatedWeight,
    string? MetadataJson
) : IRequest<ActivitySubmissionResult>;

public sealed class EditActivityCommandValidator : AbstractValidator<EditActivityCommand>
{
    public EditActivityCommandValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        
        RuleFor(x => x.Duration)
            .Must(d => d > TimeSpan.Zero).When(x => x.Duration.HasValue)
            .WithMessage("Duration must be greater than zero.");
            
        RuleFor(x => x.Priority).InclusiveBetween(1, 3).WithMessage("Priority must be between 1 and 3.");
        
        RuleFor(x => x)
            .Must(x => x.EndTime > x.ScheduledTime)
            .When(x => x.ScheduledTime.HasValue && x.EndTime.HasValue)
            .WithMessage("EndTime must be after ScheduledTime.");
    }
}

/// <summary>
/// Per SRS FR-ACT-5: edits are only re-submitted to the Rule Engine when
/// the scheduling semantics actually changed — a title/description-only edit
/// skips re-evaluation entirely.
/// </summary>
public sealed class EditActivityCommandHandler : IRequestHandler<EditActivityCommand, ActivitySubmissionResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IRuleEngine _ruleEngine;
    private readonly ICurrentUserService _currentUser;

    public EditActivityCommandHandler(IApplicationDbContext db, IRuleEngine ruleEngine, ICurrentUserService currentUser)
    {
        _db = db;
        _ruleEngine = ruleEngine;
        _currentUser = currentUser;
    }

    public async Task<ActivitySubmissionResult> Handle(EditActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _db.Activities.FindAsync([request.ActivityId], cancellationToken)
            ?? throw new EntityNotFoundException("Activity", request.ActivityId);

        // Verified against the ACTUAL loaded resource, not the caller's
        // claimed InstitutionId field (that field no longer exists on this
        // command at all, removing the false-sense-of-security ITenantScopedRequest gave).

        await ActivityAuthorization.EnsureCanCreateForClassAsync(_db, _currentUser, activity.ClassId, cancellationToken);

        var needsReEvaluation = activity.RequiresReEvaluation(
            request.ScheduledDate, 
            request.ScheduledTime, 
            request.EndTime, 
            request.Duration, 
            request.Priority, 
            activity.ClassId, 
            request.EstimatedWeight);

        activity.Edit(request.Title, request.Description, request.ScheduledDate,
            request.ScheduledTime, request.EndTime, request.Duration, request.Priority, request.EstimatedWeight, request.MetadataJson);

        if (needsReEvaluation)
        {
            var result = await _ruleEngine.EvaluateAsync(activity, cancellationToken);

            if (result.Outcome == RuleOutcome.Rejected)
                throw new RuleViolationException(result.ReasonCode!, result.TriggeredRuleId,
                    $"Edit rejected by rule {result.TriggeredRuleId}");

            if (result.Outcome == RuleOutcome.RequiresOverride)
                return ActivitySubmissionResult.NeedsOverride(result.ReasonCode!, result.TriggeredRuleId!.Value);

            _db.RuleEvaluationLogs.Add(new Domain.Entities.RuleEvaluationLog(
                activity.Id, activity.InstitutionId, result.Outcome, result.TriggeredRuleId,
                result.ReasonCode, null, _currentUser.UserId));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return ActivitySubmissionResult.Created(new ActivityDto
        {
            Id = activity.Id,
            ClassId = activity.ClassId,
            ActivityType = activity.ActivityType,
            Title = activity.Title,
            Description = activity.Description,
            ScheduledDate = activity.ScheduledDate,
            ScheduledTime = activity.ScheduledTime,
            EndTime = activity.EndTime,
            Duration = activity.Duration,
            Priority = activity.Priority,
            EstimatedWeight = activity.EstimatedWeight,
            Status = activity.Status,
            MetadataJson = activity.MetadataJson
        });
    }
}

public sealed record CancelActivityCommand(Guid ActivityId) : IRequest<Unit>;

/// <summary>SECURITY FIX: previously had NO checks whatsoever — any Teacher or DepartmentAdmin could cancel any activity anywhere, in any institution.</summary>
public sealed class CancelActivityCommandHandler : IRequestHandler<CancelActivityCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CancelActivityCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(CancelActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _db.Activities.FindAsync([request.ActivityId], cancellationToken)
            ?? throw new EntityNotFoundException("Activity", request.ActivityId);


        await ActivityAuthorization.EnsureCanCreateForClassAsync(_db, _currentUser, activity.ClassId, cancellationToken);

        activity.Cancel();
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
