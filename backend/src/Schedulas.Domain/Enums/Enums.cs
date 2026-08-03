namespace Schedulas.Domain.Enums;

public enum UserRole
{
    PlatformAdmin,
    InstitutionAdmin,
    DepartmentAdmin,
    Teacher,
    Student,
    Parent
}

public enum InstitutionType
{
    School,
    University,
    Institute,
    Academy,
    TrainingCenter
}

public enum ActivityType
{
    Assignment,
    Exam,
    Project,
    Presentation,
    Event
}

public enum ActivityStatus
{
    Approved,
    RequiresOverrideApproved,
    Cancelled
}

/// <summary>
/// The scope a RuleDefinition applies at. Evaluation always prefers the
/// most specific matching scope (Class beats Course beats Program beats
/// Department beats Institution) per Constitution §18 and Architecture §5.
/// </summary>
public enum RuleScopeLevel
{
    Institution,
    Department,
    Program,
    Course,
    Class
}

public enum RuleType
{
    MaxActivitiesPerDay,
    MaxExamsPerWeek,
    MinDaysBeforeExam,
    NoActivityOnHoliday,
    ConflictDetection,
    PriorityResolution,
    TeacherConflict,
    ClassConflict,
    RoomConflict,
    DuplicateActivity,
    OverlappingActivity
}

public enum RuleOutcome
{
    Approved,
    Rejected,
    RequiresOverride
}

public enum NotificationCategory
{
    NewActivity,
    ActivityEdited,
    ActivityCancelled,
    DeadlineReminder,
    RuleViolation,
    RuleOverride
}

public enum Theme
{
    Light,
    Dark
}
