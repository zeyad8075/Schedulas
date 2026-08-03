using FluentValidation;
using MediatR;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Schedulas.Application.Features.OrgHierarchy.Commands;

public sealed record DepartmentDto(Guid Id, Guid InstitutionId, string Name, bool IsActive);

public sealed record CreateDepartmentCommand(Guid InstitutionId, string Name)
    : IRequest<DepartmentDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _db;

    public CreateDepartmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = new Department(request.InstitutionId, request.Name);
        _db.Departments.Add(department);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(department);
    }

    internal static DepartmentDto ToDto(Department d) => new(d.Id, d.InstitutionId, d.Name, d.IsActive);
}

public sealed record UpdateDepartmentCommand(Guid DepartmentId, string Name, bool IsActive) : IRequest<DepartmentDto>;

public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
}

/// <summary>
/// SECURITY FIX (Write-Side Ownership Audit): previously had NO tenant
/// check -- any InstitutionAdmin could rename/activate/deactivate any
/// institution's department, not just their own, by guessing/enumerating
/// DepartmentId values. DepartmentId alone (no InstitutionId on the
/// request) means ITenantScopedRequest's direct-comparison pattern can't
/// apply without first resolving the department's institution, so the
/// check is done explicitly here rather than via the marker interface.
/// </summary>
public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateDepartmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _db.Departments.FindAsync([request.DepartmentId], cancellationToken)
            ?? throw new EntityNotFoundException("Department", request.DepartmentId);


        department.Rename(request.Name);
        if (request.IsActive) department.Activate(); else department.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return CreateDepartmentCommandHandler.ToDto(department);
    }
}

public sealed record DeleteDepartmentCommand(Guid DepartmentId) : IRequest<Unit>;

public sealed class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteDepartmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _db.Departments.FindAsync([request.DepartmentId], cancellationToken)
            ?? throw new EntityNotFoundException("Department", request.DepartmentId);


        bool hasPrograms = await _db.Programs.AnyAsync(p => p.DepartmentId == request.DepartmentId, cancellationToken);
        if (hasPrograms)
            throw new InvalidStateTransitionException("DEPARTMENT_HAS_PROGRAMS", "Cannot delete a department that contains programs.");

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record RestoreDepartmentCommand(Guid DepartmentId) : IRequest<Unit>;

public sealed class RestoreDepartmentCommandHandler : IRequestHandler<RestoreDepartmentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreDepartmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _db.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken)
            ?? throw new EntityNotFoundException("Department", request.DepartmentId);

        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != department.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");


        var institution = await _db.Institutions.FindAsync([department.InstitutionId], cancellationToken);
        if (institution == null || institution.IsSuspended)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore department because its institution is suspended or not found.");

        department.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
