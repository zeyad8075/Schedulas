using Schedulas.Domain.Common;

namespace Schedulas.Domain.Entities;

public class AcademicTerm : BaseEntity, IMustHaveTenant
{
    public Guid InstitutionId { get; private set; }
    public string Name { get; private set; } = default!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AcademicTerm() { }

    public AcademicTerm(Guid institutionId, string name, DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        InstitutionId = institutionId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Term name is required.", nameof(name))
            : name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public bool Contains(DateOnly date) => date >= StartDate && date <= EndDate;

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public class Holiday : BaseEntity, IMustHaveTenant
{
    public Guid InstitutionId { get; private set; }
    public Guid? AcademicTermId { get; private set; }
    public string Name { get; private set; } = default!;
    public DateOnly HolidayDate { get; private set; }

    private Holiday() { }

    public Holiday(Guid institutionId, Guid? academicTermId, string name, DateOnly holidayDate)
    {
        InstitutionId = institutionId;
        AcademicTermId = academicTermId;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Holiday name is required.", nameof(name))
            : name;
        HolidayDate = holidayDate;
    }
}
