using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schedulas.Application.Common.Interfaces;

namespace Schedulas.Infrastructure.Notifications;

/// <summary>
/// Sends a push via Firebase Cloud Messaging's HTTP v1 API to every active
/// device token registered for the recipient, per Sequence Diagram 3
/// (fan-out per recipient, resilient to partial failure — one bad/expired
/// token doesn't block delivery to the person's other devices).
/// </summary>
public sealed class FcmPushNotificationService : IPushNotificationService
{
    private readonly HttpClient _http;
    private readonly FcmTokenProvider _tokenProvider;
    private readonly FirebaseOptions _options;
    private readonly IApplicationDbContext _db;
    private readonly ILogger<FcmPushNotificationService> _logger;

    public FcmPushNotificationService(
        HttpClient http,
        FcmTokenProvider tokenProvider,
        IOptions<FirebaseOptions> options,
        IApplicationDbContext db,
        ILogger<FcmPushNotificationService> logger)
    {
        _http = http;
        _tokenProvider = tokenProvider;
        _options = options.Value;
        _db = db;
        _logger = logger;
    }

    public async Task SendAsync(Guid recipientProfileId, string title, string body, CancellationToken ct = default)
    {
        var deviceTokens = await _db.DeviceTokens
            .Where(d => d.ProfileId == recipientProfileId)
            .Select(d => d.Token)
            .ToListAsync(ct);

        if (deviceTokens.Count == 0)
        {
            _logger.LogInformation("No registered device tokens for Profile {ProfileId}; push skipped (in-app record still applies).", recipientProfileId);
            return;
        }

        var accessToken = await _tokenProvider.GetAccessTokenAsync(ct);
        var endpoint = $"https://fcm.googleapis.com/v1/projects/{_options.ProjectId}/messages:send";

        foreach (var token in deviceTokens)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(new
                {
                    message = new
                    {
                        token,
                        notification = new { title, body }
                    }
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                // One failed token (e.g. expired/unregistered) should never
                // block delivery to the recipient's other devices.
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("FCM send failed for a device token of Profile {ProfileId}: {Status} {Body}",
                    recipientProfileId, response.StatusCode, errorBody);
            }
        }
    }
}
