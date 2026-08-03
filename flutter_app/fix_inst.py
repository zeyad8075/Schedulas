import re
import os

f = "lib/features/academic/presentation/screens/institutions_screen.dart"
with open(f, 'r') as file:
    content = file.read()

# Fix undefined loc around line 43
content = re.sub(r'(Future<void> _showCreateEditDialog\(InstitutionDto\? institution\) async \{)', r'\1\n    final loc = AppLocalizations.of(context)!;', content)

# Fix undefined loc around line 90 (_confirmDelete)
content = re.sub(r'(Future<void> _confirmDelete\(InstitutionDto institution\) async \{)', r'\1\n    final loc = AppLocalizations.of(context)!;', content)

with open(f, 'w') as file:
    file.write(content)

