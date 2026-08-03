enum NotificationCategory {
  newActivity('NewActivity'),
  activityEdited('ActivityEdited'),
  activityCancelled('ActivityCancelled'),
  deadlineReminder('DeadlineReminder'),
  ruleViolation('RuleViolation'),
  ruleOverride('RuleOverride');

  final String value;
  const NotificationCategory(this.value);

  factory NotificationCategory.fromJson(String json) {
    return values.firstWhere(
      (e) => e.value == json,
      orElse: () => NotificationCategory.newActivity,
    );
  }

  String toJson() => value;
}
