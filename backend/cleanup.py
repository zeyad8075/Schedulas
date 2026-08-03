import os
import re

files_to_clean = [
    "src/Schedulas.Application/Features/Rules/Commands/RuleDefinitionCommands.cs",
    "src/Schedulas.Application/Features/Reports/ReportsFeature.cs",
    "src/Schedulas.Application/Features/OrgHierarchy/Queries/OrgHierarchyQueries.cs",
    "src/Schedulas.Application/Features/OrgHierarchy/Commands/DepartmentCommands.cs",
    "src/Schedulas.Application/Features/OrgHierarchy/Commands/ProgramCommands.cs",
    "src/Schedulas.Application/Features/OrgHierarchy/Commands/ClassCommands.cs",
    "src/Schedulas.Application/Features/OrgHierarchy/Commands/CourseCommands.cs",
    "src/Schedulas.Application/Features/AcademicCalendar/AcademicCalendarFeature.cs",
    "src/Schedulas.Application/Features/Activities/Queries/GetActivitiesQueries.cs",
    "src/Schedulas.Application/Features/Activities/Commands/EditAndCancelActivityCommands.cs",
    "src/Schedulas.Application/Features/Auth/Commands/AuthCommands.cs"
]

base_dir = "/home/zeyad/Downloads/Schedulas/backend"

pattern_to_remove = re.compile(
    r'^[ \t]*if\s*\([^)]*\)\s*[\r\n]+[ \t]*throw\s+new\s+UnauthorizedAccessException\("TENANT_SCOPE_MISMATCH"\);\s*$',
    re.MULTILINE
)

# Also single line format:
pattern_to_remove_single = re.compile(
    r'^[ \t]*if\s*\([^)]*\)\s*throw\s+new\s+UnauthorizedAccessException\("TENANT_SCOPE_MISMATCH"\);\s*$',
    re.MULTILINE
)

for file_path in files_to_clean:
    full_path = os.path.join(base_dir, file_path)
    if not os.path.exists(full_path):
        continue
        
    with open(full_path, 'r', encoding='utf-8') as f:
        content = f.read()
        
    # We MUST NOT remove it if it's inside a Restore handler because Restore uses IgnoreQueryFilters()
    # Or we can just remove them all and manually re-add them for Restore handlers, 
    # but the safer way is to split by methods.
    # Actually, we can use a custom replace function
    
    def replacer(match):
        text = match.group(0)
        return ""
    
    new_content = pattern_to_remove.sub("", content)
    new_content = pattern_to_remove_single.sub("", new_content)
    
    if new_content != content:
        with open(full_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Cleaned {file_path}")
