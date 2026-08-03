import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:schedulas/core/network/api_client.dart';
import 'package:schedulas/core/network/api_exception.dart';
import 'package:schedulas/features/dashboard/data/repositories/dashboard_repository_impl.dart';

class MockApiClient extends Mock implements ApiClient {}

void main() {
  late MockApiClient mockApiClient;
  late DashboardRepositoryImpl repository;

  setUp(() {
    mockApiClient = MockApiClient();
    repository = DashboardRepositoryImpl(mockApiClient);
  });

  group('DashboardRepositoryImpl', () {
    test('getDashboardData returns empty data on success', () async {
      when(() => mockApiClient.get<List<dynamic>>(
            any(),
            fromData: any(named: 'fromData'),
          )).thenAnswer((_) async => []);

      final result = await repository.getDashboardData();

      expect(result.isRight(), true);
      result.fold(
        (l) => fail('Should be right'),
        (r) {
          expect(r.upcomingActivitiesCount, 0);
          expect(r.unreadNotificationsCount, 0);
          expect(r.totalWorkloadHours, 0);
          expect(r.upcomingActivities, isEmpty);
          expect(r.recentNotifications, isEmpty);
        },
      );
    });

    test('getDashboardData handles Unauthorized errors correctly', () async {
      when(() => mockApiClient.get<List<dynamic>>(
            any(),
            fromData: any(named: 'fromData'),
          )).thenThrow(const ApiException('Unauthorized', statusCode: 401));

      final result = await repository.getDashboardData();

      expect(result.isLeft(), true);
      result.fold(
        (l) {
          expect(l.message, 'Session expired');
        },
        (r) => fail('Should be left'),
      );
    });
  });
}
