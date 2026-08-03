using Schedulas.Application.Common.Behaviors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using System.Text.Json;

namespace Schedulas.Application.Features.Calendar.Queries;

public sealed record GetWeeklyCalendarQuery(
    Guid InstitutionId,
    DateOnly Date, // any date in the week
    Guid? TeacherId = null,
    Guid? StudentId = null,
    Guid? ClassId = null
) : IRequest<WeeklyCalendarDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetWeeklyCalendarQueryHandler : IRequestHandler<GetWeeklyCalendarQuery, WeeklyCalendarDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenantService;

    public GetWeeklyCalendarQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ITenantService tenantService)
    {
        _db = db;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<WeeklyCalendarDto> Handle(GetWeeklyCalendarQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.PlatformAdmin)
            _tenantService.SetTenantId(request.InstitutionId);

        var diff = (7 + (int)request.Date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        var startDate = request.Date.AddDays(-diff);
        var endDate = startDate.AddDays(6);

        var query = _db.Activities
            .AsNoTracking()
            .Where(a => a.InstitutionId == request.InstitutionId && a.ScheduledDate >= startDate && a.ScheduledDate <= endDate);

        if (request.ClassId.HasValue)
        {
            query = query.Where(a => a.ClassId == request.ClassId.Value);
        }

        if (request.StudentId.HasValue)
        {
            var studentClassIds = _db.ClassStudents
                .Where(e => e.StudentId == request.StudentId.Value)
                .Select(e => e.ClassId);
            query = query.Where(a => studentClassIds.Contains(a.ClassId));
        }

        var entries = await query
            .Join(_db.Classes, a => a.ClassId, c => c.Id, (a, c) => new { Activity = a, Class = c })
            .Join(_db.Courses, ac => ac.Class.CourseId, co => co.Id, (ac, co) => new
            {
                Activity = ac.Activity,
                Class = ac.Class,
                Course = co
            })
            .ToListAsync(cancellationToken);

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

        var days = new List<DailyCalendarDto>();
        for (int i = 0; i < 7; i++)
        {
            var currentDay = startDate.AddDays(i);
            days.Add(new DailyCalendarDto
            {
                Date = currentDay,
                Entries = resultEntries.Where(e => e.Date == currentDay).OrderBy(e => e.StartTime).ThenBy(e => e.Priority).ToList()
            });
        }

        return new WeeklyCalendarDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Days = days
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
        if (status == ActivityStatus.Cancelled) return "#9E9E9E"; 
        if (status == ActivityStatus.RequiresOverrideApproved) return "#FF9800"; 
        
        return type switch
        {
            ActivityType.Exam => "#F44336", 
            ActivityType.Assignment => "#2196F3", 
            ActivityType.Project => "#4CAF50", 
            ActivityType.Presentation => "#9C27B0", 
            ActivityType.Event => "#00BCD4", 
            _ => "#607D8B" 
        };
    }
}
