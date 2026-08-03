using MediatR;
using Microsoft.AspNetCore.Mvc;
using Schedulas.Application.Features.Reports;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Schedulas.API.Common;

namespace Schedulas.API.Controllers;

/// <summary>
/// Handles all reporting and workload analytics endpoints.
/// </summary>
[ApiController]
[Route("institutions/{institutionId}/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the aggregated workload for the entire institution over a date range.
    /// </summary>
    [HttpGet("workload/institution")]
    public async Task<ActionResult<ApiResponse<WorkloadReportDto>>> GetInstitutionWorkload(
        [FromRoute] Guid institutionId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInstitutionWorkloadQuery(institutionId, startDate, endDate), cancellationToken);
        return Ok(ApiResponse<WorkloadReportDto>.Ok(result));
    }

    /// <summary>
    /// Gets the aggregated workload for a specific teacher over a date range.
    /// </summary>
    [HttpGet("workload/teachers/{teacherId}")]
    public async Task<ActionResult<ApiResponse<WorkloadReportDto>>> GetTeacherWorkload(
        [FromRoute] Guid institutionId,
        [FromRoute] Guid teacherId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeacherWorkloadQuery(institutionId, teacherId, startDate, endDate), cancellationToken);
        return Ok(ApiResponse<WorkloadReportDto>.Ok(result));
    }

    /// <summary>
    /// Gets the aggregated workload for a specific student over a date range.
    /// </summary>
    [HttpGet("workload/students/{studentId}")]
    public async Task<ActionResult<ApiResponse<WorkloadReportDto>>> GetStudentWorkload(
        [FromRoute] Guid institutionId,
        [FromRoute] Guid studentId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudentWorkloadQuery(institutionId, studentId, startDate, endDate), cancellationToken);
        return Ok(ApiResponse<WorkloadReportDto>.Ok(result));
    }

    /// <summary>
    /// Gets the distribution of activities for a specific class over a date range.
    /// </summary>
    [HttpGet("distribution/classes/{classId}")]
    public async Task<ActionResult<ApiResponse<ActivityDistributionReportDto>>> GetActivityDistribution(
        [FromRoute] Guid institutionId,
        [FromRoute] Guid classId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetActivityDistributionQuery(institutionId, classId, startDate, endDate), cancellationToken);
        return Ok(ApiResponse<ActivityDistributionReportDto>.Ok(result));
    }
}
