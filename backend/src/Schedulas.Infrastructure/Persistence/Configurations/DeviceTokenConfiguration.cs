using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedulas.Domain.Entities;

namespace Schedulas.Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> b)
    {
        b.ToTable("device_tokens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ProfileId).HasColumnName("profile_id").IsRequired();
        b.Property(x => x.Token).HasColumnName("token").IsRequired();
        b.Property(x => x.Platform).HasColumnName("platform").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active");

        b.HasIndex(x => x.ProfileId).HasDatabaseName("ix_device_tokens_profile_id");
        b.HasIndex(x => x.Token).IsUnique().HasDatabaseName("uq_device_tokens_token");
        b.HasOne<Profile>().WithMany().HasForeignKey(x => x.ProfileId);

        AuditColumns.Apply(b);
    }
}
