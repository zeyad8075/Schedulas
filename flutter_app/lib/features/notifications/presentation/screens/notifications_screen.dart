import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../l10n/app_localizations.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/widgets/error_view.dart';
import '../../../../core/widgets/loading_view.dart';
import '../providers/notification_providers.dart';
import 'package:intl/intl.dart';

class NotificationsScreen extends ConsumerStatefulWidget {
  const NotificationsScreen({super.key});

  @override
  ConsumerState<NotificationsScreen> createState() =>
      _NotificationsScreenState();
}

class _NotificationsScreenState extends ConsumerState<NotificationsScreen> {
  bool _showOnlyUnread = false;
  int _pageNumber = 1;

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    final provider = _showOnlyUnread
        ? unreadNotificationsProvider(_pageNumber)
        : notificationsProvider(_pageNumber);

    final notificationsAsync = ref.watch(provider);

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.notifications), // Needs to be added to ARB
        actions: [
          IconButton(
            icon: Icon(
                _showOnlyUnread ? Icons.mark_email_unread : Icons.all_inbox),
            tooltip: _showOnlyUnread ? 'Show All' : 'Show Unread',
            onPressed: () {
              setState(() {
                _showOnlyUnread = !_showOnlyUnread;
                _pageNumber = 1;
              });
            },
          ),
          IconButton(
            icon: const Icon(Icons.checklist),
            tooltip: 'Mark all as read',
            onPressed: () async {
              final repo = ref.read(notificationRepositoryProvider);
              await repo.markAllAsRead();
              ref.invalidate(unreadNotificationCountProvider);
              ref.invalidate(notificationsProvider);
              ref.invalidate(unreadNotificationsProvider);
            },
          ),
        ],
      ),
      body: notificationsAsync.when(
        data: (paginated) {
          if (paginated.items.isEmpty) {
            return Center(
                child: Text(loc.noData)); // Assuming loc.noData exists
          }

          return RefreshIndicator(
            onRefresh: () async {
              ref.invalidate(unreadNotificationCountProvider);
              ref.invalidate(notificationsProvider);
              ref.invalidate(unreadNotificationsProvider);
            },
            child: ListView.builder(
              itemCount:
                  paginated.items.length + 1, // +1 for pagination controls
              itemBuilder: (context, index) {
                if (index == paginated.items.length) {
                  return _buildPaginationControls(
                      paginated.hasNextPage, paginated.hasPreviousPage);
                }

                final notification = paginated.items[index];
                return Dismissible(
                  key: Key(notification.id),
                  background: Container(
                    color: Colors.green,
                    alignment: Alignment.centerLeft,
                    padding: const EdgeInsets.only(left: 16),
                    child:
                        const Icon(Icons.mark_email_read, color: Colors.white),
                  ),
                  secondaryBackground: Container(
                    color: Colors.red,
                    alignment: Alignment.centerRight,
                    padding: const EdgeInsets.only(right: 16),
                    child: const Icon(Icons.delete, color: Colors.white),
                  ),
                  onDismissed: (direction) async {
                    final repo = ref.read(notificationRepositoryProvider);
                    if (direction == DismissDirection.endToStart) {
                      await repo.deleteNotification(notification.id);
                    } else {
                      await repo.markAsRead(notification.id);
                    }
                    ref.invalidate(unreadNotificationCountProvider);
                  },
                  child: ListTile(
                    leading: CircleAvatar(
                      backgroundColor: notification.isRead
                          ? Theme.of(context)
                              .colorScheme
                              .surfaceContainerHighest
                          : Theme.of(context).colorScheme.primaryContainer,
                      child: Icon(
                        _getIconForCategory(notification.category.name),
                        color: notification.isRead
                            ? Theme.of(context).colorScheme.onSurfaceVariant
                            : Theme.of(context).colorScheme.onPrimaryContainer,
                      ),
                    ),
                    title: Text(
                      notification.title,
                      style: TextStyle(
                        fontWeight: notification.isRead
                            ? FontWeight.normal
                            : FontWeight.bold,
                      ),
                    ),
                    subtitle: Text(
                      '${DateFormat.yMMMd().add_jm().format(notification.createdAt)}\n${notification.body}',
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    isThreeLine: true,
                    onTap: () async {
                      if (!notification.isRead) {
                        final repo = ref.read(notificationRepositoryProvider);
                        await repo.markAsRead(notification.id);
                        ref.invalidate(unreadNotificationCountProvider);
                        ref.invalidate(notificationsProvider);
                        ref.invalidate(unreadNotificationsProvider);
                      }
                      if (!context.mounted) return;
                      context.push('/notifications/${notification.id}',
                          extra: notification);
                    },
                  ),
                );
              },
            ),
          );
        },
        loading: () => const LoadingView(),
        error: (err, stack) => ErrorView(
          message: err.toString(),
          onRetry: () => ref.refresh(provider),
        ),
      ),
    );
  }

  Widget _buildPaginationControls(bool hasNext, bool hasPrev) {
    if (!hasNext && !hasPrev) return const SizedBox.shrink();

    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          IconButton(
            icon: const Icon(Icons.chevron_left),
            onPressed: hasPrev ? () => setState(() => _pageNumber--) : null,
          ),
          Text('Page $_pageNumber'),
          IconButton(
            icon: const Icon(Icons.chevron_right),
            onPressed: hasNext ? () => setState(() => _pageNumber++) : null,
          ),
        ],
      ),
    );
  }

  IconData _getIconForCategory(String category) {
    switch (category) {
      case 'NewActivity':
        return Icons.event;
      case 'ActivityEdited':
        return Icons.edit_calendar;
      case 'ActivityCancelled':
        return Icons.event_busy;
      case 'DeadlineReminder':
        return Icons.timer;
      case 'RuleViolation':
        return Icons.warning;
      case 'RuleOverride':
        return Icons.security;
      default:
        return Icons.notifications;
    }
  }
}
