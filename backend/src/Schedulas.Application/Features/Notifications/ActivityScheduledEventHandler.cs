using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Behaviors;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Events;

namespace Schedulas.Application.Features.Notifications;

/// <summary>
/// Reacts to a newly (or override-)approved Activity by notifying every
/// enrolled student, their linked parents, and the assigned teacher(s) —
/// SRS FR-NOTIF-1. Arabic message text is resolved here, at the
/// Application/Presentation boundary, from a small resource lookup keyed
/// by category (Architecture §7) — kept inline for this delivery; a full
/// resx/JSON resource file is a trivial follow-up swap, not a
/// restructuring, once the full Arabic copy deck is finalized.
/// </summary>
public sealed class ActivityScheduledEventHandler : INotificationHandler<DomainEventNotification<ActivityScheduledEvent>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPushNotificationService _push;

    public ActivityScheduledEventHandler(IApplicationDbContext db, IPushNotificationService push)
    {
        _db = db;
        _push = push;
    }

    public async Task Handle(DomainEventNotification<ActivityScheduledEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == evt.ActivityId, cancellationToken);
        if (activity is null) return;

        var studentProfileIds = await _db.ClassStudents
            .Where(cs => cs.ClassId == evt.ClassId)
            .Join(_db.Students, cs => cs.StudentId, s => s.Id, (cs, s) => s.ProfileId)
            .ToListAsync(cancellationToken);

        var parentProfileIds = await _db.ParentStudentLinks
            .Join(_db.ClassStudents.Where(cs => cs.ClassId == evt.ClassId),
                link => link.StudentId, cs => cs.StudentId, (link, cs) => link.ParentId)
            .Join(_db.Parents, parentId => parentId, p => p.Id, (parentId, p) => p.ProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var teacherProfileIds = await _db.ClassTeachers
            .Where(ct => ct.ClassId == evt.ClassId)
            .Join(_db.Teachers, ct => ct.TeacherId, t => t.Id, (ct, t) => t.ProfileId)
            .ToListAsync(cancellationToken);

        var recipients = studentProfileIds.Concat(parentProfileIds).Concat(teacherProfileIds).Distinct();

        var (title, body) = BuildArabicMessage(evt.ActivityType, activity.Title, evt.WasOverride);

        var notifications = recipients.Select(recipientId => new Domain.Entities.Notification(
            recipientId, activity.InstitutionId, NotificationCategory.NewActivity, title, body, activity.Id))
            .ToList();

        _db.Notifications.AddRange(notifications);
        
        foreach (var notificationRecord in notifications)
        {
            await _push.SendAsync(notificationRecord.RecipientId, notificationRecord.Title, notificationRecord.Body, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static (string Title, string Body) BuildArabicMessage(ActivityType type, string activityTitle, bool wasOverride)
    {
        var typeLabel = type switch
        {
            ActivityType.Assignment => "واجب",
            ActivityType.Exam => "اختبار",
            ActivityType.Project => "مشروع",
            ActivityType.Presentation => "عرض تقديمي",
            _ => "نشاط"
        };

        var title = $"تمت جدولة {typeLabel} جديد";
        var body = wasOverride
            ? $"تمت جدولة \"{activityTitle}\" بموافقة استثنائية من الإدارة."
            : $"تمت جدولة \"{activityTitle}\".";

        return (title, body);
    }
}
