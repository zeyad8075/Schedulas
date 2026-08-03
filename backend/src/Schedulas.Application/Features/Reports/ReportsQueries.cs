using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.Reports;

// DTOs
public sealed record WorkloadReportDto(Guid EntityId, double TotalWorkloadMinutes, DateOnly StartDate, DateOnly EndDate);

public sealed record ActivityDistributionItemDto(ActivityType ActivityType, int Count);
public sealed record ActivityDistributionReportDto(Guid EntityId, DateOnly StartDate, DateOnly EndDate, List<ActivityDistributionItemDto> Distribution);

// Institution Workload
public sealed record GetInstitutionWorkloadQuery(Guid InstitutionId, DateOnly StartDate, DateOnly EndDate) 
    : IRequest<WorkloadReportDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetInstitutionWorkloadQueryHandler : IRequestHandler<GetInstitutionWorkloadQuery, WorkloadReportDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantService _tenantService;

    public GetInstitutionWorkloadQueryHandler(IApplicationDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<WorkloadReportDto> Handle(GetInstitutionWorkloadQuery request, CancellationToken cancellationToken)
    {
        _tenantService.SetTenantId(request.InstitutionId);

        var durations = await _db.Activities
            .AsNoTracking()
            .Where(a => a.InstitutionId == request.InstitutionId && a.ScheduledDate >= request.StartDate && a.ScheduledDate <= request.EndDate)
            .Where(a => a.Status != ActivityStatus.Cancelled) // Exclude cancelled
            .Select(a => a.Duration)
            .ToListAsync(cancellationToken);

        var totalMinutes = durations.Sum(d => d?.TotalMinutes ?? 0);

        return new WorkloadReportDto(request.InstitutionId, totalMinutes, request.StartDate, request.EndDate);
    }
}

// Teacher Workload
public sealed record GetTeacherWorkloadQuery(Guid InstitutionId, Guid TeacherId, DateOnly StartDate, DateOnly EndDate) 
    : IRequest<WorkloadReportDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetTeacherWorkloadQueryHandler : IRequestHandler<GetTeacherWorkloadQuery, WorkloadReportDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantService _tenantService;

    public GetTeacherWorkloadQueryHandler(IApplicationDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<WorkloadReportDto> Handle(GetTeacherWorkloadQuery request, CancellationToken cancellationToken)
    {
        _tenantService.SetTenantId(request.InstitutionId);

        var classIds = await _db.ClassTeachers
            .AsNoTracking()
            .Where(ct => ct.TeacherId == request.TeacherId)
            .Select(ct => ct.ClassId)
            .ToListAsync(cancellationToken);

        var durations = await _db.Activities
            .AsNoTracking()
            .Where(a => classIds.Contains(a.ClassId) && a.ScheduledDate >= request.StartDate && a.ScheduledDate <= request.EndDate)
            .Where(a => a.Status != ActivityStatus.Cancelled)
            .Select(a => a.Duration)
            .ToListAsync(cancellationToken);

        var totalMinutes = durations.Sum(d => d?.TotalMinutes ?? 0);

        return new WorkloadReportDto(request.TeacherId, totalMinutes, request.StartDate, request.EndDate);
    }
}

// Student Workload
public sealed record GetStudentWorkloadQuery(Guid InstitutionId, Guid StudentId, DateOnly StartDate, DateOnly EndDate) 
    : IRequest<WorkloadReportDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetStudentWorkloadQueryHandler : IRequestHandler<GetStudentWorkloadQuery, WorkloadReportDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantService _tenantService;

    public GetStudentWorkloadQueryHandler(IApplicationDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<WorkloadReportDto> Handle(GetStudentWorkloadQuery request, CancellationToken cancellationToken)
    {
        _tenantService.SetTenantId(request.InstitutionId);

        var classIds = await _db.ClassStudents
            .AsNoTracking()
            .Where(cs => cs.StudentId == request.StudentId)
            .Select(cs => cs.ClassId)
            .ToListAsync(cancellationToken);

        var durations = await _db.Activities
            .AsNoTracking()
            .Where(a => classIds.Contains(a.ClassId) && a.ScheduledDate >= request.StartDate && a.ScheduledDate <= request.EndDate)
            .Where(a => a.Status != ActivityStatus.Cancelled)
            .Select(a => a.Duration)
            .ToListAsync(cancellationToken);

        var totalMinutes = durations.Sum(d => d?.TotalMinutes ?? 0);

        return new WorkloadReportDto(request.StudentId, totalMinutes, request.StartDate, request.EndDate);
    }
}

// Activity Distribution
public sealed record GetActivityDistributionQuery(Guid InstitutionId, Guid ClassId, DateOnly StartDate, DateOnly EndDate) 
    : IRequest<ActivityDistributionReportDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetActivityDistributionQueryHandler : IRequestHandler<GetActivityDistributionQuery, ActivityDistributionReportDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantService _tenantService;

    public GetActivityDistributionQueryHandler(IApplicationDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<ActivityDistributionReportDto> Handle(GetActivityDistributionQuery request, CancellationToken cancellationToken)
    {
        _tenantService.SetTenantId(request.InstitutionId);

        var distribution = await _db.Activities
            .AsNoTracking()
            .Where(a => a.ClassId == request.ClassId && a.ScheduledDate >= request.StartDate && a.ScheduledDate <= request.EndDate)
            .Where(a => a.Status != ActivityStatus.Cancelled)
            .GroupBy(a => a.ActivityType)
            .Select(g => new ActivityDistributionItemDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return new ActivityDistributionReportDto(request.ClassId, request.StartDate, request.EndDate, distribution);
    }
}
