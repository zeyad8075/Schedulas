using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Schedulas.Application.Common.Interfaces;

namespace Schedulas.Infrastructure.Identity;

/// <summary>
/// Talks to Supabase's GoTrue Auth REST API directly over HttpClient
/// (endpoints documented at https://supabase.com/docs/reference/auth).
/// This is a real integration, not a stub — it requires a live Supabase
/// project configured via SupabaseOptions to function; there is no
/// in-memory fallback, per the Constitution's "no fake implementations"
/// rule.
/// </summary>
public sealed class SupabaseIdentityService : IIdentityService
{
    private readonly HttpClient _http;
    private readonly SupabaseOptions _options;

    public SupabaseIdentityService(HttpClient http, IOptions<SupabaseOptions> options)
    {
        _options = options.Value;

        http.BaseAddress = new Uri($"{_options.Url.TrimEnd('/')}/auth/v1/");
        http.DefaultRequestHeaders.Add("apikey", _options.AnonKey);
        _http = http;
    }

    public async Task<Guid> RegisterAsync(string email, string password, string fullName,
        string role, Guid? institutionId, Guid? departmentId, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("signup", new
        {
            email,
            password,
            data = new
            {
                full_name = fullName,
                role,
                institution_id = institutionId?.ToString(),
                department_id = departmentId?.ToString()
            }
        }, ct);

        await EnsureSuccess(response);

        var payload = await response.Content.ReadFromJsonAsync<SupabaseUserResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Supabase signup returned an empty response.");

        return Guid.Parse(payload.Id);
    }

    public async Task ConfirmEmailAsync(string token, CancellationToken ct = default)
    {
        // Supabase handles the confirmation link itself (redirects to a
        // configured URL with the session). This endpoint exists for the
        // API-driven confirm flow, verifying the OTP token type=signup.
        var response = await _http.PostAsJsonAsync("verify", new
        {
            type = "signup",
            token
        }, ct);

        await EnsureSuccess(response);
    }

    public async Task<(string AccessToken, string RefreshToken)> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("token?grant_type=password", new { email, password }, ct);
        await EnsureSuccess(response);

        var session = await response.Content.ReadFromJsonAsync<SupabaseSessionResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Supabase login returned an empty session.");

        return (session.AccessToken, session.RefreshToken);
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("token?grant_type=refresh_token",
            new { refresh_token = refreshToken }, ct);
        await EnsureSuccess(response);

        var session = await response.Content.ReadFromJsonAsync<SupabaseSessionResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Supabase refresh returned an empty session.");

        return (session.AccessToken, session.RefreshToken);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("recover", new { email }, ct);
        await EnsureSuccess(response);
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "user")
        {
            Content = JsonContent.Create(new { password = newPassword })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        await EnsureSuccess(response);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("logout", new { refresh_token = refreshToken }, ct);
        await EnsureSuccess(response);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        throw new SupabaseAuthException(response.StatusCode, body);
    }

    private sealed record SupabaseUserResponse([property: JsonPropertyName("id")] string Id);

    private sealed record SupabaseSessionResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}

public sealed class SupabaseAuthException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }

    public SupabaseAuthException(System.Net.HttpStatusCode statusCode, string responseBody)
        : base($"Supabase Auth request failed with {statusCode}: {responseBody}")
    {
        StatusCode = statusCode;
    }
}
