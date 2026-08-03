using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.Activities.Commands;

/// <summary>
/// Shared ownership checks for Create/Edit/Override Activity commands.
/// Extracted so the same two questions -- "does this Class actually
/// belong to the claimed institution" and "is this specific caller
/// actually allowed to touch this specific Class" -- are answered
/// identically everywhere an Activity is written, rather than answered
/// inconsistently (or, before the Write-Side Ownership Audit, not
/// answered at all).
/// </summary>
internal static class ActivityAuthorization
{
    /// <summary>
    /// Prevents a caller from claiming their own InstitutionId while
    /// targeting a ClassId that actually belongs to a different
    /// institution -- which would otherwise silently inject an activity
    /// into another institution's class while mislabeling it under the
    /// caller's own institution for every InstitutionId-scoped query.
    /// </summary>
    public static async Task EnsureClassBelongsToInstitutionAsync(
        IApplicationDbContext db, Guid classId, Guid claimedInstitutionId, CancellationToken ct)
    {
        var actualInstitutionId = await (
            from cl in db.Classes
            join c in db.Courses on cl.CourseId equals c.Id
            join p in db.Programs on c.ProgramId equals p.Id
            join d in db.Departments on p.DepartmentId equals d.Id
            where cl.Id == classId
            select (Guid?)d.InstitutionId
        ).FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException("Class", classId);

        if (actualInstitutionId != claimedInstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");
    }

    /// <summary>
    /// Teacher: must actually be assigned to teach this Class (SRS
    /// FR-ACT-1's stated intent — a Teacher creates activities for
    /// classes they teach, not any class in the institution).
    /// DepartmentAdmin: the class's department must be their own.
    /// InstitutionAdmin/PlatformAdmin: no additional restriction beyond
    /// the institution check already applied by the caller.
    /// </summary>
    public static async Task EnsureCanCreateForClassAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, Guid classId, CancellationToken ct)
    {
        if (currentUser.Role == UserRole.Teacher)
        {
            if (currentUser.UserId is not Guid profileId)
                throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

            var teachesClass = await (
                from ct2 in db.ClassTeachers
                join t in db.Teachers on ct2.TeacherId equals t.Id
                where ct2.ClassId == classId && t.ProfileId == profileId
                select ct2.Id
            ).AnyAsync(ct);

            if (!teachesClass)
                throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

            return;
        }

        if (currentUser.Role == UserRole.DepartmentAdmin)
        {
            var classDepartmentId = await (
                from cl in db.Classes
                join c in db.Courses on cl.CourseId equals c.Id
                join p in db.Programs on c.ProgramId equals p.Id
                where cl.Id == classId
                select (Guid?)p.DepartmentId
            ).FirstOrDefaultAsync(ct)
                ?? throw new EntityNotFoundException("Class", classId);

            if (currentUser.DepartmentId != classDepartmentId)
                throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");
        }

        // InstitutionAdmin / PlatformAdmin: institution-level check already covers them.
    }
}
