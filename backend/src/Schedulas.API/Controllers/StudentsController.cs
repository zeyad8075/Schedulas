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
[Route("api/v{version:apiVersion}/students")]
public class StudentsController : ControllerBase
{
    private readonly ISender _mediator;
    public StudentsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDto>> GetStudentById(Guid id)
    {
        return await _mediator.Send(new GetStudentByIdQuery(id));
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<StudentDto>>> GetStudents([FromQuery] GetStudentsQuery query)
    {
        return await _mediator.Send(query);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StudentDto>> UpdateStudent(Guid id, UpdateStudentCommand command)
    {
        if (id != command.StudentId) return BadRequest();
        return await _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStudent(Guid id)
    {
        await _mediator.Send(new DeleteStudentCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/restore")]
    public async Task<ActionResult> RestoreStudent(Guid id)
    {
        await _mediator.Send(new RestoreStudentCommand(id));
        return NoContent();
    }
}
