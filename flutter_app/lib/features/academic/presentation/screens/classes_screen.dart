import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../core/widgets/info_tile.dart';
import '../../../../core/widgets/confirmation_dialog.dart';
import '../../../../core/widgets/loading_view.dart';
import '../../../../l10n/app_localizations.dart';
import '../providers/academic_providers.dart';
import '../../domain/models/class_dto.dart';
import '../../domain/models/course_dto.dart';
import '../../domain/models/academic_term_dto.dart';
import '../widgets/class_form.dart';

class ClassesScreen extends ConsumerStatefulWidget {
  const ClassesScreen({super.key});

  @override
  ConsumerState<ClassesScreen> createState() => _ClassesScreenState();
}

class _ClassesScreenState extends ConsumerState<ClassesScreen> {
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

  Future<void> _showClassDialog(
      [ClassDto? cls,
      List<CourseDto>? courses,
      List<AcademicTermDto>? terms]) async {
    if (courses == null || courses.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Cannot create class: No courses available.')),
      );
      return;
    }

    if (terms == null || terms.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Cannot create class: No academic terms available.')),
      );
      return;
    }
    final formKey = GlobalKey<FormBuilderState>();

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(cls == null ? 'Add Class' : 'Edit Class'),
        content: SizedBox(
          width: 400,
          child: ClassForm(
            formKey: formKey,
            initialData: cls,
            courses: courses,
            terms: terms,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(classRepositoryProvider);

                if (cls == null) {
                  // Create
                  final result = await repository.createClass(
                    values['courseId'] as String,
                    values['academicTermId'] as String,
                    values['name'] as String,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(classesListProvider);
                  }
                } else {
                  // Update
                  final result = await repository.updateClass(
                    cls.id,
                    values['name'] as String,
                    values['isActive'] as bool,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(classesListProvider);
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

  Future<void> _confirmDelete(ClassDto cls) async {
    final confirm = await ConfirmationDialog.show(
      context,
      title: 'Delete Class',
      content:
          'Are you sure you want to delete ${cls.name}? This will mark it as soft-deleted.',
    );

    if (confirm == true) {
      final repository = ref.read(classRepositoryProvider);
      final result = await repository.deleteClass(cls.id);
      if (result.isRight()) {
        ref.invalidate(classesListProvider);
      }
    }
  }

  Future<void> _restore(ClassDto cls) async {
    final repository = ref.read(classRepositoryProvider);
    final result = await repository.restoreClass(cls.id);
    if (result.isRight()) {
      ref.invalidate(classesListProvider);
    }
  }

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    final coursesAsync =
        ref.watch(coursesListProvider(const PaginationParams(pageSize: 100)));
    final termsAsync = ref.watch(
        academicTermsListProvider(const PaginationParams(pageSize: 100)));

    final params = PaginationParams(
      pageNumber: _pageNumber,
      searchTerm: _searchTerm,
      isActive: _isActive,
    );

    final asyncData = ref.watch(classesListProvider(params));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.academicClasses),
        actions: [
          coursesAsync.maybeWhen(
            data: (courses) => termsAsync.maybeWhen(
              data: (terms) => IconButton(
                icon: const Icon(Icons.add),
                onPressed: () =>
                    _showClassDialog(null, courses.items, terms.items),
              ),
              orElse: () => const SizedBox.shrink(),
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
                          coursesAsync.maybeWhen(
                            data: (courses) => termsAsync.maybeWhen(
                              data: (terms) => IconButton(
                                icon: const Icon(Icons.edit),
                                onPressed: () => _showClassDialog(
                                    item, courses.items, terms.items),
                              ),
                              orElse: () => const SizedBox.shrink(),
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
