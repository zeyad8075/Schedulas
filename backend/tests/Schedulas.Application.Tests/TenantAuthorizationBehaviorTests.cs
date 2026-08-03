using FluentAssertions;
using MediatR;
using Moq;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.Application.Tests;

public class TenantAuthorizationBehaviorTests
{
    private class TestRequest : ITenantScopedRequest, IRequest<string>
    {
        public Guid? TargetInstitutionId { get; set; }
        public Guid? TargetDepartmentId { get; set; }
    }

    private class NonTenantRequest : IRequest<string> { }

    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TenantAuthorizationBehavior<TestRequest, string> _tenantBehavior;
    private readonly TenantAuthorizationBehavior<NonTenantRequest, string> _nonTenantBehavior;

    public TenantAuthorizationBehaviorTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _tenantBehavior = new TenantAuthorizationBehavior<TestRequest, string>(_currentUserServiceMock.Object);
        _nonTenantBehavior = new TenantAuthorizationBehavior<NonTenantRequest, string>(_currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_PassesThrough_ForNonTenantRequests()
    {
        // Arrange
        var request = new NonTenantRequest();
        var nextDelegate = new RequestHandlerDelegate<string>(() => Task.FromResult("Success"));

        // Act
        var result = await _nonTenantBehavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenTenantIdsMismatch()
    {
        // Arrange
        var request = new TestRequest { TargetInstitutionId = Guid.NewGuid() };
        _currentUserServiceMock.Setup(c => c.InstitutionId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(c => c.Role).Returns(UserRole.InstitutionAdmin);
        var nextDelegate = new RequestHandlerDelegate<string>(() => Task.FromResult("Success"));

        // Act
        Func<Task> act = async () => await _tenantBehavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PassesThrough_WhenTenantIdsMatch()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var request = new TestRequest { TargetInstitutionId = tenantId };
        _currentUserServiceMock.Setup(c => c.InstitutionId).Returns(tenantId);
        _currentUserServiceMock.Setup(c => c.Role).Returns(UserRole.InstitutionAdmin);
        var nextDelegate = new RequestHandlerDelegate<string>(() => Task.FromResult("Success"));

        // Act
        var result = await _tenantBehavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_PassesThrough_WhenUserIsPlatformAdmin()
    {
        // Arrange
        var request = new TestRequest { TargetInstitutionId = Guid.NewGuid() };
        _currentUserServiceMock.Setup(c => c.InstitutionId).Returns(Guid.NewGuid());
        _currentUserServiceMock.Setup(c => c.Role).Returns(UserRole.PlatformAdmin);
        var nextDelegate = new RequestHandlerDelegate<string>(() => Task.FromResult("Success"));

        // Act
        var result = await _tenantBehavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
    }
}
