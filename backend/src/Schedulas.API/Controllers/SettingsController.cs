using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Features.Settings;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/settings")]
[Authorize]
public sealed class SettingsController : ControllerBase
{
    private readonly ISender _mediator;
    public SettingsController(ISender mediator) => _mediator = mediator;

    [HttpGet("institution")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> GetInstitutionSettings(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInstitutionSettingsQuery(), ct);
        return Ok(ApiResponse<InstitutionSettingsDto>.Ok(result));
    }

    [HttpPut("institution")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> UpdateInstitutionSettings(UpdateInstitutionSettingsCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<InstitutionSettingsDto>.Ok(result, "تم تحديث إعدادات المؤسسة بنجاح"));
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserSettings(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserSettingsQuery(), ct);
        return Ok(ApiResponse<UserSettingsDto>.Ok(result));
    }

    [HttpPut("user")]
    public async Task<IActionResult> UpdateUserSettings(UpdateUserSettingsCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<UserSettingsDto>.Ok(result, "تم تحديث الإعدادات الشخصية بنجاح"));
    }
}
