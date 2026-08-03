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
[Route("api/v{version:apiVersion}/parents")]
public class ParentsController : ControllerBase
{
    private readonly ISender _mediator;
    public ParentsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ParentDto>> CreateParent(CreateParentCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ParentDto>> GetParentById(Guid id)
    {
        return await _mediator.Send(new GetParentByIdQuery(id));
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ParentDto>>> GetParents([FromQuery] GetParentsQuery query)
    {
        return await _mediator.Send(query);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteParent(Guid id)
    {
        await _mediator.Send(new DeleteParentCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/restore")]
    public async Task<ActionResult> RestoreParent(Guid id)
    {
        await _mediator.Send(new RestoreParentCommand(id));
        return NoContent();
    }
}
