import 'activity_enums.dart';

class ActivityDto {
  final String id;
  final String classId;
  final ActivityType activityType;
  final String title;
  final String? description;
  final String scheduledDate; // YYYY-MM-DD
  final String? scheduledTime; // HH:mm:ss
  final String? endTime; // HH:mm:ss
  final String? duration; // HH:mm:ss
  final int priority;
  final double? estimatedWeight;
  final ActivityStatus status;
  final String? metadataJson;

  ActivityDto({
    required this.id,
    required this.classId,
    required this.activityType,
    required this.title,
    this.description,
    required this.scheduledDate,
    this.scheduledTime,
    this.endTime,
    this.duration,
    required this.priority,
    this.estimatedWeight,
    required this.status,
    this.metadataJson,
  });

  factory ActivityDto.fromJson(Map<String, dynamic> json) {
    return ActivityDto(
      id: json['id'] as String,
      classId: json['classId'] as String,
      activityType:
          ActivityTypeExtension.fromValue(json['activityType'] as int),
      title: json['title'] as String,
      description: json['description'] as String?,
      scheduledDate: json['scheduledDate'] as String,
      scheduledTime: json['scheduledTime'] as String?,
      endTime: json['endTime'] as String?,
      duration: json['duration'] as String?,
      priority: json['priority'] as int,
      estimatedWeight: (json['estimatedWeight'] as num?)?.toDouble(),
      status: ActivityStatusExtension.fromValue(json['status'] as int),
      metadataJson: json['metadataJson'] as String?,
    );
  }
}

class ActivitySubmissionResult {
  final bool success;
  final ActivityDto? activity;
  final bool requiresOverride;
  final String? reasonCode;
  final String? triggeredRuleId;

  ActivitySubmissionResult({
    required this.success,
    this.activity,
    required this.requiresOverride,
    this.reasonCode,
    this.triggeredRuleId,
  });

  factory ActivitySubmissionResult.fromJson(Map<String, dynamic> json) {
    return ActivitySubmissionResult(
      success: json['success'] as bool? ?? false,
      activity: json['activity'] != null
          ? ActivityDto.fromJson(json['activity'] as Map<String, dynamic>)
          : null,
      requiresOverride: json['requiresOverride'] as bool? ?? false,
      reasonCode: json['reasonCode'] as String?,
      triggeredRuleId: json['triggeredRuleId'] as String?,
    );
  }
}
