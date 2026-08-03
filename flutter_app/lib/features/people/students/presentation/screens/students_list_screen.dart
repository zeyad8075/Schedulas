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
import '../providers/student_providers.dart';
import '../widgets/student_form.dart';

class StudentsListScreen extends ConsumerStatefulWidget {
  const StudentsListScreen({super.key});

  @override
  ConsumerState<StudentsListScreen> createState() => _StudentsListScreenState();
}

class _StudentsListScreenState extends ConsumerState<StudentsListScreen> {
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
        appBar: AppBar(title: Text(loc.students)),
        body: const ErrorView(message: 'Institution ID not found'),
      );
    }

    final filter = StudentFilter(
      institutionId: institutionId,
      searchTerm: _searchTerm,
      pageNumber: _pageNumber,
      pageSize: _pageSize,
    );

    final studentsAsync = ref.watch(studentsListProvider(filter));

    return Scaffold(
      appBar: AppBar(
        title: Text(loc.students),
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
      body: studentsAsync.when(
        loading: () => const LoadingView(),
        error: (error, stack) => ErrorView(
          message: error.toString(),
          onRetry: () => ref.invalidate(studentsListProvider(filter)),
        ),
        data: (paginatedList) {
          if (paginatedList.items.isEmpty) {
            return const EmptyView(message: 'No students found');
          }
          return Column(
            children: [
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: paginatedList.items.length,
                  itemBuilder: (context, index) {
                    final student = paginatedList.items[index];
                    return InfoTile(
                      title: student.fullName,
                      subtitle:
                          "student.studentNumber ?? loc.noStudentNumber • student.isActive ? loc.active : loc.inactive",
                      icon: Icons.school,
                      trailing: PopupMenuButton(
                        itemBuilder: (context) => [
                          PopupMenuItem(
                            child: Text(loc.edit),
                            onTap: () => _showStudentDialog(
                                context, ref, institutionId,
                                student: student, filter: filter),
                          ),
                          PopupMenuItem(
                            child: Text(
                                student.isActive ? loc.delete : loc.restore),
                            onTap: () {
                              if (student.isActive) {
                                _deleteStudent(
                                    context, ref, student.id, filter);
                              } else {
                                _restoreStudent(
                                    context, ref, student.id, filter);
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
            _showStudentDialog(context, ref, institutionId, filter: filter),
        child: const Icon(Icons.add),
      ),
    );
  }

  void _showStudentDialog(
      BuildContext context, WidgetRef ref, String institutionId,
      {dynamic student, required StudentFilter filter}) async {
    final formKey = GlobalKey<FormBuilderState>();
    final loc = AppLocalizations.of(context);
    final isEditing = student != null;

    dynamic availableProfiles;
    if (!isEditing) {
      // Fetch available student profiles for this institution to link
      final profileFilter = ProfileFilter(
        institutionId: institutionId,
        role: UserRole.student,
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

    if (!context.mounted) return;

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(isEditing ? loc.edit : loc.create),
        content: SizedBox(
          width: 400,
          child: StudentForm(
            formKey: formKey,
            initialData: student,
            availableProfiles: availableProfiles,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository = ref.read(studentRepositoryProvider);

                final result = isEditing
                    ? await repository.updateStudent(
                        student.id,
                        values['studentNumber'] as String?,
                      )
                    : await repository.createStudent(
                        values['profileId'] as String,
                        values['studentNumber'] as String?,
                      );

                if (result.isRight() && context.mounted) {
                  ref.invalidate(studentsListProvider(filter));
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

  void _deleteStudent(
      BuildContext context, WidgetRef ref, String id, StudentFilter filter) {
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
          final repository = ref.read(studentRepositoryProvider);
          final result = await repository.deleteStudent(id);
          if (result.isRight() && mounted) {
            ref.invalidate(studentsListProvider(filter));
          }
        },
      ),
    );
  }

  void _restoreStudent(
      BuildContext context, WidgetRef ref, String id, StudentFilter filter) {
    final loc = AppLocalizations.of(context);
    showDialog(
      context: context,
      builder: (ctx) => ConfirmationDialog(
        title: loc.restore,
        content: loc.restoreConfirmation,
        confirmText: loc.restore,
        cancelText: loc.cancel,
        onConfirm: () async {
          final repository = ref.read(studentRepositoryProvider);
          final result = await repository.restoreStudent(id);
          if (result.isRight() && mounted) {
            ref.invalidate(studentsListProvider(filter));
          }
        },
      ),
    );
  }
}
