import os
import re

files_to_check = [
    "src/Schedulas.Application/Features/OrgHierarchy/Commands/ProgramCommands.cs",
    "src/Schedulas.Application/Features/OrgHierarchy/Commands/ClassCommands.cs",
    "src/Schedulas.Application/Features/OrgHierarchy/Commands/CourseCommands.cs",
    "src/Schedulas.Application/Features/AcademicCalendar/AcademicCalendarFeature.cs",
    "src/Schedulas.Application/Features/Institutions/Commands/InstitutionCommands.cs"
]

base_dir = "/home/zeyad/Downloads/Schedulas/backend"

pattern = re.compile(
    r'(var ([a-zA-Z0-9_]+) = await _db\.[a-zA-Z0-9_]+\s*\.IgnoreQueryFilters\(\)\s*\.FirstOrDefaultAsync\(.*?cancellationToken\)\s*\?\?\s*throw\s*new\s*EntityNotFoundException\(.*?\);)',
    re.MULTILINE | re.DOTALL
)

for file_path in files_to_check:
    full_path = os.path.join(base_dir, file_path)
    if not os.path.exists(full_path):
        continue
    
    with open(full_path, 'r', encoding='utf-8') as f:
        content = f.read()
        
    def replacer(match):
        full_match = match.group(1)
        var_name = match.group(2)
        
        # Don't add if already added (safety)
        if "_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin" in full_match:
            return full_match
            
        check = f'\n\n        if (_currentUser.Role != Domain.Enums.UserRole.PlatformAdmin && _currentUser.InstitutionId != {var_name}.InstitutionId)\n            throw new UnauthorizedAccessException("TENANT_SCOPE_MISMATCH");'
        return full_match + check
    
    new_content = pattern.sub(replacer, content)
    if new_content != content:
        with open(full_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Fixed {file_path}")

