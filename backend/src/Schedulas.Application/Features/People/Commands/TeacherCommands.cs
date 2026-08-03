using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.People.Commands;

public sealed record TeacherDto(Guid Id, Guid ProfileId, Guid InstitutionId, Guid? DepartmentId, string FullName, string Email, bool IsActive);

public sealed record CreateTeacherCommand(Guid ProfileId, Guid InstitutionId, Guid? DepartmentId) : IRequest<TeacherDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => DepartmentId;
}

public sealed class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        RuleFor(x => x.InstitutionId).NotEmpty();
    }
}

public sealed class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, TeacherDto>
{
    private readonly IApplicationDbContext _db;

    public CreateTeacherCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TeacherDto> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([request.ProfileId], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", request.ProfileId);

        if (profile.Role != UserRole.Teacher)
            throw new InvalidStateTransitionException("INVALID_ROLE", "Profile does not have the Teacher role.");

        if (profile.InstitutionId != request.InstitutionId)
            throw new InvalidStateTransitionException("TENANT_MISMATCH", "Profile belongs to a different institution.");

        var existing = await _db.Teachers.FirstOrDefaultAsync(t => t.ProfileId == request.ProfileId, cancellationToken);
        if (existing != null)
            throw new InvalidStateTransitionException("ALREADY_EXISTS", "A Teacher record already exists for this Profile.");

        if (request.DepartmentId.HasValue)
        {
            var dept = await _db.Departments.FindAsync([request.DepartmentId.Value], cancellationToken)
                ?? throw new EntityNotFoundException("Department", request.DepartmentId.Value);
            if (dept.InstitutionId != request.InstitutionId)
                throw new InvalidStateTransitionException("TENANT_MISMATCH", "Department belongs to a different institution.");
        }

        var teacher = new Teacher(request.ProfileId, request.InstitutionId, request.DepartmentId);
        _db.Teachers.Add(teacher);
        await _db.SaveChangesAsync(cancellationToken);
        
        return new TeacherDto(teacher.Id, teacher.ProfileId, teacher.InstitutionId, teacher.DepartmentId, profile.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record UpdateTeacherCommand(Guid TeacherId, Guid? DepartmentId) : IRequest<TeacherDto>;

public sealed class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();
    }
}

public sealed class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, TeacherDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateTeacherCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TeacherDto> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacher = await _db.Teachers.FindAsync([request.TeacherId], cancellationToken)
            ?? throw new EntityNotFoundException("Teacher", request.TeacherId);

        if (request.DepartmentId.HasValue)
        {
            var dept = await _db.Departments.FindAsync([request.DepartmentId.Value], cancellationToken)
                ?? throw new EntityNotFoundException("Department", request.DepartmentId.Value);
            if (dept.InstitutionId != teacher.InstitutionId)
                throw new InvalidStateTransitionException("TENANT_MISMATCH", "Department belongs to a different institution.");
        }

        teacher.UpdateDepartment(request.DepartmentId);
        await _db.SaveChangesAsync(cancellationToken);
        
        var profile = await _db.Profiles.FindAsync([teacher.ProfileId], cancellationToken);
        return new TeacherDto(teacher.Id, teacher.ProfileId, teacher.InstitutionId, teacher.DepartmentId, profile!.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record DeleteTeacherCommand(Guid TeacherId) : IRequest<Unit>;

public sealed class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeleteTeacherCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacher = await _db.Teachers.FindAsync([request.TeacherId], cancellationToken)
            ?? throw new EntityNotFoundException("Teacher", request.TeacherId);

        _db.Teachers.Remove(teacher);
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}

public sealed record RestoreTeacherCommand(Guid TeacherId) : IRequest<Unit>;

public sealed class RestoreTeacherCommandHandler : IRequestHandler<RestoreTeacherCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreTeacherCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacher = await _db.Teachers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TeacherId, cancellationToken)
            ?? throw new EntityNotFoundException("Teacher", request.TeacherId);

        if (_currentUser.Role != UserRole.PlatformAdmin && _currentUser.InstitutionId != teacher.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        var profile = await _db.Profiles.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == teacher.ProfileId, cancellationToken);
        if (profile == null || profile.DeletedAt != null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore teacher because the underlying profile is deleted.");

        if (teacher.DepartmentId.HasValue)
        {
            var dept = await _db.Departments.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == teacher.DepartmentId.Value, cancellationToken);
            if (dept == null || dept.DeletedAt != null)
                throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore teacher because their associated department is deleted.");
        }

        teacher.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
