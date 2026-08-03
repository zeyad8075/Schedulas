using FluentValidation;
using MediatR;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.Activities.Commands;

/// <summary>
/// Re-submits a candidate that previously came back RequiresOverride, this
/// time with a mandatory justification, saving it if the caller's role
/// clears the Department-Admin-or-above bar (Constitution §18,
/// API Design §8, enforced by [Authorize(Roles=...)] at the controller).
///
/// SECURITY FIX (Write-Side Ownership Audit): this handler previously
/// trusted ClassId/InstitutionId with no cross-check between them, and no
/// verification that a DepartmentAdmin's override authority was actually
/// scoped to their own department. This is arguably the highest-risk
/// command in the system — it deliberately bypasses the Rule Engine's
/// normal validation — so under-checking it here is worse than
/// under-checking a normal Create. Now verifies both.
/// </summary>
public sealed record OverrideActivityCommand(
    Guid ClassId,
    Guid InstitutionId,
    ActivityType ActivityType,
    string Title,
    string? Description,
    DateOnly ScheduledDate,
    TimeOnly? ScheduledTime,
    TimeOnly? EndTime,
    TimeSpan? Duration,
    int Priority,
    decimal? EstimatedWeight,
    string? MetadataJson,
    Guid TriggeredRuleId,
    string ReasonCode,
    string OverrideJustification
) : IRequest<ActivityDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class OverrideActivityCommandValidator : AbstractValidator<OverrideActivityCommand>
{
    public OverrideActivityCommandValidator()
    {
        RuleFor(x => x.OverrideJustification)
            .NotEmpty()
            .MinimumLength(10)
            .WithMessage("Override justification must be meaningful, not a placeholder.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Priority).InclusiveBetween(1, 3).WithMessage("Priority must be between 1 and 3.");
        RuleFor(x => x)
            .Must(x => x.EndTime > x.ScheduledTime)
            .When(x => x.ScheduledTime.HasValue && x.EndTime.HasValue)
            .WithMessage("EndTime must be after ScheduledTime.");
    }
}

public sealed class OverrideActivityCommandHandler : IRequestHandler<OverrideActivityCommand, ActivityDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public OverrideActivityCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ActivityDto> Handle(OverrideActivityCommand request, CancellationToken cancellationToken)
    {
        await ActivityAuthorization.EnsureClassBelongsToInstitutionAsync(_db, request.ClassId, request.InstitutionId, cancellationToken);
        await Schedulas.Application.Features.OrgHierarchy.Commands.OrgHierarchyAuthorization
            .EnsureCanManageClassAsync(_db, _currentUser, request.ClassId, cancellationToken);

        var activity = Activity.CreateCandidate(
            request.ClassId, request.InstitutionId, request.ActivityType, request.Title,
            request.Description, request.ScheduledDate, request.ScheduledTime,
            request.EndTime, request.Duration, request.Priority,
            request.EstimatedWeight, request.MetadataJson);

        activity.MarkOverrideApproved();
        _db.Activities.Add(activity);

        _db.RuleEvaluationLogs.Add(new RuleEvaluationLog(
            activity.Id, request.InstitutionId, RuleOutcome.RequiresOverride, request.TriggeredRuleId,
            request.ReasonCode, request.OverrideJustification, _currentUser.UserId));

        await _db.SaveChangesAsync(cancellationToken);

        return new ActivityDto
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
        };
    }
}
