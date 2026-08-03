using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.OrgHierarchy.Commands;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.OrgHierarchy.Queries;

// SECURITY FIX (Phase 8 Verification pass): all four queries below
// previously trusted the caller-supplied parent id (InstitutionId /
// DepartmentId / ProgramId / CourseId) with no check that it actually
// belonged to the caller's own institution -- the same class of gap fixed
// in the Activities and Reports features. Each now verifies the chain up
// to InstitutionId before returning any data.

public sealed record GetDepartmentsQuery(
    Guid InstitutionId,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<DepartmentDto>>;

public sealed class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, PaginatedList<DepartmentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantService _tenantService;

    public GetDepartmentsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ITenantService tenantService)
    {
        _db = db;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public Task<PaginatedList<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {

        if (_currentUser.Role == UserRole.PlatformAdmin)
            _tenantService.SetTenantId(request.InstitutionId);

        var query = _db.Departments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(d => d.Name.Contains(request.SearchTerm));

        if (request.IsActive.HasValue)
            query = query.Where(d => d.IsActive == request.IsActive.Value);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "createdat" => request.SortDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name)
        };

        var projected = query.Select(d => new DepartmentDto(d.Id, d.InstitutionId, d.Name, d.IsActive));

        return PaginatedList<DepartmentDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public sealed record GetDepartmentByIdQuery(Guid DepartmentId) : IRequest<DepartmentDto>;

public sealed class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _db.Departments.FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken)
            ?? throw new EntityNotFoundException("Department", request.DepartmentId);


        return new DepartmentDto(department.Id, department.InstitutionId, department.Name, department.IsActive);
    }
}

public sealed record GetProgramsQuery(
    Guid DepartmentId,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<ProgramDto>>;

public sealed class GetProgramsQueryHandler : IRequestHandler<GetProgramsQuery, PaginatedList<ProgramDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProgramsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ProgramDto>> Handle(GetProgramsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.PlatformAdmin)
        {
            var departmentInstitutionId = await _db.Departments
                .Where(d => d.Id == request.DepartmentId)
                .Select(d => (Guid?)d.InstitutionId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException("Department", request.DepartmentId);


        }

        var query = _db.Programs.Where(p => p.DepartmentId == request.DepartmentId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(p => p.Name.Contains(request.SearchTerm));

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "createdat" => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var projected = query.Select(p => new ProgramDto(p.Id, p.DepartmentId, p.Name, p.IsActive));

        return await PaginatedList<ProgramDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public sealed record GetProgramByIdQuery(Guid ProgramId) : IRequest<ProgramDto>;

public sealed class GetProgramByIdQueryHandler : IRequestHandler<GetProgramByIdQuery, ProgramDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProgramByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProgramDto> Handle(GetProgramByIdQuery request, CancellationToken cancellationToken)
    {
        var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == request.ProgramId, cancellationToken)
            ?? throw new EntityNotFoundException("Program", request.ProgramId);

        if (_currentUser.Role != UserRole.PlatformAdmin)
        {
            var departmentInstitutionId = await _db.Departments
                .Where(d => d.Id == program.DepartmentId)
                .Select(d => (Guid?)d.InstitutionId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException("Department", program.DepartmentId);


        }

        return new ProgramDto(program.Id, program.DepartmentId, program.Name, program.IsActive);
    }
}

public sealed record GetCoursesQuery(
    Guid ProgramId,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<CourseDto>>;

public sealed class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, PaginatedList<CourseDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCoursesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.PlatformAdmin)
        {
            var programInstitutionId = await (
                from p in _db.Programs
                join d in _db.Departments on p.DepartmentId equals d.Id
                where p.Id == request.ProgramId
                select (Guid?)d.InstitutionId
            ).FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException("Program", request.ProgramId);


        }

        var query = _db.Courses.Where(c => c.ProgramId == request.ProgramId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c => c.Name.Contains(request.SearchTerm) || (c.Code != null && c.Code.Contains(request.SearchTerm)));

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "code" => request.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "createdat" => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        var projected = query.Select(c => new CourseDto(c.Id, c.ProgramId, c.Name, c.Code, c.IsActive));

        return await PaginatedList<CourseDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public sealed record GetCourseByIdQuery(Guid CourseId) : IRequest<CourseDto>;

public sealed class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCourseByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CourseDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken)
            ?? throw new EntityNotFoundException("Course", request.CourseId);

        if (_currentUser.Role != UserRole.PlatformAdmin)
        {
            var programInstitutionId = await (
                from p in _db.Programs
                join d in _db.Departments on p.DepartmentId equals d.Id
                where p.Id == course.ProgramId
                select (Guid?)d.InstitutionId
            ).FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException("Program", course.ProgramId);


        }

        return new CourseDto(course.Id, course.ProgramId, course.Name, course.Code, course.IsActive);
    }
}

public sealed record GetClassesQuery(
    Guid CourseId,
    string? SearchTerm = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PaginatedList<ClassDto>>;

public sealed class GetClassesQueryHandler : IRequestHandler<GetClassesQuery, PaginatedList<ClassDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetClassesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ClassDto>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != UserRole.PlatformAdmin)
        {
            var courseInstitutionId = await (
                from c in _db.Courses
                join p in _db.Programs on c.ProgramId equals p.Id
                join d in _db.Departments on p.DepartmentId equals d.Id
                where c.Id == request.CourseId
                select (Guid?)d.InstitutionId
            ).FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException("Course", request.CourseId);


        }

        var query = _db.Classes.Where(c => c.CourseId == request.CourseId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c => c.Name.Contains(request.SearchTerm));

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "createdat" => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        var projected = query.Select(c => new ClassDto(c.Id, c.CourseId, c.AcademicTermId, c.Name, c.IsActive));

        return await PaginatedList<ClassDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public sealed record GetClassByIdQuery(Guid ClassId) : IRequest<ClassDto>;

public sealed class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, ClassDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetClassByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ClassDto> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
    {
        var cls = await _db.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken)
            ?? throw new EntityNotFoundException("Class", request.ClassId);

        if (_currentUser.Role != UserRole.PlatformAdmin)
        {
            var courseInstitutionId = await (
                from c in _db.Courses
                join p in _db.Programs on c.ProgramId equals p.Id
                join d in _db.Departments on p.DepartmentId equals d.Id
                where c.Id == cls.CourseId
                select (Guid?)d.InstitutionId
            ).FirstOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException("Course", cls.CourseId);


        }

        return new ClassDto(cls.Id, cls.CourseId, cls.AcademicTermId, cls.Name, cls.IsActive);
    }
}
