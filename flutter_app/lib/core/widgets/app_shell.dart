import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../l10n/app_localizations.dart';
import '../../features/notifications/presentation/providers/notification_providers.dart';
import '../providers/connectivity_provider.dart';

/// Wraps go_router's StatefulShellRoute branches in a persistent bottom
/// navigation bar. Each branch (Home, Calendar, Notifications) keeps its
/// own state via IndexedStack under the hood — this widget only owns tab
/// selection and the unread badge, never business logic (that lives in
/// each screen's own providers).
class AppShell extends ConsumerWidget {
  final StatefulNavigationShell navigationShell;

  const AppShell({super.key, required this.navigationShell});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final unreadCount = ref.watch(unreadNotificationCountProvider);
    final isOffline = ref.watch(isOfflineProvider);
    final isTablet = MediaQuery.sizeOf(context).width >= 600;
    final l10n = AppLocalizations.of(context);
    final theme = Theme.of(context);

    final destinations = [
      NavigationDestination(
          icon: const Icon(Icons.dashboard_outlined),
          selectedIcon: const Icon(Icons.dashboard),
          label: l10n.navDashboard),
      NavigationDestination(
          icon: const Icon(Icons.calendar_month_outlined),
          selectedIcon: const Icon(Icons.calendar_month),
          label: l10n.navCalendar),
      NavigationDestination(
          icon: const Icon(Icons.event_outlined),
          selectedIcon: const Icon(Icons.event),
          label: l10n.navActivities),
      NavigationDestination(
        icon: Badge(
          label: Text('${unreadCount.valueOrNull ?? 0}'),
          isLabelVisible: (unreadCount.valueOrNull ?? 0) > 0,
          child: const Icon(Icons.notifications_outlined),
        ),
        selectedIcon: Badge(
          label: Text('${unreadCount.valueOrNull ?? 0}'),
          isLabelVisible: (unreadCount.valueOrNull ?? 0) > 0,
          child: const Icon(Icons.notifications),
        ),
        label: l10n.navNotifications,
      ),
      NavigationDestination(
          icon: const Icon(Icons.more_horiz),
          selectedIcon: const Icon(Icons.more_horiz),
          label: l10n.navMore),
    ];

    final railDestinations = destinations.map((d) {
      return NavigationRailDestination(
        icon: d.icon,
        selectedIcon: d.selectedIcon,
        label: Text(d.label),
      );
    }).toList();

    return Scaffold(
      body: Row(
        children: [
          if (isTablet)
            NavigationRail(
              selectedIndex: navigationShell.currentIndex,
              onDestinationSelected: (index) => navigationShell.goBranch(
                index,
                initialLocation: index == navigationShell.currentIndex,
              ),
              labelType: NavigationRailLabelType.all,
              destinations: railDestinations,
            ),
          if (isTablet) const VerticalDivider(thickness: 1, width: 1),
          Expanded(
            child: Column(
              children: [
                if (isOffline)
                  Container(
                    width: double.infinity,
                    color: theme.colorScheme.errorContainer,
                    padding:
                        const EdgeInsets.symmetric(vertical: 8, horizontal: 16),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.wifi_off,
                            size: 16,
                            color: theme.colorScheme.onErrorContainer),
                        const SizedBox(width: 8),
                        Text(
                          l10n.offlineBannerMessage,
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: theme.colorScheme.onErrorContainer,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                  ),
                Expanded(child: navigationShell),
              ],
            ),
          ),
        ],
      ),
      bottomNavigationBar: isTablet
          ? null
          : NavigationBar(
              selectedIndex: navigationShell.currentIndex,
              onDestinationSelected: (index) => navigationShell.goBranch(
                index,
                initialLocation: index == navigationShell.currentIndex,
              ),
              destinations: destinations,
            ),
    );
  }
}
