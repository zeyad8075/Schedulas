import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../domain/models/calendar_dtos.dart';
import '../../domain/repositories/calendar_repository.dart';
import '../../data/repositories/calendar_repository_impl.dart';

final calendarRepositoryProvider = Provider<CalendarRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return CalendarRepositoryImpl(apiClient);
});

class CalendarFilter {
  final String institutionId;
  final String date;
  final String? teacherId;
  final String? studentId;
  final String? classId;

  CalendarFilter({
    required this.institutionId,
    required this.date,
    this.teacherId,
    this.studentId,
    this.classId,
  });

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is CalendarFilter &&
        other.institutionId == institutionId &&
        other.date == date &&
        other.teacherId == teacherId &&
        other.studentId == studentId &&
        other.classId == classId;
  }

  @override
  int get hashCode {
    return Object.hash(
      institutionId,
      date,
      teacherId,
      studentId,
      classId,
    );
  }
}

class MonthlyCalendarFilter {
  final String institutionId;
  final int year;
  final int month;
  final String? teacherId;
  final String? studentId;
  final String? classId;

  MonthlyCalendarFilter({
    required this.institutionId,
    required this.year,
    required this.month,
    this.teacherId,
    this.studentId,
    this.classId,
  });

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is MonthlyCalendarFilter &&
        other.institutionId == institutionId &&
        other.year == year &&
        other.month == month &&
        other.teacherId == teacherId &&
        other.studentId == studentId &&
        other.classId == classId;
  }

  @override
  int get hashCode {
    return Object.hash(
      institutionId,
      year,
      month,
      teacherId,
      studentId,
      classId,
    );
  }
}

final dailyCalendarProvider =
    FutureProvider.family<DailyCalendarDto, CalendarFilter>(
        (ref, filter) async {
  final repository = ref.watch(calendarRepositoryProvider);
  final result = await repository.getDaily(
    institutionId: filter.institutionId,
    date: filter.date,
    teacherId: filter.teacherId,
    studentId: filter.studentId,
    classId: filter.classId,
  );

  return result.fold(
    (failure) => throw Exception(failure.message),
    (data) => data,
  );
});

final weeklyCalendarProvider =
    FutureProvider.family<WeeklyCalendarDto, CalendarFilter>(
        (ref, filter) async {
  final repository = ref.watch(calendarRepositoryProvider);
  final result = await repository.getWeekly(
    institutionId: filter.institutionId,
    date: filter.date,
    teacherId: filter.teacherId,
    studentId: filter.studentId,
    classId: filter.classId,
  );

  return result.fold(
    (failure) => throw Exception(failure.message),
    (data) => data,
  );
});

final monthlyCalendarProvider =
    FutureProvider.family<MonthlyCalendarDto, MonthlyCalendarFilter>(
        (ref, filter) async {
  final repository = ref.watch(calendarRepositoryProvider);
  final result = await repository.getMonthly(
    institutionId: filter.institutionId,
    year: filter.year,
    month: filter.month,
    teacherId: filter.teacherId,
    studentId: filter.studentId,
    classId: filter.classId,
  );

  return result.fold(
    (failure) => throw Exception(failure.message),
    (data) => data,
  );
});
