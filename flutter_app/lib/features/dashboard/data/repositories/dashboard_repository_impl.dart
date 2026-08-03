import 'package:fpdart/fpdart.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../core/errors/failures.dart';
import '../../domain/dashboard_data.dart';
import '../../domain/dashboard_repository.dart';

class DashboardRepositoryImpl implements DashboardRepository {
  final ApiClient _apiClient;

  DashboardRepositoryImpl(this._apiClient);

  @override
  Future<Either<Failure, DashboardData>> getDashboardData() async {
    try {
      // 1. Fetch unread notifications
      final notificationsResult = await _apiClient
          .get<List<dynamic>>(
            '/notifications/unread',
            fromData: (json) => json as List<dynamic>,
          )
          .catchError(
              (_) => <dynamic>[]); // Fallback to empty if not implemented yet

      // 2. Fetch upcoming activities
      final activitiesResult = await _apiClient
          .get<List<dynamic>>(
            '/activities',
            fromData: (json) => json as List<dynamic>,
          )
          .catchError((_) => <dynamic>[]);

      return right(DashboardData(
        upcomingActivitiesCount: activitiesResult.length,
        unreadNotificationsCount: notificationsResult.length,
        totalWorkloadHours: 0, // Placeholder
        upcomingActivities: [], // Would parse real Activity objects here
        recentNotifications: [], // Would parse real Notification objects here
      ));
    } on ApiException catch (e) {
      if (e.statusCode == 401) {
        return left(const UnauthorizedFailure('Session expired'));
      }
      return left(ServerFailure(e.message));
    } catch (e) {
      return left(UnknownFailure(e.toString()));
    }
  }
}
