using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.Infrastructure.Tests;

public class MultiTenancyTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SchedulasDbContext> _options;
    private readonly Mock<ITenantService> _tenantServiceMock;

    public MultiTenancyTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SchedulasDbContext>()
            .UseSqlite(_connection)
            .Options;

        _tenantServiceMock = new Mock<ITenantService>();
        
        using var context = new SchedulasDbContext(_options, _tenantServiceMock.Object);
        context.Database.EnsureCreated();
    }

    private SchedulasDbContext CreateContext()
    {
        return new SchedulasDbContext(_options, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task GlobalQueryFilter_FiltersByTenantId_WhenEnforced()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false);
        using (var seedContext = CreateContext())
        {
            var instA = new Institution("Tenant A", InstitutionType.School, "UTC");
            instA.GetType().GetProperty("Id")!.SetValue(instA, tenantA);

            var instB = new Institution("Tenant B", InstitutionType.School, "UTC");
            instB.GetType().GetProperty("Id")!.SetValue(instB, tenantB);

            var deptA = new Department(tenantA, "Dept A");
            var deptB = new Department(tenantB, "Dept B");

            seedContext.Institutions.AddRange(instA, instB);
            seedContext.Departments.AddRange(deptA, deptB);
            await seedContext.SaveChangesAsync();
        }

        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(true);
        _tenantServiceMock.Setup(t => t.TenantId).Returns(tenantA);

        using (var context = CreateContext())
        {
            // Act
            var depts = await context.Departments.ToListAsync();

            // Assert
            depts.Should().HaveCount(1);
            depts.First().InstitutionId.Should().Be(tenantA);
        }
    }

    [Fact]
    public async Task GlobalQueryFilter_ReturnsAll_WhenBypassed()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false);
        using (var seedContext = CreateContext())
        {
            var instA = new Institution("Tenant A", InstitutionType.School, "UTC");
            instA.GetType().GetProperty("Id")!.SetValue(instA, tenantA);

            var instB = new Institution("Tenant B", InstitutionType.School, "UTC");
            instB.GetType().GetProperty("Id")!.SetValue(instB, tenantB);

            var deptA = new Department(tenantA, "Dept A");
            var deptB = new Department(tenantB, "Dept B");

            seedContext.Institutions.AddRange(instA, instB);
            seedContext.Departments.AddRange(deptA, deptB);
            await seedContext.SaveChangesAsync();
        }

        // PlatformAdmin bypasses the filter
        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false);

        using (var context = CreateContext())
        {
            // Act
            var depts = await context.Departments.ToListAsync();

            // Assert
            depts.Should().HaveCount(2);
        }
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
