using System.Text.Json;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.RuleEngine.Rules;

/// <summary>Parameters shape: { "max": 2 }</summary>
public sealed class MaxActivitiesPerDayRule : IRule
{
    public RuleType RuleType => RuleType.MaxActivitiesPerDay;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var max = ParseMax(context.Rule.ParametersJson);
        var sameDayCount = context.OtherActivitiesInScope
            .Count(a => a.ScheduledDate == context.Candidate.ScheduledDate);

        return sameDayCount + 1 > max
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_MAX_ACTIVITIES_PER_DAY_EXCEEDED", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }

    private static int ParseMax(string json) =>
        JsonDocument.Parse(json).RootElement.TryGetProperty("max", out var v) ? v.GetInt32() : int.MaxValue;
}

/// <summary>Parameters shape: { "max": 1 }</summary>
public sealed class MaxExamsPerWeekRule : IRule
{
    public RuleType RuleType => RuleType.MaxExamsPerWeek;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        if (context.Candidate.ActivityType != ActivityType.Exam)
            return SingleRuleResult.Pass(context.Rule.Id);

        var max = ParseMax(context.Rule.ParametersJson);
        var (weekStart, weekEnd) = GetIsoWeekRange(context.Candidate.ScheduledDate);

        var sameWeekExamCount = context.OtherActivitiesInScope.Count(a =>
            a.ActivityType == ActivityType.Exam &&
            a.ScheduledDate >= weekStart && a.ScheduledDate <= weekEnd);

        return sameWeekExamCount + 1 > max
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_MAX_EXAMS_PER_WEEK_EXCEEDED", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }

    private static int ParseMax(string json) =>
        JsonDocument.Parse(json).RootElement.TryGetProperty("max", out var v) ? v.GetInt32() : int.MaxValue;

    private static (DateOnly start, DateOnly end) GetIsoWeekRange(DateOnly date)
    {
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        var start = date.AddDays(-diff);
        return (start, start.AddDays(6));
    }
}

/// <summary>Parameters shape: { "minDays": 3 }</summary>
public sealed class MinDaysBeforeExamRule : IRule
{
    public RuleType RuleType => RuleType.MinDaysBeforeExam;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        if (context.Candidate.ActivityType != ActivityType.Exam)
            return SingleRuleResult.Pass(context.Rule.Id);

        var minDays = ParseMinDays(context.Rule.ParametersJson);

        var tooCloseToAnotherExam = context.OtherActivitiesInScope.Any(a =>
            a.ActivityType == ActivityType.Exam &&
            Math.Abs(a.ScheduledDate.DayNumber - context.Candidate.ScheduledDate.DayNumber) < minDays);

        return tooCloseToAnotherExam
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_MIN_DAYS_BEFORE_EXAM_VIOLATED", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }

    private static int ParseMinDays(string json) =>
        JsonDocument.Parse(json).RootElement.TryGetProperty("minDays", out var v) ? v.GetInt32() : 0;
}

public sealed class NoActivityOnHolidayRule : IRule
{
    public RuleType RuleType => RuleType.NoActivityOnHoliday;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var isHoliday = context.Holidays.Any(h => h.HolidayDate == context.Candidate.ScheduledDate);

        return isHoliday
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_NO_ACTIVITY_ON_HOLIDAY", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }
}

/// <summary>Detects a scheduling clash: same class, same date, same type already exists.</summary>
public sealed class ConflictDetectionRule : IRule
{
    public RuleType RuleType => RuleType.ConflictDetection;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var conflict = context.OtherActivitiesInScope.Any(a =>
            a.ClassId == context.Candidate.ClassId &&
            a.ScheduledDate == context.Candidate.ScheduledDate &&
            a.ActivityType == context.Candidate.ActivityType);

        return conflict
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_CONFLICT_DETECTED", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }
}

/// <summary>
/// Not a standalone check — used by the orchestrator when two DIFFERENT
/// rules disagree (e.g., one would Reject, another would RequiresOverride)
/// to decide whose outcome wins. Implemented as an IRule for registry
/// consistency, but the orchestrator invokes its tie-break logic
/// separately from the per-rule evaluation loop (Architecture §5).
/// </summary>
public sealed class PriorityResolutionRule : IRule
{
    public RuleType RuleType => RuleType.PriorityResolution;

    public SingleRuleResult Evaluate(RuleEvaluationContext context) =>
        SingleRuleResult.Pass(context.Rule.Id); // no-op in the per-rule loop; see orchestrator tie-break logic
}
