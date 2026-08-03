using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Entities;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using Schedulas.Application.Common.Behaviors;

namespace Schedulas.Application.Features.People.Commands;

public sealed record ProfileDto(Guid Id, string FullName, string Email, string? PhoneNumber, UserRole Role, Guid? InstitutionId, Guid? DepartmentId, bool IsActive, Theme PreferredTheme);

public sealed record UpdateProfileCommand(string FullName, string? PhoneNumber) : IRequest<ProfileDto>;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateProfileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([_currentUser.UserId!.Value], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", _currentUser.UserId!.Value);

        profile.UpdateProfile(request.FullName, request.PhoneNumber);
        await _db.SaveChangesAsync(cancellationToken);
        
        return ToDto(profile);
    }

    internal static ProfileDto ToDto(Profile p) => 
        new(p.Id, p.FullName, p.Email, p.PhoneNumber, p.Role, p.InstitutionId, p.DepartmentId, p.IsActive, p.PreferredTheme);
}

public sealed record UpdateThemeCommand(Theme PreferredTheme) : IRequest<Unit>;

public sealed class UpdateThemeCommandHandler : IRequestHandler<UpdateThemeCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateThemeCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([_currentUser.UserId!.Value], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", _currentUser.UserId!.Value);

        profile.UpdateTheme(request.PreferredTheme);
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}

// Suspend/Activate are for Admins to manage other users
public sealed record SuspendProfileCommand(Guid ProfileId) : IRequest<Unit>;
public sealed record ActivateProfileCommand(Guid ProfileId) : IRequest<Unit>;

public sealed class SuspendProfileCommandHandler : IRequestHandler<SuspendProfileCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SuspendProfileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(SuspendProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([request.ProfileId], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", request.ProfileId);

        // Security check for cross-tenant management
        if (_currentUser.Role != UserRole.PlatformAdmin && _currentUser.InstitutionId != profile.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        profile.Suspend();
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}

public sealed class ActivateProfileCommandHandler : IRequestHandler<ActivateProfileCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ActivateProfileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(ActivateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FindAsync([request.ProfileId], cancellationToken)
            ?? throw new EntityNotFoundException("Profile", request.ProfileId);

        if (_currentUser.Role != UserRole.PlatformAdmin && _currentUser.InstitutionId != profile.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        profile.Activate();
        await _db.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
