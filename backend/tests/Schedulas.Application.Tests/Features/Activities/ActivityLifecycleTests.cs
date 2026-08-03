using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Features.Activities.Commands;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Schedulas.Application.Tests.Features.Activities;

public class ActivityLifecycleTests
{
    [Fact]
    public void Cancel_UpdatesStatusToCancelled()
    {
        // Arrange
        var activity = Activity.CreateCandidate(Guid.NewGuid(), Guid.NewGuid(), ActivityType.Assignment, "Title", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, null, 1, 1.0m, "{}");
        activity.MarkApproved();

        // Act
        activity.Cancel();

        // Assert
        activity.Status.Should().Be(ActivityStatus.Cancelled);
        activity.DomainEvents.Should().Contain(e => e.GetType().Name == "ActivityCancelledEvent");
    }

    [Fact]
    public void Restore_FromCancelled_ThrowsException()
    {
        // Arrange
        var activity = Activity.CreateCandidate(Guid.NewGuid(), Guid.NewGuid(), ActivityType.Assignment, "Title", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, null, 1, 1.0m, "{}");
        activity.MarkApproved();
        activity.Cancel();

        // Act & Assert
        Action act = () => activity.Restore();
        act.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Restore_FromActive_SucceedsAndClearsDeletedAt()
    {
        // Arrange
        var activity = Activity.CreateCandidate(Guid.NewGuid(), Guid.NewGuid(), ActivityType.Assignment, "Title", null, DateOnly.FromDateTime(DateTime.UtcNow), null, null, null, 1, 1.0m, "{}");
        activity.MarkApproved();
        
        // Simulating EF Core interceptor soft-delete
        var prop = typeof(Activity).GetProperty("DeletedAt");
        prop!.SetValue(activity, DateTimeOffset.UtcNow);

        activity.IsDeleted.Should().BeTrue();

        // Act
        activity.Restore();

        // Assert
        activity.IsDeleted.Should().BeFalse();
        activity.DomainEvents.Should().Contain(e => e.GetType().Name == "ActivityRestoredEvent");
    }
}
