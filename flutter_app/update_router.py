import os

path = 'lib/core/routing/app_router.dart'
with open(path, 'r') as f:
    content = f.read()

# Replace the dummy routes with our real ones
import_section = """
import '../../features/activities/presentation/screens/activities_screen.dart';
import '../../features/calendar/presentation/screens/calendar_screen.dart';
"""

# replace old imports
content = content.replace("import '../../features/activities/domain/activity.dart';", "")
content = content.replace("import '../../features/activities/presentation/screens/activity_form_screen.dart';", "")
content = content.replace("import '../../features/activities/presentation/screens/calendar_screen.dart';", import_section)
content = content.replace("import '../../features/activities/presentation/screens/class_picker_screen.dart';", "")

# replace old routes
start_idx = content.find("GoRoute(\n        path: '/activities/edit',")
end_idx = content.find("GoRoute(\n        path: '/settings',")

content = content[:start_idx] + content[end_idx:]

# replace the StatefulShellRoute dummy ones
calendar_old = "GoRoute(path: '/calendar', builder: (context, state) => const CalendarScreen()),"
activities_old = "GoRoute(path: '/activities', builder: (context, state) => const Scaffold(body: Center(child: Text('الأنشطة - سيتم التنفيذ لاحقاً')))),"
activities_new = "GoRoute(path: '/activities', builder: (context, state) => const ActivitiesScreen()),"

content = content.replace(activities_old, activities_new)

with open(path, 'w') as f:
    f.write(content)

print("done")
