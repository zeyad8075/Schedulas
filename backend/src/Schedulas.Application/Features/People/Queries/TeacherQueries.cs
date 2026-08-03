using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.People.Commands; // for TeacherDto
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;

namespace Schedulas.Application.Features.People.Queries;

public sealed record GetTeacherByIdQuery(Guid TeacherId) : IRequest<TeacherDto>;

public sealed class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, TeacherDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTeacherByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TeacherDto> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _db.Teachers.FindAsync([request.TeacherId], cancellationToken)
            ?? throw new EntityNotFoundException("Teacher", request.TeacherId);

        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != teacher.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        var profile = await _db.Profiles.FindAsync([teacher.ProfileId], cancellationToken);
        return new TeacherDto(teacher.Id, teacher.ProfileId, teacher.InstitutionId, teacher.DepartmentId, profile!.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record GetTeachersQuery(
    Guid InstitutionId,
    Guid? DepartmentId = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<TeacherDto>>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => DepartmentId;
}

public sealed class GetTeachersQueryHandler : IRequestHandler<GetTeachersQuery, PaginatedList<TeacherDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTeachersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<TeacherDto>> Handle(GetTeachersQuery request, CancellationToken cancellationToken)
    {
        var query = from t in _db.Teachers.AsNoTracking()
                    join p in _db.Profiles.AsNoTracking() on t.ProfileId equals p.Id
                    where t.InstitutionId == request.InstitutionId
                    select new { t, p };

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.t.DepartmentId == request.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.p.FullName.ToLower().Contains(search) || 
                x.p.Email.ToLower().Contains(search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "fullname" => request.SortDescending ? query.OrderByDescending(x => x.p.FullName) : query.OrderBy(x => x.p.FullName),
            "email" => request.SortDescending ? query.OrderByDescending(x => x.p.Email) : query.OrderBy(x => x.p.Email),
            _ => request.SortDescending ? query.OrderByDescending(x => x.p.FullName) : query.OrderBy(x => x.p.FullName)
        };

        var count = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x => new TeacherDto(x.t.Id, x.t.ProfileId, x.t.InstitutionId, x.t.DepartmentId, x.p.FullName, x.p.Email, x.p.IsActive)).ToList();

        return new PaginatedList<TeacherDto>(dtos, count, request.PageNumber, request.PageSize);
    }
}
