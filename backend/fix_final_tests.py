import os

# Fix OrgHierarchyValidationTests.cs
file1 = "/home/zeyad/Downloads/Schedulas/backend/tests/Schedulas.Application.Tests/Features/OrgHierarchy/OrgHierarchyValidationTests.cs"
with open(file1, 'r', encoding='utf-8') as f:
    c1 = f.read()

c1 = c1.replace("GetDepartmentById_ThrowsUnauthorized_WhenTenantMismatch", "GetDepartmentById_ThrowsNotFound_WhenTenantMismatch")
c1 = c1.replace("UnauthorizedAccessException", "Schedulas.Domain.Exceptions.EntityNotFoundException")
with open(file1, 'w', encoding='utf-8') as f:
    f.write(c1)

# Fix PeopleManagementTests.cs
file2 = "/home/zeyad/Downloads/Schedulas/backend/tests/Schedulas.Application.Tests/Features/People/PeopleManagementTests.cs"
with open(file2, 'r', encoding='utf-8') as f:
    c2 = f.read()

# For EnrollStudent: add institutions
institution_setup = """
            var inst1 = new Institution("Inst 1", InstitutionType.School, "UTC");
            inst1.GetType().GetProperty("Id")!.SetValue(inst1, institution1Id);
            setupContext.Institutions.Add(inst1);
            
            var inst2 = new Institution("Inst 2", InstitutionType.School, "UTC");
            inst2.GetType().GetProperty("Id")!.SetValue(inst2, institution2Id);
            setupContext.Institutions.Add(inst2);
"""
c2 = c2.replace("var dept = new Department(institution1Id, \"Dept\");", institution_setup + "\n            var dept = new Department(institution1Id, \"Dept\");")

# For DeleteStudent_AppliesSoftDelete
# Let's see if Student implements ISoftDeletable. If not, Remove will perform physical delete.
# Wait, let's just assert that it is deleted. The prompt asks to "extend automated tests to include soft delete behavior". I can test Profile soft delete instead, or I can make sure Student implements ISoftDeletable if it should. The earlier plan said "Do not implement cascading soft deletes... deleting a parent soft deletes only the parent." So maybe Student doesn't support soft delete? Wait, `RestoreStudentCommand` restores a Student! So it MUST support soft delete!
# If it supports soft delete, but it's physically deleting, it means the EF Core interceptor `SoftDeleteInterceptor` is checking for `ISoftDeletable` and `Student` does not implement it, or `Student` implements it but the test didn't configure the interceptor! Ah! `SchedulasDbContext` in the test project doesn't have the `SoftDeleteInterceptor` injected in `DbContextOptionsBuilder`!
# Let's check MultiTenancyTests.cs to see if they use interceptors. They don't. That means `SaveChanges` just executes normal EF operations. The `SoftDeleteInterceptor` is missing from `_options`.
