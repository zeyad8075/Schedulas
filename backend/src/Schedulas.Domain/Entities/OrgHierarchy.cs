using Schedulas.Domain.Common;
using Schedulas.Domain.Enums;

namespace Schedulas.Domain.Entities;

public class Institution : BaseEntity
{
    public string Name { get; private set; } = default!;
    public InstitutionType Type { get; private set; }
    public string Timezone { get; private set; } = "Asia/Riyadh";
    public string? LogoUrl { get; private set; }
    public bool IsSuspended { get; private set; }

    private readonly List<Department> _departments = new();
    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();

    private Institution() { } // EF Core

    public Institution(string name, InstitutionType type, string timezone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Institution name is required.", nameof(name));

        Name = name;
        Type = type;
        Timezone = string.IsNullOrWhiteSpace(timezone) ? "Asia/Riyadh" : timezone;
        IsSuspended = false;
    }

    public void UpdateDetails(string name, string? logoUrl, string timezone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Institution name is required.", nameof(name));

        Name = name;
        LogoUrl = logoUrl;
        Timezone = timezone;
    }

    public void Suspend() => IsSuspended = true;
    public void Reactivate() => IsSuspended = false;
}

public class Department : BaseEntity, IMustHaveTenant
{
    public Guid InstitutionId { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Department() { }

    public Department(Guid institutionId, string name)
    {
        InstitutionId = institutionId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Department name is required.", nameof(name))
            : name;
    }

    public void Rename(string name) => Name = name;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public class Program : BaseEntity, IMustHaveTenant
{
    public Guid InstitutionId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Program() { }

    public Program(Guid institutionId, Guid departmentId, string name)
    {
        InstitutionId = institutionId;
        DepartmentId = departmentId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Program name is required.", nameof(name))
            : name;
    }

    public void Rename(string name) => Name = name;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public class Course : BaseEntity, IMustHaveTenant
{
    public Guid InstitutionId { get; private set; }
    public Guid ProgramId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Course() { }

    public Course(Guid institutionId, Guid programId, string name, string? code)
    {
        InstitutionId = institutionId;
        ProgramId = programId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Course name is required.", nameof(name))
            : name;
        Code = code;
    }

    public void UpdateDetails(string name, string? code)
    {
        Name = name;
        Code = code;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>
/// A specific section/offering of a Course within an AcademicTerm —
/// e.g., "Course X, Section A, Fall 2026" (Database Design §3).
/// </summary>
public class Class : BaseEntity, IMustHaveTenant
{
    public Guid InstitutionId { get; private set; }
    public Guid CourseId { get; private set; }
    public Guid AcademicTermId { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Class() { }

    public Class(Guid institutionId, Guid courseId, Guid academicTermId, string name)
    {
        InstitutionId = institutionId;
        CourseId = courseId;
        AcademicTermId = academicTermId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Class name is required.", nameof(name))
            : name;
    }

    public void Rename(string name) => Name = name;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
