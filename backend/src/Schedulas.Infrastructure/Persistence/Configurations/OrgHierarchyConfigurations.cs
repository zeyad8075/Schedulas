using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedulas.Domain.Entities;
using Program = Schedulas.Domain.Entities.Program;

namespace Schedulas.Infrastructure.Persistence.Configurations;

public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> b)
    {
        b.ToTable("institutions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().IsRequired();
        b.Property(x => x.Timezone).HasColumnName("timezone").IsRequired();
        b.Property(x => x.LogoUrl).HasColumnName("logo_url");
        b.Property(x => x.IsSuspended).HasColumnName("is_suspended");
        AuditColumns.Apply(b);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("departments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_departments_institution_id");
        b.HasOne<Institution>().WithMany(i => i.Departments).HasForeignKey(x => x.InstitutionId);
        AuditColumns.Apply(b);
    }
}

public class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> b)
    {
        b.ToTable("programs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.DepartmentId).HasColumnName("department_id").IsRequired();
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.HasIndex(x => x.DepartmentId).HasDatabaseName("ix_programs_department_id");
        b.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId);
        AuditColumns.Apply(b);
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.ToTable("courses");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ProgramId).HasColumnName("program_id").IsRequired();
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.Code).HasColumnName("code");
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.HasIndex(x => x.ProgramId).HasDatabaseName("ix_courses_program_id");
        b.HasOne<Program>().WithMany().HasForeignKey(x => x.ProgramId);
        AuditColumns.Apply(b);
    }
}

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> b)
    {
        b.ToTable("classes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.CourseId).HasColumnName("course_id").IsRequired();
        b.Property(x => x.AcademicTermId).HasColumnName("academic_term_id").IsRequired();
        b.Property(x => x.Name).HasColumnName("name").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.HasIndex(x => x.CourseId).HasDatabaseName("ix_classes_course_id");
        b.HasIndex(x => x.AcademicTermId).HasDatabaseName("ix_classes_academic_term_id");
        b.HasOne<Course>().WithMany().HasForeignKey(x => x.CourseId);
        b.HasOne<AcademicTerm>().WithMany().HasForeignKey(x => x.AcademicTermId);
        AuditColumns.Apply(b);
    }
}

/// <summary>Shared helper so the standard audit block (Constitution §10) is mapped identically everywhere.</summary>
internal static class AuditColumns
{
    public static void Apply<T>(EntityTypeBuilder<T> b) where T : class, Domain.Common.IAuditableEntity, Domain.Common.ISoftDeletableEntity
    {
        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        b.Property(x => x.CreatedBy).HasColumnName("created_by");
        b.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        b.Ignore(nameof(Domain.Common.ISoftDeletableEntity.IsDeleted));
        b.Ignore("DomainEvents"); // in-memory buffer only, never persisted (Domain/Common/BaseEntity.cs)
    }
}
