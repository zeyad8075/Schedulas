using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.Activities.Queries;

/// <summary>
/// Backs GET /api/v1/activities (API Design §8) and the Calendar module's
/// GET /api/v1/calendar (§10), which is a thin filter-preset over this same
/// query rather than a separate data model (Architecture §5 rationale,
/// Database Design ER note — Calendar is a projection, not its own table).
/// </summary>
public sealed record GetActivitiesQuery(
    Guid InstitutionId,
    Guid? ClassId,
    ActivityType? Type,
    ActivityStatus? Status,
    string? SearchTerm,
    string? SortBy,
    bool SortDescending,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PaginatedList<ActivityDto>>;

/// <summary>
/// SECURITY FIX (Phase 8 Verification pass): previously trusted the
/// caller-supplied InstitutionId with no check it matched the caller's own
/// institution, and applied no per-role sub-scoping at all -- meaning SRS
/// FR-ACT-6 ("Students/Parents see only their enrolled classes/children;
/// Teachers see their own classes") was stated as a requirement but never
/// actually implemented; every authenticated role saw every activity in
/// the institution. Both are fixed here.
/// </summary>
public sealed class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, PaginatedList<ActivityDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenantService;

    public GetActivitiesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ITenantService tenantService)
    {
        _db = db;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<PaginatedList<ActivityDto>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {

        if (_currentUser.Role == UserRole.PlatformAdmin)
            _tenantService.SetTenantId(request.InstitutionId);

        var query = _db.Activities.AsQueryable();

        if (request.Status is ActivityStatus status)
            query = query.Where(a => a.Status == status);
        else
            query = query.Where(a => a.Status != ActivityStatus.Cancelled);

        var allowedClassIds = await ActivityAuthorization.ResolveVisibleClassIdsAsync(_db, _currentUser, cancellationToken);
        if (allowedClassIds is not null)
            query = query.Where(a => allowedClassIds.Contains(a.ClassId));

        if (request.ClassId is Guid classId)
            query = query.Where(a => a.ClassId == classId);

        if (request.Type is ActivityType type)
            query = query.Where(a => a.ActivityType == type);

        if (request.DateFrom is DateOnly from)
            query = query.Where(a => a.ScheduledDate >= from);

        if (request.DateTo is DateOnly to)
            query = query.Where(a => a.ScheduledDate <= to);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(a => a.Title.Contains(request.SearchTerm) || (a.Description != null && a.Description.Contains(request.SearchTerm)));

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "type" => request.SortDescending ? query.OrderByDescending(a => a.ActivityType) : query.OrderBy(a => a.ActivityType),
            "date" => request.SortDescending ? query.OrderByDescending(a => a.ScheduledDate) : query.OrderBy(a => a.ScheduledDate),
            _ => request.SortDescending ? query.OrderByDescending(a => a.ScheduledDate) : query.OrderBy(a => a.ScheduledDate)
        };

        var projected = query
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                ClassId = a.ClassId,
                ActivityType = a.ActivityType,
                Title = a.Title,
                Description = a.Description,
                ScheduledDate = a.ScheduledDate,
                ScheduledTime = a.ScheduledTime,
                EstimatedWeight = a.EstimatedWeight,
                Status = a.Status,
                MetadataJson = a.MetadataJson
            });

        return await PaginatedList<ActivityDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public sealed record GetActivityByIdQuery(Guid ActivityId) : IRequest<ActivityDto>;

/// <summary>
/// SECURITY FIX: previously had NO scoping at all -- any authenticated
/// user from any institution could fetch any activity by id. Now checks
/// institution membership and, for Student/Parent/Teacher, that the
/// activity's class is actually one they're allowed to see.
/// </summary>
public sealed class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetActivityByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ActivityDto> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var activity = await _db.Activities
            .FirstOrDefaultAsync(a => a.Id == request.ActivityId, cancellationToken)
            ?? throw new EntityNotFoundException("Activity", request.ActivityId);

        if (_currentUser.Role != UserRole.PlatformAdmin)
        {

            var allowedClassIds = await ActivityAuthorization.ResolveVisibleClassIdsAsync(_db, _currentUser, cancellationToken);
            if (allowedClassIds is not null && !allowedClassIds.Contains(activity.ClassId))
                throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");
        }

        return new ActivityDto
        {
            Id = activity.Id,
            ClassId = activity.ClassId,
            ActivityType = activity.ActivityType,
            Title = activity.Title,
            Description = activity.Description,
            ScheduledDate = activity.ScheduledDate,
            ScheduledTime = activity.ScheduledTime,
            EstimatedWeight = activity.EstimatedWeight,
            Status = activity.Status,
            MetadataJson = activity.MetadataJson
        };
    }
}

/// <summary>
/// Resolves which ClassIds a caller is allowed to see activities for, per
/// SRS FR-ACT-6. Returns null for roles with institution-wide visibility
/// (DepartmentAdmin, InstitutionAdmin, PlatformAdmin) -- meaning "no
/// further restriction beyond the institution check already applied" --
/// and a concrete (possibly empty) set of ClassIds for Student/Parent/
/// Teacher, whose visibility is narrower than the whole institution.
/// </summary>
internal static class ActivityAuthorization
{
    public static async Task<HashSet<Guid>?> ResolveVisibleClassIdsAsync(
        IApplicationDbContext db, ICurrentUserService currentUser, CancellationToken ct)
    {
        if (currentUser.Role is UserRole.PlatformAdmin or UserRole.InstitutionAdmin or UserRole.DepartmentAdmin)
            return null;

        if (currentUser.UserId is not Guid profileId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        switch (currentUser.Role)
        {
            case UserRole.Student:
            {
                var studentId = await db.Students.Where(s => s.ProfileId == profileId).Select(s => s.Id).FirstOrDefaultAsync(ct);
                return (await db.ClassStudents.Where(cs => cs.StudentId == studentId).Select(cs => cs.ClassId).ToListAsync(ct)).ToHashSet();
            }
            case UserRole.Teacher:
            {
                var teacherId = await db.Teachers.Where(t => t.ProfileId == profileId).Select(t => t.Id).FirstOrDefaultAsync(ct);
                return (await db.ClassTeachers.Where(ct2 => ct2.TeacherId == teacherId).Select(ct2 => ct2.ClassId).ToListAsync(ct)).ToHashSet();
            }
            case UserRole.Parent:
            {
                var parentId = await db.Parents.Where(p => p.ProfileId == profileId).Select(p => p.Id).FirstOrDefaultAsync(ct);
                var linkedStudentIds = db.ParentStudentLinks.Where(l => l.ParentId == parentId).Select(l => l.StudentId);
                return (await db.ClassStudents.Where(cs => linkedStudentIds.Contains(cs.StudentId)).Select(cs => cs.ClassId).ToListAsync(ct)).ToHashSet();
            }
            default:
                throw new UnauthorizedAccessException("NOT_AUTHENTICATED");
        }
    }
}
