using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schedulas.Domain.Entities;

namespace Schedulas.Infrastructure.Persistence.Configurations;

/// <summary>
/// profiles.id is NOT database-generated -- it is always supplied
/// explicitly by the Application layer as the Supabase auth.users.id
/// (see Profile's constructor doc comment). ValueGeneratedNever() is the
/// EF Core signal for that: without it, EF would try to let Postgres
/// generate the PK and ignore the value we set, which would silently
/// break the "auth.users.id is the primary identifier throughout" rule.
/// </summary>
public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.ToTable("profiles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.FullName).HasColumnName("full_name").IsRequired();
        b.Property(x => x.Email).HasColumnName("email").IsRequired();
        b.Property(x => x.PhoneNumber).HasColumnName("phone_number");
        b.Property(x => x.Role).HasColumnName("role").HasConversion<string>().IsRequired();
        b.Property(x => x.InstitutionId).HasColumnName("institution_id");
        b.Property(x => x.DepartmentId).HasColumnName("department_id");
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.Property(x => x.PreferredTheme).HasColumnName("preferred_theme").HasConversion<string>();

        b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("uq_profiles_email");
        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_profiles_institution_id");
        b.HasIndex(x => x.Role).HasDatabaseName("ix_profiles_role");

        AuditColumns.Apply(b);
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.ToTable("students");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ProfileId).HasColumnName("profile_id").IsRequired();
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.StudentNumber).HasColumnName("student_number");

        b.HasIndex(x => x.ProfileId).IsUnique().HasDatabaseName("uq_students_profile_id");
        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_students_institution_id");
        b.HasOne<Profile>().WithMany().HasForeignKey(x => x.ProfileId);

        AuditColumns.Apply(b);
    }
}

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> b)
    {
        b.ToTable("teachers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ProfileId).HasColumnName("profile_id").IsRequired();
        b.Property(x => x.InstitutionId).HasColumnName("institution_id").IsRequired();
        b.Property(x => x.DepartmentId).HasColumnName("department_id");

        b.HasIndex(x => x.ProfileId).IsUnique().HasDatabaseName("uq_teachers_profile_id");
        b.HasIndex(x => x.InstitutionId).HasDatabaseName("ix_teachers_institution_id");
        b.HasIndex(x => x.DepartmentId).HasDatabaseName("ix_teachers_department_id");
        
        b.HasOne<Profile>().WithMany().HasForeignKey(x => x.ProfileId);
        b.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId);

        AuditColumns.Apply(b);
    }
}

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> b)
    {
        b.ToTable("parents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ProfileId).HasColumnName("profile_id").IsRequired();

        b.HasIndex(x => x.ProfileId).IsUnique().HasDatabaseName("uq_parents_profile_id");
        b.HasOne<Profile>().WithMany().HasForeignKey(x => x.ProfileId);

        AuditColumns.Apply(b);
    }
}

public class ParentStudentLinkConfiguration : IEntityTypeConfiguration<ParentStudentLink>
{
    public void Configure(EntityTypeBuilder<ParentStudentLink> b)
    {
        b.ToTable("parent_student_links");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ParentId).HasColumnName("parent_id").IsRequired();
        b.Property(x => x.StudentId).HasColumnName("student_id").IsRequired();

        b.HasIndex(x => new { x.ParentId, x.StudentId })
            .IsUnique()
            .HasDatabaseName("uq_parent_student_links_parent_student")
            .HasFilter("deleted_at IS NULL");

        b.HasOne<Parent>().WithMany().HasForeignKey(x => x.ParentId);
        b.HasOne<Student>().WithMany().HasForeignKey(x => x.StudentId);

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.CreatedBy).HasColumnName("created_by");
        b.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        b.Ignore(x => x.UpdatedAt);
        b.Ignore(x => x.UpdatedBy);
        b.Ignore("DomainEvents");
    }
}

public class ClassStudentConfiguration : IEntityTypeConfiguration<ClassStudent>
{
    public void Configure(EntityTypeBuilder<ClassStudent> b)
    {
        b.ToTable("class_students");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ClassId).HasColumnName("class_id").IsRequired();
        b.Property(x => x.StudentId).HasColumnName("student_id").IsRequired();

        b.HasIndex(x => new { x.ClassId, x.StudentId })
            .IsUnique()
            .HasDatabaseName("uq_class_students_class_student")
            .HasFilter("deleted_at IS NULL");
        b.HasIndex(x => x.StudentId).HasDatabaseName("ix_class_students_student_id");

        b.HasOne<Class>().WithMany().HasForeignKey(x => x.ClassId);
        b.HasOne<Student>().WithMany().HasForeignKey(x => x.StudentId);

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.CreatedBy).HasColumnName("created_by");
        b.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        b.Ignore(x => x.UpdatedAt);
        b.Ignore(x => x.UpdatedBy);
        b.Ignore("DomainEvents");
    }
}

public class ClassTeacherConfiguration : IEntityTypeConfiguration<ClassTeacher>
{
    public void Configure(EntityTypeBuilder<ClassTeacher> b)
    {
        b.ToTable("class_teachers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ClassId).HasColumnName("class_id").IsRequired();
        b.Property(x => x.TeacherId).HasColumnName("teacher_id").IsRequired();

        b.HasIndex(x => new { x.ClassId, x.TeacherId })
            .IsUnique()
            .HasDatabaseName("uq_class_teachers_class_teacher")
            .HasFilter("deleted_at IS NULL");
        b.HasIndex(x => x.TeacherId).HasDatabaseName("ix_class_teachers_teacher_id");

        b.HasOne<Class>().WithMany().HasForeignKey(x => x.ClassId);
        b.HasOne<Teacher>().WithMany().HasForeignKey(x => x.TeacherId);

        b.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        b.Property(x => x.CreatedBy).HasColumnName("created_by");
        b.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        b.Ignore(x => x.UpdatedAt);
        b.Ignore(x => x.UpdatedBy);
        b.Ignore("DomainEvents");
    }
}
