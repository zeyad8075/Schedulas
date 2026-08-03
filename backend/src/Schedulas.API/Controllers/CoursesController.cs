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
[Route("api/v{version:apiVersion}/courses")]
[Authorize(Roles = "InstitutionAdmin,DepartmentAdmin")]
public sealed class CoursesController : ControllerBase
{
    private readonly ISender _mediator;
    public CoursesController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateCourseCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(ListClasses), new { id = result.Id },
            ApiResponse<CourseDto>.Ok(result, "تم إنشاء المقرر بنجاح"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCourseCommand body, CancellationToken ct)
    {
        var command = body with { CourseId = id };
        var result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<CourseDto>.Ok(result, "تم تحديث المقرر بنجاح"));
    }

    [HttpGet("{id:guid}/classes")]
    [Authorize(Roles = "InstitutionAdmin,DepartmentAdmin,Teacher")]
    public async Task<IActionResult> ListClasses(
        Guid id,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetClassesQuery(id, searchTerm, isActive, sortBy, sortDescending, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<ClassDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseByIdQuery(id), ct);
        return Ok(ApiResponse<CourseDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCourseCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف المقرر بنجاح"));
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new RestoreCourseCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم استعادة المقرر بنجاح"));
    }
}
