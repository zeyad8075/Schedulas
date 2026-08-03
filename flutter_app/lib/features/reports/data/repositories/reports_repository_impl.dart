import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../../../../core/network/api_client.dart';
import '../../domain/models/reports_dtos.dart';
import '../../domain/repositories/reports_repository.dart';

class ReportsRepositoryImpl implements ReportsRepository {
  final ApiClient _apiClient;

  ReportsRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, WorkloadReportDto>> getInstitutionWorkload({
    required String institutionId,
    required String startDate,
    required String endDate,
  }) async {
    try {
      final result = await _apiClient.get<WorkloadReportDto>(
        '/institutions/$institutionId/reports/workload/institution',
        queryParameters: {
          'startDate': startDate,
          'endDate': endDate,
        },
        fromData: (json) =>
            WorkloadReportDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, WorkloadReportDto>> getTeacherWorkload({
    required String institutionId,
    required String teacherId,
    required String startDate,
    required String endDate,
  }) async {
    try {
      final result = await _apiClient.get<WorkloadReportDto>(
        '/institutions/$institutionId/reports/workload/teachers/$teacherId',
        queryParameters: {
          'startDate': startDate,
          'endDate': endDate,
        },
        fromData: (json) =>
            WorkloadReportDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, WorkloadReportDto>> getStudentWorkload({
    required String institutionId,
    required String studentId,
    required String startDate,
    required String endDate,
  }) async {
    try {
      final result = await _apiClient.get<WorkloadReportDto>(
        '/institutions/$institutionId/reports/workload/students/$studentId',
        queryParameters: {
          'startDate': startDate,
          'endDate': endDate,
        },
        fromData: (json) =>
            WorkloadReportDto.fromJson(json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, ActivityDistributionReportDto>>
      getActivityDistribution({
    required String institutionId,
    required String classId,
    required String startDate,
    required String endDate,
  }) async {
    try {
      final result = await _apiClient.get<ActivityDistributionReportDto>(
        '/institutions/$institutionId/reports/distribution/classes/$classId',
        queryParameters: {
          'startDate': startDate,
          'endDate': endDate,
        },
        fromData: (json) => ActivityDistributionReportDto.fromJson(
            json as Map<String, dynamic>),
      );
      return right(result);
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
