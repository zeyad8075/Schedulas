import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../../activities/presentation/providers/activity_providers.dart';
import '../../../notifications/presentation/providers/notification_providers.dart';
import '../../../reports/presentation/providers/reports_providers.dart';
import '../../domain/dashboard_data.dart';
import '../../domain/dashboard_repository.dart';
import '../../data/repositories/dashboard_repository_impl.dart';

final dashboardRepositoryProvider = Provider<DashboardRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return DashboardRepositoryImpl(apiClient);
});

final dashboardNotifierProvider =
    StateNotifierProvider<DashboardNotifier, AsyncValue<DashboardData>>((ref) {
  return DashboardNotifier(ref);
});

class DashboardNotifier extends StateNotifier<AsyncValue<DashboardData>> {
  final Ref _ref;

  DashboardNotifier(this._ref) : super(const AsyncValue.loading()) {
    fetchDashboardData();
  }

  Future<void> fetchDashboardData() async {
    state = const AsyncValue.loading();
    try {
      final user = _ref.read(currentProfileProvider);
      if (user == null) {
        state = const AsyncValue.error('User not logged in', StackTrace.empty);
        return;
      }

      // Fetch unread notifications
      final notificationsRepo = _ref.read(notificationRepositoryProvider);
      final notificationsResult = await notificationsRepo
          .getUnreadNotifications(pageNumber: 1, pageSize: 5);

      // Fetch upcoming activities
      final activityRepo = _ref.read(activityRepositoryProvider);
      final activitiesResult = await activityRepo.getActivities(
          institutionId: user.institutionId ?? '', pageNumber: 1, pageSize: 5);

      // Fetch workload (mock dates for now, could be current month)
      final now = DateTime.now();
      final startDate =
          "${now.year}-${now.month.toString().padLeft(2, '0')}-01";
      final endDate =
          "${now.year}-${now.month.toString().padLeft(2, '0')}-${DateTime(now.year, now.month + 1, 0).day}";

      final reportsRepo = _ref.read(reportsRepositoryProvider);

      double workloadHours = 0;
      if (user.role.name == 'teacher') {
        final wResult = await reportsRepo.getTeacherWorkload(
            institutionId: user.institutionId ?? '',
            teacherId: user.id,
            startDate: startDate,
            endDate: endDate);
        wResult.map((w) => workloadHours = w.totalWorkloadMinutes / 60.0);
      } else if (user.role.name == 'student' || user.role.name == 'parent') {
        final wResult = await reportsRepo.getStudentWorkload(
            institutionId: user.institutionId ?? '',
            studentId: user.id,
            startDate: startDate,
            endDate: endDate);
        wResult.map((w) => workloadHours = w.totalWorkloadMinutes / 60.0);
      } else if (user.role.name == 'institutionAdmin') {
        final wResult = await reportsRepo.getInstitutionWorkload(
            institutionId: user.institutionId ?? '',
            startDate: startDate,
            endDate: endDate);
        wResult.map((w) => workloadHours = w.totalWorkloadMinutes / 60.0);
      }

      state = AsyncValue.data(DashboardData(
        upcomingActivitiesCount:
            activitiesResult.fold((l) => 0, (r) => r.totalCount),
        unreadNotificationsCount:
            notificationsResult.fold((l) => 0, (r) => r.totalCount),
        totalWorkloadHours: workloadHours,
        upcomingActivities: activitiesResult.fold((l) => [], (r) => r.items),
        recentNotifications:
            notificationsResult.fold((l) => [], (r) => r.items),
      ));
    } catch (e) {
      state = AsyncValue.error(e, StackTrace.current);
    }
  }
}
