using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedulas.Domain.Entities;

namespace Schedulas.Infrastructure.Persistence.Configurations;

public class AcademicTermConfiguration : IEntityTypeConfiguration<AcademicTerm>
{
    public void Configure(EntityTypeBuilder<AcademicTerm> b)
    {
        b.ToTable("academic_terms", t => t.HasCheckConstraint("ck_academic_terms_dates", "end_date > start_date"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        b.Property(x => x.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active");

        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_academic_terms_institution_id");
        b.HasOne<Institution>().WithMany().HasForeignKey(x => x.InstitutionId);

        AuditColumns.Apply(b);
    }
}

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> b)
    {
        b.ToTable("holidays");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.AcademicTermId).HasColumnName("academic_term_id");
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.HolidayDate).HasColumnName("holiday_date").HasColumnType("date").IsRequired();

        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_holidays_institution_id");
        b.HasIndex(x => x.HolidayDate).HasDatabaseName("ix_holidays_holiday_date");
        b.HasOne<Institution>().WithMany().HasForeignKey(x => x.InstitutionId);
        b.HasOne<AcademicTerm>().WithMany().HasForeignKey(x => x.AcademicTermId);

        AuditColumns.Apply(b);
    }
}
