using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using Program = Schedulas.Domain.Entities.Program;

namespace Schedulas.Application.Features.OrgHierarchy.Commands;

public sealed record ProgramDto(Guid Id, Guid DepartmentId, string Name, bool IsActive);

public sealed record CreateProgramCommand(Guid DepartmentId, string Name) : IRequest<ProgramDto>;

public sealed class CreateProgramCommandValidator : AbstractValidator<CreateProgramCommand>
{
    public CreateProgramCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}

/// <summary>
/// SECURITY FIX (Write-Side Ownership Audit): previously had NO tenant
/// check -- any InstitutionAdmin or DepartmentAdmin could create a
/// program under any institution's department. Now resolves the target
/// department's institution and, for DepartmentAdmin specifically, also
/// requires the department to be their own (narrower than
/// InstitutionAdmin's institution-wide scope).
/// </summary>
public sealed class CreateProgramCommandHandler : IRequestHandler<CreateProgramCommand, ProgramDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateProgramCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProgramDto> Handle(CreateProgramCommand request, CancellationToken cancellationToken)
    {
        var institutionId = await OrgHierarchyAuthorization.EnsureCanManageDepartmentAsync(_db, _currentUser, request.DepartmentId, cancellationToken);

        var program = new Program(institutionId, request.DepartmentId, request.Name);
        _db.Programs.Add(program);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(program);
    }

    internal static ProgramDto ToDto(Program p) => new(p.Id, p.DepartmentId, p.Name, p.IsActive);
}

public sealed record UpdateProgramCommand(Guid ProgramId, string Name, bool IsActive) : IRequest<ProgramDto>;

public sealed class UpdateProgramCommandValidator : AbstractValidator<UpdateProgramCommand>
{
    public UpdateProgramCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
}

/// <summary>SECURITY FIX: same gap as CreateProgramCommand, fixed the same way.</summary>
public sealed class UpdateProgramCommandHandler : IRequestHandler<UpdateProgramCommand, ProgramDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateProgramCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProgramDto> Handle(UpdateProgramCommand request, CancellationToken cancellationToken)
    {
        var program = await _db.Programs.FindAsync([request.ProgramId], cancellationToken)
            ?? throw new EntityNotFoundException("Program", request.ProgramId);

        await OrgHierarchyAuthorization.EnsureCanManageDepartmentAsync(_db, _currentUser, program.DepartmentId, cancellationToken);

        program.Rename(request.Name);
        if (request.IsActive) program.Activate(); else program.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return CreateProgramCommandHandler.ToDto(program);
    }
}

public sealed record DeleteProgramCommand(Guid ProgramId) : IRequest<Unit>;

public sealed class DeleteProgramCommandHandler : IRequestHandler<DeleteProgramCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteProgramCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteProgramCommand request, CancellationToken cancellationToken)
    {
        var program = await _db.Programs.FindAsync([request.ProgramId], cancellationToken)
            ?? throw new EntityNotFoundException("Program", request.ProgramId);

        await OrgHierarchyAuthorization.EnsureCanManageDepartmentAsync(_db, _currentUser, program.DepartmentId, cancellationToken);

        bool hasCourses = await _db.Courses.AnyAsync(c => c.ProgramId == request.ProgramId, cancellationToken);
        if (hasCourses)
            throw new InvalidStateTransitionException("PROGRAM_HAS_COURSES", "Cannot delete a program that contains courses.");

        _db.Programs.Remove(program);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record RestoreProgramCommand(Guid ProgramId) : IRequest<Unit>;

public sealed class RestoreProgramCommandHandler : IRequestHandler<RestoreProgramCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreProgramCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreProgramCommand request, CancellationToken cancellationToken)
    {
        var program = await _db.Programs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.ProgramId, cancellationToken)
            ?? throw new EntityNotFoundException("Program", request.ProgramId);

        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != program.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        await OrgHierarchyAuthorization.EnsureCanManageDepartmentAsync(_db, _currentUser, program.DepartmentId, cancellationToken);

        var department = await _db.Departments.FindAsync([program.DepartmentId], cancellationToken);
        if (department == null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore program because its department is deleted.");

        program.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Shared chain-resolution ownership checks for the Org Hierarchy feature.
/// Every Create/Update command below Department needs to answer "does
/// this resource's ancestor chain lead back to a department/institution
/// the caller is actually allowed to manage" -- this was previously
/// answered inconsistently (mostly: not answered) across Program, Course,
/// and Class commands. Centralized here so the same rule applies
/// everywhere instead of being reimplemented (and potentially
/// re-forgotten) per handler.
/// </summary>
internal static class OrgHierarchyAuthorization
{
    /// <summary>InstitutionAdmin: department must belong to their institution. DepartmentAdmin: department must be their own. PlatformAdmin: unrestricted.</summary>
    public static async Task<Guid> EnsureCanManageDepartmentAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, Guid departmentId, CancellationToken ct)
    {
        var departmentInstitutionId = await db.Departments
            .Where(d => d.Id == departmentId)
            .Select(d => (Guid?)d.InstitutionId)
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Department", departmentId);

        if (currentUser.Role != UserRole.PlatformAdmin)
        {


        }
        
        return departmentInstitutionId;
    }

    /// <summary>Resolves a Program's owning Department, then applies the same rule as above.</summary>
    public static async Task<Guid> EnsureCanManageProgramAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, Guid programId, CancellationToken ct)
    {
        var departmentId = await db.Programs
            .Where(p => p.Id == programId)
            .Select(p => (Guid?)p.DepartmentId)
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Program", programId);

        return await EnsureCanManageDepartmentAsync(db, currentUser, departmentId, ct);
    }

    /// <summary>Resolves a Course's owning Program -> Department, then applies the same rule.</summary>
    public static async Task<Guid> EnsureCanManageCourseAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, Guid courseId, CancellationToken ct)
    {
        var departmentId = await (
            from c in db.Courses
            join p in db.Programs on c.ProgramId equals p.Id
            where c.Id == courseId
            select (Guid?)p.DepartmentId
        ).FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Course", courseId);

        return await EnsureCanManageDepartmentAsync(db, currentUser, departmentId, ct);
    }

    /// <summary>Resolves a Class's owning Course -> Program -> Department, then applies the same rule.</summary>
    public static async Task<Guid> EnsureCanManageClassAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, Guid classId, CancellationToken ct)
    {
        var departmentId = await (
            from cl in db.Classes
            join c in db.Courses on cl.CourseId equals c.Id
            join p in db.Programs on c.ProgramId equals p.Id
            where cl.Id == classId
            select (Guid?)p.DepartmentId
        ).FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Class", classId);

        return await EnsureCanManageDepartmentAsync(db, currentUser, departmentId, ct);
    }
}
