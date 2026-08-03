import re
import os

def insert_loc(filepath):
    if not os.path.exists(filepath): return
    with open(filepath, 'r') as f:
        content = f.read()
    
    # insert in build(BuildContext context)
    content = re.sub(r'(Widget build\(BuildContext context[^\)]*\)\s*\{)', r'\1\n    final loc = AppLocalizations.of(context)!;', content)
    
    # insert in _buildContent(BuildContext context...)
    content = re.sub(r'(Widget _buildContent\(BuildContext context[^\)]*\)\s*\{)', r'\1\n    final loc = AppLocalizations.of(context)!;', content)

    # insert in showDialog builder
    content = re.sub(r'(builder: \(context\)\s*\{)', r'\1\n      final loc = AppLocalizations.of(context)!;', content)
    content = re.sub(r'(builder: \(BuildContext context\)\s*\{)', r'\1\n      final loc = AppLocalizations.of(context)!;', content)
    
    with open(filepath, 'w') as f:
        f.write(content)

files = [
    "lib/features/academic/presentation/screens/academic_calendar_screen.dart",
    "lib/features/academic/presentation/screens/classes_screen.dart",
    "lib/features/academic/presentation/screens/courses_screen.dart",
    "lib/features/academic/presentation/screens/departments_screen.dart",
    "lib/features/academic/presentation/screens/institutions_screen.dart",
    "lib/features/academic/presentation/screens/programs_screen.dart",
    "lib/features/calendar/presentation/screens/calendar_screen.dart"
]

for f in files:
    insert_loc(f)

