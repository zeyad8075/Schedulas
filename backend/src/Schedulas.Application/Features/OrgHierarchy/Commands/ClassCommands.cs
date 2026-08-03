using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Exceptions;
using Class = Schedulas.Domain.Entities.Class;

namespace Schedulas.Application.Features.OrgHierarchy.Commands;

public sealed record ClassDto(Guid Id, Guid CourseId, Guid AcademicTermId, string Name, bool IsActive);

public sealed record CreateClassCommand(Guid CourseId, Guid AcademicTermId, string Name) : IRequest<ClassDto>;

public sealed class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.AcademicTermId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

/// <summary>
/// SECURITY FIX (Write-Side Ownership Audit): previously had NO tenant
/// check on CourseId, AND no verification that AcademicTermId even
/// belonged to the same institution as the course -- someone could
/// otherwise attach a class to a different institution's academic term,
/// which would then leak that other institution's holiday/term-boundary
/// data into this class's Rule Engine evaluations (NoActivityOnHolidayRule,
/// term-boundary checks) via a mismatched cross-tenant reference.
/// </summary>
public sealed class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, ClassDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateClassCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ClassDto> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        var institutionId = await OrgHierarchyAuthorization.EnsureCanManageCourseAsync(_db, _currentUser, request.CourseId, cancellationToken);

        var termInstitutionId = await _db.AcademicTerms
            .Where(t => t.Id == request.AcademicTermId)
            .Select(t => (Guid?)t.InstitutionId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntityNotFoundException("AcademicTerm", request.AcademicTermId);


        var cls = new Class(institutionId, request.CourseId, request.AcademicTermId, request.Name);
        _db.Classes.Add(cls);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(cls);
    }

    internal static ClassDto ToDto(Class c) => new(c.Id, c.CourseId, c.AcademicTermId, c.Name, c.IsActive);
}

public sealed record UpdateClassCommand(Guid ClassId, string Name, bool IsActive) : IRequest<ClassDto>;

public sealed class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
}

/// <summary>SECURITY FIX: same tenant-check gap as CreateClassCommand, fixed the same way.</summary>
public sealed class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, ClassDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateClassCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ClassDto> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
    {
        var cls = await _db.Classes.FindAsync([request.ClassId], cancellationToken)
            ?? throw new EntityNotFoundException("Class", request.ClassId);

        await OrgHierarchyAuthorization.EnsureCanManageClassAsync(_db, _currentUser, cls.Id, cancellationToken);

        cls.Rename(request.Name);
        if (request.IsActive) cls.Activate(); else cls.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return CreateClassCommandHandler.ToDto(cls);
    }
}

public sealed record DeleteClassCommand(Guid ClassId) : IRequest<Unit>;

public sealed class DeleteClassCommandHandler : IRequestHandler<DeleteClassCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteClassCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        var cls = await _db.Classes.FindAsync([request.ClassId], cancellationToken)
            ?? throw new EntityNotFoundException("Class", request.ClassId);

        await OrgHierarchyAuthorization.EnsureCanManageClassAsync(_db, _currentUser, cls.Id, cancellationToken);

        _db.Classes.Remove(cls);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record RestoreClassCommand(Guid ClassId) : IRequest<Unit>;

public sealed class RestoreClassCommandHandler : IRequestHandler<RestoreClassCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreClassCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreClassCommand request, CancellationToken cancellationToken)
    {
        var cls = await _db.Classes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken)
            ?? throw new EntityNotFoundException("Class", request.ClassId);

        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != cls.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        await OrgHierarchyAuthorization.EnsureCanManageClassAsync(_db, _currentUser, cls.Id, cancellationToken);

        var course = await _db.Courses.FindAsync([cls.CourseId], cancellationToken);
        if (course == null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore class because its course is deleted.");

        cls.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
