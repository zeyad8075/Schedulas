import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../../../../../core/widgets/empty_view.dart';
import '../../../../../core/widgets/info_tile.dart';
import '../../../../../core/widgets/confirmation_dialog.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../providers/profile_providers.dart';

class ProfilesListScreen extends ConsumerStatefulWidget {
  const ProfilesListScreen({super.key});

  @override
  ConsumerState<ProfilesListScreen> createState() => _ProfilesListScreenState();
}

class _ProfilesListScreenState extends ConsumerState<ProfilesListScreen> {
  String _searchTerm = '';
  int _pageNumber = 1;
  static const int _pageSize = 20;

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    final user = ref.watch(currentProfileProvider);
    final institutionId = user?.institutionId;

    if (institutionId == null) {
      return Scaffold(
        appBar: AppBar(title: Text(loc.users)),
        body: const ErrorView(message: 'Institution ID not found'),
      );
    }

    final filter = ProfileFilter(
      institutionId: institutionId,
      searchTerm: _searchTerm,
      pageNumber: _pageNumber,
      pageSize: _pageSize,
    );

    final profilesAsync = ref.watch(profilesListProvider(filter));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.users),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(60),
          child: Padding(
            padding: const EdgeInsets.all(8.0),
            child: SearchBar(
              hintText: loc.commonSearch,
              leading: const Icon(Icons.search),
              onChanged: (value) {
                setState(() {
                  _searchTerm = value;
                  _pageNumber = 1;
                });
              },
            ),
          ),
        ),
      ),
      body: profilesAsync.when(
        loading: () => const LoadingView(),
        error: (error, stack) => ErrorView(
          message: error.toString(),
          onRetry: () => ref.invalidate(profilesListProvider(filter)),
        ),
        data: (paginatedList) {
          if (paginatedList.items.isEmpty) {
            return const EmptyView(message: 'No users found');
          }
          return Column(
            children: [
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: paginatedList.items.length,
                  itemBuilder: (context, index) {
                    final profile = paginatedList.items[index];
                    return InfoTile(
                      title: profile.fullName,
                      subtitle:
                          "profile.email • profile.isActive ? loc.active : loc.inactive",
                      icon: Icons.person,
                      trailing: PopupMenuButton(
                        itemBuilder: (context) => [
                          if (profile.isActive)
                            PopupMenuItem(
                              child: Text(loc.suspend),
                              onTap: () => _suspendProfile(
                                  context, ref, profile.id, filter),
                            )
                          else
                            PopupMenuItem(
                              child: Text(loc.activate),
                              onTap: () => _activateProfile(
                                  context, ref, profile.id, filter),
                            )
                        ],
                      ),
                    );
                  },
                ),
              ),
              if (paginatedList.totalPages > 1)
                Padding(
                  padding: const EdgeInsets.all(8.0),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      IconButton(
                        icon: const Icon(Icons.chevron_left),
                        onPressed: paginatedList.hasPreviousPage
                            ? () => setState(() => _pageNumber--)
                            : null,
                      ),
                      Text(
                          '${loc.page} $_pageNumber ${loc.of_} ${paginatedList.totalPages}'),
                      IconButton(
                        icon: const Icon(Icons.chevron_right),
                        onPressed: paginatedList.hasNextPage
                            ? () => setState(() => _pageNumber++)
                            : null,
                      ),
                    ],
                  ),
                )
            ],
          );
        },
      ),
    );
  }

  void _suspendProfile(
      BuildContext context, WidgetRef ref, String id, ProfileFilter filter) {
    final loc = AppLocalizations.of(context);
    showDialog(
      context: context,
      builder: (ctx) => ConfirmationDialog(
        title: loc.suspend,
        content: 'Are you sure you want to suspend this user?',
        confirmText: loc.suspend,
        cancelText: loc.cancel,
        onConfirm: () async {
          final repository = ref.read(profileRepositoryProvider);
          final result = await repository.suspendProfile(id);
          if (result.isRight() && mounted) {
            ref.invalidate(profilesListProvider(filter));
          }
        },
      ),
    );
  }

  void _activateProfile(
      BuildContext context, WidgetRef ref, String id, ProfileFilter filter) {
    final loc = AppLocalizations.of(context);
    showDialog(
      context: context,
      builder: (ctx) => ConfirmationDialog(
        title: loc.activate,
        content: 'Are you sure you want to activate this user?',
        confirmText: loc.activate,
        cancelText: loc.cancel,
        onConfirm: () async {
          final repository = ref.read(profileRepositoryProvider);
          final result = await repository.activateProfile(id);
          if (result.isRight() && mounted) {
            ref.invalidate(profilesListProvider(filter));
          }
        },
      ),
    );
  }
}
