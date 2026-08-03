using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.People.Commands;
using Schedulas.Application.Features.People.Queries;
using Schedulas.Domain.Enums;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly ISender _mediator;
    public ProfilesController(ISender mediator) => _mediator = mediator;

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMyProfile()
    {
        return await _mediator.Send(new GetMyProfileQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProfileDto>> GetProfileById(Guid id)
    {
        return await _mediator.Send(new GetProfileByIdQuery(id));
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ProfileDto>>> GetProfiles([FromQuery] GetProfilesQuery query)
    {
        return await _mediator.Send(query);
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateMyProfile(UpdateProfileCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPut("me/theme")]
    public async Task<ActionResult> UpdateMyTheme(UpdateThemeCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/suspend")]
    public async Task<ActionResult> SuspendProfile(Guid id)
    {
        await _mediator.Send(new SuspendProfileCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<ActionResult> ActivateProfile(Guid id)
    {
        await _mediator.Send(new ActivateProfileCommand(id));
        return NoContent();
    }
}
