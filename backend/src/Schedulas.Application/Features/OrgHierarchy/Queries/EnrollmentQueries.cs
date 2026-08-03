using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Features.People.Commands; // for StudentDto, TeacherDto
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;

namespace Schedulas.Application.Features.OrgHierarchy.Queries;

public sealed record GetClassStudentsQuery(Guid ClassId) : IRequest<IReadOnlyList<StudentDto>>;

public sealed class GetClassStudentsQueryHandler : IRequestHandler<GetClassStudentsQuery, IReadOnlyList<StudentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetClassStudentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<StudentDto>> Handle(GetClassStudentsQuery request, CancellationToken cancellationToken)
    {
        var cls = await _db.Classes.FindAsync([request.ClassId], cancellationToken)
            ?? throw new EntityNotFoundException("Class", request.ClassId);

        var query = from cs in _db.ClassStudents.AsNoTracking()
                    join s in _db.Students.AsNoTracking() on cs.StudentId equals s.Id
                    join p in _db.Profiles.AsNoTracking() on s.ProfileId equals p.Id
                    where cs.ClassId == request.ClassId
                    select new { s, p };

        var items = await query.ToListAsync(cancellationToken);

        return items.Select(x => new StudentDto(x.s.Id, x.s.ProfileId, x.s.InstitutionId, x.s.StudentNumber, x.p.FullName, x.p.Email, x.p.IsActive)).ToList();
    }
}

public sealed record GetClassTeachersQuery(Guid ClassId) : IRequest<IReadOnlyList<TeacherDto>>;

public sealed class GetClassTeachersQueryHandler : IRequestHandler<GetClassTeachersQuery, IReadOnlyList<TeacherDto>>
{
    private readonly IApplicationDbContext _db;

    public GetClassTeachersQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TeacherDto>> Handle(GetClassTeachersQuery request, CancellationToken cancellationToken)
    {
        var cls = await _db.Classes.FindAsync([request.ClassId], cancellationToken)
            ?? throw new EntityNotFoundException("Class", request.ClassId);

        var query = from ct in _db.ClassTeachers.AsNoTracking()
                    join t in _db.Teachers.AsNoTracking() on ct.TeacherId equals t.Id
                    join p in _db.Profiles.AsNoTracking() on t.ProfileId equals p.Id
                    where ct.ClassId == request.ClassId
                    select new { t, p };

        var items = await query.ToListAsync(cancellationToken);

        return items.Select(x => new TeacherDto(x.t.Id, x.t.ProfileId, x.t.InstitutionId, x.t.DepartmentId, x.p.FullName, x.p.Email, x.p.IsActive)).ToList();
    }
}

public sealed record GetParentStudentsQuery(Guid ParentId) : IRequest<IReadOnlyList<StudentDto>>;

public sealed class GetParentStudentsQueryHandler : IRequestHandler<GetParentStudentsQuery, IReadOnlyList<StudentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetParentStudentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<StudentDto>> Handle(GetParentStudentsQuery request, CancellationToken cancellationToken)
    {
        var parent = await _db.Parents.FindAsync([request.ParentId], cancellationToken)
            ?? throw new EntityNotFoundException("Parent", request.ParentId);

        var query = from link in _db.ParentStudentLinks.AsNoTracking()
                    join s in _db.Students.AsNoTracking() on link.StudentId equals s.Id
                    join p in _db.Profiles.AsNoTracking() on s.ProfileId equals p.Id
                    where link.ParentId == request.ParentId
                    select new { s, p };

        var items = await query.ToListAsync(cancellationToken);

        return items.Select(x => new StudentDto(x.s.Id, x.s.ProfileId, x.s.InstitutionId, x.s.StudentNumber, x.p.FullName, x.p.Email, x.p.IsActive)).ToList();
    }
}
