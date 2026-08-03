using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.Activities;
using Schedulas.Application.Features.Activities.Commands;
using Schedulas.Application.Features.Activities.Queries;
using Schedulas.Domain.Enums;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/activities")]
[Authorize]
public sealed class ActivitiesController : ControllerBase
{
    private readonly ISender _mediator;

    public ActivitiesController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid institutionId,
        [FromQuery] Guid? classId,
        [FromQuery] ActivityType? type,
        [FromQuery] ActivityStatus? status,
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetActivitiesQuery(institutionId, classId, type, status, searchTerm, sortBy, sortDescending, dateFrom, dateTo, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<ActivityDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActivityByIdQuery(id), ct);
        return Ok(ApiResponse<ActivityDto>.Ok(result));
    }

    /// <summary>
    /// Per API Design §8 / Constitution §17: Teacher and Department Admin
    /// only. Role enforcement here is the first gate; TenantAuthorizationBehavior
    /// in the Application pipeline is the second (Architecture §6).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,DepartmentAdmin")]
    public async Task<IActionResult> Create(CreateActivityCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (result.RequiresOverride)
        {
            var envelope = ApiResponse<ActivitySubmissionResult>.Fail(Common.ArabicMessages.Resolve(result.ReasonCode))
                with { Data = result };
            return UnprocessableEntity(envelope);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Activity!.Id },
            ApiResponse<ActivitySubmissionResult>.Ok(result, "تمت جدولة النشاط بنجاح"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,DepartmentAdmin")]
    public async Task<IActionResult> Edit(Guid id, EditActivityCommand body, CancellationToken ct)
    {
        var command = body with { ActivityId = id };
        var result = await _mediator.Send(command, ct);

        if (result.RequiresOverride)
        {
            var envelope = ApiResponse<ActivitySubmissionResult>.Fail(Common.ArabicMessages.Resolve(result.ReasonCode))
                with { Data = result };
            return UnprocessableEntity(envelope);
        }

        return Ok(ApiResponse<ActivitySubmissionResult>.Ok(result, "تم تحديث النشاط بنجاح"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher,DepartmentAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteActivityCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف النشاط بنجاح"));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Teacher,DepartmentAdmin")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelActivityCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم إلغاء النشاط"));
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles = "Teacher,DepartmentAdmin")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new RestoreActivityCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم استعادة النشاط بنجاح"));
    }

    /// <summary>Department Admin and above only, per Constitution §18 / API Design §8.</summary>
    [HttpPost("override")]
    [Authorize(Roles = "DepartmentAdmin,InstitutionAdmin")]
    public async Task<IActionResult> Override(OverrideActivityCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ActivityDto>.Ok(result, "تمت الموافقة على جدولة النشاط بشكل استثنائي"));
    }
}
