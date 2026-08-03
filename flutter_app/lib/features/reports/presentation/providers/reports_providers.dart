import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../domain/models/reports_dtos.dart';
import '../../domain/repositories/reports_repository.dart';
import '../../data/repositories/reports_repository_impl.dart';

final reportsRepositoryProvider = Provider<ReportsRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return ReportsRepositoryImpl(apiClient);
});

class WorkloadQueryArgs {
  final String institutionId;
  final String entityId;
  final String startDate;
  final String endDate;

  const WorkloadQueryArgs({
    required this.institutionId,
    required this.entityId,
    required this.startDate,
    required this.endDate,
  });

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is WorkloadQueryArgs &&
        other.institutionId == institutionId &&
        other.entityId == entityId &&
        other.startDate == startDate &&
        other.endDate == endDate;
  }

  @override
  int get hashCode => Object.hash(institutionId, entityId, startDate, endDate);
}

final institutionWorkloadProvider =
    FutureProvider.family<WorkloadReportDto, WorkloadQueryArgs>(
        (ref, args) async {
  final repo = ref.watch(reportsRepositoryProvider);
  final result = await repo.getInstitutionWorkload(
    institutionId: args.institutionId,
    startDate: args.startDate,
    endDate: args.endDate,
  );
  return result.fold((l) => throw l, (r) => r);
});

final teacherWorkloadProvider =
    FutureProvider.family<WorkloadReportDto, WorkloadQueryArgs>(
        (ref, args) async {
  final repo = ref.watch(reportsRepositoryProvider);
  final result = await repo.getTeacherWorkload(
    institutionId: args.institutionId,
    teacherId: args.entityId,
    startDate: args.startDate,
    endDate: args.endDate,
  );
  return result.fold((l) => throw l, (r) => r);
});

final studentWorkloadProvider =
    FutureProvider.family<WorkloadReportDto, WorkloadQueryArgs>(
        (ref, args) async {
  final repo = ref.watch(reportsRepositoryProvider);
  final result = await repo.getStudentWorkload(
    institutionId: args.institutionId,
    studentId: args.entityId,
    startDate: args.startDate,
    endDate: args.endDate,
  );
  return result.fold((l) => throw l, (r) => r);
});

final activityDistributionProvider =
    FutureProvider.family<ActivityDistributionReportDto, WorkloadQueryArgs>(
        (ref, args) async {
  final repo = ref.watch(reportsRepositoryProvider);
  final result = await repo.getActivityDistribution(
    institutionId: args.institutionId,
    classId: args.entityId,
    startDate: args.startDate,
    endDate: args.endDate,
  );
  return result.fold((l) => throw l, (r) => r);
});
