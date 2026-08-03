import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import '../../../../../l10n/app_localizations.dart';
import '../../../../../core/widgets/error_view.dart';
import '../../../../../core/widgets/loading_view.dart';
import '../../../../../core/widgets/empty_view.dart';
import '../../../../../core/widgets/info_tile.dart';
import '../../../../auth/presentation/providers/auth_providers.dart';
import '../../../students/presentation/providers/student_providers.dart';
import '../../../parents/presentation/providers/parent_providers.dart';
import '../providers/parent_student_link_providers.dart';
import '../widgets/parent_student_link_form.dart';

class ParentStudentsScreen extends ConsumerWidget {
  final String parentId;

  const ParentStudentsScreen({super.key, required this.parentId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final loc = AppLocalizations.of(context);
    final parentAsync = ref.watch(parentDetailsProvider(parentId));
    final studentsAsync = ref.watch(parentStudentsProvider(parentId));
    final user = ref.watch(currentProfileProvider);
    final institutionId = user?.institutionId;

    return Scaffold(
      appBar: AppBar(
        title: parentAsync.when(
          data: (parent) => Text('${loc.students} - ${parent.fullName}'),
          loading: () => Text(loc.students),
          error: (_, __) => Text(loc.students),
        ),
      ),
      body: studentsAsync.when(
        loading: () => const LoadingView(),
        error: (error, stack) => ErrorView(
          message: error.toString(),
          onRetry: () => ref.invalidate(parentStudentsProvider(parentId)),
        ),
        data: (students) {
          if (students.isEmpty) {
            return const EmptyView(message: 'No linked students found');
          }
          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: students.length,
            itemBuilder: (context, index) {
              final student = students[index];
              return InfoTile(
                title: student.fullName,
                subtitle:
                    "student.studentNumber ?? student.email • student.isActive ? loc.active : loc.inactive",
                icon: Icons.school,
              );
            },
          );
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _showLinkForm(context, ref, institutionId!),
        child: const Icon(Icons.link),
      ),
    );
  }

  void _showLinkForm(
      BuildContext context, WidgetRef ref, String institutionId) async {
    final formKey = GlobalKey<FormBuilderState>();
    final loc = AppLocalizations.of(context);

    dynamic availableStudents;
    final filter = StudentFilter(institutionId: institutionId, pageSize: 1000);
    try {
      final result = await ref.read(studentRepositoryProvider).getStudents(
            filter.institutionId,
            pageSize: filter.pageSize,
          );
      result.fold(
        (l) => availableStudents = [],
        (r) => availableStudents = r.items,
      );
    } catch (e) {
      availableStudents = [];
    }

    if (!context.mounted) return;

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(loc.create),
        content: SizedBox(
          width: 400,
          child: ParentStudentLinkForm(
            formKey: formKey,
            availableStudents: availableStudents,
            onSubmit: () async {
              if (formKey.currentState?.saveAndValidate() ?? false) {
                final values = formKey.currentState!.value;
                final repository =
                    ref.read(parentStudentLinkRepositoryProvider);

                final result = await repository.linkParentToStudent(
                  parentId,
                  values['studentId'] as String,
                );

                if (result.isRight() && context.mounted) {
                  ref.invalidate(parentStudentsProvider(parentId));
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
}
