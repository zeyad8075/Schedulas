import re
import os
import glob

def fix_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    # Pattern: if (condition)\n    statement;\n
    # We want to replace it with: if (condition) {\n    statement;\n}\n
    # This is tricky because the statement can be anything. But looking at the code, they are almost all `return` or `context.push` or single-line assignments.
    
    # A safer approach is to fix the specific known patterns.
    # 1. if (...) return ...;
    content = re.sub(r'if\s*\((.*?)\)\s*\n\s*return([^;]+);', r'if (\1) {\n                return\2;\n              }', content)
    
    # Let's just fix them manually, it's safer.
    
    with open(filepath, 'w') as f:
        f.write(content)

for root, _, files in os.walk('lib'):
    for file in files:
        if file.endswith('.dart'):
            fix_file(os.path.join(root, file))
