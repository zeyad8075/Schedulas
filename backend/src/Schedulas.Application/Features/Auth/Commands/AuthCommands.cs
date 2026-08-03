using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.Auth.Commands;

/// <summary>
/// SECURITY FIX (Write-Side Ownership Audit): this command previously
/// accepted Role/InstitutionId/DepartmentId directly from an
/// [AllowAnonymous] endpoint with NO verification of who was setting
/// them -- any unauthenticated caller could self-register as
/// PlatformAdmin or as InstitutionAdmin for any institution. The doc
/// comment claimed these fields were "already-validated... supplied by
/// an accepted invitation link," but no invitation-token mechanism was
/// ever actually implemented, so that claim was not true of the running
/// code. This is now a staff-invoked operation (SRS FR-AUTH-5's actual
/// intent: "Institution Admins invite users by email, pre-assigning role
/// and institution/department scope") rather than public self-service:
/// - Requires authentication (AuthController.Register is no longer
///   [AllowAnonymous] -- see the controller).
/// - Tenant-scoped via ITenantScopedRequest: an InstitutionAdmin can only
///   register users into their own institution; a DepartmentAdmin only
///   into their own department.
/// - Role-hierarchy enforced in the handler (defense in depth beyond the
///   controller's [Authorize(Roles=...)]): PlatformAdmin may grant any
///   role; InstitutionAdmin may grant anything except PlatformAdmin;
///   DepartmentAdmin may grant only Teacher/Student/Parent -- neither can
///   mint an admin account at or above their own level for someone else.
///
/// Bootstrapping note: the very first InstitutionAdmin for a newly
/// onboarded institution must be registered by a PlatformAdmin (who is
/// exempt from the tenant-scope check below). There is currently no
/// separate "invite" flow with an emailed acceptance link -- this command
/// performs immediate registration by a trusted, already-authenticated
/// staff member. A true invitation-token flow (SRS FR-AUTH-5's literal
/// wording) remains a documented follow-up, not something this fix
/// silently claims to provide.
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    UserRole Role,
    Guid? InstitutionId,
    Guid? DepartmentId
) : IRequest<Unit>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => DepartmentId;
}

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InstitutionId)
            .NotNull()
            .When(x => x.Role != UserRole.PlatformAdmin)
            .WithMessage("institutionId is required for every role except PlatformAdmin.");
    }
}

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Unit>
{
    private readonly IIdentityService _identity;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RegisterCommandHandler(IIdentityService identity, IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _identity = identity;
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        EnsureCallerMayGrantRole(request.Role);
        await EnsureDepartmentBelongsToInstitutionAsync(request.InstitutionId, request.DepartmentId, cancellationToken);

        // 1. Create the Supabase Auth identity (triggers Supabase's own
        //    confirmation email — Sequence Diagram 2). Supabase's returned
        //    id becomes this person's identifier everywhere in Schedulas.
        var supabaseUserId = await _identity.RegisterAsync(
            request.Email, request.Password, request.FullName,
            request.Role.ToString(), request.InstitutionId, request.DepartmentId, cancellationToken);

        // 2. Mirror it locally as a Profile row, using that same id as the
        //    primary key (Profile.Id == auth.users.id — never a separately
        //    generated one). Starts inactive; ConfirmEmail flips IsActive.
        var profile = new Profile(
            supabaseUserId, request.FullName, request.Email, request.Role,
            request.InstitutionId, request.DepartmentId);

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    /// <summary>
    /// Role-hierarchy check, independent of and in addition to
    /// TenantAuthorizationBehavior's institution/department match: even
    /// within their own institution, an InstitutionAdmin must not be able
    /// to grant PlatformAdmin, and a DepartmentAdmin must not be able to
    /// grant any admin role at all.
    /// </summary>
    private void EnsureCallerMayGrantRole(UserRole requestedRole)
    {
        var allowedRolesToGrant = _currentUser.Role switch
        {
            UserRole.PlatformAdmin => (UserRole[])[UserRole.PlatformAdmin, UserRole.InstitutionAdmin, UserRole.DepartmentAdmin, UserRole.Teacher, UserRole.Student, UserRole.Parent],
            UserRole.InstitutionAdmin => [UserRole.InstitutionAdmin, UserRole.DepartmentAdmin, UserRole.Teacher, UserRole.Student, UserRole.Parent],
            UserRole.DepartmentAdmin => [UserRole.Teacher, UserRole.Student, UserRole.Parent],
            _ => []
        };

        if (!allowedRolesToGrant.Contains(requestedRole))
            throw new UnauthorizedAccessException("CANNOT_GRANT_ROLE");
    }

    /// <summary>
    /// Prevents a Profile from being created with an InstitutionId and
    /// DepartmentId that don't actually match — e.g. InstitutionId set to
    /// the caller's own institution (passing the tenant check) while
    /// DepartmentId secretly points at a department belonging to a
    /// different institution entirely.
    /// </summary>
    private async Task EnsureDepartmentBelongsToInstitutionAsync(Guid? institutionId, Guid? departmentId, CancellationToken ct)
    {
        if (departmentId is not Guid deptId || institutionId is not Guid instId) return;

        var actualInstitutionId = await _db.Departments
            .Where(d => d.Id == deptId)
            .Select(d => (Guid?)d.InstitutionId)
            .FirstOrDefaultAsync(ct)
            ?? throw new Domain.Exceptions.EntityNotFoundException("Department", deptId);


    }
}

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(string AccessToken, string RefreshToken);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IIdentityService _identity;

    public LoginCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (access, refresh) = await _identity.LoginAsync(request.Email, request.Password, cancellationToken);
        return new LoginResult(access, refresh);
    }
}

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResult>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    private readonly IIdentityService _identity;

    public RefreshTokenCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var (access, refresh) = await _identity.RefreshAsync(request.RefreshToken, cancellationToken);
        return new LoginResult(access, refresh);
    }
}

public sealed record ForgotPasswordCommand(string Email) : IRequest<Unit>;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IIdentityService _identity;

    public ForgotPasswordCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        await _identity.RequestPasswordResetAsync(request.Email, cancellationToken);
        return Unit.Value;
    }
}

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Unit>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IIdentityService _identity;

    public ResetPasswordCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _identity.ResetPasswordAsync(request.Token, request.NewPassword, cancellationToken);
        return Unit.Value;
    }
}
