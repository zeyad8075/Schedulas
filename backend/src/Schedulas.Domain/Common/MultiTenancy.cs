namespace Schedulas.Domain.Common;

/// <summary>
/// Contract for entities that strictly belong to a single tenant.
/// A Global Query Filter automatically applies to these to prevent cross-tenant data leakage.
/// </summary>
public interface IMustHaveTenant
{
    Guid InstitutionId { get; }
}

/// <summary>
/// Contract for entities that optionally belong to a single tenant.
/// Used primarily for Platform Admins who operate across the system without a fixed tenant.
/// </summary>
public interface IMayHaveTenant
{
    Guid? InstitutionId { get; }
}
