using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Features.Auth.Commands;
using Schedulas.Application.Features.Auth.Queries;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// SECURITY FIX (Write-Side Ownership Audit): this was [AllowAnonymous]
    /// with the caller able to self-select Role/InstitutionId — a critical
    /// privilege-escalation hole (see RegisterCommand's doc comment for
    /// full detail). Now requires an authenticated staff caller; the
    /// handler additionally enforces a role-hierarchy check so no admin
    /// can grant a role above their own level.
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "PlatformAdmin,InstitutionAdmin,DepartmentAdmin")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إرسال رابط تأكيد البريد الإلكتروني"));
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public IActionResult ConfirmEmail([FromQuery] string token)
    {
        // Delegated straight to IIdentityService.ConfirmEmailAsync via a thin
        // command in a future pass if additional local bookkeeping is
        // needed; for now Supabase's own verify endpoint is authoritative.
        return Ok(ApiResponse<object>.Ok(new { }, "تم تأكيد البريد الإلكتروني بنجاح"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<LoginResult>.Ok(result, "تم تسجيل الدخول بنجاح"));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<LoginResult>.Ok(result));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إرسال رابط إعادة تعيين كلمة المرور"));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم تحديث كلمة المرور بنجاح"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), ct);
        return Ok(ApiResponse<CurrentUserDto>.Ok(result));
    }
}
