import os

path = 'lib/features/people/presentation/screens/people_screen.dart'
with open(path, 'r') as f:
    content = f.read()

content = content.replace('ProfilesListScreen(),', 'const ProfilesListScreen(),')
content = content.replace('StudentsListScreen(),', 'const StudentsListScreen(),')
content = content.replace('TeachersListScreen(),', 'const TeachersListScreen(),')
content = content.replace('ParentsListScreen(),', 'const ParentsListScreen(),')

with open(path, 'w') as f:
    f.write(content)

path2 = 'lib/features/people/profile/presentation/screens/profile_screen.dart'
with open(path2, 'r') as f:
    content = f.read()

content = content.replace('PopupMenuButton<Theme>', 'PopupMenuButton<model_theme.Theme>')
content = content.replace('_updateTheme(BuildContext context, WidgetRef ref, Theme theme)', '_updateTheme(BuildContext context, WidgetRef ref, model_theme.Theme theme)')

with open(path2, 'w') as f:
    f.write(content)

print("done")
