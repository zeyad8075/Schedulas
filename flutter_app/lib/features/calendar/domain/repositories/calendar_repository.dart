import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../models/calendar_dtos.dart';

abstract class CalendarRepository {
  Future<Either<Failure, DailyCalendarDto>> getDaily({
    required String institutionId,
    required String date,
    String? teacherId,
    String? studentId,
    String? classId,
  });

  Future<Either<Failure, WeeklyCalendarDto>> getWeekly({
    required String institutionId,
    required String date,
    String? teacherId,
    String? studentId,
    String? classId,
  });

  Future<Either<Failure, MonthlyCalendarDto>> getMonthly({
    required String institutionId,
    required int year,
    required int month,
    String? teacherId,
    String? studentId,
    String? classId,
  });
}
