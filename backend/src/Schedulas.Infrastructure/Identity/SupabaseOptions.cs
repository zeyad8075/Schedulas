namespace Schedulas.Infrastructure.Identity;

/// <summary>
/// Bound from configuration section "Supabase" (appsettings / environment
/// variables — Constitution §16, secrets never committed to source).
/// </summary>
public sealed class SupabaseOptions
{
    public const string SectionName = "Supabase";

    /// <summary>e.g. https://xxxx.supabase.co</summary>
    public string Url { get; set; } = default!;

    /// <summary>The `anon` public API key — safe for the API's own service calls (not the browser).</summary>
    public string AnonKey { get; set; } = default!;
}
