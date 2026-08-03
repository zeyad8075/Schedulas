using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Features.Reports;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Schedulas.Infrastructure.Persistence.Interceptors;

namespace Schedulas.Application.Tests.Features.Reports;

public class ReportsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SchedulasDbContext> _options;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public ReportsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantServiceMock = new Mock<ITenantService>();
        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(true); // Enforce tenant isolation

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.Role).Returns(UserRole.PlatformAdmin);

        _options = new DbContextOptionsBuilder<SchedulasDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new AuditableSaveChangesInterceptor(_currentUserMock.Object))
            .Options;

        using var context = new SchedulasDbContext(_options, _tenantServiceMock.Object);
        context.Database.EnsureCreated();
    }

    private SchedulasDbContext CreateContext() => new(_options, _tenantServiceMock.Object);

    public void Dispose() => _connection.Dispose();

    private async Task<(Guid instId, Guid teacherId, Guid studentId, Guid classId)> SetupDataAsync(SchedulasDbContext context)
    {
        var inst = new Institution("Test Inst", InstitutionType.University, "UTC");
        context.Institutions.Add(inst);
        await context.SaveChangesAsync();

        var term = new AcademicTerm(inst.Id, "Fall", new DateOnly(2026, 9, 1), new DateOnly(2026, 12, 31));
        var dept = new Department(inst.Id, "Science");
        context.AcademicTerms.Add(term);
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var prog = new Program(inst.Id, dept.Id, "Physics");
        context.Programs.Add(prog);
        await context.SaveChangesAsync();

        var course = new Course(inst.Id, prog.Id, "Phy 101", "PHY101");
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var cls = new Class(inst.Id, course.Id, term.Id, "A");
        context.Classes.Add(cls);
        await context.SaveChangesAsync();

        var teacherProfile = new Profile(Guid.NewGuid(), "T1 L", "t1@test.com", UserRole.Teacher, inst.Id, dept.Id);
        var studentProfile = new Profile(Guid.NewGuid(), "S1 L", "s1@test.com", UserRole.Student, inst.Id, dept.Id);
        context.Profiles.AddRange(teacherProfile, studentProfile);
        await context.SaveChangesAsync();

        var teacher = new Teacher(teacherProfile.Id, inst.Id);
        var student = new Student(studentProfile.Id, inst.Id, "S-123");
        context.Teachers.Add(teacher);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        context.ClassTeachers.Add(new ClassTeacher(cls.Id, teacher.Id));
        context.ClassStudents.Add(new ClassStudent(cls.Id, student.Id));
        await context.SaveChangesAsync();

        // Activities
        // Active Assignment, 60 mins
        var a1 = Activity.CreateCandidate(cls.Id, inst.Id, ActivityType.Assignment, "A1", null, new DateOnly(2026, 10, 1), null, null, TimeSpan.FromMinutes(60), 0, null, null);
        a1.MarkApproved();
        
        // Active Exam, 120 mins
        var a2 = Activity.CreateCandidate(cls.Id, inst.Id, ActivityType.Exam, "E1", null, new DateOnly(2026, 10, 2), null, null, TimeSpan.FromMinutes(120), 0, null, null);
        a2.MarkApproved();

        // Cancelled Project, 200 mins (Should not be counted)
        var a3 = Activity.CreateCandidate(cls.Id, inst.Id, ActivityType.Project, "P1", null, new DateOnly(2026, 10, 3), null, null, TimeSpan.FromMinutes(200), 0, null, null);
        a3.MarkApproved();
        a3.Cancel();
        
        context.Activities.AddRange(a1, a2, a3);
        await context.SaveChangesAsync();

        return (inst.Id, teacher.Id, student.Id, cls.Id);
    }

    [Fact]
    public async Task GetInstitutionWorkloadQuery_ShouldAggregateCorrectly()
    {
        // Arrange
        Guid instId;
        using (var setupContext = CreateContext())
        {
            var data = await SetupDataAsync(setupContext);
            instId = data.instId;
        }

        _tenantServiceMock.Setup(t => t.TenantId).Returns(instId);

        using var context = CreateContext();
        var handler = new GetInstitutionWorkloadQueryHandler(context, _tenantServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetInstitutionWorkloadQuery(instId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalWorkloadMinutes.Should().Be(180); // 60 + 120 (Cancelled excluded)
    }

    [Fact]
    public async Task GetTeacherWorkloadQuery_ShouldAggregateCorrectly()
    {
        // Arrange
        Guid instId, teacherId;
        using (var setupContext = CreateContext())
        {
            var data = await SetupDataAsync(setupContext);
            instId = data.instId;
            teacherId = data.teacherId;
        }

        _tenantServiceMock.Setup(t => t.TenantId).Returns(instId);

        using var context = CreateContext();
        var handler = new GetTeacherWorkloadQueryHandler(context, _tenantServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetTeacherWorkloadQuery(instId, teacherId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalWorkloadMinutes.Should().Be(180); 
    }

    [Fact]
    public async Task GetStudentWorkloadQuery_ShouldAggregateCorrectly()
    {
        // Arrange
        Guid instId, studentId;
        using (var setupContext = CreateContext())
        {
            var data = await SetupDataAsync(setupContext);
            instId = data.instId;
            studentId = data.studentId;
        }

        _tenantServiceMock.Setup(t => t.TenantId).Returns(instId);

        using var context = CreateContext();
        var handler = new GetStudentWorkloadQueryHandler(context, _tenantServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetStudentWorkloadQuery(instId, studentId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalWorkloadMinutes.Should().Be(180); 
    }

    [Fact]
    public async Task GetActivityDistributionQuery_ShouldGroupCorrectly()
    {
        // Arrange
        Guid instId, classId;
        using (var setupContext = CreateContext())
        {
            var data = await SetupDataAsync(setupContext);
            instId = data.instId;
            classId = data.classId;
        }

        _tenantServiceMock.Setup(t => t.TenantId).Returns(instId);

        using var context = CreateContext();
        var handler = new GetActivityDistributionQueryHandler(context, _tenantServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetActivityDistributionQuery(instId, classId, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Distribution.Should().HaveCount(2); // Assignment, Exam (Project was cancelled)
        result.Distribution.Should().Contain(d => d.ActivityType == ActivityType.Assignment && d.Count == 1);
        result.Distribution.Should().Contain(d => d.ActivityType == ActivityType.Exam && d.Count == 1);
    }
}
