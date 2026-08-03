using Microsoft.EntityFrameworkCore;
using Schedulas.Domain.Entities;
using Program = Schedulas.Domain.Entities.Program;

namespace Schedulas.Application.Common.Interfaces;

/// <summary>
/// Abstraction over SchedulasDbContext so Application handlers depend on
/// this interface, never on EF Core directly (Architecture §3.2 / §7).
/// Implemented by Schedulas.Infrastructure.Persistence.SchedulasDbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Institution> Institutions { get; }
    DbSet<Department> Departments { get; }
    DbSet<Program> Programs { get; }
    DbSet<Course> Courses { get; }
    DbSet<Class> Classes { get; }

    DbSet<Profile> Profiles { get; }
    DbSet<Student> Students { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Parent> Parents { get; }
    DbSet<ParentStudentLink> ParentStudentLinks { get; }
    DbSet<ClassStudent> ClassStudents { get; }
    DbSet<ClassTeacher> ClassTeachers { get; }

    DbSet<AcademicTerm> AcademicTerms { get; }
    DbSet<Holiday> Holidays { get; }

    DbSet<Activity> Activities { get; }
    DbSet<RuleDefinition> RuleDefinitions { get; }
    DbSet<RuleEvaluationLog> RuleEvaluationLogs { get; }

    DbSet<Notification> Notifications { get; }
    DbSet<DeviceToken> DeviceTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves the current authenticated caller's identity/claims. Implemented
/// in Infrastructure by reading the validated JWT (Architecture §3.3).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Domain.Enums.UserRole? Role { get; }
    Guid? InstitutionId { get; }
    Guid? DepartmentId { get; }
    bool IsAuthenticated { get; }
}

/// <summary>Wraps Supabase Auth operations (Architecture §3.3).</summary>
public interface IIdentityService
{
    Task<Guid> RegisterAsync(string email, string password, string fullName,
        string role, Guid? institutionId, Guid? departmentId, CancellationToken ct = default);
    Task ConfirmEmailAsync(string token, CancellationToken ct = default);
    Task<(string AccessToken, string RefreshToken)> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task RequestPasswordResetAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}

/// <summary>Wraps Firebase Cloud Messaging + persists in-app notification rows (Architecture §3.3).</summary>
public interface IPushNotificationService
{
    Task SendAsync(Guid recipientProfileId, string title, string body, CancellationToken ct = default);
}

/// <summary>
/// Abstraction over Supabase Storage (Architecture §3.3, Constitution's
/// storage requirement). Kept storage-provider-agnostic at the interface
/// level -- Application code never references Supabase's REST shapes
/// directly -- even though Supabase Storage is the only implementation
/// for this project.
/// </summary>
public interface IFileStorageService
{
    /// <returns>The storage path the file was saved under (bucket-relative), to persist alongside the owning entity.</returns>
    Task<string> UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default);

    Task<Stream> DownloadAsync(string bucket, string path, CancellationToken ct = default);

    Task DeleteAsync(string bucket, string path, CancellationToken ct = default);

    /// <summary>For public buckets (e.g. institution logos) -- no expiry, no signature required.</summary>
    string GetPublicUrl(string bucket, string path);

    /// <summary>For private buckets -- a time-limited signed URL, e.g. for exported reports.</summary>
    Task<string> GetSignedUrlAsync(string bucket, string path, TimeSpan expiresIn, CancellationToken ct = default);
}

/// <summary>Testable wall-clock abstraction — handlers never call DateTime.UtcNow directly.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateOnly Today { get; }
}

/// <summary>
/// Service for managing multi-tenant context and isolation.
/// Provides a controlled mechanism to bypass isolation for system tasks.
/// </summary>
public interface ITenantService
{
    Guid? TenantId { get; }
    bool IsTenantEnforced { get; }
    
    void SetTenantId(Guid tenantId);
    
    /// <summary>
    /// Begins a scope where tenant isolation is bypassed (e.g. for background jobs or platform admins).
    /// </summary>
    IDisposable BeginBypassScope();
}
