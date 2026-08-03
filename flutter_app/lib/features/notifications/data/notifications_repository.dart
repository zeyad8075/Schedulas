import '../../../core/network/api_client.dart';
import '../../../core/network/paginated_list.dart';
import '../domain/notification.dart';

class NotificationsRepository {
  final ApiClient _apiClient;

  NotificationsRepository(this._apiClient);

  /// Mirrors GET /api/v1/notifications. Scoped server-side to the caller's
  /// own RecipientId — never takes a userId parameter, by design (this
  /// was one of the endpoints the Security Audit found already correct:
  /// it resolves scope from the JWT, not from anything the client sends).
  Future<PaginatedList<AppNotification>> getNotifications({
    bool? isRead,
    NotificationCategory? category,
    int pageNumber = 1,
    int pageSize = 20,
  }) {
    return _apiClient.get(
      '/notifications',
      queryParameters: {
        if (isRead != null) 'isRead': isRead,
        if (category != null) 'category': _categoryToJson(category),
        'pageNumber': pageNumber,
        'pageSize': pageSize,
      },
      fromData: (json) => PaginatedList<AppNotification>.fromJson(
        json as Map<String, dynamic>,
        AppNotification.fromJson,
      ),
    );
  }

  Future<void> markRead(String notificationId) =>
      _apiClient.put('/notifications/$notificationId/read', fromData: (_) {});

  Future<void> markAllRead() =>
      _apiClient.put('/notifications/read-all', fromData: (_) {});

  /// Registers this device's FCM token so push notifications can actually
  /// be delivered — mirrors POST /notifications/device-tokens. Call once
  /// at app startup / whenever the platform hands back a fresh token
  /// (Firebase's onTokenRefresh). Firebase Messaging integration itself
  /// (obtaining the token) is a separate, not-yet-built piece — this
  /// method is the API-side half, ready for that to call into.
  Future<void> registerDeviceToken(
      {required String token, required String platform}) {
    return _apiClient.post(
      '/notifications/device-tokens',
      body: {'token': token, 'platform': platform},
      fromData: (_) {},
    );
  }

  String _categoryToJson(NotificationCategory category) => switch (category) {
        NotificationCategory.newActivity => 'NewActivity',
        NotificationCategory.activityEdited => 'ActivityEdited',
        NotificationCategory.activityCancelled => 'ActivityCancelled',
        NotificationCategory.deadlineReminder => 'DeadlineReminder',
        NotificationCategory.ruleViolation => 'RuleViolation',
        NotificationCategory.ruleOverride => 'RuleOverride',
      };
}
