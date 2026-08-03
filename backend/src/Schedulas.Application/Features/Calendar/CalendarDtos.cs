using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.Calendar;

public sealed record CalendarEntryDto
{
    public Guid ActivityId { get; init; }
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public TimeSpan? Duration { get; init; }
    public ActivityStatus Status { get; init; }
    public int Priority { get; init; }
    public ActivityType ActivityType { get; init; }
    
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = default!;
    public string CourseName { get; init; } = default!;
    
    // Metadata derived
    public string? TeacherId { get; init; }
    public string? TeacherName { get; init; }
    public string? RoomId { get; init; }
    public string? RoomName { get; init; }
    
    // Derived UI Color
    public string ColorHex { get; init; } = default!;
}

public sealed record DailyCalendarDto
{
    public DateOnly Date { get; init; }
    public IReadOnlyList<CalendarEntryDto> Entries { get; init; } = [];
}

public sealed record WeeklyCalendarDto
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public IReadOnlyList<DailyCalendarDto> Days { get; init; } = [];
}

public sealed record MonthlyCalendarDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public IReadOnlyList<WeeklyCalendarDto> Weeks { get; init; } = [];
}
