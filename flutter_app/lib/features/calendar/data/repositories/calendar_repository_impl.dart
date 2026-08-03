import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/errors/failures.dart';
import '../../domain/models/calendar_dtos.dart';
import '../../domain/repositories/calendar_repository.dart';

class CalendarRepositoryImpl implements CalendarRepository {
  final ApiClient _apiClient;

  CalendarRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, DailyCalendarDto>> getDaily({
    required String institutionId,
    required String date,
    String? teacherId,
    String? studentId,
    String? classId,
  }) async {
    try {
      final result = await _apiClient.get<DailyCalendarDto>(
        '/api/v1/calendar/daily',
        queryParameters: {
          'institutionId': institutionId,
          'date': date,
          if (teacherId != null) 'teacherId': teacherId,
          if (studentId != null) 'studentId': studentId,
          if (classId != null) 'classId': classId,
        },
        fromData: (json) =>
            DailyCalendarDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, WeeklyCalendarDto>> getWeekly({
    required String institutionId,
    required String date,
    String? teacherId,
    String? studentId,
    String? classId,
  }) async {
    try {
      final result = await _apiClient.get<WeeklyCalendarDto>(
        '/api/v1/calendar/weekly',
        queryParameters: {
          'institutionId': institutionId,
          'date': date,
          if (teacherId != null) 'teacherId': teacherId,
          if (studentId != null) 'studentId': studentId,
          if (classId != null) 'classId': classId,
        },
        fromData: (json) =>
            WeeklyCalendarDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, MonthlyCalendarDto>> getMonthly({
    required String institutionId,
    required int year,
    required int month,
    String? teacherId,
    String? studentId,
    String? classId,
  }) async {
    try {
      final result = await _apiClient.get<MonthlyCalendarDto>(
        '/api/v1/calendar/monthly',
        queryParameters: {
          'institutionId': institutionId,
          'year': year,
          'month': month,
          if (teacherId != null) 'teacherId': teacherId,
          if (studentId != null) 'studentId': studentId,
          if (classId != null) 'classId': classId,
        },
        fromData: (json) =>
            MonthlyCalendarDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
