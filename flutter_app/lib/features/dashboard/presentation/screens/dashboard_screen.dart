import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../../core/widgets/statistics_card.dart';
import '../../../../core/widgets/section_header.dart';
import '../../../../core/widgets/refreshable_list.dart';
import '../../../../core/widgets/info_tile.dart';
import '../../../../core/widgets/loading_view.dart';
import '../../../../core/widgets/error_view.dart';
import '../../../../core/errors/failures.dart';
import '../../../auth/presentation/providers/auth_providers.dart';
import '../providers/dashboard_providers.dart';

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final dashboardState = ref.watch(dashboardNotifierProvider);
    final userProfile = ref.watch(currentProfileProvider);
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.navDashboard),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => ref.read(authNotifierProvider.notifier).logout(),
          ),
        ],
      ),
      body: dashboardState.when(
        loading: () => const LoadingView(),
        error: (error, stack) {
          final failure = error as Failure;
          return ErrorView(
            message: failure.message,
            onRetry: () => ref
                .read(dashboardNotifierProvider.notifier)
                .fetchDashboardData(),
          );
        },
        data: (data) => RefreshableList<int>(
          items: const [1], // Single wrapper item to construct the page layout
          onRefresh: () async {
            await ref
                .read(dashboardNotifierProvider.notifier)
                .fetchDashboardData();
          },
          itemBuilder: (context, _, __) {
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0),
                  child: Text(
                    l10n.homeWelcome(userProfile?.fullName ?? ''),
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                ),
                const SizedBox(height: 16),
                SectionHeader(title: l10n.dashboardOverview),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0),
                  child: Row(
                    children: [
                      Expanded(
                        child: StatisticsCard(
                          title: l10n.dashboardUpcomingActivities,
                          value: '${data.upcomingActivitiesCount}',
                          icon: Icons.event,
                          color: Theme.of(context).colorScheme.primary,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: StatisticsCard(
                          title: l10n.navNotifications,
                          value: '${data.unreadNotificationsCount}',
                          icon: Icons.notifications,
                          color: Theme.of(context).colorScheme.secondary,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 16),
                SectionHeader(
                  title: l10n.dashboardUpcomingActivities,
                  actionLabel: l10n.dashboardViewAll,
                  onAction: () {},
                ),
                if (data.upcomingActivities.isEmpty)
                  Padding(
                    padding: const EdgeInsets.all(16.0),
                    child:
                        Center(child: Text(l10n.dashboardNoUpcomingActivities)),
                  )
                else
                  Card(
                    margin: const EdgeInsets.symmetric(horizontal: 16.0),
                    child: Column(
                      children: data.upcomingActivities
                          .map((e) => InfoTile(
                                icon: Icons.event,
                                title: e.title,
                                subtitle: DateFormat.yMMMd()
                                    .format(DateTime.parse(e.scheduledDate)),
                              ))
                          .toList(),
                    ),
                  ),
                const SizedBox(height: 16),
                SectionHeader(
                  title: l10n.navNotifications,
                  actionLabel: l10n.dashboardViewAll,
                  onAction: () {},
                ),
                if (data.recentNotifications.isEmpty)
                  Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Center(child: Text(l10n.noData)),
                  )
                else
                  Card(
                    margin: const EdgeInsets.symmetric(horizontal: 16.0),
                    child: Column(
                      children: data.recentNotifications
                          .map((e) => InfoTile(
                                icon: e.isRead
                                    ? Icons.notifications
                                    : Icons.mark_email_unread,
                                title: e.title,
                                subtitle: e.body,
                              ))
                          .toList(),
                    ),
                  ),
              ],
            );
          },
        ),
      ),
    );
  }
}
