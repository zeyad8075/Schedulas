import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../core/widgets/info_tile.dart';
import '../../../../core/widgets/confirmation_dialog.dart';
import '../../../../core/widgets/loading_view.dart';
import '../../../../l10n/app_localizations.dart';
import '../providers/academic_providers.dart';
import '../../domain/models/institution_dto.dart';
import '../widgets/institution_form.dart';

class InstitutionsScreen extends ConsumerStatefulWidget {
  const InstitutionsScreen({super.key});

  @override
  ConsumerState<InstitutionsScreen> createState() => _InstitutionsScreenState();
}

class _InstitutionsScreenState extends ConsumerState<InstitutionsScreen> {
  int _pageNumber = 1;
  String? _searchTerm;
  bool? _isSuspended;

  void _loadPage(int page) {
    setState(() {
      _pageNumber = page;
    });
  }

  void _onSearch(String value) {
    setState(() {
      _searchTerm = value.isEmpty ? null : value;
      _pageNumber = 1;
    });
  }

  Future<void> _showInstitutionDialog([InstitutionDto? institution]) async {
    final loc = AppLocalizations.of(context);
    final formKey = GlobalKey<FormBuilderState>();

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(institution == null
            ? loc.academicAddInstitution
            : loc.academicEditInstitution),
        content: SizedBox(
          width: 400,
          child: InstitutionForm(
            formKey: formKey,
            initialData: institution,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(institutionRepositoryProvider);

                if (institution == null) {
                  // Create
                  final result = await repository.createInstitution(
                    values['name'] as String,
                    values['type'] as InstitutionType,
                    values['timezone'] as String,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(institutionsListProvider);
                  }
                } else {
                  // Update
                  final result = await repository.updateInstitution(
                    institution.id,
                    values['name'] as String,
                    institution.logoUrl, // Keep existing logo URL for now
                    values['timezone'] as String,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(institutionsListProvider);
                  }
                }
              }
            },
            onCancel: () => Navigator.of(ctx).pop(),
          ),
        ),
      ),
    );
  }

  Future<void> _confirmDelete(InstitutionDto institution) async {
    final confirm = await ConfirmationDialog.show(
      context,
      title: 'Delete Institution',
      content:
          'Are you sure you want to delete ${institution.name}? This will mark it as soft-deleted.',
    );

    if (confirm == true) {
      final repository = ref.read(institutionRepositoryProvider);
      final result = await repository.deleteInstitution(institution.id);
      if (result.isRight()) {
        ref.invalidate(institutionsListProvider);
      }
    }
  }

  Future<void> _restore(InstitutionDto institution) async {
    final repository = ref.read(institutionRepositoryProvider);
    final result = await repository.restoreInstitution(institution.id);
    if (result.isRight()) {
      ref.invalidate(institutionsListProvider);
    }
  }

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);
    final params = PaginationParams(
      pageNumber: _pageNumber,
      searchTerm: _searchTerm,
      isSuspended: _isSuspended,
    );

    final asyncData = ref.watch(institutionsListProvider(params));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.academicInstitutions),
        actions: [
          IconButton(
            icon: const Icon(Icons.add),
            onPressed: () => _showInstitutionDialog(),
          ),
        ],
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(60),
          child: Padding(
            padding:
                const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    decoration: InputDecoration(
                      hintText: loc.commonSearch,
                      prefixIcon: const Icon(Icons.search),
                      border: const OutlineInputBorder(),
                    ),
                    onChanged: _onSearch,
                  ),
                ),
                const SizedBox(width: 16),
                DropdownButton<bool?>(
                  value: _isSuspended,
                  hint: Text(loc.academicIsSuspended),
                  items: const [
                    DropdownMenuItem(value: null, child: Text('All')),
                    DropdownMenuItem(value: false, child: Text('Active')),
                    DropdownMenuItem(value: true, child: Text('Suspended')),
                  ],
                  onChanged: (val) {
                    setState(() {
                      _isSuspended = val;
                      _pageNumber = 1;
                    });
                  },
                )
              ],
            ),
          ),
        ),
      ),
      body: asyncData.when(
        loading: () => const LoadingView(),
        error: (e, st) => Center(child: Text(e.toString())),
        data: (paginatedList) {
          if (paginatedList.items.isEmpty) {
            return Center(child: Text(loc.commonEmpty));
          }

          return Column(
            children: [
              Expanded(
                child: ListView.builder(
                  itemCount: paginatedList.items.length,
                  itemBuilder: (context, index) {
                    final item = paginatedList.items[index];
                    return InfoTile(
                      title: item.name,
                      subtitle: '${item.type.name} • ${item.timezone}',
                      trailing: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          IconButton(
                            icon: const Icon(Icons.edit),
                            onPressed: () => _showInstitutionDialog(item),
                          ),
                          if (item.isSuspended) ...[
                            IconButton(
                              icon: const Icon(Icons.restore),
                              onPressed: () => _restore(item),
                              tooltip: 'Restore',
                            )
                          ] else ...[
                            IconButton(
                              icon: const Icon(Icons.delete),
                              onPressed: () => _confirmDelete(item),
                            )
                          ]
                        ],
                      ),
                    );
                  },
                ),
              ),
              // Pagination Controls
              Padding(
                padding: const EdgeInsets.all(16.0),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    IconButton(
                      icon: const Icon(Icons.chevron_left),
                      onPressed: paginatedList.hasPreviousPage
                          ? () => _loadPage(_pageNumber - 1)
                          : null,
                    ),
                    Text('Page $_pageNumber of ${paginatedList.totalPages}'),
                    IconButton(
                      icon: const Icon(Icons.chevron_right),
                      onPressed: paginatedList.hasNextPage
                          ? () => _loadPage(_pageNumber + 1)
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
}
