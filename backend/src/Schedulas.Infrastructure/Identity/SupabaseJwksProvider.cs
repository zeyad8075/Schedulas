using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Schedulas.Infrastructure.Identity;

/// <summary>
/// Fetches and caches Supabase's JSON Web Key Set from
/// {SUPABASE_URL}/auth/v1/.well-known/jwks.json, so the API validates
/// Supabase-issued JWTs using Supabase's actual public signing keys
/// (asymmetric — ES256/RS256, whichever Supabase project is configured
/// with) rather than a shared symmetric secret. This is the "validate
/// JWTs using Supabase's JWKS/public keys" requirement, not the earlier
/// simplified HS256-secret approach it replaces.
///
/// Exposes a synchronous <see cref="GetSigningKeys"/> because
/// TokenValidationParameters.IssuerSigningKeyResolver's delegate signature
/// is itself synchronous (a constraint of Microsoft.IdentityModel.Tokens,
/// not something this class chooses). The keyset is cached for
/// <see cref="CacheDuration"/> and refreshed lazily on the next validation
/// after it expires — Supabase rotates signing keys infrequently, so a
/// brief synchronous refresh on cache expiry is an acceptable, standard
/// trade-off (the same pattern most JWKS-consuming middleware uses
/// internally).
/// </summary>
public sealed class SupabaseJwksProvider
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    private readonly HttpClient _http;
    private readonly SupabaseOptions _options;
    private readonly ILogger<SupabaseJwksProvider> _logger;

    private readonly object _lock = new();
    private IReadOnlyList<SecurityKey> _cachedKeys = Array.Empty<SecurityKey>();
    private DateTimeOffset _cacheExpiresAt = DateTimeOffset.MinValue;

    public SupabaseJwksProvider(IHttpClientFactory httpClientFactory, IOptions<SupabaseOptions> options, ILogger<SupabaseJwksProvider> logger)
    {
        _http = httpClientFactory.CreateClient(nameof(SupabaseJwksProvider));
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Called synchronously by TokenValidationParameters.IssuerSigningKeyResolver
    /// for every token whose 'kid' isn't already in the cache, or once the
    /// cache has expired.
    /// </summary>
    public IEnumerable<SecurityKey> GetSigningKeys()
    {
        if (DateTimeOffset.UtcNow < _cacheExpiresAt)
            return _cachedKeys;

        lock (_lock)
        {
            if (DateTimeOffset.UtcNow < _cacheExpiresAt)
                return _cachedKeys;

            try
            {
                // Intentional synchronous-over-async: IssuerSigningKeyResolver
                // is a sync delegate by contract. ASP.NET Core has no
                // SynchronizationContext, so this does not risk the classic
                // deadlock; it simply blocks the validating thread briefly,
                // only on cache-miss/expiry (at most once per CacheDuration).
                var jwks = FetchJwksAsync().GetAwaiter().GetResult();
                _cachedKeys = jwks;
                _cacheExpiresAt = DateTimeOffset.UtcNow.Add(CacheDuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Supabase JWKS; continuing with previously cached keys if any.");
                // Deliberately do not clear _cachedKeys on failure -- a
                // transient network blip should not lock out every
                // already-issued token until the next successful refresh.
            }

            return _cachedKeys;
        }
    }

    private async Task<IReadOnlyList<SecurityKey>> FetchJwksAsync()
    {
        var url = $"{_options.Url.TrimEnd('/')}/auth/v1/.well-known/jwks.json";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var jwks = new JsonWebKeySet(json);

        return jwks.GetSigningKeys().ToList();
    }
}
