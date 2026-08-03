using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Schedulas.Infrastructure.Identity;

/// <summary>
/// Runs once per request after JWT validation succeeds. ASP.NET Core's
/// [Authorize(Roles = "...")] checks for a claim of type ClaimTypes.Role
/// (configured as "role" in Program.cs's TokenValidationParameters), but
/// Supabase nests our custom role inside the user_metadata JSON claim
/// (see CurrentUserService's doc comment for why). This transformation
/// bridges the two so the framework's built-in role attribute keeps
/// working without every controller reaching into user_metadata itself.
/// </summary>
public sealed class SupabaseRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        if (identity.HasClaim(c => c.Type == "role"))
            return Task.FromResult(principal); // already promoted on a prior transformation pass

        var metadataRaw = principal.FindFirstValue("user_metadata");
        if (string.IsNullOrEmpty(metadataRaw))
            return Task.FromResult(principal);

        try
        {
            using var doc = JsonDocument.Parse(metadataRaw);
            if (doc.RootElement.TryGetProperty("role", out var roleValue) && roleValue.ValueKind == JsonValueKind.String)
            {
                identity.AddClaim(new Claim("role", roleValue.GetString()!));
            }
        }
        catch (JsonException)
        {
            // Malformed metadata is treated as "no role" rather than a hard
            // failure here; downstream authorization simply denies access.
        }

        return Task.FromResult(principal);
    }
}
