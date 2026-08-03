import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../providers/activity_providers.dart';

class ActivityDetailsScreen extends ConsumerWidget {
  final String activityId;

  const ActivityDetailsScreen({
    super.key,
    required this.activityId,
  });

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final loc = AppLocalizations.of(context);
    final activityAsync = ref.watch(activityDetailsProvider(activityId));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.activityDetails),
      ),
      body: activityAsync.when(
        data: (activity) {
          return SingleChildScrollView(
            padding: const EdgeInsets.all(16.0),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      activity.title,
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                    const SizedBox(height: 8),
                    _buildDetailRow(
                        context, loc.type, activity.activityType.name),
                    _buildDetailRow(context, loc.status, activity.status.name),
                    _buildDetailRow(context, loc.date, activity.scheduledDate),
                    if (activity.scheduledTime != null)
                      _buildDetailRow(
                          context, loc.startTime, activity.scheduledTime!),
                    if (activity.endTime != null)
                      _buildDetailRow(context, loc.endTime, activity.endTime!),
                    if (activity.duration != null)
                      _buildDetailRow(
                          context, loc.duration, activity.duration!),
                    _buildDetailRow(
                        context, loc.priority, activity.priority.toString()),
                    const Divider(),
                    Text(
                      loc.description,
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 8),
                    Text(activity.description ?? loc.noData),
                  ],
                ),
              ),
            ),
          );
        },
        loading: () => const LoadingView(),
        error: (error, stack) => ErrorView(
          message: error.toString(),
          onRetry: () => ref.invalidate(activityDetailsProvider(activityId)),
        ),
      ),
    );
  }

  Widget _buildDetailRow(BuildContext context, String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 120,
            child: Text(
              label,
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
          Expanded(
            child: Text(value),
          ),
        ],
      ),
    );
  }
}
