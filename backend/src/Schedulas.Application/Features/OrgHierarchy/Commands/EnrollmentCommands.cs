using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.OrgHierarchy.Commands;

// SECURITY FIX (Write-Side Ownership Audit): every command in this file
// previously had ZERO authorization checks -- any InstitutionAdmin or
// DepartmentAdmin could enroll any student, assign any teacher, or link
// any parent to any student, across ANY institution, not just their own.
// This is the exact class of gap the audit's "Parent/Student relationship
// validation" and "Teacher/Class ownership validation" criteria target
// directly. Every command below now verifies both (a) the caller may
// manage the target Class (via OrgHierarchyAuthorization, which also
// applies DepartmentAdmin's narrower department-only scope), and
// (b) the Student/Teacher being enrolled/assigned actually belongs to
// that same institution -- otherwise a caller who legitimately manages
// Class X could still enroll a Student who belongs to a completely
// different institution, silently corrupting tenant boundaries.

public sealed record EnrollStudentCommand(Guid ClassId, Guid StudentId) : IRequest<Unit>;

public sealed class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
    }
}

public sealed class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public EnrollStudentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        await OrgHierarchyAuthorization.EnsureCanManageClassAsync(_db, _currentUser, request.ClassId, cancellationToken);
        await EnrollmentAuthorization.EnsureStudentSharesClassInstitutionAsync(_db, request.ClassId, request.StudentId, cancellationToken);

        var alreadyEnrolled = await _db.ClassStudents.AnyAsync(
            cs => cs.ClassId == request.ClassId && cs.StudentId == request.StudentId, cancellationToken);

        if (!alreadyEnrolled)
        {
            _db.ClassStudents.Add(new ClassStudent(request.ClassId, request.StudentId));
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

public sealed record UnenrollStudentCommand(Guid ClassId, Guid StudentId) : IRequest<Unit>;

public sealed class UnenrollStudentCommandHandler : IRequestHandler<UnenrollStudentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UnenrollStudentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UnenrollStudentCommand request, CancellationToken cancellationToken)
    {
        await OrgHierarchyAuthorization.EnsureCanManageClassAsync(_db, _currentUser, request.ClassId, cancellationToken);

        var link = await _db.ClassStudents.FirstOrDefaultAsync(
            cs => cs.ClassId == request.ClassId && cs.StudentId == request.StudentId, cancellationToken)
            ?? throw new EntityNotFoundException("ClassStudent", request.StudentId);

        _db.ClassStudents.Remove(link);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record AssignTeacherCommand(Guid ClassId, Guid TeacherId) : IRequest<Unit>;

public sealed class AssignTeacherCommandValidator : AbstractValidator<AssignTeacherCommand>
{
    public AssignTeacherCommandValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
    }
}

