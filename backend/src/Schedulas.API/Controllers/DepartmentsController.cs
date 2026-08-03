using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.OrgHierarchy.Commands;
using Schedulas.Application.Features.OrgHierarchy.Queries;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/departments")]
[Authorize(Roles = "InstitutionAdmin,DepartmentAdmin")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly ISender _mediator;
    public DepartmentsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid institutionId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDepartmentsQuery(institutionId, searchTerm, isActive, sortBy, sortDescending, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<DepartmentDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> Create(CreateDepartmentCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(List), new { institutionId = result.InstitutionId },
            ApiResponse<DepartmentDto>.Ok(result, "تم إنشاء القسم بنجاح"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> Update(Guid id, UpdateDepartmentCommand body, CancellationToken ct)
    {
        var command = body with { DepartmentId = id };
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<DepartmentDto>.Ok(result, "تم تحديث القسم بنجاح"));
    }

    [HttpGet("{id:guid}/programs")]
    public async Task<IActionResult> ListPrograms(
        Guid id,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProgramsQuery(id, searchTerm, isActive, sortBy, sortDescending, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<ProgramDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDepartmentByIdQuery(id), ct);
        return Ok(ApiResponse<DepartmentDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDepartmentCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف القسم بنجاح"));
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new RestoreDepartmentCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم استعادة القسم بنجاح"));
    }
}
