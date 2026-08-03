using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Features.Rules.Commands;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rules")]
[Authorize(Roles = "InstitutionAdmin,DepartmentAdmin")]
public sealed class RulesController : ControllerBase
{
    private readonly ISender _mediator;

    public RulesController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateRuleDefinitionCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Create), new { id = result.Id },
            ApiResponse<RuleDefinitionDto>.Ok(result, "تم إنشاء القاعدة بنجاح"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRuleDefinitionCommand body, CancellationToken ct)
    {
        var command = body with { RuleId = id };
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<RuleDefinitionDto>.Ok(result, "تم تحديث القاعدة بنجاح"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateRuleDefinitionCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إلغاء تفعيل القاعدة"));
    }
}
