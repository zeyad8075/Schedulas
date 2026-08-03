using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Features.AcademicCalendar;
using Schedulas.Application.Common.Models;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public sealed class AcademicCalendarController : ControllerBase
{
    private readonly ISender _mediator;
    public AcademicCalendarController(ISender mediator) => _mediator = mediator;

    [HttpGet("academic-terms")]
    public async Task<IActionResult> ListTerms(
        [FromQuery] Guid institutionId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAcademicTermsQuery(institutionId, searchTerm, isActive, sortBy, sortDescending, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<AcademicTermDto>>.Ok(result));
    }

    [HttpGet("academic-terms/{id:guid}")]
    public async Task<IActionResult> GetTermById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAcademicTermByIdQuery(id), ct);
        return Ok(ApiResponse<AcademicTermDto>.Ok(result));
    }

    [HttpPost("academic-terms")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> CreateTerm(CreateAcademicTermCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(ListTerms), new { institutionId = result.InstitutionId },
            ApiResponse<AcademicTermDto>.Ok(result, "تم إنشاء الفصل الدراسي بنجاح"));
    }

    [HttpPut("academic-terms/{id:guid}")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> UpdateTerm(Guid id, [FromQuery] bool isActive, CancellationToken ct)
    {
        await _mediator.Send(new UpdateAcademicTermCommand(id, isActive), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم تحديث حالة الفصل الدراسي"));
    }

    [HttpDelete("academic-terms/{id:guid}")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> DeleteTerm(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteAcademicTermCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف الفصل الدراسي بنجاح"));
    }

    [HttpPost("academic-terms/{id:guid}/restore")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> RestoreTerm(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new RestoreAcademicTermCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم استعادة الفصل الدراسي بنجاح"));
    }

    [HttpGet("holidays")]
    public async Task<IActionResult> ListHolidays([FromQuery] Guid institutionId, [FromQuery] Guid? termId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHolidaysQuery(institutionId, termId), ct);
        return Ok(ApiResponse<IReadOnlyList<HolidayDto>>.Ok(result));
    }

    [HttpPost("holidays")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> CreateHoliday(CreateHolidayCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(ListHolidays), new { institutionId = result.InstitutionId },
            ApiResponse<HolidayDto>.Ok(result, "تمت إضافة العطلة بنجاح"));
    }

    [HttpDelete("holidays/{id:guid}")]
    [Authorize(Roles = "InstitutionAdmin")]
    public async Task<IActionResult> DeleteHoliday(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteHolidayCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف العطلة"));
    }
}
