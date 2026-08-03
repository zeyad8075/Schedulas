using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Features.Calendar;
using Schedulas.Application.Features.Calendar.Queries;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/calendar")]
[Authorize]
public sealed class CalendarController : ControllerBase
{
    private readonly ISender _mediator;

    public CalendarController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyCalendar(
        [FromQuery] Guid institutionId,
        [FromQuery] DateOnly date,
        [FromQuery] Guid? teacherId,
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? classId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDailyCalendarQuery(institutionId, date, teacherId, studentId, classId), ct);
        return Ok(ApiResponse<DailyCalendarDto>.Ok(result));
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyCalendar(
        [FromQuery] Guid institutionId,
        [FromQuery] DateOnly date,
        [FromQuery] Guid? teacherId,
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? classId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWeeklyCalendarQuery(institutionId, date, teacherId, studentId, classId), ct);
        return Ok(ApiResponse<WeeklyCalendarDto>.Ok(result));
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyCalendar(
        [FromQuery] Guid institutionId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] Guid? teacherId,
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? classId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMonthlyCalendarQuery(institutionId, year, month, teacherId, studentId, classId), ct);
        return Ok(ApiResponse<MonthlyCalendarDto>.Ok(result));
    }
}
