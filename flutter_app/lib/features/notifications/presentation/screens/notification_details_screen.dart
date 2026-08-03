import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import '../../domain/models/notification_dto.dart';
import '../../../../l10n/app_localizations.dart';

class NotificationDetailsScreen extends ConsumerWidget {
  final NotificationDto notification;

  const NotificationDetailsScreen({
    super.key,
    required this.notification,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final loc = AppLocalizations.of(context);
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.notificationDetails),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16.0),
        child: Card(
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  crossAxisAlignment: CrossAxisAlignment.center,
                  children: [
                    Icon(
                      _getIconForCategory(notification.category.name),
                      size: 40,
                      color: theme.colorScheme.primary,
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Text(
                        notification.title,
                        style: theme.textTheme.titleLarge,
                      ),
                    ),
                  ],
                ),
                const Divider(height: 32),
                Text(
                  DateFormat.yMMMMEEEEd()
                      .add_jm()
                      .format(notification.createdAt),
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                  ),
                ),
                const SizedBox(height: 16),
                Text(
                  notification.body,
                  style: theme.textTheme.bodyLarge,
                ),
                if (notification.relatedActivityId != null) ...[
                  const SizedBox(height: 32),
                  const Divider(),
                  const SizedBox(height: 16),
                  Text(
                    'Related Activity ID: ${notification.relatedActivityId}',
                    style: theme.textTheme.bodyMedium?.copyWith(
                      color: theme.colorScheme.secondary,
                    ),
                  ),
                ]
              ],
            ),
          ),
        ),
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
