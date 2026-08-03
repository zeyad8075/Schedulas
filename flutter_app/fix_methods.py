import os
import re

files_to_fix = [
    'lib/features/people/profile/presentation/screens/profiles_list_screen.dart',
    'lib/features/people/students/presentation/screens/students_list_screen.dart',
    'lib/features/people/teachers/presentation/screens/teachers_list_screen.dart',
]

def fix_file(file_path):
    if not os.path.exists(file_path): return
    with open(file_path, 'r') as f:
        content = f.read()

    # Find the _confirmDelete method
    # It starts with `void _confirmDelete` and ends when we reach `void _confirmRestore`
    # Replace the whole body of _confirmDelete and _confirmRestore.
    
    if file_path.endswith('profiles_list_screen.dart'):
        content = re.sub(r'void _confirmDelete\(BuildContext context, WidgetRef ref, String id\) \{.*?\n  \}', 
        r'''void _confirmDelete(BuildContext context, WidgetRef ref, String id) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && context.mounted) {
      final repository = ref.read(profileRepositoryProvider);
      final result = await repository.suspendProfile(id);
      if (result.isRight() && context.mounted) {
        ref.invalidate(profileRepositoryProvider);
      } else if (result.isLeft() && context.mounted) {
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {});
      }
    }
  }''', content, flags=re.DOTALL)
        
        content = re.sub(r'void _confirmRestore\(BuildContext context, WidgetRef ref, String id\) \{.*?\n  \}', 
        r'''void _confirmRestore(BuildContext context, WidgetRef ref, String id) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && context.mounted) {
      final repository = ref.read(profileRepositoryProvider);
      final result = await repository.activateProfile(id);
      if (result.isRight() && context.mounted) {
        ref.invalidate(profileRepositoryProvider);
      } else if (result.isLeft() && context.mounted) {
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {});
      }
    }
  }''', content, flags=re.DOTALL)

    elif file_path.endswith('students_list_screen.dart'):
        content = re.sub(r'void _confirmDelete\(BuildContext context, WidgetRef ref, String id, StudentFilter filter, bool mounted\) \{.*?\n  \}', 
        r'''void _confirmDelete(BuildContext context, WidgetRef ref, String id, StudentFilter filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && mounted) {
      final repository = ref.read(studentRepositoryProvider);
      final result = await repository.deleteStudent(id);
      if (result.isRight() && mounted) {
        ref.invalidate(studentsListProvider(filter));
      } else if (result.isLeft() && mounted) {
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {});
      }
    }
  }''', content, flags=re.DOTALL)
        
        content = re.sub(r'void _confirmRestore\(BuildContext context, WidgetRef ref, String id, StudentFilter filter, bool mounted\) \{.*?\n  \}', 
        r'''void _confirmRestore(BuildContext context, WidgetRef ref, String id, StudentFilter filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && mounted) {
      final repository = ref.read(studentRepositoryProvider);
      final result = await repository.restoreStudent(id);
      if (result.isRight() && mounted) {
        ref.invalidate(studentsListProvider(filter));
      } else if (result.isLeft() && mounted) {
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {});
      }
    }
  }''', content, flags=re.DOTALL)

    elif file_path.endswith('teachers_list_screen.dart'):
        content = re.sub(r'void _confirmDelete\(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted\) \{.*?\n  \}', 
        r'''void _confirmDelete(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && mounted) {
      final repository = ref.read(teacherRepositoryProvider);
      final result = await repository.deleteTeacher(id);
      if (result.isRight() && mounted) {
        ref.invalidate(teachersProvider);
      } else if (result.isLeft() && mounted) {
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {});
      }
    }
  }''', content, flags=re.DOTALL)
        
        content = re.sub(r'void _confirmRestore\(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted\) \{.*?\n  \}', 
        r'''void _confirmRestore(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && mounted) {
      final repository = ref.read(teacherRepositoryProvider);
      final result = await repository.restoreTeacher(id);
      if (result.isRight() && mounted) {
        ref.invalidate(teachersProvider);
      } else if (result.isLeft() && mounted) {
        result.fold((failure) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(failure.message))), (_) {});
      }
    }
  }''', content, flags=re.DOTALL)

    with open(file_path, 'w') as f:
        f.write(content)

for file in files_to_fix:
    fix_file(file)

print("done")
