using Schedulas.Application.Common.Behaviors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using System.Text.Json;

namespace Schedulas.Application.Features.Calendar.Queries;

public sealed record GetDailyCalendarQuery(
    Guid InstitutionId,
    DateOnly Date,
    Guid? TeacherId = null,
    Guid? StudentId = null,
    Guid? ClassId = null
) : IRequest<DailyCalendarDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetDailyCalendarQueryHandler : IRequestHandler<GetDailyCalendarQuery, DailyCalendarDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenantService;

    public GetDailyCalendarQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ITenantService tenantService)
    {
        _db = db;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<DailyCalendarDto> Handle(GetDailyCalendarQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.PlatformAdmin)
            _tenantService.SetTenantId(request.InstitutionId);

        var query = _db.Activities
            .AsNoTracking()
            .Where(a => a.InstitutionId == request.InstitutionId && a.ScheduledDate == request.Date);

        if (request.ClassId.HasValue)
        {
            query = query.Where(a => a.ClassId == request.ClassId.Value);
        }

        // Student scoping
        if (request.StudentId.HasValue)
        {
            var studentClassIds = _db.ClassStudents
                .Where(e => e.StudentId == request.StudentId.Value)
                .Select(e => e.ClassId);
            query = query.Where(a => studentClassIds.Contains(a.ClassId));
        }

        // Project directly to DTO (EF core translates this effectively)
        var entries = await query
            .Join(_db.Classes, a => a.ClassId, c => c.Id, (a, c) => new { Activity = a, Class = c })
            .Join(_db.Courses, ac => ac.Class.CourseId, co => co.Id, (ac, co) => new
            {
                Activity = ac.Activity,
                Class = ac.Class,
                Course = co
            })
            .ToListAsync(cancellationToken);

        // Perform final projection in memory due to metadata JSON parsing
        var resultEntries = entries.Select(x => 
        {
            var teacherId = GetMetadataString(x.Activity.MetadataJson, "teacherId");
            var roomId = GetMetadataString(x.Activity.MetadataJson, "roomId");

            return new CalendarEntryDto
            {
                ActivityId = x.Activity.Id,
                Title = x.Activity.Title,
                Description = x.Activity.Description,
                Date = x.Activity.ScheduledDate,
                StartTime = x.Activity.ScheduledTime,
                EndTime = x.Activity.EndTime,
                Duration = x.Activity.Duration,
                Status = x.Activity.Status,
                Priority = x.Activity.Priority,
                ActivityType = x.Activity.ActivityType,
                ClassId = x.Activity.ClassId,
                ClassName = x.Class?.Name ?? string.Empty,
                CourseName = x.Course?.Name ?? string.Empty,
                TeacherId = teacherId,
                RoomId = roomId,
                ColorHex = GetColorForActivity(x.Activity.ActivityType, x.Activity.Status)
            };
        }).ToList();

        if (request.TeacherId.HasValue)
        {
            var teacherStr = request.TeacherId.Value.ToString();
            resultEntries = resultEntries.Where(e => e.TeacherId == teacherStr).ToList();
        }

        return new DailyCalendarDto
        {
            Date = request.Date,
            Entries = resultEntries.OrderBy(e => e.StartTime).ThenBy(e => e.Priority).ToList()
        };
    }

    private static string? GetMetadataString(string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try 
        {
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(key, out var prop) ? prop.GetString() : null;
        }
        catch { return null; }
    }

    private static string GetColorForActivity(ActivityType type, ActivityStatus status)
    {
        if (status == ActivityStatus.Cancelled) return "#9E9E9E"; // Grey
        if (status == ActivityStatus.RequiresOverrideApproved) return "#FF9800"; // Orange
        
        return type switch
        {
            ActivityType.Exam => "#F44336", // Red
            ActivityType.Assignment => "#2196F3", // Blue
            ActivityType.Project => "#4CAF50", // Green
            ActivityType.Presentation => "#9C27B0", // Purple
            ActivityType.Event => "#00BCD4", // Cyan
            _ => "#607D8B" // BlueGrey
        };
    }
}
