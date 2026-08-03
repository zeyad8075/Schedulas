using FluentValidation;
using MediatR;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.Rules.Commands;

public sealed record RuleDefinitionDto(
    Guid Id, Guid InstitutionId, RuleScopeLevel ScopeLevel, Guid ScopeId,
    RuleType RuleType, string ParametersJson, int Priority, bool IsActive);

public sealed record CreateRuleDefinitionCommand(
    Guid InstitutionId, RuleScopeLevel ScopeLevel, Guid ScopeId,
    RuleType RuleType, string ParametersJson, int Priority
) : IRequest<RuleDefinitionDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class CreateRuleDefinitionCommandValidator : AbstractValidator<CreateRuleDefinitionCommand>
{
    public CreateRuleDefinitionCommandValidator()
    {
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.ScopeId).NotEmpty();
        RuleFor(x => x.ParametersJson).NotEmpty();
        RuleFor(x => x.ParametersJson).Must(BeValidJson).WithMessage("Parameters must be valid JSON.");
    }

    private static bool BeValidJson(string json)
    {
        try { System.Text.Json.JsonDocument.Parse(json); return true; }
        catch { return false; }
    }
}

public sealed class CreateRuleDefinitionCommandHandler : IRequestHandler<CreateRuleDefinitionCommand, RuleDefinitionDto>
{
    private readonly IApplicationDbContext _db;

    public CreateRuleDefinitionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<RuleDefinitionDto> Handle(CreateRuleDefinitionCommand request, CancellationToken cancellationToken)
    {
        var rule = new RuleDefinition(request.InstitutionId, request.ScopeLevel, request.ScopeId,
            request.RuleType, request.ParametersJson, request.Priority);

        _db.RuleDefinitions.Add(rule);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(rule);
    }

    internal static RuleDefinitionDto ToDto(RuleDefinition r) =>
        new(r.Id, r.InstitutionId, r.ScopeLevel, r.ScopeId, r.RuleType, r.ParametersJson, r.Priority, r.IsActive);
}

public sealed record UpdateRuleDefinitionCommand(
    Guid RuleId, string ParametersJson, int Priority, bool IsActive
) : IRequest<RuleDefinitionDto>;

public sealed class UpdateRuleDefinitionCommandValidator : AbstractValidator<UpdateRuleDefinitionCommand>
{
    public UpdateRuleDefinitionCommandValidator()
    {
        RuleFor(x => x.RuleId).NotEmpty();
        RuleFor(x => x.ParametersJson).NotEmpty();
    }
}

/// <summary>
/// Implements Phase 6 sequence diagram 4 exactly: this only affects future
/// Rule Engine evaluations. Already-approved activities are never
/// retroactively invalidated (Constitution §18).
///
/// SECURITY FIX (Write-Side Ownership Audit): previously had NO tenant
/// check — any InstitutionAdmin or DepartmentAdmin could edit ANY
/// institution's rules, including disabling another institution's
/// overload/conflict protections entirely. This is a safety issue, not
/// just a data-isolation one, given the Rule Engine's core purpose.
/// </summary>
public sealed class UpdateRuleDefinitionCommandHandler : IRequestHandler<UpdateRuleDefinitionCommand, RuleDefinitionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateRuleDefinitionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RuleDefinitionDto> Handle(UpdateRuleDefinitionCommand request, CancellationToken cancellationToken)
    {
        var rule = await _db.RuleDefinitions.FindAsync([request.RuleId], cancellationToken)
            ?? throw new EntityNotFoundException("RuleDefinition", request.RuleId);


        rule.Update(request.ParametersJson, request.Priority, request.IsActive);
        await _db.SaveChangesAsync(cancellationToken);

        return CreateRuleDefinitionCommandHandler.ToDto(rule);
    }
}

public sealed record DeactivateRuleDefinitionCommand(Guid RuleId) : IRequest<Unit>;

/// <summary>SECURITY FIX: same gap as UpdateRuleDefinitionCommand, fixed the same way.</summary>
public sealed class DeactivateRuleDefinitionCommandHandler : IRequestHandler<DeactivateRuleDefinitionCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeactivateRuleDefinitionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeactivateRuleDefinitionCommand request, CancellationToken cancellationToken)
    {
        var rule = await _db.RuleDefinitions.FindAsync([request.RuleId], cancellationToken)
            ?? throw new EntityNotFoundException("RuleDefinition", request.RuleId);


        rule.Deactivate();
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
