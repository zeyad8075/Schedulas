import os
import glob

screens_dir = 'lib/features/academic/presentation/screens/'

for filepath in glob.glob(os.path.join(screens_dir, '*_screen.dart')):
    with open(filepath, 'r') as f:
        content = f.read()

    # Fix ConfirmationDialog.show
    content = content.replace(
        'ConfirmationDialog.show(\n      context: context,',
        'ConfirmationDialog.show(\n      context,'
    )
    
    # Fix ctx.mounted
    content = content.replace(
        'if (result.isRight() && mounted) {\n                    Navigator.of(ctx).pop();',
        'if (result.isRight() && ctx.mounted) {\n                    Navigator.of(ctx).pop();'
    )

    with open(filepath, 'w') as f:
        f.write(content)
