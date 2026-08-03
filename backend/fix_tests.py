import os

file_path = "/home/zeyad/Downloads/Schedulas/backend/tests/Schedulas.Application.Tests/Features/People/PeopleManagementTests.cs"

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("ex.Code.Should().Be", "ex.ReasonCode.Should().Be")
content = content.replace("new Program(dept.Id, \"Prog\")", "new Program(institution1Id, dept.Id, \"Prog\")")
content = content.replace("new Course(prog.Id, \"Course\")", "new Course(institution1Id, prog.Id, \"Course\")")
content = content.replace("new AcademicTerm(institution1Id, \"Term\", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1))", "new AcademicTerm(institution1Id, \"Term\", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)))")
content = content.replace("new Class(course.Id, term.Id, \"Class 1A\")", "new Class(institution1Id, course.Id, term.Id, \"Class 1A\")")

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed tests")
