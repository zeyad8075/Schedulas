import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../core/widgets/info_tile.dart';
import '../../../../core/widgets/confirmation_dialog.dart';
import '../../../../core/widgets/loading_view.dart';
import '../../../../l10n/app_localizations.dart';
import '../providers/academic_providers.dart';
import '../../domain/models/department_dto.dart';
import '../../domain/models/institution_dto.dart';
import '../widgets/department_form.dart';

class DepartmentsScreen extends ConsumerStatefulWidget {
  const DepartmentsScreen({super.key});

  @override
  ConsumerState<DepartmentsScreen> createState() => _DepartmentsScreenState();
}

class _DepartmentsScreenState extends ConsumerState<DepartmentsScreen> {
  int _pageNumber = 1;
  String? _searchTerm;
  bool? _isActive;

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

  Future<void> _showDepartmentDialog(
      [DepartmentDto? department, List<InstitutionDto>? institutions]) async {
    if (institutions == null || institutions.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content:
                Text('Cannot create department: No institutions available.')),
      );
      return;
    }

    final formKey = GlobalKey<FormBuilderState>();

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(department == null
            ? 'Add Department'
            : 'Edit Department'),
        content: SizedBox(
          width: 400,
          child: DepartmentForm(
            formKey: formKey,
            initialData: department,
            institutions: institutions,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(departmentRepositoryProvider);

                if (department == null) {
                  // Create
                  final result = await repository.createDepartment(
                    values['institutionId'] as String,
                    values['name'] as String,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(departmentsListProvider);
                  }
                } else {
                  // Update
                  final result = await repository.updateDepartment(
                    department.id,
                    values['name'] as String,
                    values['isActive'] as bool,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(departmentsListProvider);
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

  Future<void> _confirmDelete(DepartmentDto department) async {
    final confirm = await ConfirmationDialog.show(
      context,
      title: 'Delete Department',
      content:
          'Are you sure you want to delete ${department.name}? This will mark it as soft-deleted.',
    );

    if (confirm == true) {
      final repository = ref.read(departmentRepositoryProvider);
      final result = await repository.deleteDepartment(department.id);
      if (result.isRight()) {
        ref.invalidate(departmentsListProvider);
      }
    }
  }

  Future<void> _restore(DepartmentDto department) async {
    final repository = ref.read(departmentRepositoryProvider);
    final result = await repository.restoreDepartment(department.id);
    if (result.isRight()) {
      ref.invalidate(departmentsListProvider);
    }
  }

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    // We fetch institutions separately just to populate the filter and the create dialog dropdown.
    // In a real robust app we might use a dedicated dropdown provider that fetches 100 or uses an autocomplete.
    final institutionsAsync = ref
        .watch(institutionsListProvider(const PaginationParams(pageSize: 100)));

    final params = PaginationParams(
      pageNumber: _pageNumber,
      searchTerm: _searchTerm,
      isActive: _isActive,
      // We don't have institutionId on PaginationParams, let's just omit it for now or add it later.
    );

    final asyncData = ref.watch(departmentsListProvider(params));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.academicDepartments),
        actions: [
          institutionsAsync.maybeWhen(
            data: (data) => IconButton(
              icon: const Icon(Icons.add),
              onPressed: () => _showDepartmentDialog(null, data.items),
            ),
            orElse: () => const SizedBox.shrink(),
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
                  value: _isActive,
                  hint: Text(loc.academicIsActive),
                  items: const [
                    DropdownMenuItem(value: null, child: Text('All')),
                    DropdownMenuItem(value: true, child: Text('Active')),
                    DropdownMenuItem(value: false, child: Text('Inactive')),
                  ],
                  onChanged: (val) {
                    setState(() {
                      _isActive = val;
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
                      subtitle: item.isActive ? 'Active' : 'Inactive',
                      trailing: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          institutionsAsync.maybeWhen(
                            data: (data) => IconButton(
                              icon: const Icon(Icons.edit),
                              onPressed: () =>
                                  _showDepartmentDialog(item, data.items),
                            ),
                            orElse: () => const SizedBox.shrink(),
                          ),
                          if (!item.isActive) ...[
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
