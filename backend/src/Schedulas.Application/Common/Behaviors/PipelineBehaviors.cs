using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Schedulas.Application.Common.Interfaces;

namespace Schedulas.Application.Common.Behaviors;

/// <summary>
/// Runs all registered FluentValidation validators for the request before
/// the handler executes (Constitution §14). A single ValidationException
/// carrying every field error is thrown; the global exception middleware
/// (Presentation layer) maps it to the standard 400 envelope.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}

/// <summary>
/// Marker interface a Command/Query implements to declare which
/// institution/department it targets, so this behavior can enforce
/// tenant isolation centrally rather than in every handler
/// (Architecture §4 / §6). Requests that don't implement it (e.g.
/// institution-agnostic Platform Admin operations) skip this check.
/// </summary>
public interface ITenantScopedRequest
{
    Guid? TargetInstitutionId { get; }
    Guid? TargetDepartmentId { get; }
}

/// <summary>
/// Confirms the caller's JWT-derived institution/department scope actually
/// covers the resource the request targets. Platform Admin bypasses this
/// deliberately (Architecture §4 — an explicit, audited escape hatch).
/// </summary>
public sealed class TenantAuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUser;

    public TenantAuthorizationBehavior(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is ITenantScopedRequest scoped && _currentUser.Role != Domain.Enums.UserRole.PlatformAdmin)
        {
            if (scoped.TargetInstitutionId is Guid targetInstitutionId &&
                targetInstitutionId != _currentUser.InstitutionId)
            {
                throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");
            }

            if (scoped.TargetDepartmentId is Guid targetDepartmentId &&
                _currentUser.Role == Domain.Enums.UserRole.DepartmentAdmin &&
                targetDepartmentId != _currentUser.DepartmentId)
            {
                throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");
            }
        }

        return next();
    }
}

/// <summary>Structured request logging with correlation via Serilog (Constitution §15).</summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName} for user {UserId}", requestName, _currentUser.UserId);

        try
        {
            var response = await next();
            _logger.LogInformation("Handled {RequestName} successfully", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}
