namespace Schedulas.Infrastructure.Notifications;

/// <summary>Bound from configuration section "Firebase" (Constitution §16).</summary>
public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    /// <summary>Firebase project ID, used to build the FCM v1 send endpoint.</summary>
    public string ProjectId { get; set; } = default!;

    /// <summary>
    /// OAuth2 access token for the service account with the
    /// "Firebase Cloud Messaging API" scope. In production this should be
    /// obtained via a service-account credential exchange (e.g. Google's
    /// auth library) and refreshed automatically; this option holds
    /// whatever token-provisioning mechanism the host wires up.
    /// </summary>
    public string ServiceAccountJsonPath { get; set; } = default!;
}
