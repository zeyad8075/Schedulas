using FluentValidation;
using MediatR;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Schedulas.Application.Features.OrgHierarchy.Commands;

public sealed record CourseDto(Guid Id, Guid ProgramId, string Name, string? Code, bool IsActive);

public sealed record CreateCourseCommand(Guid ProgramId, string Name, string? Code) : IRequest<CourseDto>;

public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Code).MaximumLength(50);
    }
}

/// <summary>SECURITY FIX (Write-Side Ownership Audit): previously had NO tenant check.</summary>
public sealed class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateCourseCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var institutionId = await OrgHierarchyAuthorization.EnsureCanManageProgramAsync(_db, _currentUser, request.ProgramId, cancellationToken);

        var course = new Course(institutionId, request.ProgramId, request.Name, request.Code);
        _db.Courses.Add(course);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(course);
    }

    internal static CourseDto ToDto(Course c) => new(c.Id, c.ProgramId, c.Name, c.Code, c.IsActive);
}

public sealed record UpdateCourseCommand(Guid CourseId, string Name, string? Code, bool IsActive) : IRequest<CourseDto>;

public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
}

/// <summary>SECURITY FIX: same gap as CreateCourseCommand, fixed the same way.</summary>
public sealed class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateCourseCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CourseDto> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _db.Courses.FindAsync([request.CourseId], cancellationToken)
            ?? throw new EntityNotFoundException("Course", request.CourseId);

        await OrgHierarchyAuthorization.EnsureCanManageCourseAsync(_db, _currentUser, course.Id, cancellationToken);

        course.UpdateDetails(request.Name, request.Code);
        if (request.IsActive) course.Activate(); else course.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return CreateCourseCommandHandler.ToDto(course);
    }
}

public sealed record DeleteCourseCommand(Guid CourseId) : IRequest<Unit>;

public sealed class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteCourseCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _db.Courses.FindAsync([request.CourseId], cancellationToken)
            ?? throw new EntityNotFoundException("Course", request.CourseId);

        await OrgHierarchyAuthorization.EnsureCanManageCourseAsync(_db, _currentUser, course.Id, cancellationToken);

        bool hasClasses = await _db.Classes.AnyAsync(c => c.CourseId == request.CourseId, cancellationToken);
        if (hasClasses)
            throw new InvalidStateTransitionException("COURSE_HAS_CLASSES", "Cannot delete a course that contains classes.");

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record RestoreCourseCommand(Guid CourseId) : IRequest<Unit>;

public sealed class RestoreCourseCommandHandler : IRequestHandler<RestoreCourseCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreCourseCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _db.Courses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken)
            ?? throw new EntityNotFoundException("Course", request.CourseId);

        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != course.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        await OrgHierarchyAuthorization.EnsureCanManageCourseAsync(_db, _currentUser, course.Id, cancellationToken);

        var program = await _db.Programs.FindAsync([course.ProgramId], cancellationToken);
        if (program == null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore course because its program is deleted.");

        course.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
