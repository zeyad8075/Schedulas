import os
import re

def fix(file):
    with open(file, 'r') as f:
        content = f.read()

    # We renamed `void _confirmDelete` to `void __oldDelete`. We will remove it and `void _confirmRestore`
    # and insert the new ones at the end of the class.

    content = re.sub(r'void __oldDelete.*?\n  \}', '', content, flags=re.DOTALL)
    content = re.sub(r'void _confirmRestore.*?\n  \}', '', content, flags=re.DOTALL)
    
    # insert new ones
    if 'profiles_list_screen.dart' in file:
        methods = '''
  void _confirmDelete(BuildContext context, WidgetRef ref, String id) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && context.mounted) {
      final repository = ref.read(profileRepositoryProvider);
      final result = await repository.suspendProfile(id);
      if (result.isRight() && context.mounted) { ref.invalidate(profileRepositoryProvider); }
    }
  }
  void _confirmRestore(BuildContext context, WidgetRef ref, String id) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && context.mounted) {
      final repository = ref.read(profileRepositoryProvider);
      final result = await repository.activateProfile(id);
      if (result.isRight() && context.mounted) { ref.invalidate(profileRepositoryProvider); }
    }
  }
'''
        content = content.replace('Widget build(BuildContext context) {', methods + '\n  @override\n  Widget build(BuildContext context) {', 1)
        
    elif 'students_list_screen.dart' in file:
        methods = '''
  void _confirmDelete(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && mounted) {
      final repository = ref.read(studentRepositoryProvider);
      final result = await repository.deleteStudent(id);
      if (result.isRight() && mounted) { ref.invalidate(studentsListProvider(filter)); }
    }
  }
  void _confirmRestore(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && mounted) {
      final repository = ref.read(studentRepositoryProvider);
      final result = await repository.restoreStudent(id);
      if (result.isRight() && mounted) { ref.invalidate(studentsListProvider(filter)); }
    }
  }
'''
        content = content.replace('Widget build(BuildContext context) {', methods + '\n  @override\n  Widget build(BuildContext context) {', 1)

    elif 'teachers_list_screen.dart' in file:
        methods = '''
  void _confirmDelete(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.delete, content: loc.deleteConfirmation, isDestructive: true);
    if (confirm == true && mounted) {
      final repository = ref.read(teacherRepositoryProvider);
      final result = await repository.deleteTeacher(id);
      if (result.isRight() && mounted) { ref.invalidate(teachersProvider); }
    }
  }
  void _confirmRestore(BuildContext context, WidgetRef ref, String id, dynamic filter, bool mounted) async {
    final loc = AppLocalizations.of(context)!;
    final confirm = await ConfirmationDialog.show(context, title: loc.restore, content: loc.restoreConfirmation, isDestructive: false);
    if (confirm == true && mounted) {
      final repository = ref.read(teacherRepositoryProvider);
      final result = await repository.restoreTeacher(id);
      if (result.isRight() && mounted) { ref.invalidate(teachersProvider); }
    }
  }
'''
        content = content.replace('Widget build(BuildContext context) {', methods + '\n  @override\n  Widget build(BuildContext context) {', 1)

    with open(file, 'w') as f:
        f.write(content)

fix('lib/features/people/profile/presentation/screens/profiles_list_screen.dart')
fix('lib/features/people/students/presentation/screens/students_list_screen.dart')
fix('lib/features/people/teachers/presentation/screens/teachers_list_screen.dart')
print("done")
