import re
import os

notif_details = "lib/features/notifications/presentation/screens/notification_details_screen.dart"
if os.path.exists(notif_details):
    with open(notif_details, 'r') as file:
        content = file.read()
    content = re.sub(r'notification\.title\s*\?\?\s*\'\'', 'notification.title', content)
    content = re.sub(r'notification\.message\s*\?\?\s*\'\'', 'notification.message', content)
    with open(notif_details, 'w') as file:
        file.write(content)

reports_screen = "lib/features/reports/presentation/screens/reports_screen.dart"
if os.path.exists(reports_screen):
    with open(reports_screen, 'r') as file:
        content = file.read()
    content = re.sub(r'userProfile\.fullName\s*\?\?\s*\'\'', 'userProfile.fullName', content)
    with open(reports_screen, 'w') as file:
        file.write(content)

notif_screen = "lib/features/notifications/presentation/screens/notifications_screen.dart"
if os.path.exists(notif_screen):
    with open(notif_screen, 'r') as file:
        content = file.read()
    content = re.sub(r'(if\s*\(result\s*==\s*true\)\s*\{)', r'if (!context.mounted) return;\n      \1', content)
    with open(notif_screen, 'w') as file:
        file.write(content)

dashboard_test = "test/features/dashboard/data/repositories/dashboard_repository_impl_test.dart"
if os.path.exists(dashboard_test):
    with open(dashboard_test, 'r') as file:
        content = file.read()
    content = re.sub(r'DashboardRepositoryImpl\(mockApiClient,\s*mockUserProfile\)', 'DashboardRepositoryImpl(mockApiClient)', content)
    with open(dashboard_test, 'w') as file:
        file.write(content)

calendar_screen = "lib/features/calendar/presentation/screens/calendar_screen.dart"
if os.path.exists(calendar_screen):
    with open(calendar_screen, 'r') as file:
        content = file.read()
    content = re.sub(r'\s*final loc = AppLocalizations\.of\(context\)!(;|\.)', '', content)
    with open(calendar_screen, 'w') as file:
        file.write(content)

