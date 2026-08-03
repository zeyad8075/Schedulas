using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Application.Common.Models;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Exceptions;

namespace Schedulas.Application.Features.Notifications;

public sealed record NotificationDto(
    Guid Id, NotificationCategory Category, string Title, string Body,
    Guid? RelatedActivityId, bool IsRead, DateTimeOffset CreatedAt);

public sealed record GetNotificationsQuery(
    bool? IsRead, NotificationCategory? Category, int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<NotificationDto>>;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<PaginatedList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var query = _db.Notifications.Where(n => n.RecipientId == userId);

        if (request.IsRead is bool isRead)
            query = query.Where(n => n.IsRead == isRead);

        if (request.Category is NotificationCategory category)
            query = query.Where(n => n.Category == category);

        var projected = query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Category, n.Title, n.Body, n.RelatedActivityId, n.IsRead, n.CreatedAt));

        return PaginatedList<NotificationDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest<Unit>;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(
            n => n.Id == request.NotificationId && n.RecipientId == _currentUser.UserId, cancellationToken)
            ?? throw new EntityNotFoundException("Notification", request.NotificationId);

        notification.MarkRead();
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record MarkAllNotificationsReadCommand : IRequest<Unit>;

public sealed class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkAllNotificationsReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _db.Notifications
            .Where(n => n.RecipientId == _currentUser.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var n in unread) n.MarkRead();

        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>
/// Registers (or refreshes) a device's FCM token for the current user —
/// called by the Flutter app on startup / token-refresh, feeding the
/// device_tokens table that FcmPushNotificationService reads from.
/// </summary>
public sealed record RegisterDeviceTokenCommand(string Token, string Platform) : IRequest<Unit>;

public sealed class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    public RegisterDeviceTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Platform).Must(p => p is "android" or "ios")
            .WithMessage("Platform must be 'android' or 'ios'.");
    }
}

public sealed class RegisterDeviceTokenCommandHandler : IRequestHandler<RegisterDeviceTokenCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RegisterDeviceTokenCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
            throw new UnauthorizedAccessException("NOT_AUTHENTICATED");

        var existing = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == request.Token, cancellationToken);

        if (existing is null)
        {
            _db.DeviceTokens.Add(new Domain.Entities.DeviceToken(userId, request.Token, request.Platform));
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

public sealed record GetUnreadNotificationsQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<NotificationDto>>;

public sealed class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IMediator _mediator;

    public GetUnreadNotificationsQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<PaginatedList<NotificationDto>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
    {
        // Simply delegate to GetNotificationsQuery with IsRead = false
        return _mediator.Send(new GetNotificationsQuery(false, null, request.PageNumber, request.PageSize), cancellationToken);
    }
}

public sealed record DeleteNotificationCommand(Guid NotificationId) : IRequest<Unit>;

public sealed class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteNotificationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(
            n => n.Id == request.NotificationId && n.RecipientId == _currentUser.UserId, cancellationToken)
            ?? throw new EntityNotFoundException("Notification", request.NotificationId);

        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
