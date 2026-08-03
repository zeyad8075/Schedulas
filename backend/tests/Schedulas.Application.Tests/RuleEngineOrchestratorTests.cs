using FluentAssertions;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.RuleEngine;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.Application.Tests;

public class RuleEngineOrchestratorTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly RuleEngineOrchestrator _sut;

    public RuleEngineOrchestratorTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _sut = new RuleEngineOrchestrator(_dbContextMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitialize()
    {
        // Arrange & Act & Assert
        Assert.NotNull(_sut);
    }

    [Fact]
    public void Activity_CreateCandidate_ShouldCreateCorrectly()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var institutionId = Guid.NewGuid();
        
        var activity = Activity.CreateCandidate(classId, institutionId, ActivityType.Event, "Test Activity", null, DateOnly.Parse("2026-12-25"), null, null, null, 1, null, null);

        // Assert
        activity.Should().NotBeNull();
        activity.Title.Should().Be("Test Activity");
        activity.ActivityType.Should().Be(ActivityType.Event);
    }
}
