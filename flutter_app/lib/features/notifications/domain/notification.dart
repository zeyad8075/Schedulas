/// Mirrors Schedulas.Domain.Enums.NotificationCategory.
enum NotificationCategory {
  newActivity,
  activityEdited,
  activityCancelled,
  deadlineReminder,
  ruleViolation,
  ruleOverride;

  static NotificationCategory fromJson(String value) => switch (value) {
        'NewActivity' => NotificationCategory.newActivity,
        'ActivityEdited' => NotificationCategory.activityEdited,
        'ActivityCancelled' => NotificationCategory.activityCancelled,
        'DeadlineReminder' => NotificationCategory.deadlineReminder,
        'RuleViolation' => NotificationCategory.ruleViolation,
        'RuleOverride' => NotificationCategory.ruleOverride,
        _ => throw ArgumentError('Unknown NotificationCategory: $value'),
      };
}

/// Mirrors Schedulas.Application.Features.Notifications.NotificationDto.
class AppNotification {
  final String id;
  final NotificationCategory category;
  final String title;
  final String body;
  final String? relatedActivityId;
  final bool isRead;
  final DateTime createdAt;

  const AppNotification({
    required this.id,
    required this.category,
    required this.title,
    required this.body,
    this.relatedActivityId,
    required this.isRead,
    required this.createdAt,
  });

  factory AppNotification.fromJson(Map<String, dynamic> json) =>
      AppNotification(
        id: json['id'] as String,
        category: NotificationCategory.fromJson(json['category'] as String),
        title: json['title'] as String,
        body: json['body'] as String,
        relatedActivityId: json['relatedActivityId'] as String?,
        isRead: json['isRead'] as bool,
        createdAt: DateTime.parse(json['createdAt'] as String),
      );
}
