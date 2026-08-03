using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedulas.Domain.Entities;

namespace Schedulas.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.RecipientId).HasColumnName("recipient_id").IsRequired();
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.Category).HasColumnName("category").HasConversion<string>().IsRequired();
        b.Property(x => x.Title).HasColumnName("title").IsRequired();
        b.Property(x => x.Body).HasColumnName("body").IsRequired();
        b.Property(x => x.RelatedActivityId).HasColumnName("related_activity_id");
        b.Property(x => x.IsRead).HasColumnName("is_read");
        b.Property(x => x.SentViaPush).HasColumnName("sent_via_push");

        b.HasIndex(x => x.RecipientId).HasDatabaseName("ix_notifications_recipient_id");
        b.HasIndex(x => x.IsRead).HasDatabaseName("ix_notifications_is_read");

        b.HasOne<Profile>().WithMany().HasForeignKey(x => x.RecipientId);
        b.HasOne<Activity>().WithMany().HasForeignKey(x => x.RelatedActivityId);

        b.Ignore("DomainEvents");

        AuditColumns.Apply(b);
    }
}
