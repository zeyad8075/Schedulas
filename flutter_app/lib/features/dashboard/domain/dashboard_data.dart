import '../../activities/domain/models/activity_dto.dart';
import '../../notifications/domain/models/notification_dto.dart';

class DashboardData {
  final int upcomingActivitiesCount;
  final int unreadNotificationsCount;
  final double totalWorkloadHours;
  final List<ActivityDto> upcomingActivities;
  final List<NotificationDto> recentNotifications;

  const DashboardData({
    required this.upcomingActivitiesCount,
    required this.unreadNotificationsCount,
    required this.totalWorkloadHours,
    required this.upcomingActivities,
    required this.recentNotifications,
  });

  factory DashboardData.empty() => const DashboardData(
        upcomingActivitiesCount: 0,
        unreadNotificationsCount: 0,
        totalWorkloadHours: 0,
        upcomingActivities: [],
        recentNotifications: [],
      );
}
