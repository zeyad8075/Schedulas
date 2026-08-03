using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.OrgHierarchy.Queries;

/// <summary>
/// Backs GET /api/v1/classes/mine — the entry point the Flutter Create
/// Activity flow needs (a Teacher must know which of their own classes
/// they're scheduling an activity for) and which did not previously
/// exist as a real endpoint. Built to close that documented gap rather
/// than have the client fake it.
/// </summary>
/// <summary>The subset of a Class's data a Teacher needs to pick which class they're scheduling an activity for.</summary>
/// <param name="ClassId">The Class's id — pass this as CreateActivityCommand.ClassId.</param>
/// <param name="ClassName">Display name of the class/section (e.g. "Section A").</param>
/// <param name="CourseId">The owning Course's id, for display grouping.</param>
/// <param name="CourseName">The owning Course's display name.</param>
/// <param name="AcademicTermId">The Academic Term this class belongs to.</param>
/// <param name="AcademicTermName">The Academic Term's display name (Arabic).</param>
public sealed record MyClassDto(
    Guid ClassId,
    string ClassName,
    Guid CourseId,
    string CourseName,
    Guid AcademicTermId,
    string AcademicTermName);

/// <summary>
/// No TeacherId parameter by design — the teacher's identity is resolved
/// exclusively from ICurrentUserService (i.e. the validated Supabase
/// JWT), never accepted from the client. This is the same pattern
/// UpdateInstitutionSettingsCommand/UpdateUserSettingsCommand already use
/// (Phase 8 Verification pass): when a request is inherently "give me
/// MY OWN X," the safest design isn't validating a caller-supplied id
/// against the caller — it's not exposing an id parameter to spoof at all.
/// </summary>
public sealed record GetMyTaughtClassesQuery : IRequest<IReadOnlyList<MyClassDto>>;

/// <summary>
/// Tenant isolation is structural here, not an extra check bolted on:
/// the query only ever joins through ClassTeachers rows that already
/// belong to the resolved Teacher, and every ClassTeachers row was
/// itself created by AssignTeacherCommand, which (per the Write-Side
/// Ownership Audit) already enforces that a Teacher can only be assigned
/// to a Class within their own institution. A teacher literally cannot
/// have a ClassTeachers row pointing at another institution's class, so
/// there's nothing further to filter by institution here — but the role
/// check (only a Teacher profile can call this meaningfully) and the
/// "resolve from JWT, never from a parameter" rule are the two things
/// this handler must never regress on.
/// </summary>
public sealed class GetMyTaughtClassesQueryHandler : IRequestHandler<GetMyTaughtClassesQuery, IReadOnlyList<MyClassDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyTaughtClassesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MyClassDto>> Handle(GetMyTaughtClassesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.Teacher)
            throw new UnauthorizedAccessException("NOT_A_TEACHER_PROFILE");

        if (_currentUser.UserId is not Guid profileId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var teacherId = await _db.Teachers
            .Where(t => t.ProfileId == profileId)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            throw new UnauthorizedAccessException("NOT_A_TEACHER_PROFILE");

        var classes = await (
            from ct in _db.ClassTeachers
            join cl in _db.Classes on ct.ClassId equals cl.Id
            join co in _db.Courses on cl.CourseId equals co.Id
            join term in _db.AcademicTerms on cl.AcademicTermId equals term.Id
            where ct.TeacherId == teacherId.Value && cl.IsActive
            orderby co.Name, cl.Name
            select new MyClassDto(cl.Id, cl.Name, co.Id, co.Name, term.Id, term.Name)
        ).ToListAsync(cancellationToken);

        return classes;
    }
}
