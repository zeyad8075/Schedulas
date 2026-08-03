using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Domain.Enums;

namespace Schedulas.Infrastructure.Identity;

/// <summary>
/// Reads the current caller's identity from the validated Supabase JWT.
/// Supabase's GoTrue issues user_metadata as a single claim containing a
/// JSON object (the same object supplied at signup -- see
/// SupabaseIdentityService.RegisterAsync) rather than flat top-level
/// claims, so role/institution_id/department_id are parsed out of it here.
///
/// This is an intentional MVP simplification documented in the backend
/// README: a production hardening pass would instead configure a Supabase
/// Auth Hook (Postgres function) to promote these into top-level custom
/// claims at token-mint time, so a stale cached token can't retain a role
/// that was since revoked mid-session. For now, role changes take effect
/// on next login/refresh.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub"), out var id)
            ? id
            : null;

    public UserRole? Role =>
        Enum.TryParse<UserRole>(GetMetadataValue("role"), out var role) ? role : null;

    public Guid? InstitutionId =>
        Guid.TryParse(GetMetadataValue("institution_id"), out var id) ? id : null;

    public Guid? DepartmentId =>
        Guid.TryParse(GetMetadataValue("department_id"), out var id) ? id : null;

    private string? GetMetadataValue(string key)
    {
        var raw = User?.FindFirstValue("user_metadata");
        if (string.IsNullOrEmpty(raw)) return null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.TryGetProperty(key, out var value) ? value.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
