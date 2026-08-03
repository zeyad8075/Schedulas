import os
import re

for root, _, files in os.walk('lib/features/people'):
    for file in files:
        if file.endswith('_list_screen.dart'):
            path = os.path.join(root, file)
            with open(path, 'r') as f:
                content = f.read()
            
            # Replaces ConfirmationDialog showDialog
            content = re.sub(
                r'showDialog\(\s*context:\s*context,\s*builder:\s*\(ctx\)\s*=>\s*ConfirmationDialog\(\s*title:\s*(loc\.\w+),\s*content:\s*(loc\.\w+),\s*onConfirm:\s*\(\)\s*async\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\},\s*\),\s*\);',
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

            if file == 'teachers_list_screen.dart':
                content = content.replace('institutionId: user!.institutionId', 'institutionId: ref.read(currentProfileProvider)!.institutionId')
            
            with open(path, 'w') as f:
                f.write(content)

print("done")
