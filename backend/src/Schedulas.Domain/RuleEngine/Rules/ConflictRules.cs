using System.Text.Json;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.RuleEngine.Rules;

public static class ConflictHelper
{
    public static bool IsOverlapping(Activity a, Activity b)
    {
        if (a.ScheduledDate != b.ScheduledDate) return false;
        
        if (!a.ScheduledTime.HasValue || !a.EndTime.HasValue || 
            !b.ScheduledTime.HasValue || !b.EndTime.HasValue) 
            return false;

        return a.ScheduledTime.Value < b.EndTime.Value && a.EndTime.Value > b.ScheduledTime.Value;
    }

    public static string? GetStringMetadata(string? metadataJson, string key)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return null;
        try
        {
            var doc = JsonDocument.Parse(metadataJson);
            return doc.RootElement.TryGetProperty(key, out var prop) ? prop.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}

public sealed class TeacherConflictRule : IRule
{
    public RuleType RuleType => RuleType.TeacherConflict;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var teacherId = ConflictHelper.GetStringMetadata(context.Candidate.MetadataJson, "teacherId");
        if (string.IsNullOrWhiteSpace(teacherId)) return SingleRuleResult.Pass(context.Rule.Id);

        var conflict = context.OtherActivitiesInScope.Any(a => 
            ConflictHelper.GetStringMetadata(a.MetadataJson, "teacherId") == teacherId &&
            ConflictHelper.IsOverlapping(a, context.Candidate));

        return conflict
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_TEACHER_CONFLICT", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }
}

public sealed class ClassConflictRule : IRule
{
    public RuleType RuleType => RuleType.ClassConflict;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var conflict = context.OtherActivitiesInScope.Any(a => 
            a.ClassId == context.Candidate.ClassId &&
            ConflictHelper.IsOverlapping(a, context.Candidate));

        return conflict
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_CLASS_CONFLICT", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }
}

public sealed class RoomConflictRule : IRule
{
    public RuleType RuleType => RuleType.RoomConflict;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var roomId = ConflictHelper.GetStringMetadata(context.Candidate.MetadataJson, "roomId");
        if (string.IsNullOrWhiteSpace(roomId)) return SingleRuleResult.Pass(context.Rule.Id);

        var conflict = context.OtherActivitiesInScope.Any(a => 
            ConflictHelper.GetStringMetadata(a.MetadataJson, "roomId") == roomId &&
            ConflictHelper.IsOverlapping(a, context.Candidate));

        return conflict
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_ROOM_CONFLICT", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }
}

public sealed class DuplicateActivityRule : IRule
{
    public RuleType RuleType => RuleType.DuplicateActivity;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var conflict = context.OtherActivitiesInScope.Any(a => 
            a.ClassId == context.Candidate.ClassId &&
            a.ScheduledDate == context.Candidate.ScheduledDate &&
            a.Title.Equals(context.Candidate.Title, StringComparison.OrdinalIgnoreCase));

        return conflict
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_DUPLICATE_ACTIVITY", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }
}

public sealed class OverlappingActivityRule : IRule
{
    public RuleType RuleType => RuleType.OverlappingActivity;

    public SingleRuleResult Evaluate(RuleEvaluationContext context)
    {
        var conflict = context.OtherActivitiesInScope.Any(a => 
            ConflictHelper.IsOverlapping(a, context.Candidate));

        return conflict
            ? new SingleRuleResult(RuleOutcome.Rejected, "RULE_OVERLAPPING_ACTIVITY", context.Rule.Id)
            : SingleRuleResult.Pass(context.Rule.Id);
    }
}
