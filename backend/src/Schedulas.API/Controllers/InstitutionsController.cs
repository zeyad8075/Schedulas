using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.Institutions.Commands;
using Schedulas.Application.Features.Institutions.Queries;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/institutions")]
[Authorize]
public sealed class InstitutionsController : ControllerBase
{
    private readonly ISender _mediator;
    public InstitutionsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> List(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isSuspended,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInstitutionsQuery(searchTerm, isSuspended, sortBy, sortDescending, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<InstitutionDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> Create(CreateInstitutionCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<InstitutionDto>.Ok(result, "تم إنشاء المؤسسة بنجاح"));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "PlatformAdmin,InstitutionAdmin")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInstitutionByIdQuery(id), ct);
        return Ok(ApiResponse<InstitutionDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "PlatformAdmin,InstitutionAdmin")]
    public async Task<IActionResult> Update(Guid id, UpdateInstitutionCommand body, CancellationToken ct)
    {
        var command = body with { InstitutionId = id };
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<InstitutionDto>.Ok(result, "تم تحديث بيانات المؤسسة بنجاح"));
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new SuspendInstitutionCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم تعليق المؤسسة"));
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ReactivateInstitutionCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إعادة تفعيل المؤسسة"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteInstitutionCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف المؤسسة بنجاح"));
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new RestoreInstitutionCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم استعادة المؤسسة بنجاح"));
    }
}
