import os
import re

def remove_first_injected(file):
    if not os.path.exists(file): return
    with open(file, 'r') as f:
        content = f.read()

    # The injected ones are inside the class at the top, followed by `@override Widget build(BuildContext context)`
    content = re.sub(r'  void _confirmDelete\(BuildContext context, WidgetRef ref, String id.*?\}', '', content, count=1, flags=re.DOTALL)
    content = re.sub(r'  void _confirmRestore\(BuildContext context, WidgetRef ref, String id.*?\}', '', content, count=1, flags=re.DOTALL)

    with open(file, 'w') as f:
        f.write(content)

remove_first_injected('lib/features/people/profile/presentation/screens/profiles_list_screen.dart')
remove_first_injected('lib/features/people/students/presentation/screens/students_list_screen.dart')
remove_first_injected('lib/features/people/teachers/presentation/screens/teachers_list_screen.dart')
print("done")
