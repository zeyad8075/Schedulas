using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Domain.Enums;

namespace Schedulas.Application.Features.People.Commands;

public sealed record StudentDto(Guid Id, Guid ProfileId, Guid InstitutionId, string? StudentNumber, string FullName, string Email, bool IsActive);

public sealed record CreateStudentCommand(Guid ProfileId, Guid InstitutionId, string? StudentNumber) : IRequest<StudentDto>, ITenantScopedRequest
{
    public Guid? TargetInstitutionId => InstitutionId;
    public Guid? TargetDepartmentId => null;
}

public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.ProfileId).NotEmpty();
        RuleFor(x => x.InstitutionId).NotEmpty();
        RuleFor(x => x.StudentNumber).MaximumLength(100);
    }
}

public sealed class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IApplicationDbContext _db;

    public CreateStudentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Verify profile exists and is a student
        var profile = await _db.Profiles.FindAsync([request.ProfileId], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", request.ProfileId);

        if (profile.Role != UserRole.Student)
            throw new InvalidStateTransitionException("INVALID_ROLE", "Profile does not have the Student role.");

        if (profile.InstitutionId != request.InstitutionId)
            throw new InvalidStateTransitionException("TENANT_MISMATCH", "Profile belongs to a different institution.");

        // Verify student doesn't already exist for this profile
        var existing = await _db.Students.FirstOrDefaultAsync(s => s.ProfileId == request.ProfileId, cancellationToken);
        if (existing != null)
            throw new InvalidStateTransitionException("ALREADY_EXISTS", "A Student record already exists for this Profile.");

        // Verify unique student number if provided
        if (!string.IsNullOrWhiteSpace(request.StudentNumber))
        {
            var numberExists = await _db.Students.AnyAsync(s => s.InstitutionId == request.InstitutionId && s.StudentNumber == request.StudentNumber, cancellationToken);
            if (numberExists)
                throw new InvalidStateTransitionException("DUPLICATE_NUMBER", "Student number is already in use within this institution.");
        }

        var student = new Student(request.ProfileId, request.InstitutionId, request.StudentNumber);
        _db.Students.Add(student);
        await _db.SaveChangesAsync(cancellationToken);
        
        return new StudentDto(student.Id, student.ProfileId, student.InstitutionId, student.StudentNumber, profile.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record UpdateStudentCommand(Guid StudentId, string? StudentNumber) : IRequest<StudentDto>;

public sealed class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.StudentNumber).MaximumLength(100);
    }
}

public sealed class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, StudentDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateStudentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<StudentDto> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _db.Students.FindAsync([request.StudentId], cancellationToken)
            ?? throw new EntityNotFoundException("Student", request.StudentId);

        if (!string.IsNullOrWhiteSpace(request.StudentNumber) && request.StudentNumber != student.StudentNumber)
        {
            var numberExists = await _db.Students.AnyAsync(s => s.InstitutionId == student.InstitutionId && s.StudentNumber == request.StudentNumber && s.Id != student.Id, cancellationToken);
            if (numberExists)
                throw new InvalidStateTransitionException("DUPLICATE_NUMBER", "Student number is already in use within this institution.");
        }

        student.UpdateStudentNumber(request.StudentNumber);
        await _db.SaveChangesAsync(cancellationToken);
        
        var profile = await _db.Profiles.FindAsync([student.ProfileId], cancellationToken);
        return new StudentDto(student.Id, student.ProfileId, student.InstitutionId, student.StudentNumber, profile!.FullName, profile.Email, profile.IsActive);
    }
}

public sealed record DeleteStudentCommand(Guid StudentId) : IRequest<Unit>;

public sealed class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeleteStudentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _db.Students.FindAsync([request.StudentId], cancellationToken)
            ?? throw new EntityNotFoundException("Student", request.StudentId);

        _db.Students.Remove(student);
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}

public sealed record RestoreStudentCommand(Guid StudentId) : IRequest<Unit>;

public sealed class RestoreStudentCommandHandler : IRequestHandler<RestoreStudentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreStudentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _db.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken)
            ?? throw new EntityNotFoundException("Student", request.StudentId);

        if (_currentUser.Role != UserRole.PlatformAdmin && _currentUser.InstitutionId != student.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        var profile = await _db.Profiles.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == student.ProfileId, cancellationToken);
        if (profile == null || profile.DeletedAt != null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore student because the underlying profile is deleted.");

        student.DeletedAt = null;
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
