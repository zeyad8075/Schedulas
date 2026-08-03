import re
import os

f = "lib/features/academic/presentation/screens/institutions_screen.dart"
with open(f, 'r') as file:
    content = file.read()
# Fix the f'' to '' and replace with dart interpolation syntax
content = content.replace("f'Are you sure you want to delete {institution.name}? This will mark it as soft-deleted.'", "'Are you sure you want to delete ${institution.name}? This will mark it as soft-deleted.'")

# line 43 loc undefined fix:
content = re.sub(r'(builder: \(context\) \{\n)', r'\1      final loc = AppLocalizations.of(context)!;\n', content)

with open(f, 'w') as file:
    file.write(content)

