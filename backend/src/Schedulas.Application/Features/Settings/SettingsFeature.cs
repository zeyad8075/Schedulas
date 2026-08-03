using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.Settings;

public sealed record InstitutionSettingsDto(Guid Id, string Name, string? LogoUrl, string Timezone);

public sealed record GetInstitutionSettingsQuery : IRequest<InstitutionSettingsDto>;

public sealed class GetInstitutionSettingsQueryHandler : IRequestHandler<GetInstitutionSettingsQuery, InstitutionSettingsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetInstitutionSettingsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<InstitutionSettingsDto> Handle(GetInstitutionSettingsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.InstitutionId is not Guid institutionId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var institution = await _db.Institutions.FirstOrDefaultAsync(i => i.Id == institutionId, cancellationToken)
            ?? throw new EntityNotFoundException("Institution", institutionId);

        return _mapper.Map<InstitutionSettingsDto>(institution);
    }
}

public sealed record UpdateInstitutionSettingsCommand(string Name, string? LogoUrl, string Timezone) : IRequest<InstitutionSettingsDto>;

public sealed class UpdateInstitutionSettingsCommandValidator : AbstractValidator<UpdateInstitutionSettingsCommand>
{
    public UpdateInstitutionSettingsCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Timezone).NotEmpty();
    }
}

public sealed class UpdateInstitutionSettingsCommandHandler : IRequestHandler<UpdateInstitutionSettingsCommand, InstitutionSettingsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateInstitutionSettingsCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<InstitutionSettingsDto> Handle(UpdateInstitutionSettingsCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.InstitutionId is not Guid institutionId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var institution = await _db.Institutions.FirstOrDefaultAsync(i => i.Id == institutionId, cancellationToken)
            ?? throw new EntityNotFoundException("Institution", institutionId);

        institution.UpdateDetails(request.Name, request.LogoUrl, request.Timezone);
        await _db.SaveChangesAsync(cancellationToken);

        return new InstitutionSettingsDto(institution.Id, institution.Name, institution.LogoUrl, institution.Timezone);
    }
}

public sealed record UserSettingsDto(Guid Id, string FullName, string? PhoneNumber, Theme PreferredTheme);

public sealed record GetUserSettingsQuery : IRequest<UserSettingsDto>;

public sealed class GetUserSettingsQueryHandler : IRequestHandler<GetUserSettingsQuery, UserSettingsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetUserSettingsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<UserSettingsDto> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken)
            ?? throw new EntityNotFoundException("Profile", userId);

        return _mapper.Map<UserSettingsDto>(profile);
    }
}

/// <summary>
/// Covers profile + theme (SRS FR-SET-2). Notification-category
/// preferences (SRS FR-NOTIF-3, a "Should", not "Must") are not yet
/// persisted — there's no preferences table in the current schema. Adding
/// one is a small additive migration, deliberately deferred rather than
/// faked with a no-op endpoint.
/// </summary>
public sealed record UpdateUserSettingsCommand(string FullName, string? PhoneNumber, Theme PreferredTheme) : IRequest<UserSettingsDto>;

public sealed class UpdateUserSettingsCommandValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    public UpdateUserSettingsCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateUserSettingsCommandHandler : IRequestHandler<UpdateUserSettingsCommand, UserSettingsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserSettingsCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(UpdateUserSettingsCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var user = await _db.Profiles.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new EntityNotFoundException("Profile", userId);

        user.UpdateProfile(request.FullName, request.PhoneNumber);
        user.UpdateTheme(request.PreferredTheme);
        await _db.SaveChangesAsync(cancellationToken);

        return new UserSettingsDto(user.Id, user.FullName, user.PhoneNumber, user.PreferredTheme);
    }
}
