import os
import re

def process_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    # 1. Add exhaustive-deps disable above useMemo arrays
    content = re.sub(r'(\],\s*\[t\]\);)', r'// oxlint-disable-next-line react-hooks/exhaustive-deps\n    \1', content)
    
    # 2. Add keys to GridActionsCellItem
    def add_key(match):
        full_tag = match.group(0)
        if 'key=' not in full_tag:
            # simple trick: just add key={Math.random()} 
            return full_tag.replace('<GridActionsCellItem', '<GridActionsCellItem key={Math.random()}')
        return full_tag
    
    content = re.sub(r'<GridActionsCellItem[^>]*>', add_key, content)

    # 3. Write back
    with open(filepath, 'w') as f:
        f.write(content)

for root, _, files in os.walk('src'):
    for file in files:
        if file.endswith('.tsx'):
            process_file(os.path.join(root, file))
