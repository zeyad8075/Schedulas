import re
import os

notif_details = "lib/features/notifications/presentation/screens/notification_details_screen.dart"
if os.path.exists(notif_details):
    with open(notif_details, 'r') as file:
        content = file.read()
    content = content.replace("notification.title ?? ''", "notification.title")
    content = content.replace("notification.message ?? ''", "notification.message")
    with open(notif_details, 'w') as file:
        file.write(content)

reports_screen = "lib/features/reports/presentation/screens/reports_screen.dart"
if os.path.exists(reports_screen):
    with open(reports_screen, 'r') as file:
        content = file.read()
    content = content.replace("userProfile.fullName ?? ''", "userProfile.fullName")
    with open(reports_screen, 'w') as file:
        file.write(content)

notif_screen = "lib/features/notifications/presentation/screens/notifications_screen.dart"
if os.path.exists(notif_screen):
    with open(notif_screen, 'r') as file:
        content = file.read()
    content = content.replace("if (result == true) {", "if (!context.mounted) return;\n      if (result == true) {")
    with open(notif_screen, 'w') as file:
        file.write(content)
