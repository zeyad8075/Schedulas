using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.People.Commands; // for ParentDto
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;

namespace Schedulas.Application.Features.People.Queries;

public sealed record GetParentByIdQuery(Guid ParentId) : IRequest<ParentDto>;

public sealed class GetParentByIdQueryHandler : IRequestHandler<GetParentByIdQuery, ParentDto>
{
    private readonly IApplicationDbContext _db;
    // Parent does not have InstitutionId natively, but if needed we could restrict access.
    // Assuming anyone with a valid token can get a parent's basic info, or we can restrict it if needed.

    public GetParentByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ParentDto> Handle(GetParentByIdQuery request, CancellationToken cancellationToken)
    {
        var parent = await _db.Parents.FindAsync([request.ParentId], cancellationToken)
            ?? throw new EntityNotFoundException("Parent", request.ParentId);

        var profile = await _db.Profiles.FindAsync([parent.ProfileId], cancellationToken);
        return new ParentDto(parent.Id, parent.ProfileId, profile!.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record GetParentsQuery(
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<ParentDto>>;

public sealed class GetParentsQueryHandler : IRequestHandler<GetParentsQuery, PaginatedList<ParentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetParentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<ParentDto>> Handle(GetParentsQuery request, CancellationToken cancellationToken)
    {
        var query = from p in _db.Parents.AsNoTracking()
                    join prof in _db.Profiles.AsNoTracking() on p.ProfileId equals prof.Id
                    select new { p, prof };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.prof.FullName.ToLower().Contains(search) || 
                x.prof.Email.ToLower().Contains(search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "fullname" => request.SortDescending ? query.OrderByDescending(x => x.prof.FullName) : query.OrderBy(x => x.prof.FullName),
            "email" => request.SortDescending ? query.OrderByDescending(x => x.prof.Email) : query.OrderBy(x => x.prof.Email),
            _ => request.SortDescending ? query.OrderByDescending(x => x.prof.FullName) : query.OrderBy(x => x.prof.FullName)
        };

        var count = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x => new ParentDto(x.p.Id, x.p.ProfileId, x.prof.FullName, x.prof.Email, x.prof.IsActive)).ToList();

        return new PaginatedList<ParentDto>(dtos, count, request.PageNumber, request.PageSize);
    }
}
