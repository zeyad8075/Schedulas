import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../../../../../core/widgets/empty_view.dart';
import '../../../../../core/widgets/info_tile.dart';
import '../../../../../core/widgets/confirmation_dialog.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../profile/presentation/providers/profile_providers.dart';
import '../../../domain/models/user_role.dart';
import '../../../../academic/presentation/providers/academic_providers.dart';
import '../providers/teacher_providers.dart';
import '../widgets/teacher_form.dart';

class TeachersListScreen extends ConsumerStatefulWidget {
  const TeachersListScreen({super.key});

  @override
  ConsumerState<TeachersListScreen> createState() => _TeachersListScreenState();
}

class _TeachersListScreenState extends ConsumerState<TeachersListScreen> {
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
        appBar: AppBar(title: Text(loc.teachers)),
        body: const ErrorView(message: 'Institution ID not found'),
      );
    }

    final filter = TeacherFilter(
      institutionId: institutionId,
      searchTerm: _searchTerm,
      pageNumber: _pageNumber,
      pageSize: _pageSize,
    );

    final teachersAsync = ref.watch(teachersListProvider(filter));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.teachers),
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
      body: teachersAsync.when(
        loading: () => const LoadingView(),
        error: (error, stack) => ErrorView(
          message: error.toString(),
          onRetry: () => ref.invalidate(teachersListProvider(filter)),
        ),
        data: (paginatedList) {
          if (paginatedList.items.isEmpty) {
            return const EmptyView(message: 'No teachers found');
          }
          return Column(
            children: [
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: paginatedList.items.length,
                  itemBuilder: (context, index) {
                    final teacher = paginatedList.items[index];
                    return InfoTile(
                      title: teacher.fullName,
                      subtitle:
                          "teacher.email • teacher.isActive ? loc.active : loc.inactive",
                      icon: Icons.person_4,
                      trailing: PopupMenuButton(
                        itemBuilder: (context) => [
                          PopupMenuItem(
                            child: Text(loc.edit),
                            onTap: () => _showTeacherDialog(
                                context, ref, institutionId,
                                teacher: teacher, filter: filter),
                          ),
                          PopupMenuItem(
                            child: Text(
                                teacher.isActive ? loc.delete : loc.restore),
                            onTap: () {
                              if (teacher.isActive) {
                                _deleteTeacher(
                                    context, ref, teacher.id, filter);
                              } else {
                                _restoreTeacher(
                                    context, ref, teacher.id, filter);
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
            _showTeacherDialog(context, ref, institutionId, filter: filter),
        child: const Icon(Icons.add),
      ),
    );
  }

  void _showTeacherDialog(
      BuildContext context, WidgetRef ref, String institutionId,
      {dynamic teacher, required TeacherFilter filter}) async {
    final formKey = GlobalKey<FormBuilderState>();
    final loc = AppLocalizations.of(context);
    final isEditing = teacher != null;

    dynamic availableProfiles;
    if (!isEditing) {
      final profileFilter = ProfileFilter(
        institutionId: institutionId,
        role: UserRole.teacher,
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
    }

    dynamic availableDepartments;
    final result = await ref.read(departmentRepositoryProvider).getDepartments(
          institutionId: ref.read(currentProfileProvider)!.institutionId,
          pageSize: 1000,
        );
    result.fold(
      (l) => availableDepartments = [],
      (r) => availableDepartments = r.items,
    );

    if (!context.mounted) return;

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(isEditing ? loc.edit : loc.create),
        content: SizedBox(
          width: 400,
          child: TeacherForm(
            formKey: formKey,
            initialData: teacher,
            availableProfiles: availableProfiles,
            availableDepartments: availableDepartments,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(teacherRepositoryProvider);

                final result = isEditing
                    ? await repository.updateTeacher(
                        teacher.id,
                        values['departmentId'] as String?,
                      )
                    : await repository.createTeacher(
                        values['profileId'] as String,
                        values['departmentId'] as String?,
                      );

                if (result.isRight() && context.mounted) {
                  ref.invalidate(teachersListProvider(filter));
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

  void _deleteTeacher(
      BuildContext context, WidgetRef ref, String id, TeacherFilter filter) {
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
          final repository = ref.read(teacherRepositoryProvider);
          final result = await repository.deleteTeacher(id);
          if (result.isRight() && mounted) {
            ref.invalidate(teachersListProvider(filter));
          }
        },
      ),
    );
  }

  void _restoreTeacher(
      BuildContext context, WidgetRef ref, String id, TeacherFilter filter) {
    final loc = AppLocalizations.of(context);
    showDialog(
      context: context,
      builder: (ctx) => ConfirmationDialog(
        title: loc.restore,
        content: loc.restoreConfirmation,
        confirmText: loc.restore,
        cancelText: loc.cancel,
        onConfirm: () async {
          final repository = ref.read(teacherRepositoryProvider);
          final result = await repository.restoreTeacher(id);
          if (result.isRight() && mounted) {
            ref.invalidate(teachersListProvider(filter));
          }
        },
      ),
    );
  }
}
