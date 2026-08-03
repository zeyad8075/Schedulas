import os

path = 'lib/features/people/presentation/screens/people_screen.dart'
with open(path, 'r') as f:
    content = f.read()

content = content.replace("import '../../../profile/", "import '../../profile/")
content = content.replace("import '../../../students/", "import '../../students/")
content = content.replace("import '../../../teachers/", "import '../../teachers/")
content = content.replace("import '../../../parents/", "import '../../parents/")

with open(path, 'w') as f:
    f.write(content)

print("done")
