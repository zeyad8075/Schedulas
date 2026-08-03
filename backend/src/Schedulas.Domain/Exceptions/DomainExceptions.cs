namespace Schedulas.Domain.Exceptions;

/// <summary>
/// Base type for all exceptions raised from within the Domain layer.
/// Carries a language-neutral ReasonCode; the Presentation layer resolves
/// it to Arabic text (Architecture §7). Never construct these with a
/// hardcoded Arabic message.
/// </summary>
public abstract class DomainException : Exception
{
    public string ReasonCode { get; }

    protected DomainException(string reasonCode, string debugMessage)
        : base(debugMessage)
    {
        ReasonCode = reasonCode;
    }
}

/// <summary>
/// Raised when the Rule Engine rejects an activity outright. Caught by the
/// global exception middleware and mapped to HTTP 422 with the Arabic
/// message resolved from ReasonCode (Constitution §13).
/// </summary>
public sealed class RuleViolationException : DomainException
{
    public Guid? TriggeredRuleId { get; }

    public RuleViolationException(string reasonCode, Guid? triggeredRuleId, string debugMessage)
        : base(reasonCode, debugMessage)
    {
        TriggeredRuleId = triggeredRuleId;
    }
}

/// <summary>
/// Raised when a requested entity does not exist within the caller's
/// tenant scope. Deliberately indistinguishable, from the caller's
/// perspective, from "belongs to another tenant" — see API Design §15.
/// </summary>
public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id)
        : base("ENTITY_NOT_FOUND", $"{entityName} with id '{id}' was not found.")
    {
    }
}

/// <summary>
/// Raised when an entity is asked to transition into an invalid state
/// (e.g., editing a Cancelled activity).
/// </summary>
public sealed class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException(string reasonCode, string debugMessage)
        : base(reasonCode, debugMessage)
    {
    }
}
