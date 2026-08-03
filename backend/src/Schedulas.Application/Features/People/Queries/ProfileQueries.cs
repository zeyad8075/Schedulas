using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.People.Commands; // for ProfileDto
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;

namespace Schedulas.Application.Features.People.Queries;

public sealed record GetMyProfileQuery() : IRequest<ProfileDto>;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ProfileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyProfileQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([_currentUser.UserId!.Value], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", _currentUser.UserId!.Value);
            
        return UpdateProfileCommandHandler.ToDto(profile);
    }
}

public sealed record GetProfileByIdQuery(Guid ProfileId) : IRequest<ProfileDto>;

public sealed class GetProfileByIdQueryHandler : IRequestHandler<GetProfileByIdQuery, ProfileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProfileByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProfileDto> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([request.ProfileId], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", request.ProfileId);

        if (_currentUser.Role != UserRole.PlatformAdmin && _currentUser.InstitutionId != profile.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        return UpdateProfileCommandHandler.ToDto(profile);
    }
}

public sealed record GetProfilesQuery(
    Guid InstitutionId,
    string? SearchTerm = null,
    UserRole? Role = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<ProfileDto>>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetProfilesQueryHandler : IRequestHandler<GetProfilesQuery, PaginatedList<ProfileDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProfilesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<ProfileDto>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Profiles.AsNoTracking().Where(p => p.InstitutionId == request.InstitutionId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(p => p.FullName.ToLower().Contains(search) || p.Email.ToLower().Contains(search));
        }

        if (request.Role.HasValue)
        {
            query = query.Where(p => p.Role == request.Role.Value);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "fullname" => request.SortDescending ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName),
            "email" => request.SortDescending ? query.OrderByDescending(p => p.Email) : query.OrderBy(p => p.Email),
            "role" => request.SortDescending ? query.OrderByDescending(p => p.Role) : query.OrderBy(p => p.Role),
            "isactive" => request.SortDescending ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
            _ => request.SortDescending ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName)
        };

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProfileDto>(
            items.Select(UpdateProfileCommandHandler.ToDto).ToList(),
            count,
            request.PageNumber,
            request.PageSize);
    }
}
