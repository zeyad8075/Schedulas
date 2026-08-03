using Schedulas.Domain.Common;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.Entities;

/// <summary>
/// The identity hub: one row per authenticated person, one-to-one with a
/// Supabase Auth user. Per the explicit requirement that auth.users.id be
/// the primary identifier throughout the system, Profile.Id IS the
/// Supabase auth.users.id -- not a separately generated app-side UUID with
/// a foreign-key pointer to it. This is why the constructor takes the
/// Supabase user id and assigns it directly to the inherited Id property
/// (BaseEntity.Id has a protected setter specifically to allow this),
/// rather than letting BaseEntity's default Guid.NewGuid() apply.
///
/// auth.users itself (email, password hash, email-confirmation state,
/// etc.) is owned entirely by Supabase Auth and is never duplicated here --
/// Profile holds only business-specific data, per the explicit requirement
/// to keep authentication data and business data separated
/// (Architecture §3.1, and this session's Auth requirements).
/// Role-specific data beyond that lives in Student/Teacher/Parent.
/// </summary>
public class Profile : BaseEntity, IMayHaveTenant
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public Guid? InstitutionId { get; private set; } // null only for PlatformAdmin
    public Guid? DepartmentId { get; private set; }
    public bool IsActive { get; private set; }
    public Theme PreferredTheme { get; private set; } = Theme.Light;

    private Profile() { }

    /// <param name="id">
    /// The Supabase auth.users.id for this person -- supplied by the
    /// caller (Application layer, immediately after Supabase Auth issues
    /// it), never generated locally.
    /// </param>
    public Profile(Guid id, string fullName, string email, UserRole role,
        Guid? institutionId, Guid? departmentId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Profile.Id must be a real Supabase auth.users.id.", nameof(id));

        if (role != UserRole.PlatformAdmin && institutionId is null)
            throw new ArgumentException("institutionId is required for every role except PlatformAdmin.");

        Id = id;
        FullName = string.IsNullOrWhiteSpace(fullName)
            ? throw new ArgumentException("Full name is required.", nameof(fullName))
            : fullName;
        Email = string.IsNullOrWhiteSpace(email)
            ? throw new ArgumentException("Email is required.", nameof(email))
            : email;
        Role = role;
        InstitutionId = institutionId;
        DepartmentId = departmentId;
        IsActive = false; // becomes true only after Supabase confirms the email
    }

    public void ConfirmEmail() => IsActive = true;
    public void Suspend() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdateTheme(Theme theme) => PreferredTheme = theme;
    public void UpdateProfile(string fullName, string? phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }
}

public class Student : BaseEntity, IMustHaveTenant
{
    public Guid ProfileId { get; private set; }
    public Guid InstitutionId { get; private set; }
    public string? StudentNumber { get; private set; }

    private Student() { }

    public Student(Guid profileId, Guid institutionId, string? studentNumber)
    {
        ProfileId = profileId;
        InstitutionId = institutionId;
        StudentNumber = studentNumber;
    }
    
    public void UpdateStudentNumber(string? studentNumber) => StudentNumber = studentNumber;
}

public class Teacher : BaseEntity, IMustHaveTenant
{
    public Guid ProfileId { get; private set; }
    public Guid InstitutionId { get; private set; }
    public Guid? DepartmentId { get; private set; }

    private Teacher() { }

    public Teacher(Guid profileId, Guid institutionId, Guid? departmentId = null)
    {
        ProfileId = profileId;
        InstitutionId = institutionId;
        DepartmentId = departmentId;
    }
    
    public void UpdateDepartment(Guid? departmentId) => DepartmentId = departmentId;
}

public class Parent : BaseEntity
{
    public Guid ProfileId { get; private set; }

    private Parent() { }

    public Parent(Guid profileId)
    {
        ProfileId = profileId;
    }
}

/// <summary>Junction: links a Parent to one or more Students (SRS FR-AUTH-6).</summary>
public class ParentStudentLink : BaseEntity
{
    public Guid ParentId { get; private set; }
    public Guid StudentId { get; private set; }

    private ParentStudentLink() { }

    public ParentStudentLink(Guid parentId, Guid studentId)
    {
        ParentId = parentId;
        StudentId = studentId;
    }
}

/// <summary>Junction: a Student's enrollment into a Class.</summary>
public class ClassStudent : BaseEntity
{
    public Guid ClassId { get; private set; }
    public Guid StudentId { get; private set; }

    private ClassStudent() { }

    public ClassStudent(Guid classId, Guid studentId)
    {
        ClassId = classId;
        StudentId = studentId;
    }
}

/// <summary>Junction: a Teacher's assignment to a Class.</summary>
public class ClassTeacher : BaseEntity
{
    public Guid ClassId { get; private set; }
    public Guid TeacherId { get; private set; }

    private ClassTeacher() { }

    public ClassTeacher(Guid classId, Guid teacherId)
    {
        ClassId = classId;
        TeacherId = teacherId;
    }
}
