import os
import re

# Fix profile_screen.dart
path = 'lib/features/people/profile/presentation/screens/profile_screen.dart'
if os.path.exists(path):
    with open(path, 'r') as f:
        content = f.read()
    
    content = content.replace('<Theme>', '<model_theme.Theme>')
    content = content.replace('Theme.values', 'model_theme.Theme.values')
    content = content.replace('Theme.light', 'model_theme.Theme.light')
    content = content.replace('Theme.dark', 'model_theme.Theme.dark')
    content = content.replace('Theme.system', 'model_theme.Theme.system')
    
    with open(path, 'w') as f:
        f.write(content)

# Fix onConfirm
for file in [
    'lib/features/people/profile/presentation/screens/profiles_list_screen.dart',
    'lib/features/people/students/presentation/screens/students_list_screen.dart',
    'lib/features/people/teachers/presentation/screens/teachers_list_screen.dart',
]:
    if os.path.exists(file):
        with open(file, 'r') as f:
            content = f.read()
        
        # very simple replace
        content = re.sub(
            r'showDialog\(\s*context:\s*context,\s*builder:\s*\(ctx\)\s*=>\s*ConfirmationDialog\(\s*title:\s*(loc\.\w+),\s*content:\s*(loc\.\w+),\s*onConfirm:\s*\(\)\s*async\s*{(.*?)}\s*,\s*\)\s*,\s*\);',
            r'''final confirm = await ConfirmationDialog.show(context, title: \1, content: \2, isDestructive: true);
    if (confirm == true && context.mounted) {
\3
    }''',
            content,
            flags=re.DOTALL
        )
        content = content.replace('ctx.mounted', 'context.mounted')
        content = content.replace('Navigator.of(ctx).pop();\n', '')
        content = content.replace('Navigator.of(ctx).pop();', '')
        
        with open(file, 'w') as f:
            f.write(content)

print("done")