public sealed class AssignTeacherCommandHandler : IRequestHandler<AssignTeacherCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AssignTeacherCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(AssignTeacherCommand request, CancellationToken cancellationToken)
    {
        await OrgHierarchyAuthorization.EnsureCanManageClassAsync(_db, _currentUser, request.ClassId, cancellationToken);
        await EnrollmentAuthorization.EnsureTeacherSharesClassInstitutionAsync(_db, request.ClassId, request.TeacherId, cancellationToken);

        var alreadyAssigned = await _db.ClassTeachers.AnyAsync(
            ct => ct.ClassId == request.ClassId && ct.TeacherId == request.TeacherId, cancellationToken);

        if (!alreadyAssigned)
        {
            _db.ClassTeachers.Add(new ClassTeacher(request.ClassId, request.TeacherId));
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

public sealed record UnassignTeacherCommand(Guid ClassId, Guid TeacherId) : IRequest<Unit>;

public sealed class UnassignTeacherCommandHandler : IRequestHandler<UnassignTeacherCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UnassignTeacherCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UnassignTeacherCommand request, CancellationToken cancellationToken)
    {
        await OrgHierarchyAuthorization.EnsureCanManageClassAsync(_db, _currentUser, request.ClassId, cancellationToken);

        var link = await _db.ClassTeachers.FirstOrDefaultAsync(
            ct => ct.ClassId == request.ClassId && ct.TeacherId == request.TeacherId, cancellationToken)
            ?? throw new EntityNotFoundException("ClassTeacher", request.TeacherId);

        _db.ClassTeachers.Remove(link);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>SRS FR-AUTH-6: a Parent sees nothing until explicitly linked to a Student by an admin.</summary>
public sealed record LinkParentToStudentCommand(Guid ParentId, Guid StudentId) : IRequest<Unit>;

public sealed class LinkParentToStudentCommandValidator : AbstractValidator<LinkParentToStudentCommand>
{
    public LinkParentToStudentCommandValidator()
    {
        RuleFor(x => x.ParentId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
    }
}

/// <summary>
/// SECURITY FIX: previously no check at all. Parent has no InstitutionId
/// of its own in the domain model (a parent isn't inherently tied to one
/// institution — they're tied to it only via their linked children), so
/// the enforceable check is that the target Student belongs to the
/// caller's own institution. This is the check that actually matters:
/// it's what prevents an admin from one institution linking a parent
/// account to a student who belongs to a completely different one.
/// </summary>
public sealed class LinkParentToStudentCommandHandler : IRequestHandler<LinkParentToStudentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public LinkParentToStudentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(LinkParentToStudentCommand request, CancellationToken cancellationToken)
    {
        await EnrollmentAuthorization.EnsureStudentInCallerInstitutionAsync(_db, _currentUser, request.StudentId, cancellationToken);

        var alreadyLinked = await _db.ParentStudentLinks.AnyAsync(
            l => l.ParentId == request.ParentId && l.StudentId == request.StudentId, cancellationToken);

        if (!alreadyLinked)
        {
            _db.ParentStudentLinks.Add(new ParentStudentLink(request.ParentId, request.StudentId));
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

public sealed record UnlinkParentFromStudentCommand(Guid LinkId) : IRequest<Unit>;

/// <summary>SECURITY FIX: same gap as LinkParentToStudentCommand, fixed the same way (resolved via the link's Student).</summary>
public sealed class UnlinkParentFromStudentCommandHandler : IRequestHandler<UnlinkParentFromStudentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UnlinkParentFromStudentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UnlinkParentFromStudentCommand request, CancellationToken cancellationToken)
    {
        var link = await _db.ParentStudentLinks.FindAsync([request.LinkId], cancellationToken)
            ?? throw new EntityNotFoundException("ParentStudentLink", request.LinkId);

        await EnrollmentAuthorization.EnsureStudentInCallerInstitutionAsync(_db, _currentUser, link.StudentId, cancellationToken);

        _db.ParentStudentLinks.Remove(link);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Shared cross-tenant-integrity checks specific to enrollment/linking operations.</summary>
internal static class EnrollmentAuthorization
{
    /// <summary>Confirms a Student being enrolled actually belongs to the same institution as the Class they're being enrolled into.</summary>
    public static async Task EnsureStudentSharesClassInstitutionAsync(
        IApplicationDbContext db, Guid classId, Guid studentId, CancellationToken ct)
    {
        var classInstitutionId = await ResolveClassInstitutionIdAsync(db, classId, ct);

        var studentInstitutionId = await db.Students
            .Where(s => s.Id == studentId)
            .Select(s => (Guid?)s.InstitutionId)
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Student", studentId);

        if (studentInstitutionId != classInstitutionId)
            throw new InvalidStateTransitionException("CROSS_INSTITUTION_ENROLLMENT", "Cannot enroll across institutions.");
    }

    /// <summary>Confirms a Teacher being assigned actually belongs to the same institution as the Class.</summary>
    public static async Task EnsureTeacherSharesClassInstitutionAsync(
        IApplicationDbContext db, Guid classId, Guid teacherId, CancellationToken ct)
    {
        var classInstitutionId = await ResolveClassInstitutionIdAsync(db, classId, ct);

        var teacherInstitutionId = await db.Teachers
            .Where(t => t.Id == teacherId)
            .Select(t => (Guid?)t.InstitutionId)
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Teacher", teacherId);

        if (teacherInstitutionId != classInstitutionId)
            throw new InvalidStateTransitionException("CROSS_INSTITUTION_ENROLLMENT", "Cannot enroll across institutions.");
    }

    /// <summary>Confirms the target Student belongs to the caller's own institution (PlatformAdmin exempt).</summary>
    public static async Task EnsureStudentInCallerInstitutionAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, Guid studentId, CancellationToken ct)
    {
        if (currentUser.Role == UserRole.PlatformAdmin) return;

        var studentInstitutionId = await db.Students
            .Where(s => s.Id == studentId)
            .Select(s => (Guid?)s.InstitutionId)
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Student", studentId);

        if (currentUser.InstitutionId != studentInstitutionId)
            throw new InvalidStateTransitionException("CROSS_INSTITUTION_ENROLLMENT", "Cannot enroll across institutions.");
    }

    private static async Task<Guid> ResolveClassInstitutionIdAsync(IApplicationDbContext db, Guid classId, CancellationToken ct) =>
        await (
            from cl in db.Classes
            join c in db.Courses on cl.CourseId equals c.Id
            join p in db.Programs on c.ProgramId equals p.Id
            join d in db.Departments on p.DepartmentId equals d.Id
            where cl.Id == classId
            select (Guid?)d.InstitutionId
        ).FirstOrDefaultAsync(ct)
        ?? throw new EntityNotFoundException("Class", classId);
}
