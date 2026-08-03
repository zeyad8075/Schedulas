using Schedulas.Domain.Common;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.Entities;

public class Notification : BaseEntity, IMustHaveTenant
{
    public Guid RecipientId { get; private set; } // -> Profile.Id (== Supabase auth.users.id)
    public Guid InstitutionId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string Title { get; private set; } = default!; // Arabic, resolved before construction
    public string Body { get; private set; } = default!;  // Arabic, resolved before construction
    public Guid? RelatedActivityId { get; private set; }
    public bool IsRead { get; private set; }
    public bool SentViaPush { get; private set; }

    private Notification() { }

    public Notification(Guid recipientId, Guid institutionId, NotificationCategory category,
        string title, string body, Guid? relatedActivityId)
    {
        RecipientId = recipientId;
        InstitutionId = institutionId;
        Category = category;
        Title = title;
        Body = body;
        RelatedActivityId = relatedActivityId;
    }

    public void MarkRead() => IsRead = true;
    public void MarkSentViaPush() => SentViaPush = true;
}
