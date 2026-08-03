import os
import re

def fix_file(file_path, entity_provider, delete_func, restore_func):
    if not os.path.exists(file_path): return
    with open(file_path, 'r') as f:
        content = f.read()

    # The issue was that confirmText, cancelText, and isDestructive were mixed in the ConfirmationDialog
    # We will just replace the whole `showDialog(...)` block using a simpler regex.
    # We will match `showDialog( ... );` but since `);` can appear inside, it's safer to match `void _confirmDelete(...) { ... }`

    if file_path.endswith('profiles_list_screen.dart'):
        provider = 'profileRepositoryProvider'
        delete_call = 'suspendProfile(id)'
        restore_call = 'activateProfile(id)'
        list_provider = 'profileRepositoryProvider'
        
        # for profiles
        content = re.sub(
            r'void _confirmDelete\(BuildContext context, WidgetRef ref, String id\).*?\}\s*\}',
            f'''void _confirmDelete(BuildContext context, WidgetRef ref, String id) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && context.mounted) {{
      final repository = ref.read({provider});
      final result = await repository.{delete_call};
      if (result.isRight() && context.mounted) {{
        ref.invalidate({list_provider});
      }} else if (result.isLeft() && context.mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

        content = re.sub(
            r'void _confirmRestore\(BuildContext context, WidgetRef ref, String id\).*?\}\s*\}',
            f'''void _confirmRestore(BuildContext context, WidgetRef ref, String id) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && context.mounted) {{
      final repository = ref.read({provider});
      final result = await repository.{restore_call};
      if (result.isRight() && context.mounted) {{
        ref.invalidate({list_provider});
      }} else if (result.isLeft() && context.mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

    elif file_path.endswith('students_list_screen.dart'):
        # For students
        content = re.sub(
            r'void _confirmDelete\(BuildContext context, WidgetRef ref, String id, .*? mounted\).*?\}\s*\}',
            f'''void _confirmDelete(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && mounted) {{
      final repository = ref.read(studentRepositoryProvider);
      final result = await repository.deleteStudent(id);
      if (result.isRight() && mounted) {{
        ref.invalidate(studentsListProvider(filter));
      }} else if (result.isLeft() && mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

        content = re.sub(
            r'void _confirmRestore\(BuildContext context, WidgetRef ref, String id, .*? mounted\).*?\}\s*\}',
            f'''void _confirmRestore(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && mounted) {{
      final repository = ref.read(studentRepositoryProvider);
      final result = await repository.restoreStudent(id);
      if (result.isRight() && mounted) {{
        ref.invalidate(studentsListProvider(filter));
      }} else if (result.isLeft() && mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

    elif file_path.endswith('teachers_list_screen.dart'):
        # For teachers
        content = re.sub(
            r'void _confirmDelete\(BuildContext context, WidgetRef ref, String id, .*? mounted\).*?\}\s*\}',
            f'''void _confirmDelete(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && mounted) {{
      final repository = ref.read(teacherRepositoryProvider);
      final result = await repository.deleteTeacher(id);
      if (result.isRight() && mounted) {{
        ref.invalidate(teachersProvider);
      }} else if (result.isLeft() && mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

        content = re.sub(
            r'void _confirmRestore\(BuildContext context, WidgetRef ref, String id, .*? mounted\).*?\}\s*\}',
            f'''void _confirmRestore(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && mounted) {{
      final repository = ref.read(teacherRepositoryProvider);
      final result = await repository.restoreTeacher(id);
      if (result.isRight() && mounted) {{
        ref.invalidate(teachersProvider);
      }} else if (result.isLeft() && mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

    elif file_path.endswith('parents_list_screen.dart'):
        # For parents
        content = re.sub(
            r'void _confirmDelete\(BuildContext context, WidgetRef ref, String id, .*? mounted\).*?\}\s*\}',
            f'''void _confirmDelete(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && mounted) {{
      final repository = ref.read(parentRepositoryProvider);
      final result = await repository.deleteParent(id);
      if (result.isRight() && mounted) {{
        ref.invalidate(parentsProvider);
      }} else if (result.isLeft() && mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

        content = re.sub(
            r'void _confirmRestore\(BuildContext context, WidgetRef ref, String id, .*? mounted\).*?\}\s*\}',
            f'''void _confirmRestore(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {{
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && mounted) {{
      final repository = ref.read(parentRepositoryProvider);
      final result = await repository.restoreParent(id);
      if (result.isRight() && mounted) {{
        ref.invalidate(parentsProvider);
      }} else if (result.isLeft() && mounted) {{
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {{}});
      }}
    }}
  }}''',
            content,
            flags=re.DOTALL
        )

    with open(file_path, 'w') as f:
        f.write(content)

fix_file('lib/features/people/profile/presentation/screens/profiles_list_screen.dart', '', '', '')
fix_file('lib/features/people/students/presentation/screens/students_list_screen.dart', '', '', '')
fix_file('lib/features/people/teachers/presentation/screens/teachers_list_screen.dart', '', '', '')
fix_file('lib/features/people/parents/presentation/screens/parents_list_screen.dart', '', '', '')

print("done")
