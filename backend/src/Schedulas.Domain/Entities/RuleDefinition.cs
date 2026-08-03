using Schedulas.Domain.Common;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.Entities;

/// <summary>
/// A rule is DATA, not code (Constitution §18). Institution/Department
/// Admins create, edit, and prioritize these through the API; the
/// RuleEngineOrchestrator loads active rows matching a candidate
/// Activity's scope and hands each to the matching IRule strategy.
/// </summary>
public class RuleDefinition : BaseEntity, IMustHaveTenant
{
    public Guid InstitutionId { get; private set; }
    public RuleScopeLevel ScopeLevel { get; private set; }
    public Guid ScopeId { get; private set; } // polymorphic FK matching ScopeLevel's table
    public RuleType RuleType { get; private set; }
    public string ParametersJson { get; private set; } = "{}";
    public int Priority { get; private set; }
    public bool IsActive { get; private set; } = true;

    private RuleDefinition() { }

    public RuleDefinition(Guid institutionId, RuleScopeLevel scopeLevel, Guid scopeId,
        RuleType ruleType, string parametersJson, int priority)
    {
        InstitutionId = institutionId;
        ScopeLevel = scopeLevel;
        ScopeId = scopeId;
        RuleType = ruleType;
        ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson;
        Priority = priority;
    }

    public void Update(string parametersJson, int priority, bool isActive)
    {
        ParametersJson = parametersJson;
        Priority = priority;
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>
/// Immutable audit record of every Rule Engine evaluation, whether tied to
/// a persisted Activity or a dry-run (Constitution §18, API Design §9).
/// Uses ImmutableRecordEntity, not BaseEntity -- this row is written once
/// and never updated or soft-deleted, so it carries no UpdatedAt/DeletedAt
/// columns at all (see ImmutableRecordEntity's doc comment for why mixing
/// "ignored column" with "implements ISoftDeletableEntity" is unsafe).
/// </summary>
public class RuleEvaluationLog : ImmutableRecordEntity, IMustHaveTenant
{
    public Guid? ActivityId { get; private set; }
    public Guid InstitutionId { get; private set; }
    public RuleOutcome Outcome { get; private set; }
    public Guid? TriggeredRuleId { get; private set; }
    public string? ReasonCode { get; private set; }
    public string? OverrideNote { get; private set; }
    public Guid? EvaluatedBy { get; private set; }
    public DateTimeOffset EvaluatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private RuleEvaluationLog() { }

    public RuleEvaluationLog(Guid? activityId, Guid institutionId, RuleOutcome outcome,
        Guid? triggeredRuleId, string? reasonCode, string? overrideNote, Guid? evaluatedBy)
    {
        ActivityId = activityId;
        InstitutionId = institutionId;
        Outcome = outcome;
        TriggeredRuleId = triggeredRuleId;
        ReasonCode = reasonCode;
        OverrideNote = overrideNote;
        EvaluatedBy = evaluatedBy;
    }
}
