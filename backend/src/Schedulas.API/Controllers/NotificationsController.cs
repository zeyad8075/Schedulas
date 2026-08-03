using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schedulas.API.Common;
using Schedulas.Application.Common.Models;
using Schedulas.Application.Features.Notifications;
using Schedulas.Domain.Enums;

namespace Schedulas.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly ISender _mediator;
    public NotificationsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool? isRead,
        [FromQuery] NotificationCategory? category,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(isRead, category, pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<NotificationDto>>.Ok(result));
    }

    [HttpGet("unread")]
    public async Task<IActionResult> ListUnread(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUnreadNotificationsQuery(pageNumber, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<NotificationDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new MarkNotificationReadCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand(), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم تعليم جميع الإشعارات كمقروءة"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNotification(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteNotificationCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم حذف الإشعار بنجاح"));
    }

    [HttpPost("device-tokens")]
    public async Task<IActionResult> RegisterDeviceToken(RegisterDeviceTokenCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "تم تسجيل الجهاز لاستقبال الإشعارات"));
    }
}
