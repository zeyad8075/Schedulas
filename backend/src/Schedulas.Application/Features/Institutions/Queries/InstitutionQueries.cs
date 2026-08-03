using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.Institutions.Commands;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.Institutions.Queries;

public sealed record GetInstitutionsQuery(
    string? SearchTerm = null,
    bool? IsSuspended = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1, 
    int PageSize = 20) : IRequest<PaginatedList<InstitutionDto>>;

public sealed class GetInstitutionsQueryHandler : IRequestHandler<GetInstitutionsQuery, PaginatedList<InstitutionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetInstitutionsQueryHandler(IApplicationDbContext db) => _db = db;

    public Task<PaginatedList<InstitutionDto>> Handle(GetInstitutionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Institutions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(i => i.Name.Contains(request.SearchTerm));

        if (request.IsSuspended.HasValue)
            query = query.Where(i => i.IsSuspended == request.IsSuspended.Value);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
            "createdat" => request.SortDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name)
        };

        var projected = query.Select(i => new InstitutionDto(i.Id, i.Name, i.Type, i.Timezone, i.LogoUrl, i.IsSuspended));

        return PaginatedList<InstitutionDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

/// <summary>SECURITY FIX (Phase 8 Verification pass): same gap as UpdateInstitutionCommand, fixed the same way.</summary>
public sealed record GetInstitutionByIdQuery(Guid InstitutionId) : IRequest<InstitutionDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetInstitutionByIdQueryHandler : IRequestHandler<GetInstitutionByIdQuery, InstitutionDto>
{
    private readonly IApplicationDbContext _db;

    public GetInstitutionByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<InstitutionDto> Handle(GetInstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        var institution = await _db.Institutions.FirstOrDefaultAsync(i => i.Id == request.InstitutionId, cancellationToken)
            ?? throw new EntityNotFoundException("Institution", request.InstitutionId);

        return new InstitutionDto(institution.Id, institution.Name, institution.Type,
            institution.Timezone, institution.LogoUrl, institution.IsSuspended);
    }
}
