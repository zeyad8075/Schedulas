using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Features.Notifications;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Events;
using Schedulas.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Infrastructure.Persistence.Interceptors;
using MockQueryable.Moq;

namespace Schedulas.Application.Tests.Features.Notifications;

public class NotificationsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SchedulasDbContext> _options;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IPushNotificationService> _pushServiceMock;
    private readonly Mock<IMediator> _mediatorMock;

    public NotificationsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantServiceMock = new Mock<ITenantService>();
        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false);

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.Role).Returns(UserRole.PlatformAdmin);

        _pushServiceMock = new Mock<IPushNotificationService>();
        
        _mediatorMock = new Mock<IMediator>();

        var dateTimeMock = new Mock<IDateTimeProvider>();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTimeOffset.UtcNow);

        _options = new DbContextOptionsBuilder<SchedulasDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new AuditableSaveChangesInterceptor(_currentUserMock.Object))
            .Options;

        using var context = new TestDbContext(_options, _tenantServiceMock.Object);
        context.Database.EnsureCreated();
    }

    private class TestDbContext : SchedulasDbContext
    {
        public TestDbContext(DbContextOptions<SchedulasDbContext> options, ITenantService tenantService) : base(options, tenantService) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasConversion(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter());
                }
            }
        }
    }

    private SchedulasDbContext CreateContext()
    {
        return new TestDbContext(_options, _tenantServiceMock.Object);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private async Task<(Guid instId, Guid classId, Guid studentId, Guid teacherId, Guid parentId)> SetupHierarchyAsync(SchedulasDbContext context)
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

        // Setup users
        var studentProfile = new Profile(Guid.NewGuid(), "Student", "s@test.com", UserRole.Student, inst.Id, null);
        var teacherProfile = new Profile(Guid.NewGuid(), "Teacher", "t@test.com", UserRole.Teacher, inst.Id, dept.Id);
        var parentProfile = new Profile(Guid.NewGuid(), "Parent", "p@test.com", UserRole.Parent, inst.Id, null);

        context.Profiles.AddRange(studentProfile, teacherProfile, parentProfile);

        var student = new Student(studentProfile.Id, inst.Id, "S1");
        var teacher = new Teacher(teacherProfile.Id, inst.Id, dept.Id);
        var parent = new Parent(parentProfile.Id);

        context.Students.Add(student);
        context.Teachers.Add(teacher);
        context.Parents.Add(parent);

        // Link them to class
        context.ClassStudents.Add(new ClassStudent(cls.Id, student.Id));
        context.ClassTeachers.Add(new ClassTeacher(cls.Id, teacher.Id));
        context.ParentStudentLinks.Add(new ParentStudentLink(parent.Id, student.Id));

        await context.SaveChangesAsync();

        return (inst.Id, cls.Id, studentProfile.Id, teacherProfile.Id, parentProfile.Id);
    }

    [Fact]
    public async Task ActivityScheduledEventHandler_ShouldCreateNotificationsForRecipients()
    {
        // Arrange
        Guid instId, classId, sId, tId, pId;
        Activity activity;
        using (var setupContext = CreateContext())
        {
            (instId, classId, sId, tId, pId) = await SetupHierarchyAsync(setupContext);
            activity = Activity.CreateCandidate(classId, instId, ActivityType.Assignment, "Math HW", null, DateOnly.Parse("2026-10-15"), null, null, null, 1, 1m, "{}");
            setupContext.Activities.Add(activity);
            await setupContext.SaveChangesAsync();
        }

        using var context = CreateContext();
        var handler = new ActivityScheduledEventHandler(context, _pushServiceMock.Object);
        var evt = new ActivityScheduledEvent(activity.Id, classId, ActivityType.Assignment, false);
        
        // Act
        await handler.Handle(new DomainEventNotification<ActivityScheduledEvent>(evt), CancellationToken.None);

        // Assert
        var notifications = await context.Notifications.ToListAsync();
        notifications.Should().HaveCount(3);
        notifications.Select(n => n.RecipientId).Should().BeEquivalentTo(new[] { sId, tId, pId });
        notifications.All(n => n.Title.Contains("واجب")).Should().BeTrue();
        
        _pushServiceMock.Verify(p => p.SendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task NotificationQueries_ShouldFilterAndPaginateCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var instId = Guid.NewGuid();
        _currentUserMock.Setup(c => c.UserId).Returns(userId);

        var notifications = new List<Notification>();
        for (int i = 0; i < 5; i++)
        {
            var n = new Notification(userId, instId, NotificationCategory.NewActivity, $"T{i}", "B", null);
            if (i % 2 == 0) n.MarkRead();
            notifications.Add(n);
        }

        var dbContextMock = new Mock<IApplicationDbContext>();
        dbContextMock.Setup(d => d.Notifications).Returns(notifications.AsQueryable().BuildMockDbSet().Object);

        var handler = new GetNotificationsQueryHandler(dbContextMock.Object, _currentUserMock.Object);

        // Act & Assert List
        var all = await handler.Handle(new GetNotificationsQuery(null, null), CancellationToken.None);
        all.Items.Should().HaveCount(5);

        // Act & Assert Unread filtering
        var unread = await handler.Handle(new GetNotificationsQuery(false, null), CancellationToken.None);
        unread.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteNotificationCommand_ShouldSoftDelete()
    {
        // Arrange
        Guid instId, userId;
        using (var setupContext = CreateContext())
        {
            var hierarchy = await SetupHierarchyAsync(setupContext);
            instId = hierarchy.instId;
            userId = hierarchy.studentId;
        }

        _currentUserMock.Setup(c => c.UserId).Returns(userId);
        var notifId = Guid.NewGuid();

        using (var setupContext = CreateContext())
        {
            var notification = new Notification(userId, instId, NotificationCategory.NewActivity, "T", "B", null);
            setupContext.Notifications.Add(notification);
            await setupContext.SaveChangesAsync();
            notifId = notification.Id;
        }

        using var context = CreateContext();
        var handler = new DeleteNotificationCommandHandler(context, _currentUserMock.Object);

        // Act
        await handler.Handle(new DeleteNotificationCommand(notifId), CancellationToken.None);

        // Assert
        var n = await context.Notifications.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == notifId);
        n.Should().NotBeNull();
        n!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllNotificationsReadCommand_ShouldUpdateAllUnread()
    {
        // Arrange
        Guid instId, userId;
        using (var setupContext = CreateContext())
        {
            var hierarchy = await SetupHierarchyAsync(setupContext);
            instId = hierarchy.instId;
            userId = hierarchy.studentId;
        }

        _currentUserMock.Setup(c => c.UserId).Returns(userId);

        using (var setupContext = CreateContext())
        {
            setupContext.Notifications.Add(new Notification(userId, instId, NotificationCategory.NewActivity, "T1", "B", null));
            setupContext.Notifications.Add(new Notification(userId, instId, NotificationCategory.NewActivity, "T2", "B", null));
            await setupContext.SaveChangesAsync();
        }

        using var context = CreateContext();
        var handler = new MarkAllNotificationsReadCommandHandler(context, _currentUserMock.Object);

        // Act
        await handler.Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        // Assert
        var unreadCount = await context.Notifications.CountAsync(n => !n.IsRead);
        unreadCount.Should().Be(0);
    }
}
