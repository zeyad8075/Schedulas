import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:go_router/go_router.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../../../../../core/widgets/empty_view.dart';
import '../../../../../core/widgets/info_tile.dart';
import '../../../../../core/widgets/confirmation_dialog.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../profile/presentation/providers/profile_providers.dart';
import '../../../domain/models/user_role.dart';
import '../providers/parent_providers.dart';
import '../widgets/parent_form.dart';

class ParentsListScreen extends ConsumerStatefulWidget {
  const ParentsListScreen({super.key});

  @override
  ConsumerState<ParentsListScreen> createState() => _ParentsListScreenState();
}

class _ParentsListScreenState extends ConsumerState<ParentsListScreen> {
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
        appBar: AppBar(title: Text(loc.parents)),
        body: const ErrorView(message: 'Institution ID not found'),
      );
    }

    final filter = ParentFilter(
      searchTerm: _searchTerm,
      pageNumber: _pageNumber,
      pageSize: _pageSize,
    );

    final parentsAsync = ref.watch(parentsListProvider(filter));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.parents),
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
      body: parentsAsync.when(
        loading: () => const LoadingView(),
        error: (error, stack) => ErrorView(
          message: error.toString(),
          onRetry: () => ref.invalidate(parentsListProvider(filter)),
        ),
        data: (paginatedList) {
          if (paginatedList.items.isEmpty) {
            return const EmptyView(message: 'No parents found');
          }
          return Column(
            children: [
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: paginatedList.items.length,
                  itemBuilder: (context, index) {
                    final parent = paginatedList.items[index];
                    return InfoTile(
                      title: parent.fullName,
                      subtitle:
                          "parent.email • parent.isActive ? loc.active : loc.inactive",
                      icon: Icons.family_restroom,
                      onTap: () => context.push('/people/parents/${parent.id}'),
                      trailing: PopupMenuButton(
                        itemBuilder: (context) => [
                          PopupMenuItem(
                            child: Text(
                                parent.isActive ? loc.delete : loc.restore),
                            onTap: () {
                              if (parent.isActive) {
                                _deleteParent(context, ref, parent.id, filter);
                              } else {
                                _restoreParent(context, ref, parent.id, filter);
                              }
                            },
                          ),
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
      floatingActionButton: FloatingActionButton(
        onPressed: () =>
            _showParentDialog(context, ref, institutionId, filter: filter),
        child: const Icon(Icons.add),
      ),
    );
  }

  void _showParentDialog(
      BuildContext context, WidgetRef ref, String institutionId,
      {required ParentFilter filter}) async {
    final formKey = GlobalKey<FormBuilderState>();
    final loc = AppLocalizations.of(context);

    dynamic availableProfiles;
    final profileFilter = ProfileFilter(
      institutionId: institutionId,
      role: UserRole.parent,
      pageSize: 1000,
    );
    try {
      final result = await ref.read(profileRepositoryProvider).getProfiles(
            profileFilter.institutionId,
            role: profileFilter.role,
            pageSize: profileFilter.pageSize,
          );
      result.fold(
        (l) => availableProfiles = [],
        (r) => availableProfiles = r.items,
      );
    } catch (e) {
      availableProfiles = [];
    }

    if (!context.mounted) return;

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(loc.create),
        content: SizedBox(
          width: 400,
          child: ParentForm(
            formKey: formKey,
            availableProfiles: availableProfiles,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(parentRepositoryProvider);

                final result = await repository.createParent(
                  values['profileId'] as String,
                );

                if (result.isRight() && context.mounted) {
                  ref.invalidate(parentsListProvider(filter));
                } else if (result.isLeft() && context.mounted) {
                  result.fold(
                    (failure) => ScaffoldMessenger.of(ctx)
                        .showSnackBar(SnackBar(content: Text(failure.message))),
                    (_) {},
                  );
                }
              }
            },
            onCancel: () => Navigator.of(ctx).pop(),
          ),
        ),
      ),
    );
  }

  void _deleteParent(
      BuildContext context, WidgetRef ref, String id, ParentFilter filter) {
    final loc = AppLocalizations.of(context);
    showDialog(
      context: context,
      builder: (ctx) => ConfirmationDialog(
        title: loc.delete,
        content: loc.deleteConfirmation,
        confirmText: loc.delete,
        cancelText: loc.cancel,
        isDestructive: true,
        onConfirm: () async {
          final repository = ref.read(parentRepositoryProvider);
          final result = await repository.deleteParent(id);
          if (result.isRight() && mounted) {
            ref.invalidate(parentsListProvider(filter));
          }
        },
      ),
    );
  }

  void _restoreParent(
      BuildContext context, WidgetRef ref, String id, ParentFilter filter) {
    final loc = AppLocalizations.of(context);
    showDialog(
      context: context,
      builder: (ctx) => ConfirmationDialog(
        title: loc.restore,
        content: loc.restoreConfirmation,
        confirmText: loc.restore,
        cancelText: loc.cancel,
        onConfirm: () async {
          final repository = ref.read(parentRepositoryProvider);
          final result = await repository.restoreParent(id);
          if (result.isRight() && mounted) {
            ref.invalidate(parentsListProvider(filter));
          }
        },
      ),
    );
  }
}
