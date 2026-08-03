using FluentValidation;
using MediatR;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace Schedulas.Application.Features.Institutions.Commands;

public sealed record InstitutionDto(
    Guid Id, string Name, InstitutionType Type, string Timezone, string? LogoUrl, bool IsSuspended);

public sealed record CreateInstitutionCommand(string Name, InstitutionType Type, string Timezone) : IRequest<InstitutionDto>;

public sealed class CreateInstitutionCommandValidator : AbstractValidator<CreateInstitutionCommand>
{
    public CreateInstitutionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Timezone).NotEmpty();
    }
}

public sealed class CreateInstitutionCommandHandler : IRequestHandler<CreateInstitutionCommand, InstitutionDto>
{
    private readonly IApplicationDbContext _db;

    public CreateInstitutionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<InstitutionDto> Handle(CreateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = new Institution(request.Name, request.Type, request.Timezone);
        _db.Institutions.Add(institution);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(institution);
    }

    internal static InstitutionDto ToDto(Institution i) =>
        new(i.Id, i.Name, i.Type, i.Timezone, i.LogoUrl, i.IsSuspended);
}

/// <summary>
/// SECURITY FIX (Phase 8 Verification pass): previously any caller with
/// the InstitutionAdmin role could pass ANY InstitutionId and update that
/// institution's name/logo/timezone, not just their own. Now enforced via
/// ITenantScopedRequest, same mechanism as CreateDepartmentCommand.
/// </summary>
public sealed record UpdateInstitutionCommand(Guid InstitutionId, string Name, string? LogoUrl, string Timezone)
    : IRequest<InstitutionDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class UpdateInstitutionCommandValidator : AbstractValidator<UpdateInstitutionCommand>
{
    public UpdateInstitutionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Timezone).NotEmpty();
    }
}

public sealed class UpdateInstitutionCommandHandler : IRequestHandler<UpdateInstitutionCommand, InstitutionDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateInstitutionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<InstitutionDto> Handle(UpdateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = await _db.Institutions.FindAsync([request.InstitutionId], cancellationToken)
            ?? throw new EntityNotFoundException("Institution", request.InstitutionId);

        institution.UpdateDetails(request.Name, request.LogoUrl, request.Timezone);
        await _db.SaveChangesAsync(cancellationToken);
        return CreateInstitutionCommandHandler.ToDto(institution);
    }
}

public sealed record SuspendInstitutionCommand(Guid InstitutionId) : IRequest<Unit>;

public sealed class SuspendInstitutionCommandHandler : IRequestHandler<SuspendInstitutionCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public SuspendInstitutionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(SuspendInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = await _db.Institutions.FindAsync([request.InstitutionId], cancellationToken)
            ?? throw new EntityNotFoundException("Institution", request.InstitutionId);

        institution.Suspend();
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record ReactivateInstitutionCommand(Guid InstitutionId) : IRequest<Unit>;

public sealed class ReactivateInstitutionCommandHandler : IRequestHandler<ReactivateInstitutionCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public ReactivateInstitutionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(ReactivateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = await _db.Institutions.FindAsync([request.InstitutionId], cancellationToken)
            ?? throw new EntityNotFoundException("Institution", request.InstitutionId);

        institution.Reactivate();
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record DeleteInstitutionCommand(Guid InstitutionId) : IRequest<Unit>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class DeleteInstitutionCommandHandler : IRequestHandler<DeleteInstitutionCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeleteInstitutionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = await _db.Institutions.FindAsync([request.InstitutionId], cancellationToken)
            ?? throw new EntityNotFoundException("Institution", request.InstitutionId);

        // Validation: Cannot delete if it has active departments
        bool hasDepartments = await _db.Departments.AnyAsync(d => d.InstitutionId == request.InstitutionId, cancellationToken);
        if (hasDepartments)
            throw new InvalidStateTransitionException("INSTITUTION_HAS_DEPARTMENTS", "Cannot delete an institution that contains departments.");

        _db.Institutions.Remove(institution);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record RestoreInstitutionCommand(Guid InstitutionId) : IRequest<Unit>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class RestoreInstitutionCommandHandler : IRequestHandler<RestoreInstitutionCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public RestoreInstitutionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(RestoreInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = await _db.Institutions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == request.InstitutionId, cancellationToken)
            ?? throw new EntityNotFoundException("Institution", request.InstitutionId);


        institution.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
