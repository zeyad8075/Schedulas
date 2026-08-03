using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.People.Commands; // for StudentDto
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;

namespace Schedulas.Application.Features.People.Queries;

public sealed record GetStudentByIdQuery(Guid StudentId) : IRequest<StudentDto>;

public sealed class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetStudentByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _db.Students.FindAsync([request.StudentId], cancellationToken)
            ?? throw new EntityNotFoundException("Student", request.StudentId);

        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != student.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        var profile = await _db.Profiles.FindAsync([student.ProfileId], cancellationToken);
        return new StudentDto(student.Id, student.ProfileId, student.InstitutionId, student.StudentNumber, profile!.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record GetStudentsQuery(
    Guid InstitutionId,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<StudentDto>>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, PaginatedList<StudentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetStudentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<StudentDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        var query = from s in _db.Students.AsNoTracking()
                    join p in _db.Profiles.AsNoTracking() on s.ProfileId equals p.Id
                    where s.InstitutionId == request.InstitutionId
                    select new { s, p };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.p.FullName.ToLower().Contains(search) || 
                x.p.Email.ToLower().Contains(search) ||
                (x.s.StudentNumber != null && x.s.StudentNumber.ToLower().Contains(search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "fullname" => request.SortDescending ? query.OrderByDescending(x => x.p.FullName) : query.OrderBy(x => x.p.FullName),
            "studentnumber" => request.SortDescending ? query.OrderByDescending(x => x.s.StudentNumber) : query.OrderBy(x => x.s.StudentNumber),
            "email" => request.SortDescending ? query.OrderByDescending(x => x.p.Email) : query.OrderBy(x => x.p.Email),
            _ => request.SortDescending ? query.OrderByDescending(x => x.p.FullName) : query.OrderBy(x => x.p.FullName)
        };

        var count = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x => new StudentDto(x.s.Id, x.s.ProfileId, x.s.InstitutionId, x.s.StudentNumber, x.p.FullName, x.p.Email, x.p.IsActive)).ToList();

        return new PaginatedList<StudentDto>(dtos, count, request.PageNumber, request.PageSize);
    }
}
