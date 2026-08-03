import re
import os

files_to_fix = [
    "lib/features/academic/presentation/screens/academic_calendar_screen.dart",
    "lib/features/academic/presentation/screens/classes_screen.dart",
    "lib/features/academic/presentation/screens/courses_screen.dart",
    "lib/features/academic/presentation/screens/departments_screen.dart",
    "lib/features/academic/presentation/screens/institutions_screen.dart",
    "lib/features/academic/presentation/screens/programs_screen.dart",
    "lib/features/calendar/presentation/screens/calendar_screen.dart"
]

for f in files_to_fix:
    if os.path.exists(f):
        with open(f, 'r') as file:
            content = file.read()
        
        # Remove unused loc declarations:
        content = re.sub(r'\s*final loc = AppLocalizations\.of\(context\)!(;|\.)', '', content)
        content = re.sub(r'\s*final loc = AppLocalizations\.of\(context\)(;|\.)', '', content)
        
        with open(f, 'w') as file:
            file.write(content)

dashboard_repo = "lib/features/dashboard/data/repositories/dashboard_repository_impl.dart"
if os.path.exists(dashboard_repo):
    with open(dashboard_repo, 'r') as file:
        content = file.read()
    content = re.sub(r'\s*final UserProfile \_userProfile;', '', content)
    content = re.sub(r'this\.\_userProfile, ', '', content)
    with open(dashboard_repo, 'w') as file:
        file.write(content)

notif_details = "lib/features/notifications/presentation/screens/notification_details_screen.dart"
if os.path.exists(notif_details):
    with open(notif_details, 'r') as file:
        content = file.read()
    content = content.replace("Text(notification.title ?? '')", "Text(notification.title)")
    content = content.replace("Text(notification.message ?? '')", "Text(notification.message)")
    with open(notif_details, 'w') as file:
        file.write(content)

reports_screen = "lib/features/reports/presentation/screens/reports_screen.dart"
if os.path.exists(reports_screen):
    with open(reports_screen, 'r') as file:
        content = file.read()
    content = content.replace("Text(userProfile.fullName ?? '')", "Text(userProfile.fullName)")
    with open(reports_screen, 'w') as file:
        file.write(content)

notif_screen = "lib/features/notifications/presentation/screens/notifications_screen.dart"
if os.path.exists(notif_screen):
    with open(notif_screen, 'r') as file:
        content = file.read()
    # Fix use_build_context_synchronously around line 133
    content = content.replace("if (result == true) {", "if (!context.mounted) return;\n      if (result == true) {")
    with open(notif_screen, 'w') as file:
        file.write(content)
