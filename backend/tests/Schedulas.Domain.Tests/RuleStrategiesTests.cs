using FluentAssertions;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.RuleEngine;
using Schedulas.Domain.RuleEngine.Rules;
using System;
using Xunit;

namespace Schedulas.Domain.Tests;

public class RuleStrategiesTests
{
    [Fact]
    public void NoActivityOnHolidayRule_Fails_WhenActivityOverlapsHoliday()
    {
        // Arrange
        var ruleDefinition = new RuleDefinition(Guid.NewGuid(), RuleScopeLevel.Institution, Guid.NewGuid(), RuleType.NoActivityOnHoliday, "{}", 1);
        var strategy = new NoActivityOnHolidayRule();
        var activity = Activity.CreateCandidate(Guid.NewGuid(), Guid.NewGuid(), ActivityType.Event, "Test Activity", null, 
            DateOnly.Parse("2026-12-25"), null, null, null, 1, null, null);

        var holiday = new Holiday(Guid.NewGuid(), null, "Christmas", DateOnly.Parse("2026-12-25"));
        var context = new RuleEvaluationContext { Candidate = activity, Holidays = new[] { holiday }, OtherActivitiesInScope = Array.Empty<Activity>(), Rule = ruleDefinition };

        // Act
        var result = strategy.Evaluate(context);

        // Assert
        result.Outcome.Should().Be(RuleOutcome.Rejected);
        result.ReasonCode.Should().Be("RULE_NO_ACTIVITY_ON_HOLIDAY");
    }

    [Fact]
    public void NoActivityOnHolidayRule_Passes_WhenActivityOutsideHoliday()
    {
        // Arrange
        var ruleDefinition = new RuleDefinition(Guid.NewGuid(), RuleScopeLevel.Institution, Guid.NewGuid(), RuleType.NoActivityOnHoliday, "{}", 1);
        var strategy = new NoActivityOnHolidayRule();
        var activity = Activity.CreateCandidate(Guid.NewGuid(), Guid.NewGuid(), ActivityType.Event, "Test Activity", null, 
            DateOnly.Parse("2026-12-26"), null, null, null, 1, null, null);

        var holiday = new Holiday(Guid.NewGuid(), null, "Christmas", DateOnly.Parse("2026-12-25"));
        var context = new RuleEvaluationContext { Candidate = activity, Holidays = new[] { holiday }, OtherActivitiesInScope = Array.Empty<Activity>(), Rule = ruleDefinition };

        // Act
        var result = strategy.Evaluate(context);

        // Assert
        result.Outcome.Should().Be(RuleOutcome.Approved);
    }
}
