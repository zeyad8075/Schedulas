import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../core/widgets/info_tile.dart';
import '../../../../core/widgets/confirmation_dialog.dart';
import '../../../../core/widgets/loading_view.dart';
import '../../../../l10n/app_localizations.dart';
import '../providers/academic_providers.dart';
import '../../domain/models/course_dto.dart';
import '../../domain/models/program_dto.dart';
import '../widgets/course_form.dart';

class CoursesScreen extends ConsumerStatefulWidget {
  const CoursesScreen({super.key});

  @override
  ConsumerState<CoursesScreen> createState() => _CoursesScreenState();
}

class _CoursesScreenState extends ConsumerState<CoursesScreen> {
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

  Future<void> _showCourseDialog(
      [CourseDto? course, List<ProgramDto>? programs]) async {
    if (programs == null || programs.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Cannot create course: No programs available.')),
      );
      return;
    }
    final formKey = GlobalKey<FormBuilderState>();

    await showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(
            course == null ? 'Add Course' : 'Edit Course'),
        content: SizedBox(
          width: 400,
          child: CourseForm(
            formKey: formKey,
            initialData: course,
            programs: programs,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(courseRepositoryProvider);

                if (course == null) {
                  // Create
                  final result = await repository.createCourse(
                    values['programId'] as String,
                    values['name'] as String,
                    values['code'] as String?,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(coursesListProvider);
                  }
                } else {
                  // Update
                  final result = await repository.updateCourse(
                    course.id,
                    values['name'] as String,
                    values['code'] as String?,
                    values['isActive'] as bool,
                  );
                  if (result.isRight() && ctx.mounted) {
                    Navigator.of(ctx).pop();
                    ref.invalidate(coursesListProvider);
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

  Future<void> _confirmDelete(CourseDto course) async {
    final confirm = await ConfirmationDialog.show(
      context,
      title: 'Delete Course',
      content:
          'Are you sure you want to delete ${course.name}? This will mark it as soft-deleted.',
    );

    if (confirm == true) {
      final repository = ref.read(courseRepositoryProvider);
      final result = await repository.deleteCourse(course.id);
      if (result.isRight()) {
        ref.invalidate(coursesListProvider);
      }
    }
  }

  Future<void> _restore(CourseDto course) async {
    final repository = ref.read(courseRepositoryProvider);
    final result = await repository.restoreCourse(course.id);
    if (result.isRight()) {
      ref.invalidate(coursesListProvider);
    }
  }

  @override
  Widget build(BuildContext context) {
    final loc = AppLocalizations.of(context);

    final programsAsync =
        ref.watch(programsListProvider(const PaginationParams(pageSize: 100)));

    final params = PaginationParams(
      pageNumber: _pageNumber,
      searchTerm: _searchTerm,
      isActive: _isActive,
    );

    final asyncData = ref.watch(coursesListProvider(params));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.academicCourses),
        actions: [
          programsAsync.maybeWhen(
            data: (data) => IconButton(
              icon: const Icon(Icons.add),
              onPressed: () => _showCourseDialog(null, data.items),
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
                      subtitle: (item.code != null && item.code!.isNotEmpty
                              ? '[${item.code}] '
                              : '') +
                          (item.isActive ? 'Active' : 'Inactive'),
                      trailing: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          programsAsync.maybeWhen(
                            data: (data) => IconButton(
                              icon: const Icon(Icons.edit),
                              onPressed: () =>
                                  _showCourseDialog(item, data.items),
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
