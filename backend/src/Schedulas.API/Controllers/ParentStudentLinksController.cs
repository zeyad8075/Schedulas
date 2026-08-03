using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Features.OrgHierarchy.Commands;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parent-student-links")]
[Authorize(Roles = "InstitutionAdmin,DepartmentAdmin")]
public sealed class ParentStudentLinksController : ControllerBase
{
    private readonly ISender _mediator;
    public ParentStudentLinksController(ISender mediator) => _mediator = mediator;

    [HttpGet("{id:guid}/students")]
    public async Task<IActionResult> GetParentStudents(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new Schedulas.Application.Features.OrgHierarchy.Queries.GetParentStudentsQuery(id), ct);
        return Ok(ApiResponse<IReadOnlyList<Schedulas.Application.Features.People.Commands.StudentDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(LinkParentToStudentCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم ربط ولي الأمر بالطالب بنجاح"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new UnlinkParentFromStudentCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إلغاء ربط ولي الأمر بالطالب"));
    }
}
