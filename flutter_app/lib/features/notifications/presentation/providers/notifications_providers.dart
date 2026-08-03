import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../../domain/notification.dart';
import '../../data/notifications_repository.dart';

final notificationsRepositoryProvider = Provider<NotificationsRepository>(
  (ref) => NotificationsRepository(ref.watch(apiClientProvider)),
);

/// The notification list. Re-fetches whenever the signed-in Profile
/// changes (login/logout) — no manual refresh wiring needed.
final notificationsProvider =
    FutureProvider.autoDispose<List<AppNotification>>((ref) async {
  final profile = ref.watch(currentProfileProvider);
  if (profile == null) return [];

  final repository = ref.watch(notificationsRepositoryProvider);
  final result = await repository.getNotifications(pageSize: 50);
  return result.items;
});

/// Derived count for the bottom-nav badge — recomputed automatically
/// whenever notificationsProvider refreshes, never fetched separately.
final unreadNotificationCountProvider = Provider.autoDispose<int>((ref) {
  final notifications = ref.watch(notificationsProvider).valueOrNull ?? [];
  return notifications.where((n) => !n.isRead).length;
});

/// Marks one notification read, then invalidates the list so the badge
/// and the item's own read state update together — never just flips a
/// local flag without confirming the server accepted it.
Future<void> markNotificationRead(WidgetRef ref, String notificationId) async {
  await ref.read(notificationsRepositoryProvider).markRead(notificationId);
  ref.invalidate(notificationsProvider);
}

Future<void> markAllNotificationsRead(WidgetRef ref) async {
  await ref.read(notificationsRepositoryProvider).markAllRead();
  ref.invalidate(notificationsProvider);
}
