import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../../../core/network/paginated_list.dart';
import '../../domain/models/notification_dto.dart';
import '../../domain/repositories/notification_repository.dart';
import '../../data/repositories/notification_repository_impl.dart';

final notificationRepositoryProvider = Provider<NotificationRepository>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return NotificationRepositoryImpl(apiClient);
});

// Used to paginate notifications
final notificationsProvider =
    FutureProvider.family<PaginatedList<NotificationDto>, int>(
        (ref, pageNumber) async {
  final repo = ref.watch(notificationRepositoryProvider);
  final result =
      await repo.getNotifications(pageNumber: pageNumber, pageSize: 20);
  return result.fold(
    (l) => throw l,
    (r) => r,
  );
});

// Used to paginate unread notifications
final unreadNotificationsProvider =
    FutureProvider.family<PaginatedList<NotificationDto>, int>(
        (ref, pageNumber) async {
  final repo = ref.watch(notificationRepositoryProvider);
  final result =
      await repo.getUnreadNotifications(pageNumber: pageNumber, pageSize: 20);
  return result.fold(
    (l) => throw l,
    (r) => r,
  );
});

// Get total unread count (for dashboard / badges)
final unreadNotificationCountProvider = FutureProvider<int>((ref) async {
  final repo = ref.watch(notificationRepositoryProvider);
  // Just fetch first page to get TotalCount
  final result = await repo.getUnreadNotifications(pageNumber: 1, pageSize: 1);
  return result.fold(
    (l) => 0,
    (r) => r.totalCount,
  );
});
