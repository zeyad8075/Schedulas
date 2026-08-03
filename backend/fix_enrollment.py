import os

file_path = "/home/zeyad/Downloads/Schedulas/backend/src/Schedulas.Application/Features/OrgHierarchy/Commands/EnrollmentCommands.cs"

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
    'throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");',
    'throw new InvalidStateTransitionException("CROSS_INSTITUTION_ENROLLMENT", "Cannot enroll across institutions.");'
)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed EnrollmentCommands")
