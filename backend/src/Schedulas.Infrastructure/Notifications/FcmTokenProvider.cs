using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Schedulas.Infrastructure.Notifications;

/// <summary>
/// Exchanges a Firebase service-account JSON key for a short-lived OAuth2
/// access token, per Google's server-to-server auth flow
/// (https://developers.google.com/identity/protocols/oauth2/service-account).
/// Builds and RS256-signs the JWT assertion itself rather than depending on
/// the full Google.Apis SDK, keeping the dependency footprint small. Caches
/// the token in memory until shortly before expiry.
/// </summary>
public sealed class FcmTokenProvider
{
    private const string TokenUri = "https://oauth2.googleapis.com/token";
    private const string FcmScope = "https://www.googleapis.com/auth/firebase.messaging";

    private readonly HttpClient _http;
    private readonly FirebaseOptions _options;

    private string? _cachedToken;
    private DateTimeOffset _cachedTokenExpiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FcmTokenProvider(IHttpClientFactory httpClientFactory, IOptions<FirebaseOptions> options)
    {
        _http = httpClientFactory.CreateClient(nameof(FcmTokenProvider));
        _options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedTokenExpiresAt.AddMinutes(-2))
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedTokenExpiresAt.AddMinutes(-2))
                return _cachedToken;

            var serviceAccount = ReadServiceAccount(_options.ServiceAccountJsonPath);
            var assertion = BuildSignedJwt(serviceAccount);

            var response = await _http.PostAsync(TokenUri, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = assertion
            }), ct);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Google OAuth2 token endpoint returned an empty response.");

            _cachedToken = body.AccessToken;
            _cachedTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(body.ExpiresIn);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static ServiceAccountKey ReadServiceAccount(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ServiceAccountKey>(json)
            ?? throw new InvalidOperationException($"Could not parse Firebase service account JSON at '{path}'.");
    }

    private static string BuildSignedJwt(ServiceAccountKey account)
    {
        var now = DateTimeOffset.UtcNow;
        var header = new { alg = "RS256", typ = "JWT" };
        var claims = new
        {
            iss = account.ClientEmail,
            scope = FcmScope,
            aud = TokenUri,
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddHours(1).ToUnixTimeSeconds()
        };

        var headerSegment = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var claimsSegment = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(claims));
        var unsignedToken = $"{headerSegment}.{claimsSegment}";

        using var rsa = RSA.Create();
        rsa.ImportFromPem(account.PrivateKey);
        var signature = rsa.SignData(Encoding.UTF8.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return $"{unsignedToken}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed record ServiceAccountKey(
        [property: JsonPropertyName("client_email")] string ClientEmail,
        [property: JsonPropertyName("private_key")] string PrivateKey);

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
