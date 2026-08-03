using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.RuleEngine;
using Schedulas.Domain.RuleEngine.Rules;

namespace Schedulas.Application.RuleEngine;

/// <summary>
/// Implements Domain's IRuleEngine contract. Lives in Application (not
/// Domain) specifically because it needs repository/DbContext access to
/// load RuleDefinition rows and contextual data — the actual rule
/// *logic* stays pure in Domain/RuleEngine/Rules; this class is pure
/// orchestration/plumbing (Architecture §3.2, §5, §9 rationale table).
/// </summary>
public sealed class RuleEngineOrchestrator : IRuleEngine
{
    private readonly IApplicationDbContext _db;
    private readonly IReadOnlyDictionary<RuleType, IRule> _ruleRegistry;

    public RuleEngineOrchestrator(IApplicationDbContext db)
    {
        _db = db;

        // The rule-type registry: adding a new RuleType means adding one
        // line here and one IRule implementation in Domain — nothing else
        // changes (Constitution §30).
        _ruleRegistry = new Dictionary<RuleType, IRule>
        {
            [RuleType.MaxActivitiesPerDay] = new MaxActivitiesPerDayRule(),
            [RuleType.MaxExamsPerWeek] = new MaxExamsPerWeekRule(),
            [RuleType.MinDaysBeforeExam] = new MinDaysBeforeExamRule(),
            [RuleType.NoActivityOnHoliday] = new NoActivityOnHolidayRule(),
            [RuleType.ConflictDetection] = new ConflictDetectionRule(),
            [RuleType.PriorityResolution] = new PriorityResolutionRule(),
            [RuleType.TeacherConflict] = new TeacherConflictRule(),
            [RuleType.ClassConflict] = new ClassConflictRule(),
            [RuleType.RoomConflict] = new RoomConflictRule(),
            [RuleType.DuplicateActivity] = new DuplicateActivityRule(),
            [RuleType.OverlappingActivity] = new OverlappingActivityRule(),
        };
    }

    public async Task<RuleEngineResult> EvaluateAsync(Activity candidate, CancellationToken cancellationToken = default)
    {
        // 1. Resolve the scope chain for this candidate's class, most
        //    specific first (Class > Course > Program > Department > Institution),
        //    per Constitution §18 "most specific scope wins".
        var classEntity = await _db.Classes
            .FirstOrDefaultAsync(c => c.Id == candidate.ClassId, cancellationToken)
            ?? throw new Domain.Exceptions.EntityNotFoundException(nameof(Class), candidate.ClassId);

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == classEntity.CourseId, cancellationToken)
            ?? throw new Domain.Exceptions.EntityNotFoundException(nameof(Course), classEntity.CourseId);
        var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == course.ProgramId, cancellationToken)
            ?? throw new Domain.Exceptions.EntityNotFoundException(nameof(Domain.Entities.Program), course.ProgramId);

        var scopeIdsBySpecificity = new (RuleScopeLevel Level, Guid Id)[]
        {
            (RuleScopeLevel.Class, classEntity.Id),
            (RuleScopeLevel.Course, course.Id),
            (RuleScopeLevel.Program, program.Id),
            (RuleScopeLevel.Department, program.DepartmentId),
            (RuleScopeLevel.Institution, candidate.InstitutionId),
        };

        // 2. Load every active rule matching any of those scopes, ordered
        //    most-specific-first then by admin-assigned priority.
        var scopeIds = scopeIdsBySpecificity.Select(s => s.Id).ToList();
        var applicableRules = await _db.RuleDefinitions
            .Where(r => r.IsActive && scopeIds.Contains(r.ScopeId))
            .ToListAsync(cancellationToken);

        var orderedRules = applicableRules
            .OrderBy(r => Array.FindIndex(scopeIdsBySpecificity, s => s.Id == r.ScopeId))
            .ThenByDescending(r => r.Priority)
            .ToList();

        if (orderedRules.Count == 0)
            return RuleEngineResult.Approved();

        // 3. Load contextual data once (not per-rule) for efficiency.
        var windowStart = candidate.ScheduledDate.AddDays(-14);
        var windowEnd = candidate.ScheduledDate.AddDays(14);

        var otherActivities = await _db.Activities
            .Where(a => a.ClassId == candidate.ClassId
                        && a.Id != candidate.Id
                        && a.Status != ActivityStatus.Cancelled
                        && a.ScheduledDate >= windowStart && a.ScheduledDate <= windowEnd)
            .ToListAsync(cancellationToken);

        var holidays = await _db.Holidays
            .Where(h => h.HolidayDate >= windowStart && h.HolidayDate <= windowEnd)
            .ToListAsync(cancellationToken);

        // 4. Evaluate each rule; short-circuit on the first Rejected
        //    (highest specificity/priority wins per Constitution §18).
        SingleRuleResult? firstRequiresOverride = null;

        foreach (var ruleDefinition in orderedRules)
        {
            if (!_ruleRegistry.TryGetValue(ruleDefinition.RuleType, out var strategy))
                continue; // unknown/future rule type — skip rather than fail closed on unrelated data

            var context = new RuleEvaluationContext
            {
                Candidate = candidate,
                OtherActivitiesInScope = otherActivities,
                Holidays = holidays,
                Rule = ruleDefinition
            };

            var result = strategy.Evaluate(context);

            if (result.Outcome == RuleOutcome.Rejected)
                return RuleEngineResult.Rejected(result.ReasonCode, result.RuleId);

            if (result.Outcome == RuleOutcome.RequiresOverride && firstRequiresOverride is null)
                firstRequiresOverride = result;
        }

        return firstRequiresOverride is not null
            ? RuleEngineResult.RequiresOverride(firstRequiresOverride.ReasonCode, firstRequiresOverride.RuleId)
            : RuleEngineResult.Approved();
    }
}
