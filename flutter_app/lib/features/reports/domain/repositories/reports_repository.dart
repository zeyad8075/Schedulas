import 'package:fpdart/fpdart.dart';
import '../../../../core/errors/failures.dart';
import '../models/reports_dtos.dart';

abstract class ReportsRepository {
  Future<Either<Failure, WorkloadReportDto>> getInstitutionWorkload({
    required String institutionId,
    required String startDate,
    required String endDate,
  });

  Future<Either<Failure, WorkloadReportDto>> getTeacherWorkload({
    required String institutionId,
    required String teacherId,
    required String startDate,
    required String endDate,
  });

  Future<Either<Failure, WorkloadReportDto>> getStudentWorkload({
    required String institutionId,
    required String studentId,
    required String startDate,
    required String endDate,
  });

  Future<Either<Failure, ActivityDistributionReportDto>>
      getActivityDistribution({
    required String institutionId,
    required String classId,
    required String startDate,
    required String endDate,
  });
}
