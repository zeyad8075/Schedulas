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
[Route("api/v{version:apiVersion}/programs")]
[Authorize(Roles = "InstitutionAdmin,DepartmentAdmin")]
public sealed class ProgramsController : ControllerBase
{
    private readonly ISender _mediator;
    public ProgramsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateProgramCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(ListCourses), new { id = result.Id },
            ApiResponse<ProgramDto>.Ok(result, "تم إنشاء البرنامج بنجاح"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProgramCommand body, CancellationToken ct)
    {
        var command = body with { ProgramId = id };
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<ProgramDto>.Ok(result, "تم تحديث البرنامج بنجاح"));
    }

    [HttpGet("{id:guid}/courses")]
    public async Task<IActionResult> ListCourses(
        Guid id,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCoursesQuery(id, searchTerm, isActive, sortBy, sortDescending, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<CourseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProgramByIdQuery(id), ct);
        return Ok(ApiResponse<ProgramDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProgramCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف البرنامج بنجاح"));
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new RestoreProgramCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم استعادة البرنامج بنجاح"));
    }
}
