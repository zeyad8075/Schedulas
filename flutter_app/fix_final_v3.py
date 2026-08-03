import os
import re

for root, _, files in os.walk('lib/features/people'):
    for file in files:
        if file.endswith('.dart'):
            path = os.path.join(root, file)
            with open(path, 'r') as f:
                content = f.read()

            content = re.sub(r'status:\s*.*?,', '', content)

            # fix showDialog
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
            content = content.replace('Navigator.of(ctx).pop();', '')
            
            with open(path, 'w') as f:
                f.write(content)

print("done")
