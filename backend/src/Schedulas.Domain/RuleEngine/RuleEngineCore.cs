using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.RuleEngine;

/// <summary>
/// Everything a rule strategy needs to evaluate a candidate Activity,
/// pre-loaded by the Application-layer orchestrator (Architecture §5 step 3).
/// Domain rule strategies never touch the database directly — they only
/// see the data handed to them here. This is what keeps them pure and
/// trivially unit-testable.
/// </summary>
public sealed class RuleEvaluationContext
{
    public required Activity Candidate { get; init; }
    public required IReadOnlyList<Activity> OtherActivitiesInScope { get; init; }
    public required IReadOnlyList<Holiday> Holidays { get; init; }
    public required RuleDefinition Rule { get; init; }
}

/// <summary>
/// The outcome of a single rule's evaluation. The orchestrator aggregates
/// these across all applicable rules to produce the final RuleEngineResult.
/// </summary>
public sealed record SingleRuleResult(RuleOutcome Outcome, string ReasonCode, Guid RuleId)
{
    public static SingleRuleResult Pass(Guid ruleId) => new(RuleOutcome.Approved, "RULE_PASSED", ruleId);
}

/// <summary>
/// The final, aggregated result of a full Rule Engine evaluation across all
/// applicable rules — this is what the Application-layer handler acts on.
/// </summary>
public sealed record RuleEngineResult(RuleOutcome Outcome, string? ReasonCode, Guid? TriggeredRuleId)
{
    public static RuleEngineResult Approved() => new(RuleOutcome.Approved, null, null);

    public static RuleEngineResult Rejected(string reasonCode, Guid ruleId) =>
        new(RuleOutcome.Rejected, reasonCode, ruleId);

    public static RuleEngineResult RequiresOverride(string reasonCode, Guid ruleId) =>
        new(RuleOutcome.RequiresOverride, reasonCode, ruleId);
}

/// <summary>
/// Strategy interface — one implementation per RuleType (Architecture §5,
/// §9). Adding a new rule type means: implement this interface, register it
/// in the rule-type registry. No existing rule, handler, or orchestrator
/// code changes (Constitution §30 extensibility requirement).
/// </summary>
public interface IRule
{
    RuleType RuleType { get; }
    SingleRuleResult Evaluate(RuleEvaluationContext context);
}

/// <summary>
/// The orchestrator contract, called by Command handlers before any
/// Activity is persisted (Architecture §5 step 2). The implementation
/// (Application layer, since it needs repository access to load rules and
/// contextual data) is IRuleEngine's only consumer-facing surface —
/// handlers never talk to individual IRule strategies directly.
/// </summary>
public interface IRuleEngine
{
    Task<RuleEngineResult> EvaluateAsync(Activity candidate, CancellationToken cancellationToken = default);
}
