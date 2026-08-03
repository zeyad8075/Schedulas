using FluentValidation;
using MediatR;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using Schedulas.Domain.RuleEngine;

namespace Schedulas.Application.Features.Activities.Commands;

public sealed record CreateActivityCommand(
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
    string? MetadataJson
) : IRequest<ActivitySubmissionResult>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null; // resolved from Class -> Course -> Program at handler level if needed
}

public sealed class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ScheduledDate).NotEqual(default(DateOnly));
        RuleFor(x => x.EstimatedWeight).GreaterThanOrEqualTo(0).When(x => x.EstimatedWeight.HasValue);
        
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
/// Implements the flow documented in
/// 06_Sequence_Diagrams/1_Create_Activity_via_Rule_Engine.mermaid exactly:
/// build candidate -> RuleEngine.EvaluateAsync -> branch on outcome ->
/// persist + log + raise event, or return a non-persisting result.
/// </summary>
public sealed class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivitySubmissionResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IRuleEngine _ruleEngine;
    private readonly ICurrentUserService _currentUser;

    public CreateActivityCommandHandler(IApplicationDbContext db, IRuleEngine ruleEngine, ICurrentUserService currentUser)
    {
        _db = db;
        _ruleEngine = ruleEngine;
        _currentUser = currentUser;
    }

    public async Task<ActivitySubmissionResult> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        await ActivityAuthorization.EnsureClassBelongsToInstitutionAsync(_db, request.ClassId, request.InstitutionId, cancellationToken);
        await ActivityAuthorization.EnsureCanCreateForClassAsync(_db, _currentUser, request.ClassId, cancellationToken);

        var candidate = Activity.CreateCandidate(
            request.ClassId,
            request.InstitutionId,
            request.ActivityType,
            request.Title,
            request.Description,
            request.ScheduledDate,
            request.ScheduledTime,
            request.EndTime,
            request.Duration,
            request.Priority,
            request.EstimatedWeight,
            request.MetadataJson);

        var result = await _ruleEngine.EvaluateAsync(candidate, cancellationToken);

        switch (result.Outcome)
        {
            case RuleOutcome.Rejected:
                await LogEvaluation(activityId: null, request.InstitutionId, result, overrideNote: null, cancellationToken);
                throw new RuleViolationException(result.ReasonCode!, result.TriggeredRuleId,
                    $"Activity rejected by rule {result.TriggeredRuleId}: {result.ReasonCode}");

            case RuleOutcome.RequiresOverride:
                // Caller (Command) doesn't carry override intent here — creation
                // without override info always surfaces RequiresOverride back to
                // the client rather than silently escalating. A separate
                // OverrideActivityCommand (Department Admin+ only) completes it.
                await LogEvaluation(activityId: null, request.InstitutionId, result, overrideNote: null, cancellationToken);
                return ActivitySubmissionResult.NeedsOverride(result.ReasonCode!, result.TriggeredRuleId!.Value);

            case RuleOutcome.Approved:
            default:
                candidate.MarkApproved();
                _db.Activities.Add(candidate);
                await _db.SaveChangesAsync(cancellationToken);
                await LogEvaluation(candidate.Id, request.InstitutionId, result, overrideNote: null, cancellationToken);
                // ActivityScheduledEvent raised inside MarkApproved() is dispatched
                // by the domain-event-dispatch pipeline behavior after SaveChanges
                // (registered in the DI composition root — Presentation layer).
                return ActivitySubmissionResult.Created(ToDto(candidate));
        }
    }

    private async Task LogEvaluation(Guid? activityId, Guid institutionId, RuleEngineResult result,
        string? overrideNote, CancellationToken ct)
    {
        var log = new RuleEvaluationLog(
            activityId, institutionId, result.Outcome, result.TriggeredRuleId,
            result.ReasonCode, overrideNote, _currentUser.UserId);

        _db.RuleEvaluationLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    private static ActivityDto ToDto(Activity a) => new()
    {
        Id = a.Id,
        ClassId = a.ClassId,
        ActivityType = a.ActivityType,
        Title = a.Title,
        Description = a.Description,
        ScheduledDate = a.ScheduledDate,
        ScheduledTime = a.ScheduledTime,
        EndTime = a.EndTime,
        Duration = a.Duration,
        Priority = a.Priority,
        EstimatedWeight = a.EstimatedWeight,
        Status = a.Status,
        MetadataJson = a.MetadataJson
    };
}
