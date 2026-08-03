using Schedulas.Domain.Common;

namespace Schedulas.Domain.Entities;

/// <summary>
/// A registered FCM device token for push delivery. Added during the
/// Notifications-infrastructure implementation pass (not present in the
/// original Phase 3 schema) because sending a push requires a concrete
/// device token, not just a recipient Profile id — this is a genuine,
/// small, additive migration rather than a workaround.
/// </summary>
public class DeviceToken : BaseEntity
{
    public Guid ProfileId { get; private set; }
    public string Token { get; private set; } = default!;
    public string Platform { get; private set; } = default!; // android | ios
    public bool IsActive { get; private set; } = true;

    private DeviceToken() { }

    public DeviceToken(Guid profileId, string token, string platform)
    {
        ProfileId = profileId;
        Token = string.IsNullOrWhiteSpace(token)
            ? throw new ArgumentException("Device token is required.", nameof(token))
            : token;
        Platform = platform;
    }

    public void Deactivate() => IsActive = false;
}
