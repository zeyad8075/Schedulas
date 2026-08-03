import os

file_path = "/home/zeyad/Downloads/Schedulas/backend/tests/Schedulas.Application.Tests/Features/People/PeopleManagementTests.cs"

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("new Course(institution1Id, prog.Id, \"Course\")", "new Course(institution1Id, prog.Id, \"Course\", null)")

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed Course constructor")
