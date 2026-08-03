using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.People.Commands;
using Schedulas.Application.Features.People.Queries;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teachers")]
public class TeachersController : ControllerBase
{
    private readonly ISender _mediator;
    public TeachersController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<TeacherDto>> CreateTeacher(CreateTeacherCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeacherDto>> GetTeacherById(Guid id)
    {
        return await _mediator.Send(new GetTeacherByIdQuery(id));
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<TeacherDto>>> GetTeachers([FromQuery] GetTeachersQuery query)
    {
        return await _mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TeacherDto>> UpdateTeacher(Guid id, UpdateTeacherCommand command)
    {
        if (id != command.TeacherId) return BadRequest();
        return await _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTeacher(Guid id)
    {
        await _mediator.Send(new DeleteTeacherCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/restore")]
    public async Task<ActionResult> RestoreTeacher(Guid id)
    {
        await _mediator.Send(new RestoreTeacherCommand(id));
        return NoContent();
    }
}
