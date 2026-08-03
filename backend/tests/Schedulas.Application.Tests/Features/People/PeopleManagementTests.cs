using Schedulas.Infrastructure.Persistence.Interceptors;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Features.People.Commands;
using Schedulas.Application.Features.OrgHierarchy.Commands;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.Application.Tests.Features.People;

public class PeopleManagementTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SchedulasDbContext> _options;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public PeopleManagementTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantServiceMock = new Mock<ITenantService>();
        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false); // Platform admin by default for setup

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.Role).Returns(UserRole.PlatformAdmin);

        _options = new DbContextOptionsBuilder<SchedulasDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new AuditableSaveChangesInterceptor(_currentUserMock.Object))
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

    [Fact]
    public async Task CreateStudent_WithDuplicateStudentNumber_ThrowsInvalidStateTransitionException()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var profile1Id = Guid.NewGuid();
        var profile2Id = Guid.NewGuid();
        var studentNumber = "STU-123";

        using (var setupContext = CreateContext())
        {
            var inst = new Institution("Test Inst", InstitutionType.School, "UTC");
            inst.GetType().GetProperty("Id")!.SetValue(inst, institutionId);
            setupContext.Institutions.Add(inst);

            setupContext.Profiles.Add(new Profile(profile1Id, "Student One", "s1@test.com", UserRole.Student, institutionId, null));
            setupContext.Profiles.Add(new Profile(profile2Id, "Student Two", "s2@test.com", UserRole.Student, institutionId, null));
            
            setupContext.Students.Add(new Student(profile1Id, institutionId, studentNumber));
            await setupContext.SaveChangesAsync();
        }

        using var context = CreateContext();
        var handler = new CreateStudentCommandHandler(context);
        var command = new CreateStudentCommand(profile2Id, institutionId, studentNumber);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidStateTransitionException>(() => handler.Handle(command, CancellationToken.None));
        ex.ReasonCode.Should().Be("DUPLICATE_NUMBER");
    }

    [Fact]
    public async Task RestoreStudent_FailsIfProfileIsDeleted()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var studentId = Guid.Empty;

        using (var setupContext = CreateContext())
        {
            var profile = new Profile(profileId, "Test Student", "test@test.com", UserRole.Student, institutionId, null);
            // Simulate soft delete
            profile.GetType().GetProperty("DeletedAt")!.SetValue(profile, DateTimeOffset.UtcNow);
            setupContext.Profiles.Add(profile);

            var student = new Student(profileId, institutionId, "STU-001");
            student.GetType().GetProperty("DeletedAt")!.SetValue(student, DateTimeOffset.UtcNow);
            setupContext.Students.Add(student);
            await setupContext.SaveChangesAsync();
            
            studentId = student.Id;
        }

        using var context = CreateContext();
        var handler = new RestoreStudentCommandHandler(context, _currentUserMock.Object);
        var command = new RestoreStudentCommand(studentId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidStateTransitionException>(() => handler.Handle(command, CancellationToken.None));
        ex.ReasonCode.Should().Be("PARENT_INACTIVE");
    }

    [Fact]
    public async Task DeleteStudent_AppliesSoftDelete()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var studentId = Guid.Empty;

        using (var setupContext = CreateContext())
        {
            setupContext.Profiles.Add(new Profile(profileId, "Test Student", "test@test.com", UserRole.Student, institutionId, null));
            var student = new Student(profileId, institutionId, "STU-999");
            setupContext.Students.Add(student);
            await setupContext.SaveChangesAsync();
            studentId = student.Id;
        }

        using var context = CreateContext();
        var handler = new DeleteStudentCommandHandler(context);
        
        // Act
        await handler.Handle(new DeleteStudentCommand(studentId), CancellationToken.None);

        // Assert
        using var verifyContext = CreateContext();
        var deletedStudent = await verifyContext.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == studentId);
        
        deletedStudent.Should().NotBeNull();
        deletedStudent!.DeletedAt.Should().NotBeNull();
        
        // Should be invisible to normal queries
        var visibleStudent = await verifyContext.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        visibleStudent.Should().BeNull();
    }

    [Fact]
    public async Task EnrollStudent_InDifferentInstitution_ThrowsInvalidStateTransitionException()
    {
        // Arrange
        var institution1Id = Guid.NewGuid();
        var institution2Id = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var classId = Guid.Empty;
        var studentId = Guid.Empty;

        using (var setupContext = CreateContext())
        {

            var inst1 = new Institution("Inst 1", InstitutionType.School, "UTC");
            inst1.GetType().GetProperty("Id")!.SetValue(inst1, institution1Id);
            setupContext.Institutions.Add(inst1);
            
            var inst2 = new Institution("Inst 2", InstitutionType.School, "UTC");
            inst2.GetType().GetProperty("Id")!.SetValue(inst2, institution2Id);
            setupContext.Institutions.Add(inst2);
            await setupContext.SaveChangesAsync();

            var dept = new Department(institution1Id, "Dept");
            setupContext.Departments.Add(dept);
            
            var prog = new Program(institution1Id, dept.Id, "Prog");
            setupContext.Programs.Add(prog);
            
            var course = new Course(institution1Id, prog.Id, "Course", null);
            setupContext.Courses.Add(course);
            
            var term = new AcademicTerm(institution1Id, "Term", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)));
            setupContext.AcademicTerms.Add(term);
            await setupContext.SaveChangesAsync();

            var cls = new Class(institution1Id, course.Id, term.Id, "Class 1A");
            setupContext.Classes.Add(cls);
            
            setupContext.Profiles.Add(new Profile(profileId, "Test Student", "test@test.com", UserRole.Student, institution2Id, null));
            var student = new Student(profileId, institution2Id, "STU-001");
            setupContext.Students.Add(student);
            
            await setupContext.SaveChangesAsync();
            classId = cls.Id;
            studentId = student.Id;
        }

        using var context = CreateContext();
        var handler = new EnrollStudentCommandHandler(context, _currentUserMock.Object);
        var command = new EnrollStudentCommand(classId, studentId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidStateTransitionException>(() => handler.Handle(command, CancellationToken.None));
        ex.ReasonCode.Should().Be("CROSS_INSTITUTION_ENROLLMENT");
    }

    [Fact]
    public async Task CreateParent_ProfileMustHaveParentRole()
    {
        // Arrange
        var profileId = Guid.NewGuid();

        using (var setupContext = CreateContext())
        {
            // Give the profile the STUDENT role instead of PARENT
            setupContext.Profiles.Add(new Profile(profileId, "Test Parent", "parent@test.com", UserRole.Student, Guid.NewGuid(), null));
            await setupContext.SaveChangesAsync();
        }

        using var context = CreateContext();
        var handler = new CreateParentCommandHandler(context);
        var command = new CreateParentCommand(profileId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidStateTransitionException>(() => handler.Handle(command, CancellationToken.None));
        ex.ReasonCode.Should().Be("INVALID_ROLE");
    }
}
