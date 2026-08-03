using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Schedulas.Application.Features.Activities.Commands;

public sealed record DeleteActivityCommand(Guid ActivityId) : IRequest<Unit>;

public sealed class DeleteActivityCommandHandler : IRequestHandler<DeleteActivityCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteActivityCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _db.Activities.FindAsync([request.ActivityId], cancellationToken)
            ?? throw new EntityNotFoundException("Activity", request.ActivityId);

        await ActivityAuthorization.EnsureCanCreateForClassAsync(_db, _currentUser, activity.ClassId, cancellationToken);

        _db.Activities.Remove(activity); // Interceptor turns this into a soft delete
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public sealed record RestoreActivityCommand(Guid ActivityId) : IRequest<Unit>;

public sealed class RestoreActivityCommandHandler : IRequestHandler<RestoreActivityCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RestoreActivityCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RestoreActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _db.Activities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == request.ActivityId, cancellationToken)
            ?? throw new EntityNotFoundException("Activity", request.ActivityId);

        if (_currentUser.Role != UserRole.PlatformAdmin && _currentUser.InstitutionId != activity.InstitutionId)
            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");

        await ActivityAuthorization.EnsureCanCreateForClassAsync(_db, _currentUser, activity.ClassId, cancellationToken);

        var classEntity = await _db.Classes.FindAsync([activity.ClassId], cancellationToken);
        if (classEntity == null)
            throw new InvalidStateTransitionException("PARENT_INACTIVE", "Cannot restore activity because its class is deleted.");

        activity.Restore();
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
