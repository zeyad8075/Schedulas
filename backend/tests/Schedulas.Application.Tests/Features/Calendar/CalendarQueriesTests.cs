using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Features.Calendar.Queries;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.Application.Tests.Features.Calendar;

public class CalendarQueriesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SchedulasDbContext> _options;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public CalendarQueriesTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantServiceMock = new Mock<ITenantService>();
        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false); 

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.Role).Returns(UserRole.PlatformAdmin);

        _options = new DbContextOptionsBuilder<SchedulasDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new SchedulasDbContext(_options, _tenantServiceMock.Object);
        context.Database.EnsureCreated();
    }

    private SchedulasDbContext CreateContext()
    {
        return new SchedulasDbContext(_options, _tenantServiceMock.Object);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private async Task<(Guid instId, Guid classId)> SetupHierarchyAsync(SchedulasDbContext context)
    {
        var inst = new Institution("Test Inst", InstitutionType.School, "UTC");
        context.Institutions.Add(inst);

        var dept = new Department(inst.Id, "Dept");
        context.Departments.Add(dept);

        var prog = new Schedulas.Domain.Entities.Program(inst.Id, dept.Id, "Prog");
        context.Programs.Add(prog);

        var course = new Course(inst.Id, prog.Id, "Course", null);
        context.Courses.Add(course);

        var term = new AcademicTerm(inst.Id, "Term", DateOnly.MinValue, DateOnly.MaxValue);
        context.AcademicTerms.Add(term);

        var cls = new Class(inst.Id, course.Id, term.Id, "Class 1A");
        context.Classes.Add(cls);

        await context.SaveChangesAsync();

        return (inst.Id, cls.Id);
    }

    [Fact]
    public async Task GetDailyCalendar_ShouldReturnActivities_ForSpecificDate()
    {
        // Arrange
        var date = DateOnly.Parse("2026-10-15");
        Guid instId, classId;

        using (var setupContext = CreateContext())
        {
            (instId, classId) = await SetupHierarchyAsync(setupContext);

            var activity1 = Activity.CreateCandidate(classId, instId, ActivityType.Assignment, "Math HW", null, date, new TimeOnly(9, 0), new TimeOnly(10, 0), TimeSpan.FromHours(1), 1, 1m, "{\"teacherId\":\"T1\"}");
            var activity2 = Activity.CreateCandidate(classId, instId, ActivityType.Exam, "Math Exam", null, date, new TimeOnly(11, 0), new TimeOnly(12, 0), TimeSpan.FromHours(1), 2, 5m, "{}");
            
            // different date
            var activity3 = Activity.CreateCandidate(classId, instId, ActivityType.Event, "Off day", null, date.AddDays(1), null, null, null, 1, 1m, "{}");
            
            setupContext.Activities.AddRange(activity1, activity2, activity3);
            await setupContext.SaveChangesAsync();
        }

        using var context = CreateContext();
        var handler = new GetDailyCalendarQueryHandler(context, _currentUserMock.Object, _tenantServiceMock.Object);
        var query = new GetDailyCalendarQuery(instId, date);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Date.Should().Be(date);
        result.Entries.Should().HaveCount(2);
        result.Entries[0].Title.Should().Be("Math HW");
        result.Entries[1].Title.Should().Be("Math Exam");
    }

    [Fact]
    public async Task GetWeeklyCalendar_ShouldReturnCorrectBoundaries()
    {
        // Arrange
        // Oct 15 2026 is Thursday. Week is Oct 12 (Mon) to Oct 18 (Sun).
        var date = DateOnly.Parse("2026-10-15");
        Guid instId, classId;

        using (var setupContext = CreateContext())
        {
            (instId, classId) = await SetupHierarchyAsync(setupContext);

            setupContext.Activities.Add(Activity.CreateCandidate(classId, instId, ActivityType.Assignment, "Inside 1", null, DateOnly.Parse("2026-10-12"), null, null, null, 1, 1m, "{}"));
            setupContext.Activities.Add(Activity.CreateCandidate(classId, instId, ActivityType.Assignment, "Inside 2", null, DateOnly.Parse("2026-10-18"), null, null, null, 1, 1m, "{}"));
            setupContext.Activities.Add(Activity.CreateCandidate(classId, instId, ActivityType.Assignment, "Outside Before", null, DateOnly.Parse("2026-10-11"), null, null, null, 1, 1m, "{}"));
            setupContext.Activities.Add(Activity.CreateCandidate(classId, instId, ActivityType.Assignment, "Outside After", null, DateOnly.Parse("2026-10-19"), null, null, null, 1, 1m, "{}"));
            
            await setupContext.SaveChangesAsync();
        }

        using var context = CreateContext();
        var handler = new GetWeeklyCalendarQueryHandler(context, _currentUserMock.Object, _tenantServiceMock.Object);
        var query = new GetWeeklyCalendarQuery(instId, date);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.StartDate.Should().Be(DateOnly.Parse("2026-10-12"));
        result.EndDate.Should().Be(DateOnly.Parse("2026-10-18"));
        result.Days.Should().HaveCount(7);
        result.Days[0].Entries.Should().ContainSingle(e => e.Title == "Inside 1");
        result.Days[6].Entries.Should().ContainSingle(e => e.Title == "Inside 2");
    }
}
