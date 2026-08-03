import os
import glob

repos_dir = 'lib/features/academic/data/repositories/'

for filepath in glob.glob(os.path.join(repos_dir, '*_repository_impl.dart')):
    with open(filepath, 'r') as f:
        content = f.read()

    # Fix e.fieldMessages
    content = content.replace(
        'e.fieldMessages!.values.expand((e) => e).toList()',
        'e.fieldMessages!'
    )
    
    with open(filepath, 'w') as f:
        f.write(content)
