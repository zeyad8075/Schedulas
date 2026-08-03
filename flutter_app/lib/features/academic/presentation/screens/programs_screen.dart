import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../core/widgets/info_tile.dart';
import '../../../../core/widgets/confirmation_dialog.dart';
import '../../../../core/widgets/loading_view.dart';
import '../../../../l10n/app_localizations.dart';
import '../providers/academic_providers.dart';
import '../../domain/models/program_dto.dart';
import '../../domain/models/department_dto.dart';
import '../widgets/program_form.dart';

class ProgramsScreen extends ConsumerStatefulWidget {
  const ProgramsScreen({super.key});

  @override
  ConsumerState<ProgramsScreen> createState() => _ProgramsScreenState();
}

class _ProgramsScreenState extends ConsumerState<ProgramsScreen> {
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

  Future<void> _showProgramDialog(
      [ProgramDto? program, List<DepartmentDto>? departments]) async {
    if (departments == null || departments.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Cannot create program: No departments available.')),
      );
      return;
    }
    final formKey = GlobalKey<FormBuilderState>();

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(program == null ? 'Add Program' : 'Edit Program'),
        content: SizedBox(
          width: 400,
          child: ProgramForm(
            formKey: formKey,
            initialData: program,
            departments: departments,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(programRepositoryProvider);

                if (program == null) {
                  // Create
                  final result = await repository.createProgram(
                    values['departmentId'] as String,
                    values['name'] as String,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(programsListProvider);
                  }
                } else {
                  // Update
                  final result = await repository.updateProgram(
                    program.id,
                    values['name'] as String,
                    values['isActive'] as bool,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(programsListProvider);
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

  Future<void> _confirmDelete(ProgramDto program) async {
    final confirm = await ConfirmationDialog.show(
      context,
      title: 'Delete Program',
      content:
          'Are you sure you want to delete ${program.name}? This will mark it as soft-deleted.',
    );

    if (confirm == true) {
      final repository = ref.read(programRepositoryProvider);
      final result = await repository.deleteProgram(program.id);
      if (result.isRight()) {
        ref.invalidate(programsListProvider);
      }
    }
  }

  Future<void> _restore(ProgramDto program) async {
    final repository = ref.read(programRepositoryProvider);
    final result = await repository.restoreProgram(program.id);
    if (result.isRight()) {
      ref.invalidate(programsListProvider);
    }
  }

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    final departmentsAsync = ref
        .watch(departmentsListProvider(const PaginationParams(pageSize: 100)));

    final params = PaginationParams(
      pageNumber: _pageNumber,
      searchTerm: _searchTerm,
      isActive: _isActive,
    );

    final asyncData = ref.watch(programsListProvider(params));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.academicPrograms),
        actions: [
          departmentsAsync.maybeWhen(
            data: (data) => IconButton(
              icon: const Icon(Icons.add),
              onPressed: () => _showProgramDialog(null, data.items),
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
                          departmentsAsync.maybeWhen(
                            data: (data) => IconButton(
                              icon: const Icon(Icons.edit),
                              onPressed: () =>
                                  _showProgramDialog(item, data.items),
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
