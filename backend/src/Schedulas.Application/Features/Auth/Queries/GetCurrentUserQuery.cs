using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.Auth.Queries;

public sealed record CurrentUserDto(
    Guid Id, string FullName, string Email, UserRole Role,
    Guid? InstitutionId, Guid? DepartmentId, Theme PreferredTheme);

public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken)
            ?? throw new EntityNotFoundException("Profile", userId);

        return _mapper.Map<CurrentUserDto>(profile);
    }
}
