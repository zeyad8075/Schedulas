using FluentAssertions;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Infrastructure.Identity;
using System;
using Xunit;

namespace Schedulas.Infrastructure.Tests;

public class TenantServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock;
    
    public TenantServiceTests()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public void SetTenantId_SetsTenantId_AndEnforcesTenant()
    {
        var sut = new TenantService(_currentUserMock.Object);
        var tenantId = Guid.NewGuid();

        sut.SetTenantId(tenantId);

        sut.TenantId.Should().Be(tenantId);
        sut.IsTenantEnforced.Should().BeTrue();
    }

    [Fact]
    public void BeginBypassScope_DisablesTenantEnforcement_UntilDisposed()
    {
        var sut = new TenantService(_currentUserMock.Object);
        sut.SetTenantId(Guid.NewGuid());

        sut.IsTenantEnforced.Should().BeTrue();

        using (var scope = sut.BeginBypassScope())
        {
            sut.IsTenantEnforced.Should().BeFalse();
        }

        sut.IsTenantEnforced.Should().BeTrue();
    }

    [Fact]
    public void BeginBypassScope_RestoresPreviousState_EvenIfExceptionThrown()
    {
        var sut = new TenantService(_currentUserMock.Object);
        sut.SetTenantId(Guid.NewGuid());

        try
        {
            using (var scope = sut.BeginBypassScope())
            {
                throw new InvalidOperationException("Test exception");
            }
        }
        catch { }

        sut.IsTenantEnforced.Should().BeTrue();
    }
}
