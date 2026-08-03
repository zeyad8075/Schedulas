using Schedulas.Application.Common.Behaviors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;
using Schedulas.Domain.Events;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schedulas.Application.Features.Notifications;

public sealed class ActivityRestoredEventHandler : INotificationHandler<DomainEventNotification<ActivityRestoredEvent>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPushNotificationService _push;

    public ActivityRestoredEventHandler(IApplicationDbContext db, IPushNotificationService push)
    {
        _db = db;
        _push = push;
    }

    public async Task Handle(DomainEventNotification<ActivityRestoredEvent> notification, CancellationToken cancellationToken)
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

        var (title, body) = BuildArabicMessage(activity.ActivityType, activity.Title);

        var notifications = recipients.Select(recipientId => new Domain.Entities.Notification(
            recipientId, activity.InstitutionId, NotificationCategory.ActivityEdited, title, body, activity.Id))
            .ToList();

        _db.Notifications.AddRange(notifications);
        
        foreach (var notificationRecord in notifications)
        {
            await _push.SendAsync(notificationRecord.RecipientId, notificationRecord.Title, notificationRecord.Body, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static (string Title, string Body) BuildArabicMessage(ActivityType type, string activityTitle)
    {
        var typeLabel = type switch
        {
            ActivityType.Assignment => "الواجب",
            ActivityType.Exam => "الاختبار",
            ActivityType.Project => "المشروع",
            ActivityType.Presentation => "العرض التقديمي",
            _ => "النشاط"
        };

        var title = $"استعادة {typeLabel}";
        var body = $"تمت استعادة \"{activityTitle}\" وعودته إلى الجدول.";

        return (title, body);
    }
}
