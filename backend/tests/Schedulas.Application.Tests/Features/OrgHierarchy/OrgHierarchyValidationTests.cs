using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Features.OrgHierarchy.Commands;
using Schedulas.Application.Features.OrgHierarchy.Queries;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.Application.Tests.Features.OrgHierarchy;

public class OrgHierarchyValidationTests
{
    private readonly Mock<IApplicationDbContext> _dbMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    
    public OrgHierarchyValidationTests()
    {
        _dbMock = new Mock<IApplicationDbContext>();
        _currentUserMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task DeleteDepartment_ThrowsInvalidStateTransition_WhenHasPrograms()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var institutionId = Guid.NewGuid();
        
        var department = new Department(institutionId, "Test Dept");
        var propId = department.GetType().GetProperty("Id");
        propId?.SetValue(department, departmentId);

        var program = new Program(institutionId, departmentId, "Test Program");

        var departmentsDbSetMock = new List<Department> { department }.AsQueryable().BuildMockDbSet();
        // Mock FindAsync since MockQueryable handles LINQ but FindAsync needs explicit setup
        departmentsDbSetMock.Setup(m => m.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?[] ids, CancellationToken ct) => 
                ids.Length == 1 && ids[0] is Guid id && id == departmentId ? department : null);

        var programsDbSetMock = new List<Program> { program }.AsQueryable().BuildMockDbSet();

        _dbMock.Setup(d => d.Departments).Returns(departmentsDbSetMock.Object);
        _dbMock.Setup(d => d.Programs).Returns(programsDbSetMock.Object);

        _currentUserMock.Setup(c => c.Role).Returns(UserRole.InstitutionAdmin);
        _currentUserMock.Setup(c => c.InstitutionId).Returns(institutionId);

        var handler = new DeleteDepartmentCommandHandler(_dbMock.Object, _currentUserMock.Object);
        var command = new DeleteDepartmentCommand(departmentId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidStateTransitionException>();
        ex.WithMessage("*Cannot delete a department that contains programs*");
    }

    [Fact]
    public async Task RestoreDepartment_ThrowsInvalidStateTransition_WhenInstitutionIsDeleted()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var institutionId = Guid.NewGuid();
        
        var department = new Department(institutionId, "Test Dept");
        department.DeletedAt = DateTime.UtcNow; // simulate soft delete
        var propId = department.GetType().GetProperty("Id");
        propId?.SetValue(department, departmentId);

        var departmentsDbSetMock = new List<Department> { department }.AsQueryable().BuildMockDbSet();
        departmentsDbSetMock.Setup(m => m.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?[] ids, CancellationToken ct) => 
                ids.Length == 1 && ids[0] is Guid id && id == departmentId ? department : null);
        
        // Empty institutions list (deleted or missing)
        var institutionsDbSetMock = new List<Institution>().AsQueryable().BuildMockDbSet();
        institutionsDbSetMock.Setup(m => m.FindAsync(It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?[] ids, CancellationToken ct) => null);

        _dbMock.Setup(d => d.Departments).Returns(departmentsDbSetMock.Object);
        _dbMock.Setup(d => d.Institutions).Returns(institutionsDbSetMock.Object);

        _currentUserMock.Setup(c => c.Role).Returns(UserRole.InstitutionAdmin);
        _currentUserMock.Setup(c => c.InstitutionId).Returns(institutionId);

        var handler = new RestoreDepartmentCommandHandler(_dbMock.Object, _currentUserMock.Object);
        var command = new RestoreDepartmentCommand(departmentId);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidStateTransitionException>();
        ex.WithMessage("*Cannot restore department because its institution is suspended or not found.*");
    }
}
