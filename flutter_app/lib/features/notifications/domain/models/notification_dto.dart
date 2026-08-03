import 'notification_enums.dart';

class NotificationDto {
  final String id;
  final NotificationCategory category;
  final String title;
  final String body;
  final String? relatedActivityId;
  final bool isRead;
  final DateTime createdAt;

  const NotificationDto({
    required this.id,
    required this.category,
    required this.title,
    required this.body,
    this.relatedActivityId,
    required this.isRead,
    required this.createdAt,
  });

  factory NotificationDto.fromJson(Map<String, dynamic> json) {
    return NotificationDto(
      id: json['id'] as String,
      category: json['category'] != null
          ? NotificationCategory.fromJson(json['category'].toString())
          : NotificationCategory.newActivity,
      title: json['title'] as String? ?? '',
      body: json['body'] as String? ?? '',
      relatedActivityId: json['relatedActivityId'] as String?,
      isRead: json['isRead'] as bool? ?? false,
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'] as String)
          : DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'category': category.toJson(),
      'title': title,
      'body': body,
      'relatedActivityId': relatedActivityId,
      'isRead': isRead,
      'createdAt': createdAt.toIso8601String(),
    };
  }

  NotificationDto copyWith({
    String? id,
    NotificationCategory? category,
    String? title,
    String? body,
    String? relatedActivityId,
    bool? isRead,
    DateTime? createdAt,
  }) {
    return NotificationDto(
      id: id ?? this.id,
      category: category ?? this.category,
      title: title ?? this.title,
      body: body ?? this.body,
      relatedActivityId: relatedActivityId ?? this.relatedActivityId,
      isRead: isRead ?? this.isRead,
      createdAt: createdAt ?? this.createdAt,
    );
  }
}
