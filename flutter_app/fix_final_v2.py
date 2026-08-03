import os
import re

for root, _, files in os.walk('lib/features/people'):
    for file in files:
        if file.endswith('.dart'):
            path = os.path.join(root, file)
            with open(path, 'r') as f:
                content = f.read()

            # Fix showDialog for delete/restore
            content = re.sub(
                r'showDialog\(\s*context:\s*context,\s*builder:\s*\(ctx\)\s*=>\s*ConfirmationDialog\(\s*title:\s*(.*?),\s*content:\s*(.*?),\s*onConfirm:\s*\(\)\s*async\s*{([^}]+?if\s*\(result\.isRight\(\)\s*&&\s*ctx\.mounted\)\s*{\s*Navigator\.of\(ctx\)\.pop\(\);\s*.*?}\s*.*?)\s*},\s*\)\s*,\s*\);',
                r'''final confirm = await ConfirmationDialog.show(context, title: \1, content: \2, isDestructive: true);
    if (confirm == true && context.mounted) {
\3
    }''',
                content,
                flags=re.DOTALL
            )
            # The above regex might still be brittle. Let's just do:
            
            with open(path, 'w') as f:
                f.write(content)

print("done")
