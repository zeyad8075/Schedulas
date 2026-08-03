import '../../../activities/domain/models/activity_enums.dart';

class CalendarEntryDto {
  final String activityId;
  final String title;
  final String? description;
  final String date;
  final String? startTime;
  final String? endTime;
  final String? duration;
  final ActivityStatus status;
  final int priority;
  final ActivityType activityType;

  final String classId;
  final String className;
  final String courseName;

  final String? teacherId;
  final String? teacherName;
  final String? roomId;
  final String? roomName;

  final String colorHex;

  CalendarEntryDto({
    required this.activityId,
    required this.title,
    this.description,
    required this.date,
    this.startTime,
    this.endTime,
    this.duration,
    required this.status,
    required this.priority,
    required this.activityType,
    required this.classId,
    required this.className,
    required this.courseName,
    this.teacherId,
    this.teacherName,
    this.roomId,
    this.roomName,
    required this.colorHex,
  });

  factory CalendarEntryDto.fromJson(Map<String, dynamic> json) {
    return CalendarEntryDto(
      activityId: json['activityId'] as String,
      title: json['title'] as String,
      description: json['description'] as String?,
      date: json['date'] as String,
      startTime: json['startTime'] as String?,
      endTime: json['endTime'] as String?,
      duration: json['duration'] as String?,
      status: ActivityStatusExtension.fromValue(json['status'] as int),
      priority: json['priority'] as int,
      activityType:
          ActivityTypeExtension.fromValue(json['activityType'] as int),
      classId: json['classId'] as String,
      className: json['className'] as String,
      courseName: json['courseName'] as String,
      teacherId: json['teacherId'] as String?,
      teacherName: json['teacherName'] as String?,
      roomId: json['roomId'] as String?,
      roomName: json['roomName'] as String?,
      colorHex: json['colorHex'] as String,
    );
  }
}

class DailyCalendarDto {
  final String date;
  final List<CalendarEntryDto> entries;

  DailyCalendarDto({
    required this.date,
    required this.entries,
  });

  factory DailyCalendarDto.fromJson(Map<String, dynamic> json) {
    return DailyCalendarDto(
      date: json['date'] as String,
      entries: (json['entries'] as List<dynamic>?)
              ?.map((e) => CalendarEntryDto.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class WeeklyCalendarDto {
  final String startDate;
  final String endDate;
  final List<DailyCalendarDto> days;

  WeeklyCalendarDto({
    required this.startDate,
    required this.endDate,
    required this.days,
  });

  factory WeeklyCalendarDto.fromJson(Map<String, dynamic> json) {
    return WeeklyCalendarDto(
      startDate: json['startDate'] as String,
      endDate: json['endDate'] as String,
      days: (json['days'] as List<dynamic>?)
              ?.map((e) => DailyCalendarDto.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class MonthlyCalendarDto {
  final int year;
  final int month;
  final List<WeeklyCalendarDto> weeks;

  MonthlyCalendarDto({
    required this.year,
    required this.month,
    required this.weeks,
  });

  factory MonthlyCalendarDto.fromJson(Map<String, dynamic> json) {
    return MonthlyCalendarDto(
      year: json['year'] as int,
      month: json['month'] as int,
      weeks: (json['weeks'] as List<dynamic>?)
              ?.map(
                  (e) => WeeklyCalendarDto.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}
